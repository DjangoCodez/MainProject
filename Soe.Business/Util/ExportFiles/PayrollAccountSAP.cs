using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.Reporting;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Business.Util.ExportFiles.Common;
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

namespace SoftOne.Soe.Business.Util.ExportFiles
{
    public class PayrollAccountSAP : ExportFilesBase
    {
        #region Ctor

        public PayrollAccountSAP(ParameterObject parameterObject, CreateReportResult reportResult) : base(parameterObject, reportResult)
        {
            this.reportResult = reportResult;
        }

        #endregion

        #region Public methods

        public string CreatePayrollAccountSAPFile(CompEntities entities)
        {
            #region Init repository

            PersonalDataEmployeeReportRepository personalDataRepository = new PersonalDataEmployeeReportRepository(parameterObject, reportResult);

            #endregion

            #region Prereq

            if (!TryGetIdsFromSelection(reportResult, out List<int> selectionTimePeriodIds, "periods"))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionTimePeriodIds, out _, out List<int> selectionEmployeeIds))
                return null;

            TryGetDatesFromSelection(reportResult, selectionTimePeriodIds, out _, out DateTime selectionDateTo, out _, alwaysLoadPeriods: true);
            TryGetBoolFromSelection(reportResult, out bool skipQuantity, "skipQuantity");

            var voucherHeadDtos = TimeTransactionManager.GetTimePayrollVoucherHeadDTOs_new(entities, reportResult.ActorCompanyId, selectionEmployeeIds, selectionTimePeriodIds, skipQuantity, false);

            DateTime paymentDate = selectionDateTo;
            bool lastMonth = false;
            TimePeriod period = TimePeriodManager.GetTimePeriod(entities, selectionTimePeriodIds.Last(), reportResult.ActorCompanyId);
            if (period != null)
            {
                paymentDate = period.PaymentDate.Value;
                lastMonth = paymentDate >= selectionDateTo;
            }

            #endregion

            #region Create file

            var file = CreateFile(voucherHeadDtos,  paymentDate, selectionDateTo, lastMonth);

            #endregion

            #region Close repository

            personalDataRepository.GenerateLogs();

            #endregion

