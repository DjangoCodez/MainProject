using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Business.Util.SoftOne_Stage;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Linq;

namespace SoftOne.Soe.ScheduledJobs
{
    public class SoftOneStageSyncJob : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            try
            {
                #region Init
                // Check out scheduled job
                base.Init(scheduledJob, batchNr);

                ActionResult result = CheckOutScheduledJob();

                // Execute job
                CreateLogEntry(ScheduledJobLogLevel.Information, "Startar SoftOneStageSyncJob");

                CompanyManager cm = new CompanyManager(parameterObject);
                SettingManager sm = new SettingManager(parameterObject);
                SoftOneStageUtil ssu = new SoftOneStageUtil();

                // Get mandatory parameters
                int? paramCompanyId = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "actorcompanyid").Select(s => s.IntData).FirstOrDefault();
                int? paramCustomerId = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "customerid").Select(s => s.IntData).FirstOrDefault();
                string paramCountryName = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "countryname").Select(s => s.StrData).FirstOrDefault();
                int? paramDaysBack = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "daysback").Select(s => s.IntData)?.FirstOrDefault();


                CreateLogEntry(ScheduledJobLogLevel.Information, "paramCompanyId = " + (paramCompanyId.HasValue ? paramCompanyId.Value.ToString() : ""));
                CreateLogEntry(ScheduledJobLogLevel.Information, "customerid = " + (paramCustomerId.HasValue ? paramCustomerId.Value.ToString() : ""));
                CreateLogEntry(ScheduledJobLogLevel.Information, "countryname = " + (!string.IsNullOrEmpty(paramCountryName) ? paramCountryName.ToString() : "Sweden"));
                if (paramCompanyId == null)
                    return;

                #endregion

                #region Prereq

                var companyIds = sm.GetCompanyIdsWithCompanyBoolSetting(CompanySettingType.StageSync, true);

                CreateLogEntry(ScheduledJobLogLevel.Information, "Startar jobb för " + companyIds.Count.ToString() + " företag");

                #endregion

                foreach (var companyId in companyIds)
                {
                    var company = cm.GetCompany(companyId, true, false, false);
                    CreateLogEntry(ScheduledJobLogLevel.Information, "Startar jobb för licens: " + company.License.LicenseNr + " företag: " + company.Name + " actorcompanyId=" + companyId.ToString());

                    var stageSyncDTO = ssu.CreatestageSyncDTO(companyId);

                    result = ssu.Sync(stageSyncDTO);

                    CreateLogEntry(ScheduledJobLogLevel.Information, "Jobb avslutat för licens: " + company.License.LicenseNr + " företag: " + company.Name + " actorcompanyId=" + companyId.ToString());

                }
            }
            catch (Exception ex)
            {
                string msg = String.Format("Fel vid exekvering av jobb: {0}. {1}", ex.Message, SoftOne.Soe.Util.Exceptions.SoeException.GetStackTrace());
                if (ex.InnerException != null)
                    msg += "\n" + ex.InnerException.Message;
                CreateLogEntry(ScheduledJobLogLevel.Error, msg);
                base.LogError(ex);

                CheckInScheduledJob(false);
            }
        }
    }

}
