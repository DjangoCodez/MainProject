import { Component, DestroyRef, inject, input, Input, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormArray } from '@angular/forms';
import { EmbeddedGridBaseDirective } from '@shared/directives/grid-base/embedded-grid-base.directive';
import { ShiftTypeService } from '@shared/features/shift-type/services/shift-type.service';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IdForm, IId } from '@shared/models/id.form.model';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, take, tap } from 'rxjs';
import { TimeBreakTemplatesForm } from '../../../models/time-break-templates-form.model';

@Component({
  selector: 'soe-tbt-shift-types-grid',
  standalone: false,
  templateUrl:
    '../../../../../../shared/ui-components/grid/grid-wrapper/embedded-grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
})
export class TbtShiftTypesGridComponent
  extends EmbeddedGridBaseDirective<IId, TimeBreakTemplatesForm>
  implements OnInit, OnChanges
{
  @Input({ required: true }) form!: TimeBreakTemplatesForm;
  noMargin = input(true);
  height = input(66);

  shiftTypeService = inject(ShiftTypeService);
  private destroyRef = inject(DestroyRef);

  shiftTypes: SmallGenericType[] = [];

  override ngOnInit(): void {
    super.ngOnInit();

    this.embeddedGridOptions.formRows =
      this.form.shiftTypeIds || new FormArray<any>([]);
    this.embeddedGridOptions.rowFormType = new IdForm({
      validationHandler: this.form.formValidationHandler,
      element: { id: 0 },
    });
    this.embeddedGridOptions.rowType = { id: 0 };

    this.startFlow(
      Feature.Time_Schedule_TimeBreakTemplate,
      'common.shifttype',
      {
        lookups: [this.loadShiftTypes()],
      }
    );

    this.form.shiftTypeIds.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(v => this.initRows(v));
    this.initRows(this.form.shiftTypeIds.value);
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['form'] && this.form && this.embeddedGridOptions) {
      this.embeddedGridOptions.formRows =
        this.form.shiftTypeIds || new FormArray<any>([]);
    }
  }

  override onGridReadyToDefine(grid: GridComponent<IId>): void {
    super.onGridReadyToDefine(grid);

    this.translate
      .get(['common.shifttype', 'core.delete'])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnSelect(
          'id',
          terms['common.shifttype'],
          this.shiftTypes || [],
          undefined,
          {
            dropDownIdLabel: 'id',
            dropDownValueLabel: 'name',
            flex: 100,
            editable: true,
            dynamicSelectOptions: (params: any) => {
              const currentVal = params.value != null ? Number(params.value) : null;
              const rowId = params.data?.id != null ? Number(params.data.id) : null;
              const allSelectedIds = (this.form.shiftTypeIds.value as any[])
                ?.map(item => Number(item.id))
                .filter(id => id !== 0) || [];

              return this.shiftTypes.filter(st => {
                const stId = Number(st.id);
                return (
                  stId === currentVal ||
                  stId === rowId ||
                  !allSelectedIds.includes(stId)
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
    this.initRows(this.form.shiftTypeIds.value);
  }

  private initRows(rows: IId[]) {
    this.rowData.next(rows);
  }

  override addRow(): void {
    const row: IId = {
      id: 0,
    };
    super.addRow(row, this.form.shiftTypeIds, IdForm);
  }

  override deleteRow(row: any) {
    super.deleteRow(row, this.form.shiftTypeIds);
  }

  override onCellEditingStopped(event: any): void {
    const valuesChanged = this.onCellEditingStoppedCheckIfHasChanged(event);
    if (valuesChanged) this.form?.markAsDirty();
    else return;

    if (!event.colDef?.field) return;
    if (!this.form.shiftTypeIds) return;

    const field = event.colDef.field;
    const rowValue = event.data;
    const idx = event.rowIndex || 0;

    const ctrl = this.form.shiftTypeIds.at(idx);
    if (!ctrl) return;

    ctrl.patchValue({ [field]: rowValue[field] }, { emitEvent: true });
    ctrl.markAsDirty();
    ctrl.updateValueAndValidity({ emitEvent: true });
  }

  loadShiftTypes(): Observable<SmallGenericType[]> {
    return this.shiftTypeService
      .getShiftTypesDict(false)
      .pipe(tap(x => (this.shiftTypes = x)));
  }
}
