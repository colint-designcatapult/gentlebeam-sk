using System;
using System.ComponentModel;

namespace Heracles.Application.Models.Supervision
{
    public interface IDisruptiveActionWatchdog<TValueType>
        where TValueType : INotifyPropertyChanged
    {
        void SetObject(TValueType? observableObject);
        bool UpdateLock();
    }

    public class DisruptiveActionWatchdog<TActionType, TValueType> : IDisruptiveActionWatchdog<TValueType>
        where TActionType : DisruptiveAction, new()
        where TValueType : INotifyPropertyChanged
    {
        private bool lockState = false;
        private IDisruptiveActionGuardLock disruptiveActionGuardLock;
        private TValueType? observableObject;
        private readonly Func<TValueType?, bool> lockPredicate;
        private DisruptiveActionLockArgs lockArgs = new DisruptiveActionLockArgs();

        public DisruptiveActionWatchdog(
            IDisruptiveActionGuardLock disruptiveActionGuardLock,
            DisruptiveActionLockArgs args,
            Func<TValueType?, bool> predicate,
            TValueType? observableObject
            )
        {
            this.disruptiveActionGuardLock = disruptiveActionGuardLock;
            lockArgs = args;
            lockPredicate = predicate;
            SetObject(observableObject);
        }

        public void SetObject(TValueType? observableObject)
        {
            this.observableObject = observableObject;
            // Set update listener
            if (observableObject != null)
            {
                observableObject.PropertyChanged += (s, e) => UpdateLock();
            }
            UpdateLock();
        }

        public bool UpdateLock()
        {
            if (lockPredicate(observableObject) != lockState)
            {
                SwitchLock();
            }
            return lockState;
        }

        private void SwitchLock()
        {
            if (!lockState)
            {
                disruptiveActionGuardLock.AddGuardLock<TActionType>(this, lockArgs);
            }
            else
            {
                disruptiveActionGuardLock.RemoveGuardLock<TActionType>(this);
            }
            lockState = !lockState;
        }
    }
}
