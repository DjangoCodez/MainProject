using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.Banker;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.ScheduledJobs
{
    public class BankerDownloadEndOfDayJob : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);
            var bankConnector = new BankerConnector();
            var sm = new SettingManager(parameterObject);
            var cm = new CountryCurrencyManager(parameterObject);

            #endregion

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();
            if (result.Success)
            {
                try
                {
                    CreateLogEntry(ScheduledJobLogLevel.Information, "Banker EoD: Start");

                    var sysBanks = cm.GetSysBanksForIntegration();

                    foreach (var bank in sysBanks)
                    {

                        #region Payment
                        
                        CreateLogEntry(ScheduledJobLogLevel.Information, "Payments: Start:" + bank.BIC);

                        var sendResult = bankConnector.DownloadPayment(sm, bank);

                        if (sendResult.Success)
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Information, $"Payments End:{sendResult.ErrorMessage}");
                        }
                        else
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Error, $"Payments End:{sendResult.ErrorMessage}");
                        }


                        #endregion

                        #region Finvoice

                        if (bank.SysCountryId == (int)TermGroup_Country.FI)
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Information, "Finvoice: Start:" + bank.BIC);

                            sendResult = bankConnector.DownloadFinvoice(sm, bank);

                            if (sendResult.Success)
                            {
                                CreateLogEntry(ScheduledJobLogLevel.Information, $"Finvoice End:{sendResult.ErrorMessage}");
                            }
                            else
                            {
                                CreateLogEntry(ScheduledJobLogLevel.Error, $"Finvoice End:{sendResult.ErrorMessage}");
                            }

                            CreateLogEntry(ScheduledJobLogLevel.Information, "FinvoiceAttachment: Start:" + bank.BIC);

                            sendResult = bankConnector.DownloadFinvoiceAttachment(sm, bank);

                            if (sendResult.Success)
                            {
                                CreateLogEntry(ScheduledJobLogLevel.Information, $"FinvoiceAttachment End:{sendResult.ErrorMessage}");
                            }
                            else
                            {
                                CreateLogEntry(ScheduledJobLogLevel.Error, $"FinvoiceAttachment End:{sendResult.ErrorMessage}");
                            }

                            CreateLogEntry(ScheduledJobLogLevel.Information, "FinvoiceFeedback: Start:" + bank.BIC);

                            sendResult = bankConnector.DownloadFinvoiceFeedback(sm, bank);

                            if (sendResult.Success)
                            {
                                CreateLogEntry(ScheduledJobLogLevel.Information, $"FinvoiceFeedback End:{sendResult.ErrorMessage}");
                            }
                            else
                            {
                                CreateLogEntry(ScheduledJobLogLevel.Error, $"FinvoiceFeedback End:{sendResult.ErrorMessage}");
                            }
                        }

                        #endregion

                        #region Reimport errors

                        CreateLogEntry(ScheduledJobLogLevel.Information, "TryReimportErrors: Start");
                        sendResult = bankConnector.TryReimportDownloadErrors(sm);
                        if (sendResult.Success)
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Information, $"TryReimportErrors End:{sendResult.ErrorMessage}");
                        }
                        else
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Error, $"TryReimportErrors End:{sendResult.ErrorMessage}");
                        }
                        #endregion
                    }

                    CreateLogEntry(ScheduledJobLogLevel.Information, "Banker Eod: End");
                }
                catch (Exception ex)
                {
                    result = new ActionResult(ex);
                    CreateLogEntry(ScheduledJobLogLevel.Error, string.Format("Exception vid exekvering av jobb: '{0}'", result.ErrorMessage));
                    base.LogError(ex);
                }

                // Check in scheduled job
                CheckInScheduledJob(result.Success);
            }
            else
            {
                #region Logging

                switch (result.ErrorNumber)
                {
                    default:
                        CreateLogEntry(ScheduledJobLogLevel.Error, string.Format("Fel vid exekvering av jobb: '{0}'", result.ErrorMessage));
                        break;
                }

                #endregion
            }

        }
    }
}
