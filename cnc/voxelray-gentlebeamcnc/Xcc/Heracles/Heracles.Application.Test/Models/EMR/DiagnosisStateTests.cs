using Heracles.Application.Models.EMR;
using Heracles.Application.Models.RDBMS.EMR;
using Heracles.Core.Models;

namespace Heracles.Application.Test.Models.EMR
{
    internal class DiagnosisStateTests
    {
        [Test]
        public void DefaultConstructionTest()
        {
            IDiagnosisState state = null!;
            
            Assert.DoesNotThrow(() => state = new DiagnosisState());
            Assert.That(state.IsModified, Is.False);
        }

        [Test]
        public void CopyConstructionTest()
        {
            var diagnosisSource = new Diagnosis()
            {
                Id = 1,
                SiteName = "name"
            };

            IDiagnosisState state = null!;
            Assert.DoesNotThrow(() => state = new DiagnosisState(diagnosisSource));
            Assert.That(state.IsModified, Is.False);
            Assert.That(state.Id, Is.EqualTo(diagnosisSource.Id));
            Assert.That(state.SiteName, Is.EqualTo(diagnosisSource.SiteName));

        }
    }
}
