using SoftOne.Soe.Business.Core.Reporting.Matrix.Models;
using SoftOne.Soe.Business.Core.Reporting.Matrix.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Manage;
using SoftOne.Soe.Business.Core.Reporting.Models.Manage.Models;
using SoftOne.Soe.Business.Core.Reporting.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix
{
    public class OrganisationHrMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "OrganisationHrMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        List<OrganisationHrReportDataReportDataField> filter { get; set; }
        OrganisationHrReportDataOutput _reportDataOutput { get; set; }
        List<AccountDimDTO> AccountDims { get; set; }
        List<AccountDTO> AccountInternals { get; set; }
        readonly bool useAccountHierarchy;
        #endregion

        public OrganisationHrMatrix(InputMatrix inputMatrix, OrganisationHrReportDataOutput reportDataOutput, int actorCompanyId) : base(inputMatrix)
        {
            filter = reportDataOutput?.Input?.Columns;
            _reportDataOutput = reportDataOutput;
            AccountDims = inputMatrix.AccountDims;
            AccountInternals = inputMatrix.AccountInternals;

            SettingManager sm = new SettingManager(null);
            this.useAccountHierarchy = sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseAccountHierarchy, 0, actorCompanyId, 0);
        }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            if (!base.HasReadPermission(Feature.Manage_Users_Edit))
                return possibleColumns;

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_OrganisationHrMatrixColumns.EmployeeNr));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_OrganisationHrMatrixColumns.Name));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_OrganisationHrMatrixColumns.FirstName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_OrganisationHrMatrixColumns.LastName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_OrganisationHrMatrixColumns.DateFrom));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_OrganisationHrMatrixColumns.DateTo));
            if (useAccountHierarchy)
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_OrganisationHrMatrixColumns.AccountIsPrimary));
            }
            else
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_OrganisationHrMatrixColumns.CategoryIsDefault));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_OrganisationHrMatrixColumns.CategoryName));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_OrganisationHrMatrixColumns.CategoryGroup));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_OrganisationHrMatrixColumns.SubCategory));
            }

            int nbrOfAccountDims = inputMatrix?.AccountDims?.Count(w => w.AccountDimNr != 1) ?? 0;
            if (nbrOfAccountDims > 0 && base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Accounts))
            {
                foreach (var dim in inputMatrix.AccountDims)
                {
                    MatrixDefinitionColumnOptions options = new MatrixDefinitionColumnOptions();
                    options.Key = dim.AccountDimId.ToString();
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_OrganisationHrMatrixColumns.AccountInternalNrs, options, dim.Name + " " + GetText(507, "Nummer")));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_OrganisationHrMatrixColumns.AccountInternalNames, options, dim.Name + " " + GetText(508, "Namn")));
                }
            }

            if (base.HasReadPermission(Feature.Common_ExtraFields_Account) && !inputMatrix.ExtraFields.IsNullOrEmpty())
            {
                foreach (var extraField in inputMatrix.ExtraFields)
                {
                    var accountDim = inputMatrix?.AccountDims.FirstOrDefault(w => w.AccountDimId == extraField.ConnectedRecordId);
                    MatrixDefinitionColumnOptions options = new MatrixDefinitionColumnOptions();
                    options.Key = extraField.ExtraFieldId.ToString();
                    possibleColumns.Add(CreateMatrixLayoutColumn(SetMatrixDataType((TermGroup_ExtraFieldType)extraField.Type), TermGroup_OrganisationHrMatrixColumns.ExtraFieldAccount, options, (accountDim != null ? accountDim.Name + "-" + extraField.Text : extraField.Text)));
                }
            }

            return possibleColumns;
        }

        public List<MatrixDefinitionColumn> GetMatrixDefinitionColumns()
        {
            if (definitionColumns.IsNullOrEmpty())
            {
                List<MatrixDefinitionColumn> matrixDefinitionColumns = new List<MatrixDefinitionColumn>();

                List<MatrixLayoutColumn> possibleColumns = GetMatrixLayoutColumns();

                if (filter != null)
                {
                    int columnNumber = 0;
                    foreach (var field in filter.OrderBy(o => o.Sort))
                    {
                        MatrixLayoutColumn item = possibleColumns.FirstOrDefault(w => w.Field == field.ColumnKey);

                        if (item != null)
                        {
                            columnNumber++;
                            matrixDefinitionColumns.Add(CreateMatrixDefinitionColumn(item, columnNumber, field.Selection?.Options != null ? field.Selection.Options : item.Options));
                        }
                    }
                }
                else
                {
                    foreach (MatrixLayoutColumn item in possibleColumns)
                    {
                        matrixDefinitionColumns.Add(CreateMatrixDefinitionColumn(item.MatrixDataType, item.Field, item.Title, item.Options));
                    }
                }

                definitionColumns = matrixDefinitionColumns;
            }
            return definitionColumns;
        }

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_OrganisationHrMatrixColumns column, MatrixDefinitionColumnOptions options = null, string overrideTitle = null)
        {
            MatrixLayoutColumn matrixLayoutColumn = new MatrixLayoutColumn(dataType, EnumUtility.GetName(column), string.IsNullOrEmpty(overrideTitle) ? GetText((int)column, EnumUtility.GetName(column)) : overrideTitle, options);
            if (IsAccountInternal(column))
            {
                var name = GetAccountInternalName(column, 1);
                if (!string.IsNullOrEmpty(name))
                    matrixLayoutColumn.Title = name;
            }

            return matrixLayoutColumn;
        }

        public MatrixResult GetMatrixResult()
        {
            MatrixResult result = new MatrixResult();
            result.MatrixDefinition = new MatrixDefinition() { MatrixDefinitionColumns = GetMatrixDefinitionColumns() };

            #region Create matrix

            int rowNumber = 1;

            foreach (var userItem in _reportDataOutput.OrganisationHrItems)
            {
                List<MatrixField> fields = new List<MatrixField>();

                foreach (MatrixDefinitionColumn column in GetMatrixDefinitionColumns())
                    fields.Add(CreateField(rowNumber, column, userItem));

                if (base.inputMatrix.ExportType == TermGroup_ReportExportType.MatrixExcel)
                    result.MatrixFields.AddRange(fields);
                result.JsonRows.Add(fields.CreateRow(GetMatrixDefinitionColumns()));
                rowNumber++;
            }

            #endregion

            return result;
        }

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, OrganisationHrItem orgItem)
        {
            if (base.GetEnumId<TermGroup_OrganisationHrMatrixColumns>(column, out int id))
            {
                var type = (TermGroup_OrganisationHrMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_OrganisationHrMatrixColumns.FirstName:
                        return new MatrixField(rowNumber, column.Key, orgItem.FirstName, column.MatrixDataType);
                    case TermGroup_OrganisationHrMatrixColumns.LastName:
                        return new MatrixField(rowNumber, column.Key, orgItem.LastName, column.MatrixDataType);
                    case TermGroup_OrganisationHrMatrixColumns.Name:
                        return new MatrixField(rowNumber, column.Key, orgItem.Name, column.MatrixDataType);
                    case TermGroup_OrganisationHrMatrixColumns.EmployeeNr:
                        return new MatrixField(rowNumber, column.Key, orgItem.EmployeeNr, column.MatrixDataType);
                    case TermGroup_OrganisationHrMatrixColumns.DateFrom:
                        return new MatrixField(rowNumber, column.Key, orgItem.DateFrom, column.MatrixDataType);
                    case TermGroup_OrganisationHrMatrixColumns.DateTo:
                        return new MatrixField(rowNumber, column.Key, orgItem.DateTo, column.MatrixDataType);
                    case TermGroup_OrganisationHrMatrixColumns.CategoryIsDefault:
                        return new MatrixField(rowNumber, column.Key, orgItem.CategoryIsDefault, column.MatrixDataType);
                    case TermGroup_OrganisationHrMatrixColumns.AccountIsPrimary:
                        return new MatrixField(rowNumber, column.Key, orgItem.AccountIsPrimary, column.MatrixDataType);
                    case TermGroup_OrganisationHrMatrixColumns.CategoryName:
                        return new MatrixField(rowNumber, column.Key, orgItem.CategoryName, column.MatrixDataType);
                    case TermGroup_OrganisationHrMatrixColumns.CategoryGroup:
                        return new MatrixField(rowNumber, column.Key, orgItem.CategoryGroup, column.MatrixDataType);
                    case TermGroup_OrganisationHrMatrixColumns.SubCategory:
                        return new MatrixField(rowNumber, column.Key, orgItem.SubCategory, column.MatrixDataType);
                    case TermGroup_OrganisationHrMatrixColumns.AccountInternalNames:
                        return new MatrixField(rowNumber, column.Key, orgItem.AccountAnalysisFields.GetAccountAnalysisFieldValueName(column), column.MatrixDataType);
                    case TermGroup_OrganisationHrMatrixColumns.AccountInternalNrs:
                        return new MatrixField(rowNumber, column.Key, orgItem.AccountAnalysisFields.GetAccountAnalysisFieldValueNumber(column), column.MatrixDataType);
                    case TermGroup_OrganisationHrMatrixColumns.ExtraFieldAccount:
                        return new MatrixField(rowNumber, column.Key, orgItem.ExtraFieldAnalysisFields.ExtraFieldAnalysisFieldValue(column), column.MatrixDataType);

                    default:
                        break;
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}
