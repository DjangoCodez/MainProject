import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from '@shared/shared.module';
import { ReactiveFormsModule } from '@angular/forms';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { DialogFooterComponent } from '@ui/footer/dialog-footer/dialog-footer.component';
import { SelectComponent } from '@ui/forms/select/select.component';
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component';
import { ButtonComponent } from '@ui/button/button/button.component';
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component';
import { TimeCodePayrollProductsGridComponent } from './time-code-payroll-products-grid.component';
import { TimeCodePayrollProductsDialogComponent } from './time-code-payroll-products-dialog/time-code-payroll-products-dialog.component';

@NgModule({
  declarations: [
    TimeCodePayrollProductsGridComponent,
    TimeCodePayrollProductsDialogComponent,
  ],
  exports: [TimeCodePayrollProductsGridComponent],
  imports: [
    CommonModule,
    SharedModule,
    ReactiveFormsModule,
    GridWrapperComponent,
    DialogComponent,
    DialogFooterComponent,
    SelectComponent,
    NumberboxComponent,
    ButtonComponent,
    SaveButtonComponent,
  ],
})
export class TimeCodePayrollProductsGridModule {}
