using SoftOne.Soe.Business.Core.Reporting.Matrix.Models;
using SoftOne.Soe.Business.Core.Reporting.Matrix.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Time;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix
{
    public class EmploymentHistoryMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "EmploymentHistoryMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        List<EmploymentHistoryReportDataField> filter { get; set; }
        EmploymentHistoryReportDataOutput _reportDataOutput { get; set; }

        #endregion

        public EmploymentHistoryMatrix(InputMatrix inputMatrix, EmploymentHistoryReportDataOutput reportDataOutput) : base(inputMatrix)
        {
            filter = reportDataOutput?.Input?.Columns;
            _reportDataOutput = reportDataOutput;
        }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            bool employeesEditOtherEmployeesHasPermission = base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees);
            bool employeesEmploymentsEditHasPermission = base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments);           
            if (!employeesEditOtherEmployeesHasPermission && !employeesEmploymentsEditHasPermission)
                return possibleColumns;

            if (employeesEditOtherEmployeesHasPermission)
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmploymentHistoryMatrixColumns.EmploymentNumber));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmploymentHistoryMatrixColumns.FirstName));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmploymentHistoryMatrixColumns.LastName));
            }
            if (employeesEmploymentsEditHasPermission)
            {
                var percentageColDef = new MatrixDefinitionColumnOptions();
                percentageColDef.Decimals = 2;
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_EmploymentHistoryMatrixColumns.EmploymentPercentage, percentageColDef));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmploymentHistoryMatrixColumns.WorkingPlace));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_EmploymentHistoryMatrixColumns.EmploymentStartDate));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_EmploymentHistoryMatrixColumns.EmploymentEndDate));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmploymentHistoryMatrixColumns.EmploymentType));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmploymentHistoryMatrixColumns.EmploymentEndReason));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_EmploymentHistoryMatrixColumns.TotalEmploymentDays));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_EmploymentHistoryMatrixColumns.LASDays));
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

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_EmploymentHistoryMatrixColumns column, MatrixDefinitionColumnOptions options = null)
        {
            MatrixLayoutColumn matrixLayoutColumn = new MatrixLayoutColumn(dataType, EnumUtility.GetName(column), GetText((int)column, EnumUtility.GetName(column)), options);
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

            foreach (var product in _reportDataOutput.EmploymentHistoryItems)
            {
                List<MatrixField> fields = new List<MatrixField>();

                foreach (MatrixDefinitionColumn column in GetMatrixDefinitionColumns())
                    fields.Add(CreateField(rowNumber, column, product));

                if (base.inputMatrix.ExportType == TermGroup_ReportExportType.MatrixExcel)
                    result.MatrixFields.AddRange(fields);
                result.JsonRows.Add(fields.CreateRow(GetMatrixDefinitionColumns()));
                rowNumber++;
            }

            #endregion

            return result;
        }

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, EmploymentHistoryItem EmploymentHistoryItem)
        {
            if (EnumUtility.GetValue(column.Field.FirstCharToUpperCase(), out int id, typeof(TermGroup_EmploymentHistoryMatrixColumns)))
            {
                var type = (TermGroup_EmploymentHistoryMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_EmploymentHistoryMatrixColumns.EmploymentNumber:
                        return new MatrixField(rowNumber, column.Key, EmploymentHistoryItem.EmploymentNumber, column.MatrixDataType);
                    case TermGroup_EmploymentHistoryMatrixColumns.FirstName:
                        return new MatrixField(rowNumber, column.Key, EmploymentHistoryItem.FirstName, column.MatrixDataType);
                    case TermGroup_EmploymentHistoryMatrixColumns.LastName:
                        return new MatrixField(rowNumber, column.Key, EmploymentHistoryItem.LastName, column.MatrixDataType);
                    case TermGroup_EmploymentHistoryMatrixColumns.EmploymentPercentage:
                        return new MatrixField(rowNumber, column.Key, EmploymentHistoryItem.EmploymentPercentage, column.MatrixDataType);
                    case TermGroup_EmploymentHistoryMatrixColumns.WorkingPlace:
                        return new MatrixField(rowNumber, column.Key, EmploymentHistoryItem.WorkingPlace, column.MatrixDataType);
                    case TermGroup_EmploymentHistoryMatrixColumns.EmploymentStartDate:
                        return new MatrixField(rowNumber, column.Key, EmploymentHistoryItem.EmploymentStartDate, column.MatrixDataType);
                    case TermGroup_EmploymentHistoryMatrixColumns.EmploymentEndDate:
                        return new MatrixField(rowNumber, column.Key, EmploymentHistoryItem.EmploymentEndDate, column.MatrixDataType);
                    case TermGroup_EmploymentHistoryMatrixColumns.EmploymentType:
                        return new MatrixField(rowNumber, column.Key, EmploymentHistoryItem.EmploymentType, column.MatrixDataType);
                    case TermGroup_EmploymentHistoryMatrixColumns.EmploymentEndReason:
                        return new MatrixField(rowNumber, column.Key, EmploymentHistoryItem.ReasonForEndingEmployment, column.MatrixDataType);
                    case TermGroup_EmploymentHistoryMatrixColumns.TotalEmploymentDays:
                        return new MatrixField(rowNumber, column.Key, EmploymentHistoryItem.TotalEmploymentDays, column.MatrixDataType);
                    case TermGroup_EmploymentHistoryMatrixColumns.LASDays:
                        return new MatrixField(rowNumber, column.Key, EmploymentHistoryItem.LASDays, column.MatrixDataType);
                    default:
                        break;
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}
