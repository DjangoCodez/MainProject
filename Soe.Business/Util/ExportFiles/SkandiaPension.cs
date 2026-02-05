using SoftOne.Soe.Business.Core;
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
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using static SoftOne.Soe.Business.Util.ExportFiles.SkandiaPensionCompanyDTO;

namespace SoftOne.Soe.Business.Util.ExportFiles
{
    public class SkandiaPension : ExportFilesBase
    {
        private readonly XNamespace ns = "http://schemas.skandia.se/Inrapportering_anstallningsuppgifter";
        private readonly XNamespace namespaceTns = "http://schemas.skandia.se/Inrapportering_anstallningsuppgifter";
        private readonly XNamespace namespaceXsi = "http://www.w3.org/2001/XMLSchema-instance";
        private readonly XNamespace namespaceSchemaLocation = "http://schemas.skandia.se/Inrapportering_anstallningsuppgifter";

        #region Ctor
      
        public SkandiaPension(ParameterObject parameterObject, CreateReportResult reportResult) : base(parameterObject, reportResult)
        {
            this.reportResult = reportResult;
        }

        #endregion

        public string CreateSkandiaPensionFile(CompEntities entities, bool exportExcelFile = false)
        {
            #region Init

            #endregion

            #region Init repository

            PersonalDataEmployeeReportRepository personalDataRepository = new PersonalDataEmployeeReportRepository(parameterObject, reportResult);
            #endregion

            #region Prereq

            var company = CompanyManager.GetCompany(reportResult.ActorCompanyId);
            int year = 0;

            if (!TryGetIdsFromSelection(reportResult, out List<int> selectionTimePeriodIds, "periods"))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionTimePeriodIds, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return null;
            if (!TryGetIdFromSelection(reportResult, out int? skandiaPensionReportType, key: "skandiaPensionReportType"))
                return null;

            if (employees == null)
                employees = EmployeeManager.GetAllEmployeesByIds(entities, base.ActorCompanyId, selectionEmployeeIds, loadEmployment: true);

            var rapportType = GetText((int)skandiaPensionReportType, TermGroup.SkandiaPensionReportType, 1);
            var skandiaPensionDTO = GetSkandiaPensionCompanyDTO(entities, reportResult.ActorCompanyId, reportResult.RoleId, selectionTimePeriodIds, employees, selectionEmployeeIds, year, (TermGroup_SkandiaPensionReportType)skandiaPensionReportType);

            #endregion

            #region Create File
            
            string output = !exportExcelFile ? skandiaPensionDTO.GetFile() : string.Empty;
            var directory = Directory.CreateDirectory(ConfigSettings.SOE_SERVER_DIR_TEMP_REPORT_PHYSICAL + Guid.NewGuid().ToString());
            string fileName = IOUtil.FileNameSafe("SkandiaPension" + "_" + company.Name + "_" + DateTime.Now.ToString("yyyyMMddhhmmss"));
            string filePath = StringUtility.GetValidFilePath(directory.FullName) + fileName + (exportExcelFile ? ".xlsx" : ".xml");
            int? empDag = null;

            try
            {
                if (exportExcelFile)
                    ExcelMatrix.SaveExcelFile(filePath, skandiaPensionDTO.ConvertToMatrixResult(), "SkandiaPension");
                else
                    File.WriteAllText(filePath, output, Encoding.GetEncoding("ISO-8859-1"));


            }
            catch (Exception ex)
            {
                SysLogManager.AddSysLog(ex, log4net.Core.Level.Error);
            }

            #endregion

            if (!exportExcelFile)
            {
                #region Init document

                XDocument document = XmlUtil.CreateDocument();
                document.Declaration = new XDeclaration("1.0", "UTF-8", "true");

                #endregion

                #region Create root with namespaces

                XElement root = new XElement(ns + "InrapporteringAnställningsuppgifter");
                root.Add(new XAttribute("xmlns", namespaceTns));
                root.Add(new XAttribute(XNamespace.Xmlns + "tns", namespaceTns));
                root.Add(new XAttribute(XNamespace.Xmlns + "xsi", namespaceXsi));
                root.Add(new XAttribute(namespaceXsi + "schemaLocation", namespaceSchemaLocation));
                
                #endregion

                #region header

                XElement element = new XElement(ns + rapportType);
                if (skandiaPensionDTO.SkandiaPensionReportType == TermGroup_SkandiaPensionReportType.StartReporting)
                {
                    element.Add(new XAttribute(ns + "avrapporteringsdatum", skandiaPensionDTO.Avrapporteringsdatum.ToString("yyyy-MM-dd")));
                }
                else
                {
                    element.Add(new XAttribute(ns + "avrapporteringsår", skandiaPensionDTO.Avavrapporteringsår));
                }
                element.Add(new XAttribute(ns + "datumRapportSkapad", DateTime.Today.ToString("yyyy-MM-dd")));
                element.Add(new XAttribute(ns + "antalRapporteradeArbetstagare", skandiaPensionDTO.SkandiaPensionEmployees.Count));

                XElement arbetsgivare = new XElement(ns + "Arbetsgivare");
                arbetsgivare.Add(new XAttribute(ns + "organisationsnummer", skandiaPensionDTO.Organisationsnummer));

                element.Add(arbetsgivare);

                #endregion

                #region Employees

                foreach (var employee in skandiaPensionDTO.SkandiaPensionEmployees)
                {
                    XElement emp = new XElement(ns + rapportType + "Arbetstagare");
                    emp.Add(new XAttribute(ns + "personnummer", employee.Personnummer));
                    emp.Add(new XAttribute(ns + "sorteringsbegreppArbetsgivare", employee.SorteringsbegreppArbetsgivare));
                    empDag = null;

                    foreach (var row in employee.SkandiaPensionEmployeeRows.OrderBy(o => o.RadTyp))
                    {
                        XElement empRow = null;
                        if (row.RadTyp == RadTyp.Normal && skandiaPensionDTO.SkandiaPensionReportType == TermGroup_SkandiaPensionReportType.StartReporting)
                        {
                            empRow = new XElement(ns + "Anställning");
                            empRow.Add(new XAttribute(ns + "anställningsdatum", row.Datum.ToString("yyyy-MM-dd")));

                            if (Convert.ToInt32(row.Belopp) > 0 && row.Datum >= skandiaPensionDTO.Avrapporteringsdatum.AddYears(-3))
                                empRow.Add(new XAttribute(ns + "överenskommenLön", Convert.ToInt32(row.Belopp)));

                            XElement empKategori = new XElement(ns + "Anställningskategori", row.Anställningskategori);
                            XElement empPension = new XElement(ns + "Pensionsbestämmelse", row.Pensionsbestämmelse);

                            empRow.Add(empKategori);
                            empRow.Add(empPension);
                        }
                        else if (row.RadTyp == RadTyp.NyAnställning && skandiaPensionDTO.SkandiaPensionReportType != TermGroup_SkandiaPensionReportType.StartReporting)
                        {
                            empRow = new XElement(ns + "NyAnställning");
                            empRow.Add(new XAttribute(ns + "anställningsdatum", row.Datum.ToString("yyyy-MM-dd")));
                            if (Convert.ToInt32(row.Belopp) > 0)
                                empRow.Add(new XAttribute(ns + "överenskommenLön", Convert.ToInt32(row.Belopp)));

                            XElement empKategori = new XElement(ns + "Anställningskategori", row.Anställningskategori);
                            XElement empPension = new XElement(ns + "Pensionsbestämmelse", row.Pensionsbestämmelse);

                            empRow.Add(empKategori);
                            empRow.Add(empPension);

                        }
                        else if (row.RadTyp == RadTyp.ByteAnställningskategori)
                        {
                            empRow = new XElement(ns + "ByteAnställningskategori");
                            empRow.Add(new XAttribute(ns + "datumAnställningskategoriFrOM", row.Datum.ToString("yyyy-MM-dd")));

                            if (Convert.ToInt32(row.Belopp) > 0 &&
                                ((skandiaPensionDTO.SkandiaPensionReportType == TermGroup_SkandiaPensionReportType.StartReporting && row.Datum >= skandiaPensionDTO.Avrapporteringsdatum.AddYears(-3)) ||
                                skandiaPensionDTO.SkandiaPensionReportType != TermGroup_SkandiaPensionReportType.StartReporting))
                            {
                                empRow.Add(new XAttribute(ns + "överenskommenLön", Convert.ToInt32(row.Belopp)));
                            }
                            XElement empKategori = new XElement(ns + "Anställningskategori", row.Anställningskategori);
                            empRow.Add(empKategori);
                        }
                        else if (row.RadTyp == RadTyp.ÄndradÖverenskommenLön && skandiaPensionDTO.SkandiaPensionReportType != TermGroup_SkandiaPensionReportType.StartReporting)
                        {
                            empRow = new XElement(ns + "ÄndradÖverenskommenLön");
                            empRow.Add(new XAttribute(ns + "datumÄndradÖverenskommenLönFrOM", row.Datum.ToString("yyyy-MM-dd")));
                            empRow.Add(new XAttribute(ns + "ändradÖverenskommenLön", Convert.ToInt32(row.Belopp)));

                        }
                        else if (row.RadTyp == RadTyp.Avgång)
                        {
                            empRow = new XElement(ns + "Avgång");
                            empRow.Add(new XAttribute(ns + "avgångsdatum", row.Datum.ToString("yyyy-MM-dd")));

                        }
                        emp.Add(empRow);

                    }
                  
                    if (skandiaPensionDTO.SkandiaPensionReportType != TermGroup_SkandiaPensionReportType.InterimReport && skandiaPensionDTO.SkandiaPensionReportType != TermGroup_SkandiaPensionReportType.StartReporting) {
                        foreach (var row in employee.SkandiaPensionPeriodRows)
                        {
                            XElement empRow = new XElement(ns + "PensionsgrundandeLön");
                            empRow.Add(new XAttribute(ns + "lönebelopp", Convert.ToInt32(row.Lönebelopp)));
                            empRow.Add(new XAttribute(ns + "datumLönFrOM", row.Datum.ToString("yyyy-MM-dd")));

                            XElement empPension = new XElement(ns + "Pensionsbestämmelse", row.Pensionsbestämmelse);
                            XElement empUnder = new XElement(ns + "AvgiftssatsUnderTak", Math.Round(row.AvgiftssatsUnderTak, 2).ToString().Replace(",", "."));
                            XElement empÖver = new XElement(ns + "AvgiftssatsÖverTak", Math.Round(row.AvgiftssatsÖverTak, 2).ToString().Replace(",", "."));
                            empRow.Add(empPension);
                            empRow.Add(empUnder);
                            empRow.Add(empÖver);
                            if (!row.Månadslön)
                            {
                                empDag = empDag != null ? empDag + row.AntalDagarTimanställning : row.AntalDagarTimanställning;
                            }
                            emp.Add(empRow);

                        }

                        if (employee.SkandiaPensionLeaveRows.Any())
                        {
                            foreach (var row in employee.SkandiaPensionLeaveRows)
                            {
                                XElement leaveRow = new XElement(ns + "Tjänstledighetsperiod");
                                leaveRow.Add(new XAttribute(ns + "datumEjPensionsgrundandePeriodFrOM", row.DatumEjPensionsgrundandePeriodFrOM.ToString("yyyy-MM-dd")));
                                leaveRow.Add(new XAttribute(ns + "datumEjPensionsgrundandePeriodTOM", row.DatumEjPensionsgrundandePeriodTOM.ToString("yyyy-MM-dd")));
                                emp.Add(leaveRow);
                            }

                        }
                        if (empDag != null)
                        {
                            XElement empDagar = new XElement(ns + "PensionsgrundandeTidTimanställning");
                            empDagar.Add(new XAttribute(ns + "antalDagarTimanställning", empDag));
                            emp.Add(empDagar);
                        }

                        element.Add(emp);
                    }
                    else
                    {
                        element.Add(emp);
                    }
                }

                root.Add(element);

                #endregion

                #region Validate

                document.Add(root);

                string validationMessage = ValidateXDocument(document);
                
                #endregion

                #region Create & save to file

            if (string.IsNullOrEmpty(validationMessage))
            {
                try
                {
                    document.Save(filePath);
                }
                catch (Exception ex)
                {
                    SysLogManager.AddSysLog(ex, log4net.Core.Level.Error);
                }
            }
            else
            {
                try
                {
                    validationMessage = "####################  VALIDATION ERROR IN FILE ###########################" + Environment.NewLine + Environment.NewLine + validationMessage + Environment.NewLine + Environment.NewLine + "####################  VALIDATION ERROR IN FILE ###########################" + Environment.NewLine + Environment.NewLine + document.ToString();
                    filePath = StringUtility.GetValidFilePath(directory.FullName) + "ErrorInFile_fileName" + ".xml";
                    File.WriteAllText(filePath, validationMessage);
                }
                catch (Exception ex)
                {
                    SysLogManager.AddSysLog(ex, log4net.Core.Level.Error);
                }
            }

                #endregion
            }

            #region Close repository

            personalDataRepository.GenerateLogs();

            #endregion

            return filePath; 
        }
       
