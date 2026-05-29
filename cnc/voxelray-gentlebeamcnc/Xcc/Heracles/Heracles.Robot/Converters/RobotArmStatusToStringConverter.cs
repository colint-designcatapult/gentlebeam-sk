using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using CaseConverter;
using Heracles.Robot.Models.RobotArm.Enums;

namespace Heracles.Robot.Converters
{
    public class RobotArmStatusToStringConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Status? status = value as Status?;
            if (status is null)
                throw new ArgumentNullException(nameof(value), "RobotArmStatusToStringConverter: value cannot be null.");

            switch (status.Value)
            {
                case Status.RosClientFailure:
                    return "No Connection to Ros Client";
                case Status.RosServerFailure:
                    return "No Connection to Ros Server";
                default:
                    return value.ToString().InsertSpaceBeforeUpperCase();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
