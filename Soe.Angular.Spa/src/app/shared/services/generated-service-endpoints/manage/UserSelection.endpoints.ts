


//Available methods for UserSelectionController

//get
export const getUserSelections = (type: number) => `V2/Manage/UserSelection/List/${type}`;

//get
export const getUserSelectionsDict = (type: number) => `V2/Manage/UserSelection/Dict/${type}`;

//get
export const getUserSelection = (userSelectionId: number) => `V2/Manage/UserSelection/${userSelectionId}`;

//post, takes args: (dto: number)
export const saveUserSelection = () => `V2/Manage/UserSelection`;

//delete
export const deleteUserSelection = (userSelectionId: number) => `V2/Manage/UserSelection/${userSelectionId}`;


