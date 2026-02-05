import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';

import { ReactiveFormsModule } from '@angular/forms';
import { ButtonComponent } from '@ui/button/button/button.component';
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component';
import { AutocompleteComponent } from '@ui/forms/autocomplete/autocomplete.component';
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component';
import { RadioComponent } from '@ui/forms/radio/radio.component';
import { SelectComponent } from '@ui/forms/select/select.component';
import { TextareaComponent } from '@ui/forms/textarea/textarea.component';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { GridComponent } from '@ui/grid/grid.component';
import { InstructionComponent } from '@ui/instruction/instruction.component';
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component';
import { TabComponent } from '@ui/tab/tab/tab.component';
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { AbsenceRequestsRoutingModule } from './absence-requests-routing.module';
import { AbsenceRequestsEditComponent } from './components/absence-requests-edit/absence-requests-edit.component';
import { AbsenceRequestsGridComponent } from './components/absence-requests-grid/absence-requests-grid.component';
import { AbsenceRequestsComponent } from './components/absence-requests/absence-requests.component';
import { AbsenceShiftsComponent } from './components/absence-shifts/absence-shifts.component';
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component';

@NgModule({
  declarations: [
    AbsenceRequestsComponent,
    AbsenceRequestsGridComponent,
    AbsenceRequestsEditComponent,
  ],
  imports: [
    CommonModule,
    ToolbarComponent,
    AbsenceRequestsRoutingModule,
    ReactiveFormsModule,
    TabComponent,
    GridComponent,
    GridWrapperComponent,
    MultiTabWrapperComponent,
    DatepickerComponent,
    SelectComponent,
    RadioComponent,
    TextareaComponent,
    InstructionComponent,
    AutocompleteComponent,
    EditFooterComponent,
    ButtonComponent,
    AbsenceShiftsComponent,
    ExpansionPanelComponent,
  ],
})
export class AbsenceRequestsModule {}
