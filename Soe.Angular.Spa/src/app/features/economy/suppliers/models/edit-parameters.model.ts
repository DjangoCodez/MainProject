export class SupplierEditInputParameters {
  orgNumber?: string;
  bankAccounts: {
    iban?: string;
    bic?: string;
    pg?: string;
    bg?: string;
  };

  constructor() {
    this.bankAccounts = {};
  }
}
