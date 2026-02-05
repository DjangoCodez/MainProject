import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { InstructionComponent } from '@ui/instruction/instruction.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { TimeboxComponent } from '@ui/forms/timebox/timebox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { AnnualLeaveGroupsRoutingModule } from './annual-leave-groups-routing.module';
import { AnnualLeaveGroupsEditComponent } from './components/annual-leave-groups-edit/annual-leave-groups-edit.component';
import { AnnualLeaveGroupsTypeModalComponent } from './components/annual-leave-groups-edit/annual-leave-groups-type-modal/annual-leave-groups-type-modal.component';
import { AnnualLeaveGroupsGridComponent } from './components/annual-leave-groups-grid/annual-leave-groups-grid.component';
import { AnnualLeaveGroupsComponent } from './components/annual-leave-groups/annual-leave-groups.component';

@NgModule({
  declarations: [
    AnnualLeaveGroupsComponent,
    AnnualLeaveGroupsGridComponent,
    AnnualLeaveGroupsEditComponent,
    AnnualLeaveGroupsTypeModalComponent,
  ],
  imports: [
    SharedModule,
    CommonModule,
    AnnualLeaveGroupsRoutingModule,
    ButtonComponent,
    CheckboxComponent,
    EditFooterComponent,
    GridWrapperComponent,
    SelectComponent,
    ReactiveFormsModule,
    MultiTabWrapperComponent,
    TextboxComponent,
    ToolbarComponent,
    NumberboxComponent,
    InstructionComponent,
    TimeboxComponent,
    DialogComponent,
  ],
})
export class AnnualLeaveGroupsModule {}
