import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from '@shared/shared.module';
import { GrossProfitCodesRoutingModule } from './gross-profit-codes-routing.module';
import { GrossProfitCodesComponent } from './components/gross-profit-codes/gross-profit-codes.component';
import { GrossProfitCodesGridComponent } from './components/gross-profit-codes-grid/gross-profit-codes-grid.component';
import { GrossProfitCodesEditComponent } from './components/gross-profit-codes-edit/gross-profit-codes-edit.component';
import { ButtonComponent } from '@ui/button/button/button.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { InstructionComponent } from '@ui/instruction/instruction.component'
import { LabelComponent } from '@ui/label/label.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextareaComponent } from '@ui/forms/textarea/textarea.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ReactiveFormsModule } from '@angular/forms';

@NgModule({
  declarations: [
    GrossProfitCodesComponent,
    GrossProfitCodesGridComponent,
    GrossProfitCodesEditComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    GrossProfitCodesRoutingModule,
    ButtonComponent,
    GridWrapperComponent,
    EditFooterComponent,
    SelectComponent,
    MultiTabWrapperComponent,
    TextboxComponent,
    NumberboxComponent,
    ToolbarComponent,
    ReactiveFormsModule,
    LabelComponent,
    TextareaComponent,
    InstructionComponent,
  ],
})
export class GrossProfitCodesModule {}
