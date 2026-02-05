using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Web.UI;
using SoftOne.Soe.Web.UI.WebControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Web.UI.WebControls;

namespace SoftOne.Soe.Web.soe
{
    public partial class FeaturePermissionTree : ControlBase
    {
        #region Properties

        private FeatureManager fm;
        private FeatureUtil fu;

        public Feature CurrentFeature = Feature.None;
        public Permission Permission { get; set; }
        public SoeFeatureType FeatureType { get; set; }
        public int LicenseId { get; set; }
        public int ActorCompanyId { get; set; }
        public int RoleId { get; set; }
        public string SubTitle;

        protected string Title;

        private int moduleSysFeatureId;
        private int sysXEArticleId;
        private bool modifyPermission = false;
        private List<SoeFeatureTreeNode> treeNodeCheckChanges = new List<SoeFeatureTreeNode>();

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            modifyPermission = PageBase.HasRolePermission(CurrentFeature, Permission.Modify);

            fu = new FeatureUtil(PageBase.ParameterObject);
            fm = new FeatureManager(PageBase.ParameterObject);

            SetupEventHandlers();
            SetupLanguage();
            SetupLayout();
            SetupArticles();
            SetupModules();
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                //PageBase.FeaturePermissionCache.Flush(PageBase.SoeLicense.LicenseId);
                PageBase.RemoveAllOutputCacheItems(Request.Url.AbsolutePath);
            }
        }

        #region Setup

        private void SetupEventHandlers()
        {
            //Setup event handlers
            SoeFormFooter.Save += new EventHandler(SoeFormFooter_ButtonClick);
            FeatureTree.TreeNodeCheckChanged += new TreeNodeEventHandler(FeatureTree_TreeNodeCheckChanged);
            Articles.SelectedIndexChanged += new EventHandler(Articles_SelectedIndexChanged);
            Modules.SelectedIndexChanged += new EventHandler(Modules_SelectedIndexChanged);
        }

        private void SetupLanguage()
        {
            //Setup title
            if (this.Permission == Permission.Readonly)
                Title = PageBase.GetText(1077, "Läsbehörighet") + " " + SubTitle;
            else if (this.Permission == Permission.Modify)
                Title = PageBase.GetText(1080, "Skrivbehörighet") + " " + SubTitle;

            //Setup Footer
            SoeFormPrefix.Title = Title;
            if (FeatureType == SoeFeatureType.SysXEArticle)
                SoeFormPrefix.TabType = SoeTabViewType.Admin;
            SoeFormFooter.ButtonSaveText = PageBase.GetText(1079, "Spara");
        }

        private void SetupLayout()
        {
            if (FeatureTree.Count == 0 || !modifyPermission)
                SoeFormFooter.SetButtonSaveVisbillity(false);
            else
                SoeFormFooter.SetButtonSaveVisbillity(true);
        }

        private void SetupArticles()
        {
            if (FeatureType != SoeFeatureType.SysXEArticle)
            {
                ArticlesRow.Visible = false;
                return;
            }

            ArticlesRow.Visible = true;

            //Setup Articles
            if (!IsPostBack)
            {
                //Add empty row
                Articles.Items.Add(new ListItem()
                {
                    Text = " ",
                    Value = "0",
                    Enabled = true,
                    Selected = true,
                });

                var sysXeArticles = fm.GetSysXEArticles();
                foreach (var sysXeArticle in sysXeArticles)
                {
                    Articles.Items.Add(new ListItem()
                    {
                        Text = sysXeArticle.Name,
                        Value = sysXeArticle.SysXEArticleId.ToString(),
                        Enabled = true,
                        Selected = false,
                    });
                }
            }

            if (Articles.SelectedIndex == 0)
                Modules.Enabled = false;
        }

        private void SetupModules()
        {
            //Setup Modules
            if (!IsPostBack)
            {
                //Add empty row
                Modules.Items.Add(new ListItem()
                {
                    Text = " ",
                    Value = "0",
                    Enabled = true,
                    Selected = true,
                });

                var sysFeatureRoots = fm.GetSysFeatureRoots();
                foreach (var sysFeature in sysFeatureRoots)
                {
                    Modules.Items.Add(new ListItem()
                    {
                        Text = PageBase.TextService.GetText(sysFeature.SysTermId),
                        Value = sysFeature.SysFeatureId.ToString(),
                        Enabled = true,
                        Selected = false,
                    });
                }
            }
        }

        #endregion

        #region Events

        private void SoeFormFooter_ButtonClick(object sender, EventArgs e)
        {
            foreach (var node in treeNodeCheckChanges)
            {
                if (this.FeatureType == SoeFeatureType.License)
                    CheckLicenseFeature(node);
                else if (this.FeatureType == SoeFeatureType.Company)
                    CheckCompanyFeature(node);
                else if (this.FeatureType == SoeFeatureType.Role)
                    CheckRoleFeature(node);
                else if (this.FeatureType == SoeFeatureType.SysXEArticle)
                    CheckSysXEArticleFeature(node);
            }
            treeNodeCheckChanges.Clear();

            if (this.FeatureType == SoeFeatureType.License)
                PageBase.ClearLicensePermissionsFromCache(this.LicenseId);
            if (this.FeatureType == SoeFeatureType.Company)
                PageBase.ClearCompanyPermissionsFromCache(this.LicenseId, this.ActorCompanyId);
            if (this.FeatureType == SoeFeatureType.Role)
                PageBase.ClearRolePermissionsFromCache(this.LicenseId, this.ActorCompanyId, this.RoleId);

            string message = PageBase.GetText(5271, "Behörigheter sparade");
            SoeFormFooter.SetMessage(message, SoeTabView.SoeMessageType.Success);
            SoeFormPrefix.SetStatus(SoeTabView.SoeMessageType.Success);
        }

        protected void Modules_SelectedIndexChanged(object sender, EventArgs e)
        {
            SoeFormFooter.ClearMessage();
            RefreshModule();

            if (moduleSysFeatureId > 0)
            {
                PopulateFeatureTree();
                SoeFormFooter.SetButtonSaveVisbillity(true);
                Title += " (" + Modules.SelectedItem.Text + ")";
            }
            else
            {
                FeatureTree.Clear();
                SoeFormFooter.SetButtonSaveVisbillity(false);
            }
        }

        protected void Articles_SelectedIndexChanged(object sender, EventArgs e)
        {
            SoeFormFooter.ClearMessage();
            RefreshArticle();

            if (sysXEArticleId > 0)
            {
                Modules.Enabled = true;
                RefreshModule();

                if (moduleSysFeatureId > 0)
                {
                    PopulateFeatureTree();
                    SoeFormFooter.SetButtonSaveVisbillity(true);
                    Title += " (" + Modules.SelectedItem.Text + ")";
                }
            }
            else
            {
                FeatureTree.Clear();
                Modules.Enabled = false;
            }
        }

        protected void FeatureTree_TreeNodeCheckChanged(object sender, TreeNodeEventArgs e)
        {
            SoeFeatureTreeNode node = (SoeFeatureTreeNode)e.Node;
            treeNodeCheckChanges.Add(node);
        }

        #endregion

        #region Tree

        private void PopulateFeatureTree()
        {
            FeatureTree.Clear();

            RefreshArticle();
            RefreshModule();

            if ((this.Permission == Permission.Readonly) || (this.Permission == Permission.Modify))
            {
                Collection<SoeFeatureTreeNode> treeNodes = fu.ExportModuleFeaturesFromDatabase(moduleSysFeatureId, Permission, FeatureType, LicenseId, ActorCompanyId, RoleId, sysXEArticleId);
                FeatureTree.RenderTree(treeNodes, this.Permission);
            }
        }

        #endregion

        #region Action-methods

        private void CheckLicenseFeature(SoeFeatureTreeNode node)
        {
            bool success = false;
            LicenseFeature licenseFeature = fm.GetLicenseFeature(LicenseId, node.FeatureId);

            if (node.Checked)
            {
                if (licenseFeature == null)
                {
                    //Add permission
                    licenseFeature = new LicenseFeature()
                    {
                        SysFeatureId = node.FeatureId,
                        SysPermissionId = (int)this.Permission
                    };

                    success = fm.AddLicensePermission(licenseFeature, LicenseId).Success;
                }
                else
                {
                    //Update permission
                    licenseFeature.SysPermissionId = (int)this.Permission;

                    success = fm.UpdateLicensePermission(licenseFeature).Success;
                }

                if (success)
                {
                    node.Permission = (int)this.Permission;
                    FeatureTree.SetApperance(node, this.Permission, false);
                    FeatureTree.ExpandSelfAndAbove(node);
                }
                else
                    node.Checked = false;
            }
            else
            {
                //Remove permission
                if (licenseFeature != null)
                    success = fm.DeleteLicensePermission(licenseFeature).Success;

                if (success)
                {
                    node.Permission = 0;
                    FeatureTree.SetApperance(node, this.Permission, false);
                }
                else
                    node.Checked = true;
            }
        }

        private void CheckCompanyFeature(SoeFeatureTreeNode node)
        {
            bool success = false;
            CompanyFeature companyFeature = fm.GetCompanyFeature(ActorCompanyId, node.FeatureId);

            if (node.Checked)
            {
                if (companyFeature == null)
                {
                    //Add permission
                    companyFeature = new CompanyFeature()
                    {
                        SysFeatureId = node.FeatureId,
                        SysPermissionId = (int)this.Permission
                    };

                    success = fm.AddCompanyPermission(companyFeature, ActorCompanyId).Success;
                }
                else
                {
                    //Update permission
                    companyFeature.SysPermissionId = (int)this.Permission;

                    success = fm.UpdateCompanyPermission(companyFeature).Success;
                }

                if (success)
                {
                    node.Permission = (int)this.Permission;
                    FeatureTree.SetApperance(node, this.Permission, false);
                    FeatureTree.ExpandSelfAndAbove(node);
                }
                else
                    node.Checked = false;
            }
            else
            {
                //Remove permission
                if (companyFeature != null)
                    success = fm.DeleteCompanyPermission(companyFeature).Success;

                if (success)
                {
                    node.Permission = 0;
                    FeatureTree.SetApperance(node, this.Permission, false);
                }
                else
                    node.Checked = true;
            }
        }

        private void CheckRoleFeature(SoeFeatureTreeNode node)
        {
            bool success = false;
            RoleFeature roleFeature = fm.GetRoleFeature(RoleId, node.FeatureId);

            if (node.Checked)
            {
                if (roleFeature == null)
                {
                    //Add permission
                    roleFeature = new RoleFeature()
                    {
                        SysFeatureId = node.FeatureId,
                        SysPermissionId = (int)this.Permission
                    };

                    success = fm.AddRolePermission(roleFeature, RoleId).Success;
                }
                else
                {
                    //Update permission
                    roleFeature.SysPermissionId = (int)this.Permission;

                    success = fm.UpdateRolePermission(roleFeature).Success;
                }

                if (success)
                {
                    node.Permission = (int)this.Permission;
                    FeatureTree.SetApperance(node, this.Permission, false);
                    FeatureTree.ExpandSelfAndAbove(node);
                }
                else
                    node.Checked = false;
            }
            else
            {
                //Remove permission
                if (roleFeature != null)
                    success = fm.DeleteRolePermission(roleFeature).Success;

                if (success)
                {
                    node.Permission = 0;
                    FeatureTree.SetApperance(node, this.Permission, false);
                }
                else
                    node.Checked = true;
            }
        }

        private void CheckSysXEArticleFeature(SoeFeatureTreeNode node)
        {
            RefreshArticle();

            bool success = false;
            SysXEArticleFeature sysXEArticleFeature = fm.GetSysXEArticleFeature(sysXEArticleId, (Feature)node.FeatureId);

            if (node.Checked)
            {
                if (sysXEArticleFeature == null)
                {
                    //Add permission
                    sysXEArticleFeature = new SysXEArticleFeature()
                    {
                        SysPermissionId = (int)this.Permission
                    };

                    success = fm.AddSysXEArticlePermission(sysXEArticleFeature, sysXEArticleId, node.FeatureId).Success;
                }
                else
                {
                    //Update permission
                    sysXEArticleFeature.SysPermissionId = (int)this.Permission;

                    success = fm.UpdateSysXEArticlePermission(sysXEArticleFeature).Success;
                }

                if (success)
                {
                    node.Permission = (int)this.Permission;
                    FeatureTree.SetApperance(node, this.Permission, false);
                    FeatureTree.ExpandSelfAndAbove(node);
                }
                else
                    node.Checked = false;
            }
            else
            {
                //Remove permission
                if (sysXEArticleFeature != null)
                    success = fm.DeleteSysXEArticlePermission(sysXEArticleFeature).Success;

                if (success)
                {
                    node.Permission = 0;
                    FeatureTree.SetApperance(node, this.Permission, false);
                }
                else
                    node.Checked = true;
            }
        }

        #endregion

        #region Help-methods

        private void RefreshModule()
        {
            Int32.TryParse(Modules.SelectedValue, out moduleSysFeatureId);
        }

        private void RefreshArticle()
        {
            if (FeatureType == SoeFeatureType.SysXEArticle)
                Int32.TryParse(Articles.SelectedValue, out sysXEArticleId);
        }

        #endregion

        #region Public methods

        /// Add a link to the Form.
        /// </summary>
        /// <param name="text">The text for the button</param>
        /// <param name="href">The link for the button to link to</param>
        /// <param name="feature">The Feature to verify permission for</param>
        /// <param name="permission">The minimum Permission to be allowed</param>
        public void AddLink(string label, string href, Feature feature, Permission permission)
        {
            AddLink(label, href, feature, permission, false);
        }

        /// <summary>
        /// Add a link to the Form.
        /// </summary>
        /// <param name="text">The text for the button</param>
        /// <param name="href">The link for the button to link to</param>
        /// <param name="feature">The Feature to verify permission for</param>
        /// <param name="permission">The minimum Permission to be allowed</param>
        /// <param name="permission">True if the link should open as a modal window/param>
        public void AddLink(string label, string href, Feature feature, Permission permission, bool popLink)
        {
            SoeFormFooter.AddLink(label, href, feature, permission, popLink);
        }

        #endregion
    }
}
