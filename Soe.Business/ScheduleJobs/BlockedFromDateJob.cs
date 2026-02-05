using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.ScheduledJobs
{
    public class BlockedFromDateJob : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);

            CompanyManager cm = new CompanyManager(parameterObject);
            EmployeeManager em = new EmployeeManager(parameterObject);
            SettingManager sm = new SettingManager(parameterObject);
            LoginManager lm = new LoginManager(parameterObject);
            UserManager um = new UserManager(parameterObject);

            #endregion

            // Get companies to update
            List<int> companyIds = sm.GetCompanyIdsWithCompanyIntSetting(CompanySettingType.BlockFromDateOnUserAfterNrOfDays);
            if (companyIds.IsNullOrEmpty())
            {
                // No companies to run
                return;
            }

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();
            if (result.Success)
            {
                try
                {
                    // Execute job
                    foreach (int actorCompanyId in companyIds)
                    {
                        // Get company name for clearer logging
                        var company = cm.GetCompany(actorCompanyId);
                        string companyName = company.Name;


                        User adminUser = um.GetAdminUser(actorCompanyId);
                        if (adminUser == null)
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Error, $"adminUser null {actorCompanyId}");
                            continue;
                        }

                        ParameterObject param = GetParameterObject(actorCompanyId, adminUser.UserId);
                        em = new EmployeeManager(param);
                        um = new UserManager(param);

                        CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Starting controll of blocked users  {0}", companyName));
                        int? numberOfDays = sm.GetNullableIntSetting(SettingMainType.Company, (int)CompanySettingType.BlockFromDateOnUserAfterNrOfDays, 0, actorCompanyId, 0).ToNullable();

                        if (numberOfDays.HasValue && numberOfDays.Value > 0)
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Checking users blocked from date after {0} days in company {1}", numberOfDays.Value, companyName));
                            List<User> users = um.GetUsersByCompany(actorCompanyId, param.RoleId, param.UserId, includeEnded: true);
                            List<Employee> employees = em.GetAllEmployees(actorCompanyId, loadEmployment: true);
                            var info = lm.BlockedFromDateValidation(users, actorCompanyId, numberOfDays.Value, employees);
                            if (!string.IsNullOrEmpty(info))
                            {
                                CreateLogEntry(ScheduledJobLogLevel.Information, companyName + " Blocked users " + Environment.NewLine + info);
                            }
                            else
                            {
                                CreateLogEntry(ScheduledJobLogLevel.Information, companyName + " No blocked users ");
                            }
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

                    switch (result.ErrorNumber)
                    {
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
