using SoftOne.Soe.Business.Core.Reporting.Matrix.Models;
using SoftOne.Soe.Business.Core.Reporting.Matrix.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Time;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix
{
    public class EmploymentDaysMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "EmploymentDaysMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        List<EmploymentDaysReportDataField> filter { get; set; }
        EmploymentDaysReportDataOutput _reportDataOutput { get; set; }
        List<EmploymentTypeDTO> _employmentTypes { get; set; }

        #endregion

        public EmploymentDaysMatrix(InputMatrix inputMatrix, EmploymentDaysReportDataOutput reportDataOutput, List<EmploymentTypeDTO> employmentTypes) : base(inputMatrix)
        {
            filter = reportDataOutput?.Input?.Columns;
            _reportDataOutput = reportDataOutput;
            _employmentTypes = employmentTypes;
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
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_EmploymentDaysMatrixColumns.EmploymentNumber));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmploymentDaysMatrixColumns.Name));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmploymentDaysMatrixColumns.FirstName));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmploymentDaysMatrixColumns.LastName));
            }
            if (employeesEmploymentsEditHasPermission)
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmploymentDaysMatrixColumns.WorkingPlace));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_EmploymentDaysMatrixColumns.EmploymentStartDate));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_EmploymentDaysMatrixColumns.EmploymentEndDate));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmploymentDaysMatrixColumns.EmploymentType));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmploymentDaysMatrixColumns.TimeAgreement));

                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_EmploymentDaysMatrixColumns.TotalEmploymentDays));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_EmploymentDaysMatrixColumns.EmploymentLASTypeAvaDays));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_EmploymentDaysMatrixColumns.EmploymentLASTypeSvaDays));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_EmploymentDaysMatrixColumns.EmploymentLASTypeVikDays));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_EmploymentDaysMatrixColumns.EmploymentLASTypeOtherDays));

                _employmentTypes.ForEach(e => {
                    possibleColumns.Add(CreateMatrixLayoutColumnString(MatrixDataType.Integer, e.Name));
                });
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

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_EmploymentDaysMatrixColumns column, MatrixDefinitionColumnOptions options = null)
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

        private MatrixLayoutColumn CreateMatrixLayoutColumnString(MatrixDataType dataType, string column, MatrixDefinitionColumnOptions options = null)
        {
            MatrixLayoutColumn matrixLayoutColumn = new MatrixLayoutColumn(dataType, column, column, options);
            return matrixLayoutColumn;
        }

        public MatrixResult GetMatrixResult()
        {
            MatrixResult result = new MatrixResult();
            result.MatrixDefinition = new MatrixDefinition() { MatrixDefinitionColumns = GetMatrixDefinitionColumns() };

            #region Create matrix

            int rowNumber = 1;

            foreach (var product in _reportDataOutput.EmploymentDaysItems)
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

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, EmploymentDaysItem EmploymentDaysItem)
        {
            if (EnumUtility.GetValue(column.Field.FirstCharToUpperCase(), out int id, typeof(TermGroup_EmploymentDaysMatrixColumns)))
            {
                var type = (TermGroup_EmploymentDaysMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_EmploymentDaysMatrixColumns.EmploymentNumber:
                        return new MatrixField(rowNumber, column.Key, EmploymentDaysItem.EmploymentNumber, column.MatrixDataType);
                    case TermGroup_EmploymentDaysMatrixColumns.Name:
                        return new MatrixField(rowNumber, column.Key, EmploymentDaysItem.Name, column.MatrixDataType);
                    case TermGroup_EmploymentDaysMatrixColumns.FirstName:
                        return new MatrixField(rowNumber, column.Key, EmploymentDaysItem.FirstName, column.MatrixDataType);
                    case TermGroup_EmploymentDaysMatrixColumns.LastName:
                        return new MatrixField(rowNumber, column.Key, EmploymentDaysItem.LastName, column.MatrixDataType);
                    case TermGroup_EmploymentDaysMatrixColumns.WorkingPlace:
                        return new MatrixField(rowNumber, column.Key, EmploymentDaysItem.CurrentWorkingPlace, column.MatrixDataType);
                    case TermGroup_EmploymentDaysMatrixColumns.EmploymentStartDate:
                        return new MatrixField(rowNumber, column.Key, EmploymentDaysItem.EmploymentStartDate, column.MatrixDataType);
                    case TermGroup_EmploymentDaysMatrixColumns.EmploymentEndDate:
                        return new MatrixField(rowNumber, column.Key, EmploymentDaysItem.EmploymentEndDate, column.MatrixDataType);
                    case TermGroup_EmploymentDaysMatrixColumns.EmploymentType:
                        return new MatrixField(rowNumber, column.Key, EmploymentDaysItem.CurrentEmploymentType, column.MatrixDataType);
                    case TermGroup_EmploymentDaysMatrixColumns.TimeAgreement:
                        return new MatrixField(rowNumber, column.Key, EmploymentDaysItem.CurrentTimeAgreement, column.MatrixDataType);
                    case TermGroup_EmploymentDaysMatrixColumns.EmploymentLASTypeAvaDays:
                        return new MatrixField(rowNumber, column.Key, EmploymentDaysItem.LASTypeAvaDays, column.MatrixDataType);
                    case TermGroup_EmploymentDaysMatrixColumns.EmploymentLASTypeSvaDays:
                        return new MatrixField(rowNumber, column.Key, EmploymentDaysItem.LASTypeSvaDays, column.MatrixDataType);
                    case TermGroup_EmploymentDaysMatrixColumns.EmploymentLASTypeVikDays:
                        return new MatrixField(rowNumber, column.Key, EmploymentDaysItem.LASTypeVikDays, column.MatrixDataType);
                    case TermGroup_EmploymentDaysMatrixColumns.EmploymentLASTypeOtherDays:
                        return new MatrixField(rowNumber, column.Key, EmploymentDaysItem.LASTypeOtherDays, column.MatrixDataType);
                    case TermGroup_EmploymentDaysMatrixColumns.TotalEmploymentDays:
                        return new MatrixField(rowNumber, column.Key, EmploymentDaysItem.TotalEmploymentDays, column.MatrixDataType);
                    default:
                        break;
                }
            }
            else
            {
                if (_employmentTypes.Any(e => e.Name == column.Field.FirstCharToUpperCase()))
                {
                    EmploymentTypeDTO employmentType = _employmentTypes.Find(e => e.Name == column.Field.FirstCharToUpperCase());
                    int employmentTypeId = employmentType.EmploymentTypeId ?? 0;
                    int key = employmentTypeId != 0 ? employmentTypeId : employmentType.Type;
                    if (EmploymentDaysItem.EmploymentTypesDays.ContainsKey(key))
                    {
                        return new MatrixField(rowNumber, column.Key, EmploymentDaysItem.EmploymentTypesDays[key], column.MatrixDataType);
                    }
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}
