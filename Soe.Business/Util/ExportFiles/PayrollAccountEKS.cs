using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.Reporting;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Business.Util.ExportFiles.Common;
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
    public class PayrollAccountEKS : ExportFilesBase
    {
        #region Ctor

        public PayrollAccountEKS(ParameterObject parameterObject, CreateReportResult reportResult) : base(parameterObject, reportResult)
        {
            this.reportResult = reportResult;
        }
        #endregion

        #region Public methods

        public string CreatePayrollAccountEKSFile(CompEntities entities)
        {
            #region Init

            PersonalDataEmployeeReportRepository personalDataRepository = new PersonalDataEmployeeReportRepository(parameterObject, reportResult);

            #endregion

            #region Prereq

            if (!TryGetIdsFromSelection(reportResult, out List<int> selectionTimePeriodIds, "periods"))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionTimePeriodIds, out _, out List<int> selectionEmployeeIds))
                return null;

            TryGetDatesFromSelection(reportResult, selectionTimePeriodIds, out DateTime selectionDateFrom, out DateTime selectionDateTo);

            var accountDimInternals = AccountManager.GetAccountDimInternalsByCompany(reportResult.ActorCompanyId, true);
            var voucherHeadDtos = TimeTransactionManager.GetTimePayrollVoucherHeadDTOs_new(entities, reportResult.ActorCompanyId, selectionEmployeeIds, selectionTimePeriodIds, !reportResult.GetDetailedInformation, includeAccountDimIds: accountDimInternals.Where(s => s.SysSieDimNr == (int)TermGroup_SieAccountDim.CostCentre).Select(s => s.AccountDimId).ToList());

            #endregion

            #region Create file

            StreamWriter sw = null;
            string fileName = IOUtil.FileNameSafe(Company.Name + " EKS transaktioner " + CalendarUtility.ToFileFriendlyDateTime(selectionDateFrom) + " - " + CalendarUtility.ToFileFriendlyDateTime(selectionDateTo));
            DirectoryInfo directory = Directory.CreateDirectory(ConfigSettings.SOE_SERVER_DIR_TEMP_REPORT_PHYSICAL + Guid.NewGuid().ToString());
            string filePath = StringUtility.GetValidFilePath(directory.FullName) + fileName + ".txt";

            try
            {
                FileStream file = new FileStream(filePath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read);
                sw = new StreamWriter(file, Encoding.GetEncoding(437));

                //Pos 1 - 7 Konto Vänsterställd
                //Pos 8 - 13 Bokföringsdatum(Avräkningsdatum start i perioduppsättningen)
                //Pos 14 - 76 Belopp Vänsterställd Kredit börjar med - (minus)
                //Pos 77 - 82 Kostnadsställe(Externt kod om denna finns)

                foreach (var item in voucherHeadDtos)
                {
                    foreach (var rowitem in item.Rows)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append(ExportFilesHelper.FillWithEmptyEnd(7, rowitem.Dim1Nr));
                        var rowDate = rowitem.Date ?? item.Date;
                        sb.Append(rowDate.ToString("yyMMdd"));
                        sb.Append(ExportFilesHelper.FillWithEmptyEnd(63, decimal.Round(rowitem.Amount, 2).ToString().Replace(",", ".")));
                        var costCentreAccount = rowitem.AccountInternals.FirstOrDefault(f => f.SysSieDimNr == (int)TermGroup_SieAccountDim.CostCentre);
                        sb.Append(ExportFilesHelper.FillWithEmptyEnd(7, costCentreAccount?.AccountNr ?? ""));
                        sw.WriteLine(sb);
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
