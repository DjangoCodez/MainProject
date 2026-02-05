import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ManageRoutingModule } from './manage-routing.module';
import { SharedModule } from '@shared/shared.module';

@NgModule({
  declarations: [],
  imports: [CommonModule, ManageRoutingModule, SharedModule],
})
export class ManageModule {}
