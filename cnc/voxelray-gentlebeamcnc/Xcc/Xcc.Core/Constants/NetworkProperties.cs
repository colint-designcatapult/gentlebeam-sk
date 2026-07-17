namespace Xcc.Core.Constants
{
    public class NetworkProperties
    {

        public const string DataCommandsEndPoint = "172.31.1.222:3232";
        public const string QcbEndPoint = "172.31.1.231:7000";
        public const string GcbTelemetryEndPoint = "172.31.1.100:20";
        public const string GcbCommandsEndPoint = "172.31.1.100:7";
        public const string DetectorServiceEndPoint = "172.31.1.222:5263";      

        public const string MCC_IPADDRESS = "172.31.1.222";
        //public const string MCC_IPADDRESS = "127.0.0.1"; // temp
        //public const string MCC_IPADDRESS = "10.0.0.3"; // temp

        public const string GCB_IPADDRESS = "172.31.1.100";
        public const int GCB_TELEMETRY_PORT = 20;
        public const int GCB_COMMANDS_PORT = 7;

        public const string QCB_IPADDRESS = "172.31.1.231";

        public const int QCB_COMMANDS_PORT = 7000;
        
        public const string EMR_SERVICE_IPADDRESS = "172.31.1.222"; //"127.0.0.1";
        public const int EMR_SERVICE_PORT = 3232;//8080; //7226;

        //public const string HEAD_CAMERA_IPADDRESS = "172.31.1.20";
        public const string HEAD_CAMERA_IPADDRESS = "172.31.1.80";
        public const int HEAD_CAMERA_PORT = 8080;
    }
}
