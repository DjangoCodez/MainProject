import { IHttpService } from "../../Core/Services/HttpService";
import { Constants } from "../../Util/Constants";

export interface IContractService {

    // GET
    getContractGroups(): ng.IPromise<any>;
    getContractGroup(contractGroupId: number): ng.IPromise<any>;

    // POST
    saveContractGroup(contractGroup: any): ng.IPromise<any>;

    // DELETE
    deleteContractGroup(contractGroupId: number): ng.IPromise<any>

}

export class ContractService implements IContractService {

    //@ngInject
    constructor(private httpService: IHttpService, private $q: ng.IQService) { }

    // GET
    getContractGroups() {
        return this.httpService.get(Constants.WEBAPI_BILLING_CONTRACT_CONTRACTGROUP, false);
    }

    getContractGroup(contractGroupId: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_CONTRACT_CONTRACTGROUP + contractGroupId, false);
    }

    // POST
    saveContractGroup(contractGroup: any) {
        return this.httpService.post(Constants.WEBAPI_BILLING_CONTRACT_CONTRACTGROUP, contractGroup);
    }

    // DELETE
    deleteContractGroup(contractGroupId: number) {
        return this.httpService.delete(Constants.WEBAPI_BILLING_CONTRACT_CONTRACTGROUP + contractGroupId);
    }

}