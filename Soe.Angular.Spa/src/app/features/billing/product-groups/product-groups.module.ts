import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from '@shared/shared.module';
import { ProductGroupsComponent } from './components/product-groups/product-groups.component';
import { ProductGroupsGridComponent } from './components/product-groups-grid/product-groups-grid.component';
import { ProductGroupsEditComponent } from './components/product-groups-edit/product-groups-edit.component';
import { ProductGroupsRoutingModule } from './product-groups-routing.module';
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
    ProductGroupsComponent,
    ProductGroupsGridComponent,
    ProductGroupsEditComponent,
  ],
  imports: [
    CommonModule,
    ProductGroupsRoutingModule,
    SharedModule,
    ExpansionPanelComponent,
    ButtonComponent,
    EditFooterComponent,
    GridWrapperComponent,
    ReactiveFormsModule,
    MultiTabWrapperComponent,
    TextboxComponent,
    ToolbarComponent,
  ],
})
export class ProductGroupsModule {}
