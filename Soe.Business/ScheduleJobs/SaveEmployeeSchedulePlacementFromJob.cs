using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.TimeEngine;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.ScheduledJobs
{
    public class SaveEmployeeSchedulePlacementFromJobb : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);

            CompanyManager cm = new CompanyManager(parameterObject);
            TimeScheduleManager tsm = new TimeScheduleManager(parameterObject);
            UserManager um = new UserManager(parameterObject);

            // Get parameters
            DateTime? paramStartDate = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "start").Select(s => s.DateData).FirstOrDefault();
            DateTime? paramStopDate = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "stop").Select(s => s.DateData).FirstOrDefault();
            int? paramCompanyId = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "actorcompanyid").Select(s => s.IntData).FirstOrDefault();

            #endregion

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();
            if (result.Success)
            {
                bool success = true;

                try
                {
                    // Execute job
                    CreateLogEntry(ScheduledJobLogLevel.Information, "Startar jobb för att aktivera scheman");

                    List<DateTime> dates = new List<DateTime>();
                    if (paramStartDate.HasValue || paramStopDate.HasValue)
                        dates.AddRange(CalendarUtility.GetDates(paramStartDate, paramStopDate));
                    else
                        dates.Add(DateTime.Now);

                    int nrOfSuccededHeads = 0;
                    int nrOfFailedHeads = 0;

                    List<RecalculateTimeHead> recalculateHeads = tsm.GetRecalculateTimeHeadsToProcess(SoeRecalculateTimeHeadAction.Placement, dates, paramCompanyId);
                    List<RecalculateTimeHead> hangingReCalculateHeads = tsm.GetHangingRecalculateTimeHeadsToProcess(SoeRecalculateTimeHeadAction.Placement, dates, paramCompanyId);

                    if (hangingReCalculateHeads.Any())
                    {
                        CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Hittade {0} hängande scheman", hangingReCalculateHeads.Count));
                        recalculateHeads.AddRange(hangingReCalculateHeads);
                    }

                    var recalculateHeadsByCompanyGrouping = recalculateHeads.GroupBy(r => r.ActorCompanyId).ToList();

                    CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Aktiverar scheman för {0} st företag", recalculateHeadsByCompanyGrouping.Count));
                    foreach (var recalculateHeadsByCompany in recalculateHeadsByCompanyGrouping)
                    {
                        int actorCompanyId = recalculateHeadsByCompany.Key;

                        parameterObject.SetSoeUser(um.GetSoeUser(actorCompanyId, scheduledJob.ExecuteUserId));
                        parameterObject.SetSoeCompany(cm.GetSoeCompany(actorCompanyId));
                        if (parameterObject.SoeCompany == null)
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Företag {0} hittades inte", actorCompanyId));
                            continue;
                        }

                        string companyStamp = String.Format("{0}.{1}", parameterObject.SoeCompany.ActorCompanyId, parameterObject.SoeCompany.Name);
                        CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Startar aktivering av scheman för '{0}')", companyStamp));

                        foreach (RecalculateTimeHead head in recalculateHeadsByCompany)
                        {
                            string timeStamp = head.ExcecutedStartTime.HasValue ? head.ExcecutedStartTime.Value.ToString("yyyyMMdd") : String.Empty;
                            CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Aktiverar scheman för '{0}' datum '{1}' [{2}]", companyStamp, timeStamp, head.RecalculateTimeHeadId));

                            if (hangingReCalculateHeads.Any())
                            {
                                if (hangingReCalculateHeads.Contains(head))
                                {
                                    CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Är hängande aktivering för '{0}' datum '{1}' HeadId {2}", companyStamp, timeStamp, head.RecalculateTimeHeadId));

                                    if (head.ExcecutedStartTime < DateTime.Now.AddDays(-2))
                                    {
                                        CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Hängande aktivering för '{0}' datum '{1}' HeadId {2} är för gammal, hoppar över", companyStamp, timeStamp, head.RecalculateTimeHeadId));
                                        continue;
                                    }

                                    using (CompEntities entities = new CompEntities())
                                    {
                                        var headFromDb = entities.RecalculateTimeHead.Include("RecalculateTimeRecord").FirstOrDefault(r => r.RecalculateTimeHeadId == head.RecalculateTimeHeadId);

                                        if (headFromDb != null)
                                        {
                                            if (!head.RecalculateTimeRecord.IsNullOrEmpty() && head.RecalculateTimeRecord.Any(a => a.Status == (int)TermGroup_RecalculateTimeRecordStatus.Waiting))
                                            {
                                                CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Hängande aktivering för '{0}' datum '{1}' HeadId {2} har väntande poster, hoppar över", companyStamp, timeStamp, head.RecalculateTimeHeadId));
                                                continue;
                                            }

                                            headFromDb.Status = (int)TermGroup_RecalculateTimeHeadStatus.Unprocessed;
                                            entities.SaveChanges();
                                        }
                                    }
                                }
                            }

                            TimeEngineManager tem = new TimeEngineManager(parameterObject, head.ActorCompanyId, parameterObject.SoeUser?.UserId ?? 0);
                            result = tem.SaveEmployeeSchedulePlacementFromJob(head.RecalculateTimeHeadId);
                            if (result.Success)
                            {
                                CreateLogEntry(ScheduledJobLogLevel.Success, String.Format("Aktivering av scheman klar för '{0}' datum '{1}' [{2}]", companyStamp, timeStamp, head.RecalculateTimeHeadId));
                                nrOfSuccededHeads++;
                            }
                            else
                            {
                                CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Aktivering av scheman misslyckades för '{0}' datum '{1}' [{2}]", companyStamp, timeStamp, head.RecalculateTimeHeadId));
                                nrOfFailedHeads++;
                            }
                        }

                        CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Klar med aktivering av scheman för '{0}')", companyStamp));
                    }

                    int totalBatches = nrOfSuccededHeads + nrOfFailedHeads;
                    if (nrOfFailedHeads == 0)
                        jm.CreateLogEntry(scheduledJob.SysScheduledJobId, batchNr, ScheduledJobLogLevel.Error, String.Format("Aktivering av scheman genomförd, inga fel uppstod"));
                    else
                        jm.CreateLogEntry(scheduledJob.SysScheduledJobId, batchNr, ScheduledJobLogLevel.Error, String.Format("Aktivering av scheman genomförd, {0} av {1} batcher aktiverades utan fel", nrOfSuccededHeads, totalBatches));

                    success = nrOfFailedHeads == 0;
                }
                catch (Exception ex)
                {
                    result = new ActionResult(ex);
                    base.LogError(ex);
                }

                // Check in scheduled job
                CheckInScheduledJob(success);
            }
        }
    }
}
