


//Available methods for EdiMessageController

//get
export const getSysEdiMessageHeads = () => `V2/Manage/System/Edi/SysEdiMessageHead`;

//get
export const getSysEdiMessageHeadMsg = (sysEdiMessageHeadId: number) => `V2/Manage/System/Edi/SysEdiMessageHeadMsg/${sysEdiMessageHeadId}`;

//get
export const sysEdiMessageGridHead = (status: number, take: number, missingSysCompanyId: boolean, ediMessageHeadId?: number) => `V2/Manage/System/Edi/SysEdiMessageGridHead/${status}/${take}/${missingSysCompanyId}/${ediMessageHeadId || ''}`;

//get
export const sysEdiMessagesGrid = (open: boolean, closed: boolean, raw: boolean, missingSysCompanyId: boolean) => `V2/Manage/System/Edi/SysEdiMessagesGrid/${open}/${closed}/${raw}/${missingSysCompanyId}`;

//get
export const getSysEdiMessageHead = (sysEdiMessageHead: number) => `V2/Manage/System/Edi/SysEdiMessageHead/${sysEdiMessageHead}`;

//post, takes args: (model: number)
export const sysEdiMessageHead = () => `V2/Manage/System/Edi/SysEdiMessageHead`;


