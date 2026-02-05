import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { TimeboxComponent } from '@ui/forms/timebox/timebox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { OpeningHoursEditComponent } from './components/opening-hours-edit/opening-hours-edit.component';
import { OpeningHoursGridComponent } from './components/opening-hours-grid/opening-hours-grid.component';
import { OpeningHoursComponent } from './components/opening-hours/opening-hours.component';
import { OpeningHoursRoutingModule } from './opening-hours-routing.module';

@NgModule({
  declarations: [
    OpeningHoursComponent,
    OpeningHoursGridComponent,
    OpeningHoursEditComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    OpeningHoursRoutingModule,
    ButtonComponent,
    DatepickerComponent,
    EditFooterComponent,
    GridWrapperComponent,
    SelectComponent,
    ReactiveFormsModule,
    MultiTabWrapperComponent,
    TextboxComponent,
    TimeboxComponent,
    ToolbarComponent,
  ],
})
export class OpeningHoursModule {}
