using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soe.Business.Tests.Business.Util.StaffingNeedsCalculation.Mock
{
    public static class EmployeePostMock
    {
        public static List<EmployeePostDTO> GetEmployeePosts(StaffingNeedMockScenario staffingNeedMockScenario)
        {
            switch (staffingNeedMockScenario)
            {
                case StaffingNeedMockScenario.All:
                case StaffingNeedMockScenario.FourtyHours:
                    return GetEmployeePosts();
                default:
                    return GetEmployeePosts();
            }
        }
        public static List<EmployeePostDTO> GetEmployeePosts()
        {
            var listOfEmployeePostDTOs = new List<EmployeePostDTO>
            {
              new EmployeePostDTO
              {
                EmployeePostId = 1,
                ActorCompanyId = 451,
                EmployeeGroupId = 80,
                ScheduleCycleId = 76,
                Name = "Kontor",
                Description = "",
                DateFrom = DateTime.ParseExact("2020-03-23T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                DateTo = null,
                WorkTimeWeek = 480,
                WorkTimePercent = 21.00m,
                DayOfWeeks = "",
                DayOfWeeksGenericType = null,
                OverWriteDayOfWeekIds = null,
                DayOfWeekIds = new List<int>
                {
                },
                DayOfWeeksGridString = null,
                WorkDaysWeek = 2,
                RemainingWorkDaysWeek = 2,
                Status = SoeEmployeePostStatus.None,
                Created = DateTime.ParseExact("2020-01-31T12:25:32.7900000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "70",
                Modified = DateTime.ParseExact("2020-01-31T15:18:14.8800000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ModifiedBy = "50",
                EmployeePostWeekendType = TermGroup_EmployeePostWeekendType.AutomaticWeekend,
                State = SoeEntityState.Active,
                AccountId = null,
                EmployeeGroupName = "HAO timavlönad Stämpla",
                EmployeePostSkillDTOs = new List<EmployeePostSkillDTO>
                {
                  new EmployeePostSkillDTO
                  {
                    EmployeePostSkillId = 1371,
                    EmployeePostId = 314,
                    SkillId = 1,
                    SkillLevel = 100,
                    DateTo = null,
                    SkillLevelStars = 0d,
                    SkillLevelUnreached = false,
                    SkillName = "Kontor",
                    SkillTypeName = "allmän",
                    SkillDTO = new SkillDTO
                    {
                      SkillId = 1,
                      SkillTypeId = 1,
                      ActorCompanyId = 451,
                      Name = "Butik",
                      Description = null,
                      Created = DateTime.ParseExact("2018-04-24T09:36:55.0566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "70",
                      Modified = DateTime.ParseExact("2018-05-02T13:59:28.3066667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "ICA",
                      State = SoeEntityState.Active,
                      SkillTypeDTO = new SkillTypeDTO
                      {
                        SkillTypeId = 1,
                        ActorCompanyId = 451,
                        Name = "allmän",
                        Description = null,
                        Created = DateTime.ParseExact("2018-04-24T09:34:38.6233333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "70",
                        Modified = null,
                        ModifiedBy = null,
                        State = SoeEntityState.Active
                      },
                      SkillTypeName = null,
                    }
                  }
                },
                ScheduleCycleDTO = new ScheduleCycleDTO
                {
                  ScheduleCycleId = 76,
                  ActorCompanyId = 451,
                  Name = "Helgpersonal inkl kvällar",
                  Description = "Jobbar även vardagskvällar",
                  NbrOfWeeks = 4,
                  Created = DateTime.ParseExact("2020-01-31T13:44:40.8270000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  CreatedBy = "50",
                  Modified = null,
                  ModifiedBy = null,
                  State = SoeEntityState.Active,
                  ScheduleCycleRuleDTOs = new List<ScheduleCycleRuleDTO>
                  {
                    new ScheduleCycleRuleDTO
                    {
                      ScheduleCycleRuleId = 185,
                      ScheduleCycleId = 76,
                      ScheduleCycleRuleTypeId = 63,
                      MinOccurrences = 0,
                      MaxOccurrences = 4,
                      Created = DateTime.ParseExact("2020-01-31T13:44:40.8266667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "50",
                      Modified = null,
                      ModifiedBy = null,
                      State = SoeEntityState.Active,
                      ScheduleCycleRuleTypeDTO = new ScheduleCycleRuleTypeDTO
                      {
                        ScheduleCycleRuleTypeId = 63,
                        ActorCompanyId = 451,
                        Name = "Vardag kväll",
                        DayOfWeeks = "1,2,3,4,5",
                        DayOfWeekIds = new List<int>
                        {
                          1,
                          2,
                          3,
                          4,
                          5
                        },
                        DayOfWeeksGridString = null,
                        StartTime = DateTime.ParseExact("1900-01-01T14:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        StopTime = DateTime.ParseExact("1900-01-01T21:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Created = DateTime.ParseExact("2018-04-24T09:48:25.4933333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "70",
                        Modified = DateTime.ParseExact("2020-01-31T15:46:13.2033333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        ModifiedBy = "50",
                        State = SoeEntityState.Active
                      }
                    },
                    new ScheduleCycleRuleDTO
                    {
                      ScheduleCycleRuleId = 186,
                      ScheduleCycleId = 76,
                      ScheduleCycleRuleTypeId = 104,
                      MinOccurrences = 1,
                      MaxOccurrences = 4,
                      Created = DateTime.ParseExact("2020-01-31T13:44:40.8266667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "50",
                      Modified = null,
                      ModifiedBy = null,
                      State = SoeEntityState.Active,
                      ScheduleCycleRuleTypeDTO = new ScheduleCycleRuleTypeDTO
                      {
                        ScheduleCycleRuleTypeId = 104,
                        ActorCompanyId = 451,
                        Name = "Helg",
                        DayOfWeeks = "6,0",
                        DayOfWeekIds = new List<int>
                        {
                          6,
                          0
                        },
                        DayOfWeeksGridString = null,
                        StartTime = DateTime.ParseExact("1900-01-01T08:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        StopTime = DateTime.ParseExact("1900-01-01T21:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Created = DateTime.ParseExact("2020-01-31T13:38:11.7833333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "50",
                        Modified = null,
                        ModifiedBy = null,
                        State = SoeEntityState.Active
                      }
                    }
                  }
                },
                EmployeeGroupDTO = new EmployeeGroupDTO
                {
                  EmployeeGroupId = 80,
                  ActorCompanyId = 451,
                  TimeDeviationCauseId = 374,
                  TimeCodeId = null,
                  Name = "HAO timavlönad Stämpla",
                  DeviationAxelStartHours = 2,
                  DeviationAxelStopHours = 2,
                  PayrollProductAccountingPrio = "0,0,0,0,0",
                  InvoiceProductAccountingPrio = "0,0,0,0,0",
                  AutogenTimeblocks = false,
                  AutogenBreakOnStamping = true,
                  AlwaysDiscardBreakEvaluation = false,
                  MergeScheduleBreaksOnDay = true,
                  BreakDayMinutesAfterMidnight = 180,
                  KeepStampsTogetherWithinMinutes = 0,
                  RuleWorkTimeWeek = 2295,
                  RuleWorkTimeYear = 0,
                  RuleRestTimeDay = 660,
                  RuleRestTimeWeek = 2160,
                  MaxScheduleTimeFullTime = 540,
                  MinScheduleTimeFullTime = -540,
                  MaxScheduleTimePartTime = 300,
                  MinScheduleTimePartTime = -300,
                  MaxScheduleTimeWithoutBreaks = 300,
                  RuleWorkTimeDayMinimum = 180,
                  RuleWorkTimeDayMaximumWorkDay = 0,
                  RuleWorkTimeDayMaximumWeekend = 0,
                  Created = DateTime.ParseExact("2017-03-01T10:13:48.5470000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  CreatedBy = "BjörnS",
                  Modified = DateTime.ParseExact("2018-10-31T11:57:28.4500000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  ModifiedBy = "70",
                  State = SoeEntityState.Active,
                  TimeDeviationCause = null,
                  EmployeeGroupTimeDeviationCauseTimeCode = null,
                  TimeDeviationCausesNames = null,
                  DayTypesNames = null,
                  ExternalCodes = null,
                  TimeReportType = 0,
                  TimeReportTypeName = null,
                  ExternalCodesString = null
                },
                IgnoreDaysOfWeekIds = false,
                AccountName = "",
                ValidShiftTypes = new List<ShiftTypeDTO>
                {
                },
                SkillNames = null
              },

              new EmployeePostDTO
              {
                EmployeePostId = 314,
                ActorCompanyId = 451,
                EmployeeGroupId = 80,
                ScheduleCycleId = 76,
                Name = "Post 314",
                Description = "",
                DateFrom = DateTime.ParseExact("2020-03-23T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                DateTo = null,
                WorkTimeWeek = 480,
                WorkTimePercent = 21.00m,
                DayOfWeeks = "",
                DayOfWeeksGenericType = null,
                OverWriteDayOfWeekIds = null,
                DayOfWeekIds = new List<int>
                {
                },
                DayOfWeeksGridString = null,
                WorkDaysWeek = 2,
                RemainingWorkDaysWeek = 2,
                Status = SoeEmployeePostStatus.None,
                Created = DateTime.ParseExact("2020-01-31T12:25:32.7900000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "70",
                Modified = DateTime.ParseExact("2020-01-31T15:18:14.8800000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ModifiedBy = "50",
                EmployeePostWeekendType = TermGroup_EmployeePostWeekendType.AutomaticWeekend,
                State = SoeEntityState.Active,
                AccountId = null,
                EmployeeGroupName = "HAO timavlönad Stämpla",
                EmployeePostSkillDTOs = new List<EmployeePostSkillDTO>
                {
                  new EmployeePostSkillDTO
                  {
                    EmployeePostSkillId = 1371,
                    EmployeePostId = 314,
                    SkillId = 352,
                    SkillLevel = 20,
                    DateTo = null,
                    SkillLevelStars = 0d,
                    SkillLevelUnreached = false,
                    SkillName = "Butik",
                    SkillTypeName = "allmän",
                    SkillDTO = new SkillDTO
                    {
                      SkillId = 352,
                      SkillTypeId = 77,
                      ActorCompanyId = 451,
                      Name = "Butik",
                      Description = null,
                      Created = DateTime.ParseExact("2018-04-24T09:36:55.0566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "70",
                      Modified = DateTime.ParseExact("2018-05-02T13:59:28.3066667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "ICA",
                      State = SoeEntityState.Active,
                      SkillTypeDTO = new SkillTypeDTO
                      {
                        SkillTypeId = 77,
                        ActorCompanyId = 451,
                        Name = "allmän",
                        Description = null,
                        Created = DateTime.ParseExact("2018-04-24T09:34:38.6233333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "70",
                        Modified = null,
                        ModifiedBy = null,
                        State = SoeEntityState.Active
                      },
                      SkillTypeName = null,
                    }
                  }
                },
                ScheduleCycleDTO = new ScheduleCycleDTO
                {
                  ScheduleCycleId = 76,
                  ActorCompanyId = 451,
                  Name = "Helgpersonal inkl kvällar",
                  Description = "Jobbar även vardagskvällar",
                  NbrOfWeeks = 4,
                  Created = DateTime.ParseExact("2020-01-31T13:44:40.8270000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  CreatedBy = "50",
                  Modified = null,
                  ModifiedBy = null,
                  State = SoeEntityState.Active,
                  ScheduleCycleRuleDTOs = new List<ScheduleCycleRuleDTO>
                  {
                    new ScheduleCycleRuleDTO
                    {
                      ScheduleCycleRuleId = 185,
                      ScheduleCycleId = 76,
                      ScheduleCycleRuleTypeId = 63,
                      MinOccurrences = 0,
                      MaxOccurrences = 4,
                      Created = DateTime.ParseExact("2020-01-31T13:44:40.8266667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "50",
                      Modified = null,
                      ModifiedBy = null,
                      State = SoeEntityState.Active,
                      ScheduleCycleRuleTypeDTO = new ScheduleCycleRuleTypeDTO
                      {
                        ScheduleCycleRuleTypeId = 63,
                        ActorCompanyId = 451,
                        Name = "Vardag kväll",
                        DayOfWeeks = "1,2,3,4,5",
                        DayOfWeekIds = new List<int>
                        {
                          1,
                          2,
                          3,
                          4,
                          5
                        },
                        DayOfWeeksGridString = null,
                        StartTime = DateTime.ParseExact("1900-01-01T14:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        StopTime = DateTime.ParseExact("1900-01-01T21:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Created = DateTime.ParseExact("2018-04-24T09:48:25.4933333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "70",
                        Modified = DateTime.ParseExact("2020-01-31T15:46:13.2033333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        ModifiedBy = "50",
                        State = SoeEntityState.Active
                      }
                    },
                    new ScheduleCycleRuleDTO
                    {
                      ScheduleCycleRuleId = 186,
                      ScheduleCycleId = 76,
                      ScheduleCycleRuleTypeId = 104,
                      MinOccurrences = 1,
                      MaxOccurrences = 4,
                      Created = DateTime.ParseExact("2020-01-31T13:44:40.8266667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "50",
                      Modified = null,
                      ModifiedBy = null,
                      State = SoeEntityState.Active,
                      ScheduleCycleRuleTypeDTO = new ScheduleCycleRuleTypeDTO
                      {
                        ScheduleCycleRuleTypeId = 104,
                        ActorCompanyId = 451,
                        Name = "Helg",
                        DayOfWeeks = "6,0",
                        DayOfWeekIds = new List<int>
                        {
                          6,
                          0
                        },
                        DayOfWeeksGridString = null,
                        StartTime = DateTime.ParseExact("1900-01-01T08:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        StopTime = DateTime.ParseExact("1900-01-01T21:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Created = DateTime.ParseExact("2020-01-31T13:38:11.7833333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "50",
                        Modified = null,
                        ModifiedBy = null,
                        State = SoeEntityState.Active
                      }
                    }
                  }
                },
                EmployeeGroupDTO = new EmployeeGroupDTO
                {
                  EmployeeGroupId = 80,
                  ActorCompanyId = 451,
                  TimeDeviationCauseId = 374,
                  TimeCodeId = null,
                  Name = "HAO timavlönad Stämpla",
                  DeviationAxelStartHours = 2,
                  DeviationAxelStopHours = 2,
                  PayrollProductAccountingPrio = "0,0,0,0,0",
                  InvoiceProductAccountingPrio = "0,0,0,0,0",
                  AutogenTimeblocks = false,
                  AutogenBreakOnStamping = true,
                  AlwaysDiscardBreakEvaluation = false,
                  MergeScheduleBreaksOnDay = true,
                  BreakDayMinutesAfterMidnight = 180,
                  KeepStampsTogetherWithinMinutes = 0,
                  RuleWorkTimeWeek = 2295,
                  RuleWorkTimeYear = 0,
                  RuleRestTimeDay = 660,
                  RuleRestTimeWeek = 2160,
                  MaxScheduleTimeFullTime = 540,
                  MinScheduleTimeFullTime = -540,
                  MaxScheduleTimePartTime = 300,
                  MinScheduleTimePartTime = -300,
                  MaxScheduleTimeWithoutBreaks = 300,
                  RuleWorkTimeDayMinimum = 180,
                  RuleWorkTimeDayMaximumWorkDay = 0,
                  RuleWorkTimeDayMaximumWeekend = 0,
                  Created = DateTime.ParseExact("2017-03-01T10:13:48.5470000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  CreatedBy = "BjörnS",
                  Modified = DateTime.ParseExact("2018-10-31T11:57:28.4500000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  ModifiedBy = "70",
                  State = SoeEntityState.Active,
                  TimeDeviationCause = null,
                  EmployeeGroupTimeDeviationCauseTimeCode = null,
                  TimeDeviationCausesNames = null,
                  DayTypesNames = null,
                  ExternalCodes = null,
                  TimeReportType = 0,
                  TimeReportTypeName = null,
                  ExternalCodesString = null
                },
                IgnoreDaysOfWeekIds = false,
                AccountName = "",
                ValidShiftTypes = new List<ShiftTypeDTO>
                {
                },
                SkillNames = null
              },
              new EmployeePostDTO
              {
                EmployeePostId = 315,
                ActorCompanyId = 451,
                EmployeeGroupId = 80,
                ScheduleCycleId = 35,
                Name = "Post 315",
                Description = "",
                DateFrom = DateTime.ParseExact("2020-03-23T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                DateTo = null,
                WorkTimeWeek = 1620,
                WorkTimePercent = 71.00m,
                DayOfWeeks = "",
                DayOfWeeksGenericType = null,
                OverWriteDayOfWeekIds = null,
                DayOfWeekIds = new List<int>
                {
                },
                DayOfWeeksGridString = null,
                WorkDaysWeek = 4,
                RemainingWorkDaysWeek = 4,
                Status = SoeEmployeePostStatus.None,
                Created = DateTime.ParseExact("2020-01-31T12:25:32.8200000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "70",
                Modified = DateTime.ParseExact("2020-01-31T15:16:14.2300000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ModifiedBy = "50",
                EmployeePostWeekendType = TermGroup_EmployeePostWeekendType.AutomaticWeekend,
                State = SoeEntityState.Active,
                AccountId = null,
                EmployeeGroupName = "HAO timavlönad Stämpla",
                EmployeePostSkillDTOs = new List<EmployeePostSkillDTO>
                {
                  new EmployeePostSkillDTO
                  {
                    EmployeePostSkillId = 1372,
                    EmployeePostId = 315,
                    SkillId = 352,
                    SkillLevel = 20,
                    DateTo = null,
                    SkillLevelStars = 0d,
                    SkillLevelUnreached = false,
                    SkillName = "Butik",
                    SkillTypeName = "allmän",
                    SkillDTO = new SkillDTO
                    {
                      SkillId = 352,
                      SkillTypeId = 77,
                      ActorCompanyId = 451,
                      Name = "Butik",
                      Description = null,
                      Created = DateTime.ParseExact("2018-04-24T09:36:55.0566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "70",
                      Modified = DateTime.ParseExact("2018-05-02T13:59:28.3066667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "ICA",
                      State = SoeEntityState.Active,
                      SkillTypeDTO = new SkillTypeDTO
                      {
                        SkillTypeId = 77,
                        ActorCompanyId = 451,
                        Name = "allmän",
                        Description = null,
                        Created = DateTime.ParseExact("2018-04-24T09:34:38.6233333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "70",
                        Modified = null,
                        ModifiedBy = null,
                        State = SoeEntityState.Active
                      },
                      SkillTypeName = null,
                    }
                  }
                },
                ScheduleCycleDTO = new ScheduleCycleDTO
                {
                  ScheduleCycleId = 35,
                  ActorCompanyId = 451,
                  Name = "Fast anställd",
                  Description = "",
                  NbrOfWeeks = 4,
                  Created = DateTime.ParseExact("2018-04-24T09:54:52.3330000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  CreatedBy = "70",
                  Modified = DateTime.ParseExact("2020-01-31T15:44:05.6630000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  ModifiedBy = "50",
                  State = SoeEntityState.Active,
                  ScheduleCycleRuleDTOs = new List<ScheduleCycleRuleDTO>
                  {
                    new ScheduleCycleRuleDTO
                    {
                      ScheduleCycleRuleId = 177,
                      ScheduleCycleId = 35,
                      ScheduleCycleRuleTypeId = 62,
                      MinOccurrences = 12,
                      MaxOccurrences = 16,
                      Created = DateTime.ParseExact("2020-01-31T13:34:49.2566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "50",
                      Modified = DateTime.ParseExact("2020-01-31T15:44:05.6800000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "50",
                      State = SoeEntityState.Active,
                      ScheduleCycleRuleTypeDTO = new ScheduleCycleRuleTypeDTO
                      {
                        ScheduleCycleRuleTypeId = 62,
                        ActorCompanyId = 451,
                        Name = "Vardag dag",
                        DayOfWeeks = "1,2,3,4,5",
                        DayOfWeekIds = new List<int>
                        {
                          1,
                          2,
                          3,
                          4,
                          5
                        },
                        DayOfWeeksGridString = null,
                        StartTime = DateTime.ParseExact("1900-01-01T08:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        StopTime = DateTime.ParseExact("1900-01-01T18:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Created = DateTime.ParseExact("2018-04-16T16:03:19.6733333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "ICA",
                        Modified = DateTime.ParseExact("2020-01-31T13:30:19.1900000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        ModifiedBy = "50",
                        State = SoeEntityState.Active
                      }
                    },
                    new ScheduleCycleRuleDTO
                    {
                      ScheduleCycleRuleId = 178,
                      ScheduleCycleId = 35,
                      ScheduleCycleRuleTypeId = 63,
                      MinOccurrences = 4,
                      MaxOccurrences = 4,
                      Created = DateTime.ParseExact("2020-01-31T13:34:49.2566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "50",
                      Modified = DateTime.ParseExact("2020-01-31T15:44:05.6800000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "50",
                      State = SoeEntityState.Active,
                      ScheduleCycleRuleTypeDTO = new ScheduleCycleRuleTypeDTO
                      {
                        ScheduleCycleRuleTypeId = 63,
                        ActorCompanyId = 451,
                        Name = "Vardag kväll",
                        DayOfWeeks = "1,2,3,4,5",
                        DayOfWeekIds = new List<int>
                        {
                          1,
                          2,
                          3,
                          4,
                          5
                        },
                        DayOfWeeksGridString = null,
                        StartTime = DateTime.ParseExact("1900-01-01T14:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        StopTime = DateTime.ParseExact("1900-01-01T21:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Created = DateTime.ParseExact("2018-04-24T09:48:25.4933333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "70",
                        Modified = DateTime.ParseExact("2020-01-31T15:46:13.2033333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        ModifiedBy = "50",
                        State = SoeEntityState.Active
                      }
                    },
                    new ScheduleCycleRuleDTO
                    {
                      ScheduleCycleRuleId = 179,
                      ScheduleCycleId = 35,
                      ScheduleCycleRuleTypeId = 66,
                      MinOccurrences = 1,
                      MaxOccurrences = 1,
                      Created = DateTime.ParseExact("2020-01-31T13:34:49.2566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "50",
                      Modified = DateTime.ParseExact("2020-01-31T15:44:05.6800000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "50",
                      State = SoeEntityState.Active,
                      ScheduleCycleRuleTypeDTO = new ScheduleCycleRuleTypeDTO
                      {
                        ScheduleCycleRuleTypeId = 66,
                        ActorCompanyId = 451,
                        Name = "Lördag dag",
                        DayOfWeeks = "6",
                        DayOfWeekIds = new List<int>
                        {
                          6
                        },
                        DayOfWeeksGridString = null,
                        StartTime = DateTime.ParseExact("1900-01-01T08:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        StopTime = DateTime.ParseExact("1900-01-01T17:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Created = DateTime.ParseExact("2018-05-14T14:00:11.8866667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "ICA",
                        Modified = DateTime.ParseExact("2020-01-31T13:31:33.6966667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        ModifiedBy = "50",
                        State = SoeEntityState.Active
                      }
                    },
                    new ScheduleCycleRuleDTO
                    {
                      ScheduleCycleRuleId = 180,
                      ScheduleCycleId = 35,
                      ScheduleCycleRuleTypeId = 101,
                      MinOccurrences = 1,
                      MaxOccurrences = 1,
                      Created = DateTime.ParseExact("2020-01-31T13:34:49.2566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "50",
                      Modified = DateTime.ParseExact("2020-01-31T15:44:05.6800000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "50",
                      State = SoeEntityState.Active,
                      ScheduleCycleRuleTypeDTO = new ScheduleCycleRuleTypeDTO
                      {
                        ScheduleCycleRuleTypeId = 101,
                        ActorCompanyId = 451,
                        Name = "Lördag kväll",
                        DayOfWeeks = "6",
                        DayOfWeekIds = new List<int>
                        {
                          6
                        },
                        DayOfWeeksGridString = null,
                        StartTime = DateTime.ParseExact("1900-01-01T15:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        StopTime = DateTime.ParseExact("1900-01-01T21:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Created = DateTime.ParseExact("2020-01-31T13:32:07.5566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "50",
                        Modified = null,
                        ModifiedBy = null,
                        State = SoeEntityState.Active
                      }
                    },
                    new ScheduleCycleRuleDTO
                    {
                      ScheduleCycleRuleId = 181,
                      ScheduleCycleId = 35,
                      ScheduleCycleRuleTypeId = 102,
                      MinOccurrences = 1,
                      MaxOccurrences = 1,
                      Created = DateTime.ParseExact("2020-01-31T13:34:49.2566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "50",
                      Modified = DateTime.ParseExact("2020-01-31T15:44:05.6800000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "50",
                      State = SoeEntityState.Active,
                      ScheduleCycleRuleTypeDTO = new ScheduleCycleRuleTypeDTO
                      {
                        ScheduleCycleRuleTypeId = 102,
                        ActorCompanyId = 451,
                        Name = "Söndag dag",
                        DayOfWeeks = "0",
                        DayOfWeekIds = new List<int>
                        {
                          0
                        },
                        DayOfWeeksGridString = null,
                        StartTime = DateTime.ParseExact("1900-01-01T08:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        StopTime = DateTime.ParseExact("1900-01-01T17:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Created = DateTime.ParseExact("2020-01-31T13:32:36.0300000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "50",
                        Modified = null,
                        ModifiedBy = null,
                        State = SoeEntityState.Active
                      }
                    },
                    new ScheduleCycleRuleDTO
                    {
                      ScheduleCycleRuleId = 182,
                      ScheduleCycleId = 35,
                      ScheduleCycleRuleTypeId = 103,
                      MinOccurrences = 1,
                      MaxOccurrences = 1,
                      Created = DateTime.ParseExact("2020-01-31T13:34:49.2566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "50",
                      Modified = DateTime.ParseExact("2020-01-31T15:44:05.6800000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "50",
                      State = SoeEntityState.Active,
                      ScheduleCycleRuleTypeDTO = new ScheduleCycleRuleTypeDTO
                      {
                        ScheduleCycleRuleTypeId = 103,
                        ActorCompanyId = 451,
                        Name = "Söndag kväll",
                        DayOfWeeks = "0",
                        DayOfWeekIds = new List<int>
                        {
                          0
                        },
                        DayOfWeeksGridString = null,
                        StartTime = DateTime.ParseExact("1900-01-01T15:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        StopTime = DateTime.ParseExact("1900-01-01T21:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Created = DateTime.ParseExact("2020-01-31T13:32:53.2800000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "50",
                        Modified = null,
                        ModifiedBy = null,
                        State = SoeEntityState.Active
                      }
                    }
                  }
                },
                EmployeeGroupDTO = new EmployeeGroupDTO
                {
                  EmployeeGroupId = 80,
                  ActorCompanyId = 451,
                  TimeDeviationCauseId = 374,
                  TimeCodeId = null,
                  Name = "HAO timavlönad Stämpla",
                  DeviationAxelStartHours = 2,
                  DeviationAxelStopHours = 2,
                  PayrollProductAccountingPrio = "0,0,0,0,0",
                  InvoiceProductAccountingPrio = "0,0,0,0,0",
                  AutogenTimeblocks = false,
                  AutogenBreakOnStamping = true,
                  AlwaysDiscardBreakEvaluation = false,
                  MergeScheduleBreaksOnDay = true,
                  BreakDayMinutesAfterMidnight = 180,
                  KeepStampsTogetherWithinMinutes = 0,
                  RuleWorkTimeWeek = 2295,
                  RuleWorkTimeYear = 0,
                  RuleRestTimeDay = 660,
                  RuleRestTimeWeek = 2160,
                  MaxScheduleTimeFullTime = 540,
                  MinScheduleTimeFullTime = -540,
                  MaxScheduleTimePartTime = 300,
                  MinScheduleTimePartTime = -300,
                  MaxScheduleTimeWithoutBreaks = 300,
                  RuleWorkTimeDayMinimum = 180,
                  RuleWorkTimeDayMaximumWorkDay = 0,
                  RuleWorkTimeDayMaximumWeekend = 0,
                  Created = DateTime.ParseExact("2017-03-01T10:13:48.5470000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  CreatedBy = "BjörnS",
                  Modified = DateTime.ParseExact("2018-10-31T11:57:28.4500000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  ModifiedBy = "70",
                  State = SoeEntityState.Active,
                  TimeDeviationCause = null,
                  EmployeeGroupTimeDeviationCauseTimeCode = null,
                  TimeDeviationCausesNames = null,
                  DayTypesNames = null,
                  ExternalCodes = null,
                  TimeReportType = 0,
                  TimeReportTypeName = null,
                  ExternalCodesString = null
                },
                IgnoreDaysOfWeekIds = false,
                AccountName = "",
                ValidShiftTypes = new List<ShiftTypeDTO>
                {
                },
                SkillNames = null
              },
              new EmployeePostDTO
              {
                EmployeePostId = 317,
                ActorCompanyId = 451,
                EmployeeGroupId = 80,
                ScheduleCycleId = 75,
                Name = "Post 317",
                Description = "",
                DateFrom = DateTime.ParseExact("2020-03-23T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                DateTo = null,
                WorkTimeWeek = 480,
                WorkTimePercent = 21.00m,
                DayOfWeeks = "",
                DayOfWeeksGenericType = null,
                OverWriteDayOfWeekIds = null,
                DayOfWeekIds = new List<int>
                {
                },
                DayOfWeeksGridString = null,
                WorkDaysWeek = 2,
                RemainingWorkDaysWeek = 2,
                Status = SoeEmployeePostStatus.None,
                Created = DateTime.ParseExact("2020-01-31T12:25:32.8970000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "70",
                Modified = DateTime.ParseExact("2020-01-31T15:18:31.3030000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ModifiedBy = "50",
                EmployeePostWeekendType = TermGroup_EmployeePostWeekendType.AutomaticWeekend,
                State = SoeEntityState.Active,
                AccountId = null,
                EmployeeGroupName = "HAO timavlönad Stämpla",
                EmployeePostSkillDTOs = new List<EmployeePostSkillDTO>
                {
                  new EmployeePostSkillDTO
                  {
                    EmployeePostSkillId = 1374,
                    EmployeePostId = 317,
                    SkillId = 352,
                    SkillLevel = 20,
                    DateTo = null,
                    SkillLevelStars = 0d,
                    SkillLevelUnreached = false,
                    SkillName = "Butik",
                    SkillTypeName = "allmän",
                    SkillDTO = new SkillDTO
                    {
                      SkillId = 352,
                      SkillTypeId = 77,
                      ActorCompanyId = 451,
                      Name = "Butik",
                      Description = null,
                      Created = DateTime.ParseExact("2018-04-24T09:36:55.0566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "70",
                      Modified = DateTime.ParseExact("2018-05-02T13:59:28.3066667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "ICA",
                      State = SoeEntityState.Active,
                      SkillTypeDTO = new SkillTypeDTO
                      {
                        SkillTypeId = 77,
                        ActorCompanyId = 451,
                        Name = "allmän",
                        Description = null,
                        Created = DateTime.ParseExact("2018-04-24T09:34:38.6233333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "70",
                        Modified = null,
                        ModifiedBy = null,
                        State = SoeEntityState.Active
                      },
                      SkillTypeName = null,
                    }
                  }
                },
                ScheduleCycleDTO = new ScheduleCycleDTO
                {
                  ScheduleCycleId = 75,
                  ActorCompanyId = 451,
                  Name = "Helgpersonal endast",
                  Description = "Jobbar ej vardagskvällar",
                  NbrOfWeeks = 4,
                  Created = DateTime.ParseExact("2020-01-31T13:40:41.4330000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  CreatedBy = "50",
                  Modified = null,
                  ModifiedBy = null,
                  State = SoeEntityState.Active,
                  ScheduleCycleRuleDTOs = new List<ScheduleCycleRuleDTO>
                  {
                    new ScheduleCycleRuleDTO
                    {
                      ScheduleCycleRuleId = 183,
                      ScheduleCycleId = 75,
                      ScheduleCycleRuleTypeId = 104,
                      MinOccurrences = 1,
                      MaxOccurrences = 4,
                      Created = DateTime.ParseExact("2020-01-31T13:40:41.4333333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "50",
                      Modified = null,
                      ModifiedBy = null,
                      State = SoeEntityState.Active,
                      ScheduleCycleRuleTypeDTO = new ScheduleCycleRuleTypeDTO
                      {
                        ScheduleCycleRuleTypeId = 104,
                        ActorCompanyId = 451,
                        Name = "Helg",
                        DayOfWeeks = "6,0",
                        DayOfWeekIds = new List<int>
                        {
                          6,
                          0
                        },
                        DayOfWeeksGridString = null,
                        StartTime = DateTime.ParseExact("1900-01-01T08:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        StopTime = DateTime.ParseExact("1900-01-01T21:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Created = DateTime.ParseExact("2020-01-31T13:38:11.7833333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "50",
                        Modified = null,
                        ModifiedBy = null,
                        State = SoeEntityState.Active
                      }
                    }
                  }
                },
                EmployeeGroupDTO = new EmployeeGroupDTO
                {
                  EmployeeGroupId = 80,
                  ActorCompanyId = 451,
                  TimeDeviationCauseId = 374,
                  TimeCodeId = null,
                  Name = "HAO timavlönad Stämpla",
                  DeviationAxelStartHours = 2,
                  DeviationAxelStopHours = 2,
                  PayrollProductAccountingPrio = "0,0,0,0,0",
                  InvoiceProductAccountingPrio = "0,0,0,0,0",
                  AutogenTimeblocks = false,
                  AutogenBreakOnStamping = true,
                  AlwaysDiscardBreakEvaluation = false,
                  MergeScheduleBreaksOnDay = true,
                  BreakDayMinutesAfterMidnight = 180,
                  KeepStampsTogetherWithinMinutes = 0,
                  RuleWorkTimeWeek = 2295,
                  RuleWorkTimeYear = 0,
                  RuleRestTimeDay = 660,
                  RuleRestTimeWeek = 2160,
                  MaxScheduleTimeFullTime = 540,
                  MinScheduleTimeFullTime = -540,
                  MaxScheduleTimePartTime = 300,
                  MinScheduleTimePartTime = -300,
                  MaxScheduleTimeWithoutBreaks = 300,
                  RuleWorkTimeDayMinimum = 180,
                  RuleWorkTimeDayMaximumWorkDay = 0,
                  RuleWorkTimeDayMaximumWeekend = 0,
                  Created = DateTime.ParseExact("2017-03-01T10:13:48.5470000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  CreatedBy = "BjörnS",
                  Modified = DateTime.ParseExact("2018-10-31T11:57:28.4500000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  ModifiedBy = "70",
                  State = SoeEntityState.Active,
                  TimeDeviationCause = null,
                  EmployeeGroupTimeDeviationCauseTimeCode = null,
                  TimeDeviationCausesNames = null,
                  DayTypesNames = null,
                  ExternalCodes = null,
                  TimeReportType = 0,
                  TimeReportTypeName = null,
                  ExternalCodesString = null
                },
                IgnoreDaysOfWeekIds = false,
                AccountName = "",
                ValidShiftTypes = new List<ShiftTypeDTO>
                {
                },
                SkillNames = null
              },
              new EmployeePostDTO
              {
                EmployeePostId = 323,
                ActorCompanyId = 451,
                EmployeeGroupId = 81,
                ScheduleCycleId = 37,
                Name = "Post 323",
                Description = "",
                DateFrom = DateTime.ParseExact("2020-03-23T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                DateTo = null,
                WorkTimeWeek = 2295,
                WorkTimePercent = 100.00m,
                DayOfWeeks = "",
                DayOfWeeksGenericType = null,
                OverWriteDayOfWeekIds = null,
                DayOfWeekIds = new List<int>
                {
                },
                DayOfWeeksGridString = null,
                WorkDaysWeek = 5,
                RemainingWorkDaysWeek = 5,
                Status = SoeEmployeePostStatus.None,
                Created = DateTime.ParseExact("2020-01-31T12:25:33.0700000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "70",
                Modified = DateTime.ParseExact("2020-01-31T14:29:08.1530000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ModifiedBy = "50",
                EmployeePostWeekendType = TermGroup_EmployeePostWeekendType.AutomaticWeekend,
                State = SoeEntityState.Active,
                AccountId = null,
                EmployeeGroupName = "HAO timavlönad Avvikelseregistrera",
                EmployeePostSkillDTOs = new List<EmployeePostSkillDTO>
                {
                  new EmployeePostSkillDTO
                  {
                    EmployeePostSkillId = 1376,
                    EmployeePostId = 323,
                    SkillId = 352,
                    SkillLevel = 20,
                    DateTo = null,
                    SkillLevelStars = 0d,
                    SkillLevelUnreached = false,
                    SkillName = "Butik",
                    SkillTypeName = "allmän",
                    SkillDTO = new SkillDTO
                    {
                      SkillId = 352,
                      SkillTypeId = 77,
                      ActorCompanyId = 451,
                      Name = "Butik",
                      Description = null,
                      Created = DateTime.ParseExact("2018-04-24T09:36:55.0566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "70",
                      Modified = DateTime.ParseExact("2018-05-02T13:59:28.3066667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "ICA",
                      State = SoeEntityState.Active,
                      SkillTypeDTO = new SkillTypeDTO
                      {
                        SkillTypeId = 77,
                        ActorCompanyId = 451,
                        Name = "allmän",
                        Description = null,
                        Created = DateTime.ParseExact("2018-04-24T09:34:38.6233333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "70",
                        Modified = null,
                        ModifiedBy = null,
                        State = SoeEntityState.Active
                      },
                      SkillTypeName = null,
                    }
                  }
                },
                ScheduleCycleDTO = new ScheduleCycleDTO
                {
                  ScheduleCycleId = 37,
                  ActorCompanyId = 451,
                  Name = "Butikschef",
                  Description = "",
                  NbrOfWeeks = 4,
                  Created = DateTime.ParseExact("2018-04-25T15:57:13.0900000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  CreatedBy = "70",
                  Modified = DateTime.ParseExact("2020-01-31T13:36:13.7770000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  ModifiedBy = "50",
                  State = SoeEntityState.Active,
                  ScheduleCycleRuleDTOs = new List<ScheduleCycleRuleDTO>
                  {
                    new ScheduleCycleRuleDTO
                    {
                      ScheduleCycleRuleId = 94,
                      ScheduleCycleId = 37,
                      ScheduleCycleRuleTypeId = 62,
                      MinOccurrences = 14,
                      MaxOccurrences = 14,
                      Created = DateTime.ParseExact("2018-04-25T15:57:13.0900000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "70",
                      Modified = DateTime.ParseExact("2020-01-31T13:36:13.7766667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "50",
                      State = SoeEntityState.Active,
                      ScheduleCycleRuleTypeDTO = new ScheduleCycleRuleTypeDTO
                      {
                        ScheduleCycleRuleTypeId = 62,
                        ActorCompanyId = 451,
                        Name = "Vardag dag",
                        DayOfWeeks = "1,2,3,4,5",
                        DayOfWeekIds = new List<int>
                        {
                          1,
                          2,
                          3,
                          4,
                          5
                        },
                        DayOfWeeksGridString = null,
                        StartTime = DateTime.ParseExact("1900-01-01T08:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        StopTime = DateTime.ParseExact("1900-01-01T18:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Created = DateTime.ParseExact("2018-04-16T16:03:19.6733333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "ICA",
                        Modified = DateTime.ParseExact("2020-01-31T13:30:19.1900000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        ModifiedBy = "50",
                        State = SoeEntityState.Active
                      }
                    },
                    new ScheduleCycleRuleDTO
                    {
                      ScheduleCycleRuleId = 95,
                      ScheduleCycleId = 37,
                      ScheduleCycleRuleTypeId = 63,
                      MinOccurrences = 4,
                      MaxOccurrences = 4,
                      Created = DateTime.ParseExact("2018-04-25T15:57:26.3866667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "70",
                      Modified = DateTime.ParseExact("2020-01-31T13:36:13.7766667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "50",
                      State = SoeEntityState.Active,
                      ScheduleCycleRuleTypeDTO = new ScheduleCycleRuleTypeDTO
                      {
                        ScheduleCycleRuleTypeId = 63,
                        ActorCompanyId = 451,
                        Name = "Vardag kväll",
                        DayOfWeeks = "1,2,3,4,5",
                        DayOfWeekIds = new List<int>
                        {
                          1,
                          2,
                          3,
                          4,
                          5
                        },
                        DayOfWeeksGridString = null,
                        StartTime = DateTime.ParseExact("1900-01-01T14:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        StopTime = DateTime.ParseExact("1900-01-01T21:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Created = DateTime.ParseExact("2018-04-24T09:48:25.4933333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "70",
                        Modified = DateTime.ParseExact("2020-01-31T15:46:13.2033333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        ModifiedBy = "50",
                        State = SoeEntityState.Active
                      }
                    },
                    new ScheduleCycleRuleDTO
                    {
                      ScheduleCycleRuleId = 97,
                      ScheduleCycleId = 37,
                      ScheduleCycleRuleTypeId = 66,
                      MinOccurrences = 2,
                      MaxOccurrences = 2,
                      Created = DateTime.ParseExact("2018-04-25T15:57:54.9500000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "70",
                      Modified = DateTime.ParseExact("2020-01-31T13:36:13.7766667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "50",
                      State = SoeEntityState.Active,
                      ScheduleCycleRuleTypeDTO = new ScheduleCycleRuleTypeDTO
                      {
                        ScheduleCycleRuleTypeId = 66,
                        ActorCompanyId = 451,
                        Name = "Lördag dag",
                        DayOfWeeks = "6",
                        DayOfWeekIds = new List<int>
                        {
                          6
                        },
                        DayOfWeeksGridString = null,
                        StartTime = DateTime.ParseExact("1900-01-01T08:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        StopTime = DateTime.ParseExact("1900-01-01T17:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Created = DateTime.ParseExact("2018-05-14T14:00:11.8866667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "ICA",
                        Modified = DateTime.ParseExact("2020-01-31T13:31:33.6966667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        ModifiedBy = "50",
                        State = SoeEntityState.Active
                      }
                    }
                  }
                },
                EmployeeGroupDTO = new EmployeeGroupDTO
                {
                  EmployeeGroupId = 81,
                  ActorCompanyId = 451,
                  TimeDeviationCauseId = 374,
                  TimeCodeId = null,
                  Name = "HAO timavlönad Avvikelseregistrera",
                  DeviationAxelStartHours = 2,
                  DeviationAxelStopHours = 2,
                  PayrollProductAccountingPrio = "0,0,0,0,0",
                  InvoiceProductAccountingPrio = "0,0,0,0,0",
                  AutogenTimeblocks = true,
                  AutogenBreakOnStamping = true,
                  AlwaysDiscardBreakEvaluation = false,
                  MergeScheduleBreaksOnDay = true,
                  BreakDayMinutesAfterMidnight = 180,
                  KeepStampsTogetherWithinMinutes = 0,
                  RuleWorkTimeWeek = 2295,
                  RuleWorkTimeYear = 0,
                  RuleRestTimeDay = 660,
                  RuleRestTimeWeek = 2160,
                  MaxScheduleTimeFullTime = 540,
                  MinScheduleTimeFullTime = -540,
                  MaxScheduleTimePartTime = 300,
                  MinScheduleTimePartTime = -300,
                  MaxScheduleTimeWithoutBreaks = 300,
                  RuleWorkTimeDayMinimum = 180,
                  RuleWorkTimeDayMaximumWorkDay = 0,
                  RuleWorkTimeDayMaximumWeekend = 0,
                  Created = DateTime.ParseExact("2017-03-01T12:28:15.5300000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  CreatedBy = "SoftOne (552)",
                  Modified = DateTime.ParseExact("2018-10-31T11:50:17.5530000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  ModifiedBy = "70",
                  State = SoeEntityState.Active,
                  TimeDeviationCause = null,
                  EmployeeGroupTimeDeviationCauseTimeCode = null,
                  TimeDeviationCausesNames = null,
                  DayTypesNames = null,
                  ExternalCodes = null,
                  TimeReportType = 1,
                  TimeReportTypeName = null,
                  ExternalCodesString = null
                },
                IgnoreDaysOfWeekIds = false,
                AccountName = "",
                ValidShiftTypes = new List<ShiftTypeDTO>
                {
                },
                SkillNames = null
              },
              new EmployeePostDTO
              {
                EmployeePostId = 320,
                ActorCompanyId = 451,
                EmployeeGroupId = 80,
                ScheduleCycleId = 78,
                Name = "Post 320",
                Description = "",
                DateFrom = DateTime.ParseExact("2020-03-23T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                DateTo = null,
                WorkTimeWeek = 360,
                WorkTimePercent = 16.00m,
                DayOfWeeks = "",
                DayOfWeeksGenericType = null,
                OverWriteDayOfWeekIds = null,
                DayOfWeekIds = new List<int>
                {
                },
                DayOfWeeksGridString = null,
                WorkDaysWeek = 1,
                RemainingWorkDaysWeek = 1,
                Status = SoeEmployeePostStatus.None,
                Created = DateTime.ParseExact("2020-01-31T12:25:32.9600000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "70",
                Modified = DateTime.ParseExact("2020-01-31T14:27:41.3830000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ModifiedBy = "50",
                EmployeePostWeekendType = TermGroup_EmployeePostWeekendType.AutomaticWeekend,
                State = SoeEntityState.Active,
                AccountId = null,
                EmployeeGroupName = "HAO timavlönad Stämpla",
                EmployeePostSkillDTOs = new List<EmployeePostSkillDTO>
                {
                  new EmployeePostSkillDTO
                  {
                    EmployeePostSkillId = 1388,
                    EmployeePostId = 320,
                    SkillId = 352,
                    SkillLevel = 20,
                    DateTo = null,
                    SkillLevelStars = 0d,
                    SkillLevelUnreached = false,
                    SkillName = "Butik",
                    SkillTypeName = "allmän",
                    SkillDTO = new SkillDTO
                    {
                      SkillId = 352,
                      SkillTypeId = 77,
                      ActorCompanyId = 451,
                      Name = "Butik",
                      Description = null,
                      Created = DateTime.ParseExact("2018-04-24T09:36:55.0566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "70",
                      Modified = DateTime.ParseExact("2018-05-02T13:59:28.3066667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "ICA",
                      State = SoeEntityState.Active,
                      SkillTypeDTO = new SkillTypeDTO
                      {
                        SkillTypeId = 77,
                        ActorCompanyId = 451,
                        Name = "allmän",
                        Description = null,
                        Created = DateTime.ParseExact("2018-04-24T09:34:38.6233333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "70",
                        Modified = null,
                        ModifiedBy = null,
                        State = SoeEntityState.Active
                      },
                      SkillTypeName = null,
                    }
                  }
                },
                ScheduleCycleDTO = new ScheduleCycleDTO
                {
                  ScheduleCycleId = 78,
                  ActorCompanyId = 451,
                  Name = "Joel",
                  Description = "",
                  NbrOfWeeks = 4,
                  Created = DateTime.ParseExact("2020-01-31T13:49:43.1600000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  CreatedBy = "50",
                  Modified = DateTime.ParseExact("2020-01-31T13:49:55.9870000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  ModifiedBy = "50",
                  State = SoeEntityState.Active,
                  ScheduleCycleRuleDTOs = new List<ScheduleCycleRuleDTO>
                  {
                    new ScheduleCycleRuleDTO
                    {
                      ScheduleCycleRuleId = 188,
                      ScheduleCycleId = 78,
                      ScheduleCycleRuleTypeId = 106,
                      MinOccurrences = 2,
                      MaxOccurrences = 4,
                      Created = DateTime.ParseExact("2020-01-31T13:49:43.1600000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "50",
                      Modified = DateTime.ParseExact("2020-01-31T13:49:55.9866667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "50",
                      State = SoeEntityState.Active,
                      ScheduleCycleRuleTypeDTO = new ScheduleCycleRuleTypeDTO
                      {
                        ScheduleCycleRuleTypeId = 106,
                        ActorCompanyId = 451,
                        Name = "Fredag kväll",
                        DayOfWeeks = "5",
                        DayOfWeekIds = new List<int>
                        {
                          5
                        },
                        DayOfWeeksGridString = null,
                        StartTime = DateTime.ParseExact("1900-01-01T16:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        StopTime = DateTime.ParseExact("1900-01-01T21:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Created = DateTime.ParseExact("2020-01-31T13:49:07.3766667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "50",
                        Modified = null,
                        ModifiedBy = null,
                        State = SoeEntityState.Active
                      }
                    },
                    new ScheduleCycleRuleDTO
                    {
                      ScheduleCycleRuleId = 189,
                      ScheduleCycleId = 78,
                      ScheduleCycleRuleTypeId = 104,
                      MinOccurrences = 2,
                      MaxOccurrences = 4,
                      Created = DateTime.ParseExact("2020-01-31T13:49:55.9866667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "50",
                      Modified = null,
                      ModifiedBy = null,
                      State = SoeEntityState.Active,
                      ScheduleCycleRuleTypeDTO = new ScheduleCycleRuleTypeDTO
                      {
                        ScheduleCycleRuleTypeId = 104,
                        ActorCompanyId = 451,
                        Name = "Helg",
                        DayOfWeeks = "6,0",
                        DayOfWeekIds = new List<int>
                        {
                          6,
                          0
                        },
                        DayOfWeeksGridString = null,
                        StartTime = DateTime.ParseExact("1900-01-01T08:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        StopTime = DateTime.ParseExact("1900-01-01T21:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Created = DateTime.ParseExact("2020-01-31T13:38:11.7833333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "50",
                        Modified = null,
                        ModifiedBy = null,
                        State = SoeEntityState.Active
                      }
                    }
                  }
                },
                EmployeeGroupDTO = new EmployeeGroupDTO
                {
                  EmployeeGroupId = 80,
                  ActorCompanyId = 451,
                  TimeDeviationCauseId = 374,
                  TimeCodeId = null,
                  Name = "HAO timavlönad Stämpla",
                  DeviationAxelStartHours = 2,
                  DeviationAxelStopHours = 2,
                  PayrollProductAccountingPrio = "0,0,0,0,0",
                  InvoiceProductAccountingPrio = "0,0,0,0,0",
                  AutogenTimeblocks = false,
                  AutogenBreakOnStamping = true,
                  AlwaysDiscardBreakEvaluation = false,
                  MergeScheduleBreaksOnDay = true,
                  BreakDayMinutesAfterMidnight = 180,
                  KeepStampsTogetherWithinMinutes = 0,
                  RuleWorkTimeWeek = 2295,
                  RuleWorkTimeYear = 0,
                  RuleRestTimeDay = 660,
                  RuleRestTimeWeek = 2160,
                  MaxScheduleTimeFullTime = 540,
                  MinScheduleTimeFullTime = -540,
                  MaxScheduleTimePartTime = 300,
                  MinScheduleTimePartTime = -300,
                  MaxScheduleTimeWithoutBreaks = 300,
                  RuleWorkTimeDayMinimum = 180,
                  RuleWorkTimeDayMaximumWorkDay = 0,
                  RuleWorkTimeDayMaximumWeekend = 0,
                  Created = DateTime.ParseExact("2017-03-01T10:13:48.5470000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  CreatedBy = "BjörnS",
                  Modified = DateTime.ParseExact("2018-10-31T11:57:28.4500000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  ModifiedBy = "70",
                  State = SoeEntityState.Active,
                  TimeDeviationCause = null,
                  EmployeeGroupTimeDeviationCauseTimeCode = null,
                  TimeDeviationCausesNames = null,
                  DayTypesNames = null,
                  ExternalCodes = null,
                  TimeReportType = 0,
                  TimeReportTypeName = null,
                  ExternalCodesString = null
                },
                IgnoreDaysOfWeekIds = false,
                AccountName = "",
                ValidShiftTypes = new List<ShiftTypeDTO>
                {
                },
                SkillNames = null
              },
              new EmployeePostDTO
              {
                EmployeePostId = 318,
                ActorCompanyId = 451,
                EmployeeGroupId = 80,
                ScheduleCycleId = 35,
                Name = "Post 318",
                Description = "",
                DateFrom = DateTime.ParseExact("2020-03-23T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                DateTo = null,
                WorkTimeWeek = 1590,
                WorkTimePercent = 69.00m,
                DayOfWeeks = "",
                DayOfWeeksGenericType = null,
                OverWriteDayOfWeekIds = null,
                DayOfWeekIds = new List<int>
                {
                },
                DayOfWeeksGridString = null,
                WorkDaysWeek = 3,
                RemainingWorkDaysWeek = 3,
                Status = SoeEmployeePostStatus.None,
                Created = DateTime.ParseExact("2020-01-31T12:25:32.9130000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "70",
                Modified = DateTime.ParseExact("2020-01-31T15:39:22.0830000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ModifiedBy = "50",
                EmployeePostWeekendType = TermGroup_EmployeePostWeekendType.AutomaticWeekend,
                State = SoeEntityState.Active,
                AccountId = null,
                EmployeeGroupName = "HAO timavlönad Stämpla",
                EmployeePostSkillDTOs = new List<EmployeePostSkillDTO>
                {
                  new EmployeePostSkillDTO
                  {
                    EmployeePostSkillId = 1375,
                    EmployeePostId = 318,
                    SkillId = 352,
                    SkillLevel = 20,
                    DateTo = null,
                    SkillLevelStars = 0d,
                    SkillLevelUnreached = false,
                    SkillName = "Butik",
                    SkillTypeName = "allmän",
                    SkillDTO = new SkillDTO
                    {
                      SkillId = 352,
                      SkillTypeId = 77,
                      ActorCompanyId = 451,
                      Name = "Butik",
                      Description = null,
                      Created = DateTime.ParseExact("2018-04-24T09:36:55.0566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "70",
                      Modified = DateTime.ParseExact("2018-05-02T13:59:28.3066667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "ICA",
                      State = SoeEntityState.Active,
                      SkillTypeDTO = new SkillTypeDTO
                      {
                        SkillTypeId = 77,
                        ActorCompanyId = 451,
                        Name = "allmän",
                        Description = null,
                        Created = DateTime.ParseExact("2018-04-24T09:34:38.6233333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "70",
                        Modified = null,
                        ModifiedBy = null,
                        State = SoeEntityState.Active
                      },
                      SkillTypeName = null,
                    }
                  }
                },
                ScheduleCycleDTO = new ScheduleCycleDTO
                {
                  ScheduleCycleId = 35,
                  ActorCompanyId = 451,
                  Name = "Fast anställd",
                  Description = "",
                  NbrOfWeeks = 4,
                  Created = DateTime.ParseExact("2018-04-24T09:54:52.3330000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  CreatedBy = "70",
                  Modified = DateTime.ParseExact("2020-01-31T15:44:05.6630000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  ModifiedBy = "50",
                  State = SoeEntityState.Active,
                  ScheduleCycleRuleDTOs = new List<ScheduleCycleRuleDTO>
                  {
                    new ScheduleCycleRuleDTO
                    {
                      ScheduleCycleRuleId = 177,
                      ScheduleCycleId = 35,
                      ScheduleCycleRuleTypeId = 62,
                      MinOccurrences = 12,
                      MaxOccurrences = 16,
                      Created = DateTime.ParseExact("2020-01-31T13:34:49.2566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "50",
                      Modified = DateTime.ParseExact("2020-01-31T15:44:05.6800000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "50",
                      State = SoeEntityState.Active,
                      ScheduleCycleRuleTypeDTO = new ScheduleCycleRuleTypeDTO
                      {
                        ScheduleCycleRuleTypeId = 62,
                        ActorCompanyId = 451,
                        Name = "Vardag dag",
                        DayOfWeeks = "1,2,3,4,5",
                        DayOfWeekIds = new List<int>
                        {
                          1,
                          2,
                          3,
                          4,
                          5
                        },
                        DayOfWeeksGridString = null,
                        StartTime = DateTime.ParseExact("1900-01-01T08:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        StopTime = DateTime.ParseExact("1900-01-01T18:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Created = DateTime.ParseExact("2018-04-16T16:03:19.6733333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "ICA",
                        Modified = DateTime.ParseExact("2020-01-31T13:30:19.1900000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        ModifiedBy = "50",
                        State = SoeEntityState.Active
                      }
                    },
                    new ScheduleCycleRuleDTO
                    {
                      ScheduleCycleRuleId = 178,
                      ScheduleCycleId = 35,
                      ScheduleCycleRuleTypeId = 63,
                      MinOccurrences = 4,
                      MaxOccurrences = 4,
                      Created = DateTime.ParseExact("2020-01-31T13:34:49.2566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "50",
                      Modified = DateTime.ParseExact("2020-01-31T15:44:05.6800000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "50",
                      State = SoeEntityState.Active,
                      ScheduleCycleRuleTypeDTO = new ScheduleCycleRuleTypeDTO
                      {
                        ScheduleCycleRuleTypeId = 63,
                        ActorCompanyId = 451,
                        Name = "Vardag kväll",
                        DayOfWeeks = "1,2,3,4,5",
                        DayOfWeekIds = new List<int>
                        {
                          1,
                          2,
                          3,
                          4,
                          5
                        },
                        DayOfWeeksGridString = null,
                        StartTime = DateTime.ParseExact("1900-01-01T14:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        StopTime = DateTime.ParseExact("1900-01-01T21:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Created = DateTime.ParseExact("2018-04-24T09:48:25.4933333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "70",
                        Modified = DateTime.ParseExact("2020-01-31T15:46:13.2033333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        ModifiedBy = "50",
                        State = SoeEntityState.Active
                      }
                    },
                    new ScheduleCycleRuleDTO
                    {
                      ScheduleCycleRuleId = 179,
                      ScheduleCycleId = 35,
                      ScheduleCycleRuleTypeId = 66,
                      MinOccurrences = 1,
                      MaxOccurrences = 1,
                      Created = DateTime.ParseExact("2020-01-31T13:34:49.2566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "50",
                      Modified = DateTime.ParseExact("2020-01-31T15:44:05.6800000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "50",
                      State = SoeEntityState.Active,
                      ScheduleCycleRuleTypeDTO = new ScheduleCycleRuleTypeDTO
                      {
                        ScheduleCycleRuleTypeId = 66,
                        ActorCompanyId = 451,
                        Name = "Lördag dag",
                        DayOfWeeks = "6",
                        DayOfWeekIds = new List<int>
                        {
                          6
                        },
                        DayOfWeeksGridString = null,
                        StartTime = DateTime.ParseExact("1900-01-01T08:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        StopTime = DateTime.ParseExact("1900-01-01T17:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Created = DateTime.ParseExact("2018-05-14T14:00:11.8866667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "ICA",
                        Modified = DateTime.ParseExact("2020-01-31T13:31:33.6966667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        ModifiedBy = "50",
                        State = SoeEntityState.Active
                      }
                    },
                    new ScheduleCycleRuleDTO
                    {
                      ScheduleCycleRuleId = 180,
                      ScheduleCycleId = 35,
                      ScheduleCycleRuleTypeId = 101,
                      MinOccurrences = 1,
                      MaxOccurrences = 1,
                      Created = DateTime.ParseExact("2020-01-31T13:34:49.2566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "50",
                      Modified = DateTime.ParseExact("2020-01-31T15:44:05.6800000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "50",
                      State = SoeEntityState.Active,
                      ScheduleCycleRuleTypeDTO = new ScheduleCycleRuleTypeDTO
                      {
                        ScheduleCycleRuleTypeId = 101,
                        ActorCompanyId = 451,
                        Name = "Lördag kväll",
                        DayOfWeeks = "6",
                        DayOfWeekIds = new List<int>
                        {
                          6
                        },
                        DayOfWeeksGridString = null,
                        StartTime = DateTime.ParseExact("1900-01-01T15:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        StopTime = DateTime.ParseExact("1900-01-01T21:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Created = DateTime.ParseExact("2020-01-31T13:32:07.5566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "50",
                        Modified = null,
                        ModifiedBy = null,
                        State = SoeEntityState.Active
                      }
                    },
                    new ScheduleCycleRuleDTO
                    {
                      ScheduleCycleRuleId = 181,
                      ScheduleCycleId = 35,
                      ScheduleCycleRuleTypeId = 102,
                      MinOccurrences = 1,
                      MaxOccurrences = 1,
                      Created = DateTime.ParseExact("2020-01-31T13:34:49.2566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "50",
                      Modified = DateTime.ParseExact("2020-01-31T15:44:05.6800000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "50",
                      State = SoeEntityState.Active,
                      ScheduleCycleRuleTypeDTO = new ScheduleCycleRuleTypeDTO
                      {
                        ScheduleCycleRuleTypeId = 102,
                        ActorCompanyId = 451,
                        Name = "Söndag dag",
                        DayOfWeeks = "0",
                        DayOfWeekIds = new List<int>
                        {
                          0
                        },
                        DayOfWeeksGridString = null,
                        StartTime = DateTime.ParseExact("1900-01-01T08:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        StopTime = DateTime.ParseExact("1900-01-01T17:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Created = DateTime.ParseExact("2020-01-31T13:32:36.0300000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "50",
                        Modified = null,
                        ModifiedBy = null,
                        State = SoeEntityState.Active
                      }
                    },
                    new ScheduleCycleRuleDTO
                    {
                      ScheduleCycleRuleId = 182,
                      ScheduleCycleId = 35,
                      ScheduleCycleRuleTypeId = 103,
                      MinOccurrences = 1,
                      MaxOccurrences = 1,
                      Created = DateTime.ParseExact("2020-01-31T13:34:49.2566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "50",
                      Modified = DateTime.ParseExact("2020-01-31T15:44:05.6800000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "50",
                      State = SoeEntityState.Active,
                      ScheduleCycleRuleTypeDTO = new ScheduleCycleRuleTypeDTO
                      {
                        ScheduleCycleRuleTypeId = 103,
                        ActorCompanyId = 451,
                        Name = "Söndag kväll",
                        DayOfWeeks = "0",
                        DayOfWeekIds = new List<int>
                        {
                          0
                        },
                        DayOfWeeksGridString = null,
                        StartTime = DateTime.ParseExact("1900-01-01T15:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        StopTime = DateTime.ParseExact("1900-01-01T21:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Created = DateTime.ParseExact("2020-01-31T13:32:53.2800000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "50",
                        Modified = null,
                        ModifiedBy = null,
                        State = SoeEntityState.Active
                      }
                    }
                  }
                },
                EmployeeGroupDTO = new EmployeeGroupDTO
                {
                  EmployeeGroupId = 80,
                  ActorCompanyId = 451,
                  TimeDeviationCauseId = 374,
                  TimeCodeId = null,
                  Name = "HAO timavlönad Stämpla",
                  DeviationAxelStartHours = 2,
                  DeviationAxelStopHours = 2,
                  PayrollProductAccountingPrio = "0,0,0,0,0",
                  InvoiceProductAccountingPrio = "0,0,0,0,0",
                  AutogenTimeblocks = false,
                  AutogenBreakOnStamping = true,
                  AlwaysDiscardBreakEvaluation = false,
                  MergeScheduleBreaksOnDay = true,
                  BreakDayMinutesAfterMidnight = 180,
                  KeepStampsTogetherWithinMinutes = 0,
                  RuleWorkTimeWeek = 2295,
                  RuleWorkTimeYear = 0,
                  RuleRestTimeDay = 660,
                  RuleRestTimeWeek = 2160,
                  MaxScheduleTimeFullTime = 540,
                  MinScheduleTimeFullTime = -540,
                  MaxScheduleTimePartTime = 300,
                  MinScheduleTimePartTime = -300,
                  MaxScheduleTimeWithoutBreaks = 300,
                  RuleWorkTimeDayMinimum = 180,
                  RuleWorkTimeDayMaximumWorkDay = 0,
                  RuleWorkTimeDayMaximumWeekend = 0,
                  Created = DateTime.ParseExact("2017-03-01T10:13:48.5470000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  CreatedBy = "BjörnS",
                  Modified = DateTime.ParseExact("2018-10-31T11:57:28.4500000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  ModifiedBy = "70",
                  State = SoeEntityState.Active,
                  TimeDeviationCause = null,
                  EmployeeGroupTimeDeviationCauseTimeCode = null,
                  TimeDeviationCausesNames = null,
                  DayTypesNames = null,
                  ExternalCodes = null,
                  TimeReportType = 0,
                  TimeReportTypeName = null,
                  ExternalCodesString = null
                },
                IgnoreDaysOfWeekIds = false,
                AccountName = "",
                ValidShiftTypes = new List<ShiftTypeDTO>
                {
                },
                SkillNames = null
              },
              new EmployeePostDTO
              {
                EmployeePostId = 312,
                ActorCompanyId = 451,
                EmployeeGroupId = 80,
                ScheduleCycleId = 77,
                Name = "Post 312",
                Description = "",
                DateFrom = DateTime.ParseExact("2020-03-23T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                DateTo = null,
                WorkTimeWeek = 600,
                WorkTimePercent = 26.00m,
                DayOfWeeks = "",
                DayOfWeeksGenericType = null,
                OverWriteDayOfWeekIds = null,
                DayOfWeekIds = new List<int>
                {
                },
                DayOfWeeksGridString = null,
                WorkDaysWeek = 2,
                RemainingWorkDaysWeek = 2,
                Status = SoeEmployeePostStatus.None,
                Created = DateTime.ParseExact("2020-01-31T12:25:32.6930000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "70",
                Modified = DateTime.ParseExact("2020-01-31T15:19:07.0700000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ModifiedBy = "50",
                EmployeePostWeekendType = TermGroup_EmployeePostWeekendType.AutomaticWeekend,
                State = SoeEntityState.Active,
                AccountId = null,
                EmployeeGroupName = "HAO timavlönad Stämpla",
                EmployeePostSkillDTOs = new List<EmployeePostSkillDTO>
                {
                  new EmployeePostSkillDTO
                  {
                    EmployeePostSkillId = 1369,
                    EmployeePostId = 312,
                    SkillId = 352,
                    SkillLevel = 20,
                    DateTo = null,
                    SkillLevelStars = 0d,
                    SkillLevelUnreached = false,
                    SkillName = "Butik",
                    SkillTypeName = "allmän",
                    SkillDTO = new SkillDTO
                    {
                      SkillId = 352,
                      SkillTypeId = 77,
                      ActorCompanyId = 451,
                      Name = "Butik",
                      Description = null,
                      Created = DateTime.ParseExact("2018-04-24T09:36:55.0566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "70",
                      Modified = DateTime.ParseExact("2018-05-02T13:59:28.3066667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "ICA",
                      State = SoeEntityState.Active,
                      SkillTypeDTO = new SkillTypeDTO
                      {
                        SkillTypeId = 77,
                        ActorCompanyId = 451,
                        Name = "allmän",
                        Description = null,
                        Created = DateTime.ParseExact("2018-04-24T09:34:38.6233333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "70",
                        Modified = null,
                        ModifiedBy = null,
                        State = SoeEntityState.Active
                      },
                      SkillTypeName = null,
                    }
                  }
                },
                ScheduleCycleDTO = new ScheduleCycleDTO
                {
                  ScheduleCycleId = 77,
                  ActorCompanyId = 451,
                  Name = "Lena",
                  Description = "",
                  NbrOfWeeks = 4,
                  Created = DateTime.ParseExact("2020-01-31T13:47:41.5100000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  CreatedBy = "50",
                  Modified = null,
                  ModifiedBy = null,
                  State = SoeEntityState.Active,
                  ScheduleCycleRuleDTOs = new List<ScheduleCycleRuleDTO>
                  {
                    new ScheduleCycleRuleDTO
                    {
                      ScheduleCycleRuleId = 187,
                      ScheduleCycleId = 77,
                      ScheduleCycleRuleTypeId = 105,
                      MinOccurrences = 4,
                      MaxOccurrences = 10,
                      Created = DateTime.ParseExact("2020-01-31T13:47:41.5100000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "50",
                      Modified = null,
                      ModifiedBy = null,
                      State = SoeEntityState.Active,
                      ScheduleCycleRuleTypeDTO = new ScheduleCycleRuleTypeDTO
                      {
                        ScheduleCycleRuleTypeId = 105,
                        ActorCompanyId = 451,
                        Name = "Vardagar",
                        DayOfWeeks = "1,2,3,4,5",
                        DayOfWeekIds = new List<int>
                        {
                          1,
                          2,
                          3,
                          4,
                          5
                        },
                        DayOfWeeksGridString = null,
                        StartTime = DateTime.ParseExact("1900-01-01T08:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        StopTime = DateTime.ParseExact("1900-01-01T21:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Created = DateTime.ParseExact("2020-01-31T13:46:29.6933333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "50",
                        Modified = null,
                        ModifiedBy = null,
                        State = SoeEntityState.Active
                      }
                    }
                  }
                },
                EmployeeGroupDTO = new EmployeeGroupDTO
                {
                  EmployeeGroupId = 80,
                  ActorCompanyId = 451,
                  TimeDeviationCauseId = 374,
                  TimeCodeId = null,
                  Name = "HAO timavlönad Stämpla",
                  DeviationAxelStartHours = 2,
                  DeviationAxelStopHours = 2,
                  PayrollProductAccountingPrio = "0,0,0,0,0",
                  InvoiceProductAccountingPrio = "0,0,0,0,0",
                  AutogenTimeblocks = false,
                  AutogenBreakOnStamping = true,
                  AlwaysDiscardBreakEvaluation = false,
                  MergeScheduleBreaksOnDay = true,
                  BreakDayMinutesAfterMidnight = 180,
                  KeepStampsTogetherWithinMinutes = 0,
                  RuleWorkTimeWeek = 2295,
                  RuleWorkTimeYear = 0,
                  RuleRestTimeDay = 660,
                  RuleRestTimeWeek = 2160,
                  MaxScheduleTimeFullTime = 540,
                  MinScheduleTimeFullTime = -540,
                  MaxScheduleTimePartTime = 300,
                  MinScheduleTimePartTime = -300,
                  MaxScheduleTimeWithoutBreaks = 300,
                  RuleWorkTimeDayMinimum = 180,
                  RuleWorkTimeDayMaximumWorkDay = 0,
                  RuleWorkTimeDayMaximumWeekend = 0,
                  Created = DateTime.ParseExact("2017-03-01T10:13:48.5470000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  CreatedBy = "BjörnS",
                  Modified = DateTime.ParseExact("2018-10-31T11:57:28.4500000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  ModifiedBy = "70",
                  State = SoeEntityState.Active,
                  TimeDeviationCause = null,
                  EmployeeGroupTimeDeviationCauseTimeCode = null,
                  TimeDeviationCausesNames = null,
                  DayTypesNames = null,
                  ExternalCodes = null,
                  TimeReportType = 0,
                  TimeReportTypeName = null,
                  ExternalCodesString = null
                },
                IgnoreDaysOfWeekIds = false,
                AccountName = "",
                ValidShiftTypes = new List<ShiftTypeDTO>
                {
                },
                SkillNames = null
              },
              new EmployeePostDTO
              {
                EmployeePostId = 322,
                ActorCompanyId = 451,
                EmployeeGroupId = 80,
                ScheduleCycleId = 76,
                Name = "Post 322",
                Description = "",
                DateFrom = DateTime.ParseExact("2020-03-23T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                DateTo = null,
                WorkTimeWeek = 480,
                WorkTimePercent = 21.00m,
                DayOfWeeks = "",
                DayOfWeeksGenericType = null,
                OverWriteDayOfWeekIds = null,
                DayOfWeekIds = new List<int>
                {
                },
                DayOfWeeksGridString = null,
                WorkDaysWeek = 2,
                RemainingWorkDaysWeek = 2,
                Status = SoeEmployeePostStatus.None,
                Created = DateTime.ParseExact("2020-01-31T12:25:33.0070000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "70",
                Modified = DateTime.ParseExact("2020-01-31T14:28:21.5270000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ModifiedBy = "50",
                EmployeePostWeekendType = TermGroup_EmployeePostWeekendType.AutomaticWeekend,
                State = SoeEntityState.Active,
                AccountId = null,
                EmployeeGroupName = "HAO timavlönad Stämpla",
                EmployeePostSkillDTOs = new List<EmployeePostSkillDTO>
                {
                  new EmployeePostSkillDTO
                  {
                    EmployeePostSkillId = 1390,
                    EmployeePostId = 322,
                    SkillId = 352,
                    SkillLevel = 20,
                    DateTo = null,
                    SkillLevelStars = 0d,
                    SkillLevelUnreached = false,
                    SkillName = "Butik",
                    SkillTypeName = "allmän",
                    SkillDTO = new SkillDTO
                    {
                      SkillId = 352,
                      SkillTypeId = 77,
                      ActorCompanyId = 451,
                      Name = "Butik",
                      Description = null,
                      Created = DateTime.ParseExact("2018-04-24T09:36:55.0566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "70",
                      Modified = DateTime.ParseExact("2018-05-02T13:59:28.3066667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "ICA",
                      State = SoeEntityState.Active,
                      SkillTypeDTO = new SkillTypeDTO
                      {
                        SkillTypeId = 77,
                        ActorCompanyId = 451,
                        Name = "allmän",
                        Description = null,
                        Created = DateTime.ParseExact("2018-04-24T09:34:38.6233333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "70",
                        Modified = null,
                        ModifiedBy = null,
                        State = SoeEntityState.Active
                      },
                      SkillTypeName = null,
                    }
                  }
                },
                ScheduleCycleDTO = new ScheduleCycleDTO
                {
                  ScheduleCycleId = 76,
                  ActorCompanyId = 451,
                  Name = "Helgpersonal inkl kvällar",
                  Description = "Jobbar även vardagskvällar",
                  NbrOfWeeks = 4,
                  Created = DateTime.ParseExact("2020-01-31T13:44:40.8270000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  CreatedBy = "50",
                  Modified = null,
                  ModifiedBy = null,
                  State = SoeEntityState.Active,
                  ScheduleCycleRuleDTOs = new List<ScheduleCycleRuleDTO>
                  {
                    new ScheduleCycleRuleDTO
                    {
                      ScheduleCycleRuleId = 185,
                      ScheduleCycleId = 76,
                      ScheduleCycleRuleTypeId = 63,
                      MinOccurrences = 0,
                      MaxOccurrences = 4,
                      Created = DateTime.ParseExact("2020-01-31T13:44:40.8266667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "50",
                      Modified = null,
                      ModifiedBy = null,
                      State = SoeEntityState.Active,
                      ScheduleCycleRuleTypeDTO = new ScheduleCycleRuleTypeDTO
                      {
                        ScheduleCycleRuleTypeId = 63,
                        ActorCompanyId = 451,
                        Name = "Vardag kväll",
                        DayOfWeeks = "1,2,3,4,5",
                        DayOfWeekIds = new List<int>
                        {
                          1,
                          2,
                          3,
                          4,
                          5
                        },
                        DayOfWeeksGridString = null,
                        StartTime = DateTime.ParseExact("1900-01-01T14:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        StopTime = DateTime.ParseExact("1900-01-01T21:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Created = DateTime.ParseExact("2018-04-24T09:48:25.4933333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "70",
                        Modified = DateTime.ParseExact("2020-01-31T15:46:13.2033333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        ModifiedBy = "50",
                        State = SoeEntityState.Active
                      }
                    },
                    new ScheduleCycleRuleDTO
                    {
                      ScheduleCycleRuleId = 186,
                      ScheduleCycleId = 76,
                      ScheduleCycleRuleTypeId = 104,
                      MinOccurrences = 1,
                      MaxOccurrences = 4,
                      Created = DateTime.ParseExact("2020-01-31T13:44:40.8266667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "50",
                      Modified = null,
                      ModifiedBy = null,
                      State = SoeEntityState.Active,
                      ScheduleCycleRuleTypeDTO = new ScheduleCycleRuleTypeDTO
                      {
                        ScheduleCycleRuleTypeId = 104,
                        ActorCompanyId = 451,
                        Name = "Helg",
                        DayOfWeeks = "6,0",
                        DayOfWeekIds = new List<int>
                        {
                          6,
                          0
                        },
                        DayOfWeeksGridString = null,
                        StartTime = DateTime.ParseExact("1900-01-01T08:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        StopTime = DateTime.ParseExact("1900-01-01T21:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Created = DateTime.ParseExact("2020-01-31T13:38:11.7833333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "50",
                        Modified = null,
                        ModifiedBy = null,
                        State = SoeEntityState.Active
                      }
                    }
                  }
                },
                EmployeeGroupDTO = new EmployeeGroupDTO
                {
                  EmployeeGroupId = 80,
                  ActorCompanyId = 451,
                  TimeDeviationCauseId = 374,
                  TimeCodeId = null,
                  Name = "HAO timavlönad Stämpla",
                  DeviationAxelStartHours = 2,
                  DeviationAxelStopHours = 2,
                  PayrollProductAccountingPrio = "0,0,0,0,0",
                  InvoiceProductAccountingPrio = "0,0,0,0,0",
                  AutogenTimeblocks = false,
                  AutogenBreakOnStamping = true,
                  AlwaysDiscardBreakEvaluation = false,
                  MergeScheduleBreaksOnDay = true,
                  BreakDayMinutesAfterMidnight = 180,
                  KeepStampsTogetherWithinMinutes = 0,
                  RuleWorkTimeWeek = 2295,
                  RuleWorkTimeYear = 0,
                  RuleRestTimeDay = 660,
                  RuleRestTimeWeek = 2160,
                  MaxScheduleTimeFullTime = 540,
                  MinScheduleTimeFullTime = -540,
                  MaxScheduleTimePartTime = 300,
                  MinScheduleTimePartTime = -300,
                  MaxScheduleTimeWithoutBreaks = 300,
                  RuleWorkTimeDayMinimum = 180,
                  RuleWorkTimeDayMaximumWorkDay = 0,
                  RuleWorkTimeDayMaximumWeekend = 0,
                  Created = DateTime.ParseExact("2017-03-01T10:13:48.5470000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  CreatedBy = "BjörnS",
                  Modified = DateTime.ParseExact("2018-10-31T11:57:28.4500000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  ModifiedBy = "70",
                  State = SoeEntityState.Active,
                  TimeDeviationCause = null,
                  EmployeeGroupTimeDeviationCauseTimeCode = null,
                  TimeDeviationCausesNames = null,
                  DayTypesNames = null,
                  ExternalCodes = null,
                  TimeReportType = 0,
                  TimeReportTypeName = null,
                  ExternalCodesString = null
                },
                IgnoreDaysOfWeekIds = false,
                AccountName = "",
                ValidShiftTypes = new List<ShiftTypeDTO>
                {
                },
                SkillNames = null
              },
              new EmployeePostDTO
              {
                EmployeePostId = 321,
                ActorCompanyId = 451,
                EmployeeGroupId = 80,
                ScheduleCycleId = 75,
                Name = "Post 321",
                Description = "",
                DateFrom = DateTime.ParseExact("2020-03-23T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                DateTo = null,
                WorkTimeWeek = 300,
                WorkTimePercent = 13.00m,
                DayOfWeeks = "",
                DayOfWeeksGenericType = null,
                OverWriteDayOfWeekIds = null,
                DayOfWeekIds = new List<int>
                {
                },
                DayOfWeeksGridString = null,
                WorkDaysWeek = 1,
                RemainingWorkDaysWeek = 1,
                Status = SoeEmployeePostStatus.None,
                Created = DateTime.ParseExact("2020-01-31T12:25:32.9900000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "70",
                Modified = DateTime.ParseExact("2020-01-31T14:36:57.4830000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ModifiedBy = "50",
                EmployeePostWeekendType = TermGroup_EmployeePostWeekendType.AutomaticWeekend,
                State = SoeEntityState.Active,
                AccountId = null,
                EmployeeGroupName = "HAO timavlönad Stämpla",
                EmployeePostSkillDTOs = new List<EmployeePostSkillDTO>
                {
                  new EmployeePostSkillDTO
                  {
                    EmployeePostSkillId = 1389,
                    EmployeePostId = 321,
                    SkillId = 352,
                    SkillLevel = 20,
                    DateTo = null,
                    SkillLevelStars = 0d,
                    SkillLevelUnreached = false,
                    SkillName = "Butik",
                    SkillTypeName = "allmän",
                    SkillDTO = new SkillDTO
                    {
                      SkillId = 352,
                      SkillTypeId = 77,
                      ActorCompanyId = 451,
                      Name = "Butik",
                      Description = null,
                      Created = DateTime.ParseExact("2018-04-24T09:36:55.0566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "70",
                      Modified = DateTime.ParseExact("2018-05-02T13:59:28.3066667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "ICA",
                      State = SoeEntityState.Active,
                      SkillTypeDTO = new SkillTypeDTO
                      {
                        SkillTypeId = 77,
                        ActorCompanyId = 451,
                        Name = "allmän",
                        Description = null,
                        Created = DateTime.ParseExact("2018-04-24T09:34:38.6233333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "70",
                        Modified = null,
                        ModifiedBy = null,
                        State = SoeEntityState.Active
                      },
                      SkillTypeName = null,
                    }
                  }
                },
                ScheduleCycleDTO = new ScheduleCycleDTO
                {
                  ScheduleCycleId = 75,
                  ActorCompanyId = 451,
                  Name = "Helgpersonal endast",
                  Description = "Jobbar ej vardagskvällar",
                  NbrOfWeeks = 4,
                  Created = DateTime.ParseExact("2020-01-31T13:40:41.4330000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  CreatedBy = "50",
                  Modified = null,
                  ModifiedBy = null,
                  State = SoeEntityState.Active,
                  ScheduleCycleRuleDTOs = new List<ScheduleCycleRuleDTO>
                  {
                    new ScheduleCycleRuleDTO
                    {
                      ScheduleCycleRuleId = 183,
                      ScheduleCycleId = 75,
                      ScheduleCycleRuleTypeId = 104,
                      MinOccurrences = 1,
                      MaxOccurrences = 4,
                      Created = DateTime.ParseExact("2020-01-31T13:40:41.4333333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "50",
                      Modified = null,
                      ModifiedBy = null,
                      State = SoeEntityState.Active,
                      ScheduleCycleRuleTypeDTO = new ScheduleCycleRuleTypeDTO
                      {
                        ScheduleCycleRuleTypeId = 104,
                        ActorCompanyId = 451,
                        Name = "Helg",
                        DayOfWeeks = "6,0",
                        DayOfWeekIds = new List<int>
                        {
                          6,
                          0
                        },
                        DayOfWeeksGridString = null,
                        StartTime = DateTime.ParseExact("1900-01-01T08:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        StopTime = DateTime.ParseExact("1900-01-01T21:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Created = DateTime.ParseExact("2020-01-31T13:38:11.7833333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "50",
                        Modified = null,
                        ModifiedBy = null,
                        State = SoeEntityState.Active
                      }
                    }
                  }
                },
                EmployeeGroupDTO = new EmployeeGroupDTO
                {
                  EmployeeGroupId = 80,
                  ActorCompanyId = 451,
                  TimeDeviationCauseId = 374,
                  TimeCodeId = null,
                  Name = "HAO timavlönad Stämpla",
                  DeviationAxelStartHours = 2,
                  DeviationAxelStopHours = 2,
                  PayrollProductAccountingPrio = "0,0,0,0,0",
                  InvoiceProductAccountingPrio = "0,0,0,0,0",
                  AutogenTimeblocks = false,
                  AutogenBreakOnStamping = true,
                  AlwaysDiscardBreakEvaluation = false,
                  MergeScheduleBreaksOnDay = true,
                  BreakDayMinutesAfterMidnight = 180,
                  KeepStampsTogetherWithinMinutes = 0,
                  RuleWorkTimeWeek = 2295,
                  RuleWorkTimeYear = 0,
                  RuleRestTimeDay = 660,
                  RuleRestTimeWeek = 2160,
                  MaxScheduleTimeFullTime = 540,
                  MinScheduleTimeFullTime = -540,
                  MaxScheduleTimePartTime = 300,
                  MinScheduleTimePartTime = -300,
                  MaxScheduleTimeWithoutBreaks = 300,
                  RuleWorkTimeDayMinimum = 180,
                  RuleWorkTimeDayMaximumWorkDay = 0,
                  RuleWorkTimeDayMaximumWeekend = 0,
                  Created = DateTime.ParseExact("2017-03-01T10:13:48.5470000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  CreatedBy = "BjörnS",
                  Modified = DateTime.ParseExact("2018-10-31T11:57:28.4500000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  ModifiedBy = "70",
                  State = SoeEntityState.Active,
                  TimeDeviationCause = null,
                  EmployeeGroupTimeDeviationCauseTimeCode = null,
                  TimeDeviationCausesNames = null,
                  DayTypesNames = null,
                  ExternalCodes = null,
                  TimeReportType = 0,
                  TimeReportTypeName = null,
                  ExternalCodesString = null
                },
                IgnoreDaysOfWeekIds = false,
                AccountName = "",
                ValidShiftTypes = new List<ShiftTypeDTO>
                {
                },
                SkillNames = null
              },
              new EmployeePostDTO
              {
                EmployeePostId = 316,
                ActorCompanyId = 451,
                EmployeeGroupId = 80,
                ScheduleCycleId = 35,
                Name = "Post 316",
                Description = "",
                DateFrom = DateTime.ParseExact("2020-03-23T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                DateTo = null,
                WorkTimeWeek = 1748,
                WorkTimePercent = 76.00m,
                DayOfWeeks = "",
                DayOfWeeksGenericType = null,
                OverWriteDayOfWeekIds = null,
                DayOfWeekIds = new List<int>
                {
                },
                DayOfWeeksGridString = null,
                WorkDaysWeek = 4,
                RemainingWorkDaysWeek = 4,
                Status = SoeEmployeePostStatus.None,
                Created = DateTime.ParseExact("2020-01-31T12:25:32.8670000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "70",
                Modified = DateTime.ParseExact("2020-01-31T15:19:33.9170000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ModifiedBy = "50",
                EmployeePostWeekendType = TermGroup_EmployeePostWeekendType.AutomaticWeekend,
                State = SoeEntityState.Active,
                AccountId = null,
                EmployeeGroupName = "HAO timavlönad Stämpla",
                EmployeePostSkillDTOs = new List<EmployeePostSkillDTO>
                {
                  new EmployeePostSkillDTO
                  {
                    EmployeePostSkillId = 1373,
                    EmployeePostId = 316,
                    SkillId = 352,
                    SkillLevel = 20,
                    DateTo = null,
                    SkillLevelStars = 0d,
                    SkillLevelUnreached = false,
                    SkillName = "Butik",
                    SkillTypeName = "allmän",
                    SkillDTO = new SkillDTO
                    {
                      SkillId = 352,
                      SkillTypeId = 77,
                      ActorCompanyId = 451,
                      Name = "Butik",
                      Description = null,
                      Created = DateTime.ParseExact("2018-04-24T09:36:55.0566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "70",
                      Modified = DateTime.ParseExact("2018-05-02T13:59:28.3066667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "ICA",
                      State = SoeEntityState.Active,
                      SkillTypeDTO = new SkillTypeDTO
                      {
                        SkillTypeId = 77,
                        ActorCompanyId = 451,
                        Name = "allmän",
                        Description = null,
                        Created = DateTime.ParseExact("2018-04-24T09:34:38.6233333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "70",
                        Modified = null,
                        ModifiedBy = null,
                        State = SoeEntityState.Active
                      },
                      SkillTypeName = null,
                    }
                  }
                },
                ScheduleCycleDTO = new ScheduleCycleDTO
                {
                  ScheduleCycleId = 35,
                  ActorCompanyId = 451,
                  Name = "Fast anställd",
                  Description = "",
                  NbrOfWeeks = 4,
                  Created = DateTime.ParseExact("2018-04-24T09:54:52.3330000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  CreatedBy = "70",
                  Modified = DateTime.ParseExact("2020-01-31T15:44:05.6630000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  ModifiedBy = "50",
                  State = SoeEntityState.Active,
                  ScheduleCycleRuleDTOs = new List<ScheduleCycleRuleDTO>
                  {
                    new ScheduleCycleRuleDTO
                    {
                      ScheduleCycleRuleId = 177,
                      ScheduleCycleId = 35,
                      ScheduleCycleRuleTypeId = 62,
                      MinOccurrences = 12,
                      MaxOccurrences = 16,
                      Created = DateTime.ParseExact("2020-01-31T13:34:49.2566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "50",
                      Modified = DateTime.ParseExact("2020-01-31T15:44:05.6800000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "50",
                      State = SoeEntityState.Active,
                      ScheduleCycleRuleTypeDTO = new ScheduleCycleRuleTypeDTO
                      {
                        ScheduleCycleRuleTypeId = 62,
                        ActorCompanyId = 451,
                        Name = "Vardag dag",
                        DayOfWeeks = "1,2,3,4,5",
                        DayOfWeekIds = new List<int>
                        {
                          1,
                          2,
                          3,
                          4,
                          5
                        },
                        DayOfWeeksGridString = null,
                        StartTime = DateTime.ParseExact("1900-01-01T08:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        StopTime = DateTime.ParseExact("1900-01-01T18:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Created = DateTime.ParseExact("2018-04-16T16:03:19.6733333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "ICA",
                        Modified = DateTime.ParseExact("2020-01-31T13:30:19.1900000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        ModifiedBy = "50",
                        State = SoeEntityState.Active
                      }
                    },
                    new ScheduleCycleRuleDTO
                    {
                      ScheduleCycleRuleId = 178,
                      ScheduleCycleId = 35,
                      ScheduleCycleRuleTypeId = 63,
                      MinOccurrences = 4,
                      MaxOccurrences = 4,
                      Created = DateTime.ParseExact("2020-01-31T13:34:49.2566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "50",
                      Modified = DateTime.ParseExact("2020-01-31T15:44:05.6800000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "50",
                      State = SoeEntityState.Active,
                      ScheduleCycleRuleTypeDTO = new ScheduleCycleRuleTypeDTO
                      {
                        ScheduleCycleRuleTypeId = 63,
                        ActorCompanyId = 451,
                        Name = "Vardag kväll",
                        DayOfWeeks = "1,2,3,4,5",
                        DayOfWeekIds = new List<int>
                        {
                          1,
                          2,
                          3,
                          4,
                          5
                        },
                        DayOfWeeksGridString = null,
                        StartTime = DateTime.ParseExact("1900-01-01T14:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        StopTime = DateTime.ParseExact("1900-01-01T21:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Created = DateTime.ParseExact("2018-04-24T09:48:25.4933333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "70",
                        Modified = DateTime.ParseExact("2020-01-31T15:46:13.2033333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        ModifiedBy = "50",
                        State = SoeEntityState.Active
                      }
                    },
                    new ScheduleCycleRuleDTO
                    {
                      ScheduleCycleRuleId = 179,
                      ScheduleCycleId = 35,
                      ScheduleCycleRuleTypeId = 66,
                      MinOccurrences = 1,
                      MaxOccurrences = 1,
                      Created = DateTime.ParseExact("2020-01-31T13:34:49.2566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "50",
                      Modified = DateTime.ParseExact("2020-01-31T15:44:05.6800000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "50",
                      State = SoeEntityState.Active,
                      ScheduleCycleRuleTypeDTO = new ScheduleCycleRuleTypeDTO
                      {
                        ScheduleCycleRuleTypeId = 66,
                        ActorCompanyId = 451,
                        Name = "Lördag dag",
                        DayOfWeeks = "6",
                        DayOfWeekIds = new List<int>
                        {
                          6
                        },
                        DayOfWeeksGridString = null,
                        StartTime = DateTime.ParseExact("1900-01-01T08:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        StopTime = DateTime.ParseExact("1900-01-01T17:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Created = DateTime.ParseExact("2018-05-14T14:00:11.8866667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "ICA",
                        Modified = DateTime.ParseExact("2020-01-31T13:31:33.6966667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        ModifiedBy = "50",
                        State = SoeEntityState.Active
                      }
                    },
                    new ScheduleCycleRuleDTO
                    {
                      ScheduleCycleRuleId = 180,
                      ScheduleCycleId = 35,
                      ScheduleCycleRuleTypeId = 101,
                      MinOccurrences = 1,
                      MaxOccurrences = 1,
                      Created = DateTime.ParseExact("2020-01-31T13:34:49.2566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "50",
                      Modified = DateTime.ParseExact("2020-01-31T15:44:05.6800000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "50",
                      State = SoeEntityState.Active,
                      ScheduleCycleRuleTypeDTO = new ScheduleCycleRuleTypeDTO
                      {
                        ScheduleCycleRuleTypeId = 101,
                        ActorCompanyId = 451,
                        Name = "Lördag kväll",
                        DayOfWeeks = "6",
                        DayOfWeekIds = new List<int>
                        {
                          6
                        },
                        DayOfWeeksGridString = null,
                        StartTime = DateTime.ParseExact("1900-01-01T15:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        StopTime = DateTime.ParseExact("1900-01-01T21:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Created = DateTime.ParseExact("2020-01-31T13:32:07.5566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "50",
                        Modified = null,
                        ModifiedBy = null,
                        State = SoeEntityState.Active
                      }
                    },
                    new ScheduleCycleRuleDTO
                    {
                      ScheduleCycleRuleId = 181,
                      ScheduleCycleId = 35,
                      ScheduleCycleRuleTypeId = 102,
                      MinOccurrences = 1,
                      MaxOccurrences = 1,
                      Created = DateTime.ParseExact("2020-01-31T13:34:49.2566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "50",
                      Modified = DateTime.ParseExact("2020-01-31T15:44:05.6800000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "50",
                      State = SoeEntityState.Active,
                      ScheduleCycleRuleTypeDTO = new ScheduleCycleRuleTypeDTO
                      {
                        ScheduleCycleRuleTypeId = 102,
                        ActorCompanyId = 451,
                        Name = "Söndag dag",
                        DayOfWeeks = "0",
                        DayOfWeekIds = new List<int>
                        {
                          0
                        },
                        DayOfWeeksGridString = null,
                        StartTime = DateTime.ParseExact("1900-01-01T08:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        StopTime = DateTime.ParseExact("1900-01-01T17:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Created = DateTime.ParseExact("2020-01-31T13:32:36.0300000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "50",
                        Modified = null,
                        ModifiedBy = null,
                        State = SoeEntityState.Active
                      }
                    },
                    new ScheduleCycleRuleDTO
                    {
                      ScheduleCycleRuleId = 182,
                      ScheduleCycleId = 35,
                      ScheduleCycleRuleTypeId = 103,
                      MinOccurrences = 1,
                      MaxOccurrences = 1,
                      Created = DateTime.ParseExact("2020-01-31T13:34:49.2566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "50",
                      Modified = DateTime.ParseExact("2020-01-31T15:44:05.6800000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "50",
                      State = SoeEntityState.Active,
                      ScheduleCycleRuleTypeDTO = new ScheduleCycleRuleTypeDTO
                      {
                        ScheduleCycleRuleTypeId = 103,
                        ActorCompanyId = 451,
                        Name = "Söndag kväll",
                        DayOfWeeks = "0",
                        DayOfWeekIds = new List<int>
                        {
                          0
                        },
                        DayOfWeeksGridString = null,
                        StartTime = DateTime.ParseExact("1900-01-01T15:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        StopTime = DateTime.ParseExact("1900-01-01T21:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Created = DateTime.ParseExact("2020-01-31T13:32:53.2800000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "50",
                        Modified = null,
                        ModifiedBy = null,
                        State = SoeEntityState.Active
                      }
                    }
                  }
                },
                EmployeeGroupDTO = new EmployeeGroupDTO
                {
                  EmployeeGroupId = 80,
                  ActorCompanyId = 451,
                  TimeDeviationCauseId = 374,
                  TimeCodeId = null,
                  Name = "HAO timavlönad Stämpla",
                  DeviationAxelStartHours = 2,
                  DeviationAxelStopHours = 2,
                  PayrollProductAccountingPrio = "0,0,0,0,0",
                  InvoiceProductAccountingPrio = "0,0,0,0,0",
                  AutogenTimeblocks = false,
                  AutogenBreakOnStamping = true,
                  AlwaysDiscardBreakEvaluation = false,
                  MergeScheduleBreaksOnDay = true,
                  BreakDayMinutesAfterMidnight = 180,
                  KeepStampsTogetherWithinMinutes = 0,
                  RuleWorkTimeWeek = 2295,
                  RuleWorkTimeYear = 0,
                  RuleRestTimeDay = 660,
                  RuleRestTimeWeek = 2160,
                  MaxScheduleTimeFullTime = 540,
                  MinScheduleTimeFullTime = -540,
                  MaxScheduleTimePartTime = 300,
                  MinScheduleTimePartTime = -300,
                  MaxScheduleTimeWithoutBreaks = 300,
                  RuleWorkTimeDayMinimum = 180,
                  RuleWorkTimeDayMaximumWorkDay = 0,
                  RuleWorkTimeDayMaximumWeekend = 0,
                  Created = DateTime.ParseExact("2017-03-01T10:13:48.5470000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  CreatedBy = "BjörnS",
                  Modified = DateTime.ParseExact("2018-10-31T11:57:28.4500000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  ModifiedBy = "70",
                  State = SoeEntityState.Active,
                  TimeDeviationCause = null,
                  EmployeeGroupTimeDeviationCauseTimeCode = null,
                  TimeDeviationCausesNames = null,
                  DayTypesNames = null,
                  ExternalCodes = null,
                  TimeReportType = 0,
                  TimeReportTypeName = null,
                  ExternalCodesString = null
                },
                IgnoreDaysOfWeekIds = false,
                AccountName = "",
                ValidShiftTypes = new List<ShiftTypeDTO>
                {
                },
                SkillNames = null
              },
              new EmployeePostDTO
              {
                EmployeePostId = 319,
                ActorCompanyId = 451,
                EmployeeGroupId = 80,
                ScheduleCycleId = 76,
                Name = "Post 319",
                Description = "",
                DateFrom = DateTime.ParseExact("2020-03-23T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                DateTo = null,
                WorkTimeWeek = 240,
                WorkTimePercent = 10.00m,
                DayOfWeeks = "",
                DayOfWeeksGenericType = null,
                OverWriteDayOfWeekIds = null,
                DayOfWeekIds = new List<int>
                {
                },
                DayOfWeeksGridString = null,
                WorkDaysWeek = 1,
                RemainingWorkDaysWeek = 1,
                Status = SoeEmployeePostStatus.None,
                Created = DateTime.ParseExact("2020-01-31T12:25:32.9430000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "70",
                Modified = DateTime.ParseExact("2020-01-31T14:27:20.0530000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ModifiedBy = "50",
                EmployeePostWeekendType = TermGroup_EmployeePostWeekendType.AutomaticWeekend,
                State = SoeEntityState.Active,
                AccountId = null,
                EmployeeGroupName = "HAO timavlönad Stämpla",
                EmployeePostSkillDTOs = new List<EmployeePostSkillDTO>
                {
                  new EmployeePostSkillDTO
                  {
                    EmployeePostSkillId = 1387,
                    EmployeePostId = 319,
                    SkillId = 352,
                    SkillLevel = 20,
                    DateTo = null,
                    SkillLevelStars = 0d,
                    SkillLevelUnreached = false,
                    SkillName = "Butik",
                    SkillTypeName = "allmän",
                    SkillDTO = new SkillDTO
                    {
                      SkillId = 352,
                      SkillTypeId = 77,
                      ActorCompanyId = 451,
                      Name = "Butik",
                      Description = null,
                      Created = DateTime.ParseExact("2018-04-24T09:36:55.0566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "70",
                      Modified = DateTime.ParseExact("2018-05-02T13:59:28.3066667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "ICA",
                      State = SoeEntityState.Active,
                      SkillTypeDTO = new SkillTypeDTO
                      {
                        SkillTypeId = 77,
                        ActorCompanyId = 451,
                        Name = "allmän",
                        Description = null,
                        Created = DateTime.ParseExact("2018-04-24T09:34:38.6233333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "70",
                        Modified = null,
                        ModifiedBy = null,
                        State = SoeEntityState.Active
                      },
                      SkillTypeName = null,
                    }
                  }
                },
                ScheduleCycleDTO = new ScheduleCycleDTO
                {
                  ScheduleCycleId = 76,
                  ActorCompanyId = 451,
                  Name = "Helgpersonal inkl kvällar",
                  Description = "Jobbar även vardagskvällar",
                  NbrOfWeeks = 4,
                  Created = DateTime.ParseExact("2020-01-31T13:44:40.8270000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  CreatedBy = "50",
                  Modified = null,
                  ModifiedBy = null,
                  State = SoeEntityState.Active,
                  ScheduleCycleRuleDTOs = new List<ScheduleCycleRuleDTO>
                  {
                    new ScheduleCycleRuleDTO
                    {
                      ScheduleCycleRuleId = 185,
                      ScheduleCycleId = 76,
                      ScheduleCycleRuleTypeId = 63,
                      MinOccurrences = 0,
                      MaxOccurrences = 4,
                      Created = DateTime.ParseExact("2020-01-31T13:44:40.8266667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "50",
                      Modified = null,
                      ModifiedBy = null,
                      State = SoeEntityState.Active,
                      ScheduleCycleRuleTypeDTO = new ScheduleCycleRuleTypeDTO
                      {
                        ScheduleCycleRuleTypeId = 63,
                        ActorCompanyId = 451,
                        Name = "Vardag kväll",
                        DayOfWeeks = "1,2,3,4,5",
                        DayOfWeekIds = new List<int>
                        {
                          1,
                          2,
                          3,
                          4,
                          5
                        },
                        DayOfWeeksGridString = null,
                        StartTime = DateTime.ParseExact("1900-01-01T14:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        StopTime = DateTime.ParseExact("1900-01-01T21:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Created = DateTime.ParseExact("2018-04-24T09:48:25.4933333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "70",
                        Modified = DateTime.ParseExact("2020-01-31T15:46:13.2033333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        ModifiedBy = "50",
                        State = SoeEntityState.Active
                      }
                    },
                    new ScheduleCycleRuleDTO
                    {
                      ScheduleCycleRuleId = 186,
                      ScheduleCycleId = 76,
                      ScheduleCycleRuleTypeId = 104,
                      MinOccurrences = 1,
                      MaxOccurrences = 4,
                      Created = DateTime.ParseExact("2020-01-31T13:44:40.8266667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "50",
                      Modified = null,
                      ModifiedBy = null,
                      State = SoeEntityState.Active,
                      ScheduleCycleRuleTypeDTO = new ScheduleCycleRuleTypeDTO
                      {
                        ScheduleCycleRuleTypeId = 104,
                        ActorCompanyId = 451,
                        Name = "Helg",
                        DayOfWeeks = "6,0",
                        DayOfWeekIds = new List<int>
                        {
                          6,
                          0
                        },
                        DayOfWeeksGridString = null,
                        StartTime = DateTime.ParseExact("1900-01-01T08:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        StopTime = DateTime.ParseExact("1900-01-01T21:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Created = DateTime.ParseExact("2020-01-31T13:38:11.7833333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "50",
                        Modified = null,
                        ModifiedBy = null,
                        State = SoeEntityState.Active
                      }
                    }
                  }
                },
                EmployeeGroupDTO = new EmployeeGroupDTO
                {
                  EmployeeGroupId = 80,
                  ActorCompanyId = 451,
                  TimeDeviationCauseId = 374,
                  TimeCodeId = null,
                  Name = "HAO timavlönad Stämpla",
                  DeviationAxelStartHours = 2,
                  DeviationAxelStopHours = 2,
                  PayrollProductAccountingPrio = "0,0,0,0,0",
                  InvoiceProductAccountingPrio = "0,0,0,0,0",
                  AutogenTimeblocks = false,
                  AutogenBreakOnStamping = true,
                  AlwaysDiscardBreakEvaluation = false,
                  MergeScheduleBreaksOnDay = true,
                  BreakDayMinutesAfterMidnight = 180,
                  KeepStampsTogetherWithinMinutes = 0,
                  RuleWorkTimeWeek = 2295,
                  RuleWorkTimeYear = 0,
                  RuleRestTimeDay = 660,
                  RuleRestTimeWeek = 2160,
                  MaxScheduleTimeFullTime = 540,
                  MinScheduleTimeFullTime = -540,
                  MaxScheduleTimePartTime = 300,
                  MinScheduleTimePartTime = -300,
                  MaxScheduleTimeWithoutBreaks = 300,
                  RuleWorkTimeDayMinimum = 180,
                  RuleWorkTimeDayMaximumWorkDay = 0,
                  RuleWorkTimeDayMaximumWeekend = 0,
                  Created = DateTime.ParseExact("2017-03-01T10:13:48.5470000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  CreatedBy = "BjörnS",
                  Modified = DateTime.ParseExact("2018-10-31T11:57:28.4500000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  ModifiedBy = "70",
                  State = SoeEntityState.Active,
                  TimeDeviationCause = null,
                  EmployeeGroupTimeDeviationCauseTimeCode = null,
                  TimeDeviationCausesNames = null,
                  DayTypesNames = null,
                  ExternalCodes = null,
                  TimeReportType = 0,
                  TimeReportTypeName = null,
                  ExternalCodesString = null
                },
                IgnoreDaysOfWeekIds = false,
                AccountName = "",
                ValidShiftTypes = new List<ShiftTypeDTO>
                {
                },
                SkillNames = null
              },
              new EmployeePostDTO
              {
                EmployeePostId = 136,
                ActorCompanyId = 451,
                EmployeeGroupId = 79,
                ScheduleCycleId = 37,
                Name = "Post 136",
                Description = "Marie Larsson",
                DateFrom = DateTime.ParseExact("2018-12-03T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                DateTo = DateTime.ParseExact("2020-01-31T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                WorkTimeWeek = 1197,
                WorkTimePercent = 50.00m,
                DayOfWeeks = "3",
                DayOfWeeksGenericType = null,
                OverWriteDayOfWeekIds = null,
                DayOfWeekIds = new List<int>
                {
                  3
                },
                DayOfWeeksGridString = "Onsdag",
                WorkDaysWeek = 5,
                RemainingWorkDaysWeek = 5,
                Status = SoeEmployeePostStatus.None,
                Created = DateTime.ParseExact("2018-04-30T09:52:13.1970000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "70",
                Modified = DateTime.ParseExact("2020-02-20T12:21:02.2770000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ModifiedBy = "ICA",
                EmployeePostWeekendType = TermGroup_EmployeePostWeekendType.PreferEvenWeekWeekend,
                State = SoeEntityState.Active,
                AccountId = null,
                EmployeeGroupName = "HAO månadsavlönad",
                EmployeePostSkillDTOs = new List<EmployeePostSkillDTO>
                {
                  new EmployeePostSkillDTO
                  {
                    EmployeePostSkillId = 564,
                    EmployeePostId = 136,
                    SkillId = 354,
                    SkillLevel = 20,
                    DateTo = null,
                    SkillLevelStars = 0d,
                    SkillLevelUnreached = false,
                    SkillName = "Frukt",
                    SkillTypeName = "allmän",
                    SkillDTO = new SkillDTO
                    {
                      SkillId = 354,
                      SkillTypeId = 77,
                      ActorCompanyId = 451,
                      Name = "Frukt",
                      Description = null,
                      Created = DateTime.ParseExact("2018-05-02T13:59:38.4300000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "ICA",
                      Modified = null,
                      ModifiedBy = null,
                      State = SoeEntityState.Active,
                      SkillTypeDTO = new SkillTypeDTO
                      {
                        SkillTypeId = 77,
                        ActorCompanyId = 451,
                        Name = "allmän",
                        Description = null,
                        Created = DateTime.ParseExact("2018-04-24T09:34:38.6233333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "70",
                        Modified = null,
                        ModifiedBy = null,
                        State = SoeEntityState.Active
                      },
                      SkillTypeName = null,
                    }
                  },
                  new EmployeePostSkillDTO
                  {
                    EmployeePostSkillId = 1213,
                    EmployeePostId = 136,
                    SkillId = 352,
                    SkillLevel = 20,
                    DateTo = null,
                    SkillLevelStars = 0d,
                    SkillLevelUnreached = false,
                    SkillName = "Butik",
                    SkillTypeName = "allmän",
                    SkillDTO = new SkillDTO
                    {
                      SkillId = 352,
                      SkillTypeId = 77,
                      ActorCompanyId = 451,
                      Name = "Butik",
                      Description = null,
                      Created = DateTime.ParseExact("2018-04-24T09:36:55.0566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "70",
                      Modified = DateTime.ParseExact("2018-05-02T13:59:28.3066667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "ICA",
                      State = SoeEntityState.Active,
                      SkillTypeDTO = new SkillTypeDTO
                      {
                        SkillTypeId = 77,
                        ActorCompanyId = 451,
                        Name = "allmän",
                        Description = null,
                        Created = DateTime.ParseExact("2018-04-24T09:34:38.6233333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "70",
                        Modified = null,
                        ModifiedBy = null,
                        State = SoeEntityState.Active
                      },
                      SkillTypeName = null,
                    }
                  }
                },
                ScheduleCycleDTO = new ScheduleCycleDTO
                {
                  ScheduleCycleId = 37,
                  ActorCompanyId = 451,
                  Name = "Butikschef",
                  Description = "",
                  NbrOfWeeks = 4,
                  Created = DateTime.ParseExact("2018-04-25T15:57:13.0900000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  CreatedBy = "70",
                  Modified = DateTime.ParseExact("2020-01-31T13:36:13.7770000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  ModifiedBy = "50",
                  State = SoeEntityState.Active,
                  ScheduleCycleRuleDTOs = new List<ScheduleCycleRuleDTO>
                  {
                    new ScheduleCycleRuleDTO
                    {
                      ScheduleCycleRuleId = 94,
                      ScheduleCycleId = 37,
                      ScheduleCycleRuleTypeId = 62,
                      MinOccurrences = 14,
                      MaxOccurrences = 14,
                      Created = DateTime.ParseExact("2018-04-25T15:57:13.0900000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "70",
                      Modified = DateTime.ParseExact("2020-01-31T13:36:13.7766667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "50",
                      State = SoeEntityState.Active,
                      ScheduleCycleRuleTypeDTO = new ScheduleCycleRuleTypeDTO
                      {
                        ScheduleCycleRuleTypeId = 62,
                        ActorCompanyId = 451,
                        Name = "Vardag dag",
                        DayOfWeeks = "1,2,3,4,5",
                        DayOfWeekIds = new List<int>
                        {
                          1,
                          2,
                          3,
                          4,
                          5
                        },
                        DayOfWeeksGridString = null,
                        StartTime = DateTime.ParseExact("1900-01-01T08:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        StopTime = DateTime.ParseExact("1900-01-01T18:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Created = DateTime.ParseExact("2018-04-16T16:03:19.6733333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "ICA",
                        Modified = DateTime.ParseExact("2020-01-31T13:30:19.1900000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        ModifiedBy = "50",
                        State = SoeEntityState.Active
                      }
                    },
                    new ScheduleCycleRuleDTO
                    {
                      ScheduleCycleRuleId = 95,
                      ScheduleCycleId = 37,
                      ScheduleCycleRuleTypeId = 63,
                      MinOccurrences = 4,
                      MaxOccurrences = 4,
                      Created = DateTime.ParseExact("2018-04-25T15:57:26.3866667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "70",
                      Modified = DateTime.ParseExact("2020-01-31T13:36:13.7766667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "50",
                      State = SoeEntityState.Active,
                      ScheduleCycleRuleTypeDTO = new ScheduleCycleRuleTypeDTO
                      {
                        ScheduleCycleRuleTypeId = 63,
                        ActorCompanyId = 451,
                        Name = "Vardag kväll",
                        DayOfWeeks = "1,2,3,4,5",
                        DayOfWeekIds = new List<int>
                        {
                          1,
                          2,
                          3,
                          4,
                          5
                        },
                        DayOfWeeksGridString = null,
                        StartTime = DateTime.ParseExact("1900-01-01T14:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        StopTime = DateTime.ParseExact("1900-01-01T21:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Created = DateTime.ParseExact("2018-04-24T09:48:25.4933333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "70",
                        Modified = DateTime.ParseExact("2020-01-31T15:46:13.2033333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        ModifiedBy = "50",
                        State = SoeEntityState.Active
                      }
                    },
                    new ScheduleCycleRuleDTO
                    {
                      ScheduleCycleRuleId = 97,
                      ScheduleCycleId = 37,
                      ScheduleCycleRuleTypeId = 66,
                      MinOccurrences = 2,
                      MaxOccurrences = 2,
                      Created = DateTime.ParseExact("2018-04-25T15:57:54.9500000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "70",
                      Modified = DateTime.ParseExact("2020-01-31T13:36:13.7766667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "50",
                      State = SoeEntityState.Active,
                      ScheduleCycleRuleTypeDTO = new ScheduleCycleRuleTypeDTO
                      {
                        ScheduleCycleRuleTypeId = 66,
                        ActorCompanyId = 451,
                        Name = "Lördag dag",
                        DayOfWeeks = "6",
                        DayOfWeekIds = new List<int>
                        {
                          6
                        },
                        DayOfWeeksGridString = null,
                        StartTime = DateTime.ParseExact("1900-01-01T08:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        StopTime = DateTime.ParseExact("1900-01-01T17:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Created = DateTime.ParseExact("2018-05-14T14:00:11.8866667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "ICA",
                        Modified = DateTime.ParseExact("2020-01-31T13:31:33.6966667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        ModifiedBy = "50",
                        State = SoeEntityState.Active
                      }
                    }
                  }
                },
                EmployeeGroupDTO = new EmployeeGroupDTO
                {
                  EmployeeGroupId = 79,
                  ActorCompanyId = 451,
                  TimeDeviationCauseId = 374,
                  TimeCodeId = null,
                  Name = "HAO månadsavlönad",
                  DeviationAxelStartHours = 2,
                  DeviationAxelStopHours = 2,
                  PayrollProductAccountingPrio = "0,0,0,0,0",
                  InvoiceProductAccountingPrio = "0,0,0,0,0",
                  AutogenTimeblocks = false,
                  AutogenBreakOnStamping = true,
                  AlwaysDiscardBreakEvaluation = false,
                  MergeScheduleBreaksOnDay = true,
                  BreakDayMinutesAfterMidnight = 180,
                  KeepStampsTogetherWithinMinutes = 0,
                  RuleWorkTimeWeek = 2400,
                  RuleWorkTimeYear = 0,
                  RuleRestTimeDay = 660,
                  RuleRestTimeWeek = 2160,
                  MaxScheduleTimeFullTime = 540,
                  MinScheduleTimeFullTime = -540,
                  MaxScheduleTimePartTime = 300,
                  MinScheduleTimePartTime = -300,
                  MaxScheduleTimeWithoutBreaks = 300,
                  RuleWorkTimeDayMinimum = 180,
                  RuleWorkTimeDayMaximumWorkDay = 0,
                  RuleWorkTimeDayMaximumWeekend = 0,
                  Created = DateTime.ParseExact("2017-03-01T10:13:48.1700000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  CreatedBy = "BjörnS",
                  Modified = DateTime.ParseExact("2018-12-10T14:28:04.2670000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  ModifiedBy = "70",
                  State = SoeEntityState.Active,
                  TimeDeviationCause = null,
                  EmployeeGroupTimeDeviationCauseTimeCode = null,
                  TimeDeviationCausesNames = null,
                  DayTypesNames = null,
                  ExternalCodes = null,
                  TimeReportType = 0,
                  TimeReportTypeName = null,
                  ExternalCodesString = null
                },
                IgnoreDaysOfWeekIds = false,
                AccountName = "",
                ValidShiftTypes = new List<ShiftTypeDTO>
                {
                },
                SkillNames = null
              },
              new EmployeePostDTO
              {
                EmployeePostId = 137,
                ActorCompanyId = 451,
                EmployeeGroupId = 79,
                ScheduleCycleId = 37,
                Name = "Post 137",
                Description = "Christian Palmén",
                DateFrom = DateTime.ParseExact("2018-12-03T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                DateTo = DateTime.ParseExact("2020-01-31T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                WorkTimeWeek = 957,
                WorkTimePercent = 40.00m,
                DayOfWeeks = "3",
                DayOfWeeksGenericType = null,
                OverWriteDayOfWeekIds = null,
                DayOfWeekIds = new List<int>
                {
                  3
                },
                DayOfWeeksGridString = "Onsdag",
                WorkDaysWeek = 5,
                RemainingWorkDaysWeek = 5,
                Status = SoeEmployeePostStatus.None,
                Created = DateTime.ParseExact("2018-04-30T09:52:13.2270000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "70",
                Modified = DateTime.ParseExact("2020-02-20T12:21:20.9500000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ModifiedBy = "ICA",
                EmployeePostWeekendType = TermGroup_EmployeePostWeekendType.PreferEvenWeekWeekend,
                State = SoeEntityState.Active,
                AccountId = null,
                EmployeeGroupName = "HAO månadsavlönad",
                EmployeePostSkillDTOs = new List<EmployeePostSkillDTO>
                {
                  new EmployeePostSkillDTO
                  {
                    EmployeePostSkillId = 566,
                    EmployeePostId = 137,
                    SkillId = 355,
                    SkillLevel = 20,
                    DateTo = null,
                    SkillLevelStars = 0d,
                    SkillLevelUnreached = false,
                    SkillName = "Kolonial",
                    SkillTypeName = "allmän",
                    SkillDTO = new SkillDTO
                    {
                      SkillId = 355,
                      SkillTypeId = 77,
                      ActorCompanyId = 451,
                      Name = "Kolonial",
                      Description = null,
                      Created = DateTime.ParseExact("2018-05-02T13:59:50.0400000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "ICA",
                      Modified = null,
                      ModifiedBy = null,
                      State = SoeEntityState.Active,
                      SkillTypeDTO = new SkillTypeDTO
                      {
                        SkillTypeId = 77,
                        ActorCompanyId = 451,
                        Name = "allmän",
                        Description = null,
                        Created = DateTime.ParseExact("2018-04-24T09:34:38.6233333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "70",
                        Modified = null,
                        ModifiedBy = null,
                        State = SoeEntityState.Active
                      },
                      SkillTypeName = null,
                    }
                  },
                  new EmployeePostSkillDTO
                  {
                    EmployeePostSkillId = 1212,
                    EmployeePostId = 137,
                    SkillId = 352,
                    SkillLevel = 20,
                    DateTo = null,
                    SkillLevelStars = 0d,
                    SkillLevelUnreached = false,
                    SkillName = "Butik",
                    SkillTypeName = "allmän",
                    SkillDTO = new SkillDTO
                    {
                      SkillId = 352,
                      SkillTypeId = 77,
                      ActorCompanyId = 451,
                      Name = "Butik",
                      Description = null,
                      Created = DateTime.ParseExact("2018-04-24T09:36:55.0566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "70",
                      Modified = DateTime.ParseExact("2018-05-02T13:59:28.3066667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "ICA",
                      State = SoeEntityState.Active,
                      SkillTypeDTO = new SkillTypeDTO
                      {
                        SkillTypeId = 77,
                        ActorCompanyId = 451,
                        Name = "allmän",
                        Description = null,
                        Created = DateTime.ParseExact("2018-04-24T09:34:38.6233333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "70",
                        Modified = null,
                        ModifiedBy = null,
                        State = SoeEntityState.Active
                      },
                      SkillTypeName = null,
                    }
                  }
                },
                ScheduleCycleDTO = new ScheduleCycleDTO
                {
                  ScheduleCycleId = 37,
                  ActorCompanyId = 451,
                  Name = "Butikschef",
                  Description = "",
                  NbrOfWeeks = 4,
                  Created = DateTime.ParseExact("2018-04-25T15:57:13.0900000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  CreatedBy = "70",
                  Modified = DateTime.ParseExact("2020-01-31T13:36:13.7770000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  ModifiedBy = "50",
                  State = SoeEntityState.Active,
                  ScheduleCycleRuleDTOs = new List<ScheduleCycleRuleDTO>
                  {
                    new ScheduleCycleRuleDTO
                    {
                      ScheduleCycleRuleId = 94,
                      ScheduleCycleId = 37,
                      ScheduleCycleRuleTypeId = 62,
                      MinOccurrences = 14,
                      MaxOccurrences = 14,
                      Created = DateTime.ParseExact("2018-04-25T15:57:13.0900000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "70",
                      Modified = DateTime.ParseExact("2020-01-31T13:36:13.7766667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "50",
                      State = SoeEntityState.Active,
                      ScheduleCycleRuleTypeDTO = new ScheduleCycleRuleTypeDTO
                      {
                        ScheduleCycleRuleTypeId = 62,
                        ActorCompanyId = 451,
                        Name = "Vardag dag",
                        DayOfWeeks = "1,2,3,4,5",
                        DayOfWeekIds = new List<int>
                        {
                          1,
                          2,
                          3,
                          4,
                          5
                        },
                        DayOfWeeksGridString = null,
                        StartTime = DateTime.ParseExact("1900-01-01T08:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        StopTime = DateTime.ParseExact("1900-01-01T18:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Created = DateTime.ParseExact("2018-04-16T16:03:19.6733333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "ICA",
                        Modified = DateTime.ParseExact("2020-01-31T13:30:19.1900000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        ModifiedBy = "50",
                        State = SoeEntityState.Active
                      }
                    },
                    new ScheduleCycleRuleDTO
                    {
                      ScheduleCycleRuleId = 95,
                      ScheduleCycleId = 37,
                      ScheduleCycleRuleTypeId = 63,
                      MinOccurrences = 4,
                      MaxOccurrences = 4,
                      Created = DateTime.ParseExact("2018-04-25T15:57:26.3866667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "70",
                      Modified = DateTime.ParseExact("2020-01-31T13:36:13.7766667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "50",
                      State = SoeEntityState.Active,
                      ScheduleCycleRuleTypeDTO = new ScheduleCycleRuleTypeDTO
                      {
                        ScheduleCycleRuleTypeId = 63,
                        ActorCompanyId = 451,
                        Name = "Vardag kväll",
                        DayOfWeeks = "1,2,3,4,5",
                        DayOfWeekIds = new List<int>
                        {
                          1,
                          2,
                          3,
                          4,
                          5
                        },
                        DayOfWeeksGridString = null,
                        StartTime = DateTime.ParseExact("1900-01-01T14:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        StopTime = DateTime.ParseExact("1900-01-01T21:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Created = DateTime.ParseExact("2018-04-24T09:48:25.4933333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "70",
                        Modified = DateTime.ParseExact("2020-01-31T15:46:13.2033333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        ModifiedBy = "50",
                        State = SoeEntityState.Active
                      }
                    },
                    new ScheduleCycleRuleDTO
                    {
                      ScheduleCycleRuleId = 97,
                      ScheduleCycleId = 37,
                      ScheduleCycleRuleTypeId = 66,
                      MinOccurrences = 2,
                      MaxOccurrences = 2,
                      Created = DateTime.ParseExact("2018-04-25T15:57:54.9500000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "70",
                      Modified = DateTime.ParseExact("2020-01-31T13:36:13.7766667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "50",
                      State = SoeEntityState.Active,
                      ScheduleCycleRuleTypeDTO = new ScheduleCycleRuleTypeDTO
                      {
                        ScheduleCycleRuleTypeId = 66,
                        ActorCompanyId = 451,
                        Name = "Lördag dag",
                        DayOfWeeks = "6",
                        DayOfWeekIds = new List<int>
                        {
                          6
                        },
                        DayOfWeeksGridString = null,
                        StartTime = DateTime.ParseExact("1900-01-01T08:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        StopTime = DateTime.ParseExact("1900-01-01T17:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Created = DateTime.ParseExact("2018-05-14T14:00:11.8866667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "ICA",
                        Modified = DateTime.ParseExact("2020-01-31T13:31:33.6966667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        ModifiedBy = "50",
                        State = SoeEntityState.Active
                      }
                    }
                  }
                },
                EmployeeGroupDTO = new EmployeeGroupDTO
                {
                  EmployeeGroupId = 79,
                  ActorCompanyId = 451,
                  TimeDeviationCauseId = 374,
                  TimeCodeId = null,
                  Name = "HAO månadsavlönad",
                  DeviationAxelStartHours = 2,
                  DeviationAxelStopHours = 2,
                  PayrollProductAccountingPrio = "0,0,0,0,0",
                  InvoiceProductAccountingPrio = "0,0,0,0,0",
                  AutogenTimeblocks = false,
                  AutogenBreakOnStamping = true,
                  AlwaysDiscardBreakEvaluation = false,
                  MergeScheduleBreaksOnDay = true,
                  BreakDayMinutesAfterMidnight = 180,
                  KeepStampsTogetherWithinMinutes = 0,
                  RuleWorkTimeWeek = 2400,
                  RuleWorkTimeYear = 0,
                  RuleRestTimeDay = 660,
                  RuleRestTimeWeek = 2160,
                  MaxScheduleTimeFullTime = 540,
                  MinScheduleTimeFullTime = -540,
                  MaxScheduleTimePartTime = 300,
                  MinScheduleTimePartTime = -300,
                  MaxScheduleTimeWithoutBreaks = 300,
                  RuleWorkTimeDayMinimum = 180,
                  RuleWorkTimeDayMaximumWorkDay = 0,
                  RuleWorkTimeDayMaximumWeekend = 0,
                  Created = DateTime.ParseExact("2017-03-01T10:13:48.1700000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  CreatedBy = "BjörnS",
                  Modified = DateTime.ParseExact("2018-12-10T14:28:04.2670000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  ModifiedBy = "70",
                  State = SoeEntityState.Active,
                  TimeDeviationCause = null,
                  EmployeeGroupTimeDeviationCauseTimeCode = null,
                  TimeDeviationCausesNames = null,
                  DayTypesNames = null,
                  ExternalCodes = null,
                  TimeReportType = 0,
                  TimeReportTypeName = null,
                  ExternalCodesString = null
                },
                IgnoreDaysOfWeekIds = false,
                AccountName = "",
                ValidShiftTypes = new List<ShiftTypeDTO>
                {
                },
                SkillNames = null
              },
              new EmployeePostDTO
              {
                EmployeePostId = 313,
                ActorCompanyId = 451,
                EmployeeGroupId = 80,
                ScheduleCycleId = 35,
                Name = "Post 313",
                Description = "",
                DateFrom = DateTime.ParseExact("2020-03-23T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                DateTo = null,
                WorkTimeWeek = 1625,
                WorkTimePercent = 71.00m,
                DayOfWeeks = "",
                DayOfWeeksGenericType = null,
                OverWriteDayOfWeekIds = null,
                DayOfWeekIds = new List<int>
                {
                },
                DayOfWeeksGridString = null,
                WorkDaysWeek = 4,
                RemainingWorkDaysWeek = 4,
                Status = SoeEmployeePostStatus.None,
                Created = DateTime.ParseExact("2020-01-31T12:25:32.7570000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "70",
                Modified = DateTime.ParseExact("2020-01-31T15:19:48.8700000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ModifiedBy = "50",
                EmployeePostWeekendType = TermGroup_EmployeePostWeekendType.AutomaticWeekend,
                State = SoeEntityState.Active,
                AccountId = null,
                EmployeeGroupName = "HAO timavlönad Stämpla",
                EmployeePostSkillDTOs = new List<EmployeePostSkillDTO>
                {
                  new EmployeePostSkillDTO
                  {
                    EmployeePostSkillId = 1370,
                    EmployeePostId = 313,
                    SkillId = 352,
                    SkillLevel = 20,
                    DateTo = null,
                    SkillLevelStars = 0d,
                    SkillLevelUnreached = false,
                    SkillName = "Butik",
                    SkillTypeName = "allmän",
                    SkillDTO = new SkillDTO
                    {
                      SkillId = 352,
                      SkillTypeId = 77,
                      ActorCompanyId = 451,
                      Name = "Butik",
                      Description = null,
                      Created = DateTime.ParseExact("2018-04-24T09:36:55.0566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "70",
                      Modified = DateTime.ParseExact("2018-05-02T13:59:28.3066667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "ICA",
                      State = SoeEntityState.Active,
                      SkillTypeDTO = new SkillTypeDTO
                      {
                        SkillTypeId = 77,
                        ActorCompanyId = 451,
                        Name = "allmän",
                        Description = null,
                        Created = DateTime.ParseExact("2018-04-24T09:34:38.6233333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "70",
                        Modified = null,
                        ModifiedBy = null,
                        State = SoeEntityState.Active
                      },
                      SkillTypeName = null,
                    }
                  }
                },
                ScheduleCycleDTO = new ScheduleCycleDTO
                {
                  ScheduleCycleId = 35,
                  ActorCompanyId = 451,
                  Name = "Fast anställd",
                  Description = "",
                  NbrOfWeeks = 4,
                  Created = DateTime.ParseExact("2018-04-24T09:54:52.3330000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  CreatedBy = "70",
                  Modified = DateTime.ParseExact("2020-01-31T15:44:05.6630000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  ModifiedBy = "50",
                  State = SoeEntityState.Active,
                  ScheduleCycleRuleDTOs = new List<ScheduleCycleRuleDTO>
                  {
                    new ScheduleCycleRuleDTO
                    {
                      ScheduleCycleRuleId = 177,
                      ScheduleCycleId = 35,
                      ScheduleCycleRuleTypeId = 62,
                      MinOccurrences = 12,
                      MaxOccurrences = 16,
                      Created = DateTime.ParseExact("2020-01-31T13:34:49.2566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "50",
                      Modified = DateTime.ParseExact("2020-01-31T15:44:05.6800000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "50",
                      State = SoeEntityState.Active,
                      ScheduleCycleRuleTypeDTO = new ScheduleCycleRuleTypeDTO
                      {
                        ScheduleCycleRuleTypeId = 62,
                        ActorCompanyId = 451,
                        Name = "Vardag dag",
                        DayOfWeeks = "1,2,3,4,5",
                        DayOfWeekIds = new List<int>
                        {
                          1,
                          2,
                          3,
                          4,
                          5
                        },
                        DayOfWeeksGridString = null,
                        StartTime = DateTime.ParseExact("1900-01-01T08:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        StopTime = DateTime.ParseExact("1900-01-01T18:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Created = DateTime.ParseExact("2018-04-16T16:03:19.6733333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "ICA",
                        Modified = DateTime.ParseExact("2020-01-31T13:30:19.1900000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        ModifiedBy = "50",
                        State = SoeEntityState.Active
                      }
                    },
                    new ScheduleCycleRuleDTO
                    {
                      ScheduleCycleRuleId = 178,
                      ScheduleCycleId = 35,
                      ScheduleCycleRuleTypeId = 63,
                      MinOccurrences = 4,
                      MaxOccurrences = 4,
                      Created = DateTime.ParseExact("2020-01-31T13:34:49.2566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "50",
                      Modified = DateTime.ParseExact("2020-01-31T15:44:05.6800000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "50",
                      State = SoeEntityState.Active,
                      ScheduleCycleRuleTypeDTO = new ScheduleCycleRuleTypeDTO
                      {
                        ScheduleCycleRuleTypeId = 63,
                        ActorCompanyId = 451,
                        Name = "Vardag kväll",
                        DayOfWeeks = "1,2,3,4,5",
                        DayOfWeekIds = new List<int>
                        {
                          1,
                          2,
                          3,
                          4,
                          5
                        },
                        DayOfWeeksGridString = null,
                        StartTime = DateTime.ParseExact("1900-01-01T14:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        StopTime = DateTime.ParseExact("1900-01-01T21:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Created = DateTime.ParseExact("2018-04-24T09:48:25.4933333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "70",
                        Modified = DateTime.ParseExact("2020-01-31T15:46:13.2033333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        ModifiedBy = "50",
                        State = SoeEntityState.Active
                      }
                    },
                    new ScheduleCycleRuleDTO
                    {
                      ScheduleCycleRuleId = 179,
                      ScheduleCycleId = 35,
                      ScheduleCycleRuleTypeId = 66,
                      MinOccurrences = 1,
                      MaxOccurrences = 1,
                      Created = DateTime.ParseExact("2020-01-31T13:34:49.2566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "50",
                      Modified = DateTime.ParseExact("2020-01-31T15:44:05.6800000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "50",
                      State = SoeEntityState.Active,
                      ScheduleCycleRuleTypeDTO = new ScheduleCycleRuleTypeDTO
                      {
                        ScheduleCycleRuleTypeId = 66,
                        ActorCompanyId = 451,
                        Name = "Lördag dag",
                        DayOfWeeks = "6",
                        DayOfWeekIds = new List<int>
                        {
                          6
                        },
                        DayOfWeeksGridString = null,
                        StartTime = DateTime.ParseExact("1900-01-01T08:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        StopTime = DateTime.ParseExact("1900-01-01T17:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Created = DateTime.ParseExact("2018-05-14T14:00:11.8866667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "ICA",
                        Modified = DateTime.ParseExact("2020-01-31T13:31:33.6966667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        ModifiedBy = "50",
                        State = SoeEntityState.Active
                      }
                    },
                    new ScheduleCycleRuleDTO
                    {
                      ScheduleCycleRuleId = 180,
                      ScheduleCycleId = 35,
                      ScheduleCycleRuleTypeId = 101,
                      MinOccurrences = 1,
                      MaxOccurrences = 1,
                      Created = DateTime.ParseExact("2020-01-31T13:34:49.2566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "50",
                      Modified = DateTime.ParseExact("2020-01-31T15:44:05.6800000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "50",
                      State = SoeEntityState.Active,
                      ScheduleCycleRuleTypeDTO = new ScheduleCycleRuleTypeDTO
                      {
                        ScheduleCycleRuleTypeId = 101,
                        ActorCompanyId = 451,
                        Name = "Lördag kväll",
                        DayOfWeeks = "6",
                        DayOfWeekIds = new List<int>
                        {
                          6
                        },
                        DayOfWeeksGridString = null,
                        StartTime = DateTime.ParseExact("1900-01-01T15:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        StopTime = DateTime.ParseExact("1900-01-01T21:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Created = DateTime.ParseExact("2020-01-31T13:32:07.5566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "50",
                        Modified = null,
                        ModifiedBy = null,
                        State = SoeEntityState.Active
                      }
                    },
                    new ScheduleCycleRuleDTO
                    {
                      ScheduleCycleRuleId = 181,
                      ScheduleCycleId = 35,
                      ScheduleCycleRuleTypeId = 102,
                      MinOccurrences = 1,
                      MaxOccurrences = 1,
                      Created = DateTime.ParseExact("2020-01-31T13:34:49.2566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "50",
                      Modified = DateTime.ParseExact("2020-01-31T15:44:05.6800000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "50",
                      State = SoeEntityState.Active,
                      ScheduleCycleRuleTypeDTO = new ScheduleCycleRuleTypeDTO
                      {
                        ScheduleCycleRuleTypeId = 102,
                        ActorCompanyId = 451,
                        Name = "Söndag dag",
                        DayOfWeeks = "0",
                        DayOfWeekIds = new List<int>
                        {
                          0
                        },
                        DayOfWeeksGridString = null,
                        StartTime = DateTime.ParseExact("1900-01-01T08:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        StopTime = DateTime.ParseExact("1900-01-01T17:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Created = DateTime.ParseExact("2020-01-31T13:32:36.0300000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "50",
                        Modified = null,
                        ModifiedBy = null,
                        State = SoeEntityState.Active
                      }
                    },
                    new ScheduleCycleRuleDTO
                    {
                      ScheduleCycleRuleId = 182,
                      ScheduleCycleId = 35,
                      ScheduleCycleRuleTypeId = 103,
                      MinOccurrences = 1,
                      MaxOccurrences = 1,
                      Created = DateTime.ParseExact("2020-01-31T13:34:49.2566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      CreatedBy = "50",
                      Modified = DateTime.ParseExact("2020-01-31T15:44:05.6800000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                      ModifiedBy = "50",
                      State = SoeEntityState.Active,
                      ScheduleCycleRuleTypeDTO = new ScheduleCycleRuleTypeDTO
                      {
                        ScheduleCycleRuleTypeId = 103,
                        ActorCompanyId = 451,
                        Name = "Söndag kväll",
                        DayOfWeeks = "0",
                        DayOfWeekIds = new List<int>
                        {
                          0
                        },
                        DayOfWeeksGridString = null,
                        StartTime = DateTime.ParseExact("1900-01-01T15:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        StopTime = DateTime.ParseExact("1900-01-01T21:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Created = DateTime.ParseExact("2020-01-31T13:32:53.2800000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        CreatedBy = "50",
                        Modified = null,
                        ModifiedBy = null,
                        State = SoeEntityState.Active
                      }
                    }
                  }
                },
                EmployeeGroupDTO = new EmployeeGroupDTO
                {
                  EmployeeGroupId = 80,
                  ActorCompanyId = 451,
                  TimeDeviationCauseId = 374,
                  TimeCodeId = null,
                  Name = "HAO timavlönad Stämpla",
                  DeviationAxelStartHours = 2,
                  DeviationAxelStopHours = 2,
                  PayrollProductAccountingPrio = "0,0,0,0,0",
                  InvoiceProductAccountingPrio = "0,0,0,0,0",
                  AutogenTimeblocks = false,
                  AutogenBreakOnStamping = true,
                  AlwaysDiscardBreakEvaluation = false,
                  MergeScheduleBreaksOnDay = true,
                  BreakDayMinutesAfterMidnight = 180,
                  KeepStampsTogetherWithinMinutes = 0,
                  RuleWorkTimeWeek = 2295,
                  RuleWorkTimeYear = 0,
                  RuleRestTimeDay = 660,
                  RuleRestTimeWeek = 2160,
                  MaxScheduleTimeFullTime = 540,
                  MinScheduleTimeFullTime = -540,
                  MaxScheduleTimePartTime = 300,
                  MinScheduleTimePartTime = -300,
                  MaxScheduleTimeWithoutBreaks = 300,
                  RuleWorkTimeDayMinimum = 180,
                  RuleWorkTimeDayMaximumWorkDay = 0,
                  RuleWorkTimeDayMaximumWeekend = 0,
                  Created = DateTime.ParseExact("2017-03-01T10:13:48.5470000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  CreatedBy = "BjörnS",
                  Modified = DateTime.ParseExact("2018-10-31T11:57:28.4500000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  ModifiedBy = "70",
                  State = SoeEntityState.Active,
                  TimeDeviationCause = null,
                  EmployeeGroupTimeDeviationCauseTimeCode = null,
                  TimeDeviationCausesNames = null,
                  DayTypesNames = null,
                  ExternalCodes = null,
                  TimeReportType = 0,
                  TimeReportTypeName = null,
                  ExternalCodesString = null
                },
                IgnoreDaysOfWeekIds = false,
                AccountName = "",
                ValidShiftTypes = new List<ShiftTypeDTO>
                {
                },
                SkillNames = null
              }
            };
            return listOfEmployeePostDTOs;
        }
    }
}
