using Xcc.Core.Domain.DataManagement.Common.Users;
using Xcc.Core.Infra.DataManagement.Common.DataAccess;
using Xcc.Infra.Persistence.DataAccess.gRPC;
using ProtoTypesConverter = Heracles.Application.Protos.ProtoTypesConverter;

namespace Heracles.Application.Commands.gRPC.Common;

public class GrpcRoleCommands
    : RootEntryCommandWrapper<
        RoleRecord,
        Com.Empyreanmed.Heracles.Roles.V1.Role, 
        Invokers.GrpcRoleMethodsInvoker>
    , IRoleCommands
{
    public GrpcRoleCommands(Invokers.GrpcRoleMethodsInvoker invoker)
        : base(invoker, ProtoTypesConverter.ToProto, ProtoTypesConverter.FromProto)
    {
    }
}