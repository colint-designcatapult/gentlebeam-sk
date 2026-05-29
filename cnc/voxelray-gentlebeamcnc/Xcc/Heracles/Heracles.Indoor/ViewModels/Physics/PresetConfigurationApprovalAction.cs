using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Infra.DataManagement.System.DataAccess;

using System;
using System.Threading.Tasks;
using Xcc.Application.ViewModels.Approval;

namespace Heracles.Indoor.ViewModels.Physics
{
    public class PresetConfigurationApprovalAction(
    IPresetConfigurationCommands presetCommands,
    IPresetConfiguration preset) : IApprovalAction
    {
        public async Task ApproveAsync(string username, string password)
        {
            if (preset is null)
            {
                throw new NullReferenceException("No preset to update");
            }

            var approvedPreset = await presetCommands.ApproveAsync(preset.Id, username, password);
            preset.ApprovedBy = approvedPreset.ApprovedBy;
        }
    }
}
