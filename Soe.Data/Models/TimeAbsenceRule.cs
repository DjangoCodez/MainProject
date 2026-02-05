using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class TimeAbsenceRuleHead : ICreatedModified, IState
    {
        public string TypeName { get; set; }
        public string TimeCodeName { get; set; }
        public string CompanyName { get; set; }
        public string EmployeeGroupNames { get; set; }
        public bool IsSickDuringIwh
        {
            get
            {
                return this.Type == (int)TermGroup_TimeAbsenceRuleType.SickDuringInconvenientWorkingHours_PAID ||
                       this.Type == (int)TermGroup_TimeAbsenceRuleType.SickDuringInconvenientWorkingHours_UNPAID;
            }
        }
        public bool IsSickDuringStandby
        {
            get
            {
                return this.Type == (int)TermGroup_TimeAbsenceRuleType.SickDuringStandby_PAID ||
                       this.Type == (int)TermGroup_TimeAbsenceRuleType.SickDuringStandby_UNPAID;
            }
        }
        public bool IsSickDuringIwhOrStandBy
        {
            get
            {
                return this.IsSickDuringIwh || this.IsSickDuringStandby;
            }
        }
    }

    public partial class TimeAbsenceRuleRow : ICreatedModified, IState
    {
        public string PayrollProductName { get; set; }
        public string TypeName { get; set; }
        public string ScopeName { get; set; }
    }

    public static partial class EntityExtensions
    {
        #region TimeAbsenceRuleHead

        public static TimeAbsenceRuleHeadDTO ToDTO(this TimeAbsenceRuleHead e, bool includeRows = false, bool includePayrollProducts = false)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded() && includeRows && !e.TimeAbsenceRuleRow.IsLoaded)
                {
                    e.TimeAbsenceRuleRow.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("TimeAbsenceRule.cs e.TimeAbsenceRuleRow");
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            TimeAbsenceRuleHeadDTO dto = new TimeAbsenceRuleHeadDTO()
            {
                TimeAbsenceRuleHeadId = e.TimeAbsenceRuleHeadId,
                ActorCompanyId = e.ActorCompanyId,
                TimeCodeId = e.TimeCodeId,
                EmployeeGroupIds = e.GetEmployeeGroupIds(),
                Type = (TermGroup_TimeAbsenceRuleType)e.Type,
                Name = e.Name,
                Description = e.Description,
                TypeName = e.TypeName,
                CompanyName = e.CompanyName,
                TimeCodeName = e.TimeCodeName,
                EmployeeGroupNames = e.EmployeeGroupNames,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            if (e.TimeCode != null)
                dto.TimeCode = e.TimeCode.ToDTO(includePayrollProducts);
            if (includeRows)
                dto.TimeAbsenceRuleRows = e.TimeAbsenceRuleRow?.Where(r => r.State == (int)SoeEntityState.Active).OrderBy(r => r.Start).ThenBy(r => r.Stop).ThenBy(r => r.Type).ToDTOs().ToList() ?? new List<TimeAbsenceRuleRowDTO>();

            return dto;
        }

        public static IEnumerable<TimeAbsenceRuleHeadDTO> ToDTOs(this IEnumerable<TimeAbsenceRuleHead> l, bool includeRows = false, bool includePayrollProducts = false)
        {
            var dtos = new List<TimeAbsenceRuleHeadDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeRows, includePayrollProducts));
                }
            }
            return dtos;
        }

        public static TimeAbsenceRuleHeadGridDTO ToGridDTO(this TimeAbsenceRuleHead e)
        {
            if (e == null)
                return null;

            return new TimeAbsenceRuleHeadGridDTO()
            {
                TimeAbsenceRuleHeadId = e.TimeAbsenceRuleHeadId,
                Type = (TermGroup_TimeAbsenceRuleType)e.Type,
                TypeName = e.TypeName,
                ActorCompanyId = e.ActorCompanyId,
                EmployeeGroupIds = e.GetEmployeeGroupIds(),
                EmployeeGroupNames = e.EmployeeGroupNames,
                TimeCodeId = e.TimeCodeId,
                TimeCodeName = e.TimeCodeName,
                Name = e.Name,
                Description = e.Description,
            };
        }

        public static IEnumerable<TimeAbsenceRuleHeadGridDTO> ToGridDTOs(this IEnumerable<TimeAbsenceRuleHead> l)
        {
            var dtos = new List<TimeAbsenceRuleHeadGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        public static List<TimeAbsenceRuleHead> Filter(this List<TimeAbsenceRuleHead> l, TermGroup_TimeAbsenceRuleType absenceRuleType, int timeCodeId, int employeeGroupId)
        {
            if (l.IsNullOrEmpty())
                return new List<TimeAbsenceRuleHead>();

            return l
                .FilterByTypeAndTimeCode(absenceRuleType, timeCodeId)
                .FilterByEmployeeeGroup(employeeGroupId)
                .ToList();
        }

        public static List<TimeAbsenceRuleHead> FilterByTypeAndTimeCode(this List<TimeAbsenceRuleHead> l, TermGroup_TimeAbsenceRuleType absenceRuleType, int timeCodeId)
        {
            if (l.IsNullOrEmpty())
                return new List<TimeAbsenceRuleHead>();

            return l
                .Where(i => i.Type == (int)absenceRuleType && i.TimeCodeId == timeCodeId)
                .ToList();
        }

        public static List<TimeAbsenceRuleHead> FilterByEmployeeeGroup(this IEnumerable<TimeAbsenceRuleHead> l, int employeeGroupId)
        {
            var valid = new List<TimeAbsenceRuleHead>();
            foreach (var e in l)
            {
                List<int> employeeGroupIds = e.GetEmployeeGroupIds();
                if (employeeGroupIds.IsNullOrEmpty() || employeeGroupIds.Contains(employeeGroupId))
                    valid.Add(e);
            }
            return valid;
        }

        #endregion

        #region TimeAbsenceRuleRow

        public static TimeAbsenceRuleRowDTO ToDTO(this TimeAbsenceRuleRow e, bool loadTimeAbsenceRuleRowPayrollProducts = false)
        {
            if (e == null)
                return null;

            TimeAbsenceRuleRowDTO dto = new TimeAbsenceRuleRowDTO()
            {
                TimeAbsenceRuleRowId = e.TimeAbsenceRuleRowId,
                TimeAbsenceRuleHeadId = e.TimeAbsenceRuleHeadId,
                PayrollProductId = e.PayrollProductId,
                PayrollProductNr = e.PayrollProduct?.Number ?? string.Empty,
                PayrollProductName = e.PayrollProduct?.Name ?? e.PayrollProductName,
                HasMultiplePayrollProducts = e.HasMultiplePayrollProducts,
                Type = (TermGroup_TimeAbsenceRuleRowType)e.Type,
                Scope = (TermGroup_TimeAbsenceRuleRowScope)e.Scope,
                TypeName = e.TypeName,
                ScopeName = e.ScopeName,
                Start = e.Start,
                Stop = e.Stop,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            if (!e.TimeAbsenceRuleRowPayrollProducts.IsNullOrEmpty())
                dto.PayrollProductRows = e.TimeAbsenceRuleRowPayrollProducts.ToDTOs().ToList();

            return dto;
        }

        public static IEnumerable<TimeAbsenceRuleRowDTO> ToDTOs(this IEnumerable<TimeAbsenceRuleRow> l)
        {
            var dtos = new List<TimeAbsenceRuleRowDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static TimeAbsenceRuleRowPayrollProductsDTO ToDTO(this TimeAbsenceRuleRowPayrollProducts e)
        {
            if (e == null)
                return null;

            return new TimeAbsenceRuleRowPayrollProductsDTO()
            {
                TimeAbsenceRuleRowPayrollProductsId = e.TimeAbsenceRuleRowPayrollProductsId,
                SourcePayrollProductId = e.SourcePayrollProductId,
                TargetPayrollProductId = e.TargetPayrollProductId,
                SourcePayrollProductNr = e.SourcePayrollProduct?.Number ?? string.Empty,
                SourcePayrollProductName = e.SourcePayrollProduct?.Name ?? string.Empty,
                TargetPayrollProductNr = e.TargetPayrollProduct?.Number ?? string.Empty,
                TargetPayrollProductName = e.TargetPayrollProduct?.Name ?? string.Empty,
            };
        }

        public static IEnumerable<TimeAbsenceRuleRowPayrollProductsDTO> ToDTOs(this IEnumerable<TimeAbsenceRuleRowPayrollProducts> l)
        {
            var dtos = new List<TimeAbsenceRuleRowPayrollProductsDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static List<TimeAbsenceRuleRow> FromDTO(this List<TimeAbsenceRuleRowDTO> l)
        {
            List<TimeAbsenceRuleRow> rows = new List<TimeAbsenceRuleRow>();

            foreach (TimeAbsenceRuleRowDTO e in l)
            {
                TimeAbsenceRuleRow timeAbsenceRuleRow = new TimeAbsenceRuleRow()
                {
                    TimeAbsenceRuleRowId = e.TimeAbsenceRuleRowId,
                    Type = (int)e.Type,
                    Scope = (int)e.Scope,
                    Start = e.Start,
                    Stop = e.Stop,
                    State = (int)e.State,
                };

                if (e.PayrollProductId.HasValue && e.PayrollProductId != 0)
                    timeAbsenceRuleRow.PayrollProductId = e.PayrollProductId.Value;

                if (e.PayrollProductRows != null)
                {
                    foreach (TimeAbsenceRuleRowPayrollProductsDTO payrollProductRowItem in e.PayrollProductRows)
                    {
                        TimeAbsenceRuleRowPayrollProducts timeAbsenceRuleRowPayrollProductRow = new TimeAbsenceRuleRowPayrollProducts()
                        {
                            SourcePayrollProductId = payrollProductRowItem.SourcePayrollProductId
                        };

                        if (payrollProductRowItem.TargetPayrollProductId.HasValue && payrollProductRowItem.TargetPayrollProductId.Value != 0)
                            timeAbsenceRuleRowPayrollProductRow.TargetPayrollProductId = payrollProductRowItem.TargetPayrollProductId.Value;

                        timeAbsenceRuleRow.TimeAbsenceRuleRowPayrollProducts.Add(timeAbsenceRuleRowPayrollProductRow);
                    }
                }

                timeAbsenceRuleRow.HasMultiplePayrollProducts = !timeAbsenceRuleRow.TimeAbsenceRuleRowPayrollProducts.IsNullOrEmpty();

                rows.Add(timeAbsenceRuleRow);
            }

            return rows;
        }

        public static IEnumerable<TimeAbsenceRuleRow> Filter(this IEnumerable<TimeAbsenceRuleRow> l, TermGroup_TimeAbsenceRuleRowType? type = null, TermGroup_TimeAbsenceRuleRowScope? scope = null)
        {
            return l?
                .Where(r =>
                    r.State == (int)SoeEntityState.Active &&
                    (!type.HasValue || r.Type == (int)type.Value) &&
                    (!scope.HasValue || r.Scope == (int)scope.Value));
        }

        public static List<TimeAbsenceRuleRow> GetRows(this TimeAbsenceRuleHead e, TermGroup_TimeAbsenceRuleRowType? type = null, TermGroup_TimeAbsenceRuleRowScope? scope = null)
        {
            return e?.TimeAbsenceRuleRow
                .Filter(type, scope)
                .ToList() ?? new List<TimeAbsenceRuleRow>();
        }

        public static TimeAbsenceRuleRow GetRow(this List<TimeAbsenceRuleRow> l, int absenceDayNumber, TermGroup_TimeAbsenceRuleRowType? absenceRuleRowType = null)
        {
            return l?
                .Filter(absenceRuleRowType)
                .Where(r => r.Start <= absenceDayNumber && r.Stop >= absenceDayNumber)
                .OrderBy(r => r.Start)
                .ThenBy(e => e.Stop)
                .FirstOrDefault();
        }

        public static TimeAbsenceRuleRow GetLastRow(this List<TimeAbsenceRuleRow> l, int? stopsBefore = null)
        {
            return l?.Where(r => !stopsBefore.HasValue || r.Stop < stopsBefore.Value).OrderBy(r => r.Stop).ThenBy(r => r.Start).LastOrDefault();
        }

        public static TimeAbsenceRuleRowPayrollProducts GetSourceProductRow(this TimeAbsenceRuleRow e, int sourceProductId)
        {
            return e?.TimeAbsenceRuleRowPayrollProducts?.FirstOrDefault(i => i.SourcePayrollProductId == sourceProductId);
        }

        public static bool ContainsSequenceRow(this TimeAbsenceRuleHead e)
        {
            List<TimeAbsenceRuleRow> rows = e.GetRows();
            foreach (TimeAbsenceRuleRow row in rows)
            {
                if (rows.Any(r => r.Start == row.Start && r.Stop != row.Stop && r.Type == row.Type))
                    return true;
            }
            return false;
        }

        public static bool ContainsScope(this List<TimeAbsenceRuleRowDTO> l, TermGroup_TimeAbsenceRuleRowScope scope)
        {
            return l?.Any(r => r.Scope == scope && r.State == SoeEntityState.Active) ?? false;
        }

        public static bool IsScopeCalendarYear(this TimeAbsenceRuleHead e)
        {
            if (e?.TimeAbsenceRuleRow != null && e.TimeAbsenceRuleRow.Filter(scope: TermGroup_TimeAbsenceRuleRowScope.Calendaryear).Any())
                return true;
            return false;
        }

        public static int GetMaxDays(this TimeAbsenceRuleHead e, TermGroup_TimeAbsenceRuleRowType type, bool doApplyInfinityAssumption = false)
        {
            return e?.GetRows(type).GetMaxDays(doApplyInfinityAssumption) ?? 0;
        }

        public static int GetMaxDays(this List<TimeAbsenceRuleRow> l, bool doApplyInfinityAssumption = false)
        {
            if (doApplyInfinityAssumption)
            {
                TimeAbsenceRuleRow lastRow = l.GetLastRow();
                if (lastRow != null && lastRow.Stop >= 999)
                {
                    TimeAbsenceRuleRow nextLastRow = l.GetLastRow(stopsBefore: lastRow.Stop);
                    if (nextLastRow != null)
                        return nextLastRow.Stop + 1;
                }
            }
            return l?.Max(i => i.Stop) ?? 0;
        }

        public static bool IsInInterval(this TimeAbsenceRuleRow e, int dayNumber)
        {
            return e != null && e.Start <= dayNumber && e.Stop >= dayNumber;
        }

        #endregion

        #region TimeAbsenceRuleHeadEmployeeGroup

        public static List<TimeAbsenceRuleHeadEmployeeGroup> GetEmployeeGroups(this TimeAbsenceRuleHead e)
        {
            return e?.TimeAbsenceRuleHeadEmployeeGroup?.Where(m => m.State == (int)SoeEntityState.Active).ToList() ?? new List<TimeAbsenceRuleHeadEmployeeGroup>();
        }

        public static List<int> GetEmployeeGroupIds(this TimeAbsenceRuleHead e)
        {
            return e.GetEmployeeGroups().Select(m => m.EmployeeGroupId).Distinct().ToList();
        }

        #endregion
    }
}
