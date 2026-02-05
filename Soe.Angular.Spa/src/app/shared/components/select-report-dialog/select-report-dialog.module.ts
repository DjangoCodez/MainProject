import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SelectReportDialogComponent } from './components/select-report-dialog/select-report-dialog.component';
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { SelectComponent } from '@ui/forms/select/select.component';
import { SharedModule } from '@shared/shared.module';
import { ReactiveFormsModule } from '@angular/forms';

@NgModule({
  declarations: [SelectReportDialogComponent],
  exports: [SelectReportDialogComponent],
  imports: [
    CommonModule,
    SharedModule,
    DialogComponent,
    ButtonComponent,
    SelectComponent,
    CheckboxComponent,
    ReactiveFormsModule,
  ],
})
export class SelectReportDialogModule {}
