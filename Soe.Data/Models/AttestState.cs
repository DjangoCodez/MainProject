using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SoftOne.Soe.Data
{
    public partial class AttestState : ICreatedModified, IState
    {
        public string EntityName { get; set; }
    }

    public static partial class EntityExtensions
    {
        #region AttestState

        public readonly static Expression<Func<AttestState, AttestStateDTO>> AttestStateDTO =
        e => new AttestStateDTO
        {
            AttestStateId = e.AttestStateId,
            ActorCompanyId = e.ActorCompanyId,
            Entity = (TermGroup_AttestEntity)e.Entity,
            Module = (SoeModule)e.Module,
            Name = e.Name,
            Description = e.Description,
            Color = e.Color,
            ImageSource = e.ImageSource,
            Sort = e.Sort,
            Initial = e.Initial,
            Closed = e.Closed,
            Hidden = e.Hidden,
            Locked = e.Locked,
            Created = e.Created,
            CreatedBy = e.CreatedBy,
            Modified = e.Modified,
            ModifiedBy = e.ModifiedBy,
            State = (SoeEntityState)e.State
        };

        public static IEnumerable<AttestStateDTO> ToDTOs(this IEnumerable<AttestState> l)
        {
            var dtos = new List<AttestStateDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static AttestStateDTO ToDTO(this AttestState e)
        {
            if (e == null)
                return null;

            return new AttestStateDTO()
            {
                AttestStateId = e.AttestStateId,
                ActorCompanyId = e.ActorCompanyId,
                Entity = (TermGroup_AttestEntity)e.Entity,
                Module = (SoeModule)e.Module,
                Name = e.Name,
                Description = e.Description,
                Color = e.Color,
                ImageSource = e.ImageSource,
                Sort = e.Sort,
                Initial = e.Initial,
                Closed = e.Closed,
                Hidden = e.Hidden,
                Locked = e.Locked,
                EntityName = e.EntityName,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };
        }

        public static IEnumerable<AttestStateSmallDTO> ToSmallDTOs(this IEnumerable<AttestState> l)
        {
            var dtos = new List<AttestStateSmallDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToSmallDTO());
                }
            }
            return dtos;
        }

        public static AttestStateSmallDTO ToSmallDTO(this AttestState e)
        {
            if (e == null)
                return null;

            return new AttestStateSmallDTO()
            {
                AttestStateId = e.AttestStateId,
                Name = e.Name,
                Description = e.Description,
                Color = e.Color,
                ImageSource = e.ImageSource,
                Sort = e.Sort,
                Initial = e.Initial,
                Closed = e.Closed,
            };
        }

        public static IEnumerable<AttestStateSmallDTO> ToSmallDTOs(this IEnumerable<AttestStateDTO> l)
        {
            var dtos = new List<AttestStateSmallDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToSmallDTO());
                }
            }
            return dtos;
        }

        public static AttestStateSmallDTO ToSmallDTO(this AttestStateDTO e)
        {
            if (e == null)
                return null;

            return new AttestStateSmallDTO()
            {
                AttestStateId = e.AttestStateId,
                Name = e.Name,
                Description = e.Description,
                Color = e.Color,
                ImageSource = e.ImageSource,
                Sort = e.Sort,
                Initial = e.Initial,
                Closed = e.Closed,
            };
        }

        public static List<AttestStateDTO> Filter(this List<AttestStateDTO> l, List<int> attestStateIds)
        {
            var attestStates = new List<AttestStateDTO>();
            foreach (int attestStateId in attestStateIds)
            {
                var e = l.FirstOrDefault(i => i.AttestStateId == attestStateId);
                if (e != null)
                    attestStates.Add(e);
            }
            return attestStates;
        }

        public static bool Contains(this List<AttestState> l, int attestStateId)
        {
            return l?.Any(i => i.AttestStateId == attestStateId) ?? false;
        }

        public static string GetAttestStateNameLowest(this IEnumerable<AttestState> l)
        {
            l.GetAttestStateLowest(out int? _, out int _, out string _, out string attestStateLowestName);
            return attestStateLowestName;
        }

        public static string GetAttestStateColorLowest(this IEnumerable<AttestState> l)
        {
            l.GetAttestStateLowest(out int? _, out int _, out string attestStateLowestColor, out string _);
            return attestStateLowestColor;
        }

        public static int? GetAttestStateIdLowest(this IEnumerable<AttestState> l)
        {
            l.GetAttestStateLowest(out int? attestStateId, out int _, out string _, out string _);
            return attestStateId;
        }

        public static void GetAttestStateLowest(this IEnumerable<AttestState> l, out int? attestStateId, out int attestStateLowestSort, out string attestStateLowestColor, out string attestStateLowestName, string defaultColor = "#FFFFFF")
        {
            AttestState attestStateLowest = null;

            if (!l.IsNullOrEmpty())
            {
                if (l.Count() == 1 && l.First().AttestStateId < 0)
                    attestStateLowest = l.First();
                else
                    attestStateLowest = l.Where(i => i.Sort >= 0).OrderBy(i => i.Sort).FirstOrDefault();
            }

            attestStateId = attestStateLowest?.AttestStateId;
            attestStateLowestSort = attestStateLowest?.Sort ?? 0;
            attestStateLowestColor = attestStateLowest?.Color ?? defaultColor;
            attestStateLowestName = attestStateLowest?.Name ?? string.Empty;
        }

        public static bool DoRemind(this IEnumerable<AttestState> l, AttestState attestStateReminder)
        {
            if (l.IsNullOrEmpty() || attestStateReminder == null)
                return false;

            return l.Any(a => a.Sort > 0) && l.Any(a => a.Sort < attestStateReminder.Sort);
        }

        #endregion
    }
}
