


//Available methods for StockPlaceController

//get
export const getStockPlaces = (addEmptyRow: boolean, stockId: number) => `V2/Billing/Stock/StockPlace/${addEmptyRow}/${stockId}`;

//get
export const getStockPlace = (stockShelfId: number) => `V2/Billing/Stock/StockPlace/${stockShelfId}`;

//post, takes args: (stockPlaceDTO: number)
export const saveStockPlace = () => `V2/Billing/Stock/StockPlace`;

//delete
export const deleteStockPlace = (stockShelfId: number) => `V2/Billing/Stock/StockPlace/${stockShelfId}`;


