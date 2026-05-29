using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Xcc.Application.UI.Converters
{
    public class IntToVisibilityVisibleConverter : MarkupExtension, IValueConverter
    {
        public Visibility InvisibilityMode { get; set; } = Visibility.Collapsed;

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is null)
                return InvisibilityMode;

            if (parameter is null)
                return Visibility.Visible;

            return (int)value == (int)parameter ? Visibility.Visible : InvisibilityMode;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => Binding.DoNothing;

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }

    public class IntToInvisibilityConverter : MarkupExtension, IValueConverter
    {
        public Visibility InvisibilityMode { get; set; } = Visibility.Collapsed;

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is null)
                return Visibility.Visible;

            if (parameter is null)
                return InvisibilityMode;

            return (int)value == (int)parameter ? InvisibilityMode : Visibility.Visible;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => Binding.DoNothing;

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }



    public class IntToVisibilityVisibleConverter1 : MarkupExtension, IValueConverter
    {
        public Visibility InvisibilityMode { get; set; } = Visibility.Collapsed;

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is null || int.TryParse(value.ToString(), out int valueAsInt) == false)
                return InvisibilityMode;

            if (parameter is null || int.TryParse(parameter.ToString(), out int parameterAsInt) == false)
                return Visibility.Visible;

            return valueAsInt == parameterAsInt ? Visibility.Visible : InvisibilityMode;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => Binding.DoNothing;

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }

    public class IntToInvisibilityConverter1 : MarkupExtension, IValueConverter
    {
        public Visibility InvisibilityMode { get; set; } = Visibility.Collapsed;

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is null || int.TryParse(value.ToString(), out int valueAsInt) == false)
                return Visibility.Visible;

            if (parameter is null || int.TryParse(parameter.ToString(), out int parameterAsInt) == false)
                return InvisibilityMode;

            return valueAsInt == parameterAsInt ? InvisibilityMode : Visibility.Visible;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => Binding.DoNothing;

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
