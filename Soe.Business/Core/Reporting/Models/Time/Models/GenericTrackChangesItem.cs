using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time.Models
{
    public class GenericTrackChangesItem
    {
        public string ActionMethod { get; set; }
        public SoeEntityType Entity { get; set; }
        public string EntityText { get; set; }
        public string TopRecordName { get; set; }
        public string TopEntity1Text { get; set; }
        public string TopEntity2Text { get; set; }
        public string Column { get; set; }
        public string Action { get; set; }
        public string Role { get; set; }
        public Guid Batch { get; set; }
        public int BatchNbr { get; set; }
        public string FromValue { get; set; }
        public string ToValue { get; set; }
        public int RecordId { get; set; }
        public string RecordName { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
    }
}
