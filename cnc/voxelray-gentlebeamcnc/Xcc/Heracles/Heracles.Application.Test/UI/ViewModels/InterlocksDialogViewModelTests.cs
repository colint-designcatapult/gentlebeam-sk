using Heracles.Core.Models;
using Heracles.Application.UI.ViewModels;
using Moq;
using Prism.Events;
using Prism.Ioc;
using Xcc.Application.Models;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Core.Models;
using Xcc.Core.Services;
using Xcc.Infra.GryphonBoard;

namespace Heracles.Application.Test.UI.ViewModels;

[NonParallelizable]
internal class InterlocksDialogViewModelTests
{
    private const ulong AvailableInterlocks = (uint)GcbInterlockFlags.All;

    [Test]
    public void TelemetryUpdate_ProgressivelyDisclosesOperatorChecksAndBadTechnicalGroups()
    {
        var previousContext = SynchronizationContext.Current;
        SynchronizationContext.SetSynchronizationContext(new ImmediateSynchronizationContext());

        try
        {
            var doorMask = 1UL << (int)SystemInterlock.DoorClosed;
            var masterFaultMask = 1UL << (int)SystemInterlock.MasterFaultClear;
            var store = new Mock<IGCBDataStore>();
            store.SetupGet(value => value.SystemTelemetry)
                .Returns(CreateTelemetry(AvailableInterlocks, doorMask));
            store.SetupGet(value => value.ActiveFaults).Returns(Array.Empty<FaultEntry>());
            var eventAggregator = new EventAggregator();
            var viewModel = new InterlocksDialogViewModel(
                store.Object,
                eventAggregator,
                Mock.Of<IHeraclesExternalSettings>(),
                CreateContainer(Mock.Of<IMainBoardAPI>()),
                Mock.Of<IPopUpService>());

            Assert.Multiple(() =>
            {
                Assert.That(viewModel.SystemIsReady, Is.True);
                Assert.That(viewModel.SystemReadinessText, Is.EqualTo("Ready"));
                Assert.That(viewModel.OperatorInterlocks.Select(item => item.DisplayName),
                    Is.EqualTo(new[] { "E-stops", "Door closed", "Keys" }));
                Assert.That(viewModel.EStops.State, Is.True);
                Assert.That(viewModel.EStops.ShowDetails, Is.False);
                Assert.That(viewModel.Keys.State, Is.True);
                Assert.That(viewModel.Keys.ShowDetails, Is.False);
                Assert.That(viewModel.AttentionItems, Is.Empty);
                Assert.That(viewModel.HasAttentionItems, Is.False);
                Assert.That(viewModel.ShowClearFaultsButton, Is.False);
            });

            var remoteEStopMask = 1UL << (int)SystemInterlock.RemoteEStopReleased;
            var remoteKeyMask = 1UL << (int)SystemInterlock.RemoteKeyOn;
            var robotArmMask = 1UL << (int)SystemInterlock.Kuka1Ready;
            var waterTemperatureMask = 1UL << (int)SystemInterlock.WaterTemperatureOk;
            var timer2Mask = 1UL << (int)SystemInterlock.Timer2Ready;
            var spareInterlock2Mask = 1UL << (int)SystemInterlock.SpareInterlock2;
            eventAggregator.GetEvent<SystemTelemetryChangedEvent>().Publish(
                CreateTelemetry(AvailableInterlocks & ~remoteEStopMask, remoteEStopMask));
            Assert.That(viewModel.ShowClearFaultsButton, Is.False);

            var problemMask =
                remoteEStopMask
                | remoteKeyMask
                | robotArmMask
                | waterTemperatureMask
                | timer2Mask
                | spareInterlock2Mask
                | masterFaultMask;
            var problemTelemetry = CreateTelemetry(
                AvailableInterlocks & ~problemMask,
                remoteEStopMask);
            eventAggregator.GetEvent<SystemTelemetryChangedEvent>().Publish(problemTelemetry);

            Assert.Multiple(() =>
            {
                Assert.That(viewModel.SystemIsReady, Is.False);
                Assert.That(viewModel.SystemReadinessText, Is.EqualTo("Attention required"));
                Assert.That(viewModel.EStops.State, Is.False);
                Assert.That(viewModel.EStops.ShowDetails, Is.True);
                Assert.That(viewModel.EStops.Details.Single(item => item.DisplayName == "Base e-stop").State, Is.True);
                Assert.That(viewModel.EStops.Details.Single(item => item.DisplayName == "Remote e-stop").State, Is.False);
                Assert.That(viewModel.Door.State, Is.True);
                Assert.That(viewModel.Keys.State, Is.False);
                Assert.That(viewModel.Keys.ShowDetails, Is.True);
                Assert.That(viewModel.AttentionItems.Select(item => item.DisplayName),
                    Is.EqualTo(new[] { "Robot arm", "Cooling system", "Backup timers", "Auxiliary safety circuit" }));
                Assert.That(viewModel.AttentionItems
                    .Single(item => item.DisplayName == "Auxiliary safety circuit")
                    .Details.Single(item => item.DisplayName == "Spare interlock 2").State,
                    Is.False);
                Assert.That(viewModel.AttentionItems.Select(item => item.DisplayName),
                    Does.Not.Contain("Master fault"));
                Assert.That(viewModel.ShowClearFaultsButton, Is.True);
            });

            var systemFaultTelemetry = CreateTelemetry(
                AvailableInterlocks,
                doorMask,
                hasActiveFault: true);
            eventAggregator.GetEvent<SystemTelemetryChangedEvent>().Publish(systemFaultTelemetry);
            var activeFaults = new[]
            {
                new FaultEntry(
                    SystemFault.VoltageFault,
                    1,
                    GcbStateNew.Ready,
                    100,
                    "Voltage feedback exceeded target.",
                    "Voltage feedback exceeded target."),
                new FaultEntry(
                    SystemFault.CoilFault,
                    2,
                    GcbStateNew.Ready,
                    101,
                    "X-coil current missed target.",
                    "X-coil current missed target."),
            };
            eventAggregator.GetEvent<FaultsChangedEvent>().Publish(activeFaults);

            Assert.Multiple(() =>
            {
                Assert.That(viewModel.SystemIsReady, Is.False);
                Assert.That(viewModel.AttentionItems.Select(item => item.DisplayName),
                    Is.EqualTo(activeFaults.Select(fault => fault.Message)));
                Assert.That(viewModel.AttentionItems, Has.All.Property(nameof(InterlockGroupStatusItem.State)).False);
                Assert.That(viewModel.ShowClearFaultsButton, Is.True);
            });

            eventAggregator.GetEvent<FaultsChangedEvent>().Publish(Array.Empty<FaultEntry>());

            var masterOnlyTelemetry = CreateTelemetry(
                AvailableInterlocks & ~masterFaultMask,
                doorMask);
            eventAggregator.GetEvent<SystemTelemetryChangedEvent>().Publish(masterOnlyTelemetry);

            Assert.Multiple(() =>
            {
                Assert.That(viewModel.SystemIsReady, Is.False);
                Assert.That(viewModel.EStops.ShowDetails, Is.False);
                Assert.That(viewModel.Keys.ShowDetails, Is.False);
                Assert.That(viewModel.AttentionItems.Select(item => item.DisplayName),
                    Is.EqualTo(new[] { "Master fault" }));
                Assert.That(viewModel.ShowClearFaultsButton, Is.True);
            });
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(previousContext);
        }
    }

