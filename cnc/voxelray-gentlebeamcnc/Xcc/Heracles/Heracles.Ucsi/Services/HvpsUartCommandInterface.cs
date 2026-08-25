using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xcc.Core.Enums;
using Xcc.Core.Logging;

namespace Heracles.Ucsi.Services
{
    /// <summary>
    /// Implementation of IUcsiHvpsUartCommandInterface using .NET's built-in SerialPort.
    /// Communicates directly with HVPS Interface Board via FTDI UART.
    /// Protocol: ASCII-based with * prefix and \n terminator.
    /// Baud rate: 38400, Data bits: 8, Parity: None, Stop bits: 1
    /// UCSI-specific implementation for direct HVPS configuration access.
    /// Implements IAsyncDisposable to ensure proper cleanup of serial port resources.
    /// 
    /// ARCHITECTURE: Single-threaded message pump
    /// All serial I/O happens on one background thread (_messageLoopTask).
    /// Commands are queued via _commandQueue and processed sequentially.
    /// This eliminates race conditions between log reader and command handlers.
    /// </summary>
    public class UcsiHvpsUartCommandInterface : IUcsiHvpsUartCommandInterface, IAsyncDisposable
    {
        /// <summary>
        /// Command object queued for execution on the message loop thread.
        /// Uses TaskCompletionSource to allow async callers to await results.
        /// </summary>
        private abstract class QueuedCommand
        {
            public abstract Task ExecuteAsync(SerialPort serialPort, ILogWriter logWriter);
        }

        private class RequestSystemConfigCommand : QueuedCommand
        {
            public TaskCompletionSource<SystemConfigResponse> CompletionSource { get; } = new();

            public override async Task ExecuteAsync(SerialPort serialPort, ILogWriter logWriter)
            {
                try
                {
                    // Clear any pending data in the input buffer (critical to prevent corruption from log reader)
                    serialPort.DiscardInBuffer();
                    await Task.Delay(10);  // Small delay to let buffers settle

                    // Send: *ACFGS\n (protocol uses single LF, not CRLF)
                    byte[] commandBytes = System.Text.Encoding.ASCII.GetBytes("*ACFGS\n");
                    serialPort.Write(commandBytes, 0, commandBytes.Length);
                    
                    logWriter.Log(
                        $"HVPS UART: Sent ACFGS command ({commandBytes.Length} bytes)",
                        LogRecordSeverity.Info,
                        LogRecordType.System);

                    // Firmware response format: *ACFGS + 32 binary floats (128 bytes) + \n
                    // = 6 bytes header + 128 bytes binary + 1 byte terminator = 135 bytes total
                    
                    byte[] responseBuffer = new byte[135];
                    int totalRead = 0;
                    
                    // Read all 135 bytes asynchronously
                    using (var cts = new CancellationTokenSource(TimeoutMs))
                    {
                        while (totalRead < 135)
                        {
                            try
                            {
                                int bytesRead = await serialPort.BaseStream.ReadAsync(responseBuffer, totalRead, 135 - totalRead, cts.Token);
                                if (bytesRead == 0)
                                {
                                    logWriter.Log(
                                        $"HVPS UART: Connection closed or no data received at position {totalRead}/135",
                                        LogRecordSeverity.Error,
                                        LogRecordType.System);
                                    throw new InvalidOperationException("Connection closed or no data received");
                                }
                                totalRead += bytesRead;
                            }
                            catch (OperationCanceledException)
                            {
                                logWriter.Log(
                                    $"HVPS UART: Timeout reading response - got {totalRead}/135 bytes, resetting port",
                                    LogRecordSeverity.Error,
                                    LogRecordType.System);
                                throw new InvalidOperationException($"Timeout reading response - got {totalRead}/135 bytes");
                            }
                        }
                    }

                    // Validate header
                    string header = System.Text.Encoding.ASCII.GetString(responseBuffer, 0, 6);
                    if (header != "*ACFGS")
                    {
                        logWriter.Log(
                            $"HVPS UART: Invalid ACFGS response header: '{header}' (expected '*ACFGS')",
                            LogRecordSeverity.Error,
                            LogRecordType.System);
                        throw new InvalidOperationException($"Invalid ACFGS response header: '{header}'");
                    }

                    // Validate terminator
                    if (responseBuffer[134] != '\n')
                    {
                        logWriter.Log(
                            $"HVPS UART: Invalid ACFGS terminator: 0x{responseBuffer[134]:X2} (expected 0x0A)",
                            LogRecordSeverity.Error,
                            LogRecordType.System);
                        throw new InvalidOperationException($"Invalid ACFGS terminator");
                    }

                    // Parse binary payload: 32 little-endian floats from bytes 6-133
                    float[] values = new float[32];
                    for (int i = 0; i < 32; i++)
                    {
                        values[i] = BitConverter.ToSingle(responseBuffer, 6 + (i * 4));
                    }

                    logWriter.Log(
                        "HVPS UART: System config retrieved successfully (32 values)",
                        LogRecordSeverity.Info,
                        LogRecordType.System);

                    CompletionSource.SetResult(new SystemConfigResponse(values));
                }
                catch (Exception ex)
                {
                    logWriter.Log(
                        $"HVPS UART: Failed to request system config: {ex.Message}",
                        LogRecordSeverity.Error,
                        LogRecordType.System);
                    CompletionSource.SetException(new InvalidOperationException($"Failed to request system config: {ex.Message}", ex));
                }

                await Task.CompletedTask;  // Return completed task
            }
        }

