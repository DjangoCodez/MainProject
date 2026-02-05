import {
  Component,
  computed,
  inject,
  OnDestroy,
  OnInit,
  signal,
} from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { CompanyGroupTransferService } from '../../services/company-group-transfer.service';
import { CompanyGroupTransferForm } from '../../models/company-group-transfer-form.model';
import {
  CompanyGroupTransferType,
  DistributionCodeBudgetType,
  Feature,
} from '@shared/models/generated-interfaces/Enumerations';
import { ICompanyGroupTransferModel } from '@shared/models/generated-interfaces/EconomyModels';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import {
  BehaviorSubject,
  forkJoin,
  Observable,
  of,
  Subject,
  takeUntil,
  tap,
} from 'rxjs';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { AccountYearService } from '@features/economy/account-years-and-periods/services/account-year.service';
import { AccountYearDTO } from '@features/economy/account-years-and-periods/models/account-years-and-periods.model';
import { VoucherService } from '@features/economy/voucher/services/voucher.service';
import { DateUtil } from '@shared/util/date-util';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { BudgetService } from '@features/economy/budget/services/budget.service';
import {
  CompanyGroupTransferHeadDTO,
  SaveTransferModel,
} from '../../models/company-group-transfer.model';
import { CompanyGroupAdministrationService } from '@features/economy/company-group-administration/services/company-group-administration.service';
import {
  IBudgetHeadGridDTO,
  ICompanyGroupAdministrationGridDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CrudActionTypeEnum } from '@shared/enums';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { PersistedAccountingYearService } from '@features/economy/services/accounting-year.service';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import { ResponseUtil } from '@shared/util/response-util';

