


//Available methods for PriceBasedMarkupController

//get
export const getPriceBasedMarkup = (id: number) => `V2/Billing/PriceBasedMarkup/GetPriceBasedMarkup/${id}`;

//get
export const getPriceBasedMarkupGrid = (priceBasedMarkupId?: number) => `V2/Billing/PriceBasedMarkup/Grid/${priceBasedMarkupId || ''}`;

//post, takes args: (priceBasedMarkup: number)
export const savePriceBasedMarkup = () => `V2/Billing/PriceBasedMarkup/Markup/PriceBased`;

//delete
export const deletePriceBasedMarkup = (priceBaseMarkupId: number) => `V2/Billing/PriceBasedMarkup/Markup/PriceBased/${priceBaseMarkupId}`;

//get
export const getPriceLists = () => `V2/Billing/PriceBasedMarkup/PriceList/`;


