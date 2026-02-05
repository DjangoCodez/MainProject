using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class PayrollProductDistributionRuleHead : ICreatedModified, IState
    {

    }

    public partial class PayrollProductDistributionRule : ICreatedModified, IState
    {
        public bool IsValid(decimal value) => value <= Stop;
    }
    public static partial class EntityExtensions
    {

        public static PayrollProductDistributionRuleHeadGridDTO ToGridDTO(this PayrollProductDistributionRuleHead e)
        {
            if (e == null)
                return null;

            PayrollProductDistributionRuleHeadGridDTO dto = new PayrollProductDistributionRuleHeadGridDTO()
            {
                PayrollProductDistributionRuleHeadId = e.PayrollProductDistributionRuleHeadId,
                Name = e.Name,
                Description = e.Description,
             
            };

          
            return dto;
        }

        public static IEnumerable<PayrollProductDistributionRuleHeadGridDTO> ToGridDTOs(this IEnumerable<PayrollProductDistributionRuleHead> l)
        {
            var dtos = new List<PayrollProductDistributionRuleHeadGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        public static PayrollProductDistributionRuleHeadDTO ToDTO(this PayrollProductDistributionRuleHead e)
        {
            if (e == null)
                return null;

            PayrollProductDistributionRuleHeadDTO dto = new PayrollProductDistributionRuleHeadDTO()
            {
                PayrollProductDistributionRuleHeadId = e.PayrollProductDistributionRuleHeadId,
                ActorCompanyId = e.ActorCompanyId,
                Name = e.Name,
                Description = e.Description,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State

            };

            if (e.PayrollProductDistributionRule.Any())
                dto.Rules = (List<PayrollProductDistributionRuleDTO>)e.PayrollProductDistributionRule.Where(w=> w.State == (int)SoeEntityState.Active).ToDTOs();

            return dto;
        }

        public static IEnumerable<PayrollProductDistributionRuleHeadDTO> ToDTOs(this IEnumerable<PayrollProductDistributionRuleHead> l)
        {
            var dtos = new List<PayrollProductDistributionRuleHeadDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static PayrollProductDistributionRuleDTO ToDTO(this PayrollProductDistributionRule e)
        {
            if (e == null)
                return null;

            PayrollProductDistributionRuleDTO dto = new PayrollProductDistributionRuleDTO()
            {
                PayrollProductDistributionRuleId = e.PayrollProductDistributionRuleId,
                PayrollProductDistributionRuleHeadId = e.PayrollProductDistributionRuleId,
                ActorCompanyId = e.ActorCompanyId,
                PayrollProductId = e.PayrollProductId,
                Type = e.Type,
                Start = e.Start,
                Stop = e.Stop,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State

            };

            return dto;
        }

        public static IEnumerable<PayrollProductDistributionRuleDTO> ToDTOs(this IEnumerable<PayrollProductDistributionRule> l)
        {
            var dtos = new List<PayrollProductDistributionRuleDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }
    }

};
