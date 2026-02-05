import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { InstructionComponent } from '@ui/instruction/instruction.component'
import { LabelComponent } from '@ui/label/label.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { SupplierCentralGridComponent } from './components/supplier-central-grid/supplier-central-grid.component';
import { SupplierCentralHeaderComponent } from './components/supplier-central-header/supplier-central-header.component';
import { SupplierCentralComponent } from './components/supplier-central/supplier-central.component';
import { SupplierCentralRoutingModule } from './supplier-central-routing.module';
import { GridFooterAmountsComponent } from '@shared/components/grid-footer-amount/grid-footer-amounts.component';

@NgModule({
  declarations: [
    SupplierCentralComponent,
    SupplierCentralGridComponent,
    SupplierCentralHeaderComponent,
  ],
  imports: [
    CommonModule,
    SupplierCentralRoutingModule,
    MultiTabWrapperComponent,
    SharedModule,
    ReactiveFormsModule,
    ToolbarComponent,
    EditFooterComponent,
    GridWrapperComponent,
    ExpansionPanelComponent,
    LabelComponent,
    InstructionComponent,
    CheckboxComponent,
    SelectComponent,
    TextboxComponent,
    GridFooterAmountsComponent,
  ],
})
export class SupplierCentralModule {}