    [Test]
    public void ClearFaultsButton_IsHiddenForMainApplication()
    {
        var previousContext = SynchronizationContext.Current;
        SynchronizationContext.SetSynchronizationContext(new ImmediateSynchronizationContext());

        try
        {
            var masterFaultMask = 1UL << (int)SystemInterlock.MasterFaultClear;
            var store = new Mock<IGCBDataStore>();
            store.SetupGet(value => value.SystemTelemetry)
                .Returns(CreateTelemetry(AvailableInterlocks & ~masterFaultMask, 0));
            store.SetupGet(value => value.ActiveFaults)
                .Returns([
                    new FaultEntry(
                        SystemFault.VoltageFault,
                        1,
                        GcbStateNew.Ready,
                        100,
                        "Voltage feedback exceeded target.",
                        "Voltage feedback exceeded target."),
                ]);

            var containerProvider = new Mock<IContainerProvider>(MockBehavior.Strict);
            var viewModel = new InterlocksDialogViewModel(
                store.Object,
                new EventAggregator(),
                Mock.Of<IHeraclesCoreSettings>(),
                containerProvider.Object,
                Mock.Of<IPopUpService>());

            Assert.That(viewModel.ShowClearFaultsButton, Is.False);
            containerProvider.VerifyNoOtherCalls();
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(previousContext);
        }
    }

    [Test]
    public async Task ClearFaultsCommand_ClearsMainBoardFaults()
    {
        var previousContext = SynchronizationContext.Current;
        SynchronizationContext.SetSynchronizationContext(new ImmediateSynchronizationContext());

        try
        {
            var commandInvoked = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var mainBoardApi = new Mock<IMainBoardAPI>();
            mainBoardApi
                .Setup(api => api.ClearFaults())
                .Returns(() =>
                {
                    commandInvoked.SetResult();
                    return Task.CompletedTask;
                });
            var store = new Mock<IGCBDataStore>();
            store.SetupGet(value => value.SystemTelemetry)
                .Returns(CreateTelemetry(AvailableInterlocks, 0));
            store.SetupGet(value => value.ActiveFaults).Returns(Array.Empty<FaultEntry>());
            var viewModel = new InterlocksDialogViewModel(
                store.Object,
                new EventAggregator(),
                Mock.Of<IHeraclesExternalSettings>(),
                CreateContainer(mainBoardApi.Object),
                Mock.Of<IPopUpService>());

            viewModel.ClearFaultsCommand.Execute();
            await commandInvoked.Task.WaitAsync(TimeSpan.FromSeconds(1));

            mainBoardApi.Verify(api => api.ClearFaults(), Times.Once);
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(previousContext);
        }
    }

    private static IContainerProvider CreateContainer(IMainBoardAPI mainBoardApi)
    {
        var containerProvider = new Mock<IContainerProvider>();
        containerProvider
            .Setup(provider => provider.Resolve(typeof(IMainBoardAPI)))
            .Returns(mainBoardApi);
        return containerProvider.Object;
    }

    private static SystemNormalTelemetry CreateTelemetry(
        ulong activeMask,
        ulong requiredMask,
        bool hasActiveFault = false)
    {
        var activeFaults = hasActiveFault ? 1UL << (int)SystemFault.VoltageFault : 0;
        return new SystemNormalTelemetry
        {
            Faults = new SystemFaults((uint)activeFaults, null, activeFaults, ulong.MaxValue),
            Interlocks = new SystemInterlocks(
                (uint)activeMask,
                (uint)requiredMask,
                activeMask,
                AvailableInterlocks,
                requiredMask),
        };
    }

    private sealed class ImmediateSynchronizationContext : SynchronizationContext
    {
        public override void Post(SendOrPostCallback callback, object? state) => callback(state);
    }
}
