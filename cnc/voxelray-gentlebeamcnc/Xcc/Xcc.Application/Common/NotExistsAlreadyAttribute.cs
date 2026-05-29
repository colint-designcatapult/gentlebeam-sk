using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Xcc.Application.Common
{
    public class NotExistsAlreadyAttribute : ValidationAttribute
    {
        private const string PropertyNotFoundStringFormat = "The property '{0}' was not found.";

        public NotExistsAlreadyAttribute(string collectionPropertyName, string? originalValuePropertyNamePropertyName = null!)
        {
            CollectionPropertyName = collectionPropertyName;
            OriginalValuePropertyName = originalValuePropertyNamePropertyName;
        }
        public string CollectionPropertyName { get; }
        public string? OriginalValuePropertyName { get; }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null)
                return ValidationResult.Success;

            var listProperty = validationContext.ObjectType.GetProperty(CollectionPropertyName);

            if (listProperty == null)
            {
                return new ValidationResult(string.Format(PropertyNotFoundStringFormat, CollectionPropertyName));
            }

            var list = listProperty.GetValue(validationContext.ObjectInstance) as IEnumerable;

            if (list == null)
                return ValidationResult.Success;

            foreach (var item in list)
            {
                if (item?.ToString()?.Equals(value.ToString(), StringComparison.OrdinalIgnoreCase) == true)
                {
                    if (!string.IsNullOrEmpty(OriginalValuePropertyName))
                    {
                        var originalProperty = validationContext.ObjectType.GetProperty(OriginalValuePropertyName);
                        if (originalProperty == null)
                        {
                            return new ValidationResult(string.Format(PropertyNotFoundStringFormat, OriginalValuePropertyName));
                        }
                        var originalValue = originalProperty.GetValue(validationContext.ObjectInstance);
                        if (originalValue != null)
                        {
                            var equals = value?.ToString()?.Equals(originalValue);
                            if (equals is true)
                                return ValidationResult.Success;
                        }
                    }

                    return new ValidationResult(ErrorMessage ?? $"The value '{value}' already exists.");
                }
            }

            return ValidationResult.Success;
        }
    }
}
