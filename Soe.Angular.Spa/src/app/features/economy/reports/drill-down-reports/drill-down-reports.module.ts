import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { DrillDownReportsRoutingModule } from './drill-down-reports-routing.module';
import { DrillDownReportSearchComponent } from './components/drill-down-reports-search/drill-down-reports-search.component';
import { ButtonComponent } from '@ui/button/button/button.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SelectComponent } from '@ui/forms/select/select.component';
import { SharedModule } from '@shared/shared.module';
import { ReactiveFormsModule } from '@angular/forms';
import { DrillDownReportsComponent } from './components/drill-down-reports/drill-down-reports.component';
import { DrillDownReportsGridComponent } from './components/drill-down-reports-grid/drill-down-reports-grid.component';
import { VoucherModule } from '@features/economy/voucher/voucher.module';

@NgModule({
  declarations: [
    DrillDownReportsComponent,
    DrillDownReportsGridComponent,
    DrillDownReportSearchComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    DrillDownReportsRoutingModule,
    SelectComponent,
    ButtonComponent,
    ReactiveFormsModule,
    GridWrapperComponent,
    MultiTabWrapperComponent,
    VoucherModule,
  ],
})
export class DrillDownReportsModule {}
