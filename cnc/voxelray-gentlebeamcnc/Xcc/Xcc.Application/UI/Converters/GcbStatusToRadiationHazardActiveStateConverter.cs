using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using Xcc.Core.Enums;
using Xcc.Infra.GryphonBoard;

namespace Xcc.Application.UI.Converters
{
    public class GcbStatusToRadiationHazardActiveStateConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is null)
                throw new ArgumentException("'ControlBoardState' value is not specified.");

            if(!Enum.TryParse(value.ToString(), out GcbStateNew state))
                throw new ArgumentException($"The value '{value}' is not a valid 'ControlBoardState' value.");

            return (SystemTelemetry.IsEmissionState(state));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
