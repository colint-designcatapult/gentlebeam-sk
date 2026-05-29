using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;

namespace Xcc.Application.UI.Converters
{
    public class EnumToItemsSourceConverter : MarkupExtension, IValueConverter
    {
        public int SkipCount { get; set; } = 0;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Type type)
                return Binding.DoNothing;

            if (type.IsEnum == false)
                throw new ArgumentException($"{nameof(value)} should be an enum type");

            return Enum
                .GetValues(type)
                .Cast<Enum>()
                .Skip(SkipCount);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
