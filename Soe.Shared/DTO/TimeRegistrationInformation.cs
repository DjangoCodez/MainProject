using System;
using System.Collections.Generic;
using System.Text;

namespace SoftOne.Soe.Shared.DTO
{
    /// <summary>
    /// Model for time registration that is incomplete
    /// </summary>
    public class TimeRegistrationInformation
    {
        /// <summary>
        /// EmployeeNr
        /// </summary>
        public string EmployeeNr { get; set; }
        /// <summary>
        /// Registration Code, can be mapped to Deviation cause or Payroll product
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Date connected to transaction. Mandatory
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// From date and time
        /// </summary>
        public DateTime? From { get; set; }
        /// <summary>
        /// To date and time
        /// </summary>
        public DateTime? To { get; set; }
        /// <summary>
        /// Comment
        /// </summary>
        public string Comment { get; set; }
        /// <summary>
        /// Quantity of time in minutes
        /// </summary>
        public int Minutes { get; set; }
        /// <summary>
        /// -- ReadOnly - Return value --
        /// Status of transaction
        /// Unprocessed = 0
        /// Error = 1
        /// Processed = 2
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// -- ReadOnly - Return value --
        /// ErrorMessage
        /// </summary>
        public string ErrorMessage { get; set; }
        /// <summary>
        /// -- ReadOnly - Return value --
        /// ErrorNumber
        /// EmployeeNotFound = 1
        /// </summary
        public int ErrorNumber { get; set; }
    }
}
