using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;

namespace Xcc.Infra.GryphonBoard
{
    public interface IGcbXRayCommandOperator
    {
        byte[] GenerateDirectiveCmd(GCBDirectiveCommandNew command);
        //byte[] GenerateOperationalPointLoadingCmd(GcbOperationalPoint op, int authenticationCode);

        /// <summary>
        /// Can be used for both Loading and Confirmation commands
        /// </summary>
        /// <param name="packetType"></param>
        /// <param name="op"></param>
        /// <param name="sessionKey"></param>
        /// <returns></returns>
        byte[] GenerateOperationalPointCmd(GCBPacketType packetType, GcbOperationalPoint op, IGcbSessionAuthentication sessionKey);
        byte[] GenerateConditioningCmd(float filamentSetpoint);
        byte[] GenerateWarmupCmd(float filamentSetpoint);
        byte[] GenerateQcbPingCmd();
        byte[] GenerateQcbReadingsCmd(uint command);
        byte[] GenerateCalibrationHvpsKvCmd(float kvSetpoint);
        byte[] GenerateCalibrationHvpsPowerCmd(float powerSetpoint);
        byte[] GenerateCalibrationHvpsMaLimitCmd(float maSetpoint);
        byte[] GenerateCalibrationHvpsGridCmd(float gridVoltage);
        byte[] GenerateCalibrationHvpsFilamentCmd(float filamentCurrent);
        byte[] GenerateCalibrationHvpsPidCmd(bool enable);
        byte[] GenerateCalibrationCoilsCmd(float xCoilMilliamps, float yCoilMilliamps, float fCoilMilliamps);
        byte[] GenerateCalibrationSetpointRequest();
        byte[] GenerateCalibrationEmissionCmd(uint command);
        byte[] GenerateNewSessionCmd();
        byte[] GenerateOperationalPointQueryCmd();
        byte[] GenerateReleaseTreatmentPlanCmd(GCBReleaseCommandScope scope, IGcbSessionAuthentication sessionKey);
        byte[] GenerateTelemetryRequestCmd();
        byte[] GenerateVersionInfoRequestCmd();
        byte[] GenerateFaultInfoRequestCmd(uint index);
        byte[] GenerateWaitForButtonCmd(IGcbSessionAuthentication sessionKey);
    }
}
