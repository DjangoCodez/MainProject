import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { ButtonComponent } from '@ui/button/button/button.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { InstructionComponent } from '@ui/instruction/instruction.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component'
import { SelectComponent } from '@ui/forms/select/select.component';
import { AccountProvisionBaseRoutingModule } from './account-provision-base-routing.module';
import { AccountProvisionBaseGridComponent } from './components/account-provision-base-grid/account-provision-base-grid.component';
import { AccountProvisionBaseComponent } from './components/account-provision-base/account-provision-base.component';

@NgModule({
  declarations: [
    AccountProvisionBaseComponent,
    AccountProvisionBaseGridComponent,
  ],
  imports: [
    CommonModule,
    AccountProvisionBaseRoutingModule,
    GridWrapperComponent,
    SelectComponent,
    ButtonComponent,
    SaveButtonComponent,
    InstructionComponent,
    MultiTabWrapperComponent,
    ReactiveFormsModule,
  ],
})
export class AccountProvisionBaseModule {}
