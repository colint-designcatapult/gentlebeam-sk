using Com.Empyreanmed.Heracles.Settings.V1;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Heracles.Application.Infra.DataManagement.System.DataAccess;
using Heracles.Core.Models;
using System;
using System.Threading.Tasks;
using Xcc.Core.Exceptions;
using Xcc.Infra.Networking.gRPC.Channels;
using ProtoTypesConverter = Heracles.Application.Protos.ProtoTypesConverter;

namespace Heracles.Application.Infra.DataManagement.System.DataAccess.gRPC
{
    public class GrpcSettingsCommands : ISettingsCommands
    {
        public GrpcSettingsCommands(IGrpcChannelManager grpcSettings)
        {
            GrpcSettings = grpcSettings;
        }

        public IGrpcChannelManager GrpcSettings { get; }

        public async Task<ISystemSettings> GetSettingsAsync()
        {
            try
            {
                var client = new SettingsService.SettingsServiceClient(GrpcSettings.Channel);

                var callOptions = new CallOptions().WithDeadline(GrpcSettings.GetRpcDeadline()).WithHeaders(GrpcSettings.Headers);
                var response = await client.GetSettingsAsync(new GetSettingsRequest(), callOptions);

                ISystemSettings output = ProtoTypesConverter.FromProto(response.Settings);
                return output;
            }
            catch (Exception ex)
            {
                throw new DataServiceException("Settings error: cannot retrieve settings from the database", ex);
            }
        }

        public async Task<ISystemSettings> UpdateSettingsAsync(ISystemSettings oldValue, ISystemSettings newValue)
        {
            if (oldValue == null || newValue == null)
            {
                throw new ArgumentNullException("Update settings error: invalid argument");
            }

            try
            {
                var client = new SettingsService.SettingsServiceClient(GrpcSettings.Channel);

                var oldProtosValue = ProtoTypesConverter.ToProto(oldValue);
                var newProtosValue = ProtoTypesConverter.ToProto(newValue);

                var mask = new FieldMask();
                mask.Paths.Add(Xcc.Application.Common.GenericExtensions.CompareProperties(oldProtosValue, newProtosValue));
                // Workaround to have moses accepting the request:
                mask.Paths.Add("id");

                var callOptions = new CallOptions().WithDeadline(GrpcSettings.GetRpcDeadline()).WithHeaders(GrpcSettings.Headers);
                var response = await client.UpdateSettingsAsync(new UpdateSettingsRequest() { Settings = newProtosValue, UpdateMask = mask }, callOptions);

                ISystemSettings output = ProtoTypesConverter.FromProto(response.Settings);
                return output;
            }
            catch (Exception ex)
            {
                throw new DataServiceException("Settings error: cannot submit to the database", ex);
            }
        }
    }
}
