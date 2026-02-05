import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from '@shared/shared.module';
import { SystemRoutingModule } from './system-routing.module';

@NgModule({
  declarations: [],
  exports: [],
  imports: [CommonModule, SharedModule, SystemRoutingModule],
})
export class SystemModule {}
