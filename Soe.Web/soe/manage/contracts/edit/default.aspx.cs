using Soe.Sys.Common.DTO;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util.Exceptions;
using SoftOne.Soe.Web.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.HtmlControls;

namespace SoftOne.Soe.Web.soe.manage.contracts.edit
{
    public partial class _default : PageBase
    {
        #region Variables

        private AccountManager am;
        private CompanyManager cm;
        private CountryCurrencyManager ccm;
        private LicenseManager lm;
        private LoginManager lom;
        private FeatureManager fm;
        private RoleManager rm;
        private SettingManager sm;
        private UserManager um;

        private License license;
        private List<SysXEArticle> sysXEArticles;
        private List<LicenseArticle> licenseArticles;

        private bool liber;
        private string licenseNo;
        private int currentLicenseId;

        public bool IsAuthorized
        {
            get
            {
                if (license == null)
                    return true;

                //Rule 1: Same License
                if (SoeCompany.LicenseId == license.LicenseId)
                    return true;

                //Rule 2: Administrators on SupportLicense
                if (SoeLicense.Support && SoeUser.IsAdmin)
                    return true;

                return false;
            }
        }

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Manage_Contracts_Edit;
            base.Page_Init(sender, e);

            //Add scripts and style sheets
            Scripts.Add("default.js");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            am = new AccountManager(ParameterObject);
            cm = new CompanyManager(ParameterObject);
            ccm = new CountryCurrencyManager(ParameterObject);
            lm = new LicenseManager(ParameterObject);
            lom = new LoginManager(ParameterObject);
            fm = new FeatureManager(ParameterObject);
            rm = new RoleManager(ParameterObject);
            sm = new SettingManager(ParameterObject);
            um = new UserManager(ParameterObject);

            //Mandatory parameters

            //Mode 
            PreOptionalParameterCheck(Request.Url.AbsolutePath, Request.Url.PathAndQuery);

            //Optional parameters

            if (QS["license"] != null)
                Int32.TryParse(QS["license"], out currentLicenseId);
            else
                currentLicenseId = 0;

            licenseNo = QS["licenseNr"];
            if (!String.IsNullOrEmpty(licenseNo))
            {
                license = LicenseCacheManager.Instance.GetLicense(licenseNo);
                if (license == null)
                {
                    Form1.MessageWarning = GetText(1282, "License hittades inte");
                    Mode = SoeFormMode.Register;
                }
            }

            //Mode
            string editModeTabHeaderText = GetText(1056, "Redigera licens");
            string registerModeTabHeaderText = GetText(1555, "Registrera licens");
            PostOptionalParameterCheck(Form1, license, true, editModeTabHeaderText, registerModeTabHeaderText);

            Form1.Title = license != null ? license.Name : "";

            #endregion

            #region Authorization

            if (!IsAuthorized)
                RedirectToUnauthorized(UnauthorizationType.DataAuthorityMissing);

            #endregion

            #region Prereq

            bool currentLicense = license != null && license.LicenseId == SoeLicense.LicenseId;

            // Liber setting
            liber = sm.GetBoolSetting(SettingMainType.Application, (int)ApplicationSettingType.LiberAutoCopying, UserId, SoeCompany.ActorCompanyId, 0);
            if (liber)
                ModifySysXEArticles.Value = Boolean.TrueString;

            //Needed in save
            sysXEArticles = fm.GetSysXEArticles();
            licenseArticles = license != null ? licenseArticles = lm.GetLicenseArticles(license.LicenseId) : new List<LicenseArticle>();

            #endregion

            #region Actions

            if (Form1.IsPosted)
            {
                Save();
            }

            #endregion

            #region Populate

            SysServiceManager ssm = new SysServiceManager(null);
            var validSysServers = ssm.GetValidSysServers();
            if (!validSysServers.Any(i => i.SysServerId == 0))
                validSysServers.Insert(0, new SysServerDTO() { SysServerId = 0, Url = " "});

            SysServer.ConnectDataSource(validSysServers, "Url", "SysServerId");

            BaseCurrency.ConnectDataSource(ccm.GetSysCurrenciesDict(false));

            ModifySysXEArticles.InfoText = GetText(5376, "Kryssa i för att att uppdatera artiklar enligt nedan");
            ModifySysXEArticles.ReadOnly = currentLicense;
            ModifySysXEArticles.Value = (license == null) ? Boolean.TrueString : Boolean.FalseString;

