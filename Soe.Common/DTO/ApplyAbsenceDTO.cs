using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    public class ApplyAbsenceDTO
    {
        public DateTime Date { get; set; }
        public int? NewProductId { get; set; }
        public int SysPayrollTypeLevel3 { get; set; }
        public List<int> TimePayrollTransactionIdsToRecalculate { get; set; }
        public bool IsVacation { get; set; }
        public int? TimeDeviationCauseId { get; set; }
    }
}
