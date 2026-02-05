using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.modalforms
{
    public partial class RegContact : PageBase
	{
		protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            ((ModalFormMaster)Master).HeaderText = GetText(1313, "Ny kontakt");
			((ModalFormMaster)Master).Action = "/modalforms/RegContact.aspx";

            #endregion

            #region Populate

            Type.ConnectDataSource(GetGrpText(TermGroup.SysContactEComType));

            #endregion

            if (F.Count > 0)
			{
				Response.Redirect(Request.UrlReferrer.ToString());
			}
		}
	}
}
