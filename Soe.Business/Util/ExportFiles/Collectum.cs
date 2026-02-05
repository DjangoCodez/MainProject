using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.Reporting;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace SoftOne.Soe.Business.Util.ExportFiles
{
    public class Collectum : ExportFilesBase
    {
        #region Variables

        private readonly XNamespace namespaceGrundUppgifterITP = "http://collectum.se/granssnitt/grunduppgifterITP/3.0";
        private readonly XNamespace namespaceTyper = "http://collectum.se/typer/2.0";
        private readonly XNamespace namespacePaket = "http://collectum.se/paket/1.0";
        private readonly XNamespace namespaceArkitekturella = "http://collectum.se/arkitekturella/2.0";
        private readonly XNamespace namespaceGranssnitt = "http://collectum.se/granssnitt/pa/grunduppgifterITP/3.0";
        private readonly XNamespace namespaceNyanmalan = "http://collectum.se/paket/pa/nyanmalan/2.0";
        private readonly XNamespace namespaceFlyttAnstalldaInomKoncern = "http://collectum.se/paket/pa/flyttAnstalldaInomKoncern/3.0";
        private readonly XNamespace namespaceXsi = "http://www.w3.org/2001/XMLSchema-instance";
        private readonly XNamespace namespaceAvanmalan = "http://collectum.se/paket/pa/avanmalan/2.0";
        private readonly XNamespace namespaceLonandring = "http://collectum.se/paket/pa/loneandring/3.0";
        private bool showSocialSec = false;

        #endregion

        #region Ctor

        public Collectum(ParameterObject parameterObject, CreateReportResult reportResult) : base(parameterObject, reportResult)
        {
            this.reportResult = reportResult;
        }

        #endregion

        #region Public methods

        public string CreateCollectumFile(CompEntities entities)
        {
            #region Prereq

            if (!TryGetIdsFromSelection(reportResult, out List<int> selectionTimePeriodIds, "periods"))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionTimePeriodIds, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return null;

            TryGetDatesFromSelection(reportResult, selectionTimePeriodIds, out DateTime selectionDateFrom, out DateTime selectionDateTo, out var selectedTimePeriods, alwaysLoadPeriods: true);
            TryGetBoolFromSelection(reportResult, out bool setAsFinal, "setAsFinal");

            if (employees == null)
                employees = EmployeeManager.GetAllEmployeesByIds(reportResult.ActorCompanyId, selectionEmployeeIds, loadEmployment: true);

            this.showSocialSec = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId);

            DateTime timeStamp = DateTime.Now;
            int handelsenummer = 1;

            #endregion

            #region Init repository

            PersonalDataEmployeeReportRepository personalDataRepository = new PersonalDataEmployeeReportRepository(parameterObject, reportResult);

            #endregion

            #region Init document

            XDocument document = XmlUtil.CreateDocument();
            document.Declaration = new XDeclaration("1.0", "UTF-8", "yes");

            #endregion

            #region Create root with namespaces

            XElement root = new XElement(namespaceGrundUppgifterITP + "GrunduppgifterITP");
            root.Add(new XAttribute("version", "3.0.0.0"));
            root.Add(new XAttribute(XNamespace.Xmlns + "arkitekturella", namespaceArkitekturella));
            root.Add(new XAttribute(XNamespace.Xmlns + "granssnitt", namespaceGranssnitt));
            root.Add(new XAttribute(XNamespace.Xmlns + "avanmalan", namespaceAvanmalan));
            root.Add(new XAttribute(XNamespace.Xmlns + "flyttAnstalldaInomKoncern", namespaceFlyttAnstalldaInomKoncern));
            root.Add(new XAttribute(XNamespace.Xmlns + "loneandring", namespaceLonandring));
            root.Add(new XAttribute(XNamespace.Xmlns + "nyanmalan", namespaceNyanmalan));
            root.Add(new XAttribute(XNamespace.Xmlns + "typer", namespaceTyper));
            root.Add(new XAttribute(XNamespace.Xmlns + "paket", namespacePaket));
            root.Add(new XAttribute(XNamespace.Xmlns + "xsi", namespaceXsi));

            #endregion

            #region header

            XElement headerElement = new XElement(namespaceGrundUppgifterITP + "Header");
            headerElement.Add(new XAttribute("version", "2.0.0.2"));
            headerElement.Add(new XElement(namespaceArkitekturella + "SkickatFran", Company.Name));
            headerElement.Add(new XElement(namespaceArkitekturella + "SkickatTill", "Collectum"));

            #region TimeStamp

            XElement timeStampElement = new XElement(namespaceArkitekturella + "Timestamp");
            timeStampElement.Add(new XElement(namespaceTyper + "Datetime", timeStamp));
            timeStampElement.Add(new XElement(namespaceTyper + "Fractions", 0));

            headerElement.Add(timeStampElement);
            headerElement.Add(new XElement(namespaceArkitekturella + "Sekvensnummer", 1));
            headerElement.Add(new XElement(namespaceArkitekturella + "Produktion", "true"));
            headerElement.Attributes("xmlns").Remove();

            root.Add(headerElement);

            #endregion

            #endregion

            #region Organisationsnummer

            XElement organisationsnummerElement = new XElement(namespaceGrundUppgifterITP + "Organisationsnummer", StringUtility.Orgnr16XXXXXX_Dash_XXXX(Company.OrgNr));
            //organisationsnummerElement.Attributes("xmlns").Remove();

            root.Add(organisationsnummerElement);

            #endregion

            #region Employees

            List<int> selectedTimePeriodIds = selectedTimePeriods.Select(t => t.TimePeriodId).ToList();
            List<EmployeeTimePeriod> employeeTimePeriods = TimePeriodManager.GetEmployeeTimePeriods(reportResult.ActorCompanyId, selectedTimePeriodIds);
            Dictionary<int, List<int>> validEmployeesByTimePeriod = PayrollManager.GetValidEmployeesForTimePeriod(entities, reportResult.ActorCompanyId, selectedTimePeriodIds, employees, employeeTimePeriods: employeeTimePeriods);
            List<EventHistoryDTO> eventHistories = new List<EventHistoryDTO>();

            foreach (Employee employee in employees)
            {
                if (employee.CollectumITPPlan == 0)
                    continue;

                DateTime handelsetidpunkt = CalendarUtility.DATETIME_DEFAULT;

                EmployeeCollectum employeeCollectum = CreateEmployeeCollectum(entities, employee, selectedTimePeriods, validEmployeesByTimePeriod, selectionDateFrom, selectionDateTo, handelsenummer, timeStamp, ref handelsetidpunkt);
                if (employeeCollectum == null)
                    continue;

                if (!employeeCollectum.EventHistories.IsNullOrEmpty())
                    eventHistories.AddRange(employeeCollectum.EventHistories);

                #region NyAnmalan element

                bool nyamnalan = Company.ActorCompanyId == 1108079 && DateTime.Today < new DateTime(2019, 06, 17);
                if (employeeCollectum.NyAnmalan.Valid || nyamnalan)
                {
                    XElement nyAnmalanHandelseElement = new XElement(namespaceGrundUppgifterITP + "NyanmalanHandelse",
                        new XAttribute("Handelsenummer", employeeCollectum.Handelsenummer));

                    DateTime nyAnmalanTidpunkt = handelsetidpunkt;

                    XElement nyAnmalanElement = new XElement(namespaceGrundUppgifterITP + "Nyanmalan",
                        new XAttribute("version", "2.1.0.0"),
                        new XElement(namespaceNyanmalan + "Organisationsnummer", employeeCollectum.Organisationsnummer),
                        new XElement(namespaceNyanmalan + "KostnadsstalleId", employeeCollectum.KostnadsstalleId),
                        new XElement(namespaceNyanmalan + "Avtalsplanid", employeeCollectum.Avtalsplanid),
                        new XElement(namespaceNyanmalan + "Personnummer", employeeCollectum.Personnummer),
                        new XElement(namespaceNyanmalan + "Handelsetidpunkt", nyAnmalanTidpunkt.ToShortDateString()),
                        new XElement(namespaceNyanmalan + "Efternamn", employeeCollectum.Efternamn),
                        new XElement(namespaceNyanmalan + "Fornamn", employeeCollectum.Fornamn));
                    personalDataRepository.AddEmployeeSocialSec(employee);

                    if (employee.CollectumITPPlan == (int)TermGroup_PayrollReportsCollectumITPplan.ITP1)
                    {
                        nyAnmalanElement.Add(
                            new XElement(namespaceNyanmalan + "Manadslon", employeeCollectum.Manadslon));
                        nyAnmalanElement.Add(
                           new XElement(namespaceNyanmalan + "AvtaladManadslon", employeeCollectum.AvtaladManadslon));
                    }
                    else if (employee.CollectumITPPlan == (int)TermGroup_PayrollReportsCollectumITPplan.ITP2)
                    {
                        nyAnmalanElement.Add(
                            new XElement(namespaceNyanmalan + "Arslon", employeeCollectum.Arslon),
                            new XElement(namespaceNyanmalan + "ArslonEfterLoneavstaende", employeeCollectum.ArslonEfterLoneavstaende),
                            new XElement(namespaceNyanmalan + "FulltArbetsfor", employeeCollectum.NyAnmalan.FulltArbetsfor.ToInt()),
                            new XElement(namespaceNyanmalan + "GradAvArbetsoformaga", employeeCollectum.GradAvArbetsoformaga),
                            new XElement(namespaceNyanmalan + "VanligITP2", employeeCollectum.NyAnmalan.VanligITP2.ToInt()));
                    }

                    XElement tidstampelElement = new XElement(namespaceNyanmalan + "Tidsstampel",
                        new XElement(namespaceTyper + "Datetime", employeeCollectum.Tidsstampel),
                        new XElement(namespaceTyper + "Fractions", 0));

                    nyAnmalanElement.Add(tidstampelElement);
                    nyAnmalanHandelseElement.Add(nyAnmalanElement);
                    nyAnmalanHandelseElement.Attributes("xmlns").Remove();
                    root.Add(nyAnmalanHandelseElement);
                }

                #endregion

                #region FlyttAnstalldaInomKoncern element

                if (employeeCollectum.FlyttAnstalldaInomKoncern.Valid)
                {
                    XElement flyttAnstalldaInomKoncernHandelseElement = new XElement(namespaceGrundUppgifterITP + "FlyttAnstalldaInomKoncernHandelse",
                        new XAttribute("Handelsenummer", employeeCollectum.Handelsenummer));

                    DateTime flyttAnstalldaInomKoncernTidpunkt = handelsetidpunkt;

                    XElement flyttAnstalldaInomKoncernElement = new XElement(namespaceGrundUppgifterITP + "FlyttAnstalldaInomKoncern",
                        new XAttribute("version", "2.0.0.0"),
                        new XElement(namespaceFlyttAnstalldaInomKoncern + "Organisationsnummer", StringUtility.Orgnr16XXXXXX_Dash_XXXX(Company.OrgNr)),
                        new XElement(namespaceFlyttAnstalldaInomKoncern + "KostnadsstalleId", employeeCollectum.KostnadsstalleId),
                        new XElement(namespaceFlyttAnstalldaInomKoncern + "Avtalsplanid", employeeCollectum.Avtalsplanid),
                        new XElement(namespaceFlyttAnstalldaInomKoncern + "Personnummer", employeeCollectum.Personnummer),
                        new XElement(namespaceFlyttAnstalldaInomKoncern + "Handelsetidpunkt", flyttAnstalldaInomKoncernTidpunkt.ToShortDateString()),
                        new XElement(namespaceFlyttAnstalldaInomKoncern + "OrganisationsnummerFran", employeeCollectum.FlyttAnstalldaInomKoncern.OrganisationsnummerFran),
                        new XElement(namespaceFlyttAnstalldaInomKoncern + "KostnadsstalleIdFran", employeeCollectum.FlyttAnstalldaInomKoncern.KostnadsstalleIdFran));

                    XElement tidstampelElement = new XElement(namespaceFlyttAnstalldaInomKoncern + "Tidsstampel",
                        new XElement(namespaceTyper + "Datetime", employeeCollectum.Tidsstampel),
                        new XElement(namespaceTyper + "Fractions", 0));

                    flyttAnstalldaInomKoncernElement.Add(tidstampelElement);
                    flyttAnstalldaInomKoncernHandelseElement.Add(flyttAnstalldaInomKoncernElement);
                    flyttAnstalldaInomKoncernHandelseElement.Attributes("xmlns").Remove();
                    root.Add(flyttAnstalldaInomKoncernHandelseElement);
                }

                #endregion

                #region LoneAndring element

                if (employeeCollectum.LoneAndring.Valid)
                {
                    XElement loneandringHandelseElement = new XElement(namespaceGrundUppgifterITP + "LoneandringHandelse",
                        new XAttribute("Handelsenummer", employeeCollectum.Handelsenummer));


                    DateTime loneAndringTidpunkt = handelsetidpunkt;

                    XElement lonAndringElement = new XElement(namespaceGrundUppgifterITP + "Loneandring",
                        new XAttribute("version", "3.0.0.3"),
                        new XElement(namespaceLonandring + "Organisationsnummer", StringUtility.Orgnr16XXXXXX_Dash_XXXX(Company.OrgNr)),
                        new XElement(namespaceLonandring + "KostnadsstalleId", employeeCollectum.KostnadsstalleId),
                        //new XElement(namespaceLonandring + "Avtalsplanid", employeeCollectum.Avtalsplanid),
                        new XElement(namespaceLonandring + "Personnummer", employeeCollectum.Personnummer),
                        new XElement(namespaceLonandring + "Handelsetidpunkt", loneAndringTidpunkt.ToShortDateString()));

                    if (employee.CollectumITPPlan == (int)TermGroup_PayrollReportsCollectumITPplan.ITP1)
                    {
                        lonAndringElement.Add(
                            new XElement(namespaceLonandring + "Manadslon", employeeCollectum.Manadslon));
                    }
                    else if (employee.CollectumITPPlan == (int)TermGroup_PayrollReportsCollectumITPplan.ITP2)
                    {
                        lonAndringElement.Add(
                            new XElement(namespaceLonandring + "Arslon", employeeCollectum.Arslon),
                            new XElement(namespaceLonandring + "ArslonEfterLoneavstaende", employeeCollectum.ArslonEfterLoneavstaende),
                            new XElement(namespaceLonandring + "GradAvArbetsoformaga", employeeCollectum.GradAvArbetsoformaga));
                    }

                    XElement tidstampelElement = new XElement(namespaceLonandring + "Tidsstampel",
                        new XElement(namespaceTyper + "Datetime", employeeCollectum.Tidsstampel),
                        new XElement(namespaceTyper + "Fractions", 0));

                    lonAndringElement.Add(tidstampelElement);
                    loneandringHandelseElement.Add(lonAndringElement);
                    loneandringHandelseElement.Attributes("xmlns").Remove();
                    root.Add(loneandringHandelseElement);
                }

                #endregion

                #region AvAnmalan element

                if (employeeCollectum.AvAnmalan.Valid)
                {
                    XElement avAnmalanHandelseElement = new XElement(namespaceGrundUppgifterITP + "AvanmalanHandelse",
                        new XAttribute("Handelsenummer", employeeCollectum.Handelsenummer));

                    XElement avAnmalanElement = new XElement(namespaceGrundUppgifterITP + "Avanmalan",
                        new XAttribute("version", "2.0.0.3"),
                        new XElement(namespaceAvanmalan + "Organisationsnummer", StringUtility.Orgnr16XXXXXX_Dash_XXXX(Company.OrgNr)),
                        new XElement(namespaceAvanmalan + "KostnadsstalleId", employeeCollectum.KostnadsstalleId),
                        //new XElement(namespaceAvanmalan + "Avtalsplanid", employeeCollectum.Avtalsplanid),
                        new XElement(namespaceAvanmalan + "Personnummer", employeeCollectum.Personnummer),
                        new XElement(namespaceAvanmalan + "Handelsetidpunkt", employeeCollectum.Handelsetidpunkt > DateTime.Now.AddYears(-100) ? employeeCollectum.Handelsetidpunkt.ToShortDateString() : CalendarUtility.DATETIME_DEFAULT.ToShortDateString()),
                        new XElement(namespaceAvanmalan + "Avgangsorsak", employeeCollectum.AvAnmalan.Avgangsorsak));

                    if (employeeCollectum.AvAnmalan.DatumForForaldraledighet.HasValue)
                        avAnmalanElement.Add(new XElement(namespaceAvanmalan + "DatumForForaldraledighet", employeeCollectum.AvAnmalan.DatumForForaldraledighet.Value));

                    XElement tidstampelElement = new XElement(namespaceAvanmalan + "Tidsstampel",
                          new XElement(namespaceTyper + "Datetime", employeeCollectum.Tidsstampel),
                          new XElement(namespaceTyper + "Fractions", 0));

                    avAnmalanElement.Add(tidstampelElement);
                    avAnmalanHandelseElement.Add(avAnmalanElement);
                    avAnmalanHandelseElement.Attributes("xmlns").Remove();
                    root.Add(avAnmalanHandelseElement);
                }

                #endregion

                handelsenummer++;
            }

            #endregion

            #region Validate

            document.Add(root);
            string validationMessage = ValidateXDocument(document);

            #endregion

            #region Create & save to file

            string fileName = IOUtil.FileNameSafe(Company.Name + "_Collectum_" + GetYearMonthDay(selectionDateFrom) + " - " + GetYearMonthDay(selectionDateTo));
            DirectoryInfo directory = Directory.CreateDirectory(ConfigSettings.SOE_SERVER_DIR_TEMP_REPORT_PHYSICAL + Guid.NewGuid().ToString());
            string filePath = StringUtility.GetValidFilePath(directory.FullName) + fileName + ".xml";

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

            #region Close repository

            personalDataRepository.GenerateLogs();

            #endregion

            #region SetAsFinal

            if (setAsFinal && eventHistories.Any())
            {
                GeneralManager.SaveEventHistories(entities, eventHistories, Company.ActorCompanyId);
            }

            #endregion

            return filePath;
        }

        public EmployeeCollectum CreateEmployeeCollectum(CompEntities entities, Employee employee, List<TimePeriod> selectedTimePeriods, Dictionary<int, List<int>> validPeriods, DateTime fromDate, DateTime toDate, int handelsenummer, DateTime timeStamp, ref DateTime handelsetidpunkt)
        {
            #region Prereq

            handelsetidpunkt = CalendarUtility.DATETIME_DEFAULT;

            if (selectedTimePeriods.IsNullOrEmpty())
                return null;

            List<Employment> employments = employee.GetActiveEmployments();
            if (employments.IsNullOrEmpty())
                return null;

            var selectionTimePeriodIds = selectedTimePeriods.Select(t => t.TimePeriodId).Distinct().ToList();
            int validTimePeriodId = validPeriods.FirstOrDefault(v => v.Value.Contains(employee.EmployeeId)).Key;
            TimePeriod timePeriod = selectedTimePeriods.FirstOrDefault(p => p.TimePeriodId == validTimePeriodId) ?? selectedTimePeriods.OrderBy(p => p.StartDate).FirstOrDefault();

            handelsetidpunkt = selectedTimePeriods.FirstOrDefault().PaymentDate.Value;
            Employment firstEmployment = employments.GetFirstEmployment();
            Employment lastEmployment = employments.GetLastEmployment();
            Employment currentEmployment = employee.GetEmployment(toDate);
            DateTime firstEmploymentStartDate = firstEmployment.DateFrom.HasValue ? employments.GetFirstEmployment().DateFrom.Value : CalendarUtility.DATETIME_DEFAULT;
            TermGroup_PayrollExportSalaryType salaryType = EmployeeManager.GetEmployeeSalaryType(employee, fromDate, toDate);

            #endregion

            EmployeeCollectum employeeCollectum = new EmployeeCollectum
            {
                Handelsenummer = handelsenummer,
                KostnadsstalleId = employee.CollectumCostPlace,
                Avtalsplanid = employee.CollectumAgreedOnProduct,
                Tidsstampel = timeStamp,
                Personnummer = showSocialSec ? StringUtility.SocialSecYYYYMMDD_Dash_XXXX(employee.SocialSec) : StringUtility.SocialSecYYMMDD_Dash_Stars(employee.SocialSec),
                Fornamn = employee.FirstName,
                Efternamn = employee.LastName,
                Organisationsnummer = StringUtility.Orgnr16XXXXXX_Dash_XXXX(Company.OrgNr),
                Arslon = 0,
                ArslonEfterLoneavstaende = 0,
                GradAvArbetsoformaga = 0
            };

            #region Set type

            bool nyanmalanValid = false;
            bool loneandringValid = true;
            bool avanmalanValid = false;
            bool flyttinomKoncernValid = false;

            if (timePeriod.PayrollStartDate != timePeriod.StartDate && CalendarUtility.IsDateInRange(firstEmploymentStartDate, timePeriod.PayrollStartDate, timePeriod.PayrollStopDate))
                nyanmalanValid = true;

            if (!nyanmalanValid && timePeriod.PayrollStartDate == timePeriod.StartDate && CalendarUtility.IsDateInRange(firstEmploymentStartDate, timePeriod.StartDate, timePeriod.StopDate))
                nyanmalanValid = true;

            if (nyanmalanValid)
            {
                var history = GeneralManager.GetLastEventHistory(entities, TermGroup_EventHistoryType.CollectumNyAnmalan, employee.EmployeeId, Company.ActorCompanyId);

                if (history != null && history.DateData.HasValue && history.DateData.Value > timePeriod.StartDate.AddMonths(-2))
                    nyanmalanValid = false;
            }
            else if (!employee.CollectumCancellationDate.HasValue)
            {
                var avanmalanHistory = GeneralManager.GetLastEventHistory(entities, TermGroup_EventHistoryType.CollectumAvAnmalan, employee.EmployeeId, Company.ActorCompanyId);

                if (avanmalanHistory?.DateData != null && avanmalanHistory.IntData != 1)
                {
                    var nyanmalanhistory = GeneralManager.GetLastEventHistory(entities, TermGroup_EventHistoryType.CollectumNyAnmalan, employee.EmployeeId, Company.ActorCompanyId);
                    var nyanmalanDate = nyanmalanhistory?.DateData != null ? nyanmalanhistory.DateData.Value : (DateTime?)null;

                    if (nyanmalanDate == null || nyanmalanDate.HasValue && nyanmalanDate.Value < avanmalanHistory.DateData.Value)
                        nyanmalanValid = true;
                }
            }

            if (employee.CollectumCancellationDate.HasValue)
                avanmalanValid = true;

            //if (true == false) //TODO
            //    flyttinomKoncernValid = true;

            employeeCollectum.NyAnmalan = new NyAnmalan();
            employeeCollectum.FlyttAnstalldaInomKoncern = new FlyttAnstalldaInomKoncern();
            employeeCollectum.LoneAndring = new LoneAndring();
            employeeCollectum.AvAnmalan = new AvAnmalan();

            if (nyanmalanValid || loneandringValid)
            {
                //employeeCollectum.Arslon = 0; //TODO
                //employeeCollectum.ArslonEfterLoneavstaende = 0; //TODO
                //employeeCollectum.GradAvArbetsoformaga = 0; //TODO

                var employees = new List<Employee>() { employee };
                var transactionItems = TimeTransactionManager.GetTimePayrollStatisticsDTOs(entities, Company.ActorCompanyId, employees, selectionTimePeriodIds);
                transactionItems = transactionItems.Where(i => !i.IsEmploymentTaxBelowLimitHidden).ToList();
                var itp1Transactions = transactionItems.Where(t => PayrollRulesUtil.isITP1(t.PensionCompany)).ToList();
                employeeCollectum.Manadslon = Convert.ToInt64(itp1Transactions.Sum(x => x.Amount));
                employeeCollectum.Handelsetidpunkt = handelsetidpunkt;

                if (employeeCollectum.Manadslon < 0)
                {
                    loneandringValid = false;
                    nyanmalanValid = false;
                }
                else
                {
                    if (salaryType == TermGroup_PayrollExportSalaryType.Hourly)
                    {
                        decimal mulitplier = new decimal(12);
                        // Den årslön som ska anges i samband med en nyanmälan av en timanställd, som uppfyller arbetstidskravet om 96 timmar, 
                        // är genomsnittet av de tre arbetade kalendermånadernas utbetalda lön multiplicerat med 12(12, 2 om semestertillägg inte 
                        // ingår i beräkningsunderlaget).Arbetsgivaren har möjlighet att rapportera en ny årslön varje månad, förutsatt att arbetstidskravet 
                        // om 96 timmar fortfarande uppfylls.

                        var transactionItemsWork = transactionItems.Where(t => PayrollRulesUtil.isITP1(t.PensionCompany) && t.IsHourlySalary()).ToList();
                        if (transactionItems.Any(a => !a.IsVacationAddition()))
                            mulitplier = new decimal(12.2);

                        if (Decimal.Divide(transactionItemsWork.Sum(t => t.Quantity), 60) >= 96)
                        {
                            List<int> timePeriodIds = selectedTimePeriods.Select(t => t.TimePeriodId).ToList();
                            List<EmployeeTimePeriodValue> values = TimePeriodManager.GetPaidEmployeeTimePeriodValues(entities, timePeriodIds, employee.EmployeeId, Company.ActorCompanyId);
                            List<EmployeeTimePeriodValue> grossSalaryValues = values.Where(f => f.Type == (int)SoeEmployeeTimePeriodValueType.GrossSalary).ToList();

                            int divideBy = grossSalaryValues.Count > 2 ? 3 : grossSalaryValues.Count;

                            if (divideBy > 0)
                                employeeCollectum.Arslon = Convert.ToInt64((grossSalaryValues.Sum(v => v.Value) / divideBy) * 12);
                            else
                                employeeCollectum.Arslon = 0;
                        }
                    }
                    else
                    {
                        if (employee.CollectumITPPlan == (int)TermGroup_PayrollReportsCollectumITPplan.ITP1)
                        {
                            if (nyanmalanValid)
                            {
                                var employment = currentEmployment;
                                var onDate = toDate;
                                if (employment == null)
                                {
                                    employment = firstEmployment;
                                    onDate = firstEmploymentStartDate;
                                }

                                if (employment != null)
                                {
                                    var payrollGroup = employment.GetPayrollGroup(onDate);
                                    if (payrollGroup != null)
                                    {
                                        var setting = payrollGroup.PayrollGroupSetting.FirstOrDefault(p => p.Type == (int)PayrollGroupSettingType.PayrollFormula);
                                        if (setting != null && setting.IntData.HasValue && setting.IntData.Value != 0)
                                        {
                                            PayrollPriceFormulaResultDTO result = PayrollManager.EvaluatePayrollPriceFormula(entities, reportResult.ActorCompanyId, employee, employment, null, onDate, null, null, setting.IntData.Value);
                                            if (result != null)
                                                employeeCollectum.AvtaladManadslon = Convert.ToInt64(result.Amount);
                                        }
                                    }
                                }
                            }

                            var transactionItemsWork = transactionItems.Where(t => PayrollRulesUtil.isITP1(t.PensionCompany) && t.IsMonthlySalary()).ToList();
                            var transactionItemsArslon = transactionItemsWork.Where(t => t.IsCollectumArslon()).ToList();

                            employeeCollectum.Arslon = Convert.ToInt64((transactionItemsArslon.Sum(v => v.Amount)));
                            var transactionItemsLonevaxling = transactionItems.Where(t => t.IsCollectumLonevaxling()).ToList();
                            employeeCollectum.ArslonEfterLoneavstaende = employeeCollectum.Arslon - Convert.ToInt64((transactionItemsLonevaxling.Sum(v => v.Amount)));
                        }
                        else
                        {
                            var transactionItemsWork = transactionItems.Where(t => PayrollRulesUtil.isITP2(t.PensionCompany) && t.IsMonthlySalary()).ToList();
                            var transactionItemsArslon = transactionItemsWork.Where(t => t.IsCollectumArslon()).ToList();
                            employeeCollectum.Arslon = Convert.ToInt32(decimal.Multiply((transactionItemsArslon.Sum(v => v.Amount)), new decimal(12.2)));
                            var transactionItemsLonevaxling = transactionItems.Where(t => t.IsCollectumLonevaxling()).ToList();
                            employeeCollectum.ArslonEfterLoneavstaende = employeeCollectum.Arslon - Convert.ToInt64((transactionItemsLonevaxling.Sum(v => v.Amount)));

                        }
                    }
                }
            }

            if (nyanmalanValid)
            {
                employeeCollectum.NyAnmalan.FulltArbetsfor = true;
                employeeCollectum.NyAnmalan.Valid = true;
                employeeCollectum.NyAnmalan.VanligITP2 = true;
                employeeCollectum.Handelsetidpunkt = handelsetidpunkt;
                employeeCollectum.EventHistories.Add(new EventHistoryDTO(Company.ActorCompanyId, TermGroup_EventHistoryType.CollectumNyAnmalan, SoeEntityType.Employee, employee.EmployeeId, userId: UserId, booleanValue: true, dateValue: handelsetidpunkt));
            }
            else
            {
                employeeCollectum.NyAnmalan.Valid = false;
            }

            if (flyttinomKoncernValid)
            {
                employeeCollectum.FlyttAnstalldaInomKoncern.Valid = true;
                employeeCollectum.FlyttAnstalldaInomKoncern.KostnadsstalleIdFran = string.Empty;
                employeeCollectum.FlyttAnstalldaInomKoncern.OrganisationsnummerFran = string.Empty;
            }
            else
            {
                employeeCollectum.FlyttAnstalldaInomKoncern.Valid = false;
            }

            if (!nyanmalanValid && loneandringValid && employeeCollectum.Arslon == 0 && employee.CollectumITPPlan != (int)TermGroup_PayrollReportsCollectumITPplan.ITP1)
                loneandringValid = false;

            if (loneandringValid)
            {
                var history = GeneralManager.GetLastEventHistory(entities, TermGroup_EventHistoryType.CollectumLoneAndring, employee.EmployeeId, Company.ActorCompanyId);
                if (employee.CollectumITPPlan == (int)TermGroup_PayrollReportsCollectumITPplan.ITP2 && history != null && history.DecimalData.HasValue && history.DecimalData.Value == employeeCollectum.Arslon)
                    employeeCollectum.LoneAndring.Valid = false;
                else
                {
                    employeeCollectum.LoneAndring.Valid = true;
                    if (employee.CollectumITPPlan == (int)TermGroup_PayrollReportsCollectumITPplan.ITP1)
                        employeeCollectum.EventHistories.Add(new EventHistoryDTO(Company.ActorCompanyId, TermGroup_EventHistoryType.CollectumLoneAndring, SoeEntityType.Employee, employee.EmployeeId, userId: UserId, decimalValue: employeeCollectum.Manadslon, dateValue: handelsetidpunkt));
                    else
                        employeeCollectum.EventHistories.Add(new EventHistoryDTO(Company.ActorCompanyId, TermGroup_EventHistoryType.CollectumLoneAndring, SoeEntityType.Employee, employee.EmployeeId, userId: UserId, decimalValue: employeeCollectum.Arslon, dateValue: handelsetidpunkt));

                }
            }
            else
            {
                employeeCollectum.LoneAndring.Valid = false;
            }

            if (avanmalanValid)
            {
                //Avgångsorsak
                TermGroup_EmploymentEndReason endreason = (TermGroup_EmploymentEndReason)lastEmployment.GetEndReason(lastEmployment.DateTo);
                employeeCollectum.AvAnmalan.Avgangsorsak = 1;

                //Värdeförråd: 1, 2, 3 eller 9 
                //     1 = Avslutad anställning: försäkring och anställning avslutas – debitering upphör. 
                //     2 = Föräldraledighet: försäkring avslutas, anställningen kvarstår – debitering upphör. 
                //     3 = Tjänstledig: försäkring avslutas, anställningen kvarstår – debitering upphör. 
                //     9 = Annullera tidigare registrerad avanmälan: försäkring och anställning aktiveras igen – debitering återupptas.  

                switch (endreason)
                {
                    case TermGroup_EmploymentEndReason.None:
                    case TermGroup_EmploymentEndReason.SE_CompanyChanged:
                    case TermGroup_EmploymentEndReason.SE_Deceased:
                    case TermGroup_EmploymentEndReason.SE_EmploymentChanged:
                    case TermGroup_EmploymentEndReason.SE_Fired:
                    case TermGroup_EmploymentEndReason.SE_LaidOfDueToRedundancy:
                    case TermGroup_EmploymentEndReason.SE_OwnRequest:
                    case TermGroup_EmploymentEndReason.SE_Retirement:
                    case TermGroup_EmploymentEndReason.SE_TemporaryEmploymentEnds:
                        employeeCollectum.AvAnmalan.Avgangsorsak = 1;
                        break;
                }
                if (lastEmployment.GetEndDate() == null && endreason == TermGroup_EmploymentEndReason.None)
                {
                    if (employee.CollectumCancellationDateIsLeaveOfAbsence)
                        employeeCollectum.AvAnmalan.Avgangsorsak = 3;
                    else
                        employeeCollectum.AvAnmalan.Avgangsorsak = 2;
                }

                employeeCollectum.AvAnmalan.Valid = true;
                employeeCollectum.Handelsetidpunkt = employee.CollectumCancellationDate ?? CalendarUtility.DATETIME_DEFAULT;
                employeeCollectum.EventHistories.Add(new EventHistoryDTO(Company.ActorCompanyId, TermGroup_EventHistoryType.CollectumAvAnmalan, SoeEntityType.Employee, employee.EmployeeId, userId: UserId, booleanValue: true, dateValue: handelsetidpunkt, integerValue: employeeCollectum.AvAnmalan.Avgangsorsak));

            }
            else
            {
                employeeCollectum.AvAnmalan.Valid = false;
            }

            #endregion

            return employeeCollectum;
        }

        #endregion

        #region Help-methods

        private XmlSchemaSet CreateSchemas()
        {
            XmlSchemaSet schemas = new XmlSchemaSet();
            schemas.Add(namespaceGrundUppgifterITP.ToString(), XmlReader.Create(new StringReader(GetXsd(@"granssnitt\pa\grunduppgifterITP\3.0\grunduppgifterITP.xsd"))));
            schemas.Add(namespaceTyper.ToString(), XmlReader.Create(new StringReader(GetXsd(@"\typer\2.0\centrala typer.xsd"))));
            schemas.Add(namespaceArkitekturella.ToString(), XmlReader.Create(new StringReader(GetXsd(@"arkitekturella\2.0\header.xsd"))));
            schemas.Add(namespaceNyanmalan.ToString(), XmlReader.Create(new StringReader(GetXsd(@"paket\pa\nyanmalan\2.0\nyanmalan.xsd"))));
            schemas.Add(namespaceFlyttAnstalldaInomKoncern.ToString(), XmlReader.Create(new StringReader(GetXsd(@"paket\pa\FlyttAnstalldaInomKoncern\3.0\FlyttAnstalldaInomKoncern.xsd"))));
            schemas.Add(namespaceAvanmalan.ToString(), XmlReader.Create(new StringReader(GetXsd(@"paket\pa\avanmalan\2.0\avanmalan.xsd"))));
            schemas.Add(namespaceLonandring.ToString(), XmlReader.Create(new StringReader(GetXsd(@"paket\pa\loneandring\3.0\Loneandring.xsd"))));
            return schemas;
        }

        private string GetXsd(string path)
        {
            if (File.Exists(ConfigSettings.SOE_SERVER_DIR_REPORT_EXTERNAL_COLLECTUM_PHYSICAL + path))
                return File.ReadAllText(ConfigSettings.SOE_SERVER_DIR_REPORT_EXTERNAL_COLLECTUM_PHYSICAL + path);
            else if (File.Exists(ConfigSettings.SOE_SERVER_DIR_DEFAULT_PHYSICAL + @"External\Collectum\" + path))
                return File.ReadAllText(ConfigSettings.SOE_SERVER_DIR_REPORT_CONFIGFILE + @"External\Collectum\" + path);

            else if (File.Exists(ConfigSettings.SOE_SERVER_DIR_REPORT_CONFIGFILE + @"External\Collectum\" + path))
                return File.ReadAllText(ConfigSettings.SOE_SERVER_DIR_REPORT_CONFIGFILE + @"External\Collectum\" + path);

            return string.Empty;
        }

        private string ValidateXDocument(XDocument document)
        {
            bool errors = false;
            string message = "";

            XmlSchemaSet schemas = CreateSchemas();

            document.Validate(schemas, (o, e) =>
            {
                message = e.Message;
                errors = true;
            });

            return errors ? message : String.Empty;
        }

        #endregion
    }

    #region Classes

    public class EmployeeCollectum
    {
        public EmployeeCollectum()
        {
            this.EventHistories = new List<EventHistoryDTO>();
        }
        public int Handelsenummer { get; set; }
        public string Personnummer { get; set; }
        public string Fornamn { get; set; }
        public string Efternamn { get; set; }
        public string Organisationsnummer { get; set; }
        public DateTime Handelsetidpunkt { get; set; }
        public string KostnadsstalleId { get; set; }
        public string Avtalsplanid { get; set; }
        public long Manadslon { get; set; }
        public long AvtaladManadslon { get; set; }
        public long Arslon { get; set; }
        public long ArslonEfterLoneavstaende { get; set; }
        public int GradAvArbetsoformaga { get; set; }
        public decimal Tjanstgoringsgrad { get; set; }
        public DateTime Tidsstampel { get; set; }
        public NyAnmalan NyAnmalan { get; set; }
        public FlyttAnstalldaInomKoncern FlyttAnstalldaInomKoncern { get; set; }
        public LoneAndring LoneAndring { get; set; }
        public AvAnmalan AvAnmalan { get; set; }
        public List<EventHistoryDTO> EventHistories { get; set; }
    }

    public class NyAnmalan
    {
        public bool Valid { get; set; }
        public bool FulltArbetsfor { get; set; }
        public bool VanligITP2 { get; set; }
    }

    public class FlyttAnstalldaInomKoncern
    {
        public bool Valid { get; set; }
        public string OrganisationsnummerFran { get; set; }
        public string KostnadsstalleIdFran { get; set; }

    }

    public class LoneAndring
    {
        public bool Valid { get; set; }
    }

    public class AvAnmalan
    {
        public bool Valid { get; set; }
        public int Avgangsorsak { get; set; }
        public DateTime? DatumForForaldraledighet { get; set; }
    }


    #endregion
}
