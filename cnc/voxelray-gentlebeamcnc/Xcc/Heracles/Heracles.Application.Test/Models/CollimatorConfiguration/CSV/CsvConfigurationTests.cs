using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Domain.DataManagement.System.Physics;
using Heracles.Application.Models.CollimatorConfiguration;
using Heracles.Application.Models.CollimatorConfiguration.CSV;
using Heracles.Application.Models.CollimatorConfiguration.CSV.Types;
using Heracles.Core.Enums;
using Heracles.Core.Models;
using Heracles.Core.Models.RDBMS;
using System.Globalization;
using Xcc.Application.Domain.System;
using Xcc.Core.Domain.DataManagement.System;
using Xcc.Core.Enums;
using Xcc.Infra.Persistence.CSV;

namespace Heracles.Application.Test.Models.CollimatorConfiguration.CSV
{
    internal class CsvConfigurationTests
    {
        MemoryStream _memoryStream;
        StreamReader _streamReader;
        StreamWriter _streamWriter;
        CsvWriter _csvWriter;
        CsvConfiguration _csvConfiguration;

        static ICollimator collimator =
            new Collimator {
                Configuration = new Domain.DataManagement.System.Collimators.CollimatorConfiguration
                {
                    ReferencedDoseRate = 0.0f,
                    Energy = Energy.Energy_100,
                    Type = TargetType.TargetType_61_Fields
                },
                IsActive = true,
                Serial = "123"
            };

        static IPresetConfiguration presetConfiguration =
            new PresetConfiguration() { CollimatorConfigurationId = 1, IsActive = false, IsDefault = true, PresetName = "Preset" };

        static ICoilConfigurationEntry coilConfigurationEntry =
            new CoilConfigurationEntry
            {
                FieldName = TreatmentFieldName.Plus4L2,
                PresetConfigurationId = 1,
                XDeflectionCurrent = 1000,
                YDeflectionCurrent = 2000,
                FocusCurrent = 0.1
            };

        static ICorrectionMatrix correctionMatrix = new CorrectionMatrix()
        {
            Cm11 = 1.1,
            Cm12 = 1.2,
            Cm13 = 1.3,
            Cm21 = 2.1,
            Cm22 = 2.2,
            Cm23 = 2.3,
            MagnetometerType = MagnetometerType.Back
        };

        static IReferenceField referenceField = new ReferenceField
        {
            MagnetometerType = MagnetometerType.Back,
            Rf11 = 1.1, Rf21 = 2.1, Rf31 = 3.1
        };

        static IHeaterCurrentConfig heaterCurrent = new HeaterCurrentConfig { HeaterCurrent = 2500, PresetConfigurationId = 1 };

        static IOutputFactor outputFactor = new OutputFactor { Factor = 1.01, FieldName = TreatmentFieldName.Plus4C, Id = 1 };

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var defaultCulture = new CultureInfo("en-US");

            Thread.CurrentThread.CurrentCulture = defaultCulture;
            Thread.CurrentThread.CurrentUICulture = defaultCulture;
            CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
            CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;
        }

        [SetUp]
        public void Setup()
        {
            _memoryStream = new();
            _streamReader = new(_memoryStream);
            _streamWriter = new(_memoryStream);
            _csvConfiguration = new CsvConfiguration();
            _csvWriter = new CsvWriter(_streamWriter);
        }

        [TearDown]
        public void Teardown()
        {
            _csvWriter.Dispose();
            _streamWriter.Dispose();
            _streamReader.Dispose();
            _memoryStream.Dispose();
        }

        [Test]
        public void ReadCollimatorTableTest()
        {
            CsvCollimator csvCollimator = new(collimator);
            _csvWriter.WriteTable(CsvConfiguration.COLLIMATOR_TABLE, new List<CsvCollimator> { csvCollimator });
            _streamWriter.Flush();
            _memoryStream.Seek(0, SeekOrigin.Begin);

            Assert.DoesNotThrow(() => _csvConfiguration.ReadCsv(_streamReader));
            Assert.That(_csvConfiguration.Collimator, Is.Not.Null);
            Assert.That(_csvConfiguration.Preset, Is.Null);
        }

