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
    public static class TimeScheduleTaskMock
    {
        public static List<TimeScheduleTaskDTO> GetTimeScheduleTasks(StaffingNeedMockScenario staffingNeedMockScenario)
        {
            switch (staffingNeedMockScenario)
            {
                case StaffingNeedMockScenario.All:
                case StaffingNeedMockScenario.FourtyHours:
                    return GetTimeScheduleTasks();
                default:
                    return GetTimeScheduleTasks();
            }
        }

        private static List<TimeScheduleTaskDTO> GetTimeScheduleTasks()
        {
            var listOfTimeScheduleTaskDTOs = new List<TimeScheduleTaskDTO>
            {
                  new TimeScheduleTaskDTO
                {
                TimeScheduleTaskId = 1,
                ShiftTypeId = 284,
                TimeScheduleTaskTypeId = null,
                Name = "Butik ny",
                Description = null,
                StartTime = DateTime.ParseExact("1900-01-01T08:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                StopTime = DateTime.ParseExact("1900-01-01T21:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                Length = 780,
                StartDate = DateTime.ParseExact("2020-03-23T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                StopDate = null,
                NbrOfOccurrences = null,
                RecurrencePattern = "1_1______",
                OnlyOneEmployee = false,
                DontAssignBreakLeftovers = false,
                AllowOverlapping = false,
                MinSplitLength = 15,
                NbrOfPersons = 1,
                IsStaffingNeedsFrequency = false,
                Created = DateTime.ParseExact("2018-05-02T13:18:33.2130000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "ICA",
                Modified = DateTime.ParseExact("2020-01-31T15:51:17.6770000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ModifiedBy = "50",
                State = SoeEntityState.Active,
                AccountId = null,
                Account2Id = null,
                Account3Id = null,
                Account4Id = null,
                Account5Id = null,
                Account6Id = null,
                RecurrencePatternDescription = null,
                RecurrenceStartsOnDescription = null,
                RecurrenceEndsOnDescription = null,
                RecurringDates = new DailyRecurrenceDatesOutput
                {
                    RecurrenceDates = new List<DateTime>
                    {
                    DateTime.ParseExact("2020-03-23T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-03-24T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-03-25T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-03-26T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-03-27T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-03-28T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-03-29T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-03-30T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-03-31T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-01T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-02T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-03T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-04T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-05T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-06T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-07T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-08T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-09T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-10T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-11T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-12T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-13T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-14T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-15T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-16T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-17T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-18T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-19T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-20T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-21T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-22T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-23T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-24T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-25T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-26T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
                    },
                    RemovedDates = new List<DateTime>
                    {
                    }
                },
                ExcludedDates = null,
                AccountName = ""
                },
                new TimeScheduleTaskDTO
                {
                TimeScheduleTaskId = 320,
                ShiftTypeId = 284,
                TimeScheduleTaskTypeId = null,
                Name = "Butik ny",
                Description = null,
                StartTime = DateTime.ParseExact("1900-01-01T08:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                StopTime = DateTime.ParseExact("1900-01-01T21:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                Length = 780,
                StartDate = DateTime.ParseExact("2020-03-23T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                StopDate = null,
                NbrOfOccurrences = null,
                RecurrencePattern = "1_1______",
                OnlyOneEmployee = false,
                DontAssignBreakLeftovers = false,
                AllowOverlapping = false,
                MinSplitLength = 15,
                NbrOfPersons = 1,
                IsStaffingNeedsFrequency = false,
                Created = DateTime.ParseExact("2018-05-02T13:18:33.2130000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "ICA",
                Modified = DateTime.ParseExact("2020-01-31T15:51:17.6770000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ModifiedBy = "50",
                State = SoeEntityState.Active,
                AccountId = null,
                Account2Id = null,
                Account3Id = null,
                Account4Id = null,
                Account5Id = null,
                Account6Id = null,
                RecurrencePatternDescription = null,
                RecurrenceStartsOnDescription = null,
                RecurrenceEndsOnDescription = null,
                RecurringDates = new DailyRecurrenceDatesOutput
                {
                    RecurrenceDates = new List<DateTime>
                    {
                    DateTime.ParseExact("2020-03-23T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-03-24T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-03-25T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-03-26T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-03-27T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-03-28T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-03-29T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-03-30T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-03-31T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-01T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-02T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-03T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-04T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-05T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-06T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-07T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-08T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-09T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-10T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-11T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-12T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-13T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-14T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-15T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-16T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-17T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-18T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-19T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-20T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-21T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-22T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-23T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-24T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-25T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-26T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
                    },
                    RemovedDates = new List<DateTime>
                    {
                    }
                },
                ExcludedDates = null,
                AccountName = ""
                },
                new TimeScheduleTaskDTO
                {
                TimeScheduleTaskId = 322,
                ShiftTypeId = 4230,
                TimeScheduleTaskTypeId = null,
                Name = "Butik ny",
                Description = null,
                StartTime = DateTime.ParseExact("1900-01-01T08:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                StopTime = DateTime.ParseExact("1900-01-01T21:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                Length = 780,
                StartDate = DateTime.ParseExact("2020-03-23T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                StopDate = null,
                NbrOfOccurrences = null,
                RecurrencePattern = "1_1______",
                OnlyOneEmployee = false,
                DontAssignBreakLeftovers = false,
                AllowOverlapping = false,
                MinSplitLength = 15,
                NbrOfPersons = 1,
                IsStaffingNeedsFrequency = false,
                Created = DateTime.ParseExact("2018-05-02T13:23:28.3070000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "ICA",
                Modified = DateTime.ParseExact("2021-03-08T10:48:47.2330000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ModifiedBy = "SoftOne (4624)",
                State = SoeEntityState.Active,
                AccountId = null,
                Account2Id = null,
                Account3Id = null,
                Account4Id = null,
                Account5Id = null,
                Account6Id = null,
                RecurrencePatternDescription = null,
                RecurrenceStartsOnDescription = null,
                RecurrenceEndsOnDescription = null,
                RecurringDates = new DailyRecurrenceDatesOutput
                {
                    RecurrenceDates = new List<DateTime>
                    {
                    DateTime.ParseExact("2020-03-23T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-03-24T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-03-25T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-03-26T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-03-27T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-03-28T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-03-29T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-03-30T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-03-31T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-01T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-02T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-03T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-04T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-05T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-06T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-07T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-08T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-09T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-10T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-11T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-12T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-13T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-14T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-15T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-16T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-17T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-18T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-19T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-20T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-21T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-22T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-23T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-24T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-25T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-26T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
                    },
                    RemovedDates = new List<DateTime>
                    {
                    }
                },
                ExcludedDates = null,
                AccountName = ""
                },
                new TimeScheduleTaskDTO
                {
                TimeScheduleTaskId = 324,
                ShiftTypeId = 284,
                TimeScheduleTaskTypeId = null,
                Name = "Butik ny",
                Description = null,
                StartTime = DateTime.ParseExact("1900-01-01T09:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                StopTime = DateTime.ParseExact("1900-01-01T15:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                Length = 240,
                StartDate = DateTime.ParseExact("2020-03-23T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                StopDate = null,
                NbrOfOccurrences = null,
                RecurrencePattern = "2_1_2__2___",
                OnlyOneEmployee = false,
                DontAssignBreakLeftovers = false,
                AllowOverlapping = false,
                MinSplitLength = 15,
                NbrOfPersons = 1,
                IsStaffingNeedsFrequency = false,
                Created = DateTime.ParseExact("2018-05-02T13:28:04.3570000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "ICA",
                Modified = DateTime.ParseExact("2020-01-31T13:15:42.0530000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ModifiedBy = "50",
                State = SoeEntityState.Active,
                AccountId = null,
                Account2Id = null,
                Account3Id = null,
                Account4Id = null,
                Account5Id = null,
                Account6Id = null,
                RecurrencePatternDescription = null,
                RecurrenceStartsOnDescription = null,
                RecurrenceEndsOnDescription = null,
                RecurringDates = new DailyRecurrenceDatesOutput
                {
                    RecurrenceDates = new List<DateTime>
                    {
                    DateTime.ParseExact("2020-03-24T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-03-31T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-07T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-14T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
                    },
                    RemovedDates = new List<DateTime>
                    {
                    }
                },
                ExcludedDates = null,
                AccountName = ""
                },
                new TimeScheduleTaskDTO
                {
                TimeScheduleTaskId = 325,
                ShiftTypeId = 284,
                TimeScheduleTaskTypeId = null,
                Name = "Butik ny",
                Description = null,
                StartTime = DateTime.ParseExact("1900-01-01T09:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                StopTime = DateTime.ParseExact("1900-01-01T13:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                Length = 180,
                StartDate = DateTime.ParseExact("2020-03-23T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                StopDate = null,
                NbrOfOccurrences = null,
                RecurrencePattern = "2_1_2__4___",
                OnlyOneEmployee = false,
                DontAssignBreakLeftovers = false,
                AllowOverlapping = false,
                MinSplitLength = 15,
                NbrOfPersons = 1,
                IsStaffingNeedsFrequency = false,
                Created = DateTime.ParseExact("2018-05-02T13:29:59.4100000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "ICA",
                Modified = DateTime.ParseExact("2020-01-31T13:16:34.6830000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ModifiedBy = "50",
                State = SoeEntityState.Active,
                AccountId = null,
                Account2Id = null,
                Account3Id = null,
                Account4Id = null,
                Account5Id = null,
                Account6Id = null,
                RecurrencePatternDescription = null,
                RecurrenceStartsOnDescription = null,
                RecurrenceEndsOnDescription = null,
                RecurringDates = new DailyRecurrenceDatesOutput
                {
                    RecurrenceDates = new List<DateTime>
                    {
                    DateTime.ParseExact("2020-03-26T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-02T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-09T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-16T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
                    },
                    RemovedDates = new List<DateTime>
                    {
                    }
                },
                ExcludedDates = null,
                AccountName = ""
                },
                new TimeScheduleTaskDTO
                {
                TimeScheduleTaskId = 327,
                ShiftTypeId = 284,
                TimeScheduleTaskTypeId = null,
                Name = "Butik ny",
                Description = null,
                StartTime = DateTime.ParseExact("1900-01-01T12:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                StopTime = DateTime.ParseExact("1900-01-01T18:15:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                Length = 315,
                StartDate = DateTime.ParseExact("2020-03-23T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                StopDate = null,
                NbrOfOccurrences = null,
                RecurrencePattern = "2_1_2__5___",
                OnlyOneEmployee = false,
                DontAssignBreakLeftovers = false,
                AllowOverlapping = false,
                MinSplitLength = 15,
                NbrOfPersons = 1,
                IsStaffingNeedsFrequency = false,
                Created = DateTime.ParseExact("2018-05-02T13:33:07.9670000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "ICA",
                Modified = DateTime.ParseExact("2020-02-11T11:47:24.9770000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ModifiedBy = "ICA",
                State = SoeEntityState.Active,
                AccountId = null,
                Account2Id = null,
                Account3Id = null,
                Account4Id = null,
                Account5Id = null,
                Account6Id = null,
                RecurrencePatternDescription = null,
                RecurrenceStartsOnDescription = null,
                RecurrenceEndsOnDescription = null,
                RecurringDates = new DailyRecurrenceDatesOutput
                {
                    RecurrenceDates = new List<DateTime>
                    {
                    DateTime.ParseExact("2020-03-27T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-03T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-10T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-17T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
                    },
                    RemovedDates = new List<DateTime>
                    {
                    }
                },
                ExcludedDates = null,
                AccountName = ""
                },
                new TimeScheduleTaskDTO
                {
                TimeScheduleTaskId = 328,
                ShiftTypeId = 284,
                TimeScheduleTaskTypeId = null,
                Name = "Butik ny",
                Description = null,
                StartTime = DateTime.ParseExact("1900-01-01T08:06:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                StopTime = DateTime.ParseExact("1900-01-01T12:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                Length = 234,
                StartDate = DateTime.ParseExact("2020-03-23T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                StopDate = null,
                NbrOfOccurrences = null,
                RecurrencePattern = "2_1_2__6___",
                OnlyOneEmployee = false,
                DontAssignBreakLeftovers = false,
                AllowOverlapping = false,
                MinSplitLength = 15,
                NbrOfPersons = 1,
                IsStaffingNeedsFrequency = false,
                Created = DateTime.ParseExact("2018-05-02T13:34:51.5670000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "ICA",
                Modified = DateTime.ParseExact("2020-01-31T13:29:46.0170000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ModifiedBy = "50",
                State = SoeEntityState.Active,
                AccountId = null,
                Account2Id = null,
                Account3Id = null,
                Account4Id = null,
                Account5Id = null,
                Account6Id = null,
                RecurrencePatternDescription = null,
                RecurrenceStartsOnDescription = null,
                RecurrenceEndsOnDescription = null,
                RecurringDates = new DailyRecurrenceDatesOutput
                {
                    RecurrenceDates = new List<DateTime>
                    {
                    DateTime.ParseExact("2020-03-28T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-04T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-11T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    DateTime.ParseExact("2020-04-18T00:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
                    },
                    RemovedDates = new List<DateTime>
                    {
                    }
                },
                ExcludedDates = null,
                AccountName = ""
                }
            };

            return listOfTimeScheduleTaskDTOs;
        }
    }
}
