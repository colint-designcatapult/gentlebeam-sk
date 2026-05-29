using Com.Empyreanmed.Heracles.Qcsamples.V1;
using Heracles.Application.Domain.DataManagement.System.QualityCheck;
using Heracles.Application.Infra.DataManagement.System.DataAccess.gRPC.Invokers;
using System;
using System.Threading.Tasks;
using Xcc.Core.Exceptions;
using Xcc.Infra.Persistence.DataAccess.gRPC;
using ProtoTypesConverter = Heracles.Application.Protos.ProtoTypesConverter;

namespace Heracles.Application.Infra.DataManagement.System.DataAccess.gRPC
{
    public class GrpcQcSampleCommands 
        : ChildEntryCommandWrapper<IQcSampleHeader, QCSample, GrpcQcSampleMethodsInvoker>
        , IQcSampleCommands
    {
        public GrpcQcSampleCommands(GrpcQcSampleMethodsInvoker invoker)
            : base(invoker, ProtoTypesConverter.ToProto, ProtoTypesConverter.FromProto)
        {
        }
        public async Task<IQcSampleHeader> ApproveAsync(long entryId, string username, string password)
        {
            try
            {
                return ConvertFromProto(await Invoker.ApproveAsync(entryId, username, password));
            }
            catch (Exception e)
            {
                string msg = $"Failed to approve QcSample entry id={entryId} by user {username}";
                throw new DataServiceException(msg, e);
            }
        }
    }
}
