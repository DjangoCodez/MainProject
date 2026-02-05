import { IHttpService } from "../../../Core/Services/HttpService";
import { Constants } from "../../../Util/Constants";

export interface ISelectSupplierService {

    getSuppliers(onlyActive: boolean): ng.IPromise<any>

    getSuppliersBySearch(dto: any): ng.IPromise<any>
}

export class SelectSupplierService implements ISelectSupplierService {

    //@ngInject
    constructor(private httpService: IHttpService, private $q: ng.IQService) { }

    // GET        
    getSuppliers(onlyActive: boolean) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_SUPPLIER + "?onlyActive=" + onlyActive, false);
    }

    getSuppliersBySearch(dto: any) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_SUPPLIERS_BY_SEARCH, dto);
    }
}