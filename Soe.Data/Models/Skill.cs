using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class Skill : ICreatedModified, IState
    {
        public string SkillTypeName { get; set; }
    }

    public partial class SkillType : ICreatedModified, IState
    {

    }

    public static partial class EntityExtensions
    {
        #region Skill

        public static SkillDTO ToDTO(this Skill e)
        {
            if (e == null)
                return null;

            SkillDTO dto = new SkillDTO()
            {
                SkillId = e.SkillId,
                SkillTypeId = e.SkillTypeId,
                ActorCompanyId = e.ActorCompanyId,
                Name = e.Name,
                Description = e.Description,
                SkillTypeName = e.SkillTypeName,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            if (e.SkillTypeReference.IsLoaded)
                dto.SkillTypeDTO = e.SkillType.ToDTO();

            return dto;
        }

        public static IEnumerable<SkillDTO> ToDTOs(this IEnumerable<Skill> l)
        {
            var dtos = new List<SkillDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static SkillGridDTO ToGridDTO(this Skill e)
        {
            if (e == null)
                return null;

            SkillGridDTO dto = new SkillGridDTO()
            {
                SkillId = e.SkillId,
                SkillTypeId = e.SkillTypeId,
                SkillTypeName = e.SkillTypeName,
                Name = e.Name,
                Description = e.Description,
                State = (SoeEntityState)e.State
            };

            return dto;
        }

        public static IEnumerable<SkillGridDTO> ToGridDTOs(this IEnumerable<Skill> l)
        {
            var dtos = new List<SkillGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region ShiftTypeSkill

        public static ShiftTypeSkillDTO ToDTO(this ShiftTypeSkill e)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded())
                {
                    if (!e.SkillReference.IsLoaded)
                    {
                        e.SkillReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("Skill.cs e.SkillReference");
                    }
                    if (e.Skill != null && !e.Skill.SkillTypeReference.IsLoaded)
                    {
                        e.Skill.SkillTypeReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("Skill.cs e.Skill.SkillTypeReference");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            return new ShiftTypeSkillDTO()
            {
                ShiftTypeSkillId = e.ShiftTypeSkillId,
                ShiftTypeId = e.ShiftTypeId,
                SkillId = e.SkillId,
                SkillLevel = e.SkillLevel,
                SkillName = e.Skill?.Name ?? string.Empty,
                SkillTypeName = e.Skill?.SkillType?.Name ?? string.Empty
            };
        }

        public static IEnumerable<ShiftTypeSkillDTO> ToDTOs(this IEnumerable<ShiftTypeSkill> l)
        {
            var dtos = new List<ShiftTypeSkillDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static bool IsValid(this List<ShiftTypeSkill> l, List<EmployeeSkill> employeeSkills, DateTime date)
        {
            if (employeeSkills == null)
                return false;

            foreach (ShiftTypeSkill e in l)
            {
                EmployeeSkill employeeSkill = employeeSkills.FirstOrDefault(s => s.SkillId == e.SkillId);
                if (employeeSkill == null || employeeSkill.SkillLevel < e.SkillLevel || (employeeSkill.DateTo.HasValue && employeeSkill.DateTo.Value < date))
                    return false;
            }

            return true;
        }

        public static bool IsValid(this List<ShiftTypeSkill> l, List<EmployeePostSkill> employeePostSkills)
        {
            if (employeePostSkills == null)
                return false;

            foreach (ShiftTypeSkill e in l)
            {
                EmployeePostSkill employeePostSkill = employeePostSkills.FirstOrDefault(s => s.SkillId == e.SkillId);
                if (employeePostSkill == null || employeePostSkill.SkillLevel < e.SkillLevel)
                    return false;
            }

            return true;
        }

        #endregion

        #region EmployeePostSkill

        public static bool IsValid(this List<EmployeePostSkill> l, List<EmployeeSkill> employeeSkills, DateTime date)
        {
            if (employeeSkills == null)
                return false;

            foreach (EmployeePostSkill e in l)
            {
                EmployeeSkill employeeSkill = employeeSkills.FirstOrDefault(s => s.SkillId == e.SkillId);
                if (employeeSkill == null || employeeSkill.SkillLevel < e.SkillLevel || (employeeSkill.DateTo.HasValue && employeeSkill.DateTo.Value < date))
                    return false;
            }

            return true;
        }

        #endregion

        #region SkillType

        public static SkillTypeDTO ToDTO(this SkillType e)
        {
            if (e == null)
                return null;

            return new SkillTypeDTO()
            {
                SkillTypeId = e.SkillTypeId,
                ActorCompanyId = e.ActorCompanyId,
                Name = e.Name,
                Description = e.Description,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };
        }

        public static IEnumerable<SkillTypeDTO> ToDTOs(this IEnumerable<SkillType> l)
        {
            var dtos = new List<SkillTypeDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static SkillTypeGridDTO ToGridDTO(this SkillType e)
        {
            if (e == null)
                return null;

            return new SkillTypeGridDTO()
            {
                SkillTypeId = e.SkillTypeId,
                Name = e.Name,
                Description = e.Description,
                State = (SoeEntityState)e.State
            };
        }

        public static IEnumerable<SkillTypeGridDTO> ToGridDTOs(this IEnumerable<SkillType> l)
        {
            var dtos = new List<SkillTypeGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        #endregion
    }
}
