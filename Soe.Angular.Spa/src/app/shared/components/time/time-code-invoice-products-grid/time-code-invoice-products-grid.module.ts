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
import { TimeCodeInvoiceProductsGridComponent } from './time-code-invoice-products-grid.component';
import { TimeCodeInvoiceProductsDialogComponent } from './time-code-invoice-products-dialog/time-code-invoice-products-dialog.component';

@NgModule({
  declarations: [
    TimeCodeInvoiceProductsGridComponent,
    TimeCodeInvoiceProductsDialogComponent,
  ],
  exports: [TimeCodeInvoiceProductsGridComponent],
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
export class TimeCodeInvoiceProductsGridModule {}
