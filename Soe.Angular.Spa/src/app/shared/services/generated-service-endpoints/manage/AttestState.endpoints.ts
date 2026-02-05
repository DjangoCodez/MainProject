


//Available methods for AttestStateController

//get
export const getAttestStates = (entity: number, module: number, addEmptyRow: boolean) => `V2/Manage/Attest/AttestState/AttestState/${entity}/${module}/${addEmptyRow}`;

//get
export const getAttestState = (attestStateId: number) => `V2/Manage/Attest/AttestState/AttestState/${attestStateId}`;

//get
export const getInitialAttestState = (entity: number) => `V2/Manage/Attest/AttestState/AttestState/Initial/${entity}`;

//get
export const getAttestStatesGenericList = (entity: number, module: number, addEmptyRow: boolean, addMultipleRow: boolean) => `V2/Manage/Attest/AttestState/AttestState/GenericList/${entity}/${module}/${addEmptyRow}/${addMultipleRow}`;

//get
export const getUserValidAttestStates = (entity: number, dateFrom: string, dateTo: string, excludePayrollStates: boolean, employeeGroupId?: number) => `V2/Manage/Attest/AttestState/AttestState/UserValidAttestStates/${entity}/${encodeURIComponent(dateFrom)}/${encodeURIComponent(dateTo)}/${excludePayrollStates}/${employeeGroupId || ''}`;

//get
export const hasHiddenAttestState = (entity: number) => `V2/Manage/Attest/AttestState/AttestState/HasHiddenAttestState/${entity}`;

//get
export const hasInitialAttestState = (entity: number, module: number) => `V2/Manage/Attest/AttestState/AttestState/HasInitialAttestState/${entity}/${module}`;

//post, takes args: (attestStateDTO: number)
export const saveAttestState = () => `V2/Manage/Attest/AttestState/AttestState/`;

//delete
export const deleteAttestState = (attestStateId: number) => `V2/Manage/Attest/AttestState/AttestState/${attestStateId}`;

//get
export const getAttestEntitiesGenericList = (addEmptyRow: boolean, skipUnknown: boolean, module: number) => `V2/Manage/Attest/AttestState/AttestEntity/GenericList/${addEmptyRow}/${skipUnknown}/${module}`;


