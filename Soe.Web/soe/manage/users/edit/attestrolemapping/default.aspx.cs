using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util.Exceptions;
using SoftOne.Soe.Web.Controls;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using System.Web.UI.HtmlControls;

namespace SoftOne.Soe.Web.soe.manage.users.edit.attestrolemapping
{
    public partial class _default : PageBase
    {
        #region Variables

        private CompanyManager cm;
        private UserManager um;

        private User user;
        private Company company;
        private Dictionary<Company, List<AttestRole>> validCompanyAndRolesDict;

        #endregion

        private bool IsAuthorized
        {
            get
            {
                if (user == null)
                    return false;

                //Rule 1: Same User
                if (UserId == user.UserId)
                    return true;

                //Rule 2: Administrators or SupportLicense
                if (SoeLicense.Support && SoeUser.IsAdmin)
                    return true;

                //Rule 3: Administrators on Company and User connected to Company
                if (um.IsUserAdminInCompany(SoeUser, SoeCompany.ActorCompanyId) && um.IsUserConnectedToCompany(user.UserId, SoeCompany.ActorCompanyId))
                    return true;

                return false;
            }
        }

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Manage_Users_Edit_AttestRoleMapping;
            base.Page_Init(sender, e);

            //Add scripts and style sheets
            Scripts.Add("default.js");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            cm = new CompanyManager(ParameterObject);
            um = new UserManager(ParameterObject);

            //Mandatory parameters
            if (Int32.TryParse(QS["company"], out int actorCompanyId))
            {
                company = cm.GetCompany(actorCompanyId);
                if (company == null)
                    throw new SoeEntityNotFoundException("Company", this.ToString());
            }
            else
                throw new SoeQuerystringException("company", this.ToString());

            if (Int32.TryParse(QS["user"], out int userId))
            {
                user = um.GetUser(userId, loadUserCompanyRole: true);
                if (user == null)
                    throw new SoeEntityNotFoundException("User", this.ToString());
            }
            else
                throw new SoeQuerystringException("user", this.ToString());

            Form1.Title = user.Name;

            #endregion

            #region Authorization

            if (!IsAuthorized)
                RedirectToUnauthorized(UnauthorizationType.DataAuthorityMissing);

            #endregion

            #region Actions

            //Needed in save
            LoadValidCompaniesAndAttestRoles();

            if (Form1.IsPosted)
            {
                Save();
            }

            #endregion

            #region Populate

            RenderCompanyAndAttestRoles();

            #endregion

            #region MessageFromSelf

            if (!String.IsNullOrEmpty(MessageFromSelf))
            {
                if (MessageFromSelf == "SAVED")
                    Form1.MessageSuccess = GetText(5239, "Attestroller för användare sparade");
                else if (MessageFromSelf == "NOTSAVED")
                    Form1.MessageError = GetText(5240, "Attestroller för användare kunde inte sparas");
                else if (MessageFromSelf == "INVALID_MAXAMOUNT")
                    Form1.MessageWarning = GetText(5246, "Maxbelopp får inte överstiga maxbeloppet för attestrollen");
            }

            #endregion

            #region Navigation

            #endregion
        }

        #region Action-methods

        protected override void Save()
        {
            //bool result = true;

            //Build a list of checked AttestRoles
            var attestRoleUsers = new List<AttestRoleUser>();

            foreach (var pair in this.validCompanyAndRolesDict)
            {
                Company validCompany = pair.Key;
                List<AttestRole> validAttestRoles = pair.Value;

                foreach (AttestRole validAttestRole in validAttestRoles)
                {
                    #region AttestRole

                    if (StringUtility.GetBool(F[GetCheckID(validCompany, validAttestRole)]))
                    {
                        AttestRoleUser attestRoleUser = new AttestRoleUser()
                        {                            
                            DateFrom = CalendarUtility.GetNullableDateTime(F[GetDateFromID(validCompany, validAttestRole)]),
                            DateTo = CalendarUtility.GetNullableDateTime(F[GetDateToID(validCompany, validAttestRole)]),
                            MaxAmount = NumberUtility.ToDecimal(F[GetMaxAmountID(validCompany, validAttestRole)], 2),

                            //Set FK
                            UserId = user.UserId,
                            AttestRoleId = validAttestRole.AttestRoleId,
                        };

                        if (attestRoleUser.MaxAmount > validAttestRole.DefaultMaxAmount)
                            RedirectToSelf("INVALID_MAXAMOUNT", true);

                        attestRoleUsers.Add(attestRoleUser);
                    }

                    #endregion
                }
            }
            
            RedirectToSelf("NOTSAVED", true);
        }

