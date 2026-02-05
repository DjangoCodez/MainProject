using System;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Status.Models
{
    public class SoftOneStatusUpTimeItem 
    {
        public string StatusServiceGroupName { get; set; }
        public DateTime Date { get; set; }
        public decimal UpTimeOnDate { get; set; }
        public decimal TotalUpTimeOnDate { get; set; }
        public decimal WebUpTimeOnDate { get; set; }
        public decimal MobileUpTimeOnDate { get; set; }
    }
}
