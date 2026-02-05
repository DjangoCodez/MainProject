import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { GridComponent } from '@ui/grid/grid.component';
import { MultiSelectGridComponent } from './multi-select-grid.component';

@NgModule({
  declarations: [MultiSelectGridComponent],
  exports: [MultiSelectGridComponent],
  imports: [CommonModule, GridComponent],
})
export class MultiSelectGridModule {}
