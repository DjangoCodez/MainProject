using System;

namespace SoftOne.Soe.Util.Exceptions
{
    public class SoeTransactionFailedException : SoeException
    {
        public SoeTransactionFailedException(string source)
            : base("The transaction failed" + ". " + GetStackTrace())
        {
            base.Source = source;
        }

        public SoeTransactionFailedException(Exception innerException, string source)
            : base("The transaction failed" + ". " + GetStackTrace(), innerException)
        {
            base.Source = source;
        }
    }
}
