import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { SchoolHolidayEditComponent } from './components/school-holiday-edit/school-holiday-edit.component';
import { SchoolHolidayGridComponent } from './components/school-holiday-grid/school-holiday-grid.component';
import { SchoolHolidayComponent } from './components/school-holiday/school-holiday.component';
import { SchoolHolidayRoutingModule } from './school-holiday-routing.module';

@NgModule({
  declarations: [
    SchoolHolidayComponent,
    SchoolHolidayGridComponent,
    SchoolHolidayEditComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    SchoolHolidayRoutingModule,
    DatepickerComponent,
    ButtonComponent,
    CheckboxComponent,
    EditFooterComponent,
    GridWrapperComponent,
    ReactiveFormsModule,
    MultiTabWrapperComponent,
    TextboxComponent,
    ToolbarComponent,
  ],
})
export class SchoolHolidayModule {}
