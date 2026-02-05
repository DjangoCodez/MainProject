


//Available methods for TimeStampV2Controller

//get
export const getTimeStamp = (timeStampEntryId: number) => `V2/Time/TimeStamp/TimeStamp/${timeStampEntryId}`;

//get
export const getTimeStampEntryUserAgentClientInfo = (timeStampEntryId: number) => `V2/Time/TimeStamp/TimeStamp/UserAgentClientInfo/${timeStampEntryId}`;

//post, takes args: (model: number)
export const searchTimeStamps = () => `V2/Time/TimeStamp/Search`;

//post, takes args: (items: number)
export const saveTimeStamps = () => `V2/Time/TimeStamp/TimeStamp/Save/`;


