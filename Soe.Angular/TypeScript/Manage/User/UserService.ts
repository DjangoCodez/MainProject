import { IHttpService } from "../../Core/Services/HttpService";
import { Constants } from "../../Util/Constants";
import { UserCompanyRoleDelegateHistoryGridDTO, UserCompanyRoleDelegateHistoryUserDTO, UserGridDTO, UserReplacementDTO } from "../../Common/Models/UserDTO";
import { EmployeeUserDTO, UserRolesDTO, DeleteUserDTO } from "../../Common/Models/EmployeeUserDTO";
import { ContactAddressItemDTO } from "../../Common/Models/ContactAddressDTOs";
import { IActionResult } from "../../Scripts/TypeLite.Net4";
import { TermGroup_TrackChangesActionMethod } from "../../Util/CommonEnumerations";

export interface IUserService {

    // GET

    getContactAddressItems(actorId: number): ng.IPromise<any[]>;
    getUsers(userIds: string, includeInactive: boolean): ng.IPromise<UserGridDTO[]>;
    getUsersByLicense(licenseId: number, setDefaultRoleName: boolean, includeInactive: boolean, includeEnded: boolean, includeNotStarted: boolean): ng.IPromise<UserGridDTO[]>;
    getUsersByCompany(actorCompanyId: number, setDefaultRoleName: boolean, includeInactive: boolean, includeEnded: boolean, includeNotStarted: boolean, userCompanyRoleDate?: Date): ng.IPromise<UserGridDTO[]>;
    getUsersByRole(roleId: number, setDefaultRoleName: boolean, includeInactive: boolean, includeEnded: boolean, includeNotStarted: boolean): ng.IPromise<UserGridDTO[]>;
    getUserLicenseInfo(): ng.IPromise<any>;
    getCompaniesWithSupportLogin(selectedLicenseId: number): ng.IPromise<number[]>;
    getUserForEdit(userId: number, currentUserId: number): ng.IPromise<EmployeeUserDTO>;
    getUserCompanyRoleDelegateHistoryForUser(userId: number): ng.IPromise<UserCompanyRoleDelegateHistoryGridDTO[]>;
    getTargetUserForDelegation(userId: number, userCondition: string): ng.IPromise<UserCompanyRoleDelegateHistoryUserDTO>;
    validateInactivateUser(userId: number): ng.IPromise<IActionResult>;
    validateDeleteUser(userId: number): ng.IPromise<IActionResult>;
    validateImmediateDeleteUser(userId: number): ng.IPromise<IActionResult>;

    // POST

    validateSaveUser(employeeUser: EmployeeUserDTO, contactAddresses: any): ng.IPromise<IActionResult>
    saveEmployeeUser(employeeUser: EmployeeUserDTO, contactAddresses: ContactAddressItemDTO[], userReplacement: UserReplacementDTO, saveRoles: boolean, saveAttestRoles: boolean, userRoles: UserRolesDTO[]): ng.IPromise<IActionResult>
    sendActivationEmail(userIds: number[]): ng.IPromise<IActionResult>
    sendForgottenUsername(userIds: number[]): ng.IPromise<IActionResult>
    deleteUser(input: DeleteUserDTO): ng.IPromise<IActionResult>
    saveUserCompanyRoleDelegateHistory(targetUser: UserCompanyRoleDelegateHistoryUserDTO, sourceUserId: number): ng.IPromise<IActionResult>

    // DELETE
    deleteUserCompanyRoleDelegateHistory(userCompanyRoleDelegateHistoryHeadId: number): ng.IPromise<IActionResult>
}

export class UserService implements IUserService {

    //@ngInject
    constructor(private httpService: IHttpService, private $q: ng.IQService) { }

    // GET

    getContactAddressItems(actorId: number) {
        return this.httpService.get(Constants.WEBAPI_CORE_CONTACT_ADDRESSITEM + actorId, false).then(x => {
            return x.map(y => {
                let obj = new ContactAddressItemDTO();
                angular.extend(obj, y);
                return obj;
            })
        });
    }

