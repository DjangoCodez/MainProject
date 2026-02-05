using SoftOne.Soe.Business.Core.Reporting.Matrix.Models;
using SoftOne.Soe.Business.Core.Reporting.Models.Economy;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix
{
    public class EconomyMatrixDataManager : BaseMatrixDataManager
    {
        #region Ctor

        public EconomyMatrixDataManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Common

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns(SoeReportTemplateType reportTemplateType)
        {
            InputMatrix inputMatrix;

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));

            switch (reportTemplateType)
            {
                case SoeReportTemplateType.SupplierAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.SupplierMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null);
                    return new SupplierMatrix(inputMatrix, null).GetMatrixLayoutColumns();
                case SoeReportTemplateType.CustomerAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.CustomerMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null);
                    return new CustomerMatrix(inputMatrix, null).GetMatrixLayoutColumns();
                case SoeReportTemplateType.InventoryAnalysis:
                    inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.InventoryMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null);
                    return new InventoryMatrix(inputMatrix, null).GetMatrixLayoutColumns();
				case SoeReportTemplateType.DepreciationAnalysis:
					inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.DepreciationMatrixColumns), base.GetPermissionRepository(), TermGroup_ReportExportType.MatrixGrid, accountDims, null);
                    return new DepreciationMatrix(inputMatrix, null).GetMatrixLayoutColumns();
            }

            return new List<MatrixLayoutColumn>();
        }

        public List<Insight> GetInsights(SoeReportTemplateType reportTemplateType, int actorCompanyId, int roleId)
        {
            List<Insight> insights = new List<Insight>();

            #region Prereq

            bool hasInsightsPermission = FeatureManager.HasRolePermission(Feature.Economy_Insights, Permission.Readonly, roleId, actorCompanyId);

            List<MatrixLayoutColumn> allColumns = GetMatrixLayoutColumns(reportTemplateType);           

            #endregion

            #region Custom

            insights.Add(new Insight((int)TermGroup_FixedInsights.Custom, GetText((int)TermGroup_FixedInsights.Custom, (int)TermGroup.FixedInsights, "Egen"), !hasInsightsPermission, allColumns, Insight.GetAllChartTypes()));

            #endregion

            #region Fixed

            switch (reportTemplateType)
            {
                case SoeReportTemplateType.SupplierAnalysis:
                    #region SupplierAnalysis
                   
                    insights.Add(new Insight((int)TermGroup_FixedInsights.Supplier_Country, GetText((int)TermGroup_FixedInsights.Supplier_Country, (int)TermGroup.FixedInsights, "Land"), !hasInsightsPermission, GetFilteredPossibleColumns(allColumns, new List<TermGroup_SupplierMatrixColumns>() { TermGroup_SupplierMatrixColumns.Country }), SimpleOneColumnChartTypes));
                    insights.Add(new Insight((int)TermGroup_FixedInsights.Supplier_VatType, GetText((int)TermGroup_FixedInsights.Supplier_VatType, (int)TermGroup.FixedInsights, "Momstyp"), false, GetFilteredPossibleColumns(allColumns, new List<TermGroup_SupplierMatrixColumns>() { TermGroup_SupplierMatrixColumns.VatType }), SimpleOneColumnChartTypes));

                    break;
                    #endregion
                case SoeReportTemplateType.CustomerAnalysis:
                    #region CustomerAnalysis
                   
                    insights.Add(new Insight((int)TermGroup_FixedInsights.Customer_VisitingPostalAdressByCustomer, GetText((int)TermGroup_FixedInsights.Customer_VisitingPostalAdressByCustomer, (int)TermGroup.FixedInsights, "Kunder baserade på besöksort"), false, GetFilteredPossibleColumns(allColumns, new List<TermGroup_CustomerMatrixColumns>() { TermGroup_CustomerMatrixColumns.VisitingAddressPostalAddress }), SimpleOneColumnChartTypes));

                    break;
                    #endregion
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

        public MatrixResult CreateSupplierData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Data
            List<SupplierReportDataReportDataField> fields = new List<SupplierReportDataReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new SupplierReportDataReportDataField(s)).ToList();

            SupplierReportData supplierReportData = new SupplierReportData(parameterObject, new SupplierReportDataInput(reportResult, fields));
            SupplierReportDataOutput output = supplierReportData.CreateOutput(reportResult);

            #endregion

            #region InputMatrix

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var accountInternals = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.SupplierMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection, accountInternals);

            #endregion

            #region Create matrix

            var matrix = new SupplierMatrix(inputMatrix, output);
            var result = matrix.GetMatrixResult();

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return result;
        }
        public MatrixResult CreateCustomerData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Data
            List<CustomerReportDataReportDataField> fields = new List<CustomerReportDataReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new CustomerReportDataReportDataField(s)).ToList();

            CustomerReportData customerReportData = new CustomerReportData(parameterObject, new CustomerReportDataInput(reportResult, fields));
            CustomerReportDataOutput output = customerReportData.CreateOutput(reportResult);

            #endregion

            #region InputMatrix

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var accountInternals = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.CustomerMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection, accountInternals);

            #endregion

            #region Create matrix

            var matrix = new CustomerMatrix(inputMatrix, output);
            var result = matrix.GetMatrixResult();

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return result;
        }
        public MatrixResult CreateInventoryData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Data
            List<InventoryReportDataReportDataField> fields = new List<InventoryReportDataReportDataField>();
            if (matrixColumnsSelection?.Columns != null)
                fields = matrixColumnsSelection.Columns.Select(s => new InventoryReportDataReportDataField(s)).ToList();

            InventoryReportData inventoryReportData = new InventoryReportData(parameterObject, new InventoryReportDataInput(reportResult, fields));
            InventoryReportDataOutput output = inventoryReportData.CreateOutput(reportResult);

            #endregion

            #region InputMatrix

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            var accountInternals = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(ActorCompanyId));
            InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.InventoryMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, accountDims, matrixColumnsSelection, accountInternals);

            #endregion

            #region Create matrix

            var matrix = new InventoryMatrix(inputMatrix, output);
            var result = matrix.GetMatrixResult();

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return result;
        }
		public MatrixResult CreateDepreciationData(CreateReportResult reportResult)
		{
			base.reportResult = reportResult;
			MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);
			base.InitPersonalDataEmployeeReportRepository();
			List<DepreciationReportDataReportDataField> fields = new List<DepreciationReportDataReportDataField>();
			if (matrixColumnsSelection?.Columns != null)
				fields = matrixColumnsSelection.Columns.Select(s => new DepreciationReportDataReportDataField(s)).ToList();

			DepreciationReportData depreciationReportData = new DepreciationReportData(parameterObject, new DepreciationReportDataInput(reportResult, fields));
			DepreciationReportDataOutput output = depreciationReportData.CreateOutput(reportResult);

			InputMatrix inputMatrix = new InputMatrix(GetTermGroupContent(TermGroup.DepreciationMatrixColumns), base.GetPermissionRepository(), reportResult.ExportType, null, matrixColumnsSelection, null);

			var matrix = new DepreciationMatrix(inputMatrix, output);
			var result = matrix.GetMatrixResult();

			base.personalDataRepository.GenerateLogs();

			return result;
		}
		#endregion
	}

}
