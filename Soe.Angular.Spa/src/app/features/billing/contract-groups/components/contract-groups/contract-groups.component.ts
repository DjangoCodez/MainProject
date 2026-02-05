import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { ContractGroupsGridComponent } from '../contract-groups-grid/contract-groups-grid.component';
import { ContractGroupsEditComponent } from '../contract-groups-edit/contract-groups-edit.component';
import { ContractGroupsForm } from '../../models/contract-groups-form.model';

@Component({
    selector: 'soe-contract-groups',
    templateUrl: '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
    standalone: false
})
export class ContractGroupsComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: ContractGroupsGridComponent,
      editComponent: ContractGroupsEditComponent,
      FormClass: ContractGroupsForm,
      gridTabLabel: 'billing.contract.contractgroups.contractgroups',
      editTabLabel: 'billing.contract.contractgroups.contractgroup',
      createTabLabel: 'billing.contract.contractgroups.newcontractgroup',
      exportFilenameKey: 'billing.contract.contractgroups.contractgroups',
    },
  ];
}
