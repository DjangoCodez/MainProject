import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { InventoriesComponent } from './components/inventories/inventories.component';
import { AutocompleteComponent } from '@ui/forms/autocomplete/autocomplete.component'
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { IconButtonComponent } from '@ui/button/icon-button/icon-button.component'
import { InstructionComponent } from '@ui/instruction/instruction.component'
import { LabelComponent } from '@ui/label/label.component'
import { MenuButtonComponent } from '@ui/button/menu-button/menu-button.component'
import { MultiSelectComponent } from '@ui/forms/select/multi-select/multi-select.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component'
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextareaComponent } from '@ui/forms/textarea/textarea.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { InventoriesRoutingModule } from './inventories-routing.module';
import { SharedModule } from '@shared/shared.module';
import { ReactiveFormsModule } from '@angular/forms';
import { InventoriesGridComponent } from './components/inventories-grid/inventories-grid.component';
import { InventoriesEditComponent } from './components/inventories-edit/inventories-edit.component';
import { InventoriesAdjustmentDialogComponent } from './components/inventories-adjustment-dialog/inventories-adjustment-dialog.component';
import { AccountingRowsModule } from '@shared/components/accounting-rows/accounting-rows.module';
import { CategoriesModule } from '@shared/components/categories/categories.module';
import { AccountingSettingsModule } from '../../../shared/components/accounting-settings/accounting-settings.module';
import { InventoriesTracingGridComponent } from './components/inventories-tracing-grid/inventories-tracing-grid.component';
import { VoucherModule } from '../voucher/voucher.module';
import { FileDisplayModule } from '@shared/components/file-display/file-display.module';
import { InventoriesGridHeaderComponent } from './components/inventories-grid-header/inventories-grid-header.component';
import { TrackChangesComponent } from '../suppliers/components/track-changes/track-changes.component';

@NgModule({
  declarations: [
    InventoriesComponent,
    InventoriesGridComponent,
    InventoriesEditComponent,
    InventoriesAdjustmentDialogComponent,
    InventoriesGridHeaderComponent,
    InventoriesTracingGridComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    InventoriesRoutingModule,
    ExpansionPanelComponent,
    ButtonComponent,
    IconButtonComponent,
    MenuButtonComponent,
    SaveButtonComponent,
    EditFooterComponent,
    DatepickerComponent,
    GridWrapperComponent,
    SelectComponent,
    MultiSelectComponent,
    NumberboxComponent,
    ReactiveFormsModule,
    MultiTabWrapperComponent,
    TextareaComponent,
    TextboxComponent,
    ToolbarComponent,
    TrackChangesComponent,
    LabelComponent,
    CheckboxComponent,
    AutocompleteComponent,
    DialogComponent,
    AccountingRowsModule,
    CategoriesModule,
    AccountingSettingsModule,
    InstructionComponent,
    VoucherModule,
    FileDisplayModule,
    VoucherModule,
  ],
})
export class InventoriesModule {}
