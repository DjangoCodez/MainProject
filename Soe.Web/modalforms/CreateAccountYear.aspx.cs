using SoftOne.Soe.Business.Core;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Web.modalforms
{
    public partial class CreateAccountYear : PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            AccountManager am = new AccountManager(ParameterObject);

            int nbrOfOpenAccountYears = am.GetNumberOfOpenAccountYears(SoeCompany.ActorCompanyId);

            if (nbrOfOpenAccountYears >= 3)
            {
                ((ModalFormMaster)Master).HeaderText = GetText(5465, "Byt redovisningsår");
                ((ModalFormMaster)Master).InfoText = GetText(7282, "Redovisningsår kan inte skapas automatiskt. Max antal öppna är tre.");
                ((ModalFormMaster)Master).Action = Url;

                if (F.Count > 0)
                {
                    Response.Redirect(Request.UrlReferrer.ToString());
                }
            }
            else
            {
                ((ModalFormMaster)Master).HeaderText = GetText(5465, "Byt redovisningsår");
                ((ModalFormMaster)Master).InfoText = GetText(7491, "Inget redovisningsår finns för nuvarande datum. Vill du skapa ett nytt år?");
                ((ModalFormMaster)Master).SubmitButtonText = GetText(52, "Ja");
                ((ModalFormMaster)Master).CancelButtonText = GetText(53, "Nej");
                ((ModalFormMaster)Master).Action = Url;

                int currentAccountYearId = 0;

                //Get AccountYears (include Open, Closed and Locked)
                Dictionary<int, string> accountYears = am.GetAccountYearsDict(SoeCompany.ActorCompanyId, false, false, true, false);
                if (accountYears.Count > 0)
                {
                    if (CurrentAccountYear != null)
                        currentAccountYearId = CurrentAccountYear.AccountYearId;
                }
                else
                {
                    Message.LabelSetting = GetText(1752, "Inga år upplagda");
                    Message.Visible = true;
                }

                if (F.Count > 0)
                {
                    Response.Redirect("/soe/economy/accounting/yearend/?createyear=true");
                }
            }
        }
    }
}