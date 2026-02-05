import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { DaterangepickerComponent } from '@ui/forms/datepicker/daterangepicker/daterangepicker.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { RadioComponent } from '@ui/forms/radio/radio.component'
import { SelectComponent } from '@ui/forms/select/select.component';
import { AccountingPeriodSelectionComponent } from './components/accounting-period-selection/accounting-period-selection.component';

@NgModule({
  declarations: [AccountingPeriodSelectionComponent],
  imports: [
    CommonModule,
    ExpansionPanelComponent,
    ReactiveFormsModule,
    RadioComponent,
    SelectComponent,
    DaterangepickerComponent,
  ],
  exports: [AccountingPeriodSelectionComponent],
})
export class AccountingPeriodSelectionModule {}