            DeleteSysXEArticles.InfoText = GetText(5967, "Kryssa i för att att ta bort artiklar som avmarkeras. OBS! Kunden kommer tappa befintliga behörigheter!");
            DeleteSysXEArticles.ReadOnly = currentLicense;

            CannotModifyOwnLicenseWarning.Visible = currentLicense;

            #region SysXEArticles

            HtmlTableRow tRow;
            HtmlTableCell tCell;
            CheckBoxEntry check;
            Text text;
            Text module;
            Text articleNr;
            Text articleNrYear1;
            Text articleNrYear2;
            Text description;

            foreach (SysXEArticle sysXEArticle in sysXEArticles)
            {
                tRow = new HtmlTableRow();

                tCell = new HtmlTableCell();
                module = new Text()
                {
                    LabelSetting = sysXEArticle.ModuleGroup,
                    FitInTable = true,
                };
                tCell.Controls.Add(module);
                tRow.Cells.Add(tCell);

                tCell = new HtmlTableCell();
                articleNr = new Text()
                {
                    LabelSetting = sysXEArticle.ArticleNr,
                    FitInTable = true,
                };
                tCell.Controls.Add(articleNr);
                tRow.Cells.Add(tCell);

                tCell = new HtmlTableCell();
                articleNrYear1 = new Text()
                {
                    LabelSetting = sysXEArticle.ArticleNrYear1,
                    FitInTable = true,
                };
                tCell.Controls.Add(articleNrYear1);
                tRow.Cells.Add(tCell);

                tCell = new HtmlTableCell();
                articleNrYear2 = new Text()
                {
                    LabelSetting = sysXEArticle.ArticleNrYear2,
                    FitInTable = true,
                };
                tCell.Controls.Add(articleNrYear2);
                tRow.Cells.Add(tCell);


                tCell = new HtmlTableCell();
                text = new Text()
                {
                    LabelSetting = sysXEArticle.Name,
                    FitInTable = true,
                };
                tCell.Controls.Add(text);
                tRow.Cells.Add(tCell);

                tCell = new HtmlTableCell();
                description = new Text()
                {
                    LabelSetting = sysXEArticle.Description,
                    FitInTable = true,
                };
                tCell.Controls.Add(description);
                tRow.Cells.Add(tCell);

                tCell = new HtmlTableCell();
                check = new CheckBoxEntry()
                {
                    ID = "SysXEArticle" + "_" + sysXEArticle.SysXEArticleId,
                    CssClass = "sysXEArticles",
                    Value = Boolean.FalseString,
                    HideLabel = true,
                    DisableSettings = true,
                    FitInTable = true,
                };

                // Liber setting
                if (liber && (sysXEArticle.SysXEArticleId == (int)SoeXeArticle.User || sysXEArticle.SysXEArticleId == (int)SoeXeArticle.Billing || sysXEArticle.SysXEArticleId == (int)SoeXeArticle.Order || sysXEArticle.SysXEArticleId == (int)SoeXeArticle.TimeProjectBillingReport))
                {
                    check.Value = Boolean.TrueString;
                }

                if (license != null)
                {
                    var licenseArticle = licenseArticles.FirstOrDefault(i => i.SysXEArticleId == sysXEArticle.SysXEArticleId);
                    if (licenseArticle != null)
                        check.Value = Boolean.TrueString;
                }
                tCell.Controls.Add(check);
                tRow.Cells.Add(tCell);

                SysXEArticleTable.Rows.Add(tRow);
            }

            #endregion

            #endregion

            #region Set data

            if (license != null)
            {
                LicenseNr.Value = license.LicenseNr;
                Name.Value = license.Name;
                OrgNr.Value = license.OrgNr;
                LegalName.Value = license.LegalName;
                MaxNrOfUsers.Value = license.MaxNrOfUsers.ToString();
                MaxNrOfEmployees.Value = license.MaxNrOfEmployees.ToString();
                MaxNrOfMobileUsers.Value = license.MaxNrOfMobileUsers.ToString();
                ConcurrentUsers.Value = license.ConcurrentUsers.ToString();
                NrOfCompanies.Value = license.NrOfCompanies.ToString();
                SysServer.Value = validSysServers.FirstOrDefault(f => f != null && f.SysServerId == license.SysServerId)?.SysServerId.ToString() ?? "0";
                if (license.TerminationDate.HasValue)
                    TerminationDate.Value = license.TerminationDate.Value.ToShortDateString();

                BaseCurrency.Visible = false;
            }
            else
            {
                // Default values
                LicenseNr.Value = lm.GetNextLicenseNr();
                BaseCurrency.Value = ((int)TermGroup_Currency.SEK).ToString();

                //Can only copy permissions from License if user has permission to modify permissions
                if (HasRolePermission(Feature.Manage_Contracts_Edit_Permission, Permission.Modify))
                {
                    TemplateLicense.Visible = true;
                    TemplateLicense.ConnectDataSource(lm.GetLicensesDict(true, false));
                }
            }

