


//Available methods for SieController

//post, takes args: (exportDTO: number)
export const sieExport = () => `V2/Economy/Sie/Export`;

//post, takes args: (importDTO: number)
export const sieImport = () => `V2/Economy/Sie/Import`;

//post, takes args: (file: number)
export const sieImportReadFile = () => `V2/Economy/Sie/Import/ReadFile`;

//get
export const getSieImportHistory = () => `V2/Economy/Sie/Import/History`;

//post, takes args: (importReverseRequest: number)
export const reverseImport = () => `V2/Economy/Sie/Import/Reverse`;


