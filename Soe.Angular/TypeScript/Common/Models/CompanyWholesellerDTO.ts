import { CompanyWholesellerListDTO } from "./CompanyWholesellerListDTO";
import { ISysWholesellerDTO, IEdiConnectionDTO } from "../../Scripts/TypeLite.Net4";

export class CompanyWholesellerDTO extends CompanyWholesellerListDTO {   

    useEdi: boolean;    
    created: Date;
    createdBy: string;
    modified: Date;
    modifiedBy: string;
    sysWholesellerEdiId: number;
    wholeseller: ISysWholesellerDTO;
    ediConnections: IEdiConnectionDTO[];
    messageTypes: string;
    hasEdiFeature: boolean;
    ediWholesellerSenderNrs: string;
}