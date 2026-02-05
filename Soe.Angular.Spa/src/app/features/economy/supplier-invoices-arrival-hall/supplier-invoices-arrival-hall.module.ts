import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component';
import { SupplierInvoicesArrivalHallRoutingModule } from './supplier-invoices-arrival-hall-routing.module';
import { SupplierInvoicesArrivalHallGridComponent } from './components/supplier-invoices-arrival-hall-grid/supplier-invoices-arrival-hall-grid.component';
import { SupplierInvoicesArrivalHallComponent } from './components/supplier-invoices-arrival-hall/supplier-invoices-arrival-hall.component';
import { SupplierInvoiceEditModule } from '../shared/supplier-invoice/supplier-invoice-edit.module';
import { FileUploadComponent } from '@ui/forms/file-upload/file-upload.component';
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { GridFooterAmountsComponent } from '@shared/components/grid-footer-amount/grid-footer-amounts.component';
import { MenuButtonComponent } from '@ui/button/menu-button/menu-button.component';

@NgModule({
  declarations: [
    SupplierInvoicesArrivalHallGridComponent,
    SupplierInvoicesArrivalHallComponent,
  ],
  exports: [],
  imports: [
    CommonModule,
    GridWrapperComponent,
    MultiTabWrapperComponent,
    ToolbarComponent,
    FileUploadComponent,
    SupplierInvoicesArrivalHallRoutingModule,
    SupplierInvoiceEditModule,
    GridFooterAmountsComponent,
    MenuButtonComponent,
  ],
})
export class SupplierInvoicesArrivalHallModule {}
