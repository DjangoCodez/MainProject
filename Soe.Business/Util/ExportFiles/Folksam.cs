using SoftOne.Soe.Business.Core.Reporting;
using SoftOne.Soe.Business.Core.Reporting.Matrix.Models;
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

namespace SoftOne.Soe.Business.Util.ExportFiles
{
    public class FolksamGTP : ExportFilesBase
    {
        #region Ctor

        public FolksamGTP(ParameterObject parameterObject, CreateReportResult reportResult) : base(parameterObject, reportResult)
        {
            this.reportResult = reportResult;
        }

        #endregion

        public string CreateFolksamGTPFile(CompEntities entities, bool exportExcelFile = false)
        {
            #region Init

            #endregion

            #region Init repository

            PersonalDataEmployeeReportRepository personalDataRepository = new PersonalDataEmployeeReportRepository(parameterObject, reportResult);

            #endregion

            #region Prereq

            var company = CompanyManager.GetCompany(reportResult.ActorCompanyId);

            if (!TryGetIdsFromSelection(reportResult, out List<int> selectionTimePeriodIds, "periods"))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionTimePeriodIds, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return null;

            TryGetBoolFromSelection(reportResult, out bool setAsFinal, "setAsFinal");

            if (employees == null)
                employees = EmployeeManager.GetAllEmployeesByIds(entities, base.ActorCompanyId, selectionEmployeeIds, loadEmployment: true);

            var folksamGTP = GetFolksamCompanyGTPDTO(entities, reportResult.ActorCompanyId, reportResult.UserId, reportResult.RoleId, selectionTimePeriodIds, employees, setAsFinal);
            List<EventHistoryDTO> eventHistories = new List<EventHistoryDTO>();

            if (setAsFinal && folksamGTP.EventHistories.Any())
            {
                eventHistories.AddRange(folksamGTP.EventHistories);
            }

            #endregion

            #region Create File

            string output = !exportExcelFile ? folksamGTP.GetFile() : string.Empty;
            var directory = Directory.CreateDirectory(ConfigSettings.SOE_SERVER_DIR_TEMP_REPORT_PHYSICAL + Guid.NewGuid().ToString());
            string fileName = IOUtil.FileNameSafe("GTP" + "_" + company.Name + "_" + DateTime.Now.ToString("yyyyMMddhhmmss"));
            string filePath = StringUtility.GetValidFilePath(directory.FullName) + fileName + (exportExcelFile ? ".xlsx" : ".txt");

            try
            {
                if (exportExcelFile)
                    ExcelMatrix.SaveExcelFile(filePath, folksamGTP.ConvertToMatrixResult(), "GTP");
                else
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

        public FolksamCompanyGTPDTO GetFolksamCompanyGTPDTO(CompEntities entities, int actorCompanyId, int userId, int roleId, List<int> selectionTimePeriodIds, List<Employee> employees, bool setAsFinal)
        {
            FolksamCompanyGTPDTO dto = new FolksamCompanyGTPDTO();

            List<TimePeriod> timePeriods = TimePeriodManager.GetTimePeriods(selectionTimePeriodIds, actorCompanyId).Where(x => x.PayrollStopDate.HasValue).ToList();
            if (!timePeriods.Any())
                return dto;

            DateTime dateFrom = timePeriods.OrderBy(x => x.PayrollStartDate).LastOrDefault().PayrollStopDate.Value;
            DateTime dateTo = CalendarUtility.GetEndOfYear(dateFrom);

            int kundnummer = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.PayrollExportFolksamCustomerNumber, reportResult.UserId, reportResult.ActorCompanyId, 0);
            var transactions = TimeTransactionManager.GetTimePayrollStatisticsSmallDTOs_new(entities, reportResult.ActorCompanyId, employees, selectionTimePeriodIds, setPensionCompany: true, ignoreAccounting: true);
            bool showSocialSec = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec, Permission.Readonly, roleId, actorCompanyId);

            foreach (var employee in employees)
            {
                if (employee.GTPExcluded)
                    continue;

                var employeeTransactions = transactions.Where(e => e.EmployeeId == employee.EmployeeId).ToList();

                List<int> skipTransactionIds = employeeTransactions.Where(p => !p.IsScheduleTransaction).Select(t => t.TransactionId).ToList();
                //employeeTransactions.AddRange(TimeTransactionManager.GetPayrollStartValuesAsTimePayrollStatisticsSmallDTOs(employee, ActorCompanyId, dateFrom.Year, skipTransactionIds: skipTransactionIds, setPensionCompany: true));

                employeeTransactions = filterTransactions(employee, employeeTransactions);

                if (!employeeTransactions.Any())
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

                var transactionsGroupOnMonth = employeeTransactions.GroupBy(t => t.GTPAgreementNumber + '#' + t.PaymentDate.Value.Year.ToString() + t.PaymentDate.Value.Month.ToString());

                foreach (var monthGroup in transactionsGroupOnMonth)
                {
                    int inkomstAr = monthGroup.FirstOrDefault().PaymentDate.Value.Year;
                    int month = monthGroup.FirstOrDefault().PaymentDate.Value.Month;
                    var gtpAgreementNumber = monthGroup.LastOrDefault().GTPAgreementNumber;

                    FolksamGTPEmployeeDTO folksamGTOEmployee = new FolksamGTPEmployeeDTO()
                    {
                        Transaktionskod = "E0203",
                        EmployeeId = employee.EmployeeId,
                        Kundnummer = kundnummer.ToString(),
                        InkomstAr = inkomstAr,
                        InkomstManad = month,
                        Avtalsnummer = gtpAgreementNumber.ToString(),
                        Personnummer = showSocialSec ? StringUtility.SocialSecYYMMDDXXXX(employee.SocialSec) : StringUtility.SocialSecYYMMDD_Dash_Stars(employee.SocialSec),
                        Periodcitat = "01",
                        Inkomst = Convert.ToInt32(monthGroup.Sum(t => t.Amount)),
                        Inkomstkalla = "01",
                        Tecken = "+",
                        Anstallningsnummer = employee.EmployeeNr,
                        Namn = employee.Name
                    };

                    folksamGTOEmployee.EventHistories.Add(new EventHistoryDTO(Company.ActorCompanyId, TermGroup_EventHistoryType.GTP_Folksam, SoeEntityType.Employee, employee.EmployeeId, userId: UserId, integerValue: folksamGTOEmployee.Inkomst, booleanValue: true, dateValue: anstalldFrom));

                    if (folksamGTOEmployee.Inkomst != 0)
                    {
                        foreach (var product in monthGroup.GroupBy(m => m.PayrollProductId))
                        {
                            FolksamGTPEmployeeTransactionDTO folksamGTPEmployeeTransactionDTO = new FolksamGTPEmployeeTransactionDTO()
                            {
                                Type = product.FirstOrDefault().SysPayrollTypeLevel1Name + "#" + product.FirstOrDefault().SysPayrollTypeLevel2Name + "#" + product.FirstOrDefault().SysPayrollTypeLevel3Name + "#" + product.FirstOrDefault().SysPayrollTypeLevel4Name,
                                PayrollProductNumber = product.FirstOrDefault().PayrollProductNumber,
                                Name = product.FirstOrDefault().PayrollProductName,
                                Quantity = product.Sum(p => p.Quantity),
                                Amount = product.Sum(p => p.Amount),
                            };

                            folksamGTOEmployee.Transactions.Add(folksamGTPEmployeeTransactionDTO);
                        }

                        dto.FolksamGTPEmployees.Add(folksamGTOEmployee);
                    }
                }
            }
            return dto;
        }

