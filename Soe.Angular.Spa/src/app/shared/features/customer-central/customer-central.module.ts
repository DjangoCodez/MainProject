import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { FileDisplayModule } from '@shared/components/file-display/file-display.module';
import { SelectCustomerDialogModule } from '@shared/components/select-customer-dialog/select-customer-dialog.module';
import { SharedModule } from '@shared/shared.module';
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { InstructionComponent } from '@ui/instruction/instruction.component'
import { LabelComponent } from '@ui/label/label.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { CustomerCentralContractGridComponent } from './components/customer-central-contract-grid/customer-central-contract-grid.component';
import { CustomerCentralEditComponent } from './components/customer-central-edit/customer-central-edit.component';
import { CustomerCentralInvoiceGridComponent } from './components/customer-central-invoice-grid/customer-central-invoice-grid.component';
import { CustomerCentralOfferGridComponent } from './components/customer-central-offer-grid/customer-central-offer-grid.component';
import { CustomerCentralOrderGridComponent } from './components/customer-central-order-grid/customer-central-order-grid.component';
import { CustomerCentralComponent } from './components/customer-central/customer-central.component';
import { CustomerCentralRoutingModule } from './customer-central-routing.module';

@NgModule({
  declarations: [
    CustomerCentralComponent,
    CustomerCentralEditComponent,
    CustomerCentralContractGridComponent,
    CustomerCentralOfferGridComponent,
    CustomerCentralOrderGridComponent,
    CustomerCentralInvoiceGridComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    SelectCustomerDialogModule,
    CustomerCentralRoutingModule,
    MultiTabWrapperComponent,
    ToolbarComponent,
    ReactiveFormsModule,
    CheckboxComponent,
    TextboxComponent,
    LabelComponent,
    NumberboxComponent,
    ExpansionPanelComponent,
    GridWrapperComponent,
    InstructionComponent,
    FileDisplayModule,
  ],
})
export class CustomerCentralModule {}