        private class SetSystemConfigValueCommand : QueuedCommand
        {
            public int Index { get; set; }
            public float Value { get; set; }
            public TaskCompletionSource<bool> CompletionSource { get; } = new();

            public override async Task ExecuteAsync(SerialPort serialPort, ILogWriter logWriter)
            {
                try
                {
                    // Clear any pending data in the input buffer (critical to prevent corruption from log reader)
                    serialPort.DiscardInBuffer();
                    await Task.Delay(10);  // Small delay to let buffers settle

                    // Frame Type 3: CONFIG_SET command (binary-packed, 15 bytes total)
                    byte[] commandFrame = new byte[15];
                    int pos = 0;

                    // Byte 0: asterisk
                    commandFrame[pos++] = (byte)'*';

                    // Bytes 1-5: "..SET"
                    commandFrame[pos++] = (byte)'.';
                    commandFrame[pos++] = (byte)'.';
                    commandFrame[pos++] = (byte)'S';
                    commandFrame[pos++] = (byte)'E';
                    commandFrame[pos++] = (byte)'T';

                    // Bytes 6-7: Type field (uint16_t, little-endian) = 0 for system config
                    commandFrame[pos++] = 0x00;
                    commandFrame[pos++] = 0x00;

                    // Bytes 8-9: ID field (uint16_t, little-endian) = index
                    byte[] indexBytes = BitConverter.GetBytes((ushort)Index);
                    commandFrame[pos++] = indexBytes[0];
                    commandFrame[pos++] = indexBytes[1];

                    // Bytes 10-13: Data field (float, little-endian) = value
                    byte[] valueBytes = BitConverter.GetBytes(Value);
                    commandFrame[pos++] = valueBytes[0];
                    commandFrame[pos++] = valueBytes[1];
                    commandFrame[pos++] = valueBytes[2];
                    commandFrame[pos++] = valueBytes[3];

                    // Byte 14: newline terminator
                    commandFrame[pos++] = (byte)'\n';

                    serialPort.Write(commandFrame, 0, commandFrame.Length);
                    
                    logWriter.Log(
                        $"HVPS UART: Sent CONFIG_SET command (15 bytes, index={Index}, value={Value})",
                        LogRecordSeverity.Info,
                        LogRecordType.System);

                    // Read response up to 20 bytes looking for terminator
                    byte[] responseBuffer = new byte[20];
                    int totalRead = 0;
                    
                    using (var cts = new CancellationTokenSource(TimeoutMs))
                    {
                        while (totalRead < 20)
                        {
                            try
                            {
                                int bytesRead = await serialPort.BaseStream.ReadAsync(responseBuffer, totalRead, 20 - totalRead, cts.Token);
                                if (bytesRead == 0)
                                {
                                    logWriter.Log(
                                        $"HVPS UART: Connection closed while reading CONFIG_SET response at position {totalRead}",
                                        LogRecordSeverity.Error,
                                        LogRecordType.System);
                                    throw new InvalidOperationException("Connection closed or no data received");
                                }
                                totalRead += bytesRead;
                                
                                // Check for terminator ONLY after reading at least 6 bytes
                                if (totalRead >= 6 && responseBuffer[totalRead - 1] == '\n')
                                {
                                    break;  // Got complete response
                                }
                            }
                            catch (OperationCanceledException)
                            {
                                logWriter.Log(
                                    $"HVPS UART: Timeout reading CONFIG_SET response - got {totalRead} bytes",
                                    LogRecordSeverity.Error,
                                    LogRecordType.System);
                                throw new InvalidOperationException($"Timeout reading CONFIG_SET response - got {totalRead} bytes");
                            }
                        }
                    }

                    // Validate response header
                    if (totalRead == 0 || responseBuffer[0] != '*')
                    {
                        logWriter.Log(
                            $"HVPS UART: Invalid CONFIG_SET response - wrong header byte: 0x{(totalRead > 0 ? responseBuffer[0] : 0):X2}",
                            LogRecordSeverity.Error,
                            LogRecordType.System);
                        throw new InvalidOperationException($"Invalid CONFIG_SET response header");
                    }

                    // Validate response terminator (should be at totalRead-1)
                    if (responseBuffer[totalRead - 1] != '\n')
                    {
                        logWriter.Log(
                            $"HVPS UART: Invalid CONFIG_SET response - wrong terminator: 0x{responseBuffer[totalRead - 1]:X2}",
                            LogRecordSeverity.Error,
                            LogRecordType.System);
                        throw new InvalidOperationException($"Invalid CONFIG_SET response terminator");
                    }

                    logWriter.Log(
                        $"HVPS UART: CONFIG_SET response received ({totalRead} bytes)",
                        LogRecordSeverity.Info,
                        LogRecordType.System);

                    // Clear any remaining data in FTDI buffer after CONFIG_SET response
                    serialPort.DiscardInBuffer();

                    logWriter.Log(
                        $"HVPS UART: Config value set successfully - Index {Index} = {Value}",
                        LogRecordSeverity.Info,
                        LogRecordType.System);

                    CompletionSource.SetResult(true);
                }
                catch (Exception ex)
                {
                    logWriter.Log(
                        $"HVPS UART: Failed to set config value at index {Index}: {ex.Message}",
                        LogRecordSeverity.Error,
                        LogRecordType.System);
                    CompletionSource.SetException(new InvalidOperationException($"Failed to set config value at index {Index}: {ex.Message}", ex));
                }

                await Task.CompletedTask;  // Return completed task
            }
        }

