using System;
using Xcc.Core.Domain.GryphonBoard;

namespace Xcc.Application.Models
{
    public interface INotifyWarmupEvent
    {
        event EventHandler<WarmupEventArgs> WarmupEvent;
    }

    public class WarmupEventArgs
    {
        public WarmupEventType EventType { get; }
        public WarmupParameters WarmupParameters { get; }
        public double HeaterCurrent { get; }

        public WarmupEventArgs(WarmupEventType eventType, WarmupParameters warmupParameters)
        {
            EventType = eventType;
            WarmupParameters = warmupParameters;
        }

        public static WarmupEventArgs Start(WarmupParameters warmupParameters)
        {
            return new WarmupEventArgs(WarmupEventType.Start, warmupParameters);
        }

        public static WarmupEventArgs Done(WarmupParameters warmupParameters)
        {
            return new WarmupEventArgs(WarmupEventType.Done, warmupParameters);
        }
    }

    public enum WarmupEventType
    {
        Start = 0,
        Done = 1,
        Failed = 2,
    }
}
