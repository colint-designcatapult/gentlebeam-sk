using System;
using System.Threading.Tasks;

namespace Heracles.Ucsi.Services
{
    /// <summary>
    /// Interface for FTDI UART communication directly with HVPS Interface Board.
    /// Uses ASCII protocol: 38400 baud, 8-N-1
    /// Bypasses main GCB board (separate communication channel).
    /// UCSI-specific interface for direct HVPS configuration access.
    /// </summary>
    public record SystemConfigResponse(float[] Values);

    /// <summary>
    /// Event args for connection state changes
    /// </summary>
    public class ConnectionStateChangedEventArgs : EventArgs
    {
        public bool IsConnected { get; }

        public ConnectionStateChangedEventArgs(bool isConnected)
        {
            IsConnected = isConnected;
        }
    }

    public interface IUcsiHvpsUartCommandInterface
    {
        /// <summary>True when USB UART connection is established and port is open</summary>
        bool IsConnected { get; }

        /// <summary>Raised when connection state changes (connected/disconnected)</summary>
        event EventHandler<ConnectionStateChangedEventArgs>? ConnectionStateChanged;

        /// <summary>Send *ACFGS\n, receive all 32 config values</summary>
        Task<SystemConfigResponse> RequestSystemConfig();

        /// <summary>Send *CONFIG_SET[0][index][value]\n, set individual config value</summary>
        Task SetSystemConfigValue(int index, float value);

        /// <summary>Initialize FTDI UART connection (called once at startup)</summary>
        Task InitializeAsync();

        /// <summary>Close FTDI UART connection (called on shutdown)</summary>
        Task CloseAsync();
    }
}
