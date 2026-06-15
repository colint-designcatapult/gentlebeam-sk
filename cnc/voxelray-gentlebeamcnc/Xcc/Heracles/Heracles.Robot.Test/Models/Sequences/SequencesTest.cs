using Heracles.Robot.Models.RobotArm.Interfaces;
using Heracles.Robot.Models.Sequences;
using Moq;
using Xcc.Core.Logging;

namespace Heracles.Robot.Test.Models.Sequences
{
    internal class SequencesTest
    {
        string _name;
        IList<IStep> _steps;
        Mock<IStep> _step1;
        Mock<IStep> _step2;
        Mock<IStep> _step3;
        Mock<ILogRepository> _logService;
        Sequence _sequence;

        [SetUp]
        public void Setup()
        {
            _step1 = new Mock<IStep>();
            _step2 = new Mock<IStep>(); 
            _step3 = new Mock<IStep>();
            _steps = new List<IStep>();
            _logService = new Mock<ILogRepository>();
        }

        [Test]
        public void SequenceDo_ReturnFalse_On_NullStepsList()
        {
            // Arrange
            ISequence sequence = new Sequence("sequence", null, _logService.Object);
            // Act
            var r = sequence.Do();
            // Assert
            Assert.That(r, Is.EqualTo(false));
        }

        [Test]
        public void SequenceDo_ReturnFalse_On_EmptyStepsList()
        {
            // Arrange
            ISequence sequence = new Sequence("sequence", _steps, _logService.Object);
            // Act
            var r = sequence.Do();
            // Assert
            Assert.That(r, Is.EqualTo(false));
        }

        [Test]
        public void SequenceName_ReturnNull_On_NullName()
        {
            // Arrange
            ISequence sequence = new Sequence(null, null, _logService.Object);
            // Act
            var r = sequence.Name;
            // Assert
            Assert.That(r, Is.EqualTo(null));
        }
        [Test]
        public void SequenceName_ReturnEmpty_On_EmptyName()
        {
            // Arrange
            ISequence sequence = new Sequence(string.Empty, null, _logService.Object);
            // Act
            var r = sequence.Name;
            // Assert
            Assert.That(r, Is.EqualTo(string.Empty));
        }

        [Test]
        public void SequenceName_ReturnMyName_On_MyNameName()
        {
            // Arrange
            string name = "MyName";
            ISequence sequence = new Sequence(name, null, _logService.Object);
            // Act
            var r = sequence.Name;
            // Assert
            Assert.That(r, Is.EqualTo(name));
        }

        [Test]
        public void SequenceToString_ReturnNull_On_NullName()
        {
            // Arrange
            ISequence sequence = new Sequence(null, null, _logService.Object);
            // Act
            var r = sequence.ToString();
            // Assert
            Assert.That(r, Is.EqualTo(null));
        }
        [Test]
        public void SequenceToString_ReturnEmpty_On_EmptyName()
        {
            // Arrange
            ISequence sequence = new Sequence(string.Empty, null, _logService.Object);
            // Act
            var r = sequence.ToString();
            // Assert
            Assert.That(r, Is.EqualTo(string.Empty));
        }

        [Test]
        public void SequenceToString_ReturnMyName_On_MyNameName()
        {
            // Arrange
            string name = "MyName";
            ISequence sequence = new Sequence(name, null, _logService.Object);
            // Act
            var r = sequence.ToString();
            // Assert
            Assert.That(r, Is.EqualTo(name));
        }

