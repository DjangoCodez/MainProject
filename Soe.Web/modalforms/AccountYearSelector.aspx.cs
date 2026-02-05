using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Web.modalforms
{
    public partial class AccountYearSelector : PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            AccountManager am = new AccountManager(ParameterObject);

            ((ModalFormMaster)Master).HeaderText = GetText(5465, "Byt redovisningsår");
            ((ModalFormMaster)Master).Action = Url;

            string module = "";
            string urlReferrer = Request.UrlReferrer.AbsolutePath;
            if (urlReferrer.StartsWith("/soe/"))
            {
                string[] pathParts = urlReferrer.Split('/');
                if (pathParts.Count() > 2)
                    module = pathParts[2];
            }

            if (module == Constants.SOE_MODULE_ECONOMY || module == Constants.SOE_MODULE_BILLING)
            {
                //Get AccountYears (include Open, Closed and Locked)
                Dictionary<int, string> accountYears = am.GetAccountYearsDict(SoeCompany.ActorCompanyId, false, false, true, false);
                AccountYear.ConnectDataSource(accountYears);

                if (accountYears.Count > 0)
                {
                    if (CurrentAccountYear != null)
                        AccountYear.Value = CurrentAccountYear.AccountYearId.ToString();

                    Message.LabelSetting = GetText(5468, "Efter byte av redovisningsår skickas du till startsidan för");
                    if (module == Constants.SOE_MODULE_ECONOMY)
                        Message.LabelSetting += " " + GetText(5473, "Ekonomi");
                    else if (module == Constants.SOE_MODULE_BILLING)
                        Message.LabelSetting += " " + GetText(5469, "Försäljning");
                    Message.Visible = true;
                }
                else
                {
                    Message.LabelSetting = GetText(1752, "Inga år upplagda");
                    Message.Visible = true;
                }
            }
            else
            {
                Message.LabelSetting = GetText(1755, "År kan bara bytas från Ekonomi och Försäljning");
                Message.Visible = true;
            }

            if (F.Count > 0)
            {
                int accountYearId = StringUtility.GetInt(F["AccountYear"], 0);
                if (accountYearId > 0)
                {
                    Response.Redirect("/setaccountyear.aspx/?accountYear=" + accountYearId + "&module=" + module);
                }
            }
        }
    }
}