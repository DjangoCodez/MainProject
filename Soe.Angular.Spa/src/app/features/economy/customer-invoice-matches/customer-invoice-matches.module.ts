import { NgModule } from '@angular/core';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { ActorInvoiceMatchesModule } from '../shared/actor-invoice-matches/actor-invoice-matches.module';
import { CustomerInvoiceMatchesGridComponent } from './components/customer-invoice-matches-grid/customer-invoice-matches-grid.component';
import { CustomerInvoiceMatchesComponent } from './components/customer-invoice-matches/customer-invoice-matches.component';
import { CustomerInvoiceMatchesRoutingModule } from './customer-invoice-matches-routing.module';

@NgModule({
  declarations: [
    CustomerInvoiceMatchesComponent,
    CustomerInvoiceMatchesGridComponent,
  ],
  imports: [
    CustomerInvoiceMatchesRoutingModule,
    MultiTabWrapperComponent,
    ActorInvoiceMatchesModule,
  ],
  providers: [FlowHandlerService, ToolbarService],
})
export class CustomerInvoiceMatchesModule {}
