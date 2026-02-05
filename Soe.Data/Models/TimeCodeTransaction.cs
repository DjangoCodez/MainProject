using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class TimeCodeTransaction : ICreatedModified, IState, ITask
    {
        public Guid? Guid { get; private set; }
        public Guid? GuidTimeBlock { get; private set; }
        public string TypeName { get; private set; }
        public string TimeRuleName { get; private set; }
        public int? TimeRuleSort { get; private set; }

        public bool IsScheduleTransaction { get; private set; }
        public bool IsSickDuringIwhTransaction { get; private set; }
        public bool IsAbsenceDuringStandbyTransaction { get; private set; }
        public bool IsSickDuringIwhOrStandbyTransaction => this.IsSickDuringIwhTransaction || this.IsAbsenceDuringStandbyTransaction;
        public bool IsGeneratedFromUseTimeCodeMaxAsStandardMinutes { get; private set; }
        public bool IsTurnedByTimeCodeRanking { get; private set; }

        public void Activate(Guid guid)
        {
            this.State = (int)SoeEntityState.Active;
            SetIdentifier(guid);
        }
        public void SetIdentifier(Guid? guid) => this.Guid = guid;
        public void SetTimeBlockIdentifier(Guid? guid) => this.GuidTimeBlock = guid;
        public void SetTimeAndQuantity(DateTime start, DateTime stop, decimal? quantity = null)
        {
            this.Start = start;
            this.Stop = stop;
            SetQuantity(quantity);
        }
        public void SetQuantity(decimal? quantity = null) => this.Quantity = quantity ?? (int)this.Stop.Subtract(this.Start).TotalMinutes;
        public void SetTypeName(string typeName) => this.TypeName = typeName;
        public void SetTimeBlock(TimeBlock timeBlock, int? timeBlockDateId = null)
        {
            if (timeBlock != null)
            {
                if (timeBlock.TimeBlockId > 0)
                    this.TimeBlockId = timeBlock.TimeBlockId;
                else
                    this.TimeBlock = timeBlock;

                this.SetTimeBlockIdentifier(timeBlock.GuidId);
            }

            if (timeBlockDateId.HasValue)
                this.TimeBlockDateId = timeBlockDateId.Value;
        }
        public void SetTimeRule(TimeRule timeRule)
        {
            if (timeRule != null)
            {
                if (timeRule.TimeRuleId > 0)
                    this.TimeRuleId = timeRule.TimeRuleId;
                else
                    this.TimeRule = timeRule;
                this.TimeRuleName = timeRule.Name;
                this.TimeRuleSort = timeRule.Sort;
                this.SetIsGeneratedFromUseTimeCodeMaxAsStandardMinutes(timeRule.UseStandardMinutes);
            }
        }
        public void SetIsScheduleTransaction(bool value) => this.IsScheduleTransaction = value;
        public void SetIsSickDuringIwhTransaction(bool value) => this.IsSickDuringIwhTransaction = value;
        public void SetIsGeneratedFromUseTimeCodeMaxAsStandardMinutes(bool value) => this.IsGeneratedFromUseTimeCodeMaxAsStandardMinutes = value;
        public void SetIsAbsenceDuringStandbyTransaction(bool value) => this.IsAbsenceDuringStandbyTransaction = value;
        public void SetTurnedByTimeCodeRanking() => this.IsTurnedByTimeCodeRanking = true;
        public void SetTimeCodeRankingId(int timeCodeRankingId) => this.TimeCodeRankingId = timeCodeRankingId;
    }

    public static partial class EntityExtensions
    {
        #region TimeCodeTransaction

        public static TimeCodeTransactionDTO ToDTO(this TimeCodeTransaction e)
        {
            if (e == null)
                return null;
            
            TimeCodeTransactionDTO dto = new TimeCodeTransactionDTO
            {
                TimeCodeTransactionId = e.TimeCodeTransactionId,
                TimeBlockId = e.TimeBlockId,
                TimeRuleId = e.TimeRuleId,
                TimeCodeId = e.TimeCodeId,
                CustomerInvoiceRowId = e.CustomerInvoiceRowId,
                ProjectId = e.ProjectId,
                ProjectInvoiceDayId = e.ProjectInvoiceDayId,
                SupplierInvoiceId = e.SupplierInvoiceId,
                TimeBlockDateId = e.TimeBlockDateId,
                Type = (SoeTimeCodeType)e.Type,
                TypeName = e.TypeName,
                Amount = e.Amount ?? 0,
                AmountCurrency = e.AmountCurrency ?? 0,
                AmountEntCurrency = e.AmountEntCurrency ?? 0,
                AmountLedgerCurrency = e.AmountLedgerCurrency ?? 0,
                Vat = e.Vat ?? 0,
                VatCurrency = e.VatCurrency ?? 0,
                VatEntCurrency = e.VatEntCurrency ?? 0,
                VatLedgerCurrency = e.VatLedgerCurrency ?? 0,
                Quantity = e.Quantity,
                InvoiceQuantity = e.InvoiceQuantity ?? 0,
                Start = e.Start,
                Stop = e.Stop,
                Comment = e.Comment,
                State = (SoeEntityState)e.State,
                IsProvision = e.IsProvision,
                IsEarnedHoliday = e.IsEarnedHoliday,
            };

            if (e.TimeCode != null)
            {
                dto.TimeCodeName = e.TimeCode.Name;
                dto.TimeCodeTypeName = "";
                dto.QuantityText = e.TimeCode.IsRegistrationTypeTime ? CalendarUtility.GetHoursAndMinutesString((int)e.Quantity, false) : e.Quantity.ToString();
                dto.IsRegistrationTypeQuantity = e.TimeCode.IsRegistrationTypeQuantity;
                dto.IsRegistrationTypeTime = e.TimeCode.IsRegistrationTypeTime;
            }

            return dto;
        }

        public static IEnumerable<TimeCodeTransactionDTO> ToDTOs(this IEnumerable<TimeCodeTransaction> l)
        {
            var dtos = new List<TimeCodeTransactionDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static TimeCodeTransaction GetDiscardedState(this IEnumerable<TimeCodeTransaction> l, TimeBlock timeBlock, TimeCodeTransactionType? type = null, int? timeCodeId = null, int? timeRuleId = null, bool useTimeBlockTimes = false)
        {
            if (l.IsNullOrEmpty() || timeBlock == null)
                return null;

            if (useTimeBlockTimes)
                l = l.Where(e => e.TimeBlockDateId == timeBlock.TimeBlockDateId && e.Start == timeBlock.StartTime && e.Stop == timeBlock.StopTime);
            else
                l = l.Where(e => e.TimeBlockDateId == timeBlock.TimeBlockDateId && e.TimeBlockId == timeBlock.TimeBlockId);

            return l.Filter(type, timeCodeId, timeRuleId).OrderBy(t => t.State).FirstOrDefault();
        }

        public static IEnumerable<TimeCodeTransaction> Filter(this IEnumerable<TimeCodeTransaction> l, TimeCodeTransactionType? type, int? timeCodeId = null, int? timeRuleId = null)
        {
            if (l != null)
            {
                if (type.HasValue)
                    l = l.Where(t => t.Type == (int)type.Value);
                if (timeCodeId.HasValue)
                    l = l.Where(t => t.TimeCodeId == timeCodeId.Value);
                if (timeRuleId.HasValue)
                    l = l.Where(t => !t.TimeRuleId.HasValue || t.TimeRuleId.Value == timeRuleId.Value);
            }
            return l;
        }

        public static List<TimeCodeTransaction> FilterTransactionsToRoundWholeDay(this List<TimeCodeTransaction> l)
        {
            return l?.Where(i => i.UseRoundingWholeDay()).OrderBy(i => i.Start).ToList() ?? new List<TimeCodeTransaction>();
        }

        public static List<AttestEmployeeDayTimeCodeTransactionDTO> ToAttestEmployeeTimeCodeTransactionDTOs(this List<TimeCodeTransaction> l, List<TimeRule> timeRules)
        {
            if (l == null)
                return null;

            var dtos = new List<AttestEmployeeDayTimeCodeTransactionDTO>();

            foreach (TimeCodeTransaction e in l)
            {
                TimeRule timeRule = e.TimeRuleId.HasValue ? timeRules?.FirstOrDefault(i => i.TimeRuleId == e.TimeRuleId.Value) : null;

                var dto = new AttestEmployeeDayTimeCodeTransactionDTO()
                {
                    TimeCodeTransactionId = e.TimeCodeTransactionId,
                    TimeCodeId = e.TimeCodeId,
                    TimeRuleId = e.TimeRuleId,
                    TimeBlockId = e.TimeBlockId,
                    ProjectTimeBlockId = e.ProjectTimeBlockId,

                    StartTime = e.Start,
                    StopTime = e.Stop,
                    Quantity = e.Quantity,
                    TimeCodeName = e.TimeCode?.Name ?? string.Empty,
                    TimeCodeType = e.TimeCode != null ? (SoeTimeCodeType)e.TimeCode.Type : SoeTimeCodeType.None,
                    TimeCodeRegistrationType = e.TimeCode != null ? (TermGroup_TimeCodeRegistrationType)e.TimeCode.Type : TermGroup_TimeCodeRegistrationType.Unknown,
                    TimeRuleName = timeRule?.Name ?? string.Empty,
                    TimeRuleSort = timeRule?.Sort,
                };
                dtos.Add(dto);
            }

            return dtos;
        }

        public static AttestEmployeeDayTimeCodeTransactionDTO ToAttestEmployeeTimeCodeTransactionDTO(this TimeTransactionItem e, TimeCode timeCode)
        {
            var dto = new AttestEmployeeDayTimeCodeTransactionDTO()
            {
                TimeCodeTransactionId = e.TimeTransactionId,
                TimeCodeId = e.TimeCodeId,
                TimeRuleId = e.TimeRuleId,
                TimeBlockId = e.TimeBlockId,
                ProjectTimeBlockId = null, //Not available in TimeTransactionItem

                StartTime = e.TimeCodeStart ?? CalendarUtility.DATETIME_DEFAULT,
                StopTime = e.TimeCodeStop ?? CalendarUtility.DATETIME_DEFAULT,
                Quantity = e.Quantity,
                TimeCodeName = timeCode?.Name ?? string.Empty,
                TimeCodeType = e.TimeCodeType,
                TimeCodeRegistrationType = e.TimeCodeRegistrationType,
                TimeRuleName = e.TimeRuleName,
                TimeRuleSort = e.TimeRuleSort,
            };

            //Guids
            if (e.GuidInternalPK.HasValue)
                dto.GuidId = e.GuidInternalPK.Value.ToString();
            if (e.GuidTimeBlockFK.HasValue)
                dto.GuidIdTimeBlock = e.GuidTimeBlockFK.Value.ToString();

            return dto;
        }

        public static AttestEmployeeDayTimeCodeTransactionDTO GetTimeCodeTransaction(this TimeTransactionItem e, List<AttestEmployeeDayTimeCodeTransactionDTO> timeCodeTransactions)
        {
            if (!e.GuidInternalFK.HasValue)
                return null;

            return timeCodeTransactions.FirstOrDefault(i => i.GuidId == e.GuidInternalFK.Value.ToString());
        }

        public static TimeTransactionItem CreateTransactionItem(this TimeCodeTransaction e, SoeTimeTransactionType type, int transactionId, int productId, int timeBlockDateId, DateTime date, decimal factor, Employee employee, TimeBlock timeBlock, AttestStateDTO attestState, EmployeeChild employeeChild = null, PayrollProduct payrollProduct = null)
        {
            if (e == null)
                return null;

            bool isPayroll = type == SoeTimeTransactionType.TimePayroll;
            bool isInvoice = type == SoeTimeTransactionType.TimeInvoice;

            return new TimeTransactionItem
            {
                //Keys
                GuidInternalFK = e.Guid,
                GuidTimeBlockFK = e.GuidTimeBlock,

                //Transaction
                TimeTransactionId = transactionId,
                TransactionType = isPayroll ? SoeTimeTransactionType.TimePayroll : SoeTimeTransactionType.TimeInvoice,
                Quantity = e.Quantity * factor,
                InvoiceQuantity = 0,
                Comment = timeBlock?.Comment ?? string.Empty,
                TransactionSysPayrollTypeLevel1 = payrollProduct?.SysPayrollTypeLevel1,
                TransactionSysPayrollTypeLevel2 = payrollProduct?.SysPayrollTypeLevel2,
                TransactionSysPayrollTypeLevel3 = payrollProduct?.SysPayrollTypeLevel3,
                TransactionSysPayrollTypeLevel4 = payrollProduct?.SysPayrollTypeLevel4,
                ScheduleTransactionType = isPayroll && e.IsScheduleTransaction ? SoeTimePayrollScheduleTransactionType.Absence : SoeTimePayrollScheduleTransactionType.None,
                IsScheduleTransaction = isPayroll && e.IsScheduleTransaction,
                ManuallyAdded = false,
                IsAdded = false,
                IsFixed = false,
                IsReversed = false,
                IsVacationReplacement = false,
                ReversedDate = null,

                //Employee
                EmployeeId = employee?.EmployeeId ?? 0,
                EmployeeName = employee?.Name ?? string.Empty,
                EmployeeChildId = employeeChild?.EmployeeChildId,
                EmployeeChildName = employeeChild?.Name,

                //Product
                ProductId = productId,
                ProductNr = payrollProduct?.Number ?? string.Empty,
                ProductName = payrollProduct?.Name ?? string.Empty,
                ProductVatType = isInvoice ? TermGroup_InvoiceProductVatType.Service : TermGroup_InvoiceProductVatType.None,
                PayrollProductSysPayrollTypeLevel1 = payrollProduct?.SysPayrollTypeLevel1,
                PayrollProductSysPayrollTypeLevel2 = payrollProduct?.SysPayrollTypeLevel2,
                PayrollProductSysPayrollTypeLevel3 = payrollProduct?.SysPayrollTypeLevel3,
                PayrollProductSysPayrollTypeLevel4 = payrollProduct?.SysPayrollTypeLevel4,

                //TimeCode
                TimeCodeId = e.TimeCode?.TimeCodeId ?? 0,
                Code = e.TimeCode?.Name ?? string.Empty,
                CodeName = e.TimeCode?.Code ?? string.Empty,
                TimeCodeStart = isPayroll ? e.Start : (DateTime?)null,
                TimeCodeStop = isPayroll ? e.Stop : (DateTime?)null,
                TimeCodeType = SoeTimeCodeType.None,
                TimeCodeRegistrationType = TermGroup_TimeCodeRegistrationType.Unknown,

                //TimeBlock
                TimeBlockId = e.TimeBlockId,

                //TimeBlockDate
                TimeBlockDateId = timeBlockDateId,
                Date = date,

                //TimeRule
                TimeRuleId = 0,
                TimeRuleName = String.Empty,
                TimeRuleSort = 0,

                //AttestState
                AttestStateId = isPayroll && e.IsScheduleTransaction ? 0 : attestState.AttestStateId,
                AttestStateName = isPayroll && e.IsScheduleTransaction ? string.Empty : attestState.Name,
                AttestStateColor = isPayroll && e.IsScheduleTransaction ? string.Empty : attestState.Color,
                AttestStateSort = isPayroll && e.IsScheduleTransaction ? 0 : attestState.Sort,
                AttestStateInitial = (!isPayroll && !e.IsScheduleTransaction) && attestState.Initial,
            };
        }

        public static TimeCodeTransaction Get(this List<TimeCodeTransaction> l, string guid)
        {
            if (l == null || String.IsNullOrEmpty(guid))
                return null;

            return l.FirstOrDefault(i => i.Guid.HasValue && i.Guid.Value.ToString() == guid);
        }

        public static TimeCodeTransaction GetPrevious(this List<TimeCodeTransaction> l, TimeCodeTransaction current)
        {
            return l?.Where(t => t.Start < current.Start).OrderBy(t => t.Start).Last();
        }

        public static TimeBlock GetTimeBlock(this TimeTransactionItem e, List<TimeBlock> timeBlocks)
        {
            if (!e.GuidTimeBlockFK.HasValue)
                return null;

            return timeBlocks.FirstOrDefault(i => i.GuidId.HasValue && i.GuidId.Value == e.GuidTimeBlockFK.Value);
        }

        public static AccountDTO GetAccountingFromString(this TimeCodeTransaction e, List<AccountDim> accountDimsWithAccounts)
        {
            AccountDTO accountDTO = new AccountDTO();

            string accountingString = e.Accounting;
            if (string.IsNullOrEmpty(accountingString))
                return accountDTO;

            accountDTO.AccountInternals = new List<AccountInternalDTO>();

            string[] accountNumbers = accountingString.Split(';');
            if (accountNumbers.Length > 0)
            {
                int dimCounter = 0;

                Account account = accountDimsWithAccounts[dimCounter].Account.FirstOrDefault(a => a.AccountNr == accountNumbers[dimCounter]);
                if (account != null)
                {
                    accountDTO.AccountId = account.AccountId;
                    accountDTO.AccountNr = account.AccountNr;
                    accountDTO.Name = account.Name;
                }

                foreach (string accountNumber in accountNumbers)
                {
                    if (dimCounter == 0)
                    {
                        dimCounter++;
                        continue;
                    }

                    if (!string.IsNullOrEmpty(accountNumber) && accountDimsWithAccounts.Count > dimCounter)
                    {
                        AccountInternalDTO accountInternalDTO = new AccountInternalDTO();

                        if (accountNumber == "-")
                        {
                            // No account
                            accountInternalDTO.AccountId = -1;
                            accountInternalDTO.AccountNr = "-";
                            accountInternalDTO.AccountDimId = accountDimsWithAccounts[dimCounter].AccountDimId;
                            accountDTO.AccountInternals.Add(accountInternalDTO);
                        }
                        else
                        {
                            account = accountDimsWithAccounts[dimCounter].Account.FirstOrDefault(a => a.AccountNr == accountNumbers[dimCounter]);
                            if (account != null)
                            {
                                accountInternalDTO.AccountId = account.AccountId;
                                accountInternalDTO.AccountNr = account.AccountNr;
                                accountInternalDTO.Name = account.Name;
                                accountInternalDTO.AccountDimId = account.AccountDimId;
                                accountDTO.AccountInternals.Add(accountInternalDTO);
                            }
                        }
                    }
                    dimCounter++;
                }
            }

            return accountDTO;
        }

        public static List<int> GetTimeRuleIds(this List<TimeCodeTransaction> l)
        {
            return l?.Where(t => t.TimeRuleId.HasValue).Select(t => t.TimeRuleId.Value).Distinct().ToList() ?? new List<int>();
        }

        public static bool UseRoundingWholeDay(this TimeCodeTransaction e)
        {
            return e?.TimeCode?.UseRoundingWholeDay() ?? false;
        }

        public static int SumQuantity(this IEnumerable<TimeCodeTransaction> l, int? timeCodeId = null)
        {
            if (l.IsNullOrEmpty())
                return 0;

            if (timeCodeId.HasValue)
                return (int)l.Where(i => i.TimeCodeId == timeCodeId.Value).Sum(e => e.Quantity);
            else
                return (int)l.Sum(e => e.Quantity);
        }

        public static bool DoIncreaseTimeCodeTransactionsAfterTimeCode(this List<TimeCodeTransaction> l, TimeCode timeCode)
        {
            if (l.IsNullOrEmpty() || timeCode == null || timeCode.AdjustQuantityByBreakTime != (int)TermGroup_AdjustQuantityByBreakTime.Add)
                return false;

            //Valid if has no TimeScheduleType
            if (!timeCode.AdjustQuantityTimeScheduleTypeId.HasValue)
                return true;
            //Valid if contains TimeScheduleType
            if (l.Any(i => i.TimeBlock?.CalculatedTimeScheduleTypeId == timeCode.AdjustQuantityTimeScheduleTypeId.Value))
                return true;
            return false;
        }

        #endregion
    }
}
