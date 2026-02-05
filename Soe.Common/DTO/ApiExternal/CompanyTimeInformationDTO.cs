using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO.ApiExternal
{
    public class CompanyTimeInformationDTO
    {
        /// <summary>
        /// Container of information. Message will contain errormessages if request has failed.
        /// </summary>
        public CompanyTimeInformationDTO()
        {
            CompanyTimeInformationRows = new List<CompanyTimeInformationRowDTO>();
        }
        /// <summary>
        /// List of CompanyTimeInformationRows
        /// </summary>
        public List<CompanyTimeInformationRowDTO> CompanyTimeInformationRows { get; set; }
        /// <summary>
        /// Information or error message
        /// </summary>
        public string Message { get; set; }
    }

    /// <summary>
    /// Request model in order to get a CompanyTimeInformationDTO back.
    /// </summary>
    public class CompanyTimeInformationRequest
    {
        public CompanyTimeInformationRequest()
        {
            Keys = new List<CompanyTimeInformationRequestKey>();
            CalculateCost = false;
        }

        /// <summary>
        /// List of all keys. Each key needs for be unique.
        /// </summary>
        public List<CompanyTimeInformationRequestKey> Keys { get; set; }

        /// <summary>
        /// Information will be fetch to yesterday from today minus DaysOfHistory
        /// </summary>
        public int DaysOfHistory { get; set; }
        /// <summary>
        /// Set this if cost needs to be calculated.
        /// </summary>
        public bool CalculateCost { get; set; }
    }

    /// <summary>
    /// Model for keys. Use according to instructions
    /// </summary>
    public class CompanyTimeInformationRequestKey
    {
        /// <summary>
        /// Key to identify company
        /// </summary>
        public string MainKey { get; set; }
        /// <summary>
        /// Key to control access
        /// </summary>
        public string ControlKey { get; set; }
    }

    public class CompanyTimeInformationRowDTO
    {
        /// <summary>
        /// Key used to identify company
        /// </summary>
        public string MainKey { get; set; }
        /// <summary>
        /// Name of Company
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The date on wich all data is aggregated
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// Net minutes is payed time during the day
        /// </summary>
        public int NetMinutes { get; set; }
        /// <summary>
        /// Gross minutes is net minutes plus time added for OB (inconvenient work hours) 50-70-100% extra.
        /// </summary>
        public int GrossMinutes { get; set; }
        /// <summary>
        /// Total cost of Gross minutes.
        /// </summary>
        public decimal Cost { get; set; }
    }
}
