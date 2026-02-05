using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;

namespace SoftOne.Soe.Data
{
    public partial class ApiSetting : ICreatedModified, IState
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public bool IsModified(ApiSettingDTO dto)
        {
            if (dto == null)
                return false;

            return
                this.Value != dto.Value ||
                this.StartDate != dto.StartDate ||
                this.StopDate != dto.StopDate ||
                this.State != (int)dto.State;
        }
    }

    public static partial class EntityExtensions
    {
        #region ApiSetting

        public static ApiSettingDTO ToDTO(this ApiSetting e)
        {
            if (e == null)
                return null;

            return new ApiSettingDTO()
            {
                ApiSettingId = e.ApiSettingId,
                Type = (TermGroup_ApiSettingType)e.Type,
                Value = e.Value,
                StartDate = e.StartDate,
                StopDate = e.StopDate,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };
        }

        public static IEnumerable<ApiSettingDTO> ToDTOs(this IEnumerable<ApiSetting> l)
        {
            var dtos = new List<ApiSettingDTO>();

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
