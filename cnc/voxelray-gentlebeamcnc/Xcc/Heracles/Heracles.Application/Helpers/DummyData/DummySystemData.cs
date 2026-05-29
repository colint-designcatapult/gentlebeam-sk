using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Domain.DataManagement.System.Physics;
using Heracles.Application.Domain.DataManagement.System.QualityCheck;
using Heracles.Application.Infra.DataManagement.System.DataAccess;
using Heracles.Application.Models.CollimatorConfiguration;
using Heracles.Application.Models.CollimatorConfiguration.CSV.Types;
using Heracles.Application.Models.QualityCheck;
using Heracles.Core.Enums;
using Heracles.Core.Models;
using Heracles.Core.Models.RDBMS;
using Xcc.Application.Common;
using Xcc.Application.Domain.QualityAssurance;
using Xcc.Application.Domain.System;
using Xcc.Core.Domain.DataManagement.System;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using Xcc.Infra.Persistence.CSV;

namespace Heracles.Application.Helpers.DummyData
{
    public class DummySystemData
    {
        public static IDictionary<Energy, double> DoseRateMap { get; } = 
            new Dictionary<Energy, double> {
                    { Energy.Energy_50, 600 },
                    { Energy.Energy_70, 600 },
                    { Energy.Energy_100, 600 }};

        public DummySystemData(
            ICollimatorCommands collimatorCommands,
            IOutputFactorCommands outputFactorCommands,
            ILogWriter logWriter,
            IPresetConfigurationCommands presetConfigurationCommands,
            ICoilConfigurationCommands coilConfigurationCommands,
            ICorrectionMatrixCommands correctionMatrixCommands,
            IReferenceFieldCommands referenceFieldCommands,
            IHeaterCurrentConfigCommands heaterCurrentConfigCommands,
            ICollimatorConfigurationCommands collimatorConfigurationCommands,
            IHeadCommands headCommands,
            ISafetyCheckCommands safetyCheckCommands,
            IQcSampleCommands qcSampleCommands,
            IQcSampleFieldCommands qcSampleFieldCommands,
            IIntensityCommands intensityCommands
            )
        {
            CollimatorCommands = collimatorCommands;
            OutputFactorCommands = outputFactorCommands;
            LogWriter = logWriter;
            PresetConfigurationCommands = presetConfigurationCommands;
            CoilConfigurationCommands = coilConfigurationCommands;
            CorrectionMatrixCommands = correctionMatrixCommands;
            ReferenceFieldCommands = referenceFieldCommands;
            HeaterCurrentConfigCommands = heaterCurrentConfigCommands;
            CollimatorConfigurationCommands = collimatorConfigurationCommands;
            HeadCommands = headCommands;
            SafetyCheckCommands = safetyCheckCommands;
            QcSampleFieldCommands = qcSampleFieldCommands;
            IntensityCommands = intensityCommands;
            QcSampleCommands = qcSampleCommands;
        }

        #region Properties
        public ICollimatorCommands CollimatorCommands { get; }
        public ICollimatorConfigurationCommands CollimatorConfigurationCommands { get; }
        public IHeadCommands HeadCommands { get; }
        public ISafetyCheckCommands SafetyCheckCommands { get; }
        public IQcSampleCommands QcSampleCommands { get; }
        public IQcSampleFieldCommands QcSampleFieldCommands { get; }
        public IIntensityCommands IntensityCommands { get; }
        public IOutputFactorCommands OutputFactorCommands { get; }
        public ILogWriter LogWriter { get; }
        public IPresetConfigurationCommands PresetConfigurationCommands { get; }
        public ICoilConfigurationCommands CoilConfigurationCommands { get; }
        public ICorrectionMatrixCommands CorrectionMatrixCommands { get; }
        public IReferenceFieldCommands ReferenceFieldCommands { get; }
        public IHeaterCurrentConfigCommands HeaterCurrentConfigCommands { get; }
        #endregion Properties

