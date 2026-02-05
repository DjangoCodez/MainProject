using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Soe.Business.Tests.Business.Util.StaffingNeedsCalculation.Mock
{
    public static class ScheduleCycleMock
    {
        public static List<ScheduleCycleRuleDTO> GetScheduleCycleRules(StaffingNeedMockScenario staffingNeedMockScenario)
        {
            switch (staffingNeedMockScenario)
            {
                case StaffingNeedMockScenario.All:
                case StaffingNeedMockScenario.FourtyHours:
                    return GetScheduleCycleRules();
                default:
                    return GetScheduleCycleRules();
            }
        }
        private static List<ScheduleCycleRuleDTO> GetScheduleCycleRules()
        {
            var listOfScheduleCycleRuleDTOs = new List<ScheduleCycleRuleDTO>
            {
              new ScheduleCycleRuleDTO
              {
                ScheduleCycleRuleId = 177,
                ScheduleCycleId = 35,
                ScheduleCycleRuleTypeId = 62,
                MinOccurrences = 12,
                MaxOccurrences = 16,
                Created = DateTime.ParseExact("2020-01-31T13:34:49.2570000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
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
                  Created = DateTime.ParseExact("2018-04-16T16:03:19.6730000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  CreatedBy = "ICA",
                  Modified = DateTime.ParseExact("2020-01-31T13:30:19.1900000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  ModifiedBy = "50",
                  State = SoeEntityState.Active,
                }
              },
              new ScheduleCycleRuleDTO
              {
                ScheduleCycleRuleId = 178,
                ScheduleCycleId = 35,
                ScheduleCycleRuleTypeId = 63,
                MinOccurrences = 4,
                MaxOccurrences = 4,
                Created = DateTime.ParseExact("2020-01-31T13:34:49.2570000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
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
                  Created = DateTime.ParseExact("2018-04-24T09:48:25.4930000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  CreatedBy = "70",
                  Modified = DateTime.ParseExact("2020-01-31T15:46:13.2030000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  ModifiedBy = "50",
                  State = SoeEntityState.Active,
                }
              },
              new ScheduleCycleRuleDTO
              {
                ScheduleCycleRuleId = 179,
                ScheduleCycleId = 35,
                ScheduleCycleRuleTypeId = 66,
                MinOccurrences = 1,
                MaxOccurrences = 1,
                Created = DateTime.ParseExact("2020-01-31T13:34:49.2570000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
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
                  Created = DateTime.ParseExact("2018-05-14T14:00:11.8870000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  CreatedBy = "ICA",
                  Modified = DateTime.ParseExact("2020-01-31T13:31:33.6970000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  ModifiedBy = "50",
                  State = SoeEntityState.Active,
                }
              },
              new ScheduleCycleRuleDTO
              {
                ScheduleCycleRuleId = 180,
                ScheduleCycleId = 35,
                ScheduleCycleRuleTypeId = 101,
                MinOccurrences = 1,
                MaxOccurrences = 1,
                Created = DateTime.ParseExact("2020-01-31T13:34:49.2570000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
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
                  Created = DateTime.ParseExact("2020-01-31T13:32:07.5570000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  CreatedBy = "50",
                  Modified = null,
                  ModifiedBy = null,
                  State = SoeEntityState.Active,
                }
              },
              new ScheduleCycleRuleDTO
              {
                ScheduleCycleRuleId = 181,
                ScheduleCycleId = 35,
                ScheduleCycleRuleTypeId = 102,
                MinOccurrences = 1,
                MaxOccurrences = 1,
                Created = DateTime.ParseExact("2020-01-31T13:34:49.2570000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
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
                  State = SoeEntityState.Active,
                }
              },
              new ScheduleCycleRuleDTO
              {
                ScheduleCycleRuleId = 182,
                ScheduleCycleId = 35,
                ScheduleCycleRuleTypeId = 103,
                MinOccurrences = 1,
                MaxOccurrences = 1,
                Created = DateTime.ParseExact("2020-01-31T13:34:49.2570000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
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
                  State = SoeEntityState.Active,
                }
              },
              new ScheduleCycleRuleDTO
              {
                ScheduleCycleRuleId = 94,
                ScheduleCycleId = 37,
                ScheduleCycleRuleTypeId = 62,
                MinOccurrences = 14,
                MaxOccurrences = 14,
                Created = DateTime.ParseExact("2018-04-25T15:57:13.0900000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "70",
                Modified = DateTime.ParseExact("2020-01-31T13:36:13.7770000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
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
                  Created = DateTime.ParseExact("2018-04-16T16:03:19.6730000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  CreatedBy = "ICA",
                  Modified = DateTime.ParseExact("2020-01-31T13:30:19.1900000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  ModifiedBy = "50",
                  State = SoeEntityState.Active,
                }
              },
              new ScheduleCycleRuleDTO
              {
                ScheduleCycleRuleId = 95,
                ScheduleCycleId = 37,
                ScheduleCycleRuleTypeId = 63,
                MinOccurrences = 4,
                MaxOccurrences = 4,
                Created = DateTime.ParseExact("2018-04-25T15:57:26.3870000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "70",
                Modified = DateTime.ParseExact("2020-01-31T13:36:13.7770000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
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
                  Created = DateTime.ParseExact("2018-04-24T09:48:25.4930000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  CreatedBy = "70",
                  Modified = DateTime.ParseExact("2020-01-31T15:46:13.2030000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  ModifiedBy = "50",
                  State = SoeEntityState.Active,
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
                Modified = DateTime.ParseExact("2020-01-31T13:36:13.7770000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
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
                  Created = DateTime.ParseExact("2018-05-14T14:00:11.8870000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  CreatedBy = "ICA",
                  Modified = DateTime.ParseExact("2020-01-31T13:31:33.6970000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  ModifiedBy = "50",
                  State = SoeEntityState.Active,
                }
              },
              new ScheduleCycleRuleDTO
              {
                ScheduleCycleRuleId = 183,
                ScheduleCycleId = 75,
                ScheduleCycleRuleTypeId = 104,
                MinOccurrences = 1,
                MaxOccurrences = 4,
                Created = DateTime.ParseExact("2020-01-31T13:40:41.4330000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
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
                  Created = DateTime.ParseExact("2020-01-31T13:38:11.7830000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  CreatedBy = "50",
                  Modified = null,
                  ModifiedBy = null,
                  State = SoeEntityState.Active,
                }
              },
              new ScheduleCycleRuleDTO
              {
                ScheduleCycleRuleId = 185,
                ScheduleCycleId = 76,
                ScheduleCycleRuleTypeId = 63,
                MinOccurrences = 0,
                MaxOccurrences = 4,
                Created = DateTime.ParseExact("2020-01-31T13:44:40.8270000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
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
                  Created = DateTime.ParseExact("2018-04-24T09:48:25.4930000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  CreatedBy = "70",
                  Modified = DateTime.ParseExact("2020-01-31T15:46:13.2030000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  ModifiedBy = "50",
                  State = SoeEntityState.Active,
                }
              },
              new ScheduleCycleRuleDTO
              {
                ScheduleCycleRuleId = 186,
                ScheduleCycleId = 76,
                ScheduleCycleRuleTypeId = 104,
                MinOccurrences = 1,
                MaxOccurrences = 4,
                Created = DateTime.ParseExact("2020-01-31T13:44:40.8270000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
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
                  Created = DateTime.ParseExact("2020-01-31T13:38:11.7830000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  CreatedBy = "50",
                  Modified = null,
                  ModifiedBy = null,
                  State = SoeEntityState.Active,
                }
              },
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
                  Created = DateTime.ParseExact("2020-01-31T13:46:29.6930000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  CreatedBy = "50",
                  Modified = null,
                  ModifiedBy = null,
                  State = SoeEntityState.Active,
                }
              },
              new ScheduleCycleRuleDTO
              {
                ScheduleCycleRuleId = 188,
                ScheduleCycleId = 78,
                ScheduleCycleRuleTypeId = 106,
                MinOccurrences = 2,
                MaxOccurrences = 4,
                Created = DateTime.ParseExact("2020-01-31T13:49:43.1600000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "50",
                Modified = DateTime.ParseExact("2020-01-31T13:49:55.9870000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
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
                  Created = DateTime.ParseExact("2020-01-31T13:49:07.3770000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  CreatedBy = "50",
                  Modified = null,
                  ModifiedBy = null,
                  State = SoeEntityState.Active,
                }
              },
              new ScheduleCycleRuleDTO
              {
                ScheduleCycleRuleId = 189,
                ScheduleCycleId = 78,
                ScheduleCycleRuleTypeId = 104,
                MinOccurrences = 2,
                MaxOccurrences = 4,
                Created = DateTime.ParseExact("2020-01-31T13:49:55.9870000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
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
                  Created = DateTime.ParseExact("2020-01-31T13:38:11.7830000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                  CreatedBy = "50",
                  Modified = null,
                  ModifiedBy = null,
                  State = SoeEntityState.Active,
                }
              }
            };

            return listOfScheduleCycleRuleDTOs;
        }
    }
}

