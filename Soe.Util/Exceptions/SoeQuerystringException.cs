using System;

namespace SoftOne.Soe.Util.Exceptions
{
    [Serializable]
    public class SoeQuerystringException : SoeException
    {
        public SoeQuerystringException(string qs, string source)
            : base("QueryString " + qs + " is invalid")
        {
            base.Source = source;
        }

        public SoeQuerystringException(string qs, Exception innerException, string source)
            : base("QueryString " + qs + " is invalid", innerException)
        {
            base.Source = source;
        }
    }
}
