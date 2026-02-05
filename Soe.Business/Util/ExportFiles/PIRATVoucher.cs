using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.Reporting;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SoftOne.Soe.Common.DTO;

namespace SoftOne.Soe.Business.Util.ExportFiles
{
    public class PIRATVoucher : ExportFilesBase
    {
        #region Ctor

        public PIRATVoucher(ParameterObject parameterObject, CreateReportResult ReportResult) : base(parameterObject, ReportResult) { }

        #endregion

        #region Public methods

        public string CreateFile(CompEntities entities, int? actorCompanyId = null)
        {
            #region Init

            if (ReportResult == null)
                return null;

            var cm = new CompanyManager(parameterObject);
            var sb = new StringBuilder();

            #endregion

            #region Prereq

            Company company = cm.GetCompany(actorCompanyId.HasValue ? actorCompanyId.Value : ReportResult.ActorCompanyId);
            AccountDimDTO accountDimStdDTO = AccountManager.GetAccountDimStd(company.ActorCompanyId).ToDTO();
            List<AccountStd> accounts = AccountManager.GetAccountStdsByCompany(company.ActorCompanyId, null, false, false);
            List<AccountDimDTO> accountDimInternalDTOs = AccountManager.GetAccountDimInternalsByCompany(company.ActorCompanyId).ToDTOs();
            List<AccountInternalDTO> accountInternalInIntervalDTOs = new List<AccountInternalDTO>();
            List<AccountPeriod> accountPeriods = AccountManager.GetAccountPeriods(company.ActorCompanyId);
            List<AccountPeriodDTO> accountPeriodDTOs = new List<AccountPeriodDTO>();
            List<AccountYearDTO> accountYearsDTOs = AccountManager.GetAccountYears(company.ActorCompanyId, false, false).ToDTOs().ToList();


            foreach (var period in accountPeriods)
            {
                accountPeriodDTOs.Add(period.ToDTO());
            }

            es.ActorCompanyId = company.ActorCompanyId;

            #endregion

            #region Content

            List<VoucherHeadDTO> voucherHeads = VoucherManager.GetVoucherHeadDTOsFromSelection(es, accountDimStdDTO, orderByVoucherNr: false);

            foreach(var head in voucherHeads.OrderBy(h => h.Date))
            {
                string headStr = head.Date.ToShortDateString() + ";" + head.VoucherSeriesTypeNr + ";" + head.VoucherNr + ";";
                foreach(var row in head.Rows)
                {
                    string rowStr = row.Dim1Nr + ";" + row.Dim2Nr + ";" + row.Dim3Nr + ";" + row.Amount.ToString().Replace(',','.') + ";" + row.Quantity + ";" + (row.Text != null ? (row.Text.Length > 25 ? row.Text.Substring(0, 25) : row.Text) : "") + ";";

                    sb.AppendLine(headStr + rowStr);
                }
            }

            #endregion

            #region Create File

            var directory = Directory.CreateDirectory(ConfigSettings.SOE_SERVER_DIR_TEMP_PIRATVOUCHER_PHYSICAL + Guid.NewGuid().ToString());
            string fileName = "PiratVoucher" + "_" + company.Name + "_" + DateTime.Now.ToString("yyyyMMddhhmmss");
            string filePath = StringUtility.GetValidFilePath(directory.FullName) + fileName + ".txt";

            try
            {
                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                SysLogManager slm = new SysLogManager(parameterObject);
                slm.AddSysLog(ex, log4net.Core.Level.Error);
            }

            #endregion

            // Temporary returns file content
            return sb.ToString();
            //return filePath;
        }

        #endregion
    }
}



