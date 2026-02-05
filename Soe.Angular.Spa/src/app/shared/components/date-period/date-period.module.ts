import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { SharedModule } from '@shared/shared.module';
import { DatePeriodComponent } from './date-period.component';
import { ReactiveFormsModule } from '@angular/forms';
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component'
import { SelectComponent } from '@ui/forms/select/select.component';

@NgModule({
  declarations: [DatePeriodComponent],
  imports: [
    CommonModule,
    SharedModule,
    ReactiveFormsModule,
    SelectComponent,
    DatepickerComponent,
  ],
  exports: [DatePeriodComponent],
})
export class DatePeriodModule {}
