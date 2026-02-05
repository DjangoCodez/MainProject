import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BillingRoutingModule } from './billing-routing.module';
import { SharedModule } from '@shared/shared.module';

@NgModule({
  imports: [CommonModule, BillingRoutingModule, SharedModule],
  declarations: [],
})
export class BillingModule {}
