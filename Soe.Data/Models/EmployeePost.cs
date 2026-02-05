using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Data
{
    public partial class EmployeePost : ICreatedModified, IState
    {
        public List<SmallGenericType> DayOfWeeksGenericType { get; set; }
        public List<int> DayOfWeekIds { get; set; }
        public string DayOfWeeksGridString { get; set; }
        public string SkillNames { get; set; }
    }

    public static partial class EntityExtensions
    {
        #region EmployeePost

        public static EmployeePostDTO ToDTO(this EmployeePost e, bool includeEmployeeGroupName)
        {
            if (e == null)
                return null;

            EmployeePostDTO dto = new EmployeePostDTO()
            {
                EmployeePostId = e.EmployeePostId,
                ActorCompanyId = e.ActorCompanyId,
                EmployeeGroupId = e.EmployeeGroupId,
                ScheduleCycleId = e.ScheduleCycleId,
                Name = e.Name,
                Description = e.Description,
                DateFrom = e.DateFrom,
                DateTo = e.DateTo,
                WorkTimeWeek = e.WorkTimeWeek,
                WorkTimePercent = e.WorkTimePercent,
                DayOfWeeks = e.DayOfWeeks,
                DayOfWeeksGridString = e.DayOfWeeksGridString,
                DayOfWeekIds = e.DayOfWeekIds,
                DayOfWeeksGenericType = e.DayOfWeeksGenericType,
                WorkDaysWeek = e.WorkDaysWeek,
                Status = (SoeEmployeePostStatus)e.Status,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                EmployeePostWeekendType = (TermGroup_EmployeePostWeekendType)e.WeekendType,
                AccountId = e.AccountId,
                AccountName = e.Account?.Name ?? string.Empty,
                SkillNames = e.SkillNames
            };

            if (includeEmployeeGroupName && e.EmployeeGroupId != null)
            {
                if (!e.EmployeeGroupReference.IsLoaded)
                    e.EmployeeGroupReference.Load();
                dto.EmployeeGroupName = e.EmployeeGroup.Name;
            }

            if (e.EmployeePostSkill != null)
                dto.EmployeePostSkillDTOs = e.EmployeePostSkill.ToDTOs(true);
            if (e.ScheduleCycle != null)
                dto.ScheduleCycleDTO = e.ScheduleCycle.ToDTO();
            if (e.EmployeeGroup != null)
                dto.EmployeeGroupDTO = e.EmployeeGroup.ToDTO();

            return dto;
        }

        public static IEnumerable<EmployeePostDTO> ToDTOs(this IEnumerable<EmployeePost> l, bool includeEmployeeGroupName)
        {
            var dtos = new List<EmployeePostDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeEmployeeGroupName));
                }
            }
            return dtos;
        }

        #endregion

        #region EmployeePostSkill

        public static EmployeePostSkillDTO ToDTO(this EmployeePostSkill e, bool setNames)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded() && setNames)
                {
                    if (!e.SkillReference.IsLoaded)
                    {
                        e.SkillReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("EmployeeSkill.cs e.SkillReference");
                    }
                    if (e.Skill != null && !e.Skill.SkillTypeReference.IsLoaded)
                    {
                        e.Skill.SkillTypeReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("EmployeeSkill.cs e.Skill.SkillTypeReference");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            EmployeePostSkillDTO dto = new EmployeePostSkillDTO()
            {
                EmployeePostSkillId = e.EmployeePostSkillId,
                EmployeePostId = e.EmployeePostId,
                SkillId = e.SkillId,
                SkillLevel = e.SkillLevel,
                SkillName = e.Skill?.Name ?? string.Empty,
                SkillTypeName = e.Skill?.SkillType?.Name ?? string.Empty
            };

            if (e.SkillReference.IsLoaded)
                dto.SkillDTO = e.Skill.ToDTO();

            return dto;
        }

        public static List<EmployeePostSkillDTO> ToDTOs(this IEnumerable<EmployeePostSkill> l, bool setNames)
        {
            var dtos = new List<EmployeePostSkillDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(setNames));
                }
            }
            return dtos;
        }

        public static EmployeeSkillDTO ToEmployeeSkillDTO(this EmployeePostSkill e, bool setNames)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded() && setNames)
                {
                    if (!e.SkillReference.IsLoaded)
                    {
                        e.SkillReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("EmployeeSkill.cs e.SkillReference2");
                    }
                    if (e.Skill != null && !e.Skill.SkillTypeReference.IsLoaded)
                    {
                        e.Skill.SkillTypeReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("EmployeeSkill.cs e.Skill.SkillTypeReference2");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            EmployeeSkillDTO dto = new EmployeeSkillDTO()
            {
                EmployeeSkillId = e.EmployeePostSkillId,
                EmployeeId = e.EmployeePostId,
                SkillId = e.SkillId,
                SkillLevel = e.SkillLevel,
                SkillName = e.Skill?.Name ?? string.Empty,
                SkillTypeName = e.Skill?.SkillType?.Name ?? string.Empty
            };

            return dto;
        }

        public static List<EmployeeSkillDTO> ToEmployeeSkillDTOs(this IEnumerable<EmployeePostSkill> l, bool setNames)
        {
            var dtos = new List<EmployeeSkillDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToEmployeeSkillDTO(setNames));
                }
            }
            return dtos;
        }

        #endregion
    }
}
