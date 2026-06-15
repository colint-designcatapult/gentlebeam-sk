using System.Globalization;

namespace Empyrean.Common.Test
{
    public static class G
    {
        // Same as in App.xaml.cs, OnStartup()
        public static readonly CultureInfo Culture = new("en-US");

        public static void SetupCulture()
        {
            Thread.CurrentThread.CurrentCulture = Culture;
            Thread.CurrentThread.CurrentUICulture = Culture;
            CultureInfo.DefaultThreadCurrentCulture = Culture;
            CultureInfo.DefaultThreadCurrentUICulture = Culture;
        }
    }
}