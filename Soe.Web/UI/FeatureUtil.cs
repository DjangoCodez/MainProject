using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Web.UI.WebControls;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;

namespace SoftOne.Soe.Web.UI
{
    public class FeatureUtil
    {
        #region Variables

        private Collection<SoeFeatureTreeNode> featureTreeView;
        private readonly FeatureManager fm;

        #endregion

        #region Ctor

        public FeatureUtil(ParameterObject parameterObject)
        {
            this.fm = new FeatureManager(parameterObject);
        }

        #endregion

        #region SoeFeatureTreeNode

        public Collection<SoeFeatureTreeNode> ExportModuleFeaturesFromDatabase(int sysFeatureId, Permission permission, SoeFeatureType featureType, int licenseId, int actorCompanyId, int roleId, int sysXEArticleId)
        {
            this.featureTreeView = new Collection<SoeFeatureTreeNode>();

            if (featureType == SoeFeatureType.None)
                return this.featureTreeView;

            SysFeatureDTO sysFeature = this.fm.GetSysFeature(sysFeatureId)?.ToDTO();
            if (sysFeature != null)
                AddRootFeature(sysFeature, permission, featureType, licenseId, actorCompanyId, roleId, sysXEArticleId);

            return this.featureTreeView;
        }

        public Collection<SoeFeatureTreeNode> ExportModuleFeaturesFromDatabase(int sysFeatureId)
        {
            this.featureTreeView = new Collection<SoeFeatureTreeNode>();

            SysFeatureDTO sysFeature = this.fm.GetSysFeature(sysFeatureId)?.ToDTO();
            if (sysFeature != null)
                AddRootFeature(sysFeature);

            return this.featureTreeView;
        }

        private SoeFeatureTreeNode AddChildrenFeature(SoeFeatureTreeNode soeFeatureTreeNodeParent, SysFeatureDTO sysFeature, Permission permission, SoeFeatureType featureType, int licenseId, int actorCompanyId, int roleId, int sysXEArticleId)
        {
            //Create child
            SoeFeatureTreeNode node = CreateSoeFeatureTreeNode(sysFeature, permission, featureType, licenseId, actorCompanyId, roleId, sysXEArticleId);

            //Add child
            soeFeatureTreeNodeParent.ChildNodes.Add(node);

            return node;
        }

        private SoeFeatureTreeNode AddChildrenFeature(SoeFeatureTreeNode soeFeatureTreeNodeParent, SysFeatureDTO sysFeature)
        {
            //Create child
            SoeFeatureTreeNode node = CreateSoeFeatureTreeNode(sysFeature);

            //Add child
            soeFeatureTreeNodeParent.ChildNodes.Add(node);

            return node;
        }

        private SoeFeatureTreeNode CreateSoeFeatureTreeNode(SysFeatureDTO sysFeature, Permission permission, SoeFeatureType featureType, int licenseId, int actorCompanyId, int roleId, int sysXEArticleId)
        {           
            SoeFeatureTreeNode node = new SoeFeatureTreeNode()
            {
                FeatureId = sysFeature.SysFeatureId,
                Text = Convert.ToString(sysFeature.SysTermId, CultureInfo.CurrentCulture),
                FeatureName = Convert.ToString(sysFeature.SysTermId, CultureInfo.CurrentCulture),

                //Disable click
                NavigateUrl = "#",
                SelectAction = TreeNodeSelectAction.None,
            };

            FeaturePermissionItem featurePermissionItem = this.fm.LoadFeaturePermissions(featureType, licenseId, actorCompanyId, roleId, sysXEArticleId)[sysFeature.SysFeatureId];

            switch (featureType)
            {
                case SoeFeatureType.License:
                    node.Permission = (int)featurePermissionItem.LicensePermission;
                    break;
                case SoeFeatureType.Company:
                    Permission companyPermission = featurePermissionItem.CompanyPermission;
                    node.Permission = (int)companyPermission;
                    node.ShowCheckBox = (featurePermissionItem.HasPermission(SoeFeatureType.License, permission));
                    break;
                case SoeFeatureType.Role:
                    Permission rolePermission = featurePermissionItem.RolePermission;
                    node.Permission = (int)rolePermission;
                    node.ShowCheckBox = (featurePermissionItem.HasPermission(SoeFeatureType.Company, permission));
                    break;
                case SoeFeatureType.SysXEArticle:
                    node.Permission = (int)featurePermissionItem.SysXEArticlePermission;
                    break;
            }

            return node;
        }

