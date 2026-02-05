using SoftOne.Soe.Business.Core.Banker;
using SoftOne.Soe.Business.Core.ManagerWrappers;
using SoftOne.Soe.Business.Core.Reporting.Billing;
using SoftOne.Soe.Business.Core.Reports;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.API.Fortnox;
using SoftOne.Soe.Business.Util.API.InExchange;
using SoftOne.Soe.Business.Util.API.Intrum;
using SoftOne.Soe.Business.Util.API.Shared;
using SoftOne.Soe.Business.Util.API.VismaEAccounting;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Business.Util.Finvoice;
using SoftOne.Soe.Business.Util.PeppolBilling;
using SoftOne.Soe.Business.Util.Svefaktura;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.DTO.Reports;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Transactions;
using System.Xml;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Core
{
    public class ElectronicInvoiceMananger : ManagerBase
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ElectronicInvoiceMananger(ParameterObject parameterObject) : base(parameterObject) { }

        public ActionResult CreateEInvoice(int actorCompanyId, int userId, int invoiceId, bool download, bool overrideWarnings = false)
        {
            var result = new ActionResult(false);

            var eInvoiceFormat = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingEInvoiceFormat, 0, actorCompanyId, 0);
            bool createSveFakturaFile = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingSveFakturaToFile, 0, actorCompanyId, 0);

            if (eInvoiceFormat == (int)TermGroup_EInvoiceFormat.Finvoice || eInvoiceFormat == (int)TermGroup_EInvoiceFormat.Finvoice2 || eInvoiceFormat == (int)TermGroup_EInvoiceFormat.Finvoice3)
            {
                bool singleInvoicePerFile = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingFinvoiceSingleInvoicePerFile, 0, actorCompanyId, 0);
                bool hasSendFinvoicePermission = FeatureManager.HasRolePermission(Feature.Billing_Invoice_Invoices_Edit_EInvoice_SendFinvoice, Permission.Modify, RoleId, actorCompanyId);
                if (!hasSendFinvoicePermission || download)
                    result = CreateFinvoiceCustomerInvoiceExportFile(null, actorCompanyId, userId, invoiceId, singleInvoicePerFile, overrideWarnings);
                else
                    result = SendFinvoice(new List<int>() { invoiceId }, actorCompanyId, singleInvoicePerFile, overrideWarnings);
            }
            else if (eInvoiceFormat == (int)TermGroup_EInvoiceFormat.Svefaktura && createSveFakturaFile)
            {
                result = CreateSveFakturaExportFile(null, actorCompanyId, userId);
            }
            else if (eInvoiceFormat == (int)TermGroup_EInvoiceFormat.Svefaktura || eInvoiceFormat == (int)TermGroup_EInvoiceFormat.SvefakturaTidbok)
            {
                return CreateEInvoiceFTP(new List<int>() { invoiceId }, actorCompanyId, userId);
            }
            else if (eInvoiceFormat == (int)TermGroup_EInvoiceFormat.SvefakturaAPI)
            {
                return CreateEInvoiceAPI((TermGroup_EInvoiceFormat)eInvoiceFormat, new List<int>() { invoiceId }, actorCompanyId, userId);
            }
            else if (eInvoiceFormat == (int)TermGroup_EInvoiceFormat.Intrum)
            {
                return CreateIntrumInvoice(new List<int>() { invoiceId }, actorCompanyId, false);
            }
            else
            {
                return new ActionResult(false, 0, GetText(7433, "Inställning för e-faktura format saknas eller är ogiltig"));
            }
            return result;
        }

        public bool WillExportEInvoiceAsFile(int actorCompanyId)
        {
            /**
             * Find out if the company will export the e-invoice as a file,
             * or if it is work that can be offloaded.
             */

            var createSveFakturaFile = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingSveFakturaToFile, 0, actorCompanyId, 0);
            var eInvoiceFormat = (TermGroup_EInvoiceFormat)SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingEInvoiceFormat, 0, actorCompanyId, 0);
            var hasSendFinvoicePermission = FeatureManager.HasRolePermission(Feature.Billing_Invoice_Invoices_Edit_EInvoice_SendFinvoice, Permission.Modify, RoleId, actorCompanyId);
            if (FinvoiceBase.IsFinvoice(eInvoiceFormat))
            {
                if (!hasSendFinvoicePermission)
                    return true;
            }

            if (eInvoiceFormat == TermGroup_EInvoiceFormat.Svefaktura && createSveFakturaFile)
                return true;

            return false;
        }
        public ActionResult CreateEInvoice(List<CustomerInvoiceGridDTO> items, int actorCompanyId, int userId, bool download = false, bool overrideWarnings = false)
        {
            ActionResult result = new ActionResult(false);

            int eInvoiceFormat = 0;
            eInvoiceFormat = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingEInvoiceFormat, 0, actorCompanyId, 0);
            bool createSveFakturaFile = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingSveFakturaToFile, 0, actorCompanyId, 0);

            if (eInvoiceFormat == (int)TermGroup_EInvoiceFormat.Finvoice || eInvoiceFormat == (int)TermGroup_EInvoiceFormat.Finvoice2 || eInvoiceFormat == (int)TermGroup_EInvoiceFormat.Finvoice3)
            {
                bool singleInvoicePerFile = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingFinvoiceSingleInvoicePerFile, 0, actorCompanyId, 0);
                bool hasSendFinvoicePermission = FeatureManager.HasRolePermission(Feature.Billing_Invoice_Invoices_Edit_EInvoice_SendFinvoice, Permission.Modify, RoleId, actorCompanyId);
                if (!hasSendFinvoicePermission || download)
                    result = CreateFinvoiceCustomerInvoiceExportFile(items, actorCompanyId, userId, 0, singleInvoicePerFile, overrideWarnings);
                else
                    result = SendFinvoice(items.Select(x => x.CustomerInvoiceId).ToList(), actorCompanyId, singleInvoicePerFile, overrideWarnings);
            }
            else if (eInvoiceFormat == (int)TermGroup_EInvoiceFormat.Svefaktura && createSveFakturaFile)
            {
                result = CreateSveFakturaExportFile(items, actorCompanyId, userId);
            }
            else if (eInvoiceFormat == (int)TermGroup_EInvoiceFormat.Svefaktura || eInvoiceFormat == (int)TermGroup_EInvoiceFormat.SvefakturaTidbok)
            {
                return CreateEInvoiceFTP(items.Select(x => x.CustomerInvoiceId).ToList(), actorCompanyId, userId);
            }
            else if (eInvoiceFormat == (int)TermGroup_EInvoiceFormat.SvefakturaAPI)
            {
                result = CreateEInvoiceAPI((TermGroup_EInvoiceFormat)eInvoiceFormat, items.Select(x => x.CustomerInvoiceId).ToList(), actorCompanyId, userId);
            }
            else if (eInvoiceFormat == (int)TermGroup_EInvoiceFormat.Intrum)
            {
                return CreateIntrumInvoice(items.Select(x => x.CustomerInvoiceId).ToList(), actorCompanyId, false);
            }
            else
            {
                return new ActionResult(false, 0, GetText(7433, "Inställning för e-faktura format saknas eller är ogiltig"));
            }

            return result;
        }

        private ActionResult CreateEInvoiceFTP(List<int> items, int actorCompanyId, int userId)
        {
            List<int> invoicesWithProject = new List<int>();
            List<Tuple<int, string, byte[]>> invoiceAttachments = new List<Tuple<int, string, byte[]>>();
            List<string> invoiceAttachementNames = new List<string>();

            bool apiRegistered = SettingManager.GetCompanyBoolSetting(CompanySettingType.InExchangeAPISendRegistered);
            if (apiRegistered)
            {
                return new ActionResult(false, 0, "Försöker skicka till Inexchange via FTP fast skicka API är aktiverat");
            }

            bool releaseMode = SettingManager.GetBoolSetting(SettingMainType.Application, (int)ApplicationSettingType.ReleaseMode, 0, 0, 0);
#if DEBUG
            releaseMode = false;
#endif

            int companySettingTypeInExchangeFtpUsername = 0, companySettingTypeInExchangeFtpPassword = 0, companySettingTypeInExchangeFtpAddress = 0;

            var result = new ActionResult();

            if (releaseMode)
            {
                //Get CompanySettingTypes of production ftp-settings
                companySettingTypeInExchangeFtpUsername = (int)CompanySettingType.InExchangeFtpUsername;
                companySettingTypeInExchangeFtpPassword = (int)CompanySettingType.InExchangeFtpPassword;
                companySettingTypeInExchangeFtpAddress = (int)CompanySettingType.InExchangeFtpAddress;

            }
            else
            {
                //Get CompanySettingTypes of test ftp-settings
                companySettingTypeInExchangeFtpUsername = (int)CompanySettingType.InExchangeFtpUsernameTest;
                companySettingTypeInExchangeFtpPassword = (int)CompanySettingType.InExchangeFtpPasswordTest;
                companySettingTypeInExchangeFtpAddress = (int)CompanySettingType.InExchangeFtpAddressTest;
            }

            //Get ftp-settings primarily from company's settings, secondarily from application's settings
            var settingUserName = SettingManager.GetUserCompanySetting(SettingMainType.Company, companySettingTypeInExchangeFtpUsername, userId, actorCompanyId, 0);
            if (settingUserName == null || settingUserName.StrData == string.Empty)
                settingUserName = SettingManager.GetUserCompanySetting(SettingMainType.Company, companySettingTypeInExchangeFtpUsername, userId, 2, 0);

            var settingPassWd = SettingManager.GetUserCompanySetting(SettingMainType.Company, companySettingTypeInExchangeFtpPassword, userId, actorCompanyId, 0);
            if (settingPassWd == null || settingPassWd.StrData == string.Empty)
                settingPassWd = SettingManager.GetUserCompanySetting(SettingMainType.Company, companySettingTypeInExchangeFtpPassword, userId, 2, 0);

            var settingFTPAddress = SettingManager.GetUserCompanySetting(SettingMainType.Company, companySettingTypeInExchangeFtpAddress, userId, actorCompanyId, 0);
            if (settingFTPAddress == null || settingFTPAddress.StrData == string.Empty)
                settingFTPAddress = SettingManager.GetUserCompanySetting(SettingMainType.Company, companySettingTypeInExchangeFtpAddress, userId, 2, 0);

            string ftpUserName = settingUserName.StrData;
            string PassWd = settingPassWd.StrData;
            string ftpAddress = settingFTPAddress.StrData;

            try
            {
                foreach (var customerInvoiceId in items)
                {

                    //other attachements
                    CustomerInvoice invoice = InvoiceManager.GetCustomerInvoice(customerInvoiceId, true);

                    // Check if Orgn Exits
                    if (invoice.Customer != null && string.IsNullOrEmpty(invoice.Customer.OrgNr))
                    {
                        result.Success = false;

                        result.Value = GetText(11, "Org.nr på företaget är ej angivet");
                        return result;
                    }


                    if (invoice.AddAttachementsToEInvoice)
                    {
                        invoiceAttachments = InvoiceDistributionManager.GetInvoiceDocuments(invoice.InvoiceId, actorCompanyId, (SoeOriginType)invoice.Origin.Type, null, true);

                        foreach (var image in invoiceAttachments)
                        {
                            invoiceAttachementNames.Add(image.Item2);

                            Uri tprUri = new Uri(ftpAddress.ToString() + "/attachment/" + image.Item2);
                            //Send it with ftp
                            FtpUtility.UploadData(tprUri, image.Item3, ftpUserName, PassWd);
                        }
                    }

                    Uri uri = new Uri(ftpAddress.ToString() + "/" + invoice.InvoiceNr.ToString() + customerInvoiceId.ToString() + ".xml");

                    result = CreatePeppolSveFaktura(actorCompanyId, userId, customerInvoiceId, invoice.InvoiceNr.ToString() + customerInvoiceId.ToString() + ".pdf", invoiceAttachementNames, null, TermGroup_EInvoiceFormat.Svefaktura, out byte[] svefakt);
                    if (!result.Success)
                    {
                        return result;
                    }
                    //Send it with ftp
                    FtpUtility.UploadData(uri, svefakt, ftpUserName, PassWd);

                    result = InvoiceDistributionManager.EinvoiceMessageSent(TermGroup_EDistributionType.Inexchange, customerInvoiceId, null, TermGroup_EDistributionStatusType.Sent, "FTP");

                    if (result.Success)
                    {
                        foreach (var invoiceAttachment in invoiceAttachments.Where(a => a.Item1 > 0))
                        {
                            InvoiceAttachmentManager.ConnectInvoiceAttachmentToDistribution(invoiceAttachment.Item1, result.IntegerValue, true);
                        }
                    }

                    invoiceAttachementNames.Clear();
                }
            }
            catch (Exception e)
            {
                result = new ActionResult(e, GetText(6023, "Ett fel uppstod vid utskick av elektronisk faktura:"));
                base.LogError(e, this.log);
            }

            result.Keys = invoicesWithProject;

            return result;
        }

        private ActionResult GetInexchangeCompanyId(string loginToken, bool releaseMode, int contactGLNEcomId, int actorCustomerId, int actorCompanyId, InExchangeApiSendInfo sendInfo)
        {
            if (string.IsNullOrEmpty(loginToken))
                return new ActionResult("Empty token");

            var ids = new List<string>();
            var searchResult = new ActionResult("");
            if (!string.IsNullOrEmpty(sendInfo.recipientGLN))
            {
                ids = ActorManager.GetCompanyExternalCodeValues(TermGroup_CompanyExternalCodeEntity.CustomerContact_InexchangeCompanyId, contactGLNEcomId, actorCompanyId);
            }
            else if (!string.IsNullOrEmpty(sendInfo.recipientOrgNo))
            {
                ids = ActorManager.GetCompanyExternalCodeValues(TermGroup_CompanyExternalCodeEntity.Customer_InexchangeCompanyId, actorCustomerId, actorCompanyId);
            }

            if (ids.Count == 1)
            {
                return new ActionResult { Success = true, StringValue = ids.First() };
            }

            #region Search for matching GLN number
            if (!string.IsNullOrEmpty(sendInfo.recipientGLN))
            {
                searchResult = InExchangeConnector.GetInexchangeBuyerReciverCompanyId(loginToken, releaseMode, actorCompanyId, new InexchangeBuyerPartyLookup { PartyId = actorCompanyId.ToString(), GLN = sendInfo.recipientGLN });
                if (searchResult.Success)
                {
                    ActorManager.SaveCompanyExternalCode(new CompanyExternalCodeDTO
                    {
                        Entity = TermGroup_CompanyExternalCodeEntity.CustomerContact_InexchangeCompanyId,
                        ExternalCode = searchResult.StringValue,
                        ActorCompanyId = actorCompanyId,
                        RecordId = contactGLNEcomId
                    }, actorCompanyId);

                    return searchResult;
                }
            }
            #endregion

            #region Search for matching Org number
            if (!string.IsNullOrEmpty(sendInfo.recipientOrgNo))
            {
                searchResult = InExchangeConnector.GetInexchangeBuyerReciverCompanyId(loginToken, releaseMode, actorCompanyId, new InexchangeBuyerPartyLookup { PartyId = actorCompanyId.ToString(), OrgNo = sendInfo.recipientOrgNo });
                if (searchResult.Success)
                {
                    ActorManager.SaveCompanyExternalCode(new CompanyExternalCodeDTO
                    {
                        Entity = TermGroup_CompanyExternalCodeEntity.Customer_InexchangeCompanyId,
                        ExternalCode = searchResult.StringValue,
                        ActorCompanyId = actorCompanyId,
                        RecordId = actorCustomerId
                    }, actorCompanyId);

                    return searchResult;
                }
            }
            #endregion

            if (!searchResult.Success && searchResult.IntegerValue > 1)
            {
                searchResult.ErrorMessage = GetText(7757, "Kunden är inte upplagd i nätverket för e-faktura hantering: " + sendInfo.recipientName + ":" + sendInfo.recipientGLN + " : " + sendInfo.recipientOrgNo) + "\n" +
                                            GetText(7792, "Flera matchande e-faktura mottagare hittades vid sökning i Inexchange") + "\n" + searchResult.ErrorMessage;
            }
            else
            {
                searchResult = new ActionResult { Success = false, ErrorMessage = GetText(7757, "Kunden är inte upplagd i nätverket för e-faktura hantering: " + sendInfo.recipientName + ":" + sendInfo.recipientGLN + " : " + sendInfo.recipientOrgNo) };
            }

            return searchResult;
        }

        public ActionResult CreateEInvoiceAPI(TermGroup_EInvoiceFormat eInvoiceFormat, List<int> customerInvoiceIds, int actorCompanyId, int userId)
        {
            var result = new ActionResult(false);

            #region API

            var invoiceAttachments = new List<Tuple<int, string, byte[]>>();
            var attachments = new Dictionary<string, byte[]>();
            bool releaseModeAPI = ElectronicInvoiceMananger.GetInexchangeReleaseMode(SettingManager, actorCompanyId, userId);

            try
            {
                #region Items

                int defaultInvoiceRptTemplateId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingDefaultInvoiceTemplate, 0, actorCompanyId, 0);
                bool useInExchangeForAllInvoices = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingUseInExchangeDeliveryProvider, userId, actorCompanyId, 0, false);

                foreach (var customerInvoiceId in customerInvoiceIds)
                {
                    CustomerInvoice invoice = InvoiceManager.GetCustomerInvoice(customerInvoiceId, true, true, false, false, true, true, false, true, false, false, false, false);

                    if (invoice.InvoiceDeliveryProvider == (int)SoeInvoiceDeliveryProvider.Intrum)
                    {
                        result = CreateIntrumInvoice(new List<int> { customerInvoiceId }, actorCompanyId, false);
                        if (!result.Success)
                        {
                            return result;
                        }
                        continue;
                    }

                    Customer customer = invoice.Actor?.Customer;
                    if (customer == null)
                    {
                        result.ErrorMessage = GetText(7612, "Kund saknas.");
                        return result;
                    }

                    if (string.IsNullOrEmpty(invoice.InvoiceNr))
                    {
                        result.ErrorMessage = GetText(5896, "Fakturanummer saknas.");
                        return result;
                    }

                    var controlInvoice = InvoiceDistributionManager.GetActiveEntry(invoice.InvoiceId, TermGroup_EDistributionType.Inexchange);

                    if (controlInvoice != null && (controlInvoice.DistributionStatus == (int)TermGroup_EDistributionStatusType.PendingInPlatform || controlInvoice.DistributionStatus == (int)TermGroup_EDistributionStatusType.Sent))
                    {
                        result.ErrorMessage = GetText(7375, "Fakturan är redan skickad till InExchange!");
                        return result;
                    }

                    if (invoice.AddAttachementsToEInvoice)
                    {
                        invoiceAttachments = InvoiceDistributionManager.GetInvoiceDocuments(invoice, actorCompanyId, SoeOriginType.CustomerInvoice, null, false);
                        invoiceAttachments.ForEach(a => attachments.Add(a.Item2, a.Item3));

                        //checklist...
                        var checklists = InvoiceManager.GetInvoiceFromOrderCheckLists(actorCompanyId, invoice.InvoiceId).Where(x => x.AddAttachementsToEInvoice);
                        if (checklists.Any())
                        {
                            //if samlingsfaktura then it could be mulitiple orders....
                            foreach (var groupedCheckLists in checklists.GroupBy(x => x.RecordId))
                            {
                                var checkListDocuments = ChecklistManager.GetChecklistAsDocuments(ReportManager, ReportDataManager, groupedCheckLists.First().RecordId, groupedCheckLists.Select(x => x.ChecklistHeadRecordId).ToList(), actorCompanyId);
                                if (checkListDocuments.Any())
                                {
                                    attachments.AddRange(checkListDocuments);
                                }
                            }
                        }

                        foreach (var attachment in attachments)
                        {
                            if (attachment.Value == null || attachment.Value.Length == 0)
                            {
                                result.ErrorMessage = string.Format(GetText(7772, "Bilaga {0} innehåller ingen data"), attachment.Key);
                                return result;
                            }
                        }
                    }

                    var sendInfo = new InExchangeApiSendInfo { recipientOrgNo = customer.OrgNr, recipientName = customer.Name, country = "SE" };
                    string svefakturaFileName = $"{invoice.InvoiceNr}_{invoice.InvoiceId}.xml";
                    string pdfInvoiceFileName = $"{invoice.InvoiceNr}_{invoice.InvoiceId}_invoice.pdf";

                    var attachementNames = attachments.Keys.Select(x => x).ToList();
                    result = CreatePeppolSveFaktura(invoice, actorCompanyId, userId, invoice.InvoiceNr.ToString() + invoice.InvoiceId.ToString() + ".xml", attachementNames, sendInfo, eInvoiceFormat, out byte[] invoiceXML);
                    
                    if (!result.Success)
                    {
                        return result;
                    }
#if DEBUG
                    File.WriteAllBytes(@"c:\Temp\inexchange\invoicexml" + svefakturaFileName, invoiceXML);
#endif

                    attachments.Add(svefakturaFileName, invoiceXML);

                    //If timebook add to timebook-service
                    if (invoice.ProjectId.HasValue && invoice.PrintTimeReport && invoiceXML.Length > 0)
                    {
                        var timeBookResult = this.CreateTimebook(invoice.InvoiceId, invoice.IncludeOnlyInvoicedTime);
                        if (!timeBookResult.Success)
                            return new ActionResult("Timebook printout failed:" + timeBookResult.ErrorMessage);

                        if (timeBookResult.BinaryData != null && timeBookResult.BinaryData.Length > 0)
                        {
#if DEBUG
                            //File.WriteAllBytes(@"c:\Temp\inexchange\" + invoice.InvoiceNr.ToString() + invoice.InvoiceId.ToString() + "_timebook.pdf", timeBook);
#endif
                            attachments.Add($"{invoice.InvoiceNr}_{invoice.InvoiceId}_timebook.pdf", timeBookResult.BinaryData);
                        }
                        else
                        {
                            base.LogError($"CreateEInvoiceAPI createTimebook failed for invoice {invoice.InvoiceNr}");
                        }
                    }

                    if (invoiceXML.Length > 0)
                    {
                        //einvoice checks
                        if (invoice.InvoiceDeliveryType == (int)SoeInvoiceDeliveryType.Electronic)
                        {
                            if (!customer.IsPrivatePerson.GetValueOrDefault() && string.IsNullOrEmpty(customer.OrgNr) && string.IsNullOrEmpty(sendInfo.recipientGLN))
                            {
                                result = new ActionResult(GetText(7777, "Du har valt fakturametod \"e-faktura\".\\nFör att kunna skicka e-faktura krävs att du har angett ett korrekt organisationsnummer eller GLN-nummer.\\nDenna information kan registreras på kundkortet"));
                                return result;
                            }
                        }

                        var billingTemplateId = invoice.Actor.Customer?.BillingTemplate ?? defaultInvoiceRptTemplateId;
                        if (billingTemplateId == 0)
                        {
                            result = new ActionResult(GetText(4209, "Standardrapport för faktura saknas"));
                            return result;
                        }

                        var reportDto = InvoiceDistributionManager.GetInvoicePdf(customerInvoiceId, billingTemplateId, 0, invoice.PrintTimeReport, invoice.IncludeOnlyInvoicedTime, OrderInvoiceRegistrationType.Invoice, false);
                        //var reportDto = InvoiceDistributionManager.PrintInvoiceReport(customerInvoiceId, billingTemplateId, null, invoice.InvoiceNr, invoice.ActorId ?? 0, 0, invoice.PrintTimeReport, invoice.IncludeOnlyInvoicedTime, OrderInvoiceRegistrationType.Invoice, 0, false, "", actorCompanyId, ReportDataManager, InvoiceManager, EmailManager, null, 0);
                        if (reportDto == null || !reportDto.Success)
                        {
                            result = new ActionResult(GetText(7753, "Misslyckades skapa faktura fil"));
                            return result;
                        }
                        else
                        {
                            attachments.Add(pdfInvoiceFileName, reportDto.BinaryData);
                        }

                        //Send thru API
                        string token = InExchangeConnector.GetToken(actorCompanyId, releaseModeAPI);

                        var inexchangeCompanyCheckResult = GetInexchangeCompanyId(token, releaseModeAPI, invoice.ContactGLNId ?? 0,invoice.ActorId ?? 0, actorCompanyId, sendInfo);
                        if (inexchangeCompanyCheckResult.Success)
                        {
                            sendInfo.inexchangeCompanyId = inexchangeCompanyCheckResult.StringValue;
                        }
                        else if (!useInExchangeForAllInvoices)
                        {
                            result = inexchangeCompanyCheckResult;
                            return result;
                        }

                        result = InExchangeConnector.SendSveFakturaMessageToBePostedInExchangeApi(actorCompanyId, invoice.InvoiceId, svefakturaFileName, pdfInvoiceFileName, attachments, token, releaseModeAPI, sendInfo);

                        // Save GUID
                        if (result.Success)
                        {
                            if (result.Value.ToString() != string.Empty)
                                result = InvoiceDistributionManager.EinvoiceMessageSent(TermGroup_EDistributionType.Inexchange, invoice.InvoiceId, result.Value.ToString(), true);

                            if (!result.Success)
                            {
                                result.ErrorMessage = string.Format("{0}{1}", result.ErrorMessage, result.Exception);
                                return result;
                            }
                            else
                            {
                                foreach (var invoiceAttachment in invoiceAttachments.Where(a => a.Item1 > 0))
                                {
                                    InvoiceAttachmentManager.ConnectInvoiceAttachmentToDistribution(invoiceAttachment.Item1, result.IntegerValue, true);
                                }
                            }
                        }
                        else
                        {
                            result.ErrorMessage = "Fel uppstod vid sändning till InExchange!";
                            return result;
                        }
                    }
                    else
                    {
                        result.Success = false;
                        result.ErrorMessage = "Fel uppstod när SveFaktura genererades!";
                        return result;
                    }

                    attachments.Clear();
                }

                #endregion
            }
            catch (Exception e)
            {
                result = new ActionResult(e, GetText(6023, "Ett fel uppstod vid utskick av elektronisk faktura genom API:"));
                base.LogError(e, this.log);
            }

            result.Keys = new List<int>();

            #endregion

            return result;
        }

        public static bool GetInexchangeReleaseMode(SettingManager sm, int actorCompanyId, int userId)
        {
            bool releaseModeAPI = !sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingSveFakturaToAPITestMode, userId, actorCompanyId, 0, false);
