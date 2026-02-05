using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.time.import.payrollimport
{
    public partial class _default : PageBase
    {
        #region Variables

        protected bool isAdmin;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Import_PayrollImport;
            base.Page_Init(sender, e);
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            isAdmin = base.IsAdmin;

            #endregion
        }
    }
}
