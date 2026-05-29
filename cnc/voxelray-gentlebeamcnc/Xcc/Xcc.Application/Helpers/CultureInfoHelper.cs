using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Markup;

namespace Xcc.Application.Helpers;

public static class CultureInfoHelper
{
    public static void SetCurrentCulture(string name = "en-US")
    {

        var defaultCulture = new CultureInfo(name);

        Thread.CurrentThread.CurrentCulture = defaultCulture;
        Thread.CurrentThread.CurrentUICulture = defaultCulture;
        CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
        CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;

        FrameworkElement.LanguageProperty.OverrideMetadata(
            typeof(FrameworkElement),
            new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
    }
}