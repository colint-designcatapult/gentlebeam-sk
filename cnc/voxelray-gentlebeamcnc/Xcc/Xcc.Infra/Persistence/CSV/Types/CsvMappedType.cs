using System.Collections.Generic;

namespace Xcc.Infra.Persistence.CSV.Types
{
    public interface ICsvMappedType
    {
        bool TryParse(string value);
    }

    public class CsvMappedType<T> : ICsvMappedType
    {
        public T Value { get; set; }
        public ICsvValueMap<T> Mapping { get; }

        public CsvMappedType(T value, ICsvValueMap<T> mapping)
        {
            Value = value;
            Mapping = mapping;
        }

        public override string ToString()
        {
            return Mapping.ToCsvString(Value);
        }

        public bool TryParse(string csvValue)
        {
            Value = Mapping.ToValue(csvValue);
            return true;
        }
    }

    public class CsvBool : CsvMappedType<bool>
    {
        public class CsvBoolMap : CsvValueMap<bool>
        {
            static IDictionary<bool, string> boolToString = new Dictionary<bool, string>() { { false, "FALSE" }, { true, "TRUE" } };
            public CsvBoolMap() : base(boolToString) { }
        }

        static CsvBoolMap map = new();
        public CsvBool(bool value = false) : base(value, map) { }
    }
}
