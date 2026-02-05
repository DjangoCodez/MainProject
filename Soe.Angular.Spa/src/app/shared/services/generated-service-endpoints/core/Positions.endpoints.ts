


//Available methods for PositionsController

//get
export const getSysPositionsGrid = (positionId?: number) => `V2/Core/SysPosition/Grid/${positionId || ''}`;

//get
export const getSysPositions = (sysCountryId: number, sysLanguageId: number) => `V2/Core/SysPosition/${sysCountryId}/${sysLanguageId}`;

//get
export const getSysPositionsDict = (sysCountryId: number, sysLanguageId: number, addEmptyRow: boolean) => `V2/Core/SysPosition/${sysCountryId}/${sysLanguageId}/${addEmptyRow}`;

//get
export const getSysPosition = (sysPositionId: number) => `V2/Core/SysPosition/${sysPositionId}`;

//post, takes args: (sysPosition: number)
export const saveSysPosition = () => `V2/Core/SysPosition`;

//delete
export const deleteSysPosition = (sysPositionId: number) => `V2/Core/SysPosition/${sysPositionId}`;


