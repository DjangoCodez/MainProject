import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { DeleteButtonComponent } from '@ui/button/delete-button/delete-button.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { InstructionComponent } from '@ui/instruction/instruction.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component'
import { SelectComponent } from '@ui/forms/select/select.component';
import { EarnedHolidayGridComponent } from './components/earned-holiday-grid/earned-holiday-grid.component';
import { EarnedHolidayComponent } from './components/earned-holiday/earned-holiday.component';
import { EarnedHolidayRoutingModule } from './earned-holiday-routing.module';

@NgModule({
  declarations: [EarnedHolidayComponent, EarnedHolidayGridComponent],
  imports: [
    CommonModule,
    EarnedHolidayRoutingModule,
    SharedModule,
    ReactiveFormsModule,
    MultiTabWrapperComponent,
    GridWrapperComponent,
    SelectComponent,
    CheckboxComponent,
    ButtonComponent,
    SaveButtonComponent,
    DeleteButtonComponent,
    InstructionComponent,
  ],
})
export class EarnedHolidayModule {}
