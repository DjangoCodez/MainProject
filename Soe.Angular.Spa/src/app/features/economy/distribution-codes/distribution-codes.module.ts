import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from '@shared/shared.module';
import { DistributionCodesRoutingModule } from './distribution-codes-routing.module';
import { DistributionCodesComponent } from './components/distribution-codes/distribution-codes.component';
import { DistributionCodesGridComponent } from './components/distribution-codes-grid/distribution-codes-grid.component';
import { DistributionCodesEditComponent } from './components/distribution-codes-edit/distribution-codes-edit.component';
import { ButtonComponent } from '@ui/button/button/button.component'
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { LabelComponent } from '@ui/label/label.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ReactiveFormsModule } from '@angular/forms';
import { DistributionCodesEditGridComponent } from './components/distribution-codes-edit/distribution-codes-edit-grid/distribution-codes-edit-grid.component';
@NgModule({
  declarations: [
    DistributionCodesComponent,
    DistributionCodesGridComponent,
    DistributionCodesEditComponent,
    DistributionCodesEditGridComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    DistributionCodesRoutingModule,
    ButtonComponent,
    EditFooterComponent,
    DatepickerComponent,
    GridWrapperComponent,
    SelectComponent,
    NumberboxComponent,
    ReactiveFormsModule,
    MultiTabWrapperComponent,
    TextboxComponent,
    ToolbarComponent,
    LabelComponent,
    ExpansionPanelComponent,
  ],
})
export class DistributionCodesModule {}
