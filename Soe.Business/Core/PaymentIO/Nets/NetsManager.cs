using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Transactions;

namespace SoftOne.Soe.Business.Core.PaymentIO.Nets
{
    public class NetsManager : PaymentIOManager
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private NetsFile netsFile;

        #endregion

        #region Ctor

        public NetsManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Import

        //public ActionResult Import(StreamReader sr, int actorCompanyId, int userId, string fileName, Origin origin, bool templateIsSuggestion, int paymentMethodId)
        //{
        //    var result = new ActionResult(true);
        //    result.ErrorMessage = GetText(8076, "Filimport genomförd");

        //    sr.DiscardBufferedData();
        //    sr.BaseStream.Position = 0;

        //    try
        //    {
        //        netsFile = new NetsFile();

        //        bool newSection = false;
        //        int alternative = 0;

        //        while (!sr.EndOfStream)
        //        {
        //            string line = sr.ReadLine();
        //            if (string.IsNullOrEmpty(line))
        //                continue;

        //            line = Utilities.AddPadding(line, Utilities.BGC_LINE_MAX_LENGTH);
        //            string postCode = Utilities.GetPgPostCode(line).ToString();
        //            if (String.IsNullOrEmpty(postCode))
        //                continue;

        //            int tc = Convert.ToInt32(postCode);

        //            if (newSection)
        //            {
        //                netsFile.Sections.Add(new Section());
        //                newSection = false;
        //            }

        //            switch (tc)
        //            {
        //                case (int)NetsRecordType.StartTransmissionRecord:
        //                    if (alternative == 0)
        //                        alternative = 1;
        //                    netsFile.StartTransmissionRecord = new StartAssignmentRecord(line);
        //                    newSection = true;
        //                    break;
        //                case (int)NetsRecordType.StartAssignmentRecord: 
        //                    netsFile.Sections.Last().StartAssignmentRecord = new StartAssignmentRecord();
        //                    break;
        //                case (int)PgPostType.SummaryPost:
        //                    netsFile.Sections.Last().SummaryPost = new SummaryPost(line);
        //                    //newSection = true;
        //                    break;
        //                case (int)PgPostType.CreditAmountPost:
        //                    netsFile.Sections.Last().Posts.Add(new AmountPostCredit(line));
        //                    break;
        //                case (int)PgPostType.DebitAmountPost:
        //                    netsFile.Sections.Last().Posts.Add(new AmountPostDebit(line));
        //                    break;
        //                case (int)PgPostType.ReceiverIdentityPost:
        //                    if (alternative == 0) alternative = 2;
        //                    netsFile.Sections.Last().Posts.Add(new ReceiverIdentityPost(line));
        //                    break;
        //                case (int)NetsRecordType.TransactionRecord1:
        //                    netsFile.Sections.Last().Posts.Add(new ReceiverPost(line, false, alternative));
        //                    break;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        base.LogError(ex, this.log);
        //        result.Exception = ex;
        //        return result;
        //    }
        //    try
        //    {
        //        sr.DiscardBufferedData();
        //        sr.BaseStream.Position = 0;
        //        if (!netsFile.IsValid())
        //        {
        //            result.Success = false;
        //            result.ErrorNumber = (int)ActionResultSave.PaymentFilePgFailed;
        //            result.ErrorMessage = GetText(8079, "Filstrukturen är inte giltlig");
        //            return result;
        //        }

        //        result = ConvertNetsFileToEntity(netsFile, origin, paymentMethodId, fileName, actorCompanyId, userId, templateIsSuggestion);
        //        if (!result.Success)
        //            return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        base.LogError(ex, this.log);
        //        result.Exception = ex;
        //    }

        //    return result;
        //}

        //private ActionResult ConvertNetsFileToEntity(NetsFile fileObject, Origin origin, int paymentMethodId, string fileName, int actorCompanyId, int userId, bool templateIsSuggestion)
        //{
        //    var result = new ActionResult(true);

