import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { InstructionComponent } from '@ui/instruction/instruction.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { StaffingNeedsLocationsGridComponent } from './components/staffing-needs-loactions-grid/staffing-needs-locations-grid.component';
import { StaffingNeedsLocationGroupsEditComponent } from './components/staffing-needs-location-groups-edit/staffing-needs-location-groups-edit.component';
import { StaffingNeedsLocationGroupsGridComponent } from './components/staffing-needs-location-groups-grid/staffing-needs-location-groups-grid.component';
import { StaffingNeedsLocationsEditComponent } from './components/staffing-needs-locations-edit/staffing-needs-locations-edit.component';
import { StaffingNeedsRulesEditGridComponent } from './components/staffing-needs-rules-edit/staffing-needs-rules-edit-grid/staffing-needs-rules-edit-grid.component';
import { StaffingNeedsRulesEditComponent } from './components/staffing-needs-rules-edit/staffing-needs-rules-edit.component';
import { StaffingNeedsRulesGridComponent } from './components/staffing-needs-rules-grid/staffing-needs-rules-grid.component';
import { StaffingNeedsLocationGroupsComponent } from './pages/staffing-needs-location-groups/staffing-needs-location-groups.component';
import { StaffingNeedsLocationsComponent } from './pages/staffing-needs-locations/staffing-needs-locations.component';
import { StaffingNeedsRulesComponent } from './pages/staffing-needs-rules/staffing-needs-rules.component';
import { StaffingNeedsRoutingModule } from './staffing-needs-routing.module';

@NgModule({
  declarations: [
    StaffingNeedsLocationGroupsComponent,
    StaffingNeedsLocationGroupsEditComponent,
    StaffingNeedsLocationGroupsGridComponent,
    StaffingNeedsLocationsComponent,
    StaffingNeedsLocationsEditComponent,
    StaffingNeedsLocationsGridComponent,
    StaffingNeedsRulesComponent,
    StaffingNeedsRulesEditComponent,
    StaffingNeedsRulesGridComponent,
    StaffingNeedsRulesEditGridComponent,
  ],
  exports: [
    StaffingNeedsLocationGroupsComponent,
    StaffingNeedsLocationGroupsEditComponent,
    StaffingNeedsLocationGroupsGridComponent,
    StaffingNeedsLocationsComponent,
    StaffingNeedsLocationsEditComponent,
    StaffingNeedsLocationsGridComponent,
    StaffingNeedsRulesComponent,
    StaffingNeedsRulesEditComponent,
    StaffingNeedsRulesGridComponent,
  ],
  imports: [
    SharedModule,
    StaffingNeedsRoutingModule,
    CommonModule,
    TextboxComponent,
    ButtonComponent,
    ReactiveFormsModule,
    InstructionComponent,
    EditFooterComponent,
    SelectComponent,
    ToolbarComponent,
    GridWrapperComponent,
    MultiTabWrapperComponent,
  ],
})
export class StaffingNeedsModule {}
