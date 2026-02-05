import {
  Component,
  inject,
  input,
  Input,
  OnDestroy,
  OnInit,
  signal,
} from '@angular/core';
import { IncomingDeliveriesForm } from '@features/time/incoming-deliveries/models/incoming-deliveries-form.model';
import {
  createLengthValidator,
  createMinSplitLengthValidator,
  createStartStopTimeValidator,
} from '@features/time/incoming-deliveries/models/incoming-deliveries-rows-form.model';
import { IncomingDeliveriesService } from '@features/time/incoming-deliveries/services/incoming-deliveries.service';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { ShiftTypeService } from '@shared/features/shift-type/services/shift-type.service';
import { TermCollection } from '@shared/localization/term-types';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import {
  IIncomingDeliveryRowDTO,
  IIncomingDeliveryTypeSmallDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { DateUtil } from '@shared/util/date-util';
import { GridComponent } from '@ui/grid/grid.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { CellEditingStoppedEvent } from 'ag-grid-community';
import {
  BehaviorSubject,
  Observable,
  of,
  Subject,
  take,
  takeUntil,
  tap,
} from 'rxjs';

@Component({
  selector: 'soe-incoming-deliveries-edit-rows-grid',
  templateUrl: './incoming-deliveries-edit-rows-grid.component.html',
  styleUrl: './incoming-deliveries-edit-rows-grid.component.scss',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class IncomingDeliveriesEditRowsGridComponent
  extends GridBaseDirective<IIncomingDeliveryRowDTO>
  implements OnInit, OnDestroy
{
  @Input({ required: true }) form!: IncomingDeliveriesForm;
  minLength = input<number>(0);

  private _destroy$ = new Subject<void>();

  incompingDeliveriesService = inject(IncomingDeliveriesService);
  shiftTypeService = inject(ShiftTypeService);
  messageboxService = inject(MessageboxService);

  rows = new BehaviorSubject<IIncomingDeliveryRowDTO[]>([]);

  showLedger = signal(false);

  delaySetInitialData = false;

  override ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Schedule_StaffingNeeds_IncomingDeliveries,
      'Time.Schedule.IncomingDeliveries.Rows',
      {
        skipInitialLoad: true,
        lookups: [this.loadShiftTypes(), this.loadIncomingDeliveryTypes()],
      }
    );

    // Update grid data when form changes
    this.form?.rows.valueChanges
      .pipe(takeUntil(this._destroy$))
      .subscribe(rows => {
        this.setGridData(rows, false);
      });

    this.form?.valueChanges.pipe(takeUntil(this._destroy$)).subscribe(frm => {
      // Set grid data when form is loaded on copy
      if (frm.isCopy) {
        this.form.controls.isCopy.patchValue(false);
        this.setGridData(frm.rows, false);
      }
    });
  }

  override loadTerms(): Observable<TermCollection> {
    return super
      .loadTerms([
        'core.notspecified',
        'core.warning',
        'time.schedule.incomingdelivery.validation.nbrofpackagesislowerthanallowed',
        'time.schedule.incomingdelivery.validation.lengthislowerthanallowed',
        'time.schedule.incomingdelivery.validation.minsplitlengthislowerthanallowed',
        'time.schedule.incomingdelivery.validation.startlaterthanstop',
      ])
      .pipe(
        tap(() => {
          this.addFormValidators();
        })
      );
  }

  private addFormValidators() {
    const minLengthString = DateUtil.minutesToTimeSpan(this.minLength());

    this.form.addValidators([
      createLengthValidator(
        `${this.terms['time.schedule.incomingdelivery.validation.lengthislowerthanallowed']} (${minLengthString})`,
        this.minLength()
      ),
      createMinSplitLengthValidator(
        `${this.terms['time.schedule.incomingdelivery.validation.minsplitlengthislowerthanallowed']} (${minLengthString})`,
        this.minLength()
      ),
      createStartStopTimeValidator(
        this.terms[
          'time.schedule.incomingdelivery.validation.startlaterthanstop'
        ]
      ),
    ]);
  }

  loadShiftTypes(): Observable<SmallGenericType[]> {
    return this.shiftTypeService.getShiftTypesDict(true).pipe(
      tap(shiftTypes => {
        // Rename empty to 'Not specified'
        const empty = shiftTypes.find(s => s.id === 0);
        if (empty) empty.name = this.terms['core.notspecified'];
      })
    );
  }

  loadIncomingDeliveryTypes(): Observable<IIncomingDeliveryTypeSmallDTO[]> {
    return this.incompingDeliveriesService.getIncomingDeliveryTypesSmall();
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

  override onGridReadyToDefine(grid: GridComponent<IIncomingDeliveryRowDTO>) {
    super.onGridReadyToDefine(grid);

    this.grid.api.updateGridOptions({
      onCellEditingStopped: this.onCellEditingStopped.bind(this),
    });

    this.translate
      .get([
        'common.name',
        'common.description',
        'core.delete',
        'core.warning',
        'time.schedule.incomingdelivery.row.allowoverlapping',
        'time.schedule.incomingdelivery.row.dontassignbreakleftovers',
        'time.schedule.incomingdelivery.row.length',
        'time.schedule.incomingdelivery.row.minsplitlength',
        'time.schedule.incomingdelivery.row.nbrofpackages',
        'time.schedule.incomingdelivery.row.nbrofpersons',
        'time.schedule.incomingdelivery.row.onlyoneemployee',
        'time.schedule.incomingdelivery.row.starttime',
        'time.schedule.incomingdelivery.row.stoptime',
        'time.schedule.incomingdelivery.row.totallength',
        'time.schedule.incomingdeliverytype.incomingdeliverytype',
        'time.schedule.shifttype.shifttype',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 2,
          editable: true,
        });
        this.grid.addColumnText('description', terms['common.description'], {
          flex: 2,
          editable: true,
          enableHiding: true,
        });
        this.grid.addColumnAutocomplete(
          'shiftTypeId',
          terms['time.schedule.shifttype.shifttype'],
          {
            editable: true,
            flex: 2,
            source: () => this.shiftTypeService.performShiftTypes.data || [],
            optionDisplayNameField: 'shiftTypeName',
          }
        );
        this.grid.addColumnAutocomplete(
          'incomingDeliveryTypeId',
          terms['time.schedule.incomingdeliverytype.incomingdeliverytype'],
          {
            editable: true,
            flex: 2,
            source: () =>
              this.incompingDeliveriesService.performIncomingDeliveryTypesSmall
                .data || [],
            optionIdField: 'incomingDeliveryTypeId',
            optionDisplayNameField: 'typeName',
          }
        );
        this.grid.addColumnTimeSpan(
          'incomingDeliveryTypeLength',
          terms['time.schedule.incomingdelivery.row.length'],
          {
            editable: false,
            width: 50,
          }
        );
        this.grid.addColumnNumber(
          'nbrOfPackages',
          terms['time.schedule.incomingdelivery.row.nbrofpackages'],
          { editable: true, flex: 1, decimals: 0 }
        );
        this.grid.addColumnTimeSpan(
          'totalLength',
          terms['time.schedule.incomingdelivery.row.totallength'],
          { editable: false, width: 80 }
        );
        this.grid.addColumnNumber(
          'nbrOfPersons',
          terms['time.schedule.incomingdelivery.row.nbrofpersons'],
          { editable: true, flex: 1, decimals: 0 }
        );
        this.grid.addColumnTimeSpan(
          'length',
          terms['time.schedule.incomingdelivery.row.length'],
          { editable: true, width: 50 }
        );
        this.grid.addColumnTime(
          'startTime',
          terms['time.schedule.incomingdelivery.row.starttime'],
          { editable: true, flex: 1 }
        );
        this.grid.addColumnTime(
          'stopTime',
          terms['time.schedule.incomingdelivery.row.stoptime'],
          { editable: true, flex: 1 }
        );
        this.grid.addColumnTimeSpan('minSplitLength', '', {
          tooltip: terms['time.schedule.incomingdelivery.row.minsplitlength'],
          editable: true,
          width: 60,
          iconHeaderParams: {
            iconName: 'cut',
          },
        });
        this.grid.addColumnBool('onlyOneEmployee', '', {
          tooltip: terms['time.schedule.incomingdelivery.row.onlyoneemployee'],
          editable: true,
          width: 40,
          iconHeaderParams: {
            iconName: 'user',
          },
        });
        this.grid.addColumnBool('allowOverlapping', '', {
          tooltip: terms['time.schedule.incomingdelivery.row.allowoverlapping'],
          editable: true,
          width: 40,
          iconHeaderParams: {
            iconName: 'user-group',
          },
        });
        this.grid.addColumnBool('dontAssignBreakLeftovers', '', {
          tooltip:
            terms[
              'time.schedule.incomingdelivery.row.dontassignbreakleftovers'
            ],
          editable: true,
          width: 40,
          iconHeaderParams: {
            iconName: 'user-slash',
          },
        });
        this.grid.addColumnIcon('warnings', '', {
          width: 40,
          iconName: 'exclamation-circle',
          iconClass: 'warning-color',
          enableHiding: false,
          tooltip: terms['core.warning'],
          showIcon: row => {
            return (<any>row).warnings?.length > 0;
          },
          onClick: row => {
            this.showRowWarnings(row);
          },
        });
        this.grid.addColumnIconDelete({
          tooltip: terms['core.delete'],
          onClick: row => {
            this.deleteRow(row);
          },
        });
        //this.grid.applyStartEditOnCellFocused();
        super.finalizeInitGrid();
      });
  }

  override onGridIsDefined(): void {
    if (this.delaySetInitialData) {
      this.setGridData(this.form.rows.value, false);
      this.delaySetInitialData = false;
    }
  }

  private addRow() {
    // Create new row and update grid data
    const rows = this.rows.value;
    const row = {
      nbrOfPackages: 1,
      nbrOfPersons: 1,
      length: this.minLength(),
      startTime: DateUtil.defaultDateTime(),
      stopTime: DateUtil.defaultDateTime(),
      minSplitLength: this.minLength(),
    } as IIncomingDeliveryRowDTO;

    rows.push(row);
    this.setGridData(rows);

    setTimeout(() => {
      // Need to wait for grid to be ready before starting editing
      this.grid.startEditing(rows.length - 1, 'name');
    });
  }

  private deleteRow(row: IIncomingDeliveryRowDTO) {
    // Delete row and update grid data
    const rows = this.rows.value;
    rows.splice(rows.indexOf(row), 1);
    this.setGridData(rows);
  }

  private onCellEditingStopped(event: CellEditingStoppedEvent) {
    if (!super.onCellEditingStoppedCheckIfHasChanged(event)) return;

    const field = event.colDef.field;
    if (!field) return;

    const rowsForm = this.form.rows.at(event.rowIndex ?? 0);

    // Update form when values change in the grid
    switch (field) {
      case 'incomingDeliveryTypeId':
        const type = this.getIncomingDeliveryType(event.newValue);
        rowsForm.updateIncomingDeliveryType(type);
        break;
      case 'nbrOfPackages':
        rowsForm.controls.nbrOfPackages.patchValue(event.newValue, {
          emitEvent: false,
        });
        rowsForm.updateTotalLength();
        rowsForm.updateLength();
        break;
      case 'nbrOfPersons':
        rowsForm.controls.nbrOfPersons.patchValue(event.newValue, {
          emitEvent: false,
        });
        rowsForm.updateLength();
        rowsForm.updateStopTime();
        break;
      case 'length':
        rowsForm.controls.length.patchValue(event.newValue, {
          emitEvent: false,
        });
        rowsForm.updateStopTime();
        break;
      case 'startTime':
        rowsForm.controls.startTime.patchValue(event.newValue, {
          emitEvent: false,
        });
        rowsForm.updateStopTime();
        break;
      case 'onlyOneEmployee':
      case 'allowOverlapping':
      case 'dontAssignBreakLeftovers':
        rowsForm.controls[field].patchValue(event.newValue, {
          emitEvent: false,
        });
        this.grid.api.setFocusedCell(event.rowIndex!, field);
        break;
      default:
        // All fields that are not specificly handled above
        rowsForm.controls[field].patchValue(event.newValue, {
          emitEvent: false,
        });
        break;
    }

    if (event.data)
      event.data.warnings = rowsForm.validateRow(this.minLength());

    // Since we are not emitting the changes, the grid will not update
    // We need to refresh the warnings icon manually
    this.grid.api.refreshCells({
      rowNodes: [event.node],
      columns: ['warnings'],
    });

    this.setDirty();
  }

  private getIncomingDeliveryType(
    incomingDeliveryTypeId: number
  ): IIncomingDeliveryTypeSmallDTO | undefined {
    return this.incompingDeliveriesService.performIncomingDeliveryTypesSmall.data?.find(
      i => i.incomingDeliveryTypeId === incomingDeliveryTypeId
    );
  }

  validateRows() {
    for (let i = 0; i < this.form.rows.length; i++) {
      const rowsForm = this.form.rows.at(i);
      if (rowsForm)
        rowsForm.value.warnings = rowsForm.validateRow(this.minLength());
    }
  }

  private showRowWarnings(row: IIncomingDeliveryRowDTO) {
    const warnings: string[] = (<any>row).warnings;
    if (warnings.length > 0) {
      this.translate
        .get(warnings)
        .pipe(take(1))
        .subscribe(terms => {
          this.messageboxService.warning(
            this.terms['core.warning'],
            warnings.map(w => terms[w]).join('<br>'),
            { buttons: 'ok' }
          );
        });
    }
  }

  private setGridData(rows: IIncomingDeliveryRowDTO[], patchForm = true) {
    if (!this.gridIsDefined) {
      this.delaySetInitialData = true;
      return;
    }

    rows.forEach(row => {
      if (row.incomingDeliveryTypeId && !row.incomingDeliveryTypeDTO) {
        row.incomingDeliveryTypeDTO = <any>(
          this.getIncomingDeliveryType(row.incomingDeliveryTypeId)
        );
      }
    });

    // Update grid
    this.validateRows();
    if (this.grid) this.rows.next(rows);

    if (patchForm) this.patchForm();
  }

  private patchForm() {
    // Update form with grid data
    this.form.patchRows(this.rows.value);
    this.setDirty();
  }

  private setDirty() {
    this.form?.markAsDirty();
    this.form?.markAsTouched();
  }

  ngOnDestroy(): void {
    this._destroy$.next();
    this._destroy$.complete();
  }
}
