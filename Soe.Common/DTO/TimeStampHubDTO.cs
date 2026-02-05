using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    public class TimeStampHubDTO
    {
        public int EmployeeId { get; set; }
        public int ActorCompanyId { get; set; }
        public int TimeTerminalId { get; set; }
        public TimeStampEntryType TimeStampEntryType { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeNr { get; set; }
        public DateTime Time { get; set; }
        public bool isBreak { get; set; }
        public string ImageUrl { get; set; }
        public List<int> CategoryIds { get; set; }
        public string Groupkey
        {
            get
            {
                string ids = string.Empty;

                if (this.CategoryIds != null)
                {
                    foreach (var categoryId in this.CategoryIds)
                    {
                        ids += ids.ToString() + "_";
                    }
                }

                return $"company{this.ActorCompanyId}categories{ids}";
            }
        }
    }
}
