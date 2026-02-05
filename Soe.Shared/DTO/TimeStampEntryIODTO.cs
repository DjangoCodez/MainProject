using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftOne.Soe.Shared.DTO
{
    public class TimeStampEntryIODTO
    {
        /// <summary>
        /// EmployeeNr is the employee number of the employee that has made the time stamp entry.
        /// </summary>
        public string EmployeeNr { get; set; }
        /// <summary>
        /// EmployeeExternalCode is the external code of the employee that has made the time stamp entry.
        /// </summary>
        public string EmployeeExternalCode { get; set; }
        /// <summary>
        /// TimeStamp is the time stamp of the time stamp entry.
        /// </summary>  
        public DateTime TimeStamp { get; set; }
        /// <summary>
        /// In, Out or Unknown.
        /// </summary>  
        public TimeStampEntryType TimeStampEntryType { get; set; }
        /// <summary>
        /// Break is true if the time stamp entry is a break. False is not flagged as a break but could be.
        /// </summary>
        public bool IsBreak { get; set; }
        /// <summary>
        /// The Accounted AccountNr of the time stamp entry. Null if not accounted.
        /// </summary> 
        public string AccountNr { get; set; }
        /// <summary>
        /// The SieAccountDim of the AccountNr of the time stamp entry. Null if not accounted. (not needed on post)
        /// </summary>
        public int? SieAccountDim { get; set; }
        /// <summary>
        /// TimeTerminalId is the id of the time terminal that has made the time stamp entry. (mandatory on post)
        /// </summary>
        public int? TimeTerminalId { get; set; }
        /// <summary>
        /// Connected Accounts to the TimeStampEntry. (not mandatory on post)
        /// </summary>
        public List<TimeStampEntryIOAccountDTO> Accounts { get; set; } = new List<TimeStampEntryIOAccountDTO>();
        /// <summary>
        /// Connected TimeScheduleTypes to the TimeStampEntry. (not mandatory on post)
        /// </summary>
        public List<TimeStampEntryIOTimeScheduleTypeDTO> TimeScheduleTypes { get; set; } = new List<TimeStampEntryIOTimeScheduleTypeDTO>();
    }

    public class TimeStampEntryIOTimeScheduleTypeDTO
    {
        public string TimeScheduleTypeCode { get; set; }
    }

    public class TimeStampEntryIOAccountDTO
    {
        public string AccountNr { get; set; }
        public int? SieAccountDim { get; set; }
    }
}
