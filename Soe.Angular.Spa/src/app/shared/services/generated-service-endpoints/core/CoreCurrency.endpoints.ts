


//Available methods for CoreCurrencyController

//post, takes args: (currency: number)
export const saveCurrency = () => `V2/Core/Currency`;

//delete
export const deleteCurrency = (currencyId: number) => `V2/Core/Currency/${currencyId}`;

//get
export const getCurrency = (currencyId: number) => `V2/Core/Currency/${currencyId}`;

//get
export const getCurrenciesGrid = (currencyId: number) => `V2/Core/Currency/Grid/${currencyId}`;

//get
export const getSysCurrencies = () => `V2/Core/Currency/Sys`;

//get
export const getSysCurrenciesDict = () => `V2/Core/Currency/Sys/Dict`;

//get
export const getCompCurrencies = (loadRates: boolean) => `V2/Core/Currency/Comp?loadRates=${loadRates}`;

//get
export const getCompCurrenciesDict = (addEmptyRow: boolean) => `V2/Core/Currency/Comp/Dict/${addEmptyRow}`;

//get
export const getCompCurrenciesDictSmall = () => `V2/Core/Currency/Comp/DictSmall`;

//get
export const getCompCurrencyRate = (sysCurrencyId: number, date: string, rateToBase: boolean) => `V2/Core/Currency/Comp/${sysCurrencyId}/${encodeURIComponent(date)}/${rateToBase}`;

//get
export const getLedgerCurrency = (actorId: number) => `V2/Core/Currency/Ledger/${actorId}`;

//get
export const getEnterpriseCurrency = () => `V2/Core/Currency/Enterprise`;

//get
export const getCompanyCurrency = () => `V2/Core/Currency/Comp/BaseCurrency`;


