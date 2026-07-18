using System;
using Xcc.Core.Enums;

namespace Xcc.Core.Domain.GryphonBoard
{
    public class FaultEntry
    {
        public int FaultId { get; set; }
        public SystemFault FaultType { get; set; }
        public GCBFaultDetails FaultIdSupportingDetails { get; set; }
        public int FaultEntryState { get; set; }
        public string FaultEntryStateString => ((GcbStateNew)FaultEntryState).ToString();
        public int FaultTimeValue { get; set; }
        public float ExpectedParameter { get; set; }
        public int ExpectedParameterSupportingDetails { get; set; }
        public float ParameterTolerance { get; set; }
        public float MeasuredParameter { get; set; }
        public int MeasuredParameterSupportingDetails { get; set; }

        public override string ToString()
        {
            return
                $"FaultId: {FaultId} Hex: {Convert.ToString(FaultId, 16)} Binary: {Convert.ToString(FaultId, 2)}{Environment.NewLine}" +
                $"FaultIdSupportingDetails: {(int)FaultIdSupportingDetails} ({FaultIdSupportingDetails}) " +
                $"Hex: {Convert.ToString((int)FaultIdSupportingDetails, 16)} Binary: {Convert.ToString((int)FaultIdSupportingDetails, 2)}{Environment.NewLine}" +
                $"FaultEntryState: {FaultEntryState} Hex: {Convert.ToString(FaultEntryState, 16)} Binary: {Convert.ToString(FaultEntryState, 2)}{Environment.NewLine}" +
                $"FaultTimeValue: {FaultTimeValue} Hex: {Convert.ToString(FaultTimeValue, 16)} Binary: {Convert.ToString(FaultTimeValue, 2)}{Environment.NewLine}" +
                $"ExpectedParameter: {ExpectedParameter}{Environment.NewLine}" +
                $"ExpectedParameterSupportingDetails: {ExpectedParameterSupportingDetails} Hex: {Convert.ToString(ExpectedParameterSupportingDetails, 16)} Binary: {Convert.ToString(ExpectedParameterSupportingDetails, 2)}{Environment.NewLine}" +
                $"ParameterTolerance: {ParameterTolerance}{Environment.NewLine}" +
                $"MeasuredParameter: {MeasuredParameter}{Environment.NewLine}" +
                $"MeasuredParameterSupportingDetails: {MeasuredParameterSupportingDetails} Hex: {Convert.ToString(MeasuredParameterSupportingDetails, 16)} Binary: {Convert.ToString(MeasuredParameterSupportingDetails, 2)}{Environment.NewLine}";
        }        
    }
}
