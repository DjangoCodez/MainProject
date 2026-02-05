import { Component, inject, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { ISysPositionGridDTO } from '@shared/models/generated-interfaces/SOESysModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take } from 'rxjs/operators';
import { PositionsService } from '../../services/positions.service';

@Component({
  selector: 'soe-positions-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class PositionsGridComponent
  extends GridBaseDirective<ISysPositionGridDTO, PositionsService>
  implements OnInit
{
  service = inject(PositionsService);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Manage_Preferences_Registry_Positions,
      'Manage.Registry.SysPositions'
    );
  }
  override onGridReadyToDefine(grid: GridComponent<ISysPositionGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.name',
        'common.description',
        'core.edit',
        'manage.registry.sysposition.countrycode',
        'manage.registry.sysposition.language',
        'manage.registry.sysposition.ssyk',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText(
          'sysCountryCode',
          terms['manage.registry.sysposition.countrycode'],
          {
            flex: 10,
          }
        );
        this.grid.addColumnText(
          'sysLanguageCode',
          terms['manage.registry.sysposition.language'],
          {
            flex: 10,
          }
        );
        this.grid.addColumnText(
          'code',
          terms['manage.registry.sysposition.ssyk'],
          {
            flex: 10,
          }
        );
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 20,
        });
        this.grid.addColumnText('description', terms['common.description'], {
          flex: 50,
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