    getUsers(userIds: string, includeInactive: boolean) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_USERS + userIds + "/" + includeInactive, false);
    }

    getUsersByLicense(licenseId: number, setDefaultRoleName: boolean, includeInactive: boolean, includeEnded: boolean, showNotStarted: boolean): ng.IPromise<UserGridDTO[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_USER_BY_LICENSE + licenseId + "/" + setDefaultRoleName + "/" + includeInactive + "/" + includeEnded + "/" + showNotStarted, false);
    }

    getUsersByCompany(actorCompanyId: number, setDefaultRoleName: boolean, includeInactive: boolean, includeEnded: boolean, showNotStarted: boolean, userCompanyRoleDate: Date = undefined): ng.IPromise<UserGridDTO[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_USER_BY_COMPANY + actorCompanyId + "/" + setDefaultRoleName + "/" + includeInactive + "/" + includeEnded + "/" + showNotStarted + "/" + (userCompanyRoleDate ? userCompanyRoleDate.toDateTimeString() : "null"), false);
    }

    getUsersByRole(roleId: number, setDefaultRoleName: boolean, includeInactive: boolean, includeEnded: boolean, showNotStarted: boolean): ng.IPromise<UserGridDTO[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_USER_BY_ROLE + roleId + "/" + setDefaultRoleName + "/" + includeInactive + "/" + includeEnded + "/" + showNotStarted, false);
    }

    getUserLicenseInfo(): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_USER_LICENSE_INFO, false);
    }

    getCompaniesWithSupportLogin(selectedLicenseId: number): ng.IPromise<number[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_USER_COMPANIES_WITH_SUPPORT_LOGIN + selectedLicenseId, false);
    }

    getUserForEdit(userId: number, currentUserId: number): ng.IPromise<EmployeeUserDTO> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_USER_FOR_EDIT + userId + "/" + currentUserId, false).then((x: EmployeeUserDTO) => {
            if (x) {
                var user: EmployeeUserDTO = new EmployeeUserDTO();
                angular.extend(user, x);
                user.fixDates();

                return user;
            } else {
                return x;
            }
        });
    }

    getUserCompanyRoleDelegateHistoryForUser(userId: number): ng.IPromise<UserCompanyRoleDelegateHistoryGridDTO[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_USER_USER_COMPANY_ROLE_DELEGATE_HISTORY + userId, false).then(x => {
            return x.map(y => {
                let obj = new UserCompanyRoleDelegateHistoryGridDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    getTargetUserForDelegation(userId: number, userCondition: string): ng.IPromise<UserCompanyRoleDelegateHistoryUserDTO> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_USER_USER_COMPANY_ROLE_DELEGATE_HISTORY_TARGET_USER + userId + "/" + userCondition, false).then(x => {
            if (!x)
                return null;

            let obj = new UserCompanyRoleDelegateHistoryUserDTO();
            angular.extend(obj, x);
            obj.setTypes();
            return obj;
        });
    }

    validateInactivateUser(userId: number): ng.IPromise<IActionResult> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_USER_VALIDATE_INACTIVATE + userId, false);
    }

    validateDeleteUser(userId: number): ng.IPromise<IActionResult> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_USER_VALIDATE_DELETE + userId, false);
    }

    validateImmediateDeleteUser(userId: number): ng.IPromise<IActionResult> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_USER_VALIDATE_IMMEDIATE_DELETE + userId, false);
    }


    // POST

    validateSaveUser(employeeUser: EmployeeUserDTO, contactAddresses: any) {
        var model = {
            employeeUser: employeeUser,
            contactAdresses: contactAddresses,
        };

        return this.httpService.post(Constants.WEBAPI_MANAGE_USER_VALIDATE_SAVE_USER, model);
    }

    saveEmployeeUser(employeeUser: EmployeeUserDTO, contactAddresses: ContactAddressItemDTO[], userReplacement: UserReplacementDTO, saveRoles: boolean, saveAttestRoles: boolean, userRoles: UserRolesDTO[]) {
        var model = {
            actionMethod: TermGroup_TrackChangesActionMethod.User_Save,
            employeeUser: employeeUser,
            contactAdresses: contactAddresses,
            userReplacement: userReplacement,
            saveRoles: saveRoles,
            saveAttestRoles: saveAttestRoles,
            userRoles: userRoles
        };

        return this.httpService.post(Constants.WEBAPI_TIME_EMPLOYEE, model);
    }

    sendActivationEmail(userIds: number[]): ng.IPromise<IActionResult> {
        var model = {
            numbers: userIds
        };

        return this.httpService.post(Constants.WEBAPI_MANAGE_USER_SEND_ACTIVATION_EMAIL, model);
    }

    sendForgottenUsername(userIds: number[]): ng.IPromise<IActionResult> {
        var model = {
            numbers: userIds
        };

        return this.httpService.post(Constants.WEBAPI_MANAGE_USER_SEND_FORGOTTEN_USERNAME, model);
    }

    deleteUser(input: DeleteUserDTO): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_MANAGE_USER_DELETE, input);
    }

    saveUserCompanyRoleDelegateHistory(targetUser: UserCompanyRoleDelegateHistoryUserDTO, sourceUserId: number): ng.IPromise<IActionResult> {
        var model = {
            targetUser: targetUser,
            sourceUserId: sourceUserId
        }
        return this.httpService.post(Constants.WEBAPI_MANAGE_USER_USER_COMPANY_ROLE_DELEGATE_HISTORY_SAVE, model);
    }

    // DELETE

    deleteUserCompanyRoleDelegateHistory(userCompanyRoleDelegateHistoryHeadId: number): ng.IPromise<IActionResult> {
        return this.httpService.delete(Constants.WEBAPI_MANAGE_USER_USER_COMPANY_ROLE_DELEGATE_HISTORY_DELETE + userCompanyRoleDelegateHistoryHeadId);
    }
}
