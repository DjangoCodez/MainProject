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
    public static class TimeCodeBreakGroupMock
    {
        public static List<TimeCodeBreakGroupDTO> GetTimeCodeBreakGroups(StaffingNeedMockScenario staffingNeedMockScenario)
        {
            switch (staffingNeedMockScenario)
            {
                case StaffingNeedMockScenario.All:
                case StaffingNeedMockScenario.FourtyHours:
                    return GetTimeCodeBreakGroups();
                default:
                    return GetTimeCodeBreakGroups();
            }
        }

        public static List<TimeCodeBreakGroupDTO> GetTimeCodeBreakGroups()
        {
            var listOfTimeCodeBreakGroupDTOs = new List<TimeCodeBreakGroupDTO>
            {
              new TimeCodeBreakGroupDTO
              {
                TimeCodeBreakGroupId = 43,
                ActorCompanyId = 451,
                Name = "15",
                Description = null,
                Created = DateTime.ParseExact("2017-03-01T10:13:47.3570000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "BjörnS",
                Modified = null,
                ModifiedBy = null,
                State = SoeEntityState.Active,
                TimeCodeBreaks = new List<TimeCodeDTO>
                {
                  new TimeCodeDTO
                  {
                    TimeCodeId = 886,
                    ActorCompanyId = 451,
                    Type = SoeTimeCodeType.Break,
                    RegistrationType = TermGroup_TimeCodeRegistrationType.Time,
                    Code = "T15",
                    Name = "15 minuter timlön",
                    Description = "",
                    RoundingType = TermGroup_TimeCodeRoundingType.None,
                    RoundingValue = 0,
                    RoundStartTime = false,
                    MinutesByConstantRules = 0,
                    FactorBasedOnWorkPercentage = false,
                    Created = DateTime.ParseExact("2017-03-01T10:13:47.7470000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "BjörnS",
                    Modified = DateTime.ParseExact("2018-05-07T16:09:58.7070000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "SoftOne (50)",
                    State = SoeEntityState.Active,
                    KontekId = null,
                    IsAbsence = false,
                    MinMinutes = 15,
                    MaxMinutes = 15,
                    DefaultMinutes = 15,
                    StartType = 1,
                    StopType = 2,
                    StartTime = null,
                    StartTimeMinutes = 0,
                    StopTimeMinutes = 0,
                    Payed = false,
                    Template = false,
                    TimeCodeBreakGroupId = 43,
                    TimeCodeBreakGroupName = "15",
                    TimeCodeBreakEmployeeGroupNames = null,
                    Note = null,
                    IsWorkOutsideSchedule = false,
                    TimeCodeRules = new List<TimeCodeRuleDTO>
                    {
                    },
                    PayrollProducts = null,
                    PayrollProductNames = null,
                    CompanyName = null
                  },
                  new TimeCodeDTO
                  {
                    TimeCodeId = 892,
                    ActorCompanyId = 451,
                    Type = SoeTimeCodeType.Break,
                    RegistrationType = TermGroup_TimeCodeRegistrationType.Time,
                    Code = "M15",
                    Name = "15 minuter månadslön",
                    Description = "",
                    RoundingType = TermGroup_TimeCodeRoundingType.None,
                    RoundingValue = 0,
                    RoundStartTime = false,
                    MinutesByConstantRules = 0,
                    FactorBasedOnWorkPercentage = false,
                    Created = DateTime.ParseExact("2017-03-01T10:13:47.8270000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "BjörnS",
                    Modified = DateTime.ParseExact("2018-05-07T17:08:06.8630000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "SoftOne (50)",
                    State = SoeEntityState.Active,
                    KontekId = null,
                    IsAbsence = false,
                    MinMinutes = 15,
                    MaxMinutes = 15,
                    DefaultMinutes = 15,
                    StartType = 1,
                    StopType = 2,
                    StartTime = null,
                    StartTimeMinutes = 0,
                    StopTimeMinutes = 0,
                    Payed = false,
                    Template = false,
                    TimeCodeBreakGroupId = 43,
                    TimeCodeBreakGroupName = "15",
                    TimeCodeBreakEmployeeGroupNames = null,
                    Note = null,
                    IsWorkOutsideSchedule = false,
                    TimeCodeRules = new List<TimeCodeRuleDTO>
                    {
                    },
                    PayrollProducts = null,
                    PayrollProductNames = null,
                    CompanyName = null
                  }
                }
              },
              new TimeCodeBreakGroupDTO
              {
                TimeCodeBreakGroupId = 677,
                ActorCompanyId = 451,
                Name = "20",
                Description = null,
                Created = DateTime.ParseExact("2018-05-07T16:06:56.5900000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "SoftOne (50)",
                Modified = null,
                ModifiedBy = null,
                State = SoeEntityState.Active,
                TimeCodeBreaks = new List<TimeCodeDTO>
                {
                  new TimeCodeDTO
                  {
                    TimeCodeId = 911,
                    ActorCompanyId = 451,
                    Type = SoeTimeCodeType.Break,
                    RegistrationType = TermGroup_TimeCodeRegistrationType.Time,
                    Code = "T20",
                    Name = "20 minuter timlön",
                    Description = "",
                    RoundingType = TermGroup_TimeCodeRoundingType.None,
                    RoundingValue = 0,
                    RoundStartTime = false,
                    MinutesByConstantRules = 0,
                    FactorBasedOnWorkPercentage = false,
                    Created = DateTime.ParseExact("2017-03-10T09:56:48.7870000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "SoftOne (552)",
                    Modified = DateTime.ParseExact("2018-05-07T16:10:36.7870000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "SoftOne (50)",
                    State = SoeEntityState.Active,
                    KontekId = null,
                    IsAbsence = false,
                    MinMinutes = 20,
                    MaxMinutes = 20,
                    DefaultMinutes = 20,
                    StartType = 1,
                    StopType = 2,
                    StartTime = null,
                    StartTimeMinutes = 0,
                    StopTimeMinutes = 0,
                    Payed = false,
                    Template = false,
                    TimeCodeBreakGroupId = 677,
                    TimeCodeBreakGroupName = "20",
                    TimeCodeBreakEmployeeGroupNames = null,
                    Note = null,
                    IsWorkOutsideSchedule = false,
                    TimeCodeRules = new List<TimeCodeRuleDTO>
                    {
                    },
                    PayrollProducts = null,
                    PayrollProductNames = null,
                    CompanyName = null
                  }
                }
              },
              new TimeCodeBreakGroupDTO
              {
                TimeCodeBreakGroupId = 44,
                ActorCompanyId = 451,
                Name = "30",
                Description = null,
                Created = DateTime.ParseExact("2017-03-01T10:13:47.3730000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "BjörnS",
                Modified = null,
                ModifiedBy = null,
                State = SoeEntityState.Active,
                TimeCodeBreaks = new List<TimeCodeDTO>
                {
                  new TimeCodeDTO
                  {
                    TimeCodeId = 887,
                    ActorCompanyId = 451,
                    Type = SoeTimeCodeType.Break,
                    RegistrationType = TermGroup_TimeCodeRegistrationType.Time,
                    Code = "T30",
                    Name = "30 minuter timlön",
                    Description = "",
                    RoundingType = TermGroup_TimeCodeRoundingType.None,
                    RoundingValue = 0,
                    RoundStartTime = false,
                    MinutesByConstantRules = 0,
                    FactorBasedOnWorkPercentage = false,
                    Created = DateTime.ParseExact("2017-03-01T10:13:47.7630000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "BjörnS",
                    Modified = DateTime.ParseExact("2018-05-07T16:11:09.0830000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "SoftOne (50)",
                    State = SoeEntityState.Active,
                    KontekId = null,
                    IsAbsence = false,
                    MinMinutes = 30,
                    MaxMinutes = 30,
                    DefaultMinutes = 30,
                    StartType = 1,
                    StopType = 2,
                    StartTime = null,
                    StartTimeMinutes = 0,
                    StopTimeMinutes = 0,
                    Payed = false,
                    Template = false,
                    TimeCodeBreakGroupId = 44,
                    TimeCodeBreakGroupName = "30",
                    TimeCodeBreakEmployeeGroupNames = null,
                    Note = null,
                    IsWorkOutsideSchedule = false,
                    TimeCodeRules = new List<TimeCodeRuleDTO>
                    {
                    },
                    PayrollProducts = null,
                    PayrollProductNames = null,
                    CompanyName = null
                  },
                  new TimeCodeDTO
                  {
                    TimeCodeId = 893,
                    ActorCompanyId = 451,
                    Type = SoeTimeCodeType.Break,
                    RegistrationType = TermGroup_TimeCodeRegistrationType.Time,
                    Code = "M30",
                    Name = "30 minuter månadslön",
                    Description = "",
                    RoundingType = TermGroup_TimeCodeRoundingType.None,
                    RoundingValue = 0,
                    RoundStartTime = false,
                    MinutesByConstantRules = 0,
                    FactorBasedOnWorkPercentage = false,
                    Created = DateTime.ParseExact("2017-03-01T10:13:47.8430000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "BjörnS",
                    Modified = DateTime.ParseExact("2018-05-07T17:08:36.0370000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "SoftOne (50)",
                    State = SoeEntityState.Active,
                    KontekId = null,
                    IsAbsence = false,
                    MinMinutes = 30,
                    MaxMinutes = 30,
                    DefaultMinutes = 30,
                    StartType = 1,
                    StopType = 2,
                    StartTime = null,
                    StartTimeMinutes = 0,
                    StopTimeMinutes = 0,
                    Payed = false,
                    Template = false,
                    TimeCodeBreakGroupId = 44,
                    TimeCodeBreakGroupName = "30",
                    TimeCodeBreakEmployeeGroupNames = null,
                    Note = null,
                    IsWorkOutsideSchedule = false,
                    TimeCodeRules = new List<TimeCodeRuleDTO>
                    {
                    },
                    PayrollProducts = null,
                    PayrollProductNames = null,
                    CompanyName = null
                  }
                }
              },
              new TimeCodeBreakGroupDTO
              {
                TimeCodeBreakGroupId = 45,
                ActorCompanyId = 451,
                Name = "45",
                Description = null,
                Created = DateTime.ParseExact("2017-03-01T10:13:47.3900000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "BjörnS",
                Modified = null,
                ModifiedBy = null,
                State = SoeEntityState.Active,
                TimeCodeBreaks = new List<TimeCodeDTO>
                {
                  new TimeCodeDTO
                  {
                    TimeCodeId = 888,
                    ActorCompanyId = 451,
                    Type = SoeTimeCodeType.Break,
                    RegistrationType = TermGroup_TimeCodeRegistrationType.Time,
                    Code = "T45",
                    Name = "45 minuter timlön",
                    Description = "",
                    RoundingType = TermGroup_TimeCodeRoundingType.None,
                    RoundingValue = 0,
                    RoundStartTime = false,
                    MinutesByConstantRules = 0,
                    FactorBasedOnWorkPercentage = false,
                    Created = DateTime.ParseExact("2017-03-01T10:13:47.7800000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "BjörnS",
                    Modified = DateTime.ParseExact("2018-05-07T16:11:29.6470000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "SoftOne (50)",
                    State = SoeEntityState.Active,
                    KontekId = null,
                    IsAbsence = false,
                    MinMinutes = 45,
                    MaxMinutes = 45,
                    DefaultMinutes = 45,
                    StartType = 1,
                    StopType = 2,
                    StartTime = null,
                    StartTimeMinutes = 0,
                    StopTimeMinutes = 0,
                    Payed = false,
                    Template = false,
                    TimeCodeBreakGroupId = 45,
                    TimeCodeBreakGroupName = "45",
                    TimeCodeBreakEmployeeGroupNames = null,
                    Note = null,
                    IsWorkOutsideSchedule = false,
                    TimeCodeRules = new List<TimeCodeRuleDTO>
                    {
                    },
                    PayrollProducts = null,
                    PayrollProductNames = null,
                    CompanyName = null
                  },
                  new TimeCodeDTO
                  {
                    TimeCodeId = 894,
                    ActorCompanyId = 451,
                    Type = SoeTimeCodeType.Break,
                    RegistrationType = TermGroup_TimeCodeRegistrationType.Time,
                    Code = "M45",
                    Name = "45 minuter månadslön",
                    Description = "",
                    RoundingType = TermGroup_TimeCodeRoundingType.None,
                    RoundingValue = 0,
                    RoundStartTime = false,
                    MinutesByConstantRules = 0,
                    FactorBasedOnWorkPercentage = false,
                    Created = DateTime.ParseExact("2017-03-01T10:13:47.8430000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "BjörnS",
                    Modified = DateTime.ParseExact("2018-05-07T17:10:25.4600000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "SoftOne (50)",
                    State = SoeEntityState.Active,
                    KontekId = null,
                    IsAbsence = false,
                    MinMinutes = 45,
                    MaxMinutes = 45,
                    DefaultMinutes = 45,
                    StartType = 1,
                    StopType = 2,
                    StartTime = null,
                    StartTimeMinutes = 0,
                    StopTimeMinutes = 0,
                    Payed = false,
                    Template = false,
                    TimeCodeBreakGroupId = 45,
                    TimeCodeBreakGroupName = "45",
                    TimeCodeBreakEmployeeGroupNames = null,
                    Note = null,
                    IsWorkOutsideSchedule = false,
                    TimeCodeRules = new List<TimeCodeRuleDTO>
                    {
                    },
                    PayrollProducts = null,
                    PayrollProductNames = null,
                    CompanyName = null
                  }
                }
              },
              new TimeCodeBreakGroupDTO
              {
                TimeCodeBreakGroupId = 46,
                ActorCompanyId = 451,
                Name = "60",
                Description = null,
                Created = DateTime.ParseExact("2017-03-01T10:13:47.3900000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "BjörnS",
                Modified = null,
                ModifiedBy = null,
                State = SoeEntityState.Active,
                TimeCodeBreaks = new List<TimeCodeDTO>
                {
                  new TimeCodeDTO
                  {
                    TimeCodeId = 889,
                    ActorCompanyId = 451,
                    Type = SoeTimeCodeType.Break,
                    RegistrationType = TermGroup_TimeCodeRegistrationType.Time,
                    Code = "T60",
                    Name = "60 minuter timlön",
                    Description = "",
                    RoundingType = TermGroup_TimeCodeRoundingType.None,
                    RoundingValue = 0,
                    RoundStartTime = false,
                    MinutesByConstantRules = 0,
                    FactorBasedOnWorkPercentage = false,
                    Created = DateTime.ParseExact("2017-03-01T10:13:47.7970000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "BjörnS",
                    Modified = DateTime.ParseExact("2018-05-07T16:11:52.0230000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "SoftOne (50)",
                    State = SoeEntityState.Active,
                    KontekId = null,
                    IsAbsence = false,
                    MinMinutes = 60,
                    MaxMinutes = 60,
                    DefaultMinutes = 60,
                    StartType = 1,
                    StopType = 2,
                    StartTime = null,
                    StartTimeMinutes = 0,
                    StopTimeMinutes = 0,
                    Payed = false,
                    Template = false,
                    TimeCodeBreakGroupId = 46,
                    TimeCodeBreakGroupName = "60",
                    TimeCodeBreakEmployeeGroupNames = null,
                    Note = null,
                    IsWorkOutsideSchedule = false,
                    TimeCodeRules = new List<TimeCodeRuleDTO>
                    {
                    },
                    PayrollProducts = null,
                    PayrollProductNames = null,
                    CompanyName = null
                  },
                  new TimeCodeDTO
                  {
                    TimeCodeId = 895,
                    ActorCompanyId = 451,
                    Type = SoeTimeCodeType.Break,
                    RegistrationType = TermGroup_TimeCodeRegistrationType.Time,
                    Code = "M60",
                    Name = "60 minuter månadslön",
                    Description = "",
                    RoundingType = TermGroup_TimeCodeRoundingType.None,
                    RoundingValue = 0,
                    RoundStartTime = false,
                    MinutesByConstantRules = 0,
                    FactorBasedOnWorkPercentage = false,
                    Created = DateTime.ParseExact("2017-03-01T10:13:47.8570000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "BjörnS",
                    Modified = DateTime.ParseExact("2018-05-07T17:10:41.6500000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "SoftOne (50)",
                    State = SoeEntityState.Active,
                    KontekId = null,
                    IsAbsence = false,
                    MinMinutes = 60,
                    MaxMinutes = 60,
                    DefaultMinutes = 60,
                    StartType = 1,
                    StopType = 2,
                    StartTime = null,
                    StartTimeMinutes = 0,
                    StopTimeMinutes = 0,
                    Payed = false,
                    Template = false,
                    TimeCodeBreakGroupId = 46,
                    TimeCodeBreakGroupName = "60",
                    TimeCodeBreakEmployeeGroupNames = null,
                    Note = null,
                    IsWorkOutsideSchedule = false,
                    TimeCodeRules = new List<TimeCodeRuleDTO>
                    {
                    },
                    PayrollProducts = null,
                    PayrollProductNames = null,
                    CompanyName = null
                  }
                }
              },
              new TimeCodeBreakGroupDTO
              {
                TimeCodeBreakGroupId = 47,
                ActorCompanyId = 451,
                Name = "75",
                Description = null,
                Created = DateTime.ParseExact("2017-03-01T10:13:47.4030000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "BjörnS",
                Modified = null,
                ModifiedBy = null,
                State = SoeEntityState.Active,
                TimeCodeBreaks = new List<TimeCodeDTO>
                {
                  new TimeCodeDTO
                  {
                    TimeCodeId = 890,
                    ActorCompanyId = 451,
                    Type = SoeTimeCodeType.Break,
                    RegistrationType = TermGroup_TimeCodeRegistrationType.Time,
                    Code = "T75",
                    Name = "75 minuter timlön",
                    Description = "",
                    RoundingType = TermGroup_TimeCodeRoundingType.None,
                    RoundingValue = 0,
                    RoundStartTime = false,
                    MinutesByConstantRules = 0,
                    FactorBasedOnWorkPercentage = false,
                    Created = DateTime.ParseExact("2017-03-01T10:13:47.7970000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "BjörnS",
                    Modified = DateTime.ParseExact("2018-05-07T16:12:18.8030000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "SoftOne (50)",
                    State = SoeEntityState.Active,
                    KontekId = null,
                    IsAbsence = false,
                    MinMinutes = 75,
                    MaxMinutes = 75,
                    DefaultMinutes = 75,
                    StartType = 1,
                    StopType = 2,
                    StartTime = null,
                    StartTimeMinutes = 0,
                    StopTimeMinutes = 0,
                    Payed = false,
                    Template = false,
                    TimeCodeBreakGroupId = 47,
                    TimeCodeBreakGroupName = "75",
                    TimeCodeBreakEmployeeGroupNames = null,
                    Note = null,
                    IsWorkOutsideSchedule = false,
                    TimeCodeRules = new List<TimeCodeRuleDTO>
                    {
                    },
                    PayrollProducts = null,
                    PayrollProductNames = null,
                    CompanyName = null
                  },
                  new TimeCodeDTO
                  {
                    TimeCodeId = 896,
                    ActorCompanyId = 451,
                    Type = SoeTimeCodeType.Break,
                    RegistrationType = TermGroup_TimeCodeRegistrationType.Time,
                    Code = "M75",
                    Name = "75 minuter månadslön",
                    Description = "",
                    RoundingType = TermGroup_TimeCodeRoundingType.None,
                    RoundingValue = 0,
                    RoundStartTime = false,
                    MinutesByConstantRules = 0,
                    FactorBasedOnWorkPercentage = false,
                    Created = DateTime.ParseExact("2017-03-01T10:13:47.8570000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "BjörnS",
                    Modified = DateTime.ParseExact("2018-05-07T17:11:00.4130000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "SoftOne (50)",
                    State = SoeEntityState.Active,
                    KontekId = null,
                    IsAbsence = false,
                    MinMinutes = 75,
                    MaxMinutes = 75,
                    DefaultMinutes = 75,
                    StartType = 1,
                    StopType = 2,
                    StartTime = null,
                    StartTimeMinutes = 0,
                    StopTimeMinutes = 0,
                    Payed = false,
                    Template = false,
                    TimeCodeBreakGroupId = 47,
                    TimeCodeBreakGroupName = "75",
                    TimeCodeBreakEmployeeGroupNames = null,
                    Note = null,
                    IsWorkOutsideSchedule = false,
                    TimeCodeRules = new List<TimeCodeRuleDTO>
                    {
                    },
                    PayrollProducts = null,
                    PayrollProductNames = null,
                    CompanyName = null
                  }
                }
              },
              new TimeCodeBreakGroupDTO
              {
                TimeCodeBreakGroupId = 48,
                ActorCompanyId = 451,
                Name = "90",
                Description = null,
                Created = DateTime.ParseExact("2017-03-01T10:13:47.4200000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "BjörnS",
                Modified = null,
                ModifiedBy = null,
                State = SoeEntityState.Active,
                TimeCodeBreaks = new List<TimeCodeDTO>
                {
                  new TimeCodeDTO
                  {
                    TimeCodeId = 891,
                    ActorCompanyId = 451,
                    Type = SoeTimeCodeType.Break,
                    RegistrationType = TermGroup_TimeCodeRegistrationType.Time,
                    Code = "T90",
                    Name = "90 minuter timlön",
                    Description = "",
                    RoundingType = TermGroup_TimeCodeRoundingType.None,
                    RoundingValue = 0,
                    RoundStartTime = false,
                    MinutesByConstantRules = 0,
                    FactorBasedOnWorkPercentage = false,
                    Created = DateTime.ParseExact("2017-03-01T10:13:47.8100000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "BjörnS",
                    Modified = DateTime.ParseExact("2018-05-07T16:12:44.7900000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "SoftOne (50)",
                    State = SoeEntityState.Active,
                    KontekId = null,
                    IsAbsence = false,
                    MinMinutes = 90,
                    MaxMinutes = 90,
                    DefaultMinutes = 90,
                    StartType = 1,
                    StopType = 2,
                    StartTime = null,
                    StartTimeMinutes = 0,
                    StopTimeMinutes = 0,
                    Payed = false,
                    Template = false,
                    TimeCodeBreakGroupId = 48,
                    TimeCodeBreakGroupName = "90",
                    TimeCodeBreakEmployeeGroupNames = null,
                    Note = null,
                    IsWorkOutsideSchedule = false,
                    TimeCodeRules = new List<TimeCodeRuleDTO>
                    {
                    },
                    PayrollProducts = null,
                    PayrollProductNames = null,
                    CompanyName = null
                  },
                  new TimeCodeDTO
                  {
                    TimeCodeId = 897,
                    ActorCompanyId = 451,
                    Type = SoeTimeCodeType.Break,
                    RegistrationType = TermGroup_TimeCodeRegistrationType.Time,
                    Code = "M90",
                    Name = "90 minuter månadslön",
                    Description = "",
                    RoundingType = TermGroup_TimeCodeRoundingType.None,
                    RoundingValue = 0,
                    RoundStartTime = false,
                    MinutesByConstantRules = 0,
                    FactorBasedOnWorkPercentage = false,
                    Created = DateTime.ParseExact("2017-03-01T10:13:47.8730000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "BjörnS",
                    Modified = DateTime.ParseExact("2018-05-07T17:11:19.0700000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "SoftOne (50)",
                    State = SoeEntityState.Active,
                    KontekId = null,
                    IsAbsence = false,
                    MinMinutes = 90,
                    MaxMinutes = 90,
                    DefaultMinutes = 90,
                    StartType = 1,
                    StopType = 2,
                    StartTime = null,
                    StartTimeMinutes = 0,
                    StopTimeMinutes = 0,
                    Payed = false,
                    Template = false,
                    TimeCodeBreakGroupId = 48,
                    TimeCodeBreakGroupName = "90",
                    TimeCodeBreakEmployeeGroupNames = null,
                    Note = null,
                    IsWorkOutsideSchedule = false,
                    TimeCodeRules = new List<TimeCodeRuleDTO>
                    {
                    },
                    PayrollProducts = null,
                    PayrollProductNames = null,
                    CompanyName = null
                  }
                }
              }
            };
            return listOfTimeCodeBreakGroupDTOs;
        }
    }
}
