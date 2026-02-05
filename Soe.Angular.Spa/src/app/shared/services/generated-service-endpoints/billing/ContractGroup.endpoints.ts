


//Available methods for ContractGroupController

//get
export const getContractGroups = (id?: number) => `V2/Billing/Contract/ContractGroup/?id=${id}`;

//get
export const getContractGroup = (contractGroupId: number) => `V2/Billing/Contract/ContractGroup/${contractGroupId}`;

//get
export const getContractTraceViews = (contractId: number) => `V2/Billing/Contract/GetContractTraceViews/${contractId}`;

//post, takes args: (contractGroup: number)
export const saveContractGroup = () => `V2/Billing/Contract/ContractGroup/`;

//delete
export const deleteContractGroup = (contractGroupId: number) => `V2/Billing/Contract/ContractGroup/${contractGroupId}`;


