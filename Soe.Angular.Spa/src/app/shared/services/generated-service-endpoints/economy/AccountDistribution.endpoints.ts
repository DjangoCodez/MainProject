


//Available methods for AccountDistributionController

//get
export const getAccountDistributionHeadsUsedIn = (type?: number, triggerType?: number, date?: number, useInVoucher?: boolean, useInSupplierInvoice?: boolean, useInCustomerInvoice?: boolean, useInImport?: boolean, useInPayrollVoucher?: boolean, useInPayrollVacationVoucher?: boolean) => `V2/Economy/Accounting/AccountDistribution/UsedIn?type=${type}&triggerType=${triggerType}&date=${encodeURIComponent(String(date))}&useInVoucher=${useInVoucher}&useInSupplierInvoice=${useInSupplierInvoice}&useInCustomerInvoice=${useInCustomerInvoice}&useInImport=${useInImport}&useInPayrollVoucher=${useInPayrollVoucher}&useInPayrollVacationVoucher=${useInPayrollVacationVoucher}`;

//get
export const getAccountDistributionHeads = (loadOpen: boolean, loadClosed: boolean, loadEntries: boolean, accountDistributionHeadId?: number) => `V2/Economy/Accounting/AccountDistribution/${loadOpen}/${loadClosed}/${loadEntries}/${accountDistributionHeadId || ''}`;

//get
export const getAccountDistributionHead = (accountDistributionHeadId: number) => `V2/Economy/Accounting/AccountDistribution/${accountDistributionHeadId}`;

//get
export const getAccountDistributionHeadsAuto = (accountDistributionHeadId?: number) => `V2/Economy/Accounting/AccountDistributionAuto/${accountDistributionHeadId || ''}`;

//get
export const getAccountDistributionTraceViews = (accountDistributionHeadId: number) => `V2/Economy/Accounting/AccountDistribution/GetAccountDistributionTraceViews/${accountDistributionHeadId}`;

//post, takes args: (model: number)
export const saveAccountDistribution = () => `V2/Economy/Accounting/AccountDistribution`;

//delete
export const deleteAccountDistribution = (accountDistributionHeadId: number) => `V2/Economy/Accounting/AccountDistribution/${accountDistributionHeadId}`;


