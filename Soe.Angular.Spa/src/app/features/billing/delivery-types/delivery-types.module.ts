import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from '@shared/shared.module';
import { DeliveryTypesRoutingModule } from './delivery-types-routing.module';
import { DeliveryTypesComponent } from './components/delivery-types/delivery-types.component';
import { DeliveryTypesGridComponent } from './components/delivery-types-grid/delivery-types-grid.component';
import { DeliveryTypesEditComponent } from './components/delivery-types-edit/delivery-types-edit.component';
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
    DeliveryTypesComponent,
    DeliveryTypesGridComponent,
    DeliveryTypesEditComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    DeliveryTypesRoutingModule,
    ExpansionPanelComponent,
    ButtonComponent,
    EditFooterComponent,
    ReactiveFormsModule,
    GridWrapperComponent,
    MultiTabWrapperComponent,
    TextboxComponent,
    ToolbarComponent,
  ],
})
export class DeliveryTypesModule {}
