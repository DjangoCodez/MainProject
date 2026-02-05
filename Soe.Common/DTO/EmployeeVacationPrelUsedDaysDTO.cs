namespace SoftOne.Soe.Common.DTO
{
    public class EmployeeVacationPrelUsedDaysDTO
    {
        public int EmployeeId { get; set; }
        public decimal Sum { get; set; }
        public bool IsHours { get; set; }
        public string Details { get; set; }
    }
}
