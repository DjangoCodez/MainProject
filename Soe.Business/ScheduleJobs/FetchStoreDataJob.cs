using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.ScheduledJobs
{
    public class FetchStoreDataJob : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr); 
            
            string actorcompanyIdsString = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "actorcompanyids").Select(s => s.StrData).FirstOrDefault();
            string apiactorcompanyIdsString = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "apiactorcompanyids").Select(s => s.StrData).FirstOrDefault();
            #endregion

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();
            if (result.Success)
            {
                try
                {
                    // Execute job

                    List<int> actorcompanyIds = new List<int>();

                    using (CompEntities entities = new CompEntities())
                    {
                        actorcompanyIds = entities.StaffingNeedsLocationGroup.Where(r => r.Company.State == (int)SoeEntityState.Active && r.State == (int)SoeEntityState.Active).Select(i => i.ActorCompanyId).Distinct().ToList();
                    }

                    var apiActorCompanyIds = actorcompanyIds;

                    try
                    {
                        if (new SysServiceManager(null).GetSysCompDBId() == 8)
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Information, "Do not run this job on Axfood, use AxfoodJob");
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        CreateLogEntry(ScheduledJobLogLevel.Error, "Axfood check failed: " + ex.ToString());
                        base.LogError(ex);
                    }

                    if (!string.IsNullOrEmpty(actorcompanyIdsString) && actorcompanyIdsString.Split(',').Length > 0)
                    {
                        actorcompanyIds = actorcompanyIdsString.Split(',').Select(s => int.Parse(s.Trim())).ToList();
                        CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("ActorCompanyIds: '{0}'", string.Join(",", actorcompanyIds)));
                    }

                    if (!string.IsNullOrEmpty(apiactorcompanyIdsString) && apiactorcompanyIdsString.Split(',').Length > 0)
                    {
                        apiActorCompanyIds = apiactorcompanyIdsString.Split(',').Select(s => int.Parse(s.Trim())).ToList();
                        CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("ApiActorCompanyIds: '{0}'", string.Join(",", apiactorcompanyIdsString)));
                    }

                    ImportExportManager iem = new ImportExportManager(parameterObject);

                    CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("FetchStoreDataJob Logg: '{0}'", "TryToSetAccountidOnStaffingneedsFrequency starts"));
                    result = iem.TryToSetAccountidOnStaffingneedsFrequency(actorcompanyIds);       
                    CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("ImportStoreDataFromSftp Logg: '{0}'", "ImportStoreDataFromApi starts " + result.ErrorMessage));
                    var fetched = new List<int>();
                    result = iem.ImportStoreDataFromFromAPI(apiActorCompanyIds, out fetched);
                    CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("TryToSetAccountidOnStaffingneedsFrequency Logg: '{0}'", "ImportStoreDataFromSftp starts " + result.ErrorMessage));
                    result = iem.ImportStoreDataFromSftp(actorcompanyIds.Where(w => !fetched.Contains(w)).ToList());
                    CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("ImportStoreDataFromApi Logg: '{0}' ImportEDWDataFromSftp starts ", result.ErrorMessage));
                    result = iem.ImportEDWDataFromSftp();
                    CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("ImportEDWDataFromSftp Logg: '{0}' RemoveDuplicatesFromStaffingNeedFrequencyTables starts", result.ErrorMessage));
                    iem.RemoveDuplicatesFromStaffingNeedFrequencyTables();
                    CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Logg: '{0}'", result.ErrorMessage));

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
