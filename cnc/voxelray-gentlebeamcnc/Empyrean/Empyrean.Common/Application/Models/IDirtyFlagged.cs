using System.ComponentModel;

namespace Empyrean.Common.Application.Models
{
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
    }

    public interface IPropagateIsValid
    {
        /// <summary>
        /// Indicates if object in the valid state.
        /// </summary>
        public bool IsValid { set; get; }

        public event EventHandler<bool>? IsValidChanged;
    }

    public interface IDirtyFlaggedBindableBase : IDirtyFlagged, INotifyPropertyChanged, INotifyDataErrorInfo, IPropagateIsValid
    {
    }

}
