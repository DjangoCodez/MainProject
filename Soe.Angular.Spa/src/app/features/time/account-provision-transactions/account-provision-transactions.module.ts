import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { ButtonComponent } from '@ui/button/button/button.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { InstructionComponent } from '@ui/instruction/instruction.component'
import { MenuButtonComponent } from '@ui/button/menu-button/menu-button.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component'
import { SelectComponent } from '@ui/forms/select/select.component';
import { AccountProvisionTransactionsRoutingModule } from './account-provision-transactions-routing.module';
import { AccountProvisionTransactionsGridComponent } from './components/account-provision-transactions-grid/account-provision-transactions-grid.component';
import { AccountProvisionTransactionsComponent } from './components/account-provision-transactions/account-provision-transactions.component';

@NgModule({
  declarations: [
    AccountProvisionTransactionsComponent,
    AccountProvisionTransactionsGridComponent,
  ],
  imports: [
    CommonModule,
    AccountProvisionTransactionsRoutingModule,
    GridWrapperComponent,
    SelectComponent,
    ButtonComponent,
    MenuButtonComponent,
    SaveButtonComponent,
    InstructionComponent,
    MultiTabWrapperComponent,
    ReactiveFormsModule,
  ],
})
export class AccountProvisionTransactionsModule {}
