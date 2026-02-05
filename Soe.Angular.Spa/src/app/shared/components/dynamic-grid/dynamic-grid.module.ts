import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { SharedModule } from '@shared/shared.module';
import { GridComponent } from '@ui/grid/grid.component';
import { DynamicGridComponent } from './dynamic-grid.component';

@NgModule({
  declarations: [DynamicGridComponent],
  imports: [CommonModule, GridComponent, SharedModule],
  exports: [DynamicGridComponent],
})
export class DynamicGridModule {}
