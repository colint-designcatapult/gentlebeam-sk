using Heracles.Core.Enums;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace Heracles.Application.UI.Converters
{
    public class PlanStatusToColorConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            PlanStatus? status = value as PlanStatus?;

            if (status is null)
                return Brushes.Red;

            switch (status.Value)
            {
                case PlanStatus.PENDING_APPROVAL:
                    return Brushes.Gray;
                case PlanStatus.APPROVED:
                    return Brushes.LimeGreen;
                case PlanStatus.REJECTED:
                    return Brushes.Red;
                default:
                    return Brushes.Red;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
