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
using System.Text;

namespace SoftOne.Soe.Business.Util.ExportFiles
{
    public class PayrollAccountSIE : ExportFilesBase
    {
        #region Ctor

        public PayrollAccountSIE(ParameterObject parameterObject, CreateReportResult reportResult) : base(parameterObject, reportResult) 
        {
            this.reportResult = reportResult;
        }

        #endregion

        #region Public methods

        public string CreatePayrollAccountSIEFile(CompEntities entities)
        {
            #region Init repository

            PersonalDataEmployeeReportRepository personalDataRepository = new PersonalDataEmployeeReportRepository(parameterObject, reportResult);

            #endregion

            #region Prereq

            if (!TryGetIdsFromSelection(reportResult, out List<int> selectionTimePeriodIds, "periods"))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionTimePeriodIds, out _, out List<int> selectionEmployeeIds))
                return null;

            TryGetDatesFromSelection(reportResult, out _, out DateTime selectionDateTo);
            TryGetBoolFromSelection(reportResult, out bool skipQuantity, "skipQuantity");
            TryGetBoolFromSelection(reportResult, out bool netted, "voucherNetted");

            var timeperiod = TimePeriodManager.GetTimePeriod(entities, selectionTimePeriodIds.FirstOrDefault(), reportResult.ActorCompanyId);
            var voucherHeadDtos = TimeTransactionManager.GetTimePayrollVoucherHeadDTOs_new(entities, reportResult.ActorCompanyId, selectionEmployeeIds, selectionTimePeriodIds, skipQuantity, !reportResult.GetDetailedInformation, excludeAccountingExport: true);

            DateTime paymentDate = selectionDateTo;
            TimePeriod period = TimePeriodManager.GetTimePeriod(entities, selectionTimePeriodIds.Last(), reportResult.ActorCompanyId);
            if (period != null)
                paymentDate = period.PaymentDate.Value;

            #endregion

            #region Create file

            string accountingYear = GetYearMonthDay(CalendarUtility.GetBeginningOfYear(timeperiod.PaymentDate)) + " " + GetYearMonthDay(CalendarUtility.GetEndOfYear(timeperiod.PaymentDate));
            string date = GetYearMonthDay(paymentDate);

            var file = CreateFile(voucherHeadDtos, date, paymentDate, selectionDateTo, accountingYear, skipQuantity, netted);

            #endregion

            #region Close repository

            personalDataRepository.GenerateLogs();

            #endregion

