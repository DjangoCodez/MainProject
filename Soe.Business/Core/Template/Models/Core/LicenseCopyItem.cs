using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.Template.Models.Core
{
    public class LicenseCopyItem
    {
        public int LicenseId { get; set; }
        public string LicenseNr { get; set; }
        public string Name { get; set; }
        public string OrgNr { get; set; }
        public bool Support { get; set; }
        public int? NrOfCompanies { get; set; }
        public int MaxNrOfUsers { get; set; }
        public int MaxNrOfEmployees { get; set; }
        public int MaxNrOfMobileUsers { get; set; }
        public int? ConcurrentUsers { get; set; }
        public DateTime? TerminationDate { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public int State { get; set; }
        public bool AllowDuplicateUserLogin { get; set; }
        public string LegalName { get; set; }
        public bool IsAccountingOffice { get; set; }
        public int AccountingOfficeId { get; set; }
        public string AccountingOfficeName { get; set; }
        public int? SysServerId { get; set; }
       public List<CompanyCopyItem> CompanyCopyItems { get; set; } = new List<CompanyCopyItem>();
    }

    public class CompanyCopyItem
    {
        public int ActorCompanyId { get; set; }
        public string Name { get; set; }
        public TemplateCompanyDataItem CompanyData { get; set; }
    }
}