        private SerialPort? _serialPort;
        private readonly string _portName;
        private readonly ILogWriter _logWriter;
        private Task? _initializationTask;
        private readonly object _initializationLock = new object();
        private const int BaudRate = 38400;
        private const int DataBits = 8;
        private const Parity ParityBits = Parity.None;
        private const StopBits StopBitsCount = StopBits.One;
        private const int TimeoutMs = 2000;  // 2000ms timeout for read operations (aligns with UI button greying)
        private const int InitializationDelayMs = 500;  // 500ms delay for firmware to stabilize after port open
        private bool _disposed = false;
        
        // Single-threaded message pump for all serial I/O
        private Task? _messageLoopTask;
        private CancellationTokenSource? _messageLoopCancellation;
        private readonly Queue<QueuedCommand> _commandQueue = new Queue<QueuedCommand>();
        private readonly object _commandQueueLock = new object();
        private bool _isExecutingCommand = false;  // Flag to disable log reading during command execution
        
        // Exponential backoff reconnection timing (for when port closes unexpectedly)
        private int _reconnectAttempt = 0;  // 0 = not reconnecting, 1+ = attempt number
        private DateTime _nextReconnectTime = DateTime.MinValue;  // When to next attempt reconnection
        private static readonly int[] ReconnectDelaysMs = { 500, 1000, 2000, 3000, 5000, 10000 };  // Delays: immediate, 500ms, 1s, 2s, 3s, 5s, then repeat 10s
        