        [Test]
        public void ReadCollimatorTable_NegativeTest()
        {
            CsvCollimator csvCollimator = new(collimator);
            _csvWriter.WriteTable(CsvConfiguration.COLLIMATOR_TABLE, new List<CsvCollimator> { csvCollimator, csvCollimator });
            _streamWriter.Flush();
            _memoryStream.Seek(0, SeekOrigin.Begin);

            // There should be only one record:
            Assert.Throws<CsvWrongFormatException>(() => _csvConfiguration.ReadCsv(_streamReader));
            Assert.That(_csvConfiguration.Collimator, Is.Null);
        }

        [Test]
        public void ReadPresetTableTest()
        {
            CsvPreset preset = new(presetConfiguration);
            _csvWriter.WriteTable(CsvConfiguration.PRESET_CONFIGURATION_TABLE, new List<CsvPreset> { preset });
            _streamWriter.Flush();
            _memoryStream.Seek(0, SeekOrigin.Begin);

            Assert.DoesNotThrow(() => _csvConfiguration.ReadCsv(_streamReader));
            Assert.That(_csvConfiguration.Collimator, Is.Null);
            Assert.That(_csvConfiguration.Preset, Is.Not.Null);
        }

        [Test]
        public void ReadPresetTable_NegativeTest()
        {
            CsvPreset preset = new(presetConfiguration);
            _csvWriter.WriteTable(CsvConfiguration.PRESET_CONFIGURATION_TABLE, new List<CsvPreset> { preset, preset });
            _streamWriter.Flush();
            _memoryStream.Seek(0, SeekOrigin.Begin);

            // There should be only one record:
            Assert.Throws<CsvWrongFormatException>(() => _csvConfiguration.ReadCsv(_streamReader));
            Assert.That(_csvConfiguration.Preset, Is.Null);
        }

        [Test]
        public void ReadCoilConfigrationTableTest()
        {
            CsvCoilConfiguration coilConfiguration = new(coilConfigurationEntry);
            _csvWriter.WriteTable(CsvConfiguration.COIL_CONFIGURATION_TABLE, new List<CsvCoilConfiguration> { coilConfiguration, coilConfiguration });
            _streamWriter.Flush();
            _memoryStream.Seek(0, SeekOrigin.Begin);

            Assert.DoesNotThrow(() => _csvConfiguration.ReadCsv(_streamReader));
            Assert.Multiple(() =>
            {
                Assert.That(_csvConfiguration.Collimator, Is.Null);
                Assert.That(_csvConfiguration.Preset, Is.Null);
                Assert.That(_csvConfiguration.CoilConfigurations, Is.Not.Null);
                Assert.That(_csvConfiguration.CoilConfigurations, Has.Count.EqualTo(2));
            });
        }

        [Test]
        public void ReadCorrectionMatrixTableTest()
        {
            CsvCorrectionMatrix csvCorrectionMatrix = new(correctionMatrix);
            _csvWriter.WriteTable(CsvConfiguration.CORRECTION_MATRIX_TABLE, new List<CsvCorrectionMatrix> { csvCorrectionMatrix, csvCorrectionMatrix });
            _streamWriter.Flush();
            _memoryStream.Seek(0, SeekOrigin.Begin);

            Assert.DoesNotThrow(() => _csvConfiguration.ReadCsv(_streamReader));
            Assert.Multiple(() =>
            {
                Assert.That(_csvConfiguration.Collimator, Is.Null);
                Assert.That(_csvConfiguration.CorrectionMatrixEntries, Is.Not.Null);
                Assert.That(_csvConfiguration.CorrectionMatrixEntries, Has.Count.EqualTo(2));
            });
        }

