import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { TimeWorkReductionRoutingModule } from './time-work-reduction-routing.module';
import { TimeWorkReductionComponent } from './components/time-work-reduction/time-work-reduction.component';
import { TimeWorkReductionGridComponent } from './components/time-work-reduction-grid/time-work-reduction-grid.component';
import { TimeWorkReductionEditComponent } from './components/time-work-reduction-edit/time-work-reduction-edit.component';
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridComponent } from '@ui/grid/grid.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MenuButtonComponent } from '@ui/button/menu-button/menu-button.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { SharedModule } from '@shared/shared.module';
import { ReactiveFormsModule } from '@angular/forms';
import { TimeWorkReductionReconciliationDialogComponent } from './components/time-work-reduction-reconciliation-dialog/time-work-reduction-reconciliation-dialog.component';
import { TimeWorkReductionYearGridComponent } from './components/time-work-reduction-year-grid/time-work-reduction-year-grid.component';
import { TimeWorkReductionReconcilationGenerateOutcome } from './components/time-work-reduction-reconcilation-generate-outcome/time-work-reduction-reconcilation-generate-outcome.component';

@NgModule({
  declarations: [
    TimeWorkReductionComponent,
    TimeWorkReductionGridComponent,
    TimeWorkReductionEditComponent,
    TimeWorkReductionReconciliationDialogComponent,
    TimeWorkReductionYearGridComponent,
    TimeWorkReductionReconcilationGenerateOutcome,
  ],
  imports: [
    CommonModule,
    TimeWorkReductionRoutingModule,
    MultiTabWrapperComponent,
    ToolbarComponent,
    SharedModule,
    ReactiveFormsModule,
    GridWrapperComponent,
    ButtonComponent,
    ExpansionPanelComponent,
    TextboxComponent,
    CheckboxComponent,
    EditFooterComponent,
    SelectComponent,
    DialogComponent,
    DatepickerComponent,
    SaveButtonComponent,
    GridComponent,
    MenuButtonComponent,
  ],
})
export class TimeWorkReductionModule {}
