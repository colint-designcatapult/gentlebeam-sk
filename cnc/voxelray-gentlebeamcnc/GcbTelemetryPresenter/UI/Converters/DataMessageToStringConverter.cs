using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using GcbTelemetryPresenter.Domain;

namespace GcbTelemetryPresenter.UI.Converters;

public class DataMessageToStringConverter : MarkupExtension, IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var message = value as DataMessage?;
        return message?.SystemTelemetry?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}