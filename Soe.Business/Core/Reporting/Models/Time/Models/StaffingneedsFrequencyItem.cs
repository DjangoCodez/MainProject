using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time.Models
{
    public class StaffingneedsFrequencyItem
    {
        public int StaffingNeedsFrequencyId { get; set; }
        public int ActorCompanyId { get; set; }
        public DateTime TimeFrom { get; set; }
        public DateTime TimeTo { get; set; }
        public int Minutes
        {
            get
            {
                return (int)TimeTo.Subtract(TimeFrom).TotalMinutes;
            }
        }
        public decimal AmountPerMinute
        {
            get
            {
                if (Minutes != 0)
                    return decimal.Divide(Amount, Minutes);
                return 0;
            }
        }

        public decimal CostPerMinute
        {
            get
            {
                if (Minutes != 0)
                    return decimal.Divide(Cost, Minutes);
                return 0;
            }
        }

        public decimal MinutesPerMinute
        {
            get
            {
                if (Minutes != 0)
                    return decimal.Divide(NbrOfMinutes, Minutes);
                return 0;
            }
        }

        public decimal ItemsPerMinute
        {
            get
            {
                if (Minutes != 0)
                    return decimal.Divide(NbrOfItems, Minutes);
                return 0;
            }
        }
        public decimal NbrOfItems { get; set; }
        public decimal NbrOfCustomers { get; set; }
        public int NbrOfMinutes { get; set; }
        public decimal Amount { get; set; }
        public decimal Cost { get; set; }

        public string ExternalCode { get; set; }
        public string ParentExternalCode { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string AccountParentNumber { get; set; }
        public string AccountParentName { get; set; }
        public FrequencyType FrequencyType { get; set; }
    }
}
