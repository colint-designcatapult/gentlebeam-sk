using Heracles.Robot.Models.Interlock;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace Heracles.Robot.UI.Converters
{
    public class InterlockStateToColorConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            State  ? state = value as State?;

            if (state is null)
                return Brushes.Red;

            switch (state.Value)
            {
                case State.Allow:
                    return Brushes.YellowGreen;
                case State.Deny:
                    return Brushes.OrangeRed;
                default:
                    return Brushes.DarkGray;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
