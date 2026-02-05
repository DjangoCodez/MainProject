using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Transactions;

namespace SoftOne.Soe.Business.Core.PaymentIO.Pg
{
    public class PgManager : PaymentIOManager
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private PgFile pgFile;
        SettingManager sm;
        #endregion

        #region Ctor

        public PgManager(ParameterObject parameterObject)
            : base(parameterObject)
        {
            sm = new SettingManager(parameterObject);
        }

        #endregion

        #region Import

        public ActionResult Import(StreamReader sr, int actorCompanyId, string fileName, Origin origin, bool templateIsSuggestion, int paymentMethodId, ref List<string> log, int userId = 0, int batchId = 0, int paymentImportId = 0, ImportPaymentType importType = ImportPaymentType.None)
        {
            var result = new ActionResult(true);
            result.ErrorMessage = GetText(8076, "Filimport genomförd");

            sr.DiscardBufferedData();
            sr.BaseStream.Position = 0;

            try
            {
                pgFile = new PgFile();

                bool newSection = false;
                int alternative = 0;

                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    if (string.IsNullOrEmpty(line))
                        continue;

                    line = Utilities.AddPadding(line, Utilities.PG_LINE_MAX_LENGTH);
                    string postCode = Utilities.GetPgPostCode(line).ToString();
                    if (String.IsNullOrEmpty(postCode))
                        continue;

                    int tc = Convert.ToInt32(postCode);

                    if (newSection)
                    {
                        pgFile.Sections.Add(new Section());
                        newSection = false;
                    }

                    switch (tc)
                    {
                        case (int)PgPostType.OpeningPost:
                            if (alternative == 0)
                                alternative = 1;
                            pgFile.OpeningPost = new OpeningPost(line);
                            newSection = true;
                            break;
                        case (int)PgPostType.SenderPost: //only necessary internaly for validation in import
                            pgFile.Sections.Last().SenderPost = new SenderPost();
                            break;
                        case (int)PgPostType.SummaryPost:
                            pgFile.Sections.Last().SummaryPost = new SummaryPost(line);
                            //newSection = true;
                            break;
                        case (int)PgPostType.CreditAmountPost:
                            pgFile.Sections.Last().Posts.Add(new AmountPostCredit(line));
                            break;
                        case (int)PgPostType.DebitAmountPost:
                            pgFile.Sections.Last().Posts.Add(new AmountPostDebit(line));
                            break;
                        case (int)PgPostType.ReceiverIdentityPost:
                            if (alternative == 0) alternative = 2;
                            pgFile.Sections.Last().Posts.Add(new ReceiverIdentityPost(line));
                            break;
                        case (int)PgPostType.ReceiverPost:
                            pgFile.Sections.Last().Posts.Add(new ReceiverPost(line, false, alternative));
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
                if (!pgFile.IsValid())
                {
                    result.Success = false;
                    result.ErrorNumber = (int)ActionResultSave.PaymentFilePgFailed;
                    result.ErrorMessage = GetText(8079, "Filstrukturen är inte giltlig");
                    return result;
                }

                if (userId == 0)
                    result = ConvertPgFileToEntity(pgFile, origin, paymentMethodId, fileName, actorCompanyId, templateIsSuggestion);
                else
                    result = ConvertStreamToEntity(pgFile, origin, paymentMethodId, fileName, actorCompanyId, templateIsSuggestion, ref log, userId, batchId, paymentImportId, importType);


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

        private ActionResult ConvertPgFileToEntity(PgFile fileObject, Origin origin, int paymentMethodId, string fileName, int actorCompanyId, bool templateIsSuggestion)
        {
            var result = new ActionResult(true);

            List<PaymentRow> paymentRowsToAdd = new List<PaymentRow>();
            string paymentNumber = string.Empty;
            string invoiceNumber = string.Empty;

            using (var entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    #region Settings

                    //VoucherSeries
                    int customerInvoicePaymentSeriesTypeId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.CustomerPaymentVoucherSeriesType, 0, actorCompanyId, 0);

                    //AccountYear
                    int accountYearId = SettingManager.GetIntSetting(entities, SettingMainType.UserAndCompany, (int)UserSettingType.AccountingAccountYear, base.UserId, actorCompanyId, 0);

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
                    VoucherSeries voucherSerie = VoucherManager.GetVoucherSerieByType(entities, customerInvoicePaymentSeriesTypeId, accountYearId);
                    if (voucherSerie == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "VoucherSeries");

                    #endregion

                    foreach (Section section in fileObject.Sections)
                    {
                        #region Section

                        foreach (IPost post in section.Posts)
                        {
                            string pgCurrencyCode = string.Empty;

                            switch (post.PostType)
                            {
                                case (int)PgPostType.ReceiverPost:
                                    #region ReceiverPost

                                    var rPost = ((ReceiverPost)post);
                                    paymentNumber = rPost.PaymentNumber.ToString();

                                    #endregion
                                    break;
                                case (int)PgPostType.OpeningPost:
                                    #region OpeningPost

                                    pgCurrencyCode = ((OpeningPost)post).CurrencyCodeAmount;

                                    #endregion
                                    break;
                                case (int)PgPostType.CreditAmountPost:
                                case (int)PgPostType.DebitAmountPost:
                                    #region CreditAmountPost / DebitAmountPost

                                    #region Invoice

                                    Invoice invoice = origin.Invoice;
                                    if (invoice.InvoiceNr == invoiceNumber || Utilities.GetInvoiceNrFromOCR(invoice.OCR) == invoiceNumber)
                                    {
                                        invoice = InvoiceManager.GetInvoice(entities, invoice.InvoiceId);
                                    }
                                    else
                                    {
                                        #region Dummy invoice, since no match was found and the payment needs to be saved

                                        decimal rate = 0M;
                                        int sysCurrencyId = pgCurrencyCode == TermGroup_Currency.EUR.ToString() ? (int)BgMaxCurrency.EUR : (int)BgMaxCurrency.SEK;
                                        CompCurrency currency = CountryCurrencyManager.GetCompCurrency(entities, sysCurrencyId, actorCompanyId);
                                        if (currency != null)
                                            rate = currency.RateToBase;

                                        invoice = new Invoice()
                                        {
                                            Type = (int)SoeInvoiceType.CustomerInvoice,
                                            BillingType = post.PostType == (int)PgPostType.CreditAmountPost ? (int)TermGroup_BillingType.Credit : (int)TermGroup_BillingType.Debit,
                                            InvoiceNr = invoiceNumber,
                                            PaymentNr = paymentNumber,
                                            SysPaymentTypeId = (int)TermGroup_SysPaymentType.PG,
                                            InvoiceDate = ((AmountPost)post).AccountDate,
                                            VoucherDate = ((AmountPost)post).AccountDate,
                                            DueDate = ((AmountPost)post).AccountDate,
                                            CurrencyDate = ((AmountPost)post).AccountDate,
                                            ReferenceYour = string.Empty,
                                            ReferenceOur = "payment from file, not matched",
                                            OCR = string.Empty,
                                            CurrencyRate = rate,
                                            TotalAmount = Utilities.GetAmount(((AmountPost)post).Amount),
                                            TotalAmountCurrency = invoice.TotalAmount / rate,
                                            VATAmount = 0,
                                            VATAmountCurrency = 0,
                                            VatType = 0,
                                        };

                                        #endregion
                                    }

                                    if (invoice == null)
                                        break;

                                    #endregion

                                    #region PaymentRow

                                    //Save payment row
                                    PaymentRow paymentRow = new PaymentRow
                                    {
                                        Status = (int)SoePaymentStatus.Verified,
                                        State = (int)SoeEntityState.Active,
                                        SysPaymentTypeId = (int)TermGroup_SysPaymentType.PG,
                                        PaymentNr = paymentNumber,
                                        PayDate = ((AmountPost)post).AccountDate,
                                        CurrencyRate = invoice.CurrencyRate,
                                        CurrencyDate = invoice.CurrencyDate,
                                        Amount = Utilities.GetAmount(((AmountPost)post).Amount),
                                        BankFee = 0,
                                        AmountDiff = 0,

                                        //Set references
                                        Invoice = invoice,
                                    };
                                    SetCreatedProperties(paymentRow);

                                    //Set currency amounts
                                    CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, paymentRow);

                                    paymentRowsToAdd.Add(paymentRow);

                                    #endregion

                                    #endregion
                                    break;
                            }
                        }

                        #endregion
                    }

                    using (var transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        if (result.Success && !templateIsSuggestion)
                        {
                            Payment payment = new Payment()
                            {
                                //Set references
                                Origin = OriginManager.GetOrigin(entities, origin.OriginId),
                            };

                            Payment paymentOriginal = PaymentManager.GetPayment(entities, payment.PaymentId);

                            foreach (PaymentRow paymentRow in paymentRowsToAdd)
                            {
                                PaymentRowSaveDTO dto = new PaymentRowSaveDTO()
                                {
                                    // Origin
                                    OriginId = origin.OriginId,
                                    OriginType = (SoeOriginType)origin.Type,
                                    OriginStatus = (SoeOriginStatus)origin.Status,
                                    OriginDescription = origin.Description,
                                    VoucherSeriesId = voucherSerie.VoucherSeriesId,
                                    AccountYearId = accountYearId,

                                    // Invoice
                                    InvoiceType = (SoeInvoiceType)paymentRow.Invoice.Type,
                                    OnlyPayment = true,
                                    BillingType = (TermGroup_BillingType)paymentRow.Invoice.BillingType,
                                    ActorId = paymentRow.Invoice.ActorId.Value,
                                    InvoiceNr = paymentRow.Invoice.InvoiceNr,
                                    InvoiceDate = paymentRow.Invoice.InvoiceDate,
                                    PaymentDate = paymentRow.Invoice.DueDate,
                                    VoucherDate = paymentRow.Invoice.VoucherDate,
                                    TotalAmount = paymentRow.Invoice.TotalAmount,
                                    TotalAmountCurrency = paymentRow.Invoice.TotalAmountCurrency,
                                    VatAmount = paymentRow.Invoice.VATAmount,
                                    VatAmountCurrency = paymentRow.Invoice.VATAmountCurrency,
                                    CurrencyId = paymentRow.Invoice.CurrencyId,
                                    CurrencyRate = paymentRow.Invoice.CurrencyRate,
                                    CurrencyDate = paymentRow.Invoice.CurrencyDate,
                                    FullyPayed = paymentRow.Invoice.FullyPayed,

                                    // PaymentImport
                                    ImportDate = DateTime.Now,
                                    ImportFilename = fileName,
                                };

                                if (!result.Success)
                                    continue;

                                //If whole chain should be created, no match on invoice
                                if (paymentRow.IsAdded())
                                {
                                    //save
                                    result = PaymentManager.SavePaymentRow(entities, transaction, dto, null, company.ActorCompanyId, false, true);
                                }
                                else
                                {
                                    //Save paymentRows and paymentImports
                                    payment.PaymentRow.Add(paymentRow);

                                    //Save PaymentAccountRows
                                    CustomerInvoice customerInvoice = InvoiceManager.GetCustomerInvoice(entities, paymentRow.Invoice.InvoiceId, loadActor: true, loadInvoiceRow: true, loadInvoiceAccountRow: true);
                                    result = PaymentManager.AddPaymentAccountRowsFromCustomerInvoice(entities, paymentRow, paymentMethod, customerInvoice, company.ActorCompanyId);
                                }

                                if (!result.Success)
                                    return result;
                            }

                            if (payment.IsAdded())
                                result = AddEntityItem(entities, payment, "Payment", transaction);
                        }

                        #region Deprecated, if used then update with storing unmatched entities
                        /*
                        if (result.Success && templateIsSuggestion)
                        {
                            //Find existing payment
                            if (!origin.PaymentReference.IsLoaded)
                                origin.PaymentReference.Load();

                            Payment paymentSuggestion = paymentManager.GetPayment(entities, origin.Payment.PaymentId, true); ;
                            foreach (PaymentRow paymentRow in paymentSuggestion.PaymentRow)
                            {
                                foreach (PaymentRow paymentRowImported in paymentRowsToAdd)
                                {
                                    //Update fields
                                    if (paymentRowImported.Invoice.InvoiceNr == paymentRow.Invoice.InvoiceNr)
                                    {
                                        paymentRow.PayDate = paymentRowImported.PayDate;
                                        paymentRow.Status = (int)SoePaymentStatus.ManualPayment;
                                        paymentRow.AmountCurrency = paymentRowImported.AmountCurrency;
                                        paymentRow.AmountDiff = paymentRowImported.AmountDiff;
                                    }
                                }
                            }
                            SaveEntityItem(entities, paymentSuggestion, true);
                        }
                        */
                        #endregion

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
            //TODO: somewhere the message is reset, locate correct and remove this check
            if (result.Success && string.IsNullOrEmpty(result.ErrorMessage))
                result.ErrorMessage = GetText(8076, "Filimport genomförd");

            return result;
        }

        private ActionResult ConvertStreamToEntity(PgFile fileObject, Origin origin, int paymentMethodId, string fileName, int actorCompanyId, bool templateIsSuggestion, ref List<string> logText, int userId = 0, int batchId = 0, int paymentImportId = 0, ImportPaymentType importType = ImportPaymentType.None)
        {
            var result = new ActionResult(true);
            var PaymentImportIOToAdd = new List<PaymentImportIO>();
            int status = 0;
            int state = 0;
            int type = 1;
            decimal totalInvoiceAmount = 0;

            List<PaymentRow> paymentRowsToAdd = new List<PaymentRow>();
            string paymentNumber = String.Empty;
            string invoiceNumber = String.Empty;

            DateTime? fileDate = null;

            using (var entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    #region Settings

                    //VoucherSeries
                    int customerInvoicePaymentSeriesTypeId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.CustomerPaymentVoucherSeriesType, 0, actorCompanyId, 0);

                    //AccountYear
                    int accountYearId = SettingManager.GetIntSetting(entities, SettingMainType.UserAndCompany, (int)UserSettingType.AccountingAccountYear, base.UserId, actorCompanyId, 0);

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
                    VoucherSeries voucherSerie = VoucherManager.GetVoucherSerieByType(entities, customerInvoicePaymentSeriesTypeId, accountYearId);
                    if (voucherSerie == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "VoucherSeries");

                    #endregion

                    foreach (Section section in fileObject.Sections)
                    {
                        #region Section

                        foreach (IPost post in section.Posts)
                        {
                            string pgCurrencyCode = string.Empty;

                            switch (post.PostType)
                            {
                                case (int)PgPostType.ReceiverPost:
                                    #region ReceiverPost

                                    var rPost = ((ReceiverPost)post);
                                    paymentNumber = rPost.PaymentNumber.ToString();

                                    #endregion
                                    break;
                                case (int)PgPostType.OpeningPost:
                                    #region OpeningPost

                                    pgCurrencyCode = ((OpeningPost)post).CurrencyCodeAmount;

                                    #endregion
                                    break;
                                case (int)PgPostType.CreditAmountPost:
                                case (int)PgPostType.DebitAmountPost:
                                    #region CreditAmountPost / DebitAmountPost

                                    #region Invoice

                                    Invoice invoice = origin.Invoice;
                                    if (invoice.InvoiceNr == invoiceNumber || Utilities.GetInvoiceNrFromOCR(invoice.OCR) == invoiceNumber)
                                    {
                                        invoice = InvoiceManager.GetInvoice(entities, invoice.InvoiceId);

                                        if (invoice == null)
                                            break;

                                        if (fileDate == null)
                                            fileDate = ((AmountPost)post).AccountDate;

                                        status = (int)ImportPaymentIOStatus.Match;
                                        state = (int)ImportPaymentIOState.Open;
                                        type = invoice.TotalAmount >= 0 ? (int)TermGroup_BillingType.Debit : (int)TermGroup_BillingType.Credit;

                                        if (Utilities.GetAmount(Utilities.GetAmount(((AmountPost)post).Amount)) < invoice.TotalAmount)
                                        {
                                            status = (int)ImportPaymentIOStatus.PartlyPaid;
                                        }

                                        if (Utilities.GetAmount(Utilities.GetAmount(((AmountPost)post).Amount)) > invoice.TotalAmount)
                                        {
                                            status = (int)ImportPaymentIOStatus.Rest;
                                        }

                                        if (invoice.FullyPayed)
                                        {
                                            if (importType == ImportPaymentType.CustomerPayment)
                                                status = (int)ImportPaymentIOStatus.Unknown;
                                            else
                                            {
                                                status = (int)ImportPaymentIOStatus.Paid;
                                                state = (int)ImportPaymentIOState.Closed;
                                            }

                                        }

                                        PaymentImportIO paymentImportIO = new PaymentImportIO
                                        {
                                            ActorCompanyId = actorCompanyId,
                                            BatchNr = batchId,
                                            Type = post.PostType == (int)PgPostType.CreditAmountPost ? (int)TermGroup_BillingType.Credit : (int)TermGroup_BillingType.Debit,
                                            CustomerId = invoice.Actor != null ? invoice.Actor.Customer.ActorCustomerId : 0,
                                            Customer = invoice != null ? StringUtility.Left(invoice.ActorName,50) : StringUtility.Left(((AmountPost)post).SenderReference,50),
                                            InvoiceId = invoice.InvoiceId,
                                            InvoiceNr = invoice.InvoiceNr != null ? invoice.InvoiceNr : string.Empty,
                                            InvoiceAmount = invoice.TotalAmount,
                                            RestAmount = invoice != null ? invoice.TotalAmount - Utilities.GetAmount(((AmountPost)post).Amount) : 0,
                                            PaidAmount = Utilities.GetAmount(((AmountPost)post).Amount),
                                            Currency = "SEK",
                                            InvoiceDate = invoice != null ? invoice.DueDate : null,
                                            PaidDate = ((AmountPost)post).AccountDate,
                                            MatchCodeId = 0,
                                            Status = status,
                                            State = state,
                                            PaidAmountCurrency = Utilities.GetAmount(((AmountPost)post).Amount),
                                            InvoiceSeqnr = invoice.SeqNr.HasValue ? invoice.SeqNr.Value.ToString() : string.Empty,
                                            ImportType = (int)importType,
                                        };

                                        totalInvoiceAmount = totalInvoiceAmount + Utilities.GetAmount(((AmountPost)post).Amount);

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

                                    }
                                    else
                                    {
                                        #region Dummy invoice, since no match was found and the payment needs to be saved

                                        //decimal rate = 0M;
                                        //int sysCurrencyId = pgCurrencyCode == TermGroup_Currency.EUR.ToString() ? (int)BgMaxCurrency.EUR : (int)BgMaxCurrency.SEK;
                                        //CompCurrency currency = CountryCurrencyManager.GetCompCurrency(entities, sysCurrencyId, actorCompanyId);
                                        //if (currency != null)
                                        //    rate = currency.RateToBase;

                                        //invoice = new Invoice()
                                        //{
                                        //    Type = (int)SoeInvoiceType.CustomerInvoice,
                                        //    BillingType = post.PostType == (int)PgPostType.CreditAmountPost ? (int)TermGroup_BillingType.Credit : (int)TermGroup_BillingType.Debit,
                                        //    InvoiceNr = invoiceNumber,
                                        //    PaymentNr = paymentNumber,
                                        //    SysPaymentTypeId = (int)TermGroup_SysPaymentType.PG,
                                        //    InvoiceDate = ((AmountPost)post).AccountDate,
                                        //    VoucherDate = ((AmountPost)post).AccountDate,
                                        //    DueDate = ((AmountPost)post).AccountDate,
                                        //    CurrencyDate = ((AmountPost)post).AccountDate,
                                        //    ReferenceYour = string.Empty,
                                        //    ReferenceOur = "payment from file, not matched",
                                        //    OCR = string.Empty,
                                        //    CurrencyRate = rate,
                                        //    TotalAmount = Utilities.GetAmount(((AmountPost)post).Amount),
                                        //    TotalAmountCurrency = invoice.TotalAmount / rate,
                                        //    VATAmount = 0,
                                        //    VATAmountCurrency = 0,
                                        //    VatType = 0,
                                        //};

                                        status = (int)ImportPaymentIOStatus.Unknown;
                                        state = (int)ImportPaymentIOState.Open;

                                        PaymentImportIO paymentImportIO = new PaymentImportIO
                                        {
                                            ActorCompanyId = actorCompanyId,
                                            BatchNr = batchId,
                                            Type = post.PostType == (int)PgPostType.CreditAmountPost ? (int)TermGroup_BillingType.Credit : (int)TermGroup_BillingType.Debit,
                                            CustomerId = 0,
                                            Customer = StringUtility.Left( ((AmountPost)post).SenderReference, 50),
                                            InvoiceId =  0,
                                            InvoiceNr =  string.Empty,
                                            InvoiceAmount = 0,
                                            RestAmount = 0,
                                            PaidAmount = Utilities.GetAmount(((AmountPost)post).Amount),
                                            Currency = "SEK",
                                            InvoiceDate = null,
                                            PaidDate = ((AmountPost)post).AccountDate,
                                            MatchCodeId = 0,
                                            Status = status,
                                            State = state,
                                            PaidAmountCurrency = Utilities.GetAmount(((AmountPost)post).Amount),
                                            InvoiceSeqnr = string.Empty,
                                            ImportType = (int)importType,
                                        };

                                        PaymentImportIOToAdd.Add(paymentImportIO);

                                        #endregion
                                    }

                                    if (invoice == null)
                                        break;

                                    #endregion

                                    #region PaymentRow

                                    //Save payment row
                                    PaymentRow paymentRow = new PaymentRow
                                    {
                                        Status = (int)SoePaymentStatus.Verified,
                                        State = (int)SoeEntityState.Active,
                                        SysPaymentTypeId = (int)TermGroup_SysPaymentType.PG,
                                        PaymentNr = paymentNumber,
                                        PayDate = ((AmountPost)post).AccountDate,
                                        CurrencyRate = invoice.CurrencyRate,
                                        CurrencyDate = invoice.CurrencyDate,
                                        Amount = Utilities.GetAmount(((AmountPost)post).Amount),
                                        BankFee = 0,
                                        AmountDiff = 0,

                                        //Set references
                                        Invoice = invoice,
                                    };
                                    SetCreatedProperties(paymentRow);

                                    //Set currency amounts
                                    CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, paymentRow);

                                    paymentRowsToAdd.Add(paymentRow);

                                    #endregion

                                    #endregion
                                    break;
                            }
                        }

                        #endregion
                    }

                    using (var transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        if (result.Success && !templateIsSuggestion)
                        {

                            foreach (var paymentIO in PaymentImportIOToAdd)
                            {
                                entities.PaymentImportIO.AddObject(paymentIO);
                            }

                            result = SaveChanges(entities, transaction);
                            if (result.Success)
                            {
                                //ActionResult result2 = new ActionResult();
                                PaymentImport paymentImport = PaymentManager.GetPaymentImport(entities, paymentImportId, actorCompanyId);

                                paymentImport.TotalAmount = totalInvoiceAmount;
                                paymentImport.NumberOfPayments = PaymentImportIOToAdd.Count;
                                paymentImport.ImportDate = fileDate.HasValue ? fileDate.Value : paymentImport.ImportDate;

                                result = PaymentManager.UpdatePaymentImportHead(entities, paymentImport, paymentImportId, actorCompanyId);

                                //Commit transaction
                                transaction.Complete();

                            }
                            else
                            {
                                // Set result
                                result.Success = true;
                                result.ErrorNumber = (int)ActionResultSave.NothingSaved;
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
            //TODO: somewhere the message is reset, locate correct and remove this check
            if (result.Success && string.IsNullOrEmpty(result.ErrorMessage))
                result.ErrorMessage = GetText(8076, "Filimport genomförd");

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
                result = ConvertEntityToPgFile(entities, paymentMethod, paymentRows, paymentId, actorCompanyId);
                if (!result.Success)
                    return result;

                pgFile = result.Value as PgFile;
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
            var customerNr = pgFile.OpeningPost.CustomerNumber;

            byte[] file = WriteFileToMemory(pgFile);
            if (file != null)
                result = CreatePaymentExport( fileName, paymentRows, TermGroup_SysPaymentMethod.PG, customerNr, guid, file);
            else
                result.Success = false;

            return result;
        }

        private ActionResult ConvertEntityToPgFile(CompEntities entities, PaymentMethod paymentMethod, IEnumerable<PaymentRow> paymentRows, int paymentId, int actorCompanyId)
        {
            var result = new ActionResult(true);
            var fileObject = new PgFile();
            int observationMethod = sm.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierPaymentObservationMethod, 0, actorCompanyId, 0);
            int observationDays = sm.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierPaymentObservationDays, 0, actorCompanyId, 0);

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
                SummaryPost summaryPost = null;

                var sysCurrencyId = (int)PgCurrency.Undefined;
                bool newSection = false;
                bool firstSectionCreated = false;
                var customerNr = paymentMethod.CustomerNr;
                var senderAccount = 0;
                decimal totalSum = 0M;
                int totalCount = 0;

                foreach (PaymentRow paymentRow in paymentRows)
                {
                    bool addReceiverPost = false;
                    IPost receiverPost = null;
                    totalCount++;

                    int paymentNumber = Utilities.PaymentNumber(paymentRow.PaymentNr);
                    if (paymentNumber == 0)
                    {
                        throw new ActionFailedException((int)ActionResultSave.PaymentInvalidAccountNumber);
                    }

                    if (paymentRow.Invoice != null)
                        invoice = paymentRow.Invoice;

                    if (!invoice.CurrencyReference.IsLoaded)
                        invoice.CurrencyReference.Load();

                    if (sysCurrencyId == (int)PgCurrency.Undefined)
                    {
                        if (invoice != null)
                        {
                            switch (invoice.Currency.SysCurrencyId)
                            {
                                case (int)PgCurrency.SEK:
                                case (int)PgCurrency.EUR:
                                    break;
                                default:
                                    result.Success = false;
                                    result.ErrorNumber = (int)ActionResultSave.NothingSaved;
                                    result.ErrorMessage = GetText(8080, "Valuta ej giltig för plusgiro");
                                    return result;
                            }
                            sysCurrencyId = invoice.Currency.SysCurrencyId;
                        }
                    }

                    if (invoice != null)
                    {
                        if (!invoice.ActorReference.IsLoaded)
                            invoice.ActorReference.Load();
                        if (!invoice.Actor.SupplierReference.IsLoaded)
                            invoice.Actor.SupplierReference.Load();

                        string receiverIdentity = invoice.Actor.Supplier.SupplierNr;
                        string message = invoice.InvoiceNr;
                        int sysPaymentTypeId = Utilities.GetPgPaymentMethod(paymentRow.SysPaymentTypeId);
                        if (sysPaymentTypeId == 0)
                            continue;

                        string currencyCode = CountryCurrencyManager.GetCurrencyCode(sysCurrencyId);
                        string currencyCodePocket = currencyCode; //n/a
                        int SeqNr = paymentRow.SeqNr;

                        if (String.IsNullOrEmpty(currencyCode))
                            continue;
                        DateTime now = DateTime.Now;
                        int compareResult = 0;
                        switch (invoice.BillingType)
                        {
                            case (int)TermGroup_BillingType.Credit:
                                DateTime firstAccountDate = paymentRow.PayDate;
                                DateTime lastAccountDate = paymentRow.PayDate;

                                compareResult = DateTime.Compare(firstAccountDate, now);
                                if (compareResult < 0)
                                {
                                    firstAccountDate = now;
                                    lastAccountDate = now;
                                }
                                if (observationMethod == (int)SoeSupplierPaymentObservationMethod.Observation ||
                                   observationMethod == (int)SoeSupplierPaymentObservationMethod.ObservationTotalAmount)
                                {
                                    lastAccountDate = lastAccountDate.AddDays(observationDays);
                                }
                                if (sysPaymentTypeId == 4) // BG
                                {
                                    receiverPost = new ReceiverPost(sysPaymentTypeId, receiverIdentity, invoice.Actor.Supplier.Name, paymentNumber);
                                    addReceiverPost = true;
                                    // BG needs reciverIdentity instead of payment nr
                                    post = new AmountPostCredit(sysPaymentTypeId, currencyCode, receiverIdentity, message, Math.Abs(Utilities.GetAmount(paymentRow.Amount)), firstAccountDate, lastAccountDate);
                                }
                                else
                                {
                                    post = new AmountPostCredit(sysPaymentTypeId, currencyCode, paymentNumber.ToString(), message, Math.Abs(Utilities.GetAmount(paymentRow.Amount)), firstAccountDate, lastAccountDate);
                                }
                                break;
                            case (int)TermGroup_BillingType.Debit:
                                DateTime date = paymentRow.PayDate;
                                compareResult = DateTime.Compare(date, now);
                                if (compareResult < 0)
                                    date = now;

                                string verificationNumber = paymentId.ToString();

                                if (sysPaymentTypeId == 4) // BG
                                {
                                    receiverPost = new ReceiverPost(sysPaymentTypeId, receiverIdentity, invoice.Actor.Supplier.Name, paymentNumber);
                                    addReceiverPost = true;
                                    // BG needs reciverIdentity instead of payment nr
                                    post = new AmountPostDebit(sysPaymentTypeId, currencyCode, receiverIdentity, message, Math.Abs(Utilities.GetAmount(paymentRow.Amount)), date, verificationNumber, (int)invoice.SeqNr);
                                }
                                else
                                {
                                    post = new AmountPostDebit(sysPaymentTypeId, currencyCode, paymentNumber, message, Math.Abs(Utilities.GetAmount(paymentRow.Amount)), date, verificationNumber, (int)invoice.SeqNr);
                                }
                                break;
                        }

                        var tmpSenderAccount = Utilities.GetSenderAccountNumber(entities, company, TermGroup_SysPaymentMethod.PG);
                        if (tmpSenderAccount == 0)
                            //Jesper was here
                            return new ActionResult(false, (int)ActionResultSave.EntityNotFound, "Betalningsmetod innehåller inte något betalningsnummer");

                        if (senderAccount != tmpSenderAccount || sysCurrencyId != invoice.Currency.SysCurrencyId)
                            newSection = true;
                        senderAccount = tmpSenderAccount;

                        if (section == null) //create first section
                        {
                            #region Create File's Opening Post
                            DateTime openingDate = DateTime.Now;
                            int productionNumber = GetProductionNumber(customerNr, openingDate, entities);
                            if (productionNumber > Utilities.PG_PRODUCTION_NUMBER_MAX_VALUE)
                            {
                                productionNumber = 0;

                                result.Success = false;
                                result.ErrorNumber = (int)ActionResultSave.PaymentFilePgFailed;
                                result.ErrorMessage = GetText(8081, "Kan inte skapa flera filer");
                                return result;
                            }
                            firstSectionCreated = true;
                            fileObject.OpeningPost = new OpeningPost(customerNr, openingDate, productionNumber);
                            #endregion

                            //new section
                            //always the first post in the section
                            section = CreateSenderPost(company, customerNr, senderAccount, currencyCode, currencyCodePocket);
                        }

                        if (newSection && !firstSectionCreated)
                        {
                            //store previous
                            section.SummaryPost = summaryPost;
                            fileObject.Sections.Add(section);

                            //new section
                            section = CreateSenderPost(company, customerNr, senderAccount, currencyCode, currencyCodePocket);
                            newSection = false;
                        }

                        var receiverName = company.Name;
                        totalSum += paymentRow.Amount;
                        summaryPost = new SummaryPost(customerNr, senderAccount, Utilities.GetAmount(totalSum), totalCount, currencyCodePocket, currencyCode);
                    }

                    if (section != null && post != null)
                    {
                        if (addReceiverPost)
                            section.Posts.Add(receiverPost);
                        section.Posts.Add(post);
                    }
                }
                //Add last section
                if (section != null)
                {
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

        private static Section CreateSenderPost(Company company, string customerNr, int senderAccount, string currencyCode, string currencyCodePocket)
        {
            Section section = new Section();
            string senderClassification1 = company.Name;
            string senderClassification2 = string.Empty;
            section.SenderPost = new SenderPost(customerNr, senderAccount, senderClassification1, senderClassification2, currencyCodePocket, currencyCode);
            return section;
        }

        /// <summary>
        /// Writes the object to the memory and returns a byte array.
        /// </summary>
        /// <param name="fileObject"></param>
        /// <returns></returns>
        private byte[] WriteFileToMemory(PgFile fileObject)
        {
            StreamWriter sw = null;
            MemoryStream ms = new MemoryStream();

            try
            {
                sw = new StreamWriter(ms, Constants.ENCODING_LATIN1);

                sw.WriteLine(fileObject.OpeningPost.ToString());
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

        #region Help Methods

        /// <summary>
        /// Returns the Pg production number for the customer which is 1-9 for a given combination of customerNr and productionDate
        /// If an existing number is used the parser at the bank will overwrite the old exported data.
        /// </summary>
        /// <param name="customerNr"></param>
        /// <param name="productionDate"></param>
        /// <returns></returns>
        private int GetProductionNumber(string customerNr, DateTime productionDate, CompEntities ent)
        {
            var productionEntries = (from payExport in ent.PaymentExport
                                     where payExport.CustomerNr == customerNr && payExport.ExportDate == productionDate
                                     select payExport).Count();
            return ++productionEntries;
        }

        #endregion

        #region Create artifacts

        #region Deprecated
        /*
        private ActionResult AddPaymentExport(CompEntities entities, TransactionScope transaction, string fileName, Payment payment, string customerNr)
        {
            var result = new ActionResult(true);

            var paymentExport = new PaymentExport
            {
                ExportDate = DateTime.Now,
                Filename = fileName,
                NumberOfPayments = payment.PaymentRow.Count,
                CustomerNr = customerNr,
                Type = (int)TermGroup_PaymentMethod.PG,
            };
            SetCreatedProperties(paymentExport);

            payment.PaymentExport = paymentExport;
            return result;
        }
         */
        #endregion

        #endregion
    }
}
