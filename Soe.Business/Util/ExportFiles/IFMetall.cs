using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.Reporting;
using SoftOne.Soe.Business.Core.Reporting.Matrix.Models;
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
using static SoftOne.Soe.Business.Util.ExportFiles.IFMetallCompanyDTO;

namespace SoftOne.Soe.Business.Util.ExportFiles
{
    public class IFMetall : ExportFilesBase
    {
        #region Ctor

        public IFMetall(ParameterObject parameterObject, CreateReportResult reportResult) : base(parameterObject, reportResult)
        {
            this.reportResult = reportResult;
        }

        #endregion

        public string CreateIFMetallFile(CompEntities entities, bool exportExcelFile = false)
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

            if (employees == null)
                employees = EmployeeManager.GetAllEmployeesByIds(entities, base.ActorCompanyId, selectionEmployeeIds, loadEmployment: true);

            employees = employees.Where(w => !w.IFWorkPlace.IsNullOrEmpty()).ToList();
            var ifMetallDTO = GetIFMetallCompanyDTO(entities, reportResult.ActorCompanyId, reportResult.RoleId, selectionTimePeriodIds, employees);
         
            #endregion

            #region Create File

            string output = !exportExcelFile ? ifMetallDTO.GetFile() : string.Empty;
            var directory = Directory.CreateDirectory(ConfigSettings.SOE_SERVER_DIR_TEMP_REPORT_PHYSICAL + Guid.NewGuid().ToString());
            string fileName = IOUtil.FileNameSafe("IFMetall" + "_" + company.Name + "_" + DateTime.Now.ToString("yyyyMMddhhmmss"));
            string filePath = StringUtility.GetValidFilePath(directory.FullName) + fileName + (exportExcelFile ? ".xlsx" : ".txt");

            try
            {
                if (exportExcelFile) 
                    ExcelMatrix.SaveExcelFile(filePath, ifMetallDTO.ConvertToMatrixResult(), "IFMetall");
                else
                    File.WriteAllText(filePath, output, Encoding.GetEncoding("ISO-8859-1"));


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

        public IFMetallCompanyDTO GetIFMetallCompanyDTO(CompEntities entities, int actorCompanyId, int roleId, List<int> selectionTimePeriodIds, List<Employee> employees)
        {
            IFMetallCompanyDTO dto = new IFMetallCompanyDTO();

            Company company = CompanyManager.GetCompany(actorCompanyId);
            List<int> unionFeeIds = PayrollManager.GetUnionFees(actorCompanyId).Where(w => w.Association == (int)TermGroup_UnionFeeAssociation.IFMetall).Select(s => s.UnionFeeId).ToList(); 
            List<TimePeriod> timePeriods = TimePeriodManager.GetTimePeriods(selectionTimePeriodIds, actorCompanyId).Where(x => x.PayrollStopDate.HasValue).ToList();
            if (!timePeriods.Any())
                return dto;

            DateTime dateFrom = timePeriods.Min(x => x.StartDate);
            DateTime dateTo = timePeriods.Max(x => x.StopDate);
         
            List<TimePayrollTransaction> alltransactions = TimeTransactionManager.GetTimePayrollTransactionsForEmployees(entities, employees.Select(s => s.EmployeeId).ToList(), dateFrom, dateTo, sysPayrollTypeLevel2: TermGroup_SysPayrollType.SE_Deduction_UnionFee);
            List<TimePayrollTransaction> transactions = alltransactions.Where(w=> w.UnionFeeId.HasValue && unionFeeIds.Contains(w.UnionFeeId.Value)).ToList();

            bool showSocialSec = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec, Permission.Readonly, roleId, actorCompanyId);

            var orgNr = company.OrgNr.Replace("-", "").Trim();
            var companyName = company.Name;

            foreach (var timePeriod in timePeriods)
            {
                var year = timePeriod.PaymentDate.Value.Year;
                var month = timePeriod.PaymentDate.Value.Month;
                var paymentDate = timePeriod.PaymentDate.Value;

                foreach (var employee in employees)
                {
                    var employeeTransactions = transactions.Where(e => e.EmployeeId == employee.EmployeeId && e.TimePeriodId.HasValue && timePeriod.TimePeriodId == e.TimePeriodId.Value).ToList();
                  
                    if (!employeeTransactions.Any())
                        continue;

                    IFMetallEmployeeDTO ifMetallEmployee = new IFMetallEmployeeDTO()
                    {
                        TimePeriodId = timePeriod.TimePeriodId,
                        Forbundsnummer = employee.IFAssociationNumber > 0 ? employee.IFAssociationNumber : 38,
                        Arbetsstallenummer = employee.IFWorkPlace,
                        Personnummer = showSocialSec ? StringUtility.SocialSecYYMMDDXXXX(employee.SocialSec) : StringUtility.SocialSecYYMMDD_Dash_Stars(employee.SocialSec),
                        Namn = employee.Name,
                        Avgift = Math.Abs(Convert.ToDecimal(employeeTransactions.Sum(t => t.Amount))),
                        Betalkod = employee.IFPaymentCode,
                        Arbetsgivarnummer = orgNr,
                        ArbetsgivarensNamn = companyName,
                        RedovisningsperiodAr = year,
                        RedovisningsperiodManad = month,
                        Loneutbetalningsdag = paymentDate,
                    };

                    dto.IFMetallEmployees.Add(ifMetallEmployee);

                }
            }
            return dto;
        }

    }
  
