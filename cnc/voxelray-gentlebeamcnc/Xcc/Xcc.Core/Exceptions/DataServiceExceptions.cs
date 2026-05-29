using System;

namespace Xcc.Core.Exceptions
{
    public class DataServiceException : Exception
    {
        public DataServiceException()
        {
        }

        public DataServiceException(string message, Exception? inner = null)
            : base(message, inner)
        {
        }
    }
}
