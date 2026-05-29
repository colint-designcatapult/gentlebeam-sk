using System.ComponentModel;
using Moq;
using Xcc.Infra.UPS;

namespace Xcc.Test.Xcc.Infra.UPS
{
    internal class UpsTelemetryTests
    {
        [SetUp]
        public void SetUp()
        {
            G.SetupCulture();
        }
        
        [Test]
        public void Defaults()
        {
            var sut = new UpsTelemetry();
            
            // system properties
            Assert.That(sut.BatteryReplacementDate, Is.Null);
            // input properties
            Assert.That(sut.InputStatus, Is.Null);
            Assert.That(sut.InputFrequency, Is.EqualTo(0).Within(G.Precision));
            Assert.That(sut.InputVoltage, Is.EqualTo(0).Within(G.Precision));
            Assert.That(sut.InputCurrent, Is.EqualTo(0).Within(G.Precision));
            Assert.That(sut.InputPower, Is.EqualTo(0).Within(G.Precision));
            // output properties
            Assert.That(sut.OutputStatus, Is.Null);
            Assert.That(sut.OutputFrequency, Is.EqualTo(0).Within(G.Precision));
            Assert.That(sut.OutputMeasuredVoltage, Is.EqualTo(0).Within(G.Precision));
            Assert.That(sut.OutputMeasuredCurrent, Is.EqualTo(0).Within(G.Precision));
            Assert.That(sut.OutputPower, Is.EqualTo(0).Within(G.Precision));
            Assert.That(sut.OutputLoad, Is.EqualTo(0).Within(G.Precision));
            // battery properties
            Assert.That(sut.BatteryStatus, Is.Null);
            Assert.That(sut.TimeOnBattery, Is.Null);
            Assert.That(sut.EstimatedBatRuntime, Is.EqualTo(0));
            Assert.That(sut.BatteryChargedPercent, Is.EqualTo(0).Within(G.Precision));
            Assert.That(sut.BattaryVoltage, Is.EqualTo(0).Within(G.Precision));
            Assert.That(sut.BattaryCurrent, Is.EqualTo(0).Within(G.Precision));
            Assert.That(sut.Temperature, Is.EqualTo(0).Within(G.Precision));
            Assert.That(sut.MaxTempRecorded, Is.EqualTo(0).Within(G.Precision));
            Assert.That(sut.BattaryChargerCurrent, Is.EqualTo(0).Within(G.Precision));
            Assert.That(sut.TotalMinutesON, Is.EqualTo(0).Within(G.Precision));
            Assert.That(sut.UsedTimesCounter, Is.EqualTo(0));
            Assert.That(sut.DepletionCounter, Is.EqualTo(0));
            // alarms properties
            Assert.That(sut.BatteryHealth, Is.False);
            Assert.That(sut.BatteryNotInUse, Is.False);
            Assert.That(sut.BatteryCharged, Is.False);
            Assert.That(sut.NormalTemperature, Is.False);
            Assert.That(sut.InputInRange, Is.False);
            Assert.That(sut.OutputInRange, Is.False);
            Assert.That(sut.NotOverloaded, Is.False);
            Assert.That(sut.InverterOK, Is.False);
            Assert.That(sut.OutputsEnabled, Is.False);
            Assert.That(sut.FanOK, Is.False);
            Assert.That(sut.FuseOK, Is.False);
            Assert.That(sut.GeneralSystemFault, Is.False);
            Assert.That(sut.BackfeedRelayFault, Is.False);
            Assert.That(sut.BatteryReplacement, Is.False);
            Assert.That(sut.WiringFault, Is.False);
            // unit properties
            Assert.That(sut.Model, Is.Null);
            Assert.That(sut.Serial, Is.Null);
        }
        
