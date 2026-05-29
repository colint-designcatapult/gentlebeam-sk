using Com.Empyreanmed.Heracles.TreatmentFields.V1;
using Google.Protobuf.Collections;
using Heracles.Application.Commands.gRPC.EMR.Invokers;
using Heracles.Core.Commands;
using Heracles.Core.Models.EMR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xcc.Core.Exceptions;
using Xcc.Infra.Persistence.DataAccess.gRPC;
using ProtoTypesConverter = Heracles.Application.Protos.ProtoTypesConverter;

namespace Heracles.Application.Commands.gRPC.EMR
{
    public class GrpcTreatmentFieldCommands 
        : ChildEntryCommandWrapper<ITreatmentField, TreatmentField, GrpcTreatmentFieldMethodsInvoker>
        , IEmrTreatmentFieldCommands
    {
        public GrpcTreatmentFieldCommands(GrpcTreatmentFieldMethodsInvoker invoker)
            : base(invoker, ProtoTypesConverter.ToProto, ProtoTypesConverter.FromProto)
        {
        }

        public async Task<ICollection<ITreatmentField>> CreateBunchAsync(ICollection<ITreatmentField> fields)
        {
            try
            {
                RepeatedField<TreatmentField> collection = new RepeatedField<TreatmentField>();

                foreach (var field in fields)
                {
                    collection.Add(ConvertToProto(field));
                }

                var createdBunch = await Invoker.CreateBunchAsync(collection);

                ICollection<ITreatmentField> result = new List<ITreatmentField>(createdBunch.Count);
                foreach (var item in createdBunch)
                {
                    result.Add(ConvertFromProto(item));
                }
                return result;
            }
            catch (Exception e)
            {
                string msg = $"Failed to save a new {typeof(TreatmentField).Name}";
                throw new DataServiceException(msg, e);
            }
        }
    }
}
