import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { ButtonComponent } from '@ui/button/button/button.component'
import { DeleteButtonComponent } from '@ui/button/delete-button/delete-button.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { GridComponent } from '@ui/grid/grid.component'
import { IconButtonComponent } from '@ui/button/icon-button/icon-button.component'
import { LabelComponent } from '@ui/label/label.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component';
import { SelectCustomerInvoiceDialogComponent } from './component/select-customer-invoice-dialog/select-customer-invoice-dialog.component';
import { SelectCustomerInvoiceComponent } from './component/select-customer-invoice/select-customer-invoice.component';

@NgModule({
  declarations: [
    SelectCustomerInvoiceComponent,
    SelectCustomerInvoiceDialogComponent,
  ],
  imports: [
    CommonModule,
    DialogComponent,
    GridComponent,
    ButtonComponent,
    IconButtonComponent,
    DeleteButtonComponent,
    TextboxComponent,
    ReactiveFormsModule,
    LabelComponent,
  ],
  exports: [
    SelectCustomerInvoiceComponent,
    SelectCustomerInvoiceDialogComponent,
  ],
})
export class SelectCustomerInvoiceDialogModule {}
