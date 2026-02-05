using SoftOne.Soe.Common.DTO;

namespace SoftOne.Soe.Business.Core.TimeEngine
{
    #region Input

    public class TaskSaveExpenseInputDTO : TimeEngineInputDTO
    {
        public ExpenseRowDTO ExpenseRow { get; set; }
        public int? CustomerInvoiceId { get; set; }
        public bool ReturnEntity { get; set; }
        public TaskSaveExpenseInputDTO(ExpenseRowDTO expenseRow, int? customerInvoiceId = null, bool returnEntity = false)
        {
            this.ExpenseRow = expenseRow;
            this.CustomerInvoiceId = customerInvoiceId;
            this.ReturnEntity = returnEntity;
        }
    }
    public class DeleteExpenseInputDTO : TimeEngineInputDTO
    {
        public int ExpenseRowId { get; set; }
        public bool NoErrorIfExpenseRowNotFound { get; set; }
        public DeleteExpenseInputDTO(int expenseRowId, bool noErrorIfExpenseRowNotFound)
        {
            this.ExpenseRowId = expenseRowId;
            this.NoErrorIfExpenseRowNotFound = noErrorIfExpenseRowNotFound;
        }
    }

    #endregion

    #region Output

    public class TaskSaveExpenseOutputDTO : TimeEngineOutputDTO { }
    public class TaskSaveExpenseValidationOutputDTO : TimeEngineOutputDTO
    {
        public SaveExpenseValidationDTO ValidationOutput { get; set; }
    }

    #endregion
}
