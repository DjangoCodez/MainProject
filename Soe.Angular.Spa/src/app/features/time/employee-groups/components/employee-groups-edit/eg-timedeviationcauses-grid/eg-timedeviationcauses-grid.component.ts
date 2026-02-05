import { Component, inject, Input, input, OnInit, signal } from '@angular/core';
import { EmployeeGroupTimeDeviationCauseForm } from '@features/time/employee-groups/models/eg-timedeviationcauses-form.model';
import { EmployeeGroupsForm } from '@features/time/employee-groups/models/employee-groups-form.model';
import { EmployeeGroupsService } from '@features/time/employee-groups/services/employee-groups.service';
import { EmbeddedGridBaseDirective } from '@shared/directives/grid-base/embedded-grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IEmployeeGroupTimeDeviationCauseDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarEmbeddedGridConfig } from '@ui/toolbar/models/toolbar';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take } from 'rxjs';

@Component({
  selector: 'soe-eg-timedeviationcauses-grid',
  standalone: false,
  templateUrl:
    '../../../../../../shared/ui-components/grid/grid-wrapper/embedded-grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
})
export class EgTimedeviationcausesGridComponent
  extends EmbeddedGridBaseDirective<
    IEmployeeGroupTimeDeviationCauseDTO,
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
      'time.employee.employeegroups.deviationcauses.grid'
    );
    this.form.timeDeviationCauses.valueChanges.subscribe(v => this.initRows(v));
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
            'time.employee.employeegroup.deviationcauses.linked'
          ),
        }),
      ],
    });
  }

  override onGridReadyToDefine(
    grid: GridComponent<IEmployeeGroupTimeDeviationCauseDTO>
  ): void {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'time.time.timedeviationcause.timedeviationcause',
        'core.permission',
        'time.employee.employeegroup.deviationcauses.showinterminal',
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
            flex: 80,
            editable: true,
          }
        );
        this.grid.addColumnBool(
          'useInTimeTerminal',
          terms['time.employee.employeegroup.deviationcauses.showinterminal'],
          {
            flex: 20,
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

  private initRows(rows: IEmployeeGroupTimeDeviationCauseDTO[]) {
    this.rowData.next(rows);
  }

  override addRow(): void {
    const row: IEmployeeGroupTimeDeviationCauseDTO = {
      employeeGroupTimeDeviationCauseId: 0,
      employeeGroupId: this.form?.value.employeeGroupId,
      timeDeviationCauseId: 0,
      useInTimeTerminal: false,
    };
    super.addRow(
      row,
      this.form.timeDeviationCauses,
      EmployeeGroupTimeDeviationCauseForm
    );
  }

  override deleteRow(row: any) {
    super.deleteRow(row, this.form.timeDeviationCauses);
  }
}
