import { Component, OnInit, inject } from '@angular/core';
import { GridComponent } from '@shared/ui-components/grid/grid.component';
import { take, tap } from 'rxjs/operators';
import {
  CompanySettingType,
  Feature,
} from '@shared/models/generated-interfaces/Enumerations';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { SettingsUtil } from '@shared/util/settings-util';
import { IExportDefinitionGridDTO } from '@shared/models/generated-interfaces/ExportDTO';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { ExportStandardDefinitionsService } from '../../services/export-standard-definitions.service';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';

@Component({
  selector: 'soe-export-standard-definitions-grid',
  templateUrl: './export-standard-definitions-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ExportStandardDefinitionsGridComponent
  extends GridBaseDirective<
    IExportDefinitionGridDTO,
    ExportStandardDefinitionsService
  >
  implements OnInit
{
  service = inject(ExportStandardDefinitionsService);
  coreService = inject(CoreService);
  useAccountsHierarchy = false;
  detailService = inject(ExportStandardDefinitionsService);

  types: ISmallGenericType[] = [];

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      Feature.Time_Export_StandardDefinitions,
      'Time.Export.ExportStandardDefinitions'
    );
  }

  loadCompanySettings() {
    return this.coreService
      .getCompanySettings([CompanySettingType.UseAccountHierarchy])
      .pipe(
        tap(x => {
          this.useAccountsHierarchy = SettingsUtil.getBoolCompanySetting(
            x,
            CompanySettingType.UseAccountHierarchy
          );
        })
      );
  }

  override onGridReadyToDefine(grid: GridComponent<IExportDefinitionGridDTO>) {
    super.onGridReadyToDefine(grid);
    this.translate
      .get([
        'common.active',
        'common.name',
        'common.date',
        'common.modified',
        'common.created',
        'core.edit',
        'core.total',
        'core.filtered',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnActive('isActive', terms['common.active'], {
          idField: 'exportDefinitionId',
        });
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 60,
        });
        this.grid.addColumnDate(
          'created',
          terms['common.date'] + ' ' + terms['common.created'],
          {
            flex: 20,
            maxWidth: 200,
          }
        );
        this.grid.addColumnDate(
          'modified',
          terms['common.date'] + ' ' + terms['common.modified'],
          {
            flex: 20,
            maxWidth: 200,
          }
        );
        this.grid.addColumnIconEdit({
          flex: 20,
          tooltip: terms['core.edit'],
          enableHiding: true,
          onClick: row => {
            this.edit(row);
          },
        });

        this.grid.finalizeInitGrid({
          termTotal: terms['core.total'],
          termFiltered: terms['core.filtered'],
          tooltip: 'Antal poster',
        });
      });
  }
}
