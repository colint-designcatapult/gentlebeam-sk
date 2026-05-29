using Heracles.Application.Services;
using Heracles.Core.Enums;
using Heracles.Core.Models;
using Heracles.Robot.Models.Sequences;

using Prism.Mvvm;
using Prism.Services.Dialogs;

using System;
using System.Threading.Tasks;
using Heracles.Robot.Models.RobotArm.Interfaces;
using Heracles.Robot.Services;
using Xcc.Core.Logging;

namespace Heracles.Robot.Models
{
    public class RobotModel : BindableBase
    {
        private const string _robotPositionsXmlFileName = "HeraclesRobotPositions.xml";
        private const string _robotSequencesTreatmentXmlFileName = "SequencesTreatment.xml";
        private const string _robotSequencesImagingXmlFileName = "SequencesImaging.xml";

        IPositionsPresetsMonitor _positionsPresetsMonitor = new PositionsPresetsXMLMonitor(_robotPositionsXmlFileName);
        public RobotModel() { }

        public RobotModel(
            IRobotArmService robotArmService,
            IAcbService acbService,
            ILogRepository logWriter,
            IHeraclesMainSettings heraclesMainSettings,
            IDialogService dialogService)
        {
            RobotArmService = robotArmService;
            LogWriter = logWriter;

            var stepFactory = new StepFactory(robotArmService, _positionsPresetsMonitor, LogWriter, acbService, dialogService, heraclesMainSettings);
            var sequenceFactory = new SequenceFactory(LogWriter);
            SequencesTreatmentProvider = new SequencesXMLProvider(_robotSequencesTreatmentXmlFileName, stepFactory, sequenceFactory);
            SequencesImagingProvider = new SequencesXMLProvider(_robotSequencesImagingXmlFileName, stepFactory, sequenceFactory);
        }


        #region Read-only properties
        private IRobotArmService RobotArmService { get; }
        private ILogRepository LogWriter { get; }
        private ISequencesProvider SequencesTreatmentProvider { get; }
        private ISequencesProvider SequencesImagingProvider { get; }
        #endregion Read-only properties


