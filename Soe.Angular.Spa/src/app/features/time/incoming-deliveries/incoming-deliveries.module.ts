import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { IconButtonComponent } from '@ui/button/icon-button/icon-button.component'
import { IconModule } from '@ui/icon/icon.module'
import { InstructionComponent } from '@ui/instruction/instruction.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextareaComponent } from '@ui/forms/textarea/textarea.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { IncomingDeliveriesEditRowsGridComponent } from './components/incoming-deliveries-edit/incoming-deliveries-edit-rows-grid/incoming-deliveries-edit-rows-grid.component';
import { IncomingDeliveriesEditComponent } from './components/incoming-deliveries-edit/incoming-deliveries-edit.component';
import { IncomingDeliveriesGridComponent } from './components/incoming-deliveries-grid/incoming-deliveries-grid.component';
import { IncomingDeliveriesComponent } from './components/incoming-deliveries/incoming-deliveries.component';
import { IncomingDeliveriesRoutingModule } from './incoming-deliveries-routing.module';

@NgModule({
  declarations: [
    IncomingDeliveriesComponent,
    IncomingDeliveriesGridComponent,
    IncomingDeliveriesEditComponent,
    IncomingDeliveriesEditRowsGridComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    ReactiveFormsModule,
    IncomingDeliveriesRoutingModule,
    ButtonComponent,
    IconButtonComponent,
    ExpansionPanelComponent,
    EditFooterComponent,
    GridWrapperComponent,
    IconModule,
    InstructionComponent,
    SelectComponent,
    MultiTabWrapperComponent,
    TextareaComponent,
    TextboxComponent,
    ToolbarComponent,
  ],
})
export class IncomingDeliveriesModule {}
