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
    public class CompanyInformationSendPushNotificationJob : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);

            UserManager um = new UserManager(parameterObject);
            CompanyManager cm = new CompanyManager(parameterObject);            
            GeneralManager gm = new GeneralManager(parameterObject);

            // Get parameters            
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
                    CreateLogEntry(ScheduledJobLogLevel.Information, "Startar jobb för att skicka pushnotifieringar för intern information");

                    using (CompEntities entities = new CompEntities())
                    {                        
                        List<Information> informations = gm.GetCompanyInformationsForSendPushNotificationsJob(entities, paramCompanyId, DateTime.Now);
                        var informationsByCompany = informations.GroupBy(i => i.ActorCompanyId).ToList();
                        var companies = cm.GetCompanies(informations.Select(x => x.ActorCompanyId).Distinct().ToList());
                        Dictionary<int, Company> companiesDict = companies.GroupBy(g => g.ActorCompanyId).ToDictionary(x => x.Key, x => x.FirstOrDefault());

                        //Log
                        CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Skickar pushnotifieringar för {0} st företag", informationsByCompany.Count));
                        int nrOfSuccededPushAttempts = 0;
                        int nrOfFailedPushAttempts = 0;

                        foreach (var companyInformations in informationsByCompany)
                        {
                            int actorCompanyId = companyInformations.Key;
                            if (companiesDict.ContainsKey(actorCompanyId))
                            {
                                parameterObject.SetSoeUser(um.GetSoeUser(actorCompanyId, scheduledJob.ExecuteUserId));
                                parameterObject.SetSoeCompany(cm.GetSoeCompany(companiesDict[actorCompanyId]));

                                string companyStamp = String.Format("{0}.{1}", parameterObject.SoeCompany.ActorCompanyId, parameterObject.SoeCompany.Name);
                                CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Börjar skicka pushnotifieringar för {0})", companyStamp));

                                foreach (Information information in companyInformations)
                                {
                                    string informationStamp = information.Subject;
                                    CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Skickar pushnotifieringar för '{0}' - '{1}'", companyStamp, informationStamp));

                                    result = gm.SendPushNotification(entities, information);
                                    if (result.Success)
                                    {
                                        CreateLogEntry(ScheduledJobLogLevel.Success, String.Format("Pushnotifieringar har skickats för '{0}' - '{1}'", companyStamp, informationStamp));
                                        nrOfSuccededPushAttempts++;
                                    }
                                    else
                                    {
                                        CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Inga pushnotifieringar har skickats för '{0}' - '{1}'", companyStamp, informationStamp));
                                        nrOfFailedPushAttempts++;
                                    }
                                }

                                CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Klar med att skicka pushnotifieringar för '{0}')", companyStamp));
                            }
                            else
                            {
                                CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Företag {0} hittades inte", companyInformations.Key));
                                continue;
                            }
                        }

                        int totalBatches = nrOfSuccededPushAttempts + nrOfFailedPushAttempts;
                        jm.CreateLogEntry(scheduledJob.SysScheduledJobId, batchNr, ScheduledJobLogLevel.Information, String.Format("Klar med att skicka pushnotifieringar, {0} av {1} försök lyckades", nrOfSuccededPushAttempts, totalBatches));

                        success = nrOfFailedPushAttempts == 0;
                    }
                }
                catch (Exception ex)
                {
                    result = new ActionResult(ex);
                    CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel vid exekvering av jobb: '{0}'", result.ErrorMessage));
                    base.LogError(ex);
                }

                // Check in scheduled job
                CheckInScheduledJob(success);
            }
        }
    }
}
