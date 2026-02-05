import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { IconButtonComponent } from '@ui/button/icon-button/icon-button.component'
import { SelectComponent } from '@ui/forms/select/select.component';
import { PurchaseCustomerInvoiceRowsComponent } from './purchase-customer-invoice-rows.component';

@NgModule({
  declarations: [PurchaseCustomerInvoiceRowsComponent],
  exports: [PurchaseCustomerInvoiceRowsComponent],
  imports: [
    CommonModule,
    GridWrapperComponent,
    SelectComponent,
    IconButtonComponent,
    ReactiveFormsModule,
  ],
})
export class PurchaseCustomerInvoiceRowsModule {}
