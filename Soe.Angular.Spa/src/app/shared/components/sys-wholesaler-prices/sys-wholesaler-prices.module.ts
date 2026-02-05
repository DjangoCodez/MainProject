import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { SharedModule } from '@shared/shared.module';
import { GridComponent } from '@ui/grid/grid.component'
import { LabelComponent } from '@ui/label/label.component';
import { SysWholesalerPricesComponent } from './sys-wholesaler-prices.component';

@NgModule({
  imports: [CommonModule, SharedModule, LabelComponent, GridComponent],
  exports: [SysWholesalerPricesComponent],
  declarations: [SysWholesalerPricesComponent],
})
export class SysWholesalerPricesModule {}
