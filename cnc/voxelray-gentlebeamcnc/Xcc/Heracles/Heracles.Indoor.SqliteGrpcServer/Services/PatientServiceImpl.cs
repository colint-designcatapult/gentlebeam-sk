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
        var created = await _repo.CreateAsync(request.Patient);
        return new CreatePatientResponse { Patient = created };
    }

    public override async Task<UpdatePatientResponse> UpdatePatient(UpdatePatientRequest request, ServerCallContext context)
    {
        var updated = await _repo.UpdateAsync(request.Patient.Id, request.Patient);
        return new UpdatePatientResponse { Patient = updated };
    }

    public override async Task<DeletePatientResponse> DeletePatient(DeletePatientRequest request, ServerCallContext context)
    {
        await _repo.DeleteAsync(request.Id);
        return new DeletePatientResponse();
    }
}
