using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;

namespace SoftOne.Soe.Data
{
    public partial class EventHistory : ICreatedModified, IState
    {
        public string TypeName { get; set; }
        public string EntityName { get; set; }
        public string RecordName { get; set; }
    }

    public static partial class EntityExtensions
    {
        #region EventHistory

        public static EventHistoryDTO ToDTO(this EventHistory e)
        {
            if (e == null)
                return null;

            return new EventHistoryDTO()
            {
                EventHistoryId = e.EventHistoryId,
                Type = (TermGroup_EventHistoryType)e.Type,
                Entity = (SoeEntityType)e.Entity,
                RecordId = e.RecordId,
                BatchId = e.BatchId,
                UserId = e.UserId,
                StringValue = e.StrData,
                IntegerValue = e.IntData,
                DecimalValue = e.DecimalData,
                BooleanValue = e.BoolData,
                DateValue = e.DateData,
                TypeName = e.TypeName,
                EntityName = e.EntityName,
                RecordName = e.RecordName,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };
        }

        public static IEnumerable<EventHistoryDTO> ToDTOs(this IEnumerable<EventHistory> l)
        {
            List<EventHistoryDTO> dtos = new List<EventHistoryDTO>();
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
