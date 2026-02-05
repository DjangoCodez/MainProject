using System;
using System.Globalization;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;

namespace SoftOne.Soe.Web.soe.billing.preferences.deliverytype.edit
{
    public partial class _default : PageBase
    {
        #region Variables

        protected InvoiceManager im;

        protected DeliveryType type;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Billing_Preferences_DeliveryType_Edit;
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
            int typeId;
            if (Int32.TryParse(QS["type"], out typeId))
            {
                if (Mode == SoeFormMode.Prev || Mode == SoeFormMode.Next)
                {
                    type = im.GetPrevNextDeliveryType(typeId, SoeCompany.ActorCompanyId, Mode);
                    ClearSoeFormObject();
                    if (type != null)
                        Response.Redirect(Request.Url.AbsolutePath + "?type=" + type.DeliveryTypeId);
                    else
                        Response.Redirect(Request.Url.AbsolutePath + "?type=" + typeId);
                }
                else
                {
                    type = im.GetDeliveryType(typeId);
                    if (type == null)
                    {
                        Form1.MessageWarning = GetText(3245, "Leveranssätt hittades inte");
                        Mode = SoeFormMode.Register;
                    }
                }
            }

            // Mode
            string editModeTabHeaderText = GetText(3236, "Redigera leveranssätt");
            string registerModeTabHeaderText = GetText(3235, "Registrera leveranssätt");
            PostOptionalParameterCheck(Form1, type, true, editModeTabHeaderText, registerModeTabHeaderText);

            Form1.Title = type != null ? type.Name : "";

            #endregion

            #region Actions

            if (Form1.IsPosted)
            {
                Save();
            }

            #endregion

            #region Set data

            if (type != null)
            {
                Code.Value = type.Code;
                Name.Value = type.Name;
            }

            #endregion

            #region MessageFromSelf

            if (!String.IsNullOrEmpty(MessageFromSelf))
            {
                if (MessageFromSelf == "SAVED")
                    Form1.MessageSuccess = GetText(3246, "Leveranssätt sparat");
                else if (MessageFromSelf == "NOTSAVED")
                    Form1.MessageError = GetText(3247, "Leveranssätt kunde inte sparas");
                else if (MessageFromSelf == "UPDATED")
                    Form1.MessageSuccess = GetText(3248, "Leveranssätt uppdaterat");
                else if (MessageFromSelf == "NOTUPDATED")
                    Form1.MessageError = GetText(3249, "Leveranssätt kunde inte uppdateras");
                else if (MessageFromSelf == "EXIST")
                    Form1.MessageInformation = GetText(3250, "Leveranssätt finns redan");
                else if (MessageFromSelf == "FAILED")
                    Form1.MessageError = GetText(3251, "Leveranssätt kunde inte sparas");
                else if (MessageFromSelf == "DELETED")
                    Form1.MessageSuccess = GetText(1964, "Leveranssätt borttaget");
                else if (MessageFromSelf == "NOTDELETED")
                    Form1.MessageError = GetText(3252, "Leveranssätt kunde inte tas bort");
            }

            #endregion

            #region Navigation

            if (type != null)
            {
                Form1.SetRegLink(GetText(3235, "Registrera leveranssätt"), "", 
                    Feature.Billing_Preferences_DeliveryType_Edit, Permission.Modify);
            }

            #endregion
        }

        #region Action-methods

        protected override void Save()
        {
            string code = F["Code"];
            string name = F["Name"];

            if (type == null)
            {
                // Validation: Type not already exist
                if (im.DeliveryTypeExists(code, SoeCompany.ActorCompanyId))
                    RedirectToSelf("EXIST", true);

                // Create Condition
                type = new DeliveryType()
                {
                    Code = code,
                    Name = name,
                };

                if (im.AddDeliveryType(type, SoeCompany.ActorCompanyId).Success)
                    RedirectToSelf("SAVED");
                else
                    RedirectToSelf("NOTSAVED", true);
            }
            else
            {
                if (type.Code != code)
                {
                    // Validation: Type not already exist
                    if (im.DeliveryTypeExists(code, SoeCompany.ActorCompanyId))
                        RedirectToSelf("EXIST", true);
                }

                // Update Type
                type.Code = code;
                type.Name = name;

                if (im.UpdateDeliveryType(type).Success)
                    RedirectToSelf("UPDATED");
                else
                    RedirectToSelf("NOTUPDATED", true);
            }

            RedirectToSelf("FAILED", true);
        }

        protected override void Delete()
        {
            if (im.DeleteDeliveryType(type).Success)
                RedirectToSelf("DELETED", false, true);
            else
                RedirectToSelf("NOTDELETED", true);
        }

        #endregion
    }
}
