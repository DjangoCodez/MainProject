


//Available methods for EndReasonController

//get
export const getEndReasonsGrid = (endReasonId?: number) => `V2/Time/EndReason/Grid?endReasonId=${endReasonId}`;

//get
export const getEndReason = (endReasonId: number) => `V2/Time/EndReason/EndReason/${endReasonId}`;

//post, takes args: (endReasonDTO: number)
export const saveEndReason = () => `V2/Time/EndReason/EndReason`;

//post, takes args: (model: number)
export const updateEndReasonsState = () => `V2/Time/EndReason/UpdateState`;

//delete
export const deleteEndReason = (endReasonId: number) => `V2/Time/EndReason/EndReason/${endReasonId}`;


