using Moq;
using Xcc.Application.UI.Converters;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;

namespace Xcc.Test.Xcc.Application.UI.Converters;

internal class GcbStatusToRadiationHazardActiveStateConverterTests
{
    private readonly GcbStatusToRadiationHazardActiveStateConverter _converter = new();

    [TestCase(GcbStateNew.Emission, false, true)]
    [TestCase(GcbStateNew.Imaging, false, true)]
    [TestCase(GcbStateNew.Ready, true, true)]
    [TestCase(GcbStateNew.Ready, false, false)]
    public void Convert_ActivatesForEmissionStateOrHvpsEmissionFlag(
        GcbStateNew controlBoardState,
        bool hvpsEmissionOn,
        bool expected)
    {
        var telemetry = new Mock<ISystemTelemetry>();
        telemetry.SetupGet(value => value.ControlBoardState).Returns(controlBoardState);
        telemetry.SetupGet(value => value.Hvps).Returns(new HvpsTelemetryStatus(
            hvpsEmissionOn ? 1u << 5 : 0,
            0,
            null));

        var result = _converter.Convert(
            telemetry.Object,
            typeof(bool),
            null!,
            null!);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void Convert_WhenTelemetryIsUnavailable_ReturnsInactive()
    {
        Assert.Multiple(() =>
        {
            Assert.That(
                _converter.Convert(null!, typeof(bool), null!, null!),
                Is.EqualTo(false));
            Assert.That(
                _converter.Convert(System.Windows.DependencyProperty.UnsetValue, typeof(bool), null!, null!),
                Is.EqualTo(false));
        });
    }
}
