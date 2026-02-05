import { Component, OnInit, inject, signal } from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { AccountDistributionService } from '../../services/account-distribution.service';
import { AccountDistributionForm } from '../../models/account-distribution-form.model';
import {
  Feature,
  SoeAccountDistributionType,
  SoeEntityState,
  TermGroup,
  TermGroup_AccountDistributionCalculationType,
  TermGroup_AccountDistributionPeriodType,
  TermGroup_AccountDistributionTriggerType,
  WildCard,
} from '@shared/models/generated-interfaces/Enumerations';
import { BehaviorSubject, concatMap, Observable, of, tap } from 'rxjs';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { VoucherSeriesTypeService } from '../../../services/voucher-series-type.service';
import {
  AccountDistributionHeadDTO,
  AccountDistributionRowDTO,
} from '../../models/account-distribution.model';
import { CrudActionTypeEnum } from '@shared/enums';
import { EconomyService } from '../../../services/economy.service';
import {
  IAccountDimDTO,
  IAccountDimSmallDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { TraceRowPageName } from '@shared/components/trace-rows/models/trace-rows.model';
import { AccountDistributionEntryDTO } from '@features/economy/inventory-writeoffs/models/inventory-writeoffs.model';
import { InventoryWriteoffsService } from '@features/economy/inventory-writeoffs/services/inventory-writeoffs.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { AccountDistributionUrlParamsService } from '../../services/account-distribution-params.service';
import { DateUtil } from '@shared/util/date-util';
import { AccountDistributionAutoService } from '@features/economy/account-distribution-auto/services/account-distribution-auto.service';
import { ProgressOptions } from '@shared/services/progress/progress-options.class';

@Component({
  selector: 'soe-account-distribution-edit',
  templateUrl: './account-distribution-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class AccountDistributionEditComponent
  extends EditBaseDirective<
    AccountDistributionHeadDTO,
    AccountDistributionService,
    AccountDistributionForm
  >
  implements OnInit
{
  //#region Properties
  service = inject(AccountDistributionService);
  writeoffsService = inject(InventoryWriteoffsService);
  accountDistributionAutoService = inject(AccountDistributionAutoService);

  coreService = inject(CoreService);
  voucherSeriesService = inject(VoucherSeriesTypeService);
  public economyService = inject(EconomyService);
  urlService = inject(AccountDistributionUrlParamsService);

  distributionRow = new BehaviorSubject<AccountDistributionRowDTO[]>([]);
  distributionActiveRows = new BehaviorSubject<AccountDistributionRowDTO[]>([]);
  triggerType: ISmallGenericType[] = [];
  calculationType: ISmallGenericType[] = [];
  periodTypes: ISmallGenericType[] = [];
  filteredPeriodTypes: ISmallGenericType[] = [];
  voucherSeries: ISmallGenericType[] = [];

  registrationAsTriggerType = false;
  amountAsPeriodType = false;
  isPeriodAccountDistribution = false;
  isAutomaticAccountDistribution = false;

  entryTotalCount!: number;
  entryTransferredCount!: number;
  entryRemainingCount!: number;
  entryLatestTransferDate!: Date;
  entryTotalAmount: number = 0;
  entryPeriodAmount: number = 0;
  entryTransferredAmount: number = 0;
  entryRemainingAmount: number = 0;

  entriesIsStarted = signal(false);

  //Account Dim
  accountDim1Name = '';
  accountDim2Name = '';
  accountDim3Name = '';
  accountDim4Name = '';
  accountDim5Name = '';
  accountDim6Name = '';

  amountOperatorWildCardOptions: SmallGenericType[] = [];

  pageName = TraceRowPageName.AccountDistribution;

  //#endregion
  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      this.urlService.isPeriod()
        ? Feature.Economy_Preferences_VoucherSettings_AccountDistributionPeriod
        : Feature.Economy_Preferences_VoucherSettings_AccountDistributionAuto,
      {
        lookups: [
          this.loadCalculationTypes(),
          this.loadTriggerTypes(),
          this.loadPeriodTypes(),
          this.loadVoucherSeriesTypes(),
          this.loadCompanySettings(),
          this.loadAccountDims(),
          this.loadAccounts(),
          this.loadOperators(),
        ],
      }
    );

    if (
      this.urlService.isPeriod() ||
      this.urlService.typeId() === SoeAccountDistributionType.Period
    ) {
      this.isPeriodAccountDistribution = true;
      this.form?.type.patchValue(SoeAccountDistributionType.Period);
    } else if (
      this.urlService.isAuto() ||
      this.urlService.typeId() === SoeAccountDistributionType.Auto
    ) {
      this.form?.type.patchValue(SoeAccountDistributionType.Auto);
      this.isAutomaticAccountDistribution = true;
      this.service = this.accountDistributionAutoService;
    }
    this.triggerTypeChange(this.form?.triggerType.value);

    this.additionalDeleteProps = { skipUpdateGrid: false };
  }

  override copy(): void {
    const additionalProps = {
      distributionRows: this.form?.getDistributionRowsForCopy() ?? [],
    };
    super.copy(additionalProps);
  }

  override newRecord(): Observable<void> {
    let clearValues = () => {};

    if (this.form?.isNew || this.form?.isCopy) {
      if (this.isAutomaticAccountDistribution) {
        this.form.patchValue({
          type: SoeAccountDistributionType.Auto,
          sort: 0,
          calculationType: 1,
          amount: 0,
          amountOperator: 2,
          triggerType: 1,
        });
      }
      if (this.isPeriodAccountDistribution) {
        this.form.patchValue({
          type: SoeAccountDistributionType.Period,
          sort: 0,
          calculationType: 1,
          keepRow: false,
        });
        this.calculationTypeChange(this.form?.calculationType.value);
      }
    }

    if (this.form?.isCopy) {
      clearValues = () => {
        this.form?.accountDistributionHeadId.patchValue(0);
        const rows = this.form?.additionalPropsOnCopy.distributionRows;
        this.form?.resetDistributionAccountingRow(rows);
        this.setDistributionRows(<AccountDistributionRowDTO[]>rows ?? []);
      };
    }

    return of(clearValues());
  }

  loadTriggerTypes(): Observable<ISmallGenericType[]> {
    return this.performLoadData.load$(
      this.coreService
        .getTermGroupContent(
          TermGroup.AccountDistributionTriggerType,
          false,
          false
        )
        .pipe(
          tap(data => {
            this.triggerType = data;
          })
        )
    );
  }

  loadCalculationTypes(): Observable<ISmallGenericType[]> {
    return this.performLoadData.load$(
      this.coreService
        .getTermGroupContent(
          TermGroup.AccountDistributionCalculationType,
          false,
          false
        )
        .pipe(
          tap(data => {
            this.calculationType = data;
          })
        )
    );
  }

  loadPeriodTypes(): Observable<ISmallGenericType[]> {
    return this.performLoadData.load$(
      this.coreService
        .getTermGroupContent(
          TermGroup.AccountDistributionPeriodType,
          false,
          false
        )
        .pipe(
          tap(data => {
            this.periodTypes = data;
          })
        )
    );
  }

  loadVoucherSeriesTypes(): Observable<ISmallGenericType[]> {
    return this.performLoadData.load$(
      this.voucherSeriesService.getVoucherSeriesTypesByCompany().pipe(
        tap(data => {
          this.voucherSeries = data;
        })
      )
    );
  }

  override performSave(): void {
    if (!this.form || this.form.invalid || !this.service) return;
    const distributionRow: AccountDistributionRowDTO[] = [];
    this.distributionActiveRows.getValue().forEach(f => {
      f.dim1Id = f.dim1Id === -1 ? 0 : f.dim1Id;
      f.dim2Id = f.dim2Id === -1 ? 0 : f.dim2Id;
      f.dim3Id = f.dim3Id === -1 ? 0 : f.dim3Id;
      f.dim4Id = f.dim4Id === -1 ? 0 : f.dim4Id;
      f.dim5Id = f.dim5Id === -1 ? 0 : f.dim5Id;
      f.dim6Id = f.dim6Id === -1 ? 0 : f.dim6Id;
      distributionRow.push(f);
    });

    const accountDistributionHead: AccountDistributionHeadDTO = {
      ...this.form.getRawValue(),
      periodValue: this.form.getRawValue()?.numberOfTimes || 1,
    };

    const model = {
      accountDistributionHead: accountDistributionHead,
      accountDistributionRows: distributionRow,
    };

    const message = this.translate.instant(
      'economy.accounting.accountdistribution.savingprogress'
    );
    const options: ProgressOptions = {
      message: message,
    };

    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service.save(model).pipe(tap(this.updateFormValueAndEmitChange)),
      undefined,
      undefined,
      options
    );
  }

  private loadAccounts(): Observable<IAccountDimDTO[]> {
    return this.economyService
      .getAccountDims(true, false, false, false, false, false, false, false)
      .pipe(
        tap(acctDims => {
          this.accountDim1Name = acctDims[0].name;
          this.form?.dim1Id.patchValue(acctDims[0].accountDimId);
        })
      );
  }

  private loadAccountDims(): Observable<IAccountDimSmallDTO[]> {
    return this.economyService
      .getAccountDimsSmall(
        false,
        true,
        false,
        false,
        false,
        false,
        false,
        false
      )
      .pipe(
        tap(dims => {
          let pos = 2;

          for (const dim of dims) {
            if (pos == 2) {
              this.accountDim2Name = dim.name;
              this.form?.dim2Id.patchValue(dim.accountDimId);
            }
            if (pos == 3) {
              this.accountDim3Name = dim.name;
              this.form?.dim3Id.patchValue(dim.accountDimId);
            }
            if (pos == 4) {
              this.accountDim4Name = dim.name;
              this.form?.dim4Id.patchValue(dim.accountDimId);
            }
            if (pos == 5) {
              this.accountDim5Name = dim.name;
              this.form?.dim5Id.patchValue(dim.accountDimId);
            }
            if (pos == 6) {
              this.accountDim6Name = dim.name;
              this.form?.dim6Id.patchValue(dim.accountDimId);
            }
            pos++;
          }
        })
      );
  }

  private loadExistingEntries(): Observable<AccountDistributionEntryDTO[]> {
    return this.performLoadData.load$(
      this.writeoffsService
        .getAccountDistributionEntriesForHead(this.form?.getIdControl()?.value)
        .pipe(
          tap(value => {
            this.entriesIsStarted.set(
              value.some(
                e =>
                  e.voucherHeadId &&
                  e.voucherHeadId != 0 &&
                  e.state !== SoeEntityState.Deleted
              )
            );
            this.entryTransferredCount = 0;
            this.entryTotalCount = value.length;
            this.entryTotalAmount = 0;
            this.entryPeriodAmount = 0;
            this.entryTransferredAmount = 0;
            this.calculateInfo(value);
          })
        )
    );
  }

  calculateInfo(value: AccountDistributionEntryDTO[]) {
    value.forEach(entry => {
      let rowAmount: number = 0;
      if (entry.accountDistributionEntryRowDTO[0]?.debitAmount > 0)
        rowAmount = entry.accountDistributionEntryRowDTO[0]?.debitAmount;
      else rowAmount = entry.accountDistributionEntryRowDTO[0]?.creditAmount;

      this.entryTotalAmount += rowAmount;

      if (entry.voucherHeadId) {
        this.entryTransferredCount++;
        this.entryTransferredAmount += rowAmount;
        this.entryLatestTransferDate = entry.date;
      }

      if (this.entryPeriodAmount == 0) this.entryPeriodAmount = rowAmount;

      this.entryRemainingAmount =
        this.entryTotalAmount - this.entryTransferredAmount;
      this.entryRemainingCount =
        this.entryTotalCount - this.entryTransferredCount;
    });
  }

  private loadOperators(): Observable<SmallGenericType[]> {
    this.amountOperatorWildCardOptions = [
      {
        id: WildCard.LessThan,
        name: '<',
      },
      {
        id: WildCard.LessThanOrEquals,
        name: '<=',
      },
      {
        id: WildCard.Equals,
        name: '=',
      },
      {
        id: WildCard.GreaterThanOrEquals,
        name: '>=',
      },
      {
        id: WildCard.GreaterThan,
        name: '>',
      },
      {
        id: WildCard.NotEquals,
        name: '<>',
      },
    ];

    return of(this.amountOperatorWildCardOptions);
  }

  override loadData(): Observable<void> {
    return this.performLoadData.load$(
      this.service.get(this.form?.getIdControl()?.value).pipe(
        tap(value => {
          if (this.isPeriodAccountDistribution) value.keepRow = false;
          this.triggerTypeChange(value.triggerType);
          this.periodTypeChange(value.periodType);
          this.form?.patchValue(value);
          this.form?.customPatchValue(value);
          this.setDistributionRows(value.rows);
          this.calculationTypeChange(value.calculationType);
        }),
        concatMap(() => this.loadExistingEntries()),
        concatMap(() => this.loadAccounts()),
        concatMap(() => this.loadAccountDims())
      )
    );
  }

  private setDistributionRows(rows: AccountDistributionRowDTO[]): void {
    rows = rows.sort((a, b) => (a.rowNbr > b.rowNbr ? 1 : -1));
    this.distributionRow.next(rows);
  }

  triggerTypeChange(value: TermGroup_AccountDistributionTriggerType) {
    if (value == TermGroup_AccountDistributionTriggerType.Registration) {
      this.registrationAsTriggerType = true;
    } else this.registrationAsTriggerType = false;
  }

  triggerDateChange() {
    if (
      this.form?.triggerType.value !==
      TermGroup_AccountDistributionTriggerType.Registration
    )
      return;

    const originalStartDate = new Date(this.form?.startDate.getRawValue());
    const originalEndDate = new Date(this.form?.endDate.getRawValue());
    let dayOfPeriod = this.form?.dayNumber.getRawValue();
    let endDate = new Date(originalEndDate);

    if (!originalStartDate || !originalEndDate) return;
    if (originalStartDate > originalEndDate) return;

    if (dayOfPeriod <= 0) {
      dayOfPeriod = 31;
    }

    const lastDayOfEndMonth = DateUtil.getDateLastInMonth(endDate).getDate();

    endDate = new Date(
      endDate.getFullYear(),
      endDate.getMonth(),
      lastDayOfEndMonth < dayOfPeriod ? lastDayOfEndMonth : dayOfPeriod
    );

    let months = DateUtil.diffMonths(endDate, originalStartDate) + 1;

    if (originalEndDate < endDate) {
      months -= 1;
    }

    this.form?.numberOfTimes.patchValue(months);
  }

  calculationTypeChange(
    calculationType: TermGroup_AccountDistributionCalculationType
  ) {
    if (!this.isPeriodAccountDistribution) return;

    if (
      calculationType === TermGroup_AccountDistributionCalculationType.Amount
    ) {
      this.filteredPeriodTypes = this.periodTypes.filter(
        pt => pt.id === TermGroup_AccountDistributionPeriodType.Amount
      );
    } else {
      this.filteredPeriodTypes = this.periodTypes.filter(
        pt => pt.id !== TermGroup_AccountDistributionPeriodType.Amount
      );
    }

    const periodType = this.filteredPeriodTypes.find(
      pt => pt.id === this.form?.periodType.value
    );
    this.form?.periodType.patchValue(
      periodType?.id || this.filteredPeriodTypes[0]?.id || null
    );
  }

  periodTypeChange(periodType: TermGroup_AccountDistributionPeriodType) {
    if (periodType == TermGroup_AccountDistributionPeriodType.Amount)
      this.amountAsPeriodType = true;
    else this.amountAsPeriodType = false;
  }
}
