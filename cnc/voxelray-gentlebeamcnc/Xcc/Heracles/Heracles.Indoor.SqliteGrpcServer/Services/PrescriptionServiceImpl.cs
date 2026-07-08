using Com.Empyreanmed.Heracles.Prescriptions.V1;
using Grpc.Core;
using Heracles.Indoor.SqliteGrpcServer.Infrastructure;

namespace Heracles.Indoor.SqliteGrpcServer.Services;

public sealed class PrescriptionServiceImpl : PrescriptionService.PrescriptionServiceBase
{
    private readonly SqliteProtoRepository<Prescription> _repo;
    public PrescriptionServiceImpl(SqliteProtoRepository<Prescription> repo) => _repo = repo;

    public override async Task<ListPrescriptionsResponse> ListPrescriptions(ListPrescriptionsRequest request, ServerCallContext context)
    {
        IList<Prescription> items = request.HasSimulationId
            ? await _repo.ReadByParentIdAsync(request.SimulationId)
            : await _repo.ReadAllAsync();

        var r = new ListPrescriptionsResponse();
        r.Prescriptions.AddRange(items);
        return r;
    }

    public override async Task<GetPrescriptionResponse> GetPrescription(GetPrescriptionRequest request, ServerCallContext context)
    {
        var item = await _repo.ReadAsync(request.Id)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Prescription {request.Id} not found"));
        return new GetPrescriptionResponse { Prescription = item };
    }

    public override async Task<CreatePrescriptionResponse> CreatePrescription(CreatePrescriptionRequest request, ServerCallContext context)
    {
        var created = await _repo.CreateAsync(request.Prescription, request.Prescription.SimulationId);
        return new CreatePrescriptionResponse { Prescription = created };
    }

    public override async Task<UpdatePrescriptionResponse> UpdatePrescription(UpdatePrescriptionRequest request, ServerCallContext context)
    {
        var updated = await _repo.UpdateAsync(request.Prescription.Id, request.Prescription);
        return new UpdatePrescriptionResponse { Prescription = updated };
    }

    public override async Task<DeletePrescriptionResponse> DeletePrescription(DeletePrescriptionRequest request, ServerCallContext context)
    {
        await _repo.DeleteAsync(request.PrescriptionId);
        return new DeletePrescriptionResponse();
    }
}
