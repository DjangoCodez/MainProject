using SoftOne.Communicator.Shared.DTO;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Communicator;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;

namespace SoftOne.Soe.Business.Core
{
    public class EmailManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static private readonly string[] BlockedFromDomains = { "gmail.com", "yahoo.com", "yahoo.se" };

        #endregion

        #region Ctor

        public EmailManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Email

        public ActionResult SendEmail(String from, String to, List<string> cc, String subject, String body, bool emailcontentIsHtml)
        {
            ActionResult result = new ActionResult();

            bool useCommunicator = true;

            if (!useCommunicator)
            {
                try
                {
                    Email mail = new Email(from, to, subject, body, emailcontentIsHtml, SettingManager.isTest(), cc.ToArray());
                    mail.Send();
                }
                catch (SmtpException smtpEx)
                {
                    base.LogError(smtpEx, this.log);
                    result.Exception = smtpEx;
                    result.ErrorMessage = GetText(8067, "Anslutning till e-postserver misslyckades");
                    return result;
                }
                catch (FormatException fex)
                {
                    base.LogError(fex, this.log);
                    result.Exception = fex;
                    result.ErrorMessage = GetText(8068, "Maillista innehöll epostadresser som inte var angivna på rätt format");
                    return result;
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.ErrorMessage = GetText(8069, "Postning misslyckades");
                    return result;
                }
            }
            else
            {
                try
                {
                    result = SendEmailViaCommunicator(from, to, cc, subject, body, emailcontentIsHtml, null);
                }
                catch (FormatException fex)
                {
                    base.LogError(fex, this.log);
                    result.Exception = fex;
                    result.ErrorMessage = GetText(8068, "Maillista innehöll epostadresser som inte var angivna på rätt format");
                    return result;
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.ErrorMessage = GetText(8069, "Postning misslyckades");
                    return result;
                }
            }
            return result;
        }

        public ActionResult SendEmailViaCommunicator(string from, string to, List<string> cc, string subject, string body, bool emailcontentIsHtml, List<MessageAttachmentDTO> attachments, Dictionary<string,string> customMailArgs = null)
        {
            if (SettingManager.SiteType == TermGroup_SysPageStatusSiteType.Test)
            {
                if (!to.ToLower().Contains("@softone."))
                {
                    LogInfo("SendEmailViaCommunicator only works on softone emailaddresses when SiteType is test");
                    return new ActionResult();
                }
                cc = cc.IsNullOrEmpty() ? new List<string>() : cc.Where(s => s.ToLower().Contains("@softone.")).ToList();
            }

            MailMessageDTO mailMessageDTO = CreateMailMessageDTO(from, to, cc, subject, body, emailcontentIsHtml, attachments);
            if (customMailArgs != null)
            {
                mailMessageDTO.CustomArgs = customMailArgs;
            }

            return CommunicatorConnector.SendMailMessageFireAndForget(mailMessageDTO);
        }

        private MailMessageDTO CreateMailMessageDTO(String from, String to, List<string> cc, String subject, String body, bool emailcontentIsHtml, List<MessageAttachmentDTO> attachments)
        {
            return new MailMessageDTO()
            {
                SenderEmail = from,
                SenderName = from,
                subject = subject,
                recievers = to.ObjToList(),
                cc = cc,
                body = body,
                EmailcontentIsHtml = emailcontentIsHtml,
                MessageAttachmentDTOs = attachments ?? new List<MessageAttachmentDTO>(),
            };
        }

        public ActionResult SendEmail(String from, String to, List<string> cc, String subject, String body, bool emailcontentIsHtml, bool convert, List<MessageAttachmentDTO> attachments)
        {
            ActionResult result = new ActionResult();

            try
            {
                Email mail = new Email(from, to, subject, body, emailcontentIsHtml, convert, SettingManager.isTest(), cc.ToArray());

                if (attachments != null)
                {
                    foreach (MessageAttachmentDTO dto in attachments)
                    {
                        mail.Attach(new List<byte[]>() { dto.Data }, dto.Name);
                    }
                }

                mail.Send();
            }
            catch (SmtpException smtpEx)
            {
                base.LogError(smtpEx, this.log);
                result.Exception = smtpEx;
                result.ErrorMessage = GetText(8067, "Anslutning till e-postserver misslyckades");
                return result;
            }
            catch (FormatException fex)
            {
                base.LogError(fex, this.log);
                result.Exception = fex;
                result.ErrorMessage = GetText(8068, "Maillista innehöll epostadresser som inte var angivna på rätt format");
                return result;
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result.Exception = ex;
                result.ErrorMessage = GetText(8069, "Postning misslyckades");
                return result;
            }

            return result;
        }

