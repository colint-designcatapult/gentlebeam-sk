using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Infra.GryphonBoard;

namespace Xcc.Test.Xcc.Core.Models;

internal class SystemTelemetryTests
{
    [TestCase(GcbStateNew.Fault, true)]
    [TestCase(GcbStateNew.ColdFault, true)]
    [TestCase(GcbStateNew.WarmupFault, true)]
    [TestCase(GcbStateNew.Ready, false)]
    public void IsFaultState_UsesPublishedState(GcbStateNew state, bool expected)
    {
        ISystemTelemetry telemetry = new SystemNormalTelemetry { ControlBoardState = state };
        Assert.That(telemetry.IsFaultState(), Is.EqualTo(expected));
    }

    [TestCase(GcbStateNew.Emission, true)]
    [TestCase(GcbStateNew.Imaging, true)]
    [TestCase(GcbStateNew.Ready, false)]
    public void IsEmissionState_UsesPublishedState(GcbStateNew state, bool expected)
    {
        ISystemTelemetry telemetry = new SystemCalibrationTelemetry { ControlBoardState = state };
        Assert.That(telemetry.IsEmissionState(), Is.EqualTo(expected));
    }

    [Test]
    public void Formatting_UsesSemanticTelemetryContract()
    {
        ISystemTelemetry telemetry = new SystemCalibrationTelemetry
        {
            ControlBoardState = GcbStateNew.Ready,
            Faults = new SystemFaults(0x12, 0x34, 0, ulong.MaxValue),
            Interlocks = new SystemInterlocks(0x56, null, 0, 0),
            Hvps = new HvpsTelemetryStatus(0x78, 0x9A, 0xBC),
            KvFeedback = 1.25f,
        };

        var horizontal = telemetry.ToString();
        var vertical = telemetry.GetVerticallyFormattedString();

        Assert.Multiple(() =>
        {
            Assert.That(horizontal, Does.Contain("FirmwareMode: Calibration"));
            Assert.That(horizontal, Does.Contain("Faults: No active faults"));
            Assert.That(horizontal, Does.Contain("Hvps: HvpsTelemetryStatus"));
            Assert.That(horizontal, Does.Not.Contain("HvpsIOStatus"));
            Assert.That(vertical, Does.Match(@"ControlBoardState\s*: Ready"));
            Assert.That(vertical, Does.Match(@"KvFeedback\s*: 1\.250"));
        });
    }
}
