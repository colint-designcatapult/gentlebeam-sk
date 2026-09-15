using System;
using Heracles.Application.Models.Treatment;
using Xcc.Application.AppLayer.Model;
using Xcc.Application.AppLayer.Service;

namespace Heracles.Indoor.Models.UseCases;

// Only explicit view commands create requests. The scope covers synchronous store events,
// never an await; asynchronous readers carry their captured request to completion.
public sealed class PatientRecordReadAudit(
    IActionAuditService audit,
    IAuthorizedUserStore users,
    ITreatmentInfoStore treatmentInfo)
{
    public sealed record Request(long PatientId, long? DiagnosisId, long UserId, bool PlanningVisible);

    public Request? CurrentRequest { get; private set; }
    public bool PlanningVisible { get; set; } = true;

    public Request? CreateRequest(long? diagnosisId = null)
    {
        var patient = treatmentInfo.Patient;
        var user = users.AuthorizedUser;
        return patient is { Id: > 0 } && user is { Id: > 0 }
            ? new Request(patient.Id, diagnosisId, user.Id, PlanningVisible)
            : null;
    }

    public void InRequest(Request? request, Action action)
    {
        var previous = CurrentRequest;
        CurrentRequest = request;
        try { action(); }
        finally { CurrentRequest = previous; }
    }

    public void Record(Request? request, string surface, long recordId, bool planningOnly = false)
    {
        if (request is null || recordId <= 0 || users.AuthorizedUser?.Id != request.UserId ||
            treatmentInfo.Patient?.Id != request.PatientId ||
            (request.DiagnosisId.HasValue && treatmentInfo.Diagnosis?.Id != request.DiagnosisId) ||
            (planningOnly && (!request.PlanningVisible || !PlanningVisible)))
            return;

        audit.RegisterAction($"Viewed patient {surface} patient id={request.PatientId} record id={recordId}");
    }
}
