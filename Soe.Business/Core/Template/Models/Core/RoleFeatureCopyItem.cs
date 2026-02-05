using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.Template.Models.Core
{
    // For Roles and Features
    public class RoleAndFeatureCopyItem
    {
        public int RoleId { get; set; }
        public RoleAndFeatureCopyItem()
        {
                RoleFeatures = new List<RoleFeatureCopyItem>(); 
        }
        public string RoleName { get; set; }
        public int ActorCompanyId { get; set; }
        public int Sort { get; set; }
        public int TermId { get; set; }
        public bool IsAdmin { get; set; }
        public string ExternalCodesString { get; set; }
        public List<RoleFeatureCopyItem> RoleFeatures { get; set; }

    }

    public class RoleFeatureCopyItem
    {
        public int SysFeatureId { get; set; }
        public int SysPermissionId { get; set; }
    }

    // For Companys and Features
    public class CompanyAndFeatureCopyItem
    {
        public CompanyAndFeatureCopyItem()
        {
            CompanyFeatures = new List<CompanyFeatureCopyItem>();
        }
        public string CompanyName { get; set; }
        public int ActorCompanyId { get; set; }
        public int Sort { get; set; }
        public int TermId { get; set; }
        public bool IsAdmin { get; set; }
        public string ExternalCodesString { get; set; }
        public List<CompanyFeatureCopyItem> CompanyFeatures { get; set; }

    }

    public class CompanyFeatureCopyItem
    {
        public int SysFeatureId { get; set; }
        public int SysPermissionId { get; set; }
    }
}
