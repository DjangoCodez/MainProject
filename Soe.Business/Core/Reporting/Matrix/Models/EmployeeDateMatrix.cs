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
    public class EmployeeDateMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "EmployeeDateMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        List<EmployeeDateReportDataField> filter { get; set; }
        List<EmployeeDateItem> employeeDateItems { get; set; }
        #endregion

        public EmployeeDateMatrix(InputMatrix inputMatrix, EmployeeDateReportDataOutput reportDataOutput) : base(inputMatrix)
        {
            filter = reportDataOutput?.Input?.Columns;
            employeeDateItems = reportDataOutput?.EmployeeDateItems;
        }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            if (!base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees))
                return possibleColumns;

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_EmployeeDateMatrixColumns.EmployeeId, new MatrixDefinitionColumnOptions() { Hidden = true }));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeDateMatrixColumns.EmployeeNr));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeDateMatrixColumns.EmployeeName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_EmployeeDateMatrixColumns.Date));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeDateMatrixColumns.DateTypeName));
            if (base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Employment))
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeDateMatrixColumns.EmployeeGroupName));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeDateMatrixColumns.PayrollGroupName));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeDateMatrixColumns.VacationGroupName));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_EmployeeDateMatrixColumns.EmploymentPercent));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_EmployeeDateMatrixColumns.EmploymentFte));
            }
            if (base.HasReadPermission(Feature.Time_Schedule_SchedulePlanningUser))
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_EmployeeDateMatrixColumns.ScheduleTime));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_EmployeeDateMatrixColumns.ScheduleAbsenceTime));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_EmployeeDateMatrixColumns.PercentScheduleAbsenceTime));
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

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_EmployeeDateMatrixColumns column, MatrixDefinitionColumnOptions options = null)
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

            foreach (var employee in employeeDateItems)
            {
                List<MatrixField> fields = new List<MatrixField>();

                foreach (MatrixDefinitionColumn column in GetMatrixDefinitionColumns())
                    fields.Add(CreateField(rowNumber, column, employee));

                if (base.inputMatrix.ExportType == TermGroup_ReportExportType.MatrixExcel)
                    result.MatrixFields.AddRange(fields);
                result.JsonRows.Add(fields.CreateRow(GetMatrixDefinitionColumns()));
                rowNumber++;
            }

            #endregion

            return result;
        }

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, EmployeeDateItem employeeDateItem)
        {
            if (EnumUtility.GetValue(column.Field.FirstCharToUpperCase(), out int id, typeof(TermGroup_EmployeeDateMatrixColumns)))
            {
                var type = (TermGroup_EmployeeDateMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_EmployeeDateMatrixColumns.EmployeeId:
                        return new MatrixField(rowNumber, column.Key, employeeDateItem.EmployeeId, column.MatrixDataType);
                    case TermGroup_EmployeeDateMatrixColumns.EmployeeNr:
                        return new MatrixField(rowNumber, column.Key, employeeDateItem.EmployeeNr, column.MatrixDataType);
                    case TermGroup_EmployeeDateMatrixColumns.EmployeeName:
                        return new MatrixField(rowNumber, column.Key, employeeDateItem.EmployeeName, column.MatrixDataType);
                    case TermGroup_EmployeeDateMatrixColumns.Date:
                        return new MatrixField(rowNumber, column.Key, employeeDateItem.Date, column.MatrixDataType);
                    case TermGroup_EmployeeDateMatrixColumns.EmploymentPercent:
                        return new MatrixField(rowNumber, column.Key, employeeDateItem.Percent, column.MatrixDataType);
                    case TermGroup_EmployeeDateMatrixColumns.EmploymentFte:
                        return new MatrixField(rowNumber, column.Key, employeeDateItem.FTE, column.MatrixDataType);
                    case TermGroup_EmployeeDateMatrixColumns.EmployeeGroupName:
                        return new MatrixField(rowNumber, column.Key, employeeDateItem.EmployeeGroupName, column.MatrixDataType);
                    case TermGroup_EmployeeDateMatrixColumns.PayrollGroupName:
                        return new MatrixField(rowNumber, column.Key, employeeDateItem.PayrollGroupName, column.MatrixDataType);
                    case TermGroup_EmployeeDateMatrixColumns.VacationGroupName:
                        return new MatrixField(rowNumber, column.Key, employeeDateItem.VacationGroupName, column.MatrixDataType);
                    case TermGroup_EmployeeDateMatrixColumns.DateTypeName:
                        return new MatrixField(rowNumber, column.Key, employeeDateItem.DayTypeName, column.MatrixDataType);
                    case TermGroup_EmployeeDateMatrixColumns.ScheduleTime:
                        return new MatrixField(rowNumber, column.Key, employeeDateItem.ScheduleTime, column.MatrixDataType);
                    case TermGroup_EmployeeDateMatrixColumns.ScheduleAbsenceTime:
                        return new MatrixField(rowNumber, column.Key, employeeDateItem.ScheduleAbsenceTime, column.MatrixDataType);
                    case TermGroup_EmployeeDateMatrixColumns.PercentScheduleAbsenceTime:
                        return new MatrixField(rowNumber, column.Key, employeeDateItem.PercentScheduleAbsenceTime, column.MatrixDataType);
                    default:
                        break;
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}
