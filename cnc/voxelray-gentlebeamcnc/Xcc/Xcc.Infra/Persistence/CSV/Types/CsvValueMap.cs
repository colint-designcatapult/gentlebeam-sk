using System.Collections.Generic;
using System.Linq;

namespace Xcc.Infra.Persistence.CSV.Types
{
    public interface ICsvValueMap<T>
    {
        string ToCsvString(T value);
        T ToValue(string csvValue);
    }

    /// <summary>
    /// Utility class for custom data to CSV string conversion
    /// for custom CSV table format
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="mapValueToString"></param>
    /// <throws>ArgumentException</throws> if there are duplicated values in the input mapping
    public class CsvValueMap<T>(IDictionary<T, string> mapValueToString) : ICsvValueMap<T>
    {
        public IDictionary<string, T> MapToValue { get; } = mapValueToString.ToDictionary(kv => kv.Value, kv => kv.Key);
        public IDictionary<T, string> MapToCSV { get; } = mapValueToString;

        /// <summary>
        /// Converts a value into its custom CSV string representation
        /// </summary>
        /// <param name="value"></param>
        /// <throws>KeyNotFoundException</throws> if the value key is not present in the mapping
        public string ToCsvString(T value)
        {
            return MapToCSV[value];
        }

        /// <summary>
        /// Converts a value from its custom CSV string representation
        /// </summary>
        /// <param name="csvValue"></param>
        /// <throws>KeyNotFoundException</throws> if the string value is not present in the mapping
        public T ToValue(string csvValue)
        {
            return MapToValue[csvValue];
        }
    }
}
