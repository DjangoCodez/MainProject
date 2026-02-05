using System;
using System.Collections.Generic;
using System.Text;

namespace SoftOne.Soe.Shared.DTO
{
    public class EmployeeDayInformation
    {
        /// <summary>
        /// EmployeeNr (Key)
        /// </summary>
        public string EmployeeNr { get; set; }
        /// <summary>
        /// EmployeeExternalCode (addtional/alternative key) This is sued if EmployeeNr is not available
        /// 
        public string EmployeeExternalCode { get; set; }
        /// <summary>
        /// Date of information
        /// </summary>
        public DateTime Date {  get; set; }
        /// <summary>
        /// Employee information rows
        /// </summary>
        public List<EmployeeInformationRow> InformationRows { get; set; } = new List<EmployeeInformationRow>();
    }

    public class EmployeeInformationRow
    {
        /// <summary>
        /// Optional Code to group information
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Mandatory value of information.
        /// </summary>
        public string Value { get; set; }
    }
}
