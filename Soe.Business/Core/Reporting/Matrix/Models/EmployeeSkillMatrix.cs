using SoftOne.Soe.Business.Core.Reporting.Matrix.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Time;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix.Models
{
    public class EmployeeSkillMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "EmployeeSkillMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        List<EmployeeSkillReportDataField> filter { get; set; }
        EmployeeSkillReportDataOutput _reportDataOutput { get; set; }
        readonly bool useAccountHierarchy;
        #endregion

        public EmployeeSkillMatrix(InputMatrix inputMatrix, EmployeeSkillReportDataOutput reportDataOutput, int actorCompanyId) : base(inputMatrix)
        {
            filter = reportDataOutput?.Input?.Columns;
            _reportDataOutput = reportDataOutput;
            
            SettingManager sm = new SettingManager(null);
            useAccountHierarchy = sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseAccountHierarchy, 0, actorCompanyId, 0);
        }
        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {                
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            if (!base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees))
                return possibleColumns;

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSkillMatrixColumns.EmployeeNr));
            if(!useAccountHierarchy)
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSkillMatrixColumns.CategoryName));
            else
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSkillMatrixColumns.AccountName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSkillMatrixColumns.Name));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSkillMatrixColumns.FirstName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSkillMatrixColumns.LastName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSkillMatrixColumns.Gender));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSkillMatrixColumns.BirthYear)); 
            if (base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Skills))
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSkillMatrixColumns.EmploymentTypeName));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSkillMatrixColumns.PositionName));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSkillMatrixColumns.SSYKCode));
            }
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSkillMatrixColumns.SkillName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSkillMatrixColumns.SkillDescription));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_EmployeeSkillMatrixColumns.SkillDate));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_EmployeeSkillMatrixColumns.SkillLevel));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSkillMatrixColumns.SkillTypeName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSkillMatrixColumns.SkillTypeDescription));
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

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_EmployeeSkillMatrixColumns column, MatrixDefinitionColumnOptions options = null)
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

            foreach (var employee in _reportDataOutput.EmployeeSkillItems)
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

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, EmployeeSkillItem employee)
        {
            if (EnumUtility.GetValue(column.Field.FirstCharToUpperCase(), out int id, typeof(TermGroup_EmployeeSkillMatrixColumns)))
            {
                var type = (TermGroup_EmployeeSkillMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_EmployeeSkillMatrixColumns.EmployeeNr:
                        return new MatrixField(rowNumber, column.Key, employee.EmployeeNr, column.MatrixDataType);
                    case TermGroup_EmployeeSkillMatrixColumns.Name:
                        return new MatrixField(rowNumber, column.Key, employee.EmployeeName, column.MatrixDataType);
                    case TermGroup_EmployeeSkillMatrixColumns.CategoryName:
                        return new MatrixField(rowNumber, column.Key, employee.CategoryName, column.MatrixDataType);
                    case TermGroup_EmployeeSkillMatrixColumns.AccountName:
                        return new MatrixField(rowNumber, column.Key, employee.CategoryName, column.MatrixDataType);
                    case TermGroup_EmployeeSkillMatrixColumns.FirstName:
                        return new MatrixField(rowNumber, column.Key, employee.FirstName, column.MatrixDataType);
                    case TermGroup_EmployeeSkillMatrixColumns.LastName:
                        return new MatrixField(rowNumber, column.Key, employee.LastName, column.MatrixDataType);
                    case TermGroup_EmployeeSkillMatrixColumns.Gender:
                        return new MatrixField(rowNumber, column.Key, employee.Gender, column.MatrixDataType);
                    case TermGroup_EmployeeSkillMatrixColumns.SSYKCode:
                        return new MatrixField(rowNumber, column.Key, employee.SSYKCode, column.MatrixDataType);
                    case TermGroup_EmployeeSkillMatrixColumns.PositionName:
                        return new MatrixField(rowNumber, column.Key, employee.PositionName, column.MatrixDataType);
                    case TermGroup_EmployeeSkillMatrixColumns.EmploymentTypeName:
                        return new MatrixField(rowNumber, column.Key, employee.EmploymentTypeName, column.MatrixDataType);
                    case TermGroup_EmployeeSkillMatrixColumns.BirthYear:
                        return new MatrixField(rowNumber, column.Key, employee.BirthYear, column.MatrixDataType);
                    case TermGroup_EmployeeSkillMatrixColumns.SkillDate:
                        return new MatrixField(rowNumber, column.Key, employee.SkillDate, column.MatrixDataType);
                    case TermGroup_EmployeeSkillMatrixColumns.SkillLevel:
                        return new MatrixField(rowNumber, column.Key, employee.SkillLevel, column.MatrixDataType);
                    case TermGroup_EmployeeSkillMatrixColumns.SkillTypeId:
                        return new MatrixField(rowNumber, column.Key, employee.SkillTypeId, column.MatrixDataType);
                    case TermGroup_EmployeeSkillMatrixColumns.SkillDescription:
                        return new MatrixField(rowNumber, column.Key, employee.SkillDescription, column.MatrixDataType);
                    case TermGroup_EmployeeSkillMatrixColumns.SkillTypeDescription:
                        return new MatrixField(rowNumber, column.Key, employee.SkillTypeDescription, column.MatrixDataType);
                    case TermGroup_EmployeeSkillMatrixColumns.SkillTypeName:
                        return new MatrixField(rowNumber, column.Key, employee.SkillTypeName, column.MatrixDataType);
                    case TermGroup_EmployeeSkillMatrixColumns.SkillName:
                        return new MatrixField(rowNumber, column.Key, employee.SkillName, column.MatrixDataType);
     
                    default:
                        break;
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}
