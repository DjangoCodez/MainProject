import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ServiceUsersComponent } from './components/service-users/service-users.component';
import { ServiceUsersGridComponent } from './components/service-users-grid/service-users-grid.component';
import { ServiceUsersEditComponent } from './components/service-users-edit/service-users-edit.component';
import { ServiceUserRoutingModule } from './service-users-routing.module';
import { SharedModule } from '@shared/shared.module';
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { TextboxComponent } from '@ui/forms/textbox/textbox.component';
import { SelectComponent } from '@ui/forms/select/select.component';
import { MultiSelectComponent } from '@ui/forms/select/multi-select/multi-select.component';
import { InstructionComponent } from '@ui/instruction/instruction.component';
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component';
import { ReactiveFormsModule } from '@angular/forms';
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component';

@NgModule({
  declarations: [ServiceUsersComponent, ServiceUsersEditComponent],
  imports: [
    CommonModule,
    ServiceUsersGridComponent,
    ServiceUserRoutingModule,
    SharedModule,
    MultiTabWrapperComponent,
    GridComponent,
    ToolbarComponent,
    TextboxComponent,
    SelectComponent,
    MultiSelectComponent,
    InstructionComponent,
    EditFooterComponent,
    ReactiveFormsModule,
    ExpansionPanelComponent,
  ],
})
export class ServiceUsersModule {}
