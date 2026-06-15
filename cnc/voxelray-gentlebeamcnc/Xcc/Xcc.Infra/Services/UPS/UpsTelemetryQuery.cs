using System.Threading;
using Xcc.Core.Domain.UPS;
using Xcc.Infra.UPS;

namespace Xcc.Infra.Services.UPS
{
    public class UpsTelemetryQuery(UpsDevice device)
    {
        public IUpsTelemetry QueryTelemetry(CancellationToken? token = null)
        {
            UpsTelemetry upsTelemetry = new UpsTelemetry();

            upsTelemetry.ParseSystemData(QuerySystemData(token));

            upsTelemetry.ParseBatteryData(QueryBatteryData(token));

            upsTelemetry.ParseAlarmsData(QueryAlarmsData(token));

            upsTelemetry.ParseOutputData(QueryOutputData(token));

            upsTelemetry.ParseInputData(QueryInputData(1, token));

            upsTelemetry.ParseUnitIdData(QueryUidData(token));

            //for (int i = 0; i < 3; i++)
            //{
            //    circuitDataTokens[i] = QueryCircuitData(i+1, token);
            //}

            return upsTelemetry;
        }

        public string[] QueryData(string query, CancellationToken? token)
        {
            return device.ExecuteQuery(query, token).Split(';');
        }

        public string[] QuerySystemData(CancellationToken? token) => QueryData("SYS", token);
        public string[] QueryBatteryData(CancellationToken? token) => QueryData("BAT", token);
        public string[] QueryAlarmsData(CancellationToken? token) => QueryData("ALM", token);
        public string[] QueryOutputData(CancellationToken? token) => QueryData("OUT", token);
        public string[] QueryInputData(int input, CancellationToken? token) => QueryData($"INP.{input}", token);
        public string[] QueryCircuitData(int index, CancellationToken? token) => QueryData($"PDU.{index}", token);
        public string[] QueryUidData(CancellationToken? token) => QueryData("UID", token);
    }
}
