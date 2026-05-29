using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Markup;

namespace GcbTelemetryPresenter.UI.Converters;

public class FullPathToFilenameConverter : MarkupExtension, IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        string? path = value as string;

        if (path is null)
            return string.Empty;

        var pattern = @"(\w+_)(.+).txt";
        var match = Regex.Match(path, pattern);

        if (match is {Success: true, Groups.Count: 3})
        {
            return  match.Groups[2].Value.Replace('_', ' ');
        }

        return string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}