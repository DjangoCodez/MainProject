using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.Reporting;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Util.ExportFiles
{
    public class CertificateOfEmployment : ExportFilesBase
    {
        private readonly DateTime _selectionDate;
        private readonly List<DayType> dayTypes;
        private readonly List<HolidayDTO> companyHolidays;
        private readonly List<int> hourlySalaryProducts;
        private readonly List<int> scheduledTimeProducts;
        private readonly List<int> absenceProducts;
        private readonly List<int> illnessProducts;
        private readonly List<int> illnesssQualifyingDayProducts;
        private readonly List<int> overtimeAdditionProducts;
        private readonly List<int> overtimeCompensationProducts;
        private readonly List<int> additionalTimeProducts;
        private readonly List<int> dutyProducts;
        private readonly List<int> obProducts;
        private readonly List<int> permissionProducts;
        private readonly List<int> vacationProducts;
        private readonly List<int> vacationAdditionProducts;
        private readonly List<int> vacationCompensationProducts;
        private readonly List<int> vacationCompensationDirectPayProducts;
        private readonly List<int> vacationSalaryproducts;
        private readonly List<int> accProducts;
        private readonly List<int> selectedAbsenceProducts;
        private readonly Dictionary<int, string> endReasons;
        private readonly List<EmploymentTypeDTO> _employmentTypes;
        private readonly string _selectionSpecial;
        private readonly List<int> illnessInconvenientSalaryIds;
        private readonly bool sendToArbetsgivarIntyg;

        public string CompanyName;
        public string CareOf;
        public string Street;
        public string ZipCode;
        public string City;
        public string Email;
        public string OrgNr;
        public string Telephone;

        public CertificateOfEmployment(ParameterObject parameterObject, CreateReportResult reportResult, List<Employee> employees, DateTime selectionDate, string selectionSpecial, bool sendToArbetsgivarIntyg) : base(parameterObject, reportResult)
        {
            this.reportResult = reportResult;
            TermGroup_Languages lang = TermCacheManager.Instance.GetLang();
            _employmentTypes = EmployeeManager.GetEmploymentTypes(parameterObject.ActorCompanyId, lang);
            CertificateOfEmploymentEmployees = new List<CertificateOfEmploymentEmployee>();
            _selectionDate = selectionDate;
            _selectionSpecial = selectionSpecial;

            CompanyName = Company.Name;
            OrgNr = Company.OrgNr;
            this.sendToArbetsgivarIntyg = sendToArbetsgivarIntyg;

            Contact contact = ContactManager.GetContactFromActor(Company.ActorCompanyId);
            List<ContactECom> contactEcoms = contact != null ? ContactManager.GetContactEComs(contact.ContactId) : null;
            Email = contactEcoms.GetEComText(TermGroup_SysContactEComType.Email);
            Telephone = contactEcoms.GetEComText(TermGroup_SysContactEComType.PhoneJob);
            List<ContactAddressRow> contactAddressRows = contact != null ? ContactManager.GetContactAddressRows(contact.ContactId) : null;
            Street = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.Address);
            CareOf = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.AddressCO);
            ZipCode = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.PostalCode);
            City = contactAddressRows.GetContactAddressRowText(TermGroup_SysContactAddressType.Distribution, TermGroup_SysContactAddressRowType.PostalAddress);

            dayTypes = CalendarManager.GetDayTypesByCompany(ReportResult.ActorCompanyId, true);
            companyHolidays = CalendarManager.GetHolidaysByCompany(ReportResult.ActorCompanyId, loadDayType: true);
            hourlySalaryProducts = ProductManager.GetPayrollProductIdsByType(ReportResult.ActorCompanyId, TermGroup_SysPayrollType.SE_GrossSalary, TermGroup_SysPayrollType.SE_GrossSalary_HourlySalary);
            scheduledTimeProducts = ProductManager.GetPayrollProductIdsByType(ReportResult.ActorCompanyId, TermGroup_SysPayrollType.SE_Time, TermGroup_SysPayrollType.SE_Time_WorkedScheduledTime);
            absenceProducts = ProductManager.GetPayrollProductIdsByType(ReportResult.ActorCompanyId, TermGroup_SysPayrollType.SE_GrossSalary, TermGroup_SysPayrollType.SE_GrossSalary_Absence);
            illnessProducts = ProductManager.GetPayrollProductIdsByType(ReportResult.ActorCompanyId, TermGroup_SysPayrollType.SE_GrossSalary, TermGroup_SysPayrollType.SE_GrossSalary_SicknessSalary, TermGroup_SysPayrollType.SE_GrossSalary_SicknessSalary_Day2_14);
            illnesssQualifyingDayProducts = ProductManager.GetPayrollProductIdsByType(ReportResult.ActorCompanyId, TermGroup_SysPayrollType.SE_GrossSalary, TermGroup_SysPayrollType.SE_GrossSalary_SicknessSalary, TermGroup_SysPayrollType.SE_GrossSalary_SicknessSalary_Deduction);
            overtimeAdditionProducts = ProductManager.GetPayrollProductIdsByType(ReportResult.ActorCompanyId, TermGroup_SysPayrollType.SE_GrossSalary, TermGroup_SysPayrollType.SE_GrossSalary_OvertimeAddition);
            overtimeCompensationProducts = ProductManager.GetPayrollProductIdsByType(ReportResult.ActorCompanyId, TermGroup_SysPayrollType.SE_GrossSalary, TermGroup_SysPayrollType.SE_GrossSalary_OvertimeCompensation);
            vacationAdditionProducts = ProductManager.GetPayrollProductIdsByType(ReportResult.ActorCompanyId, TermGroup_SysPayrollType.SE_GrossSalary, TermGroup_SysPayrollType.SE_GrossSalary_VacationAddition);
            additionalTimeProducts = ProductManager.GetPayrollProductIdsByType(ReportResult.ActorCompanyId, TermGroup_SysPayrollType.SE_GrossSalary, TermGroup_SysPayrollType.SE_GrossSalary_AdditionalTime);
            dutyProducts = ProductManager.GetPayrollProductIdsByType(ReportResult.ActorCompanyId, TermGroup_SysPayrollType.SE_GrossSalary, TermGroup_SysPayrollType.SE_GrossSalary_Duty);
            obProducts = ProductManager.GetPayrollProductIdsByType(ReportResult.ActorCompanyId, TermGroup_SysPayrollType.SE_GrossSalary, TermGroup_SysPayrollType.SE_GrossSalary_OBAddition);
            permissionProducts = ProductManager.GetPayrollProductIdsByType(ReportResult.ActorCompanyId, TermGroup_SysPayrollType.SE_GrossSalary, TermGroup_SysPayrollType.SE_GrossSalary_Absence, TermGroup_SysPayrollType.SE_GrossSalary_Absence_Permission);
            vacationProducts = ProductManager.GetPayrollProductIdsByType(ReportResult.ActorCompanyId, TermGroup_SysPayrollType.SE_GrossSalary, TermGroup_SysPayrollType.SE_GrossSalary_Absence, TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation);
            vacationCompensationProducts = ProductManager.GetPayrollProductIdsByType(ReportResult.ActorCompanyId, TermGroup_SysPayrollType.SE_GrossSalary, TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation);
            vacationCompensationProducts.AddRange(ProductManager.GetPayrollProductIdsByType(ReportResult.ActorCompanyId, TermGroup_SysPayrollType.SE_GrossSalary, TermGroup_SysPayrollType.SE_GrossSalary_VacationAdditionVariable));
            vacationCompensationProducts.AddRange(ProductManager.GetPayrollProductIdsByType(ReportResult.ActorCompanyId, TermGroup_SysPayrollType.SE_GrossSalary, TermGroup_SysPayrollType.SE_GrossSalary_VacationAdditionOrSalaryPrepayment));
            vacationCompensationDirectPayProducts = ProductManager.GetPayrollProductIdsByType(ReportResult.ActorCompanyId, TermGroup_SysPayrollType.SE_GrossSalary, TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation, TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_DirectPaid);
            vacationSalaryproducts = ProductManager.GetPayrollProductIdsByType(ReportResult.ActorCompanyId, TermGroup_SysPayrollType.SE_GrossSalary, TermGroup_SysPayrollType.SE_GrossSalary_VacationSalary);
            accProducts = ProductManager.GetPayrollProductIdsByType(ReportResult.ActorCompanyId, TermGroup_SysPayrollType.SE_Time, TermGroup_SysPayrollType.SE_Time_Accumulator, TermGroup_SysPayrollType.SE_Time_Accumulator_AccumulatorPlaceholder, TermGroup_SysPayrollType.SE_Time_Accumulator_Withdrawal);
            selectedAbsenceProducts = absenceProducts.Where(w => !permissionProducts.Contains(w) && !vacationProducts.Contains(w) && !accProducts.Contains(w)).ToList();
            endReasons = EmployeeManager.GetSystemEndReasons(Company.ActorCompanyId, includeCompanyEndReasons: true);
            bool showSocialSec = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec, Permission.Readonly, ReportResult.RoleId, ReportResult.ActorCompanyId);

            illnessInconvenientSalaryIds = new List<int>();
            var timeAbsenceRulesInput = new GetTimeAbsenceRulesInput(ReportResult.ActorCompanyId)
            {
                LoadRows = true,
            };
            var timeAbsenceRules = TimeRuleManager.GetTimeAbsenceRules(timeAbsenceRulesInput).ToDTOs(true, true).ToList();
            if (timeAbsenceRules.Any())
            {
                foreach (var timeAbsenceRule in timeAbsenceRules.Where(w => w.TimeAbsenceRuleRows != null))
                {
                    foreach (var timeAbsenceRuleRow in timeAbsenceRule.TimeAbsenceRuleRows.Where(w => w.PayrollProductRows != null))
                    {
                        foreach (var productMapping in timeAbsenceRuleRow.PayrollProductRows.Where(w => w.TargetPayrollProductId.HasValue))
                        {
                            illnessInconvenientSalaryIds.Add(productMapping.TargetPayrollProductId.Value);
                        }
                    }
                }

                illnessInconvenientSalaryIds = illnessInconvenientSalaryIds.Where(w => illnessProducts.Contains(w)).ToList();
                illnessProducts = illnessProducts.Where(w => !illnessInconvenientSalaryIds.Contains(w)).ToList();
            }

            foreach (var employee in employees)
            {
                List<EmployeePosition> employeePositions = EmployeeManager.GetEmployeePositions(employee.EmployeeId);
                var employeePosition = employeePositions.Any(w => w.Default) ? employeePositions.FirstOrDefault(w => w.Default) : employeePositions.LastOrDefault();

                if (employeePosition == null)
                {
                    ReportResult.SetErrorMessage(SoeReportDataResultMessage.Error, new Exception("Befattning saknas på anställd " + employee.EmployeeNrAndName));
                    ErrorMessage = "Befattning saknas på anställd " + employee.EmployeeNrAndName;
                    return;
                }

                //Make sure ContactPerson is loaded
                if (!employee.ContactPersonReference.IsLoaded)
                    employee.ContactPersonReference.Load();

                var contactId = ContactManager.GetContactIdFromActorId(employee.ContactPersonId);
                List<ContactECom> employeeContactEcoms = employee.ContactPersonReference != null ? ContactManager.GetContactEComs(contactId) : null;
                string email = employeeContactEcoms.GetEComText(TermGroup_SysContactEComType.Email);
                if (string.IsNullOrEmpty(email))
                    email = employeeContactEcoms.GetEComText(TermGroup_SysContactEComType.CompanyAdminEmail);

                string telephone = employeeContactEcoms.GetEComText(TermGroup_SysContactEComType.PhoneMobile);
                if (string.IsNullOrEmpty(telephone))
                    telephone = employeeContactEcoms.GetEComText(TermGroup_SysContactEComType.PhoneHome);
                if (string.IsNullOrEmpty(telephone))
                    telephone = employeeContactEcoms.GetEComText(TermGroup_SysContactEComType.PhoneJob);

                CertificateOfEmploymentEmployees.Add(new CertificateOfEmploymentEmployee()
                {
                    Employee = employee,
                    EmployeeNr = employee.EmployeeNr,
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    SocialSec = showSocialSec ? employee.SocialSec : StringUtility.SocialSecYYMMDD_Dash_Stars(employee.SocialSec),
                    Note = employee.Note,
                    CertificateOfEmploymentMergedEmployments = CreateCertificateOfEmploymentMergedEmployments(employee),
                    Email = email,
                    Phone = telephone,
                });
            }
        }

        public List<CertificateOfEmploymentEmployee> CertificateOfEmploymentEmployees { get; set; }
        public string ErrorMessage { get; set; }
        private List<CertificateOfEmploymentEmployment> CreateCertificateOfEmploymentMergedEmployments(Employee employee)
        {
            List<CertificateOfEmploymentEmployment> certificateOfEmploymentEmployments = new List<CertificateOfEmploymentEmployment>();

            DateTime stopDate = _selectionDate.Date;
            DateTime startDate = CalendarUtility.GetBeginningOfMonth(stopDate.AddMonths(-12));
            List<EmployeePosition> employeePositions = EmployeeManager.GetEmployeePositions(employee.EmployeeId);
            EmployeePosition employeePosition = employeePositions.FirstOrDefault(w => w.Default) ?? employeePositions.LastOrDefault();
            List<Employment> employments = employee.GetEmployments(startDate, stopDate);

            List<EmploymentDTO> mergedEmployments = new List<EmploymentDTO>();
            EmploymentDTO prevEmployment = null;
            int numberOfEmploymentsHandled = 0;

            foreach (var employment in employments.ToSplittedDTOs(includeEmployeeGroup: true, includePayrollGroup: true))
            {
                if (employment.DateFrom > stopDate)
                    continue;

                DateTime dateFrom = employment.DateFrom.ToValueOrDefault();
                if (prevEmployment != null)
                {
                    if (prevEmployment.DateTo == dateFrom.AddDays(-1))
                    {
                        prevEmployment.DateTo = employment.DateTo;
                    }
                    else
                    {
                        mergedEmployments.Add(prevEmployment.CloneDTO());
                        prevEmployment = employment;
                    }
                }
                else
                {
                    prevEmployment = employment;
                }

                numberOfEmploymentsHandled++;

                if (employments.Count == numberOfEmploymentsHandled)
                    mergedEmployments.Add(prevEmployment);
            }

            foreach (var employment in mergedEmployments)
            {
                employment.ApplyEmploymentChanges(employment.DateTo.ToValueOrToday());

                TermGroup_PayrollExportSalaryType salaryType;

                if (employment.DateTo.HasValue)
                    salaryType = EmployeeManager.GetEmployeeSalaryType(employee, employment.DateTo.Value, employment.DateTo.Value);
                else
                    salaryType = EmployeeManager.GetEmployeeSalaryType(employee, _selectionDate, _selectionDate);

                EmployeeGroup employeeGroup = EmployeeManager.GetEmployeeGroup(employment.EmployeeGroupId);

                CertificateOfEmploymentEmployment certificateOfEmploymentEmployment = new CertificateOfEmploymentEmployment()
                {
                    DateFrom = employment.DateFrom.ToValueOrDefault(),
                    DateTo = employment.DateTo.ToValueOrDefault(),
                    PositionCode = employeePosition?.Position?.Code ?? string.Empty,
                    PositionName = employeePosition?.Position?.Name ?? string.Empty,
                    EmploymentType = (TermGroup_EmploymentType)_employmentTypes.GetType(employment.EmploymentType),
                    EmploymentTypeName = employment.GetEmploymentTypeName(_employmentTypes),
                    WorkTime = employment.WorkTimeWeek,
                    WorkTimeEmployeeGroup = employeeGroup?.RuleWorkTimeWeek ?? 0,
                    EmployeeGroup = employeeGroup,
                    WorkPercent = employment.Percent,
                    EndReason = employment.EmploymentEndReason,
                    EndReasonName = endReasons.FirstOrDefault(f => employment.EmploymentEndReason == f.Key).Value ?? string.Empty,
                    PayrollYear = employment.DateTo.HasValue && employment.DateTo.Value < _selectionDate ? employment.DateTo.Value.Year : _selectionDate.Year,
                    SalaryType = salaryType,
                    EmploymentDTO = employment,
                };

                certificateOfEmploymentEmployment.EndReasonCode = ArbetsgivarintygPunktNu.GetOrsakNumber(certificateOfEmploymentEmployment);

                AddScheduleInformation(employee, certificateOfEmploymentEmployment);
                AddPayrollAmount(employee, certificateOfEmploymentEmployment);
                AddGroupsRedDaysAndTransactions(employee, certificateOfEmploymentEmployment);
                AddLeaveOfAbsenceInformation(employee, certificateOfEmploymentEmployment);
               
                certificateOfEmploymentEmployments.Add(certificateOfEmploymentEmployment);

            }

            return certificateOfEmploymentEmployments;
        }

        public void AddPayrollAmount(Employee employee, CertificateOfEmploymentEmployment certificateOfEmploymentEmployment)
        {
            Employment employment = employee.GetEmployment(certificateOfEmploymentEmployment.EmploymentDTO.DateTo);
            PayrollGroup payrollGroup = employment?.GetPayrollGroup(certificateOfEmploymentEmployment.EmploymentDTO.DateTo);
            if (payrollGroup != null)
            {
                if (!payrollGroup.PayrollGroupSetting.IsLoaded)
                    payrollGroup.PayrollGroupSetting.Load();

                if (!payrollGroup.PayrollGroupSetting.IsNullOrEmpty())
                {
                    PayrollGroupSetting setting = payrollGroup.PayrollGroupSetting.FirstOrDefault(p => p.Type == (int)PayrollGroupSettingType.PayrollFormula);
                    if (setting != null && setting.IntData.HasValue && setting.IntData.Value != 0)
                    {
                        PayrollPriceFormulaResultDTO result = PayrollManager.EvaluatePayrollPriceFormula(ReportResult.ActorCompanyId, employee, employment, null, certificateOfEmploymentEmployment.EmploymentDTO.DateTo ?? DateTime.Now, payrollPriceFormulaId: setting.IntData.Value);
                        if (result != null)
                            certificateOfEmploymentEmployment.PayrollAmount = decimal.Round(result.Amount, 2);
                    }
                }
            }
        }

        private void AddRedDays(Employee employee, CertificateOfEmploymentEmployment certificateOfEmploymentEmployment)
        {
            if (certificateOfEmploymentEmployment.SalaryType != TermGroup_PayrollExportSalaryType.Hourly)
            {
                DateTime stopDate = _selectionDate.Date;
                DateTime startDate = CalendarUtility.GetBeginningOfMonth(stopDate.AddMonths(-12));
                DateTime date = startDate > certificateOfEmploymentEmployment.DateFrom ? startDate : certificateOfEmploymentEmployment.DateFrom;
                DateTime endDate = certificateOfEmploymentEmployment.EmploymentDTO.DateTo < stopDate ? certificateOfEmploymentEmployment.DateTo : stopDate;
                EmployeeGroup employeeGroup = EmployeeManager.GetEmployeeGroup(certificateOfEmploymentEmployment.EmployeeGroup.EmployeeGroupId);
                var shifts = TimeScheduleManager.GetTimeSchedulePlanningShiftsAggregated_ByProcedure(ReportResult.ActorCompanyId, ReportResult.UserId, employee.EmployeeId, ReportResult.RoleId, date, endDate, new List<int>() { employee.EmployeeId }, new List<int>(), TimeSchedulePlanningDisplayMode.Admin, false, true, false);

                while (date <= endDate)
                {
                    Employment employment2 = employee.GetEmployment(date);
                    if (employment2 != null && employeeGroup != null)
                    {
                        bool useZeroSchedule = false;
                        bool isCompanyHalfday = false;
                        bool isCompanyHoliday = false;

                        if (!employeeGroup.DayType.IsLoaded)
                            employeeGroup.DayType.Load();

                        // Rule 1. EmployeeGroup doesn't work at all
                        if (employeeGroup.DayType.Count == 0)
                            useZeroSchedule = true;

                        // Rule 2. EmployeeGroup doesn't work on the specified DayType
                        DayType dayType = CalendarManager.GetDayType(date, employeeGroup, companyHolidays, dayTypes);
                        if (dayType == null)
                            useZeroSchedule = true;

                        // Rule 3. EmployeeGroup usually work on the specified DayType but it is a Company Holiday and not a HalfDay
                        List<HolidayDTO> holidaysForCompanyAndDate = companyHolidays.Where(i => i.Date.Date == date.Date).ToList();
                        isCompanyHoliday = holidaysForCompanyAndDate.Count > 0;
                        isCompanyHalfday = CalendarManager.IsDateHalfDay(holidaysForCompanyAndDate, dayType);
                        if (isCompanyHoliday && !isCompanyHalfday)
                            useZeroSchedule = true;

                        if (useZeroSchedule || isCompanyHalfday)
                        {
                            var dayDTO = TimeScheduleManager.GetTimeSchedulePlanningTemplateAggregated(ReportResult.ActorCompanyId, ReportResult.RoleId, ReportResult.UserId, employee.EmployeeId, date, date);
                            int minutes = Convert.ToInt32(dayDTO.Sum(s => s.ScheduleTime.TotalMinutes - s.ScheduleBreakTime));

                            if (minutes > 0){
                                var dayTransaction = certificateOfEmploymentEmployment.FilteredtransItems.Where(w => w.TimeBlockDate == date);
                                if (dayTransaction.Any())
                                    minutes -= Convert.ToInt32(dayTransaction.Where(w => w.IsWorkTime() || absenceProducts.Contains(w.PayrollProductId)).Sum(s => s.Quantity));
                            }
                            if (minutes > 0)
                            {
                                TimePayrollStatisticsDTO dto = new TimePayrollStatisticsDTO();

                                //TimePayrollTransaction
                                dto.EmployeeId = employee.EmployeeId;
                                dto.Quantity = !isCompanyHalfday ? minutes : minutes - Convert.ToInt32(shifts.Where(i => i.StartTime.Date == date).Sum(s => s.ScheduleTime.TotalMinutes - s.ScheduleBreakTime));
                                dto.PayrollProductNumber = "0";
                                dto.TimeBlockDate = date;
                                dto.PayrollProductName = !isCompanyHalfday ? "Ledig dag" : "Halvdag";
                                dto.SysPayrollTypeLevel1 = (int)TermGroup_SysPayrollType.SE_Time;
                                dto.SysPayrollTypeLevel2 = (int)TermGroup_SysPayrollType.SE_Time_WorkedScheduledTime;
                                dto.SysPayrollTypeLevel1Name = GetText((int)TermGroup_SysPayrollType.SE_Time, (int)TermGroup.SysPayrollType);
                                dto.SysPayrollTypeLevel2Name = GetText((int)TermGroup_SysPayrollType.SE_Time_WorkedScheduledTime, (int)TermGroup.SysPayrollType);
                                dto.SysPayrollTypeLevel3Name = "Ledig dag";
                                dto.SysPayrollTypeLevel4Name = string.Empty;
                                dto.PayrollProductId = (scheduledTimeProducts?.FirstOrDefault() ?? hourlySalaryProducts?.FirstOrDefault()) ?? 0;
                                certificateOfEmploymentEmployment.FilteredtransItems.Add(dto);
                            }
                        }

                        date = date.AddDays(1);
                    }
                }
            }

        }

        private void AddGroupsRedDaysAndTransactions(Employee employee, CertificateOfEmploymentEmployment certificateOfEmploymentEmployment)
        {

            using (CompEntities entities = new CompEntities())
            {
                List<Employee> thisEmployee = new List<Employee>() { employee };
                DateTime stopDate = _selectionDate.Date;
                DateTime startDate = CalendarUtility.GetBeginningOfMonth(stopDate.AddMonths(-12));

                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                var timePeriodIds = entitiesReadOnly.EmployeeTimePeriod.Where(w => w.EmployeeId == employee.EmployeeId && w.State == (int)SoeEntityState.Active && w.TimePeriod.StartDate >= startDate).Select(s => s.TimePeriodId).ToList();

                #region Get and filter transactions

                bool overTimeOrAddedTimeHasDifferentHourlySalary = false;
                decimal overtimeHourlySalary = 0;
                decimal addedTimeHourlySalary = 0;

                List<TimePayrollStatisticsDTO> timePayrollStatisticsDTOs = TimeTransactionManager.GetTimePayrollStatisticsDTOs(entities, ReportResult.ActorCompanyId, thisEmployee, timePeriodIds, ignoreAccounting: true);
                timePayrollStatisticsDTOs = timePayrollStatisticsDTOs.Where(w => !w.RetroactivePayrollOutcomeId.HasValue).ToList();

                foreach (var timePayrollStatisticsDTO in timePayrollStatisticsDTOs.Where(t => !t.IsEmploymentTaxBelowLimitHidden && t.TimeBlockDate.Date >= startDate && t.TimeBlockDate.Date <= stopDate))
                {
                    if (!timePayrollStatisticsDTO.IsScheduleTransaction)
                    {
                        if (hourlySalaryProducts.Contains(timePayrollStatisticsDTO.PayrollProductId) || scheduledTimeProducts.Contains(timePayrollStatisticsDTO.PayrollProductId))
                        {
                            certificateOfEmploymentEmployment.FilteredtransItems.Add(timePayrollStatisticsDTO);
                            continue;
                        }

                        if (absenceProducts.Contains(timePayrollStatisticsDTO.PayrollProductId))
                        {
                            certificateOfEmploymentEmployment.FilteredtransItems.Add(timePayrollStatisticsDTO);
                            if (timePayrollStatisticsDTO.IsVacationUnPaid() || timePayrollStatisticsDTO.IsAbsenceVacationNoVacationDaysDeducted() || !vacationProducts.Contains(timePayrollStatisticsDTO.PayrollProductId))
                                continue;
                        }

                        if (overtimeAdditionProducts.Contains(timePayrollStatisticsDTO.PayrollProductId) || overtimeCompensationProducts.Contains(timePayrollStatisticsDTO.PayrollProductId))
                        {
                            certificateOfEmploymentEmployment.FilteredtransItems.Add(timePayrollStatisticsDTO);
                            certificateOfEmploymentEmployment.OtherFilteredtransItems.Add(timePayrollStatisticsDTO);
                            var salary = timePayrollStatisticsDTO.Quantity != 0 ? decimal.Round(decimal.Multiply(60, decimal.Divide(timePayrollStatisticsDTO.Amount, timePayrollStatisticsDTO.Quantity)), 2) : 0;

                            if (overtimeHourlySalary != 0 && salary != 0 && !SameAmount(overtimeHourlySalary, salary))
                                overTimeOrAddedTimeHasDifferentHourlySalary = true;

                            overtimeHourlySalary = salary;
                            continue;
                        }

                        if (additionalTimeProducts.Contains(timePayrollStatisticsDTO.PayrollProductId))
                        {
                            certificateOfEmploymentEmployment.FilteredtransItems.Add(timePayrollStatisticsDTO);
                            certificateOfEmploymentEmployment.OtherFilteredtransItems.Add(timePayrollStatisticsDTO);
                            var salary = timePayrollStatisticsDTO.Quantity != 0 ? decimal.Round(decimal.Multiply(60, decimal.Divide(timePayrollStatisticsDTO.Amount, timePayrollStatisticsDTO.Quantity)), 2) : 0;

                            if (addedTimeHourlySalary != 0 && salary != 0 && !SameAmount(addedTimeHourlySalary, salary))
                                overTimeOrAddedTimeHasDifferentHourlySalary = true;

                            addedTimeHourlySalary = salary;
                            continue;
                        }
                        else if (accProducts.Contains(timePayrollStatisticsDTO.PayrollProductId))
                        {
                            certificateOfEmploymentEmployment.FilteredtransItems.Add(timePayrollStatisticsDTO);
                            continue;
                        }

                        if (timePayrollStatisticsDTO.IsBenefitAndNotInvert())
                        {
                            if (sendToArbetsgivarIntyg)
                                certificateOfEmploymentEmployment.OtherFilteredtransItems.Add(timePayrollStatisticsDTO);
                            continue;
                        }

                        if (vacationCompensationProducts.Contains(timePayrollStatisticsDTO.PayrollProductId) || vacationSalaryproducts.Contains(timePayrollStatisticsDTO.PayrollProductId))
                        {
                            certificateOfEmploymentEmployment.OtherFilteredtransItems.Add(timePayrollStatisticsDTO);
                            continue;
                        }
                        if (vacationProducts.Contains(timePayrollStatisticsDTO.PayrollProductId))
                        {
                            certificateOfEmploymentEmployment.OtherFilteredtransItems.Add(timePayrollStatisticsDTO);
                            continue;
                        }

                        if (illnessInconvenientSalaryIds.Contains(timePayrollStatisticsDTO.PayrollProductId))
                        {
                            certificateOfEmploymentEmployment.OtherFilteredtransItems.Add(timePayrollStatisticsDTO);
                            continue;
                        }

                    }
                    if (timePayrollStatisticsDTO.IsGrossSalary())
                    {
                        if (hourlySalaryProducts.Contains(timePayrollStatisticsDTO.PayrollProductId) || scheduledTimeProducts.Contains(timePayrollStatisticsDTO.PayrollProductId))
                            continue;

                        if (absenceProducts.Contains(timePayrollStatisticsDTO.PayrollProductId))
                            continue;

                        if (timePayrollStatisticsDTO.IsMonthlySalary())
                            continue;

                        certificateOfEmploymentEmployment.OtherFilteredtransItems.Add(timePayrollStatisticsDTO);

                    }

                }

                if (!overTimeOrAddedTimeHasDifferentHourlySalary)
                {
                    certificateOfEmploymentEmployment.OtherFilteredtransItems = certificateOfEmploymentEmployment.OtherFilteredtransItems.Where(w => !overtimeAdditionProducts.Contains(w.PayrollProductId) && !overtimeCompensationProducts.Contains(w.PayrollProductId)).ToList();
                    certificateOfEmploymentEmployment.OtherFilteredtransItems = certificateOfEmploymentEmployment.OtherFilteredtransItems.Where(w => !additionalTimeProducts.Contains(w.PayrollProductId)).ToList();
                }

                certificateOfEmploymentEmployment.OverTimeOrAddedTimeHasDifferentHourlySalary = overTimeOrAddedTimeHasDifferentHourlySalary;
                certificateOfEmploymentEmployment.OverTimePerHour = !overTimeOrAddedTimeHasDifferentHourlySalary ? overtimeHourlySalary : 0;
                certificateOfEmploymentEmployment.AddedTimePerHour = !overTimeOrAddedTimeHasDifferentHourlySalary ? addedTimeHourlySalary : 0;
                certificateOfEmploymentEmployment.HasOtherDisbursementTransactions = certificateOfEmploymentEmployment.OtherFilteredtransItems.Any();
                certificateOfEmploymentEmployment.HasVacationAddition = certificateOfEmploymentEmployment.HasOtherDisbursementTransactions && certificateOfEmploymentEmployment.OtherFilteredtransItems.Any(a => vacationAdditionProducts.Contains(a.PayrollProductId));

                #endregion

                AddRedDays(employee, certificateOfEmploymentEmployment);

                #region Group filteredtransItemslevel

                certificateOfEmploymentEmployment.FilteredtransItems = certificateOfEmploymentEmployment.FilteredtransItems.OrderBy(t => t.TimeBlockDate).ToList();
                TermGroup_TimeSchedulePlanningVisibleDays group = TermGroup_TimeSchedulePlanningVisibleDays.Month;
                List<IGrouping<string, TimePayrollStatisticsDTO>> groupedTrans = null;

                if (_selectionSpecial != null && _selectionSpecial.ToLower().Contains("#grouponday#"))
                {
                    group = TermGroup_TimeSchedulePlanningVisibleDays.Day;
                    groupedTrans = certificateOfEmploymentEmployment.FilteredtransItems.GroupBy(t => t.TimeBlockDate.Date.ToString()).ToList();
                }
                else if (_selectionSpecial != null && _selectionSpecial.ToLower().Contains("#grouponweek#"))
                {
                    group = TermGroup_TimeSchedulePlanningVisibleDays.Week;
                    groupedTrans = certificateOfEmploymentEmployment.FilteredtransItems.GroupBy(t => t.TimeBlockDate.Year.ToString() + CalendarUtility.GetWeekNr(t.TimeBlockDate).ToString()).ToList();
                }
                else
                {
                    groupedTrans = certificateOfEmploymentEmployment.FilteredtransItems.GroupBy(t => t.TimeBlockDate.Year.ToString() + t.TimeBlockDate.Month.ToString()).ToList();
                }

                if (groupedTrans.Any())
                {
                    foreach (var intervalGroup in groupedTrans)
                    {
                        #region GroupTrans Element

                        var workSum = intervalGroup.Where(trans =>
                                      hourlySalaryProducts.Contains(trans.PayrollProductId) ||
                                      scheduledTimeProducts.Contains(trans.PayrollProductId) ||
                                      vacationProducts.Contains(trans.PayrollProductId) ||
                                      permissionProducts.Contains(trans.PayrollProductId) ||
                                      accProducts.Contains(trans.PayrollProductId)).Sum(i => i.Quantity);
                        var absenceSum = intervalGroup.Where(trans => !trans.IsCentrounding && selectedAbsenceProducts.Contains(trans.PayrollProductId)).Sum(i => i.Quantity);
                        var overTimeSum = intervalGroup.Where(trans => overtimeAdditionProducts.Contains(trans.PayrollProductId)
                                      || overtimeCompensationProducts.Contains(trans.PayrollProductId)).Sum(i => i.Quantity);
                        var addedTimeSum = intervalGroup.Where(trans => additionalTimeProducts.Contains(trans.PayrollProductId)).Sum(i => i.Quantity);

                        //Remove red days from workSum if absence is present on date.
                        foreach (var ledigDag in intervalGroup.Where(a => a.SysPayrollTypeLevel3Name == "Ledig dag"))
                        {
                            if (intervalGroup.Any(trans => trans.TimeBlockDate == ledigDag.TimeBlockDate && selectedAbsenceProducts.Contains(trans.PayrollProductId)))
                            {
                                workSum -= ledigDag.Quantity;
                                ledigDag.Quantity = 0;
                            }
                        }

                        var groupTrans = new GroupTrans()
                        {
                            Type = Enum.GetName(typeof(TermGroup_TimeSchedulePlanningVisibleDays), group),
                            DateFrom = CalendarUtility.GetBeginningOfMonth(intervalGroup.Min(m => m.TimeBlockDate)),
                            WorkSum = workSum,
                            AbsenceSum = absenceSum,
                            OverTimeSum = overTimeSum,
                            AddedTimeSum = addedTimeSum
                        };

                        #endregion

                        #region Transactions

                        if (intervalGroup.Any())
                        {
                            foreach (var trans in intervalGroup)
                            {
                                groupTrans.FilteredtransItems.Add(trans);
                            }
                        }

                        #endregion

                        certificateOfEmploymentEmployment.GroupTrans.Add(groupTrans);

                    }
                }

                #endregion

                #region Group filteredOtherDisbursementTransItems

                var otherFilteredtransItems = certificateOfEmploymentEmployment.OtherFilteredtransItems.OrderBy(t => t.TimeBlockDate).ToList();
                TermGroup_TimeSchedulePlanningVisibleDays groupOtherDisbursementTransItems = TermGroup_TimeSchedulePlanningVisibleDays.Month;
                List<IGrouping<string, TimePayrollStatisticsDTO>> groupedTransOtherDisbursementTransItems = null;

                if (_selectionSpecial != null && _selectionSpecial.ToLower().Contains("#grouponday#"))
                {
                    groupOtherDisbursementTransItems = TermGroup_TimeSchedulePlanningVisibleDays.Day;
                    groupedTransOtherDisbursementTransItems = otherFilteredtransItems.GroupBy(t => t.TimeBlockDate.Date.ToString() + "#" + t.PayrollProductName).ToList();

                }
                else if (_selectionSpecial != null && _selectionSpecial.ToLower().Contains("#grouponweek#"))
                {
                    groupOtherDisbursementTransItems = TermGroup_TimeSchedulePlanningVisibleDays.Week;
                    groupedTransOtherDisbursementTransItems = otherFilteredtransItems.GroupBy(t => t.TimeBlockDate.Year.ToString() + CalendarUtility.GetWeekNr(t.TimeBlockDate).ToString() + "#" + t.PayrollProductName).ToList();
                }
                else
                {
                    groupedTransOtherDisbursementTransItems = otherFilteredtransItems.GroupBy(t => t.TimeBlockDate.Year.ToString() + t.TimeBlockDate.Month.ToString() + "#" + t.PayrollProductName).ToList();
                }


                if (groupedTransOtherDisbursementTransItems.Count > 0)
                {
                    List<OtherGroupTrans> otherGroupTranses = new List<OtherGroupTrans>();
                    foreach (var intervalGroup in groupedTransOtherDisbursementTransItems)
                    {
                        foreach (var productNumberGroup in intervalGroup.GroupBy(i => i.PayrollProductNumber).ToList())
                        {
                            #region GroupOtherTrans element

                            var productGroup = productNumberGroup.OrderBy(p => p.TimeBlockDate).ToList();
                            var amount = productGroup.Sum(p => p.Amount);
                            var quantity = productGroup.Sum(p => p.Quantity);
                            bool isDutyOrStandby = productGroup.Any(trans => trans.IsGrossSalaryStandby());
                            var days = productGroup.GroupBy(trans => trans.TimeBlockDate).Distinct().Count();
                            OtherPayrollType otherPayrollType = OtherPayrollType.Inget;
                            var payrollProductId = productGroup.FirstOrDefault().PayrollProductId;

                            if (overtimeAdditionProducts.Contains(payrollProductId))
                                otherPayrollType = OtherPayrollType.Overtidstillagg;
                            else if (additionalTimeProducts.Contains(payrollProductId))
                                otherPayrollType = OtherPayrollType.Mertid;
                            else if (dutyProducts.Contains(payrollProductId))
                                otherPayrollType = OtherPayrollType.Jour;
                            else if (illnessProducts.Contains(payrollProductId))
                                otherPayrollType = OtherPayrollType.Sjuklon;
                            else if (vacationSalaryproducts.Contains(payrollProductId) || vacationCompensationDirectPayProducts.Contains(payrollProductId))
                                otherPayrollType = OtherPayrollType.Semesterersattning;
                            else if (vacationProducts.Contains(payrollProductId))
                            {
                                otherPayrollType = OtherPayrollType.Semesterersattning;
                                quantity = 0;
                            }
                            else if (illnesssQualifyingDayProducts.Contains(payrollProductId))
                            {
                                otherPayrollType = OtherPayrollType.Sjuklon;
                                quantity = 0;
                            }
                            else if (obProducts.Contains(payrollProductId))
                                otherPayrollType = OtherPayrollType.OB;

                            if (certificateOfEmploymentEmployment.HasVacationAddition && otherPayrollType == OtherPayrollType.Semesterersattning)
                                continue;

                            if (certificateOfEmploymentEmployment.HasVacationAddition && vacationAdditionProducts.Contains(payrollProductId))
                                otherPayrollType = OtherPayrollType.Semesterersattning;

                            var otherGroupTrans = new OtherGroupTrans()
                            {
                                Type = Enum.GetName(typeof(TermGroup_TimeSchedulePlanningVisibleDays), groupOtherDisbursementTransItems),
                                DateFrom = CalendarUtility.GetBeginningOfMonth(intervalGroup.Min(m => m.TimeBlockDate)),
                                PayrollProductNumber = productGroup.FirstOrDefault().PayrollProductNumber,
                                Name = productGroup.FirstOrDefault().PayrollProductName,
                                Days = days,
                                Amount = amount,
                                Quantity = quantity,
                                IsDutyOrStandby = isDutyOrStandby,
                                OtherPayrollType = otherPayrollType,
                                FilteredtransItems = productNumberGroup.ToList()
                            };

                            if (otherGroupTrans.OtherPayrollType == OtherPayrollType.Jour || otherGroupTrans.OtherPayrollType == OtherPayrollType.Sjuklon)
                                otherGroupTrans.ShowHours = true;


                            #endregion                 

                            otherGroupTranses.Add(otherGroupTrans);
                        }

                    }

                    #region Group on OtherPayrollType aswell   


                    foreach (var other in otherGroupTranses.Where(w => w.OtherPayrollType != OtherPayrollType.Inget).GroupBy(g => $"{g.DateFrom.Year}#{g.DateFrom.Month}#{g.OtherPayrollType}#{g.ShowHours}").OrderByDescending(o => o.Key))
                    {
                        var first = other.First();
                        var row = new OtherGroupTrans()
                        {
                            Type = Enum.GetName(typeof(TermGroup_TimeSchedulePlanningVisibleDays), groupOtherDisbursementTransItems),
                            DateFrom = CalendarUtility.GetBeginningOfMonth(first.DateFrom),
                            PayrollProductNumber = "*",
                            Name = OtherPayrollTypesLoneTyperName(first.OtherPayrollType),
                            Days = other.Sum(s => s.Days),
                            Amount = other.Sum(s => s.Amount),
                            Quantity = other.Where(w => w.Amount > 0).Sum(s => s.Quantity) + other.Where(w => w.Amount < 0).Sum(s => -Math.Abs(s.Quantity)),
                            IsDutyOrStandby = first.IsDutyOrStandby,
                            OtherPayrollType = first.OtherPayrollType,
                            ShowHours = first.ShowHours,
                            FilteredtransItems = other.SelectMany(s => s.FilteredtransItems).ToList()
                        };

                        if (row.Quantity != 0 || row.Amount != 0)
                            certificateOfEmploymentEmployment.OtherGroupTrans.Add(row);
                    }

                    foreach (var other in otherGroupTranses.Where(w => w.OtherPayrollType == OtherPayrollType.Inget).OrderByDescending(o => o.DateFrom))
                    {
                        certificateOfEmploymentEmployment.OtherGroupTrans.Add(other);
                    }

                    #endregion
                }

                #endregion
            }
        }

        private bool SameAmount(decimal amount, decimal otherAmount)
        {
            var diff = decimal.Subtract(amount, otherAmount);
            return diff > new decimal(-1) && diff < new decimal(1);
        }

        private void AddScheduleInformation(Employee employee, CertificateOfEmploymentEmployment certificateOfEmploymentEmployment)
        {
            var templates = TimeScheduleManager.GetTimeScheduleTemplateHeadsForEmployee(ReportResult.ActorCompanyId, employee.EmployeeId, CalendarUtility.DATETIME_DEFAULT, _selectionDate);

            if (templates.Any())
            {
                var last = templates.OrderBy(o => o.StartDate).Last();
                if (last.NoOfDays > 7)
                    certificateOfEmploymentEmployment.VariableWeekTime = true;
            }
        }

        private void AddLeaveOfAbsenceInformation(Employee employee, CertificateOfEmploymentEmployment certificateOfEmploymentEmployment)
        {

        }

        public string OtherPayrollTypesLoneTyperName(OtherPayrollType otherPayrollType)
        {
            switch (otherPayrollType)
            {
                case OtherPayrollType.Inget:
                    return string.Empty;
                case OtherPayrollType.Sjuklon:
                    return "Sjuklön";
                case OtherPayrollType.Overtidstillagg:
                    return "Övertidstillägg";
                case OtherPayrollType.Mertid:
                    return "Mertid";
                case OtherPayrollType.Jour:
                    return "Jour m.m";
                case OtherPayrollType.OB:
                    return "OB tillägg";
                case OtherPayrollType.AndraFormaner:
                    return "Andra förmåner";
                case OtherPayrollType.Semesterersattning:
                    return "Semesterersättning";
                case OtherPayrollType.Lonetillagg:
                    return "Lönetillägg";
                default:
                    return string.Empty;
            }
        }
    }


    public class CertificateOfEmploymentEmployee
    {
        public Employee Employee { get; set; }
        public string EmployeeNr { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string SocialSec { get; set; }
        public string Note { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public List<CertificateOfEmploymentEmployment> CertificateOfEmploymentMergedEmployments { get; set; }
    }


    public class CertificateOfEmploymentEmployment
    {
        public CertificateOfEmploymentEmployment()
        {
            FilteredtransItems = new List<TimePayrollStatisticsDTO>();
            OtherFilteredtransItems = new List<TimePayrollStatisticsDTO>();
            GroupTrans = new List<GroupTrans>();
            OtherGroupTrans = new List<OtherGroupTrans>();
        }
        public EmploymentDTO EmploymentDTO { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public string PositionCode { get; set; }
        public string PositionName { get; set; }
        public TermGroup_EmploymentType EmploymentType { get; set; }
        public string EmploymentTypeName { get; set; }
        public int WorkTime { get; set; }
        public int WorkTimeEmployeeGroup { get; set; }
        public decimal WorkPercent { get; set; }
        public int EndReason { get; set; }
        public string EndReasonName { get; set; }
        public TermGroup_PayrollExportSalaryType SalaryType { get; set; }
        public int PayrollYear { get; set; }
        public decimal PayrollAmount { get; set; }
        public decimal OverTimePerHour { get; set; }
        public decimal AddedTimePerHour { get; set; }
        public bool HasOtherDisbursementTransactions { get; set; }
        public bool VariableWeekTime { get; set; }
        public bool HasLeaveOfAbsense { get; set; }
        public DateTime? StartLeaveOfAbsence { get; set; }
        public DateTime? StopLeaveOfAbsence { get; set; }
        public EmployeeGroup EmployeeGroup { get; set; }
        public List<GroupTrans> GroupTrans { get; set; }
        public List<OtherGroupTrans> OtherGroupTrans { get; set; }
        public List<TimePayrollStatisticsDTO> FilteredtransItems { get; set; }
        public List<TimePayrollStatisticsDTO> OtherFilteredtransItems { get; set; }
        public bool OverTimeOrAddedTimeHasDifferentHourlySalary { get; set; }
        public bool HasVacationAddition { get; set; }
        public int EndReasonCode { get; internal set; }
    }

    public class GroupTrans
    {
        public GroupTrans()
        {
            FilteredtransItems = new List<TimePayrollStatisticsDTO>();
        }
        public string Type { get; set; }
        public DateTime DateFrom { get; set; }
        public decimal WorkSum { get; set; }
        public decimal AbsenceSum { get; set; }
        public decimal OverTimeSum { get; set; }
        public decimal AddedTimeSum { get; set; }
        public List<TimePayrollStatisticsDTO> FilteredtransItems { get; set; }
    }

    public class OtherGroupTrans
    {
        public OtherGroupTrans()
        {
            FilteredtransItems = new List<TimePayrollStatisticsDTO>();
        }
        public string Type { get; set; }

        public string PayrollProductNumber { get; set; }

        public string Name { get; set; }
        public DateTime DateFrom { get; set; }
        public int Days { get; set; }
        public decimal Amount { get; set; }
        public decimal Quantity { get; set; }
        public bool IsDutyOrStandby { get; set; }
        public bool ShowHours { get; set; }
        public OtherPayrollType OtherPayrollType { get; set; }
        public List<TimePayrollStatisticsDTO> FilteredtransItems { get; set; }
    }



    public enum OtherPayrollType
    {
        Inget = 0,
        Sjuklon = 1,
        Overtidstillagg = 2,
        Mertid = 3,
        Jour = 4,
        OB = 5,
        AndraFormaner = 6,
        Semesterersattning = 7,
        Lonetillagg = 8,
    }

}
