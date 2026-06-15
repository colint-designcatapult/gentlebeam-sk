using Heracles.Robot.Models;

namespace Heracles.Robot.ViewModels
{
    public class UserSettingsViewModel
    {
        #region Constructors
        public UserSettingsViewModel() { }
        public UserSettingsViewModel(RobotModel robotModel)
        { 
            RobotModel = robotModel;
        }

        #endregion Constructors

        #region Properties
        public RobotModel RobotModel { get; }
        #endregion Properties
    }
}
