


//Available methods for FileController

//get
export const getFileRecord = (fileRecordId: number) => `V2/Core/Files/GetFile/${fileRecordId}`;

//get
export const getFileRecords = (entity: number, recordId: number) => `V2/Core/Files/GetFiles/${entity}/${recordId}`;

//post, takes args: (model: number)
export const getFilesAsZip = () => `V2/Core/Files/GetFilesAsZip`;

//post, takes args: (model: number)
export const sendDocumentsAsEmail = () => `V2/Core/Files/SendDocumentsAsEmail`;

//post, takes args: (entity: number, type: number, recordId: number, extractZip: boolean)
export const uploadInvoiceFile = (entity: number, type: number, recordId: number, extractZip: boolean) => `V2/Core/Files/${entity}/${type}/${recordId}?extractZip=${extractZip}`;

//post, takes args: (entity: number, type: number, recordId: number, roles: string, messageGroups: string)
export const uploadFileWithRolesAndMessageGroups = (entity: number, type: number, recordId: number, roles: string, messageGroups: string) => `V2/Core/Files/${entity}/${type}/${recordId}/${encodeURIComponent(roles)}/${encodeURIComponent(messageGroups)}`;

//post, takes args: ()
export const getByteArray = () => `V2/Core/Files/GetArray/`;

//post, takes args: (entity: number)
export const uploadInvoiceFileByEntityType = (entity: number) => `V2/Core/Files/Invoice/${entity}`;

//post, takes args: (model: number)
export const checkForDuplicates = () => `V2/Core/Files/CheckForDuplicates`;

//post, takes args: (entity: number, type: number)
export const uploadFile = (entity: number, type: number) => `V2/Core/Files/Upload/${entity}/${type}`;

//post, takes args: (entity: number, type: number, recordId: number)
export const uploadFileForRecord = (entity: number, type: number, recordId: number) => `V2/Core/Files/Upload/${entity}/${type}/${recordId}`;

//post, takes args: (entity: number, type: number, dataStorageId: number)
export const updateDataStorageFile = (entity: number, type: number, dataStorageId: number) => `V2/Core/Files/Replace/${entity}/${type}/${dataStorageId}`;

//post, takes args: (fileRecord: number)
export const updateFileRecord = () => `V2/Core/Files/Update/`;

//delete
export const deleteFileRecord = (dataStorageRecordId: number) => `V2/Core/Files/Delete/${dataStorageRecordId}`;


