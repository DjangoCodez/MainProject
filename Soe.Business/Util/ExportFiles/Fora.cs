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

namespace SoftOne.Soe.Business.Util.ExportFiles
{
    public class Fora : ExportFilesBase
    {
        #region Ctor

        public Fora(ParameterObject parameterObject, CreateReportResult reportResult) : base(parameterObject, reportResult)
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

            if (!TryGetIdsFromSelection(reportResult, out List<int> selectionTimePeriodIds, "periods"))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionTimePeriodIds, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return null;

            if (employees == null)
                employees = EmployeeManager.GetAllEmployeesByIds(entities, base.ActorCompanyId, selectionEmployeeIds, loadEmployment: true);

            StringBuilder sb = new StringBuilder();
            int year = 0;

            #endregion

            #region Group on Personnummer

            List<EmployeeFora> validEmployeeForas = new List<EmployeeFora>();

            List<EmployeeFora> employeeForas = CreateEmployeeForas(entities, employees, year, selectionTimePeriodIds);
            foreach (var employeeForasByPersonnummer in employeeForas.Where(e => e.LonforRapporteringsAret != 0 && !e.Tjansteman).GroupBy(e => e.Personnummer).ToList())
            {
                if (employeeForasByPersonnummer == null)
                    continue;

                var first = employeeForasByPersonnummer.OrderBy(t => t.StartDatum).First();
                var last = employeeForasByPersonnummer.OrderBy(t => t.StartDatum).Last();

                EmployeeFora employeeFora = new EmployeeFora()
                {
                    EmployeeId = first.EmployeeId,
                    Rapporteringar = first.Rapporteringar,
                    Avtalsnummer = first.Avtalsnummer,
                    Personnummer = first.Personnummer,
                    Namn = first.Namn,
                    LonforRapporteringsAret = employeeForasByPersonnummer.Sum(i => i.LonforRapporteringsAret),
                    SlutatUnderRapporterinsgAret = first.SlutatUnderRapporterinsgAret,
                    KollektivAvtal = first.KollektivAvtal,
                    AFACategory = first.AFACategory,
                    StartDatum = first.StartDatum,
                    SlutDatum = last.SlutDatum
                };

                validEmployeeForas.Add(employeeFora);
            }

            #endregion

            foreach (var employeeFora in validEmployeeForas)
            {
                #region Employee

                Employee employee = employees.FirstOrDefault(i => i.EmployeeId == employeeFora.EmployeeId);
                if (employee == null)
                    continue;

                if (employeeFora.Rapporteringar > 2000)
                    sb.Append(employeeFora.Rapporteringar.ToString().Substring(2));
                else
                    sb.Append(DateTime.Now.AddYears(-1).Year.ToString().Substring(2));

                sb.Append(CreateFieldBlank(employeeFora.Avtalsnummer, 7));
                sb.Append(CreateFieldBlank("    ", 4));
                sb.Append(CreateFieldBlank(employeeFora.Personnummer, 10));
                sb.Append(CreateFieldBlank(employeeFora.Namn, 32));
                sb.Append(CreateFieldBlank("", 16));
                sb.Append(CreateFieldSum(employeeFora.LonforRapporteringsAret.ToString(), 7));
                sb.Append(CreateFieldBlank(employeeFora.SlutatUnderRapporterinsgAret.ToString(), 1));
                sb.Append(CreateFieldBlank(employeeFora.KollektivAvtal, 1));
                sb.Append(employeeFora.StartDatum.HasValue ? CreateFieldBlank(DateTimeString(employeeFora.StartDatum.Value), 6) : CreateFieldBlank("", 6));
                sb.Append(employeeFora.SlutDatum.HasValue ? CreateFieldBlank(DateTimeString(employeeFora.SlutDatum.Value), 6) : CreateFieldBlank("", 6));
                personalDataRepository.AddEmployeeSocialSec(employee);

                #endregion

                sb.AppendLine();
            }

            #region Create File

            var directory = Directory.CreateDirectory(ConfigSettings.SOE_SERVER_DIR_TEMP_REPORT_PHYSICAL + Guid.NewGuid().ToString());
            string fileName = IOUtil.FileNameSafe("Fora" + "_" + Company.Name + "_" + DateTime.Now.ToString("yyyyMMddhhmmss"));
            string filePath = StringUtility.GetValidFilePath(directory.FullName) + fileName + ".txt";

