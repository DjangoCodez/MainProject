


//Available methods for SystemController

//get
export const getBankintegrationRequestGrid = (fileType: number) => `V2/Manage/System/Bankintegration/Request/Grid/${fileType}`;

//post, takes args: (filter: number)
export const searchBankintegrationRequest = () => `V2/Manage/System/Bankintegration/Request/Search/`;

//get
export const getBankintegrationRequestFiles = (requestId: number) => `V2/Manage/System/Bankintegration/Request/Files/${requestId}`;

//get
export const getBankintegrationOnboardingGrid = () => `V2/Manage/System/Bankintegration/Onboarding/Grid`;

//post, takes args: (model: number)
export const sendAuthorizationResponse = () => `V2/Manage/System/Bankintegration/Onboarding/SendAuthorizationResponse`;

//get
export const getBankintegrationOnboarding = (onboardingrequestId: number) => `V2/Manage/System/Bankintegration/Onboarding/${onboardingrequestId}`;

//get
export const getBankintegrationBanks = () => `V2/Manage/System/Bankintegration/Banks`;

//post, takes args: (filter: number)
export const getIncomingEmails = () => `V2/Manage/System/Communicator/IncomingEmail/Grid`;

//get
export const getIncomingEmail = (incomingEmailId: number) => `V2/Manage/System/Communicator/IncomingEmail/${incomingEmailId}`;

//get
export const getIncomingEmailAttachment = (attachmentId: number) => `V2/Manage/System/Communicator/IncomingEmail/Attachment/${attachmentId}`;