        private string ValidateXDocument(XDocument document)
        {
            bool errors = false;
            string message = "";
            string path = ConfigSettings.SOE_SERVER_DIR_REPORT_EXTERNAL_SKANDIAPENSION_PHYSICAL + "Inrapportering_anstallningsuppgifter_230101_3.xsd";
            string file = File.ReadAllText(path);

            XmlSchemaSet schemas = new XmlSchemaSet();
            schemas.Add(namespaceTns.ToString(), XmlReader.Create(new StringReader(file)));
            document.Validate(schemas, (o, e) =>
            {
                message = e.Message;
                errors = true;
            });

            if (errors)
            {
                return message;
            }
            return "";
        }
        public SkandiaPensionCompanyDTO GetSkandiaPensionCompanyDTO(CompEntities entities, int actorCompanyId, int roleId, List<int> selectionTimePeriodIds, List<Employee> employees, List<int> selectionEmployeeIds, int year, TermGroup_SkandiaPensionReportType skandiaPensionReportType)
        {
            SkandiaPensionCompanyDTO dto = new SkandiaPensionCompanyDTO();

            List<TimePeriod> timePeriods = TimePeriodManager.GetTimePeriods(selectionTimePeriodIds, actorCompanyId).Where(x => x.PayrollStopDate.HasValue).ToList();
            if (timePeriods.IsNullOrEmpty())
                return dto;

            year = timePeriods.OrderBy(x => x.PayrollStartDate).LastOrDefault()?.PayrollStopDate.Value.Year ?? 0;
            Company company = CompanyManager.GetCompany(actorCompanyId);

            if (!timePeriods.Any())
                return dto;

            DateTime dateFrom = timePeriods.Min(x => x.PayrollStartDate.Value);
            DateTime dateTo = timePeriods.Max(x => x.PayrollStopDate.Value);
            DateTime splitDate_AKAP_KR = dateTo;

            if (skandiaPensionReportType == TermGroup_SkandiaPensionReportType.AnnualReport) //Årsrapport
            {
                dateFrom = new DateTime(year, 1, 1);
                dateTo = CalendarUtility.GetEndOfYear(dateFrom);
                splitDate_AKAP_KR = new DateTime(year, 7, 1);                                //AKAP_KR 2 delad Årsrapport     
            }
            else if (skandiaPensionReportType == TermGroup_SkandiaPensionReportType.SemiAnnualReport) //Halvårsrapport
            {
                if (dateFrom < new DateTime(year, 7, 1))
                {
                    dateFrom = new DateTime(year, 1, 1);
                    dateTo = new DateTime(year, 6, 30);
                }
                else
                {
                    dateFrom = new DateTime(year, 7, 1);
                    dateTo = CalendarUtility.GetEndOfYear(dateFrom);
                }
                splitDate_AKAP_KR = dateTo;
            }
            else if (skandiaPensionReportType == TermGroup_SkandiaPensionReportType.StartReporting) //Start Rapportering
            {
                dateTo = dateFrom;
                splitDate_AKAP_KR = dateFrom;
            }

            Dictionary<int, List<EmploymentPriceTypeChangeDTO>> priceTypeChanges = EmployeeManager.GetEmploymentPriceTypeChangesForEmployees(reportResult.ActorCompanyId, selectionEmployeeIds, dateFrom, dateTo);
            List<PayrollGroup> payrollGroups = PayrollManager.GetPayrollGroups(entities, reportResult.ActorCompanyId, true, true, loadSettings: true);
            var transactions = TimeTransactionManager.GetTimePayrollStatisticsSmallDTOs_new(entities, reportResult.ActorCompanyId, employees, selectionTimePeriodIds, setPensionCompany: true, ignoreAccounting: true);
            bool showSocialSec = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec, Permission.Readonly, roleId, actorCompanyId);
   
