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
    public class TimeStampConversionJob : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);

            CompanyManager cm = new CompanyManager(parameterObject);
            TimeStampManager tsm = new TimeStampManager(parameterObject);

            bool runCompleteJob = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "runcomplete" && s.BoolData.HasValue).Select(s => s.BoolData.Value).FirstOrDefault();

            #endregion

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();
            string startMessage = "Startar konvertering av stämplingar";

            if (result.Success)
            {
                #region Move time stamps from raw to entries

                try
                {
                    // Execute job. Timestamp does not use the raw table so this only affects timespot and webtimestamp.
                    CreateLogEntry(ScheduledJobLogLevel.Information, "Startar flytt av timestamps från raw till entries");
                    foreach (int timeStampEntryRawId in tsm.GetUnhandledTimeStampEntryRawIds())
                    {
                        result = tsm.TransferFromTimeStampEntryRawToTimeStampEntry(timeStampEntryRawId);
                        if (!result.Success)
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Flytt av timeStampEntryRawId = {0} misslyckades: {1}", timeStampEntryRawId, result.ErrorMessage));
                        }
                    }
                }
                catch (Exception ex)
                {
                    result = new ActionResult(ex);
                    base.LogError(ex);
                }

                if (!result.Success)
                {
                    #region Logging

                    switch (result.ErrorNumber)
                    {
                        default:
                            CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel vid exekvering av jobb: '{0}'", result.ErrorMessage));
                            break;
                    }

                    #endregion
                }

                #endregion

                #region Handle time stamp entries

                if (runCompleteJob)
                {
                    DateTime? lastStartOfJob = null;
                    Random random = new Random();

                    if (random.Next(1, 5) != 1)
                    {
                        lastStartOfJob = LastStartOfJob(scheduledJob.SysScheduledJobId, startMessage);

                        if (lastStartOfJob.HasValue)
                        {
                            lastStartOfJob = lastStartOfJob.Value.AddMinutes(-1);
                            CreateLogEntry(ScheduledJobLogLevel.Information, Environment.MachineName + " Kontrollerar från föregående job " + lastStartOfJob.ToString());
                        }
                    }
                    else
                    {
                        CreateLogEntry(ScheduledJobLogLevel.Information, Environment.MachineName + " Kontrollerar längre bak i tiden än senaste starten");
                    }

                    // Get all company ids with new time stamp entries
                    List<int> companyIds = tsm.GetCompanyIdsWithNewEntries(lastStartOfJob);

                    if (companyIds.Any())
                    {
                        int numberOfCompanies = companyIds.Count;

                        if (result.Success)
                        {
                            try
                            {
                                // Execute job
                                CreateLogEntry(ScheduledJobLogLevel.Information, startMessage);
                                int count = 1;
                                foreach (int actorCompanyId in companyIds)
                                {
                                    UserDTO user = parameterObject?.SoeUser;
                                    CompanyDTO company = cm.GetCompany(actorCompanyId)?.ToCompanyDTO();
                                    if (company == null)
                                        continue;

                                    TimeRuleManager trm = new TimeRuleManager(null);
                                    trm.FlushTimeRulesFromCache(actorCompanyId);
                                    var defaultRoleId = new UserManager(null).GetDefaultRoleId(actorCompanyId, user.UserId);
                                    parameterObject = ParameterObject.Create(user: user, company: company, roleId: defaultRoleId);
                                    tsm = new TimeStampManager(parameterObject);

                                    // Get company name for clearer logging
                                    CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Konverterar stämplingar för '{0}' ({1}) {2}/{3} batchnr {4}", company.Name, company.ActorCompanyId, count, numberOfCompanies, batchNr));
                                    result = tsm.RemoveDuplicateTimeStampEntries(actorCompanyId);

                                    if (result.Success && result.IntegerValue > 0)
                                        CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("RemoveDuplicateTimeStampEntries kört för '{0}' tog bort {1} stämplingar", company.Name, result.IntegerValue));

                                    if (!result.Success)
                                    {
                                        CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Error in RemoveDuplicateTimeStampEntries: {0}", result.ErrorMessage));
                                    }
                                    else if (result.IntegerValue > 0)
                                    {
                                        CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("{0} st dubletter hittade för '{1}' ({2}), detta beror troligtvis på att en anställd har stämplat dubbelt, mer info finns i terminalloggen.", result.IntegerValue, company.Name, actorCompanyId));
                                    }

                                    result = tsm.ConvertTimeStampsToTimeBlocks(actorCompanyId);
                                    if (!result.Success)
                                    {
                                        CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Konvertering misslyckades: {0}", result.ErrorMessage));
                                    }
                                    else
                                    {
                                        result = tsm.DiscardUnsuccessfulEntries(actorCompanyId);
                                        if (!result.Success)
                                            CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Rensning av sju dagar gamla stämplingar som ej konverterats för '{0}' ({1}) misslyckades: {2}", company.Name, actorCompanyId, result.ErrorMessage));
                                        else if (result.IntegerValue > 0)
                                            CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Rensat {0} st tre dagar gamla stämplingar som ej konverterats för '{1}' ({2}). (Status satt till {3})", result.IntegerValue, company.Name, actorCompanyId, (int)TermGroup_TimeStampEntryStatus.ProcessedWithNoResult));
                                    }
                                    count++;
                                }
                            }
                            catch (Exception ex)
                            {
                                result = new ActionResult(ex);
                                base.LogError(ex);
                            }

                            if (!result.Success)
                            {
                                #region Logging

                                string prefix = "Conversion failed. ";
                                switch (result.ErrorNumber)
                                {
                                    case (int)ActionResultSave.EntityNotFound:
                                        CreateLogEntry(ScheduledJobLogLevel.Error, prefix + "EntityNotFound [" + result.ErrorMessage + "]");
                                        break;
                                    case (int)ActionResultSave.Unknown:
                                        CreateLogEntry(ScheduledJobLogLevel.Error, prefix + result.ErrorMessage);
                                        break;
                                    default:
                                        CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel vid exekvering av jobb: '{0}'", result.ErrorMessage));
                                        break;
                                }

                                #endregion
                            }
                        }
                    }
                    else
                    {
                        CreateLogEntry(ScheduledJobLogLevel.Information, "Inga nya stämplingar funna");
                    }
                }

                #endregion

                // Check in scheduled job
                CheckInScheduledJob(result.Success);
            }
        }
    }
}
