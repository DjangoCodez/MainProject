import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { InstructionComponent } from '@ui/instruction/instruction.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextareaComponent } from '@ui/forms/textarea/textarea.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { TimeboxComponent } from '@ui/forms/timebox/timebox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { TimeDeviationCausesEditComponent } from './components/time-deviation-causes-edit/time-deviation-causes-edit.component';
import { TimeDeviationCausesGridComponent } from './components/time-deviation-causes-grid/time-deviation-causes-grid.component';
import { TimeDeviationCausesComponent } from './components/time-deviation-causes/time-deviation-causes.component';
import { TimeDeviationCausesRoutingModule } from './time-deviation-causes-routing.module';

@NgModule({
  declarations: [
    TimeDeviationCausesComponent,
    TimeDeviationCausesGridComponent,
    TimeDeviationCausesEditComponent,
  ],
  imports: [
    CommonModule,
    TimeDeviationCausesRoutingModule,
    SharedModule,
    ReactiveFormsModule,
    MultiTabWrapperComponent,
    GridWrapperComponent,
    ToolbarComponent,
    TextboxComponent,
    EditFooterComponent,
    SelectComponent,
    CheckboxComponent,
    ExpansionPanelComponent,
    TextareaComponent,
    NumberboxComponent,
    InstructionComponent,
    TimeboxComponent,
  ],
})
export class TimeDeviationCausesModule {}
