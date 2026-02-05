using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public static partial class EntityExtensions
    {
        #region EmployeeSetting

        public static EmployeeSettingDTO ToDTO(this EmployeeSetting e)
        {
            if (e == null)
                return null;

            return new EmployeeSettingDTO()
            {
                EmployeeSettingId = e.EmployeeSettingId,
                EmployeeId = e.EmployeeId,
                ActorCompanyId = e.ActorCompanyId,
                EmployeeSettingAreaType = (TermGroup_EmployeeSettingType)e.EmployeeSettingAreaType,
                EmployeeSettingGroupType = (TermGroup_EmployeeSettingType)e.EmployeeSettingGroupType,
                EmployeeSettingType = e.EmployeeSettingType,
                DataType = (SettingDataType)e.DataType,
                ValidFromDate = e.ValidFromDate,
                ValidToDate = e.ValidToDate,
                Name = e.Name,
                StrData = e.StrData,
                IntData = e.IntData,
                DecimalData = e.DecimalData,
                BoolData = e.BoolData,
                DateData = e.DateData,
                TimeData = e.TimeData,
                State = (SoeEntityState)e.State
            };
        }

        public static List<EmployeeSettingDTO> ToDTOs(this IEnumerable<EmployeeSetting> l)
        {
            var dtos = new List<EmployeeSettingDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        /// <summary>
        /// Why do we need this
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        public static List<EmployeeSetting> DistinctByPK(this IEnumerable<EmployeeSetting> l)
        {
            return l?
                .GroupBy(e => e.EmployeeSettingId)
                .Select(g => g.First())
                .ToList() ?? new List<EmployeeSetting>();
        }

        public static List<EmployeeSetting> FilterByEmployee(this IEnumerable<EmployeeSetting> l, int employeeId)
        {
            return l?
                .Where(e => e.EmployeeId == employeeId)
                .ToList() ?? new List<EmployeeSetting>();       
        }

        public static List<EmployeeSetting> FilterByDate(this IEnumerable<EmployeeSetting> l, DateTime date)
        {
            return l?
                .Where(e => 
                    (!e.ValidFromDate.HasValue || e.ValidFromDate.Value <= date) &&
                    (!e.ValidToDate.HasValue || e.ValidToDate.Value >= date))
                .OrderByDescending(e => e.ValidToDate)
                .ToList() ?? new List<EmployeeSetting>();
        }

        public static List<EmployeeSetting> FilterByDates(this IEnumerable<EmployeeSetting> l, DateTime fromDate, DateTime toDate)
        {
            return l?
                .Where(e => 
                    (!e.ValidFromDate.HasValue || e.ValidFromDate.Value <= toDate) &&
                    (!e.ValidToDate.HasValue || e.ValidToDate.Value >= fromDate))
                .ToList() ?? new List<EmployeeSetting>();
        }

        public static List<EmployeeSetting> FilterByTypes(this IEnumerable<EmployeeSetting> l, TermGroup_EmployeeSettingType? area, TermGroup_EmployeeSettingType? group = null, TermGroup_EmployeeSettingType? type = null)
        {
            return l?
                .Where(e => 
                    (!area.HasValue || e.EmployeeSettingAreaType == (int)area.Value) && 
                    (!group.HasValue || e.EmployeeSettingGroupType == (int)group.Value) && 
                    (!type.HasValue || e.EmployeeSettingType == (int)type.Value))
                .ToList() ?? new List<EmployeeSetting>();
        }

        public static List<EmployeeSetting> GetSettingsByDatesSortedByEmployeeAndType(this List<EmployeeSetting> l, DateTime startDate, DateTime stopDate)
        {
            var settings = new List<EmployeeSetting>();
            if (!l.IsNullOrEmpty())
            {
                foreach (var employeeSettingsByEmployee in l.GroupBy(setting => setting.EmployeeId))
                {
                    foreach (var employeeSettingsByEmployeeAndType in employeeSettingsByEmployee.GroupBy(setting => setting.EmployeeSettingType))
                    {
                        var setting = employeeSettingsByEmployeeAndType.FilterByDates(startDate, stopDate).FirstOrDefault();
                        if (setting != null)
                            settings.Add(setting);
                    }
                }
            }
            return settings;
        }

        public static EmployeeSetting GetSetting(this List<EmployeeSetting> l, int employeeId, DateTime date)
        {
            return l?
                .FilterByEmployee(employeeId)
                .FilterByDate(date)
                .FirstOrDefault();
        }

        public static EmployeeSetting GetSetting(this List<EmployeeSetting> l, int employeeId, DateTime date, TermGroup_EmployeeSettingType area, TermGroup_EmployeeSettingType group, TermGroup_EmployeeSettingType type)
        {
            return l?
                .FilterByEmployee(employeeId)
                .FilterByDate(date)
                .FilterByTypes(area, group, type)
                .FirstOrDefault();
        }

        public static string GetEmployeeSettingValueAsString(this EmployeeSetting e)
        {
            switch (e.DataType)
            {
                case (int)SettingDataType.String:
                    return e.StrData;
                case (int)SettingDataType.Integer:
                    return e.IntData.ToString();
                case (int)SettingDataType.Decimal:
                    return e.DecimalData.ToString();
                case (int)SettingDataType.Boolean:
                    return e.BoolData.ToString();
                case (int)SettingDataType.Date:
                    return e.DateData?.ToString("yyyy-MM-dd");
                case (int)SettingDataType.Time:
                    return e.TimeData?.ToString("HH:mm:ss");
                default:
                    return null;
            }
        }

        #endregion
    }
}