        //    List<PaymentRow> paymentRowsToAdd = new List<PaymentRow>();
        //    string paymentNumber = String.Empty;
        //    string invoiceNumber = String.Empty;

        //    using (var entities = new CompEntities())
        //    {
        //        try
        //        {
        //            entities.Connection.Open();

        //            #region Settings

        //            //VoucherSeries
        //            int customerInvoicePaymentSeriesTypeId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.CustomerPaymentVoucherSeriesType, userId, actorCompanyId);

        //            //AccountYear
        //            int accountYearId = SettingManager.GetIntSetting(entities, SettingMainType.UserAndCompany, (int)UserSettingType.AccountingAccountYear, userId, actorCompanyId);

        //            #endregion

        //            #region Prereq

        //            //PaymentMethod
        //            PaymentMethod paymentMethod = PaymentManager.GetPaymentMethod(entities, paymentMethodId, actorCompanyId, true);
        //            if (paymentMethod == null)
        //                return new ActionResult((int)ActionResultSave.EntityNotFound, "PaymentMethod");

        //            //Company
        //            Company company = CompanyManager.GetCompany(entities, actorCompanyId);
        //            if (company == null)
        //                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

        //            //Get default VoucherSerie for Payment for current AccountYear
        //            VoucherSeries voucherSerie = VoucherManager.GetVoucherSerieByType(entities, customerInvoicePaymentSeriesTypeId, accountYearId);
        //            if (voucherSerie == null)
        //                return new ActionResult((int)ActionResultSave.EntityNotFound, "VoucherSeries");

        //            #endregion

        //            foreach (Section section in fileObject.Sections)
        //            {
        //                #region Section

        //                foreach (IRecord post in section.Posts)
        //                {
        //                    string pgCurrencyCode = string.Empty;

        //                    switch (post.RecordType)
        //                    {
        //                        case (int)PgPostType.ReceiverPost:
        //                            #region ReceiverPost

        //                            var rPost = ((ReceiverPost)post);
        //                            paymentNumber = rPost.PaymentNumber.ToString();

        //                            #endregion
        //                            break;
        //                        case (int)PgPostType.OpeningPost:
        //                            #region OpeningPost

        //                            pgCurrencyCode = ((OpeningPost)post).CurrencyCodeAmount;

        //                            #endregion
        //                            break;
        //                        case (int)PgPostType.CreditAmountPost:
        //                        case (int)PgPostType.DebitAmountPost:
        //                            #region CreditAmountPost / DebitAmountPost

        //                            #region Invoice

        //                            Invoice invoice = origin.Invoice;
        //                            if (invoice.InvoiceNr == invoiceNumber || Utilities.GetInvoiceNrFromOCR(invoice.OCR) == invoiceNumber)
        //                            {
        //                                invoice = InvoiceManager.GetInvoice(entities, invoice.InvoiceId);
        //                            }
        //                            else
        //                            {
        //                                #region Dummy invoice, since no match was found and the payment needs to be saved

        //                                decimal rate = 0M;
        //                                int sysCurrencyId = pgCurrencyCode == TermGroup_Currency.EUR.ToString() ? (int)BgMaxCurrency.EUR : (int)BgMaxCurrency.SEK;
        //                                CompCurrency currency = CountryCurrencyManager.GetCompCurrency(entities, sysCurrencyId, actorCompanyId);
        //                                if (currency != null)
        //                                    rate = currency.RateToBase;

