using System;
using System.Web.UI.HtmlControls;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Business.Util;

namespace SoftOne.Soe.Web.soe.common.emailtemplate
{
    public partial class _default : PageBase
    {
        #region Variables

        protected EmailManager em;
        protected EmailTemplate emailTemplate;
        protected int type;

        //Module specifics
        protected bool EnableEconomy { get; set; }
        protected bool EnableBilling { get; set; }
        protected bool EnableManage { get; set; }
        protected string BodyText { get; set; }
        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            //Set variables to reuse page with different contet
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
                    case Feature.Billing_Preferences_EmailTemplate_Edit:
                        EnableBilling = true;
                        break;
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            em = new EmailManager(ParameterObject);

            //Mode 
            PreOptionalParameterCheck(Request.Url.AbsolutePath, Request.Url.PathAndQuery);

            //Optional parameters
            int emailTemplateId;
            if (Int32.TryParse(QS["emailtemplate"], out emailTemplateId))
            {
                if (Mode == SoeFormMode.Prev || Mode == SoeFormMode.Next)
                {
                    emailTemplate = em.GetEmailTemplate(emailTemplateId, SoeCompany.ActorCompanyId);
                    ClearSoeFormObject();
                    if (emailTemplate != null)
                        Response.Redirect(Request.Url.AbsolutePath + "?emailtemplate=" + emailTemplateId);
                    else
                        Response.Redirect(Request.Url.AbsolutePath + "?emailtemplate=" + emailTemplateId);
                }
                else
                {
                    emailTemplate = em.GetEmailTemplate(emailTemplateId, SoeCompany.ActorCompanyId);
                    if (emailTemplate == null)
                    {
                        Form1.MessageWarning = GetText(4146, "E-postmall hittades inte");
                        Mode = SoeFormMode.Register;
                    }
                }
            }

            //Mode
            string editModeTabHeaderText = GetText(4147, "Redigera e-postmall");
            string registerModeTabHeaderText = GetText(4143, "Registrera e-postmall");
            PostOptionalParameterCheck(Form1, emailTemplate, true, editModeTabHeaderText, registerModeTabHeaderText);

            Form1.Title = emailTemplate != null ? emailTemplate.Name : "";

            BodyText = GetText(4145, "Meddelandets innehåll");

            #endregion

            #region Actions

            if (Form1.IsPosted)
            {
                Save();
            }

            #endregion

            #region Set data

            if (emailTemplate != null)
            {
                Name.Value = emailTemplate.Name;
                IsHTML.Value = emailTemplate.BodyIsHTML.ToString();
                Body.Value = emailTemplate.Body;
                Subject.Value = emailTemplate.Subject;

                var a = new HtmlAnchor();
                a.HRef = Request.Url.Scheme + @"://" + Request.Url.Authority + "/soe/common/emailtemplate/preview/?t=" + emailTemplate.EmailTemplateId + "&a=" + SoeCompany.ActorCompanyId;
                a.Attributes.Add("target", "_blank");
                a.InnerHtml = GetText(4158, "Förhandsgranska");
                preview.Controls.Add(a);
            }

            #endregion

            #region MessageFromSelf

            if (!String.IsNullOrEmpty(MessageFromSelf))
            {
                if (MessageFromSelf == "SAVED")
                    Form1.MessageSuccess = GetText(4148, "E-postmall sparad");
                else if (MessageFromSelf == "NOTSAVED")
                    Form1.MessageError = GetText(4149, "E-postmall kunde inte sparas");
                else if (MessageFromSelf == "UPDATED")
                    Form1.MessageSuccess = GetText(4150, "E-postmall uppdaterad");
                else if (MessageFromSelf == "NOTUPDATED")
                    Form1.MessageError = GetText(4151, "E-postmall kunde inte uppdateras");
                else if (MessageFromSelf == "EXIST")
                    Form1.MessageInformation = GetText(4152, "En e-post mall med samma namn finns redan");
                else if (MessageFromSelf == "FAILED")
                    Form1.MessageError = GetText(4149, "E-postmall kunde inte sparas");
                else if (MessageFromSelf == "DELETED")
                    Form1.MessageSuccess = GetText(1974, "E-postmall borttagen");
                else if (MessageFromSelf == "NOTDELETED")
                    Form1.MessageError = GetText(4153, "E-postmall kunde inte tas bort");
                else
                    Form1.MessageError = MessageFromSelf;
            }

            #endregion
        }

        #region Action-methods

        protected override void Save()
        {
            //ValidateForm();

            string name = F["Name"];
            string body = F["Body"];
            string subject = F["Subject"];

            bool isHTML = Convert.ToBoolean(F["IsHTML"]);

            if (string.IsNullOrEmpty(name))
                RedirectToSelf("NOTSAVED", true);

            if (emailTemplate == null)
            {
                emailTemplate = new EmailTemplate()
                {
                    Name = name,
                    BodyIsHTML = isHTML,
                    Body = body,
                    Subject = subject,
                };

                ActionResult result = em.AddEmailTemplate(emailTemplate, SoeCompany.ActorCompanyId);
                if (result.Success)
                    RedirectToSelf("SAVED");
                else if (result.ErrorNumber == (int)ActionResultSave.EmailTemplateExists)
                    RedirectToSelf("EXIST", true);
                else
                    RedirectToSelf("NOTSAVED", true);
            }
            else
            {
                emailTemplate.Name = name;
                emailTemplate.Body = body;
                emailTemplate.BodyIsHTML = isHTML;
                emailTemplate.Subject = subject;

                if (em.UpdateEmailTemplate(emailTemplate, SoeCompany.ActorCompanyId).Success)
                    RedirectToSelf("UPDATED");
                else
                    RedirectToSelf("NOTUPDATED", true);
            }
            RedirectToSelf("FAILED", true);
        }

        protected override void Delete()
        {
            if (em.DeleteEmailTemplate(emailTemplate, SoeCompany.ActorCompanyId).Success)
                RedirectToSelf("DELETED", false, true);
            else
                RedirectToSelf("NOTDELETED", true);
        }

        #endregion
    }
}
