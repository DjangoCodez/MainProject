


//Available methods for EmployeeRequestController

//get
export const getEmployeeRequestsGrid = (fromDate: number, toDate: number, employeeRequestId?: number) => `V2/Time/Schedule/EmployeeRequest/Grid/${encodeURIComponent(String(fromDate))}/${encodeURIComponent(String(toDate))}/${employeeRequestId || ''}`;


