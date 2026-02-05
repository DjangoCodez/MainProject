import {
  Component,
  DestroyRef,
  inject,
  input,
  OnInit,
  signal,
} from '@angular/core';
import { ProjectBudgetEditComponent } from '@features/billing/project-budget/components/budget-edit/project-budget-edit.component';
import { ProjectBudgetForm } from '@features/billing/project-budget/models/projet-budget-form.model';
import { ProjectService } from '@features/billing/project/services/project.service';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { SoeFormGroup, SoeSelectFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  DistributionCodeBudgetType,
  Feature,
  ProjectCentralBudgetRowType,
  TermGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import { IBudgetHeadGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { GridComponent } from '@ui/grid/grid.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { BehaviorSubject, filter, take, tap } from 'rxjs';
import { RowDoubleClickedEvent } from 'ag-grid-community';
import { BudgetHeadGridDTO } from '@features/economy/budget/models/budget.model';
import _ from 'lodash';
import { Perform } from '@shared/util/perform.class';
import { CrudActionTypeEnum } from '@shared/enums';
import { ProjectBudgetService } from '@features/billing/project-budget/services/project-budget.service';
import { MessagingService } from '@shared/services/messaging.service';
import {
  PROJECT_REFRESH_BUDGET,
  ProjectRefreshBudgetPayload,
} from '@features/billing/project/models/project-event';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  selector: 'soe-project-budgethead-grid',
  standalone: false,
  templateUrl: './project-budgethead-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
})
export class ProjectBudgetheadGridComponent
  extends GridBaseDirective<IBudgetHeadGridDTO>
  implements OnInit
{
  coreService = inject(CoreService);
  projectService = inject(ProjectService);
  projectBudgetService = inject(ProjectBudgetService);
  messageboxService = inject(MessageboxService);
  messagingService = inject(MessagingService);
  perform = new Perform<any>(this.progressService);

  budgetHeadRows = new BehaviorSubject<IBudgetHeadGridDTO[]>([]);
  projectId = input.required<number>();
  name = input<string>();
  number = input<string>();
  startDate = input<Date | undefined>();
  stopDate = input<Date | undefined>();
  budgetTypes: SmallGenericType[] = [];
  budgetTypesAll: SmallGenericType[] = [];

  destroyed$ = inject(DestroyRef);
  validationHandler = inject(ValidationHandler);
  form = new SoeFormGroup(this.validationHandler, {
    budgetTypeId: new SoeSelectFormControl(0),
  });

  migrationNeeded = signal(false);
  showBudgetTypeSelect = signal(false);
  oldBudgetHeadId: number = 0;

  allowedTypes: ProjectCentralBudgetRowType[] = [
    ProjectCentralBudgetRowType.BillableMinutesInvoiced,
    ProjectCentralBudgetRowType.IncomePersonellTotal,
    ProjectCentralBudgetRowType.IncomeMaterialTotal,
    ProjectCentralBudgetRowType.IncomeTotal,
    ProjectCentralBudgetRowType.CostPersonell,
    ProjectCentralBudgetRowType.CostMaterial,
    ProjectCentralBudgetRowType.CostExpense,
    ProjectCentralBudgetRowType.OverheadCostPerHour,
    ProjectCentralBudgetRowType.OverheadCost,
    ProjectCentralBudgetRowType.CostTotal,
    ProjectCentralBudgetRowType.BillableMinutesInvoicedIB,
    ProjectCentralBudgetRowType.IncomePersonellTotalIB,
    ProjectCentralBudgetRowType.IncomeMaterialTotalIB,
    //ProjectCentralBudgetRowType.IncomeTotalIB,
    ProjectCentralBudgetRowType.CostPersonellIB,
    ProjectCentralBudgetRowType.CostMaterialIB,
    ProjectCentralBudgetRowType.CostExpenseIB,
    ProjectCentralBudgetRowType.OverheadCostPerHourIB,
    ProjectCentralBudgetRowType.OverheadCostIB,
    ProjectCentralBudgetRowType.CostTotalIB,
  ];

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Billing_Project_Edit_Budget,
      'Billing.Project.Budget',
      {
        skipInitialLoad: true,
        lookups: [this.loadProjectBudgetTypes(), this.loadBudgetHeadRows()],
      }
    );

    this.messagingService
      .onEvent<ProjectRefreshBudgetPayload>(PROJECT_REFRESH_BUDGET)
      .pipe(
        takeUntilDestroyed(this.destroyed$),
        filter(message => message?.data?.projectId === this.projectId())
      )
      .subscribe(() => this.loadBudgetHeadRows().subscribe());
  }

  override onGridReadyToDefine(grid: GridComponent<IBudgetHeadGridDTO>): void {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.type',
        'common.name',
        'common.created',
        'core.edit',
        'economy.accounting.budget.createforecast',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.onRowDoubleClicked = (
          event: RowDoubleClickedEvent<BudgetHeadGridDTO, any>
        ) => {
          this.openProjectBudget(event.data!);
        };

        this.grid.addColumnText('type', terms['common.type'], { flex: 1 });
        this.grid.addColumnText('name', terms['common.name'], { flex: 1 });
        this.grid.addColumnText('created', terms['common.created'], {
          flex: 1,
        });
        this.grid.addColumnIcon('', '', {
          iconName: 'chart-mixed-up-circle-dollar',
          iconClass: 'chart-mixed-up-circle-dollar',
          tooltip: terms['economy.accounting.budget.createforecast'],
          onClick: row => {
            this.createForecast(row.budgetHeadId);
          },
          showIcon: row => {
            return (
              row.budgetTypeId ===
                DistributionCodeBudgetType.ProjectBudgetForecast ||
              row.budgetTypeId ===
                DistributionCodeBudgetType.ProjectBudgetExtended
            );
          },
        });
        this.grid.addColumnIcon('', '', {
          iconName: 'pen',
          iconClass: 'pen',
          tooltip: terms['core.edit'],
          onClick: row => {
            this.openProjectBudget(row);
          },
        });
        super.finalizeInitGrid();
        this.grid.setNbrOfRowsToShow(5, 5);
      });
  }

  loadProjectBudgetTypes() {
    return this.coreService
      .getTermGroupContent(TermGroup.BudgetType, false, true)
      .pipe(
        tap(types => {
          types.forEach((type, i) => {
            if (
              type.id === +DistributionCodeBudgetType.ProjectBudgetExtended ||
              type.id === +DistributionCodeBudgetType.ProjectBudgetIB
            )
              this.budgetTypesAll.push(type);
          });
        })
      );
  }

  loadBudgetHeadRows() {
    return this.projectService
      .getBudgetHeadGridForProject(
        this.projectId(),
        SoeConfigUtil.actorCompanyId
      )
      .pipe(
        tap(x => {
          const oldBudget = x.find(
            p =>
              p.budgetTypeId === DistributionCodeBudgetType.ProjectBudget ||
              p.budgetTypeId === 0
          );
          setTimeout(() => {
            if (oldBudget) {
              this.oldBudgetHeadId = oldBudget.budgetHeadId;
              this.migrationNeeded.set(true);
            } else {
              this.migrationNeeded.set(false);
            }
          }, 50);

          this.budgetHeadRows.next(x);
          this.setProjectBudgetTypes();
        })
      );
  }

  setProjectBudgetTypes() {
    this.budgetTypes = [];
    if (
      this.budgetHeadRows.value.some(
        r =>
          r.budgetTypeId === +DistributionCodeBudgetType.ProjectBudgetExtended
      )
    ) {
      const type = this.budgetTypesAll.find(
        t => t.id === +DistributionCodeBudgetType.ProjectBudgetForecast
      );
      if (type) this.budgetTypes.push(type);
    } else {
      const type = this.budgetTypesAll.find(
        t => t.id === +DistributionCodeBudgetType.ProjectBudgetExtended
      );
      if (type) this.budgetTypes.push(type);
    }
    if (
      !this.budgetHeadRows.value.some(
        r => r.budgetTypeId === +DistributionCodeBudgetType.ProjectBudgetIB
      )
    ) {
      const type = this.budgetTypesAll.find(
        t => t.id === +DistributionCodeBudgetType.ProjectBudgetIB
      );
      if (type) this.budgetTypes.push(type);
    }
    if (
      !this.budgetHeadRows.value.some(
        r =>
          r.budgetTypeId === +DistributionCodeBudgetType.ProjectBudgetChangeWork
      )
    ) {
      const type = this.budgetTypesAll.find(
        t => t.id === +DistributionCodeBudgetType.ProjectBudgetChangeWork
      );
      if (type) this.budgetTypes.push(type);
    }
    this.showBudgetTypeSelect.set(this.budgetTypes.length > 0);
  }

  addBudget() {
    const projectBudgetTypeId = this.form?.get('budgetTypeId')
      ?.value as DistributionCodeBudgetType;
    if (!projectBudgetTypeId) return;

    if (
      this.budgetHeadRows.value.some(
        r => r.budgetTypeId === projectBudgetTypeId
      )
    ) {
      this.messageboxService.error(
        this.translate.instant('core.error'),
        this.translate.instant('billing.projects.budget.typeerror')
      );
      return;
    }

    this.openNewTab(0, projectBudgetTypeId, {
      projectId: this.projectId(),
      budgetTypeId: projectBudgetTypeId,
      name: this.name(),
      number: this.number(),
      startDate: this.startDate(),
      stopDate: this.stopDate(),
    });
  }

  openProjectBudget(budget: IBudgetHeadGridDTO) {
    this.openNewTab(budget.budgetHeadId, budget.budgetTypeId);
  }

  createForecast(budgetHeadId: number) {
    if (budgetHeadId > 0) {
      this.openNewTab(0, DistributionCodeBudgetType.ProjectBudgetForecast, {
        createForecast: true,
        forecastFromId: budgetHeadId,
      });
    }
  }

  openNewTab(id: number, type: DistributionCodeBudgetType, data?: any) {
    this.openEditInNewTab.emit({
      id,
      additionalProps: {
        editComponent: ProjectBudgetEditComponent,
        FormClass: ProjectBudgetForm,
        editTabLabel: this.projectBudgetService.getBudgetTabLabel(type),
        data,
      },
    });
  }

  migrate() {
    this.perform.crud(
      CrudActionTypeEnum.Work,
      this.projectBudgetService.migrateBudgetHead(this.oldBudgetHeadId),
      result => {
        if (result.success) this.loadBudgetHeadRows().subscribe();
      },
      undefined,
      undefined
    );
  }
}
