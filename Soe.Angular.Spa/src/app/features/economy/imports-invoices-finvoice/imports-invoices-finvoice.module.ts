import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MenuButtonComponent } from '@ui/button/menu-button/menu-button.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { SharedModule } from '@shared/shared.module';
import { ImportsInvoicesFinvoiceComponent } from './components/imports-invoices-finvoice/imports-invoices-finvoice.component';
import { ImportsInvoicesFinvoiceGridComponent } from './components/imports-invoices-finvoice-grid/imports-invoices-finvoice-grid.component';
import { ImportsInvoicesFinvoiceRoutingModule } from './imports-invoices-finvoice-routing.module';
import { FinvoiceFilterGridComponent } from './components/finvoice-filter-grid/finvoice-filter-grid.component';
import { ReactiveFormsModule } from '@angular/forms';

@NgModule({
  declarations: [
    ImportsInvoicesFinvoiceComponent,
    ImportsInvoicesFinvoiceGridComponent,
    FinvoiceFilterGridComponent,
  ],
  imports: [
    CommonModule,
    ImportsInvoicesFinvoiceRoutingModule,
    SharedModule,
    MultiTabWrapperComponent,
    GridWrapperComponent,
    ToolbarComponent,
    SelectComponent,
    ReactiveFormsModule,
    CheckboxComponent,
    DialogComponent,
    ButtonComponent,
    MenuButtonComponent,
    EditFooterComponent,
  ],
})
export class ImportsInvoicesFinvoiceModule {}
