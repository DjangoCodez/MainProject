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
    public class ValidateDeviationCauseFromProjectInvoiceDayJob : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);

            CompanyManager cm = new CompanyManager(parameterObject);
            SettingManager sm = new SettingManager(parameterObject);
            ProjectManager pm = new ProjectManager(parameterObject);
            TimeCodeManager tm = new TimeCodeManager(parameterObject);
            TimeDeviationCauseManager td = new TimeDeviationCauseManager(parameterObject);

            int? paramCompanyId = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "actorcompanyid").Select(s => s.IntData).FirstOrDefault();
            int? paramInvoiceId = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "invoiceid").Select(s => s.IntData).FirstOrDefault();

            #endregion

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();

            if (result.Success)
            {
                try
                {
                    using (CompEntities entities = new CompEntities())
                    {
                        entities.Connection.Open();

                        var companies = base.GetCompanies(paramCompanyId);

                        CreateLogEntry(ScheduledJobLogLevel.Information, "Startar matchning tidkod mot orsak");

                        foreach (var company in companies)
                        {
                            result = pm.ValidateTimeCodeTransactionsAndCauses(entities, company);
                            if (result.Success)
                                CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Matching klar för '{0}' ({1})", company.Name, company.ActorCompanyId));
                            else
                                CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel i matchningsjobb för '{0}' ({1})", company.Name, company.ActorCompanyId) + ": " + result.ErrorMessage);

                        }

                        CreateLogEntry(ScheduledJobLogLevel.Information, "Startar kontroll av tidavtal och avvikelseorsak");
                        foreach (Company company in companies)
                        {
                            result = pm.ValidateEmployeeGroupTimeDeviationCode(entities, company);
                            if (result.Success)
                                CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Kontroll klar för '{0}' ({1})", company.Name, company.ActorCompanyId));
                            else
                                CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel i kontroll av tidavtal för '{0}' ({1})", company.Name, company.ActorCompanyId) + ": " + result.ErrorMessage);

                        }
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

