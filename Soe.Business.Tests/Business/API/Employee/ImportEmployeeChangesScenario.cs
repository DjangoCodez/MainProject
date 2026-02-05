using Soe.Business.Tests.Business.Mock;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util.IO;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Shared.DTO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Soe.Business.Tests.Business.API.Employee
{
    public class ImportEmployeeChangesScenario
    {
        #region Properties

        private readonly ParameterObject parameterObject;
        private EmployeeChangeIOItem container;
        private List<AccountDTO> accounts;
        private List<AccountDimDTO> accountDims;
        private List<AttestRoleDTO> attestRoles;
        private List<EmployeeUserDTO> employees;
        private List<PositionDTO> employeePositions;
        private List<EmploymentTypeDTO> employmentTypes;
        private List<EndReasonDTO> employmentEndReasons;
        private List<EmployeeGroupDTO> employeeGroups;
        private List<PayrollGroupDTO> payrollGroups;
        private List<PayrollPriceTypeDTO> payrollPriceTypes;
        private List<PayrollLevelDTO> payrollLevels;
        private List<PayrollPriceFormulaDTO> payrollPriceFormulas;
        private List<RoleDTO> userRoles;
        private List<TimeDeviationCauseDTO> timeDeviationCauses;
        private List<VacationGroupDTO> vacationsGroups;
        private List<AnnualLeaveGroupDTO> annualLeaveGroups;

        #endregion

        #region Ctor

        private ImportEmployeeChangesScenario(ParameterObject parameterObject)
        {
            this.parameterObject = parameterObject;
        }

        #endregion

        #region Setup

        public static ImportEmployeeChangesScenario CreateScenario(ParameterObject parameterObject)
        {
            ImportEmployeeChangesScenario scenario = new ImportEmployeeChangesScenario(parameterObject);
            scenario.CreateContainer();
            return scenario;
        }

        #endregion

        #region Scenarios

        internal EmployeeUserImportBatch ImportChangesToEmployee(List<EmployeeUserDTO> employees, List<TestImportRow> rows)
        {
            CreateEmployeeChanges(rows);
            EmployeeUserImportBatch batch = ImportEmployeeChanges(employees);
            return batch;
        }

        #endregion

        #region Internal methods

        internal EmployeeUserDTO GetEmployee(string employeeNr)
        {
            return this.employees.FirstOrDefault(i => i.EmployeeNr == employeeNr);
        }

        internal List<EmployeeUserDTO> GetEmployees(List<string> employeeNrs)
        {
            return this.employees.Where(i => employeeNrs.Contains(i.EmployeeNr)).ToList();
        }

        internal TestImportRow GenerateChange(ImportEmployeeChangesTestsInput input, EmployeeUserDTO employee)
        {
            if (input == null || employee == null)
                return null;

            EmploymentDTO employment = input.Parameters.UseFirstEmployment ? employee.Employments?.FirstOrDefault() : null;
            DateTime? fromDate = GenerateDateFrom(employee, employment, input);
            DateTime? toDate = GenerateDateTo(employee, employment, input);
            string value = GenerateValue(employee, employment, input, fromDate, toDate);
            string optionalExternalCode = GenerateOptionalExternalCode(employee, employment, input, fromDate);
            DateTime? optionalEmploymentDate = GenerateOptionalEmploymentDate(employment, input);
            return GenerateRow(employee, input.Type, value, fromDate, toDate, optionalExternalCode: optionalExternalCode, optionalEmploymentDate: optionalEmploymentDate, delete: input.Parameters.Delete);
        }

        internal TestImportRow GenerateRow(EmployeeUserDTO employee, EmployeeChangeType type, string value, DateTime? fromDate = null, DateTime? toDate = null, DateTime? optionalEmploymentDate = null, string optionalExternalCode = null, bool delete = false)
        {
            return new TestImportRow(employee, type, value, fromDate, toDate, optionalEmploymentDate, optionalExternalCode, delete);
        }

        #endregion

        #region Help-methods

        private void CreateContainer()
        {
            Mock();
            this.container = new EmployeeChangeIOItem(null, this.GetApiLookup(), this.GetApiPermissions());
        }

        private void Mock()
        {
            this.accounts = AccountMock.Mock();
            this.accountDims = AccountDimMock.Mock();
            this.attestRoles = AttestRoleMock.Mock();
            this.employees = EmployeeUserMock.Mock();
            this.employeePositions = PositionsMock.Mock();
            this.employeeGroups = EmployeeGroupMock.Mock();
            this.employmentEndReasons = EmploymentEndReasonMock.Mock();
            this.employmentTypes = EmploymentTypeMock.Mock();
            this.payrollGroups = PayrollGroupMock.Mock();
            this.payrollPriceTypes = PayrollPriceTypeMock.Mock();
            this.payrollLevels = PayrollLevelMock.Mock();
            this.payrollPriceFormulas = PayrollPriceFormulaMock.Mock();
            this.userRoles = RoleMock.Mock();
            this.timeDeviationCauses = TimeDeviationCauseMock.Mock();
            this.vacationsGroups = VacationGroupMock.Mock();
        }

        private ApiLookupEmployee GetApiLookup()
        {
            ApiConfig config = new ApiConfig(0, this.GetApiSettings());
            ApiLookupEmployee lookup = new ApiLookupEmployee(
                config,
                actorCompanyId: 0,
                defaultRoleId: 1,
                defaultEmployeeGroupId: 1,
                defaultPayrollGroupId: 0,
                defaultVacationGroupId: 0,
                companyCountryId: TermGroup_Country.SE,
                employeeGroups: this.employeeGroups,
                payrollgroups: this.payrollGroups,
                vacationsGroups: this.vacationsGroups,
                annualLeaveGroups: this.annualLeaveGroups
                );
            lookup.SetOptionalLookups(
                terms: null,
                accountDims: this.accountDims,
                accountInternals: this.accounts,
                attestRoles: this.attestRoles,
                contactAddresses: null,
                contactEcoms: null,
                employeePositions: this.employeePositions,
                employmentEndReasons: this.employmentEndReasons,
                employmentTypes: this.employmentTypes,
                extraFields: null,
                extraFieldRecords: null,
                payrollPriceTypes: this.payrollPriceTypes,
                payrollLevels: this.payrollLevels,
                payrollPriceFormulas: this.payrollPriceFormulas,
                userRoles: this.userRoles,
                timeDeviationCauses: this.timeDeviationCauses,
                timeWorkAccounts: null);
            return lookup;
        }

        private List<ApiSettingDTO> GetApiSettings()
        {
            return new List<ApiSettingDTO>();
        }

        private Dictionary<EmployeeChangeType, bool> GetApiPermissions()
        {
            Dictionary<EmployeeChangeType, bool> permissions = new Dictionary<EmployeeChangeType, bool>();

            foreach (EmployeeChangeType type in Enum.GetValues(typeof(EmployeeChangeType)))
            {
                permissions.Add(type, true);
            }

            return permissions;
        }

        private string GenerateValue(EmployeeUserDTO employee, EmploymentDTO employment, ImportEmployeeChangesTestsInput input, DateTime? fromDate, DateTime? toDate)
        {
            if (input == null)
                return null;

            if (employee != null)
            {
                switch (input.Type)
                {
                    case EmployeeChangeType.FirstName:
                        return GenerateFirstName();
                    case EmployeeChangeType.LastName:
                        return GenerateLastName();
                    case EmployeeChangeType.SocialSec:
                        return GenerateSocialSec();
                    case EmployeeChangeType.ExternalCode:
                        return GenerateExternalCode();
                    case EmployeeChangeType.Email:
                        return GenerateEmail();
                    case EmployeeChangeType.ExcludeFromPayroll:
                        return GenerateExcludeFromPayroll();
                    case EmployeeChangeType.WantsExtraShifts:
                        return GenerateWantsExtraShifts();
                    case EmployeeChangeType.EmploymentStartDateChange:
                        return fromDate?.ToShortDateString() ?? string.Empty;
                    case EmployeeChangeType.EmploymentStopDateChange:
                        return toDate?.ToShortDateString() ?? string.Empty;
                    case EmployeeChangeType.EmployeeGroup:
                        return GenerateEmployeeGroup();
                    case EmployeeChangeType.PayrollGroup:
                        return GeneratePayrollGroup();
                    case EmployeeChangeType.VacationGroup:
                        return GenerateVacationGroup();
                    case EmployeeChangeType.WorkTimeWeekMinutes:
                        return GenerateWorkTimeWeek();
                    case EmployeeChangeType.EmploymentPercent:
                        return GenerateEmploymentPercent();
                    case EmployeeChangeType.BaseWorkTimeWeek:
                        return GenerateBaseWorkTimeWeek();
                    case EmployeeChangeType.EmploymentExternalCode:
                        return GenerateEmploymentExternalCode();
                    case EmployeeChangeType.TaxRate:
                        return GenerateTaxRate();
                    case EmployeeChangeType.IsSecondaryEmployment:
                        return GenerateEmploymentIsSecondaryEmployment();
                    case EmployeeChangeType.ExperienceMonths:
                        return GenerateEmploymentExperienceMonths();
                    case EmployeeChangeType.ExperienceAgreedOrEstablished:
                        return GenerateEmploymentExperienceAgreedOrEstablished();
                    case EmployeeChangeType.WorkTasks:
                        return GenerateEmploymentWorkTasks();
                    case EmployeeChangeType.SpecialConditions:
                        return GenerateEmploymentSpecialConditions();
                    case EmployeeChangeType.WorkPlace:
                        return GenerateEmploymentWorkPlace();
                    case EmployeeChangeType.SubstituteFor:
                        return GenerateSubstituteFor();
                    case EmployeeChangeType.SubstituteForDueTo:
                        return GenerateSubstituteForDueTo();
                    case EmployeeChangeType.EmploymentEndReason:
                        return GenerateEndReason();
                    case EmployeeChangeType.HierarchicalAccount:
                        return GenerateHierchalAccount();
                    case EmployeeChangeType.EmploymentPriceType:
                        return GenerateEmploymentPriceType();
                    case EmployeeChangeType.UserRole:
                        return GenerateUserRole();
                    case EmployeeChangeType.AttestRole:
                        return GenerateAttestRole();
                    case EmployeeChangeType.BlockedFromDate:
                        return GenerateBlockedFromDate();
                    case EmployeeChangeType.ExternalAuthId:
                        return GenerateExternalAuthId();
                    case EmployeeChangeType.PayrollStatisticsPersonalCategory:
                        return GenerateEmployeeIntValueReport(employee.PayrollReportsPersonalCategory, (int)TermGroup_PayrollExportPersonalCategory.LEDARNA);
                    case EmployeeChangeType.PayrollStatisticsWorkTimeCategory:
                        return GenerateEmployeeIntValueReport(employee.PayrollReportsWorkTimeCategory, (int)TermGroup_PayrollExportWorkTimeCategory.PartTimeRetired);
                    case EmployeeChangeType.PayrollStatisticsSalaryType:
                        return GenerateEmployeeIntValueReport(employee.PayrollReportsSalaryType, (int)TermGroup_PayrollExportSalaryType.Hourly);
                    case EmployeeChangeType.PayrollStatisticsWorkPlaceNumber:
                        return GenerateEmployeeIntValueReport(employee.PayrollReportsWorkPlaceNumber, int.MaxValue);
                    case EmployeeChangeType.PayrollStatisticsCFARNumber:
                        return GenerateEmployeeIntValueReport(employee.PayrollReportsCFARNumber, int.MaxValue);
                    case EmployeeChangeType.ControlTaskWorkPlaceSCB:
                        return GenerateEmployeeReportStringValue(employee.WorkPlaceSCB, Guid.NewGuid().ToString(), 100);
                    case EmployeeChangeType.ControlTaskPartnerInCloseCompany:
                        return GenerateEmployeeReportBoolValue(employee.PartnerInCloseCompany, true);
                    case EmployeeChangeType.ControlTaskBenefitAsPension:
                        return GenerateEmployeeReportBoolValue(employee.BenefitAsPension, true);
                    case EmployeeChangeType.AFACategory:
                        return GenerateEmployeeIntValueReport(employee.AFACategory, (int)TermGroup_AfaCategory.Undantas);
                    case EmployeeChangeType.AFASpecialAgreement:
                        return GenerateEmployeeIntValueReport(employee.AFASpecialAgreement, (int)TermGroup_AfaSpecialAgreement.EgetAvtalTjansteman);
                    case EmployeeChangeType.AFAWorkplaceNr:
                        return GenerateEmployeeReportStringValue(employee.AFAWorkplaceNr, Guid.NewGuid().ToString(), 100);
                    case EmployeeChangeType.AFAParttimePensionCode:
                        return GenerateEmployeeReportBoolValue(employee.AFAParttimePensionCode, true);
                    case EmployeeChangeType.CollectumITPPlan:
                        return GenerateEmployeeIntValueReport(employee.CollectumITPPlan, (int)TermGroup_PayrollReportsCollectumITPplan.ITP2);
                    case EmployeeChangeType.CollectumAgreedOnProduct:
                        return GenerateEmployeeReportStringValue(employee.CollectumAgreedOnProduct, Guid.NewGuid().ToString(), 100);
                    case EmployeeChangeType.CollectumCostPlace:
                        return GenerateEmployeeReportStringValue(employee.CollectumCostPlace, Guid.NewGuid().ToString(), 100);
                    case EmployeeChangeType.CollectumCancellationDate:
                        return GenerateEmployeeReportDateValue(employee.CollectumCancellationDate, DateTime.Now.AddYears(1));
                    case EmployeeChangeType.CollectumCancellationDateIsLeaveOfAbsence:
                        return GenerateEmployeeReportBoolValue(employee.CollectumCancellationDateIsLeaveOfAbsence, true);
                    case EmployeeChangeType.KPARetirementAge:
                        return GenerateEmployeeIntValueReport(employee.KpaRetirementAge, int.MaxValue);
                    case EmployeeChangeType.KPABelonging:
                        return GenerateEmployeeIntValueReport(employee.KpaBelonging, (int)KpaBelonging.BEA);
                    case EmployeeChangeType.KPAEndCode:
                        return GenerateEmployeeIntValueReport(employee.KpaEndCode, (int)KpaEndCode.UD);
                    case EmployeeChangeType.KPAAgreementType:
                        return GenerateEmployeeIntValueReport(employee.KpaAgreementType, (int)KpaAgreementType.AKAP_KL);
                    case EmployeeChangeType.BygglosenAgreementArea:
                        return GenerateEmployeeReportStringValue(employee.BygglosenAgreementArea, Guid.NewGuid().ToString(), 10);
                    case EmployeeChangeType.BygglosenAllocationNumber:
                        return GenerateEmployeeReportStringValue(employee.BygglosenAllocationNumber, Guid.NewGuid().ToString(), 10);
                    case EmployeeChangeType.BygglosenSalaryFormula:
                        return GeneratePayrollPriceFormula();
                    case EmployeeChangeType.BygglosenMunicipalCode:
                        return GenerateEmployeeReportStringValue(employee.BygglosenMunicipalCode, Guid.NewGuid().ToString(), 10);
                    case EmployeeChangeType.BygglosenProfessionCategory:
                        return GenerateEmployeeReportStringValue(employee.BygglosenProfessionCategory, Guid.NewGuid().ToString(), 10);
                    case EmployeeChangeType.BygglosenSalaryType:
                        return GenerateEmployeeIntValueReport(employee.BygglosenSalaryType, (int)TermGroup_BygglosenSalaryType.TimeSalary);
                    case EmployeeChangeType.BygglosenWorkPlaceNumber:
                        return GenerateEmployeeReportStringValue(employee.BygglosenWorkPlaceNumber, Guid.NewGuid().ToString(), 100);
                    case EmployeeChangeType.BygglosenLendedToOrgNr:
                        return GenerateEmployeeReportStringValue(employee.BygglosenLendedToOrgNr, Guid.NewGuid().ToString(), 10);
                    case EmployeeChangeType.BygglosenAgreedHourlyPayLevel:
                        return GenerateEmployeeDecimalValueReport(employee.BygglosenAgreedHourlyPayLevel, decimal.MaxValue);
                    case EmployeeChangeType.GTPAgreementNumber:
                        return GenerateEmployeeIntValueReport(employee.GtpAgreementNumber, (int)TermGroup_GTPAgreementNumber.Fremia_HRF);
                    case EmployeeChangeType.GTPExcluded:
                        return GenerateEmployeeReportBoolValue(employee.GtpExcluded, true);
                    case EmployeeChangeType.AGIPlaceOfEmploymentCity:
                        return GenerateEmployeeReportStringValue(employee.AGIPlaceOfEmploymentCity, Guid.NewGuid().ToString(), 100);
                    case EmployeeChangeType.AGIPlaceOfEmploymentAddress:
                        return GenerateEmployeeReportStringValue(employee.AGIPlaceOfEmploymentAddress, Guid.NewGuid().ToString(), 100);
                    case EmployeeChangeType.AGIPlaceOfEmploymentIgnore:
                        return GenerateEmployeeReportBoolValue(employee.AGIPlaceOfEmploymentIgnore, true);
                    case EmployeeChangeType.IFAssociationNumber:
                        return GenerateEmployeeIntValueReport(employee.IFAssociationNumber, int.MaxValue);
                    case EmployeeChangeType.IFPaymentCode:
                        return GenerateEmployeeIntValueReport(employee.IFPaymentCode, (int)TermGroup_IFPaymentCode.PayedFee);
                    case EmployeeChangeType.IFWorkPlace:
                        return GenerateEmployeeReportStringValue(employee.IFWorkPlace, Guid.NewGuid().ToString(), 10);
                    case EmployeeChangeType.VacationDaysPaid:
                        return GenerateVacactionDay();
                    case EmployeeChangeType.VacationDaysUnPaid:
                        return GenerateVacactionDay();
                    case EmployeeChangeType.VacationDaysAdvance:
                        return GenerateVacactionDay();
                }
            }
            return string.Empty;

            string GenerateFirstName()
            {
                if (!input.Parameters.NewValue)
                    return employee.FirstName;

                return !string.IsNullOrEmpty(employee.FirstName) ? $"{employee.FirstName}Test" : "Test";
            }
            string GenerateLastName()
            {
                if (!input.Parameters.NewValue)
                    return employee.LastName;

                return !string.IsNullOrEmpty(employee.LastName) ? $"{employee.LastName}Test" : "Test";
            }
            string GenerateSocialSec()
            {
                if (!input.Parameters.NewValue)
                    return employee.SocialSec;

                List<string> socialSecs = SsnMock.Mock(employee.Sex);
                return socialSecs.FirstOrDefault(ssn => ssn != employee.SocialSec);
            }
            string GenerateExternalCode()
            {
                if (!input.Parameters.NewValue)
                    return employee.ExternalCode.NullToEmpty();

                List<string> externalCodes = ExternalCodesMock.Mock();
                return externalCodes.FirstOrDefault(id => id != employee?.ExternalCode);
            }
            string GenerateEmail()
            {
                if (!input.Parameters.NewValue)
                    return employee.Email;

                string newEmail = "Test@Testsson.se";
                if (!string.IsNullOrEmpty(employee.Email))
                {
                    int indexOfAt = employee.Email.IndexOf('@');
                    int indexOfDot = employee.Email.LastIndexOf('.');
                    if (indexOfAt > 0 && indexOfDot > 0 && indexOfAt < indexOfDot)
                    {
                        string name = employee.Email.Substring(0, indexOfAt);
                        string domain = employee.Email.Substring(indexOfAt + 1);
                        string[] domainParts = domain.Split('.');
                        if (domainParts.Length == 2)
                            newEmail = $"{name}@{domainParts[0]}.{(domainParts[1].Equals("se") ? "com" : "se")}";
                    }
                }
                return newEmail;
            }
            string GenerateExcludeFromPayroll()
            {
                if (input.Parameters.ForceInvalid)
                    return "abc";

                if (input.Parameters.NewValue)
                    return employee.ExcludeFromPayroll ? "0" : "1";
                else
                    return StringUtility.GetString(employee.ExcludeFromPayroll);
            }
            string GenerateWantsExtraShifts()
            {
                if (input.Parameters.ForceInvalid)
                    return "abc";

                if (input.Parameters.NewValue)
                    return employee.WantsExtraShifts ? "0" : "1";
                else
                    return StringUtility.GetString(employee.WantsExtraShifts);
            }
            string GenerateEmployeeGroup()
            {
                if (!input.Parameters.NewValue)
                    return (employment?.EmployeeGroupName).NullToEmpty();

                EmployeeGroupDTO employeeGroup = this.employeeGroups.FirstOrDefault(i => i.EmployeeGroupId != employment?.EmployeeGroupId);
                return (employeeGroup?.ExternalCodes?.FirstOrDefault() ?? employeeGroup?.Name).NullToEmpty();
            }
            string GeneratePayrollGroup()
            {
                if (!input.Parameters.NewValue)
                    return (employment?.PayrollGroupName).NullToEmpty();

                PayrollGroupDTO payrollGroup = this.payrollGroups.FirstOrDefault(i => i.PayrollGroupId != employment?.PayrollGroupId);
                return (payrollGroup?.ExternalCodes?.FirstOrDefault() ?? payrollGroup?.Name).NullToEmpty();
            }
            string GenerateVacationGroup()
            {
                if (!input.Parameters.NewValue)
                    return (employment?.EmploymentVacationGroup?.FirstOrDefault()?.Name).NullToEmpty();

                int vacationGroupId = employment.EmploymentVacationGroup?.FirstOrDefault()?.VacationGroupId ?? 1;
                VacationGroupDTO vacationGroup = this.vacationsGroups.FirstOrDefault(i => i.VacationGroupId != vacationGroupId);
                return (vacationGroup?.ExternalCodes?.FirstOrDefault() ?? vacationGroup?.Name).NullToEmpty();
            }
            string GenerateWorkTimeWeek()
            {
                if (!input.Parameters.NewValue)
                    return (employment?.WorkTimeWeek.ToString()).NullToEmpty();

                decimal workTimeWeek = 0;
                if (employment != null)
                    workTimeWeek = employment.WorkTimeWeek > 0 ? Decimal.Multiply(employment.WorkTimeWeek, new decimal(1.1)) : Decimal.Multiply(40, 60);

                return workTimeWeek.ToString();
            }
            string GenerateEmploymentPercent()
            {
                if (!input.Parameters.NewValue)
                    return (employment?.Percent.ToString()).NullToEmpty();

                decimal percent = 0;
                if (employment != null)
                    percent = employment.Percent > 0 ? Decimal.Multiply(employment.Percent, new decimal(1.1)) : Decimal.Multiply(40, 60);

                return percent.ToString();
            }
            string GenerateBaseWorkTimeWeek()
            {
                if (!input.Parameters.NewValue)
                    return (employment?.WorkTimeWeek.ToString()).NullToEmpty();

                decimal baseWorkTimeWeek = 0;
                if (employment != null)
                    baseWorkTimeWeek = employment.BaseWorkTimeWeek > 0 ? Decimal.Multiply(employment.BaseWorkTimeWeek, new decimal(1.1)) : Decimal.Multiply(40, 60);

                return baseWorkTimeWeek.ToString();
            }
            string GenerateEmploymentExternalCode()
            {
                if (!input.Parameters.NewValue)
                    return (employment?.ExternalCode?.ToString()).NullToEmpty();

                List<string> externalCodes = ExternalCodesMock.Mock();
                return externalCodes.FirstOrDefault(id => id != employment?.ExternalCode);
            }
            string GenerateEmploymentIsSecondaryEmployment()
            {
                if (!input.Parameters.NewValue)
                    return (employment?.IsSecondaryEmployment.ToString()).NullToEmpty();

                return "1";
            }
            string GenerateEmploymentWorkTasks()
            {
                if (!input.Parameters.NewValue)
                    return (employment?.WorkTasks?.ToString()).NullToEmpty();

                return "Do stuff";
            }
            string GenerateEmploymentWorkPlace()
            {
                if (!input.Parameters.NewValue)
                    return (employment?.WorkPlace?.ToString()).NullToEmpty();

                return "Do stuff";
            }
            string GenerateEmploymentSpecialConditions()
            {
                if (!input.Parameters.NewValue)
                    return (employment?.SpecialConditions?.ToString()).NullToEmpty();

                return "Do stuff";
            }
            string GenerateEmploymentExperienceMonths()
            {
                if (!input.Parameters.NewValue)
                    return (employment?.ExperienceMonths.ToString()).NullToEmpty();

                return "12";
            }
            string GenerateVacactionDay()
            {
                if (!input.Parameters.NewValue && employee.EmployeeVacationSE != null)
                    return employee.EmployeeVacationSE.NullToEmpty();

                return "25";
            }
            string GenerateTaxRate()
            {
                if (!input.Parameters.NewValue)
                    return employee.TempTaxRate.NullToEmpty();

                return "32";
            }
            string GenerateEmploymentExperienceAgreedOrEstablished()
            {
                if (!input.Parameters.NewValue)
                    return (employment?.ExperienceAgreedOrEstablished.ToString()).NullToEmpty();

                return "1";
            }
            string GenerateSubstituteFor()
            {
                if (!input.Parameters.NewValue)
                    return (employment?.SubstituteFor?.ToString()).NullToEmpty();

                return "Anna";
            }
            string GenerateSubstituteForDueTo()
            {
                if (!input.Parameters.NewValue)
                    return (employment?.SubstituteForDueTo?.ToString()).NullToEmpty();

                return "2020-01-01";
            }
            string GenerateEndReason()
            {
                if (!input.Parameters.NewValue)
                    return (employment?.EmploymentEndReason.ToString()).NullToEmpty();

                return ((int)TermGroup_EmploymentEndReason.SE_Fired).ToString();
            }
            string GenerateHierchalAccount()
            {
                EmployeeAccountDTO employeeAccount = employee.Accounts?.FirstOrDefault();
                if (employeeAccount == null)
                    return string.Empty;

                AccountDTO account;
                if (input.Parameters.NewValue)
                    account = this.accounts?.FirstOrDefault(i => i.AccountId != employeeAccount.AccountId);
                else
                    account = this.accounts?.FirstOrDefault(i => i.AccountId == employeeAccount.AccountId);

                return (account?.ExternalCode ?? account?.AccountNr).NullToEmpty();
            }
            string GenerateEmploymentPriceType()
            {
                EmploymentPriceTypeDTO employmentPrice = employment?.PriceTypes?.FirstOrDefault();
                if (employmentPrice == null)
                    return string.Empty;

                decimal? amount = employmentPrice.GetAmount(fromDate);
                if (input.Parameters.NewValue)
                    amount = amount.HasValue ? Decimal.Multiply(amount.Value, new decimal(1.1)) : 1000;

                return (amount?.ToString()).NullToEmpty();
            }
            string GenerateUserRole()
            {
                UserCompanyRoleDTO userRole = employee.UserRoles?.FirstOrDefault()?.Roles?.FirstOrDefault();
                if (userRole == null)
                    return string.Empty;

                RoleDTO role;
                if (input.Parameters.NewValue)
                    role = this.userRoles?.FirstOrDefault(i => i.RoleId != userRole.RoleId);
                else
                    role = this.userRoles?.FirstOrDefault(i => i.RoleId == userRole.RoleId);

                return (role?.ActualName).NullToEmpty();
            }
            string GenerateAttestRole()
            {
                UserAttestRoleDTO userAttestRole = employee.UserRoles?.FirstOrDefault()?.AttestRoles?.FirstOrDefault();
                if (userAttestRole == null)
                    return string.Empty;

                AttestRoleDTO attestRole;
                if (input.Parameters.NewValue)
                    attestRole = this.attestRoles?.FirstOrDefault(i => i.AttestRoleId != userAttestRole.AttestRoleId);
                else
                    attestRole = this.attestRoles?.FirstOrDefault(i => i.AttestRoleId == userAttestRole.AttestRoleId);

                return (attestRole?.Name).NullToEmpty();
            }
            string GenerateBlockedFromDate()
            {
                if (!input.Parameters.NewValue)
                    return employee.BlockedFromDate.HasValue ? employee.BlockedFromDate.Value.ToShortDateString() : string.Empty;

                return (employee.BlockedFromDate.HasValue ? employee.BlockedFromDate.Value.AddDays(1) : CalendarUtility.GetEndOfYear(employment.DateTo ?? employment.DateFrom.Value)).Date.ToShortDateString();
            }
            string GenerateExternalAuthId()
            {
                if (!input.Parameters.NewValue)
                    return employee.ExternalAuthId.NullToEmpty();

                List<string> externalCodes = ExternalCodesMock.Mock();
                return externalCodes.FirstOrDefault(id => id != employee.ExternalAuthId);
            }
            string GenerateEmployeeIntValueReport(int? value, int newValue)
            {
                if (input.Parameters.Delete)
                    return null;
                else if (!input.Parameters.NewValue)
                    return value.NullToEmpty();
                else if (input.Parameters.ForceInvalid)
                    return "abc";

                return newValue.ToString();
            }
            string GenerateEmployeeDecimalValueReport(decimal? value, decimal newValue)
            {
                if (input.Parameters.Delete)
                    return null;
                else if (!input.Parameters.NewValue)
                    return value.HasValue ? value.Value.ToString() : string.Empty;
                else if (input.Parameters.ForceInvalid)
                    return "abc";

                return newValue.ToString();
            }
            string GenerateEmployeeReportStringValue(string value, string newValue, int? maxLength = null)
            {
                if (maxLength.HasValue)
                    newValue = newValue?.Left(maxLength.Value);
                if (input.Parameters.Delete)
                    return null;
                else if (!input.Parameters.NewValue)
                    return value;
                else if (input.Parameters.ForceInvalid)
                    return "";

                return newValue;
            }
            string GenerateEmployeeReportDateValue(DateTime? value, DateTime? newValue)
            {
                if (input.Parameters.Delete)
                    return string.Empty;
                else if (!input.Parameters.NewValue)
                    return value?.ToShortDateString() ?? CalendarUtility.DATETIME_DEFAULT.ToShortDateString();
                else if (input.Parameters.ForceInvalid)
                    return "abc";

                return newValue?.ToShortDateString() ?? CalendarUtility.DATETIME_DEFAULT.ToShortDateString();
            }
            string GenerateEmployeeReportBoolValue(bool? value, bool? newValue)
            {
                if (input.Parameters.Delete)
                    return "0";
                else if (!input.Parameters.NewValue)
                    return value.HasValue ? StringUtility.GetString(value.Value) : "0";
                else if (input.Parameters.ForceInvalid)
                    return "abc";

                return newValue.HasValue ? StringUtility.GetString(newValue.Value) : "0";
            }
            string GeneratePayrollPriceFormula()
            {
                if (!input.Parameters.NewValue)
                    return (employee.BygglosenSalaryFormulaName).NullToEmpty();

                PayrollPriceFormulaDTO payrollPriceFormula = this.payrollPriceFormulas.LastOrDefault();
                return (payrollPriceFormula?.Name).NullToEmpty();
            }
        }

        private DateTime? GenerateOptionalEmploymentDate( EmploymentDTO employment, ImportEmployeeChangesTestsInput input)
        {
            if (input != null && employment != null)
            {
                switch (input.Type)
                {
                    case EmployeeChangeType.EmploymentEndReason:
                        return employment.DateTo;
                }
            }
            return null;
        }

        private string GenerateOptionalExternalCode(EmployeeUserDTO employee, EmploymentDTO employment, ImportEmployeeChangesTestsInput input, DateTime? fromDate)
        {
            if (input == null)
                return null;

            if (employee != null)
            {
                switch (input.Type)
                {
                    case EmployeeChangeType.EmploymentPriceType:
                        return GenerateEmploymentPriceType();
                }
            }
            return string.Empty;

            string GenerateEmploymentPriceType()
            {
                if (input.Parameters.ForceInvalid)
                    return Guid.NewGuid().ToString();

                EmploymentPriceTypeDTO employmentPriceType = employment?.PriceTypes?.FirstOrDefault();
                if (employmentPriceType == null)
                    return string.Empty;

                PayrollPriceTypeDTO payrollPriceType;
                if (input.Parameters.NewOptionalExternalCode1)
                    payrollPriceType = this.payrollPriceTypes?.FirstOrDefault(i => i.PayrollPriceTypeId != employmentPriceType.PayrollPriceTypeId);
                else
                    payrollPriceType = this.payrollPriceTypes?.FirstOrDefault(i => i.PayrollPriceTypeId == employmentPriceType.PayrollPriceTypeId);

                int? payrollTypeLevelId = employmentPriceType.GetPayrollLevel(fromDate);
                PayrollLevelDTO payrollLevel;
                if (input.Parameters.NewOptionalExternalCode2)
                    payrollLevel = this.payrollLevels?.FirstOrDefault(i => i.PayrollLevelId != payrollTypeLevelId);
                else
                    payrollLevel = this.payrollLevels?.FirstOrDefault(i => i.PayrollLevelId == payrollTypeLevelId);

                return $"{(payrollPriceType?.Code).NullToEmpty()}{EmployeeChangeIOItem.DELIMETER}{(payrollLevel?.Code).NullToEmpty()}";
            }
        }

        private DateTime? GenerateDateFrom(EmployeeUserDTO employee, EmploymentDTO employment, ImportEmployeeChangesTestsInput input)
        {
            if (input == null)
                return null;

            switch (input.Type)
            {
                case EmployeeChangeType.EmploymentStartDateChange:
                    return GenerateEmploymentStartDate();
                case EmployeeChangeType.EmploymentStopDateChange:
                    return GenerateEmploymentStopDate();
                case EmployeeChangeType.HierarchicalAccount:
                    return GenerateHierchalAccount();
                case EmployeeChangeType.UserRole:
                    return GenerateUserRole();
                case EmployeeChangeType.AttestRole:
                    return GenerateAttestRole();
            }

            return employment?.DateFrom;

            DateTime? GenerateEmploymentStartDate()
            {
                if (!input.Parameters.NewFromDate)
                    return employment.DateFrom;

                if (input.Parameters.CustomFlag)
                {
                    if (employment.DateTo.HasValue)
                        return employment.DateTo.Value.Subtract(employment.DateFrom.Value).TotalDays > 31 ? employment.DateTo.Value.AddMonths(-1) : employment.DateTo.Value.AddDays(-1);
                }
                else
                {
                    if (employment.DateTo.HasValue)
                        return employment.DateTo.Value.AddDays(1);
                }

                return null;
            }
            DateTime? GenerateEmploymentStopDate()
            {
                return employment.DateFrom;
            }
            DateTime GenerateHierchalAccount()
            {
                EmployeeAccountDTO employeeAccount = employee?.Accounts?.FirstOrDefault();
                if (employeeAccount == null)
                    return CalendarUtility.DATETIME_DEFAULT;

                if (input.Parameters.NewFromDate)
                    return employeeAccount.DateFrom.AddDays(-1); //Only valid to update backwards
                else
                    return employeeAccount.DateFrom;
            }
            DateTime? GenerateUserRole()
            {
                UserCompanyRoleDTO userRole = employee?.UserRoles?.FirstOrDefault()?.Roles?.FirstOrDefault();
                if (userRole == null)
                    return CalendarUtility.DATETIME_DEFAULT;

                if (input.Parameters.NewFromDate)
                    return userRole.DateFrom.HasValue ? userRole.DateFrom.Value.AddDays(-1) : CalendarUtility.GetBeginningOfYear(DateTime.Today).Date; //Only valid to update backwards
                else
                    return userRole.DateFrom;
            }
            DateTime? GenerateAttestRole()
            {
                UserAttestRoleDTO attestRole = employee?.UserRoles?.FirstOrDefault()?.AttestRoles?.FirstOrDefault();
                if (attestRole == null)
                    return CalendarUtility.DATETIME_DEFAULT;

                if (input.Parameters.NewFromDate)
                    return attestRole.DateFrom.HasValue ? attestRole.DateFrom.Value.AddDays(-1) : CalendarUtility.GetBeginningOfYear(DateTime.Today).Date; //Only valid to update backwards
                else
                    return attestRole.DateFrom;
            }
        }

        private DateTime? GenerateDateTo(EmployeeUserDTO employee, EmploymentDTO employment, ImportEmployeeChangesTestsInput input)
        {
            if (input == null)
                return null;

            switch (input.Type)
            {
                case EmployeeChangeType.EmploymentStartDateChange:
                    return GenerateEmploymentStartDate();
                case EmployeeChangeType.EmploymentStopDateChange:
                    return GenerateEmploymentStopDate();
                case EmployeeChangeType.HierarchicalAccount:
                    return GenerateHierchalAccount();
                case EmployeeChangeType.UserRole:
                    return GenerateUserRole();
                case EmployeeChangeType.AttestRole:
                    return GenerateAttestRole();
            }

            return employment?.DateTo;

            DateTime? GenerateEmploymentStartDate()
            {
                return employment.DateTo;
            }
            DateTime? GenerateEmploymentStopDate()
            {
                if (!input.Parameters.NewToDate)
                    return employment.DateTo;

                if (employment.DateTo.HasValue)
                    return employment.DateTo.Value.AddMonths(1);
                else
                    return CalendarUtility.GetEndOfYear(employment.DateFrom).Date;
            }
            DateTime? GenerateHierchalAccount()
            {
                EmployeeAccountDTO employeeAccount = employee?.Accounts?.FirstOrDefault();
                if (employeeAccount == null)
                    return null;

                if (input.Parameters.NewToDate)
                    return employeeAccount.DateTo.HasValue ? employeeAccount.DateTo.Value.AddDays(1) : CalendarUtility.GetEndOfYear(DateTime.Today).Date;
                else
                    return employeeAccount.DateTo;
            }
            DateTime? GenerateUserRole()
            {
                UserCompanyRoleDTO userRole = employee?.UserRoles?.FirstOrDefault()?.Roles?.FirstOrDefault();
                if (userRole == null)
                    return CalendarUtility.DATETIME_DEFAULT;

                if (input.Parameters.NewToDate)
                    return userRole.DateTo.HasValue ? userRole.DateTo.Value.AddDays(1) : CalendarUtility.GetEndOfYear(DateTime.Today).Date;
                else
                    return userRole.DateTo;
            }
            DateTime? GenerateAttestRole()
            {
                UserAttestRoleDTO attestRole = employee?.UserRoles?.FirstOrDefault()?.AttestRoles?.FirstOrDefault();
                if (attestRole == null)
                    return CalendarUtility.DATETIME_DEFAULT;

                if (input.Parameters.NewToDate)
                    return attestRole.DateTo.HasValue ? attestRole.DateTo.Value.AddDays(1) : CalendarUtility.GetEndOfYear(DateTime.Today).Date;
                else
                    return attestRole.DateTo;
            }
        }

        private void CreateEmployeeChanges(List<TestImportRow> rows)
        {
            if (rows == null)
                rows = new List<TestImportRow>();

            if (this.container.EmployeeChangeIODTOs == null)
                this.container.EmployeeChangeIODTOs = new List<EmployeeChangeIODTO>();

            foreach (var rowsByEmployee in rows.Where(row => row?.Employee != null).GroupBy(row => row.Employee.EmployeeNr))
            {
                CreateEmployeeChange(rowsByEmployee.Key, rowsByEmployee.ToList());
            }
        }

        private void CreateEmployeeChange(string employeeNr, List<TestImportRow> rows)
        {
            EmployeeChangeIODTO employeeChange = new EmployeeChangeIODTO()
            {
                EmployeeNr = employeeNr,
                EmployeeChangeRowIOs = new List<EmployeeChangeRowIODTO>(),
            };

            foreach (TestImportRow row in rows)
            {
                CreateEmployeeChangeRow(employeeChange, row);
            }

            this.container.EmployeeChangeIODTOs.Add(employeeChange);
        }

        private void CreateEmployeeChangeRow(EmployeeChangeIODTO employeeChange, TestImportRow row)
        {
            if (employeeChange == null || row == null)
                return;

            employeeChange.EmployeeChangeRowIOs.Add(new EmployeeChangeRowIODTO()
            {
                EmployeeChangeType = row.Type,
                Value = row.Value,
                FromDate = row.FromDate,
                ToDate = row.ToDate,
                OptionalEmploymentDate = row.OptionalEmploymentDate,
                OptionalExternalCode = row.OptionalExternalCode,
                Delete = row.Delete,
            });
        }

        private EmployeeUserImportBatch ImportEmployeeChanges(List<EmployeeUserDTO> employees)
        {
            ApiManager apim = new ApiManager(this.parameterObject);
            EmployeeUserImportBatch batch = apim.ImportEmployeeChangesFromTest(this.container, employees);
            return batch;
        }

        #endregion
    }

    #region Help-classes

    internal class TestImportRow
    {
        public EmployeeUserDTO Employee { get; set; }
        public EmployeeChangeType Type { get; private set; }
        public string Value { get; private set; }
        public DateTime? FromDate { get; private set; }
        public DateTime? ToDate { get; private set; }
        public DateTime OptionalEmploymentDate { get; private set; }
        public string OptionalExternalCode { get; private set; }
        public bool Delete { get; set; }

        public TestImportRow(EmployeeUserDTO employee, EmployeeChangeType type, string value, DateTime? fromDate = null, DateTime? toDate = null, DateTime? optionalEmploymentDate = null, string optionalExternalCode = null, bool delete = false)
        {
            this.Employee = employee;
            this.Type = type;
            this.Value = value;
            this.FromDate = fromDate;
            this.ToDate = toDate;
            this.OptionalEmploymentDate = optionalEmploymentDate ?? DateTime.MinValue;
            this.OptionalExternalCode = optionalExternalCode.NullToEmpty();
            this.Delete = delete;
        }
    }

    #endregion
}
