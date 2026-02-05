import { ICompanyWholesellerPriceListViewDTO } from "../../Scripts/TypeLite.Net4";
import { TermGroup_Country, PriceListOrigin } from "../../Util/CommonEnumerations";

export class CompanyWholesellerPriceListViewDTO implements ICompanyWholesellerPriceListViewDTO {   

    actorCompanyId: number;
    companyWholesellerId: number;
    companyWholesellerPriceListId: number;
    date: Date;
    isUsed: boolean;
    priceListImportedHeadId: number;
    priceListName: string;
    priceListOrigin: PriceListOrigin;
    provider: number;
    sysPriceListHeadId: number;
    sysWholesellerCountry: TermGroup_Country;
    sysWholesellerId: number;
    sysWholesellerName: string;
    version: number;
    hasNewerVersion: boolean;
    isSelected: boolean;
}
