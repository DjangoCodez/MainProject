using SoftOne.Soe.Business.Core.Reporting.Matrix.Models;
using SoftOne.Soe.Business.Core.Reporting.Matrix.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models;
using SoftOne.Soe.Business.Core.Reporting.Models.Time;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix
{
    public class LongtermAbsenceMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "LongtermAbsenceMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        List<LongtermAbsenceReportDataField> filter { get; set; }
        List<LongtermAbsenceItem> longtermAbsenceItems { get; set; }
        public bool useAccountHierarchy { get; set; }
        #endregion

        public LongtermAbsenceMatrix(InputMatrix inputMatrix, LongtermAbsenceReportDataOutput reportDataOutput, int ActorCompanyId) : base(inputMatrix)
        {
            filter = reportDataOutput?.Input?.Columns;
            longtermAbsenceItems = reportDataOutput?.LongtermAbsenceItems;
            SettingManager sm = new SettingManager(null);
            this.useAccountHierarchy = sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseAccountHierarchy, 0, ActorCompanyId, 0);
        }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            if (!base.HasReadPermission(Feature.Time_Time_Attest_Edit))
                return possibleColumns;

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_LongtermAbsenceMatrixColumns.EmployeeNr));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_LongtermAbsenceMatrixColumns.LastName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_LongtermAbsenceMatrixColumns.FirstName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_LongtermAbsenceMatrixColumns.Name));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_LongtermAbsenceMatrixColumns.SocialSec));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_LongtermAbsenceMatrixColumns.PayrollTypeLevel1Name));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_LongtermAbsenceMatrixColumns.PayrollTypeLevel2Name));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_LongtermAbsenceMatrixColumns.PayrollTypeLevel3Name));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_LongtermAbsenceMatrixColumns.PayrollTypeLevel1));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_LongtermAbsenceMatrixColumns.PayrollTypeLevel2));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_LongtermAbsenceMatrixColumns.PayrollTypeLevel3));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_LongtermAbsenceMatrixColumns.StartDateInInterval));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_LongtermAbsenceMatrixColumns.StopDateInInterval));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_LongtermAbsenceMatrixColumns.NumberOfDaysInInterval));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_LongtermAbsenceMatrixColumns.EntireSelectedPeriod));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_LongtermAbsenceMatrixColumns.StartDate));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_LongtermAbsenceMatrixColumns.StopDate));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_LongtermAbsenceMatrixColumns.NumberOfDaysTotal));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_LongtermAbsenceMatrixColumns.NumberOfDaysBeforeInterval));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_LongtermAbsenceMatrixColumns.NumberOfDaysAfterInterval));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_LongtermAbsenceMatrixColumns.Ratio));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.DateAndTime, TermGroup_LongtermAbsenceMatrixColumns.Created));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.DateAndTime, TermGroup_LongtermAbsenceMatrixColumns.Modified));

            int nbrOfAccountDims = inputMatrix?.AccountDims?.Count(w => w.AccountDimNr != 1) ?? 0;
            if (nbrOfAccountDims > 0 && (base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Accounts) || base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Categories)))
            {
                foreach (var dim in inputMatrix.AccountDims.Where(w => w.IsInternal))
                {
                    MatrixDefinitionColumnOptions options = new MatrixDefinitionColumnOptions();
                    options.Key = dim.AccountDimId.ToString();
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_LongtermAbsenceMatrixColumns.AccountInternalNrs, options, dim.Name + " " + GetText(500, "Nummer")));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_LongtermAbsenceMatrixColumns.AccountInternalNames, options, dim.Name + " " + GetText(501, "Namn")));
                }

                foreach (var dim in inputMatrix.AccountDims.Where(w => w.IsInternal))
                {
                    MatrixDefinitionColumnOptions options = new MatrixDefinitionColumnOptions();
                    options.Key = dim.AccountDimId.ToString();
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_LongtermAbsenceMatrixColumns.EmployeeAccountInternalNrs, options, dim.Name + " " + GetText(5, "Nummer") + $" ({GetText(1500, "Tillhörighet")})"));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_LongtermAbsenceMatrixColumns.EmployeeAccountInternalNames, options, dim.Name + " " + GetText(508, "Namn") + $" ({GetText(1500, "Tillhörighet")})"));
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

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_LongtermAbsenceMatrixColumns column, MatrixDefinitionColumnOptions options = null, string overrideTitle = null)
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

            foreach (var employee in longtermAbsenceItems)
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

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, LongtermAbsenceItem longtermAbsenceItem)
        {
            if (base.GetEnumId<TermGroup_LongtermAbsenceMatrixColumns>(column, out int id))
            {
                var type = (TermGroup_LongtermAbsenceMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_LongtermAbsenceMatrixColumns.EmployeeNr: return new MatrixField(rowNumber, column.Key, longtermAbsenceItem.EmployeeNr, column.MatrixDataType);
                    case TermGroup_LongtermAbsenceMatrixColumns.FirstName: return new MatrixField(rowNumber, column.Key, longtermAbsenceItem.FirstName, column.MatrixDataType);
                    case TermGroup_LongtermAbsenceMatrixColumns.LastName: return new MatrixField(rowNumber, column.Key, longtermAbsenceItem.LastName, column.MatrixDataType);
                    case TermGroup_LongtermAbsenceMatrixColumns.Name: return new MatrixField(rowNumber, column.Key, longtermAbsenceItem.Name, column.MatrixDataType);
                    case TermGroup_LongtermAbsenceMatrixColumns.PayrollTypeLevel1Name: return new MatrixField(rowNumber, column.Key, longtermAbsenceItem.PayrollTypeLevel1Name, column.MatrixDataType);
                    case TermGroup_LongtermAbsenceMatrixColumns.PayrollTypeLevel2Name: return new MatrixField(rowNumber, column.Key, longtermAbsenceItem.PayrollTypeLevel2Name, column.MatrixDataType);
                    case TermGroup_LongtermAbsenceMatrixColumns.PayrollTypeLevel3Name: return new MatrixField(rowNumber, column.Key, longtermAbsenceItem.PayrollTypeLevel3Name, column.MatrixDataType);
                    case TermGroup_LongtermAbsenceMatrixColumns.PayrollTypeLevel1: return new MatrixField(rowNumber, column.Key, longtermAbsenceItem.PayrollTypeLevel1, column.MatrixDataType);
                    case TermGroup_LongtermAbsenceMatrixColumns.PayrollTypeLevel2: return new MatrixField(rowNumber, column.Key, longtermAbsenceItem.PayrollTypeLevel2, column.MatrixDataType);
                    case TermGroup_LongtermAbsenceMatrixColumns.PayrollTypeLevel3: return new MatrixField(rowNumber, column.Key, longtermAbsenceItem.PayrollTypeLevel3, column.MatrixDataType);
                    case TermGroup_LongtermAbsenceMatrixColumns.StartDateInInterval: return new MatrixField(rowNumber, column.Key, longtermAbsenceItem.StartDateInInterval, column.MatrixDataType);
                    case TermGroup_LongtermAbsenceMatrixColumns.StopDateInInterval: return new MatrixField(rowNumber, column.Key, longtermAbsenceItem.StopDateInInterval, column.MatrixDataType);
                    case TermGroup_LongtermAbsenceMatrixColumns.StartDate: return new MatrixField(rowNumber, column.Key, longtermAbsenceItem.StartDate, column.MatrixDataType);
                    case TermGroup_LongtermAbsenceMatrixColumns.StopDate: return new MatrixField(rowNumber, column.Key, longtermAbsenceItem.StopDate, column.MatrixDataType);
                    case TermGroup_LongtermAbsenceMatrixColumns.NumberOfDaysInInterval: return new MatrixField(rowNumber, column.Key, longtermAbsenceItem.NumberOfDaysInInterval, column.MatrixDataType);
                    case TermGroup_LongtermAbsenceMatrixColumns.EntireSelectedPeriod: return new MatrixField(rowNumber, column.Key, longtermAbsenceItem.EntireSelectedPeriod, column.MatrixDataType);
                    case TermGroup_LongtermAbsenceMatrixColumns.NumberOfDaysTotal: return new MatrixField(rowNumber, column.Key, longtermAbsenceItem.NumberOfDaysTotal, column.MatrixDataType);
                    case TermGroup_LongtermAbsenceMatrixColumns.NumberOfDaysBeforeInterval: return new MatrixField(rowNumber, column.Key, longtermAbsenceItem.NumberOfDaysBeforeInterval, column.MatrixDataType);
                    case TermGroup_LongtermAbsenceMatrixColumns.NumberOfDaysAfterInterval: return new MatrixField(rowNumber, column.Key, longtermAbsenceItem.NumberOfDaysAfterInterval, column.MatrixDataType);
                    case TermGroup_LongtermAbsenceMatrixColumns.SocialSec: return new MatrixField(rowNumber, column.Key, longtermAbsenceItem.SocialSec, column.MatrixDataType);
                    case TermGroup_LongtermAbsenceMatrixColumns.Ratio: return new MatrixField(rowNumber, column.Key, longtermAbsenceItem.Ratio, column.MatrixDataType);
                    case TermGroup_LongtermAbsenceMatrixColumns.AccountInternalNames: return new MatrixField(rowNumber, column.Key, longtermAbsenceItem.AccountAnalysisFields?.GetAccountAnalysisFieldValueName(column) ?? "", column.MatrixDataType);
                    case TermGroup_LongtermAbsenceMatrixColumns.AccountInternalNrs: return new MatrixField(rowNumber, column.Key, longtermAbsenceItem.AccountAnalysisFields?.GetAccountAnalysisFieldValueNumber(column) ?? "", column.MatrixDataType);
                    case TermGroup_LongtermAbsenceMatrixColumns.EmployeeAccountInternalNames: return new MatrixField(rowNumber, column.Key, longtermAbsenceItem.EmployeeAccountAnalysisFields?.GetAccountAnalysisFieldValueName(column) ?? "", column.MatrixDataType);
                    case TermGroup_LongtermAbsenceMatrixColumns.EmployeeAccountInternalNrs: return new MatrixField(rowNumber, column.Key, longtermAbsenceItem.EmployeeAccountAnalysisFields?.GetAccountAnalysisFieldValueNumber(column) ?? "", column.MatrixDataType);
                    case TermGroup_LongtermAbsenceMatrixColumns.Created: return new MatrixField(rowNumber, column.Key, 
                        longtermAbsenceItem.Created, column.MatrixDataType);
                    case TermGroup_LongtermAbsenceMatrixColumns.Modified:
                        return new MatrixField(rowNumber, column.Key,
                            longtermAbsenceItem.Modified, column.MatrixDataType);
                    default:
                        break;
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}
