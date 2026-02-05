import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { GridComponent } from '@ui/grid/grid.component'
import { InstructionComponent } from '@ui/instruction/instruction.component'
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component';
import { SysWholesalerPricesModule } from '../sys-wholesaler-prices/sys-wholesaler-prices.module';
import { SearchInvoiceProductDialogComponent } from './search-invoice-product-dialog.component';
import { SearchInvoiceProductGridHeaderComponent } from './search-invoice-product-grid-header/search-invoice-product-grid-header.component';
import { SearchInvoiceProductGridComponent } from './search-invoice-product-grid/search-invoice-product-grid.component';

@NgModule({
  imports: [
    CommonModule,
    SharedModule,
    ReactiveFormsModule,
    GridComponent,
    DialogComponent,
    SysWholesalerPricesModule,
    TextboxComponent,
    SelectComponent,
    NumberboxComponent,
    InstructionComponent,
    ButtonComponent,
  ],
  exports: [SearchInvoiceProductDialogComponent],
  declarations: [
    SearchInvoiceProductDialogComponent,
    SearchInvoiceProductGridComponent,
    SearchInvoiceProductGridHeaderComponent,
  ],
})
export class SearchInvoiceProductDialogModule {}
