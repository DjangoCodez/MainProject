


//Available methods for IncomingDeliveryController

//get
export const getIncomingDeliveriesGrid = (incomingDeliveryHeadId?: number) => `V2/Time/IncomingDelivery/Grid/${incomingDeliveryHeadId || ''}`;

//get
export const getIncomingDelivery = (incomingDeliveryHeadId: number) => `V2/Time/IncomingDelivery/${incomingDeliveryHeadId}`;

//post, takes args: (model: number)
export const saveIncomingDelivery = () => `V2/Time/IncomingDelivery`;

//delete
export const deleteIncomingDelivery = (incomingDeliveryHeadId: number) => `V2/Time/IncomingDelivery/${incomingDeliveryHeadId}`;

//get
export const getIncomingDeliveryRows = (incomingDeliveryHeadId: number) => `V2/Time/IncomingDelivery/IncomingDeliveryRow/${incomingDeliveryHeadId}`;

//get
export const getIncomingDeliveryTypesGrid = (incomingDeliveryTypeId?: number) => `V2/Time/IncomingDelivery/IncomingDeliveryType/Grid?incomingDeliveryTypeId=${incomingDeliveryTypeId}`;

//get
export const getIncomingDeliveryTypesSmall = () => `V2/Time/IncomingDelivery/IncomingDeliveryType/Small`;

//get
export const getIncomingDeliveryTypesDict = (addEmptyRow: boolean) => `V2/Time/IncomingDelivery/IncomingDeliveryType/Dict/${addEmptyRow}`;

//get
export const getIncomingDeliveryType = (incomingDeliveryTypeId: number) => `V2/Time/IncomingDelivery/IncomingDeliveryType/${incomingDeliveryTypeId}`;

//post, takes args: (incomingDeliveryTypeDTO: number)
export const saveIncomingDeliveryType = () => `V2/Time/IncomingDelivery/IncomingDeliveryType`;

//delete
export const deleteIncomingDeliveryType = (incomingDeliveryTypeId: number) => `V2/Time/IncomingDelivery/IncomingDeliveryType/${incomingDeliveryTypeId}`;


