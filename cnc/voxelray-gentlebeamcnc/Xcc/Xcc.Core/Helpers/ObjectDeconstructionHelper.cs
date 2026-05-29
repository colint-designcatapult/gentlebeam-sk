using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Xcc.Core.Helpers
{
    public interface IObjectDeconstructionHelper<T>
    {
        ICollection<object> GetPropertyValues(T obj);
    }

    /// <summary>
    /// Helper class to extract property values from a specified Type by a list of their names
    /// </summary>
    /// <typeparam name="Type"></typeparam>
    public class ObjectDeconstructionHelper<Type>
    {
        public const BindingFlags DefaultBindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;
        private IList<PropertyInfo>? properties;

        public IList<string> PropertyList { get; private set; } = new List<string>();
        public IList<PropertyInfo>? Properties { 
            get => properties;
            private set
            {
                properties = value;
                PropertyList = value?.Select(prop => prop.Name).ToList() ?? new List<string>();
            }
        }
        public BindingFlags BindingFlags { get; } = DefaultBindingFlags;

        public static IList<string> GetAllPropertyNames(BindingFlags bindingFlags)
        {
            return typeof(Type).GetProperties(bindingFlags).Select(prop => prop.Name).ToList();
        }

        public static IList<PropertyInfo> GetAllProperties(BindingFlags bindingFlags)
        {
            return typeof(Type).GetProperties(bindingFlags);
        }

        public ObjectDeconstructionHelper(BindingFlags bindingFlags = DefaultBindingFlags)
        {
            BindingFlags = bindingFlags;
            Properties = GetAllProperties(bindingFlags);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="propertyList">Custom property list for custom set or order of properties to extract</param>
        /// <param name="bindingFlags">Custom binding flags value for property access (see docs on GetProperties method)</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public ObjectDeconstructionHelper(IList<string> propertyList, BindingFlags bindingFlags = DefaultBindingFlags)
        {
            BindingFlags = bindingFlags;

            IList<string> fullPropertyList = GetAllPropertyNames(BindingFlags);
            if (propertyList != null)
            {
                if (propertyList.Any(name => !fullPropertyList.Contains(name)))
                {
                    throw new ArgumentException(
                        $"Type {typeof(Type).Name} is not compatible with the required property list");
                }
                PropertyList = propertyList;
            }
            else
            {
                throw new ArgumentNullException("Property list is null");
            }
        }

        public ICollection<object?> GetPropertyValues(Type obj)
        {
            var values = new List<object?>();
            foreach (string propertyName in PropertyList)
            {
                object? value = typeof(Type).GetProperty(propertyName, BindingFlags)!.GetValue(obj, null);
                values.Add(value);
            }

            return values;
        }
    }
}
