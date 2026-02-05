import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';

import { SharedModule } from '@shared/shared.module';
import { GridComponent } from '@ui/grid/grid.component';
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component';
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { SupplierInvoicesOverview } from './components/supplier-invoices-overview/supplier-invoices-overview';
import { SupplierInvoicesOverviewRoutingModule } from './supplier-invoices-overview-routing-module';
import { SupplierInvoicesOverviewGrid } from './components/supplier-invoices-overview-grid/supplier-invoices-overview-grid';

@NgModule({
  declarations: [SupplierInvoicesOverview],
  imports: [
    CommonModule,
    SharedModule,
    MultiTabWrapperComponent,
    GridComponent,
    ToolbarComponent,
    SupplierInvoicesOverviewRoutingModule,
    SupplierInvoicesOverviewGrid,
  ],
})
export class SupplierInvoicesOverviewModule {}