            return file;
        }

        public string CreatePayrollVacationAccountSIEFile(CompEntities entities)
        {
            #region Init

            PersonalDataEmployeeReportRepository personalDataRepository = new PersonalDataEmployeeReportRepository(parameterObject, reportResult);

            #endregion

            #region Prereq

            if (!TryGetDateFromSelection(reportResult, out DateTime selectionDate, "date"))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDate.AddDays(-60), selectionDate.AddDays(60), out List<Employee> employees, out List<int> selectionEmployeeIds))
                return null;

            if (employees == null)
                employees = EmployeeManager.GetAllEmployeesByIds(reportResult.Input.ActorCompanyId, selectionEmployeeIds, loadEmployment: true);

            TryGetBoolFromSelection(reportResult, out bool skipQuantity, "skipQuantity");

            #endregion

            #region Create file

            string accountingYear = GetYearMonthDay(CalendarUtility.GetBeginningOfYear(selectionDate)) + " " + GetYearMonthDay(CalendarUtility.GetEndOfYear(selectionDate));
            string date = GetYearMonthDay(selectionDate);
            var voucherHead = PayrollManager.GetEmployeeCalculateVacationResultHeadVoucher(entities, reportResult.ActorCompanyId, selectionDate, employees, skipQuantity);
            
            var file = CreateFile(voucherHead.ObjToList(), date, selectionDate, selectionDate, accountingYear, skipQuantity: skipQuantity);

            #endregion

            #region Close repository

            personalDataRepository.GenerateLogs();

            #endregion

            return file;
        }

        private string CreateFile(List<PayrollVoucherHeadDTO> voucherHeadDtos, string date, DateTime paymentDate, DateTime selectionDateTo, string accountingYear, bool skipQuantity, bool netted = false)
        {
            string delimiterStart = "\"";
            string delimiterStop = "\" ";

            //Get Dims in order to get the correct SIE-Dimension
            List<AccountDim> internalDims = AccountManager.GetAccountDimsByCompany(reportResult.ActorCompanyId, false, true, true);
            internalDims = internalDims.Where(d => d.SysSieDimNr != null).ToList();

            string SIEdim1 = internalDims.Any(d => d.SysSieDimNr == 1) ? "#DIM 1 \"Kostnadsställe\"" : string.Empty;
            string SIEdim2 = internalDims.Any(d => d.SysSieDimNr == 2) ? "#DIM 2 \"Kostnadsbärare\" 1" : string.Empty;
            string SIEdim6 = internalDims.Any(d => d.SysSieDimNr == 6) ? "#DIM 6 \"Projekt\"" : string.Empty;
            string SIEdim7 = internalDims.Any(d => d.SysSieDimNr == 7) ? "#DIM 7 \"Anställd\"" : string.Empty;
            string SIEdim8 = internalDims.Any(d => d.SysSieDimNr == 8) ? "#DIM 8 \"Kund\"" : string.Empty;
            string SIEdim9 = internalDims.Any(d => d.SysSieDimNr == 9) ? "#DIM 9 \"Leverantör\"" : string.Empty;
            string SIEdim10 = internalDims.Any(d => d.SysSieDimNr == 10) ? "#DIM 10 \"Faktura\"" : string.Empty;

            string fileName = IOUtil.FileNameSafe(Company.Name + " SIE transaktioner " + GetYearMonthDay(selectionDateTo) + " - " + GetYearMonthDay(selectionDateTo));
            DirectoryInfo directory = Directory.CreateDirectory(ConfigSettings.SOE_SERVER_DIR_TEMP_REPORT_PHYSICAL + Guid.NewGuid().ToString());
            string filePath = StringUtility.GetValidFilePath(directory.FullName) + fileName + ".si";
            string nl = Environment.NewLine;

            try
            {
                var sb = new StringBuilder();

                string tab = "\t";

                sb.Append("#FLAGGA 0" + nl);
                sb.Append("#PROGRAM SoftOne" + nl);
                sb.Append("#FORMAT PC8" + nl);                  // Mandatory, filetype
                sb.Append("#GEN " + date + nl);  // Mandatory parameter, when and by whom this was generated
                sb.Append("#SIETYP 4" + nl);
                sb.Append("#FNAMN" + tab + delimiterStart + Company.Name + delimiterStart + nl);
                sb.Append("#RAR 0 " + accountingYear + nl);

                if (!string.IsNullOrEmpty(SIEdim1))
                    sb.Append(SIEdim1 + nl);
                if (!string.IsNullOrEmpty(SIEdim2))
                    sb.Append(SIEdim2 + nl);
                if (!string.IsNullOrEmpty(SIEdim6))
                    sb.Append(SIEdim6 + nl);
                if (!string.IsNullOrEmpty(SIEdim7))
                    sb.Append(SIEdim7 + nl);
                if (!string.IsNullOrEmpty(SIEdim8))
                    sb.Append(SIEdim8 + nl);
                if (!string.IsNullOrEmpty(SIEdim9))
                    sb.Append(SIEdim9 + nl);
                if (!string.IsNullOrEmpty(SIEdim10))
                    sb.Append(SIEdim10 + nl);

                var rows = new List<PayrollVoucherRowDTO>();

                foreach (var item in voucherHeadDtos)
                {
                    foreach (var row in item.Rows)
                    {
                        rows.Add(row);
                    }
                }

                //#VER serie vernr verdatum vertext regdatum sign
                sb.Append("#VER" + tab + delimiterStart + delimiterStop + tab + delimiterStart + delimiterStop + date + " " + delimiterStart + "Export från SoftOne Lön: " + GetYearMonthDay(paymentDate) + delimiterStop + nl);
                sb.Append("{" + nl);

                foreach (var rowGroup in rows.GroupBy(g => GroupOnDims(g, netted)))
                {
                    var row = rowGroup.FirstOrDefault();
                    var amount = rowGroup.Sum(s => s.Amount);

                    if (amount == 0)
                        continue;

                    string internalAccountString = "{}";

                    if (internalDims.Any())
                    {
                        //1 = Kostnadsställe / resultatenhet.
                        //2 = Kostnadsbärare (skall vara underdimension till 1).
                        //6 = Projekt.
                        //7 = Anställd.
                        //8 = Kund.
                        //9 = Leverantör.
                        //10 = Faktura.

                        string sie1 = string.Empty;
                        string sie2 = string.Empty;
                        string sie6 = string.Empty;
                        string sie7 = string.Empty;
                        string sie8 = string.Empty;
                        string sie9 = string.Empty;
                        string sie10 = string.Empty;

                        switch (row.Dim2SIENr)
                        {
                            case 1: sie1 = delimiterStart + "1" + delimiterStop + " " + delimiterStart + row.Dim2Nr + delimiterStop + " "; break;
                            case 2: sie2 = delimiterStart + "2" + delimiterStop + " " + delimiterStart + row.Dim2Nr + delimiterStop + " "; break;
                            case 6: sie6 = delimiterStart + "6" + delimiterStop + " " + delimiterStart + row.Dim2Nr + delimiterStop + " "; break;
                            case 7: sie7 = delimiterStart + "7" + delimiterStop + " " + delimiterStart + row.Dim2Nr + delimiterStop + " "; break;
                            case 8: sie8 = delimiterStart + "8" + delimiterStop + " " + delimiterStart + row.Dim2Nr + delimiterStop + " "; break;
                            case 9: sie9 = delimiterStart + "9" + delimiterStop + " " + delimiterStart + row.Dim2Nr + delimiterStop + " "; break;
                            case 10: sie10 = delimiterStart + "10" + delimiterStop + " " + delimiterStart + row.Dim2Nr + delimiterStop + " "; break;
                        }

                        switch (row.Dim3SIENr)
                        {
                            case 1: sie1 = delimiterStart + "1" + delimiterStop + " " + delimiterStart + row.Dim3Nr + delimiterStop + " "; break;
                            case 2: sie2 = delimiterStart + "2" + delimiterStop + " " + delimiterStart + row.Dim3Nr + delimiterStop + " "; break;
                            case 6: sie6 = delimiterStart + "6" + delimiterStop + " " + delimiterStart + row.Dim3Nr + delimiterStop + " "; break;
                            case 7: sie7 = delimiterStart + "7" + delimiterStop + " " + delimiterStart + row.Dim3Nr + delimiterStop + " "; break;
                            case 8: sie8 = delimiterStart + "8" + delimiterStop + " " + delimiterStart + row.Dim3Nr + delimiterStop + " "; break;
                            case 9: sie9 = delimiterStart + "9" + delimiterStop + " " + delimiterStart + row.Dim3Nr + delimiterStop + " "; break;
                            case 10: sie10 = delimiterStart + "10" + delimiterStop + " " + delimiterStart + row.Dim3Nr + delimiterStop + " "; break;
                        }

                        switch (row.Dim4SIENr)
                        {
                            case 1: sie1 = delimiterStart + "1" + delimiterStop + " " + delimiterStart + row.Dim4Nr + delimiterStop + " "; break;
                            case 2: sie2 = delimiterStart + "2" + delimiterStop + " " + delimiterStart + row.Dim4Nr + delimiterStop + " "; break;
                            case 6: sie6 = delimiterStart + "6" + delimiterStop + " " + delimiterStart + row.Dim4Nr + delimiterStop + " "; break;
                            case 7: sie7 = delimiterStart + "7" + delimiterStop + " " + delimiterStart + row.Dim4Nr + delimiterStop + " "; break;
                            case 8: sie8 = delimiterStart + "8" + delimiterStop + " " + delimiterStart + row.Dim4Nr + delimiterStop + " "; break;
                            case 9: sie9 = delimiterStart + "9" + delimiterStop + " " + delimiterStart + row.Dim4Nr + delimiterStop + " "; break;
                            case 10: sie10 = delimiterStart + "10" + delimiterStop + " " + delimiterStart + row.Dim4Nr + delimiterStop + " "; break;
                        }

                        switch (row.Dim5SIENr)
                        {
                            case 1: sie1 = delimiterStart + "1" + delimiterStop + " " + delimiterStart + row.Dim5Nr + delimiterStop + " "; break;
                            case 2: sie2 = delimiterStart + "2" + delimiterStop + " " + delimiterStart + row.Dim5Nr + delimiterStop + " "; break;
                            case 6: sie6 = delimiterStart + "6" + delimiterStop + " " + delimiterStart + row.Dim5Nr + delimiterStop + " "; break;
                            case 7: sie7 = delimiterStart + "7" + delimiterStop + " " + delimiterStart + row.Dim5Nr + delimiterStop + " "; break;
                            case 8: sie8 = delimiterStart + "8" + delimiterStop + " " + delimiterStart + row.Dim5Nr + delimiterStop + " "; break;
                            case 9: sie9 = delimiterStart + "9" + delimiterStop + " " + delimiterStart + row.Dim5Nr + delimiterStop + " "; break;
                            case 10: sie10 = delimiterStart + "10" + delimiterStop + " " + delimiterStart + row.Dim5Nr + delimiterStop + " "; break;
                        }

                        switch (row.Dim6SIENr)
                        {
                            case 1: sie1 = delimiterStart + "1" + delimiterStop + " " + delimiterStart + row.Dim6Nr + delimiterStop + " "; break;
                            case 2: sie2 = delimiterStart + "2" + delimiterStop + " " + delimiterStart + row.Dim6Nr + delimiterStop + " "; break;
                            case 6: sie6 = delimiterStart + "6" + delimiterStop + " " + delimiterStart + row.Dim6Nr + delimiterStop + " "; break;
                            case 7: sie7 = delimiterStart + "7" + delimiterStop + " " + delimiterStart + row.Dim6Nr + delimiterStop + " "; break;
                            case 8: sie8 = delimiterStart + "8" + delimiterStop + " " + delimiterStart + row.Dim6Nr + delimiterStop + " "; break;
                            case 9: sie9 = delimiterStart + "9" + delimiterStop + " " + delimiterStart + row.Dim6Nr + delimiterStop + " "; break;
                            case 10: sie10 = delimiterStart + "10" + delimiterStop + " " + delimiterStart + row.Dim6Nr + delimiterStop + " "; break;
                        }

                        internalAccountString = '{' + ((!string.IsNullOrEmpty(SIEdim1) ? sie1 : string.Empty) +
                                                        (!string.IsNullOrEmpty(SIEdim2) ? sie2 : string.Empty) +
                                                        (!string.IsNullOrEmpty(SIEdim6) ? sie6 : string.Empty) +
                                                        (!string.IsNullOrEmpty(SIEdim7) ? sie7 : string.Empty) +
                                                        (!string.IsNullOrEmpty(SIEdim8) ? sie8 : string.Empty) +
                                                        (!string.IsNullOrEmpty(SIEdim9) ? sie9 : string.Empty) +
                                                        (!string.IsNullOrEmpty(SIEdim10) ? sie10 : string.Empty)).TrimEnd() + "}";

                        //#TRANS kontonr {objektlista} belopp transdat transtext kvantitet sign

                    }

                    sb.Append(tab +
                        "#TRANS" +
                        tab +
                        row.Dim1Nr +
                        tab +
                        internalAccountString.PadRight(20, ' ') +
                        tab +
                        amount.ToString().PadLeft(15, ' ').Replace(",", ".") +
                        tab +
                        (!string.IsNullOrEmpty(row.Text) ? tab + delimiterStart + row.Text + delimiterStop : string.Empty) +
                        (row.SkipQuantity || skipQuantity ? "" : GetValidQuantity(row.Quantity)) +
                        nl);

                }
                sb.Append("}");

                File.WriteAllText(filePath, sb.ToString(), Encoding.GetEncoding(437));

            }
            catch (Exception ex)
            {
                SysLogManager slm = new SysLogManager(parameterObject);
                slm.AddSysLog(ex, log4net.Core.Level.Error);
            }

            #endregion

            return filePath;
        }
        
        private string GroupOnDims(PayrollVoucherRowDTO row, bool netted = false) {
            var prefix = string.Empty;
            if (row == null)
                return string.Empty;

            if (!netted)
                prefix = row.Amount >= 0 ? "1" : "0";

            return prefix+"|"+row.Dim1Nr?.ToString() + "#" + row.Dim2Nr?.ToString() + "#" + row.Dim3Nr?.ToString() + "#" + row.Dim4Nr?.ToString() + "#" + row.Dim5Nr?.ToString() + "#" + row.Dim6Nr?.ToString();
        }
        
        private string GetValidQuantity(decimal? source)
        {
            if (!source.HasValue)
                return StringUtility.GetAsciiDoubleQoute();

            return Decimal.Round(source.Value, 6).ToString().Replace(',', '.');
        }
    }
}