            try
            {
                File.WriteAllText(filePath, sb.ToString(), Encoding.GetEncoding(1252));
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

        public List<EmployeeFora> CreateEmployeeForas(CompEntities entities, List<Employee> employees, int year, List<int> selectionTimePeriodIds)
        {
            List<EmployeeFora> employeeForas = new List<EmployeeFora>();
            ForaColletiveAgrementDTO foraColletiveAgrementDTO = new ForaColletiveAgrementDTO();

            if (year == 0)
            {
                List<TimePeriod> timePeriods = TimePeriodManager.GetTimePeriods(selectionTimePeriodIds, reportResult.ActorCompanyId).Where(x => x.PayrollStopDate.HasValue).ToList();
                DateTime? payrollStopDate = timePeriods?.OrderBy(x => x.PayrollStartDate).LastOrDefault()?.PayrollStopDate;
                if (!payrollStopDate.HasValue)
                    return employeeForas;

                year = payrollStopDate.Value.Year;
            }
            var dateFrom = new DateTime(year, 1, 1);
            var dateTo = CalendarUtility.GetEndOfYear(dateFrom);

            List<EmployeeGroup> employeeGroups = EmployeeManager.GetEmployeeGroups(entities, reportResult.ActorCompanyId, true, true, true, true);
            List<PayrollGroup> payrollGroups = PayrollManager.GetPayrollGroups(entities, reportResult.ActorCompanyId, true, true, loadSettings: true);
            List<PayrollPriceType> payrollPriceTypes = PayrollManager.GetPayrollPriceTypes(entities, reportResult.ActorCompanyId, null, false);
            var transactions = TimeTransactionManager.GetTimePayrollStatisticsSmallDTOs_new(entities, reportResult.ActorCompanyId, employees, selectionTimePeriodIds, setPensionCompany: true, ignoreAccounting: true);
            bool showSocialSec = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId);
            string avtalsnummer = SettingManager.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.PayrollExportForaAgreementNumber, reportResult.UserId, reportResult.ActorCompanyId, 0);

            var afaCategories = base.GetTermGroupContent(TermGroup.PayrollReportsAFACategory).ToDictionary();

            foreach (var employee in employees)
            {
                if (employee.AFACategory == (int)TermGroup_AfaCategory.Undantas)
                    continue;

                var employeeTransactions = transactions.Where(e => e.EmployeeId == employee.EmployeeId).ToList();
                List<int> skipTransactionIds = employeeTransactions.Where(p => !p.IsScheduleTransaction).Select(t => t.TransactionId).ToList();
                employeeTransactions.AddRange(TimeTransactionManager.GetPayrollStartValuesAsTimePayrollStatisticsSmallDTOs(employee, ActorCompanyId, dateFrom.Year, skipTransactionIds: skipTransactionIds, setPensionCompany: true));

                var filteredEmployeeTransactions = FilterTransactions(employee, employeeTransactions);
                var lastFilteredEmployeeTransaction = filteredEmployeeTransactions.LastOrDefault();
                var kollektivAvtal = foraColletiveAgrementDTO.GetForaColletiveAgrement(lastFilteredEmployeeTransaction?.ForaCollectiveAgreementId ?? 0);
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

                var category = string.Empty;
                if (!afaCategories.IsNullOrEmpty())
                {
                    if (employee.AFACategory != 0)
                    {
                        category = afaCategories != null && afaCategories.Any(c => c.Key == employee.AFACategory) ? afaCategories.FirstOrDefault(c => c.Key == employee.AFACategory).Value : string.Empty;
                    }
                    else
                    {
                        var getlastEmployment = employee.Employment.GetLastEmployment(anstalldTom);
                        if (getlastEmployment != null)
                        {
                            PayrollGroup payrollGroup = employee.GetPayrollGroup(getlastEmployment.DateFrom, payrollGroups: base.GetPayrollGroupsFromCache(entities, CacheConfig.Company(ReportResult.ActorCompanyId)));
                            if (payrollGroup != null)
                            {
                                int? settingcategory = payrollGroup?.PayrollGroupSetting.FirstOrDefault(f => f.Type == (int)PayrollGroupSettingType.ForaCategory)?.IntData;
                                if (settingcategory.HasValue)
                                    category = afaCategories != null && afaCategories.Any(c => c.Key == settingcategory.Value) ? afaCategories.FirstOrDefault(c => c.Key == settingcategory.Value).Value : string.Empty;
                            }
                        }
                    }
                }

                EmployeeFora employeeFora = new EmployeeFora()
                {
                    EmployeeId = employee.EmployeeId,
                    Rapporteringar = year,
                    Avtalsnummer = avtalsnummer,
                    Personnummer = showSocialSec ? StringUtility.SocialSecYYMMDDXXXX(employee.SocialSec) : StringUtility.SocialSecYYMMDD_Dash_Stars(employee.SocialSec),
                    Namn = employee.Name,
                    LonforRapporteringsAret = Convert.ToInt32(filteredEmployeeTransactions.Sum(t => t.Amount)),
                    SlutatUnderRapporterinsgAret = anstalldTom == null ? string.Empty : "S",
                    KollektivAvtal = kollektivAvtal.shortText,
                    AFACategory = category,
                    StartDatum = anstalldFrom,
                    SlutDatum = anstalldTom
                };

                employeeFora.Transactions = new List<EmployeeForaTransactionDTO>();

                var totalGrossTrans = employeeTransactions.Where(t => t.IsGrossSalary()).ToList();
                if (!totalGrossTrans.IsNullOrEmpty())
                {
                    employeeFora.LonforRapporteringsAretEjFora = Convert.ToInt32(totalGrossTrans.Sum(p => p.Amount)) - employeeFora.LonforRapporteringsAret;

                    EmployeeForaTransactionDTO EmployeeForaTransactionDTO = new EmployeeForaTransactionDTO()
                    {
                        Type = "GrossSalaryTotal",
                        PayrollProductNumber = "",
                        Name = "",
                        Quantity = 0,
                        Amount = totalGrossTrans.Sum(p => p.Amount),
                    };

                    employeeFora.Transactions.Add(EmployeeForaTransactionDTO);
                }

                if (employeeFora.LonforRapporteringsAret != 0)
                {
                    foreach (var product in filteredEmployeeTransactions.GroupBy(m => m.PayrollProductId))
                    {
                        EmployeeForaTransactionDTO EmployeeForaTransactionDTO = new EmployeeForaTransactionDTO()
                        {
                            Type = product.FirstOrDefault().SysPayrollTypeLevel1Name + "#" + product.FirstOrDefault().SysPayrollTypeLevel2Name + "#" + product.FirstOrDefault().SysPayrollTypeLevel3Name + "#" + product.FirstOrDefault().SysPayrollTypeLevel4Name,
                            PayrollProductNumber = product.FirstOrDefault().PayrollProductNumber,
                            Name = product.FirstOrDefault().PayrollProductName,
                            Quantity = product.Sum(p => p.Quantity),
                            Amount = product.Sum(p => p.Amount),
                        };

                        employeeFora.Transactions.Add(EmployeeForaTransactionDTO);
                    }
                }

                if (employee.AFACategory == (int)TermGroup_AfaCategory.Tjansteman)
                    employeeFora.Tjansteman = true;

                employeeForas.Add(employeeFora);
            }

