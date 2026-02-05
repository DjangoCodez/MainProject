


//Available methods for FieldSettingController

//get
export const getFieldSettings = (type: number, formId?: number) => `V2/Manage/FieldSetting/Grid/${type}/${formId || ''}`;

//post, takes args: (fieldSetting: number)
export const saveFieldSetting = () => `V2/Manage/FieldSetting/FieldSettings/`;


