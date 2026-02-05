import { Component, OnInit, inject, signal } from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import {
  AccountPeriodDTO,
  AccountYearDTO,
  SaveAccountYearModel,
  VoucherSeriesDTO,
} from '../../../models/account-years-and-periods.model';
import { AccountYearService } from '../../../services/account-year.service';
import {
  Feature,
  TermGroup,
  TermGroup_AccountStatus,
  TermGroup_AccountYearStatus,
} from '@shared/models/generated-interfaces/Enumerations';
import { BehaviorSubject, Observable, of, tap } from 'rxjs';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { BrowserUtil } from '@shared/util/browser-util';
import { DateUtil } from '@shared/util/date-util';
import { Perform } from '@shared/util/perform.class';
import { AccountYearForm } from '../../../models/account-year-form.model';
import { VoucherService } from '@src/app/features/economy/voucher/services/voucher.service';
import { VoucherSeriesTypeService } from '@src/app/features/economy/services/voucher-series-type.service';
import { VoucherSeriesTypeDTO } from '@src/app/features/economy/models/voucher-series-type.model';
import { orderBy } from 'lodash';
import { IVoucherGridDTO } from '@shared/models/generated-interfaces/VoucherHeadDTOs';
import { GrossProfitCodeDTO } from '@src/app/features/economy/gross-profit-codes/models/gross-profit-codes.model';
import { GrossProfitCodesService } from '@src/app/features/economy/gross-profit-codes/services/gross-profit-codes.service';
import { CrudActionTypeEnum } from '@shared/enums';
import { StorageService } from '@shared/services/storage.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Component({
  selector: 'soe-account-year-edit',
  templateUrl: './account-year-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class AccountYearEditComponent
  extends EditBaseDirective<
    SaveAccountYearModel,
    AccountYearService,
    AccountYearForm
  >
  implements OnInit
{
  private readonly coreService = inject(CoreService);
  service = inject(AccountYearService);
  voucherService = inject(VoucherService);
  voucherSeriesTypeService = inject(VoucherSeriesTypeService);
  grossProfitService = inject(GrossProfitCodesService);
  storageService = inject(StorageService);

  enableDeleteYear = signal(true);

  private readonly perform = new Perform<any[]>(this.progressService);
  accountStatuses: ISmallGenericType[] = [];
  voucherSeriesTypes = new BehaviorSubject<VoucherSeriesTypeDTO[]>([]);
  periodData = new BehaviorSubject<AccountPeriodDTO[]>([]);
  voucherSeriesData = new BehaviorSubject<VoucherSeriesDTO[]>([]);
  voucherTemplateData = new BehaviorSubject<IVoucherGridDTO[]>([]);
  grossProfitData = new BehaviorSubject<GrossProfitCodeDTO[]>([]);

  latestTo: Date | null = null;
  latestYear: AccountYearDTO | null = null;
  fromValue: Date | null = null;
  keepNumberSeries = false;
  accountYearStatus: TermGroup_AccountStatus = TermGroup_AccountStatus.Closed;
  hasUnlockYearPermission = false;
  visibleUnlockButton = signal(true);

  ngOnInit() {
    super.ngOnInit();

    if (!this.form?.isNew) {
      this.startFlow(Feature.Economy_Accounting_AccountPeriods, {
        lookups: [
          this.loadVoucherSeriesTypes(),
          this.loadAccountStatuses(),
          this.loadVoucherSeriesGridData(),
          this.loadVoucherTemplateGridData(),
          this.loadGrossProfitGridData(),
        ],
        additionalModifyPermissions: [
          Feature.Economy_Accounting_AccountPeriods_UnlockYear,
        ],
      });
    } else
      this.startFlow(Feature.Economy_Accounting_AccountPeriods, {
        lookups: [this.loadAccountStatuses(), this.loadVoucherSeriesTypes()],
      });

    this.form?.keepNumberSeries.valueChanges.subscribe(x => {
      this.keepNumberSeries = x;
    });

    this.form?.addDateValidators(this.service.validator);
  }

  override onPermissionsLoaded() {
    super.onPermissionsLoaded();
    if (
      this.flowHandler.hasModifyAccess(
        Feature.Economy_Accounting_AccountPeriods_UnlockYear
      )
    ) {
      this.hasUnlockYearPermission = true;
    }
  }

  private loadAccountStatuses(): Observable<ISmallGenericType[]> {
    return this.perform.load$(
      this.coreService
        .getTermGroupContent(TermGroup.AccountYearStatus, false, false, true)
        .pipe(
          tap(x => {
            this.accountStatuses = x;
          })
        )
    );
  }

  loadVoucherSeriesTypes(): Observable<unknown> {
    return this.perform.load$(
      this.voucherSeriesTypeService.getGrid().pipe(
        tap(x => {
          this.voucherSeriesTypes.next(orderBy(x, s => s.name));
        })
      )
    );
  }

  override loadData(): Observable<void> {
    const id = this.form?.getIdControl()?.value;

    if (id) {
      return this.performLoadData.load$(
        this.service.get(this.form?.getIdControl()?.value).pipe(
          tap(accountYear => {
            if (!accountYear) {
              this.enableDeleteYear.set(false);
              return;
            }

            // Status must be less than 2 to allow deletion
            const hasValidStatus =
              accountYear.status < TermGroup_AccountYearStatus.Open;

            // Check if periods are valid for deletion
            const hasPeriods = accountYear.periods?.length > 0;
            const allPeriodsValidForDeletion =
              !hasPeriods ||
              !accountYear.periods.some(
                period => period.status > TermGroup_AccountYearStatus.NotStarted
              );

            // Can delete if status is valid and all periods are valid for deletion
            this.enableDeleteYear.set(
              hasValidStatus && allPeriodsValidForDeletion
            );

            this.periodData.next(accountYear.periods);
            this.form?.reset(accountYear);
            this.setUnlockButtonVisibility();
            this.accountYearStatus = accountYear.status;
          })
        )
      );
    } else return this.newRecord();
  }

  loadVoucherSeriesGridData(): Observable<VoucherSeriesDTO[]> {
    return this.voucherService
      .getVoucherSeriesByYear(this.form?.getIdControl()?.value, false)
      .pipe(
        tap(vouchers => {
          vouchers.forEach(x => {
            const type = this.voucherSeriesTypes.value.find(
              t => t.voucherSeriesTypeId == x.voucherSeriesTypeId
            );
            x.startNr = type?.startNr || 0;
            if (x.voucherNrLatest && type && x.voucherNrLatest < type.startNr)
              x.voucherNrLatest = undefined;
          });
          this.voucherSeriesData.next(vouchers);
        })
      );
  }

  loadVoucherTemplateGridData(): Observable<IVoucherGridDTO[]> {
    const id = this.form?.getIdControl()?.value;
    if (id)
      return this.voucherService.getVoucherTemplates(id).pipe(
        tap(template => {
          this.voucherTemplateData.next(template);
        })
      );
    else return of();
  }

  loadGrossProfitGridData(): Observable<GrossProfitCodeDTO[]> {
    const id = this.form?.getIdControl()?.value;
    if (id)
      return this.grossProfitService
        .getGrossProfitCodesByYear(this.form?.getIdControl()?.value)
        .pipe(
          tap(template => {
            this.grossProfitData.next(template);
          })
        );
    else return of();
  }

  override onFinished(): void {
    if (this.form?.isNew) {
      const latestYearDTO = this.service.latestAccountingYear;
      if (latestYearDTO?.to) {
        const latestToDate = new Date(latestYearDTO.to);
        this.fromValue = latestToDate.addDays(1);

        this.form.from.patchValue(this.fromValue);
        this.form.to.patchValue(
          DateUtil.getDateLastInMonth(
            new Date(latestToDate.setFullYear(latestToDate.getFullYear() + 1))
          )
        );
      }
    }
  }

  doDirty(isDirty: boolean) {
    if (isDirty) this.form?.markAsDirty();
  }

  reloadGrossProfit() {
    this.loadGrossProfitGridData();
    this.loadData();
    this.loadVoucherSeriesTypes();
  }

  reloadVoucherTemplate() {
    this.loadVoucherTemplateGridData();
    this.loadData();
    this.loadVoucherSeriesTypes();
  }

  performSave() {
    if (!this.form || this.form.invalid) return;

    const seriesToSave = this.voucherSeriesData.value.filter(
      s =>
        (s.isModified && !s.isDeleted) || (s.isDeleted && s.voucherSeriesId > 0)
    );

    const model = new SaveAccountYearModel();
    model.accountYear = this.form.getAllValues({
      includeDisabled: true,
    }) as AccountYearDTO;
    model.accountYear.periods = this.periodData.value;

    model.voucherSeries = seriesToSave;
    model.keepNumbers = this.keepNumberSeries;

    this.save(model);
  }

  save(model: SaveAccountYearModel) {
    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service.save(model).pipe(
        tap(response => {
          this.emitChanges(response, model);
        })
      )
    );
  }

  emitChanges = (
    backendResponse: BackendResponse,
    model: SaveAccountYearModel
  ) => {
    if (!backendResponse.success) return;

    // Release cache
    this.loadVoucherSeriesGridData().subscribe();
    this.updateFormValueAndEmitChange(
      backendResponse,
      model.accountYear.status !== this.accountYearStatus
    );
    this.setUnlockButtonVisibility();
    if (model.accountYear.status !== this.accountYearStatus) {
      this.refreshPage();
    }
  };

  setUnlockButtonVisibility() {
    this.visibleUnlockButton.set(this.form?.isLocked || false);
  }

  unlockYear() {
    if (!this.hasUnlockYearPermission) return;
    this.form?.unlockYear();
  }

  override onSaveCompleted(backendResponse: BackendResponse): void {
    if (backendResponse.success) {
      this.form?.markAsPristine();
      this.form?.markAsUntouched();
    }
  }

  refreshPage() {
    //refresh
    const url =
      '/soe/economy/accounting/yearend/?ay=' +
      this.form?.getRawValue()?.accountYearId +
      '&spa=True';
    setTimeout(() => {
      BrowserUtil.openInSameTab(window, url);
    }, 100);

    this.storageService.set('newData', this.form?.getRawValue());
  }
}
