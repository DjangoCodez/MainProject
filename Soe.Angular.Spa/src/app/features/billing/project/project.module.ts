import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProjectRoutingModule } from './project-routing.module';
import { ProjectComponent } from './components/project/project.component';
import { ProjectGridComponent } from './components/project-grid/project-grid.component';
import { ProjectEditComponent } from './components/project-edit/project-edit.component';
import { SharedModule } from '@shared/shared.module';
import { ReactiveFormsModule } from '@angular/forms';
import { AutocompleteComponent } from '@ui/forms/autocomplete/autocomplete.component';
import { ButtonComponent } from '@ui/button/button/button.component';
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component';
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component';
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { IconButtonComponent } from '@ui/button/icon-button/icon-button.component';
import { InstructionComponent } from '@ui/instruction/instruction.component';
import { LabelComponent } from '@ui/label/label.component';
import { MenuButtonComponent } from '@ui/button/menu-button/menu-button.component';
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component';
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component';
import { SelectComponent } from '@ui/forms/select/select.component';
import { TextareaComponent } from '@ui/forms/textarea/textarea.component';
import { TextboxComponent } from '@ui/forms/textbox/textbox.component';
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ProjectGridHeaderComponent } from './components/project-grid/project-grid-header/project-grid-header.component';
import { AccountDimsModule } from '@shared/components/account-dims/account-dims.module';
import { CategoriesModule } from '@shared/components/categories/categories.module';
import { AccountingSettingsModule } from '@shared/components/accounting-settings/accounting-settings.module';
import { ProjectPricelistComponent } from './components/project-edit/project-pricelist/project-pricelist.component';
import { ProjectPricelistHeaderComponent } from './components/project-edit/project-pricelist/project-pricelist-header/project-pricelist-header.component';
import { PricelistTypeDialogModule } from '@shared/components/pricelist-type-dialog/pricelist-type-dialog.module';
import { TraceRowsModule } from '@shared/components/trace-rows/trace-rows.module';
import { ProjectPersonsComponent } from './components/project-edit/project-persons/project-persons/project-persons.component';
import { ProjectPersonDialogComponent } from './components/project-edit/project-persons/project-person-dialog/project-person-dialog.component';
import { ProjectBudgetheadGridComponent } from './components/project-edit/project-budgethead-grid/project-budgethead-grid.component';
import { ChangePeriodDialogData } from '../project-budget/components/budget-edit/change-period-modal/change-period-modal.model';
import { ChangePeriodModal } from '../project-budget/components/budget-edit/change-period-modal/change-period-modal.component';
import { MultiSelectComponent } from '@ui/forms/select/multi-select/multi-select.component';
import { FileDisplayModule } from '@shared/components/file-display/file-display.module';

@NgModule({
  declarations: [
    ProjectComponent,
    ProjectGridComponent,
    ProjectEditComponent,
    ProjectGridHeaderComponent,
    ProjectPricelistComponent,
    ProjectPricelistHeaderComponent,
    ProjectPersonsComponent,
    ProjectPersonDialogComponent,
    ProjectBudgetheadGridComponent,
    ChangePeriodModal,
  ],
  imports: [
    CommonModule,
    ProjectRoutingModule,
    SharedModule,
    ReactiveFormsModule,
    MultiTabWrapperComponent,
    ToolbarComponent,
    GridWrapperComponent,
    MultiSelectComponent,
    SelectComponent,
    CheckboxComponent,
    ButtonComponent,
    IconButtonComponent,
    MenuButtonComponent,
    SaveButtonComponent,
    LabelComponent,
    ExpansionPanelComponent,
    TextboxComponent,
    DatepickerComponent,
    AutocompleteComponent,
    AccountDimsModule,
    CategoriesModule,
    AccountingSettingsModule,
    TextareaComponent,
    PricelistTypeDialogModule,
    TraceRowsModule,
    EditFooterComponent,
    DialogComponent,
    ReactiveFormsModule,
    InstructionComponent,
    FileDisplayModule,
  ],
})
export class ProjectModule {}
