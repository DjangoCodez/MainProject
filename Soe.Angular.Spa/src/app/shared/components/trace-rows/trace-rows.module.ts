import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { InstructionComponent } from '@ui/instruction/instruction.component';
import { TraceRowsComponent } from './trace-rows/trace-rows.component';

@NgModule({
  declarations: [TraceRowsComponent],
  exports: [TraceRowsComponent],
  imports: [CommonModule, GridWrapperComponent, InstructionComponent],
})
export class TraceRowsModule {}
