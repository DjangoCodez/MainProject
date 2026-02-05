


//Available methods for ReportV2Controller

//get
export const getReportsDict = (sysReportTemplateTypeId: number, onlyOriginal: boolean, onlyStandard: boolean, addEmptyRow: boolean, useRole: boolean) => `V2/Report/Reports/${sysReportTemplateTypeId}/${onlyOriginal}/${onlyStandard}/${addEmptyRow}/${useRole}`;

//get
export const getReports = (actorCompanyId: number, sysReportTemplateTypeId: number, onlyOriginal: boolean, onlyStandard: boolean, addEmptyRow: boolean, useRole: boolean) => `V2/Report/Reports/${actorCompanyId}/${sysReportTemplateTypeId}/${onlyOriginal}/${onlyStandard}/${addEmptyRow}/${useRole}`;

//get
export const getReportViewsForModule = (module: number, onlyOriginal: boolean, onlyStandard: boolean) => `V2/Report/Reports/${module}/${onlyOriginal}/${onlyStandard}`;

//get
export const getReport = (reportId: number, loadReportSelection: boolean, loadSysReportTemplateType: boolean, loadReportRolePermission: boolean) => `V2/Report/Report/${reportId}/${loadReportSelection}/${loadSysReportTemplateType}/${loadReportRolePermission}`;

//get
export const getReportExportTypes = (sysReportTemplateId: number, userReportTemplateId: number, sysReportType: number) => `V2/Report/Report/ExportTypes/${sysReportTemplateId}/${userReportTemplateId}/${sysReportType}`;

//post, takes args: (reportDTO: number)
export const saveReport = () => `V2/Report/Report/Save/`;

//delete
export const deleteReport = (reportId: number) => `V2/Report/Report/${reportId}`;

//get
export const getStandardReport = (settingMainType: number, settingType: number, reportTemplateType: number) => `V2/Report/StandardReport/${settingMainType}/${settingType}/${reportTemplateType}`;

//get
export const getCompanySettingReportId = (settingMainType: number, settingType: number, reportTemplateType: number) => `V2/Report/StandardReportId/${settingMainType}/${settingType}/${reportTemplateType}`;

//get
export const getSettingOrStandardReport = (settingMainType: number, settingType: number, reportTemplateType: number, reportType: number) => `V2/Report/SettingOrStandardReportId/${settingMainType}/${settingType}/${reportTemplateType}/${reportType}`;

//get
export const settingReportCheckPermission = (settingMainType: number, settingType: number, reportTemplateType: number) => `V2/Report/SettingReportCheckPermission/${settingMainType}/${settingType}/${reportTemplateType}`;

//get
export const settingReportHasPermission = (settingMainType: number, settingType: number, reportTemplateType: number) => `V2/Report/SettingReportHasPermission/${settingMainType}/${settingType}/${reportTemplateType}`;

//get
export const getSmallDTOReportName = (reportId: number) => `V2/Report/Report/Small/${reportId}`;

//get
export const getReportName = (reportId: number) => `V2/Report/Report/${reportId}`;

//get
export const getReportViewsInPackage = (reportPackageId: number) => `V2/Report/ReportsInPackage/${reportPackageId}`;

//post, takes args: (model: number)
export const getReportsForTypes = () => `V2/Report/ReportsForTypes/`;

//post, takes args: (model: number)
export const getProjectsBySearchNoLimit = () => `V2/Report/Project/Search/`;

//get
export const getReportsFromFile = (dataStorageId: number) => `V2/Report/ReportImport/${dataStorageId}`;


