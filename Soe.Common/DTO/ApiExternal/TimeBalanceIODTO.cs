using System;

namespace SoftOne.Soe.Common.DTO.ApiExternal
{
    public class TimeBalanceIODTO
    {
        /// <summary>
        /// Set Type of balance
        ///  Unknown = 0,
        ///  TimeAccumulator = 1,
        ///  RemainingDaysPaid = 2,
        ///  RemainingDaysYear1 = 3,
        ///  RemainingDaysYear2 = 4,
        ///  RemainingDaysYear3 = 5,
        ///  RemainingDaysYear4 = 6,
        ///  RemainingDaysYear5 = 7,
        ///  RemainingDaysOverdue = 8,
        ///  RemainingDaysUnPaid = 9,
        /// </summary>
        public TimeBalanceIOType TimeBalanceIOType { get; set; }
        /// <summary>
        /// Name on Type 1 (TimeAccumulator) otherwise empty
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Set date of balance
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// Set employeeNr in order to connect balance to the correct employee
        /// </summary>
        public string EmployeeNr { get; set; }
        /// <summary>
        /// On type 1 (TimeAccumulator) this is set in hours (3,5 => 3 hours and 30 minutes)
        /// On type 1-8 this set in days
        /// </summary>
        public decimal Quantity { get; set; }
        /// <summary>
        /// Set this on Vacationdays. Needs to be the same date on all values of the same batch.
        /// </summary>
        public DateTime? AdjustmentDate { get; set; }
    }

    /// <summary>
    /// TimeAccumulator = 1 (Like Flex or Comp)
    /// RemainingDaysPaid = 2 (Remaining paid days current vacationyear)
    /// RemainingDaysYear1 to 5 =3-7 (Remaing days from previous years)
    /// RemainingDaysOverdue = 8 (Days that are older than 5 years)
    /// RemainingDaysUnPaid = 2 (Remaining unpaid days current vacationyear)
    /// </summary>

    public enum TimeBalanceIOType
    {
        Unknown = 0,
        TimeAccumulator = 1,
        RemainingDaysPaid = 2,
        RemainingDaysYear1 = 3,
        RemainingDaysYear2 = 4,
        RemainingDaysYear3 = 5,
        RemainingDaysYear4 = 6,
        RemainingDaysYear5 = 7,
        RemainingDaysOverdue = 8,
        RemainingDaysUnPaid = 9,
        RemainingDaysAdvance = 10,
        CurrentLASDays = 20,
        BalanceLASDays = 21
    }
}
