


//Available methods for TermController

//get
export const getTranslationPart = (lang: string, part: string) => `V2/Core/Translation/${encodeURIComponent(lang)}/${encodeURIComponent(part)}`;

//get
export const getTranslations = (recordType: number, recordId: number, loadLangName: boolean) => `V2/Core/Translation/${recordType}/${recordId}/${loadLangName}`;

//get
export const getTermGroupContent = (sysTermGroupId: number, addEmptyRow: boolean, skipUnknown: boolean, sortById: boolean) => `V2/Core/SysTermGroup/${sysTermGroupId}/${addEmptyRow}/${skipUnknown}/${sortById}`;


