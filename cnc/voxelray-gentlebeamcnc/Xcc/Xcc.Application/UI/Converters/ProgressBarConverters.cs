using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Xcc.Application.UI.Converters
{
    public class ProgressBarSegmentsVisibilityConverter : MarkupExtension, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values is null || values.Length < 5)
                throw new ArgumentException(" Progress 'Value', 'Minimum', 'Maximum' values, segment index and number of segments must be specified.");

            if(!double.TryParse(values[0].ToString(), out double value))
                throw new ArgumentException("Progress 'Value' value is not specified.");

            if (!double.TryParse(values[1].ToString(), out double minimum))
                throw new ArgumentException("Progress 'Minimum' value is not specified.");

            if (!double.TryParse(values[2].ToString(), out double maximum))
                throw new ArgumentException("Progress 'Maximum' value is not specified.");

            if (!double.TryParse(values[3].ToString(), out double segmentIndex))
                throw new ArgumentException("Segment index is not specified.");

            if (!double.TryParse(values[4].ToString(), out double segmentsCount))
                throw new ArgumentException("Number of segments value is not specified.");

            if(segmentsCount <= 0)
                throw new ArgumentException("Number of segments value cannot be negative or zero.");

            segmentIndex = segmentsCount - segmentIndex;

            var segmentValue = segmentIndex / segmentsCount * 100;
            var progressValue = (value - minimum) / (maximum - minimum) * 100;
            
            return (progressValue >= segmentValue) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }

    public class ProgressBarValueConverter : MarkupExtension, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values is null || values.Length < 2)
                throw new ArgumentException("'Total' and 'Current' values must be specified.");

            if (!double.TryParse(values[0].ToString(), out double total))
                throw new ArgumentException("'Total' value is not specified.");

            if (total == 0)
                throw new ArgumentException("'Total' value cannot be 0.");

            if (!double.TryParse(values[1].ToString(), out double current))
                throw new ArgumentException("'Current' value is not specified.");

            _ = double.TryParse(values[2].ToString(), out double threshold);
            
            var progressValue = (total - current < threshold) ? 100d : current / total * 100d;

            return progressValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
