


//Available methods for ContactAddressController

//get
export const getContactAddresses = (actorId: number, type: number, addEmptyRow: boolean, includeRows: boolean, includeCareOf: boolean) => `V2/Core/ContactAddress/${actorId}/${type}/${addEmptyRow}/${includeRows}/${includeCareOf}`;

//get
export const getContactAddressItemsDict = (contactPersonId: number) => `V2/Core/ContactAddressDict/${contactPersonId}`;

//get
export const getContactAddressItems = (actorId: number) => `V2/Core/ContactAddressItem/${actorId}`;

//get
export const getContactAddressItemsByUser = (userId: number) => `V2/Core/ContactAddressItem/ByUser/${userId}`;

//get
export const getSysContactAddressRowTypeIds = (sysContactTypeId: number) => `V2/Core/Address/AddressRowType/${sysContactTypeId}`;

//get
export const getSysContactAddressTypeIds = (sysContactTypeId: number) => `V2/Core/Address/AddressType/${sysContactTypeId}`;

//get
export const getSysContactEComTypeIds = (sysContactTypeId: number) => `V2/Core/Address/EComType/${sysContactTypeId}`;


