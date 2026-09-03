using Empyrean.Common.Infra.Networking.Udp;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Core.Helpers;
using Xcc.Infra.GryphonBoard;
using static System.Formats.Asn1.AsnWriter;

namespace Xcc.Infra.GryphonBoard.CommandAPI
{
    public class GcbXRayCommandOperator : IGcbXRayCommandOperator
    {
        protected uint packetCounter = 0;

        /// <summary>
        /// This command is used to obtain the version information of the firmware. 
        /// </summary>
        /// <returns></returns>
        public byte[] GenerateVersionInfoRequestCmd()
        {
            return UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)GCBPacketType.VersionInfo,
                packetCounter: ++packetCounter,
                payload: [0 /* reserved field */]);
        }

        /// <summary>
        /// This command is used to obtain information about the fault which trigger the system to enter into a fault state. 
        /// </summary>
        /// <returns></returns>
        public byte[] GenerateFaultInfoRequestCmd(uint index)
        {
            return UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)GCBPacketType.FaultInfo,
                packetCounter: ++packetCounter,
                payload: [index]);
        }

        /// <summary>
        /// This command is used to request that the board perform a conditioning cycle, to a desired setpoint.
        /// </summary>
        /// <param name="filamentSetpoint">[mA] Target setpoint for the conditioning process</param>
        /// <returns></returns>
        public byte[] GenerateConditioningCmd(float filamentSetpoint)
        {
            return UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)GCBPacketType.ConditioningCmd,
                packetCounter: ++packetCounter,
                payload: [
                    filamentSetpoint, 
                    0 /* reserved field */
                    ]);
        }

        /// <summary>
        /// This command is used to request that the board perform a warmup, to a desired setpoint.
        /// </summary>
        /// <param name="filamentSetpoint">[mA] Target setpoint for the warmup process</param>
        /// <returns></returns>
        public byte[] GenerateWarmupCmd(float filamentSetpoint)
        {
            return UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)GCBPacketType.WarmupCmd,
                packetCounter: ++packetCounter,
                payload: [filamentSetpoint]);
        }

        public byte[] GenerateQcbPingCmd()
        {
            return UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)GCBPacketType.QcbPing,
                packetCounter: ++packetCounter,
                payload: [0, 0]);
        }

        public byte[] GenerateQcbReadingsCmd(uint command)
        {
            return UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)GCBPacketType.QcbReadingsCommand,
                packetCounter: ++packetCounter,
                payload: [command, 0u]);
        }

        /// <summary>
        /// This command is used in calibration mode to set HVPS kilovoltage.
        /// Payload: [cmd_id=5 (SET_KV), kv_value, flags]
        /// </summary>
        /// <param name="kvSetpoint">[kV] Kilovoltage setpoint (0-100)</param>
        /// <returns></returns>
        public byte[] GenerateCalibrationHvpsKvCmd(float kvSetpoint)
        {
            return UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)GCBPacketType.CalibrationHvpsCmd,
                packetCounter: ++packetCounter,
                payload: new UdpPacket.Field[] { new UdpPacket.Field(5), new UdpPacket.Field(kvSetpoint), new UdpPacket.Field(0) });
        }

        /// <summary>
        /// This command is used in calibration mode to set HVPS power.
        /// Payload: [cmd_id=4 (SET_PWR), power_value, flags]
        /// </summary>
        /// <param name="powerSetpoint">[W] Power setpoint</param>
        /// <returns></returns>
        public byte[] GenerateCalibrationHvpsPowerCmd(float powerSetpoint)
        {
            return UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)GCBPacketType.CalibrationHvpsCmd,
                packetCounter: ++packetCounter,
                payload: new UdpPacket.Field[] { new UdpPacket.Field(4), new UdpPacket.Field(powerSetpoint), new UdpPacket.Field(0) });
        }

        /// <summary>
        /// This command is used in calibration mode to set HVPS mA limit.
        /// Payload: [cmd_id=6 (SET_MA_LIM), ma_limit_value, flags]
        /// </summary>
        /// <param name="maSetpoint">[mA] mA current limit setpoint</param>
        /// <returns></returns>
        public byte[] GenerateCalibrationHvpsMaLimitCmd(float maSetpoint)
        {
            return UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)GCBPacketType.CalibrationHvpsCmd,
                packetCounter: ++packetCounter,
                payload: new UdpPacket.Field[] { new UdpPacket.Field(6), new UdpPacket.Field(maSetpoint), new UdpPacket.Field(0) });
        }

        /// <summary>
        /// This command is used in calibration mode to set HVPS grid voltage.
        /// Payload: [cmd_id=7 (SET_GRID), grid_value, flags]
        /// </summary>
        /// <param name="gridVoltage">[V] Grid voltage setpoint (0-600)</param>
        /// <returns></returns>
        public byte[] GenerateCalibrationHvpsGridCmd(float gridVoltage)
        {
            return UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)GCBPacketType.CalibrationHvpsCmd,
                packetCounter: ++packetCounter,
                payload: new UdpPacket.Field[] { new UdpPacket.Field(7), new UdpPacket.Field(gridVoltage), new UdpPacket.Field(0) });
        }

        /// <summary>
        /// This command is used in calibration mode to set HVPS filament current.
        /// Payload: [cmd_id=8 (SET_FIL), filament_value, flags]
        /// </summary>
        /// <param name="filamentCurrent">[mA] Filament heater current setpoint (0-4000)</param>
        /// <returns></returns>
        public byte[] GenerateCalibrationHvpsFilamentCmd(float filamentCurrent)
        {
            return UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)GCBPacketType.CalibrationHvpsCmd,
                packetCounter: ++packetCounter,
                payload: new UdpPacket.Field[] { new UdpPacket.Field(8), new UdpPacket.Field(filamentCurrent), new UdpPacket.Field(0) });
        }

        /// <summary>
        /// This command is used in calibration mode to enable/disable PID controller.
        /// Payload: [cmd_id=10 (SET_CONFIG), enable_value, flags=0x18]
        /// </summary>
        /// <param name="enable">true to enable PID, false to disable</param>
        /// <returns></returns>
        public byte[] GenerateCalibrationHvpsPidCmd(bool enable)
        {
            return UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)GCBPacketType.CalibrationHvpsCmd,
                packetCounter: ++packetCounter,
                payload: new UdpPacket.Field[] { new UdpPacket.Field(10), new UdpPacket.Field(enable ? 1.0f : 0.0f), new UdpPacket.Field(0x18) });
        }
        /// <summary>
        /// This command is used in calibration mode to set coil currents (X, Y, Focus).
        /// Payload: [x_coil_milliamps, y_coil_milliamps, focus_coil_milliamps]
        /// Valid ranges: X/Y Coil [-2000, 2000] mA, Focus [0, 3000] mA
        /// </summary>
        /// <param name="xCoilMilliamps">X Deflection Coil current [mA]</param>
        /// <param name="yCoilMilliamps">Y Deflection Coil current [mA]</param>
        /// <param name="fCoilMilliamps">Focus Coil current [mA]</param>
        /// <returns></returns>
        public byte[] GenerateCalibrationCoilsCmd(float xCoilMilliamps, float yCoilMilliamps, float fCoilMilliamps)
        {
            return UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)GCBPacketType.CalibrationCoilsCmd,
                packetCounter: ++packetCounter,
                payload: [xCoilMilliamps, yCoilMilliamps, fCoilMilliamps]);
        }

        /// <summary>
        /// This command is used in calibration mode to request the current HVPS setpoints.
        /// Response contains 5 float values: Power, KV, MA Limit, Grid, Filament
        /// </summary>
        /// <returns></returns>
        public byte[] GenerateCalibrationSetpointRequest()
        {
            return UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)GCBPacketType.CalibrationSetpointCmd,
                packetCounter: ++packetCounter,
                payload: [0 /* reserved field */]);
        }

        /// <summary>
        /// This command is used in calibration mode to control HVPS emission (beam on/off).
        /// Payload: 0x03 = HVPS_CMD_CAL_START (turn on), 0x04 = HVPS_CMD_CAL_STOP (turn off)
        /// </summary>
        /// <param name="command">0x03 for START, 0x04 for STOP</param>
        /// <returns></returns>
        public byte[] GenerateCalibrationEmissionCmd(uint command)
        {
            return UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)GCBPacketType.CalibrationEmissionCmd,
                packetCounter: ++packetCounter,
                payload: [command]);
        }

        /// <summary>
        /// This command is used to begin staging a new single-emission treatment plan. If successful, the firmware responds with a new session ID.
        /// </summary>
        public byte[] GenerateNewSessionCmd()
        {
            return UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)GCBPacketType.NewSessionCmd,
                packetCounter: ++packetCounter,
                payload: []);
        }


        /// <summary>
        /// This command requests the emission currently staged on the board.
        /// </summary>
        public byte[] GenerateOperationalPointQueryCmd()
        {
            return UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)GCBPacketType.OperationalPointQueryCmd,
                packetCounter: ++packetCounter,
                payload: []);
        }

        public byte[] GenerateReleaseTreatmentPlanCmd(GCBReleaseCommandScope scope, IGcbSessionAuthentication sessionKey)
        {
            var packet = UdpPacketBuilder.BuildPacket(
                packetType: (uint)GCBPacketType.ReleaseTreatmentPlan,
                packetCounter: ++packetCounter,
                payload: [
                    (int)scope, // Plan or Point
                    0 // authentication code, calc it below from the packet payload:
                    ]);
            return sessionKey.Sign(packet).Buffer;
        }

        /// <summary>
        /// This command is used to direct the boards to perform or begin a specific operation	
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public byte[] GenerateDirectiveCmd(GCBDirectiveCommandNew command)
        {
            return UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)GCBPacketType.DirectiveCmd,
                packetCounter: ++packetCounter,
                payload: [
                    (int)command,        // Directive Id
                    0x01 << (int)command // Confirmation bit ('1' shifted left by 'the value of the command' times)
                    ]);
        }

        
        /// <summary>
        /// Loading or confirmation command for the session's emission.
        /// </summary>
        public byte[] GenerateOperationalPointCmd(GCBPacketType packetType, GcbOperationalPoint op, IGcbSessionAuthentication sessionKey)
        {
            UdpPacket packet = UdpPacketBuilder.BuildPacket(
                packetType: (uint)packetType,
                packetCounter: ++packetCounter,
                payload: [
                    op.TotalPointTime,
                    op.RemainingPointTime,
                    op.SetpointKv,
                    op.TargetMA,
                    op.FilamentSetpoint,
                    op.XCoilSetpoint,
                    op.YCoilSetpoint,
                    op.FocusCoilSetpoint,
                    0 // authentication code, calc it below from the packet payload:
                    ]);
            return sessionKey.Sign(packet).Buffer;
        }

        /// <summary>
        /// This command is used to request the current status of the system.
        /// </summary>
        /// <returns></returns>
        public byte[] GenerateTelemetryRequestCmd()
        {
            return UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)GCBPacketType.TelemetryRequest,
                packetCounter: ++packetCounter,
                payload: [0 /*reserved field*/]);
        }

        public byte[] GenerateWaitForButtonCmd(IGcbSessionAuthentication sessionKey)
        {
            var packet = UdpPacketBuilder.BuildPacket(
                packetType: (uint)GCBPacketType.WaitForButtonCmd,
                packetCounter: ++packetCounter,
                payload: [
                    0, // Reserved
                    0  // authentication code, calc it below from the packet payload:
                ]);
            return sessionKey.Sign(packet).Buffer;
        }

    }
}
