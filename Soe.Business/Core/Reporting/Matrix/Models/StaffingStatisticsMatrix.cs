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
    public class StaffingStatisticsMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "StaffingStatisticsMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        List<StaffingStatisticsReportDataField> filter { get; set; }
        List<StaffingStatisticsItem> staffingStatisticsItems { get; set; }
        public bool useAccountHierarchy { get; set; }
        #endregion

        public StaffingStatisticsMatrix(InputMatrix inputMatrix, StaffingStatisticsReportDataOutput reportDataOutput, int ActorCompanyId) : base(inputMatrix)
        {
            filter = reportDataOutput?.Input?.Columns;
            staffingStatisticsItems = reportDataOutput?.StaffingStatisticsItems;
            SettingManager sm = new SettingManager(null);
            this.useAccountHierarchy = sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseAccountHierarchy, 0, ActorCompanyId, 0);
        }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            if (!base.HasReadPermission(Feature.Time_Schedule_SchedulePlanning_Dashboard))
                return possibleColumns;

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_StaffingStatisticsMatrixColumns.Date));

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_StaffingStatisticsMatrixColumns.ScheduleSales));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_StaffingStatisticsMatrixColumns.ScheduleHours));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_StaffingStatisticsMatrixColumns.SchedulePersonelCost));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_StaffingStatisticsMatrixColumns.ScheduleSalaryPercent));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_StaffingStatisticsMatrixColumns.ScheduleLPAT));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_StaffingStatisticsMatrixColumns.ScheduleFPAT));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_StaffingStatisticsMatrixColumns.ScheduleBPAT));

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_StaffingStatisticsMatrixColumns.TimeSales));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_StaffingStatisticsMatrixColumns.TimeHours));
            if (base.HasReadPermission(Feature.Time_Schedule_SchedulePlanning_ShowCosts))
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_StaffingStatisticsMatrixColumns.TimePersonelCost));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_StaffingStatisticsMatrixColumns.TimeSalaryPercent));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_StaffingStatisticsMatrixColumns.TimeLPAT));
            }
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_StaffingStatisticsMatrixColumns.TimeFPAT));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_StaffingStatisticsMatrixColumns.TimeBPAT));

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_StaffingStatisticsMatrixColumns.ForecastSales));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_StaffingStatisticsMatrixColumns.ForecastHours));
            if (base.HasReadPermission(Feature.Time_Schedule_SchedulePlanning_ShowCosts))
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_StaffingStatisticsMatrixColumns.ForecastPersonelCost));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_StaffingStatisticsMatrixColumns.ForecastSalaryPercent));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_StaffingStatisticsMatrixColumns.ForecastLPAT));
            }
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_StaffingStatisticsMatrixColumns.ForecastFPAT));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_StaffingStatisticsMatrixColumns.ForecastBPAT));

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_StaffingStatisticsMatrixColumns.BudgetSales));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_StaffingStatisticsMatrixColumns.BudgetHours));
            if (base.HasReadPermission(Feature.Time_Schedule_SchedulePlanning_ShowCosts))
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_StaffingStatisticsMatrixColumns.BudgetPersonelCost));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_StaffingStatisticsMatrixColumns.BudgetSalaryPercent));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_StaffingStatisticsMatrixColumns.BudgetLPAT));
            }
            if (base.HasReadPermission(Feature.Time_Schedule_SchedulePlanning_ShowCosts))
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_StaffingStatisticsMatrixColumns.ScheduleAndTimePersonalCost));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_StaffingStatisticsMatrixColumns.ScheduleAndTimeHours));

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_StaffingStatisticsMatrixColumns.TemplateScheduleSales));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_StaffingStatisticsMatrixColumns.TemplateScheduleHours));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_StaffingStatisticsMatrixColumns.TemplateSchedulePersonelCost));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_StaffingStatisticsMatrixColumns.TemplateScheduleSalaryPercent));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_StaffingStatisticsMatrixColumns.TemplateScheduleLPAT));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_StaffingStatisticsMatrixColumns.TemplateScheduleFPAT));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_StaffingStatisticsMatrixColumns.TemplateScheduleBPAT));

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_StaffingStatisticsMatrixColumns.TimeSales));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_StaffingStatisticsMatrixColumns.TimeHours));

            int nbrOfAccountDims = inputMatrix?.AccountDims?.Count(w => w.AccountDimNr != 1) ?? 0;
            if (nbrOfAccountDims > 0 && (base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Accounts) || base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Categories)))
            {
                foreach (var dim in inputMatrix.AccountDims.Where(w => w.IsInternal))
                {
                    MatrixDefinitionColumnOptions options = new MatrixDefinitionColumnOptions();
                    options.Key = dim.AccountDimId.ToString();
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_StaffingStatisticsMatrixColumns.AccountInternalNrs, options, dim.Name + " " + GetText(507, "Nummer")));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_StaffingStatisticsMatrixColumns.AccountInternalNames, options, dim.Name + " " + GetText(508, "Namn")));
                }
            }

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_StaffingStatisticsMatrixColumns.EmployeeName, null, GetText(508, "Namn")));

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

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_StaffingStatisticsMatrixColumns column, MatrixDefinitionColumnOptions options = null, string overrideTitle = null)
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

            foreach (var employee in staffingStatisticsItems)
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

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, StaffingStatisticsItem staffingStatisticsItem)
        {
            if (base.GetEnumId<TermGroup_StaffingStatisticsMatrixColumns>(column, out int id))
            {
                var type = (TermGroup_StaffingStatisticsMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_StaffingStatisticsMatrixColumns.Date: return new MatrixField(rowNumber, column.Key, staffingStatisticsItem.Date, column.MatrixDataType);
                    case TermGroup_StaffingStatisticsMatrixColumns.ForecastSales: return new MatrixField(rowNumber, column.Key, staffingStatisticsItem.ForecastSales, column.MatrixDataType);
                    case TermGroup_StaffingStatisticsMatrixColumns.ForecastHours: return new MatrixField(rowNumber, column.Key, staffingStatisticsItem.ForecastHours, column.MatrixDataType);
                    case TermGroup_StaffingStatisticsMatrixColumns.ForecastPersonelCost: return new MatrixField(rowNumber, column.Key, staffingStatisticsItem.ForecastPersonelCost, column.MatrixDataType);
                    case TermGroup_StaffingStatisticsMatrixColumns.ForecastSalaryPercent: return new MatrixField(rowNumber, column.Key, staffingStatisticsItem.ForecastSalaryPercent, column.MatrixDataType);
                    case TermGroup_StaffingStatisticsMatrixColumns.ForecastLPAT: return new MatrixField(rowNumber, column.Key, staffingStatisticsItem.ForecastLPAT, column.MatrixDataType);
                    case TermGroup_StaffingStatisticsMatrixColumns.ForecastFPAT: return new MatrixField(rowNumber, column.Key, staffingStatisticsItem.ForecastFPAT, column.MatrixDataType);
                    case TermGroup_StaffingStatisticsMatrixColumns.ForecastBPAT: return new MatrixField(rowNumber, column.Key, staffingStatisticsItem.ForecastBPAT, column.MatrixDataType);
                    case TermGroup_StaffingStatisticsMatrixColumns.BudgetSales: return new MatrixField(rowNumber, column.Key, staffingStatisticsItem.BudgetSales, column.MatrixDataType);
                    case TermGroup_StaffingStatisticsMatrixColumns.BudgetHours: return new MatrixField(rowNumber, column.Key, staffingStatisticsItem.BudgetHours, column.MatrixDataType);
                    case TermGroup_StaffingStatisticsMatrixColumns.BudgetPersonelCost: return new MatrixField(rowNumber, column.Key, staffingStatisticsItem.BudgetPersonelCost, column.MatrixDataType);
                    case TermGroup_StaffingStatisticsMatrixColumns.BudgetSalaryPercent: return new MatrixField(rowNumber, column.Key, staffingStatisticsItem.BudgetSalaryPercent, column.MatrixDataType);
                    case TermGroup_StaffingStatisticsMatrixColumns.BudgetLPAT: return new MatrixField(rowNumber, column.Key, staffingStatisticsItem.BudgetLPAT, column.MatrixDataType);
                    case TermGroup_StaffingStatisticsMatrixColumns.BudgetFPAT: return new MatrixField(rowNumber, column.Key, staffingStatisticsItem.BudgetFPAT, column.MatrixDataType);
                    case TermGroup_StaffingStatisticsMatrixColumns.BudgetBPAT: return new MatrixField(rowNumber, column.Key, staffingStatisticsItem.BudgetBPAT, column.MatrixDataType);
                    case TermGroup_StaffingStatisticsMatrixColumns.TemplateScheduleSales: return new MatrixField(rowNumber, column.Key, staffingStatisticsItem.TemplateScheduleSales, column.MatrixDataType);
                    case TermGroup_StaffingStatisticsMatrixColumns.TemplateScheduleHours: return new MatrixField(rowNumber, column.Key, staffingStatisticsItem.TemplateScheduleHours, column.MatrixDataType);
                    case TermGroup_StaffingStatisticsMatrixColumns.TemplateSchedulePersonelCost: return new MatrixField(rowNumber, column.Key, staffingStatisticsItem.TemplateSchedulePersonelCost, column.MatrixDataType);
                    case TermGroup_StaffingStatisticsMatrixColumns.TemplateScheduleSalaryPercent: return new MatrixField(rowNumber, column.Key, staffingStatisticsItem.TemplateScheduleSalaryPercent, column.MatrixDataType);
                    case TermGroup_StaffingStatisticsMatrixColumns.TemplateScheduleLPAT: return new MatrixField(rowNumber, column.Key, staffingStatisticsItem.TemplateScheduleLPAT, column.MatrixDataType);
                    case TermGroup_StaffingStatisticsMatrixColumns.TemplateScheduleFPAT: return new MatrixField(rowNumber, column.Key, staffingStatisticsItem.TemplateScheduleFPAT, column.MatrixDataType);
                    case TermGroup_StaffingStatisticsMatrixColumns.TemplateScheduleBPAT: return new MatrixField(rowNumber, column.Key, staffingStatisticsItem.TemplateScheduleBPAT, column.MatrixDataType);
                    case TermGroup_StaffingStatisticsMatrixColumns.ScheduleSales: return new MatrixField(rowNumber, column.Key, staffingStatisticsItem.ScheduleSales, column.MatrixDataType);
                    case TermGroup_StaffingStatisticsMatrixColumns.ScheduleHours: return new MatrixField(rowNumber, column.Key, staffingStatisticsItem.ScheduleHours, column.MatrixDataType);
                    case TermGroup_StaffingStatisticsMatrixColumns.SchedulePersonelCost: return new MatrixField(rowNumber, column.Key, staffingStatisticsItem.SchedulePersonelCost, column.MatrixDataType);
                    case TermGroup_StaffingStatisticsMatrixColumns.ScheduleSalaryPercent: return new MatrixField(rowNumber, column.Key, staffingStatisticsItem.ScheduleSalaryPercent, column.MatrixDataType);
                    case TermGroup_StaffingStatisticsMatrixColumns.ScheduleLPAT: return new MatrixField(rowNumber, column.Key, staffingStatisticsItem.ScheduleLPAT, column.MatrixDataType);
                    case TermGroup_StaffingStatisticsMatrixColumns.ScheduleFPAT: return new MatrixField(rowNumber, column.Key, staffingStatisticsItem.ScheduleFPAT, column.MatrixDataType);
                    case TermGroup_StaffingStatisticsMatrixColumns.ScheduleBPAT: return new MatrixField(rowNumber, column.Key, staffingStatisticsItem.ScheduleBPAT, column.MatrixDataType);
                    case TermGroup_StaffingStatisticsMatrixColumns.TimeSales: return new MatrixField(rowNumber, column.Key, staffingStatisticsItem.TimeSales, column.MatrixDataType);
                    case TermGroup_StaffingStatisticsMatrixColumns.TimeHours: return new MatrixField(rowNumber, column.Key, staffingStatisticsItem.TimeHours, column.MatrixDataType);
                    case TermGroup_StaffingStatisticsMatrixColumns.TimePersonelCost: return new MatrixField(rowNumber, column.Key, staffingStatisticsItem.TimePersonelCost, column.MatrixDataType);
                    case TermGroup_StaffingStatisticsMatrixColumns.TimeSalaryPercent: return new MatrixField(rowNumber, column.Key, staffingStatisticsItem.TimeSalaryPercent, column.MatrixDataType);
                    case TermGroup_StaffingStatisticsMatrixColumns.TimeLPAT: return new MatrixField(rowNumber, column.Key, staffingStatisticsItem.TimeLPAT, column.MatrixDataType);
                    case TermGroup_StaffingStatisticsMatrixColumns.TimeFPAT: return new MatrixField(rowNumber, column.Key, staffingStatisticsItem.TimeFPAT, column.MatrixDataType);
                    case TermGroup_StaffingStatisticsMatrixColumns.TimeBPAT: return new MatrixField(rowNumber, column.Key, staffingStatisticsItem.TimeBPAT, column.MatrixDataType);
                    case TermGroup_StaffingStatisticsMatrixColumns.ScheduleAndTimeHours: return new MatrixField(rowNumber, column.Key, staffingStatisticsItem.ScheduleAndTimeHours, column.MatrixDataType);
                    case TermGroup_StaffingStatisticsMatrixColumns.ScheduleAndTimePersonalCost: return new MatrixField(rowNumber, column.Key, staffingStatisticsItem.ScheduleAndTimePersonalCost, column.MatrixDataType);
                    case TermGroup_StaffingStatisticsMatrixColumns.EmployeeName: return new MatrixField(rowNumber, column.Key, staffingStatisticsItem.EmployeeName, column.MatrixDataType);
                    case TermGroup_StaffingStatisticsMatrixColumns.AccountInternalNames: return new MatrixField(rowNumber, column.Key, staffingStatisticsItem.AccountAnalysisFields.GetAccountAnalysisFieldValueName(column), column.MatrixDataType);
                    case TermGroup_StaffingStatisticsMatrixColumns.AccountInternalNrs: return new MatrixField(rowNumber, column.Key, staffingStatisticsItem.AccountAnalysisFields.GetAccountAnalysisFieldValueNumber(column), column.MatrixDataType);
                    default:
                        break;
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}
