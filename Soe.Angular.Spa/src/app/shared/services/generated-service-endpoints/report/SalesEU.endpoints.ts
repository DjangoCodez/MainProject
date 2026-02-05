


//Available methods for SalesEUController

//get
export const salesEU = (startDate: string, stopDate: string) => `V2/Report/SalesEU/${encodeURIComponent(startDate)}/${encodeURIComponent(stopDate)}`;

//get
export const salesEUDetails = (actorId: number, startDate: string, stopDate: string) => `V2/Report/SalesEUDetails/${actorId}/${encodeURIComponent(startDate)}/${encodeURIComponent(stopDate)}`;

//get
export const salesEUExportFile = (periodType: number, startDate: string, stopDate: string) => `V2/Report/SalesEUExportFile/${periodType}/${encodeURIComponent(startDate)}/${encodeURIComponent(stopDate)}`;


