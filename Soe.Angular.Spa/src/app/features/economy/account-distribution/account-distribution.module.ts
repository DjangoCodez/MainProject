import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { InstructionComponent } from '@ui/instruction/instruction.component'
import { LabelComponent } from '@ui/label/label.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { InventoryWriteoffsModule } from '../inventory-writeoffs/inventory-writeoffs.module';
import { AccountDistributionComponent } from './components/account-distribution/account-distribution.component';
import { AccountDistributionEditComponent } from './components/account-distribution-edit/account-distribution-edit.component';
import { AccountDistributionGridComponent } from './components/account-distribution-grid/account-distribution-grid.component';
import { AccountDistributionRoutingModule } from './account-distribution-routing.module';
import { SharedModule } from '@shared/shared.module';
import { ReactiveFormsModule } from '@angular/forms';
import { TraceRowsModule } from '@shared/components/trace-rows/trace-rows.module';
import { DistributionRowsComponent } from './components/distribution-rows/distribution-rows.component';
import { AccountDistributionUrlParamsService } from './services/account-distribution-params.service';

@NgModule({
  declarations: [
    AccountDistributionComponent,
    AccountDistributionEditComponent,
    AccountDistributionGridComponent,
    DistributionRowsComponent,
  ],
  imports: [
    CommonModule,
    AccountDistributionRoutingModule,
    InventoryWriteoffsModule,
    TraceRowsModule,
    SharedModule,
    MultiTabWrapperComponent,
    ToolbarComponent,
    TextboxComponent,
    SelectComponent,
    ReactiveFormsModule,
    EditFooterComponent,
    GridWrapperComponent,
    ButtonComponent,
    SharedModule,
    CommonModule,
    ExpansionPanelComponent,
    LabelComponent,
    NumberboxComponent,
    CheckboxComponent,
    InstructionComponent,
    DatepickerComponent,
  ],
  providers: [AccountDistributionUrlParamsService],
})
export class AccountDistributionModule {}
