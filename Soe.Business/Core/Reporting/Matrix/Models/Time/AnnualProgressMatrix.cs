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
    public class AnnualProgressMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "AnnualProgressMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        List<AnnualProgressReportDataField> filter { get; set; }
        List<AnnualProgressItem> annualProgressItems { get; set; }
        public bool useAccountHierarchy { get; set; }
        #endregion

        public AnnualProgressMatrix(InputMatrix inputMatrix, AnnualProgressReportDataOutput reportDataOutput, int ActorCompanyId) : base(inputMatrix)
        {
            filter = reportDataOutput?.Input?.Columns;
            annualProgressItems = reportDataOutput?.AnnualProgressItems;
            SettingManager sm = new SettingManager(null);
            this.useAccountHierarchy = sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseAccountHierarchy, 0, ActorCompanyId, 0);
        }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            if (!base.HasReadPermission(Feature.Time_Schedule_SchedulePlanning_Dashboard))
                return possibleColumns;

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_AnnualProgressMatrixColumns.Date));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AnnualProgressMatrixColumns.GoalPerWeek));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AnnualProgressMatrixColumns.LastYearAveragePerWeek));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AnnualProgressMatrixColumns.DifferenceAverageFromLastYearPerWeek));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AnnualProgressMatrixColumns.DifferenceAverageFromGoalPerWeek));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AnnualProgressMatrixColumns.RemainingYearAveragePerWeek));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AnnualProgressMatrixColumns.GoalPerMonth));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AnnualProgressMatrixColumns.LastYearAveragePerMonth));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AnnualProgressMatrixColumns.DifferenceAverageFromLastYearPerMonth));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AnnualProgressMatrixColumns.RemainingYearAveragePerMonth));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AnnualProgressMatrixColumns.SalesToDate));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AnnualProgressMatrixColumns.WorkingHoursToDate));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AnnualProgressMatrixColumns.FPATGoal));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AnnualProgressMatrixColumns.FPATToDate));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AnnualProgressMatrixColumns.AveragePerWeekToDate));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_AnnualProgressMatrixColumns.AveragePerMonthToDate));


            int nbrOfAccountDims = inputMatrix?.AccountDims?.Count(w => w.AccountDimNr != 1) ?? 0;
            if (nbrOfAccountDims > 0 && (base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Accounts) || base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Categories)))
            {
                foreach (var dim in inputMatrix.AccountDims.Where(w => w.IsInternal))
                {
                    MatrixDefinitionColumnOptions options = new MatrixDefinitionColumnOptions();
                    options.Key = dim.AccountDimId.ToString();
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_AnnualProgressMatrixColumns.AccountInternalNrs, options, dim.Name + " " + GetText(507, "Nummer")));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_AnnualProgressMatrixColumns.AccountInternalNames, options, dim.Name + " " + GetText(508, "Namn")));
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

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_AnnualProgressMatrixColumns column, MatrixDefinitionColumnOptions options = null, string overrideTitle = null)
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

            foreach (var employee in annualProgressItems)
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

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, AnnualProgressItem annualProgressItem)
        {
            if (base.GetEnumId<TermGroup_AnnualProgressMatrixColumns>(column, out int id))
            {
                var type = (TermGroup_AnnualProgressMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_AnnualProgressMatrixColumns.Date: return new MatrixField(rowNumber, column.Key, annualProgressItem.Date, MatrixDataType.Date);
                    case TermGroup_AnnualProgressMatrixColumns.GoalPerWeek: return new MatrixField(rowNumber, column.Key, annualProgressItem.GoalPerWeek, MatrixDataType.Decimal);
                    case TermGroup_AnnualProgressMatrixColumns.GoalPerMonth: return new MatrixField(rowNumber, column.Key, annualProgressItem.GoalPerMonth, MatrixDataType.Decimal);
                    case TermGroup_AnnualProgressMatrixColumns.LastYearAveragePerWeek: return new MatrixField(rowNumber, column.Key, annualProgressItem.LastYearAveragePerWeek, MatrixDataType.Decimal);
                    case TermGroup_AnnualProgressMatrixColumns.LastYearAveragePerMonth: return new MatrixField(rowNumber, column.Key, annualProgressItem.LastYearAveragePerMonth, MatrixDataType.Decimal);
                    case TermGroup_AnnualProgressMatrixColumns.DifferenceAverageFromLastYearPerWeek: return new MatrixField(rowNumber, column.Key, annualProgressItem.DifferenceAverageFromLastYearPerWeek, MatrixDataType.Decimal);
                    case TermGroup_AnnualProgressMatrixColumns.DifferenceAverageFromLastYearPerMonth: return new MatrixField(rowNumber, column.Key, annualProgressItem.DifferenceAverageFromLastYearPerMonth, MatrixDataType.Decimal);
                    case TermGroup_AnnualProgressMatrixColumns.DifferenceAverageFromGoalPerWeek: return new MatrixField(rowNumber, column.Key, annualProgressItem.DifferenceAverageFromGoalPerWeek, MatrixDataType.Decimal);
                    case TermGroup_AnnualProgressMatrixColumns.DifferenceAverageFromGoalPerMonth: return new MatrixField(rowNumber, column.Key, annualProgressItem.DifferenceAverageFromGoalPerMonth, MatrixDataType.Decimal);
                    case TermGroup_AnnualProgressMatrixColumns.RemainingYearAveragePerWeek: return new MatrixField(rowNumber, column.Key, annualProgressItem.RemainingYearAveragePerWeek, MatrixDataType.Decimal);
                    case TermGroup_AnnualProgressMatrixColumns.RemainingYearAveragePerMonth: return new MatrixField(rowNumber, column.Key, annualProgressItem.RemainingYearAveragePerMonth, MatrixDataType.Decimal);
                    case TermGroup_AnnualProgressMatrixColumns.SalesToDate: return new MatrixField(rowNumber, column.Key, annualProgressItem.SalesToDate, MatrixDataType.Decimal);
                    case TermGroup_AnnualProgressMatrixColumns.WorkingHoursToDate: return new MatrixField(rowNumber, column.Key, annualProgressItem.WorkingHoursToDate, MatrixDataType.Decimal);
                    case TermGroup_AnnualProgressMatrixColumns.FPATGoal: return new MatrixField(rowNumber, column.Key, annualProgressItem.FPATGoal, MatrixDataType.Decimal);
                    case TermGroup_AnnualProgressMatrixColumns.FPATToDate: return new MatrixField(rowNumber, column.Key, annualProgressItem.FPATToDate, MatrixDataType.Decimal);
                    case TermGroup_AnnualProgressMatrixColumns.AccountInternalNames: return new MatrixField(rowNumber, column.Key, annualProgressItem.AccountAnalysisFields.GetAccountAnalysisFieldValueName(column), column.MatrixDataType);
                    case TermGroup_AnnualProgressMatrixColumns.AccountInternalNrs: return new MatrixField(rowNumber, column.Key, annualProgressItem.AccountAnalysisFields.GetAccountAnalysisFieldValueNumber(column), column.MatrixDataType);
                    case TermGroup_AnnualProgressMatrixColumns.AveragePerWeekToDate: return new MatrixField(rowNumber, column.Key, annualProgressItem.AveragePerWeekToDate, MatrixDataType.Decimal);
                    case TermGroup_AnnualProgressMatrixColumns.AveragePerMonthToDate: return new MatrixField(rowNumber, column.Key, annualProgressItem.AveragePerMonthToDate, MatrixDataType.Decimal);
                    default:
                        break;
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}
