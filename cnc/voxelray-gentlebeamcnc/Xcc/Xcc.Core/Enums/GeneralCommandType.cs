namespace Xcc.Core.Enums
{
    public enum GeneralCommandType
    {
        Heatbeat = 1,
        DeviceInfo = 2,
        DirectiveCommand = 3,
        PeriodicTelemetryRequest = 4,
        FaultInfoRequest = 5,
        OperationalPointCommand = 6,
        OpertionalPointQuery = 7,
        HeaterLKG_query = 8,//last Known Good Heater value - query (instead of "old" "CalibrationSetCommand")
        CalibrationSetQuery = 9,
        HeadBoardCommand = 10,
        NetworkConfig = 11,
        QC_DataQuery = 12,
        MagnetometerRead = 13,
        /////////////////// TODO: implement new added commands ///////////////////
        CoolingConfigCommand = 20,
        CoolingConfigQuery = 21,
        CoilConfigCommand = 22,
        CoilConfigQuery = 23,
        HVPS_configCommand = 24,
        HVPS_configQuery = 25,
        /// ///////////////////////////////////////////////////////////////////////
        CoilCalibration = 30,

    }
}