            return employeeForas;
        }

        #endregion

        #region Help-methods

        private string CreateFieldBlank(string originValue, int targetSize, bool truncate = true)
        {
            if (targetSize > originValue.Length)
            {
                StringBuilder blanks = new StringBuilder();
                int diff = targetSize - originValue.Length;
                for (int i = 0; i < diff; i++)
                {
                    blanks.Append(" ");
                }
                return (originValue + blanks.ToString());
            }
            else if (targetSize == originValue.Length)
                return originValue;
            else if (truncate)
                return originValue.Substring(0, targetSize);
            else
                return originValue;
        }

        private string CreateFieldSum(string originValue, int targetSize, bool truncate = true)
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
            else if (truncate)
                return originValue.Substring(0, targetSize - 1);
            else
                return originValue;
        }

        private string DateTimeString(DateTime date) 
            => date.ToString("yyMMdd");

        private List<TimePayrollStatisticsSmallDTO> FilterTransactions(Employee employee, List<TimePayrollStatisticsSmallDTO> transactions) =>
            transactions?.Where(t => PayrollRulesUtil.isFora(t.PensionCompany) && t.EmployeeId == employee?.EmployeeId).ToList() ?? new List<TimePayrollStatisticsSmallDTO>();

        #endregion
    }

    public class EmployeeFora
    {
        public int EmployeeId { get; set; }
        public int Rapporteringar { get; set; }
        public string Avtalsnummer { get; set; }
        public string Personnummer { get; set; }
        public string Namn { get; set; }
        public int LonforRapporteringsAret { get; set; }
        public int LonforRapporteringsAretEjFora { get; set; }
        public string SlutatUnderRapporterinsgAret { get; set; }
        public string KollektivAvtal { get; set; }
        public DateTime? StartDatum { get; set; }
        public DateTime? SlutDatum { get; set; }
        public string AFACategory { get; set; }
        public int AFACategoryId { get; set; }
        public decimal OnlyAggregatedSalary { get; set; }
        public bool Tjansteman { get; set; }

        public List<EmployeeForaTransactionDTO> Transactions { get; set; }

    }

    public class EmployeeForaTransactionDTO
    {
        public string Type { get; set; }
        public string PayrollProductNumber { get; set; }
        public string Name { get; set; }
        public decimal Quantity { get; set; }
        public decimal Amount { get; set; }
    }
}
