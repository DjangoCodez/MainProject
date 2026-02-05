import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { PriceOptimizationGridComponent } from '../price-optimization-grid/price-optimization-grid.component';
import { PriceOptimizationEditComponent } from '../price-optimization-edit/price-optimization-edit.component';
import { PriceOptimizationForm } from '../../models/price-optimization-form.model';

@Component({
  selector: 'soe-purchase-price-optimization',
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class PriceOptimizationComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: PriceOptimizationGridComponent,
      editComponent: PriceOptimizationEditComponent,
      FormClass: PriceOptimizationForm,
      gridTabLabel: 'billing.purchase.priceoptimization.priceoptimizations',
      editTabLabel: 'billing.purchase.priceoptimization.priceoptimization',
      createTabLabel: 'billing.purchase.priceoptimization.new',
    },
  ];
}
