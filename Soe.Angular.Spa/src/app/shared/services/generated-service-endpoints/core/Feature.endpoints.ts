


//Available methods for FeatureController

//get
export const hasReadOnlyPermissions = (featureIds: string) => `V2/Core/Feature/ReadOnlyPermission/${encodeURIComponent(featureIds)}`;

//get
export const hasModifyPermissions = (featureIds: string) => `V2/Core/Feature/ModifyPermission/${encodeURIComponent(featureIds)}`;


