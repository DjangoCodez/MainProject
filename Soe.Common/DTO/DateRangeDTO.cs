using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System;
using TypeLite;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class DateRangeDTO
    {
        public DateTime Start { get; set; }
        public DateTime Stop { get; set; }
        public string Value { get; set; }
        public string Comment { get; set; }
        [TsIgnore]
        public int Days
        {
            get
            {
                return (int)this.Stop.Subtract(this.Start).TotalDays + 1;
            }
        }
        public int Minutes
        {
            get
            {
                return (int)this.Stop.Subtract(this.Start).TotalMinutes;
            }
        }

        public DateRangeDTO()
        {
        }

        public DateRangeDTO((DateTime start, DateTime stop) range) : this(range.start, range.stop)
        {
        }

        public DateRangeDTO(DateTime start, DateTime stop, string value = null, string comment = null)
        {
            this.Start = start;
            this.Stop = stop;
            this.Value = value.EmptyToNull();
            this.Comment = comment;
        }

        public int GetNumberOfDays()
        {
            return (int)this.Stop.Subtract(this.Start).TotalDays + 1;
        }

        public bool IsValid(DateTime date)
        {
            return this.Start <= date && this.Stop >= date;
        }

        public string GetInterval()
        {
            return $"{this.Start.ToString("yyyyMMdd")} - {this.Stop.ToString("yyyyMMdd")}";
        }
    }
}
