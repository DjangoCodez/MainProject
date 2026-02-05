import { Component, Input, OnInit } from '@angular/core';
import { PayrollPriceTypesForm } from '@features/time/payroll-price-types/models/payroll-price-types-form.model';
import { PayrollPriceTypesPeriodsForm } from '@features/time/payroll-price-types/models/payroll-price-types-periods-form.model';
import { EmbeddedGridBaseDirective } from '@shared/directives/grid-base/embedded-grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IPayrollPriceTypePeriodDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { DateUtil } from '@shared/util/date-util';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { CellEditingStoppedEvent } from 'ag-grid-community';
import { take } from 'rxjs';

enum GridColumns {
  fromDate = 'fromDate',
  amount = 'amount',
}

@Component({
  selector: 'soe-payroll-price-types-edit-periods-grid',
  templateUrl: './payroll-price-types-edit-periods-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class PayrollPriceTypesEditPeriodsGridComponent
  extends EmbeddedGridBaseDirective<IPayrollPriceTypePeriodDTO>
  implements OnInit
{
  @Input({ required: true }) form!: PayrollPriceTypesForm;

  override ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Preferences_SalarySettings_PriceType_Edit,
      'Time.Payroll.PayrollPriceType.PayrollPriceTypesPeriods'
    );

    this.form.valueChanges.subscribe(value => {
      this.rowData.next(value.periods);
    });
  }

  override onGridReadyToDefine(
    grid: GridComponent<IPayrollPriceTypePeriodDTO>
  ) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get(['common.fromdate', 'common.amount', 'core.delete'])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnDate(
          GridColumns.fromDate,
          terms['common.fromdate'],
          {
            flex: 50,
            editable: true,
          }
        );
        this.grid.addColumnNumber(GridColumns.amount, terms['common.amount'], {
          flex: 50,
          editable: true,
          decimals: 3,
          resizable: false,
          suppressSizeToFit: true,
        });
        this.grid.addColumnIconDelete({
          tooltip: terms['core.delete'],
          onClick: row => {
            this.deleteRow(row);
          },
        });
        super.finalizeInitGrid();
      });
  }

  override addRow(): void {
    const newrow: IPayrollPriceTypePeriodDTO = {
      payrollPriceTypePeriodId: 0,
      payrollPriceTypeId: this.form?.getIdControl()?.value || 0,
      fromDate: DateUtil.getToday(),
      amount: 0,
    };

    super.addRow(
      newrow,
      this.form.payrollPriceTypePeriods,
      PayrollPriceTypesPeriodsForm
    );

    setTimeout(() => {
      // Need to wait for grid to be ready before starting editing
      this.grid.startEditing(
        this.rowData.value.length - 1,
        GridColumns.fromDate
      );
    });
  }

  override onCellEditingStopped(event: CellEditingStoppedEvent) {
    if (!super.onCellEditingStoppedCheckIfHasChanged(event)) {
      return;
    }

    const field = event.colDef.field;
    if (!field) {
      return;
    }

    const rowsForm = this.form.payrollPriceTypePeriods.at(event.rowIndex ?? 0);

    switch (field) {
      case GridColumns.fromDate:
        rowsForm.controls.fromDate.patchValue(event.newValue, {
          emitEvent: false,
        });
        break;
      case GridColumns.amount:
        rowsForm.controls.amount.patchValue(event.newValue, {
          emitEvent: false,
        });
        break;
    }

    this.setDirty();
  }

  override deleteRow(row: any) {
    super.deleteRow(row, this.form.payrollPriceTypePeriods);
  }

  private setDirty() {
    this.form?.markAsDirty();
    this.form?.markAsTouched();
  }
}
