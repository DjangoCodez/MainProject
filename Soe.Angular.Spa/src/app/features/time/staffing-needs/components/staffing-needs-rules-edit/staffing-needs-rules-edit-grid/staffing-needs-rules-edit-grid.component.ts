import { Component, Input, OnInit, inject } from '@angular/core';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import {
  IDayTypeAndWeekdayDTO,
  IStaffingNeedsRuleRowDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take, tap } from 'rxjs';
import { StaffingNeedsRulesForm } from '@features/time/staffing-needs/models/staffing-needs-rules-form.model';
import { StaffingNeedsRulesRowForm } from '@features/time/staffing-needs/models/staffing-needs-rules-row-form.model';
import { NumberUtil } from '@shared/util/number-util';
import { TimeService } from '@src/app/features/time/services/time.service';
import { CellKeyDownEvent, CellValueChangedEvent } from 'ag-grid-community';
import { EmbeddedGridBaseDirective } from '@shared/directives/grid-base/embedded-grid-base.directive';

interface IDayTypeSlim {
  dayId: number;
  dayTypeId?: number;
  weekdayNr?: number;
  name: string;
}

@Component({
  selector: 'soe-staffing-needs-rules-edit-grid',
  templateUrl:
    '../../../../../../shared/ui-components/grid/grid-wrapper/embedded-grid-wrapper-template.html', //'./staffing-needs-rules-edit-grid.component.html',
  styleUrls: ['./staffing-needs-rules-edit-grid.component.scss'],
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class StaffingNeedsRulesEditGridComponent
  extends EmbeddedGridBaseDirective<
    IStaffingNeedsRuleRowDTO,
    StaffingNeedsRulesForm
  >
  implements OnInit
{
  @Input({ required: true }) form!: StaffingNeedsRulesForm;

  private readonly timeService = inject(TimeService);
  gridToolbarService = inject(ToolbarService);
  dayTypes: IDayTypeSlim[] = [];

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Preferences_NeedsSettings_Rules_Edit,
      'Time.Schedule.StaffingNeedsRules',
      {
        lookups: [this.loadDayTypes()],
      }
    );
    this.form.valueChanges.subscribe(v => this.initRows(v.rows));
  }

  onCellValueChanged(evt: CellValueChangedEvent): void {
    //this.form?.patchRows(this.subData.value);
    this.form?.markAsDirty();
  }

  onCellKeyDown(event: CellKeyDownEvent) {
    this.form?.markAsDirty();
  }

  override onGridReadyToDefine(grid: GridComponent<IStaffingNeedsRuleRowDTO>) {
    super.onGridReadyToDefine(grid);

    this.grid = grid;
    this.grid.setNbrOfRowsToShow(5, 10);
    this.translate
      .get(['common.day', 'common.value', 'common.delete'])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnModified('isModified');
        this.grid.addColumnNumber('sort', '', {
          width: 50,
          rowDragable: true,
        });
        this.grid.addColumnSelect(
          'dayId',
          terms['common.day'],
          this.dayTypes,
          this.subGridDayTypeChanged.bind(this),
          { dropDownIdLabel: 'dayId', dropDownValueLabel: 'name', flex: 50 }
        );
        this.grid.addColumnNumber('value', terms['common.value'], {
          flex: 15,
          decimals: 2,
        });
        this.grid.addColumnIconDelete({
          tooltip: terms['common.delete'],
          onClick: row => this.deleteRow(row),
        });

        this.grid.context.suppressGridMenu = true;
        this.grid.context.suppressFiltering = true;
        this.grid.setRowSelection('singleRow');

        this.grid.columns.forEach(col => {
          if (col.field !== 'sort') {
            col.editable = true;
            col.sortable = false;
          } else {
            col.onCellClicked = () => {
              this.grid.selectRowOnCellClicked(col.field);
            };
          }
          col.floatingFilter = false;
        });
        this.grid.applyDragOptions({
          rowDragFinishedSortIndexNrFieldName: 'sort',
          rowDragFinishedCallback: (row: any) => {
            this.form?.markAsDirty();
          },
        });
        super.finalizeInitGrid();
      });
  }

  override createGridToolbar(): void {
    super.createGridToolbar({
      showSorting: true,
      sortingField: 'sort',
    });
  }

  override addRow(): void {
    const maxCount = NumberUtil.max(this.rowData.value, 'sort');
    const row: IStaffingNeedsRuleRowDTO = {
      sort: maxCount + 1,
      dayId: 0,
      dayTypeId: 0,
      weekday: 0,
      dayName: '',
      value: 0,
      staffingNeedsRuleRowId: 0,
      staffingNeedsRuleId: this.form?.value.staffingNeedsRuleId,
    };
    super.addRow(row, this.form.rows, StaffingNeedsRulesRowForm);
  }

  override deleteRow(row: any) {
    super.deleteRow(row, this.form.rows);
  }

  private initRows(rows: IStaffingNeedsRuleRowDTO[]) {
    const subData = rows.map(x => {
      x['dayName'] = '';
      const searchKey = x.dayTypeId ? 'dayTypeId' : 'weekday';
      const matchingKey = x.dayTypeId ? 'dayTypeId' : 'weekdayNr';
      const obj = this.dayTypes.find(d => d[matchingKey] === x[searchKey]);
      if (obj) {
        x['dayId'] = obj['dayId'] || 0;
        x['dayName'] = obj['name'] || '';
        x['dayTypeId'] = obj['dayTypeId'] || x['dayTypeId'];
        x['weekday'] = obj['weekdayNr'] || x['weekday'];
      }
      return x;
    });

    this.rowData.next(subData);
  }

  subGridDayTypeChanged(row: any) {
    const rowData = row.data;
    if (!rowData) return;
    const obj = this.dayTypes.find((d: any) => {
      return d.dayId == row.data.dayId;
    });
    if (!obj) return;
    rowData.dayTypeId = obj.dayTypeId || rowData.dayTypeId;
    rowData.weekday = obj.weekdayNr || rowData.weekday;
    rowData.dayName = obj.name || rowData.dayName;
  }

  private loadDayTypes() {
    return this.timeService.getDayTypesAndWeekdays().pipe(
      tap(dayTypes => {
        this.dayTypes = dayTypes.map(
          (dayType: IDayTypeAndWeekdayDTO, index: number) => {
            return {
              dayId: index + 1,
              dayTypeId: dayType.dayTypeId,
              weekdayNr: dayType.weekdayNr,
              name: dayType.name,
            };
          }
        );
        const empty = { dayId: 0, name: '' };
        this.dayTypes.splice(0, 0, empty);
      })
    );
  }
}
