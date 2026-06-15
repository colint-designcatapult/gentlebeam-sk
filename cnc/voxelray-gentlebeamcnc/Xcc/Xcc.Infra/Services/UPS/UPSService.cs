using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xcc.Core.Domain.UPS;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using Xcc.Core.Models;
using Xcc.Core.Services;
using Xcc.Infra.UPS;

namespace Xcc.Infra.Services.UPS
{
    public class UpsService(
        IAppGlobals appGlobals,
        IUpsSettings upsSettings,
        ILogWriter logWriter)
        : IUpsService
    {
        public const int RetryDelayMs = 2000;
        public static readonly int MinUpsCount = 1;

        public event EventHandler<UpsTelemetryUpdatedArgs>? UpsTelemetryUpdated;

        public IList<string> QueryUpsList() //TODO: binding to service screen
        {
            // TODO: maybe we need to move/abstract this GetConnectedDevices,
            // to decrease dependency on HidDevice class
            HidDevice.InterfaceDetails[] devices = HidDevice.GetConnectedDevices();

            var upsDevicePathList = devices
                .Where(device => (device.VID == upsSettings.UpsHidVendorId &&
                        device.PID == upsSettings.UpsHidProductId))
                .Select(device => device.devicePath)
                .ToList();

#if DEBUG
            foreach (var path in upsDevicePathList)
                Debug.WriteLine($"UPS device path: {path}");
#endif
            return upsDevicePathList;
        }

        public void Start()
        {
            _ = Task.Run(async () =>
            {
                while (!appGlobals.AppCancellationTokenSource.Token.IsCancellationRequested)
                {

                    try
                    {
                        var upsDevicePathList = QueryUpsList();

                        if (upsDevicePathList.Count < MinUpsCount)
                        {
                            // Make a pause before trying to query device list again
                            await Task.Delay(RetryDelayMs, appGlobals.AppCancellationTokenSource.Token);
                        }
                        else
                        {
                            await RunUpsPolling(upsDevicePathList);
                        }
                    }
                    catch (Exception)
                    {
                        // Some error is happen during device enumeration/setup.
                        // Let's make a pause before trying again.
                        await Task.Delay(RetryDelayMs, appGlobals.AppCancellationTokenSource.Token);
                    }
                }
            });
        }

        private async Task RunUpsPolling(IList<string> upsDevicePathList)
        {
            var linkedTokenSource =
                CancellationTokenSource.CreateLinkedTokenSource(appGlobals.AppCancellationTokenSource.Token);

            var tasks = StartUpsPolling(upsDevicePathList, linkedTokenSource.Token);

            if (tasks.Count > 0)
            {
                await Task.WhenAny(tasks);
            }

            // Stop polling when any of the polling tasks was cancelled/finished:
            linkedTokenSource.Cancel();
        }

        private List<Task> StartUpsPolling(IList<string> upsDevicePathList,
            CancellationToken token)
        {
            var tasks = new List<Task>();
            foreach(var devicePath in upsDevicePathList)
            {
                HidDevice hidDevice = new();
                hidDevice.Initialize(devicePath, false);

                UpsDevice upsDevice = new(hidDevice);
                var unitIdData = GetUnitIdData(upsDevice);
                // Now we use primary UPS only, but there may be secondary UPS as well
                if (unitIdData.Model == upsSettings.PrimaryUpsModel)
                {
                    // Pass the device to the telemetry polling task
                    tasks.Add(
                        UpsTelemetryPolling.Poll(upsDevice, 
                                OnPrimaryUpsTelemetryUpdated,
                            upsDevice => OnUpsDisconnected(UpsType.Primary, upsDevice),
                            token));
                }
                else
                {
                    _ = logWriter.LogAsync(
                        $"Unknown UPS device: path={devicePath}; model={unitIdData.Model}; serial={unitIdData.Serial}", 
                        LogRecordSeverity.Warn, LogRecordType.System);
                    upsDevice.Close(); // we need to explicitely close the device if it doesn't fit for polling
                }
            }

            return tasks;
        }

        private void OnUpsDisconnected(UpsType upsType, UpsDevice device)
        {
            _ = logWriter.LogAsync(
                                    $"{upsType} UPS device is disconnected",
                                    LogRecordSeverity.Warn, LogRecordType.System);
        }

        private void OnPrimaryUpsTelemetryUpdated(IUpsTelemetry telemetry)
        {
            UpsTelemetryUpdated?.Invoke(this, new UpsTelemetryUpdatedArgs(UpsType.Primary, telemetry));
        }

        private UpsTelemetry.UnitIdData GetUnitIdData(UpsDevice device)
        {
            string[] unitId = new UpsTelemetryQuery(device).QueryUidData(token: null);

            return UpsTelemetry.ParseUnitId(unitId);
        }
    }

    public static class UpsTelemetryPolling
    {
        const int DefaultPollTimeoutMs = 2000;
        const int MaxPollDelayMs = 5000;
        public static async Task Poll(
            UpsDevice device,
            Action<IUpsTelemetry> callback,
            Action<UpsDevice>? onDisconnectedCallback,
            CancellationToken token,
            int timeoutMs = DefaultPollTimeoutMs)
        {
            var query = new UpsTelemetryQuery(device);
            while (!token.IsCancellationRequested)
            {
                try
                {                    
                    await Task.Delay(timeoutMs, token); // it takes 465ms to perform all queries)

                    // Set a watchdog to cancel telemetry query if it takes too long:
                    var localTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(MaxPollDelayMs, localTokenSource.Token);
                        localTokenSource.Cancel();
                    }, localTokenSource.Token);

                    // Now try getting the telemetry itself:
                    var upsTelemetry = query.QueryTelemetry(localTokenSource.Token);
                    callback?.Invoke(upsTelemetry);

                    // Cancel the watchdog if it's still running
                    localTokenSource.Cancel();
                }
                catch (Exception)
                {
                    callback?.Invoke(new UpsTelemetry());

                    if (!token.IsCancellationRequested)
                    {
                        if (device.IsConnected() == false)
                        {
                            onDisconnectedCallback?.Invoke(device);
                            break;
                        }
                    }
                }
            }
            device.Close();
        }
    }
}
