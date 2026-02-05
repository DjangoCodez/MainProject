import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { SharedModule } from '@shared/shared.module';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { InstructionComponent } from '@ui/instruction/instruction.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { EmployeeCardNumbersGridComponent } from './components/employee-card-numbers-grid/employee-card-numbers-grid.component';
import { EmployeeCardNumbersComponent } from './components/employee-card-numbers/employee-card-numbers.component';
import { EmployeeCardNumbersRoutingModule } from './employee-card-numbers-routing.module';

@NgModule({
  declarations: [
    EmployeeCardNumbersComponent,
    EmployeeCardNumbersGridComponent,
  ],
  imports: [
    SharedModule,
    CommonModule,
    MultiTabWrapperComponent,
    GridWrapperComponent,
    ToolbarComponent,
    InstructionComponent,
    EmployeeCardNumbersRoutingModule,
  ],
})
export class EmployeeCardNumbersModule {}
