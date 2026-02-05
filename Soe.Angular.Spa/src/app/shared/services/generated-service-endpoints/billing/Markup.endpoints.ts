


//Available methods for MarkupController

//get
export const getMarkup = (isDiscount: boolean) => `V2/Billing/Invoice/Markup/${isDiscount}`;

//get
export const getDiscount = (sysWholesellerId: number, code: string) => `V2/Billing/Invoice/Markup/Discount/${sysWholesellerId}/${encodeURIComponent(code)}`;

//post, takes args: (items: number)
export const saveMarkup = () => `V2/Billing/Invoice/Markup`;


