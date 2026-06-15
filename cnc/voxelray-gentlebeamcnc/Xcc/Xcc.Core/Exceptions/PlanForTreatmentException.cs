using System;

namespace Xcc.Core.Exceptions
{
    public class PlanForTreatmentException : Exception
    {
        public PlanForTreatmentException()
        {
        }

        public PlanForTreatmentException(string message)
            : base(message)
        {
        }

        public PlanForTreatmentException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
