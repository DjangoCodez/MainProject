using Soe.WebApi.Extensions;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Soe.WebApi.Controllers.Manage
{
    [RoutePrefix("Manage/User")]
    public class UserController : SoeApiController
    {
        #region Variables

        private readonly ActorManager am;
        private readonly AccountManager accm;
        private readonly CompanyManager cm;
        private readonly UserManager um;
        private readonly FeatureManager fm;

        #endregion

        #region Constructor

        public UserController(ActorManager am, CompanyManager cm, UserManager um, AccountManager accm, FeatureManager fm)
        {
            this.am = am;
            this.cm = cm;
            this.um = um;
            this.accm = accm;
            this.fm = fm;
        }

        #endregion

        #region User

        [HttpGet]
        [Route("ForOrigins/{includeEmployeeCategories:bool}")]
        public IHttpActionResult GetUsersForOrigin(bool includeEmployeeCategories)
        {
            return Content(HttpStatusCode.OK, um.GetUsersByCompany(base.ActorCompanyId, base.RoleId, base.UserId,
                setDefaultRoleName: false,
                active: true,
                includeEnded: false,
                skipNonEmployeeUsers: false,
                includeEmployeesWithSameAccountOnAttestRole: false,
                includeEmployeesWithSameAccount: false,
                setEmployeeCategories: includeEmployeeCategories
                ).ToForOriginDTOs());
        }

        [HttpGet]
        [Route("")]
        public IHttpActionResult GetUsers(HttpRequestMessage message)
        {
            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
                return Content(HttpStatusCode.OK, um.GetUsersByCompanyDict(base.ActorCompanyId, base.RoleId, base.UserId, message.GetBoolValueFromQS("addEmptyRow"), message.GetBoolValueFromQS("includeKey"), message.GetBoolValueFromQS("useFullName"), message.GetBoolValueFromQS("includeLoginName")).ToSmallGenericTypes());

            //if (!fm.HasRolePermission(Feature.Manage_Users, Permission.Readonly, base.RoleId, base.ActorCompanyId))
            //    return Content(HttpStatusCode.Forbidden, new List<UserSmallDTO>());

            return Content(HttpStatusCode.OK, um.GetUsersByCompany(base.ActorCompanyId, base.RoleId, base.UserId,
                setDefaultRoleName: message.GetBoolValueFromQS("setDefaultRoleName"),
                active: message.GetNullableBoolValueFromQS("active"),
                includeEnded: message.GetBoolValueFromQS("showEnded"),
                skipNonEmployeeUsers: message.GetBoolValueFromQS("skipNonEmployeeUsers"),
                includeEmployeesWithSameAccountOnAttestRole: message.GetBoolValueFromQS("includeEmployeesWithSameAccountOnAttestRole"),
                includeEmployeesWithSameAccount: message.GetBoolValueFromQS("includeEmployeesWithSameAccount"),
                setEmployeeCategories: message.GetBoolValueFromQS("includeEmployeeCategories")
                ).ToSmallDTOs());
        }

        [HttpGet]
        [Route("Users/{userIds}/{includeInactive:bool}")]
        public IHttpActionResult GetUsers(string userIds, bool includeInactive)
        {
            List<int> userIdsList = StringUtility.SplitNumericList(userIds);

            List<User> users = um.GetUsers(
                        userIdsList,
                        includeInactive ? (bool?)null : true
                        ).Where(w => w.LicenseId == base.LicenseId).ToList();

            List<UserGridDTO> dtos = new List<UserGridDTO>();
            foreach (var user in users)
            {
                if (user.LicenseId == base.LicenseId)
                {
                    UserGridDTO dto = user.ToGridDTO();
                    if (dto != null)
                        dtos.Add(dto);
                }
            }

            if (userIdsList.Count == 1 && userIdsList.First() == base.UserId)
                return Content(HttpStatusCode.OK, dtos);

            if (!fm.HasRolePermission(Feature.Manage_Users, Permission.Readonly, base.RoleId, base.ActorCompanyId))
                return Content(HttpStatusCode.Forbidden, new List<UserGridDTO>());

            return Content(HttpStatusCode.OK, dtos);
        }

        [HttpGet]
        [Route("ByLicense/{licenseId:int}/{setDefaultRoleName:bool}/{includeInactive:bool}/{includeEnded:bool}/{includeNotStarted:bool}")]
        public IHttpActionResult GetUsersByLicense(int licenseId, bool setDefaultRoleName, bool includeInactive, bool includeEnded, bool includeNotStarted)
        {
            return Content(HttpStatusCode.OK, um.GetUsersByLicense(licenseId, base.ActorCompanyId, base.RoleId, base.UserId,
                setDefaultRoleName: setDefaultRoleName,
                active: includeInactive ? (bool?)null : true,
                includeEnded: includeEnded,
                includeNotStarted: includeNotStarted
                ).ToGridDTOs());
        }

        [HttpGet]
        [Route("ByCompany/{actorCompanyId:int}/{setDefaultRoleName:bool}/{includeInactive:bool}/{includeEnded:bool}/{includeNotStarted:bool}/{userCompanyRoleDate}")]
        public IHttpActionResult GetUsersByCompany(int actorCompanyId, bool setDefaultRoleName, bool includeInactive, bool includeEnded, bool includeNotStarted, string userCompanyRoleDate)
        {
            if (!fm.HasRolePermission(Feature.Manage_Users, Permission.Readonly, base.RoleId, base.ActorCompanyId))
                return Content(HttpStatusCode.Forbidden, new List<UserGridDTO>());

            return Content(HttpStatusCode.OK, um.GetUsersByCompany(actorCompanyId, base.RoleId, base.UserId,
                setDefaultRoleName: setDefaultRoleName,
                active: includeInactive ? (bool?)null : true,
                includeEnded: includeEnded,
                includeNotStarted: includeNotStarted,
                userCompanyRoleDate: BuildDateTimeFromString(userCompanyRoleDate, true),
                setSoftOneIdLoginName: true
                ).ToGridDTOs());
        }

        [HttpGet]
        [Route("ByCompany/{actorCompanyId:int}/{setDefaultRoleName:bool}/{includeInactive:bool}/{includeEnded:bool}/{userCompanyRoleDate}")]
        public IHttpActionResult GetUsersByCompanyDate(int actorCompanyId, bool setDefaultRoleName, bool includeInactive, bool includeEnded, string userCompanyRoleDate)
        {
            if (!fm.HasRolePermission(Feature.Manage_Users, Permission.Readonly, base.RoleId, base.ActorCompanyId))
                return Content(HttpStatusCode.Forbidden, new List<UserGridDTO>());

            return Content(HttpStatusCode.OK, um.GetUsersByCompany(
                actorCompanyId,
                base.RoleId,
                base.UserId,
                setDefaultRoleName: setDefaultRoleName,
                active: includeInactive ? (bool?)null : true,
                includeEnded: includeEnded,
                userCompanyRoleDate: BuildDateTimeFromString(userCompanyRoleDate, true)
                ).ToGridDTOs());
        }

        [HttpGet]
        [Route("ByRole/{roleId:int}/{setDefaultRoleName:bool}/{includeInactive:bool}/{includeEnded:bool}/{includeNotStarted:bool}")]
        public IHttpActionResult GetUsersByRole(int roleId, bool setDefaultRoleName, bool includeInactive, bool includeEnded, bool includeNotStarted)
        {
            if (!fm.HasRolePermission(Feature.Manage_Users, Permission.Readonly, base.RoleId, base.ActorCompanyId))
                return Content(HttpStatusCode.Forbidden, new List<UserGridDTO>());

            return Content(HttpStatusCode.OK, um.GetUsersByRole(
                base.ActorCompanyId,
                roleId,
                base.UserId,
                setDefaultRoleName: setDefaultRoleName,
                active: includeInactive ? (bool?)null : true,
                includeEnded: includeEnded,
                includeNotStarted: includeNotStarted
                ).ToGridDTOs());
        }

        [HttpGet]
        [Route("GetUserLicenseInfo")]
        public IHttpActionResult GetUserLicenseInfo()
        {
            return Content(HttpStatusCode.OK, um.GetNrOfUsersAndMaxByLicense(base.LicenseId, true));
        }

        [HttpGet]
        [Route("CompaniesWithSupportLogin/{selectedLicenseId:int}")]
        public IHttpActionResult CompaniesWithSupportLogin(int selectedLicenseId)
        {
            return Content(HttpStatusCode.OK, cm.GetSupportLoginAllowedCompanyIdsByLicenseId(selectedLicenseId));
        }

        [HttpGet]
        [Route("{userId:int}")]
        public IHttpActionResult GetUser(int userId)
        {
            UserSmallDTO user = um.GetUser(userId).ToSmallDTO();
            if (user.LicenseId == base.LicenseId)
                return Content(HttpStatusCode.OK, user);
            else
                return Content(HttpStatusCode.Forbidden, new UserSmallDTO());
        }

        [HttpGet]
        [Route("Current/")]
        public IHttpActionResult GetCurrentUser()
        {
            return Content(HttpStatusCode.OK, um.GetUser(base.UserId).ToSmallDTO());
        }

        [HttpGet]
        [Route("UsernamesWithLogin/")]
        public IHttpActionResult GetUserNamesWithLogin()
        {
            return Content(HttpStatusCode.OK, um.GetUsersWithNameAndLogin(base.ActorCompanyId));
        }

        [HttpGet]
        [Route("UsersWithoutEmployees/{companyId:int}/{includeUserId:int}/{addEmptyRow:bool}")]
        public IHttpActionResult GetUsersWithoutEmployees(int companyId, int includeUserId, bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, um.GetUsersWithoutEmployeesDict(base.LicenseId, companyId, includeUserId.ToNullable(), addEmptyRow, false, false, false).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("UserForEdit/{userId:int}/{currentUserId:int}")]
        public IHttpActionResult GetUserForEdit(int userId, int currentUserId)
        {
            return Content(HttpStatusCode.OK, am.GetEmployeeUserDTOFromUser(userId, base.ActorCompanyId, currentUserId != 0 ? currentUserId : (int?)null, loadExternalAuthId: true));
        }

        [HttpGet]
        [Route("AccountIdsFromHierarchyByUser/{dateFrom}/{dateTo}/{useMaxAccountDimId:bool}/{includeVirtualParented:bool}/{includeOnlyChildrenOneLevel:bool}/{onlyDefaultAccounts:bool}/{useEmployeeAccountIfNoAttestRole:bool}/{includeAbstract:bool}/{employeeId:int}")]
        public IHttpActionResult GetAccountIdsFromHierarchyByUser(string dateFrom, string dateTo, bool useMaxAccountDimId, bool includeVirtualParented, bool includeOnlyChildrenOneLevel, bool onlyDefaultAccounts, bool useEmployeeAccountIfNoAttestRole, bool includeAbstract, int employeeId)
        {
            AccountHierarchyInput input = AccountHierarchyInput.GetInstance();
            input.AddParamValue(AccountHierarchyParamType.UseDefaultEmployeeAccountDimSelector, useMaxAccountDimId);
            input.AddParamValue(AccountHierarchyParamType.IncludeVirtualParented, includeVirtualParented);
            input.AddParamValue(AccountHierarchyParamType.IncludeOnlyChildrenOneLevel, includeOnlyChildrenOneLevel);
            input.AddParamValue(AccountHierarchyParamType.OnlyDefaultAccounts, onlyDefaultAccounts);
            input.AddParamValue(AccountHierarchyParamType.UseEmployeeAccountIfNoAttestRole, useEmployeeAccountIfNoAttestRole);
            input.AddParamValue(AccountHierarchyParamType.IncludeAbstract, includeAbstract);

            return Content(HttpStatusCode.OK, accm.GetAccountIdsFromHierarchyByUser(base.ActorCompanyId, base.UserId, BuildDateTimeFromString(dateFrom, true), BuildDateTimeFromString(dateTo, true), input, employeeId));
        }

        [HttpGet]
        [Route("AccountsFromHierarchyByUser/{dateFrom}/{dateTo}/{useMaxAccountDimId:bool}/{includeVirtualParented:bool}/{includeOnlyChildrenOneLevel:bool}/{onlyDefaultAccounts:bool}/{useEmployeeAccountIfNoAttestRole:bool}/{companyId:int}")]
        public IHttpActionResult GetAccountsFromHierarchyByUser(string dateFrom, string dateTo, bool useMaxAccountDimId, bool includeVirtualParented, bool includeOnlyChildrenOneLevel, bool onlyDefaultAccounts, bool useEmployeeAccountIfNoAttestRole, int companyId)
        {
            if (companyId == 0)
                companyId = base.ActorCompanyId;
            AccountHierarchyInput input = AccountHierarchyInput.GetInstance();
            input.AddParamValue(AccountHierarchyParamType.UseDefaultEmployeeAccountDimSelector, useMaxAccountDimId);
            input.AddParamValue(AccountHierarchyParamType.IncludeVirtualParented, includeVirtualParented);
            input.AddParamValue(AccountHierarchyParamType.IncludeOnlyChildrenOneLevel, includeOnlyChildrenOneLevel);
            input.AddParamValue(AccountHierarchyParamType.OnlyDefaultAccounts, onlyDefaultAccounts);
            input.AddParamValue(AccountHierarchyParamType.UseEmployeeAccountIfNoAttestRole, useEmployeeAccountIfNoAttestRole);

            return Content(HttpStatusCode.OK, accm.GetAccountsFromHierarchyByUser(companyId, base.UserId, BuildDateTimeFromString(dateFrom, true), BuildDateTimeFromString(dateTo, true), input));
        }

        [HttpGet]
        [Route("AccountsFromHierarchyByUserSetting/{dateFrom}/{dateTo}/{useMaxAccountDimId:bool}/{includeVirtualParented:bool}/{includeOnlyChildrenOneLevel:bool}/{useDefaultEmployeeAccountDimEmployee:bool}")]
        public IHttpActionResult GetAccountsFromHierarchyByUserSetting(string dateFrom, string dateTo, bool useMaxAccountDimId, bool includeVirtualParented, bool includeOnlyChildrenOneLevel, bool useDefaultEmployeeAccountDimEmployee)
        {
            AccountHierarchyInput input = AccountHierarchyInput.GetInstance();
            input.AddParamValue(AccountHierarchyParamType.UseDefaultEmployeeAccountDimEmployee, true); //Always true for performance issue
            input.AddParamValue(AccountHierarchyParamType.UseDefaultEmployeeAccountDimSelector, useMaxAccountDimId);
            input.AddParamValue(AccountHierarchyParamType.IncludeVirtualParented, includeVirtualParented);
            input.AddParamValue(AccountHierarchyParamType.IncludeOnlyChildrenOneLevel, includeOnlyChildrenOneLevel);

            List<AccountDTO> accounts = accm.GetAccountsFromHierarchyByUserSetting(base.ActorCompanyId, base.RoleId, base.UserId, BuildDateTimeFromString(dateFrom, true), BuildDateTimeFromString(dateTo, true), doFilterByDefaultEmployeeAccountDimEmployee: useDefaultEmployeeAccountDimEmployee, input: input);
            return Content(HttpStatusCode.OK, accounts);
        }

        [HttpPost]
        [Route("DefaultRole")]
        public IHttpActionResult GetDefaultRoleId(GetDefaultRoleModel model)
        {
            return Content(HttpStatusCode.OK, um.GetDefaultRoleId(base.ActorCompanyId, model.UserId, model.Date, model.UserCompanyRoles));
        }

        [HttpPost]
        [Route("ValidateSaveUser")]
        public IHttpActionResult ValidateSaveUser(ValidateSaveEmployeeModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, am.ValidateSaveUser(model.EmployeeUser, model.ContactAdresses));
        }

        [HttpGet]
        [Route("ValidateInactivateUser/{userId:int}")]
        public IHttpActionResult ValidateInactivateUser(int userId)
        {
            return Content(HttpStatusCode.OK, am.ValidateDeleteUser(userId));
        }

        [HttpGet]
        [Route("ValidateDeleteUser/{userId:int}")]
        public IHttpActionResult ValidateDeleteUser(int userId)
        {
            return Content(HttpStatusCode.OK, am.ValidateDeleteUser(userId));
        }

        [HttpGet]
        [Route("ValidateImmediateDeleteUser/{userId:int}")]
        public IHttpActionResult ValidateImmediateDeleteUser(int userId)
        {
            return Content(HttpStatusCode.OK, am.ValidateDeleteUser(userId));
        }

        [HttpPost]
        [Route("SendActivationEmail")]
        public IHttpActionResult SendActivationEmail(ListIntModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, um.SendActivationEmail(model.Numbers));
        }

        [HttpPost]
        [Route("SendForgottenUsername")]
        public IHttpActionResult SendForgottenUsername(ListIntModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, um.SendForgottenUsername(model.Numbers));
        }

        [HttpPost]
        [Route("Delete")]
        public IHttpActionResult DeleteUser(DeleteUserDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, um.DeleteUser(model, base.ActorCompanyId));
        }

        #endregion

        #region UserCompanyRoleDelegateHistory

        [HttpGet]
        [Route("UserCompanyRoleDelegateHistory/{userId:int}")]
        public IHttpActionResult GetUserCompanyRoleDelegateHistoryForUser(int userId)
        {
            return Content(HttpStatusCode.OK, um.GetUserCompanyRoleDelegateHistoryForUser(base.ActorCompanyId, base.RoleId, base.UserId, userId));
        }

        [HttpGet]
        [Route("UserCompanyRoleDelegateHistory/TargetUser/{userId:int}/{userCondition}")]
        public IHttpActionResult SearchTargetUserForDelegation(int userId, string userCondition)
        {
            return Content(HttpStatusCode.OK, um.SearchTargetUserForDelegation(base.ActorCompanyId, userId, base.UserId, userCondition));
        }

        [HttpPost]
        [Route("UserCompanyRoleDelegateHistory/Save")]
        public IHttpActionResult SaveUserCompanyRoleDelegateHistory(SaveUserCompanyRoleDelegateHistoryModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, um.SaveUserCompanyRoleDelegation(model.TargetUser, base.ActorCompanyId, model.SourceUserId, base.UserId));
        }

        [HttpDelete]
        [Route("UserCompanyRoleDelegateHistory/Delete/{userCompanyRoleDelegateHistoryHeadId:int}")]
        public IHttpActionResult DeleteUserCompanyRoleDelegateHistory(int userCompanyRoleDelegateHistoryHeadId)
        {
            return Content(HttpStatusCode.OK, um.DeleteUserCompanyRoleDelegation(userCompanyRoleDelegateHistoryHeadId, base.ActorCompanyId));
        }

        #endregion

        #region UserReplacement

        [HttpGet]
        [Route("UserReplacement/{type:int}/{originUserId:int}")]
        public IHttpActionResult GetUserReplacement(int type, int originUserId)
        {
            return Content(HttpStatusCode.OK, am.GetUserReplacement(base.ActorCompanyId, (UserReplacementType)type, originUserId).ToDTO());
        }

        #endregion

        #region UserSelection

        [HttpGet]
        [Route("UserSelection/List/{type:int}")]
        public IHttpActionResult GetUserSelections(int type)
        {
            return Content(HttpStatusCode.OK, um.GetUserSelectionsDict((UserSelectionType)type, base.UserId, base.RoleId, base.ActorCompanyId).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("UserSelection/{userSelectionId:int}")]
        public IHttpActionResult GetUserSelection(int userSelectionId)
        {
            return Content(HttpStatusCode.OK, um.GetUserSelection(userSelectionId, loadAccess: true).ToDTO());
        }

        [HttpPost]
        [Route("UserSelection")]
        public IHttpActionResult SaveUserSelection(UserSelectionDTO dto)
        {
            return Content(HttpStatusCode.Accepted, um.SaveUserSelection(dto));
        }

        [HttpDelete]
        [Route("UserSelection/{userSelectionId:int}")]
        public IHttpActionResult DeleteUserSelection(int userSelectionId)
        {
            return Content(HttpStatusCode.Accepted, um.DeleteUserSelection(userSelectionId));
        }

        #endregion
    }
}