import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { InstructionComponent } from '@ui/instruction/instruction.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { IncomingDeliveryTypesEditComponent } from './components/incoming-delivery-types-edit/incoming-delivery-types-edit.component';
import { IncomingDeliveryTypesGridComponent } from './components/incoming-delivery-types-grid/incoming-delivery-types-grid.component';
import { IncomingDeliveryTypesComponent } from './components/incoming-delivery-types/incoming-delivery-types.component';
import { IncomingDeliveryTypesRoutingModule } from './incoming-delivery-types-routing.module';

@NgModule({
  declarations: [
    IncomingDeliveryTypesComponent,
    IncomingDeliveryTypesEditComponent,
    IncomingDeliveryTypesGridComponent,
  ],
  imports: [
    SharedModule,
    CommonModule,
    IncomingDeliveryTypesRoutingModule,
    ButtonComponent,
    EditFooterComponent,
    GridWrapperComponent,
    SelectComponent,
    MultiTabWrapperComponent,
    TextboxComponent,
    ToolbarComponent,
    ReactiveFormsModule,
    ExpansionPanelComponent,
    InstructionComponent,
    NumberboxComponent,
  ],
})
export class IncomingDeliveryTypesModule {}
