using Heracles.Application.Commands.gRPC.EMR.Invokers;
using System;
using System.Threading.Tasks;
using Xcc.Core.Exceptions;
using Xcc.Core.Domain.DataManagement.Common.Users;
using Xcc.Core.Infra.DataManagement.Common.DataAccess;
using Xcc.Infra.Persistence.DataAccess.gRPC;
using ProtoTypesConverter = Heracles.Application.Protos.ProtoTypesConverter;
using User = Com.Empyreanmed.Heracles.Users.V1.User;

namespace Heracles.Application.Commands.gRPC.EMR
{
    public class GrpcUserCommands 
        : RootEntryCommandWrapper<IUser, User, GrpcUserMethodsInvoker>
        , IUserCommands
    {
        public GrpcUserCommands(GrpcUserMethodsInvoker invoker)
            : base(invoker, ProtoTypesConverter.ToProto, ProtoTypesConverter.FromProto)
        {
        }

        public async Task ResetUserLockoutAsync(long userId)
        {
            try
            {
                await Invoker.ResetUserLockoutAsync(userId);
            }
            catch (Exception ex)
            {
                throw new DataServiceException("Failed to reset the user lockout.", ex);
            }
        }
    }
}
