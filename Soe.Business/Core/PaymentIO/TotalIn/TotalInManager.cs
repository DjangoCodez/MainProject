using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Transactions;

namespace SoftOne.Soe.Business.Core.PaymentIO.TotalIn
{
    public class TotalInManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public TotalInManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Import

        public ActionResult Import(StreamReader sr, int actorCompanyId, bool templateIsSuggestion, int paymentMethodId, ref List<string> log, int userId = 0, int batchId = 0, int paymentImportId = 0, ImportPaymentType importType = ImportPaymentType.None)
        {
            var result = new ActionResult(true)
            {
                ErrorMessage = GetText(8076, "Filimport genomförd")
            };

            sr.DiscardBufferedData();
            sr.BaseStream.Position = 0;
            List<TotalInFile> totalInFile = new List<TotalInFile>();
            try
            {

                DateTime paidDate = DateTime.Now;
                decimal amount = 0;
                string seqnr = string.Empty;
                string currencyCode = string.Empty;
                string customerName = string.Empty;
                bool firstRecord = true;
                bool isCredit = false;

                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    if (string.IsNullOrEmpty(line))
                        continue;

                    line = Utilities.AddPadding(line, Utilities.BGC_LINE_MAX_LENGTH);
                    string postCode = line.Left(2);
                    if (string.IsNullOrEmpty(postCode))
                        continue;
                    if (!int.TryParse(postCode, out int code))
                        continue;

                    switch (code)
                    {
                        case (int)TotalinTransactionCodes.SectionStart: //only necessary internaly for validation in import
                            paidDate = Utilities.GetDate(line.Substring(41, 8));
                            currencyCode = line.Substring(38, 3);
                            break;
                        case (int)TotalinTransactionCodes.PaymentPost:
                        case (int)TotalinTransactionCodes.PaymentReductionPost:

                            if (!firstRecord)
                            {
                                totalInFile.Add(new TotalInFile(paidDate, amount, seqnr, currencyCode, customerName, isCredit));
                                customerName = string.Empty;
                            }
                            firstRecord = false;

                            var idententifier = line.Substring(2, 35).Trim();
                            seqnr = (!string.IsNullOrEmpty(idententifier) && !idententifier.All(c => c == '0')) ? idententifier : "";
                            amount = Utilities.GetAmount(line.Substring(37, 15));
                            isCredit = code == (int)TotalinTransactionCodes.PaymentReductionPost;
                            break;
                        case (int)TotalinTransactionCodes.ReferenceNumberPost:
                        case (int)TotalinTransactionCodes.InformationPost:
                            var idententifier2 = line.Substring(2, 35).Trim();
                            if (!string.IsNullOrEmpty(idententifier2) && !idententifier2.All(c => c == '0'))
                            {
                                seqnr += string.IsNullOrEmpty(seqnr) ? idententifier2 : "," + idententifier2;
                            }
                            break;
                        case (int)TotalinTransactionCodes.NamePost:
                        case (int)TotalinTransactionCodes.OtherNamePost:
                            customerName = line.Substring(2, 35).Trim();
                            break;

                        case (int)TotalinTransactionCodes.SectionEnd:
                            if (!string.IsNullOrEmpty(seqnr))
                            {
                                totalInFile.Add(new TotalInFile(paidDate, amount, seqnr, currencyCode, customerName, isCredit));
                                seqnr = string.Empty;
                            }
                            break;

                    }
                }
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result.Exception = ex;
                return result;
            }

            try
            {
                if (userId == 0)
                    result = new ActionResult(true);
                else
                    result = ConvertStreamToEntity(totalInFile, actorCompanyId, paymentMethodId, templateIsSuggestion, ref log, userId, batchId, paymentImportId, importType);

                if (!result.Success)
                    return result;
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result.Exception = ex;
            }

            //TODO: somewhere the message is reset, locate correct and remove this check
            if (result.Success && string.IsNullOrEmpty(result.ErrorMessage))
                result.ErrorMessage = GetText(8076, "Filimport genomförd");

