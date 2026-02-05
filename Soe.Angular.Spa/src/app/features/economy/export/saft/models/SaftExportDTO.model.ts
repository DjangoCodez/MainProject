export class SaftExportDTO implements ISaftExportDTO {
  voucherNr: number;
  rowNr: number;
  accountNr: string;
  accountName: string;
  createdDate: string;
  date: string;
  voucherText: string;
  debetAmount: number;
  creditAmount: number;
  amount: number;
  taxAmount: number;
  voucherSeriesTypeId: number;
  voucherHeadId: number;
  supplierCustomerName: string;

  constructor(
    _voucherNr: number,
    _rowNr: number,
    _accountNr: string,
    _accountName: string,
    _createdDate: string,
    _date: string,
    _voucherText: string,
    _debetAmount: number,
    _creditAmount: number,
    _amount: number,
    _taxAmount: number,
    _voucherSeriesTypeId: number,
    _voucherHeadId: number,
    _supplierCustomerName: string
  ) {
    this.voucherNr = _voucherNr;
    this.rowNr = _rowNr;
    this.accountNr = _accountNr;
    this.accountName = _accountName;
    this.createdDate = _createdDate;
    this.date = _date;
    this.voucherText = _voucherText;
    this.debetAmount = _debetAmount;
    this.creditAmount = _creditAmount;
    this.amount = _amount;
    this.taxAmount = _taxAmount;
    this.voucherSeriesTypeId = _voucherSeriesTypeId;
    this.voucherHeadId = _voucherHeadId;
    this.supplierCustomerName = _supplierCustomerName;
  }
}

export interface ISaftExportDTO {
  voucherNr: number;
  rowNr: number;
  accountNr: string;
  accountName: string;
  createdDate: string;
  date: string;
  voucherText: string;
  debetAmount: number;
  creditAmount: number;
  amount: number;
  taxAmount: number;
  voucherSeriesTypeId: number;
  voucherHeadId: number;
  supplierCustomerName: string;
}
