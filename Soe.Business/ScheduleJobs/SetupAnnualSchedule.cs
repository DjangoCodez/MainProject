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
    public class SetupAnnualSchedule : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);

            CompanyManager cm = new CompanyManager(parameterObject);
            SettingManager sm = new SettingManager(parameterObject);
            TimeScheduleManager tsm = new TimeScheduleManager(parameterObject);

            int? paramCompanyId = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "actorcompanyid").Select(s => s.IntData).FirstOrDefault();

            #endregion

            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();

            if (result.Success)
            {
                try
                {
                    var companyIds = sm.GetCompanyIdsWithCompanyBoolSetting(CompanySettingType.TimeCalculatePlanningPeriodScheduledTime);
                    var companies = base.GetCompanies(paramCompanyId);

                    // Execute job
                    CreateLogEntry(ScheduledJobLogLevel.Information, "Startar uppsättning av planningsperioder");
                    using (CompEntities entities = new CompEntities())
                    {
                        var companysWithSetting = entities.EmployeeGroup.Where(w => w.RuleWorkTimeYear2018 != 0 || w.RuleWorkTimeYear2019 != 0).Select(s => s.ActorCompanyId).Distinct().ToList();
                        companies = companies.Where(w => companysWithSetting.Contains(w.ActorCompanyId)).ToList();
                        string createdBy = "SoftOneSystem23";

                        foreach (var company in companies)
                        {
                            TimePeriodHead head = new TimePeriodHead()
                            {
                                ActorCompanyId = company.ActorCompanyId,
                                Name = "År",
                                CreatedBy = createdBy,
                                TimePeriodType = (int)TermGroup_TimePeriodType.RuleWorkTime
                            };

                            entities.TimePeriodHead.AddObject(head);
                            entities.SaveChanges();

                            UserCompanySetting setting = new UserCompanySetting()
                            {
                                ActorCompanyId = company.ActorCompanyId,
                                IntData = head.TimePeriodHeadId,
                                SettingTypeId = (int)CompanySettingType.TimeDefaultPlanningPeriod,
                                DataTypeId = (int)SettingDataType.Integer,                                
                            };
                            setting.Created = DateTime.Now;
                            entities.UserCompanySetting.AddObject(setting);
                            entities.SaveChanges();
                            int rownr = 1;

                            if (entities.EmployeeGroup.Any(a => a.ActorCompanyId == company.ActorCompanyId && a.RuleWorkTimeYear2018 != 0))
                            {
                                foreach (var item in entities.EmployeeGroup.Where(a => a.ActorCompanyId == company.ActorCompanyId && a.RuleWorkTimeYear2018 != 0).GroupBy(g => g.RuleWorkTimeYear2018))
                                {
                                    TimePeriod period = new TimePeriod()
                                    {
                                        StartDate = new DateTime(2018, 1, 1),
                                        StopDate = new DateTime(2018, 12, 31),
                                        RowNr = rownr,
                                        Name = "2018"
                                    };
                                    head.TimePeriod.Add(period);

                                    foreach (var employeeGroup in item)
                                    {
                                        employeeGroup.EmployeeGroupRuleWorkTimePeriod.Add(
                                            new EmployeeGroupRuleWorkTimePeriod()
                                            {
                                                TimePeriod = period,
                                                RuleWorkTime = item.Key,
                                                EmployeeGroup = employeeGroup
                                            });
                                    }
                                }
                                entities.SaveChanges();
                            }

                            if (entities.EmployeeGroup.Any(a => a.ActorCompanyId == company.ActorCompanyId && a.RuleWorkTimeYear2019 != 0))
                            {
                                foreach (var item in entities.EmployeeGroup.Where(a => a.ActorCompanyId == company.ActorCompanyId && a.RuleWorkTimeYear2019 != 0).GroupBy(g => g.RuleWorkTimeYear2019))
                                {
                                    TimePeriod period = new TimePeriod()
                                    {
                                        StartDate = new DateTime(2019, 1, 1),
                                        StopDate = new DateTime(2019, 12, 31),
                                        RowNr = rownr,
                                        Name = "2019"
                                    };
                                    head.TimePeriod.Add(period);

                                    foreach (var employeeGroup in item)
                                    {
                                        employeeGroup.EmployeeGroupRuleWorkTimePeriod.Add(
                                            new EmployeeGroupRuleWorkTimePeriod()
                                            {
                                                TimePeriod = period,
                                                RuleWorkTime = item.Key,
                                                EmployeeGroup = employeeGroup
                                            });
                                    }
                                }
                                entities.SaveChanges();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    result = new ActionResult(ex);
                    base.LogError(ex);
                }

                // Check in scheduled job
                CheckInScheduledJob(result.Success);
            }
        }
    }
}
