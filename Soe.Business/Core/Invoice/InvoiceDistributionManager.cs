using SoftOne.Communicator.Shared.DTO;
using SoftOne.Soe.Business.Core.Reports;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.API.InExchange;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.DTO.Reports;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Text;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Core
{
    public class InvoiceDistributionManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly List<int> InvoiceIdsWithUpdatedStatus = new List<int>();

        #endregion

        #region Ctor

        public InvoiceDistributionManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region distribution item

        public int EnsureDistributionItem(int originId, TermGroup_EDistributionType type, TermGroup_EDistributionStatusType status, string guid, bool allwaysNew, string msg = null)
        {
            this.InvoiceIdsWithUpdatedStatus.Add(originId);
            using (CompEntities entities = new CompEntities())
            {
                var invDistr = allwaysNew ? null : GetDistributionItem(entities, originId, type);
                if (invDistr == null)
                {
                    invDistr = new InvoiceDistribution
                    {
                        OriginId = originId,
                        DistributionType = (short)type,
                        Guid = guid,
                        Msg = msg,
                        ActorCompanyId = base.ActorCompanyId
                    };
                    SetCreatedProperties(invDistr);
                    entities.InvoiceDistribution.AddObject(invDistr);
                }
                else if (!string.IsNullOrEmpty(msg) && string.IsNullOrEmpty(invDistr.Msg))
                {
                    invDistr.Msg = msg;
                }
                else if (
                            invDistr.DistributionStatus == (short)TermGroup_EDistributionStatusType.Error &&
                            (status == TermGroup_EDistributionStatusType.Sent || status == TermGroup_EDistributionStatusType.PendingInPlatform)
                        )
                {
                    invDistr.Msg = msg;
                }

                if (invDistr.DistributionStatus != (short)status && status == TermGroup_EDistributionStatusType.PendingInPlatform)
                    invDistr.Msg = null;

                if (string.IsNullOrEmpty(invDistr.Guid) || invDistr.Guid != guid)
                    invDistr.Guid = guid;

                if (invDistr.InvoiceDistributionId != 0)
                {
                    SetModifiedProperties(invDistr);
                }

                
                invDistr.DistributionStatus = (short)status;
                entities.SaveChanges();
                return invDistr.InvoiceDistributionId;
            }
        }

        public void SetDistributionStatusOnUnsetInvoices(List<int> allInvoiceIds, string errorMessage)
        {
            var remainingInvoiceIds = allInvoiceIds.Where(i => !this.InvoiceIdsWithUpdatedStatus.Contains(i)).ToList();
            foreach (var invoiceId in remainingInvoiceIds)
            {
                EnsureDistributionItem(invoiceId, TermGroup_EDistributionType.Unknown, TermGroup_EDistributionStatusType.Error, null, false, errorMessage);
            }
        }

        private ActionResult UpdateDistributionItem(int invoiceDistributionId, TermGroup_EDistributionStatusType status, string msg)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return UpdateDistributionItem(entities, invoiceDistributionId, status, msg);
        }

        private ActionResult UpdateDistributionItem(CompEntities entities, int invoiceDistributionId, TermGroup_EDistributionStatusType status, string msg)
        {
            var invDistr = GetDistributionItem(entities, invoiceDistributionId);
            if (invDistr != null)
            {
                invDistr.DistributionStatus = (short)status;
                invDistr.Msg = msg;
                SetModifiedProperties(invDistr);
                return SaveChanges(entities);
            }
            else
            {
                return new ActionResult(false);
            }
        }

        private ActionResult UpdateDistributionItem(int originId, TermGroup_EDistributionType type, TermGroup_EDistributionStatusType status, string msg)
        {
            using (var entities = new CompEntities())
            {
                var invDistr = GetDistributionItem(entities, originId, type);
                if (invDistr != null)
                {
                    invDistr.DistributionStatus = (short)status;
                    invDistr.Msg = msg;
                    SetModifiedProperties(invDistr);
                    return SaveChanges(entities);
                }
                else
                {
                    return new ActionResult(false);
                }
            }
        }

        public ActionResult UpdateDistributionItem(string guid, TermGroup_EDistributionType type, TermGroup_EDistributionStatusType status, string msg)
        {
            using (var entities = new CompEntities())
            {
                var invDistr = GetDistributionItem(entities, guid, type);
                if (invDistr != null)
                {
                    invDistr.DistributionStatus = (short)status;
                    invDistr.Msg = msg;

                    if (type == TermGroup_EDistributionType.Intrum || type == TermGroup_EDistributionType.Finvoice)
                        UpdateElectronicStatusIcon(invDistr.OriginId);

                    SetModifiedProperties(invDistr);
                    return SaveChanges(entities);
                }
                else
                {
                    return new ActionResult(false);
                }
            }
        }

        private InvoiceDistribution GetDistributionItem(CompEntities entities, int originId, TermGroup_EDistributionType type)
        {
            return entities.InvoiceDistribution.FirstOrDefault(x => x.OriginId == originId && x.DistributionType == (short)type);
        }

        private InvoiceDistribution GetDistributionItem(CompEntities entities, string guid, TermGroup_EDistributionType type)
        {
            return entities.InvoiceDistribution.FirstOrDefault(x => x.Guid == guid && x.DistributionType == (short)type);
        }

        private InvoiceDistribution GetDistributionItem(CompEntities entities, int invoiceDistributionId)
        {
            return entities.InvoiceDistribution.FirstOrDefault(x => x.InvoiceDistributionId == invoiceDistributionId);
        }

        public List<int> GetCompanysInActiveDistributionItems(CompEntities entities, TermGroup_EDistributionType type)
        {
            return (from id in entities.InvoiceDistribution
                    where id.State == (int)SoeEntityState.Active && id.DistributionType == (int)type && id.DistributionStatus != (int)TermGroup_EDistributionStatusType.Sent
                   select id.ActorCompanyId).Distinct().ToList();
        }

        public List<InvoiceDistribution> GetDistributionItems(CompEntities entities, TermGroup_EDistributionType type, List<string> guids, TermGroup_EDistributionStatusType status, int? actorCompanyId)
        {
            IQueryable<InvoiceDistribution> query = (from i in entities.InvoiceDistribution
                                                     where
                                                        i.DistributionType == (int)type &&
                                                        i.State == (int)SoeEntityState.Active
                                                     select i);
            
            if (actorCompanyId.HasValue)
            {
                query = query.Where(x => x.ActorCompanyId == actorCompanyId.Value);
            }

            if (!guids.IsNullOrEmpty())
            {
                query = query.Where(x => guids.Contains(x.Guid));
            }

            if (status != TermGroup_EDistributionStatusType.Unknown)
            {
                query = query.Where(x => x.DistributionStatus == (int)status );
            }

            return query.ToList();
        }

        public List<InvoiceDistributionDTO> GetDistributionItems(int actorCompanyId, SoeOriginType originType, TermGroup_EDistributionType type, TermGroup_GridDateSelectionType allItemsSelection)
        {
            var selectionDate = InvoiceManager.GetSelectionDate(allItemsSelection);

            var langId = base.GetLangId();
            var statusTexts = base.GetTermGroupDict(TermGroup.EDistributionStatusType, langId);
            var originTypes = base.GetTermGroupDict(TermGroup.OriginType, langId);
            var distTypes = base.GetTermGroupDict(TermGroup.EdistributionTypes, langId);

            using (var entities = new CompEntities())
            {
                IQueryable<InvoiceDistribution> query = (from id in entities.InvoiceDistribution
                                                             //join origin in entities.Origin
                                                         where id.State == (int)SoeEntityState.Active && (id.DistributionType == (int)type || type == TermGroup_EDistributionType.Unknown) && id.ActorCompanyId == actorCompanyId
                                                             && id.Created >= selectionDate
                                                             && id.Origin.Type != (int)SoeOriginType.Purchase
                                                         select id);

                if (originType != SoeOriginType.None)
                {
                    query = query.Where(x => x.Origin.Type == (int)originType);
                }

                var result = query.Select(EntityExtensions.GetInvoiceDistributionInvoiceDTO)
                                .ToList();

                if (originType == SoeOriginType.None || originType == SoeOriginType.Purchase)
                {
                    result.AddRange((from id in entities.InvoiceDistribution
                                         //join origin in entities.Origin
                                     where id.State == (int)SoeEntityState.Active && (id.DistributionType == (int)type || type == TermGroup_EDistributionType.Unknown) && id.ActorCompanyId == actorCompanyId
                                         && id.Created >= selectionDate
                                         && id.Origin.Type == (int)SoeOriginType.Purchase
                                     select id)
                           .Select(EntityExtensions.GetInvoiceDistributionPurchaseDTO)
                           .ToList());
                }

                foreach (var item in result)
                {
                    item.OriginTypeName = originTypes[item.OriginTypeId.GetValueOrDefault()];
                    item.StatusName = statusTexts[item.Status];
                    item.TypeName = distTypes[item.Type];
                }

                return result.OrderByDescending(d => d.Created).ToList();
            }
        }

        #endregion

        #region email

        public void SendRemindersAsEmails(List<CustomerInvoiceGridDTO> items, int actorCompanyId, ParameterObject paramO, int? emailTemplateId)
        {
            var im = new InvoiceManager(paramO);
            var rm = new ReportManager(paramO);
            
            foreach (var item in items)
            {
                try
                {
                    var invoice = im.GetCustomerInvoice(item.CustomerInvoiceId, loadActor: true, loadOriginInvoiceMapping: true);
                    if (invoice == null)
                        continue;

                    var invoiceDistributionId = EnsureDistributionItem(invoice.InvoiceId, TermGroup_EDistributionType.Email, TermGroup_EDistributionStatusType.PendingInPlatform, null, true);

                    //Changed to using reminderEcom
                    if (item.ReminderContactEComId.GetValueOrDefault() == 0)
                    {
                        UpdateDistributionItem(invoiceDistributionId, TermGroup_EDistributionStatusType.Error, GetText(7627, "Ingen e-post mottagare är angiven"));
                        continue;
                    }

                    CompanySettingType companySettingType = CompanySettingType.CustomerDefaultReminderTemplate;
                    SoeReportTemplateType reportTemplateType = SoeReportTemplateType.BillingInvoiceReminder;
                    int reportId = rm.GetCompanySettingReportId(SettingMainType.Company, companySettingType, reportTemplateType, actorCompanyId, base.UserId);

                    int selectedEmailTemplateId = emailTemplateId ?? 0;
                    if (selectedEmailTemplateId == 0)
                        selectedEmailTemplateId = this.SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingDefaultEmailTemplate, 0, ActorCompanyId, 0);

                    SendEmail(invoice.InvoiceId, reportId, new List<int> { item.ReminderContactEComId.Value }, invoice.InvoiceNr, 0, false, false, OrderInvoiceRegistrationType.Invoice, selectedEmailTemplateId, false, null, actorCompanyId, null, invoiceDistributionId, true);
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                }
            }
        }
        /* NEW
        public void SendAsEmails(List<CustomerInvoice> invoices, int actorCompanyId, ParameterObject paramO, int? emailTemplateId, int? reportId, int? languageId, bool mergePdfs)
        {
            var rm = new ReportManager(paramO);
            var rdm = new ReportDataManager(paramO);
            var im = new InvoiceManager(paramO);
            var clm = new ChecklistManager(paramO);

            foreach (var invoice in invoices)
            {
                List<Tuple<int, string, byte[]>> invoiceAttachments;
                var emailAttachments = new List<KeyValuePair<string, byte[]>>();

                int invoiceDistributionId = EnsureDistributionItem(invoice.InvoiceId, TermGroup_EDistributionType.Email, TermGroup_EDistributionStatusType.PendingInPlatform, null, true);
                if ( invoice.ContactEComId.GetValueOrDefault() == 0 && (invoice.CustomerEmail == null || string.IsNullOrEmpty(invoice.CustomerEmail.Trim())) )
                {
                    UpdateInvoiceEmailStatus(im, invoice.InvoiceId, TermGroup_ReportPrintoutStatus.Error, "No email address specified on invoice", invoiceDistributionId);
                    continue;
                }

                try
                {
                    #region Email

                    invoiceAttachments = GetInvoiceDocuments(invoice.InvoiceId, actorCompanyId, (SoeOriginType)invoice.Origin.Type, null, true);
                    invoiceAttachments.ForEach(a => emailAttachments.Add(new KeyValuePair<string, byte[]>(a.Item2, a.Item3)));
                    //emailAttachments = invoice.AddAttachementsToEInvoice ? GetInvoiceDocuments(invoice.InvoiceId, actorCompanyId, (SoeOriginType)invoice.Origin.Type, null, true) : new Dictionary<string, byte[]>();

                    //try to fetch if there are checklist on a order that the invoice originates from.....
                    if (invoice.OriginInvoiceMapping.IsLoaded && invoice.OriginInvoiceMapping.Any())
                    {
                        var connectedOrderIds = invoice.OriginInvoiceMapping.Select(x => x.OriginId).ToList();
                        if (connectedOrderIds.Any())
                        {
                            foreach (var orderId in connectedOrderIds)
                            {
                                var checklistIds = clm.GetChecklistHeadRecordsWithRows(SoeEntityType.Order, orderId, actorCompanyId, false).Where(x => x.AddAttachementsToEInvoice).Select(y => y.ChecklistHeadRecordId).ToList();
                                if (checklistIds != null && checklistIds.Any())
                                {
                                    var checklists = clm.GetChecklistAsDocuments(rm, rdm, orderId, checklistIds, actorCompanyId);
                                    if (checklists != null && checklists.Any())
                                    {
                                        emailAttachments.AddRange(checklists);
                                    }
                                }
                            }
                        }
                    }

                    #endregion

                    var mailResult = SendEmail(invoice.InvoiceId, reportId.GetValueOrDefault(), new List<int> { (int)invoice.ContactEComId }, invoice.InvoiceNr, languageId ?? 0, invoice.PrintTimeReport, invoice.IncludeOnlyInvoicedTime, OrderInvoiceRegistrationType.Invoice, emailTemplateId ?? 0, mergePdfs, invoice.CustomerEmail, actorCompanyId, emailAttachments, invoiceDistributionId);

                    if (mailResult.Success && invoiceAttachments != null && invoiceAttachments.Count > 0)
                    {
                        foreach (var invoiceAttachment in invoiceAttachments.Where(a => a.Item1 > 0))
                        {
                            InvoiceAttachmentManager.ConnectInvoiceAttachmentToDistribution(invoiceAttachment.Item1, invoiceDistributionId, true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    UpdateInvoiceEmailStatus(im, invoice.InvoiceId, TermGroup_ReportPrintoutStatus.Error, "Exception:" + ex.Message, invoiceDistributionId);
                }
            }
        }
        */


        public void SendAsEmails(List<CustomerInvoice> invoices, int actorCompanyId, ParameterObject paramO, int? emailTemplateId, int? reportId, int? languageId, bool mergePdfs)
        {
            var evaluatedSelections = new List<EvaluatedSelection>();

            var rm = new ReportManager(paramO);
            var rdm = new ReportDataManager(paramO);
            var im = new InvoiceManager(paramO);
            var clm = new ChecklistManager(paramO);
            var em = new EmailManager(this.parameterObject);

            foreach (var invoice in invoices)
            {
                evaluatedSelections.Clear();

                List<Tuple<int, string, byte[]>> invoiceAttachments;
                var emailAttachments = new List<KeyValuePair<string, byte[]>>();

                int invoiceDistributionId = EnsureDistributionItem(invoice.InvoiceId, TermGroup_EDistributionType.Email, TermGroup_EDistributionStatusType.PendingInPlatform, null, true);
                if (invoice.ContactEComId.GetValueOrDefault() == 0 && (invoice.CustomerEmail == null || string.IsNullOrEmpty(invoice.CustomerEmail.Trim())))
                {
                    UpdateInvoiceEmailStatus(invoice.InvoiceId, TermGroup_ReportPrintoutStatus.Error, GetText(7627, "Ingen e-post mottagare är angiven"), invoiceDistributionId);
                    continue;
                }

                try
                {
                    #region Email

                    invoiceAttachments = GetInvoiceDocuments(invoice.InvoiceId, actorCompanyId, (SoeOriginType)invoice.Origin.Type, null, true);
                    invoiceAttachments.ForEach(a => emailAttachments.Add(new KeyValuePair<string, byte[]>(a.Item2, a.Item3)));
                    //emailAttachments = invoice.AddAttachementsToEInvoice ? GetInvoiceDocuments(invoice.InvoiceId, actorCompanyId, (SoeOriginType)invoice.Origin.Type, null, true) : new Dictionary<string, byte[]>();

                    //try to fetch if there are checklist on a order that the invoice originates from.....
                    if (invoice.OriginInvoiceMapping.IsLoaded && invoice.OriginInvoiceMapping.Any())
                    {
                        var connectedOrderIds = invoice.OriginInvoiceMapping.Select(x => x.OriginId).ToList();
                        if (connectedOrderIds.Any())
                        {
                            foreach (var orderId in connectedOrderIds)
                            {
                                var checklistIds = clm.GetChecklistHeadRecordsWithRows(SoeEntityType.Order, orderId, actorCompanyId, false).Where(x => x.AddAttachementsToEInvoice).Select(y => y.ChecklistHeadRecordId).ToList();
                                if (checklistIds != null && checklistIds.Any())
                                {
                                    var checklists = clm.GetChecklistAsDocuments(rm, rdm, orderId, checklistIds, actorCompanyId);
                                    if (checklists != null && checklists.Any())
                                    {
                                        emailAttachments.AddRange(checklists);
                                    }
                                }
                            }
                        }
                    }

                    #endregion

                    ReportPrintoutDTO dto = PrintInvoiceReport(invoice.InvoiceId, reportId.GetValueOrDefault(), new List<int> { (int)invoice.ContactEComId }, invoice.InvoiceNr, invoice.ActorId.Value, languageId ?? 0, invoice.PrintTimeReport, invoice.IncludeOnlyInvoicedTime, OrderInvoiceRegistrationType.Invoice, emailTemplateId ?? 0, mergePdfs, invoice.CustomerEmail, actorCompanyId, rdm, im, em, emailAttachments, invoiceDistributionId);
                    if (dto.Status == (int)TermGroup_ReportPrintoutStatus.SentFailed)
                    {
                        var errorText = $"{GetText(7408, "Skicka epost misslyckades")}: {dto.EmailMessage}";
                        UpdateInvoiceEmailStatus(invoice.InvoiceId, TermGroup_ReportPrintoutStatus.Error, errorText, invoiceDistributionId);
                    }
                    else if (dto.Status == (int)TermGroup_ReportPrintoutStatus.Error)
                    {
                        var errorText = $"{GetText(5947, "Utskrift misslyckades")}: {dto.ResultMessageDetails} ({dto.ResultMessage})";
                        UpdateInvoiceEmailStatus(invoice.InvoiceId, TermGroup_ReportPrintoutStatus.Error, errorText, invoiceDistributionId);
                    }
                    else if (dto.Status == (int)TermGroup_ReportPrintoutStatus.Sent && invoiceAttachments != null && invoiceAttachments.Count > 0)
                    {
                        foreach (var invoiceAttachment in invoiceAttachments.Where(a => a.Item1 > 0))
                        {
                            InvoiceAttachmentManager.ConnectInvoiceAttachmentToDistribution(invoiceAttachment.Item1, invoiceDistributionId, true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    UpdateInvoiceEmailStatus(invoice.InvoiceId, TermGroup_ReportPrintoutStatus.Error, "Exception:" + ex.Message, invoiceDistributionId);
                }
            }
        }

        public ReportPrintoutDTO PrintInvoiceReport(int invoiceId, int reportId, List<int> emailRecipients, string invoiceNr, int actorCustomerId, int languageId, bool printTimeReport, bool includeOnlyInvoicedTime, OrderInvoiceRegistrationType registrationType, int emailTemplateId, bool mergePdfs, string singleRecipient, int actorCompanyId, ReportDataManager rdm, InvoiceManager im, EmailManager em, List<KeyValuePair<string, byte[]>> emailAttachments, int invoiceDistributionId)
        {
            var reportItem = ReportManager.GetBillingInvoiceReportDTOSingle(invoiceId, reportId, emailRecipients, languageId, invoiceNr, actorCustomerId, printTimeReport, includeOnlyInvoicedTime, registrationType, false, emailTemplateId, false, singleRecipient);
            var report = ReportManager.GetReport(reportItem.ReportId, actorCompanyId);

            var emailTemplate = reportItem.EmailTemplateId.HasValue ? em.GetEmailTemplate(reportItem.EmailTemplateId.Value, actorCompanyId) : null;
            if (!emailRecipients.IsNullOrEmpty() || !string.IsNullOrEmpty(singleRecipient))
            {
                ReplaceVarsInEmailTemplate(emailTemplate, im, null, invoiceId);
            }

            var evaluatedSelections = new List<EvaluatedSelection>();
            var selection = new Selection(actorCompanyId, this.UserId, this.parameterObject.RoleId, this.LoginName, report: report.ToDTO(), isMainReport: true, exportType: (int)TermGroup_ReportExportType.Pdf, exportFileType: 0);
            selection.Evaluate(reportItem, 0);
            selection.Evaluated.MergePdfs = mergePdfs;
            selection.Evaluated.EmailTemplate = emailTemplate != null ? emailTemplate.ToDTO() : null;
            selection.Evaluated.EmailAttachments = emailAttachments;
            selection.Evaluated.InvoiceDistributionId = invoiceDistributionId;
            evaluatedSelections.Add(selection.Evaluated);

            ReportPrintoutDTO dto = rdm.PrintReportDTO(evaluatedSelections[0], true);
            return dto;
        }

        public ActionResult SendAsEmail(int invoiceId, int reportId, List<int> emailRecipients, int languageId, string invoiceNr, int actorCustomerId, bool printTimeReport, bool includeOnlyInvoicedTime, OrderInvoiceRegistrationType registrationType, bool invoiceCopy, bool asReminder, int emailTemplateId, bool addAttachmentsToEinvoice, List<int> attachmentIds, List<int> checklistIds, bool mergePdfs, string singleRecipient, int actorCompanyId)
        {
            var rm = new ReportManager(this.parameterObject);
            var rdm = new ReportDataManager(this.parameterObject);

            var result = new ActionResult();
            List<Tuple<int, string, byte[]>> invoiceAttachments = new List<Tuple<int, string, byte[]>>();
            var emailAttachments = new List<KeyValuePair<string, byte[]>>();
            int invoiceDistributionId = 0;
            singleRecipient = singleRecipient?.Trim();

            if ( emailRecipients.IsNullOrEmpty() && string.IsNullOrEmpty(singleRecipient) )
            {
                return new ActionResult(GetText(7627, "Ingen e-post mottagare är angiven"));
            }

            try
            {
                invoiceDistributionId = EnsureDistributionItem(invoiceId, TermGroup_EDistributionType.Email, TermGroup_EDistributionStatusType.PendingInPlatform, null, true);

                #region Email

                if (attachmentIds != null && attachmentIds.Any())
                {
                    var soeOriginType = registrationType == OrderInvoiceRegistrationType.Order ? SoeOriginType.Order : SoeOriginType.CustomerInvoice;
                    invoiceAttachments = GetInvoiceDocuments(invoiceId, actorCompanyId, soeOriginType, attachmentIds, true, false);
                    invoiceAttachments.ForEach(a => emailAttachments.Add(new KeyValuePair<string, byte[]>(a.Item2, a.Item3)));
                }

                if (checklistIds != null && checklistIds.Any())
                {
                    var checkListInvoiceId = invoiceId;
                    var chm = new ChecklistManager(this.parameterObject);

                    //if invoice checklist are saved on order....
                    if (registrationType == OrderInvoiceRegistrationType.Invoice)
                    {
                        var checkListHeadRecord = chm.GetChecklistHeadRecord(checklistIds.First(), actorCompanyId);
                        checkListInvoiceId = checkListHeadRecord == null ? invoiceId : checkListHeadRecord.RecordId;
                    }

                    var checklists = chm.GetChecklistAsDocuments(rm, rdm, checkListInvoiceId, checklistIds, actorCompanyId);
                    if (checklists.Any())
                    {
                        if (emailAttachments == null)
                            emailAttachments = new List<KeyValuePair<string, byte[]>>();

                        emailAttachments.AddRange(checklists);
                    }
                }

                #endregion

                result = SendEmail(invoiceId, reportId, emailRecipients, invoiceNr, languageId, printTimeReport, includeOnlyInvoicedTime, registrationType, emailTemplateId, mergePdfs, singleRecipient, actorCompanyId, emailAttachments, invoiceDistributionId, false);

                if (result.Success && invoiceAttachments != null && invoiceAttachments.Count > 0)
                {
                    foreach (var invoiceAttachment in invoiceAttachments.Where(a => a.Item1 > 0))
                    {
                        InvoiceAttachmentManager.ConnectInvoiceAttachmentToDistribution(invoiceAttachment.Item1, invoiceDistributionId, true);
                    }
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                if (invoiceDistributionId > 0)
                    UpdateInvoiceEmailStatus(invoiceId, TermGroup_ReportPrintoutStatus.Error, "Exception:" + ex.Message, invoiceDistributionId);
            }

            return result;
        }

        public DownloadFileDTO GetInvoicePdf(int invoiceId, int reportId, int languageId, bool printTimeReport, bool includeOnlyInvoicedTime, OrderInvoiceRegistrationType registrationType, bool asReminder)
        {
            var rrm = new RequestReportManager(this.parameterObject, this.ReportManager, this.ReportDataManager);

            var downloadFileDTO = rrm.PrintCustomerInvoice(new CustomerInvoicePrintDTO
            {
                Ids = new List<int>() { invoiceId },
                ReportId = reportId,
                Queue = false,
                ReportLanguageId = languageId,
                PrintTimeReport = printTimeReport,
                IncludeOnlyInvoiced = includeOnlyInvoicedTime,
                OrderInvoiceRegistrationType = registrationType,
                ReturnAsBinary = true,
                exportType = TermGroup_ReportExportType.Pdf,
                AsReminder = asReminder
            }, true);

            return downloadFileDTO;
        }

        public ActionResult SendEmail(int invoiceId, int reportId, List<int> emailRecipients, string invoiceNr, int languageId, bool printTimeReport, bool includeOnlyInvoicedTime, OrderInvoiceRegistrationType registrationType, int emailTemplateId, bool mergePdfs, string singleRecipient, int actorCompanyId, List<KeyValuePair<string, byte[]>> emailAttachments, int invoiceDistributionId, bool asReminder)
        {
            var reportDto = GetInvoicePdf(invoiceId, reportId, languageId, printTimeReport, includeOnlyInvoicedTime, registrationType, asReminder);
            if (reportDto == null || !reportDto.Success || reportDto.BinaryData == null || reportDto.BinaryData.Length == 0)
            {
                var errorText = $"{GetText(7753, "Misslyckades skapa faktura fil")}: {reportDto?.ErrorMessage}";
                UpdateInvoiceEmailStatus(invoiceId, TermGroup_ReportPrintoutStatus.Error, errorText, invoiceDistributionId);
                return new ActionResult(errorText);
            }

            var fileName = invoiceNr.ToString() + Constants.SOE_SERVER_FILE_PDF_SUFFIX;

            EmailTemplate emailTemplate = (emailTemplateId > 0) ? EmailManager.GetEmailTemplate(emailTemplateId, base.ActorCompanyId) : null;

            if (emailTemplateId > 0 && emailTemplate == null)
                return new ActionResult(4146, GetText(4146, "E-postmall hittades inte"));

            if (emailTemplate != null && !emailRecipients.IsNullOrEmpty() || !string.IsNullOrEmpty(singleRecipient))
            {
                ReplaceVarsInEmailTemplate(emailTemplate, this.InvoiceManager, null, invoiceId);
            }

            if (emailAttachments == null)
            {
                emailAttachments = new List<KeyValuePair<string, byte[]>>();
            }

            emailAttachments.Add(new KeyValuePair<string, byte[]>(fileName, reportDto.BinaryData));
            if (mergePdfs && emailAttachments.Count > 1)
                emailAttachments = PDFUtility.MergeDictionary(fileName, emailAttachments);

            Dictionary<string, string> customMailArg = null;
            customMailArg = new Dictionary<string, string>() { { "SOEInvoiceDistribution", $"{ConfigurationSetupUtil.GetCurrentSysCompDbId()}#{invoiceDistributionId}" } };

            var result = EmailManager.SendEmailWithAttachment(base.ActorCompanyId, emailTemplate?.ToDTO(), emailRecipients, emailAttachments, new List<string> { singleRecipient }, customMailArg);
            if (result.Success)
            {
                UpdateInvoiceEmailStatus(invoiceId, TermGroup_ReportPrintoutStatus.Sent, result.StringValue, invoiceDistributionId);
            }
            else {
                var errorText = GetText(7408, "Skicka epost misslyckades") + ": " + (result.ErrorMessage);
                UpdateInvoiceEmailStatus(invoiceId, TermGroup_ReportPrintoutStatus.Error, errorText, invoiceDistributionId);
            }

            return result;
        }

        public ActionResult UpdateEmailStatus(int invoiceDistributionId, TermGroup_ReportPrintoutStatus reportPrintStatus, string msg)
        {
            using (CompEntities entities = new CompEntities())
            {
                var item = GetDistributionItem(entities, invoiceDistributionId);
                if (item == null)
                    return new ActionResult((int)ActionResultSave.EntityIsNull, "InvoiceDistribution");

                //return UpdateDistributionItem(entities, invoiceDistributionId,TermGroup_EDistributionStatusType.Error, msg);
                return UpdateInvoiceEmailStatus(entities,item.OriginId, reportPrintStatus, msg, invoiceDistributionId);
            }
        }

        public ActionResult UpdateInvoiceEmailStatus(int invoiceId, TermGroup_ReportPrintoutStatus reportPrintStatus, string msg, int invoiceDistributionId)
        {
           
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return UpdateInvoiceEmailStatus(entities, invoiceId, reportPrintStatus, msg, invoiceDistributionId);

        }

        public ActionResult UpdateInvoiceEmailStatus(CompEntities entities, int invoiceId, TermGroup_ReportPrintoutStatus reportPrintStatus, string msg, int invoiceDistributionId)
        {
            ActionResult result;

            var customerInvoice = entities.Invoice.OfType<CustomerInvoice>().FirstOrDefault(x => x.InvoiceId == invoiceId && x.State == (int)SoeEntityState.Active);  //im.GetCustomerInvoice(entities, invoiceId, true, false, false, false, false, false, false, false, false, false, false, false, false);
            if (customerInvoice == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "CustomerInvoice");

            var status = (SoeStatusIcon)customerInvoice.StatusIcon;
            TermGroup_EDistributionStatusType distributionStatus = TermGroup_EDistributionStatusType.Unknown;
            switch (reportPrintStatus)
            {
                case TermGroup_ReportPrintoutStatus.Sent:
                    {
                        status |= SoeStatusIcon.Email;
                        status &= ~SoeStatusIcon.EmailError;
                        distributionStatus = TermGroup_EDistributionStatusType.Sent;
                        break;
                    }
                case TermGroup_ReportPrintoutStatus.Error:
                case TermGroup_ReportPrintoutStatus.SentFailed:
                    {
                        status |= SoeStatusIcon.EmailError;
                        status &= ~SoeStatusIcon.Email;
                        distributionStatus = TermGroup_EDistributionStatusType.Error;
                        break;
                    }
                case TermGroup_ReportPrintoutStatus.Queued:
                    {
                        status &= ~SoeStatusIcon.Email;
                        status &= ~SoeStatusIcon.EmailError;
                        distributionStatus = TermGroup_EDistributionStatusType.PendingInPlatform;
                        break;
                    }
            }

            customerInvoice.StatusIcon = (int)status;
            result = SaveChanges(entities);

            UpdateDistributionItem(entities, invoiceDistributionId, distributionStatus, msg);

            return result;
        }

        public ActionResult UpdatePurchaseEmailStatus(PurchaseManager pm, int purchaseId, int reportPrintStatus, string msg, int invoiceDistributionId)
        {
            
            using (CompEntities entities = new CompEntities())
            {
                var purchase = pm.GetPurchase(entities, purchaseId,false, base.ActorCompanyId);
                if (purchase == null)
                    return new ActionResult((int)ActionResultSave.EntityIsNull, "Purchase");

                var status = (SoeStatusIcon)purchase.StatusIcon;

                if (reportPrintStatus == (int)TermGroup_ReportPrintoutStatus.Sent)
                {
                    status |= SoeStatusIcon.Email;
                    status &= ~SoeStatusIcon.EmailError;
                }
                else
                {
                    status |= SoeStatusIcon.EmailError;
                    status &= ~SoeStatusIcon.Email;
                }

                purchase.StatusIcon = (int)status;
                var result = SaveChanges(entities);
                if (!result.Success)
                    return result;
            }
            
            return UpdateDistributionItem(invoiceDistributionId, reportPrintStatus == (int)TermGroup_ReportPrintoutStatus.Sent ? TermGroup_EDistributionStatusType.Sent : TermGroup_EDistributionStatusType.Error, msg);
        }

        private static void ReplaceVarsInEmailTemplate(EmailTemplate template, InvoiceManager im, CustomerInvoice invoice, int invoiceId)
        {
            if (template == null)
            {
                return;
            }

            if ((!string.IsNullOrEmpty(template.Body) && template.Body.IndexOf("[[") != -1) || (!string.IsNullOrEmpty(template.Subject) && template.Subject.IndexOf("[[") != -1))
            {
                if (invoice == null)
                {
                    invoice = im.GetCustomerInvoice(invoiceId, false, true);
                }

                if (invoice != null)
                {
                    template.Body = ReplaceVarsInText(template.Body, invoice);
                    template.Subject = ReplaceVarsInText(template.Subject, invoice);
                }
            }
        }

        private static string ReplaceVarsInText(string text, CustomerInvoice invoice)
        {
            const string startToken = "[[";
            const string endToken = "]]";
            var sb = new StringBuilder(text);
            sb.Replace(startToken + "InvoiceNr" + endToken, invoice.InvoiceNr);
            sb.Replace(startToken + "OrderNr" + endToken, invoice.OrderNumbers);
            sb.Replace(startToken + "OurReference" + endToken, string.IsNullOrEmpty(invoice.ReferenceOur) ? "" : invoice.ReferenceOur);
            sb.Replace(startToken + "YourReference" + endToken, string.IsNullOrEmpty(invoice.ReferenceYour) ? "" : invoice.ReferenceYour);
            sb.Replace(startToken + "Label" + endToken, string.IsNullOrEmpty(invoice.InvoiceLabel) ? "" : invoice.InvoiceLabel);
            sb.Replace(startToken + "CustomerName" + endToken, invoice.Actor != null && invoice.Actor.Customer != null && invoice.Actor.Customer.IsOneTimeCustomer && !String.IsNullOrEmpty(invoice.CustomerName) ? invoice.CustomerName : invoice.Actor?.Customer?.Name);
            sb.Replace(startToken + "CustomerNr" + endToken, invoice.Actor?.Customer?.CustomerNr);
            return sb.ToString();
        }

        public ActionResult UpdateEmailStatusFromCommunicator(List<CommunicatorMessageEvent> messages)
        {
            var actionResult = new ActionResult();
            using (CompEntities entities = new CompEntities())
            {
                foreach (var message in messages)
                {
                    if (message.CommunicatorMetaData.Any())
                    {
                        var invoiceDistributionIdStr = message.CommunicatorMetaData.FirstOrDefault(x => x.Type == CommunicatorMetaDataType.InvoiceDistributionId)?.Value;

                        if (!string.IsNullOrEmpty(invoiceDistributionIdStr) && int.TryParse(invoiceDistributionIdStr, out int invoiceDistributionId))
                        {
                            var item = GetDistributionItem(entities, invoiceDistributionId);
                            var statusType = TermGroup_ReportPrintoutStatus.Unknown;

                            if (item != null)
                            {
                                var msg = $"{message.Event}:{message.Timestamp.ToString("yyyyMMdd")}: {message.Message} ";
                                switch (message.Event)
                                {
                                    case CommunicatorMessageEventType.Processed:
                                        statusType = TermGroup_ReportPrintoutStatus.Queued;
                                        break;
                                    case CommunicatorMessageEventType.Delivered:
                                        statusType = TermGroup_ReportPrintoutStatus.Delivered;
                                        break;
                                    case CommunicatorMessageEventType.Bounce:
                                    case CommunicatorMessageEventType.Dropped:
                                        statusType = TermGroup_ReportPrintoutStatus.Error;
                                        break;
                                }

                                var updateResult = UpdateInvoiceEmailStatus(entities, item.OriginId, statusType, msg, invoiceDistributionId);
                                if (!updateResult.Success)
                                {
                                    base.LogError(updateResult.ErrorMessage);
                                    actionResult.Strings.Add(updateResult.ErrorMessage);
                                }
                            }
                        }
                    }
                }

            }

            return actionResult;
        }

        #endregion

        #region einvoice

        public ActionResult EinvoiceMessageSent(TermGroup_EDistributionType type, CompEntities entities, CustomerInvoice invoice, string Guid, bool waitingForConfirmation)
        {
            int distributionId = EnsureDistributionItem(invoice.InvoiceId, type, waitingForConfirmation ? TermGroup_EDistributionStatusType.PendingInPlatform : TermGroup_EDistributionStatusType.Sent, Guid, false);
            var result = UpdateElectronicStatusIcon(entities, invoice);

            if (result.Success)
                result.IntegerValue = distributionId;

            return result;
        }
        public ActionResult EinvoiceMessageSent(TermGroup_EDistributionType type, int invoiceId, string Guid, bool waitingForConfirmation)
        {
            int distributionId = EnsureDistributionItem(invoiceId,type, waitingForConfirmation ? TermGroup_EDistributionStatusType.PendingInPlatform : TermGroup_EDistributionStatusType.Sent, Guid, false);
            SoeStatusIcon newStatus = type == TermGroup_EDistributionType.Finvoice ? SoeStatusIcon.DownloadEinvoice : SoeStatusIcon.None;
            
            var result = UpdateElectronicStatusIcon(invoiceId, newStatus);

            if (result.Success)
                result.IntegerValue = distributionId;

            return result;
        }

        public ActionResult EinvoiceMessageSent(TermGroup_EDistributionType type, int invoiceId, string Guid, TermGroup_EDistributionStatusType status, string msg)
        {
            var result = new ActionResult();
            int distributionId = EnsureDistributionItem(invoiceId, type, status, Guid, false, msg);
            if (distributionId == 0)
            {
                return new ActionResult(false);
            }

            if (status != TermGroup_EDistributionStatusType.Error && 
                type != TermGroup_EDistributionType.Fortnox && 
                type != TermGroup_EDistributionType.VismaEAccounting)
            {
                result = UpdateElectronicStatusIcon(invoiceId);
            }

            if (result.Success)
                result.IntegerValue = distributionId;

            return result;
        }

        private ActionResult UpdateElectronicStatusIcon(CompEntities entities, CustomerInvoice customerInvoice, SoeStatusIcon newStatus = SoeStatusIcon.None)
        {
            if (customerInvoice == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "CustomerInvoice");

            var status = new SoeStatusIcon();
            status = (SoeStatusIcon)customerInvoice.StatusIcon;
            if(newStatus == SoeStatusIcon.DownloadEinvoice)
                status |= SoeStatusIcon.DownloadEinvoice;
            else if (!status.HasFlag(SoeStatusIcon.ElectronicallyDistributed))
                status |= SoeStatusIcon.ElectronicallyDistributed;

            customerInvoice.StatusIcon = (int)status;

            return SaveChanges(entities);
        }

        private ActionResult UpdateElectronicStatusIcon(int invoiceId, SoeStatusIcon newStatus = SoeStatusIcon.None)
        {
            var im = new InvoiceManager(this.parameterObject);
            using (CompEntities entities = new CompEntities())
            {
                CustomerInvoice customerInvoice = im.GetCustomerInvoice(entities, invoiceId, true, false, false, false, false, false, false, false, false, false, false, false);
                return UpdateElectronicStatusIcon(entities, customerInvoice, newStatus);
            }
        }

        public InvoiceDistribution GetActiveEntry(int originId, TermGroup_EDistributionType type)
        {
            using (CompEntities entities = new CompEntities())
            {
                return (from i in entities.InvoiceDistribution
                        where i.OriginId == originId &&
                        i.DistributionType == (int)type &&
                        i.State == (int)SoeEntityState.Active
                        select i).FirstOrDefault();
            }
        }

        public InExchangeDTO GetInExchangeEntry(int actorCompanyId, int invoiceId)
        {
            using (CompEntities entities = new CompEntities())
            {
                var invDistr = GetDistributionItem(entities, invoiceId, TermGroup_EDistributionType.Inexchange);
                if (invDistr != null)
                {
                    return new InExchangeDTO
                    {
                        Created = invDistr.Created,
                        InvoiceId = invDistr.OriginId,
                        Message = invDistr.Msg,
                        State = invDistr.State,
                        InvoiceState = invDistr.DistributionStatus
                    };
                }
            }

            return null;
        }

        public ActionResult GetAndUpdateStatusesFromInExchange(bool releaseMode)
        {
            var result = new ActionResult(true);
            bool transactionComplete = false;
            var lastFetchDate = SettingManager.GetDateSetting(SettingMainType.Application, (int)CompanySettingType.InExchangeLastStatusFetchDate, 0, 0, 0);
            if (lastFetchDate == DateTime.MinValue)
            {
                lastFetchDate = DateTime.Today.AddDays(-14);
            }

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    var actorCompanys = GetCompanysInActiveDistributionItems(entities, TermGroup_EDistributionType.Inexchange);

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        if (actorCompanys.Count > 0)
                        {
                            foreach (int i in actorCompanys)
                            {
                                var documentStatusList = InExchangeConnector.GetStatusList(i, releaseMode, lastFetchDate);
                                if (documentStatusList.Any())
                                {
                                    result = UpdateStateFromInExchangeEntrys(entities, documentStatusList, i);

                                    if (!result.Success)
                                    {
                                        return new ActionResult(false, (int)ActionResultSave.NothingSaved, "Fel inträffade när status skulle uppdateras från InExchange. Actorcompanyid: " + i);
                                    }

                                    result = SaveChanges(entities);
                                    if (!result.Success)
                                    {
                                        return new ActionResult(false, (int)ActionResultSave.NothingSaved, "Fel inträffade när status skulle sparas från InExchange. Actorcompanyid: " + i);
                                    }
                                }
                            }
                        }

                        if (result.Success)
                        {
                            transaction.Complete();
                            transactionComplete = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                }
                finally
                {
                    if (result.Success)
                    {
                        if (transactionComplete)
                        {
                            SettingManager.UpdateInsertDateSetting(SettingMainType.Application, (int)CompanySettingType.InExchangeLastStatusFetchDate, DateTime.Now.Date, 0, 0, 0);
                        }
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            return result;
        }

        public ActionResult UpdateStateFromInExchangeEntrys(CompEntities entities, List<InExchangeAPIDocumentStatus> statusList, int actorCompanyId)
        {
            var result = new ActionResult();
            int invoiceId;

            var guids = statusList.Select(x => x.id).ToList();
            var inExchangeList = GetDistributionItems(entities, TermGroup_EDistributionType.Inexchange, guids, TermGroup_EDistributionStatusType.Unknown, actorCompanyId);

            if (!inExchangeList.IsNullOrEmpty() && statusList.Count > 0)
            {
                foreach (var status in statusList)
                {
                    InvoiceDistribution invoiceDistribution = null;

                    if (!string.IsNullOrEmpty(status.erpDocumentId) && int.TryParse(status.erpDocumentId, out invoiceId) )
                    {
                        invoiceDistribution = inExchangeList.FirstOrDefault(x => x.OriginId == invoiceId);
                    }

                    if (invoiceDistribution == null)
                    {
                        invoiceDistribution = inExchangeList.FirstOrDefault(x => x.Guid == status.id);
                    }

                    if (invoiceDistribution != null)
                    {
                        invoiceDistribution.DistributionStatus = (short)status.SOEInExchangeStatusType;
                        if ((invoiceDistribution.DistributionStatus == (short)InExchangeStatusType.Error) || (invoiceDistribution.DistributionStatus == (short)InExchangeStatusType.Stopped))
                        {
                            invoiceDistribution.Msg = $"{status.error?.status}: {status.error?.message}";
                            SetModifiedProperties(invoiceDistribution);
                        }
                        else if (invoiceDistribution.DistributionStatus == (short)InExchangeStatusType.Unknown)
                        {
                            invoiceDistribution.Msg = $"{status.currentStatus.time:yyyy-MM-dd hh:mm} : {status.currentStatus.status}";
                            SetModifiedProperties(invoiceDistribution);
                        }
                        else
                        {
                            invoiceDistribution.Msg = $"{status.currentStatus.time:yyyy-MM-dd hh:mm}";
                            SetModifiedProperties(invoiceDistribution);
                        }
                    }
                }
            }

            return result;
        }

        #endregion

        #region Finvoice

        public ActionResult UpdateFinvoiceFromFeedback(int actorCompanyId, string fileContent)
        {
            var result = new ActionResult();

            XDocument xdoc = XDocument.Parse(fileContent);

            var nameSpace = xdoc.Root.Name.Namespace;
            var rootName = xdoc.Root.Name.ToString();
            if (nameSpace == null || !(nameSpace.NamespaceName == "http://www.pankkiyhdistys.fi/verkkolasku/finvoice/finvoiceack.xsd" || rootName == "Finvoiceack") )
            {
                return new ActionResult(8176, "Kan inte läsa från XML fil");
            }

            var ebNameSpace = xdoc.Root?.GetNamespaceOfPrefix("eb");

            var messageId = xdoc.Descendants(ebNameSpace + "MessageId").FirstOrDefault()?.Value;
            var reasonText = xdoc.Descendants(nameSpace + "Text").FirstOrDefault()?.Value;
            var code = xdoc.Descendants(nameSpace + "Code").FirstOrDefault()?.Value;

            var errorMessage = code + ": " + reasonText;

            var error = xdoc.Descendants("Error").FirstOrDefault();
            if (error != null)
            {
                reasonText = error.Descendants("Text").FirstOrDefault()?.Value;
                code = error.Descendants("Code").FirstOrDefault()?.Value;
                var location = error.Descendants("Location").FirstOrDefault()?.Value;
                errorMessage += $"\n {code}:{reasonText} ({location})";
            }

            int invoiceId;
            var invoiceIdToExtract = messageId.Contains('_') ? messageId.Split('_').ElementAtOrDefault(1) : messageId;

            if (int.TryParse(invoiceIdToExtract, out invoiceId))
            {
                result = UpdateDistributionItem(invoiceId, TermGroup_EDistributionType.Finvoice, TermGroup_EDistributionStatusType.Error, errorMessage);
            }

            return result;
        }

        #endregion

        #region helpers
        public List<Tuple<int, string, byte[]>> GetInvoiceDocuments(int invoiceId, int actorCompanyId, SoeOriginType type, List<int> onlyTheseIds, bool useOriginalFileNames, bool addSuffix = true)
        {
            var invoice = InvoiceManager.GetCustomerInvoice(invoiceId);
            return GetInvoiceDocuments(invoice, actorCompanyId, type, onlyTheseIds, useOriginalFileNames, addSuffix);
        }
        public List<Tuple<int, string, byte[]>> GetInvoiceDocuments(CustomerInvoice invoice, int actorCompanyId, SoeOriginType type, List<int> onlyTheseIds, bool useOriginalFileNames, bool addSuffix = true)
        {
            var attachments = new List<Tuple<int, string, byte[]>>();
            var gem = new GeneralManager(null);
            var grm = new GraphicsManager(null);
            var invoiceId = invoice?.InvoiceId ?? 0;

            /*
             var entityType = SoeEntityType.None;

            switch (type)
            {
                case SoeOriginType.Order:
                    entityType = SoeEntityType.Order;
                    break;
                case SoeOriginType.CustomerInvoice:
                    entityType = SoeEntityType.CustomerInvoice;
                    break;
            }*/

            List<ImagesDTO> items = new List<ImagesDTO>();
            switch (type)
            {
                case SoeOriginType.CustomerInvoice:
                    var invoiceRecords = gem.GetDataStorageRecordsForCustomerInvoice(base.ActorCompanyId, null, invoiceId, SoeEntityType.None, new List<SoeDataStorageRecordType> { SoeDataStorageRecordType.OrderInvoiceFileAttachment, SoeDataStorageRecordType.InvoicePdf, SoeDataStorageRecordType.InvoiceBitmap, SoeDataStorageRecordType.OrderInvoiceSignature }, invoice != null ? invoice.AddSupplierInvoicesToEInvoices : false, null, true);
                    foreach (var record in invoiceRecords)
                    {
                        if (!items.Any(r => r.ImageId == record.ImageId && r.SourceType == record.SourceType))
                            items.Add(record);
                    }
                    break;
                case SoeOriginType.Order:
                    var typeNameSignature = TermCacheManager.Instance.GetText(7465, 1, "Signatur");
                    var typeNameSupplierInvoice = TermCacheManager.Instance.GetText(31, 1, "Leverantörsfaktura");
                    var typeNameEdi = TermCacheManager.Instance.GetText(7467, 1, "EDI");

                    // Add head
                    var signatures = grm.GetImages(base.ActorCompanyId, SoeEntityImageType.OrderInvoiceSignature, SoeEntityType.Order, invoiceId).ToDTOs(true, typeNameSignature, true).ToList();
                    foreach (var signature in signatures)
                    {
                        if (!items.Any(r => r.ImageId == signature.ImageId && r.SourceType == signature.SourceType))
                            items.Add(signature);
                    }

                    var rows = grm.GetImagesFromOrderRows(base.ActorCompanyId, invoiceId, true, null, null, false, addToDistribution: invoice?.AddSupplierInvoicesToEInvoices ?? false);
                    foreach (var row in rows)
                    {
                        if (!items.Any(r => r.ImageId == row.ImageId && r.SourceType == row.SourceType))
                        {
                            row.ConnectedTypeName = typeNameSupplierInvoice;
                            items.Add(row);
                        }
                    }

                    if (invoice != null && invoice.ProjectId.HasValue)
                    {
                        var links = grm.GetImagesFromLinkToProject(base.ActorCompanyId, invoiceId, invoice.ProjectId.Value, true, null, false, addToDistribution: invoice?.AddSupplierInvoicesToEInvoices ?? false);
                        foreach (var link in links)
                        {
                            if (!items.Any(r => r.ImageId == link.ImageId && r.SourceType == link.SourceType))
                            {
                                link.ConnectedTypeName = typeNameSupplierInvoice;
                                items.Add(link);
                            }
                        }
                    }

                    var records = gem.GetDataStorageRecordsForCustomerInvoice(base.ActorCompanyId, null, invoiceId, SoeEntityType.None, new List<SoeDataStorageRecordType> { SoeDataStorageRecordType.OrderInvoiceFileAttachment, SoeDataStorageRecordType.OrderInvoiceSignature }, invoice?.AddSupplierInvoicesToEInvoices ?? false, null, true);
                    foreach (var record in records)
                    {
                        if (!items.Any(r => r.ImageId == record.ImageId && r.SourceType == record.SourceType))
                        {
                            items.Add(record);
                        }
                    }

                    // Add children
                    var mappings = InvoiceManager.GetChildInvoices(invoice.InvoiceId);
                    foreach (var mapping in mappings)
                    {
                        var typeNameChild = TermCacheManager.Instance.GetText(7469, 1, "underorder");

                        var childSignatures = grm.GetImages(base.ActorCompanyId, SoeEntityImageType.OrderInvoiceSignature, SoeEntityType.Order, mapping.ChildInvoiceId).ToDTOs(true, typeNameSignature + " (" + typeNameChild + ")", true).ToList();
                        foreach (var signature in childSignatures)
                        {
                            items.Add(signature);
                        }

                        var childRows = grm.GetImagesFromOrderRows(base.ActorCompanyId, mapping.ChildInvoiceId, true, typeNameEdi + " (" + typeNameChild + ")", null, false, mapping.MainInvoiceId);
                        foreach (var row in childRows)
                        {
                            if (!items.Any(r => r.ImageId == row.ImageId && r.SourceType == row.SourceType))
                            {
                                row.ConnectedTypeName = typeNameSupplierInvoice + " (" + typeNameChild + ")";
                                items.Add(row);
                            }
                        }

                        if (invoice != null && invoice.ProjectId.HasValue)
                        {
                            var childLinks = grm.GetImagesFromLinkToProject(base.ActorCompanyId, mapping.ChildInvoiceId, invoice.ProjectId.Value, true, null, false, mapping.MainInvoiceId);
                            foreach (var link in childLinks)
                            {
                                if (!items.Any(r => r.ImageId == link.ImageId && r.SourceType == link.SourceType))
                                {
                                    link.ConnectedTypeName = typeNameSupplierInvoice + " (" + typeNameChild + ")";
                                    items.Add(link);
                                }
                            }
                        }

                        var childRecords = gem.GetDataStorageRecordsForCustomerInvoice(base.ActorCompanyId, null, mapping.ChildInvoiceId, SoeEntityType.None, new List<SoeDataStorageRecordType> { SoeDataStorageRecordType.OrderInvoiceFileAttachment, SoeDataStorageRecordType.OrderInvoiceSignature }, invoice?.AddSupplierInvoicesToEInvoices ?? false, mapping.MainInvoiceId, true);
                        foreach (var record in childRecords)
                        {
                            if (!items.Any(r => r.ImageId == record.ImageId && r.SourceType == record.SourceType))
                            {
                                items.Add(record);
                            }
                        }
                    }
                    break;
                default:
                    var offerContractRecords = gem.GetDataStorageRecordsForCustomerInvoice(base.ActorCompanyId, null, invoiceId, SoeEntityType.None, new List<SoeDataStorageRecordType> { SoeDataStorageRecordType.OrderInvoiceFileAttachment }, invoice?.AddSupplierInvoicesToEInvoices ?? false, null, true);
                    foreach (var record in offerContractRecords)
                    {
                        if (!items.Any(r => r.ImageId == record.ImageId && r.SourceType == record.SourceType))
                        {
                            items.Add(record);
                        }
                    }
                    break;
            }
            //var items = grm.GetImages(actorCompanyId, SoeEntityImageType.OrderInvoice, entityType, invoiceId).ToDTOs(false).ToList();
            //var allItems = GraphicsManager.GetImages(actorCompanyId, SoeEntityImageType.OrderInvoice, SoeEntityType.CustomerInvoice, invoiceId).ToDTOs(true).ToList();
            //items.AddRange(gem.GetDataStorageRecords(actorCompanyId, null, invoiceId, SoeEntityType.None, SoeDataStorageRecordType.OrderInvoiceFileAttachment,false,true).ToImagesDTOs(true));
            //items.AddRange(grm.GetImagesFromOrderRows(actorCompanyId, invoiceId, true));

            if (items.Any() )
            {
                string attachementName;
                foreach (var image in items)
                {
                    if (addSuffix)
                    {
                        if (image.FormatType == ImageFormatType.JPG)
                            attachementName = useOriginalFileNames ? FixFileName(FileUtil.AddSuffix(image.FileName, "_" + image.ImageId.ToString() + invoiceId.ToString())) : image.ImageId.ToString() + invoiceId.ToString() + ".jpg";
                        else if (image.FormatType == ImageFormatType.PDF)
                            attachementName = useOriginalFileNames ? FixFileName(FileUtil.AddSuffix(image.FileName, "_" + image.ImageId.ToString() + invoiceId.ToString())) : image.ImageId.ToString() + invoiceId.ToString() + ".pdf";
                        else if (image.FormatType == ImageFormatType.PNG)
                            attachementName = useOriginalFileNames ? FixFileName(FileUtil.AddSuffix(image.FileName, "_" + image.ImageId.ToString() + invoiceId.ToString())) : image.ImageId.ToString() + invoiceId.ToString() + ".png";
                        else if (image.FormatType == ImageFormatType.NONE)
                            //File name is used more then once, add imageid to ensure uniqunes...
                            attachementName = FixFileName(FileUtil.AddSuffix(image.FileName, "_" + image.ImageId.ToString()));
                        else //Format unknown
                            continue;
                    }
                    else
                    {
                        attachementName = image.FileName;
                    }

                    if (onlyTheseIds == null)
                    {
                        if (image.IncludeWhenDistributed == true)
                            attachments.Add(new Tuple<int, string, byte[]>(image.InvoiceAttachmentId.HasValue ? image.InvoiceAttachmentId.Value : 0, attachementName, image.Image));
                    }
                    else if (onlyTheseIds.Any() && onlyTheseIds.Contains(image.ImageId))
                    {
                        attachments.Add(new Tuple<int, string, byte[]>(image.InvoiceAttachmentId.HasValue ? image.InvoiceAttachmentId.Value : 0, attachementName, image.Image));
                    }
                }
            }

            return attachments;
        }

        private static string FixFileName(string fileName)
        {
            // return  Regex.Replace(fileName, "[ÅÄÖåäö ]", "_");  //fileName.Replace("ö", "_").Replace(" ", "_");
            return Regex.Replace(fileName, "[^\u0000-\u007F]", "_").Replace(" ", "_");
        }

        #endregion
    }
}
