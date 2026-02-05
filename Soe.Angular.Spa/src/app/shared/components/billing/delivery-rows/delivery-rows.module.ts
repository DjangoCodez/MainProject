import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { DeliveryRowsComponent } from './delivery-rows.component';

@NgModule({
  declarations: [DeliveryRowsComponent],
  exports: [DeliveryRowsComponent],
  imports: [CommonModule, GridWrapperComponent],
})
export class DeliveryRowsModule {}
