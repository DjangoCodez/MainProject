using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util.Exceptions;
using SoftOne.Soe.Web.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.HtmlControls;

namespace SoftOne.Soe.Web.soe.manage.users.edit.usermapping
{
    public partial class _default : PageBase
    {
        #region Variables

        private CompanyManager cm;
        private UserManager um;

        private User user;
        private Company company;
        private List<UserCompanyRole> userCompanyRoles;
        private List<UserCompanyRole> currentUserCompanyRoles;
        private Dictionary<Company, List<Role>> validCompanyAndRolesDict;

        #endregion

        public bool IsAuthorized
        {
            get
            {
                if (user == null)
                    return false;

                //Rule 1: Same User
                if (UserId == user.UserId)
                    return true;

                //Rule 2: Administrators on SupportLicense
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
            this.Feature = Feature.Manage_Users_Edit_UserMapping;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            cm = new CompanyManager(ParameterObject);
            um = new UserManager(ParameterObject);

            //Mandatory parameters
            if (Int32.TryParse(QS["company"], out int actorCompanyId))
            {
                company = cm.GetCompany(actorCompanyId, true);
                if (company == null)
                    throw new SoeEntityNotFoundException("Company", this.ToString());
            }
            else
                throw new SoeQuerystringException("company", this.ToString());

            if (Int32.TryParse(QS["user"], out int userId))
            {
                user = um.GetUser(userId, loadUserCompanyRole: true, loadInactive: true);
                if (user == null)
                    throw new SoeEntityNotFoundException("User", this.ToString());
                if (user.UserCompanyRole == null)
                    throw new SoeEntityNotFoundException("UserCompanyRole", this.ToString());
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
            LoadValidCompanyAndRoles();

            if (Form1.IsPosted)
            {
                Save();
            }

            #endregion

            #region Populate

            LoadUserCompanyRoles();
            RenderCompanyAndRole();

            #endregion

            #region MessageFromSelf

            if (!String.IsNullOrEmpty(MessageFromSelf))
            {
                if (MessageFromSelf == "SAVED")
                    Form1.MessageSuccess = GetText(1530, "Användare kopplat till företag/roller");
                else if (MessageFromSelf == "SAVED_DEFAULTROLECHANGED")
                    Form1.MessageSuccess = GetText(1530, "Användare kopplat till företag/roller") + ". " + GetText(5302, "Förvald roll ändrad");
                else if (MessageFromSelf == "NOTSAVED")
                    Form1.MessageError = GetText(1621, "Avändare kunde inte kopplas till företag/roller");
                else if (MessageFromSelf == "MANDATORY_ROLE")
                    Form1.MessageError = GetText(5301, "Kan inte ta bort alla roller för användaren. Ta bort användaren istället");
            }

            #endregion

            #region Navigation

            //Form1.AddLink(GetText(1063, "Redigera användare"), "/soe/manage/users/edit/?user=" + user.UserId + "&license=" + SoeCompany.License.LicenseId + "&company=" + SoeCompany.ActorCompanyId,
            //    Feature.Manage_Users_Edit, Permission.Readonly);

            #endregion
        }

        #region Action-methods

        protected override void Save()
        {
            //Build a Dictionary for each Company that contains a list with checked Roles
            var selectedCompanyAndRolesDict = new Dictionary<int, List<int>>();
            int nrOfSelectedRoles = 0;

            foreach (var pair in this.validCompanyAndRolesDict)
            {
                Company validCompany = pair.Key;
                List<Role> validRoles = pair.Value;
                List<int> roleIds = new List<int>();

                foreach (Role validRole in validRoles)
                {
                    bool isSelected = StringUtility.GetBool(F[GetCheckId(validCompany, validRole)]);
                    if (isSelected)
                    {
                        roleIds.Add(validRole.RoleId);
                        nrOfSelectedRoles++;
                    }
                }

                selectedCompanyAndRolesDict.Add(validCompany.ActorCompanyId, roleIds);
            }

            if (nrOfSelectedRoles == 0)
                RedirectToSelf("MANDATORY_ROLE", true);

            //Deprecated
            RedirectToSelf("NOTSAVED", true);
        }

        #endregion

        #region Help methods

        private void LoadValidCompanyAndRoles()
        {
            this.validCompanyAndRolesDict = cm.GetValidCompanyAndRoles(IsAdmin, UserId, SoeUser.LicenseId);
        }

        private void LoadUserCompanyRoles()
        {
            this.userCompanyRoles = user.UserCompanyRole.ToList();
            this.currentUserCompanyRoles = um.GetUserCompanyRolesByUser(UserId);
        }

        private void RenderCompanyAndRole()
        {
            foreach (var pair in this.validCompanyAndRolesDict)
            {
                Company validCompany = pair.Key;
                List<Role> validRoles = pair.Value;

                if (validRoles.IsNullOrEmpty())
                    continue;

                #region Prefix

                HtmlTableRow tRow;
                HtmlTableCell tCell;

                var div = new HtmlGenericControl("div");
                var fieldset = new HtmlGenericControl("fieldset");
                var legend = new HtmlGenericControl("legend")
                {
                    InnerText = validCompany.Name,
                };
                fieldset.Controls.Add(legend);

                var tableCompany = new HtmlTable();
                tableCompany.CellSpacing = 2;

                #endregion

                foreach (var validRole in validRoles)
                {
                    #region Role

                    if (!HasCurrentUserPermissionToSee(validCompany, validRole))
                        continue;

                    bool readOnly = !HasCurrentUserPermissionToModify(validCompany, validRole, out string readOnlyInfo);

                    tRow = new HtmlTableRow();

                    //Role
                    tCell = new HtmlTableCell();
                    if (HasRolePermission(Feature.Manage_Roles_Edit, Permission.Readonly))
                    {
                        var link = new Link()
                        {
                            ID = GetRoleID(validCompany, validRole),
                            Href = GetRoleLink(validCompany.ActorCompanyId, validRole.RoleId),
                            Value = validRole.Name,
                            Alt = validRole.Name,
                        };
                        tCell.Controls.Add(link);
                    }
                    else
                    {
                        var text = new Text()
                        {
                            ID = GetRoleID(validCompany, validRole),
                            LabelSetting = validRole.Name,
                            FitInTable = true,
                        };
                        tCell.Controls.Add(text);
                    }
                    tCell.Attributes.Add("style", "width:200px");
                    tRow.Controls.Add(tCell);

                    //Check
                    tCell = new HtmlTableCell();
                    var check = new CheckBoxEntry()
                    {
                        ID = GetCheckId(validCompany, validRole),
                        DisableSettings = true,
                        FitInTable = true,
                        HideLabel = true,
                        Value = HasSelectedUserCompanyRole(validCompany, validRole).ToString().ToString(),
                        ReadOnly = readOnly,
                        InfoText = readOnlyInfo,
                    };
                    tCell.Controls.Add(check);
                    tRow.Controls.Add(tCell);

                    tableCompany.Rows.Add(tRow);

                    #endregion
                }

                #region Postfix

                fieldset.Controls.Add(tableCompany);
                div.Controls.Add(fieldset);
                CompanyRoleMapping.Controls.Add(div);

                #endregion
            }
        }

        private bool HasSelectedUserCompanyRole(Company company, Role role)
        {
            return um.HasUserCompanyRole(this.userCompanyRoles, this.user.ToDTO(), company.ToCompanyDTO(), role);
        }

        private bool HasCurrentUserCompanyRole(Company company, Role role)
        {
            return um.HasUserCompanyRole(this.currentUserCompanyRoles, SoeUser, company.ToCompanyDTO(), role);
        }

        /// <summary>
        /// Can see Role checkbox if either:
        /// a) Is Admin
        /// b) Current or selected User has UserCompanyRole
        /// </summary>
        /// <returns>True if current User has permission to see checkbox for given Company and Role</returns>
        private bool HasCurrentUserPermissionToSee(Company company, Role role)
        {
            if (IsAdmin)
                return true;
            if (HasCurrentUserCompanyRole(company, role))
                return true;
            if (HasSelectedUserCompanyRole(company, role))
                return true;

            return false;
        }

        /// <summary>
        /// Cannot modify Role checkbox if
        /// a) Is current login:
        /// 
        /// Can modify Role checkbox if either:
        /// a) Is Admin
        /// b) Current User has UserCompanyRole
        /// </summary>
        /// <returns>True if current User has permission to modify checkbox for given Company and Role</returns>
        private bool HasCurrentUserPermissionToModify(Company company, Role role, out string message)
        {
            message = "";

            if (IsAdmin)
                return true;
            if (HasCurrentUserCompanyRole(company, role))
                return true;

            message = GetText(5692, "Kan ej modifiera en roll du inte själv har");
            return false;
        }

        private string GetCheckId(Company company, Role role)
        {
            return company.ActorCompanyId + "_" + role.RoleId;
        }

        private string GetRoleID(Company company, Role role)
        {
            return GetID("Role", company, role);
        }

        private string GetID(string prefix, Company company, Role role)
        {
            return prefix + "_" + company.ActorCompanyId + "_" + role.RoleId;
        }

        private string GetRoleLink(int actorCompanyId, int roleId)
        {
            return String.Format("/soe/{0}/roles/edit/?license={1}&licenseNr={2}&company={3}&role={4}", SoeModule.Manage.ToString().ToLower(), SoeLicense.LicenseId, SoeLicense.LicenseNr, actorCompanyId, roleId);
        }

        #endregion
    }
}
