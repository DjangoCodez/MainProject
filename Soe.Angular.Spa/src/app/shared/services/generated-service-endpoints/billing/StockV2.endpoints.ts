


//Available methods for StockV2Controller

//get
export const getGridStocks = (stockId?: number) => `V2/Billing/Stock/StockGrid/${stockId || ''}`;

//get
export const getStocks = (addEmptyRow: boolean) => `V2/Billing/Stock/Stock/${addEmptyRow}`;

//get
export const getSmallGenericStocks = (addEmptyRow: boolean) => `V2/Billing/Stock/StockSmall/${addEmptyRow}`;

//get
export const getStock = (stockId: number, addStockShelfs: boolean) => `V2/Billing/Stock/Stock/${stockId}/${addStockShelfs}`;

//get
export const getStocksDict = (addEmptyRow: boolean, sort?: boolean) => `V2/Billing/Stock/Stock/Dict/${addEmptyRow}?sort=${sort}`;

//get
export const getStocksDictForInvoiceProduct = (productId: number, addEmptyRow: boolean) => `V2/Billing/Stock/Stock/ByProduct/${productId}/${addEmptyRow}`;

//get
export const getStocksForInvoiceProduct = (productId: number) => `V2/Billing/Stock/Stock/ByProduct/${productId}`;

//post, takes args: (stockDTO: number)
export const saveStock = () => `V2/Billing/Stock/Stock`;

//delete
export const deleteStock = (stockId: number) => `V2/Billing/Stock/Stock/${stockId}`;

//post, takes args: (invoiceProductId: number, fromStockId: number, toStockId: number, quantity: number)
export const stockTransfer = (invoiceProductId: number, fromStockId: number, toStockId: number, quantity: number) => `V2/Billing/Stock/StockTransfer/${invoiceProductId}/${fromStockId}/${toStockId}/${quantity}`;

//post, takes args: (model: number)
export const importStockBalances = () => `V2/Billing/Stock/ImportStockBalances`;

//post, takes args: (stockId: number)
export const recalculateStockBalance = (stockId: number) => `V2/Billing/Stock/RecalculateStockBalance/${stockId}`;

//get
export const getStockPlaces = (addEmptyRow: boolean, stockId: number) => `V2/Billing/Stock/StockPlace/${addEmptyRow}/${stockId}`;

//get
export const getStockPlace = (stockShelfId: number) => `V2/Billing/Stock/StockPlace/${stockShelfId}`;

//post, takes args: (stockPlaceDTO: number)
export const saveStockPlace = () => `V2/Billing/Stock/StockPlace`;

//delete
export const deleteStockPlace = (stockShelfId: number) => `V2/Billing/Stock/StockPlace/${stockShelfId}`;

//get
export const validateStockPlace = (stockShelfId: number) => `V2/Billing/Stock/StockPlace/Validate/${stockShelfId}`;


