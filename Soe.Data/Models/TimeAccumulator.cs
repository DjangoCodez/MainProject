using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoftOne.Soe.Data
{
    public partial class TimeAccumulator : ICreatedModified, IState
    {
        public string TypeName { get; set; }
    }

    public static partial class EntityExtensions
    {
        public static TimeAccumulatorDTO ToDTO(this TimeAccumulator e)
        {
            if (e == null)
                return null;

            TimeAccumulatorDTO dto = new TimeAccumulatorDTO()
            {
                TimeAccumulatorId = e.TimeAccumulatorId,
                ActorCompanyId = e.ActorCompanyId,
                TimePeriodHeadId = e.TimePeriodHeadId,
                TimeCodeId = e.TimeCodeId,
                Type = (TermGroup_TimeAccumulatorType)e.Type,
                TypeName = e.TypeName,
                Name = e.Name,
                Description = e.Description,
                ShowInTimeReports = e.ShowInTimeReports,
                FinalSalary = e.FinalSalary,
                TimePeriodHeadName = e.TimePeriodHead?.Name ?? string.Empty,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                UseTimeWorkAccount = e.UseTimeWorkAccount,
                UseTimeWorkReductionWithdrawal = e.UseTimeWorkReductionWithdrawal,
                TimeWorkReductionEarningId = e.TimeWorkReductionEarningId,
            };

            if (e.TimeAccumulatorInvoiceProduct != null)
                dto.InvoiceProducts = e.TimeAccumulatorInvoiceProduct.ToDTOs();
            if (e.TimeAccumulatorPayrollProduct != null)
                dto.PayrollProducts = e.TimeAccumulatorPayrollProduct.ToDTOs();
            if (e.TimeAccumulatorTimeCode != null)
                dto.TimeCodes = e.TimeAccumulatorTimeCode.ToDTOs();
            if (e.TimeAccumulatorEmployeeGroupRule != null)
                dto.EmployeeGroupRules = e.TimeAccumulatorEmployeeGroupRule.Where(r => r.State == (int)SoeEntityState.Active).ToDTOs();
            if (e.TimeWorkReductionEarningId != null)
                dto.TimeWorkReductionEarning = e.TimeWorkReductionEarning.ToDTO();

            return dto;
        }

        public static IEnumerable<TimeAccumulatorDTO> ToDTOs(this IEnumerable<TimeAccumulator> l)
        {
            var dtos = new List<TimeAccumulatorDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static TimeAccumulator FromGridDTO(this TimeAccumulatorGridDTO dto)
        {
            if (dto == null)
                return null;

            return new TimeAccumulator()
            {
                TimeAccumulatorId = dto.TimeAccumulatorId,
                Type = (int)dto.Type,
                Name = dto.Name,
                Description = dto.Description,
                TypeName = dto.TypeName,
            };
        }

        public static IEnumerable<TimeAccumulator> FromGridDTOs(this IEnumerable<TimeAccumulatorGridDTO> l)
        {
            var dtos = new List<TimeAccumulator>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.FromGridDTO());
                }
            }
            return dtos;
        }

        public static TimeAccumulatorGridDTO ToGridDTO(this TimeAccumulator e)
        {
            if (e == null)
                return null;

            return new TimeAccumulatorGridDTO()
            {
                TimeAccumulatorId = e.TimeAccumulatorId,
                Type = e.Type != 0 ? (TermGroup_TimeAccumulatorType)e.Type : TermGroup_TimeAccumulatorType.Rolling,
                Name = e.Name,
                Description = e.Description,
                TypeName = e.TypeName,
                ShowInTimeReports = e.ShowInTimeReports,
                TimePeriodHeadName = e.TimePeriodHead?.Name ?? string.Empty,
                State = (SoeEntityState)e.State,
            };
        }

        public static IEnumerable<TimeAccumulatorGridDTO> ToGridDTOs(this IEnumerable<TimeAccumulator> l)
        {
            var dtos = new List<TimeAccumulatorGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }
    }
}
