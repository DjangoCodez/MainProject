using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;

namespace SoftOne.Soe.Web.soe.economy.accounting.voucherhistory
{
    public partial class _default : PageBase
    {
        private VoucherManager vm = null;
        private List<VoucherRowHistory> voucherRowHistory;

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Economy_Accounting_VoucherHistory;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region init

            vm = new VoucherManager(ParameterObject);

            //Mandatory parameters

            //Mode 
            PreOptionalParameterCheck(Request.Url.AbsolutePath, Request.Url.PathAndQuery);

            //Optional parameters

            //Mode
            PostOptionalParameterCheck(Form1, null, true);

            voucherRowHistory = new List<VoucherRowHistory>();

            //Set UserControl parameters
            SelectionStd.SoeForm = Form1;
            SelectionVoucher.SoeForm = Form1;
            SelectionAccount.SoeForm = Form1;
            SelectionUser.SoeForm = Form1;

            SortField.ConnectDataSource(GetGrpText(TermGroup.VoucherRowHistorySortField));
            SortOrder.ConnectDataSource(GetGrpText(TermGroup.VoucherRowHistorySortOrder));

            //Default values
            if (Mode != SoeFormMode.Repopulate)
            {
                SortField.Value = Convert.ToString((int)TermGroup_VoucherRowHistorySortField.VoucherNr);
                SortOrder.Value = Convert.ToString((int)TermGroup_VoucherRowHistorySortOrder.Ascending);
            }

            SoeGrid1.Title = GetText(5425, "Historik");

            #endregion

            #region Actions

            if (Form1.IsPosted)
            {
                Evaluate();

                //Always keep data in Form if Form is posted
                SelectionStd.SoeForm.PreviousForm = F;
                SelectionVoucher.SoeForm.PreviousForm = F;
                SelectionUser.SoeForm.PreviousForm = F;

                SortField.Value = F["SortField"];
                SortOrder.Value = F["SortOrder"];
            }
            else
            {
                if (Int32.TryParse(QS["voucher"], out int voucherHeadId))
                {
                    voucherRowHistory = vm.GetVoucherRowHistoryFromVoucher(SoeCompany.ActorCompanyId, voucherHeadId).ToList();
                }
            }

            #endregion

            #region Populate

            SelectionStd.Populate(true);
            SelectionStd.GetSelectedAccountYearId(true, out int accountYearIdFrom, out int accountYearIdTo);
            SelectionVoucher.Populate(true, accountYearIdFrom, accountYearIdTo);
            SelectionAccount.Populate(true);
            SelectionUser.Populate(true);

            if (voucherRowHistory.IsNullOrEmpty())
            {
                SoeGrid1.Visible = false;
                if (Form1.IsPosted)
                    Form1.MessageInformation = GetText(1343, "Urvalet gav ingen data");
            }

            SoeGrid1.DataSource = voucherRowHistory;
            SoeGrid1.RowDataBound += SoeGrid1_RowDataBound;
            SoeGrid1.DataBind();

            #endregion

            #region MessageFromSelf

            if (!String.IsNullOrEmpty(MessageFromSelf))
            {
                if (MessageFromSelf == "EVALUATE_FAILED")
                    Form1.MessageWarning = GetText(1450, "Felaktigt urval");
            }

            #endregion
        }

        private void Evaluate()
        {
            bool evaluated = false;

            Selection s = new Selection(SoeCompany.ActorCompanyId, UserId, RoleId, SoeUser.LoginName);

            //SelectionStd
            SelectionStd.F = Request.Form;
            s.SelectionStd = new SelectionStd();
            if (SelectionStd.Evaluate(s.SelectionStd, s.Evaluated))
            {
                //SelectionVoucher
                SelectionVoucher.F = Request.Form;
                s.SelectionVoucher = new SelectionVoucher();
                if (SelectionVoucher.Evaluate(s.SelectionVoucher, s.Evaluated))
                {
                    //SelectionAccount
                    SelectionAccount.F = Request.Form;
                    s.SelectionAccount = new SelectionAccount();
                    if (SelectionAccount.Evaluate(s.SelectionAccount, s.Evaluated))
                    {
                        //SelectionUser
                        s.SelectionUser = new SelectionUser();
                        SelectionUser.F = Request.Form;
                        evaluated = SelectionUser.Evaluate(s.SelectionUser, s.Evaluated);
                    }
                }
            }

            if (evaluated)
            {
                if (Int32.TryParse(F["SortField"], out int sortField) && Int32.TryParse(F["SortOrder"], out int sortOrder))
                {
                    voucherRowHistory = vm.GetVoucherRowHistoryFromSelection(s.Evaluated, (TermGroup_VoucherRowHistorySortField)sortField, (TermGroup_VoucherRowHistorySortOrder)sortOrder, Constants.VOUCHERROWHISTORY_MAXROWS, out int rowsFromDb);

                    if (rowsFromDb > Constants.VOUCHERROWHISTORY_MAXROWS)
                    {
                        Form1.MessageInformation =
                            GetText(1645, "Urvalet genererade") + " " + rowsFromDb + " " +
                            GetText(1646, "rader från behandlingshistoriken") + ". " +
                            GetText(1647, "Första") + " " + Constants.VOUCHERROWHISTORY_MAXROWS + " " + GetText(1648, "raderna visas");
                    }
                }
            }
            else
            {
                RedirectToSelf("EVALUATE_FAILED", true);
            }
        }

        private void SoeGrid1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            VoucherRowHistory voucherRowHistory = ((e.Row.DataItem) as VoucherRowHistory);
            if (voucherRowHistory != null)
            {
                e.Row.VerticalAlign = VerticalAlign.Top;
                PlaceHolder phEventText = (PlaceHolder)e.Row.FindControl("phEventText");
                if (phEventText != null && !String.IsNullOrEmpty(voucherRowHistory.EventText))
                {
                    Label lbl = new Label();

                    string[] eventText = voucherRowHistory.EventText.Split(Constants.VOUCHERROWHISTORY_EVENTTEXT_DELIMETER);
                    for (int i = 0; i < eventText.Length; i++)
                    {
                        string text = eventText[i];
                        if (String.IsNullOrEmpty(text))
                            continue;

                        if (String.IsNullOrEmpty(lbl.Text))
                            lbl.Text = text;
                        else
                            lbl.Text += HttpUtility.HtmlDecode("<br>" + text);
                    }

                    phEventText.Controls.Add(lbl);
                }
            }
        }
    }
}
