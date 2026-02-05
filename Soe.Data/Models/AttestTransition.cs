using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class AttestTransition : ICreatedModified, IState
    {
        public bool IsSigningTransition 
        { 
            get { return this.Module == (int)SoeModule.Manage; }             
        }
    }

    public static partial class EntityExtensions
    {
        #region AttestTransition

        public static AttestTransitionDTO ToDTO(this AttestTransition e, bool includeAttestStates)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (includeAttestStates && !e.IsAdded())
                {
                    if (!e.AttestStateFromReference.IsLoaded)
                    {
                        e.AttestStateFromReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("AttestTransition.cs e.AttestStateFromReference");
                    }
                    if (!e.AttestStateToReference.IsLoaded)
                    {
                        e.AttestStateToReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("AttestTransition.cs e.AttestStateToReference");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            AttestTransitionDTO dto = new AttestTransitionDTO()
            {
                AttestTransitionId = e.AttestTransitionId,
                ActorCompanyId = e.ActorCompanyId,
                AttestStateFromId = e.AttestStateFromId,
                AttestStateToId = e.AttestStateToId,
                Module = (SoeModule)e.Module,
                Name = e.Name,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                NotifyChangeOfAttestState = e.NotifyChangeOfAttestState
            };

            if (includeAttestStates)
            {
                dto.AttestStateFrom = e.AttestStateFrom?.ToDTO();
                dto.AttestStateTo = e.AttestStateTo?.ToDTO();
            }

            return dto;
        }

        public static AttestTransitionGridDTO ToGridDTO(this AttestTransition e)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded())
                {
                    if (!e.AttestStateFromReference.IsLoaded)
                    {
                        e.AttestStateFromReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("AttestTransition.cs e.AttestStateFromReference");
                    }
                    if (!e.AttestStateToReference.IsLoaded)
                    {
                        e.AttestStateToReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("AttestTransition.cs e.AttestStateToReference");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            return new AttestTransitionGridDTO()
            {
                AttestTransitionId = e.AttestTransitionId,
                Name = e.Name,
                AttestStateFrom = e.AttestStateFrom?.Name ?? string.Empty,
                EntityName = e.AttestStateFrom?.EntityName ?? string.Empty,
                AttestStateTo = e.AttestStateTo?.Name ?? string.Empty,
            };
        }

        public static List<AttestStateDTO> GetAttestStatesTo(this List<AttestTransition> l, params int[] excludeIds)
        {
            List<AttestStateDTO> attestStates = new List<AttestStateDTO>();
            foreach (var attestTransition in l)
            {
                if (attestTransition.AttestStateTo == null)
                    continue;
                if (attestStates.Any(i => i.AttestStateId == attestTransition.AttestStateTo.AttestStateId))
                    continue;
                if (excludeIds.Where(i => i != 0).Contains(attestTransition.AttestStateTo.AttestStateId))
                    continue;

                attestStates.Add(attestTransition.AttestStateTo.ToDTO());
            }
            return attestStates.OrderBy(i => i.Sort).ToList();
        }

        public static IEnumerable<AttestTransitionDTO> ToDTOs(this IEnumerable<AttestTransition> l, bool includeAttestStates)
        {
            var dtos = new List<AttestTransitionDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeAttestStates));
                }
            }
            return dtos;
        }

        public static IEnumerable<AttestTransitionGridDTO> ToGridDTOs(this IEnumerable<AttestTransition> l)
        {
            var dtos = new List<AttestTransitionGridDTO>();
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
