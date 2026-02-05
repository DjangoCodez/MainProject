using SoftOne.Common.KeyVault.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.Banker;
using SoftOne.Soe.Business.Core.RSK;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SoftOne.Soe.ScheduledJobs
{
    public class RSKUpdateSysProductsJob : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);
            var rskManager = new RSKManager(parameterObject);
            var sysPLManager = new SysPriceListManager(parameterObject);
            var vaultsettings = KeyVaultSettingsHelper.GetKeyVaultSettings();

            int? monthsToGoBack = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "monthstogoback").Select(s => s.IntData).FirstOrDefault();

            #endregion

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();
            if (result.Success)
            {
                try
                {
                    CreateLogEntry(ScheduledJobLogLevel.Information, "RSK update names job start: " + DateTime.Now.ToString());
                    int noOfCollected = 0;
                    int noOfUpdated = 0;

                    var sysProductGroups = rskManager.GetSysProductGroupsSmall(ExternalProductType.Plumbing);
                    var listParts = sysProductGroups.SplitList(sysProductGroups.Count / 6);

                    int taskCounter = 0;
                    Task[] tasks = new Task[listParts.Count()];
                    foreach (var groupPart in listParts)
                    {
                        tasks[taskCounter] = new Task(() =>
                        {
                            foreach (var productGroup in groupPart)
                            {
                                if (sysProductGroups.Any(g => g.Field2 == productGroup.Field1))
                                    continue;

                                CreateLogEntry(ScheduledJobLogLevel.Information, string.Format("Starting update for group {0}", productGroup.Field3));
                                result = rskManager.UpdateSysProductNameFromProductGroup(productGroup, vaultsettings, monthsToGoBack);

                                if (result.Success)
                                {
                                    CreateLogEntry(ScheduledJobLogLevel.Information, string.Format("Update for product group {0} finished. {1} products collected, {2} matched and updated.", productGroup.Field3, result.IntegerValue, result.IntegerValue2));
                                    noOfCollected = noOfCollected + result.IntegerValue;
                                    noOfUpdated = noOfUpdated + result.IntegerValue2;
                                }
                                else
                                {
                                    CreateLogEntry(ScheduledJobLogLevel.Information, string.Format("Update for product group {0} failed. {1}", productGroup.Field3, result.Exception));
                                }
                            }
                        });
                        taskCounter++;
                    }

                    // Start tasks
                    foreach(var task in tasks) { task.Start(); }

                    // Await tasks
                    Task.WaitAll(tasks);

                    #region Single loop

                    /*foreach (var productGroup in sysProductGroups)
                    {
                        if (sysProductGroups.Any(g => g.Field2 == productGroup.Field1))
                            continue;

                        CreateLogEntry(ScheduledJobLogLevel.Information, string.Format("Starting update for group {0}", productGroup.Field3));
                        result = rskManager.UpdateSysProductNameFromProductGroup(productGroup);

                        if (result.Success)
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Information, string.Format("Update for product group {0} finished. {1} products collected, {2} matched and updated.", productGroup.Field3, result.IntegerValue, result.IntegerValue2)); 
                            noOfCollected = noOfCollected + result.IntegerValue;
                            noOfUpdated = noOfUpdated + result.IntegerValue2;
                        }
                        else
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Information, string.Format("Update for product group {0} failed. {1}", productGroup.Field3, result.Exception));
                        }
                    }*/

                    #endregion

                    CreateLogEntry(ScheduledJobLogLevel.Information, string.Format("RSK update names job finished: " + DateTime.Now.ToString() + ". Totalt hämtade {0}, totalt uppdaterade {1}.", noOfCollected, noOfUpdated));
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
