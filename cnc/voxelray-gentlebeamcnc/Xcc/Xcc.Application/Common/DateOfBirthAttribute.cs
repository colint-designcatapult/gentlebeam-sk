using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Xcc.Core.Constants;

namespace Xcc.Application.Common
{
    public class DateOfBirthAttribute : ValidationAttribute
    {
        public DateOfBirthAttribute() : base() { }

        public override bool IsValid(object? value)
        {
            if (DateOnly.TryParseExact(value?.ToString(), CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern, out DateOnly date) == false)
            {
                ErrorMessage = StringConstants.Common.Validation.DateParseError;
                return false;
            }

            if (date > DateOnly.FromDateTime(DateTime.Now))
            {
                ErrorMessage = StringConstants.Common.Validation.FutureDateError;
                return false;
            }

            return true;
        }
    }
}
