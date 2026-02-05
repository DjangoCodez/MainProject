using SoftOne.Soe.Common.Util;
using System;
using System.Linq;
using System.Reflection;

namespace SoftOne.Soe.Business.Util
{
    public static class CleanerUtil
    {
        public static bool IsValidForCleanup(object model)
        {
            if (model == null)
                return false;

            Type type = model.GetType();
            if (!type.HasCleanerAttribute())
                return false;

            return true;
        }

        /// <summary>
        /// Cleans the object properties by replacing exception details with user-friendly messages.
        /// </summary>
        public static void CleanObject(object model)
        {
            if (model == null) return; // Null check for the model

            // Get the properties of the model
            var properties = model.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();

            for (int i = 0; i < properties.Count; i++)
            {
                var propInfo = properties[i];
                var value = propInfo.GetValue(model);

                if (propInfo.HasClearFieldAttribute()) 
                {
                    // Check if the property value is an exception
                    if (value is Exception ex)
                    {
                        // Handle and clean the exception details
                        var errorMessage = CleanErrorMessage(ex, propInfo, model);
                        propInfo.SetValue(model, errorMessage);
                    }
                    else if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
                    {
                        // Create an exception from string value and clean it
                        var errorMessage = CleanErrorMessage(new Exception(stringValue), propInfo, model);
                        propInfo.SetValue(model, errorMessage);
                    }
                    else
                    {
                        // Set non-exception or empty values to null
                        propInfo.SetValue(model, null);
                    }
                }
            }
        }

        /// <summary>
        /// Cleans the exception details and returns a user-friendly message.
        /// </summary>

        private static string CleanErrorMessage(Exception ex, PropertyInfo propInfo, object model)
        {
            string errorMessage = "An unexpected error occurred. Please try again later or contact support.";

            if (ex != null)
            {
                if (ex.ToString().Contains("Timeout"))
                    errorMessage = "Timeout error occurred. Please try again later or contact support";
                else if (ex.ToString().Contains("NullReferenceException"))
                    errorMessage = "Null reference error occurred. Please try again later or contact support";
                else if (ex.ToString().Contains("OutOfMemoryException"))
                    errorMessage = "Out of memory error occurred. Please try again later or contact support";
                else if (ex.ToString().Contains("StackOverflowException"))
                    errorMessage = "Stack overflow error occurred. Please try again later or contact support";
                else if (ex.ToString().Contains("ArgumentException"))
                    errorMessage = "Argument error occurred. Please try again later or contact support";
                else if (ex.ToString().Contains("ArgumentNullException"))
                    errorMessage = "Argument null error occurred. Please try again later or contact support";
                else if (ex.ToString().Contains("ArgumentOutOfRangeException"))
                    errorMessage = "Argument out of range error occurred. Please try again later or contact support";
                else if (ex.ToString().Contains("FormatException"))
                    errorMessage = "Format error occurred. Please try again later or contact support";
                else if (ex.ToString().Contains("IndexOutOfRangeException"))
                    errorMessage = "Index out of range error occurred. Please try again later or contact support";
                else if (ex.ToString().Contains("InvalidCastException"))
                    errorMessage = "Invalid cast error occurred. Please try again later or contact support";
                else if (ex.ToString().Contains("InvalidOperationException"))
                    errorMessage = "Invalid operation error occurred. Please try again later or contact support";
                else if (ex.ToString().Contains("KeyNotFoundException"))
                    errorMessage = "Key not found error occurred. Please try again later or contact support";
                else if (ex.ToString().Contains("NotSupportedException"))
                    errorMessage = "Not supported error occurred. Please try again later or contact support";
                else if (ex.ToString().Contains("ObjectDisposedException"))
                    errorMessage = "Object disposed error occurred. Please try again later or contact support";
                else if (ex.ToString().Contains("UnauthorizedAccessException"))
                    errorMessage = "Unauthorized access error occurred. Please try again later or contact support";
                else if (ex.ToString().Contains("FileNotFoundException"))
                    errorMessage = "File not found error occurred. Please try again later";
                else
                    errorMessage = "See error message for details";
            }

            return "SOE: " + errorMessage;
        }
    }
}
