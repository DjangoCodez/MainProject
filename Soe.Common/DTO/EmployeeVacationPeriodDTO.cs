namespace SoftOne.Soe.Common.DTO
{
    public class EmployeeVacationPeriodDTO
    {
        public bool CalculateHours { get; set; }
        public decimal CalculateHoursDayFactor { get; set; }
        public int EmployeeId { get; set; }
        public int TimePeriodId { get; set; }
        public decimal RemainingDaysPaid { get; set; }
        public decimal RemainingDaysUnpaid { get; set; }
        public decimal RemainingDaysAdvance { get; set; }
        public decimal RemainingDaysYear1 { get; set; }
        public decimal RemainingDaysYear2 { get; set; }
        public decimal RemainingDaysYear3 { get; set; }
        public decimal RemainingDaysYear4 { get; set; }
        public decimal RemainingDaysYear5 { get; set; }
        public decimal RemainingDaysOverdue { get; set; }

        public decimal EarnedDaysRemainingHoursPaid { get; set; }
        public decimal EarnedDaysRemainingHoursUnpaid { get; set; }
        public decimal EarnedDaysRemainingHoursAdvance { get; set; }
        public decimal EarnedDaysRemainingHoursYear1 { get; set; }
        public decimal EarnedDaysRemainingHoursYear2 { get; set; }
        public decimal EarnedDaysRemainingHoursYear3 { get; set; }
        public decimal EarnedDaysRemainingHoursYear4 { get; set; }
        public decimal EarnedDaysRemainingHoursYear5 { get; set; }
        public decimal EarnedDaysRemainingHoursOverdue { get; set; }

        public decimal PeriodDaysPaid { get; set; }
        public decimal PeriodDaysUnpaid { get; set; }
        public decimal PeriodDaysAdvance { get; set; }
        public decimal PeriodDaysSavedYear1 { get; set; }
        public decimal PeriodDaysSavedYear2 { get; set; }
        public decimal PeriodDaysSavedYear3 { get; set; }
        public decimal PeriodDaysSavedYear4 { get; set; }
        public decimal PeriodDaysSavedYear5 { get; set; }
        public decimal PeriodDaysOverdue { get; set; }
        public decimal PeriodVacationCompensationPaidCount { get; set; }
        public decimal PeriodVacationCompensationSavedCount { get; set; }

        public decimal DaysPaid
        {
            get
            {
                if (CalculateHours)
                    return (EarnedDaysRemainingHoursPaid - ((PeriodDaysPaid - PeriodVacationCompensationPaidCount) * CalculateHoursDayFactor));
                else
                    return (RemainingDaysPaid - PeriodDaysPaid - PeriodVacationCompensationPaidCount);



            }
        }
        public decimal DaysUnpaid
        {
            get
            {
                if (CalculateHours)
                    return (EarnedDaysRemainingHoursUnpaid - (PeriodDaysUnpaid * CalculateHoursDayFactor));
                else
                    return (RemainingDaysUnpaid - PeriodDaysUnpaid);
            }
        }
        public decimal DaysAdvance
        {
            get
            {
                if (CalculateHours)
                    return (EarnedDaysRemainingHoursAdvance - (PeriodDaysAdvance * CalculateHoursDayFactor));
                else
                    return (RemainingDaysAdvance - PeriodDaysAdvance);
            }
        }
        public decimal DaysSaved
        {
            get
            {
                if (CalculateHours)
                    return ((EarnedDaysRemainingHoursYear1 + EarnedDaysRemainingHoursYear2 + EarnedDaysRemainingHoursYear3 + EarnedDaysRemainingHoursYear4 + EarnedDaysRemainingHoursYear5 + EarnedDaysRemainingHoursOverdue) -
                        ((PeriodDaysSavedYear1 + PeriodDaysSavedYear2 + PeriodDaysSavedYear3 + PeriodDaysSavedYear4 + PeriodDaysSavedYear5 + PeriodDaysOverdue + PeriodVacationCompensationSavedCount) * CalculateHoursDayFactor));
                else
                    return ((RemainingDaysYear1 + RemainingDaysYear2 + RemainingDaysYear3 + RemainingDaysYear4 + RemainingDaysYear5 + RemainingDaysOverdue) -
                            (PeriodDaysSavedYear1 + PeriodDaysSavedYear2 + PeriodDaysSavedYear3 + PeriodDaysSavedYear4 + PeriodDaysSavedYear5 + PeriodDaysOverdue + PeriodVacationCompensationSavedCount));
            }
        }

        public decimal DaysSum
        {
            get
            {
                return this.DaysPaid + this.DaysUnpaid + this.DaysAdvance + this.DaysSaved;
            }
        }

        public bool HasNegativeVacationDays
        {
            get
            {
                return this.DaysSum < 0 ||
                        this.RemainingDaysAdvance < 0 ||
                        this.RemainingDaysOverdue < 0 ||
                        this.RemainingDaysPaid < 0 ||
                        this.RemainingDaysUnpaid < 0 ||
                        this.RemainingDaysYear1 < 0 ||
                        this.RemainingDaysYear2 < 0 ||
                        this.RemainingDaysYear3 < 0 ||
                        this.RemainingDaysYear4 < 0 ||
                        this.RemainingDaysYear5 < 0;
            }
        }
    }
}
