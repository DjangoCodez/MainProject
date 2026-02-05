import { CellValueChangedEvent } from 'ag-grid-community';
import {
  Component,
  inject,
  Input,
  OnDestroy,
  OnInit,
  signal,
} from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { ExtraFieldForm } from '@shared/features/extra-fields/models/extra-fields-form.model';
import { ExtraFieldsService } from '@shared/features/extra-fields/services/extra-fields.service';
import { TermGroup_ExtraFieldValueType } from '@shared/models/generated-interfaces/Enumerations';
import { IExtraFieldValueDTO } from '@shared/models/generated-interfaces/ExtraFieldDTO';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { NumberUtil } from '@shared/util/number-util';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { BehaviorSubject, Subject, take, takeUntil } from 'rxjs';
import { ExtraFieldsUrlParamsService } from '@shared/features/extra-fields/services/extra-fields-url.service';

@Component({
  selector: 'soe-extra-fields-edit-values-grid',
  templateUrl: './extra-fields-edit-values-grid.component.html',
  styleUrl: './extra-fields-edit-values-grid.component.scss',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ExtraFieldsEditValuesGridComponent
  extends GridBaseDirective<IExtraFieldValueDTO>
  implements OnInit, OnDestroy
{
  @Input({ required: true }) form!: ExtraFieldForm;

  extraFieldsService = inject(ExtraFieldsService);
  urlService = inject(ExtraFieldsUrlParamsService);

  private _destroy$ = new Subject<void>();

  rows = new BehaviorSubject<IExtraFieldValueDTO[]>([]);

  override ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      this.extraFieldsService.getPermission(this.urlService.entityType()),
      'Common.ExtraFields.Values',
      { skipInitialLoad: true }
    );

    // Update grid data when form changes
    this.form?.extraFieldValues.valueChanges
      .pipe(takeUntil(this._destroy$))
      .subscribe(rows => {
        this.setGridData(rows, false);
      });

    this.form?.valueChanges.pipe(takeUntil(this._destroy$)).subscribe(frm => {
      // Set grid data when form is loaded on copy
      if (frm.isCopy) {
        this.form.controls.isCopy.patchValue(false);
        this.setGridData(frm.extraFieldValues, false);
      }
    });
  }

  override createGridToolbar(): void {
    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('addrow', {
          iconName: signal('plus'),
          caption: signal('common.newrow'),
          tooltip: signal('common.newrow'),
          onAction: () => this.addRow(),
        }),
      ],
    });
  }

  override onGridReadyToDefine(grid: GridComponent<IExtraFieldValueDTO>) {
    super.onGridReadyToDefine(grid);

    grid.setNbrOfRowsToShow(5, 10);
    this.grid.context.suppressGridMenu = true;

    this.grid.api.updateGridOptions({
      onCellValueChanged: this.onCellValueChanged.bind(this),
    });

    this.translate
      .get(['common.sort', 'common.value', 'core.delete'])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnNumber('sort', terms['common.sort'], {
          width: 75,
          rowDragable: true,
        });
        this.grid.addColumnText('value', terms['common.value'], {
          flex: 1,
          editable: true,
          enableHiding: false,
        });

        this.grid.addColumnIconDelete({
          tooltip: terms['core.delete'],
          onClick: row => {
            this.deleteRow(row);
          },
        });

        // Enable row drag and drop
        this.grid.applyDragOptions({
          rowDragFinishedSortIndexNrFieldName: 'sort',
          rowDragFinishedCallback: this.patchForm.bind(this),
        });

        super.finalizeInitGrid();
      });
  }

  private addRow() {
    // Create new row and update grid data
    const rows = this.rows.value;
    const maxCount = NumberUtil.max(rows, 'sort');
    const row: Partial<IExtraFieldValueDTO> = {
      type: TermGroup_ExtraFieldValueType.String,
      sort: maxCount + 1,
      value: '',
    };
    rows.push(row as IExtraFieldValueDTO);
    this.setGridData(rows);
  }

  private deleteRow(row: IExtraFieldValueDTO) {
    // Delete row and update grid data
    const rows = this.rows.value;
    rows.splice(rows.indexOf(row), 1);
    // Re-sort rows to prevent holes in sort order
    rows.forEach((r, i) => (r.sort = i + 1));
    this.setGridData(rows);
  }

  private onCellValueChanged(event: CellValueChangedEvent) {
    if (event.newValue === event.oldValue) return;

    // Update form when value changes in grid
    switch (event.colDef.field) {
      case 'value':
        this.patchForm();
        break;
    }
  }

  private setGridData(rows: IExtraFieldValueDTO[], patchForm = true) {
    // Update grid data and sort rows
    if (this.grid) this.rows.next(rows.sort((a, b) => a.sort - b.sort));

    if (patchForm) this.patchForm();
  }

  private patchForm() {
    // Update form with grid data
    this.form.patchExtraFieldValues(this.rows.value);
    this.form?.markAsDirty();
    this.form?.markAsTouched();
  }

  ngOnDestroy(): void {
    this._destroy$.next();
    this._destroy$.complete();
  }
}
