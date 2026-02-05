using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Linq;

namespace SoftOne.Soe.ScheduledJobs
{
    public class SendSupplierInvoiceAttestReminderJob : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);

            var am = new AttestManager(parameterObject);
            var sm = new SettingManager(parameterObject);
            var um = new UserManager(parameterObject);
            var sim = new SupplierInvoiceManager(parameterObject);

            int userId = scheduledJob.ExecuteUserId;
            int? paramCompanyId = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "actorcompanyid").Select(s => s.IntData).FirstOrDefault();

            #endregion

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();
            if (result.Success)
            {
                try
                {
                    var companies = base.GetCompanies(paramCompanyId);

                    CreateLogEntry(ScheduledJobLogLevel.Information, "Startar jobb påminnelse om att attestera leverantörsfakturor");

                    foreach (var company in companies)
                    {
                        #region Company

                        int noOfSentMails = 0;

                        bool supplierInvoiceAutoReminder = sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.SupplierInvoiceAutoReminder, userId, company.ActorCompanyId, 0);
                        if (!supplierInvoiceAutoReminder)
                            continue;

                        User adminUser = um.GetAdminUser(company.ActorCompanyId);
                        if (adminUser != null)
                            userId = adminUser.UserId;  // Get enought credentials to run something inside company

                        #endregion

                        #region SupplierInvoice

                        var supplierInvoiceItems = sim.GetSupplierInvoicesForGrid(true, false,TermGroup_ChangeStatusGridAllItemsSelection.Twelve_Months);
                        supplierInvoiceItems = sim.FilterSupplierInvoices(SoeOriginStatusClassification.SupplierInvoicesAttestFlowHandled, supplierInvoiceItems);
                        var invoiceIds = supplierInvoiceItems.Select(i => i.SupplierInvoiceId).Distinct().ToList();

                        if (am.SendSingleAttestReminders(company.ActorCompanyId, 0, userId, invoiceIds).Success)
                            noOfSentMails = invoiceIds.Count;

                        #endregion

                        jm.CreateLogEntry(scheduledJob.SysScheduledJobId, batchNr, ScheduledJobLogLevel.Information, "Skickade " + noOfSentMails.ToString() + " påminnelsemail för " + company.Name);
                    }
                }
                catch (Exception ex)
                {
                    result = new ActionResult(ex);
                    CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel vid exekvering av jobb: '{0}'", result.ErrorMessage));
                    base.LogError(ex);
                }

                // Check in scheduled job
                CheckInScheduledJob(result.Success);
            }
        }


    }
}
