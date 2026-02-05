import {
  Component,
  OnInit,
  inject,
  signal,
  OnDestroy,
  computed,
  effect,
  DestroyRef,
} from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { ProjectBudgetService } from '../../services/project-budget.service';
import { ProjectBudgetForm } from '../../models/projet-budget-form.model';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import {
  BudgetHeadStatus,
  DistributionCodeBudgetType,
  Feature,
  ProjectCentralBudgetRowType,
  TermGroup,
  TermGroup_ProjectBudgetPeriodType,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  BehaviorSubject,
  Observable,
  Subscription,
  finalize,
  of,
  tap,
} from 'rxjs';
import { DecimalPipe } from '@angular/common';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { MatDialogRef } from '@angular/material/dialog';
import { ProgressOptions } from '@shared/services/progress';
import { CrudActionTypeEnum } from '@shared/enums';
import { TermCollection } from '@shared/localization/term-types';

import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component';
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component';
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component';
import { MessageboxComponent } from '@ui/dialog/messagebox/messagebox.component';
import { MessageboxData } from '@ui/dialog/models/messagebox';
import { TextboxComponent } from '@ui/forms/textbox/textbox.component';
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { ReactiveFormsModule } from '@angular/forms';
import { BillingModule } from '../../../../billing/billing.module';
import {
  ProjectBudgetEditSimpleGridComponent,
  ProjectBudgetRowActions,
} from './project-budget-edit-simple-grid/project-budget-edit-simple-grid.component';
import {
  BudgetHeadProjectDTO,
  BudgetRowProjectChangeLogDTO,
  BudgetRowProjectDTO,
} from '../../models/project-budget.model';
import { SumType } from '@features/billing/project/models/project-budgets-model';
import { BrowserUtil } from '@shared/util/browser-util';
import { ChangeLogModal } from './change-log-modal/change-log-modal.component';
import { ChangeLogDialogData } from './change-log-modal/change-log-modal.model';
import { Perform } from '@shared/util/perform.class';
import { MessagingService } from '@shared/services/messaging.service';
import {
  PROJECT_REFRESH_BUDGET,
  ProjectRefreshBudgetPayload,
} from '@features/billing/project/models/project-event';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import { ResponseUtil } from '@shared/util/response-util';