            return file;
        }

        public string CreatePayrollSAPHrlFile(CompEntities entities)
        {
            #region Init repository

            PersonalDataEmployeeReportRepository personalDataRepository = new PersonalDataEmployeeReportRepository(parameterObject, reportResult);

            #endregion

            #region Prereq

            if (!TryGetIdsFromSelection(reportResult, out List<int> selectionTimePeriodIds, "periods"))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionTimePeriodIds, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return null;

            TryGetDatesFromSelection(reportResult, selectionTimePeriodIds, out _, out DateTime selectionDateTo);

            if (employees == null)
                employees = EmployeeManager.GetAllEmployeesByIds(entities, base.ActorCompanyId, selectionEmployeeIds, loadEmployment: true);

            var transactions = TimeTransactionManager.GetTimePayrollStatisticsDTOs(entities, reportResult.Input.ActorCompanyId, employees, selectionTimePeriodIds, 1, null);
            var payrollGroups = PayrollManager.GetPayrollGroups(entities, reportResult.ActorCompanyId, true, true, loadSettings: true);
           
            DateTime paymentDate = selectionDateTo;
            bool lastMonth = false;
            TimePeriod period = TimePeriodManager.GetTimePeriod(entities, selectionTimePeriodIds.Last(), reportResult.ActorCompanyId);
            if (period != null)
            {
                paymentDate = period.PaymentDate.Value;
                lastMonth = paymentDate >= selectionDateTo;
            }

            #endregion

            #region Create file

            var file = CreateHrlFile(transactions, employees, payrollGroups, paymentDate, selectionDateTo, lastMonth);

            #endregion

            #region Close repository

            personalDataRepository.GenerateLogs();

            #endregion

            return file;
        }

        public string CreatePayrollVacationAccountSAPFile(CompEntities entities)
        {
            #region Init repository

            PersonalDataEmployeeReportRepository personalDataRepository = new PersonalDataEmployeeReportRepository(parameterObject, reportResult);

            #endregion

            #region Prereq

            if (!TryGetDateFromSelection(reportResult, out DateTime selectionDate, "date"))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDate.AddDays(-60), selectionDate.AddDays(60), out List<Employee> employees, out List<int> selectionEmployeeIds))
                return null;

            TryGetBoolFromSelection(reportResult, out bool skipQuantity, "skipQuantity");

            if (employees == null)
                employees = EmployeeManager.GetAllEmployeesByIds(reportResult.Input.ActorCompanyId, selectionEmployeeIds, loadEmployment: true);

            var voucherHead = PayrollManager.GetEmployeeCalculateVacationResultHeadVoucher(entities, reportResult.ActorCompanyId, selectionDate, employees, skipQuantity);

            #endregion

            #region Create file

            var file = CreateFile(voucherHead.ObjToList(), selectionDate, selectionDate);

            #endregion

            #region Close repository

            personalDataRepository.GenerateLogs();

            #endregion

            return file;
        }

        #endregion

        #region Help-methods

        private string CreateFile(List<PayrollVoucherHeadDTO> voucherHeadDtos, DateTime paymentDate, DateTime selectionDateTo, bool lastMonth = false)
        {
            var dateNow = DateTime.Now;
            var settings = new SAPFileSettings
            {

                IDOCTYP = "ACC_DOCUMENT04",
                MESTYP = "SalaryFiles",
                STDMES = "SF",
                SNDPOR = "SOFTONE",
                SNDPRT = "LS",

                Username = "CoopIP",
                SNDPRN = "COOP_IP",
                BUS_ACT = "RFBU",
                COMP_CODE = 1034,
                POSTING_DATE_DAY_LAST = "28",     //DAY That counts if payment is based on last month
                POSTING_DATE_DAY_THIS = "20",     //DAY That counts if payment is based on this month
                DOC_TYPE = "SL",
                REF_DOC_NO = "1",                 //(Reference Document Number): Ska innehålla ett frivilligt Löp-/serienummer.

                CURRENCY = "SEK",
            };

            string headerText = dateNow.ToString("yyyyMMdd") + "_" + Company.CompanyNr + "_" + dateNow.ToString("HHmmss");
            var head = voucherHeadDtos.FirstOrDefault();
            string fileName = IOUtil.FileNameSafe(Company.Name + "_SAP_Postings_" + CalendarUtility.ToFileFriendlyDateTime(paymentDate));
            DirectoryInfo directory = Directory.CreateDirectory(ConfigSettings.SOE_SERVER_DIR_TEMP_REPORT_PHYSICAL + Guid.NewGuid().ToString());
            string filePath = StringUtility.GetValidFilePath(directory.FullName) + fileName + ".xml";
           
            try
            {
                //Get Dims in order to get the correct SAP-Dimension
                List<AccountDim> internalDims = AccountManager.GetAccountDimsByCompany(reportResult.ActorCompanyId, false, true, true);
                var costCenterAccountDim = internalDims.FirstOrDefault(s => s.SysSieDimNr == (int)TermGroup_SieAccountDim.CostCentre);
                using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accounts = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(Company.ActorCompanyId));

                FinancialPostings postings = new FinancialPostings();

                FinancialPostingsFinancialPosting financialPosting = new FinancialPostingsFinancialPosting()
                {
                    BEGIN = 1,
                };
                postings.FinancialPosting = financialPosting;
                financialPosting.Header = new SAPHeader()
                {
                    IDOCTYP = settings.IDOCTYP,
                    MESTYP = settings.MESTYP,
                    STDMES = settings.STDMES,
                    SNDPOR = settings.SNDPOR,
                    SNDPRT = settings.SNDPRT,
                    SNDPRN = settings.SNDPRN,
                    SEGMENT = 1,
                };

                financialPosting.E1BPACHE09 = new FinancialPostingsFinancialPostingE1BPACHE09()
                {
                    BUS_ACT = settings.BUS_ACT,
                    USERNAME = settings.Username,
                    HEADER_TXT = headerText,
                    COMP_CODE = settings.COMP_CODE,
                    DOC_DATE = dateNow.ToString("yyyyMMdd"),
                    PSTNG_DATE = selectionDateTo.ToString("yyyyMM") + (lastMonth ? settings.POSTING_DATE_DAY_LAST : settings.POSTING_DATE_DAY_THIS),
                    FISC_YEAR = selectionDateTo.ToString("yyyy"),
                    FIS_PERIOD =  selectionDateTo.ToString("MM"),
                    DOC_TYPE = settings.DOC_TYPE,
                    REF_DOC_NO = settings.REF_DOC_NO,
                    SEGMENT = 1
                };

                List<FinancialPostingsFinancialPostingE1BPACGL09> glList = new List<FinancialPostingsFinancialPostingE1BPACGL09>();
                List<FinancialPostingsFinancialPostingE1BPACCR09> crList = new List<FinancialPostingsFinancialPostingE1BPACCR09>();
                int glCounter = 1;
                Dictionary<string, List<PayrollVoucherRowDTO>> dict = null;

                foreach (var rowGroup in head.Rows.GroupBy(g => GroupOnAccountAndCostCenter(g, costCenterAccountDim, accounts)))
                {
                    dict = new Dictionary<string, List<PayrollVoucherRowDTO>>();
                    foreach (var rowGroupOnTimeCode in rowGroup.GroupBy(g => GroupRowOnTimeCode(g)))
                    {
                        var vatKey = "";
                        if (rowGroupOnTimeCode.Any(a => PayrollRulesUtil.IsCompensation_Vat(a.SysPayrollTypeLevel1, a.SysPayrollTypeLevel2)))
                        {
                           if (Is6percent(rowGroupOnTimeCode.ToList()))
                                vatKey = "V3";
                            else if (Is12percent(rowGroupOnTimeCode.ToList()))
                                vatKey = "V2";
                            else if (Is25percent(rowGroupOnTimeCode.ToList()))
                                vatKey = "V1";
                        }
                        if (!dict.ContainsKey(vatKey)) 
                            dict.Add(vatKey, rowGroupOnTimeCode.ToList());
                        else
                            dict[vatKey].AddRange(rowGroupOnTimeCode);
                    }
                    
                    var first = rowGroup.First();
                    var costCenterAccountInternal = first.AccountInternals.FirstOrDefault(f => f.AccountDimId == costCenterAccountDim.AccountDimId);
                    var costCenterAccount = accounts.FirstOrDefault(f => f.AccountDimId == costCenterAccountDim.AccountDimId && f.AccountId == costCenterAccountInternal?.AccountId);
  
                    foreach (var dictRow in dict)
                    {

                        glList.Add(new FinancialPostingsFinancialPostingE1BPACGL09()
                        {
                            ITEMNO_ACC = ExportFilesHelper.FillWithZerosBeginning(10, glCounter.ToString()),
                            GL_ACCOUNT = ExportFilesHelper.FillWithZerosBeginning(10, first.Dim1Nr),
                            DOC_TYPE = settings.DOC_TYPE,
                            COMP_CODE = settings.COMP_CODE,
                            FISC_YEAR = selectionDateTo.ToString("yyyy"),
                            PSTNG_DATE = selectionDateTo.ToString("yyyyMM") + (lastMonth ? settings.POSTING_DATE_DAY_LAST : settings.POSTING_DATE_DAY_THIS),
                            ALLOC_NMBR = selectionDateTo.ToString("yyyyMM") + (lastMonth ? settings.POSTING_DATE_DAY_LAST : settings.POSTING_DATE_DAY_THIS),
                            TAX_CODE = dictRow.Key,
                            COSTCENTER = string.IsNullOrEmpty(costCenterAccount?.AccountNr) ? "" : ExportFilesHelper.FillWithZerosBeginning(10, costCenterAccount?.AccountNr ?? ""),
                            PROFIT_CTR = string.IsNullOrEmpty(costCenterAccount?.AccountNr) ? "" : ExportFilesHelper.FillWithZerosBeginning(10, costCenterAccount?.AccountNr ?? ""),
                            ORDERID = "", //TODO: DISKUTERAS om den behövs
                            SEGMENT = "1"
                        });

                        var amount = dictRow.Value.Sum(s => s.Amount);
                        var vatAmount = decimal.Zero;
                        if (dictRow.Key != "")
                            vatAmount = dictRow.Value.Where(w => PayrollRulesUtil.IsCompensation_Vat(w.SysPayrollTypeLevel1, w.SysPayrollTypeLevel2)).Sum(s => s.Amount);

                        if (vatAmount == 0)
                        {
                            crList.Add(new FinancialPostingsFinancialPostingE1BPACCR09()
                            {
                                ITEMNO_ACC = ExportFilesHelper.FillWithZerosBeginning(10, glCounter.ToString()),
                                CURRENCY = settings.CURRENCY,
                                AMT_DOCCUR = GetValidAmount(amount),
                                SEGMENT = 1
                            });
                        }
                        else
                        {
                            crList.Add(new FinancialPostingsFinancialPostingE1BPACCR09()
                            {
                                ITEMNO_ACC = ExportFilesHelper.FillWithZerosBeginning(10, glCounter.ToString()),
                                CURRENCY = settings.CURRENCY,
                                AMT_DOCCUR = GetValidAmount(vatAmount),
                                AMT_BASE = GetValidAmount(amount),
                                SEGMENT = 1
                            });
                        }
                        
                        glCounter++;
                    }
                }

                financialPosting.E1BPACGL09 = glList.ToArray();
                financialPosting.E1BPACCR09 = crList.ToArray();

                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(FinancialPostings));

                //serialize the object to a string
                StringBuilder sb = new StringBuilder();
                using (StringWriter sw = new StringWriter(sb))
                {
                    serializer.Serialize(sw, postings);
                }


                File.WriteAllText(filePath, sb.ToString(), Encoding.GetEncoding(437));
            }

            catch (Exception ex)
            {
                SysLogManager slm = new SysLogManager(parameterObject);
                slm.AddSysLog(ex, log4net.Core.Level.Error);
            }

            return filePath;
        }
       
        private string CreateHrlFile(List<TimePayrollStatisticsDTO> transactions, List<Employee> employees, List<PayrollGroup> payrollGroups, DateTime paymentDate, DateTime selectionDateTo, bool lastMonth = false)
        {
            var dateNow = DateTime.Now;
            var settings = new SAPFileSettings
            {

                IDOCTYP = "ACC_STAT_KEY_FIG01",
                MESTYP = "CATS TimeData",
                STDMES = "",
                SNDPOR = "SOFTONE",
                SNDPRT = "LS",

                Username = "CoopIP",
                SNDPRN = "COOP_IP",
                BUS_ACT = "RFBU",
                CO_AREA = "1000", 
                POSTING_DATE_DAY_LAST = "28",     //DAY That counts if payment is based on last month
                POSTING_DATE_DAY_THIS = "20",     //DAY That counts if payment is based on this month
                NO_PART_TIME = true,

            };

            string headerText = dateNow.ToString("yyyyMMdd") + "_" + Company.CompanyNr + "_" + dateNow.ToString("HHmmss");
            string fileName = IOUtil.FileNameSafe(Company.Name + "_SAP_HRL_" + CalendarUtility.ToFileFriendlyDateTime(paymentDate));
            DirectoryInfo directory = Directory.CreateDirectory(ConfigSettings.SOE_SERVER_DIR_TEMP_REPORT_PHYSICAL + Guid.NewGuid().ToString());
            string filePath = StringUtility.GetValidFilePath(directory.FullName) + fileName + ".xml";
            
            try
            {
                TimeDatas datas = new TimeDatas();
                Dictionary<int, Dictionary<DateTime, string>> employeeCodeDict = new Dictionary<int, Dictionary<DateTime, string>>();
                TimeDatasTimeData timeData = new TimeDatasTimeData()
                {
                    BEGIN = 1,
                };
                datas.TimeData = timeData;
                timeData.Header = new SAPHeader()
                {
                    IDOCTYP = settings.IDOCTYP,
                    MESTYP = settings.MESTYP,
                    STDMES = settings.STDMES,
                    SNDPOR = settings.SNDPOR,
                    SNDPRT = settings.SNDPRT,
                    SNDPRN = settings.SNDPRN,
                    SEGMENT = 1,
                };
                TimeDataE1ACCSTATKEYFIG sTatKeyFig = new TimeDataE1ACCSTATKEYFIG
                {
                    SEGMENT = 1,
                    E1BPDOCHDRP = new TimeDataE1BPDOCHDRP()
                    {
                        SEGMENT = 1,
                        CO_AREA = settings.CO_AREA,
                        DOCDATE = selectionDateTo.ToString("yyyyMM") + (lastMonth ? settings.POSTING_DATE_DAY_LAST : settings.POSTING_DATE_DAY_THIS),
                        POSTGDATE = selectionDateTo.ToString("yyyyMM") + (lastMonth ? settings.POSTING_DATE_DAY_LAST : settings.POSTING_DATE_DAY_THIS),
                        DOC_HDR_TX = headerText,
                        USERNAME = settings.Username,
                    }
                };
                timeData.E1ACC_STAT_KEY_FIG = sTatKeyFig;
                Dictionary<string, List<TimePayrollStatisticsDTO>> outputDict = new Dictionary<string, List<TimePayrollStatisticsDTO>>();
                List<TimeDataE1BPSKFITM> list = new List<TimeDataE1BPSKFITM>();

                foreach (var rowGroup in transactions.GroupBy(g => GroupOnCostCenter(g)))
                {
                    var workTimes = rowGroup.Where(t => t.IsWorkTime()).ToList();                //100
                    var overtimes = rowGroup.Where(t => t.IsOvertimeAddition()).ToList();        //110
                    var obAdditions = rowGroup.Where(t => t.IsOBAddition()).ToList();            //120
                    var additionalTimes = rowGroup.Where(t => t.IsAddedTime()).ToList();         //130

                    var absences = rowGroup.Where(t => t.IsAbsence()).ToList();
                    var vaccations = absences.Where(t => t.IsAbsenceVacation()).ToList();                                        //510
                    var sick114s = absences.Where(t => t.IsAbsenceSickDay2_14() || t.IsAbsenceSickDayQualifyingDay()).ToList();   //520
                    var sick15s = absences.Where(t => t.IsAbsenceSickDay15()).ToList();                                           //521
                    var tempParentalLeaves = absences.Where(t => t.IsAbsenceTemporaryParentalLeave()).ToList();                   //560
                    var parentalLeaves = absences.Where(t => t.IsParentalLeave()).ToList();                                       //570
                    var otherAbsences = absences.Where(t => !t.IsAbsenceVacation() &&                                             //590
                                                            !t.IsAbsenceSickDay2_14() &&
                                                            !t.IsAbsenceSickDayQualifyingDay() &&
                                                            !t.IsAbsenceSickDay15() &&
                                                            !t.IsAbsenceTemporaryParentalLeave() &&
                                                            !t.IsParentalLeave()).ToList();

                    if (workTimes.Any())
                        GetWorkCode(settings.NO_PART_TIME, "100", workTimes, employees, payrollGroups, rowGroup.Key, ref employeeCodeDict, ref outputDict);
                    if (overtimes.Any())
                        GetWorkCode(settings.NO_PART_TIME, "110", overtimes, employees, payrollGroups, rowGroup.Key, ref employeeCodeDict, ref outputDict);
                    if (obAdditions.Any())
                        GetWorkCode(settings.NO_PART_TIME, "120", obAdditions, employees, payrollGroups, rowGroup.Key, ref employeeCodeDict, ref outputDict);
                    if (additionalTimes.Any())
                        GetWorkCode(settings.NO_PART_TIME, "130", additionalTimes, employees, payrollGroups, rowGroup.Key, ref employeeCodeDict, ref outputDict);

                    if (vaccations.Any())
                        GetWorkCode(settings.NO_PART_TIME, "510", vaccations, employees, payrollGroups, rowGroup.Key, ref employeeCodeDict, ref outputDict);
                    if (sick114s.Any())
                        GetWorkCode(settings.NO_PART_TIME, "520", sick114s, employees, payrollGroups, rowGroup.Key, ref employeeCodeDict, ref outputDict);
                    if (sick15s.Any())
                        GetWorkCode(settings.NO_PART_TIME, "521", sick15s, employees, payrollGroups, rowGroup.Key, ref employeeCodeDict, ref outputDict);
                    if (tempParentalLeaves.Any())
                        GetWorkCode(settings.NO_PART_TIME, "560", tempParentalLeaves, employees, payrollGroups, rowGroup.Key, ref employeeCodeDict, ref outputDict);
                    if (parentalLeaves.Any())
                        GetWorkCode(settings.NO_PART_TIME, "570", parentalLeaves, employees, payrollGroups, rowGroup.Key, ref employeeCodeDict, ref outputDict);
                    if (otherAbsences.Any())
                        GetWorkCode(settings.NO_PART_TIME, "590", otherAbsences, employees, payrollGroups, rowGroup.Key, ref employeeCodeDict, ref outputDict);

                }
                foreach (var dictRow in outputDict)
                {
                    string[] key = dictRow.Key.Split('#');

                    var costCenter = key[0];
                    var sTATKEYFIG = key[1];
                    list.Add(new TimeDataE1BPSKFITM()
                    {
                        STATKEYFIG = sTATKEYFIG,
                        STAT_QTY = GetValidQuantity(dictRow.Value.Sum(t => t.Quantity / 60)),
                        REC_CCTR = ExportFilesHelper.FillWithZerosBeginning(10, costCenter ?? ""),
                        REC_WBS_EL = ExportFilesHelper.FillWithZerosBeginning(10, costCenter ?? ""),
                        SEGMENT = 1
                    });
                }

                sTatKeyFig.E1BPSKFITM = list.ToArray();

                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(TimeDatas));
                
                XmlWriterSettings xmlSettings = new XmlWriterSettings() { 
                    Indent = true
                };
                using (var memStm = new MemoryStream())
                using (var xw = XmlWriter.Create(memStm, xmlSettings))
                {
                    serializer.Serialize(xw, datas);
                    var utf8 = memStm.ToArray();
                    File.WriteAllBytes(filePath, utf8);
                }

            }

            catch (Exception ex)
            {
                SysLogManager slm = new SysLogManager(parameterObject);
                slm.AddSysLog(ex, log4net.Core.Level.Error);
            }

            return filePath;
        }

        private String GroupOnCostCenter( TimePayrollStatisticsDTO transaction)
        {
            if (transaction.Dim2SIENr.HasValue && transaction.Dim2SIENr.Value == (int)TermGroup_SieAccountDim.CostCentre)
                return transaction.Dim2Nr;
            else if (transaction.Dim3SIENr.HasValue && transaction.Dim3SIENr.Value == (int)TermGroup_SieAccountDim.CostCentre)
                return transaction.Dim3Nr;
            else if (transaction.Dim4SIENr.HasValue && transaction.Dim4SIENr.Value == (int)TermGroup_SieAccountDim.CostCentre)
                return transaction.Dim4Nr;
            else if (transaction.Dim5SIENr.HasValue && transaction.Dim5SIENr.Value == (int)TermGroup_SieAccountDim.CostCentre)
                return transaction.Dim5Nr;
            else if (transaction.Dim6SIENr.HasValue && transaction.Dim6SIENr.Value == (int)TermGroup_SieAccountDim.CostCentre)
                return transaction.Dim6Nr;
            else    
                return "";
        }

        private string GroupOnAccountAndCostCenter(PayrollVoucherRowDTO row, AccountDim costCenterAccountDim, List<AccountDTO> accounts)
        {
            var costCenterAccountInternal = row.AccountInternals.FirstOrDefault(f => f.AccountDimId == costCenterAccountDim.AccountDimId);
            var costCenterAccount = accounts.FirstOrDefault(f => f.AccountDimId == costCenterAccountDim.AccountDimId && f.AccountId == costCenterAccountInternal?.AccountId);

            return row.Dim1Nr.ToString() + "#" + costCenterAccount?.AccountNr ?? "";
        }
        
        private void GetWorkCode(bool noPartTime, string code, List<TimePayrollStatisticsDTO> rows, List<Employee> employees, List<PayrollGroup> payrollGroups, string costCenter, ref Dictionary<int, Dictionary<DateTime, string>> employeeCodeDict, ref Dictionary<string, List<TimePayrollStatisticsDTO>> outputDict)
        {
            foreach (TimePayrollStatisticsDTO row in rows)
            {
                var empCode = "";
                Employee employee = employees.FirstOrDefault(w => w.EmployeeId == row.EmployeeId);
                DateTime transactionDate = row.TimeBlockDate;
                if (employee == null)
                    return;

                if (employeeCodeDict.ContainsKey(employee.EmployeeId))
                    empCode = employeeCodeDict[employee.EmployeeId].FirstOrDefault(w => w.Key == transactionDate).Value ?? "";

                if (empCode == "")
                {
                    empCode = GetEmploymentTypeCode(employee, transactionDate, payrollGroups, noPartTime);

                    if (empCode != "")
                    {
                        if (!employeeCodeDict.ContainsKey(employee.EmployeeId))
                            employeeCodeDict.Add(employee.EmployeeId, new Dictionary<DateTime, string>
                                    {
                                        { transactionDate, empCode }
                                    });
                        else
                            employeeCodeDict[employee.EmployeeId].AddRange(new Dictionary<DateTime, string>
                                    {
                                        { transactionDate, empCode }
                                    });
                    }
                }
                if (empCode != "")
                {
                    string outCode = costCenter + "#" +code + empCode;

                    if (!outputDict.ContainsKey(outCode))
                        outputDict.Add(outCode, row.ObjToList());
                    else
                        outputDict[outCode].Add(row);
                }
            }

          

        }
        
        private string GetEmploymentTypeCode(Employee employee, DateTime date, List<PayrollGroup> payrollGroups, bool noPartTime = false )
        {

            string codeEmp = "";
            string codeKat = "";
            string codeKon = "";

            Employment employment = employee.GetEmployment(date);
            if (employment == null)
                return "";

            bool fullTime = noPartTime || employment.GetPercent(date) >= 100;
            var employmentType = employment.GetEmploymentType(date);
            PayrollGroup payrollGroup = employment.GetPayrollGroup(date, payrollGroups);
            var personalKategori = payrollGroup.PayrollGroupSetting?.FirstOrDefault(t => t.Type == (int)PayrollGroupSettingType.PayrollReportsPersonalCategory)?.IntData.ToString() ?? null;

            if(personalKategori == null)
                return "";
            codeKat = (personalKategori.Equals("1") || personalKategori.Equals("3") || personalKategori.Equals("4") || personalKategori.Equals("5") || personalKategori.Equals("6")) ?
                    "1" :
                    "2";

            switch (employmentType)
            {
                case 4:                                // Tillsvidareanställning
                    codeEmp = fullTime ? "1" : "2";
                    break;
                case 1:                                // Provanställning
                case 2:                                // Vikariat
                case 3:                                // Semestervikarie
                    codeEmp = fullTime ? "5" : "6";
                    break;
                default:
                    codeEmp = fullTime ? "3" : "4";
                    break;

            }
            codeKon = employee.Sex == TermGroup_Sex.Male ? "1" : "2";

            if (codeEmp == "" || codeKat == "" || codeKon == "")
                return "";

            return codeKat + "" + codeEmp + "" + codeKon;
        }

        private string GetValidQuantity(decimal? source)
        {
            if (!source.HasValue)
                return StringUtility.GetAsciiDoubleQoute();

            return Decimal.Round(source.Value, 2).ToString().Replace(',', '.');
        }
        
        private string GetValidAmount(decimal source)
        {   
            if(source >= 0)
                return $"+{Decimal.Round(source, 2).ToString().Replace(',', '.')}";
            else
               return  Decimal.Round(source, 2).ToString().Replace(',', '.');
        }
        
        private string GroupRowOnTimeCode(PayrollVoucherRowDTO row)
        {
            return row.TimeCodeTransactionId.ToString() ?? "";
        }
                
        private bool Is6percent(List<PayrollVoucherRowDTO> rows)
        {
            return rows.Any(a => a.SysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_Compensation_Vat_6Percent);
        }
        
        private bool Is12percent(List<PayrollVoucherRowDTO> rows)
        {
            return rows.Any(a => a.SysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_Compensation_Vat_12Percent);
        }
        
        private bool Is25percent(List<PayrollVoucherRowDTO> rows)
        {
            return rows.Any(a => a.SysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_Compensation_Vat_25Percent);
        }

        #endregion
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.coop.se/coopip/timedata/1.0")]

    public partial class TimeDatas
    {
        public TimeDatasTimeData TimeData { get; set; }
    }
     
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class TimeDatasTimeData
    {
        public SAPHeader Header { get; set; }
        [System.Xml.Serialization.XmlElementAttribute("E1ACC_STAT_KEY_FIG")]
        public TimeDataE1ACCSTATKEYFIG E1ACC_STAT_KEY_FIG { get; set; }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int BEGIN { get; set; }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class SAPHeader
    {
        public string IDOCTYP { get; set; }
        public string MESTYP { get; set; }
        public string STDMES { get; set; }
        public string SNDPOR { get; set; }
        public string SNDPRT { get; set; }
        public string SNDPRN { get; set; }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int SEGMENT { get; set; }
 
    }
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.coop.se/coopip/timedata/1.0")]
    public partial class TimeDataE1ACCSTATKEYFIG
    {
        [System.Xml.Serialization.XmlAttributeAttribute()] 
        public int SEGMENT { get; set; }
        [System.Xml.Serialization.XmlElementAttribute("E1BPDOCHDRP")]
        public TimeDataE1BPDOCHDRP E1BPDOCHDRP { get; set; }
        [System.Xml.Serialization.XmlElementAttribute("E1BPSKFITM")]
        public TimeDataE1BPSKFITM[] E1BPSKFITM { get; set; }
    }  
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.coop.se/coopip/timedata/1.0")]
    public partial class TimeDataE1BPDOCHDRP
    {

        public string CO_AREA { get; set; }
        public string DOCDATE { get; set; }
        public string POSTGDATE { get; set; }
        public string DOC_HDR_TX { get; set; }
        public string USERNAME { get; set; }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int SEGMENT { get; set; }
    }
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.coop.se/coopip/timedata/1.0")]
    public partial class TimeDataE1BPSKFITM
    {
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int SEGMENT { get; set; }
        public string STATKEYFIG { get; set; }
        public string STAT_QTY { get; set; }
        public string SEG_TEXT { get; set; }
        public string REC_CCTR { get; set; }
        public string REC_WBS_EL { get; set; }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.coop.se/coopip/financialpostings/1.0")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.coop.se/coopip/financialpostings/1.0", IsNullable = false)]
    public partial class FinancialPostings
    {
        public FinancialPostingsFinancialPosting FinancialPosting { get; set; }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.coop.se/coopip/financialpostings/1.0")]
    public partial class FinancialPostingsFinancialPosting
    {

        public SAPHeader Header { get; set; }

        public FinancialPostingsFinancialPostingE1BPACHE09 E1BPACHE09 { get; set; }
        [System.Xml.Serialization.XmlElementAttribute("E1BPACGL09")]
        public FinancialPostingsFinancialPostingE1BPACGL09[] E1BPACGL09 { get; set; }
        [System.Xml.Serialization.XmlElementAttribute("E1BPACCR09")]
        public FinancialPostingsFinancialPostingE1BPACCR09[] E1BPACCR09 { get; set; }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int BEGIN {  get; set; }

    }
    
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.coop.se/coopip/financialpostings/1.0")]
    public partial class FinancialPostingsFinancialPostingE1BPACHE09
    {

        public string BUS_ACT { get; set;}
        public string USERNAME { get; set;}
        public string HEADER_TXT { get; set;}
        public ushort COMP_CODE { get; set;}
        public string DOC_DATE { get; set;}
        public string PSTNG_DATE { get; set;}
        public string FISC_YEAR { get; set;}
        public string FIS_PERIOD { get; set;}
        public string DOC_TYPE { get; set;}
        public string REF_DOC_NO { get; set;}
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int SEGMENT {get; set;}
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.coop.se/coopip/financialpostings/1.0")]
    public partial class FinancialPostingsFinancialPostingE1BPACGL09
    {
        public string ITEMNO_ACC {get; set;}
        public string GL_ACCOUNT { get; set;}
        public string ITEM_TEXT { get; set;}
        public string DOC_TYPE { get; set;}
        public ushort COMP_CODE { get; set;}
        public string FIS_PERIOD { get; set;}
        public string FISC_YEAR { get; set;}
        public string PSTNG_DATE { get; set;}
        public string VALUE_DATE { get; set;}
        public string ALLOC_NMBR { get; set;}
        public string TAX_CODE { get; set;}
        public string COSTCENTER { get; set;}
        public string PROFIT_CTR { get; set;}
        public string ORDERID { get; set;}
        public string MATERIAL { get; set;}
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SEGMENT { get; set;}

    }

   

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.coop.se/coopip/financialpostings/1.0")]
    public partial class FinancialPostingsFinancialPostingE1BPACCR09
    {

        public string ITEMNO_ACC { get; set;}

        public string CURRENCY { get; set;}

        public string AMT_DOCCUR { get; set;}

        public string AMT_BASE { get; set;}
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int SEGMENT { get; set;}
    }

    public class SAPFileSettings
    {
        public string Username { get; set; }
        public string SNDPRN { get; set; }
        public string BUS_ACT { get;  set; }
        public ushort COMP_CODE { get;  set; }
        public string CO_AREA { get; set; }
        public string POSTING_DATE_DAY_LAST { get; set; }
        public string POSTING_DATE_DAY_THIS { get; set; }
        public string DOC_TYPE { get; set; }
        public string REF_DOC_NO { get; set; }
        public string CURRENCY { get; set; }
        public string IDOCTYP { get; set; }
        public string MESTYP { get; set; }
        public string STDMES { get; set; }
        public string SNDPOR { get; set; }
        public string SNDPRT { get; set; }
        public bool NO_PART_TIME { get; set; }
    }
}
