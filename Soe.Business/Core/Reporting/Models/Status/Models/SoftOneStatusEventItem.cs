using System;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Status.Models
{
    public class SoftOneStatusEventItem
    {
        public int Prio { get; set; }
        public string Url { get; set; }
        public Guid SoftOneStatusGuid { get; set; }
        public string StatusServiceTypeName { get; set; }
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
        public int Minutes { get; set; }
        public DateTime? LastMessageSent { get; set; }
        public string Message { get; set; }
        public string StatusEventTypeName { get; set; }
        public string JobDescriptionName { get; set; }
    }
}
