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
    public class AnnualLeaveTransactionJob : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);

            AnnualLeaveManager alm = new AnnualLeaveManager(parameterObject);
            CompanyManager cm = new CompanyManager(parameterObject);
            EmployeeManager em = new EmployeeManager(parameterObject);

            int singleActorCompanyId = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "actorcompanyid" && s.IntData.HasValue).Select(s => s.IntData.Value).FirstOrDefault();
            int offsetDays = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "offsetdays" && s.IntData.HasValue).Select(s => s.IntData.Value).FirstOrDefault();
            int recalcPreviousYearDays = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "recalcpreviousyeardays" && s.IntData.HasValue).Select(s => s.IntData.Value).FirstOrDefault();
            if (recalcPreviousYearDays == default)
                recalcPreviousYearDays = 0;

            #endregion

            string startMessage = "Starting calculation of annual leave days";
            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();
            if (result.Success)
            {
                try
                {
                    List<int> companyIds = new List<int>();

                    if (singleActorCompanyId != default && singleActorCompanyId > 0)
                        companyIds.Add(singleActorCompanyId);
                    else
                        companyIds = alm.GetCompanyIdsWithAnnualLeaveSetting();

                    if (companyIds.Any())
                    {
                        int numberOfCompanies = companyIds.Count;
                        DateTime startDate = new DateTime(DateTime.Now.AddDays(-1).Year, 1, 1);
                        DateTime stopDate = DateTime.Now.AddDays(-1); // yesterday as default
                        bool recalcPreviousYearBalance = CalculatePreviousYearBalance();

                        if (offsetDays != default && offsetDays > 0)
                            stopDate = stopDate.AddDays(offsetDays);

                        if (startDate > stopDate)
                            throw new Exception("Start date cannot be after stop date. Terminating job.");

                        //Execute the job
                        CreateLogEntry(ScheduledJobLogLevel.Information, String.Format(startMessage, startDate, stopDate));

                        int count = 1;
                        foreach (int actorCompanyId in companyIds)
                        {
                            CompanyDTO company = cm.GetCompany(actorCompanyId)?.ToCompanyDTO();
                            if (company == null)
                                continue;

                            CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Starting caclulation for '{0}' ({1}) {2}/{3} batchnr {4}", company.Name, company.ActorCompanyId, count, numberOfCompanies, batchNr));

                            List<int> employeeIds = em.GetAllEmployeeIds(actorCompanyId, active: true);
                            if (employeeIds.Any())
                            {
                                if (recalcPreviousYearBalance)
                                {
                                    result = alm.CalculateAnnualLeaveTransactions(actorCompanyId, employeeIds, startDate, stopDate, true);
                                    if (result.Success)
                                        CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Previous year balance calculation finished OK for '{0}' ({1})", company.Name, company.ActorCompanyId));
                                    else
                                        CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Previous year balance calculation failed for '{0}' ({1}): {2}", company.Name, company.ActorCompanyId, result.ErrorMessage));
                                }

                                result = alm.CalculateAnnualLeaveTransactions(actorCompanyId, employeeIds, startDate, stopDate);
                                if (result.Success)
                                    CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Calculation finished OK for '{0}' ({1})", company.Name, company.ActorCompanyId));
                                else
                                    CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Calculation failed for '{0}' ({1}): {2}", company.Name, company.ActorCompanyId, result.ErrorMessage));
                            }
                            count++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    result = new ActionResult(ex);
                    CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Error when executing job: '{0}'", result.ErrorMessage));
                    base.LogError(ex);
                }

                // Check in scheduled job
                CheckInScheduledJob(result.Success);
            }

            bool CalculatePreviousYearBalance()
            {
                // if 1:st of January, no need to run previous year separately, since yearly balance calculation is already included on this date.
                if (DateTime.Today.Month == 1 && DateTime.Today.Day == 1)
                    return false;

                DateTime thresholdDate = new DateTime(DateTime.Today.Year, 1, 1).AddDays(recalcPreviousYearDays);
                if (DateTime.Today <= thresholdDate)
                    return true;

                return false;
            }
        }
    }
}