            #endregion

            #region MessageFromSelf

            if (!String.IsNullOrEmpty(MessageFromSelf))
            {
                if (MessageFromSelf == "SAVED")
                {
                    LicenseInstructions.HeaderText = GetText(1683, "Konfigurera license");
                    LicenseInstructions.Numeric = true;
                    LicenseInstructions.Instructions = new List<string>()
                    {
                        GetText(1684, "Default företag, användare och roll skapade. Komplettera data för dem vid behov"),
                        GetText(1685, "Sätt önskad behörighet på licens, företag och roll") + " (" + GetText(1706, "Välj Läsbehörighet eller Skrivbehörighet") + ")",
                        GetText(1686, "Logga in på det skapade företaget med nya användarnamn 'sys' och organisationsnummer som lösenord"),
                        GetText(1687, "Efter inlogg, ändra lösenord på användare 'sys'"),
                    };
                    Form1.MessageSuccess = GetText(1682, "Licens sparad");
                }
                else if (MessageFromSelf == "SAVED_WITH_ERRORS")
                {
                    Form1.MessageWarning = GetText(1682, "Licens sparad") + ". " + GetText(1917, "Alla defaultinställningar för licens kunde inte sparas, se logg");
                }
                else if (MessageFromSelf == "NOTSAVED")
                    Form1.MessageError = GetText(2059, "Kunde inte spara licens");
                else if (MessageFromSelf == "NOTSAVED_SUPPORTEXISTS")
                    Form1.MessageError = GetText(3030, "Kunde inte spara licens, det finns redan en supportlicens registrerad");
                else if (MessageFromSelf == "UPDATED")
                    Form1.MessageSuccess = GetText(2060, "Licens uppdaterat");
                else if (MessageFromSelf == "UPDATED_WITHERRORS")
                    Form1.MessageSuccess = GetText(2060, "Licens uppdaterat") + ". " + GetText(1917, "Alla defaultinställningar för licens kunde inte sparas, se logg");
                else if (MessageFromSelf == "NOTUPDATED")
                    Form1.MessageError = GetText(2061, "Kunde inte uppdatera licens");
                else if (MessageFromSelf == "NOTUPDATED_SUPPORTEXISTS")
                    Form1.MessageError = GetText(3031, "Kunde inte uppdatera licens, det finns redan en supportlicens registrerad");
                else if (MessageFromSelf == "DELETED")
                    Form1.MessageSuccess = GetText(1986, "License borttagen");
                else if (MessageFromSelf == "NOTDELETED")
                    Form1.MessageError = GetText(1283, "License kunde inte tas bort");
                else if (MessageFromSelf == "BADINPUT")
                    Form1.MessageWarning = GetText(2099, "Felaktig indata");
                else if (MessageFromSelf == "EXIST")
                    Form1.MessageInformation = GetText(1241, "License finns redan");
                else if (MessageFromSelf == "FAILED_INVALIDCOMPANYANDUSERS")
                    Form1.MessageWarning = GetText(1919, "Användare och företag måste vara större än 0");
            }

            #endregion

            #region Navigation

            if (license != null)
            {
                Form1.SetRegLink(GetText(2019, "Registrera licens"), "",
                    Feature.Manage_Contracts_Edit, Permission.Modify);

                Form1.AddLink(GetText(1077, "Läsbehörighet"), "permission/" + GetBaseQS() + "&permission=" + (int)Permission.Readonly,
                    Feature.Manage_Contracts_Edit_Permission, Permission.Readonly);
                Form1.AddLink(GetText(1080, "Skrivbehörighet"), "permission/" + GetBaseQS() + "&permission=" + (int)Permission.Modify,
                    Feature.Manage_Contracts_Edit_Permission, Permission.Readonly);

                Form1.AddLink(GetText(1613, "Visa företag"), "/soe/manage/companies/" + GetBaseQS(),
                    Feature.Manage_Companies, Permission.Readonly);
                Form1.AddLink(GetText(1577, "Visa användare"), "/soe/manage/users/" + GetBaseQS(),
                    Feature.Manage_Users, Permission.Readonly);
            }

