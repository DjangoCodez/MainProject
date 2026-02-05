


//Available methods for PaymentInformationController

//get
export const getBicFromIban = (iban: string) => `V2/Economy/PaymentInformation/BicFromIban/${encodeURIComponent(iban)}`;

//get
export const isIbanValid = (iban: string) => `V2/Economy/PaymentInformation/IsIbanValid/${encodeURIComponent(iban)}`;


