using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftOne.Soe.Shared.DTO
{
    /// <summary>
    /// Get or save ShiftType information
    /// </summary>
    public class ShiftTypeIODTO
    {
        /// <summary>
        /// Database Id from ShiftType
        /// </summary>
        public int ShiftTypeId { get; set; }
        /// <summary>
        /// Code for connected TimeScheduleTpe (nullable)
        /// </summary>
        public string TimeScheduleTypeCode { get; set; }
        /// <summary>
        /// Type of TimeScheduleBlockType 
        /// </summary>
        public TermGroup_TimeScheduleTemplateBlockType? TimeScheduleTemplateBlockType { get; set; }
        /// <summary>
        /// Name of ShiftType
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Desciption
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Hex color
        /// </summary>
        public string Color { get; set; }
        /// <summary>
        /// Short code for ShiftType
        /// </summary>
        public string NeedsCode { get; set; }
        /// <summary>
        /// External id, for external system information if needed
        /// </summary>
        public int? ExternalId { get; set; }
        /// <summary>
        /// External code, for external system information if needed
        /// </summary>
        public string ExternalCode { get; set; }
        /// <summary>
        /// Default length
        /// </summary>
        public int DefaultLength { get; set; }
        /// <summary>
        /// Limit startTime
        /// </summary>  
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// Limit stopTime
        /// </summary>
        public DateTime? StopTime { get; set; }
        /// <summary>
        /// Is a shiftype where money is beeing handled
        /// </summary>
        public bool HandlingMoney { get; set; }
        /// <summary>
        /// Hierarchical account number (nullable -> belongs to all)
        /// </summary>
        public string AccountNr { get; set; }
    }
}
