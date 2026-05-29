using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Windows.Markup;

namespace Xcc.Application.Common
{
    public static class EnumEntensions
    {
        /// <summary>
        /// Gets an attribute on an enum field value
        /// </summary>
        /// <typeparam name="T">The type of the attribute you want to retrieve</typeparam>
        /// <param name="enumVal">The enum value</param>
        /// <returns>The attribute of type T that exists on the enum value</returns>
        /// <example><![CDATA[string desc = myEnumVariable.GetAttributeOfType<DescriptionAttribute>().Description;]]></example>
        public static T GetAttributeOfType<T>(this Enum enumVal) where T : Attribute
        {
            var type = enumVal.GetType();
            var memberInfo = type.GetMember(enumVal.ToString());
            var attributes = memberInfo[0].GetCustomAttributes(typeof(T), false);
            return attributes.Length > 0 ? (T)attributes[0] : null;
        }

        /// <summary>
        /// A generic extension method to retreive an Enum attribute
        /// </summary>
        public static TAttribute? GetAttribute<TAttribute>(this Enum enumValue)
                where TAttribute : Attribute
        {
            return enumValue.GetType()
                            .GetMember(enumValue.ToString())
                            .First()
                            .GetCustomAttribute<TAttribute>();
        }

        public static string? Description(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());

            if (field == null)
                return null;

            var attributes = field.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes.Length != 0)
            {
                foreach (var attribute in attributes)
                {
                    if (attribute is DescriptionAttribute descriptionAttribute)
                    {
                        return descriptionAttribute.Description;
                    }
                }
            }

            return null;
        }

        public static string GetDisplayName(this Enum value)
        {
            return value.GetAttributeOfType<DisplayAttribute>()?.Name ?? value.ToString();
        }
    }

    public class EnumToItemsSource(Type type) : MarkupExtension
    {
        private readonly Type _type = type;
        public int SkipCount { get; set; } = 0;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_type.IsEnum == false)
                throw new ArgumentException($"{nameof(_type)} should be an enum type");

            return Enum
                .GetValues(_type)
                .Cast<Enum>()
                .Skip(SkipCount)
                .Select(e => new { Value = e, DisplayName = e.GetDisplayName() });
        }
    }
}