        private List<TimePayrollStatisticsSmallDTO> filterTransactions(Employee employee, List<TimePayrollStatisticsSmallDTO> transactions)
        {
            return transactions.Where(t => PayrollRulesUtil.isGTP(t.PensionCompany) && t.EmployeeId == employee.EmployeeId).ToList();
        }

    }

    public class FolksamGTPDTO
    {
        public FolksamGTPDTO()
        {
            FolksamGTPEmployees = new List<FolksamGTPEmployeeDTO>();
            EventHistories = new List<EventHistoryDTO>();
        }

        public List<EventHistoryDTO> EventHistories { get; set; }
        public List<FolksamGTPEmployeeDTO> FolksamGTPEmployees { get; set; }
    }
    public class FolksamCompanyGTPDTO
    {
        public FolksamCompanyGTPDTO()
        {
            FolksamGTPEmployees = new List<FolksamGTPEmployeeDTO>();
        }

        public List<FolksamGTPEmployeeDTO> FolksamGTPEmployees { get; set; }

        public List<EventHistoryDTO> EventHistories
        {
            get
            {
                return this.FolksamGTPEmployees.SelectMany(s => s.EventHistories).ToList();
            }
        }

        public string GetFile()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var employee in FolksamGTPEmployees)
            {
                sb.Append(employee.GetRow() + Environment.NewLine);
            }

