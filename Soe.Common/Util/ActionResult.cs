using SoftOne.Soe.Common.DTO;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TypeLite;

namespace SoftOne.Soe.Common.Util
{
    [KnownType(typeof(List<TaskWatchLogDTO>))]
    [Cleaner]
    public class ActionResult
    {
        #region Public properties

        public bool Success { get; set; }
        public int SuccessNumber { get; set; }
        public int ErrorNumber { get; set; }
        public string ErrorMessage { get; set; }
        public string InfoMessage { get; set; }
        public bool CanUserOverride { get; set; }

        public int IntegerValue { get; set; }
        public int IntegerValue2 { get; set; }
        public decimal DecimalValue { get; set; }
        public string StringValue { get; set; }
        public bool BooleanValue { get; set; }
        public bool BooleanValue2 { get; set; }
        public DateTime DateTimeValue { get; set; }

        public object Value { get; set; }
        public object Value2 { get; set; }
        public List<DateTime> Dates { get; set; }
        public List<int> Keys { get; set; }
        public List<string> Strings { get; set; }
        [TsIgnore]
        public Dictionary<int, int> IdDict { get; set; }
        [TsIgnore]
        public Dictionary<int, int> IntDict { get; set; }
        [TsIgnore]
        public Dictionary<int, int> IntDict2 { get; set; }
        [TsIgnore]
        public Dictionary<int, string> StrDict { get; set; }
        [TsIgnore]
        public Dictionary<string, int> StrDict2 { get; set; }
        [TsIgnore]
        public Dictionary<string, List<int>> StrIntListDict { get; set; }

        public int ObjectsAffected { get; set; }
        [ClearField]
        public string StackTrace { get; set; }

        private Exception ex;
        [TsIgnore]
        [ClearField]
        public Exception Exception
        {
            set
            {
                if (value != null)
                {
                    ex = new Exception("See error message for details");
                    Success = false;
                    SuccessNumber = (int)ActionResultSave.Unknown;
                    ErrorNumber = (int)ActionResultSave.Unknown;

                    ErrorMessage += value.Message;
                    StackTrace += value.StackTrace;

                    Exception innerException = value.InnerException;
                    while (innerException != null)
                    {
                        ErrorMessage += " " + innerException.Message;
                        innerException = innerException.InnerException;
                    }
                }
            }
            get
            {
                return ex;
            }
        }

        public DateTime Modified { get; set; }
        public string ModifiedBy { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Success defaulted to true
        /// </summary>
        public ActionResult()
        {
            Success = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorMessage"></param>
        public ActionResult(string errorMessage)
        {
            Success = false;
            ErrorMessage = errorMessage;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="success">Successful or not?</param>
        public ActionResult(bool success)
        {
            Success = success;
            SuccessNumber = (int)ActionResultSave.Unknown;
            ErrorNumber = (int)ActionResultSave.Unknown;
        }

        /// <summary>
        /// Success defaulted to false
        /// </summary>
        /// <param name="errNr">Error number</param>
        public ActionResult(int errNr)
        {
            Success = false;

            SuccessNumber = (int)ActionResultSave.Unknown;
            ErrorNumber = errNr;
        }

        public ActionResult(int errNr, string errMessage, string stringValue = null)
        {
            Success = false;

            SuccessNumber = (int)ActionResultSave.Unknown;
            ErrorNumber = errNr;
            ErrorMessage = errMessage;
            if (!String.IsNullOrEmpty(stringValue))
                StringValue = stringValue;
        }


        public ActionResult(int errNr, int integerValue)
        {
            Success = false;

            SuccessNumber = (int)ActionResultSave.Unknown;
            ErrorNumber = errNr;
            IntegerValue = integerValue;
        }

        public ActionResult(bool success, int errNr, string errMessage, int? integerValue = null)
        {
            Success = success;

            if (success)
            {
                SuccessNumber = errNr;
                ErrorNumber = (int)ActionResultSave.Unknown;
            }
            else
            {
                SuccessNumber = (int)ActionResultSave.Unknown;
                ErrorNumber = errNr;
            }
            ErrorMessage = errMessage;
            if (integerValue.HasValue)
                IntegerValue = integerValue.Value;
        }

        /// <summary>
        /// Success defaulted to false
        /// </summary>
        /// <param name="ex">Exception</param>
        public ActionResult(Exception ex)
        {
            Success = false;

            SuccessNumber = (int)ActionResultSave.Unknown;
            ErrorNumber = (int)ActionResultSave.Unknown;
            if (ex != null)
            {
                StackTrace = ex.StackTrace;
                ErrorMessage = ex.Message;
                while (ex.InnerException != null)
                {
                    ErrorMessage += "\n" + ex.InnerException;
                    ex = ex.InnerException;
                }
            }
        }

        /// <summary>
        /// Success defaulted to false
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <param name="msg">Additional message before error message</param>
        public ActionResult(Exception ex, string msg)
        {
            Success = false;

            SuccessNumber = (int)ActionResultSave.Unknown;
            ErrorNumber = (int)ActionResultSave.Unknown;
            if (ex != null)
            {
                StackTrace = ex.StackTrace;
                ErrorMessage = msg + ex.Message;
                while (ex.InnerException != null)
                {
                    ErrorMessage += "\n" + ex.InnerException;
                    ex = ex.InnerException;
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a detailed error message containing the Error Message, inner exceptions and stack trace.
        /// </summary>
        /// <returns></returns>
        public string GetErrorMsg()
        {
            var msg = string.Empty;
            if (!string.IsNullOrEmpty(this.ErrorMessage))
            {
                msg += string.Concat("Error message: ", this.ErrorMessage, Environment.NewLine);
            }

            if (this.Exception != null)
            {
                int i = 1;
                var iterator = this.Exception;
                while (iterator != null)
                {
                    msg += string.Concat("Exception Message ", i++, ": ", iterator.Message, Environment.NewLine);
                    iterator = iterator.InnerException;
                }

                msg += "Stacktrace: " + this.Exception.StackTrace;
            }

            return msg;
        }

        public bool DoAcceptError(List<ActionResultSave> acceptErrors)
        {
            return acceptErrors?.Contains((ActionResultSave)this.ErrorNumber) ?? false;
        }

        #endregion
    }
}
