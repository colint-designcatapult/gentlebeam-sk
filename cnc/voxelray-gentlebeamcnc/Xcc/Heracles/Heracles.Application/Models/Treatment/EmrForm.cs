using Xcc.Application.Helpers;
using Xcc.Core.Models;

namespace Heracles.Application.Models.Treatment
{
    public interface IEmrForm : IDirtyFlaggedBindableBase
    {
        bool IsReadOnly { get; set; }
    }

    public class EmrForm : DirtyFlaggedBindableBase, IEmrForm
    {
        private bool isReadOnly;

        public bool IsReadOnly { get => isReadOnly; set => SetProperty(ref isReadOnly, value); }
    }

}
