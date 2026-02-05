import { NgModule } from '@angular/core';

import { SignatoryContractRoutingModule } from './signatory-contract-routing.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { LabelComponent } from '@ui/label/label.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextareaComponent } from '@ui/forms/textarea/textarea.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { SignatoryContractComponent } from './components/signatory-contract/signatory-contract.component';
import { SignatoryContractGridComponent } from './components/signatory-contract-grid/signatory-contract-grid.component';
import { SignatoryContractEditComponent } from './components/signatory-contract-edit/signatory-contract-edit.component';
import { SignatoryContractService } from './services/signatory-contract.service';
import { ReactiveFormsModule } from '@angular/forms';
import { SignatoryContractPermissionEditGridComponent } from './components/signatory-contract-permission-edit-grid/signatory-contract-permission-edit-grid.component';
import { SignatoryContractPermissionsService } from './services/signatory-contract-permissions.service';
import { SubSignatoryContractEditGridComponent } from './components/sub-signatory-contract-edit-grid/sub-signatory-contract-edit-grid.component';
import { SubSignatoryContractService } from './services/sub-signatory-contract.service';
import { SubSignatoryContractEditDialogComponent } from './components/sub-signatory-contract-edit-dialog/sub-signatory-contract-edit-dialog.component';
import { DynamicGridModule } from '@shared/components/dynamic-grid/dynamic-grid.module';
import { SignatoryContractRevokeDialogComponent } from './components/signatory-contract-revoke-dialog/signatory-contract-revoke-dialog.component';

@NgModule({
  declarations: [
    SignatoryContractComponent,
    SignatoryContractGridComponent,
    SignatoryContractEditComponent,
    SignatoryContractPermissionEditGridComponent,
    SubSignatoryContractEditGridComponent,
    SubSignatoryContractEditDialogComponent,
    SignatoryContractRevokeDialogComponent,
  ],
  imports: [
    SignatoryContractRoutingModule,
    MultiTabWrapperComponent,
    GridWrapperComponent,
    ToolbarComponent,
    ReactiveFormsModule,
    CheckboxComponent,
    EditFooterComponent,
    SelectComponent,
    ExpansionPanelComponent,
    DialogComponent,
    ButtonComponent,
    SaveButtonComponent,
    DynamicGridModule,
    TextboxComponent,
    LabelComponent,
    TextareaComponent,
    DatepickerComponent,
  ],
  providers: [
    SignatoryContractService,
    SignatoryContractPermissionsService,
    SubSignatoryContractService,
  ],
})
export class SignatoryContractModule {}
