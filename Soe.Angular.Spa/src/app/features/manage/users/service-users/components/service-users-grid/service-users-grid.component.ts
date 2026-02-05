import { Component, inject, OnInit } from '@angular/core';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take } from 'rxjs';
import { ServiceUserService } from '../../services/service-users.service';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { IServiceUserDTO } from '@shared/models/generated-interfaces/ServiceUserDTO';

@Component({
  selector: 'soe-service-users-grid',
  templateUrl:
    '../../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  imports: [GridWrapperComponent],
  standalone: true,
})
export class ServiceUsersGridComponent
  extends GridBaseDirective<IServiceUserDTO, ServiceUserService>
  implements OnInit
{
  readonly service = inject(ServiceUserService);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(Feature.ClientManagement_Clients, 'Manage.ServiceUsers');
  }

  override onGridReadyToDefine(grid: GridComponent<IServiceUserDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.username',
        'manage.serviceuser.serviceprovider',
        'common.role',
        'core.created',
        'core.createdby',
        'common.modified',
        'common.modifiedby',
        'core.edit',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText(
          'serviceProviderName',
          terms['manage.serviceuser.serviceprovider'],
          {
            valueGetter: params =>
              params.data?.serviceProvider.companyName || '',
          }
        );
        this.grid.addColumnText('userName', terms['common.username'], {
          flex: 2,
        });
        this.grid.addColumnText('roleName', terms['common.role'], {
          flex: 2,
        });
        this.grid.addColumnDate('created', terms['core.created'], {
          flex: 1,
        });
        this.grid.addColumnText('createdBy', terms['core.createdby'], {
          flex: 1,
        });
        this.grid.addColumnDate('modified', terms['core.modified'], {
          flex: 1,
        });
        this.grid.addColumnText('modifiedBy', terms['core.modifiedby'], {
          flex: 1,
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
