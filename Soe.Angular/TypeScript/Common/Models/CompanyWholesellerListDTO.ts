import { ICompanyWholesellerListDTO } from "../../Scripts/TypeLite.Net4";

export class CompanyWholesellerListDTO implements ICompanyWholesellerListDTO {   

    companySysWholesellerDtoId: number;    
    active: boolean;
    name: string;
    sysWholesellerId: number;
}
