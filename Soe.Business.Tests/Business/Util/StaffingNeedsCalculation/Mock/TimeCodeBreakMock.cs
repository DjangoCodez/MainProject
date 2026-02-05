using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Globalization;

namespace Soe.Business.Tests.Business.Util.StaffingNeedsCalculation.Mock
{
    public static class TimeCodeBreakMock
    {
        public static List<TimeCodeBreakDTO> GetTimeCodeBreaks(StaffingNeedMockScenario staffingNeedMockScenario)
        {
            switch (staffingNeedMockScenario)
            {
                case StaffingNeedMockScenario.All:
                case StaffingNeedMockScenario.FourtyHours:
                    return GetTimeCodeBreaks();
                default:
                    return GetTimeCodeBreaks();
            }
        }
        private static List<TimeCodeBreakDTO> GetTimeCodeBreaks()
        {
            var listOfTimeCodeBreakDTOs = new List<TimeCodeBreakDTO>
            {
              new TimeCodeBreakDTO
              {
                MinMinutes = 15,
                MaxMinutes = 15,
                DefaultMinutes = 15,
                StartType = 1,
                StopType = 2,
                StartTime = null,
                StartTimeMinutes = 0,
                StopTimeMinutes = 0,
                Template = false,
                TimeCodeBreakGroupId = 43,
                TimeCodeRules = null,
                TimeCodeDeviationCauses = null,
                EmployeeGroupIds = new List<int>
                {
                  79
                },
                TimeCodeId = 892,
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
                Payed = false,
                Created = DateTime.ParseExact("2017-03-01T10:13:47.8270000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "BjörnS",
                Modified = DateTime.ParseExact("2018-05-07T17:08:06.8630000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ModifiedBy = "SoftOne (50)",
                State = SoeEntityState.Active,
                InvoiceProducts = null,
                PayrollProducts = null
              },
              new TimeCodeBreakDTO
              {
                MinMinutes = 30,
                MaxMinutes = 30,
                DefaultMinutes = 30,
                StartType = 1,
                StopType = 2,
                StartTime = null,
                StartTimeMinutes = 0,
                StopTimeMinutes = 0,
                Template = false,
                TimeCodeBreakGroupId = 44,
                TimeCodeRules = null,
                TimeCodeDeviationCauses = null,
                EmployeeGroupIds = new List<int>
                {
                  79
                },
                TimeCodeId = 893,
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
                Payed = false,
                Created = DateTime.ParseExact("2017-03-01T10:13:47.8430000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "BjörnS",
                Modified = DateTime.ParseExact("2018-05-07T17:08:36.0370000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ModifiedBy = "SoftOne (50)",
                State = SoeEntityState.Active,
                InvoiceProducts = null,
                PayrollProducts = null
              },
              new TimeCodeBreakDTO
              {
                MinMinutes = 45,
                MaxMinutes = 45,
                DefaultMinutes = 45,
                StartType = 1,
                StopType = 2,
                StartTime = null,
                StartTimeMinutes = 0,
                StopTimeMinutes = 0,
                Template = false,
                TimeCodeBreakGroupId = 45,
                TimeCodeRules = null,
                TimeCodeDeviationCauses = null,
                EmployeeGroupIds = new List<int>
                {
                  79
                },
                TimeCodeId = 894,
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
                Payed = false,
                Created = DateTime.ParseExact("2017-03-01T10:13:47.8430000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "BjörnS",
                Modified = DateTime.ParseExact("2018-05-07T17:10:25.4600000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ModifiedBy = "SoftOne (50)",
                State = SoeEntityState.Active,
                InvoiceProducts = null,
                PayrollProducts = null
              },
              new TimeCodeBreakDTO
              {
                MinMinutes = 60,
                MaxMinutes = 60,
                DefaultMinutes = 60,
                StartType = 1,
                StopType = 2,
                StartTime = null,
                StartTimeMinutes = 0,
                StopTimeMinutes = 0,
                Template = false,
                TimeCodeBreakGroupId = 46,
                TimeCodeRules = null,
                TimeCodeDeviationCauses = null,
                EmployeeGroupIds = new List<int>
                {
                  79
                },
                TimeCodeId = 895,
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
                Payed = false,
                Created = DateTime.ParseExact("2017-03-01T10:13:47.8570000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "BjörnS",
                Modified = DateTime.ParseExact("2018-05-07T17:10:41.6500000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ModifiedBy = "SoftOne (50)",
                State = SoeEntityState.Active,
                InvoiceProducts = null,
                PayrollProducts = null
              },
              new TimeCodeBreakDTO
              {
                MinMinutes = 75,
                MaxMinutes = 75,
                DefaultMinutes = 75,
                StartType = 1,
                StopType = 2,
                StartTime = null,
                StartTimeMinutes = 0,
                StopTimeMinutes = 0,
                Template = false,
                TimeCodeBreakGroupId = 47,
                TimeCodeRules = null,
                TimeCodeDeviationCauses = null,
                EmployeeGroupIds = new List<int>
                {
                  79
                },
                TimeCodeId = 896,
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
                Payed = false,
                Created = DateTime.ParseExact("2017-03-01T10:13:47.8570000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "BjörnS",
                Modified = DateTime.ParseExact("2018-05-07T17:11:00.4130000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ModifiedBy = "SoftOne (50)",
                State = SoeEntityState.Active,
                InvoiceProducts = null,
                PayrollProducts = null
              },
              new TimeCodeBreakDTO
              {
                MinMinutes = 90,
                MaxMinutes = 90,
                DefaultMinutes = 90,
                StartType = 1,
                StopType = 2,
                StartTime = null,
                StartTimeMinutes = 0,
                StopTimeMinutes = 0,
                Template = false,
                TimeCodeBreakGroupId = 48,
                TimeCodeRules = null,
                TimeCodeDeviationCauses = null,
                EmployeeGroupIds = new List<int>
                {
                  79
                },
                TimeCodeId = 897,
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
                Payed = false,
                Created = DateTime.ParseExact("2017-03-01T10:13:47.8730000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "BjörnS",
                Modified = DateTime.ParseExact("2018-05-07T17:11:19.0700000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ModifiedBy = "SoftOne (50)",
                State = SoeEntityState.Active,
                InvoiceProducts = null,
                PayrollProducts = null
              },
              new TimeCodeBreakDTO
              {
                MinMinutes = 15,
                MaxMinutes = 15,
                DefaultMinutes = 15,
                StartType = 1,
                StopType = 2,
                StartTime = null,
                StartTimeMinutes = 0,
                StopTimeMinutes = 0,
                Template = false,
                TimeCodeBreakGroupId = 43,
                TimeCodeRules = null,
                TimeCodeDeviationCauses = null,
                EmployeeGroupIds = new List<int>
                {
                  80,
                  81
                },
                TimeCodeId = 886,
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
                Payed = false,
                Created = DateTime.ParseExact("2017-03-01T10:13:47.7470000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "BjörnS",
                Modified = DateTime.ParseExact("2018-05-07T16:09:58.7070000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ModifiedBy = "SoftOne (50)",
                State = SoeEntityState.Active,
                InvoiceProducts = null,
                PayrollProducts = null
              },
              new TimeCodeBreakDTO
              {
                MinMinutes = 20,
                MaxMinutes = 20,
                DefaultMinutes = 20,
                StartType = 1,
                StopType = 2,
                StartTime = null,
                StartTimeMinutes = 0,
                StopTimeMinutes = 0,
                Template = false,
                TimeCodeBreakGroupId = 677,
                TimeCodeRules = null,
                TimeCodeDeviationCauses = null,
                EmployeeGroupIds = new List<int>
                {
                  80,
                  81
                },
                TimeCodeId = 911,
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
                Payed = false,
                Created = DateTime.ParseExact("2017-03-10T09:56:48.7870000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "SoftOne (552)",
                Modified = DateTime.ParseExact("2018-05-07T16:10:36.7870000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ModifiedBy = "SoftOne (50)",
                State = SoeEntityState.Active,
                InvoiceProducts = null,
                PayrollProducts = null
              },
              new TimeCodeBreakDTO
              {
                MinMinutes = 30,
                MaxMinutes = 30,
                DefaultMinutes = 30,
                StartType = 1,
                StopType = 2,
                StartTime = null,
                StartTimeMinutes = 0,
                StopTimeMinutes = 0,
                Template = false,
                TimeCodeBreakGroupId = 44,
                TimeCodeRules = null,
                TimeCodeDeviationCauses = null,
                EmployeeGroupIds = new List<int>
                {
                  80,
                  81
                },
                TimeCodeId = 887,
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
                Payed = false,
                Created = DateTime.ParseExact("2017-03-01T10:13:47.7630000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "BjörnS",
                Modified = DateTime.ParseExact("2018-05-07T16:11:09.0830000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ModifiedBy = "SoftOne (50)",
                State = SoeEntityState.Active,
                InvoiceProducts = null,
                PayrollProducts = null
              },
              new TimeCodeBreakDTO
              {
                MinMinutes = 45,
                MaxMinutes = 45,
                DefaultMinutes = 45,
                StartType = 1,
                StopType = 2,
                StartTime = null,
                StartTimeMinutes = 0,
                StopTimeMinutes = 0,
                Template = false,
                TimeCodeBreakGroupId = 45,
                TimeCodeRules = null,
                TimeCodeDeviationCauses = null,
                EmployeeGroupIds = new List<int>
                {
                  80,
                  81
                },
                TimeCodeId = 888,
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
                Payed = false,
                Created = DateTime.ParseExact("2017-03-01T10:13:47.7800000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "BjörnS",
                Modified = DateTime.ParseExact("2018-05-07T16:11:29.6470000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ModifiedBy = "SoftOne (50)",
                State = SoeEntityState.Active,
                InvoiceProducts = null,
                PayrollProducts = null
              },
              new TimeCodeBreakDTO
              {
                MinMinutes = 60,
                MaxMinutes = 60,
                DefaultMinutes = 60,
                StartType = 1,
                StopType = 2,
                StartTime = null,
                StartTimeMinutes = 0,
                StopTimeMinutes = 0,
                Template = false,
                TimeCodeBreakGroupId = 46,
                TimeCodeRules = null,
                TimeCodeDeviationCauses = null,
                EmployeeGroupIds = new List<int>
                {
                  80,
                  81
                },
                TimeCodeId = 889,
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
                Payed = false,
                Created = DateTime.ParseExact("2017-03-01T10:13:47.7970000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "BjörnS",
                Modified = DateTime.ParseExact("2018-05-07T16:11:52.0230000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ModifiedBy = "SoftOne (50)",
                State = SoeEntityState.Active,
                InvoiceProducts = null,
                PayrollProducts = null
              },
              new TimeCodeBreakDTO
              {
                MinMinutes = 75,
                MaxMinutes = 75,
                DefaultMinutes = 75,
                StartType = 1,
                StopType = 2,
                StartTime = null,
                StartTimeMinutes = 0,
                StopTimeMinutes = 0,
                Template = false,
                TimeCodeBreakGroupId = 47,
                TimeCodeRules = null,
                TimeCodeDeviationCauses = null,
                EmployeeGroupIds = new List<int>
                {
                  80,
                  81
                },
                TimeCodeId = 890,
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
                Payed = false,
                Created = DateTime.ParseExact("2017-03-01T10:13:47.7970000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "BjörnS",
                Modified = DateTime.ParseExact("2018-05-07T16:12:18.8030000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ModifiedBy = "SoftOne (50)",
                State = SoeEntityState.Active,
                InvoiceProducts = null,
                PayrollProducts = null
              },
              new TimeCodeBreakDTO
              {
                MinMinutes = 90,
                MaxMinutes = 90,
                DefaultMinutes = 90,
                StartType = 1,
                StopType = 2,
                StartTime = null,
                StartTimeMinutes = 0,
                StopTimeMinutes = 0,
                Template = false,
                TimeCodeBreakGroupId = 48,
                TimeCodeRules = null,
                TimeCodeDeviationCauses = null,
                EmployeeGroupIds = new List<int>
                {
                  80,
                  81
                },
                TimeCodeId = 891,
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
                Payed = false,
                Created = DateTime.ParseExact("2017-03-01T10:13:47.8100000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "BjörnS",
                Modified = DateTime.ParseExact("2018-05-07T16:12:44.7900000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ModifiedBy = "SoftOne (50)",
                State = SoeEntityState.Active,
                InvoiceProducts = null,
                PayrollProducts = null
              }
            };
            return listOfTimeCodeBreakDTOs;
        }
    }
}
