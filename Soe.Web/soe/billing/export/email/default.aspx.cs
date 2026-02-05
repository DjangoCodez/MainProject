using System;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System.Linq;
using SoftOne.Soe.Data;

namespace SoftOne.Soe.Web.soe.billing.export.email
{
    public partial class _default : PageBase
    {
        private ContactManager ctm;
        private EmailManager em;

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Billing_Export_Email;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init
           
            ctm = new ContactManager(ParameterObject);
            em = new EmailManager(ParameterObject);

            Form1.Title = GetText(5444, "Exportera fakturor via e-post");

            #endregion

            #region Save

            if (Form1.IsPosted)
                Save();

            #endregion

            #region Populate

            Template.ConnectDataSource(em.GetEmailTemplatesDic(SoeCompany.ActorCompanyId, false));

            #endregion

            #region Set data

            string emailAdress = "";
            var contact = ctm.GetContactFromActor(SoeCompany.ActorCompanyId);
            if (contact != null)
            {
                var emailAddress = ctm.GetContactECom(contact.ContactId, (int)TermGroup_SysContactEComType.Email, false);
                if (emailAddress != null)
                    emailAdress = emailAddress.Text;
            }

            if (!String.IsNullOrEmpty(emailAdress))
            {
                FromAddress.LabelSetting = emailAdress;
            }
            else
            {
                Form1.DisableSave = true;
                Form1.MessageInformation = GetText(4137, "Företaget som skickar saknar e-postadress");
                FromAddress.LabelSetting = GetText(4137, "Företaget som skickar saknar e-postadress");
            }

            #endregion

            #region MessageFromSelf

            if (!String.IsNullOrEmpty(MessageFromSelf))
            {
                if (MessageFromSelf == "MANDATORY_REPORT_MISSING")
                    Form1.MessageWarning = GetText(4209, "Standardrapport för faktura saknas");
            }

            #endregion
        }

        #region Action-methods

        protected override void Save()
        {
            ReportManager rm = new ReportManager(ParameterObject);

            Report report = rm.GetSettingOrStandardReport(SettingMainType.Company, CompanySettingType.BillingDefaultInvoiceTemplate, SoeReportTemplateType.BillingInvoice, SoeReportType.CrystalReport, UserId, SoeCompany.ActorCompanyId, RoleId);
            if (report != null)
            {
                int emailtemplateId = Convert.ToInt32(F["template"]);
                int emailReportId = report.ReportId;
                string url = String.Format("/soe/billing/invoice/status/?emailtemplateId={0}&emailReportId={1}&classificationgroup={2}", emailtemplateId, emailReportId, (int)SoeOriginStatusClassificationGroup.HandleCustomerInvoices);

                Response.Redirect(url, true);
            }
            else
            {
                RedirectToSelf("MANDATORY_REPORT_MISSING", true);
            }
        }

        #endregion
    }
}
