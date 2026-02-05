using SoftOne.Soe.Common.DTO;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.TimeEngine
{
    #region Input

    public class SaveRetroactivePayrollInputDTO : TimeEngineInputDTO
    {
        public RetroactivePayrollDTO RetroactivePayrollInput { get; set; }
        public SaveRetroactivePayrollInputDTO(RetroactivePayrollDTO retroactivePayroll)
        {
            this.RetroactivePayrollInput = retroactivePayroll;
        }
    }
    public class SaveRetroactivePayrollOutcomeInputDTO : TimeEngineInputDTO
    {
        public int RetroactivePayrollId { get; set; }
        public int EmployeeId { get; set; }
        public List<RetroactivePayrollOutcomeDTO> RetroOutcomesInput { get; set; }
        public SaveRetroactivePayrollOutcomeInputDTO(int retroactivePayrollId, int employeeId, List<RetroactivePayrollOutcomeDTO> retroOutcomesInput)
        {
            this.RetroactivePayrollId = retroactivePayrollId;
            this.EmployeeId = employeeId;
            this.RetroOutcomesInput = retroOutcomesInput;
        }
        public override int? GetIdCount()
        {
            return 1;
        }
    }
    public class DeleteRetroactivePayrollInputDTO : TimeEngineInputDTO
    {
        public int RetroactivePayrollId { get; set; }
        public DeleteRetroactivePayrollInputDTO(int retroactivePayrollId)
        {
            this.RetroactivePayrollId = retroactivePayrollId;
        }
    }
    public class CalculateRetroactivePayrollInputDTO : TimeEngineInputDTO
    {
        public RetroactivePayrollDTO RetroactivePayrollInput { get; set; }
        public bool IncludeAlreadyCalculated { get; set; }
        public List<int> FilterEmployeeIds { get; set; }
        public override int? GetIdCount()
        {
            return FilterEmployeeIds?.Count();
        }
        public CalculateRetroactivePayrollInputDTO(RetroactivePayrollDTO retroactivePayrollInput, bool includeAlreadyCalculated, List<int> filterEmployeeIds)
        {
            this.RetroactivePayrollInput = retroactivePayrollInput;
            this.FilterEmployeeIds = filterEmployeeIds;
            this.IncludeAlreadyCalculated = includeAlreadyCalculated;
        }
    }
    public class DeleteRetroactivePayrollOutcomesInputDTO : TimeEngineInputDTO
    {
        public RetroactivePayrollDTO RetroactivePayrollInput { get; set; }
        public DeleteRetroactivePayrollOutcomesInputDTO(RetroactivePayrollDTO retroactivePayrollInput)
        {
            this.RetroactivePayrollInput = retroactivePayrollInput;
        }
    }
    public class CreateRetroactivePayrollTransactionsInputDTO : TimeEngineInputDTO
    {
        public RetroactivePayrollDTO RetroactivePayrollInput { get; set; }
        public List<int> FilterEmployeeIds { get; set; }
        public CreateRetroactivePayrollTransactionsInputDTO(RetroactivePayrollDTO retroactivePayrollInput, List<int> filterEmployeeIds)
        {
            this.RetroactivePayrollInput = retroactivePayrollInput;
            this.FilterEmployeeIds = filterEmployeeIds;
        }
        public override int? GetIdCount()
        {
            return FilterEmployeeIds?.Count();
        }
    }
    public class DeleteRetroactivePayrollTransactionsInputDTO : TimeEngineInputDTO
    {
        public RetroactivePayrollDTO RetroactivePayrollInput { get; set; }
        public List<int> FilterEmployeeIds { get; set; }
        public DeleteRetroactivePayrollTransactionsInputDTO(RetroactivePayrollDTO retroactivePayrollInput, List<int> filterEmployeeIds)
        {
            this.RetroactivePayrollInput = retroactivePayrollInput;
            this.FilterEmployeeIds = filterEmployeeIds;
        }
        public override int? GetIdCount()
        {
            return FilterEmployeeIds?.Count();
        }
    }

    #endregion

    #region Output

    public class SaveRetroactivePayrollOutputDTO : TimeEngineOutputDTO { }
    public class SaveRetroactivePayrollOutcomeOutputDTO : TimeEngineOutputDTO { }
    public class DeleteRetroactivePayrollOutputDTO : TimeEngineOutputDTO { }
    public class CalculateRetroactivePayrollOutputDTO : TimeEngineOutputDTO { }
    public class DeleteRetroactivePayrollOutcomesOutputDTO : TimeEngineOutputDTO { }
    public class CreateRetroactivePayrollTransactionsOutputDTO : TimeEngineOutputDTO { }
    public class DeleteRetroactivePayrollTransactionsOutputDTO : TimeEngineOutputDTO { }

    #endregion
}
