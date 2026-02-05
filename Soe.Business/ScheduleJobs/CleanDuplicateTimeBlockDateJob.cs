using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Linq;

namespace SoftOne.Soe.ScheduledJobs
{
    public class CleanDuplicateTimeBlockDateJob : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);

            CompanyManager cm = new CompanyManager(parameterObject);
            TimeBlockManager tm  = new TimeBlockManager(parameterObject);

            // Get parameters
            int? paramCompanyId = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "actorcompanyid").Select(s => s.IntData).FirstOrDefault();
            string paramEmployeeNr = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "employeenr").Select(s => s.StrData).FirstOrDefault();
            DateTime? paramDateFrom = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "datefrom").Select(s => s.DateData).FirstOrDefault();
            DateTime? paramDateTo = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "dateto").Select(s => s.DateData).FirstOrDefault();

            if (!paramCompanyId.HasValue || paramEmployeeNr.IsNullOrEmpty() || !paramDateFrom.HasValue || !paramDateTo.HasValue)
            {
                CreateLogEntry(ScheduledJobLogLevel.Error, "Felaktiga parametrar. actorCompanyId, employeeNr, dateFrom, dateTo måste anges");
                return;
            }                

            #endregion

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();
            if (result.Success)
            {
                try
                {
                    Company company = cm.GetCompany(paramCompanyId.Value);
                    if (company != null)
                    {
                        CreateLogEntry(ScheduledJobLogLevel.Information, $"Startar rensning av TimeBlockDate (actorCompanyId:{paramCompanyId}, employeeNr:{paramEmployeeNr}, dateFrom:{paramDateFrom}, dateTo:{paramDateTo})");
                        result = tm.CleanDuplicateTimeBlockDates(company, paramEmployeeNr, paramDateFrom.Value, paramDateTo.Value);
                        if (result.Success)
                        {
                            if (!result.Keys.IsNullOrEmpty())
                                CreateLogEntry(ScheduledJobLogLevel.Information, $"Rensning av dubbla TimeBlockDate klar. Borttagna TimeBlockDateId: {result.Keys.ToCommaSeparated()}");
                            else
                                CreateLogEntry(ScheduledJobLogLevel.Information, $"Rensning av dubbla TimeBlockDate klar. Inga dubletter hittades");

                        }
                        else
                            CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Rensning misslyckades : {0}", result.ErrorMessage));
                    }
                    else
                        CreateLogEntry(ScheduledJobLogLevel.Error, string.Format("Företag {0} hittades inte", paramCompanyId.Value));
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
