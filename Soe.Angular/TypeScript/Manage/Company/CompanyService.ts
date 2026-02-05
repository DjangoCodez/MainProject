import { CopyFromTemplateCompanyInputDTO } from "../../Common/Models/CompanyDTOs";
import { IHttpService } from "../../Core/Services/HttpService";
import { Constants } from "../../Util/Constants";

export interface ICompanyService {

    // GET
    getCompany(actorCompanyId: number): ng.IPromise<any>;
    getTemplateCompanies(licenseId: number): ng.IPromise<any>;
    getCompaniesByLicense(licenseId: number): ng.IPromise<any>;

    // POST
    saveCompany(company: any): ng.IPromise<any>;
    copyFromTemplateCompany(dto: CopyFromTemplateCompanyInputDTO): ng.IPromise<any>;
    // DELETE
    deleteCompany(actorCompanyId: number): ng.IPromise<any>;
}

export class CompanyService implements ICompanyService {

    //@ngInject
    constructor(private httpService: IHttpService, private $q: ng.IQService) { }

    // GET
    getCompany(actorCompanyId: number) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_COMPANY_COMPANY + actorCompanyId, false);
    }

    getTemplateCompanies(licenseId: number) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_COMPANY_TEMPLATE_COMPANIES + licenseId, false);
    }

    getCompaniesByLicense(licenseId: number) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_COMPANY_COMPANIES_BY_LICENSE + licenseId, false);
    }


    // POST
    saveCompany(company: any) {
        return this.httpService.post(Constants.WEBAPI_MANAGE_COMPANY_COMPANY, company);
    }

    copyFromTemplateCompany(dto: CopyFromTemplateCompanyInputDTO) {
        return this.httpService.post(Constants.WEBAPI_MANAGE_COMPANY_TEMPLATE_COPY, dto);
    }


    // DELETE
    deleteCompany(actorCompanyId: number): ng.IPromise<any> {
        return this.httpService.delete(Constants.WEBAPI_MANAGE_COMPANY_COMPANY + actorCompanyId);
    }

}
