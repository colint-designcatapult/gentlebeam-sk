using Empyrean.Common.Infra.Networking.Udp;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Core.Models;
using Xcc.Infra.GryphonBoard.CommandAPI;

namespace Xcc.Infra.GryphonBoard;

public interface ISystemTelemetryProcessor
{
    bool Process(byte[] datagram);
    void NotifyTelemetryExpired();
}

public sealed class SystemTelemetryProcessor : ISystemTelemetryProcessor
{
    private const uint PublicationIntervalMilliseconds = 250;
    private static readonly FirmwareVersionSignature NormalSignature = new(2, 0, 1, FirmwareMode.Normal);
    private static readonly FirmwareVersionSignature CalibrationSignature = new(1, 0, 0, FirmwareMode.Calibration);

    private readonly ISystemTelemetryChanged _systemTelemetryChangedCallback;
    private readonly IGCBDataStore _gcbDataStore;
    private readonly UdpPacket _packet = new();
    private FirmwareVersionSignature? _selectedVersion;
    private NormalTelemetryState? _normalState;
    private CalibrationTelemetryState? _calibrationState;
    private uint? _lastPublishedRuntime;

    public SystemTelemetryProcessor(
        ISystemTelemetryChanged systemTelemetryChangedCallback,
        IGCBDataStore gcbDataStore)
    {
        _systemTelemetryChangedCallback = systemTelemetryChangedCallback;
        _gcbDataStore = gcbDataStore;
    }

    public bool Process(byte[] datagram)
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
            PublishIfDue(_normalState);
            return true;
        }

        if (_selectedVersion == CalibrationSignature)
        {
            if (_packet.PayloadLength != 47u)
                return false;

            _calibrationState!.Update(_packet);
            PublishIfDue(_calibrationState);
            return true;
        }

        return false;
    }

    public void NotifyTelemetryExpired() =>
        _systemTelemetryChangedCallback.OnSystemTelemetryChanged(null);

    private void SelectTelemetryParser(UdpPacket packet)
    {
        if (packet.PayloadLength != 19u)
        {
            ClearSelection();
            return;
        }

        var version = VersionInfoParser.Parse(packet);
        var signature = version.Mode switch
        {
            FirmwareMode.Normal => NormalSignature,
            FirmwareMode.Calibration => CalibrationSignature,
            _ => (FirmwareVersionSignature?)null,
        };

        if (signature is null)
        {
            ClearSelection();
            return;
        }
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

    private void PublishIfDue(NormalTelemetryState state)
    {
        if (!IsPublicationDue(state.Runtime))
            return;

        _systemTelemetryChangedCallback.OnSystemTelemetryChanged(state.Snapshot());
        _lastPublishedRuntime = state.Runtime;
    }

    private void PublishIfDue(CalibrationTelemetryState state)
    {
        if (!IsPublicationDue(state.Runtime))
            return;

        _systemTelemetryChangedCallback.OnSystemTelemetryChanged(state.Snapshot());
        _lastPublishedRuntime = state.Runtime;
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
