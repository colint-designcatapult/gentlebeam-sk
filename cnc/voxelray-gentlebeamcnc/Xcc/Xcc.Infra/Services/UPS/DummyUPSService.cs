using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xcc.Core.Domain.UPS;
using Xcc.Core.Models;
using Xcc.Core.Services;
using Xcc.Infra.UPS;

namespace Xcc.Infra.Services.UPS
{
    public class DummyUPSService(IUPSTelemetryStore upsTelemetryStore, IAppGlobals appGlobals, IDebugSettings debugSettings): IUpsService
    {
        public IUPSTelemetryStore UPSTelemetryStore { get; } = upsTelemetryStore;
        public IAppGlobals AppGlobals { get; } = appGlobals;

        public event EventHandler<UpsTelemetryUpdatedArgs>? UpsTelemetryUpdated;

        public void Start()
        {
            try
            {
                Task.Run(() => ObtainUPSTelemetry(AppGlobals.AppCancellationTokenSource.Token), AppGlobals.AppCancellationTokenSource.Token);

                //var cts = new CancellationTokenSource(TimeSpan.FromSeconds(25));
                //Task.Run(() => ObtainUPSTelemetry(cts.Token), cts.Token);
            }
            catch (Exception)
            {
            }
        }

        private void ObtainUPSTelemetry(CancellationToken token)
        {
            var primaryBatteryChargedPercent = 100;

            try
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var t = Task.Delay(2000, token);
                        t.Wait(token);

                        //switch (batteryDataTokens[0])
                        //{
                        //    case ("BAT=1"):
                        //        BatteryStatus = "unknown";
                        //        break;
                        //    case ("BAT=2"):
                        //        BatteryStatus = "normal";
                        //        break;
                        //    case ("BAT=3"):
                        //        BatteryStatus = "low";
                        //        break;
                        //    case ("BAT=4"):
                        //        BatteryStatus = "depleted";
                        //        break;
                        //}

                        //TimeOnBattery = batteryDataTokens[1];
                        //EstimatedBatRuntime = int.Parse(batteryDataTokens[2]);
                        //BatteryChargedPercent = double.Parse(batteryDataTokens[3]);
                        //BattaryVoltage = double.Parse(batteryDataTokens[4]);
                        //BattaryCurrent = double.Parse(batteryDataTokens[5]);
                        //Temperature = double.Parse(batteryDataTokens[6]);
                        //MaxTempRecorded = double.Parse(batteryDataTokens[7]);
                        //BattaryChargerCurrent = double.Parse(batteryDataTokens[8]);
                        //TotalMinutesON = double.Parse(batteryDataTokens[9]);
                        //UsedTimesCounter = int.Parse(batteryDataTokens[10]);
                        //DepletionCounter = int.Parse(batteryDataTokens[11]);

                        var rand = new Random();
                        var primaryEstimatedBatRuntime = (int)(primaryBatteryChargedPercent * (9 + rand.NextDouble() * 2));

                        string[] batteryDataTokens =
                        [
                            "2",
                            "10",
                            primaryEstimatedBatRuntime.ToString(),
                            primaryBatteryChargedPercent.ToString(),
                            "13,4",
                            "14,5",
                            "15,6",
                            "16,7",
                            "17,8",
                            "18,9",
                            "19",
                            "20"
                        ];
                        
                        // InputStatus = "PROPER RANGE";
                        //   InputFrequency = double.Parse(inputDataTokens[1]);
                        //   InputVoltage = double.Parse(inputDataTokens[2]);
                        //   InputCurrent = double.Parse(inputDataTokens[3]);
                        //   InputPower = double.Parse(inputDataTokens[4]);

                        var inputVoltage = Convert.ToInt32(rand.NextDouble() * 50.0 + 200.0);

                        string[] inputData =
                        [
                            "1",
                            "1,23",
                            inputVoltage.ToString(),
                            "60,4",
                            "220,5"
                        ];
                        //UpsTelemetry.RetreiveInputData(inputData);

                        string[] alarmsData = Enumerable.Repeat("0", 28).ToArray();
                        alarmsData[0] = "0";
                        alarmsData[1] = "0";
                        alarmsData[2] = debugSettings.IsUpsActivated ? "1" : "0";

                        OnPrimaryUpsTelemetryUpdated(UpsTelemetry.Parse(null, batteryDataTokens, alarmsData, inputData, null, null, null));
                        
                        primaryBatteryChargedPercent -= 10;
                        if (primaryBatteryChargedPercent <= 0)
                            primaryBatteryChargedPercent = 100;

                    }
                    catch (Exception)
                    {
                        OnPrimaryUpsTelemetryUpdated(null);
                    }
                }
            }
            catch (Exception)
            {
                OnPrimaryUpsTelemetryUpdated(null);
            }
        }

        private void OnPrimaryUpsTelemetryUpdated(IUpsTelemetry? upsTelemetry)
        {
            //UPSTelemetryStore.Primary = upsTelemetry;
            UpsTelemetryUpdated?.Invoke(this, new UpsTelemetryUpdatedArgs(UpsType.Primary, upsTelemetry));
        }
    }
}
