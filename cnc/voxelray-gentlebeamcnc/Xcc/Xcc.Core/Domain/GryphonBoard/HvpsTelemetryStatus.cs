namespace Xcc.Core.Domain.GryphonBoard;

public readonly record struct HvpsTelemetryStatus(
    uint RawStatusFlags,
    uint RawIoFlags,
    uint? RawErrorFlags)
{
    public bool TestMode => IsStatusSet(0);
    public bool HighVoltageControlEnabled => IsStatusSet(1);
    public bool GridControlEnabled => IsStatusSet(2);
    public bool Warming => IsStatusSet(3);
    public bool KilovoltageRamping => IsStatusSet(4);
    public bool EmissionOn => IsStatusSet(5);
    public bool ConfigurationUnlocked => IsStatusSet(6);
    public bool PidEnabled => IsStatusSet(7);
    public bool CalibrationGridInterlockEnabled => IsStatusSet(8);
    public bool FastWarmupEnabled => IsStatusSet(9);

    public bool GridClockStatus => IsIoSet(0);
    public bool FilamentClockFault => IsIoSet(1);
    public bool GridInterlock => IsIoSet(2);
    public bool BeamControl => IsIoSet(3);
    public bool GridStatus => IsIoSet(4);
    public bool CathodeArc => IsIoSet(5);
    public bool FanFault => IsIoSet(6);
    public bool PowerFactorCorrectionOk => IsIoSet(7);
    public bool HighVoltageInterlock => IsIoSet(8);
    public bool HighVoltageStatus => IsIoSet(9);
    public bool Overcurrent24VoltFault => IsIoSet(10);
    public bool MasterFault => IsIoSet(11);
    public bool HighVoltageOvercurrentFault => IsIoSet(12);
    public bool Temperature1Fault => IsIoSet(13);
    public bool CathodeOvercurrentFault => IsIoSet(14);
    public bool Temperature3Fault => IsIoSet(15);
    public bool Temperature2Fault => IsIoSet(16);

    public uint UnknownStatusFlags => RawStatusFlags & 0xFFFFFC00u;
    public uint UnknownIoFlags => RawIoFlags & 0xFFFE0000u;
    public bool HasActiveFaultInput => (RawIoFlags & 0x0001FC62u) != 0;

    private bool IsStatusSet(int bit) => (RawStatusFlags & (1u << bit)) != 0;
    private bool IsIoSet(int bit) => (RawIoFlags & (1u << bit)) != 0;
}
