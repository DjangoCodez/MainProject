using SoftOne.Soe.Business.Util.LogCollector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SoftOne.Soe.Web
{
    public partial class ErrorPage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Get the last error from the server
            Exception ex = Server.GetLastError();
            bool isDebug = false;

#if DEBUG
            isDebug = true;
#else
            isDebug = false;
#endif

            // Check if there is an exception and log it
            if (ex != null)
            {
                LogError(ex); // Custom method to log the error

                if (isDebug)
                    throw ex; // Throw the exception if in debug mode

                lblErrorMessage.Text = Server.HtmlEncode(ex.Message); // Display error message (optional)

                // Clear the error from the server
                Server.ClearError();
            }
        }

        /// <summary>
        /// Log error details to a text file or any other logging mechanism.
        /// </summary>
        private void LogError(Exception ex)
        {
            LogCollector.LogError(ex, "From ErrorPage.aspx");
        }
    }
}