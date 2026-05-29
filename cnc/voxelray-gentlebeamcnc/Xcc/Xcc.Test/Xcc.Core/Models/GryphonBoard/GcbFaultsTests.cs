using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;

namespace Xcc.Test.Xcc.Core.Models.GryphonBoard
{
    public class GcbFaultsTests
    {
        [Test]
        public void Ctor_CheckFault()
        {
            var sut = new GcbFaults(6);

            Assert.That(sut.FaultDetails, Is.EqualTo(GCBFaultDetails.Reserved));
            
            Assert.That(sut.CheckFault(GCBFaultBit.Reserved), Is.False);
            Assert.That(sut.CheckFault(GCBFaultBit.InterlockFault), Is.True);
            Assert.That(sut.CheckFault(GCBFaultBit.HvpsReportedFault), Is.True);
            Assert.That(sut.CheckFault(GCBFaultBit.VoltageFault), Is.False);
        }

        [Test]
        public void Constructor_WithBitAndDetails()
        {
            var bit = GCBFaultBit.InterlockFault;
            var details = GCBFaultDetails.AcdBusSetupIssue;
            
            var sut = new GcbFaults(bit, details);
        
            Assert.That(sut.CheckFault(bit), Is.True);
            Assert.That(sut.FaultDetails, Is.EqualTo(details));
        }
        
        public static IEnumerable<TestCaseData> GetFaultsCases()
        {
            yield return new TestCaseData(0x0, Array.Empty<GCBFaultBit>())
                .SetName("0 - {}");
            
            yield return new TestCaseData(0x6, new[] {
                    GCBFaultBit.HvpsReportedFault,  
                    GCBFaultBit.InterlockFault })
                .SetName("6 - {InterlockFault, HvpsReportedFault}");
        }
        
        [TestCaseSource(nameof(GetFaultsCases))]
        public void GetFaults(int flags, GCBFaultBit[] expectedFaults)
        {
            var sut = new GcbFaults(flags);
            var faults = sut.GetFaults().ToList();
            Assert.That(faults, Is.EquivalentTo(expectedFaults));
        }
    }
}