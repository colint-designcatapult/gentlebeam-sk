using System;
using System.Collections.Generic;
using System.Linq;

namespace Xcc.Application.Common
{
    public static class GenericExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>Returns a list of different property names</returns>
        public static ICollection<string>? CompareProperties<T>(T a, T b, bool toSnakeCase = true) where T : class
        {
            if (a == null || b == null)
                return null;

            var fieldMask = new List<string>();

            var aProperties = a.GetType().GetProperties();
            var bProperties = b.GetType().GetProperties();

            foreach (var aProperty in aProperties)
            {
                foreach (var bProperty in bProperties)
                {
                    if (aProperty.Name == bProperty.Name)
                    {

                        var aValue = aProperty.GetValue(a);
                        var bValue = bProperty.GetValue(b);

                        if (!object.Equals(aValue, bValue))
                        {
                            if (toSnakeCase)
                                fieldMask.Add(CaseConverter.Converters.ToSnakeCase(aProperty.Name));
                            else
                                fieldMask.Add(aProperty.Name);
                        }

                        break;
                    }
                }
            }

            return fieldMask;
        }

        /// <summary>
         /// 
         /// </summary>
         /// <typeparam name="T"></typeparam>
         /// <param name="obj"></param>
         /// <returns>Returns a list of property names</returns>
        public static ICollection<string>? GetPropertiesSnakeCase<T>(T obj) where T : class
        {
            if (obj == null)
                return null;

            var propertyNames = Core.Common.GenericExtensions.GetProperties(obj)?.ToList();

            return propertyNames?.Select(x => CaseConverter.Converters.ToSnakeCase(x)).ToList();
        }

        public static T Clamp<T>(this T V, T L0, T L1) where T : IComparable<T>
        {
            if (V.CompareTo(L0) < 0)
                return L0;

            if (V.CompareTo(L1) > 0)
                return L1;

            return V;
        }
    }
}
