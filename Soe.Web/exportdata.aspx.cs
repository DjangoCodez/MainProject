using System;
using System.Text;

namespace SoftOne.Soe.Web
{
    public partial class exportdata : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // TODO: Encoding in ISO-8859-1 makes Excel work fine, but this is no good since Unicode characters
            // won't work. There shold be a real Excel file builder and exporter instead, and a separate plaintext exporter
            // that uses UTF-8.

            Response.Clear();
            Response.ContentType = "text/csv"; // This makes Excel default application for opening.
            Response.ContentEncoding = Encoding.GetEncoding("ISO-8859-1");

            //Remark: Content-Disposition is not a standard HTTP header
            // TODO: export.csv hard coded.
            Response.AddHeader("Content-Disposition", "attachment; filename=export.csv");

            Response.Write(Request.Form["data"]);
        }
    }
}
