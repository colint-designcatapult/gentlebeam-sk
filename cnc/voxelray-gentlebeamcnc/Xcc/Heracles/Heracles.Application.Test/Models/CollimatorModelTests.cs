using Heracles.Application.AppLayer.Collimators;
using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Infra.DataManagement.System.DataAccess;
using Moq;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Domain.DataManagement.System;
using Xcc.Core.Exceptions;
using Xcc.Core.Logging;

namespace Heracles.Application.Test.Models
{
    [Ignore("Temporarily disabled for refactoring")]
    public class CollimatorModelTests
    {
        ICollection<ICollimator> collimatorsToReturn = new List<ICollimator>(){
            new Collimator{Id = 1, IsActive = true, CollimatorConfigurationId = 1L, Serial = "1" }, 
            new Collimator{Id = 2, IsActive = false, CollimatorConfigurationId = 2L, Serial = "2"}
        };

        ICollection<IPresetConfiguration> presetsToReturn = new List<IPresetConfiguration>(){
            new PresetConfiguration{Id = 1, CollimatorConfigurationId = 1}, new PresetConfiguration{Id = 2, CollimatorConfigurationId = 2}
        };

        ICollection<IHead> headsToReturn = new List<IHead> { new Head { Id = 1L, CreationDate = DateTime.Now, IsActive = true, Serial = "12345" } };

        private ICollimatorConfiguration MakeConfiguration()
        {
            return new Domain.DataManagement.System.Collimators.CollimatorConfiguration
            {
                Id = 1L,
                Energy = Core.Enums.Energy.Energy_50,
                SsdType = Core.Enums.SsdType.SsdType50mm,
                ReferencedDoseRate = 123.4f,
                Type = Core.Enums.TargetType.TargetType_50mm_SSD_13_Fields
            };
        }


        Mock<ICollimatorCommands> fakeCollimatorCommands;
        Mock<ICollimatorConfigurationCommands> fakeCollimatorConfigurationCommands;
        Mock<IHeadCommands> fakeHeadCommands;
        Mock<IPresetConfigurationCommands> fakePresetCommands;
        Mock<ILogWriter> fakeLogService;

        [SetUp]
        public void Setup() 
        {
            fakeCollimatorCommands = new Mock<ICollimatorCommands>();
            fakeCollimatorCommands.Setup(cmd => cmd.ReadListAsync(It.IsAny<long>())).Returns(Task.FromResult<ICollection<ICollimator>>([]));

            fakeCollimatorConfigurationCommands = new Mock<ICollimatorConfigurationCommands>();
            fakeHeadCommands = new Mock<IHeadCommands>();
            fakeHeadCommands.Setup(cmd => cmd.ReadAllAsync()).Returns(Task.FromResult<ICollection<IHead>>([]));

            fakePresetCommands = new Mock<IPresetConfigurationCommands>();
            fakePresetCommands.Setup(cmd => cmd.ReadListAsync(It.IsAny<long>())).Returns(Task.FromResult<ICollection<IPresetConfiguration>>([]));
            fakeLogService = new Mock<ILogWriter>();
        }

        private CollimatorModel MakeModel()
        {
            return new CollimatorModel();
            //return new CollimatorModel(
            //    fakeCollimatorCommands.Object,
            //    fakeCollimatorConfigurationCommands.Object,
            //    fakePresetCommands.Object,
            //    fakeHeadCommands.Object,
            //    fakeLogService.Object);
        }

        [Test]
        public void GetMatchingConfiguration_PositiveTest()
        {
            fakeHeadCommands.Setup(cmd => cmd.ReadAllAsync()).Returns(Task.FromResult(headsToReturn));
            var configuration = MakeConfiguration();
            fakeCollimatorConfigurationCommands.Setup(
                cmd => cmd.ReadAllAsync())
                .Returns(Task.FromResult((ICollection<ICollimatorConfiguration>)[configuration]));

            var model = MakeModel();
            //model.FetchCollimatorsDataAsync().GetAwaiter().GetResult();

            ICollimatorConfiguration? match = null;
            Assert.DoesNotThrow(() => match = model.FindConfigurationByType(configuration.Type, configuration.Energy));
            Assert.That(match, Is.EqualTo(configuration));
        }