        [Test]
        public void ReadReferenceFieldTableTest()
        {
            CsvReferenceField csvReferenceField = new(referenceField);
            _csvWriter.WriteTable(CsvConfiguration.REFERENCE_FIELD_TABLE, new List<CsvReferenceField> { csvReferenceField, csvReferenceField });
            _streamWriter.Flush();
            _memoryStream.Seek(0, SeekOrigin.Begin);

            Assert.DoesNotThrow(() => _csvConfiguration.ReadCsv(_streamReader));
            Assert.Multiple(() =>
            {
                Assert.That(_csvConfiguration.Collimator, Is.Null);
                Assert.That(_csvConfiguration.ReferenceFieldEntries, Is.Not.Null);
                Assert.That(_csvConfiguration.ReferenceFieldEntries, Has.Count.EqualTo(2));
            });
        }

        [Test]
        public void ReadHeatCurrentTableTest()
        {
            CsvHeaterCurrent csvHeaterCurrent = new(heaterCurrent.HeaterCurrent!.Value);
            _csvWriter.WriteTable(CsvConfiguration.HEATER_CURRENT_TABLE, new List<CsvHeaterCurrent> { csvHeaterCurrent });
            _streamWriter.Flush();
            _memoryStream.Seek(0, SeekOrigin.Begin);

            Assert.DoesNotThrow(() => _csvConfiguration.ReadCsv(_streamReader));
            Assert.Multiple(() =>
            {
                Assert.That(_csvConfiguration.Collimator, Is.Null);
                Assert.That(_csvConfiguration.HeaterCurrentConfig, Is.Not.Null);
                Assert.That(_csvConfiguration.HeaterCurrentConfig.HeaterCurrent, Is.EqualTo(heaterCurrent.HeaterCurrent));
            });
        }

        [Test]
        public void ReadHeatCurrentTable_NegativeTest()
        {
            CsvHeaterCurrent csvHeaterCurrent = new(heaterCurrent.HeaterCurrent!.Value);
            _csvWriter.WriteTable(CsvConfiguration.HEATER_CURRENT_TABLE, new List<CsvHeaterCurrent> { csvHeaterCurrent, csvHeaterCurrent });
            _streamWriter.Flush();
            _memoryStream.Seek(0, SeekOrigin.Begin);
            
            // There should be only one record:
            Assert.Throws<CsvWrongFormatException>(() => _csvConfiguration.ReadCsv(_streamReader));
        }

        [Test]
        public void ReadOutputFactorTableTest()
        {
            CsvOutputFactor csvOutputFactor = new(outputFactor);
            _csvWriter.WriteTable(CsvConfiguration.OUTPUT_FACTOR_TABLE, new List<CsvOutputFactor> { csvOutputFactor, csvOutputFactor });
            _streamWriter.Flush();
            _memoryStream.Seek(0, SeekOrigin.Begin);

            Assert.DoesNotThrow(() => _csvConfiguration.ReadCsv(_streamReader));
            Assert.Multiple(() =>
            {
                Assert.That(_csvConfiguration.Collimator, Is.Null);
                Assert.That(_csvConfiguration.OutputFactorEntries, Is.Not.Null);
                Assert.That(_csvConfiguration.OutputFactorEntries, Has.Count.EqualTo(2));
            });
        }

        [Test]
        public void ReadUnknownTableTest()
        {
            // No matter what type of table we write, just use OutputFactors with an unsupported table name:
            CsvOutputFactor csvOutputFactor = new(outputFactor);
            _csvWriter.WriteTable<CsvOutputFactor>("UnknownTable", new List<CsvOutputFactor> { csvOutputFactor, csvOutputFactor });
            _streamWriter.Flush();
            _memoryStream.Seek(0, SeekOrigin.Begin);

            Assert.Throws<CsvWrongFormatException>(() => _csvConfiguration.ReadCsv(_streamReader));
        }
    }
}
