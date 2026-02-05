using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Data.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class HelpGroup : ICreatedModified
    {

    }

    public static partial class EntityExtensions
    {
        #region HelpGroup

        public static HelpGroupTreeDTO ToHelpGroupTreeDTO(this HelpGroup e, bool loadHelpTexts = true, bool loadChildren = true)
        {
            if (!e.HelpGroupName.IsLoaded)
            {
                e.HelpGroupName.Load();
                DataProjectLogCollector.LogLoadedEntityInExtension("HelpGroup.cs e.HelpGroupName");
            }
            if (e.HelpGroupName.Count == 0)
                return null;
            if (!e.ParentReference.IsLoaded)
            {
                e.ParentReference.Load();
                DataProjectLogCollector.LogLoadedEntityInExtension("HelpGroup.cs e.ParentReference");
            }

            var dto = new HelpGroupTreeDTO()
            {
                HelpGroupId = e.HelpGroupId,
                HelpGroupName = e.HelpGroupName.First().Name,
                ParentHelpGroupId = e.ParentHelpGroupId,
            };

            if (loadChildren)
            {
                List<HelpGroupTreeDTO> children = new List<HelpGroupTreeDTO>();
                if (!e.Children.IsLoaded)
                {
                    e.Children.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("HelpGroup.cs e.Children");
                }

                foreach (var item in e.Children)
                {
                    children.Add(item.ToHelpGroupTreeDTO());
                }
                dto.Children = children;
            }

            if (loadHelpTexts)
            {
                List<HelpTextDTOBase> helpTexts = new List<HelpTextDTOBase>();
                if (!e.Help.IsLoaded)
                {
                    e.Help.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("HelpGroup.cs e.Help");
                }

                foreach (var item in e.Help)
                {
                    if (!item.HelpText.IsLoaded)
                    {
                        item.HelpText.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("HelpGroup.cs e.HelpText");
                    }

                    var helpText = item.HelpText.FirstOrDefault();
                    if (helpText != null)
                    {
                        helpTexts.Add(new HelpTextDTOBase()
                        {
                            Title = helpText.Title,
                            HelpTextId = helpText.HelpTextId,
                        });
                    }
                }
                dto.HelpTexts = helpTexts;
            }

            return dto;
        }

        public static List<HelpGroupTreeDTO> ToHelpGroupTreeDTOs(this IEnumerable<HelpGroup> l, bool loadHelpTexts = true, bool loadChildren = true)
        {
            return l?.Select(s => s.ToHelpGroupTreeDTO(loadHelpTexts, loadChildren)).Where(w => w != null).ToList() ?? new List<HelpGroupTreeDTO>();
        }

        #endregion
    }
}
