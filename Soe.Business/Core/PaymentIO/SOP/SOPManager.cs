using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Transactions;
namespace SoftOne.Soe.Business.Core.PaymentIO.SOP
{
    public class SOPManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //private BgMaxFile bgMaxFile;

        #endregion

        #region Ctor

        public SOPManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Import

        public ActionResult Import(StreamReader sr, int actorCompanyId, string fileName, List<Origin> origins, bool templateIsSuggestion, int paymentMethodId, ref List<string> log)
        {
            var result = new ActionResult(true);
            result.ErrorMessage = GetText(8076, "Filimport genomförd");

            sr.DiscardBufferedData();
            sr.BaseStream.Position = 0;

            try
            {
                result = ConvertSOPFileToEntity(sr, actorCompanyId, origins, paymentMethodId, fileName, templateIsSuggestion, ref log);
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

        private ActionResult ConvertSOPFileToEntity(StreamReader sr, int actorCompanyId, List<Origin> origins, int paymentMethodId, string fileName, bool templateIsSuggestion, ref List<string> logText)
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

                    if (!company.PaymentMethod.IsLoaded)
                        company.PaymentMethod.Load();

                    //Get default VoucherSerie for Payment for current AccountYear
                    VoucherSeries voucherSerie = VoucherManager.GetVoucherSerieByType(entities, customerInvoicePaymentSeriesTypeId, accountYearId);
                    if (voucherSerie == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "VoucherSeries");

                    #endregion

                    string bgCurrencyCode = String.Empty;
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        if (string.IsNullOrEmpty(line))
                            continue;
                        #region Section
                        string[] parts = line.Split(new char[] { ';' });
                        string paymentNr = String.Empty;
                        //bgCurrencyCode = section.PaymentStart.CurrencyCode.Trim();


                        #endregion

                        #region InvoiceNr

                        int invoiceNr = 0;
                        Int32.TryParse(parts[1], out invoiceNr);
                        decimal amount = 0M;
                        Decimal.TryParse(parts[4], out amount);
                        DateTime date;
                        DateTime.TryParse(parts[2], out date);

                        if (invoiceNr == 0)
                            break;

                        #endregion

                        #region Invoice

                        Invoice invoice = null;

                        origin = origins.FirstOrDefault(o => Convert.ToInt32(o.Invoice.InvoiceNr) == invoiceNr);
                        if (origin == null)
                        {
                            logText.Add(GetText(7195, "Underlaget kunde inte hittas för faktura med nummer") + ": " + invoiceNr + ".");
                            continue;
                        }

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

                            if (invoice.PaymentRow.Count > 0)
                            {
                                decimal sum = invoice.PaymentRow.Where(p => p.State == (int)SoeEntityState.Active).Select(p => p.AmountCurrency).Sum();
                                if (invoice.TotalAmountCurrency < (sum + amount))
                                {
                                    logText.Add(GetText(7197, "Faktura med nummer") + ": " + invoiceNr + " " + GetText(7198, "prickades ej av") + ". " + GetText(7199, "Beloppet på betalningar överstiger fakturans totalbelopp") + ".");
                                    continue;
                                }
                            }
                        }

                        foreach (OriginInvoiceMapping oimap in origin.OriginInvoiceMapping.Where(m => m.Type == (int)SoeOriginInvoiceMappingType.CustomerPayment))
                        {
                            Invoice originInvoice = oimap.Invoice;
                            invoice = originInvoice;
                        }

                        #endregion

                        #region PaymentRow

                        //Save payment row

                        PaymentRow paymentRow = new PaymentRow
                        {
                            Status = (int)SoePaymentStatus.Verified,
                            State = (int)SoeEntityState.Active,
                            SysPaymentTypeId = (int)TermGroup_SysPaymentType.Bank,
                            PaymentNr = paymentNr,
                            CurrencyRate = invoice.CurrencyRate,
                            CurrencyDate = invoice.CurrencyDate,
                            Amount = amount,
                            BankFee = 0,
                            AmountDiff = 0,
                            PayDate = date,

                            //Set references
                            Invoice = invoice,
                        };
                        SetCreatedProperties(paymentRow);

                        //Set currency amounts
                        CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, paymentRow);

                        paymentRowsToAdd.Add(paymentRow);



                        ////Set payment dates
                        //foreach (PaymentRow paymentRow in paymentRowsToAdd)
                        //{
                        //    //Don't set previous groups dates
                        //    if (paymentRow.PayDate < CalendarUtility.DATETIME_DEFAULT)
                        //    {
                        //        paymentRow.PayDate = section.PaymentEnd.PaymentDate;

                        //        //Propogate to newly created invoices
                        //        if (paymentRow.Invoice.IsAdded())
                        //        {
                        //            DateTime date = section.PaymentEnd.PaymentDate;
                        //            paymentRow.Invoice.DueDate = date;
                        //            paymentRow.Invoice.CurrencyDate = date;
                        //            paymentRow.Invoice.VoucherDate = date;
                        //        }
                        //    }
                        //}

                        #endregion
                    }

                    using (var transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        if (result.Success && !templateIsSuggestion)
                        {
                            int seqNr = SequenceNumberManager.GetNextSequenceNumber(entities, transaction, actorCompanyId, Enum.GetName(typeof(SoeOriginType), SoeOriginType.CustomerPayment), 1, false);

                            #region Origin payment

                            //Create payment Origin for the CustomerInvoice
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
                                if (customerInvoice.Origin.Status != (int)SoeOriginStatus.Origin &&
                                    customerInvoice.Origin.Status != (int)SoeOriginStatus.Voucher &&
                                    customerInvoice.ExportStatus == (int)SoeInvoiceExportStatusType.ExportedAndClosed)
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

                                if (!result.Success)
                                    return result;
                                else
                                    logText.Add(GetText(7196, "Betalning skapad för faktura med nummer") + ": " + paymentRow.Invoice.InvoiceNr + ".");

                                seqNr++;
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
                        //result.Value = logText;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            return result;
        }

        #endregion
    }
}
