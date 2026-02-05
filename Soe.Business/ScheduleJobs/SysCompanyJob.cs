using Soe.Sys.Common.DTO;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.ScheduledJobs
{
    public class SysCompanyJob : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);

            #endregion

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();
            if (result.Success)
            {
                try
                {
                    SettingManager sm = new SettingManager(parameterObject);
                    SysServiceManager ssm = new SysServiceManager(parameterObject);
                    if (sm.GetBoolSetting(SettingMainType.Application, (int)ApplicationSettingType.SyncToSysService, 0, 0, 0))
                    {
                        var sysCompDbId = ssm.GetSysCompDBId();

                        CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("sysCompDbId: '{0}'", sysCompDbId));

                        if (sysCompDbId != 0 && sysCompDbId != null)
                        {
                            CompanyManager cm = new CompanyManager(parameterObject);
                            var companies = cm.GetCompanies();

                            CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Companies: '{0}'", companies.Count));

                            List<SysCompanyDTO> sysCompanyDTOs = new List<SysCompanyDTO>();
                            foreach (var company in companies.Where(c => c.State == (int)SoeEntityState.Active))
                            {
                                var dto = ssm.CreateSysCompanyDTO(company.ActorCompanyId, sysCompDbId);
                                dto.ModifiedBy = "CompSyncJob";
                                sysCompanyDTOs.Add(dto);
                            }

                            CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("SysCompanyDTOs: '{0}'", sysCompanyDTOs.Count));

                            ActionResult saveResult = ssm.SaveSysCompanies(sysCompanyDTOs);

                            if (!saveResult.Success)
                            {
                                CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel vid exekvering av jobb: '{0}'", result.ErrorMessage));
                            }
                            {
                                CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Vid exekvering av jobb: '{0}'", result.ErrorMessage));
                            }
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
