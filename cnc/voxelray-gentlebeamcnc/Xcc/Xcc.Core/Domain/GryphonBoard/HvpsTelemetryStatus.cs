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
    public bool FilamentClockFault => IsStatusSet(10);
    public bool GridInterlock => IsIoSet(2);
    public bool BeamControl => IsIoSet(3);
    public bool GridStatus => IsIoSet(4);
    public bool CathodeArc => IsStatusSet(11);
    public bool FanFault => IsStatusSet(12);
    public bool PowerFactorCorrectionOk => IsIoSet(7);
    public bool HighVoltageInterlock => IsIoSet(8);
    public bool HighVoltageStatus => IsIoSet(9);
    public bool Overcurrent24VoltFault => IsStatusSet(13);
    public bool MasterFault => IsStatusSet(14);
    public bool HighVoltageOvercurrentFault => IsStatusSet(15);
    public bool Temperature1Fault => IsStatusSet(16);
    public bool CathodeOvercurrentFault => IsStatusSet(17);
    public bool Temperature3Fault => IsStatusSet(18);
    public bool Temperature2Fault => IsStatusSet(19);

    public uint UnknownStatusFlags => RawStatusFlags & 0xFFFFFC00u;
    public uint UnknownIoFlags => RawIoFlags & 0xFFFE0000u;
    public bool HasActiveFaultInput => (RawIoFlags & 0x0001FC62u) != 0;

    private bool IsStatusSet(int bit) => (RawStatusFlags & (1u << bit)) != 0;
    private bool IsIoSet(int bit) => (RawIoFlags & (1u << bit)) != 0;
}