        #endregion

        #region Help methods

        private void LoadValidCompaniesAndAttestRoles()
        {
            this.validCompanyAndRolesDict = cm.GetValidCompanyAndAttestRoles(IsAdmin, UserId, SoeUser.LicenseId);
        }

        private void RenderCompanyAndAttestRoles()
        {
            foreach (var pair in this.validCompanyAndRolesDict)
            {
                Company validCompany = pair.Key;
                List<AttestRole> validAttestRoles = pair.Value;

                if (validAttestRoles.IsNullOrEmpty())
                    continue;

                #region Prefix

                HtmlTableRow tRow;
                HtmlTableCell tCell;

                string companyName = validCompany.Name;
                if (validCompany.ActorCompanyId != SoeCompany.ActorCompanyId)
                    companyName += " (" + GetText(10098, "Logga in på företaget för att kunna gå in på attestrollerna") + ")";

                var divCompanyRow = new HtmlGenericControl("div");
                var divCompany = new HtmlGenericControl("div");
                var fieldset = new HtmlGenericControl("fieldset");
                var legend = new HtmlGenericControl("legend")
                {
                    InnerText = companyName,
                };
                fieldset.Controls.Add(legend);

                var tableCompany = new HtmlTable();
                tableCompany.CellSpacing = 2;

                #endregion

                #region Header

                var tRowHeader = new HtmlTableRow();
                var tCellHeader = new HtmlTableCell();

                //AttestRole
                tCellHeader = new HtmlTableCell();
                tCellHeader.Controls.Add(new Text()
                {
                    TermID = 5223,
                    DefaultTerm = "Attestroll",
                    FitInTable = true,
                });
                tRowHeader.Controls.Add(tCellHeader);

                //Description
                tCellHeader = new HtmlTableCell();
                tCellHeader.Controls.Add(new Text()
                {
                    TermID = 5225,
                    DefaultTerm = "Beskrivning",
                    FitInTable = true,
                });
                tRowHeader.Controls.Add(tCellHeader);

                //Module
                tCellHeader = new HtmlTableCell();
                tCellHeader.Controls.Add(new Text()
                {
                    TermID = 1920,
                    DefaultTerm = "Modul",
                    FitInTable = true,
                });
                tRowHeader.Controls.Add(tCellHeader);

                //Check
                tRowHeader.Controls.Add(new HtmlTableCell());

                //DateFrom
                tCellHeader = new HtmlTableCell();
                tCellHeader.Controls.Add(new Text()
                {
                    TermID = 5241,
                    DefaultTerm = "Från",
                    FitInTable = true,
                });
                tRowHeader.Controls.Add(tCellHeader);

                //DateTo
                tCellHeader = new HtmlTableCell();
                tCellHeader.Controls.Add(new Text()
                {
                    TermID = 5242,
                    DefaultTerm = "Till",
                    FitInTable = true,
                });
                tRowHeader.Controls.Add(tCellHeader);

                //MaxAmount
                tCellHeader = new HtmlTableCell();
                tCellHeader.Controls.Add(new Text()
                {
                    TermID = 5243,
                    DefaultTerm = "Maxbelopp",
                    FitInTable = true,
                });
                tRowHeader.Controls.Add(tCellHeader);

                //Info
                tRowHeader.Controls.Add(new HtmlTableCell());

                tableCompany.Rows.Add(tRowHeader);

                #endregion

                foreach (AttestRole attestRole in validAttestRoles.OrderBy(i => i.Module).ThenBy(i => i.Name))
                {
                    #region AttestRole

                    var attestRoleUser = (from aru in attestRole.AttestRoleUser
                                          where aru.UserId == user.UserId
                                          select aru).FirstOrDefault();

                    tRow = new HtmlTableRow();

                    //AttestRole
                    tCell = new HtmlTableCell();
                    if (HasPermissions(attestRole, out string href))
                    {
                        var link = new Link()
                        {
                            ID = GetAttestRoleID(validCompany, attestRole),
                            Href = href,
                            Value = attestRole.Name.ToUpper(),
                            Alt = attestRole.Name,
                        };
                        tCell.Controls.Add(link);
                    }
                    else
                    {
                        var text = new Text()
                        {
                            ID = GetAttestRoleID(validCompany, attestRole),
                            LabelSetting = attestRole.Name,
                            FitInTable = true,
                        };
                        tCell.Controls.Add(text);
                    }
                    tCell.Attributes.Add("style", "width:200px");
                    tRow.Controls.Add(tCell);

                    //Description
                    tCell = new HtmlTableCell();
                    var description = new Text()
                    {
                        ID = GetDescriptionID(validCompany, attestRole),
                        LabelSetting = StringUtility.Left(attestRole.Description != null ? attestRole.Description.ToUpper() : String.Empty, 50),
                        FitInTable = true,
                    };
                    tCell.Attributes.Add("style", "width:200px");
                    tCell.Controls.Add(description);
                    tRow.Controls.Add(tCell);

                    //Module
                    tCell = new HtmlTableCell();
                    var module = new Text()
                    {
                        ID = GetModuleID(validCompany, attestRole),
                        LabelSetting = TextService.GetModuleName(attestRole.Module),
                        FitInTable = true,
                    };
                    tCell.Attributes.Add("style", "width:90px");
                    tCell.Controls.Add(module);
                    tRow.Controls.Add(tCell);

                    //Check
                    tCell = new HtmlTableCell();
                    var check = new CheckBoxEntry()
                    {
                        ID = GetCheckID(validCompany, attestRole),
                        HideLabel = true,
                        DisableSettings = true,
                        FitInTable = true,
                        OnChange = "AttestRoleChecked('" + GetID(validCompany, attestRole) + "')",
                        Value = attestRoleUser != null ? Boolean.TrueString : Boolean.FalseString,
                    };
                    tCell.Controls.Add(check);
                    tRow.Controls.Add(tCell);

                    //DateFrom
                    tCell = new HtmlTableCell();
                    var dateFrom = new DateEntry()
                    {
                        ID = GetDateFromID(validCompany, attestRole),
                        HideLabel = true,
                        DisableSettings = true,
                        FitInTable = true,
                        Width = 100,
                        Value = attestRoleUser != null && attestRoleUser.DateFrom.HasValue ? attestRoleUser.DateFrom.Value.ToShortDateString() : String.Empty,
                    };
                    tCell.Controls.Add(dateFrom);
                    tRow.Controls.Add(tCell);

                    //DateTo
                    tCell = new HtmlTableCell();
                    var dateTo = new DateEntry()
                    {
                        ID = GetDateToID(validCompany, attestRole),
                        HideLabel = true,
                        DisableSettings = true,
                        FitInTable = true,
                        Width = 100,
                        Value = attestRoleUser != null && attestRoleUser.DateTo.HasValue ? attestRoleUser.DateTo.Value.ToShortDateString() : String.Empty,
                    };
                    tCell.Controls.Add(dateTo);
                    tRow.Controls.Add(tCell);

                    //MaxAmount
                    tCell = new HtmlTableCell();
                    var maxAmount = new NumericEntry()
                    {
                        ID = GetMaxAmountID(validCompany, attestRole),
                        HideLabel = true,
                        DisableSettings = true,
                        FitInTable = true,
                        MaxLength = 8,
                        Width = 60,
                        ReadOnly = attestRoleUser == null,
                        Value = (attestRoleUser != null ? Decimal.Round(attestRoleUser.MaxAmount, 0) : Decimal.Round(attestRole.DefaultMaxAmount, 0)).ToString(),
                        InfoText = "(" + Decimal.Round(attestRole.DefaultMaxAmount, 0) + ")",
                    };
                    tCell.Controls.Add(maxAmount);
                    tRow.Controls.Add(tCell);

                    //Info
                    tCell = new HtmlTableCell();
                    var info = new Link()
                    {
                        Href = "#",
                        ImageSrc = "/img/information.png",
                        CssClass = "infoLink",
                        SkipTabstop = true,
                        Invisible = attestRoleUser == null,
                        Alt = cm.GetCreatedModified(attestRoleUser as EntityObject),
                    };
                    tCell.Controls.Add(info);
                    tRow.Controls.Add(tCell);

                    tableCompany.Rows.Add(tRow);

                    #endregion
                }

                #region Postfix

                fieldset.Controls.Add(tableCompany);
                divCompany.Controls.Add(fieldset);
                divCompanyRow.Controls.Add(divCompany);
                AttestRoleMapping.Controls.Add(divCompany);
                AttestRoleMapping.Controls.Add(divCompanyRow);

                #endregion
            }
        }

