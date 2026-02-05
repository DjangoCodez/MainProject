using System;
using System.Collections.Generic;
using System.Text;

namespace SoftOne.Soe.Shared.DTO
{
    public class EmployeeOrganisationInformation
    {
        /// <summary>
        /// Employee Position and information in organisation
        /// </summary>
        public EmployeeOrganisationInformation()
        {
            EmployeeOrganisationPositions = new List<EmployeeOrganisationPosition>();
            EmployeeOrganisationAccounts = new List<EmployeeOrganisationAccount>();
        }
        /// <summary>
        /// EmployeeNr (Key)
        /// </summary>
        public string EmployeeNr { get; set; }
        /// <summary>
        /// Employee First name
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        /// Employee Last name
        /// </summary>
        public string LastName { get; set; }
        /// <summary>
        /// Employee Email
        /// </summary
        public string Email { get; set; }
        /// <summary>
        /// Positions on employee
        /// </summary>
        public List<EmployeeOrganisationPosition> EmployeeOrganisationPositions { get; set; }
        /// <summary>
        /// Hierachical place in organisation
        /// </summary>
        public List<EmployeeOrganisationAccount> EmployeeOrganisationAccounts { get; set; }
    }

    public class EmployeeOrganisationPosition
    {
        /// <summary>
        /// Default position
        /// </summary>
        public bool Default { get; set; }
        /// <summary>
        /// Position Code
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Position Title
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Position Description
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// System information connected to position Code
        /// </summary>
        public string SysTitle { get; set; }
        /// <summary>
        /// System information connected to position Code
        /// </summary>
        public string SysDescription { get; set; }
    }

    public class EmployeeOrganisationAccount
    {
        /// <summary>
        /// Executive to Employee - First name
        /// </summary>
        public string ExecutiveFirstName { get; set; }
        /// <summary>
        /// Executive to Employee - last name
        /// </summary>
        public string ExecutiveLastName { get; set; }
        /// <summary>
        /// Executive to Employee - Position Code
        /// </summary>
        public string ExecutivePositionCode { get; set; }
        /// <summary>
        /// Executive to Employee - AttestRole name
        /// </summary>
        public string ExecutivePositionRole { get; set; }
        /// <summary>
        /// Executive to Employee - Position Title
        /// </summary>
        public string ExecutivePositionTitle { get; set; }
        /// <summary>
        /// Dimension number of found Executive Account (Hierachical level)
        /// </summary>
        public string ExecutiveAccountDimNr { get; set; }
        /// <summary>
        /// Name of found Executive Account Dimension (Hierachical level)
        /// </summary>
        public string ExecutiveAccountDim { get; set; }
        /// <summary>
        /// Name of found Executive Account 
        /// </summary>
        public string ExecutiveAccountName { get; set; }
        /// <summary>
        /// Number on found Executive Account 
        /// </summary>
        public string ExecutiveAccountNumber { get; set; }
        /// <summary>
        /// Account Dimension Number connected to Employee
        /// </summary>
        public string AccountDimNr { get; set; }
        /// <summary>
        /// Account Dimension Name connected to Employee
        /// </summary>
        public string AccountDim { get; set; }
        /// <summary>
        /// Account Name connected to Employee
        /// </summary>
        public string AccountName { get; set; }
        /// <summary>
        /// Account Number connected to Employee
        /// </summary>
        public string AccountNumber { get; set; }
        /// <summary>
        /// Default account
        /// </summary>
        public bool Default { get; set; }
        /// <summary>
        /// First date of connecition to Account connected to Employee. 1900-01-01 means no date i set
        /// </summary>
        public DateTime FromDate { get; set; }
        /// <summary>
        /// Last date of connecition to Account connected to Employee.
        /// </summary>
        public DateTime? ToDate { get; set; }
    }
}
