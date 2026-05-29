using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xcc.Infra.Persistence.CSV.Types;

namespace Xcc.Infra.Persistence.CSV
{
    public class CsvWrongFormatException : Exception
    {
        public CsvWrongFormatException(string message) : base(message) { }
    }

    public class CsvReader : IDisposable
    {
        static Regex TableNameRegex = new Regex("^(.*)\\sTable:$");
        public StreamReader? Stream { get; set; }

        public CsvReader(StreamReader stream)
        {
            Stream = stream;
        }

        public ICollection<T> ReadTable<T>()
            where T : class, new()
        {
            ICollection<string> allTypeProperties = GetPropertyList<T>().ToHashSet();

            ICollection<string>? propertiesHeader = ReadRow();

            // Validate that all properties from header are present in the type T:
            if (propertiesHeader is null)
            {
                throw new InvalidOperationException("CsvReader.ReadTable<T> error: header row is empty");
            }
            else 
            {
                foreach (string property in propertiesHeader)
                {
                    if (!allTypeProperties.Contains(property))
                    {
                        throw new CsvWrongFormatException($"Invalid CSV format: {property} property is not supported");
                    }
                }
                return ReadRecords<T>(propertiesHeader);
            }
        }

        public string? SeekTableName()
        {
            string? tableName = null;
            do 
            {
                var tableRow = ReadRow();
                
                if (tableRow == null)
                {
                    break; // end of file
                }
                else if (tableRow.Count == 1)
                { // Skip rows with comma separators presented to get a table name row:
                    Match match = TableNameRegex.Match(tableRow.First());
                    if (match.Success && match.Groups.Count > 1)
                    {
                        tableName = match.Groups[1].Value;
                    }
                }
            } while(string.IsNullOrEmpty(tableName));
                
            return tableName;
        }

        private ICollection<string>? ReadRow()
        {
            if (Stream == null)
                throw new NullReferenceException("CsvReader.ReadRow: no stream to read");
            else
            {
                return Stream.ReadLine()?.Split(',');
            }
        }

        private static IList<string> GetPropertyList<T>()
        {
            return typeof(T).GetProperties().Select(prop => prop.Name).ToList();
        }

        private ICollection<T> ReadRecords<T>(ICollection<string> properties)
            where T : class, new()
        {
            ICollection<T> records = new List<T>();
            
            while (ReadRecord<T>(properties) is T record)
            {
                records.Add(record);
            }
            
            return records;
        }

        public T? ReadRecord<T>(ICollection<string> properties)
            where T: class, new()
        {
            var row = ReadRow();
            if (row != null && row.Any(s => !string.IsNullOrEmpty(s)))
            {
                if (row.Count != properties.Count)
                {
                    throw new CsvWrongFormatException($"Invalid CSV format: table row does not match its header");
                }
                
                T record = new T();
                var propertyValueList = properties.Zip(row);
                foreach (var property in propertyValueList)
                {
                    string name = property.First;
                    string value = property.Second;
                    var propertyInfo = typeof(T).GetProperty(name);
                    if (propertyInfo is null)
                    {
                        throw new NullReferenceException($"CsvReader.ReadRecord<T> error: property info is null for {name}");
                    }
                    else if (propertyInfo.PropertyType == typeof(string))
                    {
                        propertyInfo.SetValue(record, value, null);
                    }
                    else if (propertyInfo.PropertyType.IsValueType)
                    {
                        var convertedValue = Convert.ChangeType(value, propertyInfo.PropertyType);
                        propertyInfo.SetValue(record, convertedValue, null);
                    }
                    else if (propertyInfo.GetValue(record, null) is ICsvMappedType csvType)
                    {
                        csvType.TryParse(value);
                    }
                    else
                    {
                        throw new CsvWrongFormatException($"Invalid CSV format: cannot parse \"{value}\" into {name} property");
                    }
                }
                
                return record;
            }
            else 
            {
                return null;
            }
        }

        public void Dispose()
        {
            Stream = null;
        }
    }
}
