using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Business.Util
{
    /// <summary>
    /// Wrapper object combining salary document(s) with Action Result
    /// </summary>
    public class SalaryExportResult
    {
        #region Members

        public ActionResult Result { get; set; }
        public byte[] Salary { get; set; }
        public byte[] Schedule { get; set; }
        public byte[] SalaryAndSchedule { get; set; }
        public bool UsesSeparateFiles { get; set; }
        public SoeTimeSalaryExportFormat Format { get; set; }
        public string Extension { get; set; }

        #endregion

        #region Ctor

        public SalaryExportResult()
        {
            Result = new ActionResult(false);
        }

        #endregion
    }
}
