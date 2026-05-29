using Com.Empyreanmed.Heracles.Patients.V1;
using Google.Protobuf.WellKnownTypes;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xcc.Infra.Networking.gRPC.Channels;
using Xcc.Infra.Persistence.DataAccess.gRPC.Invokers;

namespace Heracles.Application.Commands.gRPC.EMR.Invokers
{
    public class GrpcPatientMethodsInvoker : AbstractRootGrpcInvoker<Patient>
    {
        public GrpcPatientMethodsInvoker(IGrpcChannelManager grpcSettings)
            : base(grpcSettings)
        {
            Client = new PatientService.PatientServiceClient(Channel);
        }

        public PatientService.PatientServiceClient Client { get; private set; }

        public override async Task<Patient> CreateAsync(Patient entry)
        {
            var request = new CreatePatientRequest() { Patient = entry };
            request.Patient.ClearId();

            var response = await CallWithOptions(Client.CreatePatientAsync, request);
            return response.Patient;
        }

        public override async Task<Patient> UpdateAsyncWithMask(Patient newEntry, FieldMask mask)
        {
            var request = new UpdatePatientRequest()
            {
                Patient = newEntry,
                UpdateMask = mask
            };
            var response = await CallWithOptions(Client.UpdatePatientAsync, request);
            return response.Patient;
        }

        public override async Task<bool> DeleteAsync(long entryId)
        {
            var response = await CallWithOptions(Client.DeletePatientAsync, new DeletePatientRequest { Id = entryId });
            return true;
        }

        public override async Task<ICollection<Patient>> ReadAllAsync()
        {
            var response = await CallWithOptions(Client.ListPatientsAsync, new ListPatientsRequest());
            return response.Patients;
        }

        public override async Task<Patient> ReadAsync(long entryId)
        {
            var response = await CallWithOptions(Client.GetPatientAsync, new GetPatientRequest { Id = entryId });
            return response.Patient;
        }
    }
}
