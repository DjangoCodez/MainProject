import {
  AfterViewInit,
  Component,
  inject,
  Input,
  OnInit,
  signal,
} from '@angular/core';
import { EmbeddedGridBaseDirective } from '@shared/directives/grid-base/embedded-grid-base.directive';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarEmbeddedGridConfig } from '@ui/toolbar/models/toolbar';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import {
  CellEditingStartedEvent,
  CellEditingStoppedEvent,
} from 'ag-grid-community';
import { Observable, of, Subject, takeUntil } from 'rxjs';
import { EditableGridTestDataForm } from '../../models/editable-grid-test-data-form.model';
import { UiComponentsTestForm } from '../../models/ui-components-test-form.model';

export class EditableGridTestDataDTO {
  id: number;
  city: string;
  name2: string;
  timeFrom: Date;
  timeTo: Date;
  length: number;
  itemId: number;
  typeId: number;
  isDefault: boolean;
  date: Date;
  number: number;

  constructor() {
    this.id = 0;
    this.city = '';
    this.name2 = '';
    this.timeFrom = new Date();
    this.timeTo = new Date();
    this.length = 0;
    this.itemId = 0;
    this.typeId = 0;
    this.isDefault = false;
    this.date = new Date();
    this.number = 0;
  }
}

@Component({
  selector: 'soe-editable-grid-test',
  templateUrl:
    '../../../../../../shared/ui-components/grid/grid-wrapper/embedded-grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class EditableGridTestComponent
  extends EmbeddedGridBaseDirective<
    EditableGridTestDataDTO,
    UiComponentsTestForm
  >
  implements OnInit, AfterViewInit
{
  @Input({ required: true }) form!: UiComponentsTestForm;

  items: SmallGenericType[] = [];
  typeItems: SmallGenericType[] = [];

  private _destroy$ = new Subject<void>();

  readonly flowHandler = inject(FlowHandlerService);
  messageboxService = inject(MessageboxService);
  delaySetInitialData = true;

  constructor() {
    super();
  }

  ngAfterViewInit() {
    // this.setupGrid(this.grid);
  }

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(Feature.None, 'Manage.System.EditableGrid');

    this.form?.editableGridRows.valueChanges
      .pipe(takeUntil(this._destroy$))
      .subscribe(rows => {
        this.grid.refreshCells();
      });

    this.loadItems();
    this.loadTypeItems();
  }

  private setGridData(rows: EditableGridTestDataDTO[], patchForm = true) {
    if (this.grid) this.rowData.next(rows);
    if (patchForm) this.patchForm();
  }

  override addRow(): void {
    console.log('Add new row in editable grid');
    const newRow = new EditableGridTestDataDTO();
    super.addRow(newRow, this.form.editableGridRows, EditableGridTestDataForm);
  }

  private patchForm() {
    // Update form with grid data
    this.form.patchRows(this.rowData.value);
    //this.setDirty();
  }

  loadItems() {
    this.items = [
      { id: 1, name: 'Stockholm' },
      { id: 2, name: 'Söderhamn' },
    ];
  }

  loadTypeItems() {
    this.typeItems = [
      { id: 1, name: 'Type 1' },
      { id: 2, name: 'Type 2' },
      { id: 3, name: 'Type 3' },
      { id: 4, name: 'Type 4' },
      { id: 5, name: 'Type 5' },
    ];
  }

  getData(): Observable<EditableGridTestDataDTO[]> {
    // testdata
    // return of([
    //   {
    //     name: 'Stockholm',
    //     timeFrom: new Date(),
    //     timeTo: new Date(),
    //     length: 0,
    //     itemId: 1,
    //     typeId: 2,
    //     isDefault: false,
    //   } as EditableGridTestDataDTO,
    //   {
    //     name: 'Söderhamn',
    //     timeFrom: new Date(),
    //     timeTo: new Date(),
    //     length: 0,
    //     itemId: 2,
    //     typeId: 1,
    //     isDefault: true,
    //   } as EditableGridTestDataDTO,
    // ]);

    return of(this.form.editableGridRows.value);
  }

  override createGridToolbar(
    config?: Partial<ToolbarEmbeddedGridConfig>
  ): void {
    super.createGridToolbar();
    this.toolbarService.createItemGroup({
      alignLeft: true,
      items: [
        this.toolbarService.createToolbarLabel('label', {
          labelKey: signal('Editable grid'),
        }),
      ],
    });
  }

  override onGridReadyToDefine(grid: GridComponent<EditableGridTestDataDTO>) {
    super.onGridReadyToDefine(grid);

    this.grid = grid;

    // set options
    this.embeddedGridOptions = {
      newRowStartEditField: 'city',
    };

    // set testdata
    this.getData().subscribe(rows => {
      this.setGridData(rows, false);
    });

    this.grid.enableRowSelection(undefined, true);
    this.grid.addColumnText('city', 'City', {
      flex: 1,
      editable: true,
      showSetFilter: true,
    });
    this.grid.addColumnTime('timeFrom', 'Time from', {
      flex: 1,
      editable: true,
    });
    this.grid.addColumnTime('timeTo', 'Time to', {
      flex: 1,
      editable: true,
    });
    this.grid.addColumnTimeSpan('length', 'Length', {
      flex: 1,
      editable: true,
    });
    this.grid.addColumnSelect('itemId', 'Item', this.items, null, {
      flex: 1,
      editable: true,
    });
    this.grid.addColumnAutocomplete<SmallGenericType>('typeId', 'Type', {
      source: _ => this.typeItems ?? [],
      editable: true,
      optionIdField: 'id',
      optionNameField: 'name',
      //optionDisplayNameField: 'name',
    });
    this.grid.addColumnBool('isDefault', 'Default', {
      flex: 1,
      editable: true,
    });
    this.grid.addColumnText('name2', 'City2', {
      flex: 1,
      editable: true,
      showSetFilter: true,
    });
    this.grid.addColumnDate('date', 'Date', {
      flex: 1,
      editable: true,
    });
    this.grid.addColumnNumber('number', 'Number', {
      flex: 1,
      editable: true,
    });

    super.finalizeInitGrid({
      termTotal: 'Total',
      termFiltered: 'Filtered',
      tooltip: 'Nr of items',
    });

    this.grid.options.stopEditingWhenCellsLoseFocus = true;
  }

  override onCellEditingStopped(event: CellEditingStoppedEvent) {
    super.onCellEditingStopped(event);
    if (!super.onCellEditingStoppedCheckIfHasChanged(event)) return;

    // console.log('onCellEditingStopped', event);
    const field = event.colDef.field;
    if (!field) return;

    const rowsForm = <EditableGridTestDataForm>(
      this.form.editableGridRows.at(event.rowIndex ?? 0)
    );

    // Update form when values change in the grid
    switch (field) {
      case 'length':
        rowsForm.controls.length.patchValue(event.newValue, {
          emitEvent: true,
        });
        rowsForm.updateStopTime();
        this.updateStopTime(event.data, rowsForm);
        break;
      case 'timeFrom':
        rowsForm.controls.timeFrom.patchValue(event.newValue, {
          emitEvent: true,
        });
        rowsForm.updateStopTime();
        this.updateStopTime(event.data, rowsForm);
        break;
      case 'name':
        rowsForm.controls.name.patchValue(event.newValue, {
          emitEvent: true,
        });
        rowsForm.controls.name2.patchValue(event.newValue, {
          emitEvent: true,
        });
        const rowNode = this.grid.api.getRowNode(event.data.AG_NODE_ID)!;
        rowNode.setDataValue('name2', event.newValue);
        break;
      default:
        // All fields that are not specificly handled above
        rowsForm.controls[field].patchValue(event.newValue, {
          emitEvent: true,
        });
        break;
    }

    // Since we are not emitting the changes, the grid will not update
    // We need to refresh the warnings icon manually
    this.grid.api.refreshCells({
      rowNodes: [event.node],
    });
  }

  override onCellEditingStarted(event: CellEditingStartedEvent) {
    super.onCellEditingStarted(event);
    const field = event.colDef.field;
    if (!field) return;

    const rowsForm = <EditableGridTestDataForm>(
      this.form.editableGridRows.at(event.rowIndex ?? 0)
    );

    // force initial value from calculation
    if (field == 'timeTo') {
      // prevent infinite loop
      if (event.node.data._initialized) {
        event.node.data._initialized = false;
        return;
      }
      event.node.data._initialized = true;

      const newValue = (rowsForm.controls.timeFrom.value as Date).addMinutes(
        rowsForm.controls.length.value
      );

      rowsForm.controls.timeTo.patchValue(newValue, {
        emitEvent: true,
      });

      event.api.stopEditing();
      event.node.setDataValue(field, newValue);
      event.api.startEditingCell({
        rowIndex: event.rowIndex ?? 0,
        colKey: field,
      });
    }
  }

  updateStopTime(data: any, rowForm: EditableGridTestDataForm) {
    const newValue = (rowForm.controls.timeFrom.value as Date).addMinutes(
      rowForm.controls.length.value
    );
    const rowNode = this.grid.api.getRowNode(data.AG_NODE_ID)!;
    rowNode.setDataValue('timeTo', newValue);
  }
}
