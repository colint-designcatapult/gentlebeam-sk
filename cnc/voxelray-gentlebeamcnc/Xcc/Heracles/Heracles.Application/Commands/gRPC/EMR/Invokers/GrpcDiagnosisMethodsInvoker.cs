using Com.Empyreanmed.Heracles.Diagnoses.V1;
using Google.Protobuf.WellKnownTypes;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xcc.Infra.Networking.gRPC.Channels;
using Xcc.Infra.Persistence.DataAccess.gRPC.Invokers;

namespace Heracles.Application.Commands.gRPC.EMR.Invokers
{
    public class GrpcDiagnosisMethodsInvoker : AbstractChildGrpcInvoker<Diagnosis>
    {
        public GrpcDiagnosisMethodsInvoker(IGrpcChannelManager grpcSettings)
            : base(grpcSettings)
        {
            Client = new DiagnosisService.DiagnosisServiceClient(Channel);
        }

        public DiagnosisService.DiagnosisServiceClient Client { get; private set; }

        public override async Task<Diagnosis> CreateAsync(Diagnosis entry)
        {
            var request = new CreateDiagnosisRequest { Diagnosis = entry };
            request.Diagnosis.ClearId();

            var response = await CallWithOptions(Client.CreateDiagnosisAsync, request);
            return response.Diagnosis;
        }
        public override async Task<Diagnosis> ReadAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.GetDiagnosisAsync,
                new GetDiagnosisRequest { Id = entryId });
            return response.Diagnosis;
        }

        public override async Task<bool> DeleteAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.DeleteDiagnosisAsync,
                new DeleteDiagnosisRequest { Id = (ulong)entryId });
            return true;
        }

        public override async Task<Diagnosis> UpdateAsyncWithMask(Diagnosis entry, FieldMask mask)
        {
            var request = new UpdateDiagnosisRequest { Diagnosis = entry, UpdateMask = mask };
            var response = await CallWithOptions(Client.UpdateDiagnosisAsync, request);
            return response.Diagnosis;
        }

        public override async Task<ICollection<Diagnosis>> ReadListAsync(long parentId)
        {
            var response = await CallWithOptions(
                Client.ListDiagnosesAsync,
                new ListDiagnosesRequest { PatientId = parentId });

            return response.Diagnoses;
        }
    }
}
