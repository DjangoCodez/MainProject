import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { InstructionComponent } from '@ui/instruction/instruction.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component'
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { CustomerProductPriceListTypesEditComponent } from './components/customer-product-pricelisttypes-edit/customer-product-pricelisttypes-edit.component';
import { CustomerProductPriceListTypesGridComponent } from './components/customer-product-pricelisttypes-grid/customer-product-pricelisttypes-grid.component';
import { CustomerProductPriceListTypesComponent } from './components/customer-product-pricelisttypes/customer-product-pricelisttypes.component';
import { CustomerProductPriceListRoutingModule } from './customer-product-pricelist-routing.module';
import { CustomerProductPriceListsComponent } from './components/customer-product-pricelists/customer-product-pricelists.component';
import { PriceListUpdateComponent } from './components/customer-product-pricelisttypes-grid/pricelist-update-modal/pricelist-update-modal.component';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';

@NgModule({
  declarations: [
    CustomerProductPriceListTypesGridComponent,
    CustomerProductPriceListTypesEditComponent,
    CustomerProductPriceListTypesComponent,
    CustomerProductPriceListsComponent,
    PriceListUpdateComponent,
  ],
  imports: [
    SharedModule,
    CommonModule,
    GridWrapperComponent,
    ReactiveFormsModule,
    DialogComponent,
    MultiTabWrapperComponent,
    TextboxComponent,
    ButtonComponent,
    SaveButtonComponent,
    CheckboxComponent,
    DatepickerComponent,
    SelectComponent,
    ToolbarComponent,
    ExpansionPanelComponent,
    EditFooterComponent,
    NumberboxComponent,
    InstructionComponent,
    CustomerProductPriceListRoutingModule,
  ],
})
export class CustomerProductPricelistsModule {}
