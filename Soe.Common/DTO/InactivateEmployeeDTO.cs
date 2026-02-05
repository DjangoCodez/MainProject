
namespace SoftOne.Soe.Common.DTO
{
    public class InactivateEmployeeDTO
    {
        public int EmployeeId { get; set; }
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
