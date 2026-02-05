using Newtonsoft.Json;
using SoftOne.Soe.Business.Core.Reporting;
using SoftOne.Soe.Business.Core.Reporting.Matrix.Models;
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

    public class ForaMonthly : ExportFilesBase
    {
        #region Ctor
        public ForaMonthly(ParameterObject parameterObject, CreateReportResult reportResult) : base(parameterObject, reportResult)
        {
            this.reportResult = reportResult;
        }

        #endregion

        #region Public methods

        public string CreateForaMonthlyFile(CompEntities entities, bool exportExcelFile)
        {
            #region Prereq

            if (!TryGetIdsFromSelection(reportResult, out List<int> selectionTimePeriodIds, "periods"))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionTimePeriodIds, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return null;

            TryGetBoolFromSelection(reportResult, out bool setAsFinal, "setAsFinal");

            if (employees == null)
                employees = EmployeeManager.GetAllEmployeesByIds(entities, base.ActorCompanyId, selectionEmployeeIds, loadEmployment: true);

            var timePeriods = TimePeriodManager.GetTimePeriods(TermGroup_TimePeriodType.Payroll, Company.ActorCompanyId);
            var selectedTimePeriods = timePeriods.Where(w => selectionTimePeriodIds.Contains(w.TimePeriodId)).OrderBy(o => o.StartDate).ToList();
            List<int> actorIds = employees.Select(s => s.ContactPerson?.ActorContactPersonId ?? 0).ToList();
            var contacts = ContactManager.GetContactsFromActors(entities, actorIds, loadActor: true, loadAddresses: true);

            Guid guid = Guid.NewGuid();

            #endregion

            #region Collections

            List<PayrollGroup> payrollGroups = PayrollManager.GetPayrollGroups(entities, reportResult.ActorCompanyId, true, true, loadSettings: true);

            List<EmployeeSetting> employeeSettingsFokId = new List<EmployeeSetting>();
            selectedTimePeriods.ForEach(t => employeeSettingsFokId.AddRange(EmployeeManager.GetEmployeeSettings(entities, reportResult.ActorCompanyId, selectionEmployeeIds, t.StartDate, t.PayrollStopDate ?? t.StopDate, TermGroup_EmployeeSettingType.Reporting, TermGroup_EmployeeSettingType.Reporting_Fora, TermGroup_EmployeeSettingType.Reporting_Fora_FokId)));

            List<EmployeeSetting> employeeSettingsOption1 = new List<EmployeeSetting>();
            selectedTimePeriods.ForEach(t => employeeSettingsOption1.AddRange(EmployeeManager.GetEmployeeSettings(entities, reportResult.ActorCompanyId, selectionEmployeeIds, t.StartDate, t.PayrollStopDate ?? t.StopDate, TermGroup_EmployeeSettingType.Reporting, TermGroup_EmployeeSettingType.Reporting_Fora, TermGroup_EmployeeSettingType.Reporting_Fora_Option1)));

            List<EmployeeSetting> employeeSettingsOption2 = new List<EmployeeSetting>();
            selectedTimePeriods.ForEach(t => employeeSettingsOption2.AddRange(EmployeeManager.GetEmployeeSettings(entities, reportResult.ActorCompanyId, selectionEmployeeIds, t.StartDate, t.PayrollStopDate ?? t.StopDate, TermGroup_EmployeeSettingType.Reporting, TermGroup_EmployeeSettingType.Reporting_Fora, TermGroup_EmployeeSettingType.Reporting_Fora_Option2)));

            var transactions = TimeTransactionManager.GetTimePayrollStatisticsSmallDTOs_new(entities, reportResult.ActorCompanyId, employees, selectionTimePeriodIds, setPensionCompany: true, ignoreAccounting: true);

            List<EventHistoryDTO> events = new List<EventHistoryDTO>();

            #endregion

            #region Generate classes

            ForaJsonMonthly foraJsonMonthly = new ForaJsonMonthly();
            foraJsonMonthly.source = "SoftOne";

            ForaReport foraReport = new ForaReport()
            {
                refId = guid.ToString(),
                organizationNumber = StringUtility.RemoveDash(StringUtility.OrgNrWithout16(Company.OrgNr))
            };
            foraJsonMonthly.reports.Add(foraReport);

            foreach (var employeeBySocialSec in employees.GroupBy(g => g.SocialSec))
            {
                var employee = employeeBySocialSec.FirstOrDefault();
                if (employee == null)
                    continue;

                if (employee.AFACategory == (int)TermGroup_AfaCategory.Undantas)
                    continue;

                var employeeTransactions = transactions.Where(e => employeeBySocialSec.Select(s => s.EmployeeId).Contains(e.EmployeeId)).ToList();
                var filteredEmployeeTransactions = employeeTransactions;
                filteredEmployeeTransactions = FilterTransactions(employeeBySocialSec.ToList(), filteredEmployeeTransactions);

                ForaEmployment foraEmployment = new ForaEmployment()
                {
                    employeeId = StringUtility.SocialSecYYYYMMDDXXXX(employee.SocialSec),
                    firstName = employee.FirstName,
                    lastName = employee.LastName
                };

                var lastEmployment = employee.Employment.GetLastEmployment();
                if (lastEmployment?.DateTo != null && lastEmployment.FinalSalaryStatusId != 0)
                    foraEmployment.employmentEndDate = lastEmployment.DateTo.Value.ToString("yyyyMMdd");

                if (StringUtility.IsSamordningsnummer(employee.SocialSec))
                {
                    string address = null;
                    string addressCo = null;
                    string postalCode = null;
                    string postalAddress = null;
                    string postalCountry = null;

                    var contactAddressItems = ContactManager.GetContactAddressItems(entities, employee.ContactPerson?.ActorContactPersonId ?? 0, preLoadedContacts: contacts) ?? new List<ContactAddressItem>();
                    foreach (var item in contactAddressItems)
                    {
                        if (item.ContactAddressItemType == ContactAddressItemType.AddressDistribution)
                        {
                            address = item.Address;
                            addressCo = item.AddressCO;
                            postalCode = item.PostalCode;
                            postalAddress = item.PostalAddress;
                            postalCountry = item.Country;
                        }
                    }
                    if (StringUtility.IsSamordningsnummer(employee.SocialSec))
                    {
                        if (!string.IsNullOrEmpty(postalCountry))
                        {
                            switch (postalCountry)
                            {
                                case "Sverige":
                                case "Sweden":
                                    postalCountry = "SE";
                                    break;
                                case "Norge":
                                case "Norway":
                                    postalCountry = "NO";
                                    break;
                                case "Danmark":
                                case "Denmark":
                                    postalCountry = "DK";
                                    break;
                                case "Finland":
                                case "Suomi":
                                    postalCountry = "FI";
                                    break;
                                default:
                                    postalCountry = "SE";
                                    break;
                            }
                        }

                        foraEmployment.address = new ForaAddress()
                        {
                            coAddress = string.IsNullOrEmpty(addressCo) ? null : addressCo,
                            streetAddress = string.IsNullOrEmpty(address) ? null : address,
                            streetAddress2 = null,
                            postalCode = string.IsNullOrEmpty(postalCode) ? null : postalCode,
                            city = string.IsNullOrEmpty(postalAddress) ? null : postalAddress,
                            countryCode = string.IsNullOrEmpty(postalCountry) ? "SE" : postalCountry,
                        };
                    }
                }
                else
                    foraEmployment.address = null;

                //On aldrig skickad förut så ska man skicka, annars skicka endast om det är ändringar inom period (periodens start till löneperiodsstopp
                var history = GeneralManager.GetLastEventHistory(entities, TermGroup_EventHistoryType.ForaFokId, employee.EmployeeId, Company.ActorCompanyId);

                DateTime? lastDate = null;
                foreach (var timePeriod in selectedTimePeriods.OrderBy(o => o.StartDate).ThenBy(o => o.ExtraPeriod ? 1 : 0))
                {
                    var inPeriod = filteredEmployeeTransactions.Where(t => t.TimePeriodId == timePeriod.TimePeriodId || (!t.TimePeriodId.HasValue && t.Date >= timePeriod.StartDate && t.Date <= timePeriod.StopDate)).ToList();

                    if (inPeriod.Any())
                    {
                        if (!lastDate.HasValue)
                            lastDate = selectedTimePeriods.OrderByDescending(o => o.StartDate).FirstOrDefault()?.StopDate;
                        var payMonth = timePeriod.PaymentDate.Value.ToString("yyyyMM");
                        var employeeIdString = StringUtility.SocialSecYYYYMMDDXXXX(employee.SocialSec);
                        var existingPayroll = foraReport.payrolls?.FirstOrDefault(f => f.employeeId == employeeIdString && f.payMonth == payMonth);

                        if (existingPayroll == null)
                        {
                            ForaPayroll foraPayroll = new ForaPayroll()
                            {
                                employeeId = employeeIdString,
                                firstName = employee.FirstName,
                                lastName = employee.LastName,
                                payMonth = payMonth,
                                payAmount = Convert.ToInt32(inPeriod.Sum(t => t.Amount)),
                            };

                            foraReport.payrolls.Add(foraPayroll);
                        }
                        else
                        {
                            existingPayroll.payAmount += Convert.ToInt32(inPeriod.Sum(t => t.Amount));
                        }
                    }

                    var fokCodeSettings = employeeSettingsFokId.DistinctByPK().FilterByEmployee(employee.EmployeeId).FilterByDates(timePeriod.StartDate, timePeriod.PayrollStopDate ?? timePeriod.StopDate);
                    var foraMarkupIsSet = false;
                    List<ForaMarkup> alreadyReportedMarkups = new List<ForaMarkup>();
                    foreach (var fokCodeSetting in fokCodeSettings)
                    {
                        ForaMarkup foraMarkup = null;
                        if (fokCodeSetting != null)
                        {
                            DateTime markupStartDate = fokCodeSetting.ValidFromDate ?? employee.GetLastEmployment(lastDate)?.DateFrom ?? lastDate ?? DateTime.MinValue;

                            var existingMarkup = foraEmployment.markups?.FirstOrDefault(f => f.startDate == markupStartDate.ToString("yyyyMMdd") && f.fokId == fokCodeSetting.StrData);
                            if (existingMarkup == null)
                            {
                                foraMarkup = new ForaMarkup()
                                {
                                    fokId = fokCodeSetting.StrData,
                                    startDate = markupStartDate.ToString("yyyyMMdd"),
                                    StartDate = markupStartDate,
                                };

                                if (history == null || history.DateData == null || history.DateData.Value != markupStartDate || history.StrData != foraMarkup.fokId)
                                    foraEmployment.markups.Add(foraMarkup);
                                else
                                    alreadyReportedMarkups.Add(foraMarkup);
                            }

                            foraMarkupIsSet = true;
                        }
                    }

                    if (!foraMarkupIsSet) // If no fok code is set, check if there is a payroll group with a fok code
                    {
                        var getlastEmployment = employee.Employment.GetEmployment(lastDate);
                        if (getlastEmployment != null)
                        {
                            PayrollGroup payrollGroup = employee.GetPayrollGroup(getlastEmployment.DateFrom, payrollGroups: payrollGroups);
                            if (payrollGroup != null)
                            {
                                var startDate = getlastEmployment.DateFrom;
                                string fokCodeFromPayrollGroup = payrollGroup?.PayrollGroupSetting.FirstOrDefault(f => f.Type == (int)PayrollGroupSettingType.ForaFok)?.StrData;
                                if (!string.IsNullOrEmpty(fokCodeFromPayrollGroup) && startDate.HasValue)
                                {
                                    var existingMarkup = foraEmployment.markups?.FirstOrDefault(f => startDate.Value <= lastDate.Value && f.startDate == startDate.Value.ToString("yyyyMMdd") && f.fokId == fokCodeFromPayrollGroup);

                                    if (existingMarkup == null)
                                    {
                                        var foraMarkup = new ForaMarkup()
                                        {
                                            fokId = fokCodeFromPayrollGroup,
                                            startDate = startDate.Value.ToString("yyyyMMdd"),
                                            StartDate = startDate.Value,
                                        };
                                        if (history == null || history.DateData == null || history.DateData.Value != startDate.Value || history.StrData != foraMarkup.fokId)
                                            foraEmployment.markups.Add(foraMarkup);
                                        else
                                            alreadyReportedMarkups.Add(foraMarkup);
                                    }
                                }
                            }
                        }
                    }

                    EventHistory historyChoice1 = null;
                    EventHistory historyChoice2 = null;

                    var markups = alreadyReportedMarkups;
                    markups.AddRange(foraEmployment.markups);

                    foreach (var foraMarkup in markups)
                    {
                        if (foraMarkup.StartDate != DateTime.MinValue)
                        {
                            var fokParameterChoices = employeeSettingsOption1.DistinctByPK().FilterByEmployee(employee.EmployeeId).FilterByDates(foraMarkup.StartDate, DateTime.MaxValue);

                            foreach (var fokParameterChoice in fokParameterChoices.Where(w => !string.IsNullOrEmpty(w.StrData)))
                            {
                                historyChoice1 = historyChoice1 ?? GeneralManager.GetLastEventHistory(entities, TermGroup_EventHistoryType.ForaChoice1, employee.EmployeeId, Company.ActorCompanyId);
                                if (foraMarkup.fokParameterChoices == null)
                                    foraMarkup.fokParameterChoices = new List<ForaFokparameterchoice>();

                                if (historyChoice1 == null || historyChoice1.DateData == null || historyChoice1.DateData.Value != foraMarkup.StartDate || historyChoice1.StrData != fokParameterChoice.StrData)
                                {
                                    foraMarkup.fokParameterChoices.Add(new ForaFokparameterchoice()
                                    {
                                        value = fokParameterChoice.StrData,
                                        endDate = fokParameterChoice.ValidToDate.HasValue ? fokParameterChoice.ValidToDate.Value.ToString("yyyyMMdd") : null,
                                        ChoiceId = 1,
                                        EndDate = fokParameterChoice.ValidToDate,
                                    });

                                    if (!foraEmployment.markups.Contains(foraMarkup))
                                        foraEmployment.markups.Add(foraMarkup);
                                }
                            }
                            var fokParameterChoices2 = employeeSettingsOption2.DistinctByPK().FilterByEmployee(employee.EmployeeId).FilterByDates(foraMarkup.StartDate, DateTime.MaxValue);
                            foreach (var fokParameterChoice in fokParameterChoices2.Where(w => !string.IsNullOrEmpty(w.StrData)))
                            {
                                historyChoice2 = historyChoice2 ?? GeneralManager.GetLastEventHistory(entities, TermGroup_EventHistoryType.ForaChoice2, employee.EmployeeId, Company.ActorCompanyId);

                                if (foraMarkup.fokParameterChoices == null)
                                    foraMarkup.fokParameterChoices = new List<ForaFokparameterchoice>();

                                foraMarkup.fokParameterChoices.Add(new ForaFokparameterchoice()
                                {
                                    value = fokParameterChoice.StrData,
                                    endDate = fokParameterChoice.ValidToDate.HasValue ? fokParameterChoice.ValidToDate.Value.ToString("yyyyMMdd") : null,
                                    ChoiceId = 2,
                                    EndDate = fokParameterChoice.ValidToDate,
                                });

                                if (!foraEmployment.markups.Contains(foraMarkup))
                                    foraEmployment.markups.Add(foraMarkup);
                            }
                        }
                    }
                }

                if (setAsFinal)
                {
                    foreach (var markup in foraEmployment.markups)
                    {
                        if (markup.StartDate != DateTime.MinValue)
                        {
                            events.Add(new EventHistoryDTO(
                                Company.ActorCompanyId,
                                TermGroup_EventHistoryType.ForaFokId,
                                SoeEntityType.Employee,
                                employee.EmployeeId,
                                userId: UserId,
                                stringValue: markup.fokId,
                                dateValue: markup.StartDate
                            ));
                        }

                        // Process markup fok parameter choices if available
                        if (markup.fokParameterChoices != null)
                        {
                            foreach (var choice in markup.fokParameterChoices)
                            {
                                var eventHistoryType = choice.ChoiceId == 1 ? TermGroup_EventHistoryType.ForaChoice1 : TermGroup_EventHistoryType.ForaChoice2;

                                events.Add(new EventHistoryDTO(
                                    Company.ActorCompanyId,
                                    eventHistoryType,
                                    SoeEntityType.Employee,
                                    employee.EmployeeId,
                                    userId: UserId,
                                    stringValue: choice.value,
                                    dateValue: choice.EndDate
                                ));
                            }
                        }
                    }
                }
                if (foraEmployment.markups != null && foraEmployment.markups.Any())
                    foraReport.employments.Add(foraEmployment);
            }

            #endregion

            if (events.Any())
                GeneralManager.SaveEventHistories(entities, events, Company.ActorCompanyId);

            #region Create File


            var directory = Directory.CreateDirectory(ConfigSettings.SOE_SERVER_DIR_TEMP_REPORT_PHYSICAL + Guid.NewGuid().ToString());
            string fileName = IOUtil.FileNameSafe("Fora" + "_" + Company.Name + "_" + DateTime.Now.ToString("yyyyMMddhhmmss") + "_" + guid.ToString());
            string filePath = StringUtility.GetValidFilePath(directory.FullName) + fileName + ".txt";

            try
            {
                if (!exportExcelFile)
                {
                    //Make sure that empty fields are not serialized
                    JsonSerializerSettings settings = new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        Formatting = Formatting.Indented,
                    };

                    File.WriteAllText(filePath, JsonConvert.SerializeObject(foraJsonMonthly, settings), Encoding.GetEncoding(1252));
                }
                else

                    ExcelMatrix.SaveExcelFile(filePath, ConvertToMatrixResult(foraJsonMonthly), "Fora");
            }
            catch (Exception ex)
            {
                SysLogManager.AddSysLog(ex, log4net.Core.Level.Error);
            }

            #endregion

            return filePath;
        }

        #endregion

        private List<TimePayrollStatisticsSmallDTO> FilterTransactions(List<Employee> employees, List<TimePayrollStatisticsSmallDTO> transactions)
        {
            return transactions.Where(t => PayrollRulesUtil.isFora(t.PensionCompany) && employees.Select(s => s.EmployeeId).Contains(t.EmployeeId)).ToList();
        }

        private MatrixResult ConvertToMatrixResult(ForaJsonMonthly foraJsonMonthly)
        {
            MatrixResult result = new MatrixResult
            {
                MatrixDefinition = new MatrixDefinition()
            };
            result.MatrixDefinition.MatrixDefinitionColumns.AddRange(GetMatrixDefinitionColumns());
            int row = 1;

            foreach (var employee in foraJsonMonthly.reports.SelectMany(s => s.payrolls))
            {

                result.MatrixFields.Add(new MatrixField(row, result.MatrixDefinition.MatrixDefinitionColumns.First(f => f.Field == "Personnummer").Key, employee.employeeId));
                result.MatrixFields.Add(new MatrixField(row, result.MatrixDefinition.MatrixDefinitionColumns.First(f => f.Field == "Namn").Key, employee.firstName + " " + employee.lastName));
                result.MatrixFields.Add(new MatrixField(row, result.MatrixDefinition.MatrixDefinitionColumns.First(f => f.Field == "Period").Key, employee.payMonth));
                result.MatrixFields.Add(new MatrixField(row, result.MatrixDefinition.MatrixDefinitionColumns.First(f => f.Field == "Lön").Key, employee.payAmount.ToString()));
                var employment = foraJsonMonthly.reports.SelectMany(s => s.employments).FirstOrDefault(f => f.employeeId == employee.employeeId);

                // FokId with Start Date
                string fokIds = employment?.markups != null ? string.Join(", ", employment.markups.Select(m => $"Start: {m.startDate} Value: {m.fokId}")) : "";
                result.MatrixFields.Add(new MatrixField(row, result.MatrixDefinition.MatrixDefinitionColumns.First(f => f.Field == "FokId").Key, fokIds));

                // Parameters with optional end date
                string parameters = employment?.markups != null
                    ? string.Join(", ", employment.markups
                        .Where(m => m.fokParameterChoices != null) // Check if fokParameterChoices is not null
                        .SelectMany(m => m.fokParameterChoices)
                        .Select(s => $"{s.value}" + (s.endDate != null ? $" Slutdatum YYYYmmDD: {s.endDate}" : "")))
                    : "";
                result.MatrixFields.Add(new MatrixField(row, result.MatrixDefinition.MatrixDefinitionColumns.First(f => f.Field == "Parameter").Key, parameters));

                row++;
            }

            return result;
        }

        private List<MatrixDefinitionColumn> GetMatrixDefinitionColumns()
        {
            List<MatrixDefinitionColumn> matrixDefinitionColumns = new List<MatrixDefinitionColumn>
            {
                new MatrixDefinitionColumn() { Field = "Personnummer", Key = Guid.NewGuid(), MatrixDataType = MatrixDataType.String, Title = "Personnummer" },
                new MatrixDefinitionColumn() { Field = "Namn", Key = Guid.NewGuid(), MatrixDataType = MatrixDataType.String, Title = "Namn" },
                new MatrixDefinitionColumn() { Field = "Period", Key = Guid.NewGuid(), MatrixDataType = MatrixDataType.String, Title = "Period"},
                new MatrixDefinitionColumn() { Field = "Lön", Key = Guid.NewGuid(), MatrixDataType = MatrixDataType.String, Title = "Lön" },
                new MatrixDefinitionColumn() { Field = "FokId", Key = Guid.NewGuid(), MatrixDataType = MatrixDataType.String, Title = "FokId" },
                new MatrixDefinitionColumn() { Field = "Parameter", Key = Guid.NewGuid(), MatrixDataType = MatrixDataType.String, Title = "Parameter" },
            };

            return matrixDefinitionColumns;
        }
    }

    public class ForaJsonMonthly
    {
        /// <summary>
        /// Obligatorisk
        /// String (40)
        /// SoftOne
        /// </summary>
        public string source { get; set; }
        public List<ForaReport> reports { get; set; } = new List<ForaReport>();
    }

    public class ForaReport
    {
        /// <summary>
        /// Ej obligatorisk
        /// String (40)
        /// Unikt id för att underlätta felrapporten när filen laddasupp .Ex “001”
        /// </summary>
        public string refId { get; set; }
        /// <summary>
        /// Ej obligatorisk
        /// String (10)
        /// XXXXXXXXXX
        /// 2265601712
        /// Endast 1 organisation per fil
        /// </summary>
        public string organizationNumber { get; set; }
        public List<ForaEmployment> employments { get; set; } = new List<ForaEmployment>();
        /// <summary>
        /// Ej obligatorisk
        /// </summary>
        public List<ForaPayroll> payrolls { get; set; } = new List<ForaPayroll>();
    }

    public class ForaEmployment
    {
        /// <summary>
        /// Obligatorisk
        /// String (12)
        /// YYYYMMDDXXXX
        /// 195408012230 (pnr) 200107614321 (samordnr)
        /// </summary>
        public string employeeId { get; set; }
        /// <summary>
        /// Obligatorisk
        /// String(40)
        /// </summary>
        public string firstName { get; set; }
        /// <summary>
        /// Obligatorisk
        /// String (40)
        /// </summary>
        public string lastName { get; set; }
        /// <summary>
        /// Adress ska anges för individer med samordningsnummer
        /// </summary>
        public ForaAddress address { get; set; }
        /// <summary>
        /// Obligatorisk
        /// String (8)
        /// YYYYMMDD
        /// Exempel 20240101
        /// </summary>
        public string employmentEndDate { get; set; }
        public List<ForaMarkup> markups { get; set; } = new List<ForaMarkup>();
    }

    public class ForaAddress
    {
        /// <summary>
        /// Ej obligatorisk
        /// String(40)
        /// </summary>
        public string coAddress { get; set; }
        /// <summary>
        /// Obligatorisk för individer med samordningsnummer
        /// String(60)
        /// </summary>
        public string streetAddress { get; set; }
        /// <summary>
        /// Ej obligatorisk
        /// String (40)
        /// </summary>
        public string streetAddress2 { get; set; }
        /// <summary>
        /// Obligatorisk för individer med samordningsnummer
        /// Lämnas tom om utländsk adress
        /// String (10)
        /// 11134
        /// </summary>
        public string postalCode { get; set; }
        /// <summary>
        /// Obligatorisk för individer med samordningsnummer
        /// String (40)
        /// </summary>
        public string city { get; set; }
        /// <summary>
        /// Obligatorisk för individer med samordningsnummer
        /// String (3)
        /// Exempel: SE
        /// </summary>
        public string countryCode { get; set; }
    }

    public class ForaMarkup
    {
        [JsonIgnore]
        public DateTime StartDate { get; set; }
        /// <summary>
        /// Obligatorisk
        /// String (9)
        /// Exempel FOK301048
        /// </summary>
        public string fokId { get; set; }
        /// <summary>
        /// Obligatorisk
        /// String (8)
        /// YYYYMMDD
        /// Exempel 20240101
        /// </summary>
        public string startDate { get; set; }
        /// <summary>
        /// Ej obligatorisk
        /// </summary>       

        public List<ForaFokparameterchoice> fokParameterChoices { get; set; }
    }

    public class ForaFokparameterchoice
    {
        [JsonIgnore]
        public int ChoiceId { get; set; }
        [JsonIgnore]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Obligatorisk
        /// String (16)
        /// Exempel TILLVAL_ASL65
        /// </summary>
        public string value { get; set; }
        /// <summary>
        /// Ej obligatorisk
        /// String(8)
        /// YYYYMMDD
        /// Exempel 20240101
        /// </summary>
        public string endDate { get; set; }
    }

    public class ForaPayroll
    {
        /// <summary>
        /// Obligatorisk
        /// String (12)
        /// YYYYMMDDXXXX
        /// 195408012230 (pnr) 200107614321 (samordnr)
        /// </summary>
        public string employeeId { get; set; }
        /// <summary>
        /// Obligatorisk
        /// String(40)
        /// </summary>
        public string firstName { get; set; }
        /// <summary>
        /// Obligatorisk
        /// String (40)
        /// </summary>
        public string lastName { get; set; }
        /// <summary>
        /// Obligatorisk
        /// String (6)
        /// YYYYMM
        /// 202403
        /// </summary>
        public string payMonth { get; set; }
        /// <summary>
        /// Obligatorisk
        /// String (8)
        /// Exempel 72000
        /// </summary>
        public int payAmount { get; set; }
        /// <summary>
        /// Ej obligatorisk
        /// </summary>
        public List<ForaAttributablepay> attributablePays { get; set; }
    }

    public class ForaAttributablepay
    {
        /// <summary>
        /// Obligatorisk
        /// String (6)
        /// YYYYMM
        /// 202406
        /// </summary>
        public string attributablePayMonth { get; set; }
        /// <summary>
        /// Obligatorisk
        /// Integer (8)
        /// Exempel 20000
        /// </summary>
        public int attributablePayAmount { get; set; }
    }
}
