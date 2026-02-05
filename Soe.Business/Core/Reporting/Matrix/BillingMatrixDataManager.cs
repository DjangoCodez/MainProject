using SoftOne.Soe.Business.Core.Reporting.Matrix.Models;
using SoftOne.Soe.Business.Core.Reporting.Models.Billing;
using SoftOne.Soe.Business.Core.Reporting.Models.Time;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix
{
    public class BillingMatrixDataManager : BaseMatrixDataManager
    {
        #region Ctor

        public BillingMatrixDataManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Common

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns(SoeReportTemplateType reportTemplateType)
        {
            InputMatrix inputMatrix;

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));

            switch (reportTemplateType)
            {
                case SoeReportTemplateType.InvoiceProductAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.InvoiceProductMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null);
                    return new InvoiceProductMatrix(inputMatrix, null).GetMatrixLayoutColumns();
                case SoeReportTemplateType.OrderAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.OrderAnalysisMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null);
                    return new OrderMatrix(inputMatrix, null).GetMatrixLayoutColumns();
                case SoeReportTemplateType.InvoiceAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.InvoiceAnalysisMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null);
                    return new InvoiceMatrix(inputMatrix, null).GetMatrixLayoutColumns();
                case SoeReportTemplateType.InvoiceProductUnitConvertAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.InvoiceProductUnitConvertMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null);
                    return new InvoiceProductUnitConvertMatrix(inputMatrix, null).GetMatrixLayoutColumns();
            }

            return new List<MatrixLayoutColumn>();
        }

        public List<Insight> GetInsights(SoeReportTemplateType reportTemplateType, int actorCompanyId, int roleId)
        {
            List<Insight> insights = new List<Insight>();

            #region Prereq

            bool hasInsightsPermission = FeatureManager.HasRolePermission(Feature.Billing_Insights, Permission.Readonly, roleId, actorCompanyId);

            List<MatrixLayoutColumn> allColumns = GetMatrixLayoutColumns(reportTemplateType);        

            #endregion

            #region Custom

            insights.Add(new Insight((int)TermGroup_FixedInsights.Custom, GetText((int)TermGroup_FixedInsights.Custom, (int)TermGroup.FixedInsights, "Egen"), !hasInsightsPermission, allColumns, Insight.GetAllChartTypes()));

            #endregion

            #region Fixed

            switch (reportTemplateType)
            {
                case SoeReportTemplateType.InvoiceProductAnalysis:
                    #region InvoiceProductAnalysis

                    insights.Add(new Insight((int)TermGroup_FixedInsights.InvoiceProduct_ProductType, GetText((int)TermGroup_FixedInsights.InvoiceProduct_ProductType, (int)TermGroup.FixedInsights, "Artikeltyp"), false, GetFilteredPossibleColumns(allColumns, new List<TermGroup_InvoiceProductMatrixColumns>() { TermGroup_InvoiceProductMatrixColumns.ProductType }), SimpleOneColumnChartTypes, TermGroup_InsightChartTypes.Pie));
                    insights.Add(new Insight((int)TermGroup_FixedInsights.InvoiceProduct_VatCode, GetText((int)TermGroup_FixedInsights.InvoiceProduct_VatCode, (int)TermGroup.FixedInsights, "Momskod"), !hasInsightsPermission, GetFilteredPossibleColumns(allColumns, new List<TermGroup_InvoiceProductMatrixColumns>() { TermGroup_InvoiceProductMatrixColumns.VatCodeName }), SimpleOneColumnChartTypes, TermGroup_InsightChartTypes.Pie));

                    #endregion
                    break;
                case SoeReportTemplateType.EmploymentHistoryAnalysis:
                    #region EmploymentHistoryAnalysis

                    //insights.Add(new Insight((int)TermGroup_FixedInsights.InvoiceProduct_ProductType, GetText((int)TermGroup_FixedInsights.InvoiceProduct_ProductType, (int)TermGroup.FixedInsights, "Artikeltyp"), false, GetFilteredPossibleColumns(allColumns, new List<TermGroup_InvoiceProductMatrixColumns>() { TermGroup_InvoiceProductMatrixColumns.ProductType }), SimpleOneColumnChartTypes, TermGroup_InsightChartTypes.Pie));
                    //insights.Add(new Insight((int)TermGroup_FixedInsights.InvoiceProduct_VatCode, GetText((int)TermGroup_FixedInsights.InvoiceProduct_VatCode, (int)TermGroup.FixedInsights, "Momskod"), !hasInsightsPermission, GetFilteredPossibleColumns(allColumns, new List<TermGroup_InvoiceProductMatrixColumns>() { TermGroup_InvoiceProductMatrixColumns.VatCodeName }), SimpleOneColumnChartTypes, TermGroup_InsightChartTypes.Pie));

                    #endregion
                    break;
                case SoeReportTemplateType.OrderAnalysis:
                    #region OrderAnalysis

                    //insights.Add(new Insight((int)TermGroup_FixedInsights.InvoiceProduct_ProductType, GetText((int)TermGroup_FixedInsights.InvoiceProduct_ProductType, (int)TermGroup.FixedInsights, "Artikeltyp"), false, GetFilteredPossibleColumns(allColumns, new List<TermGroup_OrderAnalysisMatrixColumns>() { TermGroup_OrderAnalysisMatrixColumns.ProductType }), SimpleOneColumnChartTypes, TermGroup_InsightChartTypes.Pie));
                    //insights.Add(new Insight((int)TermGroup_FixedInsights.InvoiceProduct_VatCode, GetText((int)TermGroup_FixedInsights.InvoiceProduct_VatCode, (int)TermGroup.FixedInsights, "Momskod"), !hasInsightsPermission, GetFilteredPossibleColumns(allColumns, new List<TermGroup_OrderAnalysisMatrixColumns>() { TermGroup_OrderAnalysisMatrixColumns.VatCodeName }), SimpleOneColumnChartTypes, TermGroup_InsightChartTypes.Pie));

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
        public MatrixResult CreateInvoiceProductUnitConvertData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);

            #endregion

            #region Data
            var fields = new List<InvoiceProductUnitConvertReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new InvoiceProductUnitConvertReportDataField(s)).ToList();

            var userReportData = new InvoiceProductUnitConvertReportData(parameterObject, new InvoiceProductUnitConvertReportDataInput(reportResult, fields));
            var output = userReportData.CreateOutput(reportResult);

            #endregion

            #region InputMatrix

            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.InvoiceProductUnitConvertMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, null, matrixColumnsSelection, null);

            #endregion


            #region Create matrix

            var matrix = new InvoiceProductUnitConvertMatrix(inputMatrix, output);
            var result = matrix.GetMatrixResult();

            #endregion

            return result;
        }
        public MatrixResult CreateInvoiceProductData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);

            #endregion


            #region Data
            List<InvoiceProductReportDataReportDataField> fields = new List<InvoiceProductReportDataReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new InvoiceProductReportDataReportDataField(s)).ToList();

            InvoiceProductReportData userReportData = new InvoiceProductReportData(parameterObject, new InvoiceProductReportDataInput(reportResult, fields));
            InvoiceProductReportDataOutput output = userReportData.CreateOutput(reportResult);

            #endregion

            #region InputMatrix

            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.InvoiceProductMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, null, matrixColumnsSelection, null);

            #endregion

            #region Create matrix

            var matrix = new InvoiceProductMatrix(inputMatrix, output);
            var result = matrix.GetMatrixResult();

            #endregion

            return result;
        }

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
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var accountInternals = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(ActorCompanyId));
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

        public MatrixResult CreateOrderData(CreateReportResult reportResult) 
        {
            base.reportResult = reportResult;

            #region Prereq

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Data
            List<OrderReportDataField> fields = new List<OrderReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new OrderReportDataField(s)).ToList();

            OrderReportData userReportData = new OrderReportData(parameterObject, new OrderReportDataInput(reportResult, fields));
            OrderReportDataOutput output = userReportData.CreateOutput(reportResult);

            #endregion

            #region InputMatrix

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var accountInternals = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.OrderAnalysisMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection, accountInternals);

            #endregion

            #region Create matrix

            var matrix = new OrderMatrix(inputMatrix, output);
            var result = matrix.GetMatrixResult();

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return result;
        }

        public MatrixResult CreateInvoiceData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);

            #endregion

            #region Data
            var fields = new List<InvoiceReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new InvoiceReportDataField(s)).ToList();

            var userReportData = new InvoiceReportData(parameterObject, new InvoiceReportDataInput(reportResult, fields));
            InvoiceReportDataOutput output = userReportData.CreateOutput(reportResult);

            #endregion

            #region InputMatrix

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var accountInternals = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.InvoiceAnalysisMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection, accountInternals);

            #endregion

            #region Create matrix

            var matrix = new InvoiceMatrix(inputMatrix, output);
            var result = matrix.GetMatrixResult();

            #endregion

            return result;
        }

        #endregion
    }


}
