import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ImportRowsComponent } from './import-rows.component';
import { ButtonComponent } from '@ui/button/button/button.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';

@NgModule({
  declarations: [ImportRowsComponent],
  exports: [ImportRowsComponent],
  imports: [CommonModule, GridWrapperComponent, ButtonComponent],
})
export class ImportRowsModule {}
