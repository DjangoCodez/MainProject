import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TimeWorkAccountEditComponent } from './components/time-work-account-edit/time-work-account-edit.component';
import { TimeWorkAccountGridComponent } from './components/time-work-account-grid/time-work-account-grid.component';
import { TimeWorkAccountComponent } from './components/time-work-account/time-work-account.component';
import { TimeWorkAccountRoutingModule } from './time-work-account-routing.module';
import { ButtonComponent } from '@ui/button/button/button.component';
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component';
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component';
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component';
import { GridComponent } from '@ui/grid/grid.component';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { MenuButtonComponent } from '@ui/button/menu-button/menu-button.component';
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component';
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component';
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component';
import { SelectComponent } from '@ui/forms/select/select.component';
import { TextboxComponent } from '@ui/forms/textbox/textbox.component';
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { TimeWorkAccountYearWorkTimeWeekEditComponent } from './components/time-work-account-year-worktimeweek-edit/time-work-account-year-worktimeweek-edit.component';
import { TimeWorkAccountYearEditComponent } from './components/time-work-account-year-edit/time-work-account-year-edit.component';
import { TimeWorkAccountOutputComponent } from './components/time-work-account-output/time-work-account-output.component';
import { TimeWorkAcoountYearGenerateOutcomeComponent } from './components/time-work-account-year-generate-outcome/time-work-account-year-generate-outcome.component';
import { TimeWorkAccountYearGridComponent } from './components/time-work-account-year-grid/time-work-account-year-grid.component';

@NgModule({
  declarations: [
    TimeWorkAccountComponent,
    TimeWorkAccountEditComponent,
    TimeWorkAccountYearEditComponent,
    TimeWorkAccountYearWorkTimeWeekEditComponent,
    TimeWorkAccountGridComponent,
    TimeWorkAccountOutputComponent,
    TimeWorkAcoountYearGenerateOutcomeComponent,
    TimeWorkAccountYearGridComponent,
  ],
  exports: [
    TimeWorkAccountComponent,
    TimeWorkAccountEditComponent,
    TimeWorkAccountYearEditComponent,
    TimeWorkAccountYearWorkTimeWeekEditComponent,
    TimeWorkAccountGridComponent,
    TimeWorkAccountOutputComponent,
    TimeWorkAcoountYearGenerateOutcomeComponent,
  ],
  imports: [
    SharedModule,
    TimeWorkAccountRoutingModule,
    MultiTabWrapperComponent,
    DialogComponent,
    TextboxComponent,
    NumberboxComponent,
    SelectComponent,
    CheckboxComponent,
    EditFooterComponent,
    ReactiveFormsModule,
    ButtonComponent,
    MenuButtonComponent,
    SaveButtonComponent,
    ToolbarComponent,
    GridComponent,
    GridWrapperComponent,
    DatepickerComponent,
    ExpansionPanelComponent,
    CommonModule,
  ],
})
export class TimeWorkAccountModule {}
