import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { TimeCodeBreakGroupEditComponent } from './components/time-code-break-group-edit/time-code-break-group-edit.component';
import { TimeCodeBreakGroupGridComponent } from './components/time-code-break-group-grid/time-code-break-group-grid.component';
import { TimeCodeBreakGroupComponent } from './components/time-code-break-group/time-code-break-group.component';
import { TimeCodeBreakGroupRoutingModule } from './time-code-break-group-routing.module';

@NgModule({
  declarations: [
    TimeCodeBreakGroupComponent,
    TimeCodeBreakGroupGridComponent,
    TimeCodeBreakGroupEditComponent,
  ],
  imports: [
    SharedModule,
    CommonModule,
    TimeCodeBreakGroupRoutingModule,
    ButtonComponent,
    EditFooterComponent,
    ReactiveFormsModule,
    GridWrapperComponent,
    MultiTabWrapperComponent,
    TextboxComponent,
    ToolbarComponent,
    ExpansionPanelComponent,
  ],
})
export class TimeCodeBreakGroupModule {}
