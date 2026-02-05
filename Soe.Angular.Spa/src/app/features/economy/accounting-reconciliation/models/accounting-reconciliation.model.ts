import { ReconciliationRowType } from '@shared/models/generated-interfaces/Enumerations';
import { IReconciliationRowDTO } from '@shared/models/generated-interfaces/ReconciliationRowDTO';

export class AccountingReconciliationFilterDTO {
  fromDate?: Date;
  toDate?: Date;
  fromAccount?: string;
  toAccount?: string;
  currentAccountYearId?: number;
  currentAccountDimId?: number;
}

export class ReconciliationRowDTO implements IReconciliationRowDTO {
  type!: ReconciliationRowType;
  actorCompanyId!: number;
  accountId!: number;
  accountYearId!: number;
  associatedId!: number;
  rowStatus!: number;
  originType!: number;
  voucherSeriesId!: number;
  showInfo!: boolean;
  account!: string;
  name!: string;
  number!: string;
  typeName!: string;
  voucherSeriesTypeName!: string;
  customerAmount!: number;
  supplierAmount!: number;
  paymentAmount!: number;
  ledgerAmount!: number;
  diffAmount!: number;
  fromDate!: Date;
  toDate!: Date;
  date!: Date;
  attestStateColor!: string;
  attestStateName!: string;
}
