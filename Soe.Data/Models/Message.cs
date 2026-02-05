using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class Message : ICreatedModified, IState
    {
        public bool IsSent
        {
            get { return this.SentDate.HasValue; }
        }
    }

    public static partial class EntityExtensions
    {
        #region MessageGroup

        public static MessageGroupDTO ToDTO(this MessageGroup e, bool includeMembers)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (includeMembers && !e.IsAdded() && !e.MessageGroupMapping.IsLoaded)
                {
                    e.MessageGroupMapping.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("Message.cs e.MessageGroupMapping");
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            MessageGroupDTO dto = new MessageGroupDTO()
            {
                MessageGroupId = e.MessageGroupId,
                LicenseId = e.License?.LicenseId ?? 0,
                ActorCompanyId = e.ActorCompanyId,
                UserId = e.UserId,
                Name = e.Name,
                Description = e.Description,
                IsPublic = e.IsPublic,
                NoUserValidation = e.NoUserValidation
            };

            // Extensions
            if (includeMembers)
            {
                dto.GroupMembers = new List<MessageGroupMemberDTO>();
                foreach (MessageGroupMapping mapping in e.MessageGroupMapping.Where(m => m.State == (int)SoeEntityState.Active))
                {
                    dto.GroupMembers.Add(new MessageGroupMemberDTO()
                    {
                        MessageGroupId = e.MessageGroupId,
                        Entity = (SoeEntityType)mapping.Entity,
                        RecordId = mapping.RecordId,
                    });
                }
            }

            return dto;
        }

        public static MessageGroupGridDTO ToGridDTO(this MessageGroup e)
        {
            if (e == null)
                return null;

            return new MessageGroupGridDTO()
            {
                MessageGroupId = e.MessageGroupId,
                Name = e.Name,
                Description = e.Description,
                IsPublic = e.IsPublic
            };
        }

        public static List<MessageGroupGridDTO> ToGridDTOs(this List<MessageGroup> l)
        {
            var dtos = new List<MessageGroupGridDTO>();

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