        [Test]
        public void GettersSetters()
        {
            // system properties
            var batteryReplacementDate = "value 1";
            // input properties
            var inputStatus = "value 2";
            var inputFrequency = 3.3;
            var inputVoltage = 4.4;
            var inputCurrent = 5.5;
            var inputPower = 6.6;
            // output properties
            var outputStatus = "value 7";
            var outputFrequency = 7.7;
            var outputMeasuredVoltage = 8.8;
            var outputMeasuredCurrent = 9.9;
            var outputPower = 10.10;
            var outputLoad = 11.11;
            // battery properties
            var batteryStatus = "value 12";
            var timeOnBattery = "value 13";
            var estimatedBatRuntime = 14;
            var batteryChargedPercent = 15.15;
            var battaryVoltage = 16.16;
            var battaryCurrent = 17.17;
            var temperature = 18.18;
            var maxTempRecorded = 19.19;
            var battaryChargerCurrent = 20.20;
            var totalMinutesON = 21.21;
            var usedTimesCounter = 22;
            var depletionCounter = 23;
            // alarms properties
            var batteryHealth = true;
            var batteryNotInUse = true;
            var batteryCharged = true;
            var normalTemperature = true;
            var inputInRange = true;
            var outputInRange = true;
            var notOverloaded = true;
            var inverterOK = true;
            var outputsEnabled = true;
            var fanOK = true;
            var fuseOK = true;
            var generalSystemFault = true;
            var backfeedRelayFault = true;
            var batteryReplacement = true;
            var wiringFault = true;
            // unit properties
            var model = "value 24";
            var serial = "value 25";
            
            var sut = new UpsTelemetry
            {
                // system properties
                BatteryReplacementDate = batteryReplacementDate,
                // input properties
                InputStatus = inputStatus,
                InputFrequency = inputFrequency,
                InputVoltage = inputVoltage,
                InputCurrent = inputCurrent,
                InputPower = inputPower,
                // output properties
                OutputStatus = outputStatus,
                OutputFrequency = outputFrequency,
                OutputMeasuredVoltage = outputMeasuredVoltage,
                OutputMeasuredCurrent = outputMeasuredCurrent,
                OutputPower = outputPower,
                OutputLoad = outputLoad,
                // battery properties
                BatteryStatus = batteryStatus,
                TimeOnBattery = timeOnBattery,
                EstimatedBatRuntime = estimatedBatRuntime,
                BatteryChargedPercent = batteryChargedPercent,
                BattaryVoltage = battaryVoltage,
                BattaryCurrent = battaryCurrent,
                Temperature = temperature,
                MaxTempRecorded = maxTempRecorded,
                BattaryChargerCurrent = battaryChargerCurrent,
                TotalMinutesON = totalMinutesON,
                UsedTimesCounter = usedTimesCounter,
                DepletionCounter = depletionCounter,
                // alarms properties
                BatteryHealth = batteryHealth,
                BatteryNotInUse = batteryNotInUse,
                BatteryCharged = batteryCharged,
                NormalTemperature = normalTemperature,
                InputInRange = inputInRange,
                OutputInRange = outputInRange,
                NotOverloaded = notOverloaded,
                InverterOK = inverterOK,
                OutputsEnabled = outputsEnabled,
                FanOK = fanOK,
                FuseOK = fuseOK,
                GeneralSystemFault = generalSystemFault,
                BackfeedRelayFault = backfeedRelayFault,
                BatteryReplacement = batteryReplacement,
                WiringFault = wiringFault,
                // unit properties
                Model = model,
                Serial = serial
            };
            
            // system properties
            Assert.That(sut.BatteryReplacementDate, Is.EqualTo(batteryReplacementDate));
            // input properties
            Assert.That(sut.InputStatus, Is.EqualTo(inputStatus));
            Assert.That(sut.InputFrequency, Is.EqualTo(inputFrequency).Within(G.Precision));
            Assert.That(sut.InputVoltage, Is.EqualTo(inputVoltage).Within(G.Precision));
            Assert.That(sut.InputCurrent, Is.EqualTo(inputCurrent).Within(G.Precision));
            Assert.That(sut.InputPower, Is.EqualTo(inputPower).Within(G.Precision));
            // output properties
            Assert.That(sut.OutputStatus, Is.EqualTo(outputStatus));
            Assert.That(sut.OutputFrequency, Is.EqualTo(outputFrequency).Within(G.Precision));
            Assert.That(sut.OutputMeasuredVoltage, Is.EqualTo(outputMeasuredVoltage).Within(G.Precision));
            Assert.That(sut.OutputMeasuredCurrent, Is.EqualTo(outputMeasuredCurrent).Within(G.Precision));
            Assert.That(sut.OutputPower, Is.EqualTo(outputPower).Within(G.Precision));
            Assert.That(sut.OutputLoad, Is.EqualTo(outputLoad).Within(G.Precision));
            // battery properties
            Assert.That(sut.BatteryStatus, Is.EqualTo(batteryStatus));
            Assert.That(sut.TimeOnBattery, Is.EqualTo(timeOnBattery));
            Assert.That(sut.EstimatedBatRuntime, Is.EqualTo(estimatedBatRuntime));
            Assert.That(sut.BatteryChargedPercent, Is.EqualTo(batteryChargedPercent).Within(G.Precision));
            Assert.That(sut.BattaryVoltage, Is.EqualTo(battaryVoltage).Within(G.Precision));
            Assert.That(sut.BattaryCurrent, Is.EqualTo(battaryCurrent).Within(G.Precision));
            Assert.That(sut.Temperature, Is.EqualTo(temperature).Within(G.Precision));
            Assert.That(sut.MaxTempRecorded, Is.EqualTo(maxTempRecorded).Within(G.Precision));
            Assert.That(sut.BattaryChargerCurrent, Is.EqualTo(battaryChargerCurrent).Within(G.Precision));
            Assert.That(sut.TotalMinutesON, Is.EqualTo(totalMinutesON).Within(G.Precision));
            Assert.That(sut.UsedTimesCounter, Is.EqualTo(usedTimesCounter));
            Assert.That(sut.DepletionCounter, Is.EqualTo(depletionCounter));
            // alarms properties
            Assert.That(sut.BatteryHealth, Is.EqualTo(batteryHealth));
            Assert.That(sut.BatteryNotInUse, Is.EqualTo(batteryNotInUse));
            Assert.That(sut.BatteryCharged, Is.EqualTo(batteryCharged));
            Assert.That(sut.NormalTemperature, Is.EqualTo(normalTemperature));
            Assert.That(sut.InputInRange, Is.EqualTo(inputInRange));
            Assert.That(sut.OutputInRange, Is.EqualTo(outputInRange));
            Assert.That(sut.NotOverloaded, Is.EqualTo(notOverloaded));
            Assert.That(sut.InverterOK, Is.EqualTo(inverterOK));
            Assert.That(sut.OutputsEnabled, Is.EqualTo(outputsEnabled));
            Assert.That(sut.FanOK, Is.EqualTo(fanOK));
            Assert.That(sut.FuseOK, Is.EqualTo(fuseOK));
            Assert.That(sut.GeneralSystemFault, Is.EqualTo(generalSystemFault));
            Assert.That(sut.BackfeedRelayFault, Is.EqualTo(backfeedRelayFault));
            Assert.That(sut.BatteryReplacement, Is.EqualTo(batteryReplacement));
            Assert.That(sut.WiringFault, Is.EqualTo(wiringFault));
            //unit properties
            Assert.That(sut.Model, Is.EqualTo(model));
            Assert.That(sut.Serial, Is.EqualTo(serial));
        }
        
