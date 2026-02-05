import { ICoreService } from "../../Core/Services/CoreService";
import { IHttpService } from "../../Core/Services/HttpService";
import { SoeModule, TermGroup_AttestEntity, SoeTimeCodeType, TermGroup_InvoiceProductVatType } from "../../Util/CommonEnumerations";
import { Constants } from "../../Util/Constants";

export interface IGDPRService {

    // GET
    getActorsWithoutConsent(): ng.IPromise<any>;

    // POST
    giveConsent(date: Date, customers: number[], suppliers: number[], contactPersons: number[]): ng.IPromise<any>;
    deleteActorsWithoutConsent(customers: number[], suppliers: number[], contactPersons: number[]): ng.IPromise<any>;

    // DELETE
    //deleteAttestWorkFlowTemplate(attestTemplateId: number): ng.IPromise<any>;

}

export class GDPRService implements IGDPRService {

    //@ngInject
    constructor(private httpService: IHttpService, private $q: ng.IQService) { }

    // GET
    getActorsWithoutConsent() {
        return this.httpService.get(Constants.WEBAPI_CORE_GDPR_WITHOUTCONSENT, false);
    }

    // POST
    giveConsent(date: Date, customers: number[], suppliers: number[], contactPersons: number[]): ng.IPromise<any> {
        var model = {
            date: date,
            customers: customers,
            suppliers: suppliers,
            contactPersons: contactPersons,
        }
        return this.httpService.post(Constants.WEBAPI_CORE_GDPR_WITHOUTCONSENT, model);
    }

    // POST
    deleteActorsWithoutConsent(customers: number[], suppliers: number[], contactPersons: number[]): ng.IPromise<any> {
        var model = {
            customers: customers,
            suppliers: suppliers,
            contactPersons: contactPersons,
        }
        return this.httpService.post(Constants.WEBAPI_CORE_GDPR_WITHOUTCONSENT_DELETE, model);
    }

    // DELETE
}
