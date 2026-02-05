using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class PayrollReviewHead : ICreatedModified, IState
    {
        public string StatusName { get; set; }
        public string PayrollGroupNames { get; set; }
        public string PayrollPriceTypeNames { get; set; }
        public string PayrollLevelNames { get; set; }
    }

    public static partial class EntityExtensions
    {
        #region PayrollReview

        public static PayrollReviewHeadDTO ToDTO(this PayrollReviewHead e, bool includeRows)
        {
            if (e == null)
                return null;

            PayrollReviewHeadDTO dto = new PayrollReviewHeadDTO()
            {
                PayrollReviewHeadId = e.PayrollReviewHeadId,
                Name = e.Name,
                DateFrom = e.DateFrom,
                Status = (TermGroup_PayrollReviewStatus)e.Status,
                StatusName = e.StatusName,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            dto.PayrollGroupIds = e.GetPayrollGroupIds();
            dto.PayrollGroupNames = e.PayrollGroupNames;
            dto.PayrollPriceTypeIds = e.GetPayrollPriceTypeIds();
            dto.PayrollPriceTypeNames = e.PayrollGroupNames;
            dto.PayrollLevelIds = e.GetPayrollLevelIds();
            dto.PayrollLevelNames = e.PayrollLevelNames;
            if (includeRows)
                dto.Rows = e.PayrollReviewRow.ToDTOs().ToList();

            return dto;
        }

        public static IEnumerable<PayrollReviewHeadDTO> ToDTOs(this IEnumerable<PayrollReviewHead> l, bool includeRows)
        {
            var dtos = new List<PayrollReviewHeadDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeRows));
                }
            }
            return dtos;
        }

        public static PayrollReviewRowDTO ToDTO(this PayrollReviewRow e)
        {
            if (e == null)
                return null;

            return new PayrollReviewRowDTO()
            {
                PayrollReviewRowId = e.PayrollReviewRowId,
                PayrollReviewHeadId = e.PayrollReviewHeadId,
                EmployeeId = e.EmployeeId,
                PayrollGroupId = e.PayrollGroupId,
                PayrollPriceTypeId = e.PayrollPriceTypeId,
                PayrollLevelId = e.PayrollLevelId,
                Adjustment = e.Adjustment,
                Amount = e.Amount,
                EmployeeNr = e.Employee?.EmployeeNr ?? string.Empty,
                EmployeeName = e.Employee?.Name ?? string.Empty,
                PayrollGroupName = e.PayrollGroup?.Name ?? string.Empty,
                PayrollPriceTypeName = e.PayrollPriceType?.Name ?? string.Empty,
            };
        }

        public static IEnumerable<PayrollReviewRowDTO> ToDTOs(this IEnumerable<PayrollReviewRow> l)
        {
            var dtos = new List<PayrollReviewRowDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static List<int> GetPayrollGroupIds(this PayrollReviewHead e)
        {
            return e?.PayrollReviewHeadPayrollGroup?.Select(p => p.PayrollGroupId).ToList() ?? new List<int>();
        }

        public static string GetPayrollGroupNames(this PayrollReviewHead e, List<PayrollGroup> payrollGroups)
        {
            List<int> ids = e.GetPayrollGroupIds();
            return !ids.IsNullOrEmpty() ? payrollGroups?.Where(p => ids.Contains(p.PayrollGroupId)).Select(p => p.Name).ToCommaSeparated() : null;
        }

        public static List<int> GetPayrollPriceTypeIds(this PayrollReviewHead e)
        {
            return e?.PayrollReviewHeadPayrollPriceType?.Select(p => p.PayrollPriceTypeId).ToList() ?? new List<int>();
        }

        public static string GetPayrollPriceTypeNames(this PayrollReviewHead e, List<PayrollPriceType> payrollPriceTypes)
        {
            List<int> ids = e.GetPayrollPriceTypeIds();
            return !ids.IsNullOrEmpty() ? payrollPriceTypes?.Where(p => ids.Contains(p.PayrollPriceTypeId)).Select(p => p.Name).ToCommaSeparated() : null;
        }

        public static List<int?> GetPayrollLevelIds(this PayrollReviewHead e)
        {
            return e?.PayrollReviewHeadPayrollLevel?.Select(p => p.PayrollLevelId).ToList() ?? new List<int?>();
        }

        public static string GetPayrollLevelNames(this PayrollReviewHead e, List<PayrollLevel> payrollLevels)
        {
            List<int?> ids = e.GetPayrollLevelIds();
            return !ids.IsNullOrEmpty() ? payrollLevels?.Where(p => ids.Contains(p.PayrollLevelId)).Select(p => p.Name).ToCommaSeparated() : null;
        }

        #endregion
    }
}
