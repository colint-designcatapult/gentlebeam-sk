using System.Threading.Tasks;
using Heracles.Application.Commands.gRPC.Common.Invokers;
using Heracles.Application.Protos;
using Xcc.Core.Domain.DataManagement.System;
using Xcc.Core.Infra.DataManagement.Common.DataAccess;

namespace Heracles.Application.Commands.gRPC.Common;

public class GrpcSystemCommands : ISystemCommands
{
    public GrpcSystemCommands(Invokers.GrpcSystemMethodsInvoker invoker)
    {
        _invoker = invoker;
    }

    public async Task<MosesSystemInfo> GetSystemInfoAsync()
    {
        var info = await _invoker.GetSystemInfoAsync();
        return ProtoTypesConverter.FromProto(info);
    }

    private readonly GrpcSystemMethodsInvoker _invoker;
}