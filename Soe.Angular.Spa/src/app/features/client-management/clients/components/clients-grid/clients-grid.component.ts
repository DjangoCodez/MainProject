import { Component, inject, OnInit, signal } from '@angular/core';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { take } from 'rxjs';
import { ClientsService } from '../../services/clients.service';
import { ClientGridDTO } from '../../models/clients.model';
import {
  AddClientModalComponent,
  AddClientModalData,
} from '../add-client-modal/add-client-modal.component';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { AddClientForm } from '../../models/add-client.form';
import { ValidationHandler } from '@shared/handlers';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';

@Component({
  selector: 'soe-cm-clients-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  imports: [GridWrapperComponent],
  standalone: true,
})
export class ClientsGridComponent
  extends GridBaseDirective<ClientGridDTO, ClientsService>
  implements OnInit
{
  readonly service = inject(ClientsService);
  private readonly validationHandler = inject(ValidationHandler);
  private readonly dialogService = inject(DialogService);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.ClientManagement_Clients,
      'ClientManagement.Clients'
    );
  }

  override onFinished(): void {
    this.createPageToolbar();
  }

  private createPageToolbar(): void {
    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('addClient', {
          caption: signal('clientmanagement.clients.add'),
          tooltip: signal('clientmanagement.clients.add'),
          iconName: signal('plus'),
          onAction: () => this.openAddClientModal(),
        }),
      ],
    });
  }

  private openAddClientModal(): void {
    // Create a new connection request
    this.service.createConnectionRequest().subscribe(connectionRequest => {
      if (!connectionRequest) {
        console.error('Failed to create connection request, check permission!');
        return;
      }

      const dialogData: AddClientModalData = {
        title: 'clientmanagement.clients.add.title',
        size: 'md',
        connectionRequest: new AddClientForm({
          validationHandler: this.validationHandler,
          element: connectionRequest,
        }),
      };

      this.dialogService
        .open(AddClientModalComponent, dialogData)
        .afterClosed()
        .subscribe((result: boolean) => {
          if (result) {
            this.refreshGrid();
          }
        });
    });
  }

  override onGridReadyToDefine(grid: GridComponent<ClientGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'clientmanagement.clients.name',
        'clientmanagement.clients.licensenumber',
        'clientmanagement.clients.licensename',
        'core.created',
        'core.createdby',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText(
          'tcName',
          terms['clientmanagement.clients.name'],
          {
            flex: 2,
          }
        );
        this.grid.addColumnText(
          'tcLicenseNr',
          terms['clientmanagement.clients.licensenumber'],
          {
            flex: 2,
          }
        );
        this.grid.addColumnText(
          'tcLicenseName',
          terms['clientmanagement.clients.licensename'],
          {
            flex: 2,
          }
        );
        this.grid.addColumnDate('created', terms['core.created'], {
          flex: 1,
        });
        this.grid.addColumnText('createdBy', terms['core.createdby'], {
          flex: 1,
        });

        super.finalizeInitGrid();
      });
  }
}