        public void PopulateDB()
        {
            try
            {
                if (!DBIsEmpty())
                    return;

                // Add collimators:
                long collimatorIndex = 0;
                TargetType[] collimatorTypes =
                [
                    //TargetType.TargetType_50mm_SSD_13_Fields, 
                    //TargetType.TargetType_30mm_SSD_7_Fields, 
                    TargetType.TargetType_QC_Collimator,
                    TargetType.TargetType_50mm_SSD_15mm_Field,
                    TargetType.TargetType_50mm_SSD_20mm_Field,
                    TargetType.TargetType_50mm_SSD_30mm_Field,
                    TargetType.TargetType_50mm_SSD_40mm_Field,
                    TargetType.TargetType_50mm_SSD_50mm_Field
                ];

                const string performedBy = "admin@admin.com";

                var head = HeadCommands.CreateAsync(new Head
                {
                    CreationDate = DateTime.Now,
                    IsActive = true,
                    Serial = "11110"
                }).GetAwaiter().GetResult();

                ulong qcCollimatorSerial = 0xCDCD1234CDCD1234;
                var collimatorSerialDictQc = new Dictionary<Energy, ulong>()
                {
                    { Energy.Energy_50, qcCollimatorSerial },
                };

                var collimatorSerialDict13 = new Dictionary<Energy, ulong>() {
                    { Energy.Energy_50, 0x78563412EFCDAB90 },
                    //{ Energy.Energy_50, 0x1234567890ABCDEF },
                    { Energy.Energy_70, 0xFEEDFACEDEADBEEF },
                    //{ Energy.Energy_100, 0x0001000200030004 }
                };

                var collimatorSerialDict7 = new Dictionary<Energy, ulong>() {
                    { Energy.Energy_50, 0x1234567890ABCD00 },
                    //{ Energy.Energy_70, 0xFEEDFACEDEADBE00 },
                    //{ Energy.Energy_100, 0x0001000200030004 }
                };

                var collimatorSerialDict15 = new Dictionary<Energy, ulong>() {
                    { Energy.Energy_50,  0x1111111111111111 },
                    { Energy.Energy_70,  0x2222222222222222 },
                    { Energy.Energy_100, 0x3333333333333333 },
                };

                var collimatorSerialDict20 = new Dictionary<Energy, ulong>() {
                    { Energy.Energy_50, 0x4444444444444444 },
                    { Energy.Energy_70, 0x5555555555555555 },
                    { Energy.Energy_100, 0x6666666666666666 },
                };

                var collimatorSerialDict30 = new Dictionary<Energy, ulong>() {
                    { Energy.Energy_50, 0x7777777777777777 },
                    { Energy.Energy_70, 0x8888888888888888 },
                    { Energy.Energy_100, 0x9999999999999999 },
                };

                var collimatorSerialDict40 = new Dictionary<Energy, ulong>() {
                    { Energy.Energy_50, 0xAAAAAAAAAAAAAAAA },
                    { Energy.Energy_70, 0xBBBBBBBBBBBBBBBB },
                    { Energy.Energy_100, 0xCCCCCCCCCCCCCCCC },
                };

                var collimatorSerialDict50 = new Dictionary<Energy, ulong>() {
                    { Energy.Energy_50, 0xDDDDDDDDDDDDDDDD },
                    { Energy.Energy_70, 0xEEEEEEEEEEEEEEEE },
                    { Energy.Energy_100, 0xFFFFFFFFFFFFFFFF },
                };


                foreach (var collimatorType in collimatorTypes)
                {
                    var collimatorSerialDict = collimatorType switch
                    {
                        TargetType.TargetType_30mm_SSD_7_Fields => collimatorSerialDict7,
                        TargetType.TargetType_50mm_SSD_13_Fields => collimatorSerialDict13,
                        TargetType.TargetType_50mm_SSD_15mm_Field => collimatorSerialDict15,
                        TargetType.TargetType_50mm_SSD_20mm_Field => collimatorSerialDict20,
                        TargetType.TargetType_50mm_SSD_30mm_Field => collimatorSerialDict30,
                        TargetType.TargetType_50mm_SSD_40mm_Field => collimatorSerialDict40,
                        TargetType.TargetType_50mm_SSD_50mm_Field => collimatorSerialDict50,
                        TargetType.TargetType_QC_Collimator => collimatorSerialDictQc,
                        _ => null,
                    };

                    foreach (var energyValue in Enum.GetValues<Energy>())
                    {
                        if (collimatorSerialDict is null || !collimatorSerialDict.ContainsKey(energyValue))
                            continue;

                        var collimatorConfiguration = CollimatorConfigurationCommands.CreateAsync(new CollimatorConfiguration
                        {
                            CreationDate = DateTime.Now,
                            Energy = energyValue,
                            ReferencedDoseRate = DoseRateMap[energyValue],
                            SsdType = collimatorType == TargetType.TargetType_30mm_SSD_7_Fields ? SsdType.SsdType30mm : SsdType.SsdType50mm,
                            Type = collimatorType
                        }).GetAwaiter().GetResult();

                        var collimator = CollimatorCommands.CreateAsync(new Collimator
                        {
                            CreationDate = DateTime.Now,
                            IsActive = true,
                            //Serial = "1111" + (collimatorIndex++).ToString(),
                            Serial = collimatorSerialDict[energyValue].ToString("X"),
                            CollimatorConfigurationId = collimatorConfiguration.Id,
                            HeadId = head.Id
                        }).GetAwaiter().GetResult();

                        collimator.Configuration = collimatorConfiguration;
                        
                        //if (referencedQcSample)
                        //{
                        //    referencedQcSample = false;
                        //}

                        // Make a preset
                        IPresetConfiguration preset = new PresetConfiguration
                        {
                            CollimatorConfigurationId = collimatorConfiguration.Id,
                            IsActive = true,
                            CreationDate = DateTime.Now,
                            PresetName = "preset_" + collimator.Id.ToString(),
                            IsDefault = true
                        };
                        // Make a set of coil configurations
                        var fieldMapping = TargetTypeConverter.GetIndexToTreatmentFieldNameMapping(collimatorType);
                        ICollection<ICoilConfigurationEntry> coilConfigurations = new List<ICoilConfigurationEntry>();

                        foreach (var field in fieldMapping.Values)
                        {
                            var entry = new CoilConfigurationEntry
                            {
                                FieldName = field,
                                PresetConfigurationId = preset.Id,
                                XDeflectionCurrent = 0.0,
                                YDeflectionCurrent = 0.0,
                                FocusCurrent = 0.0
                            };

                            coilConfigurations.Add(entry);
                        }

                        // Make a set of qcSampleFields
                        AddQcReportsAsync(performedBy, collimatorConfiguration, fieldMapping).GetAwaiter().GetResult();

                        double heaterCurrent = 3000;
                        var heaterCurrentConfig = new HeaterCurrentConfig() { HeaterCurrent = heaterCurrent };

                        // Make a set of magnetometer configurations
                        // consisting of correction matrices and reference field currents
                        ICollection<ICorrectionMatrixEntry> correctionMatrixEntries = new List<ICorrectionMatrixEntry>();
                        ICollection<IReferenceFieldEntry> referenceFieldEntries = new List<IReferenceFieldEntry>();
                        MagnetometerType[] magnetometers = { MagnetometerType.Front, MagnetometerType.Back };
                        foreach (var magnetometerType in magnetometers)
                        {
                            correctionMatrixEntries.Add(
                                new CorrectionMatrixEntry()
                                {
                                    MagnetometerType = magnetometerType,
                                    PresetConfigurationId = preset.Id,
                                    CreationDate = DateTime.Now,
                                    Cm11 = 0,
                                    Cm12 = 0,
                                    Cm13 = 0,
                                    Cm21 = 0,
                                    Cm22 = 0,
                                    Cm23 = 0
                                });

                            referenceFieldEntries.Add(
                                new ReferenceFieldEntry()
                                {
                                    MagnetometerType = magnetometerType,
                                    CreationDate = DateTime.Now,
                                    Rf11 = 0,
                                    Rf21 = 0,
                                    Rf31 = 0
                                });
                        }

                        // Make output factors list:
                        ICollection<IOutputFactor> outputFactors = new List<IOutputFactor>();
                        // We set 13 cell collimator factors other than 1.0
                        // so that treatment fields dummy data for these collimators would have inconsistent calculated dose
                        double outputFactorValue = 1d;
                        foreach (var field in fieldMapping)
                        {
                            outputFactors.Add(new OutputFactor
                            {
                                CreationDate = DateTime.Now,
                                Factor = outputFactorValue,
                                FieldName = field.Value,
                            });
                        }


                        //if (energyValue != Energy.Energy_100) 
                        {
                            // For the goal of preset auto-creation testing,
                            // we do not make a preset & values in the DB for 50mm & 70kV Energy collimators

                            // Store the preset itself
                            preset = PresetConfigurationCommands.CreateAsync(preset).GetAwaiter().GetResult();

                            if (collimatorType == TargetType.TargetType_50mm_SSD_50mm_Field &&
                                energyValue == Energy.Energy_70)
                            {
                                continue;
                            }

                            // Store all the coil configuration fields
                            foreach (var c in coilConfigurations)
                            {
                                c.PresetConfigurationId = preset.Id;
                                CoilConfigurationCommands.CreateAsync(c).GetAwaiter().GetResult();
                            }

                            // Store magnetometer settings
                            foreach (var cm in correctionMatrixEntries)
                            {
                                cm.PresetConfigurationId = preset.Id;
                                CorrectionMatrixCommands.CreateAsync(cm).GetAwaiter().GetResult();
                            }
                            foreach (var rf in referenceFieldEntries)
                            {
                                rf.PresetConfigurationId = preset.Id;
                                ReferenceFieldCommands.CreateAsync(rf).GetAwaiter().GetResult();
                            }

                            // Store heater current
                            heaterCurrentConfig.PresetConfigurationId = preset.Id;
                            HeaterCurrentConfigCommands.CreateAsync(heaterCurrentConfig).GetAwaiter().GetResult();

                            // Store output factors
                            foreach (var of in outputFactors)
                            {
                                of.PresetConfigurationId = preset.Id;
                                OutputFactorCommands.CreateAsync(of).GetAwaiter().GetResult();
                            }

                            //if (energyValue != Energy.Energy_50) // do not approve for tests
                                PresetConfigurationCommands.ApproveAsync(preset.Id, "Admin", "password").GetAwaiter().GetResult();
                        }

                        if (energyValue == Energy.Energy_50)
                        {
                            // Write 50kV preset to a calibration CSV file to be able to export it later:
                            string collimatorTypeName = collimatorType.GetAttribute<DisplayAttribute>()!.Name!;
                            string separatorLine = ",,,,,,";
                            string filename = $"heracles-calibration-{collimatorTypeName}-{(int)energyValue}kV.csv";
                            using (var csvFileStream = new StreamWriter(filename))
                            {
                                using (var csvWriter = new CsvWriter(csvFileStream))
                                {
                                    csvWriter.WriteTable("Collimator", new List<CsvCollimator> { new CsvCollimator(collimator) });

                                    csvFileStream.WriteLine(separatorLine);
                                    csvFileStream.WriteLine(separatorLine);

                                    csvWriter.WriteTable("PresetConfiguration", new List<CsvPreset> { new CsvPreset(preset) });

                                    csvFileStream.WriteLine(separatorLine);
                                    csvFileStream.WriteLine(separatorLine);

                                    csvWriter.WriteTable("CoilConfiguration", coilConfigurations.Select(x => new CsvCoilConfiguration(x)).ToList());

                                    csvFileStream.WriteLine(separatorLine);
                                    csvFileStream.WriteLine(separatorLine);

                                    csvWriter.WriteTable(
                                        "CorrectionMatrix",
                                        correctionMatrixEntries.Select(x => new CsvCorrectionMatrix(x)).ToList());

                                    csvFileStream.WriteLine(separatorLine);
                                    csvFileStream.WriteLine(separatorLine);

                                    csvWriter.WriteTable(
                                        "ReferenceField",
                                        referenceFieldEntries.Select(x => new CsvReferenceField(x)).ToList());

                                    csvFileStream.WriteLine(separatorLine);
                                    csvFileStream.WriteLine(separatorLine);

                                    csvWriter.WriteTable("HeaterCurrentConfig", new List<CsvHeaterCurrent> { new CsvHeaterCurrent(heaterCurrent) });

                                    csvFileStream.WriteLine(separatorLine);
                                    csvFileStream.WriteLine(separatorLine);

                                    csvWriter.WriteTable("OutputFactor", outputFactors.Select(x => new CsvOutputFactor(x)).ToList());
                                }
                            }
                            // Check that everything gets read back without any errors:
                            //CsvConfiguration csvConfiguration = new CsvConfiguration();
                            //csvConfiguration.ReadCsv(filename);
                        }
                    }
                }

                AddSafetyCheckRecordsAsync(performedBy).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Task.Run(() => LogWriter.Log(
                    $"Failed to PopulateDB. {ex.Message}. {ex.InnerException?.Message}", 
                    Xcc.Core.Enums.LogRecordSeverity.Error, 
                    Xcc.Core.Enums.LogRecordType.Error));
            }
        }

