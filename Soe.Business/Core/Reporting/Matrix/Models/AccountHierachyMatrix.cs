using SoftOne.Soe.Business.Core.Reporting.Matrix.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Manage;
using SoftOne.Soe.Business.Core.Reporting.Models.Manage.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix.Models
{
    public class AccountHierarchyMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "AccountHierarchyMatrix";
        private List<MatrixDefinitionColumn> DefinitionColumns { get; set; }
        List<AccountHierarchyReportDataField> Filter { get; set; }
        private AccountHierarchyReportDataOutput ReportDataOutput { get; set; }

        #endregion

        public AccountHierarchyMatrix(InputMatrix inputMatrix, AccountHierarchyReportDataOutput reportDataOutput, int actorCompanyId) : base(inputMatrix)
        {
            Filter = reportDataOutput?.Input?.Columns;
            ReportDataOutput = reportDataOutput;
        }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            if (!base.HasReadPermission(Feature.Economy_Accounting_Accounts) || this.inputMatrix.AccountDims.IsNullOrEmpty())
                return possibleColumns;

            foreach (var dim in inputMatrix.AccountDims)
            {
                MatrixDefinitionColumnOptions options = new MatrixDefinitionColumnOptions();
                options.Key = dim.AccountDimId.ToString();
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_AccountHierarchyMatrixColumns.AccountNumbers, options, dim.Name + " " + GetText(1005, "Nummer")));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_AccountHierarchyMatrixColumns.AccountNames, options, dim.Name + " " + GetText(1004, "Namn")));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_AccountHierarchyMatrixColumns.AccountNamesAndNumbers, options, dim.Name + " " + GetText(1005, "Nummer") + " " + GetText(1004, "Namn")));
                if (base.HasReadPermission(Feature.Manage_Users_Edit) && base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments))
                {
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_AccountHierarchyMatrixColumns.AccountExecutiveName, options, dim.Name + " " + GetText(1001, "Chef Namn")));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_AccountHierarchyMatrixColumns.AccountExecutiveEmail, options, dim.Name + " " + GetText(1002, "Chef E-post")));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_AccountHierarchyMatrixColumns.AccountExecutiveUsername, options, dim.Name + " " + GetText(1003, "Chef Användarnamn")));
                }
            }

            return possibleColumns;
        }

        public List<MatrixDefinitionColumn> GetMatrixDefinitionColumns()
        {
            if (DefinitionColumns.IsNullOrEmpty())
            {
                List<MatrixDefinitionColumn> matrixDefinitionColumns = new List<MatrixDefinitionColumn>();

                List<MatrixLayoutColumn> possibleColumns = GetMatrixLayoutColumns();

                if (Filter != null)
                {
                    int columnNumber = 0;
                    foreach (var field in Filter.OrderBy(o => o.Sort))
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

                DefinitionColumns = matrixDefinitionColumns;
            }
            return DefinitionColumns;
        }

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_AccountHierarchyMatrixColumns column, MatrixDefinitionColumnOptions options = null, string overrideTitle = null)
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

            foreach (var accountHierarchyItem in ReportDataOutput.AccountHierarchyItems)
            {
                List<MatrixField> fields = new List<MatrixField>();

                foreach (AccountHierarchyItem item in accountHierarchyItem.Item2)
                {
                    foreach (MatrixDefinitionColumn column in GetMatrixDefinitionColumns())
                    {
                        if (item.AccountField.AccountDimId.ToString() == column.Options.Key)
                            fields.Add(CreateField(item.RowNumber, column, item));
                    }
                }

                if (base.inputMatrix.ExportType == TermGroup_ReportExportType.MatrixExcel)
                    result.MatrixFields.AddRange(fields);
                result.JsonRows.Add(fields.CreateRow(GetMatrixDefinitionColumns()));
                rowNumber++;
            }

            #endregion

            return result;
        }

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, AccountHierarchyItem accountHierarchyItem)
        {
            if (EnumUtility.GetValue(column.Field.FirstCharToUpperCase(), out int id, typeof(TermGroup_AccountHierarchyMatrixColumns), column.Options?.Key == null ? "" : column.Options.Key))
            {
                var type = (TermGroup_AccountHierarchyMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_AccountHierarchyMatrixColumns.AccountNames:
                        return new MatrixField(rowNumber, column.Key, accountHierarchyItem.AccountField.Name, column.MatrixDataType);
                    case TermGroup_AccountHierarchyMatrixColumns.AccountNumbers:
                        return new MatrixField(rowNumber, column.Key, accountHierarchyItem.AccountField.AccountNr, column.MatrixDataType);
                    case TermGroup_AccountHierarchyMatrixColumns.AccountNamesAndNumbers:
                        return new MatrixField(rowNumber, column.Key, accountHierarchyItem.AccountField.AccountNr + " " + accountHierarchyItem.AccountField.Name, column.MatrixDataType);
                    case TermGroup_AccountHierarchyMatrixColumns.AccountExecutiveName:
                        return new MatrixField(rowNumber, column.Key, accountHierarchyItem.AccountField.ExecutiveName, column.MatrixDataType);
                    case TermGroup_AccountHierarchyMatrixColumns.AccountExecutiveEmail:
                        return new MatrixField(rowNumber, column.Key, accountHierarchyItem.AccountField.ExecutiveEmail, column.MatrixDataType);
                    case TermGroup_AccountHierarchyMatrixColumns.AccountExecutiveUsername:
                        return new MatrixField(rowNumber, column.Key, accountHierarchyItem.AccountField.ExecutiveUserName, column.MatrixDataType);

                    default:
                        break;
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}
