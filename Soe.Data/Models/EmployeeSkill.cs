using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Data.Util;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SoftOne.Soe.Data
{
    public partial class EmployeeSkill
    {

    }

    public static partial class EntityExtensions
    {
        #region EmployeeSkill

        public static EmployeeSkillDTO ToDTO(this EmployeeSkill e, bool setNames)
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

            return new EmployeeSkillDTO()
            {
                EmployeeSkillId = e.EmployeeSkillId,
                EmployeeId = e.EmployeeId,
                SkillId = e.SkillId,
                SkillLevel = e.SkillLevel,
                DateTo = e.DateTo,
                SkillName = e.Skill?.Name ?? string.Empty,
                SkillTypeName = e.Skill?.SkillType?.Name ?? string.Empty
            };
        }

        public static List<EmployeeSkillDTO> ToDTOs(this IEnumerable<EmployeeSkill> l, bool setNames)
        {
            var dtos = new List<EmployeeSkillDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(setNames));
                }
            }
            return dtos;
        }

        public readonly static Expression<Func<EmployeeSkill, EmployeeSkillDTO>> GetEmployeeSkillDTO =
         e => new EmployeeSkillDTO
         {
             EmployeeSkillId = e.EmployeeSkillId,
             EmployeeId = e.EmployeeId,
             SkillId = e.SkillId,
             SkillLevel = e.SkillLevel,
             DateTo = e.DateTo,
             SkillName = e.Skill != null ? e.Skill.Name : string.Empty,//Cannot use null propagating operator in querys
             SkillTypeName = e.Skill != null && e.Skill.SkillType != null ? e.Skill.SkillType.Name : string.Empty,//Cannot use null propagating operator in querys
         };

        #endregion
    }
}
