using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Linq;

namespace SoftOne.Soe.ScheduledJobs
{
    public class PurgeDeletedRecordsJob : ScheduledJobBase, IScheduledJob
    {

        //Make a mehtod to delete files in a folder and subfolders that are older than specified date.



        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);

            #endregion

            // Check if any other job is running

            var now = DateTime.Now;
            if (now.Hour > 5 && now.Hour < 22)
            {
                CreateLogEntry(ScheduledJobLogLevel.Information, "Rensning av borttagna poster körs inte mellan kl 05-22:" + now.ToString());
                jm.PostponeScheduledJob(scheduledJob, CalendarUtility.TimeSpanToMinutes(new DateTime(now.Year, now.Month, now.Day, 22, 0, 0), now));
                return;
            }

            if (jm.IsAnyJobRunning(scheduledJob.DatabaseName.ToLower()))
            {
                CreateLogEntry(ScheduledJobLogLevel.Warning, "Kan ej starta rensning av borttagna poster. Annat jobb körs. Skjuter upp exekvering 5 min");
                jm.PostponeScheduledJob(scheduledJob, 5);
                return;
            }

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();
            if (result.Success)
            {
                try
                {
                    // Execute job
                    CreateLogEntry(ScheduledJobLogLevel.Information, "Startar rensning av borttagna poster");

                    using (var entities = new CompEntities())
                    {
                        entities.CommandTimeout = (60 * 60);
                        int count = 0;

                        #region Time
                        try
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Information, "Rensar poster i tidmodulen");

                            var purgeResults = entities.PurgeDeletedTimeRecords();
                            if (purgeResults != null)
                            {
                                var purgeResult = purgeResults.FirstOrDefault();
                                string message = "{0} poster raderade från {1}";

                                CreateLogEntry(ScheduledJobLogLevel.Information, String.Format(message, purgeResult.TimeInvoiceTransactionAccountsDeleted, "TimeInvoiceTransactionAccount"));
                                CreateLogEntry(ScheduledJobLogLevel.Information, String.Format(message, purgeResult.TimeInvoiceTransactionsDeleted, "TimeInvoiceTransaction"));
                                CreateLogEntry(ScheduledJobLogLevel.Information, String.Format(message, purgeResult.TimePayrollTransactionAccountsDeleted, "TimePayrollTransactionAccount"));
                                CreateLogEntry(ScheduledJobLogLevel.Information, String.Format(message, purgeResult.TimePayrollTransactionsDeleted, "TimePayrollTransaction"));
                                CreateLogEntry(ScheduledJobLogLevel.Information, String.Format(message, purgeResult.TimeCodeTransactionsDeleted, "TimeCodeTransaction"));
                                CreateLogEntry(ScheduledJobLogLevel.Information, String.Format(message, purgeResult.TimeBlockAccountsDeleted, "TimeBlockAccount"));
                                CreateLogEntry(ScheduledJobLogLevel.Information, String.Format(message, purgeResult.TimeBlockCodeMappingsDeleted, "TimeBlockCodeMapping"));
                                CreateLogEntry(ScheduledJobLogLevel.Information, String.Format(message, purgeResult.TimeStampEntryTimeBlockMappingsDeleted, "TimeStampEntryTimeBlockMapping"));
                                CreateLogEntry(ScheduledJobLogLevel.Information, String.Format(message, purgeResult.TimeStampEntriesDeleted, "TimeStampEntry"));
                                CreateLogEntry(ScheduledJobLogLevel.Information, String.Format(message, purgeResult.TimeBlocksDeleted, "TimeBlock"));
                                CreateLogEntry(ScheduledJobLogLevel.Information, String.Format(message, purgeResult.TimeBlockGroupsDeleted, "TimeBlockGroup"));
                                CreateLogEntry(ScheduledJobLogLevel.Information, String.Format(message, purgeResult.TimeScheduleTemplateBlockAccountsDeleted, "TimeScheduleTemplateBlockAccount"));
                                CreateLogEntry(ScheduledJobLogLevel.Information, String.Format(message, purgeResult.TimeScheduleTemplateBlockHistoriesDeleted, "TimeScheduleTemplateBlockHistory"));
                                CreateLogEntry(ScheduledJobLogLevel.Information, String.Format(message, purgeResult.TimeScheduleTemplateBlockQueuesDeleted, "TimeBlockGroup"));
                                CreateLogEntry(ScheduledJobLogLevel.Information, String.Format(message, purgeResult.TimeScheduleTemplateBlocksDeleted, "TimeScheduleTemplateBlock"));
                            }
                        }
                        catch (Exception ex)
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel vid rensning av tidmodulen: {0}", ex.Message));
                        }

                        #endregion

                        #region Economy 
                        try
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Information, "Rensar raderade levfaktura rader");
                            entities.PurgeDeletedSupplierInvoiceRows();
                        }
                        catch (Exception ex)
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel vid rensning av levfaktura rader: {0}", ex.Message));
                        }

                        #endregion

                        #region Billing

                        try
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Information, "Rensar raderade konteringsrader (kundfaktura rader)");
                            entities.PurgeDeletedCustomerInvoiceAccountRows();
                        }
                        catch (Exception ex)
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel vid rensning av CustomerInvoiceAccountRow: {0}", ex.Message));
                        }

                        try
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Information, "Rensar raderade kundfaktura rader");
                            entities.PurgeDeletedCustomerInvoiceRows();
                        }
                        catch (Exception ex)
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel vid rensning av CustomerInvoiceRow: {0}", ex.Message));
                        }

                        try
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Information, "Rensar Edi Entries");
                            count = entities.PurgeEdiEntry();
                            CreateLogEntry(ScheduledJobLogLevel.Information, $"Tog bort {count} Edi entries");
                        }
                        catch (Exception ex)
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel vid rensning av Edi Entries: {0}", ex.Message));
                        }

                        #endregion

                        #region Common

                        CreateLogEntry(ScheduledJobLogLevel.Information, "Flyttar från UserSession till UserSessionHistory");
                        try
                        {
                            entities.PurgeUserSession();
                        }
                        catch (Exception ex)
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel vid rensning av UserSession: {0}", ex.Message));
                        }

                        try
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Information, "Rensar DataStorage");
                            count = entities.PurgeDataStorage();
                            CreateLogEntry(ScheduledJobLogLevel.Information, $"Tog bort {count} DataStorage poster");
                        }
                        catch (Exception ex)
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel vid rensning av DataStorage: {0}", ex.Message));
                        }

                        try
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Information, "Rensar IO tabellerna");
                            count = entities.PurgeOldIORecords();
                            CreateLogEntry(ScheduledJobLogLevel.Information, $"Tog bort {count} IO poster");
                        }
                        catch (Exception ex)
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel vid rensning av IO tabellerna: {0}", ex.Message));
                        }

                        #endregion

                        #region Files

                        try
                        {
                            FileUtil.DeleteOldFiles(Constants.SOE_CRGEN_PATH, DateTime.Now.AddHours(-12));
                            FileUtil.DeleteOldFiles(ConfigSettings.SOE_SERVER_DIR_TEMP_PHYSICAL, DateTime.Now.AddHours(-12));
                        }
                        catch (Exception ex)
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel vid rensning av temporära filer: {0}", ex.Message));

                        }

                        #endregion
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

                // Check in scheduled job
                CheckInScheduledJob(result.Success);
            }
        }
    }
}
