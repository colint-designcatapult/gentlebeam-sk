using Heracles.Application.Domain.DataManagement.System.QualityCheck;
using Heracles.Application.Infra.DataManagement.System;
using System;
using System.Threading.Tasks;
using Xcc.Application.ViewModels.Approval;

namespace Heracles.Application.AppLayer.QualityAssurance.QualityCheck
{
    public class QcSampleApprovalAction(
        IQcRepository qcRepository,
        IQcSampleHeader qcSample) : IApprovalAction
    {
        public Task ApproveAsync(string username, string password)
        {
            if (qcSample is null)
            {
                throw new NullReferenceException("No QcSample to update");
            }

            return qcRepository.ApproveQcSampleAsync(qcSample, username, password);
        }
    }
}
