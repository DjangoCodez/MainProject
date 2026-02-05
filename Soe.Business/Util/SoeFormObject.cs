using SoftOne.Soe.Common.Util;
using System.Collections.Specialized;

namespace SoftOne.Soe.Business.Util
{
    public class SoeFormObject
    {
        public SoeFormMode Mode { get; set; }
        public NameValueCollection F { get; set; }
        public string AbsolutePath { get; set; }

        public SoeFormObject()
        {
            Mode = SoeFormMode.Save;
            F = null;
            AbsolutePath = "";
        }
    }
}
