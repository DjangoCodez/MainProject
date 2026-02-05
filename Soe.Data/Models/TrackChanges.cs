using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Data
{
    public partial class TrackChanges : ICreated
    {

    }

    public static partial class EntityExtensions
    {
        #region TrackChanges

        public static TrackChangesDTO ToDTO(this TrackChanges e)
        {
            if (e == null)
                return null;

            return new TrackChangesDTO()
            {
                TrackChangesId = e.TrackChangesId,
                ActorCompanyId = e.ActorCompanyId,
                Batch = new Guid(e.Batch),
                Entity = (SoeEntityType)e.Entity,
                RecordId = e.RecordId,
                ColumnName = e.ColumnName,
                ParentEntity = e.ParentEntity.HasValue ? (SoeEntityType)e.ParentEntity : SoeEntityType.None,
                ParentRecordId = e.ParentRecordId,
                Action = (TermGroup_TrackChangesAction)e.Action,
                DataType = (SettingDataType)e.DataType,
                FromValue = e.FromValue,
                ToValue = e.ToValue,
                FromValueName = e.FromValueName,
                ToValueName = e.ToValueName,
                Created = e.Created,
                CreatedBy = e.CreatedBy
            };
        }

        public static IEnumerable<TrackChangesDTO> ToDTOs(this IEnumerable<TrackChanges> l)
        {
            var dtos = new List<TrackChangesDTO>();
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
