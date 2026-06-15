using Heracles.Application.Models;
using Heracles.Robot.Services;

using Prism.Commands;
using Prism.Regions;

using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Xcc.Application.UI.Mvvm;
using Xcc.Core.Enums;
using Xcc.Core.Helpers;
using Xcc.Core.Logging;

namespace Heracles.Robot.ViewModels
{
    public class MovementsViewModel : RegionViewModelBase
    {
        #region Constructors
        public MovementsViewModel(IRobotArmService robot, ILogRepository logWriter, HeraclesMainSettings heraclesMainSettings, IRegionManager regionManager) : base(regionManager)
        {
            LogWriter = logWriter;
            MovementMatrix = new ObservableCollection<double>(Matrix.IdentityMatrix(4, 4).mat.Cast<double>());
            _robot = robot;
            HeraclesMainSettings = heraclesMainSettings;
            //_camera = new Camera();
            //_camera = camera;

            ImagesFolder = "images";
            Directory.CreateDirectory(ImagesFolder);
        }


        #endregion Constructors


        #region Properties
        public ObservableCollection<double> MovementMatrix { get; set; }
        IRobotArmService _robot;
        ILogRepository LogWriter { get; }

        bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnCanExecuteChanged(); } }
        

        public HeraclesMainSettings HeraclesMainSettings { get; }

        public string ImagesFolder { get ; }
        #endregion Properties


        void OnCanExecuteChanged()
        {
            MoveByMatrixCommand.RaiseCanExecuteChanged();
        }

        //private ICamera _camera;

        #region Commands

        private DelegateCommand? _moveByMatrixCommand;
        public DelegateCommand MoveByMatrixCommand
        {
            get => _moveByMatrixCommand ??= new DelegateCommand(
                async () =>
                {
                    IsBusy = true;
                    await Task.Run(() =>
                    {
                        Xcc.Application.Models.RobotArm.MovementMatrix m = new();
                        for (int i = 0; i < m.rows * m.cols; ++i)
                        {
                            m[i / m.cols, i % m.rows] = MovementMatrix[i];
                        }
                        var r = _robot.MoveByMatrixAction(m);
                        LogWriter.Log($"{this.GetType().FullName}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}: Finished, r = {r}", LogRecordSeverity.Info, LogRecordType.System);
                    });
                    IsBusy = false;
                },
                canExecuteMethod: () => !IsBusy);
        }

        private DelegateCommand? _captureCommand;
        public DelegateCommand CaptureCommand
        {
            get => _captureCommand ??= new DelegateCommand(
                async () =>
                {
                    IsBusy = true;
                    await Task.Run(() =>
                    {
                        //_logService.Log($"{this.GetType().FullName}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}: Started", LogRecordSeverity.Info, LogRecordType.User);
                        //var bitmapSource = _camera.Read();

                        //var position = DateTime.Now;
                        //string PathToDatabase = string.Empty;
                        //var positionString = DateTime.Now.ToString("yyyy'-'MM'-'dd'_'HH'_'mm'_'ss'.'fff");
                        //var pathToScreenshot = Path.Combine(PathToDatabase, $"Screenshot_{positionString}.png");

                        //if (bitmapSource != null)
                        //{
                        //    using (var fileStream = new FileStream(pathToScreenshot, FileMode.Create))
                        //    {
                        //        BitmapEncoder encoder = new PngBitmapEncoder();
                        //        encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                        //        encoder.Save(fileStream);
                        //    }
                        //    _logService.Log($"{this.GetType().FullName}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}: Finished, pathToScreenshot = {pathToScreenshot}", LogRecordSeverity.Info, LogRecordType.User);
                        //}
                        //else
                        //{
                        //    _logService.Log($"{this.GetType().FullName}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}: Unable to read frame", LogRecordSeverity.Error, LogRecordType.System);
                        //}
                    });
                    IsBusy = false;
                },
                canExecuteMethod: () => !IsBusy);
        }

        private DelegateCommand<string> _cameraScreenshotCommand;
        public DelegateCommand<string> CameraScreenshotCommand
        {
            get => _cameraScreenshotCommand ??= new DelegateCommand<string>(
                (pathToScreenshot) =>
                {

                },
                canExecuteMethod: (pathToScreenshot) => true);
        }
        #endregion Commands
    }
}
