using System.Text;

namespace Heracles.Application.Models
{
    public class AppGlobals : Xcc.Application.Models.AppGlobals
    {
        public static Encoding Encoding = Encoding.UTF8; //for debug, doesn't support chinese symbols, but allows insert binary fields from scripts

        //public static Encoding Encoding = Encoding.Unicode; // supports chinese symbols
    }
}