#if DEBUG
            releaseModeAPI = false;
#endif
            return releaseModeAPI;
        }

        #region Intrum

        public ActionResult CreateIntrumInvoice(List<int> customerInvoiceIds, int actorCompanyId, bool fromBatchRun)
        {
            var result = new ActionResult(false);

            if (!fromBatchRun && customerInvoiceIds.Count > 10)
            {
                return CreateIntrumInvoicePending(customerInvoiceIds);
            }

            List<SysCountry> sysCountries = SysDbCache.Instance.SysCountrys;

            var intrumConfig = new SoftoneIntrumConfiguration
            {
                ClientNo = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.IntrumClientNo, UserId, actorCompanyId, 0),
                HubNo = SettingManager.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.IntrumHubNo, UserId, actorCompanyId, 0),
                LedgerNo = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.IntrumLedgerNo, UserId, actorCompanyId, 0),
                ClientUser = SettingManager.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.IntrumUser, UserId, actorCompanyId, 0),
                ClientSecret = SettingManager.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.IntrumPwd, UserId, actorCompanyId, 0),
                ClientBatchNo = 2,
                IJBatchNo = 2,
                ClientEmailAddress1 = "admin@client.org",
                ClientEmailAddress2 = "admin@client.org"
            };

            bool testMode = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.IntrumTestMode, UserId, actorCompanyId, 0, false);
            if (SettingManager.isTest() || SettingManager.isDev())
            {
                testMode = true;
            }