        private SoeFeatureTreeNode CreateSoeFeatureTreeNode(SysFeatureDTO sysFeature)
        {
            SoeFeatureTreeNode node = new SoeFeatureTreeNode()
            {
                FeatureId = sysFeature.SysFeatureId,
                Text = Convert.ToString(sysFeature.SysTermId, CultureInfo.CurrentCulture),
                FeatureName = Convert.ToString(sysFeature.SysTermId, CultureInfo.CurrentCulture),

                //Disable click
                NavigateUrl = "#",
                SelectAction = TreeNodeSelectAction.None,
            };

            node.ShowCheckBox = true;
            node.Permission = (int)Permission.None;

            return node;
        }

        private void AddRootFeature(SysFeatureDTO sysFeature, Permission permission, SoeFeatureType featureType, int licenseId, int actorCompanyId, int roleId, int sysXEArticleId)
        {
            //Create root
            SoeFeatureTreeNode root = CreateSoeFeatureTreeNode(sysFeature, permission, featureType, licenseId, actorCompanyId, roleId, sysXEArticleId);
            root.ExpandAll();
            root.Collapse();

            //Find childrens
            FindChildrenFeatures(root, sysFeature, permission, featureType, licenseId, actorCompanyId, roleId, sysXEArticleId);

            //Add root
            this.featureTreeView.Add(root);
        }

        private void AddRootFeature(SysFeatureDTO sysFeature)
        {
            //Create root
            SoeFeatureTreeNode root = CreateSoeFeatureTreeNode(sysFeature);
            root.ExpandAll();
            root.Collapse();

            //Find childrens
            FindChildrenFeatures(root, sysFeature);

            //Add root
            this.featureTreeView.Add(root);
        }

        private void FindChildrenFeatures(SoeFeatureTreeNode soeFeatureTreeNodeParent, SysFeatureDTO sysFeatureParent, Permission permission, SoeFeatureType featureType, int licenseId, int actorCompanyId, int roleId, int sysXEArticleId)
        {
            //Uses SysDbCache
            var sysFeatureChildrens = (from sf in SysDbCache.Instance.SysFeatures
                                       where sf.ParentFeatureId != null &&
                                       sf.ParentFeatureId == sysFeatureParent.SysFeatureId &&
                                       !sf.Inactive
                                       orderby sf.Order ascending
                                       select sf).ToList();

            foreach (var sysFeatureChild in sysFeatureChildrens)
            {
                //Add child
                SoeFeatureTreeNode soeFeatureTreeNodeChild = AddChildrenFeature(soeFeatureTreeNodeParent, sysFeatureChild, permission, featureType, licenseId, actorCompanyId, roleId, sysXEArticleId);

                //Find children
                FindChildrenFeatures(soeFeatureTreeNodeChild, sysFeatureChild, permission, featureType, licenseId, actorCompanyId, roleId, sysXEArticleId);
            }
        }

        private void FindChildrenFeatures(SoeFeatureTreeNode soeFeatureTreeNodeParent, SysFeatureDTO sysFeatureParent)
        {
            //Uses SysDbCache
            var sysFeatureChildrens = (from sf in SysDbCache.Instance.SysFeatures
                                       where sf.ParentFeatureId != null &&
                                       sf.ParentFeatureId == sysFeatureParent.SysFeatureId &&
                                       !sf.Inactive
                                       select sf).ToList();

            foreach (var sysFeatureChild in sysFeatureChildrens)
            {
                //Add child
                SoeFeatureTreeNode soeFeatureTreeNodeChild = AddChildrenFeature(soeFeatureTreeNodeParent, sysFeatureChild);

                //Find children
                FindChildrenFeatures(soeFeatureTreeNodeChild, sysFeatureChild);
            }
        }

        #endregion
    }
}