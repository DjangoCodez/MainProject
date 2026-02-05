import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextareaComponent } from '@ui/forms/textarea/textarea.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { LeisureCodeTypesEditComponent } from './components/leisure-code-types-edit/leisure-code-types-edit.component';
import { LeisureCodeTypesGridComponent } from './components/leisure-code-types-grid/leisure-code-types-grid.component';
import { LeisureCodeTypesComponent } from './components/leisure-code-types/leisure-code-types.component';
import { LeisureCodeTypesRoutingModule } from './leisure-code-types-routing.module';

@NgModule({
  declarations: [
    LeisureCodeTypesComponent,
    LeisureCodeTypesGridComponent,
    LeisureCodeTypesEditComponent,
  ],
  imports: [
    SharedModule,
    CommonModule,
    LeisureCodeTypesRoutingModule,
    ButtonComponent,
    EditFooterComponent,
    GridWrapperComponent,
    SelectComponent,
    ReactiveFormsModule,
    MultiTabWrapperComponent,
    TextboxComponent,
    ToolbarComponent,
    TextareaComponent,
  ],
})
export class LeisureCodeTypesModule {}
