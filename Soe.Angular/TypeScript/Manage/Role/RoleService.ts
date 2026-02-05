import { RoleEditDTO, RoleGridDTO } from "../../Common/Models/RoleDTOs";
import { IHttpService } from "../../Core/Services/HttpService";
import { IActionResult, ISmallGenericType } from "../../Scripts/TypeLite.Net4";
import { Constants } from "../../Util/Constants";


export interface IRoleService {

    // GET
    getRoles(actorCompanyId: number): ng.IPromise<RoleGridDTO[]>;
    getAllRoles(actorCompanyId: number): ng.IPromise<RoleGridDTO[]>;
    getRole(roleId: number): ng.IPromise<RoleEditDTO>;
    getStartPages(): ng.IPromise<ISmallGenericType[]>;
    getRoleHasUsers(roleId: number): ng.IPromise<IActionResult>;

    // POST
    saveRole(role: RoleEditDTO): ng.IPromise<IActionResult>;

    // DELETE
    deleteRole(roleId: number): ng.IPromise<IActionResult>;
    updateRolesState(dict: any): ng.IPromise<any>;
}

export class RoleService implements IRoleService {

    //@ngInject
    constructor(private httpService: IHttpService, private $q: ng.IQService) { }

    // GET
    getRoles(actorCompanyId: number): ng.IPromise<RoleGridDTO[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ROLE_ROLES + actorCompanyId, false).then(x => {
            return x.map(y => {
                let obj = new RoleEditDTO();
                angular.extend(obj, y);
                return obj;
            });
        });
    }

    getAllRoles(actorCompanyId: number): ng.IPromise<RoleGridDTO[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ROLE_ALL_ROLES + actorCompanyId, false).then(x => {
            return x.map(y => {
                let obj = new RoleEditDTO();
                angular.extend(obj, y);
                return obj;
            });
        });
    }

    getRole(roleId: number): ng.IPromise<RoleEditDTO> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ROLE_FOR_EDIT + roleId, false).then(x => {
            let obj = new RoleEditDTO();
            angular.extend(obj, x);
            return obj;
        });
    }
    getRoleHasUsers(roleId: number): ng.IPromise<IActionResult> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ROLE_VERIFY_ROLE_HAS_USERS + roleId,false);
    }

    getStartPages(): ng.IPromise<ISmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ROLE_ROLES_START_PAGES, false);
    }

    // POST
    saveRole(role: RoleEditDTO) {
        return this.httpService.post(Constants.WEBAPI_MANAGE_ROLE, role);
    }

    // DELETE
    deleteRole(roleId: number): ng.IPromise<IActionResult> {
        return this.httpService.delete(Constants.WEBAPI_MANAGE_ROLE + roleId);
    }
    updateRolesState(dict: any): ng.IPromise<any> {
        var model = { dict: dict };
        return this.httpService.post(Constants.WEBAPI_MANAGE_ROLE_ROLES_UPDATE_STATE, model);
    }
}
