using SoftOne.Soe.Business.Core.Reporting.Matrix.Models;
using SoftOne.Soe.Business.Core.Reporting.Matrix.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models;
using SoftOne.Soe.Business.Core.Reporting.Models.Time;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix
{
    public class EmployeeAccountMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "EmployeeAccountMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        List<EmployeeAccountReportDataField> filter { get; set; }
        EmployeeAccountReportDataOutput _reportDataOutput { get; set; }
        readonly bool useAccountHierarchy;
        readonly int DefaultEmployeeAccountDimId;
        #endregion

        public EmployeeAccountMatrix(InputMatrix inputMatrix, EmployeeAccountReportDataOutput reportDataOutput, int actorCompanyId, int defaultEmployeeAccountDimId) : base(inputMatrix)
        {
            filter = reportDataOutput?.Input?.Columns;
            _reportDataOutput = reportDataOutput;
            DefaultEmployeeAccountDimId = defaultEmployeeAccountDimId;

            SettingManager sm = new SettingManager(null);
            this.useAccountHierarchy = sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseAccountHierarchy, 0, actorCompanyId, 0);
        }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            if (!base.HasReadPermission(Feature.Manage_Users_Edit))
                return possibleColumns;

            if (base.HasReadPermission(Feature.Common_Categories_Employee))
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_EmployeeAccountMatrixColumns.EmployeeId, new MatrixDefinitionColumnOptions() { Hidden = true }));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeAccountMatrixColumns.EmployeeNr));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeAccountMatrixColumns.EmployeeName));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeAccountMatrixColumns.FirstName));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeAccountMatrixColumns.LastName));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_EmployeeAccountMatrixColumns.DateFrom));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_EmployeeAccountMatrixColumns.FixedAccounting));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeAccountMatrixColumns.Type));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_EmployeeAccountMatrixColumns.Percent));
            }

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeAccountMatrixColumns.AccountInternalStd));

            int nbrOfAccountDims = inputMatrix?.AccountDims?.Count(w => w.AccountDimNr != 1) ?? int.MaxValue;
            if (nbrOfAccountDims > 0 && !this.inputMatrix.AccountDims.IsNullOrEmpty() && base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Accounts))
            {
                foreach (var dim in inputMatrix.AccountDims)
                {
                    MatrixDefinitionColumnOptions options = new MatrixDefinitionColumnOptions();
                    options.Key = dim.AccountDimId.ToString();
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeAccountMatrixColumns.AccountInternalNrs, options, dim.Name + " " + GetText(507, "Nummer")));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeAccountMatrixColumns.AccountInternalNames, options, dim.Name + " " + GetText(508, "Namn")));
                }
            }

            if (this.useAccountHierarchy)
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeAccountMatrixColumns.AccountStdName));
            else
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeAccountMatrixColumns.CategoryName));

            if (base.HasReadPermission(Feature.Common_ExtraFields_Employee) && !inputMatrix.ExtraFields.IsNullOrEmpty())
            {
                foreach (var extraField in inputMatrix.ExtraFields)
                {
                    MatrixDefinitionColumnOptions options = new MatrixDefinitionColumnOptions();
                    options.Key = extraField.ExtraFieldId.ToString();
                    possibleColumns.Add(CreateMatrixLayoutColumn(SetMatrixDataType((TermGroup_ExtraFieldType)extraField.Type), TermGroup_EmployeeAccountMatrixColumns.ExtraFieldEmployee, options, extraField.Text));
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
                    // Hidden
                    foreach (MatrixLayoutColumn item in possibleColumns.Where(c => c.IsHidden()))
                    {
                        matrixDefinitionColumns.Add(CreateMatrixDefinitionColumn(item.MatrixDataType, item.Field, item.Title, item.Options));
                    }

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

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_EmployeeAccountMatrixColumns column, MatrixDefinitionColumnOptions options = null, string overrideTitle = null)
        {
            MatrixLayoutColumn matrixLayoutColumn = new MatrixLayoutColumn(dataType, EnumUtility.GetName(column), string.IsNullOrEmpty(overrideTitle) ? GetText((int)column, EnumUtility.GetName(column)) : overrideTitle, options);
            if (IsAccountInternalStd(column))
            {
                var name = GetAccountInternalStdName(column);
                if (!string.IsNullOrEmpty(name))
                    matrixLayoutColumn.Title = name;
            }
            else if (IsAccountStd(column))
            {
                var name = GetAccountStdName(column, DefaultEmployeeAccountDimId);
                if (!string.IsNullOrEmpty(name))
                    matrixLayoutColumn.Title = GetText((int)TermGroup_EmployeeAccountMatrixColumns.Default, "Standard") + " " + name;
            }
            else if (IsAccountInternal(column))
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

            foreach (var employeeDocumentItem in _reportDataOutput.EmployeeAccountItems)
            {
                List<MatrixField> fields = new List<MatrixField>();

                foreach (MatrixDefinitionColumn column in GetMatrixDefinitionColumns())
                    fields.Add(CreateField(rowNumber, column, employeeDocumentItem));

                if (base.inputMatrix.ExportType == TermGroup_ReportExportType.MatrixExcel)
                    result.MatrixFields.AddRange(fields);
                result.JsonRows.Add(fields.CreateRow(GetMatrixDefinitionColumns()));
                rowNumber++;
            }

            #endregion

            return result;
        }

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, EmployeeAccountItem employeeAccountItem)
        {
            if (base.GetEnumId<TermGroup_EmployeeAccountMatrixColumns>(column, out int id))
            {
                var type = (TermGroup_EmployeeAccountMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_EmployeeAccountMatrixColumns.EmployeeId:
                        return new MatrixField(rowNumber, column.Key, employeeAccountItem.EmployeeId, column.MatrixDataType);
                    case TermGroup_EmployeeAccountMatrixColumns.EmployeeName:
                        return new MatrixField(rowNumber, column.Key, employeeAccountItem.EmployeeName, column.MatrixDataType);
                    case TermGroup_EmployeeAccountMatrixColumns.FirstName:
                        return new MatrixField(rowNumber, column.Key, employeeAccountItem.FirstName, column.MatrixDataType);
                    case TermGroup_EmployeeAccountMatrixColumns.LastName:
                        return new MatrixField(rowNumber, column.Key, employeeAccountItem.LastName, column.MatrixDataType);
                    case TermGroup_EmployeeAccountMatrixColumns.EmployeeNr:
                        return new MatrixField(rowNumber, column.Key, employeeAccountItem.EmployeeNr, column.MatrixDataType);
                    case TermGroup_EmployeeAccountMatrixColumns.DateFrom:
                        return new MatrixField(rowNumber, column.Key, employeeAccountItem.DateFrom, column.MatrixDataType);
                    case TermGroup_EmployeeAccountMatrixColumns.FixedAccounting:
                        return new MatrixField(rowNumber, column.Key, employeeAccountItem.FixedAccounting, column.MatrixDataType);
                    case TermGroup_EmployeeAccountMatrixColumns.Type:
                        return new MatrixField(rowNumber, column.Key, employeeAccountItem.Type, column.MatrixDataType);
                    case TermGroup_EmployeeAccountMatrixColumns.Percent:
                        return new MatrixField(rowNumber, column.Key, employeeAccountItem.Percent, column.MatrixDataType);
                    case TermGroup_EmployeeAccountMatrixColumns.CategoryName:
                        return new MatrixField(rowNumber, column.Key, employeeAccountItem.CategoryName, column.MatrixDataType);
                    case TermGroup_EmployeeAccountMatrixColumns.AccountStdName:
                        return new MatrixField(rowNumber, column.Key, employeeAccountItem.AccountStdName, column.MatrixDataType);
                    case TermGroup_EmployeeAccountMatrixColumns.AccountInternalStd:
                        return new MatrixField(rowNumber, column.Key, employeeAccountItem.AccountInternalStd, column.MatrixDataType);
                    case TermGroup_EmployeeAccountMatrixColumns.AccountInternalNames:
                        return new MatrixField(rowNumber, column.Key, employeeAccountItem.AccountAnalysisFields.GetAccountAnalysisFieldValueName(column), column.MatrixDataType);
                    case TermGroup_EmployeeAccountMatrixColumns.AccountInternalNrs:
                        return new MatrixField(rowNumber, column.Key, employeeAccountItem.AccountAnalysisFields.GetAccountAnalysisFieldValueNumber(column), column.MatrixDataType);
                    case TermGroup_EmployeeAccountMatrixColumns.ExtraFieldEmployee:
                        return new MatrixField(rowNumber, column.Key, employeeAccountItem.ExtraFieldAnalysisFields.ExtraFieldAnalysisFieldValue(column), column.MatrixDataType);
                    default:
                        break;
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}
