using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Linq;

namespace SoftOne.Soe.ScheduledJobs
{
    public class EdiTransferJob : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);

            #endregion

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();
            if (result.Success)
            {
                try
                {
                    // Execute job
                    EdiManager ediManager = new EdiManager(parameterObject);
                    SettingManager settingManager = new SettingManager(parameterObject);
                    CompanyManager companyManager = new CompanyManager(parameterObject);

                    var actorCompanyIds = settingManager.GetCompanyIdsWithCompanyBoolSetting(CompanySettingType.BillingEdiTransferToSupplierInvoice);
                    actorCompanyIds.AddRange(settingManager.GetCompanyIdsWithCompanyBoolSetting(CompanySettingType.BillingEdiTransferToOrderAdvanced));                    
                    actorCompanyIds = actorCompanyIds.Distinct().OrderBy(o => 0).ToList();

                    using (CompEntities entities = new CompEntities())
                    {
                        DateTime sevenDaysAgo = DateTime.Today.AddDays(-7);
                        var actorCompanyIdfromlastSevenDays = entities.EdiEntry.Where(w => actorCompanyIds.Contains(w.ActorCompanyId) && w.Status == (int)TermGroup_EDIStatus.Unprocessed && w.State == (int)SoeEntityState.Active && w.Created > sevenDaysAgo).Select(s => s.ActorCompanyId);
                        actorCompanyIds = actorCompanyIds.Where(w => actorCompanyIdfromlastSevenDays.Contains(w)).ToList();
                    }

                    var companies = companyManager.GetCompanies(actorCompanyIds).OrderBy(o => o.ActorCompanyId).ToList();
                    int quantity = companies.Count;
                    int count = 1;
                    foreach (var company in companies)
                    {
                        CreateLogEntry(ScheduledJobLogLevel.Information, String.Format($"Edi Processing: {company.ActorCompanyId} {company.Name} {count}/{quantity} "));
                        var transferResult = ediManager.TransferToOrdersAndSupplierInvoicesFromEdi(company.ActorCompanyId);

                        if (!transferResult.Success && !string.IsNullOrEmpty(transferResult.ErrorMessage))
                            CreateLogEntry(ScheduledJobLogLevel.Information, String.Format($"Edi Processing Error: {company.ActorCompanyId} {company.Name} {result.ErrorMessage}"));

                        count++;
                    }

                    CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Logg: '{0}'", result.ErrorMessage));
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
