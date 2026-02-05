import { IHttpService } from "../../Core/Services/HttpService";
import { SoeModule, TermGroup_AttestEntity} from "../../Util/CommonEnumerations";
import { Constants } from "../../Util/Constants";
import { AttestStateDTO } from "../../Common/Models/AttestStateDTO";
import { IAttestRoleDTO, IAttestTransitionDTO } from "../../Scripts/TypeLite.Net4";

export interface ISigningService {

    // GET
    getAttestRole(attestRoleId: number): ng.IPromise<IAttestRoleDTO>
    getAttestRoles(module: SoeModule): ng.IPromise<any>;
    getAttestState(attestStateId: number): ng.IPromise<AttestStateDTO>
    getAttestStates(entity: TermGroup_AttestEntity, module: SoeModule, addEmptyRow: boolean): ng.IPromise<AttestStateDTO[]>;
    getAttestEntitiesGenericList(addEmptyRow: boolean, skipUnknown: boolean, module: SoeModule): ng.IPromise<any>;
    getAttestTransitionGridDTOs(entity: TermGroup_AttestEntity, module: SoeModule, setEntityName: boolean): ng.IPromise<any>;
    getAttestTransition(attestTransitionId: number): ng.IPromise<IAttestTransitionDTO>
    getAttestWorkFlowTemplates(entity: TermGroup_AttestEntity): ng.IPromise<any>;
    getAttestWorkFlowTemplate(attestWorkFlowTemplateId: number): ng.IPromise<any>;
    getAttestWorkFlowTemplateRows(attestWorkFlowTemplateId: number): ng.IPromise<any>;

    // POST
    saveAttestState(attestState: AttestStateDTO): ng.IPromise<any>;
    saveAttestTransition(attestTransition: IAttestTransitionDTO): ng.IPromise<any>;
    saveAttestRole(attestRole: IAttestRoleDTO): ng.IPromise<any>;
    saveAttestWorkFlowTemplate(attestTemplate): ng.IPromise<any>;
    saveAttestWorkFlowTemplateRows(attestTemplateRows, attestTemplateId): ng.IPromise<any>;

    // DELETE
    deleteAttestState(attestStateId: number): ng.IPromise<any>;
    deleteAttestTransition(attestTransitionId: number): ng.IPromise<any>;
    deleteAttestRole(attestRoleId: number): ng.IPromise<any>;
    deleteAttestWorkFlowTemplate(attestTemplateId: number): ng.IPromise<any>;
}

export class SigningService implements ISigningService {

    //@ngInject
    constructor(private httpService: IHttpService, private $q: ng.IQService) { }

    // GET
    getAttestRoles(module: SoeModule) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_ROLE + "?module=" + module, false, Constants.WEBAPI_ACCEPT_GRID_DTO);
    }
    getAttestRole(attestRoleId: number) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_ROLE + attestRoleId, false);
    }
    getAttestStates(entity: TermGroup_AttestEntity, module: SoeModule, addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_STATE + entity + "/" + module + "/" + addEmptyRow, false);
    }
    getAttestState(attestStateId: number) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_STATE + attestStateId, false);
    }
    getAttestEntitiesGenericList(addEmptyRow: boolean, skipUnknown: boolean, module: SoeModule) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_ENTITY_GL + addEmptyRow + "/" + skipUnknown + "/" + module, false);
    }
    getAttestTransitionGridDTOs(entity: TermGroup_AttestEntity, module: SoeModule, setEntityName: boolean) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_TRANSITION + entity + "/" + module + "/" + setEntityName, false, Constants.WEBAPI_ACCEPT_GRID_DTO);
    }

    getAttestTransition(attestTransitionId: number): ng.IPromise<IAttestTransitionDTO> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_TRANSITION + attestTransitionId, false);
    }

    getAttestWorkFlowTemplates(entity: TermGroup_AttestEntity): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_WORK_FLOW_TEMPLATE_LIST + entity, false);
    }

    getAttestWorkFlowTemplate(attestWorkFlowTemplateId: number): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_WORK_FLOW_TEMPLATE + attestWorkFlowTemplateId, false);
    }
    getAttestWorkFlowTemplateRows(attestWorkFlowTemplateId: number): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_WORK_FLOW_TEMPLATE_ATTEST_WORK_FLOW_TEMPLATE_ROWS + attestWorkFlowTemplateId, false);
    }

    // POST
    saveAttestState(attestState: AttestStateDTO): ng.IPromise<any> {
        return this.httpService.post(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_STATE, attestState);
    }    

    saveAttestTransition(attestTransition: IAttestTransitionDTO): ng.IPromise<any> {
        return this.httpService.post(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_TRANSITION, attestTransition);
    }
    saveAttestRole(attestRole: IAttestRoleDTO): ng.IPromise<any> {
        return this.httpService.post(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_ROLE, attestRole);
    }
    
    saveAttestWorkFlowTemplate(attestTemplate): ng.IPromise<any> {
        return this.httpService.post(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_WORK_FLOW_TEMPLATE, attestTemplate);
    }

    saveAttestWorkFlowTemplateRows(attestTemplateRows, attestTemplateId): ng.IPromise<any> {
        return this.httpService.post(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_WORK_FLOW_TEMPLATE_ATTEST_WORK_FLOW_TEMPLATE_ROWS + attestTemplateId, attestTemplateRows);
    }

    // DELETE
    deleteAttestState(attestStateId: number): ng.IPromise<any> {
        return this.httpService.delete(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_STATE + attestStateId);
    }
    deleteAttestTransition(attestTransitionId: number): ng.IPromise<any> {
        return this.httpService.delete(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_TRANSITION + attestTransitionId);
    }
    deleteAttestRole(attestRoleId: number): ng.IPromise<any> {
        return this.httpService.delete(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_ROLE + attestRoleId);
    }
    deleteAttestWorkFlowTemplate(attestTemplateId: number): ng.IPromise<any> {
        return this.httpService.delete(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_WORK_FLOW_TEMPLATE + attestTemplateId);
    }
}