    public class IFMetallCompanyDTO
    {
        public IFMetallCompanyDTO()
        {
            IFMetallEmployees = new List<IFMetallEmployeeDTO>();
        }



        public List<IFMetallEmployeeDTO> IFMetallEmployees { get; set; }

        public string GetFile()
        {
            StringBuilder sb = new StringBuilder();
            foreach(var timePeriod in IFMetallEmployees.GroupBy(g => g.TimePeriodId))
            {
                foreach (var workPlace in timePeriod.GroupBy(g => g.Arbetsstallenummer))
                {
                    var first = timePeriod.FirstOrDefault(f => f.Arbetsstallenummer == workPlace.Key);
                    var rows = timePeriod.Where(f => f.Arbetsstallenummer == workPlace.Key).ToList();
                    decimal sum = rows.Sum(s => s.Avgift);

                    if (first != null)
                    {
                        sb.Append(first.GetHead() + Environment.NewLine);
                        foreach (var employee in rows)
                        {
                            sb.Append(employee.GetRow() + Environment.NewLine);
                        }
                        sb.Append(first.GetSummary(rows.Count, sum) + Environment.NewLine);
                    }
                }
            }
            return sb.ToString();
        }
        private List<MatrixDefinitionColumn> GetMatrixDefinitionColumns()
        {
            List<MatrixDefinitionColumn> matrixDefinitionColumns = new List<MatrixDefinitionColumn>
            {
                new MatrixDefinitionColumn() { Field = "Forbundsnummer", Key = Guid.NewGuid(), MatrixDataType = MatrixDataType.Integer, Title = "Forbundsnummer" },
                new MatrixDefinitionColumn() { Field = "Arbetsstallenummer", Key = Guid.NewGuid(), MatrixDataType = MatrixDataType.String, Title = "Arbetsstallenummer" },
                new MatrixDefinitionColumn() { Field = "Personnummer", Key = Guid.NewGuid(), MatrixDataType = MatrixDataType.String, Title = "Personnummer" },
                new MatrixDefinitionColumn() { Field = "Namn", Key = Guid.NewGuid(), MatrixDataType = MatrixDataType.String, Title = "Namn" },
                new MatrixDefinitionColumn() { Field = "Avgift", Key = Guid.NewGuid(), MatrixDataType = MatrixDataType.Decimal, Title = "Avgift" },
                new MatrixDefinitionColumn() { Field = "Betalkod", Key = Guid.NewGuid(), MatrixDataType = MatrixDataType.Integer, Title = "Betalkod" },
                new MatrixDefinitionColumn() { Field = "Arbetsgivarnummer", Key = Guid.NewGuid(), MatrixDataType = MatrixDataType.String, Title = "Arbetsgivarnummer" },
                new MatrixDefinitionColumn() { Field = "ArbetsgivarensNamn", Key = Guid.NewGuid(), MatrixDataType = MatrixDataType.String, Title = "ArbetsgivarensNamn" },
                new MatrixDefinitionColumn() { Field = "RedovisningsperiodAr", Key = Guid.NewGuid(), MatrixDataType = MatrixDataType.Integer, Title = "RedovisningsperiodAr" },
                new MatrixDefinitionColumn() { Field = "RedovisningsperiodManad", Key = Guid.NewGuid(), MatrixDataType = MatrixDataType.Integer, Title = "RedovisningsperiodManad" },
                new MatrixDefinitionColumn() { Field = "Loneutbetalningsdag", Key = Guid.NewGuid(), MatrixDataType = MatrixDataType.String, Title = "Loneutbetalningsdag" },
            };


            return matrixDefinitionColumns;
        }
        public MatrixResult ConvertToMatrixResult()
        {
            MatrixResult result = new MatrixResult
            {
                MatrixDefinition = new MatrixDefinition()
            };
            result.MatrixDefinition.MatrixDefinitionColumns.AddRange(GetMatrixDefinitionColumns());
            int row = 1;

            foreach (var employee in IFMetallEmployees)
            {
                result.MatrixFields.Add(new MatrixField(row, result.MatrixDefinition.MatrixDefinitionColumns.First(f => f.Field == "Forbundsnummer").Key, employee.Forbundsnummer));
                result.MatrixFields.Add(new MatrixField(row, result.MatrixDefinition.MatrixDefinitionColumns.First(f => f.Field == "Arbetsstallenummer").Key, employee.Arbetsstallenummer));
                result.MatrixFields.Add(new MatrixField(row, result.MatrixDefinition.MatrixDefinitionColumns.First(f => f.Field == "Personnummer").Key, employee.Personnummer));
                result.MatrixFields.Add(new MatrixField(row, result.MatrixDefinition.MatrixDefinitionColumns.First(f => f.Field == "Namn").Key, employee.Namn));
                result.MatrixFields.Add(new MatrixField(row, result.MatrixDefinition.MatrixDefinitionColumns.First(f => f.Field == "Avgift").Key, Math.Abs(employee.Avgift)));
                result.MatrixFields.Add(new MatrixField(row, result.MatrixDefinition.MatrixDefinitionColumns.First(f => f.Field == "Betalkod").Key, employee.Betalkod));
                result.MatrixFields.Add(new MatrixField(row, result.MatrixDefinition.MatrixDefinitionColumns.First(f => f.Field == "Arbetsgivarnummer").Key, employee.Arbetsgivarnummer));
                result.MatrixFields.Add(new MatrixField(row, result.MatrixDefinition.MatrixDefinitionColumns.First(f => f.Field == "ArbetsgivarensNamn").Key, employee.ArbetsgivarensNamn));
                result.MatrixFields.Add(new MatrixField(row, result.MatrixDefinition.MatrixDefinitionColumns.First(f => f.Field == "RedovisningsperiodAr").Key, employee.RedovisningsperiodAr));
                result.MatrixFields.Add(new MatrixField(row, result.MatrixDefinition.MatrixDefinitionColumns.First(f => f.Field == "RedovisningsperiodManad").Key, employee.RedovisningsperiodManad));
                result.MatrixFields.Add(new MatrixField(row, result.MatrixDefinition.MatrixDefinitionColumns.First(f => f.Field == "Loneutbetalningsdag").Key, employee.Loneutbetalningsdag.ToString("yyyyMMdd")));
                row++;
            }

            return result;
        }

