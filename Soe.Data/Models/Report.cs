using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SoftOne.Soe.Data
{
    [DebuggerDisplay("NumberAndName = {ReportNr}/{Name}")]
    public partial class Report : ICreatedModified, IState
    {
        public string ReportSelectionText { get; set; }
        public string NameWithReportSelectionText { get; set; }
        public int? SysReportTemplateTypeId { get; set; }
        public int? SysReportTemplateTypeSelectionType { get; set; }
        public bool? SysReportTemplateTypeGroupMapping { get; set; }
        public int? SysReportTemplateTypeModule { get; set; }
        public bool IsSystemReport { get; set; }

        public string RoleNames { get; set; }
        public string SysReportTypeName { get; set; }
        public string ExportTypeName { get; set; }
        public List<SysReportTemplateSetting> ReportTemplateSettings { get; set; }

        public bool ValidForScheduledJob
        {
            get
            {
                return this.SysReportTemplateTypeId == (int)SoeReportTemplateType.TimeAbsenceReport ||
                 this.SysReportTemplateTypeId == (int)SoeReportTemplateType.TimeAccumulatorDetailedReport ||
                 this.SysReportTemplateTypeId == (int)SoeReportTemplateType.TimeAccumulatorReport ||
                 this.SysReportTemplateTypeId == (int)SoeReportTemplateType.TimeCategorySchedule ||
                 this.SysReportTemplateTypeId == (int)SoeReportTemplateType.TimeCategoryStatistics ||
                 this.SysReportTemplateTypeId == (int)SoeReportTemplateType.TimeEmployeeLineSchedule ||
                 this.SysReportTemplateTypeId == (int)SoeReportTemplateType.TimeEmployeeSchedule ||
                 this.SysReportTemplateTypeId == (int)SoeReportTemplateType.TimeEmployeeScheduleSmallReport ||
                 this.SysReportTemplateTypeId == (int)SoeReportTemplateType.TimeEmployeeTemplateSchedule ||
                 this.SysReportTemplateTypeId == (int)SoeReportTemplateType.TimeMonthlyReport;
            }
        }
    }

    public static partial class EntityExtensions
    {
        #region Report

        public static IEnumerable<ReportViewDTO> ToReportViewDTOs(this IEnumerable<Report> l)
        {
            var dtos = new List<ReportViewDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToReportViewDTO());
                }
            }
            return dtos;
        }

        public static ReportViewDTO ToReportViewDTO(this Report e)
        {
            return new ReportViewDTO()
            {
                ActorCompanyId = e.ActorCompanyId,
                ReportId = e.ReportId,
                ReportName = e.Name,
                ReportDescription = e.Description,
                ReportNr = e.ReportNr,
                ExportType = e.ExportType,
                ReportSelectionId = e.ReportSelectionId,
                SysReportTemplateTypeId = e.SysReportTemplateTypeId ?? 0,
            };
        }

        public static IEnumerable<ReportViewGridDTO> ToReportViewGridDTOs(this IEnumerable<Report> l)
        {
            var dtos = new List<ReportViewGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToReportViewGridDTO());
                }
            }
            return dtos;
        }

        public static ReportViewGridDTO ToReportViewGridDTO(this Report e)
        {
            return new ReportViewGridDTO()
            {
                ActorCompanyId = e.ActorCompanyId,
                ReportId = e.ReportId,
                ReportNr = e.ReportNr,
                ReportName = e.Name,
                ReportDescription = e.Description,
                Original = e.Original,
                Standard = e.Standard,
                SelectionType = (SoeSelectionType)(e.SysReportTemplateTypeSelectionType ?? 0),
                SysReportTemplateTypeId = e.SysReportTemplateTypeId ?? 0,
                SysReportTypeName = e.SysReportTypeName,
                ReportSelectionId = e.ReportSelectionId,
                ReportSelectionText = e.ReportSelectionText,
                ExportType = e.ExportType,
                ExportTypeName = e.ExportTypeName,
                RoleNames = e.RoleNames,
                IsMigrated = e.SysReportTemplateTypeId.HasValue && ((SoeReportTemplateType)e.SysReportTemplateTypeId.Value).IsReportMigrated(),
                IsSystemReport = e.IsSystemReport
            };
        }

        public static ReportDTO ToDTO(this Report e)
        {
            if (e == null)
                return null;

            var dto = new ReportDTO
            {
                ReportId = e.ReportId,
                ActorCompanyId = e.ActorCompanyId,
                ReportTemplateId = e.ReportTemplateId,
                ReportSelectionId = e.ReportSelectionId,
                Module = (SoeModule)e.Module,
                ExportType = (TermGroup_ReportExportType)e.ExportType,
                IncludeAllHistoricalData = e.IncludeAllHistoricalData,
                IncludeBudget = e.IncludeBudget,
                DetailedInformation = e.GetDetailedInformation,
                NoOfYearsBackinPreviousYear = e.NoOfYearsBackinPreviousYear,
                NrOfDecimals = e.NrOfDecimals,
                ShowRowsByAccount = e.ShowRowsByAccount,
                ReportNr = e.ReportNr,
                Name = e.Name,
                Description = e.Description,
                Standard = e.Standard,
                Original = e.Original,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                ExportFileType = (TermGroup_ReportExportFileType)e.FileType,
                ShowInAccountingReports = e.ShowInAccountingReports,
                SortByLevel1 = (TermGroup_ReportGroupAndSortingTypes)e.SortByLevel1,
                SortByLevel2 = (TermGroup_ReportGroupAndSortingTypes)e.SortByLevel2,
                SortByLevel3 = (TermGroup_ReportGroupAndSortingTypes)e.SortByLevel3,
                SortByLevel4 = (TermGroup_ReportGroupAndSortingTypes)e.SortByLevel4,
                GroupByLevel1 = (TermGroup_ReportGroupAndSortingTypes)e.GroupByLevel1,
                GroupByLevel2 = (TermGroup_ReportGroupAndSortingTypes)e.GroupByLevel2,
                GroupByLevel3 = (TermGroup_ReportGroupAndSortingTypes)e.GroupByLevel3,
                GroupByLevel4 = (TermGroup_ReportGroupAndSortingTypes)e.GroupByLevel4,
                Settings = e.ReportSetting.ToDTOs().ToList(),
                IsSortAscending = e.IsSortAscending,
                Special = e.Special
            };

            // Extensions
            dto.SysReportTemplateTypeId = e.SysReportTemplateTypeId;
            dto.SysReportTemplateTypeSelectionType = e.SysReportTemplateTypeSelectionType;
            if (e.ReportSelection != null)
            {
                dto.ReportSelectionText = e.ReportSelection.ReportSelectionText;
                if (e.ReportSelection.ReportSelectionDate != null)
                    dto.ReportSelectionDate = e.ReportSelection.ReportSelectionDate.ToDTOs().ToList();
                if (e.ReportSelection.ReportSelectionInt != null)
                    dto.ReportSelectionInt = e.ReportSelection.ReportSelectionInt.ToDTOs().ToList();
                if (e.ReportSelection.ReportSelectionStr != null)
                    dto.ReportSelectionStr = e.ReportSelection.ReportSelectionStr.ToDTOs().ToList();
            }

            if (e.ReportRolePermission != null)
            {
                dto.RoleIds = e.ReportRolePermission.Where(r => r.State == (int)SoeEntityState.Active).Select(r => r.RoleId).ToList();
            }

            if (e.ReportTemplateSettings != null)
            {
                dto.ReportTemplateSettings = e.ReportTemplateSettings.Select(s => s.ToDTO()).ToList();
            }

            return dto;
        }

        public static IEnumerable<ReportDTO> ToDTOs(this IEnumerable<Report> l)
        {
            var dtos = new List<ReportDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static ReportSmallDTO ToSmallDTO(this Report e)
        {
            if (e == null)
                return null;

            return new ReportSmallDTO()
            {
                ReportId = e.ReportId,
                ReportNr = e.ReportNr,
                Name = e.Name,
            };
        }

        public static IEnumerable<ReportSmallDTO> ToSmallDTOs(this IEnumerable<Report> l)
        {
            var dtos = new List<ReportSmallDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToSmallDTO());
                }
            }
            return dtos;
        }

        public static bool IsValid(this Report e, bool onlyOriginal, bool onlyStandard, bool onlyWithSelections = false)
        {
            if (e == null)
                return false;

            if (onlyWithSelections)
            {
                if (e.ReportSelectionId == null)
                    return false;
            }
            else
            {
                if (onlyOriginal && !e.Original)
                    return false;
                if (onlyStandard && !e.Standard)
                    return false;
            }

            return true;
        }

        #endregion

        #region Report setting

        public static ReportSettingDTO ToDTO(this ReportSetting e)
        {
            if (e == null)
                return null;

            return new ReportSettingDTO()
            {
                ReportSettingId = e.ReportSettingId,
                ReportId = e.ReportId,
                DataTypeId = (SettingDataType)e.DataTypeId,
                IntData = e.IntData,
                StrData = e.StrData,
                BoolData = e.BoolData,
                Value = e.Value,
                Type = (TermGroup_ReportSettingType)e.Type,
            };
        }

        public static IEnumerable<ReportSettingDTO> ToDTOs(this IEnumerable<ReportSetting> l)
        {
            var dtos = new List<ReportSettingDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion
    }
}