            return sb.ToString();
        }

        private List<MatrixDefinitionColumn> GetMatrixDefinitionColumns()
        {
            List<MatrixDefinitionColumn> matrixDefinitionColumns = new List<MatrixDefinitionColumn>
            {
                new MatrixDefinitionColumn() { Field = "Anstallningsnummer", Key = Guid.NewGuid(), MatrixDataType = MatrixDataType.String, Title = "Anställningsnummer" },
                new MatrixDefinitionColumn() { Field = "Namn", Key = Guid.NewGuid(), MatrixDataType = MatrixDataType.String, Title = "Namn" },
                new MatrixDefinitionColumn() { Field = "Transaktionskod", Key = Guid.NewGuid(), MatrixDataType = MatrixDataType.String, Title = "Transaktionskod" },
                new MatrixDefinitionColumn() { Field = "Inkomstkalla", Key = Guid.NewGuid(), MatrixDataType = MatrixDataType.String, Title = "Inkomstkälla" },
                new MatrixDefinitionColumn() { Field = "Kundnummer", Key = Guid.NewGuid(), MatrixDataType = MatrixDataType.String, Title = "Kundnummer" },
                new MatrixDefinitionColumn() { Field = "Personnummer", Key = Guid.NewGuid(), MatrixDataType = MatrixDataType.String, Title = "Personnummer" },
                new MatrixDefinitionColumn() { Field = "InkomstAr", Key = Guid.NewGuid(), MatrixDataType = MatrixDataType.Integer, Title = "Inkomst år" },
                new MatrixDefinitionColumn() { Field = "InkomstManad", Key = Guid.NewGuid(), MatrixDataType = MatrixDataType.Integer, Title = "Inkomst månad" },
                new MatrixDefinitionColumn() { Field = "Periodcitat", Key = Guid.NewGuid(), MatrixDataType = MatrixDataType.String, Title = "Periodcitat" },
                new MatrixDefinitionColumn() { Field = "Inkomst", Key = Guid.NewGuid(), MatrixDataType = MatrixDataType.Integer, Title = "Inkomst" },
                new MatrixDefinitionColumn() { Field = "Tecken", Key = Guid.NewGuid(), MatrixDataType = MatrixDataType.String, Title = "Tecken" },
                new MatrixDefinitionColumn() { Field = "Avtalsnummer", Key = Guid.NewGuid(), MatrixDataType = MatrixDataType.String, Title = "Avtalsnummer" }
            };

            return matrixDefinitionColumns;

        }

        public MatrixResult ConvertToMatrixResult()
        {
            MatrixResult result = new MatrixResult();
            result.MatrixDefinition = new MatrixDefinition();
            result.MatrixDefinition.MatrixDefinitionColumns.AddRange(GetMatrixDefinitionColumns());
            int row = 1;

            foreach (var employee in FolksamGTPEmployees)
            {
                result.MatrixFields.Add(new MatrixField(row, result.MatrixDefinition.MatrixDefinitionColumns.First(f => f.Field == "Anstallningsnummer").Key, employee.Anstallningsnummer));
                result.MatrixFields.Add(new MatrixField(row, result.MatrixDefinition.MatrixDefinitionColumns.First(f => f.Field == "Namn").Key, employee.Namn));
                result.MatrixFields.Add(new MatrixField(row, result.MatrixDefinition.MatrixDefinitionColumns.First(f => f.Field == "Transaktionskod").Key, employee.Transaktionskod));
                result.MatrixFields.Add(new MatrixField(row, result.MatrixDefinition.MatrixDefinitionColumns.First(f => f.Field == "Inkomstkalla").Key, employee.Inkomstkalla));
                result.MatrixFields.Add(new MatrixField(row, result.MatrixDefinition.MatrixDefinitionColumns.First(f => f.Field == "Kundnummer").Key, employee.Kundnummer));
                result.MatrixFields.Add(new MatrixField(row, result.MatrixDefinition.MatrixDefinitionColumns.First(f => f.Field == "Personnummer").Key, employee.Personnummer));
                result.MatrixFields.Add(new MatrixField(row, result.MatrixDefinition.MatrixDefinitionColumns.First(f => f.Field == "InkomstAr").Key, employee.InkomstAr));
                result.MatrixFields.Add(new MatrixField(row, result.MatrixDefinition.MatrixDefinitionColumns.First(f => f.Field == "InkomstManad").Key, employee.InkomstManad));
                result.MatrixFields.Add(new MatrixField(row, result.MatrixDefinition.MatrixDefinitionColumns.First(f => f.Field == "Periodcitat").Key, employee.Periodcitat));
                result.MatrixFields.Add(new MatrixField(row, result.MatrixDefinition.MatrixDefinitionColumns.First(f => f.Field == "Inkomst").Key, employee.Inkomst));
                result.MatrixFields.Add(new MatrixField(row, result.MatrixDefinition.MatrixDefinitionColumns.First(f => f.Field == "Tecken").Key, employee.Tecken));
                result.MatrixFields.Add(new MatrixField(row, result.MatrixDefinition.MatrixDefinitionColumns.First(f => f.Field == "Avtalsnummer").Key, employee.Avtalsnummer));
                row++;
            }

            return result;
        }

    }
    public class FolksamGTPEmployeeDTO
    {
        public FolksamGTPEmployeeDTO()
        {
            Transactions = new List<FolksamGTPEmployeeTransactionDTO>();
            EventHistories = new List<EventHistoryDTO>();
        }

        public int EmployeeId { get; set; }
        public string Anstallningsnummer { get; set; }
        public string Namn { get; set; }
        public string Transaktionskod { get; set; }
        public string Inkomstkalla { get; set; }
        public string Kundnummer { get; set; }
        public string Personnummer { get; set; }
        public int InkomstAr { get; set; }
        public int InkomstManad { get; set; }
        public string Periodcitat { get; set; }
        public int Inkomst { get; set; }
        public string Tecken { get; set; }
        public string Avtalsnummer { get; set; }
        public List<FolksamGTPEmployeeTransactionDTO> Transactions { get; set; }
        public List<EventHistoryDTO> EventHistories { get; set; }

        public string GetRow()
        {
            //Fält                Position    Längd   Fältbeskrivning
            //Transaktionskod     1–5         5       Transaktionstyp = E0203
            //Inkomstkälla        6–7         2       Företag = 01
            //Kundnummer          8–13        6        Högerställt, nollutfyllt
            //Personnummer        14 –23      10      ÅÅMMDDNNNN
            //Inkomst år          24 –27      4       År då lönen utbetalas SSÅÅ
            //Inkomst månad       28–29       2       Månad då lönen utbetalas MM
            //Periodicitet        30–31       2       Månadsutbetalning = 01
            //Inkomst             32–40       9       Högerställt, nollutfyllt
            //Tecken              41          1       Värde är +.Enbart positiva inkomstbelopp är tillåtna.
            //Avtalsnummer        42–50       9       Högerställt, nollutfyllt

            //Exempel på en inrapporterad person:
            //E020301003456ÅÅMMDDXXXX20110201000030000+000011223

            //Transaktionskod = E0203
            //Inkomstkälla = 01
            //Kundnummer = 3456
            //Personnummer = ÅÅMMDDNNNN
            //Inkomst år = 2011
            //Inkomst månad = 02
            //Periodicitet = 01
            //Pensionsgrundande inkomst = 30000
            //Tecken = +
            //Avtalsnummer = 11223

            StringBuilder sb = new StringBuilder();
            // Innehåller uppgifter om transaktionskod, fast värde är E0203. Position 1–5
            sb.Append("E0203");

            //Innehåller uppgifter om inkomst, Företag = 01.Position 6–7
            sb.Append("01");

            //Innehåller arbetsgivarens kundnummer,kundnummer ska vara högerställt och nollutfyllt.Position 8–13
            sb.Append(ExportFilesHelper.FillWithZerosBeginning(6, Kundnummer));

            //Innehåller personnummer med formatet ÅÅMMDDNNNN.Position 14–23
            sb.Append(StringUtility.SocialSecYYMMDDXXXX(Personnummer));

            //Innehåller uppgift om år då inrapporterad lön utbetalas med format SSÅÅ.Position 24–27
            sb.Append(InkomstAr.ToString());

            //Innehåller uppgift om månad då inrapporterad            lön utbetalas med format MM. Position 28–29
            sb.Append(InkomstManad.ToString().Length == 1 ? "0" + InkomstManad.ToString() : InkomstManad.ToString());

            //Månadsutbetalning=01. Position 30–31
            sb.Append("01");

            //Innehåller uppgift om inkomst för perioden            inrapporteringen avser, denna uppgift är högerställd och nollutfylld.Nollöner får endast förekomma vid rättelse. Position 32–40
            sb.Append(ExportFilesHelper.FillWithZerosBeginning(9, Inkomst.ToString()));

            //Värde är +.Enbart positiva inkomstbelopp är tillåtna. Position 41
            sb.Append("+");

            //Högerställt, nollutfyllt. Position 42–50.
            sb.Append(ExportFilesHelper.FillWithZerosBeginning(9, Avtalsnummer));

            return sb.ToString();
        }
    }

    public class FolksamGTPEmployeeTransactionDTO
    {
        public string Type { get; set; }
        public string PayrollProductNumber { get; set; }
        public string Name { get; set; }
        public decimal Quantity { get; set; }
        public decimal Amount { get; set; }
    }

}
