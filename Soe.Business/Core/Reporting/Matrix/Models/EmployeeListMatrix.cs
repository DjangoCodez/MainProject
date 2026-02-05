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
    public class EmployeeListMatrix : BaseMatrix, IMatrixModel
    {
        #region Column

        public static readonly string prefix = "EmployeeListMatrix";
        private List<MatrixDefinitionColumn> definitionColumns { get; set; }
        List<EmployeeListReportDataReportDataField> filter { get; set; }
        public int ActorCompanyId { get; set; }
        readonly bool useAccountHierarchy;

        #endregion

        public EmployeeListMatrix(InputMatrix inputMatrix, EmployeeListReportDataOutput reportDataOutput, int ActorCompanyID) : base(inputMatrix)
        {
            Employees = reportDataOutput != null ? reportDataOutput.Employees : new List<EmployeeListItem>();
            filter = reportDataOutput?.Input?.Columns;
            SettingManager sm = new SettingManager(null);
            this.useAccountHierarchy = sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseAccountHierarchy, 0, ActorCompanyID, 0);
        }

        List<EmployeeListItem> Employees { get; set; }

        public List<MatrixLayoutColumn> GetMatrixLayoutColumns()
        {
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            if (!base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees))
                return possibleColumns;

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_EmployeeListMatrixColumns.EmployeeId, new MatrixDefinitionColumnOptions() { Hidden = true }));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.EmployeeNr));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.EmployeeName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.FirstName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.LastName));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.EmployeeExternalCode));
            if (base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec))
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.SocialSec));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.Gender));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_EmployeeListMatrixColumns.Age));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_EmployeeListMatrixColumns.BirthDate));

            if (base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll) && base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll_Salary))
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_EmployeeListMatrixColumns.ExcludeFromPayroll));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_EmployeeListMatrixColumns.Vacant));

            if (base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_User))
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.UserName));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.Language));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.DefaultCompany));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.DefaultRole));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_EmployeeListMatrixColumns.IsMobileUser));
            }

            if (base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Contact))
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.DistributionAddress));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.Email));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.CellPhone));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.HomePhone));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.ClosestRelative));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.DistributionAddressRow));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.DistributionAddressRow2));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.DistributionZipCode));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.DistributionCity));
            }

            if (base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Time_CalculatedCostPerHour))
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_EmployeeListMatrixColumns.EmployeeCalculatedCostPerHour));

            if (base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Note))
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.Note));

            if (base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments))
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_EmployeeListMatrixColumns.EmploymentDate));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_EmployeeListMatrixColumns.FirstEmploymentDate));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_EmployeeListMatrixColumns.EndDate));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_EmployeeListMatrixColumns.LASDays));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.EmploymentTypeName));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.PayrollGroupName));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.EmployeeGroupName));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.VacationGroupName));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Time, TermGroup_EmployeeListMatrixColumns.WorkTimeWeekMinutes, new MatrixDefinitionColumnOptions() { MinutesToTimeSpan = true }));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_EmployeeListMatrixColumns.WorkTimeWeekPercent));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.WorkPlace));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_EmployeeListMatrixColumns.HasSecondaryEmployment));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.EmplymentExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.EmplymentTypeExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.EmploymentTypeOnSecondaryEmployment));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.ExternalCode));

                if (base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll_Salary))
                {
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_EmployeeListMatrixColumns.Salary));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_EmployeeListMatrixColumns.HourlySalary));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_EmployeeListMatrixColumns.MonthlySalary));
                }
                if (base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_AbsenceVacation_Vacation))
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.VacationDaysPaidByLaw));
            }

            if (base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Contact_DisbursementAccount))
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.DisbursementMethodText));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.DisbursementClearingNr));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.DisbursementAccountNr));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.DisbursementCountryCode));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.DisbursementBIC));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.DisbursementIBAN));
            }

            if (base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Skills))
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.SSYKCode));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.SSYKName));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.PositionCode));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.PositionName));
            }

            if (base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Reports))
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.PayrollStatisticsPersonalCategory));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.PayrollStatisticsWorkTimeCategory));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.PayrollStatisticsSalaryType));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.PayrollStatisticsWorkPlaceNumber));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.PayrollStatisticsCFARNumber));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.WorkPlaceSCB));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_EmployeeListMatrixColumns.PartnerInCloseCompany));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_EmployeeListMatrixColumns.BenefitAsPension));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.AFACategory));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.AFASpecialAgreement));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.AFAWorkplaceNr));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_EmployeeListMatrixColumns.AFAParttimePensionCode));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.CollectumITPPlan));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.CollectumCostPlace));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.CollectumAgreedOnProduct));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Date, TermGroup_EmployeeListMatrixColumns.CollectumCancellationDate));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_EmployeeListMatrixColumns.CollectumCancellationDateIsLeaveOfAbsence));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.KPARetirementAge));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.KPABelonging));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.KPAEndCode));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.KPAAgreementType));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.BygglosenAgreementArea));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.BygglosenAllocationNumber));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.BygglosenMunicipalCode));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.BygglosenSalaryFormula));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.BygglosenProfessionCategory));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.BygglosenSalaryType));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.BygglosenWorkPlaceNumber));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.BygglosenLendedToOrgNr));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Decimal, TermGroup_EmployeeListMatrixColumns.BygglosenAgreedHourlyPayLevel));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.GTPAgreementNumber));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_EmployeeListMatrixColumns.GTPExcluded));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Integer, TermGroup_EmployeeListMatrixColumns.IFAssociationNumber));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.IFWorkPlace));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.IFPaymentCode));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.AGIPlaceOfEmploymentAddress));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.AGIPlaceOfEmploymentCity));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.Boolean, TermGroup_EmployeeListMatrixColumns.AGIPlaceOfEmploymentIgnore));
            }

            if (base.HasReadPermission(Feature.Manage_Users_Edit))
            {
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.NearestExecutiveUserName));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.NearestExecutiveName));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.NearestExecutiveEmail));
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.NearestExecutiveCellphone));
                if (base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec))
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.NearestExecutiveSocialSec));
            }

            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.DateAndTime, TermGroup_EmployeeListMatrixColumns.Created));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.CreatedBy));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.DateAndTime, TermGroup_EmployeeListMatrixColumns.Modified));
            possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.ModifiedBy));

            if (!useAccountHierarchy)
                possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.CategoryName));

            int nbrOfAccountDims = inputMatrix?.AccountDims?.Count(w => w.AccountDimNr != 1) ?? 0;
            if (nbrOfAccountDims > 0 && base.HasReadPermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Accounts))
            {
                foreach (var dim in inputMatrix.AccountDims)
                {
                    MatrixDefinitionColumnOptions options = new MatrixDefinitionColumnOptions();
                    options.Key = dim.AccountDimId.ToString();
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.AccountInternalNrs, options, dim.Name + " " + GetText(507, "Nummer")));
                    possibleColumns.Add(CreateMatrixLayoutColumn(MatrixDataType.String, TermGroup_EmployeeListMatrixColumns.AccountInternalNames, options, dim.Name + " " + GetText(508, "Namn")));
                }
            }

            if (base.HasReadPermission(Feature.Common_ExtraFields_Employee) && !inputMatrix.ExtraFields.IsNullOrEmpty())
            {
                foreach (var extraField in inputMatrix.ExtraFields)
                {
                    MatrixDefinitionColumnOptions options = new MatrixDefinitionColumnOptions();
                    options.Key = extraField.ExtraFieldId.ToString();
                    possibleColumns.Add(CreateMatrixLayoutColumn(SetMatrixDataType(extraField.Type), TermGroup_EmployeeListMatrixColumns.ExtraFieldEmployee, options, extraField.Text));
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

        private MatrixLayoutColumn CreateMatrixLayoutColumn(MatrixDataType dataType, TermGroup_EmployeeListMatrixColumns column, MatrixDefinitionColumnOptions options = null, string overrideTitle = null)
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

            foreach (var employee in Employees)
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
        private MatrixField CreateField(int rowNumber, MatrixDefinitionColumn column, EmployeeListItem employee)
        {
            if (base.GetEnumId<TermGroup_EmployeeListMatrixColumns>(column, out int id))
            {
                var type = (TermGroup_EmployeeListMatrixColumns)id;

                switch (type)
                {
                    case TermGroup_EmployeeListMatrixColumns.EmployeeId:
                        return new MatrixField(rowNumber, column.Key, employee.EmployeeId, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.EmployeeNr:
                        return new MatrixField(rowNumber, column.Key, employee.EmployeeNr, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.EmployeeName:
                        return new MatrixField(rowNumber, column.Key, employee.EmployeeName, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.FirstName:
                        return new MatrixField(rowNumber, column.Key, employee.FirstName, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.LastName:
                        return new MatrixField(rowNumber, column.Key, employee.LastName, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.SocialSec:
                        return new MatrixField(rowNumber, column.Key, employee.SocialSec, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.EmployeeExternalCode:
                        return new MatrixField(rowNumber, column.Key, employee.EmployeeExternalCode, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.ExternalAuthId:
                        return new MatrixField(rowNumber, column.Key, employee.ExternalAuthId, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.Gender:
                        return new MatrixField(rowNumber, column.Key, employee.Gender, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.Age:
                        return new MatrixField(rowNumber, column.Key, employee.Age, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.BirthDate:
                        return new MatrixField(rowNumber, column.Key, employee.BirthDate, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.UserName:
                        return new MatrixField(rowNumber, column.Key, employee.UserName, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.Language:
                        return new MatrixField(rowNumber, column.Key, employee.Language, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.DefaultCompany:
                        return new MatrixField(rowNumber, column.Key, employee.DefaultCompany, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.DefaultRole:
                        return new MatrixField(rowNumber, column.Key, employee.DefaultRole, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.IsMobileUser:
                        return new MatrixField(rowNumber, column.Key, employee.IsMobileUser, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.IsSysUser:
                        return new MatrixField(rowNumber, column.Key, employee.IsSysUser, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.EmployeeCalculatedCostPerHour:
                        return new MatrixField(rowNumber, column.Key, employee.EmployeeCalculatedCostPerHour, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.Note:
                        return new MatrixField(rowNumber, column.Key, employee.Note, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.EmploymentDate:
                        return new MatrixField(rowNumber, column.Key, employee.EmploymentDate, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.FirstEmploymentDate:
                        return new MatrixField(rowNumber, column.Key, employee.FirstEmploymentDate, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.EndDate:
                        return new MatrixField(rowNumber, column.Key, employee.EndDate, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.LASDays:
                        return new MatrixField(rowNumber, column.Key, employee.LASDays, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.EmploymentTypeName:
                        return new MatrixField(rowNumber, column.Key, employee.EmploymentTypeName, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.PayrollGroupName:
                        return new MatrixField(rowNumber, column.Key, employee.PayrollGroupName, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.EmployeeGroupName:
                        return new MatrixField(rowNumber, column.Key, employee.EmployeeGroupName, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.VacationGroupName:
                        return new MatrixField(rowNumber, column.Key, employee.VacationGroupName, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.WorkTimeWeekMinutes:
                        return new MatrixField(rowNumber, column.Key, employee.WorkTimeWeekMinutes, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.WorkTimeWeekPercent:
                        return new MatrixField(rowNumber, column.Key, employee.WorkTimeWeekPercent, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.WorkPlace:
                        return new MatrixField(rowNumber, column.Key, employee.WorkPlace, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.HasSecondaryEmployment:
                        return new MatrixField(rowNumber, column.Key, employee.HasSecondaryEmployment, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.EmploymentTypeOnSecondaryEmployment:
                        return new MatrixField(rowNumber, column.Key, employee.EmploymentTypeOnSecondaryEmployment, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.EmplymentExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment:
                        return new MatrixField(rowNumber, column.Key, employee.SecondaryEmploymentExcludeFromWorkTimeWeekCalculation, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.EmplymentTypeExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment:
                        return new MatrixField(rowNumber, column.Key, employee.SecondaryEmploymentExcludeFromWorkTimeWeekCalculationEmploymentType, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.ExternalCode:
                        return new MatrixField(rowNumber, column.Key, employee.EmploymentExternalCode, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.DisbursementMethodText:
                        return new MatrixField(rowNumber, column.Key, employee.DisbursementMethodText, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.DisbursementClearingNr:
                        return new MatrixField(rowNumber, column.Key, employee.DisbursementClearingNr, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.DisbursementAccountNr:
                        return new MatrixField(rowNumber, column.Key, employee.DisbursementAccountNr, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.DisbursementCountryCode:
                        return new MatrixField(rowNumber, column.Key, employee.DisbursementCountryCode, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.DisbursementBIC:
                        return new MatrixField(rowNumber, column.Key, employee.DisbursementBIC, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.DisbursementIBAN:
                        return new MatrixField(rowNumber, column.Key, employee.DisbursementIBAN, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.SSYKCode:
                        return new MatrixField(rowNumber, column.Key, employee.SSYKCode, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.SSYKName:
                        return new MatrixField(rowNumber, column.Key, employee.SSYKName, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.PositionCode:
                        return new MatrixField(rowNumber, column.Key, employee.Position?.Code ?? "", column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.PositionName:
                        return new MatrixField(rowNumber, column.Key, employee.Position?.Name ?? "", column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.PositionSysName:
                        return new MatrixField(rowNumber, column.Key, employee.Position?.SysPositionName ?? "", column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.PayrollStatisticsPersonalCategory:
                        return new MatrixField(rowNumber, column.Key, employee.PayrollStatisticsPersonalCategory, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.PayrollStatisticsWorkTimeCategory:
                        return new MatrixField(rowNumber, column.Key, employee.PayrollStatisticsWorkTimeCategory, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.PayrollStatisticsSalaryType:
                        return new MatrixField(rowNumber, column.Key, employee.PayrollStatisticsSalaryType, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.PayrollStatisticsWorkPlaceNumber:
                        return new MatrixField(rowNumber, column.Key, employee.PayrollStatisticsWorkPlaceNumber, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.PayrollStatisticsCFARNumber:
                        return new MatrixField(rowNumber, column.Key, employee.PayrollStatisticsCFARNumber, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.WorkPlaceSCB:
                        return new MatrixField(rowNumber, column.Key, employee.WorkPlaceSCB, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.PartnerInCloseCompany:
                        return new MatrixField(rowNumber, column.Key, employee.PartnerInCloseCompany, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.BenefitAsPension:
                        return new MatrixField(rowNumber, column.Key, employee.BenefitAsPension, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.AFACategory:
                        return new MatrixField(rowNumber, column.Key, employee.AFACategory, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.AFASpecialAgreement:
                        return new MatrixField(rowNumber, column.Key, employee.AFASpecialAgreement, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.AFAWorkplaceNr:
                        return new MatrixField(rowNumber, column.Key, employee.AFAWorkplaceNr, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.AFAParttimePensionCode:
                        return new MatrixField(rowNumber, column.Key, employee.AFAParttimePensionCode, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.CollectumITPPlan:
                        return new MatrixField(rowNumber, column.Key, employee.CollectumITPPlan, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.CollectumAgreedOnProduct:
                        return new MatrixField(rowNumber, column.Key, employee.CollectumAgreedOnProduct, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.CollectumCostPlace:
                        return new MatrixField(rowNumber, column.Key, employee.CollectumCostPlace, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.CollectumCancellationDate:
                        return new MatrixField(rowNumber, column.Key, employee.CollectumCancellationDate, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.CollectumCancellationDateIsLeaveOfAbsence:
                        return new MatrixField(rowNumber, column.Key, employee.CollectumCancellationDateIsLeaveOfAbsence, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.KPARetirementAge:
                        return new MatrixField(rowNumber, column.Key, employee.KPARetirementAge, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.KPABelonging:
                        return new MatrixField(rowNumber, column.Key, employee.KPABelonging, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.KPAEndCode:
                        return new MatrixField(rowNumber, column.Key, employee.KPAEndCode, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.KPAAgreementType:
                        return new MatrixField(rowNumber, column.Key, employee.KPAAgreementType, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.BygglosenAgreementArea:
                        return new MatrixField(rowNumber, column.Key, employee.BygglosenAgreementArea, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.BygglosenAllocationNumber:
                        return new MatrixField(rowNumber, column.Key, employee.BygglosenAllocationNumber, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.BygglosenMunicipalCode:
                        return new MatrixField(rowNumber, column.Key, employee.BygglosenMunicipalCode, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.BygglosenSalaryFormula:
                        return new MatrixField(rowNumber, column.Key, employee.BygglosenSalaryFormula, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.BygglosenProfessionCategory:
                        return new MatrixField(rowNumber, column.Key, employee.BygglosenProfessionCategory, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.BygglosenSalaryType:
                        return new MatrixField(rowNumber, column.Key, employee.BygglosenSalaryType, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.BygglosenWorkPlaceNumber:
                        return new MatrixField(rowNumber, column.Key, employee.BygglosenWorkPlaceNumber, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.BygglosenLendedToOrgNr:
                        return new MatrixField(rowNumber, column.Key, employee.BygglosenLendedToOrgNr, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.BygglosenAgreedHourlyPayLevel:
                        return new MatrixField(rowNumber, column.Key, employee.BygglosenAgreedHourlyPayLevel, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.GTPAgreementNumber:
                        return new MatrixField(rowNumber, column.Key, employee.GTPAgreementNumber, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.GTPExcluded:
                        return new MatrixField(rowNumber, column.Key, employee.GTPExcluded, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.AGIPlaceOfEmploymentAddress:
                        return new MatrixField(rowNumber, column.Key, employee.AGIPlaceOfEmploymentAddress, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.AGIPlaceOfEmploymentCity:
                        return new MatrixField(rowNumber, column.Key, employee.AGIPlaceOfEmploymentCity, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.AGIPlaceOfEmploymentIgnore:
                        return new MatrixField(rowNumber, column.Key, employee.AGIPlaceOfEmploymentIgnore, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.Email:
                        return new MatrixField(rowNumber, column.Key, employee.Email, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.CellPhone:
                        return new MatrixField(rowNumber, column.Key, employee.CellPhone, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.HomePhone:
                        return new MatrixField(rowNumber, column.Key, employee.HomePhone, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.ClosestRelative:
                        return new MatrixField(rowNumber, column.Key, employee.ClosestRelative, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.DistributionAddress:
                        return new MatrixField(rowNumber, column.Key, employee.DistributionAddress, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.ExcludeFromPayroll:
                        return new MatrixField(rowNumber, column.Key, employee.ExcludeFromPayroll, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.Vacant:
                        return new MatrixField(rowNumber, column.Key, employee.Vacant, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.Created:
                        return new MatrixField(rowNumber, column.Key, employee.Created, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.CreatedBy:
                        return new MatrixField(rowNumber, column.Key, employee.CreatedBy, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.Modified:
                        return new MatrixField(rowNumber, column.Key, employee.Modified, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.ModifiedBy:
                        return new MatrixField(rowNumber, column.Key, employee.ModifiedBy, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.CategoryName:
                        return new MatrixField(rowNumber, column.Key, employee.CategoryName, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.DistributionAddressRow:
                        return new MatrixField(rowNumber, column.Key, employee.AddressRow, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.DistributionAddressRow2:
                        return new MatrixField(rowNumber, column.Key, employee.AddressRow2, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.DistributionZipCode:
                        return new MatrixField(rowNumber, column.Key, employee.ZipCode, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.DistributionCity:
                        return new MatrixField(rowNumber, column.Key, employee.City, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.AccountInternalNames:
                        return new MatrixField(rowNumber, column.Key, employee.AccountAnalysisFields.GetAccountAnalysisFieldValueName(column), column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.AccountInternalNrs:
                        return new MatrixField(rowNumber, column.Key, employee.AccountAnalysisFields.GetAccountAnalysisFieldValueNumber(column), column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.ExtraFieldEmployee:
                        return new MatrixField(rowNumber, column.Key, employee.ExtraFieldAnalysisFields.ExtraFieldAnalysisFieldValue(column), column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.NearestExecutiveName:
                        return new MatrixField(rowNumber, column.Key, employee.NearestExecutiveName, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.NearestExecutiveEmail:
                        return new MatrixField(rowNumber, column.Key, employee.NearestExecutiveEmail, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.NearestExecutiveUserName:
                        return new MatrixField(rowNumber, column.Key, employee.NearestExecutiveUserName, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.NearestExecutiveSocialSec:
                        return new MatrixField(rowNumber, column.Key, employee.NearestExecutiveSocialSec, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.NearestExecutiveCellphone:
                        return new MatrixField(rowNumber, column.Key, employee.NearestExecutiveCellPhone, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.HourlySalary:
                        return new MatrixField(rowNumber, column.Key, employee.HourlySalary, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.Salary:
                        return new MatrixField(rowNumber, column.Key, employee.Salary, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.MonthlySalary:
                        return new MatrixField(rowNumber, column.Key, employee.MonthlySalary, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.VacationDaysPaidByLaw:
                        return new MatrixField(rowNumber, column.Key, employee.VacationDaysPaidByLaw, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.IFAssociationNumber:
                        return new MatrixField(rowNumber, column.Key, employee.IFAssociationNumber, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.IFPaymentCode:
                        return new MatrixField(rowNumber, column.Key, employee.IFPaymentCode, column.MatrixDataType);
                    case TermGroup_EmployeeListMatrixColumns.IFWorkPlace:
                        return new MatrixField(rowNumber, column.Key, employee.IFWorkPlace, column.MatrixDataType);
                    default:
                        break;
                }
            }

            return new MatrixField(rowNumber, column.Key, "");
        }
    }
}