        // system properties
        [TestCase(nameof(UpsTelemetry.BatteryReplacementDate), "value")]
        // input properties
        [TestCase(nameof(UpsTelemetry.InputStatus), "value")]
        [TestCase(nameof(UpsTelemetry.InputFrequency), 3.3)]
        [TestCase(nameof(UpsTelemetry.InputVoltage), 3.3)]
        [TestCase(nameof(UpsTelemetry.InputCurrent), 3.3)]
        [TestCase(nameof(UpsTelemetry.InputPower), 3.3)]
        // output properties
        [TestCase(nameof(UpsTelemetry.OutputStatus), "value")]
        [TestCase(nameof(UpsTelemetry.OutputFrequency), 3.3)]
        [TestCase(nameof(UpsTelemetry.OutputMeasuredVoltage), 3.3)]
        [TestCase(nameof(UpsTelemetry.OutputMeasuredCurrent), 3.3)]
        [TestCase(nameof(UpsTelemetry.OutputPower), 3.3)]
        [TestCase(nameof(UpsTelemetry.OutputLoad), 3.3)]
        // battery properties
        [TestCase(nameof(UpsTelemetry.BatteryStatus), "value")]
        [TestCase(nameof(UpsTelemetry.TimeOnBattery), "value")]
        [TestCase(nameof(UpsTelemetry.EstimatedBatRuntime), 2)]
        [TestCase(nameof(UpsTelemetry.BatteryChargedPercent), 3.3)]
        [TestCase(nameof(UpsTelemetry.BattaryVoltage), 3.3)]
        [TestCase(nameof(UpsTelemetry.BattaryCurrent), 3.3)]
        [TestCase(nameof(UpsTelemetry.Temperature), 3.3)]
        [TestCase(nameof(UpsTelemetry.MaxTempRecorded), 3.3)]
        [TestCase(nameof(UpsTelemetry.BattaryChargerCurrent), 3.3)]
        [TestCase(nameof(UpsTelemetry.TotalMinutesON), 3.3)]
        [TestCase(nameof(UpsTelemetry.UsedTimesCounter), 2)]
        [TestCase(nameof(UpsTelemetry.DepletionCounter), 2)]
        // alarms properties
        [TestCase(nameof(UpsTelemetry.BatteryHealth), true)]
        [TestCase(nameof(UpsTelemetry.BatteryNotInUse), true)]
        [TestCase(nameof(UpsTelemetry.BatteryCharged), true)]
        [TestCase(nameof(UpsTelemetry.NormalTemperature), true)]
        [TestCase(nameof(UpsTelemetry.InputInRange), true)]
        [TestCase(nameof(UpsTelemetry.OutputInRange), true)]
        [TestCase(nameof(UpsTelemetry.NotOverloaded), true)]
        [TestCase(nameof(UpsTelemetry.InverterOK), true)]
        [TestCase(nameof(UpsTelemetry.OutputsEnabled), true)]
        [TestCase(nameof(UpsTelemetry.FanOK), true)]
        [TestCase(nameof(UpsTelemetry.FuseOK), true)]
        [TestCase(nameof(UpsTelemetry.GeneralSystemFault), true)]
        [TestCase(nameof(UpsTelemetry.BackfeedRelayFault), true)]
        [TestCase(nameof(UpsTelemetry.BatteryReplacement), true)]
        [TestCase(nameof(UpsTelemetry.WiringFault), true)]
        // unit properties
        [TestCase(nameof(UpsTelemetry.Model), "value")]
        [TestCase(nameof(UpsTelemetry.Serial), "value")]
        public void PropertyChanged_False_WithSameValue(string property, object value)
        {
            var sut = new UpsTelemetry();
            sut.SetNotifiedPropertyValue(property, value); // Init with value
            
            // Set same value
            bool isPropertyChangedInvoked = sut.SetNotifiedPropertyValue(property, value);
            Assert.That(isPropertyChangedInvoked, Is.False);
        }
        
