using Xcc.Core.Domain.DataManagement.Common.Users;
using Xcc.Core.Infra.DataManagement.Common.DataAccess;
using Xcc.Infra.Persistence.DataAccess.gRPC;
using ProtoTypesConverter = Heracles.Application.Protos.ProtoTypesConverter;

namespace Heracles.Application.Commands.gRPC.Common;

public class GrpcPermissionCommands
    : ChildEntryCommandWrapper<
            PermissionRecord,
            Com.Empyreanmed.Heracles.RolesPermissions.V1.RolesPermissions,
            Invokers.GrpcPermissionMethodsInvoker>
        , IPermissionCommands
{
    public GrpcPermissionCommands(Invokers.GrpcPermissionMethodsInvoker invoker)
        : base(invoker, ProtoTypesConverter.ToProto, ProtoTypesConverter.FromProto)
    {
    }
}