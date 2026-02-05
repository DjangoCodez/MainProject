import { Component, inject, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take } from 'rxjs';
import { ISysCompanyDTO } from 'src/app/features/manage/models/sysCompany.model';
import { SysCompanyService } from '../../services/sys-company.service';

@Component({
  selector: 'soe-sys-company-grid',
  templateUrl:
    '../../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class SysCompanyGridComponent
  extends GridBaseDirective<ISysCompanyDTO>
  implements OnInit
{
  service = inject(SysCompanyService);

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      Feature.Manage_System,
      'Soe.Manage.System.SysCompany.SysCompany'
    );
  }

  override onGridReadyToDefine(grid: GridComponent<ISysCompanyDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.name',
        'common.licensenr',
        'common.licensnamefull',
        'manage.system.syscompany.syscompdb',
        'manage.system.syscompany.usesbankintegration',
        'manage.system.syscompany.verifiedorgnr',
        'common.server',
        'core.edit',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('name', terms['common.name'], { flex: 1 });
        this.grid.addColumnText('licenseNumber', terms['common.licensenr'], {
          flex: 1,
        });
        this.grid.addColumnText('licenseName', terms['common.licensnamefull'], {
          flex: 1,
        });
        this.grid.addColumnText(
          'dbName',
          terms['manage.system.syscompany.syscompdb'],
          {
            flex: 1,
          }
        );
        this.grid.addColumnText('serverName', terms['common.server'], {
          flex: 1,
        });
        this.grid.addColumnBool(
          'usesBankIntegration',
          terms['manage.system.syscompany.usesbankintegration'],
          { flex: 1 }
        );
        this.grid.addColumnText(
          'verifiedOrgNr',
          terms['manage.system.syscompany.verifiedorgnr'],
          { flex: 1 }
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
}
