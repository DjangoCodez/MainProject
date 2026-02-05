import { Component, OnInit, inject } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IEndReasonGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take, tap } from 'rxjs/operators';
import { EndReasonsService } from '../../services/end-reasons.service';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Component({
  selector: 'soe-end-reasons-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class EndReasonsGridComponent
  extends GridBaseDirective<IEndReasonGridDTO, EndReasonsService>
  implements OnInit
{
  service = inject(EndReasonsService);
  progressService = inject(ProgressService);
  performAction = new Perform<any>(this.progressService);

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Employee_EndReasons,
      'Time.Employee.EndReasons'
    );
  }

  override createGridToolbar(): void {
    return super.createGridToolbar({
      useDefaltSaveOption: true,
    });
  }

  override onGridReadyToDefine(grid: GridComponent<IEndReasonGridDTO>) {
    super.onGridReadyToDefine(grid);
    this.translate
      .get(['common.name', 'common.active', 'core.edit'])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnActive('isActive', terms['common.active'], {
          showCheckbox: row => {
            return !row.systemEndReson;
          },
          editable: true,
          idField: 'endReasonId',
        });
        this.grid.addColumnText('name', terms['common.name'], { flex: 100 });
        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          showIcon: row => {
            return !row.systemEndReson;
          },
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
        .updateEndReasonsState(this.grid.selectedItemsService.toDict())
        .pipe(
          tap((response: BackendResponse) => {
            if (response.success) this.refreshGrid();
          })
        )
    );
  }

  override edit(row: IEndReasonGridDTO) {
    if (row.systemEndReson) {
      return;
    }
    super.edit(row);
  }
}
