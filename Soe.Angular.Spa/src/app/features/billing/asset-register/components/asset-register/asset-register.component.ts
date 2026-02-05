import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { AssetRegisterGridComponent } from '../asset-register-grid/asset-register-grid.component';

@Component({
  selector: 'soe-asset-register',
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class AssetRegisterComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: AssetRegisterGridComponent,
      gridTabLabel: 'billing.asset.asset', //TODO: add to DB
    },
  ];
}
