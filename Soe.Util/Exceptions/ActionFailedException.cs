using System;

namespace SoftOne.Soe.Util.Exceptions
{
    public class ActionFailedException : SoeException
    {
		public int ErrorNumber { get; private set; }

		public ActionFailedException(string message, Exception ex = null)
			: this(0, message, ex)
		{
		}

		public ActionFailedException(int errorNumber = 0, string message = "", Exception ex = null)
			: base(message, ex)
		{
			this.ErrorNumber = errorNumber;
		}
	}
}
