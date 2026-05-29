using Heracles.Core.Commands;
using Heracles.Core.Models.EMR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xcc.Core.Exceptions;
using Xcc.Infra.Networking.gRPC.EventStreams;

namespace Heracles.Application.Models
{
    public class PlanEventSource(
        IPlanEventStream eventStream,
        CancellationToken globalCancellationToken)
        : BaseEventSource(
            connectionLossStrategy: new TimeoutOnDisconnect(ReconnectTimeout, globalCancellationToken),
            globalCancellationToken: globalCancellationToken)
    {
        public const uint ReconnectTimeout = 1000;

        public event EventHandler<IPlan>? PlanChangedEvent;

        protected override async Task RunEventStreamProcessing(CancellationToken cancellationToken)
        {
            try
            {
                await eventStream.RunStreamAsync((args) =>
                {
                    PlanChangedEvent?.Invoke(this, args);
                }, cancellationToken
                );
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new PlanForTreatmentException("Treatment event stream fatal error.", ex);
            }
        }
    }
}
