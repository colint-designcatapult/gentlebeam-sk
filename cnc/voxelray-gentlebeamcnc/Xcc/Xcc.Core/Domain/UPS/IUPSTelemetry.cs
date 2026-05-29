using System;

namespace Xcc.Core.Domain.UPS
{
    public interface IUpsTelemetry
    {

        #region system properties

        public string? BatteryReplacementDate { get; set; }
        #endregion

        #region input properties

        public string? InputStatus { get; set; }
        public double InputFrequency { get; set; }
        public double InputVoltage { get; set; }
        public double InputCurrent { get; set; }
        public double InputPower { get; set; }

        #endregion

        #region output properties

        public string? OutputStatus { get; set; }
        public double OutputFrequency { get; set; }
        public double OutputMeasuredVoltage { get; set; }
        public double OutputMeasuredCurrent { get; set; }
        public double OutputPower { get; set; }
        public double OutputLoad { get; set; }

        #endregion

        #region battery properties

        public string? BatteryStatus { get; set; }
        public string? TimeOnBattery { get; set; }
        public int EstimatedBatRuntime { get; set; }
        public double BatteryChargedPercent { get; set; }
        public double BattaryVoltage { get; set; }
        public double BattaryCurrent { get; set; }
        public double Temperature { get; set; }
        public double MaxTempRecorded { get; set; }
        public double BattaryChargerCurrent { get; set; }
        public double TotalMinutesON { get; set; }
        public int UsedTimesCounter { get; set; }
        public int DepletionCounter { get; set; }

        #endregion

        #region alarms properties

        public bool BatteryHealth { get; set; }
        public bool BatteryNotInUse { get; set; }
        public bool BatteryCharged { get; set; }
        public bool NormalTemperature { get; set; }
        public bool InputInRange { get; set; }
        public bool OutputInRange { get; set; }
        public bool NotOverloaded { get; set; }
        public bool InverterOK { get; set; }
        public bool OutputsEnabled { get; set; }
        public bool FanOK { get; set; }
        public bool FuseOK { get; set; }
        public bool GeneralSystemFault { get; set; }
        public bool BackfeedRelayFault { get; set; }
        public bool BatteryReplacement { get; set; }
        public bool WiringFault { get; set; }

        #endregion

        #region unit id
        public string? Model { get; set; }
        public string? Serial { get; set; }

        #endregion

        event EventHandler<bool?> BatteryInUseStateUpdated;
    }
}