#if DEBUG
            testMode = true;
#endif

            if (intrumConfig.ClientNo == 0 || string.IsNullOrEmpty(intrumConfig.HubNo) || intrumConfig.LedgerNo == 0)
            {
                return new ActionResult("Missing intrum setting");
            }

            var intrumConnector = new IntrumConnector();
            using (var entities = new CompEntities())
            {
                var successCount = 0;
                var errorCount = 0;
                foreach (var customerInvoiceId in customerInvoiceIds)
                {
                    var invoice = InvoiceManager.GetCustomerInvoiceDistribution(entities, SoeOriginType.CustomerInvoice, customerInvoiceId, actorCompanyId);
                    if (invoice.InvoiceDeliveryProvider != (int)SoeInvoiceDeliveryProvider.Intrum)
                    {
                        continue;
                    }

                    string mappedDebetInvoiceNr = null;

                    if (invoice.BillingType == (int)TermGroup_BillingType.Credit)
                    {
                        mappedDebetInvoiceNr = InvoiceManager.GetMappedInvoices(entities, customerInvoiceId, SoeOriginInvoiceMappingType.CreditInvoice, true, false).FirstOrDefault()?.InvoiceNr ?? "";
                    }

                    var invoiceRows = InvoiceManager.GetCustomerInvoiceRowsSmall(entities, customerInvoiceId);
                    var customerDistributionInfo = GetCustomerDistributionDTO(invoice.ActorId, invoice.ContactEComId, invoice.BillingAddressId);

                    try
                    {
                        result = intrumConnector.SendInvoice(testMode, intrumConfig, invoice, invoiceRows, customerDistributionInfo, sysCountries, mappedDebetInvoiceNr);
                    }
                    catch (Exception ex)
                    {
                        result = new ActionResult(ex.Message);
                    }

                    if (result.Success)
                    {
                        successCount++;
                        result = InvoiceDistributionManager.EinvoiceMessageSent(TermGroup_EDistributionType.Intrum, customerInvoiceId, null, TermGroup_EDistributionStatusType.Sent, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        if (!result.Success)
                        {
                            return result;
                        }
                    }
                    else
                    {
                        errorCount++;
                        result = InvoiceDistributionManager.EinvoiceMessageSent(TermGroup_EDistributionType.Intrum, customerInvoiceId, null, TermGroup_EDistributionStatusType.Error, result.ErrorMessage);
                        if (!result.Success)
                        {
                            return result;
                        }
                    }

                    if (fromBatchRun && ((successCount + errorCount) % IntrumConnector.MAX_SEND_BATCH_SIZE == 0))
                    {
                        Thread.Sleep(1000 * 60);
                    }
                }

                result.IntegerValue = successCount;
                result.IntegerValue2 = errorCount;
            }

            return result;
        }

        private ActionResult CreateIntrumInvoicePending(List<int> customerInvoiceIds)
        {
            var result = new ActionResult(true);

            foreach (var customerInvoiceId in customerInvoiceIds)
            {
                //is then picked up by scheduled work....
                result = InvoiceDistributionManager.EinvoiceMessageSent(TermGroup_EDistributionType.Intrum, customerInvoiceId, null, TermGroup_EDistributionStatusType.PendingInPlatform, null);
                if (!result.Success)
                {
                    return result;
                }
            }

            return result;
        }
        #endregion

        #region Fortnox
        public ActionResult CreateFortnoxInvoices(List<int> customerInvoiceIds, int actorCompanyId)
        {
            var connector = new FortnoxConnector();
            return CreateExternalCustomerLedgerInvoices(connector, customerInvoiceIds, actorCompanyId);
        }

        public ActionResult CreateVismaEAccountingInvoices(List<int> customerInvoiceIds, int actorCompanyId)
        {
            var connector = new VismaEAccountingIntegrationManager();
            return CreateExternalCustomerLedgerInvoices(connector, customerInvoiceIds, actorCompanyId);
        }

        public ActionResult CreateExternalCustomerLedgerInvoices(IExternalInvoiceSystem connector, List<int> customerInvoiceIds, int actorCompanyId)
        {
            if (!FeatureManager.HasRolePermission(connector.Params.Feature, Permission.Modify, RoleId, actorCompanyId))
                return new ActionResult(GetText(1155, "Behörighet saknas"));

            var result = SetRefreshToken(connector, actorCompanyId, connector.Params.RefreshTokenStoragePoint);
            if (!result.Success)
                return result;
            result = PerformCreateExternalCustomerLedgerInvoices(connector, customerInvoiceIds, actorCompanyId);

            SettingManager.UpdateInsertDateSetting(SettingMainType.Company,
                (int)connector.Params.LastSyncStoragePoint,
                DateTime.Now, UserId, actorCompanyId, 0);

            return result;
        }

        private ActionResult SetRefreshToken(IExternalInvoiceSystem connector, int actorCompanyId, CompanySettingType refreshTokenSetting)
        {
            var refreshToken = SettingManager.GetStringSetting(SettingMainType.Company, (int)connector.Params.RefreshTokenStoragePoint, UserId, actorCompanyId, 0);
            if (string.IsNullOrEmpty(refreshToken))
                return new ActionResult(GetText(6020, "Integrationen är inte aktiv."));

            connector.SetAuthFromRefreshToken(refreshToken);

            SettingManager.UpdateInsertStringSetting(SettingMainType.Company,
                (int)connector.Params.RefreshTokenStoragePoint,
                connector.GetRefreshToken(), UserId, actorCompanyId, 0);
            return new ActionResult();
        }

        public ActionResult PerformCreateExternalCustomerLedgerInvoices(IExternalInvoiceSystem connector, List<int> customerInvoiceIds, int actorCompanyId)
        {
            string batch = Guid.NewGuid().ToString();
            var result = new ActionResult();
            int successCount = 0;

            SoeInvoiceExportStatusType typeWhenSuccess = SoeInvoiceExportStatusType.ExportedAndOpen; //default
            if (SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.CustomerCloseInvoicesWhenExported, 0, actorCompanyId, 0))
                typeWhenSuccess = SoeInvoiceExportStatusType.ExportedAndClosed;

            try
            {
                using (var entities = new CompEntities())
                {
                    var defaultSysCountryId = CompanyManager.GetCompanySysCountryId(entities, actorCompanyId);
                    var timeCodes = TimeCodeManager.GetTimeCodes(entities, actorCompanyId);
                    var errorList = new List<string>();

                    foreach (int invoiceId in customerInvoiceIds)
                    {
                        var invoice = InvoiceManager.GetCustomerInvoiceDistribution(entities, SoeOriginType.CustomerInvoice, invoiceId, actorCompanyId);
                        var customer = GetCustomerDistributionDTO(invoice.ActorId, invoice.ContactEComId, invoice.BillingAddressId);
                        SetDeliveryAddressFromInvoice(invoice.InvoiceHeadText, customer);
                        SetExtendedCustomerInfo(entities, invoice.ActorId, customer);
                        SetCountryInfo(defaultSysCountryId, invoice, customer);

                        var rows = InvoiceManager.GetCustomerInvoiceRowsDistribution(entities, invoiceId);
                        AddTextRowsFromInvoice(invoice, rows);

                        if (actorCompanyId == 7 || actorCompanyId == 3017945) // Harry P
                        {
                            // This is still in evaluating phase. Let's remove the company check later when we decide to go live with it.
                            AddTimeSheetAsTextRows(entities, actorCompanyId, invoiceId, invoice.IncludeInvoicedTime, rows, timeCodes);
                        }

                        result = connector.AddInvoice(invoice, customer, rows);
                        //Log
                        TermGroup_EDistributionStatusType status = result.Success ?
                            TermGroup_EDistributionStatusType.Sent :
                            TermGroup_EDistributionStatusType.Error;

                        InvoiceDistributionManager.EinvoiceMessageSent(connector.Params.DistributionStatusType, invoiceId, batch, status, result.ErrorMessage);

                        if (!result.Success)
                        {
                            if (result.BooleanValue)
                            {
                                //Invoice was created but failed to create tax deduction rows
                                InvoiceManager.SetExportStatus(entities, actorCompanyId, invoiceId, typeWhenSuccess, true);
                                entities.SaveChanges();
                            }

                            string errorMessage = string.Format(GetText(9508, "gällande fakturanummer {0}: {1}"), invoice.InvoiceNr, result.ErrorMessage);

                            errorList.Add(errorMessage);
                        } else {
                            successCount++;
                            InvoiceManager.SetExportStatus(entities, actorCompanyId, invoiceId, typeWhenSuccess, true);
                            if (!string.IsNullOrEmpty(result.StringValue))
                                InvoiceManager.SetExternalId(entities, actorCompanyId, invoiceId, result.StringValue);

                            entities.SaveChanges();
                        }
                    }
                    if (errorList.Count > 0)
                    {
                        int totalCount = customerInvoiceIds.Count;
                        int failedCount = customerInvoiceIds.Count - successCount;

                        errorList.Insert(0, string.Format(GetText(9383, "{0} av {1} fakturor kunde inte exporteras."), failedCount, totalCount));
                        errorList.Insert(1, string.Format(GetText(9442, "Felmedelande från {0}:"), connector.Name));

                        result.Success = false;
                        result.BooleanValue2 = true; //To hide the frontend error message 
                        result.ErrorMessage = string.Join("\n", errorList.ToArray());

                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                if (successCount > 0)
                    this.LogInfo($"Error when saving invoices to Visma/Fortnox. InvoiceIds: {customerInvoiceIds.JoinToString(",")}. Check error log.");

                this.LogError(ex, this.log);
                return new ActionResult(GetText(1148, "Ett fel inträffade."));
            }

            return result;
        }

        public void SyncCustomerFortnoxVisma(int actorCustomerId, CustomerDTO customerInput)
        {
            try {
                CustomerDistributionDTO customer = GetCustomerDistributionDTO(actorCustomerId, customerInput.ContactEComId, null, true);

                //Customer Sync
                SyncWithIntegration(new VismaEAccountingIntegrationManager(), customer, customerInput);
                SyncWithIntegration(new FortnoxConnector(), customer, customerInput);
            }
            catch (Exception ex)
            {
                LogError(ex, log);
            }
        }

        private void SyncWithIntegration(IExternalInvoiceSystem connector, CustomerDistributionDTO customer, CustomerDTO customerInput)
        {
            if (SetRefreshToken(connector, ActorCompanyId, connector.Params.RefreshTokenStoragePoint).Success)
            {
                connector.AddCustomer(customer, customerInput);
            }
        }

        public static bool HasFortnoxVismaIntegration(int actorCompanyId, SettingManager settingManager)
        {
            var isValidVisma = settingManager.GetStringSetting(
                SettingMainType.Company,
                (int)CompanySettingType.BillingVismaEAccountingRefreshToken,
                0,
                actorCompanyId,
                0).HasValue();
            var isValidFortnox = settingManager.GetStringSetting(
                SettingMainType.Company,
                (int)CompanySettingType.BillingFortnoxRefreshToken,
                0,
                actorCompanyId,
                0).HasValue();

            return isValidVisma || isValidFortnox;
        }

        private void SetExtendedCustomerInfo(CompEntities entities, int actorId, CustomerDistributionDTO dto)
        {
            var result = entities.Customer.Where(c => c.ActorCustomerId == actorId).Select(c => new
            {
                IsPrivatePerson = c.IsPrivatePerson,
            }).FirstOrDefault();

            if (result == null)
                return;

            dto.IsPrivatePerson = result.IsPrivatePerson ?? false;
        }

        private void SetCountryInfo(int defaultCountryId, CustomerInvoiceDistributionDTO invoice, CustomerDistributionDTO customer)
        {
            var countryId = invoice.ActorSysCountryId ?? defaultCountryId;

            if (countryId > 0)
            {
                var country = CountryCurrencyManager.GetSysCountry(countryId);
                customer.CountryCode = country != null ? country.Code : null;
            }
        }

        private void SetDeliveryAddressFromInvoice(string invoiceHeadText, CustomerDistributionDTO dto)
        {
            if (string.IsNullOrEmpty(invoiceHeadText))
                return;


            string[] lines = invoiceHeadText.Split(
                new string[] { "\r\n", "\r", "\n" },
                StringSplitOptions.None
            );

            if (lines.Length == 0)
                return;

            dto.DeliveryAddressName = lines[0];

            if (lines.Length > 1) dto.DeliveryAddressStreet = lines[1];
            else dto.DeliveryAddressStreet = string.Empty;

            if (lines.Length > 2) dto.DeliveryAddressPostalCode = lines[2];
            else dto.DeliveryAddressPostalCode = string.Empty;

            if (lines.Length > 3) dto.DeliveryAddressCity = lines[3];
            else dto.DeliveryAddressCity = string.Empty;

            if (lines.Length > 4) dto.DeliveryCountry = lines[4];
            else dto.DeliveryCountry = string.Empty;

            if (lines.Length > 5) dto.DeliveryAddressCO = lines[5];
            else dto.DeliveryAddressCO = string.Empty;
        }

        private void AddTextRowsFromInvoice(CustomerInvoiceDistributionDTO dto, List<CustomerInvoiceRowDistributionDTO> rows)
        {
            if (!string.IsNullOrEmpty(dto.InvoiceLabel))
            {
                rows.Insert(0, new CustomerInvoiceRowDistributionDTO()
                {
                    Product = null,
                    Text = $"{GetText(9117, "Märkning")}: {dto.InvoiceLabel}",
                });
            }
            if (!string.IsNullOrEmpty(dto.WorkingDescription) && dto.ShowWorkingDescription)
            {
                rows.Insert(1, new CustomerInvoiceRowDistributionDTO()
                {
                    Product = null,
                    Text = $"{GetText(9369, "Arbetsbeskrivning")}: {dto.WorkingDescription}",
                });
            }
        }

        public void AddTimeSheetAsTextRows(CompEntities entities, int actorCompanyId, int invoiceId, bool includeOnlyInvoiced, List<CustomerInvoiceRowDistributionDTO> rows, List<TimeCode> timeCodes)
        {
            var stateUtility = new StateUtility(entities, InvoiceManager);
            var generator = new TimeProjectDataReportGenerator(InvoiceManager, ProjectManager, AttestManager, TimeCodeManager, TimeDeviationCauseManager, TimeTransactionManager, SettingManager, stateUtility);
            var timeSheetParams = new TimeProjectReportParams(ActorCompanyId, UserId, RoleId, includeOnlyInvoiced, null, null);

            var timeSheetData = generator.GetTimeProjectReportData(entities, timeSheetParams, timeCodes, invoiceId);
            var timeEntries = timeSheetData.Projects
                .SelectMany(project => project.Employees)
                .SelectMany(employee => employee.TimeEntries,
                    (employee, entry) => new { Employee = employee, Entry = entry })
                .Where(x => x.Entry.InvoiceTimeInMinutes > 0)
                .OrderBy(x => x.Entry.Date)
                .ToList();

            if (!timeEntries.Any())
                return;
            
            rows.Add(new CustomerInvoiceRowDistributionDTO(""));
            foreach (var item in timeEntries
                .Where(x => x.Entry.InvoiceTimeInMinutes > 0)
                .OrderBy(x => x.Entry.Date))
            {
                var entry = item.Entry;
                var employee = item.Employee;
                int minutes = entry.InvoiceTimeInMinutes;
                string formattedInvoiceMinutes = TimeSpan.FromMinutes(minutes).ToString(@"hh\:mm");

                rows.Add(new CustomerInvoiceRowDistributionDTO($"{entry.Date.GetSwedishFormattedDate()}, {employee.Name}, {entry.TimeCodeName} - {formattedInvoiceMinutes}h"));
                if (!string.IsNullOrEmpty(entry.Note))
                    rows.Add(new CustomerInvoiceRowDistributionDTO(entry.Note));
            }
        }

        #endregion

        public ActionResult AddInexchangeInvoices(int actorCompanyId, int userId)
        {
            bool releaseModeAPI = ElectronicInvoiceMananger.GetInexchangeReleaseMode(SettingManager, actorCompanyId, userId);

            bool parseProductRows = SettingManager.GetCompanyBoolSetting(CompanySettingType.SupplierInvoiceProductRowsImport);
            var errorMessages = new List<string>();

            var invoiceList = InExchangeConnector.GetIncomingDocuments(actorCompanyId, releaseModeAPI);
            if (invoiceList == null)
            {
                return new ActionResult { Success = false, ErrorMessage = GetText(7436, "Misslyckades koppla upp sig/hämta fakturor från Inexchange") };
            }

            var formatCheck = invoiceList.FirstOrDefault(x => x.documentFormat != "svefaktura");
            if (formatCheck != null)
            {
                return new ActionResult { Success = false, ErrorMessage = GetText(7527, "Felaktigt efaktura format") + ": " + formatCheck.documentFormat };
            }

            var import = new ImportExportManager(parameterObject);

            var transferedCount = 0;
            var documentOKIds = new List<string>();
            try
            {
                foreach (var doc in invoiceList)
                {
                    var invoice = doc.CreateSupplierInvoiceIO(parseProductRows);

                    var supplierInvoiceIOItem = new SupplierInvoiceIOItem();
                    supplierInvoiceIOItem.supplierInvoices.Add(invoice);
                    var importResult = import.ImportSupplierInvoiceIO(supplierInvoiceIOItem, TermGroup_IOImportHeadType.SupplierInvoice, 0, TermGroup_IOSource.Connect, TermGroup_IOType.Inexchange, actorCompanyId, true);
                    if (importResult.Success)
                    {
                        transferedCount++;
                        documentOKIds.Add(doc.id);
                    }
                    else
                    {
                        errorMessages.Add(importResult.ErrorMessage);
                        LogError("ElectronicInvolices.GetEInvoices failed importing invoice: " + importResult.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex, this.log);
            }
            finally
            {
                if (documentOKIds.Any())
                {
                    InExchangeConnector.SendDocumentHandled(documentOKIds, actorCompanyId, releaseModeAPI);
                }
            }

            var transferMessage = string.Format(GetText(7437, "Antal hämtade fakturor: {0} \n Antal sparade fakturor: {1} \n Antal misslyckade fakturor: {2}"), invoiceList.Count, transferedCount, invoiceList.Count - transferedCount);
            var result = new ActionResult(invoiceList.Count == transferedCount);
            if (result.Success)
            {
                result.InfoMessage = transferMessage;
            }
            else
            {
                result.ErrorMessage = transferMessage + '\n' + string.Join("\n", errorMessages.ToArray());
            }


            return result;
        }

        #region Finvoice

        public ActionResult SendFinvoice(List<int> invoiceIds, int actorCompanyId, bool singleInvoicePerFile, bool overrideWarnings)
        {
            var result = new ActionResult(false);

            List<FileDataItem> fileList = null;
            string companyGuid = CompanyManager.GetCompanyGuid(actorCompanyId);

            foreach (var invoiceId in invoiceIds)
            {
                var invoice = InvoiceManager.GetCustomerInvoiceSmallEx(invoiceId);
                if (invoice.InvoiceDelieryProvider == (int)SoeInvoiceDeliveryProvider.Inexchange)
                {
                    result = SendFinVoiceToInexchange(new List<int> { invoiceId }, this.UserId, actorCompanyId);
                    if (!result.Success)
                    {
                        return result;
                    }
                    continue;
                }

                var senderOperator = SettingManager.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.BillingFinvoiceOperator, 0, actorCompanyId, 0);
                var controlInvoice = InvoiceDistributionManager.GetActiveEntry(invoiceId, TermGroup_EDistributionType.Finvoice);

                if (controlInvoice != null && (controlInvoice.DistributionStatus == (int)TermGroup_EDistributionStatusType.PendingInPlatform || controlInvoice.DistributionStatus == (int)TermGroup_EDistributionStatusType.Sent))
                {
                    result.ErrorMessage = GetText(7682, "Fakturan är redan skickad till Finvoice");
                    return result;
                }

                result = CreateFinvoice(actorCompanyId, invoiceId, true, out fileList, false, false, false, true, overrideWarnings);
                if (!result.Success)
                    return result;

                var sysBank = CountryCurrencyManager.GetSysBank(senderOperator);
                if (sysBank == null)
                    return new ActionResult(GetText(7683, "Finvoice-operatör saknas"));

                var messageGuid = Guid.NewGuid().ToString("N");
                result = BankerConnector.UploadFinvoiceFile(SettingManager, actorCompanyId, companyGuid, SysServiceManager.GetSysCompDBId(), sysBank, messageGuid, fileList[0].FileData);

                if (result.Success)
                {
                    result = InvoiceDistributionManager.EinvoiceMessageSent(TermGroup_EDistributionType.Finvoice, invoiceId, messageGuid, TermGroup_EDistributionStatusType.Sent, null);
                    if (!result.Success)
                    {
                        return result;
                    }

                    //Send attachment?
                    if (fileList.Count > 1)
                    {
                        var attchmentResult = BankerConnector.UploadFinvoiceAttachmentFile(SettingManager, actorCompanyId, companyGuid, SysServiceManager.GetSysCompDBId(), sysBank, messageGuid, fileList[1].FileData);

                        if (!attchmentResult.Success)
                        {
                            InvoiceDistributionManager.UpdateDistributionItem(messageGuid, TermGroup_EDistributionType.Finvoice, TermGroup_EDistributionStatusType.Error, "Attachment send error:" + attchmentResult);
                            return attchmentResult;
                        }
                    }
                }
            }

            return result;
        }

        public ActionResult CreateFinvoiceCustomerInvoiceExportFile(List<CustomerInvoiceGridDTO> items, int actorCompanyId, int userId, int invoiceId, bool singleInvoicePerFile, bool overrideWarnings)
        {
            ActionResult result = new ActionResult(false);

            var itemsZipped = false;
            var itemsForApi = new List<int>();

            if (items != null)
            {
                //Items for inexchange Api (printing)
                itemsForApi = (from i in items
                               where (i.InvoiceDeliveryProvider == (int)SoeInvoiceDeliveryProvider.Inexchange)
                               select i.CustomerInvoiceId).ToList();

                //The rest = normal finvoice
                items = (from i in items
                         where (i.DeliveryType == (int)SoeInvoiceDeliveryType.Electronic && i.InvoiceDeliveryProvider != (int)SoeInvoiceDeliveryProvider.Inexchange)
                         select i).ToList();
            }

            List<FileDataItem> fileList = null;

            if (invoiceId != 0)
            {
                CustomerInvoice invoice = InvoiceManager.GetCustomerInvoice(invoiceId, false, true, false, false, true, true, false, true, false, false, false, false);
                if (invoice.InvoiceDeliveryProvider == (int)SoeInvoiceDeliveryProvider.Inexchange)
                {
                    result = SendFinVoiceToInexchange(new List<int> { invoiceId }, userId, actorCompanyId);
                    if (result.Success)
                        return result;
                }
                else
                {
                    result = CreateFinvoice(actorCompanyId, invoiceId, true, out fileList, false, false, false, true, overrideWarnings);
                }
            }
            else
            {
                if (itemsForApi.Count > 0)
                {
                    //These have to sended one by one
                    result = SendFinVoiceToInexchange(itemsForApi, userId, actorCompanyId);
                    if (result.Success)
                        return result;
                }

                if (items.Count == 1)
                {
                    result = CreateFinvoice(actorCompanyId, items[0].CustomerInvoiceId, true, out fileList, false, false, false, true, overrideWarnings);
                }
                else if (items.Count > 1)
                {
                    result = CreateBulkFinvoice(actorCompanyId, items.Select(i => i.CustomerInvoiceId).ToList(), true, singleInvoicePerFile, out fileList, overrideWarnings);
                    itemsZipped = singleInvoicePerFile;
                }
            }

            if (!result.Success)
            {
                return result;
            }

            //no files to save from Inexchange functions....
            if (fileList != null && fileList.Any())
            {
                using (var entities = new CompEntities())
                {
                    try
                    {
                        using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                        {
                            if (entities.Connection.State != ConnectionState.Open)
                                entities.Connection.Open();

                            #region DataStorage

                            var data = fileList.FirstOrDefault();
                            string fileName = itemsZipped ? "FinvoiceZipped" : "FINV_" + "AINEISTO";
                            if (items != null && items.Count == 1)
                                fileName = "Finvoice " + items[0].InvoiceNr;
                            else if (invoiceId != 0)
                            {
                                var invoiceNr = InvoiceManager.GetInvoiceNr(entities, invoiceId);
                                fileName = "Finvoice " + invoiceNr;
                            }

                            var dataStorage = GeneralManager.CreateDataStorage(entities, SoeDataStorageRecordType.FinvoiceCustomerInvoiceExport, null, data.FileData, null, null, actorCompanyId, fileName: fileName + (itemsZipped ? Constants.SOE_SERVER_FILE_ZIP_SUFFIX : Constants.SOE_SERVER_FILE_XML_SUFFIX));

                            var attachmentDataStorageList = new List<DataStorage>();

                            //Attachmens?
                            foreach (var fileAttachment in fileList.Skip(1))
                            {
                                var attachmentDataStorage = GeneralManager.CreateDataStorage(entities, SoeDataStorageRecordType.FinvoiceCustomerInvoiceExportAttachments, null, fileAttachment.FileData, null, null, actorCompanyId, fileName: fileAttachment.FileName);
                                attachmentDataStorageList.Add(attachmentDataStorage);
                            }

                            #endregion

                            result = SaveChanges(entities, transaction);

                            //Commmit transaction
                            if (result.Success)
                            {
                                transaction.Complete();

                                if (dataStorage != null)
                                {
                                    result.StringValue = dataStorage.FileName;
                                    result.IntegerValue2 = dataStorage.DataStorageId;
                                }

                                result.Keys = attachmentDataStorageList.Select(d => d.DataStorageId).ToList();
                                result.Strings = attachmentDataStorageList.Select(d => d.FileName).ToList();

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        base.LogError(ex, this.log);
                    }
                    finally
                    {
                        if (result.Success)
                        {
                            result.ErrorMessage = "";
                            result.IntegerValue = items == null ? 0 : items.Count;
                        }
                        else
                            base.LogTransactionFailed(this.ToString(), this.log);

                        entities.Connection.Close();
                    }
                }
            }

            //Update StatusIcon
            var invoiceIds = (invoiceId > 0) ? new List<int> { invoiceId } : items.Select(i => i.CustomerInvoiceId).ToList();
            foreach (int id in invoiceIds)
            {
                InvoiceDistributionManager.EinvoiceMessageSent(TermGroup_EDistributionType.Finvoice, id, null, false);
            }

            return result;
        }

        private ActionResult SendFinVoiceToInexchange(List<int> invoiceIds, int userId, int actorCompanyId)
        {
            bool releaseModeAPI = !SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingSveFakturaToAPITestMode, userId, actorCompanyId, 0, false);
#if DEBUG
            releaseModeAPI = false;
#endif
            var result = new ActionResult();

            //These have to sended one by one
            var token = InExchangeConnector.GetToken(actorCompanyId, releaseModeAPI);

            foreach (var invoiceId in invoiceIds)
            {
                CustomerInvoice invoice = InvoiceManager.GetCustomerInvoice(invoiceId, false, true, false, false, true, true, false, true, false, false, false, false);

                string streetname = "";
                string postalCode = "";
                string town = "";

                List<FileDataItem> fileList;
                var dataList = CreateFinvoice(actorCompanyId, invoiceId, true, out fileList, true, true, true, false);
                var data = fileList.FirstOrDefault();

                //send single file to be printed
                Customer customer = invoice.Actor != null ? invoice.Actor.Customer : null;
                Contact customerContactPreferences = ContactManager.GetContactFromActor(invoice.Actor.ActorId);
                if (customerContactPreferences != null)
                {
                    //ContactAddressRow Billing            
                    var customerBillingAddress = ContactManager.GetContactAddressRows(customerContactPreferences.ContactId, (int)TermGroup_SysContactAddressType.Billing);

                    //use the billingaddress on the invoie if it exits
                    if (customerBillingAddress.Any(a => a.ContactAddress.ContactAddressId == invoice.BillingAddressId))
                        customerBillingAddress = customerBillingAddress.Where(a => a.ContactAddress.ContactAddressId == invoice.BillingAddressId).ToList();

                    ContactAddressRow customerPostalCode = customerBillingAddress.GetRow(TermGroup_SysContactAddressRowType.PostalCode);
                    ContactAddressRow customerPostalAddress = customerBillingAddress.GetRow(TermGroup_SysContactAddressRowType.PostalAddress);
                    ContactAddressRow customerAddressStreetName = customerBillingAddress.GetRow(TermGroup_SysContactAddressRowType.Address);

                    if (invoice.BillingAdressText != null)
                    {
                        if (invoice.BillingAdressText.Length > 0)
                        {
                            string[] separators = { Environment.NewLine, "\n", "\r" };
                            string[] address = invoice.BillingAdressText.Split(separators, StringSplitOptions.None);

                            if (address.Count() >= 2) { streetname = address[1]; } else { streetname = ""; }
                            if (address.Count() >= 3) { postalCode = address[2]; } else { postalCode = ""; }
                            if (address.Count() >= 4) { town = address[3]; } else { town = ""; }
                        }
                        else
                        {
                            streetname = customerAddressStreetName != null && !string.IsNullOrEmpty(customerAddressStreetName.Text) ? customerAddressStreetName.Text : string.Empty;
                            town = customerPostalAddress != null && !string.IsNullOrEmpty(customerPostalAddress.Text) ? customerPostalAddress.Text : string.Empty;
                            postalCode = customerPostalCode != null && !string.IsNullOrEmpty(customerPostalCode.Text) ? customerPostalCode.Text : string.Empty;
                        }
                    }
                    else
                    {
                        streetname = customerAddressStreetName != null && !String.IsNullOrEmpty(customerAddressStreetName.Text) ? customerAddressStreetName.Text : String.Empty;
                        town = customerPostalAddress != null && !String.IsNullOrEmpty(customerPostalAddress.Text) ? customerPostalAddress.Text : String.Empty;
                        postalCode = customerPostalCode != null && !String.IsNullOrEmpty(customerPostalCode.Text) ? customerPostalCode.Text : String.Empty;
                    }
                }

                result = InExchangeConnector.SendFinVoiceMessageToBePostedInExchangeApi(actorCompanyId, invoice.InvoiceId, data.FileData, "finvoice", token, invoice.InvoiceId.ToString() + invoice.InvoiceNr.ToString() + ".xml", "FI", customer?.OrgNr, customer?.Name, streetname, town, postalCode, releaseModeAPI);

                // Save GUID
                if (result.Success && result.Value.ToString() != string.Empty)
                {
                    result = InvoiceDistributionManager.EinvoiceMessageSent(TermGroup_EDistributionType.Inexchange, invoice.InvoiceId, result.Value.ToString(), true);
                }
            }

            InExchangeConnector.RevokeTokenForCustomer(token, releaseModeAPI);
            return result;
        }

        public ActionResult CreateFinvoice(int actorCompanyId, int invoiceId, bool original, out List<FileDataItem> outFiles, bool noSoap = false, bool utf8 = false, bool printService = false, bool createAttachments = false, bool overrideWarnings = false)
        {
            #region Prereq

            outFiles = new List<FileDataItem>();

            if (actorCompanyId == 0 || invoiceId == 0)
                return new ActionResult { ErrorMessage = "ActorCompanyId or Invoice is 0" };

            Company company = CompanyManager.GetCompany(actorCompanyId);
            if (company == null)
                return new ActionResult { ErrorMessage = "Failed finding company" };

            //Get SysCurrencies
            List<SysCurrency> sysCurrencies = CountryCurrencyManager.GetSysCurrencies(true);

            //Get Billing settings
            decimal interestPercent = SettingManager.GetDecimalSetting(SettingMainType.Company, (int)CompanySettingType.CustomerInterestPercent, 0, actorCompanyId, 0);
            string SenderAddress = SettingManager.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.BillingFinvoiceAddress, 0, actorCompanyId, 0);
            string SenderOperator = SettingManager.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.BillingFinvoiceOperator, 0, actorCompanyId, 0);

            CustomerInvoice invoice = InvoiceManager.GetCustomerInvoice(invoiceId, false, true, false, false, true, true, false, true, false, false, false, false);
            List<CustomerInvoiceRow> invoiceRows = invoice.ActiveCustomerInvoiceRows.Where(i => i.Type == (int)SoeInvoiceRowType.ProductRow || i.Type == (int)SoeInvoiceRowType.BaseProductRow || i.Type == (int)SoeInvoiceRowType.TextRow || i.IsInvoiceFeeRow || i.IsFreightAmountRow).OrderBy(i => i.RowNr).ToList();
            PaymentInformation paymentInformation = PaymentManager.GetPaymentInformationFromActor(company.ActorCompanyId, true, false);
            SysCurrency invoiceCurrency = sysCurrencies.FirstOrDefault(i => i.SysCurrencyId == invoice.Currency.SysCurrencyId);
            Customer customer = invoice.Actor != null ? invoice.Actor.Customer : null;

            if (invoice.InvoiceDeliveryProvider == (int)SoeInvoiceDeliveryProvider.Inexchange)
                noSoap = true;
            //If the customer hasn't been marked as finvoicereceiver
            else if (customer == null || !customer.IsFinvoiceCustomer && invoice.InvoiceDeliveryProvider != (int)SoeInvoiceDeliveryProvider.Inexchange)
                return new ActionResult(7565, GetText(7565, "Kunden är inte uppsatt som mottagare av Finvoice"));

            var customerBillingAddressRows = new List<ContactAddressRow>();
            var customerDeliveryAddressRows = new List<ContactAddressRow>();
            var companyAddress = new List<ContactAddressRow>();
            var companyBoardHQAddress = new List<ContactAddressRow>();
            var companyContactEcoms = new List<ContactECom>();
            var customerContactEcoms = new List<ContactECom>();

            #endregion

            #region Billing And Delivery Address

            Contact customerContactPreferences = ContactManager.GetContactFromActor(invoice.Actor.ActorId);
            if (customerContactPreferences != null)
            {
                //use the billingaddress on the invoie if it exits
                var billingAddressRows = ContactManager.GetContactAddressRows(customerContactPreferences.ContactId, (int)TermGroup_SysContactAddressType.Billing);
                if (!billingAddressRows.IsNullOrEmpty())
                {
                    customerBillingAddressRows = billingAddressRows.Where(a => a.ContactAddress.ContactAddressId == invoice.BillingAddressId).ToList();
                }

                //use the deliveryAddress on the invoie if it exits
                var deliveryAddressRows = ContactManager.GetContactAddressRows(customerContactPreferences.ContactId, (int)TermGroup_SysContactAddressType.Delivery);
                if (!deliveryAddressRows.IsNullOrEmpty())
                {
                    customerDeliveryAddressRows = deliveryAddressRows.Where(a => a.ContactAddress.ContactAddressId == invoice.DeliveryAddressId).ToList();
                }

                customerContactEcoms = ContactManager.GetContactEComs(customerContactPreferences.ContactId);
            }

            #endregion

            #region CompanyAddress and ContactEcoms

            Contact companyContactPreferences = ContactManager.GetContactFromActor(company.ActorCompanyId);
            if (companyContactPreferences != null)
            {
                companyContactEcoms = ContactManager.GetContactEComs(companyContactPreferences.ContactId);
                companyAddress = ContactManager.GetContactAddressRows(companyContactPreferences.ContactId, (int)TermGroup_SysContactAddressType.Distribution);
                companyBoardHQAddress = ContactManager.GetContactAddressRows(companyContactPreferences.ContactId, (int)TermGroup_SysContactAddressType.BoardHQ);
            }

            #endregion

            #region Content

            bool printHasTaxBillLabel = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingPrintTaxBillText, 0, actorCompanyId, 0);
            string exemptionReason = (printHasTaxBillLabel) ? GetText(5975, "Godkänd för F-skatt") : string.Empty;
            TermGroup_EInvoiceFormat eInvoiceFormat = (TermGroup_EInvoiceFormat)SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingEInvoiceFormat, 0, actorCompanyId, 0);
            var defaultInvoiceText = InvoiceManager.GetDefaultInvoiceText(actorCompanyId);

            FinvoiceEdiItem finvoiceItem = new FinvoiceEdiItem(this.parameterObject, invoice, paymentInformation, company, invoiceCurrency, customer, invoiceRows, customerBillingAddressRows, customerDeliveryAddressRows, customerContactEcoms, companyAddress, companyBoardHQAddress, companyContactEcoms, exemptionReason, original, printService, defaultInvoiceText, eInvoiceFormat);

            byte[] fileAttachments = null;

            if (createAttachments && eInvoiceFormat == TermGroup_EInvoiceFormat.Finvoice3 && invoice.AddAttachementsToEInvoice)
            {
                var invoiceAttachments = InvoiceDistributionManager.GetInvoiceDocuments(invoice.InvoiceId, actorCompanyId, SoeOriginType.CustomerInvoice, null, false);
                if (invoiceAttachments.Any())
                {
                    var validationResult = FinvoiceAttachmentItem.CheckForInvalidAttachments(this, invoice.InvoiceNr, invoiceAttachments);
                    if (!validationResult.Success)
                        return validationResult;

                    // Validate FInvoice Operator
                    var operatorValidationResult = finvoiceItem.ValidateOperatorForAttachment(customer, SettingManager, UserId, actorCompanyId, 0, invoice.InvoiceNr);
                    if (!overrideWarnings && !operatorValidationResult.Success)
                        return operatorValidationResult;

                    var encoding = Encoding.GetEncoding("ISO-8859-15");
                    XDocument finvoiceAttachmentsXML = XmlUtil.CreateDocument("ISO-8859-15");

                    var finvoiceAttachments = new FinvoiceAttachmentItem(invoice, invoiceAttachments, finvoiceItem.MessageTransmissionDetails.MessageIdentifier);
                    finvoiceAttachments.ToXml(finvoiceItem.MessageTransmissionDetails, ref finvoiceAttachmentsXML);
#if DEBUG
                    //File.WriteAllText(@"C:\Temp\finvoice\finvoiceAttachments_" + invoice.InvoiceId.ToString() + ".xml", finvoiceAttachmentsXML.ToString());
#endif
                    var fileString = "<?xml version=\"1.0\" encoding=\"iso-8859-15\"?>" + Environment.NewLine + finvoiceAttachmentsXML.ToString();
                    fileAttachments = encoding.GetBytes(fileString);

                    finvoiceItem.AddAttachmentMessageDetails = true;
                }
            }

            //Document
            XDocument finvoiceXml = (utf8) ? XmlUtil.CreateDocument(Encoding.UTF8) : XmlUtil.CreateDocument("ISO-8859-15");

            finvoiceItem.ToXml(ref finvoiceXml, eInvoiceFormat);
            bool valid = finvoiceItem.Validate(eInvoiceFormat); //todo: actions if not valid, now used only to log errors
            if (!valid)
                return new ActionResult("Finvoice validation faild");

            //Stream data
            MemoryStream stream = new MemoryStream();
            XmlDocument xmlDoc = new XmlDocument();

            if (!noSoap && (!string.IsNullOrEmpty(SenderAddress) && !string.IsNullOrEmpty(SenderOperator)))
            {
                XDocument finvoiceSoap = XmlUtil.CreateDocument("ISO-8859-15");
                finvoiceItem.AddSoapEnvelope(ref finvoiceSoap, customer, SenderAddress, SenderOperator, finvoiceItem.TimestampStr);

                stream.Position = 0;
                using (var streamWriter = new StreamWriter(stream, Encoding.GetEncoding("ISO-8859-15")))
                {
                    streamWriter.Write(finvoiceSoap.ToString());
                    streamWriter.WriteLine("");
                    streamWriter.WriteLine("<?xml version=\"1.0\" encoding=\"iso-8859-15\"?>");
                    streamWriter.Write(finvoiceXml.ToString());
                    streamWriter.Flush();
                }
            }
            else
                finvoiceXml.Save(stream);

            #endregion
            var fileName = "Finvoice " + invoice?.InvoiceNr;
            outFiles.Add(new FileDataItem { FileName = fileName + Constants.SOE_SERVER_FILE_XML_SUFFIX, FileData = stream.ToArray() });
            if (fileAttachments != null)
            {
                outFiles.Add(new FileDataItem { FileName = fileName + "_attachments" + Constants.SOE_SERVER_FILE_XML_SUFFIX, FileData = fileAttachments, Attachment = true });
            }

            return new ActionResult();
        }

        public ActionResult CreateBulkFinvoice(int actorCompanyId, List<int> invoiceIds, bool original, bool zipped, out List<FileDataItem> outFiles, bool overrideWarnings)
        {
            #region Prereq
            outFiles = new List<FileDataItem>();

            if (actorCompanyId == 0 || invoiceIds.Count == 0)
                return new ActionResult("No invoices selected");

            Company company = CompanyManager.GetCompany(actorCompanyId);
            if (company == null)
                return new ActionResult { ErrorMessage = "Failed finding company" };

            #endregion

            var stream = new MemoryStream { Position = 0 };

            var encoding = Encoding.GetEncoding("ISO-8859-15");
            var newLineBytes = encoding.GetBytes(Environment.NewLine);

            foreach (var invoiceId in invoiceIds)
            {
                List<FileDataItem> fileList;
                var result = CreateFinvoice(actorCompanyId, invoiceId, original, out fileList, false, false, false, true, overrideWarnings);
                if (!result.Success)
                {
                    return result;
                }

                var invoiceFile = fileList != null && fileList.Count > 0 ? fileList.FirstOrDefault() : null;
                var invoiceAttachmentFile = fileList != null && fileList.Count > 1 ? fileList[1] : null;

                if (zipped)
                {
                    outFiles.Add(invoiceFile);

                    if (invoiceAttachmentFile != null)
                        outFiles.Add(invoiceAttachmentFile);
                }
                else
                {
                    if (invoiceFile != null)
                    {
                        if (stream.Position > 0)
                        {
                            stream.Write(newLineBytes, 0, newLineBytes.Length);
                        }
                        stream.Write(invoiceFile.FileData, 0, invoiceFile.FileData.Length);
                    }
                    if (invoiceAttachmentFile != null)
                    {
                        outFiles.Add(invoiceAttachmentFile);
                    }
                }
            }

            if (zipped)
            {
                var guid = Guid.NewGuid();
                var tempfolder = ConfigSettings.SOE_SERVER_DIR_TEMP_REPORT_PHYSICAL;
                var zippedpath = $@"{tempfolder}\{guid.ToString()}.zip";

                if (ZipUtility.ZipFiles(zippedpath, outFiles.Select(f => new Tuple<string, byte[]>(item1: f.FileName, item2: f.FileData)).ToList()))
                {
                    var data = File.ReadAllBytes(zippedpath);
                    File.Delete(zippedpath);

                    outFiles = new List<FileDataItem>();
                    outFiles.Add(new FileDataItem { FileData = data });
                }
            }
            else
            {
                outFiles.Insert(0, new FileDataItem { FileData = stream.ToArray() });
            }
            return new ActionResult();
        }

        #endregion

        #region Svefaktura
        public ActionResult CreateSveFakturaExportFile(List<CustomerInvoiceGridDTO> items, int actorCompanyId, int userId)
        {
            ActionResult result = new ActionResult(false);
            Company company = CompanyManager.GetCompany(actorCompanyId);
            if (company == null)
                return null;

            if (items != null)
            {
                items = (from i in items
                         where (i.DeliveryType == (int)SoeInvoiceDeliveryType.Electronic)
                         select i).ToList();
            }

            var stream = new MemoryStream { Position = 0 };

            XDocument header = CreateSveFakturaSBDHEnvelope(company.VatNr, company.Name);
            string headerString = header.ToString();
            headerString = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" + headerString;
            int last = headerString.LastIndexOf("</");
            string headerStart = headerString.Substring(0, last);
            string headerEnd = headerString.Substring(last);
            UTF8Encoding u8 = new UTF8Encoding();
            byte[] start = u8.GetBytes(headerStart);
            byte[] end = u8.GetBytes(headerEnd);
            stream.Write(start, 0, u8.GetByteCount(headerStart));
            DataStorage dataStorage = null;
            Byte[] data = null;

            foreach (var item in items)
            {
                CustomerInvoice invoice = InvoiceManager.GetCustomerInvoice(item.CustomerInvoiceId);

                // Check if Orgn Exits
                if (invoice.Customer != null && String.IsNullOrEmpty(invoice.Customer.OrgNr))
                {
                    result.Success = false;

                    result.Value = GetText(11, "Org.nr på företaget är ej angivet");
                    return result;
                }
                bool hasProject = false;
                XDocument svefaktXml = CreateSveFakturaForFile(actorCompanyId, userId, item.CustomerInvoiceId, string.Empty, out hasProject);
                string sveFaktura = svefaktXml.ToString();
                sveFaktura = sveFaktura + "\r\n";
                byte[] sveFaktByte = u8.GetBytes(sveFaktura);
                stream.Write(sveFaktByte, 0, u8.GetByteCount(sveFaktura));

                //Update Statusicon
                result = InvoiceDistributionManager.EinvoiceMessageSent(TermGroup_EDistributionType.Inexchange, item.CustomerInvoiceId, null, false);
            }

            stream.Write(end, 0, u8.GetByteCount(headerEnd));
            data = stream.ToArray();

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        if (entities.Connection.State != ConnectionState.Open)
                            entities.Connection.Open();

                        if (data == null)
                            return new ActionResult(false, (int)ActionResultSave.EntityIsNull, "data");

                        #region DataStorage

                        string description = "SVEFAKTURA_" + DateTime.Now.ToString("g") + Constants.SOE_SERVER_FILE_XML_SUFFIX;

                        dataStorage = GeneralManager.CreateDataStorage(entities, SoeDataStorageRecordType.BillingInvoiceXML, null, data, null, null, actorCompanyId, description);

                        #endregion

                        result = SaveChanges(entities, transaction);

                        //Commmit transaction
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                }
                finally
                {
                    if (result.Success)
                    {
                        result.ErrorMessage = "";
                        result.StringValue = dataStorage?.Description;
                        result.IntegerValue = items.Count;
                        result.IntegerValue2 = dataStorage?.DataStorageId ?? 0;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            return result;
        }

        private DownloadFileDTO CreateTimebook(int invoiceId, bool includeOnlyInvoicedTime)
        {
            var rrm = new RequestReportManager(this.parameterObject, this.ReportManager, this.ReportDataManager);

            var downloadFileDTO = rrm.PrintProjectTimeBook(new ProjectTimeBookPrintDTO
            {
                InvoiceId = invoiceId,
                Queue = false,
                IncludeOnlyInvoiced = includeOnlyInvoicedTime,
                ReturnAsBinary = true,
                exportType = TermGroup_ReportExportType.Pdf
            });

            return downloadFileDTO;
            /*
            var billingTimeProjectReportId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingDefaultTimeProjectReportTemplate, base.UserId, base.ActorCompanyId, 0);

            if (billingTimeProjectReportId == 0)
            {
                base.LogWarning("createTimebook failed getting billingTimeProjectReportId");
            }

            var reportItem = new BillingInvoiceTimeProjectReportDTO(actorCompanyId, billingTimeProjectReportId, (int)SoeReportTemplateType.TimeProjectReport, new BillingInvoiceReportDTO(actorCompanyId, 0, 0, new List<int>() { invoiceId }, false, false, includeOnlyInvoiced: includeOnlyInvoicedTime), false, true);
            var report = ReportManager.GetReport(reportItem.ReportId, actorCompanyId);

            var selection = new Selection(actorCompanyId, this.UserId, this.parameterObject.RoleId, this.LoginName,
                    report: report.ToDTO(), isMainReport: true, exportType: (int)TermGroup_ReportExportType.Pdf, exportFileType: 0);

            selection.Evaluate(reportItem, 0);

            ReportPrintoutDTO dto = ReportDataManager.PrintReportDTO(selection.Evaluated, true);
            return dto.Data;
            */
        }

        public void SetDistributionStatusOnUnsetInvoices(List<int> invoiceIds, string errorMessage)
        {
            this.InvoiceDistributionManager.SetDistributionStatusOnUnsetInvoices(invoiceIds, errorMessage);
        }

        public CustomerDistributionDTO GetCustomerDistributionDTO(int actorId, int? invoiceContactEComId, int? billingAddressId, bool useDefaultEmailForCustomer = false)
        {
            var result = new CustomerDistributionDTO();
            Customer customer = CustomerManager.GetCustomer(actorId, true, loadContactAddresses: true);

            var contact = customer.Actor.Contact.First();
            var customerContactEcoms = ContactManager.GetContactEComs(contact.ContactId);

            if (!customerContactEcoms.IsNullOrEmpty())
            {
                result.MobilePhone = customerContactEcoms.FirstOrDefault(x => x.SysContactEComTypeId == (int)TermGroup_SysContactEComType.PhoneMobile)?.Text;
                if (!useDefaultEmailForCustomer && invoiceContactEComId.GetValueOrDefault() > 0)
                    result.Email = customerContactEcoms.FirstOrDefault(x => x.ContactEComId == invoiceContactEComId)?.Text;
                else if (useDefaultEmailForCustomer) //When creating customer that doenst have imvoice
                    result.Email = customerContactEcoms.FirstOrDefault(x => x.SysContactEComTypeId == (int)TermGroup_SysContactEComType.Email)?.Text;

                if (customer.ReminderContactEComId.GetValueOrDefault() > 0)
                    result.ReminderEmail = customerContactEcoms.FirstOrDefault(x => x.ContactEComId == customer.ReminderContactEComId.Value)?.Text;
            }

            string addressName;
            string streetAddress;
            string addressCo;
            string postalCode;
            string customerCity;
            string customerCountry;

            var contactAddreses = contact.ContactAddress.ToList();
            var address = contactAddreses.FirstOrDefault(c => c.SysContactAddressTypeId == (int)TermGroup_SysContactAddressType.Delivery);
            if (address != null)
            {
                ContactManager.ParseContactAddress(address, TermGroup_SysContactAddressType.Delivery, out addressName, out streetAddress, out addressCo, out postalCode, out customerCity, out customerCountry);
                result.DeliveryAddressName = addressName;
                result.DeliveryAddressCity = customerCity;
                result.DeliveryAddressStreet = streetAddress;
                result.DeliveryAddressPostalCode = postalCode;
                result.DeliveryAddressCO = addressCo;
                result.DeliveryCountry = customerCountry;
            }

            address = billingAddressId.GetValueOrDefault() > 0 ?
                    contactAddreses.FirstOrDefault(c => c.ContactAddressId == billingAddressId.Value) :
                    contactAddreses.FirstOrDefault(c => c.SysContactAddressTypeId == (int)TermGroup_SysContactAddressType.Billing);

            if (address != null)
            {
                ContactManager.ParseContactAddress(address, TermGroup_SysContactAddressType.Billing, out addressName, out streetAddress, out addressCo, out postalCode, out customerCity, out customerCountry);
                result.BillingAddressName = addressName;
                result.BillingAddressCity = customerCity;
                result.BillingAddressStreet = streetAddress;
                result.BillingAddressPostalCode = postalCode;
                result.BillingAddressCO = addressCo;
                result.BillingCountry = customerCountry;
            }

            address = contactAddreses.FirstOrDefault(c => c.SysContactAddressTypeId == (int)TermGroup_SysContactAddressType.Visiting);

            if (address != null)
            {
                ContactManager.ParseContactAddress(address, TermGroup_SysContactAddressType.Visiting, out addressName, out streetAddress, out addressCo, out postalCode, out customerCity, out customerCountry);
                result.VisitorAddressStreet = streetAddress;
            }

            return result;
        }
        public ActionResult CreatePeppolSveFaktura(int actorCompanyId, int userId, int invoiceId, string timeReportFilename, List<string> attachementNames, InExchangeApiSendInfo apiSendInfo, TermGroup_EInvoiceFormat fileFormat, out byte[] file)
        {
            CustomerInvoice invoice = InvoiceManager.GetCustomerInvoice(invoiceId, true, true, false, false, true, true, false, true, false, false, false, false);
            return CreatePeppolSveFaktura(invoice, actorCompanyId, userId, timeReportFilename, attachementNames, apiSendInfo, fileFormat, out file);
        }
        public ActionResult CreatePeppolSveFaktura(CustomerInvoice invoice, int actorCompanyId, int userId, string timeReportFilename, List<string> attachementNames, InExchangeApiSendInfo apiSendInfo, TermGroup_EInvoiceFormat fileFormat, out byte[] file)
        {
            var hasProject = false;
            file = Array.Empty<byte>();
            #region Prereq

            if (actorCompanyId == 0 || invoice == null)
                return new ActionResult("actorCompanyId or invoice missing");

            Company company = CompanyManager.GetCompany(actorCompanyId);
            if (company == null)
                return new ActionResult("Company missing");

            #endregion

            #region Prereq

            //Get SysCurrencies
            List<SysCurrency> sysCurrencies = CountryCurrencyManager.GetSysCurrencies(true);

            InvoiceManager.SetPriceListTypeInclusiveVat(invoice, actorCompanyId);

            int eInvoiceFormat = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingEInvoiceFormat, 0, actorCompanyId, 0);
            bool printTimeReport = (invoice.PrintTimeReport && (eInvoiceFormat == (int)TermGroup_EInvoiceFormat.SvefakturaTidbok));

            if (printTimeReport && invoice.ProjectId.HasValue && invoice.ProjectId > 0)
                hasProject = true;

            CustomerInvoice creditedInvoice = null;
            if (invoice.IsCredit)
            {
                if (!invoice.Origin.OriginInvoiceMapping.IsLoaded)
                {
                    invoice.Origin.OriginInvoiceMapping.Load();
                }
                var originInvoiceMapping = invoice.Origin.OriginInvoiceMapping.FirstOrDefault(x => x.Type == (int)SoeOriginInvoiceMappingType.CreditInvoice);
                if (originInvoiceMapping != null)
                    creditedInvoice = InvoiceManager.GetCustomerInvoice(originInvoiceMapping.InvoiceId);
            }

            List<CustomerInvoiceRow> invoiceRows = invoice.ActiveCustomerInvoiceRows.Where(i => i.Type == (int)SoeInvoiceRowType.TextRow || i.Type == (int)SoeInvoiceRowType.ProductRow || i.Type == (int)SoeInvoiceRowType.BaseProductRow || (i.Type == (int)SoeInvoiceRowType.AccountingRow && (i.IsFreightAmountRow || i.IsInvoiceFeeRow))).ToList();
            PaymentInformation paymentInformation = PaymentManager.GetPaymentInformationFromActor(company.ActorCompanyId, true, false);
            SysCurrency invoiceCurrency = sysCurrencies.FirstOrDefault(i => i.SysCurrencyId == invoice.Currency.SysCurrencyId);
            Customer customer = invoice.Actor?.Customer;

            List<ContactAddressRow> customerBillingAddress = new List<ContactAddressRow>();
            List<ContactAddressRow> customerDeliveryAddress = new List<ContactAddressRow>();
            List<ContactECom> customerContactEcoms;
            List<ContactAddressRow> companyAddress = new List<ContactAddressRow>();
            List<ContactAddressRow> companyBoardHQAddress = new List<ContactAddressRow>();
            List<ContactECom> companyContactEcoms = new List<ContactECom>();

            #endregion

            #region Billing And Delivery Address

            var contactId = ContactManager.GetContactIdFromActorId(invoice.Actor.ActorId);
            if (contactId > 0)
            {
                //ContactAddressRow Billing            
                customerBillingAddress = ContactManager.GetContactAddressRows(contactId, (int)TermGroup_SysContactAddressType.Billing);
                //use the billingaddress on the invoie if it exits
                if (customerBillingAddress.Any(a => a.ContactAddress.ContactAddressId == invoice.BillingAddressId))
                    customerBillingAddress = customerBillingAddress.Where(a => a.ContactAddress.ContactAddressId == invoice.BillingAddressId).ToList();

                //ContactAddressRow Delivery            
                customerDeliveryAddress = ContactManager.GetContactAddressRows(contactId, (int)TermGroup_SysContactAddressType.Delivery);
                //use the deliveryAddress on the invoie if it exits
                if (customerDeliveryAddress.Any(a => a.ContactAddress.ContactAddressId == invoice.DeliveryAddressId))
                    customerDeliveryAddress = customerDeliveryAddress.Where(a => a.ContactAddress.ContactAddressId == invoice.DeliveryAddressId).ToList();

                customerContactEcoms = ContactManager.GetContactEComs(contactId);
            }
            else
            {
                customerContactEcoms = new List<ContactECom>();
            }

            #endregion

            #region CompanyAddress and ContactEcoms

            Contact companyContactPreferences = ContactManager.GetContactFromActor(company.ActorCompanyId);
            if (companyContactPreferences != null)
            {
                companyContactEcoms = ContactManager.GetContactEComs(companyContactPreferences.ContactId);
                companyAddress = ContactManager.GetContactAddressRows(companyContactPreferences.ContactId, (int)TermGroup_SysContactAddressType.Distribution);
                companyBoardHQAddress = ContactManager.GetContactAddressRows(companyContactPreferences.ContactId, (int)TermGroup_SysContactAddressType.BoardHQ);

            }

            //Set some contact data to be used by calling function....
            if (apiSendInfo != null)
            {
                if (invoice.ContactEComId != null && invoice.ContactEComId > 0 && customerContactEcoms != null)
                {
                    var buyerEmail = customerContactEcoms.FirstOrDefault(x => x.ContactEComId == invoice.ContactEComId);
                    apiSendInfo.recipientEmail = buyerEmail == null ? string.Empty : buyerEmail.Text;
                }
                if (companyContactEcoms != null)
                {
                    var companyEmail = companyContactEcoms.FirstOrDefault(i => i.SysContactEComTypeId == (int)TermGroup_SysContactEComType.Email);
                    apiSendInfo.senderEmail = companyEmail?.Text;
                }
                if (invoice.ContactGLNId != null && invoice.ContactGLNId > 0 && customerContactEcoms != null)
                {
                    var buyerGLN = customerContactEcoms.FirstOrDefault(x => x.ContactEComId == invoice.ContactGLNId);
                    apiSendInfo.recipientGLN = buyerGLN == null ? string.Empty : buyerGLN.Text;
                }
                apiSendInfo.senderName = company.Name;
            }

            #endregion

            bool printHasTaxBillLabel = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingPrintTaxBillText, 0, actorCompanyId, 0);
            string exemptionReason = string.Empty;
            if (printHasTaxBillLabel)
                exemptionReason = GetText(5975, "Godkänd för F-skatt");

            XDocument invoiceXml;
            if (fileFormat == TermGroup_EInvoiceFormat.Peppol)
            {
                var peppol = new PeppolBilling(invoice, paymentInformation, company, invoiceCurrency, customer, invoiceRows, customerBillingAddress, customerDeliveryAddress, customerContactEcoms, companyAddress, companyBoardHQAddress, companyContactEcoms, exemptionReason, creditedInvoice, actorCompanyId, userId, hasProject ? timeReportFilename : string.Empty, attachementNames, InvoiceManager.GetDefaultInvoiceText(actorCompanyId));
                invoiceXml = XmlUtil.CreateDocument();
                peppol.ToXml(ref invoiceXml);
            }
            else
            {
                var svefakt = new SvefakturaItem(invoice, paymentInformation, company, invoiceCurrency, customer, invoiceRows, customerBillingAddress, customerDeliveryAddress, customerContactEcoms, companyAddress, companyBoardHQAddress, companyContactEcoms, exemptionReason, creditedInvoice, actorCompanyId, userId, hasProject ? timeReportFilename : string.Empty, attachementNames, InvoiceManager.GetDefaultInvoiceText(actorCompanyId));
                //Document
                invoiceXml = XmlUtil.CreateDocument(Constants.ENCODING_IBM437, true);
                svefakt.ToXml(ref invoiceXml);
            }

            //Stream data
            var stream = new MemoryStream();

            invoiceXml.Save(stream);

            file = stream.ToArray();
            return new ActionResult();
        }

        public XDocument CreateSveFakturaForFile(int actorCompanyId, int userId, int invoiceId, string timeReportFilename, out bool hasProject, List<string> attachementNames = null)
        {
            hasProject = false;
            try
            {
                #region Prereq

                if (actorCompanyId == 0 || invoiceId == 0)
                    return null;

                Company company = CompanyManager.GetCompany(actorCompanyId);
                if (company == null)
                    return null;

                #endregion

                #region Prereq

                //Get SysCurrencies
                List<SysCurrency> sysCurrencies = CountryCurrencyManager.GetSysCurrencies(true);

                //Get Billing settings
                decimal interestPercent = SettingManager.GetDecimalSetting(SettingMainType.Company, (int)CompanySettingType.CustomerInterestPercent, 0, actorCompanyId, 0);

                CustomerInvoice invoice = InvoiceManager.GetCustomerInvoice(invoiceId, true, true, false, false, true, true, false, true, false, false, false, false);

                if (invoice.ProjectId.HasValue && invoice.ProjectId > 0)
                    hasProject = true;

                CustomerInvoice creditedInvoice = null;
                if (invoice.IsCredit)
                {
                    if (!invoice.Origin.OriginInvoiceMapping.IsLoaded)
                    {
                        invoice.Origin.OriginInvoiceMapping.Load();
                    }
                    var originInvoiceMapping = invoice.Origin.OriginInvoiceMapping.FirstOrDefault(x => x.Type == (int)SoeOriginInvoiceMappingType.CreditInvoice);
                    if (originInvoiceMapping != null)
                        creditedInvoice = InvoiceManager.GetCustomerInvoice(originInvoiceMapping.InvoiceId);
                }

                List<CustomerInvoiceRow> invoiceRows = invoice.ActiveCustomerInvoiceRows.Where(i => i.Type == (int)SoeInvoiceRowType.TextRow || i.Type == (int)SoeInvoiceRowType.ProductRow || i.Type == (int)SoeInvoiceRowType.BaseProductRow || (i.Type == (int)SoeInvoiceRowType.AccountingRow && (i.IsFreightAmountRow || i.IsInvoiceFeeRow))).ToList();
                PaymentInformation paymentInformation = PaymentManager.GetPaymentInformationFromActor(company.ActorCompanyId, true, false);
                SysCurrency invoiceCurrency = sysCurrencies.FirstOrDefault(i => i.SysCurrencyId == invoice.Currency.SysCurrencyId);
                Customer customer = invoice.Actor?.Customer;

                List<ContactAddressRow> customerBillingAddress = new List<ContactAddressRow>();
                List<ContactAddressRow> customerDeliveryAddress = new List<ContactAddressRow>();
                List<ContactECom> customerContactEcoms;
                List<ContactAddressRow> companyAddress = new List<ContactAddressRow>();
                List<ContactAddressRow> companyBoardHQAddress = new List<ContactAddressRow>();
                List<ContactECom> companyContactEcoms = new List<ContactECom>();

                #endregion

                #region Billing And Delivery Address

                Contact customerContactPreferences = ContactManager.GetContactFromActor(invoice.Actor.ActorId);
                if (customerContactPreferences != null)
                {
                    //ContactAddressRow Billing            
                    customerBillingAddress = ContactManager.GetContactAddressRows(customerContactPreferences.ContactId, (int)TermGroup_SysContactAddressType.Billing);
                    //use the billingaddress on the invoie if it exits
                    if (customerBillingAddress.Any(a => a.ContactAddress.ContactAddressId == invoice.BillingAddressId))
                        customerBillingAddress = customerBillingAddress.Where(a => a.ContactAddress.ContactAddressId == invoice.BillingAddressId).ToList();

                    //ContactAddressRow Delivery            
                    customerDeliveryAddress = ContactManager.GetContactAddressRows(customerContactPreferences.ContactId, (int)TermGroup_SysContactAddressType.Delivery);
                    //use the deliveryAddress on the invoie if it exits
                    if (customerDeliveryAddress.Any(a => a.ContactAddress.ContactAddressId == invoice.DeliveryAddressId))
                        customerDeliveryAddress = customerDeliveryAddress.Where(a => a.ContactAddress.ContactAddressId == invoice.DeliveryAddressId).ToList();

                    customerContactEcoms = ContactManager.GetContactEComs(customerContactPreferences.ContactId);
                }
                else
                {
                    customerContactEcoms = new List<ContactECom>();
                }

                #endregion

                #region CompanyAddress and ContactEcoms

                Contact companyContactPreferences = ContactManager.GetContactFromActor(company.ActorCompanyId);
                if (companyContactPreferences != null)
                {
                    companyContactEcoms = ContactManager.GetContactEComs(companyContactPreferences.ContactId);
                    companyAddress = ContactManager.GetContactAddressRows(companyContactPreferences.ContactId, (int)TermGroup_SysContactAddressType.Distribution);
                    companyBoardHQAddress = ContactManager.GetContactAddressRows(companyContactPreferences.ContactId, (int)TermGroup_SysContactAddressType.BoardHQ);
                }

                #endregion

                bool printHasTaxBillLabel = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BillingPrintTaxBillText, 0, actorCompanyId, 0);
                String exemptionReason = String.Empty;
                if (printHasTaxBillLabel)
                    exemptionReason = GetText(5975, "Godkänd för F-skatt");

                SvefakturaItem svefakItem = new SvefakturaItem(invoice, paymentInformation, company, invoiceCurrency, customer, invoiceRows, customerBillingAddress, customerDeliveryAddress, customerContactEcoms, companyAddress, companyBoardHQAddress, companyContactEcoms, exemptionReason, creditedInvoice, actorCompanyId, userId, hasProject ? timeReportFilename : String.Empty, attachementNames);

                //Document
                XDocument svefakXml = new XDocument();

                svefakItem.ToXml(ref svefakXml);
                bool valid = svefakItem.Validate();//validate to log errors but always return the xml
                return svefakXml;
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                return new XDocument();
            }
        }
        private XDocument CreateSveFakturaSBDHEnvelope(string companyVatNr, string companyName)
        {

            //Namespaces
            XNamespace sh = "urn:sfti:documents:StandardBusinessDocumentHeader";
            XNamespace ns = "urn:sfti:documents:BasicInvoice:1:0";
            XNamespace ns_xsi = "http://www.w3.org/2001/XMLSchema-instance";

            //root
            XElement root = new XElement(sh + "StandardBusinessDocument",
                new XAttribute(XNamespace.Xmlns + "sh", sh),
                new XAttribute(XNamespace.Xmlns + "xsi", ns_xsi));

            #region StandardBusinessDocumentHeader

            XElement header = new XElement(sh + "StandardBusinessDocumentHeader");
            root.Add(header);

            XElement element = new XElement(sh + "Sender");
            XElement identifier = new XElement(sh + "Identifier",
                new XAttribute("Authority", "countrycode:organizationid"), companyVatNr);
            element.Add(identifier);
            root.Add(element);

            element = new XElement(sh + "Sender");
            identifier = new XElement(sh + "Identifier",
                new XAttribute("Authority", "operatorid"), companyName);
            element.Add(identifier);
            root.Add(element);

            element = new XElement(sh + "Reciever");
            identifier = new XElement(sh + "Identifier",
                new XAttribute("Authority", "countrycode:organizationid"), "");
            element.Add(identifier);
            root.Add(element);

            element = new XElement(sh + "Reciever");
            identifier = new XElement(sh + "Identifier",
                new XAttribute("Authority", "operatorid"), "");
            element.Add(identifier);
            root.Add(element);
            #endregion

            #region DocumentIdentifier

            header = new XElement(sh + "DocumentIdentification");
            element = new XElement(sh + "Standard", ns);
            header.Add(element);
            element = new XElement(sh + "TypeVersion", "1.0");
            header.Add(element);
            element = new XElement(sh + "InstanceIdentifier", "1");
            header.Add(element);
            element = new XElement(sh + "Type", "BasicInvoice");
            header.Add(element);
            element = new XElement(sh + "MultipleType", "false");
            header.Add(element);
            element = new XElement(sh + "CreationDateAndTime", DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"));
            header.Add(element);
            root.Add(header);

            #endregion DocumentIdentifier
            XDocument envelope = new XDocument(
                new XDeclaration("1.0", "utf-8", "true"));
            envelope.Add(root);
            return envelope;
        }

        #endregion

        #region Status

        /*public ActionResult UpdateDistributionStatus(bool success, string msgId, int status, string message)
        {
            using (var entities = new CompEntities())
            {
                var export = entities.InvoiceDistribution.FirstOrDefault(p => p.MsgId == msgId);
                if (export != null)
                {
                    TermGroup_PaymentTransferStatus exportStatus = (TermGroup_PaymentTransferStatus)status;

                    if (exportStatus != TermGroup_PaymentTransferStatus.None && export.TransferStatus != (int)exportStatus)
                    {
                        export.TransferMsg = message;
                        export.TransferStatus = (int)exportStatus;

                        return SaveChanges(entities);
                    }

                    return new ActionResult { Success = true, InfoMessage = "Payment found but with wrong status:" + msgId };
                }
            }

            return new ActionResult("Payment export not found:" + msgId);
        }*/

        #endregion
    }
}
