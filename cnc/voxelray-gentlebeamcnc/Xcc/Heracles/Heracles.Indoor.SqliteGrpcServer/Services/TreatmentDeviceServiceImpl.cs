using Com.Empyreanmed.Heracles.TreatmentDevices.V1;
using Grpc.Core;
using Heracles.Indoor.SqliteGrpcServer.Infrastructure;

namespace Heracles.Indoor.SqliteGrpcServer.Services;

public sealed class TreatmentDeviceServiceImpl : TreatmentDevicesService.TreatmentDevicesServiceBase
{
    private readonly SqliteProtoRepository<TreatmentDevice> _repo;
    public TreatmentDeviceServiceImpl(SqliteProtoRepository<TreatmentDevice> repo) => _repo = repo;

    public override async Task<ListTreatmentDevicesResponse> ListTreatmentDevices(
        ListTreatmentDevicesRequest request, ServerCallContext context)
    {
        var items = await _repo.ReadByParentIdAsync(request.SimulationId);
        var r = new ListTreatmentDevicesResponse();
        r.TreatmentDevices.AddRange(items);
        return r;
    }

    public override async Task<GetTreatmentDeviceResponse> GetTreatmentDevice(
        GetTreatmentDeviceRequest request, ServerCallContext context)
    {
        var item = await _repo.ReadAsync(request.TreatmentDeviceId)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"TreatmentDevice {request.TreatmentDeviceId} not found"));
        return new GetTreatmentDeviceResponse { TreatmentDevice = item };
    }

    public override async Task<CreateTreatmentDeviceResponse> CreateTreatmentDevice(
        CreateTreatmentDeviceRequest request, ServerCallContext context)
    {
        var created = await _repo.CreateAsync(request.TreatmentDevice, request.TreatmentDevice.SimulationId);
        return new CreateTreatmentDeviceResponse { TreatmentDevice = created };
    }

    public override async Task<UpdateTreatmentDeviceResponse> UpdateTreatmentDevice(
        UpdateTreatmentDeviceRequest request, ServerCallContext context)
    {
        var updated = await _repo.UpdateAsync(request.TreatmentDevice.Id, request.TreatmentDevice);
        return new UpdateTreatmentDeviceResponse { TreatmentDevice = updated };
    }

    public override async Task<DeleteTreatmentDeviceResponse> DeleteTreatmentDevice(
        DeleteTreatmentDeviceRequest request, ServerCallContext context)
    {
        await _repo.DeleteAsync(request.TreatmentDeviceId);
        return new DeleteTreatmentDeviceResponse();
    }
}
