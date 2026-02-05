using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using System;
using System.Data.Entity;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;

namespace SoftOne.Soe.Data
{
    public partial class CustomerInvoice : ICreatedModified
    {
        public int NbrOfChecklists { get; set; }
        public string CustomerBlockNote { get; set; }
        public bool PriceListTypeInclusiveVat { get; set; }
        public bool TransferedFromOrder { get; set; }
        public bool TransferedFromOffer { get; set; }
        public SoeOriginType TransferedFromType { get; set; }
        public int AgreementOriginIdForServiceOrder { get; set; }
        public int AgreementActorIdForServiceOrder { get; set; }

        public List<CustomerInvoiceRow> ActiveCustomerInvoiceRows
        {
            get
            {
                if (this.CustomerInvoiceRow == null)
                    return null;

                try
                {
                    if (!this.IsAdded())
                    {
                        if (!this.CustomerInvoiceRow.IsLoaded)
                            this.CustomerInvoiceRow.Load();
                    }
                }
                catch (InvalidOperationException ioExc)
                {
                    //Entity not attached, cannot load
                    ioExc.ToString(); //Prevent compiler warning
                }

                if (this.CustomerInvoiceRow != null)
                    return this.CustomerInvoiceRow.Where<CustomerInvoiceRow>(cir => cir.State == (int)SoeEntityState.Active).ToList();
                return null;
            }
        }

        public bool LoadCustomerInvoiceAccountRows(SOECompEntities entities)
        {
            if (InvoiceId != 0 && this.CustomerInvoiceRow.IsLoaded)
            {
                entities.CustomerInvoiceAccountRow.Where(car => car.CustomerInvoiceRow.InvoiceId == InvoiceId && car.State == (int)SoeEntityState.Active).Load();
                foreach (var cir in this.CustomerInvoiceRow.Where(row => row.EntityState != EntityState.Added && row.EntityState != EntityState.Detached))
                {
                    cir.CustomerInvoiceAccountRowIsLoaded = true;
                }
                return true;
            }
            return false;
        }

        public List<int> CategoryIds { get; set; }

        //Only use when transfer attachments from order to invoice 
        public List<Images> TempImagesToCopy { get; set; }

        public List<DataStorageRecord> TempAttachmentsToCopy { get; set; }

        public List<ChecklistHeadRecord> ChecklistHeadRecords { get; set; }
    }

    public partial class CustomerInvoiceRow : ICreatedModified, IState
    {
        private bool _CustomerInvoiceAccountRowIsLoaded { get; set; }
        public int TempRowId { get; set; }

        public bool CustomerInvoiceAccountRowIsLoaded
        {
            get
            {
                return _CustomerInvoiceAccountRowIsLoaded || this.CustomerInvoiceAccountRow.IsLoaded;
            }
            set
            {
                _CustomerInvoiceAccountRowIsLoaded = value;
            }
        }

        public void LoadCustomerInvoiceAccountRow()
        {
            if (!CustomerInvoiceAccountRowIsLoaded)
            {
                CustomerInvoiceAccountRow.Load();
                CustomerInvoiceAccountRowIsLoaded = true;
            }
        }

        public IEnumerable<CustomerInvoiceAccountRow> ActiveCustomerInvoiceAccountRows
        {
            get
            {
                if (this.CustomerInvoiceAccountRow == null)
                    return null;

                try
                {
                    if (!this.IsAdded())
                    {
                        if (!this.CustomerInvoiceAccountRowIsLoaded)
                            this.LoadCustomerInvoiceAccountRow();
                    }
                }
                catch (InvalidOperationException ioExc)
                {
                    //Entity not attached, cannot load
                    ioExc.ToString(); //Prevent compiler warning
                }

                if (this.CustomerInvoiceAccountRow != null)
                    return this.CustomerInvoiceAccountRow.Where<CustomerInvoiceAccountRow>(ciar => ciar.State == (int)SoeEntityState.Active).OrderBy(ciar => ciar.RowNr);
                return null;
            }
        }
    }

    public static partial class EntityExtensions
    {
        public readonly static Expression<Func<CustomerInvoice, CustomerInvoiceSmallExDTO>> GetCustomerInvoiceSmallExDTO =
         i => new CustomerInvoiceSmallExDTO
         {
             ActorId = i.ActorId,
             CurrencyId = i.CurrencyId,
             DeliveryAddressId = i.DeliveryAddressId,
             InvoiceId = i.InvoiceId,
             InvoiceNr = i.InvoiceNr,
             PriceListTypeId = i.PriceListTypeId,
             ProjectId = i.ProjectId,
             SeqNr = i.SeqNr,
             SysWholesellerId = i.SysWholeSellerId,
             VoucherHeadId = i.VoucherHeadId,
             CashSale = i.CashSale,
             InvoiceDeliveryType = i.InvoiceDeliveryType,
             InvoiceDelieryProvider = i.InvoiceDeliveryProvider,
             InvoiceDate = i.InvoiceDate,
             CurrencyRate = i.CurrencyRate,
             OriginType = i.Origin.Type
         };

        public readonly static Expression<Func<CustomerInvoice, CustomerInvoiceSmallDTO>> GetCustomerInvoiceSmallDTO =
         i => new CustomerInvoiceSmallDTO
         {
             ActorId = i.ActorId,
             InvoiceId = i.InvoiceId,
             InvoiceNr = i.InvoiceNr,
             PriceListTypeId = i.PriceListTypeId,
             ProjectId = i.ProjectId,
             SeqNr = i.SeqNr,
         };

        public readonly static Expression<Func<CustomerInvoice, CustomerInvoiceActorDTO>> GetCustomerInvoiceActorDTO =
         i => new CustomerInvoiceActorDTO
         {
             ActorId = i.ActorId,
             CurrencyId = i.CurrencyId,
             DeliveryAddressId = i.DeliveryAddressId,
             InvoiceId = i.InvoiceId,
             InvoiceNr = i.InvoiceNr,
             PriceListTypeId = i.PriceListTypeId,
             ProjectId = i.ProjectId,
             SeqNr = i.SeqNr,
             SysWholesellerId = i.SysWholeSellerId,
             VoucherHeadId = i.VoucherHeadId,
             CustomerName = i.Actor.Customer.Name,
             CustomerNr = i.Actor.Customer.CustomerNr,
         };

        #region CustomerInvoiceToDTO