        public class IFMetallEmployeeDTO
        {
            public int TimePeriodId { get; set; }
            public int Forbundsnummer { get; set; }
            public string Arbetsstallenummer { get; set; }
            public string Personnummer { get; set; }
            public string Namn { get; set; }
            public decimal Avgift { get; set; }
            public int Betalkod { get; set; }
            public string Arbetsgivarnummer { get; set; }
            public string ArbetsgivarensNamn { get; set; }
            public int RedovisningsperiodManad { get; set; }
            public int RedovisningsperiodAr { get; set; }
            public DateTime Loneutbetalningsdag { get; set; }
          
            //Filen innehåller tre sorters posttyper – S1, S2 och S3

            //S1 – Inledningspost, ska innehålla uppgifter om arbetsgivare, redovisningsperiod och löneutbetalningsdag.
            //S2 – Detaljposter, innehåller medlemsuppgifter.
            //S3 – Avslutningspost, innehåller arbetsgivarens totaler dvs. antal poster och summerad avgift.

            //Posterna ska komma i ordning S1, S2 …….S2, S3 för varje arbetsgivare.

            //INLEDNINGSPOST:
            //Posttyp 2 tkn(alfanum) S1(Alltid S1)
            //Förbundsnummer 2 tkn(num) 38
            //Arbetsställenummer 4 tkn(num)
            //Arbetsgivarnummer 10 tkn(num)
            //Arbetsgivarens namn 24 tkn(alfanum)
            //Redovisningsperiodstyp 1 tkn(num) 0
            //Redovisningsperiod 2 tkn(num) Vilken månad det gäller
            //Redovisningsår 2 tkn(num) vilket år det gäller
            //Löneutbetalningsdag 6 tkn(num) ÅÅMMDD
            //Filler 13 tkn(num) nollutfyllnad

