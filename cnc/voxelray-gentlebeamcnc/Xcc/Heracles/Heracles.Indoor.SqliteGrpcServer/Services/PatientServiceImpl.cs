using Com.Empyreanmed.Heracles.Patients.V1;
using Grpc.Core;
using Heracles.Indoor.SqliteGrpcServer.Infrastructure;

namespace Heracles.Indoor.SqliteGrpcServer.Services;

public sealed class PatientServiceImpl : PatientService.PatientServiceBase
{
    private readonly SqliteProtoRepository<Patient> _repo;
    public PatientServiceImpl(SqliteProtoRepository<Patient> repo) => _repo = repo;

    public override async Task<ListPatientsResponse> ListPatients(ListPatientsRequest request, ServerCallContext context)
    {
        var items = await _repo.ReadAllAsync();
        var r = new ListPatientsResponse();
        r.Patients.AddRange(items);
        return r;
    }

    public override async Task<SearchPatientsResponse> SearchPatients(SearchPatientsRequest request, ServerCallContext context)
    {
        var all = await _repo.ReadAllAsync();
        var filtered = all.Where(p =>
            (!request.HasFirstName || p.FirstName.Contains(request.FirstName, StringComparison.OrdinalIgnoreCase)) &&
            (!request.HasLastName  || p.LastName .Contains(request.LastName,  StringComparison.OrdinalIgnoreCase)));

        var r = new SearchPatientsResponse();
        r.Patients.AddRange(filtered);
        return r;
    }

    public override async Task<GetPatientResponse> GetPatient(GetPatientRequest request, ServerCallContext context)
    {
        var item = await _repo.ReadAsync(request.Id)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Patient {request.Id} not found"));
        return new GetPatientResponse { Patient = item };
    }

    public override async Task<CreatePatientResponse> CreatePatient(CreatePatientRequest request, ServerCallContext context)
    {
        var created = await _repo.CreateIfNoMatchAsync(request.Patient, existing =>
            SameIdentity(existing, request.Patient));

        if (created is null)
        {
            throw new RpcException(new Status(
                StatusCode.AlreadyExists,
                "A patient with the same identifying information already exists"));
        }

        return new CreatePatientResponse { Patient = created };
    }

    public override async Task<UpdatePatientResponse> UpdatePatient(UpdatePatientRequest request, ServerCallContext context)
    {
        var updated = await _repo.UpdateIfNoMatchAsync(
            request.Patient.Id,
            request.Patient,
            existing => SameIdentity(existing, request.Patient));

        if (updated is null)
        {
            throw new RpcException(new Status(
                StatusCode.AlreadyExists,
                "A patient with the same identifying information already exists"));
        }

        return new UpdatePatientResponse { Patient = updated };
    }

    public override async Task<DeletePatientResponse> DeletePatient(DeletePatientRequest request, ServerCallContext context)
    {
        await _repo.DeleteAsync(request.Id);
        return new DeletePatientResponse();
    }

    private static bool SameIdentity(Patient existing, Patient candidate)
    {
        return SameRequiredText(existing.FirstName, candidate.FirstName) &&
            SameRequiredText(existing.LastName, candidate.LastName) &&
            existing.HasSex == candidate.HasSex &&
            (!existing.HasSex || existing.Sex == candidate.Sex) &&
            SameDateOfBirth(existing, candidate) &&
            SameRequiredText(existing.Mrn, candidate.Mrn);
    }

    private static bool SameRequiredText(string existing, string candidate) =>
        string.Equals(existing, candidate, StringComparison.Ordinal);

    private static bool SameDateOfBirth(Patient existing, Patient candidate)
    {
        var existingDob = existing.Dob;
        var candidateDob = candidate.Dob;
        if ((existingDob is null) != (candidateDob is null))
            return false;

        return existingDob is null ||
            existingDob.ToDateTime().Date == candidateDob!.ToDateTime().Date;
    }
}
