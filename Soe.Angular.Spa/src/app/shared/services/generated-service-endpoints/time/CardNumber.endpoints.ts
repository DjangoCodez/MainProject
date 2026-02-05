


//Available methods for CardNumberController

//get
export const getCardNumbers = () => `V2/Time/CardNumber/Grid/`;

//get
export const cardNumberExists = (cardNumber: string, excludeEmployeeId: number) => `V2/Time/CardNumber/Exists/${encodeURIComponent(cardNumber)}/${excludeEmployeeId}`;

//delete
export const deleteCardNumber = (employeeId: number) => `V2/Time/CardNumber/${employeeId}`;


