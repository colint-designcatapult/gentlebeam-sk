using System;
using System.Collections.Generic;
using System.Linq;
using Empyrean.Common.Infra.Networking.Udp;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;

namespace Xcc.Infra.GryphonBoard.CommandAPI
{
    public static class GcbXRayCmdResponseGenerator
    {
        public static byte[] GenerateInvalidPacketResponse(uint packetCounter)
        {
            return UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)GCBPacketType.InvalidPacket,
                packetCounter: packetCounter,
                payload: [ 0 /*reserved*/]);
        }

        public static byte[] GenerateVersionInfoResponse(uint packetCounter, VersionInfo info)
        {
            return UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)GCBPacketType.VersionInfoResponse,
                packetCounter: packetCounter,
                payload: [
                    info.Major, info.Minor, info.Level, info.FirmwareChecksum, (uint)info.Mode
                    ]);
        }

        public static byte[] GenerateFaultInfoResponse(uint packetCounter, FaultEntry faultEntry)
        {
            return UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)GCBPacketType.FaultInfoResponse,
                packetCounter: packetCounter,
                payload: [
                    faultEntry.FaultId,
                    (int)faultEntry.FaultIdSupportingDetails,
                    faultEntry.FaultEntryState,
                    faultEntry.FaultTimeValue,
                    faultEntry.ExpectedParameter,
                    faultEntry.ExpectedParameterSupportingDetails,
                    faultEntry.ParameterTolerance,
                    faultEntry.MeasuredParameter,
                    faultEntry.MeasuredParameterSupportingDetails
                ]);
        }

        public static byte[] GenerateConditioningResponse(uint packetCounter, GcbProcessingStatus status)
        {
            return UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)GCBPacketType.ConditioningResponse,
                packetCounter: packetCounter,
                payload: [
                    (int)status,
                    0 /* reserved field */
                    ]);
        }

        public static byte[] GenerateWarmUpResponse(uint packetCounter, GcbProcessingStatus status)
        {
            return UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)GCBPacketType.WarmupResponse,
                packetCounter: packetCounter,
                payload: [(int)status]
                );
        }

        public static byte[] GenerateNewSessionResponse(uint packetCounter, GcbProcessingStatus status, int sessionId)
        {
            return UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)GCBPacketType.NewSessionResponse,
                packetCounter: packetCounter,
                payload: [(uint)status, sessionId]);
        }


        public static byte[] GenerateOperationalPointQueryResponse(uint packetCounter, GcbProcessingStatus status, GcbOperationalPoint point)
        {
            return UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)GCBPacketType.OperationalPointQueryResponse,
                packetCounter: packetCounter,
                payload: [
                    (int)status,
                    point.PointIndex,
                    point.TotalPointTime,
                    point.RemainingPointTime,
                    point.SetpointKv,
                    point.TargetMA,
                    point.FilamentSetpoint,
                    point.XCoilSetpoint,
                    point.YCoilSetpoint,
                    point.FocusCoilSetpoint,
                    point.AutoExecution ? 1 : 0,
                    ]);
        }

        public static byte[] GenerateReleasePlanResponse(uint packetCounter, GcbProcessingStatus scopeStatus, GcbProcessingStatus authenticationStatus)
        {
            return UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)GCBPacketType.ReleaseTreatmentPlanResponse,
                packetCounter: packetCounter,
                payload: [
                    (int)scopeStatus,
                    (int)authenticationStatus
                    ]);
        }

        public static byte[] GenerateDirectiveResponse(uint packetCounter, GcbProcessingStatus status)
        {
            return UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)GCBPacketType.DirectiveCmdResponse,
                packetCounter: packetCounter,
                payload: [ (int)status /*cmd processing status*/ ]);
        }


        public static byte[] GenerateOperationalPointResponse(uint packetCounter, OperationalPointCmdType commandType, ICollection<int> fieldStatuses)
        {
            if (fieldStatuses is null || fieldStatuses.Count == 0)
            {
                throw new ArgumentException("Field statuses collection is null or empty");
            }

            uint packetType = commandType == OperationalPointCmdType.Load
                ? (uint)GCBPacketType.OperationalPointLoadingResponse
                : (uint)GCBPacketType.OperationalPointConfirmationResponse;


            return UdpPacketBuilder.BuildRawPacket(
                packetType: packetType,
                packetCounter: ++packetCounter,
                payload: fieldStatuses.Select(x => new UdpPacket.Field(x)).ToList());
        }

        //public byte[] GenerateTelemetryRequestCmd()
        //{
        //    return UdpPacketBuilder.BuildRawPacket(
        //        packetType: (uint)GCBPacketType.TelemetryRequest,
        //        packetCounter: ++packetCounter,
        //        payload: [0 /*reserved field*/]);
        //}

        //public byte[] GenerateQCDataQueryCmd(uint operationalPoitnIndex)
        //{
        //    return UdpPacketBuilder.BuildRawPacket(
        //        packetType: (uint)GCBPacketType.QCDataQueryCmd,
        //        packetCounter: ++packetCounter,
        //        payload: [
        //            operationalPoitnIndex,
        //            0 /*reserved field*/
        //            ]);

        //}
    }
}
