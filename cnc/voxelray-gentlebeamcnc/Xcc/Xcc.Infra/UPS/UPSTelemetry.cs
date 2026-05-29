using System;
using Prism.Mvvm;
using Xcc.Core.Domain.UPS;

namespace Xcc.Infra.UPS
{
    public class UpsTelemetry : BindableBase, IUpsTelemetry
    {
        #region system properties

        private string? _batteryReplacementDate;
        public string? BatteryReplacementDate { get { return _batteryReplacementDate; } set { SetProperty(ref _batteryReplacementDate, value); } }

        #endregion


        #region input properties

        private string? _inputStatus;
        public string? InputStatus { get { return _inputStatus; } set { SetProperty(ref _inputStatus, value); } }

        private double _inputFrequency;
        public double InputFrequency { get { return _inputFrequency; } set { SetProperty(ref _inputFrequency, value); } }

        private double _inputVoltage;
        public double InputVoltage { get { return _inputVoltage; } set { SetProperty(ref _inputVoltage, value); } }

        private double _inputCurrent;
        public double InputCurrent { get { return _inputCurrent; } set { SetProperty(ref _inputCurrent, value); } }

        private double _inputPower;
        public double InputPower { get { return _inputPower; } set { SetProperty(ref _inputPower, value); } }

        #endregion

        #region output properties

        private string? _outputStatus;
        public string? OutputStatus { get { return _outputStatus; } set { SetProperty(ref _outputStatus, value); } }

        private double _outputFrequency;
        public double OutputFrequency { get { return _outputFrequency; } set { SetProperty(ref _outputFrequency, value); } }

        private double _outputMeasuredVoltage;
        public double OutputMeasuredVoltage { get { return _outputMeasuredVoltage; } set { SetProperty(ref _outputMeasuredVoltage, value); } }

        private double _outputMeasuredCurrent;
        public double OutputMeasuredCurrent { get { return _outputMeasuredCurrent; } set { SetProperty(ref _outputMeasuredCurrent, value); } }

        private double _outputPower;
        public double OutputPower { get { return _outputPower; } set { SetProperty(ref _outputPower, value); } }

        private double _outputLoad;
        public double OutputLoad { get { return _outputLoad; } set { SetProperty(ref _outputLoad, value); } }

        #endregion

        #region battery properties

        private string? _batteryStatus;
        public string? BatteryStatus { get { return _batteryStatus; } set { SetProperty(ref _batteryStatus, value); } }

        private string? _timeOnBattery;
        public string? TimeOnBattery { get { return _timeOnBattery; } set { SetProperty(ref _timeOnBattery, value); } }

        private int _estimatedBatRuntime;
        public int EstimatedBatRuntime { get { return _estimatedBatRuntime; } set { SetProperty(ref _estimatedBatRuntime, value); } }

        private double _batteryChargedPercent;
        public double BatteryChargedPercent { get { return _batteryChargedPercent; } set { SetProperty(ref _batteryChargedPercent, value); } }

        private double _battaryVoltage;
        public double BattaryVoltage { get { return _battaryVoltage; } set { SetProperty(ref _battaryVoltage, value); } }

        private double _battaryCurrent;
        public double BattaryCurrent { get { return _battaryCurrent; } set { SetProperty(ref _battaryCurrent, value); } }

        private double _temperature;
        public double Temperature { get { return _temperature; } set { SetProperty(ref _temperature, value); } }

        private double _maxTempRecorded;
        public double MaxTempRecorded { get { return _maxTempRecorded; } set { SetProperty(ref _maxTempRecorded, value); } }

        private double _battaryChargerCurrent;
        public double BattaryChargerCurrent { get { return _battaryChargerCurrent; } set { SetProperty(ref _battaryChargerCurrent, value); } }

        private double _totalMinutesON;
        public double TotalMinutesON { get { return _totalMinutesON; } set { SetProperty(ref _totalMinutesON, value); } }

        private int _usedTimesCounter;
        public int UsedTimesCounter { get { return _usedTimesCounter; } set { SetProperty(ref _usedTimesCounter, value); } }

        private int _depletionCounter;
        public int DepletionCounter { get { return _depletionCounter; } set { SetProperty(ref _depletionCounter, value); } }

        #endregion

        #region alarms properties

        private bool _batteryHealth;
        public bool BatteryHealth { get { return _batteryHealth; } set { SetProperty(ref _batteryHealth, value); } }
        private bool _batteryNotInUse;