        /// <summary>
        /// Creates a mail for each customer and attatches the customers invoices to it and saves the email
        /// To work the smtp settings in web.config needs to be correct.
        /// </summary>
        /// <param name="actorCompanyId">The sending company</param>
        /// <param name="emailTemplateId">Email content template</param>
        /// <param name="customerIds">CustomerId and the attachements as byte arrays</param>
        /// <param name="emailContentIsHTML">Mail body formated to HTML or plain text</param>
        /// <returns></returns>
        public ActionResult SendEmailWithAttachment(int actorCompanyId, EmailTemplateDTO emailTemplate, List<int> recipents, List<KeyValuePair<string, byte[]>> attachments, List<string> additionalRecipients, Dictionary<string, string> customMailArgs)
        {
            ActionResult result = new ActionResult();
            var sentToMail = new List<string>();
            try
            {
                #region Prereq

                result.ErrorMessage = GetText(8065, "Postning genomförd");

                Company company = CompanyManager.GetCompany(actorCompanyId, loadActorAndContact: true);
                if (company == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));
                if (company.Actor == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "Actor");
                if (company.Actor.Contact == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "Contact");
                if (company.Actor.Contact.Count < 1)
                    return new ActionResult(false);


                #endregion

                #region From 

                string fromAddress = "";
                bool useDefaultEmail = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseDefaultEmailAddress, 0, actorCompanyId, 0);
                if (useDefaultEmail)
                {
                    fromAddress = SettingManager.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.DefaultEmailAddress, 0, actorCompanyId, 0);
                }

                if (string.IsNullOrEmpty(fromAddress))
                {
                    ContactECom from = ContactManager.GetContactECom(company.Actor.Contact.First().ContactId, (int)TermGroup_SysContactEComType.Email, false);
                    if (from == null)
                        return new ActionResult { Success = false, ErrorMessage = "Failed finding default Company Contact" };

                    fromAddress = from.Text;
                }

                if (string.IsNullOrEmpty(fromAddress))
                {
                    return new ActionResult { Success = false, ErrorMessage = GetText(4137, "Företaget som skickar saknar e-postadress") };
                }

                var emailValidation = IsValidFromAddress(fromAddress);
                if (!emailValidation.Success)
                {
                    return emailValidation;
                }

                bool useCommunicator = true;

                #endregion

                #region CC

                var cc = new List<string>();

                string copyAddress = SettingManager.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.BillingCCInvoiceMailAddress, 0, actorCompanyId, 0);
                if (!string.IsNullOrEmpty(copyAddress))
                {
                    cc.Add(copyAddress);
                }

                #endregion

                #region Send ecom

                foreach (int contactEComid in recipents)
                {
                    #region To

                    ContactECom to = ContactManager.GetContactECom(contactEComid, true);
                    if (to == null)
                        continue;

                    if (string.IsNullOrEmpty(to.Text))
                    {
                        result.ErrorMessage = GetText(8066, "Kunden måste ha en emailadress registrerad under kontaktuppgifter");
                        return result;
                    }

                    #endregion

                    if (!cc.IsNullOrEmpty())
                    {
                        //not ok for sendgrid to have To also in Cc
                        cc = cc.Where(s => s.ToLower() != to.Text.ToLower()).ToList();
                    }
                    result = SendEmailWithAttachment(useCommunicator, fromAddress, to.Text, emailTemplate, attachments, cc, ref sentToMail,customMailArgs);
                }

                #endregion

                #region Send Documents to EmailAddresses
                if (!additionalRecipients.IsNullOrEmpty())
                {
                    foreach (var recipient in additionalRecipients)
                    {
                        if (!string.IsNullOrEmpty(recipient))
                        {
                            if (!cc.IsNullOrEmpty())
                            {
                                cc = cc.Where(s => s.ToLower() != recipient.ToLower()).ToList();
                            }
                            result = SendEmailWithAttachment(useCommunicator, fromAddress, recipient, emailTemplate, attachments, cc, ref sentToMail, customMailArgs);
                        }
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                result.Exception = ex;
                result.ErrorMessage = ex.Message;
                return result;
            }

            if (sentToMail.Any())
            {
                result.StringValue += "[" + string.Join(",", sentToMail) + "]";
            }

            return result;
        }

        public ActionResult SendEmailWithAttachment(bool useCommunicator, string from, string to, EmailTemplateDTO emailTemplate, List<KeyValuePair<string, byte[]>> attachments, List<string> cc, ref List<string> sentToMail, Dictionary<string, string> customMailArgs = null)
        {
            ActionResult result = new ActionResult();

            #region Send

            try
            {
                if (useCommunicator)
                {

                    List<MessageAttachmentDTO> MessageAttachmentDTOs = new List<MessageAttachmentDTO>();

                    foreach (var file in attachments)
                    {
                        if (file.Value == null || file.Value.Length == 0)
                        {
                            return new ActionResult(string.Format(GetText(7772, "Bilaga {0} innehåller ingen data"), file.Key));
                        }

                        var messageAttachmentDTO = new MessageAttachmentDTO
                        {
                            Data = file.Value,
                            Filesize = file.Value.LongLength,
                            Name = file.Key
                        };

                        MessageAttachmentDTOs.Add(messageAttachmentDTO);
                    }

                    result = SendEmailViaCommunicator(from, to, cc, emailTemplate.Subject, emailTemplate.Body, emailTemplate.BodyIsHTML, MessageAttachmentDTOs, customMailArgs);
                    sentToMail.Add(to);

                }
                else
                {
                    var mail = new Email(from, to, emailTemplate.Subject, emailTemplate.Body, emailTemplate.BodyIsHTML, SettingManager.isTest(), cc.ToArray());
                    foreach (var file in attachments)
                    {
                        mail.Attach(file.Value, file.Key); //- MIME Type doesn´t seem to work, using filename instead. BUG 7379
                    }
                    mail.Send();

                    sentToMail.Add(to);
                }

            }
            catch (SmtpException smtpEx)
            {
                base.LogError(smtpEx, this.log);
                result.Exception = smtpEx;
                result.ErrorMessage = GetText(8067, "Anslutning till e-postserver misslyckades");
                return result;
            }
            catch (FormatException fex)
            {
                base.LogError(fex, this.log);
                result.Exception = fex;
                result.ErrorMessage = GetText(8068, "Maillista innehöll epostadresser som inte var angivna på rätt format");
                return result;
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result.Exception = ex;
                result.ErrorMessage = GetText(8069, "Postning misslyckades");
                return result;
            }

            return result;

            #endregion
        }

        public ActionResult SendEmailToPayrollAdministrator(int actorCompanyId, int payrollExportId)
        {
            ActionResult result = new ActionResult();

            Company company = CompanyManager.GetCompany(actorCompanyId);
            if (company == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

            string exportSystem = "Payroll";
            string from = "support@softone.se";
            string to = SettingManager.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.SalaryExportEmail, 0, actorCompanyId, 0);
            string copy = SettingManager.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.SalaryExportEmailCopy, 0, actorCompanyId, 0);
            bool useCommunicator = true;

            bool emailcontentIsHtml = false;
            TimeSalaryExport payrollExport = TimeSalaryManager.GetTimeSalaryExport(payrollExportId, actorCompanyId, false);

            // Create filenames
            Dictionary<int, string> dict = TimeSalaryManager.GetExportTargetsDict(true);
            foreach (var pair in dict)
            {
                if (payrollExport.ExportTarget == pair.Key)
                    exportSystem = pair.Value;
            }

            DateTime fromDate = DateTime.Parse(payrollExport.StartInterval.ToString());
            DateTime toDate = DateTime.Parse(payrollExport.StopInterval.ToString());
            string fromTo = fromDate.ToShortDateString() + "-" + toDate.ToShortDateString();
            string fromToWide = fromDate.ToShortDateString() + " - " + toDate.ToShortDateString();
            string subject = company.Name + " - " + exportSystem + " - " + fromToWide;
            string body = company.Name + " - " + exportSystem + " - " + fromToWide;

            List<string> cc = new List<string>();
            if (copy != String.Empty && Validator.ValidateEmail(copy))
                cc.Add(copy);

            List<MessageAttachmentDTO> attachments = new List<MessageAttachmentDTO>();
            attachments.Add(new MessageAttachmentDTO()
            {
                Name = company.Name + "_" + "Loneunderlag" + "_" + exportSystem + "_" + fromTo + "." + payrollExport.Extension,
                Data = payrollExport.File1,
                Filesize = payrollExport.File1.Length,
            });

            if (payrollExport.File2.Length >= 1)
            {
                string extension = payrollExport.Extension;
                if (payrollExport.ExportTarget == (int)SoeTimeSalaryExportTarget.Hogia214006 || payrollExport.ExportTarget == (int)SoeTimeSalaryExportTarget.Hogia214007 || payrollExport.ExportTarget == (int)SoeTimeSalaryExportTarget.Flex)
                    extension = "sch";

                attachments.Add(new MessageAttachmentDTO()
                {
                    Name = company.Name + "_" + "Arbetsschema" + "_" + exportSystem + "_" + fromTo + "." + extension,
                    Data = payrollExport.File2,
                    Filesize = payrollExport.File2.Length
                });
            }

            if (!useCommunicator)
            {

                try
                {
                    Email mail = new Email(from, to, subject, body, emailcontentIsHtml, false, SettingManager.isTest(), cc.ToArray());

                    if (attachments != null)
                    {
                        foreach (var item in attachments)
                        {
                            ContentType type = new ContentType("text/plain");

                            item.Name = item.Name.Replace("ö", "o");
                            item.Name = item.Name.Replace("Ö", "O");
                            item.Name = item.Name.Replace("ä", "a");
                            item.Name = item.Name.Replace("Ä", "a");
                            item.Name = item.Name.Replace("å", "a");
                            item.Name = item.Name.Replace("Å", "a");

                            type.Name = item.Name;

                            mail.Attach(new List<byte[]>() { item.Data }, type);
                        }
                    }

                    mail.Send();
                }
                catch (SmtpException smtpEx)
                {
                    base.LogError(smtpEx, this.log);
                    result.Exception = smtpEx;
                    result.ErrorMessage = GetText(8067, "Anslutning till e-postserver misslyckades");
                    return result;
                }
                catch (FormatException fex)
                {
                    base.LogError(fex, this.log);
                    result.Exception = fex;
                    result.ErrorMessage = GetText(8068, "Maillista innehöll epostadresser som inte var angivna på rätt format");
                    return result;
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.ErrorMessage = GetText(8069, "Postning misslyckades");
                    return result;
                }
            }
            else
            {
                try
                {
                    result = SendEmailViaCommunicator(from, to, cc, subject, body, emailcontentIsHtml, attachments);
                }
                catch (FormatException fex)
                {
                    base.LogError(fex, this.log);
                    result.Exception = fex;
                    result.ErrorMessage = GetText(8068, "Maillista innehöll epostadresser som inte var angivna på rätt format");
                    return result;
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.ErrorMessage = GetText(8069, "Postning misslyckades");
                    return result;
                }
            }

            return result;
        }

        #endregion

        #region EmailTemplate

        public List<EmailTemplate> GetEmailTemplates(int actorCompanyId, int? id = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmailTemplate.NoTracking();
            return GetEmailTemplates(entities, actorCompanyId, id);
        }

        public List<EmailTemplate> GetEmailTemplates(CompEntities entities, int actorCompanyId, int? id = null)
        {
            IQueryable<EmailTemplate> emailTemplatesQuery = from tmpl in entities.EmailTemplate
                                                            where tmpl.Company.ActorCompanyId == actorCompanyId
                                                            select tmpl;
            if (id.HasValue)
            {
                emailTemplatesQuery = emailTemplatesQuery.Where( tmpl => tmpl.EmailTemplateId == id.Value );
            }

            return emailTemplatesQuery.ToList();
        }

        public List<EmailTemplate> GetEmailTemplatesByType(int actorCompanyId, int type)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmailTemplate.NoTracking();
            return GetEmailTemplatesByType(entities, actorCompanyId, type);
        }

        public List<EmailTemplate> GetEmailTemplatesByType(CompEntities entities, int actorCompanyId, int type)
        {
            return (from tmpl in entities.EmailTemplate
                    where tmpl.Company.ActorCompanyId == actorCompanyId &&
                    tmpl.Type == type
                    select tmpl).ToList();
        }

        public object GetEmailTemplatesByTypeDict(int actorCompanyId, int type, bool addEmptyRow)
        {
            var dict = new Dictionary<int, string>();
            if (addEmptyRow)
                dict.Add(0, " ");

            List<EmailTemplate> templates = GetEmailTemplatesByType(actorCompanyId, type);
            foreach (EmailTemplate template in templates)
            {
                dict.Add(template.EmailTemplateId, template.Name);
            }

            return dict;
        }

        public EmailTemplate GetEmailTemplate(int emailTemplateId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmailTemplate.NoTracking();
            return GetEmailTemplate(entities, emailTemplateId, actorCompanyId);
        }

        public EmailTemplate GetEmailTemplate(CompEntities entities, int emailTemplateId, int actorCompanyId)
        {
            return (from tmpl in entities.EmailTemplate
                    where (tmpl.Company.ActorCompanyId == actorCompanyId &&
                    tmpl.EmailTemplateId == emailTemplateId)
                    select tmpl).FirstOrDefault();
        }

        public object GetEmailTemplatesDic(int actorCompanyId, bool addEmptyRow)
        {
            var dict = new Dictionary<int, string>();
            if (addEmptyRow)
                dict.Add(0, " ");

            List<EmailTemplate> templates = GetEmailTemplates(actorCompanyId);
            foreach (EmailTemplate template in templates)
            {
                dict.Add(template.EmailTemplateId, template.Name);
            }

            return dict;
        }

        public bool EmailTemplateExist(string name, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.EmailTemplate.NoTracking();
            return EmailTemplateExist(entities, name, actorCompanyId);
        }

        public bool EmailTemplateExist(CompEntities entities, string name, int actorCompanyId)
        {
            return (from tmpl in entities.EmailTemplate
                    where ((tmpl.Name.ToLower() == name.ToLower()) &&
                    (tmpl.Company.ActorCompanyId == actorCompanyId))
                    select tmpl).Any();

        }

        public ActionResult AddEmailTemplate(EmailTemplate emailTemplate, int actorCompanyId)
        {
            if (emailTemplate == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "EmailTemplate");

            if (EmailTemplateExist(emailTemplate.Name, actorCompanyId))
                return new ActionResult((int)ActionResultSave.EmailTemplateExists);

            using (CompEntities entities = new CompEntities())
            {
                emailTemplate.Company = CompanyManager.GetCompany(entities, actorCompanyId);
                if (emailTemplate.Company == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                return AddEntityItem(entities, emailTemplate, "EmailTemplate");
            }
        }

        public ActionResult UpdateEmailTemplate(EmailTemplate emailTemplate, int actorCompanyId)
        {
            if (emailTemplate == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "EmailTemplate");

            using (CompEntities entities = new CompEntities())
            {
                var originalEmailTemplate = GetEmailTemplate(entities, emailTemplate.EmailTemplateId, actorCompanyId);
                if (originalEmailTemplate == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "EmailTemplate");

                return UpdateEntityItem(entities, originalEmailTemplate, emailTemplate, "EmailTemplate");
            }
        }

        public ActionResult SaveEmailTemplate(EmailTemplateDTO emailTemplate, int actorCompanyId)
        {
            EmailTemplate newTemplate = null;
            if (emailTemplate == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "EmailTemplate");

            int emailTemplateId = 0;
            if (!emailTemplate.BodyIsHTML)
                emailTemplate.Body = StringUtility.HTMLToText(emailTemplate.Body, true);

            using (CompEntities entities = new CompEntities())
            {
                if (emailTemplate.EmailTemplateId == 0)
                {
                    #region add template

                    if (EmailTemplateExist(entities, emailTemplate.Name, actorCompanyId))
                        return new ActionResult((int)ActionResultSave.EmailTemplateExists);

                    Company company = CompanyManager.GetCompany(entities, actorCompanyId);
                    if (company == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "Company");

                    newTemplate = new EmailTemplate()
                    {
                        Company = company,
                        Body = emailTemplate.Body,
                        BodyIsHTML = emailTemplate.BodyIsHTML,
                        Name = emailTemplate.Name,
                        Subject = emailTemplate.Subject,
                        Type = emailTemplate.Type,
                    };

                    SetCreatedProperties(newTemplate);
                    entities.EmailTemplate.AddObject(newTemplate);

                    #endregion
                }
                else
                {
                    #region update template
                    emailTemplateId = emailTemplate.EmailTemplateId;
                    var originalEmailTemplate = GetEmailTemplate(entities, emailTemplate.EmailTemplateId, actorCompanyId);
                    if (originalEmailTemplate == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "EmailTemplate");

                    originalEmailTemplate.Body = emailTemplate.Body;
                    originalEmailTemplate.BodyIsHTML = emailTemplate.BodyIsHTML;
                    originalEmailTemplate.Name = emailTemplate.Name;
                    originalEmailTemplate.Subject = emailTemplate.Subject;
                    originalEmailTemplate.Type = emailTemplate.Type;

                    SetModifiedProperties(originalEmailTemplate);

                    #endregion                    
                }

                ActionResult result = SaveChanges(entities);
                result.IntegerValue = emailTemplateId == 0 ? newTemplate.EmailTemplateId : emailTemplate.EmailTemplateId;
                return result;
            }
        }

        public ActionResult DeleteEmailTemplate(EmailTemplate emailTemplate, int actorCompanyId)
        {
            if (emailTemplate == null)
                return new ActionResult((int)ActionResultDelete.EntityIsNull, "EmailTemplate");

            using (CompEntities entities = new CompEntities())
            {
                var originalEmailTemplate = GetEmailTemplate(entities, emailTemplate.EmailTemplateId, actorCompanyId);
                if (originalEmailTemplate == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "EmailTemplate");

                return ChangeEntityState(entities, originalEmailTemplate, SoeEntityState.Deleted, true);
            }
        }

        public ActionResult DeleteEmailTemplate(int emailTemplateId, int actorCompanyId)
        {
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                var emailTemplate = GetEmailTemplate(entities, emailTemplateId, actorCompanyId);
                if (emailTemplate == null)
                    result = new ActionResult((int)ActionResultSave.EntityNotFound, "EmailTemplate");

                if (result.Success)
                {
                    entities.DeleteObject(emailTemplate);
                    result = SaveChanges(entities);
                }
            }

            return result;
        }

        #endregion

        #region Helpers

        public static ActionResult IsValidFromAddress(string fromAddress)
        {
            if (string.IsNullOrEmpty(fromAddress))
                return new ActionResult(TermCacheManager.Instance.GetText(4591, 1, "Ogiltig e-postadress !")+ " (" + fromAddress + ")");
            
            var localFromAddress = fromAddress.ToLower();
            var result = IsValidEmail(fromAddress);
            if (!result.Success)
                return result;

            var blocked = BlockedFromDomains.Any(item => fromAddress.Contains(item));

            if (blocked)
            {
                return new ActionResult(TermCacheManager.Instance.GetText(7727, 1, "Avsändande e-postadress innehåller ogiltig domän.") + " ("+ fromAddress+")");
            }

            if (BlockedFromDomains.Contains(localFromAddress))
            {
                return new ActionResult(TermCacheManager.Instance.GetText(7727,1, "Avsändande e-postadress innehåller ogiltig domän.") + " (" + fromAddress + ")");
            }

            return new ActionResult();
        }

        public static ActionResult IsValidEmail(string emailaddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailaddress);

                return new ActionResult();
            }
            catch 
            {
                return new ActionResult(TermCacheManager.Instance.GetText(4591, 1, "Ogiltig e-postadress !"));
            }
        }

        #endregion
    }
}
