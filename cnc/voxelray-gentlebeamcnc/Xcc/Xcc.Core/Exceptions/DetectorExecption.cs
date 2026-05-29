using System;

namespace Xcc.Core.Exceptions
{
    public class DetectorExecption : Exception
    {
        public DetectorExecption()
        {
        }

        public DetectorExecption(string message) : base(message)
        {
        }

        public DetectorExecption(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
