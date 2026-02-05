using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;

namespace SoftOne.Soe.Web.soe.manage.system.admin.checklists
{
    public partial class _default : PageBase
    {
        #region Variables

        private ChecklistManager clm;
        private CompanyManager cm;
        private LicenseManager lm;
        private List<License> licenses;  // All licenses
        private List<ChecklistHead> sourceChecklistHeads;
        private int sourceActorCompanyId;  // Copy from 
        private int targetActorCompanyId;  // Copy to 
        private int sourceCheckListHeadId; // ChecklistID to copy from 

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Manage_System;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            cm = new CompanyManager(ParameterObject);
            clm = new ChecklistManager(ParameterObject);
            lm = new LicenseManager(ParameterObject);

            SetupLicense(SourceLicenses);  // Fill source licenses
            SetupLicense(TargetLicenses);  // Fill target licenses
            SetTerms();
            CheckButtonVisibility();       // Set Copy button visible if all selections are ok
        }

        private void SetTerms()
        {
            SoeFormPrefix.Title = GetText(4771, "Checklist kopiering");
            SoeFormPrefix.TabType = SoeTabViewType.Admin;
            LabelSourceLicenses.Text = GetText(4770, "Välj Lisens");
            LabelTargetLicense.Text = GetText(4770, "Välj Lisens");
            LabelSourceCompanies.Text = GetText(4762, "Välj företag");
            LabelTargetCompany.Text = GetText(4762, "Välj företag");
            LabelSourceCheckList.Text = GetText(4766, "Välj Checklist");
            ButtonCopy.Text = GetText(4768, "Kopiera");
            LabelCopyFrom.Text = GetText(4764, "Kopiera Från");
            LabelCopyTo.Text = GetText(4765, "Kopiera Till");
        }

        private void CheckButtonVisibility()
        {
            ButtonCopy.Visible = false;
            if (SourceCheckLists.SelectedValue != "0" && SourceCheckLists.SelectedValue != ""
                && SourceCompanies.SelectedValue != "0" && SourceCompanies.SelectedValue != ""
                && SourceCheckLists.SelectedValue != "0" && SourceCheckLists.SelectedValue != ""
                && TargetCompanies.SelectedValue != "0" && TargetCompanies.SelectedValue != ""
                && TargetLicenses.SelectedValue != "0" && TargetLicenses.SelectedValue != "")
            {
                ButtonCopy.Visible = true;

            }
            else ButtonCopy.Visible = false;
        }

        private void SetupCompany(int licenseId, DropDownList ddl)
        {
            ddl.Items.Clear();

            License selectedLicense = lm.GetLicense(licenseId);
            if (selectedLicense == null)
                return;

            if (lm.LicenseHasCompanies(selectedLicense.LicenseId))
            {
                //Add empty row
                ddl.Items.Add(new ListItem()
                {
                    Text = " ",
                    Value = "0",
                    Enabled = true,
                    Selected = false,
                });

                List<Company> companies = cm.GetCompaniesByLicense(licenseId);
                foreach (var company in companies.OrderBy(i => i.ActorCompanyId))
                {
                    ddl.Items.Add(new ListItem()
                    {
                        Text = company.Name + ". " + company.VatNr,
                        Value = company.ActorCompanyId.ToString(),
                        Enabled = true,
                        Selected = false,
                    });
                }
            }

        }

