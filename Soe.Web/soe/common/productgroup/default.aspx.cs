using System;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.common.productgroup
{
    public partial class _default : PageBase
    {
        #region Variables

        //Module specifics
        public bool EnableBilling { get; set; }

        protected ProductGroup productGroup;

        protected ProductGroupManager pm;

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
                    case Feature.Billing_Preferences_ProductSettings_ProductGroup_Edit:
                        EnableBilling = true;
                        break;
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            pm = new ProductGroupManager(ParameterObject);

            //Mandatory parameters

            //Mode 
            PreOptionalParameterCheck(Request.Url.AbsolutePath, Request.Url.PathAndQuery);

            //Optional parameters
            int productGroupId;
            if (Int32.TryParse(QS["productgroup"], out productGroupId))
            {
                if (Mode == SoeFormMode.Prev || Mode == SoeFormMode.Next)
                {
                    productGroup = pm.GetPrevNextProductGroup(productGroupId, SoeCompany.ActorCompanyId, Mode);
                    ClearSoeFormObject();
                    if (productGroup != null)
                        Response.Redirect(Request.Url.AbsolutePath + "?productgroup=" + productGroup.ProductGroupId);
                    else
                        Response.Redirect(Request.Url.AbsolutePath + "?productgroup=" + productGroupId);
                }
                else
                {
                    productGroup = pm.GetProductGroup(SoeCompany.ActorCompanyId, productGroupId);
                    if (productGroup == null)
                    {
                        Form1.MessageWarning = GetText(4245, "Produktgrupp hittades inte");
                        Mode = SoeFormMode.Register;
                    }
                }
            }

            //Mode
            string editModeTabHeaderText = GetText(4246, "Redigera produktgrupp");
            string registerModeTabHeaderText = GetText(4247, "Registrera produktgrupp");
            PostOptionalParameterCheck(Form1, productGroup, true, editModeTabHeaderText, registerModeTabHeaderText);

            Form1.Title = productGroup != null ? productGroup.Name : "";

            #endregion

            #region Actions

            if (Form1.IsPosted)
            {
                Save();
                RedirectToSelf("SAVED");
            }

            #endregion

            #region Set data

            if (productGroup != null)
            {
                Code.Value = productGroup.Code;
                Name.Value = productGroup.Name;
            }

            #endregion

            #region MessageFromSelf

            if (!String.IsNullOrEmpty(MessageFromSelf))
            {
                if (MessageFromSelf == "SAVED")
                    Form1.MessageSuccess = GetText(4248, "Produktgrupp sparad");
                else if (MessageFromSelf == "NOTSAVED")
                    Form1.MessageError = GetText(4249, "Produktgrupp kunde inte sparas");
                else if (MessageFromSelf == "UPDATED")
                    Form1.MessageSuccess = GetText(4250, "Produktgrupp uppdaterad");
                else if (MessageFromSelf == "NOTUPDATED")
                    Form1.MessageError = GetText(4251, "Produktgrupp kunde inte uppdateras");
                else if (MessageFromSelf == "EXIST")
                    Form1.MessageInformation = GetText(4252, "Produktgrupp finns redan");
                else if (MessageFromSelf == "FAILED")
                    Form1.MessageError = GetText(4253, "Produktgrupp kunde inte sparas");
                else if (MessageFromSelf == "DELETED")
                    Form1.MessageSuccess = GetText(1978, "Produktgrupp borttaget");
                else if (MessageFromSelf == "NOTDELETED")
                    Form1.MessageError = GetText(4254, "Produktgrupp kunde inte tas bort");
            }

            #endregion
        }

        #region Action-methods

        protected override void Save()
        {
            string code = F["Code"];
            string name = F["Name"];

            if (productGroup == null)
            {
                productGroup = new ProductGroup()
                {
                    Code = code,
                    Name = name,
                };

                if (pm.AddProductGroup(SoeCompany.ActorCompanyId, productGroup).Success)
                    RedirectToSelf("SAVED");
                else
                    RedirectToSelf("NOTSAVED", true);
            }
            else
            {
                productGroup.Code = code;
                productGroup.Name = name;

                if (pm.UpdateProductGroup(productGroup, SoeCompany.ActorCompanyId).Success)
                    RedirectToSelf("UPDATED");
                else
                    RedirectToSelf("NOTUPDATED", true);
            }

            RedirectToSelf("FAILED", true);
        }

        protected override void Delete()
        {
            if (pm.DeleteProductGroup(productGroup, SoeCompany.ActorCompanyId).Success)
                RedirectToSelf("DELETED", false, true);
            else
                RedirectToSelf("NOTDELETED", true);
        }

        #endregion
    }
}
