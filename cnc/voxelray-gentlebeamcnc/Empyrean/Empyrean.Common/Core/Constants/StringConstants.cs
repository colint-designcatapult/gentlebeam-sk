namespace Empyrean.Common.Core.Constants
{
    public static class StringConstants
    {
        public static class Common
        {
            public static class Validation
            {
                public const string FieldRequiredError = "Field is required.";

                // DateOfBirth
                public const string DateParseError = "Please enter a valid date.";
                public const string FutureDateError = $"Date can't be later than today.";

                // DateOnly
                public const string DateOnlyParseError = "Please enter a valid date.";

                // Numeric, IntRange, DoubleRange, FloatRange
                public const string NumericStartsWithDashError = "Entry cannot start with a dash.";
                public const string NumericEndsWithDashError = "Entry cannot end with a dash.";
                public const string NumericInvalidCharacterError = "Only digits and dashes are allowed.";
                public const string NumericTwoDashesError = "Two dashes in a row are not allowed.";
                public static readonly string NumericMinRangeFormatString = $"{{0}} must be{Environment.NewLine}at least {{1}}.";
                public static readonly string NumericMinMaxRangeFormatString = $"The field {{0}} must be{Environment.NewLine}between {{1}} and {{2}}.";

                public const string StringIsNullOrEmpty = "Please start to type something.";
                public const string NotANumberError = "Please enter a number.";
                public const string ValueRangeRequest = "Please enter a number in the range."; // {Min}-{Max}

                // Email Rule
                public const string EmailParseError = "Please enter a valid email address.";

                // Name Rule
                public const string NameEndsWithWhitespaceError = "Name cannot end with a space.";
                public const string NameStartsWithDashError = "Name cannot start with a dash.";
                public const string NameEndsWithDashError = "Name cannot end with a dash.";
                public const string NameStartsWithApostropheError = "Name cannot start with an apostrophe.";
                public const string NameEndsWithApostropheError = "Name cannot end with an apostrophe.";
                public const string NameStartsWithPeriodError = "Name cannot start with a period.";
                public const string NameInvalidCharacterError = "Name contains invalid characters.";
                public const string NameInvalidError = "Please enter a valid name."; //Strings containing any "-" " " "'" twice in a row counts as non-valid
                public const string NameExistsAlready = "Name already exists.";

                public const string InvalidValueStringFormat = "Invalid {0} value.";
                public const string InvalidFormatMessage = "Invalid format.";
            }
        }
    }
}
