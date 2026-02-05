using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Transactions;

namespace SoftOne.Soe.Business.Core.PaymentIO.Cfp
{
    public class CfpManager : PaymentIOManager
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private CfpFile pgFile;

        #endregion

        #region Ctor

        public CfpManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Import

        public ActionResult Import(StreamReader sr, int actorCompanyId, string fileName, List<PaymentRow> paymentRows, Dictionary<string, decimal> notFoundinFile, int paymentMethodId, ref List<string> log, int userId = 0, int batchId = 0, int paymentImportId = 0, ImportPaymentType importType = ImportPaymentType.None)
        {
            var result = new ActionResult(true);
            result.ErrorMessage = GetText(8076, "Filimport genomförd");

            sr.DiscardBufferedData();
            sr.BaseStream.Position = 0;
            var daPosts = new List<DA1Post>();

            try
            {
                pgFile = new CfpFile();

                DateTime payDate = DateTime.Now;
                decimal amount = 0;
                string seqnr = string.Empty;
                string currencyCode = string.Empty;
                bool added = true;
                string message = string.Empty;

                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    if (string.IsNullOrEmpty(line))
                        continue;

                    line = Utilities.AddPadding(line, Utilities.BGC_LINE_MAX_LENGTH);
                    string postCode = line.Left(4);
                    if (string.IsNullOrEmpty(postCode))
                        continue;

                    switch (postCode)
                    {
                        case Utilities.DA1_OPENING_RECORD_TYPE: //only necessary internaly for validation in import
                            //pgFile.Sections.Last().SenderPost = new SenderPost();
                            //newSection = true;
                            payDate = Utilities.GetDate(line.Substring(36, 8));
                            currencyCode = line.Substring(47, 3);
                            break;
                        case Utilities.DA1_ALTERNATE_OPENING_RECORD_TYPE: //only necessary internaly for validation in import
                            //pgFile.Sections.Last().SenderPost = new SenderPost();
                            //newSection = true;
                            payDate = Utilities.GetDate(line.Substring(72, 8));
                            break;
                        case Utilities.DA1_PAYMENT_RECORD_TYPE:
                            //pgFile.Sections.Last().Posts.Add(new AmountPostDebit(line));
                            if (!added)
                            {
                                daPosts.Add(new DA1Post(payDate, amount, seqnr, currencyCode, message));
                                seqnr = "0";
                                added = true;
                            }

                            amount = Utilities.GetAmount(line.Substring(22, 13));
                            break;
                        case Utilities.DA1_SENDERNOTES_RECORD_TYPE:
                            //pgFile.Sections.Last().Posts.Add(new AmountPostDebit(line));
                            seqnr = line.Substring(31, 35).Trim();
                            added = false;
                            break;
                        case Utilities.DA1_RECEIVER_RECORD_TYPE:
                            //pgFile.Sections.Last().Posts.Add(new AmountPostDebit(line));
                            message = line.Substring(22, 35).Trim();
                            added = false;
                            break;
                        case Utilities.DA1_CLOSING_RECORD_TYPE:
                            //pgFile.Sections.Last().SummaryPost = new SummaryPost(line);
                            //newSection = true;
                            if (!added)
                            {
                                daPosts.Add(new DA1Post(payDate, amount, seqnr, currencyCode, message));
                                seqnr = string.Empty;
                                added = true;
                            }
                            break;
                        case Utilities.DA1_DEBIT_RECORD_TYPE:
                            //pgFile.Sections.Last().Posts.Add(new AmountPostCredit(line));
                            amount = Utilities.GetAmount(line.Substring(39, 13));
                            seqnr = line.Substring(52, 25).Trim();
                            daPosts.Add(new DA1Post(payDate, amount, seqnr, currencyCode, message, false, true));
                            seqnr = "0";
                            added = true;
                            break;
                        case Utilities.DA1_CREDIT_RECORD_TYPE:
                            //pgFile.Sections.Last().Posts.Add(new AmountPostDebit(line));
                            amount = Utilities.GetAmount(line.Substring(39, 13));
                            seqnr = line.Substring(52, 25).Trim();
                            daPosts.Add(new DA1Post(payDate, amount, seqnr, currencyCode, message, true, true));
                            seqnr = "0";
                            added = true;
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
                sr.DiscardBufferedData();
                sr.BaseStream.Position = 0;

                if (userId == 0)
                    result = ConvertCfpFileToEntity(daPosts, paymentMethodId, fileName, actorCompanyId, userId);
                else
                    result = ConvertStreamToEntity(daPosts, paymentRows, paymentMethodId, actorCompanyId, ref log, userId, batchId, paymentImportId, importType);
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

        private ActionResult ConvertCfpFileToEntity(List<DA1Post> daPosts, int paymentMethodId, string fileName, int actorCompanyId, int userId)
        {
            var result = new ActionResult(true);
            var updatedRows = new Dictionary<int, PaymentRow>();
            var status = (int)SoePaymentStatus.Verified;

            using (var entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    #region Settings

                    //VoucherSeries
                    int supplierInvoicePaymentSeriesTypeId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierPaymentVoucherSeriesType, 0, actorCompanyId, 0);

                    //AccountYear
                    int accountYearId = SettingManager.GetIntSetting(entities, SettingMainType.UserAndCompany, (int)UserSettingType.AccountingAccountYear, userId, actorCompanyId, 0);

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

                    //Get default VoucherSerie for Payment for current AccountYear
                    VoucherSeries voucherSerie = VoucherManager.GetVoucherSerieByType(entities, supplierInvoicePaymentSeriesTypeId, accountYearId);
                    if (voucherSerie == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "VoucherSeries");

                    #endregion

                    DateTime? fileDate = null;
                    List<PaymentRow> paymentRows = new List<PaymentRow>();
                    foreach (DA1Post post in daPosts)
                    {
                        foreach (PaymentRow row in paymentRows)
                        {

                            Invoice invoice = InvoiceManager.GetInvoice(row.InvoiceId.Value);
                            if (post.Reference == invoice.SeqNr.GetValueOrDefault().ToString())
                                paymentRows.Add(row);
                        }

                    }

                    foreach (DA1Post post in daPosts)
                    {
                        fileDate = post.PayDate;

                        var row = paymentRows.FirstOrDefault(r => r.SeqNr.ToString() == post.Reference);
                        if (row == null)
                            continue;
                             
                        row.Status = status;
                        row.PayDate = post.PayDate;
                        row.Amount = post.Amount;

                        //Set currency amounts
                        CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, row);

                        updatedRows.Add(row.PaymentRowId, row);
                    }

                    using (var transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        foreach (var item in updatedRows)
                        {
                            if (!result.Success)
                                continue;
                            //Update PaymentRow(s) and apply paymentImport entity
                            result = AddPaymentImport(entities, fileName, item.Value, transaction);
                        }

                        if (result.Success && updatedRows.Count > 0)
                        {
                            // Create voucher for payments
                            VoucherManager vm = new VoucherManager(parameterObject);
                            result = vm.SaveVoucherFromPayment(entities, ConfigSettings.TRANSACTIONSCOPEOPTION_ATTACH, paymentRows, SoeOriginType.SupplierPayment, false, accountYearId, actorCompanyId, true, fileDate);
                        }

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
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            return result;
        }

        private ActionResult AddPaymentImport(CompEntities entities, string fileName, PaymentRow paymentRow, TransactionScope transaction)
        {
            if (paymentRow == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull);

            var result = new ActionResult(true);

            //var originalPaymentRow = PaymentManager.GetPaymentRow(entities, paymentRow.PaymentRowId);
            //if (originalPaymentRow == null)
            //    return new ActionResult(false, (int)ActionResultSave.EntityNotFound, "PaymentRow");

            // Set payment row verified
            paymentRow.Status = (int)SoePaymentStatus.Verified;

            var paymentImport = new PaymentImport
            {
                ImportDate = DateTime.Now,
                Filename = fileName,
            };
            SetCreatedProperties(paymentImport);

            //if (!originalPaymentRow.PaymentImportReference.IsLoaded)
            //    originalPaymentRow.PaymentImportReference.Load();

            //result = UpdateEntityItem(entities, originalPaymentRow, paymentRow, "PaymentRow");

            //originalPaymentRow = PaymentManager.GetPaymentRow(entities, paymentRow.PaymentRowId);
            //originalPaymentRow.PaymentImport = new PaymentImport
            //{
            //    ImportDate = DateTime.Now,
            //    Filename = fileName,
            //};

            //SetCreatedProperties(paymentImport);
            //paymentImport.PaymentRow.Add(paymentRow);
            if (result.Success)
                return SaveEntityItem(entities, paymentRow, transaction);
            return result;
        }

        private ActionResult ConvertStreamToEntity(List<DA1Post> file, List<PaymentRow> paymentRows, int paymentMethodId, int actorCompanyId, ref List<string> log, int userId = 0, int batchId = 0, int paymentImportId = 0, ImportPaymentType importType = ImportPaymentType.None)
        {
            var result = new ActionResult(true);

            var paymentImportIOToAdd = new List<PaymentImportIO>();
            var pm = new PaymentManager(this.parameterObject);
            decimal totalInvoiceAmount = 0;

            using (var entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    #region Settings

                    //VoucherSeries
                    int supplierInvoicePaymentSeriesTypeId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierPaymentVoucherSeriesType, 0, actorCompanyId, 0);

                    //AccountYear
                    int accountYearId = SettingManager.GetIntSetting(entities, SettingMainType.UserAndCompany, (int)UserSettingType.AccountingAccountYear, userId, actorCompanyId, 0);

                    #endregion

                    #region Prereq

                    //PaymentMethod
                    PaymentMethod paymentMethod = PaymentManager.GetPaymentMethod(entities, paymentMethodId, actorCompanyId, true);
                    if (paymentMethod == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "PaymentMethod");

                    //Get default VoucherSerie for Payment for current AccountYear
                    VoucherSeries voucherSerie = VoucherManager.GetVoucherSerieByType(entities, supplierInvoicePaymentSeriesTypeId, accountYearId);
                    if (voucherSerie == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "VoucherSeries");

                    #endregion
                    //List<PaymentRow> paymentRows = new List<PaymentRow>();
                    var cfpCurrencyCode = string.Empty;
                    foreach (DA1Post daPost in file)
                    {
                        #region FileContent

                        cfpCurrencyCode = daPost.CurrencyCode;

                        var row = daPost.ReferenceIsInvoiceNr ? paymentRows.FirstOrDefault(r => r.Invoice.InvoiceNr == daPost.Reference) : paymentRows.FirstOrDefault(r => r.Invoice.SeqNr.Value.ToString() == daPost.Reference);
                        if (row != null)
                        {
                            if (row.Status == (int)SoePaymentStatus.None || row.Status == (int)SoePaymentStatus.Pending)
                            {

                                // TODO: Populate here
                                PaymentImportIO paymentImportIO = new PaymentImportIO
                                {
                                    ActorCompanyId = actorCompanyId,
                                    BatchNr = batchId,
                                    Type = daPost.IsCredit ? (int)TermGroup_BillingType.Credit : (int)TermGroup_BillingType.Debit,
                                    CustomerId = row.Invoice.Actor != null ? row.Invoice.Actor.Supplier.ActorSupplierId : 0,
                                    Customer = row.Invoice.ActorName != null ? StringUtility.Left(row.Invoice.ActorName, 50) : string.Empty,
                                    InvoiceId = row.InvoiceId != null ? row.InvoiceId : 0,
                                    InvoiceNr = row.Invoice.InvoiceNr != null ? row.Invoice.InvoiceNr : string.Empty,
                                    InvoiceAmount = row.Invoice.TotalAmount,
                                    RestAmount = row.Invoice != null ? row.Invoice.TotalAmount - row.Invoice.PaidAmount : 0,
                                    PaidAmount = daPost.Amount,
                                    Currency = cfpCurrencyCode,
                                    InvoiceDate = row.Invoice != null ? row.Invoice.DueDate : null,
                                    PaidDate = daPost.PayDate,
                                    MatchCodeId = 0,
                                    Status = (int)ImportPaymentIOStatus.Match,
                                    State = (int)ImportPaymentIOState.Open,
                                    PaidAmountCurrency = daPost.Amount,
                                    InvoiceSeqnr = row.Invoice.SeqNr.HasValue ? row.Invoice.SeqNr.Value.ToString() : string.Empty,
                                    ImportType = (int)importType,
                                };

                                totalInvoiceAmount = totalInvoiceAmount + daPost.Amount;

                                // Check for duplicates
                                if (paymentImportIOToAdd.Any(p => p.CustomerId == paymentImportIO.CustomerId && p.InvoiceId == paymentImportIO.InvoiceId && p.PaidAmount == paymentImportIO.PaidAmount && p.PaidDate == paymentImportIO.PaidDate))
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

                                paymentImportIOToAdd.Add(paymentImportIO);
                            }
                            else if(row.Status == (int)SoePaymentStatus.Checked)
                            {
                                PaymentImportIO paymentImportIO = new PaymentImportIO
                                {
                                    ActorCompanyId = actorCompanyId,
                                    BatchNr = batchId,
                                    Type = daPost.IsCredit ? (int)TermGroup_BillingType.Credit : (int)TermGroup_BillingType.Debit,
                                    CustomerId = row.Invoice.Actor != null ? row.Invoice.Actor.Supplier.ActorSupplierId : 0,
                                    Customer = row.Invoice.ActorName != null ? StringUtility.Left(row.Invoice.ActorName, 50) : string.Empty,
                                    InvoiceId = row.InvoiceId != null ? row.InvoiceId : 0,
                                    InvoiceNr = row.Invoice.InvoiceNr != null ? row.Invoice.InvoiceNr : string.Empty,
                                    InvoiceAmount = row.Invoice.TotalAmount,
                                    RestAmount = row.Invoice != null ? row.Invoice.TotalAmount - row.Invoice.PaidAmount : 0,
                                    PaidAmount = daPost.Amount,
                                    Currency = cfpCurrencyCode,
                                    InvoiceDate = row.Invoice != null ? row.Invoice.DueDate : null,
                                    PaidDate = daPost.PayDate,
                                    MatchCodeId = 0,
                                    Status = (int)ImportPaymentIOStatus.Paid,
                                    State = (int)ImportPaymentIOState.Closed,
                                    PaidAmountCurrency = daPost.Amount,
                                    InvoiceSeqnr = row.Invoice.SeqNr.HasValue ? row.Invoice.SeqNr.Value.ToString() : string.Empty,
                                    ImportType = (int)importType,
                                };

                                paymentImportIOToAdd.Add(paymentImportIO);

                                totalInvoiceAmount = totalInvoiceAmount + daPost.Amount;
                                
                                log.Add(GetText(7080, "Betalning med sekvensnummer") + " " + row.SeqNr + " " + GetText(7081, "för faktura med nr") + " " + row.Invoice.InvoiceNr + " " + GetText(7082, "är redan verifierad"));
                            }
                            else
                            {
                                log.Add(GetText(7080, "Betalning med sekvensnummer") + " " + row.SeqNr + " " + GetText(7081, "för faktura med nr") + " " + row.Invoice.InvoiceNr + " " + GetText(7082, "är redan verifierad"));
                            }
                        }
                        else
                        {

                            log.Add(GetText(7083, "Kunde inte hitta en matchande betalning till rad") + ": " + daPost.ToString());

                            PaymentImportIO paymentImportIO = new PaymentImportIO
                            {
                                ActorCompanyId = actorCompanyId,
                                BatchNr = batchId,
                                Type = daPost.IsCredit ? (int)TermGroup_BillingType.Credit : (int)TermGroup_BillingType.Debit,
                                CustomerId = 0,
                                Customer = StringUtility.Left(daPost.Message, 50),
                                InvoiceId = 0,
                                InvoiceNr = daPost.Reference,
                                InvoiceAmount = 0,
                                RestAmount = 0,
                                PaidAmount = daPost.Amount,
                                Currency = "SEK",
                                InvoiceDate = null,
                                PaidDate = daPost.PayDate,
                                MatchCodeId = 0,
                                Status = (int)ImportPaymentIOStatus.Error,
                                State = (int)ImportPaymentIOState.Closed,
                                PaidAmountCurrency = daPost.Amount,
                                InvoiceSeqnr = string.Empty,
                                ImportType = (int)importType,
                            };

                            paymentImportIOToAdd.Add(paymentImportIO);
                        }
                    }
                    #endregion

                    int numberOfPayments = 1;

                    foreach (var paymentIO in paymentImportIOToAdd)
                    {
                        using (var transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                        {
                            if (result.Success)
                            {

                                entities.PaymentImportIO.AddObject(paymentIO);

                                result = SaveEntityItem(entities, paymentIO, transaction);

                                if (result.Success)
                                {
                                    PaymentImport paymentImport = pm.GetPaymentImport(entities, paymentImportId, actorCompanyId);

                                    paymentImport.TotalAmount = paymentImport.TotalAmount + paymentIO.PaidAmount.Value;
                                    paymentImport.NumberOfPayments = numberOfPayments++;

                                    //paymentImport.ImportDate = fileDate.HasValue ? fileDate.Value : paymentImport.ImportDate;

                                    result = pm.UpdatePaymentImportHead(entities, paymentImport, paymentImportId, actorCompanyId);

                                    transaction.Complete();
                                }
                                else
                                {
                                    result.ErrorNumber = (int)ActionResultSave.NothingSaved;
                                    result.ErrorMessage = string.Format("Leverantörsfaktura med nr {0} är felaktig, importen är avbruten!", paymentIO.InvoiceNr);

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
                    if (result.Success)
                    {
                        //Set success properties
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            return result;
        }
        #endregion

        #region Export

        /// <summary>
        /// Exports PgFile to stream for save dialog
        /// </summary>
        /// <param name="entities">The ObjectContext to use</param>
        /// <param name="transaction">The current Transction</param>
        /// <param name="actorCompanyId">The ActorCompanyId</param>
        /// <param name="payment">The Payment</param>
        /// <returns>PaymentExport, contains null if the export failed</returns>
        public ActionResult Export(CompEntities entities, TransactionScope transaction, PaymentMethod paymentMethod, List<PaymentRow> paymentRows, int paymentId, int actorCompanyId)
        {
            var result = new ActionResult(true);

            try
            {
                result = ConvertEntityToCfpFile(entities, paymentMethod, paymentRows, paymentId, actorCompanyId);
                if (!result.Success)
                    return result;

                pgFile = result.Value as CfpFile;
                if (pgFile == null)
                    return result;
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result.Exception = ex;
                return result;
            }

            string guid = Guid.NewGuid().ToString("N");
            var fileName = Utilities.GetPGFileNameOnServer(guid);
            var customerNr = pgFile.Sections[0].SenderPost.CustomerNumber;

            byte[] file = WriteFileToMemory(pgFile);
            if (file != null)
                result = CreatePaymentExport(fileName, paymentRows, TermGroup_SysPaymentMethod.Cfp, customerNr, guid, file);
            else
                result.Success = false;

            return result;
        }

        private ActionResult ConvertEntityToCfpFile(CompEntities entities, PaymentMethod paymentMethod, IEnumerable<PaymentRow> paymentRows, int paymentId, int actorCompanyId)
        {
            var result = new ActionResult(true);
            var fileObject = new CfpFile();
            //int observationMethod = sm.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierPaymentObservationMethod, 0, actorCompanyId);
            //int observationDays = sm.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierPaymentObservationDays, 0, actorCompanyId);
            //Sort Valuta, Datum, Lev

            try
            {
                #region Prereq

                Company company = CompanyManager.GetCompany(entities, actorCompanyId);
                if (company == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                if (!company.ActorReference.IsLoaded)
                    company.ActorReference.Load();
                if (company.Actor == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "Actor");

                if (!company.Actor.SupplierReference.IsLoaded)
                    company.Actor.SupplierReference.Load();

                if (!company.PaymentMethod.IsLoaded)
                    company.PaymentMethod.Load();
                if (company.PaymentMethod == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "PaymentMethod");

                #endregion

                Invoice invoice = null;
                Section section = null;
                IPost post = null;
                IPost messagePost = null;
                IPost creditPost = null;
                SummaryPost summaryPost = null;

                var sysCurrencyId = (int)PgCurrency.Undefined;
                bool newSection = false;
                bool firstSectionCreated = false;
                bool addCreditPost = false;
                var customerNr = Utilities.PaymentNumberLong(company.OrgNr);
                string senderAccount = String.Empty;
                decimal totalSum = 0M;
                int totalCount = 0;
                int tmpSysPaymentTypeId = 0;
                DateTime tmpPayDate = DateTime.MinValue;
                int tmpActorId = 0;

                // Set PayDate atleast today
                DateTime now = DateTime.Now;
                int compareResult = 0;
                foreach (PaymentRow row in paymentRows)
                {
                    compareResult = DateTime.Compare(row.PayDate, now);
                    if (compareResult < 0)
                        row.PayDate = now;
                }

                // Sort
                paymentRows = paymentRows.OrderBy(r => r.SysPaymentTypeId).ThenBy(r => r.PayDate).ThenBy(r => r.Invoice.ActorId);

                // Check if credit is greater then debit
                var groups = paymentRows.GroupBy(r => new { r.SysPaymentTypeId, r.PayDate, r.Invoice.ActorId }).Select(g =>
                    new { id = g.Key, total = g.Sum(r => r.Amount), DebitCount = g.Count(r => r.Amount > 0), CreditCount = g.Count(r => r.Amount < 0) });
                foreach (var rec in groups)
                {
                    if (rec.total < 0)
                    {
                        result.Success = false;
                        result.ErrorNumber = (int)ActionResultSave.PaymentIncorrectCreditAmount;
                        return result;
                    }
                }

                foreach (PaymentRow paymentRow in paymentRows)
                {
                    bool addMessagePost = false;

                    string paymentNumber = Utilities.PaymentNumberLong(paymentRow.PaymentNr);
                    if (string.IsNullOrEmpty(paymentNumber) || string.IsNullOrWhiteSpace(paymentNumber))
                    {
                        throw new ActionFailedException((int)ActionResultSave.PaymentInvalidAccountNumber);
                    }

                    if (paymentRow.Invoice != null)
                        invoice = paymentRow.Invoice;

                    if (!invoice.CurrencyReference.IsLoaded)
                        invoice.CurrencyReference.Load();

                    if (sysCurrencyId == (int)PgCurrency.Undefined && invoice != null)
                    {
                        switch (invoice.Currency.SysCurrencyId)
                        {
                            case (int)PgCurrency.SEK:
                                break;
                            default:
                                result.Success = false;
                                result.ErrorNumber = (int)ActionResultSave.NothingSaved;
                                result.ErrorMessage = GetText(8080, "Valuta ej giltig för plusgiro");
                                return result;
                        }
                        sysCurrencyId = invoice.Currency.SysCurrencyId;
                    }

                    if (invoice != null)
                    {
                        if (!invoice.ActorReference.IsLoaded)
                            invoice.ActorReference.Load();
                        if (!invoice.Actor.SupplierReference.IsLoaded)
                            invoice.Actor.SupplierReference.Load();

                        string message = String.IsNullOrEmpty(invoice.OCR) ? invoice.InvoiceNr : invoice.OCR;
                        int sysPaymentTypeId = Utilities.GetCfpPaymentMethod(paymentRow.SysPaymentTypeId);
                        if (sysPaymentTypeId == 99)
                            continue;

                        string currencyCode = CountryCurrencyManager.GetCurrencyCode(sysCurrencyId);
                        string currencyCodePocket = currencyCode; //n/a
                        int SeqNr = paymentRow.SeqNr;

                        if (string.IsNullOrEmpty(currencyCode))
                            continue;

                        string verificationNumber = paymentId.ToString();

                        // Check if credit for this post
                        var check = (from rec in groups
                                     where rec.id.SysPaymentTypeId == paymentRow.SysPaymentTypeId &&
                                           rec.id.PayDate == paymentRow.PayDate &&
                                           rec.id.ActorId == invoice.ActorId &&
                                           rec.CreditCount > 0
                                     select rec).ToList();

                        if (paymentRow.SysPaymentTypeId != tmpSysPaymentTypeId || paymentRow.PayDate != tmpPayDate || invoice.ActorId != tmpActorId)
                        {
                            addCreditPost = false;
                            if (check != null && check.Count == 1)
                            {
                                if (!addCreditPost)
                                {
                                    post = new AmountPost(sysPaymentTypeId, currencyCode, paymentNumber, string.Empty, Math.Abs(Utilities.GetAmount(check[0].total)), paymentRow.PayDate);
                                    messagePost = new MessagePost(SeqNr);
                                    addMessagePost = true;
                                    addCreditPost = true;
                                    if (invoice.BillingType == (int)TermGroup_BillingType.Debit)
                                        creditPost = new AmountPostDebit(message, Math.Abs(Utilities.GetAmount(paymentRow.Amount)), invoice.SeqNr.ToString());
                                    else
                                        creditPost = new AmountPostCredit(message, Math.Abs(Utilities.GetAmount(paymentRow.Amount)), invoice.SeqNr.ToString());
                                }
                                else
                                {
                                    addCreditPost = true;
                                    if (invoice.BillingType == (int)TermGroup_BillingType.Debit)
                                        creditPost = new AmountPostDebit(message, Math.Abs(Utilities.GetAmount(paymentRow.Amount)), invoice.SeqNr.ToString());
                                    else
                                        creditPost = new AmountPostCredit(message, Math.Abs(Utilities.GetAmount(paymentRow.Amount)), invoice.SeqNr.ToString());
                                }
                            }
                        }
                        else
                        {
                            if (check != null && check.Count == 1)
                            {
                                if (!addCreditPost)
                                {
                                    post = new AmountPost(sysPaymentTypeId, currencyCode, paymentNumber, String.Empty, Math.Abs(Utilities.GetAmount(check[0].total)), paymentRow.PayDate);
                                    messagePost = new MessagePost(SeqNr);
                                    addMessagePost = true;
                                    addCreditPost = true;
                                    if (invoice.BillingType == (int)TermGroup_BillingType.Debit)
                                        creditPost = new AmountPostDebit(message, Math.Abs(Utilities.GetAmount(paymentRow.Amount)), invoice.SeqNr.ToString());
                                    else
                                        creditPost = new AmountPostCredit(message, Math.Abs(Utilities.GetAmount(paymentRow.Amount)), invoice.SeqNr.ToString());
                                }
                                else
                                {
                                    addCreditPost = true;
                                    if (invoice.BillingType == (int)TermGroup_BillingType.Debit)
                                        creditPost = new AmountPostDebit(message, Math.Abs(Utilities.GetAmount(paymentRow.Amount)), invoice.SeqNr.ToString());
                                    else
                                        creditPost = new AmountPostCredit(message, Math.Abs(Utilities.GetAmount(paymentRow.Amount)), invoice.SeqNr.ToString());
                                }
                            }
                            else
                                addCreditPost = false;
                        }
                        switch (invoice.BillingType)
                        {
                            case (int)TermGroup_BillingType.Credit:
                                break;
                            case (int)TermGroup_BillingType.Debit:
                                if (sysPaymentTypeId == 9) // Bank
                                {
                                    post = new AmountPost(sysPaymentTypeId, currencyCode, paymentNumber, message, Math.Abs(Utilities.GetAmount(paymentRow.Amount)), paymentRow.PayDate);
                                    messagePost = new MessagePost(invoice.SeqNr.ToString());
                                    addMessagePost = true;
                                }
                                else
                                {
                                    if (addCreditPost)
                                    {
                                    }
                                    else
                                    {
                                        post = new AmountPost(sysPaymentTypeId, currencyCode, paymentNumber, message, Math.Abs(Utilities.GetAmount(paymentRow.Amount)), paymentRow.PayDate);
                                        messagePost = new MessagePost(invoice.SeqNr.ToString());
                                        addMessagePost = true;
                                    }
                                }
                                break;
                        }

                        string tmpSenderAccount = Utilities.GetSenderAccountNumber(entities, company, TermGroup_SysPaymentMethod.Cfp).ToString();
                        tmpSenderAccount = Utilities.PaymentNumberLong(tmpSenderAccount);
                        if (String.IsNullOrEmpty(tmpSenderAccount))
                            //Jesper was here
                            return new ActionResult(false, (int)ActionResultSave.EntityNotFound, "Betalningsmetod innehåller inte något betalningsnummer");

                        if (senderAccount != tmpSenderAccount || sysCurrencyId != invoice.Currency.SysCurrencyId)
                            newSection = true;
                        senderAccount = tmpSenderAccount;

                        if (section == null) //create first section
                        {
                            #region Create File's Opening Post
                            firstSectionCreated = true;

                            //new section
                            //always the first post in the section
                            section = new Section();
                            section.SenderPost = new SenderPost(customerNr, senderAccount, currencyCode, currencyCodePocket);
                            #endregion
                        }

                        if (newSection && !firstSectionCreated)
                        {
                            //store previous
                            section.SummaryPost = summaryPost;
                            fileObject.Sections.Add(section);

                            //new section
                            section = new Section
                            {
                                SenderPost = new SenderPost(customerNr, senderAccount, currencyCode, currencyCodePocket)
                            };
                            newSection = false;
                        }

                        totalSum += paymentRow.Amount;
                        summaryPost = new SummaryPost(Utilities.GetAmount(totalSum), totalCount);
                    }

                    if (section != null && (post != null || creditPost != null))
                    {
                        if (post != null)
                        {
                            section.Posts.Add(post);
                            tmpActorId = (int)invoice.ActorId;
                            tmpPayDate = paymentRow.PayDate;
                            tmpSysPaymentTypeId = paymentRow.SysPaymentTypeId;
                            post = null;
                            totalCount++;
                        }
                        if (addMessagePost)
                            section.Posts.Add(messagePost);
                        if (addCreditPost)
                            section.Posts.Add(creditPost);
                    }
                }
                //Add last section
                if (section != null)
                {
                    summaryPost.TotalNumberOfPosts = totalCount;
                    section.SummaryPost = summaryPost;
                    fileObject.Sections.Add(section);
                }
            }
            catch (ActionFailedException ex)
            {
                result.Success = false;
                result.ErrorNumber = ex.ErrorNumber;
                return result;
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result.Exception = ex;
                return result;
            }

            if (result.Success && fileObject.IsValid())
                result.Value = fileObject;

            return result;
        }

        /// <summary>
        /// Writes the object to the memory and returns a byte array.
        /// </summary>
        /// <param name="fileObject"></param>
        /// <returns></returns>
        private byte[] WriteFileToMemory(CfpFile fileObject)
        {
            StreamWriter sw = null;
            MemoryStream ms = new MemoryStream();

            try
            {
                sw = new StreamWriter(ms, Constants.ENCODING_LATIN1);

                //sw.WriteLine(fileObject.OpeningPost.ToString());
                foreach (Section section in fileObject.Sections)
                {
                    sw.WriteLine(section.SenderPost.ToString());
                    foreach (IPost post in section.Posts)
                    {
                        sw.WriteLine(post.ToString());
                    }
                    sw.WriteLine(section.SummaryPost.ToString());
                }
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                return null;
            }
            finally
            {
                if (sw != null)
                    sw.Close();
            }

            return ms.ToArray();
        }

        #endregion
    }
}
