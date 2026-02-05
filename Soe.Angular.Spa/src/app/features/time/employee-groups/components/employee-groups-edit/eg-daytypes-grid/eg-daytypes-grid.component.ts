import { Component, inject, input, Input, OnInit, signal } from '@angular/core';
import { DayTypesService } from '@features/time/day-types/services/day-types.service';
import { EmployeeGroupsForm } from '@features/time/employee-groups/models/employee-groups-form.model';
import { EmbeddedGridBaseDirective } from '@shared/directives/grid-base/embedded-grid-base.directive';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IdForm, IId } from '@shared/models/id.form.model';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarEmbeddedGridConfig } from '@ui/toolbar/models/toolbar';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, take, tap } from 'rxjs';

@Component({
  selector: 'soe-eg-daytypes-grid',
  standalone: false,
  templateUrl:
    '../../../../../../shared/ui-components/grid/grid-wrapper/embedded-grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
})
export class EgDaytypesGridComponent
  extends EmbeddedGridBaseDirective<IId, EmployeeGroupsForm>
  implements OnInit
{
  @Input({ required: true }) form!: EmployeeGroupsForm;
  noMargin = input(true);
  height = input(66);

  gridToolbarService = inject(ToolbarService);

  dayTypesService = inject(DayTypesService);

  dayTypes: SmallGenericType[] = [];

  override ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Employee_Groups_Edit,
      'time.employee.employeegroups.daytypes',
      {
        lookups: [this.loadDayTypes()],
      }
    );
    this.form.dayTypeIds.valueChanges.subscribe(v => this.initRows(v));
  }

  override createGridToolbar(
    config?: Partial<ToolbarEmbeddedGridConfig>
  ): void {
    super.createGridToolbar();
    this.toolbarService.createItemGroup({
      alignLeft: true,
      items: [
        this.toolbarService.createToolbarLabel('label', {
          labelKey: signal('time.employee.employeegroup.daytypes.scheduled'),
        }),
      ],
    });
  }

  override onGridReadyToDefine(grid: GridComponent<IId>): void {
    super.onGridReadyToDefine(grid);

    this.translate
      .get(['common.daytype', 'core.delete', 'core.permission'])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnSelect(
          'id',
          terms['common.daytype'],
          this.dayTypes || [],
          undefined,
          {
            dropDownIdLabel: 'id',
            dropDownValueLabel: 'name',
            flex: 100,
            editable: true,
            // dynamicSelectOptions: () => { TODO: Doesn't work.
            //   return this.setSelectDropdownValues(
            //     this.form.dayTypeIds.value,
            //     this.dayTypes
            //   );
            // },
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

  private initRows(rows: IId[]) {
    this.rowData.next(rows);
  }

  override addRow(): void {
    const row: IId = {
      id: 0,
    };
    super.addRow(row, this.form.dayTypeIds, IdForm);
  }

  override deleteRow(row: any) {
    super.deleteRow(row, this.form.dayTypeIds);
  }

  private setSelectDropdownValues(
    selectedValues: SmallGenericType[],
    originalValues: SmallGenericType[]
  ): SmallGenericType[] {
    const newValues: SmallGenericType[] = [];
    originalValues.forEach(og => {
      if (!selectedValues.some(sv => sv.id === og.id)) {
        newValues.unshift(og);
        console.log(og);
        console.log(newValues);
      }
    });
    return newValues;
  }

  loadDayTypes(): Observable<SmallGenericType[]> {
    return this.dayTypesService
      .getDayTypesByCompanyDict(true, false)
      .pipe(tap(x => (this.dayTypes = x)));
  }
}