        [Test]
        public void GetMatchingConfiguration_NegativeTest()
        {
            var configuration = MakeConfiguration();
            var model = MakeModel();

            ICollimatorConfiguration? match = null;
            Assert.DoesNotThrow(() => match = model.FindConfigurationByType(configuration.Type, configuration.Energy));
            Assert.That(match, Is.Null);
        }

        [Test]
        public void FetchCollimatorsAsync_ExistingDataTest()
        {
            fakeHeadCommands.Setup(cmd => cmd.ReadAllAsync()).Returns(Task.FromResult(headsToReturn));
            fakeCollimatorCommands.Setup(cmd => cmd.ReadListAsync(It.IsAny<long>())).Returns(Task.FromResult(collimatorsToReturn));
            fakePresetCommands.Setup(cmd => cmd.ReadListAsync(It.IsAny<long>())).Returns(Task.FromResult(presetsToReturn));

            fakeCollimatorConfigurationCommands.Setup(
                cmd => cmd.ReadAllAsync())
                .Returns(Task.FromResult((ICollection<ICollimatorConfiguration>)[MakeConfiguration()]));

            var model = MakeModel();
            
            //Assert.DoesNotThrowAsync(() => model.FetchCollimatorsDataAsync());
            Assert.That(model.Collimators, Is.Not.Empty);
            Assert.That(model.Collimators.Count, Is.EqualTo(collimatorsToReturn.Count));
        }

        [Test]
        public void FetchCollimatorsAsync_DoesNotResetUnknownActiveCollimator()
        {
            fakeHeadCommands.Setup(cmd => cmd.ReadAllAsync()).Returns(Task.FromResult(headsToReturn));
            fakeCollimatorCommands.Setup(cmd => cmd.ReadListAsync(It.IsAny<long>())).Returns(Task.FromResult(collimatorsToReturn));
            fakePresetCommands.Setup(cmd => cmd.ReadListAsync(It.IsAny<long>())).Returns(Task.FromResult(presetsToReturn));

            fakeCollimatorConfigurationCommands.Setup(
                cmd => cmd.ReadAllAsync())
                .Returns(Task.FromResult((ICollection<ICollimatorConfiguration>)[MakeConfiguration()]));

            var model = MakeModel();
            // Add unknown collimator before fetching data:
            string someUnknownSerial = "SomeUnseenSerial";
            model.SetActiveCollimator(someUnknownSerial);

            //Assert.DoesNotThrowAsync(() => model.FetchCollimatorsDataAsync());
            Assert.Multiple(() =>
            {
                Assert.That(model.ActiveCollimator.Serial, Is.EqualTo(someUnknownSerial));
                Assert.That(model.ActiveCollimator.Id, Is.EqualTo(-1));
            });
        }

        [Test]
        public void FetchCollimatorsAsync_UpdatesRegisteredActiveCollimator()
        {
            fakeHeadCommands.Setup(cmd => cmd.ReadAllAsync()).Returns(Task.FromResult(headsToReturn));
            fakeCollimatorCommands.Setup(cmd => cmd.ReadListAsync(It.IsAny<long>())).Returns(Task.FromResult(collimatorsToReturn));
            fakePresetCommands.Setup(cmd => cmd.ReadListAsync(It.IsAny<long>())).Returns(Task.FromResult(presetsToReturn));

            fakeCollimatorConfigurationCommands.Setup(
                cmd => cmd.ReadAllAsync())
                .Returns(Task.FromResult((ICollection<ICollimatorConfiguration>)[MakeConfiguration()]));

            var model = MakeModel();
            // Set an active collimator before fetching data:
            var registeredCollimator = collimatorsToReturn.First();
            model.SetActiveCollimator(registeredCollimator.Serial);
            
            Assert.That(model.ActiveCollimator.Id, Is.EqualTo(-1));

            //Assert.DoesNotThrowAsync(() => model.FetchCollimatorsDataAsync());
            Assert.That(model.ActiveCollimator.Id, Is.EqualTo(registeredCollimator.Id));
        }

