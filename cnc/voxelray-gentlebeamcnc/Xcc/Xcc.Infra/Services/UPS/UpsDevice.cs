using System;
using System.Text;
using System.Threading;

namespace Xcc.Infra.Services.UPS
{
    public class UpsDevice(IHidDevice device) : IDisposable
    {
        #region Public methods
        /// <summary>
        /// Sends byte array query to the device and reads its responses until proper response shows up
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public string ExecuteQuery(string query, CancellationToken? token = null)
        {
            // Write query to the device with a trailing <CR>:
            SendQuery(query);
            
            string responsePrefix = $"{query}=";
            // Wait for a proper response, as we may have ring buffer filled with something irrelevant:
            string response = string.Empty;
            do
            {
                response = GetResponse();
                token?.ThrowIfCancellationRequested();
            } while (!response.StartsWith(responsePrefix));

            // Trim response prefix and return the rest of the string:
            return response.Remove(0, responsePrefix.Length);
        }

        /// <summary>
        /// Sends byte array query to the device
        /// </summary>
        /// <param name="query"></param>
        public void SendQuery(string query)
        {
            device.Write(Encoding.ASCII.GetBytes($"{query}\r"));
        }

        /// <summary>
        /// Reads response from device just once, 
        /// the result is not guaranteed to be the response to the actual query,
        /// as UPS uses ring report buffer and may fill it with a set of responses
        /// </summary>
        /// <returns></returns>
        public string GetResponse() => GetDeviceResponseString(device);

        public bool IsConnected() 
        {
            try
            {
                return device.IsConnected();
            }
            catch
            {
                return false;
            }
        }

        public void Close()
        {
            device.Close();
        }

        public void Dispose()
        {
            device.Dispose();
        }
        #endregion Public methods

        #region Private methods
        private static string GetDeviceResponseString(IHidDevice device)
        {
            string rxStr = string.Empty;

            //Read a byte array from the device in blocks and decode them to a string
            int bytesRead = 0;
            do
            {
                var tempRx = device.Read();
                bytesRead = tempRx.Length;
                if (bytesRead > 0)
                {
                    // reading is done in chunks of 9 bytes and filled with \0 to complement to 9 - discard \0 (null chars)
                    var temp = Encoding.ASCII.GetString(tempRx).Trim('\0');
                    rxStr += temp;
                    if (rxStr.Contains("\r") == true)
                    {
                        rxStr = rxStr.Split('\r')[0];// take string up to the first encounter of <CR> == \r
                        break;
                    }
                }
            } while (bytesRead > 0);

            return rxStr;
        }

        #endregion Private methods
    }
}
