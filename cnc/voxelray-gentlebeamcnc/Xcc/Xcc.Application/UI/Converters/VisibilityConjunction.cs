using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Xcc.Application.UI.Converters
{
    /// <summary>
    /// Converts multiple Visibility values to Visibility.Hidden or Visibility.Collapsed,
    /// if one of the source values is Visibility.Hidden or Visibility.Collapsed respectively.
    /// </summary>
    public class VisibilityConjunction : MarkupExtension, IMultiValueConverter
    {
        public Visibility Invisibility { get; set; } = Visibility.Collapsed;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (var value in values)
            {
                if (value is not Visibility visibility)
                    continue;

                if (visibility != Visibility.Visible)
                    return Invisibility;
            }

            return Visibility.Visible;
        }
        
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }

    /// <summary>
    /// Converts multiple Visibility values to Visibility.Visible,
    /// if one of the source values is Visibility.Visible.
    /// </summary>
    public class VisibilityDisjunction : MarkupExtension, IMultiValueConverter
    {
        public Visibility Invisibility { get; set; } = Visibility.Collapsed;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (var value in values)
            {
                if (value is not Visibility visibility)
                    continue;

                if (visibility == Visibility.Visible)
                    return Visibility.Visible;
            }

            return Invisibility;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
