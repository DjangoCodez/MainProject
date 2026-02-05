import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EconomyRoutingModule } from './economy-routing.module';
import { SharedModule } from '@shared/shared.module';

@NgModule({
  declarations: [],
  imports: [CommonModule, EconomyRoutingModule, SharedModule],
})
export class EconomyModule {}
