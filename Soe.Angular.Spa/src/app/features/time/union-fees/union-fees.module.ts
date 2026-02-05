import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { UnionFeesEditComponent } from './components/union-fees-edit/union-fees-edit.component';
import { UnionFeesGridComponent } from './components/union-fees-grid/union-fees-grid.component';
import { UnionFeesComponent } from './components/union-fees/union-fees.component';
import { UnionFeesRoutingModule } from './union-fees-routing.module';

@NgModule({
  declarations: [
    UnionFeesComponent,
    UnionFeesGridComponent,
    UnionFeesEditComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    ReactiveFormsModule,
    UnionFeesRoutingModule,
    MultiTabWrapperComponent,
    GridWrapperComponent,
    ToolbarComponent,
    ButtonComponent,
    ExpansionPanelComponent,
    TextboxComponent,
    EditFooterComponent,
    SelectComponent,
  ],
})
export class UnionFeesModule {}
