using System;
using Xcc.Core.Domain.DataManagement.Common;

namespace Heracles.Application.Domain.DataManagement.System.Collimators
{
    public interface IPresetConfiguration : IEntry
    {
        DateTime CreationDate { get; set; }
        string PresetName {  get; set; }
        long CollimatorConfigurationId {  get; set; }
        bool IsDefault { get; set; }
        bool IsActive {  get; set; }
        string ApprovedBy { get; set; }
        bool IsApproved { get; }
    }
}
