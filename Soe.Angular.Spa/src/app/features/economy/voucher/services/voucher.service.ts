import { Injectable } from '@angular/core';
import { IVoucherGridDTO } from '@shared/models/generated-interfaces/VoucherHeadDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  deleteVouchersOnlySuperSupport,
  deleteVoucherOnlySuperSupport,
  getGridVoucherTemplates,
  getVoucherRowHistory,
  getVoucherRows,
  getVouchersBySeries,
  editVoucherNrOnlySuperSupport,
  getSmallVoucherTemplates,
  deleteVoucher,
  saveVoucher,
  getVoucher,
} from '@shared/services/generated-service-endpoints/economy/Voucher.endpoints';
import {
  getAccountYear,
  getAccountYearId,
  getAllAccountYears,
} from '@shared/services/generated-service-endpoints/economy/AccountYear.endpoints';
import { BehaviorSubject, Observable, map, of, tap } from 'rxjs';
import {
  IAccountPeriodDTO,
  IAccountYearDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import {
  getDefaultVoucherSeriesId,
  getVoucherSeriesByYear,
} from '@shared/services/generated-service-endpoints/economy/VoucherSeries.endpoints';
import { VoucherSeriesDTO } from '../../account-years-and-periods/models/account-years-and-periods.model';
import {
  CalculateAccountBalanceForAccountsFromVoucherModel,
  EditVoucherNrModel,
  SaveVoucherModel,
  VoucherGridDTO,
  VoucherGridFilterDTO,
  VoucherHeadDTO,
} from '../models/voucher.model';
import { getCompanySettingReportId } from '@shared/services/generated-service-endpoints/report/ReportV2.endpoints';
import { IVoucherRowDTO } from '@shared/models/generated-interfaces/VoucherRowDTOs';
import { IVoucherRowHistoryViewDTO } from '@shared/models/generated-interfaces/VoucherRowHistoryDTOs';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { calculateAccountBalanceForAccountsFromVoucher } from '@shared/services/generated-service-endpoints/economy/AccountBalance.endpoints';
import {
  getAccountPeriod,
  updateAccountPeriodStatus,
} from '@shared/services/generated-service-endpoints/economy/AccountPeriod.endpoints';
import { TermCollection } from '@shared/localization/term-types';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class VoucherService {
  constructor(private http: SoeHttpClient) {}

  filter = new VoucherGridFilterDTO();
  private terms: TermCollection = {};

  private gridFilterSubject = new BehaviorSubject<VoucherGridFilterDTO>(
    this.filter
  );

  private isTemplateSubject = new BehaviorSubject<boolean>(false);
  readonly gridFilter$ = this.gridFilterSubject.asObservable();
  readonly isTemplateSubject$ = this.isTemplateSubject.asObservable();

  setTerms(terms: TermCollection) {
    this.terms = terms;
  }
  setIsTemplateSubject(isTemplate: boolean) {
    this.isTemplateSubject.next(isTemplate);
  }

  setFilterSubject(filter: VoucherGridFilterDTO) {
    this.filter = filter;
    this.gridFilterSubject.next(filter);
  }

  public setIconAndColor(voucher: VoucherGridDTO) {
    voucher.expander = '';

    if (voucher.modified) {
      voucher.modifiedTooltip =
        this.terms['economy.accounting.voucher.vouchermodified'];
      voucher.modifiedIconValue = 'exclamation-circle';
      voucher.modifiedIconClass = 'warningColor';
    }
    if (voucher.hasDocuments) {
      voucher.hasDocumentsTooltip = this.terms['core.attachments'];
      voucher.hasDocumentsIconValue = 'paperclip';
    }

    if (voucher.hasNoRows) {
      voucher.accRowsIconValue = 'ban';
      voucher.accRowsIconClass = 'errorColor';
      voucher.accRowsText =
        this.terms['economy.accounting.voucher.missingrows'];
    } else if (voucher.hasUnbalancedRows) {
      voucher.accRowsIconValue = 'siren-on';
      voucher.accRowsIconClass = 'errorColor';
      voucher.accRowsText =
        this.terms['economy.accounting.voucher.unbalancedrowswarning'];
    } else {
      voucher.accRowsIconValue = '';
      voucher.accRowsText = '';
    }
  }

  getGrid(id?: number): Observable<VoucherGridDTO[]> {
    if (this.isTemplateSubject.value) {
      return this.getVoucherTemplates(this.filter.accountYearId, id).pipe(
        map(rows => {
          return rows.map(row => {
            this.setIconAndColor(row);
            return row;
          });
        })
      );
    } else {
      return this.getVouchersBySeries(
        this.filter.accountYearId,
        this.filter.voucherSeriesTypeId,
        id
      ).pipe(
        map(rows => {
          return rows.map(row => {
            this.setIconAndColor(row);
            return row;
          });
        })
      );
    }
  }

  getVoucherTemplates(accountYearId: number, voucherHeadId?: number) {
    return this.http
      .get<
        VoucherGridDTO[]
      >(getGridVoucherTemplates(accountYearId, voucherHeadId))
      .pipe(
        tap(vouchers => {
          vouchers.map(voucher => {
            const obj = new VoucherGridDTO();
            Object.assign(obj, voucher);
            obj.fixDates();
            return obj;
          });
        })
      );
  }
  getVouchersBySeries(
    accountYearId: number,
    voucherSeriesTypeId: number,
    voucherHeadId?: number
  ) {
    return this.http
      .get<
        VoucherGridDTO[]
      >(getVouchersBySeries(accountYearId, voucherSeriesTypeId, voucherHeadId))
      .pipe(
        tap(vouchers => {
          vouchers.map(voucher => {
            const obj = new VoucherGridDTO();
            Object.assign(obj, voucher);
            obj.fixDates();
            return obj;
          });
        })
      );
  }

  get(id: number) {
    return this.http.get<VoucherHeadDTO>(
      getVoucher(id, false, true, true, false)
    );
  }

  getVoucher(
    voucherHeadId: number,
    loadVoucherSeries: boolean,
    loadVoucherRows: boolean,
    loadVoucherRowAccounts: boolean,
    loadAccountBalance: boolean
  ) {
    return this.http.get<VoucherHeadDTO>(
      getVoucher(
        voucherHeadId,
        loadVoucherSeries,
        loadVoucherRows,
        loadVoucherRowAccounts,
        loadAccountBalance
      )
    );
  }

  getAccountYears(addEmptyRow: boolean, excludeNew: boolean) {
    return this.http.get<IAccountYearDTO[]>(
      getAllAccountYears(addEmptyRow, excludeNew)
    );
  }
  getVoucherTemplatesDict(accountYearId: number) {
    return this.http.get<ISmallGenericType[]>(
      getSmallVoucherTemplates(accountYearId)
    );
  }

  getVoucherSeriesByYear(accountYearId: number, includeTemplate: boolean) {
    return this.http
      .get<
        VoucherSeriesDTO[]
      >(getVoucherSeriesByYear(accountYearId, includeTemplate))
      .pipe(
        map(data => {
          return data.map(item => {
            const obj = new VoucherSeriesDTO();
            Object.assign(obj, item);
            obj.voucherSeriesTypeNumberName = `${obj.voucherSeriesTypeNr} - ${obj.voucherSeriesTypeName}`;
            return obj;
          });
        }),
        tap(data => {
          data = data.sort((a, b) =>
            a.voucherSeriesTypeNr > b.voucherSeriesTypeNr
              ? 1
              : a.voucherSeriesTypeNr === b.voucherSeriesTypeNr
                ? 0
                : -1
          );
        })
      );
  }

  getCompanySettingReportId(
    settingMainType: number,
    settingType: number,
    reportTemplateType: number
  ) {
    return this.http.get<number>(
      getCompanySettingReportId(
        settingMainType,
        settingType,
        reportTemplateType
      )
    );
  }
  getVoucherRows(voucherHeadId: number) {
    return this.http.get<IVoucherRowDTO[]>(getVoucherRows(voucherHeadId));
  }
  getDefaultVoucherSeriesId(accountYearId: number, type: number) {
    return this.http.get<number>(
      getDefaultVoucherSeriesId(accountYearId, type)
    );
  }

  getVoucherRowHistory(voucherHeadId: number) {
    return this.http.get<IVoucherRowHistoryViewDTO[]>(
      getVoucherRowHistory(voucherHeadId)
    );
  }
  editVoucherNrOnlySuperSupport(model: EditVoucherNrModel): Observable<any> {
    return this.http.post<VoucherHeadDTO>(
      editVoucherNrOnlySuperSupport(),
      model
    );
  }

  deleteVouchersOnlySuperSupport(ids: number[]): Observable<any> {
    return this.http.delete(deleteVouchersOnlySuperSupport(ids));
  }

  saveVoucher(model: SaveVoucherModel): Observable<any> {
    return this.http.post<VoucherHeadDTO>(saveVoucher(), model);
  }

  save(model: VoucherHeadDTO): Observable<any> {
    return of();
  }

  deleteVoucher(id: number): Observable<any> {
    return this.http.delete(deleteVoucher(id));
  }
  calculateAccountBalanceForAccountsFromVoucher(
    model: CalculateAccountBalanceForAccountsFromVoucherModel
  ): Observable<any> {
    return this.http.post<VoucherHeadDTO>(
      calculateAccountBalanceForAccountsFromVoucher(),
      model
    );
  }
  getAccountYear(id: number, loadPeriods: boolean, useCache: boolean) {
    return this.http.get<IAccountYearDTO>(getAccountYearId(id, loadPeriods));
  }

  getAccountYearByDate(date: string, useCache: boolean) {
    return this.http.get<IAccountYearDTO>(getAccountYear(date), {
      useCache: useCache,
    });
  }
  updateAccountPeriodStatus(
    accountPeriodId: number,
    status: number
  ): Observable<any> {
    return this.http.post<any>(
      updateAccountPeriodStatus(accountPeriodId, status),
      { accountPeriodId: accountPeriodId, status: status }
    );
  }

  getAccountPeriod(
    accountYearId: number,
    date: string,
    includeAccountYear: boolean,
    useCache: boolean = false
  ) {
    return this.http.get<IAccountPeriodDTO>(
      getAccountPeriod(accountYearId, date, includeAccountYear),
      { useCache: useCache }
    );
  }

  delete(
    voucherHeadId: number,
    checkTransfer: boolean
  ): Observable<BackendResponse> {
    return this.http.delete(
      deleteVoucherOnlySuperSupport(voucherHeadId, checkTransfer)
    );
  }
}