        //                                invoice = new Invoice()
        //                                {
        //                                    Type = (int)SoeInvoiceType.CustomerInvoice,
        //                                    BillingType = post.PostType == (int)PgPostType.CreditAmountPost ? (int)TermGroup_BillingType.Credit : (int)TermGroup_BillingType.Debit,
        //                                    InvoiceNr = invoiceNumber,
        //                                    PaymentNr = paymentNumber,
        //                                    SysPaymentTypeId = (int)TermGroup_SysPaymentType.PG,
        //                                    InvoiceDate = ((AmountPost)post).AccountDate,
        //                                    VoucherDate = ((AmountPost)post).AccountDate,
        //                                    DueDate = ((AmountPost)post).AccountDate,
        //                                    CurrencyDate = ((AmountPost)post).AccountDate,
        //                                    ReferenceYour = string.Empty,
        //                                    ReferenceOur = "payment from file, not matched",
        //                                    OCR = string.Empty,
        //                                    CurrencyRate = rate,
        //                                    TotalAmount = Utilities.GetAmount(((AmountPost)post).Amount),
        //                                    TotalAmountCurrency = invoice.TotalAmount / rate,
        //                                    VATAmount = 0,
        //                                    VATAmountCurrency = 0,
        //                                    VatType = 0,
        //                                };

        //                                #endregion
        //                            }

        //                            if (invoice == null)
        //                                break;

        //                            #endregion

        //                            #region PaymentRow

        //                            //Save payment row
        //                            PaymentRow paymentRow = new PaymentRow
        //                            {
        //                                Status = (int)SoePaymentStatus.Verified,
        //                                State = (int)SoeEntityState.Active,
        //                                SysPaymentTypeId = (int)TermGroup_SysPaymentType.PG,
        //                                PaymentNr = paymentNumber,
        //                                PayDate = ((AmountPost)post).AccountDate,
        //                                CurrencyRate = invoice.CurrencyRate,
        //                                CurrencyDate = invoice.CurrencyDate,
        //                                Amount = Utilities.GetAmount(((AmountPost)post).Amount),
        //                                BankFee = 0,
        //                                AmountDiff = 0,

        //                                //Set references
        //                                Invoice = invoice,
        //                            };
        //                            SetCreatedProperties(paymentRow);

        //                            //Set currency amounts
        //                            CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, paymentRow);

        //                            paymentRowsToAdd.Add(paymentRow);

        //                            #endregion

        //                            #endregion
        //                            break;
        //                    }
        //                }

        //                #endregion
        //            }

        //            using (var transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
        //            {
        //                if (result.Success && !templateIsSuggestion)
        //                {
        //                    Payment payment = new Payment()
        //                    {
        //                        //Set references
        //                        Origin = OriginManager.GetOrigin(entities, origin.OriginId),
        //                    };

        //                    Payment paymentOriginal = PaymentManager.GetPayment(entities, payment.PaymentId);

        //                    foreach (PaymentRow paymentRow in paymentRowsToAdd)
        //                    {
        //                        PaymentRowSaveDTO dto = new PaymentRowSaveDTO()
        //                        {
        //                            // Origin
        //                            OriginId = origin.OriginId,
        //                            OriginType = (SoeOriginType)origin.Type,
        //                            OriginStatus = (SoeOriginStatus)origin.Status,
        //                            OriginDescription = origin.Description,
        //                            VoucherSeriesId = voucherSerie.VoucherSeriesId,
        //                            AccountYearId = accountYearId,

        //                            // Invoice
        //                            InvoiceType = (SoeInvoiceType)paymentRow.Invoice.Type,
        //                            OnlyPayment = true,
        //                            BillingType = (TermGroup_BillingType)paymentRow.Invoice.BillingType,
        //                            ActorId = paymentRow.Invoice.ActorId.Value,
        //                            InvoiceNr = paymentRow.Invoice.InvoiceNr,
        //                            InvoiceDate = paymentRow.Invoice.InvoiceDate,
        //                            PaymentDate = paymentRow.Invoice.DueDate,
        //                            VoucherDate = paymentRow.Invoice.VoucherDate,
        //                            TotalAmount = paymentRow.Invoice.TotalAmount,
        //                            TotalAmountCurrency = paymentRow.Invoice.TotalAmountCurrency,
        //                            VatAmount = paymentRow.Invoice.VATAmount,
        //                            VatAmountCurrency = paymentRow.Invoice.VATAmountCurrency,
        //                            CurrencyId = paymentRow.Invoice.CurrencyId,
        //                            CurrencyRate = paymentRow.Invoice.CurrencyRate,
        //                            CurrencyDate = paymentRow.Invoice.CurrencyDate,
        //                            FullyPayed = paymentRow.Invoice.FullyPayed,

