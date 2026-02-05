


//Available methods for ContactPersonsController

//get
export const getContactPersons = (contactpersonId?: number) => `V2/Core/ContactPerson/ContactPersons/${contactpersonId || ''}`;

//get
export const getContactPersonsByActorId = (actorId: number) => `V2/Core/ContactPerson/ContactPersonsByActorId/${actorId}`;

//get
export const getContactPersonsByActorIds = (actorIds: string) => `V2/Core/ContactPerson/ContactPersonsByActorIds/${encodeURIComponent(actorIds)}`;

//get
export const getContactPerson = (actorContactPersonId: number) => `V2/Core/ContactPerson/ContactPerson/${actorContactPersonId}`;

//get
export const getContactPersonCategories = (contactPersonId: number) => `V2/Core/ContactPerson/Categories/${contactPersonId}`;

//get
export const getContactPersonForExport = (actorId: number) => `V2/Core/ContactPerson/ContactPerson/Export/${actorId}`;

//post, takes args: (contactPerson: number)
export const saveContactPerson = () => `V2/Core/ContactPerson`;

//post, takes args: (contactPersonIds: number[])
export const deleteContactPersons = () => `V2/Core/ContactPerson/Delete/`;

//delete
export const deleteContactPerson = (contactPersonId: number) => `V2/Core/ContactPerson/${contactPersonId}`;


