import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { AutocompleteComponent } from '@ui/forms/autocomplete/autocomplete.component'
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { IconButtonComponent } from '@ui/button/icon-button/icon-button.component'
import { InstructionComponent } from '@ui/instruction/instruction.component'
import { LabelComponent } from '@ui/label/label.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component'
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component';
import { SieEditComponent } from './components/sie-edit/sie-edit.component';
import { SieComponent } from './components/sie/sie.component';
import { SieRoutingModule } from './sie-routing.module';
import { AccountingPeriodSelectionModule } from '@shared/components/accounting-period-selection';
import { SieConflictsGridComponent } from './components/sie-edit/sie-conflicts-grid/sie-conflicts-grid.component';

@NgModule({
  declarations: [SieComponent, SieEditComponent, SieConflictsGridComponent],
  imports: [
    SieRoutingModule,
    MultiTabWrapperComponent,
    ReactiveFormsModule,
    SelectComponent,
    LabelComponent,
    AccountingPeriodSelectionModule,
    ExpansionPanelComponent,
    CheckboxComponent,
    TextboxComponent,
    NumberboxComponent,
    ButtonComponent,
    IconButtonComponent,
    SaveButtonComponent,
    AutocompleteComponent,
    EditFooterComponent,
    GridWrapperComponent,
    InstructionComponent,
  ],
})
export class SieModule {}
