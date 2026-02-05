using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.ScheduledJobs
{
    public class MigrateProjectBudgetsJob : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);

            CompanyManager cm = new CompanyManager(parameterObject);
            ProjectBudgetManager pbm = new ProjectBudgetManager(parameterObject);

            int? paramCompanyId = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "actorcompanyid").Select(s => s.IntData).FirstOrDefault();
            List<int> companyIds = new List<int>();
            string paramCompanyIds = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "actorcompanyids").Select(s => s.StrData).FirstOrDefault();
            if (paramCompanyIds.HasValue())
            {
                var ids = paramCompanyIds.Split(';');
                companyIds = Array.ConvertAll(ids, int.Parse).ToList();
            }

            #endregion

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();

            if (result.Success)
            {
                try
                {
                    List<Company> companies;
                    if (!companyIds.IsNullOrEmpty())
                        companies = cm.GetCompanies(companyIds);
                    else if (paramCompanyId.HasValue)
                        companies = cm.GetCompany(paramCompanyId.Value).ObjToList();
                    else
                        companies = cm.GetCompanies();

                    // Execute job
                    CreateLogEntry(ScheduledJobLogLevel.Information, "Startar jobbet för migrering av project budget");
                    foreach (Company company in companies)
                    {
                        var budgetIdsToMigrate = pbm.GetBudgetHeadsToMigrateForCompany(company.ActorCompanyId);

                        CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Startar migrering av {0} projekt budget(ar) för '{1}' ({2})", budgetIdsToMigrate.Count, company.Name, company.ActorCompanyId));

                        foreach(var budgetHeadId in budgetIdsToMigrate)
                        {
                            result = pbm.MigrateBudgetHeadIncludingRows(budgetHeadId, true);
                            if (result.Success)
                                CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Migrering klar för budget head id '{0}' för '{1}' ({2})", budgetHeadId, company.Name, company.ActorCompanyId) + ": " + result.InfoMessage);
                            else
                                CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel i migreringsjobb för budget head id '{0}' för '{1}' ({2})", budgetHeadId, company.Name, company.ActorCompanyId) + ": " + result.ErrorMessage);
                        }

                        if (result.Success)
                            CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Migrering klar för '{0}' ({1})", company.Name, company.ActorCompanyId) + ": " + result.InfoMessage);
                        else
                            CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel i migreringsjobb för '{0}' ({1})", company.Name, company.ActorCompanyId) + ": " + result.ErrorMessage);

                    }

                    /*List<Company> companies;
                    if (!companyIds.IsNullOrEmpty())
                        companies = cm.GetCompanies(companyIds);
                    else if (paramCompanyId.HasValue)
                        companies = cm.GetCompany(paramCompanyId.Value).ObjToList();
                    else
                        companies = cm.GetCompanies();

                    // Execute job
                    CreateLogEntry(ScheduledJobLogLevel.Information, "Startar migrering av ProjectInvoiceDay");
                    foreach (Company company in companies)
                    {
                        result = pm.MigrateProjectInvoiceDaysToProjectTimeBlocks(company.ActorCompanyId);
                        if(result.Success)
                            CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Migrering klar för '{0}' ({1})", company.Name, company.ActorCompanyId) + ": " + result.InfoMessage);
                        else
                            CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel i migreringsjobb för '{0}' ({1})", company.Name, company.ActorCompanyId) + ": " + result.ErrorMessage);

                    }*/
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

                // Check in scheduled job
                CheckInScheduledJob(result.Success);
            }
        }
    }
}

