


//Available methods for StockProductController

//get
export const getStockProducts = (includeInactive: boolean, stockProductId?: number) => `V2/Billing/Stock/StockProducts?includeInactive=${includeInactive}&stockProductId=${stockProductId}`;

//get
export const getStockProductsByStockId = (stockId: number) => `V2/Billing/Stock/GetStockProductsByStockId/${stockId}`;

//get
export const getStockProductsByProductId = (productId: number) => `V2/Billing/Stock/StockProducts/${productId}`;

//get
export const getStockProduct = (stockProductId: number) => `V2/Billing/Stock/StockProduct/${stockProductId}`;

//get
export const getStockProductTransactions = (stockProductId: number) => `V2/Billing/Stock/StockProduct/Transactions/${stockProductId}`;

//post, takes args: (stockTransactionDTO: number)
export const saveStockTransaction = () => `V2/Billing/Stock/StockProduct/Transactions`;

//get
export const getStockProductProducts = (stockId?: number, onlyActive?: boolean) => `V2/Billing/Stock/StockProducts/Products/${stockId || ''}/${onlyActive || ''}`;

//post, takes args: (model: number)
export const validateProductsInStock = () => `V2/Billing/Stock/StockProducts/ValidateProductsInStock`;