        // system properties
        [TestCase(nameof(UpsTelemetry.BatteryReplacementDate), "value", "new value")]
        // input properties
        [TestCase(nameof(UpsTelemetry.InputStatus), "value", "new value")]
        [TestCase(nameof(UpsTelemetry.InputFrequency), 3.3, 5.5)]
        [TestCase(nameof(UpsTelemetry.InputVoltage), 3.3, 5.5)]
        [TestCase(nameof(UpsTelemetry.InputCurrent), 3.3, 5.5)]
        [TestCase(nameof(UpsTelemetry.InputPower), 3.3, 5.5)]
        // output properties
        [TestCase(nameof(UpsTelemetry.OutputStatus), "value", "new value")]
        [TestCase(nameof(UpsTelemetry.OutputFrequency), 3.3, 5.5)]
        [TestCase(nameof(UpsTelemetry.OutputMeasuredVoltage), 3.3, 5.5)]
        [TestCase(nameof(UpsTelemetry.OutputMeasuredCurrent), 3.3, 5.5)]
        [TestCase(nameof(UpsTelemetry.OutputPower), 3.3, 5.5)]
        [TestCase(nameof(UpsTelemetry.OutputLoad), 3.3, 5.5)]
        // battery properties
        [TestCase(nameof(UpsTelemetry.BatteryStatus), "value", "new value")]
        [TestCase(nameof(UpsTelemetry.TimeOnBattery), "value", "new value")]
        [TestCase(nameof(UpsTelemetry.EstimatedBatRuntime), 2, 4)]
        [TestCase(nameof(UpsTelemetry.BatteryChargedPercent), 3.3, 5.5)]
        [TestCase(nameof(UpsTelemetry.BattaryVoltage), 3.3, 5.5)]
        [TestCase(nameof(UpsTelemetry.BattaryCurrent), 3.3, 5.5)]
        [TestCase(nameof(UpsTelemetry.Temperature), 3.3, 5.5)]
        [TestCase(nameof(UpsTelemetry.MaxTempRecorded), 3.3, 5.5)]
        [TestCase(nameof(UpsTelemetry.BattaryChargerCurrent), 3.3, 5.5)]
        [TestCase(nameof(UpsTelemetry.TotalMinutesON), 3.3, 5.5)]
        [TestCase(nameof(UpsTelemetry.UsedTimesCounter), 2, 4)]
        [TestCase(nameof(UpsTelemetry.DepletionCounter), 2, 4)]
        // alarms properties
        [TestCase(nameof(UpsTelemetry.BatteryHealth), false, true)]
        [TestCase(nameof(UpsTelemetry.BatteryNotInUse), false, true)]
        [TestCase(nameof(UpsTelemetry.BatteryCharged), false, true)]
        [TestCase(nameof(UpsTelemetry.NormalTemperature), false, true)]
        [TestCase(nameof(UpsTelemetry.InputInRange), false, true)]
        [TestCase(nameof(UpsTelemetry.OutputInRange), false, true)]
        [TestCase(nameof(UpsTelemetry.NotOverloaded), false, true)]
        [TestCase(nameof(UpsTelemetry.InverterOK), false, true)]
        [TestCase(nameof(UpsTelemetry.OutputsEnabled), false, true)]
        [TestCase(nameof(UpsTelemetry.FanOK), false, true)]
        [TestCase(nameof(UpsTelemetry.FuseOK), false, true)]
        [TestCase(nameof(UpsTelemetry.GeneralSystemFault), false, true)]
        [TestCase(nameof(UpsTelemetry.BackfeedRelayFault), false, true)]
        [TestCase(nameof(UpsTelemetry.BatteryReplacement), false, true)]
        [TestCase(nameof(UpsTelemetry.WiringFault), false, true)]
        // unit properties
        [TestCase(nameof(UpsTelemetry.Model), "value", "new value")]
        [TestCase(nameof(UpsTelemetry.Serial), "value", "new value")]
        public void PropertyChanged_True_WithNewValue(string property, object initValue, object newValue)
        {
            var sut = new UpsTelemetry();
            sut.SetNotifiedPropertyValue(property, initValue); // Init with value
            
            // Set new value
            bool isPropertyChangedInvoked = sut.SetNotifiedPropertyValue(property, newValue);
            Assert.That(isPropertyChangedInvoked, Is.True);
        }
        
        [Test]
        public void BatteryNotInUse_WithSameValue_NotInvoke_BatteryInUseStateUpdated(
            [Values(false, true)] bool value)
        {
            var sut = new UpsTelemetry { BatteryNotInUse = value };

            bool isInvoked = false;
            sut.BatteryInUseStateUpdated += (s, e) => { isInvoked = true; };

            sut.BatteryNotInUse = value;
            
            Assert.That(isInvoked, Is.False);
        }
        
