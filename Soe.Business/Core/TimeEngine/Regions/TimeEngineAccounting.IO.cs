using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.TimeEngine
{
    #region Input

    public class RecalculateAccountingFromPayrollInputDTO : TimeEngineInputDTO
    {
        public List<int> EmployeeIds { get; set; }
        public int TimePeriodId { get; set; }
        public RecalculateAccountingFromPayrollInputDTO(List<int> employeeIds, int timePeriodId)
        {
            this.EmployeeIds = employeeIds;
            this.TimePeriodId = timePeriodId;
        }
        public override int? GetIdCount()
        {
            return this.EmployeeIds?.Count;
        }
        public override int? GetIntervalCount()
        {
            return 1;
        }
    }
    public class RecalculateAccountingInputDTO : TimeEngineInputDTO
    {
        public List<AttestEmployeeDaySmallDTO> Items { get; set; }
        public SoeRecalculateAccountingMode Mode { get; set; }
        public bool DoRecalculateFromShiftType => this.Mode == SoeRecalculateAccountingMode.FromShiftType;
        public bool DoRecalculateFromSchedule => this.Mode == SoeRecalculateAccountingMode.FromSchedule;
        public bool DoRecalculateFromTime => this.Mode == SoeRecalculateAccountingMode.FromTime;

        public List<int> EmployeeIds { get { return this.Items.GetEmployeeIds(); } }
        public string DateInterval { get { return this.Items.GetDateInterval(); } }
        public RecalculateAccountingInputDTO(List<AttestEmployeeDaySmallDTO> items, SoeRecalculateAccountingMode mode)
        {
            this.Items = items;
            this.Mode = mode;
        }
        public override int? GetIdCount()
        {
            return this.Items.GetNrOfEmployees();
        }
        public override int? GetIntervalCount()
        {
            return this.Items.GetNrOfDates();
        }

    }
    public class SaveAccountProvisionBaseInputDTO : TimeEngineInputDTO
    {
        public List<AccountProvisionBaseDTO> InputProvisions { get; set; }
        public SaveAccountProvisionBaseInputDTO(List<AccountProvisionBaseDTO> inputProvisions) : base()
        {
            this.InputProvisions = inputProvisions;
        }
    }
    public class LockUnlockAccountProvisionBaseInputDTO : TimeEngineInputDTO
    {
        public int TimePeriodId { get; set; }
        public LockUnlockAccountProvisionBaseInputDTO(int timePeriodId) : base()
        {
            this.TimePeriodId = timePeriodId;
        }
    }
    public class UpdateAccountProvisionTransactionsInputDTO : TimeEngineInputDTO
    {
        public List<AccountProvisionTransactionGridDTO> InputTransactions { get; set; }
        public UpdateAccountProvisionTransactionsInputDTO(List<AccountProvisionTransactionGridDTO> inputTransactions) : base()
        {
            this.InputTransactions = inputTransactions;
        }
    }

    #endregion

    #region Output

    public class RecalculateAccountingFromPayrollOutputDTO : TimeEngineOutputDTO { }
    public class RecalculateAccountingOutputDTO : TimeEngineOutputDTO { }
    public class LockUnlockAccountProvisionBaseOutputDTO : TimeEngineOutputDTO { }
    public class SaveAccountProvisionBaseOutputDTO : TimeEngineOutputDTO { }
    public class UpdateAccountProvisionTransactionOutputDTO : TimeEngineOutputDTO { }

    #endregion
}