        private async Task AddQcReportsAsync(string performedBy, ICollimatorConfiguration collimatorConfiguration, IDictionary<int, TreatmentFieldName> fieldMapping)
        {
            bool referencedQcSample = true;
            double qcSampleFieldIntensity = 200.0;
            for (int sampleIdx = 0; sampleIdx < 2; ++sampleIdx)
            {
                var qcSample = await QcSampleCommands.CreateAsync(new QcSampleHeader
                {
                    CollimatorConfigurationId = collimatorConfiguration.Id,
                    CreationDate = DateTime.Now.AddDays(-1 + sampleIdx),
                    Duration = 12.34f,
                    HeaterCurrent = 2000.0f,
                    PerformedBy = performedBy,
                    EmissionCurrent = (float)CurrentCalculator.CalculateCurrent(collimatorConfiguration.Energy),
                    Referenced = referencedQcSample
                });


                foreach (var field in fieldMapping.Values)
                {
                    if (referencedQcSample && field == TreatmentFieldName.PlusC && fieldMapping.Count > 1)
                    {
                        // For test purposes, we skip central field in the reference sample (except for 1-cell collimators),
                        // just to check what happens if data is incomplete
                        continue;
                    }
                    var qcSampleField = await QcSampleFieldCommands.CreateAsync(new QcSampleField
                    {
                        CreationDate = DateTime.Now,
                        QcSampleId = qcSample.Id,
                        Name = field
                    });

                    for (int diodeIndex = 0; diodeIndex < 5; ++diodeIndex)
                    {
                        var intensity = await IntensityCommands.CreateAsync(new Intensity
                        {
                            QcSampleFieldId = qcSampleField.Id,
                            DiodeName = Intensity.GetDiodeName(diodeIndex),
                            IntensityValue = qcSampleFieldIntensity + Random.Shared.NextDouble() * 4 - 2 //+-2, i.e. +-1% deviation at max
                        });
                    }
                }
                referencedQcSample = false; // next sample will not be referenced
            }
        }

        private async Task AddSafetyCheckRecordsAsync(string performedBy)
        {
            for (int dayShift = -3; dayShift <= 0; ++dayShift)
            {
                var safetyCheck = await SafetyCheckCommands.CreateAsync(new SafetyCheck
                {
                    CreationDate = DateTime.Now.AddDays(dayShift),
                    Duration = 60,
                    Energy = Energy.Energy_50,
                    Dose = 123.4f,
                    DoorInterlock = true,
                    EStop = true,
                    LiveAudio = true,
                    LiveVideo = true,
                    SStop = true,
                    XRayLight = true,
                    XRaySound = true,
                    PerformedBy = performedBy
                });
            }
        }

        private double randomInRange(Random random, double minValue, double maxValue)
        {
            return (maxValue - minValue) * random.NextDouble() + minValue;
        }

        private bool DBIsEmpty()
        {
            var list = CollimatorConfigurationCommands.ReadAllAsync().GetAwaiter().GetResult();
            return list == null || list.Count == 0;
        }
    }
}
