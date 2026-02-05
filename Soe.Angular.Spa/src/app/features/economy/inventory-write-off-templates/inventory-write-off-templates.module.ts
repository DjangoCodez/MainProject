import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from '@shared/shared.module';
import { InventoryWriteOffTemplatesRoutingModule } from './inventory-write-off-templates-routing.module';
import { InventoryWriteOffTemplatesComponent } from './components/inventory-write-off-templates/inventory-write-off-templates.component';
import { InventoryWriteOffTemplatesGridComponent } from './components/inventory-write-off-templates-grid/inventory-write-off-templates-grid.component';
import { InventoryWriteOffTemplatesEditComponent } from './components/inventory-write-off-templates-edit/inventory-write-off-templates-edit.component';
import { ButtonComponent } from '@ui/button/button/button.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ReactiveFormsModule } from '@angular/forms';
import { AccountingSettingsModule } from '../../../shared/components/accounting-settings/accounting-settings.module';

@NgModule({
  declarations: [
    InventoryWriteOffTemplatesComponent,
    InventoryWriteOffTemplatesGridComponent,
    InventoryWriteOffTemplatesEditComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    InventoryWriteOffTemplatesRoutingModule,
    ExpansionPanelComponent,
    ButtonComponent,
    EditFooterComponent,
    GridWrapperComponent,
    SelectComponent,
    ReactiveFormsModule,
    MultiTabWrapperComponent,
    TextboxComponent,
    ToolbarComponent,
    AccountingSettingsModule,
  ],
})
export class InventoryWriteOffTemplatesModule {}
