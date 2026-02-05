using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class EmployeeVacationSE : ICreatedModified, IState
    {
        public DateTime? AdjustmentDateOrCreated
        {
            get
            {
                return this.AdjustmentDate ?? this.Created;
            }
        }

        public EmployeeVacationSE Clone(Employee employee, int prevVacationSEId)
        {
            return new EmployeeVacationSE()
            {
                Employee = employee,
                PrevEmployeeVacationSEId = prevVacationSEId != 0 ? prevVacationSEId : (int?)null,
                EarnedDaysPaid = this.EarnedDaysPaid,
                EarnedDaysUnpaid = this.EarnedDaysUnpaid,
                EarnedDaysAdvance = this.EarnedDaysAdvance,
                SavedDaysYear1 = this.SavedDaysYear1,
                SavedDaysYear2 = this.SavedDaysYear2,
                SavedDaysYear3 = this.SavedDaysYear3,
                SavedDaysYear4 = this.SavedDaysYear4,
                SavedDaysYear5 = this.SavedDaysYear5,
                SavedDaysOverdue = this.SavedDaysOverdue,

                UsedDaysPaid = this.UsedDaysPaid,
                PaidVacationAllowance = this.PaidVacationAllowance,
                PaidVacationVariableAllowance = this.PaidVacationVariableAllowance,
                UsedDaysUnpaid = this.UsedDaysUnpaid,
                UsedDaysAdvance = this.UsedDaysAdvance,
                UsedDaysYear1 = this.UsedDaysYear1,
                UsedDaysYear2 = this.UsedDaysYear2,
                UsedDaysYear3 = this.UsedDaysYear3,
                UsedDaysYear4 = this.UsedDaysYear4,
                UsedDaysYear5 = this.UsedDaysYear5,
                UsedDaysOverdue = this.UsedDaysOverdue,

                RemainingDaysPaid = this.RemainingDaysPaid,
                RemainingDaysUnpaid = this.RemainingDaysUnpaid,
                RemainingDaysAdvance = this.RemainingDaysAdvance,
                RemainingDaysYear1 = this.RemainingDaysYear1,
                RemainingDaysYear2 = this.RemainingDaysYear2,
                RemainingDaysYear3 = this.RemainingDaysYear3,
                RemainingDaysYear4 = this.RemainingDaysYear4,
                RemainingDaysYear5 = this.RemainingDaysYear5,
                RemainingDaysOverdue = this.RemainingDaysOverdue,

                EarnedDaysRemainingHoursPaid = this.EarnedDaysRemainingHoursPaid,
                EarnedDaysRemainingHoursUnpaid = this.EarnedDaysRemainingHoursUnpaid,
                EarnedDaysRemainingHoursAdvance = this.EarnedDaysRemainingHoursAdvance,
                EarnedDaysRemainingHoursYear1 = this.EarnedDaysRemainingHoursYear1,
                EarnedDaysRemainingHoursYear2 = this.EarnedDaysRemainingHoursYear2,
                EarnedDaysRemainingHoursYear3 = this.EarnedDaysRemainingHoursYear3,
                EarnedDaysRemainingHoursYear4 = this.EarnedDaysRemainingHoursYear4,
                EarnedDaysRemainingHoursYear5 = this.EarnedDaysRemainingHoursYear5,
                EarnedDaysRemainingHoursOverdue = this.EarnedDaysRemainingHoursOverdue,

                EmploymentRatePaid = this.EmploymentRatePaid,
                EmploymentRateYear1 = this.EmploymentRateYear1,
                EmploymentRateYear2 = this.EmploymentRateYear2,
                EmploymentRateYear3 = this.EmploymentRateYear3,
                EmploymentRateYear4 = this.EmploymentRateYear4,
                EmploymentRateYear5 = this.EmploymentRateYear5,
                EmploymentRateOverdue = this.EmploymentRateOverdue,

                DebtInAdvanceAmount = this.DebtInAdvanceAmount,
                DebtInAdvanceDueDate = this.DebtInAdvanceDueDate,
                DebtInAdvanceDelete = this.DebtInAdvanceDelete,
            };
        }
    }

    public static partial class EntityExtensions
    {
        #region EmployeeVacationSE

        public static EmployeeVacationSEDTO ToDTO(this EmployeeVacationSE e)
        {
            if (e == null)
                return null;

            return new EmployeeVacationSEDTO()
            {
                EmployeeVacationSEId = e.EmployeeVacationSEId,
                EmployeeId = e.EmployeeId,
                AdjustmentDate = e.AdjustmentDate,
                EarnedDaysPaid = e.EarnedDaysPaid,
                EarnedDaysUnpaid = e.EarnedDaysUnpaid,
                EarnedDaysAdvance = e.EarnedDaysAdvance,
                SavedDaysYear1 = e.SavedDaysYear1,
                SavedDaysYear2 = e.SavedDaysYear2,
                SavedDaysYear3 = e.SavedDaysYear3,
                SavedDaysYear4 = e.SavedDaysYear4,
                SavedDaysYear5 = e.SavedDaysYear5,
                SavedDaysOverdue = e.SavedDaysOverdue,
                UsedDaysPaid = e.UsedDaysPaid,
                PaidVacationAllowance = e.PaidVacationAllowance,
                PaidVacationVariableAllowance = e.PaidVacationVariableAllowance,
                UsedDaysUnpaid = e.UsedDaysUnpaid,
                UsedDaysAdvance = e.UsedDaysAdvance,
                UsedDaysYear1 = e.UsedDaysYear1,
                UsedDaysYear2 = e.UsedDaysYear2,
                UsedDaysYear3 = e.UsedDaysYear3,
                UsedDaysYear4 = e.UsedDaysYear4,
                UsedDaysYear5 = e.UsedDaysYear5,
                UsedDaysOverdue = e.UsedDaysOverdue,
                RemainingDaysPaid = e.RemainingDaysPaid,
                RemainingDaysUnpaid = e.RemainingDaysUnpaid,
                RemainingDaysAdvance = e.RemainingDaysAdvance,
                RemainingDaysYear1 = e.RemainingDaysYear1,
                RemainingDaysYear2 = e.RemainingDaysYear2,
                RemainingDaysYear3 = e.RemainingDaysYear3,
                RemainingDaysYear4 = e.RemainingDaysYear4,
                RemainingDaysYear5 = e.RemainingDaysYear5,
                RemainingDaysOverdue = e.RemainingDaysOverdue,
                EarnedDaysRemainingHoursPaid = e.EarnedDaysRemainingHoursPaid,
                EarnedDaysRemainingHoursUnpaid = e.EarnedDaysRemainingHoursUnpaid,
                EarnedDaysRemainingHoursAdvance = e.EarnedDaysRemainingHoursAdvance,
                EarnedDaysRemainingHoursYear1 = e.EarnedDaysRemainingHoursYear1,
                EarnedDaysRemainingHoursYear2 = e.EarnedDaysRemainingHoursYear2,
                EarnedDaysRemainingHoursYear3 = e.EarnedDaysRemainingHoursYear3,
                EarnedDaysRemainingHoursYear4 = e.EarnedDaysRemainingHoursYear4,
                EarnedDaysRemainingHoursYear5 = e.EarnedDaysRemainingHoursYear5,
                EarnedDaysRemainingHoursOverdue = e.EarnedDaysRemainingHoursOverdue,
                EmploymentRatePaid = e.EmploymentRatePaid,
                EmploymentRateYear1 = e.EmploymentRateYear1,
                EmploymentRateYear2 = e.EmploymentRateYear2,
                EmploymentRateYear3 = e.EmploymentRateYear3,
                EmploymentRateYear4 = e.EmploymentRateYear4,
                EmploymentRateYear5 = e.EmploymentRateYear5,
                EmploymentRateOverdue = e.EmploymentRateOverdue,
                DebtInAdvanceAmount = e.DebtInAdvanceAmount,
                DebtInAdvanceDueDate = e.DebtInAdvanceDueDate,
                DebtInAdvanceDelete = e.DebtInAdvanceDelete,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy
            };
        }

        public static EmployeeVacationSE GetByAdjustmentDateOrCreated(this List<EmployeeVacationSE> l, DateTime dateFrom, DateTime dateTo)
        {
            if (l.IsNullOrEmpty() || dateFrom > dateTo)
                return null;

            return l
                .Where(i => i.AdjustmentDateOrCreated.HasValue && i.AdjustmentDateOrCreated.Value <= dateTo)
                .OrderByDescending(i => i.AdjustmentDateOrCreated.Value)
                .ThenByDescending(i => i.EmployeeVacationSEId)
                .FirstOrDefault();
        }

        public static decimal SumRemainingDays(this EmployeeVacationSE e)
        {
            if (e == null)
                return 0;

            return (
                e.RemainingDaysPaid ?? 0 +
                e.RemainingDaysUnpaid ?? 0 +
                e.RemainingDaysAdvance ?? 0 +
                e.RemainingDaysYear1 ?? 0 +
                e.RemainingDaysYear2 ?? 0 +
                e.RemainingDaysYear3 ?? 0 +
                e.RemainingDaysYear4 ?? 0 +
                e.RemainingDaysYear5 ?? 0 +
                e.RemainingDaysOverdue ?? 0
                );
        }

        public static bool IsSame(this EmployeeVacationSE e, EmployeeIODTO io)
        {
            if (io == null)
                return false;

            return
                e.EarnedDaysPaid == io.EarnedDaysPaid &&
                e.EarnedDaysUnpaid == io.EarnedDaysUnpaid &&
                e.EarnedDaysAdvance == io.EarnedDaysAdvance &&

                e.SavedDaysYear1 == io.SavedDaysYear1 &&
                e.SavedDaysYear2 == io.SavedDaysYear2 &&
                e.SavedDaysYear3 == io.SavedDaysYear3 &&
                e.SavedDaysYear4 == io.SavedDaysYear4 &&
                e.SavedDaysYear5 == io.SavedDaysYear5 &&
                e.SavedDaysOverdue == io.SavedDaysOverdue &&

                e.UsedDaysPaid == io.UsedDaysPaid &&
                e.PaidVacationAllowance == io.PaidVacationAllowance &&
                e.PaidVacationVariableAllowance == io.PaidVacationVariableAllowance &&
                e.UsedDaysUnpaid == io.UsedDaysUnpaid &&
                e.UsedDaysAdvance == io.UsedDaysAdvance &&
                e.UsedDaysYear1 == io.UsedDaysYear1 &&
                e.UsedDaysYear2 == io.UsedDaysYear2 &&
                e.UsedDaysYear3 == io.UsedDaysYear3 &&
                e.UsedDaysYear4 == io.UsedDaysYear4 &&
                e.UsedDaysYear5 == io.UsedDaysYear5 &&
                e.UsedDaysOverdue == io.UsedDaysOverdue &&

                e.RemainingDaysPaid == io.RemainingDaysPaid &&
                e.RemainingDaysUnpaid == io.RemainingDaysUnpaid &&
                e.RemainingDaysAdvance == io.RemainingDaysAdvance &&
                e.RemainingDaysYear1 == io.RemainingDaysYear1 &&
                e.RemainingDaysYear2 == io.RemainingDaysYear2 &&
                e.RemainingDaysYear3 == io.RemainingDaysYear3 &&
                e.RemainingDaysYear4 == io.RemainingDaysYear4 &&
                e.RemainingDaysYear5 == io.RemainingDaysYear5 &&
                e.RemainingDaysOverdue == io.RemainingDaysOverdue &&

                e.EmploymentRatePaid == io.EmploymentRatePaid &&
                e.EmploymentRateYear1 == io.EmploymentRateYear1 &&
                e.EmploymentRateYear2 == io.EmploymentRateYear2 &&
                e.EmploymentRateYear3 == io.EmploymentRateYear3 &&
                e.EmploymentRateYear4 == io.EmploymentRateYear4 &&
                e.EmploymentRateYear5 == io.EmploymentRateYear5 &&
                e.EmploymentRateOverdue == io.EmploymentRateOverdue &&

                e.DebtInAdvanceAmount == io.DebtInAdvanceAmount &&
                e.DebtInAdvanceDueDate == io.DebtInAdvanceDueDate;
        }

        public static bool HasRemainingDays(this EmployeeVacationSE e)
        {
            if (e == null)
                return false;

            return
                e.RemainingDaysPaid > 0 ||
                e.RemainingDaysAdvance > 0 ||
                e.RemainingDaysOverdue > 0 ||
                e.RemainingDaysYear5 > 0 ||
                e.RemainingDaysYear4 > 0 ||
                e.RemainingDaysYear3 > 0 ||
                e.RemainingDaysYear2 > 0 ||
                e.RemainingDaysYear1 > 0;
        }

        #endregion
    }
}
