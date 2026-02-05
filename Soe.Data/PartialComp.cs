using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace SoftOne.Soe.Data
{
    #region Comp functions

    public partial class Comp
    {
        private bool? fromCore;

        public bool? FromCore
        {
            get { return fromCore; }
            set { fromCore = value; }
        }

        public static CompEntities CreateCompEntities(bool isReadOnly = false, bool requestScoped = false)
        {
            CompEntities entities = new CompEntities();
            entities.IsReadOnly = isReadOnly;
            entities.RequestScoped = requestScoped;
            entities.ThreadId = Thread.CurrentThread.ManagedThreadId;
            entities.ContextOptions.LazyLoadingEnabled = false;
            if (isReadOnly)
                entities.ContextOptions.ProxyCreationEnabled = false;  //Disable all tracking of changes in the context

            if (entities.CommandTimeout.HasValue == false || entities.CommandTimeout < 30)
                entities.CommandTimeout = 60; //Default timeout 1 minute
            return entities;
        }

    }

    #endregion

    #region Tables

    public partial class Actor : ICreatedModified
    {
        public string Name { get; set; }
        public string TypeName { get; set; }

        public List<ContactPerson> ActiveContactPersons
        {
            get
            {
                List<ContactPerson> activeContactPersons;

                if (this.ContactPersons != null)
                {
                    #region Try load

                    try
                    {
                        if (!this.IsAdded())
                        {
                            if (!this.ContactPersons.IsLoaded)
                                this.ContactPersons.Load();
                        }
                    }
                    catch (InvalidOperationException ioExc)
                    {
                        //Entity not attached, cannot load
                        ioExc.ToString(); //Prevent compiler warning
                    }

                    #endregion

                    activeContactPersons = this.ContactPersons.Where(i => i.State == (int)SoeEntityState.Active).ToList();
                }
                else
                {
                    activeContactPersons = new List<ContactPerson>();
                }

                return activeContactPersons;
            }
        }
    }

    public partial class AccountPeriod
    {
        public bool? HasVouchers { get; set; }
    }

    public partial class AnnualLeaveGroup
    {
        public string TypeName { get; set; }
        public int RuleRestTimeMinimum
        {
            get
            {
                return 24 * 60;
            }
        }
    }

    public partial class AnnualLeaveTransaction
    {
        public string TypeName { get; set; }
        public bool IsManually
        {
            get
            {
                return Type == (int)TermGroup_AnnualLeaveTransactionType.ManuallyEarned; // Transactions holding only AccumulatedMinutes
            }
        }
        public bool IsYearlyBalance
        {
            get
            {
                return Type == (int)TermGroup_AnnualLeaveTransactionType.YearlyBalance;
            }
        }
    }

    public partial class ChecklistHead : ICreatedModified, IState
    {
        public string TypeName { get; set; }
    }

    public partial class ChecklistRow : ICreatedModified, IState
    {
        public string TypeName { get; set; }
    }

    public partial class Customer : ICreatedModified, IState
    {
        public bool IsEUCountryBased { get; set; }
        public override string ToString()
        {
            return Name;
        }

        public string CustomerNrSort
        {
            get { return CustomerNr.PadLeft(20, '0'); }
        }

        //Customer categories
        public List<int> CategoryIds { get; set; }
        public List<string> CategoryNames { get; set; }
        public string CategoryNamesString
        {
            get
            {
                return StringUtility.GetCommaSeparatedString(CategoryNames, addWhiteSpace: true);
            }
        }
    }

    public partial class CustomerIO
    {
        public string StatusName { get; set; }
        public string VatTypeName { get; set; }
        public List<ContactEComIODTO> GlnNbrs { get; set; }
        public List<ContactAdressIODTO> BillingAddresses { get; set; }
        public bool? IsPrivatePerson { get; set; }
    }

    public partial class CustomerInvoiceHeadIO
    {
        public string StatusName { get; set; }
        public string BillingTypeName { get; set; }
        public string DebetInvoiceNr { get; set; }
    }

    public partial class CustomerInvoiceRowIO
    {
        public string StatusName { get; set; }
    }

    public partial class EdiReceivedMsg
    {
        /// <summary>
        /// Move to db later
        /// </summary>
        public int? ActorCompanyId { get; set; }
        public string EdiType { get; set; }        
        public static int[] ClosedStates { get { return new[] { (int)EdiTransferState.Deleted }; } }
    }

    public partial class EdiTransfer
    {
        public static int[] ClosedStates { get { return new[] { (int)EdiTransferState.Deleted, (int)EdiTransferState.NoSoftOneCustomer, (int)EdiTransferState.Transferred }; } }

    }

    public partial class EmployeeGroup
    {
        public int? DefaultDim1CostAccountId { get; set; }
        public int? DefaultDim2CostAccountId { get; set; }
        public int? DefaultDim3CostAccountId { get; set; }
        public int? DefaultDim4CostAccountId { get; set; }
        public int? DefaultDim5CostAccountId { get; set; }
        public int? DefaultDim6CostAccountId { get; set; }
        public int? DefaultDim1IncomeAccountId { get; set; }
        public int? DefaultDim2IncomeAccountId { get; set; }
        public int? DefaultDim3IncomeAccountId { get; set; }
        public int? DefaultDim4IncomeAccountId { get; set; }
        public int? DefaultDim5IncomeAccountId { get; set; }
        public int? DefaultDim6IncomeAccountId { get; set; }


    }

    public partial class Inventory : ICreatedModified, IState
    {
        public string StatusName { get; set; }
        public List<int> CategoryIds { get; set; }
        public List<string> CategoryNames { get; set; }
        public string CategoryNamesString
        {
            get
            {
                return StringUtility.GetCommaSeparatedString(CategoryNames, addWhiteSpace: true);
            }
        }
        public string ParentName { get; set; }
        public string SupplierInvoiceInfo { get; set; }
        public string CustomerInvoiceInfo { get; set; }
        public string InventoryAccountNr { get; set; }
        public string InventoryAccountName { get; set; }
    }

    public partial class Invoice : ICreatedModified, IState
    {
        public string InvoiceNrSort
        {
            get { return SortInvoiceNr(InvoiceNr); }
        }

        public static string SortInvoiceNr(string source)
        {
            if (source.Contains("-"))
            {
                string sufrix = source;

                int idx = source.IndexOf('-');
                source = source.Substring(0, idx);

                sufrix = sufrix.Substring(idx + 1, source.Length - idx);

                sufrix = sufrix.PadRight(5, '0');

                source = source + sufrix;
            }
            else
            {
                source = source + "-0000";
            }

            return source.PadLeft(15, '0');
        }

        public string ActorNr
        {
            get
            {
                if (this.Actor != null)
                {
                    //TODO: New reference
                    if (this.Actor.Customer != null)
                        return this.Actor.Customer.CustomerNr;
                    //TODO: New reference
                    else if (this.Actor.Supplier != null)
                        return this.Actor.Supplier.SupplierNr;
                }
                return String.Empty;
            }
        }

        public string ActorName
        {
            get
            {
                if (this.Actor != null)
                {
                    //TODO: New reference
                    if (this.Actor.Customer != null)
                        return this.Actor.Customer.Name;
                    //TODO: New reference
                    else if (this.Actor.Supplier != null)
                        return this.Actor.Supplier.Name;
                }
                return String.Empty;
            }
        }

        public string ProjectNr
        {
            get { return this.Project != null ? this.Project.Number : string.Empty; }
        }

        public string ProjectName
        {
            get { return this.Project != null ? this.Project.Name : string.Empty; }
        }

        public int Status
        {
            get
            {
                if (this.Origin != null)
                    return this.Origin.Status;
                return 0;
            }
            set
            {
                if (this.Origin != null)
                    this.Origin.Status = value;
            }
        }

        public string StatusName { get; set; }

        public bool IsTotalAmountPayed
        {
            get
            {
                if (this.BillingType == (int)TermGroup_BillingType.Credit)
                    return this.PaidAmount != 0 && (Decimal.Round(this.TotalAmount, 2) - Decimal.Round(this.PaidAmount, 2) >= 0);
                else
                    return this.PaidAmount != 0 && (Decimal.Round(this.TotalAmount, 2) - Decimal.Round(this.PaidAmount, 2) <= 0);
            }
        }
    }

    public partial class InvoiceProduct
    {
        // Used in the CopyInvoiceProductFromSys service
        public decimal SalesPrice { get; set; }
        public int? SysProductType { get; set; }
        //Product categories
        public List<int> CategoryIds { get; set; }
        public List<string> CategoryNames { get; set; }
        public string CategoryNamesString
        {
            get
            {
                return StringUtility.GetCommaSeparatedString(CategoryNames, addWhiteSpace: true);
            }
        }

        public bool IsSupplementCharge
        {
            get { return (CalculationType == (int)TermGroup_InvoiceProductCalculationType.SupplementCharge); }
        }
    }

    public partial class Markup : ICreatedModified, IState
    {
        public string WholesellerName { get; set; }
        public decimal WholesellerDiscountPercent { get; set; }
    }

    public partial class PaymentExport
    {
        public List<PaymentRow> PaymentRows { get; set; }
        public bool Foreign { get; set; }
        public int CancelledState { get; set; }
    }

    public partial class PaymentImportIO : IState
    {
        public DateTime? DueDate { get; set; }
    }

    public partial class PaymentInformation : ICreatedModified, IState
    {
        public IEnumerable<PaymentInformationRow> ActivePaymentInformationRows
        {
            get
            {
                if (this.PaymentInformationRow == null)
                    return null;

                try
                {
                    if (!this.IsAdded())
                    {
                        if (!this.PaymentInformationRow.IsLoaded)
                            this.PaymentInformationRow.Load();
                    }
                }
                catch (InvalidOperationException ioExc)
                {
                    //Entity not attached, cannot load
                    ioExc.ToString(); //Prevent compiler warning
                }

                if (this.PaymentInformationRow != null)
                    return this.PaymentInformationRow.Where<PaymentInformationRow>(pir => pir.State == (int)SoeEntityState.Active);
                return null;
            }
        }
    }

    public partial class PaymentInformationRow : ICreatedModified, IState
    {
        public string CurrencyCode { get; set; }
        public string SysPaymentTypeName { get; set; }
    }

    public partial class PaymentRow : ICreatedModified, IState
    {
        public bool VoucherHasMultiplePayments { get; set; }
        public string StatusName { get; set; }
        public int TransferStatus { get; set; }

        public IEnumerable<PaymentAccountRow> ActivePaymentAccountRows
        {
            get
            {
                if (this.PaymentAccountRow == null)
                    return null;

                try
                {
                    if (!this.IsAdded())
                    {
                        if (!this.PaymentAccountRow.IsLoaded)
                            this.PaymentAccountRow.Load();
                    }
                }
                catch (InvalidOperationException ioExc)
                {
                    //Entity not attached, cannot load
                    ioExc.ToString(); //Prevent compiler warning
                }

                if (this.PaymentAccountRow != null)
                    return this.PaymentAccountRow.Where<PaymentAccountRow>(pir => pir.State == (int)SoeEntityState.Active);
                return null;
            }
        }
    }

    public partial class PaymentMethod : IState
    {
        public string PaymentNr { get; set; }
        public string PayerBankId { get; set; }
        public string SysPaymentMethodName { get; set; }
        public int? SysPaymentTypeId { get; set; }
    }

    public partial class ProjectInvoiceDay
    {
        public Guid ProjectInvoiceDayTempId { get; set; }
        public string CommentExternal { get; set; }
    }

    public partial class PriceRule : IModified
    {
        public int lExampleType { get; set; }
        public int rExampleType { get; set; }
    }

    public partial class ProjectIO : ICreatedModified, IState
    {
        public string StatusName { get; set; }
    }

    public partial class ScheduledJobSetting
    {
        public List<SmallGenericType> Options { get; set; }
    }

    public partial class ShiftType
    {
        public int? AccountSettingDim2Nr { get; set; }
        public int? AccountSetting2Id { get; set; }
        public string AccountSetting2Nr { get; set; }
        public string AccountSetting2Name { get; set; }

        public int? AccountSettingDim3Nr { get; set; }
        public int? AccountSetting3Id { get; set; }
        public string AccountSetting3Nr { get; set; }
        public string AccountSetting3Name { get; set; }

        public int? AccountSettingDim4Nr { get; set; }
        public int? AccountSetting4Id { get; set; }
        public string AccountSetting4Nr { get; set; }
        public string AccountSetting4Name { get; set; }

        public int? AccountSettingDim5Nr { get; set; }
        public int? AccountSetting5Id { get; set; }
        public string AccountSetting5Nr { get; set; }
        public string AccountSetting5Name { get; set; }

        public int? AccountSettingDim6Nr { get; set; }
        public int? AccountSetting6Id { get; set; }
        public string AccountSetting6Nr { get; set; }
        public string AccountSetting6Name { get; set; }

    }

    public partial class Supplier : ICreatedModified, IState
    {
        public bool IsEUCountryBased { get; set; }
        public AttestWorkFlowHead TemplateAttestHead { get; set; }
        public List<int> CategoryIds { get; set; }
        public PaymentInformation PaymentInformation { get; set; }
        public override string ToString()
        {
            return Name;
        }
        public string SupplierNrSort
        {
            get { return SupplierNr.PadLeft(20, '0'); }
        }
    }

    public partial class SupplierAgreement
    {
        public string WholesellerName { get; set; }
        public string PriceListTypeName { get; set; }
    }

    public partial class SupplierInvoice : ICreatedModified, IState
    {
        public string AttestStateName { get; set; }
        public int? BlockReasonTextId { get; set; }
        public string BlockReason { get; set; }
        public CustomerInvoice Order { get; set; }
        public IEnumerable<SupplierInvoiceRow> ActiveSupplierInvoiceRows
        {
            get
            {
                if (this.SupplierInvoiceRow == null)
                    return null;

                try
                {
                    if (!this.IsAdded())
                    {
                        if (!this.SupplierInvoiceRow.IsLoaded)
                            this.SupplierInvoiceRow.Load();
                    }
                }
                catch (InvalidOperationException ioExc)
                {
                    //Entity not attached, cannot load
                    ioExc.ToString(); //Prevent compiler warning
                }

                if (this.SupplierInvoiceRow != null)
                    return SupplierInvoiceRow.Where<SupplierInvoiceRow>(si => si.State == (int)SoeEntityState.Active);
                return null;
            }
        }
        public List<SupplierInvoiceProjectRowDTO> SupplierInvoiceProjectRows { get; set; }
        public List<SupplierInvoiceOrderRowDTO> SupplierInvoiceOrderRows { get; set; }
        public List<SupplierInvoiceCostAllocationDTO> SupplierInvoiceCostAllocationRows { get; set; }
        public bool IsDraftOrOrigin()
        {
            return Origin != null && (Origin.Status == (int)SoeOriginStatus.Draft || Origin.Status == (int)SoeOriginStatus.Origin);
        }
    }

    public partial class SupplierInvoiceRow : ICreatedModified, IState
    {
        public IEnumerable<SupplierInvoiceAccountRow> ActiveSupplierInvoiceAccountRows
        {
            get
            {
                if (this.SupplierInvoiceAccountRow == null)
                    return null;

                try
                {
                    if (!this.IsAdded())
                    {
                        if (!this.SupplierInvoiceAccountRow.IsLoaded)
                            this.SupplierInvoiceAccountRow.Load();
                    }
                }
                catch (InvalidOperationException ioExc)
                {
                    //Entity not attached, cannot load
                    ioExc.ToString(); //Prevent compiler warning
                }

                if (this.SupplierInvoiceAccountRow != null)
                    return this.SupplierInvoiceAccountRow.Where<SupplierInvoiceAccountRow>(iar => iar.State == (int)SoeEntityState.Active && iar.Type == (int)AccountingRowType.AccountingRow).OrderBy(iar => iar.RowNr);
                return null;
            }
        }
        public IEnumerable<SupplierInvoiceAccountRow> ActiveSupplierInvoiceAttestAccountRows
        {
            get
            {
                if (this.SupplierInvoiceAccountRow == null)
                    return null;

                try
                {
                    if (!this.IsAdded())
                    {
                        if (!this.SupplierInvoiceAccountRow.IsLoaded)
                            this.SupplierInvoiceAccountRow.Load();
                    }
                }
                catch (InvalidOperationException ioExc)
                {
                    //Entity not attached, cannot load
                    ioExc.ToString(); //Prevent compiler warning
                }

                if (this.SupplierInvoiceAccountRow != null)
                    return this.SupplierInvoiceAccountRow.Where<SupplierInvoiceAccountRow>(iar => iar.State == (int)SoeEntityState.Active && iar.Type == (int)AccountingRowType.SupplierInvoiceAttestRow).OrderBy(iar => iar.RowNr);
                return null;
            }
        }
    }

    public partial class SupplierInvoiceHeadIO : ICreatedModified, IState
    {
        public string StatusName { get; set; }
        public string BillingTypeName { get; set; }
        public Dictionary<string, byte[]> Attachements;
        public Dictionary<int, string> PaymentNumbers;
        public List<SupplierInvoiceProductRowDTO> ProductRows;
    }

    public partial class SupplierInvoiceAccountingRowIO
    {
        public int Dim1Id { get; set; }
        public int Dim2Id { get; set; }
        public int Dim3Id { get; set; }
        public int Dim4Id { get; set; }
        public int Dim5Id { get; set; }
        public int Dim6Id { get; set; }
    }

    public partial class SupplierIO : ICreatedModified, IState
    {
        public string StatusName { get; set; }
        public string VatTypeName { get; set; }
        public string StandardPaymentTypeName { get; set; }
    }

    public partial class VoucherHead : ICreatedModified
    {
        public string SourceTypeName { get; set; }
        public int VoucherSeriesTypeNr
        {
            get
            {
                if (VoucherSeries != null && VoucherSeries.VoucherSeriesType != null)
                    return VoucherSeries.VoucherSeriesType.VoucherSeriesTypeNr;
                return 0;
            }
        }
        public string VoucherSeriesTypeName
        {
            get
            {
                if (VoucherSeries != null && VoucherSeries.VoucherSeriesType != null)
                    return VoucherSeries.VoucherSeriesType.Name;
                return String.Empty;
            }
        }
        public IEnumerable<VoucherRow> ActiveVoucherRows
        {
            get
            {
                if (this.VoucherRow != null)
                    return this.VoucherRow.Where<VoucherRow>(vr => vr.State == (int)SoeEntityState.Active);
                return null;
            }
        }
        //Only used for performance in reports
        public int BudgetAccountId { get; set; }
        public List<int> AccountIds { get; set; }
        public bool AccountIdsHandled { get; set; }
    }

    public partial class VoucherHeadIO : ICreatedModified, IState
    {
        public string VoucherSeriesName { get; set; }
        public string StatusName { get; set; }
    }

    public partial class VoucherRow
    {
        public long VoucherNr
        {
            get
            {
                return VoucherHead?.VoucherNr ?? 0;
            }
        }

        public int VoucherSeriesTypeNr
        {
            get
            {
                if (VoucherHead != null && VoucherHead.VoucherSeries != null && VoucherHead.VoucherSeries.VoucherSeriesType != null)
                    return VoucherHead.VoucherSeries.VoucherSeriesType.VoucherSeriesTypeNr;
                return 0;
            }
        }

        public string VoucherSeriesTypeName
        {
            get
            {
                if (VoucherHead != null && VoucherHead.VoucherSeries != null && VoucherHead.VoucherSeries.VoucherSeriesType != null)
                    return VoucherHead.VoucherSeries.VoucherSeriesType.Name;
                return String.Empty;
            }
        }

        public string AccountNr
        {
            get
            {
                if (AccountStd != null && AccountStd.Account != null)
                    return AccountStd.Account.AccountNr;
                return String.Empty;
            }
        }

        public string AccountName
        {
            get
            {
                if (AccountStd != null && AccountStd.Account != null)
                    return AccountStd.Account.Name;
                return String.Empty;
            }
        }

        public int? SysVatAccountId
        {
            get
            {
                if (AccountStd != null && AccountStd.SysVatAccountId != null)
                    return AccountStd.SysVatAccountId;
                return null;
            }
        }
    }

    public partial class VoucherRowIO : ICreatedModified, IState
    {
        public string StatusName { get; set; }
    }

    public partial class VoucherRowHistory
    {
        public string LoginName { get; set; }
        public string AccountNr { get; set; }
        public int VoucherHeadId { get; set; }
        public long VoucherNr { get; set; }
        public int VoucherSeriesId { get; set; }
        public string VoucherSeriesTypeName { get; set; }
        public int VoucherSeriesTypeNr { get; set; }
        public int PeriodNr { get; set; }
        public string YearFrom { get; set; }
        public string YearTo { get; set; }
        public string RegDate { get; set; }
    }

    /// <summary>
    /// Used to represent the mapping table between VoucherRow and Account (VoucherRow.AccountInternal). It's not a real entity.
    /// </summary>
    public class VoucherRowAccount
    {
        public int VoucherRowId { get; set; }
        public int AccountId { get; set; }

        public static int ColumnCount
        {
            get
            {
                return 2;
            }
        }
    }

    #endregion

    #region Views

    public partial class AccountGridView
    {
        public string Type { get; set; }
    }

    public partial class ContactEcomView
    {
        public bool HideText { get; set; }
    }

    public partial class CompCurrency
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public List<CompCurrencyRate> CompCurrencyRates { get; set; }
    }

    public partial class CompCurrencyRate
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public partial class CompCurrencyRate
    {
        public string IntervalTypeName { get; set; }
        public string SourceName { get; set; }
    }
    public partial class CurrencyRate
    {
        public string SourceName { get; set; }
    }
    public partial class Currency
    {
        public string IntervalName { get; set; }
    }
    public partial class ContractTraceView
    {
        public bool Foreign { get; set; }
    }

    public partial class GetEdiEntrysResult
    {
        public decimal RoundedInterpretation { get; set; }
        public int? SupplierAttestGroupId { get; set; }
        public string SupplierAttestGroupName { get; set; }
    }

    public partial class Invoice
    {
        public bool IsDebit
        {
            get { return this.BillingType == (int)TermGroup_BillingType.Debit; }
        }

        public bool IsCredit
        {
            get { return this.BillingType == (int)TermGroup_BillingType.Credit; }
        }

        public bool IsInterest
        {
            get { return this.BillingType == (int)TermGroup_BillingType.Interest; }
        }

        public bool IsClaim
        {
            get { return this.BillingType == (int)TermGroup_BillingType.Reminder; }
        }

        public bool IsSupplierInvoice
        {
            get { return this.Type == (int)SoeOriginType.SupplierInvoice; }
        }

        public bool IsCustomerInvoice
        {
            get { return this.Type == (int)SoeOriginType.CustomerInvoice; }
        }

        public bool IsContract
        {
            get { return this.Type == (int)SoeOriginType.Contract; }
        }

        public bool IsOffer
        {
            get { return this.Type == (int)SoeOriginType.Offer; }
        }

        public bool IsOrder
        {
            get { return this.Type == (int)SoeOriginType.Order; }
        }
    }

    public partial class InvoiceTraceView
    {
        public bool Foreign { get; set; }
    }

    public partial class OfferTraceView
    {
        public bool Foreign { get; set; }
    }

    public partial class OrderTraceView
    {
        public bool Foreign { get; set; }
    }

    public partial class PaymentTraceView
    {
        public bool Foreign { get; set; }
    }

    public partial class Position
    {
        public string SysPositionCode { get; set; }
        public string SysPositionName { get; set; }
        public string SysPositionDescription { get; set; }
    }

    public partial class PriceList
    {
        public string SysPriceListTypeName { get; set; }
    }

    public partial class Project
    {
        public string StatusName { get; set; }
        public string Categories { get; set; }
        public string OrderNumbers { get; set; }

        //Used in reports
        public List<Employee> Employees { get; set; }
    }

    public partial class ProjectUser
    {
        public string TypeName { get; set; }
    }

    public partial class VoucherTraceView
    {
        public bool Foreign { get; set; }
    }

    #endregion

    #region Procedures
    [DebuggerDisplay("{InvoiceNr}, {InvoiceTotalAmount}, {PayDate}")]
    public partial class GetReportBalanceListItemsResult
    {
        public bool IsMatched { get; set; }
        public bool IsMatchedAgainstInvoice { get; set; }
        public bool IsMatchedAgainstPayment { get; set; }
        public string MatchedNr { get; set; }
    }

    public partial class GetTimeCodeTransactionsForAcc_Result
    {
        public TimeCode TimeCode { get; set; }

        public bool TimeCodeRegistrationTypeIsQuantity
        {
            get { return this.TimeCode?.RegistrationType == (int)TermGroup_TimeCodeRegistrationType.Quantity; }
        }

        public decimal CalculateFactor() => this.TimeCodeRegistrationTypeIsQuantity && this.Factor == 1 ? 60 : this.Factor;
    }

    public partial class GetTimeInvoiceTransactionsForAcc_Result
    {
        public Product Product { get; set; }
    }

    public partial class GetTimePayrollTransactionsForAcc_Result
    {
        public Product Product { get; set; }
    }


    public partial class GetTimeSchedulePlanningPeriods_Result
    {
        //Midnight secure
        public DateTime? ActualStartTime
        {
            get { return Date.HasValue ? CalendarUtility.MergeDateAndTime(this.Date.Value, this.StartTime) : (DateTime?)null; }
        }
        //Midnight secure
        public DateTime? ActualStopTime
        {
            get { return ActualStartTime.HasValue ? ActualStartTime.Value.AddMinutes((this.StopTime - this.StartTime).TotalMinutes) : (DateTime?)null; }
        }
    }

    public partial class GetTimeSchedulePlanningShifts_Result
    {
        public bool BelongsToPreviousDay
        {
            get { return StartTime.Date.AddDays(-1) == CalendarUtility.DATETIME_DEFAULT; }
        }
        public bool BelongsToNextDay
        {
            get { return StartTime.Date.AddDays(1) == CalendarUtility.DATETIME_DEFAULT; }
        }
    }

    #endregion
}
