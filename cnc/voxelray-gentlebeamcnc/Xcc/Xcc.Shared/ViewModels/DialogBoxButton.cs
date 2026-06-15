using Prism.Services.Dialogs;

namespace Xcc.Shared.ViewModels
{
    public class DialogBoxButton(string text, ButtonResult result, bool isDefault = false, bool isCancel = false)
    {
        public string Text => text;
        public ButtonResult Result => result;
        public bool IsDefault => isDefault;
        public bool IsCancel => isCancel;
    }
}
