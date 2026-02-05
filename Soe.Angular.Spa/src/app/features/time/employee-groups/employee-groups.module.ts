import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';

import { ReactiveFormsModule } from '@angular/forms';
import { AccountDimsModule } from '@shared/components/account-dims/account-dims.module';
import { AccountingPrioComponent } from '@shared/components/accounting-prio/accounting-prio/accounting-prio.component';
import { SharedModule } from '@shared/shared.module';
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { InstructionComponent } from '@ui/instruction/instruction.component'
import { LabelComponent } from '@ui/label/label.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TabComponent } from '@ui/tab/tab/tab.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { TimeboxComponent } from '@ui/forms/timebox/timebox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { EgAttestTransitionsGridComponent } from './components/employee-groups-edit/eg-attest-transitions-grid-edit/eg-attest-transitions-grid.component';
import { EgDaytypesGridComponent } from './components/employee-groups-edit/eg-daytypes-grid/eg-daytypes-grid.component';
import { EgDaytypesWeekendpayGridComponent } from './components/employee-groups-edit/eg-daytypes-weekendpay-grid/eg-daytypes-weekendpay-grid.component';
import { EmployeeGroupsTimeDeviationCauseTimeCodeGridComponent } from './components/employee-groups-edit/eg-deviation-cause-time-code-grid/eg-deviation-cause-time-code-grid.component';
import { EgRuleWorkTimePeriodsGridComponent } from './components/employee-groups-edit/eg-rule-work-time-periods-grid/eg-rule-work-time-periods-grid.component';
import { EgTimeAccumulatorGridComponent } from './components/employee-groups-edit/eg-timeaccumulator-grid/eg-timeaccumulator-grid.component';
import { EgTimecodesGridComponent } from './components/employee-groups-edit/eg-timecodes-grid/eg-timecodes-grid.component';
import { EgTimedeviationcauseAbsenceAnnouncementGridComponent } from './components/employee-groups-edit/eg-timedeviationcause-absence-announcement-grid/eg-timedeviationcause-absence-announcement-grid.component';
import { EgTimedeviationcauseRequestGridComponent } from './components/employee-groups-edit/eg-timedeviationcause-request-grid/eg-timedeviationcause-request-grid.component';
import { EgTimedeviationcausesGridComponent } from './components/employee-groups-edit/eg-timedeviationcauses-grid/eg-timedeviationcauses-grid.component';
import { EmployeeGroupsEditComponent } from './components/employee-groups-edit/employee-groups-edit.component';
import { EmployeeGroupsGridComponent } from './components/employee-groups-grid/employee-groups-grid.component';
import { EmployeeGroupsComponent } from './components/employee-groups/employee-groups.component';
import { EmployeeGroupsRoutingModule } from './employee-groups-routing.module';
@NgModule({
  declarations: [
    EmployeeGroupsComponent,
    EmployeeGroupsGridComponent,
    EmployeeGroupsEditComponent,
    EmployeeGroupsTimeDeviationCauseTimeCodeGridComponent,
    EgDaytypesWeekendpayGridComponent,
    EgDaytypesGridComponent,
    EgTimeAccumulatorGridComponent,
    EgTimedeviationcauseRequestGridComponent,
    EgTimedeviationcauseAbsenceAnnouncementGridComponent,
    EgTimecodesGridComponent,
    EgTimedeviationcausesGridComponent,
    EgAttestTransitionsGridComponent,
    EgRuleWorkTimePeriodsGridComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    ReactiveFormsModule,
    EmployeeGroupsRoutingModule,
    TabComponent,
    GridWrapperComponent,
    MultiTabWrapperComponent,
    ToolbarComponent,
    EditFooterComponent,
    TextboxComponent,
    CheckboxComponent,
    SelectComponent,
    ExpansionPanelComponent,
    NumberboxComponent,
    TimeboxComponent,
    LabelComponent,
    AccountDimsModule,
    AccountingPrioComponent,
    InstructionComponent,
  ],
})
export class EmployeeGroupsModule {}
