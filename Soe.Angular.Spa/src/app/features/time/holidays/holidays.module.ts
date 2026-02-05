import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { HolidaysEditComponent } from './components/holidays-edit/holidays-edit.component';
import { HolidaysGridComponent } from './components/holidays-grid/holidays-grid.component';
import { HolidaysComponent } from './components/holidays/holidays.component';
import { HolidaysRoutingModule } from './holidays-routing.module';

@NgModule({
  declarations: [
    HolidaysComponent,
    HolidaysEditComponent,
    HolidaysGridComponent,
  ],
  imports: [
    SharedModule,
    CommonModule,
    HolidaysRoutingModule,
    ButtonComponent,
    EditFooterComponent,
    GridWrapperComponent,
    MultiTabWrapperComponent,
    TextboxComponent,
    ReactiveFormsModule,
    SelectComponent,
    DatepickerComponent,
    CheckboxComponent,
    ToolbarComponent,
    ExpansionPanelComponent,
  ],
})
export class HolidaysModule {}
