import { Component, inject, Input, input, OnInit, signal } from '@angular/core';
import { EmployeeGroupsTimeDeviationCauseTimeCodeForm } from '@features/time/employee-groups/models/eg-deviation-cause-time-code-form.model';
import { EmployeeGroupsForm } from '@features/time/employee-groups/models/employee-groups-form.model';
import { EmployeeGroupsService } from '@features/time/employee-groups/services/employee-groups.service';
import { EmbeddedGridBaseDirective } from '@shared/directives/grid-base/embedded-grid-base.directive';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IEmployeeGroupTimeDeviationCauseTimeCodeDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarEmbeddedGridConfig } from '@ui/toolbar/models/toolbar';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, take } from 'rxjs';

@Component({
  selector: 'soe-eg-deviation-cause-time-code-grid',
  templateUrl:
    '../../../../../../shared/ui-components/grid/grid-wrapper/embedded-grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class EmployeeGroupsTimeDeviationCauseTimeCodeGridComponent
  extends EmbeddedGridBaseDirective<
    IEmployeeGroupTimeDeviationCauseTimeCodeDTO,
    EmployeeGroupsForm
  >
  implements OnInit
{
  @Input({ required: true }) form!: EmployeeGroupsForm;
  noMargin = input(true);
  height = input(66);

  employeeGroupService = inject(EmployeeGroupsService);

  override ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Employee_Groups_Edit,
      'time.employee.employeegroups.deviation-cause-time-code'
    );

    this.form.valueChanges.subscribe(v =>
      this.initRows(v.employeeGroupTimeDeviationCauseTimeCode)
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
          labelKey: signal(
            'time.employee.employeegroup.deviationcauses.timecode.linked'
          ),
        }),
      ],
    });
  }

  override onGridReadyToDefine(
    grid: GridComponent<IEmployeeGroupTimeDeviationCauseTimeCodeDTO>
  ): void {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'time.time.timedeviationcause.timedeviationcause',
        'core.permission',
        'time.time.timecode.timecode',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnSelect(
          'timeDeviationCauseId',
          terms['time.time.timedeviationcause.timedeviationcause'],
          this.employeeGroupService.performDeviationCauses.data || [],
          undefined,
          {
            dropDownIdLabel: 'id',
            dropDownValueLabel: 'name',
            flex: 50,
            editable: true,
          }
        );
        this.grid.addColumnSelect(
          'timeCodeId',
          terms['time.time.timecode.timecode'],
          this.employeeGroupService.performTimeCodes.data || [],
          undefined,
          {
            dropDownIdLabel: 'id',
            dropDownValueLabel: 'name',
            flex: 50,
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

  private initRows(rows: IEmployeeGroupTimeDeviationCauseTimeCodeDTO[]) {
    this.rowData.next(rows);
  }

  override addRow(): void {
    const row: Partial<IEmployeeGroupTimeDeviationCauseTimeCodeDTO> = {
      employeeGroupId: this.form?.value.employeeGroupId,
      timeDeviationCauseId: 0,
      timeCodeId: 0,
    };
    super.addRow(
      row as IEmployeeGroupTimeDeviationCauseTimeCodeDTO,
      this.form.employeeGroupTimeDeviationCauseTimeCode,
      EmployeeGroupsTimeDeviationCauseTimeCodeForm
    );
  }

  override deleteRow(row: any) {
    super.deleteRow(row, this.form.employeeGroupTimeDeviationCauseTimeCode);
  }
}
