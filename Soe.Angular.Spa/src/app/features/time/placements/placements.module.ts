import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';

import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { DialogFooterComponent } from '@ui/footer/dialog-footer/dialog-footer.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { PlacementsGridFooterComponent } from './components/placements-grid/placements-grid-footer/placements-grid-footer.component';
import { PlacementsGridComponent } from './components/placements-grid/placements-grid.component';
import { PlacementsComponent } from './components/placements/placements.component';
import { PlacementsRoutingModule } from './placements-routing.module';
import { PlacementsControlDialogComponent } from '../../../shared/components/time/placements-control-dialog/placements-control-dialog.component';
import { PlacementsControlDialogGridComponent } from '../../../shared/components/time/placements-control-dialog/placements-control-dialog-grid/placements-control-dialog-grid.component';
import { PlacementsRecalculateStatusDialogComponent } from '@shared/components/time/placements-recalculate-status-dialog/placements-recalculate-status-dialog.component';

@NgModule({
  declarations: [PlacementsComponent, PlacementsGridComponent],
  imports: [
    PlacementsGridFooterComponent,
    PlacementsControlDialogComponent,
    PlacementsControlDialogGridComponent,
    SharedModule,
    CommonModule,
    PlacementsRoutingModule,
    MultiTabWrapperComponent,
    GridWrapperComponent,
    CheckboxComponent,
    ToolbarComponent,
    ButtonComponent,
    SelectComponent,
    ReactiveFormsModule,
    SaveButtonComponent,
    EditFooterComponent,
    DatepickerComponent,
    DialogComponent,
    DialogFooterComponent,
    PlacementsRecalculateStatusDialogComponent,
  ],
})
export class PlacementsModule {}
