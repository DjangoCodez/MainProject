import { IPriceRuleDTO } from "../../Scripts/TypeLite.Net4";

export class PriceRuleDTO implements IPriceRuleDTO {

    companyWholesellerPriceListId: number;
    lExampleType: number;
    lRule: IPriceRuleDTO;
    lRuleId: number;
    lValue: number;
    lValueType: number;
    modified: Date;
    modifiedBy: string;
    operatorType: number;
    priceListImportedHeadId: number;
    priceListTypeId: number;
    rExampleType: number;
    rRule: IPriceRuleDTO;
    rRuleId: number;
    ruleId: number;
    rValue: number;
    rValueType: number;
    useNetPrice: boolean;
}