        [TestCase(false, true)]
        [TestCase(true, false)]
        public void BatteryNotInUse_WithNewValue_Invoke_BatteryInUseStateUpdated(bool initValue, bool newValue)
        {
            var sut = new UpsTelemetry { BatteryNotInUse = initValue };
            bool expectedInvokedValue = !newValue;

            bool isInvoked = false;
            bool? invokedValue = null; 
            sut.BatteryInUseStateUpdated += (s, e) =>
            {
                isInvoked = true;
                invokedValue = e.Value;
            };

            sut.BatteryNotInUse = newValue;

            
            Assert.That(isInvoked, Is.True);
            Assert.That(invokedValue, Is.EqualTo(expectedInvokedValue));
        }
        
        [Test]
        public void Parse_With_Nulls()
        {
            var result = UpsTelemetry.Parse(
                systemDataTokens: null,
                batteryDataTokens: null,
                alarmsDataTokens: null,
                inputDataTokens: null,
                outputDataTokens: null,
                circuitDataTokens: null,
                unitId: null);

            var expected = new UpsTelemetry();
            result.AssertAllPublicPropertiesEqualTo(expected);
        }
        
        [Test]
        public void Parse_With_SystemDataTokens_Throws(
            [Range(0, 12)] int tokenCount)
        {
            var systemDataTokens = new string[tokenCount];
            
            Assert.That(() =>
            {
                var result = UpsTelemetry.Parse(
                    systemDataTokens: systemDataTokens,
                    batteryDataTokens: null,
                    alarmsDataTokens: null,
                    inputDataTokens: null,
                    outputDataTokens: null,
                    circuitDataTokens: null,
                    unitId: null);
            }, Throws.Exception);
        }
        
        [Test]
        public void Parse_With_BatteryDataTokens_Throws(
            [Range(0, 11)] int tokenCount)
        {
            var batteryDataTokens = new string[tokenCount];
            
            Assert.That(() =>
            {
                var result = UpsTelemetry.Parse(
                    systemDataTokens: null,
                    batteryDataTokens: batteryDataTokens,
                    alarmsDataTokens: null,
                    inputDataTokens: null,
                    outputDataTokens: null,
                    circuitDataTokens: null,
                    unitId: null);
            }, Throws.Exception);
        }
        
        [Test]
        public void Parse_With_AlarmsDataTokens_Throws_0()
        {
            var alarmsDataTokens = new string[1];
            
            Assert.That(() =>
            {
                var result = UpsTelemetry.Parse(
                    systemDataTokens: null,
                    batteryDataTokens: null,
                    alarmsDataTokens: alarmsDataTokens,
                    inputDataTokens: null,
                    outputDataTokens: null,
                    circuitDataTokens: null,
                    unitId: null);
            }, Throws.Exception);
        }
        
        [Test]
        public void Parse_With_AlarmsDataTokens_Throws(
            [Range(1, 27)] int tokenCount)
        {
            var alarmsDataTokens = new string[tokenCount];
            alarmsDataTokens[0] = "ALM";
            
            Assert.That(() =>
            {
                var result = UpsTelemetry.Parse(
                    systemDataTokens: null,
                    batteryDataTokens: null,
                    alarmsDataTokens: alarmsDataTokens,
                    inputDataTokens: null,
                    outputDataTokens: null,
                    circuitDataTokens: null,
                    unitId: null);
            }, Throws.Exception);
        }
        
        [Test]
        public void Parse_With_InputDataTokens_Throws(
            [Range(0, 4)] int tokenCount)
        {
            var inputDataTokens = new string[tokenCount];
            
            Assert.That(() =>
            {
                var result = UpsTelemetry.Parse(
                    systemDataTokens: null,
                    batteryDataTokens: null,
                    alarmsDataTokens: null,
                    inputDataTokens: inputDataTokens,
                    outputDataTokens: null,
                    circuitDataTokens: null,
                    unitId: null);
            }, Throws.Exception);
        }
        
        [Test]
        public void Parse_With_OutputDataTokens_Throws(
            [Range(0, 5)] int tokenCount)
        {
            var outputDataTokens = new string[tokenCount];
            
            Assert.That(() =>
            {
                var result = UpsTelemetry.Parse(
                    systemDataTokens: null,
                    batteryDataTokens: null,
                    alarmsDataTokens: null,
                    inputDataTokens: null,
                    outputDataTokens: outputDataTokens,
                    circuitDataTokens: null,
                    unitId: null);
            }, Throws.Exception);
        }
        
        [Test]
        public void Parse_With_UnitId_DoesNotThrow()
        {
            var circuitDataTokens = new string[][] { };
            
            Assert.DoesNotThrow(() =>
            {
                var result = UpsTelemetry.Parse(
                    systemDataTokens: null,
                    batteryDataTokens: null,
                    alarmsDataTokens: null,
                    inputDataTokens: null,
                    outputDataTokens: null,
                    circuitDataTokens: circuitDataTokens,
                    unitId: null);
            });
        }
        
