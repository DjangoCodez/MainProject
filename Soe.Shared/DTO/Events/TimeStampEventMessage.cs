using System;
using System.Collections.Generic;
using System.Text;

namespace SoftOne.Soe.Shared.DTO.Events
{
    public class TimeStampEventMessage
    {
        /// <summary>
        /// Employee Number
        /// </summary>
        public string ENr { get; set; }
        /// <summary>
        /// Employee ExternalCode
        /// </summary>
        public string ECode { get; set; }
        /// <summary>
        /// If not in that means out
        public bool In { get; set; }
        /// <summary>
        /// Local Time of TimeStamp
        /// </summary>
        public DateTime Time { get; set; }
        /// <summary>
        /// Account number or account external code if available
        /// </summary>
        public string Acc { get; set; }
    }
}
