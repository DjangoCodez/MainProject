using System;
using System.Web.Security;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.estatus
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Estatus;
            base.Page_Init(sender, e);
            
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            /*
            if (SoeUser != null && !String.IsNullOrEmpty(SoeUser.EstatusLoginId))
            {
                String currentDatetime = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                String hashKey = "DwsjkldJKLL789DSjklsdajklsda23u823";                
                String hashValue = FormsAuthentication.HashPasswordForStoringInConfigFile(SoeUser.EstatusLoginId + currentDatetime + hashKey, "SHA1");

                Response.Redirect("http://henrikd:82/login/default.aspx?li=" + SoeUser.EstatusLoginId + "&dt=" + currentDatetime + "&hv=" + hashValue);

            }
            else
            {
                

            } 
            */           
        }
    }
}