            #endregion
        }

        #region Action-methods

        protected override void Save()
        {
            licenseNo = F["LicenseNr"];
            string name = F["Name"];
            string orgNr = F["OrgNr"];
            int concurrentUsers = Convert.ToInt32(F["ConcurrentUsers"]);
            string legalName = F["LegalName"];
            int maxNrOfUsers = StringUtility.GetInt(F["MaxNrOfUsers"], 1);
            int maxNrOfEmployees = StringUtility.GetInt(F["MaxNrOfEmployees"], 1);
            int maxNrOfMobileUsers = StringUtility.GetInt(F["MaxNrOfMobileUsers"], 1);
            int nrOfCompanies = StringUtility.GetInt(F["NrOfCompanies"], 1);
            int sysServerId = StringUtility.GetInt(F["SysServer"], 0);
            DateTime? terminationDate = CalendarUtility.GetNullableDateTime(F["TerminationDate"]);
            int baseCurrency = Convert.ToInt32(F["BaseCurrency"]);

            if (concurrentUsers >= 0 && maxNrOfUsers >= 0 && maxNrOfEmployees >= 0 && nrOfCompanies >= 0)
            {
                if (license == null)
                {
                    //Validation: Company not already exist
                    if (lm.LicenseExist(licenseNo))
                        RedirectToSelf("EXIST", true);

                    if (concurrentUsers <= 0 || maxNrOfUsers <= 0 || maxNrOfEmployees <= 0 || nrOfCompanies <= 0)
                        RedirectToSelf("FAILED_INVALIDCOMPANYANDUSERS", true);

                    //Add License
                    license = new License()
                    {
                        LicenseNr = licenseNo,
                        Name = name,
                        OrgNr = orgNr,
                        ConcurrentUsers = concurrentUsers,
                        MaxNrOfUsers = maxNrOfUsers,
                        MaxNrOfEmployees = maxNrOfEmployees,
                        MaxNrOfMobileUsers = maxNrOfMobileUsers,
                        NrOfCompanies = nrOfCompanies,
                        TerminationDate = terminationDate,
                        LegalName = legalName,
                        IsAccountingOffice = false,
                        AccountingOfficeId = 0,
                        AccountingOfficeName = String.Empty,
                        SysServerId = sysServerId != 0 ? sysServerId : (int?)null,
                        LicenseGuid = Guid.NewGuid(),
                    };

                    //To flag if all settings for License was saved without errors, otherwise logg errors and show message
                    bool errors = false;

                    ActionResult result = lm.AddLicense(license);
                    if (result.Success)
                    {
                        string postBackUrlQs = GetBaseQS(license, prefix: "&");

                        #region Template License

                        if (Int32.TryParse(F["TemplateLicense"], out int templateLicenseId) && templateLicenseId > 0)
                        {
                            //Copy permissions from template License
                            fm.CopyLicenseFeatures(license.LicenseId, templateLicenseId);
                        }

                        #endregion

                        #region LicenseArticle

                        if (!SaveLicenseArticles().Success)
                            errors = true;

                        #endregion

                        #region Default Company, Role and User

                        // Default Company
                        Company company = new Company()
                        {
                            Name = license.Name,//GetText(1676, "Default företag"),
                            ShortName = license.Name.Length > 10 ? license.Name.Substring(0, 9) : license.Name,//"Default",
                            OrgNr = license.OrgNr,//"111111111",
                            SysCountryId = (int)TermGroup_Country.SE,
                            AllowSupportLogin = true,
                            AllowSupportLoginTo = DateTime.Now.AddDays(100),
                        };
                        result = cm.AddCompany(company, license.LicenseId);
                        if (!result.Success)
                        {
                            errors = true;

                            string message = GetText(1912, "Default företag kunde inte läggas upp för licens") + " " + license.LicenseNr;
                            SysLogManager.LogError<_default>(new SoeGeneralException(message, result.Exception, this.ToString()));
                        }

                        // API
                        SettingManager.UpdateInsertStringSetting(SettingMainType.Company, (int)CompanySettingType.CompanyAPIKey, Guid.NewGuid().ToString(), 0, company.ActorCompanyId, 0);

                        // Default Role
                        Role role = new Role()
                        {
                            TermId = (int)TermGroup_Roles.Systemadmin,
                        };
                        result = rm.AddRole(role, company.ActorCompanyId);
                        if (!result.Success)
                        {
                            errors = true;
                            string message = GetText(1913, "Default roll kunde inte läggas upp för licens") + " " + license.LicenseNr;
                            SysLogManager.LogError<_default>(new SoeGeneralException(message, result.Exception, this.ToString()));
                        }

                        //Default User
                        User user = new User()
                        {
                            LoginName = Constants.APPLICATION_LICENSEADMIN_LOGINNAME,
                            Name = Constants.APPLICATION_LICENSEADMIN_NAME,//GetText(1677, "BYT LÖSENORD FÖR ANVÄNDAREN"),
                            passwordhash = lom.GetPasswordHash(Constants.APPLICATION_LICENSEADMIN_LOGINNAME, license.OrgNr),
                            ChangePassword = true,
                            SysUser = true,
                            LangId = 1,
                            idLoginGuid = Guid.NewGuid(),
                        };

                        result = um.AddUser(user, 0, license.LicenseId, company.ActorCompanyId, role.RoleId);
                        if (!result.Success)
                        {
                            errors = true;
                            string message = GetText(1914, "Default användare kunde inte läggas upp för licens") + " " + license.LicenseNr;
                            SysLogManager.LogError<_default>(new SoeGeneralException(message, result.Exception, this.ToString()));
                        }

                        // Connect user to role and company
                        result = um.AddUserCompanyRoleMapping(user.UserId, company.ActorCompanyId, role.RoleId, true);
                        if (!result.Success)
                        {
                            errors = true;
                            string message = GetText(1915, "Kunde inte koppla användare till roll och företag för licens") + " " + license.LicenseNr;
                            SysLogManager.LogError<_default>(new SoeGeneralException(message, result.Exception, this.ToString()));
                        }

                        // Base currency 
                        SysCurrency sysCurrency = ccm.GetSysCurrency(baseCurrency, true);
                        if (sysCurrency != null)
                        {
                            Currency currency = new Currency()
                            {
                                SysCurrencyId = sysCurrency.SysCurrencyId,
                                IntervalType = Constants.CURRENCY_INTERVALTYPE_DEFAULT,
                                UseSysRate = Constants.CURRENCY_USESYSRATE_DEFAULT,
                            };

                            result = ccm.AddCurrency(currency, DateTime.Today, company.ActorCompanyId);
                            if (!result.Success)
                            {
                                errors = true;
                                string message = GetText(4161, "Default basvaluta kunde inte läggas upp för licens") + " " + license.LicenseNr;
                                SysLogManager.LogError<_default>(new SoeGeneralException(message, result.Exception, this.ToString()));
                            }
                        }

                        #endregion

                        #region Permission Company

                        //Add Permission to Company
                        CompanyFeature companyFeature = new CompanyFeature()
                        {
                            SysFeatureId = (int)Feature.Manage,
                            SysPermissionId = (int)Permission.Modify
                        };
                        fm.AddCompanyPermission(companyFeature, company.ActorCompanyId);
                        companyFeature = new CompanyFeature() { SysFeatureId = (int)Feature.Manage_Companies, SysPermissionId = (int)Permission.Modify };
                        fm.AddCompanyPermission(companyFeature, company.ActorCompanyId);
                        companyFeature = new CompanyFeature() { SysFeatureId = (int)Feature.Manage_Companies_Edit, SysPermissionId = (int)Permission.Modify };
                        fm.AddCompanyPermission(companyFeature, company.ActorCompanyId);
                        companyFeature = new CompanyFeature() { SysFeatureId = (int)Feature.Manage_Companies_Edit_Permission, SysPermissionId = (int)Permission.Modify };
                        fm.AddCompanyPermission(companyFeature, company.ActorCompanyId);
                        companyFeature = new CompanyFeature() { SysFeatureId = (int)Feature.Manage_Roles, SysPermissionId = (int)Permission.Modify };
                        fm.AddCompanyPermission(companyFeature, company.ActorCompanyId);
                        companyFeature = new CompanyFeature() { SysFeatureId = (int)Feature.Manage_Roles_Edit, SysPermissionId = (int)Permission.Modify };
                        fm.AddCompanyPermission(companyFeature, company.ActorCompanyId);
                        companyFeature = new CompanyFeature() { SysFeatureId = (int)Feature.Manage_Roles_Edit_Permission, SysPermissionId = (int)Permission.Modify };
                        fm.AddCompanyPermission(companyFeature, company.ActorCompanyId);

                        #endregion

                        #region Permission Role

                        //Add Permission to Role
                        RoleFeature roleFeature = new RoleFeature()
                        {
                            SysFeatureId = (int)Feature.Manage,
                            SysPermissionId = (int)Permission.Modify
                        };
                        fm.AddRolePermission(roleFeature, role.RoleId);
                        roleFeature = new RoleFeature() { SysFeatureId = (int)Feature.Manage_Companies, SysPermissionId = (int)Permission.Modify };
                        fm.AddRolePermission(roleFeature, role.RoleId);
                        roleFeature = new RoleFeature() { SysFeatureId = (int)Feature.Manage_Companies_Edit, SysPermissionId = (int)Permission.Modify };
                        fm.AddRolePermission(roleFeature, role.RoleId);
                        roleFeature = new RoleFeature() { SysFeatureId = (int)Feature.Manage_Companies_Edit_Permission, SysPermissionId = (int)Permission.Modify };
                        fm.AddRolePermission(roleFeature, role.RoleId);
                        roleFeature = new RoleFeature() { SysFeatureId = (int)Feature.Manage_Roles, SysPermissionId = (int)Permission.Modify };
                        fm.AddRolePermission(roleFeature, role.RoleId);
                        roleFeature = new RoleFeature() { SysFeatureId = (int)Feature.Manage_Roles_Edit, SysPermissionId = (int)Permission.Modify };
                        fm.AddRolePermission(roleFeature, role.RoleId);
                        roleFeature = new RoleFeature() { SysFeatureId = (int)Feature.Manage_Roles_Edit_Permission, SysPermissionId = (int)Permission.Modify };
                        fm.AddRolePermission(roleFeature, role.RoleId);

                        #endregion

                        #region Default settings/prereq

                        //Default AccountDim standard
                        AccountDim accountDim = new AccountDim()
                        {
                            AccountDimNr = Constants.ACCOUNTDIM_STANDARD,
                            Name = GetText(1258, "Konto"),
                            ShortName = GetText(3776, "Std"),
                            SysSieDimNr = null,
                            MinChar = null,
                            MaxChar = null,
                        };
                        result = am.AddAccountDim(accountDim, company.ActorCompanyId);
                        if (!result.Success)
                        {
                            errors = true;
                            string message = GetText(1916, "Kunde inte lägga till standard kontroll till defaultföretag för licens") + " " + license.LicenseNr;
                            SysLogManager.LogError<_default>(new SoeGeneralException(message, result.Exception, this.ToString()));
                        }

                        // Currency
                        result = sm.UpdateInsertIntSetting(SettingMainType.Company, (int)CompanySettingType.CoreBaseCurrency, baseCurrency, 0, company.ActorCompanyId, 0);
                        if (!result.Success)
                        {
                            errors = true;
                            string message = GetText(1918, "Kunde inte lägga till basvaluta till defaultföretag för licens") + " " + license.LicenseNr;
                            SysLogManager.LogError<_default>(new SoeGeneralException(message, result.Exception, this.ToString()));
                        }

                        #endregion

                        #region LIBER - Autocopy

                        if (liber)
                        {
                            List<Company> templateCompanies = cm.GetTemplateCompanies(currentLicenseId);
                            if (!templateCompanies.IsNullOrEmpty())
                            {
                                Company templateCompany = templateCompanies.FirstOrDefault();
                                if (templateCompany != null)
                                    cm.CopyAllFromTemplateCompany(templateCompany.ActorCompanyId, company.ActorCompanyId, user.UserId, update: false, liberCopy: true);
                            }
                        }

                        #endregion

                        if (errors)
                            RedirectToSelf("SAVED_WITH_ERRORS", postBackUrlQs);
                        else
                            RedirectToSelf("SAVED", postBackUrlQs);
                    }
                    else
                    {
                        if (result.ErrorNumber == (int)ActionResultSave.SupportLicenseAlreadyExists)
                            RedirectToSelf("NOTSAVED_SUPPORTEXISTS", true);
                        else
                            RedirectToSelf("NOTSAVED", true);
                    }
                }
                else
                {
                    if (license.LicenseNr != licenseNo)
                    {
                        //Validation: Company not already exist
                        if (lm.LicenseExist(licenseNo))
                            RedirectToSelf("EXIST", true);
                    }

                    //Update license
                    license.LicenseNr = licenseNo;
                    license.Name = name;
                    license.OrgNr = orgNr;
                    license.ConcurrentUsers = concurrentUsers;
                    license.MaxNrOfUsers = maxNrOfUsers;
                    license.MaxNrOfEmployees = maxNrOfEmployees;
                    license.MaxNrOfMobileUsers = maxNrOfMobileUsers;
                    license.NrOfCompanies = nrOfCompanies;
                    license.TerminationDate = terminationDate;
                    license.LegalName = legalName;
                    license.SysServerId = sysServerId != 0 ? sysServerId : (int?)null;

                    bool errors = false;

                    ActionResult result = lm.UpdateLicense(license);
                    if (result.Success)
                    {
                        #region LicenseArticle

                        if (!SaveLicenseArticles().Success)
                            errors = true;

                        #endregion

                        if (errors)
                            RedirectToSelf("UPDATED_WITHERRORS", true);
                        else
                            RedirectToSelf("UPDATED", true);
                    }
                    else
                    {
                        if (result.ErrorNumber == (int)ActionResultSave.SupportLicenseAlreadyExists)
                            RedirectToSelf("NOTUPDATED_SUPPORTEXISTS", true);
                        else
                            RedirectToSelf("NOTUPDATED", true);
                    }
                }
            }

            RedirectToSelf("BADINPUT", true);
        }