            //DETALJPOST:
            //Posttyp 2 tkn(alfanum) S2(Alltid S2)
            //Förbundsnummer 2 tkn(num) 38
            //Arbetsställenummer 4 tkn(num)
            //Personnummer 10 tkn(num)
            //Namn 24 tkn(alfanum)
            //Avgift 6 tkn(num) 4 tkn för kronor 2 tkn för ören
            //Kontrollavgift 6 tkn(num) 000000
            //Betalkod 2 tkn(num)
            //Filler 10 tkn(num) nollutfyllnad

            //AVSLUTNINGSPOST:
            //Posttyp 2 tkn(alfanum) S3(Alltid S3)
            //Förbundsnummer 2 tkn(num) 38
            //Arbetsställenummer 4 tkn(num)
            //Arbetsgivarnummer 10 tkn(num)
            //Arbetsgivarens namn 24 tkn(alfanum)
            //Antalposter 6 tkn(num)
            //Summa medlemsavgifter 9 tkn(num) 7 tkn för kronor 2 tkn för ören
            //Summa Kontrollavgifter 9 tkn(num) nollutfyllnad

            //Exempel:
            //S13800015562344639MAGNETBANDS REDOVISNING 004121204250000000000000
            //S23800011234567890KARLSSON ALLAN 057035000000010000000000
            //S23800010987654321JOHANSSON EVERT 064000000000010000000000
            //S23800011122334455MARKLUND PETRONELLA 000000000000190000000000
            //S33800015562344639MAGNETBANDS REDOVISNING 000003000121035000000000
            public string GetHead()
            {
                StringBuilder sb = new StringBuilder();

                //Posttyp 2 tkn(alfanum) S1(Alltid S1)
                sb.Append("S1");

                //Förbundsnummer 2 tkn(num) 38
                sb.Append(ExportFilesHelper.FillWithZerosBeginning(2, Forbundsnummer.ToString().Truncate(2, true)));

                //Arbetsställenummer 4 tkn(num)
                sb.Append(ExportFilesHelper.FillWithZerosBeginning(4, Arbetsstallenummer.ToString().Truncate(4, true)));

                //Arbetsgivarnummer 10 tkn(num)
                sb.Append(ExportFilesHelper.FillWithZerosBeginning(10, Arbetsgivarnummer));

                //Arbetsgivarens namn 24 tkn(alfanum)
                sb.Append(ExportFilesHelper.FillWithEmptyEnd(24, ArbetsgivarensNamn.Truncate(24,true)));

                //Redovisningsperiodstyp 1 tkn(num) 0
                sb.Append("0");

                //Redovisningsperiod 2 tkn(num) Vilken månad det gäller
                sb.Append(RedovisningsperiodManad.ToString().Length == 1 ? "0" + RedovisningsperiodManad.ToString() : RedovisningsperiodManad.ToString());

                //Redovisningsår 2 tkn(num) vilket år det gäller
                sb.Append(RedovisningsperiodAr.ToString().Substring(2,2));

                //Löneutbetalningsdag 6 tkn(num) ÅÅMMDD
                sb.Append(Loneutbetalningsdag.ToString("yyMMdd"));

                //Filler 13 tkn(num) nollutfyllnad
                sb.Append(ExportFilesHelper.FillWithZerosBeginning(13, "0"));

                return sb.ToString();
            }

