import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AccountingCodingLevelsRoutingModule } from './accounting-coding-levels-routing.module';
import { AccountingCodingLevelsComponent } from './components/accounting-coding-levels/accounting-coding-levels.component';
import { AccountingCodingLevelsGridComponent } from './components/accounting-coding-levels-grid/accounting-coding-levels-grid.component';
import { AccountingCodingLevelsEditComponent } from './components/accounting-coding-levels-edit/accounting-coding-levels-edit.component';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { DeleteButtonComponent } from '@ui/button/delete-button/delete-button.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { InstructionComponent } from '@ui/instruction/instruction.component'
import { LabelComponent } from '@ui/label/label.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ReactiveFormsModule } from '@angular/forms';

@NgModule({
  declarations: [
    AccountingCodingLevelsComponent,
    AccountingCodingLevelsGridComponent,
    AccountingCodingLevelsEditComponent,
  ],
  imports: [
    SharedModule,
    CommonModule,
    AccountingCodingLevelsRoutingModule,
    MultiTabWrapperComponent,
    ToolbarComponent,
    ExpansionPanelComponent,
    LabelComponent,
    TextboxComponent,
    ReactiveFormsModule,
    NumberboxComponent,
    EditFooterComponent,
    GridWrapperComponent,
    ButtonComponent,
    DeleteButtonComponent,
    CheckboxComponent,
    SelectComponent,
    InstructionComponent,
  ],
})
export class AccountingCodingLevelsModule {}