        // Line buffer for reading and logging HVPS text messages
        private readonly StringBuilder _logLineBuffer = new StringBuilder();
        private byte[] _readBuffer = new byte[256];

        // Connection state tracking for event notifications
        private bool _lastReportedConnectionState = false;  // Tracks what we last reported to subscribers
        public event EventHandler<ConnectionStateChangedEventArgs>? ConnectionStateChanged;

        /// <summary>
        /// Initialize UCSI HVPS UART command interface.
        /// Configure the COM port in appsettings.json under "Ucsi:Hardware:HvpsUartPort"
        /// (e.g., "COM3" for USART3 on the HVPS Interface Board).
        /// </summary>
        /// <param name="portName">Serial port name (e.g., "COM1", "COM3"). Must be configured in appsettings.json.</param>
        /// <param name="logWriter">Logger for connection diagnostics.</param>
        public UcsiHvpsUartCommandInterface(string portName, ILogWriter logWriter)
        {
            _portName = portName;
            _logWriter = logWriter;
        }

        public async Task InitializeAsync()
        {
            Task? taskToAwait;
            lock (_initializationLock)
            {
                // Detect unexpected connection loss (e.g., board power cycle)
                // If port was previously opened but is now closed, clear the cached task to retry
                if (_serialPort?.IsOpen == false)
                {
                    _logWriter.Log(
                        "HVPS UART: Detected connection loss - port was closed unexpectedly. Preparing to reconnect.",
                        LogRecordSeverity.Warn,
                        LogRecordType.System);
                    _initializationTask = null;
                }

                // If initialization task already exists, just await it
                if (_initializationTask != null)
                {
                    taskToAwait = _initializationTask;
                }
                else
                {
                    // Create the initialization task and store it
                    _initializationTask = PerformInitializationAsync();
                    taskToAwait = _initializationTask;
                }
            }

            // Always await - ensures all concurrent callers wait for Task.Delay(500) to complete
            await taskToAwait;
        }

        /// <summary>
        /// Internal method used by the message loop for automatic reconnection.
        /// Must be called from the message loop thread only.
        /// Does not interfere with the public InitializeAsync() caching mechanism.
        /// </summary>
        private async Task<bool> AttemptReconnectAsync()
        {
            // Only one reconnection attempt at a time across all threads
            lock (_initializationLock)
            {
                // If someone else started initialization while we were waiting, don't duplicate it
                if (_serialPort?.IsOpen == true)
                {
                    return true;  // Port is already open
                }
            }

            try
            {
                // Dispose old port and create new one
                try
                {
                    if (_serialPort != null)
                    {
                        if (_serialPort.IsOpen)
                        {
                            _serialPort.Close();
                        }
                        _serialPort.Dispose();
                        _serialPort = null;
                    }
                }
                catch (Exception ex)
                {
                    _logWriter.Log(
                        $"HVPS UART: Warning while disposing old port during reconnection: {ex.Message}",
                        LogRecordSeverity.Warn,
                        LogRecordType.System);
                }

                _logWriter.Log(
                    $"HVPS UART: Opening connection on port {_portName} at {BaudRate} baud",
                    LogRecordSeverity.Info,
                    LogRecordType.System);

                _serialPort = new SerialPort(_portName, BaudRate, ParityBits, DataBits, StopBitsCount)
                {
                    ReadTimeout = TimeoutMs,
                    WriteTimeout = TimeoutMs,
                    DtrEnable = true,
                    RtsEnable = true
                };
                _serialPort.Open();
                await Task.Delay(InitializationDelayMs);

                _logWriter.Log(
                    $"HVPS UART: Connection established on {_portName} (38400 baud, 8-N-1, timeout {TimeoutMs}ms)",
                    LogRecordSeverity.Info,
                    LogRecordType.System);

                return true;
            }
            catch (Exception ex)
            {
                _logWriter.Log(
                    $"HVPS UART: Failed to open port during reconnection: {ex.Message}",
                    LogRecordSeverity.Warn,
                    LogRecordType.System);
                return false;
            }
        }

