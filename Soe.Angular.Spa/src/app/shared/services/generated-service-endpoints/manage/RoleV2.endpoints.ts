


//Available methods for RoleV2Controller

//get
export const getRoles = (loadExternalCode: boolean) => `V2/Manage/Role?loadExternalCode=${loadExternalCode}`;

//get
export const byCompanyAsDict = (addEmptyRow: boolean, addEmptyRowAsAll: boolean) => `V2/Manage/Role/ByCompanyAsDict/${addEmptyRow}/${addEmptyRowAsAll}`;

//get
export const byUserAsDict = (actorCompanyId: number) => `V2/Manage/Role/ByUserAsDict/${actorCompanyId}`;