        public bool BatteryNotInUse
        {
            get { return _batteryNotInUse; }
            set
            {
                if (SetProperty(ref _batteryNotInUse, value))
                    BatteryInUseStateUpdated?.Invoke(this, !_batteryNotInUse);
            }
        }
        private bool _batteryCharged;
        public bool BatteryCharged { get { return _batteryCharged; } set { SetProperty(ref _batteryCharged, value); } }
        private bool _normalTemperature;
        public bool NormalTemperature { get { return _normalTemperature; } set { SetProperty(ref _normalTemperature, value); } }
        private bool _inputInRange;
        public bool InputInRange { get { return _inputInRange; } set { SetProperty(ref _inputInRange, value); } }
        private bool _outputInRange;
        public bool OutputInRange { get { return _outputInRange; } set { SetProperty(ref _outputInRange, value); } }
        private bool _notOverloaded;
        public bool NotOverloaded { get { return _notOverloaded; } set { SetProperty(ref _notOverloaded, value); } }
        private bool _inverterOK;
        public bool InverterOK { get { return _inverterOK; } set { SetProperty(ref _inverterOK, value); } }
        private bool _outputsEnabled;
        public bool OutputsEnabled { get { return _outputsEnabled; } set { SetProperty(ref _outputsEnabled, value); } }
        private bool _fanOK;
        public bool FanOK { get { return _fanOK; } set { SetProperty(ref _fanOK, value); } }
        private bool _fuseOK;
        public bool FuseOK { get { return _fuseOK; } set { SetProperty(ref _fuseOK, value); } }
        private bool _generalSystemFault;
        public bool GeneralSystemFault { get { return _generalSystemFault; } set { SetProperty(ref _generalSystemFault, value); } }
        private bool _backfeedRelayFault;
        public bool BackfeedRelayFault { get { return _backfeedRelayFault; } set { SetProperty(ref _backfeedRelayFault, value); } }
        private bool _batteryReplacement;
        public bool BatteryReplacement { get { return _batteryReplacement; } set { SetProperty(ref _batteryReplacement, value); } }
        private bool _wiringFault;
        public bool WiringFault { get { return _wiringFault; } set { SetProperty(ref _wiringFault, value); } }

        #endregion
        
        #region unit properties

        private string? _model;
        public string? Model { get { return _model; } set { SetProperty(ref _model, value); } }

        private string? _serial;
        public string? Serial { get { return _serial; } set { SetProperty(ref _serial, value); } }

        #endregion

        public event EventHandler<bool?>? BatteryInUseStateUpdated;

        public static UpsTelemetry Parse(
            string[]? systemDataTokens,
            string[]? batteryDataTokens,
            string[]? alarmsDataTokens,
            string[]? inputDataTokens,
            string[]? outputDataTokens,
            string[][]? circuitDataTokens,
            string[]? unitId)
        {
            UpsTelemetry upsTelemetry = new UpsTelemetry();

            if (systemDataTokens != null)
                upsTelemetry.ParseSystemData(systemDataTokens);

            if (batteryDataTokens != null)
                upsTelemetry.ParseBatteryData(batteryDataTokens);

            if (alarmsDataTokens != null)
                upsTelemetry.ParseAlarmsData(alarmsDataTokens);

            if (outputDataTokens != null)
                upsTelemetry.ParseOutputData(outputDataTokens);

            if (inputDataTokens != null)
                upsTelemetry.ParseInputData(inputDataTokens);

            //upsTelemetry.RetrieveCircuitData(circuitDataTokens);

            if (unitId != null)
                upsTelemetry.ParseUnitIdData(unitId);

            return upsTelemetry;
        }

        public static UnitIdData ParseUnitId(string[] unitId)
        {
            const int modelIndex = 7;
            var modelInfoArray= unitId[modelIndex].Split("-"); 
            if (modelInfoArray.Length != 2)
                return new UnitIdData();
            
            return new UnitIdData
            {
                Model = modelInfoArray[0],
                Serial = modelInfoArray[1]

                //Model = unitId[7],
                //Serial = unitId[8]
            };
        }

        public void ParseUnitIdData(string[] unitId)
        {
            var modelInfo = ParseUnitId(unitId);

            Model = modelInfo.Model;
            Serial = modelInfo.Serial;
        }
        
        public void ParseSystemData(string[] systemDataTokens)
        {
            if (systemDataTokens.Length >= 13)
                BatteryReplacementDate = systemDataTokens[12];
            else
                BatteryReplacementDate = null;
        }

