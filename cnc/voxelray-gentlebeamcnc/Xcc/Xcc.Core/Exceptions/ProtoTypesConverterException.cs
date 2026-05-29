using System;

namespace Xcc.Core.Exceptions
{
    public class ProtoTypesConverterException : Exception
    {
        public ProtoTypesConverterException()
        {
        }

        public ProtoTypesConverterException(string message)
            : base(message)
        {
        }

        public ProtoTypesConverterException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
