using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoftOne.Soe.ScheduledJobs
{
    public class CompanyScheduledJobHeadJob : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);

            CompanyManager companyManager = new CompanyManager(parameterObject);
            UserManager um = new UserManager(parameterObject);
            int? paramCompanyId = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "actorcompanyid").Select(s => s.IntData).FirstOrDefault();

            #endregion

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();

            if (result.Success)
            {
                try
                {
                    DateTime now = DateTime.Now;
                    List<int> actorCompanyIds;
                    using (CompEntities entities = new CompEntities())
                    {
                        actorCompanyIds = entities.ScheduledJobRow.Include("ScheduledJobHead").Where(w => w.NextExecutionTime < now && w.State == (int)SoeEntityState.Active && w.ScheduledJobHead.State == (int)SoeEntityState.Active).Select(s => s.ScheduledJobHead.ActorCompanyId).ToList();
                    }

                    // Execute job
                    CreateLogEntry(ScheduledJobLogLevel.Information, "Startar företagsspecifika jobb");

                    foreach (var actorCompanyId in actorCompanyIds.Distinct())
                    {
                        Company company = companyManager.GetCompany(actorCompanyId);
                        if (paramCompanyId.HasValue && actorCompanyId != paramCompanyId)
                            continue;

                        User adminUser = um.GetAdminUser(actorCompanyId);
                        if (adminUser == null)
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Error, $"adminUser null {actorCompanyId}");
                            continue;
                        }

                        Task.Run(() => RunScheduledJobForCompany(actorCompanyId, scheduledJob.SysScheduledJobId, batchNr, now));
                    }
                }
                catch (Exception ex)
                {
                    result = new ActionResult(ex);
                    base.LogError(ex);
                }
            }

            // Check in scheduled job
            CheckInScheduledJob(result.Success);
        }

        private void RunScheduledJobForCompany(int actorCompanyId, int sysScheduledJobId, int batchNr, DateTime now)
        {
            CompanyManager companyManager = new CompanyManager(parameterObject);
            UserManager userManager = new UserManager(parameterObject);
            Company company = companyManager.GetCompany(actorCompanyId);
            SysScheduledJobManager jm = new SysScheduledJobManager(parameterObject);

            User adminUser = userManager.GetAdminUser(actorCompanyId);
            if (adminUser == null)
            {
                jm.CreateLogEntry(sysScheduledJobId, batchNr, ScheduledJobLogLevel.Error, $"adminUser null {actorCompanyId}");
                return;
            }

            ParameterObject param = GetParameterObject(actorCompanyId, adminUser.UserId);

            using (CompEntities entities = new CompEntities())
            {
                ScheduledJobManager scheduledJobManager = new ScheduledJobManager(param);

                foreach (var ScheduledJobHead in entities.ScheduledJobRow.Where(w => w.NextExecutionTime < now && w.ScheduledJobHead.ActorCompanyId == actorCompanyId && w.State == (int)SoeEntityState.Active && w.ScheduledJobHead.State == (int)SoeEntityState.Active).OrderBy(o => o.ScheduledJobHead.Sort).Select(s => s.ScheduledJobHead).ToList())
                {
                    var head = entities.ScheduledJobHead.Include("Children").FirstOrDefault(w => w.ScheduledJobHeadId == ScheduledJobHead.ScheduledJobHeadId);

                    jm.CreateLogEntry(sysScheduledJobId, batchNr, ScheduledJobLogLevel.Information, $"Startar jobb {head.Name} på {company.Name} ({company.ActorCompanyId})");
                    var jobResult = scheduledJobManager.RunJobsOnHead(head.ScheduledJobHeadId, company.ActorCompanyId, scheduledJobManager.GetBatchNumer(head.ScheduledJobHeadId));

                    if (!string.IsNullOrEmpty(jobResult.ErrorMessage))
                        jm.CreateLogEntry(sysScheduledJobId, batchNr, ScheduledJobLogLevel.Information, $"Felmeddelande från jobb {head.Name} på {company.Name} ({company.ActorCompanyId}) felmeddelande {jobResult.ErrorMessage}");

                    if (!string.IsNullOrEmpty(jobResult.InfoMessage))
                        jm.CreateLogEntry(sysScheduledJobId, batchNr, ScheduledJobLogLevel.Information, $"Meddelande från jobb {head.Name} på {company.Name} ({company.ActorCompanyId}) meddelande {jobResult.InfoMessage}");

                    jm.CreateLogEntry(sysScheduledJobId, batchNr, ScheduledJobLogLevel.Information, $"Klar med jobb {head.Name} på {company.Name} ({company.ActorCompanyId})");
                }
            }
        }


    }
}