        private ActionResult SaveLicenseArticles()
        {
            bool modifySysXEArticles = StringUtility.GetBool(F["ModifySysXEArticles"]);
            if (!modifySysXEArticles)
                return new ActionResult(true);

            List<int> checkedSysXEArticleIds = GetCheckedLicenseFeatures();
            List<int> deletedSysXEArticleIds = new List<int>();

            var result = lm.SaveLicenseArticles(checkedSysXEArticleIds, license.LicenseId, out deletedSysXEArticleIds);
            if (result.Success)
            {
                if (checkedSysXEArticleIds.Count > 0)
                {
                    //Option not to delete existing features
                    bool deleteSysXEArticles = StringUtility.GetBool(F["DeleteSysXEArticles"]);
                    if (!deleteSysXEArticles)
                        deletedSysXEArticleIds.Clear();

                    Dictionary<int, int> featuresDict = fm.GetSysXEArticleFeaturesDict(checkedSysXEArticleIds);
                    Dictionary<int, int> deletedFeaturesDict = fm.GetSysXEArticleFeaturesDict(deletedSysXEArticleIds);

                    if (featuresDict.Count > 0)
                    {
                        result = fm.SaveLicensePermissions(license.LicenseId, featuresDict, deletedFeaturesDict);
                        if (!result.Success)
                        {
                            string message = GetText(5016, "Behörigheter enligt SoftOne Artiklar kunde inte sparas för licens") + " " + license.LicenseNr;
                            SysLogManager.LogError<_default>(new SoeGeneralException(message, result.Exception, this.ToString()));
                        }
                    }
                }
            }
            else
            {
                string message = GetText(5000, "SoftOne Artiklar kunde inte sparas för licens") + " " + license.LicenseNr;
                SysLogManager.LogError<_default>(new SoeGeneralException(message, result.Exception, this.ToString()));
            }

            return result;
        }

        private List<int> GetCheckedLicenseFeatures()
        {
            List<int> sysXEArticleIds = new List<int>();
            foreach (SysXEArticle sysXEArticle in sysXEArticles)
            {
                string id = "SysXEArticle" + "_" + sysXEArticle.SysXEArticleId;
                if (StringUtility.GetBool(F[id]))
                    sysXEArticleIds.Add(sysXEArticle.SysXEArticleId);
            }
            return sysXEArticleIds;
        }

        protected override void Delete()
        {
            ActionResult result = lm.DeleteLicense(license);
            if (result.Success)
                RedirectToSelf("DELETED", false, true);
            else
                RedirectToSelf("NOTDELETED", true);
        }

        #endregion

        #region Help-methods

        private string GetBaseQS(License license = null, string prefix = "?")
        {
            if (license == null)
                license = this.license;

            return String.Format("{0}license={1}&licenseNr={2}", prefix, license.LicenseId, license.LicenseNr);
        }

        #endregion
    }
}
