namespace Xcc.Core.Domain.DataManagement.System
{
    public class MosesSystemInfo
    {
        public MosesSystemInfo(string version)
        {
            Version = version;
        }

        public string Version { get; }
    }
}
