using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.Reporting;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.ExportFiles
{
    public class Kpa : ExportFilesBase
    {
        #region Ctor

        public Kpa(ParameterObject parameterObject, CreateReportResult reportResult) : base(parameterObject, reportResult)
        {
            this.reportResult = reportResult;
        }

        #endregion

        #region Public methods

        public string CreateFile(CompEntities entities)
        {
            #region Init

            #endregion

            #region Init repository

            PersonalDataEmployeeReportRepository personalDataRepository = new PersonalDataEmployeeReportRepository(parameterObject, reportResult);

            #endregion

            #region Prereq

            int year = 0;
            var company = CompanyManager.GetCompany(reportResult.ActorCompanyId);

            if (!TryGetIdsFromSelection(reportResult, out List<int> selectionTimePeriodIds, "periods"))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionTimePeriodIds, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return null;

            if (employees == null)
                employees = EmployeeManager.GetAllEmployeesByIds(entities, base.ActorCompanyId, selectionEmployeeIds, loadEmployment: true);

            StringBuilder sb = new StringBuilder();

            List<EmployeeKpa> employeeKPAs = CreateEmployeeKPAs(entities, selectionTimePeriodIds, year, employees);

            #endregion

            foreach (var employeeKPA in employeeKPAs)
            {
                #region Employee

                Employee employee = employees.FirstOrDefault(i => i.EmployeeId == employeeKPA.EmployeeId);
                if (employee == null)
                    continue;

                sb.Append(CreateFieldZero(employeeKPA.Transaktionskod.ToString(), 5));
                sb.Append(CreateFieldZero(employeeKPA.InkomstKalla.ToString(), 2));
                sb.Append(CreateFieldZero(employeeKPA.Kundnummer.ToString(), 6));
                sb.Append(CreateFieldZero(employeeKPA.Personnummer.ToString(), 10));
                sb.Append(CreateFieldZero(employeeKPA.InkomstAr.ToString(), 4));
                sb.Append(CreateFieldZero(employeeKPA.InkomstManad.ToString(), 2));
                sb.Append(CreateFieldZero(employeeKPA.Periodicitet.ToString(), 2));
                sb.Append(CreateFieldZero(employeeKPA.Inkomst.ToString(), 9));
                sb.Append(CreateFieldZero(employeeKPA.Tecken.ToString(), 1));
                sb.Append(CreateFieldZero(employeeKPA.Avtalsnummer.ToString(), 9));
                personalDataRepository.AddEmployeeSocialSec(employee);

                #endregion

                sb.AppendLine();
            }

            #region Create File

            var directory = Directory.CreateDirectory(ConfigSettings.SOE_SERVER_DIR_TEMP_REPORT_PHYSICAL + Guid.NewGuid().ToString());
            string fileName = IOUtil.FileNameSafe("KPA" + "_" + company.Name + "_" + DateTime.Now.ToString("yyyyMMddhhmmss"));
            string filePath = StringUtility.GetValidFilePath(directory.FullName) + fileName + ".txt";

            try
            {
                File.WriteAllText(filePath, sb.ToString(), Encoding.ASCII);
            }
            catch (Exception ex)
            {
                SysLogManager.AddSysLog(ex, log4net.Core.Level.Error);
            }

            #endregion

            #region Close repository

            personalDataRepository.GenerateLogs();

            #endregion

            return filePath;
        }

        public string CreateKPADirektFile(CompEntities entities)
        {
            #region Init

            #endregion

            #region Init repository

            PersonalDataEmployeeReportRepository personalDataRepository = new PersonalDataEmployeeReportRepository(parameterObject, reportResult);

            #endregion

            #region Prereq

            int year = 0;
            var company = CompanyManager.GetCompany(reportResult.ActorCompanyId);

            if (!TryGetIdsFromSelection(reportResult, out List<int> selectionTimePeriodIds, "periods"))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionTimePeriodIds, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return null;

            TryGetBoolFromSelection(reportResult, out bool setAsFinal, "setAsFinal");
            TryGetBoolFromSelection(reportResult, out bool onlyChangedEmployments, "onlyChangedEmployments");
            TryGetIdFromSelection(reportResult, out int? kpaAgreementType, "kpaAgreementType");

            if (employees == null)
                employees = EmployeeManager.GetAllEmployeesByIds(entities, base.ActorCompanyId, selectionEmployeeIds, loadEmployment: true);

            var direkt = CreateEmployeeKPADirekt(entities, reportResult.ActorCompanyId, reportResult.UserId, reportResult.RoleId, selectionTimePeriodIds, year, employees, onlyChangedEmployments, kpaAgreementType);
            var eventHistories = new List<EventHistoryDTO>();

            #endregion

            string output = direkt.OutPutFileString();

            if (setAsFinal && direkt.EventHistories.Any())
            {
                eventHistories.AddRange(direkt.EventHistories);
            }

            #region Create File

            var directory = Directory.CreateDirectory(ConfigSettings.SOE_SERVER_DIR_TEMP_REPORT_PHYSICAL + Guid.NewGuid().ToString());
            string fileName = IOUtil.FileNameSafe("KPA_Direkt" + "_" + company.Name + "_" + DateTime.Now.ToString("yyyyMMddhhmmss"));
            string filePath = StringUtility.GetValidFilePath(directory.FullName) + fileName + ".txt";

            try
            {
                File.WriteAllText(filePath, output, Encoding.GetEncoding("ISO-8859-1"));

                if (setAsFinal && eventHistories.Any())
                {
                    GeneralManager.SaveEventHistories(entities, eventHistories, reportResult.ActorCompanyId);
                }

            }
            catch (Exception ex)
            {
                SysLogManager.AddSysLog(ex, log4net.Core.Level.Error);
            }

            #endregion

            #region Close repository

            personalDataRepository.GenerateLogs();

            #endregion

            return filePath;
        }

        public List<EmployeeKpa> CreateEmployeeKPAs(CompEntities entities, List<int> selectionTimePeriodIds, int year, List<Employee> employees)
        {
            List<EmployeeKpa> employeeKPAs = new List<EmployeeKpa>();
            if (year == 0)
            {
                List<TimePeriod> timePeriods = TimePeriodManager.GetTimePeriods(selectionTimePeriodIds, reportResult.ActorCompanyId).Where(x => x.PayrollStopDate.HasValue).ToList();
                if (timePeriods.IsNullOrEmpty())
                    return employeeKPAs;

                year = timePeriods.OrderBy(x => x.PayrollStartDate).LastOrDefault().PayrollStopDate.Value.Year;
            }
            DateTime dateFrom = new DateTime(year, 1, 1);
            DateTime dateTo = CalendarUtility.GetEndOfYear(dateFrom);

            List<EmployeeGroup> employeeGroups = EmployeeManager.GetEmployeeGroups(entities, reportResult.ActorCompanyId, true, true, true, true);
            List<PayrollGroup> payrollGroups = PayrollManager.GetPayrollGroups(entities, reportResult.ActorCompanyId, true, true, loadSettings: true);
            List<PayrollPriceType> payrollPriceTypes = PayrollManager.GetPayrollPriceTypes(entities, reportResult.ActorCompanyId, null, false);

            var transactions = TimeTransactionManager.GetTimePayrollStatisticsSmallDTOs_new(entities, reportResult.ActorCompanyId, employees, selectionTimePeriodIds, setPensionCompany: true, ignoreAccounting: true);
            bool showSocialSec = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId);
            string avtalsnummer = SettingManager.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.PayrollExportKPAAgreementNumber, reportResult.UserId, reportResult.ActorCompanyId, 0);

            foreach (var employee in employees)
            {
                var employeeTransactions = transactions.Where(e => e.EmployeeId == employee.EmployeeId).ToList();

                List<int> skipTransactionIds = employeeTransactions.Where(p => !p.IsScheduleTransaction).Select(t => t.TransactionId).ToList();
                employeeTransactions.AddRange(TimeTransactionManager.GetPayrollStartValuesAsTimePayrollStatisticsSmallDTOs(employee, ActorCompanyId, dateFrom.Year, skipTransactionIds: skipTransactionIds, setPensionCompany: true));

                employeeTransactions = filterTransactions(employee, employeeTransactions);

                if (employeeTransactions.Count == 0)
                    continue;

                var firstEmployment = employee.Employment.GetFirstEmployment();
                var lastEmployment = employee.Employment.GetLastEmployment();

                DateTime? anstalldFrom = null;
                DateTime? anstalldTom = null;

                DateTime EmploymentStartDate = firstEmployment != null && firstEmployment.DateFrom.HasValue ? firstEmployment.DateFrom.Value : DateTime.MinValue;
                DateTime EmploymentEndDate = lastEmployment != null && lastEmployment.DateTo.HasValue ? lastEmployment.DateTo.Value : DateTime.MaxValue;

                if (EmploymentStartDate > dateFrom)
                    anstalldFrom = EmploymentStartDate;

                if (EmploymentEndDate < dateTo)
                    anstalldTom = EmploymentEndDate;


                var transactionsGroupOnMonth = employeeTransactions.GroupBy(t => t.KPAAgreementNumber + '#' + t.PaymentDate.Value.Year.ToString() + t.PaymentDate.Value.Month.ToString());

                foreach (var monthGroup in transactionsGroupOnMonth)
                {
                    int inkomstAr = monthGroup.FirstOrDefault().PaymentDate.Value.Year;
                    string month = monthGroup.FirstOrDefault().PaymentDate.Value.Month.ToString();

                    var kpaAgreementNumber = monthGroup.LastOrDefault().KPAAgreementNumber;

                    if (month.Length == 1)
                        month = "0" + month;

                    EmployeeKpa employeeKPA = new EmployeeKpa()
                    {
                        Transaktionskod = "E0203",
                        EmployeeId = employee.EmployeeId,
                        InkomstAr = inkomstAr,
                        InkomstManad = month,
                        Avtalsnummer = !string.IsNullOrEmpty(kpaAgreementNumber) ? kpaAgreementNumber : string.Empty,
                        Personnummer = showSocialSec ? StringUtility.SocialSecYYMMDDXXXX(employee.SocialSec) : StringUtility.SocialSecYYMMDD_Dash_Stars(employee.SocialSec),
                        Periodicitet = "01",
                        Inkomst = Convert.ToInt32(monthGroup.Sum(t => t.Amount)),
                        InkomstKalla = "01",
                        Tecken = "+",
                        Kundnummer = avtalsnummer
                    };

                    if (employeeKPA.Inkomst != 0)
                    {
                        employeeKPA.Transactions = new List<EmployeeKpaTransactionDTO>();

                        foreach (var product in monthGroup.GroupBy(m => m.PayrollProductId))
                        {
                            EmployeeKpaTransactionDTO EmployeeKPATransactionDTO = new EmployeeKpaTransactionDTO()
                            {
                                Type = product.FirstOrDefault().SysPayrollTypeLevel1Name + "#" + product.FirstOrDefault().SysPayrollTypeLevel2Name + "#" + product.FirstOrDefault().SysPayrollTypeLevel3Name + "#" + product.FirstOrDefault().SysPayrollTypeLevel4Name,
                                PayrollProductNumber = product.FirstOrDefault().PayrollProductNumber,
                                Name = product.FirstOrDefault().PayrollProductName,
                                Quantity = product.Sum(p => p.Quantity),
                                Amount = product.Sum(p => p.Amount),
                            };

                            employeeKPA.Transactions.Add(EmployeeKPATransactionDTO);
                        }

                        employeeKPAs.Add(employeeKPA);
                    }
                }
            }

            return employeeKPAs;
        }

        public KPADirekt CreateEmployeeKPADirekt(CompEntities entities, int actorCompanyId, int userId, int roleId, List<int> selectionTimePeriodIds, int year, List<Employee> employees, bool onlyChangedEmployments, int? kpaAgreementTypeSelection)
        {
            KPADirekt direkt = new KPADirekt();

            List<TimePeriod> timePeriods = TimePeriodManager.GetTimePeriods(selectionTimePeriodIds, actorCompanyId).Where(x => x.PayrollStopDate.HasValue).ToList();
            if (!timePeriods.Any())
                return direkt;

            year = timePeriods.OrderBy(x => x.PayrollStartDate).LastOrDefault().PayrollStopDate.Value.Year;
            var firstDate = timePeriods.OrderBy(x => x.PayrollStartDate).FirstOrDefault().PayrollStartDate.Value;

            direkt.Rapporteringsar = year;
            DateTime dateFrom = new DateTime(year, 1, 1);
            DateTime yearStartDate = dateFrom;
            DateTime dateTo = CalendarUtility.GetEndOfYear(dateFrom);
            DateTime halfYearStart = new DateTime(year, 7, 1);

            if (kpaAgreementTypeSelection.HasValue && kpaAgreementTypeSelection == (int)KpaAgreementType.AKAP_KR && firstDate == halfYearStart)
                dateFrom = halfYearStart;

            var transactions = TimeTransactionManager.GetTimePayrollStatisticsSmallDTOs_new(entities, actorCompanyId, employees, selectionTimePeriodIds, setPensionCompany: true, ignoreAccounting: true);
            bool showSocialSec = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec, Permission.Readonly, roleId, actorCompanyId);
            string avtalsnummer = SettingManager.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.PayrollExportKPAAgreementNumber, 0, actorCompanyId, 0);
            string forvaltningsnummer = SettingManager.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.PayrollExportKPAManagementNumber, 0, actorCompanyId, 0);

            foreach (var employee in employees)
            {
                var employeeTransactions = transactions.Where(e => e.EmployeeId == employee.EmployeeId).ToList();

                List<int> skipTransactionIds = employeeTransactions.Where(p => !p.IsScheduleTransaction).Select(t => t.TransactionId).ToList();
                employeeTransactions.AddRange(TimeTransactionManager.GetPayrollStartValuesAsTimePayrollStatisticsSmallDTOs(employee, ActorCompanyId, dateFrom.Year, skipTransactionIds: skipTransactionIds, setPensionCompany: true));

                employeeTransactions = filterTransactions(employee, employeeTransactions);

                if (!employeeTransactions.Any())
                    continue;

                var firstEmployment = employee.Employment.GetFirstEmployment();
                var lastEmployment = employee.Employment.GetLastEmployment();

                DateTime anstalldFrom = firstEmployment != null && firstEmployment.DateFrom.HasValue ? firstEmployment.DateFrom.Value : DateTime.MinValue;
                DateTime? anstalldTom = lastEmployment != null && lastEmployment.DateTo.HasValue ? lastEmployment.DateTo.Value : DateTime.MaxValue;

                if (anstalldFrom < yearStartDate)
                    anstalldFrom = yearStartDate;

                if (anstalldTom > dateTo)
                    anstalldTom = null;

                List<EmploymentDTO> mergedEmployments = new List<EmploymentDTO>();

                List<EmploymentDTO> employments = employee.GetEmployments(anstalldFrom, anstalldTom ?? DateTime.Today.AddYears(10)).ToSplittedDTOs(includeEmployeeGroup: true, includePayrollGroup: true);
                if (employments.Count > 1)
                {
                    EmploymentDTO prevEmployment = null;
                    int numberOfEmploymentsHandled = 0;

                    foreach (var employment in employments)
                    {
                        KpaAgreementType ekpaAgreement = KpaAgreementType.Unknown;
                        if (employee.KPAAgreementType != 0)
                        {
                            ekpaAgreement = (KpaAgreementType)employee.KPAAgreementType;
                        }
                        else
                        {
                            PayrollGroup payrollGroup = employee.GetPayrollGroup(employment.DateFrom, payrollGroups: base.GetPayrollGroupsFromCache(entities, CacheConfig.Company(actorCompanyId)));
                            if (payrollGroup != null)
                            {
                                int? settingKPAAgreementType = payrollGroup?.PayrollGroupSetting.FirstOrDefault(f => f.Type == (int)PayrollGroupSettingType.KPAAgreementType)?.IntData;
                                if (!settingKPAAgreementType.IsNullOrEmpty())
                                    ekpaAgreement = (KpaAgreementType)settingKPAAgreementType;
                            }
                        }
                        bool akap = ekpaAgreement == KpaAgreementType.AKAP_KL || ekpaAgreement == KpaAgreementType.AKAP_KR;

                        DateTime fran = employment.DateFrom.ToValueOrDefault();
                        if (prevEmployment != null)
                        {
                            employment.ExternalCode = GetAnstallningsKod(employee, employment.DateFrom ?? dateFrom, employment.DateTo ?? dateTo).ToString();
                            if (prevEmployment.ExternalCode == employment.ExternalCode)
                            {
                                if (prevEmployment.DateTo >= fran.AddDays(akap ? -2 : ekpaAgreement == KpaAgreementType.KAP_KL ? -30 : -1)) //NOSONAR
                                {
                                    prevEmployment.DateTo = employment.DateTo;
                                }
                            }
                            else
                            {
                                mergedEmployments.Add(prevEmployment.CloneDTO());
                                prevEmployment = employment;
                            }
                        }
                        else
                        {
                            employment.ExternalCode = GetAnstallningsKod(employee, employment.DateFrom ?? dateFrom, employment.DateTo ?? dateTo).ToString();
                            prevEmployment = employment;
                        }

                        numberOfEmploymentsHandled++;

                        if (employments.Count == numberOfEmploymentsHandled)
                        {
                            prevEmployment.UniqueId = Guid.NewGuid().ToString();
                            mergedEmployments.Add(prevEmployment);
                        }
                    }
                }

                EmployeeKpaDirekt employeeKPADirekt = new EmployeeKpaDirekt();
                employeeKPADirekt.EmployeeId = employee.EmployeeId;
                employeeKPADirekt.EmployeeNr = employee.EmployeeNr;
                employeeKPADirekt.Tillhorighet = employee.KPABelonging;
                employeeKPADirekt.Forvaltningsnummer = forvaltningsnummer;
                employeeKPADirekt.Kundnummer = avtalsnummer;
                employeeKPADirekt.Personnamn = employee.Name;
                employeeKPADirekt.Personnummer = showSocialSec ? StringUtility.SocialSecYYMMDDXXXX(employee.SocialSec) : StringUtility.SocialSecYYMMDD_Dash_Stars(employee.SocialSec);
                employeeKPADirekt.Sekel = StringUtility.SocialSecYYYYMMDDXXXX(employee.SocialSec).Substring(0, 2);
                direkt.EmployeeKPADirekts.Add(employeeKPADirekt);
                var grouped = employeeTransactions.OrderBy(o => o.Date).GroupBy(t => t.KPAAgreementNumber + '#' + t.PensionCompany + "#" + t.KPAAgreementType + "#ET:" + (t.EmploymentTypeDTO?.GetEmploymentType().ToString() ?? "U") + "#" + t.GetUniqueId(mergedEmployments));

                //if mergedEmployments more than on group and the first group contains t.EmploymentTypeDTO?.GetEmploymentType() == 1, then we should merge the next group with the first group
                if (grouped.Count() > 1 && employeeTransactions.Any(a => a.EmploymentTypeDTO?.GetEmploymentType() == 1))
                {
                    var propationaryGroup = grouped.FirstOrDefault(f => f.Key.Contains("#ET:1#"));
                    if (propationaryGroup != null)
                    {
                        var nextGroup = grouped.FirstOrDefault(f => f != propationaryGroup);
                        var propationaryGroupFound = false;
                        foreach (var item in grouped)
                        {
                            if (item.Key == propationaryGroup.Key)
                            {
                                propationaryGroupFound = true;
                                continue;
                            }
                            if (propationaryGroupFound)
                            {
                                nextGroup = item;
                                break;
                            }
                        }

                        if (nextGroup != null)
                        {
                            foreach (var item in propationaryGroup)
                            {
                                item.EmploymentTypeDTO = nextGroup.FirstOrDefault().EmploymentTypeDTO;
                            }
                        }

                        grouped = employeeTransactions.OrderBy(o => o.Date).GroupBy(t => t.KPAAgreementNumber + '#' + t.PensionCompany + "#" + t.KPAAgreementType + "#" + t.EmploymentTypeDTO?.GetEmploymentType() + "#" + t.GetUniqueId(mergedEmployments));
                    }
                }

                int totalLon = Convert.ToInt32(employeeTransactions.Sum(s => s.Amount));
                int lon = 0;
                decimal kPAPercentAboveBaseAmount = 0;
                decimal kPAPercentUnderBaseAmount = 0;
                bool skipEmployee = false;
                KpaAgreementType kpaAgreement = KpaAgreementType.Unknown;
                int count = 0;

                foreach (var group in grouped)
                {
                    count++;
                    skipEmployee = false;
                    var employment = employee.GetEmployment(group.FirstOrDefault().Date);
                    var payrollGroup = employment?.GetPayrollGroup(group.FirstOrDefault().Date) ?? null;

                    if (employment == null)
                        employment = employee.GetLastEmployment(group.FirstOrDefault().Date);

                    if (employment == null && group.FirstOrDefault().PaymentDate.HasValue)
                        employment = employee.GetNextEmployment(group.FirstOrDefault().PaymentDate.Value);

                    if (employment == null && group.FirstOrDefault().PaymentDate.HasValue)
                        employment = employee.GetNearestEmployment(group.FirstOrDefault().PaymentDate.Value);

                    if (employment == null)
                        throw new Exception($"Failed to find employment on employee {employee.NumberAndName}");

                    if (grouped.Count() > 1)
                    {
                        anstalldFrom = employment.DateFrom.HasValue ? (employment.DateFrom.Value < anstalldFrom ? anstalldFrom : employment.DateFrom.Value) : DateTime.MinValue; 
                        anstalldTom = employment.GetEndDate() ?? DateTime.MaxValue;
                    }

                    if (employee.KPAAgreementType != 0)
                    {
                        kpaAgreement = (KpaAgreementType)employee.KPAAgreementType;
                    }
                    else
                    {
                        var settingKPAAgreementType = payrollGroup?.PayrollGroupSetting.FirstOrDefault(f => f.Type == (int)PayrollGroupSettingType.KPAAgreementType)?.IntData;

                        if (!settingKPAAgreementType.IsNullOrEmpty())
                            kpaAgreement = (KpaAgreementType)settingKPAAgreementType;
                    }

                    if (kpaAgreementTypeSelection.HasValue && kpaAgreementTypeSelection != (int)kpaAgreement)
                    {
                        skipEmployee = true;
                        continue;
                    }

                    bool KAP = kpaAgreement == KpaAgreementType.KAP_KL || kpaAgreement == KpaAgreementType.AKAP_KL || kpaAgreement == KpaAgreementType.AKAP_KR;

                    var settingFireman = payrollGroup?.PayrollGroupSetting.FirstOrDefault(f => f.Type == (int)PayrollGroupSettingType.KPAFireman)?.BoolData;
                    bool fireman = settingFireman != null && settingFireman.Value;

                    var settingKPARetirementAge = payrollGroup?.PayrollGroupSetting.FirstOrDefault(f => f.Type == (int)PayrollGroupSettingType.KPARetirementAge)?.IntData;
                    int retirementAge = settingKPARetirementAge == null || settingKPARetirementAge.Value == 0 ? 65 : settingKPARetirementAge.Value;

                    var settingKPAPercentAboveBaseAmount = payrollGroup?.PayrollGroupSetting.FirstOrDefault(f => f.Type == (int)PayrollGroupSettingType.KPAPercentAboveBaseAmount)?.DecimalData;
                    kPAPercentAboveBaseAmount = settingKPAPercentAboveBaseAmount == null || settingKPAPercentAboveBaseAmount.Value == 0 ? 0 : settingKPAPercentAboveBaseAmount.Value;

                    var settingKPAPercentUnderBaseAmount = payrollGroup?.PayrollGroupSetting.FirstOrDefault(f => f.Type == (int)PayrollGroupSettingType.KPAPercentBelowBaseAmount)?.DecimalData;
                    kPAPercentUnderBaseAmount = settingKPAPercentUnderBaseAmount == null || settingKPAPercentUnderBaseAmount.Value == 0 ? 0 : settingKPAPercentUnderBaseAmount.Value;

                    if (KAP)
                    {
                        var history = GeneralManager.GetLastEventHistory(entities, TermGroup_EventHistoryType.KPA_EmployeeKpaDirektAnstallningKap, employee.EmployeeId, Company.ActorCompanyId);
                        payrollGroup = employment.GetPayrollGroup(anstalldFrom);
                        if (payrollGroup != null)
                        {
                            if (!payrollGroup.PayrollGroupSetting.IsLoaded)
                                payrollGroup.PayrollGroupSetting.Load();

                            if (payrollGroup.PayrollGroupSetting != null && payrollGroup.PayrollGroupSetting.Any())
                            {
                                var setting = payrollGroup.PayrollGroupSetting.FirstOrDefault(p => p.Type == (int)PayrollGroupSettingType.KPADirektSalaryFormula);

                                if (setting != null && setting.IntData.HasValue && setting.IntData.Value != 0)
                                {
                                    PayrollPriceFormulaResultDTO result = PayrollManager.EvaluatePayrollPriceFormula(entities, actorCompanyId, employee, employment, null, anstalldTom.HasValue ? anstalldTom.Value : dateTo, null, null, setting.IntData.Value);

                                    if (result != null)
                                    {
                                        lon = Convert.ToInt32(result.Amount);

                                        var historyLon = GeneralManager.GetLastEventHistory(entities, TermGroup_EventHistoryType.KPA_EmployeeKpaOverenskommenLon, employee.EmployeeId, Company.ActorCompanyId);

                                        if (historyLon != null && historyLon.DecimalData == lon)
                                            lon = 0;
                                    }
                                }
                            }
                        }

                        if (!onlyChangedEmployments || history == null || history.DateData != anstalldFrom)
                        {
                            EmployeeKpaDirektAnstallningKap anstallningKAP = new EmployeeKpaDirektAnstallningKap(employeeKPADirekt);
                            anstallningKAP.FromDatum = anstalldFrom;
                            anstallningKAP.TomDatum = anstalldTom;
                            anstallningKAP.AnstallningsKod = GetAnstallningsKod(employee, anstalldFrom, anstalldTom.HasValue ? anstalldTom.Value : dateTo);
                            anstallningKAP.Anstallningsnummer = employeeKPADirekt.EmployeeNr;
                            anstallningKAP.Brandman = fireman;
                            anstallningKAP.LonVidAnstallningensTilltrade = anstallningKAP.AnstallningsKod != 4 ? lon : 0;
                            anstallningKAP.PensionAlder = retirementAge;
                            anstallningKAP.KPAAgrementType = (int)kpaAgreement;
                            //if (anstallningKAP.AnstallningsKod != 4)
                            //    anstallningKAP.AntalArbetsdagarForTimAnstalld = employeeTransactions.Where(w => w.IsHourlySalary() && w.IsSupplementChargeBasis() && !w.IsScheduleTransaction).GroupBy(s => s.Date).Count();
                            employeeKPADirekt.EmployeeKPADirektAnstallningKAPs.Add(anstallningKAP);
                            employeeKPADirekt.EventHistories.Add(new EventHistoryDTO(Company.ActorCompanyId, TermGroup_EventHistoryType.KPA_EmployeeKpaDirektAnstallningKap, SoeEntityType.Employee, employee.EmployeeId, userId: UserId, integerValue: lon, booleanValue: true, dateValue: anstalldFrom));
                        }
                    }
                    else
                    {
                        var history = GeneralManager.GetLastEventHistory(entities, TermGroup_EventHistoryType.KPA_EmployeeKpaDirektAnstallningPfa, employee.EmployeeId, Company.ActorCompanyId);
                        if (!onlyChangedEmployments || history == null || history.DateData != dateFrom)
                        {
                            EmployeeKpaDirektAnstallningPfa anstallningPFA = new EmployeeKpaDirektAnstallningPfa(employeeKPADirekt);
                            anstallningPFA.FromDatum = anstalldFrom;
                            anstallningPFA.TomDatum = anstalldTom;
                            anstallningPFA.Anstallningsnummer = employeeKPADirekt.EmployeeNr;
                            anstallningPFA.AvslutsKod = GetAvslutskod(employee);

                            if (anstallningPFA.AvslutsKod == "UD")
                                anstallningPFA.AntalArbetsdagarForTimanstalld = employeeTransactions.Where(w => w.IsHourlySalary() && w.IsSupplementChargeBasis() && !w.IsScheduleTransaction).GroupBy(s => s.Date).Count();

                            anstallningPFA.PensionAvtal = "2";
                            anstallningPFA.Brandman = fireman;
                            anstallningPFA.PensionAlder = retirementAge;
                            employeeKPADirekt.EmployeeKPADirektAnstallningPFAs.Add(anstallningPFA);
                            employeeKPADirekt.EventHistories.Add(new EventHistoryDTO(Company.ActorCompanyId, TermGroup_EventHistoryType.KPA_EmployeeKpaDirektAnstallningPfa, SoeEntityType.Employee, employee.EmployeeId, userId: UserId, booleanValue: true, dateValue: anstalldFrom));
                        }
                    }
                }

                if (!onlyChangedEmployments)
                {
                    employeeKPADirekt.EmployeeKPADirektArsbelopp = new EmployeeKpaDirektArsbelopp(employeeKPADirekt)
                    {
                        PensionsgrundandeLon = totalLon,
                        ManadslonAKAP_KL = kpaAgreement == KpaAgreementType.AKAP_KL || kpaAgreement == KpaAgreementType.AKAP_KR ? lon : 0,
                        Ar = anstalldFrom.Year,
                        ProcentsatsForAvgiftsbaseradAlderspensionOver7_5Inkomstbasbelopp = kPAPercentAboveBaseAmount,
                        ProcentsatsForAvgiftsbaseradAlderspensionUnder7_5Inkomstbasbelopp = kPAPercentUnderBaseAmount,
                        VilketAvtalAnställningenTillhor = GetVilketAvtalAnställningenTillhor(employee, kpaAgreement),
                        FromDatum = anstalldFrom.Year <= dateFrom.Year ? dateFrom : anstalldFrom,
                        TomDatum = anstalldTom.HasValue && anstalldTom.Value < dateTo ? anstalldTom : CalendarUtility.GetEndOfMonth(timePeriods.OrderBy(x => x.PayrollStopDate).LastOrDefault().PayrollStopDate)
                    };

                    if (lon != 0)
                        employeeKPADirekt.EventHistories.Add(new EventHistoryDTO(Company.ActorCompanyId, TermGroup_EventHistoryType.KPA_EmployeeKpaOverenskommenLon, SoeEntityType.Employee, employee.EmployeeId, userId: UserId, decimalValue: lon));
                }
                if (GetAnstallningsKod(employee, anstalldFrom, anstalldTom.HasValue ? anstalldTom.Value : dateTo) != 4)
                {
                    List<Tuple<DateTime?, DateTime>> intervals = new List<Tuple<DateTime?, DateTime>>();
                    DateTime currentDate = anstalldFrom;
                    DateTime? startInterval = null;

                    while (currentDate <= dateTo)
                    {
                        var ondate = employeeTransactions.Where(a => a.Date == currentDate).ToList();
                        var absenceTransactions = !ondate.Any(a => a.IsWorkTime()) ? ondate.Where(w => w.IsAbsence()).ToList() : new List<TimePayrollStatisticsSmallDTO>();

                        if (absenceTransactions.Any())
                        {
                            if (startInterval == null)
                                startInterval = currentDate;
                        }
                        else if (startInterval.HasValue)
                        {
                            intervals.Add(Tuple.Create(startInterval, currentDate.AddDays(-1)));
                            startInterval = null;
                        }

                        if (currentDate == dateTo && startInterval.HasValue)
                        {
                            intervals.Add(Tuple.Create(startInterval, currentDate.AddDays(-1)));
                        }

                        currentDate = currentDate.AddDays(1);
                    }

                    foreach (var interval in intervals)
                    {
                        if (employeeKPADirekt.EmployeeKPADirektLedighetsuppgift == null)
                            employeeKPADirekt.EmployeeKPADirektLedighetsuppgift = new List<EmployeeKpaDirektLedighetsuppgift>();

                        employeeKPADirekt.EmployeeKPADirektLedighetsuppgift.Add(new EmployeeKpaDirektLedighetsuppgift(employeeKPADirekt)
                        {
                            FromDatum = interval.Item1.Value,
                            TomDatum = interval.Item2,
                        });
                    }
                }

                if (skipEmployee)
                    direkt.EmployeeKPADirekts.Remove(employeeKPADirekt);
            }

            return direkt;
        }


        #endregion

        #region Help-methods

        /// <summary>
        /// 1 = Tillsvidareanställning, gäller även alla behöriga lärare.
        /// 4 = Timavlönad
        /// 5 = Fiktiv anställning
        /// </summary>
        public int GetAnstallningsKod(Employee employee, DateTime fromDate, DateTime toDate)
        {
            TermGroup_PayrollExportSalaryType salaryType = EmployeeManager.GetEmployeeSalaryType(employee, fromDate, toDate);

            switch (salaryType)
            {
                case TermGroup_PayrollExportSalaryType.Monthly:
                case TermGroup_PayrollExportSalaryType.Weekly:
                    return 1;
                case TermGroup_PayrollExportSalaryType.Hourly:
                    return 4;
                default:
                    return 5;
            }
        }

        public string GetAvslutskod(Employee employee)
        {
            int? value = employee.KPAEndCode;

            if (!value.HasValue || value.Value == 0)
                return "  ";

            KpaEndCode kpaEndCode = (KpaEndCode)value.Value;

            switch (kpaEndCode)
            {
                case KpaEndCode.U1:
                    return "U1";
                case KpaEndCode.U3:
                    return "U3";
                case KpaEndCode.US:
                    return "US";
                case KpaEndCode.UD:
                    return "UD";
                default:
                    return "  ";
            }
        }

        private string GetVilketAvtalAnställningenTillhor(Employee employee, KpaAgreementType fallBack = KpaAgreementType.Unknown)
        {
            KpaAgreementType type = (KpaAgreementType)employee.KPAAgreementType;

            switch (type)
            {
                case KpaAgreementType.PFA01:
                    return 1.ToString();
                case KpaAgreementType.PFA98:
                    return 2.ToString();
                case KpaAgreementType.KAP_KL:
                    return 6.ToString();
                case KpaAgreementType.AKAP_KL:
                    return 8.ToString();
                case KpaAgreementType.AKAP_KR:
                    return 13.ToString();
            }

            return ((int)fallBack).ToString();
        }

        private string CreateFieldZero(string originValue, int targetSize, bool truncate = true)
        {
            if (targetSize == originValue.Length)
                return originValue;

            if (targetSize > originValue.Length)
            {
                StringBuilder zeros = new StringBuilder();
                int diff = targetSize - originValue.Length;
                for (int i = 0; i < diff; i++)
                {
                    zeros.Append("0");
                }
                return (zeros.ToString() + originValue);
            }
            else if (targetSize == originValue.Length)
                return originValue;
            else if (truncate)
                return originValue.Substring(0, targetSize);
            else
                return originValue;
        }

        private List<TimePayrollStatisticsSmallDTO> filterTransactions(Employee employee, List<TimePayrollStatisticsSmallDTO> transactions)
        {
            return transactions.Where(t => PayrollRulesUtil.isKPA(t.PensionCompany) && t.EmployeeId == employee.EmployeeId).ToList();
        }

        #endregion
    }

    #region Classes

    public class KPADirekt
    {
        public KPADirekt()
        {
            this.EmployeeKPADirekts = new List<EmployeeKpaDirekt>();
        }
        public string Formatversion
        {
            get
            {
                return "0030";
            }
        }
        public string PostTyp
        {
            get
            {
                return "00";
            }
        }

        public string Lonesystem
        {
            get
            {
                return "SoftOne GO";
            }
        }
        public DateTime Leveransdatum
        {
            get
            {
                return DateTime.Now;
            }
        }
        public int Rapporteringsar { get; set; }
        public string Specialtecken
        {
            get
            {
                return "ÉÅÄÖÜéåäöü$@";
            }
        }
        public string FromDatumLonesystem
        {
            get
            {
                return "00000000";
            }
        }
        public string TomDatumLonesystem
        {
            get
            {
                return "00000000";
            }
        }
        public string TypAvFil
        {
            get
            {
                return "1";
            }
        }
        public string FilFormat
        {
            get
            {
                return "01";
            }
        }

        public List<EmployeeKpaDirekt> EmployeeKPADirekts { get; set; }

        public List<EventHistoryDTO> EventHistories
        {
            get
            {
                return this.EmployeeKPADirekts.SelectMany(s => s.EventHistories).ToList();
            }
        }

        #region Avslutsposten
        public string PostTypAvslut
        {
            get
            {
                return "99";
            }
        }
        public int Antal_00_Post
        {
            get
            {
                return 1;
            }
        }
        public int Antal_10_Post
        {
            get
            {
                return this.EmployeeKPADirekts.Count;
            }
        }
        public int Antal_30_Post
        {
            get
            {
                return this.EmployeeKPADirekts.SelectMany(s => s.EmployeeKPADirektAnstallningPFAs).Count();
            }
        }
        public int Antal_31_Post
        {
            get
            {
                return this.EmployeeKPADirekts.SelectMany(s => s.EmployeeKPADirektAnstallningKAPs).Distinct().Count();
            }
        }
        public int Antal_33_Post
        {
            get
            {
                return this.EmployeeKPADirekts.Count(w => w.EmployeeKPADirektArsbelopp != null);
            }
        }
        public int Antal_37_Post
        {
            get
            {
                return this.EmployeeKPADirekts.Where(w => !w.EmployeeKPADirektLedighetsuppgift.IsNullOrEmpty()).SelectMany(s => s.EmployeeKPADirektLedighetsuppgift).Count();
            }
        }
        public int Antal_38_Post
        {
            get
            {
                return this.EmployeeKPADirekts.Where(w => !w.EmployeeKPADirektSjukOchAktivitetsersattning.IsNullOrEmpty()).SelectMany(s => s.EmployeeKPADirektSjukOchAktivitetsersattning).Count();
            }
        }


        #endregion

        #region Output

        public string OutPutFileString()
        {
            StringBuilder output = new StringBuilder();
            output.Append(EmployeeKpaDirektHelper.StringField(4, this.Formatversion) +
                            "0000000000000" +
                            this.PostTyp +
                            EmployeeKpaDirektHelper.StringField(13, this.Lonesystem) +
                            EmployeeKpaDirektHelper.Datum(Leveransdatum) +
                            EmployeeKpaDirektHelper.NumericField(4, this.Rapporteringsar) +
                            EmployeeKpaDirektHelper.StringField(12, this.Specialtecken) +
                            this.TypAvFil +
                            this.FilFormat +
                             EmployeeKpaDirektHelper.StringField(41, " ") +
                            Environment.NewLine);

            foreach (var employeeKPADirekt in EmployeeKPADirekts)
                output.Append(employeeKPADirekt.OutputFileData());

            output.Append(OutputFileDataSlutPost());

            return output.ToString();
        }

        private string OutputFileDataSlutPost()
        {
            string output = EmployeeKpaDirektHelper.NumericField(17, 0) +
                            EmployeeKpaDirektHelper.StringField(2, this.PostTypAvslut) +
                            EmployeeKpaDirektHelper.StringField(5, "99999") +
                            EmployeeKpaDirektHelper.NumericField(1, this.Antal_00_Post) +
                            EmployeeKpaDirektHelper.NumericField(6, this.Antal_10_Post) +
                            EmployeeKpaDirektHelper.NumericField(6, this.Antal_30_Post) +
                            EmployeeKpaDirektHelper.NumericField(6, this.Antal_31_Post) +
                            EmployeeKpaDirektHelper.NumericField(6, this.Antal_33_Post) +
                            EmployeeKpaDirektHelper.NumericField(6, this.Antal_37_Post) +
                            EmployeeKpaDirektHelper.NumericField(6, this.Antal_38_Post) +
                            EmployeeKpaDirektHelper.StringField(39, " ");

            return output;
        }

        #endregion
    }

    public class EmployeeKpaDirekt
    {
        public EmployeeKpaDirekt()
        {
            this.EmployeeKPADirektAnstallningPFAs = new List<EmployeeKpaDirektAnstallningPfa>();
            this.EmployeeKPADirektAnstallningKAPs = new List<EmployeeKpaDirektAnstallningKap>();
            this.EventHistories = new List<EventHistoryDTO>();
        }
        public int EmployeeId { get; set; }
        public string Kundnummer { get; set; }
        public string Forvaltningsnummer { get; set; }
        public string Personnummer { get; set; }
        public string PostTyp
        {
            get
            {
                return "10";
            }
        }
        public string Personnamn { get; set; }
        public string Sekel { get; set; }

        public List<EventHistoryDTO> EventHistories { get; set; }

        #region report only

        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }

        #endregion

        #region Information

        public List<EmployeeKpaDirektAnstallningPfa> EmployeeKPADirektAnstallningPFAs { get; set; }
        public List<EmployeeKpaDirektAnstallningKap> EmployeeKPADirektAnstallningKAPs { get; set; }
        public EmployeeKpaDirektArsbelopp EmployeeKPADirektArsbelopp { get; set; }
        public List<EmployeeKpaDirektLedighetsuppgift> EmployeeKPADirektLedighetsuppgift { get; set; }
        public List<EmployeeKpaDirektSjukOchAktivitetsersattning> EmployeeKPADirektSjukOchAktivitetsersattning { get; set; }
        public int? Tillhorighet { get; set; }
        public int KPAAgrementType { get; set; }
        #endregion

        #region Output

        public string OutputFileData()
        {
            StringBuilder output = new StringBuilder();
            output.Append(EmployeeKpaDirektHelper.Inledning(this.Kundnummer, this.Forvaltningsnummer, this.Personnummer, this.PostTyp) +
                    EmployeeKpaDirektHelper.StringField(36, this.Personnamn) +
                   EmployeeKpaDirektHelper.StringField(2, this.Sekel) +
                   EmployeeKpaDirektHelper.StringField(43, " ") + Environment.NewLine);

            foreach (var anst in EmployeeKPADirektAnstallningPFAs)
                output.Append(anst.OutputFileData() + Environment.NewLine);

            foreach (var anst in EmployeeKPADirektAnstallningKAPs.Distinct())
                output.Append(anst.OutputFileData() + Environment.NewLine);

            if (this.EmployeeKPADirektArsbelopp != null)
                output.Append(EmployeeKPADirektArsbelopp.OutputFileData() + Environment.NewLine);
            if (!this.EmployeeKPADirektLedighetsuppgift.IsNullOrEmpty())
            {
                foreach (var item in EmployeeKPADirektLedighetsuppgift)
                {
                    output.Append(item.OutputFileData() + Environment.NewLine);
                }
            }
            if (!this.EmployeeKPADirektSjukOchAktivitetsersattning.IsNullOrEmpty())
            {
                foreach (var item in EmployeeKPADirektSjukOchAktivitetsersattning)
                {
                    output.Append(item.OutputFileData() + Environment.NewLine);
                }
            }
            return output.ToString();
        }

        public XElement OutPutXElment(int employeeXmlId)
        {
            var element = new XElement("EmployeeKPADirekt",
                    new XAttribute("Id", employeeXmlId),
                    new XElement("EmployeeNr", this.EmployeeNr),
                    new XElement("EmployeeName", this.EmployeeName),
                    new XElement("Kundnummer", this.Kundnummer),
                    new XElement("Forvaltningsnummer", this.Forvaltningsnummer),
                    new XElement("Personnummer", this.Personnummer),
                    new XElement("PostTyp", this.PostTyp),
                    new XElement("Sekel", this.Sekel));

            int pfaXmlId = 0;
            foreach (var anst in EmployeeKPADirektAnstallningPFAs)
            {
                element.Add(anst.OutPutXElment(pfaXmlId));
                pfaXmlId++;
            }

            int kapXmlId = 0;
            foreach (var anst in EmployeeKPADirektAnstallningKAPs.Distinct())
            {
                element.Add(anst.OutPutXElment(kapXmlId));
                kapXmlId++;
            }

            if (this.EmployeeKPADirektArsbelopp != null)
                element.Add(this.EmployeeKPADirektArsbelopp.OutPutXElment());
            if (!this.EmployeeKPADirektLedighetsuppgift.IsNullOrEmpty())
            {
                foreach (var item in EmployeeKPADirektLedighetsuppgift)
                {
                    element.Add(item.OutPutXElment());
                }
            }
            if (!this.EmployeeKPADirektSjukOchAktivitetsersattning.IsNullOrEmpty())
            {
                foreach (var item in EmployeeKPADirektSjukOchAktivitetsersattning)
                {
                    element.Add(item.OutPutXElment());
                }
            }

            return element;
        }

        #endregion

    }

    public class EmployeeKpaDirektAnstallningPfa
    {
        public EmployeeKpaDirektAnstallningPfa(EmployeeKpaDirekt employeeKPADirekt)
        {
            this.Kundnummer = employeeKPADirekt.Kundnummer;
            this.Forvaltningsnummer = employeeKPADirekt.Forvaltningsnummer;
            this.Personnummer = employeeKPADirekt.Personnummer;
            this.Anstallningsnummer = employeeKPADirekt.EmployeeNr;
        }

        public string Kundnummer { get; set; }
        public string Forvaltningsnummer { get; set; }
        public string Personnummer { get; set; }
        public string PostTyp
        {
            get
            {
                return "30";
            }
        }
        public string Anstallningsnummer { get; set; }
        /// <summary>
        /// Fr o m datum för anställningsuppgiften. Fr o m datum får
        /// tidigast vara den 1 januari det år för vilket leveransen gäller.
        /// Fortsatt anställd från tidigare år anges åååå0101
        /// </summary>
        public DateTime FromDatum { get; set; }
        /// <summary>
        ///  T o m datum för anställningen. Är anställningen öppen,
        /// dvs inte avslutad anges "99999999", annars betraktas
        /// anställningen som avslutad
        /// </summary>
        public DateTime? TomDatum { get; set; }
        /// <summary>
        /// Okänd = blank
        /// Avgång med pension          = U1
        /// Annnan orsak                = U3
        /// Avslut fiktiv anställning   = US
        /// PFA timanstalld             = UD
        /// </summary>
        public string AvslutsKod { get; set; }
        public int PensionAlder { get; set; }
        public bool Brandman { get; set; }
        /// <summary>
        /// 2 = PFA
        /// 3 = Alternativ pensionslösning inom PFA
        /// </summary>
        public string PensionAvtal { get; set; }
        public string VisstidsForordnandeKod { get; set; }
        /// <summary>
        /// Antal pensionsgrundande arbetsdagar för en timanställd under det år
        /// för vilket leveransen avser.Används endast om anställningskod avser
        /// timanställd.Det är möjligt att rapportera 0 dagar och hela perioden
        /// (from-tom) kommer då att tillgodoräknas i en pensionsförmån.För
        /// en öppen timanställning tillgodoräknas tid till tidpunkten för aktuell
        /// beräkning.
        /// </summary>
        public int AntalArbetsdagarForTimanstalld { get; set; }

        #region Output

        public string OutputFileData()
        {
            return EmployeeKpaDirektHelper.Inledning(this.Kundnummer, this.Forvaltningsnummer, this.Personnummer, this.PostTyp) +
                   EmployeeKpaDirektHelper.StringField(20, this.Anstallningsnummer) +
                   EmployeeKpaDirektHelper.DatumInterval(this.FromDatum, this.TomDatum) +
                   EmployeeKpaDirektHelper.StringField(2, this.AvslutsKod) +
                   EmployeeKpaDirektHelper.StringField(4, $"{this.PensionAlder}{this.PensionAlder}") +
                   EmployeeKpaDirektHelper.Brandman(this.Brandman) +
                   EmployeeKpaDirektHelper.StringField(1, this.PensionAvtal) +
                   EmployeeKpaDirektHelper.StringField(1, this.VisstidsForordnandeKod) +
                   EmployeeKpaDirektHelper.NumericField(3, this.AntalArbetsdagarForTimanstalld) +
                   EmployeeKpaDirektHelper.StringField(33, " ");
        }

        public XElement OutPutXElment(int pfaXmlId)
        {
            return new XElement("EmployeeKPADirektAnstallningPFA",
                    new XAttribute("Id", pfaXmlId),
                    new XElement("Kundnummer", this.Kundnummer),
                    new XElement("Forvaltningsnummer", this.Forvaltningsnummer),
                    new XElement("Personnummer", this.Personnummer),
                    new XElement("PostTyp", this.PostTyp),
                    new XElement("Anstallningsnummer", this.Anstallningsnummer),
                    new XElement("FromDatum", this.FromDatum),
                    new XElement("TomDatum", this.TomDatum.HasValue ? this.TomDatum : CalendarUtility.DATETIME_DEFAULT),
                    new XElement("AvslutsKod", this.AvslutsKod),
                    new XElement("PensionAlder", this.PensionAlder),
                    new XElement("Brandman", this.Brandman.ToInt()),
                    new XElement("PensionAvtal", this.PensionAvtal),
                    new XElement("VisstidsForordnandeKod", this.VisstidsForordnandeKod),
                    new XElement("AntalArbetsdagarForTimanstalld", this.AntalArbetsdagarForTimanstalld));
        }

        #endregion
    }

    public class EmployeeKpaDirektAnstallningKap
    {
        public EmployeeKpaDirektAnstallningKap(EmployeeKpaDirekt employeeKPADirekt)
        {
            this.Kundnummer = employeeKPADirekt.Kundnummer;
            this.Forvaltningsnummer = employeeKPADirekt.Forvaltningsnummer;
            this.Personnummer = employeeKPADirekt.Personnummer;
            this.Tillhorighet = employeeKPADirekt.Tillhorighet.HasValue ? employeeKPADirekt.Tillhorighet.ToString() : "0";
            this.Anstallningsnummer = employeeKPADirekt.EmployeeNr;
            this.KPAAgrementType = employeeKPADirekt.KPAAgrementType;
        }

        public string Kundnummer { get; set; }
        public string Forvaltningsnummer { get; set; }
        public string Personnummer { get; set; }
        public string PostTyp
        {
            get
            {
                return "31";
            }
        }
        /// <summary>
        /// 1 = Tillsvidareanställning, gäller även alla behöriga lärare.
        /// 4 = Timavlönad
        /// 5 = Fiktiv anställning
        /// </summary>
        public int AnstallningsKod { get; set; }
        public string Anstallningsnummer { get; set; }
        /// <summary>
        /// Fr o m datum för anställningsuppgiften. Fr o m datum får
        /// tidigast vara den 1 januari det år för vilket leveransen gäller.
        /// Fortsatt anställd från tidigare år anges åååå0101
        /// </summary>
        public DateTime FromDatum { get; set; }
        /// <summary>
        ///  T o m datum för anställningen. Är anställningen öppen,
        /// dvs inte avslutad anges "99999999", annars betraktas
        /// anställningen som avslutad
        /// </summary>
        public DateTime? TomDatum { get; set; }
        /// <summary>
        /// Okänd = blank
        /// Avgång med pension          = U1
        /// Annnan orsak                = U3
        /// Avslut fiktiv anställning   = US
        /// PFA timanstalld             = UD
        /// </summary>
        public int LonVidAnstallningensTilltrade { get; set; }
        public int PensionsgrundandeTillagg { get; set; }
        public int PensionAlder { get; set; }
        public bool Brandman { get; set; }
        /// <summary>
        /// Antal pensionsgrundande arbetsdagar för en timanställd under det år
        /// för vilket leveransen avser.Används endast om anställningskod avser
        /// timanställd.Det är möjligt att rapportera 0 dagar och hela perioden
        /// (from-tom) kommer då att tillgodoräknas i en pensionsförmån.För
        /// en öppen timanställning tillgodoräknas tid till tidpunkten för aktuell
        /// beräkning.
        /// </summary>
        public int AntalArbetsdagarForTimAnstalld { get; set; }
        /// <summary>
        /// 1 = BEA
        /// 2 = PAN
        /// 3 = Medstud.
        /// Annars 0.
        /// </summary>
        public string Tillhorighet { get; set; }
        public int KPAAgrementType { get; set; }

        #region Output

        public string OutputFileData()
        {
            return EmployeeKpaDirektHelper.Inledning(this.Kundnummer, this.Forvaltningsnummer, this.Personnummer, this.PostTyp) +
                   EmployeeKpaDirektHelper.StringField(20, this.Anstallningsnummer) +
                   EmployeeKpaDirektHelper.NumericField(1, this.AnstallningsKod) +
                   EmployeeKpaDirektHelper.DatumInterval(this.FromDatum, this.TomDatum) +
                   EmployeeKpaDirektHelper.NumericField(6, this.LonVidAnstallningensTilltrade) +
                   EmployeeKpaDirektHelper.NumericField(6, this.PensionsgrundandeTillagg) +
                   EmployeeKpaDirektHelper.NumericField(6, 0) +
                   EmployeeKpaDirektHelper.StringField(4, $"{this.PensionAlder}{this.PensionAlder}") +
                   EmployeeKpaDirektHelper.Brandman(this.Brandman) +
                   //EmployeeKpaDirektHelper.NumericField(3, this.AntalArbetsdagarForTimAnstalld) +
                   //EmployeeKpaDirektHelper.StringField(1, this.Tillhorighet) +
                   EmployeeKpaDirektHelper.NumericField(2, this.KPAAgrementType) +
                   EmployeeKpaDirektHelper.StringField(19, " ");
        }

        public XElement OutPutXElment(int kapXmlId)
        {
            return new XElement("EmployeeKPADirektAnstallningKAP",
                    new XAttribute("Id", kapXmlId),
                    new XElement("Kundnummer", this.Kundnummer),
                    new XElement("Forvaltningsnummer", this.Forvaltningsnummer),
                    new XElement("Personnummer", this.Personnummer),
                    new XElement("PostTyp", this.PostTyp),
                    new XElement("Anstallningsnummer", this.Anstallningsnummer),
                    new XElement("FromDatum", this.FromDatum),
                    new XElement("TomDatum", this.TomDatum.HasValue ? this.TomDatum : CalendarUtility.DATETIME_DEFAULT),
                    new XElement("LonVidAnstallningensTilltrade", this.LonVidAnstallningensTilltrade),
                    new XElement("PensionsgrundandeTillagg", this.PensionsgrundandeTillagg),
                    new XElement("PensionAlder", this.PensionAlder),
                    new XElement("Brandman", this.Brandman.ToInt()),
                    new XElement("AntalArbetsdagarForTimanstalld", this.AntalArbetsdagarForTimAnstalld),
                    new XElement("Tillhorighet", this.Tillhorighet));
        }

        #endregion
    }

    public class EmployeeKpaDirektArsbelopp
    {
        public EmployeeKpaDirektArsbelopp(EmployeeKpaDirekt employeeKPADirekt)
        {
            this.Kundnummer = employeeKPADirekt.Kundnummer;
            this.Forvaltningsnummer = employeeKPADirekt.Forvaltningsnummer;
            this.Personnummer = employeeKPADirekt.Personnummer;
            this.Anstallningsnummer = employeeKPADirekt.EmployeeNr;
        }
        public string Kundnummer { get; set; }
        public string Forvaltningsnummer { get; set; }
        public string Personnummer { get; set; }
        public string PostTyp
        {
            get
            {
                return "33";
            }
        }
        /// <summary>
        /// Rapporteringsår
        /// </summary>
        public int Ar { get; set; }
        public string Anstallningsnummer { get; set; }
        /// <summary>
        /// Från och med den tidpunkt lönebeloppet avser.
        /// Fr o m datum får tidigast vara den 1 januari det år för
        /// vilket leveransen gäller.
        /// Observera exempel för AKAP-KL och kvartalsförmedling
        /// (sid 12).
        /// /// </summary>
        public DateTime FromDatum { get; set; }
        /// <summary>
        ///Till och med den tidpunkt lönebeloppet avser.
        // Om lönebeloppet gäller t v ange rapporteringsår +1231.
        // Observera exempel för AKAP-KL och kvartalsförmedling
        // (sid 12). 
        /// </summary>
        public DateTime? TomDatum { get; set; }
        /// <summary>
        ///Pensionsgrundande lön under året.Anges i krontal utan
        ///decimal. Inga negativa belopp.
        ///Vid sjuk/aktivitetsersättning rapporteras för tid med
        ///avgiftsbefrielseförsäkring endast faktiskt utbetald
        ///pensionsgrundande lön.Avgiftsbefrielseförsäkringen
        ///träder in från och med månaden efter den då
        ///försäkringskassan tagit beslut om pension.
        ///Avgiftsbefrielseförsäkring gäller
        ///även under tid med arbetsskadelivränta som inte är
        ///samordnad med sjuk/aktivitetsersättning.
        /// </summary>
        public int PensionsgrundandeLon { get; set; }
        /// <summary>
        /// Under året förändrad överenskommen lön i en pågående
        /// anställning.Nollutfylls om ingen förändring av den
        /// överenskomna lönen har skett i anställningen.
        /// </summary>
        public int ManadslonAKAP_KL { get; set; }
        public string ProcentsatsUnder7_5Inkomstbasbelopp
        {
            get
            {
                return "0000";
            }
        }
        /// <summary>
        /// Pensionsavgift (%) för avgiftsbestämd ålderspension
        /// under tak.Anges i två heltal och två decimaler. (T.ex. 4,5
        ///% anges: 0450. 
        /// </summary>
        public decimal ProcentsatsForAvgiftsbaseradAlderspensionUnder7_5Inkomstbasbelopp { get; set; }
        public string ProcentsatsOver7_5Inkomstbasbelopp
        {
            get
            {
                return "0000";
            }
        }
        /// <summary>
        /// Pensionsavgift (%) för avgiftsbestämd ålderspension över
        /// tak.Anges i två heltal och två decimaler. (T.ex. 4,5 %
        /// anges: 0450) 
        /// </summary>
        public decimal ProcentsatsForAvgiftsbaseradAlderspensionOver7_5Inkomstbasbelopp { get; set; }
        /// <summary>
        /// 1 = PFA01
        /// 2 = PFA98
        /// 6 = KAP-KL
        /// 8 = AKAP-KL
        /// </summary>
        public string VilketAvtalAnställningenTillhor { get; set; }

        #region Output

        public string OutputFileData()
        {
            return EmployeeKpaDirektHelper.Inledning(this.Kundnummer, this.Forvaltningsnummer, this.Personnummer, this.PostTyp) +
                EmployeeKpaDirektHelper.StringField(20, this.Anstallningsnummer) +
                EmployeeKpaDirektHelper.NumericField(4, this.Ar) +
                EmployeeKpaDirektHelper.DatumInterval(this.FromDatum, this.TomDatum) +
                EmployeeKpaDirektHelper.NumericField(7, this.PensionsgrundandeLon) +
                EmployeeKpaDirektHelper.NumericField(8, this.ManadslonAKAP_KL) +
                //this.ProcentsatsUnder7_5Inkomstbasbelopp +
                EmployeeKpaDirektHelper.NumericField(4, this.ProcentsatsForAvgiftsbaseradAlderspensionUnder7_5Inkomstbasbelopp) +
                //this.ProcentsatsOver7_5Inkomstbasbelopp +
                EmployeeKpaDirektHelper.NumericField(4, this.ProcentsatsForAvgiftsbaseradAlderspensionOver7_5Inkomstbasbelopp) +
                EmployeeKpaDirektHelper.StringField(1, this.VilketAvtalAnställningenTillhor) +
                EmployeeKpaDirektHelper.StringField(18, " ");  //9?
        }

        public XElement OutPutXElment()
        {
            return new XElement("EmployeeKPADirektArsbelopp",
                    new XElement("Kundnummer", this.Kundnummer),
                    new XElement("Forvaltningsnummer", this.Forvaltningsnummer),
                    new XElement("Personnummer", this.Personnummer),
                    new XElement("PostTyp", this.PostTyp),
                    new XElement("Anstallningsnummer", this.Anstallningsnummer),
                    new XElement("FromDatum", this.FromDatum),
                    new XElement("TomDatum", this.TomDatum.HasValue ? this.TomDatum : CalendarUtility.DATETIME_DEFAULT),
                    new XElement("PensionsgrundandeLon", this.PensionsgrundandeLon),
                    new XElement("ManadslonAKAP_KL", this.ManadslonAKAP_KL),
                    new XElement("ProcentsatsForAvgiftsbaseradAlderspensionUnder7_5Inkomstbasbelopp", this.ProcentsatsForAvgiftsbaseradAlderspensionUnder7_5Inkomstbasbelopp),
                    new XElement("ProcentsatsForAvgiftsbaseradAlderspensionOver7_5Inkomstbasbelopp", this.ProcentsatsForAvgiftsbaseradAlderspensionOver7_5Inkomstbasbelopp),
                    new XElement("VilketAvtalAnställningenTillhor", this.VilketAvtalAnställningenTillhor));
        }

        #endregion
    }


    public class EmployeeKpaDirektLedighetsuppgift
    {
        public EmployeeKpaDirektLedighetsuppgift(EmployeeKpaDirekt employeeKPADirekt)
        {
            this.Kundnummer = employeeKPADirekt.Kundnummer;
            this.Forvaltningsnummer = employeeKPADirekt.Forvaltningsnummer;
            this.Personnummer = employeeKPADirekt.Personnummer;
            this.Anstallningsnummer = employeeKPADirekt.EmployeeNr;
        }

        public string Kundnummer { get; set; }
        public string Forvaltningsnummer { get; set; }
        public string Personnummer { get; set; }
        public string Anstallningsnummer { get; set; }
        public string PostTyp
        {
            get
            {
                return "37";
            }
        }
        /// <summary>
        /// Ledighetens fr o m datum.
        /// Fr o m datum får tidigast vara den 1 januari det år för
        /// vilket leveransen gäller.
        /// /// </summary>
        public DateTime FromDatum { get; set; }
        /// <summary>
        /// Ledighetens t o m datum.
        /// Om ledigheten pågår över årsskifte anges "99999999" 
        /// </summary>
        public DateTime? TomDatum { get; set; }

        #region Output

        public string OutputFileData()
        {
            return EmployeeKpaDirektHelper.Inledning(this.Kundnummer, this.Forvaltningsnummer, this.Personnummer, this.PostTyp) +
                  EmployeeKpaDirektHelper.StringField(20, this.Anstallningsnummer) +
                  EmployeeKpaDirektHelper.DatumInterval(this.FromDatum, this.TomDatum) +
                  EmployeeKpaDirektHelper.StringField(45, " "); //45?
        }

        public XElement OutPutXElment()
        {
            return new XElement("EmployeeKPADirektLedighetsuppgift",
                    new XElement("Kundnummer", this.Kundnummer),
                    new XElement("Forvaltningsnummer", this.Forvaltningsnummer),
                    new XElement("Personnummer", this.Personnummer),
                    new XElement("PostTyp", this.PostTyp),
                    new XElement("FromDatum", this.FromDatum),
                    new XElement("TomDatum", this.TomDatum.HasValue ? this.TomDatum : CalendarUtility.DATETIME_DEFAULT));
        }

        #endregion
    }

    public class EmployeeKpaDirektSjukOchAktivitetsersattning
    {
        public EmployeeKpaDirektSjukOchAktivitetsersattning(EmployeeKpaDirekt employeeKPADirekt)
        {
            this.Kundnummer = employeeKPADirekt.Kundnummer;
            this.Forvaltningsnummer = employeeKPADirekt.Forvaltningsnummer;
            this.Personnummer = employeeKPADirekt.Personnummer;
            this.Anstallningsnummer = employeeKPADirekt.EmployeeNr;
        }

        public string Kundnummer { get; set; }
        public string Forvaltningsnummer { get; set; }
        public string Personnummer { get; set; }
        public string Anstallningsnummer { get; set; }
        public string PostTyp
        {
            get
            {
                return "38";
            }
        }
        /// <summary>
        /// Datum för händelsen (startdatum eller datum för ändring).
        /// /// </summary>
        public DateTime FromDatum { get; set; }
        /// <summary>
        /// T o m datum för händelsen. Om fr o m datum finns, men inte t o m datum, anges 99999999. 
        /// </summary>
        public DateTime? TomDatum { get; set; }
        /// <summary>
        /// Den senaste omfattningen av sjuk/aktivitetsersättning
        /// under året.Anges i procent med 3 tecken.
        /// Giltiga omfattningar:
        /// 100 = Omfattning 100%
        /// 075 = Omfattning 75%
        /// 066 = Omfattning 67%
        /// 050 = Omfattning 50%
        /// 025 = Omfattning 25%
        /// 710 = Omfattning okänd
        /// </summary>
        public int Omfattning { get; set; }

        #region Output

        public string OutputFileData()
        {
            return EmployeeKpaDirektHelper.Inledning(this.Kundnummer, this.Forvaltningsnummer, this.Personnummer, this.PostTyp) +
                   EmployeeKpaDirektHelper.StringField(20, this.Anstallningsnummer) +
                   EmployeeKpaDirektHelper.DatumInterval(this.FromDatum, this.TomDatum) +
                   EmployeeKpaDirektHelper.NumericField(3, this.Omfattning) +
                   EmployeeKpaDirektHelper.StringField(42, " ");

        }

        public XElement OutPutXElment()
        {
            return new XElement("EmployeeKPADirektLedighetsuppgift",
                    new XElement("Kundnummer", this.Kundnummer),
                    new XElement("Forvaltningsnummer", this.Forvaltningsnummer),
                    new XElement("Personnummer", this.Personnummer),
                    new XElement("PostTyp", this.PostTyp),
                    new XElement("FromDatum", this.FromDatum),
                    new XElement("TomDatum", this.TomDatum.HasValue ? this.TomDatum : CalendarUtility.DATETIME_DEFAULT),
                    new XElement("Omfattning", this.Omfattning));
        }

        #endregion
    }

    public static class EmployeeKpaDirektHelper
    {
        public static string NumericField(int targetSize, int originValue)
        {
            return FillWithChar("0", targetSize, originValue.ToString());
        }

        public static string NumericField(int targetSize, decimal originValue)
        {
            string value = decimal.Round(originValue, 2).ToString().Replace(",", "").Replace(".", "");
            return FillWithChar("0", targetSize, value);
        }

        public static string StringField(int targetSize, string originValue)
        {
            return FillWithChar(" ", targetSize, originValue, reverse: true);
        }

        public static string Inledning(string kundnummer, string forvaltningsnummer, string personnummer, string posttyp)
        {
            return StringField(4, kundnummer) + StringField(3, forvaltningsnummer) + StringField(10, personnummer) + StringField(2, posttyp);
        }

        public static string DatumInterval(DateTime fromDatum, DateTime? tomDatum)
        {
            return Datum(fromDatum) + (tomDatum.HasValue ? Datum(tomDatum.Value) : "99999999");
        }

        public static string Brandman(bool isBrandman)
        {
            return isBrandman ? "B" : " ";
        }

        public static string Datum(DateTime datum)
        {
            var returnValue = datum.Year.ToString();

            if (datum.Month.ToString().Length == 1)
                returnValue += "0" + datum.Month;
            else
                returnValue += datum.Month;

            if (datum.Day.ToString().Length == 1)
                returnValue += "0" + datum.Day;
            else
                returnValue += datum.Day;

            return returnValue;
        }


        public static string FillWithChar(string character, int targetSize, string originValue, bool truncate = true, bool reverse = false)
        {
            if (originValue == null)
                originValue = "";

            if (targetSize == originValue.Length)
                return originValue;

            if (targetSize > originValue.Length)
            {
                StringBuilder zeros = new StringBuilder();
                int diff = targetSize - originValue.Length;
                for (int i = 0; i < diff; i++)
                {
                    zeros.Append(character);
                }
                return !reverse ? (zeros.ToString() + originValue) : (originValue + zeros.ToString());
            }
            else if (truncate)
                return originValue.Substring(0, targetSize - 1);
            else
                return originValue;
        }
    }

    public class EmployeeKpa
    {
        public int EmployeeId { get; set; }
        public string Transaktionskod { get; set; }
        public string InkomstKalla { get; set; }
        public string Avtalsnummer { get; set; }
        public string Kundnummer { get; set; }
        public string Personnummer { get; set; }
        public int InkomstAr { get; set; }
        public string InkomstManad { get; set; }
        public string Periodicitet { get; set; }
        public int Inkomst { get; set; }
        public string Tecken { get; set; }
        public List<EmployeeKpaTransactionDTO> Transactions { get; set; }

    }

    public class EmployeeKpaTransactionDTO
    {
        public string Type { get; set; }
        public string PayrollProductNumber { get; set; }
        public string Name { get; set; }
        public decimal Quantity { get; set; }
        public decimal Amount { get; set; }
    }

    #endregion
}



