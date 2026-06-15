using Heracles.Robot.Models;
using Heracles.Robot.Models.Enums;
using Heracles.Robot.Models.Sequences;

namespace Heracles.Robot.Test.Models
{
    public class ActuatorTypesConverterTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void AcbActuatorId_RobotReturnOnFlangeTest()
        {
            // Arrange
            string flange = HeadActuatorName.Flange;
            // Act
            var result = ActuatorTypesConverter.AcbActuatorIdFromString(flange);
            // Assert
            Assert.That(result, Is.EqualTo(AcbActuatorId.Robot));
        }
        [Test]
        public void AcbActuatorId_ImageReturnOnImagingCradleTest()
        {
            // Arrange
            string imageCradle = HeadActuatorName.ImagingCradle;
            // Act
            var result = ActuatorTypesConverter.AcbActuatorIdFromString(imageCradle);
            // Assert
            Assert.That(result, Is.EqualTo(AcbActuatorId.Image));
        }
        [Test]
        public void AcbActuatorId_TreatmentReturnOnTreatmentCradleTest()
        {
            // Arrange
            string treatmentCradle = HeadActuatorName.TreatmentCradle;
            // Act
            var result = ActuatorTypesConverter.AcbActuatorIdFromString(treatmentCradle);
            // Assert
            Assert.That(result, Is.EqualTo(AcbActuatorId.Treatment));
        }
        [Test]
        public void AcbActuatorId_ThrowOn_UnknownArgumentTest()
        {
            // Arrange
            string unknownArgument = "unknownArgument";
            // Act
            // Assert
            Assert.Throws<ArgumentException>(() => ActuatorTypesConverter.AcbActuatorIdFromString(unknownArgument));
        }

        [Test]
        public void AcbActuatorCommand_LockReturnOnHeadLockTest()
        {
            // Arrange
            string headLock = HeadCommandName.HeadLock;
            // Act
            var result = ActuatorTypesConverter.AcbActuatorCommandFromString(headLock);
            // Assert
            Assert.That(result, Is.EqualTo(AcbActuatorCommand.Lock));
        }
        [Test]
        public void AcbActuatorCommand_UnlockReturnOnHeadUnlockTest()
        {
            // Arrange
            string headUnLock = HeadCommandName.HeadUnlock;
            // Act
            var result = ActuatorTypesConverter.AcbActuatorCommandFromString(headUnLock);
            // Assert
            Assert.That(result, Is.EqualTo(AcbActuatorCommand.Unlock));
        }
        [Test]
        public void AcbActuatorCommand_ThrowOn_UnknownArgumentTest()
        {
            // Arrange
            string unknownArgument = "unknownArgument";
            // Act
            // Assert
            Assert.Throws<ArgumentException>(() => ActuatorTypesConverter.AcbActuatorCommandFromString(unknownArgument));
        }

    }
}