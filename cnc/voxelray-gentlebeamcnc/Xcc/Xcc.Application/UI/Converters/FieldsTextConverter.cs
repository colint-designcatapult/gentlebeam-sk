using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Xcc.Application.UI.Converters;

public class FieldsTextConverter : MarkupExtension, IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (int.TryParse(value?.ToString(), out int valueAsInt) == false)
            return Xcc.Core.Constants.StringConstants.Common.FieldsUiText;

        return valueAsInt == 1 ? Xcc.Core.Constants.StringConstants.Common.FieldUiText : Xcc.Core.Constants.StringConstants.Common.FieldsUiText;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => Binding.DoNothing;

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}