        [Test]
        public void Parse_With_UnitId_Throws(
            [Range(0, 7)] int tokenCount)
        {
            var unitId = new string[tokenCount];
            
            Assert.That(() =>
            {
                var result = UpsTelemetry.Parse(
                    systemDataTokens: null,
                    batteryDataTokens: null,
                    alarmsDataTokens: null,
                    inputDataTokens: null,
                    outputDataTokens: null,
                    circuitDataTokens: null,
                    unitId: unitId);
            }, Throws.Exception);
        }
        
        [Test]
        public void Parse_With_SystemDataTokens()
        {
            var expected = new UpsTelemetry { BatteryReplacementDate = "2024-12-31" };
            
            string[] systemDataTokens = Enumerable.Repeat("", 13).ToArray();
            systemDataTokens[12] = expected.BatteryReplacementDate;

            var result = UpsTelemetry.Parse(
                systemDataTokens: systemDataTokens,
                batteryDataTokens: null,
                alarmsDataTokens: null,
                inputDataTokens: null,
                outputDataTokens: null,
                circuitDataTokens: null,
                unitId: null);

            result.AssertAllPublicPropertiesEqualTo(expected);
        }
        
        [TestCase("BAT=1", "01:00", 30, 99.9, 12.3, 1.1, 35.5, 38.0, 0.8, 1234.5, 10, 2, "unknown")]
        [TestCase("BAT=2", "02:00", 45, 88.8, 12.4, 1.2, 36.5, 39.0, 0.9, 2345.6, 11, 3, "normal")]
        [TestCase("BAT=3", "03:00", 60, 77.7, 12.5, 1.3, 37.5, 40.0, 1.0, 3456.7, 12, 4, "low")]
        [TestCase("BAT=4", "04:00", 75, 66.6, 12.6, 1.4, 38.5, 41.0, 1.1, 4567.8, 13, 5, "depleted")]
        public void Parse_With_BatteryDataTokens(
            string statusToken, string timeOnBattery,
            int estRuntime, double percent, double voltage, double current,
            double temp, double maxTemp, double chargerCurrent,
            double totalMinutes, int usedTimes, int depletionCount,
            string expectedStatus)
        {
            var batteryDataTokens = new[]
            {
                statusToken,
                timeOnBattery,
                estRuntime.ToString(G.Culture),
                percent.ToString(G.Culture),
                voltage.ToString(G.Culture),
                current.ToString(G.Culture),
                temp.ToString(G.Culture),
                maxTemp.ToString(G.Culture),
                chargerCurrent.ToString(G.Culture),
                totalMinutes.ToString(G.Culture),
                usedTimes.ToString(),
                depletionCount.ToString()
            };
            
            var expected = new UpsTelemetry
            {
                BatteryStatus = expectedStatus,
                TimeOnBattery = timeOnBattery,
                EstimatedBatRuntime = estRuntime,
                BatteryChargedPercent = percent,
                BattaryVoltage = voltage,
                BattaryCurrent = current,
                Temperature = temp,
                MaxTempRecorded = maxTemp,
                BattaryChargerCurrent = chargerCurrent,
                TotalMinutesON = totalMinutes,
                UsedTimesCounter = usedTimes,
                DepletionCounter = depletionCount,
            };
            
            var result = UpsTelemetry.Parse(
                systemDataTokens: null,
                batteryDataTokens: batteryDataTokens,
                alarmsDataTokens: null,
                inputDataTokens: null,
                outputDataTokens: null,
                circuitDataTokens: null,
                unitId: null);

            
            result.AssertAllPublicPropertiesEqualTo(expected);
        }

        [Test]
        public void Parse_With_AlarmsDataTokens_With_WrongFirstToken(
            [Values("ALLM", "test", "wrong")] string wrongFirstToken,
            [Values("0", "1")] string otherToken)
        {
            var alarmsDataTokens = Enumerable.Repeat(otherToken, 28).ToArray();
            alarmsDataTokens[0] = wrongFirstToken;

            // Empty (not set any fields)
            var expected = new UpsTelemetry();
            
            var result = UpsTelemetry.Parse(
                systemDataTokens: null,
                batteryDataTokens: null,
                alarmsDataTokens: alarmsDataTokens,
                inputDataTokens: null,
                outputDataTokens: null,
                circuitDataTokens: null,
                unitId: null);
            
            result.AssertAllPublicPropertiesEqualTo(expected);
        }
        
        [TestCase("0", true)]
        [TestCase("1", false)]
        public void Parse_With_AlarmsDataTokens(string otherToken, bool expectedValue)
        {
            var alarmsDataTokens = Enumerable.Repeat(otherToken, 28).ToArray();
            alarmsDataTokens[0] = "ALM";
            
            var expected = new UpsTelemetry
            {
                BatteryHealth = expectedValue,
                BatteryNotInUse = expectedValue,
                BatteryCharged = expectedValue,
                NormalTemperature = expectedValue,
                InputInRange = expectedValue,
                OutputInRange = expectedValue,
                NotOverloaded = expectedValue,
                InverterOK = expectedValue,
                OutputsEnabled = expectedValue,
                FanOK = expectedValue,
                FuseOK = expectedValue,
                GeneralSystemFault = expectedValue,
                BackfeedRelayFault = expectedValue,
                BatteryReplacement = expectedValue,
                WiringFault = expectedValue,
            };
            
            var result = UpsTelemetry.Parse(
                systemDataTokens: null,
                batteryDataTokens: null,
                alarmsDataTokens: alarmsDataTokens,
                inputDataTokens: null,
                outputDataTokens: null,
                circuitDataTokens: null,
                unitId: null);
            
            result.AssertAllPublicPropertiesEqualTo(expected);
        }
        
