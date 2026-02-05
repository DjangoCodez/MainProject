using SoftOne.Soe.Common.DTO;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.TimeEngine
{
    #region Input

    public class SavePayrollStartValuesInputDTO : TimeEngineInputDTO
    {
        public List<PayrollStartValueRowDTO> StartValueRows { get; set; }
        public int PayrollStartValueHeadId { get; set; }
        public SavePayrollStartValuesInputDTO(List<PayrollStartValueRowDTO> startValueRows, int payrollStartValueHeadId)
        {
            this.StartValueRows = startValueRows;
            this.PayrollStartValueHeadId = payrollStartValueHeadId;
        }
    }
    public class SaveTransactionsForPayrollStartValuesInputDTO : TimeEngineInputDTO
    {
        public int? EmployeeId { get; set; }
        public int PayrollStartValueHeadId { get; set; }
        public SaveTransactionsForPayrollStartValuesInputDTO(int? employeeId, int payrollStartValueHeadId)
        {
            this.EmployeeId = employeeId;
            this.PayrollStartValueHeadId = payrollStartValueHeadId;
        }
        public override int? GetIdCount()
        {
            return 1;
        }
    }
    public class DeletePayrollStartValueHeadInputDTO : TimeEngineInputDTO
    {
        public int PayrollStartValueHeadId { get; set; }
        public DeletePayrollStartValueHeadInputDTO(int payrollStartValueHeadId)
        {
            this.PayrollStartValueHeadId = payrollStartValueHeadId;
        }
    }
    public class DeleteTransactionsForPayrollStartValuesInputDTO : TimeEngineInputDTO
    {
        public int PayrollStartValueHeadId { get; set; }
        public int? EmployeeId { get; set; }
        public DeleteTransactionsForPayrollStartValuesInputDTO(int? employeeId, int payrollStartValueHeadId)
        {
            this.EmployeeId = employeeId;
            this.PayrollStartValueHeadId = payrollStartValueHeadId;
        }
        public override int? GetIdCount()
        {
            return 1;
        }
    }

    #endregion

    #region Output

    public class SavePayrollStartValuesOutputDTO : TimeEngineOutputDTO { }
    public class SaveTransactionsForPayrollStartValuesOutputDTO : TimeEngineOutputDTO { }
    public class DeleteTransactionsForPayrollStartValuesOutputDTO : TimeEngineOutputDTO { }
    public class DeletePayrollStartValueHeadOutputDTO : TimeEngineOutputDTO { }

    #endregion
}
