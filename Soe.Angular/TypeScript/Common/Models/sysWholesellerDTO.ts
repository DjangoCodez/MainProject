import { ISysWholesellerDTO, ISysWholeSellerSettingDTO } from "../../Scripts/TypeLite.Net4";

export class sysWholesellerDTO implements ISysWholesellerDTO {   

    isOnlyInComp: boolean;
    name: string;
    sysCountryId: number;
    sysCurrencyId: number;
    sysWholesellerId: number;
    sysWholeSellerSettingDTOs: ISysWholeSellerSettingDTO[];
    type: number;
    hasEdiFeature: boolean;    
    messageTypes: string;    
    sysWholesellerEdiId: number;
    
}
