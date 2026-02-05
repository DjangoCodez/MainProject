


//Available methods for AccountReconciliationController

//get
export const getReconciliationRows = (dim1Id: number, fromDim1: string, toDim1: string, fromDate: string, toDate: string) => `V2/Economy/Accounting/ReconciliationRows/${dim1Id}/${encodeURIComponent(fromDim1)}/${encodeURIComponent(toDim1)}/${encodeURIComponent(fromDate)}/${encodeURIComponent(toDate)}/`;

//get
export const getReconciliationPerAccount = (accountId: number, fromDate: string, toDate: string) => `V2/Economy/Accounting/ReconciliationPerAccount/${accountId}/${encodeURIComponent(fromDate)}/${encodeURIComponent(toDate)}/`;


