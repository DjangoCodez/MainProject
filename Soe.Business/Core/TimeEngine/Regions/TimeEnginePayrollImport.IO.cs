using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.TimeEngine
{
    #region Input

    public class PayrollImportInputDTO : TimeEngineInputDTO
    {
        public int PayrollImportHeadId { get; set; }
        public List<int> PayrollImportEmployeeIds { get; set; }
        public bool ReCalculatePayroll
        {
            get
            {
                return true;
            }
        }
        public PayrollImportInputDTO(int payrollImportHeadId, List<int> payrollImportEmployeeIds)
        {
            this.PayrollImportHeadId = payrollImportHeadId;
            this.PayrollImportEmployeeIds = payrollImportEmployeeIds;
        }
    }
    public class RollbackPayrollImportInputDTO : TimeEngineInputDTO
    {
        public int PayrollImportHeadId { get; set; }
        public List<int> PayrollImportEmployeeIds { get; set; }
        public bool IsRollbackFileContentMode { get; set; }
        public bool RollbackOutcomeForAllEmployees { get; set; }
        public bool RollbackFileContentForAllEmployees { get; set; }
        public bool ReCalculatePayroll
        {
            get
            {
                return true;
            }
        }
        public RollbackPayrollImportInputDTO(int payrollImportHeadId, List<int> payrollImportEmployeeIds, bool isRollbackFileContentMode, bool rollbackOutcomeForAllEmployees, bool rollbackFileContentForAllEmployees)
        {
            this.PayrollImportHeadId = payrollImportHeadId;
            this.PayrollImportEmployeeIds = payrollImportEmployeeIds;
            this.IsRollbackFileContentMode = isRollbackFileContentMode;
            this.RollbackFileContentForAllEmployees = rollbackFileContentForAllEmployees;
            this.RollbackOutcomeForAllEmployees = rollbackOutcomeForAllEmployees;
        }
    }
    public class ValidatePayrollImportInputDTO : TimeEngineInputDTO
    {
        public int PayrollImportHeadId { get; set; }
        public List<int> PayrollImportEmployeeIds { get; set; }
        public ValidatePayrollImportInputDTO(int payrollImportHeadId, List<int> payrollImportEmployeeIds)
        {
            this.PayrollImportHeadId = payrollImportHeadId;
            this.PayrollImportEmployeeIds = payrollImportEmployeeIds;
        }
    }

    #endregion

    #region Output

    public class PayrollImportOutputDTO : TimeEngineOutputDTO { }
    public class RollbackPayrollImportOutputDTO : TimeEngineOutputDTO { }
    public class ValidatePayrollImportOutputDTO : TimeEngineOutputDTO { }

    #endregion
}
