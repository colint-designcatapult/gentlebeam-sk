using Com.Empyreanmed.Heracles.Simulations.V1;
using Grpc.Core;
using Heracles.Indoor.SqliteGrpcServer.Infrastructure;

namespace Heracles.Indoor.SqliteGrpcServer.Services;

public sealed class SimulationServiceImpl : SimulationService.SimulationServiceBase
{
    private readonly SqliteProtoRepository<Simulation> _repo;
    public SimulationServiceImpl(SqliteProtoRepository<Simulation> repo) => _repo = repo;

    public override async Task<ListSimulationsResponse> ListSimulations(ListSimulationsRequest request, ServerCallContext context)
    {
        IList<Simulation> items = request.HasDiagnosisId
            ? await _repo.ReadByParentIdAsync(request.DiagnosisId)
            : await _repo.ReadAllAsync();

        var r = new ListSimulationsResponse();
        r.Simulations.AddRange(items);
        return r;
    }

    public override async Task<GetSimulationResponse> GetSimulation(GetSimulationRequest request, ServerCallContext context)
    {
        var item = await _repo.ReadAsync(request.SimulationId)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Simulation {request.SimulationId} not found"));
        return new GetSimulationResponse { Simulation = item };
    }

    public override async Task<CreateSimulationResponse> CreateSimulation(CreateSimulationRequest request, ServerCallContext context)
    {
        var created = await _repo.CreateAsync(request.Simulation, request.Simulation.DiagnosisId);
        return new CreateSimulationResponse { Simulation = created };
    }

    public override async Task<UpdateSimulationResponse> UpdateSimulation(UpdateSimulationRequest request, ServerCallContext context)
    {
        var updated = await _repo.UpdateAsync(request.Simulation.Id, request.Simulation);
        return new UpdateSimulationResponse { Simulation = updated };
    }

    public override async Task<DeleteSimulationResponse> DeleteSimulation(DeleteSimulationRequest request, ServerCallContext context)
    {
        await _repo.DeleteAsync(request.SimulationId);
        return new DeleteSimulationResponse();
    }
}
