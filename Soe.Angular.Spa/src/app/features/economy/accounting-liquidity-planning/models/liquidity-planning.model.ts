import { IGetLiquidityPlanningModel } from '@shared/models/generated-interfaces/EconomyModels';
import {
  LiquidityPlanningTransactionType,
  SoeOriginType,
} from '@shared/models/generated-interfaces/Enumerations';
import { ILiquidityPlanningDTO } from '@shared/models/generated-interfaces/LiquidityPlanningDTO';
import { DialogData, DialogSize } from '@ui/dialog/models/dialog';
import { Observable } from 'rxjs';

export class LiquidityPlanningDTO implements ILiquidityPlanningDTO {
  invoiceId: number;
  invoiceNr: string;
  originType: SoeOriginType;
  liquidityPlanningTransactionId?: number;
  date: Date;
  transactionType: LiquidityPlanningTransactionType;
  transactionTypeName: string;
  specification: string;
  valueIn: number;
  valueOut: number;
  total: number;
  created?: Date;
  createdBy: string;
  modified?: Date;
  modifiedBy: string;

  constructor(row: LiquidityPlanningDTO | null) {
    this.invoiceId = row?.invoiceId ?? 0;
    this.invoiceNr = row?.invoiceNr ?? '';
    this.originType = row?.originType ?? 0;
    this.date = row?.date ?? new Date();
    this.transactionType = row?.transactionType ?? 0;
    this.transactionTypeName = row?.transactionTypeName ?? '';
    this.liquidityPlanningTransactionId = row?.liquidityPlanningTransactionId;
    this.specification = row?.specification ?? '';
    this.valueIn = row?.valueIn ?? 0;
    this.valueOut = row?.valueOut ?? 0;
    this.total = row?.total ?? 0;
    this.createdBy = row?.createdBy ?? '';
    this.modifiedBy = row?.modifiedBy ?? '';
  }
}

export class LiquidityPlanningDialogData
  extends LiquidityPlanningDTO
  implements DialogData
{
  size?: DialogSize;
  title: string;
  content?: string;
  primaryText?: string;
  secondaryText?: string;
  disableClose?: boolean;
  disableContentScroll?: boolean;
  noToolbar?: boolean;
  hideFooter?: boolean;
  callbackAction?: () => Observable<unknown> | unknown | Promise<unknown>;

  constructor(row?: LiquidityPlanningDTO) {
    super(row ?? null);
    this.title = '';
  }
}

export class GetLiquidityPlanningModel implements IGetLiquidityPlanningModel {
  from: Date;
  to: Date;
  exclusion?: Date;
  balance: number;
  unpaid: boolean;
  paidUnchecked: boolean;
  paidChecked: boolean;
  selectedPaymentStatuses: number[];

  constructor() {
    this.from = new Date();
    this.to = new Date();
    this.balance = 0;
    this.unpaid = false;
    this.paidUnchecked = false;
    this.paidChecked = false;
    this.selectedPaymentStatuses = [];
  }
}

export interface ILiquidityPlanningChartModel {
  date: Date;
  outgoingLiquidity: number;
}
