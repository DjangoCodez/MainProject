using System;

namespace SoftOne.Soe.Util.Exceptions
{
    [Serializable]
    public class SoeGeneralException : SoeException
    {
        public SoeGeneralException(string message, string source)
            : base(message)
        {
            base.Source = source;
        }

        public SoeGeneralException(string message, Exception innerException, string source)
            : base(message, innerException)
        {
            base.Source = source;
        }
    }
}
