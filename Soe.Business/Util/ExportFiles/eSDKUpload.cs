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
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.ExportFiles
{
    //Namespaces: From Swedish tax officials documentation
    //XNamespace namespaceDefault = "http://xmls.skatteverket.se/se/skatteverket/ai/instans/infoForBeskattning/1.1";
    //XNamespace namespaceKU = "http://xmls.skatteverket.se/se/skatteverket/ai/komponent/infoForBeskattning/";

    public class eSKDUpload : ExportFilesBase
    {
        #region Ctor

        public eSKDUpload(ParameterObject parameterObject, CreateReportResult reportResult) : base(parameterObject, reportResult) { }

        #endregion

        #region Public methods

        public string CreateFileERP(CompEntities entities, Company company)
        {
            string publicId = @"-//Skatteverket, Sweden//DTD Skatteverket eSKDUpload-DTD Version 6.0//SV";
            string systemId = @"https://www1.skatteverket.se/demoeskd/eSKDUpload_6p0.dtd";
            
            XDocument document = new XDocument(
            new XDocumentType("eSKDUpload", publicId, systemId, ""));
            document.Declaration = new XDeclaration("1.0", "ISO-8859-1", "");
            var root = new XElement("eSKDUpload", new XAttribute("Version", "6.0"));

            document.Add(root);

            if (company != null)
            {
                root.Add(new XElement("OrgNr", company.OrgNr));
            }

            //<?xml version="1.0" encoding="ISO-8859-1"?>
            //<!DOCTYPE eSKDUpload PUBLIC "-//Skatteverket, Sweden//DTD Skatteverket eSKDUpload-DTD Version 6.0//SV" "https://www1.skatteverket.se/demoeskd/eSKDUpload_6p0.dtd">
            //<eSKDUpload Version="6.0">
            //  <OrgNr>599900-0465</OrgNr>
            //  <Moms>
            //    <Period>201507</Period>
            //    <ForsMomsEjAnnan>100000</ForsMomsEjAnnan>
            //    <UttagMoms>200000</UttagMoms>
            //    <UlagMargbesk>300000</UlagMargbesk>
            //    <HyrinkomstFriv>400000</HyrinkomstFriv>
            //    <InkopVaruAnnatEg>5000</InkopVaruAnnatEg>
            //    <InkopTjanstAnnatEg>6000</InkopTjanstAnnatEg>
            //    <InkopTjanstUtomEg>7000</InkopTjanstUtomEg>
            //    <InkopVaruSverige>8000</InkopVaruSverige>
            //    <InkopTjanstSverige>9000</InkopTjanstSverige>
            //    <MomsUlagImport>10000</MomsUlagImport>
            //    <ForsVaruAnnatEg>11000</ForsVaruAnnatEg>
            //    <ForsVaruUtomEg>12000</ForsVaruUtomEg>
            //    <InkopVaruMellan3p>13000</InkopVaruMellan3p>
            //    <ForsVaruMellan3p>14000</ForsVaruMellan3p>
            //    <ForsTjSkskAnnatEg>15000</ForsTjSkskAnnatEg>
            //    <ForsTjOvrUtomEg>16000</ForsTjOvrUtomEg>
            //    <ForsKopareSkskSverige>17000</ForsKopareSkskSverige>
            //    <ForsOvrigt>18000</ForsOvrigt>
            //    <MomsUtgHog>200000</MomsUtgHog>
            //    <MomsUtgMedel>15000</MomsUtgMedel>
            //    <MomsUtgLag>5000</MomsUtgLag>
            //    <MomsInkopUtgHog>2500</MomsInkopUtgHog>
            //    <MomsInkopUtgMedel>1000</MomsInkopUtgMedel>
            //    <MomsInkopUtgLag>500</MomsInkopUtgLag>
            //    <MomsImportUtgHog>2000</MomsImportUtgHog>
            //    <MomsImportUtgMedel>350</MomsImportUtgMedel>
            //    <MomsImportUtgLag>150</MomsImportUtgLag>
            //    <MomsIngAvdr>1000</MomsIngAvdr>
            //    <MomsBetala>225500</MomsBetala>
            //    <TextUpplysningMoms>Bla bla bla bla</TextUpplysningMoms>
            //  </Moms>
            //</eSKDUpload>

            bool ok;
            DateTime from = CalendarUtility.DATETIME_DEFAULT;
            DateTime to = CalendarUtility.DATETIME_MAXVALUE;
            if (es != null)
            {
                from = es.DateFrom;
                to = es.DateTo;
                ok = CreateErpXML(entities, root, es.ActorCompanyId, es.DateFrom, es.DateTo);
            }
            else if (this.ReportResult != null)
            {
                bool createVatVoucher = false;
                TryGetBoolFromSelection(this.ReportResult, out createVatVoucher, "createVatVoucher");
                TryAccountingDatesFromSelection(entities, this.ReportResult, out from, out to);

                if (createVatVoucher)
                {
                    var createResult = VoucherManager.CreateVatVoucher(from, to, this.ReportResult.ActorCompanyId);
                    if (!createResult.Success)
                    {
                        this.ReportResult.SetErrorMessage(SoeReportDataResultMessage.CreateVoucherFailed, createResult.ErrorMessage);
                        return "";
                    }
                }

                ok = CreateErpXML(entities, root, this.ReportResult.ActorCompanyId, from, to);
            }
            else
            {
                ok = false;
            }

            var path = "";

            if (ok)
            {
                string fileName = IOUtil.FileNameSafe(company.Name + "_eSDK_" + GetYearMonthDay(from) + " - " + GetYearMonthDay(to));
                DirectoryInfo directory = Directory.CreateDirectory(ConfigSettings.SOE_SERVER_DIR_TEMP_REPORT_PHYSICAL + Guid.NewGuid().ToString());
                path = StringUtility.GetValidFilePath(directory.FullName) + fileName + ".xml";

                try
                {
                    document.Save(path);
                }
                catch (Exception ex)
                {
                    LogError(ex, this.log);
                    path = string.Empty;
                }
            }

            return path;
        }
        
        private bool CreateErpXML(CompEntities entities, XElement root, int actorCompanyId, DateTime dateFrom, DateTime dateTo)
        {
            List<SysVatAccount> sysVatAccounts = SysDbCache.Instance.SysVatAccounts;
            List<AccountStd> accountStds = AccountManager.GetAccountStdsByCompany(entities, actorCompanyId, null, true, false);

            List<AccountYear> accountYears = AccountManager.GetAccountYears(entities, actorCompanyId, false, dateFrom, dateTo);

            XElement momsElement = new XElement("Moms");

            string period = dateTo != CalendarUtility.DATETIME_DEFAULT ? dateTo.ToString("yyyyMM") : dateFrom.ToString("yyyyMM");

            decimal balance = 0;
            momsElement.Add(new XElement("Period", period));
            momsElement.Add(CreateMomsElement(entities, "ForsMomsEjAnnan", 5, actorCompanyId, dateFrom, dateTo, accountYears, accountStds, sysVatAccounts, ref balance));
            momsElement.Add(CreateMomsElement(entities, "UttagMoms", 6, actorCompanyId, dateFrom, dateTo, accountYears, accountStds, sysVatAccounts, ref balance));
            momsElement.Add(CreateMomsElement(entities, "UlagMargbesk", 7, actorCompanyId, dateFrom, dateTo, accountYears, accountStds, sysVatAccounts, ref balance));
            momsElement.Add(CreateMomsElement(entities, "HyrinkomstFriv", 8, actorCompanyId, dateFrom, dateTo, accountYears, accountStds, sysVatAccounts, ref balance));
            momsElement.Add(CreateMomsElement(entities, "InkopVaruAnnatEg", 20, actorCompanyId, dateFrom, dateTo, accountYears, accountStds, sysVatAccounts, ref balance));
            momsElement.Add(CreateMomsElement(entities, "InkopTjanstAnnatEg", 21, actorCompanyId, dateFrom, dateTo, accountYears, accountStds, sysVatAccounts, ref balance));
            momsElement.Add(CreateMomsElement(entities, "InkopTjanstUtomEg", 22, actorCompanyId, dateFrom, dateTo, accountYears, accountStds, sysVatAccounts, ref balance));
            momsElement.Add(CreateMomsElement(entities, "InkopVaruSverige", 23, actorCompanyId, dateFrom, dateTo, accountYears, accountStds, sysVatAccounts, ref balance));
            momsElement.Add(CreateMomsElement(entities, "InkopTjanstSverige", 24, actorCompanyId, dateFrom, dateTo, accountYears, accountStds, sysVatAccounts, ref balance));
            momsElement.Add(CreateMomsElement(entities, "MomsUlagImport", 25, actorCompanyId, dateFrom, dateTo, accountYears, accountStds, sysVatAccounts, ref balance));
            momsElement.Add(CreateMomsElement(entities, "ForsVaruAnnatEg", 35, actorCompanyId, dateFrom, dateTo, accountYears, accountStds, sysVatAccounts, ref balance));
            momsElement.Add(CreateMomsElement(entities, "ForsVaruUtomEg", 36, actorCompanyId, dateFrom, dateTo, accountYears, accountStds, sysVatAccounts, ref balance));
            momsElement.Add(CreateMomsElement(entities, "InkopVaruMellan3p", 37, actorCompanyId, dateFrom, dateTo, accountYears, accountStds, sysVatAccounts, ref balance));
            momsElement.Add(CreateMomsElement(entities, "ForsVaruMellan3p", 38, actorCompanyId, dateFrom, dateTo, accountYears, accountStds, sysVatAccounts, ref balance));
            momsElement.Add(CreateMomsElement(entities, "ForsTjSkskAnnatEg", 39, actorCompanyId, dateFrom, dateTo, accountYears, accountStds, sysVatAccounts, ref balance));
            momsElement.Add(CreateMomsElement(entities, "ForsTjOvrUtomEg", 40, actorCompanyId, dateFrom, dateTo, accountYears, accountStds, sysVatAccounts, ref balance));
            momsElement.Add(CreateMomsElement(entities, "ForsKopareSkskSverige", 41, actorCompanyId, dateFrom, dateTo, accountYears, accountStds, sysVatAccounts, ref balance));
            momsElement.Add(CreateMomsElement(entities, "ForsOvrigt", 42, actorCompanyId, dateFrom, dateTo, accountYears, accountStds, sysVatAccounts, ref balance));
            momsElement.Add(CreateMomsElement(entities, "MomsUtgHog", 10, actorCompanyId, dateFrom, dateTo, accountYears, accountStds, sysVatAccounts, ref balance));
            momsElement.Add(CreateMomsElement(entities, "MomsUtgMedel", 11, actorCompanyId, dateFrom, dateTo, accountYears, accountStds, sysVatAccounts, ref balance));
            momsElement.Add(CreateMomsElement(entities, "MomsUtgLag", 12, actorCompanyId, dateFrom, dateTo, accountYears, accountStds, sysVatAccounts, ref balance));
            momsElement.Add(CreateMomsElement(entities, "MomsInkopUtgHog", 30, actorCompanyId, dateFrom, dateTo, accountYears, accountStds, sysVatAccounts, ref balance));
            momsElement.Add(CreateMomsElement(entities, "MomsInkopUtgMedel", 31, actorCompanyId, dateFrom, dateTo, accountYears, accountStds, sysVatAccounts, ref balance));
            momsElement.Add(CreateMomsElement(entities, "MomsInkopUtgLag", 32, actorCompanyId, dateFrom, dateTo, accountYears, accountStds, sysVatAccounts, ref balance));
            momsElement.Add(CreateMomsElement(entities, "MomsImportUtgHog", 60, actorCompanyId, dateFrom, dateTo, accountYears, accountStds, sysVatAccounts, ref balance));
            momsElement.Add(CreateMomsElement(entities, "MomsImportUtgMedel", 00, actorCompanyId, dateFrom, dateTo, accountYears, accountStds, sysVatAccounts, ref balance));
            momsElement.Add(CreateMomsElement(entities, "MomsImportUtgLag", 00, actorCompanyId, dateFrom, dateTo, accountYears, accountStds, sysVatAccounts, ref balance));
            momsElement.Add(CreateMomsElement(entities, "MomsIngAvdr", 48, actorCompanyId, dateFrom, dateTo, accountYears, accountStds, sysVatAccounts, ref balance));
            momsElement.Add(new XElement("MomsBetala", balance));
            momsElement.Add(new XElement("TextUpplysningMoms", string.Empty));

            root.Add(momsElement);

            return true;
        }

        public XElement CreateMomsElement(CompEntities entities, string name, int VatNr,int actorCompanyId, DateTime dateFrom, DateTime dateTo, List<AccountYear> accountYears, List<AccountStd> accountStds, List<SysVatAccount> sysVatAccounts, ref decimal amount)
        {
            decimal balanceAmount = AccountBalanceManager(actorCompanyId).GetBalanceChangeForVatAccount(entities, actorCompanyId, VatNr, accountYears, dateFrom, dateTo, sysVatAccounts, accountStds, false).Balance;
            balanceAmount = decimal.Negate(balanceAmount);
            balanceAmount = decimal.Truncate(balanceAmount);

            if (VatNr == 10 || VatNr == 11 || VatNr == 12 || VatNr == 30 || VatNr == 31 || VatNr == 32 || VatNr == 60 || VatNr == 61 || VatNr == 62 || VatNr == 48)
                amount += balanceAmount;

            XElement element = new XElement(name, Math.Abs(balanceAmount));

            return element;
        }

        public string CreateFileHR(CompEntities entities, CreateReportResult createReportResult)
        {
            #region Init

            EmployeeManager em = new EmployeeManager(parameterObject);
            TimeTransactionManager ttm = new TimeTransactionManager(parameterObject);
            base.reportResult = createReportResult;

            #endregion

            #region Prereq

            if (!TryGetIdsFromSelection(reportResult, out List<int> selectionTimePeriodIds, "periods"))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionTimePeriodIds, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return null;

            if (employees == null)
                employees = em.GetAllEmployeesByIds(entities, base.ActorCompanyId, selectionEmployeeIds, loadEmployment: true);

            var timePayrollTransactionItems = ttm.GetTimePayrollStatisticsSmallDTOs_new(entities, base.ActorCompanyId, employees, selectionTimePeriodIds, applyEmploymentTaxMinimumRule: true);
            DateTime dateFrom = reportResult.EvaluatedSelection != null ? reportResult.EvaluatedSelection.DateFrom : CalendarUtility.DATETIME_DEFAULT;
            DateTime dateTo = reportResult.EvaluatedSelection != null ? reportResult.EvaluatedSelection.DateTo : CalendarUtility.DATETIME_DEFAULT;

            List<TimePeriod> timePeriods = null;
            if (dateFrom == CalendarUtility.DATETIME_DEFAULT && reportResult.EvaluatedSelection == null)
            {
                timePeriods = TimePeriodManager.GetTimePeriods(selectionTimePeriodIds, base.ActorCompanyId);
                if (timePeriods != null && timePeriods.Any(a => a.PaymentDate.HasValue))
                    dateFrom = timePeriods.OrderBy(o => o.PaymentDate).First().PaymentDate.Value;
            }

            if (dateTo == CalendarUtility.DATETIME_DEFAULT && reportResult.EvaluatedSelection == null)
            {
                if (timePeriods == null)
                    timePeriods = TimePeriodManager.GetTimePeriods(selectionTimePeriodIds, base.ActorCompanyId);
                if (timePeriods != null && timePeriods.Any(a => a.PaymentDate.HasValue))
                    dateTo = timePeriods.OrderByDescending(o => o.PaymentDate).First().PaymentDate.Value;
            }

            #endregion

            #region Init repository

            PersonalDataEmployeeReportRepository personalDataRepository = reportResult.EvaluatedSelection != null ?
                new PersonalDataEmployeeReportRepository(parameterObject, reportResult.EvaluatedSelection) :
                  new PersonalDataEmployeeReportRepository(parameterObject, ReportResult);

            #endregion

            #region Init document

            string str1 = @"-//Skatteverket, Sweden//DTD Skatteverket eSKDUpload-DTD Version 6.0//SV";
            string str2 = @"https://www1.skatteverket.se/demoeskd/eSKDUpload_6p0.dtd";

            XDocument document = new XDocument(
            new XComment("Company:" + createReportResult.Input.Company.Name),
            new XDocumentType("eSKDUpload", str1, str2, ""));
            document.Declaration = new XDeclaration("1.0", "UTF-8", "yes");

            #endregion

            #region Create root

            XElement root = new XElement("eSKDUpload");
            root.Add(new XElement("OrgNr", reportResult.Input.Company.OrgNr));

            #endregion

            #region Create Moms & Ag

            root.Add(this.CreateAgElement(timePayrollTransactionItems, selectionTimePeriodIds, dateFrom, employees));

            #endregion

            #region Create & save to file

            document.Add(root);

            string fileName = IOUtil.FileNameSafe(reportResult.Input.Company.Name + "_eSDK_" + GetYearMonthDay(dateFrom) + " - " + GetYearMonthDay(dateTo));
            DirectoryInfo directory = Directory.CreateDirectory(ConfigSettings.SOE_SERVER_DIR_TEMP_REPORT_PHYSICAL + Guid.NewGuid().ToString());
            string filePath = StringUtility.GetValidFilePath(directory.FullName) + fileName + ".xml";

            try
            {
                document.Save(filePath);
            }
            catch (Exception ex)
            {
                SysLogManager slm = new SysLogManager(parameterObject);
                slm.AddSysLog(ex, log4net.Core.Level.Error);
            }

            #endregion

            #region Close repository

            personalDataRepository.GenerateLogs();

            #endregion

            return filePath;
        }

        public SKDDTO CreateSKDDTO(List<TimePayrollStatisticsSmallDTO> timePayrollTransactionItems, List<int> timePeriodIds, DateTime dateFrom, List<Employee> employees)
        {
            SKDDTO skdEmployeeDTO = new SKDDTO()
            {
                KodAmerika = false,
                Transactions = new List<SKDEmployeeTransactionDTO>(),
            };

            List<TimePeriod> timePeriods = TimePeriodManager.GetTimePeriods(timePeriodIds, base.ActorCompanyId);
            if (timePeriods.IsNullOrEmpty())
                return skdEmployeeDTO;

            TimePeriod first = timePeriods.OrderByDescending(t => t.PaymentDate.Value).FirstOrDefault();
            TimePeriod last = timePeriods.OrderBy(x => x.PayrollStartDate).LastOrDefault();
            if (first == null || last == null)
                return skdEmployeeDTO;

            DateTime paymentDate = first.PaymentDate.Value;
            int year = last.PayrollStopDate?.Year ?? 0;
            decimal defaultEmploymentTax = PayrollManager.GetSysPayrollPriceIntervalAmount(base.ActorCompanyId, (int)TermGroup_SysPayrollPrice.SE_EmploymentTax, 1977, paymentDate);

            foreach (Employee employee in employees)
            {
                decimal employeeEmploymentTax = defaultEmploymentTax;
                var birthYear = EmployeeManager.GetEmployeeBirthDate(employee);
                bool underLimit = false;

                if (birthYear.HasValue)
                    employeeEmploymentTax = PayrollManager.GetSysPayrollPriceIntervalAmount(base.ActorCompanyId, (int)TermGroup_SysPayrollPrice.SE_EmploymentTax, EmployeeManager.GetEmployeeBirthDate(employee).Value.Year, paymentDate);

                foreach (var transaction in timePayrollTransactionItems.Where(t => t.EmployeeId == employee.EmployeeId))
                {
                    #region Summery

                    if ((transaction.IsGrossSalary() || transaction.IsBenefitAndNotInvert()) && !transaction.IsBelowEmploymentTaxLimitRuleFromPreviousPeriods)
                    {
                        skdEmployeeDTO.UlagSkAvdrLon += transaction.Amount; //81
                        skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(transaction, "UlagSkAvdrLon"));
                    }

                    // förmån ska dras från bruttolön
                    if (transaction.IsGrossSalary() && !transaction.IsBenefit() && !transaction.IsBelowEmploymentTaxLimitRuleHidden)
                    {
                        skdEmployeeDTO.LonBrutto += transaction.Amount;  //50
                        skdEmployeeDTO.SumUlagAvg += transaction.Amount; //53
                        skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(transaction, "LonBrutto"));
                    }

                    if (transaction.IsBenefitAndNotInvert() && !transaction.IsBenefit_Fuel_PartAnnualized())
                    {
                        skdEmployeeDTO.Forman += transaction.Amount; //51
                        //skdEmployeeDTO.LonBrutto -= trans.Amount;
                        //skdEmployeeDTO.SumUlagAvg -= trans.Amount;  //53
                        skdEmployeeDTO.SumUlagAvg += transaction.Amount; //53
                        skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(transaction, "Forman"));
                    }
                    if (transaction.IsCostDeduction())
                    {
                        skdEmployeeDTO.AvdrKostn += transaction.Amount; //52
                        skdEmployeeDTO.SumUlagAvg -= transaction.Amount; //53
                        skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(transaction, "AvdrKostn"));
                    }

                    #endregion

                    #region Arbetsgivargifter och underlag

                    if (transaction.IsEmploymentTaxCredit())
                    {
                        if (transaction.IsEmploymentTaxCreditEarlyPension(year, EmployeeManager.GetEmployeeBirthDate(employee)))
                        {
                            if (!transaction.IsBelowEmploymentTaxLimitRuleHidden)
                            {
                                skdEmployeeDTO.AvgAldersp += transaction.Amount; //60
                                skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(transaction, "AvgAldersp"));

                                if (transaction.IsBelowEmploymentTaxLimitRuleFromPreviousPeriods)
                                {
                                    skdEmployeeDTO.AvgAlderspFromPreviousPeriods += transaction.Amount; //60
                                    skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(transaction, "AvgAlderspFromPreviousPeriods"));
                                }
                            }
                            else
                            {
                                underLimit = true;
                                skdEmployeeDTO.AvgAlderspLessThanLimit += transaction.Amount; //60
                                skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(transaction, "AvgAlderspLessThanLimit"));
                            }

                        }
                        else if (birthYear.HasValue && birthYear.Value.Year <= 1937)
                        {
                            skdEmployeeDTO.SkLonSarsk += transaction.Amount; //62
                            skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(transaction, "SkLonSarsk"));

                        }
                        else if (employeeEmploymentTax != defaultEmploymentTax)
                        {
                            if (!transaction.IsBelowEmploymentTaxLimitRuleHidden)
                            {
                                skdEmployeeDTO.AvgUngdom += transaction.Amount; //62
                                skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(transaction, "AvgUngdom"));

                                if (transaction.IsBelowEmploymentTaxLimitRuleFromPreviousPeriods)
                                {
                                    skdEmployeeDTO.AvgUngdomFromPreviousPeriods += transaction.Amount; //60
                                    skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(transaction, "AvgUngdomFromPreviousPeriods"));
                                }
                            }
                            else
                            {
                                underLimit = true;
                                skdEmployeeDTO.AvgUngdomLessThanLimit += transaction.Amount; //62
                                skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(transaction, "AvgUngdomLessThanLimit"));
                            }
                        }
                        else
                        {
                            if (!transaction.IsBelowEmploymentTaxLimitRuleHidden)
                            {
                                skdEmployeeDTO.AvgHel += transaction.Amount; //56
                                skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(transaction, "AvgHel"));

                                if (transaction.IsBelowEmploymentTaxLimitRuleFromPreviousPeriods)
                                {
                                    skdEmployeeDTO.AvgHelFromPreviousPeriods += transaction.Amount; //60
                                    skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(transaction, "AvgHelFromPreviousPeriods"));
                                }
                            }
                            else
                            {
                                underLimit = true;
                                skdEmployeeDTO.AvgHelLessThanLimit += transaction.Amount; //56
                                skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(transaction, "AvgHelLessThanLimit"));
                            }
                        }
                    }

                    #endregion

                    #region Ambassad

                    //if (PayrollRulesUtil.IsGrossSalaryEmbassy((int)trans.SysPayrollTypeLevel1, (int)trans.SysPayrollTypeLevel2, (int)trans.SysPayrollTypeLevel3, (int)trans.SysPayrollTypeLevel4))
                    //{
                    //    skdEmployeeDTO.UlagAvgAmbassad += trans.Amount; //65
                    //    skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(trans, "UlagAvgAmbassad"));
                    //}
                    //if (PayrollRulesUtil.IsEmploymentTaxCreditEmbassy((int)trans.SysPayrollTypeLevel1, (int)trans.SysPayrollTypeLevel2, (int)trans.SysPayrollTypeLevel3, (int)trans.SysPayrollTypeLevel4))
                    //{
                    //    skdEmployeeDTO.AvgAmbassad += trans.Amount; //66
                    //    skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(trans, "AvgAmbassad"));
                    //}

                    #endregion

                    #region Amerika

                    //if (PayrollRulesUtil.IsGrossSalaryAmerica((int)trans.SysPayrollTypeLevel1, (int)trans.SysPayrollTypeLevel2, (int)trans.SysPayrollTypeLevel3, (int)trans.SysPayrollTypeLevel4))
                    //{
                    //    skdEmployeeDTO.KodAmerika = true;  //67
                    //    skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(trans, "KodAmerika"));

                    //    skdEmployeeDTO.UlagAvgAmerika += trans.Amount; //69
                    //    skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(trans, "UlagAvgAmerika"));
                    //}

                    //if (PayrollRulesUtil.IsEmploymentTaxCreditAmerica((int)trans.SysPayrollTypeLevel1, (int)trans.SysPayrollTypeLevel2, (int)trans.SysPayrollTypeLevel3, (int)trans.SysPayrollTypeLevel4))
                    //{
                    //    skdEmployeeDTO.AvgAmerika += trans.Amount; //70
                    //    skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(trans, "AvgAmerika"));
                    //}

                    #endregion

                    #region Stöd

                    //if (PayrollRulesUtil.IsGrossSalary((int)trans.SysPayrollTypeLevel1, (int)trans.SysPayrollTypeLevel2, (int)trans.SysPayrollTypeLevel3, (int)trans.SysPayrollTypeLevel4))
                    //{
                    //    skdEmployeeDTO.UlagStodForetag += trans.Amount;
                    //    skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(trans, "UlagStodForetag"));
                    //}
                    //if (PayrollRulesUtil.IsGrossSalary((int)trans.SysPayrollTypeLevel1, (int)trans.SysPayrollTypeLevel2, (int)trans.SysPayrollTypeLevel3, (int)trans.SysPayrollTypeLevel4))
                    //{
                    //    skdEmployeeDTO.AvdrStodForetag += trans.Amount;
                    //    skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(trans, "AvdrStodForetag"));
                    //}
                    //if (PayrollRulesUtil.IsGrossSalary((int)trans.SysPayrollTypeLevel1, (int)trans.SysPayrollTypeLevel2, (int)trans.SysPayrollTypeLevel3, (int)trans.SysPayrollTypeLevel4))
                    //{
                    //    skdEmployeeDTO.UlagStodUtvidgat += trans.Amount;
                    //    skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(trans, "UlagStodUtvidgat"));
                    //}
                    //if (PayrollRulesUtil.IsGrossSalary((int)trans.SysPayrollTypeLevel1, (int)trans.SysPayrollTypeLevel2, (int)trans.SysPayrollTypeLevel3, (int)trans.SysPayrollTypeLevel4))
                    //{
                    //    skdEmployeeDTO.AvdrStodUtvidgat += trans.Amount;
                    //    skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(trans, "AvdrStodUtvidgat"));
                    //}

                    #endregion

                    #region Underlag och avdragen för skattedeklaration

                    if (transaction.IsTax())
                    {
                        skdEmployeeDTO.SkAvdrLon += transaction.Amount; //82
                        skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(transaction, "SkAvdrLon"));
                    }
                    //if (transaction.IsOccupationalPension())
                    //{
                    //    skdEmployeeDTO.UlagSkAvdrPension += trans.Amount; //83
                    //    skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(trans, "UlagSkAvdrPension"));
                    //}
                    if (transaction.IsOccupationalPension())
                    {
                        skdEmployeeDTO.SkAvdrPension += transaction.Amount; //84 
                        skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(transaction, "SkAvdrPension"));
                    }
                    //if (transaction.IsBenefit_Interest())
                    //{
                    //    skdEmployeeDTO.UlagSkAvdrRanta += trans.Amount; //85
                    //    skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(trans, "UlagSkAvdrRanta"));
                    //}
                    if (transaction.IsBenefit_Interest())
                    {
                        skdEmployeeDTO.SkAvdrRanta += transaction.Amount; //86
                        skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(transaction, "SkAvdrRanta"));
                    }

                    #endregion

                    if (transaction.IsAbsence_SicknessSalary_Day2_14())
                    {
                        skdEmployeeDTO.SjukLonKostnEhs += transaction.Amount; //99
                        skdEmployeeDTO.SjukLonKostnEhs += PayrollManager.CalculateEmploymentTaxSE(base.ActorCompanyId, dateFrom, transaction.Amount, employee);
                        skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(transaction, "SjukLonKostnEhs"));
                    }
                    else if (transaction.IsAbsence_SicknessSalary_Deduction())
                    {
                        skdEmployeeDTO.SjukLonKostnEhs += transaction.Amount; //99
                        skdEmployeeDTO.SjukLonKostnEhs += PayrollManager.CalculateEmploymentTaxSE(base.ActorCompanyId, dateFrom, transaction.Amount, employee);
                        skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(transaction, "SjukLonKostnEhs"));
                    }
                }

                if (!underLimit)
                {
                    foreach (var transaction in timePayrollTransactionItems.Where(t => t.EmployeeId == employee.EmployeeId))
                    {
                        if ((transaction.IsGrossSalary() || transaction.IsBenefitAndNotInvert()) && transaction.IsBelowEmploymentTaxLimitRuleFromPreviousPeriods)
                        {
                            skdEmployeeDTO.UlagSkAvdrLonFromPreviousPeriods += transaction.Amount; //81
                            skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(transaction, "UlagSkAvdrLonFromPreviousPeriods"));
                        }

                        if (transaction.IsGrossSalary())
                        {
                            if (transaction.IsGrossSalaryEarlyPension(year, EmployeeManager.GetEmployeeBirthDate(employee)))
                            {
                                skdEmployeeDTO.UlagAvgAldersp += transaction.Amount; //59
                                skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(transaction, "UlagAvgAldersp"));

                                if (transaction.IsBelowEmploymentTaxLimitRuleFromPreviousPeriods)
                                {
                                    skdEmployeeDTO.UlagAvgAlderspFromPreviousPeriods += transaction.Amount; //81
                                    skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(transaction, "UlagAvgAlderspFromPreviousPeriods"));
                                }
                            }
                            else if (transaction.IsGrossSalaryYouth(year, EmployeeManager.GetEmployeeBirthDate(employee)))
                            {
                                skdEmployeeDTO.UlagUngdom += transaction.Amount; //57
                                skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(transaction, "UlagUngdom"));

                                if (transaction.IsBelowEmploymentTaxLimitRuleFromPreviousPeriods)
                                {
                                    skdEmployeeDTO.UlagUngdomFromPreviousPeriods += transaction.Amount; //81
                                    skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(transaction, "UlagUngdomFromPreviousPeriods"));
                                }
                            }
                            else if (transaction.IsGrossSalaryTo37(EmployeeManager.GetEmployeeBirthDate(employee)))
                            {

                                skdEmployeeDTO.UlagSkLonSarsk += transaction.Amount; //61
                                skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(transaction, "UlagSkLonSarsk"));

                            }
                            else
                            {
                                skdEmployeeDTO.UlagAvgHel += transaction.Amount; //55
                                skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(transaction, "UlagAvgHel"));

                                if (transaction.IsBelowEmploymentTaxLimitRuleFromPreviousPeriods)
                                {
                                    skdEmployeeDTO.UlagAvgHelFromPreviousPeriods += transaction.Amount; //81
                                    skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(transaction, "UlagAvgHelFromPreviousPeriods"));
                                }
                            }
                        }

                        if (transaction.IsBenefitAndNotInvert())
                        {
                            if (transaction.IsBenefitAndNotInvert38To52(EmployeeManager.GetEmployeeBirthDate(employee)))
                            {
                                if (!transaction.IsBenefit_Fuel_PartAnnualized())
                                {
                                    skdEmployeeDTO.UlagAvgAldersp += transaction.Amount; //59
                                    skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(transaction, "UlagAvgAldersp"));

                                    if (transaction.IsBelowEmploymentTaxLimitRuleFromPreviousPeriods)
                                    {
                                        skdEmployeeDTO.UlagAvgAlderspFromPreviousPeriods += transaction.Amount; //81
                                        skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(transaction, "UlagAvgAlderspFromPreviousPeriods"));
                                    }
                                }
                            }
                            else if (transaction.IsBenefitAndNotInvertFrom91(EmployeeManager.GetEmployeeBirthDate(employee)))
                            {
                                if (!transaction.IsBenefit_Fuel_PartAnnualized())
                                {
                                    skdEmployeeDTO.UlagUngdom += transaction.Amount; //57
                                    skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(transaction, "UlagUngdom"));

                                    if (transaction.IsBelowEmploymentTaxLimitRuleFromPreviousPeriods)
                                    {
                                        skdEmployeeDTO.UlagUngdomFromPreviousPeriods += transaction.Amount; //81
                                        skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(transaction, "UlagUngdomFromPreviousPeriods"));
                                    }
                                }
                            }
                            else if (transaction.IsBenefitAndNotInvertTo37(EmployeeManager.GetEmployeeBirthDate(employee)))
                            {
                                if (!transaction.IsBenefit_Fuel_PartAnnualized())
                                {
                                    skdEmployeeDTO.UlagSkLonSarsk += transaction.Amount; //61
                                    skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(transaction, "UlagSkLonSarsk"));
                                }
                            }
                            else
                            {
                                if (!transaction.IsBenefit_Fuel_PartAnnualized())
                                {
                                    skdEmployeeDTO.UlagAvgHel += transaction.Amount; //55
                                    skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(transaction, "UlagAvgHel"));

                                    if (transaction.IsBelowEmploymentTaxLimitRuleFromPreviousPeriods)
                                    {
                                        skdEmployeeDTO.UlagAvgHelFromPreviousPeriods += transaction.Amount; //81
                                        skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(transaction, "UlagAvgHelFromPreviousPeriods"));
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (var transaction in timePayrollTransactionItems.Where(t => t.EmployeeId == employee.EmployeeId))
                    {
                        if (transaction.IsGrossSalary())
                        {
                            if (transaction.IsGrossSalaryEarlyPension(year, EmployeeManager.GetEmployeeBirthDate(employee)))
                            {
                                skdEmployeeDTO.UlagAvgAlderspLessThanLimit += transaction.Amount; //59
                                skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(transaction, "UlagAvgAlderspLessThanLimit"));
                            }
                            else if (transaction.IsGrossSalaryYouth(year, EmployeeManager.GetEmployeeBirthDate(employee)))
                            {
                                skdEmployeeDTO.UlagUngdomLessThanLimit += transaction.Amount; //57
                                skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(transaction, "UlagUngdomLessThanLimit"));
                            }
                            else
                            {
                                skdEmployeeDTO.UlagAvgHelLessThanLimit += transaction.Amount; //55
                                skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(transaction, "UlagAvgHelLessThanLimit"));
                            }
                        }

                        if (transaction.IsBenefitAndNotInvert())
                        {
                            if (transaction.IsBenefitAndNotInvert38To50(EmployeeManager.GetEmployeeBirthDate(employee)))
                            {
                                skdEmployeeDTO.UlagAvgAlderspLessThanLimit += transaction.Amount; //59
                                skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(transaction, "UlagAvgAlderspLessThanLimit"));
                            }
                            else if (transaction.IsBenefitAndNotInvertFrom91(EmployeeManager.GetEmployeeBirthDate(employee)))
                            {
                                skdEmployeeDTO.UlagUngdomLessThanLimit += transaction.Amount; //57
                                skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(transaction, "UlagUngdomLessThanLimit"));
                            }
                            else
                            {
                                skdEmployeeDTO.UlagAvgHelLessThanLimit += transaction.Amount; //55
                                skdEmployeeDTO.Transactions.Add(ConvertToSKDEmployeeTransaction(transaction, "UlagAvgHelLessThanLimit"));
                            }
                        }
                    }
                }
            }

            skdEmployeeDTO.SumAvg = skdEmployeeDTO.AvgHel + skdEmployeeDTO.AvgAldersp + skdEmployeeDTO.AvgAlderspSkLon + skdEmployeeDTO.AvgAmbassad + skdEmployeeDTO.AvgAmerika;
            skdEmployeeDTO.UlagSumSkAvdr = skdEmployeeDTO.UlagSkAvdrLon + skdEmployeeDTO.UlagSkAvdrPension + skdEmployeeDTO.UlagSkAvdrRanta; //87
            if (skdEmployeeDTO.SkAvdrPension == 0)
                skdEmployeeDTO.SumSkAvdr = skdEmployeeDTO.SkAvdrLon + skdEmployeeDTO.SkAvdrRanta; // 88
            else
                skdEmployeeDTO.SumSkAvdr = -(Math.Abs(skdEmployeeDTO.SkAvdrLon) + Math.Abs(skdEmployeeDTO.SkAvdrPension) + Math.Abs(skdEmployeeDTO.SkAvdrRanta)); // 88
            skdEmployeeDTO.SumAvgBetala = skdEmployeeDTO.SumSkAvdr;
            skdEmployeeDTO.TextUpplysningAg = string.Empty;

            skdEmployeeDTO.Transactions = MergeSKDEmployeeTransactionDTOs(skdEmployeeDTO.Transactions);

            return skdEmployeeDTO;
        }

        public XElement CreateAgElement(List<TimePayrollStatisticsSmallDTO> timePayrollTransactionItems, List<int> timePeriodIds, DateTime dateFrom, List<Employee> employees, SKDDTO skddto = null)
        {
            bool addReportSpecific = true;

            if (skddto == null)
            {
                skddto = CreateSKDDTO(timePayrollTransactionItems, timePeriodIds, dateFrom, employees);
                addReportSpecific = false;
            }

            // es.CalculatedGross = skddto.UlagSkAvdrLon;
            XElement element = new XElement("Ag",
                                new XElement("LonBrutto", skddto.LonBrutto),
                                new XElement("Forman", skddto.Forman),
                                new XElement("AvdrKostn", skddto.AvdrKostn),
                                new XElement("SumUlagAvg", skddto.SumUlagAvg),
                                new XElement("UlagAvgHel", skddto.UlagAvgHel),
                                new XElement("AvgHel", skddto.AvgHel),
                                new XElement("UlagAvgAldersp", skddto.UlagAvgAldersp),
                                new XElement("AvgAldersp", skddto.AvgAldersp),
                                new XElement("UlagAlderspSkLon", skddto.UlagAlderspSkLon),
                                new XElement("AvgAlderspSkLon", skddto.AvgAlderspSkLon),
                                new XElement("UlagSkLonSarsk", skddto.UlagSkLonSarsk),
                                new XElement("SkLonSarsk", skddto.SkLonSarsk),
                                new XElement("UlagAvgAmbassad", skddto.UlagAvgAmbassad),
                                new XElement("AvgAmbassad", skddto.AvgAmbassad),
                                new XElement("KodAmerika", skddto.KodAmerika.ToInt()),
                                new XElement("UlagAvgAmerika", skddto.UlagAvgAmerika),
                                new XElement("AvgAmerika", skddto.AvgAmerika),
                                new XElement("UlagStodForetag", skddto.UlagStodForetag),
                                new XElement("AvdrStodForetag", skddto.AvdrStodForetag),
                                new XElement("UlagStodUtvidgat", skddto.UlagStodUtvidgat),
                                new XElement("AvdrStodUtvidgat", skddto.AvdrStodUtvidgat),
                                new XElement("SumAvgBetala", skddto.SumAvgBetala),
                                new XElement("UlagSkAvdrLon", skddto.UlagSkAvdrLon),
                                new XElement("SkAvdrLon", skddto.SkAvdrLon),
                                new XElement("UlagSkAvdrPension", skddto.UlagSkAvdrPension),
                                new XElement("SkAvdrPension", skddto.SkAvdrPension),
                                new XElement("UlagSkAvdrRanta", skddto.UlagSkAvdrRanta),
                                new XElement("SkAvdrRanta", skddto.SkAvdrRanta),
                                new XElement("UlagSumSkAvdr", skddto.UlagSumSkAvdr),
                                new XElement("SumSkAvdr", skddto.SumSkAvdr),
                                new XElement("SjukLonKostnEhs", skddto.SjukLonKostnEhs),
                                new XElement("TextUpplysningAg", skddto.TextUpplysningAg));

            if (addReportSpecific)
            {
                element.Add(new XElement("SumAvg", skddto.SumAvg));
                element.Add(new XElement("UlagUngdom", skddto.UlagUngdom));
                element.Add(new XElement("AvgUngdom", skddto.AvgUngdom));
                element.Add(new XElement("AvgAlderspLessThanLimit", skddto.AvgAlderspLessThanLimit));
                element.Add(new XElement("AvgUngdomLessThanLimit", skddto.AvgUngdomLessThanLimit));
                element.Add(new XElement("AvgHelLessThanLimit", skddto.AvgHelLessThanLimit));
                element.Add(new XElement("AvgAlderspFromPreviousPeriods", skddto.AvgAlderspFromPreviousPeriods));
                element.Add(new XElement("AvgUngdomFromPreviousPeriods", skddto.AvgUngdomFromPreviousPeriods));
                element.Add(new XElement("AvgHelFromPreviousPeriods", skddto.AvgHelFromPreviousPeriods));
                element.Add(new XElement("UlagAvgAlderspLessThanLimit", skddto.UlagAvgAlderspLessThanLimit));
                element.Add(new XElement("UlagUngdomLessThanLimit", skddto.UlagUngdomLessThanLimit));
                element.Add(new XElement("UlagAvgHelLessThanLimit", skddto.UlagAvgHelLessThanLimit));
                element.Add(new XElement("UlagAvgAlderspFromPreviousPeriods", skddto.UlagAvgAlderspFromPreviousPeriods));
                element.Add(new XElement("UlagUngdomFromPreviousPeriods", skddto.UlagUngdomFromPreviousPeriods));
                element.Add(new XElement("UlagAvgHelFromPreviousPeriods", skddto.UlagAvgHelFromPreviousPeriods));
                element.Add(new XElement("TimePeriodName", skddto.Transactions.IsNullOrEmpty() ? "" : skddto.Transactions.First().TimePeriodName));
            }

            return element;
        }

        #endregion

        #region Private methods

        private List<SKDEmployeeTransactionDTO> MergeSKDEmployeeTransactionDTOs(List<SKDEmployeeTransactionDTO> sKDEmployeeTransactionDTOs)
        {
            List<SKDEmployeeTransactionDTO> dtos = new List<SKDEmployeeTransactionDTO>();

            foreach (var transactionsByEmployee in sKDEmployeeTransactionDTOs.GroupBy(i => i.EmployeeNr))
            {
                SKDEmployeeTransactionDTO firstByEmployee = transactionsByEmployee.FirstOrDefault();
                if (firstByEmployee == null)
                    continue;

                foreach (var transactionsByProduct in transactionsByEmployee.GroupBy(i => i.PayrollProductNumber + "-" + i.Type + "-" + i.TimePeriodName))
                {
                    SKDEmployeeTransactionDTO firstByProduct = transactionsByProduct.FirstOrDefault();
                    if (firstByProduct == null)
                        continue;

                    SKDEmployeeTransactionDTO dto = new SKDEmployeeTransactionDTO()
                    {
                        EmployeeNr = firstByEmployee.EmployeeNr,
                        EmployeeName = firstByEmployee.EmployeeName,
                        PayrollProductNumber = firstByProduct.PayrollProductNumber,
                        PayrollProductName = firstByProduct.PayrollProductName,
                        TimePeriodName = firstByProduct.TimePeriodName,
                        Quantity = transactionsByProduct.Sum(pay => pay.Quantity),
                        Amount = transactionsByProduct.Sum(pay => pay.Amount),
                        Type = firstByProduct.Type,
                        SysPayrollTypeLevel1 = firstByProduct.SysPayrollTypeLevel1,
                        SysPayrollTypeLevel2 = firstByProduct.SysPayrollTypeLevel2,
                        SysPayrollTypeLevel3 = firstByProduct.SysPayrollTypeLevel3,
                        SysPayrollTypeLevel4 = firstByProduct.SysPayrollTypeLevel4,
                        SysPayrollTypeLevel1Name = firstByProduct.SysPayrollTypeLevel1Name,
                        SysPayrollTypeLevel2Name = firstByProduct.SysPayrollTypeLevel2Name,
                        SysPayrollTypeLevel3Name = firstByProduct.SysPayrollTypeLevel3Name,
                        SysPayrollTypeLevel4Name = firstByProduct.SysPayrollTypeLevel4Name,
                    };

                    dtos.Add(dto);
                }
            }

            return dtos;
        }

        private SKDEmployeeTransactionDTO ConvertToSKDEmployeeTransaction(TimePayrollStatisticsSmallDTO timePayrollStatisticsSmallDTO, string type)
        {
            SKDEmployeeTransactionDTO skdEmployeeTransactionDTO = new SKDEmployeeTransactionDTO()
            {
                EmployeeNr = timePayrollStatisticsSmallDTO.EmployeeNr,
                EmployeeName = timePayrollStatisticsSmallDTO.EmployeeName,
                PayrollProductNumber = timePayrollStatisticsSmallDTO.PayrollProductNumber,
                PayrollProductName = timePayrollStatisticsSmallDTO.PayrollProductName,
                TimePeriodName = timePayrollStatisticsSmallDTO.TimePeriodName,
                Type = type,
                Quantity = timePayrollStatisticsSmallDTO.Quantity,
                Amount = timePayrollStatisticsSmallDTO.Amount,
                SysPayrollTypeLevel1 = timePayrollStatisticsSmallDTO.SysPayrollTypeLevel1,
                SysPayrollTypeLevel2 = timePayrollStatisticsSmallDTO.SysPayrollTypeLevel2,
                SysPayrollTypeLevel3 = timePayrollStatisticsSmallDTO.SysPayrollTypeLevel3,
                SysPayrollTypeLevel4 = timePayrollStatisticsSmallDTO.SysPayrollTypeLevel4,
                SysPayrollTypeLevel1Name = timePayrollStatisticsSmallDTO.SysPayrollTypeLevel1Name,
                SysPayrollTypeLevel2Name = timePayrollStatisticsSmallDTO.SysPayrollTypeLevel2Name,
                SysPayrollTypeLevel3Name = timePayrollStatisticsSmallDTO.SysPayrollTypeLevel3Name,
                SysPayrollTypeLevel4Name = timePayrollStatisticsSmallDTO.SysPayrollTypeLevel4Name,
            };

            return skdEmployeeTransactionDTO;
        }

        #endregion
    }
}
