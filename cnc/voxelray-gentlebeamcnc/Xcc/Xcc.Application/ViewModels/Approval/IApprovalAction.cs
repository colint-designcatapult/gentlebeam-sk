using System.Threading.Tasks;

namespace Xcc.Application.ViewModels.Approval;

public interface IApprovalAction
{
    Task ApproveAsync(string username, string password);
}