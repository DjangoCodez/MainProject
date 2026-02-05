import { Component, inject, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IFollowUpTypeGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress';
import { Perform } from '@shared/util/perform.class';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take, tap } from 'rxjs/operators';
import { EmployeeFollowupTypesService } from '../../services/employee-followup-types.service';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Component({
  selector: 'soe-employee-followup-types-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class EmployeeFollowupTypesGridComponent
  extends GridBaseDirective<IFollowUpTypeGridDTO, EmployeeFollowupTypesService>
  implements OnInit
{
  service = inject(EmployeeFollowupTypesService);
  progressService = inject(ProgressService);
  performAction = new Perform<BackendResponse>(this.progressService);

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Employee_FollowUpTypes,
      'time.employee.followuptype.followuptype'
    );
  }

  override createGridToolbar(): void {
    return super.createGridToolbar({
      useDefaltSaveOption: true,
    });
  }

  override onGridReadyToDefine(grid: GridComponent<IFollowUpTypeGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get(['common.active', 'common.name', 'core.edit'])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnActive('state', terms['common.active'], {
          editable: true,
          idField: 'followUpTypeId',
        });
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 60,
        });
        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            this.edit(row);
          },
        });
        super.finalizeInitGrid();
      });
  }

  override saveStatus(): void {
    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service
        .updateFollowUpTypesState(this.grid.selectedItemsService.toDict())
        .pipe(
          tap((response: BackendResponse) => {
            if (response.success) this.refreshGrid();
          })
        )
    );
  }
}
