using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Domain.DataManagement.System.Physics;
using Heracles.Application.Models.CollimatorConfiguration.CSV.Types;
using Heracles.Core.Enums;
using Heracles.Core.Models;
using Heracles.Core.Models.RDBMS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xcc.Application.Domain.System;
using Xcc.Core.Domain.DataManagement.System;
using Xcc.Infra.Persistence.CSV;

namespace Heracles.Application.Models.CollimatorConfiguration.CSV
{
    public class CsvConfiguration
    {
        public const string COLLIMATOR_TABLE = "Collimator";
        public const string PRESET_CONFIGURATION_TABLE = "PresetConfiguration";
        public const string COIL_CONFIGURATION_TABLE = "CoilConfiguration";
        public const string CORRECTION_MATRIX_TABLE = "CorrectionMatrix";
        public const string REFERENCE_FIELD_TABLE = "ReferenceField";
        public const string HEATER_CURRENT_TABLE = "HeaterCurrentConfig";
        public const string OUTPUT_FACTOR_TABLE = "OutputFactor";

        public ICollimatorWithPresets Collimator { get; set; }
        public IPresetConfiguration Preset { get; set; }
        public ICollection<ICoilConfigurationEntry>? CoilConfigurations { get; set; }
        public ICollection<ICorrectionMatrix>? CorrectionMatrixEntries { get; set; }
        public ICollection<IReferenceField>? ReferenceFieldEntries { get; set; }
        public IHeaterCurrentConfig? HeaterCurrentConfig { get; set; }
        public ICollection<IOutputFactor>? OutputFactorEntries { get; set; }
        public CsvConfiguration() { }

        public void ReadCsv(StreamReader csvStreamReader)
        {
            using (var csvReader = new CsvReader(csvStreamReader))
            {
                while (true)
                {
                    string tableName = csvReader.SeekTableName();
                    if (tableName == null) break;
                    try
                    {
                        switch (tableName)
                        {
                            case COLLIMATOR_TABLE:
                                Collimator = ReadCollimatorTable(csvReader);
                                break;
                            case PRESET_CONFIGURATION_TABLE:
                                Preset = ReadPresetConfigurationTable(csvReader);
                                break;
                            case COIL_CONFIGURATION_TABLE:
                                CoilConfigurations = ReadCoilConfigurationsTable(csvReader);
                                break;
                            case CORRECTION_MATRIX_TABLE:
                                CorrectionMatrixEntries = ReadCorrectionMatrixTable(csvReader);
                                break;
                            case REFERENCE_FIELD_TABLE:
                                ReferenceFieldEntries = ReadReferenceFieldsTable(csvReader);
                                break;
                            case HEATER_CURRENT_TABLE:
                                HeaterCurrentConfig = ReadHeatCurrentConfigTable(csvReader);
                                break;
                            case OUTPUT_FACTOR_TABLE:
                                OutputFactorEntries = ReadOutputFactorsTable(csvReader);
                                break;
                            default:
                                throw new InvalidDataException($"Unknow CSV calibration table name: {tableName}");
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new CsvWrongFormatException($"{tableName} table read error.\n{ex.Message}.");
                    }
                }
            }
        }

        private ICollimatorWithPresets ReadCollimatorTable(CsvReader csvReader)
        {
            var table = csvReader.ReadTable<CsvCollimator>();
            if (table.Count != 1)
            {
                throw new InvalidDataException("Collimator table must contain exactly one applicator record");
            }
            else
            {
                var csvCollimator = table.First();
                return new CollimatorWithPresets(
                    new Collimator
                    {
                        Serial = csvCollimator.Serial,
                        IsActive = csvCollimator.IsActive.Value
                    },
                    new Domain.DataManagement.System.Collimators.CollimatorConfiguration
                    {
                        Type = csvCollimator.Type.Value,
                        ReferencedDoseRate = csvCollimator.DoseRate,
                        Energy = csvCollimator.Energy.Value,
                        SsdType = (csvCollimator.Type.Value == TargetType.TargetType_30mm_SSD_7_Fields) ? SsdType.SsdType30mm : SsdType.SsdType50mm
                    });                
            }
        }

        private IPresetConfiguration ReadPresetConfigurationTable(CsvReader csvReader)
        {
            var table = csvReader.ReadTable<CsvPreset>();
            if (table.Count != 1)
            {
                throw new InvalidDataException("PresetConfiguration table must contain exactly one preset record");
            }
            else
            {
                var csvPreset = table.First();
                return new PresetConfiguration()
                {
                    PresetName = csvPreset.PresetName,
                    IsDefault = csvPreset.IsDefault.Value,
                    IsActive = csvPreset.IsActive.Value
                };
            }
        }

        private ICollection<ICoilConfigurationEntry> ReadCoilConfigurationsTable(CsvReader csvReader)
        {
            var table = csvReader.ReadTable<CsvCoilConfiguration>();

            return table.Select(
                x =>
                {
                    ICoilConfigurationEntry coilConfiguration = new CoilConfigurationEntry()
                    {
                        FieldName = x.FieldName.Value,
                        XDeflectionCurrent = x.XDeflectionCurrent,
                        YDeflectionCurrent = x.YDeflectionCurrent,
                        FocusCurrent = x.FocusCurrent
                    };

                    return coilConfiguration;
                }).ToList();
        }

        private ICollection<ICorrectionMatrix> ReadCorrectionMatrixTable(CsvReader csvReader)
        {
            var table = csvReader.ReadTable<CsvCorrectionMatrix>();

            return table.Select(
                x => (ICorrectionMatrix)new CorrectionMatrix()
                {
                    MagnetometerType = x.MagnetometerType.Value,
                    Cm11 = x.CM11,
                    Cm12 = x.CM12,
                    Cm13 = x.CM13,
                    Cm21 = x.CM21,
                    Cm22 = x.CM22,
                    Cm23 = x.CM23,
                }).ToList();
        }

        private ICollection<IReferenceField> ReadReferenceFieldsTable(CsvReader csvReader)
        {
            var table = csvReader.ReadTable<CsvReferenceField>();

            return table.Select(
                x => (IReferenceField)new ReferenceField()
                {
                    MagnetometerType = x.MagnetometerType.Value,
                    Rf11 = x.RF11, Rf21 = x.RF21, Rf31 = x.RF31,
                }).ToList();
        }

        private IHeaterCurrentConfig ReadHeatCurrentConfigTable(CsvReader csvReader)
        {
            var table = csvReader.ReadTable<CsvHeaterCurrent>();
            if (table.Count != 1)
            {
                throw new InvalidDataException("HeaterCurrent table must contain exactly one record");
            }
            else
            {
                var csvHeatCurrent = table.First();
                return new HeaterCurrentConfig()
                {
                    HeaterCurrent = csvHeatCurrent.HeaterCurrent,
                };
            }
        }

        private ICollection<IOutputFactor> ReadOutputFactorsTable(CsvReader csvReader)
        {
            var table = csvReader.ReadTable<CsvOutputFactor>();

            return table.Select(
                x => (IOutputFactor)new OutputFactor()
                {
                    FieldName = x.Field.Value,
                    Factor = x.Factor
                }).ToList();
        }
    }
}