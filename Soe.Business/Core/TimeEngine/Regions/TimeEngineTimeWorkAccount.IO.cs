using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.TimeEngine
{
    public partial class TimeEngine : ManagerBase
    {
        #region Input

        public class TimeWorkAccountYearEmployeeInputDTO : TimeEngineInputDTO
        {
            public int TimeWorkAccountId { get; set; }
            public int TimeWorkAccountYearId { get; set; }
            public List<int> EmployeeIds { get; set; }
            public List<int> TimeWorkAccountYearEmployeeIds { get; set; }
            public TimeWorkAccountYearEmployeeInputDTO(int timeWorkAccountId, int timeWorkAccountYearId, List<int> timeWorkAccountYearEmployeeIds = null, List<int> employeeIds = null)
            {
                this.TimeWorkAccountId = timeWorkAccountId;
                this.TimeWorkAccountYearId = timeWorkAccountYearId;
                this.TimeWorkAccountYearEmployeeIds = timeWorkAccountYearEmployeeIds.ToNullIfEmpty();
                this.EmployeeIds = employeeIds.ToNullIfEmpty();
            }            
        }       
        public class TaskCalculateTimeWorkAccountYearEmployeeBasisInputDTO : TimeEngineInputDTO 
        {
            public List<int> EmployeeIds { get; set; }
            public DateTime DateFrom { get; set; }
            public DateTime DateTo { get; set; }
            public TaskCalculateTimeWorkAccountYearEmployeeBasisInputDTO(List<int> employeeIds, DateTime dateFrom, DateTime dateTo)
            {
                this.EmployeeIds = employeeIds;
                this.DateFrom = dateFrom;
                this.DateTo = dateTo;
            }
        }
        public class TimeWorkAccountGenerateOutcomeInputDTO : TimeEngineInputDTO
        {
            public int TimeWorkAccountId { get; set; }
            public int TimeWorkAccountYearId { get; set; }
            public bool OverrideChoosen { get; set; }
            public DateTime PaymentDate { get; set; }
            public List<int> TimeWorkAccountYearEmployeeIds { get; set; }            
            public TimeWorkAccountGenerateOutcomeInputDTO(int timeWorkAccountId, int timeWorkAccountYearId, bool overrideChoosen, DateTime paymentDate, List<int> timeWorkAccountYearEmployeeIds = null)
            {
                this.TimeWorkAccountId = timeWorkAccountId;
                this.TimeWorkAccountYearId = timeWorkAccountYearId;
                this.OverrideChoosen = overrideChoosen;
                this.PaymentDate = paymentDate;
                this.TimeWorkAccountYearEmployeeIds = timeWorkAccountYearEmployeeIds.ToNullIfEmpty();
            }
        }
        public class TimeWorkAccountFinalSalaryInputDTO : TimeEngineInputDTO
        {
            public int EmployeeId { get; set; }
            public DateTime Date { get; set; }
            public TimeWorkAccountFinalSalaryInputDTO(int employeeId, DateTime date) 
            { 
                this.EmployeeId = employeeId;
                this.Date = date;
            }
        }

        #endregion

        #region Output

        public class CalculateTimeWorkAccountYearEmployeeOutputDTO : TimeEngineOutputDTO 
        {
            public TimeWorkAccountYearEmployeeResultDTO FunctionResult { get; set; }
            public CalculateTimeWorkAccountYearEmployeeOutputDTO()
            {
                this.FunctionResult = new TimeWorkAccountYearEmployeeResultDTO();
            }
        }
        public class CalculateTimeWorkAccountYearEmployeeBasisOutputDTO : TimeEngineOutputDTO
        {
            public List<TimeWorkAccountYearEmployeeCalculation> Basis { get; set; }
            public CalculateTimeWorkAccountYearEmployeeBasisOutputDTO()
            {
                this.Basis = new List<TimeWorkAccountYearEmployeeCalculation>();
            }
        }
        public class TimeWorkAccountChoiceSendXEMailOutputDTO : TimeEngineOutputDTO
        {
            public TimeWorkAccountChoiceResultDTO FunctionResult { get; set; }
            public TimeWorkAccountChoiceSendXEMailOutputDTO()
            {
                this.FunctionResult = new TimeWorkAccountChoiceResultDTO();
            }
        }
        public class TimeWorkAccountTransactionOutputDTO : TimeEngineOutputDTO
        {
            public TimeWorkAccountGenerateOutcomeResultDTO FunctionResult { get; set; }
            public TimeWorkAccountTransactionOutputDTO()
            {
                this.FunctionResult = new TimeWorkAccountGenerateOutcomeResultDTO();
            }
        }

        #endregion
    }
}
