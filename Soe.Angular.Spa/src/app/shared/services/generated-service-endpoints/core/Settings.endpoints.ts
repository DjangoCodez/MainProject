//Available methods for SettingsController

//get
export const getUserSettings = (settingTypeIds: string) =>
  `V2/Core/UserCompanySetting/User/${encodeURIComponent(settingTypeIds)}`;

//get
export const getCompanySettings = (settingTypeIds: string) =>
  `V2/Core/UserCompanySetting/Company/${encodeURIComponent(settingTypeIds)}`;

//get
export const getUserAndCompanySettings = (settingTypeIds: string) =>
  `V2/Core/UserCompanySetting/UserAndCompany/${encodeURIComponent(
    settingTypeIds
  )}`;

//get
export const getLicenseSettings = (settingTypeIds: string) =>
  `V2/Core/UserCompanySetting/License/${encodeURIComponent(settingTypeIds)}`;

//get
export const getLicenseSettingsForEdit = () =>
  `V2/Core/UserCompanySetting/License/ForEdit`;

//post, takes args: (settings: number)
export const saveUserCompanySettings = () => `V2/Core/UserCompanySetting`;

//get
export const getBoolSetting = (settingMainType: number, settingType: number) =>
  `V2/Core/UserCompanySetting/Bool/${settingMainType}/${settingType}`;

//get
export const getIntSetting = (settingMainType: number, settingType: number) =>
  `V2/Core/UserCompanySetting/Int/${settingMainType}/${settingType}`;

//get
export const getStringSetting = (
  settingMainType: number,
  settingType: number
) => `V2/Core/UserCompanySetting/String/${settingMainType}/${settingType}`;

//post, takes args: (model: number)
export const saveBoolSetting = () => `V2/Core/UserCompanySetting/Bool`;

//post, takes args: (model: number)
export const saveIntSetting = () => `V2/Core/UserCompanySetting/Int`;

//post, takes args: (model: number)
export const saveStringSetting = () => `V2/Core/UserCompanySetting/String`;


