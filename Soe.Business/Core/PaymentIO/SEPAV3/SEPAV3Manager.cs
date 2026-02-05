using SoftOne.Common.KeyVault;
using SoftOne.Common.KeyVault.Models;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util.Exceptions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Xml;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Core.PaymentIO.SEPAV3
{
    public class SEPAV3Manager : PaymentIOManager
    {
        #region Variables

        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private const string Camt53xmlns = "urn:iso:std:iso:20022:tech:xsd:camt.053.001.02";
        private const string Camt54xmlns = "urn:iso:std:iso:20022:tech:xsd:camt.054.001.02";

        #endregion

        #region Ctor

        public SEPAV3Manager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Export

        public ActionResult Export(CompEntities entities, List<SysCountry> sysCountries, List<SysCurrency> sysCurrencies, TransactionScope transaction, PaymentMethod paymentMethod, List<PaymentRow> paymentRows, int actorCompanyId, TermGroup_PaymentTransferStatus initialTransferStatus, bool sendPaymentFile)
        {
            byte[] file;
            
            //Currency
            var baseCurrency = CountryCurrencyManager.GetCompanyBaseCurrencyDTO(entities, actorCompanyId);
            var company = CompanyManager.GetCompany(entities, actorCompanyId);

            if (!company.SysCountryId.HasValue)
            {
                return new ActionResult(7601, GetText(7601, "Land är inte angivet på aktuellt företag") + ": " + company.Name);
            }

            var result = new ActionResult();

            foreach (PaymentRow paymentRow in paymentRows)
            {
                //Bank does not approve date's in the past in payment file
                if (paymentRow.PayDate.CompareTo(DateTime.Now.Date) < 0)
                    paymentRow.PayDate = DateTime.Now.Date;
            }

            var containsForeignCurrenciesOrSuppliers = paymentRows.Where(i => (i.Invoice.CurrencyId != baseCurrency.CurrencyId) || (i.Invoice.Actor.Supplier.SysCountryId.GetValueOrDefault() > 0 && i.Invoice.Actor.Supplier.SysCountryId != company.SysCountryId) );

            if (containsForeignCurrenciesOrSuppliers.Any() && paymentMethod.PaymentInformationRow.SysPaymentTypeId != (int)TermGroup_SysPaymentType.BIC)
            {
                var first = containsForeignCurrenciesOrSuppliers.First();
                return new ActionResult(7611, GetText(7611, "Vid internationella betalningar ska från kontot vara av typen IBAN") + $":\n { first.Invoice.InvoiceNr }:{first.Invoice.Actor.Supplier.Name }");
            }

            if (paymentMethod.PaymentInformationRow == null)
            {
                return new ActionResult(7745, GetText(7745, "Betalningsnr är inte satt på betalningsmetod") + ": " + paymentMethod.Name);
            }

            //Swedbank dont allow IBAN => BG/PG
            if (SEPABase.IsSwedbank( paymentMethod.PaymentInformationRow.BIC) && paymentMethod.PaymentInformationRow.SysPaymentTypeId == (int)TermGroup_SysPaymentType.BIC)
            {
                if (paymentRows.Any(x => x.SysPaymentTypeId == (int)TermGroup_SysPaymentType.PG || x.SysPaymentTypeId == (int)TermGroup_SysPaymentType.BG))
                {
                    return new ActionResult(4878, GetText(7673, "Swedbank tillåter inte betalningar från IBAN till BG/PG"));
                }
            }
            

            //validate amounts to be payed
            List<DateTime> payDates = paymentRows.Select(n => n.PayDate.Date).Distinct().OrderByDescending(a => a.Date.Date).ToList();
            foreach (DateTime payDate in payDates)
            {
                List<PaymentRow> paymentsForTheDate = paymentRows.Where(n => n.PayDate.Date == payDate.Date).ToList();

                List<string> paymentNumbers = paymentsForTheDate.Select(n => n.PaymentNr).Distinct().ToList();
                foreach (string paymentNumber in paymentNumbers)
                {
                    List<PaymentRow> paymentsForTheSupplier = paymentsForTheDate.Where(n => n.PaymentNr == paymentNumber).ToList();
                    decimal checkSum = paymentsForTheSupplier.Sum(r => r.Amount);
                    if (checkSum <= 0)
                    {
                        //Total amount of supplier's payments for the same day can't be negative or zero
                        result.ErrorMessage = string.Format(GetText(4862, 1), paymentsForTheSupplier[0].Invoice.ActorNr + " - " + paymentsForTheSupplier[0].Invoice.ActorName, payDate.ToShortDateString());
                        result.Success = false;
                        return result;
                    }
                }
            }

            string messageGuid = Guid.NewGuid().ToString("N");
            var exportSettings = new PaymentExportSettings(messageGuid)
            {
                AggregatePayments = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierAggregatePaymentsInSEPAExportFile, 0, actorCompanyId, 0),
                ForeignBank = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierPaymentForeignBankCode, 0, actorCompanyId, 0),
                FileName = (paymentMethod.SysPaymentMethodId == (int)TermGroup_SysPaymentMethod.ISO20022) ? Utilities.GetISO20022FileNameOnServer(messageGuid) : Utilities.GetSEPAFileNameOnServer(messageGuid),
            };

            if (sendPaymentFile && SEPABase.IsNordea(paymentMethod.PaymentInformationRow.BIC))
            {
                exportSettings.HeaderSignerId = GetNordeaBankIntegrationSignerID();
                exportSettings.HeaderSignerName = "Softone AB";
                exportSettings.HeaderSignerSchemaName = "CUST";

                if (string.IsNullOrEmpty(exportSettings.HeaderSignerId))
                {
                    return new ActionResult("Nordea-Softone SignerId for bankintegration was not found");
                }
            }
            else if (sendPaymentFile && SEPABase.IsSEB(paymentMethod.PaymentInformationRow.BIC))
            {
                exportSettings.HeaderSignerId = GetSEBBankServiceId();
                exportSettings.HeaderSignerName = "SoftOne AB";
                exportSettings.HeaderSignerSchemaName = "BANK";
            }

            try
            {
                file = CreateExportFileInMemory(entities, company, sysCountries, sysCurrencies, paymentRows, actorCompanyId, exportSettings, paymentMethod, containsForeignCurrenciesOrSuppliers.Any());
                if (file != null)
                    result = CreatePaymentExport(exportSettings.FileName, paymentRows, (TermGroup_SysPaymentMethod)paymentMethod.SysPaymentMethodId, "", exportSettings.MessageGuid, file, initialTransferStatus);
                else
                    result.Success = false;
            }
            catch (ActionFailedException ex)
            {
                base.LogError(ex, this.log);
                result.ErrorNumber = ex.ErrorNumber;
                result.ErrorMessage = ex.Message;
                result.Success = false;
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result.Exception = ex;
            }

            return result;
        }

        #endregion

        #region Import

        public ActionResult Camt53Contains(string fileContent, out bool hasCustomerPayments, out bool hasSupplierPayments)
        {
            hasCustomerPayments = false;
            hasSupplierPayments = false;

            var result = new ActionResult(true);
            try
            {
                XDocument xdoc = XDocument.Parse(fileContent);

                var nameSpace = xdoc.Root.Name.Namespace;
                if (nameSpace == null || nameSpace.NamespaceName != Camt53xmlns)
                {
                    return new ActionResult(8176, "Kan inte läsa från XML fil");
                }

                var ntrys = xdoc.Descendants(nameSpace + "Ntry").ToList();
                if (!ntrys.Any() )
                {
                    return new ActionResult {InfoMessage= GetText(7598, "Hittade inga betalningar att importera")};
                }

                foreach (var ntry in ntrys)
                {
                    var cdtDbtInd = (from e in ntry.Elements()
                                     where e.Name.LocalName == "CdtDbtInd"
                                     select e).FirstOrDefault();

                    if ( (cdtDbtInd != null) && cdtDbtInd.Value == "CRDT")
                    {
                        hasCustomerPayments = true;
                    }
                    else if((cdtDbtInd != null) && cdtDbtInd.Value == "DBIT")
                    {
                        hasSupplierPayments = true;
                    }
                }
            }
            catch (XmlException ex)
            {
                base.LogError(ex, this.log);
                return new ActionResult(8176, "Kan inte läsa från XML fil" + ex.Message);
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result.Exception = ex;
            }

            return result;
        }

        public ActionResult ImportCAMT53Extended(StreamReader sr, int actorCompanyId, string fileName, int paymentMethodId, ref List<string> log, int paymentImportId, int batchId, ImportPaymentType importType)
        {
            var result = new ActionResult(true);
            var files = new List<SEPAFile>();

            try
            {
                string fileContent = sr.ReadToEnd();
                XDocument xdoc = XDocument.Parse(fileContent);

                var nameSpace = xdoc.Root.Name.Namespace;
                if (nameSpace == null || nameSpace.NamespaceName != Camt53xmlns)
                {
                    return new ActionResult(8176, "Kan inte läsa från XML fil");
                }

                var pGroupNtrys = xdoc.Descendants(nameSpace + "Ntry").ToList();


                ReadCAMTEntryDetails(importType, files, nameSpace, pGroupNtrys);
            }
            catch (XmlException ex)
            {
                base.LogError(ex, this.log);
                return new ActionResult(8176, "Kan inte läsa från XML fil");
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result.Exception = ex;
            }

            if (importType == ImportPaymentType.CustomerPayment)
            {
                var customerpayments = files.Where(f => f.AccountTransactionType == AccountTransactionTypeIndicator.CRDT).ToList();
                if (customerpayments.Any())
                {
                    result = ConvertCustomerStreamToEntity(customerpayments, actorCompanyId, ref log, paymentImportId, batchId, ImportPaymentType.CustomerPayment);
                }
            }
            else if (importType == ImportPaymentType.SupplierPayment)
            {
                var supplierpayments = files.Where(f => f.AccountTransactionType == AccountTransactionTypeIndicator.DBIT).ToList();
                if (supplierpayments.Any())
                {
                    using var entities = new CompEntities();
                    var dataAccess = new SupplierPaymentDataAccess(entities, PaymentManager, SettingManager);
                    var conversionResult = ConvertSupplierStreamToEntity(supplierpayments, actorCompanyId, SoeOriginType.SupplierPayment, ref log, paymentImportId, batchId, ImportPaymentType.SupplierPayment, dataAccess);
                    result = conversionResult.Result;
                    if (result.Success)
                    {
                        result = AddPaymentsImports(entities, actorCompanyId, paymentImportId, conversionResult.PaymentImports, conversionResult.ImportDate);
                    }
                }
            }

            return result;
        }

        public ActionResult ImportOnboardingFile(string fileContent, int actorCompanyId)
        {
            var onboardingFiles = new List<SEPAAccountOnboarding>();

            XDocument xdoc = XDocument.Parse(fileContent);

            /*
            var nameSpace = xdoc.Root.Name.Namespace;
            
            if (nameSpace == null || nameSpace.NamespaceName != "urn:iso:std:iso:20022:tech:xsd:pain.002.001.03")
            {
                return new ActionResult(8176, "Kan inte läsa från XML fil");
            }
            */

            XElement documentRoot =
                (from e in xdoc.Elements()
                 where e.Name.LocalName == "Document"
                 select e).FirstOrDefault();

            XElement application =
                    (from e in documentRoot.Elements()
                     where e.Name.LocalName == "Application"
                     select e).FirstOrDefault();

            if (application == null)
            {
                return new ActionResult(8176, "Kan inte läsa från XML fil");
            }

            List<XElement> customers =
                    (from e in application.Elements()
                     where e.Name.LocalName == "Cust"
                     select e).ToList();


            foreach (var customer in customers)
            {
                var onBoarding = new SEPAAccountOnboarding();

                onBoarding.CompanyName = XmlUtil.GetChildElementValue(customer, "Nm");
                onBoarding.OrgNr = XmlUtil.GetChildElementValue(customer, "OrgNb");

                var acct  =
                    (from e in customer.Elements()
                     where e.Name.LocalName == "Acct"
                     select e).FirstOrDefault();

                onBoarding.BIC = XmlUtil.GetChildElementValue(acct, "BIC");
                onBoarding.Connected = XmlUtil.GetChildElementValue(acct, "regAction") == "1";

                onBoarding.IBAN = XmlUtil.GetDescendantElementValue(acct, "AcctNb", "IBAN");
                onBoarding.BGNR = XmlUtil.GetDescendantElementValue(acct, "AcctNb", "BGNR");
                onBoarding.BGNR = XmlUtil.GetDescendantElementValue(acct, "AcctNb", "Curr");
                onboardingFiles.Add(onBoarding);
            }

            return ConvertOnboardingStreamToEntity(onboardingFiles);
        }

        private ActionResult ConvertOnboardingStreamToEntity(List<SEPAAccountOnboarding> bankAccounts)
        {
            var result = new ActionResult();
            var errorList = new List<string>();
            var noOfCompaniesFound = 0;

            using (var entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {

                        foreach (var account in bankAccounts)
                        {
                            var companies = CompanyManager.GetCompaniesBySearch(entities, new CompanySearchFilterDTO { OrgNr = account.OrgNr });
                            if (companies.Count == 1)
                            {
                                noOfCompaniesFound++;
                                var company = companies.First();
                                var paymentInformation = PaymentManager.GetPaymentInformationFromActor(entities, company.ActorCompanyId, true, false);
                                PaymentInformationRow paymentInformationRow = paymentInformation != null ? paymentInformation.ActivePaymentInformationRows.FirstOrDefault(x => x.BIC == account.BIC && x.PaymentNr == account.IBAN) : null;

                                if (paymentInformation == null)
                                {
                                    var rowDto = new PaymentInformationRowDTO { BIC = account.BIC, PaymentNr = account.IBAN, SysPaymentTypeId = (int)TermGroup_SysPaymentType.BIC, BankConnected = account.Connected };
                                    result = PaymentManager.SavePaymentInformation(entities, transaction, new List<PaymentInformationRowDTO> { rowDto }, company.ActorCompanyId, 0, company.ActorCompanyId, false, false, SoeEntityType.Company);
                                    if (!result.Success)
                                    {
                                        return result;
                                    }
                                }
                                else if (paymentInformation != null && paymentInformationRow != null && paymentInformationRow.BankConnected != account.Connected)
                                {
                                    paymentInformationRow.BankConnected = account.Connected;
                                }
                                else if (paymentInformation != null && paymentInformationRow == null)
                                {
                                    PaymentManager.AddPaymentInformationRowToPaymentInformation(paymentInformation, TermGroup_SysPaymentType.BIC, account.IBAN, false, account.BIC, account.Connected);
                                }
                            }
                            else
                            {
                                errorList.Add($"Failed finding company for bank onboarding:{account.CompanyName}:{account.OrgNr}");
                            }
                        }

                        result = SaveChanges(entities);

                        // Commit transaction
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.IntegerValue = 0;
                }
                finally
                {
                    if (!result.Success)
                    {
                        base.LogTransactionFailed(this.ToString(), this.log);
                    }

                    entities.Connection.Close();
                }
            }

            result.Success = noOfCompaniesFound > 0;
            result.IntegerValue = noOfCompaniesFound;
            result.ErrorMessage = string.Join("\n", errorList.ToArray());
            
            return result;
        }

        public bool IsPain002_V3(XDocument doc)
        {
            var nameSpace = doc.Root.Name.Namespace;
            if (nameSpace != null && nameSpace.NamespaceName == "urn:iso:std:iso:20022:tech:xsd:pain.002.001.03")
                return true;
            else
                return false;
        }

        public ActionResult ImportPain002(XDocument xdoc)
        {
            var messageStatus = new SEPAMessageStatus();

            if (!IsPain002_V3(xdoc))
                return new ActionResult(8176, "Kan inte läsa från XML fil");

            XElement documentRoot =
                (from e in xdoc.Elements()
                 where e.Name.LocalName == "Document"
                 select e).FirstOrDefault();

            XElement report =
                    (from e in documentRoot.Elements()
                     where e.Name.LocalName == "CstmrPmtStsRpt"
                     select e).FirstOrDefault();

            XElement originalGroupInfo =
                    (from e in report.Elements()
                     where e.Name.LocalName == "OrgnlGrpInfAndSts"
                     select e).FirstOrDefault();

            messageStatus.OrgMessageId = XmlUtil.GetChildElementValue(originalGroupInfo, "OrgnlMsgId");
            messageStatus.MessageStatus = XmlUtil.GetChildElementValue(originalGroupInfo, "GrpSts");

            if (string.IsNullOrEmpty(messageStatus.OrgMessageId))
            {
                return new ActionResult("Failed finding original message id in file");
            }

            List<XElement> orgnlPmtInfAndSts =
                    (from e in report.Elements()
                     where e.Name.LocalName == "OrgnlPmtInfAndSts"
                     select e).ToList();

            if (orgnlPmtInfAndSts.Any())
            {
                foreach (var orgPmtInf in orgnlPmtInfAndSts)
                {
                    var orgPaymentId = XmlUtil.GetChildElementValue(orgPmtInf, "OrgnlPmtInfId");
                    XElement txInfAndSts = (from e in orgPmtInf.Elements()
                                            where e.Name.LocalName == "TxInfAndSts"
                                            select e).FirstOrDefault();

                    var orgEndToEndId = XmlUtil.GetChildElementValue(txInfAndSts, "OrgnlEndToEndId");
                    var transStatus = XmlUtil.GetChildElementValue(txInfAndSts, "TxSts");

                    var statusObject = GetTransactionStatus(txInfAndSts, orgEndToEndId, orgPaymentId, transStatus);
                    messageStatus.PaymentStatuses.Add(statusObject);
                }
            }
            else
            {
                var statusObject = GetTransactionStatus(originalGroupInfo, null, null, messageStatus.MessageStatus);
                messageStatus.PaymentStatuses.Add(statusObject);
            }

            return ConvertPain002StreamToEntity(messageStatus);
        }

        private SEPATransactionStatus GetTransactionStatus(XElement parentElement, string orgEndToEndId, string orgPaymentId, string transStatus)
        {
            XElement stsRsnInf =
                               (from e in parentElement.Elements()
                               where e.Name.LocalName == "StsRsnInf"
                               select e).FirstOrDefault();

            var statusObject = new SEPATransactionStatus
            {
                OrgEndToEndId = orgEndToEndId,
                OrgPaymentId = orgPaymentId,
                Status = transStatus
            };

            if (stsRsnInf != null)
            {
                var AddtlInfList = (from e in stsRsnInf.Elements()
                                        where e.Name.LocalName == "AddtlInf"
                                        select e).ToList();

                foreach(var addtlInf in AddtlInfList)
                {
                    statusObject.ErrorMessage += addtlInf.Value;
                }
                
                statusObject.ErrorCode = XmlUtil.GetDescendantElementValue(stsRsnInf, "Rsn", "Cd");
            }

            return statusObject;
        }

        private ActionResult ConvertPain002StreamToEntity(SEPAMessageStatus sepaStatus)
        {
            using (var entities = new CompEntities())
            {
                var export = entities.PaymentExport.Include("Payment.PaymentRow").Where(x => x.MsgId == sepaStatus.OrgMessageId && x.State == (int)SoeEntityState.Active).FirstOrDefault();
                if (export != null)
                {
                    var specificPayments = sepaStatus.PaymentStatuses.Where(x => x.OrgEndToEndId != null);
                    foreach (var status in specificPayments)
                    {
                        var endToEnd = status.OrgEndToEndId.Split(',');
                        var paymentRow = export.Payment.FirstOrDefault()?.PaymentRow.FirstOrDefault(x => x.PaymentRowId == Convert.ToInt32(endToEnd[0]));

                        if (paymentRow != null)
                        {
                            if (status.Status != ISO_Payment_TransactionStatus.ACCP.ToString() && 
                                status.Status != ISO_Payment_TransactionStatus.ACWC.ToString() &&
                                status.Status != ISO_Payment_TransactionStatus.PDNG.ToString())
                            {
                                paymentRow.Status = (int)SoePaymentStatus.Error;
                            }

                            if (status.Status != ISO_Payment_TransactionStatus.ACCP.ToString())
                            {
                                paymentRow.StatusMsg = $"{status.ErrorCode}: {status.ErrorMessage}";
                            }
                        }
                    }

                    if (sepaStatus.MessageStatus == ISO_Payment_TransactionStatus.ACCP.ToString() || sepaStatus.MessageStatus == ISO_Payment_TransactionStatus.ACWC.ToString())
                    {
                        export.TransferStatus = (int)TermGroup_PaymentTransferStatus.Completed;
                    }
                    else if (sepaStatus.MessageStatus == ISO_Payment_TransactionStatus.PART.ToString() || sepaStatus.MessageStatus == ISO_Payment_TransactionStatus.RJCT.ToString())
                    {
                        export.TransferStatus = (int)TermGroup_PaymentTransferStatus.BankError;
                        export.TransferMsg = sepaStatus.MessageStatus.ToString() + ": " + (sepaStatus.PaymentStatuses.FirstOrDefault()?.ErrorMessage?.Left(240) ?? "");
                    }

                    return SaveChanges(entities);
                }
                else
                {
                    return new ActionResult("Pain002 import failed finding payment:" + sepaStatus.OrgMessageId);
                }
            }
        }
        public ActionResult Import(StreamReader sr, int actorCompanyId, string fileName, int paymentMethodId, ref List<string> log, int paymentImportId, int batchId, ImportPaymentType importType)
        {
            var result = new ActionResult(true);

            var files = new List<SEPAFile>();
            List<PaymentMethod> paymentMethods;
            SoeOriginType originType = importType == ImportPaymentType.CustomerPayment ? SoeOriginType.CustomerPayment : SoeOriginType.SupplierPayment;

            using (var entities = new CompEntities())
            {
                #region Prereq

                //PaymentMethods
                paymentMethods = PaymentManager.GetPaymentMethods(entities, originType, actorCompanyId, false);

                #endregion
            }

            try
            {
                #region Parse
                var fileExt = Path.GetExtension(fileName).ToLower();
                if (fileExt != ".xml" && fileExt != ".nda")
                {
                    return new ActionResult(7593, GetText(8077, "Importen kunde inte slutföras. Kontrollera att du har valt rätt fil eller betalningsmetod."));
                }

                string incFile = sr.ReadToEnd();
                XDocument xdoc = XDocument.Parse(incFile);

                var nameSpace = xdoc.Root.Name.Namespace;
                if (nameSpace != null && nameSpace.NamespaceName == Camt53xmlns)
                {
                    sr.DiscardBufferedData();
                    sr.BaseStream.Seek(0, SeekOrigin.Begin);
                    return ImportCAMT53Extended(sr, actorCompanyId,fileName, paymentMethodId, ref log, paymentImportId,batchId, importType);
                }
                else if (nameSpace == null || nameSpace.NamespaceName != Camt54xmlns)
                {
                    return new ActionResult(8176, "Kan inte läsa från XML fil");
                }

                XElement documentRoot =
                    (from e in xdoc.Elements()
                     where e.Name.LocalName == "Document"
                     select e).FirstOrDefault();

                XElement pGroup =
                    (from e in documentRoot.Elements()
                     where e.Name.LocalName == "BkToCstmrDbtCdtNtfctn"
                     select e).FirstOrDefault();

                List<XElement> pGroupNctns =
                    (from e in pGroup.Elements()
                     where e.Name.LocalName == "Ntfctn"
                     select e).ToList();

                foreach (XElement pGroupNctn in pGroupNctns)
                {
                    XElement acct =
                        (from e in pGroupNctn.Elements()
                         where e.Name.LocalName == "Acct"
                         select e).FirstOrDefault();

                    XElement acctId =
                        (from e in acct.Elements()
                         where e.Name.LocalName == "Id"
                         select e).FirstOrDefault();

                    //Account that has received payments
                    string accountNumber = XmlUtil.GetChildElementValue(acctId, "IBAN");
                    if (string.IsNullOrEmpty(accountNumber))
                    {
                        acctId =
                          (from e in acctId.Elements()
                           where e.Name.LocalName == "Othr"
                           select e).FirstOrDefault();

                        accountNumber = XmlUtil.GetChildElementValue(acctId, "Id");
                    }

                    PaymentMethod paymentMethodFromFile = paymentMethods.FirstOrDefault(a => a.PaymentInformationRow?.PaymentNr != null &&
                                                                                        a.PaymentMethodId == paymentMethodId);
                    //&& a.PaymentInformationRow.PaymentNr.RemoveWhiteSpaceAndHyphen() == accountNumber);

                    //only import for the chosen bank!
                    if (paymentMethodFromFile == null || paymentMethodFromFile.PaymentMethodId != paymentMethodId)
                    {
                        continue;
                    }

                    List<XElement> pGroupNtrys =
                        (from e in pGroupNctn.Elements()
                         where e.Name.LocalName == "Ntry"
                         select e).ToList();
 
                    ReadCAMTEntryDetails(importType, files, nameSpace, pGroupNtrys);
                }

                #endregion

                if (files.Any())
                {
                    if (importType == ImportPaymentType.CustomerPayment)
                    {
                        result = ConvertCustomerStreamToEntity(files, actorCompanyId, ref log, paymentImportId, batchId, importType);
                    }
                    else
                    {
                        using (var entities = new CompEntities())
                        {
                            var dataAccess = new SupplierPaymentDataAccess(entities, PaymentManager, SettingManager);
                            var conversionResult = ConvertSupplierStreamToEntity(files, actorCompanyId, originType, ref log, paymentImportId, batchId, importType, dataAccess);
                            result = conversionResult.Result;
                            if (result.Success)
                            {
                                result = AddPaymentsImports(entities, actorCompanyId, paymentImportId, conversionResult.PaymentImports, conversionResult.ImportDate);
                            }
                        }
                    }
                }
                else
                {
                    return new ActionResult(7598, GetText(7598, "Hittade inga betalningar att importera"));
                }
            }
            catch (XmlException ex)
            {
                base.LogError(ex, this.log);
                return new ActionResult(8176, "Kan inte läsa från XML fil");
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result.Exception = ex;
            }

            return result;
        }

        internal void ReadCAMTEntryDetails(ImportPaymentType importType, List<SEPAFile> files, XNamespace nameSpace, List<XElement> pGroupNtrys)
        {
            foreach (XElement pGroupNtry in pGroupNtrys)
            {
                //debit/Credit account statment type...
                var CdtDbtInd = XmlUtil.GetChildElementValue(pGroupNtry, "CdtDbtInd");
                var accountTransactionType = GetTransactionTypeIndicator(CdtDbtInd);

                XElement bookDate =
                    (from e in pGroupNtry.Elements()
                    where e.Name.LocalName == "BookgDt"
                    select e).FirstOrDefault();

                if (bookDate == null)
                {
                    bookDate =
                    (from e in pGroupNtry.Elements()
                     where e.Name.LocalName == "ValDt"
                     select e).FirstOrDefault();
                }

                //Datetime doesnt give date out of file
                string bkgDate = XmlUtil.GetChildElementValue(bookDate, "Dt");
                bkgDate = bkgDate.Replace("T", " ");
                bkgDate = bkgDate.Replace("Z", "");
                DateTime? bookingDate = CalendarUtility.GetNullableDateTime(bkgDate, "yyyy-MM-dd");

                var bankTransactionCode = GetBankTransactionCode(pGroupNtry);
                var additionalEntryInformation = GetAdditionalEntryInformation(pGroupNtry);

                List<XElement> pGroupNtryDtlsMulti =
                    (from e in pGroupNtry.Elements()
                     where e.Name.LocalName == "NtryDtls"
                     select e).ToList();

                foreach (XElement pGroupNtryDtls in pGroupNtryDtlsMulti)
                {
                    List<XElement> paymentElements =
                        (from e in pGroupNtryDtls.Elements()
                         where e.Name.LocalName == "TxDtls"
                         select e).ToList();

                    foreach (XElement paymentElement in paymentElements)
                    {

                        XElement refs =
                            (from e in paymentElement.Elements()
                             where e.Name.LocalName == "Refs"
                             select e).FirstOrDefault();

                        var endToEndId = refs != null ? XmlUtil.GetChildElementValue(refs, "EndToEndId") : "";

                        XElement rmtInf =
                            (from e in paymentElement.Elements()
                             where e.Name.LocalName == "RmtInf"
                             select e).FirstOrDefault();

                        var strdMulti = rmtInf != null ?
                            (from e in rmtInf.Elements()
                             where e.Name.LocalName == "Strd"
                             select e).ToList() :
                             null;

                        var tryWith = false;

                        //we have cinv/scor with textrows...add it as extra rows so user can decide what to do
                        //Actuel => a lot of matched credit invoices added as textrows
                        if (rmtInf != null && !strdMulti.IsNullOrEmpty() && importType != ImportPaymentType.SupplierPayment)
                        {
                            var ustrds = (from e in rmtInf.Elements()
                                          where e.Name.LocalName == "Ustrd"
                                          select e).ToList();

                            if (!ustrds.IsNullOrEmpty())
                            {
                                files.AddRange( CreateFilesFromUstrds(paymentElement, bookingDate, ustrds, importType, accountTransactionType) );
                            }
                        }

                        if (!strdMulti.IsNullOrEmpty())
                        {
                            foreach (var strd in strdMulti)
                            {

                                XElement cdtrRefInf =
                                    (from e in strd.Elements()
                                     where e.Name.LocalName == "CdtrRefInf"
                                     select e).FirstOrDefault();

                                XElement rfrdDocInf =
                                    (from e in strd.Elements()
                                     where e.Name.LocalName == "RfrdDocInf"
                                     select e).FirstOrDefault();

                                if (cdtrRefInf != null)
                                {
                                    var file = CreateFileFromCdtrRefInf(nameSpace, bookingDate, paymentElement, strd, cdtrRefInf, endToEndId, importType, accountTransactionType, bankTransactionCode);
                                    if (file != null)
                                    {
                                        files.Add(file);
                                    }
                                }
                                else if (rfrdDocInf != null)
                                {
                                    var file = CreateFileFromRfrdDocInf(nameSpace, bookingDate, paymentElement, strd, rfrdDocInf, endToEndId, importType, accountTransactionType, bankTransactionCode);
                                    if (file != null)
                                    {
                                        files.Add(file);
                                    }
                                }
                                else
                                {
                                    //Danske bank special?
                                    tryWith = strd.Elements().Count() == 1 && strd.Elements().First().Name.LocalName == "AddtlRmtInf";
                                }
                            }
                        }
                        else
                        {
                            tryWith = true;
                        }

                        if (tryWith)
                        {
                            var file = (importType == ImportPaymentType.CustomerPayment)
                                        ||
                                       (
                                        importType == ImportPaymentType.None &&
                                        accountTransactionType == AccountTransactionTypeIndicator.CRDT
                                       ) ? 
                                       CreateCustomerFile(nameSpace, bookingDate, paymentElement, accountTransactionType, bankTransactionCode) : 
                                       CreateSupplierFile(nameSpace, bookingDate, paymentElement, endToEndId, accountTransactionType, bankTransactionCode);

                            if (file != null)
                            {
                                files.Add(file);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Parses a CAMT XML file and returns a list of SEPAFile objects.
        /// This method is intended for testing purposes.
        /// </summary>
        internal List<SEPAFile> ParseCAMTFile(string xmlContent, ImportPaymentType importType)
        {
            var files = new List<SEPAFile>();
            XDocument xdoc = XDocument.Parse(xmlContent);
            var nameSpace = xdoc.Root.Name.Namespace;

            // Try CAMT.054 structure first (Ntfctn/Ntry)
            var pGroupNtrys = xdoc.Descendants(nameSpace + "Ntry").ToList();

            if (pGroupNtrys.Any())
            {
                ReadCAMTEntryDetails(importType, files, nameSpace, pGroupNtrys);
            }

            return files;
        }

        private AccountTransactionTypeIndicator GetTransactionTypeIndicator(string type)
        {
            if (string.IsNullOrEmpty(type))
                return AccountTransactionTypeIndicator.Unkown;
            else if (type == "DBIT")
                return AccountTransactionTypeIndicator.DBIT;
            else if (type == "CRDT")
                return AccountTransactionTypeIndicator.CRDT;
            else
                return AccountTransactionTypeIndicator.Unkown;
        }  
        
        private SEPABankTransactionCode GetBankTransactionCode(XElement ntry)
        {
            //The BankTransactionCode (BkTxCd) is a mandatory element in the CAMT.053 file
            //and tells us what kind of transaction it is. Current usage is to determine if it's
            //a direct debit transfer, as SoftOne GO has special handling for those types of payments.

            string domain = string.Empty;
            string family = string.Empty;
            string subFamily = string.Empty;

            //Multiplicity of BkTxCd is 1..1
            XElement bankTransactionCodeNode = (from e in ntry.Elements()
                                                where e.Name.LocalName == "BkTxCd"
                                                select e).FirstOrDefault();

            if (bankTransactionCodeNode != null)
            {
                //Multiplicity of Domn is 0..1
                var domainNode = (from e in bankTransactionCodeNode.Elements()
                                  where e.Name.LocalName == "Domn"
                                  select e).FirstOrDefault();
                if (domainNode != null)
                {
                    //Multiplicity of Cd is 1..1
                    domain = XmlUtil.GetChildElementValue(domainNode, "Cd");
                    //Multiplicity of Fmly is 1..1
                    var familyNode = XmlUtil.GetChildElement(domainNode, "Fmly");
                    if (familyNode != null)
                    {
                        //Multiplicity of Cd is 1..1
                        family = XmlUtil.GetChildElementValue(familyNode, "Cd");
                        //Multiplicity of SubFmlyCd is 1..1
                        subFamily = XmlUtil.GetChildElementValue(familyNode, "SubFmlyCd");
                    }
                }
            }

            return new SEPABankTransactionCode(domain, family, subFamily);
        }

        private string GetAdditionalEntryInformation(XElement ntry)
        {
            // Supplementary details related to the entry.Reported if available
            // Multiplicity of AddtlNtryInf is 0..1
            return XmlUtil.GetChildElementValue(ntry, "AddtlNtryInf");
        }

        private XElement GetAmtDtlsAmt(XNamespace nameSpace, XElement paymentElement, bool txAmt, out AccountTransactionTypeIndicator amountTransactionType)
        {
            var subTagName = txAmt ? "TxAmt" : "InstdAmt";
            amountTransactionType = AccountTransactionTypeIndicator.Unkown;

            XElement amtDtls =
                    (from e in paymentElement.Elements()
                     where e.Name.LocalName == "AmtDtls"
                     select e).FirstOrDefault();

            if (amtDtls == null)
            {
                return null;
            }

            var subElement =
                   (from e in amtDtls.Elements()
                    where e.Name.LocalName == subTagName
                    select e).FirstOrDefault();

            amountTransactionType = GetTransactionTypeIndicator(subElement?.Descendants(nameSpace + "TP").FirstOrDefault()?.Value);

            if (subElement == null)
            {
                 subElement =
                   (from e in amtDtls.Elements()
                    where e.Name.LocalName == "PrtryAmt"
                    select e).FirstOrDefault(); 

                //danske bank special?
                if (subElement != null)
                {
                    amountTransactionType = GetTransactionTypeIndicator(subElement.Descendants(nameSpace + "Tp").FirstOrDefault()?.Value);
                }
            }

            if (subElement == null && !txAmt)
            {
                subElement =
                  (from e in amtDtls.Elements()
                   where e.Name.LocalName == "TxAmt"
                   select e).FirstOrDefault();
            }

            return subElement != null ? subElement.Descendants(nameSpace + "Amt").FirstOrDefault() : null;
        }

        private XElement GetParty(XElement paymentElement, ImportPaymentType importType, bool checkForUltmtDbtr = false)
        {
            XElement rltdPties =
               (from e in paymentElement.Elements()
                where e.Name.LocalName == "RltdPties"
                select e).FirstOrDefault();

            var tagName = importType == ImportPaymentType.CustomerPayment ? "Dbtr" : "Cdtr";

            XElement party = null;

            if (rltdPties != null)
            {
                party = (from e in rltdPties.Elements()
                         where e.Name.LocalName == tagName
                         select e).FirstOrDefault();

                if (party == null && importType == ImportPaymentType.CustomerPayment)
                {
                    party = (from e in rltdPties.Elements()
                             where e.Name.LocalName == "Cdtr"
                             select e).FirstOrDefault();

                    if (party == null && checkForUltmtDbtr)
                    {
                        party = (from e in rltdPties.Elements()
                                 where e.Name.LocalName == "UltmtDbtr"
                                 select e).FirstOrDefault();

                    }
                }
            }

            return party;
        }

        private SEPAFile CreateCustomerFile(XNamespace nameSpace, DateTime? bookingDate, XElement paymentElement, AccountTransactionTypeIndicator accountTransactionType, SEPABankTransactionCode transactionCode)
        {
            XElement rmtInf =
                (from e in paymentElement.Elements()
                 where e.Name.LocalName == "RmtInf"
                 select e).FirstOrDefault();

            string reference = null;
            string invoiceNr = "";

            if (rmtInf != null)
            {
                reference = XmlUtil.GetChildElementValue(rmtInf, "Ustrd");
                invoiceNr = rmtInf.Descendants(nameSpace + "Nb").FirstOrDefault()?.Value;
            }
            else
            {
                reference = XmlUtil.GetDescendantElementValue(paymentElement, "Refs", "Ref");
            }

            var party = GetParty(paymentElement, ImportPaymentType.CustomerPayment);
            var name = XmlUtil.GetChildElementValue(party, "Nm");

            #region Amount

            AccountTransactionTypeIndicator amountTransactionType;
            XElement pmtAmount = GetAmtDtlsAmt(nameSpace, paymentElement, true,out amountTransactionType);

            if (rmtInf == null && pmtAmount == null)
            {
                //Ntry/Amt
                pmtAmount = XmlUtil.GetChildElement(paymentElement.Parent?.Parent, "Amt");
            }

            var currency = XmlUtil.GetAttributeStringValue(pmtAmount, "Ccy");
            string pmtAmountStr = pmtAmount?.Value;

            #endregion

            decimal amount = Convert.ToDecimal(pmtAmountStr?.Trim(), CultureInfo.InvariantCulture);

            return new SEPAFile(bookingDate, amount, invoiceNr, currency, name, false, "", reference, accountTransactionType, transactionCode);
        }

        private SEPAFile CreateSupplierFile(XNamespace nameSpace, DateTime? bookingDate, XElement paymentElement, string endToEndId, AccountTransactionTypeIndicator accountTransactionType, SEPABankTransactionCode transactionCode)
        {
            string reference = null;
            var party = GetParty(paymentElement, ImportPaymentType.SupplierPayment); 
            string creditorName = XmlUtil.GetChildElementValue(party, "Nm");

            #region Amount

            AccountTransactionTypeIndicator amountTransactionType;
            XElement pmtAmount = GetAmtDtlsAmt(nameSpace, paymentElement, false, out amountTransactionType);

            #endregion

            
            XElement rmtInf =
            (from e in paymentElement.Elements()
                where e.Name.LocalName == "RmtInf"
                select e).FirstOrDefault();

            if (rmtInf != null)
            {
                reference = XmlUtil.GetChildElementValue(rmtInf, "Ustrd");
            }
            else
            {
                reference = XmlUtil.GetDescendantElementValue(paymentElement, "Refs", "Ref");
            }

            if (rmtInf == null && pmtAmount == null)
            {
                //Ntry/Amt
                pmtAmount = XmlUtil.GetChildElement(paymentElement.Parent?.Parent, "Amt");
            }
            

            string pmtAmountStr = pmtAmount?.Value;
            var currency = XmlUtil.GetAttributeStringValue(pmtAmount, "Ccy");
            decimal amount = Convert.ToDecimal(pmtAmountStr?.Trim(), CultureInfo.InvariantCulture);

            return new SEPAFile(bookingDate, amount, null, currency, creditorName, false, endToEndId, reference, accountTransactionType, transactionCode);
        }

        private List<SEPAFile> CreateFilesFromUstrds(XElement paymentElement, DateTime? bookingDate, List<XElement> ustrds, ImportPaymentType importType, AccountTransactionTypeIndicator accountTransactionType)
        {
            var files = new List<SEPAFile>();
            var party = GetParty(paymentElement, importType);
            string name = XmlUtil.GetChildElementValue(party, "Nm");

            foreach (var ustrd in ustrds)
            {
                var file = new SEPAFile(bookingDate.GetValueOrDefault(), 0, "", null, name, false, "", ustrd.Value, accountTransactionType, null);
                files.Add(file);
            }

            return files;
        }

        private SEPAFile CreateFileFromCdtrRefInf(XNamespace nameSpace, DateTime? bookingDate, XElement paymentElement, XElement strd, XElement cdtrRefInf, string endToEndId, ImportPaymentType importType, AccountTransactionTypeIndicator accountTransactionType, SEPABankTransactionCode transactionCode)
        {
            string paymentOCR = XmlUtil.GetChildElementValue(cdtrRefInf, "Ref");
            paymentOCR = Utilities.RemoveLeadingZeros(paymentOCR);
            paymentOCR = paymentOCR.RemoveWhiteSpace();

            var party = GetParty(paymentElement, importType);

            string name = XmlUtil.GetChildElementValue(party, "Nm");

            string reference = XmlUtil.GetChildElementValue(strd, "AddtlRmtInf");
            if (reference.Length > 140)
                reference = reference.Substring(0, 140);

            #region Amount

            AccountTransactionTypeIndicator amountTransactionType;
            XElement pmtAmountCreditMulti = strd.Descendants(nameSpace + "CdtNoteAmt").FirstOrDefault();
            XElement pmtAmountMulti = strd.Descendants(nameSpace + "RmtdAmt").FirstOrDefault();
            XElement pmtAmount = GetAmtDtlsAmt(nameSpace, paymentElement, importType == ImportPaymentType.CustomerPayment, out amountTransactionType);

            string pmtAmountStr = pmtAmountCreditMulti?.Value ?? pmtAmountMulti?.Value ?? pmtAmount?.Value;
            
            if (pmtAmountCreditMulti != null)
            {
                pmtAmountStr = "-" + pmtAmountStr;
            }

            var currency = XmlUtil.GetAttributeStringValue(pmtAmount, "Ccy");

            #endregion

            decimal amount = Convert.ToDecimal(pmtAmountStr?.Trim(), CultureInfo.InvariantCulture);
            if (amountTransactionType != AccountTransactionTypeIndicator.Unkown && amountTransactionType != accountTransactionType)
            {
                amount = -amount;
            }

            return new SEPAFile(bookingDate.GetValueOrDefault(), amount, null, currency, name, false, endToEndId, reference, accountTransactionType, transactionCode, paymentOCR);
        }

        private SEPAFile CreateFileFromRfrdDocInf(XNamespace nameSpace, DateTime? bookingDate, XElement paymentElement, XElement strd, XElement rfrdDocInf, string endToEndId, ImportPaymentType importType, AccountTransactionTypeIndicator accountTransactionType, SEPABankTransactionCode transactionCode)
        {
            string paymentInvoiceNo = XmlUtil.GetChildElementValue(rfrdDocInf, "Nb");
            paymentInvoiceNo = paymentInvoiceNo.RemoveWhiteSpace();

            string type = rfrdDocInf.Descendants(nameSpace + "Cd").FirstOrDefault()?.Value;

            var party = GetParty(paymentElement, importType, true);

            string name = XmlUtil.GetChildElementValue(party, "Nm");
            
            string reference = XmlUtil.GetChildElementValue(strd, "AddtlRmtInf");
            if (reference.Length > 140)
                reference = reference.Substring(0, 140);

            #region Amount

            XElement pmtAmount = null;

            bool isCredit = type == "CREN";
            if (type == "CREN")
            {
                pmtAmount = strd.Descendants(nameSpace + "CdtNoteAmt").FirstOrDefault();
            }
            else if (type == "CINV")
            {
                pmtAmount = strd.Descendants(nameSpace + "RmtdAmt").FirstOrDefault();
            }

            var baseCurrencyStr = "";
            var baseCurrencyAmountStr = "";
            if (pmtAmount == null)
            {
                AccountTransactionTypeIndicator amountTransactionType;
                pmtAmount = GetAmtDtlsAmt(nameSpace, paymentElement, importType == ImportPaymentType.CustomerPayment, out amountTransactionType);
            }
            else
            {
                AccountTransactionTypeIndicator amountTransactionType;
                var baseCurrencyAmount = GetAmtDtlsAmt(nameSpace, paymentElement,true, out amountTransactionType);
                if (baseCurrencyAmount != null)
                {
                    baseCurrencyAmountStr = baseCurrencyAmount?.Value;
                    baseCurrencyStr = XmlUtil.GetAttributeStringValue(baseCurrencyAmount, "Ccy");
                }
            }

            string pmtAmountStr = pmtAmount?.Value;
            var currency = XmlUtil.GetAttributeStringValue(pmtAmount, "Ccy");

            #endregion

            decimal amount = Convert.ToDecimal(pmtAmountStr?.Trim(), CultureInfo.InvariantCulture);

            var file = new SEPAFile(bookingDate.GetValueOrDefault(), amount, paymentInvoiceNo, currency, name, isCredit, endToEndId, reference, accountTransactionType, transactionCode);
            if (!string.IsNullOrEmpty(baseCurrencyAmountStr) && !string.IsNullOrEmpty(baseCurrencyStr) && baseCurrencyStr != currency)
            {
                decimal baseCurrencyAmount = Convert.ToDecimal(baseCurrencyAmountStr?.Trim(), CultureInfo.InvariantCulture);
                file.BaseCurrency = baseCurrencyStr;
                file.BaseCurrencyAmount = baseCurrencyAmount;
            }

            return file;
        }

        /// <summary>
        /// Finds the best matching payment row from a list of candidates using a scoring system.
        /// Logs a warning if multiple candidates have the same highest score.
        /// </summary>
        internal static PaymentRowInvoiceDTO FindBestMatchingPaymentRow(List<PaymentRowInvoiceDTO> candidates, SEPAFile item, Action<string> logger)
        {
            const int sameDatePoints = 1;
            const int sameInvoiceNrPoints = 2;

            if (candidates == null || candidates.Count == 0)
                return null;

            var scoredCandidates = candidates
                .Select(p => new
                {
                    PaymentRow = p,
                    Score = (p.InvoiceNr == item.InvoiceNr ? sameInvoiceNrPoints : 0) + (p.PayDate == item.PaidDate ? sameDatePoints : 0)
                })
                .OrderByDescending(x => x.Score)
                .ToList();

            var highestScore = scoredCandidates.First().Score;
            var topCandidates = scoredCandidates.Where(x => x.Score == highestScore).ToList();

            if (topCandidates.Count > 1)
                logger?.Invoke($"Multiple payment rows with same score ({highestScore}) for Amount={item.Amount}, PaidDate={item.PaidDate:yyyy-MM-dd}, InvoiceNr={item.InvoiceNr}: PaymentRowIds={string.Join(",", topCandidates.Select(x => x.PaymentRow.PaymentRowId))}");

            return topCandidates.First().PaymentRow;
        }

        /// <seealso cref="SoftOne.Soe.Business.Core.Tests.SEPAV3ManagerTests"/>
        internal ConvertSupplierStreamResult ConvertSupplierStreamToEntity(List<SEPAFile> files, int actorCompanyId, SoeOriginType paymentOriginType, ref List<string> logText, int paymentImportId, int batchId, ImportPaymentType importType, ISupplierPaymentDataAccess dataAccess, Action<string> warningLogger = null)
        {
            var conversionResult = new ConvertSupplierStreamResult();
            decimal totalInvoiceAmount = 0;
            var invoiceOrignType = paymentOriginType == SoeOriginType.CustomerPayment ? SoeOriginType.CustomerInvoice : SoeOriginType.SupplierInvoice;
            DateTime? importDate = null;

            try
            {
                //Fix paymentRowIds...Should be paymentRowId,paymentId,
                //convert to only use PaymentRowId, and handle credit invoices when combined with debets since they will have same EndToEndId but are on p
                var matchedPaymentRowsIds = new HashSet<int>();

                foreach (var endToEndGroup in files.GroupBy(g => g.EndToEndId))
                {
                    var paymentRowId = 0;
                    var paymentId = 0;
                    var multiplePayments = 0;
                    var split = endToEndGroup.Key.Split(',');

                    if (split.Length > 1)
                    {
                        int.TryParse(split[0], out paymentRowId);
                        int.TryParse(split[1], out paymentId);
                        if (split.Length > 2)
                        {
                            int.TryParse(split[2], out multiplePayments);
                        }
                    }

                    if (endToEndGroup.Count() == 1 && paymentRowId > 0 && multiplePayments <= 1)
                    {
                        endToEndGroup.First().EndToEndId = paymentRowId.ToString();
                        matchedPaymentRowsIds.Add(paymentRowId);
                    }   
                    else if (multiplePayments > 1 && endToEndGroup.Count() == 1 && paymentId > 0)
                    {
                        //multiple payments sent (credit) but only on returned
                        var first = endToEndGroup.First();
                        var paymentRows = dataAccess.GetPaymentRowsWithSupplierInvoice(paymentId, actorCompanyId);
                        var firstPaymentRowInKey = paymentRows.FirstOrDefault(p => p.PaymentRowId == paymentRowId);
                        if (firstPaymentRowInKey != null && !string.IsNullOrEmpty(firstPaymentRowInKey.PaymentNr))
                        {
                            var allWithSamePaymentNr = paymentRows.Where(x => !matchedPaymentRowsIds.Contains(x.PaymentRowId) && x.PaymentNr == firstPaymentRowInKey.PaymentNr && x.SysPaymentTypeId == firstPaymentRowInKey.SysPaymentTypeId && x.PaymentRowId != firstPaymentRowInKey.PaymentRowId);
                            foreach (var payment in allWithSamePaymentNr)
                            {
                                files.Add(new SEPAFile(first.PaidDate, payment.Amount, payment.InvoiceNr, first.CurrencyCode, first.Name, payment.PaymentRowId.ToString()));
                                matchedPaymentRowsIds.Add(payment.PaymentRowId);
                            }
                            first.Amount = firstPaymentRowInKey.Amount;
                            first.InvoiceNr = firstPaymentRowInKey.InvoiceNr;
                            first.EndToEndId = paymentRowId.ToString();
                            matchedPaymentRowsIds.Add(paymentRowId);
                        }
                    }
                    else if (paymentId > 0)
                    {
                        var paymentRows = dataAccess.GetPaymentRowsWithSupplierInvoice(paymentId, actorCompanyId);
                        var firstPaymentRowInKey = paymentRows.FirstOrDefault(p => p.PaymentRowId == paymentRowId);
                        if (firstPaymentRowInKey != null)
                        {
                            foreach (var item in endToEndGroup)
                            {
                                var paymentRowCandidates = paymentRows
                                    .Where(p => !matchedPaymentRowsIds.Contains(p.PaymentRowId)
                                             && p.AmountCurrency == item.Amount
                                             && p.InvoiceActorId == firstPaymentRowInKey.InvoiceActorId)
                                    .ToList();

                                var paymentRow = FindBestMatchingPaymentRow(paymentRowCandidates, item, warningLogger ?? (str => LogWarning(str)));
                                if (paymentRow != null)
                                {
                                    matchedPaymentRowsIds.Add(paymentRow.PaymentRowId);
                                    item.EndToEndId = paymentRow.PaymentRowId.ToString();
                                }
                            }
                        }
                    }
                }

                foreach (var post in files)
                {
                    string invoiceNr = string.IsNullOrEmpty(post.InvoiceNr) ? post.Reference ?? "" : post.InvoiceNr;
                    int paymentRowId = 0;
                    PaymentRowInvoiceDTO matchingPaymentRow = null;
                    if (!string.IsNullOrEmpty(post.EndToEndId) && int.TryParse(post.EndToEndId, out paymentRowId))
                    {
                        matchingPaymentRow = dataAccess.GetPaymentRowWithSupplierInvoice(paymentRowId, actorCompanyId);
                    }

                    #region PaymentImportIO

                        ImportPaymentIOStatus status = ImportPaymentIOStatus.Unknown;
                        ImportPaymentIOState state = ImportPaymentIOState.Open;
                        TermGroup_BillingType type = TermGroup_BillingType.None;

                        //Invoice status
                        if ((matchingPaymentRow?.InvoiceId).HasValue)
                        {
                            status = ImportPaymentIOStatus.Match;
                            state = ImportPaymentIOState.Open;

                            type = matchingPaymentRow.InvoiceTotalAmount >= 0 ? TermGroup_BillingType.Debit : TermGroup_BillingType.Credit;

                            if (post.Amount < matchingPaymentRow.InvoiceTotalAmount)
                            {
                                status = ImportPaymentIOStatus.PartlyPaid;
                            }

                            if (post.Amount > matchingPaymentRow.InvoiceTotalAmount)
                            {
                                status = ImportPaymentIOStatus.Rest;
                            }

                            if (matchingPaymentRow.FullyPayed)
                            {
                                if (importType == ImportPaymentType.CustomerPayment)
                                    status = ImportPaymentIOStatus.Unknown;
                                else
                                    status = ImportPaymentIOStatus.Paid;
                            }
                        }

                        //Paymentstatus
                        if (matchingPaymentRow != null)
                        {
                            if (matchingPaymentRow.Status == (int)SoePaymentStatus.None || matchingPaymentRow.Status == (int)SoePaymentStatus.Pending)
                            {
                                status = ImportPaymentIOStatus.Match;
                                state = ImportPaymentIOState.Open;
                            }
                            else if (matchingPaymentRow.Status == (int)SoePaymentStatus.Cancel)
                            {
                                //Strange that the payment has been canceled between export and import of result file?
                                status = ImportPaymentIOStatus.Error;
                                state = ImportPaymentIOState.Closed;
                            }
                        }

                        var bankTransactionCode = post.BankTransactionCode;
                        //string comment = null;
                        if (bankTransactionCode != null && bankTransactionCode.IsReceivedDirectDebit())
                        {
                            var automaticallyTransferToPayment = dataAccess.GetAutoTransferAutogiroSetting();
                            if (automaticallyTransferToPayment)
                            {
                                status = ImportPaymentIOStatus.Match;
                                state = ImportPaymentIOState.Closed;
                            }
                        }

                        var paymentImportIO = new PaymentImportIO
                        {
                            ActorCompanyId = actorCompanyId,
                            BatchNr = batchId,
                            Type = (int)type,
                            CustomerId = matchingPaymentRow?.InvoiceActorId.GetValueOrDefault(),
                            Customer = string.IsNullOrEmpty(post.Name) ? StringUtility.Left(matchingPaymentRow?.InvoiceActorName, 50) : StringUtility.Left(post.Name, 50),
                            InvoiceId = matchingPaymentRow?.InvoiceId ?? 0,
                            InvoiceNr = matchingPaymentRow != null ? matchingPaymentRow.InvoiceNr : invoiceNr,
                            InvoiceSeqnr = matchingPaymentRow?.InvoiceSeqNr.ToString(),
                            InvoiceAmount = matchingPaymentRow != null ? matchingPaymentRow.InvoiceTotalAmount - matchingPaymentRow.InvoicePaidAmount : 0,
                            RestAmount = matchingPaymentRow != null ? matchingPaymentRow.InvoiceTotalAmount - matchingPaymentRow.InvoicePaidAmount : 0,
                            PaidAmount = post.BaseCurrencyAmount > 0 ? post.BaseCurrencyAmount : post.Amount,
                            PaidAmountCurrency = post.Amount,
                            Currency = post.CurrencyCode,
                            InvoiceDate = matchingPaymentRow?.InvoiceDate,
                            DueDate = matchingPaymentRow?.InvoiceDueDate,
                            PaidDate = post.PaidDate,
                            MatchCodeId = 0,
                            Status = (int)status,
                            State = (int)state,
                            ImportType = (int)importType,
                        };

                        importDate = CalendarUtility.GetEarliestDate(importDate, paymentImportIO.PaidDate);

                        totalInvoiceAmount = totalInvoiceAmount + Utilities.GetAmount(post.Amount);

                        // Check for duplicates
                        if (conversionResult.PaymentImports.Any(p => p.CustomerId == paymentImportIO.CustomerId && p.InvoiceId == paymentImportIO.InvoiceId && p.PaidAmount == paymentImportIO.PaidAmount && p.PaidDate == paymentImportIO.PaidDate))
                        {
                            if (importType == ImportPaymentType.CustomerPayment)
                            {
                                paymentImportIO.Status = (int)ImportPaymentIOStatus.Unknown;
                                paymentImportIO.State = (int)ImportPaymentIOState.Open;
                            }
                            else
                            {
                                paymentImportIO.Status = (int)ImportPaymentIOStatus.Paid;
                                paymentImportIO.State = (int)ImportPaymentIOState.Closed;
                            }
                        }

                        conversionResult.PaymentImports.Add(paymentImportIO);

                        //special credit invoice handling when they are not reported back separatly in the file...instead the net sum is and that has endtoend for the debet invoice
                        //try to find matching credit invoices for this supplier for this paymentexport which wasnt picked up in the file parser
                        if (matchingPaymentRow != null && matchingPaymentRow.BillingType == (int)TermGroup_BillingType.Debit && paymentImportIO.PaidAmount < matchingPaymentRow.Amount)
                        {
                            var creditRows = dataAccess.GetPaymentRowsWithSupplierInvoice(matchingPaymentRow.PaymentId, actorCompanyId, matchingPaymentRow.InvoiceActorId, TermGroup_BillingType.Credit);
                            foreach(var creditRow in creditRows)
                            {
                                if (files.Any(f=> f.EndToEndId == creditRow.PaymentRowId.ToString()) )
                                {
                                    continue;
                                }

                                var creditPaymentImportIO = new PaymentImportIO
                                {
                                    ActorCompanyId = actorCompanyId,
                                    BatchNr = batchId,
                                    Type = (int)TermGroup_BillingType.Credit,
                                    CustomerId = creditRow.InvoiceActorId,
                                    Customer = string.IsNullOrEmpty(post.Name) ? StringUtility.Left(creditRow.InvoiceActorName, 50) : StringUtility.Left(post.Name, 50),
                                    InvoiceId = creditRow.InvoiceId,
                                    InvoiceNr = creditRow.InvoiceNr,
                                    InvoiceSeqnr = creditRow.InvoiceSeqNr.ToString(),
                                    InvoiceAmount = -(Math.Abs(creditRow.InvoiceTotalAmount) - Math.Abs(creditRow.InvoicePaidAmount)),
                                    RestAmount = -(Math.Abs(creditRow.InvoiceTotalAmount) - Math.Abs(creditRow.InvoicePaidAmount)),
                                    PaidAmount = creditRow.AmountCurrency,
                                    Currency = post.CurrencyCode,
                                    InvoiceDate = creditRow.InvoiceDate,
                                    DueDate = creditRow.InvoiceDueDate,
                                    PaidDate = post.PaidDate,
                                    MatchCodeId = 0,
                                    Status = (int)status,
                                    State = (int)state,
                                    ImportType = (int)importType,
                                };

                                conversionResult.PaymentImports.Add(creditPaymentImportIO);
                            }

                        }

                        logText.Add(paymentImportIO.Customer);

                        #endregion
                }

                conversionResult.ImportDate = importDate;
            }
            catch (Exception ex)
            {
                conversionResult.Result.Success = false;
                conversionResult.Result.Exception = ex;
            }

            return conversionResult;
        }

        private ActionResult ConvertCustomerStreamToEntity(List<SEPAFile> files, int actorCompanyId, ref List<string> logText, int paymentImportId, int batchId, ImportPaymentType importType)
        {

            var result = new ActionResult(true);
            var PaymentImportIOToAdd = new List<PaymentImportIO>();
            DateTime? importDate = null;
                
            using (var entities = new CompEntities())
            {
                try
                {
                    foreach (var post in files)
                    {
                        ImportPaymentIOStatus status = ImportPaymentIOStatus.Unknown;
                        ImportPaymentIOState state = ImportPaymentIOState.Open;
                        TermGroup_BillingType type = TermGroup_BillingType.None;

						CustomerInvoiceAmountDTO invoice = null;
						invoice ??= InvoiceManager.GetCustomerInvoiceAmount(entities, OCR: post.OCR);
						invoice ??= InvoiceManager.GetCustomerInvoiceAmount(entities, OCR: post.InvoiceNr);
						invoice ??= InvoiceManager.GetCustomerInvoiceAmount(entities, OCR: post.Reference);
						invoice ??= InvoiceManager.GetCustomerInvoiceAmount(entities, invoiceNr: post.InvoiceNr);
						invoice ??= InvoiceManager.GetCustomerInvoiceAmount(entities, invoiceNr: post.OCR);
						invoice ??= InvoiceManager.GetCustomerInvoiceAmount(entities, invoiceNr: post.Reference);
						invoice ??= InvoiceManager.GetCustomerInvoiceAmount(entities, externalId: post.OCR);
						invoice ??= InvoiceManager.GetCustomerInvoiceAmount(entities, externalId: post.InvoiceNr);
						invoice ??= InvoiceManager.GetCustomerInvoiceAmount(entities, externalId: post.Reference);
                        if (invoice != null)
                        {
                            status = ImportPaymentIOStatus.Match;
                            state = ImportPaymentIOState.Open;
                            type = invoice.TotalAmount >= 0 ? TermGroup_BillingType.Debit : TermGroup_BillingType.Credit;

                            bool isPartlyPaid = post.Amount < invoice.TotalAmount;
                            if (isPartlyPaid)
                            {
                                status = ImportPaymentIOStatus.PartlyPaid;
                            }

                            bool isRest = post.Amount > invoice.TotalAmount;
                            if (isRest)
                            {
                                status = ImportPaymentIOStatus.Rest;
                            }

                            bool isFullyPayed = invoice.FullyPayed;
                            if (isFullyPayed)
                            {
                                if (importType == ImportPaymentType.CustomerPayment)
                                    status = ImportPaymentIOStatus.Unknown;
                                else
                                    status = ImportPaymentIOStatus.Paid;
                            }
                        }

                        var paymentImportIO = new PaymentImportIO
                        {
                            ActorCompanyId = actorCompanyId,
                            BatchNr = batchId,
                            Status = (int)status,
                            State = (int)state,
                            Type = (int)type,
                            CustomerId = invoice?.ActorId ?? 0,
                            Customer = invoice != null ? StringUtility.Left(invoice.ActorName, 50) : StringUtility.Left(post.Name, 50),
                            InvoiceId = invoice?.InvoiceId ?? 0,
                            InvoiceNr = invoice?.InvoiceNr ?? "",
                            InvoiceAmount = invoice != null ? invoice.TotalAmount - invoice.PaidAmount : 0,
                            RestAmount = invoice != null ? invoice.TotalAmount - invoice.PaidAmount - post.Amount : 0,
                            PaidAmount = post.Amount,
                            Currency = post.CurrencyCode,
                            PaidDate = post.PaidDate,
                            InvoiceDate = invoice?.InvoiceDate,
                            DueDate = invoice?.DueDate,
                            MatchCodeId = 0,
                            ImportType = (int)importType,
                            OCR = post.OCR,
                        };

                        importDate = CalendarUtility.GetEarliestDate(importDate, paymentImportIO.PaidDate);

                        logText.Add(paymentImportIO.Customer);
                        PaymentImportIOToAdd.Add(paymentImportIO);
                    }

                    #region Add to DB

                    if (result.Success)
                    {
                        result = AddPaymentsImports(entities, actorCompanyId, paymentImportId, PaymentImportIOToAdd, importDate);
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                }
            }

            return result;
        }

        #endregion

        #region Help-methods

        private string GetNordeaBankIntegrationSignerID()
        {
            return KeyVaultSecretsFetcher.GetSecret("Nordea-Softone-SignerId");
        }
        private string GetSEBBankServiceId()
        {
            // This is SoftOne's id per the service agreement with SEB. 
            return "55623947170004";
        }

        private ActionResult AddPaymentsImports(CompEntities entities, int actorCompanyId, int paymentImportId, List<PaymentImportIO> PaymentImportIOToAdd, DateTime? fileDate)
        {
            int numberOfPayments = 1;
            var result = new ActionResult();
            foreach (var paymentIO in PaymentImportIOToAdd)
            {
                using (var transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                {
                    if (result.Success)
                    {
                        entities.PaymentImportIO.AddObject(paymentIO);

                        result = SaveEntityItem(entities, paymentIO, transaction);

                        if (result.Success)
                        {
                            PaymentImport paymentImport = PaymentManager.GetPaymentImport(entities, paymentImportId, actorCompanyId);

                            paymentImport.TotalAmount = paymentImport.TotalAmount + paymentIO.PaidAmount.Value;
                            paymentImport.NumberOfPayments = numberOfPayments++;
                            paymentImport.ImportDate = fileDate ?? DateTime.Now.Date;

                            result = PaymentManager.UpdatePaymentImportHead(entities, paymentImport, paymentImportId, actorCompanyId);
                            if (!result.Success)
                            {
                                return result;
                            }
                            //Commit transaction
                            transaction.Complete();
                        }
                        else
                        {
                            // Set result
                            result.ErrorNumber = (int)ActionResultSave.NothingSaved;
                            result.ErrorMessage = string.Format("Faktura med nr {0} är felaktig, importen är avbruten!", paymentIO.InvoiceNr);
                        }
                    }
                    else
                    {
                        transaction.Complete();
                    }
                }
            }
            return result;
        }
        public byte[] CreateExportFileInMemory(CompEntities entities, Company company, List<SysCountry> sysCountries, List<SysCurrency> sysCurrencies, IEnumerable<PaymentRow> paymentRows, int actorCompanyId, PaymentExportSettings exportSettings, PaymentMethod paymentMethod, bool containsForeignPayments)
        {
            SEPAModel sepaModel = new SEPAModel(entities, company, ContactManager, CountryCurrencyManager, PaymentManager, sysCountries, sysCurrencies, actorCompanyId, paymentRows, paymentMethod, exportSettings, containsForeignPayments);

            //Document            
            XDocument sepaXml = new XDocument(new XDeclaration("1.0", "UTF-8", "true"));

            sepaModel.ToXml(ref sepaXml);
            if (!sepaModel.Validate())
            {
                return null;
            }

            XmlWriter xw = null;
            MemoryStream ms = new MemoryStream();
            //Encoding cannot be set after creation thats why we have to create settings

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = Encoding.UTF8;
            try
            {
                xw = XmlWriter.Create(ms, settings);
                sepaXml.Save(xw);
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                return null;
            }
            finally
            {
                if (xw != null)
                    xw.Close();
            }

            return ms.ToArray();
        }

        #endregion
    }
}
