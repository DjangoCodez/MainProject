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
    public static class ShiftTypeMock
    {
        public static List<ShiftTypeDTO> GetShiftTypes(StaffingNeedMockScenario staffingNeedMockScenario)
        {
            switch (staffingNeedMockScenario)
            {
                case StaffingNeedMockScenario.All:
                case StaffingNeedMockScenario.FourtyHours:
                    return GetShiftTypes();
                default:
                    return GetShiftTypes();

            }
        }

        private static List<ShiftTypeDTO> GetShiftTypes()
        {
            var listOfShiftTypeDTOs = new List<ShiftTypeDTO>
            {
              new ShiftTypeDTO
              {
                ShiftTypeId = 1,
                ActorCompanyId = 451,
                TimeScheduleTypeId = null,
                TimeScheduleTypeName = null,
                TimeScheduleTemplateBlockType = null,
                Name = "Kontor",
                Description = "",
                Color = "#92CDDC",
                NeedsCode = "",
                ExternalId = null,
                ExternalCode = "",
                DefaultLength = 0,
                StartTime = null,
                StopTime = null,
                HandlingMoney = false,
                AccountId = null,
                Created = DateTime.ParseExact("2017-03-01T10:13:46.7000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "BjörnS",
                Modified = DateTime.ParseExact("2020-01-31T14:44:49.3100000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ModifiedBy = "50",
                State = SoeEntityState.Active,
                AccountInternals = new Dictionary<int, AccountSmallDTO>
                {
                  { 2, new AccountSmallDTO
                  {
                    AccountId = 7440,
                    AccountDimId = 68,
                    ParentAccountId = null,
                    Number = "98",
                    Name = "Jour",
                    Percent = 0m
                  } }
                },
                AccountInternalIds = null,
                AccountingSettings = null,
                ShiftTypeSkills = new List<ShiftTypeSkillDTO>
                {
                  new ShiftTypeSkillDTO
                  {
                    ShiftTypeSkillId = 881,
                    ShiftTypeId = 1,
                    SkillId = 352,
                    SkillLevel = 100,
                    SkillName = "Kontor",
                    SkillTypeName = "allmän",
                    SkillLevelStars = 0d,
                    Missing = false
                  }
                },
                EmployeeStatisticsTargets = null,
                LinkedShiftTypeIds = new List<int>
                {
                },
                CategoryIds = null,
                HierarchyAccounts = new List<ShiftTypeHierarchyAccountDTO>
                {
                },
                ChildHierarchyAccountIds = null
              },
              new ShiftTypeDTO
              {
                ShiftTypeId = 284,
                ActorCompanyId = 451,
                TimeScheduleTypeId = null,
                TimeScheduleTypeName = null,
                TimeScheduleTemplateBlockType = null,
                Name = "Butik",
                Description = "",
                Color = "#92CDDC",
                NeedsCode = "",
                ExternalId = null,
                ExternalCode = "",
                DefaultLength = 0,
                StartTime = null,
                StopTime = null,
                HandlingMoney = false,
                AccountId = null,
                Created = DateTime.ParseExact("2017-03-01T10:13:46.7000000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "BjörnS",
                Modified = DateTime.ParseExact("2020-01-31T14:44:49.3100000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ModifiedBy = "50",
                State = SoeEntityState.Active,
                AccountInternals = new Dictionary<int, AccountSmallDTO>
                {
                  { 2, new AccountSmallDTO
                  {
                    AccountId = 7440,
                    AccountDimId = 68,
                    ParentAccountId = null,
                    Number = "98",
                    Name = "Jour",
                    Percent = 0m
                  } }
                },
                AccountInternalIds = null,
                AccountingSettings = null,
                ShiftTypeSkills = new List<ShiftTypeSkillDTO>
                {
                  new ShiftTypeSkillDTO
                  {
                    ShiftTypeSkillId = 881,
                    ShiftTypeId = 284,
                    SkillId = 352,
                    SkillLevel = 20,
                    SkillName = "Butik",
                    SkillTypeName = "allmän",
                    SkillLevelStars = 0d,
                    Missing = false
                  }
                },
                EmployeeStatisticsTargets = null,
                LinkedShiftTypeIds = new List<int>
                {
                },
                CategoryIds = null,
                HierarchyAccounts = new List<ShiftTypeHierarchyAccountDTO>
                {
                },
                ChildHierarchyAccountIds = null
              },
              new ShiftTypeDTO
              {
                ShiftTypeId = 4229,
                ActorCompanyId = 451,
                TimeScheduleTypeId = null,
                TimeScheduleTypeName = null,
                TimeScheduleTemplateBlockType = null,
                Name = "djupfryst",
                Description = "",
                Color = "#FF548DD4",
                NeedsCode = "",
                ExternalId = null,
                ExternalCode = "",
                DefaultLength = 0,
                StartTime = null,
                StopTime = null,
                HandlingMoney = false,
                AccountId = null,
                Created = DateTime.ParseExact("2018-04-24T09:41:02.4830000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "70",
                Modified = null,
                ModifiedBy = null,
                State = SoeEntityState.Active,
                AccountInternals = new Dictionary<int, AccountSmallDTO>
                {
                },
                AccountInternalIds = null,
                AccountingSettings = null,
                ShiftTypeSkills = new List<ShiftTypeSkillDTO>
                {
                },
                EmployeeStatisticsTargets = null,
                LinkedShiftTypeIds = new List<int>
                {
                },
                CategoryIds = null,
                HierarchyAccounts = new List<ShiftTypeHierarchyAccountDTO>
                {
                },
                ChildHierarchyAccountIds = null
              },
              new ShiftTypeDTO
              {
                ShiftTypeId = 4228,
                ActorCompanyId = 451,
                TimeScheduleTypeId = null,
                TimeScheduleTypeName = null,
                TimeScheduleTemplateBlockType = TermGroup_TimeScheduleTemplateBlockType.Schedule,
                Name = "Frukt och grönt",
                Description = "",
                Color = "#4F6128",
                NeedsCode = "",
                ExternalId = null,
                ExternalCode = "",
                DefaultLength = 0,
                StartTime = null,
                StopTime = null,
                HandlingMoney = false,
                AccountId = null,
                Created = DateTime.ParseExact("2018-04-24T09:38:45.1570000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "70",
                Modified = DateTime.ParseExact("2018-11-30T15:26:32.8830000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ModifiedBy = "SoftOne (50)",
                State = SoeEntityState.Active,
                AccountInternals = new Dictionary<int, AccountSmallDTO>
                {
                },
                AccountInternalIds = null,
                AccountingSettings = null,
                ShiftTypeSkills = new List<ShiftTypeSkillDTO>
                {
                  new ShiftTypeSkillDTO
                  {
                    ShiftTypeSkillId = 199,
                    ShiftTypeId = 4228,
                    SkillId = 354,
                    SkillLevel = 20,
                    SkillName = "Frukt",
                    SkillTypeName = "allmän",
                    SkillLevelStars = 0d,
                    Missing = false
                  }
                },
                EmployeeStatisticsTargets = null,
                LinkedShiftTypeIds = new List<int>
                {
                },
                CategoryIds = null,
                HierarchyAccounts = new List<ShiftTypeHierarchyAccountDTO>
                {
                },
                ChildHierarchyAccountIds = null
              },
              new ShiftTypeDTO
              {
                ShiftTypeId = 4190,
                ActorCompanyId = 451,
                TimeScheduleTypeId = null,
                TimeScheduleTypeName = null,
                TimeScheduleTemplateBlockType = null,
                Name = "Kassa",
                Description = "",
                Color = "#FF595959",
                NeedsCode = "",
                ExternalId = null,
                ExternalCode = "",
                DefaultLength = 0,
                StartTime = null,
                StopTime = null,
                HandlingMoney = false,
                AccountId = null,
                Created = DateTime.ParseExact("2018-04-20T12:48:00.9570000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "SoftOne (50)",
                Modified = DateTime.ParseExact("2018-05-03T07:22:25.6800000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ModifiedBy = "ICA",
                State = SoeEntityState.Active,
                AccountInternals = new Dictionary<int, AccountSmallDTO>
                {
                  { 2, new AccountSmallDTO
                  {
                    AccountId = 7408,
                    AccountDimId = 68,
                    ParentAccountId = null,
                    Number = "95",
                    Name = "Kassa",
                    Percent = 0m
                  } }
                },
                AccountInternalIds = null,
                AccountingSettings = null,
                ShiftTypeSkills = new List<ShiftTypeSkillDTO>
                {
                  new ShiftTypeSkillDTO
                  {
                    ShiftTypeSkillId = 201,
                    ShiftTypeId = 4190,
                    SkillId = 369,
                    SkillLevel = 100,
                    SkillName = "kassa",
                    SkillTypeName = "allmän",
                    SkillLevelStars = 0d,
                    Missing = false
                  }
                },
                EmployeeStatisticsTargets = null,
                LinkedShiftTypeIds = new List<int>
                {
                },
                CategoryIds = null,
                HierarchyAccounts = new List<ShiftTypeHierarchyAccountDTO>
                {
                },
                ChildHierarchyAccountIds = null
              },
              new ShiftTypeDTO
              {
                ShiftTypeId = 4232,
                ActorCompanyId = 451,
                TimeScheduleTypeId = null,
                TimeScheduleTypeName = null,
                TimeScheduleTemplateBlockType = null,
                Name = "kolonial",
                Description = "",
                Color = "#E36C09",
                NeedsCode = "",
                ExternalId = null,
                ExternalCode = "",
                DefaultLength = 0,
                StartTime = null,
                StopTime = null,
                HandlingMoney = false,
                AccountId = null,
                Created = DateTime.ParseExact("2018-04-24T09:42:23.4870000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "70",
                Modified = DateTime.ParseExact("2018-11-01T13:52:25.7530000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ModifiedBy = "SoftOne (50)",
                State = SoeEntityState.Active,
                AccountInternals = new Dictionary<int, AccountSmallDTO>
                {
                },
                AccountInternalIds = null,
                AccountingSettings = null,
                ShiftTypeSkills = new List<ShiftTypeSkillDTO>
                {
                  new ShiftTypeSkillDTO
                  {
                    ShiftTypeSkillId = 200,
                    ShiftTypeId = 4232,
                    SkillId = 355,
                    SkillLevel = 20,
                    SkillName = "Kolonial",
                    SkillTypeName = "allmän",
                    SkillLevelStars = 0d,
                    Missing = false
                  }
                },
                EmployeeStatisticsTargets = null,
                LinkedShiftTypeIds = new List<int>
                {
                },
                CategoryIds = null,
                HierarchyAccounts = new List<ShiftTypeHierarchyAccountDTO>
                {
                },
                ChildHierarchyAccountIds = null
              },
              new ShiftTypeDTO
              {
                ShiftTypeId = 4231,
                ActorCompanyId = 451,
                TimeScheduleTypeId = null,
                TimeScheduleTypeName = null,
                TimeScheduleTemplateBlockType = null,
                Name = "kött och chark",
                Description = "",
                Color = "#FF953734",
                NeedsCode = "",
                ExternalId = null,
                ExternalCode = "",
                DefaultLength = 0,
                StartTime = null,
                StopTime = null,
                HandlingMoney = false,
                AccountId = null,
                Created = DateTime.ParseExact("2018-04-24T09:41:43.0800000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "70",
                Modified = null,
                ModifiedBy = null,
                State = SoeEntityState.Active,
                AccountInternals = new Dictionary<int, AccountSmallDTO>
                {
                },
                AccountInternalIds = null,
                AccountingSettings = null,
                ShiftTypeSkills = new List<ShiftTypeSkillDTO>
                {
                },
                EmployeeStatisticsTargets = null,
                LinkedShiftTypeIds = new List<int>
                {
                },
                CategoryIds = null,
                HierarchyAccounts = new List<ShiftTypeHierarchyAccountDTO>
                {
                },
                ChildHierarchyAccountIds = null
              },
              new ShiftTypeDTO
              {
                ShiftTypeId = 4230,
                ActorCompanyId = 451,
                TimeScheduleTypeId = null,
                TimeScheduleTypeName = null,
                TimeScheduleTemplateBlockType = null,
                Name = "mejeri",
                Description = "",
                Color = "#FFE7E707",
                NeedsCode = "",
                ExternalId = null,
                ExternalCode = "",
                DefaultLength = 0,
                StartTime = null,
                StopTime = null,
                HandlingMoney = false,
                AccountId = null,
                Created = DateTime.ParseExact("2018-04-24T09:41:29.6570000", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                CreatedBy = "70",
                Modified = null,
                ModifiedBy = null,
                State = SoeEntityState.Active,
                AccountInternals = new Dictionary<int, AccountSmallDTO>
                {
                },
                AccountInternalIds = null,
                AccountingSettings = null,
                ShiftTypeSkills = new List<ShiftTypeSkillDTO>
                {
                },
                EmployeeStatisticsTargets = null,
                LinkedShiftTypeIds = new List<int>
                {
                },
                CategoryIds = null,
                HierarchyAccounts = new List<ShiftTypeHierarchyAccountDTO>
                {
                },
                ChildHierarchyAccountIds = null
              }
            };
            return listOfShiftTypeDTOs;
        }
    }
}
