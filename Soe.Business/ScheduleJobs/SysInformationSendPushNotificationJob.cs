using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.ScheduledJobs
{
    public class SysInformationSendPushNotificationJob : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);
                        
            GeneralManager gm = new GeneralManager(parameterObject);
            
            #endregion

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();
            if (result.Success)
            {
                bool success = true;

                try
                {
                    // Execute job
                    CreateLogEntry(ScheduledJobLogLevel.Information, "Startar jobb för att skicka pushnotifieringar för publik information");

                    using (SOESysEntities entities = new SOESysEntities())
                    {                     
                        List<SysInformation> informations = gm.GetSysInformationsForSendPushNotificationsJob(entities, DateTime.Now);
                        
                        //Log
                        CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Skickar pushnotifieringar för {0} st objekt", informations.Count));
                        int nrOfSuccededPushAttempts = 0;
                        int nrOfFailedPushAttempts = 0;

                        foreach (var information in informations)
                        {
                            string informationStamp = information.Subject;
                            CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Börjar skicka pushnotifieringar för '{0}')", informationStamp));

                            result = gm.SendPushNotification(entities, information);
                            if (result.Success)
                            {
                                CreateLogEntry(ScheduledJobLogLevel.Success, String.Format("Pushnotifieringar har skickats för '{0}'", informationStamp));
                                nrOfSuccededPushAttempts++;
                            }
                            else
                            {
                                CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Inga pushnotifieringar har skickats för '{0}'", informationStamp));
                                nrOfFailedPushAttempts++;
                            }
                            
                            CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Klar med att skicka pushnotifieringar för '{0}')", informationStamp));                          
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
