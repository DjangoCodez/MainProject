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

        public class CalculateTimeWorkReductionReconciliationYearEmployeeInputDTO : TimeEngineInputDTO
        {
            public int TimeWorkReductionReconciliationId { get; set; }
            public int TimeWorkReductionReconciliationYearId { get; set; }
            public List<int> EmployeeIds { get; set; }
            public List<int> TimeWorkReductionReconciliationEmployeeIds { get; set; }
            public CalculateTimeWorkReductionReconciliationYearEmployeeInputDTO(int timeWorkReductionReconciliationId, int timeWorkReductionReconciliationYearId, List<int> timeWorkReductionReconciliationEmployeeIds = null, List<int> employeeIds = null)
            {
                this.TimeWorkReductionReconciliationId = timeWorkReductionReconciliationId;
                this.TimeWorkReductionReconciliationYearId = timeWorkReductionReconciliationYearId;
                this.TimeWorkReductionReconciliationEmployeeIds = timeWorkReductionReconciliationEmployeeIds.ToEmptyIfNull();
                this.EmployeeIds = employeeIds.ToEmptyIfNull();
            }
        }

        public class TimeWorkReductionReconciliationYearEmployeeGenerateOutcomeInputDTO : TimeEngineInputDTO
        {
            public int TimeWorkReductionReconciliationId { get; set; }
            public int TimeWorkReductionReconciliationYearId { get; set; }
            public List<int> TimeWorkReductionReconciliationEmployeeIds { get; set; }
            public List<int> EmployeeIds { get; set; }
            public bool OverrideChoosen { get; set; }
            public DateTime? PaymentDate { get; set; }

            public TimeWorkReductionReconciliationYearEmployeeGenerateOutcomeInputDTO(int timeWorkReductionReconciliationId, int timeWorkReductionReconciliationYearId, DateTime? paymentDate, bool overrideChoosen, List<int> timeWorkReductionReconciliationEmployeeIds = null, List<int> employeeIds = null)
            {
                this.TimeWorkReductionReconciliationId = timeWorkReductionReconciliationId;
                this.TimeWorkReductionReconciliationYearId = timeWorkReductionReconciliationYearId;
                this.OverrideChoosen = overrideChoosen;
                this.PaymentDate = paymentDate;
                this.TimeWorkReductionReconciliationEmployeeIds = timeWorkReductionReconciliationEmployeeIds.ToEmptyIfNull();
                this.EmployeeIds = employeeIds.ToEmptyIfNull();
            }
        }

        public class TimeWorkReductionReconciliationYearEmployeeReverseTransactionsInputDTO : TimeEngineInputDTO
        {
            public int TimeWorkReductionReconciliationId { get; set; }
            public int TimeWorkReductionReconciliationYearId { get; set; }
            public List<int> TimeWorkReductionReconciliationEmployeeIds { get; set; }
            public List<int> EmployeeIds { get; set; }
            public bool OverrideChoosen { get; set; }
          
            public TimeWorkReductionReconciliationYearEmployeeReverseTransactionsInputDTO(int timeWorkReductionReconciliationId, int timeWorkReductionReconciliationYearId, bool overrideChoosen, List<int> timeWorkReductionReconciliationEmployeeIds = null, List<int> employeeIds = null)
            {
                this.TimeWorkReductionReconciliationId = timeWorkReductionReconciliationId;
                this.TimeWorkReductionReconciliationYearId = timeWorkReductionReconciliationYearId;
                this.OverrideChoosen = overrideChoosen;
                this.TimeWorkReductionReconciliationEmployeeIds = timeWorkReductionReconciliationEmployeeIds.ToEmptyIfNull();
                this.EmployeeIds = employeeIds.ToEmptyIfNull();
            }
        }
        #endregion

        #region Output

        public class CalculateTimeWorkReductionReconciliationYearEmployeeOutputDTO : TimeEngineOutputDTO
        {
            public TimeWorkReductionReconciliationYearEmployeeResultDTO FunctionResult { get; set; }
            public CalculateTimeWorkReductionReconciliationYearEmployeeOutputDTO()
            {
                this.FunctionResult = new TimeWorkReductionReconciliationYearEmployeeResultDTO();
            }
        }

        public class TimeWorkReductionReconciliationTransactionResultRowDTO : TimeEngineOutputDTO 
        {
            public TimeWorkReductionReconciliationYearEmployeeResultDTO FunctionResult { get; set; }
            public TimeWorkReductionReconciliationTransactionResultRowDTO()
            {
                this.FunctionResult = new TimeWorkReductionReconciliationYearEmployeeResultDTO();
            }
        }

        #endregion
    }
}
