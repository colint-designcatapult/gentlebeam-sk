using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Heracles.Application.Models.Supervision
{
    public enum DisruptiveActionLockType
    {
        None = 0,
        Warn = 1,
        Block = 2,
    }

    public abstract class DisruptiveAction
    {
        public DisruptiveAction() { }
    }

    public class DisruptiveActionLockArgs
    {
        public DisruptiveActionLockType LockType { get; set; }
        public Action InvokeAction { get; set; }
        public string Message { get; set; }
        public DisruptiveActionLockArgs() { }
    }

    public interface IDisruptiveActionGuardLock
    {
        void AddGuardLock<TActionType>(object sender, DisruptiveActionLockType lockType, string message, Action invokeAction = null)
            where TActionType : DisruptiveAction, new();
        void AddGuardLock<TActionType>(object sender, DisruptiveActionLockArgs args)
            where TActionType : DisruptiveAction, new();

        void RemoveGuardLock<TActionType>(object sender)
            where TActionType : DisruptiveAction, new();
    }

    public interface IDisruptiveActionGuardService
    {
        DisruptiveActionLockType GetLockType<TActionType>()
            where TActionType : DisruptiveAction, new();

        void Invoke<TActionType>()
            where TActionType : DisruptiveAction, new();
    }

    public interface IDisruptiveActionWatchdogFactory
    {
        IDisruptiveActionWatchdog<TValueType> MakeWatchdog<TActionType, TValueType>(
            DisruptiveActionLockArgs args,
            Func<TValueType?, bool> predicate,
            TValueType? observableObject
            )
            where TActionType : DisruptiveAction, new()
            where TValueType : INotifyPropertyChanged;
    }

    public class DisruptiveActionGuard : IDisruptiveActionGuardLock, IDisruptiveActionGuardService, IDisruptiveActionWatchdogFactory
    {
        private Dictionary<Type, Dictionary<object, DisruptiveActionLockArgs>> LockRepository { get; set; } = new();


        #region IDisruptiveActionGuardLock
        public void AddGuardLock<TActionType>(object sender, DisruptiveActionLockType lockType, string message, Action invokeAction = null) where TActionType : DisruptiveAction, new()
        {
            GetActionLocks<TActionType>()[sender] = new DisruptiveActionLockArgs { LockType = lockType, Message = message, InvokeAction = invokeAction };
        }

        public void AddGuardLock<TActionType>(object sender, DisruptiveActionLockArgs args) where TActionType : DisruptiveAction, new()
        {
            GetActionLocks<TActionType>()[sender] = args;
        }

        public void RemoveGuardLock<TActionType>(object sender) where TActionType : DisruptiveAction, new()
        {
            GetActionLocks<TActionType>().Remove(sender);
        }
        #endregion IDisruptiveActionGuardLock


        #region IDisruptiveActionGuardService
        public DisruptiveActionLockType GetLockType<TActionType>() where TActionType : DisruptiveAction, new()
        {
            var locks = GetActionLocks<TActionType>();
            if (locks.Any(kv => kv.Value.LockType.Equals(DisruptiveActionLockType.Block)))
            {
                return DisruptiveActionLockType.Block;
            }
            else if (locks.Any(kv => kv.Value.LockType.Equals(DisruptiveActionLockType.Warn)))
            {
                return DisruptiveActionLockType.Warn;
            }
            else
            {
                return DisruptiveActionLockType.None;
            }
        }

        /// <summary>
        /// If there's no blocking locks for this type of action,
        /// invokes all the specified guard locks' callback actions and then removes all the locks.
        /// </summary>
        /// <typeparam name="TActionType"></typeparam>
        /// <exception cref="InvalidOperationException"></exception>
        public void Invoke<TActionType>() where TActionType : DisruptiveAction, new()
        {
            if (GetLockType<TActionType>() == DisruptiveActionLockType.Block)
            {
                throw new InvalidOperationException($"Cannot invoke a disruptive action of type {typeof(TActionType)} due to a blocking lock on it");
            }
            var locks = GetActionLocks<TActionType>();
            foreach(var lockItem in locks)
            {
                lockItem.Value.InvokeAction?.Invoke();
            }

            // Now, when the action is done, its locks must be cleared:
            locks.Clear();
        }
        #endregion IDisruptiveActionGuardService

        #region IDisruptiveActionWatchdogFactory
        /// <summary>
        /// The watchdog factory method. 
        /// May be extracted into a separate factory class,
        /// was placed here to avoid direct dependency on concrete Watchdog type in its clients
        /// </summary>
        /// <typeparam name="TActionType"></typeparam>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="args"></param>
        /// <param name="predicate"></param>
        /// <param name="observableObject"></param>
        /// <returns></returns>
        public IDisruptiveActionWatchdog<TValueType> MakeWatchdog<TActionType, TValueType>(DisruptiveActionLockArgs args, Func<TValueType?, bool> predicate, TValueType? observableObject)
            where TActionType : DisruptiveAction, new()
            where TValueType : INotifyPropertyChanged
        {
            return new DisruptiveActionWatchdog<TActionType, TValueType>(this, args, predicate, observableObject);
        }
        #endregion

        public DisruptiveActionGuard()
        {
        }

        #region private methods
        private Dictionary<object, DisruptiveActionLockArgs> GetActionLocks<TActionType>()
            where TActionType: DisruptiveAction, new()
        {
            var actionType = typeof(TActionType);
            if (!LockRepository.ContainsKey(actionType))
            {
                LockRepository.Add(actionType, new Dictionary<object, DisruptiveActionLockArgs>());
            }
            return LockRepository[actionType];
        }

        #endregion private methods
    }
}