            return result;
        }

        #endregion

        #region Help methods

        private ActionResult ConvertStreamToEntity(List<TotalInFile> file, int actorCompanyId, int paymentMethodId,bool templateIsSuggestion, ref List<string> logText, int userId = 0, int batchId = 0, int paymentImportId = 0, ImportPaymentType importType = ImportPaymentType.None)
        {
            CustomerInvoice customerInvoice = null;
            DateTime? fileDate = null;

            var result = new ActionResult(true);
            var PaymentImportIOToAdd = new List<PaymentImportIO>();
            int status = 0;
            int state = 0;
            int type = 1;
            decimal totalInvoiceAmount = 0;

            using (var entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    #region Settings

                    //VoucherSeries
                    int customerInvoicePaymentSeriesTypeId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.CustomerPaymentVoucherSeriesType, 0, actorCompanyId, 0);

                    //AccountYear
                    int accountYearId;
                    if (userId == 0)
                        accountYearId = SettingManager.GetIntSetting(entities, SettingMainType.UserAndCompany, (int)UserSettingType.AccountingAccountYear, base.UserId, actorCompanyId, 0);
                    else
                        accountYearId = SettingManager.GetIntSetting(entities, SettingMainType.UserAndCompany, (int)UserSettingType.AccountingAccountYear, userId, actorCompanyId, 0);

                    #endregion

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

                    //Get default VoucherSerie for Payment for current AccountYear
                    VoucherSeries voucherSerie = VoucherManager.GetVoucherSerieByType(entities, customerInvoicePaymentSeriesTypeId, accountYearId);
                    if (voucherSerie == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "VoucherSeries");

                    #endregion

                    foreach (TotalInFile post in file)
                    {
                        if (fileDate == null)
                            fileDate = post.PaidDate;

                        #region InvoiceNr

                        string invoiceNr = post.Reference;

                        #endregion

                        #region Invoice

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
                            invoice = invoiceQuery.FirstOrDefault(i => i.OCR == invoiceNr) ?? invoiceQuery.FirstOrDefault(i => i.InvoiceNr == parsedInvoiceNr);
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
                        if (invoice != null && (invoice.InvoiceNr == invoiceNr || (!string.IsNullOrEmpty(invoice.OCR) && invoice.OCR == invoiceNr)))
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
                                        status = (int)ImportPaymentIOStatus.Rest;
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

                            bool isPartlyPaid = Utilities.GetAmount(post.Amount) < invoice.TotalAmount;
                            if (isPartlyPaid)
                            {
                                status = (int)ImportPaymentIOStatus.PartlyPaid;
                            }

                            bool isRest = Utilities.GetAmount(post.Amount) > invoice.TotalAmount;
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

                        PaymentImportIO paymentImportIO = new PaymentImportIO
                        {
                            ActorCompanyId = actorCompanyId,
                            BatchNr = batchId,
                            Type = type,
                            CustomerId = customerInvoice != null ? customerInvoice.Actor.Customer.ActorCustomerId : 0,
                            Customer = customerInvoice != null ? StringUtility.Left(customerInvoice.ActorName, 50) : StringUtility.Left(post.CustomerName, 50),
                            InvoiceId = invoice?.InvoiceId ?? 0,
                            InvoiceNr = invoice?.InvoiceNr ?? invoiceNr,
                            InvoiceAmount = invoice != null ? invoice.TotalAmount - invoice.PaidAmount : 0,
                            RestAmount = invoice != null ? invoice.TotalAmount - invoice.PaidAmount - post.Amount : 0,
                            PaidAmount = post.Amount,
                            Currency = "SEK",
                            InvoiceDate = invoice?.DueDate,
                            PaidDate = post.PaidDate,
                            MatchCodeId = 0,
                            Status = status,
                            State = state,
                            ImportType = (int)importType,
                        };

                        totalInvoiceAmount += Utilities.GetAmount(post.Amount);

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
                                    paymentImport.TotalAmount += paymentIO.PaidAmount.Value;
                                    paymentImport.NumberOfPayments = numberOfPayments++;

                                    result = PaymentManager.UpdatePaymentImportHead(entities, paymentImport, paymentImportId, actorCompanyId);

                                    transaction.Complete();
                                }
                                else
                                {
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

        #endregion
    }
}
