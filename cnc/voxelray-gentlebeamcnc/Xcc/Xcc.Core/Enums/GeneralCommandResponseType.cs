namespace Xcc.Core.Enums
{
    public enum GeneralCommandResponseType
    {
        InvalidPacket = 100,
        Heatbeat = 101,
        DeviceInfo = 102,
        DirectiveCommand = 103,
        PeriodicTelemetryRequest = 104,
        FaultInfoRequest = 105,
        OperationalPointCommand = 106,
        OpertionalPointQuery = 107,
        HeaterLKG_response = 108,
        CalibrationSetQuery = 109,
        HeadBoardCommand = 110,
        NetworkConfig = 111,
        QC_DataQuery = 112,
        MagnetometerRead = 113,
        /////////TODO: implement new added commands //////////////////////////////////////
        CoolingConfigCommand = 120,
        CoolingConfigQuery = 121,
        CoilConfigCommand = 122,
        CoilConfigQuery = 123,
        HVPS_configCommand = 124,
        HVPS_configQuery = 125,
        //////////////////////////////////////////////////////////////////////////////////
        CoilCalibration = 130,
    }
}
