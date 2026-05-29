using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Xcc.Application.UI.Converters;

public class ConditionalSeparatorConverter : MarkupExtension, IMultiValueConverter
{
    public string Separator { get; set; } = ", ";

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var nonEmptyValues = values
            .Where(value => !value.Equals(DependencyProperty.UnsetValue))
            .Select(value => value.ToString())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToArray();

        if (nonEmptyValues.Length == 0)
            return string.Empty;

        // Объединяем элементы с разделителем
        return string.Join(Separator, nonEmptyValues);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException($"{nameof(ConditionalSeparatorConverter)}.{nameof(ConvertBack)} is not supported for this converter.");

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}