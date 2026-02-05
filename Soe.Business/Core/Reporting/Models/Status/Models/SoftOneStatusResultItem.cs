using System;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Status.Models
{
    public class SoftOneStatusResultItem
    {
        public DateTime Date { get; set; }
        public DateTime Created { get; set; }
        public decimal Percential90 { get; set; }
        public decimal Percential10 { get; set; }
        public decimal Max { get; set; }
        public decimal Min { get; set; }
        public decimal Median { get; set; }
        public DateTime From { get; }
        public decimal Average { get; set; }
        public int Succeded { get; set; }
        public int Hour { get; set; }
        public string ServiceTypeName { get; set; }
        public int Failed { get; set; }
        public DateTime To { get; }
    }
}
