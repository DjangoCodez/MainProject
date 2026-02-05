import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { AttestationGroupsRoutingModule } from './attestation-groups-routing.module';
import { AttestationGroupsComponent } from './components/attestation-groups/attestation-groups.component';
import { AttestationGroupEditComponent } from './components/attestation-group-edit/attestation-group-edit.component';
import { AttestationGroupsGridComponent } from './components/attestation-groups-grid/attestation-groups-grid.component';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { SharedModule } from '@shared/shared.module';
import { ReactiveFormsModule } from '@angular/forms';
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component';
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component';
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { InstructionComponent } from '@ui/instruction/instruction.component';
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component';
import { SelectComponent } from '@ui/forms/select/select.component';
import { TextboxComponent } from '@ui/forms/textbox/textbox.component';
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { UserSelectorForTemplateHeadRowComponent } from './components/attestation-group-edit/user-selector-for-template-head-row/user-selector-for-template-head-row.component';

@NgModule({
  declarations: [
    AttestationGroupsComponent,
    AttestationGroupEditComponent,
    AttestationGroupsGridComponent,
    UserSelectorForTemplateHeadRowComponent,
  ],
  imports: [
    CommonModule,
    AttestationGroupsRoutingModule,
    ReactiveFormsModule,
    MultiTabWrapperComponent,
    GridWrapperComponent,
    EditBaseDirective,
    EditFooterComponent,
    ToolbarComponent,
    SharedModule,
    ExpansionPanelComponent,
    TextboxComponent,
    CheckboxComponent,
    SelectComponent,
    InstructionComponent,
  ],
})
export class AttestationGroupsModule {}
