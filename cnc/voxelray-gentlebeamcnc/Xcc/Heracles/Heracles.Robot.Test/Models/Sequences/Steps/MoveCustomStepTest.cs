//using Heracles.Robot.Models.Sequences;
//using Heracles.Robot.Models;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Heracles.Core.Commands;
//using Moq;
//using Xcc.Application.Services;
//using Prism.Services.Dialogs;
//using Heracles.Application.Services;
//using Heracles.Robot.Models.Sequences.Steps;
//using Heracles.Application.Models.RobotArm;

//namespace Heracles.Robot.Test.Models.Sequences.Steps
//{
//    public class MoveCustomStepTest
//    {

//        string action;
//        IList<string> actionValues;
//        Mock<IRobotArmService> fakeRobotArmService;
//        Mock<IPositionsPresetsMonitor> fakePositionsPresetsMonitor;
//        Mock<ILogService> fakeLogService;
//        Mock<IAcbService> fakeAcbService;
//        Mock<IDialogService> fakeDialogService;

//        [SetUp]
//        public void Setup()
//        {
//            action = string.Empty;
//            actionValues = new List<string>();
//            fakeRobotArmService = new();
//            fakePositionsPresetsMonitor = new();
//            fakeLogService = new();
//            fakeAcbService = new();
//            fakeDialogService = new();
//        }

//        [Test]
//        public void StepDo_ReturnFalse_On_EmptyActionValues()
//        {
//            // Arrange
//            actionValues = new List<string>();
//            var step = new MoveCustomStep(action, actionValues, fakeRobotArmService.Object, fakePositionsPresetsMonitor.Object, fakeLogService.Object, fakeAcbService.Object, fakeDialogService.Object);
//            // Act
//            var r = step.Do();
//            // Assert
//            Assert.That(r, Is.EqualTo(false));
//        }

//        [Test]
//        public void StepDo_ReturnFalse_On_InvalidActionValues()
//        {
//            // Arrange
//            actionValues.Add("InvalidPositionName");
//            var step = new MoveCustomStep(action, actionValues, fakeRobotArmService.Object, fakePositionsPresetsMonitor.Object, fakeLogService.Object, fakeAcbService.Object, fakeDialogService.Object);
//            // Act
//            var r = step.Do();
//            // Assert
//            Assert.That(r, Is.EqualTo(false));
//        }

//        [Test]
//        public void StepDo_ReturnTrue_On_ValidActionValues()
//        {
//            // Arrange
//            string validPositionName = "ValidPositionName";
//            actionValues.Add(validPositionName);
//            fakePositionsPresetsMonitor.SetupGet(x => x.PositionPresets).Returns(
//                new System.Collections.ObjectModel.ObservableCollection<PositionPreset> {
//                    new PositionPreset () { Name = validPositionName }
//                });
//            fakeRobotArmService.Setup(cmd => cmd.MoveCustomAction(It.IsAny<JointsPosition>())).Returns(true);
//            var step = new MoveCustomStep(action, actionValues, fakeRobotArmService.Object, fakePositionsPresetsMonitor.Object, fakeLogService.Object, fakeAcbService.Object, fakeDialogService.Object);
//            // Act
//            var r = step.Do();
//            // Assert
//            Assert.That(r, Is.EqualTo(true));
//        }
//    }
//}
