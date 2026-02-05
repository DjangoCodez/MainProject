using System;

namespace SoftOne.Soe.Common.DTO
{
    public class SubstituteShiftDTO
    {
        public int EmployeeId;
        public DateTime StartTime;
        public DateTime StopTime;
        public DateTime Date;
        public int TotalBreakMinutes;
        public int Duration
        {
            get { return (int)(StopTime - StartTime).TotalMinutes - this.TotalBreakMinutes; }
        }

        public string Link { get; set; }

        public bool IsMoved;
        public bool IsNew;
        public bool IsCopied;
        public bool IsExtraShift;
        public bool IsAssignedDueToAbsence;
        public string OriginEmployeeName;
        public string AbsenceName;
    }
}
