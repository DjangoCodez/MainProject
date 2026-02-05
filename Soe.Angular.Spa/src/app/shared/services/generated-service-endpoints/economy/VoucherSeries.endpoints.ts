


//Available methods for VoucherSeriesController

//get
export const getVoucherSeriesByYear = (accountYearId: number, includeTemplate: boolean) => `V2/Economy/VoucherSeries/VoucherSeries/${accountYearId}/${includeTemplate}`;

//get
export const getVoucherSeriesByYear0 = (accountYearDate: string, includeTemplate: boolean) => `V2/Economy/VoucherSeries/VoucherSeries/${encodeURIComponent(accountYearDate)}/${includeTemplate}`;

//get
export const getDefaultVoucherSeriesId = (accountYearId: number, type: number) => `V2/Economy/VoucherSeries/VoucherSeries/${accountYearId}/${type}`;

//get
export const getVoucherSeriesByYearRange = (fromAccountYearId: number, toAccountYearId: number) => `V2/Economy/VoucherSeries/VoucherSeriesByYearRange/${fromAccountYearId}/${toAccountYearId}`;

//get
export const getVoucherSeriesDictByYear = (accountYearId: number, addEmptyRow: boolean, includeTemplate: boolean) => `V2/Economy/VoucherSeries/DictByYear/${accountYearId}/${addEmptyRow}/${includeTemplate}`;


