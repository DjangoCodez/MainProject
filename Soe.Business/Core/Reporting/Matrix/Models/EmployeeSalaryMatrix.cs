using SoftOne.Soe.Business.Core.Reporting.Matrix.Models;
using SoftOne.Soe.Business.Core.Reporting.Matrix.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models;
using SoftOne.Soe.Business.Core.Reporting.Models.Time;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix
{
    public class EmployeeSalaryMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "EmployeeSalaryMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        List<EmployeeSalaryReportDataField> filter { get; set; }
        EmployeeSalaryReportDataOutput _reportDataOutput { get; set; }
        readonly bool useAccountHierarchy;
        #endregion

        public EmployeeSalaryMatrix(InputMatrix inputMatrix, EmployeeSalaryReportDataOutput reportDataOutput, int actorCompanyId) : base(inputMatrix)
        {
            filter = reportDataOutput?.Input?.Columns;
            _reportDataOutput = reportDataOutput;

            SettingManager sm = new SettingManager(null);
            this.useAccountHierarchy = sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseAccountHierarchy, 0, actorCompanyId, 0);
        }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            if (!base.HasReadPermission(Feature.Manage_Users_Edit))
                return possibleColumns;

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_EmployeeSalaryMatrixColumns.EmployeeId, new MatrixDefinitionColumnOptions() { Hidden = true }));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSalaryMatrixColumns.EmployeeNr));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSalaryMatrixColumns.EmployeeName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSalaryMatrixColumns.FirstName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSalaryMatrixColumns.LastName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSalaryMatrixColumns.Gender));
            if (base.HasReadPermission(Feature.Time_Employee_EmploymentTypes))
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSalaryMatrixColumns.EmploymentTypeName));
            if (base.HasReadPermission(Feature.Time_Employee_Positions))
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSalaryMatrixColumns.Position));
            if (base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll_Salary))
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSalaryMatrixColumns.SalaryType));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSalaryMatrixColumns.SalaryTypeName));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSalaryMatrixColumns.SalaryTypeCode));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSalaryMatrixColumns.SalaryTypeDesc));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_EmployeeSalaryMatrixColumns.SalaryDateFrom));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_EmployeeSalaryMatrixColumns.SalaryAmount));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_EmployeeSalaryMatrixColumns.AccordingToPayrollGroup));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSalaryMatrixColumns.PayrollLevel));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_EmployeeSalaryMatrixColumns.ExperienceTot));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSalaryMatrixColumns.BirthYearMonth));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_EmployeeSalaryMatrixColumns.SalaryFromPayrollGroup));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_EmployeeSalaryMatrixColumns.SalaryDiff));
            }
            if (!this.useAccountHierarchy)
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSalaryMatrixColumns.CategoryName));

            int nbrOfAccountDims = inputMatrix?.AccountDims?.Count(w => w.AccountDimNr != 1) ?? 0;
            if (nbrOfAccountDims > 0 && base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Accounts))
            {
                foreach (var dim in inputMatrix.AccountDims)
                {
                    MatrixDefinitionColumnOptions options = new MatrixDefinitionColumnOptions();
                    options.Key = dim.AccountDimId.ToString();
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSalaryMatrixColumns.AccountInternalNrs, options, dim.Name + " " + GetText(507, "Nummer")));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeSalaryMatrixColumns.AccountInternalNames, options, dim.Name + " " + GetText(508, "Namn")));
                }
            }

            if (base.HasReadPermission(Feature.Common_ExtraFields_Employee) && !inputMatrix.ExtraFields.IsNullOrEmpty())
            {
                foreach (var extraField in inputMatrix.ExtraFields)
                {
                    MatrixDefinitionColumnOptions options = new MatrixDefinitionColumnOptions();
                    options.Key = extraField.ExtraFieldId.ToString();
                    possibleColumns.Add(CreateMatrixLayoutColumn(SetMatrixDataType((TermGroup_ExtraFieldType)extraField.Type), TermGroup_EmployeeSalaryMatrixColumns.ExtraFieldEmployee, options, extraField.Text));
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
                        MatrixLayoutColumn item = possibleColumns.FirstOrDefault(w => w.Field == field.ColumnKey && !w.IsHidden());

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
        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_EmployeeSalaryMatrixColumns column, MatrixDefinitionColumnOptions options = null, string overrideTitle = null)
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

            foreach (var employeeSalaryItem in _reportDataOutput.EmployeeSalaryItems)
            {
                List<MatrixField> fields = new List<MatrixField>();

                foreach (MatrixDefinitionColumn column in GetMatrixDefinitionColumns())
                    fields.Add(CreateField(rowNumber, column, employeeSalaryItem));

                if (base.inputMatrix.ExportType == TermGroup_ReportExportType.MatrixExcel)
                    result.MatrixFields.AddRange(fields);
                result.JsonRows.Add(fields.CreateRow(GetMatrixDefinitionColumns()));
                rowNumber++;
            }

            #endregion

            return result;
        }

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, EmployeeSalaryItem employeeSalaryItem)
        {
            if (base.GetEnumId<TermGroup_EmployeeSalaryMatrixColumns>(column, out int id))
            {
                var type = (TermGroup_EmployeeSalaryMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_EmployeeSalaryMatrixColumns.EmployeeId:
                        return new MatrixField(rowNumber, column.Key, employeeSalaryItem.EmployeeId, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryMatrixColumns.EmployeeName:
                        return new MatrixField(rowNumber, column.Key, employeeSalaryItem.EmployeeName, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryMatrixColumns.FirstName:
                        return new MatrixField(rowNumber, column.Key, employeeSalaryItem.FirstName, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryMatrixColumns.LastName:
                        return new MatrixField(rowNumber, column.Key, employeeSalaryItem.LastName, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryMatrixColumns.EmployeeNr:
                        return new MatrixField(rowNumber, column.Key, employeeSalaryItem.EmployeeNr, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryMatrixColumns.Gender:
                        return new MatrixField(rowNumber, column.Key, employeeSalaryItem.Gender, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryMatrixColumns.EmploymentTypeName:
                        return new MatrixField(rowNumber, column.Key, employeeSalaryItem.EmploymentType, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryMatrixColumns.Position:
                        return new MatrixField(rowNumber, column.Key, employeeSalaryItem.Position, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryMatrixColumns.SalaryType:
                        return new MatrixField(rowNumber, column.Key, employeeSalaryItem.SalaryType, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryMatrixColumns.SalaryTypeName:
                        return new MatrixField(rowNumber, column.Key, employeeSalaryItem.SalaryTypeName, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryMatrixColumns.SalaryTypeCode:
                        return new MatrixField(rowNumber, column.Key, employeeSalaryItem.SalaryTypeCode, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryMatrixColumns.SalaryTypeDesc:
                        return new MatrixField(rowNumber, column.Key, employeeSalaryItem.SalaryTypeDesc, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryMatrixColumns.SalaryDateFrom:
                        return new MatrixField(rowNumber, column.Key, employeeSalaryItem.SalaryDateFrom, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryMatrixColumns.SalaryAmount:
                        return new MatrixField(rowNumber, column.Key, employeeSalaryItem.SalaryAmount, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryMatrixColumns.AccordingToPayrollGroup:
                        return new MatrixField(rowNumber, column.Key, employeeSalaryItem.AccordingToPayrollGroup, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryMatrixColumns.CategoryName:
                        return new MatrixField(rowNumber, column.Key, employeeSalaryItem.CategoryName, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryMatrixColumns.ExperienceTot:
                        return new MatrixField(rowNumber, column.Key, employeeSalaryItem.ExperienceTot, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryMatrixColumns.PayrollLevel:
                        return new MatrixField(rowNumber, column.Key, employeeSalaryItem.PayrollLevel, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryMatrixColumns.BirthYearMonth:
                        return new MatrixField(rowNumber, column.Key, employeeSalaryItem.BirthYearMonth, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryMatrixColumns.Age:
                        return new MatrixField(rowNumber, column.Key, employeeSalaryItem.Age, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryMatrixColumns.SalaryFromPayrollGroup:
                        return new MatrixField(rowNumber, column.Key, employeeSalaryItem.SalaryFromPayrollGroup, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryMatrixColumns.SalaryDiff:
                        return new MatrixField(rowNumber, column.Key, employeeSalaryItem.SalaryDiff, column.MatrixDataType);
                    case TermGroup_EmployeeSalaryMatrixColumns.AccountInternalNames:
                        return new MatrixField(rowNumber, column.Key, employeeSalaryItem.AccountAnalysisFields.GetAccountAnalysisFieldValueName(column), column.MatrixDataType);
                    case TermGroup_EmployeeSalaryMatrixColumns.AccountInternalNrs:
                        return new MatrixField(rowNumber, column.Key, employeeSalaryItem.AccountAnalysisFields.GetAccountAnalysisFieldValueNumber(column), column.MatrixDataType);
                    case TermGroup_EmployeeSalaryMatrixColumns.ExtraFieldEmployee:
                        return new MatrixField(rowNumber, column.Key, employeeSalaryItem.ExtraFieldAnalysisFields.ExtraFieldAnalysisFieldValue(column), column.MatrixDataType);

                    default:
                        break;
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}
