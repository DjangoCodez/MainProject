using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.Reporting;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace SoftOne.Soe.Business.Util.ExportFiles
{
    public class KU10 : ExportFilesBase
    {
        #region Ctor

        public KU10(ParameterObject parameterObject, CreateReportResult reportResult) : base(parameterObject, reportResult)
        {
            this.reportResult = reportResult;
        }

        #endregion

        #region Namespaces

        // Namespaces: From Swedish tax officials documentation
        private readonly XNamespace namespaceDefault = "http://xmls.skatteverket.se/se/skatteverket/ai/instans/infoForBeskattning/4.0";
        private readonly XNamespace namespaceKU = "http://xmls.skatteverket.se/se/skatteverket/ai/komponent/infoForBeskattning/4.0";
        private readonly XNamespace namespaceGM = "http://xmls.skatteverket.se/se/skatteverket/ai/gemensamt/infoForBeskattning/4.0";
        private readonly XNamespace namespacexsi = "http://www.w3.org/2001/XMLSchema-instance";
        private readonly XNamespace schemaLocation = XNamespace.Get("http://xmls.skatteverket.se/se/skatteverket/ai/instans/infoForBeskattning/4.0 http://xmls-utv.skatteverket.se/schemalager/se/skatteverket/ai/kontrolluppgift/instans/Kontrolluppgifter_4.0.xsd");
        private readonly XNamespace namespaceDefaultAGD = "http://xmls.skatteverket.se/se/skatteverket/da/instans/schema/1.1";
        private readonly XNamespace namespaceAGD = "http://xmls.skatteverket.se/se/skatteverket/da/komponent/schema/1.1";
        private readonly XNamespace namespacexsiAGD = "http://www.w3.org/2001/XMLSchemainstance";

        #endregion

        #region Public methods 

        public string CreateKU10File(CompEntities entities)
        {
            #region Init

            if (es == null)
                return null;

            #endregion

            #region Prereq

            Company company = CompanyManager.GetCompany(reportResult.ActorCompanyId);
            string orgNr = CleanOrgNr(company.OrgNr);


            Company = CompanyManager.GetCompany(reportResult.ActorCompanyId);
            if (Company == null)
                return null;

            if (!TryGetIdsFromSelection(reportResult, out List<int> selectionTimePeriodIds, "periods"))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionTimePeriodIds, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return null;

            TryGetDatesFromSelection(reportResult, selectionTimePeriodIds, out DateTime selectionDateFrom, out DateTime selectionDateTo, out _, alwaysLoadPeriods: true);
            selectionDateTo = CalendarUtility.GetEndOfYear(selectionDateFrom);

            TryGetBoolFromSelection(reportResult, out bool removePrevSubmittedData, "removePrevSubmittedData");
            removePrevSubmittedData = reportResult.EvaluatedSelection != null ? reportResult.EvaluatedSelection.ST_KU10RemovePrevSubmittedData : removePrevSubmittedData;

            if (employees == null)
                employees = EmployeeManager.GetAllEmployeesByIds(entities, base.ActorCompanyId, selectionEmployeeIds, loadEmployment: true);

            User user = UserManager.GetUser(reportResult.UserId, loadEmployee: true);
            Employee userEmployee = null;
            if (!user.Employee.IsNullOrEmpty())
                userEmployee = EmployeeManager.GetEmployee(user.Employee.FirstOrDefault().EmployeeId, reportResult.ActorCompanyId, loadContactPerson: true);
            string contactPerson = userEmployee?.ContactPerson?.Name ?? user.Name;

            List<ContactAddressItem> contactAddressItems = ContactManager.GetContactAddressItems(company.ActorCompanyId);
            List<TimePayrollStatisticsSmallDTO> timePayrollTransactionItems = TimeTransactionManager.GetTimePayrollStatisticsSmallDTOs_new(entities, reportResult.ActorCompanyId, employees, selectionTimePeriodIds, ignoreAccounting: true);

            #endregion

            #region Init repository

            PersonalDataEmployeeReportRepository personalDataRepository = new PersonalDataEmployeeReportRepository(parameterObject, es);

            #endregion

            #region Init document

            XDocument document = XmlUtil.CreateDocument(Encoding.UTF8, true);

            #endregion

            #region Create root with namespaces

            XElement root = new XElement(namespaceDefault + "Skatteverket");
            root.Add(new XAttribute(XNamespace.Xmlns + "ku", this.namespaceKU));
            root.Add(new XAttribute(XNamespace.Xmlns + "xsi", this.namespacexsi));
            root.Add(new XAttribute("omrade", "Kontrolluppgifter"));
            root.Add(new XAttribute(this.namespacexsi + "schemaLocation", this.schemaLocation));

            #endregion

            #region Content

            XElement elementAvsandare = CreateElementAvsandare(this.namespaceKU, company);
            elementAvsandare.Add(CreateElementTekniskKontaktperson(this.namespaceKU, company, contactPerson, contactAddressItems));
            elementAvsandare.Add(CreateElementSkapad(this.namespaceKU));
            root.Add(elementAvsandare);

            XElement elementBlankettgemensamt = CreateElementBlankettgemensamt(this.namespaceKU);
            XElement elementUppgiftslamnare = new XElement(this.namespaceKU + "Uppgiftslamnare");
            elementUppgiftslamnare.Add(new XElement(this.namespaceKU + "UppgiftslamnarePersOrgnr", orgNr));
            elementUppgiftslamnare.Add(CreateElementKontaktperson(this.namespaceKU, contactPerson, contactAddressItems));
            elementBlankettgemensamt.Add(elementUppgiftslamnare);
            root.Add(elementBlankettgemensamt);

            foreach (Employee employee in employees)
            {
                List<TimePayrollStatisticsSmallDTO> timePayrollTransactionItemsEmployee = timePayrollTransactionItems.Where(e => e.EmployeeId == employee.EmployeeId && !e.IsEmploymentTaxAndHidden).ToList();

                XElement elementEmployee = this.CreateElementKU10Employee(reportResult, timePayrollTransactionItemsEmployee, employee, orgNr, removePrevSubmittedData, selectionDateFrom, selectionDateTo);
                if (elementEmployee != null)
                {
                    root.Add(elementEmployee);
                    personalDataRepository.AddEmployee(employee, elementEmployee);
                }
            }

            // Write filled root into document
            document.Add(root);
            string validated = ValidateKUXDocument(document);

            #endregion

            #region Create & save to file

            string fileName = IOUtil.FileNameSafe(company.Name + "_KU10_" + GetYearMonthDay(es.DateFrom) + " - " + GetYearMonthDay(es.DateTo));
            DirectoryInfo directory = Directory.CreateDirectory(ConfigSettings.SOE_SERVER_DIR_TEMP_REPORT_PHYSICAL + Guid.NewGuid().ToString());
            string filePath = StringUtility.GetValidFilePath(directory.FullName) + fileName + ".xml";

            if (string.IsNullOrEmpty(validated))
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
                    validated = "####################  VALIDATION ERROR IN FILE ###########################" + Environment.NewLine + Environment.NewLine + validated + Environment.NewLine + Environment.NewLine + "####################  VALIDATION ERROR IN FILE ###########################" + Environment.NewLine + Environment.NewLine + document.ToString();
                    filePath = StringUtility.GetValidFilePath(directory.FullName) + "ErrorInFile_fileName" + ".xml";
                    File.WriteAllText(filePath, validated);
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

            return filePath;
        }

        public string CreateAgdEmployeeFile(CompEntities entities)
        {
            #region Init

            #endregion

            #region Prereq

            if (!TryGetIdsFromSelection(reportResult, out List<int> selectionTimePeriodIds, "periods"))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionTimePeriodIds, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return null;

            TryGetDatesFromSelection(reportResult, selectionTimePeriodIds, out DateTime selectionDateFrom, out DateTime selectionDateTo, out var selectedTimePeriods, alwaysLoadPeriods: true);

            selectedTimePeriods = selectedTimePeriods.Where(x => x.PaymentDate.HasValue).ToList();
            if (selectedTimePeriods.IsNullOrEmpty())
                return null;

            TryGetBoolFromSelection(reportResult, out bool removePrevSubmittedData, "removePrevSubmittedData");
            TryGetBoolFromSelection(reportResult, out bool boolIncludeAbsence, "includeAbsence");

            if (employees == null)
                employees = EmployeeManager.GetAllEmployeesByIds(entities, base.ActorCompanyId, selectionEmployeeIds, loadEmployment: true);

            removePrevSubmittedData = reportResult.EvaluatedSelection != null ? reportResult.EvaluatedSelection.ST_KU10RemovePrevSubmittedData : removePrevSubmittedData;
            var includeAbsence = boolIncludeAbsence || ReportResult.ExportFileType == TermGroup_ReportExportFileType.AGD_Franvarouppgift;

            Company company = CompanyManager.GetCompany(reportResult.ActorCompanyId);
            string orgNr = CleanOrgNr(company.OrgNr);
            string specifikationsnummer = DateTime.Now.Millisecond.ToString().Right(10);

            Employee userEmployee = null;
            User user = UserManager.GetUser(reportResult.UserId, loadEmployee: true);
            if (!user.Employee.IsNullOrEmpty())
                userEmployee = EmployeeManager.GetEmployee(user.Employee.FirstOrDefault().EmployeeId, reportResult.ActorCompanyId, loadContactPerson: true);
            string contactPerson = userEmployee != null && userEmployee.ContactPerson != null ? userEmployee.ContactPerson.Name : user.Name;
            int maxAvdragRegionaltStod = Convert.ToInt32(SettingManager.GetDecimalSetting(SettingMainType.Company, (int)CompanySettingType.PayrollMaxRegionalSupportAmount, 0, reportResult.ActorCompanyId, 0));
            int avdragRegionaltStodProcent = Convert.ToInt32(SettingManager.GetDecimalSetting(SettingMainType.Company, (int)CompanySettingType.PayrollMaxRegionalSupportPercent, 0, reportResult.ActorCompanyId, 0));

            List<ContactAddressItem> contactAddressItems = ContactManager.GetContactAddressItems(company.ActorCompanyId);
            List<TimePayrollStatisticsSmallDTO> timePayrollTransactionItems = TimeTransactionManager.GetTimePayrollStatisticsSmallDTOs_new(entities, reportResult.ActorCompanyId, employees, selectionTimePeriodIds, applyEmploymentTaxMinimumRule: true, ignoreAccounting: true, isAgd: true);

            DateTime date = selectedTimePeriods.OrderByDescending(i => i.PaymentDate).FirstOrDefault().PaymentDate.Value;
            if (date < new DateTime(2018, 7, 1))
                date = new DateTime(2018, 7, 1);

            string period = "";
            if (date > DateTime.Now.AddYears(-10))
            {
                string month = $"{(date.Month.ToString().Length == 1 ? $"0{date.Month}" : date.Month.ToString())}";
                period = $"{date.Year}{month}";
            }

            #endregion

            #region Init repository

            PersonalDataEmployeeReportRepository personalDataRepository = new PersonalDataEmployeeReportRepository(parameterObject, reportResult);

            #endregion

            #region Init document

            XDocument document = new XDocument(new XDeclaration("1.0", "UTF-8", "no"));

            #endregion

            #region Create root with namespaces

            XElement root = new XElement(namespaceDefaultAGD + "Skatteverket");
            root.Add(new XAttribute(XNamespace.Xmlns + "agd", this.namespaceAGD));
            root.Add(new XAttribute(XNamespace.Xmlns + "xsi", this.namespacexsiAGD));
            root.Add(new XAttribute("omrade", "Kontrolluppgifter"));

            #endregion

            #region Content

            //Avsandare
            XElement elementAvsandare = CreateElementAvsandare(this.namespaceAGD, company);
            elementAvsandare.Add(CreateElementTekniskKontaktperson(this.namespaceAGD, company, contactPerson, contactAddressItems));
            elementAvsandare.Add(CreateElementSkapad(this.namespaceAGD));
            root.Add(elementAvsandare);

            //Arbetsgivare
            XElement elementArbetsgivare = CreateElementArbetsgivare(this.namespaceAGD, company);
            elementArbetsgivare.Add(CreateElementKontaktperson(this.namespaceAGD, contactPerson, contactAddressItems));

            //Blankettgemensamt
            XElement elementBlankettgemensamt = CreateElementBlankettgemensamt(this.namespaceAGD);
            elementBlankettgemensamt.Add(elementArbetsgivare);
            root.Add(elementBlankettgemensamt);

            List<AgdEmployeeDTO> agdEmployeeDTOs = new List<AgdEmployeeDTO>();

            foreach (var employeeGroupOnSocialSec in employees.GroupBy(b => b.SocialSec + "#" + b.ContactPerson.Name))
            {
                Employee employee = employeeGroupOnSocialSec.First();
                List<int> employeeIds = employeeGroupOnSocialSec.Select(s => s.EmployeeId).ToList();

                List<TimePayrollStatisticsSmallDTO> timePayrollTransactionItemsEmployee = timePayrollTransactionItems.Where(e => employeeIds.Contains(e.EmployeeId)).ToList();

                AgdEmployeeDTO agdEmployeeDTO;
                XElement element = CreateElementAgdEmployee(timePayrollTransactionItemsEmployee, employee, orgNr, specifikationsnummer, period, removePrevSubmittedData, selectionDateFrom, selectionDateTo, date, out agdEmployeeDTO, includeAbsence);
                if (element != null)
                {
                    agdEmployeeDTOs.Add(agdEmployeeDTO);
                    if (reportResult.ExportFileType != TermGroup_ReportExportFileType.AGD_Franvarouppgift)
                        root.Add(element);
                }

                foreach (var e in employeeGroupOnSocialSec)
                {
                    personalDataRepository.AddEmployee(e, element);
                }
            }

            maxAvdragRegionaltStod = CalulateAvdragRegionaltStod(avdragRegionaltStodProcent, maxAvdragRegionaltStod, agdEmployeeDTOs.Sum(s => s.RegionaltStodUlagAG));
           
            if (ReportResult.ExportFileType != TermGroup_ReportExportFileType.AGD_Franvarouppgift)
            {
                root.Add(CreateElementArbetsgivareHUGROUP(orgNr, period, maxAvdragRegionaltStod, agdEmployeeDTOs));
            }

            if (includeAbsence)
            {
                var franvaroSpecifikationsnummer = 1;
                foreach (var agdEmployeeDTO in agdEmployeeDTOs)
                {
                    var validTransaction = agdEmployeeDTO.Transactions.Where(w => w.Type == "TemporaryParentalLeave" || w.Type == "ParentalLeave");
                    foreach (var trans in validTransaction.GroupBy(g => g.Type))
                    { 
                        foreach(var dayTrans in trans.GroupBy(g=> g.Date))
                        {
                            if (dayTrans.Sum(s => s.Quantity) == 0)
                                continue;

                            var type = dayTrans.FirstOrDefault().Type;
                            decimal sum = Decimal.Round((dayTrans.Sum(s => s.Quantity) / 60), 2);
                            DateTime transDate = dayTrans.FirstOrDefault().Date;

                            var rowFU = CreateElementArbetsgivareFU(orgNr, period, agdEmployeeDTO, transDate, type, sum, franvaroSpecifikationsnummer);
                            if (rowFU != null)
                            {
                                root.Add(rowFU);
                                franvaroSpecifikationsnummer++;
                            }
                        }
                    }
                }
            }
          
            // Write filled root into document
            document.Add(root);

            #endregion

            #region Validation

            string validated = ValidateAgdXDocument(document);

            #endregion

            #region Create & save to file

            string fileName = IOUtil.FileNameSafe(company.Name + "_KU10_" + GetYearMonthDay(selectionDateFrom) + " - " + GetYearMonthDay(selectionDateTo));
            DirectoryInfo directory = Directory.CreateDirectory(ConfigSettings.SOE_SERVER_DIR_TEMP_REPORT_PHYSICAL + Guid.NewGuid().ToString());
            string filePath = StringUtility.GetValidFilePath(directory.FullName) + fileName + ".xml";

            if (string.IsNullOrEmpty(validated))
            {
                try
                {
                    var asText = ToXml(document);
                    File.WriteAllText(filePath, asText);
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
                    validated = "####################  VALIDATION ERROR IN FILE ###########################" + Environment.NewLine + Environment.NewLine + validated + Environment.NewLine + Environment.NewLine + "####################  VALIDATION ERROR IN FILE ###########################" + Environment.NewLine + Environment.NewLine + document.ToString();
                    filePath = StringUtility.GetValidFilePath(directory.FullName) + "ErrorInFile_fileName" + ".xml";
                    File.WriteAllText(filePath, validated);
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

            return filePath;
        }

        public KU10EmployeeDTO CreateKU10EmployeeDTO(List<TimePayrollStatisticsSmallDTO> timePayrollTransactionItems, Employee employee, string year, EmployeeTaxSE employeeTaxSE, bool removePrevSubmittedData, DateTime selectionDateFrom, DateTime selectionDateTo)
        {
            KU10EmployeeDTO kU10EmployeeDTO = new KU10EmployeeDTO();
            kU10EmployeeDTO.Transactions = new List<KU10EmployeeTransactionDTO>();
            kU10EmployeeDTO.Inkomsttagare = CleanSocSec(employee.SocialSec);

            EmployeeTaxSE taxSE = employeeTaxSE;
            bool addInfo = false;
            if (removePrevSubmittedData)
            {
                kU10EmployeeDTO.Borttag = true;
                addInfo = true;
            }
            else
            {
                List<EmployeeVehicle> employeeVehicles = EmployeeManager.GetEmployeeVehicles(employee.EmployeeId, reportResult.ActorCompanyId);
                Employment firstEmployment = employee.Employment.GetFirstEmployment();
                Employment lastEmployment = employee.Employment.GetLastEmployment();
                DateTime EmploymentStartDate = firstEmployment != null && firstEmployment.DateFrom.HasValue ? firstEmployment.DateFrom.Value : DateTime.MinValue;
                DateTime EmploymentEndDate = lastEmployment != null && lastEmployment.DateTo.HasValue ? lastEmployment.DateTo.Value : DateTime.MaxValue;

                //Check if start and enddate is within year, gör en kontroll även på 1/1
                string anstalldFrom = string.Empty;
                string anstalldTom = string.Empty;
                if (EmploymentStartDate <= selectionDateFrom)
                    anstalldFrom = "1";
                if (EmploymentEndDate >= selectionDateTo)
                    anstalldTom = "12";
                if (EmploymentStartDate > selectionDateFrom)
                    anstalldFrom = EmploymentStartDate.Month.ToString();
                if (EmploymentEndDate < selectionDateTo)
                    anstalldTom = EmploymentEndDate.Month.ToString();

                List<int> skipTransactionIds = timePayrollTransactionItems.Where(p => !p.IsScheduleTransaction).Select(t => t.TransactionId).ToList();
                timePayrollTransactionItems.AddRange(TimeTransactionManager.GetPayrollStartValuesAsTimePayrollStatisticsSmallDTOs(employee, ActorCompanyId, selectionDateFrom.Year, skipTransactionIds: skipTransactionIds));
                if (timePayrollTransactionItems.IsNullOrEmpty())
                    return null;

                foreach (var transaction in timePayrollTransactionItems)
                {
                    #region Transaction

                    //AvdragenSkatt
                    if (transaction.IsTax())
                    {
                        kU10EmployeeDTO.AvdragenSkatt += transaction.Amount; //TODO: SINK+ASINK
                        kU10EmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "AvdragenSkatt"));
                    }

                    //KontantBruttolonMm
                    if ((taxSE == null || taxSE.EmploymentTaxType != (int)TermGroup_EmployeeTaxEmploymentTaxType.PayrollTax) && transaction.IsGrossSalary())
                    {
                        kU10EmployeeDTO.KontantBruttolonMm += transaction.Amount;
                        kU10EmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "KontantBruttolonMm"));
                    }

                    //FormanUtomBilDrivmedel
                    if (transaction.IsBenefit_Not_CompanyCar_And_FuelBenefit())
                    {
                        kU10EmployeeDTO.FormanUtomBilDrivmedel += transaction.Amount;
                        kU10EmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "FormanUtomBilDrivmedel"));
                    }

                    //BilformanUtomDrivmedel
                    if (transaction.IsBenefit_CompanyCar())
                    {
                        kU10EmployeeDTO.BilformanUtomDrivmedel += transaction.Amount;
                        kU10EmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "BilformanUtomDrivmedel"));
                    }

                    //KmBilersVidBilforman
                    if (transaction.IsCompensation_CarCompensation_BenefitCar())
                    {
                        kU10EmployeeDTO.KmBilersVidBilforman += transaction.Quantity;
                        kU10EmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "KmBilersVidBilforman"));
                    }

                    //Bilersattning
                    if (transaction.IsCompensation_CarCompensation_PrivateCar())
                    {
                        kU10EmployeeDTO.Bilersattning = true;
                    }

                    //BetaltForBilforman
                    if (transaction.IsDeductionCarBenefit())
                    {
                        kU10EmployeeDTO.BetaltForBilforman += transaction.Amount;
                        kU10EmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "BetaltForBilforman"));
                    }

                    //DrivmedelVidBilforman
                    if (transaction.IsBenefit_Fuel())
                    {
                        kU10EmployeeDTO.DrivmedelVidBilforman += transaction.Amount;
                        kU10EmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "DrivmedelVidBilforman"));
                    }

                    //AndraKostnadsers
                    if (transaction.IsCompensation_Other_Taxable())
                    {
                        kU10EmployeeDTO.AndraKostnadsers += transaction.Amount;
                        kU10EmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "AndraKostnadsers"));
                    }

                    //UnderlagRutarbete
                    if (transaction.IsBenefit_RUT())
                    {
                        kU10EmployeeDTO.UnderlagRutarbete += transaction.Amount;
                        kU10EmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "UnderlagRutarbete"));
                    }

                    //UnderlagRotarbeteHyresersattning
                    if (transaction.IsBenefit_ROT())
                    {
                        kU10EmployeeDTO.UnderlagRotarbete += transaction.Amount;
                        kU10EmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "UnderlagRotarbete"));
                    }

                    //ErsMEgenavgifter
                    if (taxSE != null && taxSE.EmploymentTaxType == (int)TermGroup_EmployeeTaxEmploymentTaxType.PayrollTax && transaction.IsGrossSalary())
                    {
                        kU10EmployeeDTO.ErsMEgenavgifter += transaction.Amount;
                        kU10EmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "ErsMEgenavgifter"));
                    }

                    //Tjanstepension
                    if (transaction.IsOccupationalPension())
                    {
                        kU10EmployeeDTO.Tjanstepension += transaction.Amount;
                        kU10EmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "Tjanstepension"));
                    }

                    //ErsEjSocAvg
                    //if (transaction.IsEmploymentTax())
                    //{
                    //    kU10EmployeeDTO.ErsEjSocAvg += trans.Amount;
                    //    kU10EmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "ErsEjSocAvg"));
                    //}

                    //ErsEjSocAvgEjJobbavd
                    //if (transaction.IsEmploymentTax())
                    //{
                    //    kU10EmployeeDTO.ErsEjSocAvgEjJobbavd += transaction.Amount;
                    //    kU10EmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "ErsEjSocAvgEjJobbavd"));
                    //}

                    //Forskarskattenamnden
                    //if (transaction.IsEmploymentTax())
                    //{
                    //    kU10EmployeeDTO.Forskarskattenamnden += transaction.Amount;
                    //    kU10EmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "Forskarskattenamnden"));
                    //}

                    //VissaAvdrag
                    //if (transaction.IsEmploymentTax())
                    //{
                    //    kU10EmployeeDTO.VissaAvdrag += transaction.Amount;
                    //    kU10EmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "VissaAvdrag"));
                    //}

                    //Hyresersattning
                    if (transaction.IsCompensation_Rental())
                    {
                        kU10EmployeeDTO.Hyresersattning += transaction.Amount;
                        kU10EmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "Hyresersattning"));
                    }

                    //BostadSmahus
                    if (transaction.IsBenefit_PropertyHouse())
                    {
                        kU10EmployeeDTO.BostadSmahus = true;
                        kU10EmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "BostadSmahus"));
                    }

                    //Kost
                    if (transaction.IsBenefit_Food())
                    {
                        kU10EmployeeDTO.Kost = true;
                        kU10EmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "Kost"));
                    }

                    //BostadEjSmahus
                    if (transaction.IsBenefit_PropertyNotHouse())
                    {
                        kU10EmployeeDTO.BostadEjSmahus = true;
                        kU10EmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "BostadEjSmahus"));
                    }

                    //Ranta
                    if (transaction.IsBenefit_Interest())
                    {
                        kU10EmployeeDTO.Ranta = true;
                        kU10EmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "Ranta"));
                    }

                    //Parkering
                    if (transaction.IsBenefit_Parking())
                    {
                        kU10EmployeeDTO.Parkering = true;
                        kU10EmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "Parkering"));
                    }

                    //AnnanForman
                    if (transaction.IsBenefit_Other())
                    {
                        kU10EmployeeDTO.AnnanForman = true;
                        kU10EmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "AnnanForman"));
                        if (String.IsNullOrEmpty(kU10EmployeeDTO.SpecAvAnnanForman))
                            kU10EmployeeDTO.SpecAvAnnanForman = transaction.PayrollProductName;
                        else if (!kU10EmployeeDTO.SpecAvAnnanForman.Contains(transaction.PayrollProductName))
                            kU10EmployeeDTO.SpecAvAnnanForman += "," + transaction.PayrollProductName;

                    }

                    //FormanHarJusterats
                    //if (transaction.IsBenefit_Other())
                    //{
                    //    kU10EmployeeDTO.FormanHarJusterats = true;
                    //    kU10EmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "FormanHarJusterats"));
                    //}

                    //FormanSomPension
                    if (employee.BenefitAsPension)
                        kU10EmployeeDTO.FormanSomPension = true;

                    //TraktamenteInomRiket
                    if (transaction.IsCompensation_TravelAllowance_DomesticShortTerm())
                    {
                        kU10EmployeeDTO.TraktamenteInomRiket = true;
                        kU10EmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "TraktamenteInomRiket"));
                    }

                    //TraktamenteUtomRiket
                    if (transaction.IsCompensation_TravelAllowance_ForeignShortTerm())
                    {
                        kU10EmployeeDTO.TraktamenteUtomRiket = true;
                        kU10EmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "TraktamenteUtomRiket"));
                    }

                    //TjansteresaOver3MInrikes
                    if (transaction.IsCompensation_TravelAllowance_DomesticLongTermOrOverTwoYears())
                    {
                        kU10EmployeeDTO.TjansteresaOver3MInrikes = true;
                        kU10EmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "TjansteresaOver3MInrikes"));
                    }

                    //TjansteresaOver3MUtrikes
                    if (transaction.IsCompensation_TravelAllowance_ForeignLongTermOrOverTwoYears())
                    {
                        kU10EmployeeDTO.TjansteresaOver3MUtrikes = true;
                        kU10EmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "TjansteresaOver3MUtrikes"));
                    }

                    //Resekostnader
                    if (transaction.IsCompensation_TravelCost())
                    {
                        kU10EmployeeDTO.Resekostnader = true;
                        kU10EmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "Resekostnader"));
                    }

                    //Logi
                    if (transaction.IsCompensation_Accomodation())
                    {
                        kU10EmployeeDTO.Logi = true; //Missing?
                        kU10EmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "Logi"));
                    }

                    //Logi
                    if (transaction.IsPersonellAcquisitionOptions())
                    {
                        kU10EmployeeDTO.PersonaloptionForvarvAndel = 0; //TODO
                        kU10EmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "PersonaloptionForvarvAndel"));
                    }

                    //PartnerInCloseCompany
                    if (employee.PartnerInCloseCompany)
                    {
                        kU10EmployeeDTO.Delagare = true; //Delagare
                        addInfo = true;
                    }

                    //SpecVissaAvdrag
                    //if (transaction.IsEmploymentTax())
                    //{
                    //    kU10EmployeeDTO.SpecVissaAvdrag += transaction.Amount;
                    //    kU10EmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "SpecVissaAvdrag"));
                    //}

                    //LandskodTIN
                    //if (transaction.IsEmploymentTax())
                    //{
                    //    kU10EmployeeDTO.LandskodTIN += transaction.Amount;
                    //    kU10EmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "LandskodTIN"));
                    //}

                    //SocialAvgiftsAvtal
                    //if (transaction.IsEmploymentTax())
                    //{
                    //    kU10EmployeeDTO.SocialAvgiftsAvtal += transaction.Amount;
                    //    kU10EmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "SocialAvgiftsAvtal"));
                    //}

                    //TIN
                    //if (transaction.IsEmploymentTax())
                    //{
                    //    kU10EmployeeDTO.TIN += transaction.Amount;
                    //    kU10EmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "TIN"));
                    //}

                    //FormanUtomBilDrivmedel
                    // element.FormanUtomBilDrivmedel = Total av rutan 41-49 
                    //if (transaction.IsBenefit_PropertyHouse() ||
                    //     transaction.IsBenefit_Food() ||
                    //     transaction.IsBenefit_PropertyNotHouse() ||
                    //     transaction.IsBenefit_Interest() ||
                    //     transaction.IsBenefit_Parking() ||
                    //     transaction.IsBenefit_Other() ||
                    //     transaction.IsBenefit_Other() ||
                    //     transaction.IsEmploymentTax())
                    //{
                    //    kU10EmployeeDTO.FormanUtomBilDrivmedel += transaction.Amount;
                    //}

                    #endregion
                }

                kU10EmployeeDTO.Specifikationsnummer = 1;
                kU10EmployeeDTO.Inkomstar = year;
                kU10EmployeeDTO.AntalManBilforman = 0;
                kU10EmployeeDTO.AnstalldFrom = anstalldFrom;
                kU10EmployeeDTO.AnstalldTom = anstalldTom;
                kU10EmployeeDTO.LandskodTIN = taxSE != null && !string.IsNullOrEmpty(taxSE.CountryCode) ? taxSE.CountryCode : string.Empty;
                kU10EmployeeDTO.TIN = taxSE != null && !string.IsNullOrEmpty(taxSE.TinNumber) ? taxSE.TinNumber : string.Empty;

                #region Bilförmån

                if (!employeeVehicles.IsNullOrEmpty())
                {
                    employeeVehicles = employeeVehicles.OrderBy(e => e.CalculatedFromDate).ToList();
                    EmployeeVehicle firstEmployeeVehicle = employeeVehicles.FirstOrDefault();
                    EmployeeVehicle lastEmployeeVehicle = employeeVehicles.LastOrDefault();

                    kU10EmployeeDTO.KodForFormansbil = employeeVehicles.LastOrDefault().ModelCode;

                    int checkedMonths = 1;
                    while (checkedMonths <= 12)
                    {
                        bool hadCarInMonth = false;
                        foreach (EmployeeVehicle employeeVehicle in employeeVehicles)
                        {
                            hadCarInMonth = CalendarUtility.IsDatesOverlapping(employeeVehicle.CalculatedFromDate, employeeVehicle.CalculatedToDate, CalendarUtility.GetBeginningOfMonth(selectionDateFrom.AddMonths(checkedMonths - 1)), CalendarUtility.GetEndOfMonth(CalendarUtility.GetBeginningOfMonth(selectionDateFrom.AddMonths(checkedMonths - 1))));
                            if (hadCarInMonth)
                            {
                                kU10EmployeeDTO.AntalManBilforman++;
                                addInfo = true;
                            }
                        }
                        checkedMonths++;
                    }
                }

                #endregion

                #region Fix negatives

                kU10EmployeeDTO.AvdragenSkatt = Math.Abs(kU10EmployeeDTO.AvdragenSkatt);
                kU10EmployeeDTO.KontantBruttolonMm = kU10EmployeeDTO.KontantBruttolonMm > 0 ? kU10EmployeeDTO.KontantBruttolonMm : 0;
                kU10EmployeeDTO.FormanUtomBilDrivmedel = kU10EmployeeDTO.FormanUtomBilDrivmedel > 0 ? kU10EmployeeDTO.FormanUtomBilDrivmedel : 0;
                kU10EmployeeDTO.BilformanUtomDrivmedel = kU10EmployeeDTO.BilformanUtomDrivmedel > 0 ? kU10EmployeeDTO.BilformanUtomDrivmedel : 0;
                kU10EmployeeDTO.AntalManBilforman = kU10EmployeeDTO.AntalManBilforman > 0 ? kU10EmployeeDTO.AntalManBilforman : 0;
                kU10EmployeeDTO.KmBilersVidBilforman = kU10EmployeeDTO.KmBilersVidBilforman > 0 ? kU10EmployeeDTO.KmBilersVidBilforman : 0;
                kU10EmployeeDTO.BetaltForBilforman = kU10EmployeeDTO.BetaltForBilforman > 0 ? kU10EmployeeDTO.BetaltForBilforman : Math.Abs(kU10EmployeeDTO.BetaltForBilforman);
                kU10EmployeeDTO.DrivmedelVidBilforman = Math.Abs(kU10EmployeeDTO.DrivmedelVidBilforman);
                kU10EmployeeDTO.AndraKostnadsers = kU10EmployeeDTO.AndraKostnadsers > 0 ? kU10EmployeeDTO.AndraKostnadsers : 0;
                kU10EmployeeDTO.UnderlagRutarbete = kU10EmployeeDTO.UnderlagRutarbete > 0 ? kU10EmployeeDTO.UnderlagRutarbete : 0;
                kU10EmployeeDTO.UnderlagRotarbete = kU10EmployeeDTO.UnderlagRotarbete > 0 ? kU10EmployeeDTO.UnderlagRotarbete : 0;
                kU10EmployeeDTO.ErsMEgenavgifter = kU10EmployeeDTO.ErsMEgenavgifter > 0 ? kU10EmployeeDTO.ErsMEgenavgifter : 0;
                kU10EmployeeDTO.Tjanstepension = kU10EmployeeDTO.Tjanstepension > 0 ? kU10EmployeeDTO.Tjanstepension : 0;
                kU10EmployeeDTO.ErsEjSocAvg = kU10EmployeeDTO.ErsEjSocAvg > 0 ? kU10EmployeeDTO.ErsEjSocAvg : 0;
                kU10EmployeeDTO.ErsEjSocAvgEjJobbavd = kU10EmployeeDTO.ErsEjSocAvgEjJobbavd > 0 ? kU10EmployeeDTO.ErsEjSocAvgEjJobbavd : 0;
                kU10EmployeeDTO.Forskarskattenamnden = kU10EmployeeDTO.Forskarskattenamnden > 0 ? kU10EmployeeDTO.Forskarskattenamnden : 0;
                kU10EmployeeDTO.VissaAvdrag = Math.Abs(kU10EmployeeDTO.VissaAvdrag);
                kU10EmployeeDTO.Hyresersattning = kU10EmployeeDTO.Hyresersattning > 0 ? kU10EmployeeDTO.Hyresersattning : 0;
                kU10EmployeeDTO.SocialAvgiftsAvtal = kU10EmployeeDTO.SocialAvgiftsAvtal > 0 ? kU10EmployeeDTO.SocialAvgiftsAvtal : 0;
                kU10EmployeeDTO.Specifikationsnummer = kU10EmployeeDTO.Specifikationsnummer > 0 ? kU10EmployeeDTO.Specifikationsnummer : 0;

                #endregion
            }

            if (!kU10EmployeeDTO.Transactions.IsNullOrEmpty())
                addInfo = true;
            if (addInfo)
                kU10EmployeeDTO.GiltigaUppgifter = true;

            return kU10EmployeeDTO;
        }

        public AgdEmployeeDTO CreateAgdEmployeeDTO(List<TimePayrollStatisticsSmallDTO> timePayrollTransactionItems, Employee employee, string timePeriod, EmployeeTaxSE employeeTaxSE, DateTime selectionDateFrom, DateTime selectionDateTo, bool removePrevSubmittedData, DateTime paymentDate, bool includeAbsence = false)
        {
            AgdEmployeeDTO agdEmployeeDTO = new AgdEmployeeDTO();
            agdEmployeeDTO.EmployeeId = employee.EmployeeId;
            agdEmployeeDTO.Transactions = new List<KU10EmployeeTransactionDTO>();
            agdEmployeeDTO.BetalningsmottagarId = CleanSocSec(employee.SocialSec);
            agdEmployeeDTO.Fodelseort = !string.IsNullOrEmpty(employeeTaxSE?.BirthPlace) ? employeeTaxSE.BirthPlace : string.Empty;
            agdEmployeeDTO.LandskodFodelseort = !string.IsNullOrEmpty(employeeTaxSE?.CountryCodeBirthPlace) ? employeeTaxSE.CountryCodeBirthPlace : string.Empty;
            agdEmployeeDTO.LandskodMedborgare = !string.IsNullOrEmpty(employeeTaxSE?.CountryCodeCitizen) ? employeeTaxSE.CountryCodeCitizen : string.Empty;

            if (employeeTaxSE != null)
            {
                agdEmployeeDTO.ForstaAnstalld = employeeTaxSE.FirstEmployee;
                agdEmployeeDTO.AndraAnstalld = employeeTaxSE.SecondEmployee;
            }

            if (removePrevSubmittedData)
            {
                agdEmployeeDTO.Borttag = true;
            }
            else
            {
                Employment firstEmployment = employee.Employment.GetFirstEmployment();
                Employment lastEmployment = employee.Employment.GetLastEmployment();
                DateTime EmploymentStartDate = firstEmployment != null && firstEmployment.DateFrom.HasValue ? firstEmployment.DateFrom.Value : DateTime.MinValue;
                DateTime EmploymentEndDate = lastEmployment != null && lastEmployment.DateTo.HasValue ? lastEmployment.DateTo.Value : DateTime.MaxValue;
                Dictionary<int, object> companySettingsDict = SettingManager.GetCompanySettingsDict((int)CompanySettingTypeGroup.Payroll, reportResult.ActorCompanyId);

                if (!employee.AGIPlaceOfEmploymentIgnore)
                {
                    if (!employee.AGIPlaceOfEmploymentAddress.IsNullOrEmpty() && !employee.AGIPlaceOfEmploymentCity.IsNullOrEmpty())
                    {
                        agdEmployeeDTO.PlaceOfEmploymentAddress = employee.AGIPlaceOfEmploymentAddress;
                        agdEmployeeDTO.PlaceOfEmploymentCity = employee.AGIPlaceOfEmploymentCity;
                    }
                    else
                    {
                        agdEmployeeDTO.PlaceOfEmploymentAddress = SettingManager.GetSettingFromDict(companySettingsDict, (int)CompanySettingType.PayrollExportPlaceOfEmploymentAddress, (int)SettingDataType.String);
                        agdEmployeeDTO.PlaceOfEmploymentCity = SettingManager.GetSettingFromDict(companySettingsDict, (int)CompanySettingType.PayrollExportPlaceOfEmploymentCity, (int)SettingDataType.String);
                    }
                }

                List<int> skipTransactionIds = timePayrollTransactionItems.Where(p => !p.IsScheduleTransaction).Select(t => t.TransactionId).ToList();
                if (timePayrollTransactionItems.IsNullOrEmpty())
                    return null;

                agdEmployeeDTO.RedovisningsPeriod = timePeriod;
                agdEmployeeDTO.Specifikationsnummer = DateTime.Now.Millisecond.ToString();
                bool hasHiddenEmploymentTaxAndHidden = timePayrollTransactionItems.Where(x => x.IsEmploymentTax()).All(a => a.IsEmploymentTaxAndHidden);
                
                foreach (var transaction in timePayrollTransactionItems)
                {
                    #region Transaction

                    //Arbetsstallenummer
                    if (!String.IsNullOrEmpty(employee.WorkPlaceSCB))
                        agdEmployeeDTO.Arbetsstallenummer = employee.WorkPlaceSCB;

                    //AvdrPrelSkatt/AvdrSkattSINK/AvdrSkattASINK
                    if (transaction.IsTax())
                    {

                        if (transaction.IsSINKTax())
                        {
                            agdEmployeeDTO.DecAvdrSkattSINK += transaction.Amount;
                            agdEmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "AvdrSkattSINK"));

                        }
                        else if (transaction.IsASINKTax())
                        {
                            agdEmployeeDTO.DecAvdrSkattASINK += transaction.Amount;
                            agdEmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "AvdrSkattASINK"));
                        }
                        else
                        {
                            agdEmployeeDTO.DecAvdrPrelSkatt += transaction.Amount;
                            agdEmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "AvdrPrelSkatt"));
                        }
                    }

                    //TODO: No support yet

                    //KontantErsattningUlagAG
                    if ((employeeTaxSE == null || employeeTaxSE.EmploymentTaxType != (int)TermGroup_EmployeeTaxEmploymentTaxType.PayrollTax) && transaction.IsGrossSalary())
                    {
                        if (hasHiddenEmploymentTaxAndHidden || (employeeTaxSE != null && employeeTaxSE.ApplyEmploymentTaxMinimumRule))
                        {
                            agdEmployeeDTO.DecKontantErsattningEjUlagSA += transaction.Amount;
                            agdEmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "KontantErsattningEjUlagSA"));
                        }
                        else
                        {
                            agdEmployeeDTO.DecKontantErsattningUlagAG += transaction.Amount;
                            agdEmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "KontantErsattningUlagAG"));

                            //AvrakningAvgiftsfriErs
                            if (transaction.IsBelowEmploymentTaxLimitRuleFromPreviousPeriods)
                            {
                                agdEmployeeDTO.DecAvrakningAvgiftsfriErs += transaction.Amount;
                                agdEmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "AvrakningAvgiftsfriEr"));
                            }

                            //Regionalt stöd
                            if (employeeTaxSE.RegionalSupport)
                            {
                                agdEmployeeDTO.DecRegionaltStodUlagAG += transaction.Amount;
                                agdEmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "RegionaltStodUlagAG"));
                            }

                        }
                    }
                    if(transaction.IsBenefit_Not_CompanyCar_And_FuelBenefit() && (employeeTaxSE == null || employeeTaxSE.EmploymentTaxType != (int)TermGroup_EmployeeTaxEmploymentTaxType.PayrollTax) && (hasHiddenEmploymentTaxAndHidden || (employeeTaxSE != null && employeeTaxSE.ApplyEmploymentTaxMinimumRule)))
                    {
                        agdEmployeeDTO.DecSkatteplOvrigaFormanerEjUlagSA += transaction.Amount;
                        agdEmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "SkatteplOvrigaFormanerEjUlagSA"));
                    }
                    else if (transaction.IsBenefit_Not_CompanyCar_And_FuelBenefit())
                    {
                        agdEmployeeDTO.DecSkatteplOvrigaFormanerUlagAG += transaction.Amount;
                        agdEmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "SkatteplOvrigaFormanerUlagAG"));
                    }

                    //Tjanstepension
                    if (transaction.IsOccupationalPension())
                    {
                        agdEmployeeDTO.DecTjanstepension += transaction.Amount;
                        agdEmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "Tjanstepension"));
                    }


                    //SkatteplBilformanUlagAG
                    if (transaction.IsBenefit_CompanyCar())
                    {
                        agdEmployeeDTO.DecSkatteplBilformanUlagAG += transaction.Amount;
                        agdEmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "SkatteplBilformanUlagAG"));
                    }

                    //DrivmVidBilformanUlagAG
                    if (transaction.IsBenefit_Fuel())
                    {
                        agdEmployeeDTO.DecDrivmVidBilformanUlagAG += transaction.Amount;
                        agdEmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "DrivmVidBilformanUlagAG"));
                    }

                    //AvdragUtgiftArbetet
                    //TODO: No support yet

                    //BostadsformanSmahusUlagAG
                    if (transaction.IsBenefit_PropertyHouse())
                    {
                        agdEmployeeDTO.BostadsformanSmahusUlagAG = true;
                        agdEmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "BostadsformanSmahusUlagAG"));
                    }

                    //BostadsformanEjSmahusUlagAG
                    if (transaction.IsBenefit_PropertyNotHouse())
                    {
                        agdEmployeeDTO.BostadsformanEjSmahusUlagAG = true;
                        agdEmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "BostadsformanEjSmahusUlagAG"));
                    }

                    //Bilersattning
                    if (transaction.IsCompensation_CarCompensation_PrivateCar())
                    {
                        agdEmployeeDTO.Bilersattning = true;
                    }

                    //Traktamente
                    if (transaction.IsCompensation_TravelAllowance_DomesticShortTerm() || transaction.IsCompensation_TravelAllowance_ForeignShortTerm())
                    {
                        agdEmployeeDTO.Traktamente = true;
                    }

                    //AndraKostnadsers
                    if (transaction.IsCompensation_Other_Taxable())
                    {
                        agdEmployeeDTO.DecAndraKostnadsers += transaction.Amount;
                        agdEmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "AndraKostnadsers"));
                    }

                    //UnderlagRutarbete
                    if (transaction.IsBenefit_RUT())
                    {
                        agdEmployeeDTO.DecUnderlagRutarbete += transaction.Amount;
                        agdEmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "UnderlagRutarbete"));
                    }

                    //UnderlagRotarbete
                    if (transaction.IsBenefit_ROT())
                    {
                        agdEmployeeDTO.DecUnderlagRotarbete += transaction.Amount;
                        agdEmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "UnderlagRotarbete"));
                    }

                    //Hyresersattning
                    if (transaction.IsCompensation_Rental())
                    {
                        agdEmployeeDTO.DecHyresersattning += transaction.Amount;
                        agdEmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "Hyresersattning"));
                    }

                    //Styrelsearvode
                    if (transaction.IsBoardRemuneration())
                    {
                        agdEmployeeDTO.DecAndelStyrelsearvode += transaction.Amount;
                        agdEmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "AndelStyrelsearvode"));
                    }
                    //FrånvaroUppgift
                    if (includeAbsence && transaction.Quantity > 0 && transaction.Date >= selectionDateFrom && transaction.Date <= selectionDateTo)
                    {
                        //Tillfällig föräldrapenning
                        if (transaction.IsAbsenceTemporaryParentalLeave())
                        {
                            agdEmployeeDTO.TemporaryParentalLeave = true;
                            agdEmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "TemporaryParentalLeave"));                          
                        }
                        // Föräldraledig
                        if (transaction.IsParentalLeave())
                        {
                            agdEmployeeDTO.ParentalLeave = true;
                            agdEmployeeDTO.Transactions.Add(ConvertToKU10EmployeeTransactionDTO(transaction, "ParentalLeave"));
                        }
                    }

                    #region For sums

                        if (transaction.IsEmploymentTaxCredit())
                    {
                        agdEmployeeDTO.DecArbetsgivarintygKredit += transaction.Amount;
                    }

                    if (transaction.IsAbsence_SicknessSalary_Day2_14() && !transaction.IsAbsence_SicknessSalary_Deduction())
                    {
                        agdEmployeeDTO.DecSjuklön += transaction.Amount;
                        agdEmployeeDTO.DecSjuklön += PayrollManager.CalculateEmploymentTaxSE(reportResult.ActorCompanyId, paymentDate, transaction.Amount, employee);
                    }

                    if (transaction.IsAbsence_SicknessSalary_Deduction())
                    {
                        agdEmployeeDTO.DecSjuklön += transaction.Amount;
                        agdEmployeeDTO.DecSjuklön += PayrollManager.CalculateEmploymentTaxSE(reportResult.ActorCompanyId, paymentDate, transaction.Amount, employee);
                    }
                   
                    if (transaction.IsTax())
                    {
                        agdEmployeeDTO.DecSkatt += transaction.Amount;
                    }
                  
                    #endregion

                    #endregion
                }
             
                #region To ints

                agdEmployeeDTO.ArbetsArbAvgSlf = NumberUtility.AmountToInt(agdEmployeeDTO.DecArbetsgivarintygKredit);
                agdEmployeeDTO.Tjanstepension = NumberUtility.AmountToInt(agdEmployeeDTO.DecTjanstepension, true);
                agdEmployeeDTO.Sjuklön = NumberUtility.AmountToInt(agdEmployeeDTO.DecSjuklön, true);
                if (paymentDate < new DateTime(2024, 07, 01))   // 499 - Sjuklön in export file should be 0 after 2024-07-01 
                    agdEmployeeDTO.SjuklönExport = agdEmployeeDTO.Sjuklön;

                agdEmployeeDTO.Skatt = NumberUtility.AmountToInt(agdEmployeeDTO.DecSkatt, true);
                agdEmployeeDTO.UnderlagRutarbete = NumberUtility.AmountToInt(agdEmployeeDTO.DecUnderlagRutarbete, true);
                agdEmployeeDTO.UnderlagRotarbete = NumberUtility.AmountToInt(agdEmployeeDTO.DecUnderlagRotarbete, true);
                agdEmployeeDTO.Hyresersattning = NumberUtility.AmountToInt(agdEmployeeDTO.DecHyresersattning, true);
                agdEmployeeDTO.AndraKostnadsers = NumberUtility.AmountToInt(agdEmployeeDTO.DecAndraKostnadsers, true);
                agdEmployeeDTO.KontantErsattningEjUlagSA = NumberUtility.AmountToInt(agdEmployeeDTO.DecKontantErsattningEjUlagSA, true);
                agdEmployeeDTO.SkatteplOvrigaFormanerEjUlagSA = NumberUtility.AmountToInt(agdEmployeeDTO.DecSkatteplOvrigaFormanerEjUlagSA, true);
                agdEmployeeDTO.SkatteplBilformanEjUlagSA = NumberUtility.AmountToInt(agdEmployeeDTO.DecSkatteplBilformanEjUlagSA, true);
                agdEmployeeDTO.DrivmVidBilformanEjUlagSA = NumberUtility.AmountToInt(agdEmployeeDTO.DecDrivmVidBilformanEjUlagSA, true);
                agdEmployeeDTO.FormanSomPensionEjUlagSA = NumberUtility.AmountToInt(agdEmployeeDTO.DecFormanSomPensionEjUlagSA, true);
                agdEmployeeDTO.BostadsformanSmahusEjUlagSA = NumberUtility.AmountToInt(agdEmployeeDTO.DecBostadsformanSmahusEjUlagSA, true);
                agdEmployeeDTO.BostadsformanEjSmahusEjUlagSA = NumberUtility.AmountToInt(agdEmployeeDTO.DecBostadsformanEjSmahusEjUlagSA, true);
                agdEmployeeDTO.ErsEjSocAvgEjJobbavd = NumberUtility.AmountToInt(agdEmployeeDTO.DecErsEjSocAvgEjJobbavd, true);
                agdEmployeeDTO.AvrakningAvgiftsfriErs = NumberUtility.AmountToInt(agdEmployeeDTO.DecAvrakningAvgiftsfriErs, true);
                agdEmployeeDTO.KontantErsattningUlagAG = NumberUtility.AmountToInt(agdEmployeeDTO.DecKontantErsattningUlagAG, true);
                agdEmployeeDTO.SkatteplOvrigaFormanerUlagAG = NumberUtility.AmountToInt(agdEmployeeDTO.DecSkatteplOvrigaFormanerUlagAG, true);
                agdEmployeeDTO.SkatteplBilformanUlagAG = NumberUtility.AmountToInt(agdEmployeeDTO.DecSkatteplBilformanUlagAG, true);
                agdEmployeeDTO.DrivmVidBilformanUlagAG = NumberUtility.AmountToInt(agdEmployeeDTO.DecDrivmVidBilformanUlagAG, true);
                agdEmployeeDTO.AvdragUtgiftArbetet = NumberUtility.AmountToInt(agdEmployeeDTO.DecAvdragUtgiftArbetet, true);
                agdEmployeeDTO.AvdrPrelSkatt = NumberUtility.AmountToInt(agdEmployeeDTO.DecAvdrPrelSkatt, true);
                agdEmployeeDTO.AvdrSkattSINK = NumberUtility.AmountToInt(agdEmployeeDTO.DecAvdrSkattSINK, true);
                agdEmployeeDTO.AvdrSkattASINK = NumberUtility.AmountToInt(agdEmployeeDTO.DecAvdrSkattASINK, true);
                agdEmployeeDTO.AndelStyrelsearvode = NumberUtility.AmountToInt(agdEmployeeDTO.DecAndelStyrelsearvode, true);
                agdEmployeeDTO.RegionaltStodUlagAG = NumberUtility.AmountToInt(agdEmployeeDTO.DecRegionaltStodUlagAG, true);

                #endregion

                #region Fix negatives

                agdEmployeeDTO.Tjanstepension = agdEmployeeDTO.Tjanstepension > 0 ? agdEmployeeDTO.Tjanstepension : agdEmployeeDTO.Tjanstepension * -1; //BELOPP8
                agdEmployeeDTO.AvdrPrelSkatt = agdEmployeeDTO.AvdrPrelSkatt > 0 ? agdEmployeeDTO.AvdrPrelSkatt : agdEmployeeDTO.AvdrPrelSkatt * -1; //BELOPP8
                agdEmployeeDTO.AvdrSkattSINK = agdEmployeeDTO.AvdrSkattSINK > 0 ? agdEmployeeDTO.AvdrSkattSINK : agdEmployeeDTO.AvdrSkattSINK * -1; //BELOPP8
                agdEmployeeDTO.AvdrSkattASINK = agdEmployeeDTO.AvdrSkattASINK > 0 ? agdEmployeeDTO.AvdrSkattASINK : agdEmployeeDTO.AvdrSkattASINK * -1; //BELOPP8
                agdEmployeeDTO.KontantErsattningUlagAG = agdEmployeeDTO.KontantErsattningUlagAG > 0 ? agdEmployeeDTO.KontantErsattningUlagAG : 0; //;BELOPP8
                agdEmployeeDTO.KontantErsattningEjUlagSA = agdEmployeeDTO.KontantErsattningEjUlagSA > 0 ? agdEmployeeDTO.KontantErsattningEjUlagSA : 0; //;BELOPP7                
                agdEmployeeDTO.SkatteplOvrigaFormanerUlagAG = agdEmployeeDTO.SkatteplOvrigaFormanerUlagAG > 0 ? agdEmployeeDTO.SkatteplOvrigaFormanerUlagAG : 0; //BELOPP7
                agdEmployeeDTO.SkatteplBilformanUlagAG = agdEmployeeDTO.SkatteplBilformanUlagAG > 0 ? agdEmployeeDTO.SkatteplBilformanUlagAG : 0; //;BELOPP6
                agdEmployeeDTO.DrivmVidBilformanUlagAG = agdEmployeeDTO.DrivmVidBilformanUlagAG > 0 ? agdEmployeeDTO.DrivmVidBilformanUlagAG : 0; //BELOPP6
                agdEmployeeDTO.AndraKostnadsers = agdEmployeeDTO.AndraKostnadsers > 0 ? agdEmployeeDTO.AndraKostnadsers : 0;//BELOPP7
                agdEmployeeDTO.UnderlagRutarbete = agdEmployeeDTO.UnderlagRutarbete > 0 ? agdEmployeeDTO.UnderlagRutarbete : 0;//BELOPP6
                agdEmployeeDTO.UnderlagRotarbete = agdEmployeeDTO.UnderlagRotarbete > 0 ? agdEmployeeDTO.UnderlagRotarbete : 0;//BELOPP6
                agdEmployeeDTO.Hyresersattning = agdEmployeeDTO.Hyresersattning > 0 ? agdEmployeeDTO.Hyresersattning : 0; //BELOPP7
                agdEmployeeDTO.AvrakningAvgiftsfriErs = agdEmployeeDTO.AvrakningAvgiftsfriErs > 0 ? agdEmployeeDTO.AvrakningAvgiftsfriErs : 0;

                agdEmployeeDTO.ArbetsArbAvgSlf = agdEmployeeDTO.ArbetsArbAvgSlf > 0 ? agdEmployeeDTO.ArbetsArbAvgSlf : agdEmployeeDTO.ArbetsArbAvgSlf * -1;
                agdEmployeeDTO.DecArbetsgivarintygKredit = agdEmployeeDTO.DecArbetsgivarintygKredit > 0 ? agdEmployeeDTO.DecArbetsgivarintygKredit : agdEmployeeDTO.DecArbetsgivarintygKredit * -1;
                agdEmployeeDTO.Sjuklön = agdEmployeeDTO.Sjuklön > 0 ? agdEmployeeDTO.Sjuklön : 0;
                agdEmployeeDTO.Skatt = agdEmployeeDTO.Skatt > 0 ? agdEmployeeDTO.Skatt : agdEmployeeDTO.Skatt * -1;

                #endregion

                decimal decDrivmVidBilformanUlagAGSoc = decimal.Divide(agdEmployeeDTO.DecDrivmVidBilformanUlagAG, 1.2M);
                int drivmVidBilformanUlagAGSoc = NumberUtility.AmountToInt(decDrivmVidBilformanUlagAGSoc, true);

                var ulag = (agdEmployeeDTO.DecSkatteplOvrigaFormanerUlagAG
                 + agdEmployeeDTO.DecSkatteplBilformanUlagAG
                 + decDrivmVidBilformanUlagAGSoc
                 + agdEmployeeDTO.DecKontantErsattningUlagAG);

                agdEmployeeDTO.EmploymentTaxRate = ulag != 0 ? Math.Round((agdEmployeeDTO.DecArbetsgivarintygKredit / ulag), 4) : 0;

                #region Calculate

                agdEmployeeDTO.DecCalculatedArbetsgivarintygKredit = (agdEmployeeDTO.SkatteplOvrigaFormanerUlagAG
                    + agdEmployeeDTO.SkatteplBilformanUlagAG
                    + drivmVidBilformanUlagAGSoc
                    + agdEmployeeDTO.KontantErsattningUlagAG) * agdEmployeeDTO.EmploymentTaxRate;

                #endregion

            }

            agdEmployeeDTO.TimePeriodName = agdEmployeeDTO.Transactions.IsNullOrEmpty() ? "" : StringUtility.GetCommaSeparatedString(agdEmployeeDTO.Transactions.Select(s => s.TimePeriodName).Distinct());

            return agdEmployeeDTO;
        }

        public int CalulateAvdragRegionaltStod(int avdragRegionaltStodProcent, int maxAvdragRegionaltStod, decimal summaKontantErsattningUlagAG)
        {
            if (avdragRegionaltStodProcent == 0)
                maxAvdragRegionaltStod = 0;
            else if (maxAvdragRegionaltStod > 0 && decimal.Multiply(summaKontantErsattningUlagAG, decimal.Divide(avdragRegionaltStodProcent, 100)) < maxAvdragRegionaltStod)
                maxAvdragRegionaltStod = Convert.ToInt32(decimal.Multiply(summaKontantErsattningUlagAG, decimal.Divide(avdragRegionaltStodProcent, 100)));

            return maxAvdragRegionaltStod;
        }

        #endregion

        #region Create Elements

        // Sender is always SoftOne (XE) 
        private XElement CreateElementAvsandare(XNamespace nameSpace, Company company)
        {
            return new XElement(nameSpace + "Avsandare",
                    new XElement(nameSpace + "Programnamn", "SoftOne"),
                    new XElement(nameSpace + "Organisationsnummer", CleanOrgNr(company.OrgNr))
                    );
        }

        private XElement CreateElementTekniskKontaktperson(XNamespace nameSpace, Company company, string contactPerson, List<ContactAddressItem> contactAddressItems)
        {
            string address = "";
            string addressCo = "";
            string postalCode = "";
            string postalAddress = "";
            string ecomEmailAddress = "";
            string ecomPhone = "";

            foreach (var item in contactAddressItems)
            {
                if (item.ContactAddressItemType == ContactAddressItemType.AddressDistribution)
                {
                    address = item.Address;
                    addressCo = item.AddressCO;
                    postalCode = item.PostalCode;
                    postalAddress = item.PostalAddress;
                }
                if (item.ContactAddressItemType == ContactAddressItemType.EcomCompanyAdminEmail)
                {
                    ecomEmailAddress = item.DisplayAddress;
                }
                if (item.ContactAddressItemType == ContactAddressItemType.EComPhoneJob)
                {
                    ecomPhone = item.DisplayAddress;
                }
            }

            XElement element = new XElement(nameSpace + "TekniskKontaktperson");
            if (!String.IsNullOrEmpty(company.Name))
                element.Add(new XElement(nameSpace + "Namn", contactPerson));
            if (!String.IsNullOrEmpty(ecomPhone))
                element.Add(new XElement(nameSpace + "Telefon", ecomPhone));
            if (!String.IsNullOrEmpty(ecomEmailAddress))
                element.Add(new XElement(nameSpace + "Epostadress", ecomEmailAddress));
            if (!String.IsNullOrEmpty(address))
                element.Add(new XElement(nameSpace + "Utdelningsadress1", address));
            if (!String.IsNullOrEmpty(addressCo))
                element.Add(new XElement(nameSpace + "Utdelningsadress2", addressCo));
            if (!String.IsNullOrEmpty(postalCode))
                element.Add(new XElement(nameSpace + "Postnummer", postalCode));
            if (!String.IsNullOrEmpty(postalAddress))
                element.Add(new XElement(nameSpace + "Postort", postalAddress));

            return element;
        }

        private XElement CreateElementSkapad(XNamespace nameSpace)
        {
            return new XElement(nameSpace + "Skapad", FetchTime(DateTime.Now));
        }

        private XElement CreateElementBlankettgemensamt(XNamespace nameSpace)
        {
            return new XElement(nameSpace + "Blankettgemensamt");
        }

        private XElement CreateElementArbetsgivare(XNamespace nameSpace, Company company)
        {
            XElement element = new XElement(nameSpace + "Arbetsgivare");
            element.Add(new XElement(nameSpace + "AgRegistreradId", CleanOrgNr(company.OrgNr)));
            return element;
        }
        //FrånvaroUpggift
        private XElement CreateElementArbetsgivareFU(string orgNr, string period, AgdEmployeeDTO agdEmployeeDTO, DateTime date, string type, decimal sum, int franvaroSpecifikationsnummer)
        {
            var franvaroTyp = "";
            var kodTyp = "";
            var kodFaltKod = "";
         
            XElement elementArbetsgivareFUGROUP = null;

            switch (type) {
                case "TemporaryParentalLeave":
                    franvaroTyp = "TILLFALLIG_FORALDRAPENNING";
                    kodTyp = "TFP";
                    kodFaltKod = "825";
                    break;

                case "ParentalLeave":
                    franvaroTyp = "FORALDRAPENNING";
                    kodTyp = "FP";
                    kodFaltKod = "827";
                    break;
            }
            if (franvaroTyp != "" && kodTyp != "" && kodFaltKod != "")
            {
                elementArbetsgivareFUGROUP = new XElement(namespaceAGD + "Franvarouppgift");
                elementArbetsgivareFUGROUP.Add(new XElement(namespaceAGD + "AgRegistreradId", new XAttribute("faltkod", "201"), orgNr));
                elementArbetsgivareFUGROUP.Add(new XElement(namespaceAGD + "RedovisningsPeriod", new XAttribute("faltkod", "006"), period));
                elementArbetsgivareFUGROUP.Add(new XElement(namespaceAGD + "FranvaroDatum", new XAttribute("faltkod", "821"), date.ToShortDateString()));
                elementArbetsgivareFUGROUP.Add(new XElement(namespaceAGD + "BetalningsmottagarId", new XAttribute("faltkod", "215"), agdEmployeeDTO.BetalningsmottagarId));
                elementArbetsgivareFUGROUP.Add(new XElement(namespaceAGD + "FranvaroSpecifikationsnummer", new XAttribute("faltkod", "822"), franvaroSpecifikationsnummer));
                elementArbetsgivareFUGROUP.Add(new XElement(namespaceAGD + "FranvaroTyp", new XAttribute("faltkod", "823"), franvaroTyp));
                elementArbetsgivareFUGROUP.Add(new XElement(namespaceAGD + "FranvaroTimmar" + kodTyp, new XAttribute("faltkod", kodFaltKod), sum.ToString().Replace(",", ".")));
            }
           
            return elementArbetsgivareFUGROUP;
        }
        private XElement CreateElementArbetsgivareHUGROUP(string orgNr, string period, int avdragRegionaltStod, List<AgdEmployeeDTO> agdEmployeeDTOs)
        {
            agdEmployeeDTOs = agdEmployeeDTOs.Where(w => w != null).ToList();
            //Summerade
            var sum = 0;
            foreach (var group in agdEmployeeDTOs.Where(w => w.KontantErsattningEjUlagSA == 0 || w.SkatteplOvrigaFormanerEjUlagSA == 0).GroupBy(g => g.EmploymentTaxRate))
            {
               var sumGroup = group.Sum(s => s.DecCalculatedArbetsgivarintygKredit);
               sum += NumberUtility.AmountToInt(sumGroup, true);
            }

            //Blankettinnehall
            XElement elementBlankettinnehall = new XElement(this.namespaceAGD + "Blankettinnehall", String.Empty);

            //Blankett
            XElement elementBlankett = new XElement(this.namespaceAGD + "Blankett", String.Empty);
            elementBlankett.Add(new XElement(this.namespaceAGD + "Arendeinformation",
               new XElement(this.namespaceAGD + "Arendeagare", orgNr),
               new XElement(this.namespaceAGD + "Period", period)));


            XElement elementArbetsgivareHUGROUP = new XElement(namespaceAGD + "ArbetsgivareHUGROUP");
            elementArbetsgivareHUGROUP.Add(new XElement(namespaceAGD + "AgRegistreradId", new XAttribute("faltkod", "201"), orgNr));

            XElement elementHU = new XElement(namespaceAGD + "HU", String.Empty);
            elementHU.Add(elementArbetsgivareHUGROUP);
            elementHU.Add(new XElement(namespaceAGD + "RedovisningsPeriod", new XAttribute("faltkod", "006"), period));
            if (avdragRegionaltStod != 0)
            {
                elementHU.Add(new XElement(namespaceAGD + "AvdragRegionaltStod", new XAttribute("faltkod", "476"), avdragRegionaltStod));
                elementHU.Add(new XElement(namespaceAGD + "UlagRegionaltStod", new XAttribute("faltkod", "471"), agdEmployeeDTOs.Sum(s=> s.RegionaltStodUlagAG)));

                if(avdragRegionaltStod <= sum)
                    sum -= avdragRegionaltStod;
            }
            elementHU.Add(new XElement(namespaceAGD + "SummaArbAvgSlf", new XAttribute("faltkod", "487"), sum.ToString()));
            elementHU.Add(new XElement(namespaceAGD + "SummaSkatteavdr", new XAttribute("faltkod", "497"), agdEmployeeDTOs.Sum(i => i.AvdrPrelSkatt + i.AvdrSkattSINK + i.AvdrSkattASINK).ToString()));

            if (Convert.ToInt32(period) < 202407)
                elementHU.Add(new XElement(namespaceAGD + "TotalSjuklonekostnad", new XAttribute("faltkod", "499"), agdEmployeeDTOs.Sum(i => i.SjuklönExport).ToString()));

            elementBlankettinnehall.Add(elementHU);
            elementBlankett.Add(elementBlankettinnehall);

            return elementBlankett;
        }

        private XElement CreateElementKontaktperson(XNamespace nameSpace, string contactPerson, List<ContactAddressItem> contactAddressItems)
        {
            string ecomEmailAddress = "";
            string ecomPhone = "";

            foreach (var item in contactAddressItems)
            {
                if (item.ContactAddressItemType == ContactAddressItemType.EcomCompanyAdminEmail)
                    ecomEmailAddress = item.DisplayAddress;
                if (item.ContactAddressItemType == ContactAddressItemType.EComPhoneJob)
                    ecomPhone = item.DisplayAddress;
            }

            return new XElement(nameSpace + "Kontaktperson",
                   new XElement(nameSpace + "Namn", contactPerson),
                   new XElement(nameSpace + "Telefon", ecomPhone),
                   new XElement(nameSpace + "Epostadress", ecomEmailAddress));
        }

        private XElement CreateElementArendeinformation(XNamespace nameSpace, string orgNr, string period)
        {
            return new XElement(nameSpace + "Arendeinformation",
               new XElement(nameSpace + "Arendeagare", orgNr),
               new XElement(nameSpace + "Period", period));
        }

        private XElement CreateElementArbetsgivareIUGROUP(XNamespace nameSpace, string orgNr)
        {
            XElement element = new XElement(nameSpace + "ArbetsgivareIUGROUP");
            element.Add(new XElement(nameSpace + "AgRegistreradId", new XAttribute("faltkod", "201"), orgNr));
            return element;
        }

        private XElement CreateElementBetalningsmottagareIUGROUP(XNamespace nameSpace, string betalningsmottagarId, Employee employee, EmployeeTaxSE employeeTaxSE)
        {
            XElement element = new XElement(nameSpace + "BetalningsmottagareIUGROUP");
            if (!IsSocialSecWithX(employee.SocialSec))
                element.Add(CreateElementBetalningsmottagareIDChoice(nameSpace, betalningsmottagarId));
            else
            {
                XElement elementid = new XElement(nameSpace + "BetalningsmottagareIDChoice");
                elementid.Add(new XElement(nameSpace + "Fodelsetid", new XAttribute("faltkod", "222"), betalningsmottagarId));
                element.Add(elementid);

                string gatuadress = string.Empty;
                string postnummer = string.Empty;
                string postort = string.Empty;
                string landskodTIN = employeeTaxSE != null && !string.IsNullOrEmpty(employeeTaxSE.CountryCode) ? employeeTaxSE.CountryCode : string.Empty;
                string tin = employeeTaxSE != null && !string.IsNullOrEmpty(employeeTaxSE.TinNumber) ? employeeTaxSE.TinNumber : string.Empty;
                string landskodFodelseort = employeeTaxSE != null && !string.IsNullOrEmpty(employeeTaxSE.CountryCodeBirthPlace) ? employeeTaxSE.CountryCodeBirthPlace : string.Empty;
                string fodelseort = employeeTaxSE != null && !string.IsNullOrEmpty(employeeTaxSE.BirthPlace) ? employeeTaxSE.BirthPlace : string.Empty;

                int contactId = ContactManager.GetContactIdFromActorId(employee.ContactPersonId);
                List<ContactAddress> employeeAddresses = ContactManager.GetContactAddresses(contactId);
                ContactAddress employeeAddress = employeeAddresses.FirstOrDefault();
                if (employeeAddress != null && !employeeAddress.ContactAddressRow.IsNullOrEmpty())
                {
                    gatuadress = employeeAddress.ContactAddressRow.FirstOrDefault(t => t.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.Address)?.Text;
                    postnummer = employeeAddress.ContactAddressRow.FirstOrDefault(t => t.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalCode)?.Text;
                    postort = employeeAddress.ContactAddressRow.FirstOrDefault(t => t.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalAddress)?.Text;
                }

                if (!String.IsNullOrEmpty(landskodTIN))
                {
                    element.Add(new XElement(nameSpace + "LandskodTIN", new XAttribute("faltkod", "076"), landskodTIN));
                    
                    if (landskodTIN != "")
                    {
                        element.Add(new XElement(nameSpace + "Fodelseort", new XAttribute("faltkod", "077"), fodelseort));
                        element.Add(new XElement(nameSpace + "LandskodFodelseort", new XAttribute("faltkod", "078"), landskodFodelseort));
                    }

                    element.Add(new XElement(nameSpace + "LandskodMedborgare", new XAttribute("faltkod", "081"), landskodTIN)); //We are assuming this is the same as LandskodTIN


                }

                element.Add(new XElement(nameSpace + "Fornamn", new XAttribute("faltkod", "216"), employee.FirstName));
                element.Add(new XElement(nameSpace + "Efternamn", new XAttribute("faltkod", "217"), employee.LastName));

                if (!String.IsNullOrEmpty(gatuadress))
                    element.Add(new XElement(nameSpace + "Gatuadress", new XAttribute("faltkod", "218"), gatuadress));
                if (!String.IsNullOrEmpty(postnummer))
                    element.Add(new XElement(nameSpace + "Postnummer", new XAttribute("faltkod", "219"), postnummer));
                if (!String.IsNullOrEmpty(postort))
                    element.Add(new XElement(nameSpace + "Postort", new XAttribute("faltkod", "220"), postort));
                if (!String.IsNullOrEmpty(tin))
                    element.Add(new XElement(nameSpace + "TIN", new XAttribute("faltkod", "252"), tin));
            }
            return element;
        }

        private XElement CreateElementBetalningsmottagareIDChoice(XNamespace nameSpace, string betalningsmottagarId)
        {
            XElement element = new XElement(nameSpace + "BetalningsmottagareIDChoice");
            element.Add(new XElement(nameSpace + "BetalningsmottagarId", new XAttribute("faltkod", "215"), betalningsmottagarId));
            return element;
        }

        private XElement CreateElementKU10Employee(CreateReportResult reportResult, List<TimePayrollStatisticsSmallDTO> timePayrollTransactionItems, Employee employee, string orgNr, bool removePrevSubmittedData, DateTime selectionDateFrom, DateTime selectionDateTo)
        {
            #region Init

            #endregion

            #region Prereq

            bool showSocialSec = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId);
            EmployeeTaxSE employeeTaxSE = EmployeeManager.GetEmployeeTaxSE(employee.EmployeeId, selectionDateFrom.Year);

            KU10EmployeeDTO kU10EmployeeDTO = CreateKU10EmployeeDTO(timePayrollTransactionItems, employee, selectionDateFrom.Year.ToString(), employeeTaxSE, removePrevSubmittedData, selectionDateFrom, selectionDateTo);
            if (kU10EmployeeDTO == null || !kU10EmployeeDTO.GiltigaUppgifter || String.IsNullOrEmpty(kU10EmployeeDTO.Inkomsttagare))
                return null;

            List<XElement> employeeInformationElements = CreateEmployeeInformationElements(kU10EmployeeDTO, employee);
            if (employeeInformationElements == null)
                return null;

            #endregion

            #region XML

            //KU10
            XElement elementKU = new XElement(namespaceKU + "KU10", String.Empty);
            elementKU.Add(employeeInformationElements);
            elementKU.Add(CreateElementInkomsttagareKU10(employee, employeeTaxSE, showSocialSec));
            XElement elementUppgiftslamnareKU10 = new XElement(namespaceKU + "UppgiftslamnareKU10");
            elementUppgiftslamnareKU10.Add(new XElement(namespaceKU + "UppgiftslamnarId", new XAttribute("faltkod", "201"), orgNr));
            elementKU.Add(elementUppgiftslamnareKU10);

            //Blankettinnehall
            XElement elementBlankettinnehall = new XElement(namespaceKU + "Blankettinnehall", String.Empty);
            elementBlankettinnehall.Add(elementKU);

            //Blankett
            XElement elementBlankett = new XElement(namespaceKU + "Blankett", new XAttribute("nummer", "2300"), String.Empty);// Static 2300 = KU10 
            elementBlankett.Add(CreateElementArendeinformation(this.namespaceKU, orgNr, selectionDateFrom.Year.ToString()));
            elementBlankett.Add(elementBlankettinnehall);

            #endregion

            return elementBlankett;
        }

        private XElement CreateElementAgdEmployee(List<TimePayrollStatisticsSmallDTO> timePayrollTransactionItems, Employee employee, string orgNr, string specifikationsnummer, string period, bool removePrevSubmittedData, DateTime selectionDateFrom, DateTime selectionDateTo, DateTime paymentDate, out AgdEmployeeDTO agdEmployeeDTO, bool includeAbsence)
        {
            #region Init

            #endregion

            #region Prereq

            EmployeeTaxSE employeeTaxSE = EmployeeManager.GetEmployeeTaxSE(employee.EmployeeId, paymentDate.Year);
            var sinkNoTax = employeeTaxSE?.Type == (int)TermGroup_EmployeeTaxType.Sink && employeeTaxSE?.SinkType == (int)TermGroup_EmployeeTaxSinkType.NoTax;

            agdEmployeeDTO = CreateAgdEmployeeDTO(timePayrollTransactionItems, employee, period, employeeTaxSE, selectionDateFrom, selectionDateTo, removePrevSubmittedData, paymentDate, includeAbsence);
            if (agdEmployeeDTO == null || String.IsNullOrEmpty(agdEmployeeDTO.BetalningsmottagarId) || (!removePrevSubmittedData && agdEmployeeDTO.Transactions.IsNullOrEmpty() && agdEmployeeDTO.Transactions.Sum(s => Math.Abs(s.Amount)) == 0))
                return null;

            var grossAmount = timePayrollTransactionItems.Where(w => w.IsGrossSalary()).Sum(s => s.Amount);
            var benefitAmount = timePayrollTransactionItems.Where(w => w.IsBenefit_Not_CompanyCar_And_FuelBenefit()).Sum(s => s.Amount);
            var carBenefitAmount = timePayrollTransactionItems.Where(w => w.IsBenefit_CompanyCar()).Sum(s => s.Amount);
            var taxAmount = timePayrollTransactionItems.Where(w => w.IsTax()).Sum(s => s.Amount);

            if (Math.Abs(grossAmount) < 1 && Math.Abs(taxAmount) < 1 && Math.Abs(benefitAmount) < 1 && Math.Abs(carBenefitAmount) < 1)
                return null;

            List<XElement> employeeElements = CreateEmployeeInformationElements(agdEmployeeDTO, sinkNoTax);
            if (employeeElements == null)
                return null;

            #endregion

            #region Elements

            //IU (Uppgift)
            XElement elementUI = new XElement(this.namespaceAGD + "IU", String.Empty);
            elementUI.Add(CreateElementArbetsgivareIUGROUP(this.namespaceAGD, orgNr));
            elementUI.Add(CreateElementBetalningsmottagareIUGROUP(this.namespaceAGD, agdEmployeeDTO.BetalningsmottagarId, employee, employeeTaxSE));
            elementUI.Add(new XElement(this.namespaceAGD + "RedovisningsPeriod", new XAttribute("faltkod", "006"), period));
            if (agdEmployeeDTO.ForstaAnstalld)
                elementUI.Add(new XElement(this.namespaceAGD + "ForstaAnstalld", new XAttribute("faltkod", "062"), IntToString(agdEmployeeDTO.ForstaAnstalld.ToInt())));
            if (agdEmployeeDTO.AndraAnstalld)
                elementUI.Add(new XElement(this.namespaceAGD + "VaxaStod", new XAttribute("faltkod", "063"), IntToString(agdEmployeeDTO.AndraAnstalld.ToInt())));
            elementUI.Add(new XElement(this.namespaceAGD + "Specifikationsnummer", new XAttribute("faltkod", "570"), specifikationsnummer));
            if (!agdEmployeeDTO.PlaceOfEmploymentAddress.IsNullOrEmpty())
                elementUI.Add(new XElement(this.namespaceAGD + "ArbetsplatsensGatuadress", new XAttribute("faltkod", "245"), agdEmployeeDTO.PlaceOfEmploymentAddress));
            if (!agdEmployeeDTO.PlaceOfEmploymentAddress.IsNullOrEmpty())
                elementUI.Add(new XElement(this.namespaceAGD + "ArbetsplatsensOrt", new XAttribute("faltkod", "246"), agdEmployeeDTO.PlaceOfEmploymentCity));

            elementUI.Add(employeeElements);

            //Blankettinnehall
            XElement elementBlankettinnehall = new XElement(this.namespaceAGD + "Blankettinnehall", String.Empty);
            elementBlankettinnehall.Add(elementUI);

            //Blankett
            XElement elementBlankett = new XElement(this.namespaceAGD + "Blankett", String.Empty);
            elementBlankett.Add(CreateElementArendeinformation(this.namespaceAGD, orgNr, period));
            elementBlankett.Add(elementBlankettinnehall);

            #endregion

            return elementBlankett;
        }

        private XElement CreateElementInkomsttagareKU10(Employee employee, EmployeeTaxSE employeeTaxSE, bool showSocialSec)
        {
            XElement elementInkomsttagareKU10 = new XElement(namespaceKU + "InkomsttagareKU10");
            elementInkomsttagareKU10.Add(new XElement(namespaceKU + "Inkomsttagare", new XAttribute("faltkod", "215"), showSocialSec ? CleanSocSec(employee.SocialSec) : "No Permission"));

            if (IsSamordningsnummer(showSocialSec ? CleanSocSec(employee.SocialSec) : "") || IsSocialSecWithX(employee.SocialSec))
            {
                elementInkomsttagareKU10.Add(new XElement(namespaceKU + "Fornamn", new XAttribute("faltkod", "216"), employee.FirstName));
                elementInkomsttagareKU10.Add(new XElement(namespaceKU + "Efternamn", new XAttribute("faltkod", "217"), employee.LastName));

                string gatuadress = string.Empty;
                string postnummer = string.Empty;
                string postort = string.Empty;
                string landskodTIN = employeeTaxSE != null && !string.IsNullOrEmpty(employeeTaxSE.CountryCode) ? employeeTaxSE.CountryCode : string.Empty;
                string tin = employeeTaxSE != null && !string.IsNullOrEmpty(employeeTaxSE.TinNumber) ? employeeTaxSE.TinNumber : string.Empty;
                string countryCodeBirthPlace = employeeTaxSE != null && !string.IsNullOrEmpty(employeeTaxSE.CountryCodeBirthPlace) ? employeeTaxSE.CountryCodeBirthPlace : string.Empty;
                string birthPlace = employeeTaxSE != null && !string.IsNullOrEmpty(employeeTaxSE.BirthPlace) ? employeeTaxSE.BirthPlace : string.Empty;
                string countryCodeCitizen = employeeTaxSE != null && !string.IsNullOrEmpty(employeeTaxSE.CountryCodeCitizen) ? employeeTaxSE.CountryCodeCitizen : string.Empty;

                int contactId = ContactManager.GetContactIdFromActorId(employee.ContactPersonId);
                List<ContactAddress> employeeAddresses = ContactManager.GetContactAddresses(contactId);
                ContactAddress employeeAddress = employeeAddresses.FirstOrDefault();
                if (employeeAddress != null && !employeeAddress.ContactAddressRow.IsNullOrEmpty())
                {
                    gatuadress = employeeAddress.ContactAddressRow.FirstOrDefault(t => t.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.Address)?.Text;
                    postnummer = employeeAddress.ContactAddressRow.FirstOrDefault(t => t.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalCode)?.Text;
                    postort = employeeAddress.ContactAddressRow.FirstOrDefault(t => t.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalAddress)?.Text;
                }

                if (!String.IsNullOrEmpty(gatuadress))
                    elementInkomsttagareKU10.Add(new XElement(namespaceKU + "Gatuadress", new XAttribute("faltkod", "218"), gatuadress));
                if (!String.IsNullOrEmpty(postnummer))
                    elementInkomsttagareKU10.Add(new XElement(namespaceKU + "Postnummer", new XAttribute("faltkod", "219"), postnummer));
                if (!String.IsNullOrEmpty(postort))
                    elementInkomsttagareKU10.Add(new XElement(namespaceKU + "Postort", new XAttribute("faltkod", "220"), postort));
                if (!String.IsNullOrEmpty(tin))
                    elementInkomsttagareKU10.Add(new XElement(namespaceKU + "TIN", new XAttribute("faltkod", "252"), tin));
                if (!String.IsNullOrEmpty(landskodTIN))
                    elementInkomsttagareKU10.Add(new XElement(namespaceKU + "LandskodTIN", new XAttribute("faltkod", "076"), landskodTIN));
                if (!String.IsNullOrEmpty(birthPlace))
                    elementInkomsttagareKU10.Add(new XElement(namespaceKU + "Fodelseort", new XAttribute("faltkod", "077"), birthPlace));
                if (!String.IsNullOrEmpty(countryCodeBirthPlace))
                    elementInkomsttagareKU10.Add(new XElement(namespaceKU + "LandskodFodelseort", new XAttribute("faltkod", "078"), countryCodeBirthPlace));
                if (!String.IsNullOrEmpty(countryCodeCitizen))
                    elementInkomsttagareKU10.Add(new XElement(namespaceKU + "LandskodMedborgare", new XAttribute("faltkod", "081"), countryCodeCitizen));
            }
            return elementInkomsttagareKU10;
        }

        private List<XElement> CreateEmployeeInformationElements(KU10EmployeeDTO kU10EmployeeDTO, Employee employee)
        {
            List<XElement> elements = new List<XElement>();

            if (kU10EmployeeDTO.Borttag)
            {
                elements.Add(new XElement(namespaceKU + "Borttag", new XAttribute("faltkod", "205"), kU10EmployeeDTO.Borttag.ToInt()));
            }
            else
            {

                //Skatt
                elements.Add(new XElement(namespaceKU + "AvdragenSkatt", new XAttribute("faltkod", "001"), decimal.Truncate(kU10EmployeeDTO.AvdragenSkatt).ToString()));
                if (!String.IsNullOrEmpty(kU10EmployeeDTO.AnstalldFrom))
                    elements.Add(new XElement(namespaceKU + "AnstalldFrom", new XAttribute("faltkod", "008"), kU10EmployeeDTO.AnstalldFrom));
                if (!String.IsNullOrEmpty(kU10EmployeeDTO.AnstalldTom))
                    elements.Add(new XElement(namespaceKU + "AnstalldTom", new XAttribute("faltkod", "009"), kU10EmployeeDTO.AnstalldTom));
                if (kU10EmployeeDTO.KontantBruttolonMm != 0)
                    elements.Add(new XElement(namespaceKU + "KontantBruttolonMm", new XAttribute("faltkod", "011"), decimal.Truncate(kU10EmployeeDTO.KontantBruttolonMm).ToString()));
                if (kU10EmployeeDTO.FormanUtomBilDrivmedel != 0)
                    elements.Add(new XElement(namespaceKU + "FormanUtomBilDrivmedel", new XAttribute("faltkod", "012"), decimal.Truncate(kU10EmployeeDTO.FormanUtomBilDrivmedel).ToString()));
                if (kU10EmployeeDTO.BilformanUtomDrivmedel != 0)
                    elements.Add(new XElement(namespaceKU + "BilformanUtomDrivmedel", new XAttribute("faltkod", "013"), decimal.Truncate(kU10EmployeeDTO.BilformanUtomDrivmedel).ToString()));
                if (!String.IsNullOrEmpty(kU10EmployeeDTO.KodForFormansbil) || kU10EmployeeDTO.BilformanUtomDrivmedel != 0)
                    elements.Add(new XElement(namespaceKU + "KodForFormansbil", new XAttribute("faltkod", "014"), kU10EmployeeDTO.KodForFormansbil));
                if (kU10EmployeeDTO.AntalManBilforman != 0 || !string.IsNullOrEmpty(kU10EmployeeDTO.KodForFormansbil))
                    elements.Add(new XElement(namespaceKU + "AntalManBilforman", new XAttribute("faltkod", "015"), IntToString(kU10EmployeeDTO.AntalManBilforman)));
                if (kU10EmployeeDTO.KmBilersVidBilforman != 0)
                    elements.Add(new XElement(namespaceKU + "KmBilersVidBilforman", new XAttribute("faltkod", "016"), DecToString(kU10EmployeeDTO.KmBilersVidBilforman)));
                if (kU10EmployeeDTO.BetaltForBilforman != 0)
                    elements.Add(new XElement(namespaceKU + "BetaltForBilforman", new XAttribute("faltkod", "017"), decimal.Truncate(kU10EmployeeDTO.BetaltForBilforman).ToString()));
                if (kU10EmployeeDTO.DrivmedelVidBilforman != 0)
                    elements.Add(new XElement(namespaceKU + "DrivmedelVidBilforman", new XAttribute("faltkod", "018"), decimal.Truncate(kU10EmployeeDTO.DrivmedelVidBilforman).ToString()));

                //Andra belopp
                if (kU10EmployeeDTO.AndraKostnadsers != 0)
                    elements.Add(new XElement(namespaceKU + "AndraKostnadsers", new XAttribute("faltkod", "020"), decimal.Truncate(kU10EmployeeDTO.AndraKostnadsers).ToString()));
                if (kU10EmployeeDTO.UnderlagRutarbete != 0)
                    elements.Add(new XElement(namespaceKU + "UnderlagRutarbete", new XAttribute("faltkod", "021"), decimal.Truncate(kU10EmployeeDTO.UnderlagRutarbete).ToString()));
                if (kU10EmployeeDTO.UnderlagRotarbete != 0)
                    elements.Add(new XElement(namespaceKU + "UnderlagRotarbete", new XAttribute("faltkod", "022"), decimal.Truncate(kU10EmployeeDTO.UnderlagRotarbete).ToString()));
                if (kU10EmployeeDTO.ErsMEgenavgifter != 0)
                    elements.Add(new XElement(namespaceKU + "ErsMEgenavgifter", new XAttribute("faltkod", "025"), decimal.Truncate(kU10EmployeeDTO.ErsMEgenavgifter).ToString()));
                if (kU10EmployeeDTO.Tjanstepension != 0)
                    elements.Add(new XElement(namespaceKU + "Tjanstepension", new XAttribute("faltkod", "030"), decimal.Truncate(kU10EmployeeDTO.Tjanstepension).ToString()));
                if (kU10EmployeeDTO.ErsEjSocAvg != 0)
                    elements.Add(new XElement(namespaceKU + "ErsEjSocAvg", new XAttribute("faltkod", "031"), decimal.Truncate(kU10EmployeeDTO.ErsEjSocAvg).ToString()));
                if (kU10EmployeeDTO.ErsEjSocAvgEjJobbavd != 0)
                    elements.Add(new XElement(namespaceKU + "ErsEjSocAvgEjJobbavd", new XAttribute("faltkod", "032"), decimal.Truncate(kU10EmployeeDTO.ErsEjSocAvgEjJobbavd).ToString()));
                if (kU10EmployeeDTO.Forskarskattenamnden != 0)
                    elements.Add(new XElement(namespaceKU + "Forskarskattenamnden", new XAttribute("faltkod", "035"), decimal.Truncate(kU10EmployeeDTO.Forskarskattenamnden).ToString()));
                if (kU10EmployeeDTO.VissaAvdrag != 0)
                    elements.Add(new XElement(namespaceKU + "VissaAvdrag", new XAttribute("faltkod", "037"), decimal.Truncate(kU10EmployeeDTO.VissaAvdrag).ToString()));
                if (kU10EmployeeDTO.Hyresersattning != 0)
                    elements.Add(new XElement(namespaceKU + "Hyresersattning", new XAttribute("faltkod", "039"), decimal.Truncate(kU10EmployeeDTO.Hyresersattning).ToString()));

                //Kryss
                if (kU10EmployeeDTO.BostadSmahus.HasValue && kU10EmployeeDTO.BostadSmahus.Value)
                    elements.Add(new XElement(namespaceKU + "BostadSmahus", new XAttribute("faltkod", "041"), BoolToString(kU10EmployeeDTO.BostadSmahus)));
                if (kU10EmployeeDTO.Kost.HasValue && kU10EmployeeDTO.Kost.Value)
                    elements.Add(new XElement(namespaceKU + "Kost", new XAttribute("faltkod", "042"), BoolToString(kU10EmployeeDTO.Kost)));
                if (kU10EmployeeDTO.BostadEjSmahus.HasValue && kU10EmployeeDTO.BostadEjSmahus.Value)
                    elements.Add(new XElement(namespaceKU + "BostadEjSmahus", new XAttribute("faltkod", "043"), BoolToString(kU10EmployeeDTO.BostadEjSmahus)));
                if (kU10EmployeeDTO.Ranta.HasValue && kU10EmployeeDTO.Ranta.Value)
                    elements.Add(new XElement(namespaceKU + "Ranta", new XAttribute("faltkod", "044"), BoolToString(kU10EmployeeDTO.Ranta)));
                if (kU10EmployeeDTO.Parkering.HasValue && kU10EmployeeDTO.Parkering.Value)
                    elements.Add(new XElement(namespaceKU + "Parkering", new XAttribute("faltkod", "045"), BoolToString(kU10EmployeeDTO.Parkering)));
                if (kU10EmployeeDTO.AnnanForman.HasValue && kU10EmployeeDTO.AnnanForman.Value)
                    elements.Add(new XElement(namespaceKU + "AnnanForman", new XAttribute("faltkod", "047"), BoolToString(kU10EmployeeDTO.AnnanForman)));
                if (kU10EmployeeDTO.FormanHarJusterats.HasValue && kU10EmployeeDTO.FormanHarJusterats.Value)
                    elements.Add(new XElement(namespaceKU + "FormanHarJusterats", new XAttribute("faltkod", "048"), BoolToString(kU10EmployeeDTO.FormanHarJusterats)));
                if (kU10EmployeeDTO.FormanSomPension.HasValue && kU10EmployeeDTO.FormanSomPension.Value)
                    elements.Add(new XElement(namespaceKU + "FormanSomPension", new XAttribute("faltkod", "049"), BoolToString(kU10EmployeeDTO.FormanSomPension)));
                if (kU10EmployeeDTO.Bilersattning.HasValue && kU10EmployeeDTO.Bilersattning.Value)
                    elements.Add(new XElement(namespaceKU + "Bilersattning", new XAttribute("faltkod", "050"), BoolToString(kU10EmployeeDTO.Bilersattning)));
                if (kU10EmployeeDTO.TraktamenteInomRiket.HasValue && kU10EmployeeDTO.TraktamenteInomRiket.Value)
                    elements.Add(new XElement(namespaceKU + "TraktamenteInomRiket", new XAttribute("faltkod", "051"), BoolToString(kU10EmployeeDTO.TraktamenteInomRiket)));
                if (kU10EmployeeDTO.TraktamenteUtomRiket.HasValue && kU10EmployeeDTO.TraktamenteUtomRiket.Value)
                    elements.Add(new XElement(namespaceKU + "TraktamenteUtomRiket", new XAttribute("faltkod", "052"), BoolToString(kU10EmployeeDTO.TraktamenteUtomRiket)));
                if (kU10EmployeeDTO.TjansteresaOver3MInrikes.HasValue && kU10EmployeeDTO.TjansteresaOver3MInrikes.Value)
                    elements.Add(new XElement(namespaceKU + "TjansteresaOver3MInrikes", new XAttribute("faltkod", "053"), BoolToString(kU10EmployeeDTO.TjansteresaOver3MInrikes)));
                if (kU10EmployeeDTO.TjansteresaOver3MUtrikes.HasValue && kU10EmployeeDTO.TjansteresaOver3MUtrikes.Value)
                    elements.Add(new XElement(namespaceKU + "TjansteresaOver3MUtrikes", new XAttribute("faltkod", "054"), BoolToString(kU10EmployeeDTO.TjansteresaOver3MUtrikes)));
                if (kU10EmployeeDTO.Resekostnader.HasValue && kU10EmployeeDTO.Resekostnader.Value)
                    elements.Add(new XElement(namespaceKU + "Resekostnader", new XAttribute("faltkod", "055"), BoolToString(kU10EmployeeDTO.Resekostnader)));
                if (kU10EmployeeDTO.Logi.HasValue && kU10EmployeeDTO.Logi.Value)
                    elements.Add(new XElement(namespaceKU + "Logi", new XAttribute("faltkod", "056"), BoolToString(kU10EmployeeDTO.Logi)));

                //Arbetsställenummer
                if (!String.IsNullOrEmpty(employee.WorkPlaceSCB))
                    elements.Add(new XElement(namespaceKU + "Arbetsstallenummer", new XAttribute("faltkod", "060"), employee.WorkPlaceSCB));

                //Kryss
                if (kU10EmployeeDTO.Delagare.HasValue && kU10EmployeeDTO.Delagare.Value)
                    elements.Add(new XElement(namespaceKU + "Delagare", new XAttribute("faltkod", "061"), BoolToString(kU10EmployeeDTO.Delagare)));

                //Annan förmån: Lönetype Förmån->Annan kryss i rutan 047 (benämningen på löneraten som har annan förmån i rutan 065)
                if (!String.IsNullOrEmpty(kU10EmployeeDTO.SpecAvAnnanForman))
                    elements.Add(new XElement(namespaceKU + "SpecAvAnnanForman", new XAttribute("faltkod", "065"), kU10EmployeeDTO.SpecAvAnnanForman));
                if (!String.IsNullOrEmpty(kU10EmployeeDTO.SpecVissaAvdrag))
                    elements.Add(new XElement(namespaceKU + "SpecVissaAvdrag", new XAttribute("faltkod", "070"), kU10EmployeeDTO.SpecVissaAvdrag));
                if (kU10EmployeeDTO.SocialAvgiftsAvtal != 0)
                    elements.Add(new XElement(namespaceKU + "SocialAvgiftsAvtal", new XAttribute("faltkod", "093"), decimal.Truncate(kU10EmployeeDTO.SocialAvgiftsAvtal).ToString()));
            }

            //Inkomstår och specifikationsnummer
            elements.Add(new XElement(namespaceKU + "Inkomstar", new XAttribute("faltkod", "203"), kU10EmployeeDTO.Inkomstar));
            elements.Add(new XElement(namespaceKU + "Specifikationsnummer", new XAttribute("faltkod", "570"), kU10EmployeeDTO.Specifikationsnummer.ToString()));

            return elements;
        }

        private List<XElement> CreateEmployeeInformationElements(AgdEmployeeDTO agdEmployeeDTO, bool sinkNoTax = false)
        {
            List<XElement> elements = new List<XElement>();

            if (agdEmployeeDTO.Borttag)
            {
                elements.Add(new XElement(this.namespaceAGD + "Borttag", new XAttribute("faltkod", "205"), IntToString(agdEmployeeDTO.Borttag.ToInt())));
            }
            else
            {
                if (sinkNoTax && (agdEmployeeDTO.AvdrPrelSkatt != 0))
                    sinkNoTax = false;

                if (agdEmployeeDTO.AvdrSkattSINK == 0 && !sinkNoTax && (agdEmployeeDTO.AvdrPrelSkatt != 0 || agdEmployeeDTO.KontantErsattningUlagAG != 0 || agdEmployeeDTO.KontantErsattningEjUlagSA != 0 || agdEmployeeDTO.SkatteplOvrigaFormanerUlagAG != 0))
                    elements.Add(new XElement(this.namespaceAGD + "AvdrPrelSkatt", new XAttribute("faltkod", "001"), IntToString(agdEmployeeDTO.AvdrPrelSkatt)));
                if (agdEmployeeDTO.AvdrSkattSINK != 0 || sinkNoTax)
                    elements.Add(new XElement(this.namespaceAGD + "AvdrSkattSINK", new XAttribute("faltkod", "274"), IntToString(agdEmployeeDTO.AvdrSkattSINK)));
                if (agdEmployeeDTO.AvdrSkattASINK != 0)
                    elements.Add(new XElement(this.namespaceAGD + "AvdrSkattASINK", new XAttribute("faltkod", "275"), IntToString(agdEmployeeDTO.AvdrSkattASINK)));
                if (!String.IsNullOrEmpty(agdEmployeeDTO.Arbetsstallenummer))
                    elements.Add(new XElement(this.namespaceAGD + "Arbetsstallenummer", new XAttribute("faltkod", "060"), agdEmployeeDTO.Arbetsstallenummer));
                if (agdEmployeeDTO.AvrakningAvgiftsfriErs != 0)
                    elements.Add(new XElement(this.namespaceAGD + "AvrakningAvgiftsfriErs", new XAttribute("faltkod", "010"), IntToString(agdEmployeeDTO.AvrakningAvgiftsfriErs)));
                if (agdEmployeeDTO.KontantErsattningUlagAG != 0)
                    elements.Add(new XElement(this.namespaceAGD + "KontantErsattningUlagAG", new XAttribute("faltkod", "011"), IntToString(agdEmployeeDTO.KontantErsattningUlagAG)));
                if (agdEmployeeDTO.SkatteplOvrigaFormanerUlagAG != 0)
                    elements.Add(new XElement(this.namespaceAGD + "SkatteplOvrigaFormanerUlagAG", new XAttribute("faltkod", "012"), IntToString(agdEmployeeDTO.SkatteplOvrigaFormanerUlagAG)));
                if (agdEmployeeDTO.SkatteplBilformanUlagAG != 0)
                    elements.Add(new XElement(this.namespaceAGD + "SkatteplBilformanUlagAG", new XAttribute("faltkod", "013"), IntToString(agdEmployeeDTO.SkatteplBilformanUlagAG)));
                if (agdEmployeeDTO.DrivmVidBilformanUlagAG != 0)
                    elements.Add(new XElement(this.namespaceAGD + "DrivmVidBilformanUlagAG", new XAttribute("faltkod", "018"), IntToString(agdEmployeeDTO.DrivmVidBilformanUlagAG)));
                if (agdEmployeeDTO.AvdragUtgiftArbetet != 0)
                    elements.Add(new XElement(this.namespaceAGD + "AvdragUtgiftArbetet", new XAttribute("faltkod", "019"), IntToString(agdEmployeeDTO.AvdragUtgiftArbetet)));
                if (agdEmployeeDTO.Tjanstepension != 0)
                    elements.Add(new XElement(this.namespaceAGD + "Tjanstepension", new XAttribute("faltkod", "030"), IntToString(agdEmployeeDTO.Tjanstepension)));
                if (agdEmployeeDTO.BostadsformanSmahusUlagAG)
                    elements.Add(new XElement(this.namespaceAGD + "BostadsformanSmahusUlagAG", new XAttribute("faltkod", "041"), BoolToString(agdEmployeeDTO.BostadsformanSmahusUlagAG)));
                if (agdEmployeeDTO.BostadsformanEjSmahusUlagAG)
                    elements.Add(new XElement(this.namespaceAGD + "BostadsformanEjSmahusUlagAG", new XAttribute("faltkod", "043"), BoolToString(agdEmployeeDTO.BostadsformanEjSmahusUlagAG)));
                if (agdEmployeeDTO.Bilersattning)
                    elements.Add(new XElement(this.namespaceAGD + "Bilersattning", new XAttribute("faltkod", "050"), BoolToString(agdEmployeeDTO.Bilersattning)));
                if (agdEmployeeDTO.Traktamente)
                    elements.Add(new XElement(this.namespaceAGD + "Traktamente", new XAttribute("faltkod", "051"), BoolToString(agdEmployeeDTO.Traktamente)));
                if (agdEmployeeDTO.AndelStyrelsearvode != 0)
                    elements.Add(new XElement(this.namespaceAGD + "AndelStyrelsearvode", new XAttribute("faltkod", "023"), IntToString(agdEmployeeDTO.AndelStyrelsearvode)));
                if (agdEmployeeDTO.AndraKostnadsers != 0)
                    elements.Add(new XElement(this.namespaceAGD + "AndraKostnadsers", new XAttribute("faltkod", "020"), IntToString(agdEmployeeDTO.AndraKostnadsers)));
                if (agdEmployeeDTO.KontantErsattningEjUlagSA != 0)
                    elements.Add(new XElement(this.namespaceAGD + "KontantErsattningEjUlagSA", new XAttribute("faltkod", "131"), IntToString(agdEmployeeDTO.KontantErsattningEjUlagSA)));
                if (agdEmployeeDTO.SkatteplOvrigaFormanerEjUlagSA != 0)
                    elements.Add(new XElement(this.namespaceAGD + "SkatteplOvrigaFormanerEjUlagSA", new XAttribute("faltkod", "132"), IntToString(agdEmployeeDTO.SkatteplOvrigaFormanerEjUlagSA)));
                if (agdEmployeeDTO.UnderlagRutarbete != 0)
                    elements.Add(new XElement(this.namespaceAGD + "UnderlagRutarbete", new XAttribute("faltkod", "021"), IntToString(agdEmployeeDTO.UnderlagRutarbete)));
                if (agdEmployeeDTO.UnderlagRotarbete != 0)
                    elements.Add(new XElement(this.namespaceAGD + "UnderlagRotarbete", new XAttribute("faltkod", "022"), IntToString(agdEmployeeDTO.UnderlagRotarbete)));
                if (agdEmployeeDTO.Hyresersattning != 0)
                    elements.Add(new XElement(this.namespaceAGD + "Hyresersattning", new XAttribute("faltkod", "039"), IntToString(agdEmployeeDTO.Hyresersattning)));
                if (agdEmployeeDTO.FormanHarJusterats)
                    elements.Add(new XElement(this.namespaceAGD + "FormanHarJusterats", new XAttribute("faltkod", "048"), BoolToString(agdEmployeeDTO.FormanHarJusterats)));
            }

            return elements;
        }

        #endregion

        #region Create Schema

        private XmlSchemaSet CreateKUSchemas()
        {
            string kontrolluppgifter_1_2_xsd = File.ReadAllText(ConfigSettings.SOE_SERVER_DIR_REPORT_EXTERNAL_SKV_PHYSICAL + "Kontrolluppgifter_COMPONENT_4.0.xsd");
            string gemensamt_SKV_COMMON_1_2_xsd = File.ReadAllText(ConfigSettings.SOE_SERVER_DIR_REPORT_EXTERNAL_SKV_PHYSICAL + "gemensamt/SKV_COMMON_4.0.xsd");

            XmlSchemaSet schemas = new XmlSchemaSet();
            schemas.Add(namespaceKU.ToString(), XmlReader.Create(new StringReader(kontrolluppgifter_1_2_xsd)));
            schemas.Add(namespaceGM.ToString(), XmlReader.Create(new StringReader(gemensamt_SKV_COMMON_1_2_xsd)));

            return schemas;
        }

        #endregion

        #region Convert

        private KU10EmployeeTransactionDTO ConvertToKU10EmployeeTransactionDTO(TimePayrollStatisticsSmallDTO timePayrollStatisticsSmallDTO, string type)
        {
            return new KU10EmployeeTransactionDTO()
            {
                PayrollProductNumber = timePayrollStatisticsSmallDTO.PayrollProductNumber,
                Name = timePayrollStatisticsSmallDTO.PayrollProductName,
                Type = type,
                Quantity = timePayrollStatisticsSmallDTO.Quantity,
                Amount = timePayrollStatisticsSmallDTO.Amount,
                IsPayrollStartValue = timePayrollStatisticsSmallDTO.IsPayrollStartValues,
                TimePeriodName = timePayrollStatisticsSmallDTO.TimePeriodName,
                Date = timePayrollStatisticsSmallDTO.Date,
            };
        }

        #endregion

        #region Help-methods

        private string ValidateAgdXDocument(XDocument document)
        {
            string value = string.Empty;
            bool errors = false;
            string message = "";
            string path = ConfigSettings.SOE_SERVER_DIR_REPORT_EXTERNAL_SKV_PHYSICAL + "component/arbetsgivardeklaration_component_1.1.xsd";
            string file = File.ReadAllText(path);

            XmlSchemaSet schemas = new XmlSchemaSet();
            schemas.Add(namespaceAGD.ToString(), XmlReader.Create(new StringReader(file)));
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

        private string ValidateKUXDocument(XDocument document)
        {
            string value = string.Empty;
            bool errors = false;
            string message = "";

            XmlSchemaSet schemas = CreateKUSchemas();

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

        private string CleanOrgNr(string value)
        {
            value = value.Replace("-", "").Trim();

            if (!value.StartsWith("16"))
                value = "16" + value;

            return value;
        }

        private string CleanSocSec(string value)
        {
            value = value.Replace("-", "").Replace("X", "").Replace("x", "").Trim();
            return value;
        }

        private bool IsSocialSecWithX(string value)
        {
            return value != null && value.Contains("-x") || value.Contains("-X");
        }

        private bool IsSamordningsnummer(string value)
        {
            if (value.Length < 10)
                return false;

            int day = 0;
            int.TryParse(value.Substring(6, 2), out day);
            if (day > 60)
                return true;
            return false;
        }

        private string DecToString(decimal dec)
        {
            string value = Convert.ToInt32(dec).ToString();
            if (value == null)
                value = string.Empty;
            value = value.Trim();
            return value;
        }

        private string IntToString(int integer)
        {
            string value = integer.ToString();
            if (value == null)
                value = string.Empty;
            value = value.Trim();
            return value;
        }

        private string BoolToString(bool boolean)
        {
            string value = string.Empty;
            if (boolean == false)
                value = "0";
            else
                value = "1";
            return value;
        }

        private string BoolToString(bool? boolean)
        {
            string value = string.Empty;
            if (boolean.HasValue)
            {
                if (boolean == false)
                    value = "0";
                else
                    value = "1";
            }
            return value;
        }

        private string ToXml(XDocument xDoc)
        {
            StringBuilder builder = new StringBuilder();
            using (TextWriter writer = new StringWriter(builder))
            {
                xDoc.Save(writer);
                var xml = builder.ToString();
                xml = xml.Replace("utf-16", "utf-8");

                return xml;
            }
        }

        #endregion
    }
}
