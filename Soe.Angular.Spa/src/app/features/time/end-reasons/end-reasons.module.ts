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
import { EndReasonsEditComponent } from './components/end-reasons-edit/end-reasons-edit.component';
import { EndReasonsGridComponent } from './components/end-reasons-grid/end-reasons-grid.component';
import { EndReasonsComponent } from './components/end-reasons/end-reasons.component';
import { EndReasonsRoutingModule } from './end-reasons-routing.module';

@NgModule({
  declarations: [
    EndReasonsComponent,
    EndReasonsGridComponent,
    EndReasonsEditComponent,
  ],
  imports: [
    SharedModule,
    CommonModule,
    EndReasonsRoutingModule,
    ButtonComponent,
    EditFooterComponent,
    GridWrapperComponent,
    ReactiveFormsModule,
    MultiTabWrapperComponent,
    TextboxComponent,
    ToolbarComponent,
    ExpansionPanelComponent,
  ],
})
export class EndReasonsModule {}
