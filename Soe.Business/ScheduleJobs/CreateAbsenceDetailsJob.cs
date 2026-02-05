using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.TimeEngine;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.ScheduledJobs
{
    public class CreateAbsenceDetailsJob : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);

            CompanyManager cm = new CompanyManager(parameterObject);
            FeatureManager fm = new FeatureManager(parameterObject);
            UserManager um = new UserManager(parameterObject);

            int? paramCompanyId = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "actorcompanyid").Select(s => s.IntData).FirstOrDefault();
            int? paramEmployeeId = paramCompanyId.HasValue ? scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "employeeid").Select(s => s.IntData).FirstOrDefault() : null;
            int batchInterval = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "batchinterval").Select(s => s.IntData).FirstOrDefault() ?? 90;
            DateTime startDate = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "startdate").Select(s => s.DateData).FirstOrDefault() ?? new DateTime(2021, 1, 1);
            DateTime stopDate = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "stopdate").Select(s => s.DateData).FirstOrDefault() ?? new DateTime(2022, 12, 31);

            #endregion

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();
            if (result.Success)
            {
                try
                {
                    using (CompEntities entities = new CompEntities())
                    {
                        List<int> companyIds = paramCompanyId.HasValue ? paramCompanyId.Value.ObjToList() : fm.GetCompaniesWithPermission((int)Feature.Time_Time_Attest, (int)Permission.Modify);
                        CreateLogEntry(ScheduledJobLogLevel.Information, $"Startar kontroll av frånvaro för {companyIds.Count} företag");

                        foreach (int companyId in companyIds)
                        {
                            Company company = cm.GetCompany(companyId);
                            if (company == null)
                                continue;

                            CreateLogEntry(ScheduledJobLogLevel.Information, $"Startar kontroll av frånvaro för företag {company.Name} ({companyId})");
                            if (paramEmployeeId.HasValue)
                                CreateLogEntry(ScheduledJobLogLevel.Information, $"Kontrollerar endast anställd {paramEmployeeId.Value}");

                            User adminUser = um.GetAdminUser(companyId);
                            if (adminUser == null)
                                adminUser = um.GetUsersByCompany(companyId, 0, scheduledJob.ExecuteUserId).FirstOrDefault();
                            if (adminUser == null)
                            {
                                CreateLogEntry(ScheduledJobLogLevel.Warning, $"Ingen admin-användare hittad. Hoppar över företag {company.Name} ({companyId})");
                                continue;
                            }
                                
                            TimeEngineManager tem = new TimeEngineManager(GetParameterObject(companyId, adminUser.UserId), companyId, adminUser.UserId);
                            List<CreateAbsenceDetailResultDTO> absenceResult = tem.CreateAbsenceDetails(batchInterval, startDate, stopDate, paramEmployeeId);
                            Dictionary<int, int> failedDays = absenceResult.GetFailedDaysByEmployee();
                            if (failedDays.Any())
                            {
                                foreach (var failedDay in failedDays)
                                {
                                    CreateLogEntry(ScheduledJobLogLevel.Error, $"EmployeeId:{failedDay.Key}. Misslyckades med {failedDay.Value} dagar");
                                }
                            }
                            else if (!absenceResult.HasAnyChanges())
                            {
                                CreateLogEntry(ScheduledJobLogLevel.Information, $"Ingen ohanterad frånvaro hittades");
                            }                                

                            CreateLogEntry(ScheduledJobLogLevel.Information, $"Klar med kontroll av frånvaro för företag {company.Name} ({companyId})");
                        }
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
