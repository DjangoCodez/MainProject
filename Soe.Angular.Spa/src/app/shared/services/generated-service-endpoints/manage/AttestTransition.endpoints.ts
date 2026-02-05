


//Available methods for AttestTransitionController

//get
export const getAttestTransitions = (entity: number, module: number, setEntityName: boolean) => `V2/Manage/Attest/AttestTransition/${entity}/${module}/${setEntityName}`;

//get
export const getAttestTransitionsGrid = (entity: number, module: number, setEntityName: boolean) => `V2/Manage/Attest/AttestTransition/Grid/${entity}/${module}/${setEntityName}`;

//get
export const getAttestTransition = (attestTransitionId: number) => `V2/Manage/Attest/AttestTransition/${attestTransitionId}`;

//get
export const getUserAttestTransitions = (entity: number, dateFrom?: number, dateTo?: number) => `V2/Manage/Attest/AttestTransition/User/${entity}?dateFrom=${encodeURIComponent(String(dateFrom))}&dateTo=${encodeURIComponent(String(dateTo))}`;

//get
export const getAttestTransitionsDict = (entity: number, module: number, loadAttestRole: boolean) => `V2/Manage/Attest/AttestTransition/Dict/${entity}/${module}/${loadAttestRole}`;

//post, takes args: (attestTransitionDTO: number)
export const saveAttestTransition = () => `V2/Manage/Attest/AttestTransition`;

//delete
export const deleteAttestTransition = (attestTransitionId: number) => `V2/Manage/Attest/AttestTransition/${attestTransitionId}`;


