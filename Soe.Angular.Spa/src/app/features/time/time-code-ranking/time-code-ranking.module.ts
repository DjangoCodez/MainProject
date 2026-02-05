import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TimeCodeRankingRoutingModule } from './time-code-ranking-routing-module';
import { TimeCodeRankingComponent } from './components/time-code-ranking/time-code-ranking';
import { TimeCodeRankingEdit } from './components/time-code-ranking-edit/time-code-ranking-edit';
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component';
import { SelectComponent } from '@ui/forms/select/select.component';
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component';
import { InstructionComponent } from '@ui/instruction/instruction.component';
import { IconButtonComponent } from '@ui/button/icon-button/icon-button.component';
import { ButtonComponent } from '@ui/button/button/button.component';
import { MultiSelectComponent } from '@ui/forms/select/multi-select/multi-select.component';
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component';
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component';
import { ReactiveFormsModule } from '@angular/forms';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { SharedModule } from '@shared/shared.module';
import { TimeCodeRankingGridComponent } from './components/time-code-ranking-grid/time-code-ranking-grid.component';
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component';
import { TextboxComponent } from '@ui/forms/textbox/textbox.component';

@NgModule({
  declarations: [
    TimeCodeRankingComponent,
    TimeCodeRankingGridComponent,
    TimeCodeRankingEdit,
  ],
  imports: [
    TimeCodeRankingRoutingModule,
    GridWrapperComponent,
    ToolbarComponent,
    SharedModule,
    CommonModule,
    MultiTabWrapperComponent,
    SelectComponent,
    ExpansionPanelComponent,
    InstructionComponent,
    IconButtonComponent,
    ButtonComponent,
    MultiSelectComponent,
    SaveButtonComponent,
    EditFooterComponent,
    ReactiveFormsModule,
    DatepickerComponent,
    TextboxComponent,
  ],
})
export class TimeCodeRankingModule {}
