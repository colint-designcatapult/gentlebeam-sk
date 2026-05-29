using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Xcc.Application.UI.Converters;

public class PasswordFontConverter : MarkupExtension, IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var baseStyle = new ResourceDictionary
        {
            Source = new Uri("/Xcc.Application;component/UI/Resources/Styles/BaseStyle.xaml", UriKind.RelativeOrAbsolute)
        };

        if (value is bool valueAsBool)
        {
            return valueAsBool ? baseStyle["BaseFont"] : baseStyle["PasswordFont"];
        }

        throw new ArgumentNullException(nameof(value), $"{nameof(PasswordFontConverter)}: value must be value of type {nameof(Boolean)}");
    }

    public object ConvertBack(object? value, Type targetTypes, object? parameter, CultureInfo culture) => throw new NotSupportedException();

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}

public class PasswordFontSizeConverter : MarkupExtension, IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var constants = new ResourceDictionary
        {
            Source = new Uri("/Xcc.Application;component/UI/Resources/Constants.xaml", UriKind.RelativeOrAbsolute)
        };

        if (value is bool valueAsBool)
        {
            return valueAsBool ? constants["BaseFontSize"] : constants["HugeFontSize"];
        }

        throw new ArgumentNullException(nameof(value), $"{nameof(PasswordFontConverter)}: value must be value of type {nameof(Boolean)}");
    }

    public object ConvertBack(object? value, Type targetTypes, object? parameter, CultureInfo culture) => throw new NotSupportedException();

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}