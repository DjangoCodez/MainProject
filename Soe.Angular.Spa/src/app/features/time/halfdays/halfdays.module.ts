import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { MultiSelectGridModule } from '@shared/components/multi-select-grid';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { HalfdaysEditComponent } from './components/halfdays-edit/halfdays-edit.component';
import { HalfdaysGridComponent } from './components/halfdays-grid/halfdays-grid.component';
import { HalfdaysComponent } from './components/halfdays/halfdays.component';
import { HalfdaysRoutingModule } from './halfdays-routing.module';

@NgModule({
  declarations: [
    HalfdaysComponent,
    HalfdaysEditComponent,
    HalfdaysGridComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    HalfdaysRoutingModule,
    ButtonComponent,
    EditFooterComponent,
    GridWrapperComponent,
    MultiTabWrapperComponent,
    TextboxComponent,
    SelectComponent,
    ExpansionPanelComponent,
    ToolbarComponent,
    ReactiveFormsModule,
    MultiSelectGridModule,
    NumberboxComponent,
  ],
})
export class HalfdaysModule {}
