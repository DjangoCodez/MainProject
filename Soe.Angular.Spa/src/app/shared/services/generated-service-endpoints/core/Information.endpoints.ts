


//Available methods for InformationController

//get
export const hasNewInformations = (time: string) => `V2/Core/Information/NewSince/${encodeURIComponent(time)}`;

//get
export const getNbrOfUnreadInformations = (language: string) => `V2/Core/Information/UnreadCount/${encodeURIComponent(language)}`;

//get
export const hasSevereUnreadInformation = (language: string) => `V2/Core/Information/Unread/Severe/${encodeURIComponent(language)}`;


