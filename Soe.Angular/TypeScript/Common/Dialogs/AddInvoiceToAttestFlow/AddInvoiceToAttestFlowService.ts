import { IHttpService } from "../../../Core/Services/HttpService";
import { Constants } from "../../../Util/Constants";

export interface IAddInvoiceToAttestFlowService {

    getAttestWorkFlowGroups(): ng.IPromise<any>;
    getAttestWorkFlowGroupsDict(addEmptyRow: boolean): ng.IPromise<any>;

}

export class AddInvoiceToAttestFlowService implements IAddInvoiceToAttestFlowService {

    //@ngInject
    constructor(private httpService: IHttpService, private $q: ng.IQService) { }

    // GET        
    getAttestWorkFlowGroups() {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_SUPPLIER_ATTEST_WORK_FLOW_ATTEST_GROUP, null, Constants.CACHE_EXPIRE_LONG);
    }

    getAttestWorkFlowGroupsDict(addEmptyRow: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_SUPPLIER_ATTEST_WORK_FLOW_ATTEST_GROUP + "?addEmptyRow=" + addEmptyRow, Constants.WEBAPI_ACCEPT_GENERIC_TYPE, Constants.CACHE_EXPIRE_LONG);
    }
}