import { ICompCurrencyDTO, ICompCurrencyRateDTO } from "../../Scripts/TypeLite.Net4";
import { TermGroup_CurrencyIntervalType, TermGroup_CurrencySource } from "../../Util/CommonEnumerations";


export class CompCurrencyDTO implements ICompCurrencyDTO {
    currencyId: number;
    sysCurrencyId: number;
    code: string;
    name: string;
    date: Date;
    rateToBase: number;
    compCurrencyRates: CompCurrencyRateDTO[];
}

export class CompCurrencyRateDTO implements ICompCurrencyRateDTO {
    currencyId: number;
    currencyRateId: number;
    code: string;
    name: string;
    intervalType: TermGroup_CurrencyIntervalType;
    source: TermGroup_CurrencySource;
    date: Date;
    rateToBase: number;
    intervalTypeName: string;
    sourceName: string;
}

