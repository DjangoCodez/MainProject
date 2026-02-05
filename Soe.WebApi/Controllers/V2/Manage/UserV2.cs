using Soe.WebApi.Controllers;
using Soe.WebApi.Extensions;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Soe.WebApi.V2.Manage
{
    [RoutePrefix("V2/Manage/System")]
    public class UserV2Controller : SoeApiController
    {
        #region Variables

        private readonly ActorManager am;
        private readonly AccountManager accm;
        private readonly CompanyManager cm;
        private readonly UserManager um;

        #endregion

        #region Constructor

        public UserV2Controller(ActorManager am, CompanyManager cm, UserManager um, AccountManager accm)
        {
            this.am = am;
            this.cm = cm;
            this.um = um;
            this.accm = accm;
        }

        #endregion

        #region User

        [HttpGet]
        [Route("")]
        public IHttpActionResult GetSmallGenericUsers(bool addEmptyRow,bool includeKey,bool useFullName,bool includeLoginName)
        {
                return Content(HttpStatusCode.OK, um.GetUsersByCompanyDict(base.ActorCompanyId, base.RoleId, base.UserId, addEmptyRow, includeKey, useFullName, includeLoginName).ToSmallGenericTypes());

        }

        [HttpGet]
        [Route("")]
        public IHttpActionResult GetSmallDTOUsers(bool setDefaultRoleName,bool? active,bool? skipNonEmployeeUsers,bool? includeEmployeesWithSameAccountOnAttestRole,bool? includeEmployeeCategories,bool? showEnded)
        {
            return Content(HttpStatusCode.OK, um.GetUsersByCompany(base.ActorCompanyId, base.RoleId, base.UserId,
                setDefaultRoleName: setDefaultRoleName,
                active:active?? false,
                includeEnded: showEnded??false,
                skipNonEmployeeUsers: skipNonEmployeeUsers?? false,
                includeEmployeesWithSameAccountOnAttestRole: includeEmployeesWithSameAccountOnAttestRole??false,
                includeEmployeesWithSameAccount: includeEmployeesWithSameAccountOnAttestRole??false,
                setEmployeeCategories: includeEmployeeCategories??false
                ).ToSmallDTOs());
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
        [Route("ByCompany/{actorCompanyId:int}/{setDefaultRoleName:bool}/{includeInactive:bool}/{includeEnded:bool}/{includeNotStarted:bool}")]
        public IHttpActionResult GetUsersByCompany(int actorCompanyId, bool setDefaultRoleName, bool includeInactive, bool includeEnded, bool includeNotStarted)
        {
            return Content(HttpStatusCode.OK, um.GetUsersByCompany(actorCompanyId, base.RoleId, base.UserId,
                setDefaultRoleName: setDefaultRoleName,
                active: includeInactive ? (bool?)null : true,
                includeEnded: includeEnded,
                includeNotStarted: includeNotStarted
                ).ToGridDTOs());
        }

        [HttpGet]
        [Route("ByCompany/{actorCompanyId:int}/{setDefaultRoleName:bool}/{includeInactive:bool}/{includeEnded:bool}/{userCompanyRoleDate}")]
        public IHttpActionResult GetUsersByCompanyDate(int actorCompanyId, bool setDefaultRoleName, bool includeInactive, bool includeEnded, string userCompanyRoleDate)
        {
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
            return Content(HttpStatusCode.OK, um.GetUser(userId).ToSmallDTO());
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
        [Route("AccountIdsFromHierarchyByUser/{dateFrom}/{dateTo}/{useMaxAccountDimId:bool}/{includeVirtualParented:bool}/{includeOnlyChildrenOneLevel:bool}/{onlyDefaultAccounts:bool}/{useEmployeeAccountIfNoAttestRole:bool}/{includeAbstract:bool}")]
        public IHttpActionResult GetAccountIdsFromHierarchyByUser(string dateFrom, string dateTo, bool useMaxAccountDimId, bool includeVirtualParented, bool includeOnlyChildrenOneLevel, bool onlyDefaultAccounts, bool useEmployeeAccountIfNoAttestRole, bool includeAbstract)
        {
            AccountHierarchyInput input = AccountHierarchyInput.GetInstance();
            input.AddParamValue(AccountHierarchyParamType.UseDefaultEmployeeAccountDimSelector, useMaxAccountDimId);
            input.AddParamValue(AccountHierarchyParamType.IncludeVirtualParented, includeVirtualParented);
            input.AddParamValue(AccountHierarchyParamType.IncludeOnlyChildrenOneLevel, includeOnlyChildrenOneLevel);
            input.AddParamValue(AccountHierarchyParamType.OnlyDefaultAccounts, onlyDefaultAccounts);
            input.AddParamValue(AccountHierarchyParamType.UseEmployeeAccountIfNoAttestRole, useEmployeeAccountIfNoAttestRole);
            input.AddParamValue(AccountHierarchyParamType.IncludeAbstract, includeAbstract);

            return Content(HttpStatusCode.OK, accm.GetAccountIdsFromHierarchyByUser(base.ActorCompanyId, base.UserId, BuildDateTimeFromString(dateFrom, true), BuildDateTimeFromString(dateTo, true), input));
        }

        [HttpGet]
        [Route("AccountsFromHierarchyByUser/{dateFrom}/{dateTo}/{useMaxAccountDimId:bool}/{includeVirtualParented:bool}/{includeOnlyChildrenOneLevel:bool}/{onlyDefaultAccounts:bool}/{useEmployeeAccountIfNoAttestRole:bool}")]
        public IHttpActionResult GetAccountsFromHierarchyByUser(string dateFrom, string dateTo, bool useMaxAccountDimId, bool includeVirtualParented, bool includeOnlyChildrenOneLevel, bool onlyDefaultAccounts, bool useEmployeeAccountIfNoAttestRole)
        {
            AccountHierarchyInput input = AccountHierarchyInput.GetInstance();
            input.AddParamValue(AccountHierarchyParamType.UseDefaultEmployeeAccountDimSelector, useMaxAccountDimId);
            input.AddParamValue(AccountHierarchyParamType.IncludeVirtualParented, includeVirtualParented);
            input.AddParamValue(AccountHierarchyParamType.IncludeOnlyChildrenOneLevel, includeOnlyChildrenOneLevel);
            input.AddParamValue(AccountHierarchyParamType.OnlyDefaultAccounts, onlyDefaultAccounts);
            input.AddParamValue(AccountHierarchyParamType.UseEmployeeAccountIfNoAttestRole, useEmployeeAccountIfNoAttestRole);

            return Content(HttpStatusCode.OK, accm.GetAccountsFromHierarchyByUser(base.ActorCompanyId, base.UserId, BuildDateTimeFromString(dateFrom, true), BuildDateTimeFromString(dateTo, true), input));
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
    }
}