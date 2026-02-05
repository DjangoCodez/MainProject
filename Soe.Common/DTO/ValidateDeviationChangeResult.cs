using SoftOne.Soe.Common.Util;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    public class ValidateDeviationChangeResult
    {
        public bool Success { get; set; }
        public SoeValidateDeviationChangeResultCode ResultCode { get; set; }
        public string Message { get; set; }
        public List<TimeDeviationCauseGridDTO> TimeDeviationCauses { get; set; }
        public List<AttestEmployeeDayTimeBlockDTO> GeneratedTimeBlocks { get; set; }
        public List<AttestEmployeeDayTimeCodeTransactionDTO> GeneratedTimeCodeTransactions { get; set; }
        public List<AttestPayrollTransactionDTO> GeneratedTimePayrollTransactions { get; set; }
        public List<ApplyAbsenceDTO> ApplyAbsenceItems { get; set; }

        public ValidateDeviationChangeResult(SoeValidateDeviationChangeResultCode resultCode, string message = "", List<TimeDeviationCauseGridDTO> timeDeviationCauses = null)
        {
            this.Success = (int)resultCode >= 1 && (int)resultCode < 100;
            this.ResultCode = resultCode;
            this.Message = message;
            this.TimeDeviationCauses = timeDeviationCauses;
        }
    }
}
