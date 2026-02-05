


//Available methods for SupportV2Controller

//get
export const getSysLogsGrid = (logType: number, showUnique: boolean) => `V2/Manage/Support/SysLog/LogType/${logType}/${showUnique}`;

//get
export const getSysLog = (sysLogId: number) => `V2/Manage/Support/SysLog/${sysLogId}`;

//post, takes args: (dto: number)
export const searchSysLogs = () => `V2/Manage/Support/SysLog/Search/`;


