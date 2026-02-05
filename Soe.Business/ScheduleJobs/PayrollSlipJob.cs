using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.Reporting;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SoftOne.Soe.ScheduledJobs
{
    public class PayrollSlipJob : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);
            GeneralManager gm = new GeneralManager(parameterObject);
            UserManager um = new UserManager(parameterObject);
            CompanyManager cm = new CompanyManager(parameterObject);
            ReportManager rm = new ReportManager(parameterObject);
            int? paramCompanyId = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "actorcompanyid").Select(s => s.IntData).FirstOrDefault();

            #endregion

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();
            if (result.Success)
            {
                try
                {
                    using (CompEntities entities = new CompEntities())
                    {
                        DateTime from = DateTime.Now.AddYears(-5);
                        var companiesWithPayrollSlips = paramCompanyId.HasValue ? new List<int>(paramCompanyId.Value) : entities.DataStorage.Where(w => w.TimePeriodId.HasValue && w.EmployeeId.HasValue && w.Created.HasValue && w.Created > from).Select(s => s.ActorCompanyId).Distinct().ToList();

                        entities.CommandTimeout = 30000;
                        int count = 0;
                        int total = companiesWithPayrollSlips.Count;
                        int totalMBsaved = 0;

                        foreach (var actorCompanyId in companiesWithPayrollSlips.OrderBy(o => o))
                        {
                            count++;
                            var company = entities.Company.Include("License").First(f => f.ActorCompanyId == actorCompanyId);

                            if (company.State != (int)SoeEntityState.Active || company.License.State != (int)SoeEntityState.Active)
                                continue;

                            var periodIdDataStorageCombos = entities.DataStorage.Where(w => w.ActorCompanyId == actorCompanyId && w.TimePeriodId.HasValue && w.EmployeeId.HasValue && w.Type == (int)SoeDataStorageRecordType.PayrollSlipXML && w.DataCompressed == null && w.XMLCompressed != null && w.Created.HasValue && w.Created > from).Select(s => new { s.TimePeriodId, s.DataStorageId, s.EmployeeId }).ToList();
                            if (!periodIdDataStorageCombos.Any())
                                continue;

                            User user = um.GetAdminUser(actorCompanyId) ?? um.GetUser(scheduledJob.ExecuteUserId);

                            if (user == null)
                                continue;

                            var report = rm.GetCompanySettingReport(entities, SettingMainType.Company, CompanySettingType.DefaultPayrollSlipReport, SoeReportTemplateType.PayrollSlip, actorCompanyId, 0, null);

                            if (report == null)
                            {
                                CreateLogEntry(ScheduledJobLogLevel.Information, $"{company.ActorCompanyId} # {company.Name} No report found");
                                continue;
                            }

                            var template = rm.GetReportTemplate(entities, report.ReportTemplateId, actorCompanyId)?.Template;

                            if (template == null)
                                template = rm.GetSysReportTemplate(report.ReportTemplateId)?.Template;

                            if (template == null)
                            {
                                CreateLogEntry(ScheduledJobLogLevel.Information, $"{company.ActorCompanyId} # {company.Name} No template found");
                                continue;
                            }

                            parameterObject.SetSoeUser(um.GetSoeUser(actorCompanyId, user));
                            parameterObject.SetSoeCompany(cm.GetSoeCompany(company));
                            parameterObject.SetActiveRoleId(user.DefaultRoleId);

                            TimeReportDataManager rdp = new TimeReportDataManager(parameterObject);
                            CreateLogEntry(ScheduledJobLogLevel.Information, $"{company.ActorCompanyId} # {company.Name} started with {periodIdDataStorageCombos.Count} storage count. {count}/{total}] ");
                            int internalCount = 0;

                            foreach (var timePeriodIdGroup in periodIdDataStorageCombos.Where(w => w.TimePeriodId.HasValue).GroupBy(g => g.TimePeriodId.Value))
                            {
                                var timePeriod = entities.TimePeriod.FirstOrDefault(f => f.TimePeriodId == timePeriodIdGroup.Key);
                                if (timePeriod == null)
                                    continue;

                                List<int> employeeIds = timePeriodIdGroup.Where(w => w.EmployeeId.HasValue).Select(s => s.EmployeeId.Value).ToList();

                                var employees = entities.Employee.Include("ContactPerson").Where(w => employeeIds.Contains(w.EmployeeId)).ToList();

                                foreach (var item in timePeriodIdGroup)
                                {
                                    internalCount++;
                                    var employee = employees.FirstOrDefault(f => f.EmployeeId == item.EmployeeId.Value);
                                    var printResult = rdp.CreatePdfPayrollSlip(entities, actorCompanyId, timePeriod, employee, report, template);

                                    if (printResult != null && printResult.Success && printResult.GeneratedReport != null)
                                    {
                                        var storage = entities.DataStorage.FirstOrDefault(f => f.DataStorageId == item.DataStorageId);

                                        if (storage != null)
                                        {
                                            storage.Data = printResult.GeneratedReport;
                                            gm.CompressStorage(entities, storage);
                                        }
                                    }
                                }
                            }

                            Thread.Sleep(internalCount * 2); // slowing down process in order to not overexhausted transactionslogs
                            CreateLogEntry(ScheduledJobLogLevel.Information, $"{company.ActorCompanyId} # {company.Name} done. {Convert.ToInt32(decimal.Divide(internalCount, 1024))} MB gained");
                        }

                        CreateLogEntry(ScheduledJobLogLevel.Information, $"Job done.{totalMBsaved} MB gained during job");
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
