


//Available methods for FollowupTypesController

//get
export const getFollowUpTypes = (followUpTypeId?: number) => `V2/Time/Employee/FollowUpType/Grid/${followUpTypeId || ''}`;

//get
export const getFollowUpType = (followUpTypeId: number) => `V2/Time/Employee/FollowUpType/${followUpTypeId}`;

//post, takes args: (followUpTypeDTO: number)
export const saveFollowUpType = () => `V2/Time/Employee/FollowUpType`;

//post, takes args: (model: number)
export const updateFollowUpTypesState = () => `V2/Time/Employee/FollowUpType/UpdateState`;

//delete
export const deleteFollowUpType = (followUpTypeId: number) => `V2/Time/Employee/FollowUpType/${followUpTypeId}`;


