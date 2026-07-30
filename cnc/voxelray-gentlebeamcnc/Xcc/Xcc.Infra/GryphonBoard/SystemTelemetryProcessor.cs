using System;
using Empyrean.Common.Infra.Networking.Udp;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Core.Models;
using Xcc.Infra.GryphonBoard.CommandAPI;

namespace Xcc.Infra.GryphonBoard;

public interface ISystemTelemetryProcessor
{
    bool Process(byte[] datagram);
    bool Process(byte[] datagram, DateTimeOffset receivedAtUtc);
    void NotifyTelemetryExpired();
}

public sealed class SystemTelemetryProcessor : ISystemTelemetryProcessor
{
    private const uint PublicationIntervalMilliseconds = 250;
    private static readonly FirmwareVersionSignature NormalSignature = new(2, 0, 1, FirmwareMode.Normal);
    private static readonly FirmwareVersionSignature CalibrationSignature = new(1, 0, 0, FirmwareMode.Calibration);

    private readonly ISystemTelemetryChanged _systemTelemetryChangedCallback;
    private readonly IGCBDataStore _gcbDataStore;
    private readonly IDecodedTelemetryFrameSink _decodedTelemetryFrameSink;
    private readonly UdpPacket _packet = new();
    private FirmwareVersionSignature? _selectedVersion;
    private NormalTelemetryState? _normalState;
    private CalibrationTelemetryState? _calibrationState;
    private uint? _lastPublishedRuntime;

    public SystemTelemetryProcessor(
        ISystemTelemetryChanged systemTelemetryChangedCallback,
        IGCBDataStore gcbDataStore,
        IDecodedTelemetryFrameSink decodedTelemetryFrameSink)
    {
        _systemTelemetryChangedCallback = systemTelemetryChangedCallback;
        _gcbDataStore = gcbDataStore;
        _decodedTelemetryFrameSink = decodedTelemetryFrameSink;
    }

    public bool Process(byte[] datagram) => Process(datagram, default);

    public bool Process(byte[] datagram, DateTimeOffset receivedAtUtc)
    {
        if (!_packet.TryReset(datagram))
            return false;

        if (_packet.PacketType == (uint)GCBPacketType.FaultInfoResponse)
        {
            _gcbDataStore.ApplyFaultUpdate(FaultEntryParser.Parse(_packet));
            return false;
        }

        if (_packet.PacketType == (uint)GCBPacketType.VersionInfoResponse)
        {
            SelectTelemetryParser(_packet);
            return false;
        }

        if (_packet.PacketType != (uint)GCBPacketType.TelemetryResponse || _selectedVersion is null)
            return false;

        if (_selectedVersion == NormalSignature)
        {
            if (_packet.PayloadLength != (uint)NormalTelemetryField.PayloadFields)
            {
                return false;
            }

            _normalState!.Update(_packet);
            Publish(_normalState, datagram, receivedAtUtc);
            return true;
        }

        if (_selectedVersion == CalibrationSignature)
        {
            if (_packet.PayloadLength != 47u)
                return false;

            _calibrationState!.Update(_packet);
            Publish(_calibrationState, datagram, receivedAtUtc);
            return true;
        }

        return false;
    }

    public void NotifyTelemetryExpired() =>
        _systemTelemetryChangedCallback.OnSystemTelemetryChanged(null);

    private void SelectTelemetryParser(UdpPacket packet)
    {
        if (packet.PayloadLength != 5u)
        {
            ClearSelection();
            return;
        }

        var version = VersionInfoParser.Parse(packet);
        var signature = new FirmwareVersionSignature(
            version.Major,
            version.Minor,
            version.Level,
            version.Mode);

        if (signature == NormalSignature)
        {
            if (_selectedVersion != signature || _normalState is null)
                _normalState = new NormalTelemetryState();

            _calibrationState = null;
            _selectedVersion = signature;
            _lastPublishedRuntime = null;
            return;
        }

        if (signature == CalibrationSignature)
        {
            if (_selectedVersion != signature || _calibrationState is null)
                _calibrationState = new CalibrationTelemetryState();

            _normalState = null;
            _selectedVersion = signature;
            _lastPublishedRuntime = null;
            return;
        }

        ClearSelection();
    }

    private void Publish(NormalTelemetryState state, byte[] datagram, DateTimeOffset receivedAtUtc)
    {
        (bool publishCurrentValue, bool publishDecodedFrame) = GetPublicationTargets(state.Runtime);
        if (!publishCurrentValue && !publishDecodedFrame)
            return;

        PublishSnapshot(
            state.Runtime,
            state.Snapshot(),
            datagram,
            receivedAtUtc,
            publishCurrentValue,
            publishDecodedFrame);
    }

    private void Publish(CalibrationTelemetryState state, byte[] datagram, DateTimeOffset receivedAtUtc)
    {
        (bool publishCurrentValue, bool publishDecodedFrame) = GetPublicationTargets(state.Runtime);
        if (!publishCurrentValue && !publishDecodedFrame)
            return;

        PublishSnapshot(
            state.Runtime,
            state.Snapshot(),
            datagram,
            receivedAtUtc,
            publishCurrentValue,
            publishDecodedFrame);
    }

    private (bool CurrentValue, bool DecodedFrame) GetPublicationTargets(uint runtime) =>
        (IsPublicationDue(runtime), _decodedTelemetryFrameSink.IsEnabled);

    private void PublishSnapshot(
        uint runtime,
        ISystemTelemetry snapshot,
        byte[] datagram,
        DateTimeOffset receivedAtUtc,
        bool publishCurrentValue,
        bool publishDecodedFrame)
    {
        if (publishDecodedFrame)
        {
            DateTimeOffset timestamp = receivedAtUtc == default
                ? DateTimeOffset.UtcNow
                : receivedAtUtc;
            _decodedTelemetryFrameSink.Publish(
                new DecodedTelemetryFrame(timestamp, snapshot, datagram));
        }

        if (publishCurrentValue)
        {
            _systemTelemetryChangedCallback.OnSystemTelemetryChanged(snapshot);
            _lastPublishedRuntime = runtime;
        }
    }

    private bool IsPublicationDue(uint runtime) =>
        _lastPublishedRuntime is not uint lastRuntime
        || (runtime >= lastRuntime && runtime - lastRuntime >= PublicationIntervalMilliseconds);

    private void ClearSelection()
    {
        _selectedVersion = null;
        _normalState = null;
        _calibrationState = null;
        _lastPublishedRuntime = null;
    }

    private readonly record struct FirmwareVersionSignature(
        int Major,
        int Minor,
        int Level,
        FirmwareMode Mode);
}
