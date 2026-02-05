import { Injectable } from '@angular/core';
import { IReconciliationRowDTO } from '@shared/models/generated-interfaces/ReconciliationRowDTO';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  getReconciliationPerAccount,
  getReconciliationRows,
} from '@shared/services/generated-service-endpoints/economy/AccountReconciliation.endpoints';
import { concatMap, map, Observable, of } from 'rxjs';
import { ReconciliationRowDTO } from '../models/accounting-reconciliation.model';
import { TranslateService } from '@ngx-translate/core';
import { ReconciliationRowType } from '@shared/models/generated-interfaces/Enumerations';
import { VoucherService } from '@features/economy/voucher/services/voucher.service';
import { VoucherSeriesDTO } from '@features/economy/account-years-and-periods/models/account-years-and-periods.model';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class AccountingReconciliationService {
  constructor(
    private http: SoeHttpClient,
    private translate: TranslateService,
    private voucherService: VoucherService
  ) {}

  getGrid(id?: number) {
    return of([] as ReconciliationRowDTO[]);
  }

  save(model: IReconciliationRowDTO) {
    return of({} as BackendResponse);
  }

  delete(id: number) {
    return of({} as BackendResponse);
  }

  getRows(
    dim1Id: number,
    fromDim1: string,
    toDim1: string,
    fromDate: Date,
    toDate: Date
  ): Observable<ReconciliationRowDTO[]> {
    return this.http.get(
      getReconciliationRows(
        dim1Id,
        fromDim1,
        toDim1,
        fromDate.toDateTimeString(),
        toDate.toDateTimeString()
      )
    );
  }

  getReconciliationPerAccount(
    accountId: number,
    fromDate: Date,
    toDate: Date,
    accountYearId: number
  ): Observable<ReconciliationRowDTO[]> {
    return this.http
      .get(
        getReconciliationPerAccount(
          accountId,
          fromDate.toDateTimeString(),
          toDate.toDateTimeString()
        )
      )
      .pipe(
        concatMap(res => {
          const rows = res as ReconciliationRowDTO[];
          return this.voucherService
            .getVoucherSeriesByYear(accountYearId, false)
            .pipe(
              map(res => {
                const voucherSeries = res as VoucherSeriesDTO[];
                rows.forEach(row => {
                  row.voucherSeriesTypeName = '';
                  voucherSeries.forEach(voucherSeriesItem => {
                    if (
                      voucherSeriesItem.voucherSeriesId == row.voucherSeriesId
                    ) {
                      row.voucherSeriesTypeName =
                        voucherSeriesItem.voucherSeriesTypeName;
                    }
                  });
                });
                return rows;
              })
            );
        }),
        map(res => {
          const rows = res as ReconciliationRowDTO[];
          rows.forEach(row => {
            switch (row.type) {
              case ReconciliationRowType.Voucher:
                row.typeName = this.translate.instant(
                  'economy.accounting.voucher.voucher'
                );
                break;
              case ReconciliationRowType.CustomerInvoice:
                row.typeName = this.translate.instant(
                  'economy.accounting.accountdistribution.customerinvoice'
                );
                break;
              case ReconciliationRowType.SupplierInvoice:
                row.typeName = this.translate.instant(
                  'economy.supplier.invoice.invoice'
                );
                break;
              case ReconciliationRowType.Payment:
                row.typeName = this.translate.instant(
                  'economy.accounting.reconciliation.paymentamount'
                );
                row.name = `${this.translate.instant(
                  'economy.accounting.reconciliation.paymentinvoice'
                )} ${row.name}`;
                break;
              default:
                row.typeName = '';
                break;
            }

            switch (row.rowStatus) {
              case 1:
                row.attestStateColor = '#2ACE2A';
                row.attestStateName = this.translate.instant('common.green');
                break;
              case 2:
                row.attestStateColor = '#FCFF00';
                row.attestStateName = this.translate.instant('common.yellow');
                break;
              case 3:
                row.attestStateColor = '#FF3D3D'; //"#FF0000";
                row.attestStateName = this.translate.instant('common.red');
                break;
              default:
                row.attestStateColor = '#2ACE2A';
                row.attestStateName = this.translate.instant('common.green');
                break;
            }
          });

          return rows;
        })
      );
  }
}
