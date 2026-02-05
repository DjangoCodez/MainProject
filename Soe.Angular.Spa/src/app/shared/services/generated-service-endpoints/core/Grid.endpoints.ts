


//Available methods for GridController

//get
export const getSysGridState = (grid: string) => `V2/Core/SysGridState/${encodeURIComponent(grid)}`;

//post, takes args: (model: number)
export const saveSysGridState = () => `V2/Core/SysGridState`;

//delete
export const deleteSysGridState = (grid: string) => `V2/Core/SysGridState/${encodeURIComponent(grid)}`;

//get
export const getUserGridState = (grid: string) => `V2/Core/UserGridState/${encodeURIComponent(grid)}`;

//post, takes args: (model: number)
export const saveUserGridState = () => `V2/Core/UserGridState`;

//delete
export const deleteUserGridState = (grid: string) => `V2/Core/UserGridState/${encodeURIComponent(grid)}`;


