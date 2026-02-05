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
    public class CalculateTimeAccumulatorBalanceJob : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);

            CompanyManager cm = new CompanyManager(parameterObject);
            UserManager um = new UserManager(parameterObject);
            TimeAccumulatorManager tam = new TimeAccumulatorManager(parameterObject);

            int? paramCompanyId = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "actorcompanyid").Select(s => s.IntData).FirstOrDefault();

            #endregion

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();
            if (result.Success)
            {
                try
                {
                    List<Company> companies;
                    if (paramCompanyId.HasValue)
                    {
                        companies = cm.GetCompany(paramCompanyId.Value).ObjToList();
                    }                        
                    else
                    {
                        using (CompEntities entities = new CompEntities())
                        {
                            var ids = entities.TimeAccumulator.Where(w => w.Company.State == (int)SoeEntityState.Active && w.Company.License.State == (int)SoeEntityState.Active && w.State == (int)SoeEntityState.Active && w.Type == (int)TermGroup_TimeAccumulatorType.Rolling).Select(s => s.ActorCompanyId).Distinct().ToList();
                            companies = cm.GetCompanies(ids);
                        }
                        companies = companies.OrderBy(o => o.ActorCompanyId).ToList();
                    }

                    // Execute job
                    CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Startar beräkning av årsskiftesberäkning för saldon för {0} företag", companies.Count));
                    int count = 0;
                    int total = companies.Count;

                    foreach (Company company in companies)
                    {
                        int actorCompanyId = company.ActorCompanyId;

                        var adminUser = um.GetAdminUser(actorCompanyId);

                        if (adminUser == null)
                            adminUser = um.GetUsersByCompany(actorCompanyId, 0, scheduledJob.ExecuteUserId).FirstOrDefault();

                        ParameterObject param = null;

                        CreateLogEntry(ScheduledJobLogLevel.Information, "started actorCompanyId " + actorCompanyId.ToString());

                        if (adminUser == null)
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Error, "adminUser is null" + actorCompanyId.ToString());
                        }
                        else
                        {
                            param = GetParameterObject(actorCompanyId, adminUser.UserId);
                            if (param != null)
                                tam = new TimeAccumulatorManager(param);
                        }

                        count++;
                        CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Beräkning startar för '{0}' ({1}) {2}/{3}", company.Name, company.ActorCompanyId, count, total));
                        result = tam.CalculateTimeAccumulatorYearBalance(company.ActorCompanyId);
                        if (!result.Success)
                            CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Beräkning misslyckades för '{0}' ({1}): {2}", company.Name, company.ActorCompanyId, result.ErrorMessage));
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
