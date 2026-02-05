using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;

namespace SoftOne.Soe.Util.Exceptions
{
    [Serializable]
    public abstract class SoeException : Exception
    {
        public SoeException()
        {
        }

        public SoeException(string message)
            : base(message)
        {
        }

        public SoeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }


        public SoeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        public static string GetStackTrace(int? nrOfLines = null)
        {
            StackTrace trace = new StackTrace();
            return GetStackTrace(trace.GetFrames(), nrOfLines);
        }

        private static string GetStackTrace(StackFrame[] stackFrames, int? nrOfLines = null)
        {
            string message = "";
            string delimeter = " --> ";

            if (stackFrames == null || stackFrames.Length == 0)
                return message;

            int count = 0;

            foreach (StackFrame frame in stackFrames)
            {
                try
                {
                    MethodBase method = frame.GetMethod();
                    if (method == null)
                        continue;

                    //Exclude ctor
                    if (method.IsConstructor)
                        continue;

                    string nameSpace = method.DeclaringType != null ? method.DeclaringType.Namespace : String.Empty;
                    string declaringTypeName = method.DeclaringType != null ? method.DeclaringType.Name : String.Empty;
                    string methodName = method.Name;

                    //Include internal calls only
                    if (!nameSpace.ToString().StartsWith("SoftOne"))
                        continue;

                    //Exclude internal exception handling calls
                    if (nameSpace.ToString().StartsWith("SoftOne.Soe.Util.Exceptions"))
                        continue;
                    
                    //Delimiter
                    if (!String.IsNullOrEmpty(message))
                        message += delimeter;

                    //Namespace.Class.Method
                    message += nameSpace + "." + declaringTypeName + "." + methodName + "()";

                    if (nrOfLines.HasValue && ++count > nrOfLines)
                        break;                    
                }
                catch(Exception ex)
                {
                    ex.ToString(); //prevent compiler warning
                }
            }

            return message;
        }
    }
}
