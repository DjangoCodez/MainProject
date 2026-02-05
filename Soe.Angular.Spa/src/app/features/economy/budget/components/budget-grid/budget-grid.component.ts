import { Component, OnInit, inject } from '@angular/core';
import { ProjectBudgetEditComponent } from '@features/billing/project-budget/components/budget-edit/project-budget-edit.component';
import { ProjectBudgetForm } from '@features/billing/project-budget/models/projet-budget-form.model';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import {
  DistributionCodeBudgetType,
  Feature,
} from '@shared/models/generated-interfaces/Enumerations';
import { IBudgetHeadGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, take } from 'rxjs';
import { BudgetService } from '../../services/budget.service';

@Component({
  selector: 'soe-budget-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class BudgetGridComponent
  extends GridBaseDirective<IBudgetHeadGridDTO, BudgetService>
  implements OnInit
{
  service = inject(BudgetService);

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Economy_Accounting_Budget_Edit,
      'Economy.Accounting.Budget.Budgets'
    );
  }

  override onGridReadyToDefine(grid: GridComponent<IBudgetHeadGridDTO>): void {
    super.onGridReadyToDefine(grid);
    this.translate
      .get([
        'common.name',
        'economy.accounting.accountyear.accountyear',
        'common.created',
        'economy.accounting.budget.noofperiods',
        'common.type',
        'common.status',
        'core.edit',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('name', terms['common.name'], { flex: 1 });
        this.grid.addColumnText(
          'accountingYear',
          terms['economy.accounting.accountyear.accountyear'],
          {
            flex: 1,
            enableHiding: true,
            sort: 'desc',
          }
        );

        this.grid.addColumnText(
          'noOfPeriods',
          terms['economy.accounting.budget.noofperiods'],
          {
            width: 110,
            enableHiding: true,
          }
        );

        this.grid.addColumnText('status', terms['common.tracerows.status'], {
          flex: 1,
          enableHiding: true,
        });

        this.grid.addColumnText('type', terms['common.type'], {
          flex: 1,
          enableHiding: true,
        });

        this.grid.addColumnText('created', terms['common.created'], {
          flex: 1,
          enableHiding: true,
        });

        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            this.edit(row);
          },
        });
        super.finalizeInitGrid();
      });
  }

  openProjectBudget(budgetHeadId: number) {
    this.openEditInNewTab.emit({
      id: budgetHeadId,
      additionalProps: {
        editComponent: ProjectBudgetEditComponent,
        FormClass: ProjectBudgetForm,
        editTabLabel: 'billing.projects.list.budget',
      },
    });
  }

  override loadData(
    id?: number | undefined,
    additionalProps?: { budgetType: number; actorId: number }
  ): Observable<IBudgetHeadGridDTO[]> {
    return super.loadData(id, {
      budgetType: DistributionCodeBudgetType.AccountingBudget,
      actorId: SoeConfigUtil.actorCompanyId,
    });
  }
}
