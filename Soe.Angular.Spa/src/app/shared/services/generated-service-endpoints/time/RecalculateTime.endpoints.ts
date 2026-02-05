


//Available methods for RecalculateTimeController

//get
export const getRecalculateTimeHeads = (recalculateAction: number, loadRecords: boolean, showHistory: boolean, setExtensionNames: boolean, dateFromString: string, dateToString: string, limitNbrOfHeads?: number) => `V2/Time/RecalculateTime/RecalculateTimeHead?recalculateAction=${recalculateAction}&loadRecords=${loadRecords}&showHistory=${showHistory}&setExtensionNames=${setExtensionNames}&dateFromString=${encodeURIComponent(dateFromString)}&dateToString=${encodeURIComponent(dateToString)}&limitNbrOfHeads=${limitNbrOfHeads}`;

//get
export const getRecalculateTimeHead = (recalculateTimeHeadId: number, loadRecords: boolean, setExtensionNames: boolean) => `V2/Time/RecalculateTime/RecalculateTimeHead/${recalculateTimeHeadId}/${loadRecords}/${setExtensionNames}`;

//get
export const getRecalculateTimeHeadId = (key: string) => `V2/Time/RecalculateTime/RecalculateTimeHeadId/${encodeURIComponent(key)}`;

//post, takes args: (model: number)
export const setRecalculateTimeHeadToProcessed = () => `V2/Time/RecalculateTime/RecalculateTimeHead/SetToProcessed`;

//delete
export const cancelRecalculateTimeHead = (recalculateTimeHeadId: number) => `V2/Time/RecalculateTime/RecalculateTimeHead/${recalculateTimeHeadId}`;

//delete
export const cancelRecalculateTimeRecord = (recalculateTimeRecordId: number) => `V2/Time/RecalculateTime/RecalculateTimeRecord/${recalculateTimeRecordId}`;


