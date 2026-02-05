


//Available methods for ImportDynamicController

//post, takes args: (fileType: number, uploadFile: number)
export const getFileContent = (fileType: number) => `V2/Core/ImportDynamic/GetFileContent/${fileType}`;

//post, takes args: (model: number)
export const parseRows = () => `V2/Core/ImportDynamic/ParseRows`;


