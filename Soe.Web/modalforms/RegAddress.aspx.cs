using System;
using SoftOne.Soe.Business.Util;

namespace SoftOne.Soe.Web.modalforms
{
    public partial class RegAddress : PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ((ModalFormMaster)Master).HeaderText = GetText(3386, "Välj adresstyp");
            ((ModalFormMaster)Master).Action = Url;

            AddressType.DataSource = GetGrpText((int)TermGroup.SysContactAddressType, false, false);
            AddressType.DataTextField = "value";
            AddressType.DataValueField = "key";
            AddressType.DataBind();

            if (F.Count > 0)
                Response.Redirect(Request.UrlReferrer.ToString());
        }
    }
}
