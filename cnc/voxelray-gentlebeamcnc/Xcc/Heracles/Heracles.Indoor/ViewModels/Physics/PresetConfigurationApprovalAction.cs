using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Infra.DataManagement.System.DataAccess;

using System;
using System.Threading.Tasks;
using Xcc.Application.ViewModels.Approval;
using Xcc.Application.AppLayer.Service;

namespace Heracles.Indoor.ViewModels.Physics
{
    public class PresetConfigurationApprovalAction(
    IPresetConfigurationCommands presetCommands,
    IPresetConfiguration preset,
    IActionAuditService actionAuditService) : IApprovalAction
    {
        public async Task ApproveAsync(string username, string password)
        {
            if (preset is null)
            {
                throw new NullReferenceException("No preset to update");
            }

            var previouslyApprovedBy = preset.ApprovedBy;
            var approvedPreset = await presetCommands.ApproveAsync(preset.Id, username, password);
            if (approvedPreset.IsApproved && approvedPreset.ApprovedBy != previouslyApprovedBy)
                actionAuditService.RegisterAction("Configuration approved",
                    $"Entity=PresetConfiguration; Id={approvedPreset.Id}; Fields=ApprovedBy");
            preset.ApprovedBy = approvedPreset.ApprovedBy;
        }
    }
}
