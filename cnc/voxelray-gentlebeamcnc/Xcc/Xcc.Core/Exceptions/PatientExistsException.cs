using System;

namespace Xcc.Core.Exceptions
{
    [Serializable]
    public class PatientExistsException : InvalidOperationException
    {
        public PatientExistsException()
        {
        }

        public PatientExistsException(string message) : base(message)
        {
        }

        public PatientExistsException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}