            public string GetRow()
            {
                StringBuilder sb = new StringBuilder();
                // Posttyp 2 tkn(alfanum) S2(Alltid S2)
                sb.Append("S2");

                //Förbundsnummer 2 tkn(num) 38
                sb.Append(ExportFilesHelper.FillWithZerosBeginning(2, Forbundsnummer.ToString().Truncate(2, true)));

                //Arbetsställenummer 4 tkn(num)
                sb.Append(ExportFilesHelper.FillWithZerosBeginning(4, Arbetsstallenummer.ToString().Truncate(4, true)));

                //Personnummer 10 tkn(num)
                sb.Append(StringUtility.SocialSecYYMMDDXXXX(Personnummer));

                //Namn 24 tkn(alfanum)
                sb.Append(ExportFilesHelper.FillWithEmptyEnd(24, Namn.Truncate(24, true)));

                //Avgift 6 tkn(num) 4 tkn för kronor 2 tkn för ören
                sb.Append(ExportFilesHelper.FillWithZerosBeginning(4, (Math.Abs(Math.Floor(Avgift)).ToString())));
                sb.Append(ExportFilesHelper.FillWithZerosBeginning(2, (Math.Abs(Math.Round((Avgift - Math.Floor(Avgift)) * 100))).ToString()));

                //Kontrollavgift 6 tkn(num) 000000
                sb.Append(ExportFilesHelper.FillWithZerosBeginning(6, "0"));

                //Betalkod 2 tkn(num)
                sb.Append(ExportFilesHelper.FillWithZerosBeginning(2, Betalkod.ToString().Truncate(2, true)));

                //Filler 10 tkn(num) nollutfyllnad
                sb.Append(ExportFilesHelper.FillWithZerosBeginning(10, "0"));

                return sb.ToString();
            }

            public string GetSummary(int length, decimal sum)
            {
                StringBuilder sb = new StringBuilder();

                //Posttyp 2 tkn(alfanum) S3(Alltid S3)
                sb.Append("S3");

                //Förbundsnummer 2 tkn(num) 38
                sb.Append(ExportFilesHelper.FillWithZerosBeginning(2, Forbundsnummer.ToString().Truncate(2, true)));

                //Arbetsställenummer 4 tkn(num)
                sb.Append(ExportFilesHelper.FillWithZerosBeginning(4, Arbetsstallenummer.ToString().Truncate(4, true)));

                //Arbetsgivarnummer 10 tkn(num)
                sb.Append(ExportFilesHelper.FillWithZerosBeginning(10, Arbetsgivarnummer));

                //Arbetsgivarens namn 24 tkn(alfanum)
                sb.Append(ExportFilesHelper.FillWithEmptyEnd(24, ArbetsgivarensNamn.Truncate(24, true)));

                //Antalposter 6 tkn(num)
                sb.Append(ExportFilesHelper.FillWithZerosBeginning(6, length.ToString()));

                //Summa medlemsavgifter 9 tkn(num) 7 tkn för kronor 2 tkn för ören
                sb.Append(ExportFilesHelper.FillWithZerosBeginning(7, Math.Abs(Math.Floor(sum)).ToString()));
                sb.Append(ExportFilesHelper.FillWithZerosBeginning(2, (Math.Abs((Math.Round(sum - Math.Floor(sum))*100))).ToString()));

                //Summa Kontrollavgifter 9 tkn(num) nollutfyllnad
                sb.Append(ExportFilesHelper.FillWithZerosBeginning(9, "0"));


                return sb.ToString();
            }
        }
    }
}
