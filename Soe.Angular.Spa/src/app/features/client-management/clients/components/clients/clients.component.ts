import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { ClientsGridComponent } from '../clients-grid/clients-grid.component';

@Component({
  templateUrl: './clients.component.html',
  standalone: false,
})
export class ClientsComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: ClientsGridComponent,
      gridTabLabel: 'clientmanagement.clients.clients',
    },
  ];
}
