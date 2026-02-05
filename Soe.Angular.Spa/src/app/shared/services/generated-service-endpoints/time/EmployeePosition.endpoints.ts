


//Available methods for EmployeePositionController

//get
export const getPositionsGrid = (loadSkills: boolean, positionId?: number) => `V2/Time/Employee/Position/Grid/${loadSkills}/${positionId || ''}`;

//get
export const getPositionsDict = (addEmptyRow: boolean) => `V2/Time/Employee/Position/Dict?addEmptyRow=${addEmptyRow}`;

//get
export const getPositions = (loadSkills: boolean) => `V2/Time/Employee/Position?loadSkills=${loadSkills}`;

//get
export const getPosition = (employeePositionId: number, loadSkills: boolean) => `V2/Time/Employee/Position/${employeePositionId}/${loadSkills}`;

//post, takes args: (position: number)
export const savePosition = () => `V2/Time/Employee/Position`;

//post, takes args: ()
export const updatePositionGrid = () => `V2/Time/Employee/PositionGridUpdate`;

//post, takes args: (sysPositions: number)
export const updateAndLinkSysPositionGrid = () => `V2/Time/Employee/SysPositionGridUpdateAndLink`;

//post, takes args: (sysPositions: number)
export const updateSysPositionGrid = () => `V2/Time/Employee/SysPositionGridUpdate`;

//delete
export const deletePosition = (positionId: number) => `V2/Time/Employee/Position/${positionId}`;


