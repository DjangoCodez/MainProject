using System;
using System.Globalization;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;

namespace SoftOne.Soe.Web.soe.billing.preferences.deliverycondition.edit
{
    public partial class _default : PageBase
    {
        #region Variables

        protected InvoiceManager im;

        protected DeliveryCondition condition;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Billing_Preferences_DeliveryCondition_Edit;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            im = new InvoiceManager(ParameterObject);

            //Mandatory parameters

            // Mode 
            PreOptionalParameterCheck(Request.Url.AbsolutePath, Request.Url.PathAndQuery);

            // Optional parameters
            int conditionId;
            if (Int32.TryParse(QS["condition"], out conditionId))
            {
                if (Mode == SoeFormMode.Prev || Mode == SoeFormMode.Next)
                {
                    condition = im.GetPrevNextDeliveryCondition(conditionId, SoeCompany.ActorCompanyId, Mode);
                    ClearSoeFormObject();
                    if (condition != null)
                        Response.Redirect(Request.Url.AbsolutePath + "?condition=" + condition.DeliveryConditionId);
                    else
                        Response.Redirect(Request.Url.AbsolutePath + "?condition=" + conditionId);
                }
                else
                {
                    condition = im.GetDeliveryCondition(conditionId);
                    if (condition == null)
                    {
                        Form1.MessageWarning = GetText(3237, "Leveransvillkor hittades inte");
                        Mode = SoeFormMode.Register;
                    }
                }
            }

            // Mode
            string editModeTabHeaderText = GetText(3233, "Redigera leveransvillkor");
            string registerModeTabHeaderText = GetText(3232, "Registrera leveransvillkor");
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
            }

            #endregion

            #region MessageFromSelf

            if (!String.IsNullOrEmpty(MessageFromSelf))
            {
                if (MessageFromSelf == "SAVED")
                    Form1.MessageSuccess = GetText(3244, "Leveransvillkor sparat");
                else if (MessageFromSelf == "NOTSAVED")
                    Form1.MessageError = GetText(3238, "Leveransvillkor kunde inte sparas");
                else if (MessageFromSelf == "UPDATED")
                    Form1.MessageSuccess = GetText(3239, "Leveransvillkor uppdaterat");
                else if (MessageFromSelf == "NOTUPDATED")
                    Form1.MessageError = GetText(3240, "Leveransvillkor kunde inte uppdateras");
                else if (MessageFromSelf == "EXIST")
                    Form1.MessageInformation = GetText(3241, "Leveransvillkor finns redan");
                else if (MessageFromSelf == "FAILED")
                    Form1.MessageError = GetText(3242, "Leveransvillkor kunde inte sparas");
                else if (MessageFromSelf == "DELETED")
                    Form1.MessageSuccess = GetText(1963, "Leveransvillkor borttaget");
                else if (MessageFromSelf == "NOTDELETED")
                    Form1.MessageError = GetText(3243, "Leveransvillkor kunde inte tas bort");
            }

            #endregion

            #region Navigation

            if (condition != null)
            {
                Form1.SetRegLink(GetText(3232, "Registrera leveransvillkor"), "", 
                    Feature.Billing_Preferences_DeliveryCondition_Edit, Permission.Modify);
            }

            #endregion
        }

        #region Action-methods

        protected override void Save()
        {
            string code = F["Code"];
            string name = F["Name"];

            if (condition == null)
            {
                // Validation: Condition not already exist
                if (im.DeliveryConditionExists(code, SoeCompany.ActorCompanyId))
                    RedirectToSelf("EXIST", true);

                // Create Condition
                condition = new DeliveryCondition()
                {
                    Code = code,
                    Name = name,
                };

                if (im.AddDeliveryCondition(condition, SoeCompany.ActorCompanyId).Success)
                    RedirectToSelf("SAVED");
                else
                    RedirectToSelf("NOTSAVED", true);
            }
            else
            {
                if (condition.Code != code)
                {
                    // Validation: Condition not already exist
                    if (im.DeliveryConditionExists(code, SoeCompany.ActorCompanyId))
                        RedirectToSelf("EXIST", true);
                }

                // Update Condition
                condition.Code = code;
                condition.Name = name;

                if (im.UpdateDeliveryCondition(condition).Success)
                    RedirectToSelf("UPDATED");
                else
                    RedirectToSelf("NOTUPDATED", true);
            }

            RedirectToSelf("FAILED", true);
        }

        protected override void Delete()
        {
            if (im.DeleteDeliveryCondition(condition).Success)
                RedirectToSelf("DELETED", false, true);
            else
                RedirectToSelf("NOTDELETED", true);
        }

        #endregion
    }
}
