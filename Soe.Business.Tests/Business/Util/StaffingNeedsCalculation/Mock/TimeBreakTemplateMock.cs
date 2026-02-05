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
    public static class TimeBreakTemplateMock
    {
        public static List<TimeBreakTemplateDTO> GetTimeBreakTemplates(StaffingNeedMockScenario staffingNeedMockScenario)
        {
            switch (staffingNeedMockScenario)
            {
                case StaffingNeedMockScenario.All:
                case StaffingNeedMockScenario.FourtyHours:
                    return GetTimeBreakTemplates();
                default:
                    return GetTimeBreakTemplates();
            }
        }
        public static List<TimeBreakTemplateDTO> GetTimeBreakTemplates()
        {
            var listOfTimeBreakTemplateDTOs = new List<TimeBreakTemplateDTO>
            {
              new TimeBreakTemplateDTO
              {
                TimeBreakTemplateId = 40,
                ActorCompanyId = 451,
                ShiftTypeIds = null,
                DayTypeIds = null,
                DayOfWeeks = null,
                ShiftStartFromTime = DateTime.ParseExact("1900-01-01T05:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ShiftLength = 480,
                UseMaxWorkTimeBetweenBreaks = false,
                MinTimeBetweenBreaks = 60,
                StartDate = null,
                StopDate = null,
                Created = DateTime.ParseExact("2018-04-16T16:00:28.7230000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "ICA",
                Modified = DateTime.ParseExact("2020-01-31T13:24:31.9800000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ModifiedBy = "50",
                State = SoeEntityState.Active,
                TimeBreakTemplateRows = new List<TimeBreakTemplateRowDTO>
                {
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 348,
                    TimeBreakTemplateId = 40,
                    TimeCodeBreakGroupId = 46,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 180,
                    MinTimeBeforeEnd = 180,
                    Created = DateTime.ParseExact("2018-04-16T16:00:28.7233333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "ICA",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:28.1833333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("9a157810-d173-4e57-9d5c-dc24d098b9c4"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 349,
                    TimeBreakTemplateId = 40,
                    TimeCodeBreakGroupId = 46,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 120,
                    MinTimeBeforeEnd = 120,
                    Created = DateTime.ParseExact("2018-04-24T10:01:20.7666667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "70",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:28.1833333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("a6005859-49f1-40a6-ace0-479ad8843860"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 350,
                    TimeBreakTemplateId = 40,
                    TimeCodeBreakGroupId = 43,
                    Type = SoeTimeBreakTemplateType.Minor,
                    MinTimeAfterStart = 120,
                    MinTimeBeforeEnd = 120,
                    Created = DateTime.ParseExact("2018-04-24T10:01:20.7833333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "70",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:28.1833333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("8a24c6c4-abfe-43b3-b78c-1daeb6817d76"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 351,
                    TimeBreakTemplateId = 40,
                    TimeCodeBreakGroupId = 45,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 120,
                    MinTimeBeforeEnd = 120,
                    Created = DateTime.ParseExact("2018-04-24T10:02:34.2766667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "70",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:28.1833333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("7ff15c52-4d0b-4ad9-b963-07a3a8e2a2e0"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 352,
                    TimeBreakTemplateId = 40,
                    TimeCodeBreakGroupId = 43,
                    Type = SoeTimeBreakTemplateType.Minor,
                    MinTimeAfterStart = 120,
                    MinTimeBeforeEnd = 120,
                    Created = DateTime.ParseExact("2018-04-24T10:02:34.2766667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "70",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:28.1833333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("1dad81fd-035b-4da5-8569-f9a4aaae7801"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 353,
                    TimeBreakTemplateId = 40,
                    TimeCodeBreakGroupId = 45,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 120,
                    MinTimeBeforeEnd = 120,
                    Created = DateTime.ParseExact("2018-04-24T10:06:53.4766667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "70",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:28.1833333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("dbf4c0d1-743b-4a1c-b627-6a45d6a7b254"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 354,
                    TimeBreakTemplateId = 40,
                    TimeCodeBreakGroupId = 43,
                    Type = SoeTimeBreakTemplateType.Minor,
                    MinTimeAfterStart = 120,
                    MinTimeBeforeEnd = 120,
                    Created = DateTime.ParseExact("2018-04-24T10:06:53.4766667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "70",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:28.1833333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("6661f1df-2027-4235-99de-fcf0023b049a"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 531,
                    TimeBreakTemplateId = 40,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 120,
                    MinTimeBeforeEnd = 240,
                    Created = DateTime.ParseExact("2018-05-07T07:32:25.6633333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "ICA",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:28.1833333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("9a4f276f-ede8-464e-93bc-40da868f4ec3"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 532,
                    TimeBreakTemplateId = 40,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Minor,
                    MinTimeAfterStart = 240,
                    MinTimeBeforeEnd = 60,
                    Created = DateTime.ParseExact("2018-05-07T07:32:25.6633333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "ICA",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:28.1833333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("b863603f-8ea1-4771-8be1-d17a6d70d3e0"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 534,
                    TimeBreakTemplateId = 40,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 120,
                    MinTimeBeforeEnd = 240,
                    Created = DateTime.ParseExact("2018-05-07T07:33:27.7300000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "ICA",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:28.1833333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("b6f24a9f-1ce5-4c56-a569-be2cd919296e"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 535,
                    TimeBreakTemplateId = 40,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Minor,
                    MinTimeAfterStart = 240,
                    MinTimeBeforeEnd = 60,
                    Created = DateTime.ParseExact("2018-05-07T07:33:27.7300000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "ICA",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:28.1833333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("bce9a0cc-faba-4f28-9729-50ef1f183184"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 537,
                    TimeBreakTemplateId = 40,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 120,
                    MinTimeBeforeEnd = 240,
                    Created = DateTime.ParseExact("2018-05-07T07:35:37.7700000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "ICA",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:28.1833333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("1ea73684-726c-46ec-89f7-d16e094b660c"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 538,
                    TimeBreakTemplateId = 40,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Minor,
                    MinTimeAfterStart = 240,
                    MinTimeBeforeEnd = 60,
                    Created = DateTime.ParseExact("2018-05-07T07:35:37.7700000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "ICA",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:28.1833333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("f0fb140c-0893-4286-94af-d4f18515edbb"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 541,
                    TimeBreakTemplateId = 40,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 120,
                    MinTimeBeforeEnd = 240,
                    Created = DateTime.ParseExact("2018-05-07T07:36:36.8233333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "ICA",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:28.1833333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("6c812bc6-2651-4ffe-913b-816d5714eb21"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 542,
                    TimeBreakTemplateId = 40,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Minor,
                    MinTimeAfterStart = 240,
                    MinTimeBeforeEnd = 60,
                    Created = DateTime.ParseExact("2018-05-07T07:36:36.8233333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "ICA",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:28.1833333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("1a7d5f9d-3df3-4968-affe-a8f4dc254bbe"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 546,
                    TimeBreakTemplateId = 40,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 120,
                    MinTimeBeforeEnd = 240,
                    Created = DateTime.ParseExact("2018-05-07T10:49:33.9600000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "SoftOne (50)",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:28.1833333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("cd83b7fd-c20c-46ad-9777-757e2d3d427b"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 547,
                    TimeBreakTemplateId = 40,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Minor,
                    MinTimeAfterStart = 240,
                    MinTimeBeforeEnd = 60,
                    Created = DateTime.ParseExact("2018-05-07T10:49:33.9600000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "SoftOne (50)",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:28.1833333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("ba9a1151-e47c-47ca-82cb-34068f46bc46"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 552,
                    TimeBreakTemplateId = 40,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 120,
                    MinTimeBeforeEnd = 240,
                    Created = DateTime.ParseExact("2018-05-07T10:54:40.2933333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "SoftOne (50)",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:28.1833333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("d4bfc575-3ca7-42b8-9079-8b436df4c7fc"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 553,
                    TimeBreakTemplateId = 40,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Minor,
                    MinTimeAfterStart = 240,
                    MinTimeBeforeEnd = 60,
                    Created = DateTime.ParseExact("2018-05-07T10:54:40.2933333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "SoftOne (50)",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:28.1833333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("2c541624-3399-4e4c-96ee-9e78d169b8bc"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 557,
                    TimeBreakTemplateId = 40,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 120,
                    MinTimeBeforeEnd = 240,
                    Created = DateTime.ParseExact("2018-05-07T14:48:05.0966667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "SoftOne (50)",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:28.1833333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("c022f6a9-d02f-4376-b5e0-f3f6cbb92a2c"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 558,
                    TimeBreakTemplateId = 40,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Minor,
                    MinTimeAfterStart = 240,
                    MinTimeBeforeEnd = 60,
                    Created = DateTime.ParseExact("2018-05-07T14:48:05.0966667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "SoftOne (50)",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:28.1833333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("3a330367-5b56-48d3-a601-b47be97b7a77"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 562,
                    TimeBreakTemplateId = 40,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 120,
                    MinTimeBeforeEnd = 240,
                    Created = DateTime.ParseExact("2018-05-11T10:27:16.4466667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "SoftOne (50)",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:28.1833333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("7c68f50b-1507-484e-a2d1-035943674633"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 563,
                    TimeBreakTemplateId = 40,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Minor,
                    MinTimeAfterStart = 240,
                    MinTimeBeforeEnd = 60,
                    Created = DateTime.ParseExact("2018-05-11T10:27:16.4466667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "SoftOne (50)",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:28.1833333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("81a50f1c-709d-4182-96e6-181184ae7714"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 567,
                    TimeBreakTemplateId = 40,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 120,
                    MinTimeBeforeEnd = 240,
                    Created = DateTime.ParseExact("2018-05-14T10:06:14.6800000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "ICA",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:28.1833333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("28f49be9-da87-4d6d-b329-d431c90d6e3d"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 568,
                    TimeBreakTemplateId = 40,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Minor,
                    MinTimeAfterStart = 240,
                    MinTimeBeforeEnd = 60,
                    Created = DateTime.ParseExact("2018-05-14T10:06:14.6800000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "ICA",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:28.1833333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("7a3fe0f8-0161-4461-b25b-8b55f60d83e3"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 573,
                    TimeBreakTemplateId = 40,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 120,
                    MinTimeBeforeEnd = 240,
                    Created = DateTime.ParseExact("2018-05-14T10:11:53.2366667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "ICA",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:28.1833333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("c519f931-9896-436f-8a88-3c298fd5d593"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 574,
                    TimeBreakTemplateId = 40,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Minor,
                    MinTimeAfterStart = 240,
                    MinTimeBeforeEnd = 60,
                    Created = DateTime.ParseExact("2018-05-14T10:11:53.2366667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "ICA",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:28.1833333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("e468d084-5ad2-485c-b7f0-f60a10e92b8e"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 578,
                    TimeBreakTemplateId = 40,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 120,
                    MinTimeBeforeEnd = 240,
                    Created = DateTime.ParseExact("2018-05-14T12:22:36.7133333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "70",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:28.1833333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("dfc68bc5-b84c-4e85-bb44-00545825cf53"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 579,
                    TimeBreakTemplateId = 40,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Minor,
                    MinTimeAfterStart = 240,
                    MinTimeBeforeEnd = 60,
                    Created = DateTime.ParseExact("2018-05-14T12:22:36.7133333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "70",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:28.1833333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("f128421f-b67e-49a0-9190-526dd36cfce0"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 584,
                    TimeBreakTemplateId = 40,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 120,
                    MinTimeBeforeEnd = 240,
                    Created = DateTime.ParseExact("2018-05-14T13:44:36.0933333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "ICA",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:28.1833333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("cb625232-2dd1-456c-b0b9-3ab29dfbaa5f"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 585,
                    TimeBreakTemplateId = 40,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Minor,
                    MinTimeAfterStart = 240,
                    MinTimeBeforeEnd = 60,
                    Created = DateTime.ParseExact("2018-05-14T13:44:36.0933333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "ICA",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:28.1833333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("028e2f6d-8001-43e7-9fe0-b5a142d0566c"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 589,
                    TimeBreakTemplateId = 40,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 120,
                    MinTimeBeforeEnd = 240,
                    Created = DateTime.ParseExact("2018-05-14T13:44:40.6233333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "SoftOne (50)",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:28.1833333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("439a0f3a-afd5-4ae8-8f5c-430c6d9b37f7"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 590,
                    TimeBreakTemplateId = 40,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Minor,
                    MinTimeAfterStart = 240,
                    MinTimeBeforeEnd = 60,
                    Created = DateTime.ParseExact("2018-05-14T13:44:40.6233333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "SoftOne (50)",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:28.1833333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("7ee73022-f78e-4bf9-9450-e89304277d01"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 1541,
                    TimeBreakTemplateId = 40,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 120,
                    MinTimeBeforeEnd = 240,
                    Created = DateTime.ParseExact("2020-01-31T13:24:31.9800000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "50",
                    Modified = null,
                    ModifiedBy = null,
                    State = SoeEntityState.Active,
                    Guid = new Guid("742c3f48-d495-4c58-9036-3361e4c3a83f"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 1542,
                    TimeBreakTemplateId = 40,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Minor,
                    MinTimeAfterStart = 240,
                    MinTimeBeforeEnd = 60,
                    Created = DateTime.ParseExact("2020-01-31T13:24:31.9800000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "50",
                    Modified = null,
                    ModifiedBy = null,
                    State = SoeEntityState.Active,
                    Guid = new Guid("4fa89689-e6f3-4b72-8563-d65c864c3199"),
                    Length = 0
                  }
                }
              },
              new TimeBreakTemplateDTO
              {
                TimeBreakTemplateId = 41,
                ActorCompanyId = 451,
                ShiftTypeIds = null,
                DayTypeIds = null,
                DayOfWeeks = null,
                ShiftStartFromTime = DateTime.ParseExact("1900-01-01T05:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ShiftLength = 330,
                UseMaxWorkTimeBetweenBreaks = false,
                MinTimeBetweenBreaks = 0,
                StartDate = null,
                StopDate = null,
                Created = DateTime.ParseExact("2018-04-24T10:06:53.4770000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "70",
                Modified = DateTime.ParseExact("2020-01-31T13:24:31.9630000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ModifiedBy = "50",
                State = SoeEntityState.Active,
                TimeBreakTemplateRows = new List<TimeBreakTemplateRowDTO>
                {
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 540,
                    TimeBreakTemplateId = 41,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 60,
                    MinTimeBeforeEnd = 60,
                    Created = DateTime.ParseExact("2018-05-07T07:36:36.8233333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "ICA",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:29.7300000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("183f4299-743b-455e-a23c-f0b409205c1a"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 545,
                    TimeBreakTemplateId = 41,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 60,
                    MinTimeBeforeEnd = 60,
                    Created = DateTime.ParseExact("2018-05-07T10:49:33.9433333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "SoftOne (50)",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:29.7300000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("0b0bea77-c1d9-456d-8f7d-6c83946758aa"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 551,
                    TimeBreakTemplateId = 41,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 60,
                    MinTimeBeforeEnd = 60,
                    Created = DateTime.ParseExact("2018-05-07T10:54:40.2800000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "SoftOne (50)",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:29.7300000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("af305f43-df31-45ec-8d1d-5bd5f30403b4"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 536,
                    TimeBreakTemplateId = 41,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 60,
                    MinTimeBeforeEnd = 60,
                    Created = DateTime.ParseExact("2018-05-07T07:35:37.7533333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "ICA",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:29.7300000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("9f4e56fd-bad1-43f9-8a54-8a68e3a1e977"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 533,
                    TimeBreakTemplateId = 41,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 60,
                    MinTimeBeforeEnd = 60,
                    Created = DateTime.ParseExact("2018-05-07T07:33:27.7300000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "ICA",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:29.7300000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("bf779a5c-0ffd-4d06-96fa-508450b028c7"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 355,
                    TimeBreakTemplateId = 41,
                    TimeCodeBreakGroupId = 43,
                    Type = SoeTimeBreakTemplateType.Minor,
                    MinTimeAfterStart = 120,
                    MinTimeBeforeEnd = 120,
                    Created = DateTime.ParseExact("2018-04-24T10:06:53.4900000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "70",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:29.7300000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("164a24ad-53b8-463b-83a9-9f3adcef62ec"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 530,
                    TimeBreakTemplateId = 41,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 60,
                    MinTimeBeforeEnd = 60,
                    Created = DateTime.ParseExact("2018-05-07T07:32:25.6466667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "ICA",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:29.7300000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("07e0e832-a968-482e-9ed5-eeac27cff813"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 556,
                    TimeBreakTemplateId = 41,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 60,
                    MinTimeBeforeEnd = 60,
                    Created = DateTime.ParseExact("2018-05-07T14:48:05.0833333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "SoftOne (50)",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:29.7300000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("f5645fca-987a-4c87-a320-ea450da8f28b"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 561,
                    TimeBreakTemplateId = 41,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 60,
                    MinTimeBeforeEnd = 60,
                    Created = DateTime.ParseExact("2018-05-11T10:27:16.4466667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "SoftOne (50)",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:29.7300000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("7dafbe9c-d267-4264-b349-cb4a64ba4571"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 566,
                    TimeBreakTemplateId = 41,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 60,
                    MinTimeBeforeEnd = 60,
                    Created = DateTime.ParseExact("2018-05-14T10:06:14.6800000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "ICA",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:29.7300000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("8b1fafe0-2a8a-454d-9831-bca7986c92eb"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 572,
                    TimeBreakTemplateId = 41,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 60,
                    MinTimeBeforeEnd = 60,
                    Created = DateTime.ParseExact("2018-05-14T10:11:53.2233333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "ICA",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:29.7300000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("c3eaf7d9-ac7f-47ac-a36d-46383581ea05"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 577,
                    TimeBreakTemplateId = 41,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 60,
                    MinTimeBeforeEnd = 60,
                    Created = DateTime.ParseExact("2018-05-14T12:22:36.7133333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "70",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:29.7300000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("7d3b8fde-3e64-4062-b56a-e3383d762c24"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 583,
                    TimeBreakTemplateId = 41,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 60,
                    MinTimeBeforeEnd = 60,
                    Created = DateTime.ParseExact("2018-05-14T13:44:36.0933333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "ICA",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:29.7300000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("39847478-767d-426c-a57e-cc9456e50719"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 588,
                    TimeBreakTemplateId = 41,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 60,
                    MinTimeBeforeEnd = 60,
                    Created = DateTime.ParseExact("2018-05-14T13:44:40.6066667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "SoftOne (50)",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:29.7300000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("777819d9-0d19-46da-bc0f-af04701203bc"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 1540,
                    TimeBreakTemplateId = 41,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 60,
                    MinTimeBeforeEnd = 60,
                    Created = DateTime.ParseExact("2020-01-31T13:24:31.9800000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "50",
                    Modified = null,
                    ModifiedBy = null,
                    State = SoeEntityState.Active,
                    Guid = new Guid("7f705184-9e31-40c3-b2a7-7d8b3780dd71"),
                    Length = 0
                  }
                }
              },
              new TimeBreakTemplateDTO
              {
                TimeBreakTemplateId = 49,
                ActorCompanyId = 451,
                ShiftTypeIds = null,
                DayTypeIds = null,
                DayOfWeeks = null,
                ShiftStartFromTime = DateTime.ParseExact("1900-01-01T05:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ShiftLength = 330,

                UseMaxWorkTimeBetweenBreaks = false,
                MinTimeBetweenBreaks = 0,
                StartDate = null,
                StopDate = null,
                Created = DateTime.ParseExact("2018-05-07T07:35:37.7700000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "ICA",
                Modified = DateTime.ParseExact("2020-01-31T13:24:31.9800000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ModifiedBy = "50",
                State = SoeEntityState.Active,
                TimeBreakTemplateRows = new List<TimeBreakTemplateRowDTO>
                {
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 539,
                    TimeBreakTemplateId = 49,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 240,
                    MinTimeBeforeEnd = 270,
                    Created = DateTime.ParseExact("2018-05-07T07:35:37.7700000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "ICA",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:30.5566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("13b1d917-5dfa-4f3d-aa6f-1568d4ea2f95"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 554,
                    TimeBreakTemplateId = 49,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 240,
                    MinTimeBeforeEnd = 270,
                    Created = DateTime.ParseExact("2018-05-07T10:54:40.2933333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "SoftOne (50)",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:30.5566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("723678d9-90dc-4337-81f8-9064222b2b0b"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 548,
                    TimeBreakTemplateId = 49,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 240,
                    MinTimeBeforeEnd = 270,
                    Created = DateTime.ParseExact("2018-05-07T10:49:33.9600000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "SoftOne (50)",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:30.5566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("51c1290e-8f24-412c-8d08-b233ffbe5ed5"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 543,
                    TimeBreakTemplateId = 49,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 240,
                    MinTimeBeforeEnd = 270,
                    Created = DateTime.ParseExact("2018-05-07T07:36:36.8233333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "ICA",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:30.5566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("0fbfc66f-efab-4753-bd1e-85e293fc376d"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 569,
                    TimeBreakTemplateId = 49,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 240,
                    MinTimeBeforeEnd = 270,
                    Created = DateTime.ParseExact("2018-05-14T10:06:14.6966667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "ICA",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:30.5566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("b1f2edab-add8-475b-91f1-b90dfe6eadcf"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 564,
                    TimeBreakTemplateId = 49,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 240,
                    MinTimeBeforeEnd = 270,
                    Created = DateTime.ParseExact("2018-05-11T10:27:16.4466667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "SoftOne (50)",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:30.5566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("cf32f241-5316-4a22-bc14-f73dd1ac14f1"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 559,
                    TimeBreakTemplateId = 49,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 240,
                    MinTimeBeforeEnd = 270,
                    Created = DateTime.ParseExact("2018-05-07T14:48:05.0966667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "SoftOne (50)",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:30.5566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("75e26124-efab-4bfb-ba82-5f98e8c8bc01"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 591,
                    TimeBreakTemplateId = 49,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 240,
                    MinTimeBeforeEnd = 270,
                    Created = DateTime.ParseExact("2018-05-14T13:44:40.6233333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "SoftOne (50)",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:30.5566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("22fae637-92bc-4c96-8462-6704963202bc"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 586,
                    TimeBreakTemplateId = 49,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 240,
                    MinTimeBeforeEnd = 270,
                    Created = DateTime.ParseExact("2018-05-14T13:44:36.0933333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "ICA",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:30.5566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("32dbd238-debf-4a2e-9cfe-865808e5da15"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 580,
                    TimeBreakTemplateId = 49,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 240,
                    MinTimeBeforeEnd = 270,
                    Created = DateTime.ParseExact("2018-05-14T12:22:36.7300000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "70",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:30.5566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("fd284cc6-6cf3-4507-bd37-74ee809100be"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 575,
                    TimeBreakTemplateId = 49,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 240,
                    MinTimeBeforeEnd = 270,
                    Created = DateTime.ParseExact("2018-05-14T10:11:53.2366667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "ICA",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:30.5566667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("1ba7ee74-835d-4bc7-8743-2c54e0ce89f4"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 1543,
                    TimeBreakTemplateId = 49,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 240,
                    MinTimeBeforeEnd = 270,
                    Created = DateTime.ParseExact("2020-01-31T13:24:31.9800000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "50",
                    Modified = null,
                    ModifiedBy = null,
                    State = SoeEntityState.Active,
                    Guid = new Guid("5033f4e3-b065-4aeb-9bf7-0ed061145734"),
                    Length = 0
                  }
                }
              },
              new TimeBreakTemplateDTO
              {
                TimeBreakTemplateId = 50,
                ActorCompanyId = 451,
                ShiftTypeIds = null,
                DayTypeIds = null,
                DayOfWeeks = null,
                ShiftStartFromTime = DateTime.ParseExact("1900-01-01T05:00:00.0000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ShiftLength = 480,
                UseMaxWorkTimeBetweenBreaks = false,
                MinTimeBetweenBreaks = 0,
                StartDate = null,
                StopDate = null,
                Created = DateTime.ParseExact("2018-05-07T07:36:36.8230000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "ICA",
                Modified = DateTime.ParseExact("2020-01-31T13:24:31.9800000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ModifiedBy = "50",
                State = SoeEntityState.Active,
                TimeBreakTemplateRows = new List<TimeBreakTemplateRowDTO>
                {
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 544,
                    TimeBreakTemplateId = 50,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 270,
                    MinTimeBeforeEnd = 255,
                    Created = DateTime.ParseExact("2018-05-07T07:36:36.8233333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "ICA",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:31.8533333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("95cdbca3-4a24-4f78-86c6-a963d9d43465"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 550,
                    TimeBreakTemplateId = 50,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 270,
                    MinTimeBeforeEnd = 255,
                    Created = DateTime.ParseExact("2018-05-07T10:49:33.9600000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "SoftOne (50)",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:31.8533333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("dd45aff8-d3ae-4414-897c-1e6c7e7b174e"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 555,
                    TimeBreakTemplateId = 50,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 270,
                    MinTimeBeforeEnd = 255,
                    Created = DateTime.ParseExact("2018-05-07T10:54:40.2933333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "SoftOne (50)",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:31.8533333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("124bcfc4-16da-4db0-93cc-8d8d909738ba"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 576,
                    TimeBreakTemplateId = 50,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 270,
                    MinTimeBeforeEnd = 255,
                    Created = DateTime.ParseExact("2018-05-14T10:11:53.2533333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "ICA",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:31.8533333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("c95c8b8d-982f-40ae-88f0-56f979b05a3d"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 582,
                    TimeBreakTemplateId = 50,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 270,
                    MinTimeBeforeEnd = 255,
                    Created = DateTime.ParseExact("2018-05-14T12:22:36.7300000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "70",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:31.8533333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("6638dee8-23b1-4b6e-9f15-3d724cdb0e83"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 587,
                    TimeBreakTemplateId = 50,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 270,
                    MinTimeBeforeEnd = 255,
                    Created = DateTime.ParseExact("2018-05-14T13:44:36.0933333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "ICA",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:31.8533333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("d7cd46fb-4059-479f-a2dd-f03d937b0d6b"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 592,
                    TimeBreakTemplateId = 50,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 270,
                    MinTimeBeforeEnd = 255,
                    Created = DateTime.ParseExact("2018-05-14T13:44:40.6233333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "SoftOne (50)",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:31.8533333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("b8543a9f-e780-44b8-bb66-0e4af60295c7"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 560,
                    TimeBreakTemplateId = 50,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 270,
                    MinTimeBeforeEnd = 255,
                    Created = DateTime.ParseExact("2018-05-07T14:48:05.0966667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "SoftOne (50)",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:31.8533333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("3b1d3ab8-a28f-41d2-b683-456d83ac2161"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 565,
                    TimeBreakTemplateId = 50,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 270,
                    MinTimeBeforeEnd = 255,
                    Created = DateTime.ParseExact("2018-05-11T10:27:16.4633333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "SoftOne (50)",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:31.8533333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("3274a869-ad53-4492-8114-1697576c58f9"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 571,
                    TimeBreakTemplateId = 50,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 270,
                    MinTimeBeforeEnd = 255,
                    Created = DateTime.ParseExact("2018-05-14T10:06:14.6966667", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "ICA",
                    Modified = DateTime.ParseExact("2020-01-31T13:24:31.8533333", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    ModifiedBy = "50",
                    State = SoeEntityState.Deleted,
                    Guid = new Guid("43c03f8b-0eb9-4c46-8df6-d2573c55d6df"),
                    Length = 0
                  },
                  new TimeBreakTemplateRowDTO
                  {
                    TimeBreakTemplateRowId = 1544,
                    TimeBreakTemplateId = 50,
                    TimeCodeBreakGroupId = 44,
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = 270,
                    MinTimeBeforeEnd = 255,
                    Created = DateTime.ParseExact("2020-01-31T13:24:31.9800000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    CreatedBy = "50",
                    Modified = null,
                    ModifiedBy = null,
                    State = SoeEntityState.Active,
                    Guid = new Guid("da64168b-5327-4dd7-b3cc-3ab8e0e1994f"),
                    Length = 0
                  }
                }
              }
            };

            return listOfTimeBreakTemplateDTOs;
        }
    }
}