        private async Task PerformInitializationAsync()
        {
            try
            {
                // Properly dispose the old port before attempting to open a new one
                // This is critical when reconnecting after unexpected closure
                try
                {
                    if (_serialPort != null)
                    {
                        if (_serialPort.IsOpen)
                        {
                            _serialPort.Close();
                        }
                        _serialPort.Dispose();
                        _serialPort = null;
                    }
                }
                catch (Exception ex)
                {
                    _logWriter.Log(
                        $"HVPS UART: Warning while disposing old port: {ex.Message}",
                        LogRecordSeverity.Warn,
                        LogRecordType.System);
                }

                _logWriter.Log(
                    $"HVPS UART: Opening connection on port {_portName} at {BaudRate} baud",
                    LogRecordSeverity.Info,
                    LogRecordType.System);

                _serialPort = new SerialPort(_portName, BaudRate, ParityBits, DataBits, StopBitsCount)
                {
                    ReadTimeout = TimeoutMs,
                    WriteTimeout = TimeoutMs,
                    DtrEnable = true,  // Enable DTR - controls MCU reset/boot via FTDI chip
                    RtsEnable = true   // Enable RTS - hardware flow control/reset signal
                };
                _serialPort.Open();
                await Task.Delay(InitializationDelayMs);  // Wait for firmware to stabilize
                
                _logWriter.Log(
                    $"HVPS UART: Connection established on {_portName} (38400 baud, 8-N-1, timeout {TimeoutMs}ms)",
                    LogRecordSeverity.Info,
                    LogRecordType.System);

                // IMPORTANT: Only start the message loop on initial connection, not on reconnection
                // The message loop task persists and handles reconnection internally
                if (_messageLoopTask == null || _messageLoopTask.IsCompleted)
                {
                    StartMessageLoop();
                }
            }
            catch (Exception ex)
            {
                _logWriter.Log(
                    $"HVPS UART: Failed to open port {_portName}: {ex.Message}",
                    LogRecordSeverity.Error,
                    LogRecordType.System);
                
                // Don't clear initialization task here - let the message loop handle reconnection
                // This prevents race conditions where multiple callers try to initialize simultaneously
            }
        }

        /// <summary>
        /// Start the single-threaded message loop that handles all serial I/O.
        /// This runs as a background task and processes commands from the queue,
        /// with log reading during idle periods.
        /// </summary>
        private void StartMessageLoop()
        {
            try
            {
                // Cancel any existing message loop
                _messageLoopCancellation?.Cancel();
                _messageLoopCancellation?.Dispose();

                // Create new cancellation token for this loop
                _messageLoopCancellation = new CancellationTokenSource();

                // Fire-and-forget background task for the message loop
                _messageLoopTask = MessageLoopAsync(_messageLoopCancellation.Token);
            }
            catch (Exception ex)
            {
                _logWriter.Log(
                    $"HVPS UART: Failed to start message loop: {ex.Message}",
                    LogRecordSeverity.Error,
                    LogRecordType.System);
            }
        }

        /// <summary>
        /// Single-threaded message loop that owns all serial port I/O.
        /// Processes queued commands synchronously, and reads/logs text during idle periods.
        /// Implements automatic reconnection with exponential backoff when port closes unexpectedly.
        /// This architecture eliminates race conditions between commands and log reader.
        /// </summary>
        private async Task MessageLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    // Check if connection state has changed and notify subscribers
                    CheckAndRaiseConnectionStateChanged();

                    // If port is closed and we're not already in reconnection mode, start reconnection
                    if (_serialPort?.IsOpen == false && _reconnectAttempt == 0)
                    {
                        _reconnectAttempt = 1;
                        _nextReconnectTime = DateTime.UtcNow;
                        _logWriter.Log(
                            "HVPS UART: Connection lost - starting automatic reconnection with exponential backoff",
                            LogRecordSeverity.Warn,
                            LogRecordType.System);
                    }

