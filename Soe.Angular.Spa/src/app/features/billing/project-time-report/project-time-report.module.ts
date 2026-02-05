import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProjectTimeReportRoutingModule } from './project-time-report-routing.module';
import { ReactiveFormsModule } from '@angular/forms';
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { IconButtonComponent } from '@ui/button/icon-button/icon-button.component'
import { IconModule } from '@ui/icon/icon.module'
import { LabelComponent } from '@ui/label/label.component'
import { MenuButtonComponent } from '@ui/button/menu-button/menu-button.component'
import { MultiSelectComponent } from '@ui/forms/select/multi-select/multi-select.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextareaComponent } from '@ui/forms/textarea/textarea.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ProjectTimeReportGridComponent } from './components/project-time-report-grid/project-time-report-grid.component';
import { ProjectTimeReportComponent } from './components/project-time-report/project-time-report.component';
import { EditNoteDialogComponent } from './components/project-time-report-grid/edit-note-dialog/edit-note-dialog.component';
import { ProjectTimeReportGridHeaderComponent } from './components/project-time-report-grid-header/project-time-report-grid-header.component';
import { CustomerModule } from '@shared/features/customer/customer.module';
import { ProjectTimeReportEditGridComponent } from './components/project-time-report-edit/project-time-report-edit-grid/project-time-report-edit-grid.component';
import { ProjectTimeReportEditDialogComponent } from './components/project-time-report-edit/project-time-report-edit.dialog/project-time-report-edit.dialog.component';
import { ProjectExpensesGridComponent } from './components/project-expenses/project-expenses-grid/project-expenses-grid.component';
import { ProjectExpensesEditComponent } from './components/project-expenses/project-expenses-edit/project-expenses-edit.component';
import { EmployeeInfoDialogComponent } from './components/project-time-report-edit/employee-info-dialog/employee-info-dialog.component';
import { ProjectWeekReportGridComponent } from './components/project-week-report/project-week-report-grid/project-week-report-grid.component';
import { ProjectWeekReportGridHeaderComponent } from './components/project-week-report/project-week-report-grid-header/project-week-report-grid-header.component';
import { DatePeriodModule } from '@shared/components/date-period/date-period.module';

@NgModule({
  declarations: [
    ProjectTimeReportComponent,
    ProjectTimeReportGridComponent,
    ProjectTimeReportGridHeaderComponent,
    EditNoteDialogComponent,
    EmployeeInfoDialogComponent,
    ProjectTimeReportEditGridComponent,
    ProjectTimeReportEditDialogComponent,
    ProjectExpensesGridComponent,
    ProjectExpensesEditComponent,
    ProjectWeekReportGridComponent,
    ProjectWeekReportGridHeaderComponent,
  ],
  imports: [
    CommonModule,
    ProjectTimeReportRoutingModule,
    EditFooterComponent,
    MultiTabWrapperComponent,
    ToolbarComponent,
    TextboxComponent,
    ReactiveFormsModule,
    GridWrapperComponent,
    ButtonComponent,
    IconButtonComponent,
    MenuButtonComponent,
    SaveButtonComponent,
    ExpansionPanelComponent,
    DatepickerComponent,
    IconModule,
    SelectComponent,
    CheckboxComponent,
    DialogComponent,
    CustomerModule,
    TextareaComponent,
    LabelComponent,
    MultiSelectComponent,
    DatePeriodModule,
  ],
})
export class ProjectTimeReportModule {}
