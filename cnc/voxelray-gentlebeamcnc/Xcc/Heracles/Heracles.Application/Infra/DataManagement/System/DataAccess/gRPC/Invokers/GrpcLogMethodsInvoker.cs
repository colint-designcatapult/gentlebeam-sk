using System;
using System.Threading.Tasks;
using Com.Empyreanmed.Heracles.Logs.V1;
using Grpc.Core;
using Xcc.Core.Exceptions;
using Xcc.Core.Logging;
using Xcc.Infra.Networking.gRPC.Channels;

namespace Heracles.Application.Infra.DataManagement.System.DataAccess.gRPC.Invokers
{
    public class GrpcLogMethodsInvoker
    {
        protected IGrpcChannelManager GrpcSettings { get; }

        protected CallInvoker Channel => GrpcSettings.Channel;
        protected uint Timeout => GrpcSettings.RpcTimeoutMs;

        public GrpcLogMethodsInvoker(IGrpcChannelManager grpcSettings)
        {
            GrpcSettings = grpcSettings;
        }

        private DateTime GetDeadline()
        {
            return DateTime.UtcNow.AddMilliseconds(Timeout);
        }

        private LogService.LogServiceClient GetService()
        {
            return new LogService.LogServiceClient(Channel);
        }

        protected CallOptions GetCallOptions()
        {
            return new CallOptions(deadline: GetDeadline(), headers: GrpcSettings.Headers);
        }
        #region IGrpcLogMethodsInvoker
        public Log CreateRecord(Log record)
        {
            try
            {
                var response = GetService().CreateLog(new CreateLogRequest { Log = record }, GetCallOptions());
                return response.Log;
            }
            catch (Exception e)
            {
                string msg = $"Failed to add a log record to the DB. Message={record.Message}, Type={record.Type}, Severity={record.Severity}, Id={record.Id}";
                throw new DataServiceException(msg, e);
            }
        }

        public async Task<Log> CreateRecordAsync(Log record)
        {
            try
            {
                var response = await GetService().CreateLogAsync(new CreateLogRequest { Log = record }, GetCallOptions());
                return response.Log;
            }
            catch (Exception e)
            {
                string msg = $"Failed to add a log record to the DB";
                throw new DataServiceException(msg, e);
            }
        }

        public LogPage<Log> ReadLogPage(int pageSize)
        {
            try
            {
                var request = new ListLogsRequest
                {
                    Get = 1000,
                    Skip = 0
                };

                var response = GetService().ListLogs(request, GetCallOptions());

                return new LogPage<Log> { records = response.Logs, nextPageToken = response.NextPageToken };
            }
            catch (Exception e)
            {
                string msg = $"Failed to read a log page from the DB";
                throw new DataServiceException(msg, e);
            }
        }

        public async Task<LogPage<Log>> ReadLogPageAsync(int pageSize)
        {
            try
            {
                var request = new ListLogsRequest
                {
                    Get = 1000,
                    Skip = 0
                };

                var response = await GetService().ListLogsAsync(request, GetCallOptions());

                return new LogPage<Log> { records = response.Logs, nextPageToken = response.NextPageToken };
            }
            catch (Exception e)
            {
                string msg = $"Failed to read a log page from the DB";
                throw new DataServiceException(msg, e);
            }
        }
        #endregion IGrpcLogMethodsInvoker
    }
}
