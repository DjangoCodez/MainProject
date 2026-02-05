using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Transactions;

namespace SoftOne.Soe.Business.Core.PaymentIO.Lb
{
    public class LbManager : PaymentIOManager
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private LbFile lbFile;

        SettingManager sm;
        #endregion

        #region Ctor

        public LbManager(ParameterObject parameterObject)
            : base(parameterObject)
        {
            sm = new SettingManager(parameterObject);
        }

        #endregion

        #region Import

        public ActionResult Import(StreamReader sr, int actorCompanyId, string fileName, List<Payment> payments, Dictionary<string, decimal> notFoundinFile, int paymentMethodId, ref List<string> log, int userId = 0, int batchId = 0, int paymentImportId = 0, ImportPaymentType importType = ImportPaymentType.None)
        {
            string paymentDate = "";

            var result = new ActionResult(true);
            result.ErrorMessage = GetText(8076, "Filimport genomförd");

            sr.DiscardBufferedData();
            sr.BaseStream.Position = 0;

            try
            {
                lbFile = new LbFile();

                var isSection = false;
                var paymentNr = -1;
                var createPost = false;
                ILbSection section = null;
                LbDomesticPostGroup group = null;

                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (string.IsNullOrEmpty(line))
                        continue;

                    ILbPost post = null;
                    var isSectionEnd = false;

                    line = Utilities.AddPadding(line, Utilities.BGC_LINE_MAX_LENGTH);
                    var transactionCode = Utilities.GetLBTransactionCode(line);
                    if (String.IsNullOrEmpty(transactionCode))
                        continue;

                    if (transactionCode != Utilities.TRANSACTION_CODE_CORRECTION)
                    {
                        var tc = Convert.ToInt32(transactionCode);

                        switch (tc)
                        {
                            case (int)LbTransactionCodeDomestic.PaymentPost:
                            case (int)LbTransactionCodeDomestic.ReductionPost:
                            case (int)LbTransactionCodeDomestic.CreditInvoiceObservationPost:
                            case (int)LbTransactionCodeDomestic.CreditInvoiceObservationCompletePost:
                            case (int)LbTransactionCodeDomestic.PlusgiroPost:
                                post = new LbDomesticPaymentPost(line);
                                ((LbDomesticPaymentPost)post).Date = paymentDate;
                                createPost = true;
                                break;
                            case (int)LbTransactionCodeDomestic.CreditInvoiceRestPost:
                                post = new LbDomesticCreditInvoiceRestPost(line);
                                createPost = true;
                                break;
                            case (int)LbTransactionCodeDomestic.NamePost:
                                post = new LbDomesticNamePost(line);
                                createPost = true;
                                break;
                            case (int)LbTransactionCodeDomestic.AddressPost:
                                createPost = true;
                                post = new LbDomesticAddressPost(line);
                                break;
                            case (int)LbTransactionCodeDomestic.AccountPost:
                                post = new LbDomesticAccountPost(line);
                                createPost = true;
                                break;
                            case (int)LbTransactionCodeDomestic.OpeningPost:
                                section = new LbPaymentSection();
                                post = new LbDomesticOpeningPost(line);
                                paymentDate = ((LbDomesticOpeningPost)post).PaymentDate;
                                if (string.IsNullOrWhiteSpace(paymentDate))
                                {
                                    result.ErrorMessage = GetText(8826, "Betaldatum saknas i filen");
                                    result.Success = false;
                                    return result;
                                }
                                isSection = true;
                                break;
                            case (int)LbTransactionCodeDomestic.SummaryPost:
                                post = new LbDomesticSummaryPost(line);
                                isSectionEnd = true;
                                break;
                            case (int)LbTransactionCodeDomestic.CommentPost: //parsing error on bgc's side
                                post = new LbDomesticCommentPost(line);
                                break;
                        }

                        if (createPost && post != null && !isSectionEnd)
                        {
                            if (paymentNr == -1)
                                paymentNr = ((ILbPostGroupItem)post).BgCode;

                            if (((ILbPostGroupItem)post).BgCode == paymentNr)
                            {
                                if (group != null)
                                {
                                    group.Posts.Add(post);
                                }
                                else
                                {
                                    group = new LbDomesticPostGroup(paymentNr);
                                    group.Posts.Add(post);
                                    post = null;
                                }
                            }
                            else
                            {
                                if (section != null) section.Posts.Add(group);
                                paymentNr = ((ILbPostGroupItem)post).BgCode;
                                group = new LbDomesticPostGroup(paymentNr);
                                group.Posts.Add(post);
                                post = null;
                            }
                        }

                        if (isSection && post != null && group == null)
                        {
                            section.Posts.Add(post);
                            post = null;
                        }
                        else if (!isSection && post != null && group == null)
                        {
                            lbFile.contents.Add(post);
                            post = null;
                        }

                        if (isSectionEnd && section != null)
                        {
                            if (group != null)
                                section.Posts.Add(group);

                            if (post != null)
                                section.Posts.Add(post);

                            lbFile.contents.Add(section);
                            isSection = false;
                            section = null;
                            group = null;
                            createPost = false;
                            paymentNr = -1;
                        }
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
                if (!lbFile.IsValid())
                {
                    result.Success = false;
                    result.ErrorNumber = (int)ActionResultSave.PaymentFileLbFailed;
                    result.ErrorMessage = GetText(8078, "Filens struktur gick inte att läsa");
                    return result;
                }

                if (userId == 0)
                    result = ConvertLbFileToEntity(lbFile, payments, paymentMethodId, fileName, actorCompanyId, ref log);
                else
                    result = ConvertStreamToEntity(lbFile, payments, paymentMethodId, actorCompanyId, ref log, userId, batchId, paymentImportId, importType);


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

        private ActionResult ConvertLbFileToEntity(LbFile file, List<Payment> payments, int paymentMethodId, string fileName, int actorCompanyId, ref List<string> log)
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
                    int accountYearId = SettingManager.GetIntSetting(entities, SettingMainType.UserAndCompany, (int)UserSettingType.AccountingAccountYear, base.UserId, actorCompanyId, 0);

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

                    var lbCurrencyCode = string.Empty;
                    DateTime? fileDate = null;
                    foreach (ILbFileContent fileContent in file.contents)
                    {
                        #region FileContent

                        if (fileContent is LbPaymentSection)
                        {
                            foreach (ILbFileContent sectionContent in ((LbPaymentSection)fileContent).Posts)
                            {
                                if (sectionContent.GetType() == typeof(LbDomesticOpeningPost))
                                {
                                    if (((LbDomesticOpeningPost)sectionContent).PaymentDate.Trim().Length == 6)
                                        ((LbDomesticOpeningPost)sectionContent).PaymentDate = DateTime.Now.Year.ToString().Substring(0, 2) + ((LbDomesticOpeningPost)sectionContent).PaymentDate;

                                    var d = DateTime.ParseExact(((LbDomesticOpeningPost)sectionContent).PaymentDate, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);

                                    if (d != null)
                                        fileDate = d;
                                }


                                if (sectionContent is ILbPostGroup)
                                {
                                    foreach (ILbPostGroupItem postGroupItem in ((ILbPostGroup)sectionContent).Posts)
                                    {
                                        switch (postGroupItem.TransactionCode)
                                        {
                                            case (int)LbTransactionCodeDomestic.OpeningPost:
                                                lbCurrencyCode = ((LbDomesticOpeningPost)postGroupItem).CurrencyCode;
                                                break;
                                            case (int)LbTransactionCodeDomestic.PaymentPost:
                                            case (int)LbTransactionCodeDomestic.ReductionPost:
                                            case (int)LbTransactionCodeDomestic.CreditInvoiceObservationCompletePost:
                                            case (int)LbTransactionCodeDomestic.CreditInvoiceObservationPost:
                                            case (int)LbTransactionCodeDomestic.PlusgiroPost:
                                                bool found = false;
                                                var post = (LbDomesticPaymentPost)postGroupItem;

                                                foreach (Payment payment in payments)
                                                {
                                                    foreach (PaymentRow row in payment.PaymentRow)
                                                    {
                                                        found = false;

                                                        if (Utilities.GetPaymentType(row.SysPaymentTypeId) != TermGroup_SysPaymentType.BG && Utilities.GetPaymentType(row.SysPaymentTypeId) != TermGroup_SysPaymentType.PG)
                                                            continue;

                                                        if (Utilities.PaymentNumber(row.PaymentNr) == post.BgCode && (row.Invoice.OCR == post.InvoiceNumber.Trim() || row.Invoice.InvoiceNr == post.InvoiceNumber.Trim()))
                                                        {
                                                            found = true;

                                                            if (row.Status == (int)SoePaymentStatus.None || row.Status == (int)SoePaymentStatus.Pending)
                                                            {
                                                                //Set status on row
                                                                row.Status = status;

                                                                //Set pay date
                                                                DateTime d;
                                                                if (DateTime.TryParse(post.Date, out d))
                                                                    row.PayDate = d;
                                                                else if (DateTime.TryParseExact(post.Date, "yyMMdd", CultureInfo.InstalledUICulture, DateTimeStyles.None, out d))
                                                                    row.PayDate = d;

                                                                //Set currency amounts
                                                                CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, row);

                                                                //store row
                                                                updatedRows.Add(row.PaymentRowId, row);

                                                                log.Add(GetText(7078, "Rad med fakturanr") + " " + post.InvoiceNumber + " " + GetText(7079, "matchades med betalning med sekvensnummer") + " " + row.SeqNr);
                                                            }
                                                            else
                                                            {
                                                                log.Add(GetText(7080, "Betalning med sekvensnummer") + " " + row.SeqNr + " " + GetText(7081, "för faktura med nr") + " " + row.Invoice.InvoiceNr + " " + GetText(7082, "är redan verifierad"));
                                                            }

                                                            break;
                                                        }

                                                        #region quarantine
                                                        /*else//create payment                                                    
                                                        {
                                                            DateTime payDate = Utilities.GetDate(post.Date);
                                                            var sysCurrencyId = 0;
                                                            if (lbCurrencyCode == Utilities.CURRENCY_EUR)
                                                                sysCurrencyId = (int)LbCurrency.EUR;
                                                            else if (lbCurrencyCode == Utilities.CURRENCY_USD)
                                                                sysCurrencyId = (int)LbCurrency.EUR;
                                                            else if (lbCurrencyCode == Utilities.CURRENCY_SEK || lbCurrencyCode == Utilities.CURRENCY_SEK_ALTERNATIVE)
                                                                sysCurrencyId = (int)LbCurrency.SEK;

                                                            CompCurrency currency = CountryCurrencyManager.GetCompCurrency(entities, sysCurrencyId, actorCompanyId);
                                                            if (currency != null)
                                                                rate = currency.RateToBase != null ? currency.RateToBase : 1;

                                                            Invoice invoice = new Invoice()
                                                            {
                                                                Type = (int)SoeInvoiceType.SupplierInvoice,
                                                                InvoiceNr = post.InvoiceNumber,
                                                                PaymentNr = post.BgCode.ToString(),
                                                                InvoiceDate = payDate,
                                                                DueDate = payDate,
                                                                VoucherDate = payDate,
                                                                CurrencyDate = payDate,
                                                                TotalAmount = Utilities.GetAmount(post.Amount),
                                                                SysPaymentTypeId = (int)TermGroup_SysPaymentType.BG,
                                                            };
                                                            invoice.BillingType = post.TransactionCode == (int)LbTransactionCodeDomestic.PaymentPost ? (int)SoeBillingType.Debit : (int)SoeBillingType.Credit;
                                                            invoice.OCR = string.Empty;
                                                            invoice.ReferenceYour = string.Empty;
                                                            invoice.CurrencyRate = rate;
                                                            invoice.TotalAmountCurrency = invoice.TotalAmount / rate;
                                                            invoice.OCR = string.Empty;
                                                            invoice.ReferenceOur = "payment from file, not matched";
                                                            invoice.VATAmount = 0;
                                                            invoice.VatType = 0;

                                                            //TODO: verify that invoice is saved by save in paymentmanager
                                                        }*/
                                                        #endregion
                                                    }

                                                    #region quarantine
                                                    /*if (!found)
                                                    {
                                                        DateTime payDate = Utilities.GetDate(post.Date);
                                                        var sysCurrencyId = 0;
                                                        if (lbCurrencyCode == Utilities.CURRENCY_EUR)
                                                            sysCurrencyId = (int)LbCurrency.EUR;
                                                        else if (lbCurrencyCode == Utilities.CURRENCY_USD)
                                                            sysCurrencyId = (int)LbCurrency.EUR;
                                                        else if (lbCurrencyCode == Utilities.CURRENCY_SEK || lbCurrencyCode == Utilities.CURRENCY_SEK_ALTERNATIVE)
                                                            sysCurrencyId = (int)LbCurrency.SEK;

                                                        CompCurrency currency = CountryCurrencyManager.GetCompCurrency(entities, sysCurrencyId, actorCompanyId);
                                                        if (currency != null)
                                                            rate = currency.RateToBase != null ? currency.RateToBase : 1;

                                                        Invoice invoice = new Invoice()
                                                        {
                                                            Type = (int)SoeInvoiceType.SupplierInvoice,
                                                            InvoiceNr = post.InvoiceNumber,
                                                            PaymentNr = post.BgCode.ToString(),
                                                            InvoiceDate = payDate,
                                                            DueDate = payDate,
                                                            VoucherDate = payDate,
                                                            CurrencyDate = payDate,
                                                            TotalAmount = Utilities.GetAmount(post.Amount),
                                                            SysPaymentTypeId = (int)TermGroup_SysPaymentType.BG,
                                                        };
                                                        invoice.BillingType = post.TransactionCode == (int)LbTransactionCodeDomestic.PaymentPost ? (int)SoeBillingType.Debit : (int)SoeBillingType.Credit;
                                                        invoice.OCR = string.Empty;
                                                        invoice.ReferenceYour = string.Empty;
                                                        invoice.CurrencyRate = rate;
                                                        invoice.TotalAmountCurrency = invoice.TotalAmount / rate;
                                                        invoice.OCR = string.Empty;
                                                        invoice.ReferenceOur = "payment from file, not matched";
                                                        invoice.VATAmount = 0;
                                                        invoice.VatType = 0;

                                                        //TODO: verify that invoice is saved by save in paymentmanager
                                                    }*/
                                                    #endregion

                                                    if (found)
                                                    {
                                                        break;
                                                    }
                                                }

                                                if (!found)
                                                {
                                                    log.Add(GetText(7083, "Kunde inte hitta en matchande betalning till rad") + ": " + post.ToString());
                                                }
                                                break;
                                            case (int)LbTransactionCodeDomestic.CommentPost:
                                                var error = (LbDomesticCommentPost)postGroupItem;
                                                if (!String.IsNullOrEmpty(error.ErrorCode))
                                                {
                                                    status = GetSysErrorId(error.ErrorCode);
                                                    foreach (var row in updatedRows)
                                                    {
                                                        row.Value.Status = status;
                                                    }
                                                }
                                                break;
                                            case (int)LbTransactionCodeDomestic.NamePost:
                                            case (int)LbTransactionCodeDomestic.AddressPost:
                                            case (int)LbTransactionCodeDomestic.AccountPost:
                                                break;
                                        }
                                    }
                                }

                                #region deprecated
                                //else if (sectionContent is ILbPost)
                                //{
                                //    switch (((ILbPost)sectionContent).TransactionCode)
                                //    {
                                //        case (int)LbTransactionCodeDomestic.OpeningPost:
                                //            //var opening = ((LbDomesticOpeningPost)sectionContent);
                                //            //currencyCode = opening.CurrencyCode;
                                //            //isSection = true;
                                //            break;
                                //        case (int)LbTransactionCodeDomestic.SummaryPost:
                                //            //var summary = ((LbDomesticSummaryPost)sectionContent);
                                //            //if (summary.NegateAmount == "-")
                                //            //    negateAmount = true;
                                //            //isSectionEnd = true;
                                //            //isSection = false;
                                //            break;
                                //    }
                                //}
                                #endregion
                            }
                        }
                        else if (fileContent is LbCorrectionSection)
                        {
                            var correctionSection = ((LbCorrectionSection)fileContent);
                            #region Corrections
                            foreach (var lbFileContentItem in correctionSection.Posts)
                            {
                                var correctionPost = ((LbCorrectionPost)lbFileContentItem);
                                switch (correctionPost.CorrectionCode)
                                {
                                    case 3:
                                        //03 Makulera alla kreditfakturor BGC
                                        foreach (Payment payment in payments)
                                        {
                                            foreach (PaymentRow row in payment.PaymentRow)
                                            {
                                                if (TermGroup_SysPaymentType.BG != Utilities.GetPaymentType(row.SysPaymentTypeId))
                                                    continue;

                                                ChangeEntityState(row.Invoice, SoeEntityState.Deleted);//new status?
                                                updatedRows.Add(row.PaymentRowId, row);
                                            }
                                        }
                                        break;
                                    case 11:
                                        //11 Makulera alla fakturor med angiven betalningsdag (gäller även fakturor i kronor till plusgironr)
                                        foreach (Payment payment in payments)
                                        {
                                            foreach (PaymentRow row in payment.PaymentRow)
                                            {
                                                if (TermGroup_SysPaymentType.BG != Utilities.GetPaymentType(row.SysPaymentTypeId))
                                                    continue;
                                                if (Utilities.DateCompareConverter(correctionPost.Date, row.Invoice.DueDate.Value))
                                                {
                                                    ChangeEntityState(row.Invoice, SoeEntityState.Deleted);
                                                    updatedRows.Add(row.PaymentRowId, row);
                                                }
                                            }
                                        }
                                        break;
                                    case 12:
                                        //12 makulera alla fakturor till ett plusgironummer
                                        foreach (Payment payment in payments)
                                        {
                                            foreach (PaymentRow row in payment.PaymentRow)
                                            {
                                                if (TermGroup_SysPaymentType.PG != Utilities.GetPaymentType(row.SysPaymentTypeId))
                                                    continue;

                                                if (correctionPost.ReceiverBgCode != Utilities.PaymentNumber(row.PaymentNr))
                                                    continue;

                                                ChangeEntityState(row.Invoice, SoeEntityState.Deleted);
                                                updatedRows.Add(row.PaymentRowId, row);
                                            }
                                        }
                                        break;
                                    case 13:
                                        //13 Makulera alla fakturor till ett mottagarnr med en angiven betalningsdag ej pg
                                        foreach (Payment payment in payments)
                                        {
                                            foreach (PaymentRow row in payment.PaymentRow)
                                            {
                                                if (TermGroup_SysPaymentType.BG != Utilities.GetPaymentType(row.SysPaymentTypeId))
                                                    continue;
                                                if (row.PaymentNr != (correctionPost.ReceiverBgCode.ToString()).Trim())
                                                    continue;
                                                if (row.Invoice.Actor.Supplier.OrgNr != (correctionPost.SenderCustomerNumber.ToString()).Trim())
                                                    continue;
                                                if (Utilities.DateCompareConverter(correctionPost.Date, row.Invoice.DueDate.Value))
                                                {
                                                    ChangeEntityState(row.Invoice, SoeEntityState.Deleted);
                                                    updatedRows.Add(row.PaymentRowId, row);
                                                }
                                            }
                                        }
                                        break;
                                    case 14:
                                        //14 Enstaka faktura till ett plusgironummer med angiven betalningsdag
                                        foreach (Payment payment in payments)
                                        {
                                            foreach (PaymentRow row in payment.PaymentRow)
                                            {
                                                if (TermGroup_SysPaymentType.PG != Utilities.GetPaymentType(row.SysPaymentTypeId))
                                                    continue;
                                                if (correctionPost.ReceiverBgCode.ToString().Trim() != row.PaymentNr)
                                                    continue;
                                                if (Utilities.DateCompareConverter(correctionPost.Date, row.Invoice.DueDate.Value))
                                                {
                                                    ChangeEntityState(row.Invoice, SoeEntityState.Deleted);
                                                    updatedRows.Add(row.PaymentRowId, row);
                                                }
                                            }
                                        }
                                        break;
                                    case 16://16 Makulera lön
                                        break;
                                    case 21:
                                        //21 Ändra betalningsdag för alla fakturor (gäller även fakturor i kronor till plusgironr)
                                        foreach (Payment payment in payments)
                                        {
                                            foreach (PaymentRow row in payment.PaymentRow)
                                            {
                                                if (TermGroup_SysPaymentType.PG == Utilities.GetPaymentType(row.SysPaymentTypeId))
                                                {
                                                    if (row.Invoice.Currency.SysCurrencyId == (int)LbCurrency.SEK)
                                                    {
                                                        row.Invoice.DueDate = Utilities.GetDateTime(TermGroup_SysPaymentMethod.LB, correctionPost.Date);
                                                        updatedRows.Add(row.PaymentRowId, row);
                                                    }
                                                }
                                                else if (TermGroup_SysPaymentType.BG == Utilities.GetPaymentType(row.SysPaymentTypeId))
                                                {
                                                    row.Invoice.DueDate = Utilities.GetDateTime(TermGroup_SysPaymentMethod.LB, correctionPost.Date);
                                                    updatedRows.Add(row.PaymentRowId, row);
                                                }
                                            }
                                        }
                                        break;
                                    case 22:
                                        //22 Ändra angiven betalningsdag till ny betalningsdag för alla fakturor inklusive löner. BGC
                                        foreach (Payment payment in payments)
                                        {
                                            foreach (PaymentRow row in payment.PaymentRow)
                                            {
                                                if (TermGroup_SysPaymentType.BG != Utilities.GetPaymentType(row.SysPaymentTypeId))
                                                    continue;
                                                row.Invoice.DueDate = Utilities.GetDateTime(TermGroup_SysPaymentMethod.LB, correctionPost.Date);
                                                updatedRows.Add(row.PaymentRowId, row);
                                            }
                                        }
                                        break;
                                    case 23:
                                        //23 Betalningsdag för alla fakturor till ett plusgironummer
                                        foreach (Payment payment in payments)
                                        {
                                            foreach (PaymentRow row in payment.PaymentRow)
                                            {
                                                if (TermGroup_SysPaymentType.PG != Utilities.GetPaymentType(row.SysPaymentTypeId))
                                                    continue;
                                                if (row.PaymentNr != correctionPost.ReceiverBgCode.ToString().Trim())
                                                    continue;
                                                row.Invoice.DueDate = Utilities.GetDateTime(TermGroup_SysPaymentMethod.LB, correctionPost.Date);
                                                updatedRows.Add(row.PaymentRowId, row);
                                            }
                                        }
                                        break;
                                    case 24:
                                        //24 Angiven betalningsdag till ny betalningsdag för alla fakturor till ett plusgironummer
                                        foreach (Payment payment in payments)
                                        {
                                            foreach (PaymentRow row in payment.PaymentRow)
                                            {
                                                if (TermGroup_SysPaymentType.PG != Utilities.GetPaymentType(row.SysPaymentTypeId))
                                                    continue;
                                                if (row.PaymentNr != correctionPost.ReceiverBgCode.ToString().Trim())
                                                    continue;
                                                row.Invoice.DueDate = Utilities.GetDateTime(TermGroup_SysPaymentMethod.LB, correctionPost.Date);
                                                updatedRows.Add(row.PaymentRowId, row);
                                            }
                                        }
                                        break;
                                    case 25:
                                        //25 Enstaka faktura till ett plusgironummer med angiven betalningsdag
                                        foreach (Payment payment in payments)
                                        {
                                            foreach (PaymentRow row in payment.PaymentRow)
                                            {
                                                if (TermGroup_SysPaymentType.PG != Utilities.GetPaymentType(row.SysPaymentTypeId))
                                                    continue;
                                                if (row.PaymentNr != correctionPost.ReceiverBgCode.ToString().Trim())
                                                    continue;
                                                row.Invoice.DueDate = Utilities.GetDateTime(TermGroup_SysPaymentMethod.LB, correctionPost.Date);
                                                updatedRows.Add(row.PaymentRowId, row);

                                            }
                                        }
                                        break;
                                    case 31:
                                        //31 Makulera alla kreditfakturor till ett mottagarnr
                                        foreach (Payment payment in payments)
                                        {
                                            foreach (PaymentRow row in payment.PaymentRow)
                                            {
                                                if (TermGroup_SysPaymentType.BG != Utilities.GetPaymentType(row.SysPaymentTypeId))
                                                    continue;
                                                if (row.Invoice.Type != (int)TermGroup_BillingType.Credit)
                                                    continue;
                                                if (row.Invoice.Actor.Supplier.OrgNr != correctionPost.SenderCustomerNumber.ToString().Trim())
                                                    continue;

                                                ChangeEntityState(row.Invoice, SoeEntityState.Deleted);
                                                updatedRows.Add(row.PaymentRowId, row);
                                            }
                                        }
                                        break;
                                    case 33:
                                        //33 Makulera alla kreditfakturor till ett mottagarnr med angiven sista bevakningsdag
                                        foreach (Payment payment in payments)
                                        {
                                            foreach (var row in payment.PaymentRow)
                                            {
                                                if (TermGroup_SysPaymentType.BG != Utilities.GetPaymentType(row.SysPaymentTypeId))
                                                    continue;
                                                if (row.Invoice.Type != (int)TermGroup_BillingType.Credit)
                                                    continue;
                                                if (row.Invoice.Actor.Supplier.OrgNr != correctionPost.SenderCustomerNumber.ToString().Trim())
                                                    continue;
                                                if (Utilities.DateCompareConverter(correctionPost.Date.Trim(), row.Invoice.DueDate.Value))
                                                {
                                                    ChangeEntityState(row.Invoice, SoeEntityState.Deleted);
                                                    updatedRows.Add(row.PaymentRowId, row);
                                                }
                                            }
                                        }
                                        break;
                                    case 34:
                                        //34 Makulera enstaka kreditfaktura
                                        foreach (Payment payment in payments)
                                        {
                                            foreach (PaymentRow row in payment.PaymentRow)
                                            {
                                                if (TermGroup_SysPaymentType.BG != Utilities.GetPaymentType(row.SysPaymentTypeId))
                                                    continue;
                                                if (row.Invoice.Type != (int)TermGroup_BillingType.Credit)
                                                    continue;
                                                if (row.PaymentNr != correctionPost.ReceiverBgCode.ToString().Trim())
                                                    continue;

                                                ChangeEntityState(row.Invoice, SoeEntityState.Deleted);
                                                updatedRows.Add(row.PaymentRowId, row);
                                            }
                                        }
                                        break;
                                    case 36:
                                        //36 Makulera alla kreditfakturor med angiven sista bevakningsdag
                                        foreach (Payment payment in payments)
                                        {
                                            foreach (PaymentRow row in payment.PaymentRow)
                                            {
                                                if (TermGroup_SysPaymentType.BG != Utilities.GetPaymentType(row.SysPaymentTypeId))
                                                    continue;
                                                if (row.Invoice.Type != (int)TermGroup_BillingType.Credit)
                                                    continue;
                                                if (!Utilities.DateCompareConverter(correctionPost.Date.Trim(), row.Invoice.DueDate.Value))
                                                    continue;

                                                ChangeEntityState(row.Invoice, SoeEntityState.Deleted);
                                                updatedRows.Add(row.PaymentRowId, row);
                                            }
                                        }
                                        break;
                                    case 42:
                                        //42 Ändra angiven sista bevakningsdag till ny sista bevakningsdag för alla kreditfakturor till viss mottagare
                                        foreach (Payment payment in payments)
                                        {
                                            foreach (PaymentRow row in payment.PaymentRow)
                                            {
                                                if (TermGroup_SysPaymentType.BG != Utilities.GetPaymentType(row.SysPaymentTypeId))
                                                    continue;
                                                if (row.Invoice.Type != (int)TermGroup_BillingType.Credit)
                                                    continue;
                                                if (row.Invoice.Actor.Supplier.OrgNr != correctionPost.SenderCustomerNumber.ToString().Trim())
                                                    continue;
                                                if (row.PaymentNr != correctionPost.ReceiverBgCode.ToString().Trim())
                                                    continue;

                                                row.Invoice.DueDate = Utilities.GetDateTime(TermGroup_SysPaymentMethod.LB, correctionPost.Date);
                                                updatedRows.Add(row.PaymentRowId, row);
                                            }
                                        }
                                        break;
                                    case 44:
                                        //44 Ändra angiven sista bevakningsdag för enstaka kreditfaktura
                                        foreach (Payment payment in payments)
                                        {
                                            foreach (PaymentRow row in payment.PaymentRow)
                                            {
                                                if (TermGroup_SysPaymentType.BG != Utilities.GetPaymentType(row.SysPaymentTypeId))
                                                    continue;
                                                if (row.Invoice.Type != (int)TermGroup_BillingType.Credit)
                                                    continue;
                                                if (row.PaymentNr != correctionPost.ReceiverBgCode.ToString().Trim())
                                                    continue;

                                                row.Invoice.DueDate = Utilities.GetDateTime(TermGroup_SysPaymentMethod.LB, correctionPost.Date);
                                                updatedRows.Add(row.PaymentRowId, row);
                                            }
                                        }
                                        break;
                                    case 46:
                                        //46 Ändra angiven sista bevakningsdag till ny sista bevakningsdag för alla kreditfakturor
                                        foreach (Payment payment in payments)
                                        {
                                            foreach (PaymentRow row in payment.PaymentRow)
                                            {
                                                if (TermGroup_SysPaymentType.BG != Utilities.GetPaymentType(row.SysPaymentTypeId))
                                                    continue;
                                                if (row.Invoice.Type != (int)TermGroup_BillingType.Credit)
                                                    continue;

                                                row.Invoice.DueDate = Utilities.GetDateTime(TermGroup_SysPaymentMethod.LB, correctionPost.Date);
                                                updatedRows.Add(row.PaymentRowId, row);
                                            }
                                        }
                                        break;
                                    case 48:
                                        //48 Ändra sista bevakningsdag för alla kreditfakturor
                                        foreach (Payment payment in payments)
                                        {
                                            foreach (PaymentRow row in payment.PaymentRow)
                                            {
                                                if (TermGroup_SysPaymentType.BG != Utilities.GetPaymentType(row.SysPaymentTypeId))
                                                    continue;
                                                if (row.Invoice.Type != (int)TermGroup_BillingType.Credit)
                                                    continue;

                                                row.Invoice.DueDate = Utilities.GetDateTime(TermGroup_SysPaymentMethod.LB, correctionPost.Date);
                                                updatedRows.Add(row.PaymentRowId, row);
                                            }
                                        }
                                        break;
                                }
                            }
                            #endregion
                        }

                        #endregion

                        using (var transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                        {
                            foreach (var item in updatedRows)
                            {
                                #region PaymentRow

                                if (!result.Success)
                                    continue;

                                #region quarantine
                                /*var paymentRow = (PaymentRow)item.Value;

                                //won't get ahold of customerinvoice here...
                                SupplierInvoice supplierInvoice = InvoiceManager.GetSupplierInvoice(entities, paymentRow.Invoice.InvoiceId, false, true, false, false, true, true);

                                //Create not matching registered payments
                                if (paymentRow.Invoice.IsAdded())
                                {
                                    Origin origin = new Origin()
                                    {
                                        OriginId = ((PaymentRow)item.Value).PaymentRowId,
                                        Description = "created since no match was found",
                                        Status = (int)SoeOriginStatus.Origin
                                    };

                                    int actorCustomerId = paymentRow.Invoice.Actor.Customer.ActorCustomerId;
                                    int sysCurrencyId = paymentRow.Invoice.Currency.SysCurrencyId;

                                    //Save
                                    result = PaymentManager.SavePaymentRow(entities, transaction, origin, payments, paymentRow, paymentRow.Invoice, null, actorCustomerId, voucherSeries.VoucherSeriesId, accountYearId, paymentMethodId, sysCurrencyId, actorCompanyId, actorCompanyId, false, true);

                                    //Save PaymentAccountRows
                                    PaymentManager.AddPaymentAccountRowsFromSupplierInvoice(paymentRow, supplierInvoice, paymentMethod);
                                }

                                if (!result.Success)
                                    return result;*/
                                #endregion

                                //Update PaymentRow(s) and apply paymentImport entity
                                result = AddPaymentImport(entities, fileName, item.Value, transaction);

                                #endregion
                            }

                            if (result.Success && updatedRows.Count > 0)
                            {
                                List<PaymentRow> paymentRows = new List<PaymentRow>();

                                foreach (PaymentRow row in updatedRows.Select(p => p.Value))
                                {
                                    paymentRows.Add(PaymentManager.GetPaymentRow(entities, row.PaymentRowId, true, true, true));
                                }

                                // Create voucher for payments
                                VoucherManager vm = new VoucherManager(parameterObject);
                                result = vm.SaveVoucherFromPayment(entities, ConfigSettings.TRANSACTIONSCOPEOPTION_ATTACH, paymentRows, SoeOriginType.SupplierPayment, false, accountYearId, actorCompanyId, true, fileDate);
                            }

                            //Commit transaction
                            if (result.Success)
                                transaction.Complete();
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

        private ActionResult ConvertStreamToEntity(LbFile file, List<Payment> payments, int paymentMethodId, int actorCompanyId, ref List<string> log, int userId = 0, int batchId = 0, int paymentImportId = 0, ImportPaymentType importType = ImportPaymentType.None)
        {
            PaymentManager pm = new PaymentManager(this.parameterObject);

            var result = new ActionResult(true);
            var updatedRows = new Dictionary<int, PaymentRow>();
            var status = (int)SoePaymentStatus.Verified;

            var paymentImportIOToAdd = new List<PaymentImportIO>();
            decimal totalInvoiceAmount = 0;
            int numberOfPayments = 1;

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

                    var lbCurrencyCode = string.Empty;
                    DateTime? fileDate = null;
                    foreach (ILbFileContent fileContent in file.contents)
                    {
                        updatedRows.Clear();
                        paymentImportIOToAdd.Clear();

                        #region FileContent

                        if (fileContent is LbPaymentSection)
                        {
                            foreach (ILbFileContent sectionContent in ((LbPaymentSection)fileContent).Posts)
                            {
                                if (sectionContent.GetType() == typeof(LbDomesticOpeningPost))
                                {
                                    if (((LbDomesticOpeningPost)sectionContent).PaymentDate.Trim().Length == 6)
                                        ((LbDomesticOpeningPost)sectionContent).PaymentDate = DateTime.Now.Year.ToString().Substring(0, 2) + ((LbDomesticOpeningPost)sectionContent).PaymentDate;

                                    var d = DateTime.ParseExact(((LbDomesticOpeningPost)sectionContent).PaymentDate, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);

                                    if (d != null)
                                        fileDate = d;
                                }

                                if (sectionContent is ILbPostGroup)
                                {
                                    foreach (ILbPostGroupItem postGroupItem in ((ILbPostGroup)sectionContent).Posts)
                                    {
                                        switch (postGroupItem.TransactionCode)
                                        {
                                            case (int)LbTransactionCodeDomestic.OpeningPost:
                                                lbCurrencyCode = ((LbDomesticOpeningPost)postGroupItem).CurrencyCode;
                                                break;
                                            case (int)LbTransactionCodeDomestic.PaymentPost:
                                            case (int)LbTransactionCodeDomestic.ReductionPost:
                                            case (int)LbTransactionCodeDomestic.CreditInvoiceObservationPost:
                                            case (int)LbTransactionCodeDomestic.CreditInvoiceObservationCompletePost:
                                            case (int)LbTransactionCodeDomestic.PlusgiroPost:
                                                bool found = false;
                                                var post = (LbDomesticPaymentPost)postGroupItem;
                                                var postInvoiceNumber = post.InvoiceNumber.Trim();
                                                var postInvoiceSeqNr = post.GetInvoiceSeqNrFromInformation().GetValueOrDefault();

                                                foreach (Payment payment in payments)
                                                {
                                                    foreach (PaymentRow row in payment.PaymentRow)
                                                    {
                                                        var paymentType = Utilities.GetPaymentType(row.SysPaymentTypeId);
                                                        if (paymentType != TermGroup_SysPaymentType.BG && paymentType != TermGroup_SysPaymentType.PG && paymentType != TermGroup_SysPaymentType.Bank)
                                                            continue;

                                                        //Second part of this condition is making use of the "synthetic BG code" which is generated when the initial payment file is created.
                                                        //Refer to this.ConvertEntityToLbFile(..) and its AccountPost section.
                                                        if (Utilities.PaymentNumber(row.PaymentNr) == post.BgCode || (row.Invoice != null && ActorIdToBgCode(row.Invoice.ActorId, row.PaymentNr) == post.BgCode))
                                                        {
                                                            if (postInvoiceSeqNr > 0)
                                                            {
                                                                found = row.Invoice.SeqNr == postInvoiceSeqNr;
                                                            }
                                                            else
                                                            {
                                                                found = row.Invoice.OCR == postInvoiceNumber || row.Invoice.InvoiceNr == postInvoiceNumber;
                                                            }
                                                        }

                                                        if (found)
                                                        {
                                                            if (row.Status == (int)SoePaymentStatus.None || row.Status == (int)SoePaymentStatus.Pending)
                                                            {
                                                                var paymentImportIO = new PaymentImportIO
                                                                {
                                                                    ActorCompanyId = actorCompanyId,
                                                                    BatchNr = batchId,
                                                                    Type = post.TransactionCode == 14 || post.TransactionCode == 54 ? (int)TermGroup_BillingType.Debit : (int)TermGroup_BillingType.Credit,
                                                                    CustomerId = row.Invoice.Actor != null ? row.Invoice.Actor.Supplier.ActorSupplierId : 0,
                                                                    Customer = row.Invoice.ActorName != null ? StringUtility.Left(row.Invoice.ActorName, 50) : string.Empty,
                                                                    InvoiceId = row.InvoiceId != null ? row.InvoiceId : 0,
                                                                    InvoiceNr = row.Invoice.InvoiceNr != null ? row.Invoice.InvoiceNr : string.Empty,
                                                                    InvoiceAmount = row.Invoice.TotalAmount,
                                                                    RestAmount = row.Invoice != null ? row.Invoice.TotalAmount - row.Invoice.PaidAmount : 0,
                                                                    PaidAmount = post.TransactionCode == 14 || post.TransactionCode == 54 ? Utilities.GetAmount(post.Amount) : Utilities.GetAmount(post.Amount) * -1,
                                                                    Currency = "SEK",
                                                                    InvoiceDate = row.Invoice != null ? row.Invoice.DueDate : null,
                                                                    PaidDate = fileDate,
                                                                    MatchCodeId = 0,
                                                                    Status = (int)ImportPaymentIOStatus.Match,
                                                                    State = (int)ImportPaymentIOState.Open,
                                                                    PaidAmountCurrency = post.TransactionCode == 14 || post.TransactionCode == 54 ? Utilities.GetAmount(post.Amount) : Utilities.GetAmount(post.Amount) * -1,
                                                                    InvoiceSeqnr = row.Invoice.SeqNr.HasValue ? row.Invoice.SeqNr.Value.ToString() : string.Empty,
                                                                    ImportType = (int)importType,
                                                                };

                                                                if (row.Invoice != null && row.Invoice.BillingType == (int)TermGroup_BillingType.Credit)
                                                                {
                                                                    paymentImportIO.RestAmount = paymentImportIO.InvoiceAmount - row.Invoice.PaymentRow.Where(r => r.Status == (int)SoePaymentStatus.Checked).Sum(r => r.Amount) - paymentImportIO.PaidAmount;
                                                                    if (paymentImportIO.PaidAmount != 0 && paymentImportIO.RestAmount != 0)
                                                                    {
                                                                        paymentImportIO.Status = (int)ImportPaymentIOStatus.PartlyPaid;
                                                                    }
                                                                }

                                                                totalInvoiceAmount = totalInvoiceAmount + Utilities.GetAmount(post.Amount);
                                                                paymentImportIOToAdd.Add(paymentImportIO);
                                                                break;
                                                            }
                                                            else if (Utilities.GetAmount(post.Amount) == row.Amount)
                                                            {
                                                                log.Add(GetText(7080, "Betalning med sekvensnummer") + " " + row.SeqNr + " " + GetText(7081, "för faktura med nr") + " " + row.Invoice.InvoiceNr + " " + GetText(7082, "är redan verifierad"));
                                                                break;
                                                            }
                                                            else
                                                            {
                                                                //Search for better matching payment if several partly payments has been done...
                                                                found = false;
                                                            }
                                                        }
                                                    }

                                                    if (found)
                                                    {
                                                        break;
                                                    }
                                                }

                                                if (!found)
                                                {
                                                    log.Add(GetText(7083, "Kunde inte hitta en matchande betalning till rad") + ": " + post.ToString());

                                                    PaymentImportIO paymentImportIO = new PaymentImportIO
                                                    {
                                                        ActorCompanyId = actorCompanyId,
                                                        BatchNr = batchId,
                                                        Type = post.TransactionCode == 14 || post.TransactionCode == 54 ? (int)TermGroup_BillingType.Debit : (int)TermGroup_BillingType.Credit,
                                                        CustomerId = 0,
                                                        Customer = string.Empty,
                                                        InvoiceId = 0,
                                                        InvoiceNr = post.InvoiceNumber.Trim(),
                                                        InvoiceAmount = 0,
                                                        RestAmount = 0,
                                                        PaidAmount = Utilities.GetAmount(post.Amount),
                                                        Currency = "SEK",
                                                        InvoiceDate = null,
                                                        PaidDate = fileDate,
                                                        MatchCodeId = 0,
                                                        Status = (int)ImportPaymentIOStatus.Error,
                                                        State = (int)ImportPaymentIOState.Closed,
                                                        PaidAmountCurrency = Utilities.GetAmount(post.Amount),
                                                        InvoiceSeqnr = postInvoiceSeqNr > 0 ? postInvoiceSeqNr.ToString() : "",
                                                        ImportType = (int)importType,
                                                    };

                                                    paymentImportIOToAdd.Add(paymentImportIO);
                                                }
                                                break;

                                            case (int)LbTransactionCodeDomestic.CreditInvoiceRestPost:
                                                var creditPost = (LbDomesticCreditInvoiceRestPost)postGroupItem;
                                                if (paymentImportIOToAdd.Any())
                                                {
                                                    PaymentImportIO creditPaymentImportIO = paymentImportIOToAdd[paymentImportIOToAdd.Count - 1];

                                                    //creditPaymentImportIO.RestAmount = Utilities.GetAmount(creditPost.RestAmount) * -1;  
                                                    var restAmount = creditPaymentImportIO.InvoiceId > 0 ? creditPaymentImportIO.RestAmount : Utilities.GetAmount(creditPost.RestAmount) * -1;
                                                    if (restAmount != 0)
                                                        creditPaymentImportIO.Status = (int)ImportPaymentIOStatus.PartlyPaid;
                                                }
                                                break;
                                            case (int)LbTransactionCodeDomestic.CommentPost:
                                                var error = (LbDomesticCommentPost)postGroupItem;
                                                if (!string.IsNullOrEmpty(error.ErrorCode))
                                                {
                                                    status = GetSysErrorId(error.ErrorCode);
                                                    foreach (var row in updatedRows)
                                                    {
                                                        row.Value.Status = status;
                                                    }
                                                }
                                                break;
                                            case (int)LbTransactionCodeDomestic.NamePost:
                                            case (int)LbTransactionCodeDomestic.AddressPost:
                                            case (int)LbTransactionCodeDomestic.AccountPost:
                                                break;
                                        }
                                    }
                                }

                            }
                        }
                        else if (fileContent is LbCorrectionSection correctionSection)
                        {
                            #region Corrections
                            foreach (var lbFileContentItem in correctionSection.Posts)
                            {
                                var correctionPost = ((LbCorrectionPost)lbFileContentItem);
                                switch (correctionPost.CorrectionCode)
                                {
                                    case 3:
                                        //03 Makulera alla kreditfakturor BGC
                                        foreach (Payment payment in payments)
                                        {
                                            foreach (PaymentRow row in payment.PaymentRow)
                                            {
                                                if (TermGroup_SysPaymentType.BG != Utilities.GetPaymentType(row.SysPaymentTypeId))
                                                    continue;

                                                //ChangeEntityState(row.Invoice, SoeEntityState.Deleted);//new status?
                                                //updatedRows.Add(row.PaymentRowId, row);
                                            }
                                        }
                                        break;
                                    case 11:
                                        //11 Makulera alla fakturor med angiven betalningsdag (gäller även fakturor i kronor till plusgironr)
                                        foreach (Payment payment in payments)
                                        {
                                            foreach (PaymentRow row in payment.PaymentRow)
                                            {
                                                if (TermGroup_SysPaymentType.BG != Utilities.GetPaymentType(row.SysPaymentTypeId))
                                                    continue;
                                                if (Utilities.DateCompareConverter(correctionPost.Date, row.Invoice.DueDate.Value))
                                                {
                                                    //ChangeEntityState(row.Invoice, SoeEntityState.Deleted);
                                                    //updatedRows.Add(row.PaymentRowId, row);
                                                }
                                            }
                                        }
                                        break;
                                    case 12:
                                        //12 makulera alla fakturor till ett plusgironummer
                                        foreach (Payment payment in payments)
                                        {
                                            foreach (PaymentRow row in payment.PaymentRow)
                                            {
                                                if (TermGroup_SysPaymentType.PG != Utilities.GetPaymentType(row.SysPaymentTypeId))
                                                    continue;

                                                if (correctionPost.ReceiverBgCode != Utilities.PaymentNumber(row.PaymentNr))
                                                    continue;

                                                //ChangeEntityState(row.Invoice, SoeEntityState.Deleted);
                                                //updatedRows.Add(row.PaymentRowId, row);
                                            }
                                        }
                                        break;
                                    case 13:
                                        //13 Makulera alla fakturor till ett mottagarnr med en angiven betalningsdag ej pg
                                        foreach (Payment payment in payments)
                                        {
                                            foreach (PaymentRow row in payment.PaymentRow)
                                            {
                                                if (TermGroup_SysPaymentType.BG != Utilities.GetPaymentType(row.SysPaymentTypeId))
                                                    continue;
                                                if (row.PaymentNr != (correctionPost.ReceiverBgCode.ToString()).Trim())
                                                    continue;
                                                if (row.Invoice.Actor.Supplier.OrgNr != (correctionPost.SenderCustomerNumber.ToString()).Trim())
                                                    continue;
                                                if (Utilities.DateCompareConverter(correctionPost.Date, row.Invoice.DueDate.Value))
                                                {
                                                    //ChangeEntityState(row.Invoice, SoeEntityState.Deleted);
                                                    //updatedRows.Add(row.PaymentRowId, row);
                                                }
                                            }
                                        }
                                        break;
                                    case 14:
                                        //14 Enstaka faktura till ett plusgironummer med angiven betalningsdag
                                        foreach (Payment payment in payments)
                                        {
                                            foreach (PaymentRow row in payment.PaymentRow)
                                            {
                                                if (TermGroup_SysPaymentType.PG != Utilities.GetPaymentType(row.SysPaymentTypeId))
                                                    continue;
                                                if (correctionPost.ReceiverBgCode.ToString().Trim() != row.PaymentNr)
                                                    continue;
                                                if (Utilities.DateCompareConverter(correctionPost.Date, row.Invoice.DueDate.Value))
                                                {
                                                    //ChangeEntityState(row.Invoice, SoeEntityState.Deleted);
                                                    //updatedRows.Add(row.PaymentRowId, row);
                                                }
                                            }
                                        }
                                        break;
                                    case 16://16 Makulera lön
                                        break;
                                    case 21:
                                        //21 Ändra betalningsdag för alla fakturor (gäller även fakturor i kronor till plusgironr)
                                        foreach (Payment payment in payments)
                                        {
                                            foreach (PaymentRow row in payment.PaymentRow)
                                            {
                                                if (TermGroup_SysPaymentType.PG == Utilities.GetPaymentType(row.SysPaymentTypeId))
                                                {
                                                    if (row.Invoice.Currency.SysCurrencyId == (int)LbCurrency.SEK)
                                                    {
                                                        //row.Invoice.DueDate = Utilities.GetDateTime(TermGroup_SysPaymentMethod.LB, correctionPost.Date);
                                                        //updatedRows.Add(row.PaymentRowId, row);
                                                    }
                                                }
                                                else if (TermGroup_SysPaymentType.BG == Utilities.GetPaymentType(row.SysPaymentTypeId))
                                                {
                                                    //row.Invoice.DueDate = Utilities.GetDateTime(TermGroup_SysPaymentMethod.LB, correctionPost.Date);
                                                    //updatedRows.Add(row.PaymentRowId, row);
                                                }
                                            }
                                        }
                                        break;
                                    case 22:
                                        //22 Ändra angiven betalningsdag till ny betalningsdag för alla fakturor inklusive löner. BGC
                                        foreach (Payment payment in payments)
                                        {
                                            foreach (PaymentRow row in payment.PaymentRow)
                                            {
                                                if (TermGroup_SysPaymentType.BG != Utilities.GetPaymentType(row.SysPaymentTypeId))
                                                    continue;
                                                //row.Invoice.DueDate = Utilities.GetDateTime(TermGroup_SysPaymentMethod.LB, correctionPost.Date);
                                                //updatedRows.Add(row.PaymentRowId, row);
                                            }
                                        }
                                        break;
                                    case 23:
                                        //23 Betalningsdag för alla fakturor till ett plusgironummer
                                        foreach (Payment payment in payments)
                                        {
                                            foreach (PaymentRow row in payment.PaymentRow)
                                            {
                                                if (TermGroup_SysPaymentType.PG != Utilities.GetPaymentType(row.SysPaymentTypeId))
                                                    continue;
                                                if (row.PaymentNr != correctionPost.ReceiverBgCode.ToString().Trim())
                                                    continue;
                                                //row.Invoice.DueDate = Utilities.GetDateTime(TermGroup_SysPaymentMethod.LB, correctionPost.Date);
                                                //updatedRows.Add(row.PaymentRowId, row);
                                            }
                                        }
                                        break;
                                    case 24:
                                        //24 Angiven betalningsdag till ny betalningsdag för alla fakturor till ett plusgironummer
                                        foreach (Payment payment in payments)
                                        {
                                            foreach (PaymentRow row in payment.PaymentRow)
                                            {
                                                if (TermGroup_SysPaymentType.PG != Utilities.GetPaymentType(row.SysPaymentTypeId))
                                                    continue;
                                                if (row.PaymentNr != correctionPost.ReceiverBgCode.ToString().Trim())
                                                    continue;
                                                //row.Invoice.DueDate = Utilities.GetDateTime(TermGroup_SysPaymentMethod.LB, correctionPost.Date);
                                                //updatedRows.Add(row.PaymentRowId, row);
                                            }
                                        }
                                        break;
                                    case 25:
                                        //25 Enstaka faktura till ett plusgironummer med angiven betalningsdag
                                        foreach (Payment payment in payments)
                                        {
                                            foreach (PaymentRow row in payment.PaymentRow)
                                            {
                                                if (TermGroup_SysPaymentType.PG != Utilities.GetPaymentType(row.SysPaymentTypeId))
                                                    continue;
                                                if (row.PaymentNr != correctionPost.ReceiverBgCode.ToString().Trim())
                                                    continue;
                                                //row.Invoice.DueDate = Utilities.GetDateTime(TermGroup_SysPaymentMethod.LB, correctionPost.Date);
                                                //updatedRows.Add(row.PaymentRowId, row);

                                            }
                                        }
                                        break;
                                    case 31:
                                        //31 Makulera alla kreditfakturor till ett mottagarnr
                                        foreach (Payment payment in payments)
                                        {
                                            foreach (PaymentRow row in payment.PaymentRow)
                                            {
                                                if (TermGroup_SysPaymentType.BG != Utilities.GetPaymentType(row.SysPaymentTypeId))
                                                    continue;
                                                if (row.Invoice.Type != (int)TermGroup_BillingType.Credit)
                                                    continue;
                                                if (row.Invoice.Actor.Supplier.OrgNr != correctionPost.SenderCustomerNumber.ToString().Trim())
                                                    continue;

                                                //ChangeEntityState(row.Invoice, SoeEntityState.Deleted);
                                                //updatedRows.Add(row.PaymentRowId, row);
                                            }
                                        }
                                        break;
                                    case 33:
                                        //33 Makulera alla kreditfakturor till ett mottagarnr med angiven sista bevakningsdag
                                        foreach (Payment payment in payments)
                                        {
                                            foreach (var row in payment.PaymentRow)
                                            {
                                                if (TermGroup_SysPaymentType.BG != Utilities.GetPaymentType(row.SysPaymentTypeId))
                                                    continue;
                                                if (row.Invoice.Type != (int)TermGroup_BillingType.Credit)
                                                    continue;
                                                if (row.Invoice.Actor.Supplier.OrgNr != correctionPost.SenderCustomerNumber.ToString().Trim())
                                                    continue;
                                                if (Utilities.DateCompareConverter(correctionPost.Date.Trim(), row.Invoice.DueDate.Value))
                                                {
                                                    //ChangeEntityState(row.Invoice, SoeEntityState.Deleted);
                                                    //updatedRows.Add(row.PaymentRowId, row);
                                                }
                                            }
                                        }
                                        break;
                                    case 34:
                                        //34 Makulera enstaka kreditfaktura
                                        foreach (Payment payment in payments)
                                        {
                                            foreach (PaymentRow row in payment.PaymentRow)
                                            {
                                                if (TermGroup_SysPaymentType.BG != Utilities.GetPaymentType(row.SysPaymentTypeId))
                                                    continue;
                                                if (row.Invoice.Type != (int)TermGroup_BillingType.Credit)
                                                    continue;
                                                if (row.PaymentNr != correctionPost.ReceiverBgCode.ToString().Trim())
                                                    continue;

                                                //ChangeEntityState(row.Invoice, SoeEntityState.Deleted);
                                                //updatedRows.Add(row.PaymentRowId, row);
                                            }
                                        }
                                        break;
                                    case 36:
                                        //36 Makulera alla kreditfakturor med angiven sista bevakningsdag
                                        foreach (Payment payment in payments)
                                        {
                                            foreach (PaymentRow row in payment.PaymentRow)
                                            {
                                                if (TermGroup_SysPaymentType.BG != Utilities.GetPaymentType(row.SysPaymentTypeId))
                                                    continue;
                                                if (row.Invoice.Type != (int)TermGroup_BillingType.Credit)
                                                    continue;
                                                if (!Utilities.DateCompareConverter(correctionPost.Date.Trim(), row.Invoice.DueDate.Value))
                                                    continue;

                                                //ChangeEntityState(row.Invoice, SoeEntityState.Deleted);
                                                //updatedRows.Add(row.PaymentRowId, row);
                                            }
                                        }
                                        break;
                                    case 42:
                                        //42 Ändra angiven sista bevakningsdag till ny sista bevakningsdag för alla kreditfakturor till viss mottagare
                                        foreach (Payment payment in payments)
                                        {
                                            foreach (PaymentRow row in payment.PaymentRow)
                                            {
                                                if (TermGroup_SysPaymentType.BG != Utilities.GetPaymentType(row.SysPaymentTypeId))
                                                    continue;
                                                if (row.Invoice.Type != (int)TermGroup_BillingType.Credit)
                                                    continue;
                                                if (row.Invoice.Actor.Supplier.OrgNr != correctionPost.SenderCustomerNumber.ToString().Trim())
                                                    continue;
                                                if (row.PaymentNr != correctionPost.ReceiverBgCode.ToString().Trim())
                                                    continue;

                                                //row.Invoice.DueDate = Utilities.GetDateTime(TermGroup_SysPaymentMethod.LB, correctionPost.Date);
                                                //updatedRows.Add(row.PaymentRowId, row);
                                            }
                                        }
                                        break;
                                    case 44:
                                        //44 Ändra angiven sista bevakningsdag för enstaka kreditfaktura
                                        foreach (Payment payment in payments)
                                        {
                                            foreach (PaymentRow row in payment.PaymentRow)
                                            {
                                                if (TermGroup_SysPaymentType.BG != Utilities.GetPaymentType(row.SysPaymentTypeId))
                                                    continue;
                                                if (row.Invoice.Type != (int)TermGroup_BillingType.Credit)
                                                    continue;
                                                if (row.PaymentNr != correctionPost.ReceiverBgCode.ToString().Trim())
                                                    continue;

                                                //row.Invoice.DueDate = Utilities.GetDateTime(TermGroup_SysPaymentMethod.LB, correctionPost.Date);
                                                //updatedRows.Add(row.PaymentRowId, row);
                                            }
                                        }
                                        break;
                                    case 46:
                                        //46 Ändra angiven sista bevakningsdag till ny sista bevakningsdag för alla kreditfakturor
                                        foreach (Payment payment in payments)
                                        {
                                            foreach (PaymentRow row in payment.PaymentRow)
                                            {
                                                if (TermGroup_SysPaymentType.BG != Utilities.GetPaymentType(row.SysPaymentTypeId))
                                                    continue;
                                                if (row.Invoice.Type != (int)TermGroup_BillingType.Credit)
                                                    continue;

                                                //row.Invoice.DueDate = Utilities.GetDateTime(TermGroup_SysPaymentMethod.LB, correctionPost.Date);
                                                //updatedRows.Add(row.PaymentRowId, row);
                                            }
                                        }
                                        break;
                                    case 48:
                                        //48 Ändra sista bevakningsdag för alla kreditfakturor
                                        foreach (Payment payment in payments)
                                        {
                                            foreach (PaymentRow row in payment.PaymentRow)
                                            {
                                                if (TermGroup_SysPaymentType.BG != Utilities.GetPaymentType(row.SysPaymentTypeId))
                                                    continue;
                                                if (row.Invoice.Type != (int)TermGroup_BillingType.Credit)
                                                    continue;

                                                //row.Invoice.DueDate = Utilities.GetDateTime(TermGroup_SysPaymentMethod.LB, correctionPost.Date);
                                                //updatedRows.Add(row.PaymentRowId, row);
                                            }
                                        }
                                        break;
                                }
                            }
                            #endregion
                        }

                        #endregion

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
                                        paymentImport.ImportDate = fileDate.HasValue ? fileDate.Value : paymentImport.ImportDate;

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

        private int GetSysErrorId(string errorCode)
        {
            errorCode = errorCode.Trim();
            int errorId = 0;

            using (SOESysEntities sysEntities = new SOESysEntities())
            {
                errorId = (from lb in sysEntities.SysLbError
                           where lb.LbErrorCode == errorCode
                           select lb.SysErrorId).First();
            }

            return Convert.ToInt32(((int)SoePaymentStatus.Error).ToString() + errorId.ToString());
        }

        private string GetPaymentMethodForBank(int bankCode, int paymentMethodCode)
        {
            string returnString = String.Empty;
            switch (paymentMethodCode)
            {
                case (int)TermGroup_ForeignPaymentMethod.Normal:
                    switch (bankCode)
                    {
                        case (int)TermGroup_ForeignPaymentBankCode.Handelsbanken:
                            returnString = " ";
                            break;
                        case (int)TermGroup_ForeignPaymentBankCode.SEB:
                        case (int)TermGroup_ForeignPaymentBankCode.Swedbank:
                            returnString = "0";
                            break;
                        default:
                            returnString = "0";
                            break;
                    }
                    break;
                case (int)TermGroup_ForeignPaymentMethod.Express:
                    switch (bankCode)
                    {
                        case (int)TermGroup_ForeignPaymentBankCode.Handelsbanken:
                            returnString = " ";
                            break;
                        case (int)TermGroup_ForeignPaymentBankCode.SEB:
                        case (int)TermGroup_ForeignPaymentBankCode.Swedbank:
                            returnString = "1";
                            break;
                    }
                    break;
                case (int)TermGroup_ForeignPaymentMethod.CompanyGroup:
                    switch (bankCode)
                    {
                        case (int)TermGroup_ForeignPaymentBankCode.Handelsbanken:
                            returnString = " ";
                            break;
                        case (int)TermGroup_ForeignPaymentBankCode.SEB:
                        case (int)TermGroup_ForeignPaymentBankCode.Swedbank:
                            returnString = "2";
                            break;
                    }
                    break;
                default:
                    switch (bankCode)
                    {
                        case (int)TermGroup_ForeignPaymentBankCode.Handelsbanken:
                            returnString = " ";
                            break;
                        default:
                            returnString = "0";
                            break;
                    }
                    break;
            }
            return returnString;
        }

        private string GetPaymentFormForBank(int bankCode, int paymentForm)
        {
            string returnString = String.Empty;
            switch (paymentForm)
            {
                case (int)TermGroup_ForeignPaymentForm.Account:
                    switch (bankCode)
                    {
                        case (int)TermGroup_ForeignPaymentBankCode.Handelsbanken:
                            returnString = "B";
                            break;
                        case (int)TermGroup_ForeignPaymentBankCode.SEB:
                        case (int)TermGroup_ForeignPaymentBankCode.Swedbank:
                        case (int)TermGroup_ForeignPaymentBankCode.Nordea:
                            returnString = "1";
                            break;
                        default:
                            returnString = "0";
                            break;
                    }
                    break;
                case (int)TermGroup_ForeignPaymentForm.Check:
                    switch (bankCode)
                    {
                        case (int)TermGroup_ForeignPaymentBankCode.Handelsbanken:
                            returnString = "C";
                            break;
                        case (int)TermGroup_ForeignPaymentBankCode.SEB:
                        case (int)TermGroup_ForeignPaymentBankCode.Swedbank:
                            returnString = "0";
                            break;

                    }
                    break;
                default:
                    returnString = "0";
                    break;
            }
            return returnString;
        }

        private string GetPaymentChargeCodeForBank(int bankCode, int paymentChargeCode)
        {
            string returnString = String.Empty;
            switch (paymentChargeCode)
            {
                case (int)TermGroup_ForeignPaymentChargeCode.SenderDomesticCosts:
                    switch (bankCode)
                    {
                        case (int)TermGroup_ForeignPaymentBankCode.Swedbank:
                            returnString = "2";
                            break;
                        case (int)TermGroup_ForeignPaymentBankCode.Nordea:
                            returnString = "1";
                            break;
                        default:
                            returnString = "0";
                            break;
                    }
                    break;
                case (int)TermGroup_ForeignPaymentChargeCode.SenderAllCosts:
                    switch (bankCode)
                    {
                        case (int)TermGroup_ForeignPaymentBankCode.Handelsbanken:
                            returnString = "9";
                            break;
                        case (int)TermGroup_ForeignPaymentBankCode.SEB:
                            returnString = "3";
                            break;
                        case (int)TermGroup_ForeignPaymentBankCode.Swedbank:
                        case (int)TermGroup_ForeignPaymentBankCode.Nordea:
                            returnString = "0";
                            break;
                        default:
                            returnString = "2";
                            break;
                    }
                    break;
                case (int)TermGroup_ForeignPaymentChargeCode.RecieverAllCosts:
                    switch (bankCode)
                    {
                        case (int)TermGroup_ForeignPaymentBankCode.Handelsbanken:
                        case (int)TermGroup_ForeignPaymentBankCode.Nordea:
                            returnString = "1";
                            break;
                        default:
                            returnString = "2";
                            break;
                    }
                    break;
                default:
                    returnString = "0";
                    break;
            }
            return returnString;
        }
        #endregion

        #region Export

        /// <summary>
        /// Exports LbFile to stream for save dialog
        /// </summary>
        /// <param name="entities">The ObjectContext to use</param>
        /// <param name="transaction">The current Transaction</param>
        /// <param name="actorCompanyId">The ActorCompanyId</param>
        /// <param name="payment">The Payment</param>
        /// <returns>PaymentExport, contains null if the export failed</returns>
        public ActionResult Export(CompEntities entities, List<SysCountry> sysCountries, TransactionScope transaction, PaymentMethod paymentMethod, List<PaymentRow> paymentRows, int paymentId, int actorCompanyId)
        {
            var result = new ActionResult(true);

            try
            {
                result = ConvertEntityToLbFile(actorCompanyId, paymentRows, paymentMethod, entities, sysCountries);
                if (!result.Success)
                    return result;

                lbFile = result.Value as LbFile;
                if (lbFile == null)
                    return result;
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result.Exception = ex;
                return result;
            }

            string guid = Guid.NewGuid().ToString("N");
            string fileName = Utilities.GetLBFileNameOnServer(guid);

            byte[] file = WriteFileToMemory(lbFile);
            if (file != null)
                result = CreatePaymentExport(fileName, paymentRows, TermGroup_SysPaymentMethod.LB, "", guid, file);
            else
                result.Success = false;

            return result;
        }

        private ActionResult ConvertEntityToLbFile(int actorCompanyId, IEnumerable<PaymentRow> paymentRows, PaymentMethod paymentMethod, CompEntities entities, List<SysCountry> sysCountries)
        {
            var result = new ActionResult(true);
            var fileObject = new LbFile();
            int observationMethod = sm.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierPaymentObservationMethod, 0, actorCompanyId, 0);
            int observationDays = sm.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierPaymentObservationDays, 0, actorCompanyId, 0);
            int foreignBank = sm.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierPaymentForeignBankCode, 0, actorCompanyId, 0);

            try
            {
                #region Prereq

                var company = CompanyManager.GetCompanyDTO(entities, actorCompanyId);

                var paymentInformationRows = PaymentManager.GetPaymentInformationRowsForActor(entities, actorCompanyId, TermGroup_SysPaymentType.BG);
                PaymentInformationRow paymentInformationRow = null;
                if (paymentInformationRows != null)
                {
                    //Choose the default if more than one PaymentInformationRow's exist
                    if (paymentInformationRows.Count == 1)
                        paymentInformationRow = paymentInformationRows.FirstOrDefault();
                    else if (paymentInformationRows.Count > 1)
                        paymentInformationRow = paymentInformationRows.FirstOrDefault(i => i.Default);
                }

                if (paymentInformationRow == null)
                    return new ActionResult((int)ActionResultSave.PaymentFilePaymentInformationMissing, GetText(7531, "Bankgiro är inte angett på aktuellt företag"));

                #endregion

                /*
                #region Prereq
                Company company = CompanyManager.GetCompany(entities, actorCompanyId);
                if (company == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                if (!company.ActorReference.IsLoaded)
                    company.ActorReference.Load();
                if (!company.Actor.Contact.IsLoaded)
                    company.Actor.Contact.Load();
                if (company.Actor == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "Actor");

                if (!company.Actor.PaymentInformation.IsLoaded)
                    company.Actor.PaymentInformation.Load();
                if (company.Actor.PaymentInformation == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "PaymentInformation");


                foreach (var item in company.Actor.PaymentInformation)
                {
                    if (!item.PaymentInformationRow.IsLoaded)
                        item.PaymentInformationRow.Load();
                }

                #endregion

                #region PaymentInformation

                var paymentInformation = (from pi in company.Actor.PaymentInformation
                                          where ((pi.PaymentInformationRow.Any(i => i.SysPaymentTypeId == (int)TermGroup_SysPaymentType.BG
                                              && i.Default
                                              && i.State == (int)SoeEntityState.Active)) &&
                                          (pi.State == (int)SoeEntityState.Active))
                                          select pi).FirstOrDefault();

                if (paymentInformation == null)
                    paymentInformation = (from pi in company.Actor.PaymentInformation
                                          where ((pi.PaymentInformationRow.Any(i => i.SysPaymentTypeId == (int)TermGroup_SysPaymentType.BG
                                              && i.State == (int)SoeEntityState.Active)) &&
                                          (pi.State == (int)SoeEntityState.Active))
                                          select pi).FirstOrDefault();

                if (paymentInformation == null)
                    return new ActionResult((int)ActionResultSave.PaymentFilePaymentInformationMissing, GetText(7531, "Bankgiro är inte angett på aktuellt företag"));

                if (!paymentInformation.PaymentInformationRow.IsLoaded)
                    paymentInformation.PaymentInformationRow.Load();

                #endregion

                #region PaymentInformationRow

                if (paymentInformation.PaymentInformationRow == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "PaymentInformationRow");

                var paymentInformationRows = (from pir in paymentInformation.PaymentInformationRow
                                              where (pir.SysPaymentTypeId == (int)TermGroup_SysPaymentType.BG)
                                              select pir).ToList();

                PaymentInformationRow paymentInformationRow = null;
                if (paymentInformationRows != null)
                {
                    //Choose the default if more than one PaymentInformationRow's exist
                    if (paymentInformationRows.Count == 1)
                        paymentInformationRow = paymentInformationRows.FirstOrDefault();
                    else if (paymentInformationRows.Count > 1)
                        paymentInformationRow = paymentInformationRows.FirstOrDefault(i => i.Default);
                }

                if (paymentInformationRow == null)
                    return new ActionResult((int)ActionResultSave.PaymentFilePaymentInformationMissing);
                */

                if (!paymentMethod.PaymentInformationRowReference.IsLoaded)
                    paymentMethod.PaymentInformationRowReference.Load();

                int senderBgc = Utilities.PaymentNumber(paymentMethod.PaymentInformationRow.PaymentNr);

                SupplierInvoice invoice = null;
                Supplier supplier = null;
                LbPaymentSection paymentSection = null;
                LbPaymentSection foreignSection = null;
                decimal totalSum = 0M;
                decimal totalSumDomestic = 0M;
                int totalCount = 0;
                decimal totalSumReceiverNumber = 0M;
                string createdDate = Utilities.GetDate(DateTime.Now);
                int transactionCode = 123;
                int sysCurrencyId = (int)LbCurrency.Undefined;

                // Sort payments on invoice billing type
                paymentRows = paymentRows.OrderBy(p => p.Invoice != null ? p.Invoice.BillingType : 1000);

                foreach (PaymentRow paymentRow in paymentRows)
                {
                    #region PaymentRow

                    int paymentNumber = Utilities.PaymentNumber(paymentRow.PaymentNr);

                    if (paymentRow.Invoice != null)
                        invoice = paymentRow.Invoice as SupplierInvoice;

                    if (sysCurrencyId == (int)LbCurrency.Undefined)
                    {
                        if (invoice != null)
                        {
                            switch (invoice.Currency.SysCurrencyId)
                            {
                                case (int)LbCurrency.SEK:
                                case (int)LbCurrency.USD:
                                case (int)LbCurrency.EUR:
                                    break;
                                    //default:
                                    //    return null;
                            }
                            sysCurrencyId = invoice.Currency.SysCurrencyId;
                        }
                    }

                    if (invoice != null)
                    {


                        //if (sysCurrencyId != invoice.Currency.SysCurrencyId)
                        //    continue;

                        switch (paymentRow.SysPaymentTypeId)
                        {
                            case (int)TermGroup_SysPaymentType.PG:
                                transactionCode = (int)LbTransactionCodeDomestic.PlusgiroPost;
                                break;

                            case (int)TermGroup_SysPaymentType.BG:
                                switch (invoice.BillingType)
                                {
                                    case (int)TermGroup_BillingType.Credit:
                                        switch (observationMethod)
                                        {
                                            case 0:
                                                transactionCode = (int)LbTransactionCodeDomestic.ReductionPost;
                                                break;
                                            case 1:
                                                transactionCode = (int)LbTransactionCodeDomestic.CreditInvoiceObservationPost;
                                                break;
                                            case 2:
                                                transactionCode = (int)LbTransactionCodeDomestic.CreditInvoiceObservationCompletePost;
                                                break;
                                        }
                                        break;
                                    case (int)TermGroup_BillingType.Debit:
                                    case (int)TermGroup_BillingType.Interest:
                                    case (int)TermGroup_BillingType.Reminder:
                                        transactionCode = (int)LbTransactionCodeDomestic.PaymentPost;
                                        break;
                                }
                                break;

                            case (int)TermGroup_SysPaymentType.Bank:
                                switch (invoice.BillingType)
                                {
                                    case (int)TermGroup_BillingType.Credit:
                                        //transactionCode = (int)LbTransactionCodeDomestic.CreditInvoiceObservationCompletePost;
                                        switch (observationMethod)
                                        {
                                            case 0:
                                                transactionCode = (int)LbTransactionCodeDomestic.ReductionPost;
                                                break;
                                            case 1:
                                                transactionCode = (int)LbTransactionCodeDomestic.CreditInvoiceObservationPost;
                                                break;
                                            case 2:
                                                transactionCode = (int)LbTransactionCodeDomestic.CreditInvoiceObservationCompletePost;
                                                break;
                                        }
                                        break;
                                    case (int)TermGroup_BillingType.Debit:
                                        transactionCode = (int)LbTransactionCodeDomestic.AccountPost;
                                        break;
                                    case (int)TermGroup_BillingType.Interest:
                                    case (int)TermGroup_BillingType.Reminder:
                                        transactionCode = (int)LbTransactionCodeDomestic.AccountPost;
                                        break;
                                }
                                break;

                            case (int)TermGroup_SysPaymentType.BIC:
                                switch (invoice.BillingType)
                                {
                                    case (int)TermGroup_BillingType.Credit:
                                        transactionCode = (int)LbTransactionCodeForeign.CreditInvoicePost;
                                        break;
                                    case (int)TermGroup_BillingType.Debit:
                                        transactionCode = (int)LbTransactionCodeForeign.InvoiceReductionPost;
                                        break;
                                }
                                break;
                        }
                    }

                    #endregion

                    #region Correction
                    //if (correction)
                    //{
                    //    if (correctionSection == null)
                    //        correctionSection = new LbCorrectionSection();
                    //    correctionSection.Posts.Add(new LbCorrectionPost(string.Empty)); //parameters
                    //}
                    #endregion

                    switch (transactionCode)
                    {
                        case (int)LbTransactionCodeDomestic.PaymentPost:
                        case (int)LbTransactionCodeDomestic.CreditInvoiceObservationCompletePost:
                        case (int)LbTransactionCodeDomestic.CreditInvoiceObservationPost:
                        case (int)LbTransactionCodeDomestic.PlusgiroPost:
                        case (int)LbTransactionCodeDomestic.ReductionPost:
                            #region Domnestic

                            if (paymentSection == null)
                            {
                                paymentSection = new LbPaymentSection();
                                paymentSection.Posts.Add(new LbDomesticOpeningPost(senderBgc, createdDate, string.Empty, CountryCurrencyManager.GetCurrencyCode(sysCurrencyId)));
                            }

                            totalCount++;

                            if (invoice != null)
                            {
                                //totalSum += invoice.TotalAmount; //Old code, should use paymentRow.Amount
                                totalSum += paymentRow.Amount;
                                string invoiceOCRorNr = !string.IsNullOrEmpty(invoice.OCR) ? invoice.OCR : invoice.InvoiceNr;
                                DateTime payDate = paymentRow.PayDate;
                                if (transactionCode == (int)LbTransactionCodeDomestic.CreditInvoiceObservationPost ||
                                    transactionCode == (int)LbTransactionCodeDomestic.CreditInvoiceObservationCompletePost)
                                {
                                    payDate = paymentRow.PayDate.AddDays(observationDays);
                                }
                                //paymentSection.Posts.Add(new LbDomesticPaymentPost(transactionCode, paymentNumber, invoiceOCRorNr, Math.Abs(Utilities.GetAmount(invoice.TotalAmount)), Utilities.GetDate(paymentRow.PayDate), invoice.SeqNr.ToString() + "," + invoice.ActorName ));
                                paymentSection.Posts.Add(new LbDomesticPaymentPost(transactionCode, paymentNumber, invoiceOCRorNr, Math.Abs(Utilities.GetAmount(paymentRow.Amount)), Utilities.GetDate(payDate), invoice.SeqNr.ToString() + ", " + invoice.ActorName.TrimStart()));
                            }

                            #endregion
                            break;

                        case (int)LbTransactionCodeDomestic.AccountPost:
                            #region Domnestic

                            if (paymentSection == null)
                            {
                                paymentSection = new LbPaymentSection();
                                paymentSection.Posts.Add(new LbDomesticOpeningPost(senderBgc, createdDate, string.Empty, CountryCurrencyManager.GetCurrencyCode(sysCurrencyId)));
                            }

                            totalCount++;

                            if (invoice != null)
                            {
                                string temp = paymentRow.PaymentNr.Replace(" ", "").Replace("-", "");
                                string account = "";

                                foreach (Char c in temp)
                                {
                                    if (Char.IsDigit(c))
                                        account = account + c;
                                }

                                int syntheticBgCode = ActorIdToBgCode(invoice.ActorId, account);

                                paymentSection.Posts.Add(new LbDomesticAccountPost(syntheticBgCode, account.Substring(0, 4), account.Substring(4), "", false, checkNum: true));

                                //totalSum += invoice.TotalAmount; //Old code, should use paymentRow.Amount
                                totalSum += paymentRow.Amount;
                                string invoiceOCRorNr = !String.IsNullOrEmpty(invoice.OCR) ? invoice.OCR : invoice.InvoiceNr;
                                //paymentSection.Posts.Add(new LbDomesticPaymentPost(transactionCode, paymentNumber, invoiceOCRorNr, Math.Abs(Utilities.GetAmount(invoice.TotalAmount)), Utilities.GetDate(paymentRow.PayDate), invoice.SeqNr.ToString() + "," + invoice.ActorName ));
                                paymentSection.Posts.Add(new LbDomesticPaymentPost((int)LbTransactionCodeDomestic.PaymentPost, syntheticBgCode, invoiceOCRorNr, Math.Abs(Utilities.GetAmount(paymentRow.Amount)), Utilities.GetDate(paymentRow.PayDate), invoice.SeqNr.ToString() + "," + invoice.ActorName, checkNum: true));
                            }

                            #endregion
                            break;

                        case (int)LbTransactionCodeForeign.InvoiceReductionPost:
                        case (int)LbTransactionCodeForeign.CreditInvoicePost:
                            #region Foreign
                            var paymentInformationRowsSupplier = PaymentManager.GetPaymentInformationRowsForActor(entities, invoice.ActorId.Value);

                            if (paymentInformationRowsSupplier == null)
                                return new ActionResult((int)ActionResultSave.EntityNotFound, "PaymentInformationRow");


                            PaymentInformationRow paymentInformationRowSupplier = null;
                            if (paymentInformationRowsSupplier != null)
                            {
                                //Choose the default if more than one PaymentInformationRow's exist
                                if (paymentInformationRowsSupplier.Count == 1)
                                    paymentInformationRowSupplier = paymentInformationRowsSupplier.FirstOrDefault();
                                else if (paymentInformationRowsSupplier.Count > 1)
                                    paymentInformationRowSupplier = paymentInformationRowsSupplier.FirstOrDefault(i => i.PaymentNr == paymentRow.PaymentNr);
                            }

                            if (paymentInformationRowSupplier == null)
                                return new ActionResult((int)ActionResultSave.PaymentFilePaymentInformationMissing);

                            Contact contact = ContactManager.GetContactFromActor(entities, company.ActorCompanyId, loadAllContactInfo: true);
                            ContactAddress contactAddress = contact.ContactAddress.First();

                            string senderName = company.Name;
                            string senderAddress = Utilities.GetAddressPart(contactAddress, TermGroup_SysContactAddressRowType.Address);
                            int layoutCode = Utilities.GetLayoutCode(senderBgc);

                            if (foreignSection == null)
                            {
                                foreignSection = new LbPaymentSection();
                                string merging = String.Empty;
                                if (foreignBank == (int)TermGroup_ForeignPaymentBankCode.SEB)
                                    merging = "1";
                                foreignSection.Posts.Add(new LbForeignOpeningPost(senderBgc, createdDate, senderName, senderAddress, string.Empty, layoutCode, merging));
                            }

                            const int accountClosingCode = 0;
                            string currencyAccountCode = " ";
                            if (foreignBank > 1 && foreignBank < 5) //SEB + SWED + NORDEA
                                currencyAccountCode = "0";
                            string currencyTermAccountnumber = "0000000000";
                            if (!String.IsNullOrEmpty(paymentInformationRowSupplier.CurrencyAccount))
                            {
                                if (foreignBank > 1 && foreignBank < 5)
                                    currencyAccountCode = "1";
                                currencyTermAccountnumber = Utilities.PaymentNumberLong(paymentInformationRowSupplier.CurrencyAccount);
                            }

                            //var rbCode = Utilities.GetRiksBankIdentificationCode(company);
                            string rbCode = paymentInformationRowSupplier.PaymentCode;



                            //#region Foreign optional (inactive) posts assignments

                            string paymentType = GetPaymentFormForBank(foreignBank, paymentInformationRowSupplier.PaymentForm.HasValue ? paymentInformationRowSupplier.PaymentForm.Value : 0);
                            string debitCode = GetPaymentChargeCodeForBank(foreignBank, paymentInformationRowSupplier.ChargeCode.HasValue ? paymentInformationRowSupplier.ChargeCode.Value : 0);
                            string paymentMethodId = GetPaymentMethodForBank(foreignBank, paymentInformationRowSupplier.PaymentMethodCode.HasValue ? paymentInformationRowSupplier.PaymentMethodCode.Value : 0);

                            supplier = SupplierManager.GetSupplier(entities, invoice.ActorId.Value, true, false, false, false);
                            Contact supplierContact = ContactManager.GetContactFromActor(entities, supplier.Actor.ActorId, loadAllContactInfo: true);
                            ContactAddress supplierContactAddress = supplierContact.ContactAddress.First();
                            // Country
                            string country = String.Empty;
                            if (supplier.SysCountryId.HasValue)
                            {
                                SysCountry sysCountry = (from c in sysCountries
                                                         where c.SysCountryId == supplier.SysCountryId.Value
                                                         select c).First();
                                country = sysCountry.Code;
                            }

                            string id1 = paymentInformationRowSupplier.BIC;                //assigned by converter
                            string id2 = paymentInformationRowSupplier.PaymentNr;                //assigned by converter
                            string id3 = paymentInformationRowSupplier.ClearingCode;                //assigned by converter
                            string conversion = String.Empty;
                            if (foreignBank == (int)TermGroup_ForeignPaymentBankCode.SEB)
                                conversion = "0";

                            if (String.IsNullOrEmpty(id1) || String.IsNullOrEmpty(id2))
                                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(9328, "BIC eller kontonummer saknas. Kontrollera att rätt typ av betalmetod är vald och försök igen."));

                            var postalAddressIncCountry = Utilities.GetAddressPart(supplierContactAddress, TermGroup_SysContactAddressRowType.PostalAddress) + " " + Utilities.GetAddressPart(supplierContactAddress, TermGroup_SysContactAddressRowType.Country);

                            foreignSection.Posts.Add(new LbForeignNamePost(invoice.ActorId.Value, supplier.Name, string.Empty));
                            foreignSection.Posts.Add(new LbForeignAddressPost(invoice.ActorId.Value, Utilities.GetAddressPart(supplierContactAddress, TermGroup_SysContactAddressRowType.Address), postalAddressIncCountry, currencyAccountCode, country, debitCode, paymentType, paymentMethodId));

                            foreignSection.Posts.Add(new LbForeignBankPost(invoice.ActorId.Value, id1, id2, id3));
                            foreignSection.Posts.Add(new LbForeignPaymentPost(transactionCode, invoice.ActorId.Value, invoice.InvoiceNr, Math.Abs(Utilities.GetAmount(paymentRow.Amount)), currencyTermAccountnumber, CountryCurrencyManager.GetCurrencyCode(sysCurrencyId), Utilities.GetDate(invoice.DueDate.Value), accountClosingCode, Math.Abs(Utilities.GetAmount(paymentRow.AmountCurrency)), String.Empty, conversion));
                            foreignSection.Posts.Add(new LbForeignRiksbankPost(invoice.ActorId.Value, rbCode));
                            //#endregion

                            totalCount++;
                            totalSum += paymentRow.AmountCurrency;
                            totalSumDomestic += paymentRow.Amount;
                            totalSumReceiverNumber += Convert.ToDecimal(invoice.ActorId.Value);

                            #endregion
                            break;
                    }
                }

                #region Sections

                if (foreignSection != null)
                {
                    foreignSection.Posts.Add(new LbForeignSummaryPost(senderBgc, Utilities.GetAmount(totalSumDomestic), Utilities.GetAmount(totalSumReceiverNumber), totalCount, Utilities.GetAmount(totalSum)));
                    fileObject.contents.Add(foreignSection);
                }
                if (paymentSection != null)
                {
                    var negate = "";
                    if (totalSum < 0)
                        negate = "-";
                    paymentSection.Posts.Add(new LbDomesticSummaryPost(senderBgc, totalCount, Utilities.GetAmount(totalSum), negate));
                    fileObject.contents.Add(paymentSection);
                }

                #endregion
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result.Exception = ex;
            }

            if (result.Success && fileObject.IsValid())
                result.Value = fileObject;

            return result;
        }

        private int ActorIdToBgCode(int? actorId, string bankAccountNrFallback)
        {
            int syntheticBgCode = 0;
            // 1. Use bank account nr since this is how it's done in SOP (maybe not perfect?)
            // 2. Use supplierId
            bool parseSuccessful = int.TryParse(actorId.ToString().SubstringFromEnd(5), out syntheticBgCode);
            if (!parseSuccessful)
                parseSuccessful = int.TryParse(bankAccountNrFallback.Skip(4).Take(5).JoinToString(), out syntheticBgCode);
            return syntheticBgCode;
        }

        /// <summary>
        /// Writes the lbfile to memory and returns a byte array
        /// </summary>
        /// <param name="fileObject"></param>
        /// <returns></returns>
        private byte[] WriteFileToMemory(LbFile fileObject)
        {
            StreamWriter sw = null;
            MemoryStream ms = new MemoryStream();

            try
            {
                //FileStream file = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read);
                sw = new StreamWriter(ms, Constants.ENCODING_LATIN1);

                foreach (ILbFileContent item in fileObject.contents)
                {
                    if (!(item is LbPaymentSection))
                        continue;

                    foreach (ILbFileContent content in ((LbPaymentSection)item).Posts)
                    {
                        if (content is LbDomesticPostGroup)
                        {
                            foreach (ILbPostGroupItem groupItem in ((LbDomesticPostGroup)content).Posts)
                            {
                                sw.WriteLine(groupItem.ToString());
                            }
                        }
                        else
                        {
                            sw.WriteLine(content.ToString());
                        }
                    }
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

        #region Create artifacts

        private ActionResult AddPaymentImport(CompEntities entities, string fileName, PaymentRow paymentRow, TransactionScope transaction)
        {
            if (paymentRow == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull);

            var result = new ActionResult(true);

            var originalPaymentRow = PaymentManager.GetPaymentRow(entities, paymentRow.PaymentRowId);
            if (originalPaymentRow == null)
                return new ActionResult(false, (int)ActionResultSave.EntityNotFound, "PaymentRow");

            // Set payment row verified
            paymentRow.Status = (int)SoePaymentStatus.Verified;

            var paymentImport = new PaymentImport
            {
                ImportDate = DateTime.Now,
                Filename = fileName,
            };
            SetCreatedProperties(paymentImport);

            if (!originalPaymentRow.PaymentImportReference.IsLoaded)
                originalPaymentRow.PaymentImportReference.Load();

            result = UpdateEntityItem(entities, originalPaymentRow, paymentRow, "PaymentRow");

            originalPaymentRow = PaymentManager.GetPaymentRow(entities, paymentRow.PaymentRowId);
            originalPaymentRow.PaymentImport = new PaymentImport
            {
                ImportDate = DateTime.Now,
                Filename = fileName,
            };

            SetCreatedProperties(paymentImport);

            if (result.Success)
                return SaveEntityItem(entities, originalPaymentRow, transaction);
            return result;
        }
        /*
        private ActionResult AddPaymentExport(CompEntities entities, TransactionScope transaction, string fileName, Payment payment)
        {
            var result = new ActionResult(true);

            var paymentExport = new PaymentExport
            {
                ExportDate = DateTime.Now,
                Filename = fileName,
                NumberOfPayments = payment.PaymentRow.Count,
                CustomerNr = String.Empty,
                Type = (int)TermGroup_PaymentMethod.LB,
            };
            SetCreatedProperties(paymentExport);

            payment.PaymentExport = paymentExport;

            //NI 090226: (EF bug?)
            //Dont save here because its in a existing transaction that will be saved later. 
            //Otherwise it will cause duplicate saves
            //return SaveEntityItem(entities, payment, true, transaction);
            return result;
        }
        */
        #endregion

        #region SysLbError

        /// <summary>
        /// Get all SysLbError's
        /// Accessor for SysDbCache
        /// </summary>
        /// <returns></returns>
        public List<SysLbError> GetSysLbErrors()
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return sysEntitiesReadOnly.SysLbError
                            .ToList<SysLbError>();
        }

        #endregion
    }
}
