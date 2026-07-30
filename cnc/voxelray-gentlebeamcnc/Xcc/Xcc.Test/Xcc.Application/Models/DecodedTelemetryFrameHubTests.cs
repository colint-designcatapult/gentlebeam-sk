using Moq;
using Xcc.Application.Models;
using Xcc.Core.Domain.GryphonBoard;

namespace Xcc.Test.Xcc.Application.Models;

internal sealed class DecodedTelemetryFrameHubTests
{
    [Test]
    public void Subscription_ControlsCaptureDemandAndDelivery()
    {
        var sut = new DecodedTelemetryFrameHub();
        int delivered = 0;

        Assert.That(sut.IsEnabled, Is.False);
        using IDisposable subscription = sut.Subscribe(_ => delivered++);
        Assert.That(sut.IsEnabled, Is.True);

        sut.Publish(CreateFrame());
        subscription.Dispose();
        sut.Publish(CreateFrame());

        Assert.Multiple(() =>
        {
            Assert.That(delivered, Is.EqualTo(1));
            Assert.That(sut.IsEnabled, Is.False);
        });
    }

    [Test]
    public void Publish_RemovesFaultingSubscriberWithoutInterruptingHealthySubscribers()
    {
        var sut = new DecodedTelemetryFrameHub();
        int faultingInvocations = 0;
        int healthyInvocations = 0;
        using IDisposable faulting = sut.Subscribe(_ =>
        {
            faultingInvocations++;
            throw new InvalidOperationException("Subscriber failure.");
        });
        using IDisposable healthy = sut.Subscribe(_ => healthyInvocations++);

        sut.Publish(CreateFrame());
        sut.Publish(CreateFrame());

        Assert.Multiple(() =>
        {
            Assert.That(faultingInvocations, Is.EqualTo(1));
            Assert.That(healthyInvocations, Is.EqualTo(2));
            Assert.That(sut.IsEnabled, Is.True);
        });
    }

    private static DecodedTelemetryFrame CreateFrame() => new(
        DateTimeOffset.UtcNow,
        Mock.Of<ISystemTelemetry>(),
        Array.Empty<byte>());
}