        //                            // PaymentImport
        //                            ImportDate = DateTime.Now,
        //                            ImportFilename = fileName,
        //                        };

        //                        if (!result.Success)
        //                            continue;

        //                        //If whole chain should be created, no match on invoice
        //                        if (paymentRow.IsAdded())
        //                        {
        //                            //save
        //                            result = PaymentManager.SavePaymentRow(entities, transaction, dto, null, company.ActorCompanyId, userId, false, true);
        //                        }
        //                        else
        //                        {
        //                            //Save paymentRows and paymentImports
        //                            payment.PaymentRow.Add(paymentRow);

        //                            //Save PaymentAccountRows
        //                            CustomerInvoice customerInvoice = InvoiceManager.GetCustomerInvoice(entities, paymentRow.Invoice.InvoiceId, loadActor: true, loadInvoiceRow: true, loadInvoiceAccountRow: true);
        //                            result = PaymentManager.AddPaymentAccountRowsFromCustomerInvoice(entities, paymentRow, paymentMethod, customerInvoice, company.ActorCompanyId);
        //                        }

        //                        if (!result.Success)
        //                            return result;
        //                    }

        //                    if (payment.IsAdded())
        //                        result = AddEntityItem(entities, payment, "Payment", transaction);
        //                }

        //                #region Deprecated, if used then update with storing unmatched entities
        //                /*
        //                if (result.Success && templateIsSuggestion)
        //                {
        //                    //Find existing payment
        //                    if (!origin.PaymentReference.IsLoaded)
        //                        origin.PaymentReference.Load();

        //                    Payment paymentSuggestion = paymentManager.GetPayment(entities, origin.Payment.PaymentId, true); ;
        //                    foreach (PaymentRow paymentRow in paymentSuggestion.PaymentRow)
        //                    {
        //                        foreach (PaymentRow paymentRowImported in paymentRowsToAdd)
        //                        {
        //                            //Update fields
        //                            if (paymentRowImported.Invoice.InvoiceNr == paymentRow.Invoice.InvoiceNr)
        //                            {
        //                                paymentRow.PayDate = paymentRowImported.PayDate;
        //                                paymentRow.Status = (int)SoePaymentStatus.ManualPayment;
        //                                paymentRow.AmountCurrency = paymentRowImported.AmountCurrency;
        //                                paymentRow.AmountDiff = paymentRowImported.AmountDiff;
        //                            }
        //                        }
        //                    }
        //                    SaveEntityItem(entities, paymentSuggestion, true);
        //                }
        //                */
        //                #endregion

        //                //Commit transaction
        //                if (result.Success)
        //                    transaction.Complete();
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            base.LogError(ex, this.log);
        //            result.Exception = ex;
        //        }
        //        finally
        //        {
        //            if (result.Success)
        //            {
        //                //Set success properties
        //            }
        //            else
        //                base.LogTransactionFailed(this.ToString(), this.log);

        //            entities.Connection.Close();
        //        }
        //    }
        //    //TODO: somewhere the message is reset, locate correct and remove this check
        //    if (result.Success && string.IsNullOrEmpty(result.ErrorMessage))
        //        result.ErrorMessage = GetText(8076, "Filimport genomförd");

        //    return result;
        //}

        #endregion

        #region Export

        /// <summary>
        /// Exports netsFile to stream for save dialog
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
                result = ConvertEntityToNetsFile(entities, paymentMethod, paymentRows, paymentId, actorCompanyId);
                if (!result.Success)
                    return result;

