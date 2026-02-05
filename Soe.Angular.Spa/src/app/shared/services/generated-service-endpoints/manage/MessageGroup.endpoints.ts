


//Available methods for MessageGroupController

//get
export const getMessageGroups = () => `V2/Manage/Registry/MessageGroup`;

//get
export const getMessageGroupsGrid = () => `V2/Manage/Registry/MessageGroup/Grid`;

//get
export const getMessageGroupsDict = () => `V2/Manage/Registry/MessageGroup/Dict`;

//get
export const getMessageGroupUsersByAccount = (accountId: number) => `V2/Manage/Registry/MessageGroup/UsersByAccount/${accountId}`;

//get
export const getMessageGroupUsersByCategory = (categoryId: number) => `V2/Manage/Registry/MessageGroup/UsersByCategory/${categoryId}`;

//get
export const getMessageGroupUsersByEmployeeGroup = (employeeGroupId: number) => `V2/Manage/Registry/MessageGroup/UsersByEmployeeGroup/${employeeGroupId}`;

//get
export const getMessageGroupUsersByRole = (roleId: number) => `V2/Manage/Registry/MessageGroup/UsersByRole/${roleId}`;

//get
export const getMessageGroup = (messageGroupId: number) => `V2/Manage/Registry/MessageGroup/${messageGroupId}`;

//post, takes args: (messageGroupDTO: number)
export const saveMessageGroup = () => `V2/Manage/Registry/MessageGroup`;

//delete
export const deleteMessageGroup = (messageGroupId: number) => `V2/Manage/Registry/MessageGroup/${messageGroupId}`;


