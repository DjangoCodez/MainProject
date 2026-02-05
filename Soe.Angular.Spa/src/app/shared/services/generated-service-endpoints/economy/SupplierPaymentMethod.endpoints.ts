//Available methods for SupplierPaymentMethodController

//get
export const getPaymentInformationViewsDict = (addEmptyRow: boolean) =>
  `V2/Economy/SupplierPaymentMethod/PaymentInformation/${addEmptyRow}`;

//get
export const getPaymentInformationViewsSmall = (addEmptyRow: boolean) =>
  `V2/Economy/SupplierPaymentMethod/PaymentInformation/Small/${addEmptyRow}`;

//get
export const getPaymentInformationViews = (supplierId: number) =>
  `V2/Economy/SupplierPaymentMethod/PaymentInformation/${supplierId}`;

//get
export const getPaymentMethodsSupplierGrid = (
  addEmptyRow: boolean,
  includePaymentInformationRows: boolean,
  includeAccount: boolean,
  onlyCashSales: boolean,
  paymentMethodId?: number
) =>
  `V2/Economy/SupplierPaymentMethod/PaymentMethodSupplierGrid/${addEmptyRow}/${includePaymentInformationRows}/${includeAccount}/${onlyCashSales}/${
    paymentMethodId || ''
  }`;

//get
export const getPaymentMethods = (
  addEmptyRow: boolean,
  includePaymentInformationRows: boolean,
  includeAccount: boolean,
  onlyCashSales: boolean
) =>
  `V2/Economy/SupplierPaymentMethod/PaymentMethod/${addEmptyRow}/${includePaymentInformationRows}/${includeAccount}/${onlyCashSales}`;

//get
export const getPaymentMethod = (
  paymentMethodId: number,
  loadAccount: boolean,
  loadPaymentInformationRow: boolean
) =>
  `V2/Economy/SupplierPaymentMethod/PaymentMethod/${paymentMethodId}/${loadAccount}/${loadPaymentInformationRow}`;

//post, takes args: (paymentMethod: number)
export const savePaymentMethod = () =>
  `V2/Economy/SupplierPaymentMethod/PaymentMethod`;

//delete
export const deletePaymentMethod = (paymentMethodId: number) =>
  `V2/Economy/SupplierPaymentMethod/PaymentMethod/${paymentMethodId}`;

//get
export const getSysPaymentMethodsDict = (addEmptyRow: boolean) =>
  `V2/Economy/SupplierPaymentMethod/SysPaymentMethod/${addEmptyRow}`;
