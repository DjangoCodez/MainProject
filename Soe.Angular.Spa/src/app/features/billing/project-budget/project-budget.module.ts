import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProjectBudgetEditComponent } from './components/budget-edit/project-budget-edit.component';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component';
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component';
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component';
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { LabelComponent } from '@ui/label/label.component';
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component';
import { SelectComponent } from '@ui/forms/select/select.component';
import { TextboxComponent } from '@ui/forms/textbox/textbox.component';
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ReactiveFormsModule } from '@angular/forms';
import { BillingModule } from '../billing.module';
import { DialogModule } from '@angular/cdk/dialog';
import { ProjectBudgetEditSimpleGridComponent } from './components/budget-edit/project-budget-edit-simple-grid/project-budget-edit-simple-grid.component';

@NgModule({
  imports: [
    CommonModule,
    SharedModule,
    DialogModule,
    MultiTabWrapperComponent,
    ToolbarComponent,
    GridWrapperComponent,
    ExpansionPanelComponent,
    ReactiveFormsModule,
    ButtonComponent,
    DatepickerComponent,
    TextboxComponent,
    SelectComponent,
    CheckboxComponent,
    LabelComponent,
    EditFooterComponent,
    BillingModule,
    DialogComponent,
    ProjectBudgetEditComponent,
    ProjectBudgetEditSimpleGridComponent,
  ],
})
export class BudgetModule {}
