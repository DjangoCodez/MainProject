import { ICompanyExternalCodeDTO, ICompanyExternalCodeGridDTO } from "../../Scripts/TypeLite.Net4";
import { TermGroup_CompanyExternalCodeEntity } from "../../Util/CommonEnumerations";

export class CompanyExternalCodeDTO implements ICompanyExternalCodeDTO {
    actorCompanyId: number;
    companyExternalCodeId: number;
    entity: TermGroup_CompanyExternalCodeEntity;
    externalCode: string;
    recordId: number;

}

export class CompanyExternalCodeGridDTO implements ICompanyExternalCodeGridDTO {
    companyExternalCodeId: number;
    entity: any;
    entityName: string;
    externalCode: string;
    recordId: number;
    recordName: string;
    
}