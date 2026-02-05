using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Web.soe.manage.contracts
{
    public partial class _default : PageBase
    {
        #region Variables

        protected LicenseManager lm;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Manage_Contracts;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            lm = new LicenseManager(ParameterObject);

            #endregion

            //Get data
            List<License> licenses = lm.GetAllLicensesOnServer(setSysServerUrl: true);

            //Title
            SoeGrid1.Title = GetText(1055, "Licenser");

            //Hide certain licenses
            licenses = licenses.Where(i => i.LicenseNr != "140").ToList();

            //Bind
            SoeGrid1.DataSource = licenses;
			SoeGrid1.DataBind();

            #region Navigation

            SoeGrid1.AddRegLink(GetText(2019, "Registrera licens"), "edit/", 
				Feature.Manage_Contracts_Edit, Permission.Modify);

            #endregion
        }
    }
}
