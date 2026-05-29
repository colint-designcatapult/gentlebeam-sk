using System;

namespace Xcc.Core.Exceptions
{
    public class DataServiceUnavailableException(
        string message = "", 
        Exception? inner = null) 
        : DataServiceException(message, inner)
    {
    }
}
