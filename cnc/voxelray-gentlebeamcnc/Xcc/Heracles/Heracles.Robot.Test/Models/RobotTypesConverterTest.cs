using Heracles.Robot.Models;
using Heracles.Robot.Models.RobotArm.Enums;
using Xcc.Application.Models.RobotArm.Enums;

namespace Heracles.Robot.Test.Models
{
    public class RobotTypesConverterTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Axis_Return_X_OnXStringTest()
        {
            // Arrange
            string inputAxisX = Axis.X.ToString();
            // Act
            var result = RobotTypesConverter.AxisFromString(inputAxisX);
            // Assert
            Assert.That(result, Is.EqualTo(Axis.X));
        }

        [Test]
        public void Axis_Return_Y_OnYStringTest()
        {
            // Arrange
            string inputAxisY = Axis.Y.ToString();
            // Act
            var result = RobotTypesConverter.AxisFromString(inputAxisY);
            // Assert
            Assert.That(result, Is.EqualTo(Axis.Y));
        }

        [Test]
        public void Axis_Return_Z_OnZStringTest()
        {
            // Arrange
            string inputAxisZ = Axis.Z.ToString();
            // Act
            var result = RobotTypesConverter.AxisFromString(inputAxisZ);
            // Assert
            Assert.That(result, Is.EqualTo(Axis.Z));
        }

        [Test]
        public void Axis_ThrowOn_UnknownArgumentTest()
        {
            // Arrange
            string unknownArgument = "unknownArgument";
            // Act
            // Assert
            Assert.Throws<ArgumentException>(() => RobotTypesConverter.AxisFromString(unknownArgument));
        }

        [Test]
        public void OperatingMode_ReturnRemoteControl_OnRemoteControlStringTest()
        {
            // Arrange
            string remoteControl = "Remote control";
            // Act
            var result = RobotTypesConverter.OperatingModeFromString(remoteControl);
            // Assert
            Assert.That(result, Is.EqualTo(OperatingMode.RemoteControl));
        }

        [Test]
        public void OperatingMode_ReturnLocalControl_OnLocalControlStringTest()
        {
            // Arrange
            string localControl = "Local control";
            // Act
            var result = RobotTypesConverter.OperatingModeFromString(localControl);
            // Assert
            Assert.That(result, Is.EqualTo(OperatingMode.LocalControl));
        }

        [Test]
        public void OperatingMode_ThrowOn_UnknownArgumentTest()
        {
            // Arrange
            string unknownArgument = "unknownArgument";
            // Act
            // Assert
            Assert.Throws<ArgumentException>(() => RobotTypesConverter.OperatingModeFromString(unknownArgument));
        }
    }
}