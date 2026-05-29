using Empyrean.Common.Infra.Events;

using Heracles.Application.Models;
using Heracles.Core.Enums;
using Heracles.Core.Models;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Heracles.Robot.Models;
using Heracles.Robot.Models.Enums;
using Xcc.Core.Logging;
using Xcc.Core.Models;
using Xcc.Infra.Services.GcbServices;

namespace Heracles.Robot.Services
{
    public class AcbCommunicationServiceNew : GcbBaseUdpService, IAcbCommunicationService
    {
        // TODO: old AcbCommunicationService waits for AcbPacketId.Actuators response type only
        // Should that be done here as well? Can this cause memory leak in old implementation?
        private static readonly int RECEIVE_TIMEOUT = 10000;

        public AcbCommunicationServiceNew(
            IAppGlobals appGlobals,
            IAcbSettings acbSettings,
            IAcbCommConnectionFactory connectionFactory)
            : base(
                appGlobals.AppCancellationTokenSource.Token,
                connectionFactory.GetAcbCommConnection(),
                acbSettings.AcbReceiveTimeout > 0 ? acbSettings.AcbReceiveTimeout : RECEIVE_TIMEOUT)
        {

        }

        public void StopListening()
        {
            Stop();
        }
        
        public async Task<bool> PingAsync()
        {
            var buffer = Encoding.ASCII.GetBytes("Ping");
            await SendMessageAsync(buffer);

            return true;
        }
        protected override bool IsValidMessage(byte[] bytes)
        {
            return AcbMessageConverter.IsValidMessage(bytes);
        }
    }


    [Obsolete]
    public class AcbCommunicationService : IAcbCommunicationService
    {
        private readonly int _receiveTimeoutMs = 10000;

        private readonly object _receivedMessagesLock = new object();
        private readonly IAppGlobals _appGlobals;
        private readonly ILogWriter _logWriter;
        private Dictionary<int, byte[]> _receivedMessages;
        private UdpClient _client;
        private string _hostName;
        private int _port;
        private bool _disposed = false;
        private bool _isListening = false;
        private object _lock = new object();

        private CancellationTokenSource _cancellationTokenSource;
        private CancellationTokenSource _linkedCancellationToken;

        public event EventHandler<UdpReceiveErrorEventArgs> UdpReceiveErrorEvent;

        private bool IsListening 
        {
            get
            {
                lock (_lock) 
                    return _isListening;
            }
            set
            {
                lock (_lock)
                    _isListening = value;
            }
        }

        public AcbCommunicationService(
            IAcbSettings acbSettings,
            IHeraclesCoreSettings heraclesCoreSettings,
            IAppGlobals appGlobals,
            ILogWriter logWriter)
        {
            _appGlobals = appGlobals;
            _logWriter = logWriter;
            _hostName = heraclesCoreSettings.AcbCommandsEndPoint.Ip();
            _port = heraclesCoreSettings.AcbCommandsEndPoint.Port ?? throw new System.Exception("ACB commands port is not specified.");

            if (acbSettings.AcbReceiveTimeout > 0)
                _receiveTimeoutMs = acbSettings.AcbReceiveTimeout;

            _receivedMessages = new Dictionary<int, byte[]>();
        }

        public void StopListening()
        {
            _cancellationTokenSource?.Cancel();

            Dispose();

            IsListening = false;
        }


