using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Exceptions;

namespace Xcc.Test.Xcc.Core.Exceptions
{
    public class ExceptionsTests
    {
        [TestCase(typeof(DataServiceException))]
        [TestCase(typeof(DetectorExecption))]
        [TestCase(typeof(PatientExistsException))]
        [TestCase(typeof(PlanForTreatmentException))]
        [TestCase(typeof(ProtoTypesConverterException))]
        public void Ctor_Defaults(Type exceptionType)
        {
            var sut = (Exception)Activator.CreateInstance(exceptionType);
            Assert.That(sut, Is.Not.Null);
            Assert.That(sut.Message, Is.Not.Null);
        }

        [TestCase(typeof(GcbNoConnectionException))]
        [TestCase(typeof(DataServiceException))]
        [TestCase(typeof(DetectorExecption))]
        [TestCase(typeof(PatientExistsException))]
        [TestCase(typeof(PlanForTreatmentException))]
        [TestCase(typeof(ProtoTypesConverterException))]
        public void Ctor_WithMessage(Type exceptionType)
        {
            var message = "My message";
            var sut = (Exception)Activator.CreateInstance(exceptionType, message);

            Assert.That(sut.Message, Is.EqualTo(message));
        }

        [TestCase(typeof(DataServiceException))]
        [TestCase(typeof(DetectorExecption))]
        [TestCase(typeof(PatientExistsException))]
        [TestCase(typeof(PlanForTreatmentException))]
        [TestCase(typeof(ProtoTypesConverterException))]
        public void Ctor_WithMessageAndInnerException(Type exceptionType)
        {
            var message = "My message";
            var inner = new InvalidOperationException("My inner exception");
            var sut = (Exception)Activator.CreateInstance(exceptionType, message, inner);

            Assert.That(sut.Message, Is.EqualTo(message));
            Assert.That(sut.InnerException, Is.SameAs(inner));
        }
    }
}