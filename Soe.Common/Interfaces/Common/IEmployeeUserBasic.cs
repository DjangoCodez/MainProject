using System;

namespace SoftOne.Soe.Common.Interfaces.Common
{
    public interface IEmployeeUserBasic
    {
        string CardNumber { get; set; }
        DateTime? Created { get; set; }
        int EmployeeId { get; set; }
        string EmployeeNr { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
        DateTime? Modified { get; set; }
        string Name { get; }
        string SocialSec { get; set; }
    }
}
