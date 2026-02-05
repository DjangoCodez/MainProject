using SoftOne.Soe.Business.Core.Reporting.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time
{
    public class EmploymentDynamicContractReportData : TimeReportDataManager, IReportDataModel
    {
        private readonly EmploymentDynamicContractReportInput _reportDataInput;
        private readonly EmploymentDynamicContractReportOutput _reportDataOutput;

        private bool LoadReportInformation
        {
            get
            {
                return _reportDataInput.EmployeeTemplate.EmployeeTemplateGroups.Any(a => a.EmployeeTemplateGroupRows.Any(aa =>
                    aa.Type == TermGroup_EmployeeTemplateGroupRowType.PayrollStatisticsPersonalCategory ||
                    aa.Type == TermGroup_EmployeeTemplateGroupRowType.PayrollStatisticsWorkTimeCategory ||
                    aa.Type == TermGroup_EmployeeTemplateGroupRowType.PayrollStatisticsSalaryType ||
                    aa.Type == TermGroup_EmployeeTemplateGroupRowType.PayrollStatisticsWorkPlaceNumber ||
                    aa.Type == TermGroup_EmployeeTemplateGroupRowType.PayrollStatisticsCFARNumber ||
                    aa.Type == TermGroup_EmployeeTemplateGroupRowType.ControlTaskWorkPlaceSCB ||
                    aa.Type == TermGroup_EmployeeTemplateGroupRowType.ControlTaskPartnerInCloseCompany ||
                    aa.Type == TermGroup_EmployeeTemplateGroupRowType.ControlTaskBenefitAsPension ||
                    aa.Type == TermGroup_EmployeeTemplateGroupRowType.AFACategory ||
                    aa.Type == TermGroup_EmployeeTemplateGroupRowType.AFASpecialAgreement ||
                    aa.Type == TermGroup_EmployeeTemplateGroupRowType.AFAWorkplaceNr ||
                    aa.Type == TermGroup_EmployeeTemplateGroupRowType.AFAParttimePensionCode ||
                    aa.Type == TermGroup_EmployeeTemplateGroupRowType.CollectumITPPlan ||
                    aa.Type == TermGroup_EmployeeTemplateGroupRowType.CollectumAgreedOnProduct ||
                    aa.Type == TermGroup_EmployeeTemplateGroupRowType.CollectumCostPlace ||
                    aa.Type == TermGroup_EmployeeTemplateGroupRowType.CollectumCancellationDate ||
                    aa.Type == TermGroup_EmployeeTemplateGroupRowType.CollectumCancellationDateIsLeaveOfAbsence ||
                    aa.Type == TermGroup_EmployeeTemplateGroupRowType.KPARetirementAge ||
                    aa.Type == TermGroup_EmployeeTemplateGroupRowType.KPABelonging ||
                    aa.Type == TermGroup_EmployeeTemplateGroupRowType.KPAEndCode ||
                    aa.Type == TermGroup_EmployeeTemplateGroupRowType.KPAAgreementType ||
                    aa.Type == TermGroup_EmployeeTemplateGroupRowType.BygglosenAgreementArea ||
                    aa.Type == TermGroup_EmployeeTemplateGroupRowType.BygglosenAllocationNumber ||
                    aa.Type == TermGroup_EmployeeTemplateGroupRowType.BygglosenMunicipalCode ||
                    aa.Type == TermGroup_EmployeeTemplateGroupRowType.BygglosenSalaryFormula ||
                    aa.Type == TermGroup_EmployeeTemplateGroupRowType.BygglosenProfessionCategory ||
                    aa.Type == TermGroup_EmployeeTemplateGroupRowType.BygglosenSalaryType ||
                    aa.Type == TermGroup_EmployeeTemplateGroupRowType.BygglosenWorkPlaceNumber ||
                    aa.Type == TermGroup_EmployeeTemplateGroupRowType.BygglosenLendedToOrgNr ||
                    aa.Type == TermGroup_EmployeeTemplateGroupRowType.BygglosenAgreedHourlyPayLevel ||
                    aa.Type == TermGroup_EmployeeTemplateGroupRowType.GTPAgreementNumber ||
                    aa.Type == TermGroup_EmployeeTemplateGroupRowType.GTPExcluded ||
                    aa.Type == TermGroup_EmployeeTemplateGroupRowType.AGIPlaceOfEmploymentAddress ||
                    aa.Type == TermGroup_EmployeeTemplateGroupRowType.AGIPlaceOfEmploymentCity ||
                    aa.Type == TermGroup_EmployeeTemplateGroupRowType.AGIPlaceOfEmploymentIgnore));
            }
        }

        public EmploymentDynamicContractReportData(ParameterObject parameterObject, EmploymentDynamicContractReportInput reportDataInput) : base(parameterObject)
        {
            _reportDataInput = reportDataInput;
            _reportDataOutput = new EmploymentDynamicContractReportOutput(reportDataInput);
        }

        public EmploymentDynamicContractReportOutput CreateOutput()
        {
            base.reportResult = _reportDataInput.ReportResult;

            _reportDataOutput.Result = LoadData();
            if (!_reportDataOutput.Result.Success)
                return _reportDataOutput;

            return _reportDataOutput;
        }

        private ActionResult LoadData()
        {
            #region Prereq

            DateTime selectionDate;
            List<DateTime> substituteDates;

            TryGetBoolFromSelection(reportResult, out bool isPrintedFromSchedulePlanning, "isPrintedFromSchedulePlanning");
            if (isPrintedFromSchedulePlanning)
            {
                if (!TryGetDatesFromSelection(reportResult, out substituteDates))
                    return new ActionResult(false);

                selectionDate = substituteDates.OrderByDescending(i => i.Date).FirstOrDefault();
            }
            else
            {
                if (!TryGetDateFromSelection(reportResult, out selectionDate))
                    return new ActionResult(false);

                substituteDates = null;
            }

            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDate, selectionDate, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return new ActionResult(false);

            int? employmentId = 0;
            if (selectionEmployeeIds.Count == 1)
                TryGetIdFromSelection(reportResult, out employmentId, key: "employmentId");

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                #region Prerq

                Dictionary<int, string> reportsPersonalCategoryDict = new Dictionary<int, string>();
                Dictionary<int, string> reportsSalaryTypeDict = new Dictionary<int, string>();
                Dictionary<int, string> reportsAFACategoryDict = new Dictionary<int, string>();
                Dictionary<int, string> reportsPayrollReportsAFASpecialAgreementDict = new Dictionary<int, string>();
                Dictionary<int, string> reportsPayrollReportsCollectumITPplanDict = new Dictionary<int, string>();
                Dictionary<int, string> reportsWorkTimeCategoryDict = new Dictionary<int, string>();
                Dictionary<int, string> reportsKPABelongingDict = new Dictionary<int, string>();
                Dictionary<int, string> reportsKPAEndCodeDict = new Dictionary<int, string>();
                Dictionary<int, string> reportsKPAAgreementTypeDict = new Dictionary<int, string>();
                Dictionary<int, string> reportsBygglosenSalaryFormulaDict = new Dictionary<int, string>();
                Dictionary<int, string> reportsBygglosenSalaryTypeDict = new Dictionary<int, string>();
                Dictionary<int, string> reportsGTPAgreementNumberDict = new Dictionary<int, string>();

                List<EmploymentTypeDTO> employmentTypes = EmployeeManager.GetEmploymentTypes(entities, Company.ActorCompanyId);
                List<Role> userRoles = RoleManager.GetRolesByUser(entities, reportResult.UserId, Company.ActorCompanyId);
                bool showSocialSec = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec, Permission.Readonly, reportResult.RoleId, Company.ActorCompanyId, Company.LicenseId, entities);
                bool calculatedCostPerHourMySelfPermission = FeatureManager.HasAnyRolePermission(Feature.Time_Employee_Employees_Edit_MySelf_Time_CalculatedCostPerHour, Permission.Readonly, Company.ActorCompanyId, Company.LicenseId, userRoles, entities);
                bool calculatedCostPerHourOtherEmployeesPermission = FeatureManager.HasAnyRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Time_CalculatedCostPerHour, Permission.Readonly, Company.ActorCompanyId, Company.LicenseId, userRoles, entities);
                bool employmentsPayrollPermissionMySelfRead = FeatureManager.HasAnyRolePermission(Feature.Time_Employee_Employees_Edit_MySelf_Employments_Payroll, Permission.Readonly, Company.ActorCompanyId, Company.LicenseId, userRoles, entities);
                bool employmentsPayrollSalaryPermissionOtherEmployeesRead = FeatureManager.HasAnyRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll_Salary, Permission.Readonly, Company.ActorCompanyId, Company.LicenseId, userRoles, entities);
                bool useExperienceMonthsOnEmploymentAsStartValue = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.UseEmploymentExperienceAsStartValue, 0, Company.ActorCompanyId, 0);
                List<ContactAddressItem> contactAddressCompany = ContactManager.GetContactAddressItems(Company.ActorCompanyId);
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                Contact companyEcoms = ContactManager.GetContactAndEcomFromActor(entitiesReadOnly, Company.ActorCompanyId);
                Dictionary<int, string> disbursementMethods = EmployeeManager.GetEmployeeDisbursementMethods((int)TermGroup_Languages.Swedish);
                int mySelfEmployeeId = EmployeeManager.GetEmployeeIdForUser(reportResult.UserId, Company.ActorCompanyId);

                if (employees == null)
                    employees = EmployeeManager.GetAllEmployeesByIds(entities, reportResult.ActorCompanyId, selectionEmployeeIds, loadEmployment: true, loadContact: true, loadEmploymentPriceType: true);

                if (LoadReportInformation)
                {
                    var langId = GetLangId();
                    reportsPersonalCategoryDict = base.GetTermGroupDict(TermGroup.PayrollReportsPersonalCategory, langId, includeKey: true);
                    reportsSalaryTypeDict = base.GetTermGroupDict(TermGroup.PayrollReportsSalaryType, langId, includeKey: true);
                    reportsAFACategoryDict = base.GetTermGroupDict(TermGroup.PayrollReportsAFACategory, langId, includeKey: true);
                    reportsPayrollReportsAFASpecialAgreementDict = base.GetTermGroupDict(TermGroup.PayrollReportsAFASpecialAgreement, langId, includeKey: true);
                    reportsPayrollReportsCollectumITPplanDict = base.GetTermGroupDict(TermGroup.PayrollReportsCollectumITPplan, langId, includeKey: true);
                    reportsWorkTimeCategoryDict = base.GetTermGroupDict(TermGroup.PayrollReportsWorkTimeCategory, langId, includeKey: true);
                    reportsKPABelongingDict = base.GetTermGroupDict(TermGroup.KPABelonging, langId, includeKey: true);
                    reportsKPAEndCodeDict = base.GetTermGroupDict(TermGroup.KPAEndCode, langId, includeKey: true);
                    reportsKPAAgreementTypeDict = base.GetTermGroupDict(TermGroup.KPAAgreementType, langId, includeKey: true);
                    reportsBygglosenSalaryFormulaDict = PayrollManager.GetPayrollPriceFormulasDict(entities, base.ActorCompanyId, false);
                    reportsBygglosenSalaryTypeDict = base.GetTermGroupDict(TermGroup.BygglosenSalaryType, langId, includeKey: true);
                    reportsGTPAgreementNumberDict = base.GetTermGroupDict(TermGroup.GTPAgreementNumber, langId, includeKey: true);
                }

                #endregion

                int xmlId = 0;
                foreach (Employee employee in employees)
                {
                    if (employee == null)
                        continue;

                    var contactInfo = ContactManager.GetContactInfoForEmployee(employee.EmployeeId, Company.ActorCompanyId, false);

                    bool employeeIsMySelf = employee.EmployeeId == mySelfEmployeeId;
                    bool employmentsPayrollPermissionRead = (employeeIsMySelf && employmentsPayrollPermissionMySelfRead) || employmentsPayrollSalaryPermissionOtherEmployeesRead;
                    bool calculatedCostPerHourPermission = (employee.EmployeeId == mySelfEmployeeId && calculatedCostPerHourMySelfPermission) || (employee.EmployeeId != mySelfEmployeeId && calculatedCostPerHourOtherEmployeesPermission);

                    var extraFields = ExtraFieldManager.GetExtraFieldWithRecords(employee.EmployeeId, (int)SoeEntityType.Employee, reportResult.ActorCompanyId, 0);

                    foreach (var employment in employee.GetActiveEmployments(includeSecondary: true))
                    {
                        if (employmentId.HasValue && employmentId != 0 && employment.EmploymentId != employmentId)
                            continue;

                        DateTime currentDate = employment.GetValidEmploymentDate(selectionDate);

                        int experienceMonths = EmployeeManager.GetExperienceMonths(entities, reportResult.ActorCompanyId, employment, useExperienceMonthsOnEmploymentAsStartValue, currentDate);
                        EmployeeTaxSE employeeTax = EmployeeManager.GetEmployeeTaxSE(entities, employee.EmployeeId, currentDate.Year);
                        EmployeeVacationSE vacationSE = EmployeeManager.GetLatestEmployeeVacationSE(entities, employee.EmployeeId);
                        xmlId++;

                        var editDTO = new EmployeeTemplateEditDTO(employee.EmployeeId, employment.EmploymentId, employee.EmployeeNr, employee.SocialSec, employee.Name, _reportDataInput.EmployeeTemplate.Title?.EmptyToNull() ?? _reportDataInput.EmployeeTemplate.Name ?? "");

                        foreach (var group in _reportDataInput.EmployeeTemplate.EmployeeTemplateGroups)
                        {
                            var groupDTO = new EmployeeTemplateEditGroupDTO(group);
                            bool groupAdded = false;

                            if (group.EmployeeTemplateGroupRows.Any())
                            {
                                foreach (var row in group.EmployeeTemplateGroupRows)
                                {
                                    var initialValue = string.Empty;
                                    EmployeeTemplateEditFieldDTO fieldDTO = null;

                                    switch (row.Type)
                                    {
                                        case TermGroup_EmployeeTemplateGroupRowType.Unknown:
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.FirstName:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, employee.FirstName, row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.LastName:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, employee.LastName, row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.Name:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, employee.Name, row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.SocialSec:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, employee.SocialSec, row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.EmployeeNr:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, employee.EmployeeNr, row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.DisbursementMethod:
                                            var method = disbursementMethods.ContainsKey(employee.DisbursementMethod) ? disbursementMethods[employee.DisbursementMethod] : "";
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, method, row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.DisbursementAccountNr:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, employee.DisbursementClearingNr + "-" + employee.DisbursementAccountNr, row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.EmploymentStartDate:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, employment.DateFrom.HasValue ? employment.DateFrom.ToShortDateString() : "", row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.EmploymentStopDate:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, employment.DateTo.HasValue ? employment.DateTo.ToShortDateString() : "", row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.EmploymentType:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, employment.GetEmploymentTypeName(employmentTypes, currentDate), row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.EmploymentWorkTimeWeek:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, CalendarUtility.GetHoursAndMinutesString(employment.GetWorkTimeWeek(currentDate)), row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.PrimaryEmploymentWorkTimeWeek:
                                            int workTimeWeek = employment.GetWorkTimeWeek(currentDate);
                                            if (employment.IsSecondaryEmployment)
                                            {
                                                Employment primaryEmployment = employee.GetEmployment(currentDate);
                                                if (primaryEmployment != null)
                                                    workTimeWeek = primaryEmployment.GetWorkTimeWeek(currentDate);
                                            }
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, CalendarUtility.GetHoursAndMinutesString(workTimeWeek), row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.TotalEmploymentWorkTimeWeek:
                                            // EmploymentWorkTimeWeek + PrimaryEmploymentWorkTimeWeek
                                            int workTimeWeek2 = employment.GetWorkTimeWeek(currentDate);
                                            if (employment.IsSecondaryEmployment)
                                            {
                                                Employment primaryEmployment = employee.GetEmployment(currentDate);
                                                if (primaryEmployment != null)
                                                    workTimeWeek2 = primaryEmployment.GetWorkTimeWeek(currentDate);
                                                workTimeWeek2 += employment.GetWorkTimeWeek(currentDate);
                                            }

                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, CalendarUtility.GetHoursAndMinutesString(workTimeWeek2), row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.EmploymentFullTimeWorkWeek:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, CalendarUtility.GetHoursAndMinutesString((employment.GetFullTimeWorkTimeWeek(currentDate)) ?? 0), row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.EmploymentPercent:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, NumberUtility.GetFormattedDecimalStringValue(employment.GetPercent(currentDate), 2), row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.ExperienceMonths:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, experienceMonths.ToString(), row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.ExperienceAgreedOrEstablished:
                                            var stringValue = employment.GetExperienceAgreedOrEstablished(currentDate) ? GetText(11707, "Överenskommen") : GetText(11708, "Konstaterad");
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, stringValue, row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.VacationDaysPayed:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, (vacationSE?.RemainingDaysPaid ?? vacationSE?.EarnedDaysPaid ?? 0).ToString(), row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.VacationDaysUnpayed:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, (vacationSE?.RemainingDaysUnpaid ?? vacationSE?.EarnedDaysUnpaid ?? 0).ToString(), row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.VacationDaysAdvance:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, (vacationSE?.RemainingDaysAdvance ?? vacationSE?.EarnedDaysAdvance ?? 0).ToString(), row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.TaxRate:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, employeeTax?.TaxRate.ToValueOrEmpty() ?? string.Empty, row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.TaxTinNumber:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, employeeTax?.TinNumber ?? string.Empty, row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.TaxCountryCode:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, employeeTax?.CountryCode ?? string.Empty, row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.TaxBirthPlace:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, employeeTax?.BirthPlace ?? string.Empty, row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.TaxCountryCodeBirthPlace:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, employeeTax?.CountryCodeBirthPlace ?? string.Empty, row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.TaxCountryCodeCitizen:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, employeeTax?.CountryCodeCitizen ?? string.Empty, row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.PayrollFormula:
                                            if (int.TryParse(row.DefaultValue, out int formulaId))
                                            {
                                                PayrollPriceFormulaResultDTO result = PayrollManager.EvaluatePayrollPriceFormula(entities, reportResult.ActorCompanyId, employee, employment, null, currentDate, null, null, formulaId);
                                                fieldDTO = new EmployeeTemplateEditFieldDTO(row, NumberUtility.GetFormattedDecimalStringValue(result?.Amount ?? 0, 2) ?? "", row.Title ?? "", "");
                                            }
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.Address:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, contactInfo.FirstOrDefault(f => f.IsAddress)?.ToAddressString() ?? "", row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.AddressRow:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, contactInfo.FirstOrDefault(f => f.IsAddress)?.Address ?? "", row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.AddressRow2:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, contactInfo.FirstOrDefault(f => f.IsAddress)?.AddressCO ?? "", row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.ZipCode:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, contactInfo.FirstOrDefault(f => f.IsAddress)?.PostalCode ?? "", row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.City:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, contactInfo.FirstOrDefault(f => f.IsAddress)?.PostalAddress ?? "", row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.ZipCity:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, $"{(contactInfo.FirstOrDefault(f => f.IsAddress)?.PostalCode ?? "")} {(contactInfo.FirstOrDefault(f => f.IsAddress)?.PostalAddress ?? "")}", row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.Telephone:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, contactInfo.FirstOrDefault(f => f.ContactAddressItemType == ContactAddressItemType.EComPhoneMobile)?.EComText ?? "", row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.Email:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, contactInfo.FirstOrDefault(f => f.ContactAddressItemType == ContactAddressItemType.EComEmail)?.EComText ?? "", row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.Position:
                                            var positions = EmployeeManager.GetEmployeePositions(entities, employee.EmployeeId, true);
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, positions?.FirstOrDefault(f => f.Default)?.Position?.Name ?? "", row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.WorkTasks:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, employment.GetWorkTasks(currentDate) ?? "", row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.Department:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, employment.GetWorkPlace(currentDate) ?? "", row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.WorkPlace:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, employment.GetWorkPlace(currentDate) ?? "", row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.SpecialConditions:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, employment.GetSpecialConditions(currentDate).NullToEmpty(), row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.SubstituteFor:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, employment.GetSubstituteFor(currentDate).NullToEmpty(), row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.SubstituteForDueTo:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, employment.GetSubstituteForDueTo(currentDate).NullToEmpty(), row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.ExternalCode:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, employment.GetExternalCode(currentDate).NullToEmpty(), row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.IsSecondaryEmployment:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, employment.IsSecondaryEmployment.ToString(), row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.CompanyName:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, Company.Name, row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.CompanyOrgNr:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, Company.OrgNr, row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.CompanyAddress:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, contactAddressCompany.FirstOrDefault(f => f.IsAddress)?.ToAddressString() ?? "", row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.CompanyAddressRow:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, contactAddressCompany.FirstOrDefault(f => f.IsAddress)?.Address ?? "", row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.CompanyAddressRow2:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, contactAddressCompany.FirstOrDefault(f => f.IsAddress)?.AddressCO ?? "", row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.CompanyZipCode:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, contactAddressCompany.FirstOrDefault(f => f.IsAddress)?.PostalCode ?? "", row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.CompanyCity:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, contactAddressCompany.FirstOrDefault(f => f.IsAddress)?.PostalAddress ?? "", row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.CompanyZipCity:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, $"{(contactAddressCompany.FirstOrDefault(f => f.IsAddress)?.PostalCode ?? "")} {(contactAddressCompany.FirstOrDefault(f => f.IsAddress)?.PostalAddress ?? "")}", row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.CompanyTelephone:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, companyEcoms?.ContactECom?.FirstOrDefault(f => f.SysContactEComTypeId == (int)TermGroup_SysContactEComType.PhoneJob)?.Name ?? "", row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.CompanyEmail:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, companyEcoms?.ContactECom?.FirstOrDefault(f => f.SysContactEComTypeId == (int)TermGroup_SysContactEComType.Email)?.Name ?? "", row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.CityAndDate:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, string.Empty, row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.SignatureEmployee:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, string.Empty, row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.SignatureEmployer:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, string.Empty, row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.GeneralText:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, row.DefaultValue ?? "", row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.ExtraFieldEmployee:
                                            ExtraFieldRecordDTO extraFieldEmployee = extraFields.FirstOrDefault(f => f.ExtraFieldId == row.RecordId);
                                            if (extraFieldEmployee != null)
                                            {
                                                string extraFieldEmployeeTitle = !row.Title.IsNullOrEmpty() ? row.Title : extraFieldEmployee.ExtraFieldText;
                                                string extraFieldEmployeeValue = string.Empty;

                                                switch ((TermGroup_ExtraFieldType)extraFieldEmployee.ExtraFieldType)
                                                {
                                                    case TermGroup_ExtraFieldType.Checkbox:
                                                        extraFieldEmployeeValue = !extraFieldEmployee.Value.IsNullOrEmpty() && extraFieldEmployee.Value.ToLowerInvariant() != "false" ? "true" : "false";
                                                        break;
                                                    case TermGroup_ExtraFieldType.SingleChoice:
                                                        ExtraFieldValueDTO value = extraFieldEmployee.ExtraFieldValues != null ? extraFieldEmployee.ExtraFieldValues.Where(f => f.ExtraFieldValueId == extraFieldEmployee.IntData).FirstOrDefault() : null;
                                                        if (value != null)
                                                            extraFieldEmployeeValue = value.Value;
                                                        break;
                                                    default:
                                                        extraFieldEmployeeValue = extraFieldEmployee.Value;
                                                        break;
                                                }
                                                fieldDTO = new EmployeeTemplateEditFieldDTO(row, string.IsNullOrEmpty(extraFieldEmployeeValue) ? row.DefaultValue : extraFieldEmployeeValue, extraFieldEmployeeTitle, "");
                                            }
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.ExtraFieldAccount:
                                             if (base.UseAccountHierarchyOnCompanyFromCache(entitiesReadOnly, reportResult.ActorCompanyId))
                                            {
                                                List<EmployeeAccount> accounts = EmployeeManager.GetEmployeeAccounts(reportResult.ActorCompanyId, employee.EmployeeId, selectionDate, selectionDate);
                                                List<ExtraFieldRecordDTO> extraFieldsAccounts = ExtraFieldManager.GetExtraFieldWithRecords(accounts.Select(s => s.AccountId.Value).ToList(), (int)SoeEntityType.Account, reportResult.ActorCompanyId, 0);
                                                ExtraFieldRecordDTO extraFieldAccount = extraFieldsAccounts.Where(w => w.ExtraFieldId == row.RecordId).FirstOrDefault();
                                                if (extraFieldAccount != null)
                                                {
                                                    string extraFieldAccountTitle = !row.Title.IsNullOrEmpty() ? row.Title : extraFieldAccount.ExtraFieldText;
                                                    string extraFieldAccountValue = string.Empty;

                                                    switch ((TermGroup_ExtraFieldType)extraFieldAccount.ExtraFieldType)
                                                    {
                                                        case TermGroup_ExtraFieldType.Checkbox:
                                                            extraFieldAccountValue = !extraFieldAccount.Value.IsNullOrEmpty() ? "true" : "false";
                                                            break;
                                                        case TermGroup_ExtraFieldType.SingleChoice:
                                                            List<ExtraFieldValue> values = ExtraFieldManager.GetExtraFieldValues(extraFieldAccount.ExtraFieldId);
                                                            ExtraFieldValue value = values.Where(f => f.ExtraFieldValueId == extraFieldAccount.IntData).FirstOrDefault();
                                                            if (value != null)
                                                                extraFieldAccountValue = value.Value;
                                                            break;
                                                        default:
                                                            extraFieldAccountValue = extraFieldAccount.Value;
                                                            break;
                                                    }
                                                    fieldDTO = new EmployeeTemplateEditFieldDTO(row, string.IsNullOrEmpty(extraFieldAccountValue) ? row.DefaultValue : extraFieldAccountValue, extraFieldAccountTitle, "");
                                                }
                                            }
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.PayrollStatisticsPersonalCategory:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, GetValueFromDict(employee.PayrollStatisticsPersonalCategory, reportsPersonalCategoryDict) ?? "", "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.PayrollStatisticsWorkTimeCategory:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, GetValueFromDict(employee.PayrollStatisticsWorkTimeCategory, reportsWorkTimeCategoryDict) ?? "", "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.PayrollStatisticsSalaryType:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, GetValueFromDict(employee.PayrollStatisticsSalaryType, reportsSalaryTypeDict) ?? "", "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.PayrollStatisticsWorkPlaceNumber:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, employee.PayrollStatisticsWorkPlaceNumber?.ToString() ?? string.Empty, row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.PayrollStatisticsCFARNumber:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, employee.PayrollStatisticsCFARNumber?.ToString() ?? string.Empty, row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.ControlTaskWorkPlaceSCB:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, employee.WorkPlaceSCB ?? string.Empty, row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.ControlTaskPartnerInCloseCompany:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, employee.PartnerInCloseCompany.ToString() ?? false.ToInt().ToString(), row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.ControlTaskBenefitAsPension:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, employee.BenefitAsPension.ToString() ?? false.ToInt().ToString(), row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.AFACategory:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, GetValueFromDict(employee.AFACategory, reportsAFACategoryDict) ?? "", "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.AFASpecialAgreement:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, GetValueFromDict(employee.AFASpecialAgreement, reportsPayrollReportsAFASpecialAgreementDict) ?? "", "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.AFAWorkplaceNr:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, employee.AFAWorkplaceNr ?? string.Empty, row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.AFAParttimePensionCode:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, employee.AFAParttimePensionCode.ToString(), row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.CollectumITPPlan:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, GetValueFromDict(employee.CollectumITPPlan, reportsPayrollReportsCollectumITPplanDict) ?? "", "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.CollectumAgreedOnProduct:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, employee.CollectumAgreedOnProduct ?? string.Empty, row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.CollectumCostPlace:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, employee.CollectumCostPlace ?? string.Empty, row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.CollectumCancellationDate:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, employee.CollectumCancellationDate?.ToShortDateString() ?? string.Empty, row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.CollectumCancellationDateIsLeaveOfAbsence:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, employee.CollectumCancellationDateIsLeaveOfAbsence.ToString() ?? false.ToInt().ToString(), row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.KPARetirementAge:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, employee.KPARetirementAge.ToString() ?? string.Empty, row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.KPABelonging:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, GetValueFromDict(employee.KPABelonging, reportsKPABelongingDict) ?? "", "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.KPAEndCode:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, GetValueFromDict(employee.KPAEndCode, reportsKPAEndCodeDict) ?? "", "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.KPAAgreementType:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, GetValueFromDict(employee.KPAAgreementType, reportsKPAAgreementTypeDict) ?? "", "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.BygglosenAgreementArea:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, employee.BygglosenAgreementArea ?? string.Empty, row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.BygglosenAllocationNumber:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, employee.BygglosenAllocationNumber ?? string.Empty, row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.BygglosenMunicipalCode:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, employee.BygglosenMunicipalCode ?? string.Empty, row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.BygglosenSalaryFormula:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, GetValueFromDict(employee.BygglosenSalaryFormula, reportsBygglosenSalaryFormulaDict) ?? "", "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.BygglosenProfessionCategory:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, employee.BygglosenProfessionCategory ?? string.Empty, row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.BygglosenSalaryType:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, GetValueFromDict(employee.BygglosenSalaryType, reportsBygglosenSalaryTypeDict) ?? "", "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.BygglosenWorkPlaceNumber:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, employee.BygglosenWorkPlaceNumber ?? string.Empty, row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.BygglosenLendedToOrgNr:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, employee.BygglosenLendedToOrgNr ?? string.Empty, row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.BygglosenAgreedHourlyPayLevel:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, employee.BygglosenAgreedHourlyPayLevel?.ToString() ?? string.Empty, row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.GTPAgreementNumber:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, GetValueFromDict(employee.GTPAgreementNumber, reportsGTPAgreementNumberDict) ?? "", "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.GTPExcluded:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, employee.GTPExcluded.ToString() ?? false.ToInt().ToString(), row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.AGIPlaceOfEmploymentAddress:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, employee.AGIPlaceOfEmploymentAddress ?? string.Empty, row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.AGIPlaceOfEmploymentCity:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, employee.AGIPlaceOfEmploymentCity ?? string.Empty, row.Title ?? "", "");
                                            break;
                                        case TermGroup_EmployeeTemplateGroupRowType.AGIPlaceOfEmploymentIgnore:
                                            fieldDTO = new EmployeeTemplateEditFieldDTO(row, employee.AGIPlaceOfEmploymentIgnore.ToString() ?? false.ToInt().ToString(), row.Title ?? "", "");
                                            break;
                                        default:
                                            break;
                                    }

                                    if (fieldDTO != null)
                                    {
                                        bool isEmpty = fieldDTO.InitialValue.IsNullOrEmpty() || fieldDTO.InitialValue == bool.FalseString;
                                        if (!(row.HideInReportIfEmpty && isEmpty))
                                            groupDTO.EmployeeTemplateEditRows.Add(fieldDTO);
                                    }
                                }

                                editDTO.EmployeeTemplateEditGroups.Add(groupDTO);
                                groupAdded = true;
                            }

                            if (groupDTO.Type == TermGroup_EmployeeTemplateGroupType.SubstituteShifts && isPrintedFromSchedulePlanning && !substituteDates.IsNullOrEmpty())
                            {
                                List<SubstituteShiftDTO> substituteShifts = TimeScheduleManager.GetSubstituteShifts(entities, Company.ActorCompanyId, employee.EmployeeId, substituteDates);
                                foreach (SubstituteShiftDTO substituteShift in substituteShifts)
                                {
                                    string substituteText = string.Empty;
                                    bool isAbsence = false;

                                    if (substituteShift.IsAssignedDueToAbsence)
                                    {
                                        isAbsence = true;
                                        if (!string.IsNullOrEmpty(substituteShift.OriginEmployeeName) && !string.IsNullOrEmpty(substituteShift.AbsenceName))
                                            substituteText = string.Format(GetText(8752, "Vikariat för {0} under dennes frånvaro pga {1}"), substituteShift.OriginEmployeeName, substituteShift.AbsenceName);
                                    }
                                    else if (substituteShift.IsMoved || substituteShift.IsCopied)
                                    {
                                        if (!string.IsNullOrEmpty(substituteShift.OriginEmployeeName))
                                            substituteText = string.Format(GetText(8753, "Passet kommer från {0}"), substituteShift.OriginEmployeeName);
                                    }
                                    else if (substituteShift.IsExtraShift)
                                    {
                                        substituteText = GetText(8831, "Extrapass");
                                    }
                                    else
                                    {
                                        continue;
                                    }

                                    groupDTO.SubstituteShiftsTuples.Add(Tuple.Create(substituteShift.Date, isAbsence, substituteText, substituteShift));
                                }

                                if (!groupAdded)
                                    editDTO.EmployeeTemplateEditGroups.Add(groupDTO);
                            }
                        }

                        _reportDataOutput.EmploymentDynamicContractItems.Add(new EmploymentDynamicContractItem(editDTO));
                    }
                }
            }

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            if (_reportDataInput.CreateXElements)
                _reportDataOutput.Elements = GetXElements();

            return new ActionResult();
        }

        private List<XElement> GetXElements()
        {
            int xmlId = 0;
            return _reportDataOutput.EmploymentDynamicContractItems.Select(s => s.GetElement(xmlId++)).ToList();
        }

        private string GetValueFromDict(int? key, Dictionary<int, string> dict)
        {
            if (!key.HasValue || dict.Count == 0)
                return string.Empty;

            dict.TryGetValue(key.Value, out string value);

            if (value != null)
                return value;

            return string.Empty;
        }
    }

    public class EmploymentDynamicContractReportInput
    {
        public CreateReportResult ReportResult { get; set; }
        public EmployeeTemplateDTO EmployeeTemplate { get; set; }
        public List<DateTime> SubstituteDates { get; set; }
        public bool CreateXElements { get; set; }

        public EmploymentDynamicContractReportInput(CreateReportResult reportResult, EmployeeTemplateDTO employeeTemplate, List<DateTime> substituteDates, bool createXElements)
        {
            this.ReportResult = reportResult;
            this.EmployeeTemplate = employeeTemplate;
            this.CreateXElements = createXElements;
            this.SubstituteDates = substituteDates;
        }
    }

    public class EmploymentDynamicContractReportOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public EmploymentDynamicContractReportInput Input { get; set; }
        public List<EmploymentDynamicContractItem> EmploymentDynamicContractItems { get; set; }
        public List<XElement> Elements { get; set; }

        public EmploymentDynamicContractReportOutput(EmploymentDynamicContractReportInput input)
        {
            this.Input = input;
            this.EmploymentDynamicContractItems = new List<EmploymentDynamicContractItem>();
        }
    }
}

