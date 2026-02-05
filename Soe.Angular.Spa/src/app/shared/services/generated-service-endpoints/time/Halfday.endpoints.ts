


//Available methods for HalfdayController

//get
export const getHalfdaysGrid = (halfDayId?: number) => `V2/Time/Halfday/Grid?halfDayId=${halfDayId}`;

//get
export const getHalfday = (halfdayId: number) => `V2/Time/Halfday/${halfdayId}`;

//post, takes args: (model: number)
export const saveHalfday = () => `V2/Time/Halfday`;

//delete
export const deleteHalfday = (halfdayId: number) => `V2/Time/Halfday/${halfdayId}`;

//get
export const getHalfDayTypesDict = (addEmptyRow: boolean) => `V2/Time/Halfday/HalfDayTypesDict/${addEmptyRow}`;

//get
export const getDayTypesByCompanyDict = (addEmptyRow: boolean) => `V2/Time/Halfday/GetDayTypesByCompanyDict/${addEmptyRow}`;

//get
export const getTimeCodeBrakeDict = (addEmptyRow: boolean) => `V2/Time/Halfday/TimeCodeBrakeDict/${addEmptyRow}`;

//post, takes args: (model: number)
export const onAddHalfDay = () => `V2/Time/Halfday/OnAddHalfDay`;

//post, takes args: (model: number)
export const onUpdateHalfDay = () => `V2/Time/Halfday/OnUpdateHalfDay`;

//post, takes args: (model: number)
export const onDeleteHalfDay = () => `V2/Time/Halfday/OnDeleteHalfDay`;