                    // If we're in reconnection mode (port is closed)
                    if (_serialPort?.IsOpen == false && _reconnectAttempt > 0)
                    {
                        // Check if it's time to attempt reconnection
                        if (DateTime.UtcNow >= _nextReconnectTime)
                        {
                            // Calculate backoff delay for this attempt
                            int backoffDelayMs = ReconnectDelaysMs[Math.Min(_reconnectAttempt - 1, ReconnectDelaysMs.Length - 1)];
                            
                            _logWriter.Log(
                                $"HVPS UART: Attempting reconnection (attempt {_reconnectAttempt}, waited {backoffDelayMs}ms)",
                                LogRecordSeverity.Info,
                                LogRecordType.System);

                            try
                            {
                                // Attempt to reconnect using internal method
                                bool success = await AttemptReconnectAsync();
                                
                                if (success && _serialPort?.IsOpen == true)
                                {
                                    // Reconnection successful!
                                    _logWriter.Log(
                                        $"HVPS UART: Reconnection successful after {_reconnectAttempt} attempts",
                                        LogRecordSeverity.Info,
                                        LogRecordType.System);
                                    _reconnectAttempt = 0;  // Reset to normal operation mode
                                    continue;  // Resume normal message loop operation
                                }
                            }
                            catch (Exception ex)
                            {
                                _logWriter.Log(
                                    $"HVPS UART: Reconnection attempt {_reconnectAttempt} failed: {ex.Message}",
                                    LogRecordSeverity.Warn,
                                    LogRecordType.System);
                            }

                            // Prepare for next reconnection attempt
                            _reconnectAttempt++;
                            int nextDelayMs = ReconnectDelaysMs[Math.Min(_reconnectAttempt - 1, ReconnectDelaysMs.Length - 1)];
                            _nextReconnectTime = DateTime.UtcNow.AddMilliseconds(nextDelayMs);
                            
                            _logWriter.Log(
                                $"HVPS UART: Next reconnection attempt in {nextDelayMs}ms",
                                LogRecordSeverity.Info,
                                LogRecordType.System);
                        }
                        else
                        {
                            // Not yet time to reconnect - wait a bit before checking again
                            int waitMs = Math.Min(100, (int)(_nextReconnectTime - DateTime.UtcNow).TotalMilliseconds);
                            try
                            {
                                await Task.Delay(Math.Max(1, waitMs), cancellationToken);
                            }
                            catch (OperationCanceledException)
                            {
                                // Normal cancellation during shutdown
                            }
                        }
                        continue;
                    }

                    // Normal operation: port is open
                    if (_serialPort?.IsOpen == true)
                    {
                        QueuedCommand? command = null;
                        lock (_commandQueueLock)
                        {
                            if (_commandQueue.Count > 0)
                            {
                                command = _commandQueue.Dequeue();
                            }
                        }

                        if (command != null)
                        {
                            // Execute the command (includes all its I/O)
                            try
                            {
                                _isExecutingCommand = true;  // Disable log reading during command execution
                                await command.ExecuteAsync(_serialPort, _logWriter);
                            }
                            catch (Exception ex)
                            {
                                _logWriter.Log(
                                    $"HVPS UART: Error executing command: {ex.Message}",
                                    LogRecordSeverity.Error,
                                    LogRecordType.System);
                            }
                            finally
                            {
                                _isExecutingCommand = false;  // Re-enable log reading
                            }
                        }
                        else
                        {
                            // No command queued - try to read and log HVPS text
                            await ReadAndLogTextAsync(_serialPort, cancellationToken);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Normal cancellation during shutdown
                _logWriter.Log(
                    "HVPS UART: Message loop cancelled",
                    LogRecordSeverity.Info,
                    LogRecordType.System);
            }
            catch (Exception ex)
            {
                _logWriter.Log(
                    $"HVPS UART: Message loop failed: {ex.Message}",
                    LogRecordSeverity.Error,
                    LogRecordType.System);
            }
        }

