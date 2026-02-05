import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AccountingLiquidityPlanningRoutingModule } from './accounting-liquidity-planning-routing.module';
import { AccountingLiquidityPlanningComponent } from './components/accounting-liquidity-planning/accounting-liquidity-planning.component';
import { AccountingLiquidityPlanningGridComponent } from './components/accounting-liquidity-planning-grid/accounting-liquidity-planning-grid.component';
import { ButtonComponent } from '@ui/button/button/button.component';
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component';
import { DeleteButtonComponent } from '@ui/button/delete-button/delete-button.component';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component';
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { IconModule } from '@ui/icon/icon.module';
import { MultiSelectComponent } from '@ui/forms/select/multi-select/multi-select.component';
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component';
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component';
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component';
import { TextboxComponent } from '@ui/forms/textbox/textbox.component';
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { AccountingLiquidityPlanningGridFilterComponent } from './components/accounting-liquidity-planning-grid-filter/accounting-liquidity-planning-grid-filter.component';
import { ReactiveFormsModule } from '@angular/forms';
import { ManualTransactionComponent } from './components/manual-transaction/manual-transaction.component';
import { ChartComponent } from '@ui/chart/chart.component';

@NgModule({
  declarations: [
    AccountingLiquidityPlanningComponent,
    AccountingLiquidityPlanningGridComponent,
    AccountingLiquidityPlanningGridFilterComponent,
    ManualTransactionComponent,
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    AccountingLiquidityPlanningRoutingModule,
    MultiTabWrapperComponent,
    GridWrapperComponent,
    ExpansionPanelComponent,
    ToolbarComponent,
    DatepickerComponent,
    TextboxComponent,
    ButtonComponent,
    SaveButtonComponent,
    DeleteButtonComponent,
    DialogComponent,
    IconModule,
    EditFooterComponent,
    MultiSelectComponent,
    NumberboxComponent,
    ChartComponent,
  ],
})
export class AccountingLiquidityPlanningModule {}