        public void ParseBatteryData(string[] batteryDataTokens)
        {
            switch (int.Parse(batteryDataTokens[0]))
            {
                case 2:
                    BatteryStatus = "normal";
                    break;
                case 3:
                    BatteryStatus = "low";
                    break;
                case 4:
                    BatteryStatus = "depleted";
                    break;
                case 1:
                default:
                    BatteryStatus = "unknown";
                    break;
            }

            TimeOnBattery = batteryDataTokens[1];
            EstimatedBatRuntime = int.Parse(batteryDataTokens[2]);
            BatteryChargedPercent = double.Parse(batteryDataTokens[3]);
            BattaryVoltage = double.Parse(batteryDataTokens[4]);
            BattaryCurrent = double.Parse(batteryDataTokens[5]);
            Temperature = double.Parse(batteryDataTokens[6]);
            MaxTempRecorded = double.Parse(batteryDataTokens[7]);
            BattaryChargerCurrent = double.Parse(batteryDataTokens[8]);
            TotalMinutesON = double.Parse(batteryDataTokens[9]);
            UsedTimesCounter = int.Parse(batteryDataTokens[10]);
            DepletionCounter = int.Parse(batteryDataTokens[11]);
        }

        public void ParseAlarmsData(string[] alarmsDataTokens)
        {
            BatteryHealth = alarmsDataTokens[1] == "0";
            BatteryNotInUse = alarmsDataTokens[2] == "0";
            BatteryCharged = alarmsDataTokens[3] == "0";
            NormalTemperature = alarmsDataTokens[6] == "0";
            InputInRange = alarmsDataTokens[7] == "0";
            OutputInRange = alarmsDataTokens[8] == "0";
            NotOverloaded = alarmsDataTokens[9] == "0";
            InverterOK = alarmsDataTokens[10] == "0";
            OutputsEnabled = alarmsDataTokens[13] == "0";
            FanOK = alarmsDataTokens[14] == "0";
            FuseOK = alarmsDataTokens[15] == "0";
            GeneralSystemFault = alarmsDataTokens[16] == "0";
            BackfeedRelayFault = alarmsDataTokens[24] == "0";
            BatteryReplacement = alarmsDataTokens[26] == "0";
            WiringFault = alarmsDataTokens[27] == "0";
        }

        public void ParseInputData(string[] inputDataTokens)
        {
            switch (int.Parse(inputDataTokens[0]))
            {
                case 0:
                    InputStatus = "OUT OF RANGE";
                    break;
                case 1:
                    InputStatus = "PROPER RANGE";
                    break;
                default:
                    InputStatus = "UNKNOWN";
                    break;
            }

            InputFrequency = double.Parse(inputDataTokens[1]);
            InputVoltage = double.Parse(inputDataTokens[2]);
            InputCurrent = double.Parse(inputDataTokens[3]);
            InputPower = double.Parse(inputDataTokens[4]);
        }

        public void ParseOutputData(string[] outputDataTokens)
        {
            switch (int.Parse(outputDataTokens[0]))
            {
                case 0:
                    OutputStatus = "NO OUTPUT";
                    break;
                case 1:
                    OutputStatus = "FROM LINE";
                    break;
                case 3:
                    OutputStatus = "FROM BATTERY";
                    break;
                case 4:
                    OutputStatus = "FIRST BOOST TAP";
                    break;
                case 5:
                    OutputStatus = "SECOND BOOST TAP";
                    break;
                case 6:
                    OutputStatus = "BUCK TAP";
                    break;
                default:
                    OutputStatus = "UNKNOWN";
                    break;
            }

            OutputFrequency = double.Parse(outputDataTokens[1]);
            OutputMeasuredVoltage = double.Parse(outputDataTokens[2]);
            OutputMeasuredCurrent = double.Parse(outputDataTokens[3]);
            OutputPower = double.Parse(outputDataTokens[4]);
            OutputLoad = double.Parse(outputDataTokens[5]);
        }

        public void RetrieveCircuitData(string[][] circuitDataTokens)
        {
        }

        public static bool GetUPSState(IUpsTelemetry upsTelemetry)
        {
            const double minVoltage = 220;
            const double minVoltageAllowed = minVoltage - minVoltage * 0.1;
            const double maxVoltage = 240;
            const double maxVoltageAllowed = maxVoltage + maxVoltage * 0.1;

            const double minBatteryChargePercent = 40;

            bool upsState = true;

            upsState &= upsTelemetry.InputVoltage is >= minVoltageAllowed and <= maxVoltageAllowed;
            upsState &= upsTelemetry.BatteryChargedPercent >= minBatteryChargePercent;

            return upsState; 
        }
        
        public struct UnitIdData
        {
            public string Model { get; set; }
            public string Serial { get; set; }
        }
    }
}
