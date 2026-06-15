using Heracles.Core.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xcc.Core.Domain.DataManagement.Common.Users;
using Xcc.Core.Exceptions;
using Xcc.Infra.Persistence.DataAccess.gRPC;
using ProtoTypesConverter = Heracles.Application.Protos.ProtoTypesConverter;

namespace Heracles.Application.Commands.gRPC.Common;

public class GrpcUserRoleMappingCommands
    : ChildEntryCommandWrapper<
        UserRoleRecord, 
        Com.Empyreanmed.Heracles.UserRoles.V1.UserRole, 
        Invokers.GrpcUserRoleMethodsInvoker>
    , IUserRoleMappingCommandsExt
{
    public GrpcUserRoleMappingCommands(Invokers.GrpcUserRoleMethodsInvoker invoker)
        : base(invoker, ProtoTypesConverter.ToProto, ProtoTypesConverter.FromProto)
    {
    }
    public async Task<ICollection<UserRoleRecord>> ReadListAsync(string userEmail)
    {
        try
        {
            var list = await Invoker.ReadListAsync(userEmail);
            ICollection<UserRoleRecord> result = new List<UserRoleRecord>();
            foreach (var item in list)
            {
                result.Add(ConvertFromProto(item));
            }
            return result;
        }
        catch (Exception e)
        {
            string msg = $"Failed to get list of {typeof(UserRole).Name} entries by parent id={userEmail}";
            throw new DataServiceException(msg, e);
        }
    }
}