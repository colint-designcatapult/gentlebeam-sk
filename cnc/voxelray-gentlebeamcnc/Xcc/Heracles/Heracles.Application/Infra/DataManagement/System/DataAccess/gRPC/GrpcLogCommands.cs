using System.Collections.Generic;
using System.Threading.Tasks;
using Com.Empyreanmed.Heracles.Logs.V1;
using Heracles.Application.Infra.DataManagement.System.DataAccess.gRPC.Invokers;
using Xcc.Core.Logging;
using ProtoTypesConverter = Heracles.Application.Protos.ProtoTypesConverter;

namespace Heracles.Application.Infra.DataManagement.System.DataAccess.gRPC
{
    public class GrpcLogCommands : ILogCommands
    {
        public GrpcLogMethodsInvoker Invoker { get; }

        public GrpcLogCommands(GrpcLogMethodsInvoker invoker)
        {
            Invoker = invoker;
        }

        #region ILogCommands
        public LogPage<ILogRecord> ReadLogPage(int pageSize)
        {
            var pageResponse = Invoker.ReadLogPage(pageSize);
            return ConvertPage(pageResponse);
        }

        public async Task<LogPage<ILogRecord>> ReadLogPageAsync(int pageSize)
        {
            var pageResponse = await Invoker.ReadLogPageAsync(pageSize);
            return ConvertPage(pageResponse);
        }

        public ILogRecord CreateRecord(ILogRecord record)
        {
            var response = Invoker.CreateRecord(ProtoTypesConverter.ToProto(record));
            return ProtoTypesConverter.FromProto(response);
        }

        public async Task<ILogRecord> CreateRecordAsync(ILogRecord record)
        {
            var response = await Invoker.CreateRecordAsync(ProtoTypesConverter.ToProto(record));
            return ProtoTypesConverter.FromProto(response);
        }
        #endregion ILogCommands

        private LogPage<ILogRecord> ConvertPage(LogPage<Log> pageResponse)
        {
            LogPage<ILogRecord> page = new LogPage<ILogRecord>
            {
                records = new List<ILogRecord>(pageResponse.records != null ? pageResponse.records.Count : 0),
                nextPageToken = pageResponse.nextPageToken
            };
            if (pageResponse.records != null)
            {
                foreach (var record in pageResponse.records)
                {
                    page.records.Add(ProtoTypesConverter.FromProto(record));
                }
            }
            return page;
        }
    }
}
