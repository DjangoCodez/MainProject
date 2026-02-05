import {
  Component,
  computed,
  inject,
  OnDestroy,
  OnInit,
  signal,
} from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import {
  ISieExportConflictDTO,
  ISieExportDTO,
  ISieExportResultDTO,
} from '@shared/models/generated-interfaces/SieExportDTO';
import { SieService } from '../../services/sie.service';
import { SieExportForm } from '../../models/sie-form.model';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import {
  DistributionCodeBudgetType,
  Feature,
  SieExportType,
  TermGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import { ValidationHandler } from '@shared/handlers';
import {
  Observable,
  Subject,
  takeUntil,
  tap,
  distinctUntilChanged,
  mergeMap,
  of,
  map,
} from 'rxjs';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { AccountingService } from '@features/economy/services/accounting.service';
import { IAccountingPeriodSelection } from '@shared/components/accounting-period-selection/models/accounting-period-selection.model';
import {
  IAccountDimSmallDTO,
  IBudgetHeadGridDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { addEmptyOption } from '@shared/util/array-util';
import { CrudActionTypeEnum } from '@shared/enums';
import { DownloadUtility } from '@shared/util/download-util';
import { pairwise, startWith } from 'rxjs/operators';
import { BudgetService } from '@features/economy/budget/services/budget.service';
import { orderBy } from 'lodash';
import { SortByService } from '@shared/services/sort-by.service';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import { ResponseUtil } from '@shared/util/response-util';

@Component({
  selector: 'soe-sie-edit',
  templateUrl: './sie-edit.component.html',
  styleUrls: ['./sie-edit.component.scss'],
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class SieEditComponent
  extends EditBaseDirective<ISieExportDTO, SieService, SieExportForm>
  implements OnInit, OnDestroy
{
  private _destroy$ = new Subject<void>();
  service = inject(SieService);
  private readonly validationHandler = inject(ValidationHandler);
  private readonly coreService = inject(CoreService);
  private readonly accountingService = inject(AccountingService);
  private readonly budgetService = inject(BudgetService);
  protected readonly sortByService = inject(SortByService);
  private allBudgets: Array<IBudgetHeadGridDTO> = [];
  protected selectedExportType = signal<number>(0);
  protected showSaveButton = signal<boolean>(false);

  protected sieExportTypes = signal<Array<ISmallGenericType>>([]);
  protected voucherSeries = signal<Array<ISmallGenericType>>([]);
  protected accountDims = signal<Array<IAccountDimSmallDTO>>([]);
  protected budgets = signal<Array<ISmallGenericType>>([]);
  protected voucherSortItems = signal<Array<ISmallGenericType>>([]);
  protected sieExportTypeInfoMessage = computed(() => {
    switch (this.selectedExportType()) {
      case SieExportType.Type1:
        return this.translate.instant('economy.export.sie.type1.info');
      case SieExportType.Type2:
        return this.translate.instant('economy.export.sie.type2.info');
      case SieExportType.Type3:
        return this.translate.instant('economy.export.sie.type3.info');
      case SieExportType.Type4:
        return this.translate.instant('economy.export.sie.type4.info');
      default:
        return '';
    }
  });
  protected showVoucherSeries = computed(
    () =>
      !(
        this.selectedExportType() === 0 ||
        this.selectedExportType() == SieExportType.Type1
      )
  );
  protected isSieType3Or4 = computed(
    () =>
      this.selectedExportType() === SieExportType.Type3 ||
      this.selectedExportType() === SieExportType.Type4
  );
  protected showBudget = computed(
    () =>
      this.selectedExportType() !== 0 &&
      this.selectedExportType() !== SieExportType.Type1
  );
  protected showVoucherSort = computed(
    () => this.selectedExportType() === SieExportType.Type4
  );

  protected conflictRows = signal<Array<ISieExportConflictDTO>>([]);
  protected showConflictGrid = computed(() => {
    return this.conflictRows().length > 0;
  });

  override ngOnInit(): void {
    super.ngOnInit();

    this.form = new SieExportForm({
      validationHandler: this.validationHandler,
      element: undefined,
    });

    this.startFlow(Feature.Economy_Export_Sie_Type1, {
      skipDefaultToolbar: true,
      lookups: [
        this.loadSieTypes(),
        this.loadBudgets(),
        this.loadVoucherSortItems(),
      ],
    });

    this.form?.exportType.valueChanges
      .pipe(startWith(0), pairwise(), takeUntil(this._destroy$))
      .subscribe(([prev, next]: [number, number]) => {
        if (typeof prev === 'number' && !isNaN(next)) {
          this.exportTypeChanged(Number(next), prev);
        }
      });

    this.form?.accountingYearId.valueChanges
      .pipe(distinctUntilChanged(), takeUntil(this._destroy$))
      .subscribe(accountId => {
        this.loadVoucherSeries(accountId);
        this.filterBudgets(accountId);
      });
    this.addAccountDim();
    this.addVoucherSerie();
  }

  private exportTypeChanged(
    selectedExportType: number,
    previousExportType: number
  ): void {
    this.form?.exportObject.setValue(
      selectedExportType === SieExportType.Type3 ||
        selectedExportType === SieExportType.Type4
    );
    this.form?.resetAccountYears();

    this.selectedExportType.set(selectedExportType);
    ///Stop accountDim load on every time exportType changed
    if (
      (selectedExportType === SieExportType.Type3 ||
        selectedExportType === SieExportType.Type4) &&
      !(
        previousExportType === SieExportType.Type3 ||
        previousExportType === SieExportType.Type4
      )
    )
      this.loadAccounts(false);
    else if (
      (selectedExportType === SieExportType.Type1 ||
        selectedExportType === SieExportType.Type2) &&
      !(
        previousExportType === SieExportType.Type1 ||
        previousExportType === SieExportType.Type2
      )
    ) {
      this.loadAccounts(true);
    }

    this.filterBudgets(this.form?.accountingYearId.value);
  }

  private loadSieTypes(): Observable<void> {
    return this.performLoadData.load$(
      this.coreService
        .getTermGroupContent(TermGroup.SieExportType, false, true)
        .pipe(
          tap(exportTypes =>
            this.sieExportTypes.set(orderBy(exportTypes, ['id']))
          )
        )
    );
  }

  private loadBudgets(): Observable<void> {
    return this.performLoadData.load$(
      this.budgetService
        .getGrid(undefined, {
          budgetType: DistributionCodeBudgetType.AccountingBudget,
          actorId: 0,
        })
        .pipe(
          tap(
            budgets =>
              (this.allBudgets = <IBudgetHeadGridDTO[]>(<unknown>budgets))
          )
        )
    );
  }

  private loadVoucherSortItems(): Observable<void> {
    return this.performLoadData.load$(
      this.coreService
        .getTermGroupContent(TermGroup.SieExportVoucherSort, true, false)
        .pipe(tap(sortItems => this.voucherSortItems.set(sortItems)))
    );
  }

  private loadVoucherSeries(accountingYearId: number): void {
    if (accountingYearId && accountingYearId > 0) {
      this.performLoadData.load(
        this.accountingService
          .getVoucherSeriesDictByYear(accountingYearId, false, false)
          .pipe(
            tap(vs => {
              this.voucherSeries.set(vs ?? []);
            })
          ),
        {
          showDialog: false,
        }
      );
    } else this.voucherSeries.set([]);
  }

  private loadAccounts(onlyStandard: boolean): void {
    this.performLoadData.load(
      this.coreService
        .getAccountDimsSmall(
          true,
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
          mergeMap(dims => {
            const dimsStandard = dims ?? [];
            if (onlyStandard) return of(dimsStandard);

            return this.coreService
              .getAccountDimsSmall(
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                false
              )
              .pipe(
                map(dimInternals => {
                  return [...dimsStandard, ...dimInternals].sort(
                    dim => dim.accountDimNr
                  );
                })
              );
          }),
          tap(dims => {
            this.accountDims.set(dims);
          })
        )
    );
  }

  private filterBudgets(accountYearId: number): void {
    const filtered = this.allBudgets
      .filter(x => x.accountYearId === accountYearId)
      .map(
        x =>
          <ISmallGenericType>{
            id: x.budgetHeadId,
            name: x.name,
          }
      );

    addEmptyOption(filtered);

    this.form?.budgetHeadId.setValue(0);
    this.budgets.set(filtered);
  }

  protected accountingPeriodSelectionChanged(
    value: IAccountingPeriodSelection
  ): void {
    this.form?.patchValue({
      accountingYearId: value.accountingYearFrom,
      dateFrom: value.dateFrom,
      dateTo: value.dateTo,
    });
  }

  protected accountingPeriodSelectionLoaded(): void {
    this.showSaveButton.set(true);
  }

  protected addAccountDim(): void {
    this.form?.addAccountDim();
  }

  protected removeAccountDim(idx: number): void {
    this.form?.removeAccountDim(idx);
  }

  protected accountDimChanged(accountDimId: number, idx: number): void {
    const accounts =
      this.accountDims().find(d => d.accountDimId === accountDimId)?.accounts ??
      [];
    addEmptyOption(accounts);
    this.form!.accountSelection.at(idx).accounts = accounts;
  }

  protected addVoucherSerie(): void {
    this.form?.addVoucherSerie();
  }

  protected removeVoucherSerie(idx: number): void {
    this.form?.removeVoucherSerie(idx);
  }

  protected triggerExport(): void {
    this.conflictRows.set([]);

    const exportModel = <ISieExportDTO>this.form?.value;
    exportModel.accountSelection =
      exportModel.accountSelection.filter(
        x =>
          x.accountDimId > 0 &&
          (x.accountNrFrom.length > 0 || x.accountNrTo.length > 0)
      ) ?? [];
    exportModel.voucherSelection =
      exportModel.voucherSelection.filter(v => v.voucherSeriesId > 0) ?? [];

    this.performAction.crud(
      CrudActionTypeEnum.Work,
      this.service.export(exportModel),
      this.exportSuccess,
      this.exportFailed,
      {
        showDialogOnComplete: false,
        showToastOnComplete: false,
      }
    );
  }

  private exportSuccess = (result: BackendResponse): void => {
    if (result.success) {
      //If there are conflicts
      if (ResponseUtil.getBooleanValue(result)) {
        this.messageboxService.error(
          'core.error',
          ResponseUtil.getErrorMessage(result) ?? ''
        );

        this.showConflicts(result);
      } else {
        //export success
        this.messageboxService.success(
          'core.worked',
          ResponseUtil.getMessageValue(result) ?? ''
        );

        const file = <ISieExportResultDTO>ResponseUtil.getValueObject(result);
        DownloadUtility.downloadFile(
          file.fileName,
          file.fileType,
          file.content
        );
      }
    }
  };

  private exportFailed = (result: BackendResponse): void => {
    if (!result.success) {
      this.showConflicts(result);
    }
  };

  //Handle conflicts
  private showConflicts(result: BackendResponse): void {
    //show conflicts
    this.conflictRows.set(
      (JSON.parse(JSON.stringify(ResponseUtil.getValue2Object(result) ?? {}))
        .$values ?? []) as ISieExportConflictDTO[]
    );
  }

  ngOnDestroy(): void {
    this._destroy$.next();
    this._destroy$.complete();
  }
}
