import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AutocompleteComponent } from '@ui/forms/autocomplete/autocomplete.component'
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { IconButtonComponent } from '@ui/button/icon-button/icon-button.component'
import { InstructionComponent } from '@ui/instruction/instruction.component'
import { LabelComponent } from '@ui/label/label.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextareaComponent } from '@ui/forms/textarea/textarea.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { SuppliersRoutingModule } from './suppliers-routing.module';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { SuppliersComponent } from './components/suppliers/suppliers.component';
import { SuppliersGridComponent } from './components/suppliers-grid/suppliers-grid.component';
import { SuppliersEditComponent } from './components/suppliers-edit/suppliers-edit.component';
import { AccountingSettingsModule } from '@shared/components/accounting-settings/accounting-settings.module';
import { ContactAddressesModule } from '@shared/components/contact-addresses/contact-addresses.module';
import { ContactPersonsModule } from '../../../shared/components/contact-persons/contact-persons.module';
import { ExtraFieldsModule } from '@shared/features/extra-fields/extra-fields.module';
import { CategoriesModule } from '@shared/components/categories/categories.module';
import { PaymentInformationModule } from '@shared/components/payment-information/payment-information.module';
import { FileDisplayModule } from '@shared/components/file-display/file-display.module';
import { ExternalCompanySearchDialogComponent } from '@shared/components/external-company-search-dialog/external-company-search-dialog.component';
import { TrackChangesComponent } from './components/track-changes/track-changes.component';

@NgModule({
  declarations: [
    SuppliersComponent,
    SuppliersGridComponent,
    SuppliersEditComponent,
  ],
  imports: [
    SharedModule,
    CommonModule,
    SuppliersRoutingModule,
    MultiTabWrapperComponent,
    ToolbarComponent,
    ExpansionPanelComponent,
    LabelComponent,
    TextboxComponent,
    TextareaComponent,
    ReactiveFormsModule,
    EditFooterComponent,
    DatepickerComponent,
    GridWrapperComponent,
    ButtonComponent,
    IconButtonComponent,
    CheckboxComponent,
    SelectComponent,
    AccountingSettingsModule,
    ContactAddressesModule,
    ContactPersonsModule,
    ExtraFieldsModule,
    AutocompleteComponent,
    CategoriesModule,
    PaymentInformationModule,
    InstructionComponent,
    FileDisplayModule,
    ExternalCompanySearchDialogComponent,
    TrackChangesComponent,
  ],
})
export class SuppliersModule {}
