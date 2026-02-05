import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from '@shared/shared.module';
import { DeliveryConditionRoutingModule } from './delivery-condition-routing.module';
import { DeliveryConditionComponent } from './components/delivery-condition/delivery-condition.component';
import { DeliveryConditionGridComponent } from './components/delivery-condition-grid/delivery-condition-grid.component';
import { DeliveryConditionEditComponent } from './components/delivery-condition-edit/delivery-condition-edit.component';
import { ButtonComponent } from '@ui/button/button/button.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ReactiveFormsModule } from '@angular/forms';

@NgModule({
  declarations: [
    DeliveryConditionComponent,
    DeliveryConditionGridComponent,
    DeliveryConditionEditComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    DeliveryConditionRoutingModule,
    ExpansionPanelComponent,
    ButtonComponent,
    EditFooterComponent,
    GridWrapperComponent,
    MultiTabWrapperComponent,
    TextboxComponent,
    ToolbarComponent,
    ReactiveFormsModule,
  ],
})
export class DeliveryConditionModule {}
