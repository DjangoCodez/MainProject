using SoftOne.Soe.Common.Util;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    public class ValidateBreakChangeResult
    {
        public bool Success { get; set; }
        public SoeValidateBreakChangeError Error { get; set; }
        public string ErrorMessage { get; set; }
        public List<int> TimeCodeBreakIds { get; set; }

        public ValidateBreakChangeResult()
        {
            this.Success = true;
        }

        public ValidateBreakChangeResult(SoeValidateBreakChangeError error, string errorMessage, List<int> timeCodeBreakIds = null)
        {
            this.Error = error;
            this.ErrorMessage = errorMessage;
            this.TimeCodeBreakIds = timeCodeBreakIds;
        }
    }
}
