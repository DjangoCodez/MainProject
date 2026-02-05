import { Component } from '@angular/core';
import { DistributionCodeHeadForm } from '../../models/distribution-codes-head-form.model';
import { DistributionCodesGridComponent } from '../distribution-codes-grid/distribution-codes-grid.component';
import { DistributionCodesEditComponent } from '../distribution-codes-edit/distribution-codes-edit.component';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class DistributionCodesComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: DistributionCodesGridComponent,
      editComponent: DistributionCodesEditComponent,
      FormClass: DistributionCodeHeadForm,
      gridTabLabel: 'economy.accounting.distributioncode.distributioncodes',
      editTabLabel: 'economy.accounting.distributioncode.distributioncode',
      createTabLabel:
        'economy.accounting.distributioncode.new_distributioncode',
    },
  ];
}