            var orgNr = company.OrgNr.Replace("-", "").Trim();
            string sortingConcept = SettingManager.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.PayrollExportSkandiaSortingConcept, 0, actorCompanyId, 0);
            int newEmployee = 0;
            int endEmployee = 0;
            dto.Avavrapporteringsår = year;
            dto.Avrapporteringsdatum = dateFrom;
            dto.Organisationsnummer = orgNr;
            dto.SkandiaPensionReportType = skandiaPensionReportType;

            foreach (var employee in employees)
            {
                var employeeTransactions = transactions.Where(e => e.EmployeeId == employee.EmployeeId).ToList();
                newEmployee = 0;
                endEmployee = 0;
             
                List<DateTime?> changeDateList = priceTypeChanges.ContainsKey(employee.EmployeeId) ? priceTypeChanges[employee.EmployeeId].OrderBy(o => o.FromDate).Select(s => s.FromDate).Distinct().ToList() : null;
                List<int> skipTransactionIds = employeeTransactions.Where(p => !p.IsScheduleTransaction).Select(t => t.TransactionId).ToList();
                
                employeeTransactions.AddRange(TimeTransactionManager.GetPayrollStartValuesAsTimePayrollStatisticsSmallDTOs(employee, ActorCompanyId, dateFrom.Year, skipTransactionIds: skipTransactionIds, setPensionCompany: true));
                employeeTransactions = FilterTransactions(employee, employeeTransactions);

                var firstEmployment = employee.Employment.GetFirstEmployment();
                var lastEmployment = employee.Employment.GetLastEmployment();

                DateTime? anstalldFrom = null;
                DateTime? anstalldTom = null;

                DateTime employmentStartDate = firstEmployment != null && firstEmployment.DateFrom.HasValue ? firstEmployment.DateFrom.Value : DateTime.MinValue;
                DateTime employmentEndDate = lastEmployment != null && lastEmployment.DateTo.HasValue ? lastEmployment.DateTo.Value : DateTime.MaxValue;

                if (employmentStartDate > dateFrom)
                {
                    anstalldFrom = employmentStartDate;
                    newEmployee = 1;
                }
                if (employmentEndDate < dateTo)
                {
                    anstalldTom = employmentEndDate;
                    endEmployee = 1;
                }

                SkandiaPensionEmployeeDTO employeeSkandia = new SkandiaPensionEmployeeDTO()
                {
                    SorteringsbegreppArbetsgivare = sortingConcept,
                    Personnummer = showSocialSec ? employee.SocialSec.Replace("-", "").Trim() : string.Empty,
                };

                PayrollGroup firstPayrollGroup = employee.GetPayrollGroup(anstalldFrom ?? dateFrom,  payrollGroups: payrollGroups);
                if (firstPayrollGroup == null)
                    continue;

                PayrollGroupSetting skandiaPensionStartDate = firstPayrollGroup.PayrollGroupSetting.FirstOrDefault(w => w.Type == (int)PayrollGroupSettingType.SkandiaPensionStartDate);
                PayrollGroupSetting skandiaPensionCategory = firstPayrollGroup.PayrollGroupSetting.FirstOrDefault(w => w.Type == (int)PayrollGroupSettingType.SkandiaPensionCategory);
                PayrollGroupSetting skandiaPensionType = firstPayrollGroup.PayrollGroupSetting.FirstOrDefault(w => w.Type == (int)PayrollGroupSettingType.SkandiaPensionType);
                PayrollGroupSetting avgiftssatsUnderTak = firstPayrollGroup.PayrollGroupSetting.FirstOrDefault(w => w.Type == (int)PayrollGroupSettingType.SkandiaPensionPercentBelowBaseAmount);
                PayrollGroupSetting avgiftssatsÖverTak = firstPayrollGroup.PayrollGroupSetting.FirstOrDefault(w => w.Type == (int)PayrollGroupSettingType.SkandiaPensionPercentAboveBaseAmount);
                PayrollGroupSetting skandiaPensionSalaryFormula = firstPayrollGroup.PayrollGroupSetting.FirstOrDefault(w => w.Type == (int)PayrollGroupSettingType.SkandiaPensionSalaryFormula);
                decimal firstPrice = (decimal)((skandiaPensionSalaryFormula?.IntData.HasValue ?? false) ? PayrollManager.EvaluatePayrollPriceFormula(entities, actorCompanyId, employee, firstEmployment, null, anstalldFrom ?? dateFrom, null, null, skandiaPensionSalaryFormula.IntData.Value)?.Amount : decimal.Zero);
                bool montlyPay = skandiaPensionCategory?.IntData != null && GetMonthlyPay(skandiaPensionCategory.IntData);

                if (skandiaPensionReportType == TermGroup_SkandiaPensionReportType.StartReporting)
                {
                    newEmployee = 0;
                    anstalldFrom = employmentStartDate;
                }
                else if (skandiaPensionStartDate != null && skandiaPensionStartDate.DateData.Value.Year == year && employmentStartDate <= skandiaPensionStartDate.DateData)
                    newEmployee = 1;

                SkandiaPensionEmployeeRows firstRow = new SkandiaPensionEmployeeRows()
                {
                    RadTyp = newEmployee == 1  ? RadTyp.NyAnställning : RadTyp.Normal,
                    Belopp = montlyPay ? firstPrice : 0,
                    Datum = anstalldFrom ?? dateFrom,
                    Anställningskategori = skandiaPensionCategory != null ? GetText((int)skandiaPensionCategory.IntData, TermGroup.SkandiaPensionCategory, 1) : string.Empty,
                    Pensionsbestämmelse = skandiaPensionType != null ? GetText((int)skandiaPensionType.IntData, TermGroup.SkandiaPensionType, 1) : string.Empty,
                    AvgiftssatsUnderTak = avgiftssatsUnderTak != null ? (decimal)avgiftssatsUnderTak.DecimalData : decimal.Zero,
                    AvgiftssatsÖverTak = avgiftssatsÖverTak != null ? (decimal)avgiftssatsÖverTak.DecimalData : decimal.Zero,
                    Månadslön = montlyPay,
                };

                employeeSkandia.SkandiaPensionPeriodRows.Add(firstRow);
                employeeSkandia.SkandiaPensionEmployeeRows.Add(firstRow);

                if (!changeDateList.IsNullOrEmpty() && changeDateList.Count > 1)
                {
                    int firstChange = 0;
                    foreach (var change in changeDateList)
                    {
                        if (firstChange == 0)
                        {
                            firstChange++;
                        }
                        else
                        {
                            Employment nextEmployment = employee.GetNearestEmployment(change.Value);
                            PayrollGroup nextPayrollGroup = employee.GetPayrollGroup(change.Value, payrollGroups: payrollGroups);
                            PayrollGroupSetting nextSkandiaPensionCategory = nextPayrollGroup.PayrollGroupSetting.FirstOrDefault(w => w.Type == (int)PayrollGroupSettingType.SkandiaPensionCategory);
                            PayrollGroupSetting nextSkandiaPensionSalaryFormula = nextPayrollGroup.PayrollGroupSetting.FirstOrDefault(w => w.Type == (int)PayrollGroupSettingType.SkandiaPensionSalaryFormula);
                            decimal nextPrice = (decimal)((nextSkandiaPensionSalaryFormula?.IntData.HasValue ?? false) ? PayrollManager.EvaluatePayrollPriceFormula(entities, actorCompanyId, employee, nextEmployment, null, change.Value, null, null, nextSkandiaPensionSalaryFormula.IntData.Value)?.Amount : decimal.Zero);
                            PayrollGroupSetting nextSkandiaPensionType = nextPayrollGroup.PayrollGroupSetting.FirstOrDefault(w => w.Type == (int)PayrollGroupSettingType.SkandiaPensionType);
                            PayrollGroupSetting nextAvgiftssatsUnderTak = nextPayrollGroup.PayrollGroupSetting.FirstOrDefault(w => w.Type == (int)PayrollGroupSettingType.SkandiaPensionPercentBelowBaseAmount);
                            PayrollGroupSetting nextAvgiftssatsÖverTak = nextPayrollGroup.PayrollGroupSetting.FirstOrDefault(w => w.Type == (int)PayrollGroupSettingType.SkandiaPensionPercentAboveBaseAmount);

                            montlyPay = nextSkandiaPensionCategory?.IntData != null && GetMonthlyPay(nextSkandiaPensionCategory.IntData);

                            if (nextSkandiaPensionCategory?.IntData != skandiaPensionCategory?.IntData) //New category
                            {
                                if (change.Value < dateFrom && employeeSkandia.SkandiaPensionEmployeeRows.Any(w => w.Datum == dateFrom && w.RadTyp == RadTyp.Normal))
                                {
                                    employeeSkandia.SkandiaPensionEmployeeRows.FirstOrDefault(w => w.Datum == dateFrom && w.RadTyp == RadTyp.Normal).RadTyp = RadTyp.ByteAnställningskategori;
                                }
                                else
                                {
                                    SkandiaPensionEmployeeRows row = new SkandiaPensionEmployeeRows()
                                    {
                                        RadTyp = RadTyp.ByteAnställningskategori,
                                        Belopp = montlyPay ? nextPrice : 0,
                                        Datum = change.Value < dateFrom ? dateFrom : change.Value,
                                        Anställningskategori = nextSkandiaPensionCategory != null ? GetText((int)nextSkandiaPensionCategory.IntData, TermGroup.SkandiaPensionCategory, 1) : string.Empty,
                                        Pensionsbestämmelse = nextSkandiaPensionType != null ? GetText((int)nextSkandiaPensionType.IntData, TermGroup.SkandiaPensionType, 1) : string.Empty,
                                        AvgiftssatsUnderTak = nextAvgiftssatsUnderTak != null ? (decimal)nextAvgiftssatsUnderTak.DecimalData : decimal.Zero,
                                        AvgiftssatsÖverTak = nextAvgiftssatsÖverTak != null ? (decimal)nextAvgiftssatsÖverTak.DecimalData : decimal.Zero,
                                        Månadslön = montlyPay,
                                    };

                                    employeeSkandia.SkandiaPensionEmployeeRows.Add(row);

                                    if (skandiaPensionReportType == TermGroup_SkandiaPensionReportType.AnnualReport && row.Datum > splitDate_AKAP_KR && employeeSkandia.SkandiaPensionPeriodRows.Any(w=> w.Datum != splitDate_AKAP_KR)) //Årsrapport
                                    {
                                        var lastRow = employeeSkandia.SkandiaPensionPeriodRows.LastOrDefault();
                                        SkandiaPensionEmployeeRows splitRow = new SkandiaPensionEmployeeRows()
                                        {
                                            RadTyp = lastRow.RadTyp,
                                            Belopp = lastRow.Belopp,
                                            Datum = splitDate_AKAP_KR,
                                            Anställningskategori = lastRow.Anställningskategori,
                                            Pensionsbestämmelse = lastRow.Pensionsbestämmelse,
                                            AvgiftssatsUnderTak = lastRow.AvgiftssatsUnderTak,
                                            AvgiftssatsÖverTak = lastRow.AvgiftssatsÖverTak,
                                            Månadslön = lastRow.Månadslön,
                                        };
                                        employeeSkandia.SkandiaPensionPeriodRows.Add(splitRow);
                                    }
                                    employeeSkandia.SkandiaPensionPeriodRows.Add(row);

                                }
                            }

                            if (montlyPay && firstPrice != nextPrice) //New Salary
                            {
                                if (change.Value < dateFrom && employeeSkandia.SkandiaPensionEmployeeRows.Any(w => w.Datum == dateFrom && w.RadTyp == RadTyp.Normal))
                                {
                                    employeeSkandia.SkandiaPensionEmployeeRows.FirstOrDefault(w => w.Datum == dateFrom && w.RadTyp == RadTyp.Normal).RadTyp = RadTyp.ÄndradÖverenskommenLön;
                                }
                                else
                                {
                                    SkandiaPensionEmployeeRows row = new SkandiaPensionEmployeeRows()
                                    {
                                        RadTyp = RadTyp.ÄndradÖverenskommenLön,
                                        Belopp = montlyPay ? nextPrice : 0,
                                        Datum = change.Value < dateFrom ? dateFrom : change.Value,
                                        Anställningskategori = nextSkandiaPensionCategory != null ? GetText((int)nextSkandiaPensionCategory.IntData, TermGroup.SkandiaPensionCategory, 1) : string.Empty,
                                        Pensionsbestämmelse = nextSkandiaPensionType != null ? GetText((int)nextSkandiaPensionType.IntData, TermGroup.SkandiaPensionType, 1) : string.Empty,
                                        AvgiftssatsUnderTak = nextAvgiftssatsUnderTak != null ? (decimal)nextAvgiftssatsUnderTak.DecimalData : decimal.Zero,
                                        AvgiftssatsÖverTak = nextAvgiftssatsÖverTak != null ? (decimal)nextAvgiftssatsÖverTak.DecimalData : decimal.Zero,
                                        Månadslön = montlyPay,
                                    };
                                   
                                    employeeSkandia.SkandiaPensionEmployeeRows.Add(row);
                                }
                            }

                        }

                    }
                    
                }
                if (skandiaPensionReportType == TermGroup_SkandiaPensionReportType.AnnualReport && employeeSkandia.SkandiaPensionPeriodRows.Count == 1 && employmentStartDate < splitDate_AKAP_KR && employmentEndDate > splitDate_AKAP_KR && employeeSkandia.SkandiaPensionPeriodRows.Any(w => w.Datum != splitDate_AKAP_KR)) //Årsrapport
                {
                    SkandiaPensionEmployeeRows row = new SkandiaPensionEmployeeRows()
                    {
                        RadTyp = newEmployee == 1 ? RadTyp.NyAnställning : RadTyp.Normal,
                        Belopp = montlyPay ? firstPrice : 0,
                        Datum = splitDate_AKAP_KR,
                        Anställningskategori = skandiaPensionCategory != null ? GetText((int)skandiaPensionCategory.IntData, TermGroup.SkandiaPensionCategory, 1) : string.Empty,
                        Pensionsbestämmelse = skandiaPensionType != null ? GetText((int)skandiaPensionType.IntData, TermGroup.SkandiaPensionType, 1) : string.Empty,
                        AvgiftssatsUnderTak = avgiftssatsUnderTak != null ? (decimal)avgiftssatsUnderTak.DecimalData : decimal.Zero,
                        AvgiftssatsÖverTak = avgiftssatsÖverTak != null ? (decimal)avgiftssatsÖverTak.DecimalData : decimal.Zero,
                        Månadslön = montlyPay,
                    };
                    employeeSkandia.SkandiaPensionPeriodRows.Add(row);
                }
                if(endEmployee == 1 && anstalldTom != null)
                {                    
                    SkandiaPensionEmployeeRows row = new SkandiaPensionEmployeeRows()
                    {
                        RadTyp = RadTyp.Avgång,
                        Datum = anstalldTom.Value,
                       
                    };
                    employeeSkandia.SkandiaPensionEmployeeRows.Add(row);
                }
               
                foreach (var row in employeeSkandia.SkandiaPensionPeriodRows)
                {
                  
                    DateTime rowDateFrom = row.Datum;
                    DateTime rowDateTo = row.Datum;

                    if (row != employeeSkandia.SkandiaPensionPeriodRows.LastOrDefault() && employeeSkandia.SkandiaPensionPeriodRows.Any(w => w.Datum > rowDateFrom))
                    {
                        rowDateTo = employeeSkandia.SkandiaPensionPeriodRows.FirstOrDefault(w => w.Datum > rowDateFrom).Datum.AddDays(-1);
                    }
                    else
                    {
                        rowDateTo = dateTo;
                    }
                    row.Lönebelopp = Convert.ToInt32(employeeTransactions.Where(w => w.PaymentDate >= rowDateFrom && w.PaymentDate <= rowDateTo).Sum(s => s.Amount));

                    if (!row.Månadslön)
                    {
                        row.AntalDagarTimanställning = employeeTransactions.Where(w => w.IsHourlySalary() && w.IsSupplementChargeBasis() && !w.IsScheduleTransaction && w.Date >= rowDateFrom && w.Date <= rowDateTo).GroupBy(s => s.Date).Count();
                    }

                    if (employeeTransactions.Any(w => PayrollRulesUtil.IsLeaveOfAbsence(w.SysPayrollTypeLevel1, w.SysPayrollTypeLevel2, w.SysPayrollTypeLevel3, w.SysPayrollTypeLevel4) && w.Date >= rowDateFrom && w.Date <= rowDateTo))
                    {
                        foreach(var transaction in employeeTransactions.Where(w => PayrollRulesUtil.IsLeaveOfAbsence(w.SysPayrollTypeLevel1, w.SysPayrollTypeLevel2, w.SysPayrollTypeLevel3, w.SysPayrollTypeLevel4)).OrderBy(o=> o.Date))
                        {
                            DateTime newDate = transaction.Date;
                            if (employeeSkandia.SkandiaPensionLeaveRows.Any() && employeeSkandia.SkandiaPensionLeaveRows.Any(w => w.DatumEjPensionsgrundandePeriodTOM == newDate.Date.AddDays(-1)) )
                            {
                                employeeSkandia.SkandiaPensionLeaveRows.FirstOrDefault(w => w.DatumEjPensionsgrundandePeriodTOM == newDate.Date.AddDays(-1)).DatumEjPensionsgrundandePeriodTOM = transaction.Date;
                            }
                            else if(!employeeSkandia.SkandiaPensionLeaveRows.Any() || (employeeSkandia.SkandiaPensionLeaveRows.LastOrDefault().DatumEjPensionsgrundandePeriodFrOM != newDate.Date && employeeSkandia.SkandiaPensionLeaveRows.LastOrDefault().DatumEjPensionsgrundandePeriodTOM != newDate.Date))
                            {
                                SkandiaPensionLeaveRows leaveRow = new SkandiaPensionLeaveRows()
                                {
                                    DatumEjPensionsgrundandePeriodFrOM = transaction.Date,
                                    DatumEjPensionsgrundandePeriodTOM = transaction.Date,
                                };
                                employeeSkandia.SkandiaPensionLeaveRows.Add(leaveRow);
                            }

                        }
                    }
                  
                }
                dto.SkandiaPensionEmployees.Add(employeeSkandia);
            }

