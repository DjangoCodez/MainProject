using SoftOne.Soe.Shared.DTO;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.TimeEngine
{
    #region Input

    public class EmployeeActiveScheduleImportInputDTO : TimeEngineInputDTO
    {
        public List<EmployeeActiveScheduleIO> EmployeeActiveSchedules { get; set; }
        public EmployeeActiveScheduleImportInputDTO(List<EmployeeActiveScheduleIO> employeeActiveSchedules)
        {
            this.EmployeeActiveSchedules = employeeActiveSchedules;
        }
    }
  
    #endregion

    #region Output

    public class EmployeeActiveScheduleImportOutputDTO : TimeEngineOutputDTO { }

    #endregion
}
