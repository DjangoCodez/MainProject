import { Injectable } from '@angular/core';
import { ILiquidityPlanningDTO } from '@shared/models/generated-interfaces/LiquidityPlanningDTO';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  deleteLiquidityPlanningTransaction,
  getLiquidityPlanning,
  getLiquidityPlanningv2,
  saveLiquidityPlanningTransaction,
} from '@shared/services/generated-service-endpoints/economy/LiquidityPlanning.endpoints';
import { Observable } from 'rxjs';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class AccountingLiquidityPlanningService {
  constructor(private http: SoeHttpClient) {}

  getGridAdditionalProps: {
    from: Date;
    to: Date;
    exclusion?: Date;
    balance: number;
    unpaid: boolean;
    unchecked: boolean;
    checked: boolean;
  } = {
    from: new Date(),
    to: new Date(),
    exclusion: undefined,
    balance: 0,
    unpaid: false,
    unchecked: false,
    checked: false,
  };
  getGrid(
    id?: number,
    additionalProps?: {
      from: Date;
      to: Date;
      exclusion?: Date;
      balance: number;
      unpaid: boolean;
      unchecked: boolean;
      checked: boolean;
    }
  ): Observable<ILiquidityPlanningDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    const model = {
      from: this.getGridAdditionalProps.from,
      to: this.getGridAdditionalProps.to,
      exclusion: this.getGridAdditionalProps.exclusion,
      balance: this.getGridAdditionalProps.balance,
      unpaid: this.getGridAdditionalProps.unpaid,
      paidUnchecked: this.getGridAdditionalProps.unchecked,
      paidChecked: this.getGridAdditionalProps.checked,
    };

    return this.http.post(getLiquidityPlanning(), model);
  }

  getLiquidityPlanningNew(
    from: Date,
    to: Date,
    exclusion: Date | undefined,
    balance: number,
    unpaid: boolean,
    unchecked: boolean,
    checked: boolean
  ): Observable<ILiquidityPlanningDTO[]> {
    const model = {
      from: from,
      to: to,
      exclusion: exclusion,
      balance: balance,
      unpaid: unpaid,
      paidUnchecked: unchecked,
      paidChecked: checked,
    };
    return this.http.post(getLiquidityPlanningv2(), model);
  }

  saveLiquidityPlanningTransaction(
    liquidityPlanningTransaction: ILiquidityPlanningDTO
  ): Observable<BackendResponse> {
    return this.http.post(
      saveLiquidityPlanningTransaction(),
      liquidityPlanningTransaction
    );
  }

  deleteLiquidityPlanningTransaction(
    liquidityPlanningTransactionId: number
  ): Observable<BackendResponse> {
    return this.http.delete(
      deleteLiquidityPlanningTransaction(liquidityPlanningTransactionId)
    );
  }
}