        public async void Start()
        {
            try
            {
                if (IsListening)
                    return;

                _cancellationTokenSource = new CancellationTokenSource();
                _linkedCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(_appGlobals.AppCancellationTokenSource.Token, _cancellationTokenSource.Token);

                _client = new UdpClient(22);

                _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                IsListening = true;

                Task receiveTask = Task.Run(async () =>
                {
                    try
                    {
                        while (!_linkedCancellationToken.IsCancellationRequested)
                        {
                            try
                            {
                                var msg = await _client.ReceiveAsync(_linkedCancellationToken.Token);

                                if (msg.Buffer == null)
                                    continue;

                                if (IsValidMessage(msg.Buffer))
                                {
                                    lock (_receivedMessagesLock)
                                    {
                                        _receivedMessages[GetPacketId(msg.Buffer)] = msg.Buffer;
                                    }
                                }
                                await _logWriter.LogAsync($"AcbCommunicationService receive: {BitConverter.ToString(msg.Buffer)}", Xcc.Core.Enums.LogRecordSeverity.Info, Xcc.Core.Enums.LogRecordType.System);
                            }
                            catch (Exception ex)
                            {
                                await _logWriter.LogAsync($"AcbCommunicationService listening error: {ex.Message}", Xcc.Core.Enums.LogRecordSeverity.Error, Xcc.Core.Enums.LogRecordType.Error);
                            }
                        }
                    }
                    catch (OperationCanceledException ex)
                    {
                        await _logWriter.LogAsync($"AcbCommunicationService listening error: {ex.Message}", Xcc.Core.Enums.LogRecordSeverity.Error, Xcc.Core.Enums.LogRecordType.Error);
                    }
                    catch (Exception ex)
                    {
                        await _logWriter.LogAsync($"AcbCommunicationService listening error: {ex.Message}", Xcc.Core.Enums.LogRecordSeverity.Error, Xcc.Core.Enums.LogRecordType.Error);
                    }

                    IsListening = false;

                }, _linkedCancellationToken.Token);
            }
            catch(Exception ex)
            {
                await _logWriter.LogAsync($"AcbCommunicationService listening error: {ex.Message}", Xcc.Core.Enums.LogRecordSeverity.Error, Xcc.Core.Enums.LogRecordType.Error);
                IsListening = false;
            }
        }

        private bool IsValidMessage(byte[] bytes)
        {
            return AcbMessageConverter.IsValidMessage(bytes);
        }

        public async Task<byte[]> SendRequestAsync(byte[] buffer)
        {
            return await SendRequestAsync(buffer, _receiveTimeoutMs);
        }

        public async Task<byte[]> SendRequestAsync(byte[] buffer, int timeoutMs)
        {
            byte[] returnResult = null;

            if (_client != null)
            {
                try
                {
                    lock (_receivedMessagesLock)
                    {
                        _receivedMessages.Clear();
                    }

                    int bytes = await SendAsync(buffer);

                    if (bytes > 0)
                    {
                        returnResult = await GetResponse(GetPacketId(buffer), timeoutMs);
                        await _logWriter.LogAsync($"AcbCommunicationService send request: {BitConverter.ToString(buffer)}", Xcc.Core.Enums.LogRecordSeverity.Info, Xcc.Core.Enums.LogRecordType.System);
                    }
                }
                catch (Exception ex) when (ex is SocketException ||
                                           ex is OperationCanceledException ||
                                           ex is Exception)
                {
                    throw;
                }
            }

            return returnResult;
        }

        public async Task SendMessageAsync(byte[] buffer)
        {
            int bytes = await SendAsync(buffer);

            if (bytes > 0)
            {
                _ = _logWriter.LogAsync($"AcbCommunicationService send message: {BitConverter.ToString(buffer)}", Xcc.Core.Enums.LogRecordSeverity.Info, Xcc.Core.Enums.LogRecordType.System);
            }
        }

        private async Task<byte[]> GetResponse(int id, int timeoutMs)
        {
            byte[] result = null;

            if (!Enum.IsDefined(typeof(AcbPacketId), id))
                return result;
            
            AcbPacketId packetId = (AcbPacketId)id;
            if (packetId != AcbPacketId.Actuators)
                return result;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            while (stopwatch.ElapsedMilliseconds < _receiveTimeoutMs)
            {
                if (_receivedMessages.TryGetValue(id, out result))
                {
                    lock (_receivedMessagesLock)
                    {
                        _receivedMessages.Remove(id);
                    }

                    return result;
                }

                await Task.Delay(50);
            }

            return result;
        }

        private int GetPacketId(byte[] buffer)
        {
            return BitConverter.ToInt32(buffer.Skip(12).Take(4).ToArray(), 0);
        }

        public async Task<bool> PingAsync()
        {
            var buffer = Encoding.ASCII.GetBytes("Ping");
            var bytesSent = await SendAsync(buffer);

            return bytesSent > 0;
        }

        ~AcbCommunicationService()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                if (_cancellationTokenSource != null)
                {
                    _cancellationTokenSource.Cancel();
                    _cancellationTokenSource?.Dispose();
                }

                if (_client != null)
                {
                    _client.Close();
                    _client.Dispose();
                }

            }

            _disposed = true;
        }

        public void Stop()
        {
            StopListening();
        }

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private async Task<int> SendAsync(byte[] buffer)
        {
            await _semaphore.WaitAsync();
            int bytesSent = 0;
            try
            {
                bytesSent = await _client.SendAsync(buffer, buffer.Length, _hostName, _port);
            }
            finally
            {
                _semaphore.Release();
            }
            return bytesSent;
        }
    }
}
