import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { PositionsEditComponent } from './components/positions-edit/positions-edit.component';
import { PositionsGridComponent } from './components/positions-grid/positions-grid.component';
import { PositionsComponent } from './components/positions/positions.component';
import { PositionsRoutingModule } from './positions-routing.module';

@NgModule({
  declarations: [
    PositionsComponent,
    PositionsGridComponent,
    PositionsEditComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    PositionsRoutingModule,
    ButtonComponent,
    EditFooterComponent,
    GridWrapperComponent,
    SelectComponent,
    ReactiveFormsModule,
    MultiTabWrapperComponent,
    TextboxComponent,
    ToolbarComponent,
  ],
})
export class PositionsModule {}
