import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { SupplierCentralGridComponent } from '../supplier-central-grid/supplier-central-grid.component';
import { SupplierCentralUrlParamsService } from '../../services/supplier-central-params.service';

@Component({
  selector: 'soe-supplier-central',
  templateUrl: './supplier-central.component.html',
  styleUrl: './supplier-central.component.scss',
  standalone: false,
  providers: [SupplierCentralUrlParamsService],
})
export class SupplierCentralComponent {
  config: MultiTabConfig[] = [
    {
      gridTabLabel: 'economy.supplier.suppliercentral.suppliercentral',
      gridComponent: SupplierCentralGridComponent,
    },
  ];
}
