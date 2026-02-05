using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Web.UI;
using SoftOne.Soe.Web.UI.WebControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;

namespace SoftOne.Soe.Web.soe.manage.system.admin.features
{
    public partial class _default : PageBase
    {
        #region Variables

        private FeatureManager fm;
        private LicenseManager lm;
        private FeatureUtil fu;

        private int sourceLicenseId;
        private int sourceArticleId;
        private int destinationLicenseId;
        private int moduleSysFeatureId;
        private int dependencyFeatureId;
        private Permission destinationDependencyFeaturePermission;
        private Permission sourcePermission;
        private bool addNew = true;
        private bool promoteExisting = false;
        private bool degradeExisting = false;
        private bool deleteLeftOvers = false;
        private string copyTopUniqueId;
        private string copyBottomUniqueId;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Manage_System;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            fu = new FeatureUtil(ParameterObject);
            fm = new FeatureManager(ParameterObject);
            lm = new LicenseManager(ParameterObject);

            SoeGrid1.Title = GetText(5426, "Resultat");

            #endregion

            SetupEventHandlers();
            SetupLanguage();
            SetupLicenses();
            SetupArticles();
            SetupModules();
            SetupFeaturePermissions();

            ValidateModules();
        }

        protected override void OnPreRender(EventArgs e)
        {
            SetCopyVisibillity();
            SetFeatureTreeVisibillity();
            base.OnPreRender(e);
        }

        #region Setup

        private void SetupEventHandlers()
        {
            //Setup event handlers
            DefinePermissionManually.CheckedChanged += new EventHandler(DefinePermissionManually_CheckedChanged);
            SourceLicenses.SelectedIndexChanged += new EventHandler(LicenseSource_SelectedIndexChanged);
            SourceArticles.SelectedIndexChanged += new EventHandler(SourceArticles_SelectedIndexChanged);
            SourceLicensePermission.SelectedIndexChanged += new EventHandler(LicenseSourceCopyOptions_SelectedIndexChanged);
            Modules.SelectedIndexChanged += new EventHandler(Modules_SelectedIndexChanged);
            DestinationLicensesAll.CheckedChanged += new EventHandler(DestinationLicensesAll_CheckedChanged);
            DestinationLicenses.SelectedIndexChanged += new EventHandler(LicenseDestination_SelectedIndexChanged);
            DependancyFeature.SelectedIndexChanged += new EventHandler(DependencyFeature_SelectedIndexChanged);
            DependancyFeatureOnlyLicenses.CheckedChanged += new EventHandler(DependancyFeatureOnlyLicenses_CheckedChanged);
            DependancyFeatureOnlyLicensesAndCompanies.CheckedChanged += new EventHandler(DependancyFeatureOnlyLicensesAndCompanies_CheckedChanged);
            btnCopyBottom.Click += new EventHandler(Copy_Click);
            btnCopyTop.Click += new EventHandler(Copy_Click);

            //Setup button
            SetCopyUniqueId();
        }

        private void SetupLanguage()
        {
            SoeFormPrefix.Title = GetText(5152, "Kopiera behörigheter");
            DependancyFeatureOnlyLicenses.Text = GetText(110697, "Kopiera endast till licenser");
            DependancyFeatureOnlyLicensesAndCompanies.Text = GetText(110698, "Kopiera endast till licenser och företag");
            DestinationLicenseAddNew.Text = GetText(5255, "Lägg till nya");
            DestinationLicensePromoteExisting.Text = GetText(5256, "Uppgradera befintliga");
            DestinationLicenseDegradeExisting.Text = GetText(5257, "Degradera befintliga");
            DestinationLicenseDeleteLeftOvers.Text = GetText(5258, "Ta bort överblivna");
            DestinationLicensesAllLabel.Text = GetText(5305, "Alla licenser");
            DefinePermissionManuallyLabel.Text = GetText(5159, "Definera manuellt");
            SoeFormPrefix.TabType = SoeTabViewType.Admin;
            SetCopyText();
        }

