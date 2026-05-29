using Xcc.Shared.ViewModels;
using Prism.Services.Dialogs;

namespace Xcc.Test.Xcc.Shared.ViewModels
{
    public class DialogBoxButtonTests
    {
        [Test]
        public void DialogBoxButton_Ctor(
            [Values("yes", "no")] string text,
            [Values(ButtonResult.Yes, ButtonResult.No)] ButtonResult result,
            [Values(false, true)] bool isDefault,
            [Values(false, true)] bool isCancel)
        {
            var sut = new DialogBoxButton(text, result, isDefault, isCancel);

            Assert.That(sut.Text, Is.EqualTo(text));
            Assert.That(sut.Result, Is.EqualTo(result));
            Assert.That(sut.IsDefault, Is.EqualTo(isDefault));
            Assert.That(sut.IsCancel, Is.EqualTo(isCancel));
        }
    }
}