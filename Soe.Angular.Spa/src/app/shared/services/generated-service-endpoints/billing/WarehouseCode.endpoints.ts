


//Available methods for WarehouseCodeController

//get
export const getStocks = (addEmptyRow: boolean) => `V2/Billing/WarehouseCode/Stock/${addEmptyRow}`;

//get
export const getStock = (stockId: number) => `V2/Billing/WarehouseCode/Stock/${stockId}`;

//get
export const getStocksDict = (addEmptyRow: boolean) => `V2/Billing/WarehouseCode/Stock/Dict/${addEmptyRow}`;

//get
export const getStocksDictForInvoiceProduct = (productId: number, addEmptyRow: boolean) => `V2/Billing/WarehouseCode/Stock/ByProduct/${productId}/${addEmptyRow}`;

//get
export const getStocksForInvoiceProduct = (productId: number) => `V2/Billing/WarehouseCode/Stock/ByProduct/${productId}`;

//post, takes args: (stockDTO: number)
export const saveStock = () => `V2/Billing/WarehouseCode/Stock`;

//delete
export const deleteStock = (stockId: number) => `V2/Billing/WarehouseCode/Stock/${stockId}`;

//post, takes args: (invoiceProductId: number, fromStockId: number, toStockId: number, quantity: number)
export const stockTransfer = (invoiceProductId: number, fromStockId: number, toStockId: number, quantity: number) => `V2/Billing/WarehouseCode/StockTransfer/${invoiceProductId}/${fromStockId}/${toStockId}/${quantity}`;

//post, takes args: (model: number)
export const importStockBalances = () => `V2/Billing/WarehouseCode/ImportStockBalances`;

//post, takes args: (stockId: number)
export const recalculateStockBalance = (stockId: number) => `V2/Billing/WarehouseCode/RecalculateStockBalance/${stockId}`;


