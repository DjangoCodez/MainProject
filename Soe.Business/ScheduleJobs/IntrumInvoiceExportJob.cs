using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Linq;

namespace SoftOne.Soe.ScheduledJobs
{
    public class IntrumInvoiceExportJob : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);
            UserManager um = new UserManager(parameterObject);
            InvoiceDistributionManager im = new InvoiceDistributionManager(parameterObject);

            #endregion

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();
            if (result.Success)
            {
                try
                {
                    CreateLogEntry(ScheduledJobLogLevel.Information, "Intrum Export: Start");

                    using (var entities = new CompEntities())
                    {
                        var pendingInvoices = im.GetDistributionItems(entities, TermGroup_EDistributionType.Intrum, null, TermGroup_EDistributionStatusType.PendingInPlatform, null);

                        foreach (var companyInvoices in pendingInvoices.GroupBy(i => i.ActorCompanyId))
                        {
                            var actorCompanyId = companyInvoices.Key;
                            var invoices = companyInvoices.Select(i => i.OriginId).ToList();

                            CreateLogEntry(ScheduledJobLogLevel.Information, $"Found {invoices.Count} invoices for actorCompanyId:{actorCompanyId}");

                            User adminUser = um.GetAdminUser(actorCompanyId);
                            if (adminUser == null)
                                adminUser = um.GetUsersByCompany(actorCompanyId, 0, scheduledJob.ExecuteUserId).FirstOrDefault();

                            if (adminUser == null)
                            {
                                CreateLogEntry(ScheduledJobLogLevel.Warning, $"No admin user found. skipping company ({actorCompanyId})");
                                continue;
                            }

                            var eim = new ElectronicInvoiceMananger(GetParameterObject(actorCompanyId, adminUser.UserId));
                            var createResult = eim.CreateIntrumInvoice(invoices, actorCompanyId, true);

                            if (createResult.Success)
                            {
                                CreateLogEntry(ScheduledJobLogLevel.Information, $"Invoices processed, actorCompanyId:{actorCompanyId} Successcount: {createResult.IntegerValue} ErrorCount: {createResult.IntegerValue2}");
                            }
                            else
                            {
                                CreateLogEntry(ScheduledJobLogLevel.Error, $"Failed creating invoices, actorCompanyId: {actorCompanyId} error: {createResult.ErrorMessage}");
                            }
                        }
                    }

                    CreateLogEntry(ScheduledJobLogLevel.Information, "Intrum Export: End");
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
                        CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel vid exekvering av jobb: '{0}'", result.ErrorMessage));
                        break;
                }

                #endregion
            }

        }
    }
}