        [Test]
        public void SequenceDo_CallsMoveToOK_On_Success()
        {
            // Arrange
            _step1.SetupGet(x  => x.Id).Returns("1");
            _step1.SetupGet(x => x.NextIdIfOk).Returns("2");
            _step1.SetupGet(x => x.NextIdIfFailed).Returns("3");
            _step1.Setup(x => x.Do()).Returns(true);
            _steps.Add(_step1.Object);

            _step2.SetupGet(x => x.Id).Returns("2");
            _step2.SetupGet(x => x.NextIdIfOk).Returns("stop_ok");
            _step2.SetupGet(x => x.NextIdIfFailed).Returns("stop_ok");
            _step2.Setup(x => x.Do()).Returns(true);
            _steps.Add(_step2.Object);

            _step3.SetupGet(x => x.Id).Returns("3");
            _step3.SetupGet(x => x.NextIdIfOk).Returns("stop_ok");
            _step3.SetupGet(x => x.NextIdIfFailed).Returns("stop_ok");
            _step3.Setup(x => x.Do()).Returns(true);
            _steps.Add(_step3.Object);


            ISequence sequence = new Sequence(null, _steps, _logService.Object);
            // Act
            var r = sequence.Do();
            // Assert
            _step1.Verify(x => x.Do(), Times.Once);
            _step2.Verify(x => x.Do(), Times.Once);
            _step3.Verify(x => x.Do(), Times.Never);
        }

        [Test]
        public void SequenceDo_CallsMoveToFailed_On_Failed()
        {
            // Arrange
            _step1.SetupGet(x => x.Id).Returns("1");
            _step1.SetupGet(x => x.NextIdIfOk).Returns("2");
            _step1.SetupGet(x => x.NextIdIfFailed).Returns("3");
            _step1.Setup(x => x.Do()).Returns(false);
            _steps.Add(_step1.Object);

            _step2.SetupGet(x => x.Id).Returns("2");
            _step2.SetupGet(x => x.NextIdIfOk).Returns("stop_ok");
            _step2.SetupGet(x => x.NextIdIfFailed).Returns("stop_ok");
            _step2.Setup(x => x.Do()).Returns(true);
            _steps.Add(_step2.Object);

            _step3.SetupGet(x => x.Id).Returns("3");
            _step3.SetupGet(x => x.NextIdIfOk).Returns("stop_ok");
            _step3.SetupGet(x => x.NextIdIfFailed).Returns("stop_ok");
            _step3.Setup(x => x.Do()).Returns(true);
            _steps.Add(_step3.Object);

            ISequence sequence = new Sequence(null, _steps, _logService.Object);
            // Act
            var r = sequence.Do();
            // Assert
            _step1.Verify(x => x.Do(), Times.Once);
            _step2.Verify(x => x.Do(), Times.Never);
            _step3.Verify(x => x.Do(), Times.Once);
        }

        [Test]
        public void SequenseReset_SetsCanDoNextStepToTrue()
        {
            // Arrange
            _step1.SetupGet(x => x.Id).Returns("1");
            _step1.SetupGet(x => x.NextIdIfOk).Returns("stop_ok");
            _step1.SetupGet(x => x.NextIdIfFailed).Returns("stop_ok");
            _step1.Setup(x => x.Do()).Returns(false);
            _steps.Add(_step1.Object);

            ISequence sequence = new Sequence(null, _steps, _logService.Object);
            // Act
            var r1 = sequence.CanDoNextStep;
            var r2 = sequence.Do();
            var r3 = sequence.CanDoNextStep;
            sequence.Reset();
            var r4 = sequence.CanDoNextStep;

            // Assert
            Assert.That(r1 && !r3 && r4, Is.EqualTo(true));
        }

        [Test]
        public void SequenseDoNextStep_ReturnsFalseOnEmptySequence()
        {
            // Arrange
            ISequence sequence = new Sequence(null, null, _logService.Object);
            // Act
            var r1 = sequence.DoNextStep();
            // Assert
            Assert.That(r1, Is.EqualTo(false));
        }

        [Test]
        public void SequenceCurrentStepName_ReturnsUnknownAtEmptySequence()
        {
            // Arrange
            ISequence sequence = new Sequence(null, null, _logService.Object);
            // Act
            var r1 = sequence.CurrentStepName;
            // Assert
            Assert.That(r1, Is.EqualTo("Unknown"));
        }

        [Test]
        public void SequenceCurrentStepName_ReturnsCompletedAtEmptySequence()
        {
            // Arrange
            ISequence sequence = new Sequence(null, null, _logService.Object);
            // Act
            var r1 = sequence.NextStepName;
            // Assert
            Assert.That(r1, Is.EqualTo("Completed"));
        }

