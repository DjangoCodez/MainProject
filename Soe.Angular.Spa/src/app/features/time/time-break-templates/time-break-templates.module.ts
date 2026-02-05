import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component';
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component';
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component';
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component';
import { SelectComponent } from '@ui/forms/select/select.component';
import { TextboxComponent } from '@ui/forms/textbox/textbox.component';
import { TimeboxComponent } from '@ui/forms/timebox/timebox.component';
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component';
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component';
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { TimeBreakTemplatesEditComponent } from './components/time-break-templates-edit/time-break-templates-edit.component';
import { TimeBreakTemplatesGridComponent } from './components/time-break-templates-grid/time-break-templates-grid.component';
import { TimeBreakTemplatesComponent } from './components/time-break-templates/time-break-templates.component';
import { TbtShiftTypesGridComponent } from './components/time-break-templates-edit/tbt-shift-types-grid/tbt-shift-types-grid.component';
import { TbtWeekdaysGridComponent } from './components/time-break-templates-edit/tbt-weekdays-grid/tbt-weekdays-grid.component';
import { TimeBreakTemplatesRoutingModule } from './time-break-templates-routing.module';

@NgModule({
  declarations: [
    TimeBreakTemplatesComponent,
    TimeBreakTemplatesGridComponent,
    TimeBreakTemplatesEditComponent,
    TbtShiftTypesGridComponent,
    TbtWeekdaysGridComponent,
  ],
  imports: [
    SharedModule,
    CommonModule,
    TimeBreakTemplatesRoutingModule,
    ReactiveFormsModule,
    ButtonComponent,
    CheckboxComponent,
    DatepickerComponent,
    NumberboxComponent,
    SelectComponent,
    TextboxComponent,
    TimeboxComponent,
    EditFooterComponent,
    ExpansionPanelComponent,
    GridWrapperComponent,
    MultiTabWrapperComponent,
    ToolbarComponent,
  ],
})
export class TimeBreakTemplatesModule {}
