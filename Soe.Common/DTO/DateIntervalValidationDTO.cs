using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Common.DTO
{
    public class DateIntervalValidationDTO
    {
        public readonly Guid UniqueKey;
        public DateTime? StartDate { get; set; }
        public DateTime StartDateValue
        {
            get
            {
                return this.StartDate.HasValue ? this.StartDate.Value : CalendarUtility.DATETIME_MINVALUE;
            }
        }
        public DateTime? StopDate { get; set; }
        public DateTime StopDateValue
        {
            get
            {
                return this.StopDate.HasValue ? this.StopDate.Value : CalendarUtility.DATETIME_MAXVALUE;
            }
        }
        public bool ValidateOverlaps { get; set; }

        public DateIntervalValidationDTO(DateTime? startDate, DateTime? stopDate, bool validateOverlaps = true)
        {
            this.StartDate = startDate;
            this.StopDate = stopDate;
            this.ValidateOverlaps = validateOverlaps;
            this.UniqueKey = Guid.NewGuid();
        }
    }

    public static class DateIntervalValidationExtensions
    {
        public static ActionResult Validate(this List<DateIntervalValidationDTO> validationItems, bool validateOverlaps, bool validateMustHaveCompleteDateRange)
        {
            if (validationItems.IsNullOrEmpty())
                return new ActionResult(true);

            if (validateMustHaveCompleteDateRange)
            {
                if (!validationItems.Any(i => !i.StartDate.HasValue))
                    return new ActionResult((int)ActionResultSave.DatesMustHaveEmptyStartDate);
                if (!validationItems.Any(i => !i.StopDate.HasValue))
                    return new ActionResult((int)ActionResultSave.DatesMustHaveEmptyStopDate);

                foreach (var outer in validationItems.Where(i => i.StopDate.HasValue))
                {
                    //If has stopdate, a connected tuple must have startdate the next day
                    if (!validationItems.Any(inner => inner.StartDate == outer.StopDate.Value.AddDays(1)))
                        return new ActionResult((int)ActionResultSave.DatesMustBeConnected);
                }
            }

            if (validationItems.Any(validationItem => validationItem.StartDateValue > validationItem.StopDateValue))
                return new ActionResult((int)ActionResultSave.DatesInvalid);

            if (validationItems.Count <= 1)
                return new ActionResult(true);

            if (validateOverlaps)
            {
                foreach (var outer in validationItems.Where(i => i.ValidateOverlaps).OrderBy(i => i.StartDateValue).ThenBy(i => i.StopDateValue))
                {
                    foreach (var inner in validationItems.Where(i => i.ValidateOverlaps && i.UniqueKey != outer.UniqueKey))
                    {
                        if (CalendarUtility.IsDatesOverlapping(outer.StartDateValue, outer.StopDateValue, inner.StartDateValue, inner.StopDateValue, true))
                            return new ActionResult((int)ActionResultSave.DatesOverlapping);
                    }
                }
            }

            return new ActionResult(true);
        }
    }
}
