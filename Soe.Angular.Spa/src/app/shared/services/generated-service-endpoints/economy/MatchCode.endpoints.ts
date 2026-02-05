


//Available methods for MatchCodeController

//get
export const getMatchCodesGrid = (matchCodeId?: number) => `V2/Economy/MatchCode/Grid/${matchCodeId || ''}`;

//get
export const getMatchCodes = (matchCodeType: number, addEmptyRow: boolean) => `V2/Economy/MatchCode/ByType/${matchCodeType}/${addEmptyRow}`;

//get
export const getMatchCodesDict = (type: number) => `V2/Economy/MatchCode/Dict/${type}`;

//get
export const getMatchCode = (matchCodeId: number) => `V2/Economy/MatchCode/${matchCodeId}`;

//post, takes args: (matchCodeDTO: number)
export const saveMatchCode = () => `V2/Economy/MatchCode/MatchCode`;

//delete
export const deleteMatchCode = (matchCodeId: number) => `V2/Economy/MatchCode/${matchCodeId}`;


