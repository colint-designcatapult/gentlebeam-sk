namespace Empyrean.Common.Infra.Networking
{
    public interface IAsyncClientConnection : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns>received data</returns>
        Task<byte[]> ReceiveAsync(CancellationToken cancellationToken);

        /// <summary>        
        /// </summary>
        /// <param name="data"></param>
        /// <returns>number of bytes sent</returns>
        Task<int> SendAsync(byte[] data);
    }
}
