import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MaterialCodesRoutingModule } from './material-codes-routing.module';
import { MaterialCodesGridComponent } from './components/material-codes-grid/material-codes-grid.component';
import { MaterialCodesEditComponent } from './components/material-codes-edit/material-codes-edit.component';
import { MaterialCodesComponent } from './components/material-codes/material-codes.component';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { TextareaComponent } from '@ui/forms/textarea/textarea.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ReactiveFormsModule } from '@angular/forms';
import { MaterialCodesEditItemGridComponent } from './components/material-codes-edit-item-grid/material-codes-edit-item-grid.component';

@NgModule({
  declarations: [
    MaterialCodesComponent,
    MaterialCodesGridComponent,
    MaterialCodesEditComponent,
    MaterialCodesEditItemGridComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    MaterialCodesRoutingModule,
    ExpansionPanelComponent,
    ReactiveFormsModule,
    ButtonComponent,
    EditFooterComponent,
    GridWrapperComponent,
    MultiTabWrapperComponent,
    TextboxComponent,
    ToolbarComponent,
    TextareaComponent,
  ],
})
export class MaterialCodesModule {}
