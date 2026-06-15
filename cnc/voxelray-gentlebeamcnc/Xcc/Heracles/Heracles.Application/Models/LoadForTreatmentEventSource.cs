using Heracles.Core.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xcc.Core.Exceptions;
using Xcc.Infra.Networking.gRPC.EventStreams;

namespace Heracles.Application.Models
{
    public class LoadForTreatmentEventSource(
        ILoadForTreatmentEventStream eventStream,
        CancellationToken globalCancellationToken) 
        : BaseEventSource(
            connectionLossStrategy: new TimeoutOnDisconnect(ReconnectTimeout, globalCancellationToken),
            globalCancellationToken: globalCancellationToken)
    {
        public const uint ReconnectTimeout = 1000;

        public event EventHandler<LoadForTreatmentEventsStreamArgs>? LoadForTreatmentEvent;

        protected override async Task RunEventStreamProcessing(CancellationToken cancellationToken)
        {
            try
            {
                await eventStream.RunStreamAsync((args) =>
                {
                    LoadForTreatmentEvent?.Invoke(this, args);
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
