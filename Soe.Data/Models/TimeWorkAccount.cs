using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class TimeWorkAccount : ICreatedModified, IState
    {
        public bool DefaulWithdrawalMethodIsDirectPayment()
        {
            return this.DefaultWithdrawalMethod == (int)TermGroup_TimeWorkAccountWithdrawalMethod.DirectPayment;
        }
        public bool DefaultWithdrawalMethodIsPensionDeposit()
        {
            return this.DefaultWithdrawalMethod == (int)TermGroup_TimeWorkAccountWithdrawalMethod.PensionDeposit;
        }
        public bool DefaultWithdrawalMethodIsPaidLeave()
        {
            return this.DefaultWithdrawalMethod == (int)TermGroup_TimeWorkAccountWithdrawalMethod.PaidLeave;
        }
    }

    public partial class TimeWorkAccountWorkTimeWeek : ICreatedModified, IState
    {

    }

    public partial class TimeWorkAccountYear : ICreatedModified, IState
    {
        
    }

    public partial class TimeWorkAccountYearEmployee : ICreatedModified, IState
    {
        private EmployeeTimeWorkAccount employeeTimeWorkAccount = null;
        private TimeWorkAccountYear timeWorkAccountYear = null;

        public static TimeWorkAccountYearEmployee Create(
            EmployeeTimeWorkAccount employeeTimeWorkAccount, 
            TimeWorkAccountYear timeWorkAccountYear, 
            DateTime earningStart, 
            DateTime earningStop
            )
        {
            if (employeeTimeWorkAccount == null || timeWorkAccountYear == null)
                return null;

            return new TimeWorkAccountYearEmployee  
            {
                EmployeeId = employeeTimeWorkAccount.EmployeeId,
                TimeWorkAccountId = employeeTimeWorkAccount.TimeWorkAccountId,
                TimeWorkAccountYearId = timeWorkAccountYear.TimeWorkAccountYearId,
                EarningStart = earningStart,
                EarningStop = earningStop,
                Status = (int)TermGroup_TimeWorkAccountYearEmployeeStatus.Created,

                employeeTimeWorkAccount = employeeTimeWorkAccount,
                timeWorkAccountYear = timeWorkAccountYear,
            };
        }
        public bool IsWithdrawalMethodDirectPayment() => this.SelectedWithdrawalMethod == (int)TermGroup_TimeWorkAccountWithdrawalMethod.DirectPayment;
        public bool IsWithdrawalMethodPensionDeposit() => this.SelectedWithdrawalMethod == (int)TermGroup_TimeWorkAccountWithdrawalMethod.PensionDeposit;
        public bool IsWithdrawalMethodPaidLeave() => this.SelectedWithdrawalMethod == (int)TermGroup_TimeWorkAccountWithdrawalMethod.PaidLeave;
        public bool IsWithdrawalMethodNotChoosed() => this.SelectedWithdrawalMethod == (int)TermGroup_TimeWorkAccountWithdrawalMethod.NotChoosed;
        public void SetCalculated(TimeWorkAccountYearEmployeeCalculation calculationBasis)
        {
            this.Status = (int)TermGroup_TimeWorkAccountYearEmployeeStatus.Calculated;
            this.CalculationBasis = calculationBasis.ToPersistanceJson();
            this.SetPaidLeaveMinutes(Convert.ToInt32(calculationBasis.GetPaidLeaveMinutes()));
            this.SetBasisAmount(calculationBasis.GetAmount());
        }
        public void UserSelectedWithdrawalMethod(TermGroup_TimeWorkAccountWithdrawalMethod withdrawalMethod)
        {
            SetSelectedWithdrawalMethod(withdrawalMethod);
            this.SelectedDate = DateTime.Now;
        }
        public void SetSelectedWithdrawalMethod(TermGroup_TimeWorkAccountWithdrawalMethod withdrawalMethod, TermGroup_TimeWorkAccountYearEmployeeStatus? status = TermGroup_TimeWorkAccountYearEmployeeStatus.Choosed)
        {
            this.SelectedWithdrawalMethod = (int)withdrawalMethod;
            if (status.HasValue)
                this.Status = (int)status.Value;
        }
        public void SetNotChoosed()
        {
            this.SelectedWithdrawalMethod = (int)TermGroup_TimeWorkAccountWithdrawalMethod.NotChoosed;
            this.Status = (int)TermGroup_TimeWorkAccountYearEmployeeStatus.Calculated;
            this.SelectedDate = null;
        }
        private void SetPaidLeaveMinutes(int paidLeaveMinutes)
        {
            this.CalculatedPaidLeaveMinutes = paidLeaveMinutes;
        }
        private void SetBasisAmount(decimal amount)
        {
            this.CalculatedWorkingTimePromoted = amount;
            CalculateOptionAmounts(amount);
        }
        public void CalculateOptionAmounts(decimal amount, TimeWorkAccountYear timeWorkAccountYear = null)
        {
            if (timeWorkAccountYear != null) 
                this.timeWorkAccountYear = timeWorkAccountYear;

            this.CalculatedPaidLeaveAmount = NumberUtility.MultiplyPercent(amount, this.timeWorkAccountYear?.PaidLeavePercent ?? 0);
            this.CalculatedPensionDepositAmount = NumberUtility.MultiplyPercent(amount, this.timeWorkAccountYear?.PensionDepositPercent ?? 0 );
            this.CalculatedDirectPaymentAmount = NumberUtility.MultiplyPercent(amount, this.timeWorkAccountYear?.DirectPaymentPercent ?? 0);
        }
        public decimal GetUnitPrice()
        {
            return this.CalculatedPaidLeaveAmount != 0 ? Decimal.Multiply(Decimal.Divide(this.CalculatedPaidLeaveAmount, this.CalculatedPaidLeaveMinutes), 60) : 0;
        }
        public void UpdateStatus(TermGroup_TimeWorkAccountYearEmployeeStatus status, string modifiedBy, DateTime? modified = null)
        {
            if (this.Status != (int)status)
            {
                this.Status = (int)status;
                this.SetModified(modified ?? DateTime.Now, modifiedBy);
            }
        }
        public override string ToString()
        {
            return
                $"TimeWorkAccountId:{this.employeeTimeWorkAccount?.TimeWorkAccountId}" +
                $",TimeWorkAccountYearId:{this.timeWorkAccountYear?.TimeWorkAccountYearId}" +
                $",EarningStart:{this.timeWorkAccountYear?.EarningStart.ToShortDateString()}" +
                $",EarningStop:{this.timeWorkAccountYear?.EarningStop.ToShortDateString()}" +
                $",WithdrawalStart:{this.timeWorkAccountYear?.WithdrawalStart.ToShortDateString()}" +
                $",EmployeeLastDecidedDate:{this.timeWorkAccountYear?.EmployeeLastDecidedDate.ToShortDateString()}" +
                $",PaidAbsenceStopDate:{this.timeWorkAccountYear?.PaidAbsenceStopDate.ToShortDateString()}" +
                $",DirectPaymentLastDate:{this.timeWorkAccountYear?.DirectPaymentLastDate.ToShortDateString()}" +
                $",PensionDepositPercent:{this.timeWorkAccountYear?.PensionDepositPercent}" +
                $",PaidLeavePercent:{this.timeWorkAccountYear?.PaidLeavePercent}" +
                $",DirectPaymentPercent:{this.timeWorkAccountYear?.DirectPaymentPercent}" +
                $",WithdrawalStop:{this.timeWorkAccountYear?.WithdrawalStop.ToShortDateString()}" +
                $",WithdrawalStop:{this.timeWorkAccountYear?.WithdrawalStop.ToShortDateString()}" +
                $",EmployeeId:{this.employeeTimeWorkAccount?.EmployeeId}" +
                $",EmployeeDateFrom:{this.employeeTimeWorkAccount?.DateFrom.ToShortDateString()}" +
                $",EmployeeDateTo:{this.employeeTimeWorkAccount?.DateTo.ToShortDateString()}";
        }
    }

    public class TimeWorkAccountYearEmployeeCalculation
    {
        public int EmployeeId { get; private set; }
        public TimeWorkAccount TimeWorkAccount { get; private set; }
        public TimeWorkAccountYear TimeWorkAccountYear { get; private set; }
        public bool HasTimeAccumulatorId => this.TimeAccumulatorId.HasValue;
        public int? TimeAccumulatorId => this.TimeWorkAccountYear?.TimeAccumulatorId;
        private readonly List<TimeWorkAccountYearEmployeeTransaction> payrollTransactions;
        private readonly List<TimeWorkAccountYearEmployeeTransaction> scheduleTransactions;
        private readonly List<TimeWorkAccountYearEmployeeDay> days;

        private TimeWorkAccountYearEmployeeCalculation(
            int employeeId,
            TimeWorkAccount timeWorkAccount,
            TimeWorkAccountYear timeWorkAccountYear,
            List<TimeWorkAccountYearEmployeeTransaction> payrollTransactions, 
            List<TimeWorkAccountYearEmployeeTransaction> scheduleTransactions, 
            List<TimeWorkAccountYearEmployeeDay> days)
        {
            this.EmployeeId = employeeId;
            this.TimeWorkAccount = timeWorkAccount;
            this.TimeWorkAccountYear = timeWorkAccountYear;
            this.payrollTransactions = payrollTransactions ?? new List<TimeWorkAccountYearEmployeeTransaction>();
            this.scheduleTransactions = scheduleTransactions ?? new List<TimeWorkAccountYearEmployeeTransaction>();
            this.days = days ?? new List<TimeWorkAccountYearEmployeeDay>();
        }
        public static TimeWorkAccountYearEmployeeCalculation Empty => new TimeWorkAccountYearEmployeeCalculation(0, null, null, null, null, null);
        public static TimeWorkAccountYearEmployeeCalculation Create(
            int employeeId,
            TimeWorkAccount timeWorkAccount,
            TimeWorkAccountYear timeWorkAccountYear,
            List<TimeWorkAccountYearEmployeeTransaction> payrollTransactions,
            List<TimeWorkAccountYearEmployeeTransaction> scheduleTransactions,
            List<TimeWorkAccountYearEmployeeDay> days)
        {
            return new TimeWorkAccountYearEmployeeCalculation(employeeId, timeWorkAccount, timeWorkAccountYear, payrollTransactions, scheduleTransactions, days);
        }

        public ReadOnlyCollection<TimeWorkAccountYearEmployeeDay> GetDays() => this.days.OrderBy(t => t.Date).ToList().AsReadOnly();
        public ReadOnlyCollection<TimeWorkAccountYearEmployeeTransaction> GetPayrollTransactions() => this.payrollTransactions.AsReadOnly();
        public ReadOnlyCollection<TimeWorkAccountYearEmployeeTransaction> GetScheduleTransactions() => this.scheduleTransactions.AsReadOnly();
        private List<TimeWorkAccountYearEmployeeTransaction> GetAllTransactions() => this.payrollTransactions.Concat(this.scheduleTransactions).ToList();
        public List<int> GetPayrollTransactionIds(DateTime? date = null) => this.payrollTransactions.Filter(date).Select(t => t.Id).ToList();
        public List<int> GetScheduleTransactionIds(DateTime? date = null) => this.scheduleTransactions.Filter(date).Select(t => t.Id).ToList();
        public decimal GetAmount(DateTime? date = null) => this.GetAllTransactions().Where(t => t.Amount.HasValue).Filter(date).Sum(t => t.Amount.Value);

        private TimeWorkAccountYearEmployeeDay GetDay(DateTime date) => this.days.Find(d => d.Date == date);
        public int GetWorkTimeWeek(DateTime date) => GetDay(date)?.WorkTimeWeek ?? 0;
        public decimal GetEmploymentPercent(DateTime date) => GetDay(date)?.EmploymentPercent ?? 0;
        public decimal GetPaidLeaveMinutes()
        {
            decimal totalPaidLeaveMinutes = this.days.Where(d => d.Date >= this.TimeWorkAccountYear.EarningStart.Date && d.Date <= this.TimeWorkAccountYear.EarningStop.Date).Sum(day => day.GetPaidLeaveMinutes());
            if (totalPaidLeaveMinutes > 0)
                totalPaidLeaveMinutes = CalendarUtility.RoundMinutesToNextHalfHour(totalPaidLeaveMinutes);
            return totalPaidLeaveMinutes;
        }
        public decimal GetPaidLeaveMinutes(DateTime date)
        {
            return this.days.Find(d => d.Date == date)?.GetPaidLeaveMinutes() ?? 0;
        }
    }

    public class TimeWorkAccountYearEmployeeDay
    {
        public DateTime Date { get; private set; }
        public TimePeriodDTO TimePeriod { get; set; }
        public bool HasEmployment { get; private set; }
        public decimal EmploymentPercent { get; private set; }
        public decimal UnpaidAbsenceRatio { get; private set; }
        public int YearEarningDays { get; private set; }
        public int RuleWorkTimeWeek { get; private set; }
        public int WorkTimeWeek { get; private set; }
        public int PaidLeaveTimeMinutes { get; private set; }

        private TimeWorkAccountYearEmployeeDay() { }
        public static TimeWorkAccountYearEmployeeDay Create(DateTime date, decimal employmentPercent, int ruleWorkTimeWeek, int workTimeWeek, int paidLeaveTimeMinutes, int yearEarningDays, bool hasEmployment, TimePeriodDTO timePeriod, decimal unpaidAbsenceRatio)
        {
            return new TimeWorkAccountYearEmployeeDay
            {
                Date = date,
                TimePeriod = timePeriod,
                HasEmployment = hasEmployment,
                EmploymentPercent = employmentPercent,
                UnpaidAbsenceRatio = unpaidAbsenceRatio,
                YearEarningDays = yearEarningDays,
                RuleWorkTimeWeek = ruleWorkTimeWeek,
                WorkTimeWeek = workTimeWeek,
                PaidLeaveTimeMinutes = paidLeaveTimeMinutes,
            };
        }

        /// <summary>
        /// En specifik dag =  sysselsättningsgrad för aktuell dag*  ( Intervallets max antal timmar för aktuell dag/ (Intjänande år löneunderlag stop - Intjänande år löneunderlag start) )
        /// </summary>
        /// <returns></returns>
        public decimal GetPaidLeaveMinutes()
        {
            if (this.EmploymentPercent == 0 || this.PaidLeaveTimeMinutes == 0 || this.YearEarningDays == 0 || this.UnpaidAbsenceRatio == 1)
                return 0;

            decimal paidLeaveMinutes =
                Decimal.Multiply(
                    Decimal.Divide(this.EmploymentPercent, 100),
                    Decimal.Divide(this.PaidLeaveTimeMinutes, this.YearEarningDays)
                );

            if (paidLeaveMinutes > 0 && this.UnpaidAbsenceRatio > 0)
                paidLeaveMinutes -= Decimal.Multiply(paidLeaveMinutes, this.UnpaidAbsenceRatio);

            return paidLeaveMinutes;
        }
    }

    public class TimeWorkAccountYearEmployeeTransaction
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public DateTime Date { get; set; }
        public decimal? Amount { get; set; }
        public decimal Quantity { get; set; }

        private TimeWorkAccountYearEmployeeTransaction() { }
        public static TimeWorkAccountYearEmployeeTransaction Create(int id, int productId, DateTime date, decimal? amount, decimal quantity)
        {
            return new TimeWorkAccountYearEmployeeTransaction
            {
                Id = id,
                ProductId = productId,
                Date = date,
                Amount = amount,
                Quantity = quantity,
            };
        }
    }

    public static partial class EntityExtensions
    {
        #region TimeWorkAccount

        public static IEnumerable<TimeWorkAccountDTO> ToDTOs(this IEnumerable<TimeWorkAccount> l)
        {
            var dtos = new List<TimeWorkAccountDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static TimeWorkAccountDTO ToDTO(this TimeWorkAccount e)
        {
            if (e == null)
                return null;

            return new TimeWorkAccountDTO()
            {
                TimeWorkAccountId = e.TimeWorkAccountId,
                ActorCompanyId = e.ActorCompanyId,
                Name = e.Name,
                Code = e.Code,
                UsePensionDeposit = e.UsePensionDeposit,
                UsePaidLeave = e.UsePaidLeave,
                UseDirectPayment = e.UseDirectPayment,
                DefaultWithdrawalMethod = (TermGroup_TimeWorkAccountWithdrawalMethod)e.DefaultWithdrawalMethod,
                DefaultPaidLeaveNotUsed = (TermGroup_TimeWorkAccountWithdrawalMethod)e.DefaultPaidLeaveNotUsed,
                TimeWorkAccountYears = e.TimeWorkAccountYear?.Where(a => a.State == (int)SoeEntityState.Active).OrderBy(y => y.EarningStart).ThenBy(y => y.EarningStop).ToDTOs().ToList(),
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };
        }

        public static IEnumerable<TimeWorkAccountGridDTO> ToGridDTOs(this IEnumerable<TimeWorkAccount> l)
        {
            var dtos = new List<TimeWorkAccountGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        public static TimeWorkAccountGridDTO ToGridDTO(this TimeWorkAccount e)
        {
            if (e == null)
                return null;

            return new TimeWorkAccountGridDTO()
            {
                TimeWorkAccountId = e.TimeWorkAccountId,
                Name = e.Name,
                Code = e.Code,
                UsePensionDeposit = e.UsePensionDeposit,
                UsePaidLeave = e.UsePaidLeave,
                UseDirectPayment = e.UseDirectPayment,
                DefaultWithdrawalMethod = (TermGroup_TimeWorkAccountWithdrawalMethod)e.DefaultWithdrawalMethod,
                DefaultPaidLeaveNotUsed = (TermGroup_TimeWorkAccountWithdrawalMethod)e.DefaultPaidLeaveNotUsed,
                State = (SoeEntityState)e.State,
            };
        }

        public static void SetProperties(this TimeWorkAccount e, TimeWorkAccountDTO input)
        {
            if (e == null || input == null)
                return;

            e.Name = input.Name;
            e.Code = input.Code;
            e.UsePensionDeposit = input.UsePensionDeposit;
            e.UsePaidLeave = input.UsePaidLeave;
            e.UseDirectPayment = input.UseDirectPayment;
            e.DefaultWithdrawalMethod = (int)input.DefaultWithdrawalMethod;
            e.DefaultPaidLeaveNotUsed = (int)input.DefaultPaidLeaveNotUsed;
            e.State = (int)input.State;
        }

        #endregion

        #region TimeWorkAccountYear

        public static IEnumerable<TimeWorkAccountYearDTO> ToDTOs(this IEnumerable<TimeWorkAccountYear> l)
        {
            var dtos = new List<TimeWorkAccountYearDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static TimeWorkAccountYearDTO ToDTO(this TimeWorkAccountYear e, bool addYear = false)
        {
            if (e == null)
                return null;

            if (addYear)
            {
                e.EarningStart = e.EarningStart.AddYears(1);
                e.EarningStop = e.EarningStop.AddYears(1);
                e.WithdrawalStart = e.WithdrawalStart.AddYears(1);
                e.WithdrawalStop = e.WithdrawalStop.AddYears(1);
                e.EmployeeLastDecidedDate = e.EmployeeLastDecidedDate.AddYears(1);
                e.PaidAbsenceStopDate = e.PaidAbsenceStopDate.AddYears(1);
                e.DirectPaymentLastDate = e.DirectPaymentLastDate.AddYears(1);
            }

            return new TimeWorkAccountYearDTO()
            {
                TimeWorkAccountYearId = !addYear ? e.TimeWorkAccountYearId : 0,
                TimeWorkAccountId = e.TimeWorkAccountId,
                EarningStart = e.EarningStart,
                EarningStop = e.EarningStop,
                WithdrawalStart = e.WithdrawalStart,
                WithdrawalStop = e.WithdrawalStop,
                PensionDepositPercent = e.PensionDepositPercent,
                PaidLeavePercent = e.PaidLeavePercent,
                DirectPaymentPercent = e.DirectPaymentPercent,
                EmployeeLastDecidedDate = e.EmployeeLastDecidedDate,
                PaidAbsenceStopDate = e.PaidAbsenceStopDate,
                DirectPaymentLastDate = e.DirectPaymentLastDate,
                DirectPaymentPayrollProductId = e.DirectPaymentPayrollProductId,
                PensionDepositPayrollProductId = e.PensionDepositPayrollProductId,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                TimeWorkAccountYearEmployees = e.TimeWorkAccountYearEmployee?.Where(x => x.State == (int)SoeEntityState.Active).ToDTOs().OrderBy(ye => ye.EmployeeNumber).ToList(),
                TimeWorkAccountWorkTimeWeeks = e.TimeWorkAccountWorkTimeWeek?.Where(x => x.State == (int)SoeEntityState.Active).ToDTOs().OrderBy(ww => ww.WorkTimeWeekFrom).ToList(),
                TimeAccumulatorId = e.TimeAccumulatorId,
            };
        }

        public static void SetProperties(this TimeWorkAccountYear e, TimeWorkAccountYearDTO input)
        {
            if (e == null || input == null)
                return;

            e.EarningStart = input.EarningStart;
            e.EarningStop = input.EarningStop;
            e.WithdrawalStart = input.WithdrawalStart;
            e.WithdrawalStop = input.WithdrawalStop;
            e.PensionDepositPercent = input.PensionDepositPercent;
            e.PaidLeavePercent = input.PaidLeavePercent;
            e.DirectPaymentPercent = input.DirectPaymentPercent;
            e.EmployeeLastDecidedDate = input.EmployeeLastDecidedDate;
            e.PaidAbsenceStopDate = input.PaidAbsenceStopDate;
            e.DirectPaymentLastDate = input.DirectPaymentLastDate;
            e.PensionDepositPayrollProductId = input.PensionDepositPayrollProductId > 0 ? input.PensionDepositPayrollProductId : null;
            e.DirectPaymentPayrollProductId = input.DirectPaymentPayrollProductId > 0 ? input.DirectPaymentPayrollProductId : null;
            e.TimeAccumulatorId = input.TimeAccumulatorId > 0 ? input.TimeAccumulatorId : null;
        }

        public static List<TimeWorkAccountWorkTimeWeek> GetWorkTimeWeeks(this TimeWorkAccountYear e)
        {
            return e?.TimeWorkAccountWorkTimeWeek?.Where(w => w.State == (int)SoeEntityState.Active).ToList() ?? new List<TimeWorkAccountWorkTimeWeek>();
        }

        public static TimeWorkAccountWorkTimeWeek GetWorkTimeWeek(this TimeWorkAccountYear e, int minutes)
        {
            List<TimeWorkAccountWorkTimeWeek> workTimeWeeks = e.GetWorkTimeWeeks();
            return workTimeWeeks.FirstOrDefault(w => w.WorkTimeWeekFrom <= minutes && w.WorkTimeWeekTo > minutes) ??
                   workTimeWeeks.Where(w => w.WorkTimeWeekTo <= minutes).OrderByDescending(w => w.WorkTimeWeekTo).FirstOrDefault();
        }

        public static int GetPaidLeaveMinutes(this TimeWorkAccountYear e, int minutes)
        {
            return e.GetWorkTimeWeek(minutes)?.PaidLeaveTime ?? 0;
        }

        public static bool HasWorkTimeWeeks(this TimeWorkAccountYear e)
        {
            return e?.TimeWorkAccountWorkTimeWeek?.Any(w => w.State == (int)SoeEntityState.Active) ?? false;
        }

        public static int GetEarningDays(this TimeWorkAccountYear e)
        {
            return e != null ? (int)e.EarningStop.Subtract(e.EarningStart).TotalDays + 1 : 0;
        }

        #endregion

        #region TimeWorkAccountYearEmployee

        public static IEnumerable<TimeWorkAccountYearEmployeeDTO> ToDTOs(this IEnumerable<TimeWorkAccountYearEmployee> l)
        {
            var dtos = new List<TimeWorkAccountYearEmployeeDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static TimeWorkAccountYearEmployeeDTO ToDTO(this TimeWorkAccountYearEmployee e)
        {
            if (e == null)
                return null;

            return new TimeWorkAccountYearEmployeeDTO()
            {
                TimeWorkAccountYearEmployeeId = e.TimeWorkAccountYearEmployeeId,
                TimeWorkAccountId  = e.TimeWorkAccountId,
                EmployeeId = e.EmployeeId,
                EmployeeName = e.Employee?.Name ?? string.Empty,
                EmployeeNumber = e.Employee?.EmployeeNr ?? string.Empty,
                Status = (TermGroup_TimeWorkAccountYearEmployeeStatus)e.Status,
                EarningStart = e.EarningStart,
                EarningStop = e.EarningStop,
                SelectedWithdrawalMethod = (TermGroup_TimeWorkAccountWithdrawalMethod)e.SelectedWithdrawalMethod,
                SelectedDate   = e.SelectedDate,
                CalculatedPaidLeaveMinutes = e.CalculatedPaidLeaveMinutes,
                CalculatedPaidLeaveAmount = e.CalculatedPaidLeaveAmount,
                CalculatedPensionDepositAmount = e.CalculatedPensionDepositAmount,
                CalculatedDirectPaymentAmount  = e.CalculatedDirectPaymentAmount,
                CalculatedWorkingTimePromoted = e.CalculatedWorkingTimePromoted,
                SpecifiedWorkingTimePromoted = e.SpecifiedWorkingTimePromoted,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                SentDate = e.SentDate,
            };
        }

        public static List<TimeWorkAccountYearEmployee> GetOverlapping(this List<TimeWorkAccountYearEmployee> l, DateTime earningStart, DateTime earningStop)
        {
            return l?.Where(e => e.State == (int)SoeEntityState.Active && CalendarUtility.IsDatesOverlapping(earningStart, earningStop, e.EarningStart, e.EarningStop)).ToList() ?? new List<TimeWorkAccountYearEmployee>();
        }

        public static bool IsValidToRecalculate(this TimeWorkAccountYearEmployee e, DateTime earningStart, DateTime earningStop)
        {
            if (e == null || !CalendarUtility.IsDatesOverlapping(earningStart, earningStop, e.EarningStart, e.EarningStop))
                return true;
            return e.Status <= (int)TermGroup_TimeWorkAccountYearEmployeeStatus.Calculated;
        }

        public static int GetEarningDays(this TimeWorkAccountYearEmployee e)
        {
            if (e == null)
                return 0;
            return (int)e.EarningStop.Subtract(e.EarningStart).TotalDays;
        }

        public static void SetSelectedWithdrawalMethod(this List<TimeWorkAccountYearEmployee> l, TermGroup_TimeWorkAccountWithdrawalMethod withdrawalMethod, TermGroup_TimeWorkAccountYearEmployeeStatus? status = TermGroup_TimeWorkAccountYearEmployeeStatus.Choosed)
        {
            l?.ForEach(e => e.SetSelectedWithdrawalMethod(withdrawalMethod, status));
        }

        #endregion

        #region TimeWorkAccountWorkTimeWeek

        public static IEnumerable<TimeWorkAccountWorkTimeWeekDTO> ToDTOs(this IEnumerable<TimeWorkAccountWorkTimeWeek> l)
        {
            var dtos = new List<TimeWorkAccountWorkTimeWeekDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static TimeWorkAccountWorkTimeWeekDTO ToDTO(this TimeWorkAccountWorkTimeWeek e)
        {
            if (e == null)
                return null;

            return new TimeWorkAccountWorkTimeWeekDTO()
            {
                TimeWorkAccountWorkTimeWeekId = e.TimeWorkAccountWorkTimeWeekId,
                TimeWorkAccountYearId = e.TimeWorkAccountYearId,
                WorkTimeWeekFrom = e.WorkTimeWeekFrom,
                WorkTimeWeekTo = e.WorkTimeWeekTo,
                PaidLeaveTime = e.PaidLeaveTime,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };
        }

        #endregion

        #region TimeWorkAccountYearEmployeeTransaction

        public static List<TimeWorkAccountYearEmployeeTransaction> ToTimeWorkAccountEmployeeTransaction<T>(this IEnumerable<T> l) where T : ITransactionProc
        {
            return l.Select(e => e.ToTimeWorkAccountEmployeeTransaction()).ToList();
        }

        public static TimeWorkAccountYearEmployeeTransaction ToTimeWorkAccountEmployeeTransaction<T>(this T t) where T: ITransactionProc
        {
            return TimeWorkAccountYearEmployeeTransaction.Create(t.Id, t.ProductId, t.Date, t.Amount, t.Quantity);
        }

        public static IEnumerable<TimeWorkAccountYearEmployeeTransaction> Filter(this IEnumerable<TimeWorkAccountYearEmployeeTransaction> l, DateTime? date = null)
        {
            return (date.HasValue ? l?.Where(e => e.Date == date.Value) : l) ?? new List<TimeWorkAccountYearEmployeeTransaction>();
        }

        #endregion

        #region TimeWorkAccountYearEmployeeCalculationBasis

        public static string ToPersistanceJson(this TimeWorkAccountYearEmployeeCalculation basis)
        {
            return JsonConvert.SerializeObject(basis.ToCalculationBasis(), new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                },
                Formatting = Formatting.Indented,
            });
        }

        public static List<TimeWorkAccountYearEmployeeBasisDTO> ToCalculationBasis(this TimeWorkAccountYearEmployeeCalculation basis)
        {
            List<TimeWorkAccountYearEmployeeBasisDTO> calculationBasis = new List<TimeWorkAccountYearEmployeeBasisDTO>();
            foreach (TimeWorkAccountYearEmployeeDay day in basis.GetDays())
            {
                calculationBasis.Add(TimeWorkAccountYearEmployeeBasisDTO.Create(
                    day.Date,
                    day.TimePeriod,
                    day.RuleWorkTimeWeek,
                    day.WorkTimeWeek,
                    day.EmploymentPercent,
                    day.UnpaidAbsenceRatio,
                    day.YearEarningDays,
                    basis.GetAmount(day.Date),
                    basis.GetPaidLeaveMinutes(day.Date),
                    basis.GetPayrollTransactionIds(day.Date),
                    basis.GetScheduleTransactionIds(day.Date)
                    ));
            }
            return calculationBasis;
        }

        public static List<int> GetTimeAccumulatorIds(this IEnumerable<TimeWorkAccountYearEmployeeCalculation> l)
        {
            return l?.Where(b => b.TimeAccumulatorId.HasValue).Select(b => b.TimeAccumulatorId.Value).Distinct().ToList() ?? new List<int>();
        }

        #endregion
    }
}
