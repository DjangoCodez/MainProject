using SoftOne.Soe.Business.Core.Reporting.Matrix.Models;
using SoftOne.Soe.Business.Core.Reporting.Models.Manage;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix
{
    public class ManageMatrixDataManager : BaseMatrixDataManager
    {
        #region Ctor

        public ManageMatrixDataManager(ParameterObject parameterObject) : base(parameterObject) { }

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
                case SoeReportTemplateType.UserAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.UserMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null);
                    return new UserMatrix(inputMatrix, null, ActorCompanyId).GetMatrixLayoutColumns();
                case SoeReportTemplateType.OrganisationHrAnalysis:
                    var extraFieldsAccount = extraFields?.Where(w => w.Entity == SoeEntityType.Account).ToList();
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.OrganisationHrMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null, accountInternals, extraFieldsAccount);
                    return new OrganisationHrMatrix(inputMatrix, null, ActorCompanyId).GetMatrixLayoutColumns();
                case SoeReportTemplateType.ReportStatisticsAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.ReportStatisticsMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null);
                    return new ReportStatisticsMatrix(inputMatrix, null, ActorCompanyId).GetMatrixLayoutColumns();
                case SoeReportTemplateType.SoftOneStatusResultAnalysis:
                    if (!base.IsLicense100())
                        return new List<MatrixLayoutColumn>();
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.SoftOneStatusResultMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null);
                    return new SoftOneStatusResultMatrix(inputMatrix, null, ActorCompanyId).GetMatrixLayoutColumns();
                case SoeReportTemplateType.SoftOneStatusEventAnalysis:
                    if (!base.IsLicense100())
                        return new List<MatrixLayoutColumn>();
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.SoftOneStatusEventMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null);
                    return new SoftOneStatusEventMatrix(inputMatrix, null, ActorCompanyId).GetMatrixLayoutColumns();
                case SoeReportTemplateType.SoftOneStatusUpTimeAnalysis:
                    if (!base.IsLicense100())
                        return new List<MatrixLayoutColumn>();
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.SoftOneStatusUpTimeMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null);
                    return new SoftOneStatusUpTimeMatrix(inputMatrix, null, ActorCompanyId).GetMatrixLayoutColumns();
                case SoeReportTemplateType.LicenseInformationAnalysis:
                    if (!base.IsLicense100())
                        return new List<MatrixLayoutColumn>();
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.LicenseInformationMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null);
                    return new LicenseInformationMatrix(inputMatrix, null, ActorCompanyId).GetMatrixLayoutColumns();
                case SoeReportTemplateType.EmploymentHistoryAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.EmploymentHistoryMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null);
                    return new EmploymentHistoryMatrix(inputMatrix, null).GetMatrixLayoutColumns();
                case SoeReportTemplateType.AccountHierachyAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.AccountHierarchyMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null);
                    return new AccountHierarchyMatrix(inputMatrix, null, ActorCompanyId).GetMatrixLayoutColumns();
                case SoeReportTemplateType.AnnualProgressAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.AnnualProgressMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null);
                    return new AnnualProgressMatrix(inputMatrix, null, ActorCompanyId).GetMatrixLayoutColumns();
                case SoeReportTemplateType.LongtermAbsenceAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.LongtermAbsenceMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null);
                    return new LongtermAbsenceMatrix(inputMatrix, null, ActorCompanyId).GetMatrixLayoutColumns();
                default:
                    break;

            }

            return new List<MatrixLayoutColumn>();
        }

        public List<Insight> GetInsights(SoeReportTemplateType reportTemplateType, int actorCompanyId, int roleId)
        {
            List<Insight> insights = new List<Insight>();

            #region Prereq

            bool hasInsightsPermission = FeatureManager.HasRolePermission(Feature.Time_Insights, Permission.Readonly, roleId, actorCompanyId);

            List<MatrixLayoutColumn> allColumns = GetMatrixLayoutColumns(reportTemplateType);

            #endregion

            #region Custom

            insights.Add(new Insight((int)TermGroup_FixedInsights.Custom, GetText((int)TermGroup_FixedInsights.Custom, (int)TermGroup.FixedInsights, "Egen"), !hasInsightsPermission, allColumns, Insight.GetAllChartTypes()));

            #endregion

            #region Fixed

            switch (reportTemplateType)
            {
                case SoeReportTemplateType.UserAnalysis:
                    #region UserAnalysis

                    insights.Add(new Insight((int)TermGroup_FixedInsights.User_Role, GetText((int)TermGroup_FixedInsights.User_Role, (int)TermGroup.FixedInsights, "Funktionsroll"), false, GetFilteredPossibleColumns(allColumns, new List<TermGroup_UserMatrixColumns>() { TermGroup_UserMatrixColumns.Roles }), SimpleOneColumnChartTypes));
                    insights.Add(new Insight((int)TermGroup_FixedInsights.User_AttestRole, GetText((int)TermGroup_FixedInsights.User_AttestRole, (int)TermGroup.FixedInsights, "Attestroll"), false, GetFilteredPossibleColumns(allColumns, new List<TermGroup_UserMatrixColumns>() { TermGroup_UserMatrixColumns.AttestRoles }), SimpleOneColumnChartTypes));

                    #endregion
                    break;
                case SoeReportTemplateType.OrganisationHrAnalysis:
                    break;
                case SoeReportTemplateType.ReportStatisticsAnalysis:
                    #region ReportStatisticsAnalysis

                    insights.Add(new Insight((int)TermGroup_FixedInsights.ReportStatistics_Reports, GetText((int)TermGroup_FixedInsights.ReportStatistics_Reports, (int)TermGroup.FixedInsights, "Antal utskrivna rapporter"), false, GetFilteredPossibleColumns(allColumns, new List<TermGroup_ReportStatisticsMatrixColumns>() { TermGroup_ReportStatisticsMatrixColumns.ReportName }), SimpleOneColumnChartTypes));

                    List<MatrixLayoutColumn> columnsReportStatistics = GetFilteredPossibleColumns(allColumns, new List<TermGroup_ReportStatisticsMatrixColumns>() { TermGroup_ReportStatisticsMatrixColumns.Period, TermGroup_ReportStatisticsMatrixColumns.ReportName }, true);
                    foreach (MatrixLayoutColumn columnReportStatistics in columnsReportStatistics.OrderBy(a => a.Title))
                    {
                        if (columnReportStatistics != null)
                        {
                            columnReportStatistics.Options.GroupBy = true;
                            break;
                        }
                    }

                    insights.Add(new Insight((int)TermGroup_FixedInsights.ReportStatistics_Reports_Period, GetText((int)TermGroup_FixedInsights.ReportStatistics_Reports_Period, (int)TermGroup.FixedInsights, "Antalet utskrivna rapporter per månad"), !hasInsightsPermission, columnsReportStatistics, BarChartTypes, TermGroup_InsightChartTypes.Column));

                    #endregion
                    break;

            }

            #endregion

            return insights;
        }

        private List<MatrixLayoutColumn> GetFilteredPossibleColumns<T>(List<MatrixLayoutColumn> allColumns, List<T> columns, bool createOptions = false)
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
                        column.Options = new MatrixDefinitionColumnOptions();
                }
            }

            return filteredColumns;
        }

        #endregion

        #region Methods

        public MatrixResult CreateUserData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Data
            List<UserReportDataReportDataField> fields = new List<UserReportDataReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new UserReportDataReportDataField(s)).ToList();

            UserReportData userReportData = new UserReportData(parameterObject, new UserReportDataInput(reportResult, fields));
            UserReportDataOutput output = userReportData.CreateOutput(reportResult);

            #endregion

            #region InputMatrix

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var accountInternals = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.UserMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection, accountInternals);

            #endregion

            #region Create matrix

            var matrix = new UserMatrix(inputMatrix, output, ActorCompanyId);
            var result = matrix.GetMatrixResult();

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return result;
        }

        public MatrixResult CreateOrganisationHrData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Data
            List<OrganisationHrReportDataReportDataField> fields = new List<OrganisationHrReportDataReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new OrganisationHrReportDataReportDataField(s)).ToList();

            OrganisationHrReportData organisationHrReportData = new OrganisationHrReportData(parameterObject, new OrganisationHrReportDataInput(reportResult, fields));
            OrganisationHrReportDataOutput output = organisationHrReportData.CreateOutput(reportResult);

            #endregion

            #region InputMatrix

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var accountInternals = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var extraFields = base.GetExtraFieldsFromCache(entities, CacheConfig.Company(ActorCompanyId, 60));
            var extraFieldsAccount = extraFields?.Where(w => w.Entity == SoeEntityType.Account).ToList();
            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.OrganisationHrMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, null, accountInternals, extraFieldsAccount);

            #endregion

            #region Create matrix

            var matrix = new OrganisationHrMatrix(inputMatrix, output, ActorCompanyId);
            var result = matrix.GetMatrixResult();

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return result;
        }

        public MatrixResult CreateReportStatistics(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);
            List<ReportStatisticsReportDataField> fields = new List<ReportStatisticsReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new ReportStatisticsReportDataField(s)).ToList();

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var accountInternals = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            ReportStatisticsReportData employeeDocumentReportData = new ReportStatisticsReportData(parameterObject, reportDataInput: new ReportStatisticsReportDataInput(reportResult, fields));
            ReportStatisticsReportDataOutput output = employeeDocumentReportData.CreateOutput(reportResult);

            #region InputMatrix

            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.ReportStatisticsMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection, accountInternals);

            #endregion

            #region Create matrix

            var matrix = new ReportStatisticsMatrix(inputMatrix, output, ActorCompanyId);
            var result = matrix.GetMatrixResult();

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return result;

        }

        public MatrixResult CreateAccountHierarchy(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);
            List<AccountHierarchyReportDataField> fields = new List<AccountHierarchyReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new AccountHierarchyReportDataField(s)).ToList();

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var accountInternals = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            AccountHierarchyReportData employeeDocumentReportData = new AccountHierarchyReportData(parameterObject, reportDataInput: new AccountHierarchyReportDataInput(reportResult, fields, accountDims, accountInternals));
            AccountHierarchyReportDataOutput output = employeeDocumentReportData.CreateOutput(reportResult);

            #region InputMatrix

            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.AccountHierarchyMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection, accountInternals);

            #endregion

            #region Create matrix

            var matrix = new AccountHierarchyMatrix(inputMatrix, output, ActorCompanyId);
            var result = matrix.GetMatrixResult();

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return result;

        }

        #endregion

        #region SysAdminMethods

        public MatrixResult CreateSoftOneStatusResult(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Data
            List<SoftOneStatusResultReportDataReportDataField> fields = new List<SoftOneStatusResultReportDataReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new SoftOneStatusResultReportDataReportDataField(s)).ToList();

            SoftOneStatusResultReportData userReportData = new SoftOneStatusResultReportData(parameterObject, new SoftOneStatusResultReportDataInput(reportResult, fields));
            SoftOneStatusResultReportDataOutput output = userReportData.CreateOutput(reportResult);

            #endregion

            #region InputMatrix

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var accountInternals = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.SoftOneStatusResultMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection, accountInternals);

            #endregion

            #region Create matrix

            var matrix = new SoftOneStatusResultMatrix(inputMatrix, output, ActorCompanyId);
            var result = matrix.GetMatrixResult();

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return result;
        }

        public MatrixResult CreateSoftOneStatusEvent(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Data
            List<SoftOneStatusEventReportDataReportDataField> fields = new List<SoftOneStatusEventReportDataReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new SoftOneStatusEventReportDataReportDataField(s)).ToList();

            SoftOneStatusEventReportData userReportData = new SoftOneStatusEventReportData(parameterObject, new SoftOneStatusEventReportDataInput(reportResult, fields));
            SoftOneStatusEventReportDataOutput output = userReportData.CreateOutput(reportResult);

            #endregion

            #region InputMatrix

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var accountInternals = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.SoftOneStatusEventMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection, accountInternals);

            #endregion

            #region Create matrix

            var matrix = new SoftOneStatusEventMatrix(inputMatrix, output, ActorCompanyId);
            var result = matrix.GetMatrixResult();

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return result;
        }

        public MatrixResult CreateSoftOneStatusUpTime(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Data
            List<SoftOneStatusUpTimeReportDataReportDataField> fields = new List<SoftOneStatusUpTimeReportDataReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new SoftOneStatusUpTimeReportDataReportDataField(s)).ToList();

            SoftOneStatusUpTimeReportData userReportData = new SoftOneStatusUpTimeReportData(parameterObject, new SoftOneStatusUpTimeReportDataInput(reportResult, fields));
            SoftOneStatusUpTimeReportDataOutput output = userReportData.CreateOutput(reportResult);

            #endregion

            #region InputMatrix

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var accountInternals = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.SoftOneStatusUpTimeMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection, accountInternals);

            #endregion

            #region Create matrix

            var matrix = new SoftOneStatusUpTimeMatrix(inputMatrix, output, ActorCompanyId);
            var result = matrix.GetMatrixResult();

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return result;
        }

        public MatrixResult CreateLicenseInformation(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Data
            List<LicenseInformationReportDataReportDataField> fields = new List<LicenseInformationReportDataReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new LicenseInformationReportDataReportDataField(s)).ToList();

            LicenseInformationReportData userReportData = new LicenseInformationReportData(parameterObject, new LicenseInformationReportDataInput(reportResult, fields));
            LicenseInformationReportDataOutput output = userReportData.CreateOutput(reportResult);

            #endregion

            #region InputMatrix

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var accountInternals = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.LicenseInformationMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection, accountInternals);

            #endregion

            #region Create matrix

            var matrix = new LicenseInformationMatrix(inputMatrix, output, ActorCompanyId);
            var result = matrix.GetMatrixResult();

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return result;
        }


        #endregion
    }
}
