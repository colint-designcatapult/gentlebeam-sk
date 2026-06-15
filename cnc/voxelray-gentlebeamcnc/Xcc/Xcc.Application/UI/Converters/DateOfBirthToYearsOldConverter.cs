using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Xcc.Application.UI.Converters
{
    public class DateOfBirthToYearsOldConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not DateOnly birthdate)
                return "Unknown Age";

            var age = DateTime.Today.Year - birthdate.Year;

            // Go back to the year in which the person was born in case of a leap year
            if (birthdate.ToDateTime(new TimeOnly()) > DateTime.Today.AddYears(-age)) 
                age--;

            return $"{age} years";
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }

    public class BirthdayCakeVisibilityConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not DateOnly birthdate)
                return Visibility.Hidden;

            if(DateTime.Today.Month == birthdate.Month && DateTime.Today.Day == birthdate.Day)
                return Visibility.Visible;
            else
                return Visibility.Hidden;    
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }


    public class DateOnlyToDateTimeConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateOnly? date = value as DateOnly?;
            return date is null ? Binding.DoNothing : new DateTime(date.Value.Year, date.Value.Month, date.Value.Day);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime? date = value as DateTime?;
            return date is null ? Binding.DoNothing : DateOnly.FromDateTime(date.Value);
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
