using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using Xcc.Core.Enums;
using Xcc.Core.Domain.GryphonBoard;

namespace Xcc.Application.UI.Converters
{
    public class GcbStatusToRadiationHazardActiveStateConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not ISystemTelemetry telemetry)
                return false;

            return telemetry.ControlBoardState is GcbStateNew.Emission or GcbStateNew.Imaging
                || telemetry.Hvps.EmissionOn;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