@Component({
  selector: 'soe-project-budget-edit',
  templateUrl: './project-budget-edit.component.html',
  styleUrl: './project-budget-edit.component.scss',
  providers: [FlowHandlerService, ToolbarService],
  imports: [
    SharedModule,
    ToolbarComponent,
    ExpansionPanelComponent,
    ReactiveFormsModule,
    ButtonComponent,
    TextboxComponent,
    EditFooterComponent,
    BillingModule,
    ProjectBudgetEditSimpleGridComponent,
    DatepickerComponent,
    DecimalPipe,
  ],
})
export class ProjectBudgetEditComponent
  extends EditBaseDirective<
    BudgetHeadProjectDTO,
    ProjectBudgetService,
    ProjectBudgetForm
  >
  implements OnInit, OnDestroy
{
  service = inject(ProjectBudgetService);
  private readonly coreService = inject(CoreService);
  private readonly dialogService = inject(DialogService);
  private readonly messagingService = inject(MessagingService);
  private readonly destroyed$ = inject(DestroyRef);

  perform = new Perform<any>(this.progressService);

  budgetTypes: SmallGenericType[] = [];
  periodTypes: SmallGenericType[] = [];
  budgetHeadId: number = 0;
  budgetHeadStatus: number = 0;
  budgetHeadType: number = 0;
  timerToken: number = 0;
  currentGuid!: string;
  progress!: MatDialogRef<MessageboxComponent<MessageboxData>>;
  progressText!: string;
  isClearGrid: boolean = false;
  periodRangeSubscription$!: Subscription;

  // Head
  budgetHead: BudgetHeadProjectDTO = new BudgetHeadProjectDTO();
  budgetRowAction = signal<ProjectBudgetRowActions>(null);

  //SubGrid
  budgetRowData = new BehaviorSubject<BudgetRowProjectDTO[]>([]);

  // Sums
  totalHoursIB: number = 0;
  incomeTotal: number = 0;
  costPersonell: number = 0;
  totalHours: number = 0;
  costMaterial: number = 0;
  costExpense: number = 0;
  costOverhead: number = 0;
  costTotal: number = 0;
  result: number = 0;
  resultRatio: string = '100 %';

  incomeTotalResult: number = 0;
  incomeTotalDiffResult: number = 0;
  costPersonellResult: number = 0;
  costPersonellDiffResult: number = 0;
  totalHoursResult: number = 0;
  costMaterialResult: number = 0;
  costMaterialDiffResult: number = 0;
  costExpenseResult: number = 0;
  costExpenseDiffResult: number = 0;
  costOverheadResult: number = 0;
  costOverheadDiffResult: number = 0;
  costTotalResult: number = 0;
  costTotalDiffResult: number = 0;
  resultResult: number = 0;
  resultDiffResult: number = 0;
  resultRatioResult: string = '100 %';

  incomeTotalComparableBudget: number = 0;
  incomeTotalDiffComparableBudget: number = 0;
  costPersonellComparableBudget: number = 0;
  costPersonellDiffComparableBudget: number = 0;
  totalHoursComparableBudget: number = 0;
  costMaterialComparableBudget: number = 0;
  costMaterialDiffComparableBudget: number = 0;
  costExpenseComparableBudget: number = 0;
  costExpenseDiffComparableBudget: number = 0;
  costOverheadComparableBudget: number = 0;
  costOverheadDiffComparableBudget: number = 0;
  costTotalComparableBudget: number = 0;
  costTotalDiffComparableBudget: number = 0;
  resultComparableBudget: number = 0;
  resultDiffComparableBudget: number = 0;
  resultRatioComparableBudget: string = '100 %';

  // Signals
  projectBudgetId = signal<number | undefined>(undefined);
  projectBudgetIdNotSet = computed(() => !this.projectBudgetId());
  budgetTypeText = signal('');

  toolbarCreateForecastDisabled = signal(true);
  toolbarUpdateForecastHidden = signal(true);
  toolbarUpdateForecastDisabled = signal(true);
  toolbarChangePeriodDisabled = signal(true);

  isProjectBudgetLocked = signal(false);
  isProjectBudgetForecast = signal(false);
  isProjectBudgetIB = signal(false);
  compBudgetName = signal('');
  resultUpdatedText = signal('');

  isLoadingBudgetHead = signal(false);

  private toolbarUpdateForecastDisabledEffect = effect(() => {
    this.toolbarUpdateForecastDisabled.set(this.isFormDirty());
  });

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(Feature.Billing_Project_Edit_Budget, {
      lookups: [this.loadBudgetTypes()],
    });

    this.form
      ?.getIdControl()
      ?.valueChanges.pipe(takeUntilDestroyed(this.destroyed$))
      .subscribe(this.projectBudgetId.set);
  }

  ngOnDestroy(): void {
    this.periodRangeSubscription$?.unsubscribe();
    this.toolbarUpdateForecastDisabledEffect.destroy();
  }

  override loadTerms(): Observable<TermCollection> {
    return super.loadTerms([
      'economy.accounting.budget.use',
      'economy.accounting.budget.default',
      'economy.accounting.budget.getting',
      'billing.projects.list.budgetincometotal',
      'billing.projects.list.budgetcostmaterial',
      'billing.projects.list.budgetcostpersonal',
      'billing.projects.list.budgetcost',
      'billing.projects.list.budgetoverhead',
      'billing.projects.list.budgetcosttotal',
      'billing.projects.budget.contributionmargin',
      'common.amount',
      'core.time.hours',
      'common.reports.drilldown.periodbudgetdiff',
      'billing.projects.budget.forecast',
      'billing.project.central.budget',
      'billing.project.central.outcome',
      'billing.projects.budget.difforecast',
      'common.customer.customer.marginalincomeratio',
      'billing.projects.budget.budgetopeningbalance',
    ]);
  }

  override loadData(): Observable<void> {
    this.isLoadingBudgetHead.set(true);

    return this.performLoadData.load$(
      this.service.get(this.form?.getIdControl()?.value).pipe(
        tap(value => {
          this.budgetHead = value;
          this.form?.reset(this.budgetHead);

          const rowDtos = this.budgetHead.rows.map(r => {
            const row = { ...new BudgetRowProjectDTO(), ...r };
            return row as BudgetRowProjectDTO;
          });

          this.rowsChanged(rowDtos, false);
          this.budgetRowAction.set({
            type: 'RESET_ROWS',
            rows: rowDtos,
            emit: false,
          });
          this.budgetHeadId = this.budgetHead.budgetHeadId;
          this.budgetHeadStatus = +this.budgetHead.status;
          this.budgetHeadType = +this.budgetHead.type;

          const type = this.budgetTypes.find(t => t.id === this.budgetHeadType);
          this.budgetTypeText.set(type ? type.name : '');

          this.setProjectBudgetSignals();
          this.summarize(rowDtos);

          setTimeout(() => {
            this.setButtonCreateForecastVisible();
          }, 50);
        }),
        finalize(() => this.isLoadingBudgetHead.set(false))
      )
    );
  }

  private loadBudgetTypes() {
    return this.coreService
      .getTermGroupContent(TermGroup.BudgetType, false, false)
      .pipe(
        tap(types => {
          types.forEach((type, i) => {
            if (
              type.id === +DistributionCodeBudgetType.ProjectBudgetExtended ||
              type.id > 6
            )
              this.budgetTypes.push({ id: type.id, name: type.name });
          });
        })
      );
  }

  override onFinished(): void {
    this.formLockValidate(this.form?.value);
  }

  override createEditToolbar(): void {
    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('createforecast', {
          iconName: signal('chart-mixed-up-circle-dollar'),
          caption: signal('economy.accounting.budget.createforecast'),
          tooltip: signal('economy.accounting.budget.createforecast'),
          onAction: this.buttonCreateForecast.bind(this),
          disabled: this.toolbarCreateForecastDisabled,
          hidden: this.projectBudgetIdNotSet,
        }),
        this.toolbarService.createToolbarButton('updateresult', {
          iconName: signal('chart-pie-simple-circle-dollar'),
          caption: signal('billing.projects.budget.updateresult'),
          tooltip: signal('billing.projects.budget.updateresult'),
          onAction: this.buttonUpdateForecastResult.bind(this),
          hidden: computed(
            () =>
              this.toolbarUpdateForecastHidden() && this.projectBudgetIdNotSet()
          ),
          disabled: this.toolbarUpdateForecastDisabled,
        }),
        this.toolbarService.createToolbarButton(
          'billing.projects.list.openprojectcentral',
          {
            iconName: signal('calculator-alt'),
            caption: signal('billing.project.central.projectcentral'),
            tooltip: signal('billing.projects.list.openprojectcentral'),
            onAction: this.openProjectCentral.bind(this),
            hidden: this.projectBudgetIdNotSet,
          }
        ),
      ],
    });
  }

  private setButtonCreateForecastVisible() {
    this.toolbarCreateForecastDisabled.set(
      !(
        this.budgetHeadType ===
          +DistributionCodeBudgetType.ProjectBudgetExtended ||
        this.budgetHeadType ===
          +DistributionCodeBudgetType.ProjectBudgetForecast
      )
    );
    this.toolbarUpdateForecastHidden.set(
      !(
        this.budgetHeadType ===
        +DistributionCodeBudgetType.ProjectBudgetForecast
      )
    );
    this.toolbarUpdateForecastDisabled.set(
      !(
        this.budgetHeadType ===
          +DistributionCodeBudgetType.ProjectBudgetForecast &&
        !this.isFormDirty()
      )
    );
  }

  private openProjectCentral() {
    const url = `/soe/billing/project/central/?project=${this.form?.projectId?.value}`;

    BrowserUtil.openInNewTab(window, url);
  }

  override newRecord(): Observable<void> {
    if (this.form?.isCopy) {
      this.budgetRowData.next(this.form.rows.value);
    } else if (this.form?.data) {
      if (this.form.data.createForecast) {
        this.isProjectBudgetForecast.set(true);
        return this.performCreateForecast(this.form.data.forecastFromId ?? 0);
      } else {
        const { startDate, stopDate, projectId, number, name, budgetTypeId } =
          this.form?.data ?? {};

        this.form.reset({
          budgetHeadId: 0,
          type:
            budgetTypeId ?? +DistributionCodeBudgetType.ProjectBudgetExtended, // Abbe, change the name in the form if necessary.
          periodType: +TermGroup_ProjectBudgetPeriodType.SinglePeriod,
          projectId: projectId,
          projectNr: number,
          projectName: name,
          projectFromDate: startDate,
          projectToDate: stopDate,
          fromDate: startDate,
          toDate: stopDate,
        });

        this.budgetRowAction.set({
          type: 'RESET_ROWS',
          rows: [],
        });

        this.setProjectBudgetSignals();
      }
    }

    this.form?.markAsDirty();
    return of(undefined);
  }

  private formLockValidate(value: BudgetHeadProjectDTO) {
    if (value?.status == 2) {
      this.form?.disable();
      this.form?.disable();
    } else {
      this.form?.enable();
      this.form?.enable();
    }

    this.form?.lockUnlockFormControls(this.form?.lockStatus.value == 2);
  }

  clearGridRows() {
    this.budgetRowData.next([]);
    this.form?.rows.clear();
    this.form?.markAsDirty();
    this.form?.markAsTouched();
  }

  lock() {
    if (this.budgetHeadStatus !== BudgetHeadStatus.Active) {
      this.budgetHeadStatus = 2;
      this.form?.lockUnlockFormControls(true);
      this.form?.patchValue({ status: BudgetHeadStatus.Active });
      this.form?.markAsDirty();
      this.form?.markAsTouched();
      this.performSave({ message: this.translate.instant('core.locked') });

      this.isProjectBudgetLocked.set(true);
    }
  }

  unlock() {
    if (this.budgetHeadStatus !== BudgetHeadStatus.Preliminary) {
      this.form?.lockUnlockFormControls(false);
      this.budgetHeadStatus = 1;
      this.form?.patchValue({ status: BudgetHeadStatus.Preliminary });
      this.form?.markAsDirty();
      this.form?.markAsTouched();
      this.performSave({ message: this.translate.instant('core.unlocked') });

      this.isProjectBudgetLocked.set(false);
    }
  }

  buttonCreateForecast() {
    this.openEditInNewTab({
      id: 0,
      additionalProps: {
        editComponent: ProjectBudgetEditComponent,
        FormClass: ProjectBudgetForm,
        editTabLabel: this.service.getBudgetTabLabel(
          DistributionCodeBudgetType.ProjectBudgetForecast
        ),
        data: {
          createForecast: true,
          forecastFromId: this.form?.getIdControl()?.value,
        },
      },
    });
  }

  buttonUpdateForecastResult() {
    if (!this.form || !this.service || !this.form?.getIdControl()?.value)
      return;

    this.performUpdateForecastResult();
  }

  override performSave(options?: ProgressOptions | undefined): void {
    if (!this.form || !this.service) return;

    const dto = <BudgetHeadProjectDTO>this.form?.getRawValue();
    dto.rows = [];

    let tempLog: BudgetRowProjectChangeLogDTO[] = [];
    const fullRows = this.budgetRowData.getValue();
    fullRows.forEach(r => {
      if (r.budgetRowId <= 0) r.budgetRowId = 0;
      if (r.isDeleted && r.budgetRowId === 0) return;

      if (r.budgetRowId > 0 && r.changeLogItems && r.changeLogItems.length > 0)
        tempLog = tempLog.concat(r.changeLogItems[0]);
      dto.rows.push(r);
    });

    if (tempLog.length > 0) {
      const modalData = new ChangeLogDialogData();
      modalData.items = tempLog;
      modalData.title = this.translate.instant(
        'billing.projects.budget.handlerowhistory'
      );
      modalData.size = 'xl';

      this.dialogService
        .open(ChangeLogModal, modalData)
        .afterClosed()
        .subscribe((result: any) => {
          if (result) {
            result.budgetRowChangeLogItems.forEach(
              (logItem: BudgetRowProjectChangeLogDTO) => {
                const row = dto.rows.find(
                  r => r.budgetRowId === logItem.budgetRowId
                );
                if (row) {
                  row.changeLogItems = [];
                  row.changeLogItems.push(logItem);
                }
              }
            );

            this.save(dto, options);
          }
        });
    } else {
      this.save(dto, options);
    }
  }

  save(dto: BudgetHeadProjectDTO, options?: ProgressOptions | undefined): void {
    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service.save(dto).pipe(
        tap(value => {
          if (value.success) {
            const entityId = ResponseUtil.getEntityId(value);
            if (entityId && entityId > 0) this.budgetHeadId = entityId;
            this.updateFormValueAndEmitChange(value);

            this.setProjectBudgetSignals();
            this.messagingService.publish<ProjectRefreshBudgetPayload>(
              PROJECT_REFRESH_BUDGET,
              {
                projectId: this.form?.projectId?.value,
              }
            );
          }
        })
      ),
      undefined,
      undefined,
      options
    );
  }

  override triggerDelete(): void {
    const options: ProgressOptions = {
      callback: (val: BackendResponse) => {
        this.messagingService.publish<ProjectRefreshBudgetPayload>(
          PROJECT_REFRESH_BUDGET,
          {
            projectId: this.form?.projectId?.value,
          }
        );
      },
    };
    super.triggerDelete(options);
  }

  performCreateForecast(fromBudgetHeadId: number) {
    this.isLoadingBudgetHead.set(true);

    return this.performLoadData.load$(
      this.service.createForecast(fromBudgetHeadId).pipe(
        tap(value => {
          this.budgetHead = value;
          this.form?.reset(this.budgetHead);

          const rowDtos = this.budgetHead.rows.map(r => {
            const row = {
              ...new BudgetRowProjectDTO(),
              ...r,
            } as BudgetRowProjectDTO;
            BudgetRowProjectDTO.calculateRatio(row);
            return row;
          });

          this.rowsChanged(rowDtos, false);
          this.budgetRowAction.set({
            type: 'RESET_ROWS',
            rows: rowDtos,
          });
          this.budgetHeadId = this.budgetHead.budgetHeadId;
          this.budgetHeadStatus = +this.budgetHead.status;
          this.budgetHeadType = +this.budgetHead.type;

          const type = this.budgetTypes.find(t => t.id === this.budgetHeadType);
          this.budgetTypeText.set(type ? type.name : '');

          this.summarize(rowDtos);

          this.setProjectBudgetSignals();

          this.form?.markAsDirty();
          return of(undefined);
        }),
        finalize(() => this.isLoadingBudgetHead.set(false))
      )
    );
  }

  performUpdateForecastResult() {
    this.perform.crud(
      CrudActionTypeEnum.Work,
      this.service
        .updateForecastResult(this.form?.getIdControl()?.value)
        .pipe(tap(value => {})),
      () => {
        this.loadData().subscribe();
      },
      undefined,
      {
        showToastOnComplete: true,
        message: this.translate.instant(
          'billing.projects.budget.resultupdated'
        ),
      }
    );
  }

  getRow(type: ProjectCentralBudgetRowType): BudgetRowProjectDTO {
    const rows = this.budgetRowData.value;
    const row = rows.find(r => r.type === type);
    return row ?? new BudgetRowProjectDTO();
  }

  rowsChanged(rows: BudgetRowProjectDTO[], emitEvent: boolean = true) {
    this.summarize(rows);
    this.form?.customBudgetRowsPatchValues(rows);
    this.budgetRowData.next(rows);

    if (emitEvent) this.form?.markAsDirty();
  }

  setProjectBudgetSignals() {
    this.isProjectBudgetLocked.set(
      this.budgetHeadStatus === BudgetHeadStatus.Active
    );
    this.isProjectBudgetIB.set(
      this.budgetHead &&
        this.form?.type?.value === DistributionCodeBudgetType.ProjectBudgetIB
    );
    this.isProjectBudgetForecast.set(
      (this.budgetHead &&
        this.form?.type?.value ===
          DistributionCodeBudgetType.ProjectBudgetForecast) ||
        (this.form && this.form.data && this.form.data.createForecast)
    );

    this.compBudgetName.set(this.budgetHead.parentBudgetHeadName || ' ');
    this.resultUpdatedText.set(this.budgetHead.resultModified || '');
  }

  // Summarize
  summarize(rows: BudgetRowProjectDTO[]) {
    this.summarizeForecast(rows);
    if (this.isProjectBudgetForecast()) {
      this.summarizeResult(rows);
      this.summarizeComparableBudget(rows);
    }
  }

  summarizeForecast(rows: BudgetRowProjectDTO[]) {
    this.totalHoursIB = this.getSum(
      rows,
      ProjectCentralBudgetRowType.BillableMinutesInvoicedIB,
      SumType.Hours
    );
    const incomePersonell = this.getSum(
      rows,
      ProjectCentralBudgetRowType.IncomePersonellTotal,
      SumType.Amount
    );
    const incomeMaterial = this.getSum(
      rows,
      ProjectCentralBudgetRowType.IncomeMaterialTotal,
      SumType.Amount
    );
    this.incomeTotal = incomeMaterial + incomePersonell;

    this.costPersonell = this.getSum(
      rows,
      ProjectCentralBudgetRowType.CostPersonell,
      SumType.Amount
    );

    this.totalHours = this.getSum(
      rows,
      ProjectCentralBudgetRowType.CostPersonell,
      SumType.Hours
    );

    this.costMaterial = this.getSum(
      rows,
      ProjectCentralBudgetRowType.CostMaterial,
      SumType.Amount
    );

    this.costExpense = this.getSum(
      rows,
      ProjectCentralBudgetRowType.CostExpense,
      SumType.Amount
    );

    this.costOverhead = this.getSum(
      rows,
      ProjectCentralBudgetRowType.OverheadCost,
      SumType.Amount
    );

    this.costTotal =
      this.costPersonell +
      this.costMaterial +
      this.costExpense +
      this.costOverhead;
    this.result = this.incomeTotal - this.costTotal;
    this.resultRatio = this.getResultRatio(this.result, this.incomeTotal);
  }

  summarizeResult(rows: BudgetRowProjectDTO[]) {
    const incomePersonellResult = this.getSum(
      rows,
      ProjectCentralBudgetRowType.IncomePersonellTotal,
      SumType.ResultAmount
    );
    const incomeMaterialResult = this.getSum(
      rows,
      ProjectCentralBudgetRowType.IncomeMaterialTotal,
      SumType.ResultAmount
    );
    this.incomeTotalResult = incomeMaterialResult + incomePersonellResult;
    this.incomeTotalDiffResult = this.incomeTotal - this.incomeTotalResult;

    this.costPersonellResult = this.getSum(
      rows,
      ProjectCentralBudgetRowType.CostPersonell,
      SumType.ResultAmount
    );
    this.costPersonellDiffResult =
      this.costPersonell - this.costPersonellResult;

    this.totalHoursResult = this.getSum(
      rows,
      ProjectCentralBudgetRowType.CostPersonell,
      SumType.ResultHours,
      true
    );

    this.costMaterialResult = this.getSum(
      rows,
      ProjectCentralBudgetRowType.CostMaterial,
      SumType.ResultAmount
    );
    this.costMaterialDiffResult = this.costMaterial - this.costMaterialResult;

    this.costExpenseResult = this.getSum(
      rows,
      ProjectCentralBudgetRowType.CostExpense,
      SumType.ResultAmount
    );
    this.costExpenseDiffResult = this.costExpense - this.costExpenseResult;

    this.costOverheadResult = this.getSum(
      rows,
      ProjectCentralBudgetRowType.OverheadCost,
      SumType.ResultAmount
    );
    this.costOverheadDiffResult = this.costOverhead - this.costOverheadResult;

    this.costTotalResult =
      this.costPersonellResult +
      this.costMaterialResult +
      this.costExpenseResult +
      this.costOverheadResult;
    this.resultResult = this.incomeTotalResult - this.costTotalResult;
    this.resultRatioResult = this.getResultRatio(
      this.resultResult,
      this.incomeTotalResult
    );
    this.costTotalDiffResult = this.costTotal - this.costTotalResult;
    this.resultDiffResult = this.result - this.resultResult;
  }

  summarizeComparableBudget(rows: BudgetRowProjectDTO[]) {
    const incomePersonellCompBudget = this.getSum(
      rows,
      ProjectCentralBudgetRowType.IncomePersonellTotal,
      SumType.CompBudgetAmount
    );
    const incomeMaterialCompBudget = this.getSum(
      rows,
      ProjectCentralBudgetRowType.IncomeMaterialTotal,
      SumType.CompBudgetAmount
    );
    this.incomeTotalComparableBudget =
      incomeMaterialCompBudget + incomePersonellCompBudget;
    this.incomeTotalDiffComparableBudget =
      this.incomeTotal - this.incomeTotalComparableBudget;

    this.costPersonellComparableBudget = this.getSum(
      rows,
      ProjectCentralBudgetRowType.CostPersonell,
      SumType.CompBudgetAmount
    );
    this.costPersonellDiffComparableBudget =
      this.costPersonell - this.costPersonellComparableBudget;

    this.totalHoursComparableBudget = this.getSum(
      rows,
      ProjectCentralBudgetRowType.CostPersonell,
      SumType.CompBudgetHours,
      true
    );

    this.costMaterialComparableBudget = this.getSum(
      rows,
      ProjectCentralBudgetRowType.CostMaterial,
      SumType.CompBudgetAmount
    );
    this.costMaterialDiffComparableBudget =
      this.costMaterial - this.costMaterialComparableBudget;

    this.costExpenseComparableBudget = this.getSum(
      rows,
      ProjectCentralBudgetRowType.CostExpense,
      SumType.CompBudgetAmount
    );
    this.costExpenseDiffComparableBudget =
      this.costExpense - this.costExpenseComparableBudget;

    this.costOverheadComparableBudget = this.getSum(
      rows,
      ProjectCentralBudgetRowType.OverheadCost,
      SumType.CompBudgetAmount
    );
    this.costOverheadDiffComparableBudget =
      this.costOverhead - this.costOverheadComparableBudget;

    this.costTotalComparableBudget =
      this.costPersonellComparableBudget +
      this.costMaterialComparableBudget +
      this.costExpenseComparableBudget +
      this.costOverheadComparableBudget;
    this.resultComparableBudget =
      this.incomeTotalComparableBudget - this.costTotalComparableBudget;
    this.resultRatioComparableBudget = this.getResultRatio(
      this.resultComparableBudget,
      this.incomeTotalComparableBudget
    );
    this.costTotalDiffComparableBudget =
      this.costTotal - this.costTotalComparableBudget;
    this.resultDiffComparableBudget = this.result - this.resultComparableBudget;
  }

  getSum(
    rows: BudgetRowProjectDTO[],
    type: ProjectCentralBudgetRowType,
    sumType: SumType = SumType.Amount,
    getHours: boolean = false
  ) {
    let sum = 0;
    if (type === ProjectCentralBudgetRowType.OverheadCost) {
      const overheadCostPerHour = rows.find(
        r =>
          r.type === ProjectCentralBudgetRowType.OverheadCostPerHour &&
          !r.isDeleted
      );
      sum =
        rows
          .filter(r => r.type === type && !r.isDeleted)
          .reduce(
            (tot, r) => tot + (r[sumType] && r[sumType] > 0 ? r[sumType] : 0),
            0
          ) || 0;

      if (overheadCostPerHour && overheadCostPerHour.totalAmount > 0) {
        if (sumType === SumType.Amount)
          sum += overheadCostPerHour.totalAmount * this.totalHours;
        else if (sumType === SumType.CompBudgetAmount)
          sum +=
            overheadCostPerHour.totalAmountCompBudget *
            this.totalHoursComparableBudget;
      }
    } else {
      const total =
        rows
          .filter(r => r.type === type && !r.isDeleted)
          .reduce((tot, r) => tot + r[sumType], 0) || 0;

      if (getHours) sum = total / 60;
      else sum = total;
    }
    return sum;
  }

  getResultRatio(result: number, value: number): string {
    if (value === 0) return result < 0 ? '-100 %' : '100 %';
    return ((result / value) * 100).round(2) + ' %';
  }
}
