import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { TimeboxComponent } from '@ui/forms/timebox/timebox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { DistributionRulesEditComponent } from './components/distribution-rules-edit/distribution-rules-edit.component';
import { DistributionRulesGridComponent } from './components/distribution-rules-grid/distribution-rules-grid.component';
import { PlanningPeriodsEditComponent } from './components/planning-periods-edit/planning-periods-edit.component';
import { PlanningPeriodsGridComponent } from './components/planning-periods-grid/planning-periods-grid.component';
import { DrRulesGridComponent } from './components/distribution-rules-edit/dr-rules-grid/dr-rules-grid.component';
import { PpTimeperiodsGridComponent } from './components/planning-periods-edit/pp-timeperiods-grid/pp-timeperiods-grid.component';
import { PlanningPeriodsComponent } from './components/planning-periods/planning-period.component';
import { PlanningPeriodsRoutingModule } from './planning-periods-routing.module';

@NgModule({
  declarations: [
    PlanningPeriodsComponent,
    PlanningPeriodsGridComponent,
    PlanningPeriodsEditComponent,
    DistributionRulesGridComponent,
    DistributionRulesEditComponent,
    PpTimeperiodsGridComponent,
    DrRulesGridComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    ReactiveFormsModule,
    MultiTabWrapperComponent,
    GridWrapperComponent,
    PlanningPeriodsRoutingModule,
    ToolbarComponent,
    TextboxComponent,
    EditFooterComponent,
    SelectComponent,
    ExpansionPanelComponent,
    ButtonComponent,
    SaveButtonComponent,
    DatepickerComponent,
    DialogComponent,
    TimeboxComponent,
  ],
})
export class PlanningPeriodsModule {}
