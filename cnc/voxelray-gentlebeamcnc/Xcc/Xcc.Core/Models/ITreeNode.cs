using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Xcc.Core.Enums;

namespace Xcc.Core.Models
{
    public class CRUDEntryChangedArgs : EventArgs
    {
        public CRUDEntryChangedAction Action { get; }
        public object? Data { get; }
        public CRUDEntryChangedArgs(CRUDEntryChangedAction action, object? data = null)
        {
            Action = action;
            Data = data;
        }
    }

    public interface IObservableEntry
    {
        event EventHandler<CRUDEntryChangedArgs> EntryChanged;
    }

    public interface IDataEntry<TData>
    where TData : class
    {
        TData Data { get; set; }
    }

    public interface IAsyncDataEntry<TData>
    where TData : class
    {
        TData? Data { get; }
        Task<TData> UpdateAsync(TData data);
    }

    public interface IDataEntryHandler<TData> : IRemovable, IEntryAccess<TData>
    {
        public TData GetLocalData();
    }

    public interface IObservableDataEntryHandler<TData> : IDataEntryHandler<TData>, IObservableEntry
    {
    }


    public interface IObservableDataEntry<TData> : IAsyncDataEntry<TData>, IObservableEntry
        where TData : class
    {
        void SetData(TData newValue);
    }


    public interface INode<TChild> : INotifyCollectionChanged
    {
        public ICollection<TChild> Children { get; }
    }
    public interface ITreeManipulations<TChild>
    {
        void AddChild(TChild child);
        bool RemoveChild(TChild child);
    }

    public interface ITreeNode<TChild> : ITreeManipulations<TChild>, INode<TChild>
        where TChild : IObservableEntry
    {
        void Observe(TChild child);
    }

    public interface ITreeNodeContainer<TChild>
        where TChild : IObservableEntry
    {
        ITreeNode<TChild> Node { get; }
    }
}
