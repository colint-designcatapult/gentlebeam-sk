using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;
using Xcc.Core.Enums;

namespace Xcc.Application.UI.Converters
{
    /// <summary>
    /// For New GCB firmware
    /// </summary>
    public class TelemetryControlBoardStateNewToValueConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string returnValue = string.Empty;

            if (value != null)
                returnValue = ((GcbStateNew)value).ToString();

            if (!Enum.GetNames<GcbStateNew>().ToList().Contains(returnValue))
                return "No Comm";

            return returnValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => 0;

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
