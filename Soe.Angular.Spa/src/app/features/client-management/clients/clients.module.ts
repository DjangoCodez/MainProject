import { NgModule } from '@angular/core';
import { ClientsComponent } from './components/clients/clients.component';
import { ClientsGridComponent } from './components/clients-grid/clients-grid.component';
import { ClientsRoutingModule } from './clients-routing.module';
import { SharedModule } from '@shared/shared.module';
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';

@NgModule({
  declarations: [ClientsComponent],
  imports: [
    ClientsGridComponent,
    ClientsRoutingModule,
    SharedModule,
    MultiTabWrapperComponent,
    GridComponent,
    ToolbarComponent,
  ],
})
export class ClientsModule {}
