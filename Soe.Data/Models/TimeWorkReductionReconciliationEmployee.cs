using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Data
{
    public partial class TimeWorkReductionReconciliationEmployee : ICreatedModified, IState
    {
        public static TimeWorkReductionReconciliationEmployee Create(
            Employee employee,
            TimeWorkReductionReconciliation reconciliation,
            TimeWorkReductionReconciliationYear reconciliationYear,
            int accEarningMinutes,
            int threshold,
            TermGroup_TimeWorkReductionWithdrawalMethod withdrawalMethod
            )
        {
            if (employee == null || reconciliation == null || reconciliationYear == null)
                return null;

            return new TimeWorkReductionReconciliationEmployee
            {
                EmployeeId = employee.EmployeeId,
                TimeWorkReductionReconciliationId = reconciliation.TimeWorkReductionReconciliationId,
                TimeWorkReductionReconciliationYearId = reconciliationYear.TimeWorkReductionReconciliationYearId,
                AccEarningMinutes = accEarningMinutes,
                Threshold = threshold,
                MinutesOverThreshold = accEarningMinutes > threshold ? accEarningMinutes - threshold : 0,
                SelectedWithdrawalMethod = (int)withdrawalMethod,
                Status = (int)TermGroup_TimeWorkReductionReconciliationEmployeeStatus.Created,
            };
        }

        public void SetCalculated()
        {
            this.Status = (int)TermGroup_TimeWorkReductionReconciliationEmployeeStatus.Calculated;
        }
        public void UpdateStatus(TermGroup_TimeWorkReductionReconciliationEmployeeStatus status, string modifiedBy, DateTime? modified = null)
        {
            if (this.Status != (int)status)
            {
                this.Status = (int)status;
                this.SetModified(modified ?? DateTime.Now, modifiedBy);
            }
        }
    }
}
