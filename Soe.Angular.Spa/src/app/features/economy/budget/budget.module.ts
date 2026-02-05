import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BudgetComponent } from './components/budget/budget.component';
import { BudgetGridComponent } from './components/budget-grid/budget-grid.component';
import { BudgetEditComponent } from './components/budget-edit/budget-edit.component';
import { SharedModule } from '@shared/shared.module';
import { BudgetRoutingModule } from './budget-routing.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { LabelComponent } from '@ui/label/label.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ReactiveFormsModule } from '@angular/forms';
import { BudgetEditGridComponent } from './components/budget-edit/budget-edit-grid/budget-edit-grid.component';
import { BillingModule } from '../../billing/billing.module';
import { EditLoadResultComponent } from './components/edit-load-result/edit-load-result.component';

@NgModule({
  declarations: [
    BudgetComponent,
    BudgetGridComponent,
    BudgetEditComponent,
    BudgetEditGridComponent,
    EditLoadResultComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    BudgetRoutingModule,
    MultiTabWrapperComponent,
    ToolbarComponent,
    GridWrapperComponent,
    ExpansionPanelComponent,
    ReactiveFormsModule,
    ButtonComponent,
    SaveButtonComponent,
    TextboxComponent,
    SelectComponent,
    CheckboxComponent,
    LabelComponent,
    EditFooterComponent,
    BillingModule,
    DialogComponent,
  ],
})
export class BudgetModule {}
