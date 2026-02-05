using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    public class AccountHierarchyTreeDTO
    {
        public AccountHierarchyTreeDTO()
        {
            AccountHierarchyInfos = new List<AccountHierarchyInfo>();
        }
        public List<AccountHierarchyInfo> AccountHierarchyInfos { get; set; }
    }

    public class AccountHierarchyInfo
    {
        public AccountHierarchyInfo()
        {
            EmployeeAccountHierarchyLinks = new List<EmployeeAccountHierarchyLink>();
        }
        /// <summary>
        /// AccountId is the identifier
        /// </summary>
        public int AccountId { get; set; }
        /// <summary>
        /// Nr is the number/code of the Account
        /// </summary>
        public string Nr { get; set; }
        /// <summary>
        /// Name of Account
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Internal number in order to sort dimensions (for example costplaces and projects)
        /// </summary>
        public int DimNr { get; set; }
        /// <summary>
        /// Number according the SIE-standardspecification
        /// </summary>
        public int SieNr { get; set; }
        /// <summary>
        /// AccountParentId to parent account
        /// </summary>
        public int? ParentId { get; set; }
        /// <summary>
        /// All employees or executives connected to the account 
        /// </summary>
        public List<EmployeeAccountHierarchyLink> EmployeeAccountHierarchyLinks { get; set; }

    }

    public class EmployeeAccountHierarchyLink
    {
        /// <summary>
        /// Type of affiliation
        /// Unknown = 0
        /// Employee = 1
        /// Executive = 2
        /// </summary>
        public AffiliationInfoLinkType AffiliationInfoLinkType { get; set; }
        /// <summary>
        /// EmployeeNr of employee or executive
        /// </summary>
        public string EmployeeNr { get; set; }
        /// <summary>
        /// SocialSec of employee or executive
        /// </summary>
        public string SocialSec { get; set; }
        /// <summary>
        /// Name of employee or executive
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// Type of affiliation
    /// Unknown = 0
    /// Employee = 1
    /// Executive = 2
    /// </summary>
    public enum AffiliationInfoLinkType
    {
        Unknown = 0,
        Employee = 1,
        Executive = 2,
    }
}
