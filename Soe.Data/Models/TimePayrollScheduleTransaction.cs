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
    public partial class TimePayrollScheduleTransaction : IPayrollScheduleTransaction, ICreatedModified, IState, ITask
    {
        public DateTime Date
        {
            get
            {
                return this.TimeBlockDate?.Date ?? CalendarUtility.DATETIME_DEFAULT;
            }
        }
        public bool IsRetroactive
        {
            get
            {
                return this.RetroactivePayrollOutcomeId.HasValue;
            }
        }
    }

    public partial class GetTimePayrollScheduleTransactionsForEmployee_Result : IPayrollScheduleTransaction
    {
        #region IPayrollType

        public int? SysPayrollTypeLevel1 => this.TransactionSysPayrollTypeLevel1;
        public int? SysPayrollTypeLevel2 => this.TransactionSysPayrollTypeLevel2;
        public int? SysPayrollTypeLevel3 => this.TransactionSysPayrollTypeLevel3;
        public int? SysPayrollTypeLevel4 => this.TransactionSysPayrollTypeLevel4;

        #endregion
    }

    public partial class GetTimePayrollScheduleTransactionAccountsForEmployee_Result : IAccountId
    {
        public GetTimePayrollScheduleTransactionAccountsForEmployee_Result()
        {

        }
    }

    public static partial class EntityExtensions
    {
        #region TimePayrollScheduleTransaction

        public static List<TimePayrollScheduleTransaction> Get(this IEnumerable<TimePayrollScheduleTransaction> l, SoeTimePayrollScheduleTransactionType type, int timeBlockDateId)
        {
            return l?.Where(i => i.Type == (int)type && i.TimeBlockDateId == timeBlockDateId).ToList() ?? new List<TimePayrollScheduleTransaction>();
        }

        public static List<TimePayrollScheduleTransaction> GetGrossSalaryAndBenefitTransactions(this IEnumerable<TimePayrollScheduleTransaction> l)
        {
            return l?.Where(x => x.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary || x.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit).ToList() ?? new List<TimePayrollScheduleTransaction>();
        }

        public static AccountInternal GetAccountInternal(this TimePayrollScheduleTransaction e, int accountDimId)
        {
            try
            {
                foreach (var accountInternal in e.AccountInternal)
                {
                    if (!accountInternal.AccountReference.IsLoaded)
                    { 
                        accountInternal.AccountReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("TimePayrollScheduleTransaction.cs accountInternal.AccountReference");
                    }
                }
            }
            catch (Exception ex) { ex.ToString(); }

            return e.AccountInternal?.FirstOrDefault(i => i.Account?.AccountDimId == accountDimId);
        }

        public static List<AttestPayrollTransactionDTO> CreateTransactionItems(this IEnumerable<TimePayrollScheduleTransaction> l, List<AccountDimDTO> accountDims = null)
        {
            var transactions = new List<AttestPayrollTransactionDTO>();

            foreach (var e in l)
            {
                transactions.Add(e.CreateTransactionItem(e.AccountInternal != null ? e.AccountInternal.ToAccountDTOs().ToList() : new List<AccountDTO>(), e.AccountStd != null ? e.AccountStd.ToDTO() : new AccountDTO() { AccountId = e.AccountStdId }, accountDims));
            }

            return transactions;
        }

        public static AttestPayrollTransactionDTO CreateTransactionItem(this TimePayrollScheduleTransaction e, List<AccountDTO> accountInternals = null, AccountDTO accountStd = null, List<AccountDimDTO> accountDims = null, string attestStateName = null)
        {
            var transactionItem = new AttestPayrollTransactionDTO()
            {
                //TimePayrollTransaction
                EmployeeId = e.EmployeeId,
                TimePayrollTransactionId = e.TimePayrollScheduleTransactionId,
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
                IsPreliminary = false,
                IsExported = false,
                Comment = null,
                HasInfo = false,
                IsAdded = false,
                AddedDateFrom = null,
                AddedDateTo = null,
                IsFixed = false,
                IsSpecifiedUnitPrice = false,
                IsAdditionOrDeduction = false,
                UnionFeeId = null,
                EarningTimeAccumulatorId = null,
                EmployeeVehicleId = null,
                PayrollStartValueRowId = null,
                RetroactivePayrollOutcomeId = e.RetroactivePayrollOutcomeId,
                IsVacationFiveDaysPerWeek = false,
                TransactionSysPayrollTypeLevel1 = e.SysPayrollTypeLevel1,
                TransactionSysPayrollTypeLevel2 = e.SysPayrollTypeLevel2,
                TransactionSysPayrollTypeLevel3 = e.SysPayrollTypeLevel3,
                TransactionSysPayrollTypeLevel4 = e.SysPayrollTypeLevel4,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = null,
                ModifiedBy = null,

                //TimePayrollTransactionExtended
                PayrollPriceFormulaId = e.PayrollPriceFormulaId,
                PayrollPriceTypeId = e.PayrollPriceTypeId,
                Formula = e.Formula,
                FormulaPlain = e.FormulaPlain,
                FormulaExtracted = e.FormulaExtracted,
                FormulaNames = e.FormulaNames,
                FormulaOrigin = e.FormulaOrigin,
                PayrollCalculationPerformed = false,
                TimeUnit = (int)TermGroup_PayrollProductTimeUnit.Hours,
                QuantityWorkDays = 0,
                QuantityCalendarDays = 0,
                CalenderDayFactor = 0,
                IsDistributed = false,

                //TimePayrollScheduleTransaction
                IsScheduleTransaction = true,
                ScheduleTransactionType = (SoeTimePayrollScheduleTransactionType)e.Type,

                //PayrollProduct
                PayrollProductId = e.ProductId,
                PayrollProductNumber = e.PayrollProduct?.Number ?? string.Empty,
                PayrollProductName = e.PayrollProduct?.Name ?? string.Empty,
                PayrollProductShortName = e.PayrollProduct?.ShortName ?? string.Empty,
                PayrollProductFactor = e.PayrollProduct?.Factor ?? 0,
                PayrollProductPayed = e.PayrollProduct?.Payed ?? false,
                PayrollProductExport = e.PayrollProduct?.Export ?? false,
                PayrollProductUseInPayroll = e.PayrollProduct?.UseInPayroll ?? false,
                IsAverageCalculated = e.PayrollProduct?.AverageCalculated ?? false,
                PayrollProductSysPayrollTypeLevel1 = e.PayrollProduct?.SysPayrollTypeLevel1 ?? (int)TermGroup_SysPayrollType.None,
                PayrollProductSysPayrollTypeLevel2 = e.PayrollProduct?.SysPayrollTypeLevel2 ?? (int)TermGroup_SysPayrollType.None,
                PayrollProductSysPayrollTypeLevel3 = e.PayrollProduct?.SysPayrollTypeLevel3 ?? (int)TermGroup_SysPayrollType.None,
                PayrollProductSysPayrollTypeLevel4 = e.PayrollProduct?.SysPayrollTypeLevel4 ?? (int)TermGroup_SysPayrollType.None,

                //TimeCodeTransaction
                TimeCodeTransactionId = 0,
                StartTime = e.TimeBlockStartTime,
                StopTime = e.TimeBlockStopTime,

                //TimeCode
                TimeCodeType = SoeTimeCodeType.None,
                TimeCodeRegistrationType = TermGroup_TimeCodeRegistrationType.Unknown,
                NoOfPresenceWorkOutsideScheduleTime = null,
                NoOfAbsenceAbsenceTime = null,

                //TimeBlockDate
                TimeBlockDateId = e.TimeBlockDateId,
                Date = e.TimeBlockDate != null ? e.TimeBlockDate.Date : CalendarUtility.DATETIME_DEFAULT,

                //TimeBlock
                TimeBlockId = null,

                //AttestState
                AttestStateId = 0,
                AttestStateName = attestStateName ?? string.Empty,
                AttestStateColor = string.Empty,
                AttestStateInitial = false,
                AttestStateSort = -1,
                HasSameAttestState = true,

                //Accounting
                AccountDims = accountDims ?? new List<AccountDimDTO>(),
                AccountStd = accountStd ?? new AccountDTO(),
                AccountInternals = accountInternals ?? new List<AccountDTO>(),

                //TimePeriod
                TimePeriodId = e.TimePeriodId,
                TimePeriodName = e.TimePeriod?.Name ?? string.Empty,

                //Flags
                IsPresence = true,
            };

            transactionItem.SetAccountingStrings();

            return transactionItem;
        }

        public static List<int> GetProductIds<T>(this List<T> l) where T: IPayrollScheduleTransaction
        {
            return l?.Select(t => t.ProductId).Distinct().ToList() ?? new List<int>();
        }

        public static void SetTimePayrollTransactionType(this TimePayrollScheduleTransaction e, PayrollProduct payrollProduct)
        {
            if (payrollProduct != null)
            {
                e.SysPayrollTypeLevel1 = payrollProduct.SysPayrollTypeLevel1;
                e.SysPayrollTypeLevel2 = payrollProduct.SysPayrollTypeLevel2;
                e.SysPayrollTypeLevel3 = payrollProduct.SysPayrollTypeLevel3;
                e.SysPayrollTypeLevel4 = payrollProduct.SysPayrollTypeLevel4;
            }
        }

        public static void RemoveAccountInternal(this TimePayrollScheduleTransaction e, int accountDimId)
        {
            var accountInternal = e.AccountInternal.FirstOrDefault(i => i.Account != null && i.Account.AccountDimId == accountDimId);

            if (accountInternal != null)
                e.AccountInternal.Remove(accountInternal);
        }

        public static string GetAccountingString(this TimePayrollScheduleTransaction e, List<AccountDim> accountDims)
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

        public static string GetAccountingIdString(this TimePayrollScheduleTransaction e)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(e.AccountStdId.ToString());

            if (!e.AccountInternal.IsNullOrEmpty())
            {
                foreach (var ai in e.AccountInternal)
                {
                    sb.Append($"|{ai.AccountId}");
                }
            }      

            return sb.ToString();
        }

        public static bool HasAccountStd(this TimePayrollScheduleTransaction e)
        {
            return e.AccountStdId > 0 || e.AccountStd != null;
        }

        public static bool HasAccountInternals(this TimePayrollScheduleTransaction e)
        {
            return e.AccountInternal != null && e.AccountInternal.Count > 0;
        }

        public static bool HasAccountInternal(this TimePayrollScheduleTransaction e, int accountDimId)
        {
            return e.GetAccountInternal(accountDimId) != null;
        }

        public static bool IsValidForRetro(this TimePayrollScheduleTransaction e)
        {
            return (
                e.IsGrossSalary() ||
                e.IsAddition() ||
                e.IsDeductionHouseKeeping() ||
                e.IsDeductionOther() ||
                e.IsCompensation() ||
                (e.IsBenefit() && !e.IsBenefitInvert() && !e.IsBenefitCompanyCar()) ||
                e.IsCostDeduction() ||
                e.IsOccupationalPension()
                );
        }

        public static bool IsRetroTransaction(this TimePayrollScheduleTransaction e)
        {
            return e.RetroactivePayrollOutcomeId.HasValue;
        }

        public static bool IsExcludedInRecalculateAccounting(this TimePayrollScheduleTransaction e)
        {
            if (e.IsVacationAdditionOrSalaryPrepaymentInvert())
                return true;
            if (e.IsVacationAdditionOrSalaryVariablePrepaymentInvert())
                return true;
            if (e.IsSupplementCharge())
                return true;
            if (e.IsEmploymentTax())
                return true;
            if (e.IsTaxAndNotOptional())
                return true;
            if (e.IsNetSalary())
                return true;

            return false;
        }

        #endregion
    }
}
