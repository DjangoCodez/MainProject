import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { AccountingReconciliationRoutingModule } from './accounting-reconciliation-routing.module';
import { AccountingReconciliationComponent } from './compoents/accounting-reconciliation/accounting-reconciliation.component';
import { AccountingReconciliationGridComponent } from './compoents/accounting-reconciliation-grid/accounting-reconciliation-grid.component';
import { AccountingReconciliationGridFilterComponent } from './compoents/accounting-reconciliation-grid/accounting-reconciliation-grid-filter/accounting-reconciliation-grid-filter.component';
import { AccountingReconciliationEditComponent } from './compoents/accounting-reconciliation-edit/accounting-reconciliation-edit.component';
import { ButtonComponent } from '@ui/button/button/button.component'
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridComponent } from '@ui/grid/grid.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ReactiveFormsModule } from '@angular/forms';
import { VoucherModule } from '../voucher/voucher.module';

@NgModule({
  declarations: [
    AccountingReconciliationComponent,
    AccountingReconciliationGridComponent,
    AccountingReconciliationGridFilterComponent,
    AccountingReconciliationEditComponent,
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    AccountingReconciliationRoutingModule,
    GridComponent,
    ToolbarComponent,
    MultiTabWrapperComponent,
    SelectComponent,
    DatepickerComponent,
    ButtonComponent,
    ExpansionPanelComponent,
    GridWrapperComponent,
    VoucherModule,
  ],
})
export class AccountingReconciliationModule {}
