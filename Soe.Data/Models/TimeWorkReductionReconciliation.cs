using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class TimeWorkReductionReconciliation : ICreatedModified, IState
    {
        public TermGroup_TimeWorkReductionWithdrawalMethod GetDefaultWithdrawalMethod()
        {
            if (this.UseDirectPayment && !this.UsePensionDeposit)
                return TermGroup_TimeWorkReductionWithdrawalMethod.DirectPayment;
            else if (!this.UseDirectPayment && this.UsePensionDeposit)
                return TermGroup_TimeWorkReductionWithdrawalMethod.PensionDeposit;
            else
                return TermGroup_TimeWorkReductionWithdrawalMethod.NotChoosed;
        }
    }

    public static partial class EntityExtensions
    {
        #region TimeWorkReduction

        public static IEnumerable<TimeWorkReductionReconciliationGridDTO> ToGridDTOs(this IEnumerable<TimeWorkReductionReconciliation> l)
        {
            var dtos = new List<TimeWorkReductionReconciliationGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        public static TimeWorkReductionReconciliationGridDTO ToGridDTO(this TimeWorkReductionReconciliation e)
        {
            if (e == null)
                return null;

            var dto = new TimeWorkReductionReconciliationGridDTO()
            {
                TimeWorkReductionReconciliationId = e.TimeWorkReductionReconciliationId,
                Description = e.Description,
                ActorCompanyId = e.ActorCompanyId,
                TimeAccumulatorId = e.TimeAccumulatorId,
                UsePensionDeposit = e.UsePensionDeposit,
                UseDirectPayment = e.UseDirectPayment,
                DefaultWithdrawalMethod = (TermGroup_TimeWorkReductionWithdrawalMethod)e.DefaultWithdrawalMethod,
                State = (SoeEntityState)e.State,
            };

            return dto;

        }
        public static IEnumerable<TimeWorkReductionReconciliationDTO> ToDTOs(this IEnumerable<TimeWorkReductionReconciliation> l)
        {
            var dtos = new List<TimeWorkReductionReconciliationDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static TimeWorkReductionReconciliationDTO ToDTO(this TimeWorkReductionReconciliation e)
        {
            if (e == null)
                return null;

            var dto = new TimeWorkReductionReconciliationDTO()
            {
                TimeWorkReductionReconciliationId = e.TimeWorkReductionReconciliationId,
                Description = e.Description,
                ActorCompanyId = e.ActorCompanyId,
                TimeAccumulatorId = e.TimeAccumulatorId,
                UsePensionDeposit = e.UsePensionDeposit,
                UseDirectPayment = e.UseDirectPayment,
                DefaultWithdrawalMethod = (TermGroup_TimeWorkReductionWithdrawalMethod)e.DefaultWithdrawalMethod,
                State = (SoeEntityState)e.State
            };

            if (e.TimeWorkReductionReconciliationYear != null)
                dto.TimeWorkReductionReconciliationYearDTO = (List<TimeWorkReductionReconciliationYearDTO>)e.TimeWorkReductionReconciliationYear.Where(w=> w.State == (int)SoeEntityState.Active).ToDTOs();

            return dto;
        }
        public static IEnumerable<TimeWorkReductionReconciliationYearDTO> ToDTOs(this IEnumerable<TimeWorkReductionReconciliationYear> l)
        {
            var dtos = new List<TimeWorkReductionReconciliationYearDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static TimeWorkReductionReconciliationYearDTO ToDTO(this TimeWorkReductionReconciliationYear e)
        {
            if (e == null)
                return null;

            return new TimeWorkReductionReconciliationYearDTO()
            {
                TimeWorkReductionReconciliationYearId = e.TimeWorkReductionReconciliationYearId,
                TimeWorkReductionReconciliationId = e.TimeWorkReductionReconciliationId,
                Stop = e.Stop,
                EmployeeLastDecidedDate = e.EmployeeLastDecidedDate,
                PensionDepositPayrollProductId = e.PensionDepositPayrollProductId,
                DirectPaymentPayrollProductId = e.DirectPaymentPayrollProductId,
                State = (SoeEntityState)e.State,
            };
        }

        public static IEnumerable<TimeWorkReductionReconciliationEmployeeDTO> ToDTOs(this IEnumerable<TimeWorkReductionReconciliationEmployee> l)
        {
            var dtos = new List<TimeWorkReductionReconciliationEmployeeDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static TimeWorkReductionReconciliationEmployeeDTO ToDTO(this TimeWorkReductionReconciliationEmployee e)
        {
            if (e == null)
                return null;

            return new TimeWorkReductionReconciliationEmployeeDTO()
            {
                TimeWorkReductionReconciliationEmployeeId = e.TimeWorkReductionReconciliationEmployeeId,
                TimeWorkReductionReconciliationYearId = e.TimeWorkReductionReconciliationYearId,
                TimeWorkReductionReconciliationId = e.TimeWorkReductionReconciliationId,
                EmployeeId = e.EmployeeId,
                EmployeeNrAndName = e.Employee?.EmployeeNrAndName ?? string.Empty,
                EmployeeName = e.Employee?.Name ?? string.Empty,
                EmployeeNr = e.Employee?.EmployeeNr ?? string.Empty,
                MinutesOverThreshold = e.MinutesOverThreshold,
                SentDate = e.SentDate,
                SelectedWithdrawalMethod = (TermGroup_TimeWorkReductionWithdrawalMethod)e.SelectedWithdrawalMethod,
                SelectedDate = e.SelectedDate,
                Status = e.Status,
                State = (SoeEntityState)e.State,
                AccEarningMinutes = e.AccEarningMinutes,
                Threshold = e.Threshold,
            };
        }

        #endregion
    }


}
