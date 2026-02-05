using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Transactions;

namespace SoftOne.Soe.Business.Core.PaymentIO.BgMax
{
    public class BgMaxManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private BgMaxFile bgMaxFile;

        #endregion

        #region Ctor

        public BgMaxManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Import

        public ActionResult Import(StreamReader sr, int actorCompanyId, string fileName, List<Origin> origins, bool templateIsSuggestion, int paymentMethodId, ref List<string> log, int userId, int batchId, int paymentImportId = 0, ImportPaymentType importType = ImportPaymentType.None)
        {
            var result = new ActionResult
            {
                ErrorMessage = GetText(8076, "Filimport genomförd"),
                Success = true
            };

            sr.DiscardBufferedData();
            sr.BaseStream.Position = 0;

            try
            {
                bgMaxFile = new BgMaxFile();

                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    if (string.IsNullOrEmpty(line))
                        continue;
                    line = Utilities.AddPadding(line, Utilities.BGC_LINE_MAX_LENGTH);

                    string transactionCode = Utilities.GetBgMaxTransactionCode(line);
                    if (string.IsNullOrEmpty(transactionCode))
                        continue;
                    var tc = Convert.ToInt32(transactionCode);

                    switch (tc)
                    {
                        case (int)BgMaxTransactionCodes.PaymentGroupEnd:
                            bgMaxFile.Sections.Last().PaymentEnd = new PaymentEnd(line);
                            break;

                        case (int)BgMaxTransactionCodes.PaymentGroupStart:
                            var section = new Section();
                            section.PaymentGroups.Add(new PaymentGroup());
                            bgMaxFile.Sections.Add(section);
                            bgMaxFile.Sections.Last().PaymentStart = new PaymentStart(line);
                            break;

                        case (int)BgMaxTransactionCodes.PaymentPost:
                        case (int)BgMaxTransactionCodes.PaymentReductionPost:
                            if (bgMaxFile.Sections.Last().PaymentGroups.Last().Posts.FindAll(IsPaymentPost).Count != 0)
                                bgMaxFile.Sections.Last().PaymentGroups.Add(new PaymentGroup());
                            bgMaxFile.Sections.Last().PaymentGroups.Last().Posts.Add(new PaymentPost(line));
                            break;

                        case (int)BgMaxTransactionCodes.ReferenceNumberPost1:
                        case (int)BgMaxTransactionCodes.ReferenceNumberPost2:
                            bgMaxFile.Sections.Last().PaymentGroups.Last().Posts.Add(new PaymentPost(line));
                            break;

                        case (int)BgMaxTransactionCodes.InformationPost:
                            var lastPaymentPost = bgMaxFile.Sections.Last().PaymentGroups.Last().Posts.LastOrDefault() as PaymentPost;
                            if (lastPaymentPost != null)
                                lastPaymentPost.InformationText = line.Substring(2, 50).Trim();
                            break;

                        case (int)BgMaxTransactionCodes.SectionEnd:
                            bgMaxFile.SectionEnd = new SectionEnd(line);
                            break;

                        case (int)BgMaxTransactionCodes.SectionStart:
                            bgMaxFile.SectionStart = new SectionStart(line);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result.Exception = ex;
            }
            try
            {
                if (!bgMaxFile.IsValid())
                {
                    result.Success = false;
                    result.ErrorNumber = (int)ActionResultSave.PaymentFileBgMaxFailed;
                    result.ErrorMessage = GetText(8077, "Importen kunde inte slutföras. Kontrollera att du har valt rätt fil eller betalningsmetod.");
                    PaymentManager.DeletePaymentImportHead(paymentImportId, actorCompanyId);
                    return result;
                }

                if (userId == 0)
                    result = ConvertBgMaxFileToEntity(bgMaxFile, actorCompanyId, origins, paymentMethodId, templateIsSuggestion, ref log);
                else
                    result = ConvertStreamToEntity(bgMaxFile, actorCompanyId, origins, paymentMethodId, templateIsSuggestion, ref log, userId, batchId, paymentImportId, importType);

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

        private ActionResult ConvertBgMaxFileToEntity(BgMaxFile file, int actorCompanyId, List<Origin> origins, int paymentMethodId, bool templateIsSuggestion, ref List<string> logText, int userId = 0)
        {
            Origin origin = null;
            var result = new ActionResult(true);
            var paymentRowsToAdd = new List<PaymentRow>();

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

                    foreach (Section section in file.Sections)
                    {
                        #region Section

                        string paymentNr = section.PaymentEnd.ReceiverBankAccountNumber.Trim();

                        foreach (PaymentGroup paymentGroup in section.PaymentGroups)
                        {
                            foreach (PaymentPost post in paymentGroup.Posts)
                            {
                                switch (post.TransactionCode)
                                {
                                    case (int)BgMaxTransactionCodes.ReferenceNumberPost1:
                                    case (int)BgMaxTransactionCodes.ReferenceNumberPost2:
                                    case (int)BgMaxTransactionCodes.PaymentPost:
                                    case (int)BgMaxTransactionCodes.PaymentReductionPost:
                                        #region ReferenceNumberPost1, ReferenceNumberPost2, PaymentPost, PaymentReductionPost

                                        #region InvoiceNr

                                        string invoiceNr = string.Empty;
                                        switch (post.ReferenceCode)
                                        {
                                            case 0:
                                            case 1:
                                                break;
                                            case 2:
                                            case 3:
                                            case 4:
                                            case 5:
                                                invoiceNr = post.Reference.Trim();

                                                //invoiceNr = post.BGCPaymentSequenceNumber.Trim();
                                                break;
                                        }

                                        if (String.IsNullOrEmpty(invoiceNr))
                                            break;

                                        #endregion

                                        #region Invoice

                                        Invoice invoice = null;

                                        origin = origins.FirstOrDefault(o => o.Invoice.InvoiceNr == invoiceNr);

                                        // if post ReferenceCode = 2 OCR
                                        if (origin == null)
                                        {
                                            logText.Add(GetText(7195, "Underlaget kunde inte hittas för faktura med nummer") + ": " + invoiceNr + ".");
                                            continue;
                                        }

                                        //Reverify this condition
                                        if (origin.Invoice.InvoiceNr == invoiceNr || Utilities.GetInvoiceNrFromOCR(origin.Invoice.OCR) == invoiceNr)
                                        {
                                            invoice = InvoiceManager.GetInvoice(entities, origin.Invoice.InvoiceId);

                                            if (invoice == null)
                                            {
                                                logText.Add(GetText(7195, "Underlaget kunde inte hittas för faktura med nummer") + ": " + invoiceNr + ".");
                                                continue;
                                            }
                                            else
                                            {
                                                if (!invoice.PaymentRow.IsLoaded)
                                                    invoice.PaymentRow.Load();

                                                if (invoice.PaymentRow.Any())
                                                {
                                                    decimal sum = invoice.PaymentRow.Where(p => p.State == (int)SoeEntityState.Active).Select(p => p.AmountCurrency).Sum();
                                                    if (invoice.TotalAmountCurrency < (sum + Utilities.GetAmount(post.PaymentAmount)))
                                                    {
                                                        logText.Add(GetText(7197, "Faktura med nummer") + ": " + invoiceNr + " " + GetText(7198, "prickades ej av") + ". " + GetText(7199, "Beloppet på betalningar överstiger fakturans totalbelopp") + ".");
                                                        continue;
                                                    }
                                                }
                                            }

                                        }
                                        else
                                        {
                                            #region OBSOLETE - Dummy invoice, since no match was found and the payment needs to be saved

                                            /*decimal rate = 0M;
                                            int sysCurrencyId = bgCurrencyCode == TermGroup_Currency.EUR.ToString() ? (int)BgMaxCurrency.EUR : (int)BgMaxCurrency.SEK;
                                            CompCurrency currency = CountryCurrencyManager.GetCompCurrency(entities, sysCurrencyId, actorCompanyId);
                                            if (currency != null)
                                                rate = currency.RateToBase;

                                            invoice = new Invoice()
                                            {
                                                Type = (int)SoeInvoiceType.CustomerInvoice,
                                                BillingType = post.TransactionCode == (int)PgPostType.CreditAmountPost ? (int)TermGroup_BillingType.Credit : (int)TermGroup_BillingType.Debit,
                                                InvoiceNr = invoiceNr,
                                                PaymentNr = paymentNr,
                                                SysPaymentTypeId = (int)TermGroup_SysPaymentType.BG,
                                                ReferenceYour = post.Reference,
                                                ReferenceOur = "payment from file, not matched",
                                                OCR = string.Empty,
                                                CurrencyRate = rate,
                                                TotalAmount = Utilities.GetAmount(post.PaymentAmount),
                                                TotalAmountCurrency = invoice.TotalAmount / rate,
                                                VATAmount = 0,
                                                VATAmountCurrency = 0,
                                                VatType = 0,
                                            };*/

                                            #endregion
                                        }

                                        if (invoice == null)
                                            break;

                                        foreach (OriginInvoiceMapping oimap in origin.OriginInvoiceMapping.Where(m => m.Type == (int)SoeOriginInvoiceMappingType.CustomerPayment))
                                        {
                                            Invoice originInvoice = oimap.Invoice;
                                            if (originInvoice.InvoiceNr == invoiceNr || originInvoice.OCR == invoiceNr)
                                                invoice = originInvoice;
                                        }

                                        #endregion

                                        #region PaymentRow

                                        //Save payment row
                                        PaymentRow paymentRow = new PaymentRow
                                        {
                                            Status = (int)SoePaymentStatus.Verified,
                                            State = (int)SoeEntityState.Active,
                                            SysPaymentTypeId = (int)TermGroup_SysPaymentType.BG,
                                            PaymentNr = paymentNr,
                                            CurrencyRate = invoice.CurrencyRate,
                                            CurrencyDate = invoice.CurrencyDate,
                                            Amount = Utilities.GetAmount(post.PaymentAmount),
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
                        }

                        //Set payment dates
                        foreach (PaymentRow paymentRow in paymentRowsToAdd)
                        {
                            //Don't set previous groups dates
                            if (paymentRow.PayDate < CalendarUtility.DATETIME_DEFAULT)
                            {
                                paymentRow.PayDate = section.PaymentEnd.PaymentDate;

                                //Propogate to newly created invoices
                                if (paymentRow.Invoice.IsAdded())
                                {
                                    DateTime date = section.PaymentEnd.PaymentDate;
                                    paymentRow.Invoice.DueDate = date;
                                    paymentRow.Invoice.CurrencyDate = date;
                                    paymentRow.Invoice.VoucherDate = date;
                                }
                            }
                        }

                        #endregion
                    }

                    using (var transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        if (result.Success && !templateIsSuggestion)
                        {
                            int seqNr = SequenceNumberManager.GetNextSequenceNumber(entities, transaction, actorCompanyId, Enum.GetName(typeof(SoeOriginType), SoeOriginType.CustomerPayment), 1, false);

                            #region Origin payment

                            //Create payment Origin for the SupplierInvoice
                            Origin paymentOrigin = new Origin()
                            {
                                Type = (int)SoeOriginType.CustomerPayment,
                                Status = (int)SoeOriginStatus.Payment,

                                //Set references
                                Company = company,
                                VoucherSeries = voucherSerie,
                            };
                            SetCreatedProperties(paymentOrigin);

                            #endregion

                            #region Payment

                            //Create Payment
                            Payment payment = new Payment()
                            {
                                //Set references
                                Origin = paymentOrigin,
                                PaymentMethod = paymentMethod,
                            };
                            SetCreatedProperties(payment);

                            #endregion

                            foreach (PaymentRow paymentRow in paymentRowsToAdd)
                            {
                                CustomerInvoice customerInvoice = InvoiceManager.GetCustomerInvoice(entities, paymentRow.Invoice.InvoiceId, loadActor: true, loadInvoiceRow: true, loadInvoiceAccountRow: true, loadOrigin: true);

                                #region CustomerInvoice

                                //Get original and loaded CustomerInvoice
                                /*CustomerInvoice customerInvoice = InvoiceManager.GetCustomerInvoice(entities, item.InvoiceId, true, true, true, false, true, false, false, true, true, false, false, false);
                                if (customerInvoice == null || !customerInvoice.ActorId.HasValue)
                                    return new ActionResult((int)ActionResultSave.EntityNotFound, "CustomerInvoice");*/

                                //Can only get status Payment from Origin or Voucher
                                if (customerInvoice.Origin.Status != (int)SoeOriginStatus.Origin && customerInvoice.Origin.Status != (int)SoeOriginStatus.Voucher)
                                {
                                    result.Success = false;
                                    result.ErrorNumber = (int)ActionResultSave.InvalidStateTransition;
                                    return result;
                                }

                                //Add PaymentOrigin to CustomerInvoice
                                OriginInvoiceMapping oimap = new OriginInvoiceMapping()
                                {
                                    Type = (int)SoeOriginInvoiceMappingType.CustomerPayment,

                                    //Set references
                                    Origin = paymentOrigin,
                                };
                                customerInvoice.OriginInvoiceMapping.Add(oimap);

                                #endregion

                                #region PaymentRow

                                paymentRow.Payment = payment;//SeqNr
                                SetCreatedProperties(paymentRow);

                                paymentRow.SeqNr = seqNr;

                                //PaidAmount
                                customerInvoice.PaidAmount += paymentRow.Amount;
                                customerInvoice.PaidAmountCurrency += paymentRow.AmountCurrency;

                                //FullyPayed
                                InvoiceManager.SetInvoiceFullyPayed(entities, customerInvoice, customerInvoice.IsTotalAmountPayed);

                                //Accounting rows
                                result = PaymentManager.AddPaymentAccountRowsFromCustomerInvoice(entities, paymentRow, paymentMethod, customerInvoice, actorCompanyId);
                                if (!result.Success)
                                    return result;

                                #endregion

                                #region old

                                /*if (paymentRow.IsAdded())
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

                                    //Save
                                    result = PaymentManager.SavePaymentRow(entities, transaction, dto, null, actorCompanyId, userId, false, true);
                                }
                                else
                                {
                                    //Add paymentrow
                                    payment.PaymentRow.Add(paymentRow);

                                    //Save PaymentAccountRows
                                    result = PaymentManager.AddPaymentAccountRowsFromCustomerInvoice(entities, paymentRow, paymentMethod, customerInvoice, actorCompanyId);
                                }*/
                                #endregion

                                if (!result.Success)
                                    return result;
                                else
                                    logText.Add(GetText(7196, "Betalning skapad för faktura med nummer") + ": " + paymentRow.Invoice.InvoiceNr + ".");

                                seqNr++;
                            }

                            if (payment.IsAdded())
                                result = AddEntityItem(entities, payment, "Payment", transaction);
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
                        //result.Value = logText;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            return result;
        }

        private ActionResult ConvertStreamToEntity(BgMaxFile file, int actorCompanyId, List<Origin> origins, int paymentMethodId, bool templateIsSuggestion, ref List<string> logText, int userId, int batchId, int paymentImportId, ImportPaymentType importType = ImportPaymentType.None)
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

                    foreach (Section section in file.Sections)
                    {
                        #region Section

                        if (fileDate == null)
                            fileDate = section.PaymentEnd.PaymentDate;

                        foreach (PaymentGroup paymentGroup in section.PaymentGroups)
                        {
                            foreach (PaymentPost post in paymentGroup.Posts)
                            {
                                switch (post.TransactionCode)
                                {
                                    //case (int)BgMaxTransactionCodes.ReferenceNumberPost1:
                                    case (int)BgMaxTransactionCodes.ReferenceNumberPost2:
                                    case (int)BgMaxTransactionCodes.PaymentPost:
                                    case (int)BgMaxTransactionCodes.PaymentReductionPost:
                                        #region ReferenceNumberPost1, ReferenceNumberPost2, PaymentPost, PaymentReductionPost

                                        #region InvoiceNr

                                        string invoiceNr = string.Empty; 
                                        string alternateInvoiceNr = string.Empty;
                                        switch (post.ReferenceCode)
                                        {
                                            case 0:
                                            case 1:
                                            //break; // Har kommenterat bort break för att läsa in reference på alla rader, se bug #38564 i devops
                                            case 2:
                                            case 3:
                                            case 4:
                                            case 5:
                                                invoiceNr = post.Reference.Trim();
                                                customerInvoice = null;
                                                //invoiceNr = post.BGCPaymentSequenceNumber.Trim();
                                                break;
                                        }

                                        if (!String.IsNullOrEmpty(post.InformationText))
                                            alternateInvoiceNr = post.InformationText;

                                        #endregion

                                        #region Invoice

                                        string invoiceNrToTrim = Regex.Replace(invoiceNr, @"\s+", "");
                                        string parsedInvoiceNr = Regex.Match(invoiceNrToTrim, @"\d+").Value;

                                        string alternateInvoiceNrToTrim = Regex.Replace(alternateInvoiceNr, @"\s+", "");
                                        string alternateParsedInvoiceNr = Regex.Match(alternateInvoiceNrToTrim, @"\d+").Value;

                                        decimal invoiceAmount = Utilities.GetAmount(post.PaymentAmount);

                                        if (string.IsNullOrEmpty(invoiceNr) && invoiceAmount == 0)
                                            break;

                                        Invoice invoice = null;
                                        if (post.ReferenceCode == 2 && !string.IsNullOrEmpty(invoiceNr))
                                        {
                                            invoice = (from i in entities.Invoice
                                                          .Include("Origin")
                                                          .Include("Currency")
                                                          .Include("Actor.Customer")
                                                       where i.OCR == invoiceNr &&
                                                          i.State == (int)SoeEntityState.Active &&
                                                          i.Origin.Type == (int)SoeOriginType.CustomerInvoice &&
                                                          i.Origin.ActorCompanyId == actorCompanyId
                                                       select i).FirstOrDefault();
                                        }
                                        else if (!string.IsNullOrEmpty(parsedInvoiceNr))
                                        {
                                            invoice = (from i in entities.Invoice
                                                              .Include("Origin")
                                                              .Include("Currency")
                                                              .Include("Actor.Customer")
                                                       where i.InvoiceNr == parsedInvoiceNr &&
                                                          i.State == (int)SoeEntityState.Active &&
                                                          i.Origin.Type == (int)SoeOriginType.CustomerInvoice &&
                                                          i.Origin.ActorCompanyId == actorCompanyId
                                                       select i).FirstOrDefault();
                                        }

                                        if (invoice == null && !string.IsNullOrEmpty(parsedInvoiceNr))
                                        {
                                            var invoicesByInvoiceNr = entities.Invoice
                                                               .Include("Origin")
                                                               .Include("Currency")
                                                               .Include("Actor.Customer")
                                                               .Where(x => x.InvoiceNr.Equals(parsedInvoiceNr) &&
                                                                           x.Origin.Type == (int)SoeOriginType.CustomerInvoice &&
                                                                           x.State == (int)SoeEntityState.Active &&
                                                                           x.Origin.ActorCompanyId == actorCompanyId);

                                            bool isMoreThanOne = invoicesByInvoiceNr.Count() > 1;
                                            if (isMoreThanOne)
                                                continue;

                                            invoice = invoicesByInvoiceNr.FirstOrDefault();

                                            if (invoice == null)
                                            {
                                                var invoicesByOCR = entities.Invoice
                                                                   .Include("Origin")
                                                                   .Include("Currency")
                                                                   .Include("Actor.Customer")
                                                                   .Where(x => x.OCR.Equals(parsedInvoiceNr) &&
                                                                               x.Origin.Type == (int)SoeOriginType.CustomerInvoice &&
                                                                               x.State == (int)SoeEntityState.Active &&
                                                                               x.Origin.ActorCompanyId == actorCompanyId);
                                                if (invoicesByOCR.Count() > 1)
                                                    continue;

                                                invoice = invoicesByOCR.FirstOrDefault();
                                            }
                                        }

                                        // Checking information post if invoice not found
                                        if (invoice == null && !string.IsNullOrEmpty(alternateParsedInvoiceNr))
                                        {
                                            invoice = entities.Invoice
                                                               .Include("Origin")
                                                               .Include("Currency")
                                                               .Include("Actor.Customer")
                                                               .Where(x => x.InvoiceNr.Equals(alternateParsedInvoiceNr) &&
                                                                           x.Origin.Type == (int)SoeOriginType.CustomerInvoice &&
                                                                           x.State == (int)SoeEntityState.Active &&
                                                                           x.Origin.ActorCompanyId == actorCompanyId).FirstOrDefault();
                                        }

                                        if (invoice == null)
                                        {
                                            // invoice not found
                                            status = (int)ImportPaymentIOStatus.Unknown;
                                            state = (int)ImportPaymentIOState.Open;
                                        }


                                        //Reverify this condition
                                        if (invoice != null && (invoice.InvoiceNr == invoiceNr || Utilities.GetInvoiceNrFromOCR(invoice.OCR) == invoiceNr))
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
                                                    if (invoice.TotalAmountCurrency < (sum + Utilities.GetAmount(post.PaymentAmount)))
                                                        status = (int)ImportPaymentIOStatus.Rest;
                                                }
                                            }
                                        }

                                        #endregion

                                        #region PaymentImportIO

                                        if (invoice != null)
                                        {
                                            status = (int)ImportPaymentIOStatus.Match;
                                            state = (int)ImportPaymentIOState.Open;

                                            customerInvoice = InvoiceManager.GetCustomerInvoice(invoice.InvoiceId, true, true);
                                            type = invoice.TotalAmount >= 0 ? (int)TermGroup_BillingType.Debit : (int)TermGroup_BillingType.Credit;

                                            bool isPartlyPaid = Utilities.GetAmount(post.PaymentAmount) < invoice.TotalAmount;
                                            if (isPartlyPaid)
                                            {
                                                status = (int)ImportPaymentIOStatus.PartlyPaid;
                                            }

                                            bool isRest = Utilities.GetAmount(post.PaymentAmount) > invoice.TotalAmount;
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
                                            Customer = customerInvoice != null ? StringUtility.Left(customerInvoice.ActorName, 50) : StringUtility.Left(post.Reference, 50),
                                            InvoiceId = invoice != null ? invoice.InvoiceId : 0,
                                            InvoiceNr = invoice != null ? invoice.InvoiceNr : invoiceNr,
                                            InvoiceAmount = invoice != null ? invoice.TotalAmount - invoice.PaidAmount : 0,
                                            RestAmount = invoice != null ? invoice.TotalAmount - invoice.PaidAmount - Utilities.GetAmount(post.PaymentAmount) : 0,
                                            PaidAmount = Utilities.GetAmount(post.PaymentAmount),
                                            Currency = "SEK",
                                            InvoiceDate = invoice != null ? invoice.DueDate : null,
                                            PaidDate = section.PaymentEnd.PaymentDate,
                                            MatchCodeId = 0,
                                            Status = status,
                                            State = state,
                                            ImportType = (int)importType,
                                        };

                                        totalInvoiceAmount = totalInvoiceAmount + Utilities.GetAmount(post.PaymentAmount);

                                        // Check for duplicates
                                        if (paymentImportIO.CustomerId > 0 && paymentImportIO.InvoiceId > 0 && PaymentImportIOToAdd.Any(p => p.CustomerId == paymentImportIO.CustomerId && p.InvoiceId == paymentImportIO.InvoiceId && p.PaidAmount == paymentImportIO.PaidAmount && p.PaidDate == paymentImportIO.PaidDate))
                                        {
                                            var totalPaymentAmounts = paymentImportIO.PaidAmount + PaymentImportIOToAdd.Where(p => p.CustomerId == paymentImportIO.CustomerId && p.InvoiceId == paymentImportIO.InvoiceId && p.PaidAmount == paymentImportIO.PaidAmount && p.PaidDate == paymentImportIO.PaidDate).Sum(p => p.PaidAmount);
                                            if ((invoice.BillingType == (int)TermGroup_BillingType.Credit && ((invoice.TotalAmount - invoice.PaidAmount) - totalPaymentAmounts) > 0) || (invoice.BillingType == (int)TermGroup_BillingType.Debit && ((invoice.TotalAmount - invoice.PaidAmount) - totalPaymentAmounts) < 0))
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
                                        }

                                        PaymentImportIOToAdd.Add(paymentImportIO);

                                        logText.Add(paymentImportIO.Customer);

                                        #endregion

                                        #endregion
                                        break;
                                }
                            }
                        }

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

                                //result = SaveChanges(entities, transaction);
                                if (result.Success)
                                {
                                    //ActionResult result2 = new ActionResult();
                                    PaymentImport paymentImport = PaymentManager.GetPaymentImport(entities, paymentImportId, actorCompanyId);

                                    paymentImport.TotalAmount = paymentImport.TotalAmount + paymentIO.PaidAmount.Value;
                                    paymentImport.NumberOfPayments = numberOfPayments++;
                                    paymentImport.ImportDate = fileDate.HasValue ? fileDate.Value : paymentImport.ImportDate;

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

        private bool IsPaymentPost(IPaymentPost item)
        {
            return item is PaymentPost;
        }

        #endregion
    }
}
