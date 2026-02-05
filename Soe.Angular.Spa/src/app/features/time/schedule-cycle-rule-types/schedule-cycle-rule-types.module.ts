import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';

import { ScheduleCycleRuleTypesRoutingModule } from './schedule-cycle-rule-types-routing.module';
import { ScheduleCycleRuleTypesComponent } from './components/schedule-cycle-rule-types/schedule-cycle-rule-types.component';
import { ScheduleCycleRuleTypesGridComponent } from './components/schedule-cycle-rule-types-grid/schedule-cycle-rule-types-grid.component';
import { ScheduleCycleRuleTypesEditComponent } from './components/schedule-cycle-rule-types-edit/schedule-cycle-rule-types-edit.component';

// Shared
import { SharedModule } from '@shared/shared.module';

// UI Components
import { ButtonComponent } from '@shared/ui-components/button/button/button.component';
import { EditFooterComponent } from '@shared/ui-components/footer/edit-footer/edit-footer.component';
import { GridWrapperComponent } from '@shared/ui-components/grid/grid-wrapper/grid-wrapper.component';
import { MultiTabWrapperComponent } from '@shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper.component';
import { TextboxComponent } from '@shared/ui-components/forms/textbox/textbox.component';
import { SelectComponent } from '@shared/ui-components/forms/select/select.component';
import { TimeboxComponent } from '@shared/ui-components/forms/timebox/timebox.component';
import { TimerangeComponent } from '@shared/ui-components/forms/timebox/timerange/timerange.component';
import { MultiSelectComponent } from '@shared/ui-components/forms/select/multi-select/multi-select.component';
import { ToolbarComponent } from '@shared/ui-components/toolbar/toolbar.component';
import { ExpansionPanelComponent } from '@shared/ui-components/expansion-panel/expansion-panel.component';

@NgModule({
  declarations: [
    ScheduleCycleRuleTypesComponent,
    ScheduleCycleRuleTypesEditComponent,
    ScheduleCycleRuleTypesGridComponent,
  ],
  imports: [
    SharedModule,
    CommonModule,
    ScheduleCycleRuleTypesRoutingModule,
    ReactiveFormsModule,
    ButtonComponent,
    EditFooterComponent,
    GridWrapperComponent,
    MultiTabWrapperComponent,
    TextboxComponent,
    SelectComponent,
    TimeboxComponent,
    TimerangeComponent,
    MultiSelectComponent,
    ToolbarComponent,
    ExpansionPanelComponent,
  ],
})
export class ScheduleCycleRuleTypesModule {}
