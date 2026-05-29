using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace Xcc.Application.Helpers
{
    public class WindowHost : HwndHost
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        private const int GWL_STYLE = -16;
        private const uint WS_CHILD = 0x40000000;

        private IntPtr _childHandle;
        private int _processId;

        public event EventHandler? ProcessExitedOrTerminated;

        public WindowHost(string processPath, string? args = null)
        {
            var psi = new ProcessStartInfo
            {
                FileName = processPath,
                Arguments = args ?? string.Empty,
                UseShellExecute = true
            };

            // Start the external Win32 application
            var process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start the application."); ;

            // Wait for the process to initialize its window
            process.WaitForInputIdle();

            // Retry logic to get the MainWindowHandle
            const int maxRetries = 10;
            const int delayBetweenRetries = 500; // milliseconds
            for (int i = 0; i < maxRetries; i++)
            {
                _childHandle = process.MainWindowHandle;
                _processId = process.Id;

                if (_childHandle != IntPtr.Zero)
                {
                    break;
                }
                System.Threading.Thread.Sleep(delayBetweenRetries);
                process.Refresh(); // Refresh process information
            }

            if (_childHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to retrieve the child window handle.");
            }

            // Monitor process to detect crash or termination
            Task.Run(() =>
            {
                process.WaitForExit();
                System.Windows.Application.Current.Dispatcher.Invoke(() => ProcessExitedOrTerminated?.Invoke(this, EventArgs.Empty));
            });
        }

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            if (_childHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("Child window handle is invalid.");
            }

            // Set the external application's window as a child of the WPF window
            if (SetParent(_childHandle, hwndParent.Handle) == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to set parent window: the child process may have crashed or terminated.");
            }

            // Update the window style to make it a child window and remove the border
            int style = GetWindowLong(_childHandle, GWL_STYLE);
            style &= ~(0x00C00000 | 0x00040000); // Remove WS_CAPTION (0x00C00000) and WS_THICKFRAME (0x00040000)
            SetWindowLong(_childHandle, GWL_STYLE, (uint)(style | WS_CHILD));

            // Resize the child window to fit the parent container
            MoveWindow(_childHandle, 0, 0, 800, 600, true);

            return new HandleRef(this, _childHandle);
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            // Optionally, close the external application when the host is destroyed
            if (_childHandle != IntPtr.Zero)
            {
                Process.GetProcessById(_processId).Kill();
            }
        }
    }
}
