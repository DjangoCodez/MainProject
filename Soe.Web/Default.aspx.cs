using System;

namespace SoftOne.Soe.Web
{
    public partial class _Default : PageBase
    {
        public _Default()
        {
            ShouldLoadOidcClientScripts = false;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            CheckLanguage();

            // User is authenticated as this is checked in Startup

            Response.Redirect("/soe/", true);
        }
    }
}
