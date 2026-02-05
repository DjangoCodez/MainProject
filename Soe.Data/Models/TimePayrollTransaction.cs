using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoftOne.Soe.Data
{
    public partial class TimePayrollTransaction : IPayrollTransaction, IPayrollTransactionAccounting, ICreatedModified, IState, IModifiedWithNoCheckes, ITask
    {
        public DateTime Date
        {
            get
            {
                return this.TimeBlockDate?.Date ?? CalendarUtility.DATETIME_DEFAULT;
            }
        }
        public List<int> AccountInternalIds
        {
            get
            {
                return this.GetAccountInternalIds();
            }
        }
        public bool IsRetroactive
        {
            get
            {
                return this.RetroactivePayrollOutcomeId.HasValue;
            }
        }
        public bool IsRounding
        {
            get
            {
                return this.IsCentRounding || this.IsQuantityRounding;
            }
        }
        public bool IsEmployeeVehicle
        {
            get
            {
                return this.EmployeeVehicleId.HasValue;
            }
        }
        public bool IsUnionFee
        {
            get
            {
                return this.UnionFeeId.HasValue;
            }
        }
        public bool IsQuantityDays
        {
            get
            {
                return (this.TimePayrollTransactionExtended?.TimeUnit != (int)TermGroup_PayrollProductTimeUnit.Hours);
            }
        }
        public bool IsQuantity
        {
            get
            {
                if (this.IsQuantityDays || this.IsFixed || this.IsCentRounding || this.IsAdded || this.IsUnionFee || this.IsEmployeeVehicle)
                    return true;
                if (this.IsTax())
                    return true;
                if (this.IsEmploymentTaxCredit())
                    return true;
                if (this.IsEmploymentTaxDebit())
                    return true;
                if (this.IsSupplementCharge())
                    return true;
                if (this.IsNetSalary())
                    return true;
                if (this.IsVacationCompensationDirectPaid())
                    return true;
                if (this.IsVacationAdditionOrSalaryPrepaymentInvert())
                    return true;
                if (this.IsVacationAdditionOrSalaryVariablePrepaymentInvert())
                    return true;
                if (this.IsVacationAdditionOrSalaryPrepaymentPaid())
                    return true;
                if (this.IsVacationAdditionOrSalaryVariablePrepaymentPaid())
                    return true;
                if (this.IsBenefitInvert())
                    return true;
                if (this.IsVacationCompensationAdvance())
                    return true;

                return false;
            }
        }
        public bool PayrollProductUseInPayroll
        {
            get
            {
                return this.PayrollProduct?.UseInPayroll ?? false;
            }
        }
        public int? PayrollImportEmployeeTransactionId { get; set; }
        private List<int> GetAccountInternalIds()
        {
            return this.AccountInternal?.Select(ai => ai.AccountId).ToList() ?? new List<int>();
        }

        public PayrollProductDTO CachedProduct { get; set; }
        public TimeBlockDateDTO CachedTimeBlockDate { get; set; }
        public bool TempUsed { get; set; }

        public void SetTimeWorkAccountYearOutcome(TimeWorkAccountYearOutcome timeWorkAccountYearOutcome, string modifiedBy, DateTime? modified = null)
        {
            if (timeWorkAccountYearOutcome != null && this.TimeWorkAccountYearOutcomeId != timeWorkAccountYearOutcome.TimeWorkAccountYearOutcomeId)
            {                
                this.TimeWorkAccountYearOutcome = timeWorkAccountYearOutcome;
                this.SetModified(modifiedBy, modified);
            }
        }
        public void SetTimeWorkReductionReconciliationOutcome(TimeWorkReductionReconciliationOutcome timeWorkReductionReconciliationOutcome, string modifiedBy, DateTime? modified = null)
        {
            if (timeWorkReductionReconciliationOutcome != null && this.TimeWorkReductionReconciliationOutcomeId != timeWorkReductionReconciliationOutcome.TimeWorkReductionReconciliationOutcomeId)
            {
                this.TimeWorkReductionReconciliationOutcome = timeWorkReductionReconciliationOutcome;
                this.SetModified(modifiedBy, modified);
            }
        }
        public void SetTimeCodeTransaction(TimeCodeTransaction timeCodeTransaction, string modifiedBy, DateTime? modified = null)
        {
            if (timeCodeTransaction != null && this.TimeCodeTransactionId != timeCodeTransaction.TimeCodeTransactionId)
            {
                this.TimeCodeTransaction = timeCodeTransaction;
                this.SetModified(modifiedBy, modified);
            }
        }
        public void SetVacationYearEndRow(VacationYearEndRow vacationYearEndRow, string modifiedBy, DateTime? modified = null)
        {
            if (vacationYearEndRow != null && this.VacationYearEndRowId != vacationYearEndRow.VacationYearEndRowId)
            {
                this.VacationYearEndRow = vacationYearEndRow;
                this.SetModified(modifiedBy, modified);
            }
        }
        private void SetModified(string modifiedBy, DateTime? modified = null)
        {
            if (this.TimePayrollTransactionId > 0)
                this.SetModified(modified ?? DateTime.Now, modifiedBy);
        }
    }

    public partial class GetTimePayrollTransactionAccountsForEmployee_Result : IAccountId
    {

    }

    public partial class GetTimePayrollTransactionsForCompany_Result : IPayrollTransactionProc, IPayrollTransactionAccounting
    {
        #region IPayrollType

        public int? SysPayrollTypeLevel1 => this.TransactionSysPayrollTypeLevel1;
        public int? SysPayrollTypeLevel2 => this.TransactionSysPayrollTypeLevel2;
        public int? SysPayrollTypeLevel3 => this.TransactionSysPayrollTypeLevel3;
        public int? SysPayrollTypeLevel4 => this.TransactionSysPayrollTypeLevel4;

        #endregion

        #region IPayrollTransactionAccounting

        public List<int> AccountInternalIds { get; set; }

        #endregion
    }

    public partial class GetTimePayrollTransactionsForEmployee_Result : IPayrollTransactionProc, IPayrollTransactionAccounting, ITransactionProc
    {
        #region IPayrollType

        public int? SysPayrollTypeLevel1 => this.TransactionSysPayrollTypeLevel1;
        public int? SysPayrollTypeLevel2 => this.TransactionSysPayrollTypeLevel2;
        public int? SysPayrollTypeLevel3 => this.TransactionSysPayrollTypeLevel3;
        public int? SysPayrollTypeLevel4 => this.TransactionSysPayrollTypeLevel4;

        #endregion

        #region IPayrollTransactionAccounting

        public List<int> AccountInternalIds { get; set; }

        #endregion

        #region ITransactionProc

        public int Id => this.TimePayrollTransactionId;

        #endregion
    }

    public partial class GetTimePayrollTransactionsWithAccIntsForEmployee_Result : IPayrollType
    {
        #region IPayrollType

        public int? SysPayrollTypeLevel1 => this.TransactionSysPayrollTypeLevel1;
        public int? SysPayrollTypeLevel2 => this.TransactionSysPayrollTypeLevel2;
        public int? SysPayrollTypeLevel3 => this.TransactionSysPayrollTypeLevel3;
        public int? SysPayrollTypeLevel4 => this.TransactionSysPayrollTypeLevel4;

        #endregion
    }

    public partial class GetTimePayrollScheduleTransactionsForEmployee_Result : IPayrollScheduleTransactionProc, ITransactionProc
    {
        #region ITransactionProc

        public int Id => this.TimePayrollScheduleTransactionId;

        #endregion
    }

    public static partial class EntityExtensions
    {
        #region TimePayrollTransaction

        public static IEnumerable<TimePayrollTransactionDTO> ToDTOs(this IEnumerable<TimePayrollTransaction> l, bool includeExtended, bool includeTimeCodeTransaction)
        {
            var dtos = new List<TimePayrollTransactionDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeExtended, includeTimeCodeTransaction));
                }
            }
            return dtos;
        }

        public static TimePayrollTransactionDTO ToDTO(this TimePayrollTransaction e, bool includeExtended, bool includeTimeCodeTransaction)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded())
                {
                    if (includeExtended && !e.TimePayrollTransactionExtendedReference.IsLoaded)
                    {
                        e.TimePayrollTransactionExtendedReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("TimePayrollTransaction.cs e.TimePayrollTransactionExtendedReference");
                    }
                    if (includeTimeCodeTransaction && !e.TimeCodeTransactionReference.IsLoaded)
                    {
                        e.TimeCodeTransactionReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("TimePayrollTransaction.cs e.TimeCodeTransactionReference");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            TimePayrollTransactionDTO dto = new TimePayrollTransactionDTO()
            {
                TimePayrollTransactionId = e.TimePayrollTransactionId,
                TimeCodeTransactionId = e.TimeCodeTransactionId,
                TimeBlockId = e.TimeBlockId,
                EmployeeId = e.EmployeeId,
                TimeBlockDateId = e.TimeBlockDateId,
                PayrollProductId = e.ProductId,
                AccountId = e.AccountStdId,
                AttestStateId = e.AttestStateId,
                SysPayrollTypeLevel1 = e.SysPayrollTypeLevel1,
                SysPayrollTypeLevel2 = e.SysPayrollTypeLevel2,
                SysPayrollTypeLevel3 = e.SysPayrollTypeLevel3,
                SysPayrollTypeLevel4 = e.SysPayrollTypeLevel4,
                Date = e.TimeBlockDate != null ? e.TimeBlockDate.Date : (DateTime?)null,
                Amount = e.Amount ?? 0,
                AmountCurrency = e.AmountCurrency ?? 0,
                AmountEntCurrency = e.AmountEntCurrency ?? 0,
                AmountLedgerCurrency = e.AmountLedgerCurrency ?? 0,
                VatAmount = e.VatAmount ?? 0,
                VatAmountCurrency = e.VatAmountCurrency ?? 0,
                VatAmountEntCurrency = e.VatAmountEntCurrency ?? 0,
                VatAmountLedgerCurrency = e.VatAmountLedgerCurrency ?? 0,
                Quantity = e.Quantity,
                ManuallyAdded = e.ManuallyAdded,
                AutoAttestFailed = e.AutoAttestFailed,
                Exported = e.Exported,
                IsPreliminary = e.IsPreliminary,
                IsExtended = e.IsExtended,
                Comment = e.Comment,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                IsAdded = e.IsAdded,
            };

            dto.AttestState = e.AttestState?.ToDTO();
            dto.AccountStd = e.AccountStd?.Account?.ToDTO();
            dto.AccountInternals = e.AccountInternal?.ToDTOs();

            if (includeExtended && e.TimePayrollTransactionExtended != null)
                dto.Extended = e.TimePayrollTransactionExtended.ToDTO();
            if (includeTimeCodeTransaction && e.TimeCodeTransaction != null)
                dto.TimeCodeTransaction = e.TimeCodeTransaction.ToDTO();

            return dto;
        }

        public static List<AttestPayrollTransactionDTO> ToDTOs(this IEnumerable<TimePayrollTransaction> l, List<AccountDimDTO> accountDims = null)
        {
            var transactions = new List<AttestPayrollTransactionDTO>();

            foreach (var e in l)
            {
                List<AccountDTO> accountInternals = e.AccountInternal != null ? e.AccountInternal.ToAccountDTOs().ToList() : new List<AccountDTO>();
                AccountDTO accountStd = e.AccountStd?.ToDTO() ?? new AccountDTO();
                transactions.Add(e.ToDTO(accountInternals: accountInternals, accountStd: accountStd, accountDims: accountDims));
            }

            return transactions;
        }

        public static AttestPayrollTransactionDTO ToDTO(this TimePayrollTransaction e, List<AccountDTO> accountInternals = null, AccountDTO accountStd = null, List<AccountDimDTO> accountDims = null)
        {
            var transactionItem = new AttestPayrollTransactionDTO()
            {
                //TimePayrollTransaction
                EmployeeId = e.EmployeeId,
                TimePayrollTransactionId = e.TimePayrollTransactionId,
                Quantity = e.Quantity,
                UnitPrice = e.UnitPrice,
                UnitPriceCurrency = e.UnitPriceCurrency,
                UnitPriceEntCurrency = e.UnitPriceEntCurrency,
                Amount = e.Amount,
                AmountCurrency = e.AmountCurrency,
                AmountEntCurrency = e.AmountEntCurrency,
                VatAmount = e.VatAmount,
                VatAmountCurrency = e.VatAmountCurrency,
                VatAmountEntCurrency = e.VatAmountEntCurrency,
                ReversedDate = e.ReversedDate,
                IsReversed = e.IsReversed,
                IsPreliminary = e.IsPreliminary,
                IsExported = e.Exported,
                ManuallyAdded = e.ManuallyAdded,
                Comment = e.Comment,
                HasInfo = false,
                IsAdded = e.IsAdded,
                AddedDateFrom = e.AddedDateFrom,
                AddedDateTo = e.AddedDateTo,
                IsFixed = e.IsFixed,
                IsSpecifiedUnitPrice = e.IsSpecifiedUnitPrice,
                IsAdditionOrDeduction = e.IsAdditionOrDeduction,
                IsVacationReplacement = e.IsVacationReplacement,
                IsCentRounding = e.IsCentRounding,
                IsQuantityRounding = e.IsQuantityRounding,
                IncludedInPayrollProductChain = e.IncludedInPayrollProductChain,
                ParentId = e.ParentId,
                UnionFeeId = e.UnionFeeId,
                EarningTimeAccumulatorId = e.EarningTimeAccumulatorId,
                EmployeeVehicleId = e.EmployeeVehicleId,
                PayrollStartValueRowId = e.PayrollStartValueRowId,
                RetroactivePayrollOutcomeId = e.RetroactivePayrollOutcomeId,
                VacationYearEndRowId = e.VacationYearEndRowId,
                IsVacationFiveDaysPerWeek = e.IsVacationFiveDaysPerWeek,
                TransactionSysPayrollTypeLevel1 = e.SysPayrollTypeLevel1,
                TransactionSysPayrollTypeLevel2 = e.SysPayrollTypeLevel2,
                TransactionSysPayrollTypeLevel3 = e.SysPayrollTypeLevel3,
                TransactionSysPayrollTypeLevel4 = e.SysPayrollTypeLevel4,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,

                //TimePayrollTransactionExtended
                PayrollPriceFormulaId = e.TimePayrollTransactionExtended?.PayrollPriceFormulaId,
                PayrollPriceTypeId = e.TimePayrollTransactionExtended?.PayrollPriceTypeId,
                Formula = e.TimePayrollTransactionExtended?.Formula ?? string.Empty,
                FormulaPlain = e.TimePayrollTransactionExtended?.FormulaPlain ?? string.Empty,
                FormulaExtracted = e.TimePayrollTransactionExtended?.FormulaExtracted ?? string.Empty,
                FormulaNames = e.TimePayrollTransactionExtended?.FormulaNames ?? string.Empty,
                FormulaOrigin = e.TimePayrollTransactionExtended?.FormulaOrigin ?? string.Empty,
                PayrollCalculationPerformed = e.TimePayrollTransactionExtended?.PayrollCalculationPerformed ?? false,
                TimeUnit = e.TimePayrollTransactionExtended?.TimeUnit ?? 0,
                QuantityWorkDays = e.TimePayrollTransactionExtended?.QuantityWorkDays ?? 0,
                QuantityCalendarDays = e.TimePayrollTransactionExtended?.QuantityCalendarDays ?? 0,
                CalenderDayFactor = e.TimePayrollTransactionExtended?.CalenderDayFactor ?? 0,
                IsDistributed = e.TimePayrollTransactionExtended?.IsDistributed ?? false,
                //TimePayrollScheduleTransaction
                IsScheduleTransaction = false,

                //PayrollProduct
                PayrollProductId = e.ProductId,
                PayrollProductNumber = e.PayrollProduct?.Number ?? string.Empty,
                PayrollProductName = e.PayrollProduct?.Name ?? string.Empty,
                PayrollProductShortName = e.PayrollProduct?.ShortName ?? string.Empty,
                PayrollProductFactor = e.PayrollProduct?.Factor ?? 0,
                PayrollProductPayed = e.PayrollProduct?.Payed ?? false,
                PayrollProductExport = e.PayrollProduct?.Export ?? false,
                PayrollProductUseInPayroll = e.PayrollProduct?.UseInPayroll ?? false,
                PayrollProductSysPayrollTypeLevel1 = e.PayrollProduct?.SysPayrollTypeLevel1 ?? (int)TermGroup_SysPayrollType.None,
                PayrollProductSysPayrollTypeLevel2 = e.PayrollProduct?.SysPayrollTypeLevel2 ?? (int)TermGroup_SysPayrollType.None,
                PayrollProductSysPayrollTypeLevel3 = e.PayrollProduct?.SysPayrollTypeLevel3 ?? (int)TermGroup_SysPayrollType.None,
                PayrollProductSysPayrollTypeLevel4 = e.PayrollProduct?.SysPayrollTypeLevel4 ?? (int)TermGroup_SysPayrollType.None,
                IsAverageCalculated = e.PayrollProduct?.AverageCalculated ?? false,

                //TimeCodeTransaction
                TimeCodeTransactionId = e.TimeCodeTransactionId,
                StartTime = null,
                StopTime = null,

                //TimeCode
                TimeCodeType = SoeTimeCodeType.None,
                TimeCodeRegistrationType = TermGroup_TimeCodeRegistrationType.Unknown,
                NoOfPresenceWorkOutsideScheduleTime = null,
                NoOfAbsenceAbsenceTime = null,

                //TimeBlockDate
                TimeBlockDateId = e.TimeBlockDateId,
                Date = e.TimeBlockDate?.Date ?? CalendarUtility.DATETIME_DEFAULT,

                //TimeBlock
                TimeBlockId = e.TimeBlockId,

                //AttestState
                AttestStateId = e.AttestStateId,
                AttestStateName = String.Empty,
                AttestStateColor = String.Empty,
                AttestStateInitial = false,
                AttestStateSort = 0,
                HasSameAttestState = true,

                //Accounting
                AccountDims = accountDims ?? new List<AccountDimDTO>(),
                AccountStd = accountStd ?? new AccountDTO(),
                AccountInternals = accountInternals ?? new List<AccountDTO>(),

                //TimePeriod
                TimePeriodId = e.TimePeriodId,
                TimePeriodName = String.Empty,
            };

            transactionItem.SetAccountingStrings();

            return transactionItem;
        }

        public static IEnumerable<TimePayrollTransactionExtendedDTO> ToDTOs(this IEnumerable<TimePayrollTransactionExtended> l)
        {
            var dtos = new List<TimePayrollTransactionExtendedDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static TimePayrollTransactionExtendedDTO ToDTO(this TimePayrollTransactionExtended e)
        {
            if (e == null)
                return null;

            return new TimePayrollTransactionExtendedDTO()
            {
                TimePayrollTransactionId = e.TimePayrollTransactionId,
                ActorCompanyId = e.ActorCompanyId,
                EmployeeId = e.EmployeeId,
                PayrollPriceFormulaId = e.PayrollPriceFormulaId,
                Formula = e.Formula,
                FormulaPlain = e.FormulaPlain,
                FormulaExtracted = e.FormulaExtracted,
                FormulaNames = e.FormulaNames,
                FormulaOrigin = e.FormulaOrigin,
                PayrollCalculationPerformed = e.PayrollCalculationPerformed,
                State = (SoeEntityState)e.State
            };
        }

        public static IEnumerable<TimePayrollTransactionCompactDTO> ToCompactDTOs(this IEnumerable<TimePayrollTransaction> l)
        {
            var dtos = new List<TimePayrollTransactionCompactDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToCompactDTO());
                }
            }

            return dtos;
        }

        public static TimePayrollTransactionCompactDTO ToCompactDTO(this TimePayrollTransaction e)
        {
            if (e == null)
                return null;

            return new TimePayrollTransactionCompactDTO()
            {
                TimePayrollTransactionId = e.TimePayrollTransactionId,
                Date = e.TimeBlockDate != null ? e.TimeBlockDate.Date : CalendarUtility.DATETIME_DEFAULT,
                ProductNr = e.PayrollProduct?.Number ?? "",
                ProductName = e.PayrollProduct?.Name ?? "",
                Quantity = e.Quantity,
                Unitprice = e.UnitPrice ?? 0,
                Amount = e.Amount ?? 0,
                Comment = e.Comment,
            };
        }

        public static AttestPayrollTransactionDTO ToAttestEmployeeTimePayrollTransactionDTO(this TimeTransactionItem e, List<AccountDimDTO> accountDims, TimeBlock timeBlock, AttestEmployeeDayTimeCodeTransactionDTO timeCodeTransaction)
        {
            var dto = new AttestPayrollTransactionDTO()
            {
                //TimePayrollTransaction
                EmployeeId = e.EmployeeId,
                TimePayrollTransactionId = e.TimeTransactionId,
                Quantity = e.Quantity,
                ReversedDate = e.ReversedDate,
                ManuallyAdded = e.ManuallyAdded,
                Comment = e.Comment,
                EmployeeChildId = e.EmployeeChildId,
                TransactionSysPayrollTypeLevel1 = e.TransactionSysPayrollTypeLevel1,
                TransactionSysPayrollTypeLevel2 = e.TransactionSysPayrollTypeLevel2,
                TransactionSysPayrollTypeLevel3 = e.TransactionSysPayrollTypeLevel3,
                TransactionSysPayrollTypeLevel4 = e.TransactionSysPayrollTypeLevel4,
                IncludedInPayrollProductChain = e.IncludedInPayrollProductChain,

                //TimePayrollScheduleTransaction
                IsScheduleTransaction = e.IsScheduleTransaction,

                //PayrollProduct
                PayrollProductId = e.ProductId,
                PayrollProductNumber = e.ProductNr,
                PayrollProductName = e.ProductName,
                PayrollProductSysPayrollTypeLevel1 = e.PayrollProductSysPayrollTypeLevel1,
                PayrollProductSysPayrollTypeLevel2 = e.PayrollProductSysPayrollTypeLevel2,
                PayrollProductSysPayrollTypeLevel3 = e.PayrollProductSysPayrollTypeLevel3,
                PayrollProductSysPayrollTypeLevel4 = e.PayrollProductSysPayrollTypeLevel4,

                //TimeCodeTransaction
                StartTime = timeCodeTransaction?.StartTime ?? (timeBlock?.StartTime ?? CalendarUtility.DATETIME_DEFAULT),
                StopTime = timeCodeTransaction?.StopTime ?? (timeBlock?.StopTime ?? CalendarUtility.DATETIME_DEFAULT),

                //TimeCode
                TimeCodeType = e.TimeCodeType,
                TimeCodeRegistrationType = e.TimeCodeRegistrationType,

                //TimeBlockDate
                TimeBlockDateId = e.TimeBlockDateId ?? (timeBlock?.TimeBlockDateId ?? 0),
                Date = e.Date ?? CalendarUtility.DATETIME_DEFAULT,

                //TimeBlock
                TimeBlockId = e.TimeBlockId,

                //AttestState
                AttestStateId = e.AttestStateId,
                AttestStateName = e.AttestStateName,
                AttestStateColor = e.AttestStateColor,
                AttestStateInitial = e.AttestStateInitial,
                AttestStateSort = e.AttestStateSort,
                HasSameAttestState = true,

            };

            //Guids
            if (e.GuidId.HasValue)
                dto.GuidId = e.GuidId.Value.ToString();
            if (e.ParentGuidId.HasValue)
                dto.ParentGuidId = e.ParentGuidId.Value.ToString();

            if (timeCodeTransaction != null)
            {
                dto.GuidIdTimeBlock = timeCodeTransaction.GuidIdTimeBlock;
                dto.GuidIdTimeCodeTransaction = timeCodeTransaction.GuidId;
            }

            //Accounting
            dto.SetAccountingStrings(e, accountDims);

            return dto;
        }

        public static List<TimePayrollTransaction> Filter(this List<TimePayrollTransaction> l, DateTime fromDate, DateTime toDate)
        {
            return l?.Where(tpt => tpt.TimeBlockDate != null && CalendarUtility.IsDateInRange(tpt.TimeBlockDate.Date, fromDate, toDate)).ToList() ?? new List<TimePayrollTransaction>();
        }

        public static IQueryable<TimePayrollTransaction> FilterPayrollType(this IQueryable<TimePayrollTransaction> l, TermGroup_SysPayrollType? sysPayrollTypeLevel1 = null, TermGroup_SysPayrollType? sysPayrollTypeLevel2 = null, TermGroup_SysPayrollType? sysPayrollTypeLevel3 = null, TermGroup_SysPayrollType? sysPayrollTypeLevel4 = null)
        {
            if (sysPayrollTypeLevel1.HasValue)
                l = l.Where(tpt => tpt.SysPayrollTypeLevel1 == (int)sysPayrollTypeLevel1.Value);
            if (sysPayrollTypeLevel2.HasValue)
                l = l.Where(tpt => tpt.SysPayrollTypeLevel2 == (int)sysPayrollTypeLevel2.Value);
            if (sysPayrollTypeLevel3.HasValue)
                l = l.Where(tpt => tpt.SysPayrollTypeLevel3 == (int)sysPayrollTypeLevel3.Value);
            if (sysPayrollTypeLevel4.HasValue)
                l = l.Where(tpt => tpt.SysPayrollTypeLevel4 == (int)sysPayrollTypeLevel4.Value);
            return l;
        }

        public static IQueryable<TimePayrollTransaction> FilterDates(this IQueryable<TimePayrollTransaction> l, DateTime? fromDate, DateTime? toDate)
        {
            if (fromDate.HasValue)
                l = l.Where(tpt => tpt.TimeBlockDate.Date >= fromDate.Value);
            if (toDate.HasValue)
                l = l.Where(tpt => tpt.TimeBlockDate.Date <= toDate.Value);
            return l;
        }

        public static IQueryable<TimePayrollTransaction> FilterCurrent(this IQueryable<TimePayrollTransaction> l, bool onlyCurrent)
        {
            if (onlyCurrent)
                l = l.Where(tpt => !tpt.ReversedDate.HasValue);
            return l;
        }

        public static List<TimePayrollTransaction> GetPayrollCalculationTransactions(this List<TimePayrollTransaction> l, bool includeMassregistratioTransactions = false)
        {
            return l.Where(x => !x.IsAdded && !x.IsRetroTransaction() && !x.PayrollStartValueRowId.HasValue || (includeMassregistratioTransactions && x.IsAdded && x.MassRegistrationTemplateRowId.HasValue)).ToList();
        }

        public static List<TimePayrollTransaction> GetAbsence(this IEnumerable<TimePayrollTransaction> l)
        {
            return l?.Where(e => e.IsAbsence()).ToList() ?? new List<TimePayrollTransaction>();
        }

        public static List<TimePayrollTransaction> GetVacation(this IEnumerable<TimePayrollTransaction> l)
        {
            return l?.Where(e => e.SysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation).ToList() ?? new List<TimePayrollTransaction>();
        }

        public static List<TimePayrollTransaction> GetVacationCompensationTransactions(this List<TimePayrollTransaction> l)
        {
            return l.Where(x => x.SysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation).ToList();
        }

        public static List<TimePayrollTransaction> GetVacationPrepaymentAndVariableTransactions(this List<TimePayrollTransaction> l)
        {
            return l.Where(x => x.SysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationAdditionOrSalaryPrepayment || x.SysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationAdditionOrSalaryVariablePrepayment).ToList();
        }

        public static List<TimePayrollTransaction> GetVacationPrepaymentTransactions(this List<TimePayrollTransaction> l)
        {
            return l.Where(x => x.SysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationAdditionOrSalaryPrepayment).ToList();
        }

        public static List<TimePayrollTransaction> GetVacationAdditionWithVariableTransactions(this List<TimePayrollTransaction> l)
        {
            return l.Where(x => x.SysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationAddition || x.SysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationAdditionVariable).ToList();
        }

        public static List<TimePayrollTransaction> GetVacationPrepaymentVariableTransactions(this List<TimePayrollTransaction> l)
        {
            return l.Where(x => x.SysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationAdditionOrSalaryVariablePrepayment).ToList();
        }

        public static List<TimePayrollTransaction> GetVacationCompensationAdvanceTransactions(this List<TimePayrollTransaction> l)
        {
            return l.Where(x => x.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary && x.SysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation && x.SysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_Advance).ToList();
        }

        public static List<TimePayrollTransaction> GetVacationSalaryAdvanceTransactions(this List<TimePayrollTransaction> l)
        {
            return l.Where(x => x.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary && x.SysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationSalary && x.SysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationSalary_Advance).ToList();
        }

        public static List<TimePayrollTransaction> GetGrossSalaryAndBenefitTransactions(this List<TimePayrollTransaction> l)
        {
            return l.Where(x => x.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary || x.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit).ToList();
        }

        public static List<TimePayrollTransaction> GreaterThan(this List<TimePayrollTransaction> l, DateTime date)
        {
            return l?.Where(e => e.TimeBlockDate != null && e.TimeBlockDate.Date > date).ToList() ?? new List<TimePayrollTransaction>();
        }

        public static List<TimePayrollTransaction> ExcludeStartValues(this IEnumerable<TimePayrollTransaction> l)
        {
            return l?.Where(e => !e.PayrollStartValueRowId.HasValue).ToList() ?? new List<TimePayrollTransaction>();
        }

        public static Dictionary<DateTime, List<TimePayrollTransaction>> ByDate(this List<TimePayrollTransaction> l)
        {
            return l?
                .GroupBy(b => b.Date)
                .ToDictionary(k => k.Key, v => v.ToList()) ?? new Dictionary<DateTime, List<TimePayrollTransaction>>();
        }

        public static TimePayrollTransaction GetFirst(this List<TimePayrollTransaction> l)
        {
            return l?.Where(e => e.TimeBlockDate != null).OrderBy(t => t.TimeBlockDate.Date).FirstOrDefault();
        }

        public static TimePayrollTransaction GetLast(this List<TimePayrollTransaction> l)
        {
            return l?.Where(e => e.TimeBlockDate != null).OrderByDescending(t => t.TimeBlockDate.Date).FirstOrDefault();
        }

        public static List<PayrollCalculationPeriodSumItemDTO> ToPayrollCalculationPeriodSumItemDTOs(this IEnumerable<TimePayrollTransaction> l)
        {
            var sums = new List<PayrollCalculationPeriodSumItemDTO>();

            foreach (var e in l)
            {
                sums.Add(e.ToPayrollCalculationPeriodSumItemDTO());
            }

            return sums;
        }

        public static PayrollCalculationPeriodSumItemDTO ToPayrollCalculationPeriodSumItemDTO(this TimePayrollTransaction e)
        {
            return new PayrollCalculationPeriodSumItemDTO
            {
                SysPayrollTypeLevel1 = e.SysPayrollTypeLevel1,
                SysPayrollTypeLevel2 = e.SysPayrollTypeLevel2,
                SysPayrollTypeLevel3 = e.SysPayrollTypeLevel3,
                SysPayrollTypeLevel4 = e.SysPayrollTypeLevel4,
                Amount = e.Amount,
            };
        }

        public static VacationDaysCalculationDTO ToVacationDaysCalculationDTO(this List<TimePayrollTransaction> l, bool calculateHours = false)
        {
            VacationDaysCalculationDTO vacationDaysCalculationDTO = new VacationDaysCalculationDTO();

            #region Vacation

            decimal periodUsedDaysPaidCount = 0;
            decimal periodUsedDaysUnpaidCount = 0;
            decimal periodUsedDaysAdvanceCount = 0;
            decimal periodUsedDaysYear1Count = 0;
            decimal periodUsedDaysYear2Count = 0;
            decimal periodUsedDaysYear3Count = 0;
            decimal periodUsedDaysYear4Count = 0;
            decimal periodUsedDaysYear5Count = 0;
            decimal periodUsedDaysOverdueCount = 0;

            var vacationTransactions = l.GetVacation().Where(x => !x.IsRetroTransaction()).ToList();

            foreach (var vacationTransactionsByDate in vacationTransactions.GroupBy(x => x.TimeBlockDateId))
            {
                decimal quantity = vacationTransactionsByDate.Sum(x => x.Quantity);
                bool isVacationFiveDaysPerWeek = vacationTransactionsByDate.Any(x => x.IsVacationFiveDaysPerWeek);
                //can be a reversed day
                if (quantity != 0 || isVacationFiveDaysPerWeek)
                {
                    if (vacationTransactionsByDate.ToList().HasVacationPaidTransactions())
                        periodUsedDaysPaidCount += !calculateHours ? vacationTransactionsByDate.GetQuantityVacationDays(false) : vacationTransactionsByDate.Where(w => w.IsVacationPaid()).GetQuantityVacationDays(true);
                    else if (vacationTransactionsByDate.ToList().HasVacationUnpaidTransactions())
                        periodUsedDaysUnpaidCount += !calculateHours ? vacationTransactionsByDate.GetQuantityVacationDays(false) : vacationTransactionsByDate.Where(w => w.IsVacationUnPaid()).GetQuantityVacationDays(true);
                    else if (vacationTransactionsByDate.ToList().HasVacationAdvanceTransactions())
                        periodUsedDaysAdvanceCount += !calculateHours ? vacationTransactionsByDate.GetQuantityVacationDays(false) : vacationTransactionsByDate.Where(w => w.IsVacationAdvance()).GetQuantityVacationDays(true);
                    else if (vacationTransactionsByDate.ToList().HasVacationSavedYear1Transactions())
                        periodUsedDaysYear1Count += !calculateHours ? vacationTransactionsByDate.GetQuantityVacationDays(false) : vacationTransactionsByDate.Where(w => w.IsVacationSavedYear1()).GetQuantityVacationDays(true);
                    else if (vacationTransactionsByDate.ToList().HasVacationSavedYear2Transactions())
                        periodUsedDaysYear2Count += !calculateHours ? vacationTransactionsByDate.GetQuantityVacationDays(false) : vacationTransactionsByDate.Where(w => w.IsVacationSavedYear2()).GetQuantityVacationDays(true);
                    else if (vacationTransactionsByDate.ToList().HasVacationSavedYear3Transactions())
                        periodUsedDaysYear3Count += !calculateHours ? vacationTransactionsByDate.GetQuantityVacationDays(false) : vacationTransactionsByDate.Where(w => w.IsVacationSavedYear3()).GetQuantityVacationDays(true);
                    else if (vacationTransactionsByDate.ToList().HasVacationSavedYear4Transactions())
                        periodUsedDaysYear4Count += !calculateHours ? vacationTransactionsByDate.GetQuantityVacationDays(false) : vacationTransactionsByDate.Where(w => w.IsVacationSavedYear4()).GetQuantityVacationDays(true);
                    else if (vacationTransactionsByDate.ToList().HasVacationSavedYear5Transactions())
                        periodUsedDaysYear5Count += !calculateHours ? vacationTransactionsByDate.GetQuantityVacationDays(false) : vacationTransactionsByDate.Where(w => w.IsVacationSavedYear5()).GetQuantityVacationDays(true);
                    else if (vacationTransactionsByDate.ToList().HasVacationSavedOverdueTransactions())
                        periodUsedDaysOverdueCount += !calculateHours ? vacationTransactionsByDate.GetQuantityVacationDays(false) : vacationTransactionsByDate.Where(w => w.IsVacationSavedOverdue()).GetQuantityVacationDays(true);
                }
            }

            #endregion

            #region FinalSalary

            decimal periodVacationCompensationPaidCount = 0;
            decimal periodVacationCompensationSavedYear1Count = 0;
            decimal periodVacationCompensationSavedYear2Count = 0;
            decimal periodVacationCompensationSavedYear3Count = 0;
            decimal periodVacationCompensationSavedYear4Count = 0;
            decimal periodVacationCompensationSavedYear5Count = 0;
            decimal periodVacationCompensationSavedOverdueCount = 0;

            var vacationCompensationTransactions = l.GetVacationCompensationTransactions().Where(x => !x.IsRetroTransaction()).ToList();

            foreach (var vacationCompensationTransactionsByDate in vacationCompensationTransactions.GroupBy(x => x.TimeBlockDateId))
            {
                foreach (var vacationCompensationTransactionsByDateAndType in vacationCompensationTransactionsByDate.GroupBy(x => x.SysPayrollTypeLevel3))
                {
                    if (vacationCompensationTransactionsByDateAndType.ToList().HasVacationCompensationPaidTransactions())
                        periodVacationCompensationPaidCount += !calculateHours ? vacationCompensationTransactionsByDateAndType.GetQuantityVacationDays(false) : vacationCompensationTransactionsByDateAndType.Where(w => w.IsVacationCompensationPaid()).GetQuantityVacationDays(true);
                    else if (vacationCompensationTransactionsByDateAndType.ToList().HasVacationCompensationSavedYear1Transactions())
                        periodVacationCompensationSavedYear1Count += !calculateHours ? vacationCompensationTransactionsByDateAndType.GetQuantityVacationDays(false) : vacationCompensationTransactionsByDateAndType.Where(w => w.IsVacationCompensationSavedYear1()).GetQuantityVacationDays(true);
                    else if (vacationCompensationTransactionsByDateAndType.ToList().HasVacationCompensationSavedYear2Transactions())
                        periodVacationCompensationSavedYear2Count += !calculateHours ? vacationCompensationTransactionsByDateAndType.GetQuantityVacationDays(false) : vacationCompensationTransactionsByDateAndType.Where(w => w.IsVacationCompensationSavedYear2()).GetQuantityVacationDays(true);
                    else if (vacationCompensationTransactionsByDateAndType.ToList().HasVacationCompensationSavedYear3Transactions())
                        periodVacationCompensationSavedYear3Count += !calculateHours ? vacationCompensationTransactionsByDateAndType.GetQuantityVacationDays(false) : vacationCompensationTransactionsByDateAndType.Where(w => w.IsVacationCompensationSavedYear3()).GetQuantityVacationDays(true);
                    else if (vacationCompensationTransactionsByDateAndType.ToList().HasVacationCompensationSavedYear4Transactions())
                        periodVacationCompensationSavedYear4Count += !calculateHours ? vacationCompensationTransactionsByDateAndType.GetQuantityVacationDays(false) : vacationCompensationTransactionsByDateAndType.Where(w => w.IsVacationCompensationSavedYear4()).GetQuantityVacationDays(true);
                    else if (vacationCompensationTransactionsByDateAndType.ToList().HasVacationCompensationSavedYear5Transactions())
                        periodVacationCompensationSavedYear5Count += !calculateHours ? vacationCompensationTransactionsByDateAndType.GetQuantityVacationDays(false) : vacationCompensationTransactionsByDateAndType.Where(w => w.IsVacationCompensationSavedYear5()).GetQuantityVacationDays(true);
                    else if (vacationCompensationTransactionsByDateAndType.ToList().HasVacationCompensationSavedOverdueTransactions())
                        periodVacationCompensationSavedOverdueCount += !calculateHours ? vacationCompensationTransactionsByDateAndType.GetQuantityVacationDays(false) : vacationCompensationTransactionsByDateAndType.Where(w => w.IsVacationCompensationSavedOverdue()).GetQuantityVacationDays(true);
                }
            }

            #endregion


            #region VacationAddidtion

            decimal periodVacationAddition = 0;
            decimal periodVariableVacationAddition = 0;

            var vacationAdditionTransactions = l.GetVacationAdditionWithVariableTransactions().Where(x => !x.IsRetroTransaction()).ToList();
            foreach (var vacationAdditionTransactionsByDate in vacationAdditionTransactions.GroupBy(x => x.TimeBlockDateId))
            {
                foreach (var vacationAdditionTransactionsByDateAndType in vacationAdditionTransactionsByDate.GroupBy(x => x.SysPayrollTypeLevel2))
                {
                    if (vacationAdditionTransactionsByDateAndType.ToList().HasVacationAdditionTransactions())
                        periodVacationAddition += vacationAdditionTransactionsByDateAndType.GetQuantityVacationDays(false);
                    else if (vacationAdditionTransactionsByDateAndType.ToList().HasVariableVacationAdditionTransactions())
                        periodVariableVacationAddition += vacationAdditionTransactionsByDateAndType.GetQuantityVacationDays(false);
                }
            }

            #endregion

            #region Prepayment

            decimal periodVacationPrepaymentPaid = 0;
            decimal periodVacationPrepaymentInvert = 0;
            decimal periodVariableVariablePrepaymentPaid = 0;
            decimal periodVariableVariablePrepaymentInvert = 0;

            var vacationPrepaymentTransactions = l.GetVacationPrepaymentAndVariableTransactions().Where(x => !x.IsRetroTransaction()).ToList();
            foreach (var transaction in vacationPrepaymentTransactions)
            {
                if (transaction.IsVacationAdditionOrSalaryPrepaymentPaid())
                    periodVacationPrepaymentPaid += transaction.GetQuantityVacationDays(false);
                else if (transaction.IsVacationAdditionOrSalaryPrepaymentInvert())
                    periodVacationPrepaymentInvert += transaction.GetQuantityVacationDays(false);
                else if (transaction.IsVacationAdditionOrSalaryVariablePrepaymentPaid())
                    periodVariableVariablePrepaymentPaid += transaction.GetQuantityVacationDays(false);
                else if (transaction.IsVacationAdditionOrSalaryVariablePrepaymentInvert())
                    periodVariableVariablePrepaymentInvert += transaction.GetQuantityVacationDays(false);
            }

            #endregion

            #region DebtAdvanceAmount

            decimal debtAdvanceAmount = l.GetVacationCompensationAdvanceTransactions().Where(x => !x.IsRetroTransaction() && x.Amount.HasValue).Sum(x => x.Amount.Value); //Reglering av skuld i samband med slutlön
            debtAdvanceAmount += l.GetVacationSalaryAdvanceTransactions().Where(x => !x.IsRetroTransaction() && x.Amount.HasValue).Sum(x => x.Amount.Value);//Ökning av skuld vid uttag av förskottsdagar

            #endregion

            vacationDaysCalculationDTO.PeriodUsedDaysPaidCount = periodUsedDaysPaidCount;                                           //Betalda dagar
            vacationDaysCalculationDTO.PeriodUsedDaysUnpaidCount = periodUsedDaysUnpaidCount;                                       //Obetalda dagar
            vacationDaysCalculationDTO.PeriodUsedDaysAdvanceCount = periodUsedDaysAdvanceCount;                                     //Förskott
            vacationDaysCalculationDTO.PeriodUsedDaysYear1Count = periodUsedDaysYear1Count;                                         //Sparade år 1
            vacationDaysCalculationDTO.PeriodUsedDaysYear2Count = periodUsedDaysYear2Count;                                         //Sparade år 2
            vacationDaysCalculationDTO.PeriodUsedDaysYear3Count = periodUsedDaysYear3Count;                                         //Sparade år 3
            vacationDaysCalculationDTO.PeriodUsedDaysYear4Count = periodUsedDaysYear4Count;                                         //Sparade år 4
            vacationDaysCalculationDTO.PeriodUsedDaysYear5Count = periodUsedDaysYear5Count;                                         //Sparade år 5   
            vacationDaysCalculationDTO.PeriodUsedDaysOverdueCount = periodUsedDaysOverdueCount;                                     //Förfallna dagar
            vacationDaysCalculationDTO.PeriodVacationCompensationPaidCount = periodVacationCompensationPaidCount;                   //Slutlön - Betalda dagar
            vacationDaysCalculationDTO.PeriodVacationCompensationSavedYear1Count = periodVacationCompensationSavedYear1Count;       //Slutlön -Sparade år 1
            vacationDaysCalculationDTO.PeriodVacationCompensationSavedYear2Count = periodVacationCompensationSavedYear2Count;       //Slutlön -Sparade år 2
            vacationDaysCalculationDTO.PeriodVacationCompensationSavedYear3Count = periodVacationCompensationSavedYear3Count;       //Slutlön -Sparade år 3
            vacationDaysCalculationDTO.PeriodVacationCompensationSavedYear4Count = periodVacationCompensationSavedYear4Count;       //Slutlön -Sparade år 4
            vacationDaysCalculationDTO.PeriodVacationCompensationSavedYear5Count = periodVacationCompensationSavedYear5Count;       //Slutlön -Sparade år 5
            vacationDaysCalculationDTO.PeriodVacationCompensationSavedOverdueCount = periodVacationCompensationSavedOverdueCount;   //Slutlön -Förfallna dagar
            vacationDaysCalculationDTO.PeriodVacationAddition = periodVacationAddition;                                           //Semestertillägg
            vacationDaysCalculationDTO.PeriodVariableVacationAddition = periodVariableVacationAddition;                           //Rörligt Semestertillägg
            vacationDaysCalculationDTO.PeriodVacationPrepaymentPaid = periodVacationPrepaymentPaid;                                 //Förutbetald - Utbetald
            vacationDaysCalculationDTO.PeriodVacationPrepaymentInvert = periodVacationPrepaymentInvert;                             //Förutbetald - Motbokning
            vacationDaysCalculationDTO.PeriodVariablePrepaymentPaid = periodVariableVariablePrepaymentPaid;                         //Förutbetald rörligt - Utbetald
            vacationDaysCalculationDTO.PeriodVariablePrepaymentInvert = periodVariableVariablePrepaymentInvert;                      //Förutbetald rörligt - Motbokning
            vacationDaysCalculationDTO.PeriodDebtAdvanceAmount = debtAdvanceAmount;                                                 //Summa förskott

            return vacationDaysCalculationDTO;
        }

        public static List<TimeBlockDate> GetTimeBlockDates(this IEnumerable<TimePayrollTransaction> l)
        {
            var timeBlockDates = new List<TimeBlockDate>();
            if (l.IsNullOrEmpty())
                return timeBlockDates;

            foreach (var e in l)
            {
                if (!timeBlockDates.Any(i => i.TimeBlockDateId == e.TimeBlockDateId))
                    timeBlockDates.Add(e.TimeBlockDate);
            }
            return timeBlockDates.OrderBy(i => i.Date).ToList();
        }

        public static List<TimeBlockDate> GetTimeBlockDatesOutSideInputDates(this List<TimePayrollTransaction> l, List<TimeBlockDate> inputCollection)
        {
            var timeBlockDateIdsOutSideInputDates = l.GetTimeBlockDateIdsOutSideInputDates(inputCollection).Distinct();
            var timeBlockDates = l.GetTimeBlockDates();
            List<TimeBlockDate> timeBlockDatesOutSideInputDates = new List<TimeBlockDate>();
            foreach (var timeBlockDateId in timeBlockDateIdsOutSideInputDates)
            {
                var timeBlockDate = timeBlockDates.FirstOrDefault(x => x.TimeBlockDateId == timeBlockDateId);
                if (timeBlockDate != null)
                    timeBlockDatesOutSideInputDates.Add(timeBlockDate);
            }
            return timeBlockDatesOutSideInputDates;
        }

        public static List<DateTime> GetDates(this List<TimePayrollTransaction> l)
        {
            return l?.Where(e => e.TimeBlockDate != null).Select(e => e.TimeBlockDate.Date).Distinct().OrderBy(d => d.Date).ToList();
        }

        public static List<DateTime> GetDatesOutSideInputDates(this List<TimePayrollTransaction> l, List<DateTime> inputCollection)
        {
            List<DateTime> datesOutSideInputDates = new List<DateTime>();
            foreach (var e in l)
            {
                if (!inputCollection.Any(x => x.Date == e.TimeBlockDate.Date.Date) && !datesOutSideInputDates.Any(i => i == e.TimeBlockDate.Date.Date))
                    datesOutSideInputDates.Add(e.TimeBlockDate.Date.Date);
            }
            return datesOutSideInputDates.Distinct().ToList();
        }

        public static List<DateTime> GetAbsenceDates(this List<TimePayrollTransaction> l, List<TimeBlockDate> timeBlockDates)
        {
            List<int> timeBlockDateIds = l?.Where(i => i.IsAbsence()).Select(i => i.TimeBlockDateId).Distinct().ToList();
            if (timeBlockDateIds.IsNullOrEmpty())
                return new List<DateTime>();

            return timeBlockDates?.Where(i => timeBlockDateIds.Contains(i.TimeBlockDateId)).Select(i => i.Date).ToList() ?? new List<DateTime>();
        }

        public static DateTime? GetFirstDate(this List<TimePayrollTransaction> l)
        {
            return l.GetFirst()?.TimeBlockDate?.Date;
        }

        public static DateTime? GetLastDate(this List<TimePayrollTransaction> l)
        {
            return l.GetLast()?.TimeBlockDate?.Date;
        }

        public static DateTime? GetLastUnlockedDate(this List<TimePayrollTransaction> l, DateTime dateFrom, DateTime dateTo, List<int> lockedAttestStateIds)
        {
            DateTime? lastValid = null;

            DateTime date = dateTo;
            while (date >= dateFrom)
            {
                List<int> attestStatesIdsForDay = l.Where(t => t.TimeBlockDate?.Date == date).Select(t => t.AttestStateId).Distinct().ToList();
                if (attestStatesIdsForDay.Intersect(lockedAttestStateIds).Any())
                    return lastValid ?? dateTo.AddDays(1); //If first day (dateTo) we check is locked then invalidate whole range                  

                lastValid = date;
                date = date.AddDays(-1);
            }

            return lastValid;
        }

        public static List<int> GetTimeBlockDateIdsOutSideInputDates(this List<TimePayrollTransaction> l, List<TimeBlockDate> inputCollection)
        {
            var timeBlockDateIds = new List<int>();
            foreach (var e in l)
            {
                if (!inputCollection.Any(i => i.TimeBlockDateId == e.TimeBlockDateId) && !timeBlockDateIds.Any(i => i == e.TimeBlockDateId))
                    timeBlockDateIds.Add(e.TimeBlockDateId);
            }
            return timeBlockDateIds.Distinct().ToList();
        }

        public static List<int> GetLevel3Ids(this IEnumerable<TimePayrollTransaction> l)
        {
            return l?.Where(e => e.SysPayrollTypeLevel3.HasValue).Select(e => e.SysPayrollTypeLevel3.Value).ToList() ?? new List<int>();
        }

        public static List<int> GetProductIds<T>(this IEnumerable<T> l) where T : IPayrollTransaction
        {
            return l?.Select(t => t.ProductId).Distinct().ToList() ?? new List<int>();
        }

        public static List<int> GetAttestStateIds(this IEnumerable<TimePayrollTransaction> l)
        {
            return l?.Select(i => i.AttestStateId).Distinct().ToList() ?? new List<int>();
        }

        public static List<T> Filter<T>(this IEnumerable<T> l, DateTime start, DateTime stop) where T : ITransactionProc
        {
            return l?.Where(t => t.Date >= start && t.Date <= stop).ToList() ?? new List<T>();
        }

        public static AccountInternal GetAccountInternal(this TimePayrollTransaction e, int accountDimId)
        {
            if (e == null || e.AccountInternal.IsNullOrEmpty())
                return null;

            try
            {
                foreach (AccountInternal accountInternal in e.AccountInternal)
                {
                    if (!accountInternal.AccountReference.IsLoaded)
                    {
                        accountInternal.AccountReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("TimePayrollTransaction.cs accountInternal.AccountReference");
                    }
                }
            }
            catch (Exception ex) { ex.ToString(); }

            return e.AccountInternal?.FirstOrDefault(i => i.Account?.AccountDimId == accountDimId);
        }

        public static AccountDTO GetAccountInternal(this TimePayrollTransaction e, int accountDimId, List<AccountDTO> accountInternals)
        {
            if (e == null || e.AccountInternal.IsNullOrEmpty() || accountInternals.IsNullOrEmpty())
                return null;

            //foreach (AccountInternal accountInternal in e.AccountInternal)
            List<AccountInternal> timePayrollTransactionAccounts = e.AccountInternal.ToList();
            for (int i = timePayrollTransactionAccounts.Count - 1; i >= 0; i--)
            {
                AccountInternal accountInternal = timePayrollTransactionAccounts[i];
                AccountDTO account = accountInternals.FirstOrDefault(a => a.AccountId == accountInternal.AccountId);
                if (account?.AccountDimId == accountDimId)
                    return account;
            }

            return null;
        }

        public static void TurnAmounts(this TimePayrollTransaction e)
        {
            if (e == null)
                return;

            e.Amount = NumberUtility.TurnAmount(e.Amount);
            e.AmountCurrency = NumberUtility.TurnAmount(e.AmountCurrency);
            e.AmountEntCurrency = NumberUtility.TurnAmount(e.AmountEntCurrency);
            e.AmountLedgerCurrency = NumberUtility.TurnAmount(e.AmountLedgerCurrency);
            e.VatAmount = NumberUtility.TurnAmount(e.VatAmount);
            e.VatAmountCurrency = NumberUtility.TurnAmount(e.VatAmountCurrency);
            e.VatAmountEntCurrency = NumberUtility.TurnAmount(e.VatAmountEntCurrency);
            e.VatAmountLedgerCurrency = NumberUtility.TurnAmount(e.VatAmountLedgerCurrency);
        }

        public static void GetChain(this List<TimePayrollTransaction> l, TimePayrollTransaction parentTransaction, List<TimePayrollTransaction> chainedTransactions)
        {
            if (chainedTransactions == null)
                chainedTransactions = new List<TimePayrollTransaction>();

            if (!chainedTransactions.Any())
                chainedTransactions.Add(parentTransaction);

            var childTransaction = l.FirstOrDefault(x => x.ParentId.HasValue && x.ParentId.Value == parentTransaction.TimePayrollTransactionId);
            if (childTransaction != null)
            {
                chainedTransactions.Add(childTransaction);
                GetChain(l, childTransaction, chainedTransactions);
            }
        }

        public static decimal GetQuantityVacationDays(this TimePayrollTransaction e, bool useHoursVacation)
        {
            if (e.IsAdded || e.IsVacationAdditionOrSalaryPrepayment() || e.IsVacationAdditionOrSalaryVariablePrepayment()) //may not have an extended
                return e.Quantity;
            if (e.TimePayrollTransactionExtended == null)
                return 0;

            if (e.TimePayrollTransactionExtended.TimeUnit == (int)TermGroup_PayrollProductTimeUnit.WorkDays)
                return e.TimePayrollTransactionExtended.QuantityWorkDays;
            else if (e.TimePayrollTransactionExtended.TimeUnit == (int)TermGroup_PayrollProductTimeUnit.CalenderDays || e.TimePayrollTransactionExtended.TimeUnit == (int)TermGroup_PayrollProductTimeUnit.CalenderDayFactor || e.TimePayrollTransactionExtended.TimeUnit == (int)TermGroup_PayrollProductTimeUnit.VacationCoefficient)
                return e.TimePayrollTransactionExtended.QuantityCalendarDays;
            else if (useHoursVacation && e.TimePayrollTransactionExtended.TimeUnit == (int)TermGroup_PayrollProductTimeUnit.Hours)
                return e.TimePayrollTransactionExtended.QuantityCalendarDays;
            else if (e.Quantity < 0 && e.TimePayrollTransactionExtended.TimeUnit == (int)TermGroup_PayrollProductTimeUnit.Hours)
                return -1;
            else
                return 1; //this can only be TermGroup_PayrollProductTimeUnit.Hours?
        }

        public static string GetAccountingIdString(this TimePayrollTransaction e)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(e.AccountStdId.ToString());

            if (e.AccountInternal != null)
            {
                foreach (var accountInternal in e.AccountInternal.OrderBy(x => x.AccountId).ToList())
                {
                    if (sb.Length > 0)
                        sb.Append(",");

                    sb.Append(accountInternal.AccountId);
                }
            }
            return sb.ToString();
        }

        public static string GetFormulaExtracted(this TimePayrollTransaction e)
        {
            return e != null && e.IsExtended ? e.TimePayrollTransactionExtended.FormulaExtracted : string.Empty;
        }

        public static string GetAccountingString(this TimePayrollTransaction e, List<AccountDim> accountDims)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(e.AccountStdId.ToString());

            if (!accountDims.IsNullOrEmpty() && !e.AccountInternal.IsNullOrEmpty())
            {
                foreach (AccountDim accountDim in accountDims.Where(i => !i.IsStandard).OrderBy(i => i.AccountDimNr))
                {
                    AccountInternal accountInternal = e.AccountInternal.FirstOrDefault(i => i.Account.AccountDimId == accountDim.AccountDimId);
                    if (accountInternal != null)
                    {
                        if (sb.Length > 0)
                            sb.Append(",");
                        sb.Append(accountInternal.AccountId.ToString());
                    }
                }
            }

            return sb.ToString();
        }

        public static decimal GetPaidTime(this List<TimePayrollTransaction> l)
        {
            if (l.IsNullOrEmpty())
                return 0;

            decimal paidTime = 0;
            foreach (var e in l)
            {
                paidTime += e.GetPaidTime();
            }
            return paidTime;
        }

        public static decimal GetPaidTime(this TimePayrollTransaction e)
        {
            decimal paidWork = e.IsWork() ? e.Quantity : 0;
            decimal paidAbsence = e.IsAbsencePermission() ? e.Quantity : 0;
            return paidWork + paidAbsence;
        }

        public static decimal GetQuantityVacationDays(this IEnumerable<TimePayrollTransaction> l, bool calculateHoursVacation)
        {
            //l = transactions for a specific date
            if (!l.Any())
                return 0;

            decimal quantity = 0;

            var addedTransactions = l.Where(x => x.IsAdded).ToList();
            var otherTransactions = l.Where(x => !x.IsAdded).ToList();

            if (addedTransactions.Any())
                addedTransactions.ForEach(x => quantity += x.GetQuantityVacationDays(false));

            if (otherTransactions.Any())
            {
                if (calculateHoursVacation)
                    otherTransactions.ToList().ForEach(x => quantity += x.GetQuantityVacationDays(true));
                else
                    quantity += otherTransactions.FirstOrDefault().GetQuantityVacationDays(false);
            }

            return quantity;
        }

        public static int GetAbsenceMinutes(this IEnumerable<TimePayrollTransaction> l)
        {
            return l.GetAbsence().GetMinutes();
        }

        public static int GetMinutes(this IEnumerable<TimePayrollTransaction> l)
        {
            return (int)(l?.Sum(e => e.Quantity) ?? 0);
        }

        public static int GetTimeCodeMinutes(this TimePayrollTransaction e)
        {
            if (e.TimeCodeTransaction == null)
                return 0;
            return (int)e.TimeCodeTransaction.Stop.Subtract(e.TimeCodeTransaction.Start).TotalMinutes;
        }

        public static DateTime? GetFirstDay(this IEnumerable<TimePayrollTransaction> l)
        {
            if (l.IsNullOrEmpty() || l.Any(e => e.TimeBlockDate == null))
                return null;
            return l.OrderBy(i => i.TimeBlockDate.Date).FirstOrDefault()?.Date;
        }

        public static int GetNrOfDays(this IEnumerable<TimePayrollTransaction> l)
        {
            return l.Select(i => i.TimeBlockDateId).Distinct().Count();
        }

        public static bool HasAccountStd(this TimePayrollTransaction e)
        {
            return e.AccountStdId > 0 || e.AccountStd != null;
        }

        public static bool HasAccountInternals(this TimePayrollTransaction e)
        {
            return e.AccountInternal != null && e.AccountInternal.Count > 0;
        }

        public static void RemoveAccountInternal(this TimePayrollTransaction e, int accountDimId)
        {
            var accountInternal = e.AccountInternal.FirstOrDefault(i => i.Account != null && i.Account.AccountDimId == accountDimId);

            if (accountInternal != null)
                e.AccountInternal.Remove(accountInternal);
        }

        public static bool HasAccountInternal(this TimePayrollTransaction e, int accountDimId)
        {
            return e.GetAccountInternal(accountDimId) != null;
        }

        public static bool HasVacationPaidTransactions(this List<TimePayrollTransaction> l)
        {
            return l.Any(x => x.SysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_Paid ||
                              x.SysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_Paid_Secondary);
        }

        public static bool HasVacationUnpaidTransactions(this List<TimePayrollTransaction> l)
        {
            return l?.Any(e => e.SysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_Unpaid ||
                               e.SysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_Unpaid_Secondary) ?? false;
        }

        public static bool HasVacationAdvanceTransactions(this List<TimePayrollTransaction> l)
        {
            return l?.Any(e => e.SysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_Advance ||
                               e.SysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_Advance_Secondary) ?? false;
        }

        public static bool HasVacationSavedYear1Transactions(this List<TimePayrollTransaction> l)
        {
            return l?.Any(e => e.SysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_SavedYear1 ||
                               e.SysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_SavedYear1_Secondary) ?? false;
        }

        public static bool HasVacationSavedYear2Transactions(this List<TimePayrollTransaction> l)
        {
            return l?.Any(e => e.SysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_SavedYear2 ||
                                e.SysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_SavedYear2_Secondary) ?? false;
        }

        public static bool HasVacationSavedYear3Transactions(this List<TimePayrollTransaction> l)
        {
            return l?.Any(e => e.SysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_SavedYear3 ||
                               e.SysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_SavedYear3_Secondary) ?? false;
        }

        public static bool HasVacationSavedYear4Transactions(this List<TimePayrollTransaction> l)
        {
            return l?.Any(e => e.SysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_SavedYear4 ||
                               e.SysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_SavedYear4_Secondary) ?? false;
        }

        public static bool HasVacationSavedYear5Transactions(this List<TimePayrollTransaction> l)
        {
            return l?.Any(e => e.SysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_SavedYear5 ||
                               e.SysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_SavedYear5_Secondary) ?? false;
        }

        public static bool HasVacationSavedOverdueTransactions(this List<TimePayrollTransaction> l)
        {
            return l?.Any(e => e.SysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_SavedOverdue ||
                               e.SysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_SavedOverdue_Secondary) ?? false;
        }

        public static bool HasVacationCompensationPaidTransactions(this List<TimePayrollTransaction> l)
        {
            return l?.Any(e => e.SysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_Paid) ?? false;
        }

        public static bool HasVacationCompensationSavedYear1Transactions(this List<TimePayrollTransaction> l)
        {
            return l?.Any(e => e.SysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_SavedYear1) ?? false;
        }

        public static bool HasVacationCompensationSavedYear2Transactions(this List<TimePayrollTransaction> l)
        {
            return l?.Any(e => e.SysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_SavedYear2) ?? false;
        }

        public static bool HasVacationCompensationSavedYear3Transactions(this List<TimePayrollTransaction> l)
        {
            return l?.Any(e => e.SysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_SavedYear3) ?? false;
        }

        public static bool HasVacationCompensationSavedYear4Transactions(this List<TimePayrollTransaction> l)
        {
            return l?.Any(e => e.SysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_SavedYear4) ?? false;
        }

        public static bool HasVacationCompensationSavedYear5Transactions(this List<TimePayrollTransaction> l)
        {
            return l?.Any(e => e.SysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_SavedYear5) ?? false;
        }

        public static bool HasVacationCompensationSavedOverdueTransactions(this List<TimePayrollTransaction> l)
        {
            return l?.Any(e => e.SysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_SavedOverdue) ?? false;
        }

        public static bool HasVacationAdditionTransactions(this List<TimePayrollTransaction> l)
        {
            return l?.Any(x => x.IsVacationAddition()) ?? false;
        }

        public static bool HasVariableVacationAdditionTransactions(this List<TimePayrollTransaction> l)
        {
            return l?.Any(x => x.IsVacationAdditionVariable()) ?? false;
        }

        public static bool IsValidForRetro(this TimePayrollTransaction e)
        {
            return (
                (e.IsGrossSalary() && !e.IsVacationCompensationDirectPaid()) ||
                e.IsAddition() ||
                e.IsDeductionHouseKeeping() ||
                e.IsDeductionOther() ||
                e.IsCompensation() ||
                (e.IsBenefit() && !e.IsBenefitInvert() && !e.IsBenefitCompanyCar()) ||
                e.IsCostDeduction() ||
                e.IsOccupationalPension()
                ) && !e.PayrollStartValueRowId.HasValue;
        }

        public static bool IsRetroTransaction(this TimePayrollTransaction e)
        {
            return e.RetroactivePayrollOutcomeId.HasValue;
        }

        public static bool IsMonthlySalaryAndFixed(this TimePayrollTransaction e)
        {
            return e.IsMonthlySalary() && e.IsFixed;
        }

        public static bool HasAttestStateNoneInitial(this List<TimePayrollTransaction> l, int initialAttestStateId)
        {
            return l?.Any(e => e.HasAttestStateNoneInitial(initialAttestStateId)) ?? false;
        }

        public static bool HasAttestStateNoneInitial(this TimePayrollTransaction e, int initialAttestStateId)
        {
            return e != null && !e.IsReversed && e.AttestStateId != initialAttestStateId;
        }

        public static bool HasAmounts<T>(this List<T> l) where T: ITransactionProc
        {
            return !l.IsNullOrEmpty() && l.Any(t => t.Amount.HasValue && t.Amount != 0);
        }

        public static bool HasTransaction(this List<TimePayrollTransaction> l, DateTime date)
        {
            return l?.Any(i => i.TimeBlockDate?.Date == date) ?? false;
        }

        #endregion
    }
}
