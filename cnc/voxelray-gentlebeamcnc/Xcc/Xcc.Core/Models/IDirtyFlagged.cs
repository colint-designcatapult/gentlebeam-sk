using System;
using System.ComponentModel;

namespace Xcc.Core.Models
{
    /// <summary>
    /// Deprecated. Use IDirtyFlagged from Empyrean.Common.Application.Models
    /// </summary>
    [Obsolete]
    public interface IDirtyFlagged
    {
        /// <summary>
        /// Dirty flag for data modification
        /// </summary>
        bool IsModified { get; set; }

        /// <summary>
        /// Resets dirty flag to false
        /// </summary>
        void AcceptChanges();

        /// <summary>
        /// Recursively resets dirty flag to false
        /// </summary>
        void AcceptChangesRecursive();

        void SetIsModified();
        
        event EventHandler<bool>? IsModifiedChanged;
        event EventHandler? Changed;
    }

    /// <summary>
    /// Deprecated. Use from Empyrean.Common.Application.Models
    /// </summary>
    [Obsolete]
    public interface IPropagateIsValid
    {
        /// <summary>
        /// Indicates if object in the valid state.
        /// </summary>
        public bool IsValid { set; get; }

        public event EventHandler<bool>? IsValidChanged;
    }

    /// <summary>
    /// Deprecated. Use from Empyrean.Common.Application.Models
    /// </summary>
    [Obsolete]
    public interface IDirtyFlaggedBindableBase : IDirtyFlagged, INotifyPropertyChanged, INotifyDataErrorInfo, IPropagateIsValid
    {
    }

}
