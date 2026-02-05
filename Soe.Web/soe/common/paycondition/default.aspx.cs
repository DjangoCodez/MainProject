using System;
using System.Globalization;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.common.paycondition
{
    public partial class _default : PageBase
    {
        #region Variables

        protected PaymentManager pm;

        protected PaymentCondition condition;

        //Module specifics
        public bool EnableEconomy { get; set; }
        public bool EnableBilling { get; set; }
        private Feature FeatureEdit = Feature.None;
        private Int32 ddays = 0;
        private Int32 dpercent = 0;


        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            //Set variables to reuse page with different content
            EnableModuleSpecifics();

            base.Page_Init(sender, e);

            // Add scripts and style sheets
        }

        private void EnableModuleSpecifics()
        {
            if (CTX["Feature"] != null)
            {
                this.Feature = (Feature)CTX["Feature"];
                switch (this.Feature)
                {
                    case Feature.Economy_Preferences_PayCondition_Edit:
                        EnableEconomy = true;
                        FeatureEdit = Feature.Economy_Preferences_PayCondition_Edit;
                        break;
                    case Feature.Billing_Preferences_PayCondition_Edit:
                        EnableBilling = true;
                        FeatureEdit = Feature.Billing_Preferences_PayCondition_Edit;
                        break;
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            pm = new PaymentManager(ParameterObject);

            //Mandatory parameters

            // Mode 
            PreOptionalParameterCheck(Request.Url.AbsolutePath, Request.Url.PathAndQuery);

            // Optional parameters
            int conditionId;
            if (Int32.TryParse(QS["condition"], out conditionId))
            {
                if (Mode == SoeFormMode.Prev || Mode == SoeFormMode.Next)
                {
                    condition = pm.GetPrevNextPaymentCondition(conditionId, SoeCompany.ActorCompanyId, Mode);
                    ClearSoeFormObject();
                    if (condition != null)
                        Response.Redirect(Request.Url.AbsolutePath + "?condition=" + condition.PaymentConditionId);
                    else
                        Response.Redirect(Request.Url.AbsolutePath + "?condition=" + conditionId);
                }
                else
                {
                    condition = pm.GetPaymentCondition(conditionId, SoeCompany.ActorCompanyId);
                    if (condition == null)
                    {
                        Form1.MessageWarning = GetText(3089, "Betalningsvillkor hittades inte");
                        Mode = SoeFormMode.Register;
                    }
                }
            }

            // Mode
            string editModeTabHeaderText = GetText(3091, "Redigera betalningsvillkor");
            string registerModeTabHeaderText = GetText(3090, "Registrera betalningsvillkor");
            PostOptionalParameterCheck(Form1, condition, true, editModeTabHeaderText, registerModeTabHeaderText);

            Form1.Title = condition != null ? condition.Name : "";

            #endregion

            #region Actions

            if (Form1.IsPosted)
            {
                Save();
            }

            #endregion

            #region Set data

            if (condition != null)
            {
                Code.Value = condition.Code;
                Name.Value = condition.Name;
                Days.Value = condition.Days.ToString(CultureInfo.CurrentCulture);
                DiscountDays.Value = condition.DiscountDays.ToString();
                DiscountPercent.Value = condition.DiscountPercent.ToString();
            }

            #endregion

            #region MessageFromSelf

            if (!String.IsNullOrEmpty(MessageFromSelf))
            {
                if (MessageFromSelf == "SAVED")
                    Form1.MessageSuccess = GetText(3092, "Betalningsvillkor sparat");
                else if (MessageFromSelf == "NOTSAVED")
                    Form1.MessageError = GetText(3093, "Betalningsvillkor kunde inte sparas");
                else if (MessageFromSelf == "UPDATED")
                    Form1.MessageSuccess = GetText(3094, "Betalningsvillkor uppdaterat");
                else if (MessageFromSelf == "NOTUPDATED")
                    Form1.MessageError = GetText(5733, "Betalningsvillkor kunde inte uppdateras");
                else if (MessageFromSelf == "EXIST")
                    Form1.MessageInformation = GetText(3096, "Betalningsvillkor finns redan");
                else if (MessageFromSelf == "FAILED")
                    Form1.MessageError = GetText(3097, "Betalningsvillkor kunde inte sparas");
                else if (MessageFromSelf == "DELETED")
                    Form1.MessageSuccess = GetText(1976, "Betalningsvillkor borttaget");
                else if (MessageFromSelf == "NOTDELETED")
                    Form1.MessageError = GetText(5461, "Betalningsvillkor kunde inte tas bort");
                else
                    Form1.MessageError = MessageFromSelf;
            }

            #endregion

            #region Navigation

            if (condition != null)
            {
                Form1.SetRegLink(GetText(3090, "Registrera betalningsvillkor"), "",
                    FeatureEdit, Permission.Modify);
            }

            #endregion
        }

        #region Action-methods

        protected override void Save()
        {
            string code = F["Code"];
            string name = F["Name"];
            string days = F["Days"];
            string discountDays = F["DiscountDays"];
            string discountPercent = F["DiscountPercent"];

            if (condition == null)
            {
                // Validation: Condition not already exist
                if (pm.PaymentConditionExists(code, SoeCompany.ActorCompanyId))
                    RedirectToSelf("EXIST", true);

                // Create Condition
                condition = new PaymentCondition()
                {
                    Code = code,
                    Name = name,
                    Days = Int32.Parse(days),

                };
                if (Int32.TryParse(discountDays, out ddays))
                  condition.DiscountDays = ddays;
                if (Int32.TryParse(discountPercent, out dpercent))
                  condition.DiscountPercent = dpercent;

                if (pm.AddPaymentCondition(condition, SoeCompany.ActorCompanyId).Success)
                    RedirectToSelf("SAVED");
                else
                    RedirectToSelf("NOTSAVED", true);
            }
            else
            {
                if (condition.Code != code)
                {
                    // Validation: Condition not already exist
                    if (pm.PaymentConditionExists(code, SoeCompany.ActorCompanyId))
                        RedirectToSelf("EXIST", true);
                }

                // Update Condition
                condition.Code = code;
                condition.Name = name;
                condition.Days = Int32.Parse(days);
                if (Int32.TryParse(discountDays, out ddays))
                  condition.DiscountDays = ddays;
                if (Int32.TryParse(discountPercent, out dpercent))
                  condition.DiscountPercent = dpercent;

                if (pm.UpdatePaymentCondition(condition, SoeCompany.ActorCompanyId).Success)
                    RedirectToSelf("UPDATED");
                else
                    RedirectToSelf("NOTUPDATED", true);
            }

            RedirectToSelf("FAILED", true);
        }

        protected override void Delete()
        {
            if (pm.DeletePaymentCondition(condition, SoeCompany.ActorCompanyId).Success)
                RedirectToSelf("DELETED", false, true);
            else
                RedirectToSelf("NOTDELETED", true);
        }

        #endregion
    }
}
