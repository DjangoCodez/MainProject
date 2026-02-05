using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Data
{
    public partial class Images
    {

    }

    public static partial class EntityExtensions
    {
        #region Images

        public static ImagesDTO ToDTO(this Images e, bool useThumbnail, string connectedTypeName = null, bool canDelete = true)
        {
            if (e == null)
                return null;

            return new ImagesDTO()
            {
                ImageId = e.ImageId,
                FormatType = (ImageFormatType)e.FormatType,
                Description = e.Description,
                ConnectedTypeName = connectedTypeName.HasValue() ? connectedTypeName : String.Empty,
                Image = useThumbnail ? e.Thumbnail : e.Image,
                Type = (SoeEntityImageType)e.Type,
                FileName = e.Description, // Since it seems to contain the filename in most cases
                CanDelete = canDelete,
            };
        }

        public static IEnumerable<ImagesDTO> ToDTOs(this IEnumerable<Images> l, bool useThumbnail, string connectedTypeName = null, bool canDelete = true)
        {
            var dtos = new List<ImagesDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(useThumbnail, connectedTypeName, canDelete));
                }
            }
            return dtos;
        }

        #endregion
    }
}
