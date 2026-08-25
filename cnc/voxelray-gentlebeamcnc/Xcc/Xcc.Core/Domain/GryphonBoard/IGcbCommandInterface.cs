using System.Threading.Tasks;
using Xcc.Core.Enums;

namespace Xcc.Core.Domain.GryphonBoard
{
    /// <summary>
    /// Response from calibration setpoint request command containing all 5 HVPS setpoints
    /// </summary>
    public record CalibrationSetpointResponse(
        float PowerSetpoint,
        float KvSetpoint,
        float MaLimitSetpoint,
        float GridSetpoint,
        float FilamentSetpoint);

    /// <summary>
    /// Response from calibration emission command indicating current emission state
    /// </summary>
    public record CalibrationEmissionResponse(
        bool EmissionOn);

    public class GcbNoConnectionException : System.Exception
    {
        public GcbNoConnectionException(string message) : base(message) { }
    }

    public interface IGcbCommandInterface
    {
        Task SendOperationalPoint(OperationalPointCmdType commandType, GcbOperationalPoint operationalPoint, GcbSession session);
        Task SendDirectiveCommand(GCBDirectiveCommandNew command);
        Task ReleasePlan(GCBReleaseCommandScope scope, GcbSession session);
        Task<GcbSession> NewSession(int totalPoints);
        Task Stop();
        Task Initialize();
        Task StagePlan();
        Task ClearFaults();
        Task ClearPlan();
        Task ResetTimers();
        Task<FaultSnapshot> GetFaults();
        Task Conditioning(float conditioningSetpoint);
        Task WarmUp(float warmupSetpoint);
        Task SendHvpsKv(float kvSetpoint, float powerSetpoint);
        Task SendHvpsMaLimit(float maSetpoint);
        Task SendHvpsGrid(float gridVoltage);
        Task SendHvpsFilament(float filamentCurrent);
        Task SendHvpsPidControl(bool enable);
        Task SendCoils(float xCoil, float yCoil, float fCoil);
        Task<CalibrationSetpointResponse> RequestCalibrationSetpoints();
        Task<CalibrationEmissionResponse> SendHvpsEmission(uint command);
        Task<byte[]> SendVersionInfoRequest();
        Task<GcbOperationalPoint> QueryPoint(int pointIndex);
        Task<VersionInfo> GetVersionInfo();
    }
}
