using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Xml;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Core.PaymentIO.SEPA
{
    public class SEPAManager : PaymentIOManager
    {
        #region Variables

        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public SEPAManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Export

        public ActionResult Export(CompEntities entities, List<SysCountry> sysCountries, List<SysCurrency> sysCurrencies, TransactionScope transaction, PaymentMethod paymentMethod, List<PaymentRow> paymentRows, int paymentId, int actorCompanyId, TermGroup_PaymentTransferStatus initialTransferStatus)
        {
            byte[] file;
            string messageGuid = Guid.NewGuid().ToString("N");
            var fileName = Utilities.GetSEPAFileNameOnServer(messageGuid);

            ActionResult result = new ActionResult();

            foreach (PaymentRow paymentRow in paymentRows)
            {
                //Bank does not approve date's in the past in payment file
                if (paymentRow.PayDate.CompareTo(DateTime.Now.Date) < 0)
                    paymentRow.PayDate = DateTime.Now.Date;
            }

            //validate amounts to be payed
            List<DateTime> payDates = paymentRows.Select(n => n.PayDate.Date).Distinct().ToList();
        
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
                        result.ErrorMessage = string.Format(GetText(4862, 1), paymentsForTheSupplier[0].Invoice.ActorName, payDate.ToShortDateString());
                        result.Success = false;
                        return result;
                    }
                }
            }

            try
            {
                file = CreateExportFileInMemory(entities, sysCountries, sysCurrencies, paymentRows, actorCompanyId, paymentMethod, messageGuid);
                if (file != null)
                    result = CreatePaymentExport(fileName, paymentRows, TermGroup_SysPaymentMethod.SEPA, "", messageGuid, file, initialTransferStatus);
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

        #region PAIN001

        public ActionResult Import(XElement dataStorageXDocRoot, string incFile, int actorCompanyId, string fileName, bool templateIsSuggestion, int paymentMethodId, int paymentMethodXmlId, ref List<string> log)
        {
            var result = new ActionResult(true);

            XElement SEPATransactionElement = null;
            int paymentXmlId = 1;
            string paymentDetails = string.Empty;
            Dictionary<int, decimal> invoicePayments = new Dictionary<int, decimal>();
            decimal totalPaymentAmount = 0;
            ActionResult savePaymentResult = new ActionResult(true);

            using (var entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    #region Settings

                    //AccountYear
                    int accountYearId = SettingManager.GetIntSetting(entities, SettingMainType.UserAndCompany, (int)UserSettingType.AccountingAccountYear, base.UserId, actorCompanyId, 0);

                    //PaymentMethods
                    List<PaymentMethod> paymentMethods = PaymentManager.GetPaymentMethods(entities, SoeOriginType.CustomerPayment, actorCompanyId, false);

                    #endregion

                    #region Prereq

                    var itemsToTransfer = new List<CustomerInvoiceGridDTO>();

                    string errorMessage = string.Empty;
                    string note = string.Empty;

                    #endregion

                    #region Parse

                    //Create Xdoc for reading incoming data
                    XDocument xdoc = XDocument.Parse(incFile);
                    var nameSpace = xdoc.Root.Name.Namespace;

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

                        XElement id =
                            (from e in acct.Elements()
                             where e.Name.LocalName == "Id"
                             select e).FirstOrDefault();

                        //Account that has received payments
                        string AccountNumber = XmlUtil.GetChildElementValue(id, "IBAN");

                        PaymentMethod paymentMethodFromFile = paymentMethods.FirstOrDefault(a => a.PaymentNr != null && a.PaymentNr.Contains(AccountNumber));

                        if (paymentMethodFromFile == null)
                        {
                            continue;
                        }

                        var paymentMethodElement = new XElement("PaymentMethod",
                            new XAttribute("Id", paymentMethodXmlId),
                            new XElement("PaymentMethodName", paymentMethodFromFile != null ? paymentMethodFromFile.Name : string.Empty),
                            new XElement("PaymentMethodPaymentNr", paymentMethodFromFile != null ? paymentMethodFromFile.PaymentNr : string.Empty),
                            new XElement("FileName", fileName));

                        paymentMethodXmlId++;
                        paymentXmlId = 1;

                        List<XElement> pGroupNtrys =
                            (from e in pGroupNctn.Elements()
                             where e.Name.LocalName == "Ntry"
                             select e).ToList();

                        foreach (XElement pGroupNtry in pGroupNtrys)
                        {
                            XElement bookDate =
                                (from e in pGroupNtry.Elements()
                                 where e.Name.LocalName == "BookgDt"
                                 select e).FirstOrDefault();

                            //Datetime doesnt give date out of file
                            string bkgDate = XmlUtil.GetChildElementValue(bookDate, "Dt");
                            bkgDate = bkgDate.Replace("T", " ");
                            bkgDate = bkgDate.Replace("Z", "");
                            DateTime? bookingDate = CalendarUtility.GetNullableDateTime(bkgDate, "yyyy-MM-dd");

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
                                    //We get only invoice ocr from Sepa file not invoicenumber
                                    XElement rmtInf =
                                        (from e in paymentElement.Elements()
                                         where e.Name.LocalName == "RmtInf"
                                         select e).FirstOrDefault();


                                    var strdMulti =
                                        (from e in rmtInf.Elements()
                                         where e.Name.LocalName == "Strd"
                                         select e).ToList();

                                    foreach (var strd in strdMulti)
                                    {

                                        XElement cdtrRefInf =
                                            (from e in strd.Elements()
                                             where e.Name.LocalName == "CdtrRefInf"
                                             select e).FirstOrDefault();

                                        string paymentOCR = XmlUtil.GetChildElementValue(cdtrRefInf, "Ref");
                                        paymentOCR = Utilities.RemoveLeadingZeros(paymentOCR);
                                        paymentOCR = paymentOCR.RemoveWhiteSpace();

                                        paymentDetails = GetText(921, (int)TermGroup.Report, "Viitenumero") + ": " + paymentOCR;

                                        XElement rltdPties =
                                            (from e in paymentElement.Elements()
                                             where e.Name.LocalName == "RltdPties"
                                             select e).FirstOrDefault();

                                        XElement dbtName =
                                            (from e in rltdPties.Elements()
                                             where e.Name.LocalName == "Dbtr"
                                             select e).FirstOrDefault();

                                        string dbtorName = XmlUtil.GetChildElementValue(dbtName, "Nm");

                                        if (paymentDetails != "")
                                            paymentDetails += ", ";
                                        paymentDetails += GetText(922, (int)TermGroup.Report, "Nimi") + ": " + dbtorName;

                                        XElement bankNbr =
                                            (from e in paymentElement.Elements()
                                             where e.Name.LocalName == "Refs"
                                             select e).FirstOrDefault();

                                        string bankNumber = XmlUtil.GetChildElementValue(bankNbr, "AcctSvcrRef");

                                        if (paymentDetails != "")
                                            paymentDetails += ", ";
                                        paymentDetails += GetText(923, (int)TermGroup.Report, "Arkistointitunnus") + ": " + bankNumber;

                                        #region Amount

                                        XElement amtDtls =
                                            (from e in paymentElement.Elements()
                                             where e.Name.LocalName == "AmtDtls"
                                             select e).FirstOrDefault();

                                        XElement pmtAmountCreditMulti = strd.Descendants(nameSpace + "CdtNoteAmt").FirstOrDefault();
                                        XElement pmtAmountMulti = strd.Descendants(nameSpace + "RmtdAmt").FirstOrDefault();
                                        XElement pmtAmount = amtDtls.Descendants(nameSpace + "Amt").FirstOrDefault();
                 
                                        string pmtAmountStr = pmtAmountCreditMulti?.Value ?? pmtAmountMulti?.Value ?? pmtAmount?.Value;
                                        pmtAmountStr = pmtAmountStr.Replace(".", ",");
                                        if (pmtAmountCreditMulti != null)
                                        {
                                            pmtAmountStr = "-" + pmtAmountStr;
                                        }

                                        #endregion

                                        if (paymentDetails != "")
                                            paymentDetails += ", ";
                                        paymentDetails += GetText(924, (int)TermGroup.Report, "Suoritus") + ": " + pmtAmountStr;

                                        XElement pmtDate =
                                            (from e in paymentElement.Elements()
                                             where e.Name.LocalName == "RltdDts"
                                             select e).FirstOrDefault();

                                        //Datetime doesnt give date out of file
                                        string accDate = XmlUtil.GetChildElementValue(pmtDate, "AccptncDtTm");
                                        DateTime? paymentDate = null;
                                        accDate = accDate.Replace("T", " ");
                                        accDate = accDate.Replace("Z", "");
                                        //Time offset
                                        if (accDate.Contains("+"))
                                        {
                                            accDate = accDate.Remove(accDate.IndexOf("+"), (accDate.Length - accDate.IndexOf("+")));
                                            paymentDate = CalendarUtility.GetNullableDateTime(accDate, "yyyy-MM-dd HH:mm:ss");
                                        }
                                        else
                                            paymentDate = CalendarUtility.GetNullableDateTime(accDate, "yyyy-MM-dd HH:mm:ss");

                                        decimal amount = Convert.ToDecimal(pmtAmountStr.Trim());
                                        
                                        errorMessage = "";
                                        note = "";

                                        //If no referencenumber is given, we cant identify the payment
                                        if (string.IsNullOrEmpty(paymentOCR))
                                        {
                                            errorMessage = GetText(915, (int)TermGroup.Report, "Suoritukselta puuttuu viitenumero, ohitetaan");
                                        }

                                        Invoice paymentInvoice = GetPaymentFromInvoiceRF(entities, paymentOCR, actorCompanyId, ref log, amount);

                                        if (paymentInvoice == null && errorMessage == "")
                                        {
                                            string tmpPaymentOCR = "";
                                            if (paymentOCR.Contains("RF"))
                                                //If in Rf-Format then convert to "old-style"
                                                tmpPaymentOCR = paymentOCR.Substring(3, paymentOCR.Length - 3);
                                            else
                                                //If not in Rf-Format then convert
                                                tmpPaymentOCR = GetISO11649(paymentOCR);
                                            paymentInvoice = GetPaymentFromInvoiceRF(entities, tmpPaymentOCR, actorCompanyId, ref log, amount);
                                            //no invoice was found with same referencenumber
                                            if (paymentInvoice == null)
                                            {
                                                errorMessage = GetText(916, (int)TermGroup.Report, "Viitenumerolla ei löytynyt laskua, ohitetaan");
                                            }
                                        }

                                        CustomerInvoiceGridDTO item = null;

                                        if (paymentInvoice != null)
                                        {
                                            item = InvoiceManager.GetCustomerInvoiceForGrid(entities, paymentInvoice.InvoiceId, actorCompanyId);
                                        }

                                        if (item == null && errorMessage == "")
                                        {
                                            //No invoice found
                                            errorMessage = GetText(917, (int)TermGroup.Report, "Kohdistusta ei saatu tehtyä, ohitetaan");
                                        }
                                        else if (item != null && item.FullyPaid)
                                        {
                                            //Check if already payed
                                            errorMessage = GetText(918, (int)TermGroup.Report, "Lasku on jo maksettu, ohitetaan");
                                        }

                                        if (item != null)
                                        {
                                            totalPaymentAmount = 0;
                                            invoicePayments.TryGetValue(item.CustomerInvoiceId, out totalPaymentAmount);
                                            totalPaymentAmount += item.PaidAmount;

                                            if (amount > (item.TotalAmount - totalPaymentAmount))
                                            {
                                                //Overpayment
                                                errorMessage = string.Format(GetText(919, (int)TermGroup.Report, "Ylisuoritus {0}, ohitetaan"), (amount - (item.TotalAmount - totalPaymentAmount)).ToString());
                                            }
                                            else if (amount < (item.TotalAmount - totalPaymentAmount))
                                            {
                                                //Underpayment
                                                note = string.Format(GetText(920, (int)TermGroup.Report, "Saldoa jää {0}"), ((item.TotalAmount - totalPaymentAmount) - amount).ToString());
                                            }
                                        }

                                        SEPATransactionElement = new XElement("Payment",
                                            new XAttribute("Id", paymentXmlId),
                                            new XElement("CustomerNr", item != null ? item.ActorCustomerNr : string.Empty),
                                            new XElement("CustomerName", item != null ? item.ActorCustomerName : string.Empty),
                                            new XElement("PayerName", dbtorName),
                                            new XElement("PaymentNr", ""),
                                            new XElement("BookingDate", bookingDate != null ? bookingDate : CalendarUtility.DATETIME_DEFAULT),
                                            new XElement("PaymentDate", paymentDate != null ? paymentDate : CalendarUtility.DATETIME_DEFAULT),
                                            new XElement("BathcId", bankNumber),
                                            new XElement("InvoiceNr", item != null ? item.InvoiceNr : string.Empty),
                                            new XElement("OCR", paymentOCR),
                                            new XElement("PaymentAmount", amount),
                                            new XElement("Correction", ""),
                                            new XElement("ErrorMessage", errorMessage),
                                            new XElement("Note", note));

                                        paymentMethodElement.Add(SEPATransactionElement);

                                        paymentXmlId++;

                                        if (errorMessage != "")
                                            continue;

                                        //Got to use booking date / not payment date
                                        item.PayDate = bookingDate;
                                        item.PayAmount = amount;
                                        item.ActorCustomerName = dbtorName;

                                        itemsToTransfer.Add(item);

                                        if (invoicePayments.ContainsKey(item.CustomerInvoiceId))
                                            invoicePayments.Remove(item.CustomerInvoiceId);

                                        invoicePayments.Add(item.CustomerInvoiceId, totalPaymentAmount + amount);
                                    }
                                }
                            }

                        }

                        dataStorageXDocRoot.Add(paymentMethodElement);

                        #endregion

                        if (itemsToTransfer.Count > 0)
                        {
                            using (var transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                            {

                                var paymentRows = new List<PaymentRow>();

                                //Save payments for invoices check if zero count                                                                        
                                savePaymentResult = SavePaymentFromPaymentFile(entities, ConfigSettings.TRANSACTIONSCOPEOPTION_ATTACH, itemsToTransfer, DateTime.Now, paymentMethodFromFile != null ? paymentMethodFromFile.PaymentMethodId : paymentMethodId, accountYearId, actorCompanyId, false, fileName, out paymentRows);

                                //Add payments seqnr to xml
                                foreach (var paymentRow in paymentRows)
                                {
                                    XElement paymentSeqNrValue = dataStorageXDocRoot
                                        .Elements("PaymentMethod")
                                        .Elements("Payment").Where(i => i.Element("OCR").Value == paymentRow.Invoice.OCR && i.Element("PaymentNr").Value == "" && i.Element("PaymentAmount").Value == paymentRow.Amount.ToString().Replace(',', '.')).Select(i => i.Element("PaymentNr")).FirstOrDefault();

                                    if (paymentSeqNrValue == null)
                                    {
                                        string tmpOCR = "";
                                        if (paymentRow.Invoice.OCR.Contains("RF"))
                                            //If in Rf-Format then convert to "old-style"
                                            tmpOCR = paymentRow.Invoice.OCR.Substring(3, paymentRow.Invoice.OCR.Length - 3);
                                        else
                                            //If not in Rf-Format then convert
                                            tmpOCR = GetISO11649(paymentRow.Invoice.OCR);

                                        paymentSeqNrValue = dataStorageXDocRoot
                                        .Elements("PaymentMethod")
                                        .Elements("Payment").Where(i => i.Element("OCR").Value == tmpOCR && i.Element("PaymentNr").Value == "" && i.Element("PaymentAmount").Value == paymentRow.Amount.ToString().Replace(',', '.')).Select(i => i.Element("PaymentNr")).FirstOrDefault();
                                    }

                                    if (paymentSeqNrValue != null)
                                        paymentSeqNrValue.Value = paymentRow.SeqNr.ToString();
                                }

                                //Commit transaction
                                if (result.Success)
                                    transaction.Complete();

                                itemsToTransfer.Clear();
                            }
                        }
                    }

                    entities.SaveChanges();
                }

                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;

                    string errorMessage = savePaymentResult.Success ? "Virhe aineiston sisäänluvussa. " + paymentDetails : GetImportErrorMessage(savePaymentResult);

                    var paymentMethodElementForException = new XElement("PaymentMethod",
                           new XAttribute("Id", paymentMethodXmlId),
                           new XElement("PaymentMethodName", string.Empty),
                           new XElement("PaymentMethodPaymentNr", string.Empty),
                           new XElement("FileName", fileName));

                    SEPATransactionElement = new XElement("Payment",
                                        new XAttribute("Id", 1),
                                        new XElement("CustomerNr", string.Empty),
                                        new XElement("CustomerName", string.Empty),
                                        new XElement("PayerName", string.Empty),
                                        new XElement("PaymentNr", string.Empty),
                                        new XElement("BookingDate", CalendarUtility.DATETIME_DEFAULT),
                                        new XElement("PaymentDate", CalendarUtility.DATETIME_DEFAULT),
                                        new XElement("BathcId", string.Empty),
                                        new XElement("InvoiceNr", string.Empty),
                                        new XElement("OCR", string.Empty),
                                        new XElement("PaymentAmount", 0),
                                        new XElement("Correction", string.Empty),
                                        new XElement("ErrorMessage", errorMessage),
                                        new XElement("Note", string.Empty));

                    paymentMethodElementForException.Add(SEPATransactionElement);

                    dataStorageXDocRoot.Add(paymentMethodElementForException);
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

            return result;
        }

        public ActionResult ImportIO(string incFile, int actorCompanyId, bool templateIsSuggestion, int paymentMethodId, ref List<string> log, int userId, int batchId, int paymentImportId, ImportPaymentType importType)
        {
            var result = new ActionResult(true);
            
            var files = new List<SEPAFile>();

            using (var entities = new CompEntities())
            {
                try
                {
                    #region Settings

                    //PaymentMethods
                    List<PaymentMethod> paymentMethods = PaymentManager.GetPaymentMethods(entities, SoeOriginType.CustomerPayment, actorCompanyId, false);

                    #endregion

                    #region Parse

                    //Create Xdoc for reading incoming data
                    XDocument xdoc = XDocument.Parse(incFile);
                    var nameSpace = xdoc.Root.Name.Namespace;

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

                        XElement id =
                            (from e in acct.Elements()
                             where e.Name.LocalName == "Id"
                             select e).FirstOrDefault();

                        //Account that has received payments
                        string AccountNumber = XmlUtil.GetChildElementValue(id, "IBAN");

                        PaymentMethod paymentMethodFromFile = paymentMethods.FirstOrDefault(a => a.PaymentNr != null && a.PaymentNr.Contains(AccountNumber));

                        //only import for the chosen bank!
                        if (paymentMethodFromFile == null || paymentMethodFromFile.PaymentMethodId != paymentMethodId)
                        {
                            continue;
                        }

                        List<XElement> pGroupNtrys =
                            (from e in pGroupNctn.Elements()
                             where e.Name.LocalName == "Ntry"
                             select e).ToList();

                        foreach (XElement pGroupNtry in pGroupNtrys)
                        {
                            XElement bookDate =
                                (from e in pGroupNtry.Elements()
                                 where e.Name.LocalName == "BookgDt"
                                 select e).FirstOrDefault();

                            //Datetime doesnt give date out of file
                            string bkgDate = XmlUtil.GetChildElementValue(bookDate, "Dt");
                            bkgDate = bkgDate.Replace("T", " ");
                            bkgDate = bkgDate.Replace("Z", "");
                            DateTime? bookingDate = CalendarUtility.GetNullableDateTime(bkgDate, "yyyy-MM-dd");

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
                                    //We get only invoice ocr from Sepa file not invoicenumber
                                    XElement rmtInf =
                                        (from e in paymentElement.Elements()
                                         where e.Name.LocalName == "RmtInf"
                                         select e).FirstOrDefault();


                                    var strdMulti =
                                        (from e in rmtInf.Elements()
                                         where e.Name.LocalName == "Strd"
                                         select e).ToList();

                                    foreach (var strd in strdMulti)
                                    {

                                        XElement cdtrRefInf =
                                            (from e in strd.Elements()
                                             where e.Name.LocalName == "CdtrRefInf"
                                             select e).FirstOrDefault();

                                        string paymentOCR = XmlUtil.GetChildElementValue(cdtrRefInf, "Ref");
                                        paymentOCR = Utilities.RemoveLeadingZeros(paymentOCR);
                                        paymentOCR = paymentOCR.RemoveWhiteSpace();

                                        XElement rltdPties =
                                            (from e in paymentElement.Elements()
                                             where e.Name.LocalName == "RltdPties"
                                             select e).FirstOrDefault();

                                        XElement dbtName =
                                            (from e in rltdPties.Elements()
                                             where e.Name.LocalName == "Dbtr"
                                             select e).FirstOrDefault();

                                        string dbtorName = XmlUtil.GetChildElementValue(dbtName, "Nm");

                                        #region Amount

                                        XElement amtDtls =
                                            (from e in paymentElement.Elements()
                                             where e.Name.LocalName == "AmtDtls"
                                             select e).FirstOrDefault();

                                        XElement pmtAmountCreditMulti = strd.Descendants(nameSpace + "CdtNoteAmt").FirstOrDefault();
                                        XElement pmtAmountMulti = strd.Descendants(nameSpace + "RmtdAmt").FirstOrDefault();
                                        XElement pmtAmount = amtDtls.Descendants(nameSpace + "Amt").FirstOrDefault();

                                        string pmtAmountStr = pmtAmountCreditMulti?.Value ?? pmtAmountMulti?.Value ?? pmtAmount?.Value;
                                        if (pmtAmountCreditMulti != null)
                                        {
                                            pmtAmountStr = "-" + pmtAmountStr;
                                        }

                                        var currency = XmlUtil.GetAttributeStringValue(pmtAmount, "Ccy");

                                        #endregion

                                        decimal amount = Convert.ToDecimal(pmtAmountStr?.Trim(), CultureInfo.InvariantCulture);

                                        files.Add( new SEPAFile(bookingDate.GetValueOrDefault(), amount, paymentOCR, currency, dbtorName) );
                                    }
                                }
                            }

                        }

                        #endregion
                    }
                }

                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
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

            try
            {
                result = ConvertStreamToEntity(files, actorCompanyId, paymentMethodId, templateIsSuggestion, ref log, userId, batchId, paymentImportId, importType);

                if (!result.Success)
                    return result;
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result.Exception = ex;
            }

            return result;
        }

        private ActionResult ConvertStreamToEntity(List<SEPAFile> file, int actorCompanyId, int paymentMethodId, bool templateIsSuggestion, ref List<string> logText, int userId = 0, int batchId = 0, int paymentImportId = 0, ImportPaymentType importType = ImportPaymentType.None)
        {
            CustomerInvoice customerInvoice = null;
            DateTime? fileDate = null;

            var result = new ActionResult(true);
            var PaymentImportIOToAdd = new List<PaymentImportIO>();
            int status = 0;
            int state = 0;
            int type = 1;
            decimal totalInvoiceAmount = 0;
            DateTime? importDate = null;

            using (var entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    #region Prereq

                    //PaymentMethod
                    PaymentMethod paymentMethod = PaymentManager.GetPaymentMethod(entities, paymentMethodId, actorCompanyId, true);
                    if (paymentMethod == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "PaymentMethod");

                    //Company
                    Company company = CompanyManager.GetCompany(entities, actorCompanyId);
                    if (company == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                    if (!company.PaymentMethod.IsLoaded)
                        company.PaymentMethod.Load();

                    #endregion

                    foreach (var post in file)
                    {
                        #region Section

                        if (fileDate == null)
                            fileDate = post.PaidDate;

                        if (!importDate.HasValue || (fileDate.HasValue && fileDate.Value < importDate.Value))
                            importDate = fileDate;

                        #region ReferenceNumberPost1, ReferenceNumberPost2, PaymentPost, PaymentReductionPost

                        #region Invoice

                        string invoiceNr = post.Reference;
                        Invoice invoice = null;

                        string invoiceNrToTrim = Regex.Replace(invoiceNr, @"\s+", "");
                        string parsedInvoiceNr = Regex.Match(invoiceNrToTrim, @"\d+").Value;

                        IQueryable<Invoice> invoiceQuery = (from i in entities.Invoice.Include("Origin")
                                                                .Include("Currency")
                                                                .Include("Actor.Customer")
                                                            where i.State == (int)SoeEntityState.Active &&
                                                            i.Origin.Type == (int)SoeOriginType.CustomerInvoice &&
                                                            i.Origin.ActorCompanyId == actorCompanyId
                                                            select i);


                        if (!string.IsNullOrEmpty(invoiceNr))
                        {
                            //It should be possible to import a payment to an invoice having same OCR which is used in other invoice.
                            //Payment should be focused to the open invoice
                            var invoiceMatch = invoiceQuery.Where(i => i.OCR == invoiceNr).ToList();
                            if (invoiceMatch.Count > 1)
                            {
                                invoice = invoiceMatch.FirstOrDefault(i => !i.FullyPayed);
                            }

                            if (invoice == null && invoiceMatch.Count > 0)
                            {
                                invoice = invoiceMatch.First();
                            }

                            if (invoice == null)
                            {
                                invoice = invoiceQuery.FirstOrDefault(i => i.InvoiceNr == parsedInvoiceNr);
                            }

                            if (invoice == null)
                            {
                                var invoices = invoiceQuery
                                    .Where(i => i.OCR.EndsWith(invoiceNr)).ToList()
                                    .Where(i => i.OCR.TrimStart('0').Equals(invoiceNr)).ToList();

                                bool isMoreThanOne = invoices.Count > 1;
                                if (isMoreThanOne)
                                {
                                    // TUDO: FEATURE SHOW MESSAGE WITH INVOICE NUMBERS AND AMOUNTS

                                    continue;
                                }

                                invoice = invoices.FirstOrDefault();
                            }

                            if (invoice == null)
                            {
                                var invoicesByInvoiceNr = invoiceQuery.Where(i => i.InvoiceNr.Equals(parsedInvoiceNr)).ToList();
                                bool isMoreThanOne = invoicesByInvoiceNr.Count > 1;
                                if (isMoreThanOne)
                                {
                                    // TUDO: FEATURE SHOW MESSAGE WITH INVOICE NUMBERS AND AMOUNTS

                                    continue;
                                }

                                invoice = invoicesByInvoiceNr.FirstOrDefault();
                            }

                        }
                        if (invoice == null)
                        {
                            // invoice not found
                            status = (int)ImportPaymentIOStatus.Unknown;
                            state = (int)ImportPaymentIOState.Open;
                        }


                        //Reverify this condition
                        if (invoice != null)
                        {

                            if (invoice.InvoiceNr == invoiceNr || (!string.IsNullOrEmpty(invoice.OCR) && invoice.OCR == invoiceNr))
                            {
                                invoice = InvoiceManager.GetInvoice(entities, invoice.InvoiceId);

                                if (invoice == null)
                                {
                                    status = (int)ImportPaymentIOStatus.Unknown;
                                    state = (int)ImportPaymentIOState.Open;
                                }
                                else
                                {
                                    if (!invoice.PaymentRow.IsLoaded)
                                        invoice.PaymentRow.Load();

                                    if (invoice.PaymentRow.Any())
                                    {
                                        decimal sum = invoice.PaymentRow.Where(p => p.State == (int)SoeEntityState.Active).Select(p => p.AmountCurrency).Sum();
                                        if (invoice.TotalAmountCurrency < (sum + Utilities.GetAmount(post.Amount)))
                                        {
                                            status = (int)ImportPaymentIOStatus.Rest;
                                            //logText.Add(GetText(7197, "Faktura med nummer") + ": " + invoiceNr + " " + GetText(7198, "prickades ej av") + ". " + GetText(7199, "Beloppet på betalningar överstiger fakturans totalbelopp") + ".");
                                        }
                                    }
                                }
                            }
                        }

                        #endregion

                        #region PaymentImportIO
                        customerInvoice = null;
                        if (invoice != null)
                        {
                            status = (int)ImportPaymentIOStatus.Match;
                            state = (int)ImportPaymentIOState.Open;

                            customerInvoice = InvoiceManager.GetCustomerInvoice(invoice.InvoiceId, true, true);
                            type = invoice.TotalAmount >= 0 ? (int)TermGroup_BillingType.Debit : (int)TermGroup_BillingType.Credit;

                            bool isPartlyPaid = post.Amount < invoice.TotalAmount;
                            if (isPartlyPaid)
                            {
                                status = (int)ImportPaymentIOStatus.PartlyPaid;
                            }

                            bool isRest = post.Amount > invoice.TotalAmount;
                            if (isRest)
                            {
                                status = (int)ImportPaymentIOStatus.Rest;
                            }

                            bool isFullyPayed = invoice.FullyPayed;
                            if (isFullyPayed)
                            {
                                if (importType == ImportPaymentType.CustomerPayment)
                                    status = (int)ImportPaymentIOStatus.Unknown;
                                else
                                    status = (int)ImportPaymentIOStatus.Paid;
                            }
                        }

                        var paymentImportIO = new PaymentImportIO
                        {
                            ActorCompanyId = actorCompanyId,
                            BatchNr = batchId,
                            Type = type,
                            CustomerId = customerInvoice != null ? customerInvoice.Actor.Customer.ActorCustomerId : 0,
                            Customer = string.IsNullOrEmpty(post.CustomerName) ? StringUtility.Left(customerInvoice?.ActorName, 50) : StringUtility.Left(post.CustomerName, 50),
                            InvoiceId = invoice != null ? invoice.InvoiceId : 0,
                            InvoiceNr = invoice != null ? invoice.InvoiceNr : invoiceNr,
                            InvoiceAmount = invoice != null ? invoice.TotalAmount - invoice.PaidAmount : 0,
                            RestAmount = invoice != null ? invoice.TotalAmount - invoice.PaidAmount - post.Amount : 0,
                            PaidAmount = post.Amount,
                            Currency =  post.CurrencyCode ?? "SEK",
                            InvoiceDate = invoice?.DueDate,
                            PaidDate = post.PaidDate,
                            MatchCodeId = 0,
                            Status = status,
                            State = state,
                            ImportType = (int)importType,
                            OCR = invoiceNr,
                        };

                        totalInvoiceAmount = totalInvoiceAmount + Utilities.GetAmount(post.Amount);

                        // Check for duplicates
                        if (PaymentImportIOToAdd.Any(p => p.CustomerId == paymentImportIO.CustomerId && p.InvoiceId == paymentImportIO.InvoiceId && p.PaidAmount == paymentImportIO.PaidAmount && p.PaidDate == paymentImportIO.PaidDate))
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

                        PaymentImportIOToAdd.Add(paymentImportIO);

                        logText.Add(paymentImportIO.Customer);

                        #endregion

                        #endregion

                        #endregion
                    }

                    int numberOfPayments = 1;

                    foreach (var paymentIO in PaymentImportIOToAdd)
                    {
                        using (var transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                        {
                            if (result.Success && !templateIsSuggestion)
                            {
                                entities.PaymentImportIO.AddObject(paymentIO);

                                result = SaveEntityItem(entities, paymentIO, transaction);

                                if (result.Success)
                                {
                                    PaymentImport paymentImport = PaymentManager.GetPaymentImport(entities, paymentImportId, actorCompanyId);

                                    paymentImport.TotalAmount = paymentImport.TotalAmount + paymentIO.PaidAmount.Value;
                                    paymentImport.NumberOfPayments = numberOfPayments++;

                                    if(importDate.HasValue)
                                        paymentImport.ImportDate = importDate.Value;

                                    result = PaymentManager.UpdatePaymentImportHead(entities, paymentImport, paymentImportId, actorCompanyId);
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
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                }
                finally
                {
                    if (!result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            return result;
        }

        private string GetImportErrorMessage(ActionResult savePaymentResult)
        {
            string errorMessage = "";
            switch (savePaymentResult.ErrorNumber)
            {
                case (int)ActionResultSave.EntityNotFound:
                    if (savePaymentResult.ErrorMessage == "Company")
                        errorMessage = "Yrityksen tietojen saanti epäonnistui.";
                    if (savePaymentResult.ErrorMessage == "VoucherSeries")
                        errorMessage = "Suoritusten tositelajia ei ole määritelty asetuksissa tai tositelajia ei ole liitetty tilikauteen";
                    if (savePaymentResult.ErrorMessage == "PaymentMethod")
                        errorMessage = "Maksutavan tietojen saanti epäonnistui.";
                    if (savePaymentResult.ErrorMessage == "AccountStd")
                        errorMessage = "Maksutavan tiedoista puuttuu kirjanpitotili.";
                    if (savePaymentResult.ErrorMessage == "CustomerInvoice")
                        errorMessage = "Laskun tietoja ei saatu haettua.";
                    break;
                case (int)ActionResultSave.AccountYearNotFound:
                    errorMessage = "Tilikautta ei löydy";
                    break;
                case (int)ActionResultSave.AccountYearNotOpen:
                    errorMessage = "Tilikautta ei ole perustettu tai se ei ole avoinna";
                    break;
                case (int)ActionResultSave.AccountPeriodNotFound:
                    errorMessage = "Kautta ei löydy";
                    break;
                case (int)ActionResultSave.AccountPeriodNotOpen:
                    errorMessage = "Kausi ei ole avoinna";
                    break;
                case (int)ActionResultSave.PaymentInvoiceAssetRowNotFound:
                    errorMessage = "Laskulta " + savePaymentResult.InfoMessage + " puuttuu myyntisaamistili";
                    break;
                case (int)ActionResultSave.InvalidStateTransition:
                    if (savePaymentResult.ErrorMessage == "CustomerInvoice")
                        errorMessage = "Laskun " + savePaymentResult.InfoMessage + " tila on väärä suorituksen tallentamista varten";
                    if (savePaymentResult.ErrorMessage == "VoucherHead")
                        errorMessage = "Suorituksesta on jo luotu tosite";
                    if (savePaymentResult.ErrorMessage == "PaymentRow")
                        errorMessage = "Suorituksen tila on väärä tositteen luomista varten";
                    break;
            }
            return errorMessage;
        }

        #endregion

        #region PAIN002

        public bool IsPain002_V2(XDocument xdoc)
        {
            var nameSpace = xdoc.Root.Name.Namespace;
            if (nameSpace != null && nameSpace.NamespaceName == "urn:iso:std:iso:20022:tech:xsd:pain.002.001.02")
                return true;
            else if (xdoc.Root.Element("pain.002.001.02") != null) //Nordea Finland does this.
                return true;
            else
                return false;
        }

        public ActionResult ImportPain002(XDocument xdoc)
        {
            var messageStatus = new SEPAMessageStatus();

            if (!IsPain002_V2(xdoc))
                return new ActionResult(8176, "Kan inte läsa från XML fil");

            XElement documentRoot =
                (from e in xdoc.Elements()
                 where e.Name.LocalName == "Document"
                 select e).FirstOrDefault();

            XElement report =
                    (from e in documentRoot.Elements()
                     where e.Name.LocalName == "pain.002.001.02"
                     select e).FirstOrDefault();

            XElement originalGroupInfo =
                    (from e in report.Elements()
                     where e.Name.LocalName == "OrgnlGrpInfAndSts"
                     select e).FirstOrDefault();

            messageStatus.OrgMessageId = XmlUtil.GetChildElementValue(originalGroupInfo, "OrgnlMsgId");
            messageStatus.MessageStatus = XmlUtil.GetChildElementValue(originalGroupInfo, "GrpSts");

            messageStatus.MessageText = this.GetImportGroupErrorMessage(originalGroupInfo, messageStatus.MessageStatus);

            if (string.IsNullOrEmpty(messageStatus.OrgMessageId))
            {
                return new ActionResult("Failed finding original message id in file");
            }

            List<XElement> txInfAndStslist =
                    (from e in report.Elements()
                     where e.Name.LocalName == "TxInfAndSts"
                     select e).ToList();

            foreach (var txInfAndSts in txInfAndStslist)
            {
                var orgPaymentId = XmlUtil.GetChildElementValue(txInfAndSts, "OrgnlPmtInfId");

                var orgEndToEndId = XmlUtil.GetChildElementValue(txInfAndSts, "OrgnlEndToEndId");
                var transStatus = XmlUtil.GetChildElementValue(txInfAndSts, "TxSts");

                XElement stsRsnInf =
                                (from e in txInfAndSts.Elements()
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
                    statusObject.ErrorMessage = XmlUtil.GetChildElementValue(stsRsnInf, "AddtlStsRsnInf");
                    statusObject.ErrorCode = XmlUtil.GetDescendantElementValue(stsRsnInf, "StsRsn", "Cd");
                }

                messageStatus.PaymentStatuses.Add(statusObject);
            }

            return ConvertPain002StreamToEntity(messageStatus);
        }

        private string GetImportGroupErrorMessage(XElement originalGroupInfo, string messageStatus)
        {
            string msgText = "";
            if (messageStatus == ISO_Payment_TransactionStatus.ACTC.ToString())
            {
                msgText = GetText(12538, "Banken har mottagit betalningsmeddelandet");
            }
            else if (messageStatus == ISO_Payment_TransactionStatus.ACCP.ToString() || messageStatus == ISO_Payment_TransactionStatus.ACSP.ToString())
            {
                msgText = GetText(12539, "Alla betalningar har accepterats");
            }
            else if (messageStatus == ISO_Payment_TransactionStatus.ACSC.ToString())
            {
                msgText = GetText(12540, "Alla betalningar är genomförda");
            }
            else if (messageStatus == ISO_Payment_TransactionStatus.PDNG.ToString())
            {
                msgText = GetText(12541, "Betalningsmeddelandet väntar på avräkning");
            }
            else if (messageStatus == ISO_Payment_TransactionStatus.RJCT.ToString())
            {
                var stsRsnInf = XmlUtil.GetChildElement(originalGroupInfo, "StsRsnInf");
                if (stsRsnInf != null)
                {
                    string cd = XmlUtil.GetDescendantElementValue(stsRsnInf, "StsRsn", "Cd");
                    var additionalInfoElements = XmlUtil.GetChildElements(stsRsnInf, "AddtlStsRsnInf");

                    msgText = string.Format("{0}: {1}", cd, string.Join("", additionalInfoElements.Select(e => e.Value ?? "").Where(s => !string.IsNullOrWhiteSpace(s))));
                }
            }
            else if (messageStatus == ISO_Payment_TransactionStatus.PART.ToString())
            {
                msgText = GetText(12542, "Vissa poster i betalningsmeddelandet har avvisats");
            }

            return msgText;
        }

        private ActionResult ConvertPain002StreamToEntity(SEPAMessageStatus sepaStatus)
        {
            using (var entities = new CompEntities())
            {
                var export = entities.PaymentExport.Include("Payment.PaymentRow").Where(x => x.MsgId == sepaStatus.OrgMessageId && x.State == (int)SoeEntityState.Active).FirstOrDefault();
                var payment = export?.Payment.FirstOrDefault();
                if (payment != null)
                {
                    foreach(var paymentRow in payment.PaymentRow)
                    {
                        var rowStatus = sepaStatus.PaymentStatuses.FirstOrDefault(x => Convert.ToInt32(x.OrgEndToEndId) == paymentRow.PaymentRowId);

                        //if whole message rejected and not individual errors uee first
                        if (rowStatus == null && sepaStatus.MessageStatus == ISO_Payment_TransactionStatus.RJCT.ToString())
                        {
                            rowStatus = sepaStatus.PaymentStatuses.FirstOrDefault(x => !string.IsNullOrEmpty(x.ErrorMessage));
                        }

                        if (rowStatus != null)
                        {
                            if (rowStatus.Status == ISO_Payment_TransactionStatus.ACCP.ToString() || rowStatus.Status == ISO_Payment_TransactionStatus.ACSP.ToString())
                            {
                                paymentRow.StatusMsg = GetText(12543, "Betalningen är accepterad");
                            }
                            else if (rowStatus.Status == ISO_Payment_TransactionStatus.ACWC.ToString())
                            {
                                paymentRow.StatusMsg = GetText(12544, "Betalningen är godkänd med ändring");
                            }
                            else if (rowStatus.Status == ISO_Payment_TransactionStatus.ACSC.ToString())
                            {
                                paymentRow.Status = (int)SoePaymentStatus.Pending;
                                paymentRow.StatusMsg = GetText(12545, "Betalt");
                            }
                            else if (rowStatus.Status == ISO_Payment_TransactionStatus.PDNG.ToString())
                            {
                                paymentRow.StatusMsg = GetText(12541, "Betalningsmeddelandet väntar på avräkning");
							}
                            else if (rowStatus.Status != ISO_Payment_TransactionStatus.ACCP.ToString()) //including RJCT
                            {
                                paymentRow.Status = (int)SoePaymentStatus.Error;
                                paymentRow.StatusMsg = $"{rowStatus.ErrorCode}: {rowStatus.ErrorMessage}";
                            }
                        }
                        else if (sepaStatus.MessageStatus == ISO_Payment_TransactionStatus.RJCT.ToString())
                        {
                            paymentRow.Status = (int)SoePaymentStatus.Error;
                            paymentRow.StatusMsg = sepaStatus.MessageText;
                        }
                    }

                    if (sepaStatus.MessageStatus == ISO_Payment_TransactionStatus.ACTC.ToString())
                    {
                        export.TransferStatus = (int)TermGroup_PaymentTransferStatus.Transfered;
                        export.TransferMsg = sepaStatus.MessageText;
                    }
                    else if (sepaStatus.MessageStatus == ISO_Payment_TransactionStatus.ACCP.ToString() || sepaStatus.MessageStatus == ISO_Payment_TransactionStatus.ACSP.ToString())
                    {
                        export.TransferStatus = (int)TermGroup_PaymentTransferStatus.Completed;
                        export.TransferMsg = sepaStatus.MessageText;
                    }
                    else if (sepaStatus.MessageStatus == ISO_Payment_TransactionStatus.PDNG.ToString())
                    {
                        export.TransferStatus = (int)TermGroup_PaymentTransferStatus.Pending;
                        export.TransferMsg = GetText(12541 , "Betalningsmeddelandet väntar på avräkning");
                    }
                    else if (sepaStatus.MessageStatus == ISO_Payment_TransactionStatus.PART.ToString())
                    {
                        export.TransferStatus = (int)TermGroup_PaymentTransferStatus.PartlyRejected;
                        export.TransferMsg = GetText(12542, "Vissa poster i betalningsmeddelandet har avvisats");
                    }
                    else if (sepaStatus.MessageStatus == ISO_Payment_TransactionStatus.RJCT.ToString())
                    {
                        export.TransferStatus = (int)TermGroup_PaymentTransferStatus.BankError;
                        export.TransferMsg = sepaStatus.MessageText;
                    }
                }

                return SaveChanges(entities);
            }
        }

        #endregion

        #endregion

        #region Help-methods

        private Invoice GetPaymentFromInvoiceRF(CompEntities entities, string RF, int actorCompanyId, ref List<string> log, decimal amount)
        {
            Invoice invoiceOrig = (from i in entities.Invoice
                                           .Include("Origin")
                                   where i.OCR == RF &&
                                   i.DueDate != null && !i.FullyPayed &&
                                   i.Origin.ActorCompanyId == actorCompanyId &&
                                   i.State == (int)SoeEntityState.Active &&
                                   i.Type == 2
                                   select i).FirstOrDefault();

            if (invoiceOrig == null)
            {
                return null;
            }

            return GetPaymentFromInvoice(invoiceOrig, RF, ref log, amount);
        }

        private Invoice GetPaymentFromInvoice(Invoice invoice, string RF, ref List<string> log, decimal amount)
        {
            if (invoice == null)
            {
                return null;
            }

            if (!invoice.PaymentRow.IsLoaded)
                invoice.PaymentRow.Load();

            if (invoice.PaymentRow.Count != 0 && invoice.FullyPayed)
            {
                //Invoice has allready payment, cant be remade
                log.Add("Maksu on jo luotu. Viite: " + RF + " summa: " + amount.ToString());
                return null;
            }

            //lets get invoice to be made payment of

            return invoice;
        }

        public byte[] CreateExportFileInMemory(CompEntities entities, List<SysCountry> sysCountries, List<SysCurrency> sysCurrencies, IEnumerable<PaymentRow> paymentRows, int actorCompanyId, PaymentMethod paymentMethod, string msgId)
        {
            bool aggregatePayments = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierAggregatePaymentsInSEPAExportFile, 0, actorCompanyId, 0);

            SEPAModel sepaModel = new SEPAModel(entities, CompanyManager, ContactManager, PaymentManager, sysCountries, sysCurrencies, actorCompanyId, paymentRows, paymentMethod, aggregatePayments, msgId);

            //Document
            //XDocument sepaXml = new XDocument(new XDeclaration("1.0", Constants.ENCODING.ToString(), "true"));
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

        private static void SaveExistingFileAsOld(string path)
        {
            var old = path + "_old_" + DateTime.Now.ToString("yyyyMMddHHmmss") + Constants.SOE_SERVER_FILE_PAYMENT_SUFFIX;
            if (!File.Exists(old))
            {
                File.Copy(path, old);
            }
        }

        private string GetISO11649(string line)
        {
            /// <summary>
            /// We need to calculate international bank reference (RF) number. For backward compatibility, a Finnish
            /// Reference is first calculated and used as a base for RF reference. As in Swedes reference is not used, it 
            /// can be adapted as Finnish version. 
            /// Requirements for RF - reference makes it mandatory to calculate new checksum which is moved
            /// in front of reference. 
            /// 
            /// After 2013 switchover period alphabetic uppercase characters may be used in RF- reference from A-Z
            /// These are replaced by numbers in string starting A=10, B=11 etc. 
            /// </summary>
            /// <param name="line">Maximum length of refrence line is 20 characters (19+1)</param>
            /// <returns>RF reference</returns>
            ///////////////////
            // adding "RF00" as last characters to calculate checksum
            ///////////////////
            string origline = Utilities.RemoveLeadingZeros(line);
            line = line + "RF00";
            string newline = "";
            string Cchecksum = "";
            int Checksum = 0;
            ///////////////////
            // recode alphabets, we change characters to numeric values A=10, B=11 etc....
            ///////////////////
            foreach (char cc in line)
            {
                int numcharval = (Convert.ToInt32(cc) - 55);  // Value for character. A = 10 , B = 11 etc...
                // We have a value.
                if (numcharval > 9 && numcharval < 36)
                {
                    newline = newline + Convert.ToString(numcharval);
                }
                else
                {
                    newline = newline + cc;
                }
            }
            /////////////////////////
            // newline has all the details to calculate modulo, we need to add prezeroes so we can split 
            // calculation over to 3 parts as 32 - bit systems maybe can't handle integers so big...
            /////////////////////////
            while (newline.Length < 22)
            {
                newline = '0' + newline;
            }

            Cchecksum = newline.Substring(1, 7);
            Checksum = Convert.ToInt32(Cchecksum) % 97;

            Cchecksum = Convert.ToString(Checksum) + newline.Substring(8, 7);
            Checksum = Convert.ToInt32(Cchecksum) % 97;

            Cchecksum = Convert.ToString(Checksum) + newline.Substring(15, 7);
            Checksum = Convert.ToInt32(Cchecksum) % 97;

            Checksum = 98 - Checksum;

            if (Checksum < 10)
            {
                return "RF0" + Convert.ToString(Checksum) + origline;
            }
            else
            {
                return "RF" + Convert.ToString(Checksum) + origline;
            }

        }

        public ActionResult SavePaymentFromPaymentFile(CompEntities entities, TransactionScopeOption transactionScopeOption, List<CustomerInvoiceGridDTO> items, DateTime? bulkPayDate, int paymentMethodId, int accountYearId, int actorCompanyId, bool foreign, string fileName, out List<PaymentRow> paymentRows)
        {
            paymentRows = new List<PaymentRow>();

            if (items == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "ChangeStatusGridView");

            // Default result is successful
            ActionResult result = new ActionResult(true);

            #region Prereq

            if (bulkPayDate.HasValue)
            {
                foreach (var item in items)
                {
                    if (!item.PayDate.HasValue)
                        item.PayDate = bulkPayDate.Value;
                }
            }

            #endregion

            Payment payment = null;
            Dictionary<int, int> voucherHeadsDict = null;

            try
            {
                if (entities.Connection.State != ConnectionState.Open)
                    entities.Connection.Open();

                // Possible to include this method in a running Transaction
                using (TransactionScope transaction = new TransactionScope(transactionScopeOption, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                {
                    #region Settings

                    //VoucherSeries
                    int customerInvoicePaymentSeriesTypeId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.CustomerPaymentVoucherSeriesType, 0, actorCompanyId, 0);

                    #endregion

                    #region Prereq

                    //Get Company
                    Company company = CompanyManager.GetCompany(entities, actorCompanyId);
                    if (company == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                    //Validate AccountYear
                    AccountYear accountYear = AccountManager.GetAccountYear(entities, accountYearId);
                    result = AccountManager.ValidateAccountYear(accountYear);
                    if (!result.Success)
                        return result;

                    //Get default VoucherSerie for Payment for current AccountYear
                    VoucherSeries voucherSerie = VoucherManager.GetVoucherSerieByType(entities, customerInvoicePaymentSeriesTypeId, accountYearId);
                    if (voucherSerie == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "VoucherSeries");

                    //Get PaymentMethod
                    PaymentMethod paymentMethod = PaymentManager.GetPaymentMethod(entities, paymentMethodId, actorCompanyId, true);
                    if (paymentMethod == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "PaymentMethod");
                    if (paymentMethod.AccountStd == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountStd");

                    #endregion


                    foreach (var item in items)
                    {
                        #region Prereq

                        //If insecure then skip to the next item
                        if (item.InsecureDebt && item.OriginType == (int)SoeOriginType.CustomerInvoice)
                            continue;

                        #endregion                       

                        #region CustomerInvoice

                        //Get original and loaded CustomerInvoice
                        CustomerInvoice customerInvoice = InvoiceManager.GetCustomerInvoice(entities, item.CustomerInvoiceId, true, true, true, false, true, false, false, true, true, false, false, false);
                        if (customerInvoice == null || !customerInvoice.ActorId.HasValue)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "CustomerInvoice");

                        //Can only get status Payment from Origin or Voucher
                        if (customerInvoice.Origin.Status != (int)SoeOriginStatus.Origin && customerInvoice.Origin.Status != (int)SoeOriginStatus.Voucher)
                        {
                            result.Success = false;
                            result.ErrorNumber = (int)ActionResultSave.InvalidStateTransition;
                            result.ErrorMessage = "CustomerInvoice";
                            result.InfoMessage = customerInvoice.InvoiceNr;
                            return result;
                        }

                        #endregion

                        #region Origin payment

                        //Create payment Origin for the CustomerPayment
                        Origin paymentOrigin = new Origin()
                        {
                            Type = (int)SoeOriginType.CustomerPayment,
                            Status = (int)SoeOriginStatus.Payment,
                            Description = item.ActorCustomerName,
                            //Set references
                            Company = company,
                            VoucherSeries = voucherSerie,
                        };
                        SetCreatedProperties(paymentOrigin);

                        #endregion

                        #region Payment

                        //Create Payment
                        payment = new Payment()
                        {
                            //Set references
                            Origin = paymentOrigin,
                            PaymentMethod = paymentMethod,
                            PaymentExport = null,
                        };

                        #endregion

                        #region PaymentRow

                        //Create PaymentRow
                        PaymentRow paymentRow = new PaymentRow()
                        {
                            Status = (int)SoePaymentStatus.Verified,
                            SysPaymentTypeId = 4, //BIC
                            PaymentNr = paymentMethod.PaymentNr ?? string.Empty,
                            PayDate = item.PayDate.Value,
                            CurrencyRate = customerInvoice.CurrencyRate,
                            CurrencyDate = customerInvoice.CurrencyDate,

                            //Set references
                            Invoice = customerInvoice as Invoice,
                            Payment = payment,
                        };

                        SetCreatedProperties(paymentRow);

                        //SeqNr
                        paymentRow.SeqNr = SequenceNumberManager.GetNextSequenceNumber(entities, actorCompanyId, Enum.GetName(typeof(SoeOriginType), paymentOrigin.Type), 1, true);

                        //Set amount on payment depending on foreign or not
                        if (foreign)
                        {
                            //Set currency amounts
                            paymentRow.AmountCurrency = item.PayAmountCurrency;
                            paymentRow.BankFeeCurrency = 0;
                            paymentRow.AmountDiffCurrency = 0;
                            CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, paymentRow, true);
                        }
                        else
                        {
                            //Set currency amounts
                            paymentRow.Amount = item.PayAmount;
                            paymentRow.BankFee = 0;
                            paymentRow.AmountDiff = item.PayAmount * -1;
                            CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, paymentRow);
                        }

                        //PaidAmount
                        customerInvoice.PaidAmount += paymentRow.Amount;
                        customerInvoice.PaidAmountCurrency += paymentRow.AmountCurrency;
                        customerInvoice.RemainingAmount = customerInvoice.TotalAmount - customerInvoice.PaidAmount;

                        //FullyPayed
                        if (customerInvoice.RemainingAmount != 0)
                            customerInvoice.FullyPayed = false; // allow overpayment
                        else
                            InvoiceManager.SetInvoiceFullyPayed(entities, customerInvoice, customerInvoice.IsTotalAmountPayed);

                        //Accounting rows
                        result = PaymentManager.AddPaymentAccountRowsFromCustomerInvoice(entities, paymentRow, paymentMethod, customerInvoice, actorCompanyId);
                        if (!result.Success)
                        {
                            if (result.ErrorNumber == (int)ActionResultSave.PaymentInvoiceAssetRowNotFound)
                                result.InfoMessage = customerInvoice.InvoiceNr;
                            return result;
                        }

                        //PaymentImport
                        var paymentImport = new PaymentImport
                        {
                            ImportDate = DateTime.Now,
                            Filename = fileName,
                        };
                        SetCreatedProperties(paymentImport);
                        paymentRow.PaymentImport = paymentImport;

                        paymentRows.Add(paymentRow);

                        #endregion
                    }

                    #region Voucher

                    // Check if Voucher should also be saved at once
                    if (result.Success)
                    {
                        result = PaymentManager.TryTransferPaymentRowsToVoucher(entities, ConfigSettings.TRANSACTIONSCOPEOPTION_ATTACH, paymentRows, SoeOriginType.CustomerPayment, foreign, accountYearId, actorCompanyId);
                        if (result.Success)
                            voucherHeadsDict = NumberUtility.MergeDictictionary(voucherHeadsDict, result.IdDict);
                    }

                    #endregion

                    if (result.Success)
                        result = SaveChanges(entities, transaction);

                    //Commit transaction
                    if (result.Success)
                        transaction.Complete();
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
                    //Set success properties
                    if (voucherHeadsDict != null)
                    {
                        result.IdDict = voucherHeadsDict;
                        //result.IntegerValue = voucherHeadsDict.Count;
                    }
                }
                else
                    base.LogTransactionFailed(this.ToString(), this.log);

                if (transactionScopeOption != ConfigSettings.TRANSACTIONSCOPEOPTION_ATTACH)
                    entities.Connection.Close();
            }

            return result;
        }

        public ActionResult UpdateDataStorage(int actorCompanyId, string xmlString)
        {
            var result = new ActionResult(true);

            using (var entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    DataStorage dataStorage = GeneralManager.GetDataStorages(entities, actorCompanyId, SoeDataStorageRecordType.SEPAPaymentImport).OrderByDescending(i => i.DataStorageId).FirstOrDefault();

                    if (dataStorage == null)
                    {
                        GeneralManager.CreateDataStorage(entities, SoeDataStorageRecordType.SEPAPaymentImport, xmlString, null, null, null, actorCompanyId);
                    }
                    else
                    {
                        dataStorage.XML = xmlString;
                        SetModifiedProperties(dataStorage);
                    }

                    entities.SaveChanges();
                }

                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
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

            return result;
        }

        #endregion
    }
}
