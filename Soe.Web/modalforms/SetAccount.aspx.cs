using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.modalforms
{
    public partial class SetAccount : PageBase
    {
        protected double diff;
        protected string SelectAnAccountText { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            ((ModalFormMaster)Master).HeaderText = GetText(9017, "Ange konto");
            ((ModalFormMaster)Master).showSubmitButton = true;
            ((ModalFormMaster)Master).showActionButton = false;
            ((ModalFormMaster)Master).FormMethod = string.Empty;
            ((ModalFormMaster)Master).ContentStyle = "width: 400px";
            //Terms
            SelectAnAccountText = GetText(9018, "Ange ett konto där differansen ska placeras:");

            this.Scripts.Add("/soe/economy/accounting/balance/balancechange/OpenModalDialog.js");
            Scripts.Add("/soe/economy/accounting/balance/Accounts.js.aspx?c=" + SoeCompany.ActorCompanyId + "&amp;dim=" + 0);
            diff = Convert.ToDouble(Session[Constants.SESSION_SETACCOUNT_DIFF]);
            AmountDiff.Value = Math.Abs(diff).ToString("N2");
        }
    }
}