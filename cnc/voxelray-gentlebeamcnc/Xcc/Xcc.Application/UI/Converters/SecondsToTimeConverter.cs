using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Xcc.Application.UI.Converters;

public class SecondsToHmsConverter : MarkupExtension, IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        long seconds = value switch
        {
            int i => i,
            long l => l,
            double d => (long)Math.Round(d),
            string s when long.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) => v,
            _ => -1
        };

        if (seconds < 0) return "N/A";
        var ts = TimeSpan.FromSeconds(seconds);

        // Use TOTAL hours so 27:15:04 is shown as 27:15:04 (not 03:15:04).
        var hours = (int)ts.TotalHours;
        return $"{hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new Exception($"{nameof(SecondsToHmsConverter)}: {nameof(ConvertBack)} is not supported for this converter.");

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}
