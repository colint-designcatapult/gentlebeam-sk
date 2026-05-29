using Heracles.Application.Services;
//using Xcc.Core.Models;
using Heracles.Core.Models;
using Heracles.Robot.Models;
using Heracles.Robot.Models.RobotArm;
using Heracles.Robot.Models.RobotArm.Enums;
using Heracles.Robot.Models.RobotArm.Interfaces;
using Heracles.Robot.Models.Sequences;
using Heracles.Robot.Models.Ssh;
using Heracles.Robot.Services;

using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
//using Com.Empyreanmed.HeraclesRoboticArm.Axes.V1;
using Prism.Services.Dialogs;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Heracles.Robot.Models.Enums;
using Xcc.Application.Common;
using Xcc.Application.Models.RobotArm.Enums;
using Xcc.Core.Enums;
using Xcc.Core.Logging;


namespace Heracles.Robot.ViewModels
{
    public class RobotViewModel : BindableBase, INavigationAware
    {
        #region Properties
        private const string _robotPositionsXmlFileName = "HeraclesRobotPositions.xml";
        IPositionsPresetsMonitor _positionsPresetsMonitor = new PositionsPresetsXMLMonitor(_robotPositionsXmlFileName);
        IEventAggregator _eventAggregator;
        //IWakeOnLanService _wakeOnLanService;

        public IRegionManager RegionManager { get; }
        private IHeraclesMainSettings Settings { get; }
        public ILogRepository LogWriter { get; }
        public CoordinateSystem CoordinateSystem { get; set; }
        IRobotArmService _robotArmService;
        IAcbService _acbService;
        IDialogService _dialogService;
        #endregion Properties

        #region Commands
        public DelegateCommand RotateXPlusCommand { get; }
        public DelegateCommand RotateXMinusCommand { get; }

        public DelegateCommand RotateYPlusCommand { get; }
        public DelegateCommand RotateYMinusCommand { get; }

        public DelegateCommand RotateZPlusCommand { get; }
        public DelegateCommand RotateZMinusCommand { get; }

        public DelegateCommand TranslateXPlusCommand { get; }
        public DelegateCommand TranslateXMinusCommand { get; }

        public DelegateCommand TranslateYPlusCommand { get; }
        public DelegateCommand TranslateYMinusCommand { get; }

        public DelegateCommand TranslateZPlusCommand { get; }
        public DelegateCommand TranslateZMinusCommand { get; }

        public DelegateCommand StopCommand { get; }

        public DelegateCommand GetPositionCommand { get; }
        public DelegateCommand UpdatePositionCommand { get; }
        public DelegateCommand AddPositionCommand { get; }

        public DelegateCommand PingCommand { get; }

        public DelegateCommand MoveByJointsCommand { get; }

        public DelegateCommand RefreshCustomPositionsCommand { get; }

        public DelegateCommand NextStepCommand { get; }
        public DelegateCommand PlaySequenceCommand { get; }


        public DelegateCommand RobotActivateCommand { get; }
        public DelegateCommand RobotDeactivateCommand { get; }
        //public DelegateCommand WakeUpRosCommand { get; }
        public DelegateCommand ImagingHeadLockCommand { get; }
        public DelegateCommand ImagingHeadUnlockCommand { get; }


