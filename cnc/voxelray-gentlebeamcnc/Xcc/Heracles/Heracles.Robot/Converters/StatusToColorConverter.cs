using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using Heracles.Robot.Models.RobotArm.Enums;

namespace Heracles.Robot.Converters
{
    public class RobotArmStatusToColorConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Status? status = value as Status?;

            if (status is null)
                return Brushes.Red;

            switch (status.Value)
            {
                case Status.Activated:
                    return Brushes.YellowGreen;
                case Status.Deactivated:
                    return Brushes.Gold;
                case Status.RosClientFailure:
                case Status.RosServerFailure:
                case Status.RoboticFailure:
                    return Brushes.OrangeRed;
                default:
                    return Brushes.DarkGray;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
