import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SelectUsersDialogModule } from '@shared/components/billing/select-users-dialog/select-users-dialog.module';
import { CategoriesModule } from '@shared/components/categories/categories.module';
import { ContactAddressesModule } from '@shared/components/contact-addresses/contact-addresses.module';
import { ContactPersonsModule } from '@shared/components/contact-persons/contact-persons.module';
import { HouseholdTaxDeductionDialogModule } from '@shared/components/household-tax-deduction-dialog/household-tax-deduction-dialog.module';
import { TaxDeductionContactsModule } from '@shared/components/tax-deduction-contacts/tax-deduction-contacts.module';
import { SharedModule } from '@shared/shared.module';
import { AccountingSettingsModule } from '../../components/accounting-settings/accounting-settings.module';
import { FileDisplayModule } from '../../components/file-display/file-display.module';
import { ExtraFieldsModule } from '../extra-fields/extra-fields.module';
import { CustomerEditComponent } from './components/customer-edit/customer-edit.component';
import { CustomerProductsComponent } from './components/customer-edit/customer-products/customer-products.component';
import { CustomerGridComponent } from './components/customer-grid/customer-grid.component';
import { CustomerStatisticsComponent } from './components/customer-statistics/customer-statistics.component';
import { CustomerComponent } from './components/customer/customer.component';
import { CustomerRoutingModule } from './customer-routing.module';
import { ExternalCompanySearchDialogComponent } from '@shared/components/external-company-search-dialog/external-company-search-dialog.component';
import { SelectCustomerDialogModule } from '@shared/components/select-customer-dialog/select-customer-dialog.module';
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component';
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component';
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component';
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component';
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component';
import { TextboxComponent } from '@ui/forms/textbox/textbox.component';
import { TextareaComponent } from '@ui/forms/textarea/textarea.component';
import { SelectComponent } from '@ui/forms/select/select.component';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { ButtonComponent } from '@ui/button/button/button.component';
import { IconButtonComponent } from '@ui/button/icon-button/icon-button.component';
import { LabelComponent } from '@ui/label/label.component';
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component';
import { AutocompleteComponent } from '@ui/forms/autocomplete/autocomplete.component';
import { InstructionComponent } from '@ui/instruction/instruction.component';
import { GridComponent } from '@ui/grid/grid.component';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { SearchEinvoiceRecipientResultGridComponent } from './components/search-einvoice-recipient-dialog/search-einvoice-recipient-result-grid/search-einvoice-recipient-result-grid.component';
import { SearchEinvoiceRecipientDialogComponent } from './components/search-einvoice-recipient-dialog/search-einvoice-recipient-dialog.component';

@NgModule({
  declarations: [
    CustomerComponent,
    CustomerEditComponent,
    CustomerGridComponent,
    CustomerProductsComponent,
    CustomerStatisticsComponent,
    SearchEinvoiceRecipientDialogComponent,
    SearchEinvoiceRecipientResultGridComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    CustomerRoutingModule,
    MultiTabWrapperComponent,
    ToolbarComponent,
    EditFooterComponent,
    ExpansionPanelComponent,
    GridWrapperComponent,
    DatepickerComponent,
    ReactiveFormsModule,
    CheckboxComponent,
    TextboxComponent,
    TextareaComponent,
    SelectComponent,
    ContactAddressesModule,
    ContactPersonsModule,
    SelectUsersDialogModule,
    DialogComponent,
    ButtonComponent,
    IconButtonComponent,
    LabelComponent,
    NumberboxComponent,
    AutocompleteComponent,
    CategoriesModule,
    AccountingSettingsModule,
    FileDisplayModule,
    ExtraFieldsModule,
    HouseholdTaxDeductionDialogModule,
    TaxDeductionContactsModule,
    InstructionComponent,
    ExternalCompanySearchDialogComponent,
    SelectCustomerDialogModule,
    GridComponent,
  ],
})
export class CustomerModule {}
