using Soe.Sys.Common.DTO;
using SoftOne.Soe.Business.Core.Reporting.Models.Time;
using SoftOne.Soe.Business.Core.RptGen;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.DTO;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Communicator;
using SoftOne.Soe.Business.Util.Communicator.Kivra;
using SoftOne.Soe.Business.Util.ExportFiles;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using SoftOne.Soe.Util.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Core.Reporting
{
    public class TimeReportDataManager : BaseReportDataManager
    {
        #region Ctor

        public TimeReportDataManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region ExportFiles

        #endregion

        #region Export files XML

        public XDocument Create_KU10_ReportData(CreateReportResult reportResult, out bool authorized)
        {
            base.reportResult = reportResult;

            #region Prereq

            //Default
            authorized = true;

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Init document

            //Document
            XDocument document = XmlUtil.CreateDocument();

            //Root
            XElement rootElement = new XElement(ROOT + "_" + "KU10Report");

            XElement KU10ReportElement = null;

            #endregion

            if (reportResult.DataStorageId != 0)
            {
                #region Prereq

                DataStorage dataStorage = ReportManager.GetDataStorage(reportResult.DataStorageId);
                if (dataStorage == null || dataStorage.XML == null)
                    return null;

                Employee employee = EmployeeManager.GetEmployeeForUser(reportResult.UserId, reportResult.ActorCompanyId);
                if (employee == null || employee.EmployeeId != dataStorage.EmployeeId)
                {
                    //Can only show own salary specifications
                    authorized = false;
                    return null;
                }

                #endregion

                #region Content

                XDocument originalDocument = XDocument.Parse(dataStorage.XML);
                if (originalDocument == null)
                    return null;

                KU10ReportElement = originalDocument.Root;

                #endregion
            }
            else
            {
                #region Prereq

                if (!TryGetIdsFromSelection(reportResult, out List<int> selectionTimePeriodIds, "periods"))
                    return null;
                if (!TryGetEmployeeIdsFromSelection(reportResult, selectionTimePeriodIds, out List<Employee> employees, out List<int> selectionEmployeeIds))
                    return null;

                TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo);
                TryGetBoolFromSelection(reportResult, out bool removePrevSubmittedData, "removePrevSubmittedData");
                removePrevSubmittedData = reportResult.EvaluatedSelection != null ? reportResult.EvaluatedSelection.ST_KU10RemovePrevSubmittedData : removePrevSubmittedData;

                if (employees == null)
                    employees = EmployeeManager.GetAllEmployeesByIds(reportResult.ActorCompanyId, selectionEmployeeIds, loadEmployment: true);

                var timePeriods = TimePeriodManager.GetTimePeriods(TermGroup_TimePeriodType.Payroll, Company.ActorCompanyId);
                var selectedTimePeriods = timePeriods.Where(w => selectionTimePeriodIds.Contains(w.TimePeriodId)).OrderBy(o => o.StartDate).ToList();
                if (selectionDateFrom == DateTime.MinValue)
                    selectionDateFrom = (selectedTimePeriods.FirstOrDefault()?.StartDate ?? reportResult.EvaluatedSelection?.DateFrom) ?? CalendarUtility.DATETIME_DEFAULT;
                selectionDateTo = CalendarUtility.GetEndOfYear(selectionDateFrom);
                int year = timePeriods.OrderBy(x => x.PayrollStartDate).LastOrDefault()?.PayrollStopDate.Value.Year ?? 0;

                #endregion

                #region Init document

                KU10ReportElement = new XElement("KU10Report");

                #endregion

                #region ReportHeader

                XElement reportHeaderElement = CreateTimeReportHeaderElement(reportResult);
                reportHeaderElement.Add(CreateDateIntervalElement(selectionDateFrom, selectionDateTo));
                KU10ReportElement.Add(reportHeaderElement);

                #endregion

                #region Feature

                bool showSocialSec = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId);

                #endregion

                #region Content

                using (CompEntities entities = new CompEntities())
                {
                    List<TimePayrollStatisticsSmallDTO> timePayrollTransactionItems = TimeTransactionManager.GetTimePayrollStatisticsSmallDTOs_new(entities, reportResult.ActorCompanyId, employees, selectionTimePeriodIds, ignoreAccounting: true);

                    KU10 ku10 = new KU10(parameterObject, reportResult);

                    int employeeXmlId = 1;
                    foreach (Employee employee in employees)
                    {
                        #region Employee

                        #region Prereq

                        EmployeeTaxSE taxSe = EmployeeManager.GetEmployeeTaxSE(employee.EmployeeId, year);
                        KU10EmployeeDTO kU10EmployeeDTO = ku10.CreateKU10EmployeeDTO(timePayrollTransactionItems.Where(t => t.EmployeeId == employee.EmployeeId && !t.IsEmploymentTaxAndHidden).ToList(), employee, year.ToString(), taxSe, removePrevSubmittedData, selectionDateFrom, selectionDateTo);
                        if (kU10EmployeeDTO == null)
                            continue;

                        #endregion

                        #region Employee element

                        XElement employeeElement = new XElement("Employee",
                            new XAttribute("Id", employeeXmlId),
                            new XElement("EmployeeNr", employee.EmployeeNr),
                            new XElement("EmployeeName", employee.Name),
                            new XElement("EmployeeSocialSec", employee.ContactPerson != null && showSocialSec ? employee.SocialSec : StringUtility.SocialSecYYMMDD_Dash_Stars(employee.SocialSec)),
                            new XElement("EmployeeSex", employee.ContactPerson != null ? employee.ContactPerson.Sex : 0),
                            new XElement("UserName", employee.User != null ? employee.User.LoginName : String.Empty),
                            new XElement("Note", employee.Note));
                        base.personalDataRepository.AddEmployee(employee, employeeElement);

                        #endregion

                        #region EmployeeContactInformation element

                        Contact contact = EmployeeManager.GetEmployeeContact(entities, employee.EmployeeId);
                        if (contact != null)
                            employeeElement.Add(CreateTimeEmployeeContactInformationElements(entities, contact));

                        #endregion

                        #region KU10 element

                        XElement ku10Element = new XElement("Ku10",
                            new XAttribute("Id", employeeXmlId),
                            new XElement("Inkomstar", kU10EmployeeDTO.Inkomstar),
                            new XElement("Inkomsttagare", kU10EmployeeDTO.Inkomsttagare),
                            new XElement("AvdragenSkatt", kU10EmployeeDTO.AvdragenSkatt),
                            new XElement("AnstalldFrom", kU10EmployeeDTO.AnstalldFrom),
                            new XElement("AnstalldTom", kU10EmployeeDTO.AnstalldTom),
                            new XElement("KontantBruttolonMm", kU10EmployeeDTO.KontantBruttolonMm),
                            new XElement("FormanUtomBilDrivmedel", kU10EmployeeDTO.FormanUtomBilDrivmedel),
                            new XElement("BilformanUtomDrivmedel", kU10EmployeeDTO.BilformanUtomDrivmedel),
                            new XElement("KodForFormansbil", kU10EmployeeDTO.KodForFormansbil),
                            new XElement("AntalManBilforman", kU10EmployeeDTO.AntalManBilforman),
                            new XElement("KmBilersVidBilforman", kU10EmployeeDTO.KmBilersVidBilforman),
                            new XElement("BetaltForBilforman", kU10EmployeeDTO.BetaltForBilforman),
                            new XElement("DrivmedelVidBilforman", kU10EmployeeDTO.DrivmedelVidBilforman),
                            new XElement("AndraKostnadsers", kU10EmployeeDTO.AndraKostnadsers),
                            new XElement("UnderlagRutarbete", kU10EmployeeDTO.UnderlagRutarbete),
                            new XElement("UnderlagRotarbete", kU10EmployeeDTO.UnderlagRotarbete),
                            new XElement("ErsMEgenavgifter", kU10EmployeeDTO.ErsMEgenavgifter),
                            new XElement("Tjanstepension", kU10EmployeeDTO.Tjanstepension),
                            new XElement("ErsEjSocAvg", kU10EmployeeDTO.ErsEjSocAvg),
                            new XElement("ErsEjSocAvgEjJobbavd", kU10EmployeeDTO.ErsEjSocAvgEjJobbavd),
                            new XElement("Forskarskattenamnden", kU10EmployeeDTO.Forskarskattenamnden),
                            new XElement("VissaAvdrag", kU10EmployeeDTO.VissaAvdrag),
                            new XElement("Hyresersattning", kU10EmployeeDTO.Hyresersattning),
                            new XElement("BostadSmahus", kU10EmployeeDTO.BostadSmahus.ToInt()),
                            new XElement("Kost", kU10EmployeeDTO.Kost.ToInt()),
                            new XElement("BostadEjSmahus", kU10EmployeeDTO.BostadEjSmahus.ToInt()),
                            new XElement("Ranta", kU10EmployeeDTO.Ranta.ToInt()),
                            new XElement("Parkering", kU10EmployeeDTO.Parkering.ToInt()),
                            new XElement("AnnanForman", kU10EmployeeDTO.AnnanForman.ToInt()),
                            new XElement("FormanHarJusterats", kU10EmployeeDTO.FormanHarJusterats.ToInt()),
                            new XElement("FormanSomPension", kU10EmployeeDTO.FormanSomPension.ToInt()),
                            new XElement("Bilersattning", kU10EmployeeDTO.Bilersattning.ToInt()),
                            new XElement("TraktamenteInomRiket", kU10EmployeeDTO.TraktamenteInomRiket.ToInt()),
                            new XElement("TraktamenteUtomRiket", kU10EmployeeDTO.TraktamenteUtomRiket.ToInt()),
                            new XElement("TjansteresaOver3MInrikes", kU10EmployeeDTO.TjansteresaOver3MInrikes.ToInt()),
                            new XElement("TjansteresaOver3MUtrikes", kU10EmployeeDTO.TjansteresaOver3MUtrikes.ToInt()),
                            new XElement("Resekostnader", kU10EmployeeDTO.Resekostnader.ToInt()),
                            new XElement("Logi", kU10EmployeeDTO.Logi.ToInt()),
                            new XElement("Arbetsstallenummer", kU10EmployeeDTO.Arbetsstallenummer),
                            new XElement("Delagare", kU10EmployeeDTO.Delagare.ToInt()),
                            new XElement("SpecAvAnnanForman", 0), //kU10EmployeeDTO.SpecAvAnnanForman),
                            new XElement("SpecVissaAvdrag", 0),//kU10EmployeeDTO.SpecVissaAvdrag),
                            new XElement("LandskodTIN", kU10EmployeeDTO.LandskodTIN),
                            new XElement("SocialAvgiftsAvtal", kU10EmployeeDTO.SocialAvgiftsAvtal),
                            new XElement("TIN", kU10EmployeeDTO.TIN),
                            new XElement("Borttag", kU10EmployeeDTO.Borttag.ToInt()),
                            new XElement("Specifikationsnummer", kU10EmployeeDTO.Specifikationsnummer),
                            new XElement("GiltigaUppgifter", kU10EmployeeDTO.GiltigaUppgifter.ToInt()));

                        #endregion

                        #region Transactions element

                        int transactionsXmlId = 0;
                        foreach (var trans in kU10EmployeeDTO.Transactions)
                        {
                            XElement transElement = new XElement("Transactions",
                                new XAttribute("Id", transactionsXmlId),
                                new XElement("Type", trans.Type),
                                new XElement("PayrollProductNumber", trans.PayrollProductNumber),
                                new XElement("Name", trans.Name),
                                new XElement("Quantity", trans.Quantity),
                                new XElement("Amount", trans.Amount),
                                new XElement("IsPayrollStartValue", trans.IsPayrollStartValue.ToInt()));

                            ku10Element.Add(transElement);
                            transactionsXmlId++;
                        }

                        #endregion

                        employeeElement.Add(ku10Element);
                        KU10ReportElement.Add(employeeElement);
                        employeeXmlId++;

                        #endregion
                    }
                }

                #endregion
            }

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            #region Close document

            rootElement.Add(KU10ReportElement);
            document.Add(rootElement);

            return GetValidatedDocument(document, reportResult);

            #endregion
        }

        public XDocument Create_SKD_ReportData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            if (!TryGetIdsFromSelection(reportResult, out List<int> selectionTimePeriodIds, "periods"))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionTimePeriodIds, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return null;

            TryGetDatesFromSelection(reportResult, selectionTimePeriodIds, out DateTime selectionDateFrom, out _, out var timePeriods, alwaysLoadPeriods: true);

            var firstTimePeriod = timePeriods.FirstOrDefault();
            if (firstTimePeriod == null)
                return null;

            if (selectionDateFrom == CalendarUtility.DATETIME_DEFAULT && reportResult.EvaluatedSelection == null && timePeriods.Any(a => a.PaymentDate.HasValue))
                selectionDateFrom = timePeriods.OrderBy(o => o.PaymentDate).First().PaymentDate.Value;

            if (employees == null)
                employees = EmployeeManager.GetAllEmployeesByIds(reportResult.ActorCompanyId, selectionEmployeeIds, loadEmployment: true);

            #endregion

            #region Init repository

            if (reportResult.EvaluatedSelection != null)
                base.InitPersonalDataEmployeeReportRepository(reportResult.EvaluatedSelection);
            else
                base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Init document

            //Document
            XDocument document = XmlUtil.CreateDocument();

            //Root
            XElement rootElement = new XElement(ROOT + "_" + "SKDReport");

            //EmployeeVacationInformationReport
            XElement SKDElement = new XElement("SKDReport");

            #endregion

            #region ReportHeader

            XElement reportHeaderElement = CreateTimeReportHeaderElement(reportResult);
            reportHeaderElement.Add(new XElement("DateInterval", firstTimePeriod.PaymentDate.Value.ToString("yyyyMM")));
            SKDElement.Add(reportHeaderElement);



            #endregion

            #region Content

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                if (employees == null)
                    employees = EmployeeManager.GetAllEmployeesByIds(entities, reportResult.ActorCompanyId, selectionEmployeeIds, loadEmployment: true);

                List<TimePayrollStatisticsSmallDTO> timePayrollTransactionItems = TimeTransactionManager.GetTimePayrollStatisticsSmallDTOs_new(entities, reportResult.ActorCompanyId, employees, selectionTimePeriodIds, applyEmploymentTaxMinimumRule: true);

                eSKDUpload eSKDUpload = new eSKDUpload(parameterObject, reportResult);
                SKDDTO skdDTO = eSKDUpload.CreateSKDDTO(timePayrollTransactionItems, selectionTimePeriodIds, selectionDateFrom, employees);

                #endregion

                #region Ag element

                XElement agElement = eSKDUpload.CreateAgElement(timePayrollTransactionItems, selectionTimePeriodIds, selectionDateFrom, employees, skdDTO);

                #endregion

                #region Transactions element

                int transXMLId = 0;
                foreach (var transactionsByEmployee in skdDTO.Transactions.GroupBy(i => i.EmployeeNr))
                {
                    Employee employee = employees.FirstOrDefault(i => i.EmployeeNr == transactionsByEmployee.Key);
                    if (employee == null)
                        continue;

                    bool firstTransaction = true;
                    foreach (var transaction in transactionsByEmployee)
                    {
                        XElement transElement = new XElement("Transactions",
                            new XAttribute("Id", transXMLId),
                            new XElement("EmployeeNr", transaction.EmployeeNr),
                            new XElement("EmployeeName", transaction.EmployeeName),
                            new XElement("Type", transaction.Type),
                            new XElement("PayrollProductNumber", transaction.PayrollProductNumber),
                            new XElement("PayrollProductName", transaction.PayrollProductName),
                            new XElement("Quantity", transaction.Quantity),
                            new XElement("Amount", transaction.Amount),
                            new XElement("SysPayrollTypeLevel1", transaction.SysPayrollTypeLevel1.ToString()),
                            new XElement("SysPayrollTypeLevel2", transaction.SysPayrollTypeLevel2.ToString()),
                            new XElement("SysPayrollTypeLevel3", transaction.SysPayrollTypeLevel3.ToString()),
                            new XElement("SysPayrollTypeLevel4", transaction.SysPayrollTypeLevel4.ToString()),
                            new XElement("SysPayrollTypeLevel1Name", transaction.SysPayrollTypeLevel1),
                            new XElement("SysPayrollTypeLevel2Name", transaction.SysPayrollTypeLevel2),
                            new XElement("SysPayrollTypeLevel3Name", transaction.SysPayrollTypeLevel3),
                            new XElement("SysPayrollTypeLevel4Name", transaction.SysPayrollTypeLevel4));

                        if (firstTransaction)
                        {
                            base.personalDataRepository.AddEmployee(employee, transElement);
                            firstTransaction = false;
                        }
                        agElement.Add(transElement);
                        transXMLId++;
                    }
                }

                #endregion

                SKDElement.Add(agElement);
            }

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            #region Close document

            rootElement.Add(SKDElement);
            document.Add(rootElement);

            return GetValidatedDocument(document, reportResult);

            #endregion
        }

        public XDocument Create_SCB_SN_ReportData(CreateReportResult reportResult, bool isSCB)
        {
            base.reportResult = reportResult;

            #region Prereq

            if (!TryGetIdsFromSelection(reportResult, out List<int> selectionTimePeriodIds, "periods"))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionTimePeriodIds, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return null;

            TryGetDatesFromSelection(reportResult, selectionTimePeriodIds, out DateTime selectionDateFrom, out DateTime selectionDateTo);

            if (employees == null)
                employees = EmployeeManager.GetAllEmployeesByIds(reportResult.ActorCompanyId, selectionEmployeeIds, loadEmployment: true);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Init document

            //Document
            XDocument document = XmlUtil.CreateDocument();

            //Root
            XElement rootElement = new XElement(ROOT + "_" + "SCB_SN_Report");

            //EmployeeVacationInformationReport
            XElement scb_sn_ReportElement = new XElement("SCB_SN_Report");

            #endregion

            #region ReportHeader

            XElement reportHeaderElement = CreateTimeReportHeaderElement(reportResult);
            reportHeaderElement.Add(CreateDateIntervalElement(selectionDateFrom, selectionDateTo));
            scb_sn_ReportElement.Add(reportHeaderElement);

            #endregion

            #region Content

            SCBStatisticsFiles scb = new SCBStatisticsFiles(parameterObject, reportResult);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var headDTO = scb.CreateSCB_SLPPayrollStatisticsFileHeadDTO(entitiesReadOnly, employees, isSCB, selectionDateFrom, selectionDateTo, selectionTimePeriodIds);

            #region Headelement

            XElement headElement = new XElement("Head",
                    new XAttribute("Id", 0),
                    new XElement("NumberOfEmployees", headDTO.NumberOfEmployees),
                    new XElement("Date", headDTO.Date));

            int rowXmlId = 0;

            foreach (var row in headDTO.SCBPayrollStatisticsFileRowDTOs)
            {
                Employee employee = employees.FirstOrDefault(i => i.EmployeeNr == row.Anstnummer);
                if (employee == null)
                    continue;

                XElement rowElement = new XElement("Row",
                    new XAttribute("Id", rowXmlId),
                    new XElement("Period", row.Period),
                    new XElement("Delagarnummer", row.Delagarnummer),
                    new XElement("Arbetsplatsnummer", row.Arbetsplatsnummer),
                    new XElement("Organisationsnummer", row.Organisationsnummer),
                    new XElement("Forbundsnummer", row.Forbundsnummer),
                    new XElement("Avtalskod", row.Avtalskod),
                    new XElement("Personnummer", row.Personnummer),
                    new XElement("Anstnummer", row.Anstnummer),
                    new XElement("Namn", row.Namn),
                    new XElement("Personalkategori", row.Personalkategori),
                    new XElement("Arbetstidsart", row.Arbetstidsart),
                    new XElement("Yrkeskod", row.Yrkeskod),
                    new XElement("Forbundsspecifikkod", row.Forbundsspecifikkod),
                    new XElement("Loneform", row.Loneform),
                    new XElement("AntalanstalldaCFARnr", row.AntalanstalldaCFARnr),
                    new XElement("CFARnummer", row.CFARnummer),
                    new XElement("Helglon", row.Helglon),
                    new XElement("Reserv", row.Reserv),
                    new XElement("Falt1a", row.Falt1a),
                    new XElement("Falt1b", row.Falt1b),
                    new XElement("Falt2a", row.Falt2a),
                    new XElement("Falt2b", row.Falt2b),
                    new XElement("Falt3a", row.Falt3a),
                    new XElement("Falt3b", row.Falt3b),
                    new XElement("Falt4a", row.Falt4a),
                    new XElement("Falt4b", row.Falt4b),
                    new XElement("Falt5a", row.Falt5a),
                    new XElement("Falt5b", row.Falt5b),
                    new XElement("Falt6a", row.Falt6a),
                    new XElement("Falt6b", row.Falt6b),
                    new XElement("Falt7a", row.Falt7a),
                    new XElement("Falt7b", row.Falt7b),
                    new XElement("Falt8a", row.Falt8a),
                    new XElement("Falt8b", row.Falt8b),
                    new XElement("Falt9a", row.Falt9a),
                    new XElement("Falt9b", row.Falt9b),
                    new XElement("Falt10aa", row.Falt10aa),
                    new XElement("Falt10ab", row.Falt10ab),
                    new XElement("Falt10ba", row.Falt10ba),
                    new XElement("Falt10bb", row.Falt10bb),
                    new XElement("Falt10ca", row.Falt10ca),
                    new XElement("Falt10cb", row.Falt10cb),
                    new XElement("Falt11a", row.Falt11a),
                    new XElement("Falt11b", row.Falt11b),
                    new XElement("Falt12a", row.Falt12a),
                    new XElement("Falt12b", row.Falt12b),
                    new XElement("Falt13a", row.Falt13a),
                    new XElement("Falt13b", row.Falt13b),
                    new XElement("Falt14a", row.Falt14a),
                    new XElement("Falt14b", row.Falt14b),
                    new XElement("Falt15aa", row.Falt15aa),
                    new XElement("Falt15ab", row.Falt15ab),
                    new XElement("Falt15ba", row.Falt15ba),
                    new XElement("Falt15bb", row.Falt15bb),
                    new XElement("Falt16a", row.Falt16a),
                    new XElement("Falt16b", row.Falt16b),
                    new XElement("Falt17a", row.Falt17a),
                    new XElement("Falt17b", row.Falt17b),
                    new XElement("Falt18a", row.Falt18a),
                    new XElement("Falt18b", row.Falt18b),
                    new XElement("Falt19a", row.Falt19a),
                    new XElement("Falt19b", row.Falt19b),
                    new XElement("Falt20a", row.Falt20a),
                    new XElement("Falt20b", row.Falt20b),
                    new XElement("Falt21a", row.Falt21a),
                    new XElement("Falt21b", row.Falt21b),
                    new XElement("Falt22a", row.Falt22a),
                    new XElement("Falt22b", row.Falt22b),
                    new XElement("Falt23a", row.Falt23a),
                    new XElement("Falt23b", row.Falt23b),
                    new XElement("Falt24a", row.Falt24a),
                    new XElement("Falt24b", row.Falt24b),
                    new XElement("Falt25a", row.Falt25a),
                    new XElement("Falt25b", row.Falt25b),
                    new XElement("Falt26a", row.Falt26a),
                    new XElement("Falt26b", row.Falt26b),
                    new XElement("Falt27a", row.Falt27a),
                    new XElement("Falt27b", row.Falt27b),
                    new XElement("Falt28a", row.Falt28a),
                    new XElement("Falt28b", row.Falt28b),
                    new XElement("Falt29a", row.Falt29a),
                    new XElement("Falt29b", row.Falt29b),
                    new XElement("Falt30a", row.Falt30a),
                    new XElement("Falt30b", row.Falt30b),
                    new XElement("Falt31a", row.Falt31a),
                    new XElement("Falt31b", row.Falt31b),
                    new XElement("Falt32a", row.Falt32a),
                    new XElement("Falt32b", row.Falt32b),
                    new XElement("Falt33a", row.Falt33a),
                    new XElement("Falt33b", row.Falt33b),
                    new XElement("Falt34a", row.Falt34a),
                    new XElement("Falt34b", row.Falt34b),
                    new XElement("Falt35a", row.Falt35a),
                    new XElement("Falt35b", row.Falt35b),
                    new XElement("Falt36a", row.Falt36a),
                    new XElement("Falt36b", row.Falt36b),
                    new XElement("Falt37a", row.Falt37a),
                    new XElement("Falt37b", row.Falt37b),
                    new XElement("Falt38a", row.Falt38a),
                    new XElement("Falt38b", row.Falt38b),
                    new XElement("Falt39a", row.Falt39a),
                    new XElement("Falt39b", row.Falt39b),
                    new XElement("Falt40a", row.Falt40a),
                    new XElement("Falt40b", row.Falt40b),
                    new XElement("Falt41a", row.Falt41a),
                    new XElement("Falt41b", row.Falt41b),
                    new XElement("Falt42a", row.Falt42a),
                    new XElement("Falt42b", row.Falt42b),
                    new XElement("Falt43a", row.Falt43a),
                    new XElement("Falt43b", row.Falt43b),
                    new XElement("Falt44a", row.Falt44a),
                    new XElement("Falt44b", row.Falt44b),
                    new XElement("Falt45a", row.Falt45a),
                    new XElement("Falt45b", row.Falt45b),
                    new XElement("Falt46a", row.Falt46a),
                    new XElement("Falt46b", row.Falt46b));
                base.personalDataRepository.AddEmployee(employee, rowElement);

                int transXMLId = 0;

                foreach (var trans in row.scbPayrollStatisticsTransactionDTOs)
                {
                    XElement transElement = new XElement("Transactions",
                        new XAttribute("Id", transXMLId),
                        new XElement("Type", trans.Type),
                        new XElement("EmployeeNr", trans.EmployeeNr),
                        new XElement("PayrollProductNumber", trans.ProductNr),
                        new XElement("Quantity", trans.Quantity),
                        new XElement("Amount", trans.Amount));

                    rowElement.Add(transElement);
                    transXMLId++;

                }

                headElement.Add(rowElement);
                rowXmlId++;
            }

            scb_sn_ReportElement.Add(headElement);

            #endregion

            #endregion

            #region Close document

            rootElement.Add(scb_sn_ReportElement);
            document.Add(rootElement);

            return GetValidatedDocument(document, reportResult);

            #endregion
        }

        public XDocument Create_SCB_KLP_ReportData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            #endregion

            #region Init document

            //Document
            XDocument document = XmlUtil.CreateDocument();

            //Root
            XElement rootElement = new XElement(ROOT + "_" + "SCB_KLP_Report");

            //EmployeeVacationInformationReport
            XElement scb_KLP_ReportElement = new XElement("SCB_KLP_Report");

            #endregion

            #region ReportHeader

            TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo);

            XElement reportHeaderElement = CreateTimeReportHeaderElement(reportResult);
            reportHeaderElement.Add(CreateDateIntervalElement(selectionDateFrom, selectionDateTo));
            scb_KLP_ReportElement.Add(reportHeaderElement);

            #endregion

            #region Content

            SCBStatisticsFiles scb = new SCBStatisticsFiles(parameterObject, reportResult);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var headDTO = scb.CreateSCB_KLPPayrollStatisticsFileHeadDTO(entitiesReadOnly);

            #region Headelement

            XElement headElement = new XElement("Head",
                new XAttribute("Id", 0),
                new XElement("Date", headDTO.Date),
                new XElement("TotaltAntalAnstallda", headDTO.NumberOfEmployees),
                new XElement("OverenskommenManadslon", headDTO.OverenskommenManadslon),
                new XElement("Utbetaldlon", headDTO.Utbetaldlon),
                new XElement("DaravOvertidstillagg", headDTO.DaravOvertidstillagg),
                new XElement("ArbetadeTimmar", headDTO.ArbetadeTimmar),
                new XElement("AvtaladeTimmar", headDTO.AvtaladeTimmar),
                new XElement("DaravOvertidstimmar", headDTO.DaravOvertidstimmar),
                new XElement("Retrolon", headDTO.Retrolon),
                new XElement("Sjuklon", headDTO.Sjuklon),
                new XElement("RorligaTillagg", headDTO.RorligaTillagg));

            int employeeXMLId = 0;

            foreach (var employee in headDTO.SCBPayrollStatisticsFileRowDTOs)
            {
                XElement employeeElement = new XElement("Anstalld",
                    new XAttribute("Id", employeeXMLId),
                        new XElement("EmployeeId", employee.EmployeeId),
                        new XElement("EmployeeNr", employee.EmployeeNr),
                        new XElement("EmployeeName", employee.EmployeeName),
                        new XElement("PersonNr", employee.PersonNr),
                        new XElement("Typ", employee.Typ),
                        new XElement("Loneform", employee.Loneform),
                        new XElement("PersonalKategori", employee.PersonalKategori),
                        new XElement("ArbetsKategori", employee.ArbetsKategori),
                        new XElement("OverenskommenManadslon", employee.OverenskommenManadslon),
                        new XElement("Utbetaldlon", employee.Utbetaldlon),
                        new XElement("DaravOvertidstillagg", employee.DaravOvertidstillagg),
                        new XElement("ArbetadeTimmar", employee.ArbetadeTimmar),
                        new XElement("AvtaladeTimmar", employee.AvtaladeTimmar),
                        new XElement("DaravOvertidstimmar", employee.DaravOvertidstimmar),
                        new XElement("Retrolon", employee.Retrolon),
                        new XElement("Sjuklon", employee.Sjuklon),
                        new XElement("Sysselsattningsgrad", employee.Sysselsattningsgrad),
                        new XElement("Period", employee.Period),
                        new XElement("RorligaTillagg", employee.RorligaTillagg));

                int transXMLId = 0;

                foreach (var trans in employee.SCBPayrollStatisticsTransactionDTOs)
                {
                    XElement transElement = new XElement("Transactions",
                        new XAttribute("Id", transXMLId),
                        new XElement("Type", trans.Type),
                        new XElement("EmployeeNr", trans.EmployeeNr),
                        new XElement("PayrollProductNumber", trans.ProductNr),
                        new XElement("PayrollProductName", trans.Name),
                        new XElement("Quantity", trans.Quantity),
                        new XElement("Amount", trans.Amount),
                        new XElement("Date", trans.Date),
                        new XElement("SysPayrollTypeLevel1", trans.SysPayrollTypeLevel1 ?? 0),
                        new XElement("SysPayrollTypeLevel2", trans.SysPayrollTypeLevel2 ?? 0),
                        new XElement("SysPayrollTypeLevel3", trans.SysPayrollTypeLevel3 ?? 0),
                        new XElement("SysPayrollTypeLevel4", trans.SysPayrollTypeLevel4 ?? 0));
                    employeeElement.Add(transElement);
                    transXMLId++;
                }

                if (transXMLId == 0)
                {
                    XElement transElement = new XElement("Transactions",
                        new XAttribute("Id", transXMLId),
                        new XElement("Type", 0),
                        new XElement("EmployeeNr", string.Empty),
                        new XElement("PayrollProductNumber", string.Empty),
                        new XElement("PayrollProductName", string.Empty),
                        new XElement("Quantity", 0),
                        new XElement("Amount", 0),
                        new XElement("Date", CalendarUtility.DATETIME_DEFAULT),
                        new XElement("SysPayrollTypeLevel1", 0),
                        new XElement("SysPayrollTypeLevel2", 0),
                        new XElement("SysPayrollTypeLevel3", 0),
                        new XElement("SysPayrollTypeLevel4", 0));
                    employeeElement.Add(transElement);
                }

                headElement.Add(employeeElement);
                employeeXMLId++;
            }

            scb_KLP_ReportElement.Add(headElement);

            #endregion

            #endregion

            #region Close document

            rootElement.Add(scb_KLP_ReportElement);
            document.Add(rootElement);

            return GetValidatedDocument(document, reportResult);

            #endregion
        }

        public XDocument Create_SCB_KSP_ReportData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            #endregion

            #region Init document

            //Document
            XDocument document = XmlUtil.CreateDocument();

            //Root
            XElement rootElement = new XElement(ROOT + "_" + "SCB_KSP_Report");

            //EmployeeVacationInformationReport
            XElement scb_ksp_ReportElement = new XElement("SCB_KSP_Report");

            #endregion

            #region ReportHeader

            TryGetDateFromSelection(reportResult, out DateTime selectionDate, "date");

            XElement reportHeaderElement = CreateTimeReportHeaderElement(reportResult);
            reportHeaderElement.Add(CreateDateIntervalElement(selectionDate, selectionDate));
            scb_ksp_ReportElement.Add(reportHeaderElement);

            #endregion

            #region Content

            SCBStatisticsFiles scb = new SCBStatisticsFiles(parameterObject, reportResult);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var headDTO = scb.CreateSCB_KSPPayrollStatisticsFileHeadDTO(entitiesReadOnly);

            #region Headelement

            XElement headElement = new XElement("Head",
                    new XAttribute("Id", 0),
                    new XElement("Date", headDTO.Date),
                    new XElement("TotaltAntalAnstallda", headDTO.TotaltAntalAnstallda),
                    new XElement("AntalAnstalldaMan", headDTO.AntalAnstalldaMan),
                    new XElement("AntalAnstalldaKvinnor", headDTO.AntalAnstalldaKvinnor),
                    new XElement("TotaltAntalTillsvidareAnstallda", headDTO.TotaltAntalTillsvidareAnstallda),
                    new XElement("AntalTillsvidareAnstalldaMan", headDTO.AntalTillsvidareAnstalldaMan),
                    new XElement("AntalTillsvidareAnstalldaKvinnor", headDTO.AntalTillsvidareAnstalldaKvinnor),
                    new XElement("TotaltAntalVisstidsAnstallda", headDTO.TotaltAntalVisstidsAnstallda),
                    new XElement("AntalVisstidsAnstalldaMan", headDTO.AntalVisstidsAnstalldaMan),
                    new XElement("AntalVisstidsAnstalldaKvinnor", headDTO.AntalVisstidsAnstalldaKvinnor),
                    new XElement("TotaltAntalHeldagsfranvarande", headDTO.TotaltAntalHeldagsfranvarande),
                    new XElement("AntalHeldagsfranvarandeMan", headDTO.AntalHeldagsfranvarandeMan),
                    new XElement("AntalHeldagsfranvarandeKvinnor", headDTO.AntalHeldagsfranvarandeKvinnor),
                    new XElement("TotaltAntalHeldagsfranvarandeSjukdomArbetsskada", headDTO.TotaltAntalHeldagsfranvarandeSjukdomArbetsskada),
                    new XElement("AntalHeldagsfranvarandeSjukdomArbetsskadaMan", headDTO.AntalHeldagsfranvarandeSjukdomArbetsskadaMan),
                    new XElement("AntalHeldagsfranvarandeSjukdomArbetsskadaKvinnor", headDTO.AntalHeldagsfranvarandeSjukdomArbetsskadaKvinnor),
                    new XElement("TotaltAntalHeldagsfranvarandeSemester", headDTO.TotaltAntalHeldagsfranvarandeSemester),
                    new XElement("AntalHeldagsfranvarandeSemesterMan", headDTO.AntalHeldagsfranvarandeSemesterMan),
                    new XElement("AntalHeldagsfranvarandeSemesterKvinnor", headDTO.AntalHeldagsfranvarandeSemesterKvinnor),
                    new XElement("TotaltAntalHeldagsfranvarandeOvrigFranvaro", headDTO.TotaltAntalHeldagsfranvarandeOvrigFranvaro),
                    new XElement("AntalHeldagsfranvarandeOvrigFranvaroMan", headDTO.AntalHeldagsfranvarandeOvrigFranvaroMan),
                    new XElement("AntalHeldagsfranvarandeOvrigFranvaroKvinnor", headDTO.AntalHeldagsfranvarandeOvrigFranvaroKvinnor),
                    new XElement("NyAnstallda", headDTO.NyAnstallda),
                    new XElement("NyAnstalldaMan", headDTO.NyAnstalldaMan),
                    new XElement("NyAnstalldaKvinnor", headDTO.NyAnstalldaKvinnor),
                    new XElement("NyAnstalldaVissTidTotalt", headDTO.NyAnstalldaVissTidTotalt),
                    new XElement("NyAnstalldaVissTidMan", headDTO.NyAnstalldaVissTidMan),
                    new XElement("NyAnstalldaVissTidKvinnor", headDTO.NyAnstalldaVissTidKvinnor),
                    new XElement("NyAnstalldaTillsvidareTotalt", headDTO.NyAnstalldaTillsvidareTotalt),
                    new XElement("NyAnstalldaTillsvidareMan", headDTO.NyAnstalldaTillsvidareMan),
                    new XElement("NyAnstalldaTillsvidareKvinnor", headDTO.NyAnstalldaTillsvidareKvinnor),
                    new XElement("Avgangna", headDTO.Avgangna),
                    new XElement("AvgangnaMan", headDTO.AvgangnaMan),
                    new XElement("AvgangnaKvinnor", headDTO.AvgangnaKvinnor),
                    new XElement("AvgangnaVissTidTotalt", headDTO.AvgangnaVissTidTotalt),
                    new XElement("AvgangnaVissTidMan", headDTO.AvgangnaVissTidMan),
                    new XElement("AvgangnaVissTidKvinnor", headDTO.AvgangnaVissTidKvinnor),
                    new XElement("AvgangnaTillsvidareTotalt", headDTO.AvgangnaTillsvidareTotalt),
                    new XElement("AvgangnaTillsvidareMan", headDTO.AvgangnaTillsvidareMan),
                    new XElement("AvgangnaTillsvidareKvinnor", headDTO.AvgangnaTillsvidareKvinnor));

            int transXMLId = 0;

            foreach (var trans in headDTO.SCBPayrollStatisticsTransactionDTO)
            {
                XElement transElement = new XElement("Transactions",
                    new XAttribute("Id", transXMLId),
                    new XElement("Type", trans.Type),
                    new XElement("EmployeeNr", trans.EmployeeNr),
                    new XElement("PayrollProductNumber", trans.ProductNr),
                    new XElement("Quantity", trans.Quantity),
                    new XElement("Amount", trans.Amount));

                headElement.Add(transElement);

                transXMLId++;
            }

            scb_ksp_ReportElement.Add(headElement);

            #endregion

            #endregion

            #region Close document

            rootElement.Add(scb_ksp_ReportElement);
            document.Add(rootElement);

            return GetValidatedDocument(document, reportResult);

            #endregion
        }

        public XDocument Create_SCB_KSJU_ReportData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Init document

            //Document
            XDocument document = XmlUtil.CreateDocument();

            //Root
            XElement rootElement = new XElement(ROOT + "_" + "SCB_KSJU_Report");

            //EmployeeVacationInformationReport
            XElement scb_KSJU_ReportElement = new XElement("SCB_KSJU_Report");

            #endregion

            #region Content

            TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo);
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return null;

            if (employees == null)
                employees = EmployeeManager.GetAllEmployeesByIds(reportResult.ActorCompanyId, selectionEmployeeIds, loadEmployment: true);

            SCBStatisticsFiles scb = new SCBStatisticsFiles(parameterObject, reportResult);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var headDTO = scb.CreateSCB_KSJUPayrollStatisticsFileHeadDTO(entitiesReadOnly, employees, selectionDateFrom, selectionDateTo);

            XElement reportHeaderElement = CreateTimeReportHeaderElement(reportResult);
            reportHeaderElement.Add(CreateDateIntervalElement(selectionDateFrom, selectionDateTo));
            scb_KSJU_ReportElement.Add(reportHeaderElement);

            #region Headelement

            XElement headElement = new XElement("Head",
                    new XAttribute("Id", 0),
                    new XElement("Date", DateTime.Now.Date));

            int transXMLId = 0;

            foreach (var transByEmployee in headDTO.SCB_KSJUPayrollStatisticsFileRowDTOs.GroupBy(i => i.EmployeeId))
            {
                Employee employee = employees.FirstOrDefault(i => i.EmployeeId == transByEmployee.Key);
                if (employee == null)
                    continue;

                bool firstEmployeeTrans = true;
                foreach (var trans in transByEmployee)
                {
                    XElement transElement = new XElement("Transactions",
                        new XAttribute("Id", transXMLId),
                        new XElement("EmployeeId", trans.EmployeeId),
                        new XElement("EmployeeNr", trans.EmployeeNr),
                        new XElement("EmployeeName", trans.EmployeeName),
                        new XElement("PeOrgNr", trans.PeOrgNr),
                        new XElement("PersonNr", trans.PersonNr),
                        new XElement("SjukFrom", trans.SjukFrom),
                        new XElement("SjukTom", trans.SjukTom),
                        new XElement("HelAntDagar", trans.HelAntDagar),
                        new XElement("DelAntDagar", trans.DelAntDagar),
                        new XElement("Korrigeringsuppgift", trans.Korrigeringsuppgift),
                        new XElement("Cfar", trans.Cfar));

                    if (firstEmployeeTrans)
                    {
                        base.personalDataRepository.AddEmployee(employee, transElement);
                        firstEmployeeTrans = false;
                    }

                    headElement.Add(transElement);
                    transXMLId++;
                }
            }

            scb_KSJU_ReportElement.Add(headElement);

            #endregion

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            #region Close document

            rootElement.Add(scb_KSJU_ReportElement);
            document.Add(rootElement);

            return GetValidatedDocument(document, reportResult);

            #endregion
        }

        public XDocument Create_KPA_ReportData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            if (!TryGetIdsFromSelection(reportResult, out List<int> selectionTimePeriodIds, "periods"))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionTimePeriodIds, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return null;

            if (employees == null)
                employees = EmployeeManager.GetAllEmployeesByIds(base.ActorCompanyId, selectionEmployeeIds, loadEmployment: true);

            TryGetDatesFromSelection(reportResult, selectionTimePeriodIds, out DateTime selectionDateFrom, out DateTime selectionDateTo);

            int year = 0;

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Init document

            //Document
            XDocument document = XmlUtil.CreateDocument();

            //Root
            XElement rootElement = new XElement(ROOT + "_" + "KPAReport");

            //EmployeeVacationInformationReport
            XElement kpaReportElement = new XElement("KPAReport");

            #endregion

            #region ReportHeader

            XElement reportHeaderElement = CreateTimeReportHeaderElement(reportResult);
            reportHeaderElement.Add(CreateDateIntervalElement(selectionDateFrom, selectionDateTo));
            kpaReportElement.Add(reportHeaderElement);

            #endregion

            #region Content

            bool showSocialSec = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId);

            Kpa kpa = new Kpa(parameterObject, reportResult);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var employeeKPAs = kpa.CreateEmployeeKPAs(entitiesReadOnly, selectionTimePeriodIds, year, employees);

            int employeeXmlId = 0;
            foreach (Employee employee in employees)
            {
                #region Employee element

                XElement employeeElement = new XElement("Employee",
                        new XAttribute("Id", employeeXmlId),
                        new XElement("EmployeeNr", employee.EmployeeNr),
                        new XElement("FirstName", employee.ContactPerson != null ? employee.ContactPerson.FirstName : string.Empty),
                        new XElement("LastName", employee.ContactPerson != null ? employee.ContactPerson.LastName : string.Empty),
                        new XElement("SocialSec", showSocialSec ? employee.SocialSec : StringUtility.SocialSecYYMMDD_Dash_Stars(employee.SocialSec)),
                        new XElement("Note", employee.Note));
                base.personalDataRepository.AddEmployee(employee, employeeElement);

                int employeeKPAXmlId = 0;
                var employeeEmployeeKPAs = employeeKPAs.Where(e => e.EmployeeId == employee.EmployeeId).ToList();
                if (employeeEmployeeKPAs != null && employeeEmployeeKPAs.Count > 0)
                {
                    foreach (var employeeKPA in employeeEmployeeKPAs)
                    {
                        if (employeeKPA != null)
                        {
                            XElement employeeKPAElement = new XElement("EmployeeKPA",
                             new XAttribute("Id", employeeKPAXmlId),
                            new XElement("Transaktionskod", employeeKPA.Transaktionskod),
                            new XElement("InkomstKalla", employeeKPA.InkomstKalla),
                            new XElement("Kundnummer", employeeKPA.Kundnummer),
                            new XElement("Personnummer", employeeKPA.Personnummer),
                            new XElement("InkomstAr", employeeKPA.InkomstAr),
                            new XElement("InkomstManad", employeeKPA.InkomstManad),
                            new XElement("Inkomst", employeeKPA.Inkomst),
                            new XElement("Tecken", employeeKPA.Tecken),
                            new XElement("Avtalsnummer", employeeKPA.Avtalsnummer));

                            int employeeKPATransactionId = 0;

                            foreach (var trans in employeeKPA.Transactions)
                            {
                                XElement employeeForaTransactionElement = new XElement("EmployeeTransactionKPA",
                                        new XAttribute("Id", employeeKPATransactionId),
                                        new XElement("Type", trans.Type),
                                        new XElement("PayrollProductNumber", trans.PayrollProductNumber),
                                        new XElement("Name", trans.Name),
                                        new XElement("Quantity", trans.Quantity),
                                        new XElement("Amount", trans.Amount));

                                employeeKPAElement.Add(employeeForaTransactionElement);
                            }

                            employeeElement.Add(employeeKPAElement);
                        }

                        employeeKPAXmlId++;
                    }
                }
                else
                {
                    XElement employeeKPAElement = new XElement("EmployeeKPA",
                     new XAttribute("Id", employeeXmlId),
                    new XElement("Transaktionskod", string.Empty),
                    new XElement("InkomstKalla", string.Empty),
                    new XElement("Kundnummer", string.Empty),
                    new XElement("Personnummer", string.Empty),
                    new XElement("InkomstAr", string.Empty),
                    new XElement("InkomstManad", string.Empty),
                    new XElement("Inkomst", 0),
                    new XElement("Tecken", string.Empty),
                    new XElement("Avtalsnummer", string.Empty));

                    employeeElement.Add(employeeKPAElement);
                }

                #endregion

                employeeXmlId++;
                kpaReportElement.Add(employeeElement);
            }

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            #region Close document


            rootElement.Add(kpaReportElement);
            document.Add(rootElement);

            return GetValidatedDocument(document, reportResult);

            #endregion

        }

        public XDocument Create_Bygglosen_ReportData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            if (!TryGetIdsFromSelection(reportResult, out List<int> selectionTimePeriodIds, "periods"))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionTimePeriodIds, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return null;

            TryGetDatesFromSelection(reportResult, selectionTimePeriodIds, out DateTime selectionDateFrom, out DateTime selectionDateTo);
            TryGetBoolFromSelection(reportResult, out bool setAsFinal, "setAsFinal");
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

            if (employees == null)
                employees = EmployeeManager.GetAllEmployeesByIds(entitiesReadOnly, base.ActorCompanyId, selectionEmployeeIds, loadEmployment: true);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Init document

            //Document 
            XDocument document = XmlUtil.CreateDocument();

            //Root
            XElement rootElement = new XElement(ROOT + "_" + "BygglosenReport");

            //EmployeeVacationInformationReport
            XElement bygglosenReportElement = new XElement("BygglosenReport");

            #endregion

            #region ReportHeader

            XElement reportHeaderElement = CreateTimeReportHeaderElement(reportResult);
            reportHeaderElement.Add(CreateDateIntervalElement(selectionDateFrom, selectionDateTo));
            bygglosenReportElement.Add(reportHeaderElement);

            #endregion

            #region Content

            Bygglosen Bygglosen = new Bygglosen(parameterObject, reportResult);
            var bygglosen = Bygglosen.GetBygglosenDTO(entitiesReadOnly, reportResult.ActorCompanyId, reportResult.UserId, reportResult.RoleId, selectionTimePeriodIds, employees, setAsFinal);
            List<EventHistoryDTO> eventHistories = new List<EventHistoryDTO>();
            bygglosenReportElement.Add(bygglosen.GetDocument(string.Empty).Root);

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            #region Close document


            rootElement.Add(bygglosenReportElement);
            document.Add(rootElement);

            var doc = GetValidatedDocument(document, reportResult);

            if (doc != null && eventHistories.Any())
            {
                using (CompEntities entities = new CompEntities())
                {
                    GeneralManager.SaveEventHistories(entities, eventHistories, reportResult.ActorCompanyId);
                }
            }

            return doc;

            #endregion

        }

        public XDocument Create_Kronofogden_ReportData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                Kronofogden kronofogden = new Kronofogden(parameterObject, reportResult);
                var kronofogdenEmployees = kronofogden.GetKronofogdenEmployees(entities, out Company company, out bool setAsFinal);

                #endregion

                #region Init repository

                base.InitPersonalDataEmployeeReportRepository();

                #endregion

                #region Init document

                //Document 
                XDocument document = XmlUtil.CreateDocument();

                //Root
                XElement rootElement = new XElement(ROOT + "_" + "KronofogdenReport");

                //EmployeeVacationInformationReport
                XElement kronofogdenReportElement = new XElement("KronofogdenReport");

                #endregion

                #region ReportHeader

                XElement reportHeaderElement = CreateTimeReportHeaderElement(reportResult);
                kronofogdenReportElement.Add(reportHeaderElement);

                #endregion

                #region Content

                int xmlId = 0;
                foreach (var employee in kronofogdenEmployees)
                {
                    kronofogdenReportElement.Add(employee.OutPutXElement(xmlId++));
                }

                #endregion

                #region Close repository

                base.personalDataRepository.GenerateLogs();

                #endregion

                #region Close document


                rootElement.Add(kronofogdenReportElement);
                document.Add(rootElement);

                var doc = GetValidatedDocument(document, reportResult);

                if (doc != null && setAsFinal)
                {
                    GeneralManager.SaveEventHistories(entities, kronofogdenEmployees.Select(s => s.EventHistory(company.ActorCompanyId, base.UserId)).ToList(), company.ActorCompanyId);
                }

                return doc;

                #endregion
            }

        }

        public XDocument Create_KPADirekt_ReportData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            if (!TryGetIdsFromSelection(reportResult, out List<int> selectionTimePeriodIds, "periods"))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionTimePeriodIds, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return null;

            TryGetDatesFromSelection(reportResult, selectionTimePeriodIds, out DateTime selectionDateFrom, out DateTime selectionDateTo);
            TryGetBoolFromSelection(reportResult, out bool setAsFinal, "setAsFinal");
            TryGetBoolFromSelection(reportResult, out bool onlyChangedEmployments, "onlyChangedEmployments");
            TryGetIdFromSelection(reportResult, out int? kpaAgreementType, "kpaAgreementType");

            if (employees == null)
                employees = EmployeeManager.GetAllEmployeesByIds(base.ActorCompanyId, selectionEmployeeIds, loadEmployment: true);

            int year = 0;

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Init document

            //Document
            XDocument document = XmlUtil.CreateDocument();

            //Root
            XElement rootElement = new XElement(ROOT + "_" + "KPADirektReport");

            //EmployeeVacationInformationReport
            XElement kpaReportElement = new XElement("KPADirektReport");

            #endregion

            #region ReportHeader

            XElement reportHeaderElement = CreateTimeReportHeaderElement(reportResult);
            reportHeaderElement.Add(CreateDateIntervalElement(selectionDateFrom, selectionDateTo));
            kpaReportElement.Add(reportHeaderElement);

            #endregion

            #region Content

            bool showSocialSec = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId);

            Kpa kpa = new Kpa(parameterObject, reportResult);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var kpaDirekt = kpa.CreateEmployeeKPADirekt(entitiesReadOnly, reportResult.ActorCompanyId, reportResult.UserId, reportResult.RoleId, selectionTimePeriodIds, year, employees, onlyChangedEmployments, kpaAgreementType);
            List<EventHistoryDTO> eventHistories = new List<EventHistoryDTO>();

            int employeeXmlId = 0;
            foreach (Employee employee in employees)
            {
                var employeeKPADirekt = kpaDirekt.EmployeeKPADirekts.FirstOrDefault(w => w.EmployeeId == employee.EmployeeId);

                if (employeeKPADirekt == null)
                    continue;

                if (setAsFinal && employeeKPADirekt.EventHistories.Any())
                {
                    eventHistories.AddRange(employeeKPADirekt.EventHistories);
                }

                #region Employee element

                XElement employeeElement = new XElement("Employee",
                        new XAttribute("Id", employeeXmlId),
                        new XElement("EmployeeNr", employee.EmployeeNr),
                        new XElement("FirstName", employee.ContactPerson != null ? employee.ContactPerson.FirstName : string.Empty),
                        new XElement("LastName", employee.ContactPerson != null ? employee.ContactPerson.LastName : string.Empty),
                        new XElement("SocialSec", showSocialSec ? employee.SocialSec : StringUtility.SocialSecYYMMDD_Dash_Stars(employee.SocialSec)),
                        new XElement("Note", employee.Note));
                base.personalDataRepository.AddEmployee(employee, employeeElement);

                employeeElement.Add(employeeKPADirekt.OutPutXElment(employeeXmlId));

                #endregion

                employeeXmlId++;
                kpaReportElement.Add(employeeElement);
            }

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            #region Close document


            rootElement.Add(kpaReportElement);
            document.Add(rootElement);

            var doc = GetValidatedDocument(document, reportResult);

            if (doc != null && setAsFinal && eventHistories.Any())
            {
                using (CompEntities entities = new CompEntities())
                {
                    GeneralManager.SaveEventHistories(entities, eventHistories, reportResult.ActorCompanyId);
                }
            }

            return doc;

            #endregion

        }

        public XDocument Create_Fora_ReportData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            if (!TryGetIdsFromSelection(reportResult, out List<int> selectionTimePeriodIds, "periods"))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionTimePeriodIds, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return null;

            TryGetDatesFromSelection(reportResult, selectionTimePeriodIds, out DateTime selectionDateFrom, out DateTime selectionDateTo);

            if (employees == null)
                employees = EmployeeManager.GetAllEmployeesByIds(base.ActorCompanyId, selectionEmployeeIds, loadEmployment: true);

            int year = 0;

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Init document

            //Document
            XDocument document = XmlUtil.CreateDocument();

            //Root
            XElement rootElement = new XElement(ROOT + "_" + "ForaReport");

            //EmployeeVacationInformationReport
            XElement foraReportElement = new XElement("ForaReport");

            #endregion

            #region ReportHeader

            XElement reportHeaderElement = CreateTimeReportHeaderElement(reportResult);
            reportHeaderElement.Add(CreateDateIntervalElement(selectionDateFrom, selectionDateTo));
            foraReportElement.Add(reportHeaderElement);

            #endregion

            #region Content

            bool showSocialSec = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId);

            Fora fora = new Fora(parameterObject, reportResult);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var employeeForas = fora.CreateEmployeeForas(entitiesReadOnly, employees, year, selectionTimePeriodIds);

            int employeeXmlId = 0;
            foreach (Employee employee in employees)
            {
                #region Employee element

                XElement employeeElement = new XElement("Employee",
                        new XAttribute("Id", employeeXmlId),
                        new XElement("EmployeeNr", employee.EmployeeNr),
                        new XElement("FirstName", employee.ContactPerson != null ? employee.ContactPerson.FirstName : string.Empty),
                        new XElement("LastName", employee.ContactPerson != null ? employee.ContactPerson.LastName : string.Empty),
                        new XElement("SocialSec", showSocialSec ? employee.SocialSec : StringUtility.SocialSecYYMMDD_Dash_Stars(employee.SocialSec)),
                        new XElement("Note", employee.Note));
                base.personalDataRepository.AddEmployee(employee, employeeElement);

                var employeeFora = employeeForas.FirstOrDefault(e => e.EmployeeId == employee.EmployeeId);
                if (employeeFora != null)
                {
                    XElement employeeForaElement = new XElement("EmployeeFora",
                                               new XAttribute("Id", employeeXmlId),
                                               new XElement("Rapporteringar", employeeFora.Rapporteringar),
                                               new XElement("Avtalsnummer", employeeFora.Avtalsnummer),
                                               new XElement("Personnummer", employeeFora.Personnummer),
                                               new XElement("Namn", employeeFora.Namn),
                                               new XElement("LonforRapporteringsAret", !employeeFora.Tjansteman ? employeeFora.LonforRapporteringsAret : 0),
                                               new XElement("LonforRapporteringsAretTjm", employeeFora.Tjansteman ? employeeFora.LonforRapporteringsAret : 0),
                                               new XElement("LonforRapporteringsAretEjFora", !employeeFora.Tjansteman ? employeeFora.LonforRapporteringsAretEjFora : 0),
                                               new XElement("LonforRapporteringsAretEjForaTjm", employeeFora.Tjansteman ? employeeFora.LonforRapporteringsAretEjFora : 0),
                                               new XElement("SlutatUnderRapporterinsgAret", employeeFora.SlutatUnderRapporterinsgAret),
                                               new XElement("KollektivAvtal", employeeFora.KollektivAvtal),
                                               new XElement("AfaKategori", employee.AFACategory),
                                               new XElement("StartDatum", employeeFora.StartDatum.ToValueOrDefault()),
                                               new XElement("SlutDatum", employeeFora.SlutDatum.ToValueOrDefault()));
                    base.personalDataRepository.AddEmployeeSocialSec(employee);


                    int employeeForaTransactionId = 0;
                    foreach (var trans in employeeFora.Transactions)
                    {
                        XElement employeeForaTransactionElement = new XElement("EmployeeTransactionFora",
                                new XAttribute("Id", employeeForaTransactionId),
                                new XElement("Type", trans.Type),
                                new XElement("PayrollProductNumber", trans.PayrollProductNumber),
                                new XElement("Name", trans.Name),
                                new XElement("Quantity", trans.Quantity),
                                new XElement("Amount", trans.Amount));

                        employeeForaElement.Add(employeeForaTransactionElement);
                    }

                    employeeElement.Add(employeeForaElement);
                }
                else
                {
                    XElement employeeForaElement = new XElement("EmployeeFora",
                        new XAttribute("Id", employeeXmlId),
                        new XElement("Rapporteringar", 0),
                        new XElement("Avtalsnummer", string.Empty),
                        new XElement("Personnummer", string.Empty),
                        new XElement("Namn", string.Empty),
                        new XElement("LonforRapporteringsAret", 0),
                         new XElement("LonforRapporteringsAretTjm", 0),
                        new XElement("LonforRapporteringsAretEjFora", 0),
                         new XElement("LonforRapporteringsAretEjForaTjm", 0),
                        new XElement("SlutatUnderRapporterinsgAret", string.Empty),
                        new XElement("KollektivAvtal", string.Empty),
                        new XElement("AfaKategori", string.Empty),
                        new XElement("StartDatum", CalendarUtility.DATETIME_DEFAULT),
                        new XElement("SlutDatum", CalendarUtility.DATETIME_DEFAULT));

                    XElement employeeForaTransactionElement = new XElement("EmployeeTransactionFora",
                      new XAttribute("Id", 0),
                      new XElement("Type", string.Empty),
                      new XElement("PayrollProductNumber", string.Empty),
                      new XElement("Name", string.Empty),
                      new XElement("Quantity", 0),
                      new XElement("Amount", 0));

                    employeeForaElement.Add(employeeForaTransactionElement);
                    employeeElement.Add(employeeForaElement);
                }


                #endregion

                employeeXmlId++;
                foraReportElement.Add(employeeElement);
            }

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            #region Close document

            rootElement.Add(foraReportElement);
            document.Add(rootElement);

            return GetValidatedDocument(document, reportResult);

            #endregion
        }

        public XDocument Create_Collectum_ReportData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            if (!TryGetIdsFromSelection(reportResult, out List<int> selectionTimePeriodIds, "periods"))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionTimePeriodIds, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return null;

            TryGetDatesFromSelection(reportResult, selectionTimePeriodIds, out DateTime selectionDateFrom, out DateTime selectionDateTo, out var selectedTimePeriods, alwaysLoadPeriods: true);
            TryGetBoolFromSelection(reportResult, out bool setAsFinal, "setAsFinal");

            if (employees == null)
                employees = EmployeeManager.GetAllEmployeesByIds(reportResult.ActorCompanyId, selectionEmployeeIds, loadEmployment: true);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Init document

            //Document
            XDocument document = XmlUtil.CreateDocument();

            //Root
            XElement rootElement = new XElement(ROOT + "_" + "CollectumReport");

            //EmployeeVacationInformationReport
            XElement collectumReportElement = new XElement("CollectumReport");

            #endregion

            #region ReportHeader

            XElement reportHeaderElement = CreateTimeReportHeaderElement(reportResult);
            reportHeaderElement.Add(CreateDateIntervalElement(selectionDateFrom, selectionDateTo));
            collectumReportElement.Add(reportHeaderElement);

            #endregion

            #region Content

            DateTime timeStamp = DateTime.Now;
            int handelsenummer = 1;
            List<int> timePeriodIds = selectedTimePeriods.Select(t => t.TimePeriodId).ToList();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            List<EmployeeTimePeriod> allemployeeTimePeriods = entitiesReadOnly.EmployeeTimePeriod.Include("EmployeeTimePeriodValue").Include("EmployeeTimePeriodProductSetting").Where(w => timePeriodIds.Contains(w.TimePeriodId)).ToList();
            Dictionary<int, List<int>> validEmployeesByTimePeriod = PayrollManager.GetValidEmployeesForTimePeriod(reportResult.ActorCompanyId, timePeriodIds, employees, base.personalDataRepository.PayrollGroups, true, allemployeeTimePeriods);
            bool showSocialSec = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId);
            List<EventHistoryDTO> eventHistories = new List<EventHistoryDTO>();

            int employeeXmlId = 1;
            foreach (int employeeId in selectionEmployeeIds)
            {
                #region Prereq

                Employee employee = employees.FirstOrDefault(i => i.EmployeeId == employeeId);
                if (employee == null || employee.CollectumITPPlan == 0)
                    continue;

                Collectum collectum = new Collectum(parameterObject, reportResult);
                EmployeeCollectum employeeCollectum = null;
                DateTime handelsetidpunkt = CalendarUtility.DATETIME_DEFAULT;

                using (CompEntities entities = new CompEntities())
                {
                    employeeCollectum = collectum.CreateEmployeeCollectum(entities, employee, selectedTimePeriods, validEmployeesByTimePeriod, selectionDateFrom, selectionDateTo, handelsenummer, timeStamp, ref handelsetidpunkt);
                    if (employeeCollectum == null)
                        continue;

                    if (setAsFinal && employeeCollectum.EventHistories.Any())
                    {
                        eventHistories.AddRange(employeeCollectum.EventHistories);
                    }
                }

                if (!employee.ContactPersonReference.IsLoaded)
                    employee.ContactPersonReference.Load();

                #endregion

                #region Employee element

                XElement employeeElement = new XElement("Employee",
                        new XAttribute("Id", employeeXmlId),
                        new XElement("EmployeeNr", employee.EmployeeNr),
                        new XElement("FirstName", employee.ContactPerson != null ? employee.ContactPerson.FirstName : string.Empty),
                        new XElement("LastName", employee.ContactPerson != null ? employee.ContactPerson.LastName : string.Empty),
                        new XElement("SocialSec", showSocialSec ? employee.SocialSec : StringUtility.SocialSecYYMMDD_Dash_Stars(employee.SocialSec)),
                        new XElement("Note", employee.Note),
                        new XElement("CollectumITPPlan", employee.CollectumITPPlan),
                        new XElement("CollectumCostPlace", employee.CollectumCostPlace),
                        new XElement("CollectumAgreedOnProduct", employee.CollectumAgreedOnProduct));
                base.personalDataRepository.AddEmployee(employee, employeeElement);

                #endregion

                #region NyAnmalan element

                XElement nyAnmalanElement = new XElement("Nyanmalan",
                    new XElement("Handelsenummer", employeeCollectum.NyAnmalan.Valid ? employeeCollectum.Handelsenummer : 0),
                    new XElement("Organisationsnummer", employeeCollectum.NyAnmalan.Valid ? employeeCollectum.Organisationsnummer : string.Empty),
                    new XElement("KostnadsstalleId", employeeCollectum.NyAnmalan.Valid ? employeeCollectum.KostnadsstalleId : string.Empty),
                    new XElement("Avtalsplanid", employeeCollectum.NyAnmalan.Valid ? employeeCollectum.Avtalsplanid : string.Empty),
                    new XElement("Personnummer", employeeCollectum.NyAnmalan.Valid ? employeeCollectum.Personnummer : string.Empty),
                    new XElement("Handelsetidpunkt", handelsetidpunkt),
                    new XElement("Efternamn", employeeCollectum.NyAnmalan.Valid ? employeeCollectum.Efternamn : string.Empty),
                    new XElement("Fornamn", employeeCollectum.NyAnmalan.Valid ? employeeCollectum.Fornamn : string.Empty));

                if (employee.CollectumITPPlan == (int)TermGroup_PayrollReportsCollectumITPplan.ITP1)
                {
                    nyAnmalanElement.Add(
                        new XElement("Manadslon", employeeCollectum.Manadslon));
                    nyAnmalanElement.Add(
                        new XElement("AvtaladManadslon", employeeCollectum.AvtaladManadslon));
                }
                else if (employee.CollectumITPPlan == (int)TermGroup_PayrollReportsCollectumITPplan.ITP2)
                {
                    nyAnmalanElement.Add(
                        new XElement("Arslon", employeeCollectum.NyAnmalan.Valid ? employeeCollectum.Arslon : 0),
                        new XElement("ArslonEfterLoneavstaende", employeeCollectum.NyAnmalan.Valid ? employeeCollectum.ArslonEfterLoneavstaende : 0),
                        new XElement("FulltArbetsfor", employeeCollectum.NyAnmalan.Valid ? employeeCollectum.NyAnmalan.FulltArbetsfor.ToInt() : 1),
                        new XElement("GradAvArbetsoformaga", employeeCollectum.NyAnmalan.Valid ? employeeCollectum.GradAvArbetsoformaga : 0),
                        new XElement("VanligITP2", employeeCollectum.NyAnmalan.Valid ? employeeCollectum.NyAnmalan.VanligITP2.ToInt() : 0));
                }
                base.personalDataRepository.AddEmployeeSocialSec(employee);
                employeeElement.Add(nyAnmalanElement);

                #endregion

                #region FlyttAnstalldaInomKoncern element

                XElement flyttAnstalldaInomKoncernElement = new XElement("FlyttAnstalldaInomKoncernElement",
                    new XElement("Handelsenummer", employeeCollectum.FlyttAnstalldaInomKoncern.Valid ? employeeCollectum.Handelsenummer : 0),
                    new XElement("Organisationsnummer", employeeCollectum.FlyttAnstalldaInomKoncern.Valid ? Company.OrgNr : string.Empty),
                    new XElement("KostnadsstalleId", employeeCollectum.FlyttAnstalldaInomKoncern.Valid ? employeeCollectum.KostnadsstalleId : string.Empty),
                    new XElement("Avtalsplanid", employeeCollectum.FlyttAnstalldaInomKoncern.Valid ? employeeCollectum.Avtalsplanid : string.Empty),
                    new XElement("Personnummer", employeeCollectum.FlyttAnstalldaInomKoncern.Valid ? employeeCollectum.Personnummer : string.Empty),
                    new XElement("Handelsetidpunkt", employeeCollectum.FlyttAnstalldaInomKoncern.Valid && employeeCollectum.Handelsetidpunkt > DateTime.Now.AddYears(-100) ? employeeCollectum.Handelsetidpunkt : CalendarUtility.DATETIME_DEFAULT),
                    new XElement("OrganisationsnummerFran", employeeCollectum.FlyttAnstalldaInomKoncern.Valid ? employeeCollectum.FlyttAnstalldaInomKoncern.OrganisationsnummerFran : string.Empty),
                    new XElement("KostnadsstalleIdFran", employeeCollectum.FlyttAnstalldaInomKoncern.Valid ? employeeCollectum.FlyttAnstalldaInomKoncern.KostnadsstalleIdFran : string.Empty));
                base.personalDataRepository.AddEmployeeSocialSec(employee);
                employeeElement.Add(flyttAnstalldaInomKoncernElement);

                #endregion

                #region LoneAndring element

                XElement lonAndringElement = new XElement("Loneandring",
                    new XElement("Handelsenummer", employeeCollectum.LoneAndring.Valid ? employeeCollectum.Handelsenummer : 0),
                    new XElement("Organisationsnummer", employeeCollectum.LoneAndring.Valid ? Company.OrgNr : string.Empty),
                    new XElement("KostnadsstalleId", employeeCollectum.LoneAndring.Valid ? employeeCollectum.KostnadsstalleId : string.Empty),
                    new XElement("Avtalsplanid", employeeCollectum.LoneAndring.Valid ? employeeCollectum.Avtalsplanid : string.Empty),
                    new XElement("Personnummer", employeeCollectum.LoneAndring.Valid ? employeeCollectum.Personnummer : string.Empty),
                    new XElement("Handelsetidpunkt", handelsetidpunkt));

                if (employee.CollectumITPPlan == (int)TermGroup_PayrollReportsCollectumITPplan.ITP1)
                {
                    lonAndringElement.Add(new XElement("Manadslon", employeeCollectum.Manadslon));

                }
                else if (employee.CollectumITPPlan == (int)TermGroup_PayrollReportsCollectumITPplan.ITP2)
                {
                    lonAndringElement.Add(
                        new XElement("Arslon", employeeCollectum.LoneAndring.Valid ? employeeCollectum.Arslon : 0),
                        new XElement("ArslonEfterLoneavstaende", employeeCollectum.LoneAndring.Valid ? employeeCollectum.ArslonEfterLoneavstaende : 0),
                        new XElement("GradAvArbetsoformaga", employeeCollectum.LoneAndring.Valid ? employeeCollectum.GradAvArbetsoformaga : 0));
                }

                employeeElement.Add(lonAndringElement);

                #endregion

                #region AvAnmalan element

                XElement avAnmalanElement = new XElement("Avanmalan",
                    new XElement("Handelsenummer", employeeCollectum.AvAnmalan.Valid ? employeeCollectum.Handelsenummer : 0),
                    new XElement("Organisationsnummer", employeeCollectum.AvAnmalan.Valid ? Company.OrgNr : string.Empty),
                    new XElement("KostnadsstalleId", employeeCollectum.AvAnmalan.Valid ? employeeCollectum.KostnadsstalleId : string.Empty),
                    new XElement("Avtalsplanid", employeeCollectum.AvAnmalan.Valid ? employeeCollectum.Avtalsplanid : string.Empty),
                    new XElement("Personnummer", employeeCollectum.AvAnmalan.Valid ? employeeCollectum.Personnummer : string.Empty),
                    new XElement("Handelsetidpunkt", employeeCollectum.AvAnmalan.Valid && employeeCollectum.Handelsetidpunkt > DateTime.Now.AddYears(-100) ? employeeCollectum.Handelsetidpunkt : CalendarUtility.DATETIME_DEFAULT),
                    new XElement("Avgangsorsak", employeeCollectum.AvAnmalan.Valid ? employeeCollectum.AvAnmalan.Avgangsorsak : 0),
                    new XElement("DatumForForaldraledighet", employeeCollectum.AvAnmalan.Valid ? employeeCollectum.AvAnmalan.DatumForForaldraledighet.ToValueOrDefault() : CalendarUtility.DATETIME_DEFAULT));
                base.personalDataRepository.AddEmployeeSocialSec(employee);
                employeeElement.Add(avAnmalanElement);

                #endregion

                handelsenummer++;
                collectumReportElement.Add(employeeElement);
            }

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            #region Close document

            rootElement.Add(collectumReportElement);
            document.Add(rootElement);

            var doc = GetValidatedDocument(document, reportResult);

            if (doc != null && setAsFinal && eventHistories.Any())
            {
                using (CompEntities entities = new CompEntities())
                {
                    GeneralManager.SaveEventHistories(entities, eventHistories, reportResult.ActorCompanyId);
                }
            }

            return doc;
            #endregion
        }

        public XDocument Create_AGD_ReportData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            if (!TryGetIdsFromSelection(reportResult, out List<int> selectionTimePeriodIds, "periods"))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionTimePeriodIds, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return null;

            TryGetDatesFromSelection(reportResult, selectionTimePeriodIds, out DateTime selectionDateFrom, out DateTime selectionDateTo, out var selectedTimePeriods, alwaysLoadPeriods: true);
            List<TimePeriod> timePeriods = selectedTimePeriods?.Where(x => x.PaymentDate.HasValue).ToList();
            if (timePeriods.IsNullOrEmpty())
                return null;

            TryGetBoolFromSelection(reportResult, out bool removePrevSubmittedData, "removePrevSubmittedData");
            removePrevSubmittedData = reportResult.EvaluatedSelection?.ST_KU10RemovePrevSubmittedData ?? removePrevSubmittedData;

            if (employees == null)
                employees = EmployeeManager.GetAllEmployeesByIds(base.ActorCompanyId, selectionEmployeeIds, loadEmployment: true);

            int maxAvdragRegionaltStod = Convert.ToInt32(SettingManager.GetDecimalSetting(SettingMainType.Company, (int)CompanySettingType.PayrollMaxRegionalSupportAmount, 0, reportResult.ActorCompanyId, 0));
            int avdragRegionaltStodProcent = Convert.ToInt32(SettingManager.GetDecimalSetting(SettingMainType.Company, (int)CompanySettingType.PayrollMaxRegionalSupportPercent, 0, reportResult.ActorCompanyId, 0));

            DateTime date = timePeriods.OrderByDescending(i => i.PaymentDate).FirstOrDefault().PaymentDate.Value;
            if (date < new DateTime(2018, 7, 1))
                date = new DateTime(2018, 7, 1);

            string period = "";
            if (date > DateTime.Now.AddYears(-10))
            {
                string month = $"{(date.Month.ToString().Length == 1 ? $"0{date.Month}" : date.Month.ToString())}";
                period = $"{date.Year}{month}";
            }

            bool showSocialSec = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Init document

            //Document
            XDocument document = XmlUtil.CreateDocument();

            //Root
            XElement rootElement = new XElement(ROOT + "_" + "AgdReport");

            //AgdReport
            XElement employerDeclarationIndividualReportElement = new XElement("AgdReport");

            #endregion

            #region ReportHeader

            XElement reportHeaderElement = CreateTimeReportHeaderElement(reportResult);
            reportHeaderElement.Add(CreateDateIntervalElement(selectionDateFrom, selectionDateTo));


            #endregion

            #region Content

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                List<TimePayrollStatisticsSmallDTO> timePayrollTransactionItems = TimeTransactionManager.GetTimePayrollStatisticsSmallDTOs_new(entities, reportResult.ActorCompanyId, employees, selectionTimePeriodIds, applyEmploymentTaxMinimumRule: true, ignoreAccounting: true, isAgd: true);

                KU10 ku10 = new KU10(parameterObject, reportResult);
                List<AgdEmployeeDTO> agdEmployeeDTOs = new List<AgdEmployeeDTO>();

                foreach (var employee in employees)
                {
                    EmployeeTaxSE taxSe = EmployeeManager.GetEmployeeTaxSE(employee.EmployeeId, date.Year);
                    List<TimePayrollStatisticsSmallDTO> timePayrollTransactionItemsEmployee = timePayrollTransactionItems.Where(t => t.EmployeeId == employee.EmployeeId).ToList();
                    var agd = ku10.CreateAgdEmployeeDTO(timePayrollTransactionItemsEmployee, employee, period, taxSe, selectionDateFrom, selectionDateTo, removePrevSubmittedData, date);
                    if (agd == null)
                        continue;

                    var grossAmount = timePayrollTransactionItems.Where(w => w.IsGrossSalary()).Sum(s => s.Amount);
                    var benefitAmount = timePayrollTransactionItems.Where(w => w.IsBenefit_Not_CompanyCar_And_FuelBenefit()).Sum(s => s.Amount);
                    var carBenefitAmount = timePayrollTransactionItems.Where(w => w.IsBenefit_CompanyCar()).Sum(s => s.Amount);
                    var taxAmount = timePayrollTransactionItems.Where(w => w.IsTax()).Sum(s => s.Amount);

                    if (Math.Abs(grossAmount) < 1 && Math.Abs(taxAmount) < 1 && Math.Abs(benefitAmount) < 1 && Math.Abs(carBenefitAmount) < 1)
                        continue;

                    agdEmployeeDTOs.Add(agd);
                }
                agdEmployeeDTOs = agdEmployeeDTOs.Where(w => w != null).ToList();

                //Summerade
                var sum = 0;
                foreach (var group in agdEmployeeDTOs.Where(w => w.KontantErsattningEjUlagSA == 0 || w.SkatteplOvrigaFormanerEjUlagSA == 0).GroupBy(g => g.EmploymentTaxRate))
                {
                    var sumGroup = group.Sum(s => s.DecCalculatedArbetsgivarintygKredit);
                    sum += NumberUtility.AmountToInt(sumGroup, true);
                }

                maxAvdragRegionaltStod = ku10.CalulateAvdragRegionaltStod(avdragRegionaltStodProcent, maxAvdragRegionaltStod, agdEmployeeDTOs.Sum(s => s.RegionaltStodUlagAG));
                reportHeaderElement.Add(new XElement("AgRegistreradId", Company.OrgNr));
                reportHeaderElement.Add(new XElement("Arendeagare", Company.OrgNr));
                reportHeaderElement.Add(new XElement("Period", period));
                reportHeaderElement.Add(new XElement("RedovisningsPeriod", period));
                reportHeaderElement.Add(new XElement("AvdragRegionaltStod", maxAvdragRegionaltStod));
                reportHeaderElement.Add(new XElement("SummaArbAvgSlf", sum));
                reportHeaderElement.Add(new XElement("SummaSkatteavdr", agdEmployeeDTOs.Sum(i => i.AvdrPrelSkatt + i.AvdrSkattSINK + i.AvdrSkattASINK)));
                reportHeaderElement.Add(new XElement("TotalSjuklonekostnad", agdEmployeeDTOs.Sum(i => i.Sjuklön)));
                employerDeclarationIndividualReportElement.Add(reportHeaderElement);

                #endregion

                int employeeXmlId = 1;
                foreach (var employee in employees)
                {
                    #region Prereq

                    Contact contact = EmployeeManager.GetEmployeeContact(entities, employee.EmployeeId);

                    AgdEmployeeDTO agdEmployeeDTO = agdEmployeeDTOs.FirstOrDefault(f => f.EmployeeId == employee.EmployeeId);
                    if (agdEmployeeDTO == null || agdEmployeeDTO.Transactions.Count == 0)
                        continue;

                    #endregion

                    #region Employee element

                    XElement agdAnstalld = new XElement("AgdAnstalld",
                        new XAttribute("Id", employeeXmlId),
                        new XElement("EmployeeNr", employee.EmployeeNr),
                        new XElement("EmployeeName", employee.Name),
                        new XElement("EmployeeSocialSec", employee.ContactPerson != null && showSocialSec ? employee.SocialSec : StringUtility.SocialSecYYMMDD_Dash_Stars(employee.SocialSec)),
                        new XElement("EmployeeSex", employee.ContactPerson != null ? employee.ContactPerson.Sex : 0),
                        new XElement("UserName", employee.User != null ? employee.User.LoginName : string.Empty),
                        new XElement("Note", employee.Note));
                    base.personalDataRepository.AddEmployee(employee, agdAnstalld);

                    if (contact != null)
                        agdAnstalld.Add(CreateTimeEmployeeContactInformationElements(entities, contact));
                    agdAnstalld.Add(new XElement("ForstaAnstalld", agdEmployeeDTO.ForstaAnstalld.ToInt()));
                    agdAnstalld.Add(new XElement("ArbetsArbAvgSlf", (agdEmployeeDTO.KontantErsattningEjUlagSA == 0 && agdEmployeeDTO.SkatteplOvrigaFormanerEjUlagSA == 0) ? agdEmployeeDTO.ArbetsArbAvgSlf : 0));
                    agdAnstalld.Add(new XElement("Sjuklön", agdEmployeeDTO.Sjuklön));
                    agdAnstalld.Add(new XElement("Borttag", agdEmployeeDTO.Borttag.ToInt()));
                    agdAnstalld.Add(new XElement("Arbetsstallenummer", StringUtility.CleanString(agdEmployeeDTO.Arbetsstallenummer)));
                    agdAnstalld.Add(new XElement("AvdrPrelSkatt", agdEmployeeDTO.AvdrPrelSkatt));
                    agdAnstalld.Add(new XElement("AvdrSkattSINK", agdEmployeeDTO.AvdrSkattSINK));
                    agdAnstalld.Add(new XElement("AvdrSkattASINK", agdEmployeeDTO.AvdrSkattASINK));
                    agdAnstalld.Add(new XElement("SkattebefrEnlAvtal", StringUtility.CleanString(agdEmployeeDTO.SkattebefrEnlAvtal)));
                    agdAnstalld.Add(new XElement("Lokalanstalld", StringUtility.CleanString(agdEmployeeDTO.Lokalanstalld)));
                    agdAnstalld.Add(new XElement("AmbassadanstISvMAvtal", StringUtility.CleanString(agdEmployeeDTO.AmbassadanstISvMAvtal)));
                    agdAnstalld.Add(new XElement("EjskatteavdragEjbeskattningSv", StringUtility.CleanString(agdEmployeeDTO.EjskatteavdragEjbeskattningSv)));
                    agdAnstalld.Add(new XElement("AvrakningAvgiftsfriErs", agdEmployeeDTO.AvrakningAvgiftsfriErs));
                    agdAnstalld.Add(new XElement("KontantErsattningUlagAG", agdEmployeeDTO.KontantErsattningUlagAG));
                    agdAnstalld.Add(new XElement("SkatteplOvrigaFormanerUlagAG", agdEmployeeDTO.SkatteplOvrigaFormanerUlagAG));
                    agdAnstalld.Add(new XElement("SkatteplBilformanUlagAG", agdEmployeeDTO.SkatteplBilformanUlagAG));
                    agdAnstalld.Add(new XElement("DrivmVidBilformanUlagAG", agdEmployeeDTO.DrivmVidBilformanUlagAG));
                    agdAnstalld.Add(new XElement("AvdragUtgiftArbetet", agdEmployeeDTO.AvdragUtgiftArbetet));
                    agdAnstalld.Add(new XElement("BostadsformanSmahusUlagAG", agdEmployeeDTO.BostadsformanSmahusUlagAG.ToInt()));
                    agdAnstalld.Add(new XElement("BostadsformanEjSmahusUlagAG", agdEmployeeDTO.BostadsformanEjSmahusUlagAG.ToInt()));
                    agdAnstalld.Add(new XElement("Bilersattning", agdEmployeeDTO.Bilersattning.ToInt()));
                    agdAnstalld.Add(new XElement("Traktamente", agdEmployeeDTO.Traktamente.ToInt()));
                    agdAnstalld.Add(new XElement("AndraKostnadsers", agdEmployeeDTO.AndraKostnadsers));
                    agdAnstalld.Add(new XElement("KontantErsattningEjUlagSA", agdEmployeeDTO.KontantErsattningEjUlagSA));
                    agdAnstalld.Add(new XElement("SkatteplOvrigaFormanerEjUlagSA", agdEmployeeDTO.SkatteplOvrigaFormanerEjUlagSA));
                    agdAnstalld.Add(new XElement("SkatteplBilformanEjUlagSA", agdEmployeeDTO.SkatteplBilformanEjUlagSA));
                    agdAnstalld.Add(new XElement("DrivmVidBilformanEjUlagSA", agdEmployeeDTO.DrivmVidBilformanEjUlagSA));
                    agdAnstalld.Add(new XElement("FormanSomPensionEjUlagSA", agdEmployeeDTO.FormanSomPensionEjUlagSA));
                    agdAnstalld.Add(new XElement("BostadsformanSmahusEjUlagSA", agdEmployeeDTO.BostadsformanSmahusEjUlagSA));
                    agdAnstalld.Add(new XElement("BostadsformanEjSmahusEjUlagSA", agdEmployeeDTO.BostadsformanEjSmahusEjUlagSA));
                    agdAnstalld.Add(new XElement("ErsEjSocAvgEjJobbavd", agdEmployeeDTO.ErsEjSocAvgEjJobbavd));
                    agdAnstalld.Add(new XElement("ErsattningsKod1", StringUtility.CleanString(agdEmployeeDTO.ErsattningsKod1)));
                    agdAnstalld.Add(new XElement("ErsattningsKod2", StringUtility.CleanString(agdEmployeeDTO.ErsattningsKod2)));
                    agdAnstalld.Add(new XElement("ErsattningsKod3", StringUtility.CleanString(agdEmployeeDTO.ErsattningsKod3)));
                    agdAnstalld.Add(new XElement("ErsattningsKod4", StringUtility.CleanString(agdEmployeeDTO.ErsattningsKod4)));
                    agdAnstalld.Add(new XElement("ErsattningsBelopp1", StringUtility.CleanString(agdEmployeeDTO.ErsattningsBelopp1)));
                    agdAnstalld.Add(new XElement("ErsattningsBelopp2", StringUtility.CleanString(agdEmployeeDTO.ErsattningsBelopp2)));
                    agdAnstalld.Add(new XElement("ErsattningsBelopp3", StringUtility.CleanString(agdEmployeeDTO.ErsattningsBelopp3)));
                    agdAnstalld.Add(new XElement("ErsattningsBelopp4", StringUtility.CleanString(agdEmployeeDTO.ErsattningsBelopp4)));
                    agdAnstalld.Add(new XElement("Tjanstepension", agdEmployeeDTO.Tjanstepension));
                    agdAnstalld.Add(new XElement("Forskarskattenamnden", StringUtility.CleanString(agdEmployeeDTO.Forskarskattenamnden)));
                    agdAnstalld.Add(new XElement("VissaAvdrag", StringUtility.CleanString(agdEmployeeDTO.VissaAvdrag)));
                    agdAnstalld.Add(new XElement("ErsFormanBostadMmSINK", StringUtility.CleanString(agdEmployeeDTO.ErsFormanBostadMmSINK)));
                    agdAnstalld.Add(new XElement("LandskodArbetsland", StringUtility.CleanString(agdEmployeeDTO.LandskodArbetsland)));
                    agdAnstalld.Add(new XElement("UtsandUnderTid", StringUtility.CleanString(agdEmployeeDTO.UtsandUnderTid)));
                    agdAnstalld.Add(new XElement("KonventionMed", StringUtility.CleanString(agdEmployeeDTO.KonventionMed)));
                    agdAnstalld.Add(new XElement("KontantErsattningUlagEA", StringUtility.CleanString(agdEmployeeDTO.KontantErsattningUlagEA)));
                    agdAnstalld.Add(new XElement("SkatteplOvrigaFormanerUlagEA", StringUtility.CleanString(agdEmployeeDTO.SkatteplOvrigaFormanerUlagEA)));
                    agdAnstalld.Add(new XElement("SkatteplBilformanUlagEA", StringUtility.CleanString(agdEmployeeDTO.SkatteplBilformanUlagEA)));
                    agdAnstalld.Add(new XElement("DrivmVidBilformanUlagEA", StringUtility.CleanString(agdEmployeeDTO.DrivmVidBilformanUlagEA)));
                    agdAnstalld.Add(new XElement("BostadsformanSmahusUlagEA", StringUtility.CleanString(agdEmployeeDTO.BostadsformanSmahusUlagEA)));
                    agdAnstalld.Add(new XElement("BostadsformanEjSmahusUlagEA", StringUtility.CleanString(agdEmployeeDTO.BostadsformanEjSmahusUlagEA)));
                    agdAnstalld.Add(new XElement("Fartygssignal", StringUtility.CleanString(agdEmployeeDTO.Fartygssignal)));
                    agdAnstalld.Add(new XElement("AntalDagarSjoinkomst", StringUtility.CleanString(agdEmployeeDTO.AntalDagarSjoinkomst)));
                    agdAnstalld.Add(new XElement("NarfartFjarrfart", StringUtility.CleanString(agdEmployeeDTO.NarfartFjarrfart)));
                    agdAnstalld.Add(new XElement("FartygetsNamn", StringUtility.CleanString(agdEmployeeDTO.FartygetsNamn)));
                    agdAnstalld.Add(new XElement("UnderlagRutarbete", agdEmployeeDTO.UnderlagRutarbete));
                    agdAnstalld.Add(new XElement("UnderlagRotarbete", agdEmployeeDTO.UnderlagRotarbete));
                    agdAnstalld.Add(new XElement("Hyresersattning", agdEmployeeDTO.Hyresersattning));
                    agdAnstalld.Add(new XElement("VerksamhetensArt", agdEmployeeDTO.VerksamhetensArt));
                    agdAnstalld.Add(new XElement("FormanHarJusterats", agdEmployeeDTO.FormanHarJusterats.ToInt()));
                    agdAnstalld.Add(new XElement("Personaloption", StringUtility.CleanString(agdEmployeeDTO.Personaloption)));
                    agdAnstalld.Add(new XElement("TimePeriodName", agdEmployeeDTO.TimePeriodName));
                    agdAnstalld.Add(new XElement("ArbetsplatsensGatuadress", agdEmployeeDTO.PlaceOfEmploymentAddress));
                    agdAnstalld.Add(new XElement("ArbetsplatsensOrt", agdEmployeeDTO.PlaceOfEmploymentCity));
                    agdAnstalld.Add(new XElement("AndelStyrelsearvode", agdEmployeeDTO.AndelStyrelsearvode));
                    agdAnstalld.Add(new XElement("Fodelseort", agdEmployeeDTO.Fodelseort));
                    agdAnstalld.Add(new XElement("LandskodFodelseort", agdEmployeeDTO.LandskodFodelseort));
                    agdAnstalld.Add(new XElement("LandskodMedborgare", agdEmployeeDTO.LandskodMedborgare));

                    #endregion

                    #region Transactions element

                    int transactionsXmlId = 0;
                    foreach (var transaction in agdEmployeeDTO.Transactions)
                    {
                        XElement transElement = new XElement("Transactions",
                            new XAttribute("Id", transactionsXmlId),
                            new XElement("Type", transaction.Type),
                            new XElement("PayrollProductNumber", transaction.PayrollProductNumber),
                            new XElement("Name", transaction.Name),
                            new XElement("Quantity", transaction.Quantity),
                            new XElement("Amount", transaction.Amount),
                            new XElement("IsPayrollStartValue", transaction.IsPayrollStartValue.ToInt()));

                        agdAnstalld.Add(transElement);
                        transactionsXmlId++;
                    }

                    #endregion

                    employeeXmlId++;
                    employerDeclarationIndividualReportElement.Add(agdAnstalld);
                }
            }

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            #region Close document

            rootElement.Add(employerDeclarationIndividualReportElement);
            document.Add(rootElement);

            return GetValidatedDocument(document, reportResult);

            #endregion
        }

        #endregion

        #region Report XML

        public XDocument CreateCertificateOfEmploymentReportData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            TryGetDateFromSelection(reportResult, out DateTime selectionDate, "date");

            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDate, selectionDate, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return null;

            TryGetBoolFromSelection(reportResult, out bool sendToArbetsgivarIntyg, "sendToArbetsgivarIntygNu");
            TryGetSpecialFromSelection(reportResult, out string selectionSpecial);

            if (employees == null)
                employees = EmployeeManager.GetAllEmployeesByIds(reportResult.ActorCompanyId, selectionEmployeeIds, loadEmployment: true);

            var arbetsgivarintygnuApiNyckel = sendToArbetsgivarIntyg ? SettingManager.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.PayrollArbetsgivarintygnuApiNyckel, 0, Company.ActorCompanyId, 0) : string.Empty;
            var arbetsgivarintygnuArbetsgivarId = sendToArbetsgivarIntyg ? SettingManager.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.PayrollArbetsgivarintygnuArbetsgivarId, 0, Company.ActorCompanyId, 0) : string.Empty;
            bool hasArbetsgivarintygSetting = !string.IsNullOrEmpty(arbetsgivarintygnuApiNyckel) && !string.IsNullOrEmpty(arbetsgivarintygnuArbetsgivarId);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Init document

            XDocument document = reportResult.Input.Document;
            XElement rootElement = reportResult.Input.ElementRoot;
            XElement certificateOfEmploymentElement = reportResult.Input.ElementFirst;

            #endregion

            #region ReportHeader

            XElement reportHeaderElement = CreateTimeReportHeaderElement(reportResult);
            reportHeaderElement.Add(CreateDateIntervalElement(selectionDate, selectionDate));
            certificateOfEmploymentElement.Add(reportHeaderElement);

            #endregion

            #region Content

            CertificateOfEmployment certificateOfEmployment = new CertificateOfEmployment(parameterObject, reportResult, employees, selectionDate, selectionSpecial, sendToArbetsgivarIntyg);

            if (!string.IsNullOrEmpty(certificateOfEmployment.ErrorMessage))
                return null;

            if (hasArbetsgivarintygSetting && sendToArbetsgivarIntyg)
            {
                ArbetsgivarintygPunktNu arbetsgivarintyg = new ArbetsgivarintygPunktNu(CompDbCache.Instance.SiteType != TermGroup_SysPageStatusSiteType.Live, arbetsgivarintygnuApiNyckel, arbetsgivarintygnuArbetsgivarId);
                arbetsgivarintyg.SendArbetsgivarIntyg(certificateOfEmployment);
            }

            int employeeXmlId = 1;
            foreach (var certificateOfEmploymentEmployee in certificateOfEmployment.CertificateOfEmploymentEmployees)
            {
                #region Employee element

                XElement employeeElement = new XElement("Employee",
                    new XAttribute("Id", employeeXmlId),
                    new XElement("EmployeeNr", certificateOfEmploymentEmployee.EmployeeNr),
                    new XElement("FirstName", certificateOfEmploymentEmployee.FirstName),
                    new XElement("LastName", certificateOfEmploymentEmployee.LastName),
                    new XElement("SocialSec", certificateOfEmploymentEmployee.SocialSec),
                    new XElement("Note", certificateOfEmploymentEmployee.Note));
                base.personalDataRepository.AddEmployee(certificateOfEmploymentEmployee.Employee, employeeElement);

                #endregion

                #region Employments

                foreach (var certificateOfEmploymentEmployment in certificateOfEmploymentEmployee.CertificateOfEmploymentMergedEmployments.OrderByDescending(o => o.DateFrom).Take(1))
                {
                    #region Employment element
                    int employmentXmlId = 1;

                    XElement employmentElement = new XElement("Employment",
                                                new XAttribute("Id", employmentXmlId),
                                                new XElement("DateFrom", certificateOfEmploymentEmployment.DateFrom),
                                                new XElement("DateTo", certificateOfEmploymentEmployment.DateTo),
                                                new XElement("PositionCode", certificateOfEmploymentEmployment.PositionCode),
                                                new XElement("PositionName", certificateOfEmploymentEmployment.PositionName),
                                                new XElement("EmploymentType", (int)certificateOfEmploymentEmployment.EmploymentType),
                                                new XElement("EmploymentTypeName", certificateOfEmploymentEmployment.EmploymentTypeName),
                                                new XElement("WorkTime", certificateOfEmploymentEmployment.WorkTime),
                                                new XElement("WorkTimeEmployeeGroup", certificateOfEmploymentEmployment.WorkTimeEmployeeGroup),
                                                new XElement("WorkPercent", certificateOfEmploymentEmployment.WorkPercent),
                                                new XElement("EndReason", certificateOfEmploymentEmployment.EndReason),
                                                new XElement("EndReasonName", certificateOfEmploymentEmployment.EndReasonName),
                                                new XElement("EndReasonCode", certificateOfEmploymentEmployment.EndReasonCode),
                                                new XElement("PayrollYear", certificateOfEmploymentEmployment.PayrollYear),
                                                new XElement("TypeOfSalary", certificateOfEmploymentEmployment.SalaryType),
                                                new XElement("PayrollAmount", certificateOfEmploymentEmployment.PayrollAmount),
                                                new XElement("OverTimePerHour", certificateOfEmploymentEmployment.OverTimePerHour),
                                                new XElement("AddedTimePerHour", certificateOfEmploymentEmployment.AddedTimePerHour),
                                                new XElement("HasOtherDisbursementTransactions", certificateOfEmploymentEmployment.HasOtherDisbursementTransactions.ToInt()));


                    #endregion

                    #region Transactions



                    #endregion

                    #region Group filteredtransItemslevel


                    int groupTranXmlId = 1;
                    if (certificateOfEmploymentEmployment.GroupTrans.Any() && employmentXmlId == 1)
                    {
                        foreach (var intervalGroup in certificateOfEmploymentEmployment.GroupTrans)
                        {
                            #region GroupTrans Element

                            XElement groupTransElement = new XElement("GroupTrans",
                                new XAttribute("Id", groupTranXmlId),
                                new XElement("Type", intervalGroup.Type),
                                new XElement("DateFrom", intervalGroup.DateFrom),
                                new XElement("WorkSum", intervalGroup.WorkSum),
                                new XElement("AbsenceSum", intervalGroup.AbsenceSum),
                                new XElement("OverTimeSum", intervalGroup.OverTimeSum),
                                new XElement("AddedTimeSum", intervalGroup.AddedTimeSum));

                            #endregion

                            #region Transactions element

                            int transactionsXmlId = 0;
                            if (intervalGroup.FilteredtransItems.Any())
                            {
                                foreach (var trans in intervalGroup.FilteredtransItems)
                                {
                                    XElement transactionsElement = new XElement("Transactions",
                                        new XAttribute("Id", transactionsXmlId),
                                        new XElement("PayrollProductNumber", trans.PayrollProductNumber),
                                        new XElement("Name", trans.PayrollProductName),
                                        new XElement("Date", trans.TimeBlockDate),
                                        new XElement("Quantity", trans.Quantity),
                                        new XElement("Amount", trans.Amount),
                                        new XElement("SysPayrollTypeLevel1", trans.SysPayrollTypeLevel1 ?? 0),
                                        new XElement("SysPayrollTypeLevel2", trans.SysPayrollTypeLevel2 ?? 0),
                                        new XElement("SysPayrollTypeLevel3", trans.SysPayrollTypeLevel3 ?? 0),
                                        new XElement("SysPayrollTypeLevel4", trans.SysPayrollTypeLevel4 ?? 0),
                                        new XElement("SysPayrollTypeLevel1Name", trans.SysPayrollTypeLevel1Name),
                                        new XElement("SysPayrollTypeLevel2Name", trans.SysPayrollTypeLevel2Name),
                                        new XElement("SysPayrollTypeLevel3Name", trans.SysPayrollTypeLevel3Name),
                                        new XElement("SysPayrollTypeLevel4Name", trans.SysPayrollTypeLevel4Name));

                                    transactionsXmlId++;
                                    groupTransElement.Add(transactionsElement);
                                }

                                groupTranXmlId++;
                                employmentElement.Add(groupTransElement);
                            }
                            else
                            {
                                AddDefaultElement(reportResult, groupTransElement, "Transactions");
                            }

                            #endregion
                        }
                    }
                    else
                    {
                        XElement groupTransElement = GetDefaultElement(reportResult, "GroupTrans");
                        if (groupTransElement != null)
                        {
                            XElement transactionsElement = GetDefaultElement(reportResult, "Transactions");
                            if (transactionsElement != null)
                                groupTransElement.Add(transactionsElement);
                            employmentElement.Add(groupTransElement);
                        }
                    }

                    #endregion

                    #region Group filteredOtherDisbursementTransItems


                    int otherTransactionsXmlId = 0;
                    int groupTransXmlId = 1;
                    if (certificateOfEmploymentEmployment.OtherGroupTrans.Any())
                    {
                        foreach (var intervalGroup in certificateOfEmploymentEmployment.OtherGroupTrans)
                        {

                            #region GroupOtherTrans element

                            XElement groupTransElement = new XElement("GroupOtherTrans",
                            new XAttribute("Id", groupTransXmlId),
                            new XElement("Type", intervalGroup.Type),
                            new XElement("PayrollProductNumber", intervalGroup.PayrollProductNumber),
                            new XElement("Name", intervalGroup.Name),
                            new XElement("DateFrom", intervalGroup.DateFrom),
                            new XElement("Days", intervalGroup.Days),
                            new XElement("Amount", intervalGroup.Amount),
                            new XElement("Quantity", intervalGroup.Quantity),
                            new XElement("IsDutyOrStandby", intervalGroup.IsDutyOrStandby.ToInt()));

                            #endregion

                            #region OtherTransactions element

                            foreach (var timePayrollStatisticsDTO in intervalGroup.FilteredtransItems)
                            {
                                XElement transElement = new XElement("OtherTransactions",
                                    new XAttribute("Id", otherTransactionsXmlId),
                                    new XElement("PayrollProductNumber", timePayrollStatisticsDTO.PayrollProductNumber),
                                    new XElement("Name", timePayrollStatisticsDTO.PayrollProductName),
                                    new XElement("Date", timePayrollStatisticsDTO.TimeBlockDate),
                                    new XElement("Quantity", timePayrollStatisticsDTO.Quantity),
                                    new XElement("Amount", timePayrollStatisticsDTO.Amount),
                                    new XElement("SysPayrollTypeLevel1", timePayrollStatisticsDTO.SysPayrollTypeLevel1 ?? 0),
                                    new XElement("SysPayrollTypeLevel2", timePayrollStatisticsDTO.SysPayrollTypeLevel2 ?? 0),
                                    new XElement("SysPayrollTypeLevel3", timePayrollStatisticsDTO.SysPayrollTypeLevel3 ?? 0),
                                    new XElement("SysPayrollTypeLevel4", timePayrollStatisticsDTO.SysPayrollTypeLevel4 ?? 0),
                                    new XElement("SysPayrollTypeLevel1Name", timePayrollStatisticsDTO.SysPayrollTypeLevel1Name),
                                    new XElement("SysPayrollTypeLevel2Name", timePayrollStatisticsDTO.SysPayrollTypeLevel2Name),
                                    new XElement("SysPayrollTypeLevel3Name", timePayrollStatisticsDTO.SysPayrollTypeLevel3Name),
                                    new XElement("SysPayrollTypeLevel4Name", timePayrollStatisticsDTO.SysPayrollTypeLevel4Name));

                                otherTransactionsXmlId++;
                                groupTransElement.Add(transElement);
                            }

                            groupTransXmlId++;
                            employmentElement.Add(groupTransElement);

                            #endregion

                        }
                    }
                    else
                    {
                        XElement groupTransElement = GetDefaultElement(reportResult, "GroupOtherTrans");
                        if (groupTransElement != null)
                        {
                            XElement otherTransactionsElement = GetDefaultElement(reportResult, "OtherTransactions");
                            if (otherTransactionsElement != null)
                                groupTransElement.Add(otherTransactionsElement);
                            employmentElement.Add(groupTransElement);
                        }
                    }

                    #endregion

                    employeeElement.Add(employmentElement);
                    employmentXmlId++;
                }

                certificateOfEmploymentElement.Add(employeeElement);

                #endregion

                employeeXmlId++;
            }



            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            #region Close document

            rootElement.Add(certificateOfEmploymentElement);
            document.Add(rootElement);

            return GetValidatedDocument(document, reportResult.ReportTemplateType);

            #endregion
        }

        public XDocument CreateEmploymentContractData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            DateTime selectionDate;
            List<DateTime> substituteDates;

            TryGetBoolFromSelection(reportResult, out bool isPrintedFromSchedulePlanning, "isPrintedFromSchedulePlanning");
            if (isPrintedFromSchedulePlanning)
            {
                if (!TryGetDatesFromSelection(reportResult, out substituteDates))
                    return null;

                selectionDate = substituteDates.OrderByDescending(i => i.Date).FirstOrDefault();
            }
            else
            {
                if (!TryGetDateFromSelection(reportResult, out selectionDate))
                    return null;

                substituteDates = new List<DateTime>() { selectionDate };
            }

            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDate, selectionDate, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return null;

            int? employmentId = 0;
            if (selectionEmployeeIds.Count == 1)
                TryGetIdFromSelection(reportResult, out employmentId, key: "employmentId");

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Init document

            XDocument document = reportResult.Input.Document;
            XElement rootElement = reportResult.Input.ElementRoot;
            XElement employmentContractElement = reportResult.Input.ElementFirst;

            #endregion

            #region ReportHeaderLabels

            XElement reportHeaderLabelsElement = CreateTimeReportHeaderLabelsElement();
            reportHeaderLabelsElement.Add(CreateDateIntervalLabelReportHeaderLabelsElement());
            employmentContractElement.Add(reportHeaderLabelsElement);

            #endregion

            #region ReportHeader

            XElement reportHeaderElement = CreateExtendedPersonellReportHeaderElement(reportResult);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            reportHeaderElement.Add(new XElement("CompanyLogo", GetCompanyLogoFilePath(entitiesReadOnly, reportResult.ActorCompanyId, false)));
            reportHeaderElement.Add(new XElement("PrintedFromScheduleplanning", isPrintedFromSchedulePlanning.ToInt()));

            #region ReportSettings

            List<ReportSetting> reportSettingList = reportResult.Input.Report.ReportSetting?.ToList() ?? new List<ReportSetting>();
            AddReportSettingElements(reportSettingList, reportHeaderElement);

            #endregion

            employmentContractElement.Add(reportHeaderElement);

            #endregion

            #region PageHeaderLabels

            XElement pageHeaderLabelsElement = new XElement("PageHeaderLabels");
            employmentContractElement.Add(pageHeaderLabelsElement);

            #endregion

            #region Build XML

            using (CompEntities entities = new CompEntities())
            {
                #region Prerq

                List<EmploymentTypeDTO> employmentTypes = EmployeeManager.GetEmploymentTypes(entities, Company.ActorCompanyId);
                List<Role> userRoles = RoleManager.GetRolesByUser(entities, reportResult.UserId, Company.ActorCompanyId);
                bool showSocialSec = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec, Permission.Readonly, reportResult.RoleId, Company.ActorCompanyId, Company.LicenseId, entities);
                bool calculatedCostPerHourMySelfPermission = FeatureManager.HasAnyRolePermission(Feature.Time_Employee_Employees_Edit_MySelf_Time_CalculatedCostPerHour, Permission.Readonly, Company.ActorCompanyId, Company.LicenseId, userRoles, entities);
                bool calculatedCostPerHourOtherEmployeesPermission = FeatureManager.HasAnyRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Time_CalculatedCostPerHour, Permission.Readonly, Company.ActorCompanyId, Company.LicenseId, userRoles, entities);
                bool employmentsPayrollPermissionMySelfRead = FeatureManager.HasAnyRolePermission(Feature.Time_Employee_Employees_Edit_MySelf_Employments_Payroll, Permission.Readonly, Company.ActorCompanyId, Company.LicenseId, userRoles, entities);
                bool employmentsPayrollSalaryPermissionOtherEmployeesRead = FeatureManager.HasAnyRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll_Salary, Permission.Readonly, Company.ActorCompanyId, Company.LicenseId, userRoles, entities);
                bool useExperienceMonthsOnEmploymentAsStartValue = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.UseEmploymentExperienceAsStartValue, 0, Company.ActorCompanyId, 0);

                if (employees == null)
                    employees = EmployeeManager.GetAllEmployeesByIds(entities, reportResult.ActorCompanyId, selectionEmployeeIds, loadEmployment: true, loadContact: true, loadEmploymentPriceType: true);

                #endregion

                int employeeXmlId = 1;
                foreach (Employee employee in employees)
                {
                    #region Prereq

                    List<Employment> employments = employee.Employment.GetActiveOrHidden();

                    Employment employment = null;
                    if (employmentId.HasValue && employmentId.Value > 0)
                        employment = employments.FirstOrDefault(i => i.EmploymentId == employmentId.Value);
                    else
                        employment = employee.GetEmployment(selectionDate) ?? employments.GetFirstEmployment(discardState: true).GetNearestEmployment(employments.GetLastEmployment(discardState: true), selectionDate);
                    if (employment == null || employment.EmployeeId != employee.EmployeeId)
                        continue;

                    DateTime currentDate = employment.GetValidEmploymentDate(selectionDate);
                    PayrollGroup payrollGroup = employment.GetPayrollGroup(currentDate, base.personalDataRepository.PayrollGroups);
                    Contact contact = EmployeeManager.GetEmployeeContact(employee.EmployeeId);
                    EmployeeTaxSE employeeTax = EmployeeManager.GetEmployeeTaxSE(entities, employee.EmployeeId, currentDate.Year);
                    EmployeeVacationSE vacationSE = EmployeeManager.GetLatestEmployeeVacationSE(entities, employee.EmployeeId);

                    Employment primaryEmployment = null;
                    if (employment.IsSecondaryEmployment)
                        primaryEmployment = employee.GetEmployment(currentDate);

                    int mySelfEmployeeId = EmployeeManager.GetEmployeeIdForUser(reportResult.UserId, Company.ActorCompanyId);
                    bool employeeIsMySelf = employee.EmployeeId == mySelfEmployeeId;
                    bool employmentsPayrollPermissionRead = (employeeIsMySelf && employmentsPayrollPermissionMySelfRead) || employmentsPayrollSalaryPermissionOtherEmployeesRead;
                    bool calculatedCostPerHourPermission = (employee.EmployeeId == mySelfEmployeeId && calculatedCostPerHourMySelfPermission) || (employee.EmployeeId != mySelfEmployeeId && calculatedCostPerHourOtherEmployeesPermission);

                    #region Load references

                    if (!employee.ContactPersonReference.IsLoaded)
                        employee.ContactPersonReference.Load();
                    if (!employment.EmploymentPriceType.IsLoaded)
                        employment.EmploymentPriceType.Load();

                    foreach (EmploymentPriceType employmentPriceType in employment.EmploymentPriceType)
                    {
                        if (!employmentPriceType.PayrollPriceTypeReference.IsLoaded)
                            employmentPriceType.PayrollPriceTypeReference.Load();
                        if (!employmentPriceType.EmploymentPriceTypePeriod.IsLoaded)
                            employmentPriceType.EmploymentPriceTypePeriod.Load();
                    }

                    if (payrollGroup != null && !payrollGroup.PayrollGroupSetting.IsLoaded)
                        payrollGroup.PayrollGroupSetting.Load();

                    #endregion

                    #endregion

                    #region Employee element

                    XElement employeeElement = new XElement("Employee",
                        new XAttribute("id", employeeXmlId),
                        new XElement("EmployeeNr", employee.EmployeeNr),
                        new XElement("EmployeeFirstName", employee.FirstName),
                        new XElement("EmployeeLastName", employee.LastName),
                        new XElement("EmployeeName", employee.Name),
                        new XElement("EmployeeSocialSec", showSocialSec ? employee.SocialSec : StringUtility.SocialSecYYMMDD_Dash_Stars(employee.SocialSec)),
                        new XElement("EmployeeSex", employee.ContactPerson?.Sex ?? 0),
                        new XElement("EmployeeWorkPercentage", employment.GetPercent(currentDate)),
                        new XElement("EmployeeCalculatedCostPerHour", calculatedCostPerHourPermission ? EmployeeManager.GetEmployeeCalculatedCost(entities, employee, DateTime.Today, null) : 0));
                    base.personalDataRepository.AddEmployee(employee, employeeElement);

                    #endregion

                    #region Employment element

                    int experienceMonths = EmployeeManager.GetExperienceMonths(entities, reportResult.ActorCompanyId, employment, useExperienceMonthsOnEmploymentAsStartValue, currentDate);

                    var fullTimeWorkWeek = employment.GetFullTimeWorkTimeWeek(employment.GetEmployeeGroup(currentDate), currentDate);

                    if (primaryEmployment != null)
                        fullTimeWorkWeek = primaryEmployment.GetFullTimeWorkTimeWeek(employment.GetEmployeeGroup(currentDate), currentDate);

                    var employmentElement = new XElement("Employment",
                        new XElement("EmploymentType", employment.GetEmploymentType(employmentTypes, currentDate)),
                        new XElement("EmploymentTypeName", employment.GetEmploymentTypeName(employmentTypes)),
                        new XElement("EmploymentName", employment.GetName(currentDate)),
                        new XElement("EmploymentDateFrom", employment.DateFrom.ToValueOrDefault()),
                        new XElement("EmploymentDateTo", employment.DateTo.ToValueOrDefault()),
                        new XElement("EmploymentWorkTimeWeek", employment.GetWorkTimeWeek(currentDate)),
                        new XElement("EmploymentPercent", employment.GetPercent(currentDate)),
                        new XElement("ExperienceMonths", experienceMonths < 0 ? 0 : experienceMonths),
                        new XElement("ExperienceAgreedOrEstablished", employment.GetExperienceAgreedOrEstablished(currentDate).ToInt()),
                        new XElement("VacationDaysPayed", vacationSE?.EarnedDaysPaid ?? 0),
                        new XElement("VacationDaysUnpayed", vacationSE?.EarnedDaysUnpaid ?? 0),
                        new XElement("SpecialConditions", employment.GetSpecialConditions(currentDate).NullToEmpty()),
                        new XElement("WorkTasks", employment.GetWorkTasks(currentDate).NullToEmpty()),
                        new XElement("WorkPlace", employment.GetWorkPlace(currentDate).NullToEmpty()),
                        new XElement("TaxRate", employeeTax?.TaxRate.NullToEmpty<int?>()),
                        new XElement("EmploymentBaseWorkTimeWeek", employment.GetBaseWorkTimeWeek(currentDate)),
                        new XElement("SubstituteFor", employment.GetSubstituteFor(currentDate).NullToEmpty()),
                        new XElement("SubstituteForDueTo", employment.GetSubstituteForDueTo(currentDate).NullToEmpty()),
                        new XElement("ExternalCode", employment.GetExternalCode(currentDate).NullToEmpty()),
                        new XElement("IsSecondaryEmployment", employment.IsSecondaryEmployment.ToInt()),
                        new XElement("PrimaryEmploymentWorkTimeWeek", primaryEmployment?.GetWorkTimeWeek(currentDate) ?? employment.GetWorkTimeWeek(currentDate)),
                        new XElement("EmploymentFullTimeWorkWeek", fullTimeWorkWeek));

                    #region EmploymentHistory

                    int employmenHistoryXmlId = 1;
                    foreach (Employment employmentHistory in employments.OrderByDescending(o => o.DateFrom).ToList())
                    {
                        DateTime? date = CalendarUtility.GetDateTimeInInterval(currentDate, employmentHistory.DateFrom, employmentHistory.DateTo);

                        var employmentType = employmentHistory.GetEmploymentType(employmentTypes, date);

                        if (employmentType != (int)TermGroup_EmploymentType.SE_FixedTerm && employmentType != (int)TermGroup_EmploymentType.SE_SpecialFixedTerm)
                            continue;
                        if (!employmentHistory.DateFrom.HasValue)
                            continue;

                        DateTime from = employmentHistory.DateFrom.Value;
                        DateTime to = employmentHistory.DateTo.ToValueOrToday();

                        if (from == CalendarUtility.DATETIME_DEFAULT)  //Incorrect start date...
                            from = to;

                        List<SubstituteShiftDTO> substituteShifts = TimeScheduleManager.GetSubstituteShifts(entities, Company.ActorCompanyId, employee.EmployeeId, from, to);
                        int scheduleTime = substituteShifts.Where(i => /*i.IsNew ||*/ i.IsCopied || i.IsExtraShift).Sum(i => i.Duration);
                        int weekCount = CalendarUtility.GetWeeks(from, to).Count;
                        int averageWorkTimeWeek = scheduleTime > 0 && weekCount > 0 ? scheduleTime / weekCount : 0;

                        employmentElement.Add(new XElement("EmploymentHistory",
                                    new XAttribute("id", employmenHistoryXmlId),
                                    new XElement("EmploymentType", employmentHistory.GetEmploymentType(employmentTypes, date)),
                                    new XElement("EmploymentTypeName", employmentHistory.GetEmploymentTypeName(employmentTypes)),
                                    new XElement("EmploymentName", employmentHistory.GetName(date)),
                                    new XElement("EmploymentDateFrom", employmentHistory.DateFrom.ToValueOrToday()),
                                    new XElement("EmploymentDateTo", employmentHistory.DateTo.ToValueOrToday()),
                                    new XElement("EmploymentWorkTimeWeek", averageWorkTimeWeek),
                                    new XElement("EmploymentPercent", employmentHistory.GetPercent(date)),
                                    new XElement("ExperienceMonths", EmployeeManager.GetExperienceMonths(entities, reportResult.ActorCompanyId, employmentHistory, useExperienceMonthsOnEmploymentAsStartValue, date)),
                                    new XElement("ExperienceAgreedOrEstablished", employmentHistory.GetExperienceAgreedOrEstablished(date).ToInt()),
                                    new XElement("VacationDaysPayed", vacationSE?.EarnedDaysPaid ?? 0),
                                    new XElement("VacationDaysUnpayed", vacationSE?.EarnedDaysUnpaid ?? 0),
                                    new XElement("SpecialConditions", employmentHistory.GetSpecialConditions(date).NullToEmpty()),
                                    new XElement("WorkTasks", employmentHistory.GetWorkTasks(date).NullToEmpty()),
                                    new XElement("WorkPlace", employmentHistory.GetWorkPlace(date).NullToEmpty()),
                                    new XElement("TaxRate", employeeTax?.TaxRate.ToValueOrEmpty() ?? string.Empty),
                                    new XElement("EmploymentBaseWorkTimeWeek", employmentHistory.GetBaseWorkTimeWeek(date)),
                                    new XElement("SubstituteFor", employmentHistory.GetSubstituteFor(date).NullToEmpty()),
                                    new XElement("SubstituteForDueTo", employmentHistory.GetSubstituteForDueTo(date).NullToEmpty()),
                                    new XElement("ExternalCode", employmentHistory.GetExternalCode(date).NullToEmpty())));
                        employmenHistoryXmlId++;
                    }

                    if (employmenHistoryXmlId == 1)
                        AddDefaultElement(reportResult, employmentElement, "EmploymentHistory");

                    #endregion

                    #region PriceType element

                    int employmentPriceTypeXmlId = 1;
                    if (employmentsPayrollPermissionRead)
                    {
                        List<EmploymentPriceTypeDTO> employmentPriceTypes = EmployeeManager.GetEmploymentPriceTypes(entities, employment.EmploymentId, payrollGroup?.PayrollGroupId, currentDate);
                        foreach (EmploymentPriceTypeDTO employmentPriceType in employmentPriceTypes.Where(i => i.Type != null))
                        {
                            decimal? amount = employmentPriceType.GetEmploymentPriceTypeAmount(currentDate);

                            employmentElement.Add(new XElement("PriceType",
                                new XAttribute("id", employmentPriceTypeXmlId),
                                new XElement("PriceTypeType", employmentPriceType.Type.Type),
                                new XElement("PriceTypeName", employmentPriceType.Type.Name),
                                new XElement("PriceTypeDescription", employmentPriceType.Type.Description),
                                new XElement("PriceTypeConditionEmployedMonths", employmentPriceType.Type.ConditionEmployeedMonths),
                                new XElement("PriceTypeConditionExperienceMonths", employmentPriceType.Type.ConditionExperienceMonths),
                                new XElement("PriceTypeConditionAgeYears", employmentPriceType.Type.ConditionAgeYears),
                                new XElement("PriceTypeAmount", amount ?? 0)));
                            employmentPriceTypeXmlId++;
                        }
                    }

                    if (employmentPriceTypeXmlId == 1)
                        AddDefaultElement(reportResult, employmentElement, "PriceType");

                    #endregion

                    employeeElement.Add(employmentElement);

                    #endregion

                    #region PayrollGroup element

                    XElement payrollGroupElement = new XElement("PayrollGroup",
                        new XElement("PayrollGroupName", payrollGroup?.Name ?? string.Empty));

                    #region PayrollGroupSetting element

                    int payrollGroupSettingXmlId = 1;
                    if (payrollGroup != null && payrollGroup.PayrollGroupSetting != null)
                    {
                        foreach (PayrollGroupSetting setting in payrollGroup.PayrollGroupSetting)
                        {
                            payrollGroupElement.Add(new XElement("PayrollGroupSetting",
                                new XAttribute("id", payrollGroupSettingXmlId),
                                new XElement("PayrollGroupSettingType", setting.Type),
                                new XElement("PayrollGroupSettingName", setting.Name),
                                new XElement("PayrollGroupSettingDataType", setting.DataType),
                                new XElement("PayrollGroupSettingData", PayrollManager.GetPayrollGroupSettingValue(setting).NullToEmpty())));
                            payrollGroupSettingXmlId++;
                        }
                    }

                    if (payrollGroupSettingXmlId == 1)
                        AddDefaultElement(reportResult, payrollGroupElement, "PayrollGroupSetting");

                    #endregion

                    employeeElement.Add(payrollGroupElement);

                    #endregion

                    #region EmployeeSkill element

                    int employeeSkillXmlId = 1;
                    List<EmployeeSkill> skills = TimeScheduleManager.GetEmployeeSkills(entities, employee.EmployeeId);
                    foreach (EmployeeSkill skill in skills)
                    {
                        employeeElement.Add(new XElement("EmployeeSkill",
                            new XAttribute("id", employeeSkillXmlId),
                            new XElement("SkillTypeName", skill.Skill?.SkillType?.Name ?? string.Empty),
                            new XElement("SkillName", skill.Skill?.Name ?? string.Empty),
                            new XElement("SkillEndDate", skill.DateTo.ToValueOrDefault()),
                            new XElement("SkillLevel", skill.SkillLevel)));
                        employeeSkillXmlId++;
                    }

                    if (employeeSkillXmlId == 1)
                        AddDefaultElement(reportResult, employeeElement, "EmployeeSkill");

                    #endregion

                    #region EmployeePosition element

                    int employeePositionXmlId = 1;
                    List<EmployeePosition> employeePositions = EmployeeManager.GetEmployeePositions(entities, employee.EmployeeId);
                    foreach (EmployeePosition employeePosition in employeePositions)
                    {
                        employeeElement.Add(new XElement("EmployeePosition",
                            new XAttribute("id", employeePositionXmlId),
                            new XElement("EmployeePositionCode", employeePosition.Position?.Code ?? string.Empty),
                            new XElement("EmployeePositionName", employeePosition.Position?.Name ?? string.Empty),
                            new XElement("EmployeePositionIsDefault", employeePosition.Default.ToInt())));
                        employeePositionXmlId++;
                    }

                    if (employeePositionXmlId == 1)
                        AddDefaultElement(reportResult, employeeElement, "EmployeePosition");

                    #endregion

                    #region EmployeeEcom element

                    int employeeEcomXmlId = 1;
                    if (contact != null && contact.ContactECom != null)
                    {
                        foreach (var contactEComItem in contact.ContactECom)
                        {
                            employeeElement.Add(new XElement("EmployeeEcom",
                                new XAttribute("id", employeeEcomXmlId),
                                new XElement("EComType", contactEComItem.SysContactEComTypeId),
                                new XElement("EComName", contactEComItem.Name.NullToEmpty()),
                                new XElement("EComText", contactEComItem.Text.NullToEmpty()),
                                new XElement("EComDescription", contactEComItem.Description.NullToEmpty())));

                            employeeEcomXmlId++;
                        }
                    }

                    if (employeeEcomXmlId == 1)
                        AddDefaultElement(reportResult, employeeElement, "EmployeeEcom");

                    #endregion

                    #region EmployeeAddress element

                    int employeeAddressXmlId = 1;
                    if (contact != null)
                    {
                        List<ContactAddress> contactAddresses = ContactManager.GetContactAddresses(entities, contact.ContactId);
                        foreach (ContactAddress contactAddress in contactAddresses)
                        {
                            string address = "", co = "", postalCode = "", postalAddress = "", country = "", streetAddress = "", entrenceCode = "";

                            if (contactAddress.ContactAddressRow != null)
                            {
                                foreach (var rowTypeGroup in contactAddress.ContactAddressRow.GroupBy(r => r.SysContactAddressRowTypeId))
                                {
                                    switch (rowTypeGroup.Key)
                                    {
                                        case (int)TermGroup_SysContactAddressRowType.Address:
                                            address = rowTypeGroup.First().Text;
                                            break;
                                        case (int)TermGroup_SysContactAddressRowType.AddressCO:
                                            co = rowTypeGroup.First().Text;
                                            break;
                                        case (int)TermGroup_SysContactAddressRowType.PostalCode:
                                            postalCode = rowTypeGroup.First().Text;
                                            break;
                                        case (int)TermGroup_SysContactAddressRowType.PostalAddress:
                                            postalAddress = rowTypeGroup.First().Text;
                                            break;
                                        case (int)TermGroup_SysContactAddressRowType.Country:
                                            country = rowTypeGroup.First().Text;
                                            break;
                                        case (int)TermGroup_SysContactAddressRowType.StreetAddress:
                                            streetAddress = rowTypeGroup.First().Text;
                                            break;
                                        case (int)TermGroup_SysContactAddressRowType.EntranceCode:
                                            entrenceCode = rowTypeGroup.First().Text;
                                            break;
                                    }
                                }
                            }

                            employeeElement.Add(new XElement("EmployeeAddress",
                                new XAttribute("id", employeeAddressXmlId),
                                new XElement("AddressType", contactAddress.SysContactAddressTypeId),
                                new XElement("AddressName", contactAddress.Name),
                                new XElement("Address", address),
                                new XElement("AddressCO", co),
                                new XElement("AddressPostalCode", postalCode),
                                new XElement("AddressPostalAddress", postalAddress),
                                new XElement("AddressCountry", country),
                                new XElement("AddressStreetAddress", streetAddress),
                                new XElement("AddressEntrenceCode", entrenceCode)));
                            employeeAddressXmlId++;
                        }
                    }

                    if (employeeAddressXmlId == 1)
                        AddDefaultElement(reportResult, employeeElement, "EmployeeAddress");

                    #endregion

                    #region CostAccount element

                    EmploymentAccountStd employmentAccountStdCost = EmployeeManager.GetEmploymentAccountFromEmployeeWithDim(entities, employee.EmployeeId, EmploymentAccountType.Cost, currentDate);

                    XElement costAccount = new XElement("CostAccount",
                        new XElement("CostAccountNr", employmentAccountStdCost?.AccountStd?.Account?.AccountNr ?? string.Empty),
                        new XElement("CostAccountName", employmentAccountStdCost?.AccountStd?.Account?.Name ?? string.Empty));

                    #region CostAccountInternal element

                    int costAccountInternalXmlId = 1;
                    if (employmentAccountStdCost != null)
                    {
                        foreach (AccountInternal accountInternal in employmentAccountStdCost.AccountInternal)
                        {
                            costAccount.Add(new XElement("CostAccountInternal",
                                new XAttribute("id", costAccountInternalXmlId),
                                new XElement("CostAccountInternalNr", accountInternal.Account?.AccountNr ?? string.Empty),
                                new XElement("CostAccountInternalName", accountInternal.Account?.Name ?? string.Empty),
                                new XElement("CostAccountInternalSieDimNr", accountInternal.Account?.AccountDim?.SysSieDimNr.ToValueOrEmpty() ?? string.Empty),
                                new XElement("CostAccountInternalDimNr", accountInternal.Account?.AccountDim?.AccountDimNr.ToString() ?? string.Empty)));
                            costAccountInternalXmlId++;
                        }
                    }

                    if (costAccountInternalXmlId == 1)
                        AddDefaultElement(reportResult, costAccount, "CostAccountInternal");

                    employeeElement.Add(costAccount);

                    #endregion

                    #endregion

                    #region IncomeAccount element

                    EmploymentAccountStd employmentAccountStdIncome = EmployeeManager.GetEmploymentAccountFromEmployeeWithDim(entities, employee.EmployeeId, EmploymentAccountType.Income, currentDate);

                    XElement incomeAccountElement = new XElement("IncomeAccount",
                        new XElement("IncomeAccountNr", employmentAccountStdIncome?.AccountStd?.Account?.AccountNr ?? string.Empty),
                        new XElement("IncomeAccountName", employmentAccountStdIncome?.AccountStd?.Account?.Name ?? string.Empty));

                    #region IncomeAccountInternal element

                    int incomeAccountInternalXmlId = 1;
                    if (employmentAccountStdIncome != null)
                    {
                        foreach (AccountInternal accountInternal in employmentAccountStdIncome.AccountInternal)
                        {
                            incomeAccountElement.Add(new XElement("IncomeAccountInternal",
                                new XAttribute("id", incomeAccountInternalXmlId),
                                new XElement("IncomeAccountInternalNr", accountInternal.Account?.AccountNr),
                                new XElement("IncomeAccountInternalName", accountInternal.Account?.Name),
                                new XElement("IncomeAccountInternalSieDimNr", accountInternal.Account?.AccountDim?.SysSieDimNr.ToValueOrEmpty() ?? string.Empty),
                                new XElement("IncomeAccountInternalDimNr", accountInternal.Account?.AccountDim?.AccountDimNr.ToString() ?? string.Empty)));
                            incomeAccountInternalXmlId++;
                        }
                    }

                    if (incomeAccountInternalXmlId == 1)
                        AddDefaultElement(reportResult, costAccount, "CostAccountInternal");

                    #endregion

                    employeeElement.Add(incomeAccountElement);

                    #endregion

                    #region SubstituteShift Element

                    if (substituteDates.IsNullOrEmpty())
                        substituteDates = new List<DateTime>() { currentDate };

                    int substituteShiftXmlId = 1;
                    if (!substituteDates.IsNullOrEmpty())
                    {
                        List<SubstituteShiftDTO> substituteShifts = TimeScheduleManager.GetSubstituteShifts(entities, Company.ActorCompanyId, employee.EmployeeId, substituteDates, includeSubstituteShiftWithAbsence: true, loadAndFilterOnTimeScheduleType: true);
                        List<Tuple<DateTime, int, string, SubstituteShiftDTO>> substituteShiftTuples = new List<Tuple<DateTime, int, string, SubstituteShiftDTO>>();

                        foreach (SubstituteShiftDTO substituteShift in substituteShifts)
                        {
                            string substituteText = string.Empty;
                            int groupType = 0;

                            if (substituteShift.IsAssignedDueToAbsence)
                            {
                                groupType = 1;
                                if (!string.IsNullOrEmpty(substituteShift.OriginEmployeeName))
                                {
                                    substituteText = string.Format(GetText(8870, "Vikariat för {0}"), substituteShift.OriginEmployeeName);
                                }
                            }
                            else if (substituteShift.IsMoved || substituteShift.IsCopied)
                            {
                                groupType = 2;
                                if (!string.IsNullOrEmpty(substituteShift.OriginEmployeeName))
                                    substituteText = string.Format(GetText(8753, "Passet kommer från {0}"), substituteShift.OriginEmployeeName);
                            }
                            //else if (substituteShift.IsNew)
                            //{
                            //    substituteText = GetText(8754, "Nytt pass");
                            //}
                            else if (substituteShift.IsExtraShift)
                            {
                                groupType = 3;
                                substituteText = GetText(8831, "Extrapass");
                            }
                            else
                            {
                                continue;
                            }

                            substituteShiftTuples.Add(Tuple.Create(substituteShift.Date, groupType, substituteText, substituteShift));
                        }

                        foreach (var groupedByDate in substituteShiftTuples.GroupBy(x => x.Item1))
                        {
                            foreach (var groupedByType in groupedByDate.GroupBy(x => x.Item2))
                            {
                                foreach (var groupedByText in groupedByType.GroupBy(x => x.Item3))
                                {
                                    List<SubstituteShiftDTO> shifts = groupedByText.Select(x => x.Item4).ToList();
                                    DateTime scheduleIn = shifts.OrderBy(x => x.StartTime).FirstOrDefault().StartTime;
                                    DateTime scheduleOut = shifts.OrderByDescending(x => x.StopTime).FirstOrDefault().StopTime;
                                    int duration = shifts.Sum(s => s.Duration);

                                    employeeElement.Add(new XElement("SubstituteShift",
                                        new XAttribute("id", substituteShiftXmlId),
                                        new XElement("IsAbsence", groupedByType.Key == 1 ? 1 : 0),
                                        new XElement("IsCopiedOrMoved", groupedByType.Key == 2 ? 1 : 0),
                                        new XElement("IsExtraShift", groupedByType.Key == 3 ? 1 : 0),
                                        new XElement("Date", groupedByDate.Key),
                                        new XElement("Time", $"{scheduleIn.ToShortTimeString()}-{scheduleOut.ToShortTimeString()}"),
                                        new XElement("NbrOfHours", CalendarUtility.FormatTimeSpan(CalendarUtility.MinutesToTimeSpan(duration), false, false, false)),
                                        new XElement("SubstituteText", groupedByText.Key)));
                                    substituteShiftXmlId++;
                                }
                            }
                        }
                    }

                    if (substituteShiftXmlId == 1)
                        AddDefaultElement(reportResult, employeeElement, "SubstituteShift");

                    #endregion

                    employmentContractElement.Add(employeeElement);
                    employeeXmlId++;
                }

                #region Default element

                if (employeeXmlId == 1)
                    AddDefaultElement(reportResult, employmentContractElement, "Employee");

                #endregion
            }

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            #region Close document

            rootElement.Add(employmentContractElement);
            document.Add(rootElement);

            return GetValidatedDocument(document, SoeReportTemplateType.TimeEmploymentContract);

            #endregion
        }

        public XDocument CreateEmploymentDynamicContractData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            DateTime selectionDate;
            List<DateTime> substituteDates;

            TryGetBoolFromSelection(reportResult, out bool isPrintedFromSchedulePlanning, "isPrintedFromSchedulePlanning");
            if (isPrintedFromSchedulePlanning)
            {
                if (!TryGetDatesFromSelection(reportResult, out substituteDates))
                    return null;

                selectionDate = substituteDates.OrderByDescending(i => i.Date).FirstOrDefault();
            }
            else
            {
                if (!TryGetDateFromSelection(reportResult, out selectionDate))
                    return null;

                substituteDates = new List<DateTime>() { selectionDate };
            }

            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDate, selectionDate, out _, out List<int> selectionEmployeeIds))
                return null;

            int? employmentId = 0;
            if (selectionEmployeeIds.Count == 1)
                TryGetIdFromSelection(reportResult, out employmentId, key: "employmentId");

            TryGetIdFromSelection(reportResult, out int? employeeTemplateId, key: "employeeTemplateId");

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Init document

            XDocument document = reportResult.Input.Document;
            XElement rootElement = reportResult.Input.ElementRoot;
            XElement employmentContractElement = reportResult.Input.ElementFirst;

            #endregion

            #region ReportHeaderLabels

            XElement reportHeaderLabelsElement = CreateTimeReportHeaderLabelsElement();
            reportHeaderLabelsElement.Add(CreateDateIntervalLabelReportHeaderLabelsElement());
            employmentContractElement.Add(reportHeaderLabelsElement);

            #endregion

            #region ReportHeader

            XElement reportHeaderElement = CreateExtendedPersonellReportHeaderElement(reportResult);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            reportHeaderElement.Add(new XElement("CompanyLogo", GetCompanyLogoFilePath(entitiesReadOnly, reportResult.ActorCompanyId, false)));
            reportHeaderElement.Add(new XElement("PrintedFromScheduleplanning", isPrintedFromSchedulePlanning.ToInt()));
            employmentContractElement.Add(reportHeaderElement);

            #endregion

            #region PageHeaderLabels

            XElement pageHeaderLabelsElement = new XElement("PageHeaderLabels");
            employmentContractElement.Add(pageHeaderLabelsElement);

            #endregion

            #region GetElements

            var employeeTemplate = EmployeeManager.GetEmployeeTemplate(base.ActorCompanyId, employeeTemplateId ?? 0).ToDTO() ?? throw new SoeGeneralException("template not found", this.ToString());
            var reportData = new EmploymentDynamicContractReportData(parameterObject, new EmploymentDynamicContractReportInput(reportResult, employeeTemplate, substituteDates, true));
            var output = reportData.CreateOutput() ?? throw new SoeGeneralException("output not created", this.ToString());

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            #region Close document
            employmentContractElement.Add(output.Elements);
            rootElement.Add(employmentContractElement);
            document.Add(rootElement);

            return GetValidatedDocument(document, SoeReportTemplateType.TimeEmploymentDynamicContract);

            #endregion
        }

        public XDocument CreateEmployeeListReportData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            if (!TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return null;

            TryGetIncludeInactiveFromSelection(reportResult, out _, out _, out bool? selectionActiveEmployees);

            bool useExperienceMonthsOnEmploymentAsStartValue = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseEmploymentExperienceAsStartValue, 0, Company.ActorCompanyId, 0);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Init document

            XDocument document = reportResult.Input.Document;
            XElement rootElement = reportResult.Input.ElementRoot;
            XElement employeeReportElement = reportResult.Input.ElementFirst;

            #endregion

            #region ReportHeaderLabels

            XElement reportHeaderLabelsElement = CreateTimeReportHeaderLabelsElement();
            reportHeaderLabelsElement.Add(CreateDateIntervalLabelReportHeaderLabelsElement());
            employeeReportElement.Add(reportHeaderLabelsElement);

            #endregion

            #region ReportHeader

            XElement reportHeaderElement = CreateTimeReportHeaderElement(reportResult);
            reportHeaderElement.Add(CreateDateIntervalElement(selectionDateFrom, selectionDateTo));
            employeeReportElement.Add(reportHeaderElement);

            #endregion

            #region PageHeaderLabels

            XElement pageHeaderLabelsElement = new XElement("PageHeaderLabels");
            CreateEmployeeReportPageHeaderLabelsElement(pageHeaderLabelsElement);
            employeeReportElement.Add(pageHeaderLabelsElement);

            #endregion

            #region AccountDimensions

            List<AccountDim> accountDims = AccountManager.GetAccountDimsByCompany(Company.ActorCompanyId, false, false, true);
            foreach (AccountDim accountDim in accountDims)
            {
                XElement accountDimElement = new XElement("AccountDims",
                    new XElement("AccountDimNr", accountDim.AccountDimNr),
                    new XElement("AccountSieDimNr", accountDim.SysSieDimNr),
                    new XElement("AccountDimName", accountDim.Name));

                employeeReportElement.Add(accountDimElement);
            }

            #endregion

            #region Build XML

            using (CompEntities entities = new CompEntities())
            {
                int employeeXmlId = 1;
                if (selectionEmployeeIds.Any())
                {
                    #region Prereq

                    int langId = GetLangId();
                    Dictionary<int, string> disbursementMethodsDict = EmployeeManager.GetEmployeeDisbursementMethods(langId, true);
                    Dictionary<int, string> endReasonsDict = EmployeeManager.GetSystemEndReasons(entities, Company.ActorCompanyId, includeCompanyEndReasons: true);
                    List<GenericType> employeeTaxEmploymentTaxTypeTerms = base.GetTermGroupContent(TermGroup.EmployeeTaxEmploymentTaxType, langId: langId);
                    List<EmploymentTypeDTO> employmentTypes = EmployeeManager.GetEmploymentTypes(entities, Company.ActorCompanyId);
                    List<Role> currentRoles = RoleManager.GetRolesByUser(reportResult.UserId, reportResult.ActorCompanyId);

                    int mySelfEmployeeId = EmployeeManager.GetEmployeeIdForUser(reportResult.ActorCompanyId, reportResult.UserId);
                    bool showSocialSec = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId);
                    bool calculatedCostPerHourMySelfPermission = currentRoles.Any(role => FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_MySelf_Time_CalculatedCostPerHour, Permission.Readonly, role.RoleId, reportResult.ActorCompanyId));
                    bool calculatedCostPerHourOtherEmployeesPermission = currentRoles.Any(role => FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Time_CalculatedCostPerHour, Permission.Readonly, role.RoleId, reportResult.ActorCompanyId));

                    if (employees == null)
                        employees = EmployeeManager.GetAllEmployees(entities, reportResult.ActorCompanyId, active: selectionActiveEmployees, loadEmployment: true);

                    #endregion

                    #region Content

                    DateTime selectionDate = selectionDateFrom.Date == selectionDateTo.Date || selectionDateTo == DateTime.MaxValue.Date ? selectionDateFrom.Date : selectionDateTo.Date;

                    foreach (int employeeId in selectionEmployeeIds)
                    {
                        #region Prereq

                        Employee employee = employees.FirstOrDefault(i => i.EmployeeId == employeeId);
                        if (employee == null)
                            continue;

                        if (!employee.UserReference.IsLoaded)
                            employee.UserReference.Load();

                        List<Employment> employments = employee.GetActiveEmploymentsDesc();
                        if (employments.IsNullOrEmpty())
                            continue;

                        Employment firstEmployment = employments.GetFirstEmployment();
                        Employment lastEmployment = employments.GetLastEmployment();
                        Employment currentEmployment = employments.GetEmployment(selectionDate) ?? firstEmployment.GetNearestEmployment(lastEmployment, selectionDate);
                        if (currentEmployment == null)
                            continue;

                        DateTime currentDate = currentEmployment.GetValidEmploymentDate(selectionDate);
                        EmployeeGroup employeeGroup = employee.GetEmployeeGroup(currentDate, base.personalDataRepository.EmployeeGroups);
                        PayrollGroup payrollGroup = employee.GetPayrollGroup(currentDate, base.personalDataRepository.PayrollGroups);
                        VacationGroup vacationGroup = PayrollManager.GetVacationGroupForEmployee(entities, employee, currentDate);

                        Contact contact = ContactManager.GetContactAndEcomFromActor(entities, employee.ContactPerson.ActorContactPersonId);
                        List<ContactAddress> contactAddresses = contact != null ? ContactManager.GetContactAddresses(entities, contact.ContactId) : new List<ContactAddress>();
                        List<EmployeePosition> employeePositions = EmployeeManager.GetEmployeePositions(entities, employee.EmployeeId, loadSysPosition: true);
                        EmployeePosition defaultEmployeePosition = employeePositions.FirstOrDefault(f => f.Default);
                        EmployeeTaxSE employeeTax = EmployeeManager.GetEmployeeTaxSE(entities, employee.EmployeeId, currentDate.Year);
                        EmployeeVacationSE vacationSE = EmployeeManager.GetLatestEmployeeVacationSE(entities, employee.EmployeeId);
                        List<EmploymentPriceTypeDTO> employmentPriceTypes = currentEmployment != null && payrollGroup != null ? EmployeeManager.GetEmploymentPriceTypes(entities, currentEmployment.EmploymentId, payrollGroup.PayrollGroupId, currentDate) : new List<EmploymentPriceTypeDTO>();
                        List<PayrollGroupPriceFormula> payrollGroupPriceFormulas = payrollGroup != null ? PayrollManager.GetPayrollGroupPriceFormulas(entities, payrollGroup.PayrollGroupId, true) : new List<PayrollGroupPriceFormula>();
                        List<EmployeeSkillDTO> skills = TimeScheduleManager.GetEmployeeSkills(entities, employee.EmployeeId).ToDTOs(true);
                        Company defaultCompany = employee.User != null && employee.User.DefaultActorCompanyId.HasValue ? CompanyManager.GetCompany(entities, employee.User.DefaultActorCompanyId.Value) : null;
                        List<Role> roles = employee.User != null ? RoleManager.GetRolesByUser(entities, employee.User.UserId, Company.ActorCompanyId) : new List<Role>();
                        Role defaultRole = employee.UserId.HasValue ? UserManager.GetDefaultRole(entities, reportResult.ActorCompanyId, employee.UserId.Value, roles: roles) : null;
                        SysLanguageDTO sysLanguage = employee.User != null && employee.User.LangId.HasValue ? LanguageManager.GetSysLanguage(employee.User.LangId.Value) : null;
                        List<AttestRole> attestRoles = employee.User != null ? AttestManager.GetAttestRolesForUser(entities, employee.User.UserId, reportResult.ActorCompanyId, module: SoeModule.Time) : new List<AttestRole>();
                        List<CompanyCategoryRecord> companyCategoryRecords = CategoryManager.GetCompanyCategoryRecords(entities, SoeCategoryType.Employee, SoeCategoryRecordEntity.Employee, employee.EmployeeId, reportResult.ActorCompanyId, false, selectionDateFrom, selectionDateTo);
                        bool calculatedCostPerHourPermission = (employee.EmployeeId == mySelfEmployeeId && calculatedCostPerHourMySelfPermission) || (employee.EmployeeId != mySelfEmployeeId && calculatedCostPerHourOtherEmployeesPermission);
                        decimal sysPayrollPriceSchoolYouthLimit = PayrollManager.GetSysPayrollPriceAmount(reportResult.ActorCompanyId, (int)TermGroup_SysPayrollPrice.SE_SchoolYouthLimit, CalendarUtility.GetEndOfYear(currentDate));
                        int lasDays = EmployeeManager.GetLasDays(entities, reportResult.ActorCompanyId, employee, selectionDate);


                        #endregion

                        #region Employee element

                        XElement employeeElement = new XElement("Employee",
                            new XAttribute("Id", employeeXmlId),
                            new XElement("EmployeeNr", employee.EmployeeNr),
                            new XElement("EmployeeName", employee.Name),
                            new XElement("EmployeeSocialSec", showSocialSec ? employee.SocialSec : StringUtility.SocialSecYYMMDD_Dash_Stars(employee.SocialSec)),
                            new XElement("EmployeeSex", employee?.ContactPerson?.Sex ?? 0),
                            new XElement("UserName", employee?.User?.LoginName ?? string.Empty),
                            new XElement("Language", sysLanguage?.Name ?? string.Empty),
                            new XElement("DefaultCompany", defaultCompany?.Name ?? string.Empty),
                            new XElement("DefaultRole", defaultRole?.Name ?? string.Empty),
                            new XElement("IsMobileUser", employee.User?.IsMobileUser.ToInt() ?? 0),
                            new XElement("ChangePasswordAtNextLogin", employee.User?.ChangePassword.ToInt() ?? 0),
                            new XElement("IsSysUser", employee.User?.SysUser.ToInt() ?? 0),
                            new XElement("EmployeeCalculatedCostPerHour", calculatedCostPerHourPermission ? EmployeeManager.GetEmployeeCalculatedCost(entities, employee, DateTime.Today, null) : 0),
                            new XElement("Note", employee.Note),
                            new XElement("EmploymentDate", firstEmployment?.GetEmploymentDate().ToValueOrDefault()),
                            new XElement("EndDate", lastEmployment?.GetEndDate().ToValueOrDefault()),
                            new XElement("LASDays", lasDays),
                            new XElement("DisbursementMethodId", employee.DisbursementMethod),
                            new XElement("DisbursementMethodText", disbursementMethodsDict.ContainsKey(employee.DisbursementMethod) ? disbursementMethodsDict[employee.DisbursementMethod] : string.Empty),
                            new XElement("DisbursementClearingNr", !string.IsNullOrEmpty(employee.DisbursementClearingNr) ? employee.DisbursementClearingNr : string.Empty),
                            new XElement("DisbursementAccountNr", !string.IsNullOrEmpty(employee.DisbursementAccountNr) ? employee.DisbursementAccountNr : string.Empty),
                            new XElement("SSYKCode", defaultEmployeePosition?.Position?.SysPositionCode ?? string.Empty),
                            new XElement("SSYKName", defaultEmployeePosition?.Position?.SysPositionName ?? string.Empty),
                            new XElement("PayrollStatisticsPersonalCategory", employee.PayrollStatisticsPersonalCategory.ToInt()),
                            new XElement("PayrollStatisticsWorkTimeCategory", employee.PayrollStatisticsWorkTimeCategory.ToInt()),
                            new XElement("PayrollStatisticsSalaryType", employee.PayrollStatisticsSalaryType.ToInt()),
                            new XElement("PayrollStatisticsWorkPlaceNumber", employee.PayrollStatisticsWorkPlaceNumber.ToInt()),
                            new XElement("PayrollStatisticsCFARNumber", employee.PayrollStatisticsCFARNumber.ToInt()),
                            new XElement("WorkPlaceSCB", employee.WorkPlaceSCB),
                            new XElement("PartnerInCloseCompany", employee.PartnerInCloseCompany.ToInt()),
                            new XElement("BenefitAsPension", employee.BenefitAsPension.ToInt()),
                            new XElement("AFACategory", employee.AFACategory),
                            new XElement("AFASpecialAgreement", employee.AFASpecialAgreement),
                            new XElement("AFAWorkplaceNr", employee.AFAWorkplaceNr),
                            new XElement("AFAParttimePensionCode", employee.AFAParttimePensionCode.ToInt()),
                            new XElement("CollectumITPPlan", employee.CollectumITPPlan),
                            new XElement("CollectumAgreedOnProduct", employee.CollectumAgreedOnProduct),
                            new XElement("CollectumCostPlace", employee.CollectumCostPlace),
                            new XElement("CollectumCancellationDate", employee.CollectumCancellationDate.ToValueOrDefault()),
                            new XElement("CollectumCancellationDateIsLeaveOfAbsence", employee.CollectumCancellationDateIsLeaveOfAbsence.ToInt()),
                            new XElement("Created", employee.Created ?? CalendarUtility.DATETIME_DEFAULT),
                            new XElement("Modified", employee.Modified ?? CalendarUtility.DATETIME_DEFAULT));
                        base.personalDataRepository.AddEmployee(employee, employeeElement);

                        #endregion

                        #region Employment element                    

                        XElement employmentElement = new XElement("Employment",
                           new XAttribute("Id", 1),
                           new XElement("EmploymentType", currentEmployment?.GetEmploymentType(employmentTypes, currentDate) ?? 0),
                           new XElement("EmploymentTypeName", currentEmployment?.GetEmploymentTypeName(employmentTypes, currentDate) ?? string.Empty),
                           new XElement("EmploymentDateFrom", currentEmployment?.DateFrom.ToValueOrDefault()),
                           new XElement("EmploymentDateTo", currentEmployment?.DateTo.ToValueOrDefault()),
                           new XElement("EmploymentDays", currentEmployment?.GetEmploymentDays(stopDate: currentDate) ?? 0),
                           new XElement("BaseWorkTimeWeek", currentEmployment?.GetBaseWorkTimeWeek(currentDate) ?? 0),
                           new XElement("WorkTimeWeek", currentEmployment?.GetWorkTimeWeek(currentDate) ?? 0),
                           new XElement("WorkPercentage", currentEmployment?.GetPercent(currentDate) ?? 0),
                           new XElement("ExperienceMonths", EmployeeManager.GetExperienceMonths(entities, reportResult.ActorCompanyId, currentEmployment, useExperienceMonthsOnEmploymentAsStartValue, currentDate)),
                           new XElement("ExperienceAgreedOrEstablished", currentEmployment?.GetExperienceAgreedOrEstablished(currentDate).ToInt() ?? 0),
                           new XElement("VacationDaysPayed", vacationSE?.EarnedDaysPaid ?? 0),
                           new XElement("VacationDaysUnpayed", vacationSE?.EarnedDaysUnpaid ?? 0),
                           new XElement("SpecialConditions", currentEmployment?.GetSpecialConditions(currentDate) ?? string.Empty),
                           new XElement("WorkPlace", currentEmployment?.GetWorkPlace(currentDate) ?? string.Empty),
                           new XElement("SubstituteFor", currentEmployment?.GetSubstituteFor(currentDate) ?? string.Empty));
                        AddEmploymentEndReasonInfo(employmentElement, currentEmployment, currentDate, endReasonsDict);

                        #region EmployeeGroup element

                        XElement employeeGroupElement = new XElement("EmployeeGroup",
                           new XAttribute("Id", 1),
                           new XElement("EmployeeGroupName", employeeGroup?.Name ?? string.Empty),
                           new XElement("EmployeeGroupIsAutogen", employeeGroup?.AutogenTimeblocks.ToInt() ?? 0),
                           new XElement("EmployeeGroupRuleWorkTimeWeek", employeeGroup?.RuleWorkTimeWeek ?? 0),
                           new XElement("EmployeeGroupRuleWorkTimeYear", 0),
                           new XElement("EmployeeGroupRuleRestTimeWeek", employeeGroup?.RuleRestTimeWeek ?? 0),
                           new XElement("EmployeeGroupRuleRestTimeDay", employeeGroup?.RuleRestTimeDay ?? 0),
                           new XElement("EmployeeGroupRuleWorkTimeYear2014", 0),
                           new XElement("EmployeeGroupRuleWorkTimeYear2015", 0),
                           new XElement("EmployeeGroupRuleWorkTimeYear2016", 0),
                           new XElement("EmployeeGroupRuleWorkTimeYear2017", 0),
                           new XElement("EmployeeGroupRuleWorkTimeYear2018", 0),
                           new XElement("EmployeeGroupRuleWorkTimeYear2019", 0),
                           new XElement("EmployeeGroupRuleWorkTimeYear2020", 0),
                           new XElement("EmployeeGroupRuleWorkTimeYear2021", 0),
                           new XElement("EmployeeGroupMaxScheduleTimeFullTime", employeeGroup?.MaxScheduleTimeFullTime ?? 0),
                           new XElement("EmployeeGroupMaxScheduleTimePartTime", employeeGroup?.MaxScheduleTimePartTime ?? 0),
                           new XElement("EmployeeGroupMinScheduleTimeFullTime", employeeGroup?.MinScheduleTimeFullTime ?? 0),
                           new XElement("EmployeeGroupMinScheduleTimePartTime", employeeGroup?.MinScheduleTimePartTime ?? 0),
                           new XElement("EmployeeGroupMaxScheduleTimeWithoutBreaks", employeeGroup?.MaxScheduleTimeWithoutBreaks ?? 0));
                        employmentElement.Add(employeeGroupElement);

                        #endregion

                        #region PayrollGroup element

                        XElement payrollGroupElement = new XElement("PayrollGroup",
                           new XAttribute("Id", 1),
                           new XElement("PayrollGroupName", payrollGroup?.Name ?? string.Empty));
                        employmentElement.Add(payrollGroupElement);

                        #endregion

                        #region VacationGroup element

                        XElement VacationGroupElement = new XElement("VacationGroup",
                           new XAttribute("Id", 1),
                           new XElement("VacationGroupName", vacationGroup?.Name ?? string.Empty));
                        employmentElement.Add(VacationGroupElement);

                        #endregion

                        #region EmploymentPriceType element

                        int employmentPriceTypeXmlId = 1;
                        foreach (EmploymentPriceTypeDTO employmentPriceType in employmentPriceTypes)
                        {
                            var amountTuple = employmentPriceType.GetAmountAndDateAndIsIsPayrollGroupPriceType(currentDate);

                            XElement employmentPriceTypeElement = new XElement("EmploymentPriceType",
                                new XAttribute("Id", employmentPriceTypeXmlId),
                                new XElement("PriceTypeCode", employmentPriceType.Name),
                                new XElement("PriceTypeName", employmentPriceType.Name),
                                new XElement("PriceTypeType", (int)employmentPriceType.PayrollPriceType),
                                new XElement("PriceTypeTypeName", GetText((int)employmentPriceType.PayrollPriceType, (int)TermGroup.EmploymentPriceTypes)),
                                new XElement("PriceTypeCurrentAmount", amountTuple.Item1),
                                new XElement("PriceTypeDateFrom", amountTuple.Item2.ToValueOrDefault()),
                                new XElement("IsPayrollGroupPriceType", amountTuple.Item3.ToInt()));
                            employmentElement.Add(employmentPriceTypeElement);
                            employmentPriceTypeXmlId++;
                        }

                        if (employmentPriceTypeXmlId == 1)
                            AddDefaultElement(reportResult, employmentElement, "EmploymentPriceType");

                        #endregion

                        #region EmploymentPriceFormula element

                        int employmentPriceFormulaXmlId = 1;
                        foreach (PayrollGroupPriceFormula payrollGroupPriceFormula in payrollGroupPriceFormulas)
                        {
                            var priceFormulaResult = PayrollManager.EvaluatePayrollPriceFormula(entities, reportResult.ActorCompanyId, employee, currentEmployment, null, currentDate, payrollGroupPriceFormula.PayrollGroupPriceFormulaId, null, null);
                            if (priceFormulaResult != null)
                            {
                                XElement employmentPriceFormulaElement = new XElement("EmploymentPriceFormula",
                                    new XAttribute("Id", employmentPriceFormulaXmlId),
                                    new XElement("FormulaNames", priceFormulaResult.FormulaNames),
                                    new XElement("FormulaPlain", priceFormulaResult.FormulaPlain),
                                    new XElement("FormulaExtracted", priceFormulaResult.FormulaExtracted));

                                employmentElement.Add(employmentPriceFormulaElement);
                                employmentPriceFormulaXmlId++;
                            }
                        }

                        if (employmentPriceFormulaXmlId == 1)
                            AddDefaultElement(reportResult, employmentElement, "EmploymentPriceFormula");

                        #endregion

                        #region EmploymentHistory

                        int employmenHistoryXmlId = 1;
                        foreach (Employment employmentHistory in employments)
                        {
                            DateTime? date = CalendarUtility.GetDateTimeInInterval(currentDate, employmentHistory.DateFrom, employmentHistory.DateTo);

                            XElement employmentHistoryElement = new XElement("EmploymentHistory",
                                new XAttribute("id", employmenHistoryXmlId),
                                new XElement("EmploymentType", employmentHistory.GetEmploymentType(employmentTypes, date)),
                                new XElement("EmploymentTypeName", employmentHistory.GetEmploymentTypeName(employmentTypes, date)),
                                new XElement("EmploymentName", employmentHistory.GetName(date)),
                                new XElement("EmploymentDateFrom", employmentHistory.DateFrom.ToValueOrDefault()),
                                new XElement("EmploymentDateTo", employmentHistory.DateTo.ToValueOrDefault()),
                                new XElement("EmploymentDays", employmentHistory.GetEmploymentDays(stopDate: currentDate)),
                                new XElement("EmploymentPercent", employmentHistory.GetPercent(date)),
                                new XElement("ExperienceMonths", EmployeeManager.GetExperienceMonths(entities, reportResult.ActorCompanyId, employmentHistory, useExperienceMonthsOnEmploymentAsStartValue, currentDate)),
                                new XElement("ExperienceAgreedOrEstablished", employmentHistory.GetExperienceAgreedOrEstablished(date).ToInt()),
                                new XElement("VacationDaysPayed", vacationSE?.EarnedDaysPaid ?? 0),
                                new XElement("VacationDaysUnpayed", vacationSE?.EarnedDaysUnpaid ?? 0),
                                new XElement("SpecialConditions", employmentHistory.GetSpecialConditions(date).NullToEmpty()),
                                new XElement("WorkTasks", employmentHistory.GetWorkTasks(date).NullToEmpty()),
                                new XElement("WorkPlace", employmentHistory.GetWorkPlace(date).NullToEmpty()),
                                new XElement("TaxRate", employeeTax?.TaxRate.ToValueOrEmpty() ?? string.Empty),
                                new XElement("EmploymentBaseWorkTimeWeek", employmentHistory.GetBaseWorkTimeWeek(date)),
                                new XElement("SubstituteFor", employmentHistory.GetSubstituteFor(date).NullToEmpty()),
                                new XElement("SubstituteForDueTo", employmentHistory.GetSubstituteForDueTo(date).NullToEmpty()),
                                new XElement("ExternalCode", employmentHistory.GetExternalCode(date).NullToEmpty()));
                            AddEmploymentEndReasonInfo(employmentHistoryElement, employmentHistory, currentDate, endReasonsDict);
                            employmentElement.Add(employmentHistoryElement);
                            employmenHistoryXmlId++;
                        }

                        if (employmenHistoryXmlId == 1)
                            AddDefaultElement(reportResult, employmentElement, "EmploymentHistory");

                        #endregion

                        employeeElement.Add(employmentElement);

                        #endregion

                        #region Tax

                        // Use 31/12 for selected year when getting the system price
                        decimal schoolYouthLimitInitial = employeeTax?.SchoolYouthLimitInitial ?? 0;
                        decimal schoolYouthLimitUsed = employeeTax != null ? PayrollManager.GetSchoolYouthLimitUsed(reportResult.ActorCompanyId, employee.EmployeeId, currentDate) : 0;
                        decimal schoolYouthLimitRemaining = employeeTax != null ? PayrollRulesUtil.CalculateSchoolYouthLimitRemaining(sysPayrollPriceSchoolYouthLimit, schoolYouthLimitInitial, schoolYouthLimitUsed) : 0;
                        GenericType employmentTaxTypeTerm = employeeTax != null ? employeeTaxEmploymentTaxTypeTerms.FirstOrDefault(x => x.Id == employeeTax.EmploymentTaxType) : null;

                        XElement employeeTaxElement = new XElement("Tax",
                            new XAttribute("Id", employeeXmlId),
                            new XElement("Year", employeeTax?.Year ?? 0),
                            new XElement("MainEmployer", employeeTax?.MainEmployer.ToInt() ?? 0),
                            new XElement("Type", employeeTax?.Type ?? 0),
                            new XElement("TypeName", employeeTax != null ? GetText(employeeTax.Type, (int)TermGroup.EmployeeTaxType) : string.Empty),
                            new XElement("TaxRate", employeeTax?.TaxRate.ToInt() ?? 0),
                            new XElement("TaxRateColumn", employeeTax?.TaxRateColumn.ToInt() ?? 0),
                            new XElement("OneTimeTaxPercent", employeeTax?.OneTimeTaxPercent ?? 0),
                            new XElement("EstimatedAnnualSalary", employeeTax?.EstimatedAnnualSalary ?? 0),
                            new XElement("AdjustmentType", employeeTax?.AdjustmentType ?? 0),
                            new XElement("AdjustmentValue", employeeTax?.AdjustmentValue ?? 0),
                            new XElement("AdjustmentPeriodFrom", employeeTax?.AdjustmentPeriodFrom.ToValueOrDefault() ?? CalendarUtility.DATETIME_DEFAULT),
                            new XElement("AdjustmentPeriodTo", employeeTax?.AdjustmentPeriodTo.ToValueOrDefault() ?? CalendarUtility.DATETIME_DEFAULT),
                            new XElement("SchoolYouthLimitInitial", schoolYouthLimitInitial),
                            new XElement("SchoolYouthLimitUsed", schoolYouthLimitUsed),
                            new XElement("SchoolYouthLimitRemaining", schoolYouthLimitRemaining),
                            new XElement("SinkType", employeeTax?.SinkType ?? 0),
                            new XElement("EmploymentTaxType", employeeTax?.EmploymentTaxType ?? 0),
                            new XElement("EmploymentTaxTypeText", employmentTaxTypeTerm?.Name ?? string.Empty),
                            new XElement("EmploymentAbroadCode", employeeTax?.EmploymentAbroadCode ?? 0),
                            new XElement("RegionalSupport", employeeTax?.RegionalSupport.ToInt() ?? 0),
                            new XElement("SalaryDistressAmount", employeeTax?.SalaryDistressAmount ?? 0),
                            new XElement("SalaryDistressAmountType", employeeTax?.SalaryDistressAmountType ?? 0),
                            new XElement("SalaryDistressReservedAmount", employeeTax?.SalaryDistressReservedAmount ?? 0),
                            new XElement("FirstEmployee", employeeTax?.FirstEmployee.ToInt() ?? 0));
                        base.personalDataRepository.AddEmployeeTax(employeeTax, employeeTaxElement);
                        employeeElement.Add(employeeTaxElement);

                        #endregion

                        #region Categories element

                        int categoryXmlId = 1;
                        foreach (CompanyCategoryRecord record in companyCategoryRecords)
                        {
                            XElement categoryElement = new XElement("Categories",
                                new XAttribute("Id", categoryXmlId),
                                new XElement("CategoryCode", record.Category?.Code ?? string.Empty),
                                new XElement("CategoryName", record.Category?.Name ?? string.Empty),
                                new XElement("CategoryIsDefault", record.Default.ToInt()),
                                new XElement("CategoryDateFrom", record.DateFrom.ToValueOrDefault()),
                                new XElement("CategoryDateTo", record.DateTo.ToValueOrDefault()));

                            employeeElement.Add(categoryElement);
                            categoryXmlId++;
                        }

                        if (categoryXmlId == 1)
                            AddDefaultElement(reportResult, employeeElement, "Categories");

                        #endregion

                        #region Roles element

                        int roleXmlId = 1;
                        foreach (Role role in roles)
                        {
                            XElement roleElement = new XElement("Roles",
                                new XAttribute("Id", roleXmlId),
                                new XElement("UserRoleName", role.Name.NullToEmpty()));
                            employeeElement.Add(roleElement);
                            roleXmlId++;
                        }

                        AddDefaultElement(reportResult, employeeElement, "Roles");

                        #endregion

                        #region Attestroles element

                        int attestRoleXmlId = 1;
                        foreach (AttestRole attestRole in attestRoles)
                        {
                            XElement attestRoleElement = new XElement("AttestRoles",
                                new XAttribute("Id", attestRoleXmlId),
                                new XElement("AttestRoleName", attestRole.Name.NullToEmpty()));
                            employeeElement.Add(attestRoleElement);
                            attestRoleXmlId++;
                        }

                        if (attestRoleXmlId == 1)
                            AddDefaultElement(reportResult, employeeElement, "AttestRoles");

                        #endregion

                        #region Skills element

                        int skillsXmlId = 1;
                        foreach (EmployeeSkillDTO skill in skills)
                        {
                            XElement skillsElement = new XElement("Skills",
                                new XAttribute("Id", skillsXmlId),
                                new XElement("SkillTypeName", skill.SkillTypeName),
                                new XElement("SkillName", skill.SkillName),
                                new XElement("SkillEndDate", skill.DateTo.ToValueOrDefault()),
                                new XElement("SkillLevel", skill.SkillLevel));
                            employeeElement.Add(skillsElement);
                            skillsXmlId++;
                        }

                        if (skillsXmlId == 1)
                            AddDefaultElement(reportResult, employeeElement, "Skills");

                        #endregion

                        #region EmployeePositions element

                        int employeePositionsXmlId = 1;
                        foreach (EmployeePosition employeePosition in employeePositions)
                        {
                            XElement employeePositionElement = new XElement("EmployeePositions",
                                new XAttribute("Id", employeePositionsXmlId),
                                new XElement("Code", employeePosition?.Position?.Code ?? string.Empty),
                                new XElement("Position", employeePosition?.Position?.Name ?? string.Empty),
                                new XElement("SSYKCode", employeePosition?.Position?.SysPositionCode ?? string.Empty),
                                new XElement("SSYKName", employeePosition?.Position?.SysPositionName ?? string.Empty));
                            employeeElement.Add(employeePositionElement);
                            employeePositionsXmlId++;
                        }

                        if (employeePositionsXmlId == 1)
                            AddDefaultElement(reportResult, employeeElement, "EmployeePositions");

                        #endregion

                        #region EmployeeEcom element

                        int contactEComXmlId = 1;
                        if (contact != null && contact.ContactECom != null)
                        {
                            foreach (ContactECom ecom in contact.ContactECom)
                            {
                                if (!ecom.IsSecret)
                                {
                                    XElement ecomElement = new XElement("EmployeeEcom",
                                        new XAttribute("Id", contactEComXmlId),
                                        new XElement("EComType", ecom.SysContactEComTypeId),
                                        new XElement("EComName", ecom.Name.NullToEmpty()),
                                        new XElement("EComText", ecom.Text.NullToEmpty()),
                                        new XElement("EComDescription", ecom.Description.NullToEmpty()));

                                    employeeElement.Add(ecomElement);
                                }
                                else
                                {
                                    XElement ecomElement = new XElement("EmployeeEcom",
                                        new XAttribute("Id", contactEComXmlId),
                                        new XElement("EComType", ecom.SysContactEComTypeId),
                                        new XElement("EComName", ecom.Name.NullToEmpty()),
                                        new XElement("EComText", "**"),
                                        new XElement("EComDescription", "**"));

                                    employeeElement.Add(ecomElement);
                                }

                                contactEComXmlId++;
                            }
                        }

                        if (contactEComXmlId == 1)
                            AddDefaultElement(reportResult, employeeElement, "EmployeeEcom");

                        #endregion

                        #region EmployeeAddresses element

                        int employeeAddressXmlId = 1;
                        foreach (var contactAddress in contactAddresses)
                        {
                            #region Prereq

                            string address = "";
                            string addressCo = "";
                            string addressPostalCode = "";
                            string addressPostalAddress = "";
                            string addressCountry = "";
                            string addressStreetAddress = "";
                            string addressEntrenceCode = "";

                            if (contactAddress.ContactAddressRow != null && !contactAddress.IsSecret)
                            {
                                foreach (var contactAddressRow in contactAddress.ContactAddressRow)
                                {
                                    if (contactAddressRow.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.Address)
                                        address = contactAddressRow.Text;
                                    if (contactAddressRow.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.AddressCO)
                                        addressCo = contactAddressRow.Text;
                                    if (contactAddressRow.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalCode)
                                        addressPostalCode = contactAddressRow.Text;
                                    if (contactAddressRow.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalAddress)
                                        addressPostalAddress = contactAddressRow.Text;
                                    if (contactAddressRow.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.Country)
                                        addressCountry = contactAddressRow.Text;
                                    if (contactAddressRow.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.StreetAddress)
                                        addressStreetAddress = contactAddressRow.Text;
                                    if (contactAddressRow.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.EntranceCode)
                                        addressEntrenceCode = contactAddressRow.Text;
                                }
                            }

                            #endregion

                            XElement addressElement = new XElement("EmployeeAddresses",
                                new XAttribute("Id", employeeAddressXmlId),
                                new XElement("AddressType", contactAddress.SysContactAddressTypeId),
                                new XElement("AddressName", contactAddress.Name),
                                new XElement("Address", address),
                                new XElement("AddressCO", addressCo),
                                new XElement("AddressPostalCode", addressPostalCode),
                                new XElement("AddressPostalAddress", addressPostalAddress),
                                new XElement("AddressCountry", addressCountry),
                                new XElement("AddressStreetAddress", addressStreetAddress),
                                new XElement("AddressEntrenceCode", addressEntrenceCode));

                            employeeElement.Add(addressElement);
                            employeeAddressXmlId++;
                        }

                        if (employeeAddressXmlId == 1)
                            AddDefaultElement(reportResult, employeeElement, "EmployeeAddresses");

                        #endregion

                        #region Accounts

                        AddEmployeeAccountsElement(employeeElement, employee, currentDate);

                        #endregion

                        employeeReportElement.Add(employeeElement);
                        employeeXmlId++;
                    }

                    #endregion
                }

                #region Default element

                if (employeeXmlId == 1)
                    AddDefaultElement(reportResult, employeeReportElement, "Employee");

                #endregion
            }

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            #region Close document

            rootElement.Add(employeeReportElement);
            document.Add(rootElement);

            return GetValidatedDocument(document, reportResult);

            #endregion
        }

        public XDocument CreateEmployeeScheduleData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            TryGetIdFromSelection(reportResult, out int? selectionTimeScheduleScenarioHeadId, "timeScheduleScenarioHeadId");

            if (!TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out List<Employee> employees, out List<int> selectionEmployeeIds, out List<int> selectionAccountIds, out TermGroup_EmployeeSelectionAccountingType selectionAccountingType, timeScheduleScenarioHeadId: selectionTimeScheduleScenarioHeadId))
                return null;
            if (!TryGetEmployeePostIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out List<int> selectionEmployeePostIds))
                return null;
            if (!TryGetIdsFromSelection(reportResult, out List<int> selectionShiftTypeIds, "shiftTypes"))
                return null;
            TryGetBoolFromSelection(reportResult, out bool filterOnAccounting, "filterOnAccounting");
            TryGetBoolFromSelection(reportResult, out bool excludeAbsence, "excludeAbsence");

            TryGetIncludeInactiveFromSelection(reportResult, out bool selectionIncludeInactive, out _, out bool? selectionActiveEmployees);
            TimeScheduleScenarioHead scenarioHead = selectionTimeScheduleScenarioHeadId.HasValue ? TimeScheduleManager.GetTimeScheduleScenarioHead(selectionTimeScheduleScenarioHeadId.Value, reportResult.Input.ActorCompanyId) : null;
            List<AccountInternalDTO> validAccountInternals = filterOnAccounting && !selectionAccountIds.IsNullOrEmpty() ? AccountManager.GetAccountInternals(reportResult.Input.ActorCompanyId, null).Where(w => selectionAccountIds.Contains(w.AccountId)).ToDTOs() : null;

            var employeeSelection = reportResult?.Input?.GetSelection<EmployeeSelectionDTO>("employees") ?? null;
            bool includeSecondary = employeeSelection?.IncludeSecondary ?? false;

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Init document

            XDocument document = reportResult.Input.Document;
            XElement rootElement = reportResult.Input.ElementRoot;
            XElement timeEmployeeScheduleElement = reportResult.Input.ElementFirst;

            #endregion

            #region ReportHeaderLabels

            XElement reportHeaderLabelsElement = CreateTimeReportHeaderLabelsElement();
            reportHeaderLabelsElement.Add(CreateDateIntervalLabelReportHeaderLabelsElement());
            this.AddGrossNetCostHeaderPermissionAndSettingReportHeaderLabelElements(reportHeaderLabelsElement);
            this.AddScenarioLabelReportHeaderLabelsElement(reportHeaderLabelsElement);
            this.AddLendedDescriptionLabelsReportHeaderLabelsElement(reportHeaderLabelsElement);
            timeEmployeeScheduleElement.Add(reportHeaderLabelsElement);

            #endregion

            #region ReportHeader

            XElement reportHeaderElement = CreateTimeReportHeaderElement(reportResult);
            reportHeaderElement.Add(CreateDateIntervalElement(selectionDateFrom, selectionDateTo));
            this.AddGrossNetCostHeaderPermissionAndSettingReportHeaderElements(reportHeaderElement, reportResult.Input.RoleId, reportResult.Input.ActorCompanyId);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            reportHeaderElement.Add(new XElement("CompanyLogo", GetCompanyLogoFilePath(entitiesReadOnly, reportResult.Input.ActorCompanyId, false)));
            this.AddScenarioReportHeaderElements(reportHeaderElement, scenarioHead);
            timeEmployeeScheduleElement.Add(reportHeaderElement);

            #endregion

            #region PageHeaderLabels

            XElement pageHeaderLabelsElement = new XElement("PageHeaderLabels");
            CreateTimeEmployeeSchedulePageHeaderLabelsElement(pageHeaderLabelsElement);
            timeEmployeeScheduleElement.Add(pageHeaderLabelsElement);

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                const int MAX_SHIFTS = 15;

                CultureInfo culture = CalendarUtility.GetValidCultureInfo(Constants.SYSLANGUAGE_LANGCODE_DEFAULT);
                List<string> weekNrs = CalendarUtility.GetWeekNrs(selectionDateFrom, selectionDateTo, culture);
                bool isEmployeePosts = false;
                List<EmployeePostDTO> employeePosts = new List<EmployeePostDTO>();
                List<EmployeeGroupDTO> employeeGroups = new List<EmployeeGroupDTO>();

                if (selectionEmployeePostIds.Any())
                {
                    isEmployeePosts = true;
                    selectionEmployeePostIds = selectionEmployeePostIds.Distinct().ToList();
                    employeePosts = TimeScheduleManager.GetEmployeePosts(reportResult.Input.ActorCompanyId, true, false).ToDTOs(true).ToList();
                    employeePosts = employeePosts.Where(w => selectionEmployeePostIds.Contains(w.EmployeePostId)).ToList();
                }

                if (isEmployeePosts)
                    employees = new List<Employee>();
                else if (employees == null && !selectionEmployeeIds.IsNullOrEmpty())
                    employees = EmployeeManager.GetAllEmployeesByIds(entities, reportResult.Input.ActorCompanyId, selectionEmployeeIds, active: selectionActiveEmployees, loadEmployment: true, getHidden: true);

                List<CompanyCategoryRecord> companyCategoryRecords = CategoryManager.GetCompanyCategoryRecords(entities, SoeCategoryType.Employee, SoeCategoryRecordEntity.Employee, reportResult.Input.ActorCompanyId);
                List<HolidayDTO> companyHolidays = null; //Loaded when needed
                List<AccountDimDTO> companyAccountDims = null; //Loaded when needed

                XElement employeeElement = null;
                int employeeXmlId = 1;
                int weekXmlId = 1;
                var lineSchedule = reportResult.GetDetailedInformation;
                int hiddenEmployeeId = base.GetHiddenEmployeeIdFromCache(entities, CacheConfig.Company(reportResult.Input.ActorCompanyId));
                var employeeAttestEmployeeDays = new Dictionary<int, List<AttestEmployeeDayDTO>>();
                var templateShiftItems = new List<TimeSchedulePlanningDayDTO>();
                var employeeAccounts = base.UseAccountHierarchyOnCompanyFromCache(entitiesReadOnly, base.ActorCompanyId) ? EmployeeManager.GetEmployeeAccounts(reportResult.ActorCompanyId, selectionEmployeeIds, selectionDateFrom, selectionDateTo) : null;

                #endregion

                #region Content

                //We need to create multiple Hidden employees for lineschedule using DetailedInformation Flag
                List<Tuple<int, string>> employeeLinks = new List<Tuple<int, string>>();

                if (reportResult.Input.ReportTemplateType == SoeReportTemplateType.TimeEmployeeTemplateSchedule)
                    templateShiftItems = TimeScheduleManager.GetTimeSchedulePlanningDaysFromTemplate(entities, reportResult.Input.ActorCompanyId, reportResult.Input.RoleId, reportResult.Input.UserId, selectionDateFrom, selectionDateTo, null, selectionEmployeeIds, isEmployeePosts ? selectionEmployeePostIds : null, includeGrossNetAndCost: true, includeEmploymentTaxAndSupplementChargeCost: true, loadTasksAndDelivery: true, doNotCheckHoliday: true);

                if (isEmployeePosts)
                {
                    foreach (int employeePostId in selectionEmployeePostIds)
                    {
                        employeeLinks.Add(new Tuple<int, string>(employeePostId, string.Empty));
                    }
                }
                else
                {
                    bool loadShiftsForHidden = reportResult.Input.ReportTemplateType != SoeReportTemplateType.TimeEmployeeTemplateSchedule && selectionEmployeeIds.Contains(hiddenEmployeeId);
                    var hiddenShifts = loadShiftsForHidden ? TimeScheduleManager.GetTimeSchedulePlanningShifts_ByProcedure(reportResult.ActorCompanyId, reportResult.UserId, 0, reportResult.RoleId, selectionDateFrom, selectionDateTo, hiddenEmployeeId.ObjToList(), TimeSchedulePlanningMode.SchedulePlanning, TimeSchedulePlanningDisplayMode.Admin, true, false, false, timeScheduleScenarioHeadId: selectionTimeScheduleScenarioHeadId, includeShiftRequest: false) : null;
                    List<AttestEmployeeDayDTO> shiftItems;

                    string dayLinks = "";


                    foreach (int employeeId in selectionEmployeeIds)
                    {
                        Employee employee = employees.FirstOrDefault(i => i.EmployeeId == employeeId);
                        if (lineSchedule && hiddenShifts != null && employee?.EmployeeId == hiddenEmployeeId)
                        {
                            foreach (var shift in hiddenShifts.Where(w => w.EmployeeId == employee.EmployeeId && w.Link.HasValue).OrderBy(s => s.Link.Value))
                            {
                                if (!employeeLinks.Any(a => a.Item2.Equals(shift.Link.ToString(), StringComparison.OrdinalIgnoreCase)))
                                    employeeLinks.Add(Tuple.Create(employeeId, shift.Link.ToString()));
                            }
                        }
                        else
                        {
                            if (!lineSchedule && hiddenShifts != null && employee?.EmployeeId == hiddenEmployeeId)
                            {
                                foreach (var shift in hiddenShifts.Where(w => w.EmployeeId == employee.EmployeeId && w.Link.HasValue).OrderBy(s => s.Link.Value))
                                {
                                    if (!dayLinks.Contains(shift.Link.ToString()))
                                        dayLinks = dayLinks + "#" + shift.Link.ToString();
                                }
                            }
                            else
                            {
                                employeeLinks.Add(Tuple.Create(employeeId, string.Empty));

                                if (reportResult.Input.ReportTemplateType == SoeReportTemplateType.TimeEmployeeSchedule)
                                {
                                    var input = GetAttestEmployeeInput.CreateAttestInputForWeb(reportResult.ActorCompanyId, reportResult.UserId, reportResult.RoleId, employeeId, selectionDateFrom, selectionDateTo, null, null, true.ToLoadType(InputLoadType.GrossNetCost), InputLoadType.Shifts);
                                    input.SetOptionalParameters(companyHolidays, companyAccountDims, doGetOnlyActive: !selectionIncludeInactive, doGetHidden: true, doCalculateEmploymentTaxAndSupplementChargeCost: true, filterShiftTypeIds: !selectionShiftTypeIds.IsNullOrEmpty() ? selectionShiftTypeIds : null, timeScheduleScenarioHeadId: selectionTimeScheduleScenarioHeadId, validAccountInternals: validAccountInternals, employeeSelectionAccountingType: selectionAccountingType, doGetOnDuty: true, includeDeviationOnZeroDayFromScenario: true, doGetSecondaryAccounts: includeSecondary);

                                    shiftItems = TimeTreeAttestManager.GetAttestEmployeeDays(entities, input);
                                    employeeAttestEmployeeDays.Add(employeeId, shiftItems);

                                    foreach (var day in shiftItems)
                                    {
                                        if (day.Shifts.Any(y => y.Type == TermGroup_TimeScheduleTemplateBlockType.OnDuty))
                                        {
                                            foreach (var shiftGroup in day.Shifts.Where(s => s.Type == TermGroup_TimeScheduleTemplateBlockType.OnDuty).GroupBy(g => g.Link))
                                            {
                                                foreach (var shift in shiftGroup)
                                                {
                                                    if (!employeeLinks.Any(a => a.Item2.Equals(shift.Link.ToString(), StringComparison.OrdinalIgnoreCase)))
                                                        employeeLinks.Add(Tuple.Create(employeeId, shift.Link.ToString()));
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (reportResult.Input.ReportTemplateType == SoeReportTemplateType.TimeEmployeeTemplateSchedule)
                                {
                                    List<TimeSchedulePlanningDayDTO> employeeTemplateShiftItems = templateShiftItems.Where(s => s.EmployeeId == employeeId).ToList();

                                    if (employeeTemplateShiftItems.Any(y => y.Type == TermGroup_TimeScheduleTemplateBlockType.OnDuty))
                                    {
                                        foreach (var shiftGroup in employeeTemplateShiftItems.Where(s => s.Type == TermGroup_TimeScheduleTemplateBlockType.OnDuty).GroupBy(g => g.Link))
                                        {
                                            foreach (var shift in shiftGroup)
                                            {
                                                if (!employeeLinks.Any(a => a.Item2.Equals(shift.Link.ToString(), StringComparison.OrdinalIgnoreCase)))
                                                    employeeLinks.Add(Tuple.Create(employeeId, shift.Link.ToString()));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(dayLinks))
                        employeeLinks.Add(Tuple.Create(hiddenEmployeeId, dayLinks));
                }

                foreach (var employeeLink in employeeLinks)
                {
                    #region Employee

                    #region Prereq

                    string hiddenLink = employeeLink.Item2;
                    Employee employee = null;
                    EmployeePostDTO employeePost = null;
                    EmployeeGroupDTO employeeGroup = null;
                    List<CompanyCategoryRecord> employeeCategoryRecords = new List<CompanyCategoryRecord>();

                    if (!isEmployeePosts)
                    {
                        employee = employees.FirstOrDefault(i => i.EmployeeId == employeeLink.Item1);
                        if (employee == null)
                            continue;

                        employeeGroup = employee.GetEmployeeGroup(selectionDateFrom, this.personalDataRepository.EmployeeGroups).ToDTO(false);
                        employeeCategoryRecords = companyCategoryRecords.GetCategoryRecords(employee.EmployeeId, selectionDateFrom, selectionDateTo);
                    }
                    else
                    {
                        employeePost = employeePosts.FirstOrDefault(f => f.EmployeePostId == employeeLink.Item1);
                        if (employeePost == null)
                            continue;

                        employeeGroup = employeeGroups.FirstOrDefault(f => f.EmployeeGroupId == employeePost.EmployeeGroupId);
                    }

                    #endregion

                    #region Employee Element

                    List<AttestEmployeeDayDTO> employeeDayItem = new List<AttestEmployeeDayDTO>();
                    string employeeName = !isEmployeePosts ? employee.Name : employeePost.Name;

                    if (reportResult.Input.ReportTemplateType == SoeReportTemplateType.TimeEmployeeTemplateSchedule && !hiddenLink.IsNullOrEmpty())
                    {
                        List<TimeSchedulePlanningDayDTO> employeeTemplateShiftItems = templateShiftItems.Where(s => s.EmployeeId == employee.EmployeeId).ToList();
                        if (employeeTemplateShiftItems.Any(s => s.Type == TermGroup_TimeScheduleTemplateBlockType.OnDuty) && !isEmployeePosts)
                        {
                            employeeName = $"{employeeName} ({GetText(12165, "Jour")})";
                        }
                    }
                    else if (employee != null)
                    {
                        employeeDayItem = employeeAttestEmployeeDays.GetValue(employee.EmployeeId) ?? null;
                        if (!hiddenLink.IsNullOrEmpty() && !employeeDayItem.IsNullOrEmpty())
                        {
                            bool addedOnDutyName = false;
                            foreach (AttestEmployeeDayDTO dayItem in employeeDayItem)
                            {
                                if (dayItem.Shifts.Any(s => s.Type == TermGroup_TimeScheduleTemplateBlockType.OnDuty) && !isEmployeePosts && !addedOnDutyName)
                                {
                                    employeeName = $"{employeeName} ({GetText(12165, "Jour")})";
                                    addedOnDutyName = true;
                                }
                            }
                        }
                        else if (!employeeDayItem.IsNullOrEmpty())
                        {
                            foreach (AttestEmployeeDayDTO dayItem in employeeDayItem)
                                dayItem.TrimDayFromOnDutyOutsideSchedule();
                        }
                    }

                    employeeElement = new XElement("Employee",
                        new XAttribute("id", employeeXmlId),
                        new XElement("EmployeeNr", !isEmployeePosts ? employee.EmployeeNr : string.Empty),
                        new XElement("EmployeeName", employeeName),
                        new XElement("EmployeeCategory", employeeCategoryRecords.Where(i => i.Entity == (int)SoeCategoryType.Employee && i.Default).Select(i => i.Category.Name).ToCommaSeparated()),
                        new XElement("EmployeeGroup", employeeGroup?.Name.NullToEmpty() ?? string.Empty),
                        new XElement("EmployeeGroupRuleWorkTimeWeek", employeeGroup?.RuleWorkTimeWeek ?? 0),
                        new XElement("EmployeeGroupRuleWorkTimeYear", employeeGroup?.RuleWorkTimeYear ?? 0),
                        new XElement("EmployeeGroupRuleRestTimeWeek", employeeGroup?.RuleRestTimeWeek ?? 0),
                        new XElement("EmployeeGroupRuleRestTimeDay", employeeGroup?.RuleRestTimeDay ?? 0));
                    base.personalDataRepository.AddEmployee(employee, employeeElement);

                    #endregion

                    #region Categories Element

                    int categoryXmlId = 1;
                    foreach (CompanyCategoryRecord record in employeeCategoryRecords)
                    {
                        XElement categoryElement = new XElement("Categories",
                            new XAttribute("Id", categoryXmlId),
                            new XElement("CategoryCode", record.Category?.Code.NullToEmpty() ?? string.Empty),
                            new XElement("CategoryName", record.Category?.Name.NullToEmpty() ?? string.Empty),
                            new XElement("isDefaultCategory", record.Default.ToInt()),
                            new XElement("EmployeeNr", !isEmployeePosts ? employee.EmployeeNr : string.Empty));

                        employeeElement.Add(categoryElement);
                        categoryXmlId++;
                    }

                    if (categoryXmlId == 1)
                        AddDefaultElement(reportResult, employeeElement, "Categories");

                    #endregion

                    if (reportResult.Input.ReportTemplateType == SoeReportTemplateType.TimeEmployeeSchedule)
                    {
                        #region TimeEmployeeSchedule

                        if (companyHolidays == null)
                            companyHolidays = CalendarManager.GetHolidaysByCompany(entities, reportResult.Input.ActorCompanyId);
                        if (companyAccountDims == null)
                            companyAccountDims = AccountManager.GetAccountDimsByCompany(entities, reportResult.Input.ActorCompanyId).ToDTOs();

                        employeeElement.Add(CreateTimeEmployeeScheduleCommmonXML(entities, ref weekXmlId, reportResult, employee, selectionDateFrom, selectionDateTo, weekNrs, selectionShiftTypeIds, selectionTimeScheduleScenarioHeadId, onlyActive: !selectionIncludeInactive, returnWeek: true, loadGrossNetCost: true, doCalculateEmploymentTaxAndSupplementChargeCost: true, hiddenLink: hiddenLink, validAccountInternals: validAccountInternals, companyHolidays: companyHolidays, companyAccountDims: companyAccountDims, employeeSelectionAccountingType: selectionAccountingType, employeeDayItem: employeeDayItem, employeeAccounts: employeeAccounts, selectionAccountIds: selectionAccountIds, excludeAbsence: excludeAbsence));

                        #endregion
                    }
                    else if (reportResult.ReportTemplateType == SoeReportTemplateType.TimeEmployeeTemplateSchedule)
                    {
                        #region TimeEmployeeTemplateSchedule

                        List<int> employeeIds = employeeLink.Item1.ObjToList();
                        List<TimeSchedulePlanningDayDTO> items = templateShiftItems.Where(x => x.EmployeeId.IsEqualToAny(employeeIds.ToArray())).ToList();

                        if (items.Any() && !hiddenLink.IsNullOrEmpty())
                        {
                            items = items.Where(b => b.Link.ToString() == hiddenLink).ToList();
                            if (employee?.EmployeeId != hiddenEmployeeId)
                                items = items.Where(s => s.Type == TermGroup_TimeScheduleTemplateBlockType.OnDuty).ToList();
                        }
                        else
                            items = items.Where(b => b.Type != TermGroup_TimeScheduleTemplateBlockType.OnDuty).ToList();

                        if (items.Any())
                        {
                            #region TimeSchedulePlanningDayDTO

                            foreach (var weekNrGroup in weekNrs.GroupBy(w => w.Split('_')[2]).ToList())
                            {
                                #region Week

                                var weekNr = weekNrGroup.First();
                                var dayElements = new List<XElement>();

                                var aggregatedDays = new List<TimeSchedulePlanningAggregatedDayDTO>();
                                var week = items.Where(w => DateTime.Parse(weekNr.Split('_')[2]) == CalendarUtility.GetBeginningOfWeek(w.ActualDate)).ToList();
                                var days = week.GroupBy(i => i.StartTime.Date).ToList();
                                foreach (var day in days)
                                {
                                    aggregatedDays.Add(new TimeSchedulePlanningAggregatedDayDTO(day.ToList(), selectionShiftTypeIds));
                                }

                                int dayXmlId = 1;
                                int employeeWeekScheduleTimeTotal = 0;
                                foreach (var day in aggregatedDays.OrderBy(i => i.StartTime))
                                {
                                    #region Day

                                    #region Prereq

                                    int scheduleTime = Convert.ToInt32(day.ScheduleTime.TotalMinutes) - day.ScheduleBreakTime;
                                    employeeWeekScheduleTimeTotal += scheduleTime;

                                    var shiftElements = new List<XElement>();

                                    #endregion

                                    #region Day Element

                                    XElement dayElement = new XElement("Day",
                                        new XAttribute("id", dayXmlId),
                                        new XElement("IsTemplate", 1),
                                        new XElement("TemplateNbrOfWeeks", day.NbrOfWeeks),
                                        new XElement("TemplateDayNumber", day.DayNumber),
                                        new XElement("DayNr", (int)day.Date.DayOfWeek),
                                        new XElement("IsZeroScheduleDay", day.IsScheduleZeroDay.ToInt()),
                                        new XElement("IsAbsenceDay", day.IsWholeDayAbsence.ToInt()),
                                        new XElement("IsPreliminary", 0),
                                        new XElement("AbsencePayrollProductName", string.Empty),
                                        new XElement("AbsencePayrollProductShortName", string.Empty),
                                        new XElement("ScheduleStartTime", CalendarUtility.GetHoursAndMinutesString(day.ScheduleStartTime)),
                                        new XElement("ScheduleStopTime", CalendarUtility.GetHoursAndMinutesString(day.ScheduleStopTime)),
                                        new XElement("ScheduleTime", scheduleTime),
                                        new XElement("OccupiedTime", 0),
                                        new XElement("ScheduleBreakTime", day.ScheduleBreakTime),
                                        new XElement("ScheduleBreak1Start", CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, day.Break1StartTime)),
                                        new XElement("ScheduleBreak1Minutes", day.Break1Minutes),
                                        new XElement("ScheduleBreak2Start", CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, day.Break2StartTime)),
                                        new XElement("ScheduleBreak2Minutes", day.Break2Minutes),
                                        new XElement("ScheduleBreak3Start", CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, day.Break3StartTime)),
                                        new XElement("ScheduleBreak3Minutes", day.Break3Minutes),
                                        new XElement("ScheduleBreak4Start", CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, day.Break4StartTime)),
                                        new XElement("ScheduleBreak4Minutes", day.Break4Minutes),
                                        new XElement("ScheduleDate", day.Date),
                                        new XElement("ScheduleGrossTimeMinutes", (int)day.GrossTime.TotalMinutes),
                                        new XElement("ScheduleNetTimeMinutes", (int)day.NetTime.TotalMinutes),
                                        new XElement("ScheduleTotalCost", day.TotalCost));

                                    if (!isEmployeePosts)
                                    {
                                        Employment employment = employee.GetEmployment(day.Date);
                                        dayElement.Add(
                                            new XElement("EmploymentWorkTimeWeek", employment != null ? employment.GetWorkTimeWeek(day.Date) : 0),
                                            new XElement("EmploymentPercent", employment != null ? employment.GetPercent(day.Date) : 0));
                                    }
                                    else
                                    {
                                        dayElement.Add(
                                            new XElement("EmploymentWorkTimeWeek", employeePost.WorkTimeWeek),
                                            new XElement("EmploymentPercent", employeePost.WorkTimePercent));
                                    }

                                    #endregion

                                    int shiftXmlId = 1;
                                    foreach (var dayDTO in day.DayDTOs.OrderBy(st => st.StartTime).ToList())
                                    {
                                        #region Prereq

                                        string shiftName = StringUtility.NullToEmpty(dayDTO.ShiftTypeName);
                                        string shiftDescription = StringUtility.NullToEmpty(dayDTO.Description);
                                        if (shiftDescription == string.Empty)
                                            shiftDescription = StringUtility.NullToEmpty(dayDTO.ShiftTypeDesc);
                                        DateTime shiftStartTime = CalendarUtility.GetScheduleTime(dayDTO.StartTime);
                                        DateTime shiftStopTime = CalendarUtility.GetScheduleTime(dayDTO.StopTime);
                                        string color = dayDTO.ShiftTypeColor ?? Constants.SHIFT_TYPE_DEFAULT_COLOR;
                                        int netTimeMinutes = dayDTO.NetTime;
                                        int grossTimeMinutes = dayDTO.GrossTime;
                                        if (grossTimeMinutes == 0 && netTimeMinutes != 0)
                                            grossTimeMinutes = netTimeMinutes;
                                        decimal totalCost = dayDTO.TotalCost;
                                        string link = dayDTO.Link != null ? dayDTO.Link.ToString() : string.Empty;
                                        int lended = 0; // Not needed in Template Schedule?  

                                        TimeDeviationCause timeDeviationCause = dayDTO.TimeDeviationCauseId.HasValue ? TimeDeviationCauseManager.GetTimeDeviationCause(dayDTO.TimeDeviationCauseId.Value, reportResult.Input.ActorCompanyId, false) : null;
                                        string timeDeviationCauseName = timeDeviationCause != null ? timeDeviationCause.Name : string.Empty;

                                        // Set fixed background color if shift is absence
                                        if (timeDeviationCause != null)
                                            color = "#ef545e";   // @shiftAbsenceBackgroundColor

                                        #endregion

                                        #region Shift/Shifts Element

                                        if (shiftXmlId <= MAX_SHIFTS)
                                        {
                                            dayElement.Add(
                                                new XElement("Shift" + shiftXmlId + "Name", shiftName),
                                                new XElement("Shift" + shiftXmlId + "StartTime", shiftStartTime),
                                                new XElement("Shift" + shiftXmlId + "StopTime", shiftStopTime),
                                                new XElement("Shift" + shiftXmlId + "Description", shiftDescription),
                                                new XElement("Shift" + shiftXmlId + "Color", color),
                                                new XElement("Shift" + shiftXmlId + "GrossTimeMinutes", grossTimeMinutes),
                                                new XElement("Shift" + shiftXmlId + "NetTimeMinutes", netTimeMinutes),
                                                new XElement("Shift" + shiftXmlId + "TotalCost", totalCost),
                                                new XElement("Shift" + shiftXmlId + "TimeDeviationCauseName", timeDeviationCauseName),
                                                new XElement("Shift" + shiftXmlId + "Lended", lended),
                                                new XElement("Shift" + shiftXmlId + "Link", link));
                                        }

                                        shiftElements.Add(new XElement("Shifts",
                                            new XAttribute("id", shiftXmlId),
                                            new XElement("ShiftName", shiftName),
                                            new XElement("ShiftDescription", shiftDescription),
                                            new XElement("ShiftStartTime", shiftStartTime),
                                            new XElement("ShiftStopTime", shiftStopTime),
                                            new XElement("Color", color),
                                            new XElement("ShiftGrossTimeMinutes", grossTimeMinutes),
                                            new XElement("ShiftNetTimeMinutes", netTimeMinutes),
                                            new XElement("ShiftTotalCost", totalCost),
                                            new XElement("ShiftTimeDeviationCauseName", timeDeviationCauseName),
                                            new XElement("ShiftLended", lended)));

                                        shiftXmlId++;

                                        #endregion
                                    }

                                    #region Default Shift Element

                                    //Fill to 10 shifts
                                    for (int shiftNr = shiftXmlId; shiftNr <= MAX_SHIFTS; shiftNr++)
                                    {
                                        dayElement.Add(
                                            new XElement("Shift" + shiftNr + "Name", string.Empty),
                                            new XElement("Shift" + shiftNr + "StartTime", CalendarUtility.DATETIME_DEFAULT),
                                            new XElement("Shift" + shiftNr + "StopTime", CalendarUtility.DATETIME_DEFAULT),
                                            new XElement("Shift" + shiftNr + "Description", string.Empty),
                                            new XElement("Shift" + shiftNr + "Color", Constants.SHIFT_TYPE_DEFAULT_COLOR),
                                            new XElement("Shift" + shiftNr + "GrossTimeMinutes", 0),
                                            new XElement("Shift" + shiftNr + "NetTimeMinutes", 0),
                                            new XElement("Shift" + shiftNr + "TotalCost", 0),
                                            new XElement("Shift" + shiftNr + "TimeDeviationCauseName", 0),
                                            new XElement("Shift" + shiftNr + "Lended", 0));

                                    }

                                    //Add all shifts
                                    foreach (var shiftElement in shiftElements)
                                    {
                                        dayElement.Add(shiftElement);
                                    }

                                    #endregion

                                    dayElements.Add(dayElement);
                                    dayXmlId++;

                                    #endregion
                                }

                                #region Default Element Day

                                if (dayXmlId == 1) // Do not use default xml since it will add shifts. Old report didn't do that and adding shifts effects reports and also xmlsize
                                {
                                    XElement dayElement = new XElement("Day",
                                        new XAttribute("id", 1),
                                        new XElement("DayNr", 0),
                                        new XElement("IsZeroScheduleDay", 0),
                                        new XElement("IsAbsenceDay", 0),
                                        new XElement("IsPreliminary", 0),
                                        new XElement("AbsencePayrollProductName", ""),
                                        new XElement("AbsencePayrollProductShortName", ""),
                                        new XElement("ScheduleStartTime", "00:00"),
                                        new XElement("ScheduleStopTime", "00:00"),
                                        new XElement("ScheduleTime", 0),
                                        new XElement("OccupiedTime", 0),
                                        new XElement("ScheduleBreakTime", 0),
                                        new XElement("ScheduleBreak1Start", CalendarUtility.DATETIME_DEFAULT),
                                        new XElement("ScheduleBreak1Minutes", 0),
                                        new XElement("ScheduleBreak2Start", CalendarUtility.DATETIME_DEFAULT),
                                        new XElement("ScheduleBreak2Minutes", 0),
                                        new XElement("ScheduleBreak3Start", CalendarUtility.DATETIME_DEFAULT),
                                        new XElement("ScheduleBreak3Minutes", 0),
                                        new XElement("ScheduleBreak4Start", CalendarUtility.DATETIME_DEFAULT),
                                        new XElement("ScheduleBreak4Minutes", 0),
                                        new XElement("ScheduleDate", CalendarUtility.DATETIME_DEFAULT));

                                    if (!isEmployeePosts)
                                    {
                                        DateTime firstDateOfWeek = Convert.ToDateTime(weekNr.Split('_')[2]);
                                        DateTime lastDateOfWeek = CalendarUtility.GetLastDateOfWeek(firstDateOfWeek);

                                        Employment employmentFirstDayOfWeek = employee.GetEmployment(firstDateOfWeek);
                                        Employment employmentLastDayOfWeek = employee.GetEmployment(lastDateOfWeek);

                                        int workTimeFirstDayOfWeek = employmentFirstDayOfWeek != null ? employmentFirstDayOfWeek.GetWorkTimeWeek(firstDateOfWeek) : 0;
                                        int workTimeLastDayOfWeek = employmentLastDayOfWeek != null ? employmentLastDayOfWeek.GetWorkTimeWeek(lastDateOfWeek) : 0;

                                        decimal percentFirstDayOfWeek = employmentFirstDayOfWeek != null ? employmentFirstDayOfWeek.GetPercent(firstDateOfWeek) : 0;
                                        decimal percentLastDayOfWeek = employmentLastDayOfWeek != null ? employmentLastDayOfWeek.GetPercent(lastDateOfWeek) : 0;

                                        dayElement.Add(
                                            new XElement("EmploymentWorkTimeWeek", Math.Max(workTimeFirstDayOfWeek, workTimeLastDayOfWeek)),
                                            new XElement("EmploymentPercent", Math.Max(percentFirstDayOfWeek, percentLastDayOfWeek)));
                                    }
                                    else
                                    {
                                        dayElement.Add(
                                            new XElement("EmploymentWorkTimeWeek", employeePost.WorkTimeWeek),
                                            new XElement("EmploymentPercent", employeePost.WorkTimePercent));
                                    }

                                    dayElements.Add(dayElement);
                                }

                                #endregion

                                #region Week Element

                                var weekElement = new XElement("Week",
                                    new XAttribute("id", weekXmlId),
                                    new XElement("ScheduleWeekNr", Convert.ToInt32(weekNr.Split('_')[1])),
                                    new XElement("EmployeeWeekTimeTotal", employeeWeekScheduleTimeTotal));
                                weekXmlId++;

                                foreach (var day in dayElements)
                                {
                                    weekElement.Add(day);
                                }

                                employeeElement.Add(weekElement);

                                #endregion

                                #endregion
                            }

                            #endregion
                        }

                        #endregion
                    }

                    timeEmployeeScheduleElement.Add(employeeElement);
                    employeeXmlId++;

                    #endregion
                }

                #endregion

                #region Default Employee Element

                if (employeeXmlId == 1)
                    AddDefaultElement(reportResult, timeEmployeeScheduleElement, "Employee");

                #endregion
            }

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            #region Close document

            rootElement.Add(timeEmployeeScheduleElement);
            document.Add(rootElement);

            return GetValidatedDocument(document, reportResult);

            #endregion
        }

        public XDocument CreateEmployeeScheduleDataSmallReportData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            if (!TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo))
                return null;
            TryGetIdFromSelection(reportResult, out int? selectionTimeScheduleScenarioHeadId, "timeScheduleScenarioHeadId");
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out List<Employee> employees, out List<int> selectionEmployeeIds, out List<int> selectionAccountIds, out TermGroup_EmployeeSelectionAccountingType selectionAccountingType, selectionTimeScheduleScenarioHeadId))
                return null;

            TryGetBoolFromSelection(reportResult, out bool filterOnAccounting, "filterOnAccounting");
            TryGetBoolFromSelection(reportResult, out bool selectionIncludePreliminary, "includePreliminary");
            TryGetBoolFromSelection(reportResult, out bool selectionShowOnlyTotals, "showOnlyTotals");
            TryGetIncludeInactiveFromSelection(reportResult, out _, out _, out bool? selectionActiveEmployees);
            TryGetIdsFromSelection(reportResult, out List<int> selectionShiftTypeIds, "shiftTypes");

            if (employees == null)
                employees = EmployeeManager.GetAllEmployeesByIds(reportResult.Input.ActorCompanyId, selectionEmployeeIds, active: selectionActiveEmployees, loadEmployment: true);

            TimeScheduleScenarioHead scenarioHead = selectionTimeScheduleScenarioHeadId.HasValue ? TimeScheduleManager.GetTimeScheduleScenarioHead(selectionTimeScheduleScenarioHeadId.Value, reportResult.Input.ActorCompanyId) : null;
            AccountDim accountDimStd = AccountManager.GetAccountDimStd(reportResult.Input.ActorCompanyId);
            List<AccountInternalDTO> validAccountInternals = filterOnAccounting && !selectionAccountIds.IsNullOrEmpty() ? AccountManager.GetAccountInternals(reportResult.Input.ActorCompanyId, null).Where(w => selectionAccountIds.Contains(w.AccountId)).ToDTOs() : null;
            List<AccountDim> accountDimInternals = AccountManager.GetAccountDimInternalsByCompany(reportResult.Input.ActorCompanyId);
            List<int> birthYears = employees.Select(s => CalendarUtility.GetBirthDateFromSecurityNumber(s.SocialSec)).Where(w => w.HasValue).Select(s => s.Value.Year).ToList();
            birthYears.Add(DateTime.Now.Date.AddYears(-40).Year);
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            int sysCountryId = base.GetCompanySysCountryIdFromCache(entities, base.ActorCompanyId);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var employeeAccounts = base.UseAccountHierarchyOnCompanyFromCache(entitiesReadOnly, base.ActorCompanyId) ? EmployeeManager.GetEmployeeAccounts(reportResult.ActorCompanyId, selectionEmployeeIds, selectionDateFrom, selectionDateTo) : null;

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Init document

            XDocument document = reportResult.Input.Document;
            XElement rootElement = reportResult.Input.ElementRoot;
            XElement employeeScheduleElement = reportResult.Input.ElementFirst;

            #endregion

            #region ReportHeaderLabels

            XElement reportHeaderLabelsElement = CreateTimeReportHeaderLabelsElement();
            reportHeaderLabelsElement.Add(CreateDateIntervalLabelReportHeaderLabelsElement());
            CreateEnterpriseCurrencyReportHeaderLabelsElement(reportHeaderLabelsElement);
            CreatePayrollProductIntervalReportHeaderLabelsElement(reportHeaderLabelsElement);
            this.AddScenarioLabelReportHeaderLabelsElement(reportHeaderLabelsElement);
            employeeScheduleElement.Add(reportHeaderLabelsElement);

            #endregion

            #region ReportHeader

            XElement reportHeaderElement = CreateTimeReportHeaderElement(reportResult);
            reportHeaderElement.Add(CreateDateIntervalElement(selectionDateFrom, selectionDateTo));
            reportHeaderElement.Add(CreateIncludePreliminaryElement(selectionIncludePreliminary));
            this.AddEnterpriseCurrencyPageLabelElements(reportHeaderElement);
            this.AddShowOnlyTotalsReportHeaderElement(reportHeaderElement, selectionShowOnlyTotals);
            this.AddScenarioReportHeaderElements(reportHeaderElement, scenarioHead);
            employeeScheduleElement.Add(reportHeaderElement);

            #endregion

            #region PageHeaderLabels

            XElement pageHeaderLabelsElement = new XElement("PageHeaderLabels");
            CreateTimePayrollTransactionReportPageHeaderLabelsElement(pageHeaderLabelsElement);
            this.AddAccountDimPageHeaderLabelElements(pageHeaderLabelsElement, accountDimStd, accountDimInternals);
            employeeScheduleElement.Add(pageHeaderLabelsElement);

            #endregion

            #region Load in parallell

            Dictionary<int, List<TimeEmployeeScheduleDataSmallDTO>> timeEmployeeScheduleDataDict = null;
            List<EmployeeGroup> employeeGroups = null;
            List<PayrollGroup> payrollGroups = null;
            List<PayrollPriceType> payrollPriceTypes = null;
            List<TimeCode> timeCodes = null;
            List<TimeDeviationCause> timeDeviationCauses = null;
            List<TimeScheduleType> timeScheduleTypes = null;
            List<TimeAccumulator> timeAccumulators = null;
            List<ShiftTypeDTO> shiftTypes = null;
            List<CompanyCategoryRecord> companyCategoryRecords = null;
            List<AccountStd> accountStds = null;
            List<Account> accountInternals = null;
            List<AttestState> attestStates = null;
            List<SysPayrollPriceViewDTO> sysPayrollPrice = null;
            Dictionary<int, string> sysPayrollTypeTerms = null;
            List<Tuple<int, DateTime, decimal>> employmentTaxRates = null;
            Currency currency = null;

            Dictionary<int, int> shiftTypeDict = new Dictionary<int, int>();

            Parallel.Invoke(GetDefaultParallelOptions(), () =>
            {
                timeEmployeeScheduleDataDict = TimeScheduleManager.GetTimeEmployeeScheduleSmallDTODictForReport(selectionDateFrom, selectionDateTo, employees.Where(w => selectionEmployeeIds.Contains(w.EmployeeId)).ToList(), reportResult.Input.ActorCompanyId, reportResult.Input.RoleId, selectionShiftTypeIds, timeScheduleScenarioHeadId: selectionTimeScheduleScenarioHeadId);
            },
            () =>
            {
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                employeeGroups = EmployeeManager.GetEmployeeGroups(reportResult.ActorCompanyId);
                payrollGroups = PayrollManager.GetPayrollGroups(reportResult.ActorCompanyId, loadAccountStd: true);
                payrollPriceTypes = PayrollManager.GetPayrollPriceTypes(reportResult.ActorCompanyId, null, false);
                timeCodes = TimeCodeManager.GetTimeCodes(reportResult.ActorCompanyId);
                timeDeviationCauses = TimeDeviationCauseManager.GetTimeDeviationCauses(reportResult.ActorCompanyId);
                timeScheduleTypes = TimeScheduleManager.GetTimeScheduleTypes(reportResult.ActorCompanyId, loadFactors: true);
                timeAccumulators = TimeAccumulatorManager.GetTimeAccumulators(reportResult.Input.ActorCompanyId);
                shiftTypes = TimeScheduleManager.GetShiftTypes(reportResult.Input.ActorCompanyId).ToDTOs().ToList();
                companyCategoryRecords = CategoryManager.GetCompanyCategoryRecords(SoeCategoryType.Employee, SoeCategoryRecordEntity.Employee, reportResult.Input.ActorCompanyId);
                accountStds = AccountManager.GetAccountStdsByCompanyIgnoreState(entitiesReadOnly, reportResult.Input.ActorCompanyId);
                accountInternals = AccountManager.GetAccountsInternalsByCompany(reportResult.Input.ActorCompanyId);
                attestStates = AttestManager.GetAttestStates(reportResult.Input.ActorCompanyId, TermGroup_AttestEntity.PayrollTime, SoeModule.Time);
                sysPayrollPrice = PayrollManager.GetSysPayrollPriceView(sysCountryId);
                sysPayrollTypeTerms = base.GetTermGroupDict(TermGroup.SysPayrollType);
                employmentTaxRates = PayrollManager.GetRatesFromPayrollPriceView(entitiesReadOnly, reportResult.Input.ActorCompanyId, selectionDateFrom, selectionDateTo, birthYears, sysPayrollPrice, TermGroup_SysPayrollPrice.SE_EmploymentTax, sysCountryId);
                currency = CountryCurrencyManager.GetCurrencyFromType(reportResult.Input.ActorCompanyId, TermGroup_CurrencyType.EnterpriseCurrency);

                #region Sales element

                employeeScheduleElement.Add(CreateSalesElementFromFrequency(entitiesReadOnly, reportResult.Input.ActorCompanyId, reportResult.Input.RoleId, selectionDateFrom, selectionDateTo, selectionAccountIds.IsNullOrEmpty() ? null : selectionAccountIds));

                #endregion

                #region ShiftTypes element

                int shiftTypeXmlId = 1;
                foreach (var shiftType in shiftTypes)
                {
                    employeeScheduleElement.Add(new XElement("ShiftTypes",
                        new XAttribute("id", shiftTypeXmlId),
                        new XElement("ExternalCode", shiftType.ExternalCode),
                        new XElement("Name", shiftType.Name),
                        new XElement("Color", shiftType.Color),
                        new XElement("ScheduleTypeId", shiftType.TimeScheduleTypeId ?? 0)));

                    shiftTypeDict.Add(shiftType.ShiftTypeId, shiftTypeXmlId);
                    shiftTypeXmlId++;
                }

                #endregion

                #region Account element

                foreach (var accountInternal in accountInternals)
                {
                    employeeScheduleElement.Add(new XElement("Account",
                        new XAttribute("id", accountInternal.AccountId),
                        new XElement("Nr", accountInternal.AccountNr),
                        new XElement("Name", accountInternal.Name)));
                }

                foreach (var accountStd in accountStds)
                {
                    employeeScheduleElement.Add(new XElement("Account",
                        new XAttribute("id", accountStd.Account.AccountId),
                        new XElement("Nr", accountStd.Account.AccountNr),
                        new XElement("Name", accountStd.Account.Name)));
                }

                employeeScheduleElement.Add(new XElement("Account",
                        new XAttribute("id", 0),
                        new XElement("Nr", string.Empty),
                        new XElement("Name", string.Empty)));

                #endregion

                #region EmployeeGroup element

                foreach (var employeeGroup in employeeGroups)
                {
                    employeeScheduleElement.Add(new XElement("EmployeeGroup",
                        new XAttribute("id", employeeGroup.EmployeeGroupId),
                        new XElement("Name", employeeGroup.Name),
                        new XElement("WTWeek", employeeGroup.RuleWorkTimeWeek)));
                }

                employeeScheduleElement.Add(new XElement("EmployeeGroup",
                       new XAttribute("id", 0),
                       new XElement("Name", ""),
                       new XElement("WTWeek", 0)));

                #endregion

                #region DeviationCause element

                foreach (var timeDeviationCause in timeDeviationCauses)
                {
                    employeeScheduleElement.Add(new XElement("DeviationCause",
                        new XAttribute("id", timeDeviationCause.TimeDeviationCauseId),
                        new XElement("Code", timeDeviationCause.ExtCode),
                        new XElement("Name", timeDeviationCause.Name),
                        new XElement("HasAttachZeroDaysNbrOfDaySetting", timeDeviationCause.HasAttachZeroDaysNbrOfDaySetting.ToInt())));
                }

                employeeScheduleElement.Add(new XElement("DeviationCause",
                      new XAttribute("id", 0),
                        new XElement("Code", ""),
                        new XElement("Name", ""),
                        new XElement("HasAttachZeroDaysNbrOfDaySetting", 0)));

                #endregion

                #region TimeCode element

                foreach (var timeCode in timeCodes)
                {
                    employeeScheduleElement.Add(new XElement("TimeCode",
                        new XAttribute("id", timeCode.TimeCodeId),
                        new XElement("Code", timeCode.Code),
                        new XElement("Name", timeCode.Name),
                        new XElement("Type", timeCode.RegistrationType)));
                }

                employeeScheduleElement.Add(new XElement("TimeCode",
                      new XAttribute("id", 0),
                        new XElement("Code", ""),
                        new XElement("Name", ""),
                        new XElement("Type", 0)));

                #endregion

                #region ScheduleType element

                foreach (var timeScheduleType in timeScheduleTypes)
                {
                    employeeScheduleElement.Add(new XElement("ScheduleType",
                        new XAttribute("id", timeScheduleType.TimeScheduleTypeId),
                        new XElement("Code", timeScheduleType.Code),
                        new XElement("Name", timeScheduleType.Name),
                        new XElement("ReportScheduleType", timeScheduleType.ReportScheduleType),
                        new XElement("IsAll", timeScheduleType.IsAll.ToInt())));
                }

                employeeScheduleElement.Add(new XElement("ScheduleType",
                      new XAttribute("id", 0),
                        new XElement("Code", ""),
                        new XElement("Name", ""),
                        new XElement("ReportScheduleType", 0),
                        new XElement("IsAll", 0)));

                #endregion
            });

            #endregion

            #region Content

            int employeeXmlId = 1;
            int dayXmlId = 1;
            foreach (int employeeId in selectionEmployeeIds)
            {
                #region Prereq

                Employee employee = employees.FirstOrDefault(i => i.EmployeeId == employeeId);
                if (employee == null)
                    continue;

                if (!timeEmployeeScheduleDataDict.ContainsKey(employee.EmployeeId) || timeEmployeeScheduleDataDict[employee.EmployeeId].IsNullOrEmpty())
                    continue;

                List<DateTime> employmentDates = employee.GetEmploymentDates(selectionDateFrom, selectionDateTo);
                if (employmentDates.IsNullOrEmpty())
                    continue;

                List<TimeEmployeeScheduleDataSmallDTO> timeEmployeeScheduleDataForEmployee = timeEmployeeScheduleDataDict[employee.EmployeeId].Where(i => employmentDates.Contains(i.Date)).ToList();
                if (timeEmployeeScheduleDataForEmployee.IsNullOrEmpty())
                    continue;

                EmployeeGroup employeeGroup = employee.GetEmployeeGroup(selectionDateFrom, selectionDateTo, employeeGroups);
                if (employeeGroup == null)
                    continue;

                int birthYear = CalendarUtility.GetBirthYearFromSecurityNumber(employee.SocialSec);
                DateTime? birthDate = CalendarUtility.GetBirthDateFromSecurityNumber(employee.SocialSec);

                var employeeAccountsOnEmployee = employeeAccounts != null ? employeeAccounts.Where(i => i.EmployeeId == employee.EmployeeId).ToList() : new List<EmployeeAccount>();

                #endregion

                if (filterOnAccounting && selectionAccountingType == TermGroup_EmployeeSelectionAccountingType.TimeScheduleTemplateBlockAccount && !selectionAccountIds.IsNullOrEmpty())
                {
                    List<TimeEmployeeScheduleDataSmallDTO> filtered = new List<TimeEmployeeScheduleDataSmallDTO>();

                    foreach (var group in timeEmployeeScheduleDataForEmployee.GroupBy(g => $"{g.EmployeeId}#{g.Date}"))
                    {
                        foreach (var item in group.ToList())
                        {
                            if (item.AccountInternals != null && item.AccountInternals.ValidOnFiltered(validAccountInternals))
                            {
                                filtered.Add(item);
                                filtered.AddRange(item.GetOverlappedBreaks(group.ToList()));
                            }
                        }
                    }

                    timeEmployeeScheduleDataForEmployee = filtered.Distinct().ToList();
                }
                else if (filterOnAccounting && selectionAccountingType == TermGroup_EmployeeSelectionAccountingType.TimeScheduleTemplateBlock && !selectionAccountIds.IsNullOrEmpty())
                {
                    List<TimeEmployeeScheduleDataSmallDTO> filtered = new List<TimeEmployeeScheduleDataSmallDTO>();

                    foreach (var group in timeEmployeeScheduleDataForEmployee.GroupBy(g => $"{g.EmployeeId}#{g.Date}"))
                    {
                        foreach (var item in group.Where(w => w.AccountId.HasValue && selectionAccountIds.Contains(w.AccountId.Value)).ToList())
                        {
                            filtered.Add(item);
                            filtered.AddRange(item.GetOverlappedBreaks(group.ToList()));
                        }
                    }

                    timeEmployeeScheduleDataForEmployee = filtered.Distinct().ToList();
                }
                else if (filterOnAccounting && !selectionAccountIds.IsNullOrEmpty())
                {
                    List<TimeEmployeeScheduleDataSmallDTO> filtered = new List<TimeEmployeeScheduleDataSmallDTO>();
                    bool validForAccountOnBlockMatch = validAccountInternals.GroupBy(g => g.AccountDimId).Count() == 1;

                    foreach (var group in timeEmployeeScheduleDataForEmployee.GroupBy(g => $"{g.EmployeeId}#{g.Date}"))
                    {
                        foreach (var item in group.ToList())
                        {
                            if ((item.AccountInternals != null && item.AccountInternals.ValidOnFiltered(validAccountInternals))
                                || (validForAccountOnBlockMatch && item.AccountId.HasValue && selectionAccountIds.Contains(item.AccountId.Value)))
                            {
                                filtered.Add(item);
                                filtered.AddRange(item.GetOverlappedBreaks(group.ToList()));
                            }
                        }
                    }
                    timeEmployeeScheduleDataForEmployee = filtered.Distinct().ToList();
                }

                if (!selectionAccountIds.IsNullOrEmpty() && employeeAccountsOnEmployee.Any() && base.UseAccountHierarchyOnCompanyFromCache(entitiesReadOnly, reportResult.ActorCompanyId))
                {
                    foreach (var group in timeEmployeeScheduleDataForEmployee.Where(w => w.AccountId.HasValue && !w.IsBreak).GroupBy(g => $"{g.EmployeeId}#{g.Date}"))
                    {
                        var date = group.First().Date;

                        var employeeAccountsOnDate = employeeAccountsOnEmployee.GetEmployeeAccounts(date, date);

                        foreach (var item in group.ToList())
                        {
                            // case 1. If the shift's account is included in the list of accounts you are printing for, and is part of the employee's affiliation and is marked as default, then it is not lended = 0.
                            if (!item.AccountId.HasValue || (selectionAccountIds.Contains(item.AccountId.Value) && employeeAccountsOnDate.Any(e => e.AccountId == item.AccountId.Value && e.Default)))
                                item.LendedType = SoeTimeScheduleTemplateBlockLendedType.None;
                            // case 2. If the shift's account is included in the list of accounts you are printing for, and is part of the employee's affiliation and is NOT marked as default, then it is lended = 2.
                            else if (selectionAccountIds.Contains(item.AccountId.Value) && employeeAccountsOnDate.Any(e => e.AccountId == item.AccountId.Value && !e.Default))
                                item.LendedType = SoeTimeScheduleTemplateBlockLendedType.LendedInFromOther;
                            // case 3. If the shift's account is NOT included in the list of accounts you are printing for, but is part of the employee's affiliation, then it is lent out = 1 (in this case, the employee has their default affiliation among the accounts you are printing for, which is why it is included).
                            else if (!selectionAccountIds.Contains(item.AccountId.Value) && employeeAccountsOnDate.Any(e => e.AccountId == item.AccountId.Value))
                                item.LendedType = SoeTimeScheduleTemplateBlockLendedType.LendedToOther;
                        }
                    }
                }

                #region Employee element

                XElement employeeElement = new XElement("Employee",
                    new XAttribute("id", employeeXmlId),
                    new XElement("EmployeeNr", employee.EmployeeNr),
                    new XElement("EmployeeName", employee.Name),
                    new XElement("EmployeeGroupName", employeeGroup.Name));
                base.personalDataRepository.AddEmployee(employee, employeeElement);

                #endregion

                #region Day

                Dictionary<DateTime, int> dayDict = new Dictionary<DateTime, int>();

                foreach (DateTime day in CalendarUtility.GetDatesInInterval(selectionDateFrom, selectionDateTo))
                {
                    Convert.ToInt32(12);
                    Employment employment = employee.GetEmployment(day);
                    if (employment == null)
                        continue;
                    if (!birthDate.HasValue || birthDate.Value.AddYears(110) < DateTime.Today)
                        birthDate = DateTime.Now.Date.AddYears(-40);

                    if (birthYear == 0 || birthYear == 1900)
                        birthYear = birthDate.Value.Year;

                    List<TimeEmployeeScheduleDataSmallDTO> timeEmployeeScheduleDataForDay = timeEmployeeScheduleDataForEmployee.Where(w => w.Date == day).ToList();
                    decimal taxRate = PayrollManager.GetTaxRate(day, birthYear, employmentTaxRates);
                    var payrollGroup = employee.GetPayrollGroup(day, payrollGroups);
                    decimal supplementCharge = PayrollManager.CalculateSupplementChargePercentSE(entitiesReadOnly, reportResult.ActorCompanyId, day, payrollGroup?.PayrollGroupId, birthDate, PayrollManager.GetSysPayrollPriceInterval(reportResult.Input.ActorCompanyId, (int)TermGroup_SysPayrollPrice.SE_EmploymentTax, birthYear, day, sysCountryId, sysPayrollPrice), setDefaultAge: true, payrollGroupAccountStds: payrollGroup?.PayrollGroupAccountStd?.ToList());

                    XElement dayElement = new XElement("Day",
                        new XAttribute("id", dayXmlId),
                        new XElement("Date", day),
                        new XElement("Wk", CalendarUtility.GetWeekNr(day)),
                        new XElement("ETax", taxRate),
                        new XElement("SuplCharge", supplementCharge),
                        new XElement("Length", Convert.ToInt32(timeEmployeeScheduleDataForDay.Any() ? timeEmployeeScheduleDataForDay?.Where(w => !w.IsBreak).Sum(s => s.Quantity) : 0)),
                        new XElement("WTWeek", employment.GetWorkTimeWeek(day)),
                        new XElement("EGId", employment.GetEmployeeGroupId(day)));

                    employeeElement.Add(dayElement);
                    dayDict.Add(day, dayXmlId);
                    dayXmlId++;
                }

                #endregion

                int scheduleTransactionId = 1;
                ConcurrentBag<XElement> schTranElements = new ConcurrentBag<XElement>();
                Parallel.Invoke(GetDefaultParallelOptions(), () =>
                {
                    Parallel.ForEach(timeEmployeeScheduleDataForEmployee.GroupBy(g => g.TimeScheduleTemplateBlockId.ToString() + g.StartTime.ToString()), new ParallelOptions { MaxDegreeOfParallelism = 1 }, timeEmployeeScheduleDataGrouping =>
                    {
                        #region SchTran element

                        TimeEmployeeScheduleDataSmallDTO timeEmployeeScheduleData = timeEmployeeScheduleDataGrouping.First();

                        int dateDayXmlId = dayDict.First(f => f.Key == timeEmployeeScheduleData.Date).Value;
                        int shiftTypeId = timeEmployeeScheduleData.ShiftTypeId.HasValue && shiftTypeDict.ContainsKey(timeEmployeeScheduleData.ShiftTypeId.Value) ? shiftTypeDict[timeEmployeeScheduleData.ShiftTypeId.Value] : 0;

                        XElement schTranElement = new XElement("SchTran",
                            new XAttribute("id", scheduleTransactionId),
                            new XElement("DayId", dateDayXmlId),
                            new XElement("Q", timeEmployeeScheduleDataGrouping.Sum(s => s.Quantity)),
                            new XElement("GQ", timeEmployeeScheduleDataGrouping.Sum(s => s.GrossQuantity)),
                            new XElement("Price", timeEmployeeScheduleData.UnitPrice),
                            new XElement("Amount", timeEmployeeScheduleDataGrouping.Sum(s => s.Amount)),
                            new XElement("GAmount", timeEmployeeScheduleDataGrouping.Sum(s => s.GrossAmount)),
                            new XElement("VAmount", timeEmployeeScheduleDataGrouping.Sum(s => s.VatAmount)),
                            new XElement("Date", timeEmployeeScheduleData.Date),
                            new XElement("Start", CalendarUtility.MergeDateAndTime(timeEmployeeScheduleData.Date, timeEmployeeScheduleData.StartTime)),
                            new XElement("Stop", CalendarUtility.MergeDateAndTime(timeEmployeeScheduleData.Date, timeEmployeeScheduleData.StopTime)),
                            new XElement("ShiftType", shiftTypeId),
                            new XElement("IsBreak", timeEmployeeScheduleData.IsBreak.ToInt()),
                            new XElement("SubShift", timeEmployeeScheduleData.SubstituteShift.ToInt()),
                            new XElement("XShift", timeEmployeeScheduleData.ExtraShift.ToInt()),
                            new XElement("TDevId", timeEmployeeScheduleData.TimeDeviationCauseId.ToInt()),
                            new XElement("TCId", timeEmployeeScheduleData.TimeCodeId),
                            new XElement("STId", timeEmployeeScheduleData.ScheduleTypeId.ToInt()),
                            new XElement("L", (int)timeEmployeeScheduleData.LendedType),
                            new XElement("AccountId", timeEmployeeScheduleData.AccountId ?? 0));

                        for (int i = 1; i <= Constants.NOOFDIMENSIONS; i++)
                        {
                            if (i > accountDimInternals.Count)
                            {
                                schTranElement.Add(new XElement("IId" + i, 0));
                                continue;
                            }

                            AccountInternalDTO accountInternal = null;
                            AccountDim accountDim = accountDimInternals.Any() ? accountDimInternals.ElementAt(i - 1) : null;
                            if (accountDim != null && !timeEmployeeScheduleData.AccountInternals.IsNullOrEmpty())
                            {
                                List<Account> accountsForDim = accountInternals.Where(w => w.AccountDimId == accountDim.AccountDimId).ToList();
                                if (!accountsForDim.Any())
                                    continue;

                                List<int> accountIds = accountsForDim.Select(s => s.AccountId).ToList();

                                accountInternal = (from ai in timeEmployeeScheduleData.AccountInternals
                                                   where accountIds.Contains(ai.AccountId)
                                                   select ai).FirstOrDefault();
                            }

                            if (accountInternal != null)
                                schTranElement.Add(new XElement("IId" + i, accountInternals.FirstOrDefault(f => f.AccountId == accountInternal.AccountId)?.AccountId ?? 0));
                            else
                                schTranElement.Add(new XElement("IId" + i, 0));
                        }

                        schTranElements.Add(schTranElement);
                        scheduleTransactionId++;

                        #endregion
                    });
                });

                foreach (XElement schTranElement in schTranElements)
                {
                    employeeElement.Add(schTranElement);
                }

                employeeScheduleElement.Add(employeeElement);
                employeeXmlId++;
            }

            #region Default element

            if (employeeXmlId == 1)
                AddDefaultElement(reportResult, employeeScheduleElement, "Employee");

            #endregion

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            #region Close document

            rootElement.Add(employeeScheduleElement);
            document.Add(rootElement);

            return GetValidatedDocument(document, reportResult);

            #endregion
        }

        public XDocument CreateEmployeeScheduleCopyData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            if (!TryGetIdFromSelection(reportResult, out int? selectionTimeScheduleCopyHeadId, "scheduleCopyHead"))
                return null;

            if (!TryGetIdsFromSelection(reportResult, out List<int> selectionEmployeeIds, "employees"))
                return null;

            List<Employee> employees;

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Init document

            XDocument document = reportResult.Input.Document;
            XElement rootElement = reportResult.Input.ElementRoot;
            XElement timeEmployeeScheduleElement = reportResult.Input.ElementFirst;

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                const int MAX_SHIFTS = 15;

                // load schedule copy
                var scheduleCopyHead = TimeScheduleManager.GetScheduleCopyHead(reportResult.ActorCompanyId, selectionTimeScheduleCopyHeadId.Value, selectionEmployeeIds);

                if (scheduleCopyHead == null)
                    return null;

                CultureInfo culture = CalendarUtility.GetValidCultureInfo(Constants.SYSLANGUAGE_LANGCODE_DEFAULT);
                List<string> weekNrs = CalendarUtility.GetWeekNrs(scheduleCopyHead.DateFrom, scheduleCopyHead.DateTo, culture);
                List<ShiftType> shiftTypes = TimeScheduleManager.GetShiftTypes(reportResult.Input.ActorCompanyId);
                List<TimeDeviationCause> timeDeviationCauses = base.GetTimeDeviationCausesFromCache(entities, CacheConfig.Company(reportResult.ActorCompanyId));
                List<TimeLeisureCode> timeLeisureCodes = TimeScheduleManager.GetTimeLeisureCodes(reportResult.Input.ActorCompanyId, setTypeNames: true);
                employees = EmployeeManager.GetAllEmployeesByIds(entities, reportResult.Input.ActorCompanyId, selectionEmployeeIds, loadEmployment: true, getHidden: true);

                List<CompanyCategoryRecord> companyCategoryRecords = CategoryManager.GetCompanyCategoryRecords(entities, SoeCategoryType.Employee, SoeCategoryRecordEntity.Employee, reportResult.Input.ActorCompanyId);

                XElement employeeElement = null;
                int employeeXmlId = 1;
                int weekXmlId = 1;

                #endregion

                #region ReportHeaderLabels

                XElement reportHeaderLabelsElement = CreateTimeReportHeaderLabelsElement();
                reportHeaderLabelsElement.Add(CreateDateIntervalLabelReportHeaderLabelsElement());
                this.AddGrossNetCostHeaderPermissionAndSettingReportHeaderLabelElements(reportHeaderLabelsElement);
                this.AddScenarioLabelReportHeaderLabelsElement(reportHeaderLabelsElement);
                this.AddLendedDescriptionLabelsReportHeaderLabelsElement(reportHeaderLabelsElement);
                this.AddPublicationLabelsReportHeaderLabelsElement(reportHeaderLabelsElement);
                timeEmployeeScheduleElement.Add(reportHeaderLabelsElement);

                #endregion

                #region ReportHeader

                XElement reportHeaderElement = CreateTimeReportHeaderElement(reportResult);
                reportHeaderElement.Add(CreateDateIntervalElement(scheduleCopyHead.DateFrom, scheduleCopyHead.DateTo));
                this.AddGrossNetCostHeaderPermissionAndSettingReportHeaderElements(reportHeaderElement, reportResult.Input.RoleId, reportResult.Input.ActorCompanyId);
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                reportHeaderElement.Add(new XElement("CompanyLogo", GetCompanyLogoFilePath(entitiesReadOnly, reportResult.Input.ActorCompanyId, false)));
                reportHeaderElement.Add(new XElement("PublicationDate", scheduleCopyHead.Created?.ToShortDateString() + " " + scheduleCopyHead.Created?.ToShortTimeString()));
                reportHeaderElement.Add(new XElement("PublicationBy", scheduleCopyHead.CreatedBy));
                timeEmployeeScheduleElement.Add(reportHeaderElement);

                #endregion

                #region PageHeaderLabels

                XElement pageHeaderLabelsElement = new XElement("PageHeaderLabels");
                CreateTimeEmployeeSchedulePageHeaderLabelsElement(pageHeaderLabelsElement);
                timeEmployeeScheduleElement.Add(pageHeaderLabelsElement);

                #endregion

                #region Content

                // create employeeLinks to handle multiple rows for same employee when on-duty shifts exist
                List<Tuple<int, List<TimeScheduleCopyRowJsonDataShiftDTO>>> employeeLinks = new List<Tuple<int, List<TimeScheduleCopyRowJsonDataShiftDTO>>>();

                foreach (int employeeId in selectionEmployeeIds)
                {
                    TimeScheduleCopyRowDTO shiftCopyRow = scheduleCopyHead?.Rows?.FirstOrDefault(r => r.EmployeeId == employeeId);
                    TimeScheduleCopyRowJsonDataDTO scheduleDataDTO = shiftCopyRow.JsonData.FromTimeScheduleCopyRowJsonData();

                    employeeLinks.Add(Tuple.Create(employeeId, scheduleDataDTO.Shifts.Where(s => s.Type != TimeScheduleBlockType.OnDuty).ToList()));

                    if (scheduleDataDTO.Shifts.Any(y => y.Type == TimeScheduleBlockType.OnDuty))
                    {
                        employeeLinks.Add(Tuple.Create(employeeId, scheduleDataDTO.Shifts.Where(y => y.Type == TimeScheduleBlockType.OnDuty).ToList()));
                    }
                }


                foreach (var employeeLink in employeeLinks)
                {
                    #region Employee

                    #region Prereq

                    Employee employee = null;
                    List<TimeScheduleCopyRowJsonDataShiftDTO> employeeShifts = employeeLink.Item2;
                    EmployeeGroupDTO employeeGroup = null;

                    employee = employees.FirstOrDefault(i => i.EmployeeId == employeeLink.Item1);
                    if (employee == null)
                        continue;

                    employeeGroup = employee.GetEmployeeGroup(scheduleCopyHead.DateFrom, this.personalDataRepository.EmployeeGroups).ToDTO(false);
                    List<CompanyCategoryRecord> employeeCategoryRecords = companyCategoryRecords.GetCategoryRecords(employee.EmployeeId, scheduleCopyHead.DateFrom, scheduleCopyHead.DateTo);

                    #endregion

                    #region Employee Element

                    string employeeName = employee.Name;

                    if (!employeeShifts.IsNullOrEmpty())
                    {
                        if (employeeShifts.Any(s => s.Type == TimeScheduleBlockType.OnDuty))
                        {
                            employeeName = $"{employeeName} ({GetText(12165, "Jour")})";
                        }
                    }

                    employeeElement = new XElement("Employee",
                        new XAttribute("id", employeeXmlId),
                        new XElement("EmployeeNr", employee.EmployeeNr),
                        new XElement("EmployeeName", employeeName),
                        new XElement("EmployeeCategory", employeeCategoryRecords.Where(i => i.Entity == (int)SoeCategoryType.Employee && i.Default).Select(i => i.Category.Name).ToCommaSeparated()),
                        new XElement("EmployeeGroup", employeeGroup?.Name.NullToEmpty() ?? string.Empty),
                        new XElement("EmployeeGroupRuleWorkTimeWeek", employeeGroup?.RuleWorkTimeWeek ?? 0),
                        new XElement("EmployeeGroupRuleWorkTimeYear", employeeGroup?.RuleWorkTimeYear ?? 0),
                        new XElement("EmployeeGroupRuleRestTimeWeek", employeeGroup?.RuleRestTimeWeek ?? 0),
                        new XElement("EmployeeGroupRuleRestTimeDay", employeeGroup?.RuleRestTimeDay ?? 0));
                    base.personalDataRepository.AddEmployee(employee, employeeElement);

                    #endregion

                    #region Categories Element

                    int categoryXmlId = 1;
                    foreach (CompanyCategoryRecord record in employeeCategoryRecords)
                    {
                        XElement categoryElement = new XElement("Categories",
                            new XAttribute("Id", categoryXmlId),
                            new XElement("CategoryCode", record.Category?.Code.NullToEmpty() ?? string.Empty),
                            new XElement("CategoryName", record.Category?.Name.NullToEmpty() ?? string.Empty),
                            new XElement("isDefaultCategory", record.Default.ToInt()),
                            new XElement("EmployeeNr", employee.EmployeeNr));

                        employeeElement.Add(categoryElement);
                        categoryXmlId++;
                    }

                    if (categoryXmlId == 1)
                        AddDefaultElement(reportResult, employeeElement, "Categories");

                    #endregion

                    #region Build schedule days for employee

                    if (employeeShifts.Any())
                    {
                        #region TimeSchedulePlanningDayDTO

                        foreach (var weekNrGroup in weekNrs.GroupBy(w => w.Split('_')[2]).ToList())
                        {
                            #region Week

                            var weekNr = weekNrGroup.First();
                            var dayElements = new List<XElement>();

                            var aggregatedDays = new List<TimeSchedulePlanningAggregatedDayDTO>();
                            var week = employeeShifts.Where(w => DateTime.Parse(weekNr.Split('_')[2]) == CalendarUtility.GetBeginningOfWeek(w.Date)).ToList();
                            var days = week.GroupBy(i => i.Date).ToList();
                            foreach (var day in days)
                            {
                                var dayShifts = new List<TimeSchedulePlanningDayDTO>();
                                foreach (TimeScheduleCopyRowJsonDataShiftDTO dayShift in day.Where(x => !x.IsBreak))
                                {
                                    TimeSchedulePlanningDayDTO planningDayDTO = TimeScheduleManager.CreateTimeSchedulePlanningDayDTO(dayShift, reportResult.Input.ActorCompanyId, employee.EmployeeId, showDeviationCauseNames: true, shiftTypes: shiftTypes, timeDeviationCauses: timeDeviationCauses);
                                    TimeScheduleManager.AddBreaksToTimeSchedulePlanningDayDTO(day.Where(x => x.IsBreak), null, ref planningDayDTO);
                                    dayShifts.Add(planningDayDTO);
                                }
                                aggregatedDays.Add(new TimeSchedulePlanningAggregatedDayDTO(dayShifts));
                            }


                            int dayXmlId = 1;
                            int employeeWeekScheduleTimeTotal = 0;
                            foreach (var day in aggregatedDays.OrderBy(i => i.StartTime))
                            {
                                #region Day

                                #region Prereq

                                int scheduleTime = Convert.ToInt32(day.ScheduleTime.TotalMinutes) - day.ScheduleBreakTime;
                                int grossTimeMinutesDay = (int)day.GrossTime.TotalMinutes;
                                int netTimeMinutesDay = (int)day.NetTime.TotalMinutes;

                                if (CalendarUtility.GetHoursAndMinutesString(day.ScheduleStartTime) == "00:00" && CalendarUtility.GetHoursAndMinutesString(day.ScheduleStopTime) == "23:59" && day.Description == "zeroDay")
                                {
                                    grossTimeMinutesDay = 0;
                                    netTimeMinutesDay = 0;
                                    scheduleTime = 0;
                                }
                                employeeWeekScheduleTimeTotal += scheduleTime;

                                var shiftElements = new List<XElement>();

                                #endregion

                                #region Day Element

                                XElement dayElement = new XElement("Day",
                                    new XAttribute("id", dayXmlId),
                                    new XElement("IsTemplate", 1),
                                    new XElement("TemplateNbrOfWeeks", day.NbrOfWeeks),
                                    new XElement("TemplateDayNumber", day.DayNumber),
                                    new XElement("DayNr", (int)day.Date.DayOfWeek),
                                    new XElement("IsZeroScheduleDay", day.IsScheduleZeroDay.ToInt()),
                                    new XElement("IsAbsenceDay", day.IsWholeDayAbsence.ToInt()),
                                    new XElement("IsPreliminary", 0),
                                    new XElement("AbsencePayrollProductName", string.Empty),
                                    new XElement("AbsencePayrollProductShortName", string.Empty),
                                    new XElement("ScheduleStartTime", CalendarUtility.GetHoursAndMinutesString(day.ScheduleStartTime)),
                                    new XElement("ScheduleStopTime", CalendarUtility.GetHoursAndMinutesString(day.ScheduleStopTime)),
                                    new XElement("ScheduleTime", scheduleTime),
                                    new XElement("OccupiedTime", 0),
                                    new XElement("ScheduleBreakTime", day.ScheduleBreakTime),
                                    new XElement("ScheduleBreak1Start", CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, day.Break1StartTime)),
                                    new XElement("ScheduleBreak1Minutes", day.Break1Minutes),
                                    new XElement("ScheduleBreak2Start", CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, day.Break2StartTime)),
                                    new XElement("ScheduleBreak2Minutes", day.Break2Minutes),
                                    new XElement("ScheduleBreak3Start", CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, day.Break3StartTime)),
                                    new XElement("ScheduleBreak3Minutes", day.Break3Minutes),
                                    new XElement("ScheduleBreak4Start", CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, day.Break4StartTime)),
                                    new XElement("ScheduleBreak4Minutes", day.Break4Minutes),
                                    new XElement("ScheduleDate", day.Date),
                                    new XElement("ScheduleGrossTimeMinutes", grossTimeMinutesDay),
                                    new XElement("ScheduleNetTimeMinutes", netTimeMinutesDay),
                                    new XElement("ScheduleTotalCost", day.TotalCost));

                                Employment employment = employee.GetEmployment(day.Date);
                                dayElement.Add(
                                    new XElement("EmploymentWorkTimeWeek", employment != null ? employment.GetWorkTimeWeek(day.Date) : 0),
                                    new XElement("EmploymentPercent", employment != null ? employment.GetPercent(day.Date) : 0));

                                #endregion

                                int shiftXmlId = 1;
                                foreach (var dayDTO in day.DayDTOs.OrderBy(st => st.StartTime).ToList())
                                {
                                    #region Prereq

                                    string shiftName = StringUtility.NullToEmpty(dayDTO.ShiftTypeName);
                                    string shiftDescription = StringUtility.NullToEmpty(dayDTO.Description);
                                    if (shiftDescription == string.Empty)
                                        shiftDescription = StringUtility.NullToEmpty(dayDTO.ShiftTypeDesc);
                                    DateTime shiftStartTime = CalendarUtility.GetScheduleTime(dayDTO.StartTime);
                                    DateTime shiftStopTime = CalendarUtility.GetScheduleTime(dayDTO.StopTime);
                                    string color = dayDTO.ShiftTypeColor ?? Constants.SHIFT_TYPE_DEFAULT_COLOR;
                                    int netTimeMinutes = dayDTO.NetTime;
                                    int grossTimeMinutes = dayDTO.GrossTime;
                                    if (grossTimeMinutes == 0 && netTimeMinutes != 0)
                                        grossTimeMinutes = netTimeMinutes;
                                    decimal totalCost = dayDTO.TotalCost;
                                    string link = dayDTO.Link != null ? dayDTO.Link.ToString() : string.Empty;
                                    int lended = 0; // Not needed in Template Schedule?  

                                    TimeDeviationCause timeDeviationCause = dayDTO.TimeDeviationCauseId.HasValue ? TimeDeviationCauseManager.GetTimeDeviationCause(dayDTO.TimeDeviationCauseId.Value, reportResult.Input.ActorCompanyId, false) : null;
                                    string timeDeviationCauseName = timeDeviationCause != null ? timeDeviationCause.Name : string.Empty;

                                    // Set fixed background color if shift is absence
                                    if (timeDeviationCause != null)
                                        color = "#ef545e";   // @shiftAbsenceBackgroundColor

                                    #endregion

                                    #region TimeLeisureCode

                                    if (dayDTO.TimeLeisureCodeId.HasValidValue())
                                    {
                                        // set shift name to time leisure code
                                        shiftName = timeLeisureCodes.FirstOrDefault(x => x.TimeLeisureCodeId == dayDTO.TimeLeisureCodeId).Code;

                                        // set shift to full day for correct filtering of times in rpt template
                                        dayDTO.StartTime = CalendarUtility.GetBeginningOfDay(dayDTO.StartTime);
                                        dayDTO.StopTime = CalendarUtility.GetEndOfDay(dayDTO.StopTime);
                                    }

                                    #endregion

                                    #region Shift/Shifts Element

                                    if (shiftXmlId <= MAX_SHIFTS)
                                    {
                                        dayElement.Add(
                                            new XElement("Shift" + shiftXmlId + "Name", shiftName),
                                            new XElement("Shift" + shiftXmlId + "StartTime", shiftStartTime),
                                            new XElement("Shift" + shiftXmlId + "StopTime", shiftStopTime),
                                            new XElement("Shift" + shiftXmlId + "Description", shiftDescription),
                                            new XElement("Shift" + shiftXmlId + "Color", color),
                                            new XElement("Shift" + shiftXmlId + "GrossTimeMinutes", grossTimeMinutes),
                                            new XElement("Shift" + shiftXmlId + "NetTimeMinutes", netTimeMinutes),
                                            new XElement("Shift" + shiftXmlId + "TotalCost", totalCost),
                                            new XElement("Shift" + shiftXmlId + "TimeDeviationCauseName", timeDeviationCauseName),
                                            new XElement("Shift" + shiftXmlId + "Lended", lended),
                                            new XElement("Shift" + shiftXmlId + "Link", link));
                                    }

                                    shiftElements.Add(new XElement("Shifts",
                                        new XAttribute("id", shiftXmlId),
                                        new XElement("ShiftName", shiftName),
                                        new XElement("ShiftDescription", shiftDescription),
                                        new XElement("ShiftStartTime", shiftStartTime),
                                        new XElement("ShiftStopTime", shiftStopTime),
                                        new XElement("Color", color),
                                        new XElement("ShiftGrossTimeMinutes", grossTimeMinutes),
                                        new XElement("ShiftNetTimeMinutes", netTimeMinutes),
                                        new XElement("ShiftTotalCost", totalCost),
                                        new XElement("ShiftTimeDeviationCauseName", timeDeviationCauseName),
                                        new XElement("ShiftLended", lended)));

                                    shiftXmlId++;

                                    #endregion
                                }

                                #region Default Shift Element

                                //Fill to 10 shifts
                                for (int shiftNr = shiftXmlId; shiftNr <= MAX_SHIFTS; shiftNr++)
                                {
                                    dayElement.Add(
                                        new XElement("Shift" + shiftNr + "Name", string.Empty),
                                        new XElement("Shift" + shiftNr + "StartTime", CalendarUtility.DATETIME_DEFAULT),
                                        new XElement("Shift" + shiftNr + "StopTime", CalendarUtility.DATETIME_DEFAULT),
                                        new XElement("Shift" + shiftNr + "Description", string.Empty),
                                        new XElement("Shift" + shiftNr + "Color", Constants.SHIFT_TYPE_DEFAULT_COLOR),
                                        new XElement("Shift" + shiftNr + "GrossTimeMinutes", 0),
                                        new XElement("Shift" + shiftNr + "NetTimeMinutes", 0),
                                        new XElement("Shift" + shiftNr + "TotalCost", 0),
                                        new XElement("Shift" + shiftNr + "TimeDeviationCauseName", 0),
                                        new XElement("Shift" + shiftNr + "Lended", 0));

                                }

                                //Add all shifts
                                foreach (var shiftElement in shiftElements)
                                {
                                    dayElement.Add(shiftElement);
                                }

                                #endregion

                                dayElements.Add(dayElement);
                                dayXmlId++;

                                #endregion
                            }

                            #region Default Element Day

                            if (dayXmlId == 1) // Do not use default xml since it will add shifts. Old report didn't do that and adding shifts effects reports and also xmlsize
                            {
                                XElement dayElement = new XElement("Day",
                                    new XAttribute("id", 1),
                                    new XElement("DayNr", 0),
                                    new XElement("IsZeroScheduleDay", 0),
                                    new XElement("IsAbsenceDay", 0),
                                    new XElement("IsPreliminary", 0),
                                    new XElement("AbsencePayrollProductName", ""),
                                    new XElement("AbsencePayrollProductShortName", ""),
                                    new XElement("ScheduleStartTime", "00:00"),
                                    new XElement("ScheduleStopTime", "00:00"),
                                    new XElement("ScheduleTime", 0),
                                    new XElement("OccupiedTime", 0),
                                    new XElement("ScheduleBreakTime", 0),
                                    new XElement("ScheduleBreak1Start", CalendarUtility.DATETIME_DEFAULT),
                                    new XElement("ScheduleBreak1Minutes", 0),
                                    new XElement("ScheduleBreak2Start", CalendarUtility.DATETIME_DEFAULT),
                                    new XElement("ScheduleBreak2Minutes", 0),
                                    new XElement("ScheduleBreak3Start", CalendarUtility.DATETIME_DEFAULT),
                                    new XElement("ScheduleBreak3Minutes", 0),
                                    new XElement("ScheduleBreak4Start", CalendarUtility.DATETIME_DEFAULT),
                                    new XElement("ScheduleBreak4Minutes", 0),
                                    new XElement("ScheduleDate", CalendarUtility.DATETIME_DEFAULT));

                                DateTime firstDateOfWeek = Convert.ToDateTime(weekNr.Split('_')[2]);
                                DateTime lastDateOfWeek = CalendarUtility.GetLastDateOfWeek(firstDateOfWeek);

                                Employment employmentFirstDayOfWeek = employee.GetEmployment(firstDateOfWeek);
                                Employment employmentLastDayOfWeek = employee.GetEmployment(lastDateOfWeek);

                                int workTimeFirstDayOfWeek = employmentFirstDayOfWeek != null ? employmentFirstDayOfWeek.GetWorkTimeWeek(firstDateOfWeek) : 0;
                                int workTimeLastDayOfWeek = employmentLastDayOfWeek != null ? employmentLastDayOfWeek.GetWorkTimeWeek(lastDateOfWeek) : 0;

                                decimal percentFirstDayOfWeek = employmentFirstDayOfWeek != null ? employmentFirstDayOfWeek.GetPercent(firstDateOfWeek) : 0;
                                decimal percentLastDayOfWeek = employmentLastDayOfWeek != null ? employmentLastDayOfWeek.GetPercent(lastDateOfWeek) : 0;

                                dayElement.Add(
                                    new XElement("EmploymentWorkTimeWeek", Math.Max(workTimeFirstDayOfWeek, workTimeLastDayOfWeek)),
                                    new XElement("EmploymentPercent", Math.Max(percentFirstDayOfWeek, percentLastDayOfWeek)));

                                dayElements.Add(dayElement);
                            }

                            #endregion

                            #region Week Element

                            var weekElement = new XElement("Week",
                                new XAttribute("id", weekXmlId),
                                new XElement("ScheduleWeekNr", Convert.ToInt32(weekNr.Split('_')[1])),
                                new XElement("EmployeeWeekTimeTotal", employeeWeekScheduleTimeTotal));
                            weekXmlId++;

                            foreach (var day in dayElements)
                            {
                                weekElement.Add(day);
                            }

                            employeeElement.Add(weekElement);

                            #endregion

                            #endregion
                        }

                        #endregion
                    }

                    #endregion

                    timeEmployeeScheduleElement.Add(employeeElement);
                    employeeXmlId++;

                    #endregion
                }

                #endregion

                #region Default Employee Element

                if (employeeXmlId == 1)
                    AddDefaultElement(reportResult, timeEmployeeScheduleElement, "Employee");

                #endregion
            }

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            #region Close document

            rootElement.Add(timeEmployeeScheduleElement);
            document.Add(rootElement);

            return GetValidatedDocument(document, reportResult);

            #endregion
        }

        public XDocument CreateEmployeeLineScheduleData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            if (!TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo))
                return null;
            TryGetIdFromSelection(reportResult, out int? selectionTimeScheduleScenarioHeadId, "timeScheduleScenarioHeadId");
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out List<Employee> employees, out List<int> selectionEmployeeIds, out List<int> selectionAccountIds, out TermGroup_EmployeeSelectionAccountingType selectionAccountingType, selectionTimeScheduleScenarioHeadId))
                return null;
            if (!TryGetIdsFromSelection(reportResult, out List<int> selectionShiftTypeIds, "shiftTypes"))
                return null;

            TryGetBoolFromSelection(reportResult, out bool filterOnAccounting, "filterOnAccounting");
            TryGetBoolFromSelection(reportResult, out bool selectionIncludeAbsence, "includeAbsence");

            TryGetIncludeInactiveFromSelection(reportResult, out _, out _, out bool? selectionActiveEmployees);
            GetEnumFromSelection(reportResult, out TermGroup_TimeSchedulePlanningDayViewGroupBy selectionGroupBy, TermGroup_TimeSchedulePlanningDayViewGroupBy.Employee, key: "groupBy");
            GetEnumFromSelection(reportResult, out TermGroup_TimeSchedulePlanningDayViewSortBy selectionSortBy, TermGroup_TimeSchedulePlanningDayViewSortBy.Firstname, key: "sortBy");

            int employeeAccountDimId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.DefaultEmployeeAccountDimEmployee, 0, reportResult.ActorCompanyId, 0);
            List<Account> selectionAccounts = AccountManager.GetAccounts(GetAccountIdsFromSelection(reportResult), reportResult.Input.ActorCompanyId);
            List<AccountInternalDTO> validAccountInternals = filterOnAccounting && !selectionAccountIds.IsNullOrEmpty() ? AccountManager.GetAccountInternals(reportResult.Input.ActorCompanyId, null).Where(w => selectionAccountIds.Contains(w.AccountId)).ToDTOs() : null;
            TimeScheduleScenarioHead scenarioHead = selectionTimeScheduleScenarioHeadId.HasValue ? TimeScheduleManager.GetTimeScheduleScenarioHead(selectionTimeScheduleScenarioHeadId.Value, reportResult.Input.ActorCompanyId) : null;

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Init document

            XDocument document = reportResult.Input.Document;
            XElement rootElement = reportResult.Input.ElementRoot;
            XElement timeEmployeeLineScheduleElement = reportResult.Input.ElementFirst;

            #endregion

            #region ReportHeaderLabels

            XElement reportHeaderLabelsElement = CreateTimeReportHeaderLabelsElement();
            reportHeaderLabelsElement.Add(CreateDateIntervalLabelReportHeaderLabelsElement());
            this.AddScenarioLabelReportHeaderLabelsElement(reportHeaderLabelsElement);
            timeEmployeeLineScheduleElement.Add(reportHeaderLabelsElement);

            #endregion

            #region ReportHeader

            XElement reportHeaderElement = CreateTimeReportHeaderElement(reportResult);
            reportHeaderElement.Add(CreateDateIntervalElement(selectionDateFrom, selectionDateTo));
            this.AddGrossNetCostHeaderPermissionAndSettingReportHeaderElements(reportHeaderElement, reportResult.Input.RoleId, reportResult.Input.ActorCompanyId);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            reportHeaderElement.Add(new XElement("CompanyLogo", GetCompanyLogoFilePath(entitiesReadOnly, reportResult.Input.ActorCompanyId, false)));
            reportHeaderElement.Add(new XElement("GroupTypeName", ""));
            reportHeaderElement.Add(new XElement("SortTypeName", ""));
            reportHeaderElement.Add(new XElement("AccountsName", selectionAccounts.Where(x => x.AccountDimId == employeeAccountDimId).Select(x => x.Name).JoinToString(", ")));
            this.AddScenarioReportHeaderElements(reportHeaderElement, scenarioHead);

            timeEmployeeLineScheduleElement.Add(reportHeaderElement);

            #endregion

            #region PageHeaderLabels

            XElement pageHeaderLabelsElement = new XElement("PageHeaderLabels");
            CreateTimeEmployeeSchedulePageHeaderLabelsElement(pageHeaderLabelsElement);
            timeEmployeeLineScheduleElement.Add(pageHeaderLabelsElement);

            #endregion

            #region Content

            #region Prereq

            AccountDim accountDimStd = null;
            List<AccountDimSmallDTO> accountDimInternals = new List<AccountDimSmallDTO>();
            List<Account> accountInternals = new List<Account>();
            List<TimeDeviationCause> timeDeviationCauses = new List<TimeDeviationCause>();
            List<ShiftTypeDTO> shiftTypes = new List<ShiftTypeDTO>();
            List<int> validAccountIds = new List<int>();
            Dictionary<int, List<TimeEmployeeScheduleDataSmallDTO>> timeEmployeeScheduleDataSmallDTOs = new Dictionary<int, List<TimeEmployeeScheduleDataSmallDTO>>();
            Dictionary<int, int> shiftTypeDict = new Dictionary<int, int>();
            List<TimeEmployeeScheduleDataSmallDTO> allScheduleDTOs = new List<TimeEmployeeScheduleDataSmallDTO>();

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, reportResult.ActorCompanyId);
            DateTime dateFrom = CalendarUtility.GetBeginningOfDay(selectionDateFrom);
            DateTime dateTo = CalendarUtility.GetEndOfDay(selectionDateTo);

            if (employees == null)
                employees = EmployeeManager.GetAllEmployeesByIds(reportResult.Input.ActorCompanyId, selectionEmployeeIds, active: selectionActiveEmployees, loadEmployment: true, getHidden: true);

            Parallel.Invoke(GetDefaultParallelOptions(), () =>
            {
                timeEmployeeScheduleDataSmallDTOs = TimeScheduleManager.GetTimeEmployeeScheduleSmallDTODictForReport(dateFrom, dateTo, employees.Where(w => selectionEmployeeIds.Contains(w.EmployeeId)).ToList(), reportResult.Input.ActorCompanyId, reportResult.Input.RoleId, selectionShiftTypeIds, splitOnBreaks: false, addAmounts: false, timeScheduleScenarioHeadId: selectionTimeScheduleScenarioHeadId, includeOnDuty: true);
            },
            () =>
            {
                accountDimStd = AccountManager.GetAccountDimStd(reportResult.Input.ActorCompanyId);
                accountDimInternals = AccountManager.GetAccountDimsForPlanning(reportResult.Input.ActorCompanyId, reportResult.Input.UserId, false);
                accountInternals = AccountManager.GetAccountsInternalsByCompany(reportResult.Input.ActorCompanyId);
                timeDeviationCauses = TimeDeviationCauseManager.GetTimeDeviationCauses(reportResult.ActorCompanyId);
                shiftTypes = TimeScheduleManager.GetShiftTypes(reportResult.Input.ActorCompanyId).ToDTOs().ToList();
                validAccountIds = useAccountHierarchy ? AccountManager.GetAccountIdsFromHierarchyByUserSetting(reportResult.ActorCompanyId, reportResult.RoleId, reportResult.UserId, selectionDateFrom, selectionDateTo, null) : new List<int>();

                #region Sales element

                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                timeEmployeeLineScheduleElement.Add(CreateSalesElementFromFrequency(entitiesReadOnly, reportResult.Input.ActorCompanyId, reportResult.Input.RoleId, selectionDateFrom, selectionDateTo, useAccountHierarchy));

                #endregion

                #region ShiftTypes element

                timeEmployeeLineScheduleElement.Add(new XElement("ShiftTypes",
                new XAttribute("id", 0),
                new XElement("ExternalCode", string.Empty),
                new XElement("Name", string.Empty),
                new XElement("Color", string.Empty)));

                int shiftTypeXmlId = 1;
                foreach (var shiftType in shiftTypes)
                {
                    timeEmployeeLineScheduleElement.Add(new XElement("ShiftTypes",
                        new XAttribute("id", shiftTypeXmlId),
                        new XElement("ExternalCode", shiftType.ExternalCode),
                        new XElement("Name", shiftType.Name),
                        new XElement("Color", shiftType.Color)));

                    shiftTypeDict.Add(shiftType.ShiftTypeId, shiftTypeXmlId);
                    shiftTypeXmlId++;
                }

                #endregion
            });

            #endregion

            foreach (var pair in timeEmployeeScheduleDataSmallDTOs)
            {
                allScheduleDTOs.AddRange(pair.Value.Where(x => !x.IsZeroSchedule).ToList());
            }

            #region Filter on selection


            if (filterOnAccounting && selectionAccountingType == TermGroup_EmployeeSelectionAccountingType.TimeScheduleTemplateBlockAccount && !selectionAccountIds.IsNullOrEmpty())
            {
                List<TimeEmployeeScheduleDataSmallDTO> filtered = new List<TimeEmployeeScheduleDataSmallDTO>();

                foreach (var group in allScheduleDTOs.GroupBy(g => $"{g.EmployeeId}#{g.Date}"))
                {
                    foreach (var item in group.ToList())
                    {
                        if (item.AccountInternals != null && item.AccountInternals.ValidOnFiltered(validAccountInternals))
                        {
                            filtered.Add(item);
                            filtered.AddRange(item.GetOverlappedBreaks(group.ToList()));
                        }
                    }
                }

                allScheduleDTOs = filtered.Distinct().ToList();
            }
            else if (filterOnAccounting && selectionAccountingType == TermGroup_EmployeeSelectionAccountingType.TimeScheduleTemplateBlock && !selectionAccountIds.IsNullOrEmpty())
            {
                List<TimeEmployeeScheduleDataSmallDTO> filtered = new List<TimeEmployeeScheduleDataSmallDTO>();

                foreach (var group in allScheduleDTOs.GroupBy(g => $"{g.EmployeeId}#{g.Date}"))
                {
                    foreach (var item in group.Where(w => w.AccountId.HasValue && selectionAccountIds.Contains(w.AccountId.Value)).ToList())
                    {
                        filtered.Add(item);
                        filtered.AddRange(item.GetOverlappedBreaks(group.ToList()));
                    }
                }

                allScheduleDTOs = filtered.Distinct().ToList();
            }
            else if (filterOnAccounting && !selectionAccountIds.IsNullOrEmpty() && !validAccountInternals.IsNullOrEmpty())
            {
                List<TimeEmployeeScheduleDataSmallDTO> filtered = new List<TimeEmployeeScheduleDataSmallDTO>();
                bool validForAccountOnBlockMatch = validAccountInternals.GroupBy(g => g.AccountDimId).Count() == 1;
                foreach (var group in allScheduleDTOs.GroupBy(g => $"{g.EmployeeId}#{g.Date}"))
                {
                    foreach (var item in group.ToList())
                    {
                        if (item.AccountInternals != null && item.AccountInternals.ValidOnFiltered(validAccountInternals))
                        {
                            filtered.Add(item);
                            filtered.AddRange(item.GetOverlappedBreaks(group.ToList()));
                        }
                        else if (validForAccountOnBlockMatch && item.AccountId.HasValue && selectionAccountIds.Contains(item.AccountId.Value))
                        {
                            filtered.Add(item);
                            filtered.AddRange(item.GetOverlappedBreaks(group.ToList()));
                        }
                    }
                }
                allScheduleDTOs = filtered.Distinct().ToList();
            }

            if (!selectionIncludeAbsence)
            {
                List<TimeEmployeeScheduleDataSmallDTO> absenceSchedules = allScheduleDTOs.Where(x => x.TimeDeviationCauseId.HasValue).ToList();
                List<int> absenceBreaksIds = absenceSchedules.GetOverlappedBreaks(allScheduleDTOs.Where(x => x.IsBreak).ToList(), true).Select(x => x.TimeScheduleTemplateBlockId).ToList();

                //remove absence shifts and their breaks from collection
                allScheduleDTOs = allScheduleDTOs.Where(x => !x.TimeDeviationCauseId.HasValue).ToList();
                allScheduleDTOs = allScheduleDTOs.Where(x => !absenceBreaksIds.Contains(x.TimeScheduleTemplateBlockId)).ToList();
            }

            if (selectionShiftTypeIds.Any())
            {
                var shiftTypesShifts = allScheduleDTOs.Where(x => x.ShiftTypeId.HasValue && selectionShiftTypeIds.Contains(x.ShiftTypeId.Value)).ToList();
                var shiftTypeShiftsBreaks = shiftTypesShifts.GetOverlappedBreaks(allScheduleDTOs.Where(x => x.IsBreak).ToList(), true);
                allScheduleDTOs = shiftTypesShifts;
                shiftTypesShifts.AddRange(shiftTypeShiftsBreaks);
            }

            #endregion

            SetGroupingData(allScheduleDTOs, selectionGroupBy, shiftTypes, accountDimInternals, accountInternals);

            //Group by date
            int dayXmlId = 1;
            foreach (var scheduleDTOsByDate in allScheduleDTOs.GroupBy(x => x.Date).OrderBy(x => x.Key))
            {
                #region Day element

                XElement dateElement = new XElement("Day",
                    new XAttribute("id", dayXmlId++),
                    new XElement("Date", scheduleDTOsByDate.Key),
                    new XElement("Name", CalendarUtility.GetDayName(scheduleDTOsByDate.Key, CultureInfo.CurrentCulture)));

                timeEmployeeLineScheduleElement.Add(dateElement);

                #endregion

                int groupXmlId = 1;
                foreach (var scheduleDTOsByGroupName in scheduleDTOsByDate.GroupBy(x => x.GroupName).OrderBy(x => x.Key))
                {
                    #region Group element

                    XElement groupElement = new XElement("Group",
                        new XAttribute("id", groupXmlId++),
                        new XElement("Name", scheduleDTOsByGroupName.Key));

                    dateElement.Add(groupElement);

                    #endregion

                    //Now sort within the group
                    List<EmployeeDTO> sortedEmployees = GetSortedEmployees(selectionSortBy, scheduleDTOsByGroupName.ToList(), employees);

                    //group by employee
                    int employeeXmlId = 1;
                    foreach (var employee in sortedEmployees)
                    {
                        List<TimeEmployeeScheduleDataSmallDTO> employeeSchduleForDateAndGroup = scheduleDTOsByGroupName.Where(x => x.EmployeeId == employee.EmployeeId).ToList();
                        if (!employeeSchduleForDateAndGroup.Any())
                            continue;

                        // Do lookup for employee on OriginalEmployeeId as key since EmployeeId can be a generated number on clones.
                        List<DateTime> employmentDates = employee.Hidden ? CalendarUtility.GetDates(selectionDateFrom, selectionDateTo) : employees.FirstOrDefault(a => a.EmployeeId == employee.OriginalEmployeeId)?.GetEmploymentDates(selectionDateFrom, selectionDateTo);
                        if (employmentDates.IsNullOrEmpty())
                            continue;

                        employeeSchduleForDateAndGroup = employeeSchduleForDateAndGroup.Where(i => employmentDates.Contains(i.Date)).ToList();
                        if (!employeeSchduleForDateAndGroup.Any())
                            continue;

                        #region Employee element

                        XElement employeeElement = new XElement("Employee",
                            new XAttribute("id", employeeXmlId++),
                            new XElement("ENr", employee.EmployeeNr),
                            new XElement("EN", employee.Name));

                        base.personalDataRepository.AddEmployee(employees.FirstOrDefault(a => a.EmployeeId == employee.EmployeeId), employeeElement);
                        groupElement.Add(employeeElement);

                        #endregion

                        #region Shifts element

                        var firstShift = CalendarUtility.DATETIME_DEFAULT;
                        var lastShift = CalendarUtility.DATETIME_DEFAULT;
                        int shiftCounter = 1;
                        int nrOfShifts = 15;
                        int minNrOfShifts = 7;
                        double breakMinutes = 0;
                        double scheduleMinutes = 0;
                        XElement shiftsElement = new XElement("Shifts",
                           new XAttribute("id", 1));

                        List<TimeEmployeeScheduleDataSmallDTO> invalidScheduleDTOs = new List<TimeEmployeeScheduleDataSmallDTO>();

                        foreach (TimeEmployeeScheduleDataSmallDTO scheduleDTO in employeeSchduleForDateAndGroup.Where(i => i.StartTime < i.StopTime).OrderBy(x => x.StartTime))
                        {
                            #region Shift

                            if (useAccountHierarchy && !scheduleDTO.IsBreak && validAccountIds.Any())
                            {
                                AccountInternalDTO accountInternal = scheduleDTO.AccountInternals.FirstOrDefault(f => f.AccountDimId == employeeAccountDimId);
                                if (accountInternal != null && !validAccountIds.Contains(accountInternal.AccountId))
                                {
                                    invalidScheduleDTOs.Add(scheduleDTO);
                                    continue;
                                }
                            }
                            else if (scheduleDTO.IsBreak && useAccountHierarchy && invalidScheduleDTOs.Any(c => c.Date == scheduleDTO.Date && CalendarUtility.GetOverlappingMinutes(scheduleDTO.StartTime, scheduleDTO.StopTime, c.StartTime, c.StopTime) == Convert.ToInt32((scheduleDTO.StopTime - scheduleDTO.StartTime).TotalMinutes)))
                                continue;

                            if (firstShift == CalendarUtility.DATETIME_DEFAULT)
                                firstShift = scheduleDTO.StartTime;

                            if (lastShift < scheduleDTO.StopTime)
                                lastShift = scheduleDTO.StopTime;

                            if (scheduleDTO.IsBreak)
                                breakMinutes += scheduleDTO.Length;
                            else
                                scheduleMinutes += scheduleDTO.Length;

                            int shiftTypeId = scheduleDTO.ShiftTypeId.HasValue ? shiftTypeDict.FirstOrDefault(f => f.Key == scheduleDTO.ShiftTypeId).Value : 0;
                            TimeDeviationCause timeDeviationCause = scheduleDTO.TimeDeviationCauseId.HasValue ? timeDeviationCauses.FirstOrDefault(x => x.TimeDeviationCauseId == scheduleDTO.TimeDeviationCauseId.Value) : null;
                            string timeDeviationCauseName = timeDeviationCause != null ? timeDeviationCause.Name : string.Empty;

                            if (shiftCounter <= nrOfShifts)
                            {
                                shiftsElement.Add(
                                    new XElement("S" + shiftCounter + "Len", scheduleDTO.StopTime.TimeOfDay.TotalMinutes - scheduleDTO.StartTime.TimeOfDay.TotalMinutes),
                                    new XElement("S" + shiftCounter + "Start", scheduleDTO.StartTime.TimeOfDay.TotalMinutes),
                                    new XElement("S" + shiftCounter + "Stop", scheduleDTO.StopTime.TimeOfDay.TotalMinutes),
                                    new XElement("S" + shiftCounter + "STId", shiftTypeId),
                                    new XElement("S" + shiftCounter + "IB", scheduleDTO.IsBreak.ToInt()),
                                    new XElement("S" + shiftCounter + "AbsTxt", timeDeviationCauseName),
                                    new XElement("S" + shiftCounter + "Desc", scheduleDTO.Description),
                                    new XElement("S" + shiftCounter + "Lended", 0)); // TODO: Get value from logic (0=No flag, 1=Lended out, 2=Lended in)

                                shiftCounter++;
                            }

                            #endregion
                        }

                        //Fill to max shifts    
                        for (int shiftNr = shiftCounter; shiftNr <= minNrOfShifts; shiftNr++)
                        {
                            shiftsElement.Add(
                                new XElement("S" + shiftNr + "Len", 0),
                                new XElement("S" + shiftNr + "Start", 0),
                                new XElement("S" + shiftNr + "Stop", 0),
                                new XElement("S" + shiftNr + "STId", 0),
                                new XElement("S" + shiftNr + "IB", 0),
                                new XElement("S" + shiftNr + "AbsTxt", string.Empty),
                                new XElement("S" + shiftNr + "Desc", string.Empty),
                                new XElement("S" + shiftNr + "Lended", 0));
                        }

                        #endregion

                        #region Info element

                        string info = firstShift.ToShortTimeString() + "-" + lastShift.ToShortTimeString() + " " + CalendarUtility.MinutesToTimeSpan((int)(scheduleMinutes - breakMinutes)).ToShortTimeString() + " (" + breakMinutes + ")";
                        employeeElement.Add(new XElement("Info", info));

                        #endregion

                        //Add Shifts
                        employeeElement.Add(shiftsElement);
                    }
                }
            }

            #region Default element

            if (dayXmlId == 1)
                AddDefaultElement(reportResult, timeEmployeeLineScheduleElement, "Day");

            #endregion

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            #region Close document

            rootElement.Add(timeEmployeeLineScheduleElement);
            document.Add(rootElement);

            return GetValidatedDocument(document, reportResult);

            #endregion
        }

        public XDocument CreateReportXML(CompEntities entities, CreateReportResult reportResult, Employee employee = null, List<TimeAccumulator> timeAccumulators = null, TimePeriod currentTimePeriod = null, List<EmployeeGroup> employeeGroups = null, List<PayrollGroup> payrollGroups = null, List<PayrollPriceType> priceTypes = null, Dictionary<int, string> disbursementMethodsDict = null, List<PayrollProduct> companyProducts = null, List<AccountDim> accountDimInternals = null, PaymentInformation paymentInformation = null, List<PayrollStartValueHead> payrollStartValueHeads = null, List<VacationGroup> vacationGroups = null, List<PayrollCalculationProductDTO> payrollCalculationProductItems = null, EmployeeTimePeriod currentEmployeeTimePeriod = null)
        {
            base.reportResult = reportResult;
            XDocument xDoc = CreatePayrollSlipXML(entities, reportResult, employee: employee, timeAccumulators: timeAccumulators, currentTimePeriod: currentTimePeriod, employeeGroups: employeeGroups, payrollGroups: payrollGroups, priceTypes: priceTypes, disbursementMethodsDict: disbursementMethodsDict, companyProducts: companyProducts, accountDimInternals: accountDimInternals, paymentInformation: paymentInformation, payrollStartValueHeads: payrollStartValueHeads, vacationGroups: vacationGroups, isPreliminary: false, payrollCalculationProductItems: payrollCalculationProductItems, currentEmployeeTimePeriod: currentEmployeeTimePeriod);

            return xDoc;
        }

        public List<CreateReportResultOutput> CreatePayrollSlipData(CreateReportResult reportResult, List<PayrollCalculationProductDTO> payrollCalculationProductItems = null)
        {
            #region Prereq

            base.reportResult = reportResult;
            List<CreateReportResultOutput> outputs = new List<CreateReportResultOutput>();

            if (!TryGetIdsFromSelection(reportResult, out List<int> selectionTimePeriodIds, "periods"))
                return null;

            List<TimePeriod> timePeriods = TimePeriodManager.GetTimePeriods(selectionTimePeriodIds, reportResult.Input.ActorCompanyId).Where(o => selectionTimePeriodIds.Contains(o.TimePeriodId)).ToList();

            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionTimePeriodIds, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return null;

            if (employees == null)
                employees = EmployeeManager.GetAllEmployeesByIds(reportResult.ActorCompanyId, selectionEmployeeIds, loadEmployment: true);

            List<TimeAccumulator> timeAccumulators = TimeAccumulatorManager.GetTimeAccumulators(reportResult.Input.ActorCompanyId, loadEmployeeGroupRule: true);

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                foreach (var employee in employees.Where(w => w != null).OrderBy(o => o.EmployeeNrSort))
                {

                    if (!selectionEmployeeIds.Contains(employee.EmployeeId))
                        continue;

                    foreach (var timePeriod in timePeriods.OrderBy(x => x.PaymentDate))
                    {
                        XDocument document = null;

                        if (timePeriod == null)
                            continue;

                        CreateReportResultOutput output = new CreateReportResultOutput();
                        var prelimanary = reportResult.Input.IsPreliminary.HasValue && reportResult.Input.IsPreliminary.Value;

                        if (prelimanary)
                        {
                            document = CreatePayrollSlipXML(entities, reportResult, employee, timeAccumulators, timePeriod, isPreliminary: true, payrollCalculationProductItems: payrollCalculationProductItems, returnNullOnNoRows: false);
                        }
                        else
                        {
                            try
                            {
                                document = GetStoredPayrollSlip(entities, employee, timePeriod, timeAccumulators, payrollCalculationProductItems, out additionalLogInfo, out byte[] data);

                                if (data != null)
                                    output.Pdf = data;
                            }
                            catch (Exception ex)
                            {
                                string message = String.Format("DateStorage and Parse failed additionalinfo:{0}", additionalLogInfo);
                                SoeGeneralException soeEx = new SoeGeneralException(message, ex, this.ToString());
                                base.LogError(soeEx, this.log);
                                continue;
                            }
                        }

                        if (document != null)
                        {
                            document = GetValidatedDocument(document, SoeReportTemplateType.PayrollSlip);

                            if (document != null)
                                output.Document = document;

                            if (!prelimanary && reportResult.ReportSpecial == "SendToKivra")
                            {
                                SendPayrollSlip(entities, reportResult.ActorCompanyId, timePeriod, new List<Employee>() { employee }, report: reportResult.Input.Report, pdf: output.Pdf, document: document, template: reportResult.Template);
                            }

                            outputs.Add(output);
                        }
                    }
                }
            }

            return outputs;
        }

        public XDocument GetStoredPayrollSlip(CompEntities entities, Employee employee, TimePeriod timePeriod, List<TimeAccumulator> timeAccumulators, List<PayrollCalculationProductDTO> payrollCalculationProductItems, out string additionalLogInfo, out byte[] data)
        {
            data = null;
            XDocument document = null;
            int actorCompanyId = employee?.ActorCompanyId ?? 0;
            additionalLogInfo = String.Format("TimePeriodId:{0},EmployeeId:{1},ActorCompanyId:{2}", timePeriod.TimePeriodId, employee.EmployeeId, actorCompanyId);
            var dataStorage = GeneralManager.GetDataStorage(entities, SoeDataStorageRecordType.PayrollSlipXML, timePeriod.TimePeriodId, employee.EmployeeId, actorCompanyId);
            data = dataStorage?.Data;

            additionalLogInfo += ",dataStorageId:" + (dataStorage != null ? dataStorage.DataStorageId.ToString() : "null");
            if (dataStorage == null)
            {
                var employeeTimePeriod = entities.EmployeeTimePeriod.FirstOrDefault(f => f.EmployeeId == employee.EmployeeId && f.TimePeriodId == timePeriod.TimePeriodId);

                if ((employeeTimePeriod != null && employeeTimePeriod.Status == (int)SoeEmployeeTimePeriodStatus.Open) || SettingManager.isTest())
                    document = CreatePayrollSlipXML(entities, reportResult, employee, timeAccumulators, timePeriod, isPreliminary: true, payrollCalculationProductItems: payrollCalculationProductItems, returnNullOnNoRows: true);
            }
            else
                document = XDocument.Parse(dataStorage.XML);

            if (document == null)
                return null;

            document = TryUpdatePayrollSlipXML(document, additionalLogInfo);

            string companyLogoPath = GetCompanyLogoFilePath(entities, actorCompanyId, false);

            if (document != null && !string.IsNullOrEmpty(companyLogoPath))
            {
                try
                {
                    var path = document
                    .Element("SOE_PayrollSlip")
                    .Element("PayrollSlip")
                    .Element("ReportHeader")
                    .Element("CompanyLogo");

                    path.Value = companyLogoPath;
                }
                catch (Exception ex2)
                {
                    SoeGeneralException soeEx = new SoeGeneralException("companyLogoPath failed on payslip", ex2, this.ToString());
                    base.LogError(soeEx, this.log);
                }
            }

            return document;
        }

        public void SendPayrollSlip(CompEntities entities, int actorCompanyId, TimePeriod timePeriod, List<Employee> employees, Report report = null, byte[] pdf = null, XDocument document = null, byte[] template = null)
        {
            CommunicatorConnector communicatorConnector = new CommunicatorConnector();
            string kivraTenentKey = SettingManager.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.KivraTenentKey, 0, actorCompanyId, 0);

            if (!string.IsNullOrEmpty(kivraTenentKey))
            {
                if (pdf != null && employees.Count == 1)
                {
                    var message = KivraUtil.CreateMessage(employees.First(), timePeriod.Name, pdf, kivraTenentKey);

                    if (message != null)
                    {
                        CommunicatorConnector.SendCommunicatorMessage(message);
                        return;
                    }
                }

                report = report ?? ReportManager.GetCompanySettingReport(entities, SettingMainType.Company, CompanySettingType.DefaultPayrollSlipReport, SoeReportTemplateType.PayrollSlip, actorCompanyId, 0, null);

                SoeReportType reportType = SoeReportType.CrystalReport;
                if (template == null)
                {
                    ReportTemplate reportTemplate = ReportManager.GetReportTemplate(entities, report.ReportTemplateId, actorCompanyId);
                    template = reportTemplate?.Template;
                    reportType = reportTemplate != null ? (SoeReportType)reportTemplate.SysReportTypeId : SoeReportType.CrystalReport;
                }

                if (template == null)
                {
                    SysReportTemplate reportTemplate = ReportManager.GetSysReportTemplate(report.ReportTemplateId);
                    template = reportTemplate?.Template;
                    reportType = reportTemplate != null ? (SoeReportType)reportTemplate.SysReportTypeId : SoeReportType.CrystalReport;
                }

                if (template != null)
                {
                    if (document != null && employees.Count == 1)
                    {
                        RptGenConnector connector = RptGenConnector.GetConnector(parameterObject, reportType);
                        RptGenResultDTO crGenResult = connector.GenerateReport(TermGroup_ReportExportType.Pdf, template, document, null, this.CrGenRequestPictures, GetCulture(GetLangId()), ReportGenManager.GetXsdFileString(SoeReportTemplateType.PayrollSlip), $"rr reportid:{report.ReportId} actorcompanyid: {report.ActorCompanyId}");

                        if (crGenResult?.GeneratedReport == null || !crGenResult.Success)
                            return;

                        var message = KivraUtil.CreateMessage(employees.First(), timePeriod.Name, crGenResult?.GeneratedReport, kivraTenentKey);

                        if (message != null)
                        {
                            CommunicatorConnector.SendCommunicatorMessage(message);
                            return;
                        }
                    }

                    if (report != null && !string.IsNullOrEmpty(kivraTenentKey))
                    {

                        foreach (var employee in employees)
                        {
                            RptGenResultDTO crGenResult = CreatePdfPayrollSlip(entities, actorCompanyId, timePeriod, employee, report, template);

                            if (crGenResult.Success && crGenResult.GeneratedReport != null)
                            {
                                var message = KivraUtil.CreateMessage(employee, timePeriod.Name, crGenResult.GeneratedReport, kivraTenentKey);

                                if (message != null)
                                    CommunicatorConnector.SendCommunicatorMessage(message);
                            }
                        }
                    }
                }
            }
        }

        public RptGenResultDTO CreatePdfPayrollSlip(CompEntities entities, int actorCompanyId, TimePeriod timePeriod, Employee employee, Report report = null, byte[] template = null)
        {
            report = report ?? ReportManager.GetCompanySettingReport(entities, SettingMainType.Company, CompanySettingType.DefaultPayrollSlipReport, SoeReportTemplateType.PayrollSlip, actorCompanyId, 0, null);
            CommunicatorConnector communicatorConnector = new CommunicatorConnector();
            string kivraTenentKey = SettingManager.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.KivraTenentKey, 0, actorCompanyId, 0);
            if (report != null)
            {
                var reportTemplate = ReportManager.GetReportTemplate(entities, report.ReportTemplateId, actorCompanyId);
                template = template ?? reportTemplate?.Template;
                var reportType = reportTemplate != null ? (SoeReportType)reportTemplate.SysReportTypeId : SoeReportType.CrystalReport;

                if (template == null)
                {
                    var sysReportTemplate = ReportManager.GetSysReportTemplate(report.ReportTemplateId);
                    template = sysReportTemplate?.Template;
                    reportType = sysReportTemplate != null ? (SoeReportType)sysReportTemplate.SysReportTypeId : SoeReportType.CrystalReport;
                }

                if (template != null)
                {
                    var document = GetStoredPayrollSlip(entities, employee, timePeriod, null, null, out _, out byte[] data);

                    if (data != null)
                    {
                        return new RptGenResultDTO() { GeneratedReport = data, Success = true };
                    }

                    if (document != null)
                    {
                        RptGenConnector connector = RptGenConnector.GetConnector(parameterObject, reportType);
                        RptGenResultDTO crGenResult = connector.GenerateReport(TermGroup_ReportExportType.Pdf, template, document, null, this.CrGenRequestPictures, GetCulture(GetLangId()), ReportGenManager.GetXsdFileString(SoeReportTemplateType.PayrollSlip), $"rr reportid:{report.ReportId} actorcompanyid: {report.ActorCompanyId}");

                        if (crGenResult == null || !crGenResult.Success)
                            return null;

                        return crGenResult;
                    }
                }
            }
            return null;
        }

        public XDocument MergePayrollSlips(List<XDocument> payrollSlipDocuments)
        {
            if (payrollSlipDocuments.Count == 1)
                return payrollSlipDocuments.First();
            try
            {
                var first = payrollSlipDocuments.First();
                var payrollSlipElementOnFirst = XmlUtil.GetChildElement(first, "PayrollSlip");
                XElement employeeElementOnFirst = null;

                if (payrollSlipElementOnFirst != null)
                {
                    var periodElement = XmlUtil.GetChildElementLowerCase(payrollSlipElementOnFirst, "TimePeriod");
                    if (periodElement != null)
                        employeeElementOnFirst = XmlUtil.GetChildElementLowerCase(periodElement, "Employee");
                }

                if (employeeElementOnFirst != null)
                {
                    foreach (var item in payrollSlipDocuments)
                    {
                        if (item == first)
                            continue;

                        var payrollSlipElement = XmlUtil.GetChildElement(item, "PayrollSlip");

                        if (payrollSlipElement != null)
                        {
                            var timePeriodElement = XmlUtil.GetChildElementLowerCase(payrollSlipElement, "TimePeriod");

                            if (timePeriodElement != null)
                            {
                                var employeeElement = XmlUtil.GetChildElementLowerCase(timePeriodElement, "Employee");

                                if (employeeElement != null)
                                    employeeElementOnFirst.AddAfterSelf(employeeElement);
                            }
                        }
                    }

                    return first;
                }
            }
            catch (Exception ex)
            {
                LogError(ex, log);
            }

            return new XDocument();
        }

        public XDocument CreateEmployeeVacationDebtReportData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            TryGetIdFromSelection(reportResult, out int? employeeCalculateVacationResultHeadId, "employeeCalculateVacationResultHeadId");
            TryGetDateFromSelection(reportResult, out DateTime selectionDate, "date");

            if (selectionDate == CalendarUtility.DATETIME_DEFAULT && employeeCalculateVacationResultHeadId.HasValue)
            {
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                EmployeeCalculateVacationResultHead resultHead = entitiesReadOnly.EmployeeCalculateVacationResultHead.FirstOrDefault(f => f.EmployeeCalculateVacationResultHeadId == employeeCalculateVacationResultHeadId);
                if (resultHead != null)
                    selectionDate = resultHead.Date;
            }

            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDate, selectionDate, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return null;

            TryGetIdsFromSelection(reportResult, out List<int> selectionVacationGroupIds, "vacationGroups");
            TryGetBoolFromSelection(reportResult, out bool isFinalSalary, "isFinalSalary");
            TryGetBoolFromSelection(reportResult, out bool includeFinalSalaryProcessed, "includeFinalSalaryProcessed");
            TryGetIdFromSelection(reportResult, out int? timePeriodId, "timePeriodId");
            TryGetBoolFromSelection(reportResult, out bool setAsFinal, "setAsFinal");
            TryGetIncludeInactiveFromSelection(reportResult, out bool _, out _, out bool? selectionActiveEmployees);

            List<AccountDim> accountDims = AccountManager.GetAccountDimsByCompany(reportResult.Input.ActorCompanyId);
            List<AccountDim> accountDimInternals = accountDims.GetInternals();
            AccountDim accountDimStd = accountDims.GetStandard();

            decimal handelsGuaranteeAmount = PayrollManager.GetSysPayrollPriceAmount(reportResult.ActorCompanyId, (int)TermGroup_SysPayrollPrice.SE_Vacation_HandelsGuaranteeAmount, selectionDate);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Init document

            XDocument document = reportResult.Input.Document;
            XElement rootElement = reportResult.Input.ElementRoot;
            XElement employeeVacationDebtReportElement = reportResult.Input.ElementFirst;

            #endregion

            #region ReportHeaderLabels

            XElement reportHeaderLabelsElement = CreateTimeReportHeaderLabelsElement();
            reportHeaderLabelsElement.Add(CreateDateIntervalLabelReportHeaderLabelsElement());
            employeeVacationDebtReportElement.Add(reportHeaderLabelsElement);

            #endregion

            #region ReportHeader

            XElement reportHeaderElement = CreateTimeReportHeaderElement(reportResult);
            reportHeaderElement.Add(CreateDateIntervalElement(selectionDate));

            int maxNumberOfdims = 6;
            int dimCounter = 1;

            while (dimCounter <= maxNumberOfdims)
            {
                if (dimCounter == 1)
                {
                    foreach (var accountDim in accountDims.OrderBy(d => d.AccountDimNr))
                    {
                        if (dimCounter > maxNumberOfdims)
                            break;
                        reportHeaderElement.Add(new XElement("AccountDimNr" + dimCounter + "Name", accountDim.Name));
                        reportHeaderElement.Add(new XElement("AccountDimNr" + dimCounter + "ShortName", accountDim.ShortName));
                        reportHeaderElement.Add(new XElement("AccountDimNr" + dimCounter + "SieDimNr", accountDim.SysSieDimNr));
                        dimCounter++;
                    }
                    continue;
                }

                reportHeaderElement.Add(new XElement("AccountDimNr" + dimCounter + "Name", string.Empty));
                reportHeaderElement.Add(new XElement("AccountDimNr" + dimCounter + "ShortName", string.Empty));
                reportHeaderElement.Add(new XElement("AccountDimNr" + dimCounter + "SieDimNr", string.Empty));

                dimCounter++;
            }

            employeeVacationDebtReportElement.Add(reportHeaderElement);

            #endregion

            #region PageHeaderLabels

            XElement pageHeaderLabelsElement = new XElement("PageHeaderLabels");
            CreateEmployeeVacationSEReportPageHeaderLabelsElement(pageHeaderLabelsElement, Convert.ToInt32(handelsGuaranteeAmount));
            this.AddAccountDimPageHeaderLabelElements(pageHeaderLabelsElement, accountDimStd, accountDimInternals);
            employeeVacationDebtReportElement.Add(pageHeaderLabelsElement);

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                var containers = new List<CalculateVacationResultContainerDTO>();

                #region Prereq

                DateTime currentDate = selectionDate;

                List<EmployeeCalculateVacationResultHead> calculateResultHeads = PayrollManager.GetEmployeeCalculateVacationResultHeads(entities, reportResult.Input.ActorCompanyId)
                    .Where(w => w.Date <= currentDate)
                    .OrderByDescending(o => o.Date)
                    .ThenByDescending(t => t.Created)
                    .ToList();

                List<EmployeeTimePeriod> employeeTimePeriods = TimePeriodManager.GetEmployeeTimePeriodsForVacationDebtReport(entities, selectionEmployeeIds);
                List<PayrollProductSetting> payrollProductSettings = ProductManager.GetPayrollProductsSettings(entities, reportResult.Input.ActorCompanyId);
                List<PayrollStartValueHead> payrollStartValueHeads = PayrollManager.GetPayrollStartValueHeads(entities, reportResult.Input.ActorCompanyId);
                List<CompanyCategoryRecord> companyCategoryRecords = CategoryManager.GetCompanyCategoryRecords(entities, SoeCategoryType.Employee, SoeCategoryRecordEntity.Employee, reportResult.Input.ActorCompanyId);

                ExtensionCache.Instance.AddToEmployeePayrollGroupExtensionCaches(
                    reportResult.Input.ActorCompanyId,
                    base.personalDataRepository.EmployeeGroups,
                    base.personalDataRepository.PayrollGroups,
                    base.personalDataRepository.PayrollPriceTypes,
                    base.personalDataRepository.AnnualLeaveGroups,
                    DateTime.UtcNow.AddSeconds(Math.Min((selectionEmployeeIds?.Count > 50 ? selectionEmployeeIds.Count : 120), 300)));

                bool useExperienceMonthsOnEmploymentAsStartValue = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseEmploymentExperienceAsStartValue, 0, reportResult.Input.ActorCompanyId, 0);

                List<int> filterEmployeeIds = null;
                List<VacationGroup> vacationGroups = null;

                if (!selectionVacationGroupIds.IsNullOrEmpty())
                {
                    filterEmployeeIds = null; //all
                    vacationGroups = PayrollManager.GetVacationGroups(entities, selectionVacationGroupIds);
                }
                else
                {
                    filterEmployeeIds = selectionEmployeeIds;
                    vacationGroups = PayrollManager.GetVacationGroups(entities, reportResult.Input.ActorCompanyId, setTypeName: true, onlyActive: true);
                }

                if (employees == null)
                    employees = EmployeeManager.GetEmployeesForUsersAttestRoles(entities, out _,
                        reportResult.Input.ActorCompanyId,
                        reportResult.Input.UserId,
                        reportResult.Input.RoleId,
                        employeeFilter: filterEmployeeIds,
                        active: selectionActiveEmployees,
                        getHidden: true);

                entities.CommandTimeout = employees.Count > 1000 ? 600 : 300;

                #endregion

                #region Setup Employees and VacationGroups dictionaries

                Dictionary<int, Employment> employeeEmploymentDict = new Dictionary<int, Employment>();
                Dictionary<int, DateTime> employeeCurrentDateDict = new Dictionary<int, DateTime>();
                Dictionary<int, List<Employee>> vacationGroupEmployeesDict = new Dictionary<int, List<Employee>>();

                foreach (Employee employee in employees)
                {
                    if (employeeCurrentDateDict.ContainsKey(employee.EmployeeId))
                        continue;

                    Employment employment = employee.GetEmployment(currentDate.AddYears(-1), currentDate, forward: false);
                    if (employment == null)
                        continue;

                    DateTime employeeCurrentDate = currentDate;
                    DateTime? employeeEndDate = employment.GetEndDate();

                    if (employeeEndDate.HasValue && employeeEndDate <= currentDate)
                    {
                        if (!includeFinalSalaryProcessed && employment.HasAppliedFinalSalaryOrManually() && employeeEndDate < currentDate)
                            continue;

                        //Fix for item 101173
                        if (!timePeriodId.HasValue && !isFinalSalary && employment.FinalSalaryStatus == (int)SoeEmploymentFinalSalaryStatus.ApplyFinalSalary)
                            timePeriodId = TimePeriodManager.GetTimePeriod(entities, reportResult.Input.ActorCompanyId, employeeEndDate.Value)?.TimePeriodId;

                        employeeCurrentDate = employeeEndDate.Value;
                    }

                    employeeCurrentDateDict.Add(employee.EmployeeId, employeeCurrentDate);

                    if (!employment.EmploymentVacationGroup.IsLoaded)
                        employment.EmploymentVacationGroup.Load();
                    EmploymentVacationGroup employmentVacationGroup = employment.GetCurrentEmploymentVacationGroup(employeeCurrentDate);
                    if (employmentVacationGroup == null)
                        continue;

                    VacationGroup vacationGroup = vacationGroups.FirstOrDefault(i => i.VacationGroupId == employmentVacationGroup.VacationGroupId);
                    if (vacationGroup == null)
                        continue;

                    //Save calculated and loaded Employment for later use
                    if (!employeeEmploymentDict.ContainsKey(employee.EmployeeId))
                        employeeEmploymentDict.Add(employee.EmployeeId, employment);

                    //Save relation between Employee and VacationGroup for later use
                    List<Employee> employeesForVacationGroup = null;
                    if (vacationGroupEmployeesDict.ContainsKey(vacationGroup.VacationGroupId))
                    {
                        employeesForVacationGroup = vacationGroupEmployeesDict[vacationGroup.VacationGroupId];
                        if (!employeesForVacationGroup.Any(i => i.EmployeeId == employee.EmployeeId))
                            employeesForVacationGroup.Add(employee);
                    }
                    else
                    {
                        employeesForVacationGroup = employee.ObjToList();
                    }
                    vacationGroupEmployeesDict[vacationGroup.VacationGroupId] = employeesForVacationGroup;
                }

                #endregion

                #region Content

                int vacationGroupXmlId = 1;
                foreach (var vacationGroupEmployees in vacationGroupEmployeesDict)
                {
                    #region Prereq

                    List<Employee> employeesForVacationGroup = vacationGroupEmployees.Value;
                    if (employeesForVacationGroup.IsNullOrEmpty())
                        continue;

                    VacationGroup vacationGroup = vacationGroups.FirstOrDefault(i => i.VacationGroupId == vacationGroupEmployees.Key);
                    if (vacationGroup == null)
                        continue;

                    if (!vacationGroup.VacationGroupSE.IsLoaded)
                        vacationGroup.VacationGroupSE.Load();
                    VacationGroupSE vacationGroupSE = vacationGroup.VacationGroupSE.FirstOrDefault();
                    if (vacationGroupSE == null)
                        continue;

                    var employeeFactorDict = EmployeeManager.GetEmployeesFactors(entities, employeeCurrentDateDict.Keys.ToList())
                        .GroupBy(g => g.EmployeeId)
                        .ToDictionary(k => k.Key, v => v.ToList());

                    var employeeTimePeriodsDict = employeeTimePeriods
                        .GroupBy(g => g.EmployeeId)
                        .ToDictionary(k => k.Key, v => v.ToList());

                    #endregion

                    #region Setup transaction dictionaries

                    var attestPayrollTransactions = new List<AttestPayrollTransactionDTO>();
                    var attestPayrollTransactionsVariable = new List<AttestPayrollTransactionDTO>();
                    var timePayrollTransactionForEmployeeInVacationYear = new List<TimePayrollTransaction>();

                    List<DateTime> employeeDates = employeeCurrentDateDict.GroupBy(k => k.Value).Select(f => f.Key).ToList();
                    foreach (DateTime employeeDate in employeeDates)
                    {
                        List<int> employeeIdsWithDate = employeeCurrentDateDict.Where(e => employeeDate == e.Value).Select(t => t.Key).ToList();
                        if (!employeeIdsWithDate.Any())
                            continue;

                        List<int> employeeIds = employeesForVacationGroup.Where(e => employeeIdsWithDate.Contains(e.EmployeeId)).Select(y => y.EmployeeId).ToList();
                        if (!employeeIds.Any())
                            continue;

                        #region Load earning dates and periods

                        List<TimePeriod> timePeriodsVariable = new List<TimePeriod>();

                        DateTime actualEarningYearAmountFromDate = vacationGroupSE.ActualEarningYearAmountFromDate(employeeDate);
                        vacationGroup.GetActualDates(out DateTime fromDate, out DateTime toDate, out int nbrOfDaysForVacationYear, out int nbrOfDaysForVacationYearToDate, employeeDate);

                        DateTime dictinctEarningDate = CalendarUtility.GetEndOfMonth(employeeDate.AddMonths(actualEarningYearAmountFromDate.Month - fromDate.Month));
                        vacationGroupSE.GetActualEarningYearAmountFromDate(out DateTime earningYearAmountFromDate, out DateTime earningYearAmountToDate, out nbrOfDaysForVacationYear, out nbrOfDaysForVacationYearToDate, dictinctEarningDate);

                        DateTime actualEarningYearVariableAmountFromDate = vacationGroupSE.ActualEarningYearVariableAmountFromDate(employeeDate);
                        if (actualEarningYearVariableAmountFromDate != CalendarUtility.DATETIME_DEFAULT && actualEarningYearVariableAmountFromDate != actualEarningYearAmountFromDate)
                        {
                            DateTime variableEarningDate = CalendarUtility.GetEndOfMonth(actualEarningYearVariableAmountFromDate.AddYears(1).AddDays(-1));

                            vacationGroupSE.GetActualActualEarningYearVariableAmountFromDate(out DateTime earningYearVariableAmountFromDate, out DateTime earningYearVariableAmountToDate, out _, out _, variableEarningDate);
                            timePeriodsVariable = TimePeriodManager.GetVacationTimePeriodsForEmployees(entities, reportResult.Input.ActorCompanyId, employeeIds, earningYearVariableAmountFromDate, earningYearVariableAmountToDate);
                        }

                        List<TimePeriod> timePeriods = TimePeriodManager.GetVacationTimePeriodsForEmployees(entities, reportResult.Input.ActorCompanyId, employeeIds, earningYearAmountFromDate, earningYearAmountToDate, mandatoryTimePeriodId: timePeriodId);
                        if (!timePeriods.Any())
                            continue;

                        List<TimePeriod> allTimePeriods = timePeriods.Concat(timePeriodsVariable)
                            .GroupBy(timePeriod => timePeriod.TimePeriodId)
                            .Select(timePeriod => timePeriod.First())
                            .ToList();

                        #endregion

                        #region Load transactions

                        if (!employeeCalculateVacationResultHeadId.HasValue || employeeCalculateVacationResultHeadId == 0)
                        {
                            var allTransactionInPeriods = AttestManager.GetAttestPayrollTransactionDTOs(
                                entities,
                                reportResult.Input.ActorCompanyId,
                                allTimePeriods,
                                employeesForVacationGroup.Where(e => employeeIds.Contains(e.EmployeeId)).ToList(),
                                applyEmploymentTaxMinimumRule: false,
                                getEmployeeTimePeriodSettings: false,
                                ignoreAccounting: true,
                                employeeGroups: base.personalDataRepository.EmployeeGroups,
                                payrollGroups: base.personalDataRepository.PayrollGroups);

                            if (actualEarningYearVariableAmountFromDate != CalendarUtility.DATETIME_DEFAULT && actualEarningYearVariableAmountFromDate != actualEarningYearAmountFromDate)
                            {
                                var timePeriodIds = timePeriods.Select(s => s.TimePeriodId).ToList();
                                var employeeTimePeriodIds = employeeTimePeriods.Filter(employeeIds, timePeriodIds).ToList();

                                var employeesForTimePeriodsDict = PayrollManager.GetValidEmployeesForTimePeriod(
                                    entities,
                                    base.ActorCompanyId,
                                    timePeriodIds,
                                    employeesForVacationGroup.Where(e => employeeIds.Contains(e.EmployeeId)).ToList(),
                                    base.personalDataRepository.PayrollGroups,
                                    checkEmployeeTimePeriod: true,
                                    employeeTimePeriods: employeeTimePeriodIds
                                );

                                foreach (TimePeriod timePeriod in timePeriods)
                                {
                                    List<int> timePeriodEmployeeIds = employeesForTimePeriodsDict.GetList(timePeriod.TimePeriodId);

                                    foreach (AttestPayrollTransactionDTO transaction in allTransactionInPeriods)
                                    {
                                        if ((transaction.TimePeriodId.HasValue && transaction.TimePeriodId == timePeriod.TimePeriodId) ||
                                            (timePeriodEmployeeIds.Contains(transaction.EmployeeId) && transaction.Date >= timePeriod.StartDate && transaction.Date <= timePeriod.StopDate))
                                            attestPayrollTransactions.Add(transaction);
                                    }
                                }

                                var timePeriodIdsVariable = timePeriodsVariable.Select(s => s.TimePeriodId).ToList();
                                var employeeTimePeriodIdsVariable = employeeTimePeriods.Filter(employeeIds, timePeriodIdsVariable).ToList();

                                var employeesForTimePeriodsVariableDict = timePeriodsVariable.Any()
                                    ? PayrollManager.GetValidEmployeesForTimePeriod(
                                        entities,
                                        base.ActorCompanyId,
                                        timePeriodIdsVariable,
                                        employeesForVacationGroup.Where(e => employeeIds.Contains(e.EmployeeId)).ToList(),
                                        base.personalDataRepository.PayrollGroups,
                                        true,
                                        employeeTimePeriods: employeeTimePeriodIdsVariable
                                        )
                                    : new Dictionary<int, List<int>>();

                                foreach (TimePeriod timePeriod in timePeriodsVariable)
                                {
                                    List<int> timePeriodEmployeeIds = employeesForTimePeriodsVariableDict.GetList(timePeriod.TimePeriodId);

                                    foreach (AttestPayrollTransactionDTO transaction in allTransactionInPeriods)
                                    {
                                        if ((transaction.TimePeriodId.HasValue && transaction.TimePeriodId == timePeriod.TimePeriodId) ||
                                            (timePeriodEmployeeIds.Contains(transaction.EmployeeId) && transaction.Date >= timePeriod.StartDate && transaction.Date <= timePeriod.StopDate))
                                            attestPayrollTransactionsVariable.Add(transaction);
                                    }
                                }
                            }
                            else
                            {
                                attestPayrollTransactions.AddRange(allTransactionInPeriods);
                                attestPayrollTransactionsVariable.AddRange(allTransactionInPeriods);
                            }

                            foreach (Employee employee in employeesForVacationGroup.Where(e => employeeIds.Contains(e.EmployeeId)).ToList())
                            {
                                timePayrollTransactionForEmployeeInVacationYear.AddRange(TimeTransactionManager.GetTimePayrollTransactionsForEmployee(entities, employee.EmployeeId, fromDate, toDate, onlyCurrent: true));
                            }

                            attestPayrollTransactions = attestPayrollTransactions.Distinct().ToList();
                            attestPayrollTransactionsVariable = attestPayrollTransactionsVariable.Distinct().ToList();
                        }

                        #endregion

                        #region Load startValues

                        if (payrollStartValueHeads.Any() && (!employeeCalculateVacationResultHeadId.HasValue || employeeCalculateVacationResultHeadId == 0))
                        {
                            var payrollStartValueHeadIds = payrollStartValueHeads
                                .Where(h => earningYearAmountFromDate < h.DateTo)
                                .Select(x => x.PayrollStartValueHeadId)
                                .ToList();

                            foreach (var employee in employeesForVacationGroup.Where(e => employeeIds.Contains(e.EmployeeId)).ToList())
                            {
                                if (!employee.Employment.IsLoaded)
                                    employee.Employment.Load();

                                List<int> payrollStartValueRowIds = PayrollManager.GetPayrollStartValueRowIdsForVacationDebt(entities, reportResult.Input.ActorCompanyId, employee.EmployeeId, payrollStartValueHeadIds);
                                List<TimePayrollTransaction> payrollStartValueTransactions = PayrollManager.GetTimePayrollTransactionsFromPayrollStartValueRowIds(entities, reportResult.Input.ActorCompanyId, employee.EmployeeId, payrollStartValueRowIds);

                                foreach (TimePayrollTransaction payrollStartValueTransaction in payrollStartValueTransactions)
                                {
                                    if (!timePayrollTransactionForEmployeeInVacationYear.Any(a => a.TimePayrollTransactionId == payrollStartValueTransaction.TimePayrollTransactionId))
                                        timePayrollTransactionForEmployeeInVacationYear.Add(payrollStartValueTransaction);
                                    if (!attestPayrollTransactions.Any(a => a.TimePayrollTransactionId == payrollStartValueTransaction.TimePayrollTransactionId))
                                        attestPayrollTransactions.Add(payrollStartValueTransaction.ToDTO());
                                }
                            }
                        }

                        #endregion
                    }

                    var attestPayrollTransactionsDict = attestPayrollTransactions.GroupBy(g => g.EmployeeId).ToDictionary(k => k.Key, v => v.ToList());
                    var attestPayrollTransactionsVariableDict = attestPayrollTransactionsVariable.GroupBy(g => g.EmployeeId).ToDictionary(k => k.Key, v => v.ToList());
                    var timePayrollTransactionForEmployeeInVacationYearDict = timePayrollTransactionForEmployeeInVacationYear.GroupBy(g => g.EmployeeId).ToDictionary(k => k.Key, v => v.ToList());

                    #endregion

                    int employeeXmlId = 1;
                    var employeeElements = new List<XElement>();
                    var formulaParams = new Dictionary<string, string>();

                    foreach (var employee in employeesForVacationGroup)
                    {
                        #region Prereq

                        if (!employeeEmploymentDict.ContainsKey(employee.EmployeeId))
                            continue;

                        Employment employment = employeeEmploymentDict[employee.EmployeeId];
                        if (employment == null)
                            continue;

                        DateTime employeeCurrentDate = employeeCurrentDateDict.ContainsKey(employee.EmployeeId) ? employeeCurrentDateDict[employee.EmployeeId] : currentDate;
                        EmployeeVacationSE employeeVacationSE = EmployeeManager.GetLatestEmployeeVacationSE(entities, employee.EmployeeId);
                        EmploymentAccountStd employmentAccountStd = EmployeeManager.GetEmploymentAccount(entities, employment.EmploymentId, employment.FixedAccounting ? EmploymentAccountType.Fixed1 : EmploymentAccountType.Cost, currentDate);

                        CalculateVacationResultContainerDTO vacationCalculationContainer;
                        DateTime? employeeVacationCreatedBefore = null;
                        if (employeeCalculateVacationResultHeadId.HasValue && employeeCalculateVacationResultHeadId != 0)
                        {
                            EmployeeCalculateVacationResultHead resultHead = calculateResultHeads.FirstOrDefault(f => f.EmployeeCalculateVacationResultHeadId == employeeCalculateVacationResultHeadId);
                            if (resultHead != null)
                            {
                                vacationCalculationContainer = PayrollManager.CreateVacationCalculationContainer(resultHead, employee.EmployeeId.ObjToList());
                                employeeVacationCreatedBefore = resultHead.Created;
                            }
                            else
                                vacationCalculationContainer = new CalculateVacationResultContainerDTO();
                        }
                        else
                        {
                            vacationCalculationContainer = PayrollManager.CalculateVacationDebt(
                                entities,
                                reportResult.Input.ActorCompanyId,
                                employeeCurrentDate,
                                vacationGroup,
                                vacationGroupSE,
                                employee,
                                employment,
                                employeeGroups: base.personalDataRepository.EmployeeGroups,
                                employeeFactors: employeeFactorDict.GetList(employee.EmployeeId),
                                timePayrollTransactionForEmployeeInVacationYear: timePayrollTransactionForEmployeeInVacationYearDict.GetList(employee.EmployeeId),
                                attestPayrollTransactions: attestPayrollTransactionsDict.GetList(employee.EmployeeId),
                                attestPayrollTransactionsVariable: attestPayrollTransactionsVariableDict.GetList(employee.EmployeeId),
                                payrollProductSettings: payrollProductSettings,
                                isFinalSalary: isFinalSalary,
                                timePeriodId: timePeriodId);
                        }

                        List<EmployeeCalculateVacationResult> employeeResults = PayrollManager.GetEmployeeCalculateVacationResults(entities, employee.EmployeeId, calculateResultHeads);

                        if (setAsFinal)
                            containers.Add(vacationCalculationContainer);

                        #endregion

                        #region Employee element

                        XElement employeeElement = new XElement("Employee",
                            new XAttribute("Id", employeeXmlId),
                            new XElement("EmployeeId", employee.EmployeeId),
                            new XElement("EmployeeNr", employee.EmployeeNr),
                            new XElement("EmployeeName", employee.Name));
                        base.personalDataRepository.AddEmployee(employee, employeeElement);

                        #endregion

                        #region Employment Element

                        XElement employmentElement = new XElement("Employment",
                           new XAttribute("Id", 1),
                           new XElement("EmploymentDateFrom", employment.DateFrom.HasValue ? employment.DateFrom : CalendarUtility.DATETIME_DEFAULT),
                           new XElement("EmploymentDateTo", employment.DateTo.HasValue ? employment.DateTo : CalendarUtility.DATETIME_DEFAULT),
                           new XElement("BaseWorkTimeWeek", employment.GetBaseWorkTimeWeek(employeeCurrentDate)),
                           new XElement("WorkTimeWeek", employment.GetBaseWorkTimeWeek(employeeCurrentDate)),
                           new XElement("WorkPercentage", employment.GetPercent(employeeCurrentDate)),
                           new XElement("ExperienceMonths", EmployeeManager.GetExperienceMonths(entities, reportResult.ActorCompanyId, employment, useExperienceMonthsOnEmploymentAsStartValue, employeeCurrentDate)),
                           new XElement("ExperienceAgreedOrEstablished", employment.GetExperienceAgreedOrEstablished(employeeCurrentDate).ToInt()),
                           new XElement("VacationDaysPayed", employeeVacationSE?.EarnedDaysPaid ?? 0),
                           new XElement("VacationDaysUnpayed", employeeVacationSE?.EarnedDaysUnpaid ?? 0),
                           new XElement("SpecialConditions", employment.GetSpecialConditions(employeeCurrentDate)),
                           new XElement("WorkPlace", employment.GetWorkPlace(employeeCurrentDate)),
                           new XElement("SubstituteFor", employment.GetSubstituteFor(employeeCurrentDate)),
                           new XElement("EndReason", employment.GetEndReason(employeeCurrentDate)),
                           new XElement("EndReasonName", GetText(employment.GetEndReason(employeeCurrentDate), (int)TermGroup.EmploymentEndReason)));

                        List<Account> usedAccounts = new List<Account>();
                        if (employmentAccountStd?.AccountStd?.Account != null)
                            usedAccounts.Add(employmentAccountStd.AccountStd.Account);
                        if (employmentAccountStd?.AccountInternal != null)
                            usedAccounts.AddRange(employmentAccountStd.AccountInternal.Where(a => a.Account != null).Select(a => a.Account));

                        dimCounter = 1;
                        while (dimCounter <= maxNumberOfdims)
                        {
                            if (dimCounter == 1 && usedAccounts.Any())
                            {
                                foreach (AccountDim accountDim in accountDims.OrderBy(d => d.AccountDimNr))
                                {
                                    if (dimCounter <= 6)
                                    {
                                        Account account = usedAccounts.FirstOrDefault(a => a.AccountDimId == accountDim.AccountDimId);
                                        if (account != null)
                                        {
                                            employmentElement.Add(new XElement("AccountDimNr" + dimCounter + "Nr", account.AccountNr));
                                            employmentElement.Add(new XElement("AccountDimNr" + dimCounter + "Name", account.Name));
                                        }
                                        else
                                        {
                                            employmentElement.Add(new XElement("AccountDimNr" + dimCounter + "Nr", string.Empty));
                                            employmentElement.Add(new XElement("AccountDimNr" + dimCounter + "Name", string.Empty));
                                        }
                                    }
                                    dimCounter++;
                                }
                                continue;
                            }

                            employmentElement.Add(new XElement("AccountDimNr" + dimCounter + "Nr", string.Empty));
                            employmentElement.Add(new XElement("AccountDimNr" + dimCounter + "Name", string.Empty));
                            dimCounter++;
                        }

                        employeeElement.Add(employmentElement);

                        #endregion

                        #region FormulaSum Element

                        decimal? formulaSum_AD = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("ad")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_ARB = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("arb")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_ARBP = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("arbp")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_ARBT = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("arbt")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_BARB = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("barb")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_BV = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("bv")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_ESGF = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("esgf")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_ESGFKH = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("esgfkh")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_GB = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("gb")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_GBU = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("gbu")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_GS = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("gs")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_ML = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("ml")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_NF = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("nf")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_SB = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("sb")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_SB_NETTO = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("sb_netto")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_SBU = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("sbu")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_SBURT = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("sburt")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_SBUT = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("sbut")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_SEMOP = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("semop")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_SEMP = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("semp")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_SEMPU = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("sempu")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_SEMROP = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("semrop")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_SEMRP = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("semrp")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_SEMTP = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("semtp")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_SEMTOP = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("semtop")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_SF = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("sf")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_SGD = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("sgd")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_SGF = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("sgf")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_SGFD = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("sgfd")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_SGFDH = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("sgfdh")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_SGFV = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("sgfv")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_SGL = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("sgl")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_SGR = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("sgr")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_SGT = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("sgt")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_SL = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("sl")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_SLB = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("slb")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_SLD = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("sld")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_SLDB = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("sldb")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_SLDS1 = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("slds1")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_SLDS2 = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("slds2")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_SLDS3 = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("slds3")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_SLDS4 = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("slds4")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_SLDS5 = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("slds5")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_SLDSF = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("sldsf")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_SLDU = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("sldu")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_SN = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("sn")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_SO = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("so")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_SR = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("sr")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_SSG = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("ssg")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_SSGA = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("ssga")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_SSGB = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("ssgb")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_ST = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("st")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_STR = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("str")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_STRD = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("strd")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_TL = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("tl")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_ABD = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("abd")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_ASD = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("asd")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_VBD = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("vbd")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_VSD = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("vsd")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_VID = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("vid")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_VBSTR = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("vbstr")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_BSTRA = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("bstra")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_VSSTR = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("vsstr")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_SSTRA = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("sstra")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_VISTR = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("vistr")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_VFD = vacationCalculationContainer.FormulaResult.Where(f => f.Name.ToLower().Equals("vfd")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_TABD = employeeResults.Where(f => f.Name.ToLower().Equals("abd")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_TASD = employeeResults.Where(f => f.Name.ToLower().Equals("asd")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_TVBD = employeeResults.Where(f => f.Name.ToLower().Equals("vbd")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_TVSD = employeeResults.Where(f => f.Name.ToLower().Equals("vsd")).ToList()?.Sum(l => l.Value);
                        decimal? formulaSum_TVID = employeeResults.Where(f => f.Name.ToLower().Equals("vid")).ToList()?.Sum(l => l.Value);

                        XElement formulaSumElement = new XElement("FormulaSum",
                            new XAttribute("Id", 1));
                        formulaSumElement.Add(new XElement("FormulaSum_AD", formulaSum_AD ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_ARB", formulaSum_ARB ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_ARBP", formulaSum_ARBP ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_ARBT", formulaSum_ARBT ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_BARB", formulaSum_BARB ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_BV", formulaSum_BV ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_ESGF", formulaSum_ESGF ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_ESGFKH", formulaSum_ESGFKH ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_GB", formulaSum_GB ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_GBU", formulaSum_GBU ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_GS", formulaSum_GS ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_ML", formulaSum_ML ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_NF", formulaSum_NF ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_SB", formulaSum_SB ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_SB_NETTO", formulaSum_SB_NETTO ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_SBU", formulaSum_SBU ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_SBURT", formulaSum_SBURT ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_SBUT", formulaSum_SBUT ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_SEMOP", formulaSum_SEMOP ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_SEMP", formulaSum_SEMP ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_SEMPU", formulaSum_SEMPU ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_SEMROP", formulaSum_SEMROP ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_SEMRP", formulaSum_SEMRP ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_SEMTP", formulaSum_SEMTP ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_SEMTOP", formulaSum_SEMTOP ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_SF", formulaSum_SF ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_SGD", formulaSum_SGD ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_SGF", formulaSum_SGF ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_SGFD", formulaSum_SGFD ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_SGFDH", formulaSum_SGFDH ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_SGFV", formulaSum_SGFV ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_SGL", formulaSum_SGL ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_SGR", formulaSum_SGR ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_SGT", formulaSum_SGT ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_SL", formulaSum_SL ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_SLB", formulaSum_SLB ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_SLD", formulaSum_SLD.HasValue ? Decimal.Round(formulaSum_SLD.Value, 4) : 0));
                        formulaSumElement.Add(new XElement("FormulaSum_SLDB", formulaSum_SLDB.HasValue ? Decimal.Round(formulaSum_SLDB.Value, 4) : 0));
                        formulaSumElement.Add(new XElement("FormulaSum_SLDS1", formulaSum_SLDS1.HasValue ? Decimal.Round(formulaSum_SLDS1.Value, 4) : 0));
                        formulaSumElement.Add(new XElement("FormulaSum_SLDS2", formulaSum_SLDS2.HasValue ? Decimal.Round(formulaSum_SLDS2.Value, 4) : 0));
                        formulaSumElement.Add(new XElement("FormulaSum_SLDS3", formulaSum_SLDS3.HasValue ? Decimal.Round(formulaSum_SLDS3.Value, 4) : 0));
                        formulaSumElement.Add(new XElement("FormulaSum_SLDS4", formulaSum_SLDS4.HasValue ? Decimal.Round(formulaSum_SLDS4.Value, 4) : 0));
                        formulaSumElement.Add(new XElement("FormulaSum_SLDS5", formulaSum_SLDS5.HasValue ? Decimal.Round(formulaSum_SLDS5.Value, 4) : 0));
                        formulaSumElement.Add(new XElement("FormulaSum_SLDSF", formulaSum_SLDSF.HasValue ? Decimal.Round(formulaSum_SLDSF.Value, 4) : 0));
                        formulaSumElement.Add(new XElement("FormulaSum_SLDU", formulaSum_SLDU.HasValue ? Decimal.Round(formulaSum_SLDU.Value, 4) : 0));
                        formulaSumElement.Add(new XElement("FormulaSum_SN", formulaSum_SN ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_SO", formulaSum_SO ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_SR", formulaSum_SR ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_SSG", formulaSum_SSG ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_SSGA", formulaSum_SSGA ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_SSGB", formulaSum_SSGB ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_ST", formulaSum_ST ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_STR", formulaSum_STR ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_STRD", formulaSum_STRD ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_TL", formulaSum_TL ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_ABD", formulaSum_ABD ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_ASD", formulaSum_ASD ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_VBD", formulaSum_VBD ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_VSD", formulaSum_VSD ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_VID", formulaSum_VID ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_VBSTR", formulaSum_VBSTR ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_BSTRA", formulaSum_BSTRA ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_VSSTR", formulaSum_VSSTR ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_SSTRA", formulaSum_SSTRA ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_VISTR", formulaSum_VISTR ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_VFD", formulaSum_VFD ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_TABD", formulaSum_TABD ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_TASD", formulaSum_TASD ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_TVBD", formulaSum_TVBD ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_TVSD", formulaSum_TVSD ?? 0));
                        formulaSumElement.Add(new XElement("FormulaSum_TVID", formulaSum_TVID ?? 0));

                        employeeElement.Add(formulaSumElement);

                        #endregion

                        #region EmployeeVacationSE Element

                        List<EmployeeTimePeriod> employeeTimePeriodsForEmployee = employeeTimePeriodsDict.GetList(employee.EmployeeId);
                        AddEmployeeVacationSEElement(employeeElement, employee.EmployeeId, reportResult.Input.ActorCompanyId, true, employeeVacationCreatedBefore, employeeVacationSE: employeeVacationSE, employee: employee, employeeTimePeriods: employeeTimePeriodsForEmployee, selectionDate: selectionDate);

                        #endregion

                        #region EmployeeFactor Element

                        if (!employee.EmployeeFactor.IsLoaded)
                            employee.EmployeeFactor.Load();

                        int employeeFactorXmlId = 1;
                        foreach (EmployeeFactor employeeFactor in employee.EmployeeFactor.Where(i => i.State == (int)SoeEntityState.Active))
                        {
                            XElement employeeFactorElement = new XElement("EmployeeFactor",
                                new XAttribute("Id", employeeFactorXmlId),
                                new XElement("Type", employeeFactor.Type),
                                new XElement("TypeName", GetText(employeeFactor.Type, (int)TermGroup.EmployeeFactorType)),
                                new XElement("FromDate", employeeFactor.FromDate),
                                new XElement("Factor", employeeFactor.Factor));

                            employeeElement.Add(employeeFactorElement);
                            employeeFactorXmlId++;
                        }

                        #endregion

                        #region Categories Element

                        int categoryXmlId = 1;
                        List<CompanyCategoryRecord> employeeCategoryRecords = companyCategoryRecords.GetCategoryRecords(employee.EmployeeId, selectionDate, selectionDate, onlyDefaultCategories: false);
                        foreach (CompanyCategoryRecord record in employeeCategoryRecords)
                        {
                            XElement categoryElement = new XElement("Categories",
                                new XAttribute("Id", categoryXmlId),
                                new XElement("CategoryCode", record.Category != null ? StringUtility.NullToEmpty(record.Category.Code) : string.Empty),
                                new XElement("CategoryName", record.Category != null ? StringUtility.NullToEmpty(record.Category.Name) : string.Empty),
                                new XElement("CategoryIsDefault", record.Default.ToInt()),
                                new XElement("CategoryDateFrom", CalendarUtility.DATETIME_DEFAULT),
                                new XElement("CategoryDateTo", CalendarUtility.DATETIME_DEFAULT));

                            employeeElement.Add(categoryElement);
                            categoryXmlId++;
                        }

                        if (categoryXmlId == 1)
                        {
                            XElement categoryElement = new XElement("Categories",
                                new XAttribute("Id", 0));
                            employeeElement.Add(categoryElement);
                        }

                        #endregion

                        #region Accounts Element

                        AddEmployeeAccountsElement(employeeElement, employee, currentDate);

                        #endregion

                        #region Formula Element

                        int formulaResultXmlId = 1;
                        if (vacationCalculationContainer.FormulaResult != null)
                        {
                            foreach (var formulaResult in vacationCalculationContainer.FormulaResult)
                            {
                                XElement formulaElement = new XElement("Formula",
                                    new XAttribute("Id", formulaResultXmlId),
                                    new XElement("FormulaName", formulaResult.Name),
                                    new XElement("FormulaPlain", formulaResult.FormulaPlain),
                                    new XElement("FormulaExtracted", formulaResult.FormulaExtracted),
                                    new XElement("FormulaNames", formulaResult.FormulaNames),
                                    new XElement("FormulaValue", formulaResult.Value),
                                    new XElement("FormulaError", formulaResult.Error)
                                    );

                                if (!string.IsNullOrEmpty(formulaResult.FormulaOrigin))
                                    formulaElement.Add(new XElement("FormulaOrigin", formulaResult.FormulaOrigin));

                                employeeElement.Add(formulaElement);
                                formulaResultXmlId++;
                            }
                        }
                        if (formulaResultXmlId == 1)
                        {
                            XElement formulaElement = new XElement("Formula",
                                new XAttribute("Id", 0));
                            employeeElement.Add(formulaElement);
                        }

                        if (vacationCalculationContainer.FormulasUsed != null)
                        {
                            foreach (var formulaPair in vacationCalculationContainer.FormulasUsed)
                            {
                                if (!formulaParams.ContainsKey(formulaPair.Key))
                                    formulaParams.Add(formulaPair.Key, formulaPair.Value);
                            }
                        }

                        #endregion

                        employeeElements.Add(employeeElement);
                        employeeXmlId++;
                    }

                    #region VacationGroup Element

                    decimal vacationDayPercent = 0;
                    if (vacationGroupSE.VacationDayPercentPriceTypeId.HasValue && vacationGroupSE.VacationDayPercentPriceTypeId != 0)
                    {
                        PayrollPriceType pricetype = base.personalDataRepository.PayrollPriceTypes.GetPayrollPriceType(vacationGroupSE.VacationDayPercentPriceTypeId.Value);
                        if (pricetype != null)
                            vacationDayPercent = pricetype.GetPeriod(currentDate)?.Amount ?? 0;
                    }
                    else
                    {
                        vacationDayPercent = vacationGroupSE.VacationDayPercent ?? 0;
                    }

                    decimal vacationDayAdditionPercent = 0;
                    if (vacationGroupSE.VacationDayAdditionPercentPriceTypeId.HasValue && vacationGroupSE.VacationDayAdditionPercentPriceTypeId != 0)
                    {
                        PayrollPriceType pricetype = base.personalDataRepository.PayrollPriceTypes.GetPayrollPriceType(vacationGroupSE.VacationDayAdditionPercentPriceTypeId.Value);
                        if (pricetype != null)
                            vacationDayAdditionPercent = pricetype.GetPeriod(currentDate)?.Amount ?? 0;
                    }
                    else
                    {
                        vacationDayAdditionPercent = vacationGroupSE.VacationDayAdditionPercent ?? 0;
                    }

                    decimal vacationVariablePercent = 0;
                    if (vacationGroupSE.VacationVariablePercentPriceTypeId.HasValue && vacationGroupSE.VacationVariablePercentPriceTypeId != 0)
                    {
                        PayrollPriceType pricetype = base.personalDataRepository.PayrollPriceTypes.GetPayrollPriceType(vacationGroupSE.VacationVariablePercentPriceTypeId.Value);
                        if (pricetype != null)
                            vacationVariablePercent = pricetype.GetPeriod(currentDate)?.Amount ?? 0;
                    }
                    else
                    {
                        vacationVariablePercent = vacationGroupSE.VacationVariablePercent ?? 0;
                    }

                    XElement vacationGroupElement = new XElement("VacationGroup",
                        new XAttribute("Id", vacationGroupXmlId),
                        new XElement("VacationGroupId", vacationGroup.VacationGroupId),
                        new XElement("VacationGroupName", vacationGroup.Name),
                        new XElement("VacationGroupType", vacationGroup.Type),
                        new XElement("VacationGroupTypeName", GetText(vacationGroup.Type, (int)TermGroup.VacationGroupType)),
                        new XElement("VacationGroupFromDate", vacationGroup.FromDate),
                        new XElement("VacationDaysPaidByLaw", vacationGroup.VacationDaysPaidByLaw ?? 0),
                        new XElement("VacationDayCalculationType", vacationGroupSE.CalculationType),
                        new XElement("UseAdditionalVacationDays", vacationGroupSE.UseAdditionalVacationDays.ToInt()),
                        new XElement("NbrOfAdditionalVacationDays", vacationGroupSE.NbrOfAdditionalVacationDays),
                        new XElement("AdditionalVacationDaysFromAge1", vacationGroupSE.AdditionalVacationDaysFromAge1 ?? 0),
                        new XElement("AdditionalVacationDays1", vacationGroupSE.AdditionalVacationDays1 ?? 0),
                        new XElement("AdditionalVacationDaysFromAge2", vacationGroupSE.AdditionalVacationDaysFromAge2 ?? 0),
                        new XElement("AdditionalVacationDays2", vacationGroupSE.AdditionalVacationDays2 ?? 0),
                        new XElement("AdditionalVacationDaysFromAge3", vacationGroupSE.AdditionalVacationDaysFromAge3 ?? 0),
                        new XElement("AdditionalVacationDays3", vacationGroupSE.AdditionalVacationDays3 ?? 0),
                        new XElement("VacationHandleRule", vacationGroupSE.VacationHandleRule),
                        new XElement("VacationDaysHandleRule", vacationGroupSE.VacationDaysHandleRule),
                        new XElement("VacationDaysGrossUseFiveDaysPerWeek", vacationGroupSE.VacationDaysGrossUseFiveDaysPerWeek.ToInt()),
                        new XElement("RemainingDaysRule", vacationGroupSE.RemainingDaysRule),
                        new XElement("UseMaxRemainingDays", vacationGroupSE.UseMaxRemainingDays.ToInt()),
                        new XElement("MaxRemainingDays", vacationGroupSE.MaxRemainingDays ?? 0),
                        new XElement("RemainingDaysPayoutMonth", vacationGroupSE.RemainingDaysPayoutMonth ?? 0),
                        new XElement("EarningYearAmountFromDate", vacationGroupSE.EarningYearAmountFromDate),
                        new XElement("EarningYearVariableAmountFromDate", vacationGroupSE.EarningYearVariableAmountFromDate.ToValueOrDefault()),
                        new XElement("VacationDayPercent", vacationDayPercent),
                        new XElement("VacationDayAdditionPercent", vacationDayAdditionPercent),
                        new XElement("VacationVariablePercent", vacationVariablePercent),
                        new XElement("UseGuaranteeAmount", vacationGroupSE.UseGuaranteeAmount.ToInt()),
                        new XElement("GuaranteeAmountAccordingToHandels", vacationGroupSE.GuaranteeAmountAccordingToHandels.ToInt()),
                        new XElement("GuaranteeAmountMaxNbrOfDaysRule", vacationGroupSE.GuaranteeAmountMaxNbrOfDaysRule),
                        new XElement("GuaranteeAmountEmployedNbrOfYears", vacationGroupSE.GuaranteeAmountEmployedNbrOfYears ?? 0),
                        new XElement("GuaranteeAmountJuvenile", vacationGroupSE.GuaranteeAmountJuvenile.ToInt()),
                        new XElement("GuaranteeAmountJuvenileAgeLimit", vacationGroupSE.GuaranteeAmountJuvenileAgeLimit ?? 0),
                        new XElement("VacationAbsenceCalculationRule", vacationGroupSE.VacationAbsenceCalculationRule),
                        new XElement("VacationSalaryPayoutRule", vacationGroupSE.VacationSalaryPayoutRule),
                        new XElement("VacationSalaryPayoutDays", vacationGroupSE.VacationSalaryPayoutDays ?? 0),
                        new XElement("VacationSalaryPayoutMonth", vacationGroupSE.VacationSalaryPayoutMonth ?? 0),
                        new XElement("YearEndRemainingDaysRule", vacationGroupSE.YearEndRemainingDaysRule),
                        new XElement("YearEndOverdueDaysRule", vacationGroupSE.YearEndOverdueDaysRule),
                        new XElement("YearEndVacationVariableRule", vacationGroupSE.YearEndVacationVariableRule));

                    #endregion

                    #region FormulaParam Element

                    int formulaParamsUsedXmlId = 1;
                    foreach (var formulaParamPair in formulaParams)
                    {
                        XElement formulaUsedElement = new XElement("FormulaParam",
                            new XAttribute("Id", formulaParamsUsedXmlId),
                            new XElement("FormulaParamCode", formulaParamPair.Key),
                            new XElement("FormulaParamName", formulaParamPair.Value));

                        vacationGroupElement.Add(formulaUsedElement);
                        formulaParamsUsedXmlId++;
                    }

                    if (formulaParamsUsedXmlId == 1)
                    {
                        XElement formulaElement = new XElement("FormulaParam",
                            new XAttribute("Id", 0));
                        vacationGroupElement.Add(formulaElement);
                    }

                    foreach (XElement employeeElement in employeeElements)
                    {
                        vacationGroupElement.Add(employeeElement);
                    }

                    #endregion

                    employeeVacationDebtReportElement.Add(vacationGroupElement);
                    vacationGroupXmlId++;
                }

                #region Default element

                if (vacationGroupXmlId == 1)
                    AddDefaultElement(reportResult, employeeVacationDebtReportElement, "VacationGroup");

                #endregion

                #endregion

                #region Close repository

                base.personalDataRepository.GenerateLogs();

                #endregion

                #region Close document

                rootElement.Add(employeeVacationDebtReportElement);
                document.Add(rootElement);

                var doc = GetValidatedDocument(document, reportResult);

                if (doc != null && setAsFinal)
                    PayrollManager.SaveEmployeeCalculationResult(entities, reportResult.Input.ActorCompanyId, containers, currentDate);

                return doc;
            }

            #endregion
        }

        public XDocument CreateEmployeeTimePeriodReportData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            if (!TryGetIdsFromSelection(reportResult, out List<int> selectionTimePeriodIds, "periods"))
                return null;

            List<TimePeriod> timePeriods = TimePeriodManager.GetTimePeriods(selectionTimePeriodIds, reportResult.ActorCompanyId).Where(o => selectionTimePeriodIds.Contains(o.TimePeriodId)).ToList();
            DateTime selectionDateFrom = timePeriods.Select(t => t.StartDate).Min();
            DateTime selectionDateTo = timePeriods.Select(t => t.StopDate).Max();

            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionTimePeriodIds, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return null;

            if (employees == null)
                employees = EmployeeManager.GetAllEmployeesByIds(reportResult.ActorCompanyId, selectionEmployeeIds, loadEmployment: true);

            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            List<EmployeeTimePeriod> allEmployeeTimePeriods = TimePeriodManager.GetEmployeeTimePeriodsWithValuesAndTimePeriod(entitiesReadOnly, selectionTimePeriodIds, selectionEmployeeIds, reportResult.ActorCompanyId);
            List<TimeSalaryPaymentExport> allTimeSalaryPaymentExports = TimeSalaryManager.GetTimeSalaryPaymentExports(selectionTimePeriodIds, reportResult.ActorCompanyId, true);
            bool showSocialSec = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Init document

            XDocument document = reportResult.Input.Document;
            XElement rootElement = reportResult.Input.ElementRoot;
            XElement employeeTimePeriodReportElement = reportResult.Input.ElementFirst;

            #endregion

            #region ReportHeaderLabels

            XElement reportHeaderLabelsElement = CreateTimeReportHeaderLabelsElement();
            reportHeaderLabelsElement.Add(CreateDateIntervalLabelReportHeaderLabelsElement());
            employeeTimePeriodReportElement.Add(reportHeaderLabelsElement);

            #endregion

            #region ReportHeader

            XElement reportHeaderElement = CreateTimeReportHeaderElement(reportResult);
            reportHeaderElement.Add(CreateDateIntervalElement(selectionDateFrom, selectionDateTo));
            reportHeaderElement.Add(new XElement("CompanyLogo", GetCompanyLogoFilePath(entitiesReadOnly, reportResult.ActorCompanyId, false)));
            employeeTimePeriodReportElement.Add(reportHeaderElement);

            #endregion

            #region Content

            int employeeXmlId = 0;
            foreach (int employeeId in selectionEmployeeIds)
            {
                #region Prereq

                DateTime startDate = selectionDateTo;
                DateTime stopDate = selectionDateFrom;

                Employee employee = employees?.GetEmployee(employeeId);
                if (employee == null)
                    continue;

                List<Employment> employments = employee.GetEmployments(startDate, stopDate);
                if (employments == null)
                    continue;

                #endregion

                #region Check if loaded

                //Make sure ContactPerson is loaded
                if (!employee.ContactPersonReference.IsLoaded)
                    employee.ContactPersonReference.Load();

                #endregion

                #region Employee element

                XElement employeeElement = new XElement("Employee",
                    new XAttribute("Id", employeeXmlId),
                    new XElement("EmployeeNr", employee.EmployeeNr),
                    new XElement("FirstName", employee.ContactPerson != null ? employee.ContactPerson.FirstName : string.Empty),
                    new XElement("LastName", employee.ContactPerson != null ? employee.ContactPerson.LastName : string.Empty),
                    new XElement("SocialSec", showSocialSec ? employee.SocialSec : StringUtility.SocialSecYYMMDD_Dash_Stars(employee.SocialSec)),
                    new XElement("Note", employee.Note));
                base.personalDataRepository.AddEmployee(employee, employeeElement);

                #endregion

                #region EmployeeSummery

                decimal period0SumTypeNone = 0;
                decimal period0SumTypeTableTax = 0;
                decimal period0SumTypeOneTimeTax = 0;
                decimal period0SumTypeSINKTax = 0;
                decimal period0SumTypeASINKTax = 0;
                decimal period0SumTypeEmploymentTaxDebit = 0;
                decimal period0SumTypeSupplementChargeDebit = 0;
                decimal period0SumTypeGrossSalary = 0;
                decimal period0SumTypeNetSalary = 0;
                decimal period0SumTypeVacationCompensation = 0;
                decimal period0SumTypeBenefit = 0;
                decimal period0SumTypeCompensation = 0;
                decimal period0SumTypeDeduction = 0;
                decimal period0SumTypeUnionFee = 0;
                decimal period1SumTypeNone = 0;
                decimal period1SumTypeTableTax = 0;
                decimal period1SumTypeOneTimeTax = 0;
                decimal period1SumTypeSINKTax = 0;
                decimal period1SumTypeASINKTax = 0;
                decimal period1SumTypeEmploymentTaxDebit = 0;
                decimal period1SumTypeSupplementChargeDebit = 0;
                decimal period1SumTypeGrossSalary = 0;
                decimal period1SumTypeNetSalary = 0;
                decimal period1SumTypeVacationCompensation = 0;
                decimal period1SumTypeBenefit = 0;
                decimal period1SumTypeCompensation = 0;
                decimal period1SumTypeDeduction = 0;
                decimal period1SumTypeUnionFee = 0;
                decimal period2SumTypeNone = 0;
                decimal period2SumTypeTableTax = 0;
                decimal period2SumTypeOneTimeTax = 0;
                decimal period2SumTypeSINKTax = 0;
                decimal period2SumTypeASINKTax = 0;
                decimal period2SumTypeEmploymentTaxDebit = 0;
                decimal period2SumTypeSupplementChargeDebit = 0;
                decimal period2SumTypeGrossSalary = 0;
                decimal period2SumTypeNetSalary = 0;
                decimal period2SumTypeVacationCompensation = 0;
                decimal period2SumTypeBenefit = 0;
                decimal period2SumTypeCompensation = 0;
                decimal period2SumTypeDeduction = 0;
                decimal period2SumTypeUnionFee = 0;
                decimal period3SumTypeNone = 0;
                decimal period3SumTypeTableTax = 0;
                decimal period3SumTypeSINKTax = 0;
                decimal period3SumTypeASINKTax = 0;
                decimal period3SumTypeOneTimeTax = 0;
                decimal period3SumTypeEmploymentTaxDebit = 0;
                decimal period3SumTypeSupplementChargeDebit = 0;
                decimal period3SumTypeGrossSalary = 0;
                decimal period3SumTypeNetSalary = 0;
                decimal period3SumTypeVacationCompensation = 0;
                decimal period3SumTypeBenefit = 0;
                decimal period3SumTypeCompensation = 0;
                decimal period3SumTypeDeduction = 0;
                decimal period3SumTypeUnionFee = 0;

                #endregion

                List<Tuple<int, decimal, SoeEmployeeTimePeriodValueType>> periodValuesTuple = new List<Tuple<int, decimal, SoeEmployeeTimePeriodValueType>>();
                List<XElement> employeeTimePeriodElements = new List<XElement>();

                employeeXmlId++;

                if (selectionTimePeriodIds.Count > 0)
                {
                    foreach (int timePeriodId in selectionTimePeriodIds)
                    {
                        int employeeTimePeriodXmlId = 0;
                        decimal disburementNetAmount = 0;
                        string disbursementAccountNr = "";
                        DateTime? disburementExportDate = null;

                        List<EmployeeTimePeriod> employeeTimePeriods = allEmployeeTimePeriods.Where(p => p.EmployeeId == employeeId && p.TimePeriodId == timePeriodId).ToList();
                        if (employeeTimePeriods.Any())
                        {
                            foreach (EmployeeTimePeriod employeeTimePeriod in employeeTimePeriods.OrderByDescending(e => e.TimePeriod.PaymentDate))
                            {
                                List<TimeSalaryPaymentExport> timeSalaryPaymentExports = allTimeSalaryPaymentExports.Where(t => t.TimePeriodId == timePeriodId).ToList();
                                if (timeSalaryPaymentExports.Any())
                                {
                                    List<TimeSalaryPaymentExportEmployee> exports = new List<TimeSalaryPaymentExportEmployee>();

                                    foreach (TimeSalaryPaymentExport export in timeSalaryPaymentExports)
                                    {
                                        exports.AddRange(export.TimeSalaryPaymentExportEmployee);
                                    }

                                    foreach (TimeSalaryPaymentExportEmployee exportEmployee in exports.Where(e => e.EmployeeId == employeeId))
                                    {
                                        disburementNetAmount += exportEmployee.NetAmount;
                                        disburementExportDate = exportEmployee.PaymentDate;
                                    }

                                    disbursementAccountNr = exports.FirstOrDefault()?.DisbursementAccountNr;
                                }

                                #region EmployeeTimePeriod elements

                                XElement employeeTimePeriodElement = new XElement("EmployeeTimePeriod",
                                    new XAttribute("Id", employeeTimePeriodXmlId),
                                    new XElement("Modified", employeeTimePeriod.Modified.ToValueOrDefault()),
                                    new XElement("ModifiedBy", employeeTimePeriod.ModifiedBy),
                                    new XElement("Created", employeeTimePeriod.Created.ToValueOrDefault()),
                                    new XElement("CreatedBy", employeeTimePeriod.TimePeriod),
                                    new XElement("Status", employeeTimePeriod.Status),
                                    new XElement("DisbursementAccountNr", disbursementAccountNr),
                                    new XElement("DisburementNetAmount", disburementNetAmount),
                                    new XElement("DisburementExportDate", disburementExportDate.ToValueOrDefault()),
                                    new XElement("TimePeriodName", employeeTimePeriod.TimePeriod.TimePeriodHead.Name.Replace(">", "").Replace("<", "")),
                                    new XElement("TimePeriodStartDate", employeeTimePeriod.TimePeriod.StartDate),
                                    new XElement("TimePeriodStopDate", employeeTimePeriod.TimePeriod.StopDate),
                                    new XElement("TimePeriodPayrollStartDate", employeeTimePeriod.TimePeriod.PayrollStartDate),
                                    new XElement("TimePeriodPayrollStopDate", employeeTimePeriod.TimePeriod.PayrollStopDate),
                                    new XElement("TimePeriodPaymentDate", employeeTimePeriod.TimePeriod.PaymentDate));

                                decimal SumTypeNone = 0;
                                decimal SumTypeTableTax = 0;
                                decimal SumTypeOneTimeTax = 0;
                                decimal SumTypeSINKTax = 0;
                                decimal SumTypeASINKTax = 0;
                                decimal SumTypeEmploymentTaxDebit = 0;
                                decimal SumTypeSupplementChargeDebit = 0;
                                decimal SumTypeGrossSalary = 0;
                                decimal SumTypeNetSalary = 0;
                                decimal SumTypeVacationCompensation = 0;
                                decimal SumTypeBenefit = 0;
                                decimal SumTypeCompensation = 0;
                                decimal SumTypeDeduction = 0;
                                decimal SumTypeUnionFee = 0;


                                int EmployeeTimePeriodValueXmlId = 1;
                                List<XElement> employeeTimePeriodValueElements = new List<XElement>();

                                foreach (var value in employeeTimePeriod.EmployeeTimePeriodValue.Where(x => x.State == (int)SoeEntityState.Active))
                                {
                                    #region EmployeeTimeValuePeriod element

                                    SoeEmployeeTimePeriodValueType type = (SoeEmployeeTimePeriodValueType)value.Type;
                                    string typeString = Enum.GetName(typeof(SoeEmployeeTimePeriodValueType), type);

                                    XElement employeeTimePeriodValueElement = new XElement("EmployeeTimeValuePeriod",
                                        new XAttribute("Id", EmployeeTimePeriodValueXmlId),
                                        new XElement("Type", value.Type),
                                        new XElement("TypeString", typeString),
                                        new XElement("Value", value.Value));

                                    employeeTimePeriodValueElements.Add(employeeTimePeriodValueElement);

                                    #endregion

                                    #region Add sums

                                    switch (type)
                                    {
                                        case SoeEmployeeTimePeriodValueType.None:
                                            SumTypeNone += value.Value;
                                            periodValuesTuple.Add(Tuple.Create(EmployeeTimePeriodValueXmlId, value.Value, SoeEmployeeTimePeriodValueType.None));
                                            break;
                                        case SoeEmployeeTimePeriodValueType.TableTax:
                                            SumTypeTableTax += value.Value;
                                            periodValuesTuple.Add(Tuple.Create(EmployeeTimePeriodValueXmlId, value.Value, SoeEmployeeTimePeriodValueType.TableTax));
                                            break;
                                        case SoeEmployeeTimePeriodValueType.OneTimeTax:
                                            SumTypeOneTimeTax += value.Value;
                                            periodValuesTuple.Add(Tuple.Create(EmployeeTimePeriodValueXmlId, value.Value, SoeEmployeeTimePeriodValueType.OneTimeTax));
                                            break;
                                        case SoeEmployeeTimePeriodValueType.SINKTax:
                                            SumTypeSINKTax += value.Value;
                                            periodValuesTuple.Add(Tuple.Create(EmployeeTimePeriodValueXmlId, value.Value, SoeEmployeeTimePeriodValueType.SINKTax));
                                            break;
                                        case SoeEmployeeTimePeriodValueType.ASINKTax:
                                            SumTypeASINKTax += value.Value;
                                            periodValuesTuple.Add(Tuple.Create(EmployeeTimePeriodValueXmlId, value.Value, SoeEmployeeTimePeriodValueType.ASINKTax));
                                            break;
                                        case SoeEmployeeTimePeriodValueType.EmploymentTaxCredit:
                                            SumTypeEmploymentTaxDebit += value.Value;
                                            periodValuesTuple.Add(Tuple.Create(EmployeeTimePeriodValueXmlId, value.Value, SoeEmployeeTimePeriodValueType.EmploymentTaxCredit));
                                            break;
                                        case SoeEmployeeTimePeriodValueType.SupplementChargeCredit:
                                            SumTypeSupplementChargeDebit += value.Value;
                                            periodValuesTuple.Add(Tuple.Create(EmployeeTimePeriodValueXmlId, value.Value, SoeEmployeeTimePeriodValueType.SupplementChargeCredit));
                                            break;
                                        case SoeEmployeeTimePeriodValueType.GrossSalary:
                                            SumTypeGrossSalary += value.Value;
                                            periodValuesTuple.Add(Tuple.Create(EmployeeTimePeriodValueXmlId, value.Value, SoeEmployeeTimePeriodValueType.GrossSalary));
                                            break;
                                        case SoeEmployeeTimePeriodValueType.NetSalary:
                                            SumTypeNetSalary += value.Value;
                                            periodValuesTuple.Add(Tuple.Create(EmployeeTimePeriodValueXmlId, value.Value, SoeEmployeeTimePeriodValueType.NetSalary));
                                            break;
                                        case SoeEmployeeTimePeriodValueType.VacationCompensation:
                                            SumTypeVacationCompensation += value.Value;
                                            periodValuesTuple.Add(Tuple.Create(EmployeeTimePeriodValueXmlId, value.Value, SoeEmployeeTimePeriodValueType.VacationCompensation));
                                            break;
                                        case SoeEmployeeTimePeriodValueType.Benefit:
                                            SumTypeBenefit += value.Value;
                                            periodValuesTuple.Add(Tuple.Create(EmployeeTimePeriodValueXmlId, value.Value, SoeEmployeeTimePeriodValueType.Benefit));
                                            break;
                                        case SoeEmployeeTimePeriodValueType.Compensation:
                                            SumTypeCompensation += value.Value;
                                            periodValuesTuple.Add(Tuple.Create(EmployeeTimePeriodValueXmlId, value.Value, SoeEmployeeTimePeriodValueType.Compensation));
                                            break;
                                        case SoeEmployeeTimePeriodValueType.Deduction:
                                            SumTypeDeduction += value.Value;
                                            periodValuesTuple.Add(Tuple.Create(EmployeeTimePeriodValueXmlId, value.Value, SoeEmployeeTimePeriodValueType.Deduction));
                                            break;
                                        case SoeEmployeeTimePeriodValueType.UnionFee:
                                            SumTypeUnionFee += value.Value;
                                            periodValuesTuple.Add(Tuple.Create(EmployeeTimePeriodValueXmlId, value.Value, SoeEmployeeTimePeriodValueType.UnionFee));
                                            break;
                                        default:
                                            break;
                                    }

                                    #endregion
                                }

                                employeeTimePeriodElement.Add(new XElement("SumTypeNone", SumTypeNone));
                                employeeTimePeriodElement.Add(new XElement("SumTypeTableTax", SumTypeTableTax));
                                employeeTimePeriodElement.Add(new XElement("SumTypeOneTimeTax", SumTypeOneTimeTax));
                                employeeTimePeriodElement.Add(new XElement("SumTypeSINKTax", SumTypeSINKTax));
                                employeeTimePeriodElement.Add(new XElement("SumTypeASINKTax", SumTypeASINKTax));
                                employeeTimePeriodElement.Add(new XElement("SumTypeEmploymentTaxDebit", SumTypeEmploymentTaxDebit));
                                employeeTimePeriodElement.Add(new XElement("SumTypeSupplementChargeDebit", SumTypeSupplementChargeDebit));
                                employeeTimePeriodElement.Add(new XElement("SumTypeGrossSalary", SumTypeGrossSalary));
                                employeeTimePeriodElement.Add(new XElement("SumTypeNetSalary", SumTypeNetSalary));
                                employeeTimePeriodElement.Add(new XElement("SumTypeVacationCompensation", SumTypeVacationCompensation));
                                employeeTimePeriodElement.Add(new XElement("SumTypeBenefit", SumTypeBenefit));
                                employeeTimePeriodElement.Add(new XElement("SumTypeCompensation", SumTypeCompensation));
                                employeeTimePeriodElement.Add(new XElement("SumTypeDeduction", SumTypeDeduction));
                                employeeTimePeriodElement.Add(new XElement("SumTypeUnionFee", SumTypeUnionFee));

                                employeeTimePeriodElement.Add(employeeTimePeriodValueElements);
                                employeeTimePeriodElements.Add(employeeTimePeriodElement);

                                employeeTimePeriodXmlId++;

                                #endregion
                            }

                        }
                        else
                        {
                            XElement employeeTimePeriodElement = GetDefaultElement(reportResult, "EmployeeTimePeriod");
                            XElement employeeTimePeriodValueElement = GetDefaultElement(reportResult, "EmployeeTimeValuePeriod");
                            employeeTimePeriodElement.Add(employeeTimePeriodValueElement);
                            employeeTimePeriodElements.Add(employeeTimePeriodElement);
                        }

                    }

                    #region EmployeeSummery

                    foreach (var item in periodValuesTuple)
                    {
                        SoeEmployeeTimePeriodValueType type = item.Item3;
                        int period = item.Item1;

                        if (period == 0)
                        {
                            switch (type)
                            {
                                case SoeEmployeeTimePeriodValueType.None:
                                    period0SumTypeNone += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.TableTax:
                                    period0SumTypeTableTax += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.OneTimeTax:
                                    period0SumTypeOneTimeTax += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.SINKTax:
                                    period0SumTypeSINKTax += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.ASINKTax:
                                    period0SumTypeASINKTax += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.EmploymentTaxCredit:
                                    period0SumTypeEmploymentTaxDebit += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.SupplementChargeCredit:
                                    period0SumTypeSupplementChargeDebit += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.GrossSalary:
                                    period0SumTypeGrossSalary += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.NetSalary:
                                    period0SumTypeNetSalary += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.VacationCompensation:
                                    period0SumTypeVacationCompensation += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.Benefit:
                                    period0SumTypeBenefit += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.Compensation:
                                    period0SumTypeCompensation += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.Deduction:
                                    period0SumTypeDeduction += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.UnionFee:
                                    period0SumTypeUnionFee += item.Item2;
                                    break;
                                default:
                                    break;
                            }
                        }

                        if (period == 1)
                        {
                            switch (type)
                            {
                                case SoeEmployeeTimePeriodValueType.None:
                                    period1SumTypeNone += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.TableTax:
                                    period1SumTypeTableTax += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.OneTimeTax:
                                    period1SumTypeOneTimeTax += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.SINKTax:
                                    period1SumTypeSINKTax += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.ASINKTax:
                                    period1SumTypeASINKTax += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.EmploymentTaxCredit:
                                    period1SumTypeEmploymentTaxDebit += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.SupplementChargeCredit:
                                    period1SumTypeSupplementChargeDebit += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.GrossSalary:
                                    period1SumTypeGrossSalary += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.NetSalary:
                                    period1SumTypeNetSalary += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.VacationCompensation:
                                    period1SumTypeVacationCompensation += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.Benefit:
                                    period1SumTypeBenefit += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.Compensation:
                                    period1SumTypeCompensation += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.Deduction:
                                    period1SumTypeDeduction += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.UnionFee:
                                    period1SumTypeUnionFee += item.Item2;
                                    break;
                                default:
                                    break;
                            }
                        }

                        if (period == 2)
                        {
                            switch (type)
                            {
                                case SoeEmployeeTimePeriodValueType.None:
                                    period2SumTypeNone += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.TableTax:
                                    period2SumTypeTableTax += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.OneTimeTax:
                                    period2SumTypeOneTimeTax += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.SINKTax:
                                    period2SumTypeSINKTax += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.ASINKTax:
                                    period2SumTypeASINKTax += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.EmploymentTaxCredit:
                                    period2SumTypeEmploymentTaxDebit += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.SupplementChargeCredit:
                                    period2SumTypeSupplementChargeDebit += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.GrossSalary:
                                    period2SumTypeGrossSalary += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.NetSalary:
                                    period2SumTypeNetSalary += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.VacationCompensation:
                                    period2SumTypeVacationCompensation += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.Benefit:
                                    period2SumTypeBenefit += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.Compensation:
                                    period2SumTypeCompensation += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.Deduction:
                                    period2SumTypeDeduction += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.UnionFee:
                                    period2SumTypeUnionFee += item.Item2;
                                    break;
                                default:
                                    break;
                            }
                        }
                        if (period == 3)
                        {
                            switch (type)
                            {
                                case SoeEmployeeTimePeriodValueType.None:
                                    period3SumTypeNone += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.TableTax:
                                    period3SumTypeTableTax += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.OneTimeTax:
                                    period3SumTypeOneTimeTax += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.SINKTax:
                                    period3SumTypeSINKTax += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.ASINKTax:
                                    period3SumTypeASINKTax += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.EmploymentTaxCredit:
                                    period3SumTypeEmploymentTaxDebit += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.SupplementChargeCredit:
                                    period3SumTypeSupplementChargeDebit += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.GrossSalary:
                                    period3SumTypeGrossSalary += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.NetSalary:
                                    period3SumTypeNetSalary += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.VacationCompensation:
                                    period3SumTypeVacationCompensation += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.Benefit:
                                    period3SumTypeBenefit += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.Compensation:
                                    period3SumTypeCompensation += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.Deduction:
                                    period3SumTypeDeduction += item.Item2;
                                    break;
                                case SoeEmployeeTimePeriodValueType.UnionFee:
                                    period3SumTypeUnionFee += item.Item2;
                                    break;
                                default:
                                    break;
                            }
                        }

                    }

                    employeeElement.Add(new XElement("Period0SumTypeNone", period0SumTypeNone));
                    employeeElement.Add(new XElement("Period0SumTypeTableTax", period0SumTypeTableTax));
                    employeeElement.Add(new XElement("Period0SumTypeOneTimeTax", period0SumTypeOneTimeTax));
                    employeeElement.Add(new XElement("Period0SumTypeSINKTax", period0SumTypeSINKTax));
                    employeeElement.Add(new XElement("Period0SumTypeASINKTax", period0SumTypeASINKTax));
                    employeeElement.Add(new XElement("Period0SumTypeEmploymentTaxDebit", period0SumTypeEmploymentTaxDebit));
                    employeeElement.Add(new XElement("Period0SumTypeSupplementChargeDebit", period0SumTypeSupplementChargeDebit));
                    employeeElement.Add(new XElement("Period0SumTypeGrossSalary", period0SumTypeGrossSalary));
                    employeeElement.Add(new XElement("Period0SumTypeNetSalary", period0SumTypeNetSalary));
                    employeeElement.Add(new XElement("Period0SumTypeVacationCompensation", period0SumTypeVacationCompensation));
                    employeeElement.Add(new XElement("Period0SumTypeBenefit", period0SumTypeBenefit));
                    employeeElement.Add(new XElement("Period0SumTypeCompensation", period0SumTypeCompensation));
                    employeeElement.Add(new XElement("Period0SumTypeDeduction", period0SumTypeDeduction));
                    employeeElement.Add(new XElement("Period0SumTypeUnionFee", period0SumTypeUnionFee));
                    employeeElement.Add(new XElement("Period1SumTypeNone", period1SumTypeNone));
                    employeeElement.Add(new XElement("Period1SumTypeTableTax", period1SumTypeTableTax));
                    employeeElement.Add(new XElement("Period1SumTypeOneTimeTax", period1SumTypeOneTimeTax));
                    employeeElement.Add(new XElement("Period1SumTypeSINKTax", period1SumTypeSINKTax));
                    employeeElement.Add(new XElement("Period1SumTypeASINKTax", period1SumTypeASINKTax));
                    employeeElement.Add(new XElement("Period1SumTypeEmploymentTaxDebit", period1SumTypeEmploymentTaxDebit));
                    employeeElement.Add(new XElement("Period1SumTypeSupplementChargeDebit", period1SumTypeSupplementChargeDebit));
                    employeeElement.Add(new XElement("Period1SumTypeGrossSalary", period1SumTypeGrossSalary));
                    employeeElement.Add(new XElement("Period1SumTypeNetSalary", period1SumTypeNetSalary));
                    employeeElement.Add(new XElement("Period1SumTypeVacationCompensation", period1SumTypeVacationCompensation));
                    employeeElement.Add(new XElement("Period1SumTypeBenefit", period1SumTypeBenefit));
                    employeeElement.Add(new XElement("Period1SumTypeCompensation", period1SumTypeCompensation));
                    employeeElement.Add(new XElement("Period1SumTypeDeduction", period1SumTypeDeduction));
                    employeeElement.Add(new XElement("Period1SumTypeUnionFee", period1SumTypeUnionFee));
                    employeeElement.Add(new XElement("Period2SumTypeNone", period2SumTypeNone));
                    employeeElement.Add(new XElement("Period2SumTypeTableTax", period2SumTypeTableTax));
                    employeeElement.Add(new XElement("Period2SumTypeOneTimeTax", period2SumTypeOneTimeTax));
                    employeeElement.Add(new XElement("Period2SumTypeSINKTax", period2SumTypeSINKTax));
                    employeeElement.Add(new XElement("Period2SumTypeASINKTax", period2SumTypeASINKTax));
                    employeeElement.Add(new XElement("Period2SumTypeEmploymentTaxDebit", period2SumTypeEmploymentTaxDebit));
                    employeeElement.Add(new XElement("Period2SumTypeSupplementChargeDebit", period2SumTypeSupplementChargeDebit));
                    employeeElement.Add(new XElement("Period2SumTypeGrossSalary", period2SumTypeGrossSalary));
                    employeeElement.Add(new XElement("Period2SumTypeNetSalary", period2SumTypeNetSalary));
                    employeeElement.Add(new XElement("Period2SumTypeVacationCompensation", period2SumTypeVacationCompensation));
                    employeeElement.Add(new XElement("Period2SumTypeBenefit", period2SumTypeBenefit));
                    employeeElement.Add(new XElement("Period2SumTypeCompensation", period2SumTypeCompensation));
                    employeeElement.Add(new XElement("Period2SumTypeDeduction", period2SumTypeDeduction));
                    employeeElement.Add(new XElement("Period2SumTypeUnionFee", period2SumTypeUnionFee));
                    employeeElement.Add(new XElement("Period3SumTypeNone", period3SumTypeNone));
                    employeeElement.Add(new XElement("Period3SumTypeTableTax", period3SumTypeTableTax));
                    employeeElement.Add(new XElement("Period3SumTypeOneTimeTax", period3SumTypeOneTimeTax));
                    employeeElement.Add(new XElement("Period3SumTypeSINKTax", period3SumTypeSINKTax));
                    employeeElement.Add(new XElement("Period3SumTypeASINKTax", period3SumTypeASINKTax));
                    employeeElement.Add(new XElement("Period3SumTypeEmploymentTaxDebit", period3SumTypeEmploymentTaxDebit));
                    employeeElement.Add(new XElement("Period3SumTypeSupplementChargeDebit", period3SumTypeSupplementChargeDebit));
                    employeeElement.Add(new XElement("Period3SumTypeGrossSalary", period3SumTypeGrossSalary));
                    employeeElement.Add(new XElement("Period3SumTypeNetSalary", period3SumTypeNetSalary));
                    employeeElement.Add(new XElement("Period3SumTypeVacationCompensation", period3SumTypeVacationCompensation));
                    employeeElement.Add(new XElement("Period3SumTypeBenefit", period3SumTypeBenefit));
                    employeeElement.Add(new XElement("Period3SumTypeCompensation", period3SumTypeCompensation));
                    employeeElement.Add(new XElement("Period3SumTypeDeduction", period3SumTypeDeduction));
                    employeeElement.Add(new XElement("Period3SumTypeUnionFee", period3SumTypeUnionFee));

                    #endregion

                    employeeElement.Add(employeeTimePeriodElements);
                }

                employeeTimePeriodReportElement.Add(employeeElement);
            }

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            #region Close document

            rootElement.Add(employeeTimePeriodReportElement);
            document.Add(rootElement);

            return GetValidatedDocument(document, reportResult.ReportTemplateType);

            #endregion
        }

        public XDocument CreateEmployeeVacationInformationReportData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            TryGetDateFromSelection(reportResult, out DateTime selectionDate, "date");
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDate, selectionDate, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return null;

            TryGetIncludeInactiveFromSelection(reportResult, out bool selectionIncludeInactive, out bool _, out bool? selectionActiveEmployees);
            var handelsGuaranteeAmount = PayrollManager.GetSysPayrollPriceAmount(reportResult.ActorCompanyId, (int)TermGroup_SysPayrollPrice.SE_Vacation_HandelsGuaranteeAmount, selectionDate);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Init document

            XDocument document = reportResult.Input.Document;
            XElement rootElement = reportResult.Input.ElementRoot;
            XElement employeeVacationInformationReportElement = reportResult.Input.ElementFirst;

            #endregion

            #region ReportHeaderLabels

            XElement reportHeaderLabelsElement = CreateTimeReportHeaderLabelsElement();
            reportHeaderLabelsElement.Add(CreateDateIntervalLabelReportHeaderLabelsElement());
            employeeVacationInformationReportElement.Add(reportHeaderLabelsElement);

            #endregion

            #region ReportHeader


            XElement reportHeaderElement = CreateTimeReportHeaderElement(reportResult);
            reportHeaderElement.Add(CreateDateIntervalElement(selectionDate, selectionDate));
            employeeVacationInformationReportElement.Add(reportHeaderElement);

            #endregion

            #region PageHeaderLabels

            XElement pageHeaderLabelsElement = new XElement("PageHeaderLabels");
            CreateEmployeeVacationSEReportPageHeaderLabelsElement(pageHeaderLabelsElement, Convert.ToInt32(handelsGuaranteeAmount));
            employeeVacationInformationReportElement.Add(pageHeaderLabelsElement);

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                #region Content

                int employeeXmlId = 1;
                foreach (int employeeId in selectionEmployeeIds)
                {
                    Employee employee = (employees?.GetEmployee(employeeId, selectionActiveEmployees)) ?? EmployeeManager.GetEmployee(entities, employeeId, reportResult.ActorCompanyId, onlyActive: !selectionIncludeInactive, loadContactPerson: true);
                    if (employee == null)
                        continue;

                    #region Employee element

                    XElement employeeElement = new XElement("Employee",
                        new XAttribute("Id", employeeXmlId),
                        new XElement("EmployeeNr", employee.EmployeeNr),
                        new XElement("EmployeeName", employee.Name));
                    base.personalDataRepository.AddEmployee(employee, employeeElement);

                    #endregion

                    #region EmployeeVacationSE element

                    AddEmployeeVacationSEElement(employeeElement, employee.EmployeeId, reportResult.ActorCompanyId, true);

                    #endregion

                    employeeVacationInformationReportElement.Add(employeeElement);
                    employeeXmlId++;
                }

                #region Default Elements

                if (employeeXmlId == 1)
                    AddDefaultElement(reportResult, employeeVacationInformationReportElement, "Employee");

                #endregion

                #endregion
            }

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            #region Close document

            rootElement.Add(employeeVacationInformationReportElement);
            document.Add(rootElement);

            return GetValidatedDocument(document, reportResult.ReportTemplateType);

            #endregion
        }

        public XDocument CreateTimeAbsenceControlReport(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            if (!TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return null;

            TryGetIncludeInactiveFromSelection(reportResult, out _, out _, out bool? selectionActiveEmployees);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Init document

            XDocument document = reportResult.Input.Document;
            XElement rootElement = reportResult.Input.ElementRoot;
            XElement absenceControllReportElement = reportResult.Input.ElementFirst;

            #endregion

            #region ReportHeaderLabels

            XElement reportHeaderLabelsElement = CreateTimeReportHeaderLabelsElement();
            reportHeaderLabelsElement.Add(CreateDateIntervalLabelReportHeaderLabelsElement());
            absenceControllReportElement.Add(reportHeaderLabelsElement);

            #endregion

            #region ReportHeader

            XElement reportHeaderElement = CreateTimeReportHeaderElement(reportResult);
            reportHeaderElement.Add(CreateDateIntervalElement(selectionDateFrom, selectionDateTo));
            absenceControllReportElement.Add(reportHeaderElement);

            #endregion

            #region PageHeaderLabels

            XElement pageHeaderLabelsElement = new XElement("PageHeaderLabels");
            CreateTimePayrollTransactionReportPageHeaderLabelsElement(pageHeaderLabelsElement);
            absenceControllReportElement.Add(pageHeaderLabelsElement);

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                if (employees == null)
                    employees = EmployeeManager.GetAllEmployeesByIds(entities, reportResult.ActorCompanyId, selectionEmployeeIds, active: selectionActiveEmployees, loadEmployment: true);

                List<TimeDeviationCause> timeDeviationCauses = TimeDeviationCauseManager.GetTimeDeviationCauses(reportResult.ActorCompanyId).Where(w => w.HasAttachZeroDaysNbrOfDaySetting).ToList();
                List<int> timeDeviationCauseIds = timeDeviationCauses.Select(s => s.TimeDeviationCauseId).ToList();
                bool showSocialSec = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId, entities: entities);
                var companyTimePayrollTransactionDTOs = TimeTransactionManager.GetTimePayrollTransactionDTOForReport(selectionDateFrom, selectionDateTo, selectionEmployeeIds, reportResult.Input.ActorCompanyId);

                #endregion

                #region Content

                int employeeXmlId = 1;
                foreach (int employeeId in selectionEmployeeIds)
                {
                    #region Prereq

                    Employee employee = employees.FirstOrDefault(i => i.EmployeeId == employeeId);
                    if (employee == null)
                        continue;

                    companyTimePayrollTransactionDTOs.TryGetValue(employeeId, out List<TimePayrollTransactionDTO> timePayrollTransactionDTOs);
                    if (timePayrollTransactionDTOs.IsNullOrEmpty())
                        continue;

                    timePayrollTransactionDTOs = timePayrollTransactionDTOs.Where(w => (w.TimeDeviationCauseStartId.HasValue && timeDeviationCauseIds.Contains(w.TimeDeviationCauseStartId.Value)) || (w.TimeDeviationCauseStopId.HasValue && timeDeviationCauseIds.Contains(w.TimeDeviationCauseStopId.Value))).ToList();
                    if (!timePayrollTransactionDTOs.Any())
                        continue;

                    List<int?> employeeTimeDeviationCauseIds = timePayrollTransactionDTOs.Select(s => s.TimeDeviationCauseStartId).Distinct().ToList();
                    employeeTimeDeviationCauseIds.AddRange(timePayrollTransactionDTOs.Select(s => s.TimeDeviationCauseStopId).Distinct().ToList());
                    employeeTimeDeviationCauseIds = employeeTimeDeviationCauseIds.Distinct().ToList();

                    List<XElement> elements = new List<XElement>();

                    foreach (var employeeTimeDeviationCauseId in employeeTimeDeviationCauseIds)
                    {
                        if (!employeeTimeDeviationCauseId.HasValue)
                            continue;

                        TimeDeviationCause timeDeviationCause = timeDeviationCauses.FirstOrDefault(f => f.TimeDeviationCauseId == employeeTimeDeviationCauseId);
                        List<DateTime?> dates = timePayrollTransactionDTOs.Where(w => (w.TimeDeviationCauseStartId.HasValue || w.TimeDeviationCauseStopId.HasValue) && (w.TimeDeviationCauseStartId == employeeTimeDeviationCauseId || w.TimeDeviationCauseStopId == employeeTimeDeviationCauseId)).Select(s => s.Date).Distinct().ToList();
                        using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                        var dayDTOs = TimeScheduleManager.AdjustDatesAccordingToAttachedDays(entitiesReadOnly, null, employeeTimeDeviationCauseId.Value, reportResult.ActorCompanyId, employeeId, dates.Where(w => w.HasValue).Select(s => s.Value).ToList());
                        foreach (var item in dayDTOs.OrderBy(o => o))
                        {
                            if (!dates.Contains(item))
                            {
                                elements.Add(new XElement("UnattachedDay",
                                   new XElement("Date", item),
                                   new XElement("TimeDeviationCause", timeDeviationCause.Name)));
                            }
                        }
                    }

                    #endregion

                    #region Employee
                    if (elements.Any())
                    {
                        XElement employeeElement = new XElement("Employee",
                            new XAttribute("id", employeeXmlId),
                            new XElement("EmployeeNr", employee.EmployeeNr),
                            new XElement("EmployeeName", employee.Name),
                            new XElement("EmployeeSocialSec", employee.ContactPerson != null && showSocialSec ? employee.ContactPerson.SocialSec : StringUtility.SocialSecYYMMDD_Dash_Stars(employee.SocialSec)));
                        base.personalDataRepository.AddEmployee(employee, employeeElement);

                        employeeElement.Add(elements);
                        absenceControllReportElement.Add(employeeElement);
                    }
                    employeeXmlId++;

                    #endregion
                }

                #endregion

                #region Default element

                if (employeeXmlId == 1)
                    AddDefaultElement(reportResult, absenceControllReportElement, "Employee");

                #endregion
            }

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            #region Close document

            rootElement.Add(absenceControllReportElement);
            document.Add(rootElement);

            return GetValidatedDocument(document, reportResult);

            #endregion
        }

        public XDocument CreateTimeAccumulatorReportData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            if (!TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return null;

            TryGetIncludeInactiveFromSelection(reportResult, out _, out _, out bool? selectionActiveEmployees);
            TryGetBoolFromSelection(reportResult, out bool selectionIncludePreliminary, "includePreliminary");
            TryGetIdsFromSelection(reportResult, out List<int> selectionTimeAccumulatorIds, "timeAccumulators", nullIfEmpty: true);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Init document

            XDocument document = reportResult.Input.Document;
            XElement rootElement = reportResult.Input.ElementRoot;
            XElement timeAccumulatorReportElement = reportResult.Input.ElementFirst;

            #endregion

            #region ReportHeaderLabels

            XElement reportHeaderLabelsElement = CreateTimeReportHeaderLabelsElement();
            reportHeaderLabelsElement.Add(CreateDateIntervalLabelReportHeaderLabelsElement());
            timeAccumulatorReportElement.Add(reportHeaderLabelsElement);

            #endregion

            #region ReportHeader

            XElement reportHeaderElement = CreateTimeReportHeaderElement(reportResult);
            reportHeaderElement.Add(CreateDateIntervalElement(selectionDateFrom, selectionDateTo));
            reportHeaderElement.Add(CreateIncludePreliminaryElement(selectionIncludePreliminary));
            timeAccumulatorReportElement.Add(reportHeaderElement);

            #endregion

            #region PageHeaderLabels

            XElement pageHeaderLabelsElement = new XElement("PageHeaderLabels");
            CreateTimeAccumulatorReportPageHeaderLabelsElement(pageHeaderLabelsElement);
            timeAccumulatorReportElement.Add(pageHeaderLabelsElement);

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, reportResult.ActorCompanyId);
                bool showSumAccTodayValue = FeatureManager.HasRolePermission(Feature.Time_Payroll_Calculation, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId, entities: entities);
                List<CompanyCategoryRecord> companyCategoryRecordsForCompany = !useAccountHierarchy ? CategoryManager.GetCompanyCategoryRecords(entities, SoeCategoryType.Employee, SoeCategoryRecordEntity.Employee, reportResult.Input.ActorCompanyId) : null;
                List<TimeAccumulatorEmployeeGroupRule> timeAccEmployeeGroupRulesForCompany = TimeAccumulatorManager.GetTimeAccumulatorEmployeeGroupRulesForCompany(entities, reportResult.Input.ActorCompanyId, accumulatorIds: selectionTimeAccumulatorIds, loadTimeAccumulator: true);

                List<XElement> employeeElements = new List<XElement>();
                DateTime startOfYear = CalendarUtility.GetFirstDateOfYear(selectionDateFrom);

                #endregion

                #region Content

                int employeeXmlId = 1;
                foreach (int employeeId in selectionEmployeeIds)
                {
                    #region Prereq

                    Employee employee = employees?.GetEmployee(employeeId, selectionActiveEmployees) ?? EmployeeManager.GetEmployee(entities, employeeId, reportResult.Input.ActorCompanyId, onlyActive: selectionActiveEmployees ?? false, loadEmployment: true, loadContactPerson: true);
                    if (employee == null)
                        continue;

                    EmployeeGroup employeeGroup = employee.GetEmployeeGroup(selectionDateFrom, selectionDateTo, base.personalDataRepository.EmployeeGroups);
                    if (employeeGroup == null)
                        continue;

                    List<TimeAccumulatorEmployeeGroupRule> timeAccEmployeeGroupRules = timeAccEmployeeGroupRulesForCompany.Where(r => r.EmployeeGroupId == employeeGroup.EmployeeGroupId).ToList();
                    List<CompanyCategoryRecord> employeeCategoryRecords = !useAccountHierarchy ? companyCategoryRecordsForCompany.GetCategoryRecords(employee.EmployeeId, selectionDateFrom, selectionDateTo, onlyDefaultCategories: true) : null;

                    List<XElement> employeeTimeAccumulatorsElements = new List<XElement>();

                    #endregion

                    int timeAccumulatorXmlId = 1;
                    foreach (TimeAccumulatorEmployeeGroupRule timeAccEmployeeGroupRule in timeAccEmployeeGroupRules)
                    {
                        #region TimeAccumulatorEmployeeGroupRule

                        TimeAccumulatorBalance timeAccumulatorBalance = timeAccEmployeeGroupRule.TimeAccumulator.Type == (int)TermGroup_TimeAccumulatorType.Rolling ? TimeAccumulatorManager.GetTimeAccumulatorBalance(entities, timeAccEmployeeGroupRule.TimeAccumulatorId, employee.EmployeeId, SoeTimeAccumulatorBalanceType.Year, startOfYear) : null;
                        var timeCodeTransactionItems = TimeTransactionManager.GetTimeCodeTransactionsForAcc(entities, timeAccEmployeeGroupRule.TimeAccumulatorId, employeeId, startOfYear, selectionDateTo);
                        var timePayrollTransactionItems = TimeTransactionManager.GetTimePayrollTransactionsForAcc(entities, timeAccEmployeeGroupRule.TimeAccumulatorId, employeeId, startOfYear, selectionDateTo).ToList();
                        var timeInvoiceTransactionItems = TimeTransactionManager.GetTimeInvoiceTransactionsForAcc(entities, timeAccEmployeeGroupRule.TimeAccumulatorId, employeeId, startOfYear, selectionDateTo).ToList();

                        decimal balanceQuantity = timeAccumulatorBalance?.Quantity ?? 0;
                        decimal sumAccToday =
                            timeCodeTransactionItems.Sum(i => i.Quantity * i.CalculateFactor()) +
                            timePayrollTransactionItems.Sum(i => i.Quantity * i.Factor) +
                            timeInvoiceTransactionItems.Sum(i => i.Quantity * i.Factor) +
                            balanceQuantity;
                        decimal sumAccTodayValue = showSumAccTodayValue ? TimeAccumulatorManager.GetTimeAccumulatorValue(entities, reportResult.Input.ActorCompanyId, employee, startOfYear, selectionDateTo, sumAccToday, timeAccEmployeeGroupRule.TimeAccumulator) : 0;

                        #region TimeAccumulator Element

                        XElement timeAccumulatorElement = CreateTimeAccumulatorElement(timeAccumulatorXmlId, timeAccEmployeeGroupRule.TimeAccumulator, balanceQuantity, sumAccToday, sumAccTodayValue, showSumAccTodayValue);
                        timeAccumulatorXmlId++;

                        #endregion

                        #region Transaction Elements in period

                        List<XElement> transactionInPeriodElements = new List<XElement>();

                        var timeCodeTransactionItemsInInterval = timeCodeTransactionItems.Where(tct => tct.Date >= selectionDateFrom).ToList();
                        foreach (var timeCodeTransactionItem in timeCodeTransactionItemsInInterval)
                        {
                            transactionInPeriodElements.Add(CreateTimeAccumulatorTransactionElement(transactionInPeriodElements.Count + 1, timeCodeTransactionItem));
                        }

                        var timePayrollTransactionItemsInInterval = timePayrollTransactionItems.Where(tpt => tpt.Date >= selectionDateFrom).ToList();
                        foreach (var timePayrollTransactionItem in timePayrollTransactionItemsInInterval)
                        {
                            transactionInPeriodElements.Add(CreateTimeAccumulatorTransactionElement(transactionInPeriodElements.Count + 1, timePayrollTransactionItem));
                        }

                        var timeInvoiceTransactionItemsInInterval = timeInvoiceTransactionItems.Where(tit => tit.Date >= selectionDateFrom).ToList();
                        foreach (var timeInvoiceTransactionItem in timeInvoiceTransactionItemsInInterval)
                        {
                            transactionInPeriodElements.Add(CreateTimeAccumulatorTransactionElement(transactionInPeriodElements.Count + 1, timeInvoiceTransactionItem));
                        }

                        if (!transactionInPeriodElements.Any())
                            transactionInPeriodElements.Add(CreateTimeAccumulatorTransactionElement());

                        foreach (XElement transactionElement in transactionInPeriodElements)
                        {
                            timeAccumulatorElement.Add(transactionElement);
                        }

                        #endregion

                        #region Transaction Elements in year

                        List<XElement> transactionsInYearElements = new List<XElement>();

                        var timeCodeTransactionsInYear = timeCodeTransactionItems.Where(tct => tct.Date >= startOfYear && tct.Date < selectionDateFrom).ToList();
                        foreach (var timeCodeTransactionItem in timeCodeTransactionsInYear)
                        {
                            transactionsInYearElements.Add(CreateTimeAccumulatorTransactionElement(transactionsInYearElements.Count + 1, timeCodeTransactionItem, true));
                        }

                        var timePayrollTransactionItemsInYear = timePayrollTransactionItems.Where(tpt => tpt.Date >= startOfYear && tpt.Date < selectionDateFrom).ToList();
                        foreach (var timePayrollTransactionItem in timePayrollTransactionItemsInYear)
                        {
                            transactionsInYearElements.Add(CreateTimeAccumulatorTransactionElement(transactionsInYearElements.Count + 1, timePayrollTransactionItem, true));
                        }

                        var timeInvoiceTransactionItemsInYear = timeInvoiceTransactionItems.Where(tit => tit.Date >= startOfYear && tit.Date < selectionDateFrom).ToList();
                        foreach (var timeInvoiceTransactionItem in timeInvoiceTransactionItemsInYear)
                        {
                            transactionsInYearElements.Add(CreateTimeAccumulatorTransactionElement(transactionsInYearElements.Count + 1, timeInvoiceTransactionItem, true));
                        }

                        if (!transactionsInYearElements.Any())
                            transactionsInYearElements.Add((CreateTimeAccumulatorTransactionElement(incomingTransaction: true)));

                        foreach (XElement transactionIncomingElement in transactionsInYearElements)
                        {
                            timeAccumulatorElement.Add(transactionIncomingElement);
                        }

                        #endregion

                        #region TimeAccumulatorRule Element

                        timeAccumulatorElement.Add(CreateTimeAccumulatorEmployeeGroupRuleElement(1, timeAccEmployeeGroupRule));

                        #endregion

                        employeeTimeAccumulatorsElements.Add(timeAccumulatorElement);

                        #endregion
                    }

                    #region TimeAccumulator Element default

                    if (timeAccumulatorXmlId == 1)
                        employeeTimeAccumulatorsElements.Add(CreateTimeAccumulatorElement());

                    #endregion

                    #region EmployeeElement

                    XElement employeeElement = CreateTimeAccumulatorEmployeeElement(base.personalDataRepository, employeeXmlId, employee, employeeGroup, employeeCategoryRecords);

                    foreach (XElement timeAccumulatorElement in employeeTimeAccumulatorsElements)
                    {
                        employeeElement.Add(timeAccumulatorElement);
                    }

                    employeeElements.Add(employeeElement);
                    employeeXmlId++;

                    #endregion
                }

                #region Default TimeAccumulator

                if (employeeXmlId == 1)
                    employeeElements.Add(CreateTimeAccumulatorEmployeeElement(null));

                #endregion

                foreach (XElement employeeElement in employeeElements)
                {
                    timeAccumulatorReportElement.Add(employeeElement);
                }

                #endregion
            }

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            #region Close document

            rootElement.Add(timeAccumulatorReportElement);
            document.Add(rootElement);

            return GetValidatedDocument(document, reportResult);

            #endregion
        }

        public XDocument CreateTimeAccumulatorDetailedReportData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            if (!TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return null;

            TryGetIncludeInactiveFromSelection(reportResult, out _, out _, out bool? selectionActiveEmployees);
            TryGetBoolFromSelection(reportResult, out bool selectionIncludePreliminary, "includePreliminary");
            TryGetIdsFromSelection(reportResult, out List<int> selectionTimeAccumulatorIds, "timeAccumulators", nullIfEmpty: true);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Init document

            XDocument document = reportResult.Input.Document;
            XElement rootElement = reportResult.Input.ElementRoot;
            XElement timeAccumulatorReportElement = reportResult.Input.ElementFirst;

            #endregion

            #region ReportHeaderLabels

            XElement reportHeaderLabelsElement = CreateTimeReportHeaderLabelsElement();
            reportHeaderLabelsElement.Add(CreateDateIntervalLabelReportHeaderLabelsElement());
            timeAccumulatorReportElement.Add(reportHeaderLabelsElement);

            #endregion

            #region ReportHeader

            XElement reportHeaderElement = CreateTimeReportHeaderElement(reportResult);
            reportHeaderElement.Add(CreateDateIntervalElement(selectionDateFrom, selectionDateTo));
            reportHeaderElement.Add(CreateIncludePreliminaryElement(selectionIncludePreliminary));
            timeAccumulatorReportElement.Add(reportHeaderElement);

            #endregion

            #region PageHeaderLabels

            XElement pageHeaderLabelsElement = new XElement("PageHeaderLabels");
            CreateTimeAccumulatorDetailedReportPageHeaderLabelsElement(pageHeaderLabelsElement);
            timeAccumulatorReportElement.Add(pageHeaderLabelsElement);

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                bool showSumAccTodayValue = FeatureManager.HasRolePermission(Feature.Time_Payroll_Calculation, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId, entities: entities);
                TimePeriod timePeriod = TimePeriodManager.GetTimePeriod(entities, selectionDateFrom, TermGroup_TimePeriodType.Payroll, reportResult.Input.ActorCompanyId);
                List<TimeAccumulator> timeAccumulators = TimeAccumulatorManager.GetTimeAccumulators(entities, reportResult.Input.ActorCompanyId, timeAccumulatorIds: selectionTimeAccumulatorIds, loadEmployeeGroupRule: true);
                List<int> timeAccumulatorIds = timeAccumulators.Select(i => i.TimeAccumulatorId).ToList();
                Dictionary<int, List<TimeCode>> timeAccumulatorTimeCodes = TimeAccumulatorManager.GetTimeAccumulatorTimeCodes(entities, timeAccumulatorIds);
                Dictionary<int, List<PayrollProduct>> timeAccumulatorPayrollProducts = TimeAccumulatorManager.GetTimeAccumulatorPayrollProducts(entities, timeAccumulatorIds);

                #endregion

                #region Content

                int employeeXmlId = 1;
                foreach (int employeeId in selectionEmployeeIds)
                {
                    #region Employee

                    Employee employee = (employees?.GetEmployee(employeeId, selectionActiveEmployees)) ?? EmployeeManager.GetEmployee(entities, employeeId, reportResult.Input.ActorCompanyId, onlyActive: selectionActiveEmployees ?? false, loadEmployment: true, loadContactPerson: true);
                    if (employee == null)
                        continue;

                    EmployeeGroup employeeGroup = employee.GetEmployeeGroup(selectionDateFrom, selectionDateTo, base.personalDataRepository.EmployeeGroups);
                    if (employeeGroup == null)
                        continue;

                    GetTimeAccumulatorItemsInput timeAccInput = GetTimeAccumulatorItemsInput.CreateInput(reportResult.Input.ActorCompanyId, reportResult.Input.UserId, employee.EmployeeId, selectionDateFrom, selectionDateTo, calculateDay: true, calculatePeriod: true, calculatePlanningPeriod: true, calculateYear: true, calculateAccToday: true, calculateAccTodayValue: showSumAccTodayValue);
                    List<TimeAccumulatorItem> items = TimeAccumulatorManager.GetTimeAccumulatorItems(entities, timeAccInput, timeAccumulators, employee, employeeGroup.ObjToList(), timePeriod);

                    XElement employeeElement = new XElement("Employee",
                        new XAttribute("id", employeeXmlId),
                        new XElement("EmployeeNr", employee.EmployeeNr),
                        new XElement("EmployeeName", employee.Name),
                        new XElement("EmployeeGroupName", employeeGroup.Name));
                    personalDataRepository.AddEmployee(employee, employeeElement);

                    int timeAccumulatorXmlId = 1;
                    foreach (TimeAccumulator timeAccumulator in timeAccumulators)
                    {
                        #region TimeAccumulator

                        if (!timeAccumulator.TimeAccumulatorEmployeeGroupRule.Any(i => i.EmployeeGroupId == employeeGroup.EmployeeGroupId))
                            continue;

                        TimeAccumulatorItem item = items.FirstOrDefault(i => i.TimeAccumulatorId == timeAccumulator.TimeAccumulatorId);
                        if (item == null)
                            continue;

                        List<TimeCode> timeCodes = timeAccumulatorTimeCodes[timeAccumulator.TimeAccumulatorId] ?? new List<TimeCode>();
                        List<PayrollProduct> payrollProducts = timeAccumulatorPayrollProducts[timeAccumulator.TimeAccumulatorId] ?? new List<PayrollProduct>();

                        #region TimeAccumulator Element

                        XElement timeAccumulatorElement = new XElement("TimeAccumulator",
                            new XAttribute("id", timeAccumulatorXmlId),
                            new XElement("TimeAccName", timeAccumulator.Name),
                            new XElement("TimeAccDescription", timeAccumulator.Description),
                            new XElement("TimeAccType", timeAccumulator.Type),
                            new XElement("TimeAccTypeName", GetText(timeAccumulator.Type, (int)TermGroup.TimeAccumulatorType)),
                            new XElement("TimeAccShowInTimeReports", timeAccumulator.ShowInTimeReports.ToInt()),
                            new XElement("TimeAccFinalSalary", showSumAccTodayValue ? timeAccumulator.FinalSalary.ToInt() : 0),
                            new XElement("TimeAccFactorBasedOnWorkPercentage", timeCodes.Any(i => i.FactorBasedOnWorkPercentage).ToInt()),
                            new XElement("TimeAccBalanceQuantity", item?.TimeAccumulatorBalanceYear ?? Decimal.Zero),
                            new XElement("TimeAccSumDay", item.SumToday),
                            new XElement("TimeAccSumPeriod", item.SumPeriod),
                            new XElement("TimeAccSumYear", item.SumYear),
                            new XElement("TimeAccSumAccToday", item.SumAccToday),
                            new XElement("TimeAccSumAccTodayValue", showSumAccTodayValue && item.SumAccTodayValue.HasValue ? item.SumAccTodayValue : 0),
                            new XElement("TimeAccBalanceYear", item.TimeAccumulatorBalanceYear));

                        #endregion

                        #region TimeCode element

                        int timeCodeXmlId = 1;
                        foreach (TimeCode timeCode in timeCodes)
                        {
                            XElement timeCodeElement = new XElement("TimeCode",
                                new XAttribute("id", timeCodeXmlId),
                                new XElement("TimeCodeType", timeCode.Type),
                                new XElement("TimeCodeCode", timeCode.Code),
                                new XElement("TimeCodeName", timeCode.Name),
                                new XElement("TimeCodeFactorBasedOnWorkPercentage", timeCode.FactorBasedOnWorkPercentage.ToInt()),
                                new XElement("TimeCodeSumDay", item.SumTimeCodeTodayByTimeCode.GetValue(timeCode.TimeCodeId)),
                                new XElement("TimeCodeSumPeriod", item.SumTimeCodePeriodByTimeCode.GetValue(timeCode.TimeCodeId)),
                                new XElement("TimeCodeSumPlanningPeriod", item.SumTimeCodePlanningPeriodByTimeCode.GetValue(timeCode.TimeCodeId)),
                                new XElement("TimeCodeSumYear", item.SumTimeCodeYearByTimeCode.GetValue(timeCode.TimeCodeId)),
                                new XElement("TimeCodeSumAccToday", item.SumTimeCodeAccTodayByTimeCode.GetValue(timeCode.TimeCodeId)));
                            timeAccumulatorElement.Add(timeCodeElement);
                            timeCodeXmlId++;
                        }

                        #endregion

                        #region PayrollProduct element

                        int payrollProductXmlId = 1;
                        foreach (PayrollProduct payrollProduct in payrollProducts)
                        {
                            XElement payrollProductElement = new XElement("PayrollProduct",
                                new XAttribute("id", payrollProductXmlId),
                                new XElement("ProductNumber", payrollProduct.Number),
                                new XElement("ProductName", payrollProduct.Name),
                                new XElement("ProductShortName", payrollProduct.Name),
                                new XElement("ProductSumDay", item.SumPayrollTodayByPayrollProduct.GetValue(payrollProduct.ProductId)),
                                new XElement("ProductSumPeriod", item.SumPayrollPeriodByPayrollProduct.GetValue(payrollProduct.ProductId)),
                                new XElement("ProductSumPlanningPeriod", item.SumPayrollPlanningPeriodByPayrollProduct.GetValue(payrollProduct.ProductId)),
                                new XElement("ProductSumYear", item.SumPayrollYearByPayrollProduct.GetValue(payrollProduct.ProductId)),
                                new XElement("ProductSumAccToday", item.SumPayrollAccTodayByPayrollProduct.GetValue(payrollProduct.ProductId)));
                            timeAccumulatorElement.Add(payrollProductElement);
                            payrollProductXmlId++;
                        }

                        #endregion

                        employeeElement.Add(timeAccumulatorElement);
                        timeAccumulatorXmlId++;

                        #endregion
                    }

                    #region TimeAccumulator Element default

                    if (timeAccumulatorXmlId == 1)
                        employeeElement.Add(GetDefaultElement(reportResult, "TimeAccumulator"));

                    #endregion

                    timeAccumulatorReportElement.Add(employeeElement);
                    employeeXmlId++;

                    #endregion
                }

                #region TimeAccumulator Element default

                if (employeeXmlId == 1)
                    timeAccumulatorReportElement.Add(GetDefaultElement(reportResult, "Employee"));

                #endregion

                #endregion
            }

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            #region Close document

            rootElement.Add(timeAccumulatorReportElement);
            document.Add(rootElement);

            return GetValidatedDocument(document, reportResult);

            #endregion
        }

        public XDocument CreateTimeCategoryScheduleData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            if (!TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo))
                return null;
            if (!TryGetIdsFromSelection(reportResult, out List<int> selectionCategoryIds, "categories"))
                return null;
            if (!TryGetBoolFromSelection(reportResult, out bool selectionIncludeInactive, "includeInactive"))
                return null;

            TryGetIdFromSelection(reportResult, out int? selectionTimeScheduleScenarioHeadId, "timeScheduleScenarioHeadId");

            #endregion

            #region Init document

            XDocument document = reportResult.Input.Document;
            XElement rootElement = reportResult.Input.ElementRoot;
            XElement timeCategoryScheduleElement = reportResult.Input.ElementFirst;

            #endregion

            #region ReportHeaderLabels

            XElement reportHeaderLabelsElement = CreateTimeReportHeaderLabelsElement();
            reportHeaderLabelsElement.Add(CreateDateIntervalLabelReportHeaderLabelsElement());
            this.AddGrossNetCostHeaderPermissionAndSettingReportHeaderLabelElements(reportHeaderLabelsElement);
            timeCategoryScheduleElement.Add(reportHeaderLabelsElement);

            #endregion

            #region ReportHeader

            XElement reportHeaderElement = CreateTimeReportHeaderElement(reportResult); //TimeCategorySchedule
            reportHeaderElement.Add(CreateDateIntervalElement(selectionDateFrom, selectionDateTo));
            this.AddGrossNetCostHeaderPermissionAndSettingReportHeaderElements(reportHeaderElement, reportResult.Input.RoleId, reportResult.Input.ActorCompanyId);
            timeCategoryScheduleElement.Add(reportHeaderElement);

            #endregion

            #region PageHeaderLabels

            XElement pageHeaderLabelsElement = new XElement("PageHeaderLabels");
            CreateTimeCategorySchedulePageHeaderLabelsElement(pageHeaderLabelsElement);
            timeCategoryScheduleElement.Add(pageHeaderLabelsElement);

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                List<Employee> employees = EmployeeManager.GetAllEmployees(entities, reportResult.Input.ActorCompanyId, active: !selectionIncludeInactive, getHidden: false, getVacant: false, loadEmployment: true);
                List<Category> categories = CategoryManager.GetCategories(entities, SoeCategoryType.Employee, selectionCategoryIds, reportResult.Input.ActorCompanyId);
                Dictionary<int, List<CompanyCategoryRecord>> categoryRecordsByEmployee = CategoryManager.GetCompanyCategoryRecords(SoeCategoryType.Employee, SoeCategoryRecordEntity.Employee, reportResult.Input.ActorCompanyId).ToDict();
                List<HolidayDTO> companyHolidays = CalendarManager.GetHolidaysByCompany(entities, reportResult.Input.ActorCompanyId);
                List<AccountDimDTO> companyAccountDims = AccountManager.GetAccountDimsByCompany(entities, reportResult.Input.ActorCompanyId).ToDTOs();

                const int MAX_SHIFTS = 15;

                #endregion

                #region Content

                int categoryXmlId = 1;
                XElement categoryElement = null;
                foreach (Category category in categories)
                {
                    #region Category

                    #region Prereq

                    List<AttestEmployeeDayDTO> validDays = new List<AttestEmployeeDayDTO>();

                    foreach (Employee employee in employees)
                    {
                        if (!categoryRecordsByEmployee.ContainsKey(employee.EmployeeId))
                            continue;

                        List<CompanyCategoryRecord> categoryRecordsForEmployee = categoryRecordsByEmployee[employee.EmployeeId].GetCategoryRecords(employee.EmployeeId, category.CategoryId, selectionDateFrom, selectionDateTo);
                        if (categoryRecordsForEmployee.IsNullOrEmpty())
                            continue;

                        var input = GetAttestEmployeeInput.CreateAttestInputForWeb(reportResult.Input.ActorCompanyId, reportResult.Input.UserId, reportResult.Input.RoleId, employee.EmployeeId, selectionDateFrom, selectionDateTo, null, null, InputLoadType.GrossNetCost);
                        input.SetOptionalParameters(companyHolidays, companyAccountDims, doMergeTransactions: true, doGetOnlyActive: !selectionIncludeInactive, timeScheduleScenarioHeadId: selectionTimeScheduleScenarioHeadId);

                        validDays.AddRange(TimeTreeAttestManager.GetAttestEmployeeDays(entities, input));
                    }

                    #endregion

                    int weekXmlId = 1;
                    List<XElement> weekElements = new List<XElement>();
                    foreach (var validDaysByWeek in validDays.GroupBy(o => o.WeekNr).ToList())
                    {
                        #region Week

                        int weekNr = validDaysByWeek.Key;
                        DateTime weekDateFrom = CalendarUtility.AdjustDateToBeginningOfWeek(validDaysByWeek.Min(i => i.Date));
                        DateTime weekDateTo = CalendarUtility.AdjustDateToEndOfWeek(weekDateFrom);

                        //Week total
                        int weekCategoryTimeTotal = 0;

                        //Each employeeGroup contains one employees items
                        int employeeXmlId = 1;
                        List<XElement> employeeElements = new List<XElement>();
                        foreach (var validDaysByEmployee in validDaysByWeek.GroupBy(o => o.EmployeeId).ToList())
                        {
                            #region Employee

                            Employee employee = employees.FirstOrDefault(i => i.EmployeeId == validDaysByEmployee.Key);
                            if (employee == null)
                                continue;

                            List<int> hiddenEmployeeShiftTypeIds = employee.Hidden ? TimeScheduleManager.GetShiftTypeIdsForUsersCategories(entities, reportResult.Input.ActorCompanyId, reportResult.Input.UserId, 0, true, false) : new List<int>();

                            //Employe total
                            int employeeWeekTimeTotal = 0;

                            int dayXmlId = 1;
                            List<XElement> dayElements = new List<XElement>();
                            foreach (AttestEmployeeDayDTO item in validDaysByEmployee)
                            {
                                #region Day Element

                                if (!categoryRecordsByEmployee.ContainsKey(employee.EmployeeId))
                                    continue;

                                List<CompanyCategoryRecord> categoryRecordsForDay = categoryRecordsByEmployee[employee.EmployeeId].GetCategoryRecords(employee.EmployeeId, category.CategoryId, CalendarUtility.GetBeginningOfDay(item.Date), CalendarUtility.GetEndOfDay(item.Date));
                                if (categoryRecordsForDay.IsNullOrEmpty())
                                    continue;

                                List<TimeScheduleTemplateBlock> templateBlocks = TimeScheduleManager.GetTimeScheduleTemplateBlocksForPeriod(entities, item.EmployeeId, item.Date, timeScheduleScenarioHeadId: selectionTimeScheduleScenarioHeadId);
                                if (employee.Hidden && hiddenEmployeeShiftTypeIds.Any())
                                    templateBlocks = templateBlocks.Where(tb => tb.ShiftTypeId.HasValue && hiddenEmployeeShiftTypeIds.Contains(tb.ShiftTypeId.Value)).ToList();
                                if (!templateBlocks.Any())
                                    continue;

                                string absencePayrollProductName = "";
                                string absencePayrollProductShortName = "";
                                bool isWholeDayAbsence = item.AbsenceTime == item.ScheduleTime;
                                if (!item.AttestPayrollTransactions.IsNullOrEmpty() && item.AttestPayrollTransactions.Any(a => a.IsAbsence))
                                {
                                    absencePayrollProductName = item.AttestPayrollTransactions.FirstOrDefault().PayrollProductName;
                                    absencePayrollProductShortName = item.AttestPayrollTransactions.FirstOrDefault().PayrollProductShortName;
                                }

                                //Inc Week total
                                employeeWeekTimeTotal += ((Convert.ToInt32(item.ScheduleTime.TotalMinutes)) - (item.ScheduleBreakMinutes) - (item.TimeScheduleTypeFactorMinutes));

                                XElement dayElement = new XElement("Day",
                                    new XAttribute("id", dayXmlId),
                                    new XElement("DayNr", (int)item.Date.DayOfWeek),
                                    new XElement("Date", item.Date.ToShortDateString()),
                                    new XElement("IsZeroScheduleDay", item.IsScheduleZeroDay.ToInt()),
                                    new XElement("IsAbsenceDay", isWholeDayAbsence.ToInt()),
                                    new XElement("IsPreliminary", item.IsPrel.ToInt()),
                                    new XElement("AbsencePayrollProductName", absencePayrollProductName),
                                    new XElement("AbsencePayrollProductShortName", absencePayrollProductShortName),
                                    new XElement("ScheduleStartTime", CalendarUtility.GetHoursAndMinutesString(item.ScheduleStartTime)),
                                    new XElement("ScheduleStopTime", CalendarUtility.GetHoursAndMinutesString(item.ScheduleStopTime)),
                                    new XElement("ScheduleTime", Convert.ToInt32(item.ScheduleTime.TotalMinutes) - (item.TimeScheduleTypeFactorMinutes)),
                                    new XElement("OccupiedTime", 0),
                                    new XElement("ScheduleTypeFactorMinutes", item.TimeScheduleTypeFactorMinutes),
                                    new XElement("ScheduleBreakTime", item.ScheduleBreakMinutes),
                                    new XElement("ScheduleBreak1Start", item.ScheduleBreak1Start),
                                    new XElement("ScheduleBreak1Minutes", item.ScheduleBreak1Minutes),
                                    new XElement("ScheduleBreak2Start", item.ScheduleBreak2Start),
                                    new XElement("ScheduleBreak2Minutes", item.ScheduleBreak2Minutes),
                                    new XElement("ScheduleBreak3Start", item.ScheduleBreak3Start),
                                    new XElement("ScheduleBreak3Minutes", item.ScheduleBreak3Minutes),
                                    new XElement("ScheduleBreak4Start", item.ScheduleBreak4Start),
                                    new XElement("ScheduleBreak4Minutes", item.ScheduleBreak4Minutes),
                                    new XElement("ScheduleDate", item.Date),
                                    new XElement("ScheduleGrossTimeMinutes", item.GrossNetCosts.GetGrossTimeMinutes()),
                                    new XElement("ScheduleNetTimeMinutes", item.GrossNetCosts.GetNetTimeMinutes()),
                                    new XElement("ScheduleTotalCost", item.GrossNetCosts.GetTotalCost()));


                                int shiftXmlId = 1;
                                var shiftElements = new List<XElement>();
                                foreach (var templateBlock in templateBlocks.OrderBy(b => b.StartTime).ToList())
                                {
                                    #region TimeScheduleTemplateBlock

                                    if (templateBlock.ShiftType == null)
                                        continue;

                                    string shiftName = templateBlock.ShiftType != null ? StringUtility.NullToEmpty(templateBlock.ShiftType.Name) : string.Empty;
                                    string shiftDescription = StringUtility.NullToEmpty(templateBlock.Description);
                                    DateTime shiftStartTime = templateBlock.StartTime;
                                    DateTime shiftStopTime = templateBlock.StopTime;
                                    string color = templateBlock.ShiftType != null ? StringUtility.NullToEmpty(templateBlock.ShiftType.Color) : Constants.SHIFT_TYPE_DEFAULT_COLOR;
                                    int grossTimeMinutes = item.GrossNetCosts.GetGrossTimeMinutes(templateBlock.TimeScheduleTemplateBlockId);
                                    int netTimeMinutes = item.GrossNetCosts.GetNetTimeMinutes(templateBlock.TimeScheduleTemplateBlockId);
                                    decimal totalCost = item.GrossNetCosts.GetTotalCost(templateBlock.TimeScheduleTemplateBlockId);

                                    if (grossTimeMinutes == 0 && netTimeMinutes != 0)
                                        grossTimeMinutes = netTimeMinutes;

                                    if (shiftXmlId < 16)
                                    {
                                        dayElement.Add(
                                            new XElement("Shift" + shiftXmlId + "Name", shiftName),
                                            new XElement("Shift" + shiftXmlId + "StartTime", shiftStartTime),
                                            new XElement("Shift" + shiftXmlId + "StopTime", shiftStopTime),
                                            new XElement("Shift" + shiftXmlId + "Description", shiftDescription),
                                            new XElement("Shift" + shiftXmlId + "Color", color),
                                            new XElement("Shift" + shiftXmlId + "GrossTimeMinutes", grossTimeMinutes),
                                            new XElement("Shift" + shiftXmlId + "NetTimeMinutes", netTimeMinutes),
                                            new XElement("Shift" + shiftXmlId + "TotalCost", totalCost));
                                    }

                                    shiftElements.Add(new XElement("Shifts",
                                        new XAttribute("id", shiftXmlId),
                                        new XElement("ShiftName", shiftName),
                                        new XElement("ShiftDescription", shiftDescription),
                                        new XElement("ShiftStartTime", shiftStartTime),
                                        new XElement("ShiftStopTime", shiftStopTime),
                                        new XElement("Color", color),
                                        new XElement("ShiftGrossTimeMinutes", grossTimeMinutes),
                                        new XElement("ShiftNetTimeMinutes", netTimeMinutes),
                                        new XElement("ShiftTotalCost", totalCost)));

                                    shiftXmlId++;

                                    #endregion
                                }

                                //Fill to max shifts
                                for (int shiftNr = shiftXmlId; shiftNr <= MAX_SHIFTS; shiftNr++)
                                {
                                    dayElement.Add(
                                        new XElement("Shift" + shiftNr + "Name", string.Empty),
                                        new XElement("Shift" + shiftNr + "StartTime", CalendarUtility.DATETIME_DEFAULT),
                                        new XElement("Shift" + shiftNr + "StopTime", CalendarUtility.DATETIME_DEFAULT),
                                        new XElement("Shift" + shiftNr + "Description", string.Empty),
                                        new XElement("Shift" + shiftNr + "Color", Constants.SHIFT_TYPE_DEFAULT_COLOR),
                                        new XElement("Shift" + shiftNr + "GrossTimeMinutes", 0),
                                        new XElement("Shift" + shiftNr + "NetTimeMinutes", 0),
                                        new XElement("Shift" + shiftNr + "TotalCost", 0));
                                }

                                //Add all shifts
                                foreach (var shiftElement in shiftElements)
                                {
                                    dayElement.Add(shiftElement);
                                }

                                dayElements.Add(dayElement);
                                dayXmlId++;

                                #endregion
                            }

                            #region Employee Element

                            XElement employeeElement = new XElement("Employee",
                                new XAttribute("id", employeeXmlId),
                                new XElement("EmployeeNr", employee.EmployeeNr),
                                new XElement("EmployeeName", employee.Name),
                                new XElement("EmployeeWeekTimeTotal", employeeWeekTimeTotal)
                             );

                            //Inc Week total
                            weekCategoryTimeTotal += employeeWeekTimeTotal;

                            //Add DayElements
                            foreach (XElement dayElement in dayElements)
                            {
                                employeeElement.Add(dayElement);
                            }
                            dayElements.Clear();

                            #endregion

                            employeeXmlId++;
                            employeeElements.Add(employeeElement);

                            #endregion
                        }

                        #region Week Element

                        XElement weekElement = new XElement("Week",
                            new XAttribute("id", weekXmlId),
                            new XElement("ScheduleWeekNr", weekNr),
                            new XElement("WeekDateFrom", weekDateFrom.ToShortDateString()),
                            new XElement("WeekDateTo", weekDateTo.ToShortDateString()),
                            new XElement("WeekTimeTotal", weekCategoryTimeTotal)
                            );

                        //Add Employee Elements
                        foreach (XElement employeeElement in employeeElements)
                        {
                            weekElement.Add(employeeElement);
                        }
                        employeeElements.Clear();

                        #endregion

                        weekXmlId++;
                        weekElements.Add(weekElement);

                        #endregion
                    }

                    #region Default element Week/Employee/Day

                    if (weekXmlId == 1)
                    {
                        XElement weekElement = new XElement("Week",
                            new XAttribute("id", 1),
                            new XElement("ScheduleWeekNr", 0),
                            new XElement("WeekDateFrom", "00:00"),
                            new XElement("WeekDateTo", "00:00"),
                            new XElement("WeekTimeTotal", 0));

                        XElement employeeElement = new XElement("Employee",
                            new XAttribute("id", 1),
                            new XElement("EmployeeNr", 0),
                            new XElement("EmployeeName", ""),
                            new XElement("EmployeeWeekTimeTotal", 0));

                        XElement dayElement = new XElement("Day",
                            new XAttribute("id", 1),
                            new XElement("DayNr", 0),
                            new XElement("Date", "00:00"),
                            new XElement("IsZeroScheduleDay", 0),
                            new XElement("IsAbsenceDay", 0),
                            new XElement("IsPreliminary", 0),
                            new XElement("AbsencePayrollProductName", ""),
                            new XElement("AbsencePayrollProductShortName", ""),
                            new XElement("ScheduleStartTime", "00:00"),
                            new XElement("ScheduleStopTime", "00:00"),
                            new XElement("ScheduleTime", 0),
                            new XElement("OccupiedTime", 0),
                            new XElement("ScheduleBreakTime", 0),
                            new XElement("ScheduleBreak1Start", CalendarUtility.DATETIME_DEFAULT),
                            new XElement("ScheduleBreak1Minutes", 0),
                            new XElement("ScheduleBreak2Start", CalendarUtility.DATETIME_DEFAULT),
                            new XElement("ScheduleBreak2Minutes", 0),
                            new XElement("ScheduleBreak3Start", CalendarUtility.DATETIME_DEFAULT),
                            new XElement("ScheduleBreak3Minutes", 0),
                            new XElement("ScheduleBreak4Start", CalendarUtility.DATETIME_DEFAULT),
                            new XElement("ScheduleBreak4Minutes", 0)

                           );

                        employeeElement.Add(dayElement);
                        weekElement.Add(employeeElement);
                        weekElements.Add(weekElement);
                    }

                    #endregion

                    #region Category Element

                    categoryElement = new XElement("Category",
                        new XAttribute("id", categoryXmlId),
                        new XElement("CategoryCode", category.Code),
                        new XElement("CategoryName", category.Name)
                    );

                    //Add Week elements
                    foreach (XElement week in weekElements)
                    {
                        categoryElement.Add(week);
                    }
                    weekElements.Clear();

                    #endregion

                    categoryXmlId++;
                    timeCategoryScheduleElement.Add(categoryElement);

                    #endregion
                }

                #region Default element Week/Employee/Day/Category

                if (categoryXmlId == 1)
                {
                    XElement categoryDefaultElement = new XElement("Category",
                        new XAttribute("id", 1),
                        new XElement("CategoryCode", ""),
                        new XElement("CategoryName", ""));

                    XElement weekElement = new XElement("Week",
                        new XAttribute("id", 1),
                        new XElement("ScheduleWeekNr", 0),
                        new XElement("WeekDateFrom", "00:00"),
                        new XElement("WeekDateTo", "00:00"),
                        new XElement("WeekTimeTotal", 0));

                    XElement employeeElement = new XElement("Employee",
                        new XAttribute("id", 1),
                        new XElement("EmployeeNr", 0),
                        new XElement("EmployeeName", ""),
                        new XElement("EmployeeWeekTimeTotal", 0));

                    XElement dayElement = new XElement("Day",
                        new XAttribute("id", 1),
                        new XElement("DayNr", 0),
                        new XElement("Date", "00:00"),
                        new XElement("IsZeroScheduleDay", 0),
                        new XElement("IsAbsenceDay", 0),
                        new XElement("AbsencePayrollProductName", ""),
                        new XElement("AbsencePayrollProductShortName", ""),
                        new XElement("ScheduleStartTime", "00:00"),
                        new XElement("ScheduleStopTime", "00:00"),
                        new XElement("ScheduleTime", 0),
                        new XElement("OccupiedTime", 0),
                        new XElement("ScheduleTypeFactorMinutes", 0),
                        new XElement("ScheduleBreakTime", 0),
                        new XElement("ScheduleBreak1Start", CalendarUtility.DATETIME_DEFAULT),
                        new XElement("ScheduleBreak1Minutes", 0),
                        new XElement("ScheduleBreak2Start", CalendarUtility.DATETIME_DEFAULT),
                        new XElement("ScheduleBreak2Minutes", 0),
                        new XElement("ScheduleBreak3Start", CalendarUtility.DATETIME_DEFAULT),
                        new XElement("ScheduleBreak3Minutes", 0),
                        new XElement("ScheduleBreak4Start", CalendarUtility.DATETIME_DEFAULT),
                        new XElement("ScheduleBreak4Minutes", 0),
                        new XElement("ScheduleDate", CalendarUtility.DATETIME_DEFAULT),
                        new XElement("ScheduleGrossTimeMinutes", 0),
                        new XElement("ScheduleNetTimeMinutes", 0),
                        new XElement("ScheduleTotalCost", 0));

                    employeeElement.Add(dayElement);
                    weekElement.Add(employeeElement);
                    categoryDefaultElement.Add(weekElement);
                    timeCategoryScheduleElement.Add(categoryDefaultElement);
                }

                #endregion

                #endregion
            }

            #region Close document

            rootElement.Add(timeCategoryScheduleElement);
            document.Add(rootElement);

            return GetValidatedDocument(document, reportResult);

            #endregion
        }

        public XDocument CreateTimeCategoryStatisticsData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            if (!TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return null;

            TryGetIncludeInactiveFromSelection(reportResult, out bool selectionIncludeInactive, out _, out bool? selectionActiveEmployees);

            //Get TimeAccumulator
            List<TimeAccumulator> timeAccumulators = TimeAccumulatorManager.GetTimeAccumulators(reportResult.ActorCompanyId, showInTimeReports: true);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Init document

            XDocument document = reportResult.Input.Document;
            XElement rootElement = reportResult.Input.ElementRoot;
            XElement timeCategoryStatisticsElement = reportResult.Input.ElementFirst;

            #endregion

            #region ReportHeaderLabels

            XElement reportHeaderLabelsElement = CreateTimeReportHeaderLabelsElement();
            reportHeaderLabelsElement.Add(CreateDateIntervalLabelReportHeaderLabelsElement());
            timeCategoryStatisticsElement.Add(reportHeaderLabelsElement);

            #endregion

            #region ReportHeader

            XElement reportHeaderElement = CreateTimeReportHeaderElement(reportResult);  //TimeCategoryStatistics
            reportHeaderElement.Add(CreateDateIntervalElement(selectionDateFrom, selectionDateTo));
            timeCategoryStatisticsElement.Add(reportHeaderElement);

            #endregion

            #region PageHeaderLabels

            XElement pageHeaderLabelsElement = new XElement("PageHeaderLabels");
            CreateTimeCategoryStatisticsPageHeaderLabelsElement(pageHeaderLabelsElement);
            this.AddTimeAccumulatorPageHeaderLabelElements(pageHeaderLabelsElement, timeAccumulators);
            timeCategoryStatisticsElement.Add(pageHeaderLabelsElement);

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                Dictionary<int, List<AttestEmployeeDayDTO>> employeeItemsDict = new Dictionary<int, List<AttestEmployeeDayDTO>>();

                List<CompanyCategoryRecord> categoryRecords = CategoryManager.GetCompanyCategoryRecords(SoeCategoryType.Employee, SoeCategoryRecordEntity.Employee, reportResult.ActorCompanyId);
                List<HolidayDTO> companyHolidays = CalendarManager.GetHolidaysByCompany(entities, reportResult.ActorCompanyId);
                List<AccountDimDTO> companyAccountDims = AccountManager.GetAccountDimsByCompany(entities, reportResult.ActorCompanyId).ToDTOs();

                foreach (int employeeId in selectionEmployeeIds)
                {
                    Employee employee = (employees?.GetEmployee(employeeId, selectionActiveEmployees)) ?? EmployeeManager.GetEmployee(entities, employeeId, reportResult.ActorCompanyId, onlyActive: !selectionIncludeInactive, loadEmployment: true, loadContactPerson: true);
                    if (employee != null)
                    {
                        Employment employment = employee.GetEmployment(selectionDateFrom, selectionDateTo);
                        if (employment == null)
                            continue;

                        var input = GetAttestEmployeeInput.CreateAttestInputForWeb(reportResult.ActorCompanyId, reportResult.UserId, reportResult.RoleId, employee.EmployeeId, selectionDateFrom, selectionDateTo);
                        input.SetOptionalParameters(companyHolidays, companyAccountDims, doMergeTransactions: true, doGetOnlyActive: !selectionIncludeInactive);

                        var items = TimeTreeAttestManager.GetAttestEmployeeDays(entities, input);
                        if (items.Any())
                        {
                            if (employees == null)
                                employees = new List<Employee>();
                            if (!employees.Any(i => i.EmployeeId == employee.EmployeeId))
                                employees.Add(employee);
                            if (!employeeItemsDict.ContainsKey(employeeId))
                                employeeItemsDict.Add(employeeId, items);
                        }
                    }
                }

                #endregion

                #region Content

                //Only supported grouping for now
                SoeReportContentGroup group = SoeReportContentGroup.Category;

                switch (group)
                {
                    case SoeReportContentGroup.Category:
                        #region Category

                        int categoryXmlId = 1;
                        List<Category> categories = CategoryManager.GetCategories(entities, SoeCategoryType.Employee, reportResult.ActorCompanyId);
                        foreach (Category category in categories)
                        {
                            //Get Employee's in given Category
                            List<Employee> employeesInCategory = new List<Employee>();
                            if (employees != null)
                            {
                                foreach (Employee employee in employees)
                                {
                                    bool isEmployeeInCategory = categoryRecords != null && categoryRecords.GetCategoryRecords(employee.EmployeeId, category.CategoryId, selectionDateFrom, selectionDateTo, onlyDefaultCategories: false).Count > 0;
                                    if (isEmployeeInCategory)
                                        employeesInCategory.Add(employee);
                                }
                            }

                            if (!employeesInCategory.Any())
                                continue;

                            #region Category

                            XElement categoryElement = new XElement("Group",
                                new XAttribute("id", categoryXmlId),
                                new XElement("GroupName", category.Name),
                                new XElement("GroupCode", category.Code),
                                new XElement("GroupType", (int)group));

                            int employeeXmlId = 1;
                            foreach (Employee employee in employeesInCategory)
                            {
                                #region Employee

                                if (!employeeItemsDict.ContainsKey(employee.EmployeeId))
                                    continue;

                                #region Working variables

                                decimal scheduleTime = 0;
                                decimal scheduleBreakTime = 0;
                                decimal presenceTime = 0;
                                decimal presenceBreakTime = 0;
                                decimal absenceTime = 0;
                                decimal payrollAddedTime = 0;
                                decimal payrollOverTime = 0;
                                decimal payrollInconvinientWorkingHoursTime = 0;
                                decimal payrollInconvinientWorkingHoursScaledTime = 0;

                                #endregion

                                var items = employeeItemsDict[employee.EmployeeId];
                                foreach (var item in items)
                                {
                                    #region AttestEmployeeDay

                                    scheduleTime += Convert.ToInt32(item.ScheduleTime.TotalMinutes) - item.TimeScheduleTypeFactorMinutes;
                                    scheduleBreakTime += item.ScheduleBreakMinutes;
                                    presenceTime += item.PresenceTime.HasValue ? Convert.ToInt32(item.PresenceTime.Value.TotalMinutes) : 0;
                                    presenceBreakTime += item.PresenceBreakMinutes ?? 0;
                                    absenceTime += item.AbsenceTime.HasValue ? Convert.ToInt32(item.AbsenceTime.Value.TotalMinutes) : 0;
                                    payrollAddedTime += Convert.ToInt32(item.PayrollAddedTimeMinutes);
                                    payrollOverTime += Convert.ToInt32(item.PayrollOverTimeMinutes);
                                    payrollInconvinientWorkingHoursTime += Convert.ToInt32(item.PayrollInconvinientWorkingHoursMinutes);
                                    payrollInconvinientWorkingHoursScaledTime += Convert.ToInt32(item.PayrollInconvinientWorkingHoursScaledMinutes);

                                    #endregion
                                }

                                XElement employeeElement = new XElement("Employee",
                                    new XAttribute("id", employeeXmlId),
                                    new XElement("EmployeeName", employee.Name),
                                    new XElement("EmployeeNr", employee.EmployeeNr),
                                    new XElement("ScheduleTime", scheduleTime),
                                    new XElement("ScheduleBreakTime", scheduleBreakTime),
                                    new XElement("PresenceTime", presenceTime),
                                    new XElement("PresenceBreakTime", presenceBreakTime),
                                    new XElement("AbsenceTime", absenceTime),
                                    new XElement("PayrollAddedTime", payrollAddedTime),
                                    new XElement("PayrollOverTime", payrollOverTime),
                                    new XElement("PayrollInconvinientWorkingHoursTime", payrollInconvinientWorkingHoursTime),
                                    new XElement("PayrollInconvinientWorkingHoursScaledTime", payrollInconvinientWorkingHoursScaledTime),
                                    new XElement("CalculatedCostPerHour", EmployeeManager.GetEmployeeCalculatedCost(entities, employee, DateTime.Today, null))
                                    );
                                base.personalDataRepository.AddEmployee(employee, employeeElement);

                                #region TimeAccumulators

                                for (int i = 1; i <= Constants.NOOFTIMEACCUMULATORS; i++)
                                {
                                    if (i > timeAccumulators.Count)
                                    {
                                        employeeElement.Add(
                                            new XElement("TimeAccumulator" + i, 0));
                                        continue;
                                    }

                                    //Calculate TimeAccumulator sum
                                    decimal sum = 0;
                                    TimeAccumulator timeAccumulator = timeAccumulators.ElementAt<TimeAccumulator>(i - 1);
                                    if (timeAccumulator != null)
                                    {
                                        sum += TimeAccumulatorManager.CalculateTimeCodeTransactions(entities, employee.EmployeeId, timeAccumulator.TimeAccumulatorId, selectionDateFrom, selectionDateTo);
                                        sum += TimeAccumulatorManager.CalculateTimePayrollTransactions(entities, employee.EmployeeId, timeAccumulator.TimeAccumulatorId, selectionDateFrom, selectionDateTo);
                                    }

                                    employeeElement.Add(
                                        new XElement("TimeAccumulator" + i, sum));
                                }

                                #endregion

                                categoryElement.Add(employeeElement);
                                employeeXmlId++;

                                #endregion
                            }

                            timeCategoryStatisticsElement.Add(categoryElement);
                            categoryXmlId++;

                            #endregion
                        }

                        #region Default element Group/Employee

                        if (categoryXmlId == 1)
                            AddDefaultElement(reportResult, timeCategoryStatisticsElement, "Group");

                        #endregion

                        #endregion
                        break;
                    case SoeReportContentGroup.EmployeeGroup:
                        #region EmployeeGroup

                        //Not supported

                        #endregion
                        break;
                }

                #endregion
            }

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            #region Close document

            rootElement.Add(timeCategoryStatisticsElement);
            document.Add(rootElement);

            return GetValidatedDocument(document, reportResult);

            #endregion
        }

        public XDocument CreateTimeMonthlyReportData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            if (!TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out List<Employee> employees, out List<int> selectionEmployeeIds, out List<int> selectionAccountIds, out TermGroup_EmployeeSelectionAccountingType selectionAccountingType))
                return null;
            TryGetBoolFromSelection(reportResult, out bool filterOnAccounting, "filterOnAccounting");
            TryGetIncludeInactiveFromSelection(reportResult, out bool selectionIncludeInactive, out _, out bool? selectionActiveEmployees);
            TryGetBoolFromSelection(reportResult, out bool selectionIncludePreliminary, "includePreliminary");
            List<AccountInternalDTO> validAccountInternals = filterOnAccounting && !selectionAccountIds.IsNullOrEmpty() ? AccountManager.GetAccountInternals(reportResult.Input.ActorCompanyId, null).Where(w => selectionAccountIds.Contains(w.AccountId)).ToDTOs() : null;
            List<AccountDim> accountDims = AccountManager.GetAccountDimsByCompany(reportResult.ActorCompanyId);
            AccountDim accountDimStd = accountDims.GetStandard();
            List<AccountDim> accountDimInternals = accountDims.GetInternals();
            List<AccountDimDTO> accountDimDTOs = accountDims.ToDTOs();
            List<TimeAccumulator> timeAccumulators = TimeAccumulatorManager.GetTimeAccumulators(reportResult.ActorCompanyId, showInTimeReports: true);
            List<EmploymentTypeDTO> employmentTypes = EmployeeManager.GetEmploymentTypes(base.ActorCompanyId);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Init document

            XDocument document = reportResult.Input.Document;
            XElement rootElement = reportResult.Input.ElementRoot;
            XElement timeMonthlyElement = reportResult.Input.ElementFirst;

            //ReportHeaderLabels
            XElement reportHeaderLabelsElement = CreateTimeReportHeaderLabelsElement();
            reportHeaderLabelsElement.Add(CreateDateIntervalLabelReportHeaderLabelsElement());
            CreateEnterpriseCurrencyReportHeaderLabelsElement(reportHeaderLabelsElement);
            this.AddGrossNetCostHeaderPermissionAndSettingReportHeaderLabelElements(reportHeaderLabelsElement);
            timeMonthlyElement.Add(reportHeaderLabelsElement);

            //ReportHeader
            XElement reportHeaderElement = CreateTimeReportHeaderElement(reportResult);
            reportHeaderElement.Add(CreateDateIntervalElement(selectionDateFrom, selectionDateTo));
            reportHeaderElement.Add(CreateIncludePreliminaryElement(selectionIncludePreliminary));
            this.AddEnterpriseCurrencyPageLabelElements(reportHeaderElement);
            this.AddGrossNetCostHeaderPermissionAndSettingReportHeaderElements(reportHeaderElement, reportResult.RoleId, reportResult.ActorCompanyId);
            timeMonthlyElement.Add(reportHeaderElement);

            //PageHeaderLabels
            XElement pageHeaderLabelsElement = new XElement("PageHeaderLabels");
            CreateTimeMonthlyReportPageHeaderLabelsElement(pageHeaderLabelsElement);
            this.AddTimeAccumulatorPageHeaderLabelElements(pageHeaderLabelsElement, timeAccumulators);
            this.AddAccountDimPageHeaderLabelElements(pageHeaderLabelsElement, accountDimStd, accountDimInternals);
            timeMonthlyElement.Add(pageHeaderLabelsElement);

            #endregion

            #region Build XML

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                if (employees == null)
                {
                    if (selectionEmployeeIds.Count == 1)
                        employees = EmployeeManager.GetEmployee(entities, selectionEmployeeIds.First(), reportResult.Input.ActorCompanyId, onlyActive: !selectionIncludeInactive, loadEmployment: true, loadVacationGroup: true, loadContactPerson: true).ObjToList();
                    else if (selectionEmployeeIds.Count > 1)
                        employees = EmployeeManager.GetEmployeesForUsersAttestRoles(entities, out _, reportResult.Input.ActorCompanyId, reportResult.Input.UserId, reportResult.Input.RoleId, dateFrom: selectionDateFrom, dateTo: selectionDateTo, active: selectionActiveEmployees, getHidden: true, employeeFilter: selectionEmployeeIds);
                    else
                        employees = new List<Employee>();
                }

                bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, reportResult.Input.ActorCompanyId);
                bool useValidAccountsByHiearchy = AccountManager.TryGetAccountIdsForEmployeeAccountDim(entities, out _, reportResult.Input.ActorCompanyId, reportResult.Input.RoleId, reportResult.Input.UserId, selectionDateFrom, selectionDateTo, out List<int> validAccountIdsByHiearchy, out int employeeAccountDimId);
                int sysCountryId = base.GetCompanySysCountryIdFromCache(entities, reportResult.Input.ActorCompanyId);
                decimal defaultTaxRate = PayrollManager.GetTaxRate(entities, reportResult.Input.ActorCompanyId, selectionDateFrom, null, sysCountryId);

                bool loadGrossNetcost = true;
                if (reportResult.IsReportStandard)
                {
                    List<int> doNotLoadGrossNetCostForStandardTemplateIds = new List<int>()
                    {
                        203, //Tid - Frånvarorapport
                        204, //Tid - Frånvarorapport Sjuk
                        248, //Tid - Frånvarorapport Sjuk per anst
                        457, //Lön - Antal karensdagar
                    };

                    if (doNotLoadGrossNetCostForStandardTemplateIds.Contains(reportResult.ReportTemplateId))
                        loadGrossNetcost = false;
                }
                else
                {
                    List<int> doNotLoadGrossNetCostForReportTemplateIds = new List<int>()
                    {
                        1426, //Mathem: Tid - Frånvarorapport Sjuk per anst
                    };

                    if (doNotLoadGrossNetCostForReportTemplateIds.Contains(reportResult.ReportTemplateId))
                        loadGrossNetcost = false;
                }

                List<CompanyCategoryRecord> companyCategoryRecords = !useAccountHierarchy ? CategoryManager.GetCompanyCategoryRecords(entities, SoeCategoryType.Employee, SoeCategoryRecordEntity.Employee, reportResult.Input.ActorCompanyId) : null;
                List<ShiftType> shiftTypes = TimeScheduleManager.GetShiftTypes(reportResult.Input.ActorCompanyId, loadAccounts: true);
                List<HolidayDTO> companyHolidays = CalendarManager.GetHolidaysByCompany(entities, reportResult.Input.ActorCompanyId);
                List<SysPayrollPriceViewDTO> sysPayrollPriceViews = PayrollManager.GetSysPayrollPriceView(sysCountryId);
                Employee employeeForUser = EmployeeManager.GetEmployeeForUser(entities, reportResult.Input.UserId, reportResult.Input.ActorCompanyId);

                Dictionary<int, decimal> settingPayrollGroupMonthlyWorkTimeDict = new Dictionary<int, decimal>();
                Dictionary<int, List<TimeAccumulatorEmployeeGroupRule>> employeeGroupTimeAccumulatorsCache = new Dictionary<int, List<TimeAccumulatorEmployeeGroupRule>>();
                Currency currency = null;

                #endregion

                #region Content

                #region Sales element

                List<XElement> elementSales = CreateSalesElementFromFrequency(entities, reportResult.Input.ActorCompanyId, reportResult.Input.RoleId, selectionDateFrom, selectionDateTo, useAccountHierarchy);
                timeMonthlyElement.Add(elementSales);

                #endregion

                int employeeXmlId = 1;
                if (!employees.IsNullOrEmpty())
                {
                    foreach (Employee employee in employees.OrderBy(i => i.EmployeeNrSort))
                    {
                        #region Prereq

                        EmployeeGroup employeeGroup = employee.GetEmployeeGroup(selectionDateFrom, selectionDateTo, base.personalDataRepository.EmployeeGroups);
                        if (employeeGroup == null)
                            continue;

                        Employment employment = employee.GetEmployment(selectionDateFrom, selectionDateTo);
                        PayrollGroup payrollGroup = employment?.GetPayrollGroup(selectionDateTo, base.personalDataRepository.PayrollGroups);
                        int? payrollGroupId = payrollGroup?.PayrollGroupId;
                        if (payrollGroup != null && !payrollGroup.PayrollGroupAccountStd.IsLoaded)
                            payrollGroup.PayrollGroupAccountStd.Load();

                        List<TimeBlockDate> timeBlockDatesForEmployee = TimeBlockManager.GetTimeBlockDates(entities, employee.EmployeeId, selectionDateFrom, selectionDateTo);
                        List<CompanyCategoryRecord> categoryRecordsForEmployee = companyCategoryRecords?.GetCategoryRecords(employee.EmployeeId, selectionDateFrom, selectionDateTo, onlyDefaultCategories: true);

                        var input = GetAttestEmployeeInput.CreateAttestInputForWeb(reportResult.Input.ActorCompanyId, reportResult.UserId, reportResult.RoleId, employee.EmployeeId, selectionDateFrom, selectionDateTo, null, null, loadGrossNetcost.ToLoadType(InputLoadType.GrossNetCost), reportResult.Input.GetDetailedInformation.ToLoadType(InputLoadType.Shifts));
                        input.SetOptionalParameters(companyHolidays, accountDimDTOs, base.personalDataRepository.EmployeeGroups, base.personalDataRepository.PayrollGroups, base.personalDataRepository.PayrollPriceTypes, employee, employeeForUser, timeBlockDatesForEmployee, categoryRecordsForEmployee, doMergeTransactions: true, doGetOnlyActive: !selectionIncludeInactive, validAccountInternals: validAccountInternals, employeeSelectionAccountingType: selectionAccountingType);
                        List<AttestEmployeeDayDTO> attestEmployeeDays = TimeTreeAttestManager.GetAttestEmployeeDays(input);
                        if (attestEmployeeDays.IsNullOrEmpty())
                            continue;

                        List<GetTimeStampEntrysEmployeeSummaryResult> timeStampEntrysSummaryForEmployee = TimeStampManager.GetTimeStampEntrysEmployeeSummary(entities, selectionDateFrom, selectionDateTo, employee.EmployeeId);
                        DateTime? birthDate = EmployeeManager.GetEmployeeBirthDate(employee);

                        List<TimeAccumulatorEmployeeGroupRule> employeeGroupTimeAccumulators = new List<TimeAccumulatorEmployeeGroupRule>();
                        if (!timeAccumulators.IsNullOrEmpty())
                        {
                            if (employeeGroupTimeAccumulatorsCache.ContainsKey(employeeGroup.EmployeeGroupId))
                            {
                                employeeGroupTimeAccumulators = employeeGroupTimeAccumulatorsCache[employeeGroup.EmployeeGroupId];
                            }
                            else
                            {
                                employeeGroupTimeAccumulators = TimeAccumulatorManager.GetTimeAccumulatorEmployeeGroupRules(employeeGroup.EmployeeGroupId);
                                employeeGroupTimeAccumulatorsCache.Add(employeeGroup.EmployeeGroupId, employeeGroupTimeAccumulators);
                            }
                        }

                        var sysPayrollPriceViewForEmployeeDict = new Dictionary<DateTime, SysPayrollPriceViewDTO>();
                        var sysPayrollPriceIntervalAmountForEmployeeDict = new Dictionary<DateTime, decimal>();
                        var dayElements = new List<XElement>();
                        var timePayrollTransactionItems = new List<AttestPayrollTransactionDTO>();
                        int presenceDaysTotal = 0;
                        decimal totalTime = 0;

                        #endregion

                        #region Init Employee element

                        XElement employeeElement = new XElement("Employee",
                            new XAttribute("id", employeeXmlId),
                            new XElement("EmployeeNr", employee.EmployeeNr),
                            new XElement("EmployeeName", employee.Name),
                            new XElement("EmployeeGroupName", employeeGroup.Name));
                        base.personalDataRepository.AddEmployee(employee, employeeElement);

                        #endregion

                        int dayXmlId = 1;
                        foreach (AttestEmployeeDayDTO attestEmployeeDay in attestEmployeeDays)
                        {
                            #region Prereq

                            TimeBlockDate timeBlockDate = timeBlockDatesForEmployee.FirstOrDefault(d => d.Date == attestEmployeeDay.Date);
                            Employment employmentForDay = employee.GetEmployment(attestEmployeeDay.Date);

                            //AttestStateName
                            string attestStateName = "";
                            if (attestEmployeeDay.AttestStates.Count == 1 || (attestEmployeeDay.AttestStates.Count > 1 && attestEmployeeDay.HasSameAttestState))
                                attestStateName = attestEmployeeDay.AttestStates.First().Name;
                            else if (attestEmployeeDay.AttestStates.Count > 1)
                                attestStateName = "*";
                            if (String.IsNullOrEmpty(attestStateName) && (attestEmployeeDay.HasPeriodTimeStampsWithoutTransactions() || attestEmployeeDay.HasPeriodNoAttestStates(employeeGroup.AutogenTimeblocks)))
                                attestStateName = GetReportText(775, "Kontrollera");

                            //PayrollAbsence
                            decimal payrollAbsenceTime1 = attestEmployeeDay.PayrollAbsenceMinutes.Count > 0 ? attestEmployeeDay.PayrollAbsenceMinutes.FirstOrDefault().Value : 0;
                            string payrollAbsenceCode1 = attestEmployeeDay.PayrollAbsenceMinutes.Count > 0 ? attestEmployeeDay.PayrollAbsenceMinutes.FirstOrDefault().Key : "";
                            decimal payrollAbsenceTime2 = attestEmployeeDay.PayrollAbsenceMinutes.Count > 1 ? attestEmployeeDay.PayrollAbsenceMinutes.Skip(1).FirstOrDefault().Value : 0;
                            string payrollAbsenceCode2 = attestEmployeeDay.PayrollAbsenceMinutes.Count > 1 ? attestEmployeeDay.PayrollAbsenceMinutes.Skip(1).FirstOrDefault().Key : "";

                            //HasPresence, TotalTime and PresenceDaysTotal
                            totalTime += (payrollAbsenceTime1 + payrollAbsenceTime2);
                            bool hasPresence = attestEmployeeDay.PresenceTime.HasValue && attestEmployeeDay.PresenceTime.Value.TotalMinutes > 0;
                            if (hasPresence)
                            {
                                totalTime += Convert.ToDecimal(attestEmployeeDay.PresenceTime.Value.TotalMinutes);
                                presenceDaysTotal++;
                            }

                            #endregion

                            #region Day Element

                            XElement dayElement = new XElement("Day",
                                new XAttribute("id", dayXmlId),
                                new XElement("EmployeeCategoryCode", categoryRecordsForEmployee.GetCategoryCode()),
                                new XElement("EmployeeCategoryName", categoryRecordsForEmployee.GetCategoryName()),
                                new XElement("ScheduleWeekDay", CalendarUtility.GetDayNameFromCulture(attestEmployeeDay.Date)),
                                new XElement("ScheduleWeekDayShort", CalendarUtility.GetShortDayName(attestEmployeeDay.Date)),
                                new XElement("ScheduleDayNr", CalendarUtility.GetDayOfMonth(attestEmployeeDay.Date)),
                                new XElement("ScheduleDate", attestEmployeeDay.Date),
                                new XElement("ScheduleStartTime", CalendarUtility.GetHoursAndMinutesString(attestEmployeeDay.ScheduleStartTime)),
                                new XElement("ScheduleStopTime", CalendarUtility.GetHoursAndMinutesString(attestEmployeeDay.ScheduleStopTime)),
                                new XElement("ScheduleTime", Convert.ToInt32(attestEmployeeDay.ScheduleTime.TotalMinutes) - (attestEmployeeDay.TimeScheduleTypeFactorMinutes)),
                                new XElement("ScheduleTypeFactorMinutes", attestEmployeeDay.TimeScheduleTypeFactorMinutes),
                                new XElement("ScheduleBreakTime", attestEmployeeDay.ScheduleBreakMinutes),
                                new XElement("AttestStateName", attestStateName),
                                new XElement("PresenceStartTime", attestEmployeeDay.PresenceStartTime.HasValue ? CalendarUtility.GetHoursAndMinutesString(attestEmployeeDay.PresenceStartTime.Value) : "00:00"),
                                new XElement("PresenceStopTime", attestEmployeeDay.PresenceStopTime.HasValue ? CalendarUtility.GetHoursAndMinutesString(attestEmployeeDay.PresenceStopTime.Value) : "00:00"),
                                new XElement("PresenceTime", attestEmployeeDay.PresenceTime.HasValue ? Convert.ToInt32(attestEmployeeDay.PresenceTime.Value.TotalMinutes) : 0),
                                new XElement("PresenceBreakTime", attestEmployeeDay.PresenceBreakMinutes ?? 0),
                                new XElement("HasPresence", hasPresence.ToInt()),
                                new XElement("IsPreliminary", attestEmployeeDay.IsPrel.ToInt()),
                                new XElement("PayrollAddedTime", Convert.ToInt32(attestEmployeeDay.PayrollAddedTimeMinutes)),
                                new XElement("PayrollOverTime", Convert.ToInt32(attestEmployeeDay.PayrollOverTimeMinutes)),
                                new XElement("PayrollInconvinientWorkingHoursTime", Convert.ToInt32(attestEmployeeDay.PayrollInconvinientWorkingHoursMinutes)),
                                new XElement("PayrollInconvinientWorkingHoursScaledTime", Convert.ToInt32(attestEmployeeDay.PayrollInconvinientWorkingHoursScaledMinutes)),
                                new XElement("PayrollAbsenceTime1", Convert.ToInt32(payrollAbsenceTime1)),
                                new XElement("PayrollAbsenceCode1", payrollAbsenceCode1),
                                new XElement("PayrollAbsenceTime2", Convert.ToInt32(payrollAbsenceTime2)),
                                new XElement("PayrollAbsenceCode2", payrollAbsenceCode2),
                                new XElement("ScheduleBreak1Start", attestEmployeeDay.ScheduleBreak1Start),
                                new XElement("ScheduleBreak1Minutes", attestEmployeeDay.ScheduleBreak1Minutes),
                                new XElement("ScheduleBreak2Start", attestEmployeeDay.ScheduleBreak2Start),
                                new XElement("ScheduleBreak2Minutes", attestEmployeeDay.ScheduleBreak2Minutes),
                                new XElement("ScheduleBreak3Start", attestEmployeeDay.ScheduleBreak3Start),
                                new XElement("ScheduleBreak3Minutes", attestEmployeeDay.ScheduleBreak3Minutes),
                                new XElement("ScheduleBreak4Start", attestEmployeeDay.ScheduleBreak4Start),
                                new XElement("ScheduleBreak4Minutes", attestEmployeeDay.ScheduleBreak4Minutes),
                                new XElement("ScheduleGrossTimeMinutes", attestEmployeeDay.GrossNetCosts.GetGrossTimeMinutes()),
                                new XElement("ScheduleNetTimeMinutes", attestEmployeeDay.GrossNetCosts.GetNetTimeMinutes()),
                                new XElement("ScheduleTotalCost", attestEmployeeDay.GrossNetCosts.GetTotalCost()),
                                new XElement("EmployeeManuallyAdjusted", timeBlockDate != null ? timeStampEntrysSummaryForEmployee.HasDateEmployeeManuallyAdjustedTimeStamps(timeBlockDate.TimeBlockDateId).ToInt() : 0),
                                new XElement("PayedTime", attestEmployeeDay.PresencePayedTime.HasValue ? Convert.ToInt32(attestEmployeeDay.PresencePayedTime.Value.TotalMinutes) : 0),
                                new XElement("EmploymentTypeName", employmentForDay.GetEmploymentTypeName(employmentTypes, attestEmployeeDay.Date)));

                            #region TimeAccumulator Element

                            for (int pos = 1; pos <= Constants.NOOFTIMEACCUMULATORS; pos++)
                            {
                                decimal sum = 0;

                                TimeAccumulator timeAccumulator = pos <= timeAccumulators.Count ? timeAccumulators.ElementAt(pos - 1) : null;
                                if (timeAccumulator != null && employeeGroupTimeAccumulators.Any(x => x.TimeAccumulatorId == timeAccumulator.TimeAccumulatorId))
                                {
                                    sum += TimeAccumulatorManager.CalculateTimeCodeTransactions(entities, employee.EmployeeId, timeAccumulator.TimeAccumulatorId, attestEmployeeDay.Date, CalendarUtility.GetEndOfDay(attestEmployeeDay.Date));
                                    sum += TimeAccumulatorManager.CalculateTimePayrollTransactions(entities, employee.EmployeeId, timeAccumulator.TimeAccumulatorId, attestEmployeeDay.Date, CalendarUtility.GetEndOfDay(attestEmployeeDay.Date));
                                }

                                dayElement.Add(new XElement("TimeAccumulator" + pos, sum));
                            }

                            #endregion

                            #region Shifts Element

                            int shiftXmlId = 1;
                            if (reportResult.Input.GetDetailedInformation)
                            {
                                foreach (AttestEmployeeDayShiftDTO shift in attestEmployeeDay.Shifts)
                                {
                                    ShiftType shiftType = shiftTypes.FirstOrDefault(s => s.ShiftTypeId == shift.ShiftTypeId);

                                    var shiftElement = new XElement("Shifts",
                                        new XAttribute("id", shiftXmlId),
                                        new XElement("ShiftName", shift.ShiftTypeName),
                                        new XElement("ShiftDescription", shiftType != null ? shiftType.Description : string.Empty),
                                        new XElement("ShiftStartTime", shift.StartTime),
                                        new XElement("ShiftStopTime", shift.StopTime),
                                        new XElement("ShiftColor", shiftType != null ? shiftType.Color : string.Empty),
                                        new XElement("ShiftGrossTimeMinutes", Convert.ToInt32((shift.StopTime - shift.StartTime).TotalMinutes)),
                                        new XElement("ShiftNetTimeMinutes", Convert.ToInt32((shift.StopTime - shift.StartTime).TotalMinutes)),
                                        new XElement("ShiftTotalCost", 0),
                                        new XElement("ShiftTimeDeviationCauseName", ""));

                                    for (int pos = 1; pos <= Constants.NOOFDIMENSIONS; pos++)
                                    {
                                        AccountDim accountDim = pos <= accountDimInternals.Count && accountDimInternals.Count > 0 ? accountDimInternals.ElementAt(pos - 1) : null;
                                        Account accountInternal = accountDim != null && shiftType != null ? shiftType.AccountInternal?.FirstOrDefault(ai => ai.Account.AccountDimId == accountDim.AccountDimId)?.Account : null;

                                        shiftElement.Add(
                                            new XElement("AccountInternalNr" + pos, accountInternal != null ? accountInternal.AccountNr : string.Empty),
                                            new XElement("AccountInternalName" + pos, accountInternal != null ? accountInternal.Name : string.Empty));
                                    }

                                    dayElement.Add(shiftElement);
                                    shiftXmlId++;
                                }
                            }

                            if (shiftXmlId == 1)
                                AddDefaultElement(reportResult, dayElement, "Shifts");

                            #endregion

                            dayElements.Add(dayElement);
                            dayXmlId++;

                            #endregion

                            #region Calculate AttestPayrollTransactions

                            foreach (var transactionItem in attestEmployeeDay.AttestPayrollTransactions)
                            {
                                if (useValidAccountsByHiearchy)
                                {
                                    AccountDTO accountInternal = transactionItem.AccountInternals?.FirstOrDefault(i => i.AccountDimId == employeeAccountDimId);
                                    if (accountInternal != null && !validAccountIdsByHiearchy.Contains(accountInternal.AccountId))
                                        continue;
                                }

                                if (!timePayrollTransactionItems.Any())
                                {
                                    timePayrollTransactionItems.Add(transactionItem);
                                }
                                else
                                {
                                    bool add = true;
                                    foreach (var timePayrollTransactionItem in timePayrollTransactionItems)
                                    {
                                        if (timePayrollTransactionItem.Match(transactionItem))
                                        {
                                            timePayrollTransactionItem.Update(transactionItem);
                                            add = false;
                                            break;
                                        }
                                    }
                                    if (add)
                                        timePayrollTransactionItems.Add(transactionItem);
                                }
                            }

                            #endregion
                        }

                        #region Calculate CalculatedCostPerHour / EmploymentTax

                        decimal calculatedCostPerHour = PayrollManager.GetEmployeeHourlyPay(entities, employee, employment, selectionDateTo, out DateTime endDate, settingPayrollGroupMonthlyWorkTimeDict, base.personalDataRepository.EmployeeGroups, base.personalDataRepository.PayrollGroups);

                        if (defaultTaxRate != 0)
                        {
                            DateTime currentDate = selectionDateFrom;
                            while (currentDate < endDate)
                            {
                                if (timePayrollTransactionItems.Any(w => w.Date == currentDate))
                                {
                                    if (birthDate.HasValue)
                                    {
                                        if (!sysPayrollPriceViewForEmployeeDict.ContainsKey(currentDate))
                                            sysPayrollPriceViewForEmployeeDict.Add(currentDate, PayrollManager.GetSysPayrollPriceInterval(entities, reportResult.Input.ActorCompanyId, (int)TermGroup_SysPayrollPrice.SE_EmploymentTax, birthDate.Value.Year, currentDate, sysCountryId, sysPayrollPriceViews));
                                        if (!sysPayrollPriceIntervalAmountForEmployeeDict.ContainsKey(currentDate))
                                            sysPayrollPriceIntervalAmountForEmployeeDict.Add(currentDate, decimal.Multiply(PayrollManager.GetTaxRate(entities, reportResult.Input.ActorCompanyId, currentDate, birthDate, sysCountryId, sysPayrollPrice: sysPayrollPriceViewForEmployeeDict[currentDate]), 100));
                                    }
                                    else
                                    {
                                        if (!sysPayrollPriceViewForEmployeeDict.ContainsKey(currentDate))
                                            sysPayrollPriceViewForEmployeeDict.Add(currentDate, PayrollManager.GetSysPayrollPriceInterval(entities, reportResult.Input.ActorCompanyId, (int)TermGroup_SysPayrollPrice.SE_EmploymentTax, DateTime.Now.AddYears(-40).Year, currentDate, sysCountryId, sysPayrollPriceViews));
                                        if (!sysPayrollPriceIntervalAmountForEmployeeDict.ContainsKey(currentDate))
                                            sysPayrollPriceIntervalAmountForEmployeeDict.Add(currentDate, decimal.Multiply(defaultTaxRate, 100));
                                    }
                                }

                                currentDate = currentDate.AddDays(1);
                            }
                        }

                        #endregion

                        #region Finish Employee element

                        employeeElement.Add(new XElement("PresenceDaysTotal", presenceDaysTotal));
                        employeeElement.Add(new XElement("TotalTime", Convert.ToInt32(totalTime)));
                        employeeElement.Add(new XElement("CalculatedCostPerHour", calculatedCostPerHour));

                        for (int pos = 1; pos <= Constants.NOOFTIMEACCUMULATORS; pos++)
                        {
                            decimal sum = 0;

                            TimeAccumulator timeAccumulatorBalance = pos <= timeAccumulators.Count ? timeAccumulators.ElementAt(pos - 1) : null;
                            if (timeAccumulatorBalance != null && employeeGroupTimeAccumulators.Any(x => x.TimeAccumulatorId == timeAccumulatorBalance.TimeAccumulatorId))
                            {
                                sum += TimeAccumulatorManager.CalculateTimeCodeTransactions(entities, employee.EmployeeId, timeAccumulatorBalance.TimeAccumulatorId, CalendarUtility.GetFirstDateOfYear(selectionDateFrom), CalendarUtility.GetEndOfDay(selectionDateFrom.AddDays(-1)));
                                sum += TimeAccumulatorManager.CalculateTimePayrollTransactions(entities, employee.EmployeeId, timeAccumulatorBalance.TimeAccumulatorId, CalendarUtility.GetFirstDateOfYear(selectionDateFrom), CalendarUtility.GetEndOfDay(selectionDateFrom.AddDays(-1)));
                            }

                            employeeElement.Add(new XElement("TimeAccumulatorBalance" + pos, sum));
                        }

                        #region Day Elements

                        foreach (var dayElement in dayElements)
                        {
                            employeeElement.Add(dayElement);
                        }

                        #endregion

                        #region PayrollTransaction Elements

                        int payrollTransactionXmlId = 1;
                        foreach (var timePayrollTransactionItem in timePayrollTransactionItems.OrderBy(i => i.PayrollProductNumber))
                        {
                            #region Prereq

                            string payrollProductTypeLevel3Name = string.Empty;
                            if (timePayrollTransactionItem.PayrollProductSysPayrollTypeLevel2.HasValue && timePayrollTransactionItem.PayrollProductSysPayrollTypeLevel2.Value == (int)TermGroup_SysPayrollType.SE_Time_Accumulator)
                            {
                                if (timePayrollTransactionItem.PayrollProductSysPayrollTypeLevel3.HasValue)
                                    payrollProductTypeLevel3Name = timeAccumulators.FirstOrDefault(a => a.TimeAccumulatorId == timePayrollTransactionItem.PayrollProductSysPayrollTypeLevel3.Value)?.Name ?? string.Empty;
                            }
                            else
                                payrollProductTypeLevel3Name = timePayrollTransactionItem.PayrollProductSysPayrollTypeLevel3.HasValue ? GetText(timePayrollTransactionItem.PayrollProductSysPayrollTypeLevel3.Value, (int)TermGroup.SysPayrollType, 1, "") : string.Empty;

                            string payrollTypeLevel3Name = string.Empty;
                            if (timePayrollTransactionItem.TransactionSysPayrollTypeLevel2.HasValue && timePayrollTransactionItem.TransactionSysPayrollTypeLevel2.Value == (int)TermGroup_SysPayrollType.SE_Time_Accumulator)
                            {
                                if (timePayrollTransactionItem.TransactionSysPayrollTypeLevel3.HasValue)
                                    payrollTypeLevel3Name = timeAccumulators.FirstOrDefault(a => a.TimeAccumulatorId == timePayrollTransactionItem.TransactionSysPayrollTypeLevel3.Value)?.Name ?? string.Empty;
                            }
                            else
                                payrollTypeLevel3Name = timePayrollTransactionItem.TransactionSysPayrollTypeLevel3.HasValue ? GetText(timePayrollTransactionItem.TransactionSysPayrollTypeLevel3.Value, (int)TermGroup.SysPayrollType, 1, "") : string.Empty;


                            decimal employmentTaxPercent = sysPayrollPriceIntervalAmountForEmployeeDict.ContainsKey(timePayrollTransactionItem.Date) ? sysPayrollPriceIntervalAmountForEmployeeDict[timePayrollTransactionItem.Date] : 0;
                            decimal supplementChargePercent = 0;
                            if (payrollGroupId.HasValue)
                                supplementChargePercent = Decimal.Multiply(PayrollManager.CalculateSupplementChargePercentSE(entities, reportResult.Input.ActorCompanyId, selectionDateFrom, payrollGroupId.Value, birthDate, sysPayrollPrice: (sysPayrollPriceViewForEmployeeDict.ContainsKey(timePayrollTransactionItem.Date) ? sysPayrollPriceViewForEmployeeDict[timePayrollTransactionItem.Date] : null), payrollGroupAccountStds: (payrollGroup?.PayrollGroupAccountStd?.ToList()), setDefaultAge: true), 100);

                            #endregion

                            #region PayrollTransaction Element

                            XElement payrollTransactionElement = new XElement("PayrollTransaction",
                                new XAttribute("id", payrollTransactionXmlId),
                                new XElement("PayrollProductNumber", timePayrollTransactionItem.PayrollProductNumber),
                                new XElement("PayrollProductName", timePayrollTransactionItem.PayrollProductName),
                                new XElement("PayrollProductMinutes", Convert.ToInt32(timePayrollTransactionItem.Quantity)),
                                new XElement("PayrollProductFactor", Convert.ToInt32(timePayrollTransactionItem.PayrollProductFactor)),
                                new XElement("PayrollProductType", 0), //Deprecated but cannot delete from XML due to existing customer specific reports
                                new XElement("PayrollProductTypeLevel1", timePayrollTransactionItem.PayrollProductSysPayrollTypeLevel1.HasValue ? GetText(timePayrollTransactionItem.PayrollProductSysPayrollTypeLevel1.Value, (int)TermGroup.SysPayrollType, 1, "") : string.Empty),
                                new XElement("PayrollProductTypeLevel2", timePayrollTransactionItem.PayrollProductSysPayrollTypeLevel2.HasValue ? GetText(timePayrollTransactionItem.PayrollProductSysPayrollTypeLevel2.Value, (int)TermGroup.SysPayrollType, 1, "") : string.Empty),
                                new XElement("PayrollProductTypeLevel3", payrollProductTypeLevel3Name),
                                new XElement("PayrollProductTypeLevel4", timePayrollTransactionItem.PayrollProductSysPayrollTypeLevel4.HasValue ? GetText(timePayrollTransactionItem.PayrollProductSysPayrollTypeLevel4.Value, (int)TermGroup.SysPayrollType, 1, "") : string.Empty),
                                new XElement("UnitPrice", timePayrollTransactionItem.UnitPrice ?? 0),
                                new XElement("UnitPriceCurrency", timePayrollTransactionItem.UnitPriceCurrency ?? 0),
                                new XElement("UnitPriceEntCurrency", timePayrollTransactionItem.UnitPriceEntCurrency ?? 0),
                                new XElement("Amount", timePayrollTransactionItem.Amount ?? 0),
                                new XElement("AmountCurrency", timePayrollTransactionItem.AmountCurrency ?? 0),
                                new XElement("AmountEntCurrency", timePayrollTransactionItem.AmountEntCurrency ?? 0),
                                new XElement("VatAmount", timePayrollTransactionItem.VatAmount ?? 0),
                                new XElement("VatAmountCurrency", timePayrollTransactionItem.VatAmountCurrency ?? 0),
                                new XElement("VatAmountEntCurrency", timePayrollTransactionItem.VatAmountEntCurrency ?? 0),
                                new XElement("Quantity", timePayrollTransactionItem.Quantity),
                                new XElement("QuantityWorkDays", timePayrollTransactionItem.QuantityWorkDays),
                                new XElement("QuantityCalendarDays", timePayrollTransactionItem.QuantityCalendarDays),
                                new XElement("CalenderDayFactor", timePayrollTransactionItem.CalenderDayFactor),
                                new XElement("TimeUnit", timePayrollTransactionItem.TimeUnit),
                                new XElement("TimeUnitName", GetText(timePayrollTransactionItem.TimeUnit, (int)TermGroup.PayrollProductTimeUnit)),
                                new XElement("TimeCodeRegistrationType", (int)timePayrollTransactionItem.TimeCodeRegistrationType),
                                new XElement("IsPreliminary", timePayrollTransactionItem.IsPreliminary.ToInt()),
                                new XElement("Date", timePayrollTransactionItem.Date),
                                new XElement("PayrollType", 0), //Deprecated but cannot delete from XML due to existing customer specific reports
                                new XElement("PayrollTypeLevel1", timePayrollTransactionItem.TransactionSysPayrollTypeLevel1.HasValue ? GetText(timePayrollTransactionItem.TransactionSysPayrollTypeLevel1.Value, (int)TermGroup.SysPayrollType, 1, "") : string.Empty),
                                new XElement("PayrollTypeLevel2", timePayrollTransactionItem.TransactionSysPayrollTypeLevel2.HasValue ? GetText(timePayrollTransactionItem.TransactionSysPayrollTypeLevel2.Value, (int)TermGroup.SysPayrollType, 1, "") : string.Empty),
                                new XElement("PayrollTypeLevel3", payrollTypeLevel3Name),
                                new XElement("PayrollTypeLevel4", timePayrollTransactionItem.TransactionSysPayrollTypeLevel4.HasValue ? GetText(timePayrollTransactionItem.TransactionSysPayrollTypeLevel4.Value, (int)TermGroup.SysPayrollType, 1, "") : string.Empty),
                                new XElement("Factor", timePayrollTransactionItem.PayrollProductFactor),
                                new XElement("PayedTime", timePayrollTransactionItem.PayrollProductPayed.ToInt()),
                                new XElement("CurrencyName", currency != null ? currency.Name : string.Empty),
                                new XElement("CurrencyCode", currency != null ? currency.Code : string.Empty),
                                new XElement("Formula", timePayrollTransactionItem.Formula),
                                new XElement("FormulaExtracted", timePayrollTransactionItem.FormulaExtracted),
                                new XElement("FormulaNames", timePayrollTransactionItem.FormulaNames),
                                new XElement("FormulaOrigin", timePayrollTransactionItem.FormulaOrigin),
                                new XElement("Note", timePayrollTransactionItem.Comment.NullToEmpty()),
                                new XElement("StartTime", timePayrollTransactionItem.StartTime.ToValueOrDefault()),
                                new XElement("StopTime", timePayrollTransactionItem.StopTime.ToValueOrDefault()),
                                new XElement("IsScheduleTransaction", timePayrollTransactionItem.IsScheduleTransaction.ToInt()),
                                new XElement("EmploymentTaxPercent", employmentTaxPercent),
                                new XElement("SupplementChargePercent", supplementChargePercent),
                                new XElement("IsRetroactive", timePayrollTransactionItem.IsRetroactive.ToInt()),
                                new XElement("IsReversed", timePayrollTransactionItem.IsReversed.ToInt()));

                            for (int pos = 1; pos <= Constants.NOOFDIMENSIONS; pos++)
                            {
                                AccountDim accountDim = accountDimInternals.Count > 0 && pos <= accountDimInternals.Count ? accountDimInternals.ElementAt(pos - 1) : null;
                                AccountDTO accountInternal = accountDim != null ? timePayrollTransactionItem.AccountInternals.FirstOrDefault(ai => ai.AccountDimId == accountDim.AccountDimId) : null;

                                payrollTransactionElement.Add(
                                    new XElement("AccountInternalNr" + pos, accountInternal?.AccountNr ?? string.Empty),
                                    new XElement("AccountInternalName" + pos, accountInternal?.Name ?? string.Empty));
                            }

                            employeeElement.Add(payrollTransactionElement);
                            payrollTransactionXmlId++;

                            #endregion
                        }
                        #region Default PayrollTransaction element

                        if (payrollTransactionXmlId == 1)
                            AddDefaultElement(reportResult, employeeElement, "PayrollTransaction");

                        #endregion

                        #endregion

                        timeMonthlyElement.Add(employeeElement);
                        employeeXmlId++;

                        #endregion
                    }
                }

                #endregion

                #region Default element

                if (employeeXmlId == 1)
                    AddDefaultElement(reportResult, timeMonthlyElement, "Employee");

                #endregion
            }

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            #region Close document

            rootElement.Add(timeMonthlyElement);
            document.Add(rootElement);

            return GetValidatedDocument(document, reportResult);

            #endregion
        }

        public XDocument CreateTimeScheduleBlockHistoryData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            if (!TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return null;

            TryGetIncludeInactiveFromSelection(reportResult, out bool selectionIncludeInactive, out _, out bool? selectionActiveEmployees);

            List<int> selectionShiftTypeIds = new List<int>();

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Init document

            XDocument document = reportResult.Input.Document;
            XElement rootElement = reportResult.Input.ElementRoot;
            XElement timeEmployeeScheduleElement = reportResult.Input.ElementFirst;

            #endregion

            #region ReportHeader

            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            string companyLogoPath = GetCompanyLogoFilePath(entitiesReadOnly, reportResult.ActorCompanyId, false);

            XElement reportHeaderElement = CreateTimeReportHeaderElement(reportResult); // TimeScheduleBlockHistory
            reportHeaderElement.Add(CreateDateIntervalElement(selectionDateFrom, selectionDateTo));
            reportHeaderElement.Add(new XElement("CompanyLogo", companyLogoPath));

            timeEmployeeScheduleElement.Add(reportHeaderElement);

            #endregion

            #region ReportHeaderLabels

            XElement reportHeaderLabelsElement = CreateTimeReportHeaderLabelsElement();
            reportHeaderLabelsElement.Add(CreateDateIntervalLabelReportHeaderLabelsElement());

            timeEmployeeScheduleElement.Add(reportHeaderLabelsElement);

            #endregion

            #region PageHeaderLabels

            XElement pageHeaderLabelsElement = new XElement("PageHeaderLabels");
            CreateTimeEmployeeSchedulePageHeaderLabelsElement(pageHeaderLabelsElement);

            pageHeaderLabelsElement.Add(
                new XElement("TypeNameLabel", "Typ"),
                new XElement("FromShiftStatusLabel", "Från status"),
                new XElement("ToShiftStatusLabel", "Till status"),
                new XElement("ShiftStatusChangedLabel", "Förändrad status"),
                new XElement("FromShiftUserStatusLabel", "Från användarstatus"),
                new XElement("ToShiftUserStatusLabel", "Till användarstatus"),
                new XElement("ShiftUserStatusChangedLabel", "Förändrad användarstatus"),
                new XElement("FromEmployeeNameLabel", "Från anställd"),
                new XElement("ToEmployeeNameLabel", "Till anställd"),
                new XElement("FromEmployeeNrLabel", "Från anställningsnummer"),
                new XElement("ToEmployeeNrLabel", "Till anställningsnummer"),
                new XElement("FromTimeLabel", "Från"),
                new XElement("ToTimeLabel", "Till"),
                new XElement("TimeChangedLabel", "Förändrad tid"),
                new XElement("FromShiftTypeLabel", "Från passtyp"),
                new XElement("ToShiftTypeLabel", "Till passtyp"),
                new XElement("ShiftTypeChangedLabel", "Förändrad passtyp"),
                new XElement("FromTimeDeviationCauseLabel", "Från orsak"),
                new XElement("CreatedLabel", "Skapad"),
                new XElement("CreatedByLabel", "Förändrad av"),
                new XElement("AbsenceRequestApprovedTextLabel", "Frånvarotext"),
                new XElement("FromStartLabel", "Från starttid"),
                new XElement("FromStopLabel", "Från Stopptid"),
                new XElement("ToStartLabel", "Till Starttid"),
                new XElement("ToStopLabel", "Till Stopptid"),
                new XElement("OriginEmployeeNrLabel", "Ursprunglig anställd, nr"),
                new XElement("OriginEmployeeNameLabel", "Ursprunlig anställd"));

            pageHeaderLabelsElement.Add(
                new XElement("TimeDeviationCauseLabel", "Orsak"),
                new XElement("ShiftTypeLabel", "Passtyp"),
                new XElement("ScheduleLabel", "Schema"));


            timeEmployeeScheduleElement.Add(pageHeaderLabelsElement);

            #endregion

            #region Content

            using (CompEntities entities = new CompEntities())
            {
                if (employees == null)
                    employees = EmployeeManager.GetAllEmployees(entities, reportResult.ActorCompanyId, active: selectionActiveEmployees, loadEmployment: true, getHidden: true);

                List<HolidayDTO> companyHolidays = CalendarManager.GetHolidaysByCompany(entities, reportResult.ActorCompanyId);
                List<AccountDimDTO> companyAccountDims = AccountManager.GetAccountDimsByCompany(entities, reportResult.ActorCompanyId).ToDTOs();

                CultureInfo culture = CalendarUtility.GetValidCultureInfo(Constants.SYSLANGUAGE_LANGCODE_DEFAULT);
                List<string> weekNrs = CalendarUtility.GetWeekNrs(selectionDateFrom, selectionDateTo, culture);

                int employeeXmlId = 1;
                foreach (int employeeId in selectionEmployeeIds)
                {
                    #region Prereq

                    Employee employee = employees?.GetEmployee(employeeId, selectionActiveEmployees);
                    if (employee == null)
                        continue;

                    EmployeeGroup employeeGroup = employee.GetEmployeeGroup(selectionDateFrom, base.personalDataRepository.EmployeeGroups);
                    Category category = CategoryManager.GetCategory(SoeCategoryType.Employee, SoeCategoryRecordEntity.Employee, employee.EmployeeId, reportResult.ActorCompanyId, onlyDefaultCategory: true);
                    List<TimeScheduleTemplateBlock> templateBlocks = TimeScheduleManager.GetTimeScheduleTemplateBlocks(entities, employee.EmployeeId, selectionDateFrom, selectionDateTo);
                    List<ShiftHistoryDTO> shiftHistorys = TimeScheduleManager.GetTimeScheduleTemplateBlockHistory(reportResult.ActorCompanyId, templateBlocks.Select(i => i.TimeScheduleTemplateBlockId).ToList());

                    XElement employeeElement = new XElement("Employee",
                        new XAttribute("id", employeeXmlId),
                        new XElement("EmployeeNr", employee.EmployeeNr),
                        new XElement("EmployeeName", employee.Name),
                        new XElement("EmployeeCategory", category != null ? category.Name.ToString() : string.Empty),
                        new XElement("EmployeeGroup", employeeGroup != null && employeeGroup.Name != null ? employeeGroup.Name : string.Empty),
                        new XElement("EmployeeGroupRuleWorkTimeWeek", employeeGroup != null ? employeeGroup.RuleWorkTimeWeek : 0),
                        new XElement("EmployeeGroupRuleWorkTimeYear", 0),
                        new XElement("EmployeeGroupRuleRestTimeWeek", employeeGroup != null ? employeeGroup.RuleRestTimeWeek : 0),
                        new XElement("EmployeeGroupRuleRestTimeDay", employeeGroup != null ? employeeGroup.RuleRestTimeDay : 0));
                    base.personalDataRepository.AddEmployee(employee, employeeElement);

                    #endregion

                    #region Employee

                    List<XElement> historyElements = new List<XElement>();
                    XElement dayElement = null;
                    XElement previousDayElement = null;
                    DateTime previousDate = new DateTime();
                    bool unsavedDay = false;

                    int historyXmlId = 1;
                    foreach (ShiftHistoryDTO shift in shiftHistorys.OrderBy(s => s.ToStop))
                    {
                        #region ShiftHistory

                        bool newDate = false;

                        DateTime dayDate = Convert.ToDateTime(shift.ToStop).Date;

                        if (previousDate == new DateTime() || previousDate != Convert.ToDateTime(shift.ToStop).Date)
                        {
                            TimeScheduleTemplateBlock templateBlock = TimeScheduleManager.GetTimeScheduleTemplateBlock(entities, shift.TimeScheduleTemplateBlockId, employee.EmployeeId);
                            if (templateBlock?.Date != null)
                            {
                                int dayXmlId = 0;
                                dayDate = templateBlock.Date.Value;
                                dayElement = CreateTimeEmployeeScheduleCommmonXML(entities, ref dayXmlId, reportResult, employee, dayDate, dayDate, weekNrs, selectionShiftTypeIds, null, onlyActive: !selectionIncludeInactive, companyHolidays: companyHolidays, companyAccountDims: companyAccountDims).FirstOrDefault();
                                newDate = true;
                                unsavedDay = true;
                            }
                        }

                        if (unsavedDay && newDate && historyElements.Count != 0)
                        {
                            if (previousDayElement != null)
                            {
                                previousDayElement.Add(historyElements);
                                employeeElement.Add(previousDayElement);
                            }

                            historyElements = new List<XElement>();
                            previousDayElement = null;
                            unsavedDay = false;
                        }

                        XElement historyElement = new XElement("ShiftHistory",
                            new XAttribute("id", historyXmlId),
                            new XElement("TypeName", shift.TypeName),
                            new XElement("FromShiftStatus", shift.FromShiftStatus),
                            new XElement("ToShiftStatus", shift.ToShiftStatus),
                            new XElement("ShiftStatusChanged", shift.ShiftStatusChanged),
                            new XElement("FromShiftUserStatus", shift.FromShiftUserStatus),
                            new XElement("ToShiftUserStatus", shift.ToShiftUserStatus),
                            new XElement("ShiftUserStatusChanged", shift.ShiftUserStatusChanged),
                            new XElement("FromEmployeeName", shift.FromEmployeeName),
                            new XElement("ToEmployeeName", shift.ToEmployeeName),
                            new XElement("FromEmployeeNr", shift.FromEmployeeNr),
                            new XElement("ToEmployeeNr", shift.ToEmployeeNr),
                            new XElement("FromTime", shift.FromTime),
                            new XElement("ToTime", shift.ToTime),
                            new XElement("TimeChanged", shift.TimeChanged.ToInt()),
                            new XElement("FromShiftType", shift.FromShiftType),
                            new XElement("ToShiftType", shift.ToShiftType),
                            new XElement("ShiftTypeChanged", shift.ShiftTypeChanged.ToInt()),
                            new XElement("FromTimeDeviationCause", shift.FromTimeDeviationCause),
                            new XElement("Created", shift.Created != null ? shift.Created : CalendarUtility.DATETIME_DEFAULT),
                            new XElement("CreatedBy", shift.CreatedBy),
                            new XElement("AbsenceRequestApprovedText", shift.AbsenceRequestApprovedText),
                            new XElement("FromStart", shift.FromStart),
                            new XElement("FromStop", shift.FromStop),
                            new XElement("ToStart", shift.ToStart),
                            new XElement("ToStop", shift.ToStop),
                            new XElement("OriginEmployeeNr", shift.OriginEmployeeNr),
                            new XElement("OriginEmployeeName", shift.OriginEmployeeName));

                        if (historyElement != null && dayElement != null && shift.ToStop != null && previousDate == dayDate)
                        {
                            historyElements.Add(historyElement);
                            previousDayElement = dayElement;
                            dayElement = null;
                            continue;
                        }

                        if (previousDayElement == null && dayElement != null)
                            previousDayElement = dayElement;

                        dayElement = null;

                        if (previousDate != dayDate)
                            historyElements.Add(historyElement);

                        previousDate = dayDate;

                        #endregion
                    }

                    if (unsavedDay && historyElements.Count != 0)
                    {
                        if (previousDayElement != null)
                        {
                            previousDayElement.Add(historyElements);
                            employeeElement.Add(previousDayElement);
                        }
                        else if (dayElement != null)
                        {
                            dayElement.Add(historyElements);
                            employeeElement.Add(dayElement);
                        }
                    }

                    if (employeeElement != null)
                        timeEmployeeScheduleElement.Add(employeeElement);

                    employeeXmlId++;

                    #endregion
                }
            }

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            #region Close document

            rootElement.Add(timeEmployeeScheduleElement);
            document.Add(rootElement);

            return GetValidatedDocument(document, reportResult);

            #endregion
        }

        public XDocument CreateTimeScheduleTasksAndDeliverysReportData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo);
            TryGetIdsFromSelection(reportResult, out List<int> selectionShiftTypeIds, "shiftTypes");

            AccountDim accountDimStd = AccountManager.GetAccountDimStd(reportResult.ActorCompanyId);
            List<AccountDim> accountDimInternals = AccountManager.GetAccountDimInternalsByCompany(reportResult.ActorCompanyId);

            #endregion

            #region Init document

            XDocument document = reportResult.Input.Document;
            XElement rootElement = reportResult.Input.ElementRoot;
            XElement timeScheduleTasksAndDeliverysElement = reportResult.Input.ElementFirst;

            #endregion

            #region ReportHeaderLabels

            XElement reportHeaderLabelsElement = CreateTimeReportHeaderLabelsElement();
            reportHeaderLabelsElement.Add(CreateDateIntervalLabelReportHeaderLabelsElement());
            timeScheduleTasksAndDeliverysElement.Add(reportHeaderLabelsElement);

            #endregion

            #region ReportHeader

            XElement reportHeaderElement = CreateTimeReportHeaderElement(reportResult);
            reportHeaderElement.Add(CreateDateIntervalElement(selectionDateFrom, selectionDateTo));
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            reportHeaderElement.Add(new XElement("CompanyLogo", GetCompanyLogoFilePath(entitiesReadOnly, reportResult.ActorCompanyId, false)));
            timeScheduleTasksAndDeliverysElement.Add(reportHeaderElement);

            #endregion

            #region PageHeaderLabels

            XElement pageHeaderLabelsElement = new XElement("PageHeaderLabels");
            CreateTimeScheduleTasksAndDeliverysPageHeaderLabelsElement(pageHeaderLabelsElement);
            this.AddAccountDimPageHeaderLabelElements(pageHeaderLabelsElement, accountDimStd, accountDimInternals);
            timeScheduleTasksAndDeliverysElement.Add(pageHeaderLabelsElement);

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                List<TimeScheduleTask> timeScheduleTasks = TimeScheduleManager.GetTimeScheduleTasks(reportResult.ActorCompanyId, selectionDateFrom, selectionDateTo, loadType: true, setRecurringDescription: true);
                timeScheduleTasks = timeScheduleTasks.Where(i => i.RecurringDates != null && i.RecurringDates.HasRecurringDates()).ToList();
                if (!selectionShiftTypeIds.IsNullOrEmpty())
                    timeScheduleTasks = timeScheduleTasks.Where(t => t.ShiftTypeId.HasValue && selectionShiftTypeIds.Contains(t.ShiftTypeId.Value)).ToList();

                List<IncomingDeliveryHead> incomingDeliveryHeads = TimeScheduleManager.GetIncomingDeliveries(reportResult.ActorCompanyId, selectionDateFrom, selectionDateTo, loadRows: true, setRecurringDescription: true);
                incomingDeliveryHeads = incomingDeliveryHeads.Where(i => i.RecurringDates != null && i.RecurringDates.HasRecurringDates()).ToList();

                List<int?> allShiftTypeIds = new List<int?>();

                #endregion

                #region Content

                #region TimeScheduleTasks

                int timeScheduleTaskXmlId = 1;
                foreach (TimeScheduleTask timeScheduleTask in timeScheduleTasks)
                {
                    #region TimeScheduleTask element

                    allShiftTypeIds.Add(timeScheduleTask.ShiftTypeId);

                    XElement timeScheduleTaskElement = new XElement("TimeScheduleTask",
                        new XAttribute("id", timeScheduleTaskXmlId),
                        new XElement("Name", timeScheduleTask.Name),
                        new XElement("Description", timeScheduleTask.Description),
                        new XElement("RecurrenceDescription", timeScheduleTask.RecurrencePatternDescription),
                        new XElement("StartDate", timeScheduleTask.StartDate.ToShortDateString()),
                        new XElement("StopDate", timeScheduleTask.StopDate.ToValueOrDefault().ToShortDateString()),
                        new XElement("StartTime", timeScheduleTask.StartTime.ToValueOrDefault().ToShortDateString()),
                        new XElement("StopTime", timeScheduleTask.StopTime.ToValueOrDefault().ToShortDateString()),
                        new XElement("Length", timeScheduleTask.Length),
                        new XElement("OnlyOneEmployee", timeScheduleTask.OnlyOneEmployee.ToInt()),
                        new XElement("AllowOverlapping", timeScheduleTask.AllowOverlapping.ToInt()),
                        new XElement("MinSplitLength", timeScheduleTask.MinSplitLength),
                        new XElement("NbrOfPersons", timeScheduleTask.NbrOfPersons),
                        new XElement("DontAssignBreakLeftovers", timeScheduleTask.DontAssignBreakLeftovers.ToInt()),
                        new XElement("IsStaffingNeedsFrequency", timeScheduleTask.IsStaffingNeedsFrequency.ToInt()),
                        new XElement("TimeScheduleTaskTypeName", timeScheduleTask.TimeScheduleTaskType != null ? timeScheduleTask.TimeScheduleTaskType.Name : string.Empty),
                        new XElement("ShiftTypeId", timeScheduleTask.ShiftTypeId ?? 0));

                    int recurringDateXmlId = 1;
                    foreach (DateTime date in timeScheduleTask.RecurringDates.GetValidDates())
                    {
                        XElement recurringDateElement = new XElement("RecurringDate",
                            new XAttribute("id", recurringDateXmlId),
                            new XElement("Date", date.ToShortDateString()));

                        timeScheduleTaskElement.Add(recurringDateElement);
                        recurringDateXmlId++;
                    }

                    timeScheduleTasksAndDeliverysElement.Add(timeScheduleTaskElement);
                    timeScheduleTaskXmlId++;

                    #endregion
                }

                #region Default elements

                if (timeScheduleTaskXmlId == 1)
                {
                    XElement timeScheduleTaskElement = new XElement("TimeScheduleTask",
                        new XAttribute("id", 1));

                    XElement recurringDateElement = new XElement("RecurringDate",
                        new XAttribute("id", 1));

                    timeScheduleTaskElement.Add(recurringDateElement);
                    timeScheduleTasksAndDeliverysElement.Add(timeScheduleTaskElement);
                }

                #endregion

                #endregion

                #region IncomingDelivery

                int incomingDeliveryHeadXmlId = 1;
                foreach (IncomingDeliveryHead incomingDeliveryHead in incomingDeliveryHeads.Where(h => h.IncomingDeliveryRow != null))
                {
                    List<IncomingDeliveryRow> incomingDeliveryRows = incomingDeliveryHead.IncomingDeliveryRow.Where(i => i.State == (int)SoeEntityState.Active).ToList();
                    if (!selectionShiftTypeIds.IsNullOrEmpty())
                        incomingDeliveryRows = incomingDeliveryRows?.Where(t => t.ShiftTypeId.HasValue && selectionShiftTypeIds.Contains(t.ShiftTypeId.Value)).ToList();
                    if (incomingDeliveryRows.IsNullOrEmpty())
                        continue;

                    #region IncomingDeliveryHead element

                    XElement incomingDeliveryHeadElement = new XElement("IncomingDeliveryHead",
                        new XAttribute("id", incomingDeliveryHeadXmlId),
                        new XElement("Name", incomingDeliveryHead.Name),
                        new XElement("Description", incomingDeliveryHead.Description),
                        new XElement("RecurrenceDescription", incomingDeliveryHead.RecurrencePatternDescription),
                        new XElement("StartDate", (incomingDeliveryHead.StartDate).ToShortDateString()),
                        new XElement("StopDate", (incomingDeliveryHead.StopDate.ToValueOrDefault()).ToShortDateString()));

                    int incomingDeliveryRowXmlId = 1;
                    foreach (IncomingDeliveryRow incomingDeliveryRow in incomingDeliveryRows)
                    {
                        allShiftTypeIds.Add(incomingDeliveryRow.ShiftTypeId);

                        XElement incomingDeliveryRowElement = new XElement("IncomingDeliveryRow",
                            new XAttribute("id", incomingDeliveryRowXmlId),
                            new XElement("Name", incomingDeliveryRow.Name),
                            new XElement("Description", incomingDeliveryRow.Description),
                            new XElement("StartTime", incomingDeliveryRow.StartTime.ToValueOrDefault().ToShortDateString()),
                            new XElement("StopTime", incomingDeliveryRow.StopTime.ToValueOrDefault().ToShortDateString()),
                            new XElement("Length", incomingDeliveryRow.Length),
                            new XElement("OnlyOneEmployee", incomingDeliveryRow.OnlyOneEmployee.ToInt()),
                            new XElement("AllowOverlapping", incomingDeliveryRow.AllowOverlapping.ToInt()),
                            new XElement("MinSplitLength", incomingDeliveryRow.MinSplitLength),
                            new XElement("NbrOfPersons", incomingDeliveryRow.NbrOfPersons),
                            new XElement("DontAssignBreakLeftovers", incomingDeliveryRow.DontAssignBreakLeftovers.ToInt()),
                            new XElement("ShiftTypeId", incomingDeliveryRow.ShiftTypeId ?? 0));

                        incomingDeliveryHeadElement.Add(incomingDeliveryRowElement);
                        incomingDeliveryRowXmlId++;
                    }

                    int recurringDateXmlId = 1;
                    foreach (DateTime date in incomingDeliveryHead.RecurringDates.GetValidDates())
                    {
                        XElement recurringDateElement = new XElement("RecurringDate",
                            new XAttribute("id", recurringDateXmlId),
                            new XElement("Date", date.ToShortDateString()));

                        incomingDeliveryHeadElement.Add(recurringDateElement);
                        recurringDateXmlId++;
                    }

                    timeScheduleTasksAndDeliverysElement.Add(incomingDeliveryHeadElement);
                    incomingDeliveryHeadXmlId++;

                    #endregion
                }

                #region Default elements

                if (incomingDeliveryHeadXmlId == 1)
                {
                    XElement incomingDeliveryHeadElement = new XElement("IncomingDeliveryHead",
                        new XAttribute("id", 1));

                    XElement incomingDeliveryRowElement = new XElement("IncomingDeliveryRow",
                        new XAttribute("id", 1));

                    incomingDeliveryHeadElement.Add(incomingDeliveryRowElement);

                    XElement recurringDateElement = new XElement("RecurringDate",
                        new XAttribute("id", 1));

                    incomingDeliveryHeadElement.Add(recurringDateElement);
                    timeScheduleTasksAndDeliverysElement.Add(incomingDeliveryHeadElement);
                }

                #endregion

                #endregion

                #region ShiftType element

                int shiftTypeXmlId = 1;
                List<int> usedShiftTypeIds = allShiftTypeIds.Where(i => i.HasValue).Select(i => i.Value).Distinct().ToList();
                foreach (int shiftTypeId in usedShiftTypeIds)
                {
                    ShiftType shiftType = TimeScheduleManager.GetShiftType(shiftTypeId, loadAccounts: true, loadTimeScheduleType: true);
                    if (shiftType == null)
                        continue;

                    XElement shiftTypeElement = new XElement("ShiftType",
                        new XAttribute("id", shiftTypeXmlId),
                        new XElement("ShiftTypeId", shiftType.ShiftTypeId),
                        new XElement("Name", shiftType.Name),
                        new XElement("Description", shiftType.Description),
                        new XElement("DefaultLength", shiftType.DefaultLength),
                        new XElement("Color", shiftType.Color),
                        new XElement("HandlingMoney", shiftType.HandlingMoney.ToInt()),
                        new XElement("ExternalCode", shiftType.ExternalCode),
                        new XElement("NeedsCode", shiftType.NeedsCode),
                        new XElement("TimeScheduleType", shiftType.TimeScheduleType?.Name ?? string.Empty));

                    for (int i = 1; i <= Constants.NOOFDIMENSIONS; i++)
                    {
                        if (i > accountDimInternals.Count)
                        {
                            shiftTypeElement.Add(
                                new XElement("AccountInternalNr" + i, string.Empty),
                                new XElement("AccountInternalName" + i, string.Empty));
                            continue;
                        }

                        AccountInternal accountInternal = null;
                        AccountDim accountDim = accountDimInternals.Count > 0 ? accountDimInternals.ElementAt(i - 1) : null;
                        if (accountDim != null)
                        {
                            accountInternal = (from ai in shiftType.AccountInternal
                                               where ai.Account.AccountDimId == accountDim.AccountDimId
                                               select ai).FirstOrDefault();
                        }

                        shiftTypeElement.Add(
                            new XElement("AccountInternalNr" + i, accountInternal?.Account?.AccountNr ?? string.Empty),
                            new XElement("AccountInternalName" + i, accountInternal?.Account?.Name ?? string.Empty));
                    }

                    timeScheduleTasksAndDeliverysElement.Add(shiftTypeElement);
                    shiftTypeXmlId++;
                }

                #region Default elements

                if (shiftTypeXmlId == 1)
                {
                    XElement timeScheduleTaskElement = new XElement("ShiftType",
                        new XAttribute("id", shiftTypeXmlId));

                    timeScheduleTasksAndDeliverysElement.Add(timeScheduleTaskElement);
                }

                #endregion

                #endregion

                #endregion
            }

            #region Close document

            rootElement.Add(timeScheduleTasksAndDeliverysElement);
            document.Add(rootElement);

            return GetValidatedDocument(document, SoeReportTemplateType.TimeScheduleTasksAndDeliverysReport);

            #endregion
        }

        public XDocument CreateTimeStampEntryReportData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            if (!TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return null;

            TryGetIncludeInactiveFromSelection(reportResult, out _, out _, out bool? selectionActiveEmployees);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Init document

            XDocument document = reportResult.Input.Document;
            XElement rootElement = reportResult.Input.ElementRoot;
            XElement timeStampEntryReportElement = reportResult.Input.ElementFirst;

            #endregion

            #region ReportHeaderLabels

            XElement reportHeaderLabelsElement = CreateTimeReportHeaderLabelsElement();
            reportHeaderLabelsElement.Add(CreateDateIntervalLabelReportHeaderLabelsElement());
            timeStampEntryReportElement.Add(reportHeaderLabelsElement);

            #endregion

            #region ReportHeader

            XElement reportHeaderElement = CreateTimeReportHeaderElement(reportResult);  //TimeStampEntryReport
            reportHeaderElement.Add(CreateDateIntervalElement(selectionDateFrom, selectionDateTo));
            timeStampEntryReportElement.Add(reportHeaderElement);

            #endregion

            #region PageHeaderLabels

            XElement pageHeaderLabelsElement = new XElement("PageHeaderLabels");
            CreateTimeStampEntryReportPageHeaderLabelsElement(pageHeaderLabelsElement);
            timeStampEntryReportElement.Add(pageHeaderLabelsElement);

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                if (employees == null)
                    employees = EmployeeManager.GetAllEmployeesByIds(entities, reportResult.ActorCompanyId, selectionEmployeeIds, active: selectionActiveEmployees, loadEmployment: true);

                List<TimeTerminal> cachedTimeTerminals = new List<TimeTerminal>();
                List<TimeDeviationCause> cachedTimeDeviationCauses = new List<TimeDeviationCause>();
                bool showSocialSec = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId, entities: entities);

                #endregion

                #region Content

                int employeeXmlId = 1;
                foreach (int employeeId in selectionEmployeeIds)
                {
                    #region Prereq

                    Employee employee = employees.FirstOrDefault(i => i.EmployeeId == employeeId);
                    if (employee == null)
                        continue;

                    EmployeeGroup employeeGroup = employee.GetEmployeeGroup(selectionDateFrom, selectionDateTo, base.personalDataRepository.EmployeeGroups);
                    if (employeeGroup == null)
                        continue;

                    List<TimeStampEntry> timeStampEntries = TimeStampManager.GetTimeStampEntries(selectionDateFrom, selectionDateTo, employee.EmployeeId, loadTimeBlock: true);
                    if (timeStampEntries.IsNullOrEmpty())
                        continue;

                    List<TimeBlockDate> cachedTimeBlockDates = new List<TimeBlockDate>();

                    #endregion

                    #region Employee

                    XElement employeeElement = new XElement("Employee",
                        new XAttribute("id", employeeXmlId),
                        new XElement("EmployeeNr", employee.EmployeeNr),
                        new XElement("EmployeeName", employee.Name),
                        new XElement("EmployeeSocialSec", employee.ContactPerson != null && showSocialSec ? employee.ContactPerson.SocialSec : StringUtility.SocialSecYYMMDD_Dash_Stars(employee.SocialSec)),
                        new XElement("EmployeeSex", employee.ContactPerson?.Sex ?? 0),
                        new XElement("EmployeeGroupName", employeeGroup.Name));
                    base.personalDataRepository.AddEmployee(employee, employeeElement);

                    int timeStampEntryXmlId = 1;
                    foreach (TimeStampEntry timeStampEntry in timeStampEntries)
                    {
                        #region TimeStampEntry

                        TimeTerminal timeTerminal = TimeStampManager.GetTimeTerminalDiscardStateWithCache(timeStampEntry.TimeTerminalId, ref cachedTimeTerminals);
                        TimeBlockDate timeBlockDate = TimeBlockManager.GetTimeBlockDateWithCache(timeStampEntry.TimeBlockDateId, employee.EmployeeId, ref cachedTimeBlockDates);
                        TimeDeviationCause timeDeviationCause = TimeDeviationCauseManager.GetTimeDeviationCause(timeStampEntry.TimeDeviationCauseId, reportResult.ActorCompanyId, ref cachedTimeDeviationCauses);

                        XElement timeStampEntryElement = new XElement("TimeStampEntry",
                            new XAttribute("id", timeStampEntryXmlId),
                            new XElement("Type", timeStampEntry.Type),
                            new XElement("WithoutCard", 0),
                            new XElement("Time", timeStampEntry.Time),
                            new XElement("OriginalTime", timeStampEntry.OriginalTime.ToValueOrDefault()),
                            new XElement("Status", timeStampEntry.Status),
                            new XElement("Note", timeStampEntry.Note),
                            new XElement("Created", timeStampEntry?.Created.ToValueOrDefault() ?? CalendarUtility.DATETIME_DEFAULT), //only set when not from terminal (ask rickard)
                            new XElement("CreatedBy", timeTerminal == null ? timeStampEntry.CreatedBy : string.Empty), //only set when not from terminal (ask rickard)
                            new XElement("Modified", timeStampEntry.Modified.ToValueOrDefault()),
                            new XElement("ModifiedBy", timeStampEntry.ModifiedBy),
                            new XElement("State", timeStampEntry.State),
                            new XElement("TimeBlockDateId", timeStampEntry.TimeBlockDateId.ToInt()),
                            new XElement("TimeBlockDate", timeBlockDate?.Date ?? CalendarUtility.DATETIME_DEFAULT),
                            new XElement("TimeDeviationCause", timeDeviationCause?.Name ?? string.Empty),
                            new XElement("EmployeeManuallyAdjusted", timeStampEntry.EmployeeManuallyAdjusted.ToInt()));

                        int timeBlockXmlId = 1;
                        foreach (TimeBlock timeBlock in timeStampEntry.TimeBlock)
                        {
                            #region TimeBlock

                            timeBlockDate = TimeBlockManager.GetTimeBlockDateWithCache(timeBlock.TimeBlockDateId, employee.EmployeeId, ref cachedTimeBlockDates);

                            XElement timeBlockElement = new XElement("TimeBlock",
                                new XAttribute("id", timeBlockXmlId),
                                new XElement("Date", timeBlockDate?.Date ?? CalendarUtility.DATETIME_DEFAULT),
                                new XElement("StartTime", timeBlock.StartTime),
                                new XElement("StopTime", timeBlock.StopTime),
                                new XElement("IsBreak", timeBlock.IsBreak.ToInt()),
                                new XElement("ManuallyAdjusted", timeBlock.ManuallyAdjusted.ToInt()),
                                new XElement("Created", timeBlock.Created.ToValueOrDefault()),
                                new XElement("CreatedBy", timeBlock.CreatedBy),
                                new XElement("TimeBlockDateId", timeBlock.TimeBlockDateId));

                            timeStampEntryElement.Add(timeBlockElement);
                            timeBlockXmlId++;

                            #endregion
                        }

                        if (timeBlockXmlId == 1)
                            AddDefaultElement(reportResult, timeStampEntryElement, "TimeBlock");

                        employeeElement.Add(timeStampEntryElement);
                        timeStampEntryXmlId++;

                        #endregion
                    }

                    timeStampEntryReportElement.Add(employeeElement);
                    employeeXmlId++;

                    #endregion
                }

                #endregion

                #region Default element

                if (employeeXmlId == 1)
                    AddDefaultElement(reportResult, timeStampEntryReportElement, "Employee");

                #endregion
            }

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            #region Close document

            rootElement.Add(timeStampEntryReportElement);
            document.Add(rootElement);

            return GetValidatedDocument(document, reportResult);

            #endregion
        }

        public XDocument CreateTimePayrollTransactionReportData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            if (!TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out List<Employee> employees, out List<int> selectionEmployeeIds, out List<int> selectionAccountIds, out _))
                return null;

            TryGetBoolFromSelection(reportResult, out bool filterOnAccounting, "filterOnAccounting");
            TryGetPayrollProductIdsFromSelections(reportResult, out List<int> selectionPayrollProductIds);
            TryGetBoolFromSelection(reportResult, out bool selectionIncludePreliminary, "includePreliminary");
            TryGetBoolFromSelection(reportResult, out bool selectionShowOnlyTotals, "showOnlyTotals");
            TryGetIncludeInactiveFromSelection(reportResult, out _, out _, out bool? selectionActiveEmployees);
            TryGetIdsFromSelection(reportResult, out List<int> selectionAttestStateIds, "atteststates");

            //Get AccountDims
            AccountDim accountDimStd = AccountManager.GetAccountDimStd(reportResult.Input.ActorCompanyId);
            List<AccountDim> accountDimInternals = AccountManager.GetAccountDimInternalsByCompany(reportResult.Input.ActorCompanyId);
            List<AccountInternalDTO> validAccountInternals = filterOnAccounting && !selectionAccountIds.IsNullOrEmpty() ? AccountManager.GetAccountInternals(reportResult.Input.ActorCompanyId, null).Where(w => selectionAccountIds.Contains(w.AccountId)).ToDTOs() : null;
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, reportResult.ActorCompanyId);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Init document

            XDocument document = reportResult.Input.Document;
            XElement rootElement = reportResult.Input.ElementRoot;
            XElement timePayrollTransactionElement = reportResult.Input.ElementFirst;

            #endregion

            #region ReportHeaderLabels

            XElement reportHeaderLabelsElement = CreateTimeReportHeaderLabelsElement();
            reportHeaderLabelsElement.Add(CreateDateIntervalLabelReportHeaderLabelsElement());
            CreateEnterpriseCurrencyReportHeaderLabelsElement(reportHeaderLabelsElement);
            CreatePayrollProductIntervalReportHeaderLabelsElement(reportHeaderLabelsElement);

            timePayrollTransactionElement.Add(reportHeaderLabelsElement);

            #endregion

            #region ReportHeader

            XElement reportHeaderElement = CreateTimeReportHeaderElement(reportResult);
            reportHeaderElement.Add(CreateDateIntervalElement(selectionDateFrom, selectionDateTo));
            reportHeaderElement.Add(CreateIncludePreliminaryElement(selectionIncludePreliminary));
            this.AddEnterpriseCurrencyPageLabelElements(reportHeaderElement);
            this.AddShowOnlyTotalsReportHeaderElement(reportHeaderElement, selectionShowOnlyTotals);
            timePayrollTransactionElement.Add(reportHeaderElement);

            #endregion

            #region PageHeaderLabels

            XElement pageHeaderLabelsElement = new XElement("PageHeaderLabels");
            CreateTimePayrollTransactionReportPageHeaderLabelsElement(pageHeaderLabelsElement);
            this.AddAccountDimPageHeaderLabelElements(pageHeaderLabelsElement, accountDimStd, accountDimInternals);
            timePayrollTransactionElement.Add(pageHeaderLabelsElement);

            #endregion

            #region Prereq

            DateTime dateFrom = CalendarUtility.GetBeginningOfDay(selectionDateFrom);
            DateTime dateTo = CalendarUtility.GetEndOfDay(selectionDateTo);

            List<PayrollProduct> payrollProducts = new List<PayrollProduct>();
            List<AttestState> attestStates = new List<AttestState>();
            List<AccountStd> accountStds = new List<AccountStd>();
            List<Account> accountInternals = new List<Account>();
            List<TimeCode> timeCodes = new List<TimeCode>();
            List<CompanyCategoryRecord> companyCategoryRecords = new List<CompanyCategoryRecord>();
            List<TimeAccumulator> allAccumulators = new List<TimeAccumulator>();
            List<TimePayrollTransaction> companyTimePayrollTransactions = new List<TimePayrollTransaction>();
            List<TimePayrollScheduleTransaction> companyTimePayrollScheduleTransaction = new List<TimePayrollScheduleTransaction>();
            Dictionary<int, List<EmployeeAccount>> employeeAccountsByEmployee = new Dictionary<int, List<EmployeeAccount>>();
            Dictionary<int, List<TimeScheduleTemplateBlock>> shiftsByEmployee = new Dictionary<int, List<TimeScheduleTemplateBlock>>();
            Dictionary<int, string> timeRulesDict = new Dictionary<int, string>();
            Currency currency = null;

            Parallel.Invoke(() =>
            {
                using (CompEntities entities = new CompEntities())
                {
                    entities.TimePayrollTransaction.NoTracking();
                    entities.TimeCodeTransaction.NoTracking();
                    entities.TimeBlockDate.NoTracking();

                    entities.CommandTimeout = 10 * 60;
                    companyTimePayrollTransactions = (from tpt in entities.TimePayrollTransaction
                                                                         .Include("TimePayrollTransactionExtended")
                                                                         .Include("TimeCodeTransaction")
                                                                         .Include("AccountInternal")
                                                                         .Include("TimeBlockDate")
                                                      where tpt.ActorCompanyId == reportResult.Input.ActorCompanyId &&
                                                      selectionEmployeeIds.Contains(tpt.EmployeeId) &&
                                                      tpt.TimeBlockDate.Date >= dateFrom &&
                                                      tpt.TimeBlockDate.Date <= dateTo &&
                                                      tpt.State == (int)SoeEntityState.Active
                                                      select tpt).ToList();

                    if (filterOnAccounting && !selectionAccountIds.IsNullOrEmpty())
                    {
                        List<TimePayrollTransaction> filtered = new List<TimePayrollTransaction>();
                        foreach (var trans in companyTimePayrollTransactions.Where(w => w.AccountInternal != null))
                        {
                            if (trans.AccountInternal.ValidOnFiltered(validAccountInternals))
                                filtered.Add(trans);
                        }
                        companyTimePayrollTransactions = filtered;
                    }

                    companyTimePayrollTransactions = companyTimePayrollTransactions.OrderBy(o => o.TimeBlockDate.Date).ToList();
                }
            },
            () =>
            {
                if (employees == null)
                    employees = EmployeeManager.GetAllEmployeesByIds(reportResult.Input.ActorCompanyId, selectionEmployeeIds, active: selectionActiveEmployees, loadEmployment: true);
                companyCategoryRecords = CategoryManager.GetCompanyCategoryRecords(SoeCategoryType.Employee, SoeCategoryRecordEntity.Employee, reportResult.Input.ActorCompanyId);
                allAccumulators = TimeAccumulatorManager.GetTimeAccumulators(reportResult.Input.ActorCompanyId);
                payrollProducts = ProductManager.GetPayrollProducts(reportResult.Input.ActorCompanyId, null);
                attestStates = AttestManager.GetAttestStates(reportResult.Input.ActorCompanyId, TermGroup_AttestEntity.PayrollTime, SoeModule.Time);
                accountStds = AccountManager.GetAccountStdsByCompany(reportResult.Input.ActorCompanyId, null);
                accountInternals = AccountManager.GetAccountsInternalsByCompany(reportResult.Input.ActorCompanyId);
                timeCodes = TimeCodeManager.GetTimeCodes(reportResult.Input.ActorCompanyId, null);
                timeRulesDict = TimeRuleManager.GetAllTimeRulesDiscardedStateDict(reportResult.Input.ActorCompanyId);
                currency = CountryCurrencyManager.GetCurrencyFromType(reportResult.Input.ActorCompanyId, TermGroup_CurrencyType.EnterpriseCurrency);
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                employeeAccountsByEmployee = EmployeeManager.GetEmployeeAccounts(entitiesReadOnly, reportResult.Input.ActorCompanyId, selectionEmployeeIds).GroupBy(e => e.EmployeeId).ToDictionary(k => k.Key, v => v.ToList());
            },
            () =>
            {
                using (CompEntities entities = new CompEntities())
                {
                    entities.TimeScheduleTemplateBlock.NoTracking();
                    shiftsByEmployee = TimeScheduleManager.GetTimeScheduleTemplateBlocksForEmployees(entities, selectionEmployeeIds, selectionDateFrom, selectionDateTo).GroupBy(b => b.EmployeeId.Value).ToDictionary(k => k.Key, v => v.ToList());
                }
            },
            () =>
            {
                using (CompEntities entities = new CompEntities())
                {
                    entities.TimePayrollScheduleTransaction.NoTracking();
                    entities.TimeBlockDate.NoTracking();
                    entities.CommandTimeout = 10 * 60;
                    companyTimePayrollScheduleTransaction = (from tpt in entities.TimePayrollScheduleTransaction
                                                                        .Include("TimeBlockDate")
                                                                        .Include("AccountInternal")
                                                             where tpt.ActorCompanyId == reportResult.Input.ActorCompanyId &&
                                                             selectionEmployeeIds.Contains(tpt.EmployeeId) &&
                                                             tpt.TimeBlockDate.Date >= dateFrom &&
                                                             tpt.TimeBlockDate.Date <= dateTo &&
                                                             tpt.State == (int)SoeEntityState.Active &&
                                                             tpt.Type == (int)SoeTimePayrollScheduleTransactionType.Absence
                                                             select tpt).ToList();

                    if (filterOnAccounting && !selectionAccountIds.IsNullOrEmpty())
                    {
                        List<TimePayrollScheduleTransaction> filtered = new List<TimePayrollScheduleTransaction>();
                        foreach (var trans in companyTimePayrollScheduleTransaction.Where(w => w.AccountInternal != null))
                            if (trans.AccountInternal.ValidOnFiltered(validAccountInternals))
                                filtered.Add(trans);

                        companyTimePayrollScheduleTransaction = filtered;
                    }
                    companyTimePayrollScheduleTransaction = companyTimePayrollScheduleTransaction.OrderBy(o => o.TimeBlockDate.Date).ToList();
                }
            });

            #endregion

            #region Content

            ConcurrentBag<string> payrollProductNumbers = new ConcurrentBag<string>();
            bool hasPayrollProductIds = selectionPayrollProductIds != null && selectionPayrollProductIds.Count > 0;

            int employeeXmlId = 1;
            int dayXmlId = 1;
            foreach (int id in selectionEmployeeIds)
            {
                #region Prereq

                Employee employee = employees.FirstOrDefault(i => i.EmployeeId == id);
                if (employee == null)
                    continue;

                if (employee.Vacant || employee.Hidden)
                    continue;

                List<DateTime> employmentDates = employee.GetEmploymentDates(selectionDateFrom, selectionDateTo);
                if (employmentDates.Count == 0)
                    continue;

                EmployeeGroup employeeGroup = employee.GetEmployeeGroup(selectionDateFrom, selectionDateTo, base.personalDataRepository.EmployeeGroups);
                if (employeeGroup == null)
                    continue;

                List<TimePayrollTransaction> timePayrollTransactions = companyTimePayrollTransactions.Where(t => t.EmployeeId == employee.EmployeeId).ToList();
                List<TimePayrollScheduleTransaction> timePayrollScheduleTransactions = companyTimePayrollScheduleTransaction.Where(t => t.EmployeeId == employee.EmployeeId).ToList();

                if (!selectionAttestStateIds.IsNullOrEmpty())
                    timePayrollTransactions = timePayrollTransactions.Where(i => selectionAttestStateIds.Contains(i.AttestStateId)).ToList();
                timePayrollTransactions = timePayrollTransactions.Where(i => employmentDates.Contains(i.TimeBlockDate.Date) || i.PayrollStartValueRowId.HasValue).ToList();
                timePayrollScheduleTransactions = timePayrollScheduleTransactions.Where(i => employmentDates.Contains(i.TimeBlockDate.Date)).ToList();
                if (hasPayrollProductIds)
                {
                    timePayrollTransactions = timePayrollTransactions.Where(i => selectionPayrollProductIds.Contains(i.ProductId)).ToList();
                    timePayrollScheduleTransactions = timePayrollScheduleTransactions.Where(i => selectionPayrollProductIds.Contains(i.ProductId)).ToList();
                }
                if (timePayrollTransactions.Count == 0 && timePayrollScheduleTransactions.Count == 0)
                    continue;

                Dictionary<DateTime, List<TimeScheduleTemplateBlock>> shiftsByDate = shiftsByEmployee.GetList(employee.EmployeeId).Where(s => s.Date.HasValue).GroupBy(s => s.Date.Value).ToDictionary(k => k.Key, v => v.ToList());
                List<EmployeeAccount> employeeAccounts = employeeAccountsByEmployee.GetList(employee.EmployeeId);
                List<CompanyCategoryRecord> employeeCategoryRecords = companyCategoryRecords.GetCategoryRecords(employee.EmployeeId, selectionDateFrom, selectionDateTo, onlyDefaultCategories: true); //Niclas: Shouldnt all categories be included in reports?
                employeeCategoryRecords.GetCodeAndName(out string employeeCategoryCode, out string employeeCategoryName);

                #endregion

                #region Employee element

                XElement employeeElement = new XElement("Employee",
                    new XAttribute("id", employeeXmlId),
                    new XElement("EmployeeNr", employee.EmployeeNr),
                    new XElement("EmployeeName", employee.Name),
                    new XElement("EmployeeGroupName", employeeGroup.Name));
                base.personalDataRepository.AddEmployee(employee, employeeElement);

                #endregion

                #region Day element

                Dictionary<DateTime, int> dayDict = new Dictionary<DateTime, int>();

                foreach (DateTime date in CalendarUtility.GetDatesInInterval(selectionDateFrom, selectionDateTo))
                {
                    List<TimeScheduleTemplateBlock> shiftsForDate = shiftsByDate.GetList(date);
                    List<int> accountIds = employeeAccounts.GetEmployeeAccounts(date).Where(ea => ea.AccountId.HasValue).Select(ea => ea.AccountId.Value).ToList();
                    List<Account> accounts = accountInternals.Where(x => accountIds.Contains(x.AccountId)).ToList();

                    XElement dayElement = new XElement("Day",
                        new XAttribute("id", dayXmlId),
                        new XElement("Date", date),
                        new XElement("Length", shiftsForDate.GetWorkMinutes()),
                        new XElement("EmployeeAccounts", accounts.Select(x => x.Name).ToCommaSeparated()));
                    employeeElement.Add(dayElement);

                    dayDict.Add(date, dayXmlId);
                    dayXmlId++;
                }

                #endregion

                #region Transactions

                int payrollTransactionXmlId = 0;
                ConcurrentDictionary<string, XElement> transactionElements = new ConcurrentDictionary<string, XElement>();
                Parallel.Invoke(() =>
                {
                    Parallel.ForEach(timePayrollTransactions.GroupBy(g => g.TimePayrollTransactionId), payrollTransactionGroup =>
                    {
                        int xmlId = Interlocked.Increment(ref payrollTransactionXmlId);

                        TimePayrollTransaction payrollTransaction = payrollTransactionGroup.First();
                        PayrollProduct payrollProduct = payrollProducts.FirstOrDefault(f => f.ProductId == payrollTransaction.ProductId);
                        if (payrollProduct != null && dayDict.ContainsKey(payrollTransaction.TimeBlockDate.Date))
                        {
                            #region TimePayrollTransaction

                            bool isExtended = payrollTransaction.IsExtended && payrollTransaction.TimePayrollTransactionExtended != null;
                            bool hasTimeCode = payrollTransaction.TimeCodeTransactionId.HasValue;
                            bool hasTimeRule = hasTimeCode && payrollTransaction.TimeCodeTransaction.TimeRuleId.HasValue;
                            bool isManuallyAddedTime = payrollTransaction.ManuallyAdded && !payrollTransaction.IsAdded;
                            bool isManuallyAddedPayroll = payrollTransaction.ManuallyAdded && payrollTransaction.IsAdded;
                            if (hasPayrollProductIds && !payrollProductNumbers.Contains(payrollProduct.Number))
                                payrollProductNumbers.Add(payrollProduct.Number);

                            AccountStd accountStd = accountStds.FirstOrDefault(f => f.AccountId == payrollTransaction.AccountStdId);
                            AttestState attestState = attestStates.FirstOrDefault(f => f.AttestStateId == payrollTransaction.AttestStateId);
                            TimeCode timeCode = hasTimeCode ? timeCodes.FirstOrDefault(f => f.TimeCodeId == payrollTransaction.TimeCodeTransaction.TimeCodeId) : null;
                            if (timeCode == null)
                                hasTimeCode = false;
                            KeyValuePair<int, string> timeRule = hasTimeRule ? timeRulesDict.FirstOrDefault(f => f.Key == payrollTransaction.TimeCodeTransaction.TimeRuleId) : new KeyValuePair<int, string>(0, "");
                            if (timeRule.Key == 0)
                                hasTimeRule = false;

                            string payrollTypeLevel3Name = string.Empty;
                            if (payrollTransaction.SysPayrollTypeLevel3.HasValue)
                            {
                                if (payrollTransaction.IsTimeAccumulator())
                                    payrollTypeLevel3Name = allAccumulators.FirstOrDefault(a => a.TimeAccumulatorId == payrollTransaction.SysPayrollTypeLevel3.Value)?.Name ?? string.Empty;
                                else
                                    payrollTypeLevel3Name = GetText(payrollTransaction.SysPayrollTypeLevel3.Value, (int)TermGroup.SysPayrollType);
                            }

                            XElement transactionElement = new XElement("PayrollTransaction",
                                new XAttribute("id", xmlId),
                                new XElement("DayId", dayDict.GetValue(payrollTransaction.TimeBlockDate.Date)),
                                new XElement("EmployeeCategoryCode", employeeCategoryCode),
                                new XElement("EmployeeCategoryName", employeeCategoryName),
                                new XElement("PayrollTransactionQuantity", payrollTransactionGroup.Sum(s => s.Quantity)),
                                new XElement("PayrollTransactionQuantityWorkDays", isExtended ? payrollTransaction.TimePayrollTransactionExtended.QuantityWorkDays : 0),
                                new XElement("PayrollTransactionQuantityCalendarDays", isExtended ? payrollTransaction.TimePayrollTransactionExtended.QuantityCalendarDays : 0),
                                new XElement("PayrollTransactionCalenderDayFactor", isExtended ? payrollTransaction.TimePayrollTransactionExtended.CalenderDayFactor : 0),
                                new XElement("PayrollTransactionTimeUnit", isExtended ? payrollTransaction.TimePayrollTransactionExtended.TimeUnit : 0),
                                new XElement("PayrollTransactionTimeUnitName", isExtended ? GetText(payrollTransaction.TimePayrollTransactionExtended.TimeUnit, (int)TermGroup.PayrollProductTimeUnit) : string.Empty),
                                new XElement("IsRegistrationTypeQuantity", timeCode?.IsRegistrationTypeQuantity.ToInt() ?? 0),
                                new XElement("PayrollTransactionUnitPrice", payrollTransaction.UnitPrice ?? 0),
                                new XElement("PayrollTransactionUnitPriceCurrency", payrollTransaction.UnitPriceCurrency.HasValue ? payrollTransaction.UnitPrice.Value : 0),
                                new XElement("PayrollTransactionUnitPriceEntCurrency", payrollTransaction.UnitPriceEntCurrency.HasValue ? payrollTransaction.UnitPrice.Value : 0),
                                new XElement("PayrollTransactionAmount", payrollTransactionGroup.Where(w => w.Amount.HasValue).Sum(s => s.Amount)),
                                new XElement("PayrollTransactionAmountCurrency", payrollTransactionGroup.Where(w => w.AmountCurrency.HasValue).Sum(s => s.AmountCurrency)),
                                new XElement("PayrollTransactionAmountEntCurrency", payrollTransactionGroup.Where(w => w.AmountEntCurrency.HasValue).Sum(s => s.AmountEntCurrency)),
                                new XElement("PayrollTransactionVATAmount", payrollTransactionGroup.Where(w => w.VatAmount.HasValue).Sum(s => s.VatAmount)),
                                new XElement("PayrollTransactionVATAmountCurrency", payrollTransactionGroup.Where(w => w.VatAmountCurrency.HasValue).Sum(s => s.VatAmountCurrency)),
                                new XElement("PayrollTransactionVATAmountEntCurrency", payrollTransactionGroup.Where(w => w.VatAmountEntCurrency.HasValue).Sum(s => s.VatAmountEntCurrency)),
                                new XElement("PayrollTransactionDate", payrollTransaction.TimeBlockDate.Date),
                                new XElement("PayrollTransactionExported", payrollTransaction.Exported.ToInt()),
                                new XElement("AttestStateName", attestState != null ? attestState.Name : string.Empty),
                                new XElement("PayrollProductNumber", payrollProduct.Number),
                                new XElement("PayrollProductName", payrollProduct.Name),
                                new XElement("PayrollProductDescription", payrollProduct.Description),
                                new XElement("PayrollType", 0), //Deprecated but cannot delete from XML due to existing customer specific reports
                                new XElement("PayrollTypeName", string.Empty), //Deprecated but cannot delete from XML due to existing customer specific reports
                                new XElement("PayrollTypeLevel1", payrollTransaction.SysPayrollTypeLevel1.HasValue ? GetText(payrollTransaction.SysPayrollTypeLevel1.Value, (int)TermGroup.SysPayrollType) : string.Empty),
                                new XElement("PayrollTypeLevel2", payrollTransaction.SysPayrollTypeLevel2.HasValue ? GetText(payrollTransaction.SysPayrollTypeLevel2.Value, (int)TermGroup.SysPayrollType) : string.Empty),
                                new XElement("PayrollTypeLevel3", payrollTypeLevel3Name),
                                new XElement("PayrollTypeLevel4", payrollTransaction.SysPayrollTypeLevel4.HasValue ? GetText(payrollTransaction.SysPayrollTypeLevel4.Value, (int)TermGroup.SysPayrollType) : string.Empty),
                                new XElement("IsPreliminary", payrollTransaction.IsPreliminary.ToInt()),
                                new XElement("IsFixed", payrollTransaction.IsFixed.ToInt()),
                                new XElement("IsManuallyAddedPayroll", isManuallyAddedPayroll.ToInt()),
                                new XElement("IsManuallyAddedTime", isManuallyAddedTime.ToInt()),
                                new XElement("CurrencyName", currency?.Name ?? string.Empty),
                                new XElement("CurrencyCode", currency?.Code ?? string.Empty),
                                new XElement("Note", StringUtility.NullToEmpty(payrollTransaction.Comment)),
                                new XElement("Formula", isExtended ? StringUtility.NullToEmpty(payrollTransaction.TimePayrollTransactionExtended.Formula) : string.Empty),
                                new XElement("FormulaExtracted", isExtended ? StringUtility.NullToEmpty(payrollTransaction.TimePayrollTransactionExtended.Formula) : string.Empty),
                                new XElement("FormulaNames", isExtended ? StringUtility.NullToEmpty(payrollTransaction.TimePayrollTransactionExtended.FormulaNames) : string.Empty),
                                new XElement("FormulaOrigin", isExtended ? StringUtility.NullToEmpty(payrollTransaction.TimePayrollTransactionExtended.FormulaOrigin) : string.Empty),
                                new XElement("PayrollCalculationPerformed", isExtended ? payrollTransaction.TimePayrollTransactionExtended.PayrollCalculationPerformed.ToInt() : 0),
                                new XElement("Created", payrollTransaction.Created.ToValueOrDefault()),
                                new XElement("Modified", payrollTransaction.Modified.ToValueOrDefault()),
                                new XElement("CreatedBy", StringUtility.NullToEmpty(payrollTransaction.CreatedBy)),
                                new XElement("ModfiedBy", StringUtility.NullToEmpty(payrollTransaction.ModifiedBy)),
                                new XElement("TimeCodeName", timeCode?.Name ?? string.Empty),
                                new XElement("TimeCodeCode", timeCode?.Code ?? string.Empty),
                                new XElement("TimeRuleName", (hasTimeRule ? timeRule.Value + " (" + timeRule.Key.ToString() + ")" : string.Empty)),
                                new XElement("AccountNr", accountStd?.Account.AccountNr ?? string.Empty),
                                new XElement("AccountName", accountStd?.Account.Name ?? string.Empty));

                            for (int i = 1; i <= Constants.NOOFDIMENSIONS; i++)
                            {
                                if (i > accountDimInternals.Count)
                                {
                                    transactionElement.Add(
                                        new XElement("AccountInternalNr" + i, string.Empty),
                                        new XElement("AccountInternalName" + i, string.Empty));
                                    continue;
                                }

                                AccountInternal accountInternal = null;
                                AccountDim accountDim = accountDimInternals.Any() ? accountDimInternals.ElementAt(i - 1) : null;
                                if (accountDim != null && !payrollTransaction.AccountInternal.IsNullOrEmpty())
                                {
                                    var accountsOnDim = accountInternals.Where(w => w.AccountDimId == accountDim.AccountDimId);
                                    if (accountsOnDim.Any())
                                    {
                                        List<int> ids = accountsOnDim.Select(s => s.AccountId).ToList();

                                        accountInternal = (from ai in payrollTransaction.AccountInternal
                                                           where ids.Contains(ai.AccountId)
                                                           select ai).FirstOrDefault();
                                    }
                                }

                                if (accountInternal != null)
                                {
                                    Account account = accountInternals.FirstOrDefault(f => f.AccountId == accountInternal.AccountId);

                                    transactionElement.Add(
                                        new XElement("AccountInternalNr" + i, account?.AccountNr ?? string.Empty),
                                        new XElement("AccountInternalName" + i, account?.Name ?? string.Empty));
                                }
                                else
                                {
                                    transactionElement.Add(
                                        new XElement("AccountInternalNr" + i, string.Empty),
                                        new XElement("AccountInternalName" + i, string.Empty));
                                }
                            }

                            #endregion

                            transactionElements.TryAdd(payrollTransaction.TimeBlockDate.Date.ToShortDateString() + xmlId, transactionElement);
                        }
                    });
                },
                () =>
                {
                    Parallel.ForEach(timePayrollScheduleTransactions.GroupBy(g => g.TimePayrollScheduleTransactionId), payrollScheduleTransactionGroup =>
                    {
                        int xmlId = Interlocked.Increment(ref payrollTransactionXmlId);

                        TimePayrollScheduleTransaction payrollScheduleTransaction = payrollScheduleTransactionGroup.First();
                        PayrollProduct payrollProduct = payrollProducts.FirstOrDefault(f => f.ProductId == payrollScheduleTransaction.ProductId);
                        if (payrollProduct != null)
                        {
                            #region TimePayrollScheduleTransaction

                            AccountStd accountStd = accountStds.FirstOrDefault(f => f.AccountId == payrollScheduleTransaction.AccountStdId);

                            if (hasPayrollProductIds && !payrollProductNumbers.Contains(payrollProduct.Number))
                                payrollProductNumbers.Add(payrollProduct.Number);

                            bool isManuallyAddedTime = false;
                            bool isManuallyAddedPayroll = false;

                            string payrollTypeLevel3Name = string.Empty;
                            if (payrollScheduleTransaction.IsTimeAccumulator())
                            {
                                if (payrollScheduleTransaction.SysPayrollTypeLevel3.HasValue)
                                    payrollTypeLevel3Name = allAccumulators.FirstOrDefault(a => a.TimeAccumulatorId == payrollScheduleTransaction.SysPayrollTypeLevel3.Value)?.Name ?? string.Empty;
                            }
                            else
                                payrollTypeLevel3Name = payrollScheduleTransaction.SysPayrollTypeLevel3.HasValue ? GetText(payrollScheduleTransaction.SysPayrollTypeLevel3.Value, (int)TermGroup.SysPayrollType) : string.Empty;

                            XElement transactionElement = new XElement("PayrollTransaction",
                                new XAttribute("id", xmlId),
                                new XElement("EmployeeCategoryCode", employeeCategoryCode),
                                new XElement("EmployeeCategoryName", employeeCategoryName),
                                new XElement("PayrollTransactionQuantity", payrollScheduleTransactionGroup.Sum(s => s.Quantity)),
                                new XElement("PayrollTransactionQuantityWorkDays", 0),
                                new XElement("PayrollTransactionQuantityCalendarDays", 0),
                                new XElement("PayrollTransactionCalenderDayFactor", 0),
                                new XElement("PayrollTransactionTimeUnit", 0),
                                new XElement("PayrollTransactionTimeUnitName", string.Empty),
                                new XElement("IsRegistrationTypeQuantity", 0),
                                new XElement("PayrollTransactionUnitPrice", payrollScheduleTransaction.UnitPrice ?? 0),
                                new XElement("PayrollTransactionUnitPriceCurrency", payrollScheduleTransaction.UnitPriceCurrency.HasValue ? payrollScheduleTransaction.UnitPrice.Value : 0),
                                new XElement("PayrollTransactionUnitPriceEntCurrency", payrollScheduleTransaction.UnitPriceEntCurrency.HasValue ? payrollScheduleTransaction.UnitPrice.Value : 0),
                                new XElement("PayrollTransactionAmount", payrollScheduleTransactionGroup.Where(w => w.Amount.HasValue).Sum(s => s.Amount)),
                                new XElement("PayrollTransactionAmountCurrency", payrollScheduleTransactionGroup.Where(w => w.AmountCurrency.HasValue).Sum(s => s.AmountCurrency)),
                                new XElement("PayrollTransactionAmountEntCurrency", payrollScheduleTransactionGroup.Where(w => w.AmountEntCurrency.HasValue).Sum(s => s.AmountEntCurrency)),
                                new XElement("PayrollTransactionVATAmount", payrollScheduleTransactionGroup.Where(w => w.VatAmount.HasValue).Sum(s => s.VatAmount)),
                                new XElement("PayrollTransactionVATAmountCurrency", payrollScheduleTransactionGroup.Where(w => w.VatAmountCurrency.HasValue).Sum(s => s.VatAmountCurrency)),
                                new XElement("PayrollTransactionVATAmountEntCurrency", payrollScheduleTransactionGroup.Where(w => w.VatAmountEntCurrency.HasValue).Sum(s => s.VatAmountEntCurrency)),
                                new XElement("PayrollTransactionDate", payrollScheduleTransaction.TimeBlockDate.Date),
                                new XElement("PayrollTransactionExported", 0),
                                new XElement("AttestStateName", string.Empty),
                                new XElement("PayrollProductNumber", payrollProduct.Number),
                                new XElement("PayrollProductName", payrollProduct.Name),
                                new XElement("PayrollProductDescription", payrollProduct.Description),
                                new XElement("PayrollType", 0), //Deprecated but cannot delete from XML due to existing customer specific reports
                                new XElement("PayrollTypeName", string.Empty), //Deprecated but cannot delete from XML due to existing customer specific reports
                                new XElement("PayrollTypeLevel1", payrollScheduleTransaction.SysPayrollTypeLevel1.HasValue ? GetText(payrollScheduleTransaction.SysPayrollTypeLevel1.Value, (int)TermGroup.SysPayrollType) : string.Empty),
                                new XElement("PayrollTypeLevel2", payrollScheduleTransaction.SysPayrollTypeLevel2.HasValue ? GetText(payrollScheduleTransaction.SysPayrollTypeLevel2.Value, (int)TermGroup.SysPayrollType) : string.Empty),
                                new XElement("PayrollTypeLevel3", payrollTypeLevel3Name),
                                new XElement("PayrollTypeLevel4", payrollScheduleTransaction.SysPayrollTypeLevel4.HasValue ? GetText(payrollScheduleTransaction.SysPayrollTypeLevel4.Value, (int)TermGroup.SysPayrollType) : string.Empty),
                                new XElement("IsPreliminary", 0),
                                new XElement("IsFixed", 0),
                                new XElement("IsManuallyAddedPayroll", isManuallyAddedPayroll.ToInt()),
                                new XElement("IsManuallyAddedTime", isManuallyAddedTime.ToInt()),
                                new XElement("CurrencyName", currency?.Name ?? string.Empty),
                                new XElement("CurrencyCode", currency?.Code ?? string.Empty),
                                new XElement("Note", "ScheduleTransaction"),
                                new XElement("Formula", string.Empty),
                                new XElement("FormulaExtracted", string.Empty),
                                new XElement("FormulaNames", string.Empty),
                                new XElement("FormulaOrigin", string.Empty),
                                new XElement("PayrollCalculationPerformed", 0),
                                new XElement("Created", payrollScheduleTransaction.Created.ToValueOrDefault()),
                                new XElement("Modified", payrollScheduleTransaction.Modified.ToValueOrDefault()),
                                new XElement("CreatedBy", StringUtility.NullToEmpty(payrollScheduleTransaction.CreatedBy)),
                                new XElement("ModfiedBy", StringUtility.NullToEmpty(payrollScheduleTransaction.ModifiedBy)),
                                new XElement("TimeCodeName", string.Empty),
                                new XElement("TimeCodeCode", string.Empty),
                                new XElement("TimeRuleName", string.Empty),
                                new XElement("AccountNr", accountStd.Account.AccountNr),
                                new XElement("AccountName", accountStd.Account.Name));

                            for (int i = 1; i <= Constants.NOOFDIMENSIONS; i++)
                            {
                                if (i > accountDimInternals.Count)
                                {
                                    transactionElement.Add(
                                        new XElement("AccountInternalNr" + i, string.Empty),
                                        new XElement("AccountInternalName" + i, string.Empty));
                                    continue;
                                }

                                AccountInternal accountInternal = null;
                                AccountDim accountDim = accountDimInternals.Any() ? accountDimInternals.ElementAt(i - 1) : null;
                                if (accountDim != null && payrollScheduleTransaction.AccountInternal != null && payrollScheduleTransaction.AccountInternal.Any())
                                {
                                    var accountsOnDim = accountInternals.Where(w => w.AccountDimId == accountDim.AccountDimId);
                                    if (accountsOnDim.Any())
                                    {
                                        List<int> ids = accountsOnDim.Select(s => s.AccountId).ToList();

                                        accountInternal = (from ai in payrollScheduleTransaction.AccountInternal
                                                           where ids.Contains(ai.AccountId)
                                                           select ai).FirstOrDefault();
                                    }
                                }

                                if (accountInternal != null)
                                {
                                    Account account = accountInternals.FirstOrDefault(f => f.AccountId == accountInternal.AccountId);

                                    transactionElement.Add(
                                        new XElement("AccountInternalNr" + i, account?.AccountNr ?? string.Empty),
                                        new XElement("AccountInternalName" + i, account?.Name ?? string.Empty));
                                }
                                else
                                {
                                    transactionElement.Add(
                                        new XElement("AccountInternalNr" + i, string.Empty),
                                        new XElement("AccountInternalName" + i, string.Empty));
                                }
                            }

                            #endregion

                            transactionElements.TryAdd(payrollScheduleTransaction.TimeBlockDate.Date.ToShortDateString() + xmlId, transactionElement);
                        }
                    });
                });

                foreach (var transactionElement in transactionElements.OrderBy(k => k.Key))
                {
                    employeeElement.Add(transactionElement.Value);
                }

                #endregion

                timePayrollTransactionElement.Add(employeeElement);
                employeeXmlId++;
            }

            #region PayrollProductInterval element

            reportHeaderElement.Add(new XElement("PayrollProductInterval", hasPayrollProductIds ? payrollProductNumbers.OrderBy(nr => nr).ToCommaSeparated() : GetReportText(773, "Alla")));
            this.AddUseAccountHierarchyReportHeaderElement(reportHeaderElement, useAccountHierarchy);

            #endregion

            #region Default element

            if (employeeXmlId == 1)
                AddDefaultElement(reportResult, timePayrollTransactionElement, "Employee");

            #endregion

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            #region Close document

            rootElement.Add(timePayrollTransactionElement);
            document.Add(rootElement);

            return GetValidatedDocument(document, reportResult);

            #endregion
        }

        public XDocument CreateTimePayrollTransactionSmallReportData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            if (!TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out List<Employee> employees, out List<int> selectionEmployeeIds, out List<int> selectionAccountIds, out _))
                return null;

            TryGetBoolFromSelection(reportResult, out bool filterOnAccounting, "filterOnAccounting");
            TryGetPayrollProductIdsFromSelections(reportResult, out List<int> selectionPayrollProductIds);
            TryGetBoolFromSelection(reportResult, out bool selectionIncludePreliminary, "includePreliminary");
            TryGetBoolFromSelection(reportResult, out bool selectionShowOnlyTotals, "showOnlyTotals");
            TryGetIncludeInactiveFromSelection(reportResult, out bool _, out _, out bool? selectionActiveEmployees);
            TryGetIdsFromSelection(reportResult, out List<int> selectionAttestStateIds, "atteststates");

            //Get AccountDims
            AccountDim accountDimStd = AccountManager.GetAccountDimStd(reportResult.Input.ActorCompanyId);
            List<AccountDim> accountDimInternals = AccountManager.GetAccountDimInternalsByCompany(reportResult.Input.ActorCompanyId);
            List<AccountInternalDTO> validAccountInternals = filterOnAccounting && !selectionAccountIds.IsNullOrEmpty() ? AccountManager.GetAccountInternals(reportResult.Input.ActorCompanyId, null).Where(w => selectionAccountIds.Contains(w.AccountId)).ToDTOs() : null;

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, reportResult.ActorCompanyId);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Init document

            XDocument document = reportResult.Input.Document;
            XElement rootElement = reportResult.Input.ElementRoot;
            XElement timePayrollTransactionElement = reportResult.Input.ElementFirst;

            #endregion

            #region ReportHeaderLabels

            XElement reportHeaderLabelsElement = CreateTimeReportHeaderLabelsElement();
            reportHeaderLabelsElement.Add(CreateDateIntervalLabelReportHeaderLabelsElement());
            CreateEnterpriseCurrencyReportHeaderLabelsElement(reportHeaderLabelsElement);
            CreatePayrollProductIntervalReportHeaderLabelsElement(reportHeaderLabelsElement);
            timePayrollTransactionElement.Add(reportHeaderLabelsElement);

            #endregion

            #region ReportHeader

            XElement reportHeaderElement = CreateTimeReportHeaderElement(reportResult);
            reportHeaderElement.Add(CreateDateIntervalElement(selectionDateFrom, selectionDateTo));
            reportHeaderElement.Add(CreateIncludePreliminaryElement(selectionIncludePreliminary));
            this.AddEnterpriseCurrencyPageLabelElements(reportHeaderElement);
            this.AddShowOnlyTotalsReportHeaderElement(reportHeaderElement, selectionShowOnlyTotals);
            timePayrollTransactionElement.Add(reportHeaderElement);

            #endregion

            #region PageHeaderLabels

            XElement pageHeaderLabelsElement = new XElement("PageHeaderLabels");
            CreateTimePayrollTransactionReportPageHeaderLabelsElement(pageHeaderLabelsElement);
            this.AddAccountDimPageHeaderLabelElements(pageHeaderLabelsElement, accountDimStd, accountDimInternals);
            timePayrollTransactionElement.Add(pageHeaderLabelsElement);

            #endregion

            #region Prereq

            List<EmployeeGroup> employeeGroups = new List<EmployeeGroup>();
            List<SysPayrollPriceViewDTO> sysPayrollPrices = new List<SysPayrollPriceViewDTO>();
            List<PayrollProduct> payrollProducts = new List<PayrollProduct>();
            List<CompanyCategoryRecord> companyCategoryRecords = new List<CompanyCategoryRecord>();
            List<EmployeeAccount> companyEmployeeAccounts = new List<EmployeeAccount>();
            List<TimeAccumulator> allAccumulators = new List<TimeAccumulator>();
            List<AttestState> attestStates = new List<AttestState>();
            List<AccountStd> accountStds = new List<AccountStd>();
            List<Account> accountInternals = new List<Account>();
            Currency currency = null;
            Dictionary<int, string> levelDict = new Dictionary<int, string>();
            Dictionary<int, int> productDict = new Dictionary<int, int>();
            Dictionary<int, int> attestStateDict = new Dictionary<int, int>();
            Dictionary<int, List<TimePayrollTransactionDTO>> companyTimePayrollTransactionDTOs = new Dictionary<int, List<TimePayrollTransactionDTO>>();
            Dictionary<int, List<TimePayrollTransactionDTO>> companyTimePayrollScheduleTransaction = new Dictionary<int, List<TimePayrollTransactionDTO>>();
            Dictionary<int, List<TimeScheduleTemplateBlock>> companyShifts = new Dictionary<int, List<TimeScheduleTemplateBlock>>();
            List<Tuple<int, DateTime, decimal>> employmentTaxRates = new List<Tuple<int, DateTime, decimal>>();

            int sysCountryId = base.GetCompanySysCountryIdFromCache(entities, base.ActorCompanyId);

            Parallel.Invoke(() =>
            {
                companyTimePayrollTransactionDTOs = TimeTransactionManager.GetTimePayrollTransactionDTOForReport(selectionDateFrom, selectionDateTo, selectionEmployeeIds, reportResult.Input.ActorCompanyId);
            },
            () =>
            {
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                if (employees == null)
                    employees = EmployeeManager.GetAllEmployeesByIds(reportResult.Input.ActorCompanyId, selectionEmployeeIds, active: selectionActiveEmployees, loadEmployment: true);
                sysPayrollPrices = PayrollManager.GetSysPayrollPriceView(sysCountryId);
                employmentTaxRates = PayrollManager.GetRatesFromPayrollPriceView(entitiesReadOnly, reportResult.Input.ActorCompanyId, selectionDateFrom, selectionDateTo, employees.GetBirthyears(), sysPayrollPrices, TermGroup_SysPayrollPrice.SE_EmploymentTax, sysCountryId);
                companyCategoryRecords = CategoryManager.GetCompanyCategoryRecords(SoeCategoryType.Employee, SoeCategoryRecordEntity.Employee, reportResult.Input.ActorCompanyId);
                allAccumulators = TimeAccumulatorManager.GetTimeAccumulators(reportResult.Input.ActorCompanyId);
                payrollProducts = ProductManager.GetPayrollProducts(reportResult.Input.ActorCompanyId, null);
                attestStates = AttestManager.GetAttestStates(reportResult.Input.ActorCompanyId, TermGroup_AttestEntity.PayrollTime, SoeModule.Time);
                accountStds = AccountManager.GetAccountStdsByCompanyIgnoreState(entitiesReadOnly, reportResult.Input.ActorCompanyId);
                accountInternals = AccountManager.GetAccountsInternalsByCompany(reportResult.Input.ActorCompanyId);
                currency = CountryCurrencyManager.GetCurrencyFromType(reportResult.Input.ActorCompanyId, TermGroup_CurrencyType.EnterpriseCurrency);
                levelDict = base.GetTermGroupDict(TermGroup.SysPayrollType);
                companyEmployeeAccounts = EmployeeManager.GetEmployeeAccounts(entitiesReadOnly, reportResult.Input.ActorCompanyId, selectionEmployeeIds);

                #region Sales element
                timePayrollTransactionElement.Add(CreateSalesElementFromFrequency(entitiesReadOnly, reportResult.Input.ActorCompanyId, reportResult.Input.RoleId, selectionDateFrom, selectionDateTo));

                #endregion

                #region Products element

                int productXmlId = 1;
                foreach (var product in payrollProducts)
                {
                    timePayrollTransactionElement.Add(new XElement("Products",
                        new XAttribute("id", productXmlId),
                        new XElement("ProductNr", product.Number),
                        new XElement("ProductName", product.Name),
                        new XElement("L1", product.SysPayrollTypeLevel1.HasValue ? product.SysPayrollTypeLevel1.Value.ToString() : "0"),
                        new XElement("L2", product.SysPayrollTypeLevel2.HasValue ? product.SysPayrollTypeLevel2.Value.ToString() : "0"),
                        new XElement("L3", product.SysPayrollTypeLevel3.HasValue ? product.SysPayrollTypeLevel3.Value.ToString() : "0"),
                        new XElement("L4", product.SysPayrollTypeLevel4.HasValue ? product.SysPayrollTypeLevel4.Value.ToString() : "0")));

                    productDict.Add(product.ProductId, productXmlId);
                    productXmlId++;
                }

                #endregion

                #region AttestStates element

                int attestStateXmlId = 1;
                foreach (var attestState in attestStates)
                {
                    timePayrollTransactionElement.Add(new XElement("AttestStates",
                        new XAttribute("id", attestStateXmlId),
                        new XElement("Name", attestState.Name)));
                    attestStateDict.Add(attestState.AttestStateId, attestStateXmlId);
                    attestStateXmlId++;
                }

                timePayrollTransactionElement.Add(new XElement("AttestStates",
                       new XAttribute("id", 0),
                       new XElement("Name", string.Empty)));
                attestStateDict.Add(0, 0);

                #endregion

                #region Level element

                timePayrollTransactionElement.Add(new XElement("Level",
               new XAttribute("id", ""),
               new XElement("Name", "")));

                foreach (var ld in levelDict)
                {
                    timePayrollTransactionElement.Add(new XElement("Level",
                        new XAttribute("id", ld.Key.ToString()),
                        new XElement("Name", ld.Value)));
                }

                foreach (var ld in allAccumulators)
                {
                    int id = ld.TimeAccumulatorId * -1;
                    timePayrollTransactionElement.Add(new XElement("Level",
                        new XAttribute("id", id),
                        new XElement("Name", ld.Name)));
                }

                #endregion

                #region Account element

                foreach (var ai in accountInternals)
                {
                    timePayrollTransactionElement.Add(new XElement("Account",
                        new XAttribute("id", ai.AccountId),
                        new XElement("Nr", ai.AccountNr),
                        new XElement("Name", ai.Name)));
                }
                foreach (var a in accountStds)
                {
                    timePayrollTransactionElement.Add(new XElement("Account",
                        new XAttribute("id", a.Account.AccountId),
                        new XElement("Nr", a.Account.AccountNr),
                        new XElement("Name", a.Account.Name)));
                }

                timePayrollTransactionElement.Add(new XElement("Account",
                        new XAttribute("id", 0),
                        new XElement("Nr", string.Empty),
                        new XElement("Name", string.Empty)));

                #endregion
            },
            () =>
            {
                using (CompEntities entities = new CompEntities())
                {
                    entities.TimeScheduleTemplateBlock.NoTracking();
                    var allShifts = TimeScheduleManager.GetTimeScheduleTemplateBlocksForEmployees(entities, selectionEmployeeIds, selectionDateFrom, selectionDateTo);

                    foreach (var item in allShifts.GroupBy(g => g.EmployeeId))
                        companyShifts.Add(item.Key.Value, item.ToList());

                    employeeGroups = base.GetEmployeeGroupsFromCache(entities, CacheConfig.Company(reportResult.Input.ActorCompanyId));
                }
            },
            () =>
            {
                companyTimePayrollScheduleTransaction = TimeTransactionManager.GetTimePayrollScheduleTransactionDTOForReport(selectionDateFrom, selectionDateTo, selectionEmployeeIds, reportResult.Input.ActorCompanyId);
            });

            #endregion

            #region Content

            ConcurrentBag<string> payrollProductNumbers = new ConcurrentBag<string>();
            bool hasPayrollProductIds = selectionPayrollProductIds != null && selectionPayrollProductIds.Count > 0;

            int employeeXmlId = 1;
            int dayXmlId = 1;
            foreach (int id in selectionEmployeeIds)
            {
                #region Prereq

                Employee employee = employees.FirstOrDefault(i => i.EmployeeId == id);
                if (employee == null)
                    continue;

                List<DateTime> employmentDates = employee.GetEmploymentDates(selectionDateFrom, selectionDateTo);
                if (employmentDates.Count == 0)
                    continue;

                EmployeeGroup employeeGroup = employee.GetEmployeeGroup(selectionDateFrom, selectionDateTo, employeeGroups);
                if (employeeGroup == null)
                    continue;

                List<TimePayrollTransactionDTO> timePayrollTransactions = new List<TimePayrollTransactionDTO>();
                List<TimePayrollTransactionDTO> timePayrollScheduleTransaction = new List<TimePayrollTransactionDTO>();
                List<TimeScheduleTemplateBlock> shifts = new List<TimeScheduleTemplateBlock>();

                var valuetpt = companyTimePayrollTransactionDTOs.FirstOrDefault(f => f.Key == employee.EmployeeId);
                if (valuetpt.Value != null)
                    timePayrollTransactions = valuetpt.Value;

                var valuetst = companyTimePayrollScheduleTransaction.FirstOrDefault(f => f.Key == employee.EmployeeId);
                if (valuetst.Value != null)
                    timePayrollScheduleTransaction = valuetst.Value;

                var valueShift = companyShifts.FirstOrDefault(f => f.Key == employee.EmployeeId);
                if (valueShift.Value != null)
                    shifts = valueShift.Value;

                List<EmployeeAccount> currentEmployeeAccounts = (from ea in companyEmployeeAccounts
                                                                 where ea.EmployeeId == employee.EmployeeId
                                                                 select ea).ToList();

                if (!selectionAttestStateIds.IsNullOrEmpty())
                    timePayrollTransactions = timePayrollTransactions.Where(i => selectionAttestStateIds.Contains(i.AttestStateId)).ToList();

                timePayrollTransactions = timePayrollTransactions.Where(i => (i.Date.HasValue && employmentDates.Contains(i.Date.Value)) || i.PayrollStartValueRowId.HasValue).ToList();
                timePayrollScheduleTransaction = timePayrollScheduleTransaction.Where(i => (i.Date.HasValue && employmentDates.Contains(i.Date.Value))).ToList();
                if (hasPayrollProductIds)
                {
                    timePayrollTransactions = timePayrollTransactions.Where(i => selectionPayrollProductIds.Contains(i.PayrollProductId)).ToList();
                    timePayrollScheduleTransaction = timePayrollScheduleTransaction.Where(i => selectionPayrollProductIds.Contains(i.PayrollProductId)).ToList();

                }
                if (timePayrollTransactions.Count == 0 && timePayrollScheduleTransaction.Count == 0)
                    continue;

                int birthYear = CalendarUtility.GetBirthYearFromSecurityNumber(employee.SocialSec);

                #endregion

                #region Employee element

                XElement employeeElement = new XElement("Employee",
                    new XAttribute("id", employeeXmlId),
                    new XElement("EmployeeNr", employee.EmployeeNr),
                    new XElement("EmployeeName", employee.Name),
                    new XElement("EmployeeGroupName", employeeGroup.Name));
                base.personalDataRepository.AddEmployee(employee, employeeElement);

                #endregion

                #region Day

                Dictionary<DateTime, int> dayDict = new Dictionary<DateTime, int>();

                foreach (DateTime day in CalendarUtility.GetDatesInInterval(selectionDateFrom, selectionDateTo))
                {
                    List<int> accountIds = currentEmployeeAccounts.GetEmployeeAccounts(day).Where(ea => ea.AccountId.HasValue).Select(ea => ea.AccountId.Value).ToList();
                    List<Account> accounts = accountInternals.Where(x => accountIds.Contains(x.AccountId)).ToList();
                    decimal etx = decimal.Multiply(PayrollManager.GetTaxRate(day, birthYear, employmentTaxRates), 100);
                    decimal supCharge = 0;

                    XElement dayElement = new XElement("Day",
                        new XAttribute("id", dayXmlId),
                        new XElement("Date", day),
                        new XElement("ETax", etx),
                        new XElement("SuplCharge", supCharge),
                        new XElement("Length", shifts.Where(w => w.Date == day).ToList().GetWorkMinutes()),
                        new XElement("EmployeeAccounts", accounts.Select(x => x.Name).ToCommaSeparated()));

                    employeeElement.Add(dayElement);
                    dayDict.Add(day, dayXmlId);
                    dayXmlId++;
                }

                #endregion

                int payrollTransactionXmlId = 1;
                ConcurrentBag<XElement> transactionElements = new ConcurrentBag<XElement>();

                Parallel.Invoke(() =>
                {
                    foreach (var payrollTransactionGroup in timePayrollTransactions.GroupBy(g => $"{g.TimeBlockDateId}#{g.PayrollProductId}#{g.GetAccountingIdString()}#{g.AttestStateId}"))
                    {
                        #region PayrollTransaction element

                        if (filterOnAccounting && !selectionAccountIds.IsNullOrEmpty())
                        {
                            var firstAccountInternals = payrollTransactionGroup.FirstOrDefault()?.AccountInternals;

                            if (firstAccountInternals == null || !firstAccountInternals.ValidOnFiltered(validAccountInternals))
                                continue;
                        }

                        TimePayrollTransactionDTO payrollTransaction = payrollTransactionGroup.First();
                        PayrollProduct payrollProduct = payrollProducts.FirstOrDefault(f => f.ProductId == payrollTransaction.PayrollProductId);
                        if (payrollProduct != null && payrollTransaction.Date.HasValue && dayDict.ContainsKey(payrollTransaction.Date.Value))
                        {
                            int dateDayXmlId = dayDict[payrollTransaction.Date.Value];
                            bool isExtended = payrollTransaction.IsExtended && payrollTransaction.Extended != null;

                            if (hasPayrollProductIds && !payrollProductNumbers.Contains(payrollProduct.Number))
                                payrollProductNumbers.Add(payrollProduct.Number);

                            AccountStd accountStd = accountStds.FirstOrDefault(f => f.AccountId == payrollTransaction.AccountId) ?? accountStds.FirstOrDefault();
                            string payrollTypeLevel3Name = string.Empty;
                            if (payrollTransaction.SysPayrollTypeLevel2.HasValue && payrollTransaction.SysPayrollTypeLevel2.Value == (int)TermGroup_SysPayrollType.SE_Time_Accumulator)
                            {
                                if (payrollTransaction.SysPayrollTypeLevel3.HasValue)
                                {
                                    TimeAccumulator accumulator = allAccumulators.FirstOrDefault(a => a.TimeAccumulatorId == payrollTransaction.SysPayrollTypeLevel3.Value);
                                    if (accumulator != null)
                                        payrollTypeLevel3Name = (accumulator.TimeAccumulatorId * -1).ToString();
                                }
                            }
                            else
                                payrollTypeLevel3Name = payrollTransaction.SysPayrollTypeLevel3.HasValue ? payrollTransaction.SysPayrollTypeLevel3.Value.ToString() : string.Empty;

                            int productId = productDict.FirstOrDefault(f => f.Key == payrollTransaction.PayrollProductId).Value;
                            int attestId = attestStateDict.FirstOrDefault(f => f.Key == payrollTransaction.AttestStateId).Value;

                            XElement transactionElement = new XElement("PayrollTransaction",
                                new XAttribute("id", payrollTransactionXmlId),
                                new XElement("DayId", dateDayXmlId),
                                new XElement("Q", payrollTransactionGroup.Sum(s => s.Quantity)),
                                new XElement("QyWorkDays", isExtended ? payrollTransaction.Extended.QuantityWorkDays : 0),
                                new XElement("QCalDays", isExtended ? payrollTransaction.Extended.QuantityCalendarDays : 0),
                                new XElement("CalDayFactor", isExtended ? payrollTransaction.Extended.CalenderDayFactor : 0),
                                new XElement("Unit", isExtended ? payrollTransaction.Extended.TimeUnit : 0),
                                new XElement("Price", payrollTransaction.UnitPrice),
                                new XElement("Amount", payrollTransactionGroup.Sum(s => s.Amount)),
                                new XElement("VATAmount", payrollTransactionGroup.Sum(s => s.VatAmount)),
                                new XElement("Date", payrollTransaction.Date),
                                new XElement("Exported", payrollTransaction.Exported.ToInt()),
                                new XElement("AttestId", attestId),
                                new XElement("ProductId", productId),
                                new XElement("L1", payrollTransaction.SysPayrollTypeLevel1.HasValue ? payrollTransaction.SysPayrollTypeLevel1.Value.ToString() : string.Empty),
                                new XElement("L2", payrollTransaction.SysPayrollTypeLevel2.HasValue ? payrollTransaction.SysPayrollTypeLevel2.Value.ToString() : string.Empty),
                                new XElement("L3", payrollTypeLevel3Name),
                                new XElement("L4", payrollTransaction.SysPayrollTypeLevel4.HasValue ? payrollTransaction.SysPayrollTypeLevel4.Value.ToString() : string.Empty),
                                new XElement("Prel", payrollTransaction.IsPreliminary.ToInt()),
                                new XElement("Fixed", payrollTransaction.IsFixed.ToInt()),
                                new XElement("AccountId", accountStd.AccountId));

                            for (int i = 1; i <= Constants.NOOFDIMENSIONS; i++)
                            {
                                if (i > accountDimInternals.Count)
                                {
                                    transactionElement.Add(new XElement("IId" + i, 0));
                                    continue;
                                }

                                AccountInternalDTO accountInternal = null;
                                AccountDim accountDim = accountDimInternals.Any() ? accountDimInternals.ElementAt(i - 1) : null;
                                if (accountDim != null && payrollTransaction.AccountInternals != null && payrollTransaction.AccountInternals.Any())
                                {
                                    var accountsOnDim = accountInternals.Where(w => w.AccountDimId == accountDim.AccountDimId);
                                    if (accountsOnDim.Any())
                                    {
                                        List<int> ids = accountsOnDim.Select(s => s.AccountId).ToList();

                                        accountInternal = (from ai in payrollTransaction.AccountInternals
                                                           where ids.Contains(ai.AccountId)
                                                           select ai).FirstOrDefault();
                                    }
                                }

                                if (accountInternal != null)
                                    transactionElement.Add(new XElement("IId" + i, accountInternals.FirstOrDefault(f => f.AccountId == accountInternal.AccountId)?.AccountId ?? 0));
                                else
                                    transactionElement.Add(new XElement("IId" + i, 0));
                            }

                            transactionElements.Add(transactionElement);
                            payrollTransactionXmlId++;
                        }

                        #endregion
                    }
                },
            () =>
            {
                foreach (var payrollScheduleTransactionGroup in timePayrollScheduleTransaction.GroupBy(g => $"{g.TimeBlockDateId}#{g.PayrollProductId}#{g.GetAccountingIdString()}#{g.UnitPrice}"))
                {
                    if (filterOnAccounting && !selectionAccountIds.IsNullOrEmpty())
                    {
                        var firstAccountInternals = payrollScheduleTransactionGroup.FirstOrDefault()?.AccountInternals;

                        if (firstAccountInternals == null || !firstAccountInternals.ValidOnFiltered(validAccountInternals))
                            continue;
                    }

                    TimePayrollTransactionDTO payrollScheduleTransaction = payrollScheduleTransactionGroup.First();
                    PayrollProduct payrollProduct = payrollProducts.FirstOrDefault(f => f.ProductId == payrollScheduleTransaction.PayrollProductId);
                    AccountStd accountStd = accountStds.FirstOrDefault(f => f.AccountId == payrollScheduleTransaction.AccountId) ?? accountStds.FirstOrDefault();

                    #region PayrollScheduleTransaction element

                    if (hasPayrollProductIds && !payrollProductNumbers.Contains(payrollProduct.Number))
                        payrollProductNumbers.Add(payrollProduct.Number);

                    int dateDayXmlId = dayDict.First(f => f.Key == payrollScheduleTransaction.Date).Value;
                    int productId = productDict.FirstOrDefault(f => f.Key == payrollScheduleTransaction.PayrollProductId).Value;

                    string payrollTypeLevel3Name = string.Empty;
                    if (payrollScheduleTransaction.SysPayrollTypeLevel3.HasValue && payrollScheduleTransaction.SysPayrollTypeLevel2.Value == (int)TermGroup_SysPayrollType.SE_Time_Accumulator)
                    {
                        if (payrollScheduleTransaction.SysPayrollTypeLevel3.HasValue)
                        {
                            TimeAccumulator accumulator = allAccumulators.FirstOrDefault(a => a.TimeAccumulatorId == payrollScheduleTransaction.SysPayrollTypeLevel3.Value);
                            if (accumulator != null)
                                payrollTypeLevel3Name = (accumulator.TimeAccumulatorId * -1).ToString();
                        }
                    }
                    else
                        payrollTypeLevel3Name = payrollScheduleTransaction.SysPayrollTypeLevel3.HasValue ? payrollScheduleTransaction.SysPayrollTypeLevel3.Value.ToString() : string.Empty;

                    XElement transactionElement = new XElement("PayrollTransaction",
                        new XAttribute("id", payrollTransactionXmlId),
                        new XElement("DayId", dateDayXmlId),
                        new XElement("Q", payrollScheduleTransactionGroup.Sum(s => s.Quantity)),
                        new XElement("QyWorkDays", 0),
                        new XElement("QCalDays", 0),
                        new XElement("CalDayFactor", 0),
                        new XElement("Unit", 0),
                        new XElement("Price", payrollScheduleTransaction.UnitPrice),
                        new XElement("Amount", payrollScheduleTransactionGroup.Sum(s => s.Amount)),
                        new XElement("VATAmount", payrollScheduleTransactionGroup.Sum(s => s.VatAmount)),
                        new XElement("Date", payrollScheduleTransaction.Date),
                        new XElement("Exported", 0),
                        new XElement("AttestId", 0),
                        new XElement("ProductId", productId),
                        new XElement("L1", payrollScheduleTransaction.SysPayrollTypeLevel1.HasValue ? payrollScheduleTransaction.SysPayrollTypeLevel1.Value.ToString() : string.Empty),
                        new XElement("L2", payrollScheduleTransaction.SysPayrollTypeLevel2.HasValue ? payrollScheduleTransaction.SysPayrollTypeLevel2.Value.ToString() : string.Empty),
                        new XElement("L3", payrollTypeLevel3Name),
                        new XElement("L4", payrollScheduleTransaction.SysPayrollTypeLevel4.HasValue ? payrollScheduleTransaction.SysPayrollTypeLevel4.Value.ToString() : string.Empty),
                        new XElement("Prel", 0),
                        new XElement("Fixed", 0),
                        new XElement("AccountId", accountStd.AccountId));

                    for (int i = 1; i <= Constants.NOOFDIMENSIONS; i++)
                    {
                        if (i > accountDimInternals.Count)
                        {
                            transactionElement.Add(new XElement("IId" + i, 0));
                            continue;
                        }

                        AccountInternalDTO accountInternal = null;
                        AccountDim accountDim = accountDimInternals.Any() ? accountDimInternals.ElementAt(i - 1) : null;
                        if (accountDim != null && payrollScheduleTransaction.AccountInternals != null && payrollScheduleTransaction.AccountInternals.Any())
                        {
                            var accountsOnDim = accountInternals.Where(w => w.AccountDimId == accountDim.AccountDimId);

                            if (accountsOnDim.Any())
                            {
                                List<int> ids = accountsOnDim.Select(s => s.AccountId).ToList();

                                accountInternal = (from ai in payrollScheduleTransaction.AccountInternals
                                                   where ids.Contains(ai.AccountId)
                                                   select ai).FirstOrDefault();
                            }
                        }

                        if (accountInternal != null)
                        {
                            Account account = accountInternals.FirstOrDefault(f => f.AccountId == accountInternal.AccountId);

                            transactionElement.Add(new XElement("IId" + i, account != null ? account.AccountId : 0));
                        }
                        else
                        {
                            transactionElement.Add(new XElement("IId" + i, 0));
                        }
                    }

                    transactionElements.Add(transactionElement);
                    payrollTransactionXmlId++;

                    #endregion
                }
            });

                foreach (var transactionElement in transactionElements)
                {
                    employeeElement.Add(transactionElement);
                }

                timePayrollTransactionElement.Add(employeeElement);
                employeeXmlId++;
            }

            if (hasPayrollProductIds)
                reportHeaderElement.Add(new XElement("PayrollProductInterval", StringUtility.GetCommaSeparatedString<string>(payrollProductNumbers.OrderBy(i => i).ToList())));
            else
                reportHeaderElement.Add(new XElement("PayrollProductInterval", GetReportText(773, "Alla")));

            this.AddUseAccountHierarchyReportHeaderElement(reportHeaderElement, useAccountHierarchy);

            #region Default element

            if (employeeXmlId == 1)
                AddDefaultElement(reportResult, timePayrollTransactionElement, "Employee");

            #endregion

            #endregion

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            #region Close document

            rootElement.Add(timePayrollTransactionElement);
            document.Add(rootElement);

            return GetValidatedDocument(document, reportResult);

            #endregion
        }

        public XDocument CreatePayrollAccountingReportData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            if (!TryGetIdsFromSelection(reportResult, out List<int> selectionTimePeriodIds, "periods"))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionTimePeriodIds, out _, out List<int> selectionEmployeeIds))
                return null;

            TryGetBoolFromSelection(reportResult, out bool createVoucher, "createVoucher");
            TryGetBoolFromSelection(reportResult, out bool skipQuantity, "skipQuantity");
            TryGetIdFromSelection(reportResult, out int? voucherSeriesTypeId, "voucherSeriesType");
            TryGetDateFromSelection(reportResult, out DateTime voucherDate, "voucherDate");
            TryGetDetailedInformationFromSelection(reportResult, out bool selectionGetDetailedInformation);
            VoucherRowMergeType voucherRowMergeType = VoucherRowMergeType.DoNotMerge;
            if (TryGetIdFromSelection(reportResult, out int? voucherRowMergeTypeId, "voucherRowMergeType") && voucherRowMergeTypeId.HasValue && Enum.IsDefined(typeof(VoucherRowMergeType), voucherRowMergeTypeId))
                voucherRowMergeType = (VoucherRowMergeType)voucherRowMergeTypeId;

            AccountDim accountDimStd = AccountManager.GetAccountDimStd(reportResult.ActorCompanyId);
            List<AccountDim> accountDimInternals = AccountManager.GetAccountDimInternalsByCompany(reportResult.ActorCompanyId, true);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Init document

            XDocument document = reportResult.Input.Document;
            XElement rootElement = reportResult.Input.ElementRoot;
            XElement payrollAccountingReportReportElement = reportResult.Input.ElementFirst;

            #endregion

            #region ReportHeaderLabels

            XElement reportHeaderLabelsElement = CreatePayrollAccountingReportHeaderLabelsElement();
            payrollAccountingReportReportElement.Add(reportHeaderLabelsElement);

            #endregion

            #region ReportHeader

            XElement reportHeaderElement = CreateTimeReportHeaderElement(reportResult);  //PayrollAccountingReport
            payrollAccountingReportReportElement.Add(reportHeaderElement);

            #endregion

            #region PageHeaderLabels

            XElement pageHeaderLabelsElement = new XElement("PageHeaderLabels");
            CreatePayrollAccountingPageHeaderLabelsElement(pageHeaderLabelsElement);
            this.AddAccountDimPageHeaderLabelElements(pageHeaderLabelsElement, accountDimStd, accountDimInternals, addExludeAccounting: true);
            payrollAccountingReportReportElement.Add(pageHeaderLabelsElement);

            #endregion

            if (reportResult.ExportType != TermGroup_ReportExportType.File)
            {
                using (CompEntities entities = new CompEntities())
                {
                    #region Prereq

                    var voucherHeads = TimeTransactionManager.GetTimePayrollVoucherHeadDTOs_new(entities, reportResult.ActorCompanyId, selectionEmployeeIds, selectionTimePeriodIds, skipQuantity, !selectionGetDetailedInformation, voucherDate != CalendarUtility.DATETIME_DEFAULT && voucherDate != DateTime.MinValue ? voucherDate : (DateTime?)null, excludeAccountingExport: true);

                    #endregion

                    int voucherHeadXmlId = 1;
                    foreach (var voucherHead in voucherHeads)
                    {
                        long number = 0;
                        string voucherCreated = string.Empty;
                        #region CreateVoucher
                        if (createVoucher && voucherSeriesTypeId.HasValue && voucherSeriesTypeId.Value != 0 && voucherDate != CalendarUtility.DATETIME_DEFAULT && voucherDate != DateTime.MinValue)
                        {
                            var result = ImportExportManager.ImportPayrollAccountingToVouchers(voucherHead, reportResult.ActorCompanyId, voucherSeriesTypeId.Value, voucherDate, voucherRowMergeType, TermGroup_VoucherHeadSourceType.Payroll);

                            if (result.Success)
                            {
                                number = (long)result.Value;
                                voucherCreated = GetText(11881, "Ihopslagningstyp") + ": " + GetText((int)voucherRowMergeType, (int)TermGroup.VoucherRowMergeType);
                                var vh = VoucherManager.GetVoucherHead(entities, result.IntegerValue, false, false, false, false);

                                if (vh != null)
                                {
                                    vh.VatVoucher = false;
                                    SaveChanges(entities);
                                }
                            }
                        }

                        #endregion

                        #region VoucherHead

                        XElement voucherHeadElement = new XElement("PayrollVoucherHead",
                                                        new XAttribute("Id", voucherHeadXmlId),
                                                        new XElement("VoucherNr", number != 0 ? number : voucherHead.VoucherNr),
                                                        new XElement("Date", voucherDate != CalendarUtility.DATETIME_DEFAULT && voucherDate != DateTime.MinValue ? voucherDate : voucherHead.Date),
                                                        new XElement("Text", voucherHead.Text),
                                                        new XElement("Template", voucherHead.Template),
                                                        new XElement("TypeBalance", voucherHead.TypeBalance),
                                                        new XElement("VatVoucher", voucherHead.VatVoucher),
                                                        new XElement("Status", voucherHead.Status),
                                                        new XElement("Created", voucherHead.Created),
                                                        new XElement("CreatedBy", voucherHead.CreatedBy),
                                                        new XElement("Modified", voucherHead.Modified),
                                                        new XElement("ModifiedBy", voucherHead.ModifiedBy),
                                                        new XElement("Note", voucherCreated));

                        voucherHeadXmlId++;

                        #endregion

                        #region VoucherRows

                        int voucherRowXmlId = 1;
                        foreach (var row in voucherHead.Rows)
                        {
                            XElement voucherRowElement = new XElement("PayrollVoucherRow",
                                                            new XAttribute("Id", voucherRowXmlId),
                                                            new XElement("Amount", row.Amount),
                                                            new XElement("AmountEntCurrency", row.AmountEntCurrency),
                                                            new XElement("AccountNr", row.Dim1Nr),
                                                            new XElement("AccountName", row.Dim1Name),
                                                            new XElement("AccountDim2Nr", row.Dim2Nr),
                                                            new XElement("AccountDim3Nr", row.Dim3Nr),
                                                            new XElement("AccountDim4Nr", row.Dim4Nr),
                                                            new XElement("AccountDim5Nr", row.Dim5Nr),
                                                            new XElement("AccountDim6Nr", row.Dim6Nr),
                                                            new XElement("AccountDim2Name", row.Dim2Name),
                                                            new XElement("AccountDim3Name", row.Dim3Name),
                                                            new XElement("AccountDim4Name", row.Dim4Name),
                                                            new XElement("AccountDim5Name", row.Dim5Name),
                                                            new XElement("AccountDim6Name", row.Dim6Name),
                                                            new XElement("AccountString", row.accountString),
                                                            new XElement("Quantity", row.Quantity),
                                                            new XElement("Text", row.Text),
                                                            new XElement("AccountDistributionName", row.AccountDistributionName));

                            voucherHeadElement.Add(voucherRowElement);
                            voucherRowXmlId++;
                        }

                        #endregion

                        payrollAccountingReportReportElement.Add(voucherHeadElement);
                    }


                    #region Content

                    rootElement.Add(payrollAccountingReportReportElement);
                    document.Add(rootElement);

                    return GetValidatedDocument(document, SoeReportTemplateType.PayrollAccountingReport);

                    #endregion
                }
            }
            return null;
        }

        public XDocument CreatePayrollVacationAccountingReportData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            if (!TryGetDateFromSelection(reportResult, out DateTime selectionDate, "date"))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDate, selectionDate, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return null;

            TryGetBoolFromSelection(reportResult, out bool createVoucher, "createVoucher");
            TryGetIdFromSelection(reportResult, out int? voucherSeriesTypeId, "voucherSeriesType");
            TryGetBoolFromSelection(reportResult, out bool skipQuantity, "skipQuantity");
            TryGetDateFromSelection(reportResult, out DateTime voucherDate, "voucherDate");
            VoucherRowMergeType voucherRowMergeType = VoucherRowMergeType.DoNotMerge;
            if (TryGetIdFromSelection(reportResult, out int? voucherRowMergeTypeId, "voucherRowMergeType") && voucherRowMergeTypeId.HasValue && Enum.IsDefined(typeof(VoucherRowMergeType), voucherRowMergeTypeId))
                voucherRowMergeType = (VoucherRowMergeType)voucherRowMergeTypeId;

            AccountDim accountDimStd = AccountManager.GetAccountDimStd(reportResult.ActorCompanyId);
            List<AccountDim> accountDimInternals = AccountManager.GetAccountDimInternalsByCompany(reportResult.ActorCompanyId, true);

            if (employees == null)
                employees = EmployeeManager.GetAllEmployeesByIds(reportResult.Input.ActorCompanyId, selectionEmployeeIds, loadEmployment: true);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Init document

            XDocument document = reportResult.Input.Document;
            XElement rootElement = reportResult.Input.ElementRoot;
            XElement payrollAccountingReportReportElement = reportResult.Input.ElementFirst;

            #endregion

            #region ReportHeaderLabels

            XElement reportHeaderLabelsElement = CreatePayrollAccountingReportHeaderLabelsElement();
            payrollAccountingReportReportElement.Add(reportHeaderLabelsElement);

            #endregion

            #region ReportHeader

            XElement reportHeaderElement = CreateTimeReportHeaderElement(reportResult); //VacationCalculationResultVoucherReport
            payrollAccountingReportReportElement.Add(reportHeaderElement);

            #endregion

            #region PageHeaderLabels

            XElement pageHeaderLabelsElement = new XElement("PageHeaderLabels");
            CreatePayrollAccountingPageHeaderLabelsElement(pageHeaderLabelsElement);
            this.AddAccountDimPageHeaderLabelElements(pageHeaderLabelsElement, accountDimStd, accountDimInternals);
            payrollAccountingReportReportElement.Add(pageHeaderLabelsElement);

            #endregion

            if (reportResult.ExportType != TermGroup_ReportExportType.File)
            {
                using (CompEntities entities = new CompEntities())
                {
                    int number = 0;
                    string voucherCreated = string.Empty;
                    var voucherHead = PayrollManager.GetEmployeeCalculateVacationResultHeadVoucher(entities, reportResult.ActorCompanyId, selectionDate, employees, skipQuantity);

                    #region CreateVoucher

                    if (createVoucher && voucherSeriesTypeId.HasValue && voucherSeriesTypeId.Value != 0 && voucherDate != CalendarUtility.DATETIME_DEFAULT && voucherDate != DateTime.MinValue)
                    {
                        var result = ImportExportManager.ImportPayrollAccountingToVouchers(voucherHead, reportResult.ActorCompanyId, voucherSeriesTypeId.Value, voucherDate, voucherRowMergeType, TermGroup_VoucherHeadSourceType.Payroll);

                        if (result.Success)
                        {
                            number = Convert.ToInt32(result.Value);
                            voucherCreated = GetText(11881, "Ihopslagningstyp") + ": " + GetText((int)voucherRowMergeType, (int)TermGroup.VoucherRowMergeType);
                            var vh = VoucherManager.GetVoucherHead(entities, result.IntegerValue, false, false, false, false);

                            if (vh != null)
                            {
                                vh.VatVoucher = false;
                                SaveChanges(entities);
                            }
                        }
                    }

                    #endregion

                    #region VoucherHead

                    XElement voucherHeadElement = new XElement("PayrollVoucherHead",
                                                    new XAttribute("Id", 1),
                                                    new XElement("VoucherNr", number != 0 ? number : voucherHead.VoucherNr),
                                                    new XElement("Date", voucherDate != CalendarUtility.DATETIME_DEFAULT && voucherDate != DateTime.MinValue ? voucherDate : voucherHead.Date),
                                                    new XElement("Text", voucherHead.Text),
                                                    new XElement("Template", voucherHead.Template),
                                                    new XElement("TypeBalance", voucherHead.TypeBalance),
                                                    new XElement("VatVoucher", voucherHead.VatVoucher),
                                                    new XElement("Status", voucherHead.Status),
                                                    new XElement("Created", voucherHead.Created),
                                                    new XElement("CreatedBy", voucherHead.CreatedBy),
                                                    new XElement("Modified", voucherHead.Modified),
                                                    new XElement("ModifiedBy", voucherHead.ModifiedBy),
                                                    new XElement("Note", voucherCreated));

                    #endregion

                    #region VoucherRows

                    int voucherRowXmlId = 1;
                    foreach (var row in voucherHead.Rows)
                    {
                        XElement voucherRowElement = new XElement("PayrollVoucherRow",
                                                        new XAttribute("Id", voucherRowXmlId),
                                                        new XElement("Amount", row.Amount),
                                                        new XElement("AmountEntCurrency", row.AmountEntCurrency),
                                                        new XElement("AccountNr", row.Dim1Nr),
                                                        new XElement("AccountName", row.Dim1Name),
                                                        new XElement("AccountDim2Nr", row.Dim2Nr),
                                                        new XElement("AccountDim3Nr", row.Dim3Nr),
                                                        new XElement("AccountDim4Nr", row.Dim4Nr),
                                                        new XElement("AccountDim5Nr", row.Dim5Nr),
                                                        new XElement("AccountDim6Nr", row.Dim6Nr),
                                                        new XElement("AccountDim2Name", row.Dim2Name),
                                                        new XElement("AccountDim3Name", row.Dim3Name),
                                                        new XElement("AccountDim4Name", row.Dim4Name),
                                                        new XElement("AccountDim5Name", row.Dim5Name),
                                                        new XElement("AccountDim6Name", row.Dim6Name),
                                                        new XElement("AccountString", row.accountString),
                                                        new XElement("Quantity", row.Quantity),
                                                        new XElement("Text", row.Text),
                                                        new XElement("AccountDistributionName", row.AccountDistributionName));

                        voucherHeadElement.Add(voucherRowElement);
                        voucherRowXmlId++;
                    }

                    #endregion

                    payrollAccountingReportReportElement.Add(voucherHeadElement);

                    #region Content

                    rootElement.Add(payrollAccountingReportReportElement);
                    document.Add(rootElement);

                    return GetValidatedDocument(document, SoeReportTemplateType.PayrollVacationAccountingReport);

                    #endregion
                }
            }
            return null;
        }

        public XDocument CreatePayrollTransactionStatisticsReportData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            if (!TryGetIdsFromSelection(reportResult, out List<int> selectionTimePeriodIds, "periods"))
                return null;

            List<TimePeriod> timePeriods = TimePeriodManager.GetTimePeriods(selectionTimePeriodIds, reportResult.Input.ActorCompanyId).Where(o => selectionTimePeriodIds.Contains(o.TimePeriodId)).ToList();
            DateTime selectionDateFrom = timePeriods.Select(t => t.StartDate).Min();
            DateTime selectionDateTo = timePeriods.Select(t => t.StopDate).Max();
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionTimePeriodIds, out List<Employee> employees, out List<int> selectionEmployeeIds, out List<int> selectionAccountIds, out _))
                return null;

            TryGetBoolFromSelection(reportResult, out bool filterOnAccounting, "filterOnAccounting");
            TryGetPayrollProductIdsFromSelections(reportResult, out List<int> selectionPayrollProductIds);
            TryGetBoolFromSelection(reportResult, out bool selectionIncludeStartValues, "includeStartValues");
            TryGetBoolFromSelection(reportResult, out bool selectionShowOnlyTotals, "showOnlyTotals");
            TryGetBoolFromSelection(reportResult, out bool ignoreAccounting, "ignoreAccounting");

            if (!selectionShowOnlyTotals && reportResult.Input.ShowOnlyTotals)
                selectionShowOnlyTotals = true;

            AccountDim accountDimStd = AccountManager.GetAccountDimStd(reportResult.Input.ActorCompanyId);
            List<AccountDim> accountDimInternals = AccountManager.GetAccountDimInternalsByCompany(reportResult.Input.ActorCompanyId, true);
            List<AccountInternalDTO> validAccountInternals = filterOnAccounting && !selectionAccountIds.IsNullOrEmpty() ? AccountManager.GetAccountInternals(reportResult.Input.ActorCompanyId, null).Where(w => selectionAccountIds.Contains(w.AccountId)).ToDTOs() : null;

            string arbetsplatsnummer = SettingManager.GetUserCompanySetting(SettingMainType.Company, (int)CompanySettingType.PayrollExportSNKFOWorkPlaceNumber, 0, reportResult.Input.ActorCompanyId, 0)?.StrData?.ToString() ?? string.Empty;
            bool showSocialSec = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec, Permission.Readonly, reportResult.Input.RoleId, reportResult.Input.ActorCompanyId);
            using var entitiesReadonly = CompEntitiesProvider.LeaseReadOnlyContext();
            bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entitiesReadonly, reportResult.ActorCompanyId);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Init document

            XDocument document = reportResult.Input.Document;
            XElement rootElement = reportResult.Input.ElementRoot;
            XElement payrollTransactionStatisticsReportElement = reportResult.Input.ElementFirst;

            #endregion

            #region ReportHeaderLabels

            XElement reportHeaderLabelsElement = CreatePayrollReportHeaderLabelsElement();
            payrollTransactionStatisticsReportElement.Add(reportHeaderLabelsElement);
            CreatePayrollProductIntervalReportHeaderLabelsElement(reportHeaderLabelsElement);

            #endregion

            #region ReportHeader

            XElement reportHeaderElement = CreateTimeReportHeaderElement(reportResult);  // PayrollTransactionStatisticsReport
            this.AddShowOnlyTotalsReportHeaderElement(reportHeaderElement, selectionShowOnlyTotals);
            payrollTransactionStatisticsReportElement.Add(reportHeaderElement);

            #endregion

            #region PageHeaderLabels

            XElement pageHeaderLabelsElement = CreatePayrollTransactionStatisticsPageHeaderLabelsElement();
            this.AddAccountDimPageHeaderLabelElements(pageHeaderLabelsElement, accountDimStd, accountDimInternals);
            payrollTransactionStatisticsReportElement.Add(pageHeaderLabelsElement);

            #endregion

            #region PayrollGroup element

            if (!base.personalDataRepository.PayrollGroups.IsNullOrEmpty())
            {
                int payrollGroupXmlId = 0;
                foreach (PayrollGroup payrollGroup in base.personalDataRepository.PayrollGroups)
                {
                    #region PayrollGroup element

                    payrollGroupXmlId++;
                    string groupPersonalkategori = PayrollManager.GetPayrollGroupSettingValue(PayrollManager.GetPayrollGroupSetting(payrollGroup.PayrollGroupId, PayrollGroupSettingType.PayrollReportsPersonalCategory))?.ToString();
                    string groupArbetstidsart = PayrollManager.GetPayrollGroupSettingValue(PayrollManager.GetPayrollGroupSetting(payrollGroup.PayrollGroupId, PayrollGroupSettingType.PayrollReportsWorkTimeCategory))?.ToString();
                    string groupLonefom = PayrollManager.GetPayrollGroupSettingValue(PayrollManager.GetPayrollGroupSetting(payrollGroup.PayrollGroupId, PayrollGroupSettingType.PayrollReportsSalaryType))?.ToString();
                    string groupForbundsnummer = PayrollManager.GetPayrollGroupSettingValue(PayrollManager.GetPayrollGroupSetting(payrollGroup.PayrollGroupId, PayrollGroupSettingType.PartnerNumber))?.ToString();
                    string groupAvtalskod = PayrollManager.GetPayrollGroupSettingValue(PayrollManager.GetPayrollGroupSetting(payrollGroup.PayrollGroupId, PayrollGroupSettingType.AgreementCode))?.ToString();

                    XElement payrollGroupElement = new XElement("PayrollGroup",
                        new XAttribute("id", payrollGroupXmlId),
                        new XElement("PayrollGroupName", payrollGroup?.Name ?? string.Empty),
                        new XElement("PayrollGroupId", payrollGroup?.PayrollGroupId ?? 0),
                        new XElement("PayrollGroupSNSCBPersonalKategori", groupPersonalkategori),
                        new XElement("PayrollGroupSNSCBArbetstidsart", groupArbetstidsart),
                        new XElement("PayrollGroupSNSCBLonefom", groupLonefom),
                        new XElement("PayrollGroupSNSCBForbundsnummer", groupForbundsnummer),
                        new XElement("PayrollGroupSNSCBAvtalskod", groupAvtalskod)
                    );

                    #endregion

                    #region PayrollGroupSetting element

                    if (payrollGroup?.PayrollGroupSetting != null)
                    {
                        int payrollGroupSettingXmlId = 1;
                        foreach (PayrollGroupSetting setting in payrollGroup.PayrollGroupSetting)
                        {
                            payrollGroupElement.Add(new XElement("PayrollGroupSetting",
                                new XAttribute("id", payrollGroupSettingXmlId),
                                new XElement("PayrollGroupSettingType", setting.Type),
                                new XElement("PayrollGroupSettingName", setting.Name),
                                new XElement("PayrollGroupSettingDataType", setting.DataType),
                                new XElement("PayrollGroupSettingData", StringUtility.NullToEmpty(PayrollManager.GetPayrollGroupSettingValue(setting)))));
                            payrollGroupSettingXmlId++;
                        }

                        if (payrollGroupSettingXmlId == 1)
                        {
                            payrollGroupElement.Add(new XElement("PayrollGroupSetting",
                                new XAttribute("id", payrollGroupSettingXmlId),
                                new XElement("PayrollGroupSettingType", 0),
                                new XElement("PayrollGroupSettingName", string.Empty),
                                new XElement("PayrollGroupSettingDataType", 0),
                                new XElement("PayrollGroupSettingData", string.Empty)));
                        }
                    }
                    payrollTransactionStatisticsReportElement.Add(payrollGroupElement);

                    #endregion
                }
            }
            else
            {
                #region Default elements

                XElement payrollGroupElement = new XElement("PayrollGroup",
                        new XAttribute("id", 0),
                        new XElement("PayrollGroupName", string.Empty),
                        new XElement("PayrollGroupId", 0),
                        new XElement("PayrollGroupSNSCBPersonalKategori", string.Empty),
                        new XElement("PayrollGroupSNSCBArbetstidsart", string.Empty),
                        new XElement("PayrollGroupSNSCBLonefom", string.Empty),
                        new XElement("PayrollGroupSNSCBForbundsnummer", string.Empty),
                        new XElement("PayrollGroupSNSCBAvtalskod", string.Empty));


                payrollGroupElement.Add(new XElement("PayrollGroupSetting",
                       new XAttribute("id", 0),
                       new XElement("PayrollGroupSettingType", 0),
                       new XElement("PayrollGroupSettingName", string.Empty),
                       new XElement("PayrollGroupSettingDataType", 0),
                       new XElement("PayrollGroupSettingData", string.Empty)));

                payrollTransactionStatisticsReportElement.Add(payrollGroupElement);

                #endregion
            }

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                if (employees == null)
                    employees = EmployeeManager.GetAllEmployeesByIds(entities, reportResult.Input.ActorCompanyId, selectionEmployeeIds, loadEmployment: true);

                List<TimeAccumulator> allAccumulators = TimeAccumulatorManager.GetTimeAccumulators(entities, reportResult.Input.ActorCompanyId);
                List<TimePayrollStatisticsDTO> timePayrollStatisticsDTOs = TimeTransactionManager.GetTimePayrollStatisticsDTOs(entities, reportResult.Input.ActorCompanyId, employees, selectionTimePeriodIds, 1, null, selectionPayrollProductIds, ignoreAccounting: ignoreAccounting);
                Dictionary<int, List<EmployeeSkill>> employeeSkillsForCompany = TimeScheduleManager.GetEmployeeSkillsForCompany(reportResult.Input.ActorCompanyId).GroupBy(i => i.EmployeeId).ToDictionary(i => i.Key, i => i.ToList());
                Dictionary<int, List<EmployeePosition>> employeePositionsForCompany = EmployeeManager.GetEmployeePositionsForCompany(reportResult.Input.ActorCompanyId).GroupBy(i => i.EmployeeId).ToDictionary(i => i.Key, i => i.ToList());

                List<string> payrollProductNumbers = new List<string>();
                bool hasPayrollProductIds = !selectionPayrollProductIds.IsNullOrEmpty();

                if (selectionIncludeStartValues)
                {
                    int year = timePeriods.OrderBy(x => x.PayrollStartDate).LastOrDefault().PayrollStopDate.Value.Year;

                    foreach (Employee employee in employees)
                    {
                        List<TimePayrollStatisticsDTO> employeeTimePayrollStatisticsDTOs = timePayrollStatisticsDTOs.Where(t => t.EmployeeId == employee.EmployeeId).ToList();
                        List<int> skipTransactionIds = employeeTimePayrollStatisticsDTOs.Select(t => t.TimePayrollTransactionId).ToList();
                        timePayrollStatisticsDTOs.AddRange(TimeTransactionManager.GetPayrollStartValuesAsTimePayrollStatisticsDTOs(entities, employee, reportResult.Input.ActorCompanyId, year, false, base.personalDataRepository.PayrollGroups, base.personalDataRepository.EmployeeGroups, skipTransactionIds, ignoreAccounting));
                    }
                }

                if (filterOnAccounting && !selectionAccountIds.IsNullOrEmpty())
                {
                    List<TimePayrollStatisticsDTO> filtered = new List<TimePayrollStatisticsDTO>();

                    foreach (var dto in timePayrollStatisticsDTOs.Where(w => w.AccountInternals != null))
                    {
                        if (dto.AccountInternals.ValidOnFiltered(validAccountInternals))
                            filtered.Add(dto);
                    }

                    timePayrollStatisticsDTOs = filtered;
                }

                #endregion

                #region Content

                foreach (Employee employee in employees)
                {
                    #region Employee

                    int employeeXmlId = 1;
                    string employeePersonalkategori = employee.PayrollStatisticsPersonalCategory.HasValue ? employee.PayrollStatisticsPersonalCategory.Value.ToString() : string.Empty;
                    string employeeArbetstidsart = employee.PayrollStatisticsWorkTimeCategory.HasValue ? employee.PayrollStatisticsWorkTimeCategory.Value.ToString() : string.Empty;
                    string employeeLoneform = employee.PayrollStatisticsSalaryType.HasValue ? employee.PayrollStatisticsSalaryType.Value.ToString() : string.Empty;
                    string employeeArbetsplatsnummer = !string.IsNullOrEmpty(employee.WorkPlaceSCB) ? employee.WorkPlaceSCB : arbetsplatsnummer;
                    string employeeCfarnummer = employee.PayrollStatisticsCFARNumber.HasValue ? employee.PayrollStatisticsCFARNumber.Value.ToString() : string.Empty;

                    #region Employee element

                    decimal SSG = 0;

                    // Loop all employments for specified vacation year
                    decimal allssgs = 0;
                    decimal nrOfDays = 0;
                    List<DateTime> checkedDates = new List<DateTime>();
                    foreach (TimePeriod timePeriod in timePeriods.Where(t => selectionTimePeriodIds.Contains(t.TimePeriodId) && !t.ExtraPeriod))
                    {
                        DateTime fromDate = CalendarUtility.GetBeginningOfMonth(timePeriod.PaymentDate);
                        DateTime toDate = CalendarUtility.GetEndOfMonth(timePeriod.PaymentDate);
                        DateTime lookDate = fromDate;
                        while (lookDate <= toDate)
                        {
                            if (!checkedDates.Contains(lookDate))
                            {
                                Employment employment = employee.GetEmployment(lookDate);
                                if (employment != null)
                                    allssgs += employment.GetPercent(lookDate);
                            }

                            nrOfDays++;
                            lookDate = lookDate.AddDays(1);
                        }
                    }

                    if (nrOfDays > 0)
                        SSG = Decimal.Divide(allssgs, nrOfDays);

                    XElement employeeElement = new XElement("Employee",
                        new XAttribute("Id", employeeXmlId),
                        new XElement("EmployeeNr", employee.EmployeeNr),
                        new XElement("EmployeeName", employee.Name),
                        new XElement("EmployeeFirstName", employee.FirstName),
                        new XElement("EmployeeLastName", employee.LastName),
                        new XElement("EmployeeSocialSec", showSocialSec ? employee.SocialSec : StringUtility.SocialSecYYMMDD_Dash_Stars(employee.SocialSec)),
                        new XElement("EmployeeSex", employee.ContactPerson != null ? employee.ContactPerson.Sex : 0),
                        new XElement("EmployeeDisbursementClearingNr", employee.DisbursementClearingNr),
                        new XElement("EmployeeDisbursementAccountNr", employee.DisbursementAccountNr),
                        new XElement("HighRiskProtection", employee.HighRiskProtection.ToInt()),
                        new XElement("HighRiskProtectionTo", employee.HighRiskProtectionTo.HasValue ? employee.HighRiskProtectionTo : CalendarUtility.DATETIME_DEFAULT),
                        new XElement("MedicalCertificateDays", employee.MedicalCertificateDays.HasValue ? employee.MedicalCertificateDays : 0),
                        new XElement("MedicalCertificateReminder", employee.MedicalCertificateReminder ? 1 : 0),
                        new XElement("Note", employee.Note),
                        new XElement("SNSCBPersonalkategori", employeePersonalkategori),
                        new XElement("SNSCBArbetstidsart", employeeArbetstidsart),
                        new XElement("SNSCBLoneform", employeeLoneform),
                        new XElement("SNSCBArbetsplatsnummer", employeeArbetsplatsnummer),
                        new XElement("SNSCBCfarnummer", employeeCfarnummer),
                        new XElement("SSG", SSG));
                    base.personalDataRepository.AddEmployee(employee, employeeElement);

                    #endregion

                    #region Categories element

                    int categoryXmlId = 1;
                    if (!useAccountHierarchy)
                    {
                        List<CompanyCategoryRecord> records = CategoryManager.GetCompanyCategoryRecords(SoeCategoryType.Employee, SoeCategoryRecordEntity.Employee, employee.EmployeeId, Company.ActorCompanyId, false, selectionDateFrom, selectionDateTo);
                        foreach (CompanyCategoryRecord record in records)
                        {
                            XElement categoryElement = new XElement("Categories",
                                new XAttribute("Id", categoryXmlId),
                                new XElement("CategoryCode", record.Category != null ? StringUtility.NullToEmpty(record.Category.Code) : string.Empty),
                                new XElement("CategoryName", record.Category != null ? StringUtility.NullToEmpty(record.Category.Name) : string.Empty),
                                new XElement("isDefaultCategory", record.Default.ToInt()),
                                new XElement("EmployeeNr", employee.EmployeeNr));

                            employeeElement.Add(categoryElement);
                            categoryXmlId++;
                        }
                    }

                    if (categoryXmlId == 1)
                    {
                        XElement categoryElement = new XElement("Categories",
                            new XAttribute("Id", 0));
                        employeeElement.Add(categoryElement);
                    }

                    #endregion

                    #region Skills element

                    int skillXmlId = 1;
                    List<EmployeeSkill> employeeSkills = employeeSkillsForCompany.ContainsKey(employee.EmployeeId) ? employeeSkillsForCompany[employee.EmployeeId] : new List<EmployeeSkill>();
                    foreach (EmployeeSkill employeeSkill in employeeSkills)
                    {
                        XElement skillElement = new XElement("Skills",
                        new XAttribute("Id", skillXmlId),
                        new XElement("SkillTypeName", employeeSkill.Skill != null && employeeSkill.Skill.SkillType != null ? employeeSkill.Skill.SkillType.Name : string.Empty),
                        new XElement("SkillName", employeeSkill.Skill != null ? employeeSkill.Skill.Name : string.Empty),
                        new XElement("SkillEndDate", employeeSkill.DateTo.HasValue ? employeeSkill.DateTo : CalendarUtility.DATETIME_DEFAULT),
                        new XElement("SkillLevel", employeeSkill.SkillLevel));

                        employeeElement.Add(skillElement);
                        skillXmlId++;
                    }

                    if (skillXmlId == 1)
                    {
                        XElement skillElement = new XElement("Skills",
                            new XAttribute("Id", 0));
                        employeeElement.Add(skillElement);
                    }

                    #endregion

                    #region EmployeePositions element

                    int employeePositionsXmlId = 1;
                    List<EmployeePosition> employeePositions = employeePositionsForCompany.ContainsKey(employee.EmployeeId) ? employeePositionsForCompany[employee.EmployeeId] : new List<EmployeePosition>();
                    foreach (EmployeePosition employeePosition in employeePositions)
                    {
                        XElement employeePositionElement = new XElement("EmployeePositions",
                            new XAttribute("Id", employeePositionsXmlId),
                            new XElement("Code", employeePosition.Position.Code.NullToEmpty()),
                            new XElement("Position", employeePosition.Position.Name.NullToEmpty()));

                        employeeElement.Add(employeePositionElement);
                        employeePositionsXmlId++;
                    }

                    if (employeePositionsXmlId == 1)
                    {
                        XElement employeePositionElement = new XElement("EmployeePositions",
                            new XAttribute("Id", 0));
                        employeeElement.Add(employeePositionElement);
                    }

                    #endregion

                    #region Transactions

                    int transactionXmlId = 1;

                    IEnumerable<IGrouping<string, TimePayrollStatisticsDTO>> groups;

                    if (selectionShowOnlyTotals)
                        groups = timePayrollStatisticsDTOs.Where(t => t.EmployeeId == employee.EmployeeId).GroupBy(g => $"{g.PayrollProductId}#{g.AccountString}#{g.AttestStateId}#{g.Exported}#{g.PayrollGroupId}#{g.IsEmploymentTaxBelowLimitHidden}");
                    else if (!string.IsNullOrEmpty(reportResult.ReportSpecial) && reportResult.ReportSpecial.Contains("DPA"))
                        groups = timePayrollStatisticsDTOs.Where(t => t.EmployeeId == employee.EmployeeId).GroupBy(g => $"{g.PayrollProductId}#{g.AccountString}#{g.TimeBlockDate}#{g.Exported}");
                    else
                        groups = timePayrollStatisticsDTOs.Where(t => t.EmployeeId == employee.EmployeeId).GroupBy(g => g.TimePayrollTransactionId.ToString());

                    foreach (var group in groups)
                    {
                        TimePayrollStatisticsDTO timePayrollStatisticsDTO = group.First();
                        TimePayrollStatisticsDTO last = group.Last();
                        if (hasPayrollProductIds && !selectionPayrollProductIds.Contains(timePayrollStatisticsDTO.PayrollProductId))
                            continue;
                        if (hasPayrollProductIds && !payrollProductNumbers.Contains(timePayrollStatisticsDTO.PayrollProductNumber))
                            payrollProductNumbers.Add(timePayrollStatisticsDTO.PayrollProductNumber);

                        #region Accumulator names

                        string payrollTypeLevel3Name = string.Empty;
                        if (timePayrollStatisticsDTO.IsTimeAccumulator())
                        {
                            if (timePayrollStatisticsDTO.SysPayrollTypeLevel3.HasValue)
                                payrollTypeLevel3Name = allAccumulators.FirstOrDefault(a => a.TimeAccumulatorId == timePayrollStatisticsDTO.SysPayrollTypeLevel3.Value)?.Name ?? string.Empty;
                        }
                        else
                            payrollTypeLevel3Name = timePayrollStatisticsDTO.SysPayrollTypeLevel3Name;

                        #endregion

                        #region Transactions element

                        XElement transactionsElement = new XElement("Transactions",
                            new XAttribute("Id", transactionXmlId),
                            new XElement("TimeBlockDate", timePayrollStatisticsDTO.TimeBlockDate),
                            new XElement("PaymentDate", timePayrollStatisticsDTO.PaymentDate.ToValueOrDefault()),
                            new XElement("AttestState", timePayrollStatisticsDTO.AttestStateName),
                            new XElement("PayrollProductName", timePayrollStatisticsDTO.PayrollProductName),
                            new XElement("PayrollProductNumber", timePayrollStatisticsDTO.PayrollProductNumber),
                            new XElement("PayrollProductDescription", timePayrollStatisticsDTO.PayrollProductDescription),
                            new XElement("TimeCodeName", timePayrollStatisticsDTO.TimeCodeName),
                            new XElement("TimeCodeNumber", timePayrollStatisticsDTO.TimeCodeNumber),
                            new XElement("TimeCodeDescription", timePayrollStatisticsDTO.TimeCodeDescription),
                            new XElement("TimeBlockStartTime", timePayrollStatisticsDTO.TimeBlockStartTime),
                            new XElement("TimeBlockStopTime", last.TimeBlockStopTime),
                            new XElement("SysPayrollTypeLevel1", timePayrollStatisticsDTO.SysPayrollTypeLevel1Name),
                            new XElement("SysPayrollTypeLevel2", timePayrollStatisticsDTO.SysPayrollTypeLevel2Name),
                            new XElement("SysPayrollTypeLevel3", payrollTypeLevel3Name),
                            new XElement("SysPayrollTypeLevel4", timePayrollStatisticsDTO.SysPayrollTypeLevel4Name),
                            new XElement("UnitPrice", timePayrollStatisticsDTO.UnitPrice),
                            new XElement("UnitPriceCurrency", timePayrollStatisticsDTO.UnitPriceCurrency),
                            new XElement("UnitPriceEntCurrency", timePayrollStatisticsDTO.UnitPriceEntCurrency),
                            new XElement("UnitPriceLedgerCurrency", timePayrollStatisticsDTO.UnitPriceLedgerCurrency),
                            new XElement("Amount", group.Sum(s => s.Amount)),
                            new XElement("AmountCurrency", group.Sum(s => s.AmountCurrency)),
                            new XElement("AmountEntCurrency", group.Sum(s => s.AmountEntCurrency)),
                            new XElement("AmountLedgerCurrency", group.Sum(s => s.AmountLedgerCurrency)),
                            new XElement("VatAmount", group.Sum(s => s.VatAmount)),
                            new XElement("VatAmountCurrency", group.Sum(s => s.VatAmountCurrency)),
                            new XElement("VatAmountEntCurrency", group.Sum(s => s.VatAmountEntCurrency)),
                            new XElement("VatAmountLedgerCurrency", group.Sum(s => s.VatAmountLedgerCurrency)),
                            new XElement("Quantity", group.Sum(s => s.Quantity)),
                            new XElement("QuantityWorkDays", group.Sum(s => s.QuantityWorkDays)),
                            new XElement("QuantityCalendarDays", group.Sum(s => s.QuantityCalendarDays)),
                            new XElement("CalenderDayFactor", timePayrollStatisticsDTO.CalenderDayFactor),
                            new XElement("TimeUnit", timePayrollStatisticsDTO.TimeUnit),
                            new XElement("TimeUnitName", GetText(timePayrollStatisticsDTO.TimeUnit, (int)TermGroup.PayrollProductTimeUnit)),
                            new XElement("ManuallyAdded", timePayrollStatisticsDTO.ManuallyAdded.ToInt()),
                            new XElement("AutoAttestFailed", timePayrollStatisticsDTO.AutoAttestFailed.ToInt()),
                            new XElement("Exported", timePayrollStatisticsDTO.Exported.ToInt()),
                            new XElement("IsPreliminary", timePayrollStatisticsDTO.IsPreliminary.ToInt()),
                            new XElement("IsRetroactive", timePayrollStatisticsDTO.RetroactivePayrollOutcomeId.HasValue.ToInt()),
                            new XElement("Formula", timePayrollStatisticsDTO.Formula),
                            new XElement("FormulaPlain", timePayrollStatisticsDTO.FormulaPlain),
                            new XElement("FormulaExtracted", timePayrollStatisticsDTO.FormulaExtracted),
                            new XElement("FormulaNames", timePayrollStatisticsDTO.FormulaNames),
                            new XElement("FormulaOrigin", timePayrollStatisticsDTO.FormulaOrigin),
                            new XElement("WorkTimeWeek", timePayrollStatisticsDTO.WorkTimeWeek),
                            new XElement("EmployeeGroupName", timePayrollStatisticsDTO.EmployeeGroupName),
                            new XElement("EmployeeGroupWorkTimeWeek", timePayrollStatisticsDTO.EmployeeGroupWorkTimeWeek),
                            new XElement("PayrollGroupId", timePayrollStatisticsDTO.PayrollGroupId),
                            new XElement("PayrollGroupName", timePayrollStatisticsDTO.PayrollGroupName),
                            new XElement("PayrollCalculationPerformed", timePayrollStatisticsDTO.PayrollCalculationPerformed),
                            new XElement("Created", timePayrollStatisticsDTO.Created),
                            new XElement("Modified", timePayrollStatisticsDTO.Modified),
                            new XElement("Comment", timePayrollStatisticsDTO.Comment),
                            new XElement("CreatedBy", timePayrollStatisticsDTO.CreatedBy),
                            new XElement("ModifiedBy", timePayrollStatisticsDTO.ModifiedBy),
                            new XElement("Dim1Nr", !string.IsNullOrEmpty(timePayrollStatisticsDTO.Dim1Nr) ? timePayrollStatisticsDTO.Dim1Nr : "0"),
                            new XElement("Dim1Name", timePayrollStatisticsDTO.Dim1Name),
                            new XElement("Dim2Nr", !string.IsNullOrEmpty(timePayrollStatisticsDTO.Dim2Nr) ? timePayrollStatisticsDTO.Dim2Nr : "0"),
                            new XElement("Dim2Name", timePayrollStatisticsDTO.Dim2Name),
                            new XElement("Dim2SIENr", timePayrollStatisticsDTO.Dim2SIENr.HasValue ? timePayrollStatisticsDTO.Dim2SIENr : 0),
                            new XElement("Dim3Nr", !string.IsNullOrEmpty(timePayrollStatisticsDTO.Dim3Nr) ? timePayrollStatisticsDTO.Dim3Nr : "0"),
                            new XElement("Dim3Name", timePayrollStatisticsDTO.Dim3Name),
                            new XElement("Dim3SIENr", timePayrollStatisticsDTO.Dim3SIENr.HasValue ? timePayrollStatisticsDTO.Dim3SIENr : 0),
                            new XElement("Dim4Nr", !string.IsNullOrEmpty(timePayrollStatisticsDTO.Dim4Nr) ? timePayrollStatisticsDTO.Dim4Nr : "0"),
                            new XElement("Dim4Name", timePayrollStatisticsDTO.Dim4Name),
                            new XElement("Dim4SIENr", timePayrollStatisticsDTO.Dim4SIENr.HasValue ? timePayrollStatisticsDTO.Dim4SIENr : 0),
                            new XElement("Dim5Nr", !string.IsNullOrEmpty(timePayrollStatisticsDTO.Dim5Nr) ? timePayrollStatisticsDTO.Dim5Nr : "0"),
                            new XElement("Dim5Name", timePayrollStatisticsDTO.Dim5Name),
                            new XElement("Dim5SIENr", timePayrollStatisticsDTO.Dim5SIENr.HasValue ? timePayrollStatisticsDTO.Dim5SIENr : 0),
                            new XElement("Dim6Nr", !string.IsNullOrEmpty(timePayrollStatisticsDTO.Dim6Nr) ? timePayrollStatisticsDTO.Dim6Nr : "0"),
                            new XElement("Dim6Name", timePayrollStatisticsDTO.Dim6Name),
                            new XElement("Dim6SIENr", timePayrollStatisticsDTO.Dim6SIENr.HasValue ? timePayrollStatisticsDTO.Dim6SIENr : 0),
                            new XElement("AccountString", timePayrollStatisticsDTO.AccountString),
                            new XElement("IsEmploymentTaxBelowLimitHidden", timePayrollStatisticsDTO.IsEmploymentTaxBelowLimitHidden.ToInt()));

                        employeeElement.Add(transactionsElement);
                        transactionXmlId++;

                        #endregion
                    }

                    #region Default Transactions element

                    if (transactionXmlId == 1)
                    {
                        XElement transElement = new XElement("Transactions",
                            new XAttribute("Id", 0));
                        employeeElement.Add(transElement);
                    }

                    #endregion

                    #endregion

                    payrollTransactionStatisticsReportElement.Add(employeeElement);

                    #endregion
                }

                if (hasPayrollProductIds)
                    reportHeaderElement.Add(new XElement("PayrollProductInterval", StringUtility.GetCommaSeparatedString<string>(payrollProductNumbers.OrderBy(i => i).ToList())));
                else
                    reportHeaderElement.Add(new XElement("PayrollProductInterval", GetReportText(773, "Alla")));

                #endregion
            }

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            #region Close document

            rootElement.Add(payrollTransactionStatisticsReportElement);
            document.Add(rootElement);

            return GetValidatedDocument(document, reportResult);

            #endregion
        }

        public XDocument CreatePayrollPeriodWarningCheckReportData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            if (!TryGetIdsFromSelection(reportResult, out List<int> selectionTimePeriodIds, "periods"))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionTimePeriodIds, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return null;

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            #region Init document

            XDocument document = reportResult.Input.Document;
            XElement rootElement = reportResult.Input.ElementRoot;
            XElement payrollPeriodWarningCheckElement = reportResult.Input.ElementFirst;

            #endregion

            #region ReportHeaderLabels

            XElement reportHeaderLabelsElement = CreateTimeReportHeaderLabelsElement();
            payrollPeriodWarningCheckElement.Add(reportHeaderLabelsElement);

            #endregion

            #region ReportHeader

            XElement reportHeaderElement = CreateExtendedPersonellReportHeaderElement(reportResult, selectionTimePeriodIds.First());
            payrollPeriodWarningCheckElement.Add(reportHeaderElement);

            #endregion

            #region PageHeaderLabels

            XElement pageHeaderLabelsElement = new XElement("PageHeaderLabels");

            pageHeaderLabelsElement.Add(
                new XElement("EmployeeNrLabel", GetReportText(151, "Anst.nr:")),
                new XElement("EmployeeFirstName", GetReportText(803, "Förnamn:")),
                new XElement("EmployeeLastName", GetReportText(804, "Efternamn:")),
                new XElement("EmployeeNameLabel", GetReportText(152, "Namn:")),
                new XElement("EmployeeSocialSecLabel", GetReportText(805, "Personnummer:")),
                new XElement("TaxIsMissingLabel", GetReportText(8696, "Skatt saknas")),
                new XElement("EmploymentTaxIsMissingLabel", GetReportText(8697, "Arbetsgivaravgift saknas")),
                new XElement("EmploymentTaxDiffLabel", GetReportText(8698, "Arbetsgivaravgift debet/kredit stämmer inte överens")),
                new XElement("SupplementChargeDiffLabel", GetReportText(8699, "Påslag debet/kredit stämmer inte överens")),
                new XElement("NetSalaryIsMissingLabel", GetReportText(8700, "Nettolön saknas")),
                new XElement("NetSalaryIsNegativeLabel", GetReportText(8701, "Nettolön är negativ")),
                new XElement("NetSalaryDiffLabel", GetReportText(8702, "Nettolön på transaktionen skiljer sig från beräknat belopp, se rutan 'Summa period' i lönebilden")),
                new XElement("GrossSalaryIsNegativeLabel", GetReportText(8707, "Bruttolön är negativ")),
                new XElement("AccountNrMissingLabel", GetReportText(8708, "Kontonummer saknas")),
                new XElement("VacationGroupMissingLabel", GetReportText(8709, "Semesteravtal saknas")),
                new XElement("DisbursementMethodIsCashLabel", GetReportText(8713, "Utbetalningssätt är satt till kontant")),
                new XElement("DisbursementMethodIsUnknownLabel", GetReportText(8714, "Utbetalningssätt är satt till okänd")),
                new XElement("IsNegativeVacationDaysLabel", GetReportText(8953, "Negativt semestersaldo")));

            payrollPeriodWarningCheckElement.Add(pageHeaderLabelsElement);

            #endregion

            #region Feature

            bool showSocialSec = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId);

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                #region Content

                Dictionary<int, string> disbursementMethodsDict = EmployeeManager.GetEmployeeDisbursementMethods((int)TermGroup_Languages.Swedish);

                int timePeriodXmlId = 1;
                foreach (var timePeriodId in selectionTimePeriodIds)
                {
                    TimePeriod currentTimePeriod = TimePeriodManager.GetTimePeriod(entities, timePeriodId, Company.ActorCompanyId, true);
                    if (currentTimePeriod == null)
                        continue;

                    if (currentTimePeriod != null)
                    {
                        XElement timePeriodElement = new XElement("TimePeriod",
                            new XAttribute("Id", timePeriodXmlId),
                            new XElement("Name", currentTimePeriod.Name),
                            new XElement("StartDate", currentTimePeriod.StartDate.ToShortDateString()),
                            new XElement("StopDate", currentTimePeriod.StopDate.ToShortDateString()),
                            new XElement("PayrollStartDate", currentTimePeriod.PayrollStartDate.HasValue ? currentTimePeriod.PayrollStartDate.Value.ToShortDateString() : ""),
                            new XElement("PayrollStopDate", currentTimePeriod.PayrollStopDate.HasValue ? currentTimePeriod.PayrollStopDate.Value.ToShortDateString() : ""),
                            new XElement("PaymentDate", currentTimePeriod.PaymentDate.HasValue ? currentTimePeriod.PaymentDate.Value.ToShortDateString() : ""),
                            new XElement("IsExtraPeriod", currentTimePeriod.ExtraPeriod ? 1 : 0));

                        timePeriodXmlId++;

                        int employeeXmlId = 1;
                        foreach (var employeeId in selectionEmployeeIds)
                        {
                            #region Prereq

                            Employee employee = (employees?.GetEmployee(employeeId)) ?? EmployeeManager.GetEmployee(employeeId, base.ActorCompanyId, loadEmployment: true, loadVacationGroup: true, loadContactPerson: true, loadEmployeeTax: true);
                            if (employee == null)
                                continue;

                            var employeeTaxSE = currentTimePeriod.PaymentDate.HasValue ? EmployeeManager.GetEmployeeTaxSE(employeeId, currentTimePeriod.PaymentDate.Value.Year) : null;
                            EmployeeVacationPeriodDTO vacationPeriod = PayrollManager.GetEmployeeVacationPeriod(base.ActorCompanyId, employeeId, timePeriodId);
                            bool isNegativeVacationDays = false;

                            if (vacationPeriod != null &&
                                    (vacationPeriod.DaysSum < 0 ||
                                    vacationPeriod.RemainingDaysAdvance < 0 ||
                                    vacationPeriod.RemainingDaysOverdue < 0 ||
                                    vacationPeriod.RemainingDaysPaid < 0 ||
                                    vacationPeriod.RemainingDaysUnpaid < 0 ||
                                    vacationPeriod.RemainingDaysYear1 < 0 ||
                                    vacationPeriod.RemainingDaysYear2 < 0 ||
                                    vacationPeriod.RemainingDaysYear3 < 0 ||
                                    vacationPeriod.RemainingDaysYear4 < 0 ||
                                    vacationPeriod.RemainingDaysYear5 < 0 ||
                                    vacationPeriod.RemainingDaysYear3 < 0 ||
                                    vacationPeriod.RemainingDaysYear3 < 0))

                                isNegativeVacationDays = true;

                            #endregion

                            #region Employee

                            EmployeeDTO employeeDTO = employee.ToDTO(includeEmployments: true, includeEmployeeGroup: true, includePayrollGroup: true, includeVacationGroup: true, includeEmployeeTax: true, dateFrom: currentTimePeriod.StartDate, dateTo: currentTimePeriod.StopDate);
                            if (employeeDTO != null)
                            {
                                List<PayrollCalculationProductDTO> payrollCalculationProducts = TimeTreePayrollManager.GetPayrollCalculationProducts(entities, reportResult.ActorCompanyId, timePeriodId, employeeId);
                                PayrollCalculationPeriodSumDTO periodSum = PayrollRulesUtil.CalculateSum(payrollCalculationProducts);

                                #region Employee element

                                XElement employeeElement = new XElement("Employee",
                                    new XAttribute("Id", employeeXmlId),
                                    new XElement("EmployeeNr", employeeDTO.EmployeeNr),
                                    new XElement("EmployeeFirstName", employeeDTO.FirstName),
                                    new XElement("EmployeeLastName", employeeDTO.LastName),
                                    new XElement("EmployeeName", employeeDTO.Name),
                                    new XElement("EmployeeSocialSec", showSocialSec ? employeeDTO.SocialSec : StringUtility.SocialSecYYMMDD_Dash_Stars(employeeDTO.SocialSec)),
                                    new XElement("DisbursementMethod", disbursementMethodsDict.ContainsKey((int)employeeDTO.DisbursementMethod) ? disbursementMethodsDict[(int)employeeDTO.DisbursementMethod] : string.Empty),
                                    new XElement("DisbursementClearingNr", employeeDTO.DisbursementClearingNr),
                                    new XElement("DisbursementAccountNr", employeeDTO.DisbursementAccountNr),
                                    new XElement("Note", employeeDTO.Note),
                                    new XElement("IsTaxMissing", periodSum.IsTaxMissing ? 1 : 0),
                                    new XElement("IsEmploymentTaxMissing", periodSum.IsEmploymentTaxMissing ? 1 : 0),
                                    new XElement("HasEmploymentTaxDiff", periodSum.HasEmploymentTaxDiff ? 1 : 0),
                                    new XElement("HasSupplementChargeDiff", periodSum.HasSupplementChargeDiff ? 1 : 0),
                                    new XElement("IsNetSalaryMissing", periodSum.IsNetSalaryMissing ? 1 : 0),
                                    new XElement("IsNetSalaryNegative", periodSum.IsNetSalaryNegative ? 1 : 0),
                                    new XElement("HasNetSalaryDiff", periodSum.HasNetSalaryDiff ? 1 : 0),
                                    new XElement("IsGrossSalaryNegative", periodSum.IsGrossSalaryNegative ? 1 : 0),
                                    new XElement("IsAccountNrMissing", employeeDTO.DisbursementAccountNrIsMissing ? 1 : 0),
                                    new XElement("IsVacationGroupMissing", employeeDTO.CurrentVacationGroupId == 0 ? 1 : 0),
                                    new XElement("IsDisbursementMethodUnknown", (employeeDTO.DisbursementMethod == TermGroup_EmployeeDisbursementMethod.Unknown) ? 1 : 0),
                                    new XElement("IsDisbursementMethodCash", (employeeDTO.DisbursementMethod == TermGroup_EmployeeDisbursementMethod.SE_CashDeposit) ? 1 : 0),
                                    new XElement("ApplyEmploymentTaxMinimumRule", employeeTaxSE != null ? employeeTaxSE.ApplyEmploymentTaxMinimumRule.ToInt() : 0),
                                    new XElement("IsNegativeVacationDays", (isNegativeVacationDays) ? 1 : 0));
                                base.personalDataRepository.AddEmployee(employee, employeeElement);
                                employeeXmlId++;
                                timePeriodElement.Add(employeeElement);

                                #endregion
                            }

                            #endregion
                        }

                        if (employeeXmlId == 1)
                            AddDefaultElement(reportResult, timePeriodElement, "Employee");

                        payrollPeriodWarningCheckElement.Add(timePeriodElement);
                    }
                }

                if (timePeriodXmlId == 1)
                    AddDefaultElement(reportResult, payrollPeriodWarningCheckElement, "TimePeriodElement");

                #endregion
            }

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            #region Close document

            rootElement.Add(payrollPeriodWarningCheckElement);
            document.Add(rootElement);

            #endregion

            return GetValidatedDocument(document, SoeReportTemplateType.PayrollPeriodWarningCheck);
        }

        public XDocument CreateRoleReportData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            if (!TryGetDateFromSelection(reportResult, out DateTime selectionDate, "date"))
                selectionDate = DateTime.Today;

            #endregion

            #region Init document

            XDocument document = reportResult.Input.Document;
            XElement rootElement = reportResult.Input.ElementRoot;
            XElement roleReportElement = reportResult.Input.ElementFirst;

            #endregion

            #region ReportHeader

            XElement reportHeaderElement = CreateTimeReportHeaderElement(reportResult);
            roleReportElement.Add(reportHeaderElement);

            #endregion

            #region ReportHeaderLabels

            XElement reportHeaderLabelsElement = CreateTimeReportHeaderLabelsElement();
            roleReportElement.Add(reportHeaderLabelsElement);

            #endregion

            #region Content

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, reportResult.ActorCompanyId);

            List<AttestRole> attestRoles = AttestManager.GetAttestRoles(reportResult.ActorCompanyId, loadAttestRoleUser: true, loadExternalCode: true);
            List<Role> userRoles = RoleManager.GetRolesByCompany(reportResult.ActorCompanyId, loadExternalCode: true);

            List<Employee> employees = EmployeeManager.GetEmployeesForUsersAttestRoles(out _, reportResult.ActorCompanyId, base.UserId, base.RoleId, dateFrom: selectionDate, dateTo: selectionDate);
            List<int> employeeIds = employees.Select(i => i.EmployeeId).ToList();
            Dictionary<int, Employee> userEmployeeDict = employees.Where(e => e.UserId.HasValue).GroupBy(i => i.UserId.Value).ToDictionary(k => k.Key, v => v.FirstOrDefault());
            List<int> userIds = employees.Where(w => w.UserId.HasValue).Select(s => s.UserId.Value).ToList();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            List<User> users = entitiesReadOnly.User.Include("AttestRoleUser").Include("UserCompanyRole").Where(w => userIds.Contains(w.UserId)).ToList();

            List<CompanyCategoryRecord> companyCategoryRecords = !useAccountHierarchy ? CategoryManager.GetCompanyCategoryRecords(SoeCategoryType.Employee, SoeCategoryRecordEntity.Employee, reportResult.Input.ActorCompanyId) : null;
            Dictionary<int, List<CompanyCategoryRecord>> companyCategoryRecordsByEmployee = companyCategoryRecords?.GroupBy(i => i.RecordId).ToDictionary(k => k.Key, v => v.ToList());
            List<CompanyCategoryRecord> companyCategoryRecordsForAttestRoles = !useAccountHierarchy ? CategoryManager.GetCompanyCategoryRecords(SoeCategoryType.Employee, SoeCategoryRecordEntity.AttestRole, reportResult.Input.ActorCompanyId) : null;
            Dictionary<int, List<EmployeeAccount>> employeeAccountsByEmployee = GetEmployeeAccountsFromCache(entitiesReadOnly, CacheConfig.Company(reportResult.ActorCompanyId), employeeIds: employeeIds).GroupBy(i => i.EmployeeId).ToDictionary(k => k.Key, v => v.ToList());

            #region AttestRoles

            int attestRoleXmlId = 1;
            foreach (AttestRole attestRole in attestRoles)
            {
                List<AttestRoleDTO> attestRoleNodes = new List<AttestRoleDTO>();

                if (useAccountHierarchy)
                {
                    foreach (var attestRoleUserByAccounts in attestRole.AttestRoleUser.Where(i => i.State == (int)SoeEntityState.Active).GroupBy(g => $"{g.AttestRoleId}#{g.AccountId}"))
                    {
                        AttestRoleUser attestRoleuserAccount = attestRoleUserByAccounts.First();
                        if (attestRoleuserAccount.AccountId.HasValue && !attestRoleNodes.Any(w => w.AccountId == attestRoleuserAccount.AccountId.Value))
                        {
                            AttestRoleDTO dto = attestRole.ToDTO();
                            dto.AccountId = attestRoleuserAccount.Account.AccountId;
                            dto.Name = attestRole.Name + " " + attestRoleuserAccount.Account.Name;
                            dto.Description = attestRoleuserAccount.Account.Description;
                            attestRoleNodes.Add(dto);
                        }
                    }
                }
                else
                {
                    attestRoleNodes.Add(attestRole.ToDTO());
                }

                foreach (AttestRoleDTO attestRoleNode in attestRoleNodes)
                {
                    #region AttestRole element

                    XElement attestRoleElement = new XElement("AttestRole",
                        new XAttribute("id", attestRoleXmlId),
                        new XElement("Name", attestRoleNode.Name),
                        new XElement("Description", attestRoleNode.Description),
                        new XElement("ExternalCodes", attestRoleNode.ExternalCodesString),
                        new XElement("ShowUncategorized", attestRoleNode.ShowUncategorized),
                        new XElement("ShowAllCategories", attestRoleNode.ShowAllCategories));

                    #endregion

                    #region AttestRoleUser elements

                    List<User> usersWithAttestRole = null;
                    if (useAccountHierarchy)
                        usersWithAttestRole = users.Where(w => !w.AttestRoleUser.IsNullOrEmpty() && w.AttestRoleUser.Any(a => a.State == (int)SoeEntityState.Active && a.AttestRoleId == attestRoleNode.AttestRoleId && a.AccountId == attestRoleNode.AccountId)).ToList();
                    else
                        usersWithAttestRole = users.Where(w => !w.AttestRoleUser.IsNullOrEmpty() && w.AttestRoleUser.Any(a => a.State == (int)SoeEntityState.Active && a.AttestRoleId == attestRoleNode.AttestRoleId)).ToList();

                    int userWithAttestRoleId = 1;
                    foreach (User user in usersWithAttestRole)
                    {
                        AttestRoleUser attestRoleUser;
                        if (!useAccountHierarchy)
                            attestRoleUser = user.AttestRoleUser.First(a => a.AttestRoleId == attestRoleNode.AttestRoleId);
                        else
                            attestRoleUser = user.AttestRoleUser.First(a => a.AttestRoleId == attestRoleNode.AttestRoleId && a.AccountId == attestRoleNode.AccountId);

                        Employee employee = employees.FirstOrDefault(f => f.UserId == user.UserId);

                        XElement attestUserElement = new XElement("AttestRoleUser",
                            new XAttribute("id", userWithAttestRoleId),
                            new XElement("UserName", user.Name),
                            new XElement("EmployeeNr", employee?.EmployeeNr ?? string.Empty),
                            new XElement("FirstName", employee?.FirstName ?? string.Empty),
                            new XElement("LastName", employee?.LastName ?? string.Empty),
                            new XElement("DateFrom", attestRoleUser.DateFrom ?? CalendarUtility.DATETIME_DEFAULT),
                            new XElement("DateTo", attestRoleUser.DateTo ?? CalendarUtility.DATETIME_DEFAULT));

                        attestRoleElement.Add(attestUserElement);
                        userWithAttestRoleId++;
                    }

                    #endregion

                    #region EmployeeOnAttestRole elements

                    if (!attestRole.ShowAllCategories)
                    {
                        if (!useAccountHierarchy)
                        {
                            List<int> categoryIds = companyCategoryRecordsForAttestRoles.Where(w => w.RecordId == attestRole.AttestRoleId).Select(s => s.CategoryId).ToList();
                            if (categoryIds.Any())
                            {
                                int employeeOnAttestRoleXmlId = 1;
                                foreach (Employee employee in employees)
                                {
                                    if (!companyCategoryRecordsByEmployee.ContainsKey(employee.EmployeeId))
                                        continue;

                                    List<CompanyCategoryRecord> employeeCategoryRecords = companyCategoryRecordsByEmployee[employee.EmployeeId].GetCategoryRecords(employee.EmployeeId, selectionDate, selectionDate).Where(w => categoryIds.Contains(w.CategoryId) && w.Default).ToList();
                                    if (!employeeCategoryRecords.Any())
                                        continue;

                                    #region EmployeeOnAttestRole element

                                    CompanyCategoryRecord firemployeeCategoryRecord = employeeCategoryRecords.First();

                                    XElement employeeElement = new XElement("EmployeeOnAttestRole",
                                        new XAttribute("id", employeeOnAttestRoleXmlId),
                                        new XElement("EmployeeNr", employee?.EmployeeNr ?? string.Empty),
                                        new XElement("FirstName", employee?.FirstName ?? string.Empty),
                                        new XElement("LastName", employee?.LastName ?? string.Empty),
                                        new XElement("DateFrom", firemployeeCategoryRecord.DateFrom ?? CalendarUtility.DATETIME_DEFAULT),
                                        new XElement("DateTo", firemployeeCategoryRecord.DateTo ?? CalendarUtility.DATETIME_DEFAULT));

                                    attestRoleElement.Add(employeeElement);
                                    employeeOnAttestRoleXmlId++;

                                    #endregion
                                }
                            }
                        }
                        else if (useAccountHierarchy)
                        {
                            int employeeOnAttestRoleXmlId = 1;
                            foreach (Employee employee in employees)
                            {
                                if (!employeeAccountsByEmployee.ContainsKey(employee.EmployeeId))
                                    continue;

                                List<EmployeeAccount> employeeAccounts = employeeAccountsByEmployee[employee.EmployeeId].GetEmployeeAccountsByAccount(employee.EmployeeId, attestRoleNode.AccountId, selectionDate, selectionDate, true);
                                if (!employeeAccounts.Any())
                                    continue;

                                EmployeeAccount employeeAccount = employeeAccounts.First();

                                XElement employeeElement = new XElement("EmployeeOnAttestRole",
                                    new XAttribute("id", employeeOnAttestRoleXmlId),
                                    new XElement("EmployeeNr", employee.EmployeeNr),
                                    new XElement("FirstName", employee.FirstName),
                                    new XElement("LastName", employee.LastName),
                                    new XElement("DateFrom", employeeAccount.DateFrom),
                                    new XElement("DateTo", employeeAccount.DateTo ?? CalendarUtility.DATETIME_DEFAULT));

                                attestRoleElement.Add(employeeElement);
                                employeeOnAttestRoleXmlId++;
                            }
                        }
                    }

                    #endregion

                    roleReportElement.Add(attestRoleElement);
                    attestRoleXmlId++;
                }
            }

            #endregion

            #region Roles

            int roleElementId = 1;
            foreach (Role role in userRoles)
            {
                #region Role element

                XElement roleElement = new XElement("Role",
                    new XAttribute("id", roleElementId),
                    new XElement("Name", role.Name),
                    new XElement("ExternalCodes", role.ExternalCodesString));

                #endregion

                #region RoleUser elements

                int userWithRoleId = 1;
                foreach (var userWithAttestRole in users.Where(w => !w.UserCompanyRole.IsNullOrEmpty() && w.UserCompanyRole.Any(a => a.State == (int)SoeEntityState.Active && a.RoleId == role.RoleId)))
                {
                    Employee employee = userEmployeeDict.ContainsKey(userWithAttestRole.UserId) ? employees.FirstOrDefault(e => e.UserId == userWithAttestRole.UserId) : null;

                    XElement userRoleElement = new XElement("RoleUser",
                        new XAttribute("id", userWithRoleId),
                        new XElement("UserName", userWithAttestRole.Name),
                        new XElement("EmployeeNr", employee?.EmployeeNr ?? string.Empty),
                        new XElement("FirstName", employee?.FirstName ?? string.Empty),
                        new XElement("LastName", employee?.LastName ?? string.Empty));

                    roleElement.Add(userRoleElement);
                    userWithRoleId++;
                }

                #endregion

                roleReportElement.Add(roleElement);
            }

            #endregion

            #region Default AttestRole element

            if (!attestRoles.Any())
                AddDefaultElement(reportResult, roleReportElement, "AttestRole");

            #endregion

            #endregion

            rootElement.Add(roleReportElement);
            document.Add(rootElement);

            return GetValidatedDocument(document, SoeReportTemplateType.RoleReport);
        }

        public XDocument CreatePayrollProductData(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            #region Prereq

            TryGetPayrollProductIdsFromSelections(reportResult, out List<int> selectionPayrollProductIds);

            #endregion

            #region Init document

            XDocument document = reportResult.Input.Document;
            XElement rootElement = reportResult.Input.ElementRoot;
            XElement payrollProductReportElement = reportResult.Input.ElementFirst;

            #endregion

            #region ReportHeader

            var accountDims = AccountManager.GetAccountDimsByCompany(reportResult.ActorCompanyId);
            XElement reportHeaderElement = CreatePayrollProductReportHeaderElement(reportResult.LoginName, reportResult.ReportName, reportResult.ReportDescription, reportResult.ReportNr, accountDims);
            payrollProductReportElement.Add(reportHeaderElement);

            #endregion

            #region ReportHeaderLabels

            XElement reportHeaderLabelsElement = CreatePayrollProductReportHeaderLabelsElement();
            payrollProductReportElement.Add(reportHeaderLabelsElement);

            #endregion

            #region PageHeaderLabels

            XElement pageHeaderLabelsElement = new XElement("PageHeaderLabels");
            CreatePayrollProductPageHeaderLabelsElement(pageHeaderLabelsElement);
            payrollProductReportElement.Add(pageHeaderLabelsElement);

            #endregion

            #region Content

            using (CompEntities entities = new CompEntities())
            {
                #region PayrollProductElement

                List<int> payrollProductIds = null;
                if (!selectionPayrollProductIds.Any())
                    payrollProductIds = ProductManager.GetPayrollProducts(reportResult.ActorCompanyId, active: true)?.Select(s => s.ProductId)?.ToList();
                else
                    payrollProductIds = selectionPayrollProductIds;

                List<Account> accounts = AccountManager.GetAccountsByCompany(reportResult.ActorCompanyId, loadAccount: true, loadAccountDim: true);

                List<PayrollProductDTO> products = ProductManager.GetPayrollProducts(payrollProductIds, true, true, true, true).ToDTOs(true, true, true, true, true).ToList();
                foreach (var product in products)
                {
                    XElement payrollProductElement = CreateTimePayrollProductElement(product, accounts, accountDims);
                    payrollProductReportElement.Add(payrollProductElement);
                }

                #endregion

                #region Default payrollproduct element

                if (!products.Any())
                    AddDefaultElement(reportResult, payrollProductReportElement, "PayrollProduct");

                #endregion
            }

            #endregion

            rootElement.Add(payrollProductReportElement);
            document.Add(rootElement);

            return GetValidatedDocument(document, SoeReportTemplateType.PayrollProductReport);
        }

        #endregion

        #region Help methods

        protected int GetIdValueFromColumn(MatrixDefinitionColumn column)
        {
            if (column.Options?.Key != null && int.TryParse(column.Options.Key, out int keyId))
                return keyId;

            var input = column.Field;
            Match match = Regex.Match(input, @"(\d+)$");
            if (match.Success)
            {
                string digitsString = match.Groups[1].Value;
                if (int.TryParse(digitsString, out int Id))
                    return Id;
            }

            return 0;
        }

        protected int GetIdValueFromColumn(MatrixColumnSelectionDTO column, bool addAsKey = true)
        {
            if (column.Options?.Key != null && int.TryParse(column.Options.Key, out int keyId))
                return keyId;

            var input = column.Field;
            Match match = Regex.Match(input, @"(\d+)$");
            if (match.Success)
            {
                string digitsString = match.Groups[1].Value;
                if (int.TryParse(digitsString, out int id))
                {
                    if (addAsKey)
                    {
                        if (column.Options == null)
                            column.Options = new MatrixDefinitionColumnOptions();

                        column.Options.Key = id.ToString();
                    }

                    return id;
                }
            }

            return 0;
        }


        public void SetGroupingData(List<TimeEmployeeScheduleDataSmallDTO> shifts, TermGroup_TimeSchedulePlanningDayViewGroupBy groupSelection, List<ShiftTypeDTO> shiftTypes, List<AccountDimSmallDTO> accountDimInternals, List<Account> accountInternals)
        {
            if (groupSelection == TermGroup_TimeSchedulePlanningDayViewGroupBy.Employee)
            {
                //Do nothing, default grouping                
            }
            else if (groupSelection == TermGroup_TimeSchedulePlanningDayViewGroupBy.ShiftTypeFirstOnDay)
            {
                foreach (var groupByEmployeeAndDate in shifts.GroupBy(g => $"{g.EmployeeId}#{g.Date}"))
                {
                    if (groupByEmployeeAndDate.Any())
                    {
                        var shift = groupByEmployeeAndDate.OrderBy(o => o.StartTime).First();
                        var shiftType = shift.ShiftTypeId.HasValue ? shiftTypes.FirstOrDefault(x => x.ShiftTypeId == shift.ShiftTypeId.Value) : null;
                        if (shiftType != null)
                        {
                            foreach (var item in groupByEmployeeAndDate)
                            {
                                item.GroupName = shiftType.Name;
                                var shiftBreaks = shift.GetOverlappedBreaks(shifts.Where(x => x.IsBreak).ToList(), true);
                                foreach (var shiftBreak in shiftBreaks)
                                    shiftBreak.GroupName = shiftType.Name;
                            }
                        }
                    }
                }
            }
            else if (groupSelection == TermGroup_TimeSchedulePlanningDayViewGroupBy.ShiftType)
            {
                foreach (var shift in shifts)
                {
                    var shiftType = shift.ShiftTypeId.HasValue ? shiftTypes.FirstOrDefault(x => x.ShiftTypeId == shift.ShiftTypeId.Value) : null;
                    if (shiftType != null)
                    {
                        shift.GroupName = shiftType.Name;
                        var shiftBreaks = shift.GetOverlappedBreaks(shifts.Where(x => x.IsBreak).ToList(), true);
                        foreach (var shiftBreak in shiftBreaks)
                            shiftBreak.GroupName = shiftType.Name;
                    }
                }
            }
            else if (groupSelection == TermGroup_TimeSchedulePlanningDayViewGroupBy.Category)
            {
                //Do nothing right now
            }
            else if ((int)groupSelection > 10)
            {
                int dimLevel = (int)groupSelection - 10;
                var selectedDim = accountDimInternals.FirstOrDefault(x => x.Level == dimLevel);
                if (selectedDim != null)
                {
                    var accountsOnDim = accountInternals.Where(x => x.AccountDimId == selectedDim.AccountDimId);
                    if (accountsOnDim.Any())
                    {
                        List<int> accountIds = accountsOnDim.Select(s => s.AccountId).ToList();
                        foreach (var shift in shifts.Where(x => !x.IsBreak).ToList())
                        {
                            var accountOnShift = shift.AccountInternals?.FirstOrDefault(x => accountIds.Contains(x.AccountId));
                            if (accountOnShift != null)
                            {
                                var account = accountInternals.FirstOrDefault(x => x.AccountId == accountOnShift.AccountId);
                                if (account != null)
                                {
                                    shift.GroupName = account.Name;
                                    var shiftBreaks = shift.GetOverlappedBreaks(shifts.Where(x => x.IsBreak).ToList(), true);
                                    foreach (var shiftBreak in shiftBreaks)
                                        shiftBreak.GroupName = account.Name;
                                }
                            }
                        }
                    }
                }
            }
        }

        protected List<PayrollProductReportSetting> GetPayrollProductReportSettings(int actorCompanyId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return base.GetPayrollProductReportSettingsForCompanyFromCache(entitiesReadOnly, CacheConfig.Company(actorCompanyId));
        }

        public List<EmployeeDTO> GetSortedEmployees(TermGroup_TimeSchedulePlanningDayViewSortBy sortSelection, List<TimeEmployeeScheduleDataSmallDTO> shifts, List<Employee> employeess)
        {

            List<EmployeeDTO> employees = employeess.ToDTOs().ToList();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var hiddenEmployeeId = GetHiddenEmployeeIdFromCache(entitiesReadOnly, CacheConfig.Company(base.ActorCompanyId));
            int cloneId = int.MaxValue - 1000;

            if (hiddenEmployeeId != 0 && employees.Any(a => a.EmployeeId == hiddenEmployeeId))
            {
                var hiddenEmployee = employees.FirstOrDefault(a => a.EmployeeId == hiddenEmployeeId);
                int addedHidden = 1;
                foreach (var shiftGroup in shifts.GroupBy(g => $"{(g.EmployeeId == hiddenEmployeeId ? g.Link : string.Empty)}"))
                {
                    if (shiftGroup.First().EmployeeId == hiddenEmployeeId)
                    {

                        if (addedHidden > 1)
                        {
                            var hiddenEmployeeClone = hiddenEmployee.CloneDTO();
                            hiddenEmployeeClone.EmployeeId = cloneId;
                            employees.Add(hiddenEmployeeClone);

                            foreach (var shift in shiftGroup)
                            {
                                shift.EmployeeId = cloneId;
                            }
                        }

                        cloneId++;
                        addedHidden++;
                    }
                }
            }
            if (shifts.Any(s => s.Type == TermGroup_TimeScheduleTemplateBlockType.OnDuty))
            {
                string onDutyTerm = GetText(12165, "Jour");
                bool onDutyTermAddedOnFirst = false;
                int addedOnDuty = shifts.Any(s => s.Type != TermGroup_TimeScheduleTemplateBlockType.OnDuty) ? 1 : 0;
                foreach (var shiftGroup in shifts.Where(s => s.Type == TermGroup_TimeScheduleTemplateBlockType.OnDuty).GroupBy(g => g.Link))
                {
                    var onDutyEmployee = employees.FirstOrDefault(a => a.EmployeeId == shiftGroup.Select(g => g.EmployeeId).First());

                    if (addedOnDuty > 0)
                    {
                        var onDutyEmployeeClone = onDutyEmployee.CloneDTO();
                        onDutyEmployeeClone.EmployeeId = cloneId;
                        if (!onDutyTermAddedOnFirst)
                            onDutyEmployeeClone.Name = $"{onDutyEmployee.Name} ({onDutyTerm})";
                        employees.Add(onDutyEmployeeClone);

                        foreach (var shift in shiftGroup)
                        {
                            shift.EmployeeId = cloneId;
                        }
                    }
                    if (addedOnDuty == 0)
                    {
                        onDutyEmployee.Name = $"{onDutyEmployee.Name} ({onDutyTerm})";
                        onDutyTermAddedOnFirst = true;
                    }

                    cloneId++;
                    addedOnDuty++;
                }
            }

            var uniqueEmployeeIds = shifts.Select(x => x.EmployeeId).Distinct();
            var uniqueEmployees = employees.Where(x => uniqueEmployeeIds.Contains(x.EmployeeId)).ToList();

            foreach (var employee in uniqueEmployees)
            {
                var employeeShifts = shifts.Where(x => x.EmployeeId == employee.EmployeeId).ToList();
                if (employeeShifts.Any())
                    employee.OrderDate = employeeShifts.OrderBy(x => x.StartTime).First().StartTime;
            }

            switch (sortSelection)
            {
                case TermGroup_TimeSchedulePlanningDayViewSortBy.Firstname:
                    uniqueEmployees = uniqueEmployees.OrderByDescending(x => x.Hidden).ThenByDescending(x => x.Vacant).ThenBy(x => x.FirstName).ThenBy(x => x.LastName).ThenBy(x => x.EmployeeNrSort).ToList();
                    break;
                case TermGroup_TimeSchedulePlanningDayViewSortBy.Lastname:
                    uniqueEmployees = uniqueEmployees.OrderByDescending(x => x.Hidden).ThenByDescending(x => x.Vacant).ThenBy(x => x.LastName).ThenBy(x => x.FirstName).ThenBy(x => x.EmployeeNrSort).ToList();
                    break;
                case TermGroup_TimeSchedulePlanningDayViewSortBy.EmployeeNr:
                    uniqueEmployees = uniqueEmployees.OrderByDescending(x => x.Hidden).ThenByDescending(x => x.Vacant).ThenBy(x => x.EmployeeNrSort).ToList();
                    break;
                case TermGroup_TimeSchedulePlanningDayViewSortBy.StartTime:
                    uniqueEmployees = uniqueEmployees.OrderByDescending(x => x.Hidden).ThenByDescending(x => x.Vacant).ThenBy(x => x.OrderDate).ToList();
                    break;
                default:
                    break;
            }

            return uniqueEmployees;
        }

        #endregion
    }
}