        private DelegateCommand? _restartLinuxServerCommand;
        public DelegateCommand RestartLinuxServerCommand 
        {
            get => _restartLinuxServerCommand ??= new DelegateCommand(
                async () =>
                {
                    IsBusy = true;
                    await Task.Run(async () =>
                    {
                        try
                        {
                            ConnectionInfo connectionInfo = new ConnectionInfo
                            {
                                Host = Settings.RobotGrpcServerUri.Host,
                                Port = 22,
                                User = "iiqka",
                                Password = "empyrean1234"
                            };

                            //ConnectionInfo connectionInfo = new ConnectionInfo
                            //{
                            //    Host = Settings.RobotGrpcServerUri.Host,
                            //    Port = 22,
                            //    User = "user",
                            //    Password = "123456"
                            //};

                            //var commandString = "~/repos/heracles-ros/ROS_Scripts/Launch/Heracles_Fake.sh";
                            //var commandString = "~/heracles-ros/ROS_Scripts/Launch/Heracles_Real.sh";
                            //var commandString = "~/heracles-ros/ROS_Scripts/Launch/Heracles_Fake.sh"; // !!! The physical Robot moves !!!
                            //var commandString = "kill -9 -- $(pgrep Heracles_Fake)"; // !!! The physical Robot moves !!!

                            //var commandString = "pm2 restart all";
                            var commandString = "source ~/.nvm/nvm.sh && pm2 restart HeraclesReal";
                            //var commandString = "ls";

                            ManualResetEvent sshFinished = new ManualResetEvent(false);
                            var sshCmd = new Command(connectionInfo, commandString, LogWriter);
                            sshCmd.OnNewLine += (obj, line) =>
                            {
                                //if (line.Contains("GRPC server"))
                                //{
                                //    sshFinished.Set();
                                //}
                                Log("ssh: " + line);
                            };
                            sshCmd.OnFinish += (obj, exitCode) =>
                            {
                                Log("ssh: exit_code = " + exitCode.ToString());
                                sshFinished.Set();
                            };

                            Log($"ssh: executing {commandString}");
                            _ = sshCmd.ExecuteAsync();

                            int timeoutMS = 60000;
                            sshFinished.WaitOne(timeoutMS);
                            LogWriter.Log($"{this.GetType().FullName}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}: Finished", LogRecordSeverity.Info, LogRecordType.System);
                        }
                        catch (Exception ex)
                        {
                            LogWriter.Log($"{this.GetType().FullName}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}: Exception {ex.Message}", LogRecordSeverity.Error, LogRecordType.System);
                        }
                    });
                    IsBusy = false;
                },
                canExecuteMethod: () => !IsBusy);

        }

        private DelegateCommand? _getActuatorsStateCommand;
        public DelegateCommand GetActuatorsStateCommand
        {
            get => _getActuatorsStateCommand ??= new DelegateCommand(
                async () =>
                {
                    //IsBusy = true;
                    await Task.Run(() =>
                    {
                        try
                        {
                            string acbState = $"Flange: Actuator = {_acbService.RobotActuator.ToString()}; ";
                            acbState += $"Treatment: Cradle Actuator={_acbService.TreatmentActuator.State.ToString()}, Head-Crandle={_acbService.TreatmentActuator.ProxySensorState.ToString()}, Head-Flange={_acbService.TreatmentActuator.LightSensorState.ToString()}; ";
                            acbState += $"Image: Cradle Actuator={_acbService.ImageActuator.State.ToString()}, Head-Crandle={_acbService.ImageActuator.ProxySensorState.ToString()}, Head-Flange={_acbService.ImageActuator.LightSensorState.ToString()}; ";
                            acbState += $"Pedal Switch: {_acbService.PedalState.ToString()}; ";
                            Log(acbState);
                            _ = LogWriter.LogAsync($"{this.GetType().FullName}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}: Finished, {acbState}", LogRecordSeverity.Info, LogRecordType.System);
                        }
                        catch (Exception ex)
                        {
                            _ = LogWriter.LogAsync($"{this.GetType().FullName}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}: Exception {ex.Message}", LogRecordSeverity.Error, LogRecordType.System);
                        }
                    });
                    //IsBusy = false;
                },
                canExecuteMethod: () => true);

        }

        public DelegateCommand TreatmentHeadLockCommand { get; }
        public DelegateCommand TreatmentHeadUnlockCommand { get; }
        public DelegateCommand RobotFlangeLockCommand { get; }
        public DelegateCommand RobotFlangeUnlockCommand { get; }


        #endregion Commands



