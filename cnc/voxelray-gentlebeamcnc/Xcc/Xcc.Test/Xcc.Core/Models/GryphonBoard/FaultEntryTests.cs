using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;

namespace Xcc.Test.Xcc.Core.Models.GryphonBoard
{
    public class FaultEntryTests
    {
        [Test]
        public void FaultEntry_GettersSetters(
            [Values(0, 1)] int faultId,
            [Values(SystemFault.Reserved, SystemFault.InterlockFault)] SystemFault faultType,
            [Values(GCBFaultDetails.Reserved, GCBFaultDetails.AcdBusSetupIssue)] GCBFaultDetails faultIdSupportingDetails,
            [Values(2, 3)] int faultEntryState,
            [Values(4, 5)] int faultTimeValue,
            [Values(0.1f, 1.1f)] float expectedParameter,
            [Values(6, 7)] int expectedParameterSupportingDetails,
            [Values(0.2f, 1.2f)] float parameterTolerance,
            [Values(0.3f, 1.3f)] float measuredParameter,
            [Values(8, 9)] int measuredParameterSupportingDetails)
        {
            var sut = new FaultEntry
            {
                FaultId = faultId,
                FaultType = faultType,
                FaultIdSupportingDetails = faultIdSupportingDetails,
                FaultEntryState = faultEntryState,
                FaultTimeValue = faultTimeValue,
                ExpectedParameter = expectedParameter,
                ExpectedParameterSupportingDetails = expectedParameterSupportingDetails,
                ParameterTolerance = parameterTolerance,
                MeasuredParameter = measuredParameter,
                MeasuredParameterSupportingDetails = measuredParameterSupportingDetails,
            };

            Assert.That(sut.FaultId, Is.EqualTo(faultId));
            Assert.That(sut.FaultType, Is.EqualTo(faultType));
            Assert.That(sut.FaultIdSupportingDetails, Is.EqualTo(faultIdSupportingDetails));
            Assert.That(sut.FaultEntryState, Is.EqualTo(faultEntryState));
            Assert.That(sut.FaultTimeValue, Is.EqualTo(faultTimeValue));
            Assert.That(sut.ExpectedParameter, Is.EqualTo(expectedParameter).Within(G.Precision));
            Assert.That(sut.ExpectedParameterSupportingDetails, Is.EqualTo(expectedParameterSupportingDetails));
            Assert.That(sut.ParameterTolerance, Is.EqualTo(parameterTolerance).Within(G.Precision));
            Assert.That(sut.MeasuredParameter, Is.EqualTo(measuredParameter).Within(G.Precision));
            Assert.That(sut.MeasuredParameterSupportingDetails, Is.EqualTo(measuredParameterSupportingDetails));
        }
        
        [TestCase((int)GcbStateNew.Startup, "Startup")]
        [TestCase((int)GcbStateNew.Cold, "Cold")]
        [TestCase((int)GcbStateNew.ColdFault, "ColdFault")]
        [TestCase((int)GcbStateNew.DailyWarmup, "DailyWarmup")]
        [TestCase((int)GcbStateNew.Warmup, "Warmup")]
        [TestCase((int)GcbStateNew.WarmupFault, "WarmupFault")]
        [TestCase((int)GcbStateNew.Primed, "Primed")]
        [TestCase((int)GcbStateNew.Staging, "Staging")]
        [TestCase((int)GcbStateNew.Staged, "Staged")]
        [TestCase((int)GcbStateNew.HvpsCheck, "HvpsCheck")]
        [TestCase((int)GcbStateNew.HVSetup, "HVSetup")]
        [TestCase((int)GcbStateNew.Ready, "Ready")]
        [TestCase((int)GcbStateNew.Launching, "Launching")]
        [TestCase((int)GcbStateNew.Emission, "Emission")]
        [TestCase((int)GcbStateNew.Termination, "Termination")]
        [TestCase((int)GcbStateNew.Discharge, "Discharge")]
        [TestCase((int)GcbStateNew.Fault, "Fault")]
        [TestCase((int)GcbStateNew.SystemCrash, "SystemCrash")]
        [TestCase((int)GcbStateNew.LaunchingForImaging, "LaunchingForImaging")]
        [TestCase((int)GcbStateNew.WaitForKey, "WaitForKey")]
        [TestCase((int)GcbStateNew.Imaging, "Imaging")]
        public void FaultEntryStateString_Valid(int state, string expected)
        {
            var sut = new FaultEntry { FaultEntryState = state };
            Assert.That(sut.FaultEntryStateString, Is.EqualTo(expected));
        }

        [Test]
        public void FaultEntry_ToString()
        {
            var sut = new FaultEntry
            {
                FaultId = 10,
                FaultType = SystemFault.InterlockFault,
                FaultIdSupportingDetails = GCBFaultDetails.AcdBusTimeoutIssue,
                FaultEntryState = 2,
                FaultTimeValue = 3,
                ExpectedParameter = 1.1f,
                ExpectedParameterSupportingDetails = 4,
                ParameterTolerance = 2.3f,
                MeasuredParameter = 3.4f,
                MeasuredParameterSupportingDetails = 5,
            };
            
            var res = sut.ToString();
            
            Assert.That(res, Does.Contain("FaultId: 10"));
            Assert.That(res, Does.Contain("FaultIdSupportingDetails: 2"));
            Assert.That(res, Does.Contain("FaultEntryState: 2"));
            Assert.That(res, Does.Contain("FaultTimeValue: 3"));
            Assert.That(res, Does.Contain("ExpectedParameter: 1.1"));
            Assert.That(res, Does.Contain("ExpectedParameterSupportingDetails: 4"));
            Assert.That(res, Does.Contain("ParameterTolerance: 2.3"));
            Assert.That(res, Does.Contain("MeasuredParameter: 3.4"));
            Assert.That(res, Does.Contain("MeasuredParameterSupportingDetails: 5"));   
        }
    }
}