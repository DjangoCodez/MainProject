import { Component, OnInit, inject, signal } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { ITimeCodeGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { ProgressOptions } from '@shared/services/progress';
import { Perform } from '@shared/util/perform.class';
import { Dict } from '@ui/grid/services/selected-item.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take, tap } from 'rxjs/operators';
import { MaterialCodesService } from '../../services/material-codes.service';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Component({
  selector: 'soe-material-codes-grid',
  templateUrl: 'material-codes-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class MaterialCodesGridComponent
  extends GridBaseDirective<ITimeCodeGridDTO, MaterialCodesService>
  implements OnInit
{
  service = inject(MaterialCodesService);
  progressService = inject(ProgressService);
  hasDisabledSaveButton = signal(true);
  idFieldName = 'timeCodeId';
  performAction = new Perform<BackendResponse>(this.progressService);

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      Feature.Billing_Preferences_ProductSettings_MaterialCode,
      'Time.Time.TimeCodeMaterials',
      { useLegacyToolbar: true }
    );
  }

  selectedItemsChanged($event: Dict): void {
    this.hasDisabledSaveButton.set(Object.keys($event.dict).length <= 0);
  }

  override createLegacyGridToolbar(): void {
    super.createLegacyGridToolbar({
      reloadOption: {
        onClick: () => this.refreshGrid(),
      },
      saveOption: {
        onClick: () => {
          this.saveStatus();
        },
        disabled: this.hasDisabledSaveButton,
      },
    });
  }

  saveStatus(options?: ProgressOptions) {
    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service
        .updateTimeCodeState(this.grid.selectedItemsService.toDict())
        .pipe(
          tap((backendResponse: BackendResponse) => {
            if (backendResponse.success) this.refreshGrid();
          })
        ),
      undefined,
      undefined,
      options
    );
  }

  override onGridReadyToDefine(grid: GridComponent<ITimeCodeGridDTO>) {
    super.onGridReadyToDefine(grid);
    this.translate
      .get([
        'core.edit',
        'common.active',
        'common.code',
        'common.name',
        'common.description',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnActive('isActive', terms['common.active'], {
          editable: true,
          idField: this.idFieldName,
          minWidth: 50,
          maxWidth: 50,
          alignCenter: true,
        });

        this.grid.addColumnText('code', terms['common.code'], {
          flex: 1,
          enableHiding: false,
        });
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 1,
          enableHiding: false,
        });
        this.grid.addColumnText('description', terms['common.description'], {
          flex: 1,
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
}
