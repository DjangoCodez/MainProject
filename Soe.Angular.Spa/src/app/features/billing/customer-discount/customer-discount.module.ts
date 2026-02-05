import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CustomerDiscountComponent } from './components/customer-discount/customer-discount.component';
import { SharedModule } from '@shared/shared.module';
import { CustomerDiscountRoutingModule } from './customer-discount-routing.module';
import { CustomerDiscountGridComponent } from './components/customer-discount-grid/customer-discount-grid.component';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component';

@NgModule({
  declarations: [CustomerDiscountComponent, CustomerDiscountGridComponent],
  imports: [
    SharedModule,
    CommonModule,
    CustomerDiscountRoutingModule,
    MultiTabWrapperComponent,
    GridWrapperComponent,
  ],
})
export class CustomerDiscountModule {}
