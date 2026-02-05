import { Component, DestroyRef, inject, input, Input, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormArray } from '@angular/forms';
import { EmbeddedGridBaseDirective } from '@shared/directives/grid-base/embedded-grid-base.directive';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IdForm, IId } from '@shared/models/id.form.model';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { DateUtil } from '@shared/util/date-util';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, of, take } from 'rxjs';
import { TimeBreakTemplatesForm } from '../../../models/time-break-templates-form.model';

@Component({
  selector: 'soe-tbt-weekdays-grid',
  standalone: false,
  templateUrl:
    '../../../../../../shared/ui-components/grid/grid-wrapper/embedded-grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
})
export class TbtWeekdaysGridComponent
  extends EmbeddedGridBaseDirective<IId, TimeBreakTemplatesForm>
  implements OnInit, OnChanges
{
  @Input({ required: true }) form!: TimeBreakTemplatesForm;
  noMargin = input(true);
  height = input(66);

  private destroyRef = inject(DestroyRef);

  weekdays: SmallGenericType[] = [];

  override ngOnInit(): void {
    super.ngOnInit();

    this.embeddedGridOptions.formRows =
      this.form.dayOfWeeks || new FormArray<any>([]);
    this.embeddedGridOptions.rowFormType = new IdForm({
      validationHandler: this.form.formValidationHandler,
      element: { id: -1 },
    });
    this.embeddedGridOptions.rowType = { id: -1 };

    this.startFlow(
      Feature.Time_Schedule_TimeBreakTemplate,
      'common.weekday',
      {
        lookups: [this.loadWeekdays()],
      }
    );
    this.form.dayOfWeeks.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(v => this.initRows(v));
    this.initRows(this.form.dayOfWeeks.value);
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['form'] && this.form && this.embeddedGridOptions) {
      this.embeddedGridOptions.formRows =
        this.form.dayOfWeeks || new FormArray<any>([]);
    }
  }

  override onGridReadyToDefine(grid: GridComponent<IId>): void {
    super.onGridReadyToDefine(grid);

    this.translate
      .get(['common.weekday', 'core.delete'])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnSelect(
          'id',
          terms['common.weekday'],
          this.weekdays || [],
          undefined,
          {
            dropDownIdLabel: 'id',
            dropDownValueLabel: 'name',
            flex: 100,
            editable: true,
            dynamicSelectOptions: (params: any) => {
              if (!params.data) return this.weekdays;
              const row = params.data;
              const val = params.value;
              const allSelectedIds = (this.form.dayOfWeeks.value as any[])
                ?.map(item => Number(item.id))
                .filter(id => id !== -1) || [];

              return this.weekdays.filter(wd => {
                const wdId = Number(wd.id);
                return (
                  wdId === Number(val) ||
                  wdId === Number(row.id) ||
                  !allSelectedIds.includes(wdId)
                );
              });
            },
          }
        );
        this.grid.addColumnIconDelete({
          tooltip: terms['core.delete'],
          onClick: row => {
            this.deleteRow(row);
          },
        });

        this.grid.setNbrOfRowsToShow(1, 10);
        super.finalizeInitGrid({ hidden: true });
      });
  }

  override onGridIsDefined(): void {
    this.initRows(this.form.dayOfWeeks.value);
  }

  private initRows(rows: IId[]) {
    this.rowData.next(rows);
  }

  override addRow(): void {
    const row: IId = {
      id: -1,
    };
    super.addRow(row, this.form.dayOfWeeks, IdForm);
  }

  override deleteRow(row: any) {
    super.deleteRow(row, this.form.dayOfWeeks);
  }

  override onCellEditingStopped(event: any): void {
    const valuesChanged = this.onCellEditingStoppedCheckIfHasChanged(event);
    if (valuesChanged) this.form?.markAsDirty();
    else return;

    if (!event.colDef?.field) return;
    if (!this.form.dayOfWeeks) return;

    const field = event.colDef.field;
    const rowValue = event.data;
    const idx = event.rowIndex || 0;

    const ctrl = this.form.dayOfWeeks.at(idx);
    if (!ctrl) return;

    ctrl.patchValue({ [field]: rowValue[field] }, { emitEvent: true });
    ctrl.markAsDirty();
    ctrl.updateValueAndValidity({ emitEvent: true });
  }

  loadWeekdays(): Observable<SmallGenericType[]> {
    this.weekdays = DateUtil.getDayOfWeekNames(true, 'wide', true);
    return of(this.weekdays);
  }
}