        private bool HasPermissions(AttestRole attestRole, out string href)
        {
            href = "";

            string section = "";
            bool hasPermission = false;

            if (attestRole != null)
            {
                if (attestRole.ActorCompanyId == SoeCompany.ActorCompanyId)
                {
                    switch (attestRole.Module)
                    {
                        case (int)SoeModule.Economy:
                            section = "supplier";
                            hasPermission = HasRolePermission(Feature.Manage_Attest_Supplier_AttestRoles_Edit, Permission.Readonly);
                            break;
                        case (int)SoeModule.Billing:
                            section = "customer";
                            hasPermission = HasRolePermission(Feature.Manage_Attest_Customer_AttestRoles_Edit, Permission.Readonly);
                            break;
                        case (int)SoeModule.Time:
                            section = "time";
                            hasPermission = HasRolePermission(Feature.Manage_Attest_Time_AttestRoles_Edit, Permission.Readonly);
                            break;
                    }
                }
                else
                {
                    hasPermission = false;
                }
            }

            if (hasPermission)
                href = GetAttestRoleLink(attestRole.AttestRoleId, section);

            return hasPermission;
        }

        private string GetAttestRoleID(Company company, AttestRole attestRole)
        {
            return GetID("AttestRole", company, attestRole);
        }

        private string GetDescriptionID(Company company, AttestRole attestRole)
        {
            return GetID("Description", company, attestRole);
        }

        private string GetDateFromID(Company company, AttestRole attestRole)
        {
            return GetID("DateFrom", company, attestRole);
        }

        private string GetDateToID(Company company, AttestRole attestRole)
        {
            return GetID("DateTo", company, attestRole);
        }

        private string GetMaxAmountID(Company company, AttestRole attestRole)
        {
            return GetID("MaxAmount", company, attestRole);
        }

        private string GetModuleID(Company company, AttestRole attestRole)
        {
            return GetID("Module", company, attestRole);
        }

        private string GetCheckID(Company company, AttestRole attestRole)
        {
            return GetID("Check", company, attestRole);
        }

        private string GetID(Company company, AttestRole attestRole)
        {
            return GetID("", company, attestRole);
        }

        private string GetID(string prefix, Company company, AttestRole attestRole)
        {
            return prefix + "_" + company.ActorCompanyId + "_" + attestRole.AttestRoleId;
        }

        private string GetAttestRoleLink(int attestRoleId, string section)
        {
            return String.Format("/soe/manage/attest/{0}/role/edit/?role={1}", section, attestRoleId);
        }

        #endregion
    }
}
