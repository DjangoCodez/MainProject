import { NgModule } from '@angular/core';
import { SupplierInvoiceMatchesRoutingModule } from './supplier-invoice-matches-routing.module';
import { SupplierInvoiceMatchesGridComponent } from './components/supplier-invoice-matches-grid/supplier-invoice-matches-grid.component';
import { SupplierInvoiceMatchesComponent } from './components/supplier-invoice-matches/supplier-invoice-matches.component';
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { ActorInvoiceMatchesModule } from '../shared/actor-invoice-matches/actor-invoice-matches.module';
import { FlowHandlerService } from '@shared/services/flow-handler.service';

@NgModule({
  declarations: [
    SupplierInvoiceMatchesGridComponent,
    SupplierInvoiceMatchesComponent,
  ],
  imports: [
    SupplierInvoiceMatchesRoutingModule,
    MultiTabWrapperComponent,
    ActorInvoiceMatchesModule,
  ],
  providers: [FlowHandlerService, ToolbarService],
})
export class SupplierInvoiceMatchesModule {}