        [Test]
        public void SequenseConstructor_ThrowExceptionOnEmptyStep_Id()
        {
            // Arrange
            _step1.SetupGet(x => x.Id).Returns(string.Empty);
            _step1.SetupGet(x => x.NextIdIfOk).Returns("stop_ok");
            _step1.SetupGet(x => x.NextIdIfFailed).Returns("stop_ok");
            _step1.Setup(x => x.Do()).Returns(true);
            _steps.Add(_step1.Object);

            // Act
            // Assert
            Assert.Throws<Exception>(() => new Sequence(null, _steps, _logService.Object));
        }

        [Test]
        public void SequenseConstructor_ThrowExceptionOnEmptyStep_NextIdIfOk()
        {
            // Arrange
            _step1.SetupGet(x => x.Id).Returns("1");
            _step1.SetupGet(x => x.NextIdIfOk).Returns("");
            _step1.SetupGet(x => x.NextIdIfFailed).Returns("stop_ok");
            _step1.Setup(x => x.Do()).Returns(true);
            _steps.Add(_step1.Object);

            // Act
            // Assert
            Assert.Throws<Exception>(() => new Sequence(null, _steps, _logService.Object));
        }

        [Test]
        public void SequenseConstructor_ThrowExceptionOnInvalideStep_NextIdIfOk()
        {
            // Arrange
            _step1.SetupGet(x => x.Id).Returns("1");
            _step1.SetupGet(x => x.NextIdIfOk).Returns("invalid");
            _step1.SetupGet(x => x.NextIdIfFailed).Returns("stop_ok");
            _step1.Setup(x => x.Do()).Returns(true);
            _steps.Add(_step1.Object);

            // Act
            // Assert
            Assert.Throws<Exception>(() => new Sequence(null, _steps, _logService.Object));
        }

        [Test]
        public void SequenseConstructor_ThrowExceptionOnEmptyStep_NextIdIfFailed()
        {
            // Arrange
            _step1.SetupGet(x => x.Id).Returns("1");
            _step1.SetupGet(x => x.NextIdIfOk).Returns("stop_ok");
            _step1.SetupGet(x => x.NextIdIfFailed).Returns(string.Empty);
            _step1.Setup(x => x.Do()).Returns(true);
            _steps.Add(_step1.Object);

            // Act
            // Assert
            Assert.Throws<Exception>(() => new Sequence(null, _steps, _logService.Object));
        }

        [Test]
        public void SequenseConstructor_ThrowExceptionOnInvalideStep_NextIdIfFailed()
        {
            // Arrange
            _step1.SetupGet(x => x.Id).Returns("1");
            _step1.SetupGet(x => x.NextIdIfOk).Returns("stop_ok");
            _step1.SetupGet(x => x.NextIdIfFailed).Returns("invalid");
            _step1.Setup(x => x.Do()).Returns(true);
            _steps.Add(_step1.Object);

            // Act
            // Assert
            Assert.Throws<Exception>(() => new Sequence(null, _steps, _logService.Object));
        }

        [Test]
        public void SequenseConstructor_ThrowExceptionOnDuplicatedStepId()
        {
            // Arrange
            _step1.SetupGet(x => x.Id).Returns("1");
            _step1.SetupGet(x => x.NextIdIfOk).Returns("stop_ok");
            _step1.SetupGet(x => x.NextIdIfFailed).Returns("invalid");
            _step1.Setup(x => x.Do()).Returns(true);
            _step2.SetupGet(x => x.Id).Returns("1");
            _step2.SetupGet(x => x.NextIdIfOk).Returns("stop_ok");
            _step2.SetupGet(x => x.NextIdIfFailed).Returns("invalid");
            _step2.Setup(x => x.Do()).Returns(true);

            _steps.Add(_step1.Object);
            _steps.Add(_step2.Object);

            // Act
            // Assert
            Assert.Throws<Exception>(() => new Sequence(null, _steps, _logService.Object));
        }

