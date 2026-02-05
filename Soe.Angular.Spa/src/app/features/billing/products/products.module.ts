import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { AccountingSettingsModule } from '@shared/components/accounting-settings/accounting-settings.module';
import { BatchUpdateModule } from '@shared/components/batch-update/batch-update.module';
import { CategoriesModule } from '@shared/components/categories/categories.module';
import { DynamicGridModule } from '@shared/components/dynamic-grid/dynamic-grid.module';
import { SearchInvoiceProductDialogModule } from '@shared/components/search-invoice-product-dialog/search-invoice-product-dialog.module';
import { SelectReportDialogModule } from '@shared/components/select-report-dialog/select-report-dialog.module';
import { ExtraFieldsModule } from '@shared/features/extra-fields/extra-fields.module';
import { LanguageTranslationsModule } from '@shared/features/language-translations/language-translations.module';
import { SharedModule } from '@shared/shared.module';
import { AutocompleteComponent } from '@ui/forms/autocomplete/autocomplete.component';
import { ButtonComponent } from '@ui/button/button/button.component';
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component';
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component';
import { GridComponent } from '@ui/grid/grid.component';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { InstructionComponent } from '@ui/instruction/instruction.component';
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component';
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component';
import { SelectComponent } from '@ui/forms/select/select.component';
import { TextareaComponent } from '@ui/forms/textarea/textarea.component';
import { TextboxComponent } from '@ui/forms/textbox/textbox.component';
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { PurchaseProductsModule } from '../purchase-products/purchase-products.module';
import { AccountingPriorityComponent } from './components/products-edit/accounting-priority/accounting-priority.component';
import { ProductInvoiceStatisticsComponent } from './components/products-edit/product-invoice-statistics/product-invoice-statistics.component';
import { ProductStocksComponent } from './components/products-edit/product-stocks/product-stocks.component';
import { ProductUnitConvertGridComponent } from './components/products-edit/product-unit-convert-grid/product-unit-convert-grid.component';
import { ProductsEditComponent } from './components/products-edit/products-edit.component';
import { ProductsPriceListsComponent } from './components/products-edit/products-price-lists/products-price-lists.component';
import { SupplierProductsComponent } from './components/products-edit/supplier-products/supplier-products.component';
import { ProductsGridComponent } from './components/products-grid/products-grid.component';
import { ProductsUnitConversionComponent } from './components/products-grid/products-unit-conversion-dialog/products-unit-conversion-dialog.component';
import { ProductsComponent } from './components/products/products.component';
import { ProductsRoutingModule } from './products-routing.module';
import { CopyProductDialogComponent } from './components/products-edit/copy-product-dialog/copy-product-dialog.component';
import { DeleteButtonComponent } from '@ui/button/delete-button/delete-button.component';
import { ProductsCleanupDialogGrid } from './components/products-grid/products-cleanup-dialog/products-cleanup-dialog-grid/products-cleanup-dialog-grid';
import { ProductsCleanupDialogComponent } from './components/products-grid/products-cleanup-dialog/products-cleanup-dialog-component/products-cleanup-dialog-component';
import { LabelComponent } from '@ui/label/label.component';
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component';

@NgModule({
  declarations: [
    ProductsComponent,
    ProductsGridComponent,
    ProductsEditComponent,
    ProductsUnitConversionComponent,
    ProductsPriceListsComponent,
    AccountingPriorityComponent,
    ProductUnitConvertGridComponent,
    ProductStocksComponent,
    SupplierProductsComponent,
    ProductInvoiceStatisticsComponent,
    CopyProductDialogComponent,
    ProductsCleanupDialogComponent,
    ProductsCleanupDialogGrid,
  ],
  imports: [
    CommonModule,
    ProductsRoutingModule,
    ExpansionPanelComponent,
    SharedModule,
    ButtonComponent,
    EditFooterComponent,
    GridComponent,
    GridWrapperComponent,
    ReactiveFormsModule,
    MultiTabWrapperComponent,
    TextboxComponent,
    ToolbarComponent,
    DialogComponent,
    DynamicGridModule,
    BatchUpdateModule,
    SearchInvoiceProductDialogModule,
    SelectReportDialogModule,
    TextareaComponent,
    CheckboxComponent,
    SelectComponent,
    AutocompleteComponent,
    NumberboxComponent,
    CategoriesModule,
    AccountingSettingsModule,
    PurchaseProductsModule,
    ExtraFieldsModule,
    LanguageTranslationsModule,
    InstructionComponent,
    DeleteButtonComponent,
    LabelComponent,
    DatepickerComponent,
  ],
})
export class ProductsModule {}
