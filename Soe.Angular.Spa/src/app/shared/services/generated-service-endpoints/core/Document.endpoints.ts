


//Available methods for DocumentController

//get
export const hasNewDocuments = (time: string) => `V2/Core/Document/NewSince/${encodeURIComponent(time)}`;

//get
export const getCompanyDocuments = () => `V2/Core/Document/Company/`;

//get
export const getNbrOfUnreadCompanyDocuments = () => `V2/Core/Document/Company/UnreadCount/`;

//get
export const getMyDocuments = () => `V2/Core/Document/My/`;

//get
export const getDocument = (dataStorageId: number) => `V2/Core/Document/${dataStorageId}`;

//get
export const getDocumentUrl = (dataStorageId: number) => `V2/Core/Document/Url/${dataStorageId}`;

//get
export const getDocumentData = (dataStorageId: number) => `V2/Core/Document/Data/${dataStorageId}`;

//get
export const getDocumentFolders = () => `V2/Core/Document/Folders`;

//get
export const getDocumentRecipientInfo = (dataStorageId: number) => `V2/Core/Document/RecipientInfo/${dataStorageId}`;

//post, takes args: (model: number)
export const saveDocument = () => `V2/Core/Document`;

//post, takes args: (model: number)
export const setDocumentAsRead = () => `V2/Core/Document/SetAsRead/`;

//delete
export const deleteDocument = (dataStorageId: number) => `V2/Core/Document/${dataStorageId}`;


