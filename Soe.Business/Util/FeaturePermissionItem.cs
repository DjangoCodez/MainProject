using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Business.Util
{
    public class FeaturePermissionItem
    {
        public FeaturePermissionItem()
        {
            LicensePermission = Permission.None;
            CompanyPermission = Permission.None;
            RolePermission = Permission.None;
            SysXEArticlePermission = Permission.None;
        }

        private Permission licensePermission;
        public Permission LicensePermission
        {
            get
            {
                return licensePermission;
            }
            set
            {
                licensePermission = value;
            }
        }

        private Permission companyPermission;
        public Permission CompanyPermission
        {
            get
            {
                if (LicensePermission == Permission.Modify)
                {
                    return companyPermission;
                }
                else if (LicensePermission == Permission.Readonly)
                {
                    if ((companyPermission == Permission.Readonly) || (companyPermission == Permission.Modify))
                        return Permission.Readonly;
                }

                return Permission.None;
            }
            set
            {
                companyPermission = value;
            }
        }

        private Permission rolePermission;
        public Permission RolePermission
        {
            get
            {
                if (CompanyPermission == Permission.Modify)
                {
                    return rolePermission;
                }
                else if (CompanyPermission == Permission.Readonly)
                {
                    if ((rolePermission == Permission.Readonly) || (rolePermission == Permission.Modify))
                        return Permission.Readonly;
                }

                return Permission.None;
            }
            set
            {
                rolePermission = value;
            }
        }

        private Permission sysXEArticlePermission;
        public Permission SysXEArticlePermission
        {
            get
            {
                return sysXEArticlePermission;
            }
            set
            {
                sysXEArticlePermission = value;
            }
        }

        public bool HasPermission(SoeFeatureType featureType, Permission permission)
        {
            bool hasPermission = false;

            switch (featureType)
            {
                case SoeFeatureType.License:
                    if (LicensePermission >= permission)
                        hasPermission = true;
                    break;
                case SoeFeatureType.Company:
                    if (CompanyPermission >= permission && LicensePermission >= permission)
                        hasPermission = true;
                    break;
                case SoeFeatureType.Role:
                    if (RolePermission >= permission && CompanyPermission >= permission && LicensePermission >= permission)
                        hasPermission = true;
                    break;
                case SoeFeatureType.SysXEArticle:
                    hasPermission = SysXEArticlePermission >= permission;
                    break;
            }

            return hasPermission;
        }

        public Permission GetPermission(SoeFeatureType featureType)
        {
            Permission permission = Permission.None;

            switch (featureType)
            {
                case SoeFeatureType.License:
                    permission = LicensePermission;
                    break;
                case SoeFeatureType.Company:
                    permission = CompanyPermission;
                    break;
                case SoeFeatureType.Role:
                    permission = RolePermission;
                    break;
                case SoeFeatureType.SysXEArticle:
                    permission = SysXEArticlePermission;
                    break;
            }

            return permission;
        }
    }
}
