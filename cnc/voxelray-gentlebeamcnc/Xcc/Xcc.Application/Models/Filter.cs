using System;

namespace Xcc.Application.Models
{
    public class Filter(Type field, string fieldName, object value)
    {
        public Type Field { get; set; } = field;
        public string FieldName { get; set; } = fieldName;
        public object Value { get; set; } = value;
        

        private bool Equals(Filter other)
        {
            if(ReferenceEquals(this, other))    
                return true;

            return (Field == other.Field) && Value.Equals(other.Value);
        }

        public override bool Equals(object? other)
        {
            return other is Filter filter && Equals(filter);
        }

        public override int GetHashCode() => Field.GetHashCode() ^ Value.GetHashCode();
    }
}
