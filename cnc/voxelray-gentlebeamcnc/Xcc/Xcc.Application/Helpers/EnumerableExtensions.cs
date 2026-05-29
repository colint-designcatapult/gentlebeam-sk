using System.Collections.Generic;
using System.Linq;

namespace Xcc.Application.Helpers
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<(T item, int index)> Enumerate<T>(this IEnumerable<T> source)
        {
            return source.Select((item, index) => (item, index));
        }
    }
}
