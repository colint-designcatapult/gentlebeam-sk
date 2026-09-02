using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Xcc.Infra.Networking.gRPC.EventStreams
{
    public sealed class BroadcastEventHub<T>
    {
        private readonly object _sync = new();
        private readonly HashSet<Channel<T>> _subscribers = [];

        public void Publish(T value)
        {
            lock (_sync)
            {
                foreach (Channel<T> subscriber in _subscribers)
                {
                    subscriber.Writer.TryWrite(value);
                }
            }
        }

        public async Task StreamAsync(
            Func<T, CancellationToken, Task> consumeAsync,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(consumeAsync);

            var channel = Channel.CreateUnbounded<T>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false,
            });
            lock (_sync)
            {
                _subscribers.Add(channel);
            }

            try
            {
                await foreach (T value in channel.Reader.ReadAllAsync(cancellationToken))
                {
                    await consumeAsync(value, cancellationToken);
                }
            }
            finally
            {
                lock (_sync)
                {
                    _subscribers.Remove(channel);
                }
                channel.Writer.TryComplete();
            }
        }
    }
}
