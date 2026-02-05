import { IHttpService } from "../../../Core/Services/HttpService";
import { TermGroup_AttestEntity } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { AttestWorkFlowTemplateHeadGridDTO, AttestWorkFlowTemplateRowDTO } from "../../Models/AttestWorkFlowDTOs";
import { UserSmallDTO } from "../../Models/UserDTO";

export interface IAddDocumentToAttestFlowService {

    getAttestWorkFlowTemplates(entity: TermGroup_AttestEntity): ng.IPromise<AttestWorkFlowTemplateHeadGridDTO[]>;
    getAttestWorkFlowTemplateHeadRows(templateHeadId: number): ng.IPromise<AttestWorkFlowTemplateRowDTO[]>;
    getUsersByAttestRoleMapping(attestTransitionId: number): ng.IPromise<UserSmallDTO[]>;
}

export class AddDocumentToAttestFlowService implements IAddDocumentToAttestFlowService {

    //@ngInject
    constructor(private httpService: IHttpService) { }

    getAttestWorkFlowTemplates(entity: TermGroup_AttestEntity): ng.IPromise<AttestWorkFlowTemplateHeadGridDTO[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_WORK_FLOW_TEMPLATE_LIST + entity, false).then(x => {
            return x.map(y => {
                let obj = new AttestWorkFlowTemplateHeadGridDTO();
                angular.extend(obj, y);
                return obj;
            });
        });
    }

    getAttestWorkFlowTemplateHeadRows(templateHeadId: number): ng.IPromise<AttestWorkFlowTemplateRowDTO[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_WORK_FLOW_TEMPLATE_ATTEST_WORK_FLOW_TEMPLATE_ROWS + templateHeadId, false).then(x => {
            return x.map(y => {
                let obj = new AttestWorkFlowTemplateRowDTO();
                angular.extend(obj, y);
                return obj;
            });
        });
    }

    getUsersByAttestRoleMapping(attestTransitionId: number): ng.IPromise<UserSmallDTO[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_WORK_FLOW_USERS_BY_ATTEST_ROLE_MAPPING + attestTransitionId, true).then(x => {
            return x.map(y => {
                let obj = new UserSmallDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            })
        });
    }
}