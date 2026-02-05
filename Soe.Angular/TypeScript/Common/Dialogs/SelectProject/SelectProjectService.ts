import { IHttpService } from "../../../Core/Services/HttpService";
import { IProjectGridDTO } from "../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../Util/Constants";

export interface ISelectProjectService {

    getProjects(onlyActive: boolean, hidden: boolean, setStatusName: boolean, includeManagerName: boolean, loadOrders: boolean): ng.IPromise<IProjectGridDTO[]>
    getProjectsBySearch(number: string, name: string, customerNr: string, customerName: string, managerName: string, orderNr: string, onlyActive: boolean, hidden: boolean, showWithoutCustomer: boolean, loadMine: boolean, showAllProjects: boolean, customerId?: number): ng.IPromise<IProjectGridDTO[]>;
}

export class SelectProjectService implements ISelectProjectService {

    //@ngInject
    constructor(private httpService: IHttpService, private $q: ng.IQService) { }

    // GET        
    getProjects(onlyActive: boolean, hidden: boolean, setStatusName: boolean, includeManagerName: boolean, loadOrders: boolean) {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_PROJECT + onlyActive + "/" + hidden + "/" + setStatusName + "/" + includeManagerName + "/" + loadOrders, false);
    }

    getProjectsBySearch(number: string, name: string, customerNr: string, customerName: string, managerName: string, orderNr: string, onlyActive: boolean, hidden: boolean, showWithoutCustomer: boolean, loadMine: boolean, showAllProjects: boolean = false, customerId: number = null) {
        var model = { number: number, name: name, customerNr: customerNr, customerName: customerName, managerName: managerName, orderNr: orderNr, onlyActive: onlyActive, hidden: hidden, showWithoutCustomer: showWithoutCustomer, loadMine: loadMine, customerId: customerId, showAllProjects: showAllProjects };
        return this.httpService.post(Constants.WEBAPI_BILLING_INVOICE_PROJECT_SEARCH, model);
    }
}
