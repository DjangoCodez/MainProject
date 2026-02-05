using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using System.Diagnostics;

namespace SoftOne.Soe.Web
{
    public static class AngularConfig
    {
#if DEBUG
        public const bool isDebug = true;
#else
        public const bool isDebug = false;
#endif
        public static bool UseCacheBusting { get { return !isDebug; } }
        public static bool UseMinified { get { return !isDebug; } }
        public static bool UseBundle { get { return !isDebug; } }
        public static string Prefix { get { return isDebug ? "/angular/build/" : "/angular/dist/"; } }
        public const string ApiPrefix = "/api/";
        public const string TermVersionNr = "64.2";
    }
}
