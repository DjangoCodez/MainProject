


//Available methods for VatCodeController

//get
export const getVatCodesGrid = (vatCodeId?: number) => `V2/Economy/VatCode/Grid/${vatCodeId || ''}`;

//get
export const getVatCodesDict = (addEmptyRow: boolean) => `V2/Economy/VatCode/Dict/${addEmptyRow}`;

//get
export const getVatCodes = () => `V2/Economy/VatCode`;

//get
export const getVatCode = (vatCodeId: number) => `V2/Economy/VatCode/${vatCodeId}`;

//post, takes args: (model: number)
export const saveVatCode = () => `V2/Economy/VatCode/VatCode`;

//delete
export const deleteVatCode = (vatCodeId: number) => `V2/Economy/VatCode/${vatCodeId}`;


