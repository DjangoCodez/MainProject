


//Available methods for VoucherSeriesTypeController

//get
export const getVoucherSeriesTypes = (voucherSeriesTypeId?: number) => `V2/Economy/Account/VoucherSeriesType/${voucherSeriesTypeId || ''}`;

//get
export const getVoucherSeriesTypesByCompany = (addEmptyRow?: boolean, nameOnly?: boolean) => `V2/Economy/Account/VoucherSeriesType/ByCompany/${addEmptyRow || ''}/${nameOnly || ''}`;

//get
export const getVoucherSeriesType = (voucherSeriesTypeId: number) => `V2/Economy/Account/VoucherSeriesType/${voucherSeriesTypeId}`;

//post, takes args: (voucherSeriesTypeDTO: number)
export const saveVoucherSeriesType = () => `V2/Economy/Account/VoucherSeriesType`;

//delete
export const deleteVoucherSeriesType = (voucherSeriesTypeId: number) => `V2/Economy/Account/VoucherSeriesType/${voucherSeriesTypeId}`;


