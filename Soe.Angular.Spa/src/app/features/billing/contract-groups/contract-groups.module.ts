import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ContractGroupsComponent } from './components/contract-groups/contract-groups.component';
import { ContractGroupsGridComponent } from './components/contract-groups-grid/contract-groups-grid.component';
import { ContractGroupsEditComponent } from './components/contract-groups-edit/contract-groups-edit.component';
import { ContractGroupsRoutingModule } from './contract-groups-routing.module';
import { SharedModule } from '@shared/shared.module';
import { ReactiveFormsModule } from '@angular/forms';
import { ButtonComponent } from '@ui/button/button/button.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { LabelComponent } from '@ui/label/label.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextareaComponent } from '@ui/forms/textarea/textarea.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';

@NgModule({
  declarations: [
    ContractGroupsComponent,
    ContractGroupsGridComponent,
    ContractGroupsEditComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    ContractGroupsRoutingModule,
    ReactiveFormsModule,
    ButtonComponent,
    GridWrapperComponent,
    ToolbarComponent,
    MultiTabWrapperComponent,
    TextareaComponent,
    TextboxComponent,
    NumberboxComponent,
    LabelComponent,
    SelectComponent,
    EditFooterComponent,
    ExpansionPanelComponent,
  ],
})
export class ContractGroupsModule {}