                netsFile = result.Value as NetsFile;
                if (netsFile == null)
                    return result;
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result.Exception = ex;
                return result;
            }

            string guid = Guid.NewGuid().ToString("N");
            var fileName = Utilities.GetNetsFileNameOnServer(guid);
            var customerNr = netsFile.StartTransmissionRecord.DataSender;

            byte[] file = WriteFileToMemory(netsFile);
            if (file != null)
                result = CreatePaymentExport(fileName, paymentRows, TermGroup_SysPaymentMethod.Nets, customerNr, guid, file);
            else
                result.Success = false;

            return result;
        }

        private ActionResult ConvertEntityToNetsFile(CompEntities entities, PaymentMethod paymentMethod, IEnumerable<PaymentRow> paymentRows, int paymentId, int actorCompanyId)
        {
            var result = new ActionResult(true);
            var fileObject = new NetsFile();

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
                IRecord post = null;

                var sysCurrencyId = (int)PgCurrency.Undefined;
                
                string customerNr = String.Empty;
                string agreementId = String.Empty;
                string dataSender = paymentMethod.CustomerNr;
                char[] separator = new char[] {','};
                
                string[] separatedDataSender = dataSender.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                if (separatedDataSender.Length > 1)
                {
                    customerNr = separatedDataSender[0];
                    agreementId = separatedDataSender[1];
                }


                
                string senderAccount = String.Empty;
                decimal totalSum = 0M;
                int totalCount = 0;
                DateTime earliesDate = DateTime.MinValue;
                DateTime latestDate = DateTime.MaxValue;

                foreach (PaymentRow paymentRow in paymentRows)
                {
                    bool addReceiverPost = true;
                    IRecord receiverPost = null;
                    totalCount++;

                    string paymentNumber = Utilities.PaymentNumberLong(paymentRow.PaymentNr);
                    if (paymentNumber == String.Empty)
                    {
                        throw new ActionFailedException((int)ActionResultSave.PaymentInvalidAccountNumber);
                    }

                    if (paymentRow.Invoice != null)
                        invoice = paymentRow.Invoice;

                    if (!invoice.CurrencyReference.IsLoaded)
                        invoice.CurrencyReference.Load();

                    if (sysCurrencyId == (int)NetsCurrency.Undefined)
                    {
                        if (invoice != null)
                        {
                            switch (invoice.Currency.SysCurrencyId)
                            {
                                case (int)NetsCurrency.NOK:
                                    break;
                                default:
                                    result.Success = false;
                                    result.ErrorNumber = (int)ActionResultSave.NothingSaved;
                                    result.ErrorMessage = GetText(8080, "Valuta ej giltig för NETS");
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
                        string invoiceOCRorNr = !String.IsNullOrEmpty(invoice.OCR) ? invoice.OCR : invoice.InvoiceNr;
                        int sysPaymentTypeId = Utilities.GetNetsPaymentMethod(paymentRow.SysPaymentTypeId);
                        if (sysPaymentTypeId == 0)
                            continue;

                        string currencyCode = CountryCurrencyManager.GetCurrencyCode(sysCurrencyId);
                        string currencyCodePocket = currencyCode; //n/a
                        int seqNr = paymentRow.SeqNr;

                        if (String.IsNullOrEmpty(currencyCode))
                            continue;

                        var tmpSenderAccount = Utilities.GetSenderAccountNumberNets(entities, company, TermGroup_SysPaymentMethod.Nets);
                        if (tmpSenderAccount == String.Empty)
                            //Jesper was here
                            return new ActionResult(false, (int)ActionResultSave.EntityNotFound, "Betalningsmetod innehåller inte något betalningsnummer");

                        //if (senderAccount != tmpSenderAccount || sysCurrencyId != invoice.Currency.SysCurrencyId)
                        //    newSection = true;
                        senderAccount = tmpSenderAccount;

                        if (section == null) //create first section
                        {
                            #region Create File's Opening Posts
                            DateTime openingDate = DateTime.Now;
                            int productionNumber = GetProductionNumber(customerNr, openingDate, entities);

                            //firstSectionCreated = true;
                            fileObject.StartTransmissionRecord = new StartTransmissionRecord(customerNr, productionNumber);
                            fileObject.StartAssignmentRecord = new StartAssignmentRecord(agreementId, productionNumber, senderAccount.ToString());
                            section = new Section();
                            #endregion

                        }
                        switch (invoice.BillingType)
                        {
                            case (int)TermGroup_BillingType.Credit:
                                //DateTime firstAccountDate = invoice.DueDate.Value;
                                //DateTime lastAccountDate = invoice.DueDate.Value;
                                //post = new AmountPostCredit(sysPaymentTypeId, currencyCode, paymentNumber, message, Math.Abs(Utilities.GetAmount(invoice.TotalAmount)), firstAccountDate, lastAccountDate);
                                break;
                            case (int)TermGroup_BillingType.Debit:
                                DateTime date = paymentRow.PayDate;
                                string verificationNumber = paymentId.ToString();
                                totalSum += invoice.TotalAmount;
                                if (earliesDate == DateTime.MinValue || date < earliesDate)
                                    earliesDate = date;
                                if (latestDate == DateTime.MaxValue || date > latestDate)
                                    latestDate = date;

                                post = new TransactionRecord1(totalCount, date, paymentNumber,invoice.TotalAmount * 100, invoiceOCRorNr);
                                receiverPost = new TransactionRecord2(totalCount, invoice.Actor.Supplier.Name, seqNr.ToString(), invoice.SeqNr.Value.ToString());
                                break;
                        }

                       

                      

                        //if (newSection && !firstSectionCreated)
                        //{
                        //    //store previous
                        //    section.EndAssignmentRecord = summaryPost;
                        //    fileObject.Sections.Add(section);

                        //    //new section
                        //    section = CreateSenderPost(company, customerNr, senderAccount, currencyCode, currencyCodePocket);
                        //    newSection = false;
                        //}

                        //var receiverName = company.Name;
                        //totalSum += invoice.TotalAmount;
                        //summaryPost = new EndAssignmentRecord(customerNr, senderAccount, Utilities.GetAmount(totalSum), Utilities.GetAmount(totalCount), currencyCodePocket, currencyCode);
                    }

                    if (section != null && post != null)
                    {
                        section.Posts.Add(post);
                        if (addReceiverPost)
                            section.Posts.Add(receiverPost);
                    }
                }
                //Add last section
                if (section != null)
                {
                    fileObject.Sections.Add(section);
                    fileObject.EndAssignmentRecord = new EndAssignmentRecord(totalSum * 100, totalCount, totalCount * 2 + 2,earliesDate, latestDate);
                    fileObject.EndTransmissionRecord = new EndTransmissionRecord(totalSum * 100, totalCount, totalCount * 2 + 4, earliesDate);
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
        private byte[] WriteFileToMemory(NetsFile fileObject)
        {
            StreamWriter sw = null;
            MemoryStream ms = new MemoryStream();

            try
            {
                sw = new StreamWriter(ms, Constants.ENCODING_LATIN1);
                
                sw.WriteLine(fileObject.StartTransmissionRecord.ToString());
                sw.WriteLine(fileObject.StartAssignmentRecord.ToString());
                foreach (Section section in fileObject.Sections)
                {
                   
                    foreach (IRecord post in section.Posts)
                    {
                        sw.WriteLine(post.ToString());
                    }
                    
                }
                sw.WriteLine(fileObject.EndAssignmentRecord.ToString());
                sw.WriteLine(fileObject.EndTransmissionRecord.ToString());
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