        public static CustomerInvoiceDTO ToCustomerInvoiceDTO(this CustomerInvoice e, List<CustomerInvoiceRow> invoiceRows, List<CustomerInvoiceAccountRow> accountRows, bool includeOrigin, bool includeRows, int claimAccountId = 0)
        {
            if (e == null)
                return null;

            // Create InvoiceDTO
            InvoiceDTO dto = e.ToDTO(includeOrigin);

            // Create CustomerInvoiceDTO and copy properties from InvoiceDTO
            CustomerInvoiceDTO cidto = new CustomerInvoiceDTO();
            var properties = dto.GetType().GetProperties();
            foreach (var property in properties)
            {
                PropertyInfo pi = dto.GetType().GetProperty(property.Name);
                if (pi.CanWrite)
                    property.SetValue(cidto, pi.GetValue(dto, null), null);
            }

            // Set CustomerInvoice specific properties
            cidto.OrderType = (TermGroup_OrderType)e.OrderType;
            cidto.OriginateFrom = e.OriginateFrom;
            cidto.PaymentConditionId = e.PaymentConditionId;
            cidto.DeliveryTypeId = e.DeliveryTypeId;
            cidto.DeliveryConditionId = e.DeliveryConditionId;
            cidto.DeliveryAddressId = e.DeliveryAddressId;
            cidto.BillingAddressId = e.BillingAddressId;
            cidto.PriceListTypeId = e.PriceListTypeId;
            cidto.SysWholeSellerId = e.SysWholeSellerId;
            cidto.InvoiceText = e.InvoiceText;
            cidto.InvoiceHeadText = e.InvoiceHeadText;
            cidto.InvoiceLabel = e.InvoiceLabel;
            cidto.OrderDate = e.OrderDate;
            cidto.DeliveryDate = e.DeliveryDate;
            cidto.CentRounding = e.CentRounding;
            cidto.FreightAmount = e.FreightAmount;
            cidto.FreightAmountCurrency = e.FreightAmountCurrency;
            cidto.FreightAmountEntCurrency = e.FreightAmountEntCurrency;
            cidto.FreightAmountLedgerCurrency = e.FreightAmountLedgerCurrency;
            cidto.InvoiceFee = e.InvoiceFee;
            cidto.InvoiceFeeCurrency = e.InvoiceFeeCurrency;
            cidto.InvoiceFeeEntCurrency = e.InvoiceFeeEntCurrency;
            cidto.InvoiceFeeLedgerCurrency = e.InvoiceFeeLedgerCurrency;
            cidto.SumAmount = e.SumAmount;
            cidto.SumAmountCurrency = e.SumAmountCurrency;
            cidto.SumAmountEntCurrency = e.SumAmountEntCurrency;
            cidto.SumAmountLedgerCurrency = e.SumAmountLedgerCurrency;
            cidto.MarginalIncome = e.MarginalIncome;
            cidto.MarginalIncomeCurrency = e.MarginalIncomeCurrency;
            cidto.MarginalIncomeEntCurrency = e.MarginalIncomeEntCurrency;
            cidto.MarginalIncomeLedgerCurrency = e.MarginalIncomeLedgerCurrency;
            cidto.MarginalIncomeRatio = e.MarginalIncomeRatio;
            cidto.NoOfReminders = e.NoOfReminders;
            cidto.HasHouseholdTaxDeduction = e.HasHouseholdTaxDeduction;
            cidto.FixedPriceOrder = e.FixedPriceOrder;
            cidto.MultipleAssetRows = e.MultipleAssetRows;
            cidto.InsecureDebt = e.InsecureDebt;
            cidto.PrintTimeReport = e.PrintTimeReport;
            cidto.BillingInvoicePrinted = e.BillingInvoicePrinted;
            cidto.ContractGroupId = e.ContractGroupId;
            cidto.NextContractPeriodYear = e.NextContractPeriodYear;
            cidto.NextContractPeriodValue = e.NextContractPeriodValue;
            cidto.NextContractPeriodDate = e.NextContractPeriodDate;
            cidto.WorkingDescription = e.WorkingDescription != null ? e.WorkingDescription : string.Empty;
            cidto.IncludeOnInvoice = e.IncludeOnInvoice;
            cidto.IncludeOnlyInvoicedTime = e.IncludeOnlyInvoicedTime;
            cidto.ShiftTypeId = e.ShiftTypeId;
            cidto.PlannedStartDate = e.PlannedStartDate;
            cidto.PlannedStopDate = e.PlannedStopDate;
            cidto.EstimatedTime = e.EstimatedTime;
            cidto.RemainingTime = e.RemainingTime;
            cidto.Priority = e.Priority;
            cidto.CashSale = e.CashSale;
            cidto.InternalDescription = e.InternalDescription;
            cidto.ExternalDescription = e.ExternalDescription;
            cidto.InvoiceDeliveryType = e.InvoiceDeliveryType;
            cidto.DeliveryCustomerId = e.DeliveryCustomerId;
            cidto.BillingAdressText = e.BillingAdressText;
            cidto.DeliveryDateText = e.DeliveryDateText;
            cidto.InvoicePaymentService = e.InvoicePaymentService;
            cidto.AddAttachementsToEInvoice = e.AddAttachementsToEInvoice;
            cidto.ExternalId = e.ExternalId;
            cidto.AddSupplierInvoicesToEInvoice = e.AddSupplierInvoicesToEInvoices;
            cidto.CustomerNameFromInvoice = e.CustomerName;

            // Extensions
            if (e.Actor != null && e.Actor.Customer != null)
                cidto.CustomerInvoicePaymentService = e.Actor.Customer.InvoicePaymentService;

            cidto.CustomerBlockNote = e.CustomerBlockNote;
            if (includeRows)
            {
                cidto.CustomerInvoiceRows = new List<CustomerInvoiceRowDTO>();
                foreach (var row in invoiceRows)
                {
                    var invoiceRowAccountRows = accountRows.Where(a => a.CustomerInvoiceRow.CustomerInvoiceRowId == row.CustomerInvoiceRowId).ToList();
                    invoiceRowAccountRows.ForEach(i => accountRows.Remove(i));
                    cidto.CustomerInvoiceRows.Add(row.ToCustomerInvoiceRowDTO(e, invoiceRowAccountRows, true));
                }
            }

            if (includeOrigin && e.Origin.Type == (int)SoeOriginType.CustomerInvoice)
            {
                try
                {
                    if (!e.OriginInvoiceMapping.IsLoaded)
                    {
                        e.OriginInvoiceMapping.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.OriginInvoiceMapping");
                    }

                    cidto.HasOrder = e.OriginInvoiceMapping.Any(o => o.Type == (int)SoeOriginInvoiceMappingType.Order);
                }
                catch
                {
                    // Intentionally ignored, safe to continue
                    // NOSONAR
                }
            }

            if (e.Project != null)
                cidto.ProjectNr = e.Project.Number;

            //get claim account from invoice account rows
            if (claimAccountId == 0 && accountRows != null)
            {
                if (e.BillingType == (int)TermGroup_BillingType.Credit)
                {
                    claimAccountId = accountRows.Where(a => a.CreditRow &&
                                                            !a.VatRow &&
                                                            a.AccountStd.AccountTypeSysTermId == (int)TermGroup_AccountType.Asset &&
                                                            !a.CustomerInvoiceRow.IsCentRoundingRow &&
                                                            !a.CustomerInvoiceRow.IsFreightAmountRow &&
                                                            a.CustomerInvoiceRow.HouseholdDeductionType == null &&
                                                            !a.CustomerInvoiceRow.IsInterestRow &&
                                                            !a.CustomerInvoiceRow.IsInvoiceFeeRow)
                                                            .Select(a => a.AccountId).FirstOrDefault();
                }
                else
                {
                    claimAccountId = accountRows.Where(a => a.DebitRow &&
                                                            !a.VatRow &&
                                                            a.AccountStd.AccountTypeSysTermId == (int)TermGroup_AccountType.Asset &&
                                                            !a.CustomerInvoiceRow.IsCentRoundingRow &&
                                                            !a.CustomerInvoiceRow.IsFreightAmountRow &&
                                                            !a.CustomerInvoiceRow.IsInterestRow &&
                                                            a.CustomerInvoiceRow.HouseholdDeductionType == null &&
                                                            !a.CustomerInvoiceRow.IsInvoiceFeeRow)
                                                            .Select(a => a.AccountId).FirstOrDefault();
                }
            }
            cidto.ClaimAccountId = claimAccountId;

            return cidto;
        }

        #endregion

        #region CustomerInvoiceRowToDTO