            return dto;
        }

        private List<TimePayrollStatisticsSmallDTO> FilterTransactions(Employee employee, List<TimePayrollStatisticsSmallDTO> transactions)
        {
            return transactions.Where(t => PayrollRulesUtil.IsSkandia(t.PensionCompany) && t.EmployeeId == employee.EmployeeId).ToList();
        }

        private bool GetMonthlyPay(int? skandiaPensionCategory)
        {
            return skandiaPensionCategory == (int)TermGroup_SkandiaPensionCategory.AB_MÅNADSAVLÖNAD ||
                   skandiaPensionCategory == (int)TermGroup_SkandiaPensionCategory.BEA_MÅNADSAVLÖNAD ||
                   skandiaPensionCategory == (int)TermGroup_SkandiaPensionCategory.BRANDMAN_MÅNADSAVLÖNAD ||
                   skandiaPensionCategory == (int)TermGroup_SkandiaPensionCategory.MEDICINE_STUDERANDE_MÅNADSAVLÖNAD;
        }
                        

    }

    public class SkandiaPensionCompanyDTO
    {
        public SkandiaPensionCompanyDTO()
        {
            SkandiaPensionEmployees = new List<SkandiaPensionEmployeeDTO>();

        }
        public string GetFile()
        {

            return string.Empty;
        }
        public MatrixResult ConvertToMatrixResult()
        {
            MatrixResult result = new MatrixResult
            {
                MatrixDefinition = new MatrixDefinition()
            };

            return result;
        }

        public List<SkandiaPensionEmployeeDTO> SkandiaPensionEmployees { get; set; }
        public int Avavrapporteringsår { get; set; }
        public string Organisationsnummer { get; set; }
        public TermGroup_SkandiaPensionReportType SkandiaPensionReportType { get; set; }
        public DateTime Avrapporteringsdatum { get; set; }

        public class SkandiaPensionEmployeeDTO
        {
            public SkandiaPensionEmployeeDTO()
            {
                SkandiaPensionEmployeeRows = new List<SkandiaPensionEmployeeRows>();
                SkandiaPensionPeriodRows = new List<SkandiaPensionEmployeeRows>();
                SkandiaPensionLeaveRows = new List<SkandiaPensionLeaveRows>();
            }

            public string SorteringsbegreppArbetsgivare { get; set; }
            public string Personnummer { get; set; }
            public List<SkandiaPensionEmployeeRows> SkandiaPensionEmployeeRows { get; set; }
            public List<SkandiaPensionEmployeeRows> SkandiaPensionPeriodRows { get; set; }
            public List<SkandiaPensionLeaveRows> SkandiaPensionLeaveRows { get; set; }

        }

        public class SkandiaPensionEmployeeRows
        {
            public RadTyp RadTyp { get; set; }
            public DateTime Datum { get; set; }
            public string Anställningskategori { get; set; }
            public string Pensionsbestämmelse { get; set; }
            public decimal Belopp { get; set; }
            public decimal AvgiftssatsUnderTak { get; set; }
            public decimal AvgiftssatsÖverTak { get; set; }
            public int AntalDagarTimanställning { get; set; }
            public decimal Lönebelopp { get; set; }
            public bool Månadslön { get; set; }

        }

        public class SkandiaPensionLeaveRows
        {
            public DateTime DatumEjPensionsgrundandePeriodFrOM { get; set; }
            public DateTime DatumEjPensionsgrundandePeriodTOM { get; set; }
        }

    }
    public enum RadTyp
    {
        Normal = 0,
        NyAnställning = 1,
        ÄndradÖverenskommenLön = 2,
        ByteAnställningskategori = 3,
        Avgång = 4,
        PensionsgrundandeLön = 5

    }

}