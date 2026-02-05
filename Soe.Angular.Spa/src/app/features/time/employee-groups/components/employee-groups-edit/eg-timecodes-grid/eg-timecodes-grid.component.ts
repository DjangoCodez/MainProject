import { Component, inject, Input, input, OnInit, signal } from '@angular/core';
import { EmployeeGroupsForm } from '@features/time/employee-groups/models/employee-groups-form.model';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarEmbeddedGridConfig } from '@ui/toolbar/models/toolbar';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take } from 'rxjs';
import { EmployeeGroupsService } from '@features/time/employee-groups/services/employee-groups.service';
import { IdForm, IId } from '@shared/models/id.form.model';
import { EmbeddedGridBaseDirective } from '@shared/directives/grid-base/embedded-grid-base.directive';

@Component({
  selector: 'soe-eg-timecodes-grid',
  standalone: false,
  templateUrl:
    '../../../../../../shared/ui-components/grid/grid-wrapper/embedded-grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
})
export class EgTimecodesGridComponent
  extends EmbeddedGridBaseDirective<IId, EmployeeGroupsForm>
  implements OnInit
{
  @Input({ required: true }) form!: EmployeeGroupsForm;
  noMargin = input(true);
  height = input(66);

  employeeGroupsService = inject(EmployeeGroupsService);

  override ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Employee_Groups_Edit,
      'time.employee.employeegroups.timecodes.grid'
    );
    this.form.timeCodeIds.valueChanges.subscribe(v => this.initRows(v));
  }

  override createGridToolbar(
    config?: Partial<ToolbarEmbeddedGridConfig>
  ): void {
    super.createGridToolbar();
    this.toolbarService.createItemGroup({
      alignLeft: true,
      items: [
        this.toolbarService.createToolbarLabel('label', {
          labelKey: signal('time.employee.employeegroup.timecodes.linked'),
        }),
      ],
    });
  }

  override onGridReadyToDefine(grid: GridComponent<IId>): void {
    super.onGridReadyToDefine(grid);

    this.translate
      .get(['time.time.timecode.timecode', 'core.permission'])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnSelect(
          'id',
          terms['time.time.timecode.timecode'],
          this.employeeGroupsService.performTimeCodes.data || [],
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
  private initRows(rows: IId[]) {
    this.rowData.next(rows);
  }

  override addRow(): void {
    const row: IId = {
      id: 0,
    };
    super.addRow(row, this.form.timeCodeIds, IdForm);
  }

  override deleteRow(row: any) {
    super.deleteRow(row, this.form.timeCodeIds);
  }
}