        [Test]
        public void FetchCollimatorsAsync_ErrorPassthroughTest()
        {
            fakeHeadCommands.Setup(cmd => cmd.ReadAllAsync()).Throws<DataServiceException>();

            var model = MakeModel();

            //Assert.ThrowsAsync<DataServiceException>(() => model.FetchCollimatorsDataAsync());
            Assert.That(model.Collimators, Is.Empty);
        }


        [Test]
        public void SetActiveCollimator_UnknownCollimator()
        {
            fakeHeadCommands.Setup(cmd => cmd.ReadAllAsync()).Returns(Task.FromResult(headsToReturn));
            fakeCollimatorCommands.Setup(cmd => cmd.ReadListAsync(It.IsAny<long>())).Returns(Task.FromResult(collimatorsToReturn));
            fakePresetCommands.Setup(cmd => cmd.ReadListAsync(It.IsAny<long>())).Returns(Task.FromResult(presetsToReturn));

            fakeCollimatorConfigurationCommands.Setup(
                cmd => cmd.ReadAllAsync())
                .Returns(Task.FromResult((ICollection<ICollimatorConfiguration>)[MakeConfiguration()]));

            var model = MakeModel();

            string someUnknownSerial = "SomeUnseenSerial";
            model.SetActiveCollimator(activeCollimatorSerial: someUnknownSerial);

            Assert.Multiple(() =>
            {
                Assert.That(model.ActiveCollimator.Serial, Is.EqualTo(someUnknownSerial));
                Assert.That(model.ActiveCollimator.Id, Is.EqualTo(-1));
            });
        }

        /*
        [Test]
        public void AddCollimatorPresetAsyncTest()
        {
            fakePresetCommands.Setup(
                cmd => cmd.CreateAsync(It.IsAny<IPresetConfiguration>())
                ).Returns((IPresetConfiguration p) => Task.FromResult(p));

            var configuration = MakeConfiguration();

            var model = MakeModel();

            Assert.That(configuration.Presets, Is.Empty);

            Assert.DoesNotThrowAsync(
                () => model.AddPresetAsync(
                    configuration, "presetName",
                    isActive: false, isDefault: false
                    ));

            fakePresetCommands.Verify(cmd => cmd.CreateAsync(It.IsAny<IPresetConfiguration>()), Times.Once());
            Assert.That(configuration.Presets, Is.Not.Empty);
        }

        [Test]
        public void AddCollimatorPresetAsync_NullReferenceTest()
        {
            var model = MakeModel();

            Assert.ThrowsAsync<NullReferenceException>(
                () => model.AddPresetAsync(null!, "name", true, true)
                );
        }

        [TestCase(true)]
        [TestCase(false)]
        public void AddCollimatorDefaultPresetAsync(bool isActive)
        {
            fakePresetCommands.Setup(
                cmd => cmd.CreateAsync(It.IsAny<IPresetConfiguration>())
                ).Returns((IPresetConfiguration p) => Task.FromResult(p));

            var configuration = MakeConfiguration();
            var model = MakeModel();

            Assert.That(configuration.Presets, Is.Empty);

            Assert.DoesNotThrowAsync(
                () => model.AddDefaultPresetAsync(configuration, isActive: isActive)
                );

            fakePresetCommands.Verify(cmd => cmd.CreateAsync(It.IsAny<IPresetConfiguration>()), Times.Once());
            Assert.That(configuration.Presets, Is.Not.Empty);

            if (isActive)
            {
                Assert.That(configuration.DefaultPreset, Is.Not.Null);
            }
            else
            {
                Assert.That(configuration.DefaultPreset, Is.Null);
            }
        }
        */

