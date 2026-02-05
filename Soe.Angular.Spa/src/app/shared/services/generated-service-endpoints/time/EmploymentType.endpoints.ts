


//Available methods for EmploymentTypeController

//get
export const getEmploymentTypesGrid = (employmentTypeId?: number) => `V2/Time/EmploymentType/Grid/${employmentTypeId || ''}`;

//get
export const getEmploymentType = (employmentTypeId: number) => `V2/Time/EmploymentType/${employmentTypeId}`;

//post, takes args: (model: number)
export const saveEmploymentType = () => `V2/Time/EmploymentType`;

//delete
export const deleteEmploymentType = (employmentTypeId: number) => `V2/Time/EmploymentType/${employmentTypeId}`;

//get
export const getStandardEmploymentTypes = () => `V2/Time/EmploymentType/StandardEmploymentTypes`;

//post, takes args: (model: number)
export const updateEmploymentTypesState = () => `V2/Time/EmploymentType/UpdateState`;


