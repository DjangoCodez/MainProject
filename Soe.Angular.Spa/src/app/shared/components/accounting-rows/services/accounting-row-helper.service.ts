import { Injectable } from '@angular/core';
import { AccountingRowDTO, AmountStop } from '../models/accounting-rows-model';
import {
  IAccountDTO,
  IAccountInternalDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { IDecimalKeyValue } from '@shared/models/generated-interfaces/GenericType';

@Injectable({
  providedIn: 'root',
})
export class AccountingRowHelperService {
  constructor() {}

  //#region Public Helper Methods

  public setRowItemAccounts(
    rowItem: AccountingRowDTO,
    accountDims: any[],
    accountBalances: IDecimalKeyValue[],
    setInternalAccountFromAccount: boolean,
    account?: IAccountDTO,
    internalsFromStdIfMissing: boolean = false,
    resetInternals: boolean = true
  ): void {
    this.setBaseAccountFields(rowItem, account, accountBalances);

    if (setInternalAccountFromAccount) {
      this.setInternals(
        rowItem,
        account,
        accountDims,
        resetInternals,
        !resetInternals
      );
    } else if (internalsFromStdIfMissing) {
      this.fillMissingInternals(rowItem, account, accountDims);
    } else {
      this.updateInternalNames(rowItem, account, accountDims);
    }
  }

  //#endregion

  //#region Private Helper Methods

  private setBaseAccountFields(
    rowItem: AccountingRowDTO,
    account: IAccountDTO | undefined,
    accountBalances: { key: number; value: number }[]
  ): void {
    rowItem.dim1Id = account?.accountId ?? 0;
    rowItem.dim1Nr = account?.accountNr ?? '';
    rowItem.dim1Name = account?.name ?? '';
    rowItem.dim1Disabled = false;
    rowItem.dim1Mandatory = true;
    rowItem.dim1Stop = true;
    rowItem.quantityStop = account?.unitStop ?? false;
    rowItem.unit = account?.unit ?? '';
    rowItem.amountStop = account?.amountStop ?? AmountStop.DebitAmountStop;
    rowItem.rowTextStop = account?.rowTextStop ?? true;
    rowItem.isAccrualAccount = account?.isAccrualAccount ?? false;
    rowItem.balance = account
      ? accountBalances.find(x => x.key === account.accountId)?.value ?? 0
      : 0;
  }

  private setInternals(
    rowItem: AccountingRowDTO,
    account: IAccountDTO | undefined,
    accountDims: any[],
    clearBefore: boolean,
    requireId: boolean = false
  ): void {
    if (clearBefore) {
      for (let i = 2; i <= 6; i++) this.clearDim(rowItem, i);
    }

    account?.accountInternals?.forEach(ai => {
      if (!requireId || ai.accountId > 0) {
        this.setDimFromInternal(rowItem, ai, accountDims);
      }
    });
  }

  private fillMissingInternals(
    rowItem: any,
    account: IAccountDTO | undefined,
    accountDims: any[]
  ): void {
    account?.accountInternals?.forEach(ai => {
      if (ai.accountDimNr <= 1) return;

      const index =
        accountDims.findIndex(ad => ad.accountDimNr === ai.accountDimNr) + 1;
      const isEmpty = !rowItem[`dim${index}Id`];
      const shouldFill = isEmpty || ai.mandatoryLevel === 1;

      if (shouldFill) this.setDimFromInternal(rowItem, ai, accountDims);
    });
  }

  private updateInternalNames(
    rowItem: any,
    account: IAccountDTO | undefined,
    accountDims: any[]
  ): void {
    let index = 1;

    accountDims
      .filter((d: any) => d.accountDimNr !== 1)
      .forEach((dim: any) => {
        index++;
        const internalAccount = dim.accounts.find(
          (a: any) => a.accountId === rowItem[`dim${index}Id`]
        );

        rowItem[`dim${index}Nr`] =
          internalAccount?.accountNr ?? rowItem[`dim${index}Nr`] ?? '';
        rowItem[`dim${index}Name`] =
          internalAccount?.name ?? rowItem[`dim${index}Name`] ?? '';

        const dimAccount = account?.accountInternals?.find(
          ai => ai.accountDimNr === dim.accountDimNr
        );

        rowItem[`dim${index}Disabled`] =
          dimAccount?.mandatoryLevel === 1 || false;
        rowItem[`dim${index}Mandatory`] =
          dimAccount?.mandatoryLevel === 2 || false;
        rowItem[`dim${index}Stop`] = dimAccount?.mandatoryLevel === 3 || false;
      });
  }

  private clearDim(rowItem: any, index: number): void {
    rowItem[`dim${index}Id`] = 0;
    rowItem[`dim${index}Nr`] = '';
    rowItem[`dim${index}Name`] = '';
    rowItem[`dim${index}Disabled`] = false;
    rowItem[`dim${index}Mandatory`] = false;
    rowItem[`dim${index}Stop`] = false;
  }

  private setDimFromInternal(
    rowItem: any,
    ai: IAccountInternalDTO,
    accountDims: any[]
  ): void {
    if (ai.accountDimNr <= 1) return;

    const index =
      accountDims.findIndex(ad => ad.accountDimNr === ai.accountDimNr) + 1;

    if (ai.accountId > 0) {
      rowItem[`dim${index}Id`] = ai.accountId;
      rowItem[`dim${index}Nr`] = ai.accountNr;
      rowItem[`dim${index}Name`] = ai.name;
    }

    rowItem[`dim${index}Disabled`] = ai.mandatoryLevel === 1;
    rowItem[`dim${index}Mandatory`] = ai.mandatoryLevel === 2;
    rowItem[`dim${index}Stop`] = ai.mandatoryLevel === 3;
  }

  //#endregion
}
