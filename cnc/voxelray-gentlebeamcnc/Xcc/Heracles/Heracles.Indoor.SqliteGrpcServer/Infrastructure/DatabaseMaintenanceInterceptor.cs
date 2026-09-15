using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Heracles.Indoor.SqliteGrpcServer.Infrastructure;

internal sealed class DatabaseMaintenanceInterceptor(DatabaseMaintenanceGate gate) : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
    {
        using var admission = gate.EnterUnary();
        return await continuation(request, context).ConfigureAwait(false);
    }

    public override async Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest> requestStream, ServerCallContext context,
        ClientStreamingServerMethod<TRequest, TResponse> continuation)
    {
        // Photo uploads are finite SQLite operations and must drain before maintenance.
        using var admission = gate.EnterUnary();
        return await continuation(requestStream, context).ConfigureAwait(false);
    }

    public override async Task ServerStreamingServerHandler<TRequest, TResponse>(
        TRequest request, IServerStreamWriter<TResponse> responseStream, ServerCallContext context,
        ServerStreamingServerMethod<TRequest, TResponse> continuation)
    {
        if (context.Method is
            "/com.empyreanmed.heracles.plans.v1.PlanService/PlanEvents" or
            "/com.empyreanmed.heracles.plans.v1.PlanService/LoadForTreatmentEvents")
        {
            // These subscriptions only consume in-memory events; they never access SQLite.
            gate.CheckStreamingAdmission();
            await continuation(request, responseStream, context).ConfigureAwait(false);
            return;
        }

        // Unlike plan subscriptions, photo downloads have a bounded transfer lifecycle.
        using var admission = gate.EnterUnary();
        await continuation(request, responseStream, context).ConfigureAwait(false);
    }
}
