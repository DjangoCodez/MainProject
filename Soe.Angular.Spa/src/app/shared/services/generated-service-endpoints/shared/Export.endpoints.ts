


//Available methods for ExportController

//get
export const getExportsGrid = (module: number, exportId?: number) => `V2/Shared/Export/Grid/${module}?exportId=${exportId}`;

//get
export const getExport = (exportId: number) => `V2/Shared/Export/${exportId}`;

//post, takes args: (model: number)
export const saveExport = () => `V2/Shared/Export`;

//delete
export const deleteExport = (exportId: number) => `V2/Shared/Export/${exportId}`;

//get
export const getExportDefinitionsGrid = (exportDefinitionId?: number) => `V2/Shared/Export/ExportDefinition/Grid?exportDefinitionId=${exportDefinitionId}`;

//get
export const getExportDefinitionsDict = (addEmptyRow: boolean) => `V2/Shared/Export/ExportDefinition/Dict/${addEmptyRow}`;

//get
export const getExportDefinition = (exportDefinitionId: number) => `V2/Shared/Export/ExportDefinition/${exportDefinitionId}`;

//post, takes args: (model: number)
export const saveExportDefinition = () => `V2/Shared/Export/ExportDefinition`;


