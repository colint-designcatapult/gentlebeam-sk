using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Xcc.Core.Common
{
    public static class GenericExtensions
    {
        /// <summary>
        /// Copy properties from one object to another.
        /// </summary>
        /// <typeparam name="T">Source object type.</typeparam>
        /// <typeparam name="U">Destination object type.</typeparam>
        /// <param name="source">Source object instance.</param>
        /// <param name="destination">Destination object instance.</param>
        /// <param name="overwriteWithNull"></param>
        /// <param name="ignoreList"></param>
        public static void CopyProperties<T, U>(this T source, U destination, bool overwriteWithNull = false, IEnumerable<string>? ignoreList = null)
        {
            if (source == null || destination == null)
            {
                return;
            }
            foreach (var destinationProperty in destination.GetType().GetProperties())
            {
                if (!destinationProperty.CanWrite)
                {
                    //Debug.WriteLine($"CopyProperties: cannot write {destination}.{destinationProperty.Name}");
                    continue;
                }

                foreach (var sourceProperty in source.GetType().GetProperties())
                {
                    if (!sourceProperty.CanRead)
                        continue;

                    if (destinationProperty.Name == sourceProperty.Name && destinationProperty.PropertyType.IsAssignableFrom(sourceProperty.PropertyType))
                    {
                        if (ignoreList is not null && ignoreList.Contains(sourceProperty.Name))
                            break;

                        object? value = sourceProperty.GetValue(source, null);

                        if (overwriteWithNull || value is not null)
                            destinationProperty.SetValue(destination, value, null);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns>Returns a list of property names</returns>
        public static ICollection<string>? GetProperties<T>(T obj) where T : class
        {
            if (obj == null)
                return null;

            var propertyNames = new List<string>();

            var properties = obj.GetType().GetProperties();

            foreach (var aProperty in properties)
            {
                propertyNames.Add(aProperty.Name);
            }

            return propertyNames;
        }

        /// <summary>
        /// Returns a property name list of obj.GetType(),
        /// which implement IEnumerable (except for String).
        /// </summary>
        public static IList<string> GetEnumerablePropertyNames(object obj)
        {
            if (obj == null) 
                throw new ArgumentNullException(nameof(obj));

            var type = obj.GetType();
            return type
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(prop =>
                    prop.CanRead &&
                    prop.PropertyType != typeof(string) &&
                    typeof(IEnumerable).IsAssignableFrom(prop.PropertyType))
                .Select(prop => prop.Name)
                .ToList();
        }
    }
}
