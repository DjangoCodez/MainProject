import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { IconButtonComponent } from '@ui/button/icon-button/icon-button.component'
import { InstructionComponent } from '@ui/instruction/instruction.component'
import { MultiSelectComponent } from '@ui/forms/select/multi-select/multi-select.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TimeboxComponent } from '@ui/forms/timebox/timebox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { AnnualLeaveBalanceComponent } from './components/annual-leave-balance/annual-leave-balance.component';
import { AnnualLeaveBalanceGridComponent } from './components/annual-leave-balance-grid/annual-leave-balance-grid.component';
import { AnnualLeaveBalanceEditComponent } from './components/annual-leave-balance-edit/annual-leave-balance-edit.component';
import { AnnualLeaveBalanceRoutingModule } from './annual-leave-balance-routing.module';
import { AnnualLeaveBalanceGridFilterComponent } from './components/annual-leave-balance-grid/annual-leave-balance-grid-filter/annual-leave-balance-grid-filter.component';
import { LabelComponent } from '../../../shared/ui-components/label/label.component';

@NgModule({
  declarations: [
    AnnualLeaveBalanceComponent,
    AnnualLeaveBalanceEditComponent,
    AnnualLeaveBalanceGridComponent,
    AnnualLeaveBalanceGridFilterComponent,
  ],
  imports: [
    SharedModule,
    CommonModule,
    AnnualLeaveBalanceRoutingModule,
    ButtonComponent,
    EditFooterComponent,
    GridWrapperComponent,
    SelectComponent,
    MultiSelectComponent,
    ReactiveFormsModule,
    MultiTabWrapperComponent,
    ToolbarComponent,
    NumberboxComponent,
    DatepickerComponent,
    TimeboxComponent,
    LabelComponent,
    InstructionComponent,
    IconButtonComponent,
  ],
})
export class AnnualLeaveBalanceModule {}
