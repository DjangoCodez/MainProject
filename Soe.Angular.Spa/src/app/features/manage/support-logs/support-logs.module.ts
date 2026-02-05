import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextareaComponent } from '@ui/forms/textarea/textarea.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { TimeboxComponent } from '@ui/forms/timebox/timebox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { SupportLogsEditComponent } from './components/support-logs-edit/support-logs-edit.component';
import { SupportLogsGridHeaderComponent } from './components/support-logs-grid/support-logs-grid-header/support-logs-grid-header.component';
import { SupportLogsGridComponent } from './components/support-logs-grid/support-logs-grid.component';
import { SupportLogsComponent } from './components/support-logs/support-logs.component';
import { SupportLogsRoutingModule } from './support-logs-routing.module';

@NgModule({
  declarations: [
    SupportLogsComponent,
    SupportLogsEditComponent,
    SupportLogsGridComponent,
    SupportLogsGridHeaderComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    SupportLogsRoutingModule,
    MultiTabWrapperComponent,
    ToolbarComponent,
    ExpansionPanelComponent,
    TextboxComponent,
    TimeboxComponent,
    DatepickerComponent,
    SelectComponent,
    ReactiveFormsModule,
    NumberboxComponent,
    GridWrapperComponent,
    ButtonComponent,
    TextareaComponent,
  ],
})
export class SupportLogsModule {}
