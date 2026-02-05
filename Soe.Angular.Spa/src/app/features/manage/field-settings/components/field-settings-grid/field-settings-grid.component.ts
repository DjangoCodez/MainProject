import { Component, inject, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import {
  Feature,
  SoeFieldSettingType,
} from '@shared/models/generated-interfaces/Enumerations';
import { IFieldSettingGridDTO } from '@shared/models/generated-interfaces/FieldSettingDTO';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, take } from 'rxjs';
import { FieldSettingsService } from '../../services/field-settings.service';

@Component({
  selector: 'soe-field-settings-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class FieldSettingsGridComponent
  extends GridBaseDirective<IFieldSettingGridDTO>
  implements OnInit
{
  service = inject(FieldSettingsService);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Manage_Preferences_FieldSettings,
      'Economy.Accounting.CompanyGroup.CompanyAdministration'
    );
  }

  override onGridReadyToDefine(grid: GridComponent<IFieldSettingGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'core.function',
        'common.field',
        'manage.preferences.fieldsettings.rolesetting',
        'manage.preferences.fieldsettings.companysetting',
        'core.edit',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('formName', terms['core.function'], {
          flex: 25,
        });
        this.grid.addColumnText('fieldName', terms['common.field'], {
          flex: 25,
          enableHiding: true,
        });
        this.grid.addColumnText(
          'companySettingsSummary',
          terms['manage.preferences.fieldsettings.companysetting'],
          { flex: 25, enableHiding: true }
        );
        this.grid.addColumnText(
          'roleSettingsSummary',
          terms['manage.preferences.fieldsettings.rolesetting'],
          { flex: 25, enableHiding: true }
        );
        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            this.edit(row);
          },
        });
        super.finalizeInitGrid();
      });
  }

  override loadData(
    id?: number | undefined
  ): Observable<IFieldSettingGridDTO[]> {
    const fieldSettingsType = SoeConfigUtil.fieldSettingsType
      ? SoeConfigUtil.fieldSettingsType
      : SoeFieldSettingType.Mobile;

    return this.performLoadData.load$(
      this.service.getGrid(undefined, { type: fieldSettingsType })
    );
  }
}