@Component({
  selector: 'soe-company-group-transfer-edit',
  templateUrl: './company-group-transfer-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class CompanyGroupTransferEditComponent
  extends EditBaseDirective<
    ICompanyGroupTransferModel,
    CompanyGroupTransferService,
    CompanyGroupTransferForm
  >
  implements OnInit, OnDestroy
{
  private _destroy$ = new Subject<void>();
  service = inject(CompanyGroupTransferService);
  accountYearService = inject(AccountYearService);
  voucherService = inject(VoucherService);
  budgetService = inject(BudgetService);
  adminService = inject(CompanyGroupAdministrationService);
  ayService = inject(PersistedAccountingYearService);

  transferTypes: SmallGenericType[] = [];
  accountYearsDict: SmallGenericType[] = [];
  accountYears: AccountYearDTO[] = [];
  fromAccountPeriodDict = signal<SmallGenericType[]>([]);
  voucherSeries: SmallGenericType[] = [];
  transferRows = new BehaviorSubject<CompanyGroupTransferHeadDTO[]>([]);
  logs = signal<Array<string>>([]);
  showLogs = computed(() => {
    return this.logs().length > 0;
  });
  companyGroupAdministrations: ICompanyGroupAdministrationGridDTO[] = [];
  masterBudgets: SmallGenericType[] = [];
  childCompanies: SmallGenericType[] = [];
  filteredChildCompanies: SmallGenericType[] = [];
  filteredChildCompanyBudgets: SmallGenericType[] = [];
  childCompanyBudgets: IBudgetHeadGridDTO[] = [];
  filteredBudgets: IBudgetHeadGridDTO[] = [];
  showBudgetFields = signal(true);
  showBalanceFields = signal(true);
  selectedTransferType!: SmallGenericType;
  isConsolidation = true;

  override ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(Feature.Economy_Accounting_CompanyGroup_Transfers, {
      lookups: [
        this.loadTransfers(),
        this.loadBudgets(),
        this.loadChildCompanies(),
        this.loadCompanyGroupAdministrations(),
        this.loadAccountYearDependentData(),
      ],
      additionalModifyPermissions: [
        Feature.Economy_Accounting_Vouchers_Edit,
        Feature.Economy_Accounting_Budget_Edit,
      ],
    });

    this.form?.accountYearId.valueChanges
      .pipe(takeUntil(this._destroy$))
      .subscribe(accountYearId => {
        if (typeof accountYearId === 'number') {
          this.loadVoucherSeries();
          this.loadPeriodFromTo();
        }
      });

    this.form?.valueChanges.subscribe(() => {
      this.form?.markAsPristine();
      this.form?.markAsUntouched();
    });

    this.changeTransferType(this.form?.transferType.value);
    this.form?.childCompanyId.disable();
    this.form?.childBudgetId.disable();
  }

  override onFinished(): void {
    //load master budget
    this.filteredChildCompanies = [];
    this.childCompanies.forEach(c => {
      this.companyGroupAdministrations.find(r => {
        if (r.childActorCompanyId === c.id) {
          this.filteredChildCompanies.push(c);
        }
      });
    });
  }

  loadTransfers() {
    this.transferTypes = [];
    this.transferTypes.push({
      id: 1,
      name: this.translate.instant('common.reports.drilldown.periodamount'),
    });
    this.transferTypes.push({
      id: 2,
      name: this.translate.instant('common.reports.drilldown.budget'),
    });
    this.transferTypes.push({
      id: 3,
      name: this.translate.instant('economy.accounting.balance.balance'),
    });

    return of(this.transferTypes);
  }

  loadAccountYearDependentData() {
    return this.ayService.ensureAccountYearIsLoaded$(() => {
      return forkJoin([this.loadAccountYears(), this.loadGrid()]);
    });
  }

  loadGrid() {
    const yearId = this.ayService.selectedAccountYearId();
    if (!yearId) return of([]);

    return this.performLoadData.load$(
      this.service
        .getGrid(undefined, {
          accountYearId: yearId,
          transferType: this.form?.transferType.value,
        })
        .pipe(
          tap(result => {
            this.transferRows.next(result);
          })
        )
    );
  }

  changeTransferType(transferType: number) {
    this.showBudget(transferType);
    this.loadGrid().subscribe();
    this.isConsolidation =
      transferType === CompanyGroupTransferType.Consolidation;
  }

  showBudget(transferType: number) {
    if (transferType) {
      this.showBudgetFields.set(
        transferType === CompanyGroupTransferType.Budget
      );
      this.showBalanceFields.set(
        transferType === CompanyGroupTransferType.Balance
      );
    }
  }
  deleteCompleted(row: any) {
    this.loadGrid().subscribe();
  }

  private loadVoucherSeries(): void {
    this.performLoadData.load(
      this.voucherService
        .getVoucherSeriesByYear(this.form?.accountYearId.value, false)
        .pipe(
          tap(vouchers => {
            this.voucherSeries = vouchers.map(
              v =>
                new SmallGenericType(v.voucherSeriesId, v.voucherSeriesTypeName)
            );
            this.form?.voucherSeriesId.patchValue(this.voucherSeries[0].id);
          })
        )
    );
  }

  private loadAccountYears(): Observable<void> {
    return this.performLoadData.load$(
      this.accountYearService
        .getGrid(undefined, { getPeriods: true, excludeNew: false })
        .pipe(
          tap(res => {
            this.accountYears = res;
            this.loadFinancialYear();
          })
        )
    );
  }

  loadFinancialYear(): void {
    this.accountYearsDict = this.accountYears.map(
      x => new SmallGenericType(x.accountYearId, x.yearFromTo)
    );

    const currentFinancialYear = this.accountYears.find(r => {
      return r.accountYearId === this.ayService.selectedAccountYearId();
    });
    this.form?.accountYearId.patchValue(currentFinancialYear?.accountYearId);
  }

  loadPeriodFromTo(): void {
    this.fromAccountPeriodDict.set([]);
    const currentForm = this.accountYears.find(
      r => r.accountYearId === this.form?.accountYearId.value
    );
    this.fromAccountPeriodDict.set(
      currentForm?.periods.map(
        x =>
          new SmallGenericType(
            x.accountPeriodId,
            DateUtil.format(new Date(x.from), 'yyyy-MM')
          )
      ) ?? []
    );
    this.form?.fromAccountPeriodId.patchValue(
      this.fromAccountPeriodDict()[0].id
    );
    this.form?.toAccountPeriodId.patchValue(
      this.fromAccountPeriodDict()[this.fromAccountPeriodDict().length - 1].id
    );
  }

  private loadChildCompanies(): Observable<void> {
    return this.performLoadData.load$(
      this.adminService.getChildCompanies().pipe(
        tap(x => {
          this.childCompanies = x;
        })
      )
    );
  }

  private loadCompanyGroupAdministrations(): Observable<void> {
    return this.performLoadData.load$(
      this.adminService.getGrid().pipe(
        tap(x => {
          this.companyGroupAdministrations = x;
        })
      )
    );
  }

  private loadBudgets(): Observable<void> {
    return this.performLoadData.load$(
      this.budgetService
        .getGrid(undefined, {
          budgetType: DistributionCodeBudgetType.AccountingBudget,
          actorId: SoeConfigUtil.actorCompanyId,
        })
        .pipe(
          tap(budgets => {
            if (!SoeConfigUtil.actorCompanyId) {
              // this.filterMasterBudgets();
            } else {
              this.childCompanyBudgets = budgets;
              this.filterChildBudgets();
            }

            this.filteredBudgets = budgets.filter(
              b => b.accountYearId == this.form?.accountYearId.value
            );

            this.masterBudgets = this.filteredBudgets.map(
              b => new SmallGenericType(b.accountYearId, b.name)
            );

            // Insert empty row
            if (this.masterBudgets)
              this.masterBudgets.splice(0, 0, {
                id: 0,
                name: this.translate.instant(
                  'economy.accounting.companygroup.createbudget'
                ),
              });
          })
        )
    );
  }

  filterChildBudgets() {
    // Add budgets for current accountyear
    this.filteredChildCompanyBudgets = [];
    if (this.form?.masterBudgetId.value === 0) {
      this.filteredChildCompanyBudgets = this.childCompanyBudgets.map(
        x => new SmallGenericType(x.accountYearId, x.name)
      );
    } else {
      const selectedBudgetMaster = this.filteredBudgets.find(
        z => z.accountYearId === this.form?.masterBudgetId.value
      );

      const filteredBudgetMaster = this.childCompanyBudgets.filter(
        b =>
          b.accountYearId === selectedBudgetMaster?.accountYearId &&
          b.noOfPeriods === selectedBudgetMaster?.noOfPeriods
      );
      this.filteredChildCompanyBudgets = filteredBudgetMaster.map(
        x => new SmallGenericType(x.accountYearId, x.name)
      );
    }
  }

  changeBudget(value: number) {
    if (value || value == 0) {
      this.form?.childCompanyId.enable();
      this.form?.childBudgetId.enable();
    }
  }

  performSave() {
    this.logs.set([]);

    if (!this.form || this.form.invalid || !this.service) return;

    const model = new SaveTransferModel();
    model.accountYearId = this.form?.accountYearId.value;
    model.budgetChild = this.form?.childBudgetId.value || 0;
    model.budgetCompanyGroup = this.form?.masterBudgetId.value || 0;
    model.includeIB = false;
    model.periodFrom = this.form?.fromAccountPeriodId.value;
    model.periodTo = this.form?.toAccountPeriodId.value;
    model.transferType = this.form?.transferType.value;
    model.voucherSeriesId = this.form?.voucherSeriesId.value;

    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service.save(model),
      this.transferSuccess,
      this.transferFailed,
      {
        showToastOnComplete: false,
      }
    );
  }

  transferSuccess = (response: BackendResponse): void => {
    if (response.success) {
      this.loadBudgets().subscribe();
      this.loadGrid().subscribe();
    }
  };

  transferFailed = (response: BackendResponse): void => {
    if (!response.success) {
      const _logs =
        ((ResponseUtil.getValue2Object(response) as any).$values as string[]) ??
        [];

      this.logs.set(_logs.filter(x => String(x).length > 0) ?? []);
    }
  };

  ngOnDestroy(): void {
    this._destroy$.next();
    this._destroy$.complete();
  }
}
