import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonComponent } from '@ui/button/button/button.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ReactiveFormsModule } from '@angular/forms';
import { InventoryWriteOffMethodsRoutingModule } from './inventory-write-off-methods-routing.module';
import { InventoryWriteOffMethodsComponent } from './components/inventory-write-off-methods/Inventory-write-off-methods.component';
import { InventoryWriteOffMethodsEditComponent } from './components/inventory-write-off-methods-edit/inventory-write-off-methods-edit.component';
import { InventoryWriteOffMethodsGridComponent } from './components/inventory-write-off-methods-grid/inventory-write-off-methods-grid.component';
import { SharedModule } from '@shared/shared.module';

@NgModule({
  declarations: [
    InventoryWriteOffMethodsComponent,
    InventoryWriteOffMethodsEditComponent,
    InventoryWriteOffMethodsGridComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    InventoryWriteOffMethodsRoutingModule,
    MultiTabWrapperComponent,
    ToolbarComponent,
    TextboxComponent,
    SelectComponent,
    ReactiveFormsModule,
    NumberboxComponent,
    EditFooterComponent,
    GridWrapperComponent,
    ButtonComponent,
  ],
})
export class InventoryWriteOffMethodsModule {}
