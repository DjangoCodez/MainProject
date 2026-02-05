using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Web.UI.WebControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;

namespace SoftOne.Soe.Web.Controls
{
    public class FeatureTreeView : SoeFeatureTreeView, IFormControl
    {
        public int Count
        {
            get
            {
                return this.Nodes.Count;
            }
        }

        public void Clear()
        {
            this.Nodes.Clear();
        }

        public void RenderTree(Collection<SoeFeatureTreeNode> featureTreeNodes, Permission permission)
        {
            this.Clear();

            foreach (SoeFeatureTreeNode featureTreeNode in featureTreeNodes)
            {
                SetApperance(featureTreeNode, permission, true);
                this.Nodes.Add(featureTreeNode);
            }
        }

        #region Layout

        public void SetApperance(SoeFeatureTreeNode featureTreeNode, Permission permission, bool recursive)
        {
            //Get name
            featureTreeNode.Text = ((PageBase)HttpContext.Current.Handler).TextService.GetText(Convert.ToInt32(featureTreeNode.FeatureName)) + " [" + featureTreeNode.FeatureId + "]";

            //Readonly tree
            if (permission == Permission.Readonly)
            {
                //ReadOnly permission
                if (featureTreeNode.Permission == (int)Permission.Readonly)
                {
                    featureTreeNode.Checked = true;
                    ExpandSelfAndAbove(featureTreeNode);
                }
                //Modify permission
                else if (featureTreeNode.Permission == (int)Permission.Modify)
                {
                    featureTreeNode.Text += " [" + ((PageBase)HttpContext.Current.Handler).GetText(1080, "Skrivbehörighet") + "]";
                    featureTreeNode.ImageUrl = "~/img/checkbox.png";
                    featureTreeNode.ShowCheckBox = false;
                    ExpandSelfAndAbove(featureTreeNode);
                }
            }
            //Modify tree
            else if (permission == Permission.Modify)
            {
                //ReadOnly permission
                if (featureTreeNode.Permission == (int)Permission.Modify)
                {
                    featureTreeNode.Checked = true;
                    ExpandSelfAndAbove(featureTreeNode);
                }
                //Modify permission
                else if (featureTreeNode.Permission == (int)Permission.Readonly)
                {
                    featureTreeNode.Text += " [" + Permission.Readonly.ToString() + "]";
                    ExpandSelfAndAbove(featureTreeNode);
                }
            }

            if (recursive)
            {
                foreach (SoeFeatureTreeNode childNode in featureTreeNode.ChildNodes)
                {
                    SetApperance(childNode, permission, true);
                }
            }
        }

        public void ExpandSelfAndAbove(SoeFeatureTreeNode node)
        {
            node.Expand();

            //Recursive
            if (node.Parent != null)
                ExpandSelfAndAbove((SoeFeatureTreeNode)node.Parent);
        }

        #endregion
    }

    public class CompanyRoleTreeView : SoeCompanyRoleTreeView, IFormControl
    {
        public int Count
        {
            get
            {
                return this.Nodes.Count;
            }
        }

        public void Clear()
        {
            this.Nodes.Clear();
        }

        public void RenderTree()
        {
            this.Clear();

            var lm = new LicenseManager(null);
            var licenses = lm.GetLicenses();
            foreach (var license in licenses)
            {
                RenderLicenseTreeNode(license, false, false);
            }
        }

        public void RenderTree(int? licenseId)
        {
            this.Clear();

            LicenseManager lm = new LicenseManager(null);

            if (licenseId.HasValue)
            {
                var license = lm.GetLicenseFromCache(licenseId.Value);
                RenderLicenseTreeNode(license, true, true);
            }
            else
            {
                var licenses = lm.GetLicenses(loadCompany: true, loadRoles: true);
                foreach (var license in licenses)
                {
                    RenderLicenseTreeNode(license, true, true);
                }
            }
        }

        private void RenderLicenseTreeNode(License license, bool expandTree, bool showRoleNode)
        {
            if (license == null)
                return;

            var licenseNode = new SoeCompanyRoleTreeNode()
            {
                Id = license.LicenseId,
                Name = license.Name,
                Type = SoeCompanyRoleTreeNodeType.License,
                Text = license.Name,
            };
            if (expandTree)
                licenseNode.Expand();

            var cm = new CompanyManager(null);

            List<Company> companies;
            if (license.Company.IsLoaded)
                companies = license.Company.Where(c => c.State == (int)SoeEntityState.Active).OrderBy(c => c.Name).ToList();
            else
                companies = cm.GetCompaniesByLicense(license.LicenseId);

            foreach (var company in companies)
            {
                RenderCompanyTreeNode(licenseNode, company, expandTree, showRoleNode);
            }

            this.Nodes.Add(licenseNode);
        }

        private void RenderCompanyTreeNode(SoeCompanyRoleTreeNode licenseNode, Company company, bool expandTree, bool showRoleNode)
        {
            if (company == null)
                return;

            var companyNode = new SoeCompanyRoleTreeNode()
            {
                Id = company.ActorCompanyId,
                Name = company.Name,
                Text = company.Name,
                Type = SoeCompanyRoleTreeNodeType.Company,
            };
            if (expandTree)
                companyNode.Expand();

            if (showRoleNode)
            {
                var rm = new RoleManager(null);

                List<Role> roles;
                if (company.Role.IsLoaded)
                {
                    roles = company.Role.Where(r => r.State == (int)SoeEntityState.Active).ToList();
                    rm.SetRoleNameTexts(roles);
                    roles = roles.OrderBy(r => r.Name).ToList();
                }                    
                else
                    roles = rm.GetRolesByCompany(company.ActorCompanyId);

                foreach (var role in roles)
                {
                    RenderRoleTreeNode(companyNode, role);
                }
            }
            licenseNode.ChildNodes.Add(companyNode);
        }

        private void RenderRoleTreeNode(SoeCompanyRoleTreeNode companyNode, Role role)
        {
            if (role == null)
                return;

            string name = role.TermId.HasValue && role.TermId.Value > 0 ? ((PageBase)HttpContext.Current.Handler).TextService.GetText(role.TermId.Value, (int)TermGroup.Role) : role.Name;

            var roleNode = new SoeCompanyRoleTreeNode()
            {
                Id = role.RoleId,
                Name = name,
                Text = name,
                Type = SoeCompanyRoleTreeNodeType.Role,
            };


            companyNode.ChildNodes.Add(roleNode);
        }
    }
}
