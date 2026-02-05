using SoftOne.Soe.Business.Core.Reporting.Matrix.Models;
using SoftOne.Soe.Business.Core.Reporting.Matrix.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models;
using SoftOne.Soe.Business.Core.Reporting.Models.Time;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix
{
    public class VacationBalanceMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "VacationBalanceMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        List<VacationBalanceReportDataField> filter { get; set; }
        VacationBalanceReportDataOutput _reportDataOutput { get; set; }
        readonly bool useAccountHierarchy;
        #endregion

        public VacationBalanceMatrix(InputMatrix inputMatrix, VacationBalanceReportDataOutput reportDataOutput, int actorCompanyId) : base(inputMatrix)
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

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_VacationBalanceMatrixColumns.EmploymentNr));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_VacationBalanceMatrixColumns.Name));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_VacationBalanceMatrixColumns.FirstName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_VacationBalanceMatrixColumns.LastName));
            if (base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec))
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_VacationBalanceMatrixColumns.SocialSecurityNumber));

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_VacationBalanceMatrixColumns.Active));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_VacationBalanceMatrixColumns.BirthYear));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_VacationBalanceMatrixColumns.Age));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_VacationBalanceMatrixColumns.Gender));

            if (base.HasReadPermission(Feature.Manage_Users_Edit_UserMapping) || base.HasReadPermission(Feature.Manage_Users_Edit_AttestRoleMapping))
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_VacationBalanceMatrixColumns.Roles));

            if (base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments))
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_VacationBalanceMatrixColumns.EmploymentPosition));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_VacationBalanceMatrixColumns.PayrollAgreement));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_VacationBalanceMatrixColumns.ContractGroup));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_VacationBalanceMatrixColumns.VacationAgreement));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Time, TermGroup_VacationBalanceMatrixColumns.WeeklyWorkingHours, new MatrixDefinitionColumnOptions() { MinutesToTimeSpan = true }));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_VacationBalanceMatrixColumns.EmploymentRate));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Time, TermGroup_VacationBalanceMatrixColumns.BasicWeeklyWorkingHours, new MatrixDefinitionColumnOptions() { MinutesToTimeSpan = true }));

                if ((base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_AbsenceVacation_Vacation) || base.HasReadPermission(Feature.Time_Employee_Employees_Edit_MySelf_AbsenceVacation_Absence))
                    || (base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Contact_Children) || base.HasReadPermission(Feature.Time_Employee_Employees_Edit_MySelf_Contact_Children))
                    || (base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_AbsenceVacation_Vacation) || base.HasReadPermission(Feature.Time_Employee_Employees_Edit_MySelf_AbsenceVacation_Vacation)))
                {
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_VacationBalanceMatrixColumns.PaidEarnedDays));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_VacationBalanceMatrixColumns.PaidSelectedDays));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_VacationBalanceMatrixColumns.PaidRemainingDays));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_VacationBalanceMatrixColumns.PaidSysDegreeEarned));

                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_VacationBalanceMatrixColumns.PaidHolidayAllowance));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_VacationBalanceMatrixColumns.PaidVariableVacationSupplementsSelectedDays));

                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_VacationBalanceMatrixColumns.UnpaidEarnedDays));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_VacationBalanceMatrixColumns.UnpaidSelectedDays));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_VacationBalanceMatrixColumns.UnpaidRemainingDays));

                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_VacationBalanceMatrixColumns.AdvanceEarnedDays));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_VacationBalanceMatrixColumns.AdvanceSelectedDays));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_VacationBalanceMatrixColumns.AdvanceRemaininDays));

                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_VacationBalanceMatrixColumns.DebtCashAdvancesAmount));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_VacationBalanceMatrixColumns.DebtCashAdvancesDecay));

                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_VacationBalanceMatrixColumns.SavedYear1EarnedDays));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_VacationBalanceMatrixColumns.SavedYear1SelectedDays));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_VacationBalanceMatrixColumns.SavedYear1RemaininDays));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_VacationBalanceMatrixColumns.SavedYear1SysDegreeEarned));

                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_VacationBalanceMatrixColumns.SavedYear2EarnedDays));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_VacationBalanceMatrixColumns.SavedYear2SelectedDays));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_VacationBalanceMatrixColumns.SavedYear2RemaininDays));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_VacationBalanceMatrixColumns.SavedYear2SysDegreeEarned));

                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_VacationBalanceMatrixColumns.SavedYear3EarnedDays));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_VacationBalanceMatrixColumns.SavedYear3SelectedDays));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_VacationBalanceMatrixColumns.SavedYear3RemaininDays));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_VacationBalanceMatrixColumns.SavedYear3SysDegreeEarned));

                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_VacationBalanceMatrixColumns.SavedYear4EarnedDays));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_VacationBalanceMatrixColumns.SavedYear4SelectedDays));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_VacationBalanceMatrixColumns.SavedYear4RemaininDays));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_VacationBalanceMatrixColumns.SavedYear4SysDegreeEarned));

                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_VacationBalanceMatrixColumns.SavedYear5EarnedDays));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_VacationBalanceMatrixColumns.SavedYear5SelectedDays));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_VacationBalanceMatrixColumns.SavedYear5RemaininDays));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_VacationBalanceMatrixColumns.SavedYear5SysDegreeEarned));

                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_VacationBalanceMatrixColumns.OverdueDaysEarnedDays));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_VacationBalanceMatrixColumns.OverdueDaysSelectedDays));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_VacationBalanceMatrixColumns.OverdueDaysRemainingDays));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_VacationBalanceMatrixColumns.OverdueDaysSysDegreeEarned));

                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_VacationBalanceMatrixColumns.PreliminaryWithdrawnRemaininDays));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_VacationBalanceMatrixColumns.RemainingSelectedDays));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_VacationBalanceMatrixColumns.RemainingRemainingDays));
                }
            }

            if (!this.useAccountHierarchy)
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_VacationBalanceMatrixColumns.Categories));

            int nbrOfAccountDims = inputMatrix?.AccountDims?.Count(w => w.AccountDimNr != 1) ?? 0;
            if (nbrOfAccountDims > 0 && base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Accounts))
            {
                foreach (var dim in inputMatrix.AccountDims)
                {
                    MatrixDefinitionColumnOptions options = new MatrixDefinitionColumnOptions();
                    options.Key = dim.AccountDimId.ToString();
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_VacationBalanceMatrixColumns.AccountInternalNrs, options, dim.Name + " " + GetText(507, "Nummer")));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_VacationBalanceMatrixColumns.AccountInternalNames, options, dim.Name + " " + GetText(508, "Namn")));
                }
            }

            if (base.HasReadPermission(Feature.Common_ExtraFields_Employee) && !inputMatrix.ExtraFields.IsNullOrEmpty())
            {
                foreach (var extraField in inputMatrix.ExtraFields)
                {
                    MatrixDefinitionColumnOptions options = new MatrixDefinitionColumnOptions();
                    options.Key = extraField.ExtraFieldId.ToString();
                    possibleColumns.Add(CreateMatrixLayoutColumn(SetMatrixDataType((TermGroup_ExtraFieldType)extraField.Type), TermGroup_VacationBalanceMatrixColumns.ExtraFieldEmployee, options, extraField.Text));
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
        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_VacationBalanceMatrixColumns column, MatrixDefinitionColumnOptions options = null, string overrideTitle = null)
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

            foreach (var employeeSalaryItem in _reportDataOutput.VacationBalanceItems)
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

        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, VacationBalanceItem vacationBalanceItem)
        {
            if (base.GetEnumId<TermGroup_VacationBalanceMatrixColumns>(column, out int id))
            {
                var type = (TermGroup_VacationBalanceMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_VacationBalanceMatrixColumns.EmploymentNr:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.EmploymentNr, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.Name:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.EmployeeName, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.FirstName:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.FirstName, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.LastName:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.LastName, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.SocialSecurityNumber:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.SocialSecurityNumber, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.Active:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.Active, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.Categories:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.Categories, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.BirthYear:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.BirthYearMonth, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.Age:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.Age, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.Gender:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.Gender, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.Roles:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.Roles, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.EmploymentPosition:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.EmploymentPosition, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.PayrollAgreement:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.PayrollAgreement, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.ContractGroup:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.ContractGroup, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.VacationAgreement:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.VacationAgreement, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.WeeklyWorkingHours:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.WeeklyWorkingHours, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.EmploymentRate:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.EmploymentRate, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.BasicWeeklyWorkingHours:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.BasicWeeklyWorkingHours, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.PaidEarnedDays:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.PaidEarnDays, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.PaidSelectedDays:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.PaidSelectedDays, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.PaidRemainingDays:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.PaidRemainingDays, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.PaidSysDegreeEarned:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.PaidSysDegreeEarned, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.PaidHolidayAllowance:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.PaidHolidayAllowance, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.PaidVariableVacationSupplementsSelectedDays:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.PaidVariableVacationSupplementsSelectedDays, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.UnpaidEarnedDays:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.UnpaidEarnedDays, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.UnpaidSelectedDays:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.UnpaidSelectedDays, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.UnpaidRemainingDays:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.UnpaidRemainingDays, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.AdvanceEarnedDays:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.AdvanceEarnedDays, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.AdvanceSelectedDays:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.AdvanceSelectedDays, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.AdvanceRemaininDays:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.AdvanceRemaininDays, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.DebtCashAdvancesAmount:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.DebtCashAdvancesAmount, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.DebtCashAdvancesDecay:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.DebtCashAdvancesDecay, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.SavedYear1EarnedDays:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.SavedYear1EarnedDays, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.SavedYear1SelectedDays:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.SavedYear1SelectedDays, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.SavedYear1RemaininDays:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.SavedYear1RemaininDays, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.SavedYear1SysDegreeEarned:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.SavedYear1SysDegreeEarned, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.SavedYear2EarnedDays:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.SavedYear2EarnedDays, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.SavedYear2SelectedDays:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.SavedYear2SelectedDays, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.SavedYear2RemaininDays:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.SavedYear2RemaininDays, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.SavedYear2SysDegreeEarned:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.SavedYear2SysDegreeEarned, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.SavedYear3EarnedDays:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.SavedYear3EarnedDays, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.SavedYear3SelectedDays:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.SavedYear3SelectedDays, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.SavedYear3RemaininDays:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.SavedYear3RemaininDays, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.SavedYear3SysDegreeEarned:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.SavedYear3SysDegreeEarned, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.SavedYear4EarnedDays:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.SavedYear4EarnedDays, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.SavedYear4SelectedDays:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.SavedYear4SelectedDays, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.SavedYear4RemaininDays:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.SavedYear4RemaininDays, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.SavedYear4SysDegreeEarned:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.SavedYear4SysDegreeEarned, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.SavedYear5EarnedDays:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.SavedYear5EarnedDays, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.SavedYear5SelectedDays:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.SavedYear5SelectedDays, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.SavedYear5RemaininDays:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.SavedYear5RemaininDays, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.SavedYear5SysDegreeEarned:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.SavedYear5SysDegreeEarned, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.OverdueDaysEarnedDays:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.OverdueDaysEarnedDays, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.OverdueDaysSelectedDays:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.OverdueDaysSelectedDays, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.OverdueDaysRemainingDays:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.OverdueDaysRemainingDays, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.OverdueDaysSysDegreeEarned:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.OverdueDaysSysDegreeEarned, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.PreliminaryWithdrawnRemaininDays:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.PreliminaryWithdrawnRemaininDays, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.RemainingSelectedDays:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.RemainingSelectedDays, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.RemainingRemainingDays:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.RemainingRemainingDays, column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.AccountInternalNames:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.AccountAnalysisFields.GetAccountAnalysisFieldValueName(column), column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.AccountInternalNrs:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.AccountAnalysisFields.GetAccountAnalysisFieldValueNumber(column), column.MatrixDataType);
                    case TermGroup_VacationBalanceMatrixColumns.ExtraFieldEmployee:
                        return new MatrixField(rowNumber, column.Key, vacationBalanceItem.ExtraFieldAnalysisFields.ExtraFieldAnalysisFieldValue(column), column.MatrixDataType);
                    default:
                        break;
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}
