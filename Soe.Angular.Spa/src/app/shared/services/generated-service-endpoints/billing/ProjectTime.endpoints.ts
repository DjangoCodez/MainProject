


//Available methods for ProjectTimeController

//post, takes args: (model: number)
export const getTimeBlocksForTimeSheetFiltered = () => `V2/Billing/Project/TimeBlocksForTimeSheetFiltered`;

//get
export const getTimeBlocksForTimeSheetFilteredByProject = (fromDate: string, toDate: string, projectId: number, includeChildProjects: boolean, employeeId: number) => `V2/Billing/Project/TimeBlocksForTimeSheetFilteredByProject/${encodeURIComponent(fromDate)}/${encodeURIComponent(toDate)}/${projectId}/${includeChildProjects}/${employeeId}`;

//get
export const getEmployeesForTimeProjectRegistrationSmall = (projectId: number, fromDateString: string, toDateString: string) => `V2/Billing/Project/Employees/Small/${projectId}/${encodeURIComponent(fromDateString)}/${encodeURIComponent(toDateString)}`;

//get
export const getEmployeeScheduleAndTransactionInfo = (employeeId: number, date: string) => `V2/Billing/Project/EmployeeScheduleAndTransactionInfo/${employeeId}/${encodeURIComponent(date)}`;

//get
export const getEmployeeFirstEligibleTime = (employeeId: number, date: string) => `V2/Billing/Project/EmployeeFirstTime/${employeeId}/${encodeURIComponent(date)}`;

//post, takes args: (projectTimeBlockSaveDTOs: number)
export const recalculateWorkTime = () => `V2/Billing/Project/RecalculateWorkTime`;

//post, takes args: (model: number)
export const moveTimeRowsToDate = () => `V2/Billing/Project/MoveTimeRowsToDate`;

//post, takes args: (model: number)
export const moveTimeRowsToOrder = () => `V2/Billing/Project/MoveTimeRowsToOrder`;

//post, takes args: (model: number)
export const moveTimeRowsToOrderRow = () => `V2/Billing/Project/MoveTimeRowsToOrderRow`;

//post, takes args: (projectTimeBlockSaveDTO: number)
export const saveNotesForProjectTimeBlock = () => `V2/Billing/Project/SaveNotesForProjectTimeBlock`;

//post, takes args: (items: number)
export const validateProjectTimeBlockSaveDTO = () => `V2/Billing/Project/ValidateProjectTimeBlockSaveDTO`;

//post, takes args: (projectTimeBlockSaveDTOs: number)
export const saveProjectTimeBlockSaveDTO = () => `V2/Billing/Project/ProjectTimeBlockSaveDTO`;

//post, takes args: (model: number)
export const getEmployeesForProject = () => `V2/Billing/Project/Employees`;

//get
export const getEmployeeChildren = (employeeId: number) => `V2/Billing/Project/EmployeeChilds/${employeeId}`;

//get
export const loadProjectTimeBlockForMatrix = (employeeId: number, selectedEmployeeId: number, dateFrom: string, dateTo: string, isCopying: boolean) => `V2/Billing/Project/TimeBlocksForMatrix/${employeeId}/${selectedEmployeeId}/${encodeURIComponent(dateFrom)}/${encodeURIComponent(dateTo)}/${isCopying}`;

//post, takes args: (projectTimeMatrixBlockDTOs: number)
export const saveProjectMatrix = () => `V2/Billing/Project/TimeBlocksForMatrix`;

//get
export const getProjectsForTimeSheet = (employeeId: number) => `V2/Billing/Project/Project/ProjectsForTimeSheet/${employeeId}`;

//get
export const getProjectsForTimeSheetEmployees = (empIds: number[], projectId?: number) => `V2/Billing/Project/Project/ProjectsForTimeSheet/Employees/${projectId || ''}?empIds=${empIds}`;


