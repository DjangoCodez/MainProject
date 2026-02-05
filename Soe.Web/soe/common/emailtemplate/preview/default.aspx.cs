using SoftOne.Soe.Business.Core;
using System;
using System.Web.UI;


namespace SoftOne.Soe.Web.soe.common.emailtemplate.preview
{
    public partial class _default : Page
    {
        protected EmailManager em;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                em = new EmailManager(null);
                int templateId = Convert.ToInt32(Request.QueryString["t"]);
                int actorCompanyId= Convert.ToInt32(Request.QueryString["a"]);
                var template = em.GetEmailTemplate(templateId, actorCompanyId);
                Response.Write(template.Subject);
                Response.Write("<br />");
                Response.Write(template.Body);
                Response.End();
            }
            catch (Exception ex)
            {
                ex.ToString(); //prevent compiler warning
            }
        }
    }
}
