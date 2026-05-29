using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Infra.DataManagement.System.DataAccess.gRPC.Invokers;
using System;
using System.Threading.Tasks;
using Xcc.Application.Commands;
using Xcc.Core.Exceptions;
using Xcc.Infra.Persistence.DataAccess.gRPC;
using PresetConfiguration = Com.Empyreanmed.Heracles.PresetConfigurations.V1.PresetConfiguration;
using ProtoTypesConverter = Heracles.Application.Protos.ProtoTypesConverter;

namespace Heracles.Application.Infra.DataManagement.System.DataAccess.gRPC
{
    public class GrpcPresetConfigurationCommands
        : ChildEntryCommandWrapper<IPresetConfiguration, PresetConfiguration, GrpcPresetConfigurationMethodsInvoker>
        , IPresetConfigurationCommands
    {
        public IDataServiceProvider DataServiceProvider { get; }

        public GrpcPresetConfigurationCommands(GrpcPresetConfigurationMethodsInvoker invoker)
            : base(invoker, ProtoTypesConverter.ToProto, ProtoTypesConverter.FromProto)
        {
        }

        public async Task<IPresetConfiguration> ApproveAsync(long entryId, string username, string password)
        {
            try
            {
                return ConvertFromProto(await Invoker.ApproveAsync(entryId, username, password));
            }
            catch (Exception e)
            {
                string msg = $"Failed to approve preset configuration entry id={entryId} by user {username}";
                throw new DataServiceException(msg, e);
            }
        }
    }
}