        private void SetupLicenses()
        {
            SetupLicense(SourceLicenses);
            SetupLicense(DestinationLicenses);
        }

        private void SetupLicense(DropDownList ddl)
        {
            //Setup Licenses
            if (!IsPostBack)
            {
                //Add empty row
                ddl.Items.Add(new ListItem()
                {
                    Text = " ",
                    Value = "0",
                    Enabled = true,
                    Selected = true,
                });

                var licenses = lm.GetLicenses();
                foreach (var license in licenses.OrderBy(i => i.LicenseNr))
                {
                    ddl.Items.Add(new ListItem()
                    {
                        Text = license.LicenseNr + ". " + license.Name,
                        Value = license.LicenseId.ToString(),
                        Enabled = true,
                        Selected = false,
                    });
                }
            }
        }

        private void SetupArticles()
        {
            //Setup Licenses
            if (!IsPostBack)
            {
                //Add empty row
                SourceArticles.Items.Add(new ListItem()
                {
                    Text = " ",
                    Value = "0",
                    Enabled = true,
                    Selected = true,
                });

                var sysXeArticles = fm.GetSysXEArticles();
                foreach (var sysXeArticle in sysXeArticles.OrderBy(i => i.ArticleNr))
                {
                    SourceArticles.Items.Add(new ListItem()
                    {
                        Text = sysXeArticle.ArticleNr + ". " + sysXeArticle.Name,
                        Value = sysXeArticle.SysXEArticleId.ToString(),
                        Enabled = true,
                        Selected = false,
                    });
                }
            }
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
                    if (sysFeature.Inactive)
                        continue;

                    Modules.Items.Add(new ListItem()
                    {
                        Text = TextService.GetText(sysFeature.SysTermId),
                        Value = sysFeature.SysFeatureId.ToString(),
                        Enabled = true,
                        Selected = false,
                    });
                }
            }
        }

        private void SetupFeaturePermissions()
        {
            if (!IsPostBack)
            {
                //Add empty row
                DependancyFeature.Items.Add(new ListItem()
                {
                    Text = " ",
                    Value = "0",
                    Enabled = true,
                    Selected = true,
                });

                List<ListItem> items = new List<ListItem>();
                var sysFeatures = SysDbCache.Instance.SysFeatures.OrderBy(i => i.ToString()).ToList();
                foreach (var sysFeature in sysFeatures)
                {
                    Feature feature = (Feature)sysFeature.SysFeatureId;
                    if (feature.ToString() != sysFeature.SysFeatureId.ToString())
                    {
                        items.Add(new ListItem()
                        {
                            Text = feature.ToString() + " [" + (int)feature + "] - '" + TextService.GetText(sysFeature.SysTermId) + "' [" + sysFeature.SysTermId + "]",
                            Value = sysFeature.SysFeatureId.ToString(),
                            Enabled = true,
                            Selected = false,
                        });
                    }
                    else
                    {
                        //SysFeature is not mapped in Feature enum, i.e. not used (delete in db SOESys)
                    }
                }

                foreach (var item in items.OrderBy(i => i.Text))
                {
                    DependancyFeature.Items.Add(item);
                }
            }
        }

        #endregion

        #region Events

        private void DefinePermissionManually_CheckedChanged(object sender, EventArgs e)
        {
            SetSourceMode();
        }

        protected void LicenseSource_SelectedIndexChanged(object sender, EventArgs e)
        {
            SourceArticles.SelectedIndex = 0;

            RefreshSourceLicenses();
            if (sourceLicenseId > 0)
                PopulateFeatureTree();
            else
                FeatureTree.Clear();

            ValidateModules();
        }

        protected void SourceArticles_SelectedIndexChanged(object sender, EventArgs e)
        {
            SourceLicenses.SelectedIndex = 0;

            RefreshSourceArticles();
            if (sourceArticleId > 0)
                PopulateFeatureTree();
            else
                FeatureTree.Clear();

            ValidateModules();
        }

        protected void Modules_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshModules();
            RefreshSourcePermission();
            if (moduleSysFeatureId > 0 && sourcePermission != Permission.None)
            {
                PopulateFeatureTree();
                PopulateCompanyRoleTree();
            }
            else
            {
                FeatureTree.Clear();
                CompanyRoleTree.Clear();
                DestinationLicenses.Enabled = false;
                DestinationLicensesAll.Enabled = false;
                DependancyFeature.Enabled = false;
            }
        }

        protected void DestinationLicensesAll_CheckedChanged(object sender, EventArgs e)
        {
            DependancyFeatureOnlyLicenses.Visible = false;
            DependancyFeatureOnlyLicensesAndCompanies.Visible = false;
            DependencyFeaturePermission.Enabled = false;
            DependancyFeature.SelectedIndex = 0;
            DestinationLicenses.SelectedIndex = 0;

            if (DestinationLicensesAll.Checked)
            {
                DestinationLicenses.Enabled = false;
                DependancyFeature.Enabled = false;

                CompanyRoleTree.RenderTree(null);
            }
            else
            {
                DestinationLicenses.Enabled = true;
                DependancyFeature.Enabled = true;

                CompanyRoleTree.Clear();
            }
        }

        protected void LicenseDestination_SelectedIndexChanged(object sender, EventArgs e)
        {
            DestinationLicensesAll.Checked = false;
            DependancyFeature.SelectedIndex = 0;
            DependancyFeatureOnlyLicenses.Visible = false;
            DependancyFeatureOnlyLicensesAndCompanies.Visible = false;
            DependencyFeaturePermission.Enabled = false;

            RefreshDestinationLicenses();
            if (destinationLicenseId > 0)
                CompanyRoleTree.RenderTree(destinationLicenseId);
            else
                CompanyRoleTree.Clear();
        }

        protected void DependencyFeature_SelectedIndexChanged(object sender, EventArgs e)
        {
            CompanyRoleTree.Clear();

            RefreshDependencyFeature();
            if (dependencyFeatureId > 0)
            {
                DependancyFeatureOnlyLicenses.Visible = true;
                DependancyFeatureOnlyLicensesAndCompanies.Visible = true;
                DependencyFeaturePermission.Enabled = true;
                DestinationLicensesAll.Checked = false;
                DestinationLicenses.SelectedIndex = 0;
            }
        }

        protected void DependancyFeatureOnlyLicenses_CheckedChanged(object sender, EventArgs e)
        {
            DependancyFeatureOnlyLicensesAndCompanies.Checked = false;
        }

        protected void DependancyFeatureOnlyLicensesAndCompanies_CheckedChanged(object sender, EventArgs e)
        {
            DependancyFeatureOnlyLicenses.Checked = false;
        }

        private void LicenseSourceCopyOptions_SelectedIndexChanged(object sender, EventArgs e)
        {
            PopulateFeatureTree();
        }

        private void Copy_Click(object sender, EventArgs e)
        {
            CopyFeatures();
        }

        #endregion

        #region Tree

        private void PopulateFeatureTree()
        {
            FeatureTree.Clear();
            RefreshSourceLicenses();
            RefreshSourceArticles();
            RefreshModules();
            RefreshSourcePermission();
            if (moduleSysFeatureId > 0)
            {
                SoeFeatureType featureType = GetSoeFeatureType(false);
                if (featureType != SoeFeatureType.None)
                {
                    Collection<SoeFeatureTreeNode> treeNodes = fu.ExportModuleFeaturesFromDatabase(moduleSysFeatureId, sourcePermission, featureType, sourceLicenseId, 0, 0, sourceArticleId);
                    FeatureTree.RenderTree(treeNodes, sourcePermission);
                }
                else
                {
                    Collection<SoeFeatureTreeNode> treeNodes = fu.ExportModuleFeaturesFromDatabase(moduleSysFeatureId);
                    FeatureTree.RenderTree(treeNodes, sourcePermission);
                    FeatureTree.ExpandAll();
                }

                DestinationLicenses.Enabled = true;
                DestinationLicensesAll.Enabled = true;
                DependancyFeature.Enabled = true;
            }
        }

        private void CopyFeatures()
        {
            #region Init

            FeatureTree.CollapseAll();
            CompanyRoleTree.CollapseAll();

            RefreshDependencyFeature();
            if (CompanyRoleTree.CheckedNodes.Count == 0 && dependencyFeatureId == 0)
            {
                string message = GetText(5166, "Ange licens/företag/roll att kopiera till");
                SoeFormFooter.SetMessage(message, SoeTabView.SoeMessageType.Warning);
                SoeFormPrefix.SetStatus(SoeTabView.SoeMessageType.Warning);
                return;
            }

            RefreshCopyOptions();
            if (!addNew && !promoteExisting && !degradeExisting && !deleteLeftOvers)
            {
                string message = GetText(5167, "Ange inställningar för kopieringen");
                SoeFormFooter.SetMessage(message, SoeTabView.SoeMessageType.Warning);
                SoeFormPrefix.SetStatus(SoeTabView.SoeMessageType.Warning);
                return;
            }

            #endregion

            #region Prereq

            var destinationLicenses = new List<int>();
            var destinationCompanies = new List<int>();
            var destinationRoles = new List<int>();

            if (dependencyFeatureId > 0)
            {
                RefreshDependencyFeaturePermisson();

                destinationLicenses = fm.GetLicensesWithPermission(dependencyFeatureId, (int)destinationDependencyFeaturePermission);
                if (!DependancyFeatureOnlyLicenses.Checked)
                    destinationCompanies = fm.GetCompaniesWithPermission(dependencyFeatureId, (int)destinationDependencyFeaturePermission);
                if (!DependancyFeatureOnlyLicenses.Checked && !DependancyFeatureOnlyLicensesAndCompanies.Checked)
                    destinationRoles = fm.GetRolesWithPermission(dependencyFeatureId, (int)destinationDependencyFeaturePermission);
            }
            else
            {
                RefreshDestinationLicenses();
                if (!DestinationLicensesAll.Checked)
                    destinationLicenses.Add(destinationLicenseId);

                foreach (var treeNode in CompanyRoleTree.CheckedNodes)
                {
                    var node = treeNode as SoeCompanyRoleTreeNode;
                    if (node != null)
                    {
                        if (node.Type == SoeCompanyRoleTreeNodeType.License && DestinationLicensesAll.Checked)
                            destinationLicenses.Add(node.Id);
                        else if (node.Type == SoeCompanyRoleTreeNodeType.Company)
                            destinationCompanies.Add(node.Id);
                        else if (node.Type == SoeCompanyRoleTreeNodeType.Role)
                            destinationRoles.Add(node.Id);
                    }
                }
            }

            #endregion

            #region Copy

            ActionResult result;
            RefreshSourcePermission();
            RefreshDestinationLicenses();

            List<int> sysFeatures;
            if (DefinePermissionManually.Checked)
            {
                sysFeatures = GetSysFeaturesChecked();
                result = fm.CopyFeatures(sourcePermission, sysFeatures, destinationLicenses, destinationCompanies, destinationRoles, addNew, promoteExisting, degradeExisting, deleteLeftOvers);
            }
            else
            {
                sysFeatures = GetSysFeaturesAll();
                SoeFeatureType featureType = GetSoeFeatureType(false);
                RefreshSourceLicenses();
                RefreshSourceArticles();
                result = fm.CopyFeatures(featureType, sysFeatures, sourceLicenseId, sourceArticleId, destinationLicenses, destinationCompanies, destinationRoles, addNew, promoteExisting, degradeExisting, deleteLeftOvers);
            }

            if (result.Success)
            {
                string message = GetText(5168, "Kopiering klar");
                SoeFormFooter.SetMessage(message, SoeTabView.SoeMessageType.Success);
                SoeFormPrefix.SetStatus(SoeTabView.SoeMessageType.Success);

                List<FeatureTrackItem> tracks = result.Value as List<FeatureTrackItem>;
                if (tracks != null)
                {
                    SoeGrid1.DataSource = tracks;
                    SoeGrid1.DataBind();
                    SoeGrid1.Visible = true;
                }

                if (!destinationLicenses.IsNullOrEmpty())
                {
                    ClearLicensePermissionsFromCache(destinationLicenses.ToArray());
                }                    
            }
            else
            {
                string message = GetText(5169, "Kopiering misslyckades");
                SoeFormFooter.SetMessage(message, SoeTabView.SoeMessageType.Error);
                SoeFormPrefix.SetStatus(SoeTabView.SoeMessageType.Error);
            }

            #endregion
        }

        #endregion

        #region Help-methods

        private void PopulateCompanyRoleTree()
        {
            RefreshDestinationLicenses();
            if (destinationLicenseId > 0)
            {
                CompanyRoleTree.RenderTree(destinationLicenseId);
            }
        }

        private List<int> GetSysFeaturesChecked()
        {
            List<int> sysFeatures = new List<int>();
            foreach (var treeNode in FeatureTree.CheckedNodes)
            {
                SoeFeatureTreeNode node = treeNode as SoeFeatureTreeNode;
                if (node != null)
                {
                    sysFeatures.Add(node.FeatureId);
                }
            }
            return sysFeatures;
        }

        private List<int> GetSysFeaturesAll()
        {
            List<int> sysFeatures = new List<int>();
            foreach (var treeNode in FeatureTree.Nodes)
            {
                SoeFeatureTreeNode featureTreeNode = treeNode as SoeFeatureTreeNode;
                if (featureTreeNode != null)
                {
                    sysFeatures.Add(featureTreeNode.FeatureId);
                    FindChildrenSysFeatures(sysFeatures, featureTreeNode);
                }
            }
            return sysFeatures;
        }

        private void FindChildrenSysFeatures(List<int> sysFeatures, SoeFeatureTreeNode parentFeatureTreeNode)
        {
            foreach (var treeNode in parentFeatureTreeNode.ChildNodes)
            {
                SoeFeatureTreeNode childFeatureTreeNode = treeNode as SoeFeatureTreeNode;
                if (childFeatureTreeNode != null)
                {
                    sysFeatures.Add(childFeatureTreeNode.FeatureId);
                    FindChildrenSysFeatures(sysFeatures, childFeatureTreeNode);
                }
            }
        }

        private void RefreshSourceLicenses()
        {
            sourceLicenseId = 0;
            if (!DefinePermissionManually.Checked)
                Int32.TryParse(SourceLicenses.SelectedValue, out sourceLicenseId);
        }

        private void RefreshSourceArticles()
        {
            sourceArticleId = 0;
            if (!DefinePermissionManually.Checked)
                Int32.TryParse(SourceArticles.SelectedValue, out sourceArticleId);
        }

        private void RefreshDestinationLicenses()
        {
            Int32.TryParse(DestinationLicenses.SelectedValue, out destinationLicenseId);
        }

        private void RefreshModules()
        {
            Int32.TryParse(Modules.SelectedValue, out moduleSysFeatureId);
        }

        private void RefreshDependencyFeature()
        {
            Int32.TryParse(DependancyFeature.SelectedValue, out dependencyFeatureId);
        }

        private void RefreshDependencyFeaturePermisson()
        {
            if (DependencyFeaturePermission.SelectedValue == "ReadOnly")
                destinationDependencyFeaturePermission = Permission.Readonly;
            else if (DependencyFeaturePermission.SelectedValue == "Modify")
                destinationDependencyFeaturePermission = Permission.Modify;
            else
                destinationDependencyFeaturePermission = Permission.None;
        }

        private void RefreshSourcePermission()
        {
            if (SourceLicensePermission.SelectedValue == "ReadOnly")
                sourcePermission = Permission.Readonly;
            else if (SourceLicensePermission.SelectedValue == "Modify")
                sourcePermission = Permission.Modify;
            else
                sourcePermission = Permission.None;
        }

        private void RefreshCopyOptions()
        {
            addNew = DestinationLicenseAddNew.Checked;
            promoteExisting = DestinationLicensePromoteExisting.Checked;
            degradeExisting = DestinationLicenseDegradeExisting.Checked;
            deleteLeftOvers = DestinationLicenseDeleteLeftOvers.Checked;
        }

        private SoeFeatureType GetSoeFeatureType(bool refresh)
        {
            SoeFeatureType featureType = SoeFeatureType.None;
            if (DestinationLicensesAll.Checked)
            {
                featureType = SoeFeatureType.None;
            }
            else
            {
                if (refresh)
                {
                    RefreshSourceLicenses();
                    RefreshSourceArticles();
                }

                if (sourceLicenseId > 0)
                    featureType = SoeFeatureType.License;
                else if (sourceArticleId > 0)
                    featureType = SoeFeatureType.SysXEArticle;
            }

            return featureType;
        }

        private void SetSourceMode()
        {
            SourceLicenses.SelectedIndex = 0;
            SourceArticles.SelectedIndex = 0;
            Modules.SelectedIndex = 0;
            DestinationLicenses.SelectedIndex = 0;
            DestinationLicenses.Enabled = false;
            DestinationLicensesAll.Enabled = false;
            DependancyFeature.Enabled = false;
            DependancyFeatureOnlyLicenses.Visible = false;
            DependancyFeatureOnlyLicensesAndCompanies.Visible = false;
            DependencyFeaturePermission.Enabled = false;
            ValidateModules();
            FeatureTree.Clear();
            CompanyRoleTree.Clear();

            if (DefinePermissionManually.Checked)
            {
                SourceLicenses.Enabled = false;
                SourceArticles.Enabled = false;
            }
            else
            {
                SourceLicenses.Enabled = true;
                SourceArticles.Enabled = true;
            }
        }

        private void ValidateModules()
        {
            RefreshSourceLicenses();
            RefreshSourceArticles();
            Modules.Enabled = DefinePermissionManually.Checked || (sourceLicenseId > 0 || sourceArticleId > 0);
            if (!Modules.Enabled)
            {
                DestinationLicenses.Enabled = false;
                DestinationLicensesAll.Enabled = false;
                DependancyFeature.Enabled = false;
                DependancyFeatureOnlyLicenses.Visible = false;
                DependancyFeatureOnlyLicensesAndCompanies.Visible = false;
                DependencyFeaturePermission.Enabled = false;
            }
        }

        private void SetCopyText()
        {
            if (btnCopyTop != null)
                btnCopyTop.Text = GetText(5158, "Kopiera");
            if (btnCopyBottom != null)
                btnCopyBottom.Text = GetText(5158, "Kopiera");
        }

        private void SetCopyUniqueId()
        {
            if (btnCopyTop != null)
                copyTopUniqueId = btnCopyTop.UniqueID;
            if (btnCopyBottom != null)
                copyBottomUniqueId = btnCopyBottom.UniqueID;
        }

        private void SetCopyVisibillity()
        {
            // Ensure dependencyFeatureId is refreshed on every postback before using it
            RefreshDependencyFeature();

            bool visible = CompanyRoleTree.Nodes.Count > 0 || dependencyFeatureId > 0;
            if (btnCopyTop != null)
                btnCopyTop.Visible = visible;
            if (btnCopyBottom != null)
                btnCopyBottom.Visible = visible;

            // Hide bottom copy button when dependency feature mode is active
            if (this.dependencyFeatureId > 0)
                btnCopyBottom.Visible = false;

            SoeFormFooter.SetButtonSaveVisbillity(false);
        }

        private void SetFeatureTreeVisibillity()
        {
            bool visible = FeatureTree.Count > 0;
            SetFeatureTreeVisibillity(visible);
        }

        private void SetFeatureTreeVisibillity(bool visible)
        {
            FeatureTreeInfo.Visible = visible;
            FeatureTreeSettings.Visible = visible;
        }

        #endregion
    }
}
