using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xcc.Core.Helpers;

namespace Xcc.Infra.Persistence.CSV
{
    public class CsvWriter(StreamWriter stream) : IDisposable
    {
        public StreamWriter? Stream { get; set; } = stream;

        public void WriteTable<T>(string name, ICollection<T> objects)
        {
#if !(NET7_0_OR_GREATER)
    // This approach relies on the GetPropertyValues returning the values in a particular order,
    // see https://learn.microsoft.com/en-us/dotnet/api/system.type.getproperties?view=net-8.0
#error Can't ensure the right property order in GetProperties() reflection method
#endif

            var helper = new ObjectDeconstructionHelper<T>();

            WriteTableCaption(name);
            WriteRow(helper.PropertyList);
            foreach (var obj in objects)
            {
                var values = helper.GetPropertyValues(obj).Select(x => x?.ToString() ?? string.Empty);
                WriteRow(values);
            }
        }

        private void WriteTableCaption(string name)
        {
            Stream?.WriteLine($"{name} Table:");
        }

        private void WriteRow(IEnumerable<string> values)
        {
            Stream?.WriteLine(string.Join(',', values));
        }

        private static IList<string> GetPropertyList<T>()
        {
            return typeof(T).GetProperties().Select(prop => prop.Name).ToList();
        }

        public void Dispose()
        {
            Stream = null;
        }
    }
}
