


//Available methods for SignatoryContractController

//get
export const getSignatoryContractsGrid = (signatoryContractId?: number) => `V2/Manage/Preferences/Registry/SignatoryContract/Grid/${signatoryContractId || ''}`;

//get
export const getSignatoryContract = (signatoryContractId: number) => `V2/Manage/Preferences/Registry/SignatoryContract/${signatoryContractId}`;

//get
export const getSignatoryContractSubContract = (signatoryContractId: number) => `V2/Manage/Preferences/Registry/SignatoryContract/${signatoryContractId}/SubContract`;

//get
export const getPermissionTerms = (signatoryContractId: number) => `V2/Manage/Preferences/Registry/SignatoryContract/PermissionTerms/${signatoryContractId}`;

//post, takes args: (signatoryContract: number)
export const saveSignatoryContract = () => `V2/Manage/Preferences/Registry/SignatoryContract`;

//post, takes args: (signatoryContractId: number, item: number)
export const revokeSignatoryContract = (signatoryContractId: number) => `V2/Manage/Preferences/Registry/SignatoryContract/${signatoryContractId}/Revoke`;

//post, takes args: (authorizeRequest: number)
export const signatoryContractAuthorize = () => `V2/Manage/Preferences/Registry/SignatoryContract/Authorize`;

//post, takes args: (authenticationResponse: number)
export const signatoryContractAuthenticate = () => `V2/Manage/Preferences/Registry/SignatoryContract/Authenticate`;