        /// <summary>
        /// Attempt to read and log one line of HVPS text.
        /// Uses non-blocking checks to avoid starving the command queue.
        /// DISABLED during command execution to prevent consuming response bytes.
        /// </summary>
        private async Task ReadAndLogTextAsync(SerialPort serialPort, CancellationToken cancellationToken)
        {
            // Skip log reading if a command is executing (prevents consuming binary response bytes)
            if (_isExecutingCommand)
            {
                try
                {
                    await Task.Delay(1, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // Normal cancellation, don't treat as error
                }
                return;
            }
            
            try
            {
                // Non-blocking: only attempt read if data is available
                int bytesAvailable = serialPort.BytesToRead;
                
                if (bytesAvailable > 0)
                {
                    // Data is available - read it with no wait
                    byte[] buffer = new byte[Math.Min(bytesAvailable, 256)];
                    int bytesRead = serialPort.Read(buffer, 0, buffer.Length);
                    
                    if (bytesRead > 0)
                    {
                        // Decode bytes to string and process character by character
                        string data = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                        
                        foreach (char c in data)
                        {
                            if (c == '\n')
                            {
                                // Complete line received - log it
                                string line = _logLineBuffer.ToString().TrimEnd('\r');
                                _logLineBuffer.Clear();

                                if (!string.IsNullOrWhiteSpace(line))
                                {
                                    _logWriter.Log(
                                        $"HVPS: {line}",
                                        LogRecordSeverity.Info,
                                        LogRecordType.System);
                                }
                            }
                            else if (c != '\r')  // Ignore carriage returns
                            {
                                _logLineBuffer.Append(c);
                            }
                        }
                    }
                }
                else
                {
                    // No data available - yield to allow other operations
                    await Task.Delay(1, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Normal cancellation - don't treat as an error
                // This happens when the message loop is being cancelled or reconnecting
            }
            catch (Exception ex)
            {
                _logWriter.Log(
                    $"HVPS UART: Error reading log text: {ex.Message}",
                    LogRecordSeverity.Warn,
                    LogRecordType.System);
                
                // Reset port on read errors to allow reconnection
                await ResetPortAsync();
            }
        }

        public async Task CloseAsync()
        {
            // Stop the message loop
            StopMessageLoop();

            // Reset reconnection state (intentional close, don't auto-reconnect)
            _reconnectAttempt = 0;

            if (_serialPort?.IsOpen ?? false)
            {
                try
                {
                    _logWriter.Log(
                        $"HVPS UART: Closing connection on {_portName}",
                        LogRecordSeverity.Info,
                        LogRecordType.System);

                    _serialPort.Close();
                    _serialPort.Dispose();
                    
                    _logWriter.Log(
                        "HVPS UART: Connection closed",
                        LogRecordSeverity.Info,
                        LogRecordType.System);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Warning: Error closing serial port: {ex.Message}");
                }
                _serialPort = null;
            }

            // Reset initialization task so future reconnect attempts can succeed
            lock (_initializationLock)
            {
                _initializationTask = null;
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Stop the message loop task.
        /// </summary>
        private void StopMessageLoop()
        {
            try
            {
                _messageLoopCancellation?.Cancel();
            }
            catch { }
        }

        /// <summary>
        /// Reset the serial port connection on fatal errors (timeout, corruption, etc.).
        /// This clears the input/output buffers and prepares for reconnection.
        /// Called from the message loop's log reader when a read error occurs.
        /// Does NOT clear initialization state - let the public InitializeAsync() handle that.
        /// </summary>
        private async Task ResetPortAsync()
        {
            try
            {
                // Just close and dispose the port, let the message loop handle reconnection
                if (_serialPort != null)
                {
                    try
                    {
                        if (_serialPort.IsOpen)
                        {
                            _serialPort.Close();
                        }
                        _serialPort.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logWriter.Log(
                            $"HVPS UART: Warning while disposing port during reset: {ex.Message}",
                            LogRecordSeverity.Warn,
                            LogRecordType.System);
                    }
                    _serialPort = null;
                }

                _logWriter.Log(
                    "HVPS UART: Port reset due to read error",
                    LogRecordSeverity.Warn,
                    LogRecordType.System);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Warning: Error during port reset: {ex.Message}");
            }
        }

        /// <summary>
        /// Finalizer to ensure cleanup if DisposeAsync() is not called.
        /// </summary>
        ~UcsiHvpsUartCommandInterface()
        {
            if (!_disposed && (_serialPort?.IsOpen ?? false))
            {
                try
                {
                    _serialPort?.Close();
                    _serialPort?.Dispose();
                }
                catch { }
            }
        }

        /// <summary>
        /// Dispose implementation for IAsyncDisposable.
        /// Ensures serial port resources are properly cleaned up.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                _disposed = true;  // Set flag BEFORE disposing resources to prevent new operations
                await CloseAsync();
                try
                {
                    _messageLoopCancellation?.Dispose();
                }
                catch (ObjectDisposedException)
                {
                    // Cancellation token may have been disposed
                }
                GC.SuppressFinalize(this);
            }
        }

        public bool IsConnected => _serialPort?.IsOpen ?? false;

        /// <summary>
        /// Notify subscribers if connection state has changed.
        /// Must be called from the message loop thread to avoid concurrent modifications.
        /// </summary>
        private void CheckAndRaiseConnectionStateChanged()
        {
            bool currentState = IsConnected;
            if (currentState != _lastReportedConnectionState)
            {
                _lastReportedConnectionState = currentState;
                try
                {
                    ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(currentState));
                }
                catch (Exception ex)
                {
                    // Prevent exceptions in event handlers from crashing the message loop
                    _logWriter.Log(
                        $"HVPS UART: Error in ConnectionStateChanged event handler: {ex.Message}",
                        LogRecordSeverity.Warn,
                        LogRecordType.System);
                }
            }
        }

        public async Task<SystemConfigResponse> RequestSystemConfig()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(UcsiHvpsUartCommandInterface));
            }