        public RobotViewModel()
        {
            //int dead = 0xdead;
        }
        public RobotViewModel(
            IRegionManager regionManager, ILogRepository logWriter, IHeraclesMainSettings heraclesMainSettings, 
            IRobotArmService robotArmService, IAcbService acbService, IDialogService dialogService,
            IEventAggregator eventAggregator)
        {
            RegionManager = regionManager;
            LogWriter = logWriter;
            _robotArmService = robotArmService;
            _acbService = acbService;
            _dialogService = dialogService;
            _eventAggregator = eventAggregator;
            //_wakeOnLanService = null;

            LogWriter.Log($"{this.GetType().FullName}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}: Starting", LogRecordSeverity.Info, LogRecordType.System);

            Settings = heraclesMainSettings;

            _rotation = new Rotation(OnCanExecuteChanged);
            _translation = new Translation(OnCanExecuteChanged);
            IsBusy = false;

            RotateXPlusCommand = CreateCommand(_robotArmService.RotateAction, Axis.X, () => _rotation.RXDeg, 1, () => CoordinateSystem);
            RotateXMinusCommand = CreateCommand(_robotArmService.RotateAction, Axis.X, () => _rotation.RXDeg, -1, () => CoordinateSystem);
            RotateYPlusCommand = CreateCommand(_robotArmService.RotateAction, Axis.Y, () => _rotation.RYDeg, 1, () => CoordinateSystem);
            RotateYMinusCommand = CreateCommand(_robotArmService.RotateAction, Axis.Y, () => _rotation.RYDeg, -1, () => CoordinateSystem);
            RotateZPlusCommand = CreateCommand(_robotArmService.RotateAction, Axis.Z, () => _rotation.RZDeg, 1, () => CoordinateSystem);
            RotateZMinusCommand = CreateCommand(_robotArmService.RotateAction, Axis.Z, () => _rotation.RZDeg, -1, () => CoordinateSystem);

            TranslateXPlusCommand = CreateCommand(_robotArmService.TranslateAction, Axis.X, () => _translation.DXmm, 1, () => CoordinateSystem);
            TranslateXMinusCommand = CreateCommand(_robotArmService.TranslateAction, Axis.X, () => _translation.DXmm, -1, () => CoordinateSystem);
            TranslateYPlusCommand = CreateCommand(_robotArmService.TranslateAction, Axis.Y, () => _translation.DYmm, 1, () => CoordinateSystem);
            TranslateYMinusCommand = CreateCommand(_robotArmService.TranslateAction, Axis.Y, () => _translation.DYmm, -1, () => CoordinateSystem);
            TranslateZPlusCommand = CreateCommand(_robotArmService.TranslateAction, Axis.Z, () => _translation.DZmm, 1, () => CoordinateSystem);
            TranslateZMinusCommand = CreateCommand(_robotArmService.TranslateAction, Axis.Z, () => _translation.DZmm, -1, () => CoordinateSystem);

            StopCommand = new DelegateCommand(
                async () =>
                {
                    IsBusy = true;
                    await Task.Run(() =>
                    {
                        _robotArmService.Stop();
                    });
                    IsBusy = false;
                },
                canExecuteMethod: () => true);

            GetPositionCommand = new DelegateCommand(
                async () =>
                {
                    IsBusy = true;
                    await Task.Run(() =>
                    {
                        //_robotArmService.Playground();
                        var position = _robotArmService.CartesianAngularPosition;
                        if (position != null)
                        {
                            Log($"PositionMm={{{position.CartesianPositionMM?.ToString()}}} PositionDeg={{{position.AngularPositionDeg?.ToString()}}};");
                        }

                        var jointsDeg = _robotArmService.JointsPositionDeg;
                        if (jointsDeg != null)
                        {
                            Log($"PositionJointsDeg ={{{jointsDeg.ToString()}}};");
                        }
                    });
                    IsBusy = false;
                },
                canExecuteMethod: () => !IsBusy);

            UpdatePositionCommand = new DelegateCommand(
                async () =>
                {
                    IsBusy = true;
                    await Task.Run(() =>
                    {
                        var currentPosition = _robotArmService.JointsPositionDeg;
                        if (currentPosition != null)
                        {
                            Log($"PositionJointsDeg ={{{currentPosition.ToString()}}};");


                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                if (SelectedPositionPreset != null)
                                {
                                    var preset = SelectedPositionPreset;
                                    bool R = false;
                                    string message = $"Confirm saving joints position deg\n{currentPosition.ToString()}\nfor position '{preset.Name}'";
                                    string caption = "Confirm Position Save";

                                    _dialogService.Report(
                                        caption,
                                        message,
                                        ReportType.Confirmation,
                                        result =>
                                        {
                                            R = result.Result == ButtonResult.OK;
                                        });

                                    if (R)
                                    {
                                        preset.J1 = currentPosition.JArray[0];
                                        preset.J2 = currentPosition.JArray[1];
                                        preset.J3 = currentPosition.JArray[2];
                                        preset.J4 = currentPosition.JArray[3];
                                        preset.J5 = currentPosition.JArray[4];
                                        preset.J6 = currentPosition.JArray[5];
                                        _positionsPresetsMonitor.Save();
                                    }
                                }
                            });
                        }
                    });
                    IsBusy = false;
                },
                canExecuteMethod: () => !IsBusy && SelectedPositionPreset != null);

