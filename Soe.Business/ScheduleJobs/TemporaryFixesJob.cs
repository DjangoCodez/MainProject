using Microsoft.AspNetCore.Http.Features;
using Newtonsoft.Json;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.TimeEngine;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static SoftOne.Soe.Business.Core.TimeScheduleManager;

namespace SoftOne.Soe.ScheduledJobs
{
    public class TemporaryFixesJob : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SoftOne.Soe.Common.DTO.SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);

            UserManager um = new UserManager(null);
            CompanyManager cm = new CompanyManager(null);

            #endregion

            #region Prereq

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();
            if (!result.Success)
            {
                CreateLogEntry(ScheduledJobLogLevel.Information, "Kunde ej checka ut jobb");
                return;
            }

            CreateLogEntry(ScheduledJobLogLevel.Information, "Startar jobb, försöker hitta parametervärden");
            string methodName = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "methodname").Select(s => s.StrData).FirstOrDefault();
            string actorcompanyIdsString = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "actorcompanyids").Select(s => s.StrData).FirstOrDefault();
            string dateFromString = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "datefrom").Select(s => s.StrData).FirstOrDefault();
            string dateToString = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "dateto").Select(s => s.StrData).FirstOrDefault();
            string paramEmployeeNrs = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "employeenrs").Select(s => s.StrData).FirstOrDefault();
            bool ignoredHandled = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "ignorehandled").Select(s => s.BoolData).FirstOrDefault() ?? false;
            bool updateTemplates = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "updatetemplates").Select(s => s.BoolData).FirstOrDefault() ?? true;
            int reportTemplateId = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "reporttemplateid").Select(s => s.IntData).FirstOrDefault() ?? 0;
            int reportTemplateTemplateCompanyid = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "reporttemplatetemplatecompanyid").Select(s => s.IntData).FirstOrDefault() ?? 0;
            bool doNotSave = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "donotsave").Select(s => s.BoolData).FirstOrDefault() ?? false;
            var extendedFunction = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "extendedfunction").Select(s => s.BoolData).FirstOrDefault() ?? false;

            if (string.IsNullOrEmpty(methodName))
            {
                CreateLogEntry(ScheduledJobLogLevel.Error, "Jobb måste ha parameter 'methodname'");
                return;
            }

            CreateLogEntry(ScheduledJobLogLevel.Information, $"Startar jobb {methodName}");

            #endregion

            bool hasFoundMethod = false;

            #region Methods not requiring parameters actorcompanyid or dates

            #region Method: placementtest

            if (methodName == "placementtest")
            {
                hasFoundMethod = true;

                ConfigurationSetupUtil.Init();
                TimeEngineManager tem = new TimeEngineManager(GetParameterObject(1750, 1211), 1750, 1211);
                List<int> employeeIds = new List<int>();
                List<Employee> employees = null;

                using (CompEntities entities = new CompEntities())
                {
                    employeeIds = entities.Employee.Where(w => w.ActorCompanyId == 1750).Select(s => s.EmployeeId).ToList();
                    var withEmployeeSchedule = entities.EmployeeSchedule.Where(w => employeeIds.Contains(w.EmployeeId)).Select(s => s.EmployeeId).ToList();
                    employeeIds = employeeIds.Where(w => !withEmployeeSchedule.Contains(w)).ToList();
                    employees = entities.Employee.Include("TimeScheduleTemplateHead").Where(w => employeeIds.Contains(w.EmployeeId)).ToList();

                }

                foreach (var group in employees.GroupBy(s => s.TimeScheduleTemplateHead.First().StartDate))
                {
                    var eIds = group.Select(s => s.EmployeeId).ToList();

                    while (eIds.Any())
                    {
                        var ids = eIds.Take(10).ToList();
                        var batch = group.Where(w => ids.Contains(w.EmployeeId)).ToList();
                        List<ActivateScheduleGridDTO> placements = new List<ActivateScheduleGridDTO>();

                        foreach (var emp in batch)
                        {
                            ActivateScheduleGridDTO dto = new ActivateScheduleGridDTO()
                            {
                                EmployeeId = emp.EmployeeId
                            };

                            placements.Add(dto);
                        }

                        tem.SaveEmployeeSchedulePlacement(null, placements, TermGroup_TemplateScheduleActivateFunctions.NewPlacement, group.Key.Value, new DateTime(2020, 12, 31));

                        eIds = eIds.Where(w => !ids.Contains(w)).ToList();
                    }
                }
            }

            #endregion

            #region Method: SetDefaultRoles/SetDefaultRoleCreateUCR

            if (methodName == "SetDefaultRole" || methodName == "SetDefaultRoleCreateUCR")
            {
                hasFoundMethod = true;

                List<Company> companies = cm.GetCompanies().Where(i => i.State != (int)SoeEntityState.Deleted).OrderBy(i => i.LicenseId).ThenBy(i => i.CompanyNr).ToList();
                CreateLogEntry(ScheduledJobLogLevel.Information, $"Antal företag {companies.Count}");

                List<int> licenseIds = companies.Select(i => i.LicenseId).Distinct().ToList();
                CreateLogEntry(ScheduledJobLogLevel.Information, $"Antal licenser {licenseIds.Count}");

                foreach (int licenseId in licenseIds)
                {
                    result = um.RunJobToSetCurrentRole(licenseId, out int noOfUpdatedUsers, out int noOfIgnoredUsers, out int noOfAddedUcr, out bool abortedDueToIgnoreHandled, out List<string> invalidUsers, createNewUserCompanyRoles: methodName == "SetDefaultRoleCreateUCR", ignoredHandled: ignoredHandled);

                    CreateLogEntry(ScheduledJobLogLevel.Information, $"License: {licenseId}. Uppdaterade: {noOfUpdatedUsers}. Ignorerade: {noOfIgnoredUsers}. Felaktiga {invalidUsers.Count}. Skapade UserCompanyRoles: {noOfAddedUcr}");
                    if (abortedDueToIgnoreHandled)
                        CreateLogEntry(ScheduledJobLogLevel.Information, $"License: {licenseId}. Avbröt eftersom licens anses hanterad tidigare");

                    foreach (string invalidUser in invalidUsers.OrderBy(u => u))
                    {
                        CreateLogEntry(ScheduledJobLogLevel.Warning, invalidUser);
                    }

                    if (!result.Success)
                        CreateLogEntry(ScheduledJobLogLevel.Error, $"Fel för licens  {licenseId}: {result.ErrorMessage}");

                }
            }

            #endregion

            #region Method: EmployeeTemplates
            if (methodName == "CreateEmployeeTemplates")
            {

                if (reportTemplateId == 0)
                {
                    CreateLogEntry(ScheduledJobLogLevel.Error, "reportTemplateId == 0");
                    return;
                }

                if (reportTemplateTemplateCompanyid == 0)
                {
                    CreateLogEntry(ScheduledJobLogLevel.Error, "reportTemplateTemplateCompanyid == 0");
                    return;
                }

                using (CompEntities entities = new CompEntities())
                {
                    var actorCompanyIdsWithTemplate = entities.Report.Where(w => w.ReportTemplateId == reportTemplateId).Select(s => s.ActorCompanyId).Distinct().ToList();

                    if (!string.IsNullOrEmpty(actorcompanyIdsString))
                    {
                        actorCompanyIdsWithTemplate = new List<int>();
                        foreach (var idStr in actorcompanyIdsString.Split(','))
                        {
                            if (int.TryParse(idStr, out int id) && id > 0)
                                actorCompanyIdsWithTemplate.Add(id);
                        }
                    }

                    foreach (var id in actorCompanyIdsWithTemplate)
                    {
                        if (id == reportTemplateTemplateCompanyid)
                            continue;

                        User adminUser = GetAdminUser(um, id);
                        if (adminUser == null)
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Error, $"adminUser null {id}");
                            continue;
                        }

                        ParameterObject param = GetParameterObject(id, adminUser.UserId);
                        CompanyManager cm2 = new CompanyManager(param ?? GetParameterObject(id, entities.User.FirstOrDefault(f => f.DefaultActorCompanyId == id)?.UserId ?? 0));
                        cm2.CopyCompanyCollectiveAgreements(id, reportTemplateTemplateCompanyid, null, null, null);
                        cm2.CopyEmployeeTemplates(id, reportTemplateTemplateCompanyid, true, null);
                    }
                }
            }

            #endregion

            #region Method: setAllocationOnEmployeeAccount

            if (methodName == "setAllocationOnEmployeeAccount")
            {
                using (CompEntities entities = new CompEntities())
                {
                    var actorCompanyIdsWithEmployeeAccounts = entities.EmployeeAccount.Select(s => s.ActorCompanyId).Distinct().OrderBy(o => o).ToList();

                    if (!string.IsNullOrEmpty(actorcompanyIdsString))
                    {
                        actorCompanyIdsWithEmployeeAccounts = new List<int>();
                        foreach (var idStr in actorcompanyIdsString.Split(','))
                        {
                            if (int.TryParse(idStr, out int id) && id > 0)
                                actorCompanyIdsWithEmployeeAccounts.Add(id);
                        }
                    }
                    foreach (var actorCompanyId in actorCompanyIdsWithEmployeeAccounts)
                    {
                        CreateLogEntry(ScheduledJobLogLevel.Information, $"Startar företag {actorCompanyId}");
                        ParameterObject param = null;

                        User adminUser = GetAdminUser(um, actorCompanyId);
                        if (adminUser == null)
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Error, $"adminUser null {actorCompanyId}");
                            param = GetParameterObject(actorCompanyId, 4);
                        }
                        else
                        {
                            param = GetParameterObject(actorCompanyId, adminUser.UserId);
                        }


                        var company = cm.GetCompany(actorCompanyId, loadLicense: true);
                        CreateLogEntry(ScheduledJobLogLevel.Information, $"Starting {actorCompanyId} {company.Name}");
                        var employeeIds = entities.EmployeeAccount.Where(w => w.ActorCompanyId == actorCompanyId).Select(s => s.EmployeeId).Distinct().ToList();
                        var employees = entities.Employee.Include("ContactPerson").Where(w => employeeIds.Contains(w.EmployeeId)).ToDictionary(k => k.EmployeeId, v => v);
                        EmployeeManager em = new EmployeeManager(param);
                        var employeeAccounts = em.GetEmployeeAccounts(entities, actorCompanyId, employeeIds, new DateTime(2000, 1, 1), new DateTime(2040, 12, 31));

                        List<EmployeeAccount> updatedEmployeeAccounts = new List<EmployeeAccount>();
                        StringBuilder sbOverlapping = new StringBuilder();
                        sbOverlapping.AppendLine($"Found overlapping on actorcompanyid: [{actorCompanyId} {company.Name}] [{company.License.LicenseNr} {company.License.Name}]");
                        bool logOverlapping = false;

                        if (extendedFunction)
                        {
                            StringBuilder sbMissingDefaults = new StringBuilder();
                            foreach (var employeeAccountOnEmployeeLevel1 in employeeAccounts.Where(w => !w.ParentEmployeeAccountId.HasValue).GroupBy(g => g.EmployeeId))
                            {
                                var employee = employees[employeeAccountOnEmployeeLevel1.Key];

                                if (employee.State == (int)SoeEntityState.Active)
                                {
                                    var lastDate = employeeAccountOnEmployeeLevel1.OrderByDescending(o => o.DateTo ?? DateTime.Today.AddYears(10)).FirstOrDefault()?.DateTo ?? DateTime.Today.AddYears(10);
                                    if (lastDate > DateTime.Today.AddYears(-1))
                                    {
                                        var noDefaultDates = employeeAccountOnEmployeeLevel1.Where(w => !w.Default).ToList().SelectMany(s => CalendarUtility.GetDates(s.DateFrom, s.DateTo ?? DateTime.Today.AddYears(1))).Where(w => w > DateTime.Now.AddYears(-20)).ToList();
                                        var defaultDates = employeeAccountOnEmployeeLevel1.Where(w => w.Default).ToList().SelectMany(s => CalendarUtility.GetDates(s.DateFrom, s.DateTo ?? DateTime.Today.AddYears(1))).Where(w => w > DateTime.Now.AddYears(-20)).ToList();
                                        bool missingOverlappedByDefault = false;

                                        foreach (DateTime date in noDefaultDates)
                                        {
                                            if (!defaultDates.Contains(date))
                                            {
                                                missingOverlappedByDefault = true;
                                                break;
                                            }
                                        }
                                        if (missingOverlappedByDefault)
                                        {
                                            sbMissingDefaults.AppendLine($"{employee.NumberAndName} missing default on all dates");
                                        }
                                    }
                                }
                            }

                            CreateLogEntry(ScheduledJobLogLevel.Information, sbMissingDefaults.ToString());
                        }

                        foreach (var employeeAccountOnEmployee in employeeAccounts.Where(w => !w.ParentEmployeeAccountId.HasValue).GroupBy(g => g.EmployeeId))
                        {
                            if (employeeAccountOnEmployee.Any(a => a.MainAllocation))
                                continue;

                            var employee = employees[employeeAccountOnEmployee.Key];

                            foreach (var employeeAccount in employeeAccountOnEmployee.Where(w => w.Default))
                            {
                                var overlappning = false;

                                foreach (var employeeAccountInner in employeeAccountOnEmployee.Where(w => w.EmployeeAccountId != employeeAccount.EmployeeAccountId))
                                {
                                    if (CalendarUtility.IsDatesOverlappingNullable(employeeAccountInner.DateFrom, employeeAccountInner.DateTo, employeeAccount.DateFrom, employeeAccount.DateTo))
                                    {
                                        overlappning = true;
                                        break;
                                    }
                                }

                                if (!overlappning)
                                {
                                    employeeAccount.MainAllocation = true;
                                    employeeAccount.CreatedBy = "RD240423";
                                    employeeAccount.Created = DateTime.Now;
                                    updatedEmployeeAccounts.Add(employeeAccount);
                                }
                                else
                                {
                                    logOverlapping = true;
                                    sbOverlapping.AppendLine($"{employee.NumberAndName} overlapping {employeeAccount.DateFrom.ToShortDateString()}");
                                }
                            }
                        }

                        if (logOverlapping)
                            CreateLogEntry(ScheduledJobLogLevel.Information, sbOverlapping.ToString());
                        if (!doNotSave)
                            entities.BulkUpdate(updatedEmployeeAccounts);
                        CreateLogEntry(ScheduledJobLogLevel.Information, $"Done with {actorCompanyId} {company.Name} Updated count {updatedEmployeeAccounts.Count()}");

                    }
                }
            }

            #endregion

            #endregion

            List<int> actorCompanyIds = new List<int>();
            DateTime dateFrom = CalendarUtility.DATETIME_DEFAULT;
            DateTime dateTo = CalendarUtility.DATETIME_DEFAULT;

            if (!hasFoundMethod)
            {
                #region ActorCompanyIds

                if (!string.IsNullOrEmpty(actorcompanyIdsString))
                {
                    foreach (var idStr in actorcompanyIdsString.Split(','))
                    {
                        if (int.TryParse(idStr, out int id) && id > 0)
                            actorCompanyIds.Add(id);
                    }

                    if (!actorCompanyIds.Any())
                    {
                        CreateLogEntry(ScheduledJobLogLevel.Error, "Parameter 'actorcompanyids' är ogiltig");
                        return;
                    }
                }
                CreateLogEntry(ScheduledJobLogLevel.Information, $"Antal företag {actorCompanyIds.Count}");

                #endregion

                #region Dates

                if (!string.IsNullOrEmpty(dateFromString) && !DateTime.TryParse(dateFromString, out dateFrom))
                {
                    CreateLogEntry(ScheduledJobLogLevel.Error, "Parameter 'datefrom' är ogiltig");
                    return;
                }
                if (!string.IsNullOrEmpty(dateToString) && !DateTime.TryParse(dateToString, out dateTo))
                {
                    CreateLogEntry(ScheduledJobLogLevel.Error, "Parameter 'dateto' är ogiltig");
                    return;
                }
                if (dateFrom > dateTo)
                {
                    CreateLogEntry(ScheduledJobLogLevel.Error, "Parameter 'datefrom' får inte vara större än parameter 'dateto'");
                    return;
                }

                #endregion
            }

            #region

            if (methodName == "RestoreToScheduleWhenPlacementFailed")
            {
                foreach (var actorCompanyId in actorCompanyIds)
                {
                    using (CompEntities entities = new CompEntities())
                    {
                        #region Prereq

                        User adminUser = GetAdminUser(um, actorCompanyId);
                        if (adminUser == null)
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Error, $"adminUser null {actorCompanyId}");
                        }
                        var param = GetParameterObject(actorCompanyId, adminUser.UserId);
                        TimeBlockManager timeBlockManager = new TimeBlockManager(param);

                        #endregion

                        #region find employee and dates affected

                        var timeScheduleTemplateBlocks = entities.TimeScheduleTemplateBlock.Where(w => !w.TimeDeviationCauseId.HasValue && !w.TimeScheduleScenarioHeadId.HasValue && w.EmployeeId.HasValue && w.Date.HasValue && w.Employee.ActorCompanyId == actorCompanyId && w.Date.Value >= dateFrom && w.Date.Value <= dateTo && w.State == 0).ToList();
                        var timeScheduleTemplateBlocksByEmployee = timeScheduleTemplateBlocks.Where(W => W.StartTime != W.StopTime).GroupBy(g => g.EmployeeId).ToDictionary(k => k.Key.Value, v => v.ToList());
                        CreateLogEntry(ScheduledJobLogLevel.Information, $"Hämtning klar schemablock {actorCompanyId}");

                        var timeblocks = entities.TimeBlock.Include("TimeBlockDate").Where(w => w.Employee.ActorCompanyId == actorCompanyId && w.TimeBlockDate.Date >= dateFrom && w.TimeBlockDate.Date <= dateTo && w.State == 0).ToList();
                        var timeblocksByEmployee = timeblocks.GroupBy(g => g.EmployeeId).ToDictionary(k => k.Key, v => v.ToList());
                        CreateLogEntry(ScheduledJobLogLevel.Information, $"Hämtning klar tidblock {actorCompanyId}");

                        var timeStamps = entities.TimeStampEntry.Where(w => w.ActorCompanyId == actorCompanyId && w.Time >= dateFrom && w.Time <= dateTo && w.State == 0).ToList();
                        var timeStampsByEmployee = timeStamps.GroupBy(g => g.EmployeeId).ToDictionary(k => k.Key, v => v.ToList());
                        CreateLogEntry(ScheduledJobLogLevel.Information, $"Hämtning klar stämplingar {actorCompanyId} antal anställda att kontrollera {timeScheduleTemplateBlocksByEmployee.Count}");

                        Dictionary<int, List<DateTime>> affected = new Dictionary<int, List<DateTime>>();
                        int counter = 0;
                        foreach (var timeScheduleTemplateBlockEmployee in timeScheduleTemplateBlocksByEmployee)
                        {
                            counter++;
                            List<DateTime> dates = new List<DateTime>();

                            if (counter % 300 == 0)
                                CreateLogEntry(ScheduledJobLogLevel.Information, $"{counter} anställda kontrollerade på företag {actorCompanyId} antal hittade {affected.Count}");

                            if (timeStampsByEmployee.ContainsKey(timeScheduleTemplateBlockEmployee.Key))
                                continue;

                            timeblocksByEmployee.TryGetValue(timeScheduleTemplateBlockEmployee.Key, out List<TimeBlock> timeBlocksEmployee);

                            foreach (DateTime date in timeScheduleTemplateBlockEmployee.Value.Where(b => b.Date.HasValue).GroupBy(g => g.Date.Value).Select(k => k.Key))
                            {
                                if (timeBlocksEmployee == null || !timeBlocksEmployee.Any(a => a.EmployeeId == timeScheduleTemplateBlockEmployee.Key && a.TimeBlockDate.Date == date))
                                    dates.Add(date);
                            }

                            if (dates.Any())
                                affected.Add(timeScheduleTemplateBlockEmployee.Key, dates);
                        }

                        #endregion

                        #region SetTimeBlockDates and restore

                        TimeEngineManager tem = new TimeEngineManager(param, actorCompanyId, adminUser.UserId);
                        int affectedCounter = 0;
                        foreach (var e in affected)
                        {
                            affectedCounter++;

                            if (affectedCounter % 10 == 0)
                                CreateLogEntry(ScheduledJobLogLevel.Information, $"{affectedCounter} anställda behandlade på företag {actorCompanyId} antal hittade {affected.Count}");

                            var timeBlockDates = timeBlockManager.GetTimeBlockDates(entities, e.Key, e.Value);
                            List<AttestEmployeeDaySmallDTO> attestEmployeeDays = new List<AttestEmployeeDaySmallDTO>();
                            foreach (var timeBlockDate in timeBlockDates)
                            {
                                attestEmployeeDays.Add(new AttestEmployeeDaySmallDTO(e.Key, timeBlockDate.Date, timeBlockDate.TimeBlockDateId, null));
                            }
                            tem.RestoreDaysToSchedule(attestEmployeeDays);
                        }

                        #endregion
                    }
                }
            }


            #endregion



            if (methodName == "UnbalancedAbsence")
            {
                List<int> companyIds = new List<int>();
                if (actorCompanyIds.Any())
                {
                    companyIds = actorCompanyIds;
                }
                using (CompEntities entities = new CompEntities())
                {
                    companyIds = entities.Company.Include("License").Where(w => w.License.LicenseNr == "8000").Select(s => s.ActorCompanyId).ToList();
                }
                StringBuilder sb = new StringBuilder();

                foreach (var actorCompanyId in companyIds)
                {
                    using (CompEntities entities = new CompEntities())
                    {
                        StringBuilder csb = new StringBuilder();
                        CreateLogEntry(ScheduledJobLogLevel.Information, $"Startar företag {actorCompanyId}");

                        ParameterObject param = null;

                        User adminUser = GetAdminUser(um, actorCompanyId);
                        if (adminUser == null)
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Error, $"adminUser null {actorCompanyId}");
                            param = GetParameterObject(actorCompanyId, 4);
                        }
                        else
                        {
                            param = GetParameterObject(actorCompanyId, adminUser.UserId);
                        }

                        var company = cm.GetCompany(actorCompanyId);
                        AttestManager am = new AttestManager(param);
                        SettingManager sm = new SettingManager(param);

                        List<int> excludeAttestStateIds = new List<int>()
                        {
                            sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.SalaryExportPayrollResultingAttestStatus, param.UserId, param.ActorCompanyId, 0),
                            sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.SalaryPaymentLockedAttestStateId, param.UserId, param.ActorCompanyId, 0),
                            sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.SalaryPaymentApproved1AttestStateId, param.UserId, param.ActorCompanyId, 0),
                            sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.SalaryPaymentApproved2AttestStateId, param.UserId, param.ActorCompanyId, 0),
                            sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.SalaryPaymentExportFileCreatedAttestStateId, param.UserId, param.ActorCompanyId, 0),
                        };
                        TimeScheduleManager tsm = new TimeScheduleManager(param);

                        var fromDate = dateFrom == CalendarUtility.DATETIME_DEFAULT ? new DateTime(2022, 11, 1) : dateFrom;
                        var toDate = dateTo == CalendarUtility.DATETIME_DEFAULT ? new DateTime(2022, 11, 30) : dateTo;
                        var attestStates = entities.AttestState.Where(w => w.ActorCompanyId == param.ActorCompanyId).ToList();
                        var employees = entities.Employee.Where(w => w.State == 0 && w.ActorCompanyId == actorCompanyId).ToList();
                        var employeeIds = entities.TimeScheduleTemplateBlock.Where(w => w.EmployeeId.HasValue && w.Date.HasValue && w.Date >= fromDate && w.TimeCode.ActorCompanyId == actorCompanyId).Select(s => s.EmployeeId.Value).Distinct().ToList();
                        employeeIds = employeeIds.Take(1000).ToList();
                        var shifts = tsm.GetAllActiveTimeScheduleTemplateBlocks(employeeIds, fromDate, toDate);
                        var deviationCauses = entities.TimeDeviationCause.Where(w => w.State == 0 && w.ActorCompanyId == actorCompanyId && w.AdjustTimeInsideOfPlannedAbsence > 0).Select(s => s.TimeDeviationCauseId).ToList();
                        var timeblocks = entities.TimeBlock.Include("TimeBlockDate").Where(w => employeeIds.Contains(w.EmployeeId) && w.TimeBlockDate.Date >= fromDate && w.TimeBlockDate.Date <= toDate && w.State == 0).ToList();
                        var initialAttestStatePayroll = am.GetInitialAttestState(param.ActorCompanyId, TermGroup_AttestEntity.PayrollTime);

                        foreach (var shiftsOnEmployee in shifts.GroupBy(g => g.EmployeeId))
                        {
                            foreach (var shiftsOnDay in shiftsOnEmployee.GroupBy(g => g.Date))
                            {
                                if (shiftsOnDay.Any(a => a.TimeDeviationCauseId.HasValue && deviationCauses.Contains(a.TimeDeviationCauseId.Value)))
                                {
                                    var timeblocksOnday = timeblocks.Where(w => w.EmployeeId == shiftsOnEmployee.Key && w.TimeBlockDate.Date == shiftsOnDay.Key).ToList();

                                    var employee = employees.FirstOrDefault(w => w.EmployeeId == shiftsOnEmployee.Key);

                                    foreach (var plannedAbsenceShifts in shiftsOnDay.Where(w => w.TimeDeviationCauseId.HasValue && deviationCauses.Contains(w.TimeDeviationCauseId.Value)).GroupBy(g => g.TimeDeviationCauseId))
                                    {
                                        int shiftTimePlannedAbsenceMinutes = 0;

                                        var breaks = shiftsOnDay.Where(w => w.IsBreak).ToList();

                                        foreach (var plannedAbsenceShift in plannedAbsenceShifts.Where(w => !w.IsBreak))
                                            shiftTimePlannedAbsenceMinutes += Convert.ToInt16((plannedAbsenceShift.StopTime - plannedAbsenceShift.StartTime).TotalMinutes - plannedAbsenceShift.GetOverlappedBreaks(breaks, includeBreaksThatOnlyStartInScheduleBlock: true).Sum(s => s.TotalMinutes));

                                        int timeblockAbsenceMinutes = timeblocksOnday.Where(w => w.TimeDeviationCauseStartId == plannedAbsenceShifts.Key).Sum(s => s.TotalMinutes);

                                        if (shiftTimePlannedAbsenceMinutes != timeblockAbsenceMinutes)
                                        {
                                            var timeBlockDate = entities.TimeBlockDate.FirstOrDefault(f => f.EmployeeId == shiftsOnEmployee.Key && f.Date == shiftsOnDay.Key);

                                            if (timeBlockDate != null)
                                            {
                                                using (CompEntities entitiesDay = new CompEntities())
                                                {
                                                    var timeStampEntries = entitiesDay.TimeStampEntry.Where(w => w.EmployeeId == shiftsOnEmployee.Key && w.TimeBlockDateId == timeBlockDate.TimeBlockDateId && w.State == (int)SoeEntityState.Active).ToList();
                                                    if (timeStampEntries.Any())
                                                    {
                                                        var transactions = entitiesDay.TimePayrollTransaction.Where(w => w.TimeBlockDateId == timeBlockDate.TimeBlockDateId && w.State == (int)SoeEntityState.Active).ToList();

                                                        if (transactions.Any())
                                                        {
                                                            var currentAttestStateId = transactions.First().AttestStateId;
                                                            var attestStateNames = string.Join(",", attestStates.Where(s => transactions.Select(ss => ss.AttestStateId).Contains(s.AttestStateId)).Distinct().Select(s => s.Name).ToList());

                                                            if (!transactions.Any(a => excludeAttestStateIds.Contains(a.AttestStateId)) && transactions.GroupBy(g => g.AttestStateId).Count() == 1)
                                                            {
                                                                currentAttestStateId = transactions.First().AttestStateId;
                                                                transactions.ForEach(f => f.AttestStateId = initialAttestStatePayroll.AttestStateId);
                                                                entitiesDay.SaveChanges();

                                                                TimeEngineManager tem = new TimeEngineManager(param, param.ActorCompanyId, param.UserId, entitiesDay);
                                                                tem.ReGenerateDayBasedOnTimeStamps(timeStampEntries);

                                                                transactions = entitiesDay.TimePayrollTransaction.Where(w => w.TimeBlockDateId == timeBlockDate.TimeBlockDateId && w.State == (int)SoeEntityState.Active).ToList();
                                                                transactions.ForEach(f => f.AttestStateId = currentAttestStateId);
                                                                entitiesDay.SaveChanges();
                                                                csb.Append($"{company.Name}#{employee.EmployeeNr}#{shiftsOnDay.Key.ToShortDateString()}#{shiftTimePlannedAbsenceMinutes}#{timeblockAbsenceMinutes}#HandledResult{result.Success}_" + attestStateNames + Environment.NewLine);
                                                            }
                                                            else
                                                                csb.Append($"{company.Name}#{employee.EmployeeNr}#{shiftsOnDay.Key.ToShortDateString()}#{shiftTimePlannedAbsenceMinutes}#{timeblockAbsenceMinutes}#UnHandledMixedAttest_" + attestStateNames + Environment.NewLine);
                                                        }
                                                        else
                                                            csb.Append($"{company.Name}#{employee.EmployeeNr}#{shiftsOnDay.Key.ToShortDateString()}#{shiftTimePlannedAbsenceMinutes}#{timeblockAbsenceMinutes}#UnHandledNoTransactions" + Environment.NewLine);
                                                    }
                                                }
                                            }
                                            else
                                                csb.Append($"{company.Name}#{employee.EmployeeNr}#{shiftsOnDay.Key.ToShortDateString()}#{shiftTimePlannedAbsenceMinutes}#{timeblockAbsenceMinutes}#UnHandledNoTimeBlockDate" + Environment.NewLine);
                                        }
                                    }
                                }
                            }
                        }

                        var ctext = csb.ToString();
                        sb.Append(ctext);
                        CreateLogEntry(ScheduledJobLogLevel.Information, ctext);
                    }
                    string text = sb.ToString();
                    CreateLogEntry(ScheduledJobLogLevel.Information, text);
                }
            }

            if (methodName == "FindOverlappingEmployments")
            {
                var companies = cm.GetCompanies().Where(w => w.State == 0);
                if (actorCompanyIds.IsNullOrEmpty())
                    actorCompanyIds = cm.GetCompanies().Where(w => w.State == 0).Select(s => s.ActorCompanyId).ToList();

                foreach (var actorCompanyId in actorCompanyIds)
                {
                    var company = companies.FirstOrDefault(f => f.ActorCompanyId == actorCompanyId);
                    CreateLogEntry(ScheduledJobLogLevel.Information, $"Startar företag {actorCompanyId}");
                    ParameterObject param = null;
                    User adminUser = GetAdminUser(um, actorCompanyId);
                    if (adminUser == null)
                    {
                        CreateLogEntry(ScheduledJobLogLevel.Error, $"adminUser null {actorCompanyId}");
                        param = GetParameterObject(actorCompanyId, 4);
                    }
                    else
                    {
                        param = GetParameterObject(actorCompanyId, adminUser.UserId);
                    }
                    EmployeeManager employeeManager = new EmployeeManager(param);
                    var employees = employeeManager.GetAllEmployees(actorCompanyId, loadEmployment: true, loadContact: true);
                    Dictionary<string, Tuple<List<Employment>, List<Employment>>> dict = new Dictionary<string, Tuple<List<Employment>, List<Employment>>>();
                    DateTime defaultEndDate = DateTime.Today.AddYears(100);

                    foreach (var employee in employees)
                    {
                        var employeeInfoString = $"{employee.EmployeeNr} {employee.FirstName} {employee.LastName}";
                        var employments = employee.GetActiveEmployments();
                        var overlapping = employments
                            .Where(e1 => employments.Any(e2 =>
                            e1.DateFrom.HasValue && e2.DateFrom.HasValue &&
                                e1.EmploymentId != e2.EmploymentId && CalendarUtility.GetOverlappingMinutes(e1.DateFrom.Value, e1.GetEndDate() ?? defaultEndDate, e2.DateFrom.Value, e2.GetEndDate() ?? defaultEndDate) > 0))
                            .ToList();
                        if (overlapping.Any())
                        {
                            dict.Add(employeeInfoString, Tuple.Create(employments, overlapping));
                        }
                    }

                    if (dict.Any())
                    {
                        CreateLogEntry(ScheduledJobLogLevel.Information, $"Företag {actorCompanyId} {company.Name} har {dict.Count} anställda med överlappande anställningar");
                        var stringBuilder = new StringBuilder();
                        stringBuilder.AppendLine("--------------");
                        foreach (var item in dict)
                        {
                            stringBuilder.AppendLine(item.Key);
                            stringBuilder.AppendLine("Alla anställningar");
                            foreach (var employment in item.Value.Item1)
                            {
                                stringBuilder.AppendLine($"    {employment.DateFrom} - {employment.DateTo}");
                            }
                            stringBuilder.AppendLine("Överlappar andra anställningar");
                            foreach (var employment in item.Value.Item2)
                            {
                                stringBuilder.AppendLine($"    {employment.DateFrom} - {employment.DateTo}");
                            }
                            stringBuilder.AppendLine(" ");
                        }

                        stringBuilder.AppendLine("--------------");

                        CreateLogEntry(ScheduledJobLogLevel.Information, stringBuilder.ToString());
                    }
                }
            }

            #region Methods requiring parameters actorcompanyid and dates

            #region Method change parentAccount

            if (methodName == "changeParentAccounsAfterHierarchyChange")
            {

                if (actorCompanyIds.Count > 1)
                {
                    CreateLogEntry(ScheduledJobLogLevel.Error, "Parameter 'actorcompanyids' får endast innehålla ett värde");
                    return;
                }

                if (dateFrom == CalendarUtility.DATETIME_DEFAULT)
                {
                    CreateLogEntry(ScheduledJobLogLevel.Error, "Parameter 'datefrom' är obligatorisk");
                    return;
                }

                string changeParents = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "accountparents").Select(s => s.StrData).FirstOrDefault() ?? string.Empty;

                if (string.IsNullOrEmpty(changeParents))
                {
                    CreateLogEntry(ScheduledJobLogLevel.Error, "Parameter 'accountParents' är obligatorisk");
                    return;
                }

                List<AccountParentDTO> accountParents = JsonConvert.DeserializeObject<List<AccountParentDTO>>(changeParents); //

                TimeScheduleManager timeScheduleManager = new TimeScheduleManager(GetParameterObject(actorCompanyIds.First(), 4));
                using (CompEntities entities = new CompEntities())
                {
                    timeScheduleManager.ChangeParentAccounsAfterHierarchyChange(entities, accountParents, actorCompanyIds.First(), dateFrom, dateTo, true, extendedFunction);
                }
            }


            #endregion

            #region Method: createAccountingOnTemplateBlocksAccordingToShiftType

            if (methodName == "createAccountingOnTemplateBlocksAccordingToShiftType")
            {
                foreach (var actorCompanyId in actorCompanyIds)
                {
                    CreateLogEntry(ScheduledJobLogLevel.Information, $"Startar företag {actorCompanyId}");

                    ParameterObject param = null;

                    User adminUser = GetAdminUser(um, actorCompanyId);
                    if (adminUser == null)
                    {
                        CreateLogEntry(ScheduledJobLogLevel.Error, $"adminUser null {actorCompanyId}");
                        param = GetParameterObject(actorCompanyId, 4);
                    }
                    else
                    {
                        param = GetParameterObject(actorCompanyId, adminUser.UserId);
                    }

                    TimeScheduleManager tsm = new TimeScheduleManager(param);

                    if (string.IsNullOrEmpty(paramEmployeeNrs))
                        tsm.ResetAccountingAccordingToShiftType(actorCompanyId, dateFrom, dateTo, updateTemplates: updateTemplates);
                    else
                    {
                        var employeeNrs = paramEmployeeNrs.Split(',').ToList();

                        EmployeeManager em = new EmployeeManager(param);
                        var employeeIds = em.GetAllEmployeeIdsByEmployeeNr(actorCompanyId, employeeNrs);
                        tsm.ResetAccountingAccordingToShiftType(actorCompanyId, dateFrom, dateTo, employeeIds: employeeIds, updateTemplates: updateTemplates);
                    }
                }
            }

            #endregion

            #region Resend for missing signartures

            if (methodName == "resendsignatures")
            {
                foreach (var actorCompanyId in actorCompanyIds)
                {
                    CreateLogEntry(ScheduledJobLogLevel.Information, $"Startar företag {actorCompanyId}");

                    ParameterObject param = null;

                    User adminUser = GetAdminUser(um, actorCompanyId);
                    if (adminUser == null)
                    {
                        CreateLogEntry(ScheduledJobLogLevel.Error, $"adminUser null {actorCompanyId}");
                        param = GetParameterObject(actorCompanyId, 4);
                    }
                    else
                    {
                        param = GetParameterObject(actorCompanyId, adminUser.UserId);
                    }

                    AttestManager attestManager = new AttestManager(param);

                    using (CompEntities entities = new CompEntities())
                    {
                        var company = cm.GetCompany(entities, actorCompanyId);
                        var res = attestManager.ResendAndExtendIfError(entities, ConfigurationSetupUtil.GetCurrentSysCompDbId(), actorCompanyId, company.LicenseId, dateFrom, dateTo, false);
                        CreateLogEntry(ScheduledJobLogLevel.Information, $"{res.InfoMessage}");
                    }
                }
            }


            #endregion

            #region Method: createBreaks

            if (methodName == "createBreaks")
            {
                foreach (var actorCompanyId in actorCompanyIds)
                {
                    CreateLogEntry(ScheduledJobLogLevel.Information, $"Startar företag {actorCompanyId}");

                    ParameterObject param = null;

                    User adminUser = GetAdminUser(um, actorCompanyId);
                    if (adminUser == null)
                    {
                        CreateLogEntry(ScheduledJobLogLevel.Error, $"adminUser null {actorCompanyId}");
                        param = GetParameterObject(actorCompanyId, 4);
                    }
                    else
                    {
                        param = GetParameterObject(actorCompanyId, adminUser.UserId);
                    }

                    EmployeeManager em = new EmployeeManager(param);
                    List<Employee> employees = em.GetAllEmployees(actorCompanyId);

                    foreach (Employee employee in employees)
                    {
                        try
                        {
                            TimeScheduleManager tsm = new TimeScheduleManager(GetParameterObject(actorCompanyId, 4));
                            CreateLogEntry(ScheduledJobLogLevel.Information, $"Startar anställd  {employee.EmployeeNr} på företag {actorCompanyId}");
                            tsm.CreateAndSaveBreaksFromTemplatesForEmployees(dateFrom, dateTo, new List<int>() { employee.EmployeeId }, actorCompanyId);
                        }
                        catch (Exception ex)
                        {
                            CreateLogEntry(ScheduledJobLogLevel.Error, $"Fel för anställd  {employee.EmployeeNr} på företag {actorCompanyId}: {ex}");
                            base.LogError(ex);
                        }
                    }
                }
            }

            #endregion

            #region Method: generateFreq

            if (methodName == "generateFreq")
            {
                foreach (var actorCompanyId in actorCompanyIds)
                {
                    CreateLogEntry(ScheduledJobLogLevel.Information, $"Startar företag {actorCompanyId}");

                    ParameterObject param = null;

                    User adminUser = GetAdminUser(um, actorCompanyId);
                    if (adminUser == null)
                    {
                        CreateLogEntry(ScheduledJobLogLevel.Error, $"adminUser null {actorCompanyId}");
                        param = GetParameterObject(actorCompanyId, 4);
                    }
                    else
                    {
                        param = GetParameterObject(actorCompanyId, adminUser.UserId);
                    }

                    TimeScheduleManager tsm = new TimeScheduleManager(param);
                    tsm.CreateFakeStaffingneedsFrequancy(actorCompanyId, dateFrom, dateTo);
                }
            }

            #endregion

            #region Method: CreateTimeStamps

            if (methodName == "CreateTimeStamps")
            {
                foreach (var actorCompanyId in actorCompanyIds)
                {
                    CreateLogEntry(ScheduledJobLogLevel.Information, $"Startar företag {actorCompanyId}");

                    ParameterObject param = null;

                    User adminUser = GetAdminUser(um, actorCompanyId);
                    if (adminUser == null)
                    {
                        CreateLogEntry(ScheduledJobLogLevel.Error, $"adminUser null {actorCompanyId}");
                        param = GetParameterObject(actorCompanyId, 4);
                    }
                    else
                    {
                        param = GetParameterObject(actorCompanyId, adminUser.UserId);
                    }

                    TimeStampManager tsm = new TimeStampManager(param);
                    tsm.CreateFakeTimeStamps(actorCompanyId, dateFrom, dateTo);
                    tsm.ConvertTimeStampsToTimeBlocks(actorCompanyId);
                }
            }

            #endregion

            #region Method: CreateSalesBudget/CreateSalesBudgetTime

            if (methodName == "CreateSalesBudget" || methodName == "CreateSalesBudgetTime")
            {
                foreach (var actorCompanyId in actorCompanyIds)
                {
                    CreateLogEntry(ScheduledJobLogLevel.Information, $"Startar företag {actorCompanyId}");

                    ParameterObject param = null;

                    User adminUser = GetAdminUser(um, actorCompanyId);
                    if (adminUser == null)
                    {
                        CreateLogEntry(ScheduledJobLogLevel.Error, $"adminUser null {actorCompanyId})");
                        param = GetParameterObject(actorCompanyId, 4);
                    }
                    else
                    {
                        param = GetParameterObject(actorCompanyId, adminUser.UserId);
                    }

                    BudgetManager bm = new BudgetManager(param);
                    if (methodName == "CreateSalesBudget")
                        bm.CreateBudgetFromSales(actorCompanyId, dateFrom, dateTo);
                    else
                        bm.CreateBudgetTimeFromSchedule(actorCompanyId, dateFrom, dateTo);
                }
            }

            #endregion

            #endregion

            CreateLogEntry(ScheduledJobLogLevel.Information, "Avslutar: " + methodName);

            CheckInScheduledJob(true);
        }

        private User GetAdminUser(UserManager um, int actorCompanyId)
        {
            return um.GetAdminUser(actorCompanyId) ?? um.GetUsersByCompany(actorCompanyId, 0, scheduledJob.ExecuteUserId).FirstOrDefault();
        }
    }
}
