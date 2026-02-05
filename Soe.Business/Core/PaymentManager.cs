using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.DTO.SignatoryContract;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using SoftOne.Soe.Util.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
    public class PaymentManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static private readonly TermGroup_SysPaymentMethod[] CommonSysPaymentMetods = { TermGroup_SysPaymentMethod.LB, TermGroup_SysPaymentMethod.PG, TermGroup_SysPaymentMethod.SEPA };
        static private readonly TermGroup_SysPaymentMethod[] CustomerSysPaymentMetods = { TermGroup_SysPaymentMethod.BGMax, TermGroup_SysPaymentMethod.TOTALIN, TermGroup_SysPaymentMethod.Intrum, TermGroup_SysPaymentMethod.ISO20022, TermGroup_SysPaymentMethod.BBSOCR };
        static private readonly TermGroup_SysPaymentMethod[] SupplierSysPaymentMetods = { TermGroup_SysPaymentMethod.NordeaCA, TermGroup_SysPaymentMethod.Autogiro, TermGroup_SysPaymentMethod.Cash, TermGroup_SysPaymentMethod.Nets, TermGroup_SysPaymentMethod.Cfp, TermGroup_SysPaymentMethod.ISO20022 };
        static private readonly List<TermGroup_SysPaymentMethod> CustomerSysPaymentMetodsForImport = new List<TermGroup_SysPaymentMethod>() { TermGroup_SysPaymentMethod.BGMax, TermGroup_SysPaymentMethod.TOTALIN, TermGroup_SysPaymentMethod.PG, TermGroup_SysPaymentMethod.SEPA, TermGroup_SysPaymentMethod.Intrum, TermGroup_SysPaymentMethod.ISO20022, TermGroup_SysPaymentMethod.BBSOCR };
        static private readonly List<TermGroup_SysPaymentMethod> SupplierSysPaymentMetodsForImport = new List<TermGroup_SysPaymentMethod>() { TermGroup_SysPaymentMethod.LB, TermGroup_SysPaymentMethod.Cfp, TermGroup_SysPaymentMethod.ISO20022 };

        #endregion

        #region Ctor

        public PaymentManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Payment

        public List<Payment> GetPaymentsByCompany(int actorCompanyId, DateTime? selectionDate)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Payment.NoTracking();
            return GetPaymentsByCompany(entities, actorCompanyId, selectionDate);
        }

        public List<Payment> GetPaymentsByCompany(CompEntities entities, int actorCompanyId, DateTime? selectionDate)
        {
            if (selectionDate != null)
            {
                return (from p in entities.Payment
                            .Include("PaymentExport")
                            .Include("PaymentRow")
                        where p.Origin.ActorCompanyId == actorCompanyId &&
                        p.PaymentExport.Created >= selectionDate
                        select p).ToList();
            }
            else
            {
                return (from p in entities.Payment
                            .Include("PaymentExport")
                            .Include("PaymentRow")
                        where p.Origin.ActorCompanyId == actorCompanyId
                        select p).ToList();
            }
        }

        public List<Payment> GetPaymentsByPaymentExport(CompEntities entities, int paymentExportId, int actorCompanyId, bool loadInvoices = false)
        {
            if (loadInvoices)
            {
                return (from p in entities.Payment
                            .Include("PaymentExport")
                            .Include("PaymentRow")
                            .Include("PaymentRow.Invoice.Currency")
                        where p.PaymentExport.PaymentExportId == paymentExportId &&
                        p.Origin.ActorCompanyId == actorCompanyId
                        select p).ToList();
            }
            else
            {
                return (from p in entities.Payment
                            .Include("PaymentExport")
                            .Include("PaymentRow")
                        where p.PaymentExport.PaymentExportId == paymentExportId &&
                        p.Origin.ActorCompanyId == actorCompanyId
                        select p).ToList();
            }
        }

        public Payment GetPayment(int paymentId, bool loadPaymentRows = false, bool loadPaymentExport = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Payment.NoTracking();
            return GetPayment(entities, paymentId, loadPaymentRows, loadPaymentExport);
        }

        public Payment GetPayment(CompEntities entities, int paymentId, bool loadPaymentRows = false, bool loadPaymentExport = false)
        {
            if (loadPaymentRows && loadPaymentExport)
            {
                return (from p in entities.Payment
                            .Include("PaymentRow")
                            .Include("PaymentExport")
                        where p.PaymentId == paymentId
                        select p).FirstOrDefault();
            }
            else if (loadPaymentRows)
            {
                return (from p in entities.Payment
                            .Include("PaymentRow")
                        where p.PaymentId == paymentId
                        select p).FirstOrDefault();
            }
            else if (loadPaymentExport)
            {
                return (from p in entities.Payment
                            .Include("PaymentExport")
                        where p.PaymentId == paymentId
                        select p).FirstOrDefault();
            }
            else
            {
                return (from p in entities.Payment
                        where p.PaymentId == paymentId
                        select p).FirstOrDefault();
            }
        }

        public PaymentRow GetPaymentBySeqNr(int actorCompanyId, int seqNr)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Payment.NoTracking();
            return GetPaymentBySeqNr(entities, actorCompanyId, seqNr);
        }

        public PaymentRow GetPaymentBySeqNr(CompEntities entities, int actorCompanyId, int seqNr)
        {

            return (from p in entities.PaymentRow
                        .Include("Payment")
                        .Include("Payment.Origin")
                    where p.SeqNr == seqNr &&
                          p.Payment.Origin.ActorCompanyId == actorCompanyId
                    select p).FirstOrDefault();

        }

        public Payment GetPaymentByPaymentExport(CompEntities entities, int paymentExportId, bool loadPaymentRows)
        {
            Payment payment = null;

            if (loadPaymentRows)
            {
                payment = (from p in entities.Payment
                               .Include("PaymentExport")
                               .Include("PaymentRow")
                           where p.PaymentExport.PaymentExportId == paymentExportId
                           select p).FirstOrDefault();
            }
            else
            {
                payment = (from p in entities.Payment
                               .Include("PaymentExport")
                           where p.PaymentExport.PaymentExportId == paymentExportId
                           select p).FirstOrDefault();
            }

            return payment;
        }

        public Payment GetPaymentSuggestion(CompEntities entities, int actorCompanyId, int entity)
        {
            Payment payment = null;

            SequenceNumberRecord record = SequenceNumberManager.GetLastActiveSequenceNumberRecord(entities, actorCompanyId, entity);
            if (record != null)
            {
                PaymentRow row = (from p in entities.PaymentRow
                                      .Include("Payment.PaymentMethod")
                                  where p.PaymentRowId == record.RecordId &&
                                  p.Status == (int)SoePaymentStatus.Pending &&
                                  p.IsSuggestion
                                  select p).FirstOrDefault();

                if (row != null)
                    payment = row.Payment;
            }

            return payment;
        }

        public decimal CalculatePayment(int billingType, decimal totalAmount, decimal payAmount, decimal rowAmount)
        {
            if (billingType == (int)TermGroup_BillingType.Credit)
            {
                // Credited more than invoice total amount
                if (payAmount < totalAmount)
                    payAmount = totalAmount;
            }
            //else
            //{
            //    // Paid more than invoice total amount
            //    if (payAmount > totalAmount)
            //        payAmount = totalAmount;
            //}

            //Only support for one debt row. If multiple debt rows the amount on the first debt row will be the sum of all debt rows.
            bool isCredit = billingType == (int)TermGroup_BillingType.Credit;
            decimal amount = payAmount;

            // If a debit invoice is paid with a negative amount or a credit invoice is paid with a positive amount, negate it.
            // Shouldn't happen unless the user typed wrong.
            if ((!isCredit && rowAmount > 0) || (isCredit && rowAmount < 0))
                amount = Decimal.Negate(amount);

            return amount;
        }

        public decimal CalculateDiscount(int billingType, decimal diffAmount)
        {

            //Only support for one debt row. If multiple debt rows the amount on the first debt row will be the sum of all debt rows.
            bool isCredit = billingType == (int)TermGroup_BillingType.Credit;
            decimal amount = Decimal.Negate(diffAmount);

            // If a debit invoice is paid with a negative amount or a credit invoice is paid with a positive amount, negate it.
            // Shouldn't happen unless the user typed wrong.
            if ((!isCredit && diffAmount > 0) || (isCredit && diffAmount < 0))
                amount = Decimal.Negate(amount);

            return amount;
        }

        public int GetStandardPaymentType(object obj)
        {
            int paymentType = (int)TermGroup_SysPaymentType.Unknown;

            if (StringUtility.HasValue(obj))
            {
                string value = obj.ToString();

                if (TryParsePaymentType(value, TermGroup_SysPaymentType.BIC, out paymentType))
                    return paymentType;
                else if (TryParsePaymentType(value, TermGroup_SysPaymentType.BG, out paymentType))
                    return paymentType;
                else if (TryParsePaymentType(value, TermGroup_SysPaymentType.PG, out paymentType))
                    return paymentType;
                else if (TryParsePaymentType(value, TermGroup_SysPaymentType.Bank, out paymentType))
                    return paymentType;
                else if (TryParsePaymentType(value, TermGroup_SysPaymentType.SEPA, out paymentType))
                    return paymentType;
            }

            return paymentType;
        }

        private bool TryParsePaymentType(string value, TermGroup_SysPaymentType type, out int paymentType)
        {
            paymentType = (int)TermGroup_SysPaymentType.Unknown;
            bool valid = false;

            int id;
            Int32.TryParse(value, out id);
            if (value == type.ToString() || id == (int)type)
            {
                paymentType = (int)type;
                valid = true;
            }

            return valid;
        }

        public ActionResult SavePaymentFromSupplierPaymentGridDTO(List<SupplierPaymentGridDTO> items, SoeOriginStatusChange statusChange, DateTime? bulkPayDate, int paymentMethodId, int accountYearId, int actorCompanyId, bool sendPaymentFile)
        {
            using (CompEntities entities = new CompEntities())
            {
                return SavePaymentFromSupplierPaymentGridDTO(entities, ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, items, statusChange, bulkPayDate, paymentMethodId, accountYearId, actorCompanyId, sendPaymentFile);
            }
        }

        public ActionResult SavePaymentFromSupplierPaymentGridDTO(CompEntities entities, TransactionScopeOption transactionScopeOption, List<SupplierPaymentGridDTO> items, SoeOriginStatusChange statusChange, DateTime? bulkPayDate, int paymentMethodId, int accountYearId, int actorCompanyId, bool sendPaymentFile)
        {
            if (items == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "SupplierPaymentGridDTO");

            // Default result is successful
            ActionResult result = new ActionResult(true);

            #region Prereq

            foreach (var item in items)
            {
                if (bulkPayDate.HasValue)
                    item.PayDate = bulkPayDate.Value;
                else if (item.PayDate == null)
                {
                    item.PayDate = item.DueDate >= DateTime.Today ? item.DueDate : DateTime.Today;
                }
            }

            List<SysCountry> sysCountries = SysDbCache.Instance.SysCountrys;
            List<SysCurrency> sysCurrencies = SysDbCache.Instance.SysCurrencies;
            #endregion

            PaymentExport paymentExport = null;
            List<PaymentRow> paymentRows = new List<PaymentRow>();
            Dictionary<int, int> voucherHeadsDict = null;

            string paymentExportGuid = "";
            int paymentIOType = 0;
            bool updateSuggestionSequenceNr = false;
            var companyGuid = CompanyManager.GetCompanyGuid(actorCompanyId);

            try
            {
                if (entities.Connection.State != ConnectionState.Open)
                    entities.Connection.Open();

                #region Prereq non transaction

                //CustomerInvoice transfer to Voucher
                int supplierInvoicePaymentSeriesTypeId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierPaymentVoucherSeriesType, 0, actorCompanyId, 0);

                //Get currency
                int baseSysCurrencyId = CountryCurrencyManager.GetCompanyBaseSysCurrencyId(entities, base.ActorCompanyId);

                #endregion

                // Possible to include this method in a running Transaction
                using (TransactionScope transaction = new TransactionScope(transactionScopeOption, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                {

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
                    VoucherSeries voucherSerie = VoucherManager.GetVoucherSerieByType(entities, supplierInvoicePaymentSeriesTypeId, accountYearId);
                    if (voucherSerie == null)
                    {
                        string errorMessage = $"Voucher series error \nSavePaymentFromSupplierPaymentGridDTO failed \nsupplierInvoicePaymentSeriesTypeId: {supplierInvoicePaymentSeriesTypeId} \nAccountYearId: {accountYearId} \nActorCompanyId: {actorCompanyId}";
                        base.LogError(errorMessage);
                        return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8403, "Verifikatserie saknas"));
                    }

                    //Get PaymentMethod
                    PaymentMethod paymentMethod = PaymentManager.GetPaymentMethod(entities, paymentMethodId, actorCompanyId, true);
                    if (paymentMethod == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "PaymentMethod");
                    if (paymentMethod.AccountStd == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountStd");

                    //Get payment suggestion sequencenumber - not using SoeEntityType since a new value will affect all views
                    //int paymentSuggestionNr = SequenceNumberManager.GetNextSequenceNumber(entities, actorCompanyId, "SupplierPaymentSuggestion", 1, false);
                    int paymentSuggestionNr = SequenceNumberManager.GetNextSequenceNumberCheckRecords(entities, actorCompanyId, (int)SoeEntityType.SupplierPaymentSuggestion, 1, "SupplierPaymentSuggestion", true);

                    #endregion

                    #region Origin payment

                    //Create payment Origin for the SupplierInvoice
                    Origin paymentOrigin = new Origin()
                    {
                        Type = (int)SoeOriginType.SupplierPayment,
                        Status = (int)SoeOriginStatus.Payment,

                        //Set references
                        Company = company,
                        VoucherSeries = voucherSerie,
                        VoucherSeriesTypeId = supplierInvoicePaymentSeriesTypeId,
                    };
                    SetCreatedProperties(paymentOrigin);

                    #endregion

                    #region Payment

                    //Check if open (suggestion) payment exists
                    var payment = GetPaymentSuggestion(entities, actorCompanyId, (int)SoeEntityType.SupplierPaymentSuggestion);

                    if (payment == null)
                    {
                        //Create Payment
                        payment = new Payment()
                        {
                            //Set references
                            Origin = paymentOrigin,
                            PaymentMethod = paymentMethod,
                        };
                    }
                    else
                    {
                        bool supplierUsePaymentSuggestion = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierUsePaymentSuggestions, 0, actorCompanyId, 0);
                        if (!supplierUsePaymentSuggestion)
                            return new ActionResult((int)ActionResultSave.PaymentSuggestionExistsOnCompSettDisabled, "Payment suggestions are found even thought the company does not use payment suggestion");
                    }

                    #endregion

                    List<PaymentRow> discountPaymentRows = new List<PaymentRow>();

                    foreach (var item in items)
                    {
                        #region Prereq

                        if (!item.PayDate.HasValue)
                            continue;

                        //Validate AccountYear
                        result = AccountManager.ValidateAccountYear(item.PayDate.Value, accountYear.From, accountYear.To);
                        if (!result.Success)
                        {
                            //If not valid, let us find if there is valid date for paydate
                            int OpenAccountYearId = AccountManager.GetAccountYearId(entities, item.PayDate.Value, actorCompanyId);
                            AccountYear NewAccountYear = AccountManager.GetAccountYear(entities, OpenAccountYearId, false);
                            if (NewAccountYear == null)
                            {
                                return new ActionResult((int)ActionResultSave.AccountYearNotFound, item.PayDate.ToString());
                            }

                            result = AccountManager.ValidateAccountYear(item.PayDate.Value, NewAccountYear.From, NewAccountYear.To);
                            if (!result.Success)
                                return result;
                            else
                            {
                                accountYear = NewAccountYear;
                                accountYearId = OpenAccountYearId;
                            }
                        }
                        //Validate AccountPeriod
                        AccountPeriod accountPeriod = AccountManager.GetAccountPeriod(entities, item.PayDate.Value, actorCompanyId);
                        result = AccountManager.ValidateAccountPeriod(accountPeriod, item.PayDate.Value);
                        if (!result.Success)
                            return result;

                        // Payment nr must be entered
                        if (String.IsNullOrEmpty(item.PaymentNr) && (statusChange == SoeOriginStatusChange.CustomerInvoice_OriginToPayment || statusChange == SoeOriginStatusChange.CustomerInvoice_OriginToPaymentForeign))
                            return new ActionResult((int)ActionResultSave.PaymentNrCannotBeEmpty);

                        #endregion

                        #region SupplierInvoice

                        //Get original and loaded SupplierInvoice
                        SupplierInvoice supplierInvoice = SupplierInvoiceManager.GetSupplierInvoice(entities, item.SupplierInvoiceId, true, true, true, false, true, true, true, false);
                        if (supplierInvoice == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "SupplierInvoice");

                        //Can only get status Payment from Origin or Voucher
                        if (supplierInvoice.Origin.Status != (int)SoeOriginStatus.Origin && supplierInvoice.Origin.Status != (int)SoeOriginStatus.Voucher)
                        {
                            result.Success = false;
                            result.ErrorNumber = (int)ActionResultSave.InvalidStateTransition;
                            return result;
                        }

                        //Must have SysPaymentTypeId
                        if (!supplierInvoice.SysPaymentTypeId.HasValue && statusChange == SoeOriginStatusChange.CustomerInvoice_OriginToPayment || statusChange == SoeOriginStatusChange.CustomerInvoice_OriginToPaymentForeign)
                        {
                            result.Success = false;
                            result.ErrorNumber = (int)ActionResultSave.PaymentSysPaymentTypeMissing;
                            return result;
                        }

                        //Check if invoice includes timediscount
                        bool hasTimeDiscount = false;
                        bool hasTimeDiscount2 = true;
                        if (item.TimeDiscountDate != null && item.TimeDiscountPercent != null)
                        {
                            if (Math.Abs(item.PayAmount) < Math.Abs(item.TotalAmount) &&
                                item.PayDate <= item.TimeDiscountDate)
                            {
                                hasTimeDiscount = true;
                            }
                        }

                        //PayAmount
                        if (!hasTimeDiscount)
                        {
                            if (item.TotalAmount >= 0)
                            {
                                decimal remainingAmount = supplierInvoice.TotalAmount - supplierInvoice.PaidAmount;
                                if (item.PayAmount > remainingAmount)
                                    item.PayAmount = remainingAmount;
                            }
                            else
                            {
                                decimal remainingAmount = supplierInvoice.TotalAmount - supplierInvoice.PaidAmount;
                                if (item.PayAmount < remainingAmount)
                                    item.PayAmount = remainingAmount;
                            }
                        }

                        //Add PaymentOrigin to SupplierInvoice
                        OriginInvoiceMapping oimap = new OriginInvoiceMapping()
                        {
                            Type = (int)SoeOriginInvoiceMappingType.SupplierPayment,

                            //Set referenes
                            Origin = paymentOrigin,
                        };
                        supplierInvoice.OriginInvoiceMapping.Add(oimap);

                        //Update supplier invoices
                        supplierInvoice.PaymentNr = item.PaymentNr;
                        supplierInvoice.SysPaymentTypeId = item.SysPaymentTypeId;

                        #endregion

                        #region PaymentRow

                        //Create PaymentRow for SupplierInvoice
                        PaymentRow paymentRow = new PaymentRow()
                        {
                            Status = (int)SoePaymentStatus.Pending,
                            SysPaymentTypeId = item.SysPaymentTypeId, // supplierInvoice.SysPaymentTypeId ?? 0,
                            PaymentNr = item.PaymentNr ?? string.Empty, // supplierInvoice.PaymentNr,
                            PayDate = item.PayDate.Value,
                            IsSuggestion = (statusChange == SoeOriginStatusChange.SupplierInvoice_OriginToPaymentSuggestion || statusChange == SoeOriginStatusChange.SupplierInvoice_OriginToPaymentSuggestionForeign) ? true : false,
                            CurrencyRate = supplierInvoice.CurrencyRate,
                            CurrencyDate = supplierInvoice.CurrencyDate,

                            //Set references
                            Invoice = supplierInvoice as Invoice,
                            Payment = payment,
                        };
                        SetCreatedProperties(paymentRow);

                        //SeqNr
                        paymentRow.SeqNr = SequenceNumberManager.GetNextSequenceNumber(entities, actorCompanyId, Enum.GetName(typeof(SoeOriginType), paymentOrigin.Type), 1, true);

                        //Set amount on payment depending on foreign or not
                        if (item.SysCurrencyId != baseSysCurrencyId)
                        {
                            //Validate pay amount. Same check in GUI
                            result = ValidatePaymentAmount(item.PayAmountCurrency, item.TotalAmountCurrency, item.PaymentSeqNr);
                            if (!result.Success)
                                return result;

                            //Set currency amounts
                            paymentRow.AmountCurrency = item.PayAmountCurrency;
                            paymentRow.BankFeeCurrency = 0;
                            if (hasTimeDiscount)
                                paymentRow.AmountDiffCurrency = item.PayAmountCurrency - item.TotalAmountCurrency;
                            else
                                paymentRow.AmountDiffCurrency = 0;
                            CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, paymentRow, true);
                        }
                        else
                        {
                            //Validate pay amount. Same check in GUI
                            result = ValidatePaymentAmount(item.PayAmount, item.TotalAmount, item.PaymentSeqNr);
                            if (!result.Success)
                                return result;

                            //Set amounts
                            paymentRow.Amount = item.PayAmount;
                            paymentRow.BankFee = 0;
                            if (hasTimeDiscount)
                                paymentRow.AmountDiff = item.PayAmount - (item.TotalAmount - item.PaidAmount);
                            else
                                paymentRow.AmountDiff = 0;
                            CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, paymentRow);
                        }

                        //PaidAmount
                        supplierInvoice.PaidAmount += paymentRow.Amount;
                        supplierInvoice.PaidAmountCurrency += paymentRow.AmountCurrency;

                        //FullyPayed
                        bool isTotalAmountPayed = supplierInvoice.IsTotalAmountPayed;
                        if (hasTimeDiscount)
                            isTotalAmountPayed = true;
                        InvoiceManager.SetInvoiceFullyPayed(entities, supplierInvoice, isTotalAmountPayed);

                        //Accounting rows
                        AddPaymentAccountRowsFromSupplierInvoice(entities, paymentRow, supplierInvoice, paymentMethod, actorCompanyId);

                        paymentRows.Add(paymentRow);

                        #region Paymentrows for time discounts

                        if (hasTimeDiscount && hasTimeDiscount2)
                        {
                            supplierInvoice.PaidAmount += paymentRow.AmountDiff * -1;
                            supplierInvoice.PaidAmountCurrency += paymentRow.AmountDiffCurrency * -1;
                            if (hasTimeDiscount2)
                            {
                                PaymentRow discountPaymentRow = new PaymentRow()
                                {
                                    Status = (int)SoePaymentStatus.Pending,
                                    SysPaymentTypeId = item.SysPaymentTypeId, // supplierInvoice.SysPaymentTypeId ?? 0,
                                    PaymentNr = item.PaymentNr ?? string.Empty, // supplierInvoice.PaymentNr,
                                    PayDate = item.PayDate.Value,
                                    IsSuggestion = (statusChange == SoeOriginStatusChange.SupplierInvoice_OriginToPaymentSuggestion || statusChange == SoeOriginStatusChange.SupplierInvoice_OriginToPaymentSuggestionForeign) ? true : false,
                                    CurrencyRate = supplierInvoice.CurrencyRate,
                                    CurrencyDate = supplierInvoice.CurrencyDate,

                                    //Set references
                                    Invoice = supplierInvoice as Invoice,
                                    //                  Payment = null,
                                    Payment = payment,

                                };
                                SetCreatedProperties(discountPaymentRow);

                                //SeqNr
                                discountPaymentRow.SeqNr = SequenceNumberManager.GetNextSequenceNumber(entities, actorCompanyId, Enum.GetName(typeof(SoeOriginType), paymentOrigin.Type), 1, true);
                                discountPaymentRow.IsCashDiscount = true;

                                //Set amount on payment depending on foreign or not
                                if (item.SysCurrencyId != baseSysCurrencyId)
                                {
                                    //Set currency amounts
                                    discountPaymentRow.AmountCurrency = paymentRow.AmountDiffCurrency * -1;
                                    discountPaymentRow.BankFeeCurrency = 0;
                                    discountPaymentRow.AmountDiffCurrency = 0;
                                    CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, discountPaymentRow, true);
                                }
                                else
                                {
                                    //Set amounts
                                    discountPaymentRow.Amount = paymentRow.AmountDiff * -1;
                                    discountPaymentRow.BankFee = 0;
                                    discountPaymentRow.AmountDiff = 0;
                                    CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, discountPaymentRow);
                                }

                                discountPaymentRows.Add(discountPaymentRow);
                            }
                        }


                        #endregion

                        #endregion

                        #region PaymentSuggestionSequence

                        if (statusChange == SoeOriginStatusChange.SupplierInvoice_OriginToPaymentSuggestion || statusChange == SoeOriginStatusChange.SupplierInvoice_OriginToPaymentSuggestionForeign)
                        {
                            updateSuggestionSequenceNr = true;
                            SequenceNumberManager.SaveSequenceNumberRecord(entities, (int)SoeEntityType.SupplierPaymentSuggestion, paymentRow.PaymentRowId, paymentSuggestionNr, actorCompanyId);
                        }

                        #endregion
                    }

                    #region Voucher

                    // Check if Voucher should also should be saved at once
                    if (result.Success && (statusChange != SoeOriginStatusChange.SupplierInvoice_OriginToPaymentSuggestion && statusChange != SoeOriginStatusChange.SupplierInvoice_OriginToPaymentSuggestionForeign))
                    {
                        result = TryTransferPaymentRowsToVoucher(entities, ConfigSettings.TRANSACTIONSCOPEOPTION_ATTACH, paymentRows, SoeOriginType.SupplierPayment, false, accountYearId, actorCompanyId);
                        if (result.Success && discountPaymentRows.Count > 0)
                            result = TryTransferPaymentRowsToVoucher(entities, ConfigSettings.TRANSACTIONSCOPEOPTION_ATTACH, discountPaymentRows, SoeOriginType.SupplierPayment, false, accountYearId, actorCompanyId);
                        if (result.Success)
                            voucherHeadsDict = NumberUtility.MergeDictictionary(voucherHeadsDict, result.IdDict);
                    }

                    #endregion

                    #region Paymentfile

                    if (result.Success && (statusChange != SoeOriginStatusChange.SupplierInvoice_OriginToPaymentSuggestion && statusChange != SoeOriginStatusChange.SupplierInvoice_OriginToPaymentSuggestionForeign))
                    {
                        if (payment == null)
                        {
                            result.Success = false;
                            result.ErrorNumber = (int)ActionResultSave.PaymentNotFound;
                        }

                        result = PaymentIOManager.Export(entities, sysCountries, sysCurrencies, transaction, payment, paymentMethod.SysPaymentMethodId, actorCompanyId, sendPaymentFile ? TermGroup_PaymentTransferStatus.PendingTransfer : TermGroup_PaymentTransferStatus.None, sendPaymentFile);
                        if (!result.Success)
                            return result;
                        else if (result.Value is PaymentExport)
                        {
                            paymentExport = (result.Value as PaymentExport);
                            var nrOfPayments = paymentExport.NumberOfPayments;
                            result.IntegerValue = nrOfPayments ?? 0;
                            if (sendPaymentFile)
                            {
                                var bankerResult = Banker.BankerConnector.UploadPaymentFile(SettingManager, paymentMethod, actorCompanyId, companyGuid, SysServiceManager.GetSysCompDBId(), CountryCurrencyManager.GetSysBank(paymentMethod.PaymentInformationRow.BIC), paymentExport.MsgId, paymentExport.Data);
                                if (!bankerResult.Success)
                                    return bankerResult;
                            }
                        }

                        //Save GUID and PaymentIOType to be able to open the file from disk later
                        paymentExportGuid = result.StringValue;
                        paymentIOType = result.IntegerValue;
                    }

                    #endregion                   

                    if (result.Success)
                    {
                        //update discount payment rows (after payment file is created)
                        foreach (var discountPaymentRow in discountPaymentRows)
                        {
                            discountPaymentRow.Payment = payment;
                        }

                        if (updateSuggestionSequenceNr)
                            SequenceNumberManager.UpdateSequenceNumber(entities, actorCompanyId, "SupplierPaymentSuggestion", paymentSuggestionNr);

                        result = SaveChanges(entities, transaction);
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
                    if (voucherHeadsDict != null)
                    {
                        result.IdDict = voucherHeadsDict;
                        //result.IntegerValue = voucherHeadsDict.Count;
                    }

                    result.IntDict = new Dictionary<int, int>();
                    foreach (var row in paymentRows)
                    {
                        result.IntDict.Add(row.SeqNr, row.PaymentRowId);
                    }
                }
                else
                    base.LogTransactionFailed(this.ToString(), this.log);

                if (transactionScopeOption != ConfigSettings.TRANSACTIONSCOPEOPTION_ATTACH)
                    entities.Connection.Close();
            }

            //Set GUID and PaymentIOType
            result.StringValue = paymentExportGuid;
            result.IntegerValue = paymentIOType;
            result.IntegerValue2 = paymentExport != null ? paymentExport.PaymentExportId : 0;
            return result;
        }

        public ActionResult SavePaymentFromSupplierInvoice(CompEntities entities, SupplierInvoice invoice, PaymentMethod paymentMethod, TermGroup_SysPaymentType paymentType, SoeOriginStatusChange statusChange, int accountYearId, int actorCompanyId, bool ignoreCreateVoucher, bool ignoreSuggestion, bool overrideAccountRowsValidation = false)
        {
            // Default result is successful
            ActionResult result = new ActionResult(true);

            #region Prere

            List<SysCountry> sysCountries = SysDbCache.Instance.SysCountrys;
            List<SysCurrency> sysCurrencies = SysDbCache.Instance.SysCurrencies;
            #endregion

            Payment payment = null;
            PaymentExport paymentExport = null;
            Dictionary<int, int> voucherHeadsDict = null;

            string paymentExportGuid = "";
            int paymentIOType = 0;
            bool updateSuggestionSequenceNr = false;

            try
            {
                if (entities.Connection.State != ConnectionState.Open)
                    entities.Connection.Open();

                #region Settings

                //CustomerInvoice transfer to Voucher
                int supplierInvoicePaymentSeriesTypeId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierPaymentVoucherSeriesType, 0, actorCompanyId, 0);

                #endregion

                #region Prereq

                //Get currency
                int baseSysCurrencyId = CountryCurrencyManager.GetCompanyBaseSysCurrencyId(entities, base.ActorCompanyId);

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
                VoucherSeries voucherSerie = VoucherManager.GetVoucherSerieByType(entities, supplierInvoicePaymentSeriesTypeId, accountYearId);
                if (voucherSerie == null)
                {
                    string errorMessage = $"Voucher series error \nSavePaymentFromSupplierInvoice failed \nsupplierInvoicePaymentSeriesTypeId: {supplierInvoicePaymentSeriesTypeId} \nAccountYearId: {accountYearId} \nActorCompanyId: {actorCompanyId}";
                    base.LogError(errorMessage);
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8403, "Verifikatserie saknas"));
                }

                //Get PaymentMethod
                if (!paymentMethod.AccountStdReference.IsLoaded)
                    paymentMethod.AccountStdReference.Load();
                if (paymentMethod == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "PaymentMethod");
                if (paymentMethod.AccountStd == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountStd");

                int paymentSuggestionNr = SequenceNumberManager.GetNextSequenceNumberCheckRecords(entities, actorCompanyId, (int)SoeEntityType.SupplierPaymentSuggestion, 1, "SupplierPaymentSuggestion", true);

                #endregion

                #region Origin payment

                //Create payment Origin for the SupplierInvoice
                Origin paymentOrigin = new Origin()
                {
                    Type = (int)SoeOriginType.SupplierPayment,
                    Status = (int)SoeOriginStatus.Payment,

                    //Set references
                    Company = company,
                    VoucherSeries = voucherSerie,
                    VoucherSeriesTypeId = supplierInvoicePaymentSeriesTypeId,
                };
                SetCreatedProperties(paymentOrigin);

                #endregion

                #region Payment

                //Check if open (suggestion) payment exists
                payment = GetPaymentSuggestion(entities, actorCompanyId, (int)SoeEntityType.SupplierPaymentSuggestion);

                if (payment == null || ignoreSuggestion)
                {
                    //Create Payment
                    payment = new Payment()
                    {
                        //Set references
                        Origin = paymentOrigin,
                        PaymentMethod = paymentMethod,
                    };
                }
                else
                {
                    bool supplierUsePaymentSuggestion = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierUsePaymentSuggestions, 0, actorCompanyId, 0);
                    if (!supplierUsePaymentSuggestion)
                        return new ActionResult((int)ActionResultSave.PaymentSuggestionExistsOnCompSettDisabled, "Payment suggestions are found even thought the company does not use payment suggestion");
                }

                #endregion

                List<PaymentRow> paymentRows = new List<PaymentRow>();

                #region Prereq
                if (!invoice.DueDate.HasValue)
                    invoice.DueDate = DateTime.Today;

                //Validate AccountYear
                result = AccountManager.ValidateAccountYear(invoice.DueDate.Value, accountYear.From, accountYear.To);
                if (!result.Success)
                {
                    //If not valid, let us find if there is valid date for paydate
                    int OpenAccountYearId = AccountManager.GetAccountYearId(entities, invoice.DueDate.Value, actorCompanyId);
                    AccountYear NewAccountYear = AccountManager.GetAccountYear(entities, OpenAccountYearId, false);
                    if (NewAccountYear == null)
                    {
                        return new ActionResult((int)ActionResultSave.AccountYearNotFound, invoice.DueDate.ToString());
                    }

                    result = AccountManager.ValidateAccountYear(invoice.DueDate.Value, NewAccountYear.From, NewAccountYear.To);
                    if (!result.Success)
                        return result;
                    else
                    {
                        accountYear = NewAccountYear;
                        accountYearId = OpenAccountYearId;
                    }
                }
                //Validate AccountPeriod
                AccountPeriod accountPeriod = AccountManager.GetAccountPeriod(entities, invoice.DueDate.Value, actorCompanyId);
                result = AccountManager.ValidateAccountPeriod(accountPeriod, invoice.DueDate.Value);
                if (!result.Success)
                    return result;

                // Payment nr must be entered
                if (String.IsNullOrEmpty(invoice.PaymentNr) && (statusChange == SoeOriginStatusChange.SupplierInvoice_OriginToPayment || statusChange == SoeOriginStatusChange.SupplierInvoice_OriginToPaymentForeign))
                    return new ActionResult((int)ActionResultSave.PaymentNrCannotBeEmpty);

                #endregion

                #region SupplierInvoice

                //Must have SysPaymentTypeId
                if (!invoice.SysPaymentTypeId.HasValue && statusChange == SoeOriginStatusChange.CustomerInvoice_OriginToPayment || statusChange == SoeOriginStatusChange.CustomerInvoice_OriginToPaymentForeign)
                {
                    result.Success = false;
                    result.ErrorNumber = (int)ActionResultSave.PaymentSysPaymentTypeMissing;
                    return result;
                }

                //Add PaymentOrigin to SupplierInvoice
                OriginInvoiceMapping oimap = new OriginInvoiceMapping()
                {
                    Type = (int)SoeOriginInvoiceMappingType.SupplierPayment,

                    //Set referenes
                    Origin = paymentOrigin,
                };
                invoice.OriginInvoiceMapping.Add(oimap);

                #endregion

                #region PaymentRow

                //Create PaymentRow for SupplierInvoice
                PaymentRow paymentRow = new PaymentRow()
                {
                    Status = (int)SoePaymentStatus.Pending,
                    SysPaymentTypeId = (int)paymentType, // supplierInvoice.SysPaymentTypeId ?? 0,
                    PaymentNr = invoice.PaymentNr ?? string.Empty, // supplierInvoice.PaymentNr,
                    PayDate = invoice.DueDate.Value,
                    IsSuggestion = (statusChange == SoeOriginStatusChange.SupplierInvoice_OriginToPaymentSuggestion || statusChange == SoeOriginStatusChange.SupplierInvoice_OriginToPaymentSuggestionForeign) ? true : false,
                    CurrencyRate = invoice.CurrencyRate,
                    CurrencyDate = invoice.CurrencyDate,

                    //Set references
                    Invoice = invoice as Invoice,
                    Payment = payment,
                };
                SetCreatedProperties(paymentRow);

                //SeqNr
                paymentRow.SeqNr = SequenceNumberManager.GetNextSequenceNumber(entities, actorCompanyId, Enum.GetName(typeof(SoeOriginType), paymentOrigin.Type), 1, true);

                //Set amount on payment depending on foreign or not
                Currency currency = CountryCurrencyManager.GetCurrency(entities, invoice.CurrencyId);
                if (currency.SysCurrencyId != baseSysCurrencyId)
                {
                    //Set currency amounts
                    paymentRow.AmountCurrency = (invoice.TotalAmountCurrency - invoice.PaidAmountCurrency);
                    paymentRow.BankFeeCurrency = 0;
                    paymentRow.AmountDiffCurrency = 0;
                    CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, paymentRow, true);
                }
                else
                {
                    //Set amounts
                    paymentRow.Amount = (invoice.TotalAmount - invoice.PaidAmount);
                    paymentRow.BankFee = 0;
                    paymentRow.AmountDiff = 0;
                    CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, paymentRow);
                }

                //PaidAmount
                invoice.PaidAmount += paymentRow.Amount;
                invoice.PaidAmountCurrency += paymentRow.AmountCurrency;

                //FullyPayed
                InvoiceManager.SetInvoiceFullyPayed(entities, invoice, true);

                //Accounting rows
                AddPaymentAccountRowsFromSupplierInvoice(entities, paymentRow, invoice, paymentMethod, actorCompanyId, overrideAccountRowsValidation);

                paymentRows.Add(paymentRow);

                #endregion

                #region PaymentSuggestionSequence

                if (statusChange == SoeOriginStatusChange.SupplierInvoice_OriginToPaymentSuggestion || statusChange == SoeOriginStatusChange.SupplierInvoice_OriginToPaymentSuggestionForeign)
                {
                    updateSuggestionSequenceNr = true;
                    SequenceNumberManager.SaveSequenceNumberRecord(entities, (int)SoeEntityType.SupplierPaymentSuggestion, paymentRow.PaymentRowId, paymentSuggestionNr, actorCompanyId);
                }

                #endregion

                #region Voucher

                // Check if Voucher should also should be saved at once - 
                if (result.Success && (statusChange != SoeOriginStatusChange.SupplierInvoice_OriginToPaymentSuggestion && statusChange != SoeOriginStatusChange.SupplierInvoice_OriginToPaymentSuggestionForeign) && !ignoreCreateVoucher)
                {
                    result = TryTransferPaymentRowsToVoucher(entities, ConfigSettings.TRANSACTIONSCOPEOPTION_ATTACH, paymentRows, SoeOriginType.SupplierPayment, false, accountYearId, actorCompanyId);
                    if (result.Success)
                        voucherHeadsDict = NumberUtility.MergeDictictionary(voucherHeadsDict, result.IdDict);
                }

                #endregion

                #region Paymentfile

                // Not needed
                /*if (result.Success && (statusChange != SoeOriginStatusChange.SupplierInvoice_OriginToPaymentSuggestion && statusChange != SoeOriginStatusChange.SupplierInvoice_OriginToPaymentSuggestionForeign))
                {
                    if (payment == null)
                    {
                        result.Success = false;
                        result.ErrorNumber = (int)ActionResultSave.PaymentNotFound;
                    }

                    result = PaymentIOManager.Export(entities, sysCountries, sysCurrencies, transaction, payment, paymentMethod.SysPaymentMethodId, actorCompanyId);
                    if (!result.Success)
                        return result;

                    else if (result.Value is SoftOne.Soe.Data.PaymentExport)
                    {
                        var nrOfPayments = (result.Value as SoftOne.Soe.Data.PaymentExport).NumberOfPayments;
                        result.IntegerValue = nrOfPayments ?? 0;
                    }

                    //Save GUID and PaymentIOType to be able to open the file from disk later
                    paymentExportGuid = result.StringValue;
                    paymentIOType = result.IntegerValue;

                    if (result.Value != null && result.Value.GetType() == typeof(PaymentExport))
                        paymentExport = (PaymentExport)result.Value;
                }*/

                #endregion

                if (result.Success)
                {
                    if (updateSuggestionSequenceNr)
                        SequenceNumberManager.UpdateSequenceNumber(entities, actorCompanyId, "SupplierPaymentSuggestion", paymentSuggestionNr);

                    result = SaveChanges(entities);
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
                    }
                }
                else
                    base.LogTransactionFailed(this.ToString(), this.log);
            }

            //Set GUID and PaymentIOType
            result.StringValue = paymentExportGuid;
            result.IntegerValue = paymentIOType;
            result.IntegerValue2 = paymentExport != null ? paymentExport.PaymentExportId : 0;
            return result;
        }

        public ActionResult SaveImportPaymentFromCustomerInvoice(List<PaymentImportIODTO> items, DateTime? bulkPayDate, int paymentMethodId, int accountYearId, int actorCompanyId, bool foreign, bool setErrorMessage = false)
        {
            using (CompEntities entities = new CompEntities())
            {
                return SaveImportPaymentFromCustomerInvoice(entities, ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, items, bulkPayDate, paymentMethodId, accountYearId, actorCompanyId, foreign, setErrorMessage);
            }
        }

        public ActionResult SaveImportPaymentFromCustomerInvoice(CompEntities entities, TransactionScopeOption transactionScopeOption, List<PaymentImportIODTO> items, DateTime? bulkPayDate, int paymentMethodId, int accountYearId, int actorCompanyId, bool foreign, bool setErrorMessage = false)
        {
            if (items == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "PaymentImportIODTO");

            var result = new ActionResult(true);
            Dictionary<int, int> voucherHeadsDict = null;

            try
            {
                if (entities.Connection.State != ConnectionState.Open)
                    entities.Connection.Open();

                #region Settings

                //VoucherSeries
                int customerInvoicePaymentSeriesTypeId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.CustomerPaymentVoucherSeriesType, 0, actorCompanyId, 0);
                bool addCustomerNameToInternalDescr = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.CustomerPaymentAddCustomerNameToInternaDescr, 0, actorCompanyId, 0);

                #endregion

                #region Prereq

                //Validate AccountYear
                AccountYear accountYear = AccountManager.GetAccountYear(entities, accountYearId);
                result = AccountManager.ValidateAccountYear(accountYear);
                if (!result.Success)
                {
                    if (setErrorMessage)
                        result.ErrorMessage = InvoiceManager.GetErrorMessage(result.ErrorNumber, string.Empty);
                    return result;
                }

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

                // Possible to include this method in a running Transaction
                using (TransactionScope transaction = new TransactionScope(transactionScopeOption, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                {

                    List<PaymentRow> paymentRows = new List<PaymentRow>();
                    AccountPeriod accountPeriod = null;

                    foreach (var item in items)
                    {
                        #region Prereq

                        item.ActorCompanyId = actorCompanyId;
                        //Validate AccountYear
                        result = AccountManager.ValidateAccountYear(item.PaidDate.Value, accountYear.From, accountYear.To);
                        if (!result.Success)
                        {
                            if (setErrorMessage)
                                result.ErrorMessage = InvoiceManager.GetErrorMessage(result.ErrorNumber, item.PaidDate.Value.ToShortDateString());
                            return result;
                        }

                        //Validate AccountPeriod
                        if (accountPeriod != null)
                        {
                            result = AccountManager.ValidateAccountPeriod(accountPeriod, item.PaidDate.Value);
                            if (!result.Success)
                            {
                                accountPeriod = null;
                            }
                        }

                        if (accountPeriod == null)
                        {
                            accountPeriod = AccountManager.GetAccountPeriod(entities, item.PaidDate.Value, actorCompanyId);
                            result = AccountManager.ValidateAccountPeriod(accountPeriod, item.PaidDate.Value);
                            if (!result.Success)
                            {
                                if (setErrorMessage)
                                    result.ErrorMessage = InvoiceManager.GetErrorMessage(result.ErrorNumber, item.PaidDate.Value.ToShortDateString());
                                return result;
                            }
                        }
                        #endregion

                        #region Origin payment

                        var paymentOrigin = new Origin
                        {
                            Type = (int)SoeOriginType.CustomerPayment,
                            Status = (int)SoeOriginStatus.Payment,
                            Description = addCustomerNameToInternalDescr ? item.Customer : "",

                            //Set references
                            VoucherSeries = voucherSerie,
                            ActorCompanyId = actorCompanyId,
                            VoucherSeriesTypeId = customerInvoicePaymentSeriesTypeId,
                        };

                        SetCreatedProperties(paymentOrigin);

                        #endregion

                        #region Payment

                        //Create Payment
                        var payment = new Payment
                        {
                            //Set references
                            Origin = paymentOrigin,
                            PaymentMethod = paymentMethod,
                            PaymentExport = null, //Set when export file is physical created
                        };

                        #endregion

                        #region CustomerInvoice

                        //Get original and loaded CustomerInvoice
                        //var customerInvoice = InvoiceManager.GetCustomerInvoice(entities, item.InvoiceId.Value, true, true, true, false, true, false, false, true, true, false, false, false);
                        var customerInvoice = InvoiceManager.GetCustomerInvoice(entities, item.InvoiceId.Value, true);
                        if (customerInvoice == null || !customerInvoice.ActorId.HasValue)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, string.Format(GetText(8427, "Faktura med fakturanr {0} kunde inte hittas"), 0));

                        var customerInvoiceAccountTypeRows = InvoiceManager.GetCustomerInvoiceAccountTypeRows(entities, item.InvoiceId.Value);

                        //Can only get status Payment from Origin or Voucher
                        if (customerInvoice.Origin.Status != (int)SoeOriginStatus.Origin && customerInvoice.Origin.Status != (int)SoeOriginStatus.Voucher &&
                            customerInvoice.ExportStatus == (int)SoeInvoiceExportStatusType.ExportedAndClosed)
                        {
                            result.Success = false;
                            result.ErrorNumber = (int)ActionResultSave.InvalidStateTransition;
                            return result;
                        }


                        //Add PaymentOrigin to CustomerInvoice
                        var oimap = new OriginInvoiceMapping
                        {
                            Type = (int)SoeOriginInvoiceMappingType.CustomerPayment,

                            //Set references
                            Origin = paymentOrigin,
                        };

                        customerInvoice.OriginInvoiceMapping.Add(oimap);

                        #endregion

                        #region PaymentRow

                        //Create PaymentRow for SupplierInvoice
                        var paymentRow = new PaymentRow
                        {
                            Status = (int)SoePaymentStatus.ManualPayment,
                            SysPaymentTypeId = 0,
                            PaymentNr = customerInvoice.PaymentNr ?? string.Empty,
                            PayDate = item.PaidDate.Value,
                            CurrencyRate = customerInvoice.CurrencyRate,
                            CurrencyDate = customerInvoice.CurrencyDate,

                            //Set references
                            Invoice = customerInvoice,
                            Payment = payment,
                        };

                        SetCreatedProperties(paymentRow);

                        //SeqNr
                        paymentRow.SeqNr = SequenceNumberManager.GetNextSequenceNumber(entities, actorCompanyId, Enum.GetName(typeof(SoeOriginType), paymentOrigin.Type), 1, true);

                        MatchCode matchCode = null;

                        //Set amount on payment depending on foreign or not
                        if (foreign)
                        {
                            //Set currency amounts
                            paymentRow.AmountCurrency = item.PaidAmount.Value;
                            paymentRow.BankFeeCurrency = 0;
                            paymentRow.AmountDiffCurrency = 0;
                            CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, paymentRow, true);
                        }
                        else
                        {
                            //Set currency amounts
                            paymentRow.Amount = item.PaidAmount.Value;
                            paymentRow.BankFee = 0;
                            paymentRow.AmountDiff = item.AmountDiff;

                            if (item.MatchCodeId.HasValue && item.MatchCodeId.Value > 0)
                            {
                                matchCode = InvoiceManager.GetMatchCode(entities, item.MatchCodeId.Value);
                            }
                            CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, paymentRow);
                        }

                        //PaidAmount
                        customerInvoice.PaidAmount += paymentRow.Amount;
                        customerInvoice.PaidAmountCurrency += paymentRow.AmountCurrency;

                        //FullyPayed
                        InvoiceManager.SetInvoiceFullyPayed(entities, customerInvoice, matchCode != null ? true : customerInvoice.IsTotalAmountPayed);

                        result = AddPaymentAccountRowsFromCustomerInvoice(entities, paymentRow, paymentMethod, customerInvoice, customerInvoiceAccountTypeRows, actorCompanyId, matchCode, !customerInvoice.IsTotalAmountPayed && matchCode != null);
                        if (!result.Success)
                            return result;

                        #region Payment matching

                        if (item.MatchCodeId.HasValue && item.MatchCodeId.Value != 0)
                        {
                            InvoiceMatchingDTO dto = new InvoiceMatchingDTO
                            {
                                PaymentRowId = paymentRow.PaymentRowId,
                                Type = (int)SoeEntityType.CustomerPayment,
                            };

                            result = InvoiceManager.AddInvoicePaymentMatch(entities, transaction, new List<InvoiceMatchingDTO>() { dto }, actorCompanyId, item.MatchCodeId.Value);
                        }

                        #endregion

                        paymentRows.Add(paymentRow);

                        // Check if additional payment should be added
                        if (customerInvoice.TotalAmountCurrency != customerInvoice.PaidAmountCurrency && (customerInvoice.IsTotalAmountPayed || matchCode != null))
                        {
                            var additionalOimap = new OriginInvoiceMapping
                            {
                                Type = (int)SoeOriginInvoiceMappingType.CustomerPayment,

                                //Set references
                                Origin = paymentOrigin,
                            };
                            customerInvoice.OriginInvoiceMapping.Add(additionalOimap);

                            var additionalPaymentRow = new PaymentRow
                            {
                                Status = (int)SoePaymentStatus.ManualPayment,
                                SysPaymentTypeId = 0,
                                PaymentNr = customerInvoice.PaymentNr ?? string.Empty,
                                PayDate = item.PaidDate.Value,
                                CurrencyRate = customerInvoice.CurrencyRate,
                                CurrencyDate = customerInvoice.CurrencyDate,

                                //Set references
                                Invoice = customerInvoice,
                                Payment = payment,
                            };
                            SetCreatedProperties(additionalPaymentRow);

                            //SeqNr
                            additionalPaymentRow.SeqNr = SequenceNumberManager.GetNextSequenceNumber(entities, actorCompanyId, Enum.GetName(typeof(SoeOriginType), paymentOrigin.Type), 1, true);

                            //Set amount on payment depending on foreign or not
                            if (foreign)
                            {
                                //Set currency amounts
                                additionalPaymentRow.AmountCurrency = (customerInvoice.TotalAmountCurrency - customerInvoice.PaidAmountCurrency);

                                additionalPaymentRow.BankFeeCurrency = 0;
                                additionalPaymentRow.AmountDiffCurrency = 0;
                                CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, additionalPaymentRow, true);
                            }
                            else
                            {
                                //Set currency amounts
                                additionalPaymentRow.Amount = (customerInvoice.TotalAmountCurrency - customerInvoice.PaidAmountCurrency);
                                additionalPaymentRow.BankFee = 0;
                                additionalPaymentRow.AmountDiff = 0;
                                CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, additionalPaymentRow);
                            }

                            if (!result.Success)
                                return result;

                            paymentRows.Add(additionalPaymentRow);
                        }

                        result = SaveChanges(entities);
                        if (!result.Success)
                            return result;

                        // Set to paid
                        if (item.PaymentImportIOId != 0 && (item.Status == ImportPaymentIOStatus.Match || item.Status == ImportPaymentIOStatus.Rest || (item.Status == ImportPaymentIOStatus.PartlyPaid && matchCode != null)))
                        {
                            item.Status = ImportPaymentIOStatus.Paid;
                        }

                        item.PaymentRowId = paymentRow.PaymentRowId;
                        item.State = ImportPaymentIOState.Closed;

                        InvoiceManager.UpdatePaymentImportIO(entities, item);

                        #endregion
                    }

                    #region Voucher

                    // Check if Voucher should also should be saved at once
                    if (result.Success)
                    {
                        result = TryTransferPaymentRowsToVoucher(entities, ConfigSettings.TRANSACTIONSCOPEOPTION_ATTACH, paymentRows, SoeOriginType.CustomerPayment, foreign, accountYearId, actorCompanyId);
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
                {
                    base.LogWarning($"SaveImportPaymentFromCustomerInvoice failed with error no: {result.ErrorNumber} message:{result.ErrorMessage} ");
                    base.LogTransactionFailed(this.ToString(), this.log);
                }

                if (transactionScopeOption != ConfigSettings.TRANSACTIONSCOPEOPTION_ATTACH)
                    entities.Connection.Close();
            }

            return result;
        }

        public ActionResult SavePaymentFromCustomerInvoice(List<CustomerInvoiceGridDTO> items, DateTime? bulkPayDate, int paymentMethodId, int actorCompanyId, bool foreign)
        {
            using (CompEntities entities = new CompEntities())
            {
                return SavePaymentFromCustomerInvoice(entities, ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, items, bulkPayDate, paymentMethodId, actorCompanyId, foreign);
            }
        }

        public ActionResult SavePaymentFromCustomerInvoice(CompEntities entities, TransactionScopeOption transactionScopeOption, List<CustomerInvoiceGridDTO> items, DateTime? bulkPayDate, int paymentMethodId, int actorCompanyId, bool foreign)
        {
            if (items == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "ChangeStatusGridView");

            // Default result is successful
            ActionResult result = new ActionResult(true);

            #region Prereq

            if (bulkPayDate.HasValue)
            {
                foreach (var item in items)
                {
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

                #region Settings

                //VoucherSeries
                int customerInvoicePaymentSeriesTypeId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.CustomerPaymentVoucherSeriesType, 0, actorCompanyId, 0);

                #endregion

                #region Prereq

                //Get PaymentMethod
                PaymentMethod paymentMethod = PaymentManager.GetPaymentMethod(entities, paymentMethodId, actorCompanyId, true);
                if (paymentMethod == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "PaymentMethod");
                if (paymentMethod.AccountStd == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountStd");

                #endregion

                // Possible to include this method in a running Transaction
                using (TransactionScope transaction = new TransactionScope(transactionScopeOption, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                {

                    var paymentRows = new List<PaymentRow>();
                    var paymentRowsPerAccountYear = new Dictionary<int, List<PaymentRow>>();

                    AccountYear accountYear = null;
                    VoucherSeries voucherSerie = null;
                    AccountPeriod accountPeriod = null;

                    foreach (var item in items.OrderBy(x => x.PayDate))
                    {
                        #region Prereq

                        //If insecure then skip to the next item
                        if (item.InsecureDebt && item.OriginType == (int)SoeOriginType.CustomerInvoice)
                            continue;

                        DateTime voucherDate = item.PayDate ?? DateTime.Today;
                        if (!AccountManager.ValidateAccountYear(accountYear, voucherDate).Success)
                        {
                            if (paymentRows.Any() && accountYear != null)
                            {
                                paymentRowsPerAccountYear.Add(accountYear.AccountYearId, paymentRows);
                                paymentRows = new List<PaymentRow>();
                            }

                            accountYear = AccountManager.GetAccountYear(entities, voucherDate, actorCompanyId);

                            result = AccountManager.ValidateAccountYear(accountYear);
                            if (!result.Success)
                                return result;

                            //Get default VoucherSerie for SupplierInvoice for current AccountYear
                            voucherSerie = VoucherManager.GetVoucherSerieByType(entities, customerInvoicePaymentSeriesTypeId, accountYear.AccountYearId);

                            if (voucherSerie == null)
                                return new ActionResult((int)ActionResultSave.EntityNotFound, "VoucherSeries");
                        }

                        //Validate AccountPeriod
                        if (!AccountManager.ValidateAccountPeriod(accountPeriod, voucherDate).Success)
                        {
                            accountPeriod = AccountManager.GetAccountPeriod(entities, accountYear.AccountYearId, voucherDate, actorCompanyId);
                            result = AccountManager.ValidateAccountPeriod(accountPeriod, voucherDate);
                            if (!result.Success)
                                return result;
                        }

                        #endregion

                        #region CustomerInvoice

                        //Get original and loaded CustomerInvoice
                        CustomerInvoice customerInvoice = InvoiceManager.GetCustomerInvoice(entities, item.CustomerInvoiceId, true, true, true, false, true, false, false, true, true, false, false, false);
                        if (customerInvoice == null || !customerInvoice.ActorId.HasValue)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "CustomerInvoice");

                        //Can only get status Payment from Origin or Voucher
                        if (customerInvoice.Origin.Status != (int)SoeOriginStatus.Origin && customerInvoice.Origin.Status != (int)SoeOriginStatus.Voucher &&
                            customerInvoice.ExportStatus == (int)SoeInvoiceExportStatusType.ExportedAndClosed)
                        {
                            result.Success = false;
                            result.ErrorNumber = (int)ActionResultSave.InvalidStateTransition;
                            return result;
                        }

                        //PayAmount
                        if (item.TotalAmount >= 0)
                        {
                            decimal remainingAmount = customerInvoice.TotalAmount - customerInvoice.PaidAmount;
                            if (item.PayAmount == 0)
                                item.PayAmount = remainingAmount;
                            else if (item.PayAmount > remainingAmount)
                                item.PaymentAmountDiff = item.PayAmount - remainingAmount;
                        }
                        else
                        {
                            decimal remainingAmount = customerInvoice.TotalAmount - customerInvoice.PaidAmount;
                            if (item.PayAmount < remainingAmount)
                                item.PayAmount = remainingAmount;
                        }

                        //Check if account for overpayment is specified                        
                        result = ValidateOverPaymentAccount(entities, item.PaymentAmountDiff, actorCompanyId);
                        if (!result.Success)
                            return result;

                        #region Origin payment

                        //Create payment Origin for the SupplierInvoice
                        var paymentOrigin = new Origin
                        {
                            Type = (int)SoeOriginType.CustomerPayment,
                            Status = (int)SoeOriginStatus.Payment,

                            //Set references
                            ActorCompanyId = actorCompanyId,
                            VoucherSeries = voucherSerie,
                            VoucherSeriesTypeId = customerInvoicePaymentSeriesTypeId,
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
                            PaymentExport = null, //Set when export file is physical created
                        };

                        #endregion

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

                        //Create PaymentRow for SupplierInvoice
                        var paymentRow = new PaymentRow
                        {
                            Status = (int)SoePaymentStatus.ManualPayment,
                            SysPaymentTypeId = 0,
                            PaymentNr = customerInvoice.PaymentNr != null ? customerInvoice.PaymentNr : string.Empty,
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
                            //Validate pay amount. Same check in GUI - Bypass if both invoice and payment is zero
                            if (item.PayAmountCurrency != 0 || item.TotalAmountCurrency != 0)
                            {
                                result = ValidatePaymentAmount(item.PayAmountCurrency, item.TotalAmountCurrency, item.PaymentSeqNr);
                                if (!result.Success)
                                    return result;
                            }

                            //Set currency amounts
                            paymentRow.AmountCurrency = item.PayAmountCurrency;
                            paymentRow.BankFeeCurrency = 0;
                            paymentRow.AmountDiffCurrency = item.PaymentAmountDiff;
                            CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, paymentRow, true);
                        }
                        else
                        {

                            //Validate pay amount. Same check in GUI - Bypass if both invoice and payment is zero
                            if (item.PayAmount != 0 || item.TotalAmount != 0)
                            {
                                result = ValidatePaymentAmount(item.PayAmount, item.TotalAmount, item.PaymentSeqNr);
                                if (!result.Success)
                                    return result;
                            }

                            //Set currency amounts
                            paymentRow.Amount = item.PayAmount;
                            paymentRow.BankFee = 0;
                            paymentRow.AmountDiff = item.PaymentAmountDiff;
                            CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, paymentRow);
                        }

                        //PaidAmount
                        customerInvoice.PaidAmount += paymentRow.Amount;
                        customerInvoice.PaidAmountCurrency += paymentRow.AmountCurrency;

                        //FullyPayed
                        InvoiceManager.SetInvoiceFullyPayed(entities, customerInvoice, (item.PayAmountCurrency == 0 && item.TotalAmountCurrency == 0 && item.PayAmount == 0 && item.TotalAmount == 0) ? true : customerInvoice.IsTotalAmountPayed);

                        //Accounting rows - Bypass on zero payment
                        if (item.PayAmountCurrency != 0 || item.TotalAmountCurrency != 0 || item.PayAmount != 0 || item.TotalAmount != 0)
                        {
                            result = AddPaymentAccountRowsFromCustomerInvoice(entities, paymentRow, paymentMethod, customerInvoice, actorCompanyId);
                            if (!result.Success)
                                return result;
                        }

                        paymentRows.Add(paymentRow);

                        #endregion
                    }

                    if (paymentRows.Any())
                    {
                        paymentRowsPerAccountYear.Add(accountYear.AccountYearId, paymentRows);
                    }

                    #region Voucher

                    // Check if Voucher should also should be saved at once
                    if (result.Success)
                    {
                        foreach (var paymentRowsForYear in paymentRowsPerAccountYear)
                        {
                            result = TryTransferPaymentRowsToVoucher(entities, ConfigSettings.TRANSACTIONSCOPEOPTION_ATTACH, paymentRowsForYear.Value, SoeOriginType.CustomerPayment, foreign, paymentRowsForYear.Key, actorCompanyId);
                            if (result.Success)
                                voucherHeadsDict = NumberUtility.MergeDictictionary(voucherHeadsDict, result.IdDict);
                        }
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
                    }
                }
                else
                {
                    //Set error message
                    if (result.ErrorNumber > 0)
                        result.ErrorMessage = GetErrorMessage(result.ErrorNumber, result.StringValue, result.ErrorMessage);

                    base.LogTransactionFailed(this.ToString(), this.log);
                }

                if (transactionScopeOption != ConfigSettings.TRANSACTIONSCOPEOPTION_ATTACH)
                    entities.Connection.Close();
            }

            return result;
        }

        public ActionResult SaveCashPaymentsForCustomerInvoice(List<CashPaymentDTO> payments, int invoiceId, int? matchCodeId, decimal remainingAmount, bool sendEmail, string email, int actorCompanyId, bool useRounding)
        {
            ActionResult result = new ActionResult(true);

            Dictionary<int, int> voucherHeadsDict = null;

            try
            {
                using (CompEntities entities = new CompEntities())
                {
                    if (entities.Connection.State != ConnectionState.Open)
                        entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Settings

                        //VoucherSeries
                        int customerInvoicePaymentSeriesTypeId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.CustomerPaymentVoucherSeriesType, 0, actorCompanyId, 0);

                        // Base currency
                        int baseSysCurrencyId = CountryCurrencyManager.GetCompanyBaseSysCurrencyId(entities, actorCompanyId);

                        #endregion

                        //Get Company
                        Company company = CompanyManager.GetCompany(entities, actorCompanyId);
                        if (company == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                        //Get AccountYear
                        AccountYear accountYear = AccountManager.GetAccountYear(entities, DateTime.Today, actorCompanyId);

                        //Validate AccountYear
                        result = AccountManager.ValidateAccountYear(DateTime.Today, accountYear.From, accountYear.To);
                        if (!result.Success)
                            return result;

                        //Validate AccountPeriod
                        AccountPeriod accountPeriod = AccountManager.GetAccountPeriod(entities, DateTime.Today, actorCompanyId);
                        result = AccountManager.ValidateAccountPeriod(accountPeriod, DateTime.Today);
                        if (!result.Success)
                            return result;

                        //Get default VoucherSerie for Payment for current AccountYear
                        VoucherSeries voucherSerie = VoucherManager.GetVoucherSerieByType(entities, customerInvoicePaymentSeriesTypeId, accountYear.AccountYearId);
                        if (voucherSerie == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8403, "Verifikatserie saknas"));

                        //Get original and loaded CustomerInvoice
                        CustomerInvoice customerInvoice = InvoiceManager.GetCustomerInvoice(entities, invoiceId, true, true, true, false, true, false, false, true, true, false, false, false);
                        if (customerInvoice == null || !customerInvoice.ActorId.HasValue)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "CustomerInvoice");

                        //Can only get status Payment from Origin or Voucher
                        if (customerInvoice.Origin.Status != (int)SoeOriginStatus.Origin && customerInvoice.Origin.Status != (int)SoeOriginStatus.Voucher &&
                            customerInvoice.ExportStatus == (int)SoeInvoiceExportStatusType.ExportedAndClosed)
                        {
                            result.Success = false;
                            result.ErrorNumber = (int)ActionResultSave.InvalidStateTransition;
                            return result;
                        }

                        if (customerInvoice.InsecureDebt)
                            return new ActionResult((int)ActionResultSave.NothingSaved, GetText(7521, "Fakturan är markerad som osäker och kan ej betalas"));

                        // Get currency
                        Currency currency = CountryCurrencyManager.GetCurrency(entities, customerInvoice.CurrencyId);
                        if (currency == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "Currency");

                        // Get match code
                        MatchCode matchCode = matchCodeId.HasValue && remainingAmount != 0 ? InvoiceManager.GetMatchCode(entities, matchCodeId.Value) : null;

                        // Flag for updating invoice on rounding
                        var updateInvoice = (useRounding && (customerInvoice.Origin.Status == (int)SoeOriginStatus.Draft || customerInvoice.Origin.Status == (int)SoeOriginStatus.Origin));

                        int handled = 1;
                        int total = payments.Count;
                        PaymentMethod roundedPaymentMethod = null;
                        foreach (var paymentPart in payments)
                        {
                            //Get PaymentMethod
                            PaymentMethod paymentMethod = PaymentManager.GetPaymentMethod(entities, paymentPart.PaymentMethodId, actorCompanyId, true);
                            if (paymentMethod == null)
                                return new ActionResult((int)ActionResultSave.EntityNotFound, "PaymentMethod");
                            if (paymentMethod.AccountStd == null)
                                return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountStd");

                            #region Origin payment

                            //Create payment Origin for the SupplierInvoice
                            var paymentOrigin = new Origin()
                            {
                                Type = (int)SoeOriginType.CustomerPayment,
                                Status = (int)SoeOriginStatus.Payment,

                                //Set references
                                Company = company,
                                VoucherSeries = voucherSerie,
                                VoucherSeriesTypeId = customerInvoicePaymentSeriesTypeId,
                            };
                            SetCreatedProperties(paymentOrigin);

                            #endregion

                            #region Payment

                            //Create Payment
                            var payment = new Payment()
                            {
                                //Set references
                                Origin = paymentOrigin,
                                PaymentMethod = paymentMethod,
                                PaymentExport = null, //Set when export file is physical created
                            };

                            #endregion

                            #region CustomerInvoice

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

                            //Create PaymentRow for SupplierInvoice
                            PaymentRow paymentRow = new PaymentRow()
                            {
                                Status = (int)SoePaymentStatus.ManualPayment,
                                SysPaymentTypeId = 0,
                                PaymentNr = customerInvoice.PaymentNr != null ? customerInvoice.PaymentNr : string.Empty,
                                PayDate = DateTime.Today,
                                CurrencyRate = customerInvoice.CurrencyRate,
                                CurrencyDate = customerInvoice.CurrencyDate,

                                //Set references
                                Invoice = customerInvoice as Invoice,
                                Payment = payment,
                            };
                            SetCreatedProperties(paymentRow);

                            //SeqNr
                            paymentRow.SeqNr = SequenceNumberManager.GetNextSequenceNumber(entities, actorCompanyId, Enum.GetName(typeof(SoeOriginType), paymentOrigin.Type), 1, true);

                            bool foreign = currency.SysCurrencyId != baseSysCurrencyId;

                            //Set amount on payment depending on foreign or not
                            if (foreign)
                            {
                                // Keep payment method for rest payment
                                if (useRounding && paymentMethod.UseRoundingInCashSales && !updateInvoice && roundedPaymentMethod == null)
                                    roundedPaymentMethod = paymentMethod;

                                //Set currency amounts
                                paymentRow.AmountCurrency = handled == total ? paymentPart.AmountCurrency + remainingAmount : paymentPart.AmountCurrency;

                                paymentRow.BankFeeCurrency = 0;
                                paymentRow.AmountDiffCurrency = handled == total ? remainingAmount : 0;
                                CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, paymentRow, true);
                            }
                            else
                            {
                                // Keep payment method for rest payment
                                if (useRounding && paymentMethod.UseRoundingInCashSales && !updateInvoice && roundedPaymentMethod == null)
                                    roundedPaymentMethod = paymentMethod;

                                //Set currency amounts
                                paymentRow.Amount = handled == total ? paymentPart.AmountCurrency + remainingAmount : paymentPart.AmountCurrency;
                                paymentRow.BankFee = 0;
                                paymentRow.AmountDiff = handled == total ? remainingAmount : 0;
                                CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, paymentRow);
                            }

                            //PaidAmount
                            customerInvoice.PaidAmount += paymentRow.Amount;
                            customerInvoice.PaidAmountCurrency += paymentRow.AmountCurrency;

                            //Accounting rows - Bypass on zero payment
                            if (paymentPart.AmountCurrency != 0 || customerInvoice.TotalAmountCurrency != 0)
                            {
                                if (handled == total && matchCode != null)
                                    result = AddCashPaymentAccountRowsFromCustomerInvoice(entities, paymentRow, paymentMethod, customerInvoice, actorCompanyId, matchCode);
                                else
                                    result = AddPaymentAccountRowsFromCustomerInvoice(entities, paymentRow, paymentMethod, customerInvoice, actorCompanyId, handled == total ? matchCode : null);
                                if (!result.Success)
                                    return result;
                            }

                            #endregion

                            #region Voucher

                            // Check if Voucher should also should be saved at once
                            if (result.Success)
                            {
                                result = TryTransferPaymentRowToVoucher(entities, ConfigSettings.TRANSACTIONSCOPEOPTION_ATTACH, paymentRow, SoeOriginType.CustomerPayment, foreign, accountYear.AccountYearId, actorCompanyId);
                                if (result.Success)
                                    voucherHeadsDict = NumberUtility.MergeDictictionary(voucherHeadsDict, result.IdDict);
                            }

                            #endregion

                            handled++;
                        }

                        if (!result.Success)
                            return result;

                        // Cash sales
                        if (!customerInvoice.CashSale)
                            customerInvoice.CashSale = true;

                        //FullyPayed
                        InvoiceManager.SetInvoiceFullyPayed(entities, customerInvoice, true);

                        // Check status
                        if (customerInvoice.Origin.Status == (int)SoeOriginStatus.Draft)
                            result = OriginManager.UpdateOriginStatus(entities, transaction, customerInvoice, SoeOriginStatus.Origin, actorCompanyId, cashSale: true);

                        if (result.Success && useRounding)
                        {
                            if (updateInvoice)
                            {
                                #region Update invoice amount and add cent rounding

                                // Set pricelist
                                InvoiceManager.SetPriceListTypeInclusiveVat(entities, customerInvoice, actorCompanyId);

                                // Set cent rounding
                                customerInvoice.CentRounding = (customerInvoice.TotalAmountCurrency - payments.Sum(p => p.AmountCurrency));

                                // Update invoice
                                result = InvoiceManager.UpdateInvoiceAfterRowModification(entities, customerInvoice, customerInvoice.ActiveCustomerInvoiceRows.Count + 1, actorCompanyId, false, currency.SysCurrencyId != baseSysCurrencyId, true);

                                #endregion
                            }
                            else if (roundedPaymentMethod != null)
                            {
                                #region  Add second payment for cent amount

                                var paymentOrigin = new Origin()
                                {
                                    Type = (int)SoeOriginType.CustomerPayment,
                                    Status = (int)SoeOriginStatus.Payment,

                                    //Set references
                                    Company = company,
                                    VoucherSeries = voucherSerie,
                                };
                                SetCreatedProperties(paymentOrigin);

                                var payment = new Payment()
                                {
                                    //Set references
                                    Origin = paymentOrigin,
                                    PaymentMethod = roundedPaymentMethod,
                                    PaymentExport = null,
                                };

                                OriginInvoiceMapping oimap = new OriginInvoiceMapping()
                                {
                                    Type = (int)SoeOriginInvoiceMappingType.CustomerPayment,

                                    //Set references
                                    Origin = paymentOrigin,
                                };
                                customerInvoice.OriginInvoiceMapping.Add(oimap);

                                PaymentRow paymentRow = new PaymentRow()
                                {
                                    Status = (int)SoePaymentStatus.ManualPayment,
                                    SysPaymentTypeId = 0,
                                    PaymentNr = customerInvoice.PaymentNr != null ? customerInvoice.PaymentNr : string.Empty,
                                    PayDate = DateTime.Today,
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
                                if (currency.SysCurrencyId != baseSysCurrencyId)
                                {
                                    //Set currency amounts
                                    paymentRow.AmountCurrency = (customerInvoice.TotalAmountCurrency - customerInvoice.PaidAmountCurrency);

                                    paymentRow.BankFeeCurrency = 0;
                                    paymentRow.AmountDiffCurrency = 0;
                                    CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, paymentRow, true);
                                }
                                else
                                {
                                    //Set currency amounts
                                    paymentRow.Amount = (customerInvoice.TotalAmountCurrency - customerInvoice.PaidAmountCurrency);
                                    paymentRow.BankFee = 0;
                                    paymentRow.AmountDiff = 0;
                                    CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, paymentRow);
                                }

                                result = AddPaymentAccountRowsFromCustomerInvoice(entities, paymentRow, roundedPaymentMethod, customerInvoice, actorCompanyId, handled == total ? matchCode : null);

                                if (!result.Success)
                                    return result;


                                #region Voucher

                                // Check if Voucher should also should be saved at once
                                if (result.Success)
                                {
                                    result = TryTransferPaymentRowToVoucher(entities, ConfigSettings.TRANSACTIONSCOPEOPTION_ATTACH, paymentRow, SoeOriginType.CustomerPayment, currency.SysCurrencyId != baseSysCurrencyId, accountYear.AccountYearId, actorCompanyId);
                                    if (result.Success)
                                        voucherHeadsDict = NumberUtility.MergeDictictionary(voucherHeadsDict, result.IdDict);
                                }

                                #endregion

                                #endregion
                            }
                        }

                        if (result.Success)
                            result = SaveChanges(entities, transaction);

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
                    if (voucherHeadsDict != null)
                    {
                        result.IdDict = voucherHeadsDict;
                    }
                }
                else
                {
                    //Set error message
                    if (result.ErrorNumber > 0)
                        result.ErrorMessage = GetErrorMessage(result.ErrorNumber, result.StringValue, result.ErrorMessage);

                    base.LogTransactionFailed(this.ToString(), this.log);
                }
            }

            return result;
        }

        public ActionResult SaveImportPaymentFromSupplierInvoice(List<PaymentImportIODTO> items, DateTime? bulkPayDate, SoeOriginType originType, bool foreign, int accountYearId, int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                return SaveImportPaymentFromSupplierInvoice(entities, items, bulkPayDate, originType, foreign, accountYearId, actorCompanyId);
            }
        }

        public ActionResult SaveImportPaymentFromSupplierInvoice(CompEntities entities, List<PaymentImportIODTO> items, DateTime? bulkPayDate, SoeOriginType originType, bool foreign, int accountYearId, int actorCompanyId)
        {
            InvoiceManager im = this.InvoiceManager;

            if (items == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "PaymentImportIODTO");

            var result = new ActionResult(true);

            try
            {
                var paymentRows = new List<PaymentRow>();

                //We get account year based on date instead of using the accountyear which is provided from the client
                //as that has proven to be unreliable.
                var accountYear = AccountManager.GetAccountYear(entities, bulkPayDate ?? DateTime.Today, actorCompanyId);

                foreach (var item in items)
                {
                    #region Req
                    item.ActorCompanyId = actorCompanyId;
                    bool isPartPayment = item.Status == ImportPaymentIOStatus.PartlyPaid;
                    bool isFullyPaid = isPartPayment && item.RestAmount != 0 ? false : true;

                    SupplierInvoice supplierInvoice = SupplierInvoiceManager.GetSupplierInvoice(entities, item.InvoiceId.Value, true, true, true, true, true, true, true, false);
                    if (supplierInvoice == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8371, "Faktura saknas") + " :" + item.InvoiceNr);



                    var itemDate = bulkPayDate.HasValue ? bulkPayDate.ToValueOrToday() : item.PaidDate.ToValueOrToday();
                    if (accountYear == null || accountYear.From >= itemDate || accountYear.To <= itemDate)
                        accountYear = AccountManager.GetAccountYear(entities, itemDate, actorCompanyId);
                    if (accountYear == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountYear");
                    accountYearId = accountYear.AccountYearId;

                    #endregion

                    #region Get PaymentRow                    

                    //Get Paymentrow from database
                    IQueryable<PaymentRow> query = (from pr in entities.PaymentRow
                                                 .Include("Invoice.Actor.Supplier")
                                                 .Include("Payment.Origin")
                                                 .Include("PaymentAccountRow.AccountStd.Account")
                                                 .Include("PaymentAccountRow.AccountInternal.Account")
                                                    where
                                                    pr.InvoiceId.HasValue &&
                                                    pr.InvoiceId.Value == item.InvoiceId &&
                                                    pr.VoucherHead == null &&
                                                    (pr.Status == (int)SoePaymentStatus.Verified || pr.Status == (int)SoePaymentStatus.Pending || pr.Status == (int)SoePaymentStatus.ManualPayment) &&
                                                    !pr.IsSuggestion &&
                                                    pr.State == (int)SoeEntityState.Active
                                                    select pr);

                    var paymentRow = query.Where(pr => pr.Amount == (item.PaidAmount + item.RestAmount)).FirstOrDefault();
                    if (paymentRow == null && item.PaidAmountCurrency != 0 && item.PaidAmountCurrency != item.PaidAmount)
                    {
                        paymentRow = query.Where(pr => pr.AmountCurrency == item.PaidAmountCurrency).FirstOrDefault();
                    }

                    if (paymentRow == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(7076, "Kunde inte hitta betalning för fakturanr") + $": {item.InvoiceNr} ({item.PaidAmount}/{item.RestAmount})");
                    if (paymentRow.Payment == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "Payment");
                    if (paymentRow.Payment.Origin == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "Origin");
                    if (paymentRow.Invoice == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "Invoice");

                    PaymentMethod paymentMethod = GetPaymentMethod(entities, (int)paymentRow.Payment.PaymentMethodId, actorCompanyId, true);
                    DateTime originalPaymentDate = paymentRow.PayDate;

                    #endregion

                    #region PartPayment

                    if (isPartPayment)
                    {
                        //update existing paymentrow
                        paymentRow.Amount = (decimal)item.InvoiceAmount - (decimal)item.RestAmount;
                        paymentRow.AmountCurrency = paymentRow.Amount;
                        paymentRow.AmountEntCurrency = paymentRow.Amount;
                        paymentRow.AmountLedgerCurrency = paymentRow.Amount;
                        paymentRow.AmountDiff = (decimal)item.InvoiceAmount - paymentRow.Amount;
                        paymentRow.AmountDiffCurrency = paymentRow.AmountDiff;
                        paymentRow.AmountDiffEntCurrency = paymentRow.AmountDiff;
                        paymentRow.AmountDiffLedgerCurrency = paymentRow.AmountDiff;

                        //update accounting rows                        
                        foreach (var row in paymentRow.PaymentAccountRow)
                        {
                            row.State = (int)SoeEntityState.Deleted;
                        }

                        PaymentManager.AddPaymentAccountRowsFromSupplierInvoice(entities, paymentRow, supplierInvoice, paymentMethod, actorCompanyId);

                        //create new paymentrow for rest amount
                        PaymentRowSaveDTO newPaymentRowDTO = new PaymentRowSaveDTO()
                        {
                            OriginType = SoeOriginType.SupplierPayment,
                            OriginStatus = SoeOriginStatus.Payment,
                            PaymentDate = originalPaymentDate,
                            Amount = (decimal)item.RestAmount,
                            AmountCurrency = (decimal)item.RestAmount,
                            InvoiceId = supplierInvoice.InvoiceId,
                            FullyPayed = true,
                            PaymentNr = paymentRow.PaymentNr,
                            SysPaymentTypeId = paymentRow.SysPaymentTypeId,
                            PaymentMethodId = paymentMethod.PaymentMethodId,
                            CurrencyId = supplierInvoice.CurrencyId,
                            CurrencyDate = originalPaymentDate,
                            CurrencyRate = paymentRow.CurrencyRate,
                            State = (int)SoeEntityState.Active,
                            PaymentStatus = SoePaymentStatus.Pending,
                            VoucherSeriesId = paymentRow.Payment.Origin.VoucherSeriesId
                        };

                        using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                        {
                            result = PaymentManager.SavePaymentRow(entities, transaction, newPaymentRowDTO, null, actorCompanyId, false, false, false, true);

                            if (!result.Success)
                                return result;

                            result = SaveChanges(entities, transaction);

                            if (!result.Success)
                                return result;

                            transaction.Complete();
                        }
                    }

                    #endregion

                    #region BankFee and DiffAmount

                    decimal bankFee = 0;
                    var paymentBankFee = supplierInvoice.PaymentRow.FirstOrDefault(s => s.InvoiceId == item.InvoiceId);
                    if (paymentBankFee != null)
                    {
                        bankFee = paymentBankFee.BankFee;
                    }

                    decimal remainingAmount = !isPartPayment ? paymentRow.Amount : (decimal)item.RestAmount;
                    decimal bankFeeAmount = originType == SoeOriginType.SupplierPayment ? bankFee : 0;
                    decimal paidAmount = item.PaidAmount.Value - bankFeeAmount;

                    //Set BankFee
                    if (bankFeeAmount > 0)
                    {
                        paymentRow.BankFee = bankFeeAmount;
                        paymentRow.HasPendingBankFee = true;
                    }

                    //Set AmountDiff (not handled manually, generate default PaymentAccountRow's)
                    decimal amountDiff = paidAmount - remainingAmount;
                    if (amountDiff != 0 && isFullyPaid)
                    {
                        paymentRow.AmountDiff = Decimal.Add(paymentRow.AmountDiff, amountDiff);
                        paymentRow.HasPendingAmountDiff = true;
                    }

                    // Set paydate                   
                    paymentRow.PayDate = item.PaidDate.HasValue ? item.PaidDate.Value : bulkPayDate.Value;

                    result = PaymentManager.HandlePendingDiffAndBankFee(entities, paymentRow, originType, item.PaidAmount.Value, foreign, actorCompanyId);
                    if (!result.Success)
                        return result;

                    #endregion

                    #region Set Item Status

                    if (item.Status == ImportPaymentIOStatus.Match)
                    {
                        item.Status = ImportPaymentIOStatus.Paid;
                    }

                    item.State = ImportPaymentIOState.Closed;

                    #endregion

                    paymentRows.Add(paymentRow);

                }

                // Set to paid
                result = VoucherManager.SaveVoucherFromPayment(entities, ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, paymentRows, originType, foreign, accountYearId, actorCompanyId);

                if (result.Success)
                {
                    foreach (PaymentImportIODTO pI in items)
                    {
                        im.UpdatePaymentImportIO(entities, pI);
                    }
                }
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result.Exception = ex;
            }

            return result;
        }

        public ActionResult SavePaymentFromPaymentSuggestion(List<SupplierPaymentGridDTO> items, DateTime? bulkPayDate, int paymentMethodId, int accountYearId, int actorCompanyId, bool foreign, bool sendPaymentFile)
        {
            if (items == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "SupplierPaymentGridDTO");

            // Default result is successful
            ActionResult result = new ActionResult(true);

            if (sendPaymentFile && SignatoryContractManager.UsesSignatoryContractForPermission(TermGroup_SignatoryContractPermissionType.AccountsPayable_SendPaymentToBank))
            {
                AuthorizeRequestDTO authorizeRequest = new AuthorizeRequestDTO()
                {
                    PermissionType = TermGroup_SignatoryContractPermissionType.AccountsPayable_SendPaymentToBank,
                };
                var authResult = SignatoryContractManager.Authorize(authorizeRequest);

                if (!authResult.IsAuthorized)
                    return new ActionResult("Not authorized");
            }


            #region Prereq

            if (bulkPayDate.HasValue)
            {
                foreach (var item in items)
                {
                    item.PayDate = bulkPayDate.Value;
                }
            }

            #endregion

            List<SysCountry> sysCountries = SysDbCache.Instance.SysCountrys;
            List<SysCurrency> sysCurrencies = SysDbCache.Instance.SysCurrencies;

            using (CompEntities entities = new CompEntities())
            {
                Dictionary<int, int> voucherHeadsDict = null;

                PaymentExport paymentExport = null;

                string paymentExportGuid = "";
                int paymentIOType = 0;
                string companyGuid = CompanyManager.GetCompanyGuid(actorCompanyId);

                try
                {
                    entities.Connection.Open();

                    #region Prereq

                    //Get PaymentMethod
                    PaymentMethod paymentMethod = PaymentManager.GetPaymentMethod(entities, paymentMethodId, actorCompanyId, true);
                    if (paymentMethod == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "PaymentMethod");
                    if (paymentMethod.AccountStd == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(1481, "Konto hittades inte"));

                    #endregion

                    using (TransactionScope transaction = CreateTransactionScope(new TimeSpan(0, 30, 0), System.Transactions.IsolationLevel.ReadCommitted))
                    {
                        var paymentRows = new List<PaymentRow>();
                        var payments = new List<Payment>();

                        #region Load Payments and PaymentRows

                        var handledPaymentRowIds = new List<int>();

                        foreach (var item in items)
                        {
                            if (handledPaymentRowIds.Contains(item.PaymentRowId))
                                continue;

                            PaymentRow paymentRow = GetPaymentRow(entities, item.PaymentRowId, loadAccountRows: true, loadAccounts: true, loadOrigin: true);
                            if (paymentRow != null)
                            {
                                #region Validate

                                if (item.PayDate.HasValue && paymentRow.PayDate != item.PayDate.Value)
                                {
                                    result = ValidateNewPaymentRowDate(entities, paymentRow, item, actorCompanyId);
                                    if (!result.Success)
                                        return result;
                                    paymentRow.PayDate = item.PayDate.Value;
                                }

                                if (String.IsNullOrEmpty(item.PaymentNr))
                                    return new ActionResult((int)ActionResultSave.PaymentNrCannotBeEmpty);

                                #endregion

                                #region Add Payment

                                //Make sure Payment is loaded
                                if (!paymentRow.PaymentReference.IsLoaded)
                                    paymentRow.PaymentReference.Load();

                                if (paymentRow.Payment != null)
                                {
                                    //Make user invoice is loaded
                                    if (!paymentRow.InvoiceReference.IsLoaded)
                                        paymentRow.InvoiceReference.Load();

                                    if (paymentRow.Invoice != null)
                                    {
                                        #region PaymentMethod

                                        if (paymentMethod.PaymentMethodId != paymentRow.Payment.PaymentMethodId)
                                        {
                                            //Remove current account rows
                                            foreach (PaymentAccountRow accRow in paymentRow.PaymentAccountRow)
                                            {
                                                accRow.State = (int)SoeEntityState.Deleted;
                                                SetModifiedProperties(accRow);
                                            }

                                            result = SaveChanges(entities);
                                            if (!result.Success)
                                                return result;

                                            //Regenerate account rows
                                            result = AddPaymentAccountRowsFromSupplierInvoice(entities, paymentRow, paymentRow.Invoice as SupplierInvoice, paymentMethod, actorCompanyId);

                                            if (!result.Success)
                                                return result;
                                        }

                                        #endregion

                                        if (!paymentRow.Invoice.CurrencyReference.IsLoaded)
                                            paymentRow.Invoice.CurrencyReference.Load();

                                        paymentRow.Invoice.PaymentNr = item.PaymentNr;
                                        paymentRow.Invoice.SysPaymentTypeId = item.SysPaymentTypeId;
                                        SetModifiedProperties(paymentRow.Invoice);

                                        if (!payments.Any(i => i.PaymentId == paymentRow.Payment.PaymentId))
                                            payments.Add(paymentRow.Payment);
                                    }
                                }

                                #endregion

                                #region Add PaymentRow

                                paymentRows.Add(paymentRow);

                                #endregion

                                #region Update payment row

                                paymentRow.IsSuggestion = false;
                                //Payment nr is mandatory
                                paymentRow.PaymentNr = item.PaymentNr;
                                paymentRow.SysPaymentTypeId = item.SysPaymentTypeId;
                                SetModifiedProperties(paymentRow);

                                #endregion
                            }

                            handledPaymentRowIds.Add(item.PaymentRowId);
                        }

                        List<int> seqRecordIds = items.Select(i => i.SequenceNumberRecordId).Distinct().ToList();
                        foreach (int seqRecordId in seqRecordIds)
                        {
                            SequenceNumberManager.InactivateSequenceNumberRecord(entities, seqRecordId);
                        }

                        #endregion

                        foreach (Payment payment in payments)
                        {
                            #region Payment

                            payment.PaymentMethod = paymentMethod;
                            SetModifiedProperties(payment);

                            // Check if Voucher should also should be saved at once
                            result = TryTransferPaymentRowsToVoucher(entities, ConfigSettings.TRANSACTIONSCOPEOPTION_ATTACH, paymentRows, SoeOriginType.SupplierPayment, foreign, accountYearId, actorCompanyId);
                            if (result.Success)
                                voucherHeadsDict = NumberUtility.MergeDictictionary(voucherHeadsDict, result.IdDict);

                            #endregion

                            #region Paymentfile

                            if (result.Success)
                            {
                                if (payment == null)
                                {
                                    result.Success = false;
                                    result.ErrorNumber = (int)ActionResultSave.PaymentNotFound;
                                }

                                result = PaymentIOManager.Export(entities, sysCountries, sysCurrencies, transaction, payment, paymentMethod.SysPaymentMethodId, actorCompanyId, sendPaymentFile ? TermGroup_PaymentTransferStatus.PendingTransfer : TermGroup_PaymentTransferStatus.None, sendPaymentFile);
                                if (!result.Success)
                                    return result;

                                if (result.Value is PaymentExport)
                                {
                                    paymentExport = (result.Value as PaymentExport);

                                    if (sendPaymentFile)
                                    {
                                        var bankerResult = Banker.BankerConnector.UploadPaymentFile(SettingManager, paymentMethod, actorCompanyId, companyGuid, SysServiceManager.GetSysCompDBId(), CountryCurrencyManager.GetSysBank(paymentMethod.PaymentInformationRow.BIC), paymentExport.MsgId, paymentExport.Data);
                                        if (!bankerResult.Success)
                                        {
                                            result = bankerResult;
                                            return result;
                                        }
                                    }
                                }

                                //Save GUID and PaymentIOType to be able to open the file from disk later
                                paymentExportGuid = result.StringValue;
                                paymentIOType = result.IntegerValue;
                            }

                            #endregion
                        }

                        if (result.Success)
                        {
                            result = SaveChanges(entities, transaction);
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
                        if (voucherHeadsDict != null)
                        {
                            result.IdDict = voucherHeadsDict;
                            //result.IntegerValue = voucherHeadsDict.Count;
                        }
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                //Set GUID and PaymentIOType
                result.StringValue = paymentExportGuid;
                result.IntegerValue = paymentIOType;
                result.IntegerValue2 = paymentExport != null ? paymentExport.PaymentExportId : 0;
                return result;
            }
        }

        public ActionResult AddPaymentAccountRowsFromSupplierInvoice(CompEntities entities, PaymentRow paymentRow, SupplierInvoice supplierInvoice, PaymentMethod paymentMethod, int actorCompanyId, bool overrideValidation = false)
        {
            ActionResult result = new ActionResult(true);

            int actorId = supplierInvoice.ActorId.HasValue ? supplierInvoice.ActorId.Value : 0;

            if (!supplierInvoice.SupplierInvoiceRow.IsLoaded)
                supplierInvoice.SupplierInvoiceRow.Load();

            //Step 1: Locate the debt accounting row.
            SupplierInvoiceAccountRow supplierInvoiceAccountRow = null;

            foreach (var invoiceRow in supplierInvoice.SupplierInvoiceRow)
            {
                if (!invoiceRow.SupplierInvoiceAccountRow.IsLoaded)
                    invoiceRow.SupplierInvoiceAccountRow.Load();

                supplierInvoiceAccountRow = invoiceRow.SupplierInvoiceAccountRow.FirstOrDefault(row =>
                        !row.InterimRow && !row.VatRow && !row.ContractorVatRow &&
                        ((supplierInvoice.BillingType == (int)TermGroup_BillingType.Credit && invoiceRow.Amount >= 0) ||
                            (supplierInvoice.BillingType != (int)TermGroup_BillingType.Credit && invoiceRow.Amount < 0)));

                if (supplierInvoiceAccountRow != null)
                    break;
            }
            ;

            if (supplierInvoiceAccountRow == null)
            {
                //Early no debt row found
                result.Success = false;
                result.ErrorMessage = GetText(92041, @"Betalning kunde inte skapas, ingen konteringsrad för leverantörsskulden kunde hittas. Kontrollera konteringsraderna på faktura " + supplierInvoice.SeqNr) + supplierInvoice.SeqNr;
                return result;
            }

            //Step 2: Ensure the debt accounting row is actually using a debt account.
            int debtAccountId = 0;
            if (SupplierInvoiceManager.IsSupplierInvoiceDebtAccount(entities, supplierInvoiceAccountRow.AccountId) || overrideValidation)
                debtAccountId = supplierInvoiceAccountRow.AccountId;

            if (debtAccountId == 0)
            {
                var supplier = SupplierManager.GetSupplier(entities, supplierInvoice.Actor.Supplier.ActorSupplierId, false, false, false, false)
                    .ToDTO(false);
                supplier.AccountingSettings = SupplierManager.GetSupplierAccountSettings(entities, actorCompanyId, supplier.ActorSupplierId);
                AccountingSettingsRowDTO supplierDebitSettingsRowDTO = supplier.AccountingSettings.FirstOrDefault(i => i.Type == (int)SupplierAccountType.Debit);
                if (supplierDebitSettingsRowDTO != null && SupplierInvoiceManager.IsSupplierInvoiceDebtAccount(entities, supplierDebitSettingsRowDTO.Account1Id))
                {
                    debtAccountId = supplierDebitSettingsRowDTO.Account1Id;
                }
            }

            if (debtAccountId == 0)
                debtAccountId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.AccountSupplierDebt, 0, actorCompanyId, 0);

            AccountStd accountStd = AccountManager.GetAccountStd(entities, debtAccountId, actorCompanyId, true, true);

            if (accountStd == null)
            {
                //Early return no account found
                result.Success = false;
                result.ErrorMessage = GetText(92043, @"Betalning kunde inte skapas, inget standardkonto för leverantörsskuld hittades. Kontrollera konteringsraderna på faktura " + supplierInvoice.SeqNr) + supplierInvoice.SeqNr;
                return result;
            }

            //Step 3: Create the payment accounting rows.
            return CreatePaymentAccountingRowsFromInvoiceRow(entities, actorCompanyId, supplierInvoice, supplierInvoiceAccountRow, paymentMethod, paymentRow, accountStd);
        }

        public ActionResult CreatePaymentAccountingRowsFromInvoiceRow(CompEntities entities, int actorCompanyId, SupplierInvoice supplierInvoice, SupplierInvoiceAccountRow supplierInvoiceAccountRow, PaymentMethod paymentMethod, PaymentRow paymentRow, AccountStd debtAccount)
        {
            int actorId = supplierInvoice.ActorId.GetValueOrDefault();
            int rowNr = 0;

            //Calculate amount for row
            decimal amount = CalculatePayment(supplierInvoice.BillingType, supplierInvoice.TotalAmount, paymentRow.Amount, supplierInvoiceAccountRow.Amount);
            decimal discount = 0;
            if (supplierInvoice.TimeDiscountDate != null && supplierInvoice.TimeDiscountPercent != null && paymentRow.AmountDiff != 0)
            {
                discount = CalculateDiscount(supplierInvoice.BillingType, paymentRow.AmountDiff);
            }

            //Create debt row
            rowNr++;
            var debtAccountRow = new PaymentAccountRow
            {
                RowNr = rowNr,
                Amount = amount + discount,
                Quantity = null,
                Text = "",
                InterimRow = supplierInvoiceAccountRow.InterimRow,
                CreditRow = supplierInvoiceAccountRow.CreditRow ? false : true, //turn
                DebitRow = supplierInvoiceAccountRow.DebitRow ? false : true, //turn

                //Set references
                PaymentRow = paymentRow,
                AccountStd = debtAccount,
            };

            //Set currency amounts
            CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, debtAccountRow, paymentRow.CurrencyRate, actorId);

            //AccountInternal
            foreach (AccountInternal accountInternal in supplierInvoiceAccountRow.AccountInternal)
            {
                debtAccountRow.AccountInternal.Add(accountInternal);
            }

            paymentRow.PaymentAccountRow.Add(debtAccountRow);

            //Create bank row (with values from supplierInvoiceAccountRow) 
            rowNr++;
            var costAccountRow = new PaymentAccountRow
            {
                RowNr = rowNr,
                Amount = Decimal.Negate(amount),
                Quantity = null,
                Text = "",
                InterimRow = false,
                CreditRow = supplierInvoiceAccountRow.CreditRow,
                DebitRow = supplierInvoiceAccountRow.DebitRow,

                //Set references
                PaymentRow = paymentRow,
                AccountStd = paymentMethod.AccountStd,
            };

            //Set currency amounts
            CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, costAccountRow, paymentRow.CurrencyRate, actorId);

            //AccountInternal
            foreach (AccountInternal accountInternal in supplierInvoiceAccountRow.AccountInternal)
            {
                costAccountRow.AccountInternal.Add(accountInternal);
            }

            paymentRow.PaymentAccountRow.Add(costAccountRow);

            //Create discount row and vat row if needed
            if (discount != 0)
            {
                decimal vatAmount = 0;

                //calculate vat amount for discount if invoice includes vat
                if (supplierInvoice.VatType == (int)TermGroup_InvoiceVatType.Merchandise && supplierInvoice.VatCodeId.HasValue)
                {
                    VatCode vatCode = AccountManager.GetVatCode(entities, (int)supplierInvoice.VatCodeId);

                    vatAmount = discount * vatCode.Percent / (100 + vatCode.Percent);
                    vatAmount = Decimal.Round(vatAmount, 2);

                    //Supplier Accounts
                    Dictionary<string, int> supplierAccountsDict = new Dictionary<string, int>();
                    if (supplierInvoice.ActorId.HasValue)
                        supplierAccountsDict = SupplierManager.GetSupplierAccountsDict(entities, supplierInvoice.ActorId.Value);
                    else if (supplierInvoice.Actor != null)
                        supplierAccountsDict = SupplierManager.GetSupplierAccountsDict(entities, supplierInvoice.Actor.ActorId);

                    //Vat
                    int vatAccountId = 0;
                    if (supplierAccountsDict.ContainsKey("vat"))
                        vatAccountId = supplierAccountsDict["vat"];
                    else
                        vatAccountId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.AccountCommonVatReceivable, 0, actorCompanyId, 0);

                    //add accountrow for vat
                    rowNr++;
                    PaymentAccountRow diffVatAccountRow = new PaymentAccountRow()
                    {
                        RowNr = rowNr,
                        Amount = Decimal.Negate(vatAmount),
                        Quantity = null,
                        Text = "",
                        InterimRow = false,
                        CreditRow = supplierInvoiceAccountRow.CreditRow,
                        DebitRow = supplierInvoiceAccountRow.DebitRow,

                        //Set references
                        PaymentRow = paymentRow,
                        AccountStd = AccountManager.GetAccountStd(entities, vatAccountId, actorCompanyId, true, true),
                    };

                    //Set currency amounts
                    CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, diffVatAccountRow, paymentRow.CurrencyRate, actorId);

                    //AccountInternal
                    foreach (AccountInternal accountInternal in supplierInvoiceAccountRow.AccountInternal)
                    {
                        diffVatAccountRow.AccountInternal.Add(accountInternal);
                    }

                    paymentRow.PaymentAccountRow.Add(diffVatAccountRow);
                }

                rowNr++;
                PaymentAccountRow diffAccountRow = new PaymentAccountRow()
                {
                    RowNr = rowNr,
                    Amount = Decimal.Negate(discount - vatAmount),
                    Quantity = null,
                    Text = "",
                    InterimRow = false,
                    CreditRow = supplierInvoiceAccountRow.CreditRow,
                    DebitRow = supplierInvoiceAccountRow.DebitRow,

                    //Set references
                    PaymentRow = paymentRow,
                    AccountStd = AccountManager.GetAccountStdFromCompanySetting(entities, CompanySettingType.AccountSupplierUnderpay, actorCompanyId, loadAccountDim: true),
                };

                //Set currency amounts
                CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, diffAccountRow, paymentRow.CurrencyRate, actorId);

                //AccountInternal
                foreach (AccountInternal accountInternal in supplierInvoiceAccountRow.AccountInternal)
                {
                    diffAccountRow.AccountInternal.Add(accountInternal);
                }

                paymentRow.PaymentAccountRow.Add(diffAccountRow);
            }
            return new ActionResult();
        }

        public ActionResult AddPaymentAccountRowsFromCustomerInvoice(CompEntities entities, PaymentRow paymentRow, PaymentMethod paymentMethod, CustomerInvoice customerInvoice, int actorCompanyId, MatchCode matchCode = null, bool addDiffToTotal = false)
        {
            ActionResult result = new ActionResult(true);

            int actorId = customerInvoice.ActorId.HasValue ? customerInvoice.ActorId.Value : 0;

            #region CustomerInvoiceAccountRow

            // Get asset account row
            CustomerInvoiceAccountRow customerInvoiceAccountRow = InvoiceManager.GetCustomerInvoiceAssetAccountRow(entities, customerInvoice);
            if (customerInvoiceAccountRow == null)
                return new ActionResult((int)ActionResultSave.PaymentInvoiceAssetRowNotFound, GetText(434, (int)TermGroup.ChangeStatusGrid, "Kontrollera konteringsraderna på fakturan"));

            if (!customerInvoiceAccountRow.AccountStdReference.IsLoaded)
                customerInvoiceAccountRow.AccountStdReference.Load();

            #endregion

            #region PaymentAccountRow

            return AddPaymentAccountRowsFromCustomerInvoice(entities, paymentRow, paymentMethod, customerInvoice, actorCompanyId, matchCode, addDiffToTotal, actorId, customerInvoiceAccountRow);

            #endregion
        }

        public ActionResult AddPaymentAccountRowsFromCustomerInvoice(CompEntities entities, PaymentRow paymentRow, PaymentMethod paymentMethod, CustomerInvoice customerInvoice, List<CustomerInvoiceRow> customerInvoiceAccountTypeRows, int actorCompanyId, MatchCode matchCode = null, bool addDiffToTotal = false)
        {
            ActionResult result = new ActionResult(true);

            int actorId = customerInvoice.ActorId.HasValue ? customerInvoice.ActorId.Value : 0;

            #region CustomerInvoiceAccountRow

            // Get asset account row
            CustomerInvoiceAccountRow customerInvoiceAccountRow = InvoiceManager.GetCustomerInvoiceAssetAccountRow(entities, customerInvoice, customerInvoiceAccountTypeRows);
            if (customerInvoiceAccountRow == null)
                return new ActionResult((int)ActionResultSave.PaymentInvoiceAssetRowNotFound, GetText(434, (int)TermGroup.ChangeStatusGrid, "Kontrollera konteringsraderna på fakturan"));

            if (!customerInvoiceAccountRow.AccountStdReference.IsLoaded)
                customerInvoiceAccountRow.AccountStdReference.Load();

            #endregion

            #region PaymentAccountRow

            return AddPaymentAccountRowsFromCustomerInvoice(entities, paymentRow, paymentMethod, customerInvoice, actorCompanyId, matchCode, addDiffToTotal, actorId, customerInvoiceAccountRow);
            #endregion
        }

        private ActionResult AddPaymentAccountRowsFromCustomerInvoice(CompEntities entities, PaymentRow paymentRow, PaymentMethod paymentMethod, CustomerInvoice customerInvoice, int actorCompanyId, MatchCode matchCode, bool addDiffToTotal, int actorId, CustomerInvoiceAccountRow customerInvoiceAccountRow)
        {
            var result = new ActionResult();
            #region debt row

            int rowNr = 0;

            // Calculate amount for row
            decimal amount = CalculatePayment(customerInvoice.BillingType, customerInvoice.TotalAmount, paymentRow.Amount, customerInvoiceAccountRow.Amount);
            decimal diffAmount = 0;
            if (paymentRow.AmountDiff != 0 && addDiffToTotal)
            {
                diffAmount = Decimal.Negate(paymentRow.AmountDiff);
            }
            else
            {
                if (paymentRow.AmountDiff > 0 && amount < 0)
                {
                    diffAmount = Decimal.Negate(paymentRow.AmountDiff);
                }
                else if (paymentRow.AmountDiff > 0 && amount > 0)
                {
                    diffAmount = paymentRow.AmountDiff;
                }
            }

            // Create debt row
            rowNr++;
            PaymentAccountRow assetAccountRow = new PaymentAccountRow()
            {
                RowNr = rowNr,
                Amount = amount - diffAmount,
                Quantity = null,
                Text = "",
                InterimRow = false,
                CreditRow = !customerInvoiceAccountRow.CreditRow, // Turn
                DebitRow = !customerInvoiceAccountRow.DebitRow,   // Turn

                //Set references
                PaymentRow = paymentRow,
                AccountStd = customerInvoiceAccountRow.AccountStd,
            };

            //Set currency amounts
            CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, assetAccountRow, paymentRow.CurrencyRate, actorId);

            //AccountInternal
            foreach (AccountInternal accountInternal in customerInvoiceAccountRow.AccountInternal)
            {
                assetAccountRow.AccountInternal.Add(accountInternal);
            }

            paymentRow.PaymentAccountRow.Add(assetAccountRow);

            #endregion

            #region income row

            // Create income row
            rowNr++;
            PaymentAccountRow incomeAccountRow = new PaymentAccountRow
            {
                RowNr = rowNr,
                Amount = decimal.Negate(amount),
                Quantity = null,
                Text = "",
                InterimRow = false,
                CreditRow = customerInvoiceAccountRow.CreditRow,
                DebitRow = customerInvoiceAccountRow.DebitRow,

                //Set references
                PaymentRow = paymentRow,
                AccountStd = paymentMethod.AccountStd,
            };

            //Set currency amounts
            CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, incomeAccountRow, paymentRow.CurrencyRate, actorId);

            paymentRow.PaymentAccountRow.Add(incomeAccountRow);

            #endregion

            #region diff row            

            // create diff row
            if (diffAmount != 0)
            {
                rowNr++;
                PaymentAccountRow diffAccountRow = new PaymentAccountRow()
                {
                    RowNr = rowNr,
                    Amount = diffAmount,
                    Quantity = null,
                    Text = "",
                    InterimRow = false,
                    CreditRow = diffAmount < 0 ? true : false, // Turn
                    DebitRow = diffAmount < 0 ? false : true,   // Turn

                    //Set references
                    PaymentRow = paymentRow,
                };

                if (matchCode != null && matchCode.AccountId > 0)
                {
                    Account matchAccount = AccountManager.GetAccount(entities, actorCompanyId, matchCode.AccountId, loadAccount: true);

                    if (matchAccount != null)
                    {
                        diffAccountRow.AccountStd = matchAccount.AccountStd;
                    }
                    else
                    {
                        var defaultOverPayAccountId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.AccountCustomerOverpay, 0, actorCompanyId, 0);
                        if (defaultOverPayAccountId == 0)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(708, 32, "Överbetalningskonto saknas, skapandet av betalningar avbröts."));

                        Account overPayAccount = AccountManager.GetAccount(entities, actorCompanyId, defaultOverPayAccountId, loadAccount: true);

                        diffAccountRow.AccountStd = overPayAccount.AccountStd;
                    }

                    if (matchCode.VatAccountId.HasValue && customerInvoice.VatType != (int)TermGroup_InvoiceVatType.Contractor && customerInvoice.VatType != (int)TermGroup_InvoiceVatType.NoVat)
                    {
                        Account matchVatAccount = AccountManager.GetAccount(entities, actorCompanyId, matchCode.VatAccountId.Value, loadAccount: true);
                        if (matchVatAccount != null)
                        {
                            rowNr++;

                            var isLedger = customerInvoice.RegistrationType == (int)OrderInvoiceRegistrationType.Ledger;
                            decimal vatRateValue = 0;
                            if (isLedger)
                            {
                                var defaultVatCode = AccountManager.GetDefaultAccountingVatCode(entities, actorCompanyId);
                                vatRateValue = defaultVatCode != null && defaultVatCode.Percent > 0 ? (defaultVatCode.Percent / 100) : Decimal.Round(customerInvoice.VATAmount / (customerInvoice.TotalAmount - customerInvoice.VATAmount), 3);
                            }
                            else
                            {
                                vatRateValue = Decimal.Round(customerInvoice.VATAmount / customerInvoice.SumAmount, 2);
                            }

                            var diffExVat = Decimal.Round(diffAmount / (1 + vatRateValue), 2);
                            var vatAmount = diffAmount - diffExVat;

                            diffAccountRow.Amount = diffExVat;

                            PaymentAccountRow diffVatAccountRow = new PaymentAccountRow()
                            {
                                RowNr = rowNr,
                                Amount = vatAmount,
                                Quantity = null,
                                Text = "",
                                InterimRow = false,
                                CreditRow = diffAccountRow.CreditRow,
                                DebitRow = diffAccountRow.DebitRow,

                                PaymentRow = paymentRow,
                                AccountStd = matchVatAccount.AccountStd,
                            };

                            //Set currency amounts
                            CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, diffVatAccountRow, paymentRow.CurrencyRate, actorId);

                            paymentRow.PaymentAccountRow.Add(diffVatAccountRow);
                        }
                    }
                }
                else
                {
                    var defaultOverPayAccountId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.AccountCustomerOverpay, 0, actorCompanyId, 0);
                    if (defaultOverPayAccountId == 0)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(708, 32, "Överbetalningskonto saknas, skapandet av betalningar avbröts."));

                    Account overPayAccount = AccountManager.GetAccount(entities, actorCompanyId, defaultOverPayAccountId, loadAccount: true);

                    diffAccountRow.AccountStd = overPayAccount.AccountStd;
                }

                //Set currency amounts
                CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, diffAccountRow, paymentRow.CurrencyRate, actorId);

                paymentRow.PaymentAccountRow.Add(diffAccountRow);
            }

            return result;
            #endregion
        }

        public ActionResult AddCashPaymentAccountRowsFromCustomerInvoice(CompEntities entities, PaymentRow paymentRow, PaymentMethod paymentMethod, CustomerInvoice customerInvoice, int actorCompanyId, MatchCode matchCode)
        {
            ActionResult result = new ActionResult(true);

            int actorId = customerInvoice.ActorId.HasValue ? customerInvoice.ActorId.Value : 0;

            #region CustomerInvoiceAccountRow

            // Get asset account row
            CustomerInvoiceAccountRow customerInvoiceAccountRow = InvoiceManager.GetCustomerInvoiceAssetAccountRow(entities, customerInvoice);
            if (customerInvoiceAccountRow == null)
                return new ActionResult((int)ActionResultSave.PaymentInvoiceAssetRowNotFound, GetText(434, (int)TermGroup.ChangeStatusGrid, "Kontrollera konteringsraderna på fakturan"));

            if (!customerInvoiceAccountRow.AccountStdReference.IsLoaded)
                customerInvoiceAccountRow.AccountStdReference.Load();

            #endregion

            #region PaymentAccountRow

            #region debt row

            int rowNr = 0;
            bool isCredit = customerInvoice.BillingType == (int)TermGroup_BillingType.Credit;
            bool overPay = isCredit ? paymentRow.AmountDiff > 0 : paymentRow.AmountDiff < 0;

            // Calculate amount for row
            decimal amount = CalculatePayment(customerInvoice.BillingType, customerInvoice.TotalAmount, paymentRow.Amount, customerInvoiceAccountRow.Amount);
            decimal diffAmount = paymentRow.AmountDiff;

            // Create debt row
            rowNr++;
            PaymentAccountRow assetAccountRow = new PaymentAccountRow()
            {
                RowNr = rowNr,
                Amount = overPay ? amount - diffAmount : amount,
                Quantity = null,
                Text = "",
                InterimRow = false,
                CreditRow = !customerInvoiceAccountRow.CreditRow, // Turn
                DebitRow = !customerInvoiceAccountRow.DebitRow,   // Turn

                //Set references
                PaymentRow = paymentRow,
                AccountStd = customerInvoiceAccountRow.AccountStd,
            };

            //Set currency amounts
            CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, assetAccountRow, paymentRow.CurrencyRate, actorId);

            //AccountInternal
            foreach (AccountInternal accountInternal in customerInvoiceAccountRow.AccountInternal)
            {
                assetAccountRow.AccountInternal.Add(accountInternal);
            }

            paymentRow.PaymentAccountRow.Add(assetAccountRow);

            #endregion

            #region income row

            // Create income row
            rowNr++;
            PaymentAccountRow incomeAccountRow = new PaymentAccountRow()
            {
                RowNr = rowNr,
                Amount = overPay ? Decimal.Negate(amount) : Decimal.Negate(amount) - diffAmount,
                Quantity = null,
                Text = "",
                InterimRow = false,
                CreditRow = customerInvoiceAccountRow.CreditRow,
                DebitRow = customerInvoiceAccountRow.DebitRow,

                //Set references
                PaymentRow = paymentRow,
                AccountStd = paymentMethod.AccountStd,
            };

            //Set currency amounts
            CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, incomeAccountRow, paymentRow.CurrencyRate, actorId);

            paymentRow.PaymentAccountRow.Add(incomeAccountRow);

            #endregion

            #region diff row            

            // create diff row
            if (diffAmount != 0)
            {
                rowNr++;

                PaymentAccountRow diffAccountRow = new PaymentAccountRow()
                {
                    RowNr = rowNr,
                    Amount = diffAmount,
                    Quantity = null,
                    Text = "",
                    InterimRow = false,
                    CreditRow = overPay ? !customerInvoiceAccountRow.CreditRow : customerInvoiceAccountRow.CreditRow, // Turn
                    DebitRow = overPay ? !customerInvoiceAccountRow.DebitRow : customerInvoiceAccountRow.DebitRow,   // Turn

                    //Set references
                    PaymentRow = paymentRow,
                };

                Account matchAccount = AccountManager.GetAccount(entities, actorCompanyId, matchCode.AccountId, loadAccount: true);

                if (matchAccount != null)
                {
                    diffAccountRow.AccountStd = matchAccount.AccountStd;
                }
                else
                {
                    var defaultOverPayAccountId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.AccountCustomerOverpay, 0, actorCompanyId, 0);
                    Account overPayAccount = AccountManager.GetAccount(entities, actorCompanyId, defaultOverPayAccountId, loadAccount: true);

                    diffAccountRow.AccountStd = overPayAccount.AccountStd;
                }

                if (matchCode.VatAccountId.HasValue && customerInvoice.VatType != (int)TermGroup_InvoiceVatType.Contractor && customerInvoice.VatType != (int)TermGroup_InvoiceVatType.NoVat)
                {
                    Account matchVatAccount = AccountManager.GetAccount(entities, actorCompanyId, matchCode.VatAccountId.Value, loadAccount: true);
                    if (matchVatAccount != null)
                    {
                        rowNr++;

                        var vatRateValue = Decimal.Round(customerInvoice.VATAmount / customerInvoice.SumAmount, 2);
                        var diffExVat = Decimal.Round(paymentRow.AmountDiff / (1 + vatRateValue), 2);
                        var vatAmount = paymentRow.AmountDiff - diffExVat;

                        diffAccountRow.Amount = diffExVat;

                        PaymentAccountRow diffVatAccountRow = new PaymentAccountRow()
                        {
                            RowNr = rowNr,
                            Amount = vatAmount,
                            Quantity = null,
                            Text = "",
                            InterimRow = false,
                            CreditRow = overPay ? !customerInvoiceAccountRow.CreditRow : customerInvoiceAccountRow.CreditRow, // Turn
                            DebitRow = overPay ? !customerInvoiceAccountRow.DebitRow : customerInvoiceAccountRow.DebitRow,   // Turn
                            //Set references
                            PaymentRow = paymentRow,
                            AccountStd = matchVatAccount.AccountStd,
                        };

                        //Set currency amounts
                        CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, diffVatAccountRow, paymentRow.CurrencyRate, actorId);

                        paymentRow.PaymentAccountRow.Add(diffVatAccountRow);

                    }
                }

                //Set currency amounts
                CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, diffAccountRow, paymentRow.CurrencyRate, actorId);

                paymentRow.PaymentAccountRow.Add(diffAccountRow);
            }

            #endregion

            #endregion

            return result;
        }

        public ActionResult AddPaymentRow(CompEntities entities, PaymentRow paymentRow, int accountId, decimal amount, bool isCreditInvoice, bool isDebitRow, bool isVatRow, bool isInterimRow, int rowNr, int actorCompanyId)
        {
            var result = new ActionResult(true);

            // Credit invoice, negate amount
            if (isCreditInvoice)
                amount = Decimal.Negate(amount);

            int actorId = paymentRow.Invoice != null && paymentRow.Invoice.ActorId.HasValue ? paymentRow.Invoice.ActorId.Value : 0;
            decimal creditAmount = (isDebitRow ? (amount > 0 ? 0 : Math.Abs(amount)) : (amount > 0 ? amount : 0));
            decimal debitAmount = (isDebitRow ? (amount > 0 ? amount : 0) : (amount > 0 ? 0 : Math.Abs(amount)));
            var accountStd = AccountManager.GetAccountStd(entities, accountId, actorCompanyId, true, false);

            PaymentAccountRow paymentAccountRow = new PaymentAccountRow()
            {
                RowNr = rowNr,
                Amount = debitAmount - creditAmount,
                Quantity = null,
                Text = "",
                CreditRow = !isDebitRow,
                DebitRow = isDebitRow,
                VatRow = isVatRow,
                InterimRow = isInterimRow,

                //Set FK
                AccountStd = accountStd,
                AccountId = accountId,

                //Set references
                PaymentRow = paymentRow,
            };

            //Set currency amounts
            CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, paymentAccountRow, paymentRow.CurrencyRate, actorId);

            paymentRow.PaymentAccountRow.Add(paymentAccountRow);

            return result;
        }

        public ActionResult HandlePendingDiffAndBankFee(CompEntities entities, PaymentRow paymentRow, SoeOriginType originType, decimal paymentAmount, bool foreign, int actorCompanyId)
        {
            ActionResult result = new ActionResult(true);

            #region Prereq

            if (originType != SoeOriginType.SupplierPayment && originType != SoeOriginType.CustomerPayment)
                return new ActionResult((int)ActionResultSave.InvalidStateTransition);

            //No Diff or BankFee to handle
            if (!paymentRow.HasPendingAmountDiff && !paymentRow.HasPendingBankFee)
                return result;

            bool isCreditInvoice = paymentRow.Invoice.BillingType == (int)TermGroup_BillingType.Credit;
            int rowNr = paymentRow.PaymentAccountRow.Count;

            #endregion

            #region Cash

            //Find Cash row
            foreach (var paymentAccountRow in paymentRow.PaymentAccountRow.Where(p => p.State == (int)SoeEntityState.Active))
            {
                bool isCashRow = false;
                if (originType == SoeOriginType.SupplierPayment)
                {
                    isCashRow = IsSupplierPaymentCashRow(paymentRow.Invoice.BillingType, paymentAccountRow.DebitRow, paymentAccountRow.CreditRow);
                }
                else if (originType == SoeOriginType.CustomerPayment)
                {
                    isCashRow = IsCustomerPaymentCashRow(paymentRow.Invoice.BillingType, paymentAccountRow.DebitRow, paymentAccountRow.CreditRow);
                }

                if (isCashRow)
                {
                    //Set amount (preserve +/-)
                    if (paymentAccountRow.Amount > 0)
                        paymentAccountRow.Amount = Math.Abs(paymentAmount);
                    else
                        paymentAccountRow.Amount = Decimal.Negate(paymentAmount);
                    break;
                }
            }

            #endregion

            #region BankFee

            //SupplierPayment only
            if (paymentRow.HasPendingBankFee && originType == SoeOriginType.SupplierPayment)
            {
                //Get AccountId for BankFee
                int bankFreeAccountId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.AccountCommonBankFee, 0, actorCompanyId, 0);

                //Add row
                rowNr++;
                result = AddPaymentRow(entities, paymentRow, bankFreeAccountId, paymentRow.BankFee, isCreditInvoice, true, false, false, rowNr, actorCompanyId);
                if (!result.Success)
                    return result;

                //Set has handled
                paymentRow.HasPendingBankFee = false;
            }

            #endregion

            #region Diff

            if (paymentRow.HasPendingAmountDiff && (originType == SoeOriginType.SupplierPayment || originType == SoeOriginType.CustomerPayment))
            {
                //Get AccountId for Diff
                int diffAccountType = 0;
                if (originType == SoeOriginType.SupplierPayment)
                {
                    if (paymentRow.AmountDiff > 0)
                        diffAccountType = (foreign ? (int)CompanySettingType.AccountCommonCurrencyLoss : (int)CompanySettingType.AccountSupplierOverpay);
                    else
                        diffAccountType = (foreign ? (int)CompanySettingType.AccountCommonCurrencyProfit : (int)CompanySettingType.AccountSupplierUnderpay);
                }
                else if (originType == SoeOriginType.CustomerPayment)
                {
                    if (paymentRow.AmountDiff > 0)
                        diffAccountType = (foreign ? (int)CompanySettingType.AccountCommonCurrencyProfit : (int)CompanySettingType.AccountCustomerOverpay);
                    else
                        diffAccountType = (foreign ? (int)CompanySettingType.AccountCommonCurrencyLoss : (int)CompanySettingType.AccountCustomerUnderpay);
                }
                int diffAccountId = SettingManager.GetIntSetting(entities, SettingMainType.Company, diffAccountType, 0, actorCompanyId, 0);

                if (diffAccountId == 0)
                {
                    return new ActionResult(7458, GetText(7458, "Inställning för redovisningskonto för över/underbetalning av kund/leverantörsfakturor saknas"));
                }

                //Add row
                rowNr++;
                bool isDebitRow = originType == SoeOriginType.SupplierPayment ? paymentRow.AmountDiff > 0 : paymentRow.AmountDiff < 0;
                result = AddPaymentRow(entities, paymentRow, diffAccountId, Math.Abs(paymentRow.AmountDiff), isCreditInvoice, isDebitRow, false, false, rowNr, actorCompanyId);
                if (!result.Success)
                    return result;

                //Set as handled
                paymentRow.HasPendingAmountDiff = false;
            }

            #endregion

            return result;
        }

        public ActionResult SaveSupplierPaymentDateAndAmounts(List<SupplierPaymentGridDTO> items, int accountYearId, int actorCompanyId, int userId)
        {
            // Default result is successful
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        //Get currency
                        int baseSysCurrencyId = CountryCurrencyManager.GetCompanyBaseSysCurrencyId(entities, base.ActorCompanyId);

                        foreach (var item in items.Where(i => i.IsModified))
                        {
                            #region Prereq

                            PaymentRow paymentRow = GetPaymentRow(entities, item.PaymentRowId);
                            if (paymentRow == null)
                                continue;

                            //Make sure Invoice is loaded
                            if (!paymentRow.InvoiceReference.IsLoaded)
                                paymentRow.InvoiceReference.Load();

                            SupplierInvoice supplierInvoice = paymentRow.Invoice as SupplierInvoice;
                            if (supplierInvoice == null)
                                continue;

                            int actorId = supplierInvoice.ActorId.HasValue ? supplierInvoice.ActorId.Value : 0;

                            #endregion

                            #region Amount

                            decimal diff = item.PaymentAmount - paymentRow.Amount;
                            decimal oldAmount = paymentRow.Amount;
                            decimal oldAmountCurrency = paymentRow.AmountCurrency;
                            if (diff != 0)
                            {

                                if (item.SysCurrencyId != baseSysCurrencyId)
                                {
                                    //Validate pay amount. Same check in GUI
                                    result = ValidatePaymentAmount(item.PaymentAmountCurrency, item.TotalAmountCurrency, item.PaymentSeqNr);
                                    if (!result.Success)
                                        return result;

                                    paymentRow.AmountCurrency = item.PaymentAmountCurrency;

                                    if (diff > 0)
                                    {
                                        //Payed more. Increase Amount on SupplierInvoice
                                        supplierInvoice.PaidAmountCurrency += diff;
                                    }
                                    else
                                    {
                                        //Payed less. Decrease Amount on SupplierInvoice
                                        supplierInvoice.PaidAmountCurrency -= Decimal.Negate(diff); //turn
                                    }

                                    CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, paymentRow, true);
                                    CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, supplierInvoice, actorId);
                                    //Change amount on PaymentAccountRow
                                    //Make sure PaymentAccountRow is loaded
                                    if (!paymentRow.PaymentAccountRow.IsLoaded)
                                        paymentRow.PaymentAccountRow.Load();
                                    foreach (var accountRow in paymentRow.PaymentAccountRow)
                                    {
                                        if (item.TotalAmountCurrency < 0)
                                        {
                                            if (accountRow.AmountCurrency < 0)
                                                accountRow.AmountCurrency += diff;
                                            else
                                                accountRow.AmountCurrency -= diff;
                                        }
                                        else
                                        {
                                            if (accountRow.AmountCurrency < 0)
                                                accountRow.AmountCurrency -= diff;
                                            else
                                                accountRow.AmountCurrency += diff;
                                        }
                                    }
                                }
                                else
                                {
                                    //Validate pay amount. Same check in GUI
                                    result = ValidatePaymentAmount(item.PaymentAmount, item.TotalAmount, item.PaymentSeqNr);
                                    if (!result.Success)
                                        return result;

                                    paymentRow.Amount = item.PaymentAmount;

                                    if (diff > 0)
                                    {
                                        //Payed more. Increase Amount on Invoice
                                        supplierInvoice.PaidAmount += diff;
                                    }
                                    else
                                    {
                                        //Payed less. Decrease Amount on Invoice
                                        supplierInvoice.PaidAmount -= Decimal.Negate(diff); //turn
                                    }

                                    CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, paymentRow);
                                    CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, supplierInvoice, actorId);

                                    //Change amount on PaymentAccountRow
                                    //Make sure PaymentAccountRow is loaded
                                    if (!paymentRow.PaymentAccountRow.IsLoaded)
                                        paymentRow.PaymentAccountRow.Load();
                                    foreach (var accountRow in paymentRow.PaymentAccountRow)
                                    {
                                        if (item.TotalAmount < 0)
                                        {
                                            if (accountRow.Amount < 0)
                                                accountRow.Amount += diff;
                                            else
                                                accountRow.Amount -= diff;
                                        }
                                        else
                                        {
                                            if (accountRow.Amount < 0)
                                                accountRow.Amount -= diff;
                                            else
                                                accountRow.Amount += diff;
                                        }
                                        CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, accountRow, paymentRow.CurrencyRate, (int)supplierInvoice.ActorId);
                                    }
                                }

                                //FullyPayed
                                InvoiceManager.SetInvoiceFullyPayed(entities, paymentRow.Invoice, paymentRow.Invoice.IsTotalAmountPayed);
                                result = AddPaymentRowWhenDiff(entities, paymentRow, supplierInvoice, diff, actorCompanyId, item.SysCurrencyId != baseSysCurrencyId);
                                if (!result.Success)
                                    return result;
                            }

                            #endregion

                            #region PayDate

                            if (item.PayDate.HasValue && paymentRow.PayDate != item.PayDate.Value)
                            {
                                result = ValidateNewPaymentRowDate(entities, paymentRow, item, actorCompanyId);
                                if (result.Success)
                                    paymentRow.PayDate = item.PayDate.Value;
                            }

                            #endregion
                        }

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
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            return result;
        }

        public ActionResult SaveSupplierPaymentDate(List<SupplierPaymentGridDTO> items, DateTime newPayDate, int actorCompanyId)
        {
            // Default result is successful
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        foreach (var item in items)
                        {
                            #region Prereq

                            PaymentRow paymentRow = GetPaymentRow(entities, item.PaymentRowId);
                            if (paymentRow == null)
                                continue;

                            #endregion

                            #region PayDate

                            /* newPayDate must override old paydate, else error on wrong accountyear */
                            if (newPayDate != null && item != null)
                                item.PayDate = newPayDate;

                            result = ValidateNewPaymentRowDate(entities, paymentRow, item, actorCompanyId);
                            if (result.Success)
                            {
                                paymentRow.PayDate = newPayDate;
                            }
                            else
                            {
                                return result;
                            }

                            #endregion
                        }

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
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            return result;
        }

        internal ActionResult AddPaymentRowWhenDiff(CompEntities entities, PaymentRow oldPaymentRow, SupplierInvoice supplierInvoice, decimal diff, int actorCompanyId, bool foreign)
        {
            if (oldPaymentRow == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "PaymentRow");

            // Default result is successful
            var result = new ActionResult();

            #region Prereq

            // Get owner Company for the Invoice
            Company company = CompanyManager.GetCompany(entities, actorCompanyId, false);
            if (company == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

            // Get currency
            Currency currency = CountryCurrencyManager.GetCurrency(entities, supplierInvoice.CurrencyId);
            if (currency == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "Currency");

            // Get payment method
            if (!oldPaymentRow.PaymentReference.IsLoaded)
                oldPaymentRow.PaymentReference.Load();

            PaymentMethod paymentMethod = PaymentManager.GetPaymentMethod(entities, (int)oldPaymentRow.Payment.PaymentMethodId, actorCompanyId);
            if (paymentMethod == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "PaymentMethod");

            if (oldPaymentRow.SysPaymentTypeId == 0 || String.IsNullOrEmpty(oldPaymentRow.PaymentNr))
            {
                if (!paymentMethod.PaymentInformationRow.PaymentInformationReference.IsLoaded)
                    paymentMethod.PaymentInformationRowReference.Load();

                // Get payment type and number from PaymentInformationRow
                oldPaymentRow.SysPaymentTypeId = paymentMethod.PaymentInformationRow.SysPaymentTypeId;
                oldPaymentRow.PaymentNr = paymentMethod.PaymentInformationRow.PaymentNr;
                if (String.IsNullOrEmpty(oldPaymentRow.PaymentNr))
                    oldPaymentRow.PaymentNr = "-";
            }

            #endregion

            #region Origin

            // Create new origin
            if (!oldPaymentRow.Payment.OriginReference.IsLoaded)
                oldPaymentRow.Payment.OriginReference.Load();

            Origin origin = new Origin()
            {
                Type = oldPaymentRow.Payment.Origin.Type,
                Status = oldPaymentRow.Payment.Origin.Status,
                Description = oldPaymentRow.Payment.Origin.Description,

                //Set FK
                VoucherSeriesId = oldPaymentRow.Payment.Origin.VoucherSeriesId,
                VoucherSeriesTypeId = oldPaymentRow.Payment.Origin.VoucherSeriesTypeId,

                //Set references
                Company = company,
            };
            SetCreatedProperties(origin);
            entities.Origin.AddObject(origin);
            #endregion

            #region Payment

            // Create new Payment

            Payment payment = new Payment()
            {
                PaymentId = origin.OriginId,
                Origin = origin,
                PaymentMethod = paymentMethod,
            };
            SetCreatedProperties(payment);
            entities.Payment.AddObject(payment);

            #endregion

            #region Invoice

            // Get existing Invoice or create new
            Invoice invoice = supplierInvoice;
            if (foreign)
                invoice.PaidAmountCurrency -= diff;
            else
                invoice.PaidAmount -= diff;

            //invoice.PaymentNr = paymentRowInput.PaymentNr;
            invoice.FullyPayed = oldPaymentRow.Invoice.IsTotalAmountPayed;

            SetModifiedProperties(invoice);


            #endregion

            #region PaymentRow

            // Get existing PaymentRow or create new
            PaymentRow paymentRow = new PaymentRow()
            {
                Status = oldPaymentRow.Status,
                State = oldPaymentRow.State,
                SeqNr = oldPaymentRow.SeqNr,
                SysPaymentTypeId = oldPaymentRow.SysPaymentTypeId,
                PaymentNr = oldPaymentRow.PaymentNr,
                PayDate = oldPaymentRow.PayDate,
                CurrencyRate = oldPaymentRow.CurrencyRate,
                CurrencyDate = oldPaymentRow.CurrencyDate,
                Amount = -diff,
                AmountCurrency = -diff,
                AmountDiff = oldPaymentRow.AmountDiff,
                AmountDiffCurrency = oldPaymentRow.AmountDiffCurrency,        // TODO: Calculate currency
                HasPendingAmountDiff = oldPaymentRow.HasPendingAmountDiff,
                HasPendingBankFee = oldPaymentRow.HasPendingBankFee,

                //Set references
                Payment = payment,
                Invoice = invoice,
            };
            SetCreatedProperties(paymentRow);

            //Set currency amounts
            CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, paymentRow, foreign);

            #region PaymentAccountRow

            // Add new PaymentAccountRows
            foreach (var accountRow in oldPaymentRow.ActivePaymentAccountRows)
            {
                decimal amount = (accountRow.DebitRow) ? Math.Abs(diff) : decimal.Negate(Math.Abs(diff));

                PaymentAccountRow par = new PaymentAccountRow()
                {
                    RowNr = accountRow.RowNr,
                    Amount = amount,
                    Quantity = null,
                    Text = "",
                    CreditRow = accountRow.CreditRow,
                    DebitRow = accountRow.DebitRow,
                    VatRow = accountRow.VatRow,
                    InterimRow = accountRow.InterimRow,

                    //Set FK
                    AccountId = accountRow.AccountId,

                    //Set references
                    PaymentRow = paymentRow,
                };

                //Set currency amounts
                CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, par, paymentRow.CurrencyRate, (int)invoice.ActorId);

                paymentRow.PaymentAccountRow.Add(par);
            }

            entities.PaymentRow.AddObject(paymentRow);

            #endregion



            #endregion

            return result;
        }

        internal ActionResult ValidatePaymentAmount(decimal payAmount, decimal totalAmount, int paymentSeqNr)
        {
            ActionResult result = new ActionResult(true);

            if (totalAmount > 0)
            {
                //Debit - cannot pay zero or above total amount
                bool invalid = payAmount == 0;
                if (invalid)
                    result = new ActionResult((int)ActionResultSave.PaymentIncorrectAmount, paymentSeqNr.ToString());
            }
            else
            {
                //Credit - cannot pay zero or below total amount
                bool invalid = payAmount == 0 || payAmount < totalAmount;
                if (invalid)
                    result = new ActionResult((int)ActionResultSave.PaymentIncorrectAmount, paymentSeqNr.ToString());
            }

            return result;
        }

        internal ActionResult ValidateOverPaymentAccount(CompEntities entities, decimal amountDiff, int actorCompanyId)
        {
            ActionResult result = new ActionResult(true);

            if (amountDiff != 0)
            {
                var defaultOverPayAccountId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.AccountCustomerOverpay, 0, actorCompanyId, 0);
                if (defaultOverPayAccountId == 0)
                    result = new ActionResult((int)ActionResultSave.AccountCustomerOverpayNotSpecified, string.Empty);
            }

            return result;
        }

        private ActionResult ValidateNewPaymentRowDate(CompEntities entities, PaymentRow paymentRow, SupplierPaymentGridDTO item, int actorCompanyId)
        {
            if (paymentRow == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "PaymentRow");
            if (item == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "ChangeStatusGridView");

            DateTime newPayDate = item.PayDate.Value;
            string paymentSeqNr = item.PaymentSeqNr.ToString();

            AccountYear currentAccountYear = AccountManager.GetAccountYear(entities, paymentRow.PayDate, actorCompanyId);
            if (currentAccountYear == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "AccountYear");

            AccountYear newAccountYear = AccountManager.GetAccountYear(entities, newPayDate, actorCompanyId);
            if (newAccountYear == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "AccountYear");

            //Must have same AccountYear... Task 9647 Must be able to pay to other account year (Needs to be open)
            //if (currentAccountYear.AccountYearId != newAccountYear.AccountYearId)
            //   return new ActionResult((int)ActionResultSave.PaymentIncorrectDateAccountYear, paymentSeqNr);

            AccountPeriod newAccountPeriod = AccountManager.GetAccountPeriod(entities, newAccountYear.AccountYearId, newPayDate, actorCompanyId);
            if (newAccountPeriod == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "AccountPeriod");

            //Muste be open
            if (newAccountPeriod.Status != (int)TermGroup_AccountStatus.Open)
                return new ActionResult((int)ActionResultSave.PaymentIncorrectDateAccountPeriod, paymentSeqNr);

            return new ActionResult(true);
        }

        #endregion

        #region PaymentRow

        public List<PaymentRow> GetPaymentRowsByInvoice(int invoiceId, bool setStatusName = false, bool includePaymentMethod = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.PaymentRow.NoTracking();
            return GetPaymentRowsByInvoice(entities, invoiceId, setStatusName, includePaymentMethod);
        }

        public List<PaymentRow> GetPaymentRowsByInvoiceSmall(int invoiceId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.PaymentRow.NoTracking();
            return (from pr in entities.PaymentRow
                    where pr.InvoiceId.HasValue == true &&
                    pr.InvoiceId.Value == invoiceId &&
                    pr.State == (int)SoeEntityState.Active
                    orderby pr.SeqNr
                    select pr).ToList();
        }

        public List<PaymentRow> GetPaymentRowsByInvoice(CompEntities entities, int invoiceId, bool setStatusName = false, bool includePaymentMethod = false)
        {
            IQueryable<PaymentRow> query = (from pr in entities.PaymentRow select pr);

            if (includePaymentMethod)
            {
                query = query.Include("Payment.PaymentMethod");
            }

            var rows = query.Where(x => x.InvoiceId == invoiceId && x.State == (int)SoeEntityState.Active).OrderBy(y => y.SeqNr).ToList();

            if (setStatusName)
            {
                foreach (var paymentRow in rows)
                {
                    paymentRow.StatusName = GetText(paymentRow.Status, (int)TermGroup.PaymentStatus);
                }
            }

            return rows;
        }

        public List<PaymentRow> GetPaymentRowsForExport(Payment payment)
        {
            List<PaymentRow> paymentRows = new List<PaymentRow>();

            if (payment == null || payment.PaymentRow == null)
                return paymentRows;

            foreach (PaymentRow paymentRow in payment.PaymentRow)
            {
                if (!paymentRow.InvoiceReference.IsLoaded)
                    paymentRow.InvoiceReference.Load();

                SupplierInvoice supplierInvoice = paymentRow.Invoice as SupplierInvoice;
                if (supplierInvoice != null)
                {
                    if (!supplierInvoice.ActorReference.IsLoaded)
                        supplierInvoice.ActorReference.Load();

                    if (supplierInvoice.Actor != null && !supplierInvoice.Actor.SupplierReference.IsLoaded)
                        supplierInvoice.Actor.SupplierReference.Load();

                    if (!supplierInvoice.PaymentMethodReference.IsLoaded)
                        supplierInvoice.PaymentMethodReference.Load();

                    if (IsExportable(supplierInvoice.PaymentMethod) && paymentRow.IsCashDiscount == false)
                        paymentRows.Add(paymentRow);
                }
            }

            return paymentRows;
        }

        public PaymentRow GetPaymentRow(int paymentRowId, bool loadInvoiceAndOrigin = false, bool loadAccountRows = false, bool loadAccounts = false, bool checkMultipleRows = false, bool loadOrigin = false, bool loadPaymentExport = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.PaymentRow.NoTracking();
            return GetPaymentRow(entities, paymentRowId, loadInvoiceAndOrigin, loadAccountRows, loadAccounts, checkMultipleRows, loadOrigin, loadPaymentExport);
        }

        public PaymentRow GetPaymentRow(CompEntities entities, int paymentRowId, bool loadInvoiceAndOrigin = false, bool loadAccountRows = false, bool loadAccounts = false, bool checkMultipleRows = false, bool loadOrigin = false, bool loadPaymentExport = false)
        {
            PaymentRow paymentRow = null;
            IQueryable<PaymentRow> query = entities.PaymentRow;

            if (loadInvoiceAndOrigin)
            {
                query = query.Include("Payment.Origin.VoucherSeries").Include("Invoice");
            }
            else if (loadOrigin)
            {
                query = query.Include("Payment.Origin");
            }

            if (loadAccountRows || loadAccounts)
            {
                query = query.Include("PaymentAccountRow.AccountStd.Account")
                              .Include("PaymentAccountRow.AccountInternal.Account.AccountDim");
            }

            if (loadPaymentExport)
            {
                query = query.Include("Payment.PaymentExport");
            }

            paymentRow = (from pr in query
                          where pr.PaymentRowId == paymentRowId &&
                          pr.State == (int)SoeEntityState.Active
                          select pr).FirstOrDefault();

            if (checkMultipleRows && paymentRow.VoucherHeadId.HasValue)
            {
                paymentRow.VoucherHasMultiplePayments = (from pr in entities.PaymentRow
                                                         where pr.VoucherHeadId == paymentRow.VoucherHeadId.Value
                                                         select pr).Count() > 1;
            }

            paymentRow.StatusName = GetText(paymentRow.Status, (int)TermGroup.PaymentStatus);

            if (loadPaymentExport)
            {
                paymentRow.TransferStatus = paymentRow?.Payment?.PaymentExport?.TransferStatus ?? 0;
                if (string.IsNullOrEmpty(paymentRow.StatusMsg) && paymentRow.TransferStatus != 0 &&
                    paymentRow.TransferStatus != (int)TermGroup_PaymentTransferStatus.Transfered &&
                    paymentRow.TransferStatus != (int)TermGroup_PaymentTransferStatus.Completed)
                {
                    paymentRow.StatusMsg = GetText(paymentRow.Payment.PaymentExport.TransferStatus, (int)TermGroup.PaymentTransferStatus);
                }
            }

            return paymentRow;
        }

        public List<PaymentRowInvoiceDTO> GetPaymentRows(SoeOriginType originType, int actorCompanyId, PaymentSearchDTO searchDTO)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.PaymentRow.NoTracking();
            return GetPaymentRows(entities, originType, actorCompanyId, searchDTO);
        }

        public List<PaymentRowInvoiceDTO> GetPaymentRows(CompEntities entities, SoeOriginType originType, int actorCompanyId, PaymentSearchDTO searchDTO)
        {
            // Workaround for error in service call

            int type = (int)originType;
            bool filterSet = false;
            IQueryable<PaymentRow> query = (from pr in entities.PaymentRow
                                            where pr.Payment.Origin.ActorCompanyId == actorCompanyId &&
                                                pr.Payment.Origin.Type == type &&
                                                pr.State == (int)SoeEntityState.Active
                                            orderby pr.SeqNr
                                            select pr);

            if (searchDTO.InvoiceId.GetValueOrDefault() > 0)
            {
                query = query.Where(p => p.InvoiceId == searchDTO.InvoiceId.Value);
                filterSet = true;
            }

            if (searchDTO.PayDateFrom.HasValue)
            {
                query = query.Where(p => p.PayDate >= searchDTO.PayDateFrom.Value);
                filterSet = true;
            }

            if (searchDTO.PayDateTo.HasValue)
            {
                query = query.Where(p => p.PayDate <= searchDTO.PayDateTo.Value);
                filterSet = true;
            }

            if (searchDTO.ModifiedFrom.HasValue)
            {
                query = query.Where(p => p.Created >= searchDTO.ModifiedFrom.Value || p.Modified >= searchDTO.ModifiedFrom.Value);
                filterSet = true;
            }

            if (searchDTO.ModifiedTo.HasValue)
            {
                query = query.Where(p => p.Created <= searchDTO.ModifiedTo.Value || p.Modified <= searchDTO.ModifiedTo.Value);
                filterSet = true;
            }

            if (searchDTO.CreatedFrom.HasValue)
            {
                query = query.Where(p => p.Created >= searchDTO.CreatedFrom.Value);
                filterSet = true;
            }

            if (searchDTO.CreatedTo.HasValue)
            {
                query = query.Where(p => p.Created <= searchDTO.CreatedTo.Value);
                filterSet = true;
            }

            //Some sort of filter must be used...
            if (!filterSet)
            {
                return new List<PaymentRowInvoiceDTO>();
            }

            return query.Select(pr => new PaymentRowInvoiceDTO
            {
                PaymentRowId = pr.PaymentRowId,
                PaymentNr = pr.PaymentNr,
                SeqNr = pr.SeqNr,
                CurrencyId = pr.Invoice.CurrencyId,
                Amount = pr.Amount,
                AmountCurrency = pr.AmountCurrency,
                Status = pr.Status,
                PayDate = pr.PayDate,
                InvoiceId = pr.InvoiceId ?? 0,
                PaymentId = pr.Payment.PaymentId,
                PaymentMethodId = pr.Payment.PaymentMethodId ?? 0,
                InvoiceActorId = pr.Invoice.ActorId,
                InvoiceNr = pr.Invoice.InvoiceNr,
                InvoiceSeqNr = pr.Invoice.SeqNr,
                InvoiceDate = pr.Invoice.InvoiceDate,
                InvoiceDueDate = pr.Invoice.DueDate,
                InvoicePaidAmount = pr.Invoice.PaidAmount,
                InvoiceTotalAmount = pr.Invoice.TotalAmount,
                FullyPayed = pr.Invoice.FullyPayed,
                CurrencyDate = pr.CurrencyDate,
                CurrencyRate = pr.CurrencyRate,
                SysPaymentTypeId = pr.SysPaymentTypeId,
            }).ToList();
        }

        public PaymentRowInvoiceDTO GetPaymentRowWithSupplierInvoice(CompEntities entities, int paymentRowId, int actorCompanyId)
        {
            return (from pr in entities.PaymentRow
                    where pr.PaymentRowId == paymentRowId &&
                   pr.State == (int)SoeEntityState.Active &&
                   pr.Invoice.Origin.ActorCompanyId == actorCompanyId
                    select new PaymentRowInvoiceDTO
                    {
                        PaymentId = pr.Payment.PaymentId,
                        PaymentRowId = pr.PaymentRowId,
                        CurrencyId = pr.Invoice.CurrencyId,
                        Amount = pr.Amount,
                        AmountCurrency = pr.AmountCurrency,
                        Status = pr.Status,
                        PayDate = pr.PayDate,
                        InvoiceActorId = pr.Invoice.ActorId,
                        InvoiceActorName = pr.Invoice.Actor.Supplier.Name,
                        InvoiceId = pr.InvoiceId ?? 0,
                        InvoiceNr = pr.Invoice.InvoiceNr,
                        InvoiceSeqNr = pr.Invoice.SeqNr,
                        InvoiceDate = pr.Invoice.InvoiceDate,
                        InvoiceDueDate = pr.Invoice.DueDate,
                        InvoicePaidAmount = pr.Invoice.PaidAmount,
                        InvoiceTotalAmount = pr.Invoice.TotalAmount,
                        FullyPayed = pr.Invoice.FullyPayed,
                        BillingType = pr.Invoice.BillingType
                    }).FirstOrDefault();
        }

        public List<PaymentRowInvoiceDTO> GetPaymentRowsWithSupplierInvoice(CompEntities entities, int paymentId, int actorCompanyId, int? actorId = null, TermGroup_BillingType billingType = TermGroup_BillingType.None)
        {
            // Get payment rows which are active.
            // We exclude cancelled payment rows to prevent including rows that should not be used for matching.
            IQueryable<PaymentRow> query = (from pr in entities.PaymentRow
                                            where pr.Payment.PaymentId == paymentId &&
                                               pr.State == (int)SoeEntityState.Active &&
                                               pr.Status != (int)SoePaymentStatus.Cancel &&
                                               pr.Invoice.Origin.ActorCompanyId == actorCompanyId
                                            select pr);

            if (actorId.GetValueOrDefault() > 0)
            {
                query = query.Where(x => x.Invoice.ActorId == actorId);
            }

            if (actorId.GetValueOrDefault() > 0)
            {
                query = query.Where(x => x.Invoice.ActorId == actorId);
            }

            if (billingType != TermGroup_BillingType.None)
            {
                query = query.Where(x => x.Invoice.BillingType == (int)billingType);
            }

            return query.Select(pr => new PaymentRowInvoiceDTO
            {
                PaymentId = pr.Payment.PaymentId,
                PaymentRowId = pr.PaymentRowId,
                CurrencyId = pr.Invoice.CurrencyId,
                Amount = pr.Amount,
                AmountCurrency = pr.AmountCurrency,
                Status = pr.Status,
                PayDate = pr.PayDate,
                InvoiceActorId = pr.Invoice.ActorId,
                PaymentNr = pr.PaymentNr,
                InvoiceActorName = pr.Invoice.Actor.Supplier.Name,
                InvoiceId = pr.InvoiceId ?? 0,
                InvoiceNr = pr.Invoice.InvoiceNr,
                InvoiceSeqNr = pr.Invoice.SeqNr,
                InvoiceDate = pr.Invoice.InvoiceDate,
                InvoiceDueDate = pr.Invoice.DueDate,
                InvoicePaidAmount = pr.Invoice.PaidAmount,
                InvoiceTotalAmount = pr.Invoice.TotalAmount,
                FullyPayed = pr.Invoice.FullyPayed,
                BillingType = pr.Invoice.BillingType
            }).ToList();

        }

        public PaymentRow GetPaymentRowWithAllReferences(CompEntities entities, int paymentRowId)
        {
            return (from pr in entities.PaymentRow
                            .Include("Payment.Origin.VoucherSeries")
                            .Include("Payment.PaymentMethod")
                            .Include("PaymentAccountRow.AccountStd.Account")
                            .Include("PaymentAccountRow.AccountInternal.Account.AccountDim")
                            .Include("Invoice.Currency")
                            .Include("Invoice.Actor.Supplier")
                            .Include("Invoice.Actor.Customer")
                    where pr.PaymentRowId == paymentRowId
                    select pr).FirstOrDefault();
        }

        public DateTime? GetLastPaymentDateForInvoice(CompEntities entities, int invoiceId)
        {
            PaymentRow paymentRow = (from pr in entities.PaymentRow
                                     where pr.InvoiceId.HasValue &&
                                     pr.InvoiceId.Value == invoiceId &&
                                     !pr.IsSuggestion &&
                                     pr.Status != (int)SoePaymentStatus.Cancel
                                     orderby pr.PayDate descending
                                     select pr).FirstOrDefault();

            return paymentRow != null ? paymentRow.PayDate : (DateTime?)null;
        }


        public bool IsSupplierPaymentCashRow(int billingType, bool isDebitRow, bool isCreditRow)
        {
            bool isCashRow = false;

            if (billingType == (int)TermGroup_BillingType.Credit)
                isCashRow = isDebitRow;
            else
                isCashRow = isCreditRow;

            //if (billingType == (int)TermGroup_BillingType.Debit && isCreditRow)
            //    isCashRow = true;
            //else if (billingType == (int)TermGroup_BillingType.Credit && isDebitRow)
            //    isCashRow = true;
            //else if (billingType == (int)TermGroup_BillingType.Interest && isCreditRow)
            //    isCashRow = true;
            //else if (billingType == (int)TermGroup_BillingType.Claim && isCreditRow)
            //    isCashRow = true;

            return isCashRow;
        }

        public bool IsCustomerPaymentCashRow(int billingType, bool isDebitRow, bool isCreditRow)
        {
            bool isCashRow = false;

            if (billingType == (int)TermGroup_BillingType.Credit)
                isCashRow = isCreditRow;
            else
                isCashRow = isDebitRow;

            //if (billingType == (int)TermGroup_BillingType.Debit && isDebitRow)
            //    isCashRow = true;
            //else if (billingType == (int)TermGroup_BillingType.Credit && isCreditRow)
            //    isCashRow = true;
            //else if (billingType == (int)TermGroup_BillingType.Interest && isDebitRow)
            //    isCashRow = true;
            //else if (billingType == (int)TermGroup_BillingType.Claim && isDebitRow)
            //    isCashRow = true;

            return isCashRow;
        }

        public ActionResult UpdatePaymentRow(CompEntities entities, TransactionScope transaction, PaymentRowSaveDTO paymentRowInput, List<AccountingRowDTO> accountingRowDTOsInput, int actorCompanyId)
        {
            var result = new ActionResult(true);

            var paymentRow = GetPaymentRow(entities, (int)paymentRowInput.paymentRowId, true);
            if (paymentRow == null)
                return result;

            var previousAmount = paymentRow.Amount;
            var previousAmountCurrency = paymentRow.AmountCurrency;

            paymentRow.Text = paymentRowInput.Text;
            if (paymentRow.Payment.Origin != null)
            {
                paymentRow.Payment.Origin.Description = paymentRowInput.OriginDescription;
                paymentRow.Payment.Origin.VoucherSeriesId = paymentRowInput.VoucherSeriesId;
                SetModifiedProperties(paymentRow.Payment.Origin);
            }

            paymentRow.Payment.PaymentMethodId = paymentRowInput.PaymentMethodId;
            paymentRow.Amount = paymentRowInput.Amount;
            paymentRow.AmountCurrency = paymentRowInput.AmountCurrency;
            if (paymentRowInput.PaymentDate.HasValue)
            {
                paymentRow.PayDate = paymentRowInput.PaymentDate.Value;
            }

            List<PaymentAccountRow> paymentAccountRowsInput = null;
            if (accountingRowDTOsInput != null)
            {
                //Convert collection of AccountingRowDTOs to collection of PaymentAccountRows
                try
                {
                    paymentAccountRowsInput = ConvertToPaymentAccountRows(entities, paymentRowInput, accountingRowDTOsInput, paymentRowInput.ActorId, actorCompanyId);
                }
                catch (SoeEntityNotFoundException ex)
                {
                    result.Success = false;
                    result.ErrorMessage = string.Format(GetText(1146), ex.InnerException.Message);
                    result.ErrorNumber = (int)ActionResultSave.EntityNotFound;
                    return result;
                }
            }

            if (!paymentRow.PaymentAccountRow.IsLoaded)
            {
                paymentRow.PaymentAccountRow.Load();
            }

            foreach (var existingAccountRow in paymentRow.PaymentAccountRow)
            {
                //var result2 = ChangeEntityState(entities,existingAccountRow, SoeEntityState.Deleted,false);
                existingAccountRow.State = (int)SoeEntityState.Deleted;
                SetModifiedProperties(existingAccountRow);
            }

            foreach (PaymentAccountRow paymentAccountRowInput in paymentAccountRowsInput)
            {
                paymentRow.PaymentAccountRow.Add(paymentAccountRowInput);
            }

            // Invoice
            var invoice = SupplierInvoiceManager.GetSupplierInvoice(entities, paymentRowInput.InvoiceId, false, false, false, false, false, false, false);
            if (invoice != null)
            {
                invoice.PaidAmount = (invoice.PaidAmount - previousAmount + paymentRowInput.Amount);
                invoice.PaidAmountCurrency = (invoice.PaidAmountCurrency - previousAmountCurrency + paymentRowInput.AmountCurrency);
                invoice.FullyPayed = paymentRowInput.FullyPayed;
                SetModifiedProperties(invoice);
            }

            result = SaveChanges(entities, transaction);

            return result;
        }

        public ActionResult SavePaymentRow(PaymentRowSaveDTO paymentRowInput, List<AccountingRowDTO> accountingRowDTOsInput, bool isManualPayment, int matchCodeId, int actorCompanyId)
        {
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    if (paymentRowInput != null)
                    {
                        paymentRowInput.Amount = NumberUtility.GetFormattedDecimalValue(paymentRowInput.Amount, 2);
                        paymentRowInput.AmountCurrency = NumberUtility.GetFormattedDecimalValue(paymentRowInput.AmountCurrency, 2);
                        paymentRowInput.AmountDiff = NumberUtility.GetFormattedDecimalValue(paymentRowInput.AmountDiff, 2);
                        paymentRowInput.AmountDiffCurrency = NumberUtility.GetFormattedDecimalValue(paymentRowInput.AmountDiffCurrency, 2);
                        paymentRowInput.TotalAmount = NumberUtility.GetFormattedDecimalValue(paymentRowInput.TotalAmount, 2);
                        paymentRowInput.TotalAmountCurrency = NumberUtility.GetFormattedDecimalValue(paymentRowInput.TotalAmountCurrency, 2);
                        paymentRowInput.VatAmount = NumberUtility.GetFormattedDecimalValue(paymentRowInput.VatAmount, 2);
                        paymentRowInput.VatAmountCurrency = NumberUtility.GetFormattedDecimalValue(paymentRowInput.VatAmountCurrency, 2);
                        paymentRowInput.PaidAmount = NumberUtility.GetFormattedDecimalValue(paymentRowInput.PaidAmount, 2);
                        paymentRowInput.PaidAmountCurrency = NumberUtility.GetFormattedDecimalValue(paymentRowInput.PaidAmountCurrency, 2);
                    }

                    if (!accountingRowDTOsInput.IsNullOrEmpty())
                    {
                        accountingRowDTOsInput.ForEach(x =>
                        {
                            x.Amount = NumberUtility.GetFormattedDecimalValue(x.Amount, 2);
                            x.CreditAmount = NumberUtility.GetFormattedDecimalValue(x.CreditAmount, 2);
                            x.DebitAmount = NumberUtility.GetFormattedDecimalValue(x.DebitAmount, 2);
                        }
                        );
                    }

                    if (entities.Connection.State != ConnectionState.Open)
                        entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        int seqNr = 0;
                        //int paymentRowId = 0;

                        if ((paymentRowInput.paymentRowId != null && paymentRowInput.paymentRowId > 0))
                        {
                            result = UpdatePaymentRow(entities, transaction, paymentRowInput, accountingRowDTOsInput, actorCompanyId);
                            if (!result.Success)
                                return result;
                        }
                        else
                        {
                            result = SavePaymentRow(entities, transaction, paymentRowInput, accountingRowDTOsInput, actorCompanyId, isManualPayment, false);
                            if (!result.Success)
                                return result;

                            if (result.Success && paymentRowInput.FullyPayed && paymentRowInput.AmountDiff != 0)
                            {
                                var paymentRowInputRest = paymentRowInput.CloneDTO();
                                paymentRowInputRest.Amount = -paymentRowInput.AmountDiff;
                                paymentRowInputRest.AmountCurrency = -paymentRowInput.AmountDiffCurrency;
                                paymentRowInputRest.AmountDiff = 0;
                                paymentRowInputRest.AmountDiffCurrency = 0;
                                paymentRowInputRest.PaidAmount = -paymentRowInput.AmountDiff;
                                paymentRowInputRest.SeqNr = 0;
                                paymentRowInputRest.IsRestPayment = true;
                                paymentRowInputRest.Text = paymentRowInput.Text;

                                var result2 = SavePaymentRow(entities, transaction, paymentRowInputRest, null, actorCompanyId, isManualPayment, false);
                            }

                            #region Payment matching

                            if (matchCodeId != 0)
                            {
                                var paymentId = result.IntegerValue;
                                seqNr = (int)result.Value;

                                var dto = new InvoiceMatchingDTO()
                                {
                                    PaymentRowId = paymentId,
                                    Type = paymentRowInput.InvoiceType == SoeInvoiceType.SupplierInvoice ? (int)SoeEntityType.SupplierPayment : (int)SoeEntityType.CustomerPayment,
                                };

                                result = InvoiceManager.AddInvoicePaymentMatch(entities, transaction, new List<InvoiceMatchingDTO>() { dto }, actorCompanyId, matchCodeId);
                                result.IntegerValue = paymentId;
                                result.Value = seqNr;
                            }

                            #endregion
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

        public ActionResult SavePaymentRow(CompEntities entities, TransactionScope transaction, List<PaymentRowSaveDTO> paymentRowsToSave, int actorCompanyId, bool isManualPayment, bool import, bool adjustInvoiceAmounts = true, bool autoAddVoucher = false, bool autoAddCurrencyAmounts = false)
        {
            if (paymentRowsToSave.IsNullOrEmpty())
                return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(7598, "Hittade inga betalningar att importera"));

            // Default result is successful
            ActionResult result = new ActionResult();

            Dictionary<int, int> voucherHeadsDict = null;
            List<int> paymentRowIds = new List<int>();

            // Get owner Company for the Invoice
            Company company = CompanyManager.GetCompany(entities, actorCompanyId, false);
            if (company == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

            try
            {
                foreach (var paymentGroup in paymentRowsToSave.GroupBy(x => new { x.PaymentDate, x.InvoiceId, x.InvoiceNr, x.OriginId, x.OriginType, x.InvoiceType, x.VoucherSeriesId, x.VoucherSeriesTypeId, x.AccountYearId, x.CurrencyId }))
                {
                    #region Prereq

                    var paymentRows = new List<PaymentRow>();
                    var first = paymentGroup.First();

                    // Get currency
                    Currency currency = CountryCurrencyManager.GetCurrency(entities, paymentGroup.Key.CurrencyId);
                    if (currency == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "Currency");

                    // Get existing Invoice or create new
                    Invoice invoice = paymentGroup.Key.InvoiceId > 0 ? InvoiceManager.GetInvoice(entities, paymentGroup.Key.InvoiceId) : null;

                    #endregion

                    foreach (var paymentInput in paymentGroup)
                    {
                        #region Prereq

                        // Get payment method
                        PaymentMethod paymentMethod = PaymentManager.GetPaymentMethod(entities, paymentInput.PaymentMethodId, actorCompanyId, autoAddVoucher);
                        if (paymentMethod == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, $"PaymentMethod is missing ( {paymentInput.PaymentMethodId} )");

                        if (paymentMethod.PaymentInformationRow != null && (paymentInput.SysPaymentTypeId == 0 || string.IsNullOrEmpty(paymentInput.PaymentNr)))
                        {
                            if (!paymentMethod.PaymentInformationRow.PaymentInformationReference.IsLoaded)
                                paymentMethod.PaymentInformationRowReference.Load();

                            // Get payment type and number from PaymentInformationRow
                            paymentInput.SysPaymentTypeId = paymentMethod.PaymentInformationRow.SysPaymentTypeId;
                            paymentInput.PaymentNr = paymentMethod.PaymentInformationRow.PaymentNr;
                            if (string.IsNullOrEmpty(paymentInput.PaymentNr))
                                paymentInput.PaymentNr = "-";
                        }

                        #endregion

                        #region Origin

                        // Get existing origin or create new
                        Origin origin = paymentInput.OriginId > 0 ? OriginManager.GetOrigin(entities, paymentInput.OriginId) : null;
                        if (origin == null)
                        {
                            #region Add

                            if (paymentGroup.Key.VoucherSeriesId == 0)
                            {
                                return new ActionResult(GetText(8403, "Verifikatserie saknas") + ": " + paymentGroup.Key.PaymentDate);
                            }

                            origin = new Origin()
                            {
                                Type = (int)paymentGroup.Key.OriginType,
                                Status = (int)first.OriginStatus,
                                Description = first.OriginDescription,

                                //Set FK
                                VoucherSeriesId = paymentGroup.Key.VoucherSeriesId,
                                VoucherSeriesTypeId = paymentGroup.Key.VoucherSeriesTypeId,

                                //Set references
                                Company = company,
                            };
                            SetCreatedProperties(origin);
                            entities.Origin.AddObject(origin);

                            #endregion
                        }

                        #endregion

                        #region Payment

                        // Get existing Payment or create new
                        Payment payment = null;
                        if (import)
                        {
                            #region Import

                            if (paymentInput.OriginId > 0)
                            {
                                payment = PaymentManager.GetPayment(entities, paymentInput.OriginId, true, false);
                            }
                            else
                            {
                                payment = new Payment()
                                {
                                    PaymentId = origin.OriginId,

                                    //Set references
                                    Origin = origin,
                                    PaymentMethod = paymentMethod,
                                };
                                SetCreatedProperties(payment);
                                entities.Payment.AddObject(payment);
                            }

                            #endregion
                        }
                        else
                        {
                            if (paymentInput.OriginId == 0)
                            {
                                if (payment == null)
                                {
                                    #region Add

                                    payment = new Payment()
                                    {
                                        PaymentId = origin.OriginId,
                                        Origin = origin,
                                        PaymentMethod = paymentMethod,
                                    };
                                    SetCreatedProperties(payment);
                                    entities.Payment.AddObject(payment);

                                    #endregion
                                }
                                else
                                {
                                    #region Update

                                    payment.PaymentMethod = paymentMethod;
                                    SetModifiedProperties(payment);

                                    #endregion
                                }
                            }
                        }

                        #endregion

                        #region Invoice

                        if (invoice == null)
                        {
                            #region Add

                            if (paymentInput.InvoiceType == SoeInvoiceType.SupplierInvoice)
                            {
                                invoice = new SupplierInvoice();
                            }
                            else
                            {
                                invoice = new CustomerInvoice();
                                (invoice as CustomerInvoice).RegistrationType = (int)OrderInvoiceRegistrationType.Ledger;
                            }

                            invoice.Origin = origin;
                            invoice.ActorId = paymentInput.ActorId;
                            invoice.Type = (int)paymentInput.InvoiceType;
                            invoice.BillingType = (int)paymentInput.BillingType;
                            invoice.VatType = (int)TermGroup_InvoiceVatType.Merchandise;
                            invoice.SeqNr = null;
                            invoice.InvoiceNr = paymentInput.InvoiceNr;
                            invoice.InvoiceDate = paymentInput.InvoiceDate;
                            invoice.DueDate = paymentInput.PaymentDate;
                            invoice.VoucherDate = paymentInput.VoucherDate;
                            invoice.TotalAmount = (int)paymentInput.BillingType == (int)TermGroup_BillingType.Debit ? decimal.Negate(paymentInput.TotalAmount) : paymentInput.TotalAmount;
                            invoice.TotalAmountCurrency = (int)paymentInput.BillingType == (int)TermGroup_BillingType.Debit ? decimal.Negate(paymentInput.TotalAmount / paymentInput.CurrencyRate) : paymentInput.TotalAmount / paymentInput.CurrencyRate;
                            invoice.VATAmount = (int)paymentInput.BillingType == (int)TermGroup_BillingType.Debit ? decimal.Negate(paymentInput.VatAmount) : paymentInput.VatAmount;
                            invoice.PaidAmount = 0;
                            invoice.PaidAmountCurrency = 0;
                            invoice.RemainingAmount = 0;
                            invoice.CurrencyId = paymentInput.CurrencyId;
                            invoice.CurrencyRate = paymentInput.CurrencyRate;
                            invoice.CurrencyDate = paymentInput.CurrencyDate;
                            invoice.OnlyPayment = true;
                            invoice.SysPaymentTypeId = paymentInput.SysPaymentTypeId;
                            invoice.PaymentNr = paymentInput.PaymentNr;
                            invoice.FullyPayed = paymentInput.FullyPayed;

                            SetCreatedProperties(invoice);

                            #endregion
                        }
                        else
                        {
                            #region Update

                            if (adjustInvoiceAmounts)
                            {
                                invoice.PaidAmount += paymentInput.Amount;
                                invoice.PaidAmountCurrency += paymentInput.AmountCurrency;
                            }

                            invoice.SysPaymentTypeId = paymentInput.SysPaymentTypeId;
                            invoice.PaymentNr = paymentInput.PaymentNr;
                            invoice.FullyPayed = paymentInput.FullyPayed;

                            SetModifiedProperties(invoice);

                            #endregion
                        }

                        #endregion

                        #region PaymentImport

                        PaymentImport paymentImport = null;

                        if (import)
                        {
                            paymentImport = new PaymentImport
                            {
                                ImportDate = paymentInput.ImportDate.Value,
                                Filename = paymentInput.ImportFilename
                            };
                            SetCreatedProperties(paymentImport);
                            entities.PaymentImport.AddObject(paymentImport);
                        }

                        #endregion

                        #region PaymentRow

                        if (paymentInput.SeqNr == 0 && paymentInput.OriginStatus != SoeOriginStatus.Draft)
                            paymentInput.SeqNr = SequenceNumberManager.GetNextSequenceNumber(entities, actorCompanyId, Enum.GetName(typeof(SoeOriginType), (int)paymentInput.OriginType), 1, false);

                        // Get existing PaymentRow or create new
                        PaymentRow paymentRow = paymentInput.OriginId > 0 ? PaymentManager.GetPaymentRowWithAllReferences(entities, paymentInput.OriginId) : null;

                        if (paymentRow == null)
                        {
                            int status = 0;

                            if (paymentInput.PaymentStatus != null && paymentInput.PaymentStatus != SoePaymentStatus.None)
                            {
                                status = (int)paymentInput.PaymentStatus;
                            }
                            else
                            {
                                if (origin.Type == (int)SoeOriginType.SupplierPayment)
                                    status = (int)SoePaymentStatus.Pending;
                                else if (origin.Type == (int)SoeOriginType.CustomerPayment)
                                    status = (int)SoePaymentStatus.ManualPayment;
                            }

                            #region Add

                            if (import)
                            {
                                paymentRow = new PaymentRow()
                                {
                                    SeqNr = paymentInput.SeqNr,
                                    Status = status,
                                    HasPendingBankFee = paymentInput.HasPendingBankFee,
                                    HasPendingAmountDiff = paymentInput.HasPendingAmountDiff,
                                    VoucherHeadId = paymentInput.VoucherHeadId,

                                    //Set references
                                    Payment = payment,
                                };
                            }
                            else
                            {
                                paymentRow = new PaymentRow()
                                {
                                    Status = status,
                                    State = (int)paymentInput.State,
                                    SeqNr = paymentInput.SeqNr,
                                    SysPaymentTypeId = paymentInput.SysPaymentTypeId,
                                    PaymentNr = paymentInput.PaymentNr,
                                    PayDate = paymentInput.PaymentDate.Value,
                                    CurrencyRate = paymentInput.CurrencyRate,
                                    CurrencyDate = paymentInput.CurrencyDate,
                                    VoucherHeadId = paymentInput.VoucherHeadId,
                                    Amount = paymentInput.Amount,
                                    AmountCurrency = paymentInput.AmountCurrency,
                                    AmountDiff = paymentInput.AmountDiff,
                                    AmountDiffCurrency = paymentInput.AmountDiffCurrency,        // TODO: Calculate currency
                                    HasPendingAmountDiff = paymentInput.HasPendingAmountDiff,
                                    HasPendingBankFee = paymentInput.HasPendingBankFee,
                                    IsRestPayment = paymentInput.IsRestPayment,

                                    //Set references
                                    Payment = payment,
                                    Invoice = invoice,
                                };

                                paymentRows.Add(paymentRow);
                            }

                            if (autoAddCurrencyAmounts)
                                CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, paymentRow);

                            if (paymentImport != null)
                                paymentRow.PaymentImport = paymentImport;

                            SetCreatedProperties(paymentRow);

                            #region PaymentAccountRow

                            // Add new PaymentAccountRows
                            /*
                            if (paymentAccountRowsInput != null)
                            {
                                foreach (PaymentAccountRow paymentAccountRowToAdd in paymentAccountRowsInput)
                                {
                                    // Add PaymentAccountRow to PaymentRow
                                    paymentRow.PaymentAccountRow.Add(paymentAccountRowToAdd);
                                }
                            }
                            */
                            if (autoAddVoucher)
                            {
                                if (paymentInput.InvoiceType == SoeInvoiceType.CustomerInvoice)
                                {
                                    var customerInvoice = InvoiceManager.GetCustomerInvoice(entities, invoice.InvoiceId, true, false, false, false);
                                    AddPaymentAccountRowsFromCustomerInvoice(entities, paymentRow, paymentMethod, customerInvoice, actorCompanyId);
                                }
                                else
                                {
                                    var supplierInvoice = SupplierInvoiceManager.GetSupplierInvoice(entities, invoice.InvoiceId, true, true, true, false, true, true, true, false);
                                    AddPaymentAccountRowsFromSupplierInvoice(entities, paymentRow, supplierInvoice, paymentMethod, actorCompanyId);
                                }
                            }

                            #endregion

                            entities.PaymentRow.AddObject(paymentRow);

                            #endregion
                        }
                        else
                        {
                            #region Update

                            if (paymentInput.IsSuperSupportSave)
                            {
                                paymentRow.State = (int)paymentInput.State;
                                paymentRow.SeqNr = paymentInput.SeqNr;
                                paymentRow.SysPaymentTypeId = paymentInput.SysPaymentTypeId;
                                paymentRow.PaymentNr = paymentInput.PaymentNr;
                                paymentRow.PayDate = paymentInput.PaymentDate.Value;
                            }

                            if (paymentInput.VoucherHeadId == null)
                            {
                                paymentRow.PayDate = paymentInput.PaymentDate.Value;

                            }
                            /*
                            foreach (PaymentAccountRow accRow in paymentRow.PaymentAccountRow)
                            {
                                //Get account row from input
                                PaymentAccountRow par = paymentAccountRowsInput.FirstOrDefault(p => p.PaymentAccountRowId == accRow.PaymentAccountRowId);

                                if (par != null)
                                {
                                    accRow.AccountId = par.AccountId;
                                    accRow.Amount = par.Amount;
                                    accRow.AmountCurrency = par.AmountCurrency;
                                    accRow.AmountEntCurrency = par.AmountEntCurrency;
                                    accRow.AmountLedgerCurrency = par.AmountLedgerCurrency;
                                    accRow.CreditRow = par.CreditRow;
                                    accRow.DebitRow = par.DebitRow;
                                    accRow.InterimRow = par.InterimRow;
                                    accRow.Quantity = par.Quantity;
                                    accRow.RowNr = par.RowNr;
                                    accRow.State = par.State;
                                    accRow.Text = par.Text;
                                    accRow.VatRow = par.VatRow;

                                    if (par.AccountInternal != null)
                                    {
                                        if (!accRow.AccountInternal.IsLoaded)
                                            accRow.AccountInternal.Load();

                                        accRow.AccountInternal.Clear();

                                        foreach (AccountInternal accInt in par.AccountInternal)
                                        {
                                            accRow.AccountInternal.Add(accInt);
                                        }
                                    }

                                    SetModifiedProperties(accRow);

                                    TryDetachEntity(entities, par);
                                }
                            }
                            */
                            SetModifiedProperties(paymentRow);

                            #endregion
                        }

                        #endregion
                    }

                    #region Voucher

                    // Check if Voucher should also should be saved at once (only manual registred CustomerPayments)
                    if (result.Success && isManualPayment)
                    {
                        int baseSysCurrencyId = CountryCurrencyManager.GetCompanyBaseSysCurrencyId(entities, actorCompanyId);
                        bool foreign = currency.SysCurrencyId != baseSysCurrencyId;

                        foreach (var paymentsByOrigin in paymentRows.GroupBy(x => x.Payment.Origin.Type))
                        {
                            var originType = paymentsByOrigin.First().Payment.Origin.Type;
                            if (autoAddVoucher)
                                result = TransferPaymentRowsToVoucher(entities, ConfigSettings.TRANSACTIONSCOPEOPTION_ATTACH, paymentsByOrigin.ToList(), (SoeOriginType)originType, foreign, paymentGroup.Key.AccountYearId, actorCompanyId);
                            else
                                result = TryTransferPaymentRowsToVoucher(entities, ConfigSettings.TRANSACTIONSCOPEOPTION_ATTACH, paymentsByOrigin.ToList(), (SoeOriginType)originType, foreign, paymentGroup.Key.AccountYearId, actorCompanyId);

                            if (result.Success)
                                voucherHeadsDict = NumberUtility.MergeDictictionary(voucherHeadsDict, result.IdDict);
                        }
                    }

                    #endregion

                    if (result.Success)
                    {
                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                        {
                            //paymentRowIds.Add(origin.OriginId);
                        }
                    }
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
                if (result.Success)
                {
                    //Set success properties
                    //result.IntegerValue = paymentRowId;
                    //result.Value = paymentRowInput.SeqNr;
                    result.Keys = paymentRowIds;
                    if (voucherHeadsDict != null)
                    {
                        result.IdDict = voucherHeadsDict;
                    }
                }
                else
                    base.LogTransactionFailed(this.ToString(), this.log);
            }
            return result;

        }
        public ActionResult SavePaymentRow(CompEntities entities, TransactionScope transaction, PaymentRowSaveDTO paymentRowInput, List<AccountingRowDTO> accountingRowDTOsInput, int actorCompanyId, bool isManualPayment, bool import, bool adjustInvoiceAmounts = true, bool autoAddVoucher = false, bool autoAddCurrencyAmounts = false)
        {

            if (paymentRowInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "PaymentRowSaveDTO");

            // Default result is successful
            ActionResult result = new ActionResult();

            int paymentId = paymentRowInput.OriginId;
            int paymentRowId = 0;
            Dictionary<int, int> voucherHeadsDict = null;

            try
            {
                #region Convert

                List<PaymentAccountRow> paymentAccountRowsInput = null;
                if (accountingRowDTOsInput != null) //not used by export functions, (instead of overloading)
                {
                    // Convert collection of AccountingRowDTOs to collection of PaymentAccountRows
                    paymentAccountRowsInput = ConvertToPaymentAccountRows(entities, paymentRowInput, accountingRowDTOsInput, paymentRowInput.ActorId, actorCompanyId);
                }

                #endregion

                #region Prereq

                // Get owner Company for the Invoice
                Company company = CompanyManager.GetCompany(entities, actorCompanyId, false);
                if (company == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                // Get currency
                Currency currency = CountryCurrencyManager.GetCurrency(entities, paymentRowInput.CurrencyId);
                if (currency == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "Currency");

                // Get payment method
                PaymentMethod paymentMethod = PaymentManager.GetPaymentMethod(entities, paymentRowInput.PaymentMethodId, actorCompanyId, autoAddVoucher);
                if (paymentMethod == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, $"PaymentMethod is missing ( {paymentRowInput.PaymentMethodId} )");

                if (paymentMethod.PaymentInformationRow == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(5010, "Betalningsmetod \"{0}\" saknar betalningskonto").Replace("{0}", paymentMethod.Name));

                if (paymentRowInput.SysPaymentTypeId == 0 || string.IsNullOrEmpty(paymentRowInput.PaymentNr))
                {
                    if (!paymentMethod.PaymentInformationRow.PaymentInformationReference.IsLoaded)
                        paymentMethod.PaymentInformationRowReference.Load();

                    // Get payment type and number from PaymentInformationRow
                    paymentRowInput.SysPaymentTypeId = paymentMethod.PaymentInformationRow.SysPaymentTypeId;
                    paymentRowInput.PaymentNr = paymentMethod.PaymentInformationRow.PaymentNr;
                    if (string.IsNullOrEmpty(paymentRowInput.PaymentNr))
                        paymentRowInput.PaymentNr = "-";
                }

                #endregion

                #region Origin

                // Get existing origin or create new
                Origin origin = paymentRowInput.OriginId > 0 ? OriginManager.GetOrigin(entities, paymentRowInput.OriginId) : null;
                if (origin == null)
                {
                    #region Add

                    origin = new Origin()
                    {
                        Type = (int)paymentRowInput.OriginType,
                        Status = (int)paymentRowInput.OriginStatus,
                        Description = paymentRowInput.OriginDescription,

                        //Set FK
                        VoucherSeriesId = paymentRowInput.VoucherSeriesId,
                        VoucherSeriesTypeId = paymentRowInput.VoucherSeriesTypeId,

                        //Set references
                        Company = company,
                    };
                    SetCreatedProperties(origin);
                    entities.Origin.AddObject(origin);

                    #endregion
                }

                #endregion

                #region Payment

                // Get existing Payment or create new
                Payment payment = null;
                if (import)
                {
                    #region Import

                    if (paymentRowInput.OriginId > 0)
                    {
                        payment = PaymentManager.GetPayment(entities, paymentRowInput.OriginId, true, false);
                    }
                    else
                    {
                        payment = new Payment()
                        {
                            PaymentId = origin.OriginId,

                            //Set references
                            Origin = origin,
                            PaymentMethod = paymentMethod,
                        };
                        SetCreatedProperties(payment);
                        entities.Payment.AddObject(payment);
                    }

                    #endregion
                }
                else
                {
                    if (paymentRowInput.OriginId == 0)
                    {

                        if (payment == null)
                        {
                            #region Add

                            payment = new Payment()
                            {
                                PaymentId = origin.OriginId,
                                Origin = origin,
                                PaymentMethod = paymentMethod,
                            };
                            SetCreatedProperties(payment);
                            entities.Payment.AddObject(payment);

                            #endregion
                        }
                        else
                        {
                            #region Update

                            payment.PaymentMethod = paymentMethod;
                            SetModifiedProperties(payment);

                            #endregion
                        }
                    }
                }

                #endregion

                #region Invoice

                // Get existing Invoice or create new
                Invoice invoice = paymentRowInput.InvoiceId > 0 ? InvoiceManager.GetInvoice(entities, paymentRowInput.InvoiceId) : null;
                if (invoice == null)
                {
                    #region Add

                    if (paymentRowInput.InvoiceType == SoeInvoiceType.SupplierInvoice)
                    {
                        invoice = new SupplierInvoice();
                    }
                    else
                    {
                        invoice = new CustomerInvoice();
                        (invoice as CustomerInvoice).RegistrationType = (int)OrderInvoiceRegistrationType.Ledger;
                    }

                    invoice.Origin = origin;
                    invoice.ActorId = paymentRowInput.ActorId;
                    invoice.Type = (int)paymentRowInput.InvoiceType;
                    invoice.BillingType = (int)paymentRowInput.BillingType;
                    invoice.VatType = (int)TermGroup_InvoiceVatType.Merchandise;
                    invoice.SeqNr = null;
                    invoice.InvoiceNr = paymentRowInput.InvoiceNr;
                    invoice.InvoiceDate = paymentRowInput.InvoiceDate;
                    invoice.DueDate = paymentRowInput.PaymentDate;
                    invoice.VoucherDate = paymentRowInput.VoucherDate;
                    invoice.TotalAmount = (int)paymentRowInput.BillingType == (int)TermGroup_BillingType.Debit ? Decimal.Negate(paymentRowInput.TotalAmount) : paymentRowInput.TotalAmount;
                    invoice.TotalAmountCurrency = (int)paymentRowInput.BillingType == (int)TermGroup_BillingType.Debit ? Decimal.Negate(paymentRowInput.TotalAmount / paymentRowInput.CurrencyRate) : paymentRowInput.TotalAmount / paymentRowInput.CurrencyRate;
                    invoice.VATAmount = (int)paymentRowInput.BillingType == (int)TermGroup_BillingType.Debit ? Decimal.Negate(paymentRowInput.VatAmount) : paymentRowInput.VatAmount;
                    invoice.PaidAmount = 0;
                    invoice.PaidAmountCurrency = 0;
                    invoice.RemainingAmount = 0;
                    invoice.CurrencyId = paymentRowInput.CurrencyId;
                    invoice.CurrencyRate = paymentRowInput.CurrencyRate;
                    invoice.CurrencyDate = paymentRowInput.CurrencyDate;
                    invoice.OnlyPayment = true;
                    invoice.SysPaymentTypeId = paymentRowInput.SysPaymentTypeId;
                    invoice.PaymentNr = paymentRowInput.PaymentNr;
                    invoice.FullyPayed = paymentRowInput.FullyPayed;

                    SetCreatedProperties(invoice);

                    #endregion
                }
                else
                {
                    #region Update

                    if (adjustInvoiceAmounts)
                    {
                        invoice.PaidAmount += paymentRowInput.Amount;
                        invoice.PaidAmountCurrency += paymentRowInput.AmountCurrency;
                    }

                    invoice.SysPaymentTypeId = paymentRowInput.SysPaymentTypeId;
                    invoice.PaymentNr = paymentRowInput.PaymentNr;
                    invoice.FullyPayed = paymentRowInput.FullyPayed;

                    SetModifiedProperties(invoice);

                    #endregion
                }

                #endregion

                #region PaymentImport

                PaymentImport paymentImport = null;

                if (import)
                {
                    paymentImport = new PaymentImport
                    {
                        ImportDate = paymentRowInput.ImportDate.Value,
                        Filename = paymentRowInput.ImportFilename
                    };
                    SetCreatedProperties(paymentImport);
                    entities.PaymentImport.AddObject(paymentImport);
                }

                #endregion

                #region PaymentRow

                if (paymentRowInput.SeqNr == 0 && paymentRowInput.OriginStatus != SoeOriginStatus.Draft)
                    paymentRowInput.SeqNr = SequenceNumberManager.GetNextSequenceNumber(entities, actorCompanyId, Enum.GetName(typeof(SoeOriginType), (int)paymentRowInput.OriginType), 1, false);

                // Get existing PaymentRow or create new
                PaymentRow paymentRow = paymentRowInput.OriginId > 0 ? PaymentManager.GetPaymentRowWithAllReferences(entities, paymentRowInput.OriginId) : null;

                if (paymentRow == null)
                {
                    int status = 0;

                    if (paymentRowInput.PaymentStatus != null && paymentRowInput.PaymentStatus != SoePaymentStatus.None)
                    {
                        status = (int)paymentRowInput.PaymentStatus;
                    }
                    else
                    {
                        if (origin.Type == (int)SoeOriginType.SupplierPayment)
                            status = (int)SoePaymentStatus.Pending;
                        else if (origin.Type == (int)SoeOriginType.CustomerPayment)
                            status = (int)SoePaymentStatus.ManualPayment;
                    }

                    #region Add

                    if (import)
                    {
                        paymentRow = new PaymentRow()
                        {
                            SeqNr = paymentRowInput.SeqNr,
                            Status = status,
                            HasPendingBankFee = paymentRowInput.HasPendingBankFee,
                            HasPendingAmountDiff = paymentRowInput.HasPendingAmountDiff,
                            VoucherHeadId = paymentRowInput.VoucherHeadId,
                            Text = paymentRowInput.Text,

                            //Set references
                            Payment = payment,
                        };
                    }
                    else
                    {
                        paymentRow = new PaymentRow()
                        {
                            Status = status,
                            State = (int)paymentRowInput.State,
                            SeqNr = paymentRowInput.SeqNr,
                            SysPaymentTypeId = paymentRowInput.SysPaymentTypeId,
                            PaymentNr = paymentRowInput.PaymentNr,
                            PayDate = paymentRowInput.PaymentDate.Value,
                            CurrencyRate = paymentRowInput.CurrencyRate,
                            CurrencyDate = paymentRowInput.CurrencyDate,
                            VoucherHeadId = paymentRowInput.VoucherHeadId,
                            Amount = paymentRowInput.Amount,
                            AmountCurrency = paymentRowInput.AmountCurrency,
                            AmountDiff = paymentRowInput.AmountDiff,
                            AmountDiffCurrency = paymentRowInput.AmountDiffCurrency,        // TODO: Calculate currency
                            HasPendingAmountDiff = paymentRowInput.HasPendingAmountDiff,
                            HasPendingBankFee = paymentRowInput.HasPendingBankFee,
                            IsRestPayment = paymentRowInput.IsRestPayment,
                            Text = paymentRowInput.Text,

                            //Set references
                            Payment = payment,
                            Invoice = invoice,
                        };
                    }

                    if (autoAddCurrencyAmounts)
                        CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, paymentRow);

                    if (paymentImport != null)
                        paymentRow.PaymentImport = paymentImport;

                    SetCreatedProperties(paymentRow);

                    #region PaymentAccountRow

                    // Add new PaymentAccountRows
                    if (paymentAccountRowsInput != null)
                    {
                        foreach (PaymentAccountRow paymentAccountRowToAdd in paymentAccountRowsInput)
                        {
                            // Add PaymentAccountRow to PaymentRow
                            paymentRow.PaymentAccountRow.Add(paymentAccountRowToAdd);
                        }
                    }
                    else if (autoAddVoucher)
                    {
                        if (paymentRowInput.InvoiceType == SoeInvoiceType.CustomerInvoice)
                        {
                            var customerInvoice = InvoiceManager.GetCustomerInvoice(entities, invoice.InvoiceId, true, false, false, false);
                            AddPaymentAccountRowsFromCustomerInvoice(entities, paymentRow, paymentMethod, customerInvoice, actorCompanyId);
                        }
                        else
                        {
                            var supplierInvoice = SupplierInvoiceManager.GetSupplierInvoice(entities, invoice.InvoiceId, true, true, true, false, true, true, true, false);
                            AddPaymentAccountRowsFromSupplierInvoice(entities, paymentRow, supplierInvoice, paymentMethod, actorCompanyId);
                        }
                    }

                    #endregion

                    entities.PaymentRow.AddObject(paymentRow);

                    #endregion
                }
                else
                {
                    #region Update


                    paymentRow.Text = paymentRowInput.Text;

                    if (paymentRowInput.IsSuperSupportSave)
                    {
                        paymentRow.State = (int)paymentRowInput.State;
                        paymentRow.SeqNr = paymentRowInput.SeqNr;
                        paymentRow.SysPaymentTypeId = paymentRowInput.SysPaymentTypeId;
                        paymentRow.PaymentNr = paymentRowInput.PaymentNr;
                        paymentRow.PayDate = paymentRowInput.PaymentDate.Value;
                    }

                    if (paymentRowInput.VoucherHeadId == null)
                    {
                        paymentRow.PayDate = paymentRowInput.PaymentDate.Value;

                    }

                    foreach (PaymentAccountRow accRow in paymentRow.PaymentAccountRow)
                    {
                        //Get account row from input
                        PaymentAccountRow par = paymentAccountRowsInput.FirstOrDefault(p => p.PaymentAccountRowId == accRow.PaymentAccountRowId);

                        if (par != null)
                        {
                            accRow.AccountId = par.AccountId;
                            accRow.Amount = par.Amount;
                            accRow.AmountCurrency = par.AmountCurrency;
                            accRow.AmountEntCurrency = par.AmountEntCurrency;
                            accRow.AmountLedgerCurrency = par.AmountLedgerCurrency;
                            accRow.CreditRow = par.CreditRow;
                            accRow.DebitRow = par.DebitRow;
                            accRow.InterimRow = par.InterimRow;
                            accRow.Quantity = par.Quantity;
                            accRow.RowNr = par.RowNr;
                            accRow.State = par.State;
                            accRow.Text = par.Text;
                            accRow.VatRow = par.VatRow;

                            if (par.AccountInternal != null)
                            {
                                if (!accRow.AccountInternal.IsLoaded)
                                    accRow.AccountInternal.Load();

                                accRow.AccountInternal.Clear();

                                foreach (AccountInternal accInt in par.AccountInternal)
                                {
                                    accRow.AccountInternal.Add(accInt);
                                }
                            }

                            SetModifiedProperties(accRow);

                            TryDetachEntity(entities, par);
                        }
                    }

                    // For now only uptades to accounting rows will be done
                    /*foreach(PaymentAccountRow par in paymentAccountRowsInput)
                    {
                        PaymentAccountRow accRow = paymentRow.PaymentAccountRow.Where(p => p.PaymentAccountRowId == par.PaymentAccountRowId).FirstOrDefault();
                        if(accRow != null)
                        {
                            accRow.AccountId = par.AccountId;
                            accRow.Amount = par.Amount;
                            accRow.AmountCurrency = par.AmountCurrency;
                            accRow.AmountEntCurrency = par.AmountEntCurrency;
                            accRow.AmountLedgerCurrency = par.AmountLedgerCurrency;
                            accRow.CreditRow = par.CreditRow;
                            accRow.DebitRow = par.DebitRow;
                            accRow.InterimRow = par.InterimRow;
                            accRow.Quantity = par.Quantity;
                            accRow.RowNr = par.RowNr;
                            accRow.State = par.State;
                            accRow.Text = par.Text;
                            accRow.VatRow = par.VatRow;

                            if(par.AccountInternal != null)
                            {
                                if (!accRow.AccountInternal.IsLoaded)
                                    accRow.AccountInternal.Load();

                                accRow.AccountInternal.Clear();

                                foreach (AccountInternal accInt in par.AccountInternal)
                                {
                                    accRow.AccountInternal.Add(accInt);
                                }
                            }

                            SetModifiedProperties(accRow);
                        }
                        else
                        {
                            paymentRow.PaymentAccountRow.Add(par);
                        }
                    }*/

                    SetModifiedProperties(paymentRow);

                    #endregion
                }

                #endregion

                #region Voucher

                // Check if Voucher should also should be saved at once (only manual registred CustomerPayments)
                if (result.Success && isManualPayment)
                {
                    int baseSysCurrencyId = CountryCurrencyManager.GetCompanyBaseSysCurrencyId(entities, actorCompanyId);
                    bool foreign = currency.SysCurrencyId != baseSysCurrencyId;

                    if (autoAddVoucher)
                        result = TransferPaymentRowsToVoucher(entities, ConfigSettings.TRANSACTIONSCOPEOPTION_ATTACH, new List<PaymentRow> { paymentRow }, (SoeOriginType)origin.Type, foreign, paymentRowInput.AccountYearId, actorCompanyId);
                    else
                        result = TryTransferPaymentRowToVoucher(entities, ConfigSettings.TRANSACTIONSCOPEOPTION_ATTACH, paymentRow, (SoeOriginType)origin.Type, foreign, paymentRowInput.AccountYearId, actorCompanyId);

                    if (result.Success)
                        voucherHeadsDict = NumberUtility.MergeDictictionary(voucherHeadsDict, result.IdDict);
                }

                #endregion

                if (result.Success)
                {
                    result = SaveChanges(entities, transaction);
                    if (result.Success)
                    {
                        paymentId = origin.OriginId;
                        paymentRowId = paymentRow.PaymentRowId;
                    }
                }
            }
            catch (SoeEntityNotFoundException ex)
            {
                result.Success = false;
                result.ErrorMessage = string.Format(GetText(1146), ex.InnerException.Message);
                result.ErrorNumber = (int)ActionResultSave.EntityNotFound;
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result.Exception = ex;
                result.IntegerValue = 0;
                paymentRowInput.SeqNr = 0;
            }
            finally
            {
                if (result.Success)
                {
                    //Set success properties
                    result.IntegerValue = paymentId;
                    result.Value = paymentRowInput.SeqNr;
                    result.IntegerValue2 = paymentRowId;

                    if (voucherHeadsDict != null)
                    {
                        result.IdDict = voucherHeadsDict;
                    }
                }
                else
                    base.LogTransactionFailed(this.ToString(), this.log);
            }

            return result;
        }

        public ActionResult TransferPaymentRowsToVoucher(List<CustomerInvoiceGridDTO> items, SoeOriginType originType, bool foreign, int actorCompanyId)
        {

            if (items == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "ChangeStatusGridView");

            var result = new ActionResult(true);

            using (var entities = new CompEntities())
            {
                var paymentRowsPerAccountYear = new Dictionary<int, List<PaymentRow>>();

                foreach (var item in items.OrderBy(x => x.PayDate))
                {

                    #region Get PaymentRow

                    //Get Paymentrow from database
                    PaymentRow paymentRow = (from pr in entities.PaymentRow
                                                 .Include("Invoice.Actor.Supplier")
                                                 .Include("Payment.Origin")
                                                 .Include("PaymentAccountRow.AccountStd.Account")
                                                 .Include("PaymentAccountRow.AccountInternal.Account")
                                             where pr.Payment.PaymentId == item.CustomerPaymentId &&
                                             pr.PaymentRowId == item.CustomerPaymentRowId &&
                                             pr.InvoiceId.HasValue &&
                                             pr.InvoiceId.Value == item.CustomerInvoiceId &&
                                             (pr.Status == (int)SoePaymentStatus.Verified || pr.Status == (int)SoePaymentStatus.Pending || pr.Status == (int)SoePaymentStatus.ManualPayment) &&
                                             !pr.IsSuggestion
                                             select pr).FirstOrDefault();

                    if (paymentRow == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "PaymentRow");
                    if (paymentRow.Payment == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "Payment");
                    if (paymentRow.Payment.Origin == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "Origin");
                    if (paymentRow.Invoice == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "Invoice");

                    #endregion

                    #region AccountYear/PayDate

                    AccountYear accountYear = null;
                    //Validate handles null but adding null check so that sonarqube is appy...
                    if (accountYear == null || !AccountManager.ValidateAccountYear(accountYear, paymentRow.PayDate).Success)
                    {
                        accountYear = AccountManager.GetAccountYear(entities, paymentRow.PayDate, actorCompanyId);
                        if (accountYear == null) return new ActionResult((GetText(8404, "Redovisningsår saknas")));
                    }

                    if (item.PayDate.HasValue && item.PayDate.Value != paymentRow.PayDate)
                    {
                        var newPaydate = item.PayDate.Value;

                        // Get accountyear
                        var newAccountYear = AccountManager.GetAccountYear(entities, newPaydate, actorCompanyId);

                        //Validate AccountYear
                        result = AccountManager.ValidateAccountYear(newAccountYear, newPaydate);
                        if (!result.Success)
                            return result;

                        //Validate AccountPeriod
                        AccountPeriod accountPeriod = AccountManager.GetAccountPeriod(entities, newPaydate, actorCompanyId);
                        result = AccountManager.ValidateAccountPeriod(accountPeriod, newPaydate);
                        if (!result.Success)
                            return result;

                        if (newAccountYear.AccountYearId != accountYear.AccountYearId)
                        {
                            if (!paymentRow.PaymentReference.IsLoaded)
                                paymentRow.PaymentReference.Load();

                            if (paymentRow.Payment != null)
                            {
                                int customerPaymentSeriesTypeId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.CustomerPaymentVoucherSeriesType, 0, actorCompanyId, 0);

                                if (!paymentRow.Payment.OriginReference.IsLoaded)
                                    paymentRow.Payment.OriginReference.Load();

                                VoucherSeries voucherSerie = VoucherManager.GetVoucherSerieByType(entities, customerPaymentSeriesTypeId, newAccountYear.AccountYearId);
                                if (voucherSerie == null)
                                {
                                    string errorMessage = $"Voucher series error \nTransferPaymentRowsToVoucher failed \ncustomerPaymentSeriesTypeId: {customerPaymentSeriesTypeId} \nAccountYearId: {newAccountYear.AccountYearId} \nActorCompanyId: {actorCompanyId}";
                                    base.LogError(errorMessage);
                                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8403, "Verifikatserie saknas"));
                                }

                                paymentRow.Payment.Origin.VoucherSeriesId = voucherSerie.VoucherSeriesId;

                                SetModifiedProperties(paymentRow.Payment.Origin);
                            }
                        }

                        paymentRow.PayDate = newPaydate;
                        accountYear = newAccountYear;

                        SetModifiedProperties(paymentRow);
                    }

                    #endregion

                    #region BankFee and DiffAmount

                    decimal remainingAmount = paymentRow.Amount;
                    decimal bankFeeAmount = originType == SoeOriginType.SupplierPayment ? item.BankFee : 0;
                    decimal paidAmount = item.PaymentAmount - bankFeeAmount;

                    //Set BankFee
                    if (bankFeeAmount > 0)
                    {
                        paymentRow.BankFee = bankFeeAmount;
                        paymentRow.HasPendingBankFee = true;
                    }

                    //Set AmountDiff (not handled manually, generate default PaymentAccountRow's)
                    decimal amountDiff = paidAmount - remainingAmount;
                    if (amountDiff != 0)
                    {
                        //Always create diff (take no respect to FullyPayed)
                        paymentRow.AmountDiff = decimal.Add(paymentRow.AmountDiff, amountDiff);
                        paymentRow.HasPendingAmountDiff = true;
                    }

                    result = PaymentManager.HandlePendingDiffAndBankFee(entities, paymentRow, originType, item.PaymentAmount, foreign, actorCompanyId);
                    if (!result.Success)
                        return result;

                    #endregion

                    List<PaymentRow> paymentRows;
                    if (!paymentRowsPerAccountYear.TryGetValue(accountYear.AccountYearId, out paymentRows))
                    {
                        paymentRows = new List<PaymentRow>();
                        paymentRowsPerAccountYear.Add(accountYear.AccountYearId, paymentRows);
                    }
                    paymentRows.Add(paymentRow);
                }

                foreach (var payments in paymentRowsPerAccountYear)
                {
                    result = VoucherManager.SaveVoucherFromPayment(entities, ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, payments.Value, originType, foreign, payments.Key, actorCompanyId);
                    if (!result.Success)
                        return result;
                }

                return result;
            }
        }

        public ActionResult TransferPaymentRowsToVoucher(List<SupplierPaymentGridDTO> items, SoeOriginType originType, int accountYearId, int actorCompanyId, bool useTodaysDate = false, bool continueOnValidationError = false)
        {
            if (items == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "ChangeStatusGridView");
            try
            {
                using (CompEntities entities = new CompEntities())
                {
                    return TransferPaymentRowsToVoucher(entities, items, originType, accountYearId, actorCompanyId, useTodaysDate, continueOnValidationError);
                }
            }
            catch (Exception ex)
            {
                return new ActionResult(ex);
            }
        }

        public ActionResult TransferPaymentRowsToVoucher(CompEntities entities, List<SupplierPaymentGridDTO> items, SoeOriginType originType, int accountYearId, int actorCompanyId, bool useTodaysDate = false, bool continueOnValidationError = false)
        {
            var result = new ActionResult(true);

            //Get currency
            int baseSysCurrencyId = CountryCurrencyManager.GetCompanyBaseSysCurrencyId(entities, base.ActorCompanyId);

            List<PaymentRow> paymentRows = new List<PaymentRow>();
            List<string> failedPayments = new List<string>();
            foreach (var item in items)
            {

                #region Get PaymentRow

                //Get Paymentrow from database
                PaymentRow paymentRow = (from pr in entities.PaymentRow
                                                .Include("Invoice.Actor.Supplier")
                                                .Include("Payment.Origin")
                                                .Include("PaymentAccountRow.AccountStd.Account")
                                                .Include("PaymentAccountRow.AccountInternal.Account")
                                         where (item.PaymentRowId > 0 ? pr.PaymentRowId == item.PaymentRowId : pr.Payment.PaymentId == item.SupplierPaymentId) &&
                                         pr.InvoiceId.HasValue &&
                                         pr.InvoiceId.Value == item.SupplierInvoiceId &&
                                         (pr.Status == (int)SoePaymentStatus.Verified || pr.Status == (int)SoePaymentStatus.Pending || pr.Status == (int)SoePaymentStatus.ManualPayment) &&
                                         !pr.IsSuggestion
                                         select pr).FirstOrDefault<PaymentRow>();

                if (paymentRow == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "PaymentRow");
                if (paymentRow.Payment == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "Payment");
                if (paymentRow.Payment.Origin == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "Origin");
                if (paymentRow.Invoice == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "Invoice");

                #endregion

                #region BankFee and DiffAmount

                decimal remainingAmount = paymentRow.Amount;
                decimal bankFeeAmount = originType == SoeOriginType.SupplierPayment ? item.BankFee : 0;
                decimal paidAmount = item.PaymentAmount - bankFeeAmount;

                //Set BankFee
                if (bankFeeAmount > 0)
                {
                    paymentRow.BankFee = bankFeeAmount;
                    paymentRow.HasPendingBankFee = true;
                }

                //Set AmountDiff (not handled manually, generate default PaymentAccountRow's)
                decimal amountDiff = paidAmount - remainingAmount;
                if (amountDiff != 0)
                {
                    //Always create diff (take no respect to FullyPayed)
                    paymentRow.AmountDiff = Decimal.Add(paymentRow.AmountDiff, amountDiff);
                    paymentRow.HasPendingAmountDiff = true;
                }

                result = PaymentManager.HandlePendingDiffAndBankFee(entities, paymentRow, originType, item.PaymentAmount, item.SysCurrencyId != baseSysCurrencyId, actorCompanyId);
                if (!result.Success)
                    return result;

                #endregion

                if (continueOnValidationError)
                {
                    if (!paymentRow.ActivePaymentAccountRows.Any() || paymentRow.ActivePaymentAccountRows.Sum(x => Math.Abs(x.Amount)) == 0)
                    {
                        failedPayments.Add(paymentRow.SeqNr.ToString());
                    }
                    else
                    {
                        paymentRows.Add(paymentRow);
                    }
                }
                else
                {
                    paymentRows.Add(paymentRow);
                }
            }

            result = VoucherManager.SaveVoucherFromPayment(entities, ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, paymentRows, originType, false, accountYearId, actorCompanyId, date: useTodaysDate ? DateTime.Today : (DateTime?)null);

            if (continueOnValidationError)
                result.Strings = failedPayments;

            return result;
        }

        public ActionResult TransferPaymentRowsToVoucherUsePaymentDate(List<SupplierPaymentGridDTO> items, SoeOriginType originType, int accountYearId, int actorCompanyId, bool continueOnValidationError = false, List<SysHoliday> holidays = null)
        {
            if (items == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "ChangeStatusGridView");
            try
            {
                using (CompEntities entities = new CompEntities())
                {
                    return TransferPaymentRowsToVoucherUsePaymentDate(entities, items, originType, accountYearId, actorCompanyId, continueOnValidationError, holidays);
                }
            }
            catch (Exception ex)
            {
                return new ActionResult(ex);
            }
        }

        public ActionResult TransferPaymentRowsToVoucherUsePaymentDate(CompEntities entities, List<SupplierPaymentGridDTO> items, SoeOriginType originType, int accountYearId, int actorCompanyId, bool continueOnValidationError = false, List<SysHoliday> holidays = null)
        {
            var result = new ActionResult(true);

            //Get currency
            int baseSysCurrencyId = CountryCurrencyManager.GetCompanyBaseSysCurrencyId(entities, base.ActorCompanyId);

            List<PaymentRow> paymentRows = new List<PaymentRow>();
            List<string> failedPayments = new List<string>();
            foreach (var item in items)
            {

                #region Get PaymentRow

                //Get Paymentrow from database
                PaymentRow paymentRow = (from pr in entities.PaymentRow
                                                .Include("Invoice.Actor.Supplier")
                                                .Include("Payment.Origin")
                                                .Include("PaymentAccountRow.AccountStd.Account")
                                                .Include("PaymentAccountRow.AccountInternal.Account")
                                         where pr.Payment.PaymentId == item.SupplierPaymentId &&
                                         pr.InvoiceId.HasValue &&
                                         pr.InvoiceId.Value == item.SupplierInvoiceId &&
                                         (pr.Status == (int)SoePaymentStatus.Verified || pr.Status == (int)SoePaymentStatus.Pending || pr.Status == (int)SoePaymentStatus.ManualPayment) &&
                                         !pr.IsSuggestion
                                         select pr).FirstOrDefault<PaymentRow>();

                if (paymentRow == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "PaymentRow");
                if (paymentRow.Payment == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "Payment");
                if (paymentRow.Payment.Origin == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "Origin");
                if (paymentRow.Invoice == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "Invoice");

                #endregion

                #region BankFee and DiffAmount

                decimal remainingAmount = paymentRow.Amount;
                decimal bankFeeAmount = originType == SoeOriginType.SupplierPayment ? item.BankFee : 0;
                decimal paidAmount = item.PaymentAmount - bankFeeAmount;

                //Set BankFee
                if (bankFeeAmount > 0)
                {
                    paymentRow.BankFee = bankFeeAmount;
                    paymentRow.HasPendingBankFee = true;
                }

                //Set AmountDiff (not handled manually, generate default PaymentAccountRow's)
                decimal amountDiff = paidAmount - remainingAmount;
                if (amountDiff != 0)
                {
                    //Always create diff (take no respect to FullyPayed)
                    paymentRow.AmountDiff = Decimal.Add(paymentRow.AmountDiff, amountDiff);
                    paymentRow.HasPendingAmountDiff = true;
                }

                result = PaymentManager.HandlePendingDiffAndBankFee(entities, paymentRow, originType, item.PaymentAmount, item.SysCurrencyId != baseSysCurrencyId, actorCompanyId);
                if (!result.Success)
                    return result;

                #endregion

                if (continueOnValidationError)
                {
                    if (!paymentRow.ActivePaymentAccountRows.Any() || paymentRow.ActivePaymentAccountRows.Sum(x => Math.Abs(x.Amount)) == 0)
                    {
                        failedPayments.Add(paymentRow.SeqNr.ToString());
                    }
                    else
                    {
                        paymentRows.Add(paymentRow);
                    }
                }
                else
                {
                    paymentRows.Add(paymentRow);
                }
            }

            result = VoucherManager.SaveVoucherFromPayment(entities, ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, paymentRows, originType, false, accountYearId, actorCompanyId, usePayDateAndValidate: true, holidays: holidays);

            if (continueOnValidationError)
                result.Strings = failedPayments;

            return result;
        }

        public ActionResult TryTransferPaymentRowToVoucher(CompEntities entities, TransactionScopeOption transactionScopeOption, PaymentRow paymentRow, SoeOriginType originType, bool foreign, int accountYearId, int actorCompanyId)
        {
            if (paymentRow == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "PaymentRow");

            return TryTransferPaymentRowsToVoucher(entities, transactionScopeOption, new List<PaymentRow> { paymentRow }, originType, foreign, accountYearId, actorCompanyId);
        }

        public ActionResult TryTransferPaymentRowsToVoucher(CompEntities entities, TransactionScopeOption transactionScopeOption, List<PaymentRow> paymentRows, SoeOriginType originType, bool foreign, int accountYearId, int actorCompanyId)
        {
            bool transferToVoucher = false;
            if (originType == SoeOriginType.CustomerPayment)
                transferToVoucher = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.CustomerPaymentManualTransferToVoucher, 0, actorCompanyId, 0);
            else if (originType == SoeOriginType.SupplierPayment)
                transferToVoucher = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierPaymentManualTransferToVoucher, 0, actorCompanyId, 0);

            if (!transferToVoucher)
                return new ActionResult(true);

            return TransferPaymentRowsToVoucher(entities, transactionScopeOption, paymentRows, originType, foreign, accountYearId, actorCompanyId);
        }

        public ActionResult TransferPaymentRowsToVoucher(CompEntities entities, TransactionScopeOption transactionScopeOption, List<PaymentRow> paymentRows, SoeOriginType originType, bool foreign, int accountYearId, int actorCompanyId)
        {
            List<PaymentRow> paymentRowsToTransfer = new List<PaymentRow>();

            foreach (PaymentRow paymentRow in paymentRows)
            {
                //Make sure VoucherHead is loaded
                if (!paymentRow.VoucherHeadReference.IsLoaded)
                    paymentRow.VoucherHeadReference.Load();

                //Check if already transferred
                if (paymentRow.VoucherHead != null)
                    continue;

                paymentRowsToTransfer.Add(paymentRow);
            }

            if (paymentRowsToTransfer.Count > 0)
                return VoucherManager.SaveVoucherFromPayment(entities, transactionScopeOption, paymentRowsToTransfer, originType, foreign, accountYearId, actorCompanyId);
            else
                return new ActionResult(true);
        }

        private List<PaymentAccountRow> ConvertToPaymentAccountRows(CompEntities entities, PaymentRowSaveDTO paymentRowInput, List<AccountingRowDTO> accountingRowDTOs, int actorId, int actorCompanyId)
        {
            List<PaymentAccountRow> paymentAccountRows = new List<PaymentAccountRow>();

            if (paymentRowInput == null || accountingRowDTOs == null)
                return paymentAccountRows;

            // Get AccountInternals only once
            List<AccountInternal> accountInternals = AccountManager.GetAccountInternals(entities, actorCompanyId, true);

            foreach (AccountingRowDTO item in accountingRowDTOs)
            {
                PaymentAccountRow paymentAccountRow = new PaymentAccountRow()
                {
                    PaymentAccountRowId = item.InvoiceAccountRowId,
                    RowNr = item.RowNr,
                    Amount = item.DebitAmount - item.CreditAmount,
                    Quantity = item.Quantity,
                    Text = item.Text,
                    VatRow = item.IsVatRow,
                    InterimRow = item.IsInterimRow,
                    CreditRow = item.IsCreditRow,
                    DebitRow = item.IsDebitRow,
                    State = (int)item.State,

                    //Set references
                    AccountStd = AccountManager.GetAccountStd(entities, item.Dim1Id, actorCompanyId, true, false),
                };

                if (paymentAccountRow.AccountStd is null)
                    throw new SoeEntityNotFoundException($"Account {item.Dim1Nr}", new KeyNotFoundException(item.Dim1Nr), "PaymentManager.ConvertToPaymentAccountRows");

                //Set currency amounts
                CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, paymentAccountRow, paymentRowInput.CurrencyRate, actorId, paymentRowInput.CurrencyDate);

                // Get internal accounts (Dim2-6)
                if (item.Dim2Id != 0)
                {
                    AccountInternal accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == item.Dim2Id);
                    if (accountInternal != null)
                        paymentAccountRow.AccountInternal.Add(accountInternal);
                }
                if (item.Dim3Id != 0)
                {
                    AccountInternal accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == item.Dim3Id);
                    if (accountInternal != null)
                        paymentAccountRow.AccountInternal.Add(accountInternal);
                }
                if (item.Dim4Id != 0)
                {
                    AccountInternal accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == item.Dim4Id);
                    if (accountInternal != null)
                        paymentAccountRow.AccountInternal.Add(accountInternal);
                }
                if (item.Dim5Id != 0)
                {
                    AccountInternal accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == item.Dim5Id);
                    if (accountInternal != null)
                        paymentAccountRow.AccountInternal.Add(accountInternal);
                }
                if (item.Dim6Id != 0)
                {
                    AccountInternal accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == item.Dim6Id);
                    if (accountInternal != null)
                        paymentAccountRow.AccountInternal.Add(accountInternal);
                }

                paymentAccountRows.Add(paymentAccountRow);
            }

            return paymentAccountRows;
        }

        #endregion

        #region PaymentAccountRow

        public List<PaymentAccountRow> GetPaymentAccountRows(int paymentRowId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.PaymentAccountRow.NoTracking();
            return GetPaymentAccountRows(entities, paymentRowId);
        }

        public List<PaymentAccountRow> GetPaymentAccountRows(CompEntities entities, int paymentRowId)
        {
            return (from par in entities.PaymentAccountRow
                    where par.State == (int)SoeEntityState.Active &&
                          par.PaymentRow.PaymentRowId == paymentRowId
                    select par
                    ).ToList();
        }

        #endregion

        #region Cancel Payments

        #region Entry points

        public ActionResult CancelPayment(List<ChangeStatusGridViewDTO> items, int actorCompanyId)
        {
            // Default result is successful
            ActionResult result = new ActionResult(true);

            if (items == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "ChangeStatusGridView");

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        List<PaymentRow> paymentRows = new List<PaymentRow>();
                        List<Payment> payments = new List<Payment>();

                        #region Load Payments and PaymentRows

                        List<int> paymenRowIds = items.Select(i => i.PaymentRowId).Distinct().ToList();
                        foreach (int paymentRowId in paymenRowIds)
                        {
                            PaymentRow paymentRow = GetPaymentRow(entities, paymentRowId);
                            if (paymentRow != null)
                            {
                                #region Add Payment

                                //Make sure Payment is loaded
                                if (!paymentRow.PaymentReference.IsLoaded)
                                    paymentRow.PaymentReference.Load();

                                if (paymentRow.Payment != null)
                                {
                                    //Make sure PaymentExport is loaded
                                    if (!paymentRow.Payment.PaymentExportReference.IsLoaded)
                                        paymentRow.Payment.PaymentExportReference.Load();

                                    if (paymentRow.Payment.PaymentExport != null && !payments.Any(i => i.PaymentId == paymentRow.Payment.PaymentId))
                                        payments.Add(paymentRow.Payment);
                                }

                                #endregion

                                #region Add PaymentRow

                                paymentRows.Add(paymentRow);

                                #endregion
                            }
                        }

                        #endregion

                        foreach (Payment payment in payments)
                        {
                            #region Payment

                            if (payment.PaymentExport == null)
                                continue;

                            if (payment.PaymentExport.State == (int)SoeEntityState.Deleted)
                                return new ActionResult((int)ActionResultSave.CancelPaymentFailedAlreadyCancelled);

                            //Check if all PaymentRows for Payment exists
                            List<PaymentRow> paymentRowsForPayment = paymentRows.Where(i => i.Payment.PaymentId == payment.PaymentId).ToList();
                            if (paymentRowsForPayment.Count < payment.PaymentExport.NumberOfPayments)
                                continue;

                            //Cancel PaymentRows
                            result = CancelPaymentRows(entities, transaction, paymentRowsForPayment, actorCompanyId, false);
                            if (!result.Success)
                                return result;

                            //Cancel PaymentExport
                            result = CancelPaymentExport(entities, transaction, payment.PaymentExport, actorCompanyId, false);
                            if (!result.Success)
                                return result;

                            //Remove handled PaymentRows
                            paymentRows = paymentRows.Where(i => i.Payment.PaymentId != payment.PaymentId).ToList();

                            #endregion
                        }

                        foreach (PaymentRow paymentRow in paymentRows)
                        {
                            #region PaymentRow

                            //Cancel PaymentRow
                            result = CancelPaymentRow(entities, transaction, paymentRow, actorCompanyId, false);
                            if (!result.Success)
                                return result;

                            #endregion
                        }

                        #region PaymentSuggestion

                        foreach (ChangeStatusGridViewDTO item in items)
                        {
                            if (item.SequenceNumberRecordId != 0)
                                SequenceNumberManager.DeleteSequenceNumberRecord(entities, item.SequenceNumberRecordId, actorCompanyId);
                        }

                        #endregion

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
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            return result;
        }

        public ActionResult CancelPayment(List<SupplierPaymentGridDTO> items, int actorCompanyId)
        {
            // Default result is successful
            ActionResult result = new ActionResult(true);

            if (items == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "ChangeStatusGridView");

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        List<PaymentRow> paymentRows = new List<PaymentRow>();
                        List<Payment> payments = new List<Payment>();

                        #region Load Payments and PaymentRows

                        List<int> paymenRowIds = items.Select(i => i.PaymentRowId).Distinct().ToList();
                        foreach (int paymentRowId in paymenRowIds)
                        {
                            PaymentRow paymentRow = GetPaymentRow(entities, paymentRowId);
                            if (paymentRow != null)
                            {
                                #region Add Payment

                                //Make sure Payment is loaded
                                if (!paymentRow.PaymentReference.IsLoaded)
                                    paymentRow.PaymentReference.Load();

                                if (paymentRow.Payment != null)
                                {
                                    //Make sure PaymentExport is loaded
                                    if (!paymentRow.Payment.PaymentExportReference.IsLoaded)
                                        paymentRow.Payment.PaymentExportReference.Load();

                                    if (paymentRow.Payment.PaymentExport != null && !payments.Any(i => i.PaymentId == paymentRow.Payment.PaymentId))
                                        payments.Add(paymentRow.Payment);
                                }

                                #endregion

                                #region Add PaymentRow

                                paymentRows.Add(paymentRow);

                                #endregion
                            }
                        }

                        #endregion

                        foreach (Payment payment in payments)
                        {
                            #region Payment

                            if (payment.PaymentExport == null)
                                continue;

                            if (payment.PaymentExport.State == (int)SoeEntityState.Deleted)
                                return new ActionResult((int)ActionResultSave.CancelPaymentFailedAlreadyCancelled);

                            //Check if all PaymentRows for Payment exists
                            List<PaymentRow> paymentRowsForPayment = paymentRows.Where(i => i.Payment.PaymentId == payment.PaymentId).ToList();
                            if (paymentRowsForPayment.Count < payment.PaymentExport.NumberOfPayments)
                                continue;

                            //Cancel PaymentRows
                            result = CancelPaymentRows(entities, transaction, paymentRowsForPayment, actorCompanyId, false);
                            if (!result.Success)
                                return result;

                            //Cancel PaymentExport
                            result = CancelPaymentExport(entities, transaction, payment.PaymentExport, actorCompanyId, false);
                            if (!result.Success)
                                return result;

                            //Remove handled PaymentRows
                            paymentRows = paymentRows.Where(i => i.Payment.PaymentId != payment.PaymentId).ToList();

                            #endregion
                        }

                        foreach (PaymentRow paymentRow in paymentRows)
                        {
                            #region PaymentRow

                            //Cancel PaymentRow
                            result = CancelPaymentRow(entities, transaction, paymentRow, actorCompanyId, false);
                            if (!result.Success)
                                return result;

                            #endregion
                        }

                        #region PaymentSuggestion

                        foreach (SupplierPaymentGridDTO item in items)
                        {
                            if (item.SequenceNumberRecordId != 0)
                                SequenceNumberManager.DeleteSequenceNumberRecord(entities, item.SequenceNumberRecordId, actorCompanyId);
                        }

                        #endregion

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
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            return result;
        }

        public ActionResult CancelPaymentRow(int paymentRowId, int actorCompanyId, bool revertVoucher = false)
        {
            // Default result is successful
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        PaymentRow paymentRow = GetPaymentRow(entities, paymentRowId);

                        //Cancel PaymentRow
                        result = CancelPaymentRow(entities, transaction, paymentRow, actorCompanyId, false, revertVoucher);
                        if (!result.Success)
                            return result;

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
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            return result;
        }

        public ActionResult CancelPaymentRowWithVoucher(int paymentRowId, int actorCompanyId)
        {
            // Default result is successful
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        PaymentRow paymentRow = GetPaymentRow(entities, paymentRowId);

                        //Cancel PaymentRow
                        //Make sure Invoice is loaded
                        if (!paymentRow.InvoiceReference.IsLoaded)
                            paymentRow.InvoiceReference.Load();

                        if (paymentRow.Invoice == null)
                            return new ActionResult((int)ActionResultSave.EntityIsNull, "Invoice");

                        //Update PaymentRow
                        paymentRow.Status = (int)SoePaymentStatus.Cancel;
                        SetModifiedProperties(paymentRow);

                        if (paymentRow.Invoice is SupplierInvoice)
                        {
                            #region SupplierInvoice

                            SupplierInvoice supplierInvoice = paymentRow.Invoice as SupplierInvoice;

                            //PaidAmount
                            if (supplierInvoice.PaidAmount != 0)
                                supplierInvoice.PaidAmount -= paymentRow.Amount;
                            if (supplierInvoice.PaidAmountCurrency != 0)
                                supplierInvoice.PaidAmountCurrency -= paymentRow.AmountCurrency;
                            if (supplierInvoice.PaidAmountLedgerCurrency != 0)
                                supplierInvoice.PaidAmountLedgerCurrency -= paymentRow.AmountLedgerCurrency;
                            if (supplierInvoice.PaidAmountEntCurrency != 0)
                                supplierInvoice.PaidAmountEntCurrency -= paymentRow.AmountEntCurrency;

                            //FullyPayed
                            InvoiceManager.SetInvoiceFullyPayed(entities, supplierInvoice, supplierInvoice.IsTotalAmountPayed);

                            #endregion
                        }
                        else if (paymentRow.Invoice is CustomerInvoice)
                        {
                            #region CustomerInvoice

                            CustomerInvoice customerInvoice = paymentRow.Invoice as CustomerInvoice;

                            //PaidAmount
                            if (customerInvoice.PaidAmount != 0)
                                customerInvoice.PaidAmount -= paymentRow.Amount;
                            if (customerInvoice.PaidAmountCurrency != 0)
                                customerInvoice.PaidAmountCurrency -= paymentRow.AmountCurrency;
                            if (customerInvoice.PaidAmountLedgerCurrency != 0)
                                customerInvoice.PaidAmountLedgerCurrency -= paymentRow.AmountLedgerCurrency;
                            if (customerInvoice.PaidAmountEntCurrency != 0)
                                customerInvoice.PaidAmountEntCurrency -= paymentRow.AmountEntCurrency;

                            //FullyPayed
                            InvoiceManager.SetInvoiceFullyPayed(entities, customerInvoice, customerInvoice.IsTotalAmountPayed);

                            #endregion
                        }

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
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            return result;
        }

        public ActionResult CancelPaymentFromPaymentExport(int paymentExportId, int actorCompanyId)
        {
            // Default result is successful
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        Payment payment = GetPaymentByPaymentExport(entities, paymentExportId, true);
                        if (payment == null)
                            return new ActionResult((int)ActionResultSave.EntityIsNull, "Payment");
                        if (payment.PaymentExport == null)
                            return new ActionResult((int)ActionResultSave.EntityIsNull, "PaymentExport");

                        //Cancel PaymentRows
                        result = CancelPaymentRows(entities, transaction, payment.PaymentRow.ToList(), actorCompanyId, false);
                        if (!result.Success)
                            return result;

                        //Cancel PaymentExport
                        result = CancelPaymentExport(entities, transaction, payment.PaymentExport, actorCompanyId, false);
                        if (!result.Success)
                            return result;

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
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            return result;
        }

        #endregion

        public ActionResult CancelPaymentRows(CompEntities entities, TransactionScope transaction, List<PaymentRow> paymentRows, int actorCompanyId, bool saveChanges = true)
        {
            // Default result is successful
            ActionResult result = new ActionResult(true);

            if (paymentRows == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "PaymentRow");

            foreach (PaymentRow paymentRow in paymentRows)
            {
                result = CancelPaymentRow(entities, transaction, paymentRow, actorCompanyId, false);
                if (!result.Success)
                    return result;
            }

            if (saveChanges)
                result = SaveChanges(entities, transaction);

            return result;
        }

        public ActionResult CancelPaymentRow(CompEntities entities, TransactionScope transaction, PaymentRow paymentRow, int actorCompanyId, bool saveChanges = true, bool revertVoucher = false)
        {
            if (paymentRow == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "PaymentRow");

            #region Prereq

            // Default result is successful
            var result = new ActionResult(true);

            VoucherHead voucher = null;

            // Cannot be cancelled if is transferred to voucher
            if (paymentRow.VoucherHeadId.HasValue)
            {
                if (revertVoucher && paymentRow.VoucherHeadId.HasValue)
                {
                    voucher = VoucherManager.GetVoucherHead(entities, paymentRow.VoucherHeadId.Value, false, true, true);
                    if (voucher != null)
                    {
                        /*if (!voucher.PaymentRow.IsLoaded)
                            voucher.PaymentRow.Load();
                            
                        // Validate that only one payment is connected to the voucher
                        if(voucher.PaymentRow.Count > 1)
                            return new ActionResult((int)ActionResultSave.AccountPeriodNotOpen, GetText(111, (int)TermGroup.ChangeStatusGrid, "Perioden är inte öppen"));*/

                        if (!voucher.AccountPeriodReference.IsLoaded)
                            voucher.AccountPeriodReference.Load();

                        result = AccountManager.ValidateAccountPeriod(voucher.AccountPeriod);
                        if (!result.Success)
                            return new ActionResult((int)ActionResultSave.AccountPeriodNotOpen, GetText(111, (int)TermGroup.ChangeStatusGrid, "Perioden är inte öppen"));
                    }
                }
                else
                {
                    return new ActionResult((int)ActionResultSave.CancelPaymentFailedTransferredToVoucher, GetText(7417));
                }
            }
            if (paymentRow.Status == (int)SoePaymentStatus.Cancel)
                return new ActionResult((int)ActionResultSave.CancelPaymentFailedAlreadyCancelled, GetText(7418));

            //Make sure Invoice is loaded
            if (!paymentRow.InvoiceReference.IsLoaded)
                paymentRow.InvoiceReference.Load();

            if (paymentRow.Invoice == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "Invoice");

            #endregion

            //Update PaymentRow
            paymentRow.Status = (int)SoePaymentStatus.Cancel;
            SetModifiedProperties(paymentRow);

            if (paymentRow.Invoice is SupplierInvoice)
            {
                #region SupplierInvoice

                SupplierInvoice supplierInvoice = paymentRow.Invoice as SupplierInvoice;

                //PaidAmount
                if (supplierInvoice.PaidAmount != 0)
                    supplierInvoice.PaidAmount -= paymentRow.Amount;
                if (supplierInvoice.PaidAmountCurrency != 0)
                    supplierInvoice.PaidAmountCurrency -= paymentRow.AmountCurrency;
                if (supplierInvoice.PaidAmountLedgerCurrency != 0)
                    supplierInvoice.PaidAmountLedgerCurrency -= paymentRow.AmountLedgerCurrency;
                if (supplierInvoice.PaidAmountEntCurrency != 0)
                    supplierInvoice.PaidAmountEntCurrency -= paymentRow.AmountEntCurrency;

                //FullyPayed
                InvoiceManager.SetInvoiceFullyPayed(entities, supplierInvoice, supplierInvoice.IsTotalAmountPayed);

                #endregion
            }
            else if (paymentRow.Invoice is CustomerInvoice)
            {
                #region CustomerInvoice

                CustomerInvoice customerInvoice = paymentRow.Invoice as CustomerInvoice;

                //PaidAmount
                if (customerInvoice.PaidAmount != 0)
                    customerInvoice.PaidAmount -= paymentRow.Amount;
                if (customerInvoice.PaidAmountCurrency != 0)
                    customerInvoice.PaidAmountCurrency -= paymentRow.AmountCurrency;
                if (customerInvoice.PaidAmountLedgerCurrency != 0)
                    customerInvoice.PaidAmountLedgerCurrency -= paymentRow.AmountLedgerCurrency;
                if (customerInvoice.PaidAmountEntCurrency != 0)
                    customerInvoice.PaidAmountEntCurrency -= paymentRow.AmountEntCurrency;

                InvoiceManager.SetInvoiceFullyPayed(entities, customerInvoice, customerInvoice.IsTotalAmountPayed);

                // Delete any connected CustomerInvoiceInterest that's not already invoiced

                if (!customerInvoice.CustomerInvoiceInterestOrigin.IsLoaded)
                    customerInvoice.CustomerInvoiceInterestOrigin.Load();

                foreach (var interest in customerInvoice.CustomerInvoiceInterestOrigin.ToList())
                {
                    var interestIsInvoiced = entities.CustomerInvoiceRow.Any(r => r.CustomerInvoiceInterestId == interest.CustomerInvoiceInterestId && r.State == (int)SoeEntityState.Active);

                    if (!interestIsInvoiced)
                        entities.DeleteObject(interest);
                }

                #endregion
            }

            // Handle voucher
            if (voucher != null)
            {
                // Convert to dto
                var voucherDto = voucher.ToDTO(false, false);
                voucherDto.Rows = new List<VoucherRowDTO>();
                // Reset values
                voucherDto.VoucherHeadId = 0;
                voucherDto.Text = GetText(11824, "Makulerad") + ": " + voucher.Text; //Char.ToLowerInvariant(voucher.Text[0]) + voucher.Text.Substring(1);

                foreach (var row in voucher.VoucherRow)
                {
                    // Convert to dto
                    var rowDto = row.ToDTO(false);

                    // Handle parent
                    rowDto.TempRowId = row.VoucherRowId;
                    rowDto.ParentRowId = row.ParentRowId;

                    // Empty ids
                    rowDto.VoucherHeadId = 0;
                    rowDto.VoucherRowId = 0;

                    // Revert amounts
                    rowDto.Amount = Decimal.Negate(rowDto.Amount);
                    rowDto.AmountEntCurrency = Decimal.Negate(rowDto.AmountEntCurrency);

                    // Modifie text
                    rowDto.Text = GetText(11824, "Makulerad") + ": " + rowDto.Text;

                    //Add
                    voucherDto.Rows.Add(rowDto);
                }

                result = VoucherManager.SaveVoucher(entities, transaction, voucherDto, new List<AccountingRowDTO>(), new List<int>(), 0, base.ActorCompanyId, false, true);
            }

            if (saveChanges)
                result = SaveChanges(entities, transaction);

            return result;
        }

        public ActionResult CancelPaymentExport(CompEntities entities, TransactionScope transaction, PaymentExport paymentExport, int actorCompanyId, bool saveChanges = true)
        {
            // Default result is successful
            ActionResult result = new ActionResult(true);

            if (paymentExport == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "PaymentExport");

            ChangeEntityState(paymentExport, SoeEntityState.Deleted);

            if (saveChanges)
                result = SaveChanges(entities, transaction);

            return result;
        }

        #endregion

        #region PaymentTraceView

        public List<PaymentTraceViewDTO> GetPaymentTraceViews(int paymentRowId, int baseSysCurrencyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.PaymentTraceView.NoTracking();
            return GetPaymentTraceViews(entities, paymentRowId, baseSysCurrencyId);
        }

        public List<PaymentTraceViewDTO> GetPaymentTraceViews(CompEntities entities, int paymentRowId, int baseSysCurrencyId)
        {
            List<PaymentTraceViewDTO> dtos = new List<PaymentTraceViewDTO>();

            var items = (from v in entities.PaymentTraceView
                         where v.PaymentRowId == paymentRowId
                         select v).ToList();

            if (!items.IsNullOrEmpty())
            {
                int langId = GetLangId();
                var originTypes = base.GetTermGroupDict(TermGroup.OriginType, langId);
                var originStatuses = base.GetTermGroupDict(TermGroup.OriginStatus, langId);
                var projectTypes = base.GetTermGroupDict(TermGroup.ProjectType, langId);
                var projectStatuses = base.GetTermGroupDict(TermGroup.ProjectStatus, langId);
                var tracingTexts = base.GetTermGroupDict(TermGroup.Tracing, langId);

                foreach (var item in items)
                {
                    var dto = item.ToDTO();

                    dto.Foreign = item.SysCurrencyId != baseSysCurrencyId;
                    dto.CurrencyCode = CountryCurrencyManager.GetCurrencyCode(dto.SysCurrencyId);
                    if (dto.IsProject)
                    {
                        dto.OriginTypeName = dto.OriginType != 0 ? projectTypes[(int)dto.OriginType] : "";
                        dto.OriginStatusName = dto.OriginStatus != 0 ? projectStatuses[(int)dto.OriginStatus] : "";
                    }
                    else if (dto.IsVoucher)
                    {
                        dto.OriginTypeName = tracingTexts[22];
                        dto.OriginStatusName = "";
                    }
                    else if (dto.IsImport)
                    {
                        dto.OriginTypeName = tracingTexts[41];
                        dto.OriginStatusName = "";
                    }
                    else
                    {
                        dto.OriginTypeName = dto.OriginType != 0 ? originTypes[(int)dto.OriginType] : "";
                        dto.OriginStatusName = dto.OriginStatus != 0 ? originStatuses[(int)dto.OriginStatus] : "";
                    }

                    dtos.Add(dto);
                }
            }

            return dtos;
        }

        #endregion

        #region PaymentInformation

        public PaymentInformation GetPaymentInformationFromActor(int actorId, bool loadPaymentInformationRows, bool loadActor)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.PaymentInformation.NoTracking();
            return GetPaymentInformationFromActor(entities, actorId, loadPaymentInformationRows, loadActor);
        }

        public PaymentInformation GetPaymentInformationFromActor(CompEntities entities, int actorId, bool loadPaymentInformationRows, bool loadActor)
        {
            PaymentInformation paymentInformation = (from pi in entities.PaymentInformation
                                                     where pi.Actor.ActorId == actorId &&
                                                     pi.State == (int)SoeEntityState.Active
                                                     select pi).FirstOrDefault();

            if (paymentInformation != null)
            {
                if (loadPaymentInformationRows && !paymentInformation.PaymentInformationRow.IsLoaded)
                {
                    paymentInformation.PaymentInformationRow.Load();
                }

                if (loadActor && !paymentInformation.ActorReference.IsLoaded)
                {
                    paymentInformation.ActorReference.Load();
                }
            }

            return paymentInformation;
        }

        public IEnumerable<KeyValuePair<int, List<PaymentInformationRowDTO>>> GetPaymentInformationRowDTOsFromActors(params int[] actorIds)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.PaymentInformation.NoTracking();
            return GetPaymentInformationRowDTOsFromActors(entities, actorIds);
        }

        public IEnumerable<KeyValuePair<int, List<PaymentInformationRowDTO>>> GetPaymentInformationRowDTOsFromActors(CompEntities entities, params int[] actorIds)
        {
            var query = (from actor in entities.Actor.Include("PaymentInformation.PaymentInformationRow")
                         where actorIds.Contains(actor.ActorId)
                         select actor);

            foreach (var actor in query.ToList())
            {
                var pirList = new List<PaymentInformationRowDTO>();

                if (!actor.SupplierReference.IsLoaded)
                    actor.SupplierReference.Load();
                if (actor.Supplier != null && !actor.Supplier.FactoringSupplierReference.IsLoaded)
                    actor.Supplier.FactoringSupplierReference.Load();

                if (actor.Supplier != null && actor.Supplier.FactoringSupplier != null)
                {
                    actor.Supplier.FactoringSupplier.ActorReference.Load();
                    actor.Supplier.FactoringSupplier.Actor.PaymentInformation.Load();
                    foreach (var paymentinformation in actor.Supplier.FactoringSupplier.Actor.PaymentInformation)
                    {
                        paymentinformation.PaymentInformationRow.Load();
                        foreach (var pir in paymentinformation.PaymentInformationRow)
                        {
                            if (pir.State == (int)SoeEntityState.Active)
                            {
                                pirList.Add(new PaymentInformationRowDTO()
                                {
                                    PaymentInformationRowId = pir.PaymentInformationRowId,
                                    PaymentNr = pir.PaymentNr,
                                    SysPaymentTypeId = pir.SysPaymentTypeId,
                                    SysPaymentTypeName = ((TermGroup_SysPaymentType)pir.SysPaymentTypeId).ToString(),
                                    Default = pir.Default,
                                });
                            }
                        }
                    }
                }
                else
                {
                    foreach (var paymentinformation in actor.PaymentInformation)
                    {
                        foreach (var pir in paymentinformation.PaymentInformationRow)
                        {
                            if (pir.State == (int)SoeEntityState.Active)
                            {
                                pirList.Add(new PaymentInformationRowDTO()
                                {
                                    PaymentInformationRowId = pir.PaymentInformationRowId,
                                    PaymentNr = pir.PaymentNr,
                                    SysPaymentTypeId = pir.SysPaymentTypeId,
                                    SysPaymentTypeName = ((TermGroup_SysPaymentType)pir.SysPaymentTypeId).ToString(),
                                    Default = pir.Default,
                                });
                            }
                        }
                    }

                }
                yield return new KeyValuePair<int, List<PaymentInformationRowDTO>>(actor.ActorId, pirList);
            }
        }

        public Dictionary<int, List<PaymentInformationRowDTO>> GetPaymentInformationRowsFromActors(List<int> actorIds)
        {
            var dict = new Dictionary<int, List<PaymentInformationRowDTO>>();

            foreach (var item in GetPaymentInformationRowDTOsFromActors(actorIds.ToArray()))
            {
                dict.Add(item.Key, item.Value);
            }

            return dict;
        }

        public Dictionary<int, List<PaymentInformationRowDTO>> GetPaymentInformationRowsFromActorsFilterByPaymentMethod(List<int> actorIds, int paymentMethodId)
        {
            var dict = new Dictionary<int, List<PaymentInformationRowDTO>>();
            using (CompEntities entities = new CompEntities())
            {

                foreach (var item in GetPaymentInformationRowDTOsFromActors(entities, actorIds.ToArray()))
                {
                    PaymentMethod paymentMethod = GetPaymentMethod(entities, paymentMethodId, base.ActorCompanyId);
                    if (paymentMethod != null)
                    {
                        List<PaymentInformationRowDTO> validPaymentInformations = new List<PaymentInformationRowDTO>();
                        if (paymentMethod.SysPaymentMethodId == (int)TermGroup_SysPaymentMethod.Autogiro)
                        {
                            foreach (var actPayInfo in item.Value.Where(p => p.PaymentNr.Trim() != ""))
                            {
                                List<PaymentExportTypesDTO> validExportTypes = Constants.PaymentManager.SupportedExportTypes.Where(p => (int)p.SysPaymentType == actPayInfo.SysPaymentTypeId && (int)p.PaymentMethod == paymentMethod.SysPaymentMethodId).ToList();
                                foreach (PaymentExportTypesDTO exportType in validExportTypes)
                                {
                                    PaymentInformationRowDTO dto = actPayInfo.CloneDTO();
                                    dto.BillingType = exportType.BillingType;
                                    validPaymentInformations.Add(dto);
                                }

                                validPaymentInformations = (from p in validPaymentInformations
                                                            orderby (actPayInfo.Default == true) descending
                                                            select p).ToList();
                            }
                        }
                        else
                        {
                            foreach (var actPayInfo in item.Value)
                            {
                                List<PaymentExportTypesDTO> validExportTypes = Constants.PaymentManager.SupportedExportTypes.Where(p => (int)p.SysPaymentType == actPayInfo.SysPaymentTypeId && (int)p.PaymentMethod == paymentMethod.SysPaymentMethodId).ToList();
                                foreach (PaymentExportTypesDTO exportType in validExportTypes)
                                {
                                    PaymentInformationRowDTO dto = actPayInfo.CloneDTO();
                                    dto.BillingType = exportType.BillingType;

                                    if (!string.IsNullOrEmpty(dto.BIC))
                                    {
                                        dto.PaymentNrDisplay = dto.PaymentNr + " (" + dto.BIC + ")";
                                    }
                                    else if (dto.PaymentNr.Contains('/'))
                                    {
                                        var parts = dto.PaymentNr.Split('/');
                                        dto.PaymentNrDisplay = parts[1] + " (" + parts[0] + ")";
                                    }

                                    validPaymentInformations.Add(dto);
                                }

                                validPaymentInformations = (from p in validPaymentInformations
                                                            orderby (paymentMethod.PaymentInformationRow != null && (paymentMethod.PaymentInformationRow.SysPaymentTypeId == actPayInfo.SysPaymentTypeId)) descending
                                                            select p).ToList();
                            }
                        }

                        dict.Add(item.Key, validPaymentInformations);
                    }
                }
            }
            return dict;
        }

        public Dictionary<TermGroup_SysPaymentType, string> GetPaymentNrs(CompEntities entities, int actorCompanyId)
        {
            var dict = new Dictionary<TermGroup_SysPaymentType, string>();

            PaymentInformation paymentInformation = PaymentManager.GetPaymentInformationFromActor(entities, actorCompanyId, true, false);
            dict.Add(TermGroup_SysPaymentType.BG, PaymentManager.GetPaymentNr(paymentInformation, TermGroup_SysPaymentType.BG));
            dict.Add(TermGroup_SysPaymentType.PG, PaymentManager.GetPaymentNr(paymentInformation, TermGroup_SysPaymentType.PG));
            dict.Add(TermGroup_SysPaymentType.Bank, PaymentManager.GetPaymentNr(paymentInformation, TermGroup_SysPaymentType.Bank));
            dict.Add(TermGroup_SysPaymentType.BIC, PaymentManager.GetPaymentNr(paymentInformation, TermGroup_SysPaymentType.BIC));
            dict.Add(TermGroup_SysPaymentType.SEPA, PaymentManager.GetPaymentNr(paymentInformation, TermGroup_SysPaymentType.SEPA));

            return dict;
        }

        public PaymentInformationRow GetDefaultPaymentInformationRow(CompEntities entities, int actorId)
        {
            PaymentInformationRow paymentInformationRow = null;

            PaymentInformation paymentInformation = (from pi in entities.PaymentInformation
                                                     where pi.Actor.ActorId == actorId &&
                                                     pi.State == (int)SoeEntityState.Active
                                                     select pi).FirstOrDefault();

            if (paymentInformation != null && paymentInformation.DefaultSysPaymentTypeId != 0)
            {
                paymentInformationRow = (from pi in entities.PaymentInformationRow
                                         where pi.PaymentInformation.PaymentInformationId == paymentInformation.PaymentInformationId &&
                                         pi.SysPaymentTypeId == paymentInformation.DefaultSysPaymentTypeId &&
                                         pi.Default == true &&
                                         pi.State == (int)SoeEntityState.Active
                                         select pi).FirstOrDefault();
            }

            return paymentInformationRow;
        }

        public string GetPaymentNr(PaymentInformation paymentInformation, TermGroup_SysPaymentType type)
        {
            if (paymentInformation == null || !paymentInformation.ActivePaymentInformationRows.Any())
                return String.Empty;

            var rows = paymentInformation.ActivePaymentInformationRows.Where(i => i.SysPaymentTypeId == (int)type).ToList();
            var row = rows.OrderByDescending(i => i.Default).FirstOrDefault();
            return row != null ? row.PaymentNr : String.Empty;
        }

        public string GetPaymentBIC(PaymentInformation paymentInformation, TermGroup_SysPaymentType type)
        {
            if (paymentInformation == null || !paymentInformation.ActivePaymentInformationRows.Any())
                return String.Empty;

            var rows = paymentInformation.ActivePaymentInformationRows.Where(i => i.SysPaymentTypeId == (int)type).ToList();
            var row = rows.OrderByDescending(i => i.Default).FirstOrDefault();
            return row != null ? row.BIC : String.Empty;
        }

        private ActionResult SaveTrackChanges(CompEntities entities, TransactionScope transaction, PaymentInformationRowDTO rowInput, PaymentInformationRow paymentInformationRow, int actorId, SoeEntityType actorType, TermGroup_TrackChangesAction actionType, int actorCompanyId)
        {
            var trackStringFields = new[] {
                new SmallGenericType {Name= "PaymentNr", Id = (int)TermGroup_TrackChangesColumnType.Payment_PaymentNr },
                new SmallGenericType {Name= "BIC", Id = (int)TermGroup_TrackChangesColumnType.Payment_BIC },
                new SmallGenericType {Name= "CurrencyAccount", Id = (int)TermGroup_TrackChangesColumnType.Payment_CurrencyAccount },
                new SmallGenericType {Name= "ClearingCode", Id = (int)TermGroup_TrackChangesColumnType.Payment_ClearingCode },
                new SmallGenericType {Name= "PaymentCode", Id = (int)TermGroup_TrackChangesColumnType.Payment_PaymentCode },
            }.ToList();

            var paymentInformationRowId = actionType == TermGroup_TrackChangesAction.Insert ? rowInput.PaymentInformationRowId : paymentInformationRow.PaymentInformationRowId;
            var changes = TrackChangesManager.CreateTrackStringChanges(trackStringFields, actorCompanyId, SoeEntityType.PaymentInformationRow, paymentInformationRowId, paymentInformationRow, rowInput, actionType, actorType, actorId);

            if (
                    (actionType == TermGroup_TrackChangesAction.Insert && rowInput.PaymentForm.GetValueOrDefault() > 0) ||
                    (actionType == TermGroup_TrackChangesAction.Delete && paymentInformationRow.PaymentForm.GetValueOrDefault() > 0) ||
                    (actionType == TermGroup_TrackChangesAction.Update && rowInput.PaymentForm.GetValueOrDefault() != paymentInformationRow.PaymentForm.GetValueOrDefault())
               )
            {
                var fromValueName = paymentInformationRow == null ? null : GetText(paymentInformationRow.PaymentForm.GetValueOrDefault(), TermGroup.ForeignPaymentForm);
                var toValueName = rowInput == null ? null : GetText(rowInput.PaymentForm.GetValueOrDefault(), TermGroup.ForeignPaymentForm);
                changes.Add(TrackChangesManager.CreateTrackChangesDTO(actorCompanyId, SoeEntityType.PaymentInformationRow, TermGroup_TrackChangesColumnType.Payment_PaymentForm, paymentInformationRowId, paymentInformationRow?.PaymentForm.ToString(), rowInput?.PaymentForm.ToString(), actionType, SettingDataType.Integer, actorType, actorId, fromValueName, toValueName));
            }

            if (
                (actionType == TermGroup_TrackChangesAction.Insert && rowInput.PaymentMethodCode.GetValueOrDefault() > 0) ||
                (actionType == TermGroup_TrackChangesAction.Delete && paymentInformationRow.PaymentMethodCode.GetValueOrDefault() > 0) ||
                (actionType == TermGroup_TrackChangesAction.Update && rowInput.PaymentMethodCode.GetValueOrDefault() != paymentInformationRow.PaymentMethodCode.GetValueOrDefault())
            )
            {
                var fromValueName = paymentInformationRow == null ? null : GetText(paymentInformationRow.PaymentMethodCode.GetValueOrDefault(), TermGroup.ForeignPaymentMethod);
                var toValueName = rowInput == null ? null : GetText(rowInput.PaymentMethodCode.GetValueOrDefault(), TermGroup.ForeignPaymentMethod);
                changes.Add(TrackChangesManager.CreateTrackChangesDTO(actorCompanyId, SoeEntityType.PaymentInformationRow, TermGroup_TrackChangesColumnType.Payment_PaymentMethod, paymentInformationRowId, paymentInformationRow?.PaymentMethodCode.ToString(), rowInput?.PaymentMethodCode.ToString(), actionType, SettingDataType.Integer, actorType, actorId, fromValueName, toValueName));
            }

            if (
                (actionType == TermGroup_TrackChangesAction.Insert && rowInput.ChargeCode.GetValueOrDefault() > 0) ||
                (actionType == TermGroup_TrackChangesAction.Delete && paymentInformationRow.ChargeCode.GetValueOrDefault() > 0) ||
                (actionType == TermGroup_TrackChangesAction.Update && rowInput.ChargeCode.GetValueOrDefault() != paymentInformationRow.ChargeCode.GetValueOrDefault())
            )
            {
                var fromValueName = paymentInformationRow == null ? null : GetText(paymentInformationRow.ChargeCode.GetValueOrDefault(), TermGroup.ForeignPaymentChargeCode);
                var toValueName = rowInput == null ? null : GetText(rowInput.ChargeCode.GetValueOrDefault(), TermGroup.ForeignPaymentChargeCode);
                changes.Add(TrackChangesManager.CreateTrackChangesDTO(actorCompanyId, SoeEntityType.PaymentInformationRow, TermGroup_TrackChangesColumnType.Payment_ChargeCode, paymentInformationRowId, paymentInformationRow?.ChargeCode.ToString(), rowInput?.ChargeCode.ToString(), actionType, SettingDataType.Integer, actorType, actorId, fromValueName, toValueName));
            }

            if (
                (actionType == TermGroup_TrackChangesAction.Insert && rowInput.IntermediaryCode.GetValueOrDefault() > 0) ||
                (actionType == TermGroup_TrackChangesAction.Delete && paymentInformationRow.IntermediaryCode.GetValueOrDefault() > 0) ||
                (actionType == TermGroup_TrackChangesAction.Update && rowInput.IntermediaryCode.GetValueOrDefault() != paymentInformationRow.IntermediaryCode.GetValueOrDefault())
            )
            {
                var fromValueName = paymentInformationRow == null ? null : GetText(paymentInformationRow.IntermediaryCode.GetValueOrDefault(), TermGroup.ForeignPaymentIntermediaryCode);
                var toValueName = rowInput == null ? null : GetText(rowInput.IntermediaryCode.GetValueOrDefault(), TermGroup.ForeignPaymentIntermediaryCode);
                changes.Add(TrackChangesManager.CreateTrackChangesDTO(actorCompanyId, SoeEntityType.PaymentInformationRow, TermGroup_TrackChangesColumnType.Payment_IntermediaryCode, paymentInformationRowId, paymentInformationRow?.IntermediaryCode.ToString(), rowInput?.IntermediaryCode.ToString(), actionType, SettingDataType.Integer, actorType, actorId, fromValueName, toValueName));
            }

            return changes.Any() ? TrackChangesManager.AddTrackChanges(entities, transaction, changes) : new ActionResult();
        }

        public ActionResult SavePaymentInformation(CompEntities entities, TransactionScope transaction, List<PaymentInformationRowDTO> rows, int actorId, int defaultSysPaymentTypeId, int actorCompanyId, bool isForegin, bool saveChanges, SoeEntityType actorType)
        {
            ActionResult result = new ActionResult();

            #region Prereq

            if (rows == null)
                return result;

            // Try infer default SysPaymentType automatically if not set
            if (defaultSysPaymentTypeId == 0 && rows.Count == 1)
                defaultSysPaymentTypeId = rows[0].SysPaymentTypeId;


            #endregion

            #region PaymentInformation

            PaymentInformation paymentInformation = GetPaymentInformationFromActor(entities, actorId, true, false);
            if (paymentInformation == null)
            {
                #region Add

                Actor actor = ActorManager.GetActor(entities, actorId, false);
                if (actor == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "Actor");

                paymentInformation = new PaymentInformation()
                {
                    DefaultSysPaymentTypeId = defaultSysPaymentTypeId,

                    //Set references
                    Actor = actor,
                };
                SetCreatedProperties(paymentInformation);
                entities.PaymentInformation.AddObject(paymentInformation);

                #endregion
            }
            else
            {
                #region Update

                paymentInformation.DefaultSysPaymentTypeId = defaultSysPaymentTypeId;
                SetModifiedProperties(paymentInformation);

                #endregion
            }
            if (paymentInformation.PaymentInformationRow == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "PaymentInformation");

            #region Update/Delete

            foreach (PaymentInformationRow paymentInformationRow in paymentInformation.ActivePaymentInformationRows.Where(f => (!isForegin && (f.IntermediaryCode == null || f.IntermediaryCode == 0)) || (isForegin && f.IntermediaryCode > 0)))
            {
                var rowInput = rows.FirstOrDefault(pir => pir.PaymentInformationRowId == paymentInformationRow.PaymentInformationRowId);
                if (rowInput != null)
                {
                    #region Update

                    SaveTrackChanges(entities, transaction, rowInput, paymentInformationRow, actorId, actorType, TermGroup_TrackChangesAction.Update, actorCompanyId);

                    if (paymentInformationRow.SysPaymentTypeId != rowInput.SysPaymentTypeId ||
                        paymentInformationRow.PaymentNr != rowInput.PaymentNr ||
                        paymentInformationRow.Default != rowInput.Default ||
                        paymentInformationRow.ShownInInvoice != rowInput.ShownInInvoice ||
                        paymentInformationRow.BIC != rowInput.BIC ||
                        paymentInformationRow.ClearingCode != rowInput.ClearingCode ||
                        paymentInformationRow.PaymentCode != rowInput.PaymentCode ||
                        paymentInformationRow.PaymentMethodCode != rowInput.PaymentMethodCode ||
                        paymentInformationRow.PaymentForm != rowInput.PaymentForm ||
                        paymentInformationRow.ChargeCode != rowInput.ChargeCode ||
                        paymentInformationRow.IntermediaryCode != rowInput.IntermediaryCode ||
                        paymentInformationRow.CurrencyAccount != rowInput.CurrencyAccount ||
                        paymentInformationRow.CurrencyId != rowInput.CurrencyId)
                    {
                        paymentInformationRow.SysPaymentTypeId = rowInput.SysPaymentTypeId;
                        paymentInformationRow.PaymentNr = rowInput.PaymentNr;
                        paymentInformationRow.Default = rowInput.Default;
                        paymentInformationRow.ShownInInvoice = rowInput.ShownInInvoice;
                        // Foreign payments
                        paymentInformationRow.BIC = rowInput.BIC;
                        paymentInformationRow.ClearingCode = rowInput.ClearingCode;
                        paymentInformationRow.PaymentCode = rowInput.PaymentCode;
                        paymentInformationRow.PaymentMethodCode = rowInput.PaymentMethodCode;
                        paymentInformationRow.PaymentForm = rowInput.PaymentForm;
                        paymentInformationRow.ChargeCode = rowInput.ChargeCode;
                        paymentInformationRow.IntermediaryCode = rowInput.IntermediaryCode;
                        paymentInformationRow.CurrencyAccount = rowInput.CurrencyAccount;
                        paymentInformationRow.CurrencyId = rowInput.CurrencyId;

                        if (string.IsNullOrEmpty(paymentInformationRow.PaymentNr))
                        {
                            return new ActionResult(GetText(paymentInformationRow.SysPaymentTypeId, TermGroup.SysPaymentType) + ": " + GetText(7600, "Betalkonto saknas"));
                        }

                        SetModifiedProperties(paymentInformationRow);
                    }

                    if (this.parameterObject.SoeSupportUser != null)
                    {
                        paymentInformationRow.BankConnected = rowInput.BankConnected;
                    }

                    #endregion
                }
                else
                {
                    #region Delete

                    if (!IsPaymentInformationRowUsedByPaymentMethod(entities, paymentInformationRow.PaymentInformationRowId, actorCompanyId))
                    {
                        SaveTrackChanges(entities, transaction, null, paymentInformationRow, actorId, actorType, TermGroup_TrackChangesAction.Delete, actorCompanyId);
                        ChangeEntityState(paymentInformationRow, SoeEntityState.Deleted);
                    }
                    else
                    {
                        return new ActionResult((int)ActionResultSave.PaymentInformationRowUsedByPaymentMethod, "PaymentInformation");
                    }

                    #endregion
                }
            }

            #endregion

            #region Add

            foreach (var rowInputToAdd in rows.Where(pir => pir.PaymentInformationRowId == 0))
            {
                var paymentInformationRow = new PaymentInformationRow
                {
                    SysPaymentTypeId = rowInputToAdd.SysPaymentTypeId,
                    PaymentNr = rowInputToAdd.PaymentNr,
                    Default = rowInputToAdd.Default,
                    ShownInInvoice = rowInputToAdd.ShownInInvoice,
                    // Foreign payments
                    BIC = rowInputToAdd.BIC,
                    ClearingCode = rowInputToAdd.ClearingCode,
                    PaymentCode = rowInputToAdd.PaymentCode,
                    PaymentMethodCode = rowInputToAdd.PaymentMethodCode,
                    PaymentForm = rowInputToAdd.PaymentForm,
                    ChargeCode = rowInputToAdd.ChargeCode,
                    IntermediaryCode = rowInputToAdd.IntermediaryCode,
                    CurrencyAccount = rowInputToAdd.CurrencyAccount,
                    //Set references
                    PaymentInformation = paymentInformation,
                    BankConnected = rowInputToAdd.BankConnected,
                    CurrencyId = rowInputToAdd.CurrencyId,
                };

                if (string.IsNullOrEmpty(paymentInformationRow.PaymentNr))
                {
                    return new ActionResult(GetText(paymentInformationRow.SysPaymentTypeId, TermGroup.SysPaymentType) + ": " + GetText(7600, "Betalkonto saknas"));
                }

                SetCreatedProperties(paymentInformationRow);
                entities.PaymentInformationRow.AddObject(paymentInformationRow);
                rowInputToAdd.PaymentInformationRowId = paymentInformationRow.PaymentInformationRowId;
                SaveTrackChanges(entities, transaction, rowInputToAdd, null, actorId, actorType, TermGroup_TrackChangesAction.Insert, actorCompanyId);
            }

            #endregion

            return saveChanges ? SaveChanges(entities, transaction) : result;

            #endregion
        }

        public PaymentInformationRow AddPaymentInformationRowToPaymentInformation(PaymentInformation paymentInformation, TermGroup_SysPaymentType sysPaymentType, string paymentNr, bool isDefault, string BIC = null, bool bankConnected = false)
        {
            var paymentInformationRow = new PaymentInformationRow
            {
                SysPaymentTypeId = (int)sysPaymentType,
                PaymentNr = paymentNr,
                Default = isDefault,
                BIC = BIC,
                BankConnected = bankConnected
            };
            SetCreatedProperties(paymentInformationRow);
            paymentInformation.PaymentInformationRow.Add(paymentInformationRow);

            return paymentInformationRow;
        }

        public string GetBicFromIban(string iban)
        {
            string bic = string.Empty;

            // try find bic using one number
            int code = 0;
            int.TryParse(iban.Substring(4, 1), out code);

            switch (code)
            {
                case 1:
                case 2:
                    bic = "NDEAFIHH";
                    break;
                case 5:
                    bic = "OKOYFIHH";
                    break;
                case 6:
                    bic = "AABAFI22";
                    break;
                case 8:
                    bic = "DABAFIHH";
                    break;
            }

            if (bic.HasValue())
                return bic;

            // try find bic using two numbers
            int.TryParse(iban.Substring(4, 2), out code);
            switch (code)
            {
                case 31:
                    bic = "HANDFIHH";
                    break;
                case 33:
                    bic = "ESSEFIHX";
                    break;
                case 34:
                    bic = "DABAFIHH";
                    break;
                case 36:
                case 39:
                    bic = "SBANFIHH";
                    break;
                case 37:
                    bic = "DNBAFIHX";
                    break;
                case 38:
                    bic = "SWEDFIHH";
                    break;
            }

            if (bic.HasValue())
                return bic;

            // try find bic using three numbers
            int.TryParse(iban.Substring(4, 3), out code);
            switch (code)
            {
                case 400:
                case 402:
                case 403:
                case 715:
                    bic = "ITELFIHH";
                    break;
                case 405:
                case 497:
                    bic = "HELSFIHH";
                    break;
                case 717:
                    bic = "BIGKFIH1";
                    break;
                case 713:
                    bic = "CITIFIHX";
                    break;
                case 799:
                    bic = "HOLVFIHH";
                    break;
            }

            if (bic.HasValue())
                return bic;

            if (code >= 470 && code <= 478)
                bic = "POPFFI22";
            else if ((code >= 406 && code <= 408) || (code >= 410 && code <= 412) || (code >= 414 && code <= 421) || (code >= 423 && code <= 432) ||
     (code >= 435 && code <= 452) || (code >= 454 && code <= 464) || (code >= 483 && code <= 493) || (code >= 495 && code <= 496))
                bic = "ITELFIHH";


            return bic;
        }

        #endregion

        #region PaymentInformationRow

        public List<PaymentInformationRow> GetPaymentInformationRowsForActor(int actorId, TermGroup_SysPaymentType sysPaymentType = TermGroup_SysPaymentType.Unknown)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.PaymentInformationRow.NoTracking();
            return GetPaymentInformationRowsForActor(entities, actorId, sysPaymentType);
        }

        public List<PaymentInformationRow> GetPaymentInformationRowsForActor(CompEntities entities, int actorId, TermGroup_SysPaymentType sysPaymentType = TermGroup_SysPaymentType.Unknown)
        {
            IQueryable<PaymentInformationRow> query = (from pir in entities.PaymentInformationRow
                                                       where
                                                            (pir.PaymentInformation.Actor.ActorId == actorId) &&
                                                            (pir.PaymentInformation.State == (int)SoeEntityState.Active) &&
                                                            (pir.State != (int)SoeEntityState.Deleted)
                                                       select pir);
            if (sysPaymentType != TermGroup_SysPaymentType.Unknown)
            {
                query = query.Where(p => p.SysPaymentTypeId == (int)sysPaymentType);
            }

            return query.ToList();
        }

        public PaymentInformationRow GetPaymentInformationRowForActor(CompEntities entities, int actorId, string paymentNr)
        {
            return (from pir in entities.PaymentInformationRow
                    where ((pir.PaymentInformation.Actor.ActorId == actorId) &&
                    (pir.PaymentNr == paymentNr) &&
                    (pir.State != (int)SoeEntityState.Deleted))
                    select pir).FirstOrDefault();
        }

        public PaymentInformationRow GetPaymentInformationRow(CompEntities entities, int paymentInformationRowId)
        {
            return (from pir in entities.PaymentInformationRow
                    .Include("PaymentInformation")
                    where ((pir.PaymentInformationRowId == paymentInformationRowId) &&
                    (pir.State != (int)SoeEntityState.Deleted))
                    select pir).FirstOrDefault<PaymentInformationRow>();
        }
        public PaymentInformationRow GetPaymentInformationRow(CompEntities entities, int paymentInformationRowId, int actorCompanyId)
        {
            return (from pir in entities.PaymentInformationRow
                    .Include("PaymentInformation")
                    where ((pir.PaymentInformationRowId == paymentInformationRowId) &&
                    (pir.PaymentInformation.Actor.ActorId == actorCompanyId) &&
                    (pir.State != (int)SoeEntityState.Deleted))
                    select pir).FirstOrDefault<PaymentInformationRow>();
        }

        public bool IsPaymentInformationRowUsedByPaymentMethod(CompEntities entities, int paymentInformationRowId, int actorCompanyId)
        {
            int counter = (from pm in entities.PaymentMethod
                           where pm.PaymentInformationRow.PaymentInformationRowId == paymentInformationRowId && pm.Company.ActorCompanyId == actorCompanyId &&
                           pm.State == (int)SoeEntityState.Active
                           select pm).Count();

            if (counter > 0)
                return true;
            return false;
        }

        #endregion

        #region PaymentInformationView

        public List<PaymentInformationViewDTO> GetPaymentInformationViews(int actorId, bool setBic = false)
        {
            List<PaymentInformationViewDTO> dtos = new List<PaymentInformationViewDTO>();
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var langId = base.GetLangId();
            var paymentTypes = base.GetTermGroupDict(TermGroup.SysPaymentType, langId);

            var piws = (from piw in entitiesReadOnly.PaymentInformationView
                        where piw.ActorId == actorId
                        select piw).ToList().OrderByDescending(p => p.Default).ThenBy(p => p.DefaultSysPaymentTypeId).ThenBy(p => p.PaymentNr);

            foreach (var paymentInformationView in piws)
            {
                var dto = paymentInformationView.ToDTO(setBic);
                dto.Name = dto.SysPaymentTypeId != 0 ? paymentTypes[dto.SysPaymentTypeId] : "";

                dtos.Add(dto);
            }

            return dtos;
        }

        public Dictionary<int, string> GetPaymentInformationViewsDict(int actorId, bool addEmptyRow)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();

            if (addEmptyRow)
                dict.Add(0, " ");

            var items = GetPaymentInformationViews(actorId, true);
            foreach (var item in items)
            {
                dict.Add(item.PaymentInformationRowId, item.Name + " " + item.PaymentNrDisplay);
            }

            return dict;
        }

        public List<PaymentInformationViewDTOSmall> GetPaymentInformationViewsSmall(int actorId, bool addEmptyRow)
        {
            var list = new List<PaymentInformationViewDTOSmall>();

            if (addEmptyRow)
                list.Add(new PaymentInformationViewDTOSmall() { Id = 0, Name = " ", CurrencyCode = " " });

            var currencies = CountryCurrencyManager.GetCurrenciesWithSysCurrency(actorId);
            var items = GetPaymentInformationViews(actorId, true);
            foreach (var item in items)
            {
                var dto = new PaymentInformationViewDTOSmall() { Id = item.PaymentInformationRowId, Name = item.Name + " " + item.PaymentNrDisplay };

                if (item.CurrencyId.HasValue)
                {
                    var currency = currencies.FirstOrDefault(c => c.CurrencyId == item.CurrencyId.Value);
                    if (currency != null)
                        dto.CurrencyCode = currency.Code;
                }
                list.Add(dto);
            }

            return list;
        }

        // Show only the ones not used 
        public Dictionary<int, string> GetPaymentInformationViewsDict(int actorId, TermGroup_Country sysCountryId, bool addEmptyRow)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();

            if (addEmptyRow)
                dict.Add(0, " ");

            var items = GetPaymentInformationViews(actorId);
            foreach (var item in items)
            {
                if (sysCountryId == TermGroup_Country.FI)
                {  // Let's drop out those specific for Sweden 
                    if (item.Name.IndexOf("Ei käytössä") == -1)
                    {
                        dict.Add(item.PaymentInformationRowId, item.Name + " " + item.PaymentNr);
                    }

                }
                else
                {
                    dict.Add(item.PaymentInformationRowId, item.Name + " " + item.PaymentNr);
                }
            }

            return dict;
        }

        public PaymentInformationViewDTO GetPaymentInformationView(int paymentInformationRowId, int actorId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetPaymentInformationView(entities, paymentInformationRowId, actorId);
        }

        public PaymentInformationViewDTO GetPaymentInformationView(CompEntities entities, int paymentInformationRowId, int actorId)
        {
            var langId = base.GetLangId();
            var paymentTypes = base.GetTermGroupDict(TermGroup.SysPaymentType, langId);

            var paymentInformationView = (from piw in entities.PaymentInformationView
                                          where ((piw.PaymentInformationRowId == paymentInformationRowId) &&
                                          (piw.ActorId == actorId))
                                          select piw).FirstOrDefault();

            var dto = paymentInformationView.ToDTO();
            dto.Name = dto.SysPaymentTypeId != 0 ? paymentTypes[dto.SysPaymentTypeId] : "";

            return dto;
        }

        #endregion

        #region PaymentCondition

        public List<PaymentCondition> GetPaymentConditions(int actorCompanyId, int? paymentConditionId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.PaymentCondition.NoTracking();
            return GetPaymentConditions(entities, actorCompanyId, paymentConditionId);
        }

        public List<PaymentCondition> GetPaymentConditions(CompEntities entities, int actorCompanyId, int? paymentConditionId = null)
        {
            var query = (from p in entities.PaymentCondition
                         where p.Company.ActorCompanyId == actorCompanyId
                         select p);

            if (paymentConditionId.HasValue)
                query = query.Where(p => p.PaymentConditionId == paymentConditionId.Value);

            return query.OrderBy(p => p.Code).ToList();
        }

        public Dictionary<int, string> GetPaymentConditionsDict(int actorCompanyId, bool addEmptyRow)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();

            if (addEmptyRow)
                dict.Add(0, " ");

            List<PaymentCondition> paymentConditions = GetPaymentConditions(actorCompanyId);
            foreach (PaymentCondition paymentCondition in paymentConditions.OrderBy(p => p.Days))
            {
                dict.Add(paymentCondition.PaymentConditionId, paymentCondition.Name);
            }

            return dict;
        }

        public PaymentCondition GetPaymentCondition(int paymentConditionId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.PaymentCondition.NoTracking();
            return GetPaymentCondition(entities, paymentConditionId, actorCompanyId);
        }

        public PaymentCondition GetPaymentCondition(CompEntities entities, int paymentConditionId, int actorCompanyId)
        {
            return (from pc in entities.PaymentCondition
                    where pc.PaymentConditionId == paymentConditionId &&
                    pc.Company.ActorCompanyId == actorCompanyId
                    select pc).FirstOrDefault();
        }

        public string GetPaymentConditionName(CompEntities entities, int paymentConditionId, int actorCompanyId)
        {
            return (from pc in entities.PaymentCondition
                    where pc.PaymentConditionId == paymentConditionId &&
                    pc.Company.ActorCompanyId == actorCompanyId
                    select pc.Name).FirstOrDefault();
        }

        public int? GetPaymentConditionId(CompEntities entities, string code, int actorCompanyId)
        {
            if (string.IsNullOrEmpty(code))
                return null;

            return (from pc in entities.PaymentCondition
                    where pc.Code == code &&
                    pc.Company.ActorCompanyId == actorCompanyId
                    select pc.PaymentConditionId).FirstOrDefault();
        }

        public PaymentCondition GetPaymentConditionByCode(string code, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.PaymentCondition.NoTracking();
            return GetPaymentConditionByCode(entities, code, actorCompanyId);
        }

        public PaymentCondition GetPaymentConditionByCode(CompEntities entities, string code, int actorCompanyId)
        {
            return (from pc in entities.PaymentCondition
                    where pc.Code == code &&
                    pc.Company.ActorCompanyId == actorCompanyId
                    select pc).FirstOrDefault();
        }

        public PaymentCondition GetPrevNextPaymentCondition(int conditionId, int actorCompanyId, SoeFormMode mode)
        {
            List<PaymentCondition> paymentConditions = GetPaymentConditions(actorCompanyId);

            // Get index of current condition
            int i = 0;
            foreach (PaymentCondition paymentCondition in paymentConditions)
            {
                if (paymentCondition.PaymentConditionId == conditionId)
                    break;
                i++;
            }

            if (mode == SoeFormMode.Next && i < paymentConditions.Count - 1)
                i++;
            else if (mode == SoeFormMode.Prev && i > 0)
                i--;

            return paymentConditions.ElementAt(i);
        }

        public int GetPaymentConditionDays(int paymentConditionId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetPaymentConditionDays(entities, paymentConditionId, actorCompanyId);
        }

        public int GetPaymentConditionDays(CompEntities entities, int paymentConditionId, int actorCompanyId)
        {
            return GetPaymentCondition(entities, paymentConditionId, actorCompanyId)?.Days ?? 0;
        }

        public bool PaymentConditionExists(string code, int actorCompanyId, int? paymentConditionId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.PaymentCondition.NoTracking();
            var query = (from p in entities.PaymentCondition
                         where p.Company.ActorCompanyId == actorCompanyId &&
                         p.Code == code
                         select p);
            if (paymentConditionId.HasValue && paymentConditionId.Value > 0)
                query = query.Where(pc => pc.PaymentConditionId != paymentConditionId.Value);

            return query.Any();
        }

        public ActionResult AddPaymentCondition(PaymentCondition condition, int actorCompanyId)
        {
            if (condition == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "PaymentCondition");

            using (CompEntities entities = new CompEntities())
            {
                // Get company
                condition.Company = CompanyManager.GetCompany(entities, actorCompanyId);
                if (condition.Company == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                return AddEntityItem(entities, condition, "PaymentCondition");
            }
        }

        public ActionResult SavePaymentCondition(PaymentConditionDTO conditionInput, int actorCompanyId)
        {
            ActionResult result = new ActionResult();

            if (conditionInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "PaymentConditionDTO");

            using (CompEntities entities = new CompEntities())
            {
                Company company = CompanyManager.GetCompany(entities, actorCompanyId);
                // Get original condition
                PaymentCondition paymentCondition = GetPaymentCondition(entities, conditionInput.PaymentConditionId, actorCompanyId);

                if (PaymentConditionExists(conditionInput.Code, actorCompanyId, paymentCondition?.PaymentConditionId ?? null))
                    return new ActionResult((int)ActionResultSave.EntityExists, GetText(92025, "Kod existerar redan"));

                if (paymentCondition == null)
                {
                    paymentCondition = new PaymentCondition()
                    {
                        Company = company,
                    };

                    SetCreatedProperties(paymentCondition);
                    entities.PaymentCondition.AddObject(paymentCondition);
                }
                else
                {
                    SetModifiedProperties(paymentCondition);
                }

                // Modify it
                paymentCondition.Code = conditionInput.Code;
                paymentCondition.Name = conditionInput.Name;
                paymentCondition.Days = conditionInput.Days;
                paymentCondition.DiscountDays = conditionInput.DiscountDays;
                paymentCondition.DiscountPercent = conditionInput.DiscountPercent;
                paymentCondition.StartOfNextMonth = conditionInput.StartOfNextMonth;

                result = SaveChanges(entities);
                if (!result.Success)
                    return result;

                result.IntegerValue = paymentCondition.PaymentConditionId;
            }

            return result;
        }

        public ActionResult UpdatePaymentCondition(PaymentCondition paymentCondition, int actorCompanyId)
        {
            if (paymentCondition == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "PaymentCondition");

            using (CompEntities entities = new CompEntities())
            {
                // Get original condition
                PaymentCondition originalPaymentCondition = GetPaymentCondition(entities, paymentCondition.PaymentConditionId, actorCompanyId);
                if (originalPaymentCondition == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "PaymentCondition");

                // Modify it
                originalPaymentCondition.Code = paymentCondition.Code;
                originalPaymentCondition.Name = paymentCondition.Name;
                originalPaymentCondition.Days = paymentCondition.Days;
                originalPaymentCondition.DiscountDays = paymentCondition.DiscountDays;
                originalPaymentCondition.DiscountPercent = paymentCondition.DiscountPercent;
                originalPaymentCondition.StartOfNextMonth = paymentCondition.StartOfNextMonth;
                // Save it
                return SaveEntityItem(entities, originalPaymentCondition);
            }
        }

        public ActionResult DeletePaymentCondition(PaymentCondition paymentCondition, int actorCompanyId)
        {
            if (paymentCondition == null)
                return new ActionResult((int)ActionResultDelete.EntityIsNull, "PaymentCondition");

            using (CompEntities entities = new CompEntities())
            {
                PaymentCondition orginalPaymentCondition = GetPaymentCondition(entities, paymentCondition.PaymentConditionId, actorCompanyId);
                if (orginalPaymentCondition == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "PaymentCondition");

                return DeleteEntityItem(entities, orginalPaymentCondition);
            }
        }

        public ActionResult DeletePaymentCondition(int paymentConditionId, int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                PaymentCondition orginalCondition = GetPaymentCondition(entities, paymentConditionId, actorCompanyId);
                if (orginalCondition == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "PaymentCondition");

                var result = DeleteEntityItem(entities, orginalCondition);
                if (!result.Success && result.ErrorNumber == (int)ActionResultDelete.EntityInUse)
                {
                    result.ErrorMessage = GetText(7499, "Betalningsvillkoret används och kan inte tas bort");
                }
                return result;
            }
        }

        #endregion

        #region PaymentMethod
        public List<PaymentMethod> GetPaymentMethods(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.PaymentMethod.NoTracking();
            return GetPaymentMethods(entities, actorCompanyId);
        }
        public List<PaymentMethod> GetPaymentMethods(CompEntities entities, int actorCompanyId)
        {
            return (from pm in entities.PaymentMethod
                    where (pm.Company.ActorCompanyId == actorCompanyId) &&
                          (pm.State == (int)SoeEntityState.Active)
                    orderby pm.Name, pm.SysPaymentMethodId
                    select pm).ToList();
        }

        public List<PaymentMethod> GetPaymentMethods(SoeOriginType paymentType, int actorCompanyId, bool addEmptyRow = false, bool onlyCashSales = false, bool includeAccount = false, int? paymentMethodId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.PaymentMethod.NoTracking();
            return GetPaymentMethods(entities, paymentType, actorCompanyId, addEmptyRow, onlyCashSales, includeAccount, paymentMethodId);
        }

        public List<PaymentMethod> GetPaymentMethods(CompEntities entities, SoeOriginType paymentType, int actorCompanyId, bool addEmptyRow = false, bool onlyCashSales = false, bool includeAccount = false, int? paymentMethodId = null)
        {
            var currencies = CountryCurrencyManager.GetCurrenciesWithSysCurrency(entities, actorCompanyId);
            List<SysPaymentMethod> sysPaymentMethods = GetSysPaymentMethods();

            List<PaymentMethod> paymentMethods = !includeAccount ? (from pm in entities.PaymentMethod
                                                                    .Include("PaymentInformationRow")
                                                                    where ((pm.Company.ActorCompanyId == actorCompanyId) &&
                                                                    (pm.PaymentType == (int)paymentType) &&
                                                                    (pm.State == (int)SoeEntityState.Active))
                                                                    orderby pm.Name, pm.SysPaymentMethodId
                                                                    select pm).ToList() :
                                                                   (from pm in entities.PaymentMethod
                                                                    .Include("PaymentInformationRow")
                                                                    .Include("AccountStd.Account")
                                                                    where ((pm.Company.ActorCompanyId == actorCompanyId) &&
                                                                    (pm.PaymentType == (int)paymentType) &&
                                                                    (pm.State == (int)SoeEntityState.Active))
                                                                    orderby pm.Name, pm.SysPaymentMethodId
                                                                    select pm).ToList();
            if (paymentMethodId.HasValue)
            {
                paymentMethods = paymentMethods.Where(m => m.PaymentMethodId == paymentMethodId).ToList();
            }

            if (onlyCashSales)
                paymentMethods = paymentMethods.Where(m => m.UseInCashSales).ToList();

            foreach (PaymentMethod paymentMethod in paymentMethods)
            {
                if (paymentMethod.PaymentInformationRow != null)
                {
                    paymentMethod.PaymentNr = paymentMethod.PaymentInformationRow.PaymentNr;
                    paymentMethod.SysPaymentTypeId = paymentMethod.PaymentInformationRow.SysPaymentTypeId;
                    paymentMethod.PayerBankId = paymentMethod.PaymentInformationRow.PayerBankId;
                }

                if (paymentMethod.SysPaymentMethodId > 0)
                {
                    SysPaymentMethod sysPaymentMethod = sysPaymentMethods.FirstOrDefault(spm => spm.SysPaymentMethodId == paymentMethod.SysPaymentMethodId);
                    if (sysPaymentMethod != null)
                        paymentMethod.SysPaymentMethodName = sysPaymentMethod.Name;
                }

                if (paymentMethod.PaymentInformationRow != null && paymentMethod.PaymentInformationRow.CurrencyId.HasValue)
                {
                    var currency = currencies.FirstOrDefault(c => c.CurrencyId == paymentMethod.PaymentInformationRow.CurrencyId.Value);
                    if (currency != null)
                        paymentMethod.PaymentInformationRow.CurrencyCode = currency.Code;
                }
            }

            if (addEmptyRow)
                return paymentMethods.PrependElement(new PaymentMethod() { PaymentMethodId = 0, Name = " " }).ToList();

            return paymentMethods;
        }

        public Dictionary<int, string> GetPaymentMethodsDict(SoeOriginType paymentType, int actorCompanyId, bool addEmptyRow = false)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            if (addEmptyRow)
                dict.Add(0, " ");

            List<PaymentMethod> paymentMethods = GetPaymentMethods(paymentType, actorCompanyId);
            foreach (PaymentMethod paymentMethod in paymentMethods)
            {
                dict.Add(paymentMethod.PaymentMethodId, paymentMethod.Name);
            }

            return dict;
        }

        public Dictionary<int, string> GetPaymentMethodsDict(
            int[] paymentTypes, int actorCompanyId, bool addEmptyRow = false)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.PaymentMethod.NoTracking();

            Dictionary<int, string> dict = new Dictionary<int, string>();
            if (addEmptyRow)
                dict.Add(0, " ");

            List<SysPaymentMethod> sysPaymentMethods = GetSysPaymentMethods();

            entitiesReadOnly.PaymentMethod.NoTracking();
            List<PaymentMethod> paymentMethods = (from pm in entitiesReadOnly.PaymentMethod
                                                    .Include("PaymentInformationRow")
                                                  where pm.Company.ActorCompanyId == actorCompanyId &&
                                                  paymentTypes.Contains(pm.PaymentType) &&
                                                  pm.State == (int)SoeEntityState.Active
                                                  orderby pm.Name, pm.SysPaymentMethodId
                                                  select pm).ToList();

            foreach (PaymentMethod paymentMethod in paymentMethods)
            {
                if (paymentMethod.PaymentInformationRow != null)
                {
                    paymentMethod.PaymentNr = paymentMethod.PaymentInformationRow.PaymentNr;
                    paymentMethod.SysPaymentTypeId = paymentMethod.PaymentInformationRow.SysPaymentTypeId;
                }

                if (paymentMethod.SysPaymentMethodId > 0)
                {
                    SysPaymentMethod sysPaymentMethod = sysPaymentMethods.FirstOrDefault(spm => spm.SysPaymentMethodId == paymentMethod.SysPaymentMethodId);
                    if (sysPaymentMethod != null)
                        paymentMethod.SysPaymentMethodName = sysPaymentMethod.Name;
                }
            }

            foreach (PaymentMethod paymentMethod in paymentMethods)
            {
                dict.Add(paymentMethod.PaymentMethodId, paymentMethod.Name);
            }

            return dict;
        }

        public List<PaymentMethod> GetPaymentMethodsForImport(int paymentType, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.PaymentMethod.NoTracking();
            List<PaymentMethod> paymentMethods = (from pm in entities.PaymentMethod
                                                  where ((pm.Company.ActorCompanyId == actorCompanyId) &&
                                                  (pm.PaymentType == paymentType) &&
                                                  (pm.State == (int)SoeEntityState.Active))
                                                  orderby pm.Name, pm.SysPaymentMethodId
                                                  select pm).ToList();

            if (paymentType == (int)SoeOriginType.CustomerPayment)
            {
                return paymentMethods.Where(p => CustomerSysPaymentMetodsForImport.Contains((TermGroup_SysPaymentMethod)p.SysPaymentMethodId)).ToList();
            }
            if (paymentType == (int)SoeOriginType.SupplierPayment)
            {
                return paymentMethods.Where(p => SupplierSysPaymentMetodsForImport.Contains((TermGroup_SysPaymentMethod)p.SysPaymentMethodId)).ToList();
            }
            else
            {
                return new List<PaymentMethod>();
            }
        }

        public PaymentMethod GetPaymentMethod(int paymentMethodId, int actorCompanyId, bool loadAccount = false)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.PaymentMethod.NoTracking();
            PaymentMethod paymentMethod = GetPaymentMethod(entitiesReadOnly, paymentMethodId, actorCompanyId, loadAccount);
            if (paymentMethod != null && paymentMethod.SysPaymentMethodId > 0)
            {
                List<SysPaymentMethod> sysPaymentMethods = GetSysPaymentMethods();
                SysPaymentMethod sysPaymentMethod = sysPaymentMethods.FirstOrDefault(spm => spm.SysPaymentMethodId == paymentMethod.SysPaymentMethodId);
                if (sysPaymentMethod != null)
                    paymentMethod.SysPaymentMethodName = sysPaymentMethod.Name;

                if (paymentMethod.PaymentInformationRow != null)
                {
                    paymentMethod.PaymentNr = paymentMethod.PaymentInformationRow.PaymentNr;
                    paymentMethod.SysPaymentTypeId = paymentMethod.PaymentInformationRow.SysPaymentTypeId;
                    paymentMethod.PayerBankId = paymentMethod.PaymentInformationRow.PayerBankId;
                }
            }
            return paymentMethod;
        }

        public PaymentMethod GetPaymentMethod(CompEntities entities, int paymentMethodId, int actorCompanyId, bool loadAccount = false)
        {
            if (loadAccount)
            {
                return (from pm in entities.PaymentMethod
                            .Include("PaymentInformationRow")
                            .Include("AccountStd.Account")
                        where ((pm.PaymentMethodId == paymentMethodId) &&
                        (pm.Company.ActorCompanyId == actorCompanyId) &&
                        (pm.State == (int)SoeEntityState.Active))
                        select pm).FirstOrDefault();
            }
            else
            {
                return (from pm in entities.PaymentMethod
                            .Include("PaymentInformationRow")
                        where pm.PaymentMethodId == paymentMethodId &&
                        pm.Company.ActorCompanyId == actorCompanyId &&
                        pm.State == (int)SoeEntityState.Active
                        select pm).FirstOrDefault();
            }
        }

        public PaymentMethod GetPaymentMethodBySysId(CompEntities entities, int sysPaymentMethodId, int actorCompanyId)
        {
            return (from pm in entities.PaymentMethod
                        .Include("PaymentInformationRow")
                    where pm.SysPaymentMethodId == sysPaymentMethodId &&
                    pm.Company.ActorCompanyId == actorCompanyId &&
                    pm.State == (int)SoeEntityState.Active
                    select pm).FirstOrDefault();
        }

        public List<PaymentMethod> GetPaymentMethodsBySysId(CompEntities entities, TermGroup_SysPaymentMethod sysPaymentMethodId, SoeOriginType paymentType, int actorCompanyId)
        {
            return (from pm in entities.PaymentMethod
                        .Include("PaymentInformationRow")
                    where pm.SysPaymentMethodId == (int)sysPaymentMethodId &&
                        pm.Company.ActorCompanyId == actorCompanyId &&
                        pm.PaymentType == (int)paymentType &&
                        pm.State == (int)SoeEntityState.Active
                    select pm).ToList();
        }

        public PaymentMethod GetPrevNextPaymentMethod(int paymentMethodId, int actorCompanyId, SoeOriginType paymentType, SoeFormMode mode)
        {
            PaymentMethod paymentMethod = null;
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.PaymentMethod.NoTracking();

            if (mode == SoeFormMode.Next)
            {
                paymentMethod = (from pm in entitiesReadOnly.PaymentMethod
                                 where ((pm.PaymentMethodId > paymentMethodId) &&
                                 (pm.Company.ActorCompanyId == actorCompanyId) &&
                                 (pm.PaymentType == (int)paymentType) &&
                                 (pm.State == (int)SoeEntityState.Active))
                                 orderby pm.PaymentMethodId ascending
                                 select pm).FirstOrDefault();
            }
            else if (mode == SoeFormMode.Prev)
            {
                paymentMethod = (from pm in entitiesReadOnly.PaymentMethod
                                 where ((pm.PaymentMethodId < paymentMethodId) &&
                                 (pm.Company.ActorCompanyId == actorCompanyId) &&
                                 (pm.PaymentType == (int)paymentType) &&
                                 (pm.State == (int)SoeEntityState.Active))
                                 orderby pm.PaymentMethodId descending
                                 select pm).FirstOrDefault();
            }

            return paymentMethod;
        }

        public PaymentMethod GetPaymentMethodByBankAccountNr(int actorCompanyId, string BIC, string bankAccountNr, SoeOriginType paymentType, TermGroup_SysPaymentMethod sysPaymentMetod)
        {

            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.PaymentMethod.NoTracking();
            IQueryable<PaymentMethod> query = (from pm in entitiesReadOnly.PaymentMethod
                        .Include("PaymentInformationRow")
                                               where
                                                   pm.Company.ActorCompanyId == actorCompanyId &&
                                                   pm.State == (int)SoeEntityState.Active &&
                                                   pm.PaymentType == (int)paymentType &&
                                                   pm.PaymentInformationRow.BIC == BIC &&
                                                   pm.PaymentInformationRow.PaymentNr == bankAccountNr &&
                                                   pm.PaymentInformationRow.State == (int)SoeEntityState.Active
                                               select pm);

            if (sysPaymentMetod != TermGroup_SysPaymentMethod.None)
            {
                query = query.Where(p => p.SysPaymentMethodId == (int)sysPaymentMetod);
            }

            return query.FirstOrDefault();
        }

        private bool IsExportable(PaymentMethod paymentMethod)
        {
            return paymentMethod == null || IsExportPaymentType(paymentMethod.SysPaymentMethodId);
        }

        public bool IsExportPaymentType(int sysPaymentMethodId)
        {
            return !NonExportPaymentTypes.Cast<int>().Contains(sysPaymentMethodId);
        }
        public ActionResult SavePaymentMethod(PaymentMethodDTO methodInput, int actorCompanyId)
        {
            if (methodInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "PaymentMethodDTO");

            if (methodInput.PaymentType == SoeOriginType.None)
            {
                return new ActionResult("Payment type should not be empty!");
            }

            using (var entities = new CompEntities())
            {
                Company company = CompanyManager.GetCompany(entities, actorCompanyId);
                PaymentInformationRow paymentInformationRow = methodInput.PaymentInformationRowId.HasValue ? GetPaymentInformationRow(entities, (int)methodInput.PaymentInformationRowId) : null;
                AccountStd accountStd = AccountManager.GetAccountStd(entities, methodInput.AccountId, actorCompanyId, false, false);
                //PayerBankId
                if (paymentInformationRow != null && methodInput.PaymentInformationRow != null && methodInput.PaymentInformationRow.PayerBankId != null)
                {
                    paymentInformationRow.PayerBankId = methodInput.PaymentInformationRow.PayerBankId;
                }

                // Get original method
                PaymentMethod paymentMethod = GetPaymentMethod(entities, methodInput.PaymentMethodId, actorCompanyId);

                if (PaymentMethodExists(actorCompanyId, methodInput.Name, methodInput.PaymentType, paymentMethod?.PaymentMethodId ?? null))
                    return new ActionResult((int)ActionResultSave.EntityExists, GetText(92029, "Namnet finns redan."));

                if (paymentMethod == null)
                {
                    paymentMethod = new PaymentMethod()
                    {
                        Company = company,
                        PaymentInformationRow = paymentInformationRow,
                        AccountStd = accountStd,
                    };

                    SetCreatedProperties(paymentMethod);
                    entities.PaymentMethod.AddObject(paymentMethod);
                }
                else
                {
                    paymentMethod.AccountStd = accountStd;
                    paymentMethod.PaymentInformationRow = paymentInformationRow;
                    SetModifiedProperties(paymentMethod);
                }

                // Modify it
                paymentMethod.Name = methodInput.Name;
                paymentMethod.PaymentNr = methodInput.PaymentNr;
                paymentMethod.PaymentType = (int)methodInput.PaymentType;
                paymentMethod.SysPaymentMethodId = methodInput.SysPaymentMethodId;
                paymentMethod.CustomerNr = methodInput.CustomerNr;
                paymentMethod.UseInCashSales = methodInput.UseInCashSales;
                paymentMethod.UseRoundingInCashSales = methodInput.UseRoundingInCashSales;
                paymentMethod.TransactionCode = methodInput.TransactionCode;

                if (paymentInformationRow != null && paymentMethod.SysPaymentMethodId == (int)TermGroup_SysPaymentMethod.ISO20022 &&
                    (string.IsNullOrEmpty(paymentInformationRow.BIC)))
                {
                    return new ActionResult(7588, GetText(7588, "BIC/Bank måste anges för vald betalmetod"));
                }

                var result = SaveChanges(entities);
                if (!result.Success)
                    return result;

                result.IntegerValue = paymentMethod.PaymentMethodId;

                return result;
            }
        }

        private bool PaymentMethodExists(int actorCompanyId, string paymentMethodName, SoeOriginType originType, int? paymentMethodId = null)
        {
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.PaymentMethod.NoTracking();
            var query = from p in entitiesReadOnly.PaymentMethod
                        where p.Company.ActorCompanyId == actorCompanyId &&
                        p.Name == paymentMethodName &&
                        p.PaymentType == (int)originType
                        select p;

            if (paymentMethodId.HasValue)
                query = query.Where(p => p.PaymentMethodId != paymentMethodId.Value);
            return query.Any();
        }

        public ActionResult AddPaymentMethodNoTrans(PaymentMethod paymentMethod, int paymentInformationRowId, string accountNr, int actorCompanyId, SoeOriginType paymentType, string payerBankId = null)
        {
            using (var entities = new CompEntities())
            {
                if (paymentMethod == null)
                    return new ActionResult((int)ActionResultSave.EntityIsNull, "PaymentMethod");

                paymentMethod.Company = CompanyManager.GetCompany(entities, actorCompanyId);
                if (paymentMethod.Company == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                if (paymentInformationRowId > 0)
                {
                    paymentMethod.PaymentInformationRow = GetPaymentInformationRow(entities, paymentInformationRowId);
                    if (paymentMethod.PaymentInformationRow == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "PaymentInformationRow");
                }

                paymentMethod.AccountStd = AccountManager.GetAccountStdByNr(entities, accountNr, actorCompanyId);
                if (paymentMethod.AccountStd == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountStd");

                paymentMethod.PaymentType = (int)paymentType;

                entities.PaymentMethod.AddObject(paymentMethod);

                if (payerBankId != null)
                {
                    paymentMethod.PaymentInformationRow.PayerBankId = payerBankId;
                }

                var result = SaveChanges(entities);

                return result;
            }
        }

        public ActionResult AddPaymentMethod(PaymentMethod paymentMethod, int paymentInformationRowId, string accountNr, int actorCompanyId, SoeOriginType paymentType, CompEntities entities, TransactionScope transaction = null)
        {
            if (paymentMethod == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "PaymentMethod");

            paymentMethod.Company = CompanyManager.GetCompany(entities, actorCompanyId);
            if (paymentMethod.Company == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

            if (paymentInformationRowId > 0)
            {
                paymentMethod.PaymentInformationRow = GetPaymentInformationRow(entities, paymentInformationRowId);
                if (paymentMethod.PaymentInformationRow == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "PaymentInformationRow");
            }

            paymentMethod.AccountStd = AccountManager.GetAccountStdByNr(entities, accountNr, actorCompanyId);
            if (paymentMethod.AccountStd == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountStd");

            paymentMethod.PaymentType = (int)paymentType;

            var result = AddEntityItem(entities, paymentMethod, "PaymentMethod", transaction);

            return result;
        }

        public ActionResult UpdatePaymentMethodNoTrans(PaymentMethod paymentMethod, int paymentInformationRowId, string accountNr, int actorCompanyId, string payerBankId = null)
        {
            using (CompEntities entities = new CompEntities())
            {
                if (paymentMethod == null)
                    return new ActionResult((int)ActionResultSave.EntityIsNull, "PaymentMethod");

                PaymentMethod originalPaymentMethod = GetPaymentMethod(entities, paymentMethod.PaymentMethodId, actorCompanyId);
                if (originalPaymentMethod == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "PaymentMethod");

                originalPaymentMethod.PaymentInformationRow = GetPaymentInformationRow(entities, paymentInformationRowId);
                if (originalPaymentMethod.PaymentInformationRow == null && IsExportPaymentType(paymentMethod.SysPaymentMethodId))
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "PaymentInformationRow");

                originalPaymentMethod.AccountStd = AccountManager.GetAccountStdByNr(entities, accountNr, actorCompanyId);
                if (originalPaymentMethod.AccountStd == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountStd");

                if (payerBankId != null && originalPaymentMethod.PaymentInformationRow != null)
                {
                    originalPaymentMethod.PaymentInformationRow.PayerBankId = payerBankId;
                }

                return UpdateEntityItem(entities, originalPaymentMethod, paymentMethod, "PaymentMethod");
            }
        }

        public ActionResult UpdatePaymentMethod(PaymentMethod paymentMethod, int paymentInformationRowId, string accountNr, int actorCompanyId, CompEntities entities, TransactionScope transaction = null)
        {
            if (paymentMethod == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "PaymentMethod");

            PaymentMethod originalPaymentMethod = GetPaymentMethod(entities, paymentMethod.PaymentMethodId, actorCompanyId);
            if (originalPaymentMethod == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "PaymentMethod");

            originalPaymentMethod.PaymentInformationRow = GetPaymentInformationRow(entities, paymentInformationRowId);
            if (originalPaymentMethod.PaymentInformationRow == null && IsExportPaymentType(paymentMethod.SysPaymentMethodId))
                return new ActionResult((int)ActionResultSave.EntityNotFound, "PaymentInformationRow");

            originalPaymentMethod.AccountStd = AccountManager.GetAccountStdByNr(entities, accountNr, actorCompanyId);
            if (originalPaymentMethod.AccountStd == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountStd");

            if (transaction == null)
                return UpdateEntityItem(entities, originalPaymentMethod, paymentMethod, "PaymentMethod");
            else
                return UpdateEntityItem(entities, originalPaymentMethod, paymentMethod, "PaymentMethod", transaction);
        }

        public ActionResult DeletePaymentMethod(int paymentMethodId, int actorCompanyId)
        {
            var reslut = this.ValidatePaymentMethodUsage(paymentMethodId);
            if (!reslut.Success)
            {
                return reslut;
            }

            using (CompEntities entities = new CompEntities())
            {
                PaymentMethod orginalMethod = GetPaymentMethod(entities, paymentMethodId, actorCompanyId);
                if (orginalMethod == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "PaymentMethod");

                return ChangeEntityState(entities, orginalMethod, SoeEntityState.Deleted, true);
            }
        }

        public ActionResult DeletePaymentMethod(PaymentMethod paymentMethod, int actorCompanyId)
        {
            if (paymentMethod == null)
                return new ActionResult((int)ActionResultDelete.EntityIsNull, "PaymentMethod");

            using (CompEntities entities = new CompEntities())
            {
                PaymentMethod originalPaymentMethod = GetPaymentMethod(entities, paymentMethod.PaymentMethodId, actorCompanyId);
                if (originalPaymentMethod == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "PaymentMethod");

                return ChangeEntityState(entities, originalPaymentMethod, SoeEntityState.Deleted, true);
            }
        }

        public ActionResult ValidatePaymentMethodUsage(int paymentMethodId)
        {
            ActionResult result = new ActionResult();

            var supplierDefaultPaymentMethod = SettingManager.GetCompanyIntSetting(CompanySettingType.SupplierPaymentDefaultPaymentMethod);
            var supplierSettlePaymentMethod = SettingManager.GetCompanyIntSetting(CompanySettingType.SupplierPaymentSettlePaymentMethod);
            var customerDefaultPaymentMethod = SettingManager.GetCompanyIntSetting(CompanySettingType.CustomerPaymentDefaultPaymentMethod);
            var customerSettlePaymentMethod = SettingManager.GetCompanyIntSetting(CompanySettingType.CustomerPaymentSettlePaymentMethod);


            if (supplierDefaultPaymentMethod == paymentMethodId)
            {
                result = new ActionResult((int)ActionResultDelete.EntityInUse, GetText(92035, "Du kan inte ta bort denna betalmetod då den används som standard betalmetod. Gå till inställningar under leverantörsreskontra för att ändra standard betalmetod."));
            }
            else if (supplierSettlePaymentMethod == paymentMethodId)
            {
                result = new ActionResult((int)ActionResultDelete.EntityInUse, GetText(92036, "Du kan inte ta bort denna betalmetod då den används som utjämningsbetalmetod. Gå till inställningar under leverantörsreskontra för att ändra utjämningsbetalmetod."));
            }
            else if (customerDefaultPaymentMethod == paymentMethodId)
            {
                result = new ActionResult((int)ActionResultDelete.EntityInUse, GetText(92037, "Du kan inte ta bort denna betalmetod då den används som standard betalmetod. Gå till inställningar under kundreskontra för att ändra standard betalmetod."));
            }
            else if (customerSettlePaymentMethod == paymentMethodId)
            {
                result = new ActionResult((int)ActionResultDelete.EntityInUse, GetText(92038, "Du kan inte ta bort denna betalmetod då den används som utjämningsbetalmetod. Gå till inställningar under kundreskontra för att ändra utjämningsbetalmetod."));
            }

            return result;
        }

        #endregion

        #region PaymentExport

        public PaymentExport GetPaymentExport(int paymentExportId)
        {
            using (CompEntities entities = new CompEntities())
            {
                return (from p in entities.PaymentExport
                        where p.PaymentExportId == paymentExportId
                        select p).FirstOrDefault();
            }
        }

        public CompanySearchResultDTO GetCompanyFromPaymentMsgId(string msgId)
        {
            using (CompEntities entities = new CompEntities())
            {
                return (from p in entities.PaymentExport
                        where p.MsgId == msgId
                        select new CompanySearchResultDTO
                        {
                            ActorCompanyId = p.Payment.Select(r => r.Origin.ActorCompanyId).FirstOrDefault(),
                            CompanyGuid = p.Payment.Select(r => r.Origin.Company.CompanyGuid).FirstOrDefault().ToString()
                        }).FirstOrDefault();
            }
        }

        public List<PaymentExport> GetPaymentExports(int actorCompanyId, int type, TermGroup_ChangeStatusGridAllItemsSelection allItemsSelection)
        {
            List<PaymentExport> paymentExports = new List<PaymentExport>();

            DateTime? selectionDate = null;
            switch (allItemsSelection)
            {
                case TermGroup_ChangeStatusGridAllItemsSelection.One_Month:
                    selectionDate = DateTime.Today.AddMonths(-1);
                    break;
                case TermGroup_ChangeStatusGridAllItemsSelection.Tree_Months:
                    selectionDate = DateTime.Today.AddMonths(-3);
                    break;
                case TermGroup_ChangeStatusGridAllItemsSelection.Six_Months:
                    selectionDate = DateTime.Today.AddMonths(-6);
                    break;
                case TermGroup_ChangeStatusGridAllItemsSelection.Twelve_Months:
                    selectionDate = DateTime.Today.AddYears(-1);
                    break;
                case TermGroup_ChangeStatusGridAllItemsSelection.TwentyFour_Months:
                    selectionDate = DateTime.Today.AddYears(-2);
                    break;
            }

            int baseSysCurrencyId = CountryCurrencyManager.GetCompanyBaseSysCurrencyId(actorCompanyId);

            List<Payment> payments = GetPaymentsByCompany(actorCompanyId, selectionDate);
            foreach (Payment payment in payments)
            {
                if (payment.PaymentExport == null || payment.PaymentExport.Type != type)
                    continue;

                //Add PaymentRows from Payment
                if (payment.PaymentRow == null)
                    payment.PaymentExport.PaymentRows = new List<PaymentRow>();
                else
                    payment.PaymentExport.PaymentRows = payment.PaymentRow.Where(i => i.IsSuggestion == false).ToList();

                //Set Foreign
                PaymentRow firstPaymentRow = payment.PaymentExport.PaymentRows.FirstOrDefault(i => i.InvoiceId.HasValue);
                if (firstPaymentRow != null)
                    payment.PaymentExport.Foreign = InvoiceManager.IsInvoiceForeign(firstPaymentRow.InvoiceId.Value, baseSysCurrencyId);

                //Set status
                foreach (PaymentRow paymentRow in payment.PaymentExport.PaymentRows)
                {
                    paymentRow.StatusName = GetText(paymentRow.Status, (int)TermGroup.PaymentStatus);
                }

                if (payment.PaymentExport.State == (int)SoeEntityState.Deleted)
                {
                    //Whole file cancelled
                    payment.PaymentExport.CancelledState = (int)SoePaymentExportCancelledStates.Cancelled;
                }
                else if (payment.PaymentExport.State == (int)SoeEntityState.Active)
                {
                    //Check if some rows are cancelled
                    int nrOfCancelledPaymentRows = payment.PaymentExport.PaymentRows.Count(i => i.Status == (int)SoePaymentStatus.Cancel);
                    if (nrOfCancelledPaymentRows > 0)
                    {
                        if (nrOfCancelledPaymentRows == payment.PaymentExport.PaymentRows.Count)
                            payment.PaymentExport.CancelledState = (int)SoePaymentExportCancelledStates.Cancelled;
                        else
                            payment.PaymentExport.CancelledState = (int)SoePaymentExportCancelledStates.PartyCancelled;
                    }
                }

                paymentExports.Add(payment.PaymentExport);
            }

            return paymentExports.OrderByDescending(p => p.ExportDate).ToList();
        }

        #endregion

        #region PaymentImport

        public PaymentImport GetPaymentImport(int PaymentImportId, int ActorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.PaymentImport.NoTracking();
            return GetPaymentImport(entities, PaymentImportId, ActorCompanyId);
        }

        public PaymentImport GetPaymentImport(CompEntities entities, int PaymentImportId, int ActorCompanyId)
        {
            return (from p in entities.PaymentImport
                    where p.PaymentImportId == PaymentImportId &&
                    p.ActorCompanyId == ActorCompanyId
                    select p).FirstOrDefault();
        }

        public PaymentImportIO GetPaymentImportIO(CompEntities entities, int PaymentImportIOId, int ActorCompanyId)
        {
            return (from p in entities.PaymentImportIO
                    where p.PaymentImportIOId == PaymentImportIOId &&
                    p.ActorCompanyId == ActorCompanyId
                    select p).FirstOrDefault();
        }

        public ActionResult StartPaymentImport(TermGroup_SysPaymentMethod paymentIOType, int paymentMethodId, List<byte[]> contents, string fileName, int actorCompanyId, int userId, int batchId, int paymentImportId, ImportPaymentType importType)
        {
            PaymentIOManager pmIO = new PaymentIOManager(parameterObject);
            Stream stream = new MemoryStream(contents[0], true);

            return pmIO.Import(paymentIOType, paymentMethodId, stream, fileName, actorCompanyId, userId, batchId, paymentImportId, importType);
        }

        public ActionResult SavePaymentImportHeader(int actorCompanyId, PaymentImportDTO importDTO)
        {
            using (var entities = new CompEntities())
            {
                return SavePaymentImportHeader(entities, actorCompanyId, importDTO);
            }
        }

        public ActionResult SavePaymentImportHeader(CompEntities entities, int actorCompanyId, PaymentImportDTO importDTO)
        {
            ActionResult result;

            PaymentImport paymentImport = importDTO.PaymentImportId > 0 ? entities.PaymentImport.Where(x => x.PaymentImportId == importDTO.PaymentImportId && x.ActorCompanyId == actorCompanyId).FirstOrDefault() : null;

            if (paymentImport == null)
            {
                int NextBatchNr = GetNextBatchNrFromPaymentImport(actorCompanyId, importDTO.ImportType);
                // Get company
                paymentImport = new PaymentImport
                {
                    ActorCompanyId = actorCompanyId,
                    ImportDate = importDTO.ImportDate,
                    Filename = importDTO.Filename,
                    BatchId = NextBatchNr,
                    SysPaymentTypeId = (int)importDTO.SysPaymentTypeId,
                    Type = importDTO.Type,
                    TotalAmount = 0,
                    NumberOfPayments = 0,
                    State = 0,
                    ImportType = (int?)importDTO.ImportType,
                    PaymentLabel = importDTO.PaymentLabel,
                    TransferStatus = importDTO.TransferStatus.GetValueOrDefault(),
                    TransferMsg = importDTO.TransferStatus.GetValueOrDefault() > 0 && !string.IsNullOrEmpty(importDTO.StatusName) ? importDTO.StatusName : null
                };

                SetCreatedProperties(paymentImport);
                result = AddEntityItem(entities, paymentImport, "PaymentImport");
                if (result.Success)
                {
                    result.Value = NextBatchNr;
                    result.IntegerValue = paymentImport.PaymentImportId;
                }
                result.Value2 = paymentImport.PaymentImportId;
            }
            else
            {
                if (paymentImport.TransferStatus == (int)TermGroup_PaymentTransferStatus.SoftoneError && paymentImport.Type == 0 && importDTO.Type != 0)
                {
                    paymentImport.SysPaymentTypeId = (int)importDTO.SysPaymentTypeId;
                    paymentImport.Type = importDTO.Type;
                    paymentImport.TransferStatus = (int)TermGroup_PaymentTransferStatus.Transfered;
                    paymentImport.TransferMsg = null;
                }
                paymentImport.PaymentLabel = importDTO.PaymentLabel;
                SetModifiedProperties(paymentImport);
                result = SaveChanges(entities);
                if (result.Success)
                {
                    result.Value = paymentImport.BatchId;
                    result.IntegerValue = paymentImport.PaymentImportId;
                }
                result.Value2 = paymentImport.PaymentImportId;
            }

            return result;
        }

        public int GetNextBatchNrFromPaymentImport(int actorCompanyId, ImportPaymentType importType)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.PaymentImport.NoTracking();
            int batchId = (from h in entities.PaymentImport
                           where h.ActorCompanyId == actorCompanyId && h.ImportType == (int)importType
                           orderby h.BatchId descending
                           select h.BatchId).FirstOrDefault();

            return batchId + 1;
        }

        public ActionResult UpdatePaymentImportHead(CompEntities entities, PaymentImport paymentImport, int paymentImportId, int actorCompanyId)
        {
            if (paymentImport == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "PaymentImport");

            PaymentImport originalPaymentImport = GetPaymentImport(entities, paymentImportId, actorCompanyId);
            if (originalPaymentImport == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "PaymentImport");

            return UpdateEntityItem(entities, originalPaymentImport, paymentImport, "PaymentImport");
        }

        public ActionResult UpdatePaymentImportIO(CompEntities entities, PaymentImportIO paymentImportIO, int paymentImportIOId, int actorCompanyId)
        {
            if (paymentImportIO == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "PaymentImportIO");

            PaymentImportIO originalPaymentImportIO = GetPaymentImportIO(entities, paymentImportIOId, actorCompanyId);
            if (originalPaymentImportIO == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "PaymentImportIO");

            return UpdateEntityItem(entities, originalPaymentImportIO, paymentImportIO, "PaymentImportIO");
        }

        public ActionResult DeletePaymentImportHead(int paymentImportId, int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                PaymentImport paymentImport = GetPaymentImport(entities, paymentImportId, actorCompanyId);
                if (paymentImport == null)
                    return new ActionResult((int)ActionResultSave.EntityIsNull, "PaymentImport");

                PaymentImport originalPaymentImport = GetPaymentImport(entities, paymentImportId, actorCompanyId);
                if (originalPaymentImport == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "PaymentImport");

                paymentImport.State = (int)SoeEntityState.Deleted;

                return UpdateEntityItem(entities, originalPaymentImport, paymentImport, "PaymentImport");
            }
        }

        public ActionResult UpdatePaymentImportIOStatus(List<PaymentImportIODTO> items)
        {
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        foreach (var item in items)
                        {
                            var paymentImportIO = GetPaymentImportIO(entities, item.PaymentImportIOId, ActorCompanyId);
                            if (paymentImportIO == null)
                                return new ActionResult((int)ActionResultSave.EntityNotFound, "PaymentImportIO");

                            paymentImportIO.Comment = item.Comment;
                            paymentImportIO.Status = (int)ImportPaymentIOStatus.ManuallyHandled;

                            SetModifiedProperties(paymentImportIO);
                        }

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
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            return result;
        }

        public ActionResult SavePaymentImportIOs(List<PaymentImportIODTO> items)
        {
            var result = new ActionResult();
            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        foreach (var item in items)
                        {
                            var updateResult = InvoiceManager.UpdatePaymentImport(entities, item, true);
                        }

                        result = SaveChanges(entities, transaction);

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

        #endregion

        #region Payment Notifications

        public ActionResult SendPaymentNotification(int actorCompanyId, int paymentMethodId, string paymentPageUrl, SoeOriginStatusClassification classification)
        {
            //Check permission
            bool hasPermission = FeatureManager.HasRolePermission(Feature.Economy_Supplier_Payment_Send_Notification, Permission.Modify, RoleId, ActorCompanyId);
            if (!hasPermission)
                return new ActionResult(false, (int)ActionResultSave.NotSupported, "No Permission");

            // Get payment method
            PaymentMethod paymentMethod = GetPaymentMethod(paymentMethodId, actorCompanyId);
            if (paymentMethod == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(12184, "Inbetalningsmetod ej valt"));

            // Check whether the payment suggestion tab is visible
            bool isPaymentSuggestionTabVisible = SettingManager.GetCompanyBoolSetting(CompanySettingType.SupplierUsePaymentSuggestions);

            // Get recipient group from settings
            int recipientGroupId = SettingManager.GetCompanyIntSetting(CompanySettingType.SupplierPaymentNotificationRecipientGroup);

            // Get recipient group
            MessageGroup group = CommunicationManager.GetMessageGroup(recipientGroupId, true);
            if (group == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(12185, "Inställning för mottagargrupp saknas"));

            // Get recipient group users & Sender
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            List<User> recipientUsers = CommunicationManager.GetValidUsersForMessageGroups(entitiesReadOnly, new List<int> { recipientGroupId }, base.ActorCompanyId, 0, 0);
            User sender = UserManager.GetUser(base.UserId);

            // Get currently available payment suggestions
            List<SupplierPaymentGridDTO> paymentSuggestions = new List<SupplierPaymentGridDTO>();
            if (isPaymentSuggestionTabVisible)
            {
                paymentSuggestions = SupplierInvoiceManager.GetSupplierPaymentsForGrid(SoeOriginStatusClassification.SupplierPaymentSuggestions, TermGroup_ChangeStatusGridAllItemsSelection.All, base.ActorCompanyId);
            }

            ActionResult result = new ActionResult();
            try
            {
                string title = "";
                StringBuilder body = new StringBuilder();
                if (isPaymentSuggestionTabVisible && classification == SoeOriginStatusClassification.SupplierPaymentSuggestions && paymentSuggestions.Count > 0)
                {
                    title = string.Format(GetText(12186, "Väntar på att betalning ska skickas till banken {0}"), paymentMethod.Name);
                    body.AppendLine(GetText(12188, "Det finns betalningar i fliken betalningsförslag som väntar på att bli skickade till banken."));

                    // Add payment suggestions
                    body.AppendLine(GeneratePaymentSuggestionsListForNotification(paymentSuggestions));

                    if (!string.IsNullOrWhiteSpace(paymentPageUrl))
                        body.AppendLine(string.Format("<br/><br/><a href='{0}'>{0}</a>", paymentPageUrl));
                }
                else
                {
                    title = string.Format(GetText(12187, "Väntar på att betalning ska godkännas i banken {0}"), paymentMethod.Name);
                    body.AppendLine(string.Format(GetText(12189, "Det finns betalning i banken som väntar på att bli godkända {0}"), paymentMethod.Name));
                }

                // Setup message
                MessageEditDTO message = new MessageEditDTO()
                {
                    ActorCompanyId = actorCompanyId,
                    LicenseId = sender.LicenseId,
                    Entity = SoeEntityType.SupplierPayment,
                    Created = DateTime.Now,
                    SentDate = DateTime.Now,
                    SenderUserId = sender.UserId,
                    SenderName = sender.Name,
                    SenderEmail = sender.Email,
                    Subject = title,
                    Text = body.ToString(),
                    ShortText = body.ToString().Replace("<br/>", "\r\n"),
                    AnswerType = XEMailAnswerType.None,
                    MessagePriority = TermGroup_MessagePriority.Normal,
                    MessageType = TermGroup_MessageType.AttestInvoice,
                    MessageDeliveryType = TermGroup_MessageDeliveryType.XEmail,
                    MessageTextType = TermGroup_MessageTextType.HTML,
                };

                message.Recievers.AddRange(recipientUsers.Distinct().Select(u => new MessageRecipientDTO()
                {
                    UserId = u.UserId
                }));

                result = CommunicationManager.SendXEMail(message, actorCompanyId, RoleId, UserId);
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result.Exception = ex;
            }

            return result;
        }

        private string GeneratePaymentSuggestionsListForNotification(List<SupplierPaymentGridDTO> paymentSuggestions)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<br/><br/>");
            sb.Append("<table border='1' cellpadding='2' cellspacing='0'>");
            sb.Append("<tr>");
            sb.Append("<th style='text-align: left'>" + GetText(12190, "Fakturanummer") + "</th><th style='text-align: left'>" + GetText(12191, "Leverantör") + "</th><th style='text-align: right'>" + GetText(12192, "Att betala") + "</th><th style='text-align: right'>" + GetText(12193, "Att betala valuta") + "</th><th style='text-align: left'>" + GetText(12194, "Valuta") + "</th><th style='text-align: left'>" + GetText(12195, "Betalkonto") + "</th>");
            sb.Append("</tr>");
            decimal total = 0.0m;
            foreach (SupplierPaymentGridDTO payment in paymentSuggestions)
            {
                sb.Append("<tr>");
                sb.Append("<td>" + payment.InvoiceNr + "</td>");
                sb.Append("<td>" + payment.SupplierNr + " " + payment.SupplierName + "</td>");
                sb.Append("<td style='text-align: right'>" + payment.PaymentAmount.ToString(base.GetCulture(base.GetLangId())) + "</td>");
                sb.Append("<td style='text-align: right'>" + payment.PaymentAmountCurrency.ToString(base.GetCulture(base.GetLangId())) + "</td>");
                sb.Append("<td>" + payment.CurrencyCode + "</td>");
                sb.Append("<td>" + payment.PaymentNr + "</td>");
                sb.Append("</tr>");
                total += payment.PaymentAmount;
            }
            sb.Append("<tr><td>&nbsp;</td><td><b>" + GetText(12196, "Att betala total") + "</b></td><td style='text-align: right'><b>" + total.ToString(base.GetCulture(base.GetLangId())) + "</b></td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>");
            sb.Append("</table>");

            return sb.ToString();
        }

        #endregion

        #region SysPaymentMethod

        public TermGroup_SysPaymentMethod[] NonExportPaymentTypes = new TermGroup_SysPaymentMethod[]
        {
            TermGroup_SysPaymentMethod.Autogiro,
            TermGroup_SysPaymentMethod.Cash
        };

        /// <summary>
        /// Get all SysPaymentMethod's
        /// Accessor for SysDbCache
        /// </summary>
        /// <returns></returns>
        public List<SysPaymentMethod> GetAllSysPaymentMethods()
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return sysEntitiesReadOnly.SysPaymentMethod
                            .ToList<SysPaymentMethod>();
        }

        public List<SysPaymentMethod> GetSysPaymentMethods(bool addEmptyRow = false)
        {
            var sysPaymentMethods = new List<SysPaymentMethod>();

            List<GenericType> terms = base.GetTermGroupContent(TermGroup.PaymentMethod);

            //Uses SysDbCache
            List<SysPaymentMethod> methods = (from m in SysDbCache.Instance.SysPaymentMethods select m).ToList();

            if (addEmptyRow)
            {
                sysPaymentMethods.Add(new SysPaymentMethod()
                {
                    SysPaymentMethodId = 0,
                    SysTermId = 0,
                    Name = " ",
                });
            }

            foreach (SysPaymentMethod method in methods)
            {
                GenericType term = terms.FirstOrDefault(t => t.Id == method.SysTermId);
                if (term == null)
                    continue;

                sysPaymentMethods.Add(new SysPaymentMethod()
                {
                    SysPaymentMethodId = method.SysPaymentMethodId,
                    SysTermId = method.SysTermId,
                    Name = term.Name
                });
            }

            return sysPaymentMethods.OrderBy(m => m.Name).ToList();
        }

        public List<SysPaymentMethod> GetSysPaymentMethodsByPaymentType(SoeOriginType paymentType, bool addEmptyRow = false)
        {
            List<SysPaymentMethod> sysPaymentMethods = GetSysPaymentMethods(addEmptyRow);
            switch (paymentType)
            {
                case SoeOriginType.SupplierPayment:
                    return sysPaymentMethods.Where(x => this.GetSupplierPaymentMethodTermIds().Contains(x.SysTermId)).ToList();
                case SoeOriginType.CustomerPayment:
                    return sysPaymentMethods.Where(x => this.GetCustomerPaymentMethodTermIds().Contains(x.SysTermId)).ToList();
                default:
                    return sysPaymentMethods;
            }
        }

        private IEnumerable<int> GetCustomerPaymentMethodTermIds()
        {
            return CommonSysPaymentMetods.Concat(CustomerSysPaymentMetods).Cast<int>();
        }

        private IEnumerable<int> GetSupplierPaymentMethodTermIds()
        {
            return CommonSysPaymentMetods.Concat(SupplierSysPaymentMetods).Cast<int>();
        }

        public Dictionary<int, string> GetSysPaymentMethodsDict(SoeOriginType paymentType, TermGroup_Country countryId, bool addEmptyRow = false)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();

            if (addEmptyRow)
                dict.Add(0, " ");

            List<SysPaymentMethod> sysPaymentMethods = GetSysPaymentMethodsByPaymentType(paymentType);
            foreach (SysPaymentMethod sysPaymentMethod in sysPaymentMethods)
            {

                if (countryId == TermGroup_Country.FI)
                {  // Let's drop out those specific for Sweden 
                    if (sysPaymentMethod.Name.IndexOf("Ei käytössä") == -1)
                    {
                        dict.Add(sysPaymentMethod.SysPaymentMethodId, sysPaymentMethod.Name);
                    }

                }
                else if (countryId == TermGroup_Country.NO)
                {  // Let's drop out those specific for Sweden 
                    if (sysPaymentMethod.Name.IndexOf("Ikke brukt") == -1)
                    {
                        dict.Add(sysPaymentMethod.SysPaymentMethodId, sysPaymentMethod.Name);
                    }

                }
                else
                {   // Add all Swedish methods 
                    dict.Add(sysPaymentMethod.SysPaymentMethodId, sysPaymentMethod.Name);
                }

            }

            return dict;
        }

        public Dictionary<int, string> GetSysPaymentMethodsDict(SoeOriginType paymentType, bool addEmptyRow = false)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();

            if (addEmptyRow)
                dict.Add(0, " ");

            List<SysPaymentMethod> sysPaymentMethods = GetSysPaymentMethodsByPaymentType(paymentType);
            foreach (SysPaymentMethod sysPaymentMethod in sysPaymentMethods)
            {
                dict.Add(sysPaymentMethod.SysPaymentMethodId, sysPaymentMethod.Name);
            }

            return dict;
        }
        public Dictionary<int, string> GetSysPaymentMethodsDict(bool addEmptyRow = false)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();

            if (addEmptyRow)
                dict.Add(0, " ");

            List<SysPaymentMethod> sysPaymentMethods = GetSysPaymentMethods();
            foreach (SysPaymentMethod sysPaymentMethod in sysPaymentMethods)
            {
                dict.Add(sysPaymentMethod.SysPaymentMethodId, sysPaymentMethod.Name);
            }

            return dict;
        }

        #endregion

        #region SysPaymentType

        /// <summary>
        /// Get all SysPaymentType's
        /// Accessor for SysDbCache
        /// </summary>
        /// <returns></returns>
        public List<SysPaymentType> GetSysPaymentTypes()
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return sysEntitiesReadOnly.SysPaymentType
                            .ToList<SysPaymentType>();
        }


        #endregion

        #region Transfer

        public ActionResult TransferSupplierPayment(List<SupplierPaymentGridDTO> items, SoeOriginStatusChange statusChange, int accountYearId, int paymentMethodId, DateTime? bulkPayDate, bool sendPaymentFile)
        {
            ActionResult result = new ActionResult(false);
            PaymentManager pm = new PaymentManager(base.parameterObject);
            InvoiceManager im = new InvoiceManager(base.parameterObject);

            switch (statusChange)
            {
                case SoeOriginStatusChange.SupplierInvoice_OriginToPayment:
                    if (paymentMethodId != 0)
                        result = pm.SavePaymentFromSupplierPaymentGridDTO(items, statusChange, bulkPayDate, paymentMethodId, accountYearId, base.ActorCompanyId, sendPaymentFile);
                    break;
                case SoeOriginStatusChange.SupplierInvoice_OriginToPaymentSuggestion:
                    result = pm.SavePaymentFromSupplierPaymentGridDTO(items, statusChange, bulkPayDate, paymentMethodId, accountYearId, base.ActorCompanyId, false);
                    break;
                case SoeOriginStatusChange.SupplierPayment_PaymentSuggestionToPayed:
                    if (paymentMethodId != 0)
                        result = pm.SavePaymentFromPaymentSuggestion(items, bulkPayDate, paymentMethodId, accountYearId, base.ActorCompanyId, false, sendPaymentFile);
                    break;
                case SoeOriginStatusChange.SupplierPayment_PaymentSuggestionToCancel:
                    result = pm.CancelPayment(items, base.ActorCompanyId);
                    break;
                case SoeOriginStatusChange.SupplierPayment_PayedEditDateAndAmount:
                    result = pm.SaveSupplierPaymentDateAndAmounts(items, accountYearId, base.ActorCompanyId, base.UserId);
                    break;
                case SoeOriginStatusChange.SupplierPayment_PayedEditDateAndAmountToVoucher:
                    result = pm.SaveSupplierPaymentDateAndAmounts(items, accountYearId, base.ActorCompanyId, base.UserId);
                    if (result.Success)
                        result = pm.TransferPaymentRowsToVoucher(items, SoeOriginType.SupplierPayment, accountYearId, base.ActorCompanyId);
                    break;
                case SoeOriginStatusChange.SupplierPayment_ChangePayDate:
                    result = pm.SaveSupplierPaymentDate(items, bulkPayDate.Value, base.ActorCompanyId);
                    break;
                case SoeOriginStatusChange.SupplierPayment_PayedToVoucher:
                    result = pm.TransferPaymentRowsToVoucher(items, SoeOriginType.SupplierPayment, accountYearId, base.ActorCompanyId);
                    break;
                case SoeOriginStatusChange.SupplierPayment_PayedToCancel:
                    result = pm.CancelPayment(items, base.ActorCompanyId);
                    break;
                case SoeOriginStatusChange.SupplierPayment_ChangePayDateToVoucher:
                    result = pm.SaveSupplierPaymentDate(items, bulkPayDate.Value, base.ActorCompanyId);
                    if (result.Success)
                        result = pm.TransferPaymentRowsToVoucher(items, SoeOriginType.SupplierPayment, accountYearId, base.ActorCompanyId);
                    break;
                case SoeOriginStatusChange.OriginToMatched:
                    result = im.AddMatchUpdateInvoicePaymentStatus(items, base.ActorCompanyId, bulkPayDate);
                    break;
            }

            if (!result.Success)
                result.ErrorMessage = GetErrorMessage(result.ErrorNumber, result.StringValue, result.ErrorMessage);

            return result;
        }

        public string GetErrorMessage(int errorNumber, string stringValue, string errorMessage)
        {
            string message = "";

            switch (errorNumber)
            {
                #region General

                case (int)ActionResultSave.InvalidStateTransition:
                    message += GetText(93, (int)TermGroup.ChangeStatusGrid, "Felaktig statusförändring");
                    break;
                case (int)ActionResultSave.AccountYearNotOpen:
                    message += GetText(110, (int)TermGroup.ChangeStatusGrid, "Redovisningsåret är inte öppet") + ". " + stringValue;
                    break;
                case (int)ActionResultSave.AccountPeriodNotOpen:
                    message += GetText(111, (int)TermGroup.ChangeStatusGrid, "Perioden är inte öppen") + ". " + stringValue;
                    break;
                case (int)ActionResultSave.AccountYearVoucherDateDoNotMatch:
                    message += GetText(444, (int)TermGroup.ChangeStatusGrid, "Bokf.datum stämmer inte med aktuellt år") + ". " + stringValue;
                    break;
                case (int)ActionResultSave.AccountYearNotFound:
                    message += GetText(458, (int)TermGroup.ChangeStatusGrid, "Redovisningsåret finns inte upplagt") + ". " + stringValue;
                    break;
                case (int)ActionResultSave.AccountPeriodNotFound:
                    message += GetText(498, (int)TermGroup.ChangeStatusGrid, "Redovisningsperioden finns inte upplagt") + ". " + stringValue;
                    break;
                case (int)ActionResultSave.CustomerIsBlocked:
                    message += GetText(506, (int)TermGroup.ChangeStatusGrid, "En eller flera kunder är spärrade") + ".";
                    break;

                #endregion

                #region Payment

                case (int)ActionResultSave.PaymentFileLbFailed:
                    message += GetText(94, (int)TermGroup.ChangeStatusGrid, "Fel uppstod i LB fil");
                    break;
                case (int)ActionResultSave.PaymentFilePgFailed:
                    message += GetText(95, (int)TermGroup.ChangeStatusGrid, "Fel uppstod i PG fil");
                    break;
                case (int)ActionResultSave.PaymentIncorrectDateAccountYear:
                    message += GetText(490, (int)TermGroup.ChangeStatusGrid, "Datum ligger inte inom samma redovisningsår, måste backas");
                    if (!String.IsNullOrEmpty(errorMessage))
                    {
                        message += ". ";
                        message += GetText(407, (int)TermGroup.ChangeStatusGrid, "Bet.löp") + " " + errorMessage;
                    }
                    break;
                case (int)ActionResultSave.PaymentIncorrectDateAccountPeriod:
                    message += GetText(497, (int)TermGroup.ChangeStatusGrid, "Datum ligger inom en redovisningsperiod som ej är öppen");
                    if (!String.IsNullOrEmpty(errorMessage))
                    {
                        message += ". ";
                        message += GetText(407, (int)TermGroup.ChangeStatusGrid, "Bet.löp") + " " + errorMessage;
                    }
                    break;
                case (int)ActionResultSave.PaymentIncorrectAmount:
                    message += GetText(98, (int)TermGroup.ChangeStatusGrid, "Betalt belopp får inte vara 0 eller överstiga fakturans belopp");
                    if (!String.IsNullOrEmpty(errorMessage))
                    {
                        message += ". ";
                        message += GetText(407, (int)TermGroup.ChangeStatusGrid, "Bet.löp") + " " + errorMessage;
                    }
                    break;
                case (int)ActionResultSave.PaymentIncorrectCreditAmount:
                    message += GetText(665, (int)TermGroup.ChangeStatusGrid, "Kreditfakturor har större belopp än debetfakturor");
                    if (!String.IsNullOrEmpty(errorMessage))
                    {
                        message += ". ";
                        message += GetText(407, (int)TermGroup.ChangeStatusGrid, "Bet.löp") + " " + errorMessage;
                    }
                    break;
                case (int)ActionResultSave.PaymentNotFound:
                    message += GetText(97, (int)TermGroup.ChangeStatusGrid, "Betalning hittades inte");
                    break;
                case (int)ActionResultSave.PaymentInvoiceAssetRowNotFound:
                    message += GetText(434, (int)TermGroup.ChangeStatusGrid, "Kontrollera konteringsraderna på fakturan");
                    break;
                case (int)ActionResultSave.PaymentSysPaymentTypeMissing:
                    message += GetText(395, (int)TermGroup.ChangeStatusGrid, "Betalkonto saknas");
                    break;
                case (int)ActionResultSave.PaymentIncorrectPaymentType:
                    message += GetText(99, (int)TermGroup.ChangeStatusGrid, "Felaktig betalningstyp");
                    break;
                case (int)ActionResultSave.PaymentFilePaymentInformationMissing:
                    message += GetText(100, (int)TermGroup.ChangeStatusGrid, "Betalningsinformation saknas") + (!string.IsNullOrEmpty(errorMessage) ? ": " + errorMessage : "");
                    break;
                case (int)ActionResultSave.PaymentInvalidAccountNumber:
                    message += GetText(402, (int)TermGroup.ChangeStatusGrid, "Kontonummer saknas eller är felaktigt för en eller flera leverantörsfakturor");
                    break;
                case (int)ActionResultSave.PaymentInvalidBICIBAN:
                    string[] parts = errorMessage.Split(':');
                    string actorType = parts[0];
                    string actorName = parts[1].Trim();

                    //TODO: Dont use magic-strings
                    switch (actorType)
                    {
                        case "Company":
                            message = String.Format(GetText(403, (int)TermGroup.ChangeStatusGrid, "BIC/IBAN saknas eller är felaktigt på företaget {0}.\r\nBIC och IBAN ska skrivas i samma fält i företagsinställningar separerade med '/'"), actorName);
                            break;
                        case "Supplier":
                            message = String.Format(GetText(404, (int)TermGroup.ChangeStatusGrid, "BIC/IBAN saknas eller är felaktigt på leverantören {0}.\r\nBIC och IBAN ska skrivas i samma fält i leverantörsformuläret separerade med '/'"), actorName);
                            break;
                    }
                    break;
                case (int)ActionResultSave.CancelPaymentFailedTransferredToVoucher:
                    message += GetText(437, (int)TermGroup.ChangeStatusGrid, "Betalningsunderlag överfört till verifikat");
                    break;
                case (int)ActionResultSave.CancelPaymentFailedAlreadyCancelled:
                    message += GetText(560, (int)TermGroup.ChangeStatusGrid, "Betalningsunderlag är redan backat");
                    break;
                case (int)ActionResultSave.PaymentSuggestionExistsOnCompSettDisabled:
                    message += GetText(535, (int)TermGroup.ChangeStatusGrid, "Betalningsförslag hittade trots att företaget ej har betalningsförslag aktiverat, dessa måste tas bort.");
                    break;
                case (int)ActionResultSave.PaymentNrCannotBeEmpty:
                    message += GetText(561, (int)TermGroup.ChangeStatusGrid, "Betalkonto måste anges");
                    break;
                case (int)ActionResultSave.AccountCustomerOverpayNotSpecified:
                    message += GetText(708, (int)TermGroup.ChangeStatusGrid, "Överbetalningskonto saknas, skapandet av betalningar avbröts.");
                    break;

                #endregion

                default:
                    message = errorMessage;
                    break;
            }

            return message;
        }

        #endregion
    }
}