        [Test]
        public void AddCollimatorAsyncTest()
        {
            var config = MakeConfiguration();
            var collimatorToAdd = new Collimator() { Id = 1L, Serial = "12354", CollimatorConfigurationId = 1L, Configuration = config };
            
            fakeCollimatorCommands.Setup(
                cmd => cmd.CreateAsync(It.IsAny<ICollimator>())
                ).Returns(Task.FromResult<ICollimator>(collimatorToAdd));
                        
            fakeHeadCommands.Setup(
                cmd => cmd.ReadAllAsync()
                ).Returns(Task.FromResult(headsToReturn));

            fakeCollimatorConfigurationCommands.Setup(
                cmd => cmd.CreateAsync(It.IsAny<ICollimatorConfiguration>())
                ).Returns(Task.FromResult(config));

            var model = MakeModel();

            //Assert.DoesNotThrowAsync(
            //    () => model.CreateCollimatorAsync(collimatorToAdd)
            //    );
            Assert.That(model.Collimators.Count, Is.EqualTo(1));
            Assert.That(model.Collimators.FirstOrDefault(c => c.Serial == collimatorToAdd.Serial), Is.Not.Null);
            fakeCollimatorCommands.Verify(cmd => cmd.CreateAsync(It.IsAny<ICollimator>()), Times.Once());
        }

        /*
        [Test]
        public void AddCollimatorAsync_NullReferenceTest()
        {
            var model = MakeModel();

            Assert.ThrowsAsync<ArgumentNullException>(
                () => model.CreateCollimatorAsync(null!)
                );
        }

        [Test]
        public void AddCollimatorAsync_NullConfigurationReferenceTest()
        {
            var model = MakeModel();

            Assert.ThrowsAsync<ArgumentNullException>(
                () => model.CreateCollimatorAsync(new Collimator { Configuration = null })
                );
        }



        [Test]
        public void AddCollimatorAsync_ExceptionOnDuplicatedEntry()
        {
            fakeHeadCommands.Setup(
                cmd => cmd.ReadAllAsync()
                ).Returns(Task.FromResult(headsToReturn));

            fakeCollimatorCommands.Setup(
                cmd => cmd.CreateAsync(It.IsAny<ICollimator>())
                ).Returns((ICollimator p) => Task.FromResult<ICollimator>(new Collimator(p)));

            var model = MakeModel();

            var collimator = new Collimator
            {
                Serial = "123",
                Configuration = new Heracles.Application.Models.RDBMS.CollimatorConfiguration() { Id = 1 }
            };

            Assert.DoesNotThrowAsync(
                () => model.CreateCollimatorAsync(collimator));
            Assert.ThrowsAsync<ArgumentException>(
                () => model.CreateCollimatorAsync(collimator));
        }
        */

        [Test]
        public void AddCollimatorAsync_CreateNewHeadAndConfiguration()
        {
            fakeHeadCommands.Setup(
                cmd => cmd.CreateAsync(It.IsAny<IHead>())
                ).Returns((IHead p) => Task.FromResult<IHead>(new Head(p)));

            fakeCollimatorCommands.Setup(
                cmd => cmd.CreateAsync(It.IsAny<ICollimator>())
                ).Returns((ICollimator p) => Task.FromResult<ICollimator>(new Collimator(p)));

            fakeCollimatorConfigurationCommands.Setup(
                cmd => cmd.CreateAsync(It.IsAny<ICollimatorConfiguration>())
                ).Returns(
                    (ICollimatorConfiguration p) => 
                        Task.FromResult<ICollimatorConfiguration>(new Domain.DataManagement.System.Collimators.CollimatorConfiguration(p))
                    );

            var model = MakeModel();

            var collimator = new Collimator
            {
                Serial = "123",
                Configuration = new Domain.DataManagement.System.Collimators.CollimatorConfiguration() { Id = BaseEntry.NEW_ENTRY_ID }
            };

            Assert.That(model.ActiveHead, Is.Null);
            //Assert.DoesNotThrowAsync(
            //    () => model.CreateCollimatorAsync(collimator));
            Assert.That(model.ActiveHead, Is.Not.Null);
            
            fakeHeadCommands.Verify(cmd => cmd.CreateAsync(It.IsAny<IHead>()), Times.Once());
            fakeCollimatorConfigurationCommands.Verify(cmd => cmd.CreateAsync(It.IsAny<ICollimatorConfiguration>()), Times.Once());
        }

