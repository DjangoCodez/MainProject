using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Data.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class Position : ICreatedModified, IState
    {

    }

    public static partial class EntityExtensions
    {
        #region Position

        public static PositionDTO ToDTO(this Position e, bool includeSkills = false)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (includeSkills && !e.IsAdded() && !e.PositionSkill.IsLoaded)
                {
                    e.PositionSkill.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("Position.cs e.PositionSkill");
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            PositionDTO dto = new PositionDTO()
            {
                PositionId = e.PositionId,
                ActorCompanyId = e.ActorCompanyId,
                SysPositionId = e.SysPositionId,
                Code = e.Code,
                Name = e.Name,
                Description = e.Description,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy
            };

            // Relations
            if (includeSkills)
                dto.PositionSkills = e.PositionSkill.ToDTOs().ToList();

            return dto;
        }

        public static IEnumerable<PositionDTO> ToDTOs(this IEnumerable<Position> l, bool includeSkills = false)
        {
            var dtos = new List<PositionDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeSkills));
                }
            }
            return dtos;
        }

        public static PositionGridDTO ToGridDTO(this Position e)
        {
            if (e == null)
                return null;

            return new PositionGridDTO()
            {
                PositionId = e.PositionId,
                SysPositionId = e.SysPositionId,
                Code = e.Code,
                Name = e.Name,
                Description = e.Description,
            };
        }

        public static IEnumerable<PositionGridDTO> ToGridDTOs(this IEnumerable<Position> l)
        {
            var dtos = new List<PositionGridDTO>();
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

        #region PositionSkill

        public static PositionSkillDTO ToDTO(this PositionSkill e)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded())
                {
                    if (!e.PositionReference.IsLoaded)
                    {
                        e.PositionReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("Position.cs e.PositionReference");
                    }
                    if (!e.SkillReference.IsLoaded)
                    {
                        e.SkillReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("Position.cs e.SkillReference");
                    }
                    if (e.Skill != null && !e.Skill.SkillTypeReference.IsLoaded)
                    {
                        e.Skill.SkillTypeReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("Position.cs e.Skill.SkillTypeReference");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            return new PositionSkillDTO()
            {
                PositionSkillId = e.PositionSkillId,
                PositionId = e.PositionId,
                SkillId = e.SkillId,
                SkillLevel = e.SkillLevel,
                PositionName = e.Position?.Name ?? string.Empty,
                SkillName = e.Skill?.Name ?? string.Empty,
                SkillTypeName = e.Skill?.SkillType?.Name ?? string.Empty,
            };
        }

        public static IEnumerable<PositionSkillDTO> ToDTOs(this IEnumerable<PositionSkill> l)
        {
            var dtos = new List<PositionSkillDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion
    }
}
