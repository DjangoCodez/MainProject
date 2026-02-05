


//Available methods for VoucherController

//get
export const getVouchersBySeries = (accountYearId: number, voucherSeriesTypeId: number, voucherHeadId?: number) => `V2/Economy/Voucher/Voucher/BySeries/${accountYearId}/${voucherSeriesTypeId}/${voucherHeadId || ''}`;

//get
export const getSmallVoucherTemplates = (accountYearId: number) => `V2/Economy/Voucher/Voucher/Template/${accountYearId}`;

//get
export const getGridVoucherTemplates = (accountYearId: number, voucherHeadId?: number) => `V2/Economy/Voucher/Voucher/Template/Grid/${accountYearId}/${voucherHeadId || ''}`;

//get
export const getVoucher = (voucherHeadId: number, loadVoucherSeries: boolean, loadVoucherRows: boolean, loadVoucherRowAccounts: boolean, loadAccountBalance: boolean) => `V2/Economy/Voucher/Voucher/${voucherHeadId}/${loadVoucherSeries}/${loadVoucherRows}/${loadVoucherRowAccounts}/${loadAccountBalance}`;

//post, takes args: (model: number)
export const saveVoucher = () => `V2/Economy/Voucher/Voucher`;

//post, takes args: (model: number)
export const editVoucherNrOnlySuperSupport = () => `V2/Economy/Voucher/Voucher/SuperSupport/EditVoucherNr/`;

//delete
export const deleteVoucher = (voucherHeadId: number) => `V2/Economy/Voucher/Voucher/${voucherHeadId}`;

//delete
export const deleteVoucherOnlySuperSupport = (voucherHeadId: number, checkTransfer: boolean) => `V2/Economy/Voucher/Voucher/SuperSupport/${voucherHeadId}/${checkTransfer}`;

//delete
export const deleteVouchersOnlySuperSupport = (voucherHeadIds: number[]) => `V2/Economy/Voucher/Voucher/SuperSupport/Multiple/?voucherHeadIds=${voucherHeadIds}`;

//get
export const getVoucherRows = (voucherHeadId: number) => `V2/Economy/Voucher/VoucherRow/${voucherHeadId}`;

//get
export const getVoucherRowHistory = (voucherHeadId: number) => `V2/Economy/Voucher/Voucher/VoucherRowHistory/${voucherHeadId}`;


