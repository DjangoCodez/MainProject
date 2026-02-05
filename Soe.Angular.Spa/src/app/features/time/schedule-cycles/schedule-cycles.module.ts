import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';

import { ScheduleCyclesRoutingModule } from './schedule-cycles-routing.module';
import { ScheduleCyclesComponent } from './components/schedule-cycles/schedule-cycles.component';
import { ScheduleCyclesGridComponent } from './components/schedule-cycles-grid/schedule-cycles-grid.component';
import { ScheduleCyclesEditComponent } from './components/schedule-cycles-edit/schedule-cycles-edit.component';
import { ScRulesGridComponent } from './components/schedule-cycles-edit/sc-rules-grid/sc-rules-grid.component';

// Shared
import { SharedModule } from '@shared/shared.module';

// UI Components
import { EditFooterComponent } from '@shared/ui-components/footer/edit-footer/edit-footer.component';
import { ExpansionPanelComponent } from '@shared/ui-components/expansion-panel/expansion-panel.component';
import { GridWrapperComponent } from '@shared/ui-components/grid/grid-wrapper/grid-wrapper.component';
import { MultiTabWrapperComponent } from '@shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper.component';
import { NumberboxComponent } from '@shared/ui-components/forms/numberbox/numberbox.component';
import { SelectComponent } from '@shared/ui-components/forms/select/select.component';
import { TextboxComponent } from '@shared/ui-components/forms/textbox/textbox.component';
import { ToolbarComponent } from '@shared/ui-components/toolbar/toolbar.component';

@NgModule({
  declarations: [
    ScheduleCyclesComponent,
    ScheduleCyclesGridComponent,
    ScheduleCyclesEditComponent,
    ScRulesGridComponent,
  ],
  imports: [
    SharedModule,
    CommonModule,
    ScheduleCyclesRoutingModule,
    ReactiveFormsModule,
    EditFooterComponent,
    ExpansionPanelComponent,
    GridWrapperComponent,
    MultiTabWrapperComponent,
    NumberboxComponent,
    SelectComponent,
    TextboxComponent,
    ToolbarComponent,
  ],
})
export class ScheduleCyclesModule {}
