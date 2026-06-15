using System;
using System.Linq;
using System.Text;

namespace Xcc.Application.Common
{
    public static class StringExtensions
    {
        public static string ConvertFirstLetterToLower(this string str)
        {
            if (str.Length == 0) return str;
            else if (str.Length == 1) return str.ToLower();

            return $"{str.FirstOrDefault().ToString().ToLower()}{str.Substring(1)}";
        }

        /// <summary>
        /// Allows 2 uppercase letters together, for example, TestWordAB -> test_wordAB
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string ToSnakeCase(this string str)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));

            if (str.Length < 2)
                return str;

            var sb = new StringBuilder();
            sb.Append(char.ToLowerInvariant(str[0]));
            for (int i = 1; i < str.Length; ++i)
            {
                char c = str[i];
                if (char.IsUpper(c))
                {
                    var j = i + 1;
                    if (j < str.Length)
                    {
                        if (char.IsUpper(str[j]))
                        {
                            sb.Append(c);
                            sb.Append(str[j]);
                            i = j;
                            continue;
                        }
                    }

                    sb.Append('_');
                    sb.Append(char.ToLowerInvariant(c));
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// For example, test_wordAB_c -> TestWordABC
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string ToCamelCase(this string str)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));

            if (str.Length < 2)
                return str;

            var sb = new StringBuilder();
            sb.Append(char.ToUpperInvariant(str[0]));
            bool toUpper = false;
            for (int i = 1; i < str.Length; ++i)
            {
                char c = str[i];
                if (c == '_')
                {
                    toUpper = true;
                    continue;
                }

                if (toUpper)
                {
                    sb.Append(char.ToUpperInvariant(c));
                    toUpper = false;
                }
                else
                    sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
