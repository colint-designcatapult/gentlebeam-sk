using Heracles.Application.AppLayer.Collimators;
using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Infra.DataManagement.System.DataAccess;
using Heracles.Application.Models.RDBMS;
using Heracles.Core.Models;
using Heracles.External.Models;
using Moq;
using NUnit.Framework;
using Xcc.Application.AppLayer.Model;
using Xcc.Core.Domain.DataManagement.Common.Users;
using Xcc.Core.Logging;
using Xcc.Infra.Services;

namespace Heracles.Outdoor.Test.Models
{
    public class SafetyCheckTests
    {
        readonly ICollimator _collimatorToReturn = new Collimator {
            Configuration = new CollimatorConfiguration { 
                Energy = Core.Enums.Energy.Energy_50,
                Type = Core.Enums.TargetType.TargetType_50mm_SSD_13_Fields
            }            
        };

        readonly IUser _user = new User
        {
            EmailAddress = "test@example.com"
        };

        readonly int _qcFieldDuration = 12345;
        readonly int _safetyCheckFieldDuration = 321;

        Mock<IHeraclesExternalSettings> fakeAppSettings = null!;
        Mock<ICollimatorModel> fakeCollimatorModel = null!;
        Mock<ISafetyCheckCommands> fakeSafetyCheckCommands = null!;
        Mock<IAuthorizedUserStore> fakeAuthorizedUserStore = null!;
        Mock<ILogWriter> fakeLogService = null!;

        [SetUp]
        public void Setup()
        {
            fakeAppSettings = new Mock<IHeraclesExternalSettings>();
            fakeCollimatorModel = new Mock<ICollimatorModel>();
            fakeSafetyCheckCommands = new Mock<ISafetyCheckCommands>();
            fakeAuthorizedUserStore = new Mock<IAuthorizedUserStore>();
            fakeLogService = new Mock<ILogWriter>();

            fakeAppSettings.Setup(cmd => cmd.QcFieldDuration).Returns(_qcFieldDuration);
            fakeAppSettings.Setup(cmd => cmd.SafetyCheckFieldDuration).Returns(_safetyCheckFieldDuration);
            fakeAuthorizedUserStore.Setup(cmd => cmd.AuthorizedUser).Returns(new User
            {
                EmailAddress = _user.EmailAddress
            });
        }

        private ISafetyCheckModel MakeModel()
        {
            return new SafetyCheckModel(
                fakeAppSettings.Object,
                fakeCollimatorModel.Object,
                fakeSafetyCheckCommands.Object,
                fakeAuthorizedUserStore.Object,
                fakeLogService.Object,
                new TestDispatcherService());
        }


        [Test]
        public void CreateBlank_CorrectDataTest()
        {
            var model = MakeModel();

            Assert.DoesNotThrow(() => model.CreateBlank());
            Assert.That(model.SafetyCheck, Is.Not.Null);
            Assert.That(model.SafetyCheck.PerformedBy, Is.EqualTo(_user.EmailAddress));
            Assert.That(model.SafetyCheck.Duration, Is.EqualTo(_safetyCheckFieldDuration));
        }

        [Test]
        public void CreateEntryCollectionAsync_ErrorPassthroughTest()
        {
            fakeCollimatorModel.Setup(cmd => cmd.ActiveCollimator).Returns<ICollimator>(null!);

            var model = MakeModel();

            Assert.Throws<Exception>(() => model.CreateEntryCollection());
            Assert.That(model.Fields, Is.Null);
        }

        [Test]
        public void CreateEntryCollectionAsync_ShouldCreateModelData()
        {      
            fakeCollimatorModel.SetupGet(cmd => cmd.ActiveCollimator).Returns(_collimatorToReturn);

            var model = MakeModel();
            model.CreateBlank();

            Assert.DoesNotThrow(() => model.CreateEntryCollection());
            Assert.That(model.Fields.Count, Is.EqualTo(1));
            Assert.That(model.SafetyCheck.Energy, Is.EqualTo(model.Fields[0].Energy));
            Assert.That(model.SafetyCheck.Energy, Is.EqualTo(_collimatorToReturn.Configuration.Energy));
        }
    }
}
