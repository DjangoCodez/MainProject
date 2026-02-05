import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { AutocompleteComponent } from '@ui/forms/autocomplete/autocomplete.component';
import { ButtonComponent } from '@ui/button/button/button.component';
import { CreatedModifiedComponent } from '@ui/created-modified/created-modified.component';
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component';
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component';
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component';
import { FileUploadComponent } from '@ui/forms/file-upload/file-upload.component';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { IconButtonComponent } from '@ui/button/icon-button/icon-button.component';
import { IconModule } from '@ui/icon/icon.module';
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component';
import { PdfViewerComponent } from '@ui/pdf-viewer/pdf-viewer.component';
import { SelectComponent } from '@ui/forms/select/select.component';
import { SplitContainerComponent } from '@ui/split-container/split-container.component';
import { TextboxComponent } from '@ui/forms/textbox/textbox.component';
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { SupplierInvoiceEditComponent } from './components/supplier-invoice-edit/supplier-invoice-edit.component';
import { SupplierInvoiceEditHeadComponent } from './components/supplier-invoice-edit-head/supplier-invoice-edit-head.component';
import { SupplierInvoiceEditImagePreviewComponent } from './components/supplier-invoice-edit-image-preview/supplier-invoice-edit-image-preview.component';
import { DynamicContainerStyles } from '@shared/styles/dynamic-container-styles/dynamic-container-styles.component';
import { AutoHeightDirective } from '@shared/directives/auto-height/auto-height.directive';
import { SupplierInvoiceHistoryGridComponent } from './components/supplier-invoice-history-grid/supplier-invoice-history-grid.component';
import { SupplierInvoiceHistoryDetailsComponent } from './components/supplier-invoice-history-details/supplier-invoice-history-details.component';
import { SupplierInvoiceAccountingRowsComponent } from './components/supplier-invoice-accounting-rows/supplier-invoice-accounting-rows.component';
import { AccountingRowsModule } from '@shared/components/accounting-rows/accounting-rows.module';
import { LabelComponent } from '@ui/label/label.component';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component';
import { CostAllocationComponent } from './components/cost-allocation/cost-allocation';
import { ReBilledComponent } from './components/cost-allocation/re-billed/re-billed';
import { ChargedToProjectComponent } from './components/cost-allocation/charged-to-project/charged-to-project';
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component';
import { FileDisplayModule } from '@shared/components/file-display/file-display.module';
import { TraceRowsModule } from '@shared/components/trace-rows/trace-rows.module';
import { SupplierInvoiceProductRowsComponent } from './components/supplier-invoice-product-rows/supplier-invoice-product-rows.component';
import { CostAllocationDialog } from './components/cost-allocation/cost-allocation-dialog/cost-allocation-dialog';
import { PageHeaderComponent } from '@shared/components/page-header/page-header.component';

@NgModule({
  declarations: [
    SupplierInvoiceEditComponent,
    SupplierInvoiceEditHeadComponent,
    SupplierInvoiceEditImagePreviewComponent,
    SupplierInvoiceHistoryGridComponent,
    SupplierInvoiceHistoryDetailsComponent,
    SupplierInvoiceAccountingRowsComponent,
    CostAllocationComponent,
    ReBilledComponent,
    ChargedToProjectComponent,
    CostAllocationDialog,
  ],
  imports: [
    CommonModule,
    SharedModule,
    AutoHeightDirective,
    ExpansionPanelComponent,
    TextboxComponent,
    SelectComponent,
    ReactiveFormsModule,
    AutocompleteComponent,
    ToolbarComponent,
    NumberboxComponent,
    EditFooterComponent,
    SplitContainerComponent,
    CreatedModifiedComponent,
    ButtonComponent,
    DatepickerComponent,
    DynamicContainerStyles,
    FileUploadComponent,
    PdfViewerComponent,
    GridWrapperComponent,
    IconModule,
    AccountingRowsModule,
    IconButtonComponent,
    LabelComponent,
    ButtonComponent,
    DialogComponent,
    CheckboxComponent,
    SaveButtonComponent,
    FileDisplayModule,
    TraceRowsModule,
    SupplierInvoiceProductRowsComponent,
    PageHeaderComponent,
  ],
})
export class SupplierInvoiceEditModule {}
