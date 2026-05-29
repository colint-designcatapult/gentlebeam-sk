namespace Xcc.Core.Constants
{
    public class NetworkProperties
    {

        public const string DataCommandsEndPoint = "172.31.1.222:3232";
        public const string QcbEndPoint = "172.31.1.231:7000";
        public const string RobotArmServiceEndPoint = "172.31.1.147:30005";
        public const string RobotArmControllerEndPoint = "172.31.1.147:30300";
        public const string RobotArmControllerListenerEndPoint = "0.0.0.0:30333";
        public const string GcbTelemetryEndPoint = "172.31.1.100:20";
        public const string GcbCommandsEndPoint = "172.31.1.100:7";
        public const string DetectorServiceEndPoint = "172.31.1.222:5263";
        public const string UpsBroadcastServiceEndPoint = "172.31.1.255:58888";
        public const int RobotArmControlCommProxyPort = 58889;        

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

        public const string ROBOT_IPADDRESS = "172.31.1.147";
        public const int ROBOT_SERVER_APPLICATION_PORT = 30007;
        public const int ROBOT_SERVER_APPLICATION_PORT_LISTENER = 30008;
        public const int ROBOT_SERVER_APPLICATION_TELEMETRY_PORT_LISTENER = 30009;
        public const int ROBOT_SERVER_APPLICATION_EXTERN_TELEMETRY_PORT_LISTENER = 30010;
        
        public const int ROBOT_CONTROLLER_APPLICATION_PORT = 30300;
        
        public const int ROBOT_CONTROLLER_APPLICATION_PORT_LISTENER = 30333;

        public const int ROBOT_STATE_INFORMER_PORT = 99999;
        public const int ROBOT_CONTROLLER_APP_PORT = 30333;

        //public const string HEAD_CAMERA_IPADDRESS = "172.31.1.20";
        public const string HEAD_CAMERA_IPADDRESS = "172.31.1.80";
        public const int HEAD_CAMERA_PORT = 8080;
    }
}