            AddPositionCommand = new DelegateCommand(
                async () =>
                {
                    IsBusy = true;
                    await Task.Run(() =>
                    {
                        var currentPosition = _robotArmService.JointsPositionDeg;
                        if (currentPosition != null)
                        {
                            Log($"PositionJointsDeg ={{{currentPosition.ToString()}}};");


                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                DialogParameters dialogParameters = new()
                                {
                                    { "Title", "Position name"},
                                    { "Message", $"Current position deg {{{currentPosition.ToString()}}}"},
                                    { "ValidationCallback", (string value) => {
                                        if (PositionPresets.FirstOrDefault(x => value == x.Name) == null)
                                        {
                                            return Tuple.Create(true, "");
                                        }
                                        else
                                        {
                                            return Tuple.Create(true, $"Position '{value}' already exist.");
                                        }
                                    } }
                                };

                                _dialogService.ShowDialog("EnterStringDialogView", dialogParameters, (result) =>
                                {
                                    if (result.Result == ButtonResult.OK)
                                    {
                                        string name = string.Empty;
                                        if (result.Parameters.TryGetValue("Value", out name))
                                        {
                                            var preset = PositionPresets.FirstOrDefault(x => x.Name == name);
                                            if (preset == null)
                                            {
                                                // Add
                                                preset = new();
                                                PositionPresets.Add(preset);
                                            }

                                            // Update
                                            preset.Name = name;
                                            preset.J1 = currentPosition.JArray[0];
                                            preset.J2 = currentPosition.JArray[1];
                                            preset.J3 = currentPosition.JArray[2];
                                            preset.J4 = currentPosition.JArray[3];
                                            preset.J5 = currentPosition.JArray[4];
                                            preset.J6 = currentPosition.JArray[5];

                                            selectedPositionPreset = preset;
                                            _positionsPresetsMonitor.Save();
                                        }
                                    }
                                });
                            });
                        }
                    });
                    IsBusy = false;
                },
                canExecuteMethod: () => !IsBusy);

            PingCommand = new DelegateCommand(
                async () =>
                {
                    IsBusy = true;
                    await Task.Run(() =>
                    {
                        _robotArmService.Ping(5);
                    });
                    IsBusy = false;
                },
                canExecuteMethod: () => !IsBusy);

            MoveByJointsCommand = new DelegateCommand(
                async () =>
                {
                    IsBusy = true;
                    await Task.Run(() =>
                    {
                        JointsPosition jp = null;

                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            jp = new JointsPosition();
                            jp.JArray[0] = SelectedPositionPreset.J1;
                            jp.JArray[1] = SelectedPositionPreset.J2;
                            jp.JArray[2] = SelectedPositionPreset.J3;
                            jp.JArray[3] = SelectedPositionPreset.J4;
                            jp.JArray[4] = SelectedPositionPreset.J5;
                            jp.JArray[5] = SelectedPositionPreset.J6;

                        });
                        _robotArmService.MoveCustomAction(jp);
                    });
                    IsBusy = false;
                },
                canExecuteMethod: () => !IsBusy && SelectedPositionPreset != null);

            NextStepCommand = new DelegateCommand(
                async () =>
                {
                    IsBusy = true;
                    await Task.Run(() =>
                    {
                        ISequence currentSequence = null;

                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            currentSequence = SelectedSequence;
                        });

                        currentSequence.DoNextStep();
                        RaisePropertyChanged(nameof(CurrentNextStepName));
                        RaisePropertyChanged(nameof(CanDoNextSequenceStep));
                    });
                    IsBusy = false;
                },
                canExecuteMethod: () => !IsBusy && SelectedSequence != null && CanDoNextSequenceStep);

            PlaySequenceCommand = new DelegateCommand(
                async () =>
                {
                    IsBusy = true;
                    await Task.Run(() =>
                    {
                        ISequence currentSequence = null;

                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            currentSequence = SelectedSequence;
                        });

                        currentSequence.Do();
                        RaisePropertyChanged(nameof(CurrentNextStepName));
                        RaisePropertyChanged(nameof(CanDoNextSequenceStep));
                    });
                    IsBusy = false;
                },
                canExecuteMethod: () => !IsBusy && SelectedSequence != null && CanDoNextSequenceStep);

            RobotActivateCommand = new DelegateCommand(
                async () =>
                {
                    IsBusy = true;
                    await Task.Run(() =>
                    {
                        var mode = OperatingMode.RemoteControl;
                        var success = _robotArmService.SetOperatingMode(mode);
                        Log($"SetOperatingMode {mode} success = {success.ToString()}");
                    });
                    IsBusy = false;
                },
                canExecuteMethod: () => !IsBusy);

            RobotDeactivateCommand = new DelegateCommand(
                async () =>
                {
                    IsBusy = true;
                    await Task.Run(() =>
                    {
                        var mode = OperatingMode.LocalControl;
                        var success = _robotArmService.SetOperatingMode(mode);
                        Log($"SetOperatingMode {mode} success = {success.ToString()}");
                    });
                    IsBusy = false;
                },
                canExecuteMethod: () => !IsBusy);

            //WakeUpRosCommand = new DelegateCommand(
            //    async () =>
            //    {
            //        IsBusy = true;
            //        await wakeOnLanService.WakeUpAsync();
            //        Log($"WakeUpAsync called");
            //        IsBusy = false;
            //    },
            //    canExecuteMethod: () => !IsBusy);

            ImagingHeadLockCommand = new DelegateCommand(
                async () =>
                {
                    IsBusy = true;
                    var r = approveActuatorCommand() && (await _acbService.SendCommand(AcbActuatorId.Image, AcbActuatorCommand.Lock));
                    Log($"LockImagingCradleActuatorAsync success = {r.ToString()}");
                    IsBusy = false;
                },
                canExecuteMethod: () => !IsBusy);

            ImagingHeadUnlockCommand = new DelegateCommand(
                async () =>
                {
                    IsBusy = true;
                    var r = approveActuatorCommand() && (await _acbService.SendCommand(AcbActuatorId.Image, AcbActuatorCommand.Unlock));
                    Log($"UnlockImagingCradleActuatorAsync success = {r.ToString()}");
                    IsBusy = false;
                },
                canExecuteMethod: () => !IsBusy);

            TreatmentHeadLockCommand = new DelegateCommand(
                async () =>
                {
                    IsBusy = true;
                    var r = approveActuatorCommand() && (await _acbService.SendCommand(AcbActuatorId.Treatment, AcbActuatorCommand.Lock));
                    Log($"LockTreatmentCradleActuatorAsync success = {r.ToString()}");
                    IsBusy = false;
                },
                canExecuteMethod: () => !IsBusy);

            TreatmentHeadUnlockCommand = new DelegateCommand(
                async () =>
                {
                    IsBusy = true;
                    var r = approveActuatorCommand() && (await _acbService.SendCommand(AcbActuatorId.Treatment, AcbActuatorCommand.Unlock));
                    Log($"UnlockTreatmentCradleActuatorAsync success = {r.ToString()}");
                    IsBusy = false;
                },
                canExecuteMethod: () => !IsBusy);

            RobotFlangeLockCommand = new DelegateCommand(
                async () =>
                {
                    IsBusy = true;
                    var r = approveActuatorCommand() && (await _acbService.SendCommand(AcbActuatorId.Robot, AcbActuatorCommand.Lock));
                    Log($"LockRobotActuatorAsync success = {r.ToString()}");
                    IsBusy = false;
                },
                canExecuteMethod: () => !IsBusy);

            RobotFlangeUnlockCommand = new DelegateCommand(
                async () =>
                {
                    IsBusy = true;
                    var r = approveActuatorCommand() && (await _acbService.SendCommand(AcbActuatorId.Robot, AcbActuatorCommand.Unlock));
                    Log($"UnlockRobotActuatorAsync success = {r.ToString()}");
                    IsBusy = false;
                },
                canExecuteMethod: () => !IsBusy);

            _positionsPresetsMonitor_PositionPresetsChanged(_positionsPresetsMonitor, new EventArgs());
            _positionsPresetsMonitor.PositionPresetsChanged += _positionsPresetsMonitor_PositionPresetsChanged;

            var stepFactory = new StepFactory(_robotArmService, _positionsPresetsMonitor, LogWriter, _acbService, _dialogService, heraclesMainSettings);
            var sequenceFactory = new SequenceFactory(LogWriter);
            populateScenario("SequencesImaging.xml", stepFactory, sequenceFactory);
            populateScenario("SequencesTreatment.xml", stepFactory, sequenceFactory);

            foreach (var s in Scenario)
            {
                foreach (var sequence in s.Value)
                {
                    sequence.StepDone += Sequence_StepDone;
                }
            }
        }

        private void Log(string message)
        {
            _eventAggregator.GetEvent<LogEvent>().Publish(message);
        }

        private void populateScenario(string fileName, IStepFactory stepFactory, ISequenceFactory sequenceFactory)
        {
            string scenarioName = fileName;
            ISequencesProvider sequencesProvider = new SequencesXMLProvider(fileName, stepFactory, sequenceFactory);
            Scenario[scenarioName] = new();
            foreach (var sequenceName in sequencesProvider.SequenceNames)
            {
                Scenario[scenarioName].Add(sequencesProvider.Provide(sequenceName));
            }
        }

        private void Sequence_StepDone(object sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(CurrentNextStepName));
            RaisePropertyChanged(nameof(CanDoNextSequenceStep));
        }

        private void _positionsPresetsMonitor_PositionPresetsChanged(object sender, EventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var old = SelectedPositionPreset;

                PositionPresets = _positionsPresetsMonitor.PositionPresets;

                if (old != null)
                {
                    foreach (var position in PositionPresets)
                    {
                        if (old.Name == position.Name)
                        {
                            SelectedPositionPreset = position;
                        }
                    }
                }
            });
        }

        DelegateCommand CreateCommand(Func<Axis, float, CoordinateSystem, bool> robot_function, Axis axis, Func<float> argument_function, float sign, Func<CoordinateSystem> argument_frame)
        {
            return new DelegateCommand(
                async () =>
                {
                    IsBusy = true;
                    await Task.Run(() =>
                    {
                        robot_function(axis, sign * argument_function(), argument_frame());
                    });
                    IsBusy = false;
                },
                canExecuteMethod: () => argument_function() > 0 && !IsBusy);
        }
        Rotation _rotation;
        Translation _translation;


        private ObservableCollection<PositionPreset> positionPresets = new();
        public ObservableCollection<PositionPreset> PositionPresets { get => positionPresets; set => SetProperty(ref positionPresets, value); }

        private PositionPreset selectedPositionPreset;
        public PositionPreset SelectedPositionPreset
        {
            get => selectedPositionPreset; set
            {

                SetProperty(ref selectedPositionPreset, value);
                OnCanExecuteChanged();
            }
        }

        private Dictionary<string, Collection<ISequence>> _scenario = new();
        public Dictionary<string, Collection<ISequence>> Scenario { get => _scenario; set => SetProperty(ref _scenario, value); }
        private KeyValuePair<string, Collection<ISequence>> selectedScenario;
        public KeyValuePair<string, Collection<ISequence>> SelectedScenario
        {
            get => selectedScenario; 
            set
            {
                SetProperty(ref selectedScenario, value);
            }
        }

        private ObservableCollection<ISequence> _sequences = new();
        public ObservableCollection<ISequence> Sequences { get => _sequences; set => SetProperty(ref _sequences, value); }
        private ISequence selectedSequence;
        public ISequence SelectedSequence
        {
            get => selectedSequence; set
            {
                if (value != selectedSequence)
                {
                    value?.Reset();
                }

                SetProperty(ref selectedSequence, value);
                RaisePropertyChanged(nameof(CurrentNextStepName));
                RaisePropertyChanged(nameof(CanDoNextSequenceStep));
                OnCanExecuteChanged();
            }
        }
        public string CurrentNextStepName
        {
            get
            {
                if (SelectedSequence != null)
                {
                    return SelectedSequence.CurrentStepName + " -> " + SelectedSequence.NextStepName;
                }
                return string.Empty;
            }
        }
        public bool CanDoNextSequenceStep
        {
            get
            {
                if (SelectedSequence != null)
                {
                    return SelectedSequence.CanDoNextStep;
                }
                return false;
            }
        }

        private bool approveActuatorCommand()
        {
            bool approved = true;
            if (_robotArmService.Status != Status.Activated && _robotArmService.Status != Status.Deactivated)
            {
                string caption = "Confirm Actuator Action";
                string message = "Disconnection of a head not as a part of normal sequence could be unsafe. please approve that the Robot is in a safe position for disconnecting the actuator";
                _dialogService.Report(
                caption,
                message,
                ReportType.Confirmation,
                result =>
                {
                    approved = result.Result == ButtonResult.OK;
                });
            }
            return approved;
        }

        private void OnCanExecuteChanged()
        {
            RotateXPlusCommand?.RaiseCanExecuteChanged();
            RotateXMinusCommand?.RaiseCanExecuteChanged();
            RotateYPlusCommand?.RaiseCanExecuteChanged();
            RotateYMinusCommand?.RaiseCanExecuteChanged();
            RotateZPlusCommand?.RaiseCanExecuteChanged();
            RotateZMinusCommand?.RaiseCanExecuteChanged();

            TranslateXPlusCommand?.RaiseCanExecuteChanged();
            TranslateXMinusCommand?.RaiseCanExecuteChanged();
            TranslateYPlusCommand?.RaiseCanExecuteChanged();
            TranslateYMinusCommand?.RaiseCanExecuteChanged();
            TranslateZPlusCommand?.RaiseCanExecuteChanged();
            TranslateZMinusCommand?.RaiseCanExecuteChanged();

            StopCommand?.RaiseCanExecuteChanged();
            GetPositionCommand?.RaiseCanExecuteChanged();
            PingCommand?.RaiseCanExecuteChanged();
            MoveByJointsCommand?.RaiseCanExecuteChanged();
            RefreshCustomPositionsCommand?.RaiseCanExecuteChanged();
            NextStepCommand?.RaiseCanExecuteChanged();
            PlaySequenceCommand?.RaiseCanExecuteChanged();

            RobotActivateCommand?.RaiseCanExecuteChanged();
            RobotDeactivateCommand?.RaiseCanExecuteChanged();
            //WakeUpRosCommand?.RaiseCanExecuteChanged();
            ImagingHeadLockCommand?.RaiseCanExecuteChanged();
            ImagingHeadUnlockCommand?.RaiseCanExecuteChanged();
            TreatmentHeadLockCommand?.RaiseCanExecuteChanged();
            TreatmentHeadUnlockCommand?.RaiseCanExecuteChanged();
            RobotFlangeLockCommand?.RaiseCanExecuteChanged();
            RobotFlangeUnlockCommand?.RaiseCanExecuteChanged();
            UpdatePositionCommand?.RaiseCanExecuteChanged();
            AddPositionCommand?.RaiseCanExecuteChanged();
            RestartLinuxServerCommand?.RaiseCanExecuteChanged();
            GetActuatorsStateCommand?.RaiseCanExecuteChanged();
        }

        public Rotation Rotation { get => _rotation; set => SetProperty(ref _rotation, value); }
        public Translation Translation { get => _translation; set => SetProperty(ref _translation, value); }

        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnCanExecuteChanged(); } }

        bool _isBusy;

        #region INavigationAware
        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext) { }

        public void OnNavigatedTo(NavigationContext navigationContext) { }

        #endregion INavigationAware
    }
}