        [Test]
        public void UpdateCollimatorAsync_PositiveTest()
        {
            fakeHeadCommands.Setup(
                cmd => cmd.ReadAllAsync()
                ).Returns(Task.FromResult(headsToReturn));

            fakeCollimatorCommands.Setup(cmd => cmd.ReadListAsync(It.IsAny<long>())).Returns(Task.FromResult(collimatorsToReturn));
            fakeCollimatorCommands.Setup(
                cmd => cmd.UpdateAsync(It.IsAny<ICollimator>(), It.IsAny<ICollimator>()))
                .Returns((ICollimator oldValue, ICollimator newValue) => Task.FromResult(newValue));
            
            fakeCollimatorConfigurationCommands.Setup(
                cmd => cmd.ReadAllAsync())
                .Returns(Task.FromResult((ICollection<ICollimatorConfiguration>)[MakeConfiguration()]));
            fakePresetCommands.Setup(cmd => cmd.ReadListAsync(It.IsAny<long>())).Returns(
                Task.FromResult(presetsToReturn));

            var model = MakeModel();
            //model.FetchCollimatorsDataAsync().GetAwaiter().GetResult();

            var collimatorToUpdate = new Collimator(model.Collimators.ElementAt(0));
            var newValue = new Collimator(collimatorToUpdate) { IsActive = !collimatorToUpdate.IsActive };

            ICollimator updatedValue = null!;
            //Assert.DoesNotThrowAsync(
            //    async () => updatedValue = await model.UpdateCollimatorAsync(newValue));
            Assert.That(updatedValue, Is.Not.Null);
            Assert.That(updatedValue.IsActive, Is.Not.EqualTo(collimatorToUpdate.IsActive));
        }
        /*
        [Test]
        public void UpdateCollimatorAsync_MissingCollimatorTest()
        {
            var model = MakeModel();

            var someNonPresentCollimator = new Collimator() { Id = 1, Configuration = MakeConfiguration() };

            Assert.ThrowsAsync<InvalidOperationException>(
                () => model.UpdateCollimatorAsync(someNonPresentCollimator)
                );
        }

        [Test]
        public void UpdateCollimatorAsync_NullReferenceTest()
        {
            var model = MakeModel();

            Assert.ThrowsAsync<ArgumentNullException>(
                () => model.UpdateCollimatorAsync(null!)
                );
        }

        [Test]
        public void UpdateCollimatorAsync_ConfigurationNullReferenceTest()
        {
            fakeCollimatorCommands.Setup(cmd => cmd.ReadListAsync(It.IsAny<long>())).Returns(Task.FromResult(collimatorsToReturn));
            fakeCollimatorConfigurationCommands.Setup(
                cmd => cmd.ReadAllAsync())
                .Returns(Task.FromResult((ICollection<ICollimatorConfiguration>)[]));

            var model = MakeModel();

            //model.FetchCollimatorsDataAsync().GetAwaiter().GetResult();

            var someRegisteredCollimator = new Collimator(collimatorsToReturn.Last()) { Configuration = null };

            Assert.ThrowsAsync<ArgumentNullException>(
                () => model.UpdateCollimatorAsync(someRegisteredCollimator)
                );
        }

        [Test]
        public void UpdateCollimatorAsync_NonExistentConfigurationTest()
        {
            fakeCollimatorCommands.Setup(cmd => cmd.ReadListAsync(It.IsAny<long>())).Returns(Task.FromResult(collimatorsToReturn));
            fakeCollimatorCommands.Setup(
                cmd => cmd.UpdateAsync(It.IsAny<ICollimator>(), It.IsAny<ICollimator>()))
                .Returns((ICollimator oldValue, ICollimator newValue) => Task.FromResult(newValue));

            fakeCollimatorConfigurationCommands.Setup(
                cmd => cmd.ReadAllAsync())
                .Returns(Task.FromResult((ICollection<ICollimatorConfiguration>)[]));
            fakeCollimatorConfigurationCommands.Setup(
                cmd => cmd.CreateAsync(It.IsAny<ICollimatorConfiguration>())
                ).Returns((ICollimatorConfiguration c) => Task.FromResult(c));

            var model = MakeModel();

            //model.FetchCollimatorsDataAsync().GetAwaiter().GetResult();

            var existingCollimator = collimatorsToReturn.Last();
            
            var newConfiguration = MakeConfiguration();
            newConfiguration.Energy = Core.Enums.Energy.Energy_100;
            newConfiguration.Id = BaseEntry.NEW_ENTRY_ID;
            
            var someRegisteredCollimator = new Collimator(existingCollimator) { Configuration = newConfiguration };
            ICollimator? updatedCollimator = null;
            Assert.DoesNotThrowAsync(
                async () => updatedCollimator = await model.UpdateCollimatorAsync(someRegisteredCollimator)
                );
            Assert.That(model.CollimatorConfigurations, Contains.Item(newConfiguration));
        }

        [Test]
        public void UpdateCollimatorConfigurationDoseRateAsync_NegativeTest()
        {
            var model = MakeModel();

            var configuration = MakeConfiguration();
            var newDoseRate = 0;
            Assert.ThrowsAsync<ArgumentException>(() => model.UpdateCollimatorConfigurationDoseRateAsync(configuration, newDoseRate));
        }

        [Test]
        public void UpdateCollimatorConfigurationDoseRateAsync_PositiveTest()
        {
            var configuration = MakeConfiguration();
            var newDoseRate = 0;

            fakeCollimatorCommands.Setup(cmd => cmd.ReadListAsync(It.IsAny<long>())).Returns(Task.FromResult(collimatorsToReturn));

            fakeCollimatorConfigurationCommands.Setup(
                cmd => cmd.ReadAllAsync())
                .Returns(Task.FromResult((ICollection<ICollimatorConfiguration>)[configuration]));
            fakeCollimatorConfigurationCommands.Setup(
                cmd => cmd.UpdateAsync(It.IsAny<ICollimatorConfiguration>(), It.IsAny<ICollimatorConfiguration>())
                ).Returns(
                    (ICollimatorConfiguration oldValue, ICollimatorConfiguration newValue) =>
                        Task.FromResult<ICollimatorConfiguration>(new Heracles.Application.Models.RDBMS.CollimatorConfiguration(newValue))
                    );

            // Set active collimator to verify if it will be updated on dose rate change -> configuration update
            var model = MakeModel();
            //model.FetchCollimatorsDataAsync().GetAwaiter().GetResult();
            model.SetActiveCollimator(collimatorsToReturn.First().Serial);

            bool actualCollimatorUpdated = false;
            model.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(model.ActiveCollimator))
                {
                    actualCollimatorUpdated = true;
                }
            };            

            Assert.DoesNotThrowAsync(() => model.UpdateCollimatorConfigurationDoseRateAsync(configuration, newDoseRate));
            Assert.That(actualCollimatorUpdated, Is.True);
            Assert.That(model.ActiveCollimator.Configuration.ReferencedDoseRate, Is.EqualTo(newDoseRate));
        }
        */
    }
}
