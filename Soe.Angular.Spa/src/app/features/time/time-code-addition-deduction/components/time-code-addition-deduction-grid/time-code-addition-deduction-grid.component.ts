import { Component, inject, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { ITimeCodeGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { TimeCodeAdditionDeductionService } from '../../services/time-code-addition-deduction.service';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { GridComponent } from '@ui/grid/grid.component';
import { take, tap } from 'rxjs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Perform } from '@shared/util/perform.class';
import { CrudActionTypeEnum } from '@shared/enums';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Component({
  standalone: false,
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
})
export class TimeCodeAdditionDeductionGridComponent
  extends GridBaseDirective<ITimeCodeGridDTO, TimeCodeAdditionDeductionService>
  implements OnInit
{
  service = inject(TimeCodeAdditionDeductionService);
  performAction = new Perform<any>(this.progressService);

  ngOnInit() {
    super.ngOnInit();
    this.startFlow(
      Feature.Time_Preferences_TimeSettings_TimeCodeAdditionDeduction,
      'Time.Time.TimeCodeAdditionDeductions'
    );
  }

  override createGridToolbar(): void {
    super.createGridToolbar({
      useDefaltSaveOption: this.flowHandler.modifyPermission(),
    });
  }

  override onGridReadyToDefine(grid: GridComponent<ITimeCodeGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.active',
        'common.code',
        'common.name',
        'common.description',
        'core.edit',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnActive('state', terms['common.active'], {
          idField: 'timeCodeId',
          resizable: false,
          editable: this.flowHandler.modifyPermission(),
        });
        this.grid.addColumnText('code', terms['common.code'], {
          flex: 20,
          enableHiding: true,
        });
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 40,
        });
        this.grid.addColumnText('description', terms['common.description'], {
          flex: 40,
          enableHiding: true,
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
        .updateTimeCodeState(this.grid.selectedItemsService.toDict())
        .pipe(
          tap((response: BackendResponse) => {
            if (response.success) {
              this.refreshGrid();
            }
          })
        )
    );
  }
}