        public static CustomerInvoiceRowDTO ToCustomerInvoiceRowDTO(this CustomerInvoiceRow e, CustomerInvoice customerInvoice, List<CustomerInvoiceAccountRow> accountRows, bool includeAccountingRows)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded())
                {
                    if (!e.ProductReference.IsLoaded)
                    {
                        e.ProductReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.ProductReference");
                    }
                    if (!e.ProductUnitReference.IsLoaded)
                    {
                        e.ProductUnitReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.ProductUnitReference");
                    }
                    if (!e.VatCodeReference.IsLoaded)
                    {
                        e.VatCodeReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.VatCodeReference");
                    }
                    if (!e.VatAccountStdReference.IsLoaded)
                    {
                        e.VatAccountStdReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.VatAccountStdReference");
                    }
                    if (e.VatAccountStd != null && !e.VatAccountStd.AccountReference.IsLoaded)
                    {
                        e.VatAccountStd.AccountReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.VatAccountStd.AccountReference");
                    }
                    if (!e.HouseholdTaxDeductionRowReference.IsLoaded)
                    {
                        e.HouseholdTaxDeductionRowReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.HouseholdTaxDeductionRowReference");
                    }
                    if (!e.StockReference.IsLoaded)
                    {
                        e.StockReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.StockReference");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            var dto = new CustomerInvoiceRowDTO
            {
                CustomerInvoiceRowId = e.CustomerInvoiceRowId,
                InvoiceId = customerInvoice?.InvoiceId ?? 0,
                InvoiceNr = customerInvoice?.InvoiceNr ?? "",
                ParentRowId = e.ParentRowId,
                TargetRowId = e.TargetRowId,
                AttestStateId = e.AttestStateId,
                ProductId = e.ProductId,
                ProductUnitId = e.ProductUnitId,
                ProductUnitCode = e.ProductUnit?.Name ?? "",
                VatCodeId = e.VatCodeId,
                VatAccountId = e.VatAccountId,
                CustomerInvoiceInterestId = e.CustomerInvoiceInterestId,
                CustomerInvoiceReminderId = e.CustomerInvoiceReminderId,
                EdiEntryId = e.EdiEntryId,
                RowNr = e.RowNr,
                Type = (SoeInvoiceRowType)e.Type,
                Quantity = e.Quantity,
                InvoiceQuantity = e.InvoiceQuantity ?? 0,
                PreviouslyInvoicedQuantity = e.PreviouslyInvoicedQuantity ?? 0,
                Text = e.Text,
                SysWholesellerName = e.SysWholesellerName,
                Amount = e.Amount,
                AmountCurrency = e.AmountCurrency,
                AmountEntCurrency = e.AmountEntCurrency,
                AmountLedgerCurrency = e.AmountLedgerCurrency,
                DiscountType = e.DiscountType,
                DiscountPercent = e.DiscountPercent,
                DiscountAmount = e.DiscountAmount,
                DiscountAmountCurrency = e.DiscountAmountCurrency,
                DiscountAmountEntCurrency = e.DiscountAmountEntCurrency,
                DiscountAmountLedgerCurrency = e.DiscountAmountLedgerCurrency,
                VatRate = e.VatRate,
                VatAmount = e.VatAmount,
                VatAmountCurrency = e.VatAmountCurrency,
                VatAmountEntCurrency = e.VatAmountEntCurrency,
                VatAmountLedgerCurrency = e.VatAmountLedgerCurrency,
                SumAmount = e.SumAmount,
                SumAmountCurrency = e.SumAmountCurrency,
                SumAmountEntCurrency = e.SumAmountEntCurrency,
                SumAmountLedgerCurrency = e.SumAmountLedgerCurrency,
                PurchasePrice = e.PurchasePrice,
                PurchasePriceCurrency = e.PurchasePriceCurrency,
                PurchasePriceEntCurrency = e.PurchasePriceEntCurrency,
                PurchasePriceLedgerCurrency = e.PurchasePriceLedgerCurrency,
                MarginalIncome = e.MarginalIncome,
                MarginalIncomeCurrency = e.MarginalIncomeCurrency,
                MarginalIncomeEntCurrency = e.MarginalIncomeEntCurrency,
                MarginalIncomeLedgerCurrency = e.MarginalIncomeLedgerCurrency,
                MarginalIncomeRatio = e.MarginalIncomeRatio ?? 0,
                IsFreightAmountRow = e.IsFreightAmountRow,
                IsInvoiceFeeRow = e.IsInvoiceFeeRow,
                IsCentRoundingRow = e.IsCentRoundingRow,
                IsInterestRow = e.IsInterestRow,
                IsReminderRow = e.IsReminderRow,
                IsTimeProjectRow = e.IsTimeProjectRow,
                IsStockRow = e.IsStockRow ?? false,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                Date = e.Date,
                TimeManuallyChanged = e.TimeManuallyChanged,
                StockId = e.StockId ?? 0,
                SupplierInvoiceId = e.SupplierInvoiceId,
                HouseholdDeductionType = e.HouseholdDeductionType,
                DateTo = e.DateTo,
                DeliveryDateText = e.DeliveryDateText,
                IntrastatTransactionId = e.IntrastatTransactionId,
            };

            // Extensions
            if (e.Product != null)
            {
                if (e.Product.GetType() == typeof(InvoiceProduct))
                {
                    dto.IsLiftProduct = ((InvoiceProduct)e.Product).CalculationType == (int)TermGroup_InvoiceProductCalculationType.Lift;
                    dto.IsContractProduct = ((InvoiceProduct)e.Product).CalculationType == (int)TermGroup_InvoiceProductCalculationType.Contract;
                    dto.IsClearingProduct = ((InvoiceProduct)e.Product).CalculationType == (int)TermGroup_InvoiceProductCalculationType.Clearing;
                    dto.IsFixedPriceProduct = ((InvoiceProduct)e.Product).CalculationType == (int)TermGroup_InvoiceProductCalculationType.FixedPrice;
                    dto.SysCountryId = ((InvoiceProduct)e.Product).SysCountryId;
                    dto.IntrastatCodeId = ((InvoiceProduct)e.Product).IntrastatCodeId;
                }
                dto.ProductNr = e.Product.Number;
                dto.ProductName = e.Product.Name;
                dto.IsSupplementChargeProduct = (e.Product as InvoiceProduct).IsSupplementCharge;
            }

            dto.ProductUnitCode = e.ProductUnit?.Code ?? string.Empty;

            if (e.VatCode != null)
                dto.VatCodeCode = e.VatCode.Code;
            if (e.VatAccountStd != null && e.VatAccountStd.Account != null)
            {
                dto.VatAccountNr = e.VatAccountStd.Account.AccountNr;
                dto.VatAccountName = e.VatAccountStd.Account.Name;
            }

            if (e.Stock != null)
                dto.StockCode = e.Stock.Code;

            // HouseholdTaxDeduction
            if (e.HouseholdTaxDeductionRow != null)
            {
                dto.HouseholdProperty = e.HouseholdTaxDeductionRow.Property;
                dto.HouseholdSocialSecNbr = e.HouseholdTaxDeductionRow.SocialSecNr;
                dto.HouseholdName = e.HouseholdTaxDeductionRow.Name;
                dto.HouseholdAmount = e.HouseholdTaxDeductionRow.Amount;
                dto.HouseholdAmountCurrency = e.HouseholdTaxDeductionRow.AmountCurrency;
                dto.HouseholdApartmentNbr = e.HouseholdTaxDeductionRow.ApartmentNr;
                dto.HouseholdCooperativeOrgNbr = e.HouseholdTaxDeductionRow.CooperativeOrgNr;
                dto.HouseholdApplied = e.HouseholdTaxDeductionRow.Applied;
                dto.HouseholdAppliedDate = e.HouseholdTaxDeductionRow.AppliedDate;
                dto.HouseholdReceived = e.HouseholdTaxDeductionRow.Received;
                dto.HouseholdReceivedDate = e.HouseholdTaxDeductionRow.ReceivedDate;
                dto.HouseHoldTaxDeductionType = e.HouseholdTaxDeductionRow.HouseHoldTaxDeductionType;
            }

            // Accounting rows
            if (includeAccountingRows)
            {
                dto.AccountingRows = new List<AccountingRowDTO>();
                foreach (var row in accountRows)
                {
                    dto.AccountingRows.Add(row.AccountingRowDTO(e));
                }
            }

            return dto;
        }

        public static CustomerInvoiceRowDetailDTO ToCustomerInvoiceRowDetailDTO(this CustomerInvoiceRow e)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded())
                {
                    if (!e.ProductReference.IsLoaded)
                    {
                        e.ProductReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.ProductReference");
                    }

                    if (!e.ProductUnitReference.IsLoaded)
                    {
                        e.ProductUnitReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.ProductUnitReference");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            var dto = new CustomerInvoiceRowDetailDTO()
            {
                CustomerInvoiceRowId = e.CustomerInvoiceRowId,
                InvoiceId = e.InvoiceId,    // TODO: Add foreign key to model                
                AttestStateId = e.AttestStateId,
                EdiEntryId = e.EdiEntryId,
                ProductId = e.ProductId,
                ProductUnitCode = e.ProductUnit != null ? e.ProductUnit.Name : "",
                RowNr = e.RowNr,
                Type = (SoeInvoiceRowType)e.Type,
                Quantity = e.Quantity,
                PreviouslyInvoicedQuantity = e.PreviouslyInvoicedQuantity.GetValueOrDefault(),
                Text = e.Text,
                AmountCurrency = e.AmountCurrency,
                SumAmountCurrency = e.SumAmountCurrency,
                EdiTextValue = string.Empty,
                DiscountType = e.DiscountType,
                DiscountValue = e.DiscountPercent,
                FromDate = e.Date,
                ToDate = e.DateTo,
                IsTimeProjectRow = e.IsTimeProjectRow,
                IsExpenseRow = e.IsExpense(),
                IsTimeBillingRow = e.IsTimeBillingRow()
            };

            // Extensions
            if (e.Product != null)
            {
                dto.ProductNr = e.Product.Number;
                dto.ProductName = e.Product.Name;
            }
            dto.ProductUnitCode = e.ProductUnit != null ? e.ProductUnit.Code : string.Empty;

            return dto;
        }

        public static HandleBillingRowDTO ToHandleBillingRowDTO(this HandleBillingCustomerInvoiceRowView e, int marginalIncomeLimit)
        {
            if (e == null)
                return null;

            HandleBillingRowDTO dto = new HandleBillingRowDTO()
            {
                CustomerInvoiceRowId = e.CustomerInvoiceRowId,
                RowNr = e.RowNr,
                Type = (SoeInvoiceRowType)e.Type,
                Quantity = e.Quantity,
                InvoiceQuantity = e.InvoiceQuantity != null ? e.InvoiceQuantity : 0,
                PreviouslyInvoicedQuantity = e.PreviouslyInvoicedQuantity != null ? e.PreviouslyInvoicedQuantity : 0,
                DiscountType = e.DiscountType,
                DiscountPercent = e.DiscountPercent,
                //Amount = e.Amount,
                AmountCurrency = e.AmountCurrency,
                SumAmount = e.SumAmount,
                SumAmountCurrency = e.SumAmountCurrency,
                Text = e.Text,
                Status = e.Status,
                Created = e.Created,
                EdiEntryId = e.EdiEntryId,
                VatAmount = e.VatAmount,
                VatAmountCurrency = e.VatAmountCurrency,
                DiscountAmount = e.DiscountAmount,
                DiscountAmountCurrency = e.DiscountAmountCurrency,
                PurchasePrice = e.PurchasePrice,
                PurchasePriceCurrency = e.PurchasePriceCurrency,
                MarginalIncome = e.MarginalIncome,
                MarginalIncomeCurrency = e.MarginalIncomeCurrency,
                MarginalIncomeRatio = e.MarginalIncomeRatio ?? 0,
                IsTimeProjectRow = e.IsTimeProjectRow,
                TimeManuallyChanged = e.TimeManuallyChanged,
                Date = e.Date,
                HouseholdDeductionType = e.HouseholdDeductionType,
                IsStockRow = e.IsStockRow ?? false,
                ProductRowType = (SoeProductRowType)e.ProductRowType,
                Description = e.Description,
                InvoiceId = e.InvoiceId,
                InvoiceNr = e.InvoiceNr,
                CurrencyId = e.CurrencyId,

                ActorCustomerId = e.ActorCustomerId,
                Customer = e.CustomerNr + " - " + e.CustomerName,

                ProjectId = e.ProjectId,
                ProjectNr = e.ProjectNr,
                Project = e.ProjectNr + " - " + e.ProjectName,

                AttestStateId = e.AttestStateId,
                AttestStateName = e.AttestStateName,
                AttestStateColor = e.AttestStateColor,

                ProductId = e.ProductId,
                ProductNr = e.ProductNr,
                ProductName = e.ProductName,
                ProductCalculationType = (TermGroup_InvoiceProductCalculationType)e.ProductCalculationType,

                ProductUnitCode = e.ProductUnitName,

                EdiTextValue = String.Empty,

                MarginalIncomeLimit = marginalIncomeLimit,
            };

            return dto;
        }

        public static ProductRowDTO ToProductRowDTO(this CustomerInvoiceRow e)
        {
            if (e == null)
                return null;

            var dto = new ProductRowDTO
            {
                CustomerInvoiceRowId = e.CustomerInvoiceRowId,
                TempRowId = e.TempRowId,
                ParentRowId = e.ParentRowId,
                ProductId = e.ProductId,
                ProductUnitId = e.ProductUnitId,
                AttestStateId = e.AttestStateId,
                VatCodeId = e.VatCodeId,
                VatAccountId = e.VatAccountId,
                EdiEntryId = e.EdiEntryId,
                SupplierInvoiceId = e.SupplierInvoiceId,
                StockId = e.StockId,
                StockCode = e.Stock?.Code,
                HouseholdDeductionType = e.HouseholdDeductionType,
                RowNr = e.RowNr,
                Type = (SoeInvoiceRowType)e.Type,
                Quantity = e.Quantity,
                InvoiceQuantity = e.InvoiceQuantity,
                PreviouslyInvoicedQuantity = e.PreviouslyInvoicedQuantity,
                Text = e.Text,
                DeliveryDateText = e.DeliveryDateText,
                SysWholesellerName = e.SysWholesellerName,
                Amount = e.Amount,
                AmountCurrency = e.AmountCurrency,
                DiscountType = e.DiscountType,
                DiscountPercent = e.DiscountPercent,
                DiscountAmount = e.DiscountAmount,
                DiscountAmountCurrency = e.DiscountAmountCurrency,
                Discount2Type = e.Discount2Type,
                Discount2Percent = e.Discount2Percent,
                Discount2Amount = e.Discount2Amount,
                Discount2AmountCurrency = e.Discount2AmountCurrency,
                DiscountValue = e.DiscountType == (int)SoeInvoiceRowDiscountType.Percent ? e.DiscountPercent : e.DiscountAmountCurrency,
                Discount2Value = e.Discount2Type == (int)SoeInvoiceRowDiscountType.Percent ? e.Discount2Percent : e.Discount2AmountCurrency,
                VatRate = e.VatRate,
                VatAmount = e.VatAmount,
                VatAmountCurrency = e.VatAmountCurrency,
                SumAmount = e.SumAmount,
                SumAmountCurrency = e.SumAmountCurrency,
                PurchasePrice = e.PurchasePrice,
                PurchasePriceCurrency = e.PurchasePriceCurrency,
                MarginalIncome = e.MarginalIncome,
                MarginalIncomeCurrency = e.MarginalIncomeCurrency,
                MarginalIncomeRatio = e.MarginalIncomeRatio ?? 0,
                Date = e.Date,
                DateTo = e.DateTo,
                IsFreightAmountRow = e.IsFreightAmountRow,
                IsInvoiceFeeRow = e.IsInvoiceFeeRow,
                IsCentRoundingRow = e.IsCentRoundingRow,
                IsInterestRow = e.IsInterestRow,
                IsReminderRow = e.IsReminderRow,
                IsTimeProjectRow = e.IsTimeProjectRow,
                IsStockRow = e.IsStockRow ?? false,
                IsExpenseRow = e.IsExpense(),
                IsTimeBillingRow = e.IsTimeBillingRow(),
                TimeManuallyChanged = e.TimeManuallyChanged,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                CustomerInvoiceInterestId = e.CustomerInvoiceInterestId,
                CustomerInvoiceReminderId = e.CustomerInvoiceReminderId,
                IntrastatTransactionId = e.IntrastatTransactionId,
            };

            // HouseholdTaxDeduction
            if (e.HouseholdTaxDeductionRow != null)
            {
                dto.HouseholdProperty = e.HouseholdTaxDeductionRow.Property;
                dto.HouseholdSocialSecNbr = e.HouseholdTaxDeductionRow.SocialSecNr;
                dto.HouseholdName = e.HouseholdTaxDeductionRow.Name;
                dto.HouseholdAmount = e.HouseholdTaxDeductionRow.Amount;
                dto.HouseholdAmountCurrency = e.HouseholdTaxDeductionRow.AmountCurrency;
                dto.HouseholdApartmentNbr = e.HouseholdTaxDeductionRow.ApartmentNr;
                dto.HouseholdCooperativeOrgNbr = e.HouseholdTaxDeductionRow.CooperativeOrgNr;
                dto.HouseholdTaxDeductionType = (TermGroup_HouseHoldTaxDeductionType)e.HouseholdTaxDeductionRow.HouseHoldTaxDeductionType;
            }

            if (e.Product != null && (e.Type == (int)SoeInvoiceRowType.ProductRow || e.Type == (int)SoeInvoiceRowType.BaseProductRow))
            {
                dto.SysCountryId = ((InvoiceProduct)e.Product).SysCountryId;
                dto.IntrastatCodeId = ((InvoiceProduct)e.Product).IntrastatCodeId;
            }

            return dto;
        }

        public static List<ProductRowDTO> ToProductRowDTOs(this IEnumerable<CustomerInvoiceRow> l)
        {
            var dtos = new List<ProductRowDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToProductRowDTO());
                }
            }
            return dtos;
        }


        #endregion 

        #region CustomerInvoiceInterestToDTO

        public static CustomerInvoiceInterestDTO ToDTO(this CustomerInvoiceInterest e)
        {
            if (e == null)
                return null;

            CustomerInvoiceInterestDTO dto = new CustomerInvoiceInterestDTO()
            {
                CustomerInvoiceInterestId = e.CustomerInvoiceInterestId,
                CustomerInvoiceOriginId = e.CustomerInvoiceOrigin != null && e.CustomerInvoiceOrigin.Origin != null ? e.CustomerInvoiceOrigin.Origin.OriginId : 0,  // TODO: Add foreign key to model
                InvoiceProductId = e.InvoiceProduct != null ? e.InvoiceProduct.ProductId : 0,   // TODO: Add foreign key to model
                BatchId = e.BatchId,
                Type = (SoeInvoiceInterestHandlingType)e.Type,
                InvoiceNr = e.InvoiceNr,
                DueDate = e.DueDate,
                PayDate = e.PayDate,
                Amount = e.Amount,
                AmountCurrency = e.AmountCurrency,
                AmountEntCurrency = e.AmountEntCurrency,
                AmountLedgerCurrency = e.AmountLedgerCurrency,
                Created = e.Created,
                CreatedBy = e.CreatedBy
            };

            // Extensions
            if (e.CustomerInvoiceOrigin != null && e.CustomerInvoiceOrigin.Actor != null && e.CustomerInvoiceOrigin.Actor.Customer != null)
                dto.CustomerName = e.CustomerInvoiceOrigin.Actor.Customer.Name;

            return dto;
        }

        public static IEnumerable<CustomerInvoiceInterestDTO> ToDTOs(this IEnumerable<CustomerInvoiceInterest> l)
        {
            var dtos = new List<CustomerInvoiceInterestDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region CustomerInvoiceReminderToDTO

        public static CustomerInvoiceReminderDTO ToDTO(this CustomerInvoiceReminder e)
        {
            if (e == null)
                return null;

            CustomerInvoiceReminderDTO dto = new CustomerInvoiceReminderDTO()
            {
                CustomerInvoiceReminderId = e.CustomerInvoiceReminderId,
                CustomerInvoiceOriginId = e.CustomerInvoiceOrigin != null && e.CustomerInvoiceOrigin.Origin != null ? e.CustomerInvoiceOrigin.Origin.OriginId : 0,  // TODO: Add foreign key to model
                InvoiceProductId = e.InvoiceProduct != null ? e.InvoiceProduct.ProductId : 0,   // TODO: Add foreign key to model
                BatchId = e.BatchId,
                Type = (SoeInvoiceReminderHandlingType)e.Type,
                InvoiceNr = e.InvoiceNr,
                DueDate = e.DueDate,
                NoOfReminder = e.NoOfReminder,
                Amount = e.Amount,
                AmountCurrency = e.AmountCurrency,
                AmountEntCurrency = e.AmountEntCurrency,
                AmountLedgerCurrency = e.AmountLedgerCurrency,
                Created = e.Created,
                CreatedBy = e.CreatedBy
            };

            // Extensions
            if (e.CustomerInvoiceOrigin != null && e.CustomerInvoiceOrigin.Actor != null && e.CustomerInvoiceOrigin.Actor.Customer != null)
                dto.CustomerName = e.CustomerInvoiceOrigin.Actor.Customer.Name;

            return dto;
        }

        public static IEnumerable<CustomerInvoiceReminderDTO> ToDTOs(this IEnumerable<CustomerInvoiceReminder> l)
        {
            var dtos = new List<CustomerInvoiceReminderDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static CustomerInvoicePrintedReminderDTO ToDTO(this CustomerInvoicePrintedReminder e)
        {
            if (e == null)
                return null;

            CustomerInvoicePrintedReminderDTO dto = new CustomerInvoicePrintedReminderDTO()
            {
                CustomerInvoiceReminderId = e.CustomerInvoicePrintedReminderId,
                ActorCompanyId = e.ActorCompanyId,
                CustomerInvoiceOriginId = e.CustomerInvoiceOriginId,
                InvoiceNr = e.InvoiceNr,
                Amount = e.Amount,
                ReminderDate = e.ReminderDate,
                DueDate = e.DueDate,
                NoOfReminder = e.NoOfReminder,
                Created = e.Created,
                CreatedBy = e.CreatedBy
            };

            return dto;
        }

        #endregion

        #region OrderDTO

        public static bool IsExpense(this CustomerInvoiceRow e)
        {
            return ((SoeProductRowType)(e.ProductRowType)).HasFlag(SoeProductRowType.ExpenseRow);
        }

        public static bool IsTimeBillingRow(this CustomerInvoiceRow e)
        {
            return ((SoeProductRowType)(e.ProductRowType)).HasFlag(SoeProductRowType.TimeBillingRow);
        }

        public static bool IsHouseholdTaxDeductionRow(this CustomerInvoiceRow e)
        {
            return ((SoeProductRowType)(e.ProductRowType)).HasFlag(SoeProductRowType.HouseholdTaxDeduction);
        }

        public static void LoadHouseholdTaxDeductionRowReference(this CustomerInvoiceRow e)
        {
            if (e.IsHouseholdTaxDeductionRow() && !e.HouseholdTaxDeductionRowReference.IsLoaded)
                e.HouseholdTaxDeductionRowReference.Load();
        }

        public static OrderDTO ToOrderDTO(this CustomerInvoice e, bool includeRows)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded())
                {
                    if (!e.OriginReference.IsLoaded)
                    {
                        e.OriginReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.OriginReference");
                    }
                    if (e.Origin != null && !e.Origin.OriginUser.IsLoaded)
                    {
                        e.Origin.OriginUser.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.Origin.OriginUser");
                    }
                    foreach (var originUser in e.Origin.OriginUser)
                    {
                        if (!originUser.UserReference.IsLoaded)
                        {
                            originUser.UserReference.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("originUser.UserReference");
                        }
                    }

                    if (!e.ProjectReference.IsLoaded)
                    {
                        e.ProjectReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("originUser.UserReference");
                    }

                    if (!e.InvoiceMapping.IsLoaded)
                    {
                        e.InvoiceMapping.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.InvoiceMapping");
                    }

                    if (!e.InvoiceMapping1.IsLoaded)
                    {
                        e.InvoiceMapping1.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.InvoiceMapping1");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            var dto = new OrderDTO()
            {
                InvoiceId = e.InvoiceId,
                ActorId = e.ActorId,
                DeliveryCustomerId = e.DeliveryCustomerId,
                ContactEComId = e.ContactEComId,
                ContactGLNId = e.ContactGLNId,
                ProjectId = e.ProjectId,
                PaymentConditionId = e.PaymentConditionId,
                DeliveryTypeId = e.DeliveryTypeId,
                DeliveryConditionId = e.DeliveryConditionId,
                DeliveryAddressId = e.DeliveryAddressId,
                BillingAddressId = e.BillingAddressId,
                PriceListTypeId = e.PriceListTypeId,
                SysWholeSellerId = e.SysWholeSellerId,
                DefaultDim1AccountId = e.DefaultDim1AccountId,
                DefaultDim2AccountId = e.DefaultDim2AccountId,
                DefaultDim3AccountId = e.DefaultDim3AccountId,
                DefaultDim4AccountId = e.DefaultDim4AccountId,
                DefaultDim5AccountId = e.DefaultDim5AccountId,
                DefaultDim6AccountId = e.DefaultDim6AccountId,
                BillingType = (TermGroup_BillingType)e.BillingType,
                VatType = (TermGroup_InvoiceVatType)e.VatType,
                OrderType = (TermGroup_OrderType)e.OrderType,
                InvoiceNr = e.InvoiceNr,
                SeqNr = e.SeqNr,
                //OCR = e.OCR,
                InvoiceText = e.InvoiceText,
                InvoiceHeadText = e.InvoiceHeadText,
                InvoiceLabel = e.InvoiceLabel,
                OrderReference = e.OrderReference,
                //InternalDescription = e.InternalDescription,
                //ExternalDescription = e.ExternalDescription,
                WorkingDescription = e.WorkingDescription,
                BillingAdressText = e.BillingAdressText,
                DeliveryDateText = e.DeliveryDateText,
                CurrencyId = e.CurrencyId,
                CurrencyRate = e.CurrencyRate,
                CurrencyDate = e.CurrencyDate,
                InvoiceDate = e.InvoiceDate,
                DueDate = e.DueDate,
                VoucherDate = e.VoucherDate,
                OrderDate = e.OrderDate,
                DeliveryDate = e.DeliveryDate,
                ReferenceOur = e.ReferenceOur,
                ReferenceYour = e.ReferenceYour,
                TotalAmount = e.TotalAmount,
                TotalAmountCurrency = e.TotalAmountCurrency,
                VatAmount = e.VATAmount,
                VatAmountCurrency = e.VATAmountCurrency,
                RemainingAmount = e.RemainingAmount,
                RemainingAmountExVat = e.RemainingAmountExVat,
                CentRounding = e.CentRounding,
                FreightAmount = e.FreightAmount,
                FreightAmountCurrency = e.FreightAmountCurrency,
                InvoiceFee = e.InvoiceFee,
                InvoiceFeeCurrency = e.InvoiceFeeCurrency,
                SumAmount = e.SumAmount,
                SumAmountCurrency = e.SumAmountCurrency,
                MarginalIncomeCurrency = e.MarginalIncomeCurrency,
                MarginalIncomeRatio = e.MarginalIncomeRatio,
                IsTemplate = e.IsTemplate,
                //ManuallyAdjustedAccounting = e.ManuallyAdjustedAccounting,
                //HasHouseholdTaxDeduction = e.HasHouseholdTaxDeduction,
                FixedPriceOrder = e.FixedPriceOrder,
                //MultipleAssetRows = e.MultipleAssetRows,
                //InsecureDebt = e.InsecureDebt,
                PrintTimeReport = e.PrintTimeReport,
                //BillingInvoicePrinted = e.BillingInvoicePrinted,
                //CashSale = e.CashSale,
                AddAttachementsToEInvoice = e.AddAttachementsToEInvoice,
                StatusIcon = (SoeStatusIcon)e.StatusIcon,
                ShiftTypeId = e.ShiftTypeId,
                PlannedStartDate = e.PlannedStartDate,
                PlannedStopDate = e.PlannedStopDate,
                EstimatedTime = e.EstimatedTime,
                RemainingTime = e.RemainingTime,
                Priority = e.Priority,
                KeepAsPlanned = e.KeepAsPlanned,
                InvoiceDeliveryType = e.InvoiceDeliveryType,
                InvoicePaymentService = e.InvoicePaymentService,
                IncludeOnInvoice = e.IncludeOnInvoice,
                IncludeOnlyInvoicedTime = e.IncludeOnlyInvoicedTime,
                includeExpenseInReport = (TermGroup_IncludeExpenseInReportType)e.IncludeExpenseInReport,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                //State = (SoeEntityState)e.State,
                NbrOfChecklists = e.NbrOfChecklists,
                TriangulationSales = e.TriangulationSales,
                TransferAttachments = e.TransferAttachments ?? false,
                ContractGroupId = e.ContractGroupId,
                NextContractPeriodDate = e.NextContractPeriodDate,
                NextContractPeriodValue = e.NextContractPeriodValue,
                NextContractPeriodYear = e.NextContractPeriodYear,
                AddSupplierInvoicesToEInvoice = e.AddSupplierInvoicesToEInvoices,
                EdiTransferMode = (TermGroup_OrderEdiTransferMode)e.EdiTransferMode,
                CustomerName = e.CustomerName,
                CustomerEmail = e.CustomerEmail,
                CustomerPhoneNr = e.CustomerPhoneNr,
                ContractNr = e.ContractNr,
                OrderInvoiceTemplateId = e.OrderInvoiceTemplateId,
                ShowNote = e.ShowNote,
            };

            // Extensions
            dto.OriginStatusName = e.StatusName;
            if (e.Origin != null)
            {
                dto.OriginStatus = (SoeOriginStatus)e.Origin.Status;
                dto.OriginDescription = e.Origin.Description;
                dto.VoucherSeriesId = e.Origin.VoucherSeriesId;

                dto.OriginUsers = new List<OriginUserSmallDTO>();
                if (e.Origin.OriginUser != null)
                {
                    foreach (var user in e.Origin.OriginUser.Where(u => u.State == (int)SoeEntityState.Active).OrderByDescending(u => u.Main).ThenBy(u => u.User.Name))
                    {
                        dto.OriginUsers.Add(user.ToSmallDTO());
                    }
                }
            }
            else
                dto.OriginStatus = SoeOriginStatus.None;

            dto.ProjectNr = e.ProjectNr;
            if (e.Project != null)
                dto.ProjectIsActive = (e.Project.Status == (int)TermGroup_ProjectStatus.Active || e.Project.Status == (int)TermGroup_ProjectStatus.Guarantee);
            else
                dto.ProjectIsActive = false;

            dto.CustomerBlockNote = e.CustomerBlockNote;

            dto.CategoryIds = e.CategoryIds;

            dto.IsMainInvoice = e.InvoiceMapping.Any();

            var headInvoice = e.InvoiceMapping1.FirstOrDefault();
            if (headInvoice != null)
            {
                dto.MainInvoiceId = headInvoice.MainInvoiceId;
                dto.MainInvoiceNr = headInvoice.Invoice.InvoiceNr;
                dto.MainInvoice = headInvoice.Invoice.InvoiceNr + " - " + headInvoice.Invoice.Actor.Customer.CustomerNr + " " + headInvoice.Invoice.Actor.Customer.Name;
            }

            if (includeRows)
            {
                dto.CustomerInvoiceRows = new List<ProductRowDTO>();
                if (e.CustomerInvoiceRow != null && e.CustomerInvoiceRow.Count > 0)
                    dto.CustomerInvoiceRows = e.CustomerInvoiceRow.Where(r => r.State == (int)SoeEntityState.Active).ToProductRowDTOs();
            }

            return dto;
        }


        public readonly static Expression<Func<CustomerInvoice, OrderListDTO>> GetOrderListDTO =
         i => new OrderListDTO
         {
             OrderId = i.InvoiceId,
             OrderNr = i.SeqNr,
             CustomerId = i.ActorId ?? 0,
             CustomerNr = i.Actor.Customer.CustomerNr,
             CustomerName = i.Actor.Customer.Name,
             ProjectId = i.ProjectId,
             ProjectNr = i.Project.Number,
             ProjectName = i.Project.Name,
             ShiftTypeId = i.ShiftTypeId,
             ShiftTypeName = i.ShiftType.Name,
             ShiftTypeColor = i.ShiftType.Color,
             Priority = i.Priority,
             PlannedStartDate = i.PlannedStartDate,
             PlannedStopDate = i.PlannedStopDate,
             EstimatedTime = i.EstimatedTime,
             RemainingTime = i.RemainingTime,
             KeepAsPlanned = i.KeepAsPlanned,
             WorkingDescription = i.WorkingDescription,
             InternalDescription = i.Origin.Description ?? String.Empty,
             DeliveryAddressId = i.DeliveryAddressId,
             InvoiceHeadText = i.InvoiceHeadText,
         };

        #endregion

        #region CustomerInvoiceAccountRow

        public static SplitAccountingRowDTO ToSplitDTO(this CustomerInvoiceAccountRow e)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded())
                {
                    if (!e.AccountStdReference.IsLoaded)
                    {
                        e.AccountStdReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.AccountStdReference");
                    }
                    if (e.AccountStd != null && !e.AccountStd.AccountReference.IsLoaded)
                    {
                        e.AccountStd.AccountReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.AccountStd.AccountReference");
                    }
                    if (!e.AccountInternal.IsLoaded)
                    {
                        e.AccountInternal.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.AccountInternal");
                    }
                    if (e.AccountInternal != null)
                    {
                        foreach (var accInt in e.AccountInternal)
                        {
                            if (!accInt.AccountReference.IsLoaded)
                            {
                                accInt.AccountReference.Load();
                                DataProjectLogCollector.LogLoadedEntityInExtension("accInt.AccountReference");
                            }
                            if (accInt.Account != null && !accInt.Account.AccountDimReference.IsLoaded)
                            {
                                accInt.Account.AccountDimReference.Load();
                                DataProjectLogCollector.LogLoadedEntityInExtension("accInt.Account.AccountDimReferenc");
                            }
                        }
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            SplitAccountingRowDTO dto = new SplitAccountingRowDTO()
            {
                InvoiceAccountRowId = e.CustomerInvoiceAccountRowId,
                SplitType = e.SplitType,
                SplitPercent = e.SplitPercent,
                SplitValue = e.SplitPercent,
                AmountCurrency = e.AmountCurrency,
                CreditAmountCurrency = e.AmountCurrency < 0 ? Math.Abs(e.AmountCurrency) : 0,
                DebitAmountCurrency = e.AmountCurrency > 0 ? e.AmountCurrency : 0,
                IsCreditRow = e.CreditRow,
                IsDebitRow = e.DebitRow,
            };

            #region Accounts

            AccountStd accStd = e.AccountStd;
            if (accStd != null)
            {
                dto.Dim1Id = accStd.AccountId;
                dto.Dim1Nr = accStd.Account != null ? accStd.Account.AccountNr : String.Empty;
                dto.Dim1Name = accStd.Account != null ? accStd.Account.Name : String.Empty;
                dto.Dim1Disabled = false;
                dto.Dim1Mandatory = true;
            }

            if (e.AccountInternal != null)
            {
                foreach (AccountInternal accInt in e.AccountInternal)
                {
                    if (accInt.Account != null && accInt.Account.AccountDim != null)
                    {
                        switch (accInt.Account.AccountDim.AccountDimNr)
                        {
                            case 2:
                                dto.Dim2Id = accInt.AccountId;
                                dto.Dim2Nr = accInt.Account.AccountNr;
                                dto.Dim2Name = accInt.Account.Name;
                                break;
                            case 3:
                                dto.Dim3Id = accInt.AccountId;
                                dto.Dim3Nr = accInt.Account.AccountNr;
                                dto.Dim3Name = accInt.Account.Name;
                                break;
                            case 4:
                                dto.Dim4Id = accInt.AccountId;
                                dto.Dim4Nr = accInt.Account.AccountNr;
                                dto.Dim4Name = accInt.Account.Name;
                                break;
                            case 5:
                                dto.Dim5Id = accInt.AccountId;
                                dto.Dim5Nr = accInt.Account.AccountNr;
                                dto.Dim5Name = accInt.Account.Name;
                                break;
                            case 6:
                                dto.Dim6Id = accInt.AccountId;
                                dto.Dim6Nr = accInt.Account.AccountNr;
                                dto.Dim6Name = accInt.Account.Name;
                                break;
                        }
                    }
                }
            }

            #endregion

            return dto;
        }

        public static List<SplitAccountingRowDTO> ToSplitDTOs(this IEnumerable<CustomerInvoiceAccountRow> l)
        {
            var dtos = new List<SplitAccountingRowDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToSplitDTO());
                }
            }
            return dtos;
        }

        public static List<CustomerInvoiceAccountRowDTO> ToDTOs(this IEnumerable<CustomerInvoiceAccountRow> l, List<AccountDim> dims)
        {
            var dtos = new List<CustomerInvoiceAccountRowDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(dims));
                }
            }
            return dtos;
        }

        public static CustomerInvoiceAccountRowDTO ToDTO(this CustomerInvoiceAccountRow e, List<AccountDim> dims)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded())
                {
                    if (!e.AccountStdReference.IsLoaded)
                    {
                        e.AccountStdReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.AccountStdReference");
                    }
                    if (e.AccountStd != null && !e.AccountStd.AccountReference.IsLoaded)
                    {
                        e.AccountStd.AccountReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.AccountStd.AccountReference");
                    }
                    if (!e.AccountInternal.IsLoaded)
                    {
                        e.AccountInternal.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.AccountInternal");
                    }
                    if (e.AccountInternal != null)
                    {
                        foreach (var accInt in e.AccountInternal)
                        {
                            if (!accInt.AccountReference.IsLoaded)
                            {
                                accInt.AccountReference.Load();
                                DataProjectLogCollector.LogLoadedEntityInExtension("accInt.AccountReference");
                            }
                            if (accInt.Account != null && !accInt.Account.AccountDimReference.IsLoaded)
                            {
                                accInt.Account.AccountDimReference.Load();
                                DataProjectLogCollector.LogLoadedEntityInExtension("accInt.Account.AccountDimReference");
                            }
                        }
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            CustomerInvoiceAccountRowDTO dto = new CustomerInvoiceAccountRowDTO()
            {
                Type = AccountingRowType.AccountingRow,
                InvoiceRowId = e.CustomerInvoiceRowId,
                TempInvoiceRowId = e.CustomerInvoiceRowId,
                InvoiceAccountRowId = e.CustomerInvoiceAccountRowId,
                TempRowId = e.CustomerInvoiceAccountRowId,
                RowNr = e.RowNr,
                Quantity = e.Quantity,
                Text = e.Text,
                Amount = e.Amount,
                AmountCurrency = e.AmountCurrency,
                AmountEntCurrency = e.AmountEntCurrency,
                AmountLedgerCurrency = e.AmountLedgerCurrency,
                CreditAmount = e.Amount < 0 ? Math.Abs(e.Amount) : 0,
                CreditAmountCurrency = e.AmountCurrency < 0 ? Math.Abs(e.AmountCurrency) : 0,
                CreditAmountEntCurrency = e.AmountEntCurrency < 0 ? Math.Abs(e.AmountEntCurrency) : 0,
                CreditAmountLedgerCurrency = e.AmountLedgerCurrency < 0 ? Math.Abs(e.AmountLedgerCurrency) : 0,
                DebitAmount = e.Amount > 0 ? e.Amount : 0,
                DebitAmountCurrency = e.AmountCurrency > 0 ? e.AmountCurrency : 0,
                DebitAmountEntCurrency = e.AmountEntCurrency > 0 ? e.AmountEntCurrency : 0,
                DebitAmountLedgerCurrency = e.AmountLedgerCurrency > 0 ? e.AmountLedgerCurrency : 0,
                SplitType = e.SplitType,
                SplitPercent = e.SplitPercent,
                IsCreditRow = e.CreditRow,
                IsDebitRow = e.DebitRow,
                IsVatRow = e.VatRow,
                IsContractorVatRow = e.ContractorVatRow,
                //IsCentRoundingRow = invoiceRow != null ? invoiceRow.IsCentRoundingRow : false,
                //IsHouseholdRow = invoiceRow != null && invoiceRow.HouseholdTaxDeductionRow != null && invoiceRow.HouseholdTaxDeductionRow.AmountCurrency != 0,
                //IsClaimRow = invoiceRow != null && invoiceRow.CustomerInvoice != null && !invoiceRow.CustomerInvoice.ManuallyAdjustedAccounting && invoiceRow.Product == null,
                //IsManuallyAdjusted = invoiceRow != null && invoiceRow.CustomerInvoice != null && invoiceRow.CustomerInvoice.ManuallyAdjustedAccounting && invoiceRow.Product == null && invoiceRow.SumAmount == 0,
                IsModified = false,
                IsDeleted = (e.State == (int)SoeEntityState.Deleted),
                State = (SoeEntityState)e.State
            };

            #region Accounts

            AccountStd accStd = e.AccountStd;
            if (accStd != null)
            {
                dto.Dim1Id = accStd.AccountId;
                dto.Dim1Nr = accStd.Account != null ? accStd.Account.AccountNr : string.Empty;
                dto.Dim1Name = accStd.Account != null ? accStd.Account.Name : string.Empty;
                dto.Dim1Disabled = false;
                dto.Dim1Mandatory = true;
            }

            if (e.AccountInternal != null && dims.Any())
            {
                foreach (AccountInternal accInt in e.AccountInternal)
                {
                    if (accInt.Account != null && accInt.Account.AccountDim != null)
                    {
                        var dimPos = dims.IndexOf(dims.FirstOrDefault(x => x.AccountDimNr == accInt.Account.AccountDim.AccountDimNr)) + 1;

                        switch (dimPos)
                        {
                            case 2:
                                dto.Dim2Id = accInt.AccountId;
                                dto.Dim2Nr = accInt.Account.AccountNr;
                                dto.Dim2Name = accInt.Account.Name;
                                break;
                            case 3:
                                dto.Dim3Id = accInt.AccountId;
                                dto.Dim3Nr = accInt.Account.AccountNr;
                                dto.Dim3Name = accInt.Account.Name;
                                break;
                            case 4:
                                dto.Dim4Id = accInt.AccountId;
                                dto.Dim4Nr = accInt.Account.AccountNr;
                                dto.Dim4Name = accInt.Account.Name;
                                break;
                            case 5:
                                dto.Dim5Id = accInt.AccountId;
                                dto.Dim5Nr = accInt.Account.AccountNr;
                                dto.Dim5Name = accInt.Account.Name;
                                break;
                            case 6:
                                dto.Dim6Id = accInt.AccountId;
                                dto.Dim6Nr = accInt.Account.AccountNr;
                                dto.Dim6Name = accInt.Account.Name;
                                break;
                        }
                    }
                }
            }

            #endregion

            //// Account distribution / inventory
            //public int AccountDistributionHeadId { get; set; }
            //public int AccountDistributionNbrOfPeriods { get; set; }
            //public DateTime? AccountDistributionStartDate { get; set; }

            //public int InventoryId { get; set; }

            return dto;
        }
        public static AccountingRowDTO AccountingRowDTO(this CustomerInvoiceAccountRow e, CustomerInvoiceRow invoiceRow)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded())
                {
                    if (e.CustomerInvoiceRow != null)
                    {
                        if (!invoiceRow.ProductReference.IsLoaded)
                        {
                            invoiceRow.ProductReference.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("invoiceRow.ProductReference");
                        }
                        if (!invoiceRow.CustomerInvoiceReference.IsLoaded)
                        {
                            invoiceRow.CustomerInvoiceReference.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("invoiceRow.CustomerInvoiceReference");
                        }
                    }
                    if (!e.AccountStdReference.IsLoaded)
                    {
                        e.AccountStdReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.AccountStdReference");
                    }
                    if (e.AccountStd != null && !e.AccountStd.AccountReference.IsLoaded)
                    {
                        e.AccountStd.AccountReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.AccountStd.AccountReference");
                    }
                    if (!e.AccountInternal.IsLoaded)
                    {
                        e.AccountInternal.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.AccountInternal");
                    }
                    if (e.AccountInternal != null)
                    {
                        foreach (var accInt in e.AccountInternal)
                        {
                            if (!accInt.AccountReference.IsLoaded)
                            {
                                accInt.AccountReference.Load();
                                DataProjectLogCollector.LogLoadedEntityInExtension("accInt.AccountReference");
                            }
                            if (accInt.Account != null && !accInt.Account.AccountDimReference.IsLoaded)
                            {
                                accInt.Account.AccountDimReference.Load();
                                DataProjectLogCollector.LogLoadedEntityInExtension("accInt.Account.AccountDimReference");
                            }
                        }
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            AccountingRowDTO dto = new AccountingRowDTO()
            {
                InvoiceRowId = invoiceRow != null ? invoiceRow.CustomerInvoiceRowId : 0,    // TODO: Add foreign key to model
                TempInvoiceRowId = invoiceRow != null ? invoiceRow.CustomerInvoiceRowId : 0,
                InvoiceAccountRowId = e.CustomerInvoiceAccountRowId,
                AccountDistributionHeadId = e.AccountDistributionHeadId != null ? (int)e.AccountDistributionHeadId : 0,
                TempRowId = e.CustomerInvoiceAccountRowId,
                RowNr = e.RowNr,
                ProductName = invoiceRow != null && invoiceRow.Product != null ? invoiceRow.Product.Name : String.Empty,
                Text = e.Text,
                Quantity = e.Quantity,
                Amount = e.Amount,
                CreditAmount = e.Amount < 0 ? Math.Abs(e.Amount) : 0,
                DebitAmount = e.Amount > 0 ? e.Amount : 0,
                AmountCurrency = e.AmountCurrency,
                CreditAmountCurrency = e.AmountCurrency < 0 ? Math.Abs(e.AmountCurrency) : 0,
                DebitAmountCurrency = e.AmountCurrency > 0 ? e.AmountCurrency : 0,
                AmountEntCurrency = e.AmountEntCurrency,
                CreditAmountEntCurrency = e.AmountEntCurrency < 0 ? Math.Abs(e.AmountEntCurrency) : 0,
                DebitAmountEntCurrency = e.AmountEntCurrency > 0 ? e.AmountEntCurrency : 0,
                AmountLedgerCurrency = e.AmountLedgerCurrency,
                CreditAmountLedgerCurrency = e.AmountLedgerCurrency < 0 ? Math.Abs(e.AmountLedgerCurrency) : 0,
                DebitAmountLedgerCurrency = e.AmountLedgerCurrency > 0 ? e.AmountLedgerCurrency : 0,
                SplitType = e.SplitType,
                SplitPercent = e.SplitPercent,
                IsCreditRow = e.CreditRow,
                IsDebitRow = e.DebitRow,
                IsVatRow = e.VatRow,
                IsContractorVatRow = e.ContractorVatRow,
                IsCentRoundingRow = invoiceRow?.IsCentRoundingRow ?? false,
                IsHouseholdRow = invoiceRow?.HouseholdTaxDeductionRow != null && invoiceRow.HouseholdTaxDeductionRow.AmountCurrency != 0,
                IsClaimRow = invoiceRow?.CustomerInvoice != null && !invoiceRow.CustomerInvoice.ManuallyAdjustedAccounting && invoiceRow.Product == null,
                IsManuallyAdjusted = invoiceRow?.CustomerInvoice != null && invoiceRow.CustomerInvoice.ManuallyAdjustedAccounting && invoiceRow.Product == null && invoiceRow.SumAmount == 0,
                State = (SoeEntityState)e.State,
                ParentRowId = e.ParentRowId ?? 0,
            };

            // Accounts
            AccountStd accStd = e.AccountStd;
            if (accStd != null)
            {
                dto.Dim1Id = accStd.AccountId;
                dto.Dim1Nr = accStd.Account != null ? accStd.Account.AccountNr : String.Empty;
                dto.Dim1Name = accStd.Account != null ? accStd.Account.Name : String.Empty;
                dto.Dim1Disabled = false;
                dto.Dim1Mandatory = true;
                dto.QuantityStop = accStd.UnitStop;
                dto.Unit = accStd.Unit;
                dto.AmountStop = accStd.AmountStop;
                dto.RowTextStop = accStd.RowTextStop;
            }

            if (e.AccountInternal != null)
            {
                foreach (AccountInternal accInt in e.AccountInternal)
                {
                    if (accInt.Account != null && accInt.Account.AccountDim != null)
                    {
                        switch (accInt.Account.AccountDim.AccountDimNr)
                        {
                            case 2:
                                dto.Dim2Id = accInt.AccountId;
                                dto.Dim2Nr = accInt.Account.AccountNr;
                                dto.Dim2Name = accInt.Account.Name;
                                break;
                            case 3:
                                dto.Dim3Id = accInt.AccountId;
                                dto.Dim3Nr = accInt.Account.AccountNr;
                                dto.Dim3Name = accInt.Account.Name;
                                break;
                            case 4:
                                dto.Dim4Id = accInt.AccountId;
                                dto.Dim4Nr = accInt.Account.AccountNr;
                                dto.Dim4Name = accInt.Account.Name;
                                break;
                            case 5:
                                dto.Dim5Id = accInt.AccountId;
                                dto.Dim5Nr = accInt.Account.AccountNr;
                                dto.Dim5Name = accInt.Account.Name;
                                break;
                            case 6:
                                dto.Dim6Id = accInt.AccountId;
                                dto.Dim6Nr = accInt.Account.AccountNr;
                                dto.Dim6Name = accInt.Account.Name;
                                break;
                        }
                    }
                }
            }

            return dto;
        }

        #endregion

    }
}
