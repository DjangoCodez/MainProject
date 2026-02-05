import { Component, inject, input, Input, OnInit } from '@angular/core';
import { DistributionRuleHeadsForm } from '@features/time/planning-periods/models/distribution-rule-heads-form.model';
import { DistributionRulesForm } from '@features/time/planning-periods/models/dr-rule-form.model';
import { DistributionRuleService } from '@features/time/planning-periods/services/distribution-rule.service';

import { EmbeddedGridBaseDirective } from '@shared/directives/grid-base/embedded-grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IProductSmallDTO } from '@shared/models/generated-interfaces/ProductDTOs';
import { IPayrollProductDistributionRuleDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, startWith, take, tap } from 'rxjs';

@Component({
  selector: 'soe-dr-rules-grid',
  standalone: false,
  templateUrl:
    '../../../../../../shared/ui-components/grid/grid-wrapper/embedded-grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
})
export class DrRulesGridComponent
  extends EmbeddedGridBaseDirective<
    IPayrollProductDistributionRuleDTO,
    DistributionRuleHeadsForm
  >
  implements OnInit
{
  @Input({ required: true }) form!: DistributionRuleHeadsForm;

  private readonly service = inject(DistributionRuleService);

  private payrollProducts: IProductSmallDTO[] = [];

  toolbarNoBorder = input(true);
  toolbarNoMargin = input(true);
  toolbarNoTopBottomPadding = input(true);
  height = input(66);

  override ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Preferences_TimeSettings_PlanningPeriod,
      'time.distributionrules.rules.grid',
      {
        lookups: [this.loadPayrollProducts()],
      }
    );

    this.form.rules.valueChanges
      .pipe(startWith(this.form.rules.getRawValue()))
      .subscribe(r => {
        this.initRows(r);
      });
  }

  override onGridReadyToDefine(
    grid: GridComponent<IPayrollProductDistributionRuleDTO>
  ): void {
    super.onGridReadyToDefine(grid);

    this.embeddedGridOptions.newRowStartEditField = 'start';

    this.translate
      .get([
        'time.payroll.payrollproduct.payrollproduct',
        'common.start',
        'common.stop',
        'common.permission',
        'core.delete',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnTimeSpan('start', terms['common.start'], {
          flex: 33,
          editable: true,
        });
        this.grid.addColumnTimeSpan('stop', terms['common.stop'], {
          flex: 33,
          editable: true,
        });
        this.grid.addColumnSelect(
          'payrollProductId',
          terms['time.payroll.payrollproduct.payrollproduct'],
          this.payrollProducts || [],
          null,
          {
            flex: 33,
            editable: true,
            dropDownIdLabel: 'productId',
            dropDownValueLabel: 'name',
          }
        );
        this.grid.addColumnIconDelete({
          tooltip: terms['core.delete'],
          onClick: row => {
            this.deleteRow(row);
          },
        });

        this.grid.setNbrOfRowsToShow(1);
        super.finalizeInitGrid({ hidden: true });
      });
  }

  override onCellEditingStopped(event: any) {
    // Keep base behavior (marks form dirty)
    super.onCellEditingStopped(event);

    if (!this.onCellEditingStoppedCheckIfHasChanged(event)) return;
    if (!event.colDef?.field) return;

    const field = event.colDef
      .field as keyof IPayrollProductDistributionRuleDTO;
    const rowValue = event.data as IPayrollProductDistributionRuleDTO;

    // Find corresponding FormGroup. Prefer id; fallback to index by reference.
    let idx = this.form.rules.controls.findIndex(
      (c: any) =>
        (rowValue.payrollProductDistributionRuleId &&
          c.value.payrollProductDistributionRuleId ===
            rowValue.payrollProductDistributionRuleId) ||
        c.value === rowValue
    );
    if (idx === -1 && typeof event.rowIndex === 'number') {
      // If grid is sorted/filtered this may not align, but fallback
      idx = event.rowIndex;
    }
    const ctrl = this.form.rules.at(idx) as DistributionRulesForm | undefined;
    if (!ctrl) return;

    // Patch the single field so Angular emits value & validators re-run
    ctrl.patchValue({ [field]: rowValue[field] }, { emitEvent: true });
    ctrl.markAsDirty();
    ctrl.updateValueAndValidity({ emitEvent: true });

    // Optionally trigger array validity if you have cross-row validators
    // this.form.rules.updateValueAndValidity({ emitEvent: false });
  }

  private initRows(rows: IPayrollProductDistributionRuleDTO[]) {
    this.rowData.next(rows);
  }

  override addRow(): void {
    const row: Partial<IPayrollProductDistributionRuleDTO> = {
      payrollProductDistributionRuleId: 0,
      payrollProductDistributionRuleHeadId:
        this.form?.value.payrollProductDistributionRuleHeadId,
      payrollProductId: 0,
      start: 0,
      stop: 0,
    };
    super.addRow(
      row as IPayrollProductDistributionRuleDTO,
      this.form.rules,
      DistributionRulesForm
    );
  }

  override deleteRow(row: any) {
    super.deleteRow(row, this.form.rules);
  }

  // LOAD DATA

  private loadPayrollProducts(): Observable<void> {
    return this.performLoadData.load$(
      this.service.getPayrollProductsSmall().pipe(
        tap((value: IProductSmallDTO[]) => {
          this.payrollProducts = value;
        })
      )
    );
  }
}