        [TestCase(1, nameof(UpsTelemetry.BatteryHealth))]
        [TestCase(2, nameof(UpsTelemetry.BatteryNotInUse))]
        [TestCase(3, nameof(UpsTelemetry.BatteryCharged))]
        [TestCase(6, nameof(UpsTelemetry.NormalTemperature))]
        [TestCase(7, nameof(UpsTelemetry.InputInRange))]
        [TestCase(8, nameof(UpsTelemetry.OutputInRange))]
        [TestCase(9, nameof(UpsTelemetry.NotOverloaded))]
        [TestCase(10, nameof(UpsTelemetry.InverterOK))]
        [TestCase(13, nameof(UpsTelemetry.OutputsEnabled))]
        [TestCase(14, nameof(UpsTelemetry.FanOK))]
        [TestCase(15, nameof(UpsTelemetry.FuseOK))]
        [TestCase(16, nameof(UpsTelemetry.GeneralSystemFault))]
        [TestCase(24, nameof(UpsTelemetry.BackfeedRelayFault))]
        [TestCase(26, nameof(UpsTelemetry.BatteryReplacement))]
        [TestCase(27, nameof(UpsTelemetry.WiringFault))]
        public void Parse_With_AlarmsDataTokens_Single(int tokenIdx, string propertyName)
        {
            var alarmsDataTokens = Enumerable.Repeat("1", 28).ToArray();
            alarmsDataTokens[0] = "ALM";
            alarmsDataTokens[tokenIdx] = "0";
            
            var expected = new UpsTelemetry();
            expected.SetNotifiedPropertyValue(propertyName, true);
            
            var result = UpsTelemetry.Parse(
                systemDataTokens: null,
                batteryDataTokens: null,
                alarmsDataTokens: alarmsDataTokens,
                inputDataTokens: null,
                outputDataTokens: null,
                circuitDataTokens: null,
                unitId: null);
            
            result.AssertAllPublicPropertiesEqualTo(expected);
        }
        
        [TestCase("INPUT=0", 11.1, 22.2, 33.3, 44.4, "OUT OF RANGE")]
        [TestCase("ABCDE=0", 12.3, 23.4, 34.5, 45.6, "OUT OF RANGE")]
        [TestCase("INPUT=1", 12.1, 21.3, 24.23, 56.7, "PROPER RANGE")]
        [TestCase("ABCDE=1", 99.9, 88.8, 67.5, 78.6, "PROPER RANGE")]
        [TestCase("ABCDE=2", 1.1, 2.2, 3.3, 4.4, null)]
        [TestCase("ABCDE=A", 5.5, 6.6, 7.7, 8.8, null)]
        public void Parse_With_InputDataTokens(
            string statusToken,
            double inputFrequency, double inputVoltage, 
            double inputCurrent, double inputPower,
            string? expectedInputStatus)
        {
            var inputDataTokens = new[]
            {
                statusToken,
                inputFrequency.ToString(G.Culture),
                inputVoltage.ToString(G.Culture),
                inputCurrent.ToString(G.Culture),
                inputPower.ToString(G.Culture)
            };
            
            var expected = new UpsTelemetry
            {
                InputStatus = expectedInputStatus,
                InputFrequency = inputFrequency,
                InputVoltage = inputVoltage,
                InputCurrent = inputCurrent,
                InputPower = inputPower,
            };
            
            var result = UpsTelemetry.Parse(
                systemDataTokens: null,
                batteryDataTokens: null,
                alarmsDataTokens: null,
                inputDataTokens: inputDataTokens,
                outputDataTokens: null,
                circuitDataTokens: null,
                unitId: null);
            
            result.AssertAllPublicPropertiesEqualTo(expected);
        }
        
