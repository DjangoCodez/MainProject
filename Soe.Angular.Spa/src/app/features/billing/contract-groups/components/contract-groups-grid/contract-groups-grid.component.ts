import { Component, inject, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { IContractGroupExtendedGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ContractGroupsService } from '../../services/contract-groups.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take } from 'rxjs';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Component({
  selector: 'soe-contract-groups-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ContractGroupsGridComponent
  extends GridBaseDirective<
    IContractGroupExtendedGridDTO,
    ContractGroupsService
  >
  implements OnInit
{
  service = inject(ContractGroupsService);
  progressService = inject(ProgressService);
  performAction = new Perform<BackendResponse>(this.progressService);

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      Feature.Billing_Contract_Groups_Edit,
      'Billing.Contracts.ContractGroups'
    );
  }

  override onGridReadyToDefine(
    grid: GridComponent<IContractGroupExtendedGridDTO>
  ): void {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.name',
        'common.description',
        'common.period',
        'billing.contract.contractgroups.pricemanagement',
        'billing.contract.contractgroups.interval',
        'billing.contract.contractgroups.dayinmonth',
        'core.edit',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('name', terms['common.name'], { flex: 1 });
        this.grid.addColumnText('description', terms['common.description'], {
          flex: 1,
          enableHiding: true,
        });
        this.grid.addColumnText('periodText', terms['common.period'], {
          flex: 1,
        });
        this.grid.addColumnText(
          'priceManagementText',
          terms['billing.contract.contractgroups.pricemanagement'],
          { flex: 1 }
        );
        this.grid.addColumnNumber(
          'interval',
          terms['billing.contract.contractgroups.interval'],
          { width: 80, enableHiding: true }
        );
        this.grid.addColumnNumber(
          'dayInMonth',
          terms['billing.contract.contractgroups.dayinmonth'],
          { width: 100, enableHiding: true }
        );
        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            console.log('edit: ', row);
            this.edit(row);
          },
        });
        super.finalizeInitGrid();
      });
  }
}