        [Test]
        public void SequenseDo_ReturnsTrue_On_TrueStopOK()
        {
            // Arrange
            _step1.SetupGet(x => x.Id).Returns("1");
            _step1.SetupGet(x => x.NextIdIfOk).Returns("stop_ok");
            _step1.SetupGet(x => x.NextIdIfFailed).Returns("stop_failure");
            _step1.Setup(x => x.Do()).Returns(true);
            _steps.Add(_step1.Object);
            ISequence sequence = new Sequence(null, _steps, _logService.Object);

            // Act
            var r = sequence.Do();
            // Assert
            Assert.That(r, Is.EqualTo(true));
        }

        [Test]
        public void SequenseDo_ReturnsFalse_On_TrueStopFailure()
        {
            // Arrange
            _step1.SetupGet(x => x.Id).Returns("1");
            _step1.SetupGet(x => x.NextIdIfOk).Returns("stop_failure");
            _step1.SetupGet(x => x.NextIdIfFailed).Returns("stop_ok");
            _step1.Setup(x => x.Do()).Returns(true);
            _steps.Add(_step1.Object);
            ISequence sequence = new Sequence(null, _steps, _logService.Object);

            // Act
            var r = sequence.Do();
            // Assert
            Assert.That(r, Is.EqualTo(false));
        }

        [Test]
        public void SequenseDo_ReturnsTrue_On_FalseStopOK()
        {
            // Arrange
            _step1.SetupGet(x => x.Id).Returns("1");
            _step1.SetupGet(x => x.NextIdIfOk).Returns("stop_failure");
            _step1.SetupGet(x => x.NextIdIfFailed).Returns("stop_ok");
            _step1.Setup(x => x.Do()).Returns(false);
            _steps.Add(_step1.Object);
            ISequence sequence = new Sequence(null, _steps, _logService.Object);

            // Act
            var r = sequence.Do();
            // Assert
            Assert.That(r, Is.EqualTo(true));
        }

        [Test]
        public void SequenseDo_ReturnsFalse_On_FalseStopFailure()
        {
            // Arrange
            _step1.SetupGet(x => x.Id).Returns("1");
            _step1.SetupGet(x => x.NextIdIfOk).Returns("stop_ok");
            _step1.SetupGet(x => x.NextIdIfFailed).Returns("stop_failure");
            _step1.Setup(x => x.Do()).Returns(false);
            _steps.Add(_step1.Object);
            ISequence sequence = new Sequence(null, _steps, _logService.Object);

            // Act
            var r = sequence.Do();
            // Assert
            Assert.That(r, Is.EqualTo(false));
        }


        //[Test]
        //public void StepDo_ReturnFalse_On_InvalidActionValues()
        //{
        //    // Arrange
        //    actionValues.Add("InvalidPositionName");
        //    var step = new MoveCustomStep(action, actionValues, fakeRobotArmService.Object, fakePositionsPresetsMonitor.Object, fakeLogService.Object, fakeAcbService.Object, fakeDialogService.Object);
        //    // Act
        //    var r = step.Do();
        //    // Assert
        //    Assert.That(r, Is.EqualTo(false));
        //}

        //[Test]
        //public void StepDo_ReturnTrue_On_ValidActionValues()
        //{
        //    // Arrange
        //    string validPositionName = "ValidPositionName";
        //    actionValues.Add(validPositionName);
        //    fakePositionsPresetsMonitor.SetupGet(x => x.PositionPresets).Returns(
        //        new System.Collections.ObjectModel.ObservableCollection<PositionPreset> {
        //                    new PositionPreset () { Name = validPositionName }
        //        });
        //    fakeRobotArmService.Setup(cmd => cmd.MoveCustomAction(It.IsAny<JointsPosition>())).Returns(true);
        //    var step = new MoveCustomStep(action, actionValues, fakeRobotArmService.Object, fakePositionsPresetsMonitor.Object, fakeLogService.Object, fakeAcbService.Object, fakeDialogService.Object);
        //    // Act
        //    var r = step.Do();
        //    // Assert
        //    Assert.That(r, Is.EqualTo(true));
        //}
    }
}
