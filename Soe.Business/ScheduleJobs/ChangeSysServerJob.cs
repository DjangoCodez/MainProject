using Soe.Sys.Common.DTO;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.ScheduledJobs
{
    public class ChangeSysServerJob : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);
            int? fromServerId = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "fromserverid").Select(s => s.IntData).FirstOrDefault();
            int? toServerId = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "toserverid").Select(s => s.IntData).FirstOrDefault();
            bool? moveBack = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "moveback").Select(s => s.BoolData).FirstOrDefault();
            int? licenseId = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "licenseid").Select(s => s.IntData).FirstOrDefault();
            UserManager um = new UserManager(null);
            #endregion

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();
            if (result.Success)
            {
                try
                {
                    if (!fromServerId.HasValue)
                    {
                        CreateLogEntry(ScheduledJobLogLevel.Error, "fromServerId is missing");
                        return;
                    }

                    if (!toServerId.HasValue)
                    {
                        CreateLogEntry(ScheduledJobLogLevel.Error, "toServerId is missing");
                        return;
                    }
                    var sysServers = new LoginManager(null).GetSysServers();
                    var licenses = new LicenseManager(null).GetLicenses();

                    foreach (var license in licenses)
                    {
                        try
                        {
                            if (licenseId.HasValue && license.LicenseId != licenseId.Value)
                                continue;

                            var companies = new CompanyManager(null).GetCompaniesByLicense(license.LicenseId);
                            var actorCompanyId = companies.FirstOrDefault().ActorCompanyId;
                            var adminUser = um.GetAdminUser(actorCompanyId);

                            if (adminUser == null)
                                adminUser = um.GetUsersByCompany(actorCompanyId, 0, scheduledJob.ExecuteUserId).FirstOrDefault();

                            ParameterObject param = null;

                            CreateLogEntry(ScheduledJobLogLevel.Information, "started actorCompanyId " + actorCompanyId.ToString());

                            if (adminUser == null)
                                CreateLogEntry(ScheduledJobLogLevel.Error, "adminUser is null" + actorCompanyId.ToString());
                            else
                                param = GetParameterObject(actorCompanyId, adminUser.UserId);

                            var licenseManager = new LicenseManager(param);
                            var changeResult = licenseManager.ChangeSysServerId(license, ConfigurationSetupUtil.GetCurrentSysCompDbId(), fromServerId.Value, toServerId.Value, moveBack ?? false, sysServers);

                            if (!changeResult.Success)
                                CreateLogEntry(ScheduledJobLogLevel.Error, "Error when changing SysServerId for license: " + license.LicenseNr + " " + changeResult.StringValue);
                            else
                                CreateLogEntry(ScheduledJobLogLevel.Information, changeResult.StringValue);
                        }
                        catch (Exception ex)
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Error, "Error when changing SysServerId for license: " + license.LicenseNr + " " + ex.ToString());
                        }
                    }

                }
                catch (Exception ex)
                {
                    result = new ActionResult(ex);
                    CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel vid exekvering av jobb: '{0}'", result.ErrorMessage));
                    base.LogError(ex);
                }

                // Check in scheduled job
                CheckInScheduledJob(result.Success);
            }
        }
    }
}