            // Ensure initialization is complete before proceeding
            await InitializeAsync();

            if (_serialPort?.IsOpen != true)
            {
                _logWriter.Log(
                    "HVPS UART: Cannot request system config - serial port not open",
                    LogRecordSeverity.Error,
                    LogRecordType.System);
                throw new InvalidOperationException("Serial port not open. Call InitializeAsync() first.");
            }

            // Queue the command and await its completion
            var command = new RequestSystemConfigCommand();
            _logWriter.Log(
                "HVPS UART: Queueing RequestSystemConfig command",
                LogRecordSeverity.Info,
                LogRecordType.System);
            
            lock (_commandQueueLock)
            {
                _commandQueue.Enqueue(command);
                _logWriter.Log(
                    $"HVPS UART: Command queued (queue count: {_commandQueue.Count})",
                    LogRecordSeverity.Info,
                    LogRecordType.System);
            }

            // Wait for the message loop to execute and complete the command
            _logWriter.Log(
                "HVPS UART: Awaiting command completion",
                LogRecordSeverity.Info,
                LogRecordType.System);
            
            return await command.CompletionSource.Task;
        }

        public async Task SetSystemConfigValue(int index, float value)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(UcsiHvpsUartCommandInterface));
            }

            // Validate input
            if (index < 0 || index > 31)
            {
                _logWriter.Log(
                    $"HVPS UART: Invalid config index {index} (must be 0-31)",
                    LogRecordSeverity.Error,
                    LogRecordType.System);
                throw new ArgumentOutOfRangeException(nameof(index), "Index must be 0-31");
            }

            // Ensure initialization is complete before proceeding
            await InitializeAsync();

            if (_serialPort?.IsOpen != true)
            {
                _logWriter.Log(
                    "HVPS UART: Cannot set config value - serial port not open",
                    LogRecordSeverity.Error,
                    LogRecordType.System);
                throw new InvalidOperationException("Serial port not open. Call InitializeAsync() first.");
            }

            // Queue the command and await its completion
            var command = new SetSystemConfigValueCommand { Index = index, Value = value };
            lock (_commandQueueLock)
            {
                _commandQueue.Enqueue(command);
            }

            // Wait for the message loop to execute and complete the command
            await command.CompletionSource.Task;
        }
    }
}
