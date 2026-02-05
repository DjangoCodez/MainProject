using SoftOne.Soe.Business.Core.Reporting.Matrix.Models;
using SoftOne.Soe.Business.Core.Reporting.Matrix.Models.Generic;
using SoftOne.Soe.Business.Core.Reporting.Models;
using SoftOne.Soe.Business.Core.Reporting.Models.Bridge;
using SoftOne.Soe.Business.Core.Reporting.Models.Time;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix
{
    public class TimeMatrixDataManager : BaseMatrixDataManager
    {
        #region Ctor

        public TimeMatrixDataManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Common

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns(SoeReportTemplateType reportTemplateType)
        {
            InputMatrix inputMatrix;

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var extraFields = base.GetExtraFieldsFromCache(entities, CacheConfig.Company(ActorCompanyId, 60));
            var accountInternals = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(ActorCompanyId));

            switch (reportTemplateType)
            {
                case SoeReportTemplateType.TimeTransactionAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.TimeTransactionMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals);
                    return new TimeTransactionMatrix(inputMatrix, null).GetMatrixLayoutColumns();
                case SoeReportTemplateType.PayrollTransactionAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.PayrollTransactionMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals);
                    return new PayrollTransactionMatrix(inputMatrix, null).GetMatrixLayoutColumns();
                case SoeReportTemplateType.EmployeeAnalysis:
                    var extraFieldsEmployee = extraFields?.Where(w => w.Entity == SoeEntityType.Employee).ToList();
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmployeeListMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals, extraFieldsEmployee);
                    return new EmployeeListMatrix(inputMatrix, null, ActorCompanyId).GetMatrixLayoutColumns();
                case SoeReportTemplateType.ScheduleAnalysis:
                    var extraFieldsEmployeeScheduleTransactions = extraFields?.Where(w => w.Entity == SoeEntityType.Employee).ToList();
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.ScheduleTransactionMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals, extraFieldsEmployeeScheduleTransactions);
                    return new ScheduleTransactionMatrix(inputMatrix, null).GetMatrixLayoutColumns();
                case SoeReportTemplateType.TimeScheduledSummary:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.ScheduleTransactionMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals);
                    return new ScheduledTimeSummaryMatrix(inputMatrix, null).GetMatrixLayoutColumns();
                case SoeReportTemplateType.EmployeeDateAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmployeeDateMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals);
                    return new EmployeeDateMatrix(inputMatrix, null).GetMatrixLayoutColumns();
                case SoeReportTemplateType.TimeStampEntryAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.TimeStampEntryMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals);
                    return new TimeStampEntryMatrix(inputMatrix, null).GetMatrixLayoutColumns();
                case SoeReportTemplateType.StaffingneedsFrequencyAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.StaffingneedsFrequencyMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals);
                    return new StaffingneedsFrequencyMatrix(inputMatrix, null).GetMatrixLayoutColumns();
                case SoeReportTemplateType.EmployeeSkillAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmployeeSkillMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals);
                    return new EmployeeSkillMatrix(inputMatrix, null, ActorCompanyId).GetMatrixLayoutColumns();
                case SoeReportTemplateType.ShiftTypeSkillAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.ShiftTypeSkillMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals);
                    return new ShiftTypeSkillMatrix(inputMatrix, null).GetMatrixLayoutColumns();
                case SoeReportTemplateType.EmployeeEndReasonsAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmployeeEndReasonsMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals);
                    return new EmployeeEndReasonsMatrix(inputMatrix, null).GetMatrixLayoutColumns();
                case SoeReportTemplateType.EmployeeSalaryAnalysis:
                    var extraFieldsEmployeeSalary = extraFields?.Where(w => w.Entity == SoeEntityType.Employee).ToList();
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmployeeSalaryMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals, extraFieldsEmployeeSalary);
                    return new EmployeeSalaryMatrix(inputMatrix, null, ActorCompanyId).GetMatrixLayoutColumns();
                case SoeReportTemplateType.EmployeeTimePeriodAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmployeeTimePeriodMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals);
                    return new EmployeeTimePeriodMatrix(inputMatrix, null).GetMatrixLayoutColumns();
                case SoeReportTemplateType.StaffingStatisticsAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.StaffingStatisticsMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals);
                    return new StaffingStatisticsMatrix(inputMatrix, null, ActorCompanyId).GetMatrixLayoutColumns();
                case SoeReportTemplateType.AggregatedTimeStatisticsAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.AggregatedTimeStatisticsMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals);
                    return new AggregatedTimeStatisticsMatrix(inputMatrix, null).GetMatrixLayoutColumns();
                case SoeReportTemplateType.EmployeeMeetingAnalysis:
                    var extraFieldsEmployeeMeeting = extraFields?.Where(w => w.Entity == SoeEntityType.Employee).ToList();
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmployeeMeetingMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals, extraFieldsEmployeeMeeting);
                    return new EmployeeMeetingMatrix(inputMatrix, null, ActorCompanyId).GetMatrixLayoutColumns();
                case SoeReportTemplateType.EmployeeExperienceAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmployeeExperienceMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals);
                    return new EmployeeExperienceMatrix(inputMatrix, null).GetMatrixLayoutColumns();
                case SoeReportTemplateType.EmployeeDocumentAnalysis:
                    var extraFieldsEmployeeDocument = extraFields?.Where(w => w.Entity == SoeEntityType.Employee).ToList();
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmployeeDocumentMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals, extraFieldsEmployeeDocument);
                    return new EmployeeDocumentMatrix(inputMatrix, null, ActorCompanyId).GetMatrixLayoutColumns();
                case SoeReportTemplateType.EmployeeAccountAnalysis:
                    var extraFieldsEmployeeForEA = extraFields?.Where(w => w.Entity == SoeEntityType.Employee).ToList();
                    int defaultEmployeeAccountDimId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.DefaultEmployeeAccountDimEmployee, 0, ActorCompanyId, 0);
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmployeeAccountMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals, extraFieldsEmployeeForEA);
                    return new EmployeeAccountMatrix(inputMatrix, null, ActorCompanyId, defaultEmployeeAccountDimId).GetMatrixLayoutColumns();
                case SoeReportTemplateType.EmployeeFixedPayLinesAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmployeeFixedPayLinesMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals);
                    return new EmployeeFixedPayLinesMatrix(inputMatrix, null).GetMatrixLayoutColumns();
                case SoeReportTemplateType.PayrollProductsAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.PayrollProductsMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals);
                    return new PayrollProductsMatrix(inputMatrix, null).GetMatrixLayoutColumns();
                case SoeReportTemplateType.EmployeeSalaryDistressAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmployeeSalaryDistressMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals);
                    return new EmployeeSalaryDistressMatrix(inputMatrix, null).GetMatrixLayoutColumns();
                case SoeReportTemplateType.EmployeeSalaryUnionFeesAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmployeeSalaryUnionFeesMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals);
                    return new EmployeeSalaryUnionFeesMatrix(inputMatrix, null).GetMatrixLayoutColumns();
                case SoeReportTemplateType.EmploymentDaysAnalysis:
                    List<EmploymentTypeDTO> employmentTypes = EmployeeManager.GetEmploymentTypes(ActorCompanyId);
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmploymentDaysMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals);
                    return new EmploymentDaysMatrix(inputMatrix, null, employmentTypes).GetMatrixLayoutColumns();
                case SoeReportTemplateType.AnnualProgressAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.AnnualProgressMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null);
                    return new AnnualProgressMatrix(inputMatrix, null, ActorCompanyId).GetMatrixLayoutColumns();
                case SoeReportTemplateType.LongtermAbsenceAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.LongtermAbsenceMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null);
                    return new LongtermAbsenceMatrix(inputMatrix, null, ActorCompanyId).GetMatrixLayoutColumns();
                case SoeReportTemplateType.VacationBalanceAnalysis:
                    var extraFieldsEmployeeVacation = extraFields?.Where(w => w.Entity == SoeEntityType.Employee).ToList();
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.VacationBalanceMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals, extraFieldsEmployeeVacation);
                    return new VacationBalanceMatrix(inputMatrix, null, ActorCompanyId).GetMatrixLayoutColumns();

                case SoeReportTemplateType.ShiftQueueAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.ShiftQueueMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null);
                    return new ShiftQueueMatrix(inputMatrix, null).GetMatrixLayoutColumns();
                case SoeReportTemplateType.ShiftHistoryAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.ShiftHistoryMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null);
                    return new ShiftHistoryMatrix(inputMatrix, null).GetMatrixLayoutColumns();
                case SoeReportTemplateType.ShiftRequestAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.ShiftRequestMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null);
                    return new ShiftRequestMatrix(inputMatrix, null, ActorCompanyId).GetMatrixLayoutColumns();
                case SoeReportTemplateType.AbsenceRequestAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.AbsenceRequestMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null);
                    return new AbsenceRequestMatrix(inputMatrix, null).GetMatrixLayoutColumns();

                case SoeReportTemplateType.VismaPayrollChangesAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.VismaPayrollChangesMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null);
                    return new VismaPayrollChangesMatrix(inputMatrix, null, ActorCompanyId).GetMatrixLayoutColumns();
                case SoeReportTemplateType.TimeStampHistoryAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.TimeStampHistoryMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null);
                    return new TimeStampHistoryMatrix(inputMatrix, null).GetMatrixLayoutColumns();
                case SoeReportTemplateType.VerticalTimeTrackerAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.VerticalTimeTrackerMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null);
                    return new VerticalTimeTrackerMatrix(inputMatrix, null).GetMatrixLayoutColumns();
                case SoeReportTemplateType.HorizontalTimeTrackerAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.HorizontalTimeTrackerMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null);
                    return new HorizontalTimeTrackerMatrix(inputMatrix, null).GetMatrixLayoutColumns();
                case SoeReportTemplateType.AgiAbsenceAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.AgiAbsenceMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null);
                    return new AgiAbsenceMatrix(inputMatrix, null).GetMatrixLayoutColumns();
                case SoeReportTemplateType.EmployeeChildAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmployeeChildMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null);
                    return new EmployeeChildMatrix(inputMatrix, null).GetMatrixLayoutColumns();
                case SoeReportTemplateType.EmployeePayrollAdditionsAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmployeePayrollAdditionsMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null);
                    return new EmployeePayrollAdditionsMatrix(inputMatrix, null).GetMatrixLayoutColumns();
                case SoeReportTemplateType.AnnualLeaveTransactionAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.AnnualLeaveTransactionMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null);
                    return new AnnualLeaveTransactionMatrix(inputMatrix, null).GetMatrixLayoutColumns();
                case SoeReportTemplateType.SwapShiftAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.SwapShiftMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null);
                    return new SwapShiftMatrix(inputMatrix, null).GetMatrixLayoutColumns();
            }

            return new List<MatrixLayoutColumn>();
        }

        public List<MatrixDefinitionColumn> GetMatrixDefinitionColumns(SoeReportTemplateType reportTemplateType)
        {
            InputMatrix inputMatrix;

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var extraFields = base.GetExtraFieldsFromCache(entities, CacheConfig.Company(ActorCompanyId, 60));
            var accountInternals = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(ActorCompanyId));

            switch (reportTemplateType)
            {
                case SoeReportTemplateType.TimeTransactionAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.TimeTransactionMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals);
                    return new TimeTransactionMatrix(inputMatrix, null).GetMatrixDefinitionColumns();
                case SoeReportTemplateType.PayrollTransactionAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.PayrollTransactionMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals);
                    return new PayrollTransactionMatrix(inputMatrix, null).GetMatrixDefinitionColumns();
                case SoeReportTemplateType.EmployeeAnalysis:
                    var extraFieldsEmployee = extraFields?.Where(w => w.Entity == SoeEntityType.Employee).ToList();
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmployeeListMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals, extraFieldsEmployee);
                    return new EmployeeListMatrix(inputMatrix, null, ActorCompanyId).GetMatrixDefinitionColumns();
                case SoeReportTemplateType.ScheduleAnalysis:
                    var extraFieldsEmployeeScheduleTransactions = extraFields?.Where(w => w.Entity == SoeEntityType.Employee).ToList();
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.ScheduleTransactionMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals, extraFieldsEmployeeScheduleTransactions);
                    return new ScheduleTransactionMatrix(inputMatrix, null).GetMatrixDefinitionColumns();
                case SoeReportTemplateType.TimeScheduledSummary:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.ScheduleTransactionMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals);
                    return new ScheduledTimeSummaryMatrix(inputMatrix, null).GetMatrixDefinitionColumns();
                case SoeReportTemplateType.EmployeeDateAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmployeeDateMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals);
                    return new EmployeeDateMatrix(inputMatrix, null).GetMatrixDefinitionColumns();
                case SoeReportTemplateType.TimeStampEntryAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.TimeStampEntryMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals);
                    return new TimeStampEntryMatrix(inputMatrix, null).GetMatrixDefinitionColumns();
                case SoeReportTemplateType.StaffingneedsFrequencyAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.StaffingneedsFrequencyMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals);
                    return new StaffingneedsFrequencyMatrix(inputMatrix, null).GetMatrixDefinitionColumns();
                case SoeReportTemplateType.EmployeeSkillAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmployeeSkillMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals);
                    return new EmployeeSkillMatrix(inputMatrix, null, ActorCompanyId).GetMatrixDefinitionColumns();
                case SoeReportTemplateType.ShiftTypeSkillAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.ShiftTypeSkillMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals);
                    return new ShiftTypeSkillMatrix(inputMatrix, null).GetMatrixDefinitionColumns();
                case SoeReportTemplateType.EmployeeEndReasonsAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmployeeEndReasonsMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals);
                    return new EmployeeEndReasonsMatrix(inputMatrix, null).GetMatrixDefinitionColumns();
                case SoeReportTemplateType.EmployeeSalaryAnalysis:
                    var extraFieldsEmployeeSalary = extraFields?.Where(w => w.Entity == SoeEntityType.Employee).ToList();
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmployeeSalaryMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals, extraFieldsEmployeeSalary);
                    return new EmployeeSalaryMatrix(inputMatrix, null, ActorCompanyId).GetMatrixDefinitionColumns();
                case SoeReportTemplateType.EmployeeTimePeriodAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmployeeTimePeriodMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals);
                    return new EmployeeTimePeriodMatrix(inputMatrix, null).GetMatrixDefinitionColumns();
                case SoeReportTemplateType.StaffingStatisticsAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.StaffingStatisticsMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals);
                    return new StaffingStatisticsMatrix(inputMatrix, null, ActorCompanyId).GetMatrixDefinitionColumns();
                case SoeReportTemplateType.AggregatedTimeStatisticsAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.AggregatedTimeStatisticsMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals);
                    return new AggregatedTimeStatisticsMatrix(inputMatrix, null).GetMatrixDefinitionColumns();
                case SoeReportTemplateType.EmployeeMeetingAnalysis:
                    var extraFieldsEmployeeMeeting = extraFields?.Where(w => w.Entity == SoeEntityType.Employee).ToList();
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmployeeMeetingMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals, extraFieldsEmployeeMeeting);
                    return new EmployeeMeetingMatrix(inputMatrix, null, ActorCompanyId).GetMatrixDefinitionColumns();
                case SoeReportTemplateType.EmployeeExperienceAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmployeeExperienceMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals);
                    return new EmployeeExperienceMatrix(inputMatrix, null).GetMatrixDefinitionColumns();
                case SoeReportTemplateType.EmployeeDocumentAnalysis:
                    var extraFieldsEmployeeDocument = extraFields?.Where(w => w.Entity == SoeEntityType.Employee).ToList();
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmployeeDocumentMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals, extraFieldsEmployeeDocument);
                    return new EmployeeDocumentMatrix(inputMatrix, null, ActorCompanyId).GetMatrixDefinitionColumns();
                case SoeReportTemplateType.EmployeeAccountAnalysis:
                    int defaultEmployeeAccountDimId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.DefaultEmployeeAccountDimEmployee, 0, ActorCompanyId, 0);
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmployeeAccountMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals);
                    return new EmployeeAccountMatrix(inputMatrix, null, ActorCompanyId, defaultEmployeeAccountDimId).GetMatrixDefinitionColumns();
                case SoeReportTemplateType.EmployeeFixedPayLinesAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmployeeFixedPayLinesMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals);
                    return new EmployeeFixedPayLinesMatrix(inputMatrix, null).GetMatrixDefinitionColumns();
                case SoeReportTemplateType.PayrollProductsAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.PayrollProductsMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals);
                    return new PayrollProductsMatrix(inputMatrix, null).GetMatrixDefinitionColumns();
                case SoeReportTemplateType.EmployeeSalaryDistressAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmployeeSalaryDistressMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals);
                    return new EmployeeSalaryDistressMatrix(inputMatrix, null).GetMatrixDefinitionColumns();
                case SoeReportTemplateType.EmployeeSalaryUnionFeesAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmployeeSalaryUnionFeesMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals);
                    return new EmployeeSalaryUnionFeesMatrix(inputMatrix, null).GetMatrixDefinitionColumns();
                case SoeReportTemplateType.AccountHierachyAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmployeeSalaryUnionFeesMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals);
                    return new AccountHierarchyMatrix(inputMatrix, null, ActorCompanyId).GetMatrixDefinitionColumns();
                case SoeReportTemplateType.AnnualProgressAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.AnnualProgressMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null);
                    return new AnnualProgressMatrix(inputMatrix, null, ActorCompanyId).GetMatrixDefinitionColumns();
                case SoeReportTemplateType.LongtermAbsenceAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.LongtermAbsenceMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null);
                    return new LongtermAbsenceMatrix(inputMatrix, null, ActorCompanyId).GetMatrixDefinitionColumns();
                case SoeReportTemplateType.EmploymentDaysAnalysis:
                    List<EmploymentTypeDTO> employmentTypes = EmployeeManager.GetEmploymentTypes(ActorCompanyId);
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmploymentDaysMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals);
                    return new EmploymentDaysMatrix(inputMatrix, null, employmentTypes).GetMatrixDefinitionColumns();
                case SoeReportTemplateType.VacationBalanceAnalysis:
                    var extraFieldsEmployeeVacation = extraFields?.Where(w => w.Entity == SoeEntityType.Employee).ToList();
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.VacationBalanceMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals, extraFieldsEmployeeVacation);
                    return new VacationBalanceMatrix(inputMatrix, null, ActorCompanyId).GetMatrixDefinitionColumns();
                case SoeReportTemplateType.ShiftQueueAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.ShiftQueueMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null);
                    return new ShiftQueueMatrix(inputMatrix, null).GetMatrixDefinitionColumns();
                case SoeReportTemplateType.ShiftHistoryAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.ShiftHistoryMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null);
                    return new ShiftHistoryMatrix(inputMatrix, null).GetMatrixDefinitionColumns();
                case SoeReportTemplateType.ShiftRequestAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.ShiftRequestMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null);
                    return new ShiftRequestMatrix(inputMatrix, null, ActorCompanyId).GetMatrixDefinitionColumns();
                case SoeReportTemplateType.AbsenceRequestAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.AbsenceRequestMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null);
                    return new AbsenceRequestMatrix(inputMatrix, null).GetMatrixDefinitionColumns();
                case SoeReportTemplateType.TimeStampHistoryAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.TimeStampHistoryMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals);
                    return new TimeStampHistoryMatrix(inputMatrix, null).GetMatrixDefinitionColumns();
                case SoeReportTemplateType.VerticalTimeTrackerAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.VerticalTimeTrackerMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null);
                    return new VerticalTimeTrackerMatrix(inputMatrix, null).GetMatrixDefinitionColumns();
                case SoeReportTemplateType.HorizontalTimeTrackerAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.HorizontalTimeTrackerMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null);
                    return new HorizontalTimeTrackerMatrix(inputMatrix, null).GetMatrixDefinitionColumns();
                case SoeReportTemplateType.AnnualLeaveTransactionAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.AnnualLeaveTransactionMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null);
                    return new HorizontalTimeTrackerMatrix(inputMatrix, null).GetMatrixDefinitionColumns();
            }

            return new List<MatrixDefinitionColumn>();
        }

        public List<MatrixColumnSelectionDTO> GetMatrixColumnSelectionDTOs(SoeReportTemplateType reportTemplateType, ExportDefinitionLevel exportDefinitionlevel)
        {
            var filterFields = exportDefinitionlevel.ExportDefinitionLevelColumn.Where(w => !string.IsNullOrEmpty(w.Key)).Select(s => s.Key).Distinct().ToList();

            var fixedList = new List<Tuple<string, string>>();

            foreach (var f in filterFields)
            {
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                fixedList.Add(ExtraFieldAndAccountDims(f, base.ActorCompanyId, base.GetExtraFieldsFromCache(entitiesReadOnly, CacheConfig.Company(base.ActorCompanyId)), base.GetAccountDimsFromCache(entitiesReadOnly, CacheConfig.Company(base.ActorCompanyId))));
            }

            return GetMatrixColumnSelectionDTOs(reportTemplateType, fixedList);
        }

        public List<MatrixColumnSelectionDTO> GetMatrixColumnSelectionDTOs(SoeReportTemplateType reportTemplateType, List<Tuple<string, string>> filterFields = null)
        {
            var columns = GetMatrixDefinitionColumns(reportTemplateType);
            List<MatrixColumnSelectionDTO> selectionDTOs = new List<MatrixColumnSelectionDTO>();
            int sort = 1;
            foreach (var col in columns)
            {
                if (!filterFields.IsNullOrEmpty())
                {
                    if (!filterFields.Any(s => s.Item1.ToLower().Contains(col.Field.ToLower())))
                        continue;
                }

                var matchingFilterField = filterFields.FirstOrDefault(f => f.Item1.Equals(col.Field, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrEmpty(matchingFilterField?.Item2) && string.IsNullOrEmpty(col.Options?.Key))
                {
                    if (col.Options == null)
                        col.Options = new MatrixDefinitionColumnOptions();

                    col.Options.Key = matchingFilterField.Item2;
                }

                selectionDTOs.Add(new MatrixColumnSelectionDTO()
                {
                    Field = col.Field,
                    Key = Guid.NewGuid().ToString(),
                    Sort = sort,
                    Options = col.Options,
                    MatrixDataType = col.MatrixDataType,
                    TypeName = col.Title,

                });

                sort++;
            }

            return selectionDTOs;
        }

        public List<Insight> GetInsights(SoeReportTemplateType reportTemplateType, int actorCompanyId, int roleId)
        {
            List<Insight> insights = new List<Insight>();

            #region Prereq

            bool hasInsightsPermission = FeatureManager.HasRolePermission(Feature.Time_Insights, Permission.Readonly, roleId, actorCompanyId);
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, actorCompanyId);
            List<MatrixLayoutColumn> allColumns = GetMatrixLayoutColumns(reportTemplateType);

            #endregion

            #region Custom

            insights.Add(new Insight((int)TermGroup_FixedInsights.Custom, GetText((int)TermGroup_FixedInsights.Custom, (int)TermGroup.FixedInsights, "Egen"), !hasInsightsPermission, allColumns, Insight.GetAllChartTypes()));

            #endregion

            #region Fixed

            switch (reportTemplateType)
            {
                case SoeReportTemplateType.TimeTransactionAnalysis:
                    break;
                case SoeReportTemplateType.PayrollTransactionAnalysis:
                    break;
                case SoeReportTemplateType.EmployeeAnalysis:
                    #region EmployeeAnalysis

                    insights.Add(new Insight((int)TermGroup_FixedInsights.EmployeeList_EmploymentType, GetText((int)TermGroup_FixedInsights.EmployeeList_EmploymentType, (int)TermGroup.FixedInsights, "Anställningsform"), !hasInsightsPermission, GetFilteredPossibleColumns(allColumns, new List<TermGroup_EmployeeListMatrixColumns>() { TermGroup_EmployeeListMatrixColumns.EmploymentTypeName }), SimpleOneColumnChartTypes));

                    List<MatrixLayoutColumn> columns = GetFilteredPossibleColumns(allColumns, new List<TermGroup_EmployeeListMatrixColumns>() { TermGroup_EmployeeListMatrixColumns.WorkTimeWeekPercent }, true);
                    MatrixLayoutColumn column = columns.FirstOrDefault();
                    if (column != null)
                    {
                        column.Options.Decimals = 0;
                        column.Options.LabelPostValue = "%";
                    }
                    insights.Add(new Insight((int)TermGroup_FixedInsights.EmployeeList_WorkTimeWeekPercent, GetText((int)TermGroup_FixedInsights.EmployeeList_WorkTimeWeekPercent, (int)TermGroup.FixedInsights, "Sysselsättningsgrad"), !hasInsightsPermission, columns, SimpleOneColumnChartTypes));

                    insights.Add(new Insight((int)TermGroup_FixedInsights.EmployeeList_WorkTimeWeekMinutes, GetText((int)TermGroup_FixedInsights.EmployeeList_WorkTimeWeekMinutes, (int)TermGroup.FixedInsights, "Veckoarbetstid"), !hasInsightsPermission, GetFilteredPossibleColumns(allColumns, new List<TermGroup_EmployeeListMatrixColumns>() { TermGroup_EmployeeListMatrixColumns.WorkTimeWeekMinutes }), SimpleOneColumnChartTypes));

                    insights.Add(new Insight((int)TermGroup_FixedInsights.EmployeeList_Gender, GetText((int)TermGroup_FixedInsights.EmployeeList_Gender, (int)TermGroup.FixedInsights, "Fördelning kön"), false, GetFilteredPossibleColumns(allColumns, new List<TermGroup_EmployeeListMatrixColumns>() { TermGroup_EmployeeListMatrixColumns.Gender }), SimpleOneColumnChartTypes));

                    List<MatrixLayoutColumn> columnsAcc;
                    if (useAccountHierarchy)
                    {
                        var dimId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.DefaultEmployeeAccountDimEmployee, 0, actorCompanyId, 0);
                        columnsAcc = GetFilteredPossibleColumns(allColumns, new List<TermGroup_EmployeeListMatrixColumns>() { TermGroup_EmployeeListMatrixColumns.WorkTimeWeekPercent, TermGroup_EmployeeListMatrixColumns.AccountInternalNrs }, true, dimId.ToString());
                    }
                    else
                    {
                        columnsAcc = GetFilteredPossibleColumns(allColumns, new List<TermGroup_EmployeeListMatrixColumns>() { TermGroup_EmployeeListMatrixColumns.WorkTimeWeekPercent, TermGroup_EmployeeListMatrixColumns.CategoryName }, true);
                    }

                    foreach (MatrixLayoutColumn columnAcc in columnsAcc)
                    {
                        if (columnAcc != null)
                        {
                            if (columnAcc.Field != "workTimeWeekPercent")
                                columnAcc.Options.GroupBy = true;
                            else
                                columnAcc.Options.LabelPostValue = "%";
                        }
                    }
                    insights.Add(new Insight((int)TermGroup_FixedInsights.EmployeeList_WorkTimeWeekPercentByAccount, GetText((int)TermGroup_FixedInsights.EmployeeList_WorkTimeWeekPercentByAccount, (int)TermGroup.FixedInsights, "Sysselsättningsgrad baserad på tillhörighet"), false, columnsAcc, BarChartTypes, TermGroup_InsightChartTypes.Column));

                    #endregion
                    break;
                case SoeReportTemplateType.ScheduleAnalysis:
                    #region ScheduleAnalysis

                    insights.Add(new Insight((int)TermGroup_FixedInsights.Schedule_ShiftType, GetText((int)TermGroup_FixedInsights.Schedule_ShiftType, (int)TermGroup.FixedInsights, "Passtyp"), false, GetFilteredPossibleColumns(allColumns, new List<TermGroup_ScheduleTransactionMatrixColumns>() { TermGroup_ScheduleTransactionMatrixColumns.ShiftTypeName }), SimpleOneColumnChartTypes));
                    insights.Add(new Insight((int)TermGroup_FixedInsights.Schedule_NetHours, GetText((int)TermGroup_FixedInsights.Schedule_NetHours, (int)TermGroup.FixedInsights, "Passlängd"), !hasInsightsPermission, GetFilteredPossibleColumns(allColumns, new List<TermGroup_ScheduleTransactionMatrixColumns>() { TermGroup_ScheduleTransactionMatrixColumns.NetHoursString }), SimpleOneColumnChartTypes));

                    #endregion
                    break;
                case SoeReportTemplateType.EmployeeDateAnalysis:
                    #region EmployeeDateAnalysis

                    insights.Add(new Insight((int)TermGroup_FixedInsights.EmployeeDate_WorkTimeWeekPercent, GetText((int)TermGroup_FixedInsights.EmployeeDate_WorkTimeWeekPercent, (int)TermGroup.FixedInsights, "Sysselsättningsgrad över tid"), false, GetFilteredPossibleColumns(allColumns, new List<TermGroup_EmployeeDateMatrixColumns>() { TermGroup_EmployeeDateMatrixColumns.Date, TermGroup_EmployeeDateMatrixColumns.EmploymentPercent, TermGroup_EmployeeDateMatrixColumns.EmploymentFte }), AreaChartTypes));

                    #endregion
                    break;
                case SoeReportTemplateType.TimeStampEntryAnalysis:
                    #region TimeStampEntryAnalysis

                    insights.Add(new Insight((int)TermGroup_FixedInsights.TimeStamp_OriginType, GetText((int)TermGroup_FixedInsights.TimeStamp_OriginType, (int)TermGroup.FixedInsights, "Ursprungstyp"), false, GetFilteredPossibleColumns(allColumns, new List<TermGroup_TimeStampMatrixColumns>() { TermGroup_TimeStampMatrixColumns.OriginType }), SimpleOneColumnChartTypes));

                    #endregion
                    break;
                case SoeReportTemplateType.StaffingneedsFrequencyAnalysis:
                    break;
                case SoeReportTemplateType.EmployeeSkillAnalysis:
                    #region EmployeeSkillAnalysis

                    insights.Add(new Insight((int)TermGroup_FixedInsights.EmployeeSkill_Skill, GetText((int)TermGroup_FixedInsights.EmployeeSkill_Skill, (int)TermGroup.FixedInsights, "Kompetens"), false, GetFilteredPossibleColumns(allColumns, new List<TermGroup_EmployeeSkillMatrixColumns>() { TermGroup_EmployeeSkillMatrixColumns.SkillName }), SimpleOneColumnChartTypes));

                    List<MatrixLayoutColumn> columnsSkill = GetFilteredPossibleColumns(allColumns, new List<TermGroup_EmployeeSkillMatrixColumns>() { TermGroup_EmployeeSkillMatrixColumns.SkillName, TermGroup_EmployeeSkillMatrixColumns.Gender }, true);
                    foreach (MatrixLayoutColumn columnSkill in columnsSkill)
                    {
                        if (columnSkill != null) columnSkill.Options.GroupBy = true;
                    }

                    insights.Add(new Insight((int)TermGroup_FixedInsights.EmployeeSkill_GenderwithSkill, GetText((int)TermGroup_FixedInsights.EmployeeSkill_GenderwithSkill, (int)TermGroup.FixedInsights, "Kompetens baserat på kön"), !hasInsightsPermission, columnsSkill, BarChartTypes, TermGroup_InsightChartTypes.Column));

                    #endregion
                    break;
                case SoeReportTemplateType.ShiftTypeSkillAnalysis:
                    #region ShiftTypeSkillAnalysis

                    insights.Add(new Insight((int)TermGroup_FixedInsights.ShiftTypeSkill_Skill, GetText((int)TermGroup_FixedInsights.ShiftTypeSkill_Skill, (int)TermGroup.FixedInsights, "Kompetens"), false, GetFilteredPossibleColumns(allColumns, new List<TermGroup_ShiftTypeSkillMatrixColumns>() { TermGroup_ShiftTypeSkillMatrixColumns.Skill }), SimpleOneColumnChartTypes));
                    insights.Add(new Insight((int)TermGroup_FixedInsights.ShiftTypeSkill_ShiftTypeScheduleTypeName, GetText((int)TermGroup_FixedInsights.ShiftTypeSkill_ShiftTypeScheduleTypeName, (int)TermGroup.FixedInsights, "Schematyp"), !hasInsightsPermission, GetFilteredPossibleColumns(allColumns, new List<TermGroup_ShiftTypeSkillMatrixColumns>() { TermGroup_ShiftTypeSkillMatrixColumns.ShiftTypeScheduleTypeName }), SimpleOneColumnChartTypes));

                    #endregion
                    break;
                case SoeReportTemplateType.EmployeeEndReasonsAnalysis:
                    #region EmployeeEndReasonsAnalysis

                    insights.Add(new Insight((int)TermGroup_FixedInsights.EmployeeEndReasons_Endreason, GetText((int)TermGroup_FixedInsights.EmployeeEndReasons_Endreason, (int)TermGroup.FixedInsights, "Slutorsak"), false, GetFilteredPossibleColumns(allColumns, new List<TermGroup_EmployeeEndReasonsMatrixColumns>() { TermGroup_EmployeeEndReasonsMatrixColumns.EndReason }), SimpleOneColumnChartTypes));

                    List<MatrixLayoutColumn> columnsEndReasons = GetFilteredPossibleColumns(allColumns, new List<TermGroup_EmployeeEndReasonsMatrixColumns>() { TermGroup_EmployeeEndReasonsMatrixColumns.EndReason, TermGroup_EmployeeEndReasonsMatrixColumns.Gender }, true);
                    foreach (MatrixLayoutColumn columnEndReasons in columnsEndReasons)
                    {
                        if (columnEndReasons != null) columnEndReasons.Options.GroupBy = true;
                    }

                    insights.Add(new Insight((int)TermGroup_FixedInsights.EmployeeEndReasons_GenderwithEndreason, GetText((int)TermGroup_FixedInsights.EmployeeEndReasons_GenderwithEndreason, (int)TermGroup.FixedInsights, "Slutorsak baserat på kön"), !hasInsightsPermission, columnsEndReasons, BarChartTypes, TermGroup_InsightChartTypes.Column));

                    #endregion
                    break;
                case SoeReportTemplateType.EmployeeSalaryAnalysis:
                    #region EmployeeSalaryAnalysis

                    insights.Add(new Insight((int)TermGroup_FixedInsights.EmployeeSalary_PayrollType, GetText((int)TermGroup_FixedInsights.EmployeeSalary_PayrollType, (int)TermGroup.FixedInsights, "Lönetyp"), !hasInsightsPermission, GetFilteredPossibleColumns(allColumns, new List<TermGroup_EmployeeSalaryMatrixColumns>() { TermGroup_EmployeeSalaryMatrixColumns.SalaryType }), SimpleOneColumnChartTypes));

                    List<MatrixLayoutColumn> columnsSalary = GetFilteredPossibleColumns(allColumns, new List<TermGroup_EmployeeSalaryMatrixColumns>() { TermGroup_EmployeeSalaryMatrixColumns.SalaryType, TermGroup_EmployeeSalaryMatrixColumns.Gender }, true);
                    foreach (MatrixLayoutColumn columnSalary in columnsSalary)
                    {
                        if (columnSalary != null) columnSalary.Options.GroupBy = true;
                    }

                    insights.Add(new Insight((int)TermGroup_FixedInsights.EmployeeSalary_GenderWithPayrollType, GetText((int)TermGroup_FixedInsights.EmployeeSalary_GenderWithPayrollType, (int)TermGroup.FixedInsights, "Lönetyp fördelat på kön"), !hasInsightsPermission, columnsSalary, BarChartTypes, TermGroup_InsightChartTypes.Column));

                    #endregion
                    break;
                case SoeReportTemplateType.EmployeeTimePeriodAnalysis:
                    break;
                case SoeReportTemplateType.StaffingStatisticsAnalysis:
                    break;
                case SoeReportTemplateType.AggregatedTimeStatisticsAnalysis:
                    break;
                case SoeReportTemplateType.EmployeeMeetingAnalysis:
                    #region EmployeeMeetingAnalysis

                    insights.Add(new Insight((int)TermGroup_FixedInsights.EmployeeMeeting_MeetingType, GetText((int)TermGroup_FixedInsights.EmployeeMeeting_MeetingType, (int)TermGroup.FixedInsights, "Samtalstyp"), false, GetFilteredPossibleColumns(allColumns, new List<TermGroup_EmployeeMeetingMatrixColumns>() { TermGroup_EmployeeMeetingMatrixColumns.MeetingType }), SimpleOneColumnChartTypes));

                    List<MatrixLayoutColumn> columnsMeeting = GetFilteredPossibleColumns(allColumns, new List<TermGroup_EmployeeMeetingMatrixColumns>() { TermGroup_EmployeeMeetingMatrixColumns.MeetingType, TermGroup_EmployeeMeetingMatrixColumns.Gender }, true);
                    foreach (MatrixLayoutColumn columnMeeting in columnsMeeting)
                    {
                        if (columnMeeting != null) columnMeeting.Options.GroupBy = true;
                    }

                    insights.Add(new Insight((int)TermGroup_FixedInsights.EmployeeMeeting_GenderWithMeetingType, GetText((int)TermGroup_FixedInsights.EmployeeMeeting_GenderWithMeetingType, (int)TermGroup.FixedInsights, "Samtalstyp fördelat på kön"), !hasInsightsPermission, columnsMeeting, BarChartTypes, TermGroup_InsightChartTypes.Column));

                    #endregion
                    break;
                case SoeReportTemplateType.TimeScheduledSummary:
                    break;
                case SoeReportTemplateType.EmployeeExperienceAnalysis:
                    #region EmployeeExperienceAnalysis

                    insights.Add(new Insight((int)TermGroup_FixedInsights.EmployeeExperience_ExperienceTot, GetText((int)TermGroup_FixedInsights.EmployeeExperience_ExperienceTot, (int)TermGroup.FixedInsights, "Branschvana Totalt (Månader)"), false, GetFilteredPossibleColumns(allColumns, new List<TermGroup_EmployeeExperienceMatrixColumns>() { TermGroup_EmployeeExperienceMatrixColumns.ExperienceTot }), SimpleOneColumnChartTypes));

                    #endregion
                    break;
                case SoeReportTemplateType.EmployeeDocumentAnalysis:
                    break;
                case SoeReportTemplateType.EmployeeAccountAnalysis:
                    break;
                case SoeReportTemplateType.EmployeeFixedPayLinesAnalysis:
                    #region EmployeeFixedPayLinesAnalysis
                    break;
                #endregion
                case SoeReportTemplateType.PayrollProductsAnalysis:
                    #region PayrollProductsAnalysis
                    break;
                #endregion
                case SoeReportTemplateType.EmployeeSalaryDistressAnalysis:
                    #region EmployeeSalaryDistressAnalysis
                    break;
                #endregion
                case SoeReportTemplateType.EmployeeSalaryUnionFeesAnalysis:
                    #region EmployeeSalaryUnionFeesAnalysis
                    break;
                #endregion
                case SoeReportTemplateType.EmploymentDaysAnalysis:
                    #region EmploymentDaysAnalysis
                    break;
                #endregion
                case SoeReportTemplateType.TimeStampHistoryAnalysis:
                    #region TimeStampHistoryAnalysis
                    break;
                #endregion
                case SoeReportTemplateType.VerticalTimeTrackerAnalysis:
                    #region VerticalTimeTracker
                    break;
                #endregion
                case SoeReportTemplateType.HorizontalTimeTrackerAnalysis:
                    #region HorizontalTimeTracker
                    break;
                #endregion
                case SoeReportTemplateType.AnnualLeaveTransactionAnalysis:
                    #region AnnualLeaveTransactionAnalysis
                    break;
                    #endregion

            }

            #endregion

            return insights;
        }

        private List<MatrixLayoutColumn> GetFilteredPossibleColumns<T>(List<MatrixLayoutColumn> allColumns, List<T> columns, bool createOptions = false, string key = null)
        {
            List<string> fields = new List<string>();
            foreach (T column in columns)
            {
                fields.Add(EnumUtility.GetName(column).FirstCharToLowerCase());
            }

            List<MatrixLayoutColumn> filteredColumns = allColumns.Where(c => fields.Contains(c.Field)).ToList();

            if (createOptions)
            {
                foreach (MatrixLayoutColumn column in filteredColumns)
                {
                    if (column.Options == null)
                        column.Options = new MatrixDefinitionColumnOptions() { Key = key };
                }
            }

            return filteredColumns;
        }

        #endregion

        #region DataSelection

        public string ReportDataSelectionDTOFromMatrix(int actorCompanyId, string selection, SoeReportTemplateType reportTemplateType, ExportDefinition exportDefinition, ExportDefinitionLevel exportDefinitionLevel, string eventInfo = null)
        {
            var selectionDTOs = ReportDataSelectionDTO.FromJSON(selection);

            if (selectionDTOs.IsNullOrEmpty())
                selectionDTOs = new List<ReportDataSelectionDTO>() { new MatrixColumnsSelectionDTO() { Columns = new List<MatrixColumnSelectionDTO>(), Key = "matrixColumns" } };

            var employeeSelection = selectionDTOs.GetSelection<EmployeeSelectionDTO>("employees");

            if (employeeSelection != null)
            {
                employeeSelection.DoValidateEmployment = true;
                employeeSelection.IncludeHidden = false;
                employeeSelection.IncludeVacant = false;
                employeeSelection.IncludeEnded = false;

                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                if (eventInfo != null && int.TryParse(eventInfo, out int employeeId) && base.HasEventActivatedScheduledJob(entitiesReadOnly, actorCompanyId, TermGroup_ScheduleJobEventActivationType.EmployeeCreated))
                {
                    employeeSelection.EmployeeIds = new List<int>() { employeeId };
                }
            }

            var matrixSelection = selectionDTOs.GetSelection<MatrixColumnsSelectionDTO>("matrixColumns");
            ReportDataSelectionDTOFromMatrix(actorCompanyId, matrixSelection, reportTemplateType, exportDefinition, exportDefinitionLevel);
            return ReportDataSelectionDTO.ToJSON(selectionDTOs);
        }

        public void ReportDataSelectionDTOFromMatrix(int actorCompanyId, MatrixColumnsSelectionDTO matrixColumnsSelectionDTO, SoeReportTemplateType reportTemplateType, ExportDefinition exportDefinition, ExportDefinitionLevel exportDefinitionLevel)
        {
            if (exportDefinitionLevel == null)
                return;

            if (matrixColumnsSelectionDTO == null)
                matrixColumnsSelectionDTO = new MatrixColumnsSelectionDTO() { Columns = new List<MatrixColumnSelectionDTO>() };

            var selectionDTOs = matrixColumnsSelectionDTO.Columns;
            var fromDefinition = GetMatrixColumnSelectionDTOs(reportTemplateType, exportDefinitionLevel);
            List<ExtraFieldDTO> extraFields = new List<ExtraFieldDTO>();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            List<AccountDimDTO> accountDims = base.GetAccountDimsFromCache(entitiesReadOnly, CacheConfig.Company(actorCompanyId));
            foreach (MatrixColumnSelectionDTO col in fromDefinition)
            {
                if (exportDefinition.ActorCompanyId != actorCompanyId && col.Field.Contains("extraField") && col.Options?.Key != null && int.TryParse(col.Options.Key, out int extraFieldId))
                {
                    if (extraFields.IsNullOrEmpty())
                        extraFields = entitiesReadOnly.ExtraField.Where(w => w.ActorCompanyId == actorCompanyId).ToList().ToDTOs().ToList();

                    var extraFieldSource = entitiesReadOnly.ExtraField.FirstOrDefault(f => f.ExtraFieldId == extraFieldId && f.State == (int)SoeEntityState.Active);

                    if (extraFieldSource != null && extraFieldSource.ActorCompanyId != actorCompanyId)
                    {
                        var key = col.Options.Key;
                        col.Options.ChangeExtraFieldKey(new List<ExtraFieldDTO>() { extraFieldSource.ToDTO() }, extraFields);
                        var afterKey = col.Options.Key;
                        col.Field = col.Field.Replace(key, afterKey);
                    }
                }

                if (exportDefinition.ActorCompanyId != actorCompanyId && col.Field.Contains("account") && col.Options?.Key != null && int.TryParse(col.Options.Key, out int accountDimId))
                {
                    if (accountDims.IsNullOrEmpty())
                        accountDims = entitiesReadOnly.AccountDim.Where(w => w.ActorCompanyId == actorCompanyId).ToList().ToDTOs();

                    var accountDimSource = entitiesReadOnly.AccountDim.FirstOrDefault(f => f.AccountDimId == accountDimId && f.State == (int)SoeEntityState.Active);

                    if (accountDimSource != null && accountDimSource.ActorCompanyId != actorCompanyId)
                    {
                        var key = col.Options.Key;
                        col.Options.ChangeAccountDimIdKey(new List<AccountDimDTO>() { accountDimSource.ToDTO() }, accountDims);
                        var afterKey = col.Options.Key;
                        col.Field = col.Field.Replace(key, afterKey);
                    }
                }

                if (!selectionDTOs.Select(s => s.Field).Any(a => a == col.Field))
                    selectionDTOs.Add(col);
            }
            matrixColumnsSelectionDTO.Columns = selectionDTOs;
        }
        public Tuple<string, string> ExtraFieldAndAccountDims(string field, int actorCompanyId, List<ExtraFieldDTO> extraFields, List<AccountDimDTO> accountDims, string key = null)
        {
            if (string.IsNullOrEmpty(field))
                return Tuple.Create(string.Empty, string.Empty);

            if (string.IsNullOrEmpty(key))
                key = Regex.Match(field, @"\d+")?.Value;


            string optionKey = string.Empty;
            if (field.Contains("#"))
            {
                var arr = field.Split('#');
                field = arr[0];
                optionKey = arr[1];
            }
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            if (field.ToLower().Contains("extrafield") && key != null && int.TryParse(key, out int extraFieldId))
            {
                if (extraFields.IsNullOrEmpty())
                    extraFields = entitiesReadOnly.ExtraField.Where(w => w.ActorCompanyId == actorCompanyId).ToList().ToDTOs().ToList();
               
                var extraFieldSource = entitiesReadOnly.ExtraField.FirstOrDefault(f => f.ExtraFieldId == extraFieldId && f.State == (int)SoeEntityState.Active);

                if (extraFieldSource != null && extraFieldSource.ActorCompanyId != actorCompanyId)
                {
                    var match = extraFields.FirstOrDefault(f => f.Text == extraFieldSource.Text);
                    if (match != null)
                        field = field.Replace(extraFieldSource.ExtraFieldId.ToString(), match.ExtraFieldId.ToString());
                }
            }
            else if (field.ToLower().Contains("account") && key != null && int.TryParse(key, out int accountDimId))
            {
                if (accountDims.IsNullOrEmpty())
                    accountDims = entitiesReadOnly.AccountDim.Where(w => w.ActorCompanyId == actorCompanyId).ToList().ToDTOs();

                var accountDimSource = entitiesReadOnly.AccountDim.FirstOrDefault(f => f.AccountDimId == accountDimId && f.State == (int)SoeEntityState.Active);

                if (accountDimSource != null && accountDimSource.ActorCompanyId != actorCompanyId)
                {
                    var match = accountDims.FirstOrDefault(f => f.Name == accountDimSource.Name);
                    if (match != null)
                        field = field.Replace(accountDimSource.AccountDimId.ToString(), match.AccountDimId.ToString());
                }
            }

            return Tuple.Create(field, optionKey);
        }

        #endregion

        #region TimeTransactions

        public MatrixResult CreateTimeTransactions(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            if (!TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out _, out _))
                return null;

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Data
            List<TimeTransactionReportDataReportDataField> fields = new List<TimeTransactionReportDataReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new TimeTransactionReportDataReportDataField(s)).ToList();

            TimeTransactionReportData timeTransactionReportData = new TimeTransactionReportData(parameterObject, new TimeTransactionReportDataInput(reportResult, fields));
            TimeTransactionReportDataOutput output = timeTransactionReportData.CreateOutput(reportResult);

            #endregion

            #region InputMatrix

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var accountInternals = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var extraFields = base.GetExtraFieldsFromCache(entities, CacheConfig.Company(ActorCompanyId, 60));

            var extraFieldsEmployeeDocument = extraFields?.Where(w => w.Entity == SoeEntityType.Employee).ToList();

            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.TimeTransactionMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection, accountInternals, extraFieldsEmployeeDocument);

            #endregion

            #region Create matrix

            var matrix = new TimeTransactionMatrix(inputMatrix, output);
            var result = matrix.GetMatrixResult();

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return result;
        }

        #endregion

        #region Payroll

        #region PayrollTransactions

        public MatrixResult CreatePayrollTransactions(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);

            List<PayrollTransactionReportDataReportDataField> fields = new List<PayrollTransactionReportDataReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new PayrollTransactionReportDataReportDataField(s)).ToList();

            PayrollTransactionReportData payrollTransactionReportData = new PayrollTransactionReportData(parameterObject, new PayrollTransactionReportDataInput(reportResult, fields));
            PayrollTransactionReportDataOutput output = payrollTransactionReportData.CreateOutput(reportResult);

            #region InputMatrix

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.PayrollTransactionMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection);

            #endregion

            #region Create matrix

            var matrix = new PayrollTransactionMatrix(inputMatrix, output);
            var result = matrix.GetMatrixResult();

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return result;
        }

        #endregion

        #region EmployeeTimePeriods

        public MatrixResult CreateEmployeeTimePeriods(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);

            List<EmployeeTimePeriodReportDataField> fields = new List<EmployeeTimePeriodReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new EmployeeTimePeriodReportDataField(s)).ToList();

            EmployeeTimePeriodReportData employeeTimePeriodReportData = new EmployeeTimePeriodReportData(parameterObject, new EmployeeTimePeriodReportDataInput(reportResult, fields));
            EmployeeTimePeriodReportDataOutput output = employeeTimePeriodReportData.CreateOutput(reportResult);

            #region InputMatrix

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmployeeTimePeriodMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection);

            #endregion

            #region Create matrix

            var matrix = new EmployeeTimePeriodMatrix(inputMatrix, output);
            var result = matrix.GetMatrixResult();

            #endregion

            return result;
        }

        #endregion

        #region Employment history
        public MatrixResult CreateEmploymentHistoryData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Data
            List<EmploymentHistoryReportDataField> fields = new List<EmploymentHistoryReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new EmploymentHistoryReportDataField(s)).ToList();

            EmploymentHistoryReportData userReportData = new EmploymentHistoryReportData(parameterObject, new EmploymentHistoryReportDataInput(reportResult, fields));
            EmploymentHistoryReportDataOutput output = userReportData.CreateOutput(reportResult);

            #endregion

            #region InputMatrix

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(reportResult.ActorCompanyId));
            var accountInternals = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(reportResult.ActorCompanyId));
            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmploymentHistoryMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection, accountInternals);

            #endregion

            #region Create matrix

            var matrix = new EmploymentHistoryMatrix(inputMatrix, output);
            var result = matrix.GetMatrixResult();

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return result;
        }



        #endregion

        #region Employment days
        public MatrixResult CreateEmploymentDays(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);
            List<EmploymentDaysReportDataField> fields = new List<EmploymentDaysReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new EmploymentDaysReportDataField(s)).ToList();

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var accountInternals = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            EmploymentDaysReportData EmploymentDaysReportData = new EmploymentDaysReportData(parameterObject, reportDataInput: new EmploymentDaysReportDataInput(reportResult, fields));
            EmploymentDaysReportDataOutput output = EmploymentDaysReportData.CreateOutput(reportResult);
            List<EmploymentTypeDTO> employmentTypes = EmployeeManager.GetEmploymentTypes(ActorCompanyId);

            #region InputMatrix

            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmploymentDaysMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection, accountInternals);

            #endregion

            #region Create matrix

            var matrix = new EmploymentDaysMatrix(inputMatrix, output, employmentTypes);
            var result = matrix.GetMatrixResult();

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return result;

        }
        #endregion

        #endregion

        #region Staffing

        #region StaffingStatisticss

        public MatrixResult CreateStaffingStatisticss(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);

            List<StaffingStatisticsReportDataField> fields = new List<StaffingStatisticsReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new StaffingStatisticsReportDataField(s)).ToList();



            StaffingStatisticsReportData staffingStatisticsReportData = new StaffingStatisticsReportData(parameterObject, new StaffingStatisticsReportDataInput(reportResult, fields));
            StaffingStatisticsReportDataOutput output = staffingStatisticsReportData.CreateOutput(reportResult);

            #region InputMatrix

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.StaffingStatisticsMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection);

            #endregion

            #region Create matrix

            var matrix = new StaffingStatisticsMatrix(inputMatrix, output, ActorCompanyId);
            var result = matrix.GetMatrixResult();

            #endregion
            return result;
        }

        #endregion

        #region AnnualProgresss

        public MatrixResult CreateAnnualProgresss(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);

            List<AnnualProgressReportDataField> fields = new List<AnnualProgressReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new AnnualProgressReportDataField(s)).ToList();

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var accountInternals = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(ActorCompanyId));

            AnnualProgressReportData longtermAbsenceReportData = new AnnualProgressReportData(parameterObject, new AnnualProgressReportDataInput(reportResult, fields, accountDims, accountInternals));
            AnnualProgressReportDataOutput output = longtermAbsenceReportData.CreateOutput(reportResult);

            #region InputMatrix

            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.AnnualProgressMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection);

            #endregion

            #region Create matrix

            var matrix = new AnnualProgressMatrix(inputMatrix, output, ActorCompanyId);
            var result = matrix.GetMatrixResult();

            #endregion

            return result;
        }

        #endregion

        #region LongtermAbsences

        public MatrixResult CreateLongtermAbsence(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);

            List<LongtermAbsenceReportDataField> fields = new List<LongtermAbsenceReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new LongtermAbsenceReportDataField(s)).ToList();

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));

            LongtermAbsenceReportData annualProgressReportData = new LongtermAbsenceReportData(parameterObject, new LongtermAbsenceReportDataInput(reportResult, fields));
            LongtermAbsenceReportDataOutput output = annualProgressReportData.CreateOutput(reportResult);

            #region InputMatrix

            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.LongtermAbsenceMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection);

            #endregion

            #region Create matrix

            var matrix = new LongtermAbsenceMatrix(inputMatrix, output, ActorCompanyId);
            var result = matrix.GetMatrixResult();

            #endregion
            return result;
        }

        #endregion


        #region StaffingneedsFrequency

        public MatrixResult CreateStaffingneedsFrequencys(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);

            List<StaffingneedsFrequencyReportDataReportDataField> fields = new List<StaffingneedsFrequencyReportDataReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new StaffingneedsFrequencyReportDataReportDataField(s)).ToList();

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var accountInternals = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(ActorCompanyId));

            StaffingneedsFrequencyReportData StaffingneedsFrequencyReportData = new StaffingneedsFrequencyReportData(parameterObject, new StaffingneedsFrequencyReportDataInput(reportResult, fields, accountDims, accountInternals));
            StaffingneedsFrequencyReportDataOutput output = StaffingneedsFrequencyReportData.CreateOutput(reportResult);

            #region InputMatrix

            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.StaffingneedsFrequencyMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection, accountInternals);

            #endregion

            #region Create matrix

            var matrix = new StaffingneedsFrequencyMatrix(inputMatrix, output);
            var result = matrix.GetMatrixResult();

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return result;
        }

        //public MatrixResult CreateEmploymentHistoryData(CreateReportResult reportResult)
        //{
        //    base.reportResult = reportResult;

        //    #region Prereq

        //    MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);

        //    #endregion

        //    #region Init repository

        //    base.InitPersonalDataEmployeeReportRepository();

        //    #endregion

        //    #region Data
        //    List<EmploymentHistoryReportDataField> fields = new List<EmploymentHistoryReportDataField>();
        //    if (matrixColumnsSelection?.Columns != null)
        //        fields = matrixColumnsSelection.Columns.Select(s => new EmploymentHistoryReportDataField(s)).ToList();

        //    EmploymentHistoryReportData userReportData = new EmploymentHistoryReportData(parameterObject, new EmploymentHistoryReportDataInput(reportResult, fields));
        //    EmploymentHistoryReportDataOutput output = userReportData.CreateOutput(reportResult);

        //    #endregion

        //    #region InputMatrix

        //    var permissions = base.GetFeaturePermissionItemsFromCache(CompEntities, CacheConfig.License(Company.LicenseId, ActorCompanyId, RoleId)).ToDTOs();
        //    var accountDims = base.GetAccountDimsFromCache(CompEntities, CacheConfig.Company(ActorCompanyId));
        //    var accountInternals = base.GetAccountInternalsFromCache(CompEntities, CacheConfig.Company(ActorCompanyId));
        //    InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmploymentHistoryMatrixColumns), reportResult.ExportType, accountDims, permissions, matrixColumnsSelection, accountInternals);

        //    #endregion

        //    #region Create matrix

        //    var matrix = new EmploymentHistoryMatrix(inputMatrix, output);
        //    var result = matrix.GetMatrixResult();

        //    #endregion

        //    #region Close repository

        //    base.personalDataRepository.GenerateLogs();

        //    #endregion

        //    return result;
        //}

        #endregion

        #endregion

        #region Employee

        public MatrixResult CreateEmployeeList(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);
            List<EmployeeListReportDataReportDataField> fields = new List<EmployeeListReportDataReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new EmployeeListReportDataReportDataField(s)).ToList();

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var accountInternals = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var extraFields = base.GetExtraFieldsFromCache(entities, CacheConfig.Company(ActorCompanyId)).Where(w => w.Entity == SoeEntityType.Employee).ToList();
            EmployeeListReportData EmployeeListReportData = new EmployeeListReportData(parameterObject, new EmployeeListReportDataInput(reportResult, fields, accountDims, accountInternals));
            EmployeeListReportDataOutput output = EmployeeListReportData.CreateOutput(reportResult);

            #region InputMatrix

            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmployeeListMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection, accountInternals, extraFields);

            #endregion

            #region Create matrix

            var matrix = new EmployeeListMatrix(inputMatrix, output, ActorCompanyId);
            var result = matrix.GetMatrixResult();

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return result;

        }
        public MatrixResult CreateEmployeeSkill(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);
            List<EmployeeSkillReportDataField> fields = new List<EmployeeSkillReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new EmployeeSkillReportDataField(s)).ToList();

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var accountInternals = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            EmployeeSkillReportData EmployeeSkillReportData = new EmployeeSkillReportData(parameterObject, reportDataInput: new EmployeeSkillReportDataInput(reportResult, fields));
            EmployeeSkillReportDataOutput output = EmployeeSkillReportData.CreateOutput(reportResult);

            #region InputMatrix

            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmployeeSkillMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection, accountInternals);

            #endregion

            #region Create matrix

            var matrix = new EmployeeSkillMatrix(inputMatrix, output, ActorCompanyId);
            var result = matrix.GetMatrixResult();

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return result;

        }
        public MatrixResult CreateShiftTypeSkill(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);
            List<ShiftTypeSkillReportDataField> fields = new List<ShiftTypeSkillReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new ShiftTypeSkillReportDataField(s)).ToList();

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var accountInternals = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            ShiftTypeSkillReportData ShiftTypeSkillReportData = new ShiftTypeSkillReportData(parameterObject, reportDataInput: new ShiftTypeSkillReportDataInput(reportResult, fields));
            ShiftTypeSkillReportDataOutput output = ShiftTypeSkillReportData.CreateOutput(reportResult);

            #region InputMatrix

            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.ShiftTypeSkillMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection, accountInternals);

            #endregion

            #region Create matrix

            var matrix = new ShiftTypeSkillMatrix(inputMatrix, output);
            var result = matrix.GetMatrixResult();

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return result;

        }
        public MatrixResult CreateEmployeeEndReasons(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);
            List<EmployeeEndReasonsReportDataField> fields = new List<EmployeeEndReasonsReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new EmployeeEndReasonsReportDataField(s)).ToList();

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var accountInternals = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            EmployeeEndReasonsReportData EmployeeEndReasonsReportData = new EmployeeEndReasonsReportData(parameterObject, reportDataInput: new EmployeeEndReasonsReportDataInput(reportResult, fields));
            EmployeeEndReasonsReportDataOutput output = EmployeeEndReasonsReportData.CreateOutput(reportResult);

            #region InputMatrix

            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmployeeEndReasonsMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection, accountInternals);

            #endregion

            #region Create matrix

            var matrix = new EmployeeEndReasonsMatrix(inputMatrix, output);
            var result = matrix.GetMatrixResult();

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return result;

        }
        public MatrixResult CreatePayrollProducts(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);
            List<PayrollProductsReportDataField> fields = new List<PayrollProductsReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new PayrollProductsReportDataField(s)).ToList();

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var accountInternals = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            PayrollProductsReportData PayrollProductsReportData = new PayrollProductsReportData(parameterObject, reportDataInput: new PayrollProductsReportDataInput(reportResult, fields));
            PayrollProductsReportDataOutput output = PayrollProductsReportData.CreateOutput(reportResult);

            #region InputMatrix

            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.PayrollProductsMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection, accountInternals);

            #endregion

            #region Create matrix

            var matrix = new PayrollProductsMatrix(inputMatrix, output);
            var result = matrix.GetMatrixResult();

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return result;

        }
        public MatrixResult CreateEmployeeSalaryDistress(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);
            List<EmployeeSalaryDistressReportDataField> fields = new List<EmployeeSalaryDistressReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new EmployeeSalaryDistressReportDataField(s)).ToList();

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var accountInternals = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            EmployeeSalaryDistressReportData EmployeeSalaryDistressReportData = new EmployeeSalaryDistressReportData(parameterObject, reportDataInput: new EmployeeSalaryDistressReportDataInput(reportResult, fields));
            EmployeeSalaryDistressReportDataOutput output = EmployeeSalaryDistressReportData.CreateOutput(reportResult);

            #region InputMatrix

            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmployeeSalaryDistressMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection, accountInternals);

            #endregion

            #region Create matrix

            var matrix = new EmployeeSalaryDistressMatrix(inputMatrix, output);
            var result = matrix.GetMatrixResult();

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return result;

        }
        public MatrixResult CreateEmployeeSalaryUnionFees(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);
            List<EmployeeSalaryUnionFeesReportDataField> fields = new List<EmployeeSalaryUnionFeesReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new EmployeeSalaryUnionFeesReportDataField(s)).ToList();

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var accountInternals = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            EmployeeSalaryUnionFeesReportData EmployeeSalaryUnionFeesReportData = new EmployeeSalaryUnionFeesReportData(parameterObject, reportDataInput: new EmployeeSalaryUnionFeesReportDataInput(reportResult, fields));
            EmployeeSalaryUnionFeesReportDataOutput output = EmployeeSalaryUnionFeesReportData.CreateOutput(reportResult);

            #region InputMatrix

            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmployeeSalaryUnionFeesMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection, accountInternals);

            #endregion

            #region Create matrix

            var matrix = new EmployeeSalaryUnionFeesMatrix(inputMatrix, output);
            var result = matrix.GetMatrixResult();

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return result;

        }
        public MatrixResult CreateEmployeeFixedPayLines(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);
            List<EmployeeFixedPayLinesReportDataField> fields = new List<EmployeeFixedPayLinesReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new EmployeeFixedPayLinesReportDataField(s)).ToList();

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var accountInternals = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            EmployeeFixedPayLinesReportData EmployeeFixedPayLinesReportData = new EmployeeFixedPayLinesReportData(parameterObject, reportDataInput: new EmployeeFixedPayLinesReportDataInput(reportResult, fields));
            EmployeeFixedPayLinesReportDataOutput output = EmployeeFixedPayLinesReportData.CreateOutput(reportResult);

            #region InputMatrix

            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmployeeFixedPayLinesMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection, accountInternals);

            #endregion

            #region Create matrix

            var matrix = new EmployeeFixedPayLinesMatrix(inputMatrix, output);
            var result = matrix.GetMatrixResult();

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return result;

        }
        public MatrixResult CreateEmployeeSalary(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);
            List<EmployeeSalaryReportDataField> fields = new List<EmployeeSalaryReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new EmployeeSalaryReportDataField(s)).ToList();

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var accountInternals = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var extraFields = base.GetExtraFieldsFromCache(entities, CacheConfig.Company(ActorCompanyId, 60));
            var extraFieldsEmployee = extraFields?.Where(w => w.Entity == SoeEntityType.Employee).ToList();
            EmployeeSalaryReportData employeeSalaryReportData = new EmployeeSalaryReportData(parameterObject, reportDataInput: new EmployeeSalaryReportDataInput(reportResult, fields));
            EmployeeSalaryReportDataOutput output = employeeSalaryReportData.CreateOutput(reportResult);

            #region InputMatrix

            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmployeeSalaryMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection, accountInternals, extraFieldsEmployee);

            #endregion

            #region Create matrix

            var matrix = new EmployeeSalaryMatrix(inputMatrix, output, ActorCompanyId);
            var result = matrix.GetMatrixResult();

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return result;

        }
        public MatrixResult CreateEmployeeMeeting(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);
            List<EmployeeMeetingReportDataField> fields = new List<EmployeeMeetingReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new EmployeeMeetingReportDataField(s)).ToList();

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var accountInternals = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var extraFields = base.GetExtraFieldsFromCache(entities, CacheConfig.Company(ActorCompanyId, 60));
            var extraFieldsEmployeeMeeting = extraFields?.Where(w => w.Entity == SoeEntityType.Employee).ToList();

            EmployeeMeetingReportData employeeMeetingReportData = new EmployeeMeetingReportData(parameterObject, reportDataInput: new EmployeeMeetingReportDataInput(reportResult, fields));
            EmployeeMeetingReportDataOutput output = employeeMeetingReportData.CreateOutput(reportResult);

            #region InputMatrix

            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmployeeMeetingMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection, accountInternals, extraFieldsEmployeeMeeting);

            #endregion

            #region Create matrix

            var matrix = new EmployeeMeetingMatrix(inputMatrix, output, ActorCompanyId);
            var result = matrix.GetMatrixResult();

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return result;

        }
        public MatrixResult CreateEmployeeDates(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            if (!TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out _, out _))
                return null;

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Data
            List<EmployeeDateReportDataField> fields = new List<EmployeeDateReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new EmployeeDateReportDataField(s)).ToList();

            EmployeeDateReportData employeeDateReportData = new EmployeeDateReportData(parameterObject, new EmployeeDateReportDataInput(reportResult, fields));
            EmployeeDateReportDataOutput output = employeeDateReportData.CreateOutput(reportResult);

            #endregion

            #region InputMatrix

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var accountInternals = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmployeeDateMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection, accountInternals);

            #endregion

            #region Create matrix

            var matrix = new EmployeeDateMatrix(inputMatrix, output);
            var result = matrix.GetMatrixResult();

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return result;
        }
        public MatrixResult CreateEmployeeExperience(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            if (!TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out _, out _))
                return null;

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Data
            List<EmployeeExperienceReportDataField> fields = new List<EmployeeExperienceReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new EmployeeExperienceReportDataField(s)).ToList();

            EmployeeExperienceReportData employeeExperienceReportData = new EmployeeExperienceReportData(parameterObject, new EmployeeExperienceReportDataInput(reportResult, fields));
            EmployeeExperienceReportDataOutput output = employeeExperienceReportData.CreateOutput(reportResult);

            #endregion

            #region InputMatrix

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var accountInternals = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmployeeExperienceMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection, accountInternals);

            #endregion

            #region Create matrix

            var matrix = new EmployeeExperienceMatrix(inputMatrix, output);
            var result = matrix.GetMatrixResult();

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return result;
        }

        public MatrixResult CreateEmployeeDocument(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);
            List<EmployeeDocumentReportDataField> fields = new List<EmployeeDocumentReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new EmployeeDocumentReportDataField(s)).ToList();

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var accountInternals = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var extraFields = base.GetExtraFieldsFromCache(entities, CacheConfig.Company(ActorCompanyId, 60));
            var extraFieldsEmployee = extraFields?.Where(w => w.Entity == SoeEntityType.Employee).ToList();

            EmployeeDocumentReportData employeeDocumentReportData = new EmployeeDocumentReportData(parameterObject, reportDataInput: new EmployeeDocumentReportDataInput(reportResult, fields));
            EmployeeDocumentReportDataOutput output = employeeDocumentReportData.CreateOutput(reportResult);

            #region InputMatrix

            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmployeeDocumentMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection, accountInternals, extraFieldsEmployee);

            #endregion

            #region Create matrix

            var matrix = new EmployeeDocumentMatrix(inputMatrix, output, ActorCompanyId);
            var result = matrix.GetMatrixResult();

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return result;

        }

        public MatrixResult CreateEmployeeAccount(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);
            List<EmployeeAccountReportDataField> fields = new List<EmployeeAccountReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new EmployeeAccountReportDataField(s)).ToList();

            int defaultEmployeeAccountDimId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.DefaultEmployeeAccountDimEmployee, 0, ActorCompanyId, 0);
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var accountInternals = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var extraFields = base.GetExtraFieldsFromCache(entities, CacheConfig.Company(ActorCompanyId)).Where(w => w.Entity == SoeEntityType.Employee).ToList();
            EmployeeAccountReportData employeeAccountReportData = new EmployeeAccountReportData(parameterObject, reportDataInput: new EmployeeAccountReportDataInput(reportResult, fields));
            EmployeeAccountReportDataOutput output = employeeAccountReportData.CreateOutput(reportResult);

            #region InputMatrix

            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmployeeAccountMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection, accountInternals, extraFields);

            #endregion

            #region Create matrix

            var matrix = new EmployeeAccountMatrix(inputMatrix, output, ActorCompanyId, defaultEmployeeAccountDimId);
            var result = matrix.GetMatrixResult();

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return result;

        }

        public MatrixResult CreateVacationBalance(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);
            List<VacationBalanceReportDataField> fields = new List<VacationBalanceReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new VacationBalanceReportDataField(s)).ToList();

            int defaultEmployeeAccountDimId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.DefaultEmployeeAccountDimEmployee, 0, ActorCompanyId, 0);
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var accountInternals = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var extraFields = base.GetExtraFieldsFromCache(entities, CacheConfig.Company(ActorCompanyId)).Where(w => w.Entity == SoeEntityType.Employee).ToList();
            VacationBalanceReportData vacationBalanceReportData = new VacationBalanceReportData(parameterObject, reportDataInput: new VacationBalanceReportDataInput(reportResult, fields, accountDims, accountInternals));
            VacationBalanceReportDataOutput output = vacationBalanceReportData.CreateOutput(reportResult);

            #region InputMatrix

            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.VacationBalanceMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection, accountInternals, extraFields);

            #endregion

            #region Create matrix

            var matrix = new VacationBalanceMatrix(inputMatrix, output, ActorCompanyId);
            var result = matrix.GetMatrixResult();

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return result;
        }

        public MatrixResult CreateTimeStampHistory(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);
            List<TimeStampHistoryReportDataField> fields = new List<TimeStampHistoryReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new TimeStampHistoryReportDataField(s)).ToList();

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var accountInternals = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            TimeStampHistoryReportData timeStampHistoryReportData = new TimeStampHistoryReportData(parameterObject, reportDataInput: new TimeStampHistoryReportDataInput(reportResult, fields));
            TimeStampHistoryReportDataOutput output = timeStampHistoryReportData.CreateOutput(reportResult);

            #region InputMatrix

            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.TimeStampHistoryMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection, accountInternals);

            #endregion

            #region Create matrix

            var matrix = new TimeStampHistoryMatrix(inputMatrix, output);
            var result = matrix.GetMatrixResult();

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return result;

        }

        public MatrixResult CreateAnnualLeaveTransaction(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);
            List<AnnualLeaveTransactionReportDataField> fields = new List<AnnualLeaveTransactionReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new AnnualLeaveTransactionReportDataField(s)).ToList();

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var accountInternals = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            AnnualLeaveTransactionReportData annualLeaveTransactionReportData = new AnnualLeaveTransactionReportData(parameterObject, reportDataInput: new AnnualLeaveTransactionReportDataInput(reportResult, fields));
            AnnualLeaveTransactionReportDataOutput output = annualLeaveTransactionReportData.CreateOutput(reportResult);

            #region InputMatrix

            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.AnnualLeaveTransactionMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection, accountInternals);

            #endregion

            #region Create matrix

            var matrix = new AnnualLeaveTransactionMatrix(inputMatrix, output);
            var result = matrix.GetMatrixResult();

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return result;

        }
        #endregion

        #region Staffing

        public MatrixResult CreateScheduleTransactions(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            if (!TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out _, out _))
                return null;

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Data
            List<ScheduleTransactionReportDataReportDataField> fields = new List<ScheduleTransactionReportDataReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new ScheduleTransactionReportDataReportDataField(s)).ToList();

            ScheduleTransactionReportData scheduleTransactionReportData = new ScheduleTransactionReportData(parameterObject, new ScheduleTransactionReportDataInput(reportResult, fields));
            ScheduleTransactionReportDataOutput output = scheduleTransactionReportData.CreateOutput(reportResult);

            #endregion

            #region InputMatrix

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var extraFields = base.GetExtraFieldsFromCache(entities, CacheConfig.Company(ActorCompanyId, 60));
            var accountInternals = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var extraFieldsEmployeeScheduleTransactions = extraFields?.Where(w => w.Entity == SoeEntityType.Employee).ToList();
            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.ScheduleTransactionMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection, accountInternals, extraFieldsEmployeeScheduleTransactions);

            #endregion

            #region Create matrix

            var matrix = new ScheduleTransactionMatrix(inputMatrix, output);
            var result = matrix.GetMatrixResult();

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return result;
        }

        #region ShiftQueues

        public MatrixResult CreateShiftQueues(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;


            #region Prereq

            if (!TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out _, out _))
                return null;

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            List<ShiftQueueReportDataField> fields = new List<ShiftQueueReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new ShiftQueueReportDataField(s)).ToList();

            ShiftQueueReportData shiftQueueyReportData = new ShiftQueueReportData(parameterObject, new ShiftQueueReportDataInput(reportResult, fields, TimeReportDataManager, EmployeeManager, TimeScheduleManager));
            ShiftQueueReportDataOutput output = shiftQueueyReportData.CreateOutput(reportResult);

            #region InputMatrix

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.ShiftQueueMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection);

            #endregion

            #region Create matrix

            var matrix = new ShiftQueueMatrix(inputMatrix, output);
            var result = matrix.GetMatrixResult();

            #endregion
            return result;
        }

        #endregion

        #region TimeTracker

        #region VerticalTimeTracker

        public MatrixResult CreateVerticalTimeTracker(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;


            #region Prereq

            if (!TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out _, out _))
                return null;

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            List<VerticalTimeTrackerReportDataField> fields = new List<VerticalTimeTrackerReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new VerticalTimeTrackerReportDataField(s)).ToList();

            VerticalTimeTrackerReportData verticalTimeTrackeryReportData = new VerticalTimeTrackerReportData(parameterObject, new VerticalTimeTrackerReportDataInput(reportResult, fields));
            VerticalTimeTrackerReportDataOutput output = verticalTimeTrackeryReportData.CreateOutput(reportResult);

            #region InputMatrix

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.VerticalTimeTrackerMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection);

            #endregion

            #region Create matrix

            var matrix = new VerticalTimeTrackerMatrix(inputMatrix, output);
            var result = matrix.GetMatrixResult();

            #endregion
            return result;
        }

        #endregion

        #region HorizontalTimeTracker

        public MatrixResult CreateHorizontalTimeTracker(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;


            #region Prereq

            if (!TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out _, out _))
                return null;

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            List<HorizontalTimeTrackerReportDataField> fields = new List<HorizontalTimeTrackerReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new HorizontalTimeTrackerReportDataField(s)).ToList();

            HorizontalTimeTrackerReportData horizontalTimeTrackeryReportData = new HorizontalTimeTrackerReportData(parameterObject, new HorizontalTimeTrackerReportDataInput(reportResult, fields));
            HorizontalTimeTrackerReportDataOutput output = horizontalTimeTrackeryReportData.CreateOutput(reportResult);

            #region InputMatrix

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.HorizontalTimeTrackerMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection);

            #endregion

            #region Create matrix

            var matrix = new HorizontalTimeTrackerMatrix(inputMatrix, output);
            var result = matrix.GetMatrixResult();

            #endregion

            return result;
        }

        #endregion
        #endregion

        #region AgiAbsence
        public MatrixResult CreateAgiAbsence(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;


            #region Prereq

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            List<AgiAbsenceReportDataField> fields = new List<AgiAbsenceReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new AgiAbsenceReportDataField(s)).ToList();

            AgiAbsenceReportData agiAbsenceyReportData = new AgiAbsenceReportData(parameterObject, new AgiAbsenceReportDataInput(reportResult, fields));
            AgiAbsenceReportDataOutput output = agiAbsenceyReportData.CreateOutput(reportResult);

            #region InputMatrix

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.AgiAbsenceMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection);

            #endregion

            #region Create matrix

            var matrix = new AgiAbsenceMatrix(inputMatrix, output);
            var result = matrix.GetMatrixResult();

            #endregion

            return result;
        }
        #endregion

        #region ShiftRequests

        public MatrixResult CreateShiftRequests(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;


            #region Prereq

            if (!TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out _, out _))
                return null;

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            List<ShiftRequestReportDataField> fields = new List<ShiftRequestReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new ShiftRequestReportDataField(s)).ToList();

            ShiftRequestReportData shiftRequestyReportData = new ShiftRequestReportData(parameterObject, new ShiftRequestReportDataInput(reportResult, fields, TimeReportDataManager, EmployeeManager, TimeScheduleManager));
            ShiftRequestReportDataOutput output = shiftRequestyReportData.CreateOutput(reportResult);

            #region InputMatrix

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.ShiftRequestMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection);

            #endregion

            #region Create matrix

            var matrix = new ShiftRequestMatrix(inputMatrix, output, ActorCompanyId);
            var result = matrix.GetMatrixResult();

            #endregion
            return result;
        }

        #endregion

        #region ShiftHistory

        public MatrixResult CreateShiftHistories(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            if (!TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out _, out _))
                return null;

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            List<ShiftHistoryReportDataField> fields = new List<ShiftHistoryReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new ShiftHistoryReportDataField(s)).ToList();

            ShiftHistoryReportData shiftHistoryyReportData = new ShiftHistoryReportData(parameterObject, new ShiftHistoryReportDataInput(reportResult, fields, TimeReportDataManager, EmployeeManager, TimeScheduleManager));
            ShiftHistoryReportDataOutput output = shiftHistoryyReportData.CreateOutput(reportResult);

            #region InputMatrix

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.ShiftHistoryMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection);

            #endregion

            #region Create matrix

            var matrix = new ShiftHistoryMatrix(inputMatrix, output);
            var result = matrix.GetMatrixResult();

            #endregion
            return result;
        }

        #endregion

        #region SwapShift
        public MatrixResult CreateSwapShiftData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);
            List<SwapShiftReportDataField> fields = new List<SwapShiftReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new SwapShiftReportDataField(s)).ToList();

            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entitiesReadOnly, CacheConfig.Company(ActorCompanyId));
            var accountInternals = base.GetAccountInternalsFromCache(entitiesReadOnly, CacheConfig.Company(ActorCompanyId));
            SwapShiftReportData swapShiftReportData = new SwapShiftReportData(parameterObject, reportDataInput: new SwapShiftReportDataInput(reportResult, fields));
            SwapShiftReportDataOutput output = swapShiftReportData.CreateOutput(reportResult);

            #region InputMatrix

            InputMatrix inputMatrix = new(GetTermGroupContent(TermGroup.SwapShiftMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection, accountInternals);

            #endregion

            #region Create matrix

            var matrix = new SwapShiftMatrix(inputMatrix, output);
            var result = matrix.GetMatrixResult();

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return result;

        }
        #endregion

        #region AbsenceRequests

        public MatrixResult CreateAbsenceRequests(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;


            #region Prereq

            if (!TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out _, out _))
                return null;

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            List<AbsenceRequestReportDataField> fields = new List<AbsenceRequestReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new AbsenceRequestReportDataField(s)).ToList();

            AbsenceRequestReportData absenceRequestyReportData = new AbsenceRequestReportData(parameterObject, new AbsenceRequestReportDataInput(reportResult, TimeReportDataManager, EmployeeManager, TimeScheduleManager, fields));
            AbsenceRequestReportDataOutput output = absenceRequestyReportData.CreateOutput(reportResult);

            #region InputMatrix

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.AbsenceRequestMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection);

            #endregion

            #region Create matrix

            var matrix = new AbsenceRequestMatrix(inputMatrix, output);
            var result = matrix.GetMatrixResult();

            #endregion
            return result;
        }

        #endregion


        #region ScheduledTimeSummarys

        public MatrixResult CreateScheduledTimeSummaries(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);

            List<ScheduledTimeSummaryReportDataField> fields = new List<ScheduledTimeSummaryReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new ScheduledTimeSummaryReportDataField(s)).ToList();

            ScheduledTimeSummaryReportData shiftQueueyReportData = new ScheduledTimeSummaryReportData(parameterObject, new ScheduledTimeSummaryReportDataInput(reportResult, fields));
            ScheduledTimeSummaryReportDataOutput output = shiftQueueyReportData.CreateOutput(reportResult);

            #region InputMatrix

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.ScheduledTimeSummaryMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection);

            #endregion

            #region Create matrix

            var matrix = new ScheduledTimeSummaryMatrix(inputMatrix, output);
            var result = matrix.GetMatrixResult();

            #endregion
            return result;
        }

        #endregion


        #region AggregatedTimeStatisticss

        public MatrixResult CreateAggregatedTimeStatisticss(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);

            List<AggregatedTimeStatisticsReportDataField> fields = new List<AggregatedTimeStatisticsReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new AggregatedTimeStatisticsReportDataField(s)).ToList();

            AggregatedTimeStatisticsReportData aggregatedTimeStatisticsReportData = new AggregatedTimeStatisticsReportData(parameterObject, new AggregatedTimeStatisticsReportDataInput(reportResult, fields));
            AggregatedTimeStatisticsReportDataOutput output = aggregatedTimeStatisticsReportData.CreateOutput(reportResult);

            #region InputMatrix

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.AggregatedTimeStatisticsMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection);

            #endregion

            #region Create matrix

            var matrix = new AggregatedTimeStatisticsMatrix(inputMatrix, output);
            var result = matrix.GetMatrixResult();

            #endregion
            return result;
        }

        #endregion

        #region  EmployeeChild
        public MatrixResult CreateEmployeeChild(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;


            #region Prereq

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            List<EmployeeChildReportDataField> fields = new List<EmployeeChildReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new EmployeeChildReportDataField(s)).ToList();

            EmployeeChildReportData employeeChildyReportData = new EmployeeChildReportData(parameterObject, new EmployeeChildReportDataInput(reportResult, fields));
            EmployeeChildReportDataOutput output = employeeChildyReportData.CreateOutput(reportResult);

            #region InputMatrix

            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmployeeChildMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, null, matrixColumnsSelection);

            #endregion

            #region Create matrix

            var matrix = new EmployeeChildMatrix(inputMatrix, output);
            var result = matrix.GetMatrixResult();

            #endregion

            return result;
        }
        #endregion

        #region  EmployeePayrollAdditions
        public MatrixResult CreateEmployeePayrollAdditions(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;


            #region Prereq

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            List<EmployeePayrollAdditionsReportDataField> fields = new List<EmployeePayrollAdditionsReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new EmployeePayrollAdditionsReportDataField(s)).ToList();

            EmployeePayrollAdditionsReportData employeePayrollAdditionsReportData = new EmployeePayrollAdditionsReportData(parameterObject, new EmployeePayrollAdditionsReportDataInput(reportResult, fields));
            EmployeePayrollAdditionsReportDataOutput output = employeePayrollAdditionsReportData.CreateOutput(reportResult);

            #region InputMatrix
            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmployeePayrollAdditionsMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, null, matrixColumnsSelection);

            #endregion

            #region Create matrix

            var matrix = new EmployeePayrollAdditionsMatrix(inputMatrix, output);
            var result = matrix.GetMatrixResult();

            #endregion

            return result;
        }
        #endregion

        #endregion

        #region TimestampEntries

        public MatrixResult CreateTimeStampEntriesData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            if (!TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out _, out _))
                return null;

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Data
            List<TimeStampEntryReportDataField> fields = new List<TimeStampEntryReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new TimeStampEntryReportDataField(s)).ToList();

            TimeStampEntryReportData timeStampEntryReportData = new TimeStampEntryReportData(parameterObject, new TimeStampEntryReportDataInput(reportResult, fields));
            TimeStampEntryReportDataOutput output = timeStampEntryReportData.CreateOutput(reportResult);

            #endregion

            #region InputMatrix

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var accountInternals = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.TimeStampEntryMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection, accountInternals);

            #endregion

            #region Create matrix

            var matrix = new TimeStampEntryMatrix(inputMatrix, output);
            var result = matrix.GetMatrixResult();

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return result;
        }


        #endregion
        #region Bridge

        #region Visma

        public MatrixResult CreateVismaPayrollChanges(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;


            #region Prereq

            if (!TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out _, out _))
                return null;

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            List<VismaPayrollChangesReportDataField> fields = new List<VismaPayrollChangesReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new VismaPayrollChangesReportDataField(s)).ToList();

            VismaPayrollChangesReportData VismaPayrollChangesyReportData = new VismaPayrollChangesReportData(parameterObject, new VismaPayrollChangesReportDataInput(reportResult, fields));
            VismaPayrollChangesReportDataOutput output = VismaPayrollChangesyReportData.CreateOutput(reportResult);

            #region InputMatrix

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.VismaPayrollChangesMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection);

            #endregion

            #region Create matrix

            var matrix = new VismaPayrollChangesMatrix(inputMatrix, output, reportResult.ActorCompanyId);
            var result = matrix.GetMatrixResult();

            #endregion
            return result;
        }

        #endregion

        #endregion

        #region Generic

        #region Schedule

        public ActionResult GetMatrixResultFromPlanningDay(int actorCompanyId, List<TimeSchedulePlanningDayDTO> timeSchedulePlanningDays, List<EmployeeListDTO> employeeListDTOs, List<DateTime> intervalStartTimes, List<ReportDataSelectionDTO> selections)
        {
            bool isPeriod = false;

            // If exporting only one day from weekview
            if (intervalStartTimes.Count == 1)
                isPeriod = true;
            else if (intervalStartTimes.Count > 1 && (intervalStartTimes[1] - intervalStartTimes[0]).TotalMinutes > 1400)
                isPeriod = true;

            if (isPeriod)
                return GetMatrixResultFromPlanningDays(actorCompanyId, timeSchedulePlanningDays, employeeListDTOs, intervalStartTimes, selections);

            TryGetBoolFromSelection(selections, out bool showEmployeeGroup, "showEmployeeGroup");
            TryGetBoolFromSelection(selections, out bool showContactInfo, "showContactInfo");

            var employeeIds = employeeListDTOs.Select(s => s.EmployeeId);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var ecoms = showContactInfo ? entitiesReadOnly.ContactEcomView.Where(w => w.ActorCompanyId == actorCompanyId && employeeIds.Contains(w.EmployeeId)).ToList() : null; // TODO GDPR logging

            Dictionary<PlanningDayCellType, string> headerDict = new Dictionary<PlanningDayCellType, string>
            {
                { PlanningDayCellType.EmployeeInfo, this.GetReportText(344, "Anställd") },
                { PlanningDayCellType.EmployeeGroup, this.GetReportText(708, "Tidavtal") },
                { PlanningDayCellType.ContactInformation, this.GetReportText(398, "Kontaktuppgifter") },
                { PlanningDayCellType.Absence, this.GetReportText(268, "Frånvaro") }
            };

            var input = new GenericPlanningDayInput(timeSchedulePlanningDays, employeeListDTOs, entitiesReadOnly.ShiftType.Where(w => w.ActorCompanyId == actorCompanyId).ToList(), intervalStartTimes, headerDict);
            input.ContactEcoms = ecoms;
            input.HeaderColumnTypes.Add(PlanningDayCellType.EmployeeInfo);

            if (showEmployeeGroup)
                input.HeaderColumnTypes.Add(PlanningDayCellType.EmployeeGroup);

            if (showContactInfo)
            {
                input.HeaderColumnTypes.Add(PlanningDayCellType.ContactInformation);
                AddContactInfoToEmployees(employeeListDTOs, ecoms, actorCompanyId);
            }

            GenericPlanningDay genericPlanningDay = new GenericPlanningDay(input);
            var matrix = genericPlanningDay.CreateDay();

            return ReportDataManager.StartMatrixGeneric(matrix, actorCompanyId, base.UserId, base.RoleId, this.GetReportText(164, "Schema") + " " + this.GetReportText(158, "Dag"), selections);
        }

        private ActionResult GetMatrixResultFromPlanningDays(int actorCompanyId, List<TimeSchedulePlanningDayDTO> timeSchedulePlanningDays, List<EmployeeListDTO> employeeListDTOs, List<DateTime> dates, List<ReportDataSelectionDTO> selections)
        {
            TryGetBoolFromSelection(selections, out bool showEmployeeGroup, "showEmployeeGroup");
            TryGetBoolFromSelection(selections, out bool showContactInfo, "showContactInfo");

            var employeeIds = employeeListDTOs.Select(s => s.EmployeeId);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var ecoms = showContactInfo ? entitiesReadOnly.ContactEcomView.Where(w => w.ActorCompanyId == actorCompanyId && employeeIds.Contains(w.EmployeeId)).ToList() : null; // TODO GDPR logging

            Dictionary<PlanningDayCellType, string> headerDict = new Dictionary<PlanningDayCellType, string>
            {
                { PlanningDayCellType.EmployeeInfo, this.GetReportText(344, "Anställd") },
                { PlanningDayCellType.EmployeeGroup, this.GetReportText(708, "Tidavtal") },
                { PlanningDayCellType.ContactInformation, this.GetReportText(398, "Kontaktuppgifter") },
                { PlanningDayCellType.Absence, this.GetReportText(268, "Frånvaro") }
            };

            var input = new GenericPlanningDayInput(timeSchedulePlanningDays, employeeListDTOs, entitiesReadOnly.ShiftType.Where(w => w.ActorCompanyId == actorCompanyId).ToList(), dates, headerDict);
            input.ContactEcoms = ecoms;
            input.HeaderColumnTypes.Add(PlanningDayCellType.EmployeeInfo);

            if (showEmployeeGroup)
                input.HeaderColumnTypes.Add(PlanningDayCellType.EmployeeGroup);

            if (showContactInfo)
            {
                input.HeaderColumnTypes.Add(PlanningDayCellType.ContactInformation);
                AddContactInfoToEmployees(employeeListDTOs, ecoms, actorCompanyId);
            }

            GenericPlanningDay genericPlanningDay = new GenericPlanningDay(input);
            var matrix = genericPlanningDay.CreateDays();
            return ReportDataManager.StartMatrixGeneric(matrix, actorCompanyId, base.UserId, base.RoleId, this.GetReportText(164, "Schema") + " " + this.GetReportText(37, "Period"), selections);
        }

        private void AddContactInfoToEmployees(List<EmployeeListDTO> employeeListDTOs, List<ContactEcomView> ecoms, int actorCompanyId)
        {
            (_, List<int> ecomTypeIds) = ContactManager.GetAddressAndEcomTypesForPlanning(actorCompanyId);

            #endregion

            foreach (var employee in employeeListDTOs)
            {
                string value = string.Empty;
                foreach (var item in ecoms.Where(w => w.EmployeeId == employee.EmployeeId && (ecomTypeIds.Contains(w.SysContactEComTypeId))).OrderBy(o => o.SysContactEComTypeId))
                {
                    value += string.IsNullOrEmpty(value) ? item.Text : " " + item.Text;
                }

                employee.Description = value;
            }
        }

        #endregion
    }
}
