import { Component, inject, Input, input, OnInit, signal } from '@angular/core';
import { DayTypesService } from '@features/time/day-types/services/day-types.service';
import { EmployeeGroupDayTypeForm } from '@features/time/employee-groups/models/eg-daytypes-weekendpay-form.model';
import { EmployeeGroupsForm } from '@features/time/employee-groups/models/employee-groups-form.model';
import { EmbeddedGridBaseDirective } from '@shared/directives/grid-base/embedded-grid-base.directive';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IEmployeeGroupDayTypeDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarEmbeddedGridConfig } from '@ui/toolbar/models/toolbar';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, take, tap } from 'rxjs';

@Component({
  selector: 'soe-eg-daytypes-weekendpay-grid',
  standalone: false,
  templateUrl:
    '../../../../../../shared/ui-components/grid/grid-wrapper/embedded-grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
})
export class EgDaytypesWeekendpayGridComponent
  extends EmbeddedGridBaseDirective<
    IEmployeeGroupDayTypeDTO,
    EmployeeGroupsForm
  >
  implements OnInit
{
  @Input({ required: true }) form!: EmployeeGroupsForm;
  noMargin = input(true);
  height = input(66);

  dayTypesService = inject(DayTypesService);

  dayTypesWeekendPay: SmallGenericType[] = [];

  override ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Employee_Groups_Edit,
      'time.employee.employeegroups.daytypes.weekendpay',
      {
        lookups: [this.loadDayTypesWeekendPay()],
      }
    );
    this.form.employeeGroupDayType.valueChanges.subscribe(v =>
      this.initRows(v)
    );
  }

  override createGridToolbar(
    config?: Partial<ToolbarEmbeddedGridConfig>
  ): void {
    super.createGridToolbar();
    this.toolbarService.createItemGroup({
      alignLeft: true,
      items: [
        this.toolbarService.createToolbarLabel('label', {
          labelKey: signal('time.employee.employeegroup.daytypes.weekendpay'),
        }),
      ],
    });
  }

  override onGridReadyToDefine(
    grid: GridComponent<IEmployeeGroupDayTypeDTO>
  ): void {
    super.onGridReadyToDefine(grid);

    this.translate
      .get(['common.daytype', 'core.permission'])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnSelect(
          'dayTypeId',
          terms['common.daytype'],
          this.dayTypesWeekendPay,
          undefined,
          {
            dropDownIdLabel: 'id',
            dropDownValueLabel: 'name',
            flex: 100,
            editable: true,
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

  private initRows(rows: IEmployeeGroupDayTypeDTO[]) {
    this.rowData.next(rows);
  }

  override addRow(): void {
    const row: IEmployeeGroupDayTypeDTO = {
      employeeGroupDayTypeId: 0,
      dayTypeId: 0,
      employeeGroupId: this.form?.value.employeeGroupId,
      isHolidaySalary: true,
    };
    super.addRow(row, this.form.employeeGroupDayType, EmployeeGroupDayTypeForm);
  }

  override deleteRow(row: any) {
    super.deleteRow(row, this.form.employeeGroupDayType);
  }

  loadDayTypesWeekendPay(): Observable<SmallGenericType[]> {
    return this.dayTypesService
      .getDayTypesByCompanyDict(true, true)
      .pipe(tap(x => (this.dayTypesWeekendPay = x)));
  }
}