        #region Properties
        RobotModelState _viewModelState = RobotModelState.Initial;
        public RobotModelState ViewModelState
        {
            get => _viewModelState;
            set
            {
                SetProperty(ref _viewModelState, value);
                RaisePropertyChanged(nameof(CanTreatmentHeadPrepare));
                RaisePropertyChanged(nameof(CanTreatmentHeadRelease));
                RaisePropertyChanged(nameof(ReadyForTreatment));
                RaisePropertyChanged(nameof(CanImagingHeadPrepare));
                RaisePropertyChanged(nameof(CanImagingHeadRelease));
                RaisePropertyChanged(nameof(ReadyForImaging));
                RaisePropertyChanged(nameof(CanTreatmentHeadMoveToQcPanel));
                RaisePropertyChanged(nameof(CanChangeWorkSpace));

                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler CanExecuteChanged;

        public bool CanTreatmentHeadPrepare => (ViewModelState == RobotModelState.Initial) ||
            (ViewModelState == RobotModelState.TreatmentHeadGrabFinished) ||
            (ViewModelState == RobotModelState.ImagingHeadGrabFinished);
        public bool CanTreatmentHeadRelease => (ViewModelState == RobotModelState.Initial) ||
            (ViewModelState == RobotModelState.TreatmentHeadGrabFinished) ||
            (ViewModelState == RobotModelState.ImagingHeadGrabFinished);
        public bool ReadyForTreatment =>
            (ViewModelState == RobotModelState.TreatmentHeadGrabFinished) ||
            (ViewModelState == RobotModelState.ImagingHeadGrabFinished);
        public bool CanImagingHeadPrepare => (ViewModelState == RobotModelState.Initial) ||
            (ViewModelState == RobotModelState.TreatmentHeadGrabFinished) ||
            (ViewModelState == RobotModelState.ImagingHeadGrabFinished);
        public bool ReadyForImaging => ViewModelState == RobotModelState.ImagingHeadGrabFinished;
        public bool CanImagingHeadRelease => (ViewModelState == RobotModelState.Initial) ||
            (ViewModelState == RobotModelState.TreatmentHeadGrabFinished) ||
            (ViewModelState == RobotModelState.ImagingHeadGrabFinished);
        public bool CanTreatmentHeadMoveToQcPanel => (ViewModelState == RobotModelState.Initial) ||
            (ViewModelState == RobotModelState.TreatmentHeadGrabFinished) ||
            (ViewModelState == RobotModelState.ImagingHeadGrabFinished);
        public bool CanChangeWorkSpace => ViewModelState == RobotModelState.Initial;

        private Workspace _workspace = Workspace.Left;
        public Workspace Workspace
        {
            get => _workspace;
            set => SetProperty(ref _workspace, value);
        }
        #endregion Properties


        #region Public methods
        public async Task TreatmentHeadPrepare()
        {
            if (CanTreatmentHeadPrepare == false)
                return;

            var oldState = ViewModelState;

            ViewModelState = RobotModelState.TreatmentHeadGrabInProgress;

            var result = await Task.Run(() =>
            {
                var pickHead = SequencesTreatmentProvider.Provide(SequenceNameProvider.Provide(SequenceId.PickHead, Workspace));
                var treat = SequencesTreatmentProvider.Provide(SequenceNameProvider.Provide(SequenceId.Treat, Workspace));

                pickHead.Reset();
                treat.Reset();

                if (!pickHead.Do())
                    return false;

                if (!treat.Do())
                    return false;

                return true;
            });

            ViewModelState = result ? RobotModelState.TreatmentHeadGrabFinished : oldState;
        }

        public async Task TreatmentHeadRelease()
        {
            if (CanTreatmentHeadRelease == false)
                return;

            var oldState = ViewModelState;

            ViewModelState = RobotModelState.TreatmentHeadReleaseInProgress;

            var result = await Task.Run(() =>
            {
                var placeHead = SequencesTreatmentProvider.Provide(SequenceNameProvider.Provide(SequenceId.PlaceHead, Workspace));

                placeHead.Reset();

                if (!placeHead.Do())
                    return false;

                return true;
            });

            ViewModelState = result ? RobotModelState.Initial : oldState;
        }

        public async Task ImagingHeadPrepare()
        {
            if (CanImagingHeadPrepare == false)
                return;

            var oldState = ViewModelState;

            ViewModelState = RobotModelState.ImagingHeadGrabInProgress;

            var result = await Task.Run(() =>
            {
                var pickHead = SequencesImagingProvider.Provide(SequenceNameProvider.Provide(SequenceId.PickHead, _workspace));
                var treat = SequencesImagingProvider.Provide(SequenceNameProvider.Provide(SequenceId.Treat, _workspace));

                pickHead.Reset();
                treat.Reset();

                if (!pickHead.Do())
                    return false;

                if (!treat.Do())
                    return false;

                return true;
            });

            ViewModelState = result ? RobotModelState.ImagingHeadGrabFinished : oldState;
        }

        public async Task ImagingHeadRelease()
        {
            if (CanImagingHeadRelease == false)
                return;

            var oldState = ViewModelState;

            ViewModelState = RobotModelState.ImagingHeadReleaseInProgress;

            var result = await Task.Run(() =>
            {
                var placeHead = SequencesImagingProvider.Provide(SequenceNameProvider.Provide(SequenceId.PlaceHead, _workspace));

                placeHead.Reset();

                if (!placeHead.Do())
                    return false;

                return true;
            });

            ViewModelState = result ? RobotModelState.Initial : oldState;
        }

        public async Task TreatmentHeadMoveToQcPanel()
        {
            if (CanTreatmentHeadMoveToQcPanel == false)
                return;

            var oldState = ViewModelState;

            ViewModelState = RobotModelState.TreatmentHeadQcInProgress;

            var result = await Task.Run(() =>
            {
                var workspace = Workspace.Left;

                var pick = SequencesTreatmentProvider.Provide(SequenceNameProvider.Provide(SequenceId.PickHead, workspace));
                var qc = SequencesTreatmentProvider.Provide(SequenceNameProvider.Provide(SequenceId.QC, workspace));
                var place = SequencesTreatmentProvider.Provide(SequenceNameProvider.Provide(SequenceId.PlaceHeadFromQC, workspace));

                pick.Reset();
                qc.Reset();
                place.Reset();

                if (!pick.Do())
                    return false;
                if (!qc.Do())
                    return false;
                if (!place.Do())
                    return false;

                return true;
            });

            ViewModelState = result ? RobotModelState.Initial : oldState;
        }

        public void Stop()
        {
            RobotArmService.Stop();
        }
        #endregion Public methods
    }
}
