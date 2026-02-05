using SoftOne.Communicator.Shared.DTO;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class HelpText : ICreatedModified, IState
    {

    }

    public static partial class EntityExtensions
    {
        #region HelpText

        public static HelpTextDTOSearch ToDTOSearch(this HelpText e, SearchLevel level)
        {
            return new HelpTextDTOSearch()
            {
                HelpTextId = e.HelpTextId,
                Description = e.Description,
                PlainText = e.PlainText,
                Title = e.Title,
                Level = level,
                TitleHit = level == SearchLevel.Title || level == SearchLevel.TitleWithLimit,
                ContentHit = level == SearchLevel.Content || level == SearchLevel.ContentWithLimit,
            };
        }

        public static IEnumerable<HelpTextDTOSearch> ToDTOsSearch(this IEnumerable<HelpText> l, SearchLevel level)
        {
            return l?.Select(s => s.ToDTOSearch(level)).ToList() ?? new List<HelpTextDTOSearch>();
        }

        public static HelpTextDTO ToDTO(this HelpText e, List<MessageAttachmentDTO> attachments, string roleName, List<HelpGroupName> helpGroups = null)
        {
            var dto = new HelpTextDTO()
            {
                HelpTextId = e.HelpTextId,
                SysLanguageId = e.SysLanguageId,
                Title = e.Title,
                Description = e.Description,
                Text = e.Text,
                PlainText = e.PlainText,
                ActorCompanyId = e.ActorCompanyId,
                CompanyName = e.Company?.Name ?? string.Empty,
                RoleId = e.RoleId,
                RoleName = roleName,
                Attachments = attachments,
                Help = new HelpDTO(), //Prevent nullreference. Add data to Help if needed
            };

            if (helpGroups != null)
            {
                //Adding all groups connected to help
                foreach (HelpGroupName helpGroup in helpGroups)
                {
                    foreach (Help helpInGroup in helpGroup.HelpGroup.Help)
                    {
                        if (helpInGroup.HelpId == e.HelpId)
                        {
                            dto.Help.HelpGroups.Add(new HelpGroupDTO
                            {
                                HelpGroupId = helpGroup.HelpGroup.HelpGroupId,
                                HelpGroupName = helpGroup.Name,
                                HelpGroupNameId = helpGroup.HelpGroupNameId,
                            });
                        }
                    }
                }
            }

            return dto;
        }

        public static HelpTextSmallDTO ToSmallDTO(this HelpText e)
        {
            HelpTextSmallDTO dto = new HelpTextSmallDTO()
            {
                Title = e.Title,
                Text = e.Text,
                PlainText = e.PlainText
            };

            return dto;
        }

        #endregion
    }
}
