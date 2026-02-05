using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.Reporting;
using SoftOne.Soe.Business.Util.Config;
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
    public class PayrollAccountVisma : ExportFilesBase
    {
        #region Ctor

        public PayrollAccountVisma(ParameterObject parameterObject, CreateReportResult reportResult) : base(parameterObject, reportResult)
        {
            this.reportResult = reportResult;
        }
        #endregion

        #region Public methods

        public string CreatePayrollAccountVismaFile(CompEntities entities)
        {
            #region Init repository

            PersonalDataEmployeeReportRepository personalDataRepository = new PersonalDataEmployeeReportRepository(parameterObject, reportResult);

            #endregion

            #region Prereq

            if (!TryGetIdsFromSelection(reportResult, out List<int> selectionTimePeriodIds, "periods"))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionTimePeriodIds, out _, out List<int> selectionEmployeeIds))
                return null;

            TryGetDatesFromSelection(reportResult, selectionTimePeriodIds, out DateTime selectionDateFrom, out DateTime selectionDateTo);

            var voucherHeadDtos = TimeTransactionManager.GetTimePayrollVoucherHeadDTOs_new(entities, reportResult.ActorCompanyId, selectionEmployeeIds, selectionTimePeriodIds, !reportResult.GetDetailedInformation);

            #endregion

            #region Create file

            StreamWriter sw = null;
            string fileName = IOUtil.FileNameSafe(Company.Name + " Visma transaktioner " + CalendarUtility.ToFileFriendlyDateTime(selectionDateFrom) + " - " + CalendarUtility.ToFileFriendlyDateTime(selectionDateTo));
            DirectoryInfo directory = Directory.CreateDirectory(ConfigSettings.SOE_SERVER_DIR_TEMP_REPORT_PHYSICAL + Guid.NewGuid().ToString());
            string filePath = StringUtility.GetValidFilePath(directory.FullName) + fileName + ".visma";

            try
            {
                FileStream file = new FileStream(filePath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read);
                sw = new StreamWriter(file, Encoding.GetEncoding(437));

                string headerWrite;
                headerWrite = "\"1\"";  // Some value
                headerWrite += " \"\"";  // Empty? 
                headerWrite += " \"" + selectionDateFrom.ToShortDateString() + " - " + selectionDateTo.ToShortDateString() + "\"";  // ValDt
                headerWrite += " \"1\"";  // Susp
                headerWrite += " \"\"";  // Smdb 

                sw.WriteLine("@WaBnd (BndNo, SrNo, ValDt, Susp, SmDb, SmCr, )");
                sw.WriteLine(headerWrite);
                sw.WriteLine("@WaVO (BndNo, VoNo, VoDt, VoTp, Txt, DbAcNo, DbTxCd, CrAcNo, CrTxCd, R1, R2, R3, R4, R5, R6, Am, Qty)");

                foreach (var item in voucherHeadDtos)
                {
                    string ToWrite = "\"1\"" + " ";                          // BndNo
                    ToWrite += "\"" + item.VoucherNr + "\"" + " ";                               // VoNo
                    ToWrite += "\"" + GetYearMonthDay(item.Date) + "\"" + " ";      // VoDt
                    ToWrite += "\"\"" + " ";                                 // VoTp
                    ToWrite += "\"" + item.VoucherSeriesTypeName + " " + GetYearMonthDay(item.Date) + "\"" + " ";                    // Txt

                    foreach (var rowitem in item.Rows)
                    {
                        string dbTxCd = "0";
                        string zero = "0";
                        string sie2 = (rowitem.Dim2SIENr == 2) ? rowitem.Dim2Nr : "0";
                        string sie6 = (rowitem.Dim2SIENr == 6) ? rowitem.Dim2Nr : "0";
                        string sie7 = (rowitem.Dim2SIENr == 7) ? rowitem.Dim2Nr : "0";
                        string sie8 = (rowitem.Dim2SIENr == 8) ? rowitem.Dim2Nr : "0";
                        string sie9 = (rowitem.Dim2SIENr == 9) ? rowitem.Dim2Nr : "0";
                        string sie10 = (rowitem.Dim2SIENr == 10) ? rowitem.Dim2Nr : "0";
                        string ToWrite2 = "";

                        // Debit 
                        if (rowitem.Amount >= 0)
                        {
                            ToWrite2 += "\"" + rowitem.Dim1Nr + "\"" + " ";         // DbAcNo
                        }
                        else
                        {
                            ToWrite2 += "\"" + zero + "\"" + " ";                     // DbAcNo
                        }

                        ToWrite2 += "\"" + dbTxCd + "\"" + " ";         // DbTxCd

                        // Credit
                        if (rowitem.Amount < 0)
                        {
                            ToWrite2 += "\"" + rowitem.Dim1Nr + "\"" + " ";         // CrAcNo
                        }
                        else
                        {
                            ToWrite2 += "\"" + zero + "\"" + " ";                      // CrAcNo
                        }

                        ToWrite2 += "\"" + zero + "\"" + " ";       // CrTxCd
                        ToWrite2 += "\"" + sie2 + "\"" + " ";       // R1
                        ToWrite2 += "\"" + sie6 + "\"" + " ";       // R2
                        ToWrite2 += "\"" + sie7 + "\"" + " ";       // R3 
                        ToWrite2 += "\"" + sie8 + "\"" + " ";       // R4
                        ToWrite2 += "\"" + sie9 + "\"" + " ";       // R5
                        ToWrite2 += "\"" + sie10 + "\"" + " ";       // R6
                        if (rowitem.Amount >= 0)
                        {
                            ToWrite2 += "\"" + rowitem.Amount + "\"" + " ";       // Am
                        }
                        else
                        {
                            ToWrite2 += "\"" + (rowitem.Amount * -1) + "\"" + " ";       // Am
                        }

                        ToWrite2 += "\"" + rowitem.Quantity + "\"" + " ";      // Qty
                        sw.WriteLine(ToWrite + ToWrite2);
                    }
                }
            }
            catch (Exception ex)
            {
                SysLogManager slm = new SysLogManager(parameterObject);
                slm.AddSysLog(ex, log4net.Core.Level.Error);
            }
            finally
            {
                sw?.Close();
            }

            #endregion

            #region Close repository

            personalDataRepository.GenerateLogs();

            #endregion

            return filePath;
        }

        #endregion
    }
}
