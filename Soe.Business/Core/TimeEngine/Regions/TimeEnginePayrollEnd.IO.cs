using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.TimeEngine
{
    #region Input

    public class SaveVacationYearEndInputDTO : TimeEngineInputDTO
    {
        public TermGroup_VacationYearEndHeadContentType ContentType { get; set; }
        public List<int> ContentTypeIds { get; set; }
        public DateTime Date { get; set; }
        public SaveVacationYearEndInputDTO(TermGroup_VacationYearEndHeadContentType contentType, List<int> contentTypeIds, DateTime date)
        {
            this.ContentType = contentType;
            this.ContentTypeIds = contentTypeIds;
            this.Date = date;
        }
        public override int? GetIntervalCount()
        {
            return ContentTypeIds?.Count();
        }
    }
    public class DeleteVacationYearEndInputDTO : TimeEngineInputDTO
    {
        public int VacationYearEndHeadId { get; set; }
        public DeleteVacationYearEndInputDTO(int vacationYearEndHeadId)
        {
            this.VacationYearEndHeadId = vacationYearEndHeadId;
        }
    }
    public class CreateFinalSalaryInputDTO : TimeEngineInputDTO
    {
        public List<int> EmployeeIds { get; set; }
        public int? TimePeriodId { get; set; }
        public bool CreateReport { get; set; }
        public CreateFinalSalaryInputDTO(int employeeId, int? timePeriodId, bool createReport)
        {
            this.EmployeeIds = employeeId.ObjToList();
            this.TimePeriodId = timePeriodId;
            this.CreateReport = createReport;
        }
        public CreateFinalSalaryInputDTO(List<int> employeeIds, int? timePeriodId, bool createReport)
        {
            this.EmployeeIds = employeeIds ?? new List<int>();
            this.TimePeriodId = timePeriodId;
            this.CreateReport = createReport;
        }
        public override int? GetIdCount()
        {
            return this.EmployeeIds?.Count ?? 0;
        }
        public override int? GetIntervalCount()
        {
            return 1;
        }
    }
    public class DeleteFinalSalaryInputDTO : TimeEngineInputDTO
    {
        public List<int> EmployeeIds { get; set; }
        public int TimePeriodId { get; set; }
        public DeleteFinalSalaryInputDTO(int employeeId, int timePeriodId)
        {
            this.EmployeeIds = employeeId.ObjToList();
            this.TimePeriodId = timePeriodId;
        }
        public DeleteFinalSalaryInputDTO(List<int> employeeIds, int timePeriodId)
        {
            this.EmployeeIds = employeeIds;
            this.TimePeriodId = timePeriodId;
        }
        public override int? GetIdCount()
        {
            return this.EmployeeIds?.Count ?? 0;
        }
        public override int? GetIntervalCount()
        {
            return 1;
        }
    }
    public class ClearPayrollCalculationInputDTO : TimeEngineInputDTO
    {
        public int EmployeeId { get; set; }
        public int TimePeriodId { get; set; }
        public ClearPayrollCalculationInputDTO(int employeeId, int timePeriodId)
        {
            this.EmployeeId = employeeId;
            this.TimePeriodId = timePeriodId;
        }
    }
    public class ValidateVacationYearEndInputDTO : TimeEngineInputDTO
    {
        public DateTime Date { get; set; }
        public List<int> VacationGroupIds { get; set; }
        public List<int> EmployeeIds { get; set; }
        public ValidateVacationYearEndInputDTO(DateTime date, List<int> vacationGroupIds, List<int> employeeIds)
        {
            this.Date = date;
            this.VacationGroupIds = vacationGroupIds;
            this.EmployeeIds = employeeIds;
        }
        public override int? GetIdCount()
        {
            return VacationGroupIds?.Count ?? EmployeeIds?.Count ?? 0;
        }
        public override int? GetIntervalCount()
        {
            return 1;
        }
    }

    #endregion

    #region Output

    public class SaveVacationYearEndOutputDTO : TimeEngineOutputDTO
    {
        public VacationYearEndResultDTO Details { get; set; }
        public new ActionResult Result
        {
            get
            {
                return base.Result;
            }
            set
            {
                if(Details != null)
                    Details.Result = value;

                base.Result = value;
            }
        }

        public SaveVacationYearEndOutputDTO()
        {
            Details = new VacationYearEndResultDTO
            {
                Result = new ActionResult(true)
            };
        }
    }
    public class DeleteVacationYearEndOutputDTO : TimeEngineOutputDTO { }
    public class CreateFinalSalaryOutputDTO : TimeEngineOutputDTO { }
    public class DeleteFinalSalaryOutputDTO : TimeEngineOutputDTO { }
    public class DeleteFinalSalariesOutputDTO : TimeEngineOutputDTO { }
    public class ValidateVacationYearEndOutputDTO : TimeEngineOutputDTO { }

    #endregion
}