        private void SetupLicense(DropDownList ddl)
        {
            //Setup Licenses
            if (!IsPostBack)
            {
                licenses = lm.GetLicenses();
                //Add empty row
                ddl.Items.Add(new ListItem()
                {
                    Text = " ",
                    Value = "0",
                    Enabled = true,
                    Selected = true,
                });

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

        private void SetupSourceChecklist(DropDownList ddlSourceCompanies, DropDownList ddl)
        {
            if (ddlSourceCompanies.SelectedValue != "")
            {
                ddl.Items.Clear();
                sourceActorCompanyId = Convert.ToInt32(ddlSourceCompanies.SelectedValue);
                sourceChecklistHeads = clm.GetChecklistHeads(sourceActorCompanyId, true);

                ddl.Items.Add(new ListItem()
                {
                    Text = " ",
                    Value = "0",
                    Enabled = true,
                    Selected = true,
                });

                foreach (var Head in sourceChecklistHeads.OrderBy(i => i.ChecklistHeadId))
                {
                    ddl.Items.Add(new ListItem()
                    {
                        Text = Head.ChecklistHeadId + ". " + Head.Name,
                        Value = Head.ChecklistHeadId.ToString(),
                        Enabled = true,
                        Selected = false,
                    });
                }
            }
        }

        protected void SourceLicenses_Changed(int licenseId)
        {
            SourceCompanies.Items.Clear();

            License selectedLicense = lm.GetLicense(licenseId);
            if (selectedLicense == null)
                return;

            // Check if there are companies within license
            if (lm.LicenseHasCompanies(selectedLicense.LicenseId))
            {
                //Add empty row
                SourceCompanies.Items.Add(new ListItem()
                {
                    Text = " ",
                    Value = "0",
                    Enabled = true,
                    Selected = true,
                });
                List<Company> companies = cm.GetCompaniesByLicense(licenseId);
                foreach (var company in companies.OrderBy(i => i.LicenseId))
                {
                    SourceCompanies.Items.Add(new ListItem()
                    {
                        Text = company.Name + ". " + company.VatNr,
                        Value = company.CompanyNr.ToString(),
                        Enabled = true,
                        Selected = false,
                    });
                }
            }
        }

        protected void SourceLicenses_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SourceLicenses.SelectedValue != "")
            {
                int licenseId = Convert.ToInt32(SourceLicenses.SelectedValue);
                SetupCompany(licenseId, SourceCompanies);
                SourceCompanies.Visible = true;
            }
        }

        protected void SourceCompanies_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SourceCompanies.SelectedValue != "")
            {
                SetupSourceChecklist(SourceCompanies, SourceCheckLists);
            }
        }

        protected void TargetLicenses_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TargetLicenses.SelectedValue != "")
            {
                int licenseId = Convert.ToInt32(TargetLicenses.SelectedValue);
                SetupCompany(licenseId, TargetCompanies);
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            if (SourceCheckLists.SelectedValue != "")
                sourceCheckListHeadId = Convert.ToInt32(SourceCheckLists.SelectedValue);
            else 
                sourceCheckListHeadId = 0;

            if (SourceCompanies.SelectedValue != "")
                sourceActorCompanyId = Convert.ToInt32(SourceCompanies.SelectedValue);
            else 
                sourceActorCompanyId = 0;

            sourceActorCompanyId = Convert.ToInt32(SourceCompanies.SelectedValue);

            if (TargetCompanies.SelectedValue != "")
                targetActorCompanyId = Convert.ToInt32(TargetCompanies.SelectedValue);
            else 
                targetActorCompanyId = 0;

            if (sourceActorCompanyId > 0 && targetActorCompanyId > 0 && sourceCheckListHeadId > 0)
            {
                var result = cm.CopyChecklistFromAnotherCompany(targetActorCompanyId, sourceActorCompanyId, true, sourceCheckListHeadId);
                if (result.Success)
                    LabelResult.Text = GetText(4767, "Kopiering klar");
                else 
                    LabelResult.Text = GetText(4766, "Kopiering  misslyckats") + ": " + result.ErrorMessage;
            }
            else
            {
                StringBuilder reason = new StringBuilder();
                if (sourceActorCompanyId < 1) 
                    reason.Append("Source: ActorCompanyId missing");
                if (sourceCheckListHeadId < 1)
                    reason.Append("Source: CheckList not selected");
                if (targetActorCompanyId < 1) 
                    reason.Append("Target: ActorCompanyId missing");
                LabelResult.Text = reason.ToString();
            }
        }
    }
}