        [TestCase("OUT=0", 11.1, 22.2, 33.3, 44.4, 55.5, "NO OUTPUT")]
        [TestCase("ABC=0", 11.1, 22.2, 33.3, 44.4, 55.5, null)]
        [TestCase("OUT=1", 12.3, 23.4, 34.5, 45.6, 56.7, "FROM LINE")]
        [TestCase("ABC=1", 12.3, 23.4, 34.5, 45.6, 56.7, null)]
        [TestCase("OUT=2", 12.3, 23.4, 34.5, 45.6, 56.7, null)]
        [TestCase("ABC=2", 12.3, 23.4, 34.5, 45.6, 56.7, null)]
        [TestCase("OUT=3", 12.1, 21.3, 24.23, 56.7, 32.7, "FROM BATTERY")]
        [TestCase("ABC=3", 12.1, 21.3, 24.23, 56.7, 32.7, null)]
        [TestCase("OUT=4", 99.9, 88.8, 67.5, 78.6, 12.1, "FIRST BOOST TAP")]
        [TestCase("ABC=4", 99.9, 88.8, 67.5, 78.6, 12.1, null)]
        [TestCase("OUT=5", 1.1, 2.2, 3.3, 4.4, 5.5, "SECOND BOOST TAP")]
        [TestCase("ABC=5", 1.1, 2.2, 3.3, 4.4, 5.5, null)]
        [TestCase("OUT=6", 5.5, 6.6, 7.7, 8.8, 9.9, "BUCK TAP")]
        [TestCase("ABC=6", 5.5, 6.6, 7.7, 8.8, 9.9, null)]
        [TestCase("OUT=7", 50.05, 60.06, 70.07, 80.08, 90.09, null)]
        [TestCase("OUT=A", 50.05, 60.06, 70.07, 80.08, 90.09, null)]
        public void Parse_With_OututDataTokens(
            string statusToken,
            double outputFrequency, double outputMeasuredVoltage, 
            double outputMeasuredCurrent, double outputPower, double outputLoad,
            string? expectedOutputStatus)
        {
            var outputDataTokens = new[]
            {
                statusToken,
                outputFrequency.ToString(G.Culture),
                outputMeasuredVoltage.ToString(G.Culture),
                outputMeasuredCurrent.ToString(G.Culture),
                outputPower.ToString(G.Culture),
                outputLoad.ToString(G.Culture)
            };
            
            var expected = new UpsTelemetry
            {
                OutputStatus = expectedOutputStatus,
                OutputFrequency = outputFrequency,
                OutputMeasuredVoltage = outputMeasuredVoltage,
                OutputMeasuredCurrent = outputMeasuredCurrent,
                OutputPower = outputPower,
                OutputLoad = outputLoad,
            };
            
            var result = UpsTelemetry.Parse(
                systemDataTokens: null,
                batteryDataTokens: null,
                alarmsDataTokens: null,
                inputDataTokens: null,
                outputDataTokens: outputDataTokens,
                circuitDataTokens: null,
                unitId: null);
            
            result.AssertAllPublicPropertiesEqualTo(expected);
        }
        
        [Test]
        public void RetrieveCircuitData()
        {
            var circuitDataTokens = new string[][] { };
            
            var expected = new UpsTelemetry();
            
            var result = UpsTelemetry.Parse(
                systemDataTokens: null,
                batteryDataTokens: null,
                alarmsDataTokens: null,
                inputDataTokens: null,
                outputDataTokens: null,
                circuitDataTokens: circuitDataTokens,
                unitId: null);
            
            result.AssertAllPublicPropertiesEqualTo(expected);
        }
        
        [TestCase("123-456", "123", "456")]
        [TestCase("Model-Serial", "Model", "Serial")]
        [TestCase("", null, null)]
        [TestCase("123", null, null)]
        [TestCase("123-456-789", null, null)]
        public void Parse_With_UnitId(
            string unitToken,
            string? expectedModel, string? expectedSerial)
        {
            var unitId = new string[8];
            unitId[7] = unitToken;
            
            var expected = new UpsTelemetry
            {
                Model = expectedModel,
                Serial = expectedSerial,
            };
            
            var result = UpsTelemetry.Parse(
                systemDataTokens: null,
                batteryDataTokens: null,
                alarmsDataTokens: null,
                inputDataTokens: null,
                outputDataTokens: null,
                circuitDataTokens: null,
                unitId: unitId);
            
            result.AssertAllPublicPropertiesEqualTo(expected);
        }
        
        [Test]
        public void GetUPSState_True(
            [Values(220, 240, 220-22, 240+24)] double inputVoltage, 
            [Values(40, 70, 100)] double batteryChargedPercent)
        {
            var telemetry = new UpsTelemetry
            {
                InputVoltage = inputVoltage,
                BatteryChargedPercent = batteryChargedPercent,
            };
            
            Assert.That(UpsTelemetry.GetUPSState(telemetry), Is.True);
        }
        
        [Test]
        public void GetUPSState_False_ByVoltage(
            [Values(220-22-0.001, 240+24+0.001)] double inputVoltage, 
            [Values(40, 70, 100)] double batteryChargedPercent)
        {
            var telemetry = new UpsTelemetry
            {
                InputVoltage = inputVoltage,
                BatteryChargedPercent = batteryChargedPercent,
            };
            
            Assert.That(UpsTelemetry.GetUPSState(telemetry), Is.False);
        }
        
        [Test]
        public void GetUPSState_False_ByBattery(
            [Values(220, 240, 220-22, 240+24)] double inputVoltage, 
            [Values(0, 10, 39.999)] double batteryChargedPercent)
        {
            var telemetry = new UpsTelemetry
            {
                InputVoltage = inputVoltage,
                BatteryChargedPercent = batteryChargedPercent,
            };
            
            Assert.That(UpsTelemetry.GetUPSState(telemetry), Is.False);
        }
    }
}
