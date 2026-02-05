


//Available methods for ExtraFieldController

//get
export const getExtraField = (extraFieldId: number) => `V2/Core/ExtraField/${extraFieldId}`;

//get
export const getExtraFields = (entity: number) => `V2/Core/ExtraFields/${entity}`;

//get
export const getExtraFieldsDict = (entity: number, connectedEntity: number, connectedRecordId: number, addEmptyRow: boolean) => `V2/Core/ExtraFields/${entity}/${connectedEntity}/${connectedRecordId}/${addEmptyRow}`;

//get
export const getExtraFieldGridDTOs = (entity: number, loadRecords: boolean, connectedEntity: number, connectedRecordId: number, extraFieldId?: number) => `V2/Core/ExtraFieldGrid/${entity}/${loadRecords}/${connectedEntity}/${connectedRecordId}/${extraFieldId || ''}`;

//post, takes args: (extraField: number)
export const saveExtraField = () => `V2/Core/ExtraField`;

//delete
export const deleteExtraField = (extraFieldId: number) => `V2/Core/ExtraField/${extraFieldId}`;

//get
export const getExtraFieldRecord = (extraFieldId: number, recordId: number, entity: number) => `V2/Core/ExtraFieldRecord/${extraFieldId}/${recordId}/${entity}`;

//get
export const getExtraFieldsWitRecords = (recordId: number, entity: number, langId: number, connectedEntity: number, connectedRecordId: number) => `V2/Core/ExtraFieldsWithRecords/${recordId}/${entity}/${langId}/${connectedEntity}/${connectedRecordId}`;

//post, takes args: (model: number)
export const saveExtraFieldRecords = () => `V2/Core/ExtraFieldsWithRecords`;

//get
export const getSysExtraFields = (entity: number) => `V2/Core/SysExtraFields/${entity}`;


