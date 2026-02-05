import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { AttestationGroupsGridComponent } from '../attestation-groups-grid/attestation-groups-grid.component';
import { AttestationGroupForm } from '../../models/attestation-group-form.model';
import { AttestationGroupEditComponent } from '../attestation-group-edit/attestation-group-edit.component';

@Component({
  selector: 'soe-attestation-groups',
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class AttestationGroupsComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: AttestationGroupsGridComponent,
      editComponent: AttestationGroupEditComponent,
      FormClass: AttestationGroupForm,
      gridTabLabel: 'economy.supplier.attestgroup.attestgroups',
      editTabLabel: 'economy.supplier.attestgroup.attestgroup',
      createTabLabel: 'economy.supplier.attestgroup.new_attestgroup',
    },
  ];
}
