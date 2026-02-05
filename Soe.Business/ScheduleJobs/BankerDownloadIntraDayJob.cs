using Banker.Shared.Types;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.Banker;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.ScheduledJobs
{
    public class BankerDownloadIntraDayJob : ScheduledJobBase, IScheduledJob
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
                    CreateLogEntry(ScheduledJobLogLevel.Information, "Banker IntraDay: Start");

                    var sysBanks = cm.GetSysBanksForIntegration();

                    foreach (var bank in sysBanks)
                    {

                        #region Payment feedback

                        CreateLogEntry(ScheduledJobLogLevel.Information, "PaymentFeedback: Start:" + bank.BIC);

                        var sendResult = bankConnector.DownloadPaymentFeedback(sm, bank);

                        if (sendResult.Success)
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Information, $"PaymentFeedback:{sendResult.ErrorMessage}");
                        }
                        else
                        {
                            result.Success = false;
                            CreateLogEntry(ScheduledJobLogLevel.Error, $"PaymentFeedback:{sendResult.ErrorMessage}");
                        }

                        #endregion

                        #region OnBoarding

                        if (bankConnector.HasOnboardingFile(bank, out MaterialType onboardingMaterialType))
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Information, "OnboardingFiles: Start:" + bank.BIC);

                            sendResult = bankConnector.DownloadOnboardingfiles(sm, bank, onboardingMaterialType);

                            if (sendResult.Success)
                            {
                                CreateLogEntry(ScheduledJobLogLevel.Information, $"OnboardingFiles:{sendResult.ErrorMessage}");
                            }
                            else
                            {
                                result.Success = false;
                                CreateLogEntry(ScheduledJobLogLevel.Error, $"OnboardingFiles:{sendResult.ErrorMessage}");
                            }
                        }

                        #endregion
                    }

                    CreateLogEntry(ScheduledJobLogLevel.Information, "Banker IntraDay: End");